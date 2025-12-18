using Microsoft.EntityFrameworkCore;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEcommerce.DataAccessLayer.Repositories
{
	public class ProductRepository:GenericRepository<Product>, IProductRepository
	{
		private readonly ApplicationDbContext _context;

		public ProductRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task UpdateAsync(Product product)
		{
			var productItem =await _context.Products.FirstOrDefaultAsync(x=>x.Id == product.Id);
			if (productItem != null)
			{
				productItem.Name = product.Name;
				productItem.Description = product.Description;
				productItem.Price = product.Price;
				productItem.StockQuantity = product.StockQuantity;
				productItem.Image = product.Image;
				productItem.CategoryId = product.CategoryId;
			}
		}
		public async Task<IEnumerable<Product>> GetPagedAsync(int skip, int take)
		{
			return await _context.Products
				.Skip(skip)
				.Take(take)
				.ToListAsync();
		}

		public async Task<int> GetCountAsync()
		{
			return await _context.Products.CountAsync();
		}
	}
}
