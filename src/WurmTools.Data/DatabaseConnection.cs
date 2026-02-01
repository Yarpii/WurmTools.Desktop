namespace WurmTools.Data;

using Microsoft.Data.Sqlite;

public class DatabaseConnection : IDisposable
{
    private readonly SqliteConnection _connection;

    public DatabaseConnection(string databasePath)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        _connection = new SqliteConnection(connectionString);
        _connection.Open();
    }

    public SqliteConnection Connection => _connection;

    public void EnsureSchema()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = DatabaseSchema.CreateTables;
        cmd.ExecuteNonQuery();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
