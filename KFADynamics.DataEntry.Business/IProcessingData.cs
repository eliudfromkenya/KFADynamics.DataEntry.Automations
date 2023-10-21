using System.Reactive.Subjects;
using System.Windows.Input;

namespace KFADynamics.DataEntry.Business;
public interface IProcessingData
{
  string BatchNumbers { get; set; }
  string BranchCodes { get; set; }
  BehaviorSubject<bool> CanCancel { get; }
  ICommand CancelCommand { get; }
  BehaviorSubject<bool> CanClose { get; }
  BehaviorSubject<bool> CanHarmonize { get; }
  BehaviorSubject<bool> CanPending { get; }
  BehaviorSubject<bool> CanProcess { get; }
  BehaviorSubject<bool> CanProcessed { get; }
  bool ChangeUserBranchCodeToMatchDocumentsBranch { get; set; }
  ICommand CloseCommand { get; }
  string DocumentNumbers { get; set; }
  DocumentType DocumentType { get; set; }
  bool GenerateAfterProcessReport { get; set; }
  ICommand HarmonizeCommand { get; }
  bool HasDocuments { get; set; }
  double MainProgress { get; set; }
  double MiniProgress { get; set; }
  string Months { get; set; }
  int NumberOfPagesToEmploy { get; set; }
  ICommand PendingCommand { get; }
  bool IsBusy { get; set; }
  bool PostRecordsAfterProcessing { get; set; }
  ICommand ProcessCommand { get; }
  ICommand ProcessedCommand { get; }
  double Progress { get; set; }
  string SubBackgroundImage { get; set; }
  IUserMessage UserMessage { get; set; }
  void NotifyMessage(IUserMessage message);
  void ReportProgress(IProgressMessage message);
}
