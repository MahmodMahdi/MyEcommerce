using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

		public IActionResult Index()
		{
			var claimsIdentity =(ClaimsIdentity)User.Identity; // here i want to bring claims
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier); // here i filter claim of users
			string userId = claim.Value; // here i bring user by his id
			var Users = _context.ApplicationUsers.Where(u=>u.Id!= userId).ToList(); // here i bring all users except me
			return View(Users);
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
