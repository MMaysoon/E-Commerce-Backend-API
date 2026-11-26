using E_commerce.Application.Common;
using E_commerce.Application.Contracts;
using E_commerce.Application.DTOs.Authentication;
using E_commerce.Application.DTOs.Order;
using E_commerce.Infrastructure.Identity.Data;
using E_commerce.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Services
{
   public class IdentityService :IIdentityService
   {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IdentityService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<Result<bool>> CheckPasswordAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return Result<bool>.Fail(Error.NotFound("User not found"));

            var isvalid = await _userManager.CheckPasswordAsync(user, password);

            return isvalid;
        }

        public async Task<Result<IdentityUserResult>> CreateUserAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
        {
            var user = new ApplicationUser
            {
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                PhoneNumber = registerDto.PhoneNumber,
                DisplayName = registerDto.DisplayName
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e =>
                {
                    var type = e.Code switch
                    {
                        "DuplicateEmail" or "DuplicateUserName"
                            => ErrorType.Conflict,

                        "PasswordTooShort" or
                        "PasswordRequiresDigit" or
                        "PasswordRequiresUpper" or
                        "PasswordRequiresLower" or
                        "PasswordRequiresNonAlphanumeric"
                            => ErrorType.Validation,

                        _ => ErrorType.Failure
                    };

                    return new Error(e.Code, e.Description, type);
                }).ToList();

                return Result<IdentityUserResult>.Fail(errors);
            }

            return Result<IdentityUserResult>.Ok(new IdentityUserResult(user.Id, user.Email, user.UserName, user.DisplayName));
        }

        public async Task<Result<bool>> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
         => await _userManager.FindByEmailAsync(email) is not null;

        public async Task<Result<IdentityUserResult>> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return Result<IdentityUserResult>.Fail(Error.NotFound("User Not Found"));
            }
            else
            {
                return new IdentityUserResult(user.Id, user.Email, user.UserName, user.DisplayName);

            }
        }

        public async Task<Result<AddressDto>> GetAddressByEmailAsync(string email, CancellationToken cancellationToken = default)
        {

            // used when needed user with related data (Address) using Include
            var user = await _userManager.Users
                             .Include(u => u.Address)
                             .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
              //====
             //var user2= await _context.Users
             //                .Include(u => u.Address)
             //                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            
            if (user == null) return Result<AddressDto>.Fail(Error.NotFound($"User '{email}' not found"));

            if (user?.Address == null) return Result<AddressDto>.Fail(Error.NotFound("Address Not Found"));

            return new AddressDto
            {
                FirstName = user.Address.FirstName,
                LastName = user.Address.LastName,
                City = user.Address.City,
                Street = user.Address.Street,
                Country = user.Address.Country
            };
        }

        public async Task<Result<IReadOnlyList<string>>> GetRolesAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return Result<IReadOnlyList<string>>.Fail(Error.NotFound($"User '{email}' not found"));
            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        public async Task<Result<AddressDto>> UpSertAddressAsync(string email, AddressDto addressDto, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.Users
                         .Include(u => u.Address)
                         .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user == null) return Result<AddressDto>.Fail(Error.NotFound($"User '{email}' not found"));
            if (user.Address is null)
            {
                user.Address = new Address
                {
                    FirstName = addressDto.FirstName,
                    LastName = addressDto.LastName,
                    City = addressDto.City,
                    Street = addressDto.Street,
                    Country = addressDto.Country
                };
            }
            else
            {
                user.Address.FirstName = addressDto.FirstName;
                user.Address.LastName = addressDto.LastName;
                user.Address.City = addressDto.City;
                user.Address.Country = addressDto.Country;
                user.Address.Street = addressDto.Street;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return Result<AddressDto>.Fail(Error.Failure("Failure", string.Join("; ", result.Errors.Select(e => e.Description))));

            return addressDto;
        }


        public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(string email,CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                return Result<string>.Fail(
                    Error.NotFound("User not found"));

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            return token;
        }

        public async Task<Result<bool>> ConfirmEmailAsync(string userId,string token,CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return Result<bool>.Fail(
                    Error.NotFound("User not found"));

            if (user.EmailConfirmed)
                return Result<bool>.Fail(
                    Error.Failure("EmailConfirmed", "Email is already confirmed"));

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Select(e => new Error(e.Code, e.Description))
                    .ToList();

                return Result<bool>.Fail(errors);
            }

            return true;
        }

        public async Task<Result<bool>> IsEmailConfirmedAsync(string email,CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                return Result<bool>.Fail(
                    Error.NotFound("User not found"));

            return user.EmailConfirmed;
        }


        public async Task<Result<bool>> AddToRoleAsync(string userId,string role,CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return Result<bool>.Fail(Error.NotFound("User not found"));

            if (!await _roleManager.RoleExistsAsync(role))
                return Result<bool>.Fail(
                    Error.NotFound($"Role '{role}' does not exist"));

            if (await _userManager.IsInRoleAsync(user, role))
                return Result<bool>.Fail(
                    Error.Failure("RoleAlreadyAssigned", "User already has this role"));

            var result = await _userManager.AddToRoleAsync(user, role);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Select(e => new Error(e.Code, e.Description))
                    .ToList();

                return Result<bool>.Fail(errors);
            }

            return true;
        }

        public async Task<Result<string>> GeneratePasswordResetTokenAsync(string email,CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                return Result<string>.Fail(
                    Error.NotFound("User not found"));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            return token;
        }

        public async Task<Result<bool>> ResetPasswordAsync(string email,string token,string newPassword,CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                return Result<bool>.Fail(
                    Error.NotFound("User not found"));

            var result = await _userManager.ResetPasswordAsync(
                user, token, newPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Select(e => new Error(e.Code, e.Description))
                    .ToList();

                return Result<bool>.Fail(errors);
            }

            return true;
        }


        public async Task<Result<bool>> ChangePasswordAsync(string userId,string currentPassword,string newPassword,CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return Result<bool>.Fail(
                    Error.NotFound("User not found"));

            var result = await _userManager.ChangePasswordAsync(
                user, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Select(e => new Error(e.Code, e.Description))
                    .ToList();

                return Result<bool>.Fail(errors);
            }

            return true;
        }

    }
}
