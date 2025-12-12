using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEcommerce.DataAccessLayer.Repositories;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.ViewModels;
using Stripe;
using Utilities;

namespace MyEcommerce.PresentationLayer.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = Helper.AdminRole)]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		public OrderController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		#region Index (List of Orders)
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
		#endregion
		#region Details of Order of Users
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
		#endregion
		#region Update Details of Order
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
		#endregion
		#region Order Status
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult StartProcessing(OrderViewModel orderViewModel)
		{
			// bring Id of Order
			_unitOfWork.OrderHeaderRepository.GetById(o => o.Id == orderViewModel.OrderHeader.Id);
			// update status of Order
			_unitOfWork.OrderHeaderRepository.UpdateOrderStatus(orderViewModel.OrderHeader.Id, Helper.Proccessing, null);
			_unitOfWork.complete();
			TempData["Update"] = "Order Status has Updated succesfully";
			return RedirectToAction("Details", "Order", new { orderid = orderViewModel.OrderHeader.Id });
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult StartShipping(OrderViewModel orderViewModel)
		{
			//bring order from db
			var orderFromDB = _unitOfWork.OrderHeaderRepository.GetById(o => o.Id == orderViewModel.OrderHeader.Id);
			// update data of order like when shipping process ( TrackingNumber to follow the order,Carrior and status and Date of Shipping )
			orderFromDB.TrackingNumber = orderViewModel.OrderHeader.TrackingNumber;
			orderFromDB.Carrior = orderViewModel.OrderHeader.Carrior;
			orderFromDB.OrderStatus = Helper.Shipped;
			orderFromDB.ShippingDate = DateTime.Now;
			_unitOfWork.OrderHeaderRepository.Update(orderFromDB);
			_unitOfWork.complete();
			TempData["Update"] = "Order has Shipped succesfully";
			return RedirectToAction("Details", "Order", new { orderid = orderViewModel.OrderHeader.Id });
		}
		#endregion
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult CancelOrder(OrderViewModel orderViewModel)
		{
			//bring order from db
			var orderFromDB = _unitOfWork.OrderHeaderRepository.GetById(o => o.Id == orderViewModel.OrderHeader.Id);
			if (orderFromDB.PaymentStatus == Helper.Approve)
			{
				var option = new RefundCreateOptions
				{
					Reason = RefundReasons.RequestedByCustomer,
					PaymentIntent = orderFromDB.PaymentIntentId
				};
				var service = new RefundService();
				Refund refund = service.Create(option);

				_unitOfWork.OrderHeaderRepository.UpdateOrderStatus(orderFromDB.Id, Helper.Cancelled, Helper.Refund);
			}
			else
			{
				_unitOfWork.OrderHeaderRepository.UpdateOrderStatus(orderFromDB.Id, Helper.Cancelled, Helper.Cancelled);
			}
			_unitOfWork.complete();
				TempData["Update"] = "Order has Cancelled succesfully";
			return RedirectToAction("Details", "Order", new { orderid = orderViewModel.OrderHeader.Id });
		}
	}
}
