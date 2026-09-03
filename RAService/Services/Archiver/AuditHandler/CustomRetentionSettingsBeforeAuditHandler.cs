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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Archiver.AuditHandler
{
    public class CustomRetentionSettingsBeforeAuditHandler : IBeforeAuditHandler
    {
        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            RMAuditInfo auditInfo = new()
            {
                Object = string.Empty,
                Module = (AuditModule)model,
                Category = (AuditCategory)category,
                Action = (AuditAction)action,
                ModifyContent = new(),
            };

            AuditItem auditItem = new AuditItem
            {
                TargetSetting = "RM_RC_Audit_Configuration_File",
                OldValue = await _keyValueDao.GetValueByKeyAsync(KeyNameCollection.UploadedCustomRetentionSettingsFileName),
            };
            auditInfo.ModifyContent.Add(auditItem);

            return auditInfo;
        }
    }
}
