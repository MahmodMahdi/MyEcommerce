using MyEcommerce.ApplicationLayer.ViewModels;
namespace MyEcommerce.ApplicationLayer.Interfaces.Services
{
	public interface IHomeService
	{
		Task<PaginatedResultViewModel<ProductViewModel>> GetAllAsync(int pageNumber);
		Task<CartItemViewModel> GetProductDetailsAsync(int productId);
		Task<int> AddToCartAsync(CartItemViewModel cartItemViewModel, string userId);
	}
}
