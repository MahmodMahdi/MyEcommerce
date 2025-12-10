using MyEcommerce.DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEcommerce.DomainLayer.Interfaces
{
	public interface IShoppingCartRepository:IGenericRepository<ShoppingCart>
	{
		int IncreaseCount (ShoppingCart shoppingCart,int count);
		int DecreaseCount (ShoppingCart shoppingCart,int count);
	}
}
