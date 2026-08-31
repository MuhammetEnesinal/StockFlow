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
    public class  ProductService(
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

        public  async Task<BaseResult<bool>> DeleteAsync(int id)
        {
            var product = await _genericRepository.Query()
                .Include(p => p.Stocks)
                .Include(p => p.StockMovements)
                .Include(p => p.OrderItems)
                .Include(p => p.PurchaseOrderItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return BaseResult<bool>.Fail("Ürün bulunamadı.",ResultErrorType.NotFound);
            }

            if (product.Stocks.Any())
            {
                return BaseResult<bool>.Fail("Ürüne ait stok kaydı var silinemez.",ResultErrorType.Conflict);
            }
            if (product.StockMovements.Any())
            {
                return BaseResult<bool>.Fail("Ürüne ait stok hareketi var silinemez.", ResultErrorType.Conflict);
            }
            if (product.OrderItems.Any())
            {
                return BaseResult<bool>.Fail("Ürüne sipariş kayıdı var silinemez.", ResultErrorType.Conflict);
            }
            if (product.PurchaseOrderItems.Any())
            {
                return BaseResult<bool>.Fail("Ürüne ait satın alma kayıdı var silinemez.", ResultErrorType.Conflict);
            }

            _genericRepository.Delete(product);
            await _unitOfWork.SaveChangesAsync();

            return BaseResult<bool>.Success(true);
                
         }

        public async Task<BaseResult<IEnumerable<ResultProductDto>>> GetAllAsync()
        {
            var products = await _genericRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .ToListAsync();

            var mappedProducts = products.Select(p =>
            {
                var dto = _mapper.Map<ResultProductDto>(p);
                dto.CategoryName = p.Category.Name;
                dto.SupplierName = p.Supplier?.Name;
                return dto;
            }).ToList();

            return BaseResult<IEnumerable<ResultProductDto>>.Success(mappedProducts);
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
            resultDto.CategoryName = product.Category.Name;
            resultDto.SupplierName = product.Supplier?.Name;
            return BaseResult<ResultProductDto>.Success(resultDto);
        }

        public async Task<BaseResult<ResultProductDto>> UpdateAsync(int id, UpdateProductDto dto)
        {
            dto.Name = dto.Name.Trim();
            var valditeResult=await _updateValidator.ValidateAsync(dto);
            if (!valditeResult.IsValid)
            {
               return BaseResult<ResultProductDto>.Fail(valditeResult.Errors);
            }

            var product =await _genericRepository.GetByIdAsync(id);
            if(product == null)
            {
                return BaseResult<ResultProductDto>.Fail("Ürün bulunamadı.",ResultErrorType.NotFound);
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
            _genericRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();

            var mappedResult = _mapper.Map<ResultProductDto>(product);
            mappedResult.CategoryName = category.Name;
            mappedResult.SupplierName = supplier?.Name;
            return BaseResult<ResultProductDto>.Success(mappedResult);

        }
    }
}
