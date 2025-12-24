using Microsoft.AspNetCore.Http;
using MyEcommerce.ApplicationLayer.ViewModels;
using MyEcommerce.DomainLayer.Models;
using System.Linq.Expressions;

namespace MyEcommerce.ApplicationLayer.Interfaces.Services
{
	public interface IProductService
	{
		Task<IEnumerable<ProductViewModel>> GetAllAsync(Expression<Func<Product, bool>>? predicate = null, string? IncludeProperties = null);
		Task<ProductViewModel> GetFirstOrDefaultAsync(Expression<Func<Product, bool>>? predicate = null, string? IncludeProperties = null);
		Task AddAsync(ProductViewModel productViewModel,IFormFile image);
		Task UpdateAsync(ProductViewModel productViewModel,IFormFile image);
		Task DeleteAsync (int id);
	}
}
