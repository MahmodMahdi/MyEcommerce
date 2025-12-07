using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MyEcommerce.DomainLayer.Models
{
	public class Product
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "*")]
		[MaxLength(30, ErrorMessage = "Name must be less than 29 letters"), MinLength(2, ErrorMessage = "Name must be greater than 2 letters.")]
		public string Name { get; set; }
		public string Description { get; set; }
		[ValidateNever]
		public string Image {  get; set; }
		[Required]
		public decimal Price { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		[DisplayName("Category")]
		[Required(ErrorMessage = "*")]
		public int CategoryId { get; set; }
		[ValidateNever]
		public Category Category { get; set; }
	}
}
