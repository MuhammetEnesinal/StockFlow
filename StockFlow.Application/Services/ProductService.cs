using FluentValidation;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.CategoryDtos;
using StockFlow.Application.DTOs.ProductDtos;
using StockFlow.Application.DTOs.SupplierDtos;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Application.Interfaces.UOW;
using StockFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Services
{
    public class ProductService(
    IGenericRepository<Product> _genericRepository,
    IGenericRepository<Category> _categoryRepository,
    IGenericRepository<Supplier> _supplierRepository,
    IUnitOfWork _unitOfWork,
    IMapper _mapper,
    IValidator<CreateProductDto> _createValidator,
    IValidator<UpdateProductDto> _updateValidator) : IProductService
    {
        public async Task<BaseResult<ResultProductDto>> CreateAsync(CreateProductDto dto)
        {
            dto.Name = dto.Name.Trim();
            dto.SKU = dto.SKU.Trim();

            var validateResult = await _createValidator.ValidateAsync(dto);
            if (!validateResult.IsValid)
            {
                return BaseResult<ResultProductDto>.Fail(validateResult.Errors);
            }

            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                return BaseResult<ResultProductDto>.Fail("Belirtilen kategori bulunamadı.", ResultErrorType.NotFound);
            }

            Supplier? supplier = null;
            if (dto.SupplierId.HasValue)
            {
                supplier = await _supplierRepository.GetByIdAsync(dto.SupplierId.Value);
                if (supplier == null)
                {
                    return BaseResult<ResultProductDto>.Fail("Belirtilen tedarikçi bulunamadı.", ResultErrorType.NotFound);
                }
            }

            var mappedProduct = _mapper.Map<Product>(dto);
            await _genericRepository.AddAsync(mappedProduct);
            await _unitOfWork.SaveChangesAsync();

            var resultDto = _mapper.Map<ResultProductDto>(mappedProduct);
            resultDto.CategoryName = category.Name;
            resultDto.SupplierName = supplier?.Name;
            return BaseResult<ResultProductDto>.Success(resultDto);
        }

        public async Task<BaseResult<bool>> DeleteAsync(int id)
        {
            var check = await _genericRepository.Query()
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    Product = p,
                    HasStocks = p.Stocks.Any(),
                    HasStockMovements = p.StockMovements.Any(),
                    HasOrderItems = p.OrderItems.Any(),
                    HasPurchaseOrderItems = p.PurchaseOrderItems.Any()
                })
                .FirstOrDefaultAsync();

            if (check == null)
            {
                return BaseResult<bool>.Fail("Ürün bulunamadı.", ResultErrorType.NotFound);
            }

            var product = check.Product;

            if (check.HasStocks)
            {
                return BaseResult<bool>.Fail("Ürüne ait stok kaydı var silinemez.", ResultErrorType.Conflict);
            }
            if (check.HasStockMovements)
            {
                return BaseResult<bool>.Fail("Ürüne ait stok hareketi var silinemez.", ResultErrorType.Conflict);
            }
            if (check.HasOrderItems)
            {
                return BaseResult<bool>.Fail("Ürüne sipariş kayıdı var silinemez.", ResultErrorType.Conflict);
            }
            if (check.HasPurchaseOrderItems)
            {
                return BaseResult<bool>.Fail("Ürüne ait satın alma kayıdı var silinemez.", ResultErrorType.Conflict);
            }

            _genericRepository.Delete(product);
            await _unitOfWork.SaveChangesAsync();

            return BaseResult<bool>.Success(true);

        }

        public async Task<BaseResult<PagedResult<ResultProductDto>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var paged = await _genericRepository.GetAllAsync(pageNumber, pageSize,
                include: q => q
                    .Include(p => p.Category)
                    .Include(p => p.Supplier));
            var result = _mapper.Map<PagedResult<ResultProductDto>>(paged);

            return BaseResult<PagedResult<ResultProductDto>>.Success(result);
        }

        public async Task<BaseResult<ResultProductDto>> GetByIdAsync(int id)
        {
            var product = await _genericRepository.Query()
               .Include(p => p.Category)
               .Include(p => p.Supplier)
               .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return BaseResult<ResultProductDto>.Fail("Ürün bulunamadı.", ResultErrorType.NotFound);

            var resultDto = _mapper.Map<ResultProductDto>(product);
            return BaseResult<ResultProductDto>.Success(resultDto);
        }
        public async Task<BaseResult<ResultProductDto>> UpdateAsync(int id, UpdateProductDto dto)
        {
            dto.Name = dto.Name.Trim();
            var valditeResult = await _updateValidator.ValidateAsync(dto);
            if (!valditeResult.IsValid)
            {
                return BaseResult<ResultProductDto>.Fail(valditeResult.Errors);
            }

            var product = await _genericRepository.QueryForUpdate()
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return BaseResult<ResultProductDto>.Fail("Ürün bulunamadı.", ResultErrorType.NotFound);
            }



            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            if (category == null)
            {
                return BaseResult<ResultProductDto>.Fail("Belirtilen kategori bulunamadı.", ResultErrorType.NotFound);
            }

            Supplier? supplier = null;
            if (dto.SupplierId.HasValue)
            {
                supplier = await _supplierRepository.GetByIdAsync(dto.SupplierId.Value);
                if (supplier == null)
                {
                    return BaseResult<ResultProductDto>.Fail("Belirtilen tedarikçi bulunamadı.", ResultErrorType.NotFound);
                }
            }

            _mapper.Map(dto, product);
            await _unitOfWork.SaveChangesAsync();

            var mappedResult = _mapper.Map<ResultProductDto>(product);
            mappedResult.CategoryName = category.Name;
            mappedResult.SupplierName = supplier?.Name;
            return BaseResult<ResultProductDto>.Success(mappedResult);

        }
    }
}