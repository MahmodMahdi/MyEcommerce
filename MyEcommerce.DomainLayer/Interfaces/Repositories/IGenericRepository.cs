using System.Linq.Expressions;

namespace MyEcommerce.DomainLayer.Interfaces.Repositories
{
	public interface IGenericRepository<T> where T : class
	{
		// here i made this because if i want to use where or include with query
		Task<IEnumerable<T>> GetAllAsync(Expression<Func<T,bool>>? predicate=null,string? IncludeProperties=null);
		Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>>? predicate=null, string? IncludeProperties=null,bool tracked = true);
	    Task AddAsync(T entity);
		Task AddRangeAsync (IEnumerable<T> entity);
		Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);
		void Update (T entity);
		void Remove (T entity);
		void RemoveRange(IEnumerable<T> entities);
	}
}
