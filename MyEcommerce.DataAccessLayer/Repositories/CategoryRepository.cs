using Microsoft.EntityFrameworkCore;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces.Repositories;
using MyEcommerce.DomainLayer.Models;
using System.Threading.Tasks;

namespace MyEcommerce.DataAccessLayer.Repositories
{
	public class CategoryRepository:GenericRepository<Category>,ICategoryRepository
	{
		private readonly ApplicationDbContext _context;
		public CategoryRepository(ApplicationDbContext context): base(context)
		{
			_context = context;
		}
	}
}
