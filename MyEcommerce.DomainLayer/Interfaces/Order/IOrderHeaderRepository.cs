using MyEcommerce.DomainLayer.Models.Order;

namespace MyEcommerce.DomainLayer.Interfaces.Order
{
	public interface IOrderHeaderRepository : IGenericRepository<OrderHeader>
	{
		Task UpdateAsync(OrderHeader orderHeader);
		Task UpdateOrderStatusAsync (int id, string? OrderStatus,string? PaymentStatus);
		Task<string> TopPurchasedBuyerAsync();
	}
}
