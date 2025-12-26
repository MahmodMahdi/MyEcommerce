using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEcommerce.ApplicationLayer.ViewModels;
using Utilities;
using MyEcommerce.ApplicationLayer.Interfaces.Services;

namespace MyEcommerce.PresentationLayer.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = Helper.AdminRole)]
	public class OrderController : Controller
	{
		private readonly IOrderServices _orderServices;
		public OrderController(IOrderServices orderService)
		{
			_orderServices = orderService;
		}
		#region Index (List of Orders)
		public IActionResult Index()
		{
			return View();
		}
		[HttpGet]
		public async Task<IActionResult> GetData()
		{
			var OrderHeaders = await _orderServices.GetAllAsync();
			return Json(new { data = OrderHeaders });
		}
		#endregion
		#region Details of Order of Users
		[HttpGet]
		public async Task<IActionResult> Details(int OrderId)
		{
			var orderViewModel =await _orderServices.GetOrderViewModelAsync(OrderId);
			return View(orderViewModel);
		}
		#endregion
		#region Update Details of Order
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateDetails(UpdateOrderDto orderViewModel)
		{
			if (!ModelState.IsValid)
			{
				var Order = await _orderServices.GetOrderViewModelAsync(orderViewModel.OrderId);
				return View("Details", Order);
			}
			var success = await _orderServices.UpdateOrderDetialsAsync(orderViewModel);

			if (success)
			{
				TempData["Update"] = "Data has Updated succesfully";
			}
			else
			{
				TempData["Error"] = "Order not found or update failed ";
			}

			return RedirectToAction(nameof(Details), new { OrderId = orderViewModel.OrderId });
		}
		#endregion
		#region Order Status
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> StartProcessing(OrderViewModel orderViewModel)
		{
			var success = await _orderServices.StartProccessing(orderViewModel);
			if (success)
			{
				TempData["Update"] = "Order is now in processing stage.";
			}
			else
			{
				TempData["Error"] = "Order not found!";
			}

			return RedirectToAction(nameof(Details), new { OrderId = orderViewModel.OrderHeader.Id });
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> StartShipping(UpdateOrderDto shippingData)
		{
			var orderVM = await _orderServices.GetOrderViewModelAsync(shippingData.OrderId);
			orderVM.OrderHeader.Carrior = shippingData.Carrior;
			orderVM.OrderHeader.TrackingNumber = shippingData.TrackingNumber;
		
			var success = await _orderServices.StartShipping(orderVM);

			if (success)
			{
				TempData["Update"] = "Order has Shipped succesfully.";
			}
			else
			{
				TempData["Error"] = "Order not found!";
			}
			
			return RedirectToAction(nameof(Details), new { OrderId = shippingData.OrderId });
		}
		#endregion
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CancelOrder(OrderViewModel orderViewModel)
		{ 
			var success = await _orderServices.CancelOrderAsync(orderViewModel);

			if (success)
			{
				TempData["Update"] = "Order has Cancelled succesfully.";
			}
			else
			{
				TempData["Error"] = "Could not cancel order. Please check payment status.";
			}

			return RedirectToAction(nameof(Details), new { OrderId = orderViewModel.OrderHeader.Id });
		}
	}
}
