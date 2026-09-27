using FluentValidation;
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
        IGenericRepository<User> _userRepository,
        IUnitOfWork _unitOfWork,
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

            var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
            if (customer == null)
            {
                return BaseResult<ResultOrderDto>.Fail("Müşteri bulunamadı.", ResultErrorType.NotFound);
            }

            var reservedPerSource = new Dictionary<(int ProductId, int WarehouseId), int>();

            foreach (var item in dto.OrderItems)
            {
                var itemValidationResult = await _orderItemValidator.ValidateAsync(item);
                if (!itemValidationResult.IsValid)
                {
                    return BaseResult<ResultOrderDto>.Fail(itemValidationResult.Errors);
                }

                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    return BaseResult<ResultOrderDto>.Fail($"Ürün bulunamadı. ProductId: {item.ProductId}", ResultErrorType.NotFound);
                }

                var warehouse = await _warehouseRepository.GetByIdAsync(item.WarehouseId);
                if (warehouse == null)
                {
                    return BaseResult<ResultOrderDto>.Fail($"Depo bulunamadı. WarehouseId: {item.WarehouseId}", ResultErrorType.NotFound);
                }

                var stock = await _stockRepository.Query()
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == item.WarehouseId);

                if (stock == null)
                {
                    return BaseResult<ResultOrderDto>.Fail($"Ürün: {product.Name}, Depo: {warehouse.Name} için stok bulunamadı", ResultErrorType.NotFound);
                }

                var key = (item.ProductId, item.WarehouseId);
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
                CreatedByUserId = _currentUserService.GetUserId(),
                CustomerId = dto.CustomerId,
                Status = OrderStatus.Pending
            };

            decimal totalAmount = 0;
            var resultItems = new List<ResultOrderItemDto>();
            var stockCache = new Dictionary<(int ProductId, int WarehouseId), Stock>();

            foreach (var item in dto.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                var warehouse = await _warehouseRepository.GetByIdAsync(item.WarehouseId);

                var key = (item.ProductId, item.WarehouseId);
                if (!stockCache.TryGetValue(key, out var stock))
                {
                    stock = await _stockRepository.Query()
                        .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == item.WarehouseId);
                    _stockRepository.Update(stock);
                    stockCache[key] = stock;
                }
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
                    PerformedByUserId = _currentUserService.GetUserId()
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
                    UnitPrice = product.Price,
                    LineTotal = item.Quantity * product.Price
                });
            }

            order.TotalAmount = totalAmount;
            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            var currentUser = await _userRepository.GetByIdAsync(_currentUserService.GetUserId());
            if (currentUser == null)
            {
                return BaseResult<ResultOrderDto>.Fail("Oturum açan kullanıcı bulunamadı, lütfen tekrar giriş yapın.", ResultErrorType.Unauthorized);
            }
            

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
                CreatedByUserId = _currentUserService.GetUserId(),
                CreatedByUserName = currentUser.FullName,
                CreatedByEmployeeCode= currentUser.EmployeeCode,
                Items = resultItems
            };

            return BaseResult<ResultOrderDto>.Success(result);
        }

        public async Task<BaseResult<PagedResult<ResultOrderDto>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var query = _orderRepository.Query()
                .Include(o => o.Customer)
                .Include(o => o.CreatedByUser)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Warehouse);

            var totalCount = await query.CountAsync();

            var orders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var resultItems = new List<ResultOrderDto>();

            foreach (var order in orders)
            {
                var resultOrderDto = new ResultOrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    Status = order.Status.ToString(),
                    TotalAmount = order.TotalAmount,
                    CustomerId = order.CustomerId,
                    CustomerFullName = order.Customer.FullName,
                    CustomerEmail = order.Customer.Email,
                    CustomerPhoneNumber = order.Customer.PhoneNumber,
                    CreatedByUserId = order.CreatedByUserId,
                    CreatedByUserName = order.CreatedByUser.FullName,
                    CreatedByEmployeeCode = order.CreatedByUser.EmployeeCode,
                    Items = order.OrderItems.Select(oi => new ResultOrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        WarehouseId = oi.WarehouseId,
                        WarehouseName = oi.Warehouse.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        LineTotal = oi.Quantity * oi.UnitPrice
                    }).ToList()
                };
                resultItems.Add(resultOrderDto);
            }

            var result = new PagedResult<ResultOrderDto>
            {
                Items = resultItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

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

            var resultOrderDto = new ResultOrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                CustomerId = order.CustomerId,
                CustomerFullName = order.Customer.FullName,
                CustomerEmail = order.Customer.Email,
                CustomerPhoneNumber = order.Customer.PhoneNumber,
                CreatedByUserId = order.CreatedByUserId,
                CreatedByUserName = order.CreatedByUser.FullName,
                CreatedByEmployeeCode = order.CreatedByUser.EmployeeCode,
                Items = order.OrderItems.Select(oi => new ResultOrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    WarehouseId = oi.WarehouseId,
                    WarehouseName = oi.Warehouse.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    LineTotal = oi.Quantity * oi.UnitPrice
                }).ToList()
            };

            return BaseResult<ResultOrderDto>.Success(resultOrderDto);
        }

        public async Task<BaseResult<ResultOrderDto>> UpdateStatusAsync(int id, OrderStatus newStatus)
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


            if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Delivered)
            {
                return BaseResult<ResultOrderDto>.Fail("İptal edilmiş veya teslim edilmiş sipariş durumu değiştirilemez.", ResultErrorType.BusinessRule);
            }
            else if (order.Status == OrderStatus.Pending)
            {
                if (newStatus != OrderStatus.Confirmed && newStatus != OrderStatus.Cancelled)
                {
                    return BaseResult<ResultOrderDto>.Fail(
                        $"'Pending' durumundan sadece 'Confirmed' ya da 'Cancelled' durumuna geçilebilir.",
                        ResultErrorType.BusinessRule);
                }
            }
            else if (order.Status == OrderStatus.Confirmed)
            {
                if (newStatus != OrderStatus.Preparing && newStatus != OrderStatus.Cancelled)
                {
                    return BaseResult<ResultOrderDto>.Fail(
                        $"'Confirmed' durumundan sadece 'Preparing' ya da 'Cancelled' durumuna geçilebilir.",
                        ResultErrorType.BusinessRule);
                }
            }
            else if (order.Status == OrderStatus.Preparing)
            {
                if (newStatus != OrderStatus.Shipped && newStatus != OrderStatus.Cancelled)
                {
                    return BaseResult<ResultOrderDto>.Fail(
                        $"'Preparing' durumundan sadece 'Shipped' ya da 'Cancelled' durumuna geçilebilir.",
                        ResultErrorType.BusinessRule);
                }
            }
            else if (order.Status == OrderStatus.Shipped)
            {
                if (newStatus != OrderStatus.Delivered)
                {
                    return BaseResult<ResultOrderDto>.Fail(
                        $"'Shipped' durumundan sadece 'Delivered' durumuna geçilebilir.",
                        ResultErrorType.BusinessRule);
                }
            }
            


            order.Status = newStatus;
            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();

            var resultOrderDto = new ResultOrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                CustomerId = order.CustomerId,
                CustomerFullName = order.Customer.FullName,
                CustomerEmail = order.Customer.Email,
                CustomerPhoneNumber = order.Customer.PhoneNumber,
                CreatedByUserId = order.CreatedByUserId,
                CreatedByUserName = order.CreatedByUser.FullName,
                CreatedByEmployeeCode= order.CreatedByUser.EmployeeCode,
                Items = order.OrderItems.Select(oi => new ResultOrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    WarehouseId = oi.WarehouseId,
                    WarehouseName = oi.Warehouse.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    LineTotal = oi.Quantity * oi.UnitPrice
                }).ToList()
            };

            return BaseResult<ResultOrderDto>.Success(resultOrderDto);
        }

        public async Task<BaseResult<bool>> CancelAsync(int id)
        {
            var order = await _orderRepository.Query()
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return BaseResult<bool>.Fail("Sipariş bulunamadı.", ResultErrorType.NotFound);
            }

            if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Delivered)
            {
                return BaseResult<bool>.Fail("İptal edilmiş veya teslim edilmiş sipariş durumu değiştirilemez.", ResultErrorType.BusinessRule);
            }

            foreach (var orderItem in order.OrderItems)
            {
                var stock = await _stockRepository.Query()
                    .FirstOrDefaultAsync(s => s.ProductId == orderItem.ProductId && s.WarehouseId == orderItem.WarehouseId);

                if (stock == null)
                {
                    return BaseResult<bool>.Fail($"Stok kaydı bulunamadı. ProductId: {orderItem.ProductId}, WarehouseId: {orderItem.WarehouseId}", ResultErrorType.NotFound);
                }

                stock.Quantity += orderItem.Quantity;
                _stockRepository.Update(stock);

                var stockMovement = new StockMovement
                {
                    ProductId = orderItem.ProductId,
                    WarehouseId = orderItem.WarehouseId,
                    Type = StockMovementType.Adjustment,
                    Quantity = orderItem.Quantity,
                    Note = $"Sipariş iptali nedeniyle stok iadesi - {order.OrderNumber}",
                    OrderId = order.Id,
                    PerformedByUserId = _currentUserService.GetUserId()
                };

                await _stockMovementRepository.AddAsync(stockMovement);
            }

            order.Status = OrderStatus.Cancelled;
            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();

            return BaseResult<bool>.Success(true);
        }
    }
}