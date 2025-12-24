using MyEcommerce.DomainLayer.Models;

namespace MyEcommerce.DomainLayer.Interfaces.Repositories
{
	public interface IShoppingCartRepository:IGenericRepository<ShoppingCart>
	{
		int IncreaseCount (ShoppingCart shoppingCart,int count);
		int DecreaseCount (ShoppingCart shoppingCart,int count);
	}
}
