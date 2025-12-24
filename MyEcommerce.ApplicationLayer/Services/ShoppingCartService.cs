using Microsoft.Extensions.Logging;
using MyEcommerce.ApplicationLayer.Interfaces.Services;
using MyEcommerce.ApplicationLayer.ViewModels;
using MyEcommerce.DomainLayer.Interfaces.Repositories;
using MyEcommerce.DomainLayer.Models;
using MyEcommerce.DomainLayer.Models.Order;
using Stripe.Checkout;

using Utilities;

namespace MyEcommerce.ApplicationLayer.Services
{
	public class ShoppingCartService : IShoppingCartService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPaymentService _paymentService;
		private readonly IEmailService _emailService;
		private readonly ILogger<ShoppingCartService> _logger;
		public ShoppingCartService(IUnitOfWork unitOfWork,
			IPaymentService paymentService,
			IEmailService emailService,
			ILogger<ShoppingCartService> logger)
		{
			_unitOfWork = unitOfWork;
			_paymentService = paymentService;
			_emailService = emailService;
			_logger = logger;
		}
		public async Task<ShoppingCartViewModel> GetAllAsync(string userId)
		{
			var Carts = await _unitOfWork.ShoppingCartRepository.GetAllAsync(u => u.ApplicationUserId == userId, IncludeProperties: "Product");

			var shoppingCartViewModel = new ShoppingCartViewModel()
			{
				Carts = Carts,
				OrderHeader = new OrderHeader(),
				TotalCarts = Carts.Sum(c => c.Count * c.Product.Price)
			};
			return shoppingCartViewModel;
		}

