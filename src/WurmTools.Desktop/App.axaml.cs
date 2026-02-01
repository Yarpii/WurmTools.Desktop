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
            try
            {
                var dbPath = GetDatabasePath();
                var db = new DatabaseConnection(dbPath);
                db.EnsureSchema();

                // If the database is empty, import from source data
                var repo = new ItemRepository(db);
                if (repo.GetCountAsync().GetAwaiter().GetResult() == 0)
                {
                    var sourceDir = GetSourceDataPath();
                    if (Directory.Exists(sourceDir))
                    {
                        DatabaseBuilder.BuildFromSourceAsync(sourceDir, db).GetAwaiter().GetResult();
                    }
                }

                var viewModel = new ItemBrowserViewModel(repo);
                desktop.MainWindow = new MainWindow { DataContext = viewModel };
            }
            catch (Exception ex)
            {
                desktop.MainWindow = new MainWindow
                {
                    Content = new Avalonia.Controls.TextBlock
                    {
                        Text = $"Startup error:\n\n{ex}",
                        Margin = new Avalonia.Thickness(20),
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    }
                };
            }
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
        // Walk up from executable directory looking for data/source
        var dir = AppDomain.CurrentDomain.BaseDirectory;
        for (var i = 0; i < 8; i++)
        {
            var candidate = Path.Combine(dir, "data", "source");
            if (Directory.Exists(candidate))
                return Path.GetFullPath(candidate);

            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }

        // Fallback: look relative to working directory
        return Path.Combine(Directory.GetCurrentDirectory(), "data", "source");
    }
}
