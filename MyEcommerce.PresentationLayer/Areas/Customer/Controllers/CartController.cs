using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.Models;
using MyEcommerce.DomainLayer.ViewModels;
using System.Security.Claims;

namespace MyEcommerce.PresentationLayer.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class CartController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private ShoppingCartViewModel shoppingCartViewModel;
		public CartController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
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
				Carts = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, Includeword: "Product")
			};
			// here i want to sum the carts which user order
			foreach (var item in shoppingCartViewModel.Carts)
			{
				shoppingCartViewModel.TotalCarts += (item.Count * item.Product.Price);
			}
			return View(shoppingCartViewModel);
		}
		public IActionResult Plus(int cartId)
		{
			var ShoppingCart = _unitOfWork.ShoppingCartRepository.GetById(c => c.Id == cartId);
			_unitOfWork.ShoppingCartRepository.IncreaseCount(ShoppingCart, 1);
			_unitOfWork.complete();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Minus(int cartId)
		{
			var ShoppingCart = _unitOfWork.ShoppingCartRepository.GetById(c => c.Id == cartId);
			if (ShoppingCart.Count <= 1)
			{
				_unitOfWork.ShoppingCartRepository.Remove(ShoppingCart);
				_unitOfWork.complete();
				return RedirectToAction(nameof(Index), "Home");
			}
			else
			{
				_unitOfWork.ShoppingCartRepository.DecreaseCount(ShoppingCart, 1);
			}
			_unitOfWork.complete();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Remove(int cartId)
		{
			var ShoppingCart = _unitOfWork.ShoppingCartRepository.GetById(c => c.Id == cartId);
			_unitOfWork.ShoppingCartRepository.Remove(ShoppingCart);
			_unitOfWork.complete();
			return RedirectToAction(nameof(Index));
		}
	}
}
