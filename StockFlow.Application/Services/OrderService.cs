using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.OrderDtos;
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
        IValidator<OrderItemRequestDto> _orderItemValidator) : IOrderService
     {
        private const int SeedUserId = 1;
        public async Task<BaseResult<bool>> CancelAsync(int id)
        {
            var order =await _orderRepository.Query()
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) {

                return BaseResult<bool>.Fail("Sipariş bulunamadı.", ResultErrorType.NotFound);
            }
            if (order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Delivered)
            {
                return BaseResult<bool>.Fail("İptal edilmiş veya teslim edilmiş sipariş durumu değiştirilemez.", ResultErrorType.BusinessRule);
            }

            foreach (var orderItem in order.OrderItems)
            {
                var stock = await _stockRepository.Query()
                    .FirstOrDefaultAsync(s => s.ProductId == orderItem.ProductId && s.WarehouseId == order.WarehouseId);

                if (stock == null) {
                  
                    return BaseResult<bool>.Fail($"Stok kaydı bulunamadı. ProductId: {orderItem.ProductId}", ResultErrorType.NotFound);
                }
                stock.Quantity += orderItem.Quantity;
                _stockRepository.Update(stock);

                var stockMovement=  new StockMovement
                {
                    ProductId = orderItem.ProductId,
                    WarehouseId = order.WarehouseId,
                    Type = StockMovementType.Adjustment,
                    Quantity = orderItem.Quantity,
                    Order = order,
                    PerformedByUserId = SeedUserId
                };

                await _stockMovementRepository.AddAsync(stockMovement);

            }
            order.Status = OrderStatus.Cancelled;
            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return BaseResult<bool>.Success(true);

        }

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

            var warehouse = await _warehouseRepository.GetByIdAsync(dto.WarehouseId);
            if (warehouse == null)
            {
                return BaseResult<ResultOrderDto>.Fail("Depo bulunamadı.", ResultErrorType.NotFound);
            }

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
                var stock = await _stockRepository.Query()
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == dto.WarehouseId);
                if (stock == null)
                {
                    return BaseResult<ResultOrderDto>.Fail($"Bu depoda ürün için stok bulunamadı. ProductId: {item.ProductId}", ResultErrorType.NotFound);
                }
                if (stock.Quantity < item.Quantity)
                {
                    return BaseResult<ResultOrderDto>.Fail($"Yetersiz stok. ProductId: {item.ProductId}, Mevcut: {stock.Quantity}, İstenen: {item.Quantity}", ResultErrorType.BusinessRule);
                }
            }

            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
            var order = new Order
            {
                OrderNumber = orderNumber,
                TotalAmount = 0,
                CreatedByUserId = SeedUserId,
                WarehouseId = dto.WarehouseId,
                CustomerId = dto.CustomerId,
                Status = OrderStatus.Pending
            };

            decimal totalAmount = 0;
            var resultItems = new List<ResultOrderItemDto>();

            foreach (var item in dto.OrderItems)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);

                var stock = await _stockRepository.Query()
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == dto.WarehouseId);

                stock.Quantity -= item.Quantity;
                _stockRepository.Update(stock);

                var orderItem = new OrderItem
                {
                    Order = order,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };
                await _orderItemRepository.AddAsync(orderItem);

                var movement = new StockMovement
                {
                    ProductId = item.ProductId,
                    WarehouseId = dto.WarehouseId,
                    Type = StockMovementType.Sale,
                    Quantity = -item.Quantity,
                    Order = order,
                    PerformedByUserId = SeedUserId
                };
                await _stockMovementRepository.AddAsync(movement);

                totalAmount += item.Quantity * product.Price;
                resultItems.Add(new ResultOrderItemDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    LineTotal = item.Quantity * product.Price
                });
            }

            order.TotalAmount = totalAmount;
            await _orderRepository.AddAsync(order);   

            await _unitOfWork.SaveChangesAsync();

            var userName = (await _userRepository.GetByIdAsync(SeedUserId))?.FullName ?? "Bilinmiyor";

            var result = new ResultOrderDto
            {
                OrderNumber = order.OrderNumber,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Name,
                CustomerId = customer.Id,
                CustomerFullName = customer.FullName,
                CustomerEmail = customer.Email,
                CustomerPhoneNumber = customer.PhoneNumber,
                CreatedByUserId = SeedUserId,
                CreatedByUserName = userName,
                Items = resultItems
            };

            return BaseResult<ResultOrderDto>.Success(result);
        }



        public async Task<BaseResult<IEnumerable<ResultOrderDto>>> GetAllAsync()
        {
            var orders = await _orderRepository.Query()
              .Include(o => o.Customer)
              .Include(o => o.Warehouse)
              .Include(o => o.CreatedByUser)
              .Include(o => o.OrderItems)
                  .ThenInclude(oi => oi.Product).ToListAsync();

            var resultItems = new List<ResultOrderDto>();

            foreach (var order in orders) {

                var resultOrderDto = new ResultOrderDto
                {
                    OrderNumber = order.OrderNumber,
                    Status = order.Status.ToString(),
                    TotalAmount = order.TotalAmount,
                    WarehouseId = order.WarehouseId,
                    WarehouseName = order.Warehouse.Name,
                    CustomerId = order.CustomerId,
                    CustomerFullName = order.Customer.FullName,
                    CustomerEmail = order.Customer.Email,
                    CustomerPhoneNumber = order.Customer.PhoneNumber,
                    CreatedByUserId = order.CreatedByUserId,
                    CreatedByUserName = order.CreatedByUser.FullName,
                    Items = order.OrderItems.Select(oi => new ResultOrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        LineTotal = oi.Quantity * oi.UnitPrice
                    }).ToList()
                };
                resultItems.Add(resultOrderDto);

            }

            return BaseResult<IEnumerable<ResultOrderDto>>.Success(resultItems);
        }

        public async Task<BaseResult<ResultOrderDto>> GetByIdAsync(int id)
        {
            var order = await _orderRepository.Query()
                .Include(o => o.Customer)
                .Include(o => o.Warehouse)
                .Include(o => o.CreatedByUser)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return BaseResult<ResultOrderDto>.Fail("Sipariş bulunamadı.", ResultErrorType.NotFound);
            }

            var resultOrderDto = new ResultOrderDto
            {
                OrderNumber = order.OrderNumber,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                WarehouseId = order.WarehouseId,
                WarehouseName = order.Warehouse.Name,
                CustomerId = order.CustomerId,
                CustomerFullName = order.Customer.FullName,
                CustomerEmail = order.Customer.Email,
                CustomerPhoneNumber = order.Customer.PhoneNumber,
                CreatedByUserId = order.CreatedByUserId,
                CreatedByUserName = order.CreatedByUser.FullName,
                Items = order.OrderItems.Select(oi => new ResultOrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
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
            .Include(o => o.Warehouse)
            .Include(o => o.CreatedByUser)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);


            if(order == null)
            {
                return BaseResult<ResultOrderDto>.Fail("Sipariş bulunamadı.", ResultErrorType.NotFound);
            }

            if(order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Delivered)
            {
                return BaseResult<ResultOrderDto>.Fail("İptal edilmiş veya teslim edilmiş sipariş durumu değiştirilemez.", ResultErrorType.BusinessRule);
            }

                order.Status = newStatus;
                _orderRepository.Update(order);
                await _unitOfWork.SaveChangesAsync();

                var resultOrderDto = new ResultOrderDto
                {
                    OrderNumber = order.OrderNumber,
                    Status = order.Status.ToString(),
                    TotalAmount = order.TotalAmount,
                    WarehouseId = order.WarehouseId,
                    WarehouseName = order.Warehouse.Name,
                    CustomerId = order.CustomerId,
                    CustomerFullName = order.Customer.FullName,
                    CustomerEmail = order.Customer.Email,
                    CustomerPhoneNumber = order.Customer.PhoneNumber,
                    CreatedByUserId = order.CreatedByUserId,
                    CreatedByUserName = order.CreatedByUser.FullName,
                    Items = order.OrderItems.Select(oi => new ResultOrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        LineTotal = oi.Quantity * oi.UnitPrice
                    }).ToList()
                };
            return BaseResult<ResultOrderDto>.Success(resultOrderDto);
        }
    }
}
