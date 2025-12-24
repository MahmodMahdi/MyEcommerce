using MyEcommerce.ApplicationLayer.ViewModels;
using MyEcommerce.DomainLayer.Models.Order;

namespace MyEcommerce.ApplicationLayer.Interfaces.Services
{
	public interface IOrderServices
	{
		Task<IEnumerable<OrderHeader>> GetAllAsync();
		Task<OrderViewModel> GetOrderViewModelAsync(int OrderId);
		Task<bool> UpdateOrderDetialsAsync(UpdateOrderDto orderViewModel);
		Task<bool> CancelOrderAsync(OrderViewModel orderViewModel);
		Task<bool> StartProccessing (OrderViewModel orderViewModel);
		Task<bool> StartShipping(OrderViewModel orderViewModel);
	}
}
