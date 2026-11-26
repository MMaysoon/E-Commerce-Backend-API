using E_commerce.Application.DTOs.Authentication;
using E_commerce.Application.DTOs.Order;
using E_commerce.Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ApiBaseController
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto, CancellationToken cancellationToken)
            => ToActionResult(await _authenticationService.LoginAsync(loginDto, cancellationToken));

        
        [HttpPost("register-admin")]
        [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RegisterResponseDto>> RegisterAdmin([FromBody] RegisterDto registerDto,CancellationToken cancellationToken)
        {
            return ToActionResult(await _authenticationService.RegisterAdminAsync(registerDto, cancellationToken));
        }


        
        [HttpPost("register-superadmin")]
        [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RegisterResponseDto>> RegisterSuperAdmin([FromBody] RegisterDto registerDto,CancellationToken cancellationToken)
        {
            return ToActionResult(await _authenticationService.RegisterSuperAdminAsync( registerDto, cancellationToken));
        }

        [HttpGet("emailexists")]
        public async Task<ActionResult<bool>> CheckEmail([FromQuery] string email, CancellationToken cancellationToken)
        => ToActionResult(await _authenticationService.CheckEmailAsync(email, cancellationToken));

        [Authorize]
        [HttpGet("currentuser")]
        public async Task<ActionResult<UserDto>> GetCurrentUser(CancellationToken cancellationToken)
            => ToActionResult(await _authenticationService.GetCurrentUserAsync(GetEmailFromToken(), cancellationToken));

        [Authorize]
        [HttpGet("address")]
        public async Task<ActionResult<AddressDto>> GetUserAddress(CancellationToken cancellationToken)
            => ToActionResult(await _authenticationService.GetUserAddressAsync(GetEmailFromToken(), cancellationToken));

        [Authorize]
        [HttpPut("address")]
        public async Task<ActionResult<AddressDto>> UpdateUserAddress(AddressDto addressDto, CancellationToken cancellationToken)
            => ToActionResult(await _authenticationService.UpdateUserAddressAsync(addressDto, GetEmailFromToken(), cancellationToken));


        //Email Verification Flow (when user click confirm email to go direct to these endpoint and confirmed)
        [HttpGet("confirmemail")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> ConfirmEmail([FromQuery] ConfirmEmailDto confirmEmailDto,CancellationToken cancellationToken)
            => ToActionResult(await _authenticationService.ConfirmEmailAsync(confirmEmailDto, cancellationToken));



        // Forgot Password
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto,CancellationToken cancellationToken)
            => ToActionResult(await _authenticationService.ForgotPasswordAsync(forgotPasswordDto, cancellationToken));


        // Reset Password
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto,
            CancellationToken cancellationToken)
            => ToActionResult(await _authenticationService.ResetPasswordAsync(
                resetPasswordDto, cancellationToken));


        // Change Password
        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<bool>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto, CancellationToken cancellationToken)
            => ToActionResult(await _authenticationService.ChangePasswordAsync(changePasswordDto, GetUserIdFromToken(),cancellationToken));


    }



}
