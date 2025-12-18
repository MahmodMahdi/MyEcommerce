using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.Models;
using MyEcommerce.DomainLayer.Services;
using MyEcommerce.DomainLayer.ViewModels;
namespace MyEcommerce.PresentationLayer.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ProductController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IImageService _imageService;

		public ProductController(IUnitOfWork unitOfWork, IImageService imageService)
		{
			_unitOfWork = unitOfWork;
			_imageService = imageService;
		}

		#region Index
		public IActionResult Index() => View();

		[HttpGet]
		public async Task<IActionResult> GetData()
		{
			var products = await _unitOfWork.ProductRepository.GetAllAsync(IncludeProperties: "Category");
			return Json(new { data = products });
		}
		#endregion

		#region Create
		[HttpGet]
		public async Task<IActionResult> Create()
		{
			var productVM = new ProductViewModel
			{
				Product = new Product(),
			};
			await PopulateCategories(productVM);
			return View(productVM);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ProductViewModel productVM, IFormFile image)
		{
			// 🔹 تهيئة categories قبل ModelState check
			await PopulateCategories(productVM);

			if (image == null)
				ModelState.AddModelError("Product.Image", "Please choose product image");

			if (!ModelState.IsValid)
				return View(productVM);

			// حفظ الصورة
			productVM.Product.Image = await _imageService.SaveAsync(image);

			// إضافة المنتج
			await _unitOfWork.ProductRepository.AddAsync(productVM.Product);
			await _unitOfWork.CompleteAsync();

			TempData["Create"] = "Product created successfully";
			return RedirectToAction(nameof(Index));
		}
		#endregion

		#region Edit
		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			if (id <= 0) return NotFound();

			var product = await _unitOfWork.ProductRepository.GetByIdAsync(x => x.Id == id);
			if (product == null) return NotFound();

			var productVM = new ProductViewModel { Product = product };
			await PopulateCategories(productVM);

			return View(productVM);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(ProductViewModel productVM, IFormFile? image)
		{
			// populate categories أول حاجة قبل ModelState check
			await PopulateCategories(productVM);

			if (!ModelState.IsValid)
				return View(productVM);

			var oldProduct = await _unitOfWork.ProductRepository.GetByIdAsync(x => x.Id == productVM.Product.Id);
			if (oldProduct == null) return NotFound();

			if (image != null)
			{
				await _imageService.DeleteAsync(oldProduct.Image);
				productVM.Product.Image = await _imageService.SaveAsync(image);
			}
			else
			{
				productVM.Product.Image ??= oldProduct.Image;
			}

			await _unitOfWork.ProductRepository.UpdateAsync(productVM.Product);
			await _unitOfWork.CompleteAsync();

			TempData["Update"] = "Product updated successfully";
			return RedirectToAction(nameof(Index));
		}

		#endregion

		#region Delete
		[HttpDelete]
		public async Task<IActionResult> Delete(int id)
		{
			if (id <= 0)
				return Json(new { success = false, message = "Invalid ID" });

			var product = await _unitOfWork.ProductRepository.GetByIdAsync(x => x.Id == id);
			if (product == null)
				return Json(new { success = false, message = "Product not found" });

			await _imageService.DeleteAsync(product.Image);
			await _unitOfWork.ProductRepository.RemoveAsync(product);
			await _unitOfWork.CompleteAsync();

			return Json(new { success = true, message = "Product deleted successfully" });
		}
		#endregion

		#region Helpers
		private async Task PopulateCategories(ProductViewModel productVM)
		{
			var categories = await _unitOfWork.CategoryRepository.GetAllAsync();

			productVM.Categories = categories.Select(c => new DropDownItem
			{
				Value = c.Id.ToString(),
				Text = c.Name
			}).ToList();

			ViewBag.CategoryDropDownList = productVM.Categories.Select(x =>
				new SelectListItem
				{
					Value = x.Value,
					Text = x.Text
				});
		}
		#endregion
	}
}
