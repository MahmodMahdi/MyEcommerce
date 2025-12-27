using AutoMapper;
using Microsoft.AspNetCore.Http;
using MyEcommerce.ApplicationLayer.Interfaces.Services;
using MyEcommerce.ApplicationLayer.ViewModels;
using MyEcommerce.DomainLayer.Interfaces.Repositories;
using MyEcommerce.DomainLayer.Models;
using Utilities;

namespace MyEcommerce.ApplicationLayer.Services
{
	public class HomeService : IHomeService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		public HomeService(IUnitOfWork unitOfWork,IMapper mapper)
		{
			 _unitOfWork = unitOfWork;
			_mapper = mapper;
		}
		public async Task<PaginatedResultViewModel<ProductViewModel>> GetAllAsync(int pageNumber)
		{
			var pageSize = 8;
			var totalProducts = await _unitOfWork.ProductRepository.CountAsync();
			var numToSkip = (pageNumber - 1) * pageSize;
			var products =await _unitOfWork.ProductRepository.GetPagedAsync(numToSkip, pageSize);
			var mappedProducts = _mapper.Map<IEnumerable<ProductViewModel>>(products);
			var result = new PaginatedResultViewModel<ProductViewModel>
			{
				Items = mappedProducts,
				CurrentPage = pageNumber,
				TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize)
			};
			return result;
		}

		public async Task<CartItemViewModel> GetProductDetailsAsync(int productId)
		{
			var shoppingCart = new ShoppingCart
			{
				Product = await _unitOfWork.ProductRepository.GetFirstOrDefaultAsync(p => p.Id == productId, IncludeProperties: "Category"),
				ProductId = productId,
				Count = 1
			};
			return _mapper.Map<CartItemViewModel>(shoppingCart);
		}

		public async Task<int> AddToCartAsync(CartItemViewModel cartItemViewModel,string userId)
		{
			var product = await _unitOfWork.ProductRepository.GetFirstOrDefaultAsync(p => p.Id == cartItemViewModel.ProductId);
			if (product == null) throw new Exception("Product not found");
			var CartFromDb = await _unitOfWork.ShoppingCartRepository.GetFirstOrDefaultAsync(c => c.ApplicationUserId == userId && c.ProductId == cartItemViewModel.ProductId);
			int currentInCart = CartFromDb != null ? CartFromDb.Count : 0;
			int totalRequested = currentInCart + cartItemViewModel.Count;
			if (totalRequested > product.StockQuantity)
			{
				throw new Exception($"Sorry, you already have {currentInCart} in cart. Total available is {product.StockQuantity}.");
			}
			if (CartFromDb == null)
			{
				var cartVM = _mapper.Map<ShoppingCart>(cartItemViewModel);
				cartVM.ApplicationUserId = userId;
				cartVM.Product = null;
				await _unitOfWork.ShoppingCartRepository.AddAsync(cartVM);
			}
			else
			{
				// if he had previous order with same order
				_unitOfWork.ShoppingCartRepository.IncreaseCount(CartFromDb, cartItemViewModel.Count);
			}
			await _unitOfWork.CompleteAsync();
			var CartItems = await _unitOfWork.ShoppingCartRepository.CountAsync(s => s.ApplicationUserId == userId);
			return CartItems;
		}
	}
}
