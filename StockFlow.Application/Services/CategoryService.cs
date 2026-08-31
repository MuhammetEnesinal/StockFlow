using FluentValidation;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.CategoryDtos;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Application.Interfaces.UOW;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Services
{
    public class CategoryService(IGenericRepository<Category> _repository, IUnitOfWork _unitOfWork, IMapper _mapper, IValidator<CreateCategoryDto> _createValidator,
    IValidator<UpdateCategoryDto> _updateValidator) : ICategoryService
    {
        public async Task<BaseResult<ResultCategoryDto>> CreateAsync(CreateCategoryDto createCategoryDto)
        {
            createCategoryDto.Name = createCategoryDto.Name.Trim();

            var validationResult = await _createValidator.ValidateAsync(createCategoryDto);
            if (!validationResult.IsValid)
            {
                return BaseResult<ResultCategoryDto>.Fail(validationResult.Errors);
            }
            var category = _mapper.Map<Category>(createCategoryDto);
            await _repository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var resultDto = _mapper.Map<ResultCategoryDto>(category);
            return BaseResult<ResultCategoryDto>.Success(resultDto);
        }

        public async Task<BaseResult<bool>> DeleteAsync(int id)
        {
            var category = await _repository.Query()
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return BaseResult<bool>.Fail("Kategori Bulunamadı.", ResultErrorType.NotFound);
            }
            if (category.Products.Any())
            {
                return BaseResult<bool>.Fail("Bu kategoriye ait ürünler var,önce ürünleri silin.", ResultErrorType.Conflict);
            }
            _repository.Delete(category);
            await _unitOfWork.SaveChangesAsync();
            return BaseResult<bool>.Success(true);
        }

        public async Task<BaseResult<IEnumerable<ResultCategoryDto>>> GetAllAsync()
        {
            var categories = await _repository.GetAllAsync();
            var mappedCategories = _mapper.Map<IEnumerable<ResultCategoryDto>>(categories);
            return BaseResult<IEnumerable<ResultCategoryDto>>.Success(mappedCategories);
        }

        public async Task<BaseResult<ResultCategoryDto>> GetByIdAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
            {
                return BaseResult<ResultCategoryDto>.Fail("Kategori bulunamadı.", ResultErrorType.NotFound);
            }
            var mappedCategory = _mapper.Map<ResultCategoryDto>(category);
            return BaseResult<ResultCategoryDto>.Success(mappedCategory);
        }

        public async Task<BaseResult<ResultCategoryDto>> UpdateAsync(int id, UpdateCategoryDto dto)
        {
            dto.Name = dto.Name.Trim();

            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BaseResult<ResultCategoryDto>.Fail(validationResult.Errors);
            }
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
            {
                return BaseResult<ResultCategoryDto>.Fail("Kategori Bulunamadı.", ResultErrorType.NotFound);
            }
            _mapper.Map(dto, category);
            _repository.Update(category);
            await _unitOfWork.SaveChangesAsync();

            var resultDto = _mapper.Map<ResultCategoryDto>(category);
            return BaseResult<ResultCategoryDto>.Success(resultDto);
        }
    }
}