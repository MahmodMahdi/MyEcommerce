using MyEcommerce.DomainLayer.Models.Order;

namespace MyEcommerce.DomainLayer.Interfaces.Order
{
	public interface IOrderDetailRepository:IGenericRepository<OrderDetail>
	{
		Task UpdateAsync(OrderDetail orderDetail);
		Task<string> MostPurchasedProductAsync();

	}
}
