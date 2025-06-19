using FluentValidation;
using Lemon.Business.System;
using Lemon.Dtos;
using Lemon.Services.Attributes;
using Lemon.Services.Middleware;
using Lemon.Services.Response;
using Microsoft.AspNetCore.Mvc;

namespace Lemon.Controllers;

[ApiController]
[Route("admin/system/log")]
[RequireJwtAuth]
public class LogController(IResponseBuilder responseBuilder, ILogService logService)
    : LemonController(responseBuilder)
{
    private readonly ILogService _logService = logService;

    [HttpGet]
    [LemonAdmin("log")]
    public async Task<IActionResult> GetLogs(
        [FromQuery] LogQueryRequest request,
        IValidator<LogQueryRequest> validator
    )
    {
        await ValidateAsync(request, validator);

        var result = await _logService.GetLogsAsync(request);

        return Success(result);
    }
}
