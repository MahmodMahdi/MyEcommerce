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
			var orderFromDb = await _unitOfWork.OrderHeaderRepository.GetFirstOrDefaultAsync(x => x.Id == orderViewModel.OrderId, IncludeProperties: "ApplicationUser");
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
				try
				{
					var sendingEmailDetails = new OrderEmailDto()
					{
						Email = orderFromDb.ApplicationUser.Email,
						Name = orderFromDb.ApplicationUser.Name,
						OrderId = orderFromDb.Id,
						TrackingNumber = orderFromDb.TrackingNumber,
						Carrier = orderFromDb.Carrior,
					};
					await _emailService.SendShippingEmail(sendingEmailDetails);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "[EMAIL ERROR] Failed to send shipping update email for Order #{OrderId} to User {Email}",
							orderFromDb.Id, orderFromDb.ApplicationUser.Email);
				}
			}
			return true;
		}
		public async Task<bool> CancelOrderAsync(OrderViewModel orderViewModel)
		{
			//bring order from db
			var orderFromDB = await _unitOfWork.OrderHeaderRepository.GetFirstOrDefaultAsync(o => o.Id == orderViewModel.OrderHeader.Id);
			if (orderFromDB == null) return false;
			if (orderFromDB.PaymentStatus == Helper.Approve)
			{
				if (!string.IsNullOrEmpty(orderFromDB.PaymentIntentId))
				{
					try
					{
						var option = new RefundCreateOptions
						{
							Reason = RefundReasons.RequestedByCustomer,
							PaymentIntent = orderFromDB.PaymentIntentId
						};
						var service = new RefundService();
						Refund refund = service.Create(option); 

						_unitOfWork.OrderHeaderRepository.UpdateOrderStatus(orderFromDB.Id, Helper.Cancelled, Helper.Refund);
					}
					catch (StripeException ex)
					{
						_logger.LogError(ex, "[STRIPE ERROR] Refund failed for Order #{OrderId}. Check PaymentIntentId: {PaymentIntentId}",
								orderFromDB.Id, orderFromDB.PaymentIntentId);
						return false;
					}
				}
				else
				{
					_unitOfWork.OrderHeaderRepository.UpdateOrderStatus(orderFromDB.Id, Helper.Cancelled, Helper.Cancelled);
				}

			}
			else
			{
				_unitOfWork.OrderHeaderRepository.UpdateOrderStatus(orderFromDB.Id, Helper.Cancelled, Helper.Cancelled);
			}
			await _unitOfWork.CompleteAsync();
			return true;
		}

		public async Task<bool> StartProccessing(OrderViewModel orderViewModel)
		{
			var orderFromDb = await _unitOfWork.OrderHeaderRepository.GetFirstOrDefaultAsync(o => o.Id == orderViewModel.OrderHeader.Id,IncludeProperties:"ApplicationUser",tracked:false);
			if (orderFromDb == null) return false;
			_unitOfWork.OrderHeaderRepository.UpdateOrderStatus(orderViewModel.OrderHeader.Id, Helper.Proccessing, null);
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
					Email = orderFromDb.ApplicationUser.Email,
					Name = orderFromDb.ApplicationUser.Name,
					OrderId = orderFromDb.Id,
					TrackingNumber = orderViewModel.OrderHeader.TrackingNumber,
					Carrier = orderFromDb.Carrior
				};
				await _emailService.SendShippingEmail(SendingEmaildetails);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "[EMAIL ERROR] Failed to send shipping confirmation email for Order #{OrderId}", orderFromDb.Id); Console.WriteLine($"Sending email for shipping proccessing failed: {ex.Message}");
			}
			return true;
		}
	}
}
