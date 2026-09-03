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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Common;
using AvePoint.RA.Contract;

namespace AvePoint.RA.Service.Services.ManualApproval.AuditHandler
{
    public class ManualApprovalBeforeAuditHandler : IBeforeAuditHandler
    {
        public IRMManualApproveDao ManualApproveDao => PlatformWindsorManager.GetService<IRMManualApproveDao>();
        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {


            var info = new RMAuditInfo();
            info.Module = (AuditModule)model;
            info.Category = (AuditCategory)category;
            info.Action = (AuditAction)action;
            info.ModifyContent = new List<AuditItem>();

            if (info.Action == AuditAction.MarkToApproved || info.Action == AuditAction.MarkToRejected || info.Action == AuditAction.MarkToExtend)
            {
                var ids = args[2] as List<int>;
                List<RMManualApprove> items = await ManualApproveDao.FindListAsync(s => ids.Contains(s.Id));
                var idx = 1;
                foreach (var item in items)
                {
                    info.Object += item.Url + ";";
                    if (idx == 1)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_RC_Audit_ManualApproveStatus",
                            NewValue = GetManualApproveNewValue(info.Action),
                            OldValue = SOApproveDBStatusToString((SOApproveDBStatus)item.Status)
                        });
                    }
                    else
                    {
                        var old = info.ModifyContent.Where(s => s.TargetSetting == "RM_RC_Audit_ManualApproveStatus").FirstOrDefault();
                        if (old != null)
                        {
                            old.OldValue += "<br>" + SOApproveDBStatusToString((SOApproveDBStatus)item.Status);
                        }
                    }
                    idx++;
                }
            }
            else if (info.Action == AuditAction.ChangeAction)
            {
                var changedItems = args[2] as List<ChangedItems>;
                var ids = changedItems.Select(o => o.id).ToList();
                var option = (RelatedRecordOption)args[3];
                List<RMManualApprove> items = await ManualApproveDao.FindListAsync(s => ids.Contains(s.Id));
                var idx = 1;
                foreach (var item in items)
                {
                    info.Object += item.Url + ";";
                    if (idx == 1)
                    {
                        info.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = "RM_RC_Audit_WhetherDeleteRelatedRecord",
                            NewValue = RelatedRecordOptionToString(option),
                            OldValue = RelatedRecordOptionToString((RelatedRecordOption)item.RelatedRecordsAction)
                        });
                    }
                    //else
                    //{
                    //    var old = info.ModifyContent.Where(s => s.TargetSetting == I18NEntity.GetString("RM_RC_Audit_WhetherDeleteRelatedRecord")).FirstOrDefault();
                    //    if (old != null)
                    //    {
                    //        old.OldValue += "<br>" + RelatedRecordOptionToString((RelatedRecordOption)item.RelatedRecordsAction);
                    //    }
                    //}
                    idx++;
                }
            }
            return info;
        }

        public string GetManualApproveNewValue(AuditAction action)
        {
            if (action == AuditAction.MarkToApproved)
            {
                return SOApproveDBStatusToString(SOApproveDBStatus.Approved);
            }
            else if (action == AuditAction.MarkToExtend)
            {
                return SOApproveDBStatusToString(SOApproveDBStatus.Extend);
            }
            else if (action == AuditAction.MarkToRejected)
            {
                return SOApproveDBStatusToString(SOApproveDBStatus.Rejected);
            }
            return string.Empty;
        }

        public string SOApproveDBStatusToString(SOApproveDBStatus status)
        {
            return $"RM_JS_MA_ApproveStatus_{status.ToString()} ";
        }

        public string RelatedRecordOptionToString(RelatedRecordOption option)
        {
            switch (option)
            {
                case RelatedRecordOption.None:
                    return "RM_JS_RDM_RelatedRecordsAction_None";
                case RelatedRecordOption.Both:
                    return "RM_JS_RDM_RelatedRecordsAction_Both";
                default:
                    return string.Empty;
            }
           
        }
    }
}
