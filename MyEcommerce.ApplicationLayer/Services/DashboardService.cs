using MyEcommerce.ApplicationLayer.Interfaces.Services;
using MyEcommerce.ApplicationLayer.ViewModels;
using MyEcommerce.DomainLayer.Interfaces.Repositories;
using MyEcommerce.DomainLayer.Models;
using Utilities;

namespace MyEcommerce.ApplicationLayer.Services
{
	public class DashboardService : IDashboardService
	{
		private readonly IUnitOfWork _unitOfWork;
		public DashboardService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<DashboardViewModel> GetDashboardDataAsync()
		{
			var MostExistProduct = await _unitOfWork.ProductRepository.GetMostExistItem();
			var MostPurchasedProduct = await _unitOfWork.OrderDetailRepository.MostPurchasedProductAsync();
			var MostPurchasedBuyer = await _unitOfWork.OrderHeaderRepository.TopPurchasedBuyerAsync();
			var allOrders = await _unitOfWork.OrderHeaderRepository.CountAsync();

			return new DashboardViewModel
			{
				MostExistProduct = MostExistProduct,
				MostPurchasedBuyer = MostPurchasedBuyer,
				MostPurchasedProduct = MostPurchasedProduct,

				ApprovedOrders = await _unitOfWork.OrderHeaderRepository.CountAsync(A => A.OrderStatus == Helper.Approve),
				ShippedOrders = await _unitOfWork.OrderHeaderRepository.CountAsync(A => A.OrderStatus == Helper.Shipped),
				PendingOrders = await _unitOfWork.OrderHeaderRepository.CountAsync(A => A.OrderStatus == Helper.Pending),
				TotalOrders = allOrders,

				TotalUsers = await _unitOfWork.ApplicationUserRepository.CountAsync(u=>u.Email != "admin@ShopSphere.com"),
				TotalProducts = await _unitOfWork.ProductRepository.CountAsync(),
				TotalCategories = await _unitOfWork.CategoryRepository.CountAsync(),
				UsersLockedAccount = await _unitOfWork.ApplicationUserRepository.LockedUserAcccount()
			};
		}
	}
}
