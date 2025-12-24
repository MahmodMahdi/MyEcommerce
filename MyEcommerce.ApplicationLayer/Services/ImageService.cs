using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MyEcommerce.ApplicationLayer.Interfaces.Services;

namespace MyEcommerce.ApplicationLayer.Services
{
	public class ImageService : IImageService
	{
		private readonly IWebHostEnvironment _webHostEnvironment;
		public ImageService( IWebHostEnvironment webHostEnvironment)
		{
			_webHostEnvironment = webHostEnvironment;
		}

		public async Task<string> SaveAsync(IFormFile file)
		{
			if (file == null || file.Length == 0)
				throw new ArgumentException("File is empty", nameof(file));

			long maxImageSize = 2 *1024 * 1024;
			if (file.Length > maxImageSize)
				throw new ArgumentException("Max size of image is 2 Mega Byte");

			var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif",".webp" };
			var extension = Path.GetExtension(file.FileName).ToLower();
			if (!allowedExtensions.Contains(extension))
				throw new ArgumentException("Extension not supported! Please use JPG, JPEG, GIF, PNG or WebP");


			var folder = Path.Combine(_webHostEnvironment.WebRootPath, "Image", "Products");
			Directory.CreateDirectory(folder);

			var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
			var fullPath = Path.Combine(folder, fileName);

			await using var stream = new FileStream(fullPath, FileMode.Create);
			await file.CopyToAsync(stream);

			return $"Image/Products/{fileName}";
		}

		public Task DeleteAsync(string? imagePath)
		{
			if (string.IsNullOrWhiteSpace(imagePath)) return Task.CompletedTask;

			var cleanPath = imagePath.Replace("/",Path.DirectorySeparatorChar.ToString()).TrimStart(Path.DirectorySeparatorChar);
			var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, cleanPath);
			//var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.Replace("/", "\\"));
			if (File.Exists(fullPath))
				File.Delete(fullPath);

			return Task.CompletedTask;
		}
	}
}
