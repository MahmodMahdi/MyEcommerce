using MyEcommerce.DomainLayer.Models.Order;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEcommerce.DomainLayer.Interfaces.Order
{
	public interface IOrderHeaderRepository : IGenericRepository<OrderHeader>
	{
		Task UpdateAsync(OrderHeader orderHeader);
		Task UpdateOrderStatusAsync (int id, string? OrderStatus,string? PaymentStatus);
		Task<string> TopPurchasedBuyerAsync();
	}
}
