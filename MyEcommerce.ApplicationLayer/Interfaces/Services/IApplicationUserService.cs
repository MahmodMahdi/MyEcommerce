using MyEcommerce.ApplicationLayer.ViewModels;

namespace MyEcommerce.ApplicationLayer.Interfaces.Services
{
	public interface IApplicationUserService
	{
		Task<PaginatedResultViewModel<UserViewModel>> GetPaginatedUsersAsync(string userId, int pageNumber, int pageSize);
		Task<UserViewModel> LockUnlock(string userId);
	}
}
