using Microsoft.AspNetCore.Mvc;
using MyEcommerce.DataAccessLayer.Repositories;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.ViewModels;

namespace MyEcommerce.PresentationLayer.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		public OrderController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			return View();
		}
		[HttpGet]
		public IActionResult GetData() // return json of product data to show it into index
		{

			var OrderHeaders = _unitOfWork.OrderHeaderRepository.GetAll(Includeword: "ApplicationUser");
			return Json(new { data = OrderHeaders });
		}
		[HttpGet]
		public IActionResult Details(int OrderId)
		{
			var OrderViewModel = new OrderViewModel()
			{
				OrderHeader = _unitOfWork.OrderHeaderRepository.GetById(x => x.Id == OrderId, Includeword: "ApplicationUser"),
				OrderDetails = _unitOfWork.OrderDetailRepository.GetAll(x => x.OrderId == OrderId, Includeword: "Product")
			};
			return View(OrderViewModel);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult UpdateDetails(OrderViewModel orderViewModel)
		{
			var order = _unitOfWork.OrderHeaderRepository.GetById(x => x.Id == orderViewModel.OrderHeader.Id);
			order.Name = orderViewModel.OrderHeader.Name;
			order.PhoneNumber = orderViewModel.OrderHeader.PhoneNumber;
			order.Address = orderViewModel.OrderHeader.Address;
			order.City = orderViewModel.OrderHeader.City;
			if (orderViewModel.OrderHeader.Carrior != null)
			{
				order.Carrior = orderViewModel.OrderHeader.Carrior;
			}
			if (orderViewModel.OrderHeader.TrackingNumber != null)
			{
				order.TrackingNumber = orderViewModel.OrderHeader.TrackingNumber;
			}
			_unitOfWork.OrderHeaderRepository.Update(order);
			_unitOfWork.complete();
			TempData["Update"] = "Data has Updated succesfully";
			return RedirectToAction("Details", "Order", new { orderid = order.Id });
		}
	}
}
