using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyEcommerce.DomainLayer.Models
{
	public class ApplicationUser:IdentityUser
	{
		[Required]
		public string Name { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
	}
}
