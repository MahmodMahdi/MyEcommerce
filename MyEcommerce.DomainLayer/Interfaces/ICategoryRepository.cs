using MyEcommerce.DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEcommerce.DomainLayer.Interfaces
{
	public interface ICategoryRepository: IGenericRepository<Category>
	{
		void Update(Category category);
	}
}
