using System.ComponentModel.DataAnnotations;

namespace MyEcommerce.DomainLayer.Models
{
	public class Category
	{
		public int Id { get; set; }
		[Required]
		public string Name { get; set; }
		public string Description { get; set; }
		public DateTime CreatedTime { get; set; }= DateTime.Now;
	}
}
