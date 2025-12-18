using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyEcommerce.DataAccessLayer.Data;
using System.Security.Claims;
using Utilities;

namespace MyEcommerce.PresentationLayer.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles ="Admin")]
	public class UsersController : Controller
	{

		private readonly ApplicationDbContext _context;
		public UsersController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(int pageNumber =1)
		{
			if (pageNumber < 1) pageNumber = 1;

			int pageSize = 10; // عدد المستخدمين في كل صفحة
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			// 1. حساب العدد الإجمالي للمستخدمين (باستثناء الحالي)
			int totalUsers = await _context.ApplicationUsers
				.Where(u => u.Id != userId)
				.CountAsync();

			int numberOfPages = (int)Math.Ceiling((double)totalUsers / pageSize);
			int numberOfItemToSkip = (pageNumber - 1) * pageSize;

			var users = await _context.ApplicationUsers
				.Where(u => u.Id != userId)
				.OrderBy(u => u.UserName)
				.Skip(numberOfItemToSkip)
				.Take(pageSize)
				.ToListAsync();
			ViewBag.PageNo = pageNumber;
			ViewBag.NoOfPages = numberOfPages;

			return View(users);
		}
		public IActionResult LockUnlock(string? id)
		{
			var user = _context.ApplicationUsers.FirstOrDefault(u => u.Id==id);
			if (user == null)
			{
				return NotFound();
			}
			if (user.LockoutEnd == null || user.LockoutEnd < DateTime.Now)
			{
				user.LockoutEnd = DateTime.Now.AddYears(1);
			}
			else
			{
				user.LockoutEnd = DateTime.Now;
			}
			_context.SaveChanges();
		    return RedirectToAction("Index", "Users", new {area ="Admin"});
		}
	}
}
