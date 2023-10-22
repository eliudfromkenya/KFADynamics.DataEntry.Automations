namespace KFADynamics.DataEntry.Playwright;

public interface IClosablePage
{
  AnAction Close { get; set; }

  event EventHandler Closed;
}
