using E_commerce.Application.Common;
using E_commerce.Application.Contracts;
using E_commerce.Application.DTOs.Authentication;
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

        public async Task<Result<UserDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
        {
            // Get User By Email 
            var userResult = await _identityService.FindByEmailAsync(loginDto.Email, cancellationToken);
            if (!userResult.IsSuccess)
                return Result<UserDto>.Fail(userResult.Errors);

            // Check Password 
            var passwordResult = await _identityService.CheckPasswordAsync(loginDto.Email, loginDto.Password, cancellationToken);
            // data here boolean value indicating if the password is valid or not
            if (!passwordResult.IsSuccess || !passwordResult.data)
                return Result<UserDto>.Fail( Error.Unauthorized("Invalid Email or Password"));

            // Check Email Confirmation
            var emailConfirmedResult =await _identityService.IsEmailConfirmedAsync(loginDto.Email, cancellationToken);

            if (!emailConfirmedResult.IsSuccess)
                return Result<UserDto>.Fail(emailConfirmedResult.Errors);

            if (!emailConfirmedResult.data)
                return Result<UserDto>.Fail(Error.Unauthorized("Please confirm your email before logging in."));

            var rolesResult = await _identityService.GetRolesAsync(loginDto.Email, cancellationToken);
            if (!rolesResult.IsSuccess)
                return Result<UserDto>.Fail(rolesResult.Errors);
            var roles = rolesResult.data;
            var user = userResult.data;
            var token = _tokenService.CreateToken(user.Id, user.Email, user.UserName, roles);

            return new UserDto
            {
                Email = user.Email,
                DisplayName = user.DisplayName,
                Token = token
            };
        }


        //public async Task<Result<UserDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
        //{
        //    var result = await _identityService.CreateUserAsync(registerDto, cancellationToken);
        //    if (!result.IsSuccess || result.data is null)
        //        return Result<UserDto>.Fail(result.Errors);

        //    return new UserDto { Email = result.data.Email, DisplayName = result.data.DisplayName, Token = "Token" };

        //}


        // Register with Email Confirmation
        public async Task<Result<RegisterResponseDto>> RegisterAsync(RegisterDto registerDto,CancellationToken cancellationToken = default)
        {
            // 1. Create User
            var userResult = await _identityService.CreateUserAsync(registerDto, cancellationToken);

            if (!userResult.IsSuccess || userResult.data is null)
                return Result<RegisterResponseDto>.Fail(userResult.Errors);

            var user = userResult.data;

            // 2. Generate Email Confirmation Token
            var tokenResult = await _identityService.GenerateEmailConfirmationTokenAsync(user.Email, cancellationToken);

            if (!tokenResult.IsSuccess)
                return Result<RegisterResponseDto>.Fail(tokenResult.Errors);

            // 3. Build Confirmation Link
            var baseUrl = _emailConfirmationSettings.BaseUrl;

            Console.WriteLine($"Confirmation BaseUrl: [{baseUrl}]");
            var confirmationLink =
                                $"{baseUrl}" +
                                $"?userId={Uri.EscapeDataString(user.Id)}" +
                                $"&token={Uri.EscapeDataString(tokenResult.data)}";

            Console.WriteLine($"Confirmation Link: {confirmationLink}");

            // 4. Prepare Email
            var emailBody = $"""
                            <h2>Welcome {System.Net.WebUtility.HtmlEncode(user.DisplayName)}!</h2>
                            <p>Thank you for registering.</p>
                            <p>Please confirm your email by clicking the link below:</p>
                            <a href="{System.Net.WebUtility.HtmlEncode(confirmationLink)}">
                                Confirm Email
                            </a>
                            """;

            // 5. Send Confirmation Email
            try
            {
                await _emailService.SendEmailAsync( user.Email,"Confirm Your Email",emailBody);
            }
            catch (Exception)
            {
                return Result<RegisterResponseDto>.Fail(
                    Error.Failure(
                        "EmailSendingFailed",
                        "Your account was created, but confirmation email could not be sent. Please request a new confirmation email."));
            }

            return new RegisterResponseDto
            {
                Email = user.Email,
                Message = "Registration successful. Please check your email to confirm your account."
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
    }
}
