using E_commerce.Application.Common;
using E_commerce.Application.Contracts;
using E_commerce.Application.DTOs.Authentication;
using E_commerce.Application.DTOs.Order;
using E_commerce.Application.IServices;
using E_commerce.Application.settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IIdentityService _identityService;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly EmailConfirmationSettings _emailConfirmationSettings;
        public AuthenticationService(IIdentityService identityService, ITokenService tokenService, IEmailService emailService, IOptions<EmailConfirmationSettings> emailConfirmationSettings)
        {
            _identityService = identityService;
            _tokenService = tokenService;
            _emailService = emailService;
            _emailConfirmationSettings = emailConfirmationSettings.Value;
        }

        public async Task<Result<bool>> CheckEmailAsync(string email, CancellationToken cancellationToken = default)
                => await _identityService.EmailExistsAsync(email, cancellationToken);


        public async Task<Result<UserDto>> GetCurrentUserAsync(string email, CancellationToken cancellationToken = default)
        {
            var result = await _identityService.FindByEmailAsync(email, cancellationToken);

            if (!result.IsSuccess)
                return Result<UserDto>.Fail(result.Errors);

            var user = result.data;
            var rolesResult = await _identityService.GetRolesAsync(email, cancellationToken);
            if (!rolesResult.IsSuccess)
                return Result<UserDto>.Fail(rolesResult.Errors);
            var roles = rolesResult.data;
            var token = _tokenService.CreateToken(user.Id, user.Email, user.UserName, roles);

            return new UserDto { DisplayName = user.DisplayName, Email = user.Email, Token = token };

        }

        public async Task<Result<AddressDto>> GetUserAddressAsync(string email, CancellationToken cancellationToken = default)
        {
            var result = await _identityService.GetAddressByEmailAsync(email, cancellationToken);
            if (!result.IsSuccess)
                return Result<AddressDto>.Fail(result.Errors);
            return result.data;
        }

        public async Task<Result<UserDto>> LoginAsync(LoginDto loginDto,CancellationToken cancellationToken = default)
        {
            var invalidCredentials = Error.Unauthorized("Invalid Email or Password");

            // 1. Find User
            var userResult = await _identityService.FindByEmailAsync(
                loginDto.Email, cancellationToken);

            if (!userResult.IsSuccess || userResult.data is null)
                return Result<UserDto>.Fail(invalidCredentials);

            // 2. Check Password
            var passwordResult = await _identityService.CheckPasswordAsync(
                loginDto.Email, loginDto.Password, cancellationToken);

            if (!passwordResult.IsSuccess || !passwordResult.data)
                return Result<UserDto>.Fail(invalidCredentials);

            // 3. Check Email Confirmation
            var emailConfirmedResult =
                await _identityService.IsEmailConfirmedAsync(
                    loginDto.Email, cancellationToken);

            if (!emailConfirmedResult.IsSuccess)
                return Result<UserDto>.Fail(invalidCredentials);

            if (!emailConfirmedResult.data)
                return Result<UserDto>.Fail(
                    Error.Unauthorized("Please confirm your email before logging in."));

            // 4. Get Roles
            var rolesResult = await _identityService.GetRolesAsync(
                loginDto.Email, cancellationToken);

            if (!rolesResult.IsSuccess)
                return Result<UserDto>.Fail(rolesResult.Errors);

            // 5. Generate Token
            var user = userResult.data;
            var token = _tokenService.CreateToken(
                user.Id, user.Email, user.UserName, rolesResult.data);

            return new UserDto
            {
                Email = user.Email,
                DisplayName = user.DisplayName,
                Token = token
            };
        }



        public async Task<Result<AddressDto>> UpdateUserAddressAsync(AddressDto addressDto, string email, CancellationToken cancellationToken = default)
                => await _identityService.UpSertAddressAsync(email, addressDto, cancellationToken);



        public async Task<Result<bool>> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto,CancellationToken cancellationToken = default)
        {
            return await _identityService.ConfirmEmailAsync(
                confirmEmailDto.UserId,
                confirmEmailDto.Token,
                cancellationToken);
        }


        private async Task<Result<RegisterResponseDto>> RegisterWithRoleAsync(RegisterDto registerDto,string? role,CancellationToken cancellationToken = default)
        {
            // 1. Create User
            var userResult = await _identityService.CreateUserAsync(
                registerDto, cancellationToken);

            if (!userResult.IsSuccess || userResult.data is null)
                return Result<RegisterResponseDto>.Fail(userResult.Errors);

            var user = userResult.data;

            // 2. Assign Role if provided
            if (role is not null)
            {
                var roleResult = await _identityService.AddToRoleAsync(
                    user.Id, role, cancellationToken);

                if (!roleResult.IsSuccess)
                    return Result<RegisterResponseDto>.Fail(roleResult.Errors);
            }

            // 3. Generate Confirmation Token
            var tokenResult =
                await _identityService.GenerateEmailConfirmationTokenAsync(
                    user.Email, cancellationToken);

            if (!tokenResult.IsSuccess)
                return Result<RegisterResponseDto>.Fail(tokenResult.Errors);

            // 4. Build Confirmation Link
            var confirmationLink =
                $"{_emailConfirmationSettings.BaseUrl}" +
                $"?userId={Uri.EscapeDataString(user.Id)}" +
                $"&token={Uri.EscapeDataString(tokenResult.data)}";

            // 5. Build Email
            var emailBody = $"""
                <h2>Welcome {System.Net.WebUtility.HtmlEncode(user.DisplayName)}!</h2>
                <p>Thank you for registering.</p>
                <p>Please confirm your email by clicking the link below:</p>
                <a href="{System.Net.WebUtility.HtmlEncode(confirmationLink)}">
                    Confirm Email
                </a>
                """;

            // 6. Send Email
            try
            {
                await _emailService.SendEmailAsync(
                    user.Email, "Confirm Your Email", emailBody);
            }
            catch (Exception)
            {
                return Result<RegisterResponseDto>.Fail(
                    Error.Failure(
                        "EmailSendingFailed",
                        "Your account was created, but confirmation email could not be sent."));
            }

            return new RegisterResponseDto
            {
                Email = user.Email,
                Message = "Registration successful. Please check your email to confirm your account."
            };
        }


        public Task<Result<RegisterResponseDto>> RegisterAdminAsync(RegisterDto registerDto,CancellationToken cancellationToken = default)
        {
            return RegisterWithRoleAsync( registerDto, "Admin", cancellationToken);
        }

        public Task<Result<RegisterResponseDto>> RegisterSuperAdminAsync(RegisterDto registerDto,CancellationToken cancellationToken = default)
        {
            return RegisterWithRoleAsync(registerDto, "SuperAdmin", cancellationToken);
        }



        public async Task<Result<bool>> ForgotPasswordAsync( ForgotPasswordDto forgotPasswordDto,CancellationToken cancellationToken = default)
        {
            // 1. Generate Password Reset Token
            var tokenResult = await _identityService.GeneratePasswordResetTokenAsync(
                forgotPasswordDto.Email,
                cancellationToken);

            // 2. Don't reveal whether the email exists
            if (!tokenResult.IsSuccess)
                return true;

            // 3. Build Email Body
            var encodedEmail =
                System.Net.WebUtility.HtmlEncode(forgotPasswordDto.Email);

            var encodedToken =
                System.Net.WebUtility.HtmlEncode(tokenResult.data);


            var emailBody = $"""
                <html>
                <body>
                    <h2>Password Reset Request</h2>

                    <p>We received a request to reset your password.</p>

                    <p>Use the following information in Postman:</p>

                    <p>
                        <strong>Email:</strong>
                        {System.Net.WebUtility.HtmlEncode(forgotPasswordDto.Email)}
                    </p>

                    <p><strong>Reset Token:</strong></p>

                    <p style="word-break: break-all;">
                        {System.Net.WebUtility.HtmlEncode(tokenResult.data)}
                    </p>

                    <p>If you did not request this, please ignore this email.</p>
                </body>
                </html>
                """;


            // 4. Send Email
            try
            {
                await _emailService.SendEmailAsync(
                    forgotPasswordDto.Email,
                    "Reset Your Password",
                    emailBody);
            }
            catch (Exception)
            {
                return Result<bool>.Fail(
                    Error.Failure(
                        "EmailSendingFailed",
                        "Password reset email could not be sent."));
            }

            return true;
        }



        public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto,CancellationToken cancellationToken = default)
        {
            return await _identityService.ResetPasswordAsync(
                resetPasswordDto.Email,
                resetPasswordDto.Token,
                resetPasswordDto.NewPassword,
                cancellationToken);
        }

        public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordDto changePasswordDto,string userId,CancellationToken cancellationToken = default)
        {
            return await _identityService.ChangePasswordAsync(
                userId,
                changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword,
                cancellationToken);
        }

    }
}
