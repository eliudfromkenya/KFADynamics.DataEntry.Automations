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
using KFADynamics.DataEntry.Automations.Models;
using KFADynamics.DataEntry.Automations.Windows.ViewModels;
using KFADynamics.DataEntry.Business;
using Material.Icons;
using ReactiveUI;

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
  private UserMessage _userMessage;

  public bool IsBusy { get => _isBusy; set => this.RaiseAndSetIfChanged(ref _isBusy, value); }
  public UserMessage UserMessage { get => _userMessage; set => this.RaiseAndSetIfChanged(ref _userMessage, value); }
  public bool HasDocuments { get => _hasDocuments; set => this.RaiseAndSetIfChanged(ref _hasDocuments, value); }
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
    CancelCommand = ReactiveCommand.CreateFromTask(CancelClicked, CanCancel);
    HarmonizeCommand = ReactiveCommand.CreateFromTask(HarmonizeClicked, CanHarmonize);
    ProcessCommand = ReactiveCommand.CreateFromTask(ProcessClicked, CanProcess);
    ProcessedCommand = ReactiveCommand.CreateFromTask(ProcessedClicked, CanProcessed);
    PendingCommand = ReactiveCommand.CreateFromTask(PendingClicked, CanPending);
    CloseCommand = ReactiveCommand.CreateFromTask(CloseClicked, CanClose);
  }

  private async Task CancelClicked() => await Service.Cancel();

  public BehaviorSubject<bool> CanPending { get; } = new(false);

  private async Task PendingClicked() => await Service.PendingRecords();

  public BehaviorSubject<bool> CanProcess { get; } = new(false);

  private async Task ProcessClicked() => await Service.ProcessRecords();

  public BehaviorSubject<bool> CanHarmonize { get; } = new(false);

  private async Task HarmonizeClicked() => await Service.HarmonizeRecords();

  public BehaviorSubject<bool> CanProcessed { get; } = new(false);

  private async Task ProcessedClicked() => await Service.ProcessedRecords();

  public BehaviorSubject<bool> CanClose { get; } = new(true);
  IUserMessage IProcessingData.UserMessage { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
        DocumentType.Recievables => "Working on cash receipts - 505's",
        _ => "Unable to recognize documents being worked on",
      };
      HasDocuments = new[] { DocumentType.Sales, DocumentType.Purchases, DocumentType.CountSheets }.Contains(x);
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
         CanHarmonize.OnNext(!c);
         CanProcess.OnNext(!c);
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
         CanHarmonize.OnNext(c?.Length > 6);
         CanProcess.OnNext(c?.Length > 6);
       }
       CanProcessed.OnNext(true);
       CanPending.OnNext(true);
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

  public void NotifyMessage(IUserMessage message)
  {
    Dispatcher.UIThread.Invoke(() =>
    {
      try
      {
        _time = DateTime.Now;
        IBrush color = message.MessageType switch
        {
          MessageType.Error => Brushes.LightPink,
          MessageType.Warning => Brushes.Orange,
          MessageType.Success => Brushes.SkyBlue,
          _ => Brushes.Gray,
        };

        UserMessage = new UserMessage
        {
          ForeColor = color,
          Message = message.Message,
          MessageDetails = message.MessageDetails,
          MessageTitle = message.MessageTitle,
          Time = _time,
          MessageType = message.MessageType
        };

        _messageTask?.Dispose();
        _messageTask = Task.Run(() =>
        {
          Thread.Sleep(50000);
          if (_time == UserMessage.Time)
             UserMessage = new UserMessage();
        });
      }
      catch { }
    });
  }

  static Task? _messageTask = null;
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
          Thread.Sleep(50000);
          if (_time == UserMessage.Time)
            UserMessage = new UserMessage();
        });
      }
      catch { }
    });
  }
}
