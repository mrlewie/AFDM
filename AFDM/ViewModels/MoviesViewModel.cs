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
    //private readonly IFileService _fileService;

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

    public ObservableCollection<Movie> MovieDataAll { get; } = new ObservableCollection<Movie>();
    public ObservableCollection<Movie> MovieDataFiltered { get; set; }

    // TODO: bind to view instead of using view method?
    private string selectedFilter;
    public string SelectedFilter
    {
        get
        {
            return selectedFilter;
        }
        set
        {
            selectedFilter = value;
            //OnItemFilterClick("Act", selectedFilter);   // TODO: hook up label
        } 
    }

    public MoviesViewModel(INavigationService navigationService, IDataService dataService, IFileService fileService)
    {
        _navigationService = navigationService;
        _dataService = dataService;
        //_fileService = fileService;

        // IOptions<LocalSettingsOptions> settings add to inputs
        //_settings = settings.Value;  // TODO: implement settings on load

        ItemClickCommand = new RelayCommand<Movie>(OnItemClick);
        RefreshAllClickCommand = new RelayCommand(OnRefreshAllClick);
        MenuRefreshMetadataClickCommand = new RelayCommand<Movie>(OnMenuRefreshMetadataClick);
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (MovieDataAll.Count == 0)
        {
            // Get movies grid data
            MovieDataAll.Clear();
            var gridData = await _dataService.GetMoviesGridDataAsync();
            foreach (var item in gridData)
            {
                MovieDataAll.Add(item);
            }

            // Set filter object to new copy
            MovieDataFiltered = new ObservableCollection<Movie>(MovieDataAll);
        }
    }

    public void OnMovieFilterClick(string filterType, string filterValue)
    {
        MovieDataFiltered.Clear();
        //MovieDataFiltered = MovieDataAll;

        var tempDataFiltered = new List<Movie>();

        if (filterType == "General" && filterValue == "All")
        {
            foreach (var item in MovieDataAll)
            {
                MovieDataFiltered.Add(item);
            }
        }
        else if (filterType == "General" && filterValue == "Unplayed")
        {
            // TODO: reselect whole source data
        }
        else if (filterType == "Act")
        {
            foreach (var item in MovieDataAll)
            {
                if (item.Acts.Select(e => e.ShortName).Contains(filterValue))
                {
                    MovieDataFiltered.Add(item);
                }
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
        if (MovieDataAll.Count > 0 && MovieDataAll != null)
        {
            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            ParallelOptions options = new() { MaxDegreeOfParallelism = 4 };
            await Parallel.ForEachAsync(MovieDataAll, options, async (item, token) =>
            {
                try
                {
                    await dispatcherQueue.EnqueueAsync(async () =>
                    {
                        await _dataService.UpdateMovieViaBestWebMatchAsync(item);
                    });
                }
                catch (Exception e)
                {
                    int x = 0;
                }
            });
        }
    }

    private async void OnMenuRefreshMetadataClick(Movie? clickedItem)
    {
        if (clickedItem != null)
        {
            await _dataService.UpdateMovieViaBestWebMatchAsync(clickedItem);
        }
    }
}
