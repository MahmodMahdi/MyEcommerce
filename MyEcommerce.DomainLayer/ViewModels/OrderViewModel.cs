using MyEcommerce.DomainLayer.Models.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEcommerce.DomainLayer.ViewModels
{
	public class OrderViewModel
	{
		public OrderHeader OrderHeader { get; set; }
		public IEnumerable<OrderDetail> OrderDetails { get; set; }

	}
}
