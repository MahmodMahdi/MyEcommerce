using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyEcommerce.DomainLayer.Models
{
	public class Product
	{
		public int Id { get; set; }
		[Required]
		public string Name { get; set; }
		public string Description { get; set; }
		public string Image {  get; set; }
		[Required]
		public decimal Price { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		[Required]
		public int CategoryId { get; set; }
		public Category Category { get; set; }
	}
}
