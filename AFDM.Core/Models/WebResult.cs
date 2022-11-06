using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFDM.Core.Models;

public class SearchResultIAFD
{
    public string Name { set; get; }

    public string Year { get; set; }

    public string Url { get; set; }

    public double NameMatch { get; set; }

    public int YearMatch { get; set; }
}

public class MovieResultIAFD
{
    public string Name { set; get; }

    public string Year {  get; set; }

    public string Minutes { get; set; }

    public List<string> Directors { get; set; } = new();

    public string Distributor { get; set; }

    public string Studio { get; set; }

    public string AllGirl { get; set; }

    public string AllBoy { get; set; }

    public string Compilation { get; set; }

    public string ReleaseDate { get; set; }

    public string DateAdded { get; set; }

    public List<ActorResultIAFD> Actors { get; set; } = new();

    public List<string> Scenes { get; set; } = new();

    public List<string> Synopsis { get; set; } = new();

    public List<AwardResultIAFD> Awards { get; set; } = new();

    public List<ShopResultIAFD> Shops { get; set; } = new();
}

public class ActorResultIAFD
{ 
    public string Name { get; set; }

    public string ImageUrl { get; set; }

    public string ProfileUrl { get; set; }

    public string Credited { get; set; }

    public string Acts { get; set; }
}

public class AwardResultIAFD
{
    public string Event { get; set; }

    public List<string> Award { get; set; } = new();
}

public class ShopResultIAFD
{
    public string Name { get; set; }

    public string Url { get; set; }
}