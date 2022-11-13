using System.Collections;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using AFDM.Core.Contracts.Services;
using AFDM.Core.Models;
using F23.StringSimilarity;
using HtmlAgilityPack;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using static System.Net.WebRequestMethods;

namespace AFDM.Core.Services;

public class DataService : IDataService
{
    private readonly IFileService _fileService;

    private readonly string _appMoviesFolder = @"C:\Users\Lewis\source\repos\AFDM\AFDM\Data\Movies\";  // TODO: use local app folder
    private readonly string _userMoviesFolder = @"E:\Film\Complete";                                   // TODO: migrate this to a call to local settings
    private readonly string _iafdRoot = "https://www.iafd.com";
    private readonly string _iafdSearchMany = @"/results.asp?searchtype=comprehensive&searchstring={0}";
    private readonly string _iafdSearchSingle = @"/title.rme/title={0}/year={1}";

    private List<Movie> _allMovies;

    public DataService(IFileService fileService)
    {
        _fileService = fileService;
    }

    #region Get data functions
    public async Task<IEnumerable<Movie>> GetMoviesGridDataAsync()
    {
        if (_allMovies == null)
        {
            _allMovies = new List<Movie>(GetAllMoviesFromJsonFolder());
        }

        // TODO: add this as allMovies is not null after rescan, so gri ddoesnt update until app restart
        if (_allMovies.Count == 0)
        {
            _allMovies = new List<Movie>(GetAllMoviesFromJsonFolder());
        }

        await Task.CompletedTask;
        return _allMovies;
    }
    #endregion

