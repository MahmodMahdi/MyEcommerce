using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEcommerce.DomainLayer.Interfaces;
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

		public async Task<IActionResult> Index()
		{
			var MostPurchasedProduct =await _unitOfWork.OrderDetailRepository.MostPurchasedProductAsync();
			var MostPurchasedBuyer =await _unitOfWork.OrderHeaderRepository.TopPurchasedBuyerAsync();

			var orders =await _unitOfWork.OrderHeaderRepository.GetAllAsync();
			ViewBag.Orders = orders.Count();
			
			var ApprovedOrder =await _unitOfWork.OrderHeaderRepository.GetAllAsync(A => A.OrderStatus == Helper.Approve);
            ViewBag.ApprovedOrders = ApprovedOrder.Count();
			
			var ShippedOrder =await _unitOfWork.OrderHeaderRepository.GetAllAsync(A => A.OrderStatus == Helper.Shipped);
            ViewBag.ShippedOrders = ShippedOrder.Count();
			
			var PendingOrder =await _unitOfWork.OrderHeaderRepository.GetAllAsync(A => A.OrderStatus == Helper.Pending);
			ViewBag.PendingOrders = PendingOrder.Count();

			ViewBag.MostPurchasedProduct = MostPurchasedProduct;
			ViewBag.MostBuyer = MostPurchasedBuyer;

			var Users =await _unitOfWork.ApplicationUserRepository.GetAllAsync();
			ViewBag.Users = Users.Count();

			var Products = await _unitOfWork.ProductRepository.GetAllAsync();
			ViewBag.Products = Products.Count();

			return View();
		}
	}
}
