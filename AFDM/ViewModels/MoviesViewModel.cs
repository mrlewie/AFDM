using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Xml.Linq;
using AFDM.Contracts.Services;
using AFDM.Contracts.ViewModels;
using AFDM.Core.Contracts.Services;
using AFDM.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;

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
    public ICommand ScanMoviesFolderCommand
    {
        get;
    }
    public ICommand ChangeMovieCardSizeChangeCommand
    {
        get;
    }

    //public ICommand FixMatchItemClickCommand
    //{
        //get;
    //}

    public ObservableCollection<Movie> MovieDataAll { get; } = new ObservableCollection<Movie>();
    public ObservableCollection<Movie> MovieDataFiltered { get; } = new ObservableCollection<Movie>();
    public ObservableCollection<SearchResultIAFD> UserFixMatchIAFDSearchResults { get; set; } = new ObservableCollection<SearchResultIAFD>();


    private string? _selectedFilter;
    public string? SelectedFilter
    {
        get => _selectedFilter;
        set => _selectedFilter = value;
    }

    private double _movieCardWidth = 180; // medium
    public double MovieCardWidth
    {
        get => _movieCardWidth;
        set => SetProperty(ref _movieCardWidth, value);  // use this to trigger change on view!
    }

    private double _movieCardHeight = 260; // medium
    public double MovieCardHeight
    {
        get => _movieCardHeight;
        set => SetProperty(ref _movieCardHeight, value);  // use this to trigger change on view!
    }


    private string? _userMovieTitleQuery;
    public string? UserMovieTitleQuery
    {
        get => _userMovieTitleQuery;
        set => SetProperty(ref _userMovieTitleQuery, value);  // use this to trigger change on view!
    }

    private string? _userMovieYearQuery;
    public string? UserMovieYearQuery
    {
        get => _userMovieYearQuery;
        set => SetProperty(ref _userMovieYearQuery, value);  // use this to trigger change on view!
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
        ScanMoviesFolderCommand = new RelayCommand(OnScanFoldersClick);
        ChangeMovieCardSizeChangeCommand = new RelayCommand<string>(OnMovieCardSizeChanged);
        //FixMatchItemClickCommand = new RelayCommand<SearchResultIAFD>(OnFixMatchItemClick);

    }

    public async void OnNavigatedTo(object parameter)
    {
        if (MovieDataAll.Count == 0)
        {
            MovieDataAll.Clear();
            
            var movies = await _dataService.GetMoviesGridDataAsync();
            foreach (var movie in movies)
            {
                MovieDataAll.Add(movie);
                MovieDataFiltered.Add(movie);
            }

            // Set filter object to new copy
            //MovieDataFiltered = new ObservableCollection<Movie>(MovieDataAll);
        }
    }

    public void OnMovieFilterClick(string filterType, string filterValue)
    {
        MovieDataFiltered.Clear();

        if (filterType == "General" && filterValue == "All")
        {
            foreach (var movie in MovieDataAll)
            {
                MovieDataFiltered.Add(movie);
            }
        }
        else if (filterType == "Act")
        {
            foreach (var movie in MovieDataAll)
            {
                if (movie.Acts.Select(e => e.ShortName).Contains(filterValue))
                {
                    MovieDataFiltered.Add(movie);
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


    // TODO: improve this func
    private async void OnScanFoldersClick()
    {
        _dataService.SyncMovieFoldersWithJSONFiles();

        // TODO: improve this by adding only movies not in viewmodel already
        MovieDataAll.Clear();
        MovieDataFiltered.Clear();

        var movies = await _dataService.GetMoviesGridDataAsync();
        foreach (var movie in movies)
        {
            MovieDataAll.Add(movie);
            MovieDataFiltered.Add(movie);
        }
    }

    public void OnMovieCardSizeChanged(string? requestedSize)
    {
        if (requestedSize == "Small")
        {
            MovieCardWidth = 144;
            MovieCardHeight = 207;
        }
        else if (requestedSize == "Medium")
        {
            MovieCardWidth = 180;
            MovieCardHeight = 258;
        }
        else if (requestedSize == "Large")
        {
            MovieCardWidth = 230;
            MovieCardHeight = 331;
        }
    }

    public async void OnFixMatchItemClick(Movie? clickedMovie, SearchResultIAFD? clickedSearchResult)
    {
        if (clickedMovie != null && clickedSearchResult != null)
        {
            clickedMovie.IsAvailable = false;

            var movieResult = await _dataService.GetSpecificMovieViaIAFDAsync(clickedSearchResult.Name, clickedSearchResult.Year);
            clickedMovie.UpdateViaIAFDResult(movieResult);

            clickedMovie.IsAvailable = true;
        }
    }


    public async void OnUserMovieQuery()
    {
        UserFixMatchIAFDSearchResults.Clear();

        if (!string.IsNullOrEmpty(UserMovieTitleQuery) && string.IsNullOrEmpty(UserMovieYearQuery))
        {
            var movies = await _dataService.GetMatchingMoviesViaIAFDAsync(UserMovieTitleQuery, null);
            
            foreach (var movie in movies)
            {
                var searchResult = new SearchResultIAFD()
                {
                    Name = movie.Name,
                    Year = movie.Year,
                    Url  = movie.Url
                };
                UserFixMatchIAFDSearchResults.Add(searchResult);
            }
        }
        else if (!string.IsNullOrEmpty(UserMovieTitleQuery) && !string.IsNullOrEmpty(UserMovieYearQuery))
        {
            var movie = await _dataService.GetSpecificMovieViaIAFDAsync(UserMovieTitleQuery, UserMovieYearQuery);
            
            var searchResult = new SearchResultIAFD()
            {
                Name = movie.Name,
                Year = movie.Year,
                Url  = movie.Url
            };
            UserFixMatchIAFDSearchResults.Add(searchResult);
        }
    }
}
