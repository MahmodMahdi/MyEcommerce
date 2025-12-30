using Microsoft.Extensions.Logging;
using MyEcommerce.ApplicationLayer.Interfaces.Services;
using MyEcommerce.ApplicationLayer.ViewModels;
using MyEcommerce.DomainLayer.Interfaces.Repositories;
using MyEcommerce.DomainLayer.Models;
using MyEcommerce.DomainLayer.Models.Order;

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
				TotalCarts = Carts.Sum(c => c.Count * c.Product.AcualPrice)
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
					TotalPrice = Carts.Sum(c => c.Count * c.Product.AcualPrice)
				}

			};
			return shoppingCartViewModel;
		}

		public async Task<ShoppingCartViewModel> CreateOrderAsync(ShoppingCartViewModel shoppingCartViewModel, string userId, string domain)
		{
			await _unitOfWork.BeginTransactionAsync();
			try
			{
				// check the carts 
				var carts = await _unitOfWork.ShoppingCartRepository
				 .GetAllAsync(u => u.ApplicationUserId == userId, IncludeProperties: "Product");
				shoppingCartViewModel.Carts = carts;
				if (!carts.Any())
				{
					throw new InvalidOperationException("Cart is empty");
				}
				// check if item exsit or become out of stock
				foreach (var item in carts)
					if (item.Count > item.Product.StockQuantity)
						throw new InvalidOperationException($"Product '{item.Product.Name}' is out of stock.");
				// get the existing order
				var existingOrder = await _unitOfWork.OrderHeaderRepository.GetFirstOrDefaultAsync(
					o => o.ApplicationUserId == userId && o.OrderStatus == Helper.Pending);
				OrderHeader orderHeader;
				List<OrderDetail> existingDetails = new();
				decimal currentTotal = carts.Sum(c => c.Count * c.Product.AcualPrice);
				// if there is already order taken before
				if (existingOrder != null)
				{
					// get details of this order
					existingDetails = (await _unitOfWork.OrderDetailRepository
						.GetAllAsync(d => d.OrderId == existingOrder.Id)).ToList();

					int existingCount = existingDetails?.Count ?? 0;
					int currentCount = carts?.Count() ?? 0;
					// ckeck of the cart details is changed or same order
					bool cartChanged =
						existingOrder.TotalPrice != currentTotal ||
						existingCount != currentCount ||
						(existingDetails.Any() && carts.Any() &&
						 existingDetails.OrderBy(d => d.ProductId).First().ProductId !=
						 carts.OrderBy(c => c.ProductId).First().ProductId);
					// if not chaged it deal with old and get the same Url and order
					if (!cartChanged)
					{
						orderHeader = existingOrder;
						// نستخدم SessionId الموجود فقط
						shoppingCartViewModel.Url = await _paymentService.GetStripeSessionUrlAsync(existingOrder.SessionId);
					}
					else
					{
						// if change it will remove old and set a new data and add details into db and create stripeSession
						existingOrder.TotalPrice = currentTotal;
						existingOrder.OrderDate = DateTime.UtcNow;

						_unitOfWork.OrderDetailRepository.RemoveRange(existingDetails);

						var newDetails = carts.Select(c => new OrderDetail
						{
							OrderId = existingOrder.Id,
							ProductId = c.ProductId,
							Price = c.Product.AcualPrice,
							Count = c.Count,
							Discount = c.Product.Discount
						}).ToList();

						await _unitOfWork.OrderDetailRepository.AddRangeAsync(newDetails);

						var paymentResult = await _paymentService.CreateStripeSessionAsync(new ShoppingCartViewModel
						{
							OrderHeader = existingOrder,
							Carts = carts
						}, domain);

						existingOrder.SessionId = paymentResult.SessionId;
						shoppingCartViewModel.Url = paymentResult.Url;
						orderHeader = existingOrder;
					}
				}
				else
				{
					// if this is first creation of order 
					orderHeader = new OrderHeader
					{
						ApplicationUserId = userId,
						OrderStatus = Helper.Pending,
						PaymentStatus = Helper.Pending,
						OrderDate = DateTime.UtcNow,
						TotalPrice = currentTotal,
						Name = shoppingCartViewModel.OrderHeader.Name,
						Address = shoppingCartViewModel.OrderHeader.Address,
						City = shoppingCartViewModel.OrderHeader.City,
						PhoneNumber = shoppingCartViewModel.OrderHeader.PhoneNumber
					};

					await _unitOfWork.OrderHeaderRepository.AddAsync(orderHeader);
					await _unitOfWork.CompleteAsync();

					var orderDetails = carts.Select(c => new OrderDetail
					{
						OrderId = orderHeader.Id,
						ProductId = c.ProductId,
						Price = c.Product.AcualPrice,
						Count = c.Count,
						Discount = c.Product.Discount
					}).ToList();

					await _unitOfWork.OrderDetailRepository.AddRangeAsync(orderDetails);

					var paymentResult = await _paymentService.CreateStripeSessionAsync(new ShoppingCartViewModel
					{
						OrderHeader = orderHeader,
						Carts = carts
					}, domain);

					orderHeader.SessionId = paymentResult.SessionId;
					shoppingCartViewModel.Url = paymentResult.Url;
				}

				await _unitOfWork.CompleteAsync();
				await _unitOfWork.CommitTransactionAsync();

				shoppingCartViewModel.OrderHeader = orderHeader;
				return shoppingCartViewModel;
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollbackTransactionAsync();
				_logger.LogError(ex, "Failed to create/update order for user {UserId}", userId);
				throw;
			}
		}

		public async Task<bool> OrderConfirmation(int orderId)
		{
			var orderHeader = await _unitOfWork.OrderHeaderRepository
				.GetFirstOrDefaultAsync(x => x.Id == orderId, IncludeProperties: "ApplicationUser");
			if (orderHeader == null) return false;

			if (orderHeader.OrderStatus != Helper.Pending)
			{
				_logger.LogInformation("Order {OrderId} is already approved. Skipping confirmation.", orderId);
				return true;
			}


			// 2️- التحقق من حالة الدفع في Stripe
			var session = await _paymentService.GetStripeSession(orderHeader.SessionId);
			if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
			{
				_logger.LogWarning("Payment not verified for Order {OrderId}. Stripe Status: {Status}", orderId, session.PaymentStatus);
				return false;

			}

			await _unitOfWork.BeginTransactionAsync();
			try
			{
				orderHeader.PaymentIntentId = session.PaymentIntentId;
				orderHeader.PaymentDate = DateTime.UtcNow;
				// 3- جلب تفاصيل الطلب
				var orderDetails = (await _unitOfWork.OrderDetailRepository
					.GetAllAsync(x => x.OrderId == orderId, IncludeProperties: "Product")).ToList();

				// 4- فحص المخزون والتعامل مع العجز
				if (!IsStockAvailable(orderDetails))
				{
					// Log Warning: لأن دي حالة استثنائية (دفع بدون مخزن)
					_logger.LogWarning("STOCK ISSUE: Order {OrderId} was paid but some items are out of stock. Setting status to {Status}",
						orderId, Helper.ApprovedStockIssue);
					await _unitOfWork.OrderHeaderRepository.UpdateOrderStatusAsync(orderId, Helper.ApprovedStockIssue, Helper.Approve);
				}
				else
				{
					
					foreach (var item in orderDetails)
					{
						item.Product.StockQuantity -= item.Count;
					}
					await _unitOfWork.OrderHeaderRepository.UpdateOrderStatusAsync(orderId, Helper.Approve, Helper.Approve);
				}
				await ClearCart(orderHeader.ApplicationUserId);
				await _unitOfWork.CompleteAsync();
				await _unitOfWork.CommitTransactionAsync();

				await SendConfirmationEmail(orderHeader);

				return true;
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollbackTransactionAsync();
				_logger.LogError(ex, "[CRITICAL] OrderConfirmation failed for OrderId={OrderId}", orderId);
				return false;
			}
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
		#region Private Helper Methods
		private bool IsStockAvailable(List<OrderDetail> details)
		{
			return details.All(item => item.Product != null && item.Product.StockQuantity >= item.Count);
		}
		private async Task ClearCart(string userId)
		{
			var carts = await _unitOfWork.ShoppingCartRepository
				.GetAllAsync(x => x.ApplicationUserId == userId);

			_unitOfWork.ShoppingCartRepository.RemoveRange(carts);
		}

		private async Task SendConfirmationEmail(OrderHeader order)
		{
			try
			{
				var OrderDto = new OrderEmailDto
				{
					Email = order.ApplicationUser.Email,
					Name = order.ApplicationUser.Name,
					OrderId = order.Id,
					Total = order.TotalPrice
				};
				await _emailService.SendOrderConfirmationEmailAsync(OrderDto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Email failed for Order {OrderId}", order.Id);
			}
		}
		#endregion
	}
}
