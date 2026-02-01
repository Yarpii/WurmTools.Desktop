namespace WurmTools.Core.Services;

using WurmTools.Core.Models;

public interface IItemRepository
{
    Task<IReadOnlyList<Item>> SearchAsync(string query, int limit = 50);
    Task<Item?> GetByIdAsync(int id);
    Task<IReadOnlyList<Item>> GetByCategoryAsync(string category);
    Task<IReadOnlyList<string>> GetCategoriesAsync();
    Task<int> GetCountAsync();
}
