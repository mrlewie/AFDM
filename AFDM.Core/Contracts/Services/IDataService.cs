using AFDM.Core.Models;

namespace AFDM.Core.Contracts.Services;

public interface IDataService
{
    Task<IEnumerable<Movie>> GetMoviesGridDataAsync();
    Task<IEnumerable<string>> GetMoviesFilterDataAsync();
    
    Task<Movie> UpdateMovieViaBestWebMatchAsync(Movie movie);
    //Task<IEnumerable<string>> GetMoviesFiltersDataAsync();
    Task<IEnumerable<Movie>> GetFilteredMoviesGridDataAsync(string filterLabel, string filterValue);
}
