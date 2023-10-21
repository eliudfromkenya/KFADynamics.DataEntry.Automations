using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KFADynamics.DataEntry.Business.Models;

namespace KFADynamics.DataEntry.Business;
public static class Service
{
  public static IProcessingData? ProcessingData { get; set; }
  public static Task Cancel()
  {
    throw new NotImplementedException();
  }
  public static Task PendingRecords()
  {
    throw new NotImplementedException();
  }
  public async static Task ProcessRecords()
  {
    try
    {
      await ProcessingServices.Process(ProcessingData!, CurrentErrorHandler);
    }
    catch (Exception ex)
    {
      ProcessingData?.NotifyMessage(new UserMessage { Message = ex.Message, MessageTitle = "Error Processing Records", MessageType= MessageType.Error });
    }
  }
  public static Task HarmonizeRecords()
  {
    throw new NotImplementedException();
  }
  public static Task ProcessedRecords()
  {
    throw new NotImplementedException();
  }

  static void CurrentErrorHandler(string message, string title = "Error", Exception? error = null)
  {

  }
}
