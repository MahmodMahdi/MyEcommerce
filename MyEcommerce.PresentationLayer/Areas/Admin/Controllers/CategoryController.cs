using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEcommerce.ApplicationLayer.Services;
using MyEcommerce.ApplicationLayer.ViewModels;
using Utilities;

namespace MyEcommerce.PresentationLayer.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = Helper.AdminRole)]
	public class CategoryController : Controller
	{
		private readonly ICategoryService _categoryService;
		public CategoryController(ICategoryService categoryService)
		{
			_categoryService = categoryService;
		}
		public async Task<IActionResult> Index()
		{
			var categories = await _categoryService.GetAllAsync();
			return View(categories);
		}
		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CategoryViewModel categoryVM)
		{
			if (ModelState.IsValid)
			{
				await _categoryService.AddAsync(categoryVM);
				TempData["Create"] = "Data Has Created Successfully";
				return RedirectToAction("Index");
			}
			return View(categoryVM);
		}
		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			if (id == null | id == 0)
			{
				return NotFound();
			}
			var category =await _categoryService.GetFirstOrDefaultAsync(id);
			return View(category);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(CategoryViewModel categoryVM)
		{
			if (ModelState.IsValid)
			{
				await _categoryService.UpdateAsync(categoryVM);
				TempData["Update"] = "Data Has Updated Successfully";
				return RedirectToAction("Index");
			}
			return View(categoryVM);
		}
		[HttpGet]
		public async Task<IActionResult> Delete(int id)
		{
			
			var category =await _categoryService.GetFirstOrDefaultAsync(id);
			if(category  == null)
				return NotFound();
			return View(category);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteCategory(int id)
		{
			var category = await _categoryService.GetFirstOrDefaultAsync(id);
			if (category == null)
				return NotFound();
			await _categoryService.DeleteAsync(id);
			TempData["Delete"] = "Data Has Deleted Successfully";
			return RedirectToAction(nameof(Index));

		}
	}
}
