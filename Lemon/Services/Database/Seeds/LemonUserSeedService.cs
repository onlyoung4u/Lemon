using Lemon.Models;
using Lemon.Services.Utils;
using Microsoft.Extensions.Logging;

namespace Lemon.Services.Database.Seeds;

public class LemonUserSeedService(IFreeSql freeSql, ILogger<LemonUserSeedService> logger)
    : BaseDataSeedService(freeSql, logger)
{
    public override string Name => "管理员数据填充";
    public override int Priority => 1;

    protected override async Task<bool> IsDataExistsAsync()
    {
        return await HasDataAsync<LemonUser>();
    }

    protected override async Task SeedDataAsync()
    {
        var adminUser = new LemonUser
        {
            Username = "admin",
            Nickname = "超级管理员",
            Password = BCryptHelper.HashPassword("lemonstudio"),
        };

        await _freeSql.Insert(adminUser).ExecuteAffrowsAsync();
    }
}
