using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Lemon.Services.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Lemon.Services.Exceptions;

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class ExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlerMiddleware> logger,
    IResponseMessageService responseMessageService
)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger = logger;
    private readonly IResponseMessageService _responseMessageService = responseMessageService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, _logger);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        ILogger<ExceptionHandlerMiddleware> logger
    )
    {
        ApiResponse response;

        switch (exception)
        {
            case LemonException ex:
                response = CreateResponse(ex.Code, null, ex.Message);
                break;
            case ValidationException ex:
                response = HandleValidationException(ex);
                break;
            default:
                logger.LogError("请求异常: {Message}", exception.Message);
                response = CreateResponse(ResponseCodes.Failure, null, "服务器内部错误");
                break;
        }

        await context.Response.WriteAsJsonAsync(response, JsonOptions);
    }

    private ApiResponse HandleValidationException(ValidationException exception)
    {
        var error = exception.Errors.FirstOrDefault();
        return CreateResponse(ResponseCodes.BadRequest, null, error?.ErrorMessage);
    }

    private ApiResponse CreateResponse(int code, object? data = null, string? message = null)
    {
        var responseMessage = message ?? _responseMessageService.GetMessage(code);
        return new ApiResponse(code, responseMessage, data);
    }
}
