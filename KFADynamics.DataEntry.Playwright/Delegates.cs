using System.Data;

namespace KFADynamics.DataEntry.Playwright;

public delegate void GetReportHtml(DataSet ds, string name, string header,
     Action<string> htmlGetter, string reportSerial, string watermark = "PPAS",
     Dictionary<string, double> colWidths = null);

public delegate void AnAction(object par = null);

public delegate Task<bool?> AskQuestion(string question, string title = "Pilgrims Project Assistant", bool? defaultValue = null);

public delegate Task ShowMessage(string message, string title = "Pilgrims Project Assistant");

public record ReportMetaData
{
  public string Name;
  public Dictionary<string, string> ParameterData;
  public Func<Dictionary<string, object>> GetData;
  public string ReportCategory;
}

public delegate ReportMetaData GetReport();

public static class Delegates
{
  public static GetReportHtml GetReportHtml { get; set; }
}
