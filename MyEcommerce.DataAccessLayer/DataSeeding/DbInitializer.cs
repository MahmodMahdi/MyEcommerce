using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Models;
using Utilities;

namespace MyEcommerce.DataAccessLayer.DataSeeding
{
	public class DbInitializer:IDbInitializer
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly ApplicationDbContext _applicationDbContext;
		private readonly IConfiguration _config;
		private readonly ILogger<DbInitializer> _logger;
		public DbInitializer(
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			ApplicationDbContext applicationDbContext,
			IConfiguration config,
			ILogger<DbInitializer> logger
			)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_applicationDbContext = applicationDbContext;
			_config = config;
			_logger = logger;
		}

		public async Task Initialize()
		{
			//migration
			try
			{
				if ((await _applicationDbContext.Database.GetPendingMigrationsAsync()).Any())
				{
					await _applicationDbContext.Database.MigrateAsync();
				}

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Database migration failed");
				throw;
			}
			string[] roles = { Helper.AdminRole, Helper.EditorRole, Helper.CustomerRole };

			foreach (var role in roles)
			{
				if (!await _roleManager.RoleExistsAsync(role))
				{
					await _roleManager.CreateAsync(new IdentityRole(role));
				}
			}

			//Admin
			var adminEmail = _config["AdminSettings:Email"];
			var adminPassword = _config["AdminSettings:Password"];
			var adminUser = await _userManager.FindByEmailAsync(adminEmail);
			if (adminUser == null)
			{
				adminUser = new ApplicationUser
				{
					UserName = adminEmail,
					Email = adminEmail,
					PhoneNumber = "01212345678",
					Address = "Tanta",
					City = "Gharbia",
					EmailConfirmed = true
				};

				var result = await _userManager.CreateAsync(adminUser, adminPassword);
				if (result.Succeeded)
				{
					await _userManager.AddToRoleAsync(adminUser, Helper.AdminRole);
				}
				else
				{
					var errors = string.Join(", ", result.Errors.Select(e => e.Description));
					_logger.LogError("Failed to create Admin user: {Errors}", errors);
				}

			}
		}
	}
}

