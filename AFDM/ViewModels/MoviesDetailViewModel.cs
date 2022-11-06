using AFDM.Contracts.ViewModels;
using AFDM.Core.Contracts.Services;
using AFDM.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;

namespace AFDM.ViewModels;

public class MoviesDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly IDataService _dataService;
    private Movie? _item;

    public Movie? Item
    {
        get => _item;
        set => SetProperty(ref _item, value);
    }

    public MoviesDetailViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is string id)
        {
            var data = await _dataService.GetMoviesGridDataAsync();
            Item = data.First(i => i.ID == id);
        }
    }

    public void OnNavigatedFrom()
    {
    }
}
