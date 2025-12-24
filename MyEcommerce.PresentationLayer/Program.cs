using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MyEcommerce.ApplicationLayer.Interfaces.Services;
using MyEcommerce.ApplicationLayer.Mapping;
using MyEcommerce.ApplicationLayer.Services;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DataAccessLayer.Repositories;
using MyEcommerce.DomainLayer.Interfaces.Repositories;
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
				.MinimumLevel.Override("System", LogEventLevel.Warning)
				.WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Warning)
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
				options => {
					options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(1);
					options.Lockout.MaxFailedAccessAttempts = 5;
					options.Lockout.AllowedForNewUsers = true;
					options.SignIn.RequireConfirmedAccount = true;
				})
				.AddDefaultUI()
				.AddDefaultTokenProviders()
				.AddEntityFrameworkStores<ApplicationDbContext>();
			builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
			builder.Services.AddScoped<ICategoryService, CategoryService>();
			builder.Services.AddScoped<IProductService,ProductServices>();
			builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
			builder.Services.AddScoped<IOrderServices, OrderServices>();
			builder.Services.AddScoped<IDashboardService, DashboardService>();
			builder.Services.AddScoped<IApplicationUserService,ApplicationUserService>();
			builder.Services.AddScoped<IPaymentService,PaymentService>();
			builder.Services.AddScoped<IEmailService,EmailService>();
			builder.Services.AddScoped<IImageService,ImageService>();
			builder.Services.AddTransient<IEmailSender, EmailService>();
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
