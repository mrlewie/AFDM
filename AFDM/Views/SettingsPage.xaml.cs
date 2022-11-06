using AFDM.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace AFDM.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }

    // TODO: see here https://stackoverflow.com/questions/72849795/when-a-bound-property-gets-a-input
}
