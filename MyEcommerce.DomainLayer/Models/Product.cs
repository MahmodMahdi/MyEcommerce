using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyEcommerce.DomainLayer.Models
{
	public class Product
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int StockQuantity { get; set; }
		public string Image {  get; set; }
		public decimal Price { get; set; }
		[DisplayName("Category")]
		[Required(ErrorMessage = "*")]
		public decimal Discount { get; set; } = 0;
		public decimal AcualPrice => Price - (Price * (Discount / 100m));
		public int CategoryId { get; set; }
		public Category Category { get; set; }
	}
}
