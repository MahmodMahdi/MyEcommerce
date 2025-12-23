namespace MyEcommerce.ApplicationLayer.ViewModels
{
	public class DashboardViewModel
	{
		public int TotalOrders { get; set; }
		public int TotalCategories { get; set; }
		public int TotalUsers { get; set; }
		public int TotalProducts { get; set; }
		public int ApprovedOrders { get; set; }
		public int ShippedOrders { get; set; }
		public int PendingOrders { get; set; }
		public int UsersLockedAccount { get; set; }
	    public string MostExistProduct { get; set; }
		public string LessExistProduct { get; set; }
		public string MostPurchasedProduct { get; set; }
		public string MostPurchasedBuyer { get; set; }
	}
}
