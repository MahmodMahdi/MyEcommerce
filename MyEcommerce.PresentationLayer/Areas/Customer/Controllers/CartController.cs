using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.Models;
using MyEcommerce.DomainLayer.Models.Order;
using MyEcommerce.DomainLayer.ViewModels;
using Stripe.Checkout;
using System.Security.Claims;
using Utilities;

namespace MyEcommerce.PresentationLayer.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class CartController : Controller
	{
		private readonly IUnitOfWork _UnitOfWork;
		public int TotalCart { get; set; }
		public CartController(IUnitOfWork unitOfWork)
		{
			_UnitOfWork = unitOfWork;
		}
		[Authorize]
		public async Task<IActionResult> Index()
		{
			// here i need user who login
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var shoppingCartViewModel = new ShoppingCartViewModel()
			{
				Carts = await _UnitOfWork.ShoppingCartRepository.GetAllAsync(u => u.ApplicationUserId == userId, IncludeProperties: "Product"),
				OrderHeader = new()

			};
			HttpContext.Session.SetInt32(Helper.SessionKey, shoppingCartViewModel.Carts.Count());
			// here i want to sum the carts which user order
			shoppingCartViewModel.TotalCarts = shoppingCartViewModel.Carts.Sum(c => c.Count * c.Product.Price);

			return View(shoppingCartViewModel);
		}
		[HttpGet]
		public async Task<IActionResult> Summary()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var shoppingCartViewModel = new ShoppingCartViewModel()
			{
				Carts = await _UnitOfWork.ShoppingCartRepository.GetAllAsync(u => u.ApplicationUserId == userId, IncludeProperties: "Product"),
				OrderHeader = new()

			};
			// --- منطق التطهير الذكي ---
			bool isChanged = false;
			foreach (var cart in shoppingCartViewModel.Carts.ToList()) // ToList عشان م يحصلش Error وقت الحذف
			{
				// 1. لو المنتج خلص تماماً والمستخدم لسه عنده منه في السلة
				if (cart.Product.StockQuantity <= 0)
				{
					await _UnitOfWork.ShoppingCartRepository.RemoveAsync(cart);
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
				await _UnitOfWork.CompleteAsync();
				var actualCartCount = (await _UnitOfWork.ShoppingCartRepository.GetAllAsync(u => u.ApplicationUserId == userId)).Count();
				HttpContext.Session.SetInt32(Helper.SessionKey, actualCartCount);
				TempData["Error"] = "Some items in your cart were updated or removed due to limited stock.";
				// نعيد جلب البيانات بعد التحديث عشان الحسابات تطلع صح
				shoppingCartViewModel.Carts = await _UnitOfWork.ShoppingCartRepository.GetAllAsync(u => u.ApplicationUserId == userId, IncludeProperties: "Product");
			}
			// --- نهاية منطق التطهير ---
			// this data that appear when opening the page (Customer info)
			shoppingCartViewModel.OrderHeader.ApplicationUser = await _UnitOfWork.ApplicationUserRepository.GetByIdAsync(x => x.Id == userId);
			shoppingCartViewModel.OrderHeader.Name = shoppingCartViewModel.OrderHeader.ApplicationUser.Name;
			shoppingCartViewModel.OrderHeader.Address = shoppingCartViewModel.OrderHeader.ApplicationUser.Address;
			shoppingCartViewModel.OrderHeader.City = shoppingCartViewModel.OrderHeader.ApplicationUser.City;
			shoppingCartViewModel.OrderHeader.PhoneNumber = shoppingCartViewModel.OrderHeader.ApplicationUser.PhoneNumber;

			shoppingCartViewModel.OrderHeader.TotalPrice = shoppingCartViewModel.Carts.Sum(c => c.Count * c.Product.Price);

			return View(shoppingCartViewModel);

		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> PostSummary(ShoppingCartViewModel shoppingCartViewModel)
		{
			try
			{
				// get the user 
				var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

				// get the carts of that user assigned
				shoppingCartViewModel.Carts = await _UnitOfWork.ShoppingCartRepository.GetAllAsync(u => u.ApplicationUserId == userId, IncludeProperties: "Product");
				// التحقق النهائي قبل الدفع
				foreach (var item in shoppingCartViewModel.Carts)
				{
					if (item.Count > item.Product.StockQuantity)
					{
						TempData["Error"] = $"The item '{item.Product.Name}' is no longer available in this quantity. Only {item.Product.StockQuantity} left.";
						return RedirectToAction(nameof(Index));
					}
				}
				#region Order Header Info 
				// set a status data info for user
				shoppingCartViewModel.OrderHeader.OrderStatus = Helper.Pending;
				shoppingCartViewModel.OrderHeader.PaymentStatus = Helper.Pending;
				shoppingCartViewModel.OrderHeader.OrderDate = DateTime.Now;
				shoppingCartViewModel.OrderHeader.ApplicationUserId = userId;

				// sum count of cart total price
				shoppingCartViewModel.OrderHeader.TotalPrice = shoppingCartViewModel.Carts.Sum(c => c.Count * c.Product.Price);

				// add this data in OrderHeader in DB
				await _UnitOfWork.OrderHeaderRepository.AddAsync(shoppingCartViewModel.OrderHeader);
				await _UnitOfWork.CompleteAsync();
				#endregion
				#region Order Detail Info
				// here it will loop in Carts and get the details of order and set it into Order Detail Model
				foreach (var cart in shoppingCartViewModel.Carts)
				{
					var OrderDetail = new OrderDetail()
					{
						ProductId = cart.ProductId,
						OrderId = shoppingCartViewModel.OrderHeader.Id,
						Price = cart.Product.Price,
						Count = cart.Count
					};
					// add this data in OrderDetail in DB
					await _UnitOfWork.OrderDetailRepository.AddAsync(OrderDetail);
				}
				await _UnitOfWork.CompleteAsync();
				#endregion
				// here code of stripe method (from stripe site) but i set my value 
				return await ProcessStripePayment(shoppingCartViewModel);

			}
			catch (Exception ex)
			{
				// أحياناً Stripe يرمي خطأ من نوع HttpRequestException مباشرة
				TempData["Error"] = "Connection Failed: Please check your internet and try again.";

				// سطر للـ Debugging فقط ليظهر لك في الـ Output Window ما هو الخطأ الحقيقي
				System.Diagnostics.Debug.WriteLine($"Error Type: {ex.GetType().Name}, Message: {ex.Message}");
				return RedirectToAction(nameof(Index));
			}
		}
		public async Task<IActionResult> OrderConfirmation(int id)
		{
			try
			{
				var OrderHeader = await _UnitOfWork.OrderHeaderRepository.GetByIdAsync(u => u.Id == id);
				// check my session in stripe
				var service = new SessionService();
				var session = await service.GetAsync(OrderHeader.SessionId);
				if (session.PaymentStatus.ToLower() == "paid")
				{
					await _UnitOfWork.OrderHeaderRepository.UpdateOrderStatusAsync(id, Helper.Approve, Helper.Approve);
					// when order done it will fill the paymentIntentId of Db with PII with stripe
					OrderHeader.PaymentIntentId = session.PaymentIntentId;
					// --- منطق خصم المخزون (Clean & Efficient) ---
					var orderDetails = await _UnitOfWork.OrderDetailRepository.GetAllAsync(x => x.OrderId == id);

					foreach (var item in orderDetails)
					{
						var product = await _UnitOfWork.ProductRepository.GetByIdAsync(p => p.Id == item.ProductId);
						if (product != null)
						{
							product.StockQuantity -= item.Count; // خصم الكمية المباعة من المخزن
						}
					}
					await _UnitOfWork.CompleteAsync();

					// if status is paid then i need to remove items from cart 
					var shoppingCart = await _UnitOfWork.ShoppingCartRepository.GetAllAsync(u => u.ApplicationUserId == OrderHeader.ApplicationUserId);
					await _UnitOfWork.ShoppingCartRepository.RemoveRangeAsync(shoppingCart);
					await _UnitOfWork.CompleteAsync();
					HttpContext.Session.SetInt32(Helper.SessionKey, 0);
				}
				return View(id);
			}
			catch (Exception)
			{
				// في حال فشل النت هنا، نوجه المستخدم لصفحة تخبره بأننا سنؤكد طلبه فور عودة الخدمة
				TempData["Error"] = "Connection lost while confirming payment. Don't worry, your order is being processed.";
				return RedirectToAction("Index", "Home");
			}
		}
		public async Task<IActionResult> Plus(int CartId)
		{
			var ShoppingCart = await _UnitOfWork.ShoppingCartRepository.GetByIdAsync(c => c.Id == CartId,IncludeProperties:"Product");
			if (ShoppingCart == null) return NotFound();
			if (ShoppingCart.Count >= ShoppingCart.Product.StockQuantity)
			{
				TempData["Error"] = $"Sorry, only {ShoppingCart.Product.StockQuantity} items available in stock.";
			}
			else
			{
				_UnitOfWork.ShoppingCartRepository.IncreaseCount(ShoppingCart, 1);
				await _UnitOfWork.CompleteAsync();
			}
				return RedirectToAction(nameof(Index));
		}
		public async Task<IActionResult> Minus(int CartId)
		{
			var ShoppingCart = await _UnitOfWork.ShoppingCartRepository.GetByIdAsync(c => c.Id == CartId);
			if (ShoppingCart.Count <= 1)
			{
				await RemoveFromCart(ShoppingCart);
				return RedirectToAction(nameof(Index));
			}
			else
			{
				_UnitOfWork.ShoppingCartRepository.DecreaseCount(ShoppingCart, 1);
			}
			await _UnitOfWork.CompleteAsync();
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Remove(int cartId)
		{
			var ShoppingCart = await _UnitOfWork.ShoppingCartRepository.GetByIdAsync(c => c.Id == cartId);
			await RemoveFromCart(ShoppingCart);
			return RedirectToAction(nameof(Index));
		}
		private async Task RemoveFromCart(ShoppingCart cart)
		{
			await _UnitOfWork.ShoppingCartRepository.RemoveAsync(cart);
			await _UnitOfWork.CompleteAsync();

			var count = (await _UnitOfWork.ShoppingCartRepository.GetAllAsync(s => s.ApplicationUserId == cart.ApplicationUserId)).Count();
			HttpContext.Session.SetInt32(Helper.SessionKey, count);
		}
		private async Task<IActionResult> ProcessStripePayment(ShoppingCartViewModel shoppingCartViewModel)
		{
			#region Stripe code
			var domain = "https://localhost:7148/"; // manual
			var options = new SessionCreateOptions
			{
				LineItems = new List<SessionLineItemOptions>(),
				// manual
				Mode = "payment",
				SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={shoppingCartViewModel.OrderHeader.Id}",
				CancelUrl = domain + $"Customer/Cart/Index",
			};
			// manual to set my data
			foreach (var item in shoppingCartViewModel.Carts)
			{
				var sessionLineOption = new SessionLineItemOptions
				{
					PriceData = new SessionLineItemPriceDataOptions
					{
						UnitAmount = (long)(item.Product.Price * 100), // القيمة هنا عشرية وانا عايز رقم ثابت 
						Currency = "egp",
						ProductData = new SessionLineItemPriceDataProductDataOptions
						{
							Name = item.Product.Name,
						},
					},
					Quantity = item.Count,
				};
				options.LineItems.Add(sessionLineOption);
			}

			var service = new SessionService();
			Session session = await service.CreateAsync(options);
			// check of session to confirm (success of not)
			shoppingCartViewModel.OrderHeader.SessionId = session.Id;
			await _UnitOfWork.CompleteAsync();
			Response.Headers.Append("Location", session.Url);
			#endregion

			return new StatusCodeResult(303);

		}
	}
}
