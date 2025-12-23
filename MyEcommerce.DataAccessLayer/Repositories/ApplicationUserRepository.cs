using Microsoft.EntityFrameworkCore;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces.Repositories;
using MyEcommerce.DomainLayer.Models;

namespace MyEcommerce.DataAccessLayer.Repositories
{
	public class ApplicationUserRepository : GenericRepository<ApplicationUser>, IApplicationUserRepository
	{
		private readonly ApplicationDbContext _context;
		public ApplicationUserRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<int> LockedUserAcccount()
		{
			return await _context.ApplicationUsers.CountAsync(u => u.LockoutEnd > DateTime.Now);		
		}
	}
}
