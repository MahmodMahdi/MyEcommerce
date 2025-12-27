using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyEcommerce.DomainLayer.Models.Order
{
	public class OrderDetail
	{
		public int Id { get; set; }
		public decimal Price { get; set; }
		public int Count { get; set; }
		public decimal Discount { get; set; }
		public decimal AcualPrice => Price - ( Price * (Discount / 100m));
		public decimal TotolLinePrice => AcualPrice * Count;
		public int ProductId { get; set; }
		[ForeignKey(nameof(ProductId))]
		[ValidateNever]
		public Product Product { get; set; }

		public int OrderId { get; set; }
		[ForeignKey(nameof(OrderId))]
		[ValidateNever]
		public OrderHeader OrderHeader { get; set; }
	}
}
