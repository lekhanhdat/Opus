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
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Common.Global.Utils;

namespace AvePoint.RA.Service.Services.PhysicalReqeust.AuditHandler
{
    class PhysicalRequestAfterAuditHandler : IAfterAuditHandler
    {
        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = new RMAuditInfo();
            auditInfo.Module = (AuditModule)model;
            auditInfo.Category = (AuditCategory)category;
            auditInfo.Action = (AuditAction)action;
            switch (auditInfo.Action)
            {
                case AuditAction.SavePhysicalRequest:
                    return CollectSave(auditInfo, args, target, returnValue);
                case AuditAction.LoanPhysicalRequest:
                    return CollectLoan(auditInfo, args, target, returnValue);
                case AuditAction.PhyLoanBoxJob:
                    return CollectLoanBoxJob(auditInfo, args, target, returnValue);
                case AuditAction.UpdatePhysicalRequest:
                    return CollectUpdate(auditInfo, info, args, target, returnValue);
                case AuditAction.ApprovePhysicalRequest:
                    return CollectApprove(auditInfo, args, target, returnValue);
                case AuditAction.MobileApprovalLoanRequest:
                    return CollectMobileApprove(auditInfo, args, target, returnValue);
                case AuditAction.RejectPhysicalRequest:
                    return CollectReject(auditInfo, args, target, returnValue);
                case AuditAction.CancelRequest:
                    return CollectCancelRequest(auditInfo, args, target, returnValue);
                case AuditAction.MovePhysicalRequest:
                    return CollectMove(auditInfo, args, target, returnValue);
                case AuditAction.PhyMoveDataJob:
                    return CollectMoveDataJob(auditInfo, args, target, returnValue);
                default:
                    return null;
            }
        }

        private RMAuditInfo CollectSave(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            string title = string.Empty;
            var param = args[0] as PhysicalRequestDto;
            PhysicalRequestResult result = returnValue as PhysicalRequestResult;
            auditInfo.Object = param.Title;
            auditInfo.Status = result == null || result.HasError ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            return auditInfo;
        }
        private RMAuditInfo CollectApprove(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            PhysicalRequestParam param = args[0] as PhysicalRequestParam;
            PhysicalRequestResult result = returnValue as PhysicalRequestResult;
            auditInfo.NotNeedRecordAudit = result.StartLoanBoxJob;
            auditInfo.Object = string.Join(",", param.Requests.Select(a=>a.Titles));
            auditInfo.Status = result == null || result.HasError ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            return auditInfo;
        }
        private RMAuditInfo CollectMobileApprove(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            var param = args[0] as MobileApprovalLoanDto;
            PhysicalRequestResult result = returnValue as PhysicalRequestResult;
            auditInfo.Object = string.Join(",", param.RequestDtos.Select(a => a.Name).ToArray());
            auditInfo.Status = result == null || result.HasError ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            return auditInfo;
        }

        private RMAuditInfo CollectReject(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            PhysicalRequestParam param = args[0] as PhysicalRequestParam;
            PhysicalRequestResult result = returnValue as PhysicalRequestResult;
            auditInfo.Object = string.Join(",", param.Requests.Select(a => a.Titles));
            auditInfo.Status = result == null || result.HasError ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            return auditInfo;
        }

        private RMAuditInfo CollectCancelRequest(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            PhysicalRequestParam param = args[0] as PhysicalRequestParam;
            PhysicalRequestResult result = returnValue as PhysicalRequestResult;
            auditInfo.Object = string.Join(",", param.Requests.Select(a => a.Titles));
            auditInfo.Status = result == null || result.HasError ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            return auditInfo;
        }

        private RMAuditInfo CollectLoan(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            string title = string.Empty;
            var param = args[0] as LoanRequestDto;
            PhysicalRequestResult result = returnValue as PhysicalRequestResult;
            auditInfo.Object = string.Join(",", param.Items.Select(i => i.Name));
            auditInfo.Status = result == null || result.HasError ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            return auditInfo;
        }
        private RMAuditInfo CollectMove(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            string title = string.Empty;
            var param = args[0] as MoveRequestDto;
            PhysicalRequestResult result = returnValue as PhysicalRequestResult;
            auditInfo.Object = string.Join(",", param.Items.Select(i => i.Name));
            auditInfo.Status = result == null || result.HasError ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            return auditInfo;
        }
        private RMAuditInfo CollectMoveDataJob(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            string title = string.Empty;
            ArgumentCheck.NotNull(returnValue, nameof(returnValue));
            auditInfo.Object = returnValue?.ToString();
            auditInfo.Status = !string.IsNullOrEmpty(returnValue.ToString()) ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
            return auditInfo;
        }

        private RMAuditInfo CollectLoanBoxJob(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            string title = string.Empty;
            var jobType = (JobType)args[0];
            if (jobType == JobType.PhysicalReturnBox)
            {
                auditInfo.Category = AuditCategory.PhysicalRecordsExplorer;
                auditInfo.Action = AuditAction.PhyReturnBoxJob;
            }
            ArgumentCheck.NotNull(returnValue, nameof(returnValue));
            auditInfo.Object = returnValue?.ToString();
            auditInfo.Status = !string.IsNullOrEmpty(returnValue.ToString()) ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
            return auditInfo;
        }

        private RMAuditInfo CollectUpdate(RMAuditInfo auditInfo, RMAuditInfo compareInfo, object[] args, object target, object returnValue)
        {
            PhysicalRequestDto param = args[0] as PhysicalRequestDto;
            PhysicalRequestResult result = returnValue as PhysicalRequestResult;
            auditInfo.Object = param.Title;
            auditInfo.Status = result == null || result.HasError ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            if (compareInfo != null && compareInfo.ModifyContent != null)
            {
                auditInfo.ModifyContent = new List<AuditItem>();
                foreach(AuditItem item in compareInfo.ModifyContent)
                {
                    if (item.TargetSetting == AuditConstants.Audit_Physical_Request_File_Title)
                    {
                        auditInfo.ModifyContent.Add(new AuditItem() {
                            TargetSetting = I18NEntity.GetString(AuditConstants.Audit_Physical_Request_File_Title),
                            OldValue = item.OldValue, NewValue = param.Title });
                    }
                    else if (item.TargetSetting == AuditConstants.Audit_Physical_Request_Comment)
                    {
                        auditInfo.ModifyContent.Add(new AuditItem() {
                            TargetSetting = I18NEntity.GetString(AuditConstants.Audit_Physical_Request_Comment),
                            OldValue = item.OldValue, NewValue = param.Comment });
                    }
                    else if (item.TargetSetting == AuditConstants.Audit_Physical_Request_Hold_User)
                    {
                        auditInfo.ModifyContent.Add(new AuditItem() {
                            TargetSetting = I18NEntity.GetString(AuditConstants.Audit_Physical_Request_Hold_User),
                            OldValue = item.OldValue, NewValue = param.HoldUserId });
                    }
                    else if (item.TargetSetting == AuditConstants.Audit_Physical_Request_EndTime)
                    {
                        auditInfo.ModifyContent.Add(new AuditItem() {
                            TargetSetting = I18NEntity.GetString(AuditConstants.Audit_Physical_Request_EndTime),
                            OldValue = item.OldValue, NewValue = new DateTime(param.DisposalClass.EndTime).ToString() });
                    }
                }
            }
            return auditInfo;
        }
    }
}
