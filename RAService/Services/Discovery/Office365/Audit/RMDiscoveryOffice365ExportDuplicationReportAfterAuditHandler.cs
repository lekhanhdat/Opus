/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Audit
{
    public class RMDiscoveryOffice365ExportDuplicationReportAfterAuditHandler : IAsyncAuditAfterHandler
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365ExportDuplicationReportAfterAuditHandler));
        private IRMRemoteNodeDao RMRemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo auditInfo, AuditModule module, AuditAction action, AuditCategory category, object[] args, object returnValue)
        {
            if (returnValue is RAReturnMessage response)
            {
                auditInfo.Status = response.MessageType == RAMessageType.Failed ? (int)RAMessageType.Failed : (int)RAMessageType.Successful;
                return auditInfo;
            }
            auditInfo.Object = returnValue?.ToString();

            switch (action)
            {
                case AuditAction.DiscoveryCleanUpDuplicateDatas:
                    if (args.Length < 3) break;
                    RMDiscoveryOffice365CleanupInfoDto cleanupInfoDto = null;
                    try
                    {
                        cleanupInfoDto = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoveryOffice365CleanupInfoDto>(args[2].ToString());
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"Failed to deserialize the cleanup info for audit, exception: {e}");
                    }
                    if (cleanupInfoDto == null) break;
                    auditInfo.ModifyContent ??= [];
                    var tenantName = RMRemoteNodeDao.GetTenantNameByO365TenantId(cleanupInfoDto.O365TenantId);

                    if (!string.IsNullOrEmpty(tenantName))
                    {
                        AuditHelper.SaveNewAuditItem(auditInfo, "RM_RC_Audit_Action_DiscoveryO365CleanUpDuplicateDatas_Tenant", tenantName);
                    }
                    
                    AuditHelper.SaveNewAuditItem(auditInfo, "RM_RC_Audit_Action_DiscoveryO365CleanUpDuplicateDatas_Storage", cleanupInfoDto.CleanupInfo.StoragePolicyName);
                    break;
                default:
                    break;
            }

            return auditInfo;
        }
    }
}
