using AFDM.Core.Models;

namespace AFDM.Core.Contracts.Services;

public interface IDataService
{
    Task<IEnumerable<Movie>> GetMoviesGridDataAsync();   
    Task<Movie> UpdateMovieViaBestWebMatchAsync(Movie movie);
    Task<List<SearchResultIAFD>> GetMatchingMoviesViaIAFDAsync(string movieName, string movieYear);
    Task<MovieResultIAFD> GetSpecificMovieViaIAFDAsync(string movieName, string movieYear);
    void SyncMovieFoldersWithJSONFiles();
}
