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

using AvePoint.GCommon.GraphAPI;
using AvePoint.RA.CommonUtil;
using ExchangeCommonWrapper;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;


namespace ExchangeUtility.Graph
{
    public class FileTabEntity
    {
        [JsonProperty("docId", NullValueHandling = NullValueHandling.Ignore)]
        public String DocId { get; set; }
        [JsonProperty("objectUrl", NullValueHandling = NullValueHandling.Ignore)]
        public String ObjectUrl { get; set; }
        [JsonProperty("fileType", NullValueHandling = NullValueHandling.Ignore)]
        public String FileType { get; set; }

        [JsonProperty("fileName", NullValueHandling = NullValueHandling.Ignore)]
        public String FileName { get; set; }

        [JsonProperty("driveId", NullValueHandling = NullValueHandling.Ignore)]
        public String DriveId { get; set; }
        [JsonProperty("isPinnedTab", NullValueHandling = NullValueHandling.Ignore)]
        public Boolean IsPinnedTab { get; set; }
    }

    public class OneNoteSubEntity
    {
        [JsonProperty(PropertyName = "objectUrl")]
        public string ObjectUrl { get; set; }

        [JsonProperty(PropertyName = "wd")]
        public string Wd { get; set; }

        [JsonProperty(PropertyName = "fileType")]
        public string FileType { get; set; }

        [JsonProperty(PropertyName = "fileId")]
        public string FileID { get; set; }

        [JsonProperty(PropertyName = "baseUrl")]
        public string BaseUrl { get; set; }
    }

    [JsonConverter(typeof(EnumConverter))]
    public enum TMChannelTabType
    {
        Other,
        OneNote,
        Sharepoint,
        SharepointPage,
        Word,
        Excel,
        PPT,
        List,
        Library,
        WebSite,
        Planner,
        Wiki,
        PDF,
        Visio,
        PowerBI,
        Stream,
        Whiteboard,
        Form,
        PowerAutomate,
        VivaEngage
    }
    public class WikiTabSetting
    {
        public String subtype { get; set; }
        public Boolean hasContent { get; set; }
        public Int32 wikiTabId { get; set; }
        public String dateAdded { get; set; }
    }
    public class WikiRestoreSetting
    {
        public WikiTabSetting settings { get; set; }
        public String name { get; set; }
        public String definitionId { get; set; }
        public String id { get; set; }
        public String directive { get; set; }
        public String type { get; set; }
        public Int32 order { get; set; }
    }

    public static class TabFactory
    {
        static readonly RALogger logger = RALogger.GetInstance(typeof(TabFactory));
        public static RestoreTab CreateTabConfig(ChannelTab channelTab, Dictionary<string, string> entityIdMapping, Dictionary<string, string> tenantIdMapping, Dictionary<string, string> urlMapping = null)
        {
            return channelTab switch
            {
                { TeamsAppId: BuiltInTabTeamAppsId.Planner } => new PlannerRestoreTab(channelTab, entityIdMapping, tenantIdMapping),
                //{ TeamsAppId: BuiltInTabTeamAppsId.WebSite } => new WebSiteRestoreTab(channelTab, urlMapping),
                //{ TeamsAppId: BuiltInTabTeamAppsId.DocumentLibrary } => new DocLibRestoreTab(channelTab, urlMapping),
                //{ TeamsAppId: var teamsAppId } when BuiltInTabTeamAppsId.IsFileTab(teamsAppId) => new FileRestoreTab(channelTab, entityIdMapping, urlMapping),
                //{ TeamsAppId: var teamsAppId } when BuiltInTabTeamAppsId.IsPowerBI(teamsAppId) => new OtherTab(channelTab),
                _ => new CommonTab(channelTab, entityIdMapping)
            };
        }

        public static ConfigurationBase CreateTabConfig(ChannelTab tempTab)
        {
            ConfigurationBase config = null;

            try
            {
                var tempConfig = JsonConvert.DeserializeObject<TeamsTabConfiguration>(tempTab.Configuration.ToString());
                config = new ConfigurationBase
                {
                    EntityId = !string.IsNullOrEmpty(tempConfig.EntityId) ? tempConfig.EntityId : string.Empty,
                    ContentUrl = !string.IsNullOrEmpty(tempConfig.ContentUrl) ? tempConfig.ContentUrl : string.Empty
                };
            }
            catch (Exception e)
            {
                logger.Error($"Failed to CreateTabConfig. ex:{e}");
            }

            return config;
        }
    }

