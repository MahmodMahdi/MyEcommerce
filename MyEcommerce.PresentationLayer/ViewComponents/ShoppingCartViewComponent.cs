using Microsoft.AspNetCore.Mvc;
using MyEcommerce.DomainLayer.Interfaces;
using System.Security.Claims;
using Utilities;

namespace MyEcommerce.PresentationLayer.ViewComponents
{
	public class ShoppingCartViewComponent:ViewComponent
	{
		private readonly IUnitOfWork _unitOfWork;
		public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public async Task<IViewComponentResult> InvokeAsync()
		{
			var claimIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
			// is authenticated
			if (claim != null)
			{
				// here when user has an email previously and has log in before (so he has session) 
				if (HttpContext.Session.GetInt32(Helper.SessionKey) != null)
				{
					// here get the sessionKey
					return View(HttpContext.Session.GetInt32(Helper.SessionKey));
				}
				else
				{
					// here when any user doesn't have any session or didn't log in before
					// ( it will set session and bring the count of sessions (number of products in cart)
					HttpContext.Session.SetInt32(Helper.SessionKey, _unitOfWork.ShoppingCartRepository
						.GetAll(s => s.ApplicationUserId == claim.Value)
						.ToList()
						.Count());
					return View(HttpContext.Session.GetInt32(Helper.SessionKey));
				}
			}
			else
			{
				HttpContext.Session.Clear();
				return View(0);
			}
		}
	}
}
