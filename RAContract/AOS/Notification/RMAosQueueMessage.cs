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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Aos.Notification
{
    public class RMAosQueueMessage
    {
        [JsonProperty("QI")]
        public string QueueMessageId { get; set; }
        [JsonProperty("MT")]
        public RMAosQueueMessageType MessageType { get; set; }
        [JsonProperty("TGI")]
        public string TenantGroupId { get; set; }
        [JsonProperty("SBMI")]
        public string ServiceBusMessageId { get; set; }
        [JsonProperty("EPDM")]
        public ExtendPhysicalDeviceMessage ExtendPhysicalDeviceMessage { get; set; }
        [JsonProperty("SNM")]
        public SyncNodesMessage SyncNodesMessage { get; set; }
        [JsonProperty("DNM")]
        public DeleteNodesMessage DeleteNodesMessage { get; set; }
        [JsonProperty("SASPM")]
        public SyncAOSSecurityProfileMessage SyncAOSSecurityProfileMessage { get; set; }
        [JsonProperty("SSAM")]
        public SyncServiceAccountMessage SyncServiceAccountMessage { get; set; }
        [JsonProperty("RMT")]
        public long ReceiveMessageTime { get; set; }

        [JsonIgnore]
        public bool IsLastSyncJob
        {
            get
            {
                return !string.IsNullOrEmpty(SyncNodesMessage?.Content?.Office365TenantId);
            }
        }
    }

    public enum RMAosQueueMessageType
    {
        ExtendPhysicalDevice = 1,
        SyncNodes = 2,
        DeleteNodes = 3,
        SyncAOSSecurityProfile = 4,
        SyncServiceAccount = 5,
        ChangeTenantOwner = 13,
        UpdateNodes = 14,
        LastSyncMessage = 99,
        InitNodes = 100,
    }

    public enum RMSyncRemoteNodeAction
    {
        Add = 0,
        Delete = 1,
        Update = 2
    }

    public enum RMRemoteNodeSourceType
    {
        SharePointOnline = 0,
        ExchangeOnline = 1,
        OneDrive = 2
    }
}