    public class CommonTab: RestoreTab
    {
        protected static readonly RALogger logger = RALogger.GetInstance(typeof(RestoreTab));

        public Dictionary<string, string> EntityIdMapping { get; set; }

        public CommonTab(ChannelTab channelTab, Dictionary<string, string> entityIdMapping) : base(channelTab)
        {
            EntityIdMapping = entityIdMapping;
        }

        private TeamsTabConfiguration teamsTabConfiguration;
        //public ChannelTab ChannelTab { get; set; }
        public override ConfigurationBase Configuration
        {
            get
            {
                if (teamsTabConfiguration == null)
                {
                    ConvertTab();
                }
                return teamsTabConfiguration;
            }
        }

        private TMChannelTabType ConvertTabType(string tabAppId)
        {
            switch (tabAppId)
            {
                case TMConstant.WORDTABID:
                case TMConstant.TeamsAppNewId_WordTabs:
                    return TMChannelTabType.Word;
                case TMConstant.EXCELTABID:
                case TMConstant.TeamsAppNewId_ExcelTabs:
                    return TMChannelTabType.Excel;
                case TMConstant.PowerPointTABID:
                case TMConstant.TeamsAppNewId_PowerPointTabs:
                    return TMChannelTabType.PPT;
                case TMConstant.PDFTABID:
                    return TMChannelTabType.PDF;
                case TMConstant.WIKITABID:
                    return TMChannelTabType.Wiki;
                case TMConstant.ONENOTETABID:
                    return TMChannelTabType.OneNote;
                case TMConstant.LISTTABID:
                    return TMChannelTabType.List;
                case TMConstant.LIBRARYTABID:
                    return TMChannelTabType.Library;
                case TMConstant.SHAREPOINTTABID:
                    return TMChannelTabType.Sharepoint;
                case TMConstant.SHAREPOINTPAGETABID:
                    return TMChannelTabType.SharepointPage;
                case TMConstant.VISOTABID:
                case TMConstant.TeamsAppNewId_VisoTabs:
                    return TMChannelTabType.Visio;
                case TMConstant.WEBSITETABID:
                    return TMChannelTabType.WebSite;
                case TMConstant.PLANNERSITETABID:
                    return TMChannelTabType.Planner;
                case TMConstant.PowerBITABID:
                    return TMChannelTabType.PowerBI;
                case TMConstant.StreamTABID:
                    return TMChannelTabType.Stream;
                case TMConstant.WhiteboardTABID:
                    return TMChannelTabType.Whiteboard;
                case TMConstant.FormTABID:
                    return TMChannelTabType.Form;
                case TMConstant.PowerAutomateTABID:
                    return TMChannelTabType.PowerAutomate;
                case TMConstant.VivaEngageTABID:
                    return TMChannelTabType.VivaEngage;
                default:
                    return TMChannelTabType.Other;
            }
        }


        private static String RemoveOneNoteSelfUrl(String contentUrl)
        {
            var tempparameters = contentUrl.Substring(contentUrl.IndexOf('?') + 1).Split('&').ToList();
            var notebookSelfUrl = tempparameters.FirstOrDefault(sub => sub.Contains("notebookselfurl=",StringComparison.OrdinalIgnoreCase));
            if (!String.IsNullOrEmpty(notebookSelfUrl))
            {
                logger.Debug($"Start to remove onentoeSelfurl:{notebookSelfUrl}");
                return contentUrl.Replace("&" + notebookSelfUrl, "");
            }
            return contentUrl;
        }

