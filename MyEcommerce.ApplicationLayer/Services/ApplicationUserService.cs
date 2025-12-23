using AutoMapper;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MyEcommerce.ApplicationLayer.Interfaces.Services;
using MyEcommerce.ApplicationLayer.ViewModels;
using MyEcommerce.DataAccessLayer.Data;

namespace MyEcommerce.ApplicationLayer.Services
{
	public class ApplicationUserService : IApplicationUserService
	{
		private readonly ApplicationDbContext _context;
		private readonly IEmailSender _emailSender;
		private readonly IMapper _mapper;
		public ApplicationUserService(ApplicationDbContext context,
			IMapper mapper,
			IEmailSender emailSender)
		{
			_context = context;
			_mapper = mapper;
			_emailSender = emailSender;
		}
		public async Task<(IEnumerable<UserViewModel> Users,int totalPages)> GetPaginatedUsersAsync (string userId, int pageNumber ,int pageSize)
		{
			int totalUsers = await _context.ApplicationUsers
				.AsNoTracking()
				.Where(u => u.Id != userId)
				.CountAsync();

			int totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
			int numberOfItemToSkip = (pageNumber - 1) * pageSize;

			var users = await _context.ApplicationUsers
				.AsNoTracking()
				.Where(u => u.Id != userId)
				.OrderBy(u => u.UserName)
				.Skip(numberOfItemToSkip)
				.Take(pageSize)
				.ToListAsync();
			var usersViewMode = _mapper.Map<IEnumerable<UserViewModel>>(users);
			return (usersViewMode,totalPages);
		}

		public async Task<UserViewModel> LockUnlock(string userId)
		{
			var user =await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);
			if (user == null) return null;
			if (user.LockoutEnd == null || user.LockoutEnd < DateTime.UtcNow)
			{
				user.LockoutEnd = DateTime.Now.AddYears(1);
				// إرسال الإيميل
				string reasonMessage = $@"Your account has been disabled by the administrator. 
                             <br/><br/>If you think this is a mistake, please contact us on : support@shopspere.com";

				await _emailSender.SendEmailAsync(user.Email, "Administrative Action", reasonMessage);
			}
			else
			{
				user.LockoutEnd = DateTime.UtcNow;
			}
			await _context.SaveChangesAsync();
			return _mapper.Map<UserViewModel>(user);
	
		}
	}
}
