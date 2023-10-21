namespace KFADynamics.DataEntry.Business;

public interface IProgressMessage
{
  ProcessingState ProcessingState { get; init; }
  double MiniProgress { get; init; }
  double Progress { get; init; }
  double OverallProgress { get; init; }
  string? Message { get; init; }
  string? MessageDetails { get; init; }
  string? MessageTitle { get; init; }
}
