using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Threading;
using KFA.ItemCodes.Classes;
using KFADynamics.DataEntry.Automations.Converters;
using KFADynamics.DataEntry.Automations.Models;
using KFADynamics.DataEntry.Automations.Windows;
using KFADynamics.DataEntry.Automations.Windows.ViewModels;
using KFADynamics.DataEntry.Business;
using KFADynamics.DataEntry.Business.Classes;
using KFADynamics.DataEntry.Playwright.Models;
using Material.Icons;
using ReactiveUI;
using MessageType = KFADynamics.DataEntry.Business.MessageType;

namespace KFADynamics.DataEntry.Automations.Pages.ViewModels;

public class EntryHomeViewModel : ReactiveObject, IProcessingData
{
  private string _months = "";
  private string _branchCodes = "";
  private string _batchNumbers = "";
  private string _documentNumbers = "";
  private int _numberOfPagesToEmploy = 5;
  private bool _changeUserBranchCodeToMatchDocumentsBranch = true;
  private double _mainProgress = 0;
  private double _progress = 0;
  private double _miniProgress = 0;
  private DocumentType _documentType = DocumentType.CountSheets;
  private MaterialIconKind _miniProgressKind = MaterialIconKind.TrackChanges;
  private string _subBackgroundImage = "KFADynamics.DataEntry.Automations.Assets.Banner.png";
  private bool _hasDocuments;
  private bool _generateAfterProcessReport = true;
  private bool _postRecordsAfterProcessing = true;
  private IUserMessage _userMessage;

  public bool IsBusy { get => _isBusy; set => this.RaiseAndSetIfChanged(ref _isBusy, value); }
  public IUserMessage UserMessage { get => _userMessage; set => this.RaiseAndSetIfChanged(ref _userMessage, value); }
  public bool HasDocuments { get => _hasDocuments; set => this.RaiseAndSetIfChanged(ref _hasDocuments, value); }
  public bool HasMonths { get => _hasMonths; set => this.RaiseAndSetIfChanged(ref _hasMonths, value); }
  public bool HasDate { get => _hasDate; set => this.RaiseAndSetIfChanged(ref _hasDate, value); }
  public DateTime Date { get => _date; set => this.RaiseAndSetIfChanged(ref _date, value); }
  public bool GenerateAfterProcessReport { get => _generateAfterProcessReport; set => this.RaiseAndSetIfChanged(ref _generateAfterProcessReport, value); }
  public bool PostRecordsAfterProcessing { get => _postRecordsAfterProcessing; set => this.RaiseAndSetIfChanged(ref _postRecordsAfterProcessing, value); }
  public string SubBackgroundImage { get => _subBackgroundImage; set => _subBackgroundImage = value; }
  public string Months { get => _months; set => this.RaiseAndSetIfChanged(ref _months, value); }
  public MaterialIconKind MiniProgressKind { get => _miniProgressKind; set => this.RaiseAndSetIfChanged(ref _miniProgressKind, value); }
  public string BranchCodes { get => _branchCodes; set => this.RaiseAndSetIfChanged(ref _branchCodes, value); }
  public string BatchNumbers { get => _batchNumbers; set => this.RaiseAndSetIfChanged(ref _batchNumbers, value); }
  public string DocumentNumbers { get => _documentNumbers; set => this.RaiseAndSetIfChanged(ref _documentNumbers, value); }
  public int NumberOfPagesToEmploy { get => _numberOfPagesToEmploy; set => this.RaiseAndSetIfChanged(ref _numberOfPagesToEmploy, value); }
  public bool ChangeUserBranchCodeToMatchDocumentsBranch { get => _changeUserBranchCodeToMatchDocumentsBranch; set => this.RaiseAndSetIfChanged(ref _changeUserBranchCodeToMatchDocumentsBranch, value); }
  public double MainProgress { get => _mainProgress; set => this.RaiseAndSetIfChanged(ref _mainProgress, value); }
  public double Progress { get => _progress; set => this.RaiseAndSetIfChanged(ref _progress, value); }
  public double MiniProgress { get => _miniProgress; set => this.RaiseAndSetIfChanged(ref _miniProgress, value); }
  public DocumentType DocumentType { get => _documentType; set => this.RaiseAndSetIfChanged(ref _documentType, value); }
  public ICommand ProcessCommand { get; }
  public ICommand CancelCommand { get; }
  public ICommand HarmonizeCommand { get; }
  public ICommand ProcessedCommand { get; }
  public ICommand PendingCommand { get; }
  public ICommand CloseCommand { get; }

