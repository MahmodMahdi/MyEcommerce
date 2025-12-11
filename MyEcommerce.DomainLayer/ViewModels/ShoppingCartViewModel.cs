using MyEcommerce.DomainLayer.Models;
using MyEcommerce.DomainLayer.Models.Order;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEcommerce.DomainLayer.ViewModels
{
	public class ShoppingCartViewModel
	{
		public IEnumerable<ShoppingCart> Carts { get; set; }
		public decimal TotalCarts { get; set; }
		public OrderHeader OrderHeader { get; set; }
	}
}
