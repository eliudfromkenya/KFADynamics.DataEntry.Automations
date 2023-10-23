using System.Data;
using KFADynamics.DataEntry.Business.Events;

namespace KFADynamics.DataEntry.Business;

public delegate void ProgressEventHandler(object sender, ProgressEventArgs e);

public interface IAutomationService
{
  bool AsHeadless { get; set; }
  int SleepTime { get; set; }
  int MaxNoOfActivePages { get; set; }
  bool PostData { get; set; }
  DataSet? Data { get; set; }
  event ProgressEventHandler? ProgressEvent;
  Task ProcessAsync();
}
