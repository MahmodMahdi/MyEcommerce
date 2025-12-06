using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEcommerce.DomainLayer.Interfaces;

namespace MyEcommerce.PresentationLayer.Controllers
{
	public class CategoryController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		public CategoryController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			var categories = _unitOfWork.CategoryRepository.GetAll();
			return View(categories);
		}

	}
}
