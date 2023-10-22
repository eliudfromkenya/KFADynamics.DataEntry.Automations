using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using KFADynamics.DataEntry.Playwright;

namespace KFADynamics.DataEntry.Automations.Pages;

public delegate void CloseAction(object obj, RoutedEventArgs e);

public partial class ClosableDialogPage : Window, IClosablePage
{
  public CloseAction CloseAction { get; set; }
  AnAction IClosablePage.Close { get; set; }

  public ClosableDialogPage()
  {
    //var sa = DialogContent;
    AvaloniaXamlLoader.Load(this);
    Width = 1000;
    FontSize = 24;
  }

  private void Close_Clicked(object obj, RoutedEventArgs e)
  {
    try
    {
    }
    catch (Exception ex)
    {
      DialogsManager.ErrorHandler(ex.Message, "Error closing page", ex);
    }
  }
}