        private static String RemoveOneNoteFileID(String contentUrl)
        {
            try
            {
                Uri uri = new Uri(contentUrl);
                string queryString = uri.Query;

                var queryParameters = HttpUtility.ParseQueryString(queryString);

                if (queryParameters.AllKeys.Contains("subEntityId"))
                {
                    var originalEntity = queryParameters.Get("subEntityId") ?? String.Empty;
                    var subEntity = JsonConvert.DeserializeObject<OneNoteSubEntity>(originalEntity);
                    logger.Info("subEntityId parameter value: " + originalEntity);
                    contentUrl = contentUrl.Replace(subEntity.FileID, "");
                    logger.Info($"Content Url after remove file ID: {contentUrl}.");
                }
                else
                {
                    logger.Info("subEntityId parameter not found");
                }
                return contentUrl;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to remove the file ID:{ex}.");
            }
            return contentUrl;
        }

        public void ConvertTab()
        {
            teamsTabConfiguration = JsonConvert.DeserializeObject<TeamsTabConfiguration>(this.ChannelTab.Configuration.ToString());

            if (teamsTabConfiguration != null)
            {
                logger.Info($"Start to convert the tab {ChannelTab.DisplayName} ContentUrl:{teamsTabConfiguration.ContentUrl}, WebsiteUrl:{teamsTabConfiguration.WebsiteUrl}");
                var teamsAppId = ChannelTab?.TeamsApp?.Id ?? ChannelTab?.TeamsAppId;
                var teamsTabType = ConvertTabType(teamsAppId);
                if (teamsAppId == TMConstant.TeamsAppNewId_WordTabs
                || teamsAppId == TMConstant.TeamsAppNewId_PowerPointTabs
                || teamsAppId == TMConstant.TeamsAppNewId_VisoTabs
                || teamsAppId == TMConstant.TeamsAppNewId_ExcelTabs)
                {
                    logger.Info($"Current tab is new type of tabs:{teamsAppId}, will convert it into oldformat of tabs.");
                    var fileEntityString = (new Uri(teamsTabConfiguration.ContentUrl).Query).Split('&').ToList();
                    var tempString = HttpUtility.UrlDecode(fileEntityString[2].Substring(12));
                    var fileTabEntity = JsonConvert.DeserializeObject<FileTabEntity>(tempString);
                    teamsTabConfiguration.EntityId = fileTabEntity.DocId;
                    fileTabEntity.DocId = null;
                    fileTabEntity.DriveId = null;
                    var newstr = JsonConvert.SerializeObject(fileTabEntity);
                    teamsTabConfiguration.ContentUrl = teamsTabConfiguration.ContentUrl.Replace(fileEntityString[2].Substring(12), HttpUtility.UrlEncode(newstr).Replace("+", "%20"));
                    //ChannelTab.TeamsAppId = ChannelTab.TeamsApp.Id;
                }

                //ChannelTab.TeamsAppId = String.Format(TMConstant.TeamTabAppUrl, teamsAppId);
                //ChannelTab.TeamsApp = ChannelTab.TeamsApp;

                if (teamsTabType == TMChannelTabType.Library)
                {
                    logger.Info($"For document library tab: {ChannelTab} will change to SharePoint tab.");
                    //ChannelTab.TeamsAppId = String.Format(TMConstant.TeamTabAppUrl, TMConstant.SHAREPOINTTABID);
                    //destTab.TeamsApp = TMGroupMapping.Instance.DestTeam.Apps.FirstOrDefault(x => x.CurrentApp.AppDefinition.TeamsAppId.EqualsIgnoreCase(TMConstant.SHAREPOINTTABID))?.CurrentApp;
                }

                if (teamsTabType.Equals(TMChannelTabType.Word)
               || teamsTabType.Equals(TMChannelTabType.Excel)
               || teamsTabType.Equals(TMChannelTabType.PPT)
               || teamsTabType.Equals(TMChannelTabType.PDF)
               || teamsTabType.Equals(TMChannelTabType.OneNote)
               || teamsTabType.Equals(TMChannelTabType.Library)
               || teamsTabType.Equals(TMChannelTabType.Visio)
               || teamsTabType.Equals(TMChannelTabType.List)
               || teamsTabType.Equals(TMChannelTabType.Sharepoint)
               || teamsTabType.Equals(TMChannelTabType.SharepointPage)
               || teamsTabType.Equals(TMChannelTabType.WebSite))
                {
                    //var destTabConfiguration = new TeamsTabConfiguration()
                    //{
                    //    EntityId = teamsTabConfiguration.EntityId,
                    //    ContentUrl = teamsTabConfiguration.ContentUrl, //ReplaceManager.Instance.ReplaceUrl(ChannelTab.Configuration.ContentUrl),
                    //    WebsiteUrl = teamsTabConfiguration.WebsiteUrl, //ReplaceManager.Instance.ReplaceUrl(ChannelTab.Configuration.WebsiteUrl),
                    //};
                    if (teamsTabType.Equals(TMChannelTabType.OneNote))
                    {
                        try
                        {
                            foreach (var kvalue in EntityIdMapping)
                            {
                                var newContentUrl = teamsTabConfiguration.ContentUrl.Replace(kvalue.Key, kvalue.Value);
                                if (!string.Equals(teamsTabConfiguration.ContentUrl, newContentUrl))
                                {
                                    logger.Info($"Replace onenote tab content url from {teamsTabConfiguration.ContentUrl} to {newContentUrl}");
                                    teamsTabConfiguration.ContentUrl = newContentUrl;
                                }
                            }

                            logger.Info($"Remove the OneNoteSelfUrl.");
                            teamsTabConfiguration.ContentUrl = RemoveOneNoteSelfUrl(teamsTabConfiguration.ContentUrl);
                            teamsTabConfiguration.ContentUrl = RemoveOneNoteFileID(teamsTabConfiguration.ContentUrl);
                            logger.Info($"The ContentUrl after remove the OneNoteSelfUrl {teamsTabConfiguration.ContentUrl}.");
                        }
                        catch (Exception e)
                        {
                            logger.Warn($"Failed to process onenote tab url. ex:{e}");
                        }
                        //if (isGccH)
                        //{
                        //    destTab.Configuration.ContentUrl = destTab.Configuration.ContentUrl?.Replace(TMConstant.OneNoteTabHost, TMConstant.OneNoteTabHostGCCH);
                        //}
                        //else
                        //{
                        //    destTab.Configuration.ContentUrl = destTab.Configuration.ContentUrl?.Replace(TMConstant.OneNoteTabHostGCCH, TMConstant.OneNoteTabHost);
                        //}
                    }
                    if (teamsTabType.Equals(TMChannelTabType.List))
                    {
                        teamsTabConfiguration.ContentUrl = teamsTabConfiguration.ContentUrl?.Replace("listurl", "listUrl");
                    }
                    //if (isGccH)
                    //{
                    //    destTab.Configuration.ContentUrl = destTab.Configuration.ContentUrl?.Replace("www.microsoft365.com", "www.office365.us").Replace("www.office.com", "www.office365.us");
                    //}
                    //else
                    //{
                    //    destTab.Configuration.ContentUrl = destTab.Configuration.ContentUrl?.Replace("www.office365.us", "www.microsoft365.com");
                    //}
                    //if (teamsTab is TMOneNoteTab noteTab)
                    //{
                    //    logger.Info($"Remove the OneNoteSelfUrl.");
                    //    destTab.Configuration.ContentUrl = RemoveOneNoteSelfUrl(destTab.Configuration.ContentUrl);
                    //    destTab.Configuration.ContentUrl = RemoveOneNoteFileID(destTab.Configuration.ContentUrl);
                    //    logger.Info($"The ContentUrl after remove the OneNoteSelfUrl {destTab.Configuration.ContentUrl.Log()}.");
                    //}
                }

            }
        }
    }


