using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Lemon.Services.Cache;

/// <summary>
/// 混合缓存服务实现，结合内存缓存和Redis缓存
/// </summary>
public class HybridCacheService(
    IMemoryCache memoryCache,
    IConnectionMultiplexer? connectionMultiplexer,
    ILogger<HybridCacheService> logger
) : IHybridCacheService
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly IDatabase? _database = connectionMultiplexer?.GetDatabase();
    private readonly ILogger<HybridCacheService> _logger = logger;

    /// <summary>
    /// 同时设置内存缓存和Redis缓存
    /// </summary>
    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        try
        {
            // 设置内存缓存
            await SetMemoryCacheAsync(key, value, ttl);

            // 设置Redis缓存
            await SetRedisCacheAsync(key, value, ttl);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置混合缓存失败，键: {Key}", key);

            return false;
        }
    }

    /// <summary>
    /// 仅设置内存缓存
    /// </summary>
    public async Task<bool> SetMemoryCacheAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        try
        {
            var options = new MemoryCacheEntryOptions();
            if (ttl.HasValue)
            {
                options.SetAbsoluteExpiration(ttl.Value);
            }

            _memoryCache.Set(key, value, options);
            await Task.CompletedTask;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置内存缓存失败，键: {Key}", key);

            return false;
        }
    }

    /// <summary>
    /// 仅设置Redis缓存
    /// </summary>
    public async Task<bool> SetRedisCacheAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        if (_database == null)
        {
            _logger.LogWarning("Redis连接不可用，跳过Redis缓存设置，键: {Key}", key);
            return true;
        }

        try
        {
            var jsonValue = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, jsonValue, ttl);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置Redis缓存失败，键: {Key}", key);

            return false;
        }
    }

    /// <summary>
    /// 从缓存中获取数据（优先从内存缓存获取，如果不存在则从Redis获取）
    /// </summary>
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            // 先尝试从内存缓存获取
            var memoryResult = await GetFromMemoryCacheAsync<T>(key);
            if (memoryResult != null)
            {
                return memoryResult;
            }

            // 如果内存缓存中没有，从Redis获取
            var redisResult = await GetFromRedisCacheAsync<T>(key);
            if (redisResult != null)
            {
                // 将Redis中的数据同步到内存缓存中（不设置TTL，因为Redis中已经有TTL控制）
                await SetMemoryCacheAsync(key, redisResult);
                return redisResult;
            }

            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取混合缓存失败，键: {Key}", key);

            return default;
        }
    }

    /// <summary>
    /// 从内存缓存中获取数据
    /// </summary>
    public async Task<T?> GetFromMemoryCacheAsync<T>(string key)
    {
        try
        {
            if (_memoryCache.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            await Task.CompletedTask;

            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从内存缓存获取数据失败，键: {Key}", key);

            return default;
        }
    }

    /// <summary>
    /// 从Redis缓存中获取数据
    /// </summary>
    public async Task<T?> GetFromRedisCacheAsync<T>(string key)
    {
        if (_database == null)
        {
            _logger.LogWarning("Redis连接不可用，无法从Redis获取数据，键: {Key}", key);

            return default;
        }

        try
        {
            var value = await _database.StringGetAsync(key);
            if (value.HasValue)
            {
                return JsonSerializer.Deserialize<T>(value.ToString());
            }

            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从Redis缓存获取数据失败，键: {Key}", key);

            return default;
        }
    }

    /// <summary>
    /// 从两种缓存中删除数据
    /// </summary>
    public async Task<bool> RemoveAsync(string key)
    {
        try
        {
            // 从内存缓存删除
            await RemoveFromMemoryCacheAsync(key);

            // 从Redis缓存删除
            await RemoveFromRedisCacheAsync(key);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除混合缓存失败，键: {Key}", key);

            return false;
        }
    }

    /// <summary>
    /// 仅从内存缓存中删除数据
    /// </summary>
    public async Task<bool> RemoveFromMemoryCacheAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            await Task.CompletedTask;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从内存缓存删除数据失败，键: {Key}", key);

            return false;
        }
    }

    /// <summary>
    /// 仅从Redis缓存中删除数据
    /// </summary>
    public async Task<bool> RemoveFromRedisCacheAsync(string key)
    {
        if (_database == null)
        {
            return true;
        }

        try
        {
            await _database.KeyDeleteAsync(key);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从Redis缓存删除数据失败，键: {Key}", key);

            return false;
        }
    }

    /// <summary>
    /// 检查键是否存在于任一缓存中
    /// </summary>
    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            // 检查内存缓存
            if (await ExistsInMemoryCacheAsync(key))
            {
                return true;
            }

            // 检查Redis缓存
            return await ExistsInRedisCacheAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查混合缓存键是否存在失败，键: {Key}", key);

            return false;
        }
    }

    /// <summary>
    /// 检查键是否存在于内存缓存中
    /// </summary>
    public async Task<bool> ExistsInMemoryCacheAsync(string key)
    {
        try
        {
            var exists = _memoryCache.TryGetValue(key, out _);
            await Task.CompletedTask;

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查内存缓存键是否存在失败，键: {Key}", key);

            return false;
        }
    }

    /// <summary>
    /// 检查键是否存在于Redis缓存中
    /// </summary>
    public async Task<bool> ExistsInRedisCacheAsync(string key)
    {
        if (_database == null)
        {
            return false;
        }

        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查Redis缓存键是否存在失败，键: {Key}", key);

            return false;
        }
    }
}
