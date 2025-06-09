namespace Lemon.Services.Cache;

/// <summary>
/// 混合缓存服务接口，支持内存缓存和Redis缓存
/// </summary>
public interface IHybridCacheService
{
    /// <summary>
    /// 同时设置内存缓存和Redis缓存
    /// </summary>
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? ttl = null);

    /// <summary>
    /// 仅设置内存缓存
    /// </summary>
    Task<bool> SetMemoryCacheAsync<T>(string key, T value, TimeSpan? ttl = null);

    /// <summary>
    /// 仅设置Redis缓存
    /// </summary>
    Task<bool> SetRedisCacheAsync<T>(string key, T value, TimeSpan? ttl = null);

    /// <summary>
    /// 从缓存中获取数据（优先从内存缓存获取，如果不存在则从Redis获取）
    /// </summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// 从内存缓存中获取数据
    /// </summary>
    Task<T?> GetFromMemoryCacheAsync<T>(string key);

    /// <summary>
    /// 从Redis缓存中获取数据
    /// </summary>
    Task<T?> GetFromRedisCacheAsync<T>(string key);

    /// <summary>
    /// 从两种缓存中删除数据
    /// </summary>
    Task<bool> RemoveAsync(string key);

    /// <summary>
    /// 仅从内存缓存中删除数据
    /// </summary>
    Task<bool> RemoveFromMemoryCacheAsync(string key);

    /// <summary>
    /// 仅从Redis缓存中删除数据
    /// </summary>
    Task<bool> RemoveFromRedisCacheAsync(string key);

    /// <summary>
    /// 检查键是否存在于任一缓存中
    /// </summary>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// 检查键是否存在于内存缓存中
    /// </summary>
    Task<bool> ExistsInMemoryCacheAsync(string key);

    /// <summary>
    /// 检查键是否存在于Redis缓存中
    /// </summary>
    Task<bool> ExistsInRedisCacheAsync(string key);
}
