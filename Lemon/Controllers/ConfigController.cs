using FluentValidation;
using Lemon.Business.System;
using Lemon.Dtos;
using Lemon.Services.Attributes;
using Lemon.Services.Middleware;
using Lemon.Services.Response;
using Microsoft.AspNetCore.Mvc;

namespace Lemon.Controllers;

[ApiController]
[Route("admin/system/config")]
[RequireJwtAuth]
public class ConfigController(IResponseBuilder responseBuilder, IConfigService configService)
    : LemonController(responseBuilder)
{
    private readonly IConfigService _configService = configService;

    /// <summary>
    /// 获取配置列表
    /// </summary>
    /// <param name="request">查询参数</param>
    /// <param name="validator">验证器</param>
    /// <returns></returns>
    [HttpGet]
    [LemonAdmin("config.list")]
    public async Task<IActionResult> GetConfigs([FromQuery] ConfigQueryRequest request)
    {
        var result = await _configService.GetConfigsAsync(request);

        return Success(result);
    }

    /// <summary>
    /// 根据ID获取配置详情
    /// </summary>
    /// <param name="id">配置ID</param>
    /// <returns></returns>
    [HttpGet("{id:int:min(1)}")]
    [LemonAdmin("config.list")]
    public async Task<IActionResult> GetConfig(int id)
    {
        var result = await _configService.GetConfigAsync(id);

        return Success(result);
    }

    /// <summary>
    /// 根据键名获取配置详情
    /// </summary>
    /// <param name="key">配置键名</param>
    /// <returns></returns>
    [HttpGet("key/{key}")]
    public async Task<IActionResult> GetConfigByKey(string key)
    {
        var result = await _configService.GetConfigAsync(key);

        return Success(result);
    }

    /// <summary>
    /// 创建配置
    /// </summary>
    /// <param name="request">创建配置请求</param>
    /// <param name="validator">验证器</param>
    /// <returns></returns>
    [HttpPost]
    [LemonAdmin("config.create", "添加配置")]
    public async Task<IActionResult> CreateConfig(
        [FromBody] ConfigRequest request,
        IValidator<ConfigRequest> validator
    )
    {
        await ValidateAsync(request, validator);

        await _configService.CreateConfigAsync(request);

        return Success();
    }

    /// <summary>
    /// 更新配置
    /// </summary>
    /// <param name="id">配置ID</param>
    /// <param name="request">更新配置请求</param>
    /// <param name="validator">验证器</param>
    /// <returns></returns>
    [HttpPut("{id:int:min(1)}")]
    [LemonAdmin("config.update", "编辑配置")]
    public async Task<IActionResult> UpdateConfig(
        int id,
        [FromBody] ConfigRequest request,
        IValidator<ConfigRequest> validator
    )
    {
        await ValidateAsync(request, validator);

        await _configService.UpdateConfigAsync(id, request);

        return Success();
    }

    /// <summary>
    /// 删除配置
    /// </summary>
    /// <param name="id">配置ID</param>
    /// <returns></returns>
    [HttpDelete("{id:int:min(1)}")]
    [LemonAdmin("config.delete", "删除配置")]
    public async Task<IActionResult> DeleteConfig(int id)
    {
        await _configService.DeleteConfigAsync(id);

        return Success();
    }
}
