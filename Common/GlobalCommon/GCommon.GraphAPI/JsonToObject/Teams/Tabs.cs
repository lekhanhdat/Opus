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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Tabs : EntityBase
    {
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }

        [JsonProperty("value")]
        public Tab[] Tab { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Tab : EntityBase
    {
        //[JsonProperty("id")]
        //public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("teamsApp")]
        public TeamsAppInfo TeamsApp { get; set; }

        [JsonProperty("sortOrderIndex")]
        public string SortOrderIndex { get; set; }

        [JsonProperty("messageId")]
        public string MessageId { get; set; }

        [JsonProperty("webUrl")]
        public string WebUrl { get; set; }

        [JsonProperty("configuration")]
        public object Configuration { get; set; }

        //public ConfigurationBase KnownConfiguration
        //{
        //    get
        //    {
        //        try
        //        {
        //            switch (this.TeamsAppId)
        //            {
        //                case BuiltInTabTeamAppsId.Planner:
        //                    return JsonConvert.DeserializeObject<PlannerConfiguration>(this.Configuration.ToString());
        //                case BuiltInTabTeamAppsId.WebSite:
        //                    return JsonConvert.DeserializeObject<WebSiteConfiguration>(this.Configuration.ToString());
        //                case BuiltInTabTeamAppsId.Forms:
        //                    return JsonConvert.DeserializeObject<FormsConfiguration>(this.Configuration.ToString());
        //                case BuiltInTabTeamAppsId.OneNote:
        //                    return JsonConvert.DeserializeObject<OneNoteConfiguration>(this.Configuration.ToString());
        //                //case BuiltInTabTeamAppsId.Stream:
        //                    //return JsonConvert.DeserializeObject<StreamConfiguration>(this.Configuration.ToString());
        //                default:
        //                    //return JsonConvert.DeserializeObject<OtherConfiguration>(this.Configuration.ToString());
        //                    return null;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return null;
        //        }
        //    }
        //}
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TeamsAppInfo : EntityBase
    {
        [JsonProperty("externalId")]
        public string ExternalId { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("distributionMethod")]
        public string DistributionMethod { get; set; }
    }


    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TabAddObj : EntityBase
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("teamsApp@odata.bind")]
        public string TeamsAppOdataBind { get; set; }

        [JsonProperty("configuration")]
        public ConfigurationBase Configuration { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TabUpdateObj : EntityBase
    {
        //[JsonProperty("id")]
        //public string Id { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("sortOrderIndex")]
        public string SortOrderIndex { get; set; }

        [JsonProperty("configuration")]
        public ConfigurationBase Configuration { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ConfigurationBase : EntityBase
    {
        [JsonProperty("entityId")]
        public string EntityId { get; set; }

        [JsonProperty("contentUrl")]
        public string ContentUrl { get; set; }

        [JsonProperty("removeUrl")]
        public string RemoveUrl { get; set; }

        [JsonProperty("websiteUrl")]
        public string WebsiteUrl { get; set; }

    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class TeamsTabConfiguration: ConfigurationBase
    {
        //[JsonProperty(PropertyName = "entityId")]
        //public string EntityId { get; set; }

        //[JsonProperty(PropertyName = "contentUrl")]
        //public string ContentUrl { get; set; }

        //[JsonProperty(PropertyName = "removeUrl")]
        //public string RemoveUrl { get; set; }

        //[JsonProperty(PropertyName = "websiteUrl")]
        //public string WebsiteUrl { get; set; }

        [JsonExtensionData(ReadData = true)]
        public IDictionary<string, object> AdditionalData { get; set; }

        [JsonProperty(PropertyName = "@odata.type")]
        public string ODataType { get; set; }

        [JsonProperty(PropertyName = "wikiTabId")]
        public int wikiTabId { get; set; }

        [JsonProperty(PropertyName = "wikiDefaultTab")]
        public bool wikiDefaultTab { get; set; }

        [JsonProperty(PropertyName = "hasContent")]
        public bool hasContent { get; set; }

        [JsonProperty(PropertyName = "isPrivateMeetingWiki")]
        public bool isPrivateMeetingWiki { get; set; }

        [JsonProperty(PropertyName = "meetingNotes")]
        public bool meetingNotes { get; set; }

        [JsonProperty(PropertyName = "scenarioName")]
        public string scenarioName { get; set; }

        public TeamsTabConfiguration()
        {
            ODataType = "microsoft.graph.teamsTabConfiguration";
        }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class PlannerConfiguration : ConfigurationBase
    {
        [JsonProperty("dateAdded")]
        public string DateAdded { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class WebSiteConfiguration : ConfigurationBase
    {
        [JsonProperty("dateAdded")]
        public string DateAdded { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class FileConfiguration : ConfigurationBase
    {
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class DocLibConfiguration : ConfigurationBase
    {
    }

    //[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    //public class StreamConfiguration : ConfigurationBase
    //{

    //}

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class FormsConfiguration : ConfigurationBase
    {
        [JsonProperty("dateAdded")]
        public string DateAdded { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class OneNoteConfiguration : ConfigurationBase
    {
        [JsonProperty("dateAdded")]
        public string DateAdded { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class OtherConfiguration : ConfigurationBase
    {
        //    [JsonProperty("hasContent")]
        //    public bool HasContent { get; set; }

        [JsonProperty("dateAdded")]
        public string DateAdded { get; set; }

        //    [JsonProperty("wikiTabId")]
        //    public int WikiTabId { get; set; }

        //    [JsonProperty("objectId")]
        //    public string ObjectId { get; set; }

        //    [JsonProperty("file")]
        //    public string File { get; set; }

        //    [JsonProperty("siteUrl")]
        //    public string SiteUrl { get; set; }

        //    [JsonProperty("libraryServerRelativeUrl")]
        //    public string LibraryServerRelativeUrl { get; set; }

        //    [JsonProperty("libraryId")]
        //    public string LibraryId { get; set; }

        //    [JsonProperty("selectedDocumentLibraryTitle")]
        //    public string SelectedDocumentLibraryTitle { get; set; }

        //    [JsonProperty("selectedSiteImageUrl")]
        //    public string SelectedSiteImageUrl { get; set; }

        //    [JsonProperty("selectedSiteTitle")]
        //    public string SelectedSiteTitle { get; set; }

        //    [JsonProperty("wikiDefaultTab")]
        //    public bool? WikiDefaultTab { get; set; }

        //    [JsonProperty("isPrivateMeetingWiki")]
        //    public bool? IsPrivateMeetingWiki { get; set; }

        //    [JsonProperty("meetingNotes")]
        //    public bool? MeetingNotes { get; set; }

        //    [JsonProperty("scenarioName")]
        //    public string ScenarioName { get; set; }
    }
}