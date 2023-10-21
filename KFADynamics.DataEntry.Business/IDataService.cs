using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KFADynamics.DataEntry.Business.Events;

namespace KFADynamics.DataEntry.Business;
public interface IDataService
{
  Task<DataTable> GetTableAsync(DataFilter dataFilter);
  Task<DataTable> GetReportDataAsync(DataFilter dataFilter);
  Task UpdateProgressAsync(ProgressEventArgs Progress);
}
