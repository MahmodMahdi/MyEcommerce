using Microsoft.AspNetCore.Mvc;
using MyEcommerce.DomainLayer.Interfaces;

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
		public IActionResult GetData() // return json of product data to show it into index
		{
			
			var OrderHeaders = _unitOfWork.OrderHeaderRepository.GetAll(Includeword: "ApplicationUser");
			return Json(new { data = OrderHeaders });
		}
	}
}
