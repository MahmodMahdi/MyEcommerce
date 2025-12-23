namespace MyEcommerce.ApplicationLayer.ViewModels
{
	public class OrderEmailDto
	{
		public string Email { get; set; }
		public string Name { get; set; }
		public int OrderId { get; set; }
		public decimal Total { get; set; }
		public string? Carrier { get; set; }
		public string? TrackingNumber { get; set; }
	}
}
