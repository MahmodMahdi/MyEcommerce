using Microsoft.AspNetCore.Mvc;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.ViewModels;

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
		public IActionResult Details(int id)
		{
			var shoppingCart = new ShoppingCart()
			{
				Product = _unitOfWork.ProductRepository.GetById(p => p.Id == id, Includeword: "Category"),
				Count = 1 // here when customer need to add item to cart
			};
			return View(shoppingCart);
		}
	}
}
