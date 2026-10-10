using FluentValidation;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.WarehouseDtos;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Application.Interfaces.UOW;
using StockFlow.Domain.Entities;

namespace StockFlow.Application.Services
{
    public class WarehouseService(IGenericRepository<Warehouse> _genericRepository, IUnitOfWork _unitOfWork, IMapper _mapper, IValidator<CreateWarehouseDto> _createValidator, IValidator<UpdateWarehouseDto> _updateValidator) : IWarehouseService
    {
        public async Task<BaseResult<ResultWarehouseDto>> CreateAsync(CreateWarehouseDto dto)
        {
            dto.Name = dto.Name.Trim();
            dto.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();

            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BaseResult<ResultWarehouseDto>.Fail(validationResult.Errors);
            }
            var mappedWarehouse = _mapper.Map<Warehouse>(dto);
            await _genericRepository.AddAsync(mappedWarehouse);
            await _unitOfWork.SaveChangesAsync();
            var resultWarehouse = _mapper.Map<ResultWarehouseDto>(mappedWarehouse);
            return BaseResult<ResultWarehouseDto>.Success(resultWarehouse);
        }

        public async Task<BaseResult<bool>> DeleteAsync(int id)
        {
            var check = await _genericRepository.Query()
                .Where(w => w.Id == id)
                .Select(w => new
                {
                    Warehouse = w,
                    HasStocks = w.Stocks.Any(),
                    HasStockMovements = w.StockMovements.Any(),
                    HasOrderItems = w.OrderItems.Any(),
                    HasPurchaseOrders = w.PurchaseOrders.Any(),
                    HasUsers = w.Users.Any()
                })
                .FirstOrDefaultAsync();

            if (check == null)
            {
                return BaseResult<bool>.Fail("Depo bulunamadı.", ResultErrorType.NotFound);
            }
            var warehouse = check.Warehouse;
            if (check.HasStocks)
            {
                return BaseResult<bool>.Fail("Bu depoda hâlâ stok var, önce stokları boşaltın.", ResultErrorType.Conflict);
            }
            if (check.HasStockMovements)
            {
                return BaseResult<bool>.Fail("Bu depoya ait stok hareketi geçmişi var, depo silinemez.", ResultErrorType.Conflict);
            }
            if (check.HasOrderItems)
            {
                return BaseResult<bool>.Fail("Bu depoya bağlı sipariş kalemleri var, depo silinemez.", ResultErrorType.Conflict);
            }
            if (check.HasPurchaseOrders)
            {
                return BaseResult<bool>.Fail("Bu depoya bağlı satın alma siparişleri var, depo silinemez.", ResultErrorType.Conflict);
            }
            if (check.HasUsers)
            {
                return BaseResult<bool>.Fail("Bu depoya atanmış kullanıcılar var, önce kullanıcıları başka depoya taşıyın.", ResultErrorType.Conflict);
            }

            _genericRepository.Delete(warehouse);
            await _unitOfWork.SaveChangesAsync();
            return BaseResult<bool>.Success(true);
        }

        public async Task<BaseResult<PagedResult<ResultWarehouseDto>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var paged = await _genericRepository.GetAllAsync(pageNumber, pageSize);
            var mappedItems = _mapper.Map<List<ResultWarehouseDto>>(paged.Items);

            var result = new PagedResult<ResultWarehouseDto>
            {
                Items = mappedItems,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount
            };

            return BaseResult<PagedResult<ResultWarehouseDto>>.Success(result);
        }

        public async Task<BaseResult<ResultWarehouseDto>> GetByIdAsync(int id)
        {
            var warehouse = await _genericRepository.GetByIdAsync(id);
            if (warehouse == null)
            {
                return BaseResult<ResultWarehouseDto>.Fail("Depo bulunamadı.", ResultErrorType.NotFound);
            }
            var mappedWarehouse = _mapper.Map<ResultWarehouseDto>(warehouse);
            return BaseResult<ResultWarehouseDto>.Success(mappedWarehouse);
        }

        public async Task<BaseResult<ResultWarehouseDto>> UpdateAsync(int id, UpdateWarehouseDto dto)
        {
            dto.Name = dto.Name.Trim();
            dto.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BaseResult<ResultWarehouseDto>.Fail(validationResult.Errors);
            }

            var warehouse = await _genericRepository.QueryForUpdate()
                .FirstOrDefaultAsync(w => w.Id == id);
            if (warehouse == null)
            {
                return BaseResult<ResultWarehouseDto>.Fail("Güncellenecek depo bulunamadı.", ResultErrorType.NotFound);
            }

            _mapper.Map(dto, warehouse);
            await _unitOfWork.SaveChangesAsync();

            var mappedWarehouse = _mapper.Map<ResultWarehouseDto>(warehouse);
            return BaseResult<ResultWarehouseDto>.Success(mappedWarehouse);
        }
    }
}