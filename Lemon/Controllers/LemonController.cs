using FluentValidation;
using Lemon.Services.Exceptions;
using Lemon.Services.Response;
using Microsoft.AspNetCore.Mvc;

namespace Lemon.Controllers;

/// <summary>
/// 基础控制器，提供统一的响应方法
/// </summary>
[ApiController]
public abstract class LemonController(IResponseBuilder responseBuilder) : ControllerBase
{
    protected readonly IResponseBuilder _responseBuilder = responseBuilder;

    #region 成功响应

    /// <summary>
    /// 返回成功响应
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="data">响应数据</param>
    /// <param name="message">自定义消息</param>
    /// <returns></returns>
    protected IActionResult Success<T>(T? data = default, string? message = null)
    {
        Response.Headers["X-Lemon-Success"] = "true";
        return Ok(_responseBuilder.Success(data, message));
    }

    /// <summary>
    /// 返回成功响应（无泛型）
    /// </summary>
    /// <param name="data">响应数据</param>
    /// <param name="message">自定义消息</param>
    /// <returns></returns>
    protected IActionResult Success(object? data = null, string? message = null)
    {
        Response.Headers["X-Lemon-Success"] = "true";
        return Ok(_responseBuilder.Success(data, message));
    }

    /// <summary>
    /// 返回成功消息
    /// </summary>
    /// <param name="message">成功消息</param>
    /// <returns></returns>
    protected IActionResult SuccessWithMessage(string message)
    {
        Response.Headers["X-Lemon-Success"] = "true";
        return Ok(_responseBuilder.Success(null, message));
    }

    #endregion

    #region 错误响应

    /// <summary>
    /// 返回一般错误响应
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <returns></returns>
    protected IActionResult Error(string? message = null)
    {
        return Ok(_responseBuilder.Error(message));
    }

    /// <summary>
    /// 返回参数错误响应
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <returns></returns>
    protected IActionResult BadRequest(string? message = null)
    {
        return Ok(_responseBuilder.BadRequest(message));
    }

    /// <summary>
    /// 返回未授权响应
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <returns></returns>
    protected IActionResult Unauthorized(string? message = null)
    {
        return Ok(_responseBuilder.Unauthorized(message));
    }

    /// <summary>
    /// 返回禁止访问响应
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <returns></returns>
    protected IActionResult Forbidden(string? message = null)
    {
        return Ok(_responseBuilder.Forbidden(message));
    }

    /// <summary>
    /// 返回未找到响应
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <returns></returns>
    protected IActionResult NotFound(string? message = null)
    {
        return Ok(_responseBuilder.Create(ResponseCodes.Failure, null, message ?? "资源未找到"));
    }

    #endregion

    #region 自定义响应

    /// <summary>
    /// 返回自定义状态码响应
    /// </summary>
    /// <param name="code">状态码</param>
    /// <param name="data">数据</param>
    /// <param name="message">消息</param>
    /// <returns></returns>
    protected IActionResult CustomResponse(int code, object? data = null, string? message = null)
    {
        return Ok(_responseBuilder.Create(code, data, message));
    }

    /// <summary>
    /// 返回自定义状态码响应（泛型）
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="code">状态码</param>
    /// <param name="data">数据</param>
    /// <param name="message">消息</param>
    /// <returns></returns>
    protected IActionResult CustomResponse<T>(int code, T? data = default, string? message = null)
    {
        return Ok(_responseBuilder.Create(code, data, message));
    }

    #endregion

    #region 分页响应

    /// <summary>
    /// 返回分页数据响应
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="items">数据列表</param>
    /// <param name="total">总数</param>
    /// <returns></returns>
    protected IActionResult PagedSuccess<T>(IEnumerable<T> items, long total)
    {
        Response.Headers["X-Lemon-Success"] = "true";
        return Success(new { Items = items, Total = total });
    }

    #endregion

    #region 验证辅助方法

    /// <summary>
    /// 使用 FluentValidation 验证模型
    /// </summary>
    /// <typeparam name="T">模型类型</typeparam>
    /// <param name="model">要验证的模型</param>
    /// <param name="validator">验证器</param>
    /// <returns>如果验证失败返回错误响应，否则返回null</returns>
    protected static async Task ValidateAsync<T>(T? request, IValidator<T> validator)
    {
        if (request == null)
        {
            throw new BadRequestException("参数错误");
        }

        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.FirstOrDefault()?.ErrorMessage;
            throw new BadRequestException(firstError ?? "参数错误");
        }
    }

    /// <summary>
    /// 检查参数是否为空
    /// </summary>
    /// <param name="value">参数值</param>
    /// <param name="paramName">参数名</param>
    /// <returns></returns>
    protected IActionResult? CheckNull(object? value, string paramName)
    {
        if (value == null)
        {
            return BadRequest($"参数 {paramName} 不能为空");
        }

        return null;
    }

    /// <summary>
    /// 检查ID是否有效
    /// </summary>
    /// <param name="id">ID值</param>
    /// <param name="paramName">参数名</param>
    /// <returns></returns>
    protected IActionResult? CheckId(int id, string paramName = "id")
    {
        if (id <= 0)
        {
            return BadRequest($"参数 {paramName} 必须大于0");
        }

        return null;
    }

    #endregion

    #region 查询参数辅助方法

    /// <summary>
    /// 从查询参数中获取分页信息
    /// </summary>
    /// <param name="defaultPage">默认页码，默认为1</param>
    /// <param name="defaultLimit">默认每页数量，默认为10</param>
    /// <returns>返回页码和每页数量的元组</returns>
    protected (int page, int limit) GetPageAndLimit(int defaultPage = 1, int defaultLimit = 10)
    {
        // 从查询参数中获取 page
        var pageQuery = Request.Query["page"].FirstOrDefault();
        var page = defaultPage;
        if (
            !string.IsNullOrEmpty(pageQuery)
            && int.TryParse(pageQuery, out var parsedPage)
            && parsedPage > 0
        )
        {
            page = parsedPage;
        }

        // 从查询参数中获取 limit
        var limitQuery = Request.Query["limit"].FirstOrDefault();
        var limit = defaultLimit;
        if (
            !string.IsNullOrEmpty(limitQuery)
            && int.TryParse(limitQuery, out var parsedLimit)
            && parsedLimit > 0
        )
        {
            limit = parsedLimit;
        }

        return (page, limit);
    }

    #endregion
}
