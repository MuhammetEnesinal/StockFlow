using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.StockDtos;
using StockFlow.Application.DTOs.TransferDtos;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Application.Interfaces.UOW;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Enums;

namespace StockFlow.Application.Services
{
    public class StockService(
        IGenericRepository<Stock> _stockRepository,
        IGenericRepository<StockMovement> _stockMovementRepository,
        IGenericRepository<Product> _productRepository,
        IGenericRepository<Warehouse> _warehouseRepository,
        IUnitOfWork _unitOfWork,
        IValidator<StockInDto> _stockInValidator,
        IValidator<StockOutDto> _stockOutValidator,
        IValidator<TransferDto> _transferValidator) : IStockService
    {
        private const int SeedUserId = 1;

        public async Task<BaseResult<IEnumerable<ResultStockMovementDto>>> GetMovementsAsync(int productId, int warehouseId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return BaseResult<IEnumerable<ResultStockMovementDto>>.Fail("Ürün bulunamadı", ResultErrorType.NotFound);

            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
            if (warehouse == null)
                return BaseResult<IEnumerable<ResultStockMovementDto>>.Fail("Depo bulunamadı", ResultErrorType.NotFound);

            var movements = await _stockMovementRepository.Query()
                .Include(m => m.PerformedByUser)
                .Where(m => m.ProductId == productId && m.WarehouseId == warehouseId)
                .OrderByDescending(m => m.CreateAtTime)
                .ToListAsync();

            var movents = movements.Select(m => new ResultStockMovementDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Name,
                Type = m.Type.ToString(),
                Note = m.Note,
                Quantity = m.Quantity,
                PerformedByUserId = m.PerformedByUserId,
                PerformedByUserName = m.PerformedByUser.FullName,
                CreateAtTime = m.CreateAtTime
            }).ToList();

            return BaseResult<IEnumerable<ResultStockMovementDto>>.Success(movents);
        }

        public async Task<BaseResult<IEnumerable<ResultStockDto>>> GetStockByWarehouseAsync(int warehouseId)
        {
            var warehouse = await _warehouseRepository.Query()
                .Include(x => x.Stocks)
                .ThenInclude(s => s.Product)
                .FirstOrDefaultAsync(w => w.Id == warehouseId);

            if (warehouse == null)
            {
                return BaseResult<IEnumerable<ResultStockDto>>.Fail("Depo bulunamadı", ResultErrorType.NotFound);
            }

            var stockDtos = warehouse.Stocks.Select(s => new ResultStockDto
            {
                ProductId = s.ProductId,
                ProductName = s.Product.Name,
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Name,
                Quantity = s.Quantity
            }).ToList();

            return BaseResult<IEnumerable<ResultStockDto>>.Success(stockDtos);
        }

