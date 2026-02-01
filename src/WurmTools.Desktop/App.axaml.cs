using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
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
            // Create and show the window immediately so it appears on screen
            var window = new MainWindow();
            desktop.MainWindow = window;

            // Show a loading message while we initialize the database
            window.Content = new StackPanel
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Spacing = 12,
                Children =
                {
                    new Border
                    {
                        Width = 40, Height = 40,
                        CornerRadius = new CornerRadius(6),
                        Background = Avalonia.Media.Brush.Parse("#e94560"),
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    },
                    new TextBlock
                    {
                        Text = "Loading WurmTools...",
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        FontSize = 16,
                        Foreground = Avalonia.Media.Brush.Parse("#8899aa"),
                    }
                }
            };

            // Perform async initialization after the window is shown
            window.Opened += async (_, _) =>
            {
                try
                {
                    Console.WriteLine("[WurmTools] Starting database initialization...");

                    var dbPath = GetDatabasePath();
                    Console.WriteLine($"[WurmTools] Database path: {dbPath}");

                    var db = new DatabaseConnection(dbPath);
                    db.EnsureSchema();
                    Console.WriteLine("[WurmTools] Schema ensured.");

                    var repo = new ItemRepository(db);
                    var count = await repo.GetCountAsync();
                    Console.WriteLine($"[WurmTools] Item count: {count}");

                    if (count == 0)
                    {
                        var sourceDir = GetSourceDataPath();
                        Console.WriteLine($"[WurmTools] Source data path: {sourceDir}");
                        if (Directory.Exists(sourceDir))
                        {
                            await DatabaseBuilder.BuildFromSourceAsync(sourceDir, db);
                            Console.WriteLine("[WurmTools] Data imported.");
                        }
                        else
                        {
                            Console.WriteLine("[WurmTools] Source data directory not found.");
                        }
                    }

                    var viewModel = new ItemBrowserViewModel(repo);
                    window.Content = null; // Clear loading message
                    window.DataContext = viewModel;

                    // Re-load the XAML content now that DataContext is set
                    window.Content = new WurmTools.Modules.Items.Views.ItemBrowserView();

                    Console.WriteLine("[WurmTools] Window initialized successfully.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[WurmTools] Startup error: {ex}");
                    window.Content = new TextBlock
                    {
                        Text = $"Startup error:\n\n{ex}",
                        Margin = new Thickness(20),
                        TextWrapping = TextWrapping.Wrap
                    };
                }
            };
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
