using MyEcommerce.DomainLayer.Models.Order;

namespace MyEcommerce.DomainLayer.Interfaces.Repositories.Order
{
	public interface IOrderDetailRepository:IGenericRepository<OrderDetail>
	{
		void Update(OrderDetail orderDetail);
		Task<string> MostPurchasedProductAsync();

	}
}
