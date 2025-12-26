using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEcommerce.ApplicationLayer.ViewModels;
using System.Security.Claims;
using Utilities;
using MyEcommerce.ApplicationLayer.Interfaces.Services;

namespace MyEcommerce.PresentationLayer.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class CartController : Controller
	{
		private readonly IShoppingCartService _shoppingCartService;
		public int TotalCart { get; set; }
		public CartController(IShoppingCartService shoppingCartService)
		{
			_shoppingCartService = shoppingCartService;
		}
		[Authorize]
		public async Task<IActionResult> Index()
		{
			// here i need user who login
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var shoppingCartViewModel = await _shoppingCartService.GetAllAsync(userId);
			HttpContext.Session.SetInt32(Helper.SessionKey, shoppingCartViewModel.Carts.Count());
			return View(shoppingCartViewModel);
		}
		[Authorize]
		[HttpGet]
		public async Task<IActionResult> Summary()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			var shoppingCartVM = await _shoppingCartService.GetSummaryAsync(userId);
			if (shoppingCartVM.Carts == null || !shoppingCartVM.Carts.Any())
			{
				TempData["Error"] = "Your cart is empty or items are no longer available.";
				return RedirectToAction(nameof(Index));
			}
			HttpContext.Session.SetInt32(Helper.SessionKey, shoppingCartVM.Carts.Count());
			return View(shoppingCartVM);

		}
		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> PostSummary(ShoppingCartViewModel shoppingCartViewModel)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var domain = $"{Request.Scheme}://{Request.Host.Value}/";
			//var domain = "https://localhost:7148/"; // manual
			try
			{
				var result = await _shoppingCartService.CreateOrderAsync(shoppingCartViewModel, userId, domain);
				Response.Headers.Append("Location", result.Url);
				return new StatusCodeResult(303);
			}
			catch (Exception ex)
			{
				TempData["Error"] =ex.Message;
				return RedirectToAction(nameof(Index));
			}
		}
		[Authorize]
		public async Task<IActionResult> OrderConfirmation(int id)
		{
			try
			{
				bool isConfirmed = await _shoppingCartService.OrderConfirmation(id);
				if (isConfirmed)
				{
					HttpContext.Session.SetInt32(Helper.SessionKey, 0);
					return View(id);
				}
				return RedirectToAction($"{nameof(Index)}");
			}
			catch (Exception)
			{
				TempData["Error"] = "Connection lost while confirming payment. Don't worry, your order is being processed.";
				return RedirectToAction("Index", "Home");
			}
		}
		public async Task<IActionResult> Plus(int cartId)
		{
			try
			{
				var totalCart = await _shoppingCartService.IncrementCountAsync(cartId);
				HttpContext.Session.SetInt32(Helper.SessionKey, totalCart);
				return RedirectToAction(nameof(Index));
			}
			catch(InvalidOperationException ex)
			{
					TempData["Error"] = $"Sorry, only {ex.Message} items available in stock.";
					return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
			TempData["Error"] = "Something went wrong. Please try again";
				return RedirectToAction(nameof (Index));
			}
		}
		public async Task<IActionResult> Minus(int cartId)
		{
			try {
				var remainingItem = await _shoppingCartService.DecrementCountAsync(cartId);
				HttpContext.Session.SetInt32(Helper.SessionKey, remainingItem);
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				TempData["Error"] = "Could not update the quantity. Please try again.";
				return RedirectToAction(nameof(Index));
			}
		}
		[Authorize]
		public async Task<IActionResult> Remove(int cartId)
		{
			var totalCarts =  await _shoppingCartService.DeleteAsync(cartId);
			HttpContext.Session.SetInt32(Helper.SessionKey, totalCarts);
			return RedirectToAction(nameof(Index));
		}
	}
}
