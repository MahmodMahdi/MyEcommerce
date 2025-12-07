using Microsoft.AspNetCore.Mvc;
using MyEcommerce.DataAccessLayer.Repositories;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.Models;
using MyEcommerce.DomainLayer.ViewModels;

namespace MyEcommerce.PresentationLayer.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ProductController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
		{
			_unitOfWork = unitOfWork;
			_webHostEnvironment = webHostEnvironment;
		}
		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}
		[HttpGet]
		public IActionResult GetData() // return json of product data to show it into index
		{
			var products = _unitOfWork.ProductRepository.GetAll(Includeword:"Category");
			return Json(new { data = products });
		}
		[HttpGet]
		public IActionResult Create()
		{
			var productvm = new ProductViewModel()
			{
				Product = new Product()
			};
			PopulateCategoryDropDownList();
			return View(productvm);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(ProductViewModel productvm,IFormFile file)
		{
			if (ModelState.IsValid)
			{
				// first path
				string RootPath = _webHostEnvironment.WebRootPath;
				if(file != null)
				{
					string filename = Guid.NewGuid().ToString();
					// compile first path with folder of images
					var Upload = Path.Combine(RootPath, @"Image\Products");
					if (!Directory.Exists(Upload))
					{
						Directory.CreateDirectory(Upload);
					}
					// get extension of image
					var extension = Path.GetExtension(file.FileName);
					using(var filestream = new FileStream(Path.Combine(Upload, filename + extension), FileMode.Create))
					{
						file.CopyTo(filestream);
					}
					productvm.Product.Image = @"Images\Products\" + filename + extension;
				}
				_unitOfWork.ProductRepository.Add(productvm.Product);
				_unitOfWork.complete();
				TempData["Create"] = "Data Has Created Successfully";
				return RedirectToAction("Index");
			}
			ViewBag.CategoryDropDownList = _unitOfWork.CategoryRepository.GetAll();
			return View(productvm);
		}
		[HttpGet]
		public IActionResult Edit(int? id)
		{
			
			var productvm = new ProductViewModel()
			{
				Product = _unitOfWork.ProductRepository.GetById(c => c.Id == id)
			};
			PopulateCategoryDropDownList();
			if (id == null | id == 0)
			{
				NotFound();
			}
			return View(productvm);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(ProductViewModel productvm, IFormFile file)
		{
			if (ModelState.IsValid)
			{
				var oldProduct = _unitOfWork.ProductRepository.GetById(x=>x.Id==productvm.Product.Id);
				string RootPath = _webHostEnvironment.WebRootPath;
				if (file != null)
				{
					string filename = Guid.NewGuid().ToString();
					// compile first path with folder of images
					var Upload = Path.Combine(RootPath, @"Image\Products");
					// get extension of image
					var extension = Path.GetExtension(file.FileName);
					if (productvm.Product.Image != null)
					{
						var oldImg = Path.Combine(RootPath, oldProduct.Image.TrimStart('\\')); // here it show that img in db include /image/products so i made trim start
						if (System.IO.File.Exists(oldImg))
						{
							System.IO.File.Delete(oldImg);
						}
					}
					using (var filestream = new FileStream(Path.Combine(Upload, filename + extension), FileMode.Create))
					{
						file.CopyTo(filestream);
					}
					productvm.Product.Image = @"Image\Products\" + filename + extension;
				}
				_unitOfWork.ProductRepository.Update(productvm.Product);
				_unitOfWork.complete();
				TempData["Update"] = "Data Has Updated Successfully";
				return RedirectToAction("Index");
			}
			PopulateCategoryDropDownList();
			return View(productvm);
		}
		public void PopulateCategoryDropDownList()
		{
			ViewBag.CategoryDropDownList = _unitOfWork.CategoryRepository.GetAll();

		}
	}
}