  public BehaviorSubject<bool> CanCancel { get; } = new(false);

  public EntryHomeViewModel()
  {
    LoadUserSecretes();
    CancelCommand = ReactiveCommand.CreateFromTask(CancelClicked, CanCancel);
    HarmonizeCommand = ReactiveCommand.CreateFromTask(HarmonizeClicked, CanHarmonize);
    ProcessCommand = ReactiveCommand.CreateFromTask(ProcessClicked, CanProcess);
    ProcessedCommand = ReactiveCommand.CreateFromTask(ProcessedClicked, CanProcessed);
    PendingCommand = ReactiveCommand.CreateFromTask(PendingClicked, CanPending);
    CloseCommand = ReactiveCommand.CreateFromTask(CloseClicked, CanClose);
  }

  private void LoadUserSecretes()
  {
    Functions.RunOnBackground(() =>
    {
      try
      {
        var url = SecretAppsettingReader.ReadConfig("Dynamics:Url");
        var username = SecretAppsettingReader.ReadConfig("Dynamics:Username");
        var password = SecretAppsettingReader.ReadConfig("Dynamics:Password");
        var dbConnectionString = SecretAppsettingReader.ReadConfig("ConnectionStrings:MySQLConnection");
        var localCacheConnectionString = SecretAppsettingReader.ReadConfig("ConnectionStrings:LocalCache");
        var encryptionKey = SecretAppsettingReader.ReadConfig("ApplicationKeys:EncryptionKey1");
        LocalCache.ConnectionString = localCacheConnectionString;
        DbService.ConnectionString = dbConnectionString;

        UserSecretes = new UserSecretes
        {
          DbConnectionString = dbConnectionString,
          Url = url,
          LocalCacheConnectionString = localCacheConnectionString,
          EncryptionKey = encryptionKey,
          Password = password,
          Username = username
        };
      }
      catch (Exception)
      {
        throw;
      }
    }, CurrentErrorHandler);
  }

  private async Task CancelClicked() => await Service.Cancel(CurrentErrorHandler);

  public BehaviorSubject<bool> CanPending { get; } = new(false);

  private async Task PendingClicked()
  {
    var result = await DialogsManager.ShowMessage("Data trefbdajkdf dbvkbvksdj cxjlhcvad\n a) vhzjfhfjkdadf\n  b) fdhsdjkfdfsdf\n c)dfvfvafauhildf", "fdhyfuaba jsdh;sd", "gjhfk fahdfhgasdffjkl dfiasgdl", PromptBoxButtons.Open | PromptBoxButtons.Continue | PromptBoxButtons.Cancel, PromptBoxButtons.Cancel);
    await Service.PendingRecords(CurrentErrorHandler);
  }

  public BehaviorSubject<bool> CanProcess { get; } = new(false);

  private async Task ProcessClicked() => await Service.ProcessRecords(CurrentErrorHandler);

  public BehaviorSubject<bool> CanHarmonize { get; } = new(false);

  private async Task HarmonizeClicked() => await Service.HarmonizeRecords(CurrentErrorHandler);

  public BehaviorSubject<bool> CanProcessed { get; } = new(false);

  private async Task ProcessedClicked() => await Service.ProcessedRecords(CurrentErrorHandler);

  public BehaviorSubject<bool> CanClose { get; } = new(true);
  public UserSecretes UserSecretes { get => _userSecretes; set => this.RaiseAndSetIfChanged(ref _userSecretes, value); }

  private async Task CloseClicked()
  {
    try
    {
      if (Service.ProcessingData?.IsBusy ?? false)
        throw new Exception("It can not close while still processing\r\nEither cancel first or let it complete first");
      await Task.Run(() =>
      {
        Thread.Sleep(1000);
        Environment.Exit(0);
      });
    }
    catch (Exception ex)
    {
      NotifyMessage(new UserMessage
      {
        Message = ex.Message,
        MessageTitle = "Error closing system",
        MessageType = MessageType.Error,
        MessageDetails = ex.StackTrace
      });
    }
  }

