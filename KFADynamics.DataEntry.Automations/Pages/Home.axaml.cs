using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Themes.KFADynamics.Controls;
using Avalonia.VisualTree;
using System.Linq;

namespace KFADynamics.DataEntry.Automations.Pages;

  public partial class Home : UserControl
  {
      public Home()
      {
          InitializeComponent();

          DataContext = this;
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
