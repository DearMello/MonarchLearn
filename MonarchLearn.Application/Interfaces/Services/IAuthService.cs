using MonarchLearn.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto model);
        Task<AuthResponseDto> LoginAsync(LoginDto model);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto model);

        Task SendEmailVerificationAsync(int userId);
        Task VerifyEmailAsync(string email, string token);
        Task SendPasswordResetCodeAsync(string email);
        Task ResetPasswordAsync(string email, string token, string newPassword);
        Task ResendVerificationCodeAsync(ResendVerificationDto model);
    }
}
