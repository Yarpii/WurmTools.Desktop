namespace WurmTools.Data.Import;

using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using WurmTools.Core.Models;

public class JsonItemImporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly DatabaseConnection _db;

    public JsonItemImporter(DatabaseConnection db)
    {
        _db = db;
    }

    public async Task<int> ImportFromFileAsync(string jsonFilePath)
    {
        var json = await File.ReadAllTextAsync(jsonFilePath);
        var items = JsonSerializer.Deserialize<List<Item>>(json, JsonOptions);

        if (items is null || items.Count == 0)
            return 0;

        return await ImportItemsAsync(items);
    }

    public async Task<int> ImportItemsAsync(IEnumerable<Item> items)
    {
        const string sql = """
            INSERT INTO items (name, description, category, material, weight, skill, difficulty, is_container, is_improved, wiki_url, notes)
            VALUES (@Name, @Description, @Category, @Material, @Weight, @Skill, @Difficulty, @IsContainer, @IsImproved, @WikiUrl, @Notes)
            """;

        var count = 0;
        using var transaction = _db.Connection.BeginTransaction();

        foreach (var item in items)
        {
            await _db.Connection.ExecuteAsync(sql, new
            {
                item.Name,
                item.Description,
                item.Category,
                item.Material,
                item.Weight,
                item.Skill,
                item.Difficulty,
                IsContainer = item.IsContainer ? 1 : 0,
                IsImproved = item.IsImproved ? 1 : 0,
                item.WikiUrl,
                item.Notes
            }, transaction);
            count++;
        }

        transaction.Commit();
        return count;
    }
}
