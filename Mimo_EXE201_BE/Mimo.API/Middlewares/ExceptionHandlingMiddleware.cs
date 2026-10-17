using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Mimo.DomainLayer.Exceptions;

namespace Mimo.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Unhandled Exception: {Message}", exception.Message);

            context.Response.ContentType = "application/json";

            int statusCode = StatusCodes.Status500InternalServerError;
            string message = "Đã xảy ra lỗi máy chủ nội bộ. Vui lòng thử lại sau.";

            switch (exception)
            {
                case BaseHttpException baseHttpEx:
                    statusCode = baseHttpEx.StatusCode;
                    message = baseHttpEx.Message;
                    break;

                case BadHttpRequestException badRequestEx:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = badRequestEx.Message;
                    break;

                case UnauthorizedAccessException unauthorizedEx:
                    statusCode = StatusCodes.Status401Unauthorized;
                    message = unauthorizedEx.Message;
                    break;

                default:
                    // Trong production không để lộ raw stack trace
                    break;
            }

            context.Response.StatusCode = statusCode;

            var response = new
            {
                Error = message,
                Code = statusCode,
                Timestamp = DateTime.UtcNow
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}
