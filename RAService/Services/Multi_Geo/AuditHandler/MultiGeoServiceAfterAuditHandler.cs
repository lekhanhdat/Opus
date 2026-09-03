using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Multi_Geo.AuditHandler
{
    public class MultiGeoServiceAfterAuditHandler : IAfterAuditHandler
    {
        private readonly IRMFunctionSettingDao RMFunctionSettingDao = PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        private readonly IRMKeyValueDao RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();


        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            if (args.Length > 1 && args[1] is bool isIgnoreAudit)
            {
                if (isIgnoreAudit)
                {
                    info.NotNeedRecordAudit = true;
                    return info;
                }
            }
            switch (action)
            {
                case (int)AuditAction.EnableMultiGeoFeature:
                    return await EnableMutiGeoActionAudit(info, returnValue);
                case (int)AuditAction.SaveMultiGeoIPConfig:
                    return SaveMultiGeoIPConfigActionAudit(info, returnValue);
                case (int)AuditAction.RunMainDCSyncCommonDataJob:
                case (int)AuditAction.RunOtherDCSyncCommonDataJob:
                   info.Status = string.IsNullOrEmpty(returnValue as string) ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
                    break;
            }
            return info;
        }

        private static RMAuditInfo SaveMultiGeoIPConfigActionAudit(RMAuditInfo info, object returnValue)
        {
            var result = (RAReturnMessage)returnValue;
            info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
            return info;
        }

        private async Task<RMAuditInfo> EnableMutiGeoActionAudit(RMAuditInfo info, object returnValue)
        {
            var result = (RAReturnMessage)returnValue;
            if (result.MessageType == RAMessageType.Skipped)
            {
                info.NotNeedRecordAudit = true;
                return info;
            }
            info.Status = result.MessageType == RAMessageType.Successful ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
            return info;
        }
    }
}
