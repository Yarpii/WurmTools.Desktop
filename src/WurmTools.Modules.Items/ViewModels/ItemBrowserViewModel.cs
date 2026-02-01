namespace WurmTools.Modules.Items.ViewModels;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WurmTools.Core.Models;
using WurmTools.Core.Services;

public class ItemBrowserViewModel : INotifyPropertyChanged
{
    private readonly IItemRepository _repository;
    private string _searchText = string.Empty;
    private Item? _selectedItem;
    private int _totalCount;
    private bool _isLoading;
    private CancellationTokenSource? _searchCts;

    public ItemBrowserViewModel(IItemRepository repository)
    {
        _repository = repository;
        Items = new ObservableCollection<Item>();
        _ = InitializeAsync();
    }

    public ObservableCollection<Item> Items { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value) return;
            _searchText = value;
            OnPropertyChanged();
            _ = SearchAsync();
        }
    }

    public Item? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem == value) return;
            _selectedItem = value;
            OnPropertyChanged();
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        private set { _totalCount = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); }
    }

    private async Task InitializeAsync()
    {
        TotalCount = await _repository.GetCountAsync();
        await SearchAsync();
    }

    private async Task SearchAsync()
    {
        // Cancel any pending search
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        // Small debounce
        try
        {
            await Task.Delay(150, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        IsLoading = true;

        try
        {
            var results = await _repository.SearchAsync(_searchText);
            if (token.IsCancellationRequested) return;

            Items.Clear();
            foreach (var item in results)
                Items.Add(item);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
