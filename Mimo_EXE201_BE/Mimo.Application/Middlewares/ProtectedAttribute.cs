using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Mimo.Application.DTOs;
using Mimo.Application.ResponseCode;
using Mimo.Application.Services;
using Mimo.DomainLayer.Constants;

namespace Mimo.Application.Middlewares;

// Middleware to protect routes
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class ProtectedAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Result = ErrorResp.Unauthorized("No token provided");
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        try
        {
            var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtService>();
            var payload = jwtService.ValidateToken(token);

            if (payload == null)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Result = ErrorResp.Unauthorized("Invalid or expired token");
                return;
            }

            // add payload to context
            context.HttpContext.Items[JwtConst.PAYLOAD_KEY] = payload;
        }
        catch (Exception e)
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Result = ErrorResp.Unauthorized(e.Message);
            return;
        }
    }
}
