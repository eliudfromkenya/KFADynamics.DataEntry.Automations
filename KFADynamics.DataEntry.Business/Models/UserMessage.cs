namespace KFADynamics.DataEntry.Business.Models;

internal class UserMessage : IUserMessage
{
  public MessageType MessageType { get; init; }
  public string? Message { get; init; }
  public string? MessageDetails { get; init; }
  public string? MessageTitle { get; init; }
}
