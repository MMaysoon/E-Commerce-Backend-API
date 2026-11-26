using E_commerce.Application.Common;
using E_commerce.Application.DTOs.Authentication;
using E_commerce.Application.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Contracts
{
    public interface IIdentityService
    {
        Task<Result<IdentityUserResult>> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<bool>> CheckPasswordAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<Result<IdentityUserResult>> CreateUserAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
        Task<Result<IReadOnlyList<string>>> GetRolesAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<AddressDto>> GetAddressByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<AddressDto>> UpSertAddressAsync(string email, AddressDto addressDto, CancellationToken cancellationToken = default);
        Task<Result<bool>> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

        Task<Result<string>> GenerateEmailConfirmationTokenAsync(string email,CancellationToken cancellationToken = default);

        Task<Result<bool>> ConfirmEmailAsync(string userId,string token,CancellationToken cancellationToken = default);

        Task<Result<bool>> IsEmailConfirmedAsync(string email,CancellationToken cancellationToken = default);

        Task<Result<bool>> AddToRoleAsync(string userId,string role,CancellationToken cancellationToken = default);

        Task<Result<string>> GeneratePasswordResetTokenAsync(string email,CancellationToken cancellationToken = default);

        Task<Result<bool>> ResetPasswordAsync(string email,string token,string newPassword,CancellationToken cancellationToken = default);

        Task<Result<bool>> ChangePasswordAsync(string userId, string currentPassword,string newPassword,CancellationToken cancellationToken = default);

    }
}
