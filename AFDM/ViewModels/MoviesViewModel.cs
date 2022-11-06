using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;

using AFDM.Contracts.Services;
using AFDM.Contracts.ViewModels;
using AFDM.Core.Contracts.Services;
using AFDM.Core.Models;
using AFDM.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.Options;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Newtonsoft.Json.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace AFDM.ViewModels;

public class MoviesViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly IDataService _dataService;
    private readonly IFileService _fileService;
    private readonly LocalSettingsOptions _settings;

    public ICommand ItemClickCommand
    {
        get;
    }

    public ICommand RefreshAllClickCommand
    {
        get;
    }

    public ICommand MenuRefreshMetadataClickCommand
    {
        get;
    }

    public ICommand FilterClickCommand
    {
        get;
    }



    public ObservableCollection<Movie> Source { get; } = new ObservableCollection<Movie>();

    public MoviesViewModel(INavigationService navigationService, IDataService dataService, IFileService fileService)
    {
        _navigationService = navigationService;
        _dataService = dataService;
        _fileService = fileService;

        // IOptions<LocalSettingsOptions> settings add to inputs
        //_settings = settings.Value;  // TODO: implement settings on load

        ItemClickCommand = new RelayCommand<Movie>(OnItemClick);
        RefreshAllClickCommand = new RelayCommand(OnRefreshAllClick);
        MenuRefreshMetadataClickCommand = new RelayCommand<Movie>(OnMenuRefreshMetadataClick);
        FilterClickCommand = new RelayCommand<string>(OnFilterClickCommand);
    }

    public async void OnNavigatedTo(object parameter)
    {
        // TODO: added counter to prevent page refresh every time we go there
        if (Source.Count == 0)
        {
            Source.Clear();

            var data = await _dataService.GetMoviesGridDataAsync();
            foreach (var item in data)
            {
                Source.Add(item);
            }
        }
    }

    public void OnNavigatedFrom()
    {
    }

    private void OnItemClick(Movie? clickedItem)
    {
        if (clickedItem != null)
        {
            _navigationService.SetListDataItemForNextConnectedAnimation(clickedItem);
            _navigationService.NavigateTo(typeof(MoviesDetailViewModel).FullName!, clickedItem.ID);
        }
    }

    private async void OnRefreshAllClick()
    {
        if (Source.Count > 0 && Source != null)
        {
            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            ParallelOptions options = new() { MaxDegreeOfParallelism = 4 };
            await Parallel.ForEachAsync(Source, options, async (item, token) =>
            {
                await dispatcherQueue.EnqueueAsync(async () =>
                {
                    item.IsAvailable = false;
                    await _dataService.UpdateMovieViaBestWebMatchAsync(item);
                    item.IsAvailable = true;
                });
            });
        }
    }


    private async void OnMenuRefreshMetadataClick(Movie? clickedItem)
    {
        if (clickedItem != null)
        {
            // Flag item on UI
            clickedItem.IsAvailable = false;

            // Refresh movie metadata
            var refreshMovie = await _dataService.UpdateMovieViaBestWebMatchAsync(clickedItem);

            // Enable item on UI
            clickedItem.IsAvailable = true;

        }
    }

    private void OnFilterClickCommand(object clickedItem)
    {
        int x = 0;
    }
}