    public class RestoreTab
    {
        public RestoreTab(ChannelTab channelTab)
        {
            this.ChannelTab = channelTab;
        }
        public ChannelTab ChannelTab { get; set; }
        public virtual ConfigurationBase Configuration { get { return null; } }

    }

    public class OtherTab : RestoreTab
    {
        public OtherTab(ChannelTab channelTab) : base(channelTab)
        {
        }
        public override OtherConfiguration Configuration { get { return GenerateDefaultConfiguration(); } }
        private OtherConfiguration GenerateDefaultConfiguration()
        {
            if (string.IsNullOrEmpty(this.ChannelTab.Configuration))
                return null;
            return JsonConvert.DeserializeObject<OtherConfiguration>(this.ChannelTab.Configuration.ToString());
        }
    }

    public class PlannerRestoreTab : RestoreTab
    {
        public PlannerRestoreTab(ChannelTab channelTab, Dictionary<string, string> entityIdMapping, Dictionary<string, string> tenantIdMapping) : base(channelTab)
        {
            EntityIdMapping = entityIdMapping;
            TenantIdMapping = tenantIdMapping;
        }
        public Dictionary<string, string> EntityIdMapping { get; set; }

        public Dictionary<string, string> TenantIdMapping { get; set; }
        public override ConfigurationBase Configuration { get { return ConvertToConfiguration(); } }

