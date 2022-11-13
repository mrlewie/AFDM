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
using System.Reflection;

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

    private void RadioMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        var item = (RadioMenuFlyoutItem)sender;

        var type = item.Tag.ToString();
        var value = item.Text;
        if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(value))
        {
            ViewModel.OnMovieFilterClick(type, value);
        }
    }

    private void SizeMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        var item = (RadioMenuFlyoutItem)sender;
        
        var type = item.Tag.ToString();
        var value = item.Text;
        if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(value))
        {
            if (type == "ViewSize")
            {
                ViewModel.OnMovieCardSizeChanged(value);
            }
        }
    }
}
