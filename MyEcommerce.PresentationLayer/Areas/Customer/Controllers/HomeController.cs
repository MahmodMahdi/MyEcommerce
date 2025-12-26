using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEcommerce.ApplicationLayer.Interfaces.Services;
using MyEcommerce.ApplicationLayer.ViewModels;
using System.Security.Claims;
using Utilities;
namespace MyEcommerce.PresentationLayer.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class HomeController : Controller
	{
		private readonly IHomeService _homeService;

		public HomeController(IHomeService homeService)
		{
			_homeService = homeService;
		}

		public async Task<IActionResult> Index(int pageNumber = 1)
		{
			var products = await _homeService.GetAllAsync(pageNumber);
			return View(products);
		}
		public async Task<IActionResult> Details(int ProductId)
		{
			var shoppingCart =await _homeService.GetProductDetailsAsync(ProductId);
			return View(shoppingCart);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> Details(CartItemViewModel cartItemVM)
		{
			try
			{
				var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				int totalCartItems = await _homeService.AddToCartAsync(cartItemVM, userId);
				HttpContext.Session.SetInt32(Helper.SessionKey, totalCartItems);
				// here i want to check if customer has previous order of same product (count of order)
				TempData["Update"] = "Item added to cart successfully!";
				return RedirectToAction(nameof(Index));
			}
			catch(Exception ex)
			{
				TempData["Error"] = ex.Message;
				return RedirectToAction(nameof(Details), new { productId = cartItemVM.ProductId });
			}
		}
	}
}
