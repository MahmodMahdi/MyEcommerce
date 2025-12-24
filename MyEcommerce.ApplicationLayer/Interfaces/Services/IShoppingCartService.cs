using MyEcommerce.ApplicationLayer.ViewModels;

namespace MyEcommerce.ApplicationLayer.Interfaces.Services
{
	public interface IShoppingCartService
	{
		Task<ShoppingCartViewModel> GetAllAsync(string userId);
		Task<ShoppingCartViewModel> GetSummaryAsync(string userId);
		Task<ShoppingCartViewModel> CreateOrderAsync(ShoppingCartViewModel shoppingCartViewModel, string userId, string domain);
		Task<bool> OrderConfirmation(int orderId);
		Task<int> IncrementCountAsync(int cartId);
		Task<int> DecrementCountAsync(int cartId);
		Task<int> DeleteAsync(int id);
	}
}
