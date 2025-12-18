using Microsoft.AspNetCore.Mvc;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.Models;

namespace MyEcommerce.PresentationLayer.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class CategoryController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		public CategoryController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public async Task<IActionResult> Index()
		{
			var categories =await _unitOfWork.CategoryRepository.GetAllAsync();
			return View(categories);
		}
		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Category category)
		{
			if (ModelState.IsValid)
			{
				//_context.Categories.Add(category);
				await _unitOfWork.CategoryRepository.AddAsync(category);
				//_context.SaveChanges();
				await _unitOfWork.CompleteAsync();
				TempData["Create"] = "Data Has Created Successfully";
				return RedirectToAction("Index");
			}
			return View(category);
		}
		[HttpGet]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null | id == 0)
			{
				NotFound();
			}
			var category =await _unitOfWork.CategoryRepository.GetByIdAsync(c=>c.Id == id);
			return View(category);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(Category category)
		{
			if (ModelState.IsValid)
			{
				//_context.Categories.Update(category);
				await _unitOfWork.CategoryRepository.UpdateAsync(category);
				//_context.SaveChanges();
				await _unitOfWork.CompleteAsync();
				TempData["Update"] = "Data Has Updated Successfully";
				return RedirectToAction("Index");
			}
			return View(category);
		}
		[HttpGet]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null | id == 0)
			{
				NotFound();
			}
			var category =await _unitOfWork.CategoryRepository.GetByIdAsync(c=>c.Id==id);

			return View(category);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteCategory(int? id)
		{
			var category =await _unitOfWork.CategoryRepository.GetByIdAsync(c => c.Id == id);
			if (category == null)
			{
				{
					NotFound();
				}
			}
			//_context.Categories.Remove(category);
			await _unitOfWork.CategoryRepository.RemoveAsync(category);
			//_context.SaveChanges();
			await _unitOfWork.CompleteAsync();
			TempData["Delete"] = "Data Has Deleted Successfully";
			return RedirectToAction(nameof(Index));

		}
	}
}
