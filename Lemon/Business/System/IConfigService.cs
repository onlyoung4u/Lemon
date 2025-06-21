using Lemon.Dtos;

namespace Lemon.Business.System;

public interface IConfigService
{
    Task<PageResponse<ConfigResponse>> GetConfigsAsync(ConfigQueryRequest request);
    Task<ConfigDetailResponse> GetConfigAsync(int id);
    Task<ConfigDetailResponse> GetConfigAsync(string key);
    Task CreateConfigAsync(ConfigRequest request);
    Task UpdateConfigAsync(int id, ConfigRequest request);
    Task DeleteConfigAsync(int id);
}
