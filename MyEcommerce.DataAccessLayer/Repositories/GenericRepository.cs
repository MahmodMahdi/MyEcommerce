using Microsoft.EntityFrameworkCore;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces;
using System.Linq.Expressions;

namespace MyEcommerce.DataAccessLayer.Repositories
{
	public class GenericRepository<T> : IGenericRepository<T> where T : class
	{
		private readonly ApplicationDbContext _context;
		private DbSet<T> _dbset; // to fit with eny Table 
		public GenericRepository(ApplicationDbContext context)
		{
			_context = context;
			_dbset = _context.Set<T>();
		}
		public async Task AddAsync(T entity)
		{
			await _dbset.AddAsync(entity);
		}

		public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, string? IncludeProperties = null)
		{
			IQueryable<T> query = _dbset;
			// here if user need using where 
			if (predicate != null)
			{
				query = query.Where(predicate);
			}
			if(IncludeProperties != null)
			{
				// _context.Products.Include(// here it may be many words not only one word)
				foreach (var item in IncludeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(item);
				}
			}
			return await query.AsNoTracking().ToListAsync();
		}

		public async Task<T> GetByIdAsync(Expression<Func<T, bool>>? predicate = null, string? IncludeProperties=null)
		{

			IQueryable<T> query = _dbset;
			// here if user need using where 
			if (predicate != null)
			{
				query = query.Where(predicate);
			}
			if (IncludeProperties != null)
			{
				// _context.Products.Include(// here it may be many words not only one word)
				foreach (var item in IncludeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(item);
				}
			}
			return await query.SingleOrDefaultAsync();
		}

		public async Task RemoveAsync(T entity)
		{
			 _dbset.Remove(entity);
		}

		public async Task RemoveRangeAsync(IEnumerable<T> entities)
		{
			_dbset.RemoveRange(entities);
		}
	}
}
