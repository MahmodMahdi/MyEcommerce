using MyEcommerce.DomainLayer.Models;

namespace MyEcommerce.DomainLayer.Interfaces.Repositories
{
	public interface IApplicationUserRepository:IGenericRepository<ApplicationUser>
	{
		Task<int> LockedUserAcccount();
	}
}
