using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFADynamics.DataEntry.Business.Models;
internal readonly struct ProgressMessage : IProgressMessage
{
  public ProcessingState ProcessingState {get; init; }
  public double MiniProgress {get; init; }
  public double Progress {get; init; }
  public double OverallProgress {get; init; }
  public string? Message {get; init; }
  public string? MessageDetails {get; init; }
  public string? MessageTitle {get; init; }
}
