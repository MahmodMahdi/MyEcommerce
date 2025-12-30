using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEcommerce.ApplicationLayer.Interfaces.Services;
using MyEcommerce.ApplicationLayer.Services;
using MyEcommerce.ApplicationLayer.ViewModels;
using Utilities;

namespace MyEcommerce.PresentationLayer.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = Helper.AdminRole + "," + Helper.EditorRole)]
	public class ProductController : Controller
	{
		private readonly IProductService _productService;
		private readonly ICategoryService _categoryService;
		public ProductController(IProductService productService, ICategoryService categoryService)
		{
			_productService = productService;
			_categoryService = categoryService;
		}

		#region Index
		public IActionResult Index() => View();

		[HttpGet]
		public async Task<IActionResult> GetData()
		{
			var products = await _productService.GetAllAsync(IncludeProperties: "Category");
			return Json(new { data = products });
		}
		#endregion

		#region Create
		[HttpGet]
		public async Task<IActionResult> Create()
		{
			var productVM = new ProductViewModel();
			await PopulateCategories(productVM);
			return View(productVM);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ProductViewModel productVM, IFormFile image)
		{
			await PopulateCategories(productVM);

			if (image == null)
				ModelState.AddModelError("Image", "Please choose product image");

			if (ModelState.IsValid)
			{
				try
				{
					await _productService.AddAsync(productVM, image);
					TempData["Create"] = "Product created successfully";
					return RedirectToAction(nameof(Index));
				}
				catch (ArgumentException ex)
				{
					ModelState.AddModelError("Image", ex.Message);
					return View(productVM);
				}
			}
			return View(productVM);
		}
		#endregion

		#region Edit
		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			if (id <= 0) return NotFound();

			var product = await _productService.GetFirstOrDefaultAsync(x => x.Id == id);
			if (product == null) return NotFound();

			await PopulateCategories(product);

			return View(product);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(ProductViewModel productVM, IFormFile? image)
		{
			await PopulateCategories(productVM);

			if (ModelState.IsValid)
			{
				try
				{
					await _productService.UpdateAsync(productVM, image);
					TempData["Update"] = "Product updated successfully";
					return RedirectToAction(nameof(Index));
				}
				catch (ArgumentException ex)
				{
					ModelState.AddModelError("Image", ex.Message);
					return View(productVM);
				}
			}
			return View(productVM);
		}

		#endregion

		#region Delete
		[HttpDelete]
		public async Task<IActionResult> Delete(int id)
		{
			if (id <= 0)
				return Json(new { success = false, message = "Invalid ID" });

			var product = await _productService.GetFirstOrDefaultAsync(x => x.Id == id);
			if (product == null)
				return Json(new { success = false, message = "Product not found" });

			await _productService.DeleteAsync(id);

			return Json(new { success = true, message = "Product deleted successfully" });
		}
		#endregion

		#region Helpers
		private async Task PopulateCategories(ProductViewModel productVM)
		{
			var categories = await _categoryService.GetAllAsync();

			productVM.Categories = categories.Select(c => new DropDownItem
			{
				Value = c.Id.ToString(),
				Text = c.Name
			}).ToList();
		}
		#endregion
	}
}
