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

		public IActionResult Index()
		{
			var products = _unitOfWork.ProductRepository.GetAll();
			return View(products);
		}
		public IActionResult Details(int id) // question here (don't forget)
		{
			var shoppingCart = new ShoppingCart()
			{
				
				Product = _unitOfWork.ProductRepository.GetById(x => x.Id == id, Includeword: "Category"),
				ProductId = id,
				Count = 1 // here when customer need to add item to cart
			};
			return View(shoppingCart);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize]
		public IActionResult Details(ShoppingCart shoppingCart)
		{
			// here i bring user who has access (login)
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var Claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
			shoppingCart.ApplicationUserId = Claim.Value;

			// here i want to check if customer has previous order of same product (count of order)
			var Cart = _unitOfWork.ShoppingCartRepository.GetById(
				u => u.ApplicationUserId==Claim.Value && u.ProductId == shoppingCart.ProductId);
			if(Cart == null)
			{
				// if this is first one it only add it to DB
				_unitOfWork.ShoppingCartRepository.Add(shoppingCart);
			}
			else
			{
				// if he had previous order with same order
					_unitOfWork.ShoppingCartRepository.IncreaseCount(Cart, shoppingCart.Count);
			}
			_unitOfWork.complete();
			return RedirectToAction(nameof(Index));
		}
	}
}
