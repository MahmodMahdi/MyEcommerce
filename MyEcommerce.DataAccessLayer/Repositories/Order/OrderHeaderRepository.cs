using Microsoft.EntityFrameworkCore;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces.Repositories.Order;
using MyEcommerce.DomainLayer.Models.Order;
using Utilities;

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
			var orderFromDb = await _context.OrderHeaders.FirstOrDefaultAsync(o => o.Id == id);
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
				.Where(o => o.OrderStatus == Helper.Approve)
				.AsNoTracking()
				.GroupBy(o => o.ApplicationUserId)
				.Select(g => new
				{
					UserId = g.Key,
					TotalSpent = g.Sum(o => o.TotalPrice)
				})
				.OrderByDescending(x => x.TotalSpent)
				.Select(x => x.UserId)
				.FirstOrDefaultAsync();

			if (topBuyer == null)
				return "No Customers Yet!";

			return (await _context.Users
				.Where(u => u.Id == topBuyer)
				.Select(u => u.Name)
				.FirstOrDefaultAsync())!;
		}
	}
}
