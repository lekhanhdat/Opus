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
    public class InstalledApps : EntityBase
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("value")]
        public InstalledApp[] InstalledApp { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class InstalledApp : EntityBase
    {
        //[JsonProperty("id")]
        //public string Id { get; set; }

        [JsonProperty("teamsAppDefinition")]
        public AppDefinition AppDefinition { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AppDefinition : EntityBase
    {
        //[JsonProperty("id")]
        //public string Id { get; set; }

        [JsonProperty("teamsAppId")]
        public string TeamsAppId { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CatalogTeamsApps : EntityBase
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("value")]
        public CatalogTeamsApp[] CatalogTeamsApp { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class CatalogTeamsApp : EntityBase
    {
        //[JsonProperty("id")]
        //public string Id { get; set; }

        [JsonProperty("externalId")]
        public object ExternalId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("distributionMethod")]
        public string DistributionMethod { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TeamsAppObj : EntityBase
    {
        [JsonProperty("teamsApp@odata.bind")]
        public string TeamsAppOdataBind { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TeamsArchiveObj : EntityBase
    {
        [JsonProperty("shouldSetSpoSiteReadOnlyForMembers")]
        public bool ShouldSetSpoSiteReadOnlyForMembers { get; set; }
    }
}