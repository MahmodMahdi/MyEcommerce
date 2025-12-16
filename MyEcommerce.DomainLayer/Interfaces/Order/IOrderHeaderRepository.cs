using MyEcommerce.DomainLayer.Models.Order;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEcommerce.DomainLayer.Interfaces.Order
{
	public interface IOrderHeaderRepository : IGenericRepository<OrderHeader>
	{
		void Update(OrderHeader orderHeader);
		void UpdateOrderStatus (int id, string? OrderStatus,string? PaymentStatus);
		string MostPurchasedUser();
	}
}
