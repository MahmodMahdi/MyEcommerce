using Microsoft.Extensions.Logging;
using MyEcommerce.ApplicationLayer.Interfaces.Services;
using MyEcommerce.ApplicationLayer.ViewModels;
using MyEcommerce.DomainLayer.Interfaces.Repositories;
using MyEcommerce.DomainLayer.Models.Order;
using Stripe;
using Utilities;

namespace MyEcommerce.ApplicationLayer.Services
{
	public class OrderServices : IOrderServices
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmailService _emailService;
		private readonly ILogger<OrderServices> _logger;
		public OrderServices(IUnitOfWork unitOfWork,
			IEmailService emailService,
			ILogger<OrderServices> logger)
		{
			_unitOfWork = unitOfWork;
			_emailService = emailService;
			_logger = logger;
		}
		public async Task<IEnumerable<OrderHeader>> GetAllAsync()
		{
			var OrderHeaders = await _unitOfWork.OrderHeaderRepository.GetAllAsync(IncludeProperties: "ApplicationUser");
			return OrderHeaders;
		}
		public async Task<OrderViewModel> GetOrderViewModelAsync(int OrderId)
		{
			return new OrderViewModel()
			{
				OrderHeader = await _unitOfWork.OrderHeaderRepository.GetFirstOrDefaultAsync(x => x.Id == OrderId, IncludeProperties: "ApplicationUser"),
				OrderDetails = await _unitOfWork.OrderDetailRepository.GetAllAsync(x => x.OrderId == OrderId, IncludeProperties: "Product")
			};
		}
		public async Task<bool> UpdateOrderDetialsAsync(UpdateOrderDto orderViewModel)
		{
			var orderFromDb = await _unitOfWork.OrderHeaderRepository.GetFirstOrDefaultAsync(x => x.Id == orderViewModel.OrderId);
			if (orderFromDb == null) return false;
			var oldTrackingNumber = orderFromDb.TrackingNumber;
			var oldCarrior = orderFromDb.Carrior;
			// هنا انا محتاج اجيب ال tracking number علشان لو غيرته يبعت رسالة لليوزر بايميل
			orderFromDb.Name = orderViewModel.Name;
			orderFromDb.PhoneNumber = orderViewModel.PhoneNumber;
			orderFromDb.Address = orderViewModel.Address;
			orderFromDb.City = orderViewModel.City;
			orderFromDb.Carrior = orderViewModel.Carrior;
			orderFromDb.TrackingNumber = orderViewModel.TrackingNumber;

			_unitOfWork.OrderHeaderRepository.Update(orderFromDb);
			await _unitOfWork.CompleteAsync();
			bool hasShippingData = !string.IsNullOrEmpty(orderFromDb.TrackingNumber)
					|| !string.IsNullOrEmpty(orderFromDb.Carrior);
			bool isChanged = orderFromDb.TrackingNumber != oldTrackingNumber || orderFromDb.Carrior != oldCarrior;
			if (hasShippingData && isChanged)
			{
				var user = await _unitOfWork.ApplicationUserRepository.GetFirstOrDefaultAsync(x => x.Id == orderFromDb.ApplicationUserId, tracked: false);

				try
				{
					if (user != null)
					{
						var sendingEmailDetails = new OrderEmailDto()
						{
							Email = user.Email,
							Name = user.Name,
							OrderId = orderFromDb.Id,
							TrackingNumber = orderFromDb.TrackingNumber,
							Carrier = orderFromDb.Carrior,
						};
						await _emailService.SendShippingEmailAsync(sendingEmailDetails);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "[EMAIL ERROR] Failed to send shipping update email for Order #{OrderId} to User {Email}",
							orderFromDb.Id, user.Email);
				}
			}
			return true;
		}
		public async Task<bool> CancelOrderAsync(OrderViewModel orderViewModel)
		{
			// 1️- جلب الطلب من قاعدة البيانات مع التفاصيل والمنتجات
			var orderFromDB = await _unitOfWork.OrderHeaderRepository
				.GetFirstOrDefaultAsync(o => o.Id == orderViewModel.OrderHeader.Id);

			if (orderFromDB == null) return false;

			// 2️- جلب تفاصيل الطلب لإعادة الكميات للمخزن
			var orderDetails = await _unitOfWork.OrderDetailRepository
				.GetAllAsync(x => x.OrderId == orderFromDB.Id, IncludeProperties: "Product");

			await _unitOfWork.BeginTransactionAsync();
			try
			{
				// 3️- منطق الاسترجاع المالي (Stripe Refund)
				if (orderFromDB.PaymentStatus == Helper.Approve && !string.IsNullOrEmpty(orderFromDB.PaymentIntentId))
				{
					var options = new RefundCreateOptions
					{
						Reason = RefundReasons.RequestedByCustomer,
						PaymentIntent = orderFromDB.PaymentIntentId
					};
					var service = new RefundService();
					await service.CreateAsync(options);

					await _unitOfWork.OrderHeaderRepository.UpdateOrderStatusAsync(orderFromDB.Id, Helper.Cancelled, Helper.Refund);
				}
				else
				{
					await _unitOfWork.OrderHeaderRepository.UpdateOrderStatusAsync(orderFromDB.Id, Helper.Cancelled, Helper.Cancelled);
				}

				// 4️- أهم خطوة: إعادة المنتجات للمخزن
				// بنعملها فقط لو الطلب كان Approved 
				if (orderFromDB.OrderStatus == Helper.Approve)
				{
					foreach (var item in orderDetails)
					{
						if (item.Product != null)
						{
							item.Product.StockQuantity += item.Count; // زيادة المخزن مرة تانية
						}
					}
				}

				// 5️- تحديث حالة الطلب النهائية
				orderFromDB.OrderStatus = Helper.Cancelled;

				await _unitOfWork.CompleteAsync();
				await _unitOfWork.CommitTransactionAsync();
				return true;
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollbackTransactionAsync();
				_logger.LogError(ex, "[CANCEL ERROR] Failed to cancel order #{OrderId}", orderFromDB.Id);
				return false;
			}
		}
		public async Task<bool> StartProccessing(OrderViewModel orderViewModel)
		{
			await _unitOfWork.OrderHeaderRepository.UpdateOrderStatusAsync(orderViewModel.OrderHeader.Id, Helper.Proccessing, null);
			await _unitOfWork.CompleteAsync();
			return true;
		}
		public async Task<bool> StartShipping(OrderViewModel orderViewModel)
		{
			//bring order from db
			var orderFromDb = await _unitOfWork.OrderHeaderRepository.GetFirstOrDefaultAsync(o => o.Id == orderViewModel.OrderHeader.Id, IncludeProperties: "ApplicationUser");
			if (orderFromDb == null) return false;
			// update data of order like when shipping process ( TrackingNumber to follow the order,Carrior and status and Date of Shipping )
			orderFromDb.TrackingNumber = orderViewModel.OrderHeader.TrackingNumber;
			orderFromDb.Carrior = orderViewModel.OrderHeader.Carrior;
			orderFromDb.OrderStatus = Helper.Shipped;
			orderFromDb.ShippingDate = DateTime.Now;
			_unitOfWork.OrderHeaderRepository.Update(orderFromDb);
			await _unitOfWork.CompleteAsync();
			try
			{
				var SendingEmaildetails = new OrderEmailDto()
				{
					Email = orderFromDb.ApplicationUser?.Email?? "No Email",
					Name = orderFromDb.ApplicationUser.Name,
					OrderId = orderFromDb.Id,
					TrackingNumber = orderViewModel.OrderHeader.TrackingNumber,
					Carrier = orderFromDb.Carrior
				};
				await _emailService.SendShippingEmailAsync(SendingEmaildetails);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[EMAIL ERROR] Failed to send shipping confirmation email for Order #{OrderId}", orderFromDb.Id); Console.WriteLine($"Sending email for shipping proccessing failed: {ex.Message}");
			}
			return true;
		}
	}
}