		public async Task<ShoppingCartViewModel> GetSummaryAsync(string userId)
		{
			var Carts = (await _unitOfWork.ShoppingCartRepository.GetAllAsync(u => u.ApplicationUserId == userId, IncludeProperties: "Product")).ToList();

			var itemsToRemove = new List<ShoppingCart>();
			bool isChanged = false;
			foreach (var cart in Carts) //  عشان م يحصلش Error وقت الحذف
			{
				// 1. لو المنتج خلص تماماً والمستخدم لسه عنده منه في السلة
				if (cart.Product.StockQuantity <= 0)
				{
					itemsToRemove.Add(cart);
					isChanged = true;
				}
				// 2. لو المخزن قل وبقى أقل من اللي اليوزر حاطه في السلة
				else if (cart.Count > cart.Product.StockQuantity)
				{
					cart.Count = cart.Product.StockQuantity; // بنعدل الكمية لتناسب المتاح حالياً
					isChanged = true;
				}
			}

			if (isChanged)
			{
				if (itemsToRemove.Any())
				{
					_unitOfWork.ShoppingCartRepository.RemoveRange(itemsToRemove);
					foreach (var item in itemsToRemove)
					{
						_logger.LogWarning("Inventory Alert: Product {ProductId} removed from User {UserId} cart (Out of Stock).", item.ProductId, userId);
						Carts.Remove(item);
					}
				}
				await _unitOfWork.CompleteAsync();

			}
			// --- نهاية منطق التطهير ---
			// this data that appear when opening the page (Customer info)
			var user = await _unitOfWork.ApplicationUserRepository.GetFirstOrDefaultAsync(u => u.Id == userId);
			var shoppingCartViewModel = new ShoppingCartViewModel()
			{
				Carts = Carts,
				OrderHeader = new OrderHeader()
				{
					ApplicationUser = user,
					Name = user?.Name,
					Address = user?.Address,
					City = user?.City,
					PhoneNumber = user?.PhoneNumber,
					TotalPrice = Carts.Sum(c => c.Count * c.Product.Price)
				}

			};
			return shoppingCartViewModel;
		}
		public async Task<ShoppingCartViewModel> CreateOrderAsync(ShoppingCartViewModel shoppingCartViewModel, string userId, string domain)
		{
			// get the carts of that user assigned
			var Carts = (await _unitOfWork.ShoppingCartRepository.GetAllAsync(u => u.ApplicationUserId == userId, IncludeProperties: "Product")).ToList();
			// التحقق النهائي قبل الدفع
			foreach (var item in Carts)
			{
				if (item.Count > item.Product.StockQuantity)
				{
					throw new InvalidOperationException($"The item '{item.Product.Name}' is out of stock.");
				}
			}
			#region Order Header Info 
			// set a status data info for user
			shoppingCartViewModel.OrderHeader.OrderStatus = Helper.Pending;
			shoppingCartViewModel.OrderHeader.PaymentStatus = Helper.Pending;
			shoppingCartViewModel.OrderHeader.OrderDate = DateTime.Now;
			shoppingCartViewModel.OrderHeader.ApplicationUserId = userId;

			// sum count of cart total price
			shoppingCartViewModel.OrderHeader.TotalPrice = Carts.Sum(c => c.Count * c.Product.Price);

			// add this data in OrderHeader in DB
			await _unitOfWork.OrderHeaderRepository.AddAsync(shoppingCartViewModel.OrderHeader);
			await _unitOfWork.CompleteAsync();
			#endregion
			#region Order Detail Info
			// here it will loop in Carts and get the details of order and set it into Order Detail Model
			var orderDetailsList = Carts.Select(Carts => new OrderDetail
			{
				ProductId = Carts.ProductId,
				OrderId = shoppingCartViewModel.OrderHeader.Id,
				Price = Carts.Product.Price,
				Count = Carts.Count
			}).ToList();

			await _unitOfWork.OrderDetailRepository.AddRangeAsync(orderDetailsList);
			await _unitOfWork.CompleteAsync();
			#endregion
			shoppingCartViewModel.Carts = Carts;
			// here code of stripe method (from stripe site) but i set my value 
			var PaymentResult = await _paymentService.CreateStripeSessionAsync(shoppingCartViewModel, domain);
			// لازم تخزن الـ SessionId عشان تقدر تعمل Confirmation بعدين
			shoppingCartViewModel.OrderHeader.SessionId = PaymentResult.SessionId;
			// ونخزن الـ URL في الـ ViewModel عشان الـ Controller يوجه اليوزر
			shoppingCartViewModel.Url = PaymentResult.Url;
			await _unitOfWork.CompleteAsync();
			return shoppingCartViewModel;
		}
		public async Task<bool> OrderConfirmation(int orderId)
		{
			var OrderHeader = await _unitOfWork.OrderHeaderRepository.GetFirstOrDefaultAsync(u => u.Id == orderId, IncludeProperties: "ApplicationUser");
			if (OrderHeader == null) return false;
			// check my session in stripe
			var service = new SessionService();
			var session = await service.GetAsync(OrderHeader.SessionId);
			if (session.PaymentStatus.ToLower() == "paid")
			{
				await _unitOfWork.BeginTransactionAsync();
				try
				{
					// when order done it will fill the paymentIntentId of Db with PII with stripe
					OrderHeader.PaymentIntentId = session.PaymentIntentId;
					await _unitOfWork.OrderHeaderRepository.UpdateOrderStatusAsync(orderId, Helper.Approve, Helper.Approve);
					// --- منطق خصم المخزون 
					var orderDetails = await _unitOfWork.OrderDetailRepository.GetAllAsync(x => x.OrderId == orderId, IncludeProperties: "Product");
					foreach (var item in orderDetails)
					{
						if (item.Product != null)
						{
							item.Product.StockQuantity -= item.Count; // خصم الكمية المباعة من المخزن
						}
					}
					// مسح السلة
					var shoppingCart = await _unitOfWork.ShoppingCartRepository.GetAllAsync(u => u.ApplicationUserId == OrderHeader.ApplicationUserId);
					_unitOfWork.ShoppingCartRepository.RemoveRange(shoppingCart);
					await _unitOfWork.CompleteAsync();
					await _unitOfWork.CommitTransactionAsync();
					try
					{
						var SendingEmaildetails = new OrderEmailDto()
						{
							Email = OrderHeader.ApplicationUser.Email,
							Name = OrderHeader.ApplicationUser.Name,
							OrderId = orderId,
							Total = OrderHeader.TotalPrice
						};
						await _emailService.SendOrderConfirmationEmail(SendingEmaildetails);
					}
					catch(Exception ex) 
					{
						_logger.LogError(ex, "[EMAIL ERROR] Order confirmation email failed for Order #{OrderId}", orderId);
					}
					return true;

				}
				catch (Exception ex)
				{
					await _unitOfWork.RollbackTransactionAsync();
					_logger.LogError(ex, "[FATAL ERROR] Transaction failed for Order #{OrderId}. Changes rolled back.", orderId); return false;
				}
			}
			return false;
		}
		public async Task<int> IncrementCountAsync(int CartId)
		{
			var shoppingCart = await _unitOfWork.ShoppingCartRepository.GetFirstOrDefaultAsync(c => c.Id == CartId, IncludeProperties: "Product");
			if (shoppingCart == null) return 0;
			if (shoppingCart.Count < shoppingCart.Product.StockQuantity)
			{
				_unitOfWork.ShoppingCartRepository.IncreaseCount(shoppingCart, 1);
				await _unitOfWork.CompleteAsync();
			}
			else
			{
				_logger.LogWarning("User {UserId} tried to exceed stock for Product {ProductId}. Available: {StockQuantity}",
		         shoppingCart.ApplicationUserId, shoppingCart.ProductId, shoppingCart.Product.StockQuantity);
				throw new InvalidOperationException("you can't add any more, you reach the available max in inventory ");
			}
			return await _unitOfWork.ShoppingCartRepository.CountAsync(u => u.ApplicationUserId == shoppingCart.ApplicationUserId);
		}
		public async Task<int> DecrementCountAsync(int CartId)
		{
			var shoppingCart = await _unitOfWork.ShoppingCartRepository.GetFirstOrDefaultAsync(c => c.Id == CartId);
			if (shoppingCart == null) return 0;
			var userId = shoppingCart.ApplicationUserId;
			if (shoppingCart.Count <= 1)
			{
				await RemoveFromCart(shoppingCart);
			}
			else
			{
				_unitOfWork.ShoppingCartRepository.DecreaseCount(shoppingCart, 1);
				await _unitOfWork.CompleteAsync();
			}
			return await _unitOfWork.ShoppingCartRepository.CountAsync(u => u.ApplicationUserId == userId);
		}
		public async Task<int> DeleteAsync(int id)
		{
			var shoppingCart = await _unitOfWork.ShoppingCartRepository.GetFirstOrDefaultAsync(c => c.Id == id);
			if (shoppingCart == null)
			{
				return 0;
			}
			return await RemoveFromCart(shoppingCart);
		}
		public async Task<int> RemoveFromCart(ShoppingCart cart)
		{
			var userId = cart.ApplicationUserId;
			_unitOfWork.ShoppingCartRepository.Remove(cart);
			await _unitOfWork.CompleteAsync();
			return await _unitOfWork.ShoppingCartRepository.CountAsync(u => u.ApplicationUserId == userId);
		}
	}
}
