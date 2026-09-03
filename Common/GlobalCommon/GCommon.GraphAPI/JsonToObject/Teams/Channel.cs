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

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Channel : EntityBase
    {
        [JsonProperty("@odata.id", NullValueHandling = NullValueHandling.Ignore)]
        public string OdataId { get; set; }

        [JsonProperty("@microsoft.graph.channelCreationMode", NullValueHandling = NullValueHandling.Ignore)]
        public string CreationMode { get; set; }

        [JsonProperty("createdDateTime", NullValueHandling = NullValueHandling.Ignore)]
        public string CreatedDateTime { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        //[JsonProperty("isFavoriteByDefault")]
        //public object IsFavoriteByDefault { get; set; }

        //[JsonProperty("email")]
        //public string Email { get; set; }

        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        [JsonProperty("webUrl")]
        public string WebUrl { get; set; }

        [JsonProperty("filesFolderWebUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string FilesFolderWebUrl { get; set; }

        /// <summary>
        /// "standard" or "private or shared"
        /// </summary>
        [JsonProperty("membershipType", NullValueHandling = NullValueHandling.Ignore)]
        public string MembershipType { get; set; }

        [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
        public OTJChannelMember[] Members { get; set; }
    }
}