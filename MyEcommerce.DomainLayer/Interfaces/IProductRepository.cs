using MyEcommerce.DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEcommerce.DomainLayer.Interfaces
{
	public interface IProductRepository : IGenericRepository<Product>
	{
		Task UpdateAsync(Product product);
	}
}
