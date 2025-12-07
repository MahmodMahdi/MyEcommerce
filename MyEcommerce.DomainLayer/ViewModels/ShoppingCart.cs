using MyEcommerce.DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyEcommerce.DomainLayer.ViewModels
{
	public class ShoppingCart
	{
		public Product Product { get; set; }
		[Range(1,100,ErrorMessage ="you must enter value between 1 to 100")]
		public int Count { get; set; }
	}
}
