using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEcommerce.DataAccessLayer.Repositories
{
	public class ApplicationUserRepository : GenericRepository<ApplicationUser>, IApplicationUserRepository
	{
		private readonly ApplicationDbContext _context;
		public ApplicationUserRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
	}
}
