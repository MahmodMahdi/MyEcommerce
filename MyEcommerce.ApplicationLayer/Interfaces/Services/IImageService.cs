using Microsoft.AspNetCore.Http;

namespace MyEcommerce.ApplicationLayer.Interfaces.Services
{
	public interface IImageService
	{
		Task<string> SaveAsync(IFormFile file);
		Task DeleteAsync(string? imagePath);
	}
}
