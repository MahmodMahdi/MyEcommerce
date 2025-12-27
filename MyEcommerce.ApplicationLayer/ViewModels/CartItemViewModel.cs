using MyEcommerce.DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEcommerce.ApplicationLayer.ViewModels
{
	public class CartItemViewModel
	{
		public int ProductId { get; set; }
		public int Count { get; set; }
		public ProductViewModel? Product { get; set; } // نستخدم ProductViewModel هنا أيضاً
		public string? ApplicationUserId { get; set; }
	}
}