    #region Web functions
    public async Task<Movie> UpdateMovieViaBestWebMatchAsync(Movie movie)
    {
        if (movie.Name != null)
        {
            // Set movie to processing
            movie.IsAvailable = false;

            // Fetch all matching movie titles in IAFD database
            var searchResults = await GetMatchingMoviesViaIAFDAsync(movie.Name, movie.Year);
            if (searchResults.Count > 0)
            {
                // Auto-choose first item in search result (i.e., best match)
                var bestMatch = searchResults.First();
                var iafdResult = await GetSpecificMovieViaIAFDAsync(bestMatch.Name, bestMatch.Year);

                // Create new movie and update with new data
                movie.UpdateViaIAFDResult(iafdResult);
            }

            // Release movie from processing
            movie.IsAvailable = true;

            // Overwrite json file
            _fileService.Save(_appMoviesFolder, movie.ID + ".json", movie);
        }

        await Task.CompletedTask;
        return movie;
    }
    private async Task<List<SearchResultIAFD>> GetMatchingMoviesViaIAFDAsync(string movieName, string movieYear)
    {
        var results = new List<SearchResultIAFD>();
        if (!string.IsNullOrEmpty(movieName))
        {
            var web = new HtmlWeb();
            var url = string.Format(_iafdSearchMany, HttpUtility.UrlEncode(movieName));
            url = _iafdRoot + url.ToLower();
            var doc = await web.LoadFromWebAsync(url);

            // Extract movie results table rows and parse data if exists
            var elems = doc.DocumentNode.SelectNodes("//table[@id='titleresult']/tbody//tr");
            if (elems != null)
            {
                // Requery full IAFD table rows when extra results exist
                var moreElems = doc.DocumentNode.SelectSingleNode("//*[text() = 'See More Results...']");
                if (moreElems != null)
                {
                    url = HttpUtility.HtmlDecode(moreElems.GetAttributeValue("href", null));
                    doc = await web.LoadFromWebAsync(_iafdRoot + url);
                    elems = doc.DocumentNode.SelectNodes("//table[@id='titleresult']/tbody//tr");
                }

                // Get each movie row, calculate similarity and store it
                var matcher = new Levenshtein();
                foreach (var elem in elems)
                {
                    SearchResultIAFD result = new SearchResultIAFD();

                    var data = elem.ChildNodes;
                    result.Name = data[0].InnerText.Trim();
                    result.Year = data[1].InnerText.Trim();
                    result.Url = HttpUtility.HtmlDecode(data[0].FirstChild.GetAttributeValue("href", null));
                    result.NameMatch = matcher.Distance(movieName, result.Name);

                    if (movieYear != null && movieYear.Length == 4)
                    {
                        var yearMatch = Convert.ToInt32(result.Year) - Convert.ToInt32(movieYear);
                        result.YearMatch = Math.Abs(yearMatch);
                    }

                    results.Add(result);
                }

                // Sort by match (i.e., lowest to highest value)
                results = results.OrderBy(e => e.NameMatch)
                                 .ThenBy(e => e.YearMatch).ToList();
            }
        }

        await Task.CompletedTask;
        return results;
    }
    private async Task<MovieResultIAFD> GetSpecificMovieViaIAFDAsync(string movieName, string movieYear)
    {
        var result = new MovieResultIAFD();

        if (!string.IsNullOrEmpty(movieName) && !string.IsNullOrEmpty(movieYear))
        {
            var web = new HtmlWeb();
            var url = string.Format(_iafdSearchSingle, HttpUtility.UrlEncode(movieName), movieYear);
            url = _iafdRoot + url.ToLower();
            var doc = await web.LoadFromWebAsync(url);

            // Extract movie data if movie was returned
            var heading = doc.DocumentNode.SelectSingleNode("//h1");
            if (heading != null)
            {
                // Extract movie namd and year
                result.Name = heading.InnerText.Split('(', ')')[0].Trim();
                result.Year = heading.InnerText.Split('(', ')')[1].Trim();

                // Extract movie details (e.g., directors, studio, etc.)
                var bioElems = doc.DocumentNode.SelectNodes("//p[@class='bioheading']");
                if (bioElems != null)
                {
                    foreach (var bioElem in bioElems)
                    {
                        var bioValue = bioElem.NextSibling.InnerText.Trim();
                        if (!string.IsNullOrEmpty(bioValue) && bioValue != "No Data")
                        {
                            switch (bioElem.InnerText.Trim())
                            {
                                case "Minutes":
                                    result.Minutes = bioValue;
                                    break;

                                case "Directors":
                                    var directorElems = bioElem.NextSibling.ChildNodes.Where(e => e.Name == "a");
                                    foreach (var directorElem in directorElems)
                                    {
                                        result.Directors.Add(directorElem.InnerText);
                                    }
                                    break;

                                case "Distributor":
                                    result.Distributor = bioValue;
                                    break;

                                case "Studio":
                                    result.Studio = bioValue;
                                    break;

                                case "All-Girl":
                                    result.AllGirl = bioValue;
                                    break;

                                case "All-Male":
                                    result.AllBoy = bioValue;
                                    break;

                                case "Compilation":
                                    result.Compilation = bioValue;
                                    break;

                                case "Release Date":
                                    result.ReleaseDate = bioValue;
                                    break;

                                case "Date Added to IAFD":
                                    result.DateAdded = bioValue;
                                    break;
                            }
                        }
                    }
                }

                // Extract actor data
                var castElems = doc.DocumentNode.SelectNodes("//div[@class='castbox']//p");
                if (castElems != null)
                {
                    foreach (var castElem in castElems)
                    {
                        var newActor = new ActorResultIAFD();

                        var allItems = castElem.ChildNodes;

                        // Extract actor image
                        var img = allItems.FindFirst("img");
                        newActor.ImageUrl = img.GetAttributeValue("src", null);

                        // Extract actor name, profile url
                        var profile = allItems.FindFirst("a");
                        newActor.Name = profile.InnerText.Trim();
                        newActor.ProfileUrl = _iafdRoot + profile.GetAttributeValue("href", null);

                        // Extract credited
                        if (allItems.FindFirst("i") != null)
                        {
                            newActor.Credited = allItems.FindFirst("i").InnerText.Trim();                            
                        };

                        var textItems = allItems.Where(e => e.Name == "#text" && e.InnerText != "&nbsp;");

                        // Extract acts if exists
                        if (textItems.Count() > 0)
                        {
                            newActor.Acts = textItems.First().InnerText.Trim();
                        }

                        result.Actors.Add(newActor);
                    }
                }
                
                // Extract scenes data
                var sceneElems = doc.DocumentNode.SelectNodes("//div[@id='sceneinfo']//li");
                if (sceneElems != null)
                {
                    foreach (var sceneElem in sceneElems)
                    {
                        result.Scenes.Add(sceneElem.InnerText.Trim());
                    }
                }
                
                // Extract synopsis data
                var synopsisElems = doc.DocumentNode.SelectNodes("//div[@id='synopsis']//li");
                if (synopsisElems != null)
                {
                    foreach (var synopsisElem in synopsisElems)
                    {
                        result.Synopsis.Add(synopsisElem.InnerText.Trim());
                    }
                }

                // Extract awards data
                var awardsElems = doc.DocumentNode.SelectNodes("//div[@id='awards']/ul/li/ul");
                if (awardsElems != null)
                {
                    foreach (var awardsElem in awardsElems)
                    {
                        var newAward = new AwardResultIAFD();

                        // Extract award event
                        var awardEvent = awardsElem.PreviousSibling.InnerText.Trim();

                        // Extract all awards for award event
                        var awardEntries = awardsElem.ChildNodes;
                        foreach(var awardEntry in awardEntries)
                        {
                            newAward.Award.Add(awardEntry.InnerText.Trim()); // TODO: this might need further thought for actors
                        }

                        result.Awards.Add(newAward);
                    }
                }

                // Extract shops data
                var shopElems = doc.DocumentNode.SelectNodes("//div[@id='commerce']//p[@class='item']//a");
                if (shopElems != null)
                {
                    foreach (var shopElem in shopElems)
                    {
                        var shop = new ShopResultIAFD();

                        // Extract shop name and url
                        shop.Name = shopElem.InnerText.Trim();
                        shop.Url = _iafdRoot +  shopElem.GetAttributeValue("href", null);

                        result.Shops.Add(shop);
                    }
                }
            }
        }

        await Task.CompletedTask;
        return result;
    }
    #endregion

