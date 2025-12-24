using Microsoft.EntityFrameworkCore.Storage;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DataAccessLayer.Repositories.Order;
using MyEcommerce.DomainLayer.Interfaces.Repositories;
using MyEcommerce.DomainLayer.Interfaces.Repositories.Order;

namespace MyEcommerce.DataAccessLayer.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ApplicationDbContext _context;
		private  IDbContextTransaction _transaction;
		public ICategoryRepository CategoryRepository { get; private set; }
		public IProductRepository ProductRepository { get; private set; }
		public IShoppingCartRepository ShoppingCartRepository { get; private set; }
		public IOrderHeaderRepository OrderHeaderRepository { get; private set; }
		public IOrderDetailRepository OrderDetailRepository { get; private set; }
		public IApplicationUserRepository ApplicationUserRepository { get; private set; }

		public UnitOfWork(ApplicationDbContext context)
		{
			_context = context;
			CategoryRepository = new CategoryRepository(context);
			ProductRepository = new ProductRepository(context);
			ShoppingCartRepository = new ShoppingCartRepository(context);
			OrderHeaderRepository = new OrderHeaderRepository(context);
			OrderDetailRepository = new OrderDetailRepository(context);
			ApplicationUserRepository = new ApplicationUserRepository(context);		}
		public async Task<int> CompleteAsync()
		{
			return await _context.SaveChangesAsync();
		}
		public void Dispose()
		{
			 _context?.Dispose();
			_transaction?.Dispose(); // تأكد من التخلص من الترانزاكشن أيضاً
		}

		public async Task BeginTransactionAsync()
		{
			_transaction = await _context.Database.BeginTransactionAsync();
		}

		public async Task CommitTransactionAsync()
		{
			try
			{
				if (_transaction != null)
				{
					await _transaction.CommitAsync();
				}
			}
			finally
			{
				// نضمن دائماً التخلص من الكائن بعد الانتهاء
				if (_transaction != null)
				{
					await _transaction.DisposeAsync();
					_transaction = null;
				}
			}
		}

		public async Task RollbackTransactionAsync()
		{
			try
			{
				if (_transaction != null)
				{
					await _transaction.RollbackAsync();
				}
			}
			finally
			{
				if (_transaction != null)
				{
					await _transaction.DisposeAsync();
					_transaction = null;
				}
			}
		}
	}
}
