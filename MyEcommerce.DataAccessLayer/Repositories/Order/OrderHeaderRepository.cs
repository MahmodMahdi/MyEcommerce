using Microsoft.EntityFrameworkCore;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces.Repositories.Order;
using MyEcommerce.DomainLayer.Models.Order;

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

		public async Task UpdateOrderStatusAsync(int id, string? OrderStatus, string? PaymentStatus)
		{
			var orderFromDb =await _context.OrderHeaders.FirstOrDefaultAsync(o => o.Id == id);
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
		public async Task<string> TopPurchasedBuyerAsync()
		{
			var topBuyer = await _context.OrderHeaders
				.AsNoTracking()
				.GroupBy(o => new { o.ApplicationUserId, o.Name })
				.Select(g => new
				{
					UserName = g.Key.Name,
					TotalSpent = g.Sum(o=>o.TotalPrice)
				})
				.OrderByDescending(x => x.TotalSpent)
				.Select(x => x.UserName)
				.FirstOrDefaultAsync();
			return topBuyer ?? "No Customers Yet!";
			// here it is wrong the same of another way
			//var TopBuyer = await _context.OrderHeaders
			//	.GroupBy(P => P.ApplicationUser.Name)
			//	.Select(g => new
			//	{
			//		UserName = g.Key,
			//		count = g.Count()
			//	})
			//	.OrderByDescending(x => x.count)
			//	.Select(x => x.UserName)
			//	.FirstOrDefaultAsync();
			//return TopBuyer.ToString();
		}
	}
}
