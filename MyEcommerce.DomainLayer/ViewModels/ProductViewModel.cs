using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using MyEcommerce.DomainLayer.Models;

namespace MyEcommerce.DomainLayer.ViewModels
{
	public class ProductViewModel
	{
		public Product Product { get; set; }
		[ValidateNever]
		public List<DropDownItem> Categories { get; set; }
	}

	public class DropDownItem
	{
		public string Value { get; set; }
		public string Text { get; set; }
	}
}
