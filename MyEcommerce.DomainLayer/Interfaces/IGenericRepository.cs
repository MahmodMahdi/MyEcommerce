using System.Linq.Expressions;

namespace MyEcommerce.DomainLayer.Interfaces
{
	public interface IGenericRepository<T> where T : class
	{
		// here i made this because if i want to use where or include with query
		Task<IEnumerable<T>> GetAllAsync(Expression<Func<T,bool>>? predicate=null,string? IncludeProperties=null);
		Task<T> GetByIdAsync(Expression<Func<T, bool>>? predicate=null, string? IncludeProperties=null);
		Task AddAsync (T entity);
		Task RemoveAsync (T entity);
		Task RemoveRangeAsync(IEnumerable<T> entities);

	}
}
