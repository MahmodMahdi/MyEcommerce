using Microsoft.EntityFrameworkCore;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces.Repositories;
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
		public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, string? IncludeProperties = null)
		{
			IQueryable<T> query = _dbset;
			// here if user need using where 
			query.AsNoTracking();
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
			return await query.ToListAsync();
		}

		public async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>>? predicate = null, string? IncludeProperties=null,bool tracked = true)
		{

			IQueryable<T> query = _dbset;
			if (!tracked)
			{
				query = query.AsNoTracking();
			}
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
			return await query.FirstOrDefaultAsync();
		}
		public async Task AddAsync(T entity)
		{
			await _dbset.AddAsync(entity);
		}
		public void Update(T entity)
		{
			 _dbset.Update(entity);
		}
		public async Task AddRangeAsync(IEnumerable<T> entity)
		{
			await _dbset.AddRangeAsync(entity);
		}
		public void Remove(T entity)
		{
			 _dbset.Remove(entity);
		}

		public void RemoveRange(IEnumerable<T> entities)
		{
			_dbset.RemoveRange(entities);
		}
		//public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
		//{
		//	if (predicate == null)
		//		return await _dbset.CountAsync();
		//	return await _dbset.CountAsync(predicate);
		//}
		public async Task<int> CountAsync(Expression<Func<T, bool>>? filter = null)
		{
			IQueryable<T> query = _dbset;
			if (filter != null)
			{
				query = query.Where(filter);
			}
			return await query.CountAsync();
		}
	}
}
