using FluentValidation;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.CategoryDtos;
using StockFlow.Application.DTOs.SupplierDtos;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Application.Interfaces.UOW;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Services
{
    public class SupplierService(IGenericRepository<Supplier> _genericRepository, IUnitOfWork _unitOfWork, IMapper _mapper, IValidator<CreateSupplierDto> _createValidator,
    IValidator<UpdateSupplierDto> _updateValidator) : ISupplierService
    {
        public async Task<BaseResult<ResultSupplierDto>> CreateAsync(CreateSupplierDto dto)
        {
            dto.Name = dto.Name.Trim();
            dto.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            dto.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();

            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BaseResult<ResultSupplierDto>.Fail(validationResult.Errors);
            }
            var mappedSupplier = _mapper.Map<Supplier>(dto);
            await _genericRepository.AddAsync(mappedSupplier);
            await _unitOfWork.SaveChangesAsync();

            var mappedResult = _mapper.Map<ResultSupplierDto>(mappedSupplier);
            return BaseResult<ResultSupplierDto>.Success(mappedResult);
        }

        public async Task<BaseResult<bool>> DeleteAsync(int id)
        {
            var check = await _genericRepository.Query()
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    Supplier = s,
                    HasProducts = s.Products.Any(),
                    HasPurchaseOrders = s.PurchaseOrders.Any()
                })
                .FirstOrDefaultAsync();

            if (check == null)
            {
                return BaseResult<bool>.Fail("Tedarikçi bulunamadı.", ResultErrorType.NotFound);
            }

            var supplier = check.Supplier;

            if (check.HasProducts)
            {
                return BaseResult<bool>.Fail("Tedarikçiye bağlı ürünler var tedarikçiyi silemezsiniz.", ResultErrorType.Conflict);
            }

            if (check.HasPurchaseOrders)
            {
                return BaseResult<bool>.Fail("Tedarikçiye ait satın alma siparişleri var, tedarikçi silinemez.", ResultErrorType.Conflict);
            }

            _genericRepository.Delete(supplier);
            await _unitOfWork.SaveChangesAsync();
            return BaseResult<bool>.Success(true);
        }

        public async Task<BaseResult<PagedResult<ResultSupplierDto>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var paged = await _genericRepository.GetAllAsync(pageNumber, pageSize);
            var result = _mapper.Map<PagedResult<ResultSupplierDto>>(paged);

            return BaseResult<PagedResult<ResultSupplierDto>>.Success(result);
        }

        public async Task<BaseResult<ResultSupplierDto>> GetByIdAsync(int id)
        {
            var supplier = await _genericRepository.GetByIdAsync(id);
            if (supplier == null)
            {
                return BaseResult<ResultSupplierDto>.Fail("Tedarikçi bulunamadı.", ResultErrorType.NotFound);
            }
            var mappedSupplier = _mapper.Map<ResultSupplierDto>(supplier);
            return BaseResult<ResultSupplierDto>.Success(mappedSupplier);
        }

        public async Task<BaseResult<ResultSupplierDto>> UpdateAsync(int id, UpdateSupplierDto dto)
        {
            dto.Name = dto.Name.Trim();
            dto.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            dto.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();

            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BaseResult<ResultSupplierDto>.Fail(validationResult.Errors);
            }

            var supplier = await _genericRepository.QueryForUpdate()
                .FirstOrDefaultAsync(s => s.Id == id);
            if (supplier == null)
            {
                return BaseResult<ResultSupplierDto>.Fail("Tedarikçi bulunamadı.", ResultErrorType.NotFound);
            }

            _mapper.Map(dto, supplier);
            await _unitOfWork.SaveChangesAsync();

            var resultDto = _mapper.Map<ResultSupplierDto>(supplier);
            return BaseResult<ResultSupplierDto>.Success(resultDto);
        }
    }
}