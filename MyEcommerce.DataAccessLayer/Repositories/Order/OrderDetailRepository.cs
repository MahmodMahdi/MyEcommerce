using Microsoft.EntityFrameworkCore;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Interfaces.Repositories.Order;
using MyEcommerce.DomainLayer.Models.Order;
using Utilities;

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
		public async Task<string> MostPurchasedProductAsync()
		{
			// here it is wrong because if one user buy 4 product and onother product sold to each customer say 10 it will choose the first is top
			//var topProduct =await _context.OrderDetails.
			//	OrderByDescending(o => o.Count).
			//	Select(x => x.Product.Name).
			//	FirstOrDefaultAsync();
			//return topProduct;

			/// another way
			var topProduct = await _context.OrderDetails
				.AsNoTracking()
				.Where(x=>x.OrderHeader.OrderStatus == Helper.Approve)
				.GroupBy(P => new { P.ProductId, P.Product.Name })
				.Select(g => new
				{
					ProductName = g.Key.Name,
					TotalSold = g.Sum(x => x.Count)
				})
				.OrderByDescending(x => x.TotalSold)
				.ThenBy(x=>x.ProductName)
				.Select(p => p.ProductName)
				.FirstOrDefaultAsync();
			return topProduct ?? "No Sales Yet";

		}
	}
}
