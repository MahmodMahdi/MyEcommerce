using MyEcommerce.ApplicationLayer.ViewModels;

namespace MyEcommerce.ApplicationLayer.Services
{
	public interface ICategoryService
	{ 
		Task<IEnumerable<CategoryViewModel>> GetAllAsync();
		Task<CategoryViewModel> GetFirstOrDefaultAsync(int id);
		Task AddAsync (CategoryViewModel categoryVM);
		Task UpdateAsync(CategoryViewModel categoryVM);
		Task DeleteAsync (int id);
	}
}
