using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyEcommerce.ApplicationLayer.Extensions;
using MyEcommerce.ApplicationLayer.Mapping;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DomainLayer.Models;
using Serilog;
using Serilog.Events;
using Stripe;
using Utilities;

namespace MyEcommerce.PresentationLayer
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Serilog
			Log.Logger = new LoggerConfiguration()
         	.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
	        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Model.Validation", LogEventLevel.Error) // أضف هذا السطر هنا
	        .MinimumLevel.Override("System", LogEventLevel.Warning)
	        .WriteTo.Console()
	        .WriteTo.File("Logs/ShopSphere.txt", rollingInterval: RollingInterval.Day)
	        .CreateLogger();

			// Add services to the container.
			builder.Services.AddControllersWithViews();
			builder.Services.AddAutoMapper(typeof(MappingProfile));

			builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
			builder.Services.AddDbContext<ApplicationDbContext>(
				options => options.UseSqlServer(
				builder.Configuration.GetConnectionString("DB")
			));

			// configure stripe
			var emailSettings = builder.Configuration.GetSection("emailSettings").Get<EmailSettings>();
			builder.Services.AddSingleton(emailSettings);
			builder.Services.Configure<StripeInfo>(builder.Configuration.GetSection("stripe"));
			builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
				options =>
				{
					options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(1);
					options.Lockout.MaxFailedAccessAttempts = 5;
					options.Lockout.AllowedForNewUsers = true;
					options.SignIn.RequireConfirmedAccount = true;
				})
				.AddDefaultUI()
				.AddDefaultTokenProviders()
				.AddEntityFrameworkStores<ApplicationDbContext>();
			builder.Services.ConfigureApplicationCookie(options =>
			{
				options.ExpireTimeSpan = TimeSpan.FromDays(14);
				options.SlidingExpiration = true; // تجديد المدة تلقائياً طالما يستخدم الموقع
			});
			builder.Services.AddAuthentication()
				.AddGoogle(options =>
			{
				options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
				options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
			});
			builder.Services.AddApplicationProject();
			builder.Services.AddSession();
			builder.Services.AddDistributedMemoryCache();

			builder.Host.UseSerilog();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseSession();

			app.UseRouting();


			StripeConfiguration.ApiKey = builder.Configuration.GetSection("stripe:Secretkey").Get<string>();

			app.UseAuthentication();
			app.UseAuthorization();
			app.MapRazorPages();
			app.MapControllerRoute(
				name: "default",
				pattern: "{area=Admin}/{controller=Home}/{action=Index}/{id?}");
			app.MapControllerRoute(
				name: "Customer",
				pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

			app.Run();
		}
	}
}
