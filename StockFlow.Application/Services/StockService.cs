using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.StockDtos;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Application.Interfaces.UOW;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Services
{
    public class StockService(IGenericRepository<Stock> _stockRepository,
        IGenericRepository<StockMovement> _stockMovementRepository,
        IGenericRepository<Product> _productRepository,
        IGenericRepository<Warehouse> _warehouseRepository,
        IUnitOfWork _unitOfWork,
        IValidator<StockInDto> _stockInValidator,
        IValidator<StockOutDto> _stockOutValidator) : IStockService
    {
        private const int SeedUserId = 1;
        public async Task<BaseResult<ResultStockDto>> StockInAsync(StockInDto stockInDto)
        {
            var validationResult = await _stockInValidator.ValidateAsync(stockInDto);
            if (!validationResult.IsValid)
            {
                return BaseResult<ResultStockDto>.Fail(validationResult.Errors);
            }

            var product = await _productRepository.GetByIdAsync(stockInDto.ProductId);
            if (product == null)
            {
                return BaseResult<ResultStockDto>.Fail("Ürün bulunamadı", ResultErrorType.NotFound);
            }

            var warehouse = await _warehouseRepository.GetByIdAsync(stockInDto.WarehouseId);
            if (warehouse == null)
            {
                return BaseResult<ResultStockDto>.Fail("Depo bulunamadı", ResultErrorType.NotFound);
            }

           
            var stock = await _stockRepository.Query()
                .FirstOrDefaultAsync(s => s.ProductId == stockInDto.ProductId && s.WarehouseId == stockInDto.WarehouseId);

            if (stock == null)
            {
                stock = new Stock
                {
                    ProductId = stockInDto.ProductId,
                    WarehouseId = stockInDto.WarehouseId,
                    Quantity = 0
                };
                await _stockRepository.AddAsync(stock);
            }


            stock.Quantity += stockInDto.Quantity;
            _stockRepository.Update(stock);

            var movement = new StockMovement
            {
                ProductId = stockInDto.ProductId,
                WarehouseId = stockInDto.WarehouseId,
                Type = StockMovementType.Adjustment,
                Quantity = stockInDto.Quantity,
                Note = stockInDto.Note,
                PerformedByUserId = SeedUserId
            };
            await _stockMovementRepository.AddAsync(movement);


            await _unitOfWork.SaveChangesAsync();

            return BaseResult<ResultStockDto>.Success(new ResultStockDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Name,
                Quantity = stock.Quantity   
            });
        }

        public async Task<BaseResult<ResultStockDto>> StockOutAsync(StockOutDto stockOutDto)
        {
            var validationResult = await _stockOutValidator.ValidateAsync(stockOutDto);
            if(!validationResult.IsValid)
            {
                return BaseResult<ResultStockDto>.Fail(validationResult.Errors);
            }

            var product=await _productRepository.GetByIdAsync(stockOutDto.ProductId);
            if (product == null) { 
            
                return BaseResult<ResultStockDto>.Fail("Ürün bulunamadı", ResultErrorType.NotFound);

            }

            var warehouse = await _warehouseRepository.GetByIdAsync(stockOutDto.WarehouseId);
            if (warehouse == null)
            {
                return BaseResult<ResultStockDto>.Fail("Depo bulunamadı", ResultErrorType.NotFound);
            }

            var stock = await _stockRepository.Query()
                .FirstOrDefaultAsync(s => s.ProductId == stockOutDto.ProductId && s.WarehouseId == stockOutDto.WarehouseId);

            if (stock == null) {
                return BaseResult<ResultStockDto>.Fail("Stok bulunamadı", ResultErrorType.NotFound);
            }

            if (stock.Quantity < stockOutDto.Quantity)   
            {
                return BaseResult<ResultStockDto>.Fail(
                    $"Yetersiz stok. Mevcut: {stock.Quantity}, İstenen: {stockOutDto.Quantity}",
                    ResultErrorType.BusinessRule);
            }


            stock.Quantity -= stockOutDto.Quantity;
            _stockRepository.Update(stock);

            var movement = new StockMovement
            {
                ProductId = stockOutDto.ProductId,
                WarehouseId = stockOutDto.WarehouseId,
                Type = StockMovementType.Adjustment,
                Quantity = -stockOutDto.Quantity,
                Note = stockOutDto.Note,
                PerformedByUserId = SeedUserId
            };

            await _stockMovementRepository.AddAsync(movement);
            await _unitOfWork.SaveChangesAsync();

            return BaseResult<ResultStockDto>.Success(new ResultStockDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Name,
                Quantity = stock.Quantity
            });
        }
    }
}
