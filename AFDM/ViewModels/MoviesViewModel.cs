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

    public ICommand ItemClickCommand
    {
        get;
    }
    public ICommand ItemFilterCommand
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

    public ObservableCollection<Movie> Source { get; } = new ObservableCollection<Movie>();

    public ObservableCollection<string> Filters { get; } = new ObservableCollection<string>();
    
    private string selectedFilter;
    public string SelectedFilter
    {
        get => selectedFilter;
        set
        {
            selectedFilter = value;
            OnItemFilterClick("Act", selectedFilter);   // TODO: hook up label
        } 
    }


    public MoviesViewModel(INavigationService navigationService, IDataService dataService, IFileService fileService)
    {
        _navigationService = navigationService;
        _dataService = dataService;
        _fileService = fileService;

        // IOptions<LocalSettingsOptions> settings add to inputs
        //_settings = settings.Value;  // TODO: implement settings on load

        ItemClickCommand = new RelayCommand<Movie>(OnItemClick);
        //ItemFilterCommand = new RelayCommand(OnItemFilterClick);
        RefreshAllClickCommand = new RelayCommand(OnRefreshAllClick);
        MenuRefreshMetadataClickCommand = new RelayCommand<Movie>(OnMenuRefreshMetadataClick);
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (Source.Count == 0)
        {
            // Get movies grid data
            Source.Clear();
            var gridData = await _dataService.GetMoviesGridDataAsync();
            foreach (var item in gridData)
            {
                Source.Add(item);
            }

            // Get movies filter data
            Filters.Clear();
            var filterData = await _dataService.GetMoviesFilterDataAsync();
            foreach (var item in filterData)
            {
                Filters.Add(item);
            }
        }
    }

    private async void OnItemFilterClick(string filterLabel, string filterValue)
    {
        //Source.Clear(); // TODO: use this when we persist data
        var data = await _dataService.GetFilteredMoviesGridDataAsync(filterLabel, filterValue);

        Source.Clear();
        foreach (var item in data)
        {
            Source.Add(item);
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

            try { 
                ParallelOptions options = new() { MaxDegreeOfParallelism = 4 };
                await Parallel.ForEachAsync(Source, options, async (item, token) =>
                {
                    await dispatcherQueue.EnqueueAsync(async () =>
                    {
                        item.IsAvailable = false;
                        await _dataService.UpdateMovieViaBestWebMatchAsync(item);


                        Filters.Clear();
                        var filterData = await _dataService.GetMoviesFilterDataAsync();
                        foreach (var item in filterData)
                        {
                            Filters.Add(item);
                        }


                        item.IsAvailable = true;
                    });
                });
            }
            catch (Exception e)
            {
                int x = 0;
            }
        }
    }

    private async void OnMenuRefreshMetadataClick(Movie? clickedItem)
    {
        if (clickedItem != null)
        {
            // Flag clicked item processing
            clickedItem.IsAvailable = false;

            // Get and update item via IAFD web data
            await _dataService.UpdateMovieViaBestWebMatchAsync(clickedItem);

            // Reset the filter values
            Filters.Clear();
            var filterData = await _dataService.GetMoviesFilterDataAsync();
            foreach (var item in filterData)
            {
                Filters.Add(item);
            }

            // Unflag item as processing
            clickedItem.IsAvailable = true;
        }
    }
}
