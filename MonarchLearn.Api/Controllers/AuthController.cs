using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonarchLearn.Application.DTOs.Auth;
using MonarchLearn.Application.Interfaces.Services;

namespace MonarchLearn.Api.Controllers
{
    [Route("api/v1/auth")]
    [AllowAnonymous]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("registrations")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var response = await _authService.RegisterAsync(model);
            return Created(string.Empty, response);
        }

        [HttpPost("logins")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var response = await _authService.LoginAsync(model);
            return Ok(response);
        }

        [HttpPost("tokens/refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto model)
        {
            var response = await _authService.RefreshTokenAsync(model);
            return Ok(response);
        }

        [HttpPost("email-verifications")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto model)
        {
            await _authService.VerifyEmailAsync(model.Email, model.Code);
            return Ok(new { message = "Email verified successfully." });
        }

        [HttpPost("email-verifications/resubmissions")]
        public async Task<IActionResult> ResendVerificationCode([FromBody] ResendVerificationDto model)
        {
            await _authService.ResendVerificationCodeAsync(model);
            return Ok(new { message = "A new verification code has been sent." });
        }

        [HttpPost("password-resets/requests")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            await _authService.SendPasswordResetCodeAsync(model.Email);
            return Ok(new { message = "Password reset code sent." });
        }

        [HttpPost("password-resets/confirmations")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            await _authService.ResetPasswordAsync(model.Email, model.Code, model.NewPassword);
            return Ok(new { message = "Password has been reset successfully." });
        }
    }
}