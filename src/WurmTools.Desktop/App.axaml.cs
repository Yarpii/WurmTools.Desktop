using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using WurmTools.Data;
using WurmTools.Modules.Items.ViewModels;

namespace WurmTools.Desktop;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dbPath = GetDatabasePath();
            var db = new DatabaseConnection(dbPath);
            db.EnsureSchema();

            // If the database is empty, try to build from source data
            var repo = new ItemRepository(db);
            if (repo.GetCountAsync().GetAwaiter().GetResult() == 0)
            {
                var sourceDir = GetSourceDataPath();
                if (Directory.Exists(sourceDir))
                {
                    DatabaseBuilder.BuildFromSourceAsync(sourceDir, dbPath).GetAwaiter().GetResult();
                    // Reconnect after rebuild
                    db.Dispose();
                    db = new DatabaseConnection(dbPath);
                    repo = new ItemRepository(db);
                }
            }

            var viewModel = new ItemBrowserViewModel(repo);
            desktop.MainWindow = new MainWindow { DataContext = viewModel };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static string GetDatabasePath()
    {
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WurmTools");
        Directory.CreateDirectory(appData);
        return Path.Combine(appData, "wurmtools.sqlite");
    }

    private static string GetSourceDataPath()
    {
        // Look for data/source relative to the executable
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var sourceDir = Path.Combine(baseDir, "..", "..", "..", "..", "..", "data", "source");
        if (Directory.Exists(sourceDir))
            return Path.GetFullPath(sourceDir);

        // Fallback: look relative to working directory
        sourceDir = Path.Combine(Directory.GetCurrentDirectory(), "data", "source");
        return sourceDir;
    }
}
