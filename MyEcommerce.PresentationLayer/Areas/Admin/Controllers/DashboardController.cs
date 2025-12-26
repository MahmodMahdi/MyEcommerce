using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEcommerce.ApplicationLayer.Interfaces.Services;
using Utilities;

namespace MyEcommerce.PresentationLayer.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles =Helper.AdminRole)]
	public class DashboardController : Controller
	{
		private readonly IDashboardService _dashboardService;
		public DashboardController(IDashboardService dashboardService)
		{
			_dashboardService = dashboardService;
		}

		public async Task<IActionResult> Index()
		{
			var dashboard = await _dashboardService.GetDashboardDataAsync();

			return View(dashboard);
		}
	}
}
