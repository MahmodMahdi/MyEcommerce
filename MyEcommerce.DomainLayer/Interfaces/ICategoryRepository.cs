using MyEcommerce.DomainLayer.Models;

namespace MyEcommerce.DomainLayer.Interfaces
{
	public interface ICategoryRepository: IGenericRepository<Category>
	{
		Task UpdateAsync(Category category);
	}
}
