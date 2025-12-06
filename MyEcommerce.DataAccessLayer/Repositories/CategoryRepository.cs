using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEcommerce.DataAccessLayer.Repositories
{
	public class CategoryRepository:GenericRepository<Category>,ICategoryRepository
	{
		private readonly ApplicationDbContext _context;
		public CategoryRepository(ApplicationDbContext context): base(context)
		{
			_context = context;
		}

		public void Update(Category category)
		{
			var categoryItem = _context.Categories.FirstOrDefault(c=>c.Id == category.Id);
			if (categoryItem != null)
			{
				categoryItem.Name = category.Name;
				categoryItem.Description = category.Description;
				categoryItem.CreatedTime = DateTime.Now;
			}

		}
	}
}
