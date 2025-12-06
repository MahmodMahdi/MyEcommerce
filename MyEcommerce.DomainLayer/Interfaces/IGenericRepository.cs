using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MyEcommerce.DomainLayer.Interfaces
{
	public interface IGenericRepository<T> where T : class
	{
		// here i made this because if i want to use where or include with query
		IEnumerable<T> GetAll(Expression<Func<T,bool>>? predicate=null,string? Includeword=null);
		T GetById(Expression<Func<T, bool>> predicate, string? Includeword);
		void Add (T entity);
		void Remove (T entity);
		void RemoveRange(IEnumerable<T> entities);

	}
}
