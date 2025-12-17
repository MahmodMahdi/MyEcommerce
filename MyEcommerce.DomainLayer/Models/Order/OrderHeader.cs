using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MyEcommerce.DomainLayer.Models.Order
{
	public class OrderHeader
	{
		// details of order
		public int Id { get; set; }
		public DateTime OrderDate { get; set; }
		public DateTime ShippingDate { get; set; }
		public decimal TotalPrice { get; set; }
		public string? OrderStatus { get; set; }
		public string? PaymentStatus { get; set; }
		public string? TrackingNumber { get; set; }
		public string? Carrior { get; set; }
		public DateTime PaymentDate { get; set; }

		// Props of Stripe
		public string? SessionId { get; set; }
		public string? PaymentIntentId { get; set; }

		// Date of User 
		public string Name { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string PhoneNumber { get; set; }

		public string ApplicationUserId { get; set; }
		[ForeignKey(nameof(ApplicationUserId))]
		[ValidateNever]
		public ApplicationUser ApplicationUser { get; set; }
	}
}
