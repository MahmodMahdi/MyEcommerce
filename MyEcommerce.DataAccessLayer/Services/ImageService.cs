using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using MyEcommerce.DomainLayer.Services;

namespace MyEcommerce.DataAccessLayer.Services
{
	public class ImageService : IImageService
	{
		private readonly IWebHostEnvironment _webHostEnvironment;

		public ImageService(IWebHostEnvironment webHostEnvironment)
		{
			_webHostEnvironment = webHostEnvironment;
		}

		public async Task<string> SaveAsync(IFormFile file)
		{
			if (file == null || file.Length == 0)
				throw new ArgumentException("File is empty", nameof(file));

			var folder = Path.Combine(_webHostEnvironment.WebRootPath, "Image", "Products");
			Directory.CreateDirectory(folder);

			var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
			var fullPath = Path.Combine(folder, fileName);

			await using var stream = new FileStream(fullPath, FileMode.Create);
			await file.CopyToAsync(stream);

			return Path.Combine("Image", "Products", fileName).Replace("\\", "/");
		}

		public Task DeleteAsync(string? imagePath)
		{
			if (string.IsNullOrWhiteSpace(imagePath)) return Task.CompletedTask;

			var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.Replace("/", "\\"));
			if (File.Exists(fullPath))
				File.Delete(fullPath);

			return Task.CompletedTask;
		}
	}
}
