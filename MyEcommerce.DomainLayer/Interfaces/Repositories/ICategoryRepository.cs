using MyEcommerce.DomainLayer.Models;

namespace MyEcommerce.DomainLayer.Interfaces.Repositories
{
	public interface ICategoryRepository: IGenericRepository<Category>
	{
		void Update(Category category);
	}
}
