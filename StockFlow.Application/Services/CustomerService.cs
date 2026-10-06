using FluentValidation;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.CustomerDtos;
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
    public class CustomerService(IGenericRepository<Customer> _repository,IMapper _mapper,IUnitOfWork _unitOfWork,IValidator<CreateCustomerDto> _createValidator,IValidator<UpdateCustomerDto>_updateValidator) : ICustomerService
    {
        public async Task<BaseResult<ResultCustomerDto>> CreateAsync(CreateCustomerDto createCustomerDto)
        {
            createCustomerDto.FullName = createCustomerDto.FullName.Trim();
            createCustomerDto.Email = string.IsNullOrWhiteSpace(createCustomerDto.Email) ? null : createCustomerDto.Email.Trim();
            createCustomerDto.PhoneNumber = string.IsNullOrWhiteSpace(createCustomerDto.PhoneNumber) ? null : createCustomerDto.PhoneNumber.Trim();
          
            var validationResult = await _createValidator.ValidateAsync(createCustomerDto);
            if (!validationResult.IsValid)
            {
                return BaseResult<ResultCustomerDto>.Fail(validationResult.Errors);
            }

            var customer = _mapper.Map<Customer>(createCustomerDto);
            await _repository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            var mappedCustomer = _mapper.Map<ResultCustomerDto>(customer);
            return BaseResult<ResultCustomerDto>.Success(mappedCustomer);
        }

        public async Task<BaseResult<bool>> DeleteAsync(int id)
        {
            var customer = await _repository.Query()
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return BaseResult<bool>.Fail("Müşteri bulunamadı", ResultErrorType.NotFound);
            }
            
            if (customer.Orders.Any())
            {
                return BaseResult<bool>.Fail("Müşteri silinemez, çünkü bu müşteri ile ilişkili siparişler bulunmaktadır.", ResultErrorType.Conflict);
            }

            _repository.Delete(customer);
            await _unitOfWork.SaveChangesAsync();
            return BaseResult<bool>.Success(true);
        }

        public async Task<BaseResult<PagedResult<ResultCustomerDto>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var paged = await _repository.GetAllAsync(pageNumber, pageSize);
            var result = _mapper.Map<PagedResult<ResultCustomerDto>>(paged);

            return BaseResult<PagedResult<ResultCustomerDto>>.Success(result);
        }

        public async Task<BaseResult<ResultCustomerDto>> GetByIdAsync(int id)
        {
           var customer =await _repository.GetByIdAsync(id);
           if(customer == null)
            {
                return BaseResult<ResultCustomerDto>.Fail("Müşteri bulunamadı", ResultErrorType.NotFound);
            }
           var mappedCustomer = _mapper.Map<ResultCustomerDto>(customer);
           return BaseResult<ResultCustomerDto>.Success(mappedCustomer);
        }

        public async Task<BaseResult<ResultCustomerDto>> UpdateAsync(int id, UpdateCustomerDto updateCustomerDto)
        {
            updateCustomerDto.FullName = updateCustomerDto.FullName.Trim();
            updateCustomerDto.Email = string.IsNullOrWhiteSpace(updateCustomerDto.Email) ? null : updateCustomerDto.Email.Trim();
            updateCustomerDto.PhoneNumber = string.IsNullOrWhiteSpace(updateCustomerDto.PhoneNumber) ? null : updateCustomerDto.PhoneNumber.Trim();

            var validationResult = await _updateValidator.ValidateAsync(updateCustomerDto);
            if (!validationResult.IsValid)
            {
                return BaseResult<ResultCustomerDto>.Fail(validationResult.Errors);
            }

            var customer = await _repository.GetByIdAsync(id);
            if (customer == null)
            {
                return BaseResult<ResultCustomerDto>.Fail("Müşteri bulunamadı", ResultErrorType.NotFound);
            }

            _mapper.Map(updateCustomerDto, customer);
            _repository.Update(customer);
            await _unitOfWork.SaveChangesAsync();
            var mappedCustomer = _mapper.Map<ResultCustomerDto>(customer);
            return BaseResult<ResultCustomerDto>.Success(mappedCustomer);

        }
    }
}
