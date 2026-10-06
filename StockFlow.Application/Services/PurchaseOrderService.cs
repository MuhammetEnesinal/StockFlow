using FluentValidation;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.PurchaseOrderDtos;
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
    public class PurchaseOrderService(
        IGenericRepository<PurchaseOrder> _purchaseOrderRepository,
        IUnitOfWork _unitOfWork,
        IGenericRepository<Supplier> _supplierRepository,
        IGenericRepository<PurchaseOrderItem> _purchaseOrderItemRepository,
        IGenericRepository<User> _userRepository,
        IGenericRepository<Stock> _stockRepository,
        IGenericRepository<StockMovement> _stockMovementRepository,
        IGenericRepository<Product> _productRepository,
        IGenericRepository<Warehouse> _warehouseRepository,
        IMapper _mapper,
        IValidator<ReceiveDto> _receiveValidator,
        IValidator<CreatePurchaseOrderDto> _createPurchaseOrderValidator,
        IValidator<ReceiveItemDto> _receiveItemValidator,
        IValidator<PurchaseOrderItemRequestDto> _purchaseOrderItemRequestValidator,
        ICurrentUserService _currentUserService) : IPurchaseOrderService
    {
        public async Task<BaseResult<bool>> CancelAsync(int id)
        {
            var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(id);
            if (purchaseOrder == null)
            {
                return BaseResult<bool>.Fail("Satın alma siparişi bulunamadı", ResultErrorType.NotFound);
            }

            if (purchaseOrder.Status == PurchaseOrderStatus.Received || purchaseOrder.Status == PurchaseOrderStatus.Cancelled)
            {
                return BaseResult<bool>.Fail(
                 $"Sipariş ({purchaseOrder.PurchaseOrderNumber}) zaten teslim edilmiş veya iptal edilmiş, tekrar iptal edilemez",
                 ResultErrorType.BusinessRule);
            }

            purchaseOrder.Status = PurchaseOrderStatus.Cancelled;

            _purchaseOrderRepository.Update(purchaseOrder);
            await _unitOfWork.SaveChangesAsync();
            return BaseResult<bool>.Success(true);
        }

        public async Task<BaseResult<ResultPurchaseOrderDto>> CreateAsync(CreatePurchaseOrderDto createPurchaseOrderDto)
        {
            var validateResult = await _createPurchaseOrderValidator.ValidateAsync(createPurchaseOrderDto);
            if (!validateResult.IsValid)
            {
                return BaseResult<ResultPurchaseOrderDto>.Fail(validateResult.Errors);
            }

            var supplier = await _supplierRepository.GetByIdAsync(createPurchaseOrderDto.SupplierId);
            if (supplier == null)
            {
                return BaseResult<ResultPurchaseOrderDto>.Fail("Tedarikçi bulunamadı", ResultErrorType.NotFound);
            }

            var warehouse = await _warehouseRepository.GetByIdAsync(createPurchaseOrderDto.WarehouseId);
            if (warehouse == null)
            {
                return BaseResult<ResultPurchaseOrderDto>.Fail("Depo bulunamadı", ResultErrorType.NotFound);
            }

            foreach (var item in createPurchaseOrderDto.Items)
            {
                var itemsvalidateResult = await _purchaseOrderItemRequestValidator.ValidateAsync(item);
                if (!itemsvalidateResult.IsValid)
                {
                    return BaseResult<ResultPurchaseOrderDto>.Fail(itemsvalidateResult.Errors);
                }

                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    return BaseResult<ResultPurchaseOrderDto>.Fail("Ürün bulunamadı, lütfen önce bu ürünü sisteme ekleyiniz", ResultErrorType.NotFound);
                }
            }

            var purchaseOrderNumber = $"PO-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
            var purchaseOrder = new PurchaseOrder
            {
                PurchaseOrderNumber = purchaseOrderNumber,
                Status = PurchaseOrderStatus.Draft,
                SupplierId = createPurchaseOrderDto.SupplierId,
                WarehouseId = createPurchaseOrderDto.WarehouseId,
                CreatedByUserId = _currentUserService.GetUserId(),
                SentAt = null,
                ReceivedAt = null
            };

            var createdItems = new List<(PurchaseOrderItem Item, Product Product)>();

            foreach (var item in createPurchaseOrderDto.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);

                var purchaseOrderItem = new PurchaseOrderItem
                {
                    PurchaseOrder = purchaseOrder,
                    ProductId = item.ProductId,
                    OrderedQuantity = item.OrderedQuantity,
                    ReceivedQuantity = 0,
                    UnitPrice = product.Price
                };
                await _purchaseOrderItemRepository.AddAsync(purchaseOrderItem);

                createdItems.Add((purchaseOrderItem, product));
            }

            await _purchaseOrderRepository.AddAsync(purchaseOrder);
            await _unitOfWork.SaveChangesAsync();

            var currentUser = await _userRepository.GetByIdAsync(_currentUserService.GetUserId());
            if (currentUser == null)
            {
                return BaseResult<ResultPurchaseOrderDto>.Fail("Oturum açan kullanıcı bulunamadı, lütfen tekrar giriş yapın.", ResultErrorType.Unauthorized);
            }

            var resultItems = createdItems.Select(x => new ResultPurchaseOrderItemDto
            {
                Id = x.Item.Id,
                ProductId = x.Product.Id,
                ProductName = x.Product.Name,
                OrderedQuantity = x.Item.OrderedQuantity,
                ReceivedQuantity = x.Item.ReceivedQuantity,
                UnitPrice = x.Item.UnitPrice
            }).ToList();

            var resultDto = new ResultPurchaseOrderDto
            {
                Id = purchaseOrder.Id,
                PurchaseOrderNumber = purchaseOrder.PurchaseOrderNumber,
                Status = purchaseOrder.Status.ToString(),
                SupplierId = supplier.Id,
                SupplierName = supplier.Name,
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Name,
                CreatedByUserId = currentUser.Id,
                CreatedByUserFullName = currentUser.FullName,
                CreatedByUserEmployeeCode = currentUser.EmployeeCode,
                SentAt = purchaseOrder.SentAt,
                ReceivedAt = purchaseOrder.ReceivedAt,
                Items = resultItems
            };

            return BaseResult<ResultPurchaseOrderDto>.Success(resultDto);
        }

        public async Task<BaseResult<PagedResult<ResultPurchaseOrderDto>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var paged = await _purchaseOrderRepository.GetAllAsync(pageNumber, pageSize,
                include: q => q
                    .Include(po => po.Supplier)
                    .Include(po => po.Warehouse)
                    .Include(po => po.CreatedByUser)
                    .Include(po => po.Items)
                        .ThenInclude(poi => poi.Product));
            var result = _mapper.Map<PagedResult<ResultPurchaseOrderDto>>(paged);

            return BaseResult<PagedResult<ResultPurchaseOrderDto>>.Success(result);
        }

        public async Task<BaseResult<ResultPurchaseOrderDto>> GetByIdAsync(int id)
        {
            var purchaseOrder = await _purchaseOrderRepository.Query()
                                .Include(po => po.Supplier)
                                .Include(po => po.Warehouse)
                                .Include(po => po.Items)
                                     .ThenInclude(poi => poi.Product)
                                .Include(po => po.CreatedByUser)
                                .FirstOrDefaultAsync(po => po.Id == id);

            if (purchaseOrder == null)
            {
                return BaseResult<ResultPurchaseOrderDto>.Fail("Satın alma siparişi bulunamadı", ResultErrorType.NotFound);
            }

            var resultDto = _mapper.Map<ResultPurchaseOrderDto>(purchaseOrder);
            return BaseResult<ResultPurchaseOrderDto>.Success(resultDto);
        }

        public async Task<BaseResult<PagedResult<ResultPurchaseOrderDto>>> GetBySupplierAsync(int supplierId, int pageNumber, int pageSize)
        {
            var supplier = await _supplierRepository.GetByIdAsync(supplierId);
            if (supplier == null)
            {
                return BaseResult<PagedResult<ResultPurchaseOrderDto>>.Fail("Tedarikçi bulunamadı", ResultErrorType.NotFound);
            }

            var paged = await _purchaseOrderRepository.GetAllAsync(pageNumber, pageSize,
                filter: po => po.SupplierId == supplierId,
                include: q => q
                    .Include(po => po.Supplier)
                    .Include(po => po.Warehouse)
                    .Include(po => po.CreatedByUser)
                    .Include(po => po.Items)
                        .ThenInclude(poi => poi.Product));
            var result = _mapper.Map<PagedResult<ResultPurchaseOrderDto>>(paged);

            return BaseResult<PagedResult<ResultPurchaseOrderDto>>.Success(result);
        }

        public async Task<BaseResult<ResultPurchaseOrderDto>> ReceiveAsync(int id, ReceiveDto receiveDto)
        {
            var validateResult = await _receiveValidator.ValidateAsync(receiveDto);
            if (!validateResult.IsValid)
            {
                return BaseResult<ResultPurchaseOrderDto>.Fail(validateResult.Errors);
            }

            var purchaseOrder = await _purchaseOrderRepository.Query()
                .Include(po => po.Supplier)
                .Include(po => po.Warehouse)
                .Include(po => po.Items)
                    .ThenInclude(poi => poi.Product)
                .Include(po => po.CreatedByUser)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (purchaseOrder == null)
            {
                return BaseResult<ResultPurchaseOrderDto>.Fail("Satın alma siparişi bulunamadı", ResultErrorType.NotFound);
            }

            if (purchaseOrder.Status == PurchaseOrderStatus.Received)
            {
                return BaseResult<ResultPurchaseOrderDto>.Fail(
                    $"Sipariş ({purchaseOrder.PurchaseOrderNumber}) zaten tamamen teslim alınmış",
                    ResultErrorType.BusinessRule);
            }
            else if (purchaseOrder.Status == PurchaseOrderStatus.Cancelled)
            {
                return BaseResult<ResultPurchaseOrderDto>.Fail(
                    $"Sipariş ({purchaseOrder.PurchaseOrderNumber}) iptal edilmiş, teslim alınamaz",
                    ResultErrorType.BusinessRule);
            }
            else if (purchaseOrder.Status == PurchaseOrderStatus.Draft)
            {
                return BaseResult<ResultPurchaseOrderDto>.Fail(
                    $"Sipariş ({purchaseOrder.PurchaseOrderNumber}) henüz tedarikçiye gönderilmedi, önce gönderin",
                    ResultErrorType.BusinessRule);
            }

            foreach (var receiveItem in receiveDto.Items)
            {
                var validResult = await _receiveItemValidator.ValidateAsync(receiveItem);
                if (!validResult.IsValid)
                {
                    return BaseResult<ResultPurchaseOrderDto>.Fail(validResult.Errors);
                }

                var purchaseOrderItem = purchaseOrder.Items.FirstOrDefault(x => x.Id == receiveItem.PurchaseOrderItemId);
                if (purchaseOrderItem == null)
                {
                    return BaseResult<ResultPurchaseOrderDto>.Fail(
                        $"Sipariş kalemi bulunamadı. PurchaseOrderItemId: {receiveItem.PurchaseOrderItemId}",
                        ResultErrorType.NotFound);
                }
                var newReceivedTotal = purchaseOrderItem.ReceivedQuantity + receiveItem.ReceivedQuantity;
                if (newReceivedTotal > purchaseOrderItem.OrderedQuantity)
                {
                    return BaseResult<ResultPurchaseOrderDto>.Fail(
                        $"Sipariş edilenden fazla teslim alınamaz. Ürün: {purchaseOrderItem.Product.Name}, Sipariş edilen: {purchaseOrderItem.OrderedQuantity}, Şu ana kadar gelen: {purchaseOrderItem.ReceivedQuantity}, Bu teslimatta istenen: {receiveItem.ReceivedQuantity}",
                        ResultErrorType.BusinessRule);
                }
            }

            foreach (var receiveItem in receiveDto.Items)
            {
                var purchaseOrderItem = purchaseOrder.Items.FirstOrDefault(x => x.Id == receiveItem.PurchaseOrderItemId);
                purchaseOrderItem.ReceivedQuantity += receiveItem.ReceivedQuantity;
                _purchaseOrderItemRepository.Update(purchaseOrderItem);   // <-- YENİ: kalem izlenmiyor, ayrıca kaydedilmeli

                var stock = await _stockRepository.Query()
                    .FirstOrDefaultAsync(s => s.ProductId == purchaseOrderItem.ProductId && s.WarehouseId == purchaseOrder.WarehouseId);

                if (stock == null)
                {
                    stock = new Stock
                    {
                        ProductId = purchaseOrderItem.ProductId,
                        WarehouseId = purchaseOrder.WarehouseId,
                        Quantity = receiveItem.ReceivedQuantity
                    };
                    await _stockRepository.AddAsync(stock);
                }
                else
                {
                    stock.Quantity += receiveItem.ReceivedQuantity;
                    _stockRepository.Update(stock);
                }

                var movement = new StockMovement
                {
                    ProductId = purchaseOrderItem.ProductId,
                    WarehouseId = purchaseOrder.WarehouseId,
                    Type = StockMovementType.Purchase,
                    Quantity = receiveItem.ReceivedQuantity,
                    Note = $"Satın alma teslimatı - {purchaseOrder.PurchaseOrderNumber}",
                    PurchaseOrderId = purchaseOrder.Id,
                    PerformedByUserId = _currentUserService.GetUserId()
                };
                await _stockMovementRepository.AddAsync(movement);
            }

            bool allComplete = purchaseOrder.Items.All(i => i.ReceivedQuantity >= i.OrderedQuantity);
            if (allComplete)
            {
                purchaseOrder.Status = PurchaseOrderStatus.Received;
                purchaseOrder.ReceivedAt = DateTime.UtcNow;
            }
            else
            {
                purchaseOrder.Status = PurchaseOrderStatus.PartiallyReceived;
            }

            _purchaseOrderRepository.Update(purchaseOrder);
            await _unitOfWork.SaveChangesAsync();

            var resultDto = _mapper.Map<ResultPurchaseOrderDto>(purchaseOrder);
            return BaseResult<ResultPurchaseOrderDto>.Success(resultDto);
        }

        public async Task<BaseResult<ResultPurchaseOrderDto>> SendAsync(int id)
        {
            var purchaseOrder = await _purchaseOrderRepository.Query()
                                 .Include(po => po.Supplier)
                                 .Include(po => po.Warehouse)
                                 .Include(po => po.Items)
                                      .ThenInclude(poi => poi.Product)
                                 .Include(po => po.CreatedByUser)
                                 .FirstOrDefaultAsync(po => po.Id == id);

            if (purchaseOrder == null)
            {
                return BaseResult<ResultPurchaseOrderDto>.Fail("Satın alma siparişi bulunamadı", ResultErrorType.NotFound);
            }

            if (purchaseOrder.Status != PurchaseOrderStatus.Draft)
            {
                return BaseResult<ResultPurchaseOrderDto>.Fail("Sadece taslak durumundaki siparişler gönderilebilir", ResultErrorType.BusinessRule);
            }

            purchaseOrder.Status = PurchaseOrderStatus.Sent;
            purchaseOrder.SentAt = DateTime.UtcNow;
            _purchaseOrderRepository.Update(purchaseOrder);
            await _unitOfWork.SaveChangesAsync();

            var resultDto = _mapper.Map<ResultPurchaseOrderDto>(purchaseOrder);
            return BaseResult<ResultPurchaseOrderDto>.Success(resultDto);
        }
    }
}
