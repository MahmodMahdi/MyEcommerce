using Microsoft.EntityFrameworkCore;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces.Repositories;
using MyEcommerce.DomainLayer.Models;

namespace MyEcommerce.DataAccessLayer.Repositories
{
	public class ProductRepository:GenericRepository<Product>, IProductRepository
	{
		private readonly ApplicationDbContext _context;

		public ProductRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
		public async Task<IEnumerable<Product>> GetPagedAsync(int skip, int take)
		{
			return await _context.Products
				.AsNoTracking()
				.OrderBy(o=>o.Id)
				.Skip(skip)
				.Take(take)
				.ToListAsync();
		}

		public async Task<int> GetCountAsync()
		{
			return await _context.Products.CountAsync();
		}

		public async Task<string> GetMostExistItem()
		{
			var mostExistProduct =  await _context.Products.AsNoTracking().OrderByDescending(p => p.StockQuantity).FirstOrDefaultAsync();
			return mostExistProduct.Name ?? "No Products in Stock";
		}
	}
}
