using Microsoft.EntityFrameworkCore;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces.Order;
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

		public async Task UpdateAsync(OrderHeader orderHeader)
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
			var TopBuyer = await _context.OrderHeaders
				.GroupBy(P => P.ApplicationUser.Name)
				.Select(g => new
				{
					UserName = g.Key,
					count = g.Count()
				})
				.OrderByDescending(x => x.count)
				.Select(x => x.UserName)
				.FirstOrDefaultAsync();
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
