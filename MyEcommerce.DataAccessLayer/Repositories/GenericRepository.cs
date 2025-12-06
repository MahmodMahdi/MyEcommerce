using Microsoft.EntityFrameworkCore;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

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
		public void Add(T entity)
		{
			// Categories.Add(category);
			_dbset.Add(entity);
		}

		public IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate = null, string? Includeword = null)
		{
			IQueryable<T> query = _dbset;
			// here if user need using where 
			if (predicate != null)
			{
				query = query.Where(predicate);
			}
			if(Includeword != null)
			{
				// _context.Products.Include(// here it may be many words not only one word)
				foreach (var item in Includeword.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(item);
				}
			}
			return query.ToList();
		}

		public T GetById(Expression<Func<T, bool>>? predicate = null, string? Includeword=null)
		{

			IQueryable<T> query = _dbset;
			// here if user need using where 
			if (predicate != null)
			{
				query = query.Where(predicate);
			}
			if (Includeword != null)
			{
				// _context.Products.Include(// here it may be many words not only one word)
				foreach (var item in Includeword.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(item);
				}
			}
			return query.SingleOrDefault();
		}

		public void Remove(T entity)
		{
			_dbset.Remove(entity);
		}

		public void RemoveRange(IEnumerable<T> entities)
		{
			_dbset.RemoveRange(entities);
		}
	}
}
