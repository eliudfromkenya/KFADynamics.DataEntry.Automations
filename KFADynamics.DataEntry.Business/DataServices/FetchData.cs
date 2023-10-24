using System.Data;
using System.Text;
using KFA.ItemCodes.Classes;
using KFADynamics.DataEntry.Business.Classes;
using KFADynamics.DataEntry.Business.Models;
using MySqlConnector;
using Humanizer;
using OfficeOpenXml;
using Microsoft.VisualBasic;
using System.Diagnostics;

namespace KFADynamics.DataEntry.Business.DataServices;

internal static class FetchData
{
  static void ExportToExcel(DataSet ds, IProcessingData processingData)
  {
    try
    {
      foreach (DataTable table in ds.Tables)
        foreach (DataColumn dc in table.Columns)
          dc.ColumnName = dc.ColumnName?.Titleize();

      string dateFormat = "MMM dd, yyyy";
      string moneyFormat = "#,##0.00";
    using ExcelPackage pck = new ExcelPackage();
      foreach (DataTable dataTable in ds.Tables)
      {
        ExcelWorksheet workSheet = pck.Workbook.Worksheets.Add(dataTable.TableName);
        workSheet.Cells["A1"].LoadFromDataTable(dataTable, true);
        for (int c = 0; c < dataTable.Columns.Count; c++)
        {
          if (dataTable.Columns[c].DataType == typeof(DateTime))
            workSheet.Column(c + 1).Style.Numberformat.Format = dateFormat;
          if (dataTable.Columns[c].DataType == typeof(decimal))
            workSheet.Column(c + 1).Style.Numberformat.Format = moneyFormat;
        }
      }

      var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "KFA to Dynamics Data Transfer");
      if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);
      var filePath = Path.Combine(folder, $"Data {DateTime.Now:yyyy MMM dd HH_mm_ss}.xlsx");
      pck.SaveAs(new FileInfo(filePath));
      Process.Start(filePath);
    }
    catch (Exception ex)
    {
      processingData?.ShowMessage(new UserMessage
      {
        Message = ex.Message,
        MessageDetails = ex.StackTrace,
        MessageTitle = "Error exporting to excel",
        MessageType = MessageType.Error
      });
    }
  }
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
    var sql = Functions.ReadManifestData<BackgroundWorkHelper>($"KFADynamics.DataEntry.Business.Resources.SQLs.{file}.sql", processingData!.CurrentErrorHandler)!;

    var sqlFilter = GetFilter(out var pars, processingData);
    sql = sql.Replace("<<<sql_filter>>>", sqlFilter);

    using var tsk = Task.Run(() => DbService.GetMySqlDataSet(sql, pars));
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

    ExportToExcel(ds, processingData!);
    NotifyGetData(ds, processingData);
    return tsk.Result;
  }

  private static string GetFilter(out MySqlParameter[] pars, IProcessingData processingData)
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
        var months = GetValues("tbl_cash_sales_batches.batch_month", processingData.Months);

        if (!months.Any())
          throw new InvalidOperationException("Month(s) is required in order to process cash sales");

        for (int i = 0; i < months.Length; i++)
        {
          if (i > 0) sql.Append(" OR ");
          sql.Append($"tbl_cash_sales_batches.batch_month = @month{i + 1}");
          parameters.Add(new MySqlParameter($"@month{i + 1}", processingData.Months));
        }

        values = new[]{
          GetValues("tbl_cash_sales_batches.cost_centre_code", processingData.BranchCodes),
          GetValues("tbl_cash_sales_batches.batch_key", processingData.BatchNumbers).Concat(
          GetValues("tbl_cash_sales_batches.batch_number", processingData.BatchNumbers)),
          GetValues("tbl_cash_sales_documents.cash_sale_number", processingData.DocumentNumbers) };
        break;
      case DocumentType.Purchases:
        months = GetValues("tbl_order_batch_headers.`month`", processingData.Months);

        if (!months.Any())
          throw new InvalidOperationException("Month(s) is required in order to process purchases (40's)");

        for (int i = 0; i < months.Length; i++)
        {
          if (i > 0) sql.Append(" OR ");
          sql.Append($"tbl_order_batch_headers.`month` = @month{i + 1}");
          parameters.Add(new MySqlParameter($"@month{i + 1}", processingData.Months));
        }

        values = new[]{
          GetValues("tbl_order_documents.cost_centre_code", processingData.BranchCodes),
          GetValues("tbl_order_batch_headers.batch_key", processingData.BatchNumbers).Concat(
          GetValues("tbl_order_batch_headers.batch_number", processingData.BatchNumbers)),
          GetValues("tbl_order_documents.lpo_number", processingData.DocumentNumbers) };
        break;
      case DocumentType.PettyCash:
        months = GetValues("tbl_petty_cash_batch_headers.`month`", processingData.Months);

        if (!months.Any())
          throw new InvalidOperationException("Month(s) is required in order to process petty cash");

        for (int i = 0; i < months.Length; i++)
        {
          if (i > 0) sql.Append(" OR ");
          sql.Append($"tbl_petty_cash_batch_headers.`month` = @month{i + 1}");
          parameters.Add(new MySqlParameter($"@month{i + 1}", processingData.Months));
        }

        values = new[]{
          GetValues("tbl_petty_cash_batch_headers.cost_centre_code", processingData.BranchCodes),
          GetValues("tbl_petty_cash_batch_headers.batch_key", processingData.BatchNumbers).Concat(
          GetValues("tbl_petty_cash_batch_headers.batch_number", processingData.BatchNumbers)) };
        break;
      case DocumentType.Cheques:
        months = GetValues("tbl_cheque_requisition_batches.`month`", processingData.Months);

        if (!months.Any())
          throw new InvalidOperationException("Month(s) is required in order to process cheques");

        for (int i = 0; i < months.Length; i++)
        {
          if (i > 0) sql.Append(" OR ");
          sql.Append($"tbl_cheque_requisition_batches.`month` = @month{i + 1}");
          parameters.Add(new MySqlParameter($"@month{i + 1}", processingData.Months));
        }

        values = new[]{
          GetValues("tbl_cheque_requisition_batches.cost_centre_code", processingData.BranchCodes),
          GetValues("tbl_cheque_requisition_batches.batch_key", processingData.BatchNumbers).Concat(
          GetValues("tbl_cheque_requisition_batches.batch_number", processingData.BatchNumbers)) };
        break;
      case DocumentType.Recievables:
        months = GetValues("tbl_cash_receipts_batches.`month`", processingData.Months);

        if (!months.Any())
          throw new InvalidOperationException("Month(s) is required in order to process cash receipts (505's)");

        for (int i = 0; i < months.Length; i++)
        {
          if (i > 0) sql.Append(" OR ");
          sql.Append($"tbl_cash_receipts_batches.`month` = @month{i + 1}");
          parameters.Add(new MySqlParameter($"@month{i + 1}", processingData.Months));
        }

        values = new[]{
          GetValues("tbl_cash_receipts_batches.cost_centre_code", processingData.BranchCodes),
          GetValues("tbl_cash_receipts_batches.batch_key", processingData.BatchNumbers).Concat(
          GetValues("tbl_cash_receipts_batches.batch_number", processingData.BatchNumbers)) };
        break;
      case DocumentType.GeneralJournals:
        months = GetValues("tbl_custom_general_ledger_batches.`month`", processingData.Months);

        if (!months.Any())
          throw new InvalidOperationException("Month(s) is required in order to process general journals");

        for (int i = 0; i < months.Length; i++)
        {
          if (i > 0) sql.Append(" OR ");
          sql.Append($"tbl_custom_general_ledger_batches.`month` = @month{i + 1}");
          parameters.Add(new MySqlParameter($"@month{i + 1}", processingData.Months));
        }

        values = new[]{
          GetValues("tbl_custom_general_ledger_batches.cost_centre_code", processingData.BranchCodes),
          GetValues("tbl_custom_general_ledger_batches.batch_key", processingData.BatchNumbers).Concat(
          GetValues("tbl_custom_general_ledger_batches.batch_number", processingData.BatchNumbers)) };
        break;
      default:
        throw new InvalidOperationException($"Unable to generate filters for {processingData.DocumentType} documents");
    }
    var mx = sql.ToString().Replace("WHERE ", "");
    sql = sql.Replace(mx, $"({mx})");
    int paramIndex = 1;
    foreach (var value in values)
    {
      StringBuilder sb = new();
      for (int i = 0; i < value.Count(); i++)
      {
        var obj = value.ElementAt(i);
        if (i > 0) sb.Append(" OR ");
        var param = $"@p{paramIndex++}";
        sb.Append($"{obj.Item1}={param}");
        parameters.Add(new MySqlParameter($"{param}", obj.Item2));
      }
      if (value.Any())
        sql.Append($" AND ({sb})");
    }
    pars = parameters.ToArray();
    return sql.ToString();
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
