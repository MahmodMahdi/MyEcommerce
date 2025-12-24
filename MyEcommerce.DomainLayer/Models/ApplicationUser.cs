using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyEcommerce.DomainLayer.Models
{
	public class ApplicationUser:IdentityUser
	{
		[Required]
		public string Name { get; set; }
		public string? Address { get; set; }
		public string? City { get; set; }
	}
}
