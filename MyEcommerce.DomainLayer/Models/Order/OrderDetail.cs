using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEcommerce.DomainLayer.Models.Order
{
	public class OrderDetail
	{
		public int Id { get; set; }
		public decimal Price { get; set; }
		public int Count { get; set; }

		public int ProductId { get; set; }
		[ValidateNever]
		public Product Product { get; set; }

		public int OrderId { get; set; }
		[ValidateNever]
		public OrderHeader OrderHeader { get; set; }
	}
}
