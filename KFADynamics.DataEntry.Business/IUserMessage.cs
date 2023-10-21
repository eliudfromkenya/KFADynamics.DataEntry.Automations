using System;
using System.Linq;

namespace KFADynamics.DataEntry.Business;
public enum MessageType { Normal = 0, Error = 1, Warning = 2, Success = 3 }
public interface IUserMessage
{
  MessageType MessageType { get; init; }
  string? Message { get; init; }
  string? MessageDetails { get; init; }
  string? MessageTitle { get; init; }
}
