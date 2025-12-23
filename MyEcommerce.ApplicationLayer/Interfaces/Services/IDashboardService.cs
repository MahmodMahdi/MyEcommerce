using MyEcommerce.ApplicationLayer.ViewModels;

namespace MyEcommerce.ApplicationLayer.Interfaces.Services
{
	public interface IDashboardService
	{
		Task<DashboardViewModel> GetDashboardDataAsync();
	}
}
