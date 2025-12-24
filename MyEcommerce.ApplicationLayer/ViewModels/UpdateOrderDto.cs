namespace MyEcommerce.ApplicationLayer.ViewModels
{
	public class UpdateOrderDto
	{
		public int OrderId { get; set; }
		public string Name { get; set; }
		public string PhoneNumber { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string? Carrior { get; set; }
		public string? TrackingNumber { get; set; }
	}
}