        public async Task<BaseResult<ResultStockDto>> StockInAsync(StockInDto stockInDto)
        {
            stockInDto.Note = string.IsNullOrWhiteSpace(stockInDto.Note) ? null : stockInDto.Note.Trim();

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
                    Quantity = stockInDto.Quantity
                };
                await _stockRepository.AddAsync(stock);
            }
            else
            {
                stock.Quantity += stockInDto.Quantity;
                _stockRepository.Update(stock);
            }

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
            stockOutDto.Note = string.IsNullOrWhiteSpace(stockOutDto.Note) ? null : stockOutDto.Note.Trim();

            var validationResult = await _stockOutValidator.ValidateAsync(stockOutDto);
            if (!validationResult.IsValid)
            {
                return BaseResult<ResultStockDto>.Fail(validationResult.Errors);
            }

            var product = await _productRepository.GetByIdAsync(stockOutDto.ProductId);
            if (product == null)
            {
                return BaseResult<ResultStockDto>.Fail("Ürün bulunamadı", ResultErrorType.NotFound);
            }

            var warehouse = await _warehouseRepository.GetByIdAsync(stockOutDto.WarehouseId);
            if (warehouse == null)
            {
                return BaseResult<ResultStockDto>.Fail("Depo bulunamadı", ResultErrorType.NotFound);
            }

            var stock = await _stockRepository.Query()
                .FirstOrDefaultAsync(s => s.ProductId == stockOutDto.ProductId && s.WarehouseId == stockOutDto.WarehouseId);

            if (stock == null)
            {
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

        public async Task<BaseResult<ResultTransferDto>> TransferAsync(TransferDto dto)
        {
            dto.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();

            var validationResult = await _transferValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BaseResult<ResultTransferDto>.Fail(validationResult.Errors);
            }

            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                return BaseResult<ResultTransferDto>.Fail("Ürün bulunamadı", ResultErrorType.NotFound);
            }

            var sourceWarehouse = await _warehouseRepository.GetByIdAsync(dto.SourceWarehouseId);
            if (sourceWarehouse == null)
            {
                return BaseResult<ResultTransferDto>.Fail("Kaynak depo bulunamadı", ResultErrorType.NotFound);
            }

            var targetWarehouse = await _warehouseRepository.GetByIdAsync(dto.TargetWarehouseId);
            if (targetWarehouse == null)
            {
                return BaseResult<ResultTransferDto>.Fail("Hedef depo bulunamadı", ResultErrorType.NotFound);
            }

            var sourceStock = await _stockRepository.Query()
                .FirstOrDefaultAsync(s => s.ProductId == dto.ProductId && s.WarehouseId == dto.SourceWarehouseId);

            if (sourceStock == null)
            {
                return BaseResult<ResultTransferDto>.Fail("Kaynak depoda bu ürüne ait stok bulunamadı", ResultErrorType.NotFound);
            }

            if (sourceStock.Quantity < dto.Quantity)
            {
                return BaseResult<ResultTransferDto>.Fail(
                    $"Yetersiz stok. Mevcut: {sourceStock.Quantity}, İstenen: {dto.Quantity}",
                    ResultErrorType.BusinessRule);
            }

            sourceStock.Quantity -= dto.Quantity;
            _stockRepository.Update(sourceStock);

            var targetStock = await _stockRepository.Query()
                .FirstOrDefaultAsync(s => s.ProductId == dto.ProductId && s.WarehouseId == dto.TargetWarehouseId);

            if (targetStock == null)
            {
                targetStock = new Stock
                {
                    ProductId = dto.ProductId,
                    WarehouseId = dto.TargetWarehouseId,
                    Quantity = dto.Quantity
                };
                await _stockRepository.AddAsync(targetStock);
            }
            else
            {
                targetStock.Quantity += dto.Quantity;
                _stockRepository.Update(targetStock);
            }

            var transferGroupId = Guid.NewGuid();

            var outMovement = new StockMovement
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.SourceWarehouseId,
                Type = StockMovementType.TransferOut,
                Quantity = -dto.Quantity,
                Note = dto.Note,
                TransferGroupId = transferGroupId,
                PerformedByUserId = SeedUserId
            };
            await _stockMovementRepository.AddAsync(outMovement);

            var inMovement = new StockMovement
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.TargetWarehouseId,
                Type = StockMovementType.TransferIn,
                Quantity = dto.Quantity,
                Note = dto.Note,
                TransferGroupId = transferGroupId,
                PerformedByUserId = SeedUserId
            };
            await _stockMovementRepository.AddAsync(inMovement);

            await _unitOfWork.SaveChangesAsync();

            var result = new ResultTransferDto
            {
                ProductName = product.Name,
                SourceWarehouseId = dto.SourceWarehouseId,
                SourceWarehouseName = sourceWarehouse.Name,
                SourceRemainingQuantity = sourceStock.Quantity,
                TargetWarehouseId = dto.TargetWarehouseId,
                TargetWarehouseName = targetWarehouse.Name,
                TargetNewQuantity = targetStock.Quantity,
                TransferredQuantity = dto.Quantity
            };

            return BaseResult<ResultTransferDto>.Success(result);
        }
    }
}