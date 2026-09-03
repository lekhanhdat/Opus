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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using Newtonsoft.Json;
using System;

namespace AvePoint.RA.Contract.Aos.Notification
{
    public class SyncServiceAccountMessage
    {
        [JsonProperty("C")]
        public ServiceAccountMessage Content { get; set; }
    }

    public class ServiceAccountMessage
    {
        [JsonProperty("UN")]
        public String UserName { get; set; }
        [JsonProperty("P")]
        public String Password { get; set; }
        [JsonProperty("TI")]
        public String TenantId { get; set; }
        [JsonProperty("TN")]
        public String TenantName { get; set; }
        [JsonProperty("AU")]
        public String AdminUrl { get; set; }
        [JsonProperty("MT")]
        public ServiceAccountSyncType MessageType { get; set; }

    }
    //For Deserialize AOS Message
    public class ServiceAccountMessageModel
    {
        public String UserName { get; set; }
        public String Password { get; set; }
        public String TenantId { get; set; }
        public String TenantName { get; set; }
        public String AdminUrl { get; set; }
        public ServiceAccountSyncType MessageType { get; set; }

        public ServiceAccountMessage Convert()
        {
            return new ServiceAccountMessage()
            {
                UserName = UserName,
                Password = Password,
                TenantId = TenantId,
                TenantName = TenantName,
                AdminUrl = AdminUrl,
                MessageType = MessageType
            };
        }
    }

    public enum ServiceAccountSyncType
    {
        AddAccount = 0,
        UpdatePass = 1,
    }
}
