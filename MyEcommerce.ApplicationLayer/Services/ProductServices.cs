using AutoMapper;
using Microsoft.AspNetCore.Http;
using MyEcommerce.ApplicationLayer.Interfaces.Services;
using MyEcommerce.ApplicationLayer.ViewModels;
using MyEcommerce.DomainLayer.Interfaces.Repositories;
using MyEcommerce.DomainLayer.Models;
using System.Linq.Expressions;

namespace MyEcommerce.ApplicationLayer.Services
{
	public class ProductServices : IProductService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IImageService _imageService;
		public ProductServices(IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_imageService = imageService;
		}
		public async Task<IEnumerable<ProductViewModel>> GetAllAsync(Expression<Func<Product, bool>>? predicate = null, string? IncludeProperties = null)
		{
			var products = await _unitOfWork.ProductRepository.GetAllAsync(predicate, IncludeProperties ?? "Category");
			return _mapper.Map<IEnumerable<ProductViewModel>>(products);
		}
		public async Task<ProductViewModel> GetFirstOrDefaultAsync(Expression<Func<Product, bool>>? predicate = null, string? IncludeProperties = null)
		{
			var product = await _unitOfWork.ProductRepository.GetFirstOrDefaultAsync(predicate, IncludeProperties);
			return _mapper.Map<ProductViewModel>(product);
		}
		public async Task AddAsync(ProductViewModel productViewModel, IFormFile image)
		{
			productViewModel.Image = await _imageService.SaveAsync(image);
			var product = _mapper.Map<Product>(productViewModel);
			await _unitOfWork.ProductRepository.AddAsync(product);
			await _unitOfWork.CompleteAsync();
		}
		public async Task UpdateAsync(ProductViewModel productViewModel, IFormFile image)
		{
			var oldProduct = await _unitOfWork.ProductRepository.GetFirstOrDefaultAsync(x => x.Id == productViewModel.Id);
			if (oldProduct != null)
			{
				if (image != null && image.Length > 0)
				{
					var oldImagePath = oldProduct.Image;
					productViewModel.Image = await _imageService.SaveAsync(image);
					await _imageService.DeleteAsync(oldProduct.Image);

				}
				else
				{
					productViewModel.Image = oldProduct.Image;
				}
				_mapper.Map(productViewModel, oldProduct);
				_unitOfWork.ProductRepository.Update(oldProduct);
				await _unitOfWork.CompleteAsync();
			}
		}
		public async Task DeleteAsync(int id)
		{
			var product = await _unitOfWork.ProductRepository.GetFirstOrDefaultAsync(x => x.Id == id);

			await _imageService.DeleteAsync(product.Image);
			_unitOfWork.ProductRepository.Remove(product);
			await _unitOfWork.CompleteAsync();
		}

	}
}
