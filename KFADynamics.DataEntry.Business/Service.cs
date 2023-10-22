using KFADynamics.DataEntry.Business.Delegates;
using KFADynamics.DataEntry.Business.Models;

namespace KFADynamics.DataEntry.Business;

public static class Service
{
  public static IProcessingData? ProcessingData { get; set; }

  public static async Task Cancel(ErrorHandler errorHandler)
  {
    // throw new NotImplementedException();
  }

  public static async Task PendingRecords(ErrorHandler errorHandler)
  {
    // throw new NotImplementedException();
  }

  public static async Task ProcessRecords(ErrorHandler errorHandler)
  {
    try
    {
      await ProcessingServices.Process(ProcessingData!, errorHandler);
    }
    catch (Exception ex)
    {
      errorHandler(ex.Message, "Error Processing Records", ex);
    }
  }

  public static async Task HarmonizeRecords(ErrorHandler errorHandler)
  {
    await Task.Run(() =>
    {
      ProcessingData?.ShowMessage(new UserMessage
      {
        Message = "Leta maneno tuone venye iko",
        MessageTitle = "Tutatest Maneno",
        MessageType = MessageType.Warning
      });
    });
  }

  public static async Task ProcessedRecords(ErrorHandler errorHandler)
  {
    // throw new NotImplementedException();
  }
}
