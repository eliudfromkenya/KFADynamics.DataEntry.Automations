using System.Data;
using KFADynamics.DataEntry.Business.Events;

namespace KFADynamics.DataEntry.Business.AutomationServices;

internal class CashReceiptsService : IAutomationService
{
  public bool AsHeadless { get; set; }
  public int SleepTime { get; set; }
  public int MaxNoOfActivePages { get; set; }
  public bool PostData { get; set; }
  public DataSet? Data { get; set; }

  public event ProgressEventHandler? ProgressEvent;

  public Task ProcessAsync()
  {
    throw new NotImplementedException();
  }

  protected virtual void OnProgressEvent(ProgressEventArgs progress) => ProgressEvent?.Invoke(this, progress);
}
