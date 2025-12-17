using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEcommerce.DomainLayer.Services
{
	public interface IImageService
	{
		Task<string> SaveAsync(IFormFile file);
		Task DeleteAsync(string? imagePath);
	}
}
