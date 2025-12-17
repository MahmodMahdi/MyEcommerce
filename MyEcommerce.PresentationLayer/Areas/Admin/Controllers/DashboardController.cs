using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.ViewModels;
using Utilities;

namespace MyEcommerce.PresentationLayer.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles =Helper.AdminRole)]
	public class DashboardController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

		public DashboardController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			var MostPurchasedProduct = _unitOfWork.OrderDetailRepository.MostPurchasedProduct();
			var MostPurchasedBuyer = _unitOfWork.OrderHeaderRepository.TopPurchasedBuyer();
			ViewBag.Orders = _unitOfWork.OrderHeaderRepository.GetAll().Count();
			ViewBag.ApprovedOrders = _unitOfWork.OrderHeaderRepository.GetAll(A => A.OrderStatus == Helper.Approve).Count();
			ViewBag.ShippedOrders = _unitOfWork.OrderHeaderRepository.GetAll(A => A.OrderStatus == Helper.Shipped).Count();
			ViewBag.PendingOrders = _unitOfWork.OrderHeaderRepository.GetAll(A => A.OrderStatus == Helper.Pending).Count();
			ViewBag.MostPurchasedProduct = MostPurchasedProduct;
			ViewBag.MostBuyer = MostPurchasedBuyer;
			ViewBag.Users = _unitOfWork.ApplicationUserRepository.GetAll().Count();
			ViewBag.Products = _unitOfWork.ProductRepository.GetAll().Count();
			return View();
		}
	}
}
