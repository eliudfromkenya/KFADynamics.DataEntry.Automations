using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Avalonia.Themes.KFADynamics.Controls;
using Avalonia.Threading;
using KFADynamics.DataEntry.Automations.Interfaces;
using KFADynamics.DataEntry.Automations.Models;
using KFADynamics.DataEntry.Automations.Windows.ViewModels;
using KFADynamics.DataEntry.Business;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace KFADynamics.DataEntry.Automations.Windows;

internal sealed partial class HomePage : ReactiveWindow<HomePageViewModel<HomePage>>, IMainWindow
{
  private readonly Application _app = App.Current;
  private WindowNotificationManager? _manager;
  static HomePage _homePage = null;

  #region Control fields

  private ToggleButton NavDrawerSwitch => this.GetControl<ToggleButton>("NavDrawerSwitch2");
  private ListBox DrawerList => this.GetControl<ListBox>("DrawerList2");
  private Carousel PageCarousel => this.GetControl<Carousel>("PageCarousel2");
  private ScrollViewer mainScroller => this.GetControl<ScrollViewer>("mainScroller2");

  #endregion Control fields

  public HomePage() : this(default)
  {
  }

  public HomePage(IMainWindow window)
  {
    InitializeComponent(window);
    this.WhenActivated(PageActivated);
  }

  IThemeSwitch IMainWindow.ThemeSwitch => (IThemeSwitch)this._app!;
  IMainWindowState IMainWindow.Model => (IMainWindowState)this.DataContext;
  PixelPoint IMainWindow.Position => Utilities.GetWindowPosition(this);
  Size IMainWindow.ClientSize => this.ClientSize;
  Size? IMainWindow.FrameSize => this.FrameSize;
  WindowState IMainWindow.State => this.WindowState;

  internal static HomePage Page { get => _homePage; }

  private void InitializeComponent(IMainWindow window)
  {
    AvaloniaXamlLoader.Load(this);

    var vm = new HomePageViewModel<HomePage>(this);
    vm.PropertyChanged += (s, e) =>
    {
      if (e.PropertyName == "DarkThemeEnabled")
      {
        if (vm.DarkThemeEnabled)
        {
          Application.Current.SetValue(ThemeVariantScope.ActualThemeVariantProperty, ThemeVariant.Dark);
        }
        else
        {
          Application.Current.SetValue(ThemeVariantScope.ActualThemeVariantProperty, ThemeVariant.Light);
        }
      }
    };

    DataContext = vm;

    if (window is not null)
    {
      this.WindowStartupLocation = WindowStartupLocation.Manual;
      this.PageCarousel.SelectedIndex = window.Model != null ? window.Model.CurrentPageIndex : 0;
      this.WindowState = window.State;
      this.Position = window.Position;
      this.FrameSize = window.FrameSize;
      this.ClientSize = window.ClientSize;
    }

    #region Control getter and event binding

    DrawerList.PointerReleased += DrawerSelectionChanged;
    DrawerList.KeyUp += DrawerList_KeyUp;

    #endregion Control getter and event binding
  }

  private void TemplatedControl_OnTemplateApplied(object sender, TemplateAppliedEventArgs e)
  {
    SnackbarHost.Post("Welcome to\r\nKFA Dynamics Data Entry!");
  }

  private void HelloButtonMenuItem_OnClick(object sender, RoutedEventArgs e)
  {
    SnackbarHost.Post("Hello user !");
  }

  private void GoodbyeButtonMenuItem_OnClick(object sender, RoutedEventArgs e)
  {
    if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      desktop.Shutdown(0);
    }
  }

  public static void ShowMessage(IUserMessage message) => Page?.ShowMessageHandler(message);
  private void DrawerList_KeyUp(object sender, KeyEventArgs e)
  {
    if (e.Key == Key.Space || e.Key == Key.Enter)
    {
      DrawerSelectionChanged(sender, null);
    }
  }

  public void DrawerSelectionChanged(object sender, RoutedEventArgs args)
  {
    var listBox = sender as ListBox;
    if (!listBox.IsFocused && !listBox.IsKeyboardFocusWithin)
      return;
    try
    {
      PageCarousel.SelectedIndex = listBox.SelectedIndex;
      mainScroller.Offset = Vector.Zero;
      mainScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
      ((IMainWindowState)this.DataContext).CurrentPageIndex = listBox.SelectedIndex;
    }
    catch
    {
    }

    NavDrawerSwitch.IsChecked = false;
  }
  void PageActivated(CompositeDisposable disposables)
  {
    var topLevel = GetTopLevel(this);
    _manager = new WindowNotificationManager(topLevel) { MaxItems = 5, Position = NotificationPosition.BottomRight, Opacity = 0.8, };
    _homePage = this;
    DialogsManager.DialogHost = this.FindControl<global::DialogHostAvalonia.DialogHost>("MainDialogHost");
    DialogsManager.ErrorHandler = (message, title, ex) =>
    {
      ShowMessage(new UserMessage
      {
        Message = message,
        MessageTitle = title,
        MessageType = MessageType.Error,
        Time = DateTime.Now,
        MessageDetails = "Dialog message error"
      });
    };

    var pgTitle = this.FindControl<TextBlock>("TxbPageTitle");
    ApplicationModelBase.PageTitle
    .Subscribe(x =>
    {
      Dispatcher.UIThread.Invoke(() =>
      {        
        pgTitle.Text = x;
        pgTitle.InvalidateVisual();
      });
    })
    .DisposeWith(disposables);
  }

  private void ShowMessageHandler(IUserMessage message)
  {
    try
    {
      NotificationType notificationType = message.MessageType switch
      {
        Business.MessageType.Normal => NotificationType.Information,
        Business.MessageType.Error => NotificationType.Error,
        Business.MessageType.Warning => NotificationType.Warning,
        Business.MessageType.Success => NotificationType.Success,
        _ => NotificationType.Information
      };

      Dispatcher.UIThread.Invoke(() =>
      {
        try
        {
           _manager?.Show(new Notification(message.MessageTitle, message.Message, notificationType, TimeSpan.FromSeconds(30), null, null));
        }
        catch { }
      });
    }
    catch { }
  }
}
