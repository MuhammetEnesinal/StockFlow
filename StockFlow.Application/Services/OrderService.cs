using FluentValidation;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.OrderDtos;
using StockFlow.Application.Interfaces.Repositories;
using StockFlow.Application.Interfaces.Services;
using StockFlow.Application.Interfaces.UOW;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Enums;

namespace StockFlow.Application.Services
{
    public class OrderService(
        IGenericRepository<Order> _orderRepository,
        IGenericRepository<OrderItem> _orderItemRepository,
        IGenericRepository<Product> _productRepository,
        IGenericRepository<Warehouse> _warehouseRepository,
        IGenericRepository<Customer> _customerRepository,
        IGenericRepository<Stock> _stockRepository,
        IGenericRepository<StockMovement> _stockMovementRepository,
        IUnitOfWork _unitOfWork,
        IMapper _mapper,
        IValidator<CreateOrderDto> _createValidator,
        IValidator<OrderItemRequestDto> _orderItemValidator,
        ICurrentUserService _currentUserService) : IOrderService
    {


        public async Task<BaseResult<ResultOrderDto>> CreateAsync(CreateOrderDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BaseResult<ResultOrderDto>.Fail(validationResult.Errors);
            }

            var userId = _currentUserService.GetUserId();

            var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
            if (customer == null)
            {
                return BaseResult<ResultOrderDto>.Fail("Müşteri bulunamadı.", ResultErrorType.NotFound);
            }

            var reservedPerSource = new Dictionary<(int ProductId, int WarehouseId), int>();
            var stockCache = new Dictionary<(int ProductId, int WarehouseId), Stock>();
            var productCache = new Dictionary<int, Product>();
            var warehouseCache = new Dictionary<int, Warehouse>();

            foreach (var item in dto.OrderItems)
            {
                var itemValidationResult = await _orderItemValidator.ValidateAsync(item);
                if (!itemValidationResult.IsValid)
                {
                    return BaseResult<ResultOrderDto>.Fail(itemValidationResult.Errors);
                }

                if (!productCache.TryGetValue(item.ProductId, out var product))
                {
                    product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product == null)
                    {
                        return BaseResult<ResultOrderDto>.Fail($"Ürün bulunamadı. ProductId: {item.ProductId}", ResultErrorType.NotFound);
                    }

                    productCache[item.ProductId] = product;
                }

                if (!warehouseCache.TryGetValue(item.WarehouseId, out var warehouse))
                {
                    warehouse = await _warehouseRepository.GetByIdAsync(item.WarehouseId);
                    if (warehouse == null)
                    {
                        return BaseResult<ResultOrderDto>.Fail($"Depo bulunamadı. WarehouseId: {item.WarehouseId}", ResultErrorType.NotFound);
                    }

                    warehouseCache[item.WarehouseId] = warehouse;
                }

                var key = (item.ProductId, item.WarehouseId);

                if (!stockCache.TryGetValue(key, out var stock))
                {
                    stock = await _stockRepository.QueryForUpdate()
                        .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == item.WarehouseId);

                    if (stock == null)
                    {
                        return BaseResult<ResultOrderDto>.Fail($"Ürün: {product.Name}, Depo: {warehouse.Name} için stok bulunamadı", ResultErrorType.NotFound);
                    }

                    stockCache[key] = stock;
                }

                var alreadyReserved = reservedPerSource.TryGetValue(key, out var reserved) ? reserved : 0;

                if ((stock.Quantity - alreadyReserved) < item.Quantity)
                {
                    return BaseResult<ResultOrderDto>.Fail(
                        $"Yetersiz stok. Ürün: {product.Name}, Depo: {warehouse.Name}, Mevcut: {stock.Quantity - alreadyReserved}, İstenen: {item.Quantity}",
                        ResultErrorType.BusinessRule);
                }

