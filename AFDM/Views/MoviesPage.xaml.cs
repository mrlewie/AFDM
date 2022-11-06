using System.Numerics;
using AFDM.Contracts.Services;
using AFDM.Core.Contracts.Services;
using AFDM.Core.Models;
using AFDM.Models;
using AFDM.Services;
using AFDM.ViewModels;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel;
using Windows.Management.Core;
using Windows.Storage;
using Windows.UI.Core;
using AFDM.Core.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace AFDM.Views;

public sealed partial class MoviesPage : Page
{
    public MoviesViewModel ViewModel
    {
        get;
    }

    public MoviesPage()
    {
        ViewModel = App.GetService<MoviesViewModel>();
        InitializeComponent();
        this.NavigationCacheMode = NavigationCacheMode.Required;
        this.DataContext = ViewModel;
    }

    //private void MoviesCardSizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
    //{
    //    //var slider = sender as Slider;
    //    //var value = (double)slider.Value;
    //    //ViewModel.UpdateCardSize(value);
    //}

    private void ChangeColorItem_Click(object sender, RoutedEventArgs e)
    {

    }
}