    #region Local file functions
    public void SyncMovieFoldersWithJSONFiles()
    {
        // Get all existing movies from json files
        var jsonMovies = GetAllMoviesFromJsonFolder();

        // Get all existing movies from user folders and files
        var userMovies = GetAllMoviesFromUserFolder();

        // Compare json and user movies and if no json of it, add it
        var existingJsonIDs = jsonMovies.Select(e => e.ID).ToList();
        foreach (var userMovie in userMovies)
        {
            if (!existingJsonIDs.Contains(userMovie.ID))
            {
                _fileService.Save(_appMoviesFolder, userMovie.ID + ".json", userMovie);
            }
        }

        // Compare json and user movies and if json of it but no user folder, remove it
        var existingUserIDs = userMovies.Select(e => e.ID).ToList();
        foreach (var jsonMovie in jsonMovies)
        {
            if (!existingUserIDs.Contains(jsonMovie.ID))
            {
                _fileService.Delete(_appMoviesFolder, jsonMovie.ID + ".json");
            }
        }
    }  // TODO: hook up with viewmodel scan
    private IEnumerable<Movie> GetAllMoviesFromJsonFolder()
    {
        // Get all existing movies from json files ordered by name
        var jsonMovies = new List<Movie>();
        if (Directory.Exists(_appMoviesFolder))
        {
            var jsonMovieFiles = Directory.GetFiles(_appMoviesFolder);
            foreach (var jsonMovieFile in jsonMovieFiles)
            {
                jsonMovies.Add(_fileService.Read<Movie>(_appMoviesFolder, jsonMovieFile));
            }
        }

        return jsonMovies.OrderBy(e => e.Name).ToList();
    }
    private IEnumerable<Movie> GetAllMoviesFromUserFolder()
    {
        // Build all existing movies from user folders and files
        var userMovies = new List<Movie>();
        if (Directory.Exists(_userMoviesFolder))
        {
            var userMovieFolders = Directory.GetDirectories(_userMoviesFolder);
            foreach (var userMovieFolder in userMovieFolders)
            {
                var folderName = new DirectoryInfo(userMovieFolder).Name;

                string movieID;
                using (var hash = SHA256.Create())
                {
                    var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(folderName));
                    movieID = Convert.ToHexString(bytes);
                }

                var parts = folderName.Split('(', ')');
                var movieName = parts[0].Trim();
                var movieYear = parts.Length > 1 ? parts[1].Trim() : null;

                var movieImages = GetImageFiles(userMovieFolder, true);
                var movieCovers = GetCoverImageFiles(movieImages);
                var movieFrontCover = movieCovers.GetValueOrDefault("FrontCover");
                var movieBackCover = movieCovers.GetValueOrDefault("BackCover");

                userMovies.Add(new Movie()
                {
                    ID = movieID,
                    Folder = folderName,
                    Name = movieName,
                    Year = movieYear,
                    FrontCoverImagePath = movieFrontCover,
                    BackCoverImagePath = movieBackCover,
                    IsAvailable = true
                });
            }
        }

        return userMovies;
    }
    #endregion

    #region Helper functions (TODO: move elsewhere?)
    private static List<string> GetImageFiles(string folderPath, bool fullPath)
{
    var imageFiles = new List<string>();
    if (Directory.Exists(folderPath))
    {
        var files = Directory.GetFiles(folderPath);
        foreach (var file in files)
        {
            var exts = @"\.jpg$|\.png$"; // TODO: add more extensions?
            if (Regex.IsMatch(file, exts))
            {
                var imageFile = fullPath ? Path.Combine(folderPath, file) : file;
                imageFiles.Add(imageFile);
            }
        }
    }

    return imageFiles;
}
    private static Dictionary<string, string> GetCoverImageFiles(List<string> imageFiles)
    {
        var coverImages = new Dictionary<string, string>();
        if (imageFiles != null && imageFiles.Count > 0)
        {
            foreach (var imageFile in imageFiles)
            {
                var filename = Path.GetFileNameWithoutExtension(imageFile);
                if (filename.EndsWith("-1") || filename == "poster")
                {
                    if (!coverImages.ContainsKey("FrontCover"))
                    {
                        coverImages.Add("FrontCover", imageFile);  // TODO: what if we have 2 matches? Key error.
                    }
                }
                else if ((filename.EndsWith("-2") || filename == "fanart") && !coverImages.ContainsKey("BackCover"))
                {
                    if (!coverImages.ContainsKey("FrontCover"))
                    {
                        coverImages.Add("BackCover", imageFile);  // TODO: what if we have 2 matches? Key error.
                    }
                }
            }
        }

        return coverImages;
    }
    #endregion
}
