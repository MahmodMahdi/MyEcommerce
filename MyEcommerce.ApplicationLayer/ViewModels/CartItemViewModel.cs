namespace MyEcommerce.ApplicationLayer.ViewModels
{
	public class CartItemViewModel
	{
		public int ProductId { get; set; }
		public int Count { get; set; }
		public ProductViewModel? Product { get; set; } 
		public string? ApplicationUserId { get; set; }
	}
}
