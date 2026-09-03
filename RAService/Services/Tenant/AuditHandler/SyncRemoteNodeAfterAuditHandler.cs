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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.TaxonomyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Tenant.AuditHandler
{
    public class SyncRemoteNodeAfterAuditHandler : IAfterAuditHandler
    {
        private RALogger logger = RALogger.GetInstance(typeof(SyncRemoteNodeAfterAuditHandler));

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            var auditInfo = new RMAuditInfo()
            {
                Module = (AuditModule)model,
                Category = (AuditCategory)category,
                Action = (AuditAction)action,

                ModifyContent = new List<AuditItem>()
            };

            try
            {
                if (action == (int)AuditAction.SyncRemoteNode)
                {
                    var messages = args[0] as List<RMAosQueueMessage>;
                    if (messages != null)
                    {
                        foreach (var msg in messages)
                        {
                            auditInfo.ModifyContent.Add(Convert2AuditItem(msg));
                        }
                    }
                }

                auditInfo.Status = bool.Parse(returnValue.ToString()) ? (int)AuditStatus.Successful : (int)AuditStatus.Failed;
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
            return auditInfo;
        }

        private const string Resource_SyncNode = "RM_RN_SyncNode";
        private const string Resource_DeleteNode = "RM_RN_DeleteNode";
        private AuditItem Convert2AuditItem(RMAosQueueMessage msg)
        {
            AuditItem auditItem = new AuditItem();
            auditItem.OldValue = string.Empty;
            auditItem.NewValue = string.Empty;
            switch (msg.MessageType)
            {
                case RMAosQueueMessageType.SyncNodes:
                    auditItem.NewValue = msg.SyncNodesMessage.Content.FileLowName;
                    auditItem.TargetSetting = Resource_SyncNode;
                    break;
                case RMAosQueueMessageType.DeleteNodes:
                    auditItem.OldValue = msg.DeleteNodesMessage.Content.FileLowName;
                    auditItem.TargetSetting = Resource_DeleteNode;
                    break;
                case RMAosQueueMessageType.ExtendPhysicalDevice:
                    break;
                case RMAosQueueMessageType.SyncAOSSecurityProfile:
                    break;
                case RMAosQueueMessageType.SyncServiceAccount:
                    break;
                case RMAosQueueMessageType.ChangeTenantOwner:
                    break;
                default:
                    break;
            }
            return auditItem;
        }
    }
}
