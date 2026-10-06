using BCrypt.Net;
using FluentValidation;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.UsersDtos;
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
    public class UserService(
        IGenericRepository<User> _userGenericRepository,
        IGenericRepository<Warehouse> _warehouseGenericRepository,
        IUnitOfWork _unitOfWork,
        IMapper _mapper,
        IValidator<CreateUserDto> _createValidator,
        IValidator<UpdateUserDto> _updateValidator, IValidator<ChangePasswordDto> _changePasswordValidator
        ) : IUserService
    {
        public async Task<BaseResult<bool>> ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto)
        {
            changePasswordDto.CurrentPassword = changePasswordDto.CurrentPassword.Trim();
            changePasswordDto.NewPassword = changePasswordDto.NewPassword.Trim();

            var user = await _userGenericRepository.GetByIdAsync(id);
            if (user == null)
            {
                return BaseResult<bool>.Fail("Kullanıcı bulunamadı.", ResultErrorType.NotFound);
            }


            var validateResult = await _changePasswordValidator.ValidateAsync(changePasswordDto);
            if (!validateResult.IsValid)
            {

                return BaseResult<bool>.Fail(validateResult.Errors);
            }

            var isCorrect = BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash);
            if (!isCorrect)
            {
                return BaseResult<bool>.Fail("Mevcut şifre yanlış.", ResultErrorType.Unauthorized);
            }

            var newPassword = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            user.PasswordHash = newPassword;

            _userGenericRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return BaseResult<bool>.Success(true);
        }

        public async Task<BaseResult<ResultUserDto>> CreateAsync(CreateUserDto createUserDto)
        {
            createUserDto.Email = createUserDto.Email.Trim();
            createUserDto.FullName = createUserDto.FullName.Trim();
            createUserDto.EmployeeCode = createUserDto.EmployeeCode.Trim();
            createUserDto.PhoneNumber = string.IsNullOrWhiteSpace(createUserDto.PhoneNumber) ? null : createUserDto.PhoneNumber.Trim();

            var validateResult = await _createValidator.ValidateAsync(createUserDto);
            if (!validateResult.IsValid)
            {
                return BaseResult<ResultUserDto>.Fail(validateResult.Errors);
            }

            var existingUser = await _userGenericRepository.Query()
                    .FirstOrDefaultAsync(x => x.Email == createUserDto.Email);

            if (existingUser != null)
            {
                return BaseResult<ResultUserDto>.Fail(
                    $"'{createUserDto.Email}' email adresi başka bir kullanıcı tarafından kullanılıyor.",
                    ResultErrorType.Conflict);
            }

            var existingEmployeeCode = await _userGenericRepository.Query()
                    .FirstOrDefaultAsync(x => x.EmployeeCode == createUserDto.EmployeeCode);

            if (existingEmployeeCode != null)
            {
                return BaseResult<ResultUserDto>.Fail(
                    $"'{createUserDto.EmployeeCode}' personel kodu başka bir kullanıcı tarafından kullanılıyor.",
                    ResultErrorType.Conflict);
            }

            Warehouse? warehouse = null;
            if (createUserDto.WarehouseId != null)
            {
                warehouse = await _warehouseGenericRepository.GetByIdAsync(createUserDto.WarehouseId.Value);

                if (warehouse == null)
                {
                    return BaseResult<ResultUserDto>.Fail(
                        $"Belirtilen depo bulunamadı. WarehouseId: {createUserDto.WarehouseId.Value}.",
                        ResultErrorType.NotFound);
                }
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

            var user = new User
            {
                Email = createUserDto.Email,
                PasswordHash = hashedPassword,
                FullName = createUserDto.FullName,
                EmployeeCode = createUserDto.EmployeeCode,
                PhoneNumber = createUserDto.PhoneNumber,
                Role = createUserDto.Role,
                WarehouseId = createUserDto.WarehouseId,
                IsActive = true
            };

            await _userGenericRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var resultDto = new ResultUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                EmployeeCode = user.EmployeeCode,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                WarehouseId = user.WarehouseId,
                WarehouseName = warehouse?.Name
            };

            return BaseResult<ResultUserDto>.Success(resultDto);
        }

        public async Task<BaseResult<bool>> DeleteAsync(int id)
        {
            var user = await _userGenericRepository.Query()
                .Include(u => u.StockMovements)
                .Include(u => u.Orders)
                .Include(u => u.PurchaseOrders)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return BaseResult<bool>.Fail($"Kullanıcı bulunamadı. UserId: {id}.", ResultErrorType.NotFound);
            }

            if (user.StockMovements.Any())
            {
                return BaseResult<bool>.Fail("Bu kullanıcının geçmiş stok hareketleri var, silinemez.", ResultErrorType.Conflict);
            }

            if (user.Orders.Any())
            {
                return BaseResult<bool>.Fail("Bu kullanıcının oluşturduğu siparişler var, silinemez.", ResultErrorType.Conflict);
            }

            if (user.PurchaseOrders.Any())
            {
                return BaseResult<bool>.Fail("Bu kullanıcının oluşturduğu satın alma siparişleri var, silinemez.", ResultErrorType.Conflict);
            }

            _userGenericRepository.Delete(user);
            await _unitOfWork.SaveChangesAsync();

            return BaseResult<bool>.Success(true);
        }

        public async Task<BaseResult<PagedResult<ResultUserDto>>> GetAllAsync(int pageNumber, int pageSize)
        {
            var paged = await _userGenericRepository.GetAllAsync(pageNumber, pageSize,
                include: q => q.Include(u => u.Warehouse));
            var result = _mapper.Map<PagedResult<ResultUserDto>>(paged);

            return BaseResult<PagedResult<ResultUserDto>>.Success(result);
        }

        public async Task<BaseResult<ResultUserDto>> GetByIdAsync(int id)
        {
            var user = await _userGenericRepository.Query()
                 .Include(u => u.Warehouse)
                 .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                return BaseResult<ResultUserDto>.Fail(
                    $"Kullanıcı bulunamadı. UserId: {id}.",
                    ResultErrorType.NotFound);
            }

            var userDto = _mapper.Map<ResultUserDto>(user);
            return BaseResult<ResultUserDto>.Success(userDto);
        }

        public async Task<BaseResult<ResultUserDto>> UpdateAsync(int id, UpdateUserDto updateUserDto)
        {
            updateUserDto.Email = updateUserDto.Email.Trim();
            updateUserDto.FullName = updateUserDto.FullName.Trim();
            updateUserDto.EmployeeCode = updateUserDto.EmployeeCode.Trim();
            updateUserDto.PhoneNumber = string.IsNullOrWhiteSpace(updateUserDto.PhoneNumber) ? null : updateUserDto.PhoneNumber.Trim();

            var user = await _userGenericRepository.GetByIdAsync(id);

            if (user == null)
            {
                return BaseResult<ResultUserDto>.Fail(
                    $"Güncellenecek kullanıcı bulunamadı. UserId: {id}.",
                    ResultErrorType.NotFound);
            }

            var validateResult = await _updateValidator.ValidateAsync(updateUserDto);
            if (!validateResult.IsValid)
            {
                return BaseResult<ResultUserDto>.Fail(validateResult.Errors);
            }

            var existingUser = await _userGenericRepository.Query()
                   .FirstOrDefaultAsync(x => x.Email == updateUserDto.Email && x.Id != id);

            if (existingUser != null)
            {
                return BaseResult<ResultUserDto>.Fail(
                    $"'{updateUserDto.Email}' email adresi başka bir kullanıcı tarafından kullanılıyor.",
                    ResultErrorType.Conflict);
            }

            var existingEmployeeCode = await _userGenericRepository.Query()
                    .FirstOrDefaultAsync(x => x.EmployeeCode == updateUserDto.EmployeeCode && x.Id != id);

            if (existingEmployeeCode != null)
            {
                return BaseResult<ResultUserDto>.Fail(
                    $"'{updateUserDto.EmployeeCode}' personel kodu başka bir kullanıcı tarafından kullanılıyor.",
                    ResultErrorType.Conflict);
            }

            Warehouse? warehouse = null;
            if (updateUserDto.WarehouseId != null)
            {
                warehouse = await _warehouseGenericRepository.GetByIdAsync(updateUserDto.WarehouseId.Value);

                if (warehouse == null)
                {
                    return BaseResult<ResultUserDto>.Fail(
                        $"Belirtilen depo bulunamadı. WarehouseId: {updateUserDto.WarehouseId.Value}.",
                        ResultErrorType.NotFound);
                }
            }

            user.Email = updateUserDto.Email;
            user.FullName = updateUserDto.FullName;
            user.EmployeeCode = updateUserDto.EmployeeCode;
            user.PhoneNumber = updateUserDto.PhoneNumber;
            user.Role = updateUserDto.Role;
            user.WarehouseId = updateUserDto.WarehouseId;
            user.IsActive = updateUserDto.IsActive;

            _userGenericRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            var userDto = new ResultUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                EmployeeCode = user.EmployeeCode,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                WarehouseId = user.WarehouseId,
                WarehouseName = warehouse?.Name
            };

            return BaseResult<ResultUserDto>.Success(userDto);
        }
    }
}
