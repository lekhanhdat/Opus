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

namespace AvePoint.GCommon.GraphAPI
{
    using Newtonsoft.Json;

    public class GetConversationThreadObj : EntityBase
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        //[JsonProperty("id")]
        //public string Id { get; set; }

        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("hasAttachments")]
        public bool HasAttachments { get; set; }

        [JsonProperty("lastDeliveredDateTime")]
        public string LastDeliveredDateTime { get; set; }

        [JsonProperty("uniqueSenders")]
        public string[] UniqueSenders { get; set; }

        [JsonProperty("preview")]
        public string Preview { get; set; }

        [JsonProperty("isLocked")]
        public bool IsLocked { get; set; }
    }
}