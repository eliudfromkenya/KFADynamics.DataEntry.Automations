using Avalonia.Controls;
using Avalonia.Themes.KFADynamics.Enums;
using global::KFADynamics.DataEntry.Automations.Interfaces;
using System;
using System.Reactive.Subjects;

namespace KFADynamics.DataEntry.Automations.Windows.ViewModels;

internal sealed class HomePageViewModel<TWindow> : ApplicationModelBase
    where TWindow : Window, IMainWindow
{
  private readonly TWindow _window;

  public BehaviorSubject<double> MiniProgress { get; } = new(67.0);

  public HomePageViewModel(TWindow window)
      : base(window.ThemeSwitch)
  {
    _window = window;
  }

  public override void HelpAboutMethod() => base.RunHelpAbout(_window);

  public override void FileExitCommand()
  {
    Environment.Exit(0);
  }

  public override void SwitchThemeCommand(bool dark)
  {
    if (dark)
    {
      base.SetTheme(ApplicationTheme.Dark);
    }
    else
    {
      base.SetTheme(ApplicationTheme.Light);
    }
  }


}

