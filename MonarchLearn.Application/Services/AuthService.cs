using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MonarchLearn.Application.DTOs.Auth;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Domain.Entities.Users;
using MonarchLearn.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", model.Email);

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists", model.Email);
                throw new ConflictException("This email is already registered in the system");
            }

            if (string.IsNullOrWhiteSpace(model.Role))
            {
                _logger.LogWarning("Registration failed: Role not provided for {Email}", model.Email);
                throw new BadRequestException("Role is required. Please specify either 'Student' or 'Instructor'.");
            }

            if (model.Role != "Student" && model.Role != "Instructor")
            {
                _logger.LogWarning("Invalid role attempt: {Role} for {Email}", model.Role, model.Email);
                throw new BadRequestException("Invalid role. Only 'Student' and 'Instructor' roles are allowed for registration.");
            }

            var user = new AppUser
            {
                Email = model.Email,
                UserName = model.Email,
                FullName = model.FullName,
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogError("User creation failed for {Email}: {Errors}", model.Email, errors);
                throw new BadRequestException($"Registration failed: {errors}");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("Failed to assign {Role} role to user {Email}", model.Role, model.Email);
            }

            BackgroundJob.Enqueue<IAuthService>(x => x.SendEmailVerificationAsync(user.Id));

            _logger.LogInformation("User {Email} registered successfully as {Role}. Email verification required.", model.Email, model.Role);

            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponseDto
            {
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles.ToList(),
                Message = "Registration successful! Check your inbox for the 6-digit verification code."
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto model)
        {
            _logger.LogInformation("Login attempt for email: {Email}", model.Email);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User with email {Email} not found", model.Email);
                throw new BadRequestException("Invalid email or password");
            }

            if (user.IsDeleted)
            {
                _logger.LogWarning("Login attempt for deleted user: {Email}", model.Email);
                throw new ForbiddenException("Your account has been deactivated. Please contact support");
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Login denied for {Email}: Email not verified", model.Email);
                throw new ForbiddenException(
                    "Please verify your email before logging in. Check your inbox for the verification code.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Login failed for {Email}: Invalid password", model.Email);

                if (result.IsLockedOut)
                {
                    throw new ForbiddenException("Account is locked due to multiple failed login attempts. Please try again later.");
                }

                throw new BadRequestException("Invalid email or password");
            }

            _logger.LogInformation("User {Email} logged in successfully", model.Email);
            return await GenerateAndSaveTokensAsync(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto model)
        {
            _logger.LogInformation("Refresh token request received");

            var principal = GetPrincipalFromExpiredToken(model.AccessToken);
            if (principal == null)
            {
                _logger.LogWarning("Refresh token failed: Invalid access token");
                throw new BadRequestException("Invalid access token");
            }

            var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Refresh token failed: Email claim not found in token");
                throw new BadRequestException("Invalid token claims");
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("Refresh token failed: User {Email} not found", email);
                throw new NotFoundException("User", email);
            }

            if (user.RefreshToken != model.RefreshToken)
            {
                _logger.LogWarning("Refresh token mismatch for user {Email}", email);
                throw new BadRequestException("Invalid refresh token");
            }

            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token expired for user {Email}", email);
                throw new BadRequestException("Refresh token has expired. Please login again");
            }

            _logger.LogInformation("Refresh token validated successfully for user {Email}", email);
            return await GenerateAndSaveTokensAsync(user);
        }

        private async Task<AuthResponseDto> GenerateAndSaveTokensAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = GenerateAccessToken(user, roles);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            int refreshDays = _configuration.GetValue<int>("JwtSettings:RefreshTokenValidityInDays", 7);
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshDays);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogError("Failed to update refresh token for user {Email}", user.Email);
                throw new BadRequestException("Failed to generate authentication tokens");
            }

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(accessToken),
                RefreshToken = refreshToken,
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles.ToList()
            };
        }

        private JwtSecurityToken GenerateAccessToken(AppUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expirationMinutesStr = _configuration["JwtSettings:ExpirationMinutes"];
            double expirationMinutes = string.IsNullOrEmpty(expirationMinutesStr) ? 60 : double.Parse(expirationMinutesStr);

            return new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: creds
            );
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new BadRequestException("Access token is required");
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"])),
                ValidateLifetime = false
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(
                    token,
                    tokenValidationParameters,
                    out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new BadRequestException("Invalid token algorithm");
                }

                return principal;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                throw new BadRequestException("Invalid token structure");
            }
        }

        public async Task SendEmailVerificationAsync(int userId)
        {
            _logger.LogInformation("Email verification requested for User {UserId}", userId);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new NotFoundException("User", userId);

           
            var verificationCode = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

            BackgroundJob.Enqueue<INotificationService>(n =>
                n.ExecuteEmailSendAsync(
                    user.Id,
                    "Email Verification - MonarchLearn",
                    $@"
                <h2>Welcome to MonarchLearn!</h2>
                <p>Please verify your email by using this code in the app:</p>
                <p style='font-size: 24px; font-weight: bold; color: #27ae60; letter-spacing: 5px;'>{verificationCode}</p>
                <p>This code is valid for a limited time.</p>
            "));

            _logger.LogInformation("Verification email (OTP) queued for User {UserId}", userId);
        }

        public async Task VerifyEmailAsync(string email, string token)
        {
            _logger.LogInformation("Email verification attempt for {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Verification failed: User {Email} not found", email);
                throw new NotFoundException("User", email);
            }

          
            var result = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", token);

            if (!result)
            {
                _logger.LogError("Email verification failed for {Email}: Invalid or expired code", email);
                throw new BadRequestException("Email verification failed: Invalid or expired code.");
            }

           
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Email verified successfully for {Email}", email);
        }

        public async Task SendPasswordResetCodeAsync(string email)
        {
            _logger.LogInformation("Password reset requested for {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                return;
            }

            
            var resetCode = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

            BackgroundJob.Enqueue<INotificationService>(n =>
                n.ExecuteEmailSendAsync(
                    user.Id,
                    "Password Reset - MonarchLearn",
                    $@"
                <h2>Password Reset Request</h2>
                <p>You requested to reset your password. Use this code in the app:</p>
                <p style='font-size: 24px; font-weight: bold; color: #e74c3c; letter-spacing: 5px;'>{resetCode}</p>
                <p>If you didn't request this, please ignore this email.</p>
            "));

            _logger.LogInformation("Password reset email (OTP) queued for {Email}", email);
        }
        public async Task ResendVerificationCodeAsync(ResendVerificationDto model)
        {
            _logger.LogInformation("Resend verification code requested for email: {Email}", model.Email);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                _logger.LogWarning("Resend code failed: User with email {Email} not found", model.Email);
                throw new NotFoundException("User", model.Email);
            }

            if (user.EmailConfirmed)
            {
                _logger.LogWarning("Resend code failed: User {Email} is already verified", model.Email);
                throw new BadRequestException("This account is already verified. Please login.");
            }

            
            BackgroundJob.Enqueue<IAuthService>(x => x.SendEmailVerificationAsync(user.Id));

            _logger.LogInformation("Verification code resend task successfully enqueued for User {UserId} ({Email})", user.Id, model.Email);
        }
        public async Task ResetPasswordAsync(string email, string code, string newPassword)
        {
            _logger.LogInformation("Password reset attempt for {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Reset failed: User {Email} not found", email);
                throw new NotFoundException("Invalid email or token");
            }

            
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", code);
            if (!isValid)
            {
                throw new BadRequestException("Invalid or expired reset code.");
            }

            
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogError("Password reset failed for {Email}: {Errors}", email, errors);
                throw new BadRequestException($"Password reset failed: {errors}");
            }

            _logger.LogInformation("Password reset successful for {Email}", email);
        }
    }
}