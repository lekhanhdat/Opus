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

namespace Microsoft365.SharePoint.Rest.HubSite
{
    using Newtonsoft.Json;
    using System;


    [JsonObject(MemberSerialization = MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class SPHubSite : RestEntity
    {
        [JsonProperty("odata.type")]
        public string ODataType { get; set; }
        [JsonProperty("odata.etag")]
        public string ODataEtag { get; set; }

        [JsonProperty("ID", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Guid ID { get; set; }
        [JsonProperty("Title")]
        public string Title { get; set; }
        [JsonProperty("SiteId", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Guid SiteId { get; set; }
        [JsonProperty("TenantInstanceId")]
        public Guid? TenantInstanceId { get; set; }
        [JsonProperty("SiteUrl")]
        public string SiteUrl { get; set; }
        [JsonProperty("LogoUrl")]
        public string LogoUrl { get; set; }
        [JsonProperty("Description")]
        public string Description { get; set; }
        [JsonProperty("Targets")]
        public string Targets { get; set; }

        #region Undocument properties
        [JsonProperty("EnablePermissionsSync")]
        public bool? EnablePermissionsSync { get; set; }
        [JsonProperty("EnforcedECTs")]
        public string EnforcedECTs { get; set; }
        [JsonProperty("EnforcedECTsVersion")]
        public int? EnforcedECTsVersion { get; set; }
        [JsonProperty("HideNameInNavigation")]
        public bool? HideNameInNavigation { get; set; }
        [JsonProperty("ParentHubSiteId")]
        public Guid? ParentHubSiteId { get; set; }
        [JsonProperty("PermissionsSyncTag")]
        public int? PermissionsSyncTag { get; set; }
        [JsonProperty("RequiresJoinApproval")]
        public bool? RequiresJoinApproval { get; set; }
        [JsonProperty("SiteDesignId")]
        public Guid? SiteDesignId { get; set; }
        #endregion
    }
}
