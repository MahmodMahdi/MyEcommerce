using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyEcommerce.DomainLayer.Models;
using MyEcommerce.DomainLayer.Models.Order;

namespace MyEcommerce.DataAccessLayer.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){}
		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder); // مهم جداً عشان الـ Identity تشتغل

			// تعريف الـ Discriminator برمجياً
			builder.Entity<ApplicationUser>()
				.HasDiscriminator<string>("Discriminator")
				.HasValue("ApplicationUser");

			// جعل القيمة الافتراضية "ApplicationUser" على مستوى قاعدة البيانات
			builder.Entity<ApplicationUser>()
				.Property("Discriminator")
				.HasDefaultValue("ApplicationUser");
		}
		public DbSet<Category> Categories { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<ApplicationUser> ApplicationUsers { get; set; }
		public DbSet<ShoppingCart> ShoppingCarts { get; set; }
		public DbSet<OrderHeader> OrderHeaders { get; set; }
		public DbSet<OrderDetail> OrderDetails { get; set; }
	}
}
