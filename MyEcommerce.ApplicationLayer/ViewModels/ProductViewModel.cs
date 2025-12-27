using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyEcommerce.ApplicationLayer.ViewModels
{
	public class ProductViewModel
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "*")]
		[MaxLength(30, ErrorMessage = "Name must be less than 29 letters"), MinLength(2, ErrorMessage = "Name must be greater than 2 letters.")]
		public string Name { get; set; }
		public string Description { get; set; }
		[Required]
		[Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
		public int StockQuantity { get; set; }
		[ValidateNever]
		public string Image { get; set; }
		[Required(ErrorMessage = "*")]
		[Range(1, 100000, ErrorMessage = "Price must be between 1 to 100000")]
		public decimal Price { get; set; }
		[Range(0, 100, ErrorMessage = "Discount must be between 1 to 100")]
		public decimal Discount { get; set; }
		public decimal AcualPrice { get; set; }

		[DisplayName("Category")]
		[Required(ErrorMessage = "*")]
		public int CategoryId { get; set; }
		[ValidateNever]
		public string CategoryName { get; set; }
		[ValidateNever]
		public IEnumerable<DropDownItem> Categories { get; set; }
	}

	public class DropDownItem
	{
		public string Value { get; set; }
		public string Text { get; set; }
	}
}
