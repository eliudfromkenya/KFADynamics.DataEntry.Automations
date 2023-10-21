﻿using KFADynamics.DataEntry.Business.Events;

namespace KFADynamics.DataEntry.Business.AutomationServices;

internal class CountSheetsService : IAutomationService
{
  public bool AsHeadless { get; set; }
  public int SleepTime { get; set; }
  public int MaxNoOfActivePages { get; set; }
  public bool PostData { get; set; }
  public IDataService? DataService { get; set; }

  public event ProgressEventHandler? ProgressEvent;

  protected virtual void OnProgressEvent(ProgressEventArgs progress) => ProgressEvent?.Invoke(this, progress);

  public Task ProcessAsync()
  {
    throw new NotImplementedException();
  }
}