        public PlannerConfiguration ConvertToConfiguration()
        {
            PlannerConfiguration plannerConfiguration = new PlannerConfiguration();
            var config = JsonConvert.DeserializeObject<PlannerConfiguration>(this.ChannelTab.Configuration.ToString());
            if (!string.IsNullOrEmpty(config?.EntityId))
            {
                var plannerId = FixPlannerEntityId(config);
                if (EntityIdMapping.ContainsKey(plannerId))
                {
                    plannerConfiguration.EntityId = config.EntityId.Replace(plannerId, EntityIdMapping[plannerId]);
                    plannerConfiguration.ContentUrl = !string.IsNullOrEmpty(config.ContentUrl) ? config.ContentUrl.Replace(plannerId, EntityIdMapping[plannerId]) : string.Empty;
                    plannerConfiguration.RemoveUrl = !string.IsNullOrEmpty(config.RemoveUrl) ? config.RemoveUrl.Replace(plannerId, EntityIdMapping[plannerId]) : string.Empty;
                    plannerConfiguration.WebsiteUrl = !string.IsNullOrEmpty(config.WebsiteUrl) ? config.WebsiteUrl.Replace(plannerId, EntityIdMapping[plannerId]) : string.Empty;
                    if (TenantIdMapping != null && TenantIdMapping.Count > 0)
                    {
                        var tenantIdMap = TenantIdMapping.First();
                        if (!string.IsNullOrEmpty(plannerConfiguration.ContentUrl)) plannerConfiguration.ContentUrl = plannerConfiguration.ContentUrl.Replace(tenantIdMap.Key, tenantIdMap.Value);
                        if (!string.IsNullOrEmpty(plannerConfiguration.RemoveUrl)) plannerConfiguration.RemoveUrl = plannerConfiguration.RemoveUrl.Replace(tenantIdMap.Key, tenantIdMap.Value);
                        if (!string.IsNullOrEmpty(plannerConfiguration.WebsiteUrl)) plannerConfiguration.WebsiteUrl = plannerConfiguration.WebsiteUrl.Replace(tenantIdMap.Key, tenantIdMap.Value);
                    }
                }
                else plannerConfiguration = config;
            }
            return plannerConfiguration;
        }

        private static string FixPlannerEntityId(ConfigurationBase configuration)
        {
            try
            {
                //"tt.c_19:9e8755e306694e90a4d6109040bf0b54@thread.tacv2_p_B86LofQCfUyfB0IsVJHEfWQAFS1f_h_1594795547274"
                if (configuration.EntityId.Contains("@thread"))
                {
                    var startIndex = configuration.EntityId.IndexOf("_p_", StringComparison.OrdinalIgnoreCase);
                    var lastIndex = configuration.EntityId.IndexOf("_h_", StringComparison.OrdinalIgnoreCase);
                    return configuration.EntityId.Remove(lastIndex).Substring(startIndex + 3);
                }
                else if (configuration.EntityId.Contains("planner.v1"))
                {
                    var startIndex = configuration.EntityId.IndexOf("_p_", StringComparison.OrdinalIgnoreCase);
                    return configuration.EntityId.Substring(startIndex + 3);
                }
                else
                {
                    return configuration.EntityId;
                }
            }
            catch (Exception ex)
            {
                //logger.Warn("Failed to extract the planner ID, try again using regular. Reason : {0}", ex.ToString());
                try
                {
                    Regex rg = new Regex("(?<=(&planId=))[.\\s\\S]*?(?=(&channelId))", RegexOptions.IgnoreCase);
                    return rg.Match(configuration.ContentUrl).Value;
                }
                catch (Exception ex2)
                {
                    //logger.Warn("Failed to extract the planner ID. Reason : {0}", ex2.ToString());
                    return configuration.EntityId;
                }
            }
        }
    }

