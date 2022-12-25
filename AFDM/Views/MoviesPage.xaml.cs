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

    public bool _wasFixMatchSearchButtonClicked;
    public Movie _movieOfCurrentDialog;
    private async void ShowTermsOfUseContentDialogButton_Click(object sender, RoutedEventArgs e)
    {
        var source = (FrameworkElement)sender;
        _movieOfCurrentDialog = (Movie)source.DataContext;  // todo - i cant really do this stuff in viewmodel due to dialog...

        ContentDialogResult result = await termsOfUseContentDialog.ShowAsync();
    }

    private void termsOfUseContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        _wasFixMatchSearchButtonClicked = true;

        // TODO: disable primary button on click

        ViewModel.OnUserMovieQuery(); // may need to turn this into await and func a task return
    }

    private void termsOfUseContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        _wasFixMatchSearchButtonClicked = false;
    }

    private void TermsOfUseContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        // Ensure that the check box is unchecked each time the dialog opens.
        //ConfirmAgeCheckBox.IsChecked = false;
    }

    private void TermsOfUseContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        if (_wasFixMatchSearchButtonClicked)
        {
            args.Cancel = true;
        }
    }

    private void ListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (_movieOfCurrentDialog != null && e.ClickedItem != null)
        {
            _wasFixMatchSearchButtonClicked = false;

            // TODO: a bit sloppy
            var sourceListView = (FrameworkElement)sender;
            var sourceStackPanel = (FrameworkElement)sourceListView.Parent;
            var contentDialog = (ContentDialog)sourceStackPanel.Parent;
            contentDialog.Hide();

            var clickedItem = (SearchResultIAFD)e.ClickedItem;
            ViewModel.OnFixMatchItemClick(_movieOfCurrentDialog, clickedItem);
        }
    }







    //private void ConfirmAgeCheckBox_Checked(object sender, RoutedEventArgs e)
    //{
    //    termsOfUseContentDialog.IsPrimaryButtonEnabled = true;
    //}

    //private void ConfirmAgeCheckBox_Unchecked(object sender, RoutedEventArgs e)
    //{
    //    termsOfUseContentDialog.IsPrimaryButtonEnabled = false;
    //}
}
