namespace KFADynamics.DataEntry.Playwright.Models;

public readonly struct UserSecretes
{
  public string DbConnectionString { get; init; } = string.Empty;
  public string LocalCacheConnectionString { get; init; } = string.Empty;
  public string EncryptionKey { get; init; } = string.Empty;
  public string Url { get; init; } = string.Empty;
  public string Username { get; init; } = string.Empty;
  public string? Password { get; init; } = string.Empty;

  public UserSecretes()
  {
  }
}
