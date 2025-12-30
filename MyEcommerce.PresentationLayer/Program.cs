using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyEcommerce.ApplicationLayer.Extensions;
using MyEcommerce.ApplicationLayer.Mapping;
using MyEcommerce.DataAccessLayer.Data;
using MyEcommerce.DataAccessLayer.DataSeeding;
using MyEcommerce.DomainLayer.Models;
using Serilog;
using Serilog.Events;
using Stripe;
using Utilities;

namespace MyEcommerce.PresentationLayer
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			var logPath = Path.Combine(builder.Environment.WebRootPath, "Logs", "ShopSphere.txt");

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
				.CreateLogger();

			// Add services to the container.
			builder.Services.AddControllersWithViews();
			builder.Services.AddAutoMapper(typeof(MappingProfile));

			builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
			builder.Services.AddDbContext<ApplicationDbContext>(
				options => options.UseSqlServer(
				builder.Configuration.GetConnectionString("DB")
			));

			builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("emailSettings"));

			builder.Services.AddSingleton(resolver =>
				resolver.GetRequiredService<IOptions<EmailSettings>>().Value);
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
			builder.Services.AddAuthentication();
			builder.Services.AddAuthentication().AddGoogle(options =>
			{
				options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
				options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
			});
			builder.Services.AddApplicationProject();
			builder.Services.AddSession();
			builder.Services.AddDistributedMemoryCache();

			builder.Services.AddScoped<IDbInitializer, DbInitializer>();


			builder.Host.UseSerilog();
			
			var app = builder.Build();
			using (var scope = app.Services.CreateScope())
			{
				try
				{
					var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
					await dbInitializer.Initialize();
				}
				catch(Exception ex)
				{
					
				}
			}
			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseDeveloperExceptionPage();
				app.UseHsts();
			}
			// 1. تعريف اللغات
			var supportedCultures = new[] { "en", "ar" };
			var localizationOptions = new RequestLocalizationOptions()
				.SetDefaultCulture("en")
				.AddSupportedCultures(supportedCultures)
				.AddSupportedUICultures(supportedCultures);

			// ترتيب المصادر: يبحث في الرابط أولاً، ثم الكوكي، ثم إعدادات المتصفح
			localizationOptions.RequestCultureProviders = new List<IRequestCultureProvider>
			{
				new QueryStringRequestCultureProvider(), // 1. الرابط (?culture=ar)
                new CookieRequestCultureProvider(),      // 2. الكوكيز (عشان يفتكر اللغة في الصفحة الجاية)
                new AcceptLanguageHeaderRequestCultureProvider() // 3. لغة المتصفح (كخيار أخير)
            };
			// 3. تفعيل الإعدادات
			app.UseRequestLocalization(localizationOptions);

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();
			app.UseSession();

			var stripeKey = builder.Configuration["stripe:Secretkey"];
			if (!string.IsNullOrEmpty(stripeKey))
			{
				StripeConfiguration.ApiKey = builder.Configuration.GetSection("stripe:Secretkey").Get<string>();
			}
			app.UseAuthentication();
			app.UseAuthorization();
			app.MapRazorPages();
			app.MapControllerRoute(
				name: "Admin",
				pattern: "{area=Admin}/{controller=Home}/{action=Index}/{id?}");
			app.MapControllerRoute(
				name: "default",
				pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

			app.Run();
		}
	}
}
