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
using AvePoint.RA.Contract.Aos.Notification;

namespace AvePoint.RA.Service.Services.Tenant.SyncNodes
{
    public class SyncNodesSettings
    {
        public SyncNodesSettings(RMAosQueueMessage queueMessage)
        {
            TenantGroupId = queueMessage.TenantGroupId;
            DocAveLicenseInfo = queueMessage.SyncNodesMessage.Content.DocAveLicenseInfo;
            O365TenantGroupId = queueMessage.SyncNodesMessage.Content.Office365TenantId;
            IsManualScan = queueMessage.SyncNodesMessage.Content.IsManualScan;
            MessageType = queueMessage.MessageType;
            IsLastSyncJob = queueMessage.IsLastSyncJob;
        }

        public string TenantGroupId { get; private set; }
        public long DocAveLicenseInfo { get; private set; }
        public string O365TenantGroupId { get; private set; }
        public bool IsManualScan { get; private set; }
        public bool IsLastSyncJob { get; private set; }

        public RMAosQueueMessageType MessageType { get; private set; }
    }
}
