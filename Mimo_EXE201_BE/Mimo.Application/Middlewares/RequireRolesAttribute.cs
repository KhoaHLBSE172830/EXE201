using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Mimo.Application.DTOs;
using Mimo.Application.ResponseCode;
using Mimo.Application.Services;
using Mimo.DomainLayer.Constants;
using static Mimo.DomainLayer.Enum.GeneralEnum;

namespace Mimo.Application.Middlewares;

/// <summary>
/// Bộ lọc xác thực và phân quyền truy cập theo 4 Roles: Admin, Student, Curator, Organization
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class RequireRolesAttribute : Attribute, IAuthorizationFilter
{
    private readonly UserRoleEnum[] _allowedRoles;

    public RequireRolesAttribute(params UserRoleEnum[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Result = ErrorResp.Unauthorized("Yêu cầu mã xác thực Bearer token.");
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
                context.Result = ErrorResp.Unauthorized("Token không hợp lệ hoặc đã hết hạn.");
                return;
            }

            // Lưu payload vào HttpContext Items
            context.HttpContext.Items[JwtConst.PAYLOAD_KEY] = payload;

            // Kiểm tra phân quyền Role nếu có yêu cầu
            if (_allowedRoles.Length > 0)
            {
                var roleString = payload.Role;
                if (string.IsNullOrEmpty(roleString) || 
                    !Enum.TryParse<UserRoleEnum>(roleString, true, out var userRole) || 
                    !_allowedRoles.Contains(userRole))
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Result = ErrorResp.Forbidden("Bạn không có quyền truy cập chức năng này.");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Result = ErrorResp.Unauthorized(ex.Message);
        }
    }
}
