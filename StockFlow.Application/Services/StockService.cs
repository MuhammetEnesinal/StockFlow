using FluentValidation;
using MapsterMapper;
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
        IMapper _mapper,
        IValidator<StockInDto> _stockInValidator,
        IValidator<StockOutDto> _stockOutValidator,
        IValidator<TransferDto> _transferValidator,
        IValidator<TransferBatchDto> _transferBatchValidator,
        ICurrentUserService _currentUserService) : IStockService
    {


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
                PerformedByUserId = _currentUserService.GetUserId()
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
                return BaseResult<ResultStockDto>.Fail(
                    $"Ürün: {product.Name}, Depo: {warehouse.Name} için stok bulunamadı",
                    ResultErrorType.NotFound);
            }

            if (stock.Quantity < stockOutDto.Quantity)
            {
                return BaseResult<ResultStockDto>.Fail(
                    $"Yetersiz stok. Ürün: {product.Name}, Depo: {warehouse.Name}, Mevcut: {stock.Quantity}, İstenen: {stockOutDto.Quantity}",
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
                PerformedByUserId = _currentUserService.GetUserId()
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

        public async Task<BaseResult<PagedResult<ResultStockDto>>> GetStockByWarehouseAsync(int warehouseId, int pageNumber, int pageSize)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
            if (warehouse == null)
            {
                return BaseResult<PagedResult<ResultStockDto>>.Fail("Depo bulunamadı", ResultErrorType.NotFound);
            }

            var paged = await _stockRepository.GetAllAsync(pageNumber, pageSize,
                filter: s => s.WarehouseId == warehouseId,
                include: q => q
                    .Include(s => s.Product)
                    .Include(s => s.Warehouse));
            var result = _mapper.Map<PagedResult<ResultStockDto>>(paged);

            return BaseResult<PagedResult<ResultStockDto>>.Success(result);
        }

        public async Task<BaseResult<PagedResult<ResultStockMovementDto>>> GetMovementsAsync(int productId, int warehouseId, int pageNumber, int pageSize)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return BaseResult<PagedResult<ResultStockMovementDto>>.Fail("Ürün bulunamadı", ResultErrorType.NotFound);

            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
            if (warehouse == null)
                return BaseResult<PagedResult<ResultStockMovementDto>>.Fail("Depo bulunamadı", ResultErrorType.NotFound);

            var paged = await _stockMovementRepository.GetAllAsync(pageNumber, pageSize,
                filter: m => m.ProductId == productId && m.WarehouseId == warehouseId,
                include: q => q
                    .Include(m => m.PerformedByUser)
                    .Include(m => m.Product)
                    .Include(m => m.Warehouse),
                orderBy: q => q.OrderByDescending(m => m.CreateAtTime));
            var result = _mapper.Map<PagedResult<ResultStockMovementDto>>(paged);

            return BaseResult<PagedResult<ResultStockMovementDto>>.Success(result);
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
                return BaseResult<ResultTransferDto>.Fail(
                    $"Ürün: {product.Name}, Kaynak Depo: {sourceWarehouse.Name} için stok bulunamadı",
                    ResultErrorType.NotFound);
            }

            if (sourceStock.Quantity < dto.Quantity)
            {
                return BaseResult<ResultTransferDto>.Fail(
                    $"Yetersiz stok. Ürün: {product.Name}, Depo: {sourceWarehouse.Name}, Mevcut: {sourceStock.Quantity}, İstenen: {dto.Quantity}",
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
                PerformedByUserId = _currentUserService.GetUserId()
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
                PerformedByUserId = _currentUserService.GetUserId()
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

        public async Task<BaseResult<IEnumerable<ResultTransferDto>>> TransferBatchAsync(TransferBatchDto transferBatchDto)
        {
            var resultValidation = await _transferBatchValidator.ValidateAsync(transferBatchDto);
            if (!resultValidation.IsValid)
            {
                return BaseResult<IEnumerable<ResultTransferDto>>.Fail(resultValidation.Errors);
            }

            var reservedPerSource = new Dictionary<(int ProductId, int WarehouseId), int>();

            foreach (var transfer in transferBatchDto.Transfers)
            {
                transfer.Note = string.IsNullOrWhiteSpace(transfer.Note) ? null : transfer.Note.Trim();

                var transferValidationResult = await _transferValidator.ValidateAsync(transfer);
                if (!transferValidationResult.IsValid)
                {
                    return BaseResult<IEnumerable<ResultTransferDto>>.Fail(transferValidationResult.Errors);
                }

                var product = await _productRepository.GetByIdAsync(transfer.ProductId);
                if (product == null)
                {
                    return BaseResult<IEnumerable<ResultTransferDto>>.Fail("Ürün bulunamadı", ResultErrorType.NotFound);
                }

                var warehouse = await _warehouseRepository.GetByIdAsync(transfer.SourceWarehouseId);
                if (warehouse == null)
                {
                    return BaseResult<IEnumerable<ResultTransferDto>>.Fail("Kaynak depo bulunamadı", ResultErrorType.NotFound);
                }

                var targetWarehouse = await _warehouseRepository.GetByIdAsync(transfer.TargetWarehouseId);
                if (targetWarehouse == null)
                {
                    return BaseResult<IEnumerable<ResultTransferDto>>.Fail("Hedef depo bulunamadı", ResultErrorType.NotFound);
                }

                var sourceStock = await _stockRepository.Query()
                    .FirstOrDefaultAsync(s => s.ProductId == transfer.ProductId && s.WarehouseId == transfer.SourceWarehouseId);

                if (sourceStock == null)
                {
                    return BaseResult<IEnumerable<ResultTransferDto>>.Fail(
                        $"Ürün: {product.Name}, Kaynak Depo: {warehouse.Name} için stok bulunamadı",
                        ResultErrorType.NotFound);
                }

                var key = (transfer.ProductId, transfer.SourceWarehouseId);
                var alreadyReserved = reservedPerSource.TryGetValue(key, out var reserved) ? reserved : 0;

                if ((sourceStock.Quantity - alreadyReserved) < transfer.Quantity)
                {
                    return BaseResult<IEnumerable<ResultTransferDto>>.Fail(
                        $"Yetersiz stok. Ürün: {product.Name}, Depo: {warehouse.Name}, Mevcut: {sourceStock.Quantity - alreadyReserved}, İstenen: {transfer.Quantity}",
                        ResultErrorType.BusinessRule);
                }

                reservedPerSource[key] = alreadyReserved + transfer.Quantity;
            }

            var resultList = new List<ResultTransferDto>();
            var stockCache = new Dictionary<(int ProductId, int WarehouseId), Stock>();
            var newStockKeys = new HashSet<(int ProductId, int WarehouseId)>();   

            foreach (var transfer in transferBatchDto.Transfers)
            {
                var product = await _productRepository.GetByIdAsync(transfer.ProductId);
                var sourceWarehouse = await _warehouseRepository.GetByIdAsync(transfer.SourceWarehouseId);
                var targetWarehouse = await _warehouseRepository.GetByIdAsync(transfer.TargetWarehouseId);

                var sourceKey = (transfer.ProductId, transfer.SourceWarehouseId);
                if (!stockCache.TryGetValue(sourceKey, out var sourceStock))
                {
                    sourceStock = await _stockRepository.Query()
                        .FirstOrDefaultAsync(s => s.ProductId == transfer.ProductId && s.WarehouseId == transfer.SourceWarehouseId);
                    stockCache[sourceKey] = sourceStock;                         
                }
                sourceStock.Quantity -= transfer.Quantity;
                _stockRepository.Update(sourceStock);                            

                var targetKey = (transfer.ProductId, transfer.TargetWarehouseId);
                if (!stockCache.TryGetValue(targetKey, out var targetStock))
                {
                    targetStock = await _stockRepository.Query()
                        .FirstOrDefaultAsync(s => s.ProductId == transfer.ProductId && s.WarehouseId == transfer.TargetWarehouseId);

                    if (targetStock == null)
                    {
                        targetStock = new Stock
                        {
                            ProductId = transfer.ProductId,
                            WarehouseId = transfer.TargetWarehouseId,
                            Quantity = 0
                        };
                        await _stockRepository.AddAsync(targetStock);
                        newStockKeys.Add(targetKey);                             
                    }
                    stockCache[targetKey] = targetStock;                       
                }
                targetStock.Quantity += transfer.Quantity;
                if (!newStockKeys.Contains(targetKey))
                {
                    _stockRepository.Update(targetStock);                        
                }

                var transferGroupId = Guid.NewGuid();

                var outMovement = new StockMovement
                {
                    ProductId = transfer.ProductId,
                    WarehouseId = transfer.SourceWarehouseId,
                    Type = StockMovementType.TransferOut,
                    Quantity = -transfer.Quantity,
                    Note = transfer.Note,
                    TransferGroupId = transferGroupId,
                    PerformedByUserId = _currentUserService.GetUserId()
                };
                await _stockMovementRepository.AddAsync(outMovement);

                var inMovement = new StockMovement
                {
                    ProductId = transfer.ProductId,
                    WarehouseId = transfer.TargetWarehouseId,
                    Type = StockMovementType.TransferIn,
                    Quantity = transfer.Quantity,
                    Note = transfer.Note,
                    TransferGroupId = transferGroupId,
                    PerformedByUserId = _currentUserService.GetUserId()
                };
                await _stockMovementRepository.AddAsync(inMovement);

                resultList.Add(new ResultTransferDto
                {
                    ProductName = product.Name,
                    SourceWarehouseId = transfer.SourceWarehouseId,
                    SourceWarehouseName = sourceWarehouse.Name,
                    SourceRemainingQuantity = sourceStock.Quantity,
                    TargetWarehouseId = transfer.TargetWarehouseId,
                    TargetWarehouseName = targetWarehouse.Name,
                    TargetNewQuantity = targetStock.Quantity,
                    TransferredQuantity = transfer.Quantity
                });
            }

            await _unitOfWork.SaveChangesAsync();

            return BaseResult<IEnumerable<ResultTransferDto>>.Success(resultList);
        }

        public async Task<BaseResult<PagedResult<ResultStockMovementDto>>> GetMovementsByWarehouseAsync(int warehouseId, int pageNumber, int pageSize)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
            if (warehouse == null)
                return BaseResult<PagedResult<ResultStockMovementDto>>.Fail("Depo bulunamadı", ResultErrorType.NotFound);

            var paged = await _stockMovementRepository.GetAllAsync(pageNumber, pageSize,
                filter: m => m.WarehouseId == warehouseId,
                include: q => q
                    .Include(m => m.PerformedByUser)
                    .Include(m => m.Product)
                    .Include(m => m.Warehouse),
                orderBy: q => q.OrderByDescending(m => m.CreateAtTime));
            var result = _mapper.Map<PagedResult<ResultStockMovementDto>>(paged);

            return BaseResult<PagedResult<ResultStockMovementDto>>.Success(result);
        }
    }
}
