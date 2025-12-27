using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEcommerce.ApplicationLayer.Interfaces.Services;
using System.Security.Claims;
using Utilities;

namespace MyEcommerce.PresentationLayer.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = Helper.AdminRole)]
	public class UsersController : Controller
	{
		private readonly IApplicationUserService _applicationUserService;
		public UsersController(IApplicationUserService applicationUserService)
		{
			_applicationUserService = applicationUserService;
		}

		public async Task<IActionResult> Index(int pageNumber = 1)
		{
			if (pageNumber < 1) pageNumber = 1;

			var pageSize = 10;
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			
			var result = await _applicationUserService.GetPaginatedUsersAsync(userId, pageNumber, pageSize);
			return View(result);
		}

		public async Task<IActionResult> LockUnlock(string? id)
		{
			var user = await _applicationUserService.LockUnlock(id);
			if (user == null)
			{
				return NotFound();
			}
		
		    return RedirectToAction("Index", "Users", new {area ="Admin"});
		}
	}
}
