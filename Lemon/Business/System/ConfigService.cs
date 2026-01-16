using System.Text.Json;
using Lemon.Business.Base;
using Lemon.Dtos;
using Lemon.Models;
using Lemon.Services.Exceptions;
using Mapster;

namespace Lemon.Business.System;

public class ConfigService(IFreeSql freeSql) : BaseService(freeSql), IConfigService
{
    public async Task<PageResponse<ConfigResponse>> GetConfigsAsync(ConfigQueryRequest request)
    {
        var query = _freeSql
            .Select<LemonConfig>()
            .WhereIf(!string.IsNullOrEmpty(request.Name), x => x.Name.Contains(request.Name))
            .WhereIf(!string.IsNullOrEmpty(request.Key), x => x.Key.Contains(request.Key))
            .OrderBy(x => x.Id);

        var total = await query.CountAsync();

        var configs = await query.Page(request.Page, request.Limit).ToListAsync();

        var items = configs.Adapt<List<ConfigResponse>>();

        return new() { Items = items, Total = total };
    }

    public async Task<ConfigDetailResponse> GetConfigAsync(int id)
    {
        var config =
            await _freeSql.Select<LemonConfig>().Where(x => x.Id == id).ToOneAsync()
            ?? throw new BusinessException("配置不存在");

        return config.Adapt<ConfigDetailResponse>();
    }

    public async Task<ConfigDetailResponse> GetConfigAsync(string key)
    {
        var config =
            await _freeSql.Select<LemonConfig>().Where(x => x.Key == key).ToOneAsync()
            ?? throw new BusinessException("配置不存在");

        return config.Adapt<ConfigDetailResponse>();
    }

    public async Task<T> GetConfigValueAsync<T>(string key, T? defaultValue = default)
    {
        var config = await _freeSql
            .Select<LemonConfig>()
            .Where(x => x.Key == key && x.IsActive)
            .ToOneAsync();

        if (config == null || string.IsNullOrEmpty(config.Value))
        {
            return defaultValue!;
        }

        try
        {
            var targetType = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlyingType == typeof(string))
            {
                return (T)(object)config.Value;
            }

            if (underlyingType == typeof(Guid))
            {
                if (Guid.TryParse(config.Value, out var guidResult))
                {
                    return (T)(object)guidResult;
                }
                return defaultValue!;
            }

            if (underlyingType == typeof(TimeSpan))
            {
                if (TimeSpan.TryParse(config.Value, out var timeSpanResult))
                {
                    return (T)(object)timeSpanResult;
                }
                return defaultValue!;
            }

            if (underlyingType == typeof(DateTime))
            {
                if (DateTime.TryParse(config.Value, out var dateTimeResult))
                {
                    return (T)(object)dateTimeResult;
                }
                return defaultValue!;
            }

            if (underlyingType.IsPrimitive || underlyingType == typeof(decimal))
            {
                var converted = Convert.ChangeType(config.Value, underlyingType);
                return (T)converted;
            }

            return JsonSerializer.Deserialize<T>(config.Value) ?? defaultValue!;
        }
        catch
        {
            return defaultValue!;
        }
    }

    public async Task CreateConfigAsync(ConfigRequest request)
    {
        // 检查键名是否已存在
        var isKeyExist = await _freeSql
            .Select<LemonConfig>()
            .Where(x => x.Key == request.Key)
            .AnyAsync();

        if (isKeyExist)
        {
            throw new BusinessException("键名已存在");
        }

        var config = request.Adapt<LemonConfig>();

        await _freeSql.Insert(config).ExecuteAffrowsAsync();
    }

    public async Task UpdateConfigAsync(int id, ConfigRequest request)
    {
        // 检查配置是否存在
        var existingConfig =
            await _freeSql.Select<LemonConfig>().Where(x => x.Id == id).ToOneAsync()
            ?? throw new BusinessException("配置不存在");

        // 检查键名是否与其他配置冲突
        var isKeyExist = await _freeSql
            .Select<LemonConfig>()
            .Where(x => x.Key == request.Key && x.Id != id)
            .AnyAsync();

        if (isKeyExist)
        {
            throw new BusinessException("键名已存在");
        }

        await _freeSql
            .Update<LemonConfig>()
            .SetDto(request)
            .Where(x => x.Id == id)
            .ExecuteAffrowsAsync();
    }

    public async Task DeleteConfigAsync(int id)
    {
        var config =
            await _freeSql.Select<LemonConfig>().Where(x => x.Id == id).ToOneAsync()
            ?? throw new BusinessException("配置不存在");

        var affectedRows = await _freeSql
            .Delete<LemonConfig>()
            .Where(x => x.Id == id)
            .ExecuteAffrowsAsync();

        if (affectedRows == 0)
        {
            throw new BusinessException();
        }
    }
}
