using Lemon.Dtos;

namespace Lemon.Business.System;

public interface ILogService
{
    Task<PageResponse<LogResponse>> GetLogsAsync(LogQueryRequest request);
}
