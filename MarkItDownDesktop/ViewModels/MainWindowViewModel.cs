using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MarkItDownDesktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isCodeViewActive = false;

    [RelayCommand]
    private void SelectFileView() => IsCodeViewActive = false;

    [RelayCommand]
    private void SelectCodeView() => IsCodeViewActive = true;
}