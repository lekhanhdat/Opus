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
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.RMWeb.CP
{
    public class JobNotificationResult
    {
        [JsonProperty("profileId")]
        public int ProfileId { get; set; }

        [JsonProperty("profileName")]
        public string ProfileName { get; set; }

        [JsonProperty("profileDes")]
        public string ProfileDes { get; set; }

        [JsonProperty("profileEmailReceivers")]
        public List<ToUserInfo> ProfileEmailReceivers { get; set; }

        [JsonProperty("profileInterval")]
        public NotificationInterval ProfileInterval { get; set; }

        [JsonProperty("profileJobInfos")]
        public List<NotificationJobInfo> ProfileJobInfos { get; set; } = [];

        [JsonProperty("profileCreatedTime")]
        public string ProfileCreatedTime { get; set; }
    }
}
