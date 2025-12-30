using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEcommerce.ApplicationLayer.Interfaces.Services;
using Utilities;

namespace MyEcommerce.PresentationLayer.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = Helper.AdminRole + "," + Helper.EditorRole)]
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
			if(dashboard != null)
			return View(dashboard);
			else return View();
		}
	}
}
