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

		public void Update(Product product)
		{
			var productItem = _context.Products.FirstOrDefault(x=>x.Id == product.Id);
			if (productItem != null)
			{
				productItem.Name = product.Name;
				productItem.Description = product.Description;
				productItem.Price = product.Price;
				productItem.Image = product.Image;
				productItem.CreatedAt = DateTime.Now;
			}
		}
	}
}