                reservedPerSource[key] = alreadyReserved + item.Quantity;
            }

            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
            var order = new Order
            {
                OrderNumber = orderNumber,
                TotalAmount = 0,
                CreatedByUserId = userId,
                CustomerId = dto.CustomerId,
                Status = OrderStatus.Pending
            };

            decimal totalAmount = 0;
            var resultItems = new List<ResultOrderItemDto>();

            foreach (var item in dto.OrderItems)
            {
                var product = productCache[item.ProductId];
                var warehouse = warehouseCache[item.WarehouseId];

                var key = (item.ProductId, item.WarehouseId);
                var stock = stockCache[key];
                stock.Quantity -= item.Quantity;

                var orderItem = new OrderItem
                {
                    Order = order,
                    ProductId = item.ProductId,
                    WarehouseId = item.WarehouseId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };
                await _orderItemRepository.AddAsync(orderItem);

                var movement = new StockMovement
                {
                    ProductId = item.ProductId,
                    WarehouseId = item.WarehouseId,
                    Type = StockMovementType.Sale,
                    Quantity = -item.Quantity,
                    Note = $"Sipariş satışı - {orderNumber}",
                    Order = order,
                    PerformedByUserId = userId
                };
                await _stockMovementRepository.AddAsync(movement);

                totalAmount += item.Quantity * product.Price;
                resultItems.Add(new ResultOrderItemDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    WarehouseId = warehouse.Id,
                    WarehouseName = warehouse.Name,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price

                });
            }

            order.TotalAmount = totalAmount;
            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            var result = new ResultOrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                CustomerId = customer.Id,
                CustomerFullName = customer.FullName,
                CustomerEmail = customer.Email,
                CustomerPhoneNumber = customer.PhoneNumber,
                CreatedByUserId = userId,
                CreatedByUserFullName = _currentUserService.GetUserName(),
                CreatedByUserEmployeeCode = _currentUserService.GetEmployeeCode(),
                OrderItems = resultItems
            };

            return BaseResult<ResultOrderDto>.Success(result);
        }

        public async Task<BaseResult<PagedResult<ResultOrderDto>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var paged = await _orderRepository.GetAllAsync(pageNumber, pageSize,
                include: q => q
                    .Include(o => o.Customer)
                    .Include(o => o.CreatedByUser)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Warehouse));
            var result = _mapper.Map<PagedResult<ResultOrderDto>>(paged);

            return BaseResult<PagedResult<ResultOrderDto>>.Success(result);
        }

        public async Task<BaseResult<ResultOrderDto>> GetByIdAsync(int id)
        {
            var order = await _orderRepository.Query()
                .Include(o => o.Customer)
                .Include(o => o.CreatedByUser)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Warehouse)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return BaseResult<ResultOrderDto>.Fail("Sipariş bulunamadı.", ResultErrorType.NotFound);
            }

            var resultOrderDto = _mapper.Map<ResultOrderDto>(order);
            return BaseResult<ResultOrderDto>.Success(resultOrderDto);
        }

        public Task<BaseResult<ResultOrderDto>> ConfirmAsync(int id)
        {
            return TransitionAsync(id, OrderStatus.Pending, OrderStatus.Confirmed, "onaylanabilir");
        }

        public Task<BaseResult<ResultOrderDto>> PrepareAsync(int id)
        {
            return TransitionAsync(id, OrderStatus.Confirmed, OrderStatus.Preparing, "hazırlanabilir");
        }

        public Task<BaseResult<ResultOrderDto>> ShipAsync(int id)
        {
            return TransitionAsync(id, OrderStatus.Preparing, OrderStatus.Shipped, "kargoya verilebilir");
        }

        public Task<BaseResult<ResultOrderDto>> DeliverAsync(int id)
        {
            return TransitionAsync(id, OrderStatus.Shipped, OrderStatus.Delivered, "teslim edilebilir");
        }

        public async Task<BaseResult<bool>> CancelAsync(int id)
        {
            var order = await _orderRepository.QueryForUpdate()
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return BaseResult<bool>.Fail("Sipariş bulunamadı.", ResultErrorType.NotFound);
            }

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed && order.Status != OrderStatus.Preparing)
            {
                return BaseResult<bool>.Fail(
                    $"Sadece Pending, Confirmed veya Preparing durumundaki siparişler iptal edilebilir. Mevcut durum: {order.Status}",
                    ResultErrorType.BusinessRule);
            }

            var userId = _currentUserService.GetUserId();

            foreach (var orderItem in order.OrderItems)
            {
                var stock = await _stockRepository.QueryForUpdate()
                    .FirstOrDefaultAsync(s => s.ProductId == orderItem.ProductId && s.WarehouseId == orderItem.WarehouseId);

                if (stock == null)
                {
                    return BaseResult<bool>.Fail($"Stok kaydı bulunamadı. ProductId: {orderItem.ProductId}, WarehouseId: {orderItem.WarehouseId}", ResultErrorType.NotFound);
                }

                stock.Quantity += orderItem.Quantity;

                var stockMovement = new StockMovement
                {
                    ProductId = orderItem.ProductId,
                    WarehouseId = orderItem.WarehouseId,
                    Type = StockMovementType.Adjustment,
                    Quantity = orderItem.Quantity,
                    Note = $"Sipariş iptali nedeniyle stok iadesi - {order.OrderNumber}",
                    OrderId = order.Id,
                    PerformedByUserId = userId
                };

                await _stockMovementRepository.AddAsync(stockMovement);
            }

            order.Status = OrderStatus.Cancelled;
            await _unitOfWork.SaveChangesAsync();

            return BaseResult<bool>.Success(true);
        }

        private async Task<BaseResult<ResultOrderDto>> TransitionAsync(int id, OrderStatus expectedStatus, OrderStatus newStatus, string actionText)
        {
            var order = await _orderRepository.QueryForUpdate()
                .Include(o => o.Customer)
                .Include(o => o.CreatedByUser)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Warehouse)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return BaseResult<ResultOrderDto>.Fail("Sipariş bulunamadı.", ResultErrorType.NotFound);
            }

            if (order.Status != expectedStatus)
            {
                return BaseResult<ResultOrderDto>.Fail(
                    $"Sadece {expectedStatus} durumundaki sipariş {actionText}. Mevcut durum: {order.Status}",
                    ResultErrorType.BusinessRule);
            }

            order.Status = newStatus;
            await _unitOfWork.SaveChangesAsync();

            var resultOrderDto = _mapper.Map<ResultOrderDto>(order);
            return BaseResult<ResultOrderDto>.Success(resultOrderDto);
        }
    }
}