using System.ComponentModel.DataAnnotations;

namespace MyEcommerce.ApplicationLayer.ViewModels
{
	public class UpdateOrderDto
	{
		public int OrderId { get; set; }
		public string Name { get; set; }
		[RegularExpression("01[0125][0-9]{8}", ErrorMessage = "Enter Valid Phone Number.")]
		public string PhoneNumber { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string? Carrior { get; set; }
		[MinLength(12, ErrorMessage = "Tracking must be greater than 11 letters"), MaxLength(20, ErrorMessage = "Tracking must be less than 21 letters")]
		public string? TrackingNumber { get; set; }
	}
}
