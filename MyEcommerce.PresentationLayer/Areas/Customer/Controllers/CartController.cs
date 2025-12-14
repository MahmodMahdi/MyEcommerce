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
		public ShoppingCartViewModel shoppingCartViewModel;
		public int TotalCart { get; set; }
		public CartController(IUnitOfWork unitOfWork)
		{
			_UnitOfWork = unitOfWork;
		}
		[Authorize]
		public IActionResult Index()
		{
			// here i need user who login
			var IdentityClaims = (ClaimsIdentity)User.Identity;
			var claim = IdentityClaims.FindFirst(ClaimTypes.NameIdentifier);
			var userId = claim.Value;
			shoppingCartViewModel = new ShoppingCartViewModel()
			{
				Carts = _UnitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, Includeword: "Product"),
				OrderHeader =new()
			
			};
			// here i want to sum the carts which user order
			foreach (var item in shoppingCartViewModel.Carts)
			{
				shoppingCartViewModel.TotalCarts += (item.Count * item.Product.Price);
			}
			return View(shoppingCartViewModel);
		}
		[HttpGet]
		public IActionResult Summary()
		{
			var claimsidentity = (ClaimsIdentity)User.Identity;
			var claim = claimsidentity.FindFirst(ClaimTypes.NameIdentifier);
			var userId = claim.Value;
			shoppingCartViewModel = new ShoppingCartViewModel()
			{
				Carts = _UnitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, Includeword: "Product"),
				OrderHeader = new()

			};
			// this data that appear when opening the page (Customer info)
			shoppingCartViewModel.OrderHeader.ApplicationUser = _UnitOfWork.ApplicationUserRepository.GetById(x => x.Id == userId);
			shoppingCartViewModel.OrderHeader.Name = shoppingCartViewModel.OrderHeader.ApplicationUser.Name;
			shoppingCartViewModel.OrderHeader.Address = shoppingCartViewModel.OrderHeader.ApplicationUser.Address;
			shoppingCartViewModel.OrderHeader.City = shoppingCartViewModel.OrderHeader.ApplicationUser.City;
			shoppingCartViewModel.OrderHeader.PhoneNumber = shoppingCartViewModel.OrderHeader.ApplicationUser.PhoneNumber;
			foreach (var item in shoppingCartViewModel.Carts)
			{
				shoppingCartViewModel.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
			}
			return View(shoppingCartViewModel);

		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult PostSummary(ShoppingCartViewModel ShoppingCartViewModel)
		{
			// get the user 
			var claimsidentity = (ClaimsIdentity)User.Identity;
			var claim = claimsidentity.FindFirst(ClaimTypes.NameIdentifier);
			var userId = claim.Value;

			// get the carts of that user assigned
			ShoppingCartViewModel.Carts = _UnitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, Includeword: "Product");
			#region Order Header Info 
			// set a status data info for user
			ShoppingCartViewModel.OrderHeader.OrderStatus = Helper.Pending;
			ShoppingCartViewModel.OrderHeader.PaymentStatus = Helper.Pending;
			ShoppingCartViewModel.OrderHeader.OrderDate = DateTime.Now;
			ShoppingCartViewModel.OrderHeader.ApplicationUserId = userId;

			// sum count of cart total price
			foreach (var item in ShoppingCartViewModel.Carts)
			{
				ShoppingCartViewModel.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
			}
			// add this data in OrderHeader in DB
			_UnitOfWork.OrderHeaderRepository.Add(ShoppingCartViewModel.OrderHeader);
			_UnitOfWork.complete();
			#endregion
			#region Order Detail Info
			// here it will loop in Carts and get the details of order and set it into Order Detail Model
			foreach (var cart in ShoppingCartViewModel.Carts)
			{
				var OrderDetail = new OrderDetail()
				{
					ProductId = cart.ProductId,
					OrderId = ShoppingCartViewModel.OrderHeader.Id,
					Price = cart.Product.Price,
					Count = cart.Count
				};
				// add this data in OrderDetail in DB
				_UnitOfWork.OrderDetailRepository.Add(OrderDetail);
				_UnitOfWork.complete();
			}
			#endregion
			// here code of stripe method (from stripe site) but i set my value 
			#region Stripe code
			var domain = "https://localhost:7148/"; // manual
			var options = new SessionCreateOptions
			{
				LineItems = new List<SessionLineItemOptions>(),
				// manual
				Mode = "payment",
				SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartViewModel.OrderHeader.Id}",
				CancelUrl = domain + $"Customer/Cart/Index",
			};
			// manual to set my data
			foreach (var item in ShoppingCartViewModel.Carts)
			{
				var sessionLineOption= new SessionLineItemOptions
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
			Session session = service.Create(options);
			// check of session to confirm (success of not)
			ShoppingCartViewModel.OrderHeader.SissionId = session.Id;
			_UnitOfWork.complete();
			Response.Headers.Add("Location", session.Url);
			#endregion

			return new StatusCodeResult(303);

		}
		public IActionResult OrderConfirmation(int id)
		{
			var OrderHeader = _UnitOfWork.OrderHeaderRepository.GetById(u => u.Id == id);
			// check my session in stripe
			var service = new SessionService();
			var session = service.Get(OrderHeader.SissionId);
			if (session.PaymentStatus.ToLower() == "paid")
			{
				_UnitOfWork.OrderHeaderRepository.UpdateOrderStatus(id, Helper.Approve, Helper.Approve);
				// when order done it will fill the paymentIntentId of Db with PII with stripe
				OrderHeader.PaymentIntentId = session.PaymentIntentId;
				_UnitOfWork.complete();
			}
			// if status is paid then i need to remove items from cart 
			var shoppingCart = _UnitOfWork.ShoppingCartRepository.GetAll(u=>u.ApplicationUserId == OrderHeader.ApplicationUserId).ToList();
			_UnitOfWork.ShoppingCartRepository.RemoveRange(shoppingCart);
			_UnitOfWork.complete();
			return View();
		}
		public IActionResult Plus(int CartId)
		{
			var ShoppingCart = _UnitOfWork.ShoppingCartRepository.GetById(c => c.Id == CartId);
			_UnitOfWork.ShoppingCartRepository.IncreaseCount(ShoppingCart, 1);
			_UnitOfWork.complete();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Minus(int CartId)
		{
			var ShoppingCart = _UnitOfWork.ShoppingCartRepository.GetById(c => c.Id == CartId);
			if (ShoppingCart.Count <= 1)
			{
				_UnitOfWork.ShoppingCartRepository.Remove(ShoppingCart);
				var count = _UnitOfWork.ShoppingCartRepository.GetAll(s => s.ApplicationUserId == ShoppingCart.ApplicationUserId).ToList().Count()-1;
				HttpContext.Session.SetInt32(Helper.SessionKey, count);
				_UnitOfWork.complete();
				return RedirectToAction(nameof(Index), "Home");
			}
			else
			{
				_UnitOfWork.ShoppingCartRepository.DecreaseCount(ShoppingCart, 1);
			}
			_UnitOfWork.complete();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Remove(int cartId)
		{
			var ShoppingCart = _UnitOfWork.ShoppingCartRepository.GetById(c => c.Id == cartId);
			_UnitOfWork.ShoppingCartRepository.Remove(ShoppingCart);
			_UnitOfWork.complete();
			var count = _UnitOfWork.ShoppingCartRepository.GetAll(s => s.ApplicationUserId == ShoppingCart.ApplicationUserId).ToList().Count();
			HttpContext.Session.SetInt32(Helper.SessionKey, count);
			return RedirectToAction(nameof(Index));
		}
		
	}
}
