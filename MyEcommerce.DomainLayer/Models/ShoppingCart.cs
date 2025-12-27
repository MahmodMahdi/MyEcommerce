using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyEcommerce.DomainLayer.Models
{
	public class ShoppingCart
	{
		[Key]
		public int Id { get; set; }
		public int Count { get; set; }
		public int ProductId { get; set; }
		
		[ValidateNever]
		public Product Product { get; set; }
		public string ApplicationUserId { get; set; }

		[ValidateNever]
		public ApplicationUser ApplicationUser { get; set; }

	}
}
