using System.Linq;
using System.Reactive.Disposables;
using Avalonia.Dialogs;
using Avalonia.ReactiveUI;
using Avalonia.Themes.KFADynamics.Controls;
using Avalonia.VisualTree;
using KFADynamics.DataEntry.Automations.Pages.ViewModels;
using KFADynamics.DataEntry.Business;
using ReactiveUI;

namespace KFADynamics.DataEntry.Automations.Pages;

public partial class EntryHome : ReactiveUserControl<EntryHomeViewModel>
{
  public EntryHome()
  {
    InitializeComponent();
    DataContext = new EntryHomeViewModel();
    this.WhenActivated(PageActivated);
  }

  private void PageActivated(CompositeDisposable disposables)
  {
    DataContext ??= new EntryHomeViewModel();
    if (DataContext is EntryHomeViewModel vm)
    {
      vm.PageActivated(disposables);
      Service.ProcessingData = vm;
    }
  }

  public void OpenLeftDrawer()
  {
    var ancestors = this.GetVisualAncestors();
    if (ancestors != null)
    {
      var navDrawer = ancestors.SingleOrDefault(p => p.GetType() == typeof(NavigationDrawer));
      if (navDrawer != null)
      {
        ((NavigationDrawer)navDrawer).LeftDrawerOpened = true;
      }
    }
  }

  public void OpenProjectRepoLink() => GlobalCommand.OpenProjectRepoLink();

  public void OpenAvaloniaWebsiteLink() => GlobalCommand.OpenAvaloniaWebsiteLink();

  public void ShowAboutAvaloniaUI() => new AboutAvaloniaDialog().ShowDialog(Program.MainWindow);
}
