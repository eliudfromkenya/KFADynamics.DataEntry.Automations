using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using KFADynamics.DataEntry.Automations.Pages;
using KFADynamics.DataEntry.Automations.Windows;
using KFADynamics.DataEntry.Business;
using KFADynamics.DataEntry.Business.Delegates;

namespace KFADynamics.DataEntry.Automations;

public static class DialogsManager
{
  public static ErrorHandler ErrorHandler { get; set; }
  public static DialogHostAvalonia.DialogHost DialogHost { get; set; }
  public static async Task<string> OpenFile(string defaultPath = "")
  {
    var dialog = new OpenFolderDialog();
    return await dialog.ShowAsync(HomePage.Page);
  }


  public static async Task<string> InputText(string question, string title, string defaultValue = null)
  {
    using BehaviorSubject<string> obs = new(null);
    void GetMessage()
    {
      try
      {
        Dispatcher.UIThread.Invoke(async () =>
         {
           try
           {
             await PromptBox.Prompt(obs, question, title, defaultValue);
           }
           catch (Exception)
           {
             obs.OnNext(null);
             obs.OnCompleted();
           }
         });
      }
      catch (Exception ex)
      {
        ErrorHandler(ex.Message, "Input Dialog Error", ex);
      }
    }
    Functions.RunOnBackground(GetMessage, ErrorHandler);
    var ff = Task.Run(async () => await obs);
    return await ff;
  }

  public static async Task<PromptBoxButtons> ShowMessage(string text, string title, string details = null, PromptBoxButtons type = PromptBoxButtons.Ok | PromptBoxButtons.Cancel, PromptBoxButtons defaultResult = PromptBoxButtons.No)
  {
    using BehaviorSubject<PromptBoxButtons> obs = new(PromptBoxButtons.None);
    void GetMessage()
    {
      Dispatcher.UIThread.Invoke(async () =>
          {
            try
            {
              await PromptBox.Show(obs, text, title, details, type, defaultResult);
              obs.OnCompleted();
            }
            catch (Exception)
            {
              obs.OnNext(PromptBoxButtons.None);
              obs.OnCompleted();
            }
          });
    }

    Functions.RunOnBackground(GetMessage, ErrorHandler);
    var ff = Task.Run(async () => await obs);
    return await ff;
  }
}
