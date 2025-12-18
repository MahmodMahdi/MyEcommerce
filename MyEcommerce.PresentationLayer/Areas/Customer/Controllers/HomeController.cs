using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.Models;
using System.Security.Claims;
using Utilities;
namespace MyEcommerce.PresentationLayer.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class HomeController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

		public HomeController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<IActionResult> Index(int pageNumber = 1)
		{
			int pageSize = 8;

			int totalItems = await _unitOfWork.ProductRepository.GetCountAsync();

			int numberOfPages = (int)Math.Ceiling((double)totalItems / pageSize);
			int numberOfItemToSkip = (pageNumber - 1) * pageSize;
			var products = await _unitOfWork.ProductRepository.GetPagedAsync(numberOfItemToSkip, pageSize);

			ViewBag.PageNo = pageNumber;
			ViewBag.NoOfPages = numberOfPages;
			return View(products);
		}
		public async Task<IActionResult> Details(int ProductId)
		{
			var shoppingCart = new ShoppingCart()
			{

				Product =await _unitOfWork.ProductRepository.GetByIdAsync(x => x.Id == ProductId,IncludeProperties: "Category"),
				ProductId = ProductId,
				Count = 1 // here when customer need to add item to cart
			};
			return View(shoppingCart);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> Details(ShoppingCart shoppingCart)
		{
			// here i bring user who has access (login)
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			shoppingCart.ApplicationUserId = userId;

			// here i want to check if customer has previous order of same product (count of order)
			var CartFromDb =await _unitOfWork.ShoppingCartRepository.GetByIdAsync(
				u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);
			if (CartFromDb == null)
			{
				// if this is first one it only add it to DB
				await _unitOfWork.ShoppingCartRepository.AddAsync(shoppingCart);
				await _unitOfWork.CompleteAsync();
				// here it set a session when user go to add product to cart
				var CartItems = await _unitOfWork.ShoppingCartRepository.GetAllAsync(s => s.ApplicationUserId == userId);
				HttpContext.Session.SetInt32(Helper.SessionKey,CartItems.Count());
				
			}
			else
			{
				// if he had previous order with same order
			    _unitOfWork.ShoppingCartRepository.IncreaseCount(CartFromDb, shoppingCart.Count);
				await _unitOfWork.CompleteAsync();
			}
			return RedirectToAction(nameof(Index));
		}
	}
}
