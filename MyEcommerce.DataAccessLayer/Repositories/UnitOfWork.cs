using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEcommerce.DataAccessLayer.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ApplicationDbContext _context;
		public UnitOfWork(ApplicationDbContext context)
		{
			_context = context;
			CategoryRepository = new CategoryRepository(context);
		}

		public ICategoryRepository CategoryRepository {  get;private set; }

		public int complete()
		{
			return _context.SaveChanges();
		}

		public void Dispose()
		{
			 _context?.Dispose();
		}
	}
}
