using MyEcommerce.DomainLayer.Models;

namespace MyEcommerce.DomainLayer.Interfaces
{
	public interface IProductRepository : IGenericRepository<Product>
	{
		Task UpdateAsync(Product product);
		Task<IEnumerable<Product>> GetPagedAsync(int skip, int take);
		Task<int> GetCountAsync();

	}
}
