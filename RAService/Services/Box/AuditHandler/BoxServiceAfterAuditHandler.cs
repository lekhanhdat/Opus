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
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Box.AuditHandler
{
    public class BoxServiceAfterAuditHandler : IAfterAuditHandler
    {
        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            if (action == (int)AuditAction.BoxSaveTermSetting)
            {
                List<AuditItem> cretiaAudit = info.ModifyContent.Where(a => a.Id == ContentRepositoryAuditUtil.NeedReAuditorInAfter).ToList();
                if (cretiaAudit.Count > 0)
                {
                    BoxSettingDto dto = (BoxSettingDto)args[0];
                    cretiaAudit[0].NewValue = ContentRepositoryAuditUtil.GetRulesCretiaString(dto.AutoClassificationRules);
                }
            }
            else if (action == (int)AuditAction.BoxRunDataSyncJob)
            {
                info.Module = AuditModule.ControlPanel;
                info.Category = (AuditCategory)category;
                var jobId = returnValue as string;
                info.Object = jobId;
            }
            else if (action == (int)AuditAction.BoxRunDisposalJob)
            {
                info.Module = AuditModule.BusinessClassificationManagement;
                info.Category = AuditCategory.SharePointSettings;
                var msg = returnValue as RAReturnMessage;
                if (msg != null)
                {
                    info.Object = msg.Extsion1 as string;
                    info.Status = (int)msg.MessageType;
                }
                else
                {
                    info.Object = returnValue.ToString();
                }
                
            }
            return info;
        }
    }
}
