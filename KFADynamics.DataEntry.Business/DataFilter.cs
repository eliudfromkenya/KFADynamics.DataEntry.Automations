namespace KFADynamics.DataEntry.Business;

public readonly struct DataFilter
{
  public string[]? BranchCodes { get; }
  public string[]? Months { get; }
  public string[]? BatchKeys { get; }
  public string[]? BatchNumbers { get; }
  public string[]? DocumentNumbers { get; }
}
