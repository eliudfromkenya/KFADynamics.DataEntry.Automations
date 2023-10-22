using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using KFADynamics.DataEntry.Automations.Pages;
using KFADynamics.DataEntry.Automations.Windows;
using KFADynamics.DataEntry.Playwright;

namespace KFADynamics.DataEntry.Automations;

  internal static class Utilities
  {
      public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
      public static readonly bool IsOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

      public static Bitmap GetImageFromResources(String fileName)
      {
          return new Bitmap(AssetLoader.Open(new Uri($"avares://KFADynamics.DataEntry.Automations/Assets/{fileName}")));
      }

      public static PixelPoint GetWindowPosition(Window window)
      {
          if (!IsOSX || !window.FrameSize.HasValue)
              return window.Position;
          else
          {
              Int32 yOffset = (Int32)(window.FrameSize.Value.Height - window.ClientSize.Height);
              return new(window.Position.X, window.Position.Y + yOffset);
          }
      }

  public static async Task NewPageContentDialog(this Control ctrl, CloseAction? closeAction, string closeText)
  {
    try
    {
      var host = DialogsManager.DialogHost;
      if (host == null)
      {
        await new Window
        {
          Content = ctrl,
          SizeToContent = SizeToContent.WidthAndHeight,
        }.ShowAsDialog();
        return;
      }

      var page = new ClosableDialogPage() { Width = ctrl.Width, Height = ctrl.Height };
      page.BtnClose.Content = closeText;
      page.DialogContent.Content = ctrl;

      if (ctrl is IClosablePage closable)
        closable.Close = tt => host.IsOpen = false;

      host.DialogContent = ctrl;
      host.IsOpen = true;
    }
    catch (Exception ex)
    {
      DialogsManager.ErrorHandler(ex.Message, "Error opening dialog", ex);
    }
  }


  public static async Task NewPageContentDialog(this IClosablePage ctrl)
  {
    try
    {
      var host = DialogsManager.DialogHost;
      if (host == null)
      {
        await new Window
        {
          Content = ctrl,
          SizeToContent = SizeToContent.WidthAndHeight,
        }.ShowAsDialog();
        return;
      }

      if (ctrl is IClosablePage closable)
        closable.Close = tt => host.IsOpen = false;
      else if (ctrl is Control ctr)
      {
        var page = new ClosableDialogPage
        {
          Width = ctr.Width,
          Height = ctr.Height
        };
        page.DialogContent.Content = ctr;
      }

      host.DialogContent = ctrl;
      host.IsOpen = true;
    }
    catch (Exception ex)
    {
      DialogsManager.ErrorHandler(ex.Message, "Error opening dialog", ex);
    }
  }

  static readonly Stack<Window> CurrentLoadedDialogWindows = new();
  public static async Task ShowAsDialog(this Control control, Window parent = null)
  {
    try
    {
      if (!Dispatcher.UIThread.CheckAccess())
      {
        await Dispatcher.UIThread.Invoke(async () => await ShowAsDialog(control, parent));
        return;
      }

      var mainWindow = HomePage.Page;

      if (control is Window page)
      {
        if (!CurrentLoadedDialogWindows.Any())
          CurrentLoadedDialogWindows.Push(mainWindow);

        Dispatcher.UIThread.Invoke(() => page.Focus());
        if (mainWindow != null)
        {
          page.ShowInTaskbar = false;
          page.Topmost = false;
          page.WindowStartupLocation = WindowStartupLocation.CenterOwner;
          void Page_Closed(object sender, EventArgs e)
          {
            Dispatcher.UIThread.Invoke(() =>
            {
              try
              {
                if (mainWindow?.Content is Visual ctrl2)
                  ctrl2.Opacity = 1;

                var initialPage = CurrentLoadedDialogWindows.Pop();

                if (sender != initialPage)
                  return;

                var current = CurrentLoadedDialogWindows.Peek();
                if (current != mainWindow)
                  current.Closed += Page_Closed;
                if (current?.Content is Visual ctrl)
                  ctrl.Opacity = 1;

                initialPage.Closed -= Page_Closed;
              }
              catch
              {
                if (mainWindow?.Content is Visual ctrl)
                  ctrl.Opacity = 1;
              }
            });
          }
          foreach (var item in CurrentLoadedDialogWindows)
          {
            try
            {
              if (item?.Content is Visual ctrl)
                ctrl.Opacity = 0.002;
              page.Closed -= Page_Closed;
            }
            catch { }
          }
          page.Closed += Page_Closed;
          page.Focus();
          CurrentLoadedDialogWindows.Push(page);
          await page.ShowDialog(mainWindow);
        }
        else
        {
          page.Topmost = true;
          page.Focus();
          page.Show();
        }
      }
      else
      {
        Dispatcher.UIThread.Invoke(() => control.NewPageContentDialog(null, "Close"));
      }
    }
    catch (Exception ex)
    {
      DialogsManager.ErrorHandler(ex.Message, "Error", ex);
    }
  }



  public static Bitmap GetImageFromFile(String path)
      {
          try
          {
              return new Bitmap(GetImageFullPath(path));
          }
          catch (Exception)
          {
              return GetImageFromResources("broken-link.png");
          }
      }

      private static String GetImageFullPath(String fileName)
          => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
  }
