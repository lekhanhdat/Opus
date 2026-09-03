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

namespace AvePoint.RA.Contract.Aos.Notification
{
    public class RemoteNodesMessage
    {
        [JsonIgnore]
        public bool IsNewMessage => !string.IsNullOrWhiteSpace(StorageSasUri);
        [JsonProperty("SSU")]
        public string StorageSasUri { get; set; }
        [JsonProperty("SX")]
        public string StorageXri { get; set; }
        [JsonProperty("FLN")]
        public string FileLowName { get; set; }
        [JsonProperty("CT")]
        public int ConnectionType { get; set; }
        [JsonProperty("AT")]
        public int AppType { get; set; }
        [JsonProperty("IMS")]
        public bool IsManualScan { get; set; }
        [JsonProperty("DALI")]
        public long DocAveLicenseInfo { get; set; }
        [JsonProperty("OTI")]
        public string Office365TenantId { get; set; }
    }
    //For Deserialize AOS Message
    public class RemoteNodesMessageModel
    {
        public string StorageSasUri { get; set; }
        public string StorageXri { get; set; }
        public string FileLowName { get; set; }
        public string LastSyncJob { get; set; }
        public int ConnectionType { get; set; }
        public int AppType { get; set; }
        public bool IsManualScan { get; set; }
        public long DocAveLicenseInfo { get; set; }

        public RemoteNodesMessage Convert()
        {
            return new RemoteNodesMessage()
            {
                StorageSasUri = StorageSasUri,
                StorageXri = StorageXri,
                FileLowName = FileLowName,
                Office365TenantId = LastSyncJob,
                ConnectionType = ConnectionType,
                AppType = AppType,
                IsManualScan = IsManualScan,
                DocAveLicenseInfo = DocAveLicenseInfo
            };
        }
    }

    public class RemoteNodeMessage
    {
        public string NodeName { get; set; }
        public int NodeType { get; set; }
        public int NodeLevel { get; set; }
        public bool IsDeleteAll { get; set; }
    }
    public enum RemoteNodeLevel
    {
        Group = 0,
        Sites = 1,
    }

}
