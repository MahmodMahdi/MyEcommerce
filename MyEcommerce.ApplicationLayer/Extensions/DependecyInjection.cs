using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using MyEcommerce.ApplicationLayer.Interfaces.Services;
using MyEcommerce.ApplicationLayer.Services;
using MyEcommerce.DataAccessLayer.DataSeeding;
using MyEcommerce.DataAccessLayer.Repositories;
using MyEcommerce.DomainLayer.Interfaces.Repositories;

namespace MyEcommerce.ApplicationLayer.Extensions
{
	public static class DependecyInjection
	{
		public static IServiceCollection AddApplicationProject(this IServiceCollection services)
		{
			services.AddScoped<IUnitOfWork, UnitOfWork>();

			services.AddScoped<IHomeService, HomeService>();
			services.AddScoped<ICategoryService, CategoryService>();
			services.AddScoped<IProductService, ProductServices>();
			services.AddScoped<IShoppingCartService, ShoppingCartService>();
			services.AddScoped<IOrderServices, OrderServices>();
			services.AddScoped<IDashboardService, DashboardService>();
			services.AddScoped<IApplicationUserService, ApplicationUserService>();
			services.AddScoped<IPaymentService, PaymentService>();
			services.AddTransient<IEmailService, EmailService>();

			services.AddScoped<IImageService, ImageService>();
			services.AddTransient<IEmailSender, EmailService>();

			return services;
		}
	}
}
