using Microsoft.EntityFrameworkCore;
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
		public string MostPurchasedProduct()
		{
			var topProduct = _context.OrderDetails.
				OrderByDescending(o => o.Count).
				Select(x => x.Product.Name).
				FirstOrDefault();
			return topProduct;

			/// another way
			//var topProduct = _context.OrderDetails
			//	.Include(p => p.Product)
			//	.GroupBy(P => new { P.ProductId,P.Product.Name })
			//	.Select(g => new 
			//	{
			//		ProductName = g.Key.Name,
			//		TotalSold = g.Sum(x => x.Count)
			//	})
			//	.OrderByDescending(x => x.TotalSold)
			//	.Select(p=>p.ProductName)
			//	.FirstOrDefault();
			//return topProduct.ToString();

		}
	}
}
