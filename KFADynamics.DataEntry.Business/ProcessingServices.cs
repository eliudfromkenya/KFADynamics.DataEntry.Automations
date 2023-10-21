using System.Data;
using KFADynamics.DataEntry.Business.Delegates;
using KFADynamics.DataEntry.Business.Models;

namespace KFADynamics.DataEntry.Business;

internal static class ProcessingServices
{
  private static DataTable GetData(IProcessingData processingData)
  {
    ProgressMessage progressMessage = default;
    processingData?.ReportProgress(progressMessage = new ProgressMessage
    {
      Message = "Retrieving data from internal KFA system",
      MessageTitle = "Fetching Records",
      MiniProgress = 0,
      OverallProgress = 0,
      Progress = 0,
      ProcessingState = ProcessingState.GettingData
    });

    for (var i = 0; i < 100; i++)
    {
      progressMessage = progressMessage with { MiniProgress = (i * 7) % 100, OverallProgress = 1 * i / 6, Progress = i };
      processingData?.ReportProgress(progressMessage!);
      Thread.Sleep(100);
    }
    return new DataTable();
  }

  private static void CheckData(IProcessingData processingData, DataTable table)
  {
    ProgressMessage progressMessage = default;
    processingData?.ReportProgress(progressMessage = new ProgressMessage
    {
      Message = "Checking retrieved data",
      MessageTitle = "Analysing Records",
      MiniProgress = 0,
      OverallProgress = 0,
      Progress = 0,
      ProcessingState = ProcessingState.HarmonizingData
    });

    for (var i = 0; i < 100; i++)
    {
      progressMessage = progressMessage with { MiniProgress = (i * 7) % 100, OverallProgress = 2 * i / 6, Progress = i };
      processingData?.ReportProgress(progressMessage!);
      Thread.Sleep(100);
    }
  }

  private static void Preprocess(IProcessingData processingData, DataTable table)
  {
    ProgressMessage progressMessage = default;
    processingData?.ReportProgress(progressMessage = new ProgressMessage
    {
      Message = "Preparing data",
      MessageTitle = "Pre-Processing Records",
      MiniProgress = 0,
      OverallProgress = 0,
      Progress = 0,
      ProcessingState = ProcessingState.PreProcessing
    });

    for (var i = 0; i < 100; i++)
    {
      progressMessage = progressMessage with { MiniProgress = (i * 7) % 100, OverallProgress = 3 * i / 6, Progress = i };
      processingData?.ReportProgress(progressMessage!);
      Thread.Sleep(100);
    }
  }

  private static void ProcessRecords(IProcessingData processingData, DataTable table)
  {
    ProgressMessage progressMessage = default;
    processingData?.ReportProgress(progressMessage = new ProgressMessage
    {
      Message = "Processing Data",
      MessageTitle = "Processing Records",
      MiniProgress = 0,
      OverallProgress = 0,
      Progress = 0,
      ProcessingState = ProcessingState.Processing
    });

    for (var i = 0; i < 100; i++)
    {
      progressMessage = progressMessage with { MiniProgress = (i * 7) % 100, OverallProgress = 4 * i / 6, Progress = i };
      processingData?.ReportProgress(progressMessage!);
      Thread.Sleep(100);
    }
  }

  private static void Finalize(IProcessingData processingData, DataTable table)
  {
    ProgressMessage progressMessage = default;
    processingData?.ReportProgress(progressMessage = new ProgressMessage
    {
      Message = "Finalizing data transfer",
      MessageTitle = "Finalizing data",
      MiniProgress = 0,
      OverallProgress = 0,
      Progress = 0,
      ProcessingState = ProcessingState.Finalizing
    });

    for (var i = 0; i < 100; i++)
    {
      progressMessage = progressMessage with { MiniProgress = (i * 7) % 100, OverallProgress = 6 * i / 6, Progress = i };
      processingData?.ReportProgress(progressMessage!);
      Thread.Sleep(100);
    }
  }

  internal static async Task Process(IProcessingData processingData, ErrorHandler errorHandler)
  {
    await Task.Run(() =>
      Functions.RunOnBackground(() =>
      {
        try
        {
          processingData.IsBusy = true;
          using var table = GetData(processingData);
          CheckData(processingData, table);
          Preprocess(processingData, table);
          ProcessRecords(processingData, table);
          Postprocess(processingData, table);
          Finalize(processingData, table);
          processingData.IsBusy = false;

          processingData.ReportProgress(new ProgressMessage { });
          processingData.NotifyMessage(new UserMessage { Message = "Successfully Completed the transfer", MessageType = MessageType.Success, MessageTitle = "Done Successfully", MessageDetails = "Transfering cash sales" });


        }
        catch (Exception ex)
        {
          errorHandler(ex.Message, "Error processing", ex);
        }
      }, errorHandler));
  }

  private static void Postprocess(IProcessingData processingData, DataTable table)
  {
    ProgressMessage progressMessage = default;
    processingData?.ReportProgress(progressMessage = new ProgressMessage
    {
      Message = "Post processing data",
      MessageTitle = "Post Processing",
      MiniProgress = 0,
      OverallProgress = 0,
      Progress = 0,
      ProcessingState = ProcessingState.GettingData
    });

    for (var i = 0; i < 100; i++)
    {
      progressMessage = progressMessage with { MiniProgress = (i * 7) % 100, OverallProgress = 5 * i / 6, Progress = i };
      processingData?.ReportProgress(progressMessage!);
      Thread.Sleep(100);
    }
  }
}
