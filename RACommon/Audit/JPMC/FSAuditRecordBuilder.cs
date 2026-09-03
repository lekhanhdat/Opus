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
using AvePoint.RA.Common.ClientRequest;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Tenant;
using System;

namespace AvePoint.RA.Common.Audit.JPMC
{
    public static class FSAuditRecordBuilder
    {
        public static FSAuditRecord BuildWithValidation(FSAuditContext context, object returnValue)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
           
            if (!string.IsNullOrWhiteSpace(context.ErrorMessage)) return null;
          
            var status = ResolveStatus(context, returnValue);
        
            if (IsComparisonAction(context.AuditType) && context.ModifiedContents is not { Count: > 0 }) return null;
         
            return new FSAuditRecord
            {
                Id = Guid.NewGuid(),
                AuditType = (int)context.AuditType,
                AuditLevel = (int)context.AuditLevel,
                ActionTimeUtc = context.ActionTimeUtc,
                UserName = ResolveUserName(context.ExecutedBy),
                ClientIP = ClientRequestLocalValue.ClientIP,
                Content = context.ModifiedContents is { Count: > 0 } ? SerializerHelper.SerializeToXmlString(context.ModifiedContents) : null,
                Status = (int)context.Status,
                ObjectName = context.ObjectName,
                ConnectionGroupId = $"{context.ConnectionGroupId}",
                ConnectionId = $"{context.ConnectionId}",
                ItemId = $"{context.ItemId}",
                CurrentPath = context.CurrentPath,
                PreviousPath = context.PreviousPath
            };
        }

        private static AuditStatus ResolveStatus(FSAuditContext context, object returnValue)
        {
            if (context.Status != AuditStatus.Successful)
                return AuditStatus.Failed;

            if (returnValue is bool boolVal && boolVal == false)
                return AuditStatus.Failed;

            if (returnValue is int intVal && intVal != 1)
                return AuditStatus.Failed;

            if (returnValue is RAReturnMessage returnMsg && returnMsg.MessageType != RAMessageType.Successful)
                return AuditStatus.Failed;

            return AuditStatus.Successful;
        }

        private static bool IsComparisonAction(FSAuditType type) => type switch
        {
            FSAuditType.EditFSGroup => true,
            FSAuditType.EditFSConnection => true,
            FSAuditType.PermissionChange => true,
            FSAuditType.FSEditGeneralSettingForJPMC => true,
            FSAuditType.FSEditDocLevelSettingForJPMC => true,
            FSAuditType.FSEditLocationOwnersSetting => true,
            //FSAuditType.ApplyClassCodeSettings4FS => true,
            //FSAuditType.FSConnectionCorrelateGroup => true,
            //FSAuditType.FSClassificationSetting => true,
            _ => false
        };

        private static string ResolveUserName(FSAuditExecutedBy executedBy)
        {
            return executedBy switch
            {
                FSAuditExecutedBy.System => "RM_TS_RunSchedule",
                FSAuditExecutedBy.User => GetCurrentUserName(),
                _ => GetCurrentUserName()
            };
        }

        private static string GetCurrentUserName()
        {
            return TenantLocalValue.LogonUserEmail ?? TenantLocalValue.PartnerUser ?? WebUtil.LogOnUserName;
        }
    }
}