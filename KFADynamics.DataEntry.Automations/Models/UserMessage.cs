using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using KFADynamics.DataEntry.Business;

namespace KFADynamics.DataEntry.Automations.Models;
public readonly struct UserMessage : IUserMessage
{
  public IBrush ForeColor { get; init; }
  public string MessageTitle { get; init; }
  public string MessageDetails { get; init; }
  public string Message { get; init; }
  public DateTime Time { get; init; }
  public MessageType MessageType { get; init; }
}
