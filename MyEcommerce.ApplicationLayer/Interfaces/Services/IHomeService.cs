using MyEcommerce.ApplicationLayer.ViewModels;
using MyEcommerce.DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEcommerce.ApplicationLayer.Interfaces.Services
{
	public interface IHomeService
	{
		Task<PaginatedResultViewModel<ProductViewModel>> GetAllAsync(int pageNumber);
		Task<CartItemViewModel> GetProductDetailsAsync(int productId);
		Task<int> AddToCartAsync(CartItemViewModel cartItemViewModel,string userId);
	}
}
