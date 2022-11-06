using AFDM.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace AFDM.Views;

public sealed partial class ActorsPage : Page
{
    public ActorsViewModel ViewModel
    {
        get;
    }

    public ActorsPage()
    {
        ViewModel = App.GetService<ActorsViewModel>();
        InitializeComponent();
    }
}
