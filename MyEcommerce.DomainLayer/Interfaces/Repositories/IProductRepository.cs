using MyEcommerce.DomainLayer.Models;

namespace MyEcommerce.DomainLayer.Interfaces.Repositories
{
	public interface IProductRepository : IGenericRepository<Product>
	{
		Task<IEnumerable<Product>> GetPagedAsync(int skip, int take);
		Task<int> GetCountAsync();
		Task<string> GetMostExistItem();
	}
}
