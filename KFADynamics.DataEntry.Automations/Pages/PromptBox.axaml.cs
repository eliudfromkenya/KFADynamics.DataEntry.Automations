using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using KFADynamics.DataEntry.Automations.ViewModels;
using KFADynamics.DataEntry.Playwright;

namespace KFADynamics.DataEntry.Automations.Pages;

[Flags]
public enum PromptBoxButtons
{
  None = 0,
  Ok = 1,
  Cancel = 2,
  Yes = 4,
  No = 8, 
  Continue = 16,
  Open = 32
}

public partial class PromptBox : ReactiveUserControl<ViewModelBase>, IClosablePage
{
  public AnAction Close { get; set; }

  public PromptBox()
  {
    AvaloniaXamlLoader.Load(this);
    Width = 900;
    FontSize = 24;
  }

  public event EventHandler Closed;

  public static async Task<PromptBoxButtons?> Show(BehaviorSubject<PromptBoxButtons> obs, string question, string title, string details, PromptBoxButtons promptInput = PromptBoxButtons.Yes | PromptBoxButtons.Cancel, PromptBoxButtons defaultValue = PromptBoxButtons.Cancel | PromptBoxButtons.No)
  {
    try
    {
      var res = defaultValue;

      var dialogHost = DialogsManager.DialogHost;
      var PromptBox = new PromptBox()
      {
        Close = tt =>
          {
            try
            {
              dialogHost.IsOpen = false;
              obs.OnNext(res);
              obs.OnCompleted();
            }
            catch (Exception ex)
            {
              DialogsManager.ErrorHandler(ex.Message, "Error", ex);
            }
          }
      };
      PromptBox.FindControl<TextBlock>("Text").Text = question;
      PromptBox.FindControl<TextBlock>("Title").Text = title;
      PromptBox.FindControl<TextBlock>("Details").Text = details;
      //PromptBox.FindControl<TextBox>("TxtAnswer").Text = defaultValue;
      var buttonPanel = PromptBox.FindControl<StackPanel>("Buttons");

      void AddButton(string caption, PromptBoxButtons r, bool def = false)
      {
        var btn = new Button { Content = caption };
        btn.Click += (_, __) =>
        {
          try
          {
            res = r;
            obs.OnNext(res);
            obs.OnCompleted();
            PromptBox.Close();
          }
          catch (Exception ex)
          {
            DialogsManager.ErrorHandler(ex.Message, "Error", ex);
          }
        };
        buttonPanel?.Children.Add(btn);
        if (def)
        {
          btn.IsDefault = true;
          res = r;
          Dispatcher.UIThread.Invoke(() => btn?.Focus());
        }
      }
      if (promptInput.HasFlag(PromptBoxButtons.Open))
        AddButton("Open", PromptBoxButtons.Open, PromptBoxButtons.Open == defaultValue);

      if (promptInput.HasFlag(PromptBoxButtons.Continue))
        AddButton("Continue", PromptBoxButtons.Continue, PromptBoxButtons.Continue == defaultValue);

      if (promptInput.HasFlag(PromptBoxButtons.Ok))
        AddButton("Ok", PromptBoxButtons.Ok, PromptBoxButtons.Ok == defaultValue);

      if (promptInput.HasFlag(PromptBoxButtons.Yes))
        AddButton("Yes", PromptBoxButtons.Yes, PromptBoxButtons.Yes == defaultValue);

      if (promptInput.HasFlag(PromptBoxButtons.No))
        AddButton("No", PromptBoxButtons.No, PromptBoxButtons.No == defaultValue);

      if (promptInput.HasFlag(PromptBoxButtons.Cancel))
        AddButton("Cancel", PromptBoxButtons.Cancel, PromptBoxButtons.Cancel == defaultValue);

      PromptBox.Focus();
      var txtBox = PromptBox.FindControl<TextBox>("TxtAnswer");
      txtBox.TextChanged += (rr, yy) => txtBox.Text = txtBox.Text.ToUpper();
      txtBox.Focus();

      var tcs = new TaskCompletionSource<PromptBoxButtons>();
      PromptBox.Closed += delegate { tcs.TrySetResult(res); };

      await PromptBox.NewPageContentDialog();
      return await tcs.Task;
    }
    catch (System.Exception ex)
    {
      DialogsManager.ErrorHandler(ex.Message, "Prompt Box Error", ex);
      obs.OnNext(PromptBoxButtons.None);
      obs.OnCompleted();
      return null;
    }
  }

  public static async Task<string> Prompt(BehaviorSubject<string?> obs, string question, string title, string? defaultValue = null)
  {
    try
    {
      var dialogHost = DialogsManager.DialogHost;
      var PromptBox = new PromptBox()
      {
        //Title = title,
        //Topmost = true
        Close = tt => dialogHost.IsOpen = false
      };
      PromptBox.FindControl<TextBlock>("Text").Text = question;
      PromptBox.FindControl<TextBlock>("Title").Text = title;
      PromptBox.FindControl<TextBox>("TxtAnswer").Text = defaultValue;
      var buttonPanel = PromptBox.FindControl<StackPanel>("Buttons");

      var res = PromptBoxButtons.Ok;
      var txtBox = PromptBox.FindControl<TextBox>("TxtAnswer");
      txtBox.IsVisible = true;
      void AddButton(string caption, PromptBoxButtons r, bool def = false)
      {
        var btn = new Button { Content = caption };
        btn.Click += (_, __) =>
        {
          try
          {
            res = r;
            obs.OnNext(txtBox?.Text);
            obs.OnCompleted();
            PromptBox.Close();
          }
          catch (Exception ex)
          {
            DialogsManager.ErrorHandler(ex.Message, "Error", ex);
          }
        };
        buttonPanel.Children.Add(btn);
        if (def)
        {
          btn.IsDefault = true;
          res = r;
          Dispatcher.UIThread.Invoke(() => btn?.Focus());
        }
      }

      AddButton("Ok", PromptBoxButtons.Ok, true);
      AddButton("Cancel", PromptBoxButtons.Cancel);

      PromptBox.Focus();
      txtBox.Focus();

      var tcs = new TaskCompletionSource<string>();
      PromptBox.Closed += delegate { tcs.TrySetResult(res == PromptBoxButtons.Cancel ? null : PromptBox.FindControl<TextBox>("TxtAnswer").Text); };

      await PromptBox.NewPageContentDialog();
      return await tcs.Task;
    }
    catch (System.Exception ex)
    {
      DialogsManager.ErrorHandler(ex.Message, "Prompt Box Error", ex);
      return null;
    }
  }
}