    public class WebSiteRestoreTab : RestoreTab
    {
        public WebSiteRestoreTab(ChannelTab channelTab, Dictionary<string, string> siteUrlMapping) : base(channelTab)
        {
            this.SiteUrlMapping = siteUrlMapping;
        }

        public Dictionary<string, string> SiteUrlMapping { get; set; }
        public override ConfigurationBase Configuration { get { return ConvertToConfiguration(); } }
        public WebSiteConfiguration ConvertToConfiguration()
        {
            var webSiteConfiguration = new WebSiteConfiguration();
            var config = JsonConvert.DeserializeObject<WebSiteConfiguration>(this.ChannelTab.Configuration.ToString());
            if (config != null)
            {
                webSiteConfiguration.ContentUrl = !string.IsNullOrEmpty(config.ContentUrl) && SiteUrlMapping.Count > 0 && config.ContentUrl.Contains(SiteUrlMapping.First().Key) ?
                    config.ContentUrl.Replace(SiteUrlMapping.First().Key, SiteUrlMapping.First().Value) : config.ContentUrl;
            }
            return webSiteConfiguration;
        }
    }

    public class FileRestoreTab : RestoreTab
    {
        public FileRestoreTab(ChannelTab channelTab, Dictionary<string, string> entityIdMapping, Dictionary<string, string> siteUrlMapping) : base(channelTab)
        {
            this.EntityIdMapping = entityIdMapping;
            this.SiteUrlMapping = siteUrlMapping;
            fileConfiguration = ConvertToConfiguration();
        }
        private FileConfiguration fileConfiguration;
        public Dictionary<string, string> EntityIdMapping { get; set; }
        public Dictionary<string, string> SiteUrlMapping { get; set; }
        public override ConfigurationBase Configuration { get { return fileConfiguration; } }
        public FileConfiguration ConvertToConfiguration()
        {
            var fileConfiguration = new FileConfiguration();
            var config = JsonConvert.DeserializeObject<FileConfiguration>(this.ChannelTab.Configuration.ToString());
            if (config != null)
            {
                //由于没有doc unique id mapping，此处更新成source entity Id，SharePoint sub job中会把source entity Id更新成dest entity Id
                fileConfiguration.EntityId = config.EntityId;
                fileConfiguration.ContentUrl = !string.IsNullOrEmpty(config.ContentUrl) && SiteUrlMapping.Count > 0 && config.ContentUrl.Contains(SiteUrlMapping.First().Key) ?
                    config.ContentUrl.Replace(SiteUrlMapping.First().Key, SiteUrlMapping.First().Value) : config.ContentUrl;
            }
            return fileConfiguration;
        }
    }

    public class DocLibRestoreTab : RestoreTab
    {
        public DocLibRestoreTab(ChannelTab channelTab, Dictionary<string, string> siteUrlMapping) : base(channelTab)
        {
            this.SiteUrlMapping = siteUrlMapping;
        }

        public Dictionary<string, string> SiteUrlMapping { get; set; }
        public override ConfigurationBase Configuration { get { return ConvertToConfiguration(); } }
        public DocLibConfiguration ConvertToConfiguration()
        {
            var docLibConfiguration = new DocLibConfiguration();
            var config = JsonConvert.DeserializeObject<DocLibConfiguration>(this.ChannelTab.Configuration.ToString());
            if (config != null)
            {
                docLibConfiguration.ContentUrl = !string.IsNullOrEmpty(config.ContentUrl) && SiteUrlMapping.Count > 0 && config.ContentUrl.Contains(SiteUrlMapping.First().Key) ?
                    config.ContentUrl.Replace(SiteUrlMapping.First().Key, SiteUrlMapping.First().Value) : config.ContentUrl;
            }
            return docLibConfiguration;
        }
    }

}