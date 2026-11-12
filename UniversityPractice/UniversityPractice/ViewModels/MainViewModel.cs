using CommunityToolkit.Mvvm.ComponentModel;

namespace UniversityPractice.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Welcome to Avalonia!";
}
