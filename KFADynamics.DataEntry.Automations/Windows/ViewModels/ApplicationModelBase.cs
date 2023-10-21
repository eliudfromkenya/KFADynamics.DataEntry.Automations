using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Themes.KFADynamics.Enums;
using KFADynamics.DataEntry.Automations.Interfaces;
using KFADynamics.DataEntry.Automations.ViewModels;

namespace KFADynamics.DataEntry.Automations.Windows.ViewModels;

internal abstract class ApplicationModelBase : ViewModelBase, IMainWindowState
{
  private readonly IThemeSwitch _themeSwitch;

  private bool _aboutEnable;

  public bool AboutEnabled
  {
    get { return _aboutEnable; }
    set
    {
      _aboutEnable = value;
      OnPropertyChanged(nameof(AboutEnabled));
    }
  }

  private bool _darkThemeEnabled = true;

  public bool DarkThemeEnabled
  {
    get { return _darkThemeEnabled; }
    set
    {
      _darkThemeEnabled = value;
      OnPropertyChanged(nameof(DarkThemeEnabled));
    }
  }
  public static BehaviorSubject<string> PageTitle { get; } = new BehaviorSubject<string>("Please log in to use the system");

  private int _selectedPageIndex = 0;

  public int CurrentPageIndex
  {
    get { return _selectedPageIndex; }
    set
    {
      _selectedPageIndex = value;
      OnPropertyChanged(nameof(CurrentPageIndex));
    }
  }

  public ApplicationModelBase(IThemeSwitch themeSwitch)
  {
    AboutEnabled = true;

    ThemeVariant theme = Application.Current.GetValue(ThemeVariantScope.ActualThemeVariantProperty);

    _themeSwitch = themeSwitch;

    ApplicationTheme appTheme = theme == ThemeVariant.Dark ? ApplicationTheme.Dark : ApplicationTheme.Light;
    IntializeTheme(appTheme);
  }

  public abstract void HelpAboutMethod();

  public abstract void SwitchThemeCommand(bool dark);

  public abstract void FileExitCommand();

  protected async void RunHelpAbout(Window currentWindow)
  {
    if (AboutEnabled)
    {
      try
      {
        AboutEnabled = false;
        await new AboutWindow(IsDarkTheme(_themeSwitch.Current)).ShowDialog(currentWindow);
      }
      finally
      {
        AboutEnabled = true;
      }
    }
  }

  protected void SetTheme(ApplicationTheme theme)
  {
    IntializeTheme(theme);
    _themeSwitch.ChangeTheme(theme);
  }

  private void IntializeTheme(ApplicationTheme theme)
  {
    DarkThemeEnabled = (theme == ApplicationTheme.Dark);
  }

  private static bool IsDarkTheme(ApplicationTheme? theme)
      => theme switch
      {
        ApplicationTheme.Dark => true,
        _ => false,
      };
}
