namespace KFADynamics.DataEntry.Business.Events;

public class ProgressEventArgs : EventArgs
{
  public string? CurrentStage { get; set; }
  public double SubPercentage { get; set; }
  public double Percentage { get; set; }
  public string? Message { get; set; }
  public string? SubMessage { get; set; }
  public object? Tag { get; set; }
  public bool IsCompleted { get; set; } = false;
}
