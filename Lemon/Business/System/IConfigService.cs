using Lemon.Dtos;

namespace Lemon.Business.System;

public interface IConfigService
{
    Task<PageResponse<ConfigResponse>> GetConfigsAsync(ConfigQueryRequest request);
    Task<ConfigDetailResponse> GetConfigAsync(int id);
    Task<ConfigDetailResponse> GetConfigAsync(string key);
    Task<T> GetConfigValueAsync<T>(string key, T? defaultValue = default);
    Task CreateConfigAsync(ConfigRequest request);
    Task UpdateConfigAsync(int id, ConfigRequest request);
    Task DeleteConfigAsync(int id);
}
