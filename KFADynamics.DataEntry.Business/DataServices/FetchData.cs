using System.Data;
using System.Text;
using KFA.ItemCodes.Classes;
using KFADynamics.DataEntry.Business.Classes;
using KFADynamics.DataEntry.Business.Models;
using MySqlConnector;

namespace KFADynamics.DataEntry.Business.DataServices;

internal static class FetchData
{
  internal static DataSet GetData(IProcessingData processingData)
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

    var filter = GetFilter(processingData);

    var file = processingData!.DocumentType switch
    {
      DocumentType.CountSheets => "CountSheets",
      DocumentType.Sales => "CashSales",
      DocumentType.Purchases => "Purchases",
      DocumentType.PettyCash => "PettyCash",
      DocumentType.Cheques => "PaidCheques",
      DocumentType.Recievables => "CashReceipts",
      DocumentType.GeneralJournals => "GeneralLedger",
      _ => throw new NotImplementedException("Getting dataset of unknown document type")
    };
    var sql = Functions.ReadManifestData<BackgroundWorkHelper>($"KFADynamics.DataEntry.Business.Resources.SQLs.{file}.sql", processingData!.CurrentErrorHandler);

    using var tsk = Task.Run(() => DbService.GetMySqlDataSet(sql!));
    for (var i = 0; i < 100000; i++)
    {
      progressMessage = progressMessage with
      {
        MiniProgress = ((i * 7) / 15) % 100,
        OverallProgress = i / 100,
        Progress = ((int)(i * 4.2) / 100) % 100
      };
      processingData?.ReportProgress(progressMessage!);
      if (tsk.IsCompleted)
        break;

      Thread.Sleep(100);
    }
    var ds = tsk.Result;
    NotifyGetData(ds, processingData);
    return tsk.Result;
  }

  private static string GetFilter(IProcessingData? processingData)
  {
    List<MySqlParameter> parameters = new();
    StringBuilder sql = new("WHERE ");
    IEnumerable<(string, string)>[]? values = null;
    switch (processingData!.DocumentType)
    {
      case DocumentType.CountSheets:
        sql.Append("tbl_count_sheet_batches.date = @date");
        parameters.Add(new MySqlParameter("@date", processingData.Date));

        values = new[]{
          GetValues("tbl_count_sheet_batches.cost_centre_code", processingData.BranchCodes),
          GetValues("tbl_count_sheet_batches.batch_key", processingData.BatchNumbers).Concat(
          GetValues("tbl_count_sheet_batches.batch_number", processingData.BatchNumbers)),
          GetValues("tbl_stock_count_sheets.document_number", processingData.DocumentNumbers) };
        break;
      case DocumentType.Sales:
        sql.Append("tbl_count_sheet_batches.date = @date");
        parameters.Add(new MySqlParameter("@date", processingData.Date));

        values = new[]{
          GetValues("tbl_count_sheet_batches.cost_centre_code", processingData.BranchCodes),
          GetValues("tbl_count_sheet_batches.batch_key", processingData.BatchNumbers).Concat(
          GetValues("tbl_count_sheet_batches.batch_number", processingData.BatchNumbers)),
          GetValues("tbl_stock_count_sheets.document_number", processingData.DocumentNumbers) };
        break;
      case DocumentType.Purchases:
        sql.Append("tbl_count_sheet_batches.date = @date");
        parameters.Add(new MySqlParameter("@date", processingData.Date));

        values = new[]{
          GetValues("tbl_count_sheet_batches.cost_centre_code", processingData.BranchCodes),
          GetValues("tbl_count_sheet_batches.batch_key", processingData.BatchNumbers).Concat(
          GetValues("tbl_count_sheet_batches.batch_number", processingData.BatchNumbers)),
          GetValues("tbl_stock_count_sheets.document_number", processingData.DocumentNumbers) };
        break;
      case DocumentType.PettyCash:
        sql.Append("tbl_count_sheet_batches.date = @date");
        parameters.Add(new MySqlParameter("@date", processingData.Date));

        values = new[]{
          GetValues("tbl_count_sheet_batches.cost_centre_code", processingData.BranchCodes),
          GetValues("tbl_count_sheet_batches.batch_key", processingData.BatchNumbers).Concat(
          GetValues("tbl_count_sheet_batches.batch_number", processingData.BatchNumbers)),
          GetValues("tbl_stock_count_sheets.document_number", processingData.DocumentNumbers) };
        break;
      case DocumentType.Cheques:
        sql.Append("tbl_count_sheet_batches.date = @date");
        parameters.Add(new MySqlParameter("@date", processingData.Date));

        values = new[]{
          GetValues("tbl_count_sheet_batches.cost_centre_code", processingData.BranchCodes),
          GetValues("tbl_count_sheet_batches.batch_key", processingData.BatchNumbers).Concat(
          GetValues("tbl_count_sheet_batches.batch_number", processingData.BatchNumbers)),
          GetValues("tbl_stock_count_sheets.document_number", processingData.DocumentNumbers) };
        break;
      case DocumentType.Recievables:
        sql.Append("tbl_count_sheet_batches.date = @date");
        parameters.Add(new MySqlParameter("@date", processingData.Date));

        values = new[]{
          GetValues("tbl_count_sheet_batches.cost_centre_code", processingData.BranchCodes),
          GetValues("tbl_count_sheet_batches.batch_key", processingData.BatchNumbers).Concat(
          GetValues("tbl_count_sheet_batches.batch_number", processingData.BatchNumbers)),
          GetValues("tbl_stock_count_sheets.document_number", processingData.DocumentNumbers) };
        break;
      case DocumentType.GeneralJournals:
        sql.Append("tbl_count_sheet_batches.date = @date");
        parameters.Add(new MySqlParameter("@date", processingData.Date));

        values = new[]{
          GetValues("tbl_count_sheet_batches.cost_centre_code", processingData.BranchCodes),
          GetValues("tbl_count_sheet_batches.batch_key", processingData.BatchNumbers).Concat(
          GetValues("tbl_count_sheet_batches.batch_number", processingData.BatchNumbers)),
          GetValues("tbl_stock_count_sheets.document_number", processingData.DocumentNumbers) };
        break;
      default:
        break;
    }
  }

  private static (string, string)[] GetValues(string columnName, string code)
  {
    if (string.IsNullOrWhiteSpace(code))
      return Array.Empty<(string, string)>();

    if (code.Contains(','))
      return code.Split(',')
        .Select(c => c?.Trim())
        .Where(c => !string.IsNullOrWhiteSpace(c))
        .Select(v => (columnName, v!)).ToArray();

    if (code.Contains(' '))
      return code.Split(' ')
        .Select(c => c?.Trim())
        .Where(c => !string.IsNullOrWhiteSpace(c))
        .Select(v => (columnName, v!)).ToArray();

    return new[] { (columnName, code) };
  }

  static void NotifyGetData(DataSet ds, IProcessingData? processingData)
  {
    string msg = string.Empty;
    switch (processingData!.DocumentType)
    {
      case DocumentType.CountSheets:
        if (ds.Tables[2].Rows.Count > 0)
          msg = ds.Tables[2].Rows.Count + " had already been processed";
        processingData?.ShowMessage(new UserMessage
        {
          MessageTitle = "Succesfully retrieved data to transfer",
          MessageType = MessageType.Success,
          Message = $"{ds.Tables[3].Rows.Count} count sheets records to be transfer. {msg}",
          MessageDetails = "Stock count sheets"
        });
        break;
      case DocumentType.Sales:
        break;
      case DocumentType.Purchases:
        break;
      case DocumentType.PettyCash:
        msg = string.Empty;
        if (ds.Tables[2].Rows.Count > 0)
          msg = ds.Tables[2].Rows.Count + " had already been processed";
        processingData?.ShowMessage(new UserMessage
        {
          MessageTitle = "Succesfully retrieved data to transfer",
          MessageType = MessageType.Success,
          Message = $"{ds.Tables[3].Rows.Count} petty cash records to be transfer. {msg}",
          MessageDetails = "Stock count sheets"
        });
        break;
      case DocumentType.Cheques:
        msg = string.Empty;
        if (ds.Tables[2].Rows.Count > 0)
          msg = ds.Tables[2].Rows.Count + " had already been processed";
        processingData?.ShowMessage(new UserMessage
        {
          MessageTitle = "Succesfully retrieved data to transfer",
          MessageType = MessageType.Success,
          Message = $"{ds.Tables[3].Rows.Count} cheques records to be transfer. {msg}",
          MessageDetails = "Stock count sheets"
        });
        break;
      case DocumentType.Recievables:
        msg = string.Empty;
        if (ds.Tables[2].Rows.Count > 0)
          msg = ds.Tables[2].Rows.Count + " had already been processed";
        processingData?.ShowMessage(new UserMessage
        {
          MessageTitle = "Succesfully retrieved data to transfer",
          MessageType = MessageType.Success,
          Message = $"{ds.Tables[3].Rows.Count} 505's records to be transfer. {msg}",
          MessageDetails = "Stock count sheets"
        });
        break;
      case DocumentType.GeneralJournals:
        msg = string.Empty;
        if (ds.Tables[2].Rows.Count > 0)
          msg = ds.Tables[2].Rows.Count + " had already been processed";
        processingData?.ShowMessage(new UserMessage
        {
          MessageTitle = "Succesfully retrieved data to transfer",
          MessageType = MessageType.Success,
          Message = $"{ds.Tables[3].Rows.Count} general journals records to be transfer. {msg}",
          MessageDetails = "Stock count sheets"
        });
        break;
    }
  }
}