  private void UpdateTitle(CompositeDisposable disposables)
  {
    this.WhenAnyValue(x => x.DocumentType)
    .Subscribe(x =>
    {
      var mainHeader = x switch
      {
        DocumentType.CountSheets => "Working on stock counts (count sheets)",
        DocumentType.Sales => "Working on cash sales - stock sales",
        DocumentType.Purchases => "Working on stock purchases - 40's",
        DocumentType.PettyCash => "Working on petty cash payments",
        DocumentType.Cheques => "Working on cheque payments - AP",
        DocumentType.GeneralJournals => "Working on general journals",
        DocumentType.Recievables => "Working on cash receipts - 505's",
        _ => "Unable to recognize documents being worked on",
      };
      HasDocuments = new[] { DocumentType.Sales, DocumentType.Purchases, DocumentType.CountSheets }.Contains(x);
      HasMonths = x != DocumentType.CountSheets;
      HasDate = x == DocumentType.CountSheets; ;
      UserMessage = default;
      ApplicationModelBase.PageTitle.OnNext(mainHeader);
    }).DisposeWith(disposables);
  }

  private void EnableCommands(CompositeDisposable disposables)
  {
    this.WhenAnyValue(b => b.IsBusy)
      .Throttle(TimeSpan.FromMilliseconds(5))
     .Subscribe(c =>
     {
       Dispatcher.UIThread.Invoke(() =>
       {
         CanCancel.OnNext(c);
         this.RaisePropertyChanged(nameof(Months));
       });
     });

    this.WhenAnyValue(b => b.Months)
      .Throttle(TimeSpan.FromMilliseconds(5))
   .Subscribe(c =>
   {
     Dispatcher.UIThread.Invoke(() =>
     {
       if (!IsBusy)
       {
         CanCancel.OnNext(false);
         CanHarmonize.OnNext(c?.Length > 6 || DocumentType == DocumentType.CountSheets);
         CanProcess.OnNext(c?.Length > 6 || DocumentType == DocumentType.CountSheets);
       }
       else
       {
         CanProcessed.OnNext(false);
         CanPending.OnNext(false);
       }
     });
   });
  }

  public void PageActivated(CompositeDisposable disposables)
  {
    UpdateTitle(disposables);
    EnableCommands(disposables);
  }

  private static DateTime _time;
  private bool _isBusy;

  public void ShowMessage(IUserMessage message) => HomePage.ShowMessage(message);

  public void NotifyMessage(IUserMessage message)
  {
    Dispatcher.UIThread.Invoke(() =>
    {
      try
      {
        _time = DateTime.Now;
        UserMessage = new UserMessage
        {
          ForeColor = MessageColorConverter.ConvertColor(message.MessageType),
          Message = message.Message,
          MessageDetails = message.MessageDetails,
          MessageTitle = message.MessageTitle,
          Time = _time,
          MessageType = message.MessageType
        };

        _messageTask?.Dispose();
        _messageTask = Task.Run(() =>
        {
          if (UserMessage is UserMessage userMsg)
          {
            Thread.Sleep(100000);
            if (_time == userMsg.Time)
              UserMessage = new UserMessage();
          }
        });
      }
      catch { }
    });
  }

  private static Task _messageTask = null;
  private UserSecretes _userSecretes;
  private bool _hasMonths;
  private bool _hasDate;
  private DateTime _date = new(2022, 6, 30);

  public void ReportProgress(IProgressMessage message)
  {
    Dispatcher.UIThread.Invoke(() =>
    {
      try
      {
        _time = DateTime.Now;
        MiniProgress = message.MiniProgress;
        Progress = message.Progress;
        MainProgress = message.OverallProgress;
        MiniProgressKind = message.ProcessingState switch
        {
          ProcessingState.GettingData => MaterialIconKind.ArrowBottomBoldCircleOutline,
          ProcessingState.HarmonizingData => MaterialIconKind.AxisZRotateClockwise,
          ProcessingState.PreProcessing => MaterialIconKind.BasketFill,
          ProcessingState.PostProcessing => MaterialIconKind.BasketUnfill,
          ProcessingState.Finalizing => MaterialIconKind.WindElectricityOutline,
          _ => MaterialIconKind.BikeFast,
        };

        UserMessage = new UserMessage
        {
          ForeColor = Brushes.Gray,
          Message = message.Message,
          MessageDetails = message.MessageDetails,
          MessageTitle = message.MessageTitle,
          Time = _time,
          MessageType = MessageType.Normal
        };

        _messageTask?.Dispose();

        _messageTask = Task.Run(() =>
        {
          if (UserMessage is UserMessage userMsg)
          {
            Thread.Sleep(100000);
            if (_time == userMsg.Time)
              UserMessage = new UserMessage();
          }
        });
      }
      catch { }
    });
  }

  public void CurrentErrorHandler(string message, string title = "Error", Exception error = null)
  {
    NotifyMessage(new UserMessage { Message = message ?? error?.Message, MessageTitle = title ?? "Error", MessageType = MessageType.Error });
  }
}
