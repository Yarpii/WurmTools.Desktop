namespace WurmTools.Data;

using Dapper;
using WurmTools.Core.Models;
using WurmTools.Core.Services;

public class ItemRepository : IItemRepository
{
    private readonly DatabaseConnection _db;

    public ItemRepository(DatabaseConnection db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Item>> SearchAsync(string query, int limit = 50)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            var all = await _db.Connection.QueryAsync<Item>(
                "SELECT * FROM items ORDER BY name LIMIT @Limit",
                new { Limit = limit });
            return all.ToList();
        }

        // FTS5 search with prefix matching for type-as-you-search
        var ftsQuery = string.Join(" ", query.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(term => term + "*"));

        var results = await _db.Connection.QueryAsync<Item>("""
            SELECT i.* FROM items i
            INNER JOIN items_fts fts ON i.id = fts.rowid
            WHERE items_fts MATCH @Query
            ORDER BY rank
            LIMIT @Limit
            """,
            new { Query = ftsQuery, Limit = limit });

        return results.ToList();
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        return await _db.Connection.QuerySingleOrDefaultAsync<Item>(
            "SELECT * FROM items WHERE id = @Id",
            new { Id = id });
    }

    public async Task<IReadOnlyList<Item>> GetByCategoryAsync(string category)
    {
        var results = await _db.Connection.QueryAsync<Item>(
            "SELECT * FROM items WHERE category = @Category ORDER BY name",
            new { Category = category });
        return results.ToList();
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync()
    {
        var results = await _db.Connection.QueryAsync<string>(
            "SELECT DISTINCT category FROM items WHERE category IS NOT NULL ORDER BY category");
        return results.ToList();
    }

    public async Task<int> GetCountAsync()
    {
        return await _db.Connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM items");
    }
}
