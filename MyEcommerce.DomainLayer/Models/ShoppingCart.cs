using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MyEcommerce.DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MyEcommerce.DomainLayer.Models
{
	public class ShoppingCart
	{
		[Key]
		public int Id { get; set; }
		[Range(1, 100, ErrorMessage = "you must enter value between 1 to 100")]
		public int Count { get; set; }
		public int ProductId { get; set; }
		
		[ValidateNever]
		[ForeignKey("ProductId")]
		public Product Product { get; set; }
		public string ApplicationUserId { get; set; }

		[ValidateNever]
		[ForeignKey("ApplicationUserId")]
		public ApplicationUser ApplicationUser { get; set; }

	}
}
