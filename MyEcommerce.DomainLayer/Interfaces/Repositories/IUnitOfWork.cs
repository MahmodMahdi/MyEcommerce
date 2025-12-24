using MyEcommerce.DomainLayer.Interfaces.Repositories.Order;

namespace MyEcommerce.DomainLayer.Interfaces.Repositories
{
	public interface IUnitOfWork:IDisposable
	{
		ICategoryRepository CategoryRepository { get; }
		IProductRepository ProductRepository { get; }
		IShoppingCartRepository ShoppingCartRepository { get; }
		IOrderHeaderRepository OrderHeaderRepository { get; }
		IOrderDetailRepository OrderDetailRepository { get; }
		IApplicationUserRepository ApplicationUserRepository { get; }
		Task BeginTransactionAsync();
		Task CommitTransactionAsync();
		Task RollbackTransactionAsync();
		Task<int> CompleteAsync();
	}
}
