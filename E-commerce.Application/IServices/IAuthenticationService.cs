using E_commerce.Application.Common;
using E_commerce.Application.DTOs.Authentication;
using E_commerce.Application.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.IServices
{
    public interface IAuthenticationService
    {
        Task<Result<UserDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);

        //Task<Result<RegisterResponseDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
        Task<Result<RegisterResponseDto>> RegisterAdminAsync(RegisterDto registerDto,CancellationToken cancellationToken = default);

        Task<Result<RegisterResponseDto>> RegisterSuperAdminAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
        Task<Result<bool>> CheckEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<AddressDto>> GetUserAddressAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<AddressDto>> UpdateUserAddressAsync(AddressDto addressDto, string email, CancellationToken cancellationToken = default);
        Task<Result<UserDto>> GetCurrentUserAsync(string email, CancellationToken cancellationToken = default);

        Task<Result<bool>> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto, CancellationToken cancellationToken = default);

        Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto,CancellationToken cancellationToken = default);

        Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto,CancellationToken cancellationToken = default);

       Task<Result<bool>> ChangePasswordAsync(ChangePasswordDto changePasswordDto,string userId,CancellationToken cancellationToken = default);
    }
}
