using Microsoft.EntityFrameworkCore;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.Models;

namespace MyEcommerce.DataAccessLayer.Repositories
{
	public class CategoryRepository:GenericRepository<Category>,ICategoryRepository
	{
		private readonly ApplicationDbContext _context;
		public CategoryRepository(ApplicationDbContext context): base(context)
		{
			_context = context;
		}

		public async Task UpdateAsync(Category category)
		{
			var categoryItem =await _context.Categories.FirstOrDefaultAsync(c=>c.Id == category.Id);
			if (categoryItem != null)
			{
				categoryItem.Name = category.Name;
				categoryItem.Description = category.Description;
				categoryItem.CreatedTime = DateTime.Now;
			}

		}
	}
}
