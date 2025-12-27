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
		public async Task<PaginatedResultViewModel<UserViewModel>> GetPaginatedUsersAsync (string userId, int pageNumber ,int pageSize)
		{
			var query = _context.ApplicationUsers
				.AsNoTracking()
				.Where(u => u.Id != userId);
			int totalUsers = await query.CountAsync();
			int totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
			int numberOfItemToSkip = (pageNumber - 1) * pageSize;

			var users =await query.OrderBy(u => u.UserName)
				.Skip(numberOfItemToSkip)
				.Take(pageSize)
				.ToListAsync();
			var MappedUser = _mapper.Map<IEnumerable<UserViewModel>>(users);
			var result = new PaginatedResultViewModel<UserViewModel>
			{
				Items = MappedUser,
				CurrentPage = pageNumber,
				TotalPages = totalPages
			};
			return result;
		}

		public async Task<UserViewModel> LockUnlock(string userId)
		{
			var user =await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);
			if (user == null) return null;
			if (user.LockoutEnd == null || user.LockoutEnd < DateTime.UtcNow)
			{
				user.LockoutEnd = DateTime.Now.AddYears(1);
				// إرسال الإيميل
				string reasonMessage = $@"تم قفل حسابك بواسطة الأدمن 
                             <br/><br/>لو تعتقد أن هذا خطأ , أرجوك تواصل مع الدعم الفنى. : support@shopspere.com";

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
