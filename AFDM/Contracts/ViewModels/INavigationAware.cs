using System.ComponentModel;

namespace AFDM.Contracts.ViewModels;

public interface INavigationAware
{
    event PropertyChangedEventHandler PropertyChanged;

    void OnNavigatedTo(object parameter);

    void OnNavigatedFrom();
}
