using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class MainDataSyncCommonDataJobDetails : JMJobDetails
    {
        public string DataCenterName { get; set; }
        public MainDataSyncCommonAction Action { get; set; }

        public string ActionStr { get; set; }
    }

    public enum MainDataSyncCommonAction
    {
        None,
        InitTenant,
        RunSyncCommonDataJob,
    }
}
