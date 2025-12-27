using AutoMapper;
using MyEcommerce.ApplicationLayer.ViewModels;
using MyEcommerce.DomainLayer.Models;

namespace MyEcommerce.ApplicationLayer.Mapping
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<Category, CategoryViewModel>().ReverseMap();

			CreateMap<Product, ProductViewModel>().ReverseMap()
				.ForMember(dest=>dest.AcualPrice,opt=>opt.MapFrom(src=>src.AcualPrice))
				.ForMember(dest => dest.Category, opt => opt.Ignore());

			CreateMap<Product, ProductViewModel>()
				.ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name)) // تأكد من الوصول لـ Name
				.ForMember(dest => dest.Categories, opt => opt.Ignore())
				.ReverseMap()
				.ForMember(dest => dest.Category, opt => opt.Ignore());

			CreateMap<ApplicationUser, UserViewModel>().ReverseMap();

			CreateMap<ShoppingCart,CartItemViewModel>().ReverseMap();

			CreateMap<ApplicationUser, UserViewModel>().ReverseMap();

		}
	}
}
