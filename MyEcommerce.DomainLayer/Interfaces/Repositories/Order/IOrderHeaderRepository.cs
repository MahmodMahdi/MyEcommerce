using MyEcommerce.DomainLayer.Models.Order;

namespace MyEcommerce.DomainLayer.Interfaces.Repositories.Order
{
	public interface IOrderHeaderRepository : IGenericRepository<OrderHeader>
	{
		void Update(OrderHeader orderHeader);
		Task UpdateOrderStatusAsync (int id, string? OrderStatus,string? PaymentStatus);
		Task<string> TopPurchasedBuyerAsync();
	}
}
