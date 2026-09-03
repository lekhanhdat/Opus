using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.RMWeb.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Multi_Geo.AuditHandler
{
    public class MultiGeoServiceBeforeAuditHandler : IBeforeAuditHandler
    {
        private readonly IMultiGeoSettingService MultiGeoSettingService = PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            if(args.Length > 1 && args[1] is bool isIgnoreAudit)
            {
                if (isIgnoreAudit)
                {
                    return new RMAuditInfo { NotNeedRecordAudit = true };
                }
            }
            var info = new RMAuditInfo();
            info.ModifyContent = new List<AuditItem>();
            info.Action = (AuditAction)action;
            info.Category = (AuditCategory)category;
            info.Module = (AuditModule)model;
            switch (action)
            {
                case (int)AuditAction.SaveMultiGeoIPConfig:
                    {
                        var newMultiGeoSettingInfo = (List<MultiGeoSettingInfoDto>)args[0];
                        var oldMultiGeoSettingInfo = await MultiGeoSettingService.GetAllMultiGeoSetting();
                        foreach (var newItem in newMultiGeoSettingInfo)
                        {
                            var oldItem = oldMultiGeoSettingInfo.FirstOrDefault(m => m.DCInternalName == newItem.DCInternalName);
                            if (oldItem != null)
                            {
                                if (oldItem.IPAddresses != newItem.IPAddresses)
                                {
                                    info.ModifyContent.Add(new AuditItem
                                    {
                                        TargetSetting = "RM_AR_CP_Multi_Geo_Audit_IpConfig",
                                        OldValue = string.IsNullOrEmpty(oldItem.IPAddresses) ? string.Empty : string.Format("{0}: {1}", oldItem.DCDisplayName, oldItem.IPAddresses),
                                        NewValue = string.IsNullOrEmpty(newItem.IPAddresses) ? string.Empty : string.Format("{0}: {1}", newItem.DCDisplayName, newItem.IPAddresses)
                                    });
                                }
                            }
                        }
                        break;
                    }
            }
            return info;
        }
    }
}
