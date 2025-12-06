using MyEcommerce.DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEcommerce.DomainLayer.Interfaces
{
	public interface IProductRepository : IGenericRepository<Product>
	{
		void Update(Product product);
	}
}
