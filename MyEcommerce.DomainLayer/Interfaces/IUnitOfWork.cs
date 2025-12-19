using MyEcommerce.DomainLayer.Interfaces.Order;

namespace MyEcommerce.DomainLayer.Interfaces
{
	public interface IUnitOfWork:IDisposable
	{
		ICategoryRepository CategoryRepository { get; }
		IProductRepository ProductRepository { get; }
		IShoppingCartRepository ShoppingCartRepository { get; }
		IOrderHeaderRepository OrderHeaderRepository { get; }
		IOrderDetailRepository OrderDetailRepository { get; }
		IApplicationUserRepository ApplicationUserRepository { get; }
		Task<int> CompleteAsync();
	}
}
