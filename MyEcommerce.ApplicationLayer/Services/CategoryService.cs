using AutoMapper;
using MyEcommerce.DomainLayer.Interfaces.Repositories;
using MyEcommerce.DomainLayer.Models;
using MyEcommerce.ApplicationLayer.ViewModels;


namespace MyEcommerce.ApplicationLayer.Services
{
	public class CategoryService : ICategoryService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		public CategoryService(IUnitOfWork unitOfWork,IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}
		public async Task<IEnumerable<CategoryViewModel>> GetAllAsync()
		{
			var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
			return _mapper.Map<IEnumerable<CategoryViewModel>>(categories);
		}

		public async Task<CategoryViewModel> GetFirstOrDefaultAsync(int id)
		{
			var category = await _unitOfWork.CategoryRepository.GetFirstOrDefaultAsync(x => x.Id == id,tracked:false);
			return _mapper.Map<CategoryViewModel>(category);
		}
		public async Task AddAsync(CategoryViewModel categoryVM)
		{
			var category = _mapper.Map<Category>(categoryVM);
			category.CreatedTime = DateTime.Now;
			await _unitOfWork.CategoryRepository.AddAsync(category);
			await _unitOfWork.CompleteAsync();
		}

		public async Task UpdateAsync(CategoryViewModel categoryVM)
		{
			var category = _mapper.Map<Category>(categoryVM);
			 _unitOfWork.CategoryRepository.Update(category);
			await _unitOfWork.CompleteAsync();
		}
		public async Task DeleteAsync(int id)
		{
			var category = await _unitOfWork.CategoryRepository.GetFirstOrDefaultAsync(c => c.Id == id);
			if (category != null)
			{
				var categoryToDelete = _mapper.Map<Category>(category);
				_unitOfWork.CategoryRepository.Remove(categoryToDelete);
				await _unitOfWork.CompleteAsync();
			}
		}
	}
}
