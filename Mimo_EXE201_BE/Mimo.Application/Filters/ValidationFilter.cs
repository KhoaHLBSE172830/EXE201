using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mimo.Application.Filters
{
    /// <summary>
    /// Bộ lọc tự động kích hoạt FluentValidation cho tất cả Request DTOs trước khi vào Controller Action
    /// Chuẩn hóa phản hồi lỗi theo format chung { error, code, timestamp, errors }
    /// </summary>
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 1. Kiểm tra ModelState từ ASP.NET Core (lỗi binding kiểu dữ liệu hoặc parse JSON)
            if (!context.ModelState.IsValid)
            {
                var modelErrors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                context.Result = new BadRequestObjectResult(new
                {
                    error = "Dữ liệu yêu cầu không hợp lệ.",
                    code = StatusCodes.Status400BadRequest,
                    timestamp = DateTime.UtcNow,
                    errors = modelErrors
                });
                return;
            }

            // 2. Tự động kiểm tra và thực thi IValidator<T> tương ứng với từng DTO
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null) continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
                if (context.HttpContext.RequestServices.GetService(validatorType) is IValidator validator)
                {
                    var validationContext = new ValidationContext<object>(argument);
                    var validationResult = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                    if (!validationResult.IsValid)
                    {
                        var errors = validationResult.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(e => e.ErrorMessage).ToArray()
                            );

                        context.Result = new BadRequestObjectResult(new
                        {
                            error = "Dữ liệu yêu cầu không hợp lệ.",
                            code = StatusCodes.Status400BadRequest,
                            timestamp = DateTime.UtcNow,
                            errors
                        });
                        return;
                    }
                }
            }

            await next();
        }
    }
}
