using System.Data;
using KFADynamics.DataEntry.Business.Events;

namespace KFADynamics.DataEntry.Business.DataServices;

internal class CashReceiptsDataService : IDataService
{
  public Task<DataTable> GetReportDataAsync(DataFilter dataFilter)
  {
    throw new NotImplementedException();
  }

  public Task<DataTable> GetTableAsync(DataFilter dataFilter)
  {
    throw new NotImplementedException();
  }

  public Task UpdateProgressAsync(ProgressEventArgs Progress)
  {
    throw new NotImplementedException();
  }
}
