using MyEcommerce.DomainLayer.Models.Order;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEcommerce.DomainLayer.Interfaces.Order
{
	public interface IOrderDetailRepository:IGenericRepository<OrderDetail>
	{
		void Update(OrderDetail orderDetail);
	}
}
