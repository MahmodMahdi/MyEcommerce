using MyEcommerce.DomainLayer.Models.Order;

namespace MyEcommerce.ApplicationLayer.ViewModels
{
	public class OrderViewModel
	{
		public OrderHeader OrderHeader { get; set; }
		public IEnumerable<OrderDetail> OrderDetails { get; set; }

	}
}
