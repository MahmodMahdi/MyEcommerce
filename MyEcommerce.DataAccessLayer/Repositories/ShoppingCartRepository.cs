using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.Models;

namespace MyEcommerce.DataAccessLayer.Repositories
{
	public class ShoppingCartRepository : GenericRepository<ShoppingCart>, IShoppingCartRepository
	{
		private readonly ApplicationDbContext _context;
		public ShoppingCartRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
		public int IncreaseCount(ShoppingCart shoppingCart, int count)
		{
			shoppingCart.Count += count;
			return shoppingCart.Count;
		}

		public int DecreaseCount(ShoppingCart shoppingCart, int count)
		{
			shoppingCart.Count -= count;
			return shoppingCart.Count;
		}

		
	}
}
