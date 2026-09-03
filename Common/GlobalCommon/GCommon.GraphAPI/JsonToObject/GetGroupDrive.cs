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
    using System;

    public class GetGroupDriveObj : EntityBase
    {

        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("createdDateTime")]
        public string CreatedDateTime { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        //[JsonProperty("id")]
        //public string Id { get; set; }

        [JsonProperty("lastModifiedDateTime")]
        public string LastModifiedDateTime { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("webUrl")]
        public string WebUrl { get; set; }

        [JsonProperty("driveType")]
        public string DriveType { get; set; }

        [JsonProperty("createdBy")]
        public GGDCreatedBy CreatedBy { get; set; }

        [JsonProperty("owner")]
        public GGDOwner Owner { get; set; }

        [JsonProperty("quota")]
        public GGDQuota Quota { get; set; }
    }

    #region Sub-Object

    public class GGDOwner : EntityBase
    {

        [JsonProperty("group")]
        public Group Group { get; set; }
    }
    public class GGDGroup : EntityBase
    {

        [JsonProperty("email")]
        public string Email { get; set; }

        //[JsonProperty("id")]
        //public string Id { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }
    public class GGDQuota : EntityBase
    {

        [JsonProperty("deleted")]
        public int Deleted { get; set; }

        [JsonProperty("remaining")]
        public long Remaining { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("used")]
        public int Used { get; set; }
    }
    public class GGDCreatedBy : EntityBase
    {

        [JsonProperty("user")]
        public GGDUser User { get; set; }
    }
    public class GGDUser : EntityBase
    {

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }
    #endregion

}