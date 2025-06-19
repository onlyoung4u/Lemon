using Lemon.Business.Base;
using Lemon.Dtos;
using Lemon.Models;
using Lemon.Services.Constants;
using Lemon.Services.Exceptions;
using Lemon.Services.Jwt;
using Lemon.Services.Permission;
using Lemon.Services.Utils;

namespace Lemon.Business.Auth;

public class AuthService(
    IFreeSql freeSql,
    IJwtService jwtService,
    IPermissionService permissionService
) : BaseService(freeSql), IAuthService
{
    private readonly IJwtService _jwtService = jwtService;
    private readonly IPermissionService _permissionService = permissionService;

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string clientIp)
    {
        var user = await _freeSql
            .Select<LemonUser>()
            .Where(x => x.Username == request.Username)
            .Where(x => x.DeletedAt == null)
            .ToOneAsync();

        if (user == null || !BCryptHelper.VerifyPassword(request.Password, user.Password))
        {
            throw new BusinessException("用户名或密码错误");
        }

        if (!user.IsActive)
        {
            throw new BusinessException("用户已禁用");
        }

        var userInfo = new JwtUserInfo(user.Id.ToString(), user.Username, user.Nickname);
        var token = await _jwtService.GenerateToken(userInfo);

        await _freeSql
            .Update<LemonUser>(user.Id)
            .Set(x => x.LastLoginIp, clientIp)
            .Set(x => x.LastLoginTime, DateTime.Now)
            .ExecuteAffrowsAsync();

        return new LoginResponse { Token = token };
    }

    public async Task LogoutAsync(string token)
    {
        var result = await _jwtService.RevokeToken(token);

        if (!result)
        {
            throw new BusinessException();
        }
    }

    public async Task<UserInfoResponse> GetUserInfoAsync(int userId)
    {
        var user = await _freeSql
            .Select<LemonUser>()
            .IncludeMany(x => x.Roles)
            .Where(x => x.Id == userId)
            .FirstAsync();

        var roles = userId == 1 ? ["超级管理员"] : user.Roles.Select(x => x.Name).ToList();

        return new UserInfoResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Nickname = user.Nickname,
            Roles = roles,
        };
    }

    public async Task<List<string>> GetPermissionsAsync(int userId)
    {
        return await _permissionService.GetUserPermissionsAsync(userId);
    }

    private static MenuTreeResponse HandleMenu(LemonMenu menu, string parentPath)
    {
        var fullPath = parentPath + menu.Path;

        var component = string.Empty;
        if (!string.IsNullOrEmpty(parentPath) && string.IsNullOrEmpty(menu.Link))
        {
            component = fullPath + "/index";
        }

        var meta = new MenuMeta { Title = menu.Title };

        if (!string.IsNullOrEmpty(menu.Icon))
            meta.Icon = menu.Icon;

        if (!string.IsNullOrEmpty(menu.Link))
            meta.Link = menu.Link;

        var menuResponse = new MenuTreeResponse
        {
            Name = menu.Permission,
            Path = fullPath,
            Meta = meta,
        };

        if (!string.IsNullOrEmpty(component))
        {
            menuResponse.Component = component;
        }

        return menuResponse;
    }

    private static List<MenuTreeResponse> HandleMenuTree(
        List<LemonMenu> menus,
        int parentId = 0,
        string path = ""
    )
    {
        var tree = new List<MenuTreeResponse>();

        foreach (var menu in menus)
        {
            if (menu.ParentId == parentId)
            {
                var menuDto = HandleMenu(menu, path);

                var children = HandleMenuTree(menus, menu.Id, path + menu.Path);

                if (children.Count > 0)
                {
                    menuDto.Children = children;
                }

                tree.Add(menuDto);
            }
        }

        return tree;
    }

    public async Task<List<MenuTreeResponse>> GetMenusAsync(int userId)
    {
        var menus = new List<LemonMenu>();

        if (userId == 1)
        {
            menus = await _freeSql
                .Select<LemonMenu>()
                .Where(x => x.MenuType == MenuConstants.Types.Menu)
                .OrderBy(x => x.Sort)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }
        else
        {
            var permissions = await _permissionService.GetUserPermissionsAsync(userId);

            if (permissions.Count > 0)
            {
                menus = await _freeSql
                    .Select<LemonMenu>()
                    .Where(x => x.MenuType == MenuConstants.Types.Menu)
                    .Where(x => permissions.Contains(x.Permission))
                    .OrderBy(x => x.Sort)
                    .OrderBy(x => x.Id)
                    .ToListAsync();
            }
        }

        var mainMenu = new MenuTreeResponse
        {
            Name = "dashboard",
            Path = "/dashboard",
            Redirect = "/workspace",
            Meta = new MenuMeta
            {
                Order = -1,
                Title = "首页",
                Icon = "lucide:layout-dashboard",
            },
            Children =
            [
                new()
                {
                    Name = "workspace",
                    Path = "/workspace",
                    Component = "/dashboard/workspace/index",
                    Meta = new MenuMeta
                    {
                        Title = "工作台",
                        Icon = "carbon:workspace",
                        AffixTab = true,
                        HideInMenu = true,
                        ActivePath = "/dashboard",
                    },
                },
            ],
        };

        var menuTree = HandleMenuTree(menus);

        menuTree.Insert(0, mainMenu);

        return menuTree;
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, int userId)
    {
        var user = await _freeSql.Select<LemonUser>().Where(x => x.Id == userId).FirstAsync();

        if (!BCryptHelper.VerifyPassword(request.OldPassword, user.Password))
        {
            throw new BusinessException("旧密码错误");
        }

        await _freeSql
            .Update<LemonUser>(user.Id)
            .Set(x => x.Password, BCryptHelper.HashPassword(request.NewPassword))
            .ExecuteAffrowsAsync();
    }
}
