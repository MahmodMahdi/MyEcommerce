using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces.Order;
using MyEcommerce.DomainLayer.Models.Order;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEcommerce.DataAccessLayer.Repositories.Order
{
	public class OrderDetailRepository : GenericRepository<OrderDetail>, IOrderDetailRepository
	{
		private readonly ApplicationDbContext _context;
		public OrderDetailRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
		public void Update(OrderDetail orderDetail)
		{
			_context.OrderDetails.Update(orderDetail);
		}

	}
}
