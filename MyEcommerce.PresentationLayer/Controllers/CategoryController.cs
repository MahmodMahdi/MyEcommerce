using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.Models;

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
		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Category category)
		{
			if (ModelState.IsValid)
			{
				//_context.Categories.Add(category);
				_unitOfWork.CategoryRepository.Add(category);
				//_context.SaveChanges();
				_unitOfWork.complete();
				TempData["Create"] = "Data Has Created Successfully";
				return RedirectToAction("Index");
			}
			return View(category);
		}
	}
}
