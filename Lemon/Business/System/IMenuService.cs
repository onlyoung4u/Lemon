using Lemon.Dtos;

namespace Lemon.Business.System;

public interface IMenuService
{
    Task<List<MenuTree>> GetMenusAsync(bool withButton);
    Task CreateMenuAsync(MenuCreateRequest request);
    Task UpdateMenuAsync(int id, MenuUpdateRequest request);
    Task DeleteMenuAsync(int id);
}
