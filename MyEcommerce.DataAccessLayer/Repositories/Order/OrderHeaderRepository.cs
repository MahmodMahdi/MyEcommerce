using Microsoft.EntityFrameworkCore;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces;
using MyEcommerce.DomainLayer.Interfaces.Order;
using MyEcommerce.DomainLayer.Models.Order;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyEcommerce.DataAccessLayer.Repositories.Order
{
	public class OrderHeaderRepository : GenericRepository<OrderHeader>, IOrderHeaderRepository
	{
		private readonly ApplicationDbContext _context;
		public OrderHeaderRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}

		public void Update(OrderHeader orderHeader)
		{
			_context.OrderHeaders.Update(orderHeader);
		}

		public void UpdateOrderStatus(int id, string? OrderStatus, string? PaymentStatus)
		{
			var orderFromDb =_context.OrderHeaders.FirstOrDefault(o => o.Id == id);
			if (orderFromDb != null)
			{
				orderFromDb.OrderStatus = OrderStatus;
				orderFromDb.PaymentDate = DateTime.Now;
				if (PaymentStatus != null)
				{
					orderFromDb.PaymentStatus = PaymentStatus;
				}
			}
		}
		public string MostPurchasedUser()
		{
			var TopBuyer = _context.OrderHeaders
				.GroupBy(P => P.ApplicationUser.Name)
				.Select(g => new
				{
					UserName = g.Key,
					count = g.Count()
				})
				.OrderByDescending(x => x.count)
				.Select(x => x.UserName)
				.FirstOrDefault();
			return TopBuyer.ToString();

			/// Another way with join
			//var TopBuyer = _context.OrderHeaders
			//	.GroupBy(P => P.ApplicationUser.Name)
			//	.Select(g => new 
			//	{
			//		UserId = g.Key,
			//		count = g.Count()
			//	})
			//	.OrderByDescending(x => x.count)
			//	.Join(_context.ApplicationUsers,
			//	t=>t.UserId,
			//	u=>u.Id,
			//	(t,u)=> new
			//	{
			//		UserName = u.Name,
			//		Count = u.Id
			//	})
			//	.Select(x=>x.UserName)
			//	.FirstOrDefault();
			//return TopBuyer.ToString();
		}
	}
}
