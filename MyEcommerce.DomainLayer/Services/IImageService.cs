using Microsoft.AspNetCore.Http;

namespace MyEcommerce.DomainLayer.Services
{
	public interface IImageService
	{
		Task<string> SaveAsync(IFormFile file);
		Task DeleteAsync(string? imagePath);
	}
}
