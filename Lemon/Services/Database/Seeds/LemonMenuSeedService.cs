using Lemon.Models;
using Lemon.Services.Constants;
using Microsoft.Extensions.Logging;

namespace Lemon.Services.Database.Seeds;

public class LemonMenuSeedService(IFreeSql freeSql, ILogger<LemonMenuSeedService> logger)
    : BaseDataSeedService(freeSql, logger)
{
    public override string Name => "菜单数据填充";
    public override int Priority => 2;

    private const string Create = "create";
    private const string Update = "update";
    private const string Delete = "delete";
    private const string Handle = "handle";
    private const string Import = "import";
    private const string Export = "export";

    private static readonly string[] AllActions = [Create, Update, Delete, Handle, Import, Export];

    private static readonly Dictionary<string, string> ActionLabels = new()
    {
        { Create, "添加" },
        { Update, "修改" },
        { Delete, "删除" },
        { Handle, "处理" },
        { Import, "导入" },
        { Export, "导出" },
    };

    protected override async Task<bool> IsDataExistsAsync()
    {
        return await HasDataAsync<LemonMenu>();
    }

    protected override async Task SeedDataAsync()
    {
        var systemMenu = new LemonMenu
        {
            MenuType = MenuConstants.Types.Menu,
            ParentId = 0,
            Title = "系统管理",
            Path = "/system",
            Permission = "system",
            Icon = "material-symbols:settings",
            Sort = 99,
            IsSystem = true,
        };

        var systemMenuId = (int)await _freeSql.Insert(systemMenu).ExecuteIdentityAsync();

        var menuList = new List<LemonMenu>
        {
            new()
            {
                MenuType = MenuConstants.Types.Menu,
                ParentId = systemMenuId,
                Title = "用户管理",
                Path = "/user",
                Permission = "user.list",
                IsSystem = true,
            },
            new()
            {
                MenuType = MenuConstants.Types.Menu,
                ParentId = systemMenuId,
                Title = "角色管理",
                Path = "/role",
                Permission = "role.list",
                IsSystem = true,
            },
            new()
            {
                MenuType = MenuConstants.Types.Menu,
                ParentId = systemMenuId,
                Title = "菜单管理",
                Path = "/menu",
                Permission = "menu.list",
                IsSystem = true,
            },
            new()
            {
                MenuType = MenuConstants.Types.Menu,
                ParentId = systemMenuId,
                Title = "配置管理",
                Path = "/config",
                Permission = "config.list",
                IsSystem = true,
            },
            new()
            {
                MenuType = MenuConstants.Types.Menu,
                ParentId = systemMenuId,
                Title = "操作日志",
                Path = "/log",
                Permission = "log",
                IsSystem = true,
            },
        };

        foreach (var menu in menuList)
        {
            var menuId = (int)await _freeSql.Insert(menu).ExecuteIdentityAsync();

            if (menu.Permission.Contains('.'))
            {
                var perfix = menu.Permission.Split('.')[0];

                await _freeSql.Insert(GetActions(menuId, perfix)).ExecuteAffrowsAsync();
            }
        }
    }

    private static List<LemonMenu> GetActions(int parentId, string perfix, string[]? actions = null)
    {
        actions ??= [Create, Update, Delete];

        var menus = new List<LemonMenu>();

        foreach (var action in actions)
        {
            if (!AllActions.Contains(action))
            {
                continue;
            }

            menus.Add(
                new()
                {
                    MenuType = MenuConstants.Types.Button,
                    ParentId = parentId,
                    Title = ActionLabels[action],
                    Path = "",
                    Permission = perfix + "." + action,
                    IsSystem = true,
                }
            );
        }

        return menus;
    }
}
