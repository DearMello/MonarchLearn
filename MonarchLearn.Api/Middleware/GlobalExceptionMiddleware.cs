using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MonarchLearn.Domain.Exceptions;
using System.Net;
using System.Text.Json;
using System.Linq;

namespace MonarchLearn.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception caught by GlobalExceptionMiddleware");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode;
            object? errors = null;
            string message = exception.Message;

            switch (exception)
            {
                //  FluentValidation xetalari
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Validasiya xətası baş verdi.";
                    errors = validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray()
                        );
                    break;

                
                case NotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    break;

                // 3️Conflict - 409 (Məsələn: "Bu email artıq var")
                case ConflictException:
                    statusCode = HttpStatusCode.Conflict;
                    break;

                //  Forbidden - 403 (Məsələn: "Sertifikat üçün dərsləri bitirməlisən")
                case ForbiddenException:
                    statusCode = HttpStatusCode.Forbidden;
                    break;

                // BadRequest - 400 (Ümumi yanlış sorğu)
                case BadRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    break;

                //  Unauthorized - 401 (Token yoxdur və ya səhvdir)
                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "Bu əməliyyat üçün giriş etməniz tələb olunur.";
                    break;

                //  KeyNotFoundException - 404 (Bəzi sistem xətaları üçün)
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    break;

                //  Gözlənilməz Server xətaları - 500
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    // Development mühitində xətanın detallarını görmək üçün (opsional)
                    message = "Daxili server xətası baş verdi. Zəhmət olmasa bir az sonra yenidən yoxlayın.";
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                status = context.Response.StatusCode,
                message = message,
                errors = errors,
                
                type = exception.GetType().Name
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}