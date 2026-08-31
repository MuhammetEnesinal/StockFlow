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
    public class WarehouseService(IGenericRepository<Warehouse> _genericRepository, IUnitOfWork _unitOfWork, IMapper _mapper,IValidator<UpdateWarehouseDto> _updateValidator, IValidator<CreateWarehouseDto> _createValidator) : IWarehouseService
    {
        public async Task<BaseResult<ResultWarehouseDto>> CreateAsync(CreateWarehouseDto dto)
        {
            dto.Name = dto.Name.Trim();
            dto.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();

            var validatonResult = await _createValidator.ValidateAsync(dto);
            if (!validatonResult.IsValid)
            {
                return  BaseResult<ResultWarehouseDto>.Fail(validatonResult.Errors);
            }
            var mappedWarehouse=_mapper.Map<Warehouse>(dto);
            await _genericRepository.AddAsync(mappedWarehouse);
            await _unitOfWork.SaveChangesAsync();
            var resultWarehouse = _mapper.Map<ResultWarehouseDto>(mappedWarehouse);
            return BaseResult<ResultWarehouseDto>.Success(resultWarehouse);
        }

        public async Task<BaseResult<bool>> DeleteAsync(int id)
        {
            var warehouse = await _genericRepository.Query()
                .Include(s => s.Stocks)
                .Include(s => s.StockMovements)
                .Include(o => o.Orders)
                .Include(p => p.PurchaseOrders)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (warehouse == null)
            {
                return BaseResult<bool>.Fail("Depo bulunamadı.", ResultErrorType.NotFound);
            }
            if (warehouse.Stocks.Any())
            {
                return BaseResult<bool>.Fail("Bu depoda hâlâ stok var, önce stokları boşaltın.", ResultErrorType.Conflict);
            }
            if (warehouse.StockMovements.Any())
            {
                return BaseResult<bool>.Fail("Bu depoya ait stok hareketi geçmişi var, depo silinemez.", ResultErrorType.Conflict);
            }
            if (warehouse.Orders.Any())
            {
                return BaseResult<bool>.Fail("Bu depoya bağlı satış siparişleri var, depo silinemez.", ResultErrorType.Conflict);
            }
            if (warehouse.PurchaseOrders.Any())
            {
                return BaseResult<bool>.Fail("Bu depoya bağlı satın alma siparişleri var, depo silinemez.", ResultErrorType.Conflict);
            }

            _genericRepository.Delete(warehouse);
            await _unitOfWork.SaveChangesAsync();
            return BaseResult<bool>.Success(true);
        }

        public async Task<BaseResult<IEnumerable<ResultWarehouseDto>>> GetAllAsync()
        {
            var warehouses =await _genericRepository.GetAllAsync();
            var mappedWarehouses=_mapper.Map<IEnumerable<ResultWarehouseDto>>(warehouses);
            return BaseResult<IEnumerable<ResultWarehouseDto>>.Success(mappedWarehouses);
        }

        public async Task<BaseResult<ResultWarehouseDto>> GetByIdAsync(int id)
        {
            var warehouse=await _genericRepository.GetByIdAsync(id);
            if (warehouse == null)
            {
                return BaseResult<ResultWarehouseDto>.Fail("Depo bulunamadı.",ResultErrorType.NotFound);
            }
            var mappedWarehouse=_mapper.Map<ResultWarehouseDto>(warehouse);
            return BaseResult<ResultWarehouseDto>.Success(mappedWarehouse);
        }

        public async Task<BaseResult<ResultWarehouseDto>> UpdateAsync(int id, UpdateWarehouseDto dto)
        {
            dto.Name = dto.Name.Trim();
            dto.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
            var validationResult=await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BaseResult<ResultWarehouseDto>.Fail(validationResult.Errors);
            }

            var warehouse = await _genericRepository.GetByIdAsync(id);
            if (warehouse == null) {

                return BaseResult<ResultWarehouseDto>.Fail("Güncellenek depo bulunamadı", ResultErrorType.NotFound);
            }

            _mapper.Map(dto, warehouse);
            _genericRepository.Update(warehouse);
            await _unitOfWork.SaveChangesAsync();

            var mappedWarehouse = _mapper.Map<ResultWarehouseDto>(warehouse);
            return BaseResult<ResultWarehouseDto>.Success(mappedWarehouse);

        }
    }
}
