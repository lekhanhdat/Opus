using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.SharePoint.Report.Audit
{
    public class RMSharePointSiteMetricsReportAfterAuditHandler : IAsyncAuditAfterHandler
    {
        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo auditInfo, AuditModule module, AuditAction action, AuditCategory category, object[] args, object returnValue)
        {
            if (returnValue is RAReturnMessage response)
            {
                auditInfo.Status = response.MessageType == RAMessageType.Failed ? (int)RAMessageType.Failed : (int)RAMessageType.Successful;
                return auditInfo;
            }
            auditInfo.Object = returnValue?.ToString();

            return auditInfo;
        }
    }
}
