using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
		public async Task<IActionResult> GetData()
		{
			var OrderHeaders = await _unitOfWork.OrderHeaderRepository.GetAllAsync(IncludeProperties: "ApplicationUser");
			return Json(new { data = OrderHeaders });
		}
		#endregion
		#region Details of Order of Users
		[HttpGet]
		public async Task<IActionResult> Details(int OrderId)
		{
			var OrderViewModel = new OrderViewModel()
			{
				OrderHeader = await _unitOfWork.OrderHeaderRepository.GetByIdAsync(x => x.Id == OrderId, IncludeProperties: "ApplicationUser"),
				OrderDetails = await _unitOfWork.OrderDetailRepository.GetAllAsync(x => x.OrderId == OrderId, IncludeProperties: "Product")
			};
			return View(OrderViewModel);
		}
		#endregion
		#region Update Details of Order
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateDetails(OrderViewModel orderViewModel)
		{
			// حذف الخصائص التى لا يتم ارسالها من الفورم لتجنب مشاكل modelState
			ModelState.Remove(nameof(orderViewModel.OrderDetails));
			if (!ModelState.IsValid)
			{
				orderViewModel.OrderDetails = await _unitOfWork.OrderDetailRepository.GetAllAsync(x => x.OrderId == orderViewModel.OrderHeader.Id, IncludeProperties: "Product");
				return View("Details", orderViewModel);
			}
			var order = await _unitOfWork.OrderHeaderRepository.GetByIdAsync(x => x.Id == orderViewModel.OrderHeader.Id);
			if (order == null) return NotFound();
			order.Name = orderViewModel.OrderHeader.Name;
			order.PhoneNumber = orderViewModel.OrderHeader.PhoneNumber;
			order.Address = orderViewModel.OrderHeader.Address;
			order.City = orderViewModel.OrderHeader.City;
			if (!string.IsNullOrEmpty(orderViewModel.OrderHeader.Carrior))
			{
				order.Carrior = orderViewModel.OrderHeader.Carrior;
			}
			if (!string.IsNullOrEmpty(orderViewModel.OrderHeader.TrackingNumber))
			{
				order.TrackingNumber = orderViewModel.OrderHeader.TrackingNumber;
			}
			await _unitOfWork.OrderHeaderRepository.UpdateAsync(order);
			await _unitOfWork.CompleteAsync();
			TempData["Update"] = "Data has Updated succesfully";
			return RedirectToAction(nameof(Details), new { OrderId = order.Id });
		}
		#endregion
		#region Order Status
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> StartProcessing(OrderViewModel orderViewModel)
		{
			// bring Id of Order
			await _unitOfWork.OrderHeaderRepository.GetByIdAsync(o => o.Id == orderViewModel.OrderHeader.Id);
			// update status of Order
			await _unitOfWork.OrderHeaderRepository.UpdateOrderStatusAsync(orderViewModel.OrderHeader.Id, Helper.Proccessing, null);
			await _unitOfWork.CompleteAsync();
			TempData["Update"] = "Order Status has Updated succesfully";
			return RedirectToAction(nameof(Details), new { orderid = orderViewModel.OrderHeader.Id });
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> StartShipping(OrderViewModel orderViewModel)
		{
			//bring order from db
			var orderFromDB = await _unitOfWork.OrderHeaderRepository.GetByIdAsync(o => o.Id == orderViewModel.OrderHeader.Id);
			// update data of order like when shipping process ( TrackingNumber to follow the order,Carrior and status and Date of Shipping )
			orderFromDB.TrackingNumber = orderViewModel.OrderHeader.TrackingNumber;
			orderFromDB.Carrior = orderViewModel.OrderHeader.Carrior;
			orderFromDB.OrderStatus = Helper.Shipped;
			orderFromDB.ShippingDate = DateTime.Now;
			await _unitOfWork.OrderHeaderRepository.UpdateAsync(orderFromDB);
			await _unitOfWork.CompleteAsync();
			TempData["Update"] = "Order has Shipped succesfully";
			return RedirectToAction(nameof(Details), new { orderid = orderViewModel.OrderHeader.Id });
		}
		#endregion
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CancelOrder(OrderViewModel orderViewModel)
		{
			//bring order from db
			var orderFromDB = await _unitOfWork.OrderHeaderRepository.GetByIdAsync(o => o.Id == orderViewModel.OrderHeader.Id);
			if (orderFromDB == null) return NotFound();
			if (orderFromDB.PaymentStatus == Helper.Approve)
			{
				try
				{
					var option = new RefundCreateOptions
					{
						Reason = RefundReasons.RequestedByCustomer,
						PaymentIntent = orderFromDB.PaymentIntentId
					};
					var service = new RefundService();
					Refund refund = service.Create(option);

					await _unitOfWork.OrderHeaderRepository.UpdateOrderStatusAsync(orderFromDB.Id, Helper.Cancelled, Helper.Refund);
				}
				catch (StripeException ex)
				{
					TempData["Error"] = "Stripe Refund Error: " + ex.Message;
					return RedirectToAction(nameof(Details), new { OrderId = orderFromDB.Id });
				}
			}
			else
			{
				await _unitOfWork.OrderHeaderRepository.UpdateOrderStatusAsync(orderFromDB.Id, Helper.Cancelled, Helper.Cancelled);
			}
			await _unitOfWork.CompleteAsync();
			TempData["Update"] = "Order has Cancelled succesfully";
			return RedirectToAction(nameof(Details), new { orderid = orderViewModel.OrderHeader.Id });
		}
	}
}
