using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DataAccessLayer.Repositories.Order;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.Interfaces.Order;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEcommerce.DataAccessLayer.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ApplicationDbContext _context;
		public ICategoryRepository CategoryRepository { get; private set; }
		public IProductRepository ProductRepository { get; private set; }
		public IShoppingCartRepository ShoppingCartRepository { get; private set; }
		public OrderHeaderRepository OrderHeaderRepository { get; private set; }
		public IOrderDetailRepository OrderDetailRepository { get; private set; }
		public UnitOfWork(ApplicationDbContext context)
		{
			_context = context;
			CategoryRepository = new CategoryRepository(context);
			ProductRepository = new ProductRepository(context);
			ShoppingCartRepository = new ShoppingCartRepository(context);
			OrderHeaderRepository = new OrderHeaderRepository(context);
			OrderDetailRepository = new OrderDetailRepository(context);
		}
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
