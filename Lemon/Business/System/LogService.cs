using Lemon.Business.Base;
using Lemon.Dtos;
using Lemon.Models;
using Mapster;

namespace Lemon.Business.System;

public class LogService(IFreeSql freeSql) : BaseService(freeSql), ILogService
{
    public async Task<PageResponse<LogResponse>> GetLogsAsync(LogQueryRequest request)
    {
        var query = _freeSql
            .Select<LemonOperationLog>()
            .WhereIf(request.UserId.HasValue, x => x.UserId == request.UserId.Value)
            .WhereIf(request.StartTime.HasValue, x => x.CreatedAt >= request.StartTime.Value)
            .WhereIf(request.EndTime.HasValue, x => x.CreatedAt <= request.EndTime.Value)
            .WhereIf(!string.IsNullOrEmpty(request.Method), x => x.Method == request.Method);

        var total = await query.CountAsync();

        var logs = await query
            .OrderByDescending(x => x.CreatedAt)
            .Page(request.Page, request.Limit)
            .ToListAsync();

        var items = logs.Adapt<List<LogResponse>>();

        return new PageResponse<LogResponse> { Items = items, Total = total };
    }
}
