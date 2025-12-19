using MyEcommerce.DomainLayer.Models.Order;

namespace MyEcommerce.DomainLayer.ViewModels
{
	public class OrderViewModel
	{
		public OrderHeader OrderHeader { get; set; }
		public IEnumerable<OrderDetail> OrderDetails { get; set; }

	}
}
