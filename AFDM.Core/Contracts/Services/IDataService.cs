using AFDM.Core.Models;

namespace AFDM.Core.Contracts.Services;

public interface IDataService
{
    Task<IEnumerable<Movie>> GetMoviesGridDataAsync();   
    Task<Movie> UpdateMovieViaBestWebMatchAsync(Movie movie);
    void SyncMovieFoldersWithJSONFiles();
}
