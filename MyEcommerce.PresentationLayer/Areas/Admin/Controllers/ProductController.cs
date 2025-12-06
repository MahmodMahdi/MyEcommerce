using Microsoft.AspNetCore.Mvc;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.Models;

namespace MyEcommerce.PresentationLayer.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ProductController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		public ProductController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			var products = _unitOfWork.ProductRepository.GetAll();
			return View(products);
		}
		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(Product product)
		{
			if (ModelState.IsValid)
			{
				//_context.Categories.Add(category);
				_unitOfWork.ProductRepository.Add(product);
				//_context.SaveChanges();
				_unitOfWork.complete();
				TempData["Create"] = "Data Has Created Successfully";
				return RedirectToAction("Index");
			}
			return View(product);
		}
		[HttpGet]
		public IActionResult Edit(int? id)
		{
			if (id == null | id == 0)
			{
				NotFound();
			}
			var product = _unitOfWork.ProductRepository.GetById(c => c.Id == id);
			return View(product);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(Product product)
		{
			if (ModelState.IsValid)
			{
				//_context.Categories.Update(category);
				_unitOfWork.ProductRepository.Update(product);
				//_context.SaveChanges();
				_unitOfWork.complete();
				TempData["Update"] = "Data Has Updated Successfully";
				return RedirectToAction("Index");
			}
			return View(product);
		}
		[HttpGet]
		public IActionResult Delete(int? id)
		{
			if (id == null | id == 0)
			{
				NotFound();
			}
			var product = _unitOfWork.ProductRepository.GetById(c => c.Id == id);

			return View(product);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult DeleteCategory(int? id)
		{
			var product = _unitOfWork.ProductRepository.GetById(c => c.Id == id);
			if (product == null)
			{
				{
					NotFound();
				}
			}
			//_context.Categories.Remove(category);
			_unitOfWork.ProductRepository.Remove(product);
			//_context.SaveChanges();
			_unitOfWork.complete();
			TempData["Delete"] = "Data Has Deleted Successfully";
			return RedirectToAction(nameof(Index));

		}
	}
}
