namespace WurmTools.Data;

using WurmTools.Data.Import;

public class DatabaseBuilder
{
    /// <summary>
    /// Imports source JSON files into an existing database connection.
    /// </summary>
    public static async Task<int> BuildFromSourceAsync(string sourceDirectory, DatabaseConnection db)
    {
        db.EnsureSchema();

        var importer = new JsonItemImporter(db);
        var totalImported = 0;

        var jsonFiles = Directory.GetFiles(sourceDirectory, "*.json", SearchOption.TopDirectoryOnly);

        foreach (var file in jsonFiles.OrderBy(f => f))
        {
            var count = await importer.ImportFromFileAsync(file);
            totalImported += count;
        }

        // Store build metadata
        using var cmd = db.Connection.CreateCommand();
        cmd.CommandText = "INSERT OR REPLACE INTO meta (key, value) VALUES ('build_date', @date), ('item_count', @count)";
        cmd.Parameters.AddWithValue("@date", DateTime.UtcNow.ToString("O"));
        cmd.Parameters.AddWithValue("@count", totalImported.ToString());
        cmd.ExecuteNonQuery();

        return totalImported;
    }
}
