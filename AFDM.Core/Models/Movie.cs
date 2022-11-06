using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using AFDM.Core.Helpers;

namespace AFDM.Core.Models;
public class Movie : INotifyPropertyChanged
{
    private string _ID;
    public string ID
    {
        get => _ID;
        set
        {
            _ID = value;
            NotifyPropertyChanged("ID");
        }
    }

    private string _folder;
    public string Folder
    {
        get => _folder;
        set
        {
            _folder = value;
            NotifyPropertyChanged("Folder");
        }
    }

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            NotifyPropertyChanged("Name");
        }
    }

    private string _year;
    public string Year
    {
        get => _year;
        set
        {
            _year = value;
            NotifyPropertyChanged("Year");
        }
    }

    private string _frontCoverImagePath;
    public string FrontCoverImagePath
    {
        get => _frontCoverImagePath;
        set
        {
            _frontCoverImagePath = value;
            NotifyPropertyChanged("FrontCoverImagePath");
        }
    }

    private string _backCoverImagePath;
    public string BackCoverImagePath
    {
        get => _backCoverImagePath;
        set
        {
            _backCoverImagePath = value;
            NotifyPropertyChanged("BackCoverImagePath");
        }
    }

    private string _duration;
    public string Duration
    {
        get => _duration;
        set
        {
            _duration = value;
            NotifyPropertyChanged("Duration");
        }
    }

    private List<MovieDirector> _directors = new();
    public List<MovieDirector> Directors
    {
        get => _directors;
        set
        {
            _directors = value;
            NotifyPropertyChanged("Directors");
        }
    }

    private string _studio;
    public string Studio
    {
        get => _studio;
        set
        {
            _studio = value;
            NotifyPropertyChanged("Studio");
        }
    }

    private string _allGirl;
    public string AllGirl
    {
        get => _allGirl;
        set
        {
            _allGirl = value;
            NotifyPropertyChanged("AllGirl");
        }
    }

    private string _allBoy;
    public string AllBoy
    {
        get => _allBoy;
        set
        {
            _allBoy = value;
            NotifyPropertyChanged("AllBoy");
        }
    }

    private string _compilation;
    public string Compilation
    {
        get => _compilation;
        set
        {
            _compilation = value;
            NotifyPropertyChanged("Compilation");
        }
    }

    private string _synopsis;
    public string Synopsis
    {
        get => _synopsis;
        set
        {
            _synopsis = value;
            NotifyPropertyChanged("Synopsis");
        }
    }

    private string _videoExtension;
    public string VideoExtension
    {
        get => _videoExtension;
        set
        {
            _videoExtension = value;
            NotifyPropertyChanged("VideoExtension");
        }
    }

    private List<MovieActor> _actors = new();
    public List<MovieActor> Actors
    {
        get => _actors;
        set
        {
            _actors = value;
            NotifyPropertyChanged("Actors");
        }
    }

    private List<MovieAct> _acts = new();
    public List<MovieAct> Acts
    {
        get => _acts;
        set
        {
            _acts = value;
            NotifyPropertyChanged("Acts");
        }
    }

    private List<MovieScene> _scenes = new();
    public List<MovieScene> Scenes
    {
        get => _scenes;
        set
        {
            _scenes = value;
            NotifyPropertyChanged("Scenes");
        }
    }

    // Awards

    //public ICollection<string> Shops
    //{
    //    get; set;
    //}

    private MovieResultIAFD _rawIAFDResult;
    public MovieResultIAFD RawIAFDResult
    {
        get => _rawIAFDResult;
        set
        {
            _rawIAFDResult = value;
            NotifyPropertyChanged("RawIAFDResult");
        }
    }

    private bool _isAvailable;
    public bool IsAvailable
    {
        get => _isAvailable;
        set
        {
            _isAvailable = value;
            NotifyPropertyChanged("IsAvailable");
        }
    }


    private void SetNameViaIAFD()
    {
        Name = null;
        var iafdName = RawIAFDResult.Name;

        if (!string.IsNullOrEmpty(iafdName))
        {
            Name = iafdName;
        }
    }

    private void SetYearViaIAFD()
    {
        Year = null;
        var iafdYear = RawIAFDResult.Year;

        if (!string.IsNullOrEmpty(iafdYear))
        {
            iafdYear = iafdYear.Replace("(", "");
            iafdYear = iafdYear.Replace(")", "");

            if (iafdYear.All(char.IsDigit))
            {
                Year = iafdYear;
            }
        }
    }

    private void SetDurationViaIAFD()
    {
        Duration = null;
        var iafdMinutes = RawIAFDResult.Minutes;

        if (!string.IsNullOrEmpty(iafdMinutes))
        {
            if (iafdMinutes.All(char.IsDigit))
            {
                var mins = Convert.ToInt16(iafdMinutes);
                var span = TimeSpan.FromMinutes(mins);
                Duration = $"{span.Hours} hrs {span.Minutes} mins";
            }
        }
    }

    private void SetDirectorsViaIAFD()
    {
        Directors = new();
        var iafdDirectors = RawIAFDResult.Directors;

        foreach(var iafdDirector in iafdDirectors)
        {
            if (!string.IsNullOrEmpty(iafdDirector))
            {
                if (iafdDirector != "No Data")
                {
                    var newDirector = new MovieDirector();

                    // Seperate actual name and credited name (if exist)
                    var parts = iafdDirector.Split("(as");
                    newDirector.Name = parts[0].Trim();
                    if (parts.Count() > 1)
                    {
                        newDirector.Credited = parts[1].Replace(")", "").Trim();
                    }

                    Directors.Add(newDirector);
                }
            }
        }
    }

    private void SetStudioViaIAFD()
    {
        Studio = null;
        var iafdStudio = RawIAFDResult.Studio;
        var iafdDistributor = RawIAFDResult.Distributor;

        if (!string.IsNullOrEmpty(iafdStudio))
        {
            Studio = iafdStudio;
        }
        else if (!string.IsNullOrEmpty(iafdDistributor))
        {
            Studio = iafdDistributor;
        }
    }

    private void SetAllGirlViaIAFD()
    {
        AllGirl = null;
        var iafdAllGirl = RawIAFDResult.AllGirl;

        if (!string.IsNullOrEmpty(iafdAllGirl))
        {
            AllGirl = iafdAllGirl;
        }
    }

    private void SetAllBoyViaIAFD()
    {
        AllBoy = null;
        var iafdAllBoy = RawIAFDResult.AllBoy;

        if (!string.IsNullOrEmpty(iafdAllBoy))
        {
            AllBoy = iafdAllBoy;
        }
    }

    private void SetCompilationViaIAFD()
    {
        Compilation = null;
        var iafdCompilation = RawIAFDResult.Compilation;

        if (!string.IsNullOrEmpty(iafdCompilation))
        {
            Compilation = iafdCompilation;
        }
    }

    private void SetSynopsisViaIAFD()
    {
        Synopsis = null;
        var iafdSynopsis = RawIAFDResult.Synopsis;

        if (iafdSynopsis.Count() > 0)
        {
            var cleanSynopsis = iafdSynopsis.Select(e => e.Replace("\n", "").Trim());
            Synopsis = string.Join("\n", cleanSynopsis);
        }
    }

    private void SetActorsViaIAFD()
    {
        Actors = new();
        var iafdActors = RawIAFDResult.Actors;

        foreach (var iafdActor in iafdActors)
        {
            var newActor = new MovieActor();

            newActor.Name = iafdActor.Name;
            newActor.ImagePath = iafdActor.ImageUrl;

            if (!string.IsNullOrEmpty(iafdActor.ProfileUrl))
            {
                newActor.ProfileUrl = iafdActor.ProfileUrl;
                newActor.Gender = iafdActor.ProfileUrl.Contains("gender=f") ? "Female" : "Male";
            }

            if (!string.IsNullOrEmpty(iafdActor.Credited))
            {
                newActor.Credited = iafdActor.Credited.Split(":")[1].Replace(")", "").Trim();
            }

            if (!string.IsNullOrEmpty(iafdActor.Acts))
            {
                var acts = iafdActor.Acts.Split(" ").Select(e => e).ToList();
                foreach (var act in acts)
                {
                    var newAct = new MovieAct();

                    if (Globals.IAFD_MOVIE_ACT_MAP.Keys.Contains(act.Trim()))
                    {
                        newAct.ShortName = act.Trim();
                        newAct.LongName = Globals.IAFD_MOVIE_ACT_MAP[act.Trim()];
                    }

                    newActor.Acts.Add(newAct);
                }

                newActor.Acts = newActor.Acts.OrderBy(e => e.ShortName).ToList();
            }

            Actors.Add(newActor);
        }

        Actors = Actors.OrderBy(e => e.Gender).ThenBy(e => e.Name).ToList();
    }

    private void SetActsViaIAFD()
    {
        Acts = new();
        var iafdActors = RawIAFDResult.Actors;

        foreach (var iafdActor in iafdActors)
        {
            if (!string.IsNullOrEmpty(iafdActor.Acts))
            {
                var acts = iafdActor.Acts.Split(" ").Select(e => e).ToList();
                foreach (var act in acts)
                {
                    var newAct = new MovieAct();

                    if (Globals.IAFD_MOVIE_ACT_MAP.Keys.Contains(act.Trim()))
                    {
                        newAct.ShortName = act.Trim();
                        newAct.LongName = Globals.IAFD_MOVIE_ACT_MAP[act.Trim()];
                    }

                    Acts.Add(newAct);
                }
            }
        }

        Acts = Acts.GroupBy(e => e.ShortName).Select(e => e.First()).ToList();
        Acts = Acts.OrderBy(e => e.ShortName).ToList();
    }

    private void SetScenesViaIAFD()
    {
        Scenes = new();
        var iafdScenes = RawIAFDResult.Scenes;

        foreach (var iafdScene in iafdScenes)
        {
            var newScene = new MovieScene();

            // TODO: some actors have a period in their name, us regex to get #. only
            var parts = iafdScene.Split(". ", 2);
            newScene.Number = parts[0].Replace("Scene ", "").Trim();

            var sceneActors = parts[1].Split(",");
            foreach (var sceneActor in sceneActors)
            {
                if (Actors.Count() > 0)
                {
                    var matchingActor = Actors.Where(e => e.Name == sceneActor.Trim());
                    if (matchingActor.FirstOrDefault() != null)
                    {
                        newScene.Actors.Add(matchingActor.First());
                    }
                }
            }

            // TODO: improve this by using M, F, MF, FF, MM, MMF, Etc...
            var unqActs = newScene.Actors.SelectMany(e => e.Acts)
                                         .GroupBy(e => e.ShortName)
                                         .Select(e => e.First())
                                         .OrderBy(e => e.ShortName)
                                         .ToList();

            var genders = newScene.Actors.OrderByDescending(e => e.Gender)
                                         .GroupBy(e => e.Gender)
                                         .Select(e => new { Gender = e.Key, Count = e.Count() })
                                         .ToDictionary(e => e.Gender, e => e.Count);

            var type = string.Empty;
            foreach (var gender in genders)
            {
                type += new string(gender.Key[0], gender.Value);
            }

            List<string> map = Globals.IAFD_MOVIE_ALL_ACT;
            switch (type)
            {
                case "M":
                    // TODO: map = Globals.IAFD_MOVIE_M_ACT;
                    break;
                case "F":
                    map = Globals.IAFD_MOVIE_F_ACT;
                    break;
                case "MM":
                    // TODO: map = Globals.IAFD_MOVIE_MM_ACT;
                    break;
                case "MF":
                    map = Globals.IAFD_MOVIE_MF_ACT;
                    break;
                case "FF":
                    map = Globals.IAFD_MOVIE_FF_ACT;
                    break;
                case "MMM":
                    // TODO: map = Globals.IAFD_MOVIE_MMM_ACT;
                    break;
                case "MMF":
                    map = Globals.IAFD_MOVIE_MMF_ACT;
                    break;
                case "MFF":
                    map = Globals.IAFD_MOVIE_MFF_ACT;
                    break;
                case "FFF":
                    map = Globals.IAFD_MOVIE_FFF_ACT;
                    break;
                // TODO: what about gang bangs?
            }

            newScene.Acts = unqActs.Where(e => map.Contains(e.ShortName)).ToList();

            Scenes.Add(newScene);
        }
    }



    public void UpdateViaIAFDResult(MovieResultIAFD result)
    {
        if (result != null)
        {
            // Set new raw IAFD
            RawIAFDResult = result;

            // Set name and year via IAFD
            SetNameViaIAFD();
            SetYearViaIAFD();

            // Set duration via IAFD
            SetDurationViaIAFD();

            // Set director(s) via IAFD
            SetDirectorsViaIAFD();

            // Set studio via IAFD
            SetStudioViaIAFD();

            // Set all girl and all boy via IAFD
            SetAllGirlViaIAFD();
            SetAllBoyViaIAFD();

            // Set Compilation via IAFD 
            SetCompilationViaIAFD();

            // Set synopsis via IAFD
            SetSynopsisViaIAFD();

            // TODO: set video extension
            // 

            // Set actors via IAFD
            SetActorsViaIAFD();

            // Set acts via IAFD
            SetActsViaIAFD();

            // Set scenes via IAFD
            SetScenesViaIAFD();  // TODO: this needs a bit of a clean up


            //foreach (var awardsItem in awardsItems)
            //{
            //    var award = new AwardResultIAFD();

            //    // Extract award organisation and year
            //    var awardEvent = awardsItem.PreviousSibling.InnerText.Trim();
            //    award.Organisation = awardEvent.Split(",")[0].Trim();
            //    award.Year = awardEvent.Split(",")[1].Trim();

            //    // Extract all awards for award event
            //    var awardEntries = awardsItem.ChildNodes;
            //    foreach (var awardEntry in awardEntries)
            //    {
            //        // TODO: this needs work and omits actors
            //        var awardEntryText = awardEntry.InnerText.Trim();
            //        award.Award.Add(awardEntryText.Split(",")[0].Trim());
            //    }

            //    result.Awards.Add(award);
            //}

            //// Extract all awards for award event
            //var awardEntries = awardsElem.ChildNodes;
            //foreach (var awardEntry in awardEntries)
            //{
            //    // TODO: this needs work and omits actors
            //    var awardEntryText = awardEntry.InnerText.Trim();
            //    newAward.Award.Add(awardEntryText.Split(",")[0].Trim());
            //}

            // TODO: shops


        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void NotifyPropertyChanged(string name)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}

public class MovieDirector
{
    private string _name;
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    private string _credited;
    public string Credited
    {
        get => _credited;
        set => _credited = value;
    }
}

public class MovieActor
{
    private string _name;
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    private string _imagePath;
    public string ImagePath
    {
        get => _imagePath;
        set => _imagePath = value;
    }

    private string _profileUrl;
    public string ProfileUrl
    {
        get => _profileUrl;
        set => _profileUrl = value;
    }

    private string _gender;
    public string Gender
    {
        get => _gender;
        set => _gender = value;
    }

    private string _credited;
    public string Credited
    {
        get => _credited;
        set => _credited = value;
    }

    private List<MovieAct> _acts = new();
    public List<MovieAct> Acts
    {
        get => _acts;
        set => _acts = value;
    }
}

public class MovieScene
{
    private string _number;
    public string Number
    {
        get => _number;
        set => _number = value;
    }

    private List<MovieActor> _actors = new();
    public List<MovieActor> Actors
    {
        get => _actors;
        set => _actors = value;
    }

    private List<MovieAct> _acts = new();
    public List<MovieAct> Acts
    {
        get => _acts;
        set => _acts = value;
    }
}

public class MovieAct
{
    private string _shortName;
    public string ShortName
    {
        get => _shortName;
        set => _shortName = value;
    }

    private string _longName;
    public string LongName
    {
        get => _longName;
        set => _longName = value;
    }
}