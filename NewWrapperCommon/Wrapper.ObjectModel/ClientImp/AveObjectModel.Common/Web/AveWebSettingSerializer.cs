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



namespace AvePoint.ObjectModel.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using AvePoint.Wrapper.Common;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Compression;
    #endregion

    [AveCodeReview("2012/01/31", "Navy.Li@avepoint.com", "yanjun.wang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CO_1, CodeReviewConstants.CHECK_LIST_ID_FA_10 }, null, true)]
    internal class AveWebSettingSerializer : IAveWebSettingSerializer
    {
        private AveWeb m_Web;
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveWebSettingSerializer));
        public AveWebSettingSerializer(AveWeb web)
        {
            m_Web = web;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Obj is a part of ViewXml")]
        public AveWebSettingInfo GetObjectData()
        {
            AveWebSettingInfo webSettingInfo = new AveWebSettingInfo();
            try
            {
                if (m_Web.AllProperties.Contains("__PublishingFeatureActivated"))
                {
                    if (m_Web.AllProperties.Contains("__InheritsCustomMasterUrl"))
                    {
                        webSettingInfo.CInheriting = bool.Parse(m_Web.AllProperties["__InheritsCustomMasterUrl"].ToString());
                    }

                    webSettingInfo.CustomMasterUrl = m_Web.CustomMasterUrl;

                    if (m_Web.AllProperties.Contains("__InheritsMasterUrl"))
                    {
                        webSettingInfo.MInheriting = bool.Parse(m_Web.AllProperties["__InheritsMasterUrl"].ToString());
                    }

                    if (m_Web.AllProperties.Contains("__InheritsAlternateCssUrl"))
                    {
                        webSettingInfo.InheritAlertCss = bool.Parse(m_Web.AllProperties["__InheritsAlternateCssUrl"].ToString());
                    }

                    webSettingInfo.AlternateCSSUrl = m_Web.AlternateCssUrl;
                    webSettingInfo.InheritAlertCssUrl = m_Web.AlternateCssUrl;
                }
                webSettingInfo.MasterUrl = m_Web.MasterUrl;//ADO-75209 feature未开的话取不到，backup不下来。
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get web:{0} settings failed.Error Message:{1}.", m_Web.ServerRelativeUrl, ex.ToString());
            }
            if (m_Web.DataCache.IsPropertyAvailable("TreeViewEnabled"))
            {
                webSettingInfo.TreeViewEnabled = m_Web.TreeViewEnabled;
            }
            if (m_Web.DataCache.IsPropertyAvailable("SyndicationEnabled"))
            {
                webSettingInfo.SyndicationEnabled = m_Web.SyndicationEnabled;
            }
            if (m_Web.DataCache.IsPropertyAvailable("ParserEnabled"))
            {
                webSettingInfo.ParserEnabled = m_Web.ParserEnabled;
            }
            if (m_Web.DataCache.IsPropertyAvailable("PresenceEnabled"))
            {
                webSettingInfo.PresenceEnabled = m_Web.PresenceEnabled;
            }
            if (m_Web.DataCache.IsPropertyAvailable("ASPXPageIndexMode"))
            {
                webSettingInfo.ASPXPageIndexMode = (int)m_Web.ASPXPageIndexMode;
            }

            if (m_Web.DataCache.IsPropertyAvailable("Local"))
            {
                webSettingInfo.Locale = m_Web.Locale.LCID;
            }

            if (m_Web.DataCache.IsPropertyAvailable("Title"))
            {
                webSettingInfo.Title = m_Web.Title;
            }
            if (m_Web.DataCache.IsPropertyAvailable("Description"))
            {
                webSettingInfo.Description = m_Web.Description;
            }
            //if (m_Web.DataCache.IsPropertyAvailable("AlternateCssUrl"))
            //{
            //    webSettingInfo.AlternateCSSUrl = m_Web.AlternateCssUrl;
            //}
            if (m_Web.DataCache.IsPropertyAvailable("Language"))
            {
                webSettingInfo.Language = (int)m_Web.Language;
            }
            //if (m_Web.DataCache.IsPropertyAvailable("MasterUrl"))
            //{
            //    webSettingInfo.MasterUrl = m_Web.MasterUrl;
            //}
            //if (m_Web.DataCache.IsPropertyAvailable("CustomMasterUrl"))
            //{
            //    webSettingInfo.CustomMasterUrl = m_Web.CustomMasterUrl;
            //}
            #region Access Settings
            if (m_Web.HasUniqueRoleAssignments)
            {
                if (m_Web.DataCache.IsPropertyAvailable("RequestAccessEmail"))
                {
                    webSettingInfo.RequestAccessEmail = m_Web.RequestAccessEmail;
                }
                if (m_Web.DataCache.IsPropertyAvailable("MembersCanShare"))
                {
                    webSettingInfo.MembersCanShare = m_Web.MembersCanShare;
                }
                if (m_Web.AssociatedMemberGroup != null)
                {
                    webSettingInfo.AllowMembersEditMembership = m_Web.AssociatedMemberGroup.AllowMembersEditMembership;
                }
                if (m_Web.DataCache.IsPropertyAvailable("AccessRequestSiteDescription"))
                {
                    webSettingInfo.AccessRequestSiteDescription = m_Web.AccessRequestSiteDescription;
                }
                if (m_Web.DataCache.IsPropertyAvailable("UseAccessRequestDefault"))
                {
                    webSettingInfo.UseAccessRequestDefault = m_Web.UseAccessRequestDefault;
                }
            }
            OutputAccessRequestSettings(webSettingInfo);
            #endregion

            if (m_Web.DataCache.IsPropertyAvailable("HasUniqueRoleAssignments"))
            {
                webSettingInfo.HasUniqueRoleAssignments = m_Web.HasUniqueRoleAssignments;
            }
            if (m_Web.DataCache.IsPropertyAvailable("QuickLaunchEnabled"))
            {
                webSettingInfo.QuickLaunchEnabled = m_Web.QuickLaunchEnabled;
            }
            if (m_Web.Navigation != null)
            {
                webSettingInfo.UserSharedNav = m_Web.Navigation.UseShared;
            }
            if (m_Web.DataCache.IsPropertyAvailable("Theme"))
            {
                webSettingInfo.Theme = m_Web.Theme;
            }
            if (m_Web.DataCache.IsPropertyAvailable("AllowUnsafeUpdates"))
            {
                webSettingInfo.AllowUnsafeUpdate = m_Web.AllowUnsafeUpdates;
            }
            if (m_Web.DataCache.IsPropertyAvailable("UIVersionConfigurationEnabled"))
            {
                webSettingInfo.UiversionConfigurationEnable = m_Web.UIVersionConfigurationEnabled;
            }
            try
            {
                webSettingInfo.SiteLogoUrl = m_Web.SiteLogoUrl;
                webSettingInfo.SiteLogoDescription = m_Web.SiteLogoDescription;
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get web: {0} logo failed. Error Message: {1}.", m_Web.ServerRelativeUrl, ex.ToString());
            }
            if (m_Web.DataCache.IsPropertyAvailable("IsMultilingual"))
            {
                webSettingInfo.IsMultilingual = m_Web.IsMultilingual;
            }
            if (m_Web.DataCache.IsPropertyAvailable("OverwriteTranslationsOnChange"))
            {
                webSettingInfo.OverwriteTranslationsOnChange = m_Web.OverwriteTranslationsOnChange;
            }
            try
            {
                if (m_Web.Site.CompatibilityLevel == 14)
                {
                    webSettingInfo.ThemedCssFolderUrl = m_Web.ThemedCssFolderUrl;
                    webSettingInfo.ThemedTemplate = m_Web.ThemedTemplate;
                    webSettingInfo.InheritsThemedCssFolderUrl = m_Web.InheritsThemedCssFolderUrl;
                    if (!string.IsNullOrEmpty(m_Web.ThemedTemplate))
                    {
                        if (m_Web.ThemedTemplate.Equals("Custom", StringComparison.Ordinal))
                        {
                            Dictionary<string, object> thmxThemeProperties = m_Web.GetThmxThemeInfo();
                            AveThmxTheme theme = new AveThmxTheme(m_Web.Site, thmxThemeProperties);
                            theme.Name = webSettingInfo.ThemedTemplate.Value;
                            webSettingInfo.WebTheme = GetThmxThemeProperties(theme);
                        }
                        else
                        {
                            AveWebThemeInfo webThemeInfo = new AveWebThemeInfo();
                            webThemeInfo.ThemeName = m_Web.ThemedTemplate;
                            webSettingInfo.WebTheme = webThemeInfo;
                        }
                    }
                }
                else if (m_Web.Site.CompatibilityLevel == 15)
                {
                    IAveList list = GetFirstUniqueThemeWeb(m_Web);
                    AveCamlQuery query = new AveCamlQuery();
                    query.ViewXml = "<View>" +
                                        "<Query><Where>" +
                                        "<Eq><FieldRef Name='DisplayOrder'/><Value Type='Number'>0</Value></Eq>" +
                                        "</Where></Query>"+
                                    "</View>";
                    query.DatesInUtc = true;
                    IAveListItemCollection items = list.GetItems(query);
                    if (items.Count == 1)//ADO-51026
                    {
                        IAveListItem item = items[0];
                        webSettingInfo.ThemedTitle = item["Name"] as string;
                        webSettingInfo.ThemedCssFolderUrl = m_Web.ThemedCssFolderUrl;
                        webSettingInfo.InheritsThemedCssFolderUrl = m_Web.InheritsThemedCssFolderUrl;
                        string themeUrl = string.IsNullOrEmpty(item["ThemeUrl"] as string) ? string.Empty : item["ThemeUrl"] as string;
                        string themeFontUrl=string.IsNullOrEmpty(item["FontSchemeUrl"] as string)?string.Empty:item["FontSchemeUrl"] as string;
                        string themeImageUrl = string.IsNullOrEmpty(item["ImageUrl"] as string) ? string.Empty : item["ImageUrl"] as string;
                        if(string.IsNullOrEmpty(themeUrl))
                        {
                            string defaultUrl=this.m_Web.ServerRelativeUrl.TrimEnd('/')+"/_catalogs/theme/15/palette001.spcolor";
                            //if (m_Web.GetFile(defaultUrl).Exists)
                            //{
                                webSettingInfo.ThemedColorUrl = defaultUrl;
                            //}
                        }
                        else
                        {
                            webSettingInfo.ThemedColorUrl = GetThemeUrl(themeUrl);
                        }
                        if (!string.IsNullOrEmpty(themeFontUrl))
                        {
                            webSettingInfo.ThemedFontUrl = GetThemeUrl(themeFontUrl);
                        }
                        if (!string.IsNullOrEmpty(themeImageUrl))
                        {
                            webSettingInfo.ThemedImageUrl = GetThemeUrl(themeImageUrl);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get web:{0} theme failed.Error Message:{1}.", m_Web.ServerRelativeUrl, ex.ToString());
            }
            if (m_Web.DataCache.IsPropertyAvailable("ServerRelativeUrl"))
            {
                webSettingInfo.ServerRelativeUrl = m_Web.ServerRelativeUrl;   // back up for themecssurl
            }
            if (m_Web.RootFolder != null)
            {
                webSettingInfo.WelcomePage = m_Web.RootFolder.WelcomePage;
            }
            if (m_Web.DataCache.IsPropertyAvailable("AnonymousState"))
            {
                webSettingInfo.AnonymousState = (int)m_Web.AnonymousState;
            }
            //webSettingInfo.HiddenPages = ProcessMetaInfoForNavigation(webSettingInfo.MetaInfo);
            if (m_Web.DataCache.IsPropertyAvailable("ExcludeFromOfflineClient"))
            {
                webSettingInfo.ExcludeFromOfflineClient = m_Web.ExcludeFromOfflineClient;
            }

            if (m_Web.DataCache.IsPropertyAvailable("SupportedUILanguageIds"))
            {
                List<int> supportedUICultures = new List<int>();
                foreach (System.Globalization.CultureInfo culture in m_Web.SupportedUICultures)
                {
                    supportedUICultures.Add(culture.LCID);
                }
                webSettingInfo.SupportedUICultures = supportedUICultures;
            }

            #region backup web regional setting
            if (m_Web.RegionalSettings != null)
            {
                if (m_Web.RegionalSettings.AdjustHijriDays != null)
                {
                    webSettingInfo.AdjustHijriDays = m_Web.RegionalSettings.AdjustHijriDays;
                }
                if (m_Web.RegionalSettings.AlternateCalendarType != null)
                {
                    webSettingInfo.AltCalendarType = Convert.ToByte(m_Web.RegionalSettings.AlternateCalendarType);
                }
                if (m_Web.RegionalSettings.CalendarType != null)
                {
                    webSettingInfo.CalendarType = m_Web.RegionalSettings.CalendarType;
                }
                if (m_Web.RegionalSettings.Collation != null)
                {
                    webSettingInfo.Collation = m_Web.RegionalSettings.Collation;
                }
                if (m_Web.RegionalSettings.WorkDayStartHour != null)
                {
                    webSettingInfo.WorkDayStartHour = m_Web.RegionalSettings.WorkDayStartHour;
                }
                if (m_Web.RegionalSettings.WorkDayEndHour != null)
                {
                    webSettingInfo.WorkDayEndHour = m_Web.RegionalSettings.WorkDayEndHour;
                }
                if (m_Web.RegionalSettings.WorkDays != null)
                {
                    webSettingInfo.WorkDays = m_Web.RegionalSettings.WorkDays;
                }
                if (m_Web.RegionalSettings.Time24 != null)
                {
                    webSettingInfo.Time24 = m_Web.RegionalSettings.Time24;
                }
                if (m_Web.RegionalSettings.TimeZone != null && m_Web.RegionalSettings.TimeZone.ID != null)
                {
                    webSettingInfo.TimeZone = Convert.ToInt16(m_Web.RegionalSettings.TimeZone.ID);
                }
                if (m_Web.RegionalSettings.FirstDayOfWeek != null && m_Web.RegionalSettings.FirstWeekOfYear != null)
                {
                    webSettingInfo.CalendarViewOptions = Convert.ToByte(m_Web.RegionalSettings.FirstDayOfWeek | m_Web.RegionalSettings.FirstWeekOfYear << 3 | (m_Web.RegionalSettings.ShowWeeks ? 1 : 0) << 5);
                }
            }
            #endregion

            #region Web MetaInfo
            Dictionary<string, object> allProperties = m_Web.DataCache.GetProperty<Dictionary<string, object>>("AllPropertiesObject");
            try
            {
                //用数据库取metaInfo时，得到一个byte数组，还原时用Encoding.UTF8.GetString方法获取到一个属性的字符串
                //源端BPOS的时候，将取到的AllProperties里面的所有属性拼成一个字符串，主要是模拟成server那样的字符串，然后转换成byte数组，发给目的端。
                if (m_Web.Site.CompatibilityLevel ==14)//ADO-87353
                {
                    if (!allProperties.ContainsKey("__InheritCurrentNavigation") && !allProperties.ContainsKey("__NavigationShowSiblings"))
                    {
                        allProperties.Add("__InheritCurrentNavigation", "False");
                        allProperties.Add("__NavigationShowSiblings", "False");
                    }
                    else if (allProperties.ContainsKey("__InheritCurrentNavigation") && allProperties.ContainsKey("__NavigationShowSiblings") && ((string)allProperties["__InheritCurrentNavigation"]).Equals("True",StringComparison.OrdinalIgnoreCase) && ((string)allProperties["__NavigationShowSiblings"]).Equals("True",StringComparison.OrdinalIgnoreCase))
                    {//all true will chose the second choice .
                        allProperties["__InheritCurrentNavigation"] = "False";
                    }
                   
                }
                MetaInfoHandler infoHandler = new MetaInfoHandler();
                List<string> specialKeys = new List<string> { "docid_settings_ui", "_webnavigationsettings" };
                foreach (var key in specialKeys)
                {
                    object value;
                    if (allProperties.TryGetValue(key, out value) && value != null)
                    {
                        allProperties[key] = value.ToString().Replace("\r\n", "");
                    }
                }
                foreach (KeyValuePair<string, object> pair in allProperties)
                {
                    //和local数据库存储格式保持一致，否则Restore时无法解析。
                    var key = pair.Key.Replace(@"\", @"\\").Replace(":", @"\:");
                    var value = string.Empty;
                    if (pair.Value is string)
                    {
                        value = pair.Value.ToString().Replace(@"\", @"\\");
                    }
                    infoHandler.Add(new MetaInfoProperty(key, string.IsNullOrEmpty(value) ? pair.Value : value));
                }
                string propertiesInfo = infoHandler.ToString();
                byte[] properties = Encoding.ASCII.GetBytes(propertiesInfo);
                webSettingInfo.MetaInfo = new AveRestorableProperty<byte[]>(properties);
            }
            //Modify for FxCopCustomRules
            catch (Exception ex)
            {
                mLogger.Warn("Get the web MetaInfo failed, error:{0}.", ex.ToString());
            }

            GetWebNavigationWebAndPage(allProperties, webSettingInfo);
            #endregion

            return webSettingInfo;
        }

        private void OutputAccessRequestSettings(AveWebSettingInfo webSetting)
        {
            StringBuilder auditLog = new StringBuilder();
            auditLog.AppendLine("OutputAccessRequestSettings before Export");
            auditLog.AppendLine($"[AllowMembersEditMembership][{webSetting.AllowMembersEditMembership}]");
            auditLog.AppendLine($"[UseAccessRequestDefault][{webSetting.UseAccessRequestDefault}]");
            auditLog.AppendLine($"[RequestAccessEmail][{webSetting.RequestAccessEmail}]");
            auditLog.AppendLine($"[MembersCanShare][{webSetting.MembersCanShare}]");
            auditLog.AppendLine($"[UseAccessRequestDefault][{webSetting.AccessRequestSiteDescription}]");
            mLogger.Info(auditLog.ToString());
        }

        private void GetWebNavigationWebAndPage(Dictionary<string, object> webProperties, AveWebSettingInfo webSettingInfo)
        {
            Dictionary<Guid, string> hiddenPages = new Dictionary<Guid, string>();
            try
            {
                List<string> allExcludes = new List<string>();
                Dictionary<string, Dictionary<Guid, string>> webAndPage = new Dictionary<string, Dictionary<Guid, string>>();
                Dictionary<string, Dictionary<string, Dictionary<Guid, string>>> navigationWebAndPage = new Dictionary<string, Dictionary<string, Dictionary<Guid, string>>>();

                if (webProperties.ContainsKey("__GlobalNavigationExcludes") && webProperties["__GlobalNavigationExcludes"] is string)
                {
                    string globalNavigationExcludes = webProperties["__GlobalNavigationExcludes"] as string;
                    string[] excludes = globalNavigationExcludes.Split(';');
                    foreach (string exclude in excludes)
                    {
                        if (exclude.Trim().Length == 36)
                        {
                            allExcludes.Add(exclude);
                        }
                    }
                }
                if (webProperties.ContainsKey("__CurrentNavigationExcludes") && webProperties["__CurrentNavigationExcludes"] is string)
                {
                    string currentNavigationExcludes = webProperties["__CurrentNavigationExcludes"] as string;
                    string[] excludes = currentNavigationExcludes.Split(';');
                    foreach (string exclude in excludes)
                    {
                        if (exclude.Trim().Length == 36)
                        {
                            allExcludes.Add(exclude);
                        }
                    }
                }
                GetWebAndPage(webAndPage, "web");
                GetWebAndPage(webAndPage, "page");


                foreach (string type in webAndPage.Keys)
                {
                    foreach (Guid webPageId in webAndPage[type].Keys)
                    {
                        bool hidden = false;
                        foreach (string id in allExcludes)
                        {
                            if (webPageId.ToString() == id)
                            {
                                if (!navigationWebAndPage.ContainsKey("Hidden"))
                                {
                                    Dictionary<string, Dictionary<Guid, string>> tempWebAndPage = new Dictionary<string, Dictionary<Guid, string>>();
                                    Dictionary<Guid, string> idAndPath = new Dictionary<Guid, string>();
                                    idAndPath.Add(webPageId, webAndPage[type][webPageId]);
                                    tempWebAndPage.Add(type, idAndPath);
                                    navigationWebAndPage.Add("Hidden", tempWebAndPage);
                                }
                                else
                                {
                                    if (!navigationWebAndPage["Hidden"].ContainsKey(type))
                                    {
                                        Dictionary<Guid, string> idAndPath = new Dictionary<Guid, string>();
                                        idAndPath.Add(webPageId, webAndPage[type][webPageId]);
                                        navigationWebAndPage["Hidden"].Add(type, idAndPath);
                                    }
                                    else
                                    {
                                        navigationWebAndPage["Hidden"][type].Add(webPageId, webAndPage[type][webPageId]);
                                    }
                                }
                                hidden = true;
                                break;
                            }
                        }
                        if (!hidden)
                        {
                            if (!navigationWebAndPage.ContainsKey("Common"))
                            {
                                Dictionary<string, Dictionary<Guid, string>> tempWebAndPage = new Dictionary<string, Dictionary<Guid, string>>();
                                Dictionary<Guid, string> idAndPath = new Dictionary<Guid, string>();
                                idAndPath.Add(webPageId, webAndPage[type][webPageId]);
                                tempWebAndPage.Add(type, idAndPath);
                                navigationWebAndPage.Add("Common", tempWebAndPage);
                            }
                            else
                            {
                                if (!navigationWebAndPage["Common"].ContainsKey(type))
                                {
                                    Dictionary<Guid, string> idAndPath = new Dictionary<Guid, string>();
                                    idAndPath.Add(webPageId, webAndPage[type][webPageId]);
                                    navigationWebAndPage["Common"].Add(type, idAndPath);
                                }
                                else
                                {
                                    navigationWebAndPage["Common"][type].Add(webPageId, webAndPage[type][webPageId]);
                                }
                            }
                        }
                    }
                }
                webSettingInfo.NavigationWebAndPage = navigationWebAndPage;

            }
            catch (Exception e)
            {
                mLogger.Error("Get navigation failed. Error Message: {0}", e.ToString());
            }
        }

        private void GetWebAndPage(Dictionary<string, Dictionary<Guid, string>> websAndPages, string type)
        {
            if (!websAndPages.ContainsKey(type))
            {
                websAndPages.Add(type, new Dictionary<Guid, string>());
            }
            try
            {
                if (type.Equals("web", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (AveWeb sub in m_Web.Webs) 
                    {
                        if (!websAndPages[type].ContainsKey(sub.ID)) 
                        {
                            websAndPages[type].Add(sub.ID, sub.Url);
                        }
                    }
                }
                if (type.Equals("page", StringComparison.OrdinalIgnoreCase))
                {
                    var pageListIdStr = m_Web.AllProperties["__PagesListId"];

                    if (pageListIdStr != null)
                    {
                        Guid listId = new Guid(pageListIdStr.ToString());
                        IAveList list = m_Web.Lists[listId];
                        IAveListItemCollection items = list.Items;
                        foreach (IAveListItem item in items) 
                        {
                            string fullurl = item.File.ServerRelativeUrl.TrimStart('/');
                            if (!websAndPages[type].ContainsKey(item.UniqueId)) 
                            {
                                websAndPages[type].Add(item.UniqueId, fullurl);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                mLogger.Warn("Get web hidden navigation failed . error : {0}", ex.ToString());
            }
        }

        private bool IsTCompressedBytes(byte[] buffer)
        {
            if (buffer == null || buffer.Length < 12)
            {
                return false;
            }
            if (buffer[0] == 0xA8 && buffer[1] == 0xA9 && buffer[2] == 0x30 && buffer[3] == 0x31
                && buffer[4] == 0x0C && buffer[5] == 0x00 && buffer[6] == 0x00 && buffer[7] == 0x00)
            {
                return true;
            }
            return false;
        }

        public AveWebSettingInfo GetObjectData(AveBackupOption option)
        {
            AveWebSettingInfo webSettinginfo = GetObjectData();
            if (option.BackupRelatedTermSets)
            {
                BackupMetaDataNavigationRelativeTerm(webSettinginfo.MetaInfo.Value, webSettinginfo, option);
            }
            return webSettinginfo;
        }

        private void BackupMetaDataNavigationRelativeTerm(byte[] MetaInfo, AveWebSettingInfo webSettingInfo, AveBackupOption option)
        {
            try
            {
                var metaInfoDictionary = AveCompressedUtility.GetMetaInfoDictionary(MetaInfo);
                if (metaInfoDictionary.ContainsKey("_webnavigationsettings"))
                {
                    //
                    string navigationXml = metaInfoDictionary["_webnavigationsettings"].ToString();
                    List<AveTaxFieldInfo> taxFieldInfo = new List<AveTaxFieldInfo>();
                    QueryTaxonomyProperty(navigationXml, taxFieldInfo);

                    AveMetaDataServiceSerializer serializer = m_Web.Site.MetaDataServiceSerializer as AveMetaDataServiceSerializer;
                    webSettingInfo.MetaDataNavigationRelativeTerm = serializer.GetRelatedMetadataInfo(m_Web.Site, taxFieldInfo, option);
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Failed to get navigation related MMS. Error: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// 判断源端是否是Managed Metadata Navigation
        /// </summary>
        /// <param name="navigationXml"></param>
        /// <returns></returns>
        private void QueryTaxonomyProperty(string navigationXml, List<AveTaxFieldInfo> taxFieldInfo)
        {
            List<Guid> restoredTermSetId = new List<Guid>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(navigationXml);
            XmlNodeList taxonomyNodes = xmlDoc.SelectNodes("WebNavigationSettings/SiteMapProviderSettings/TaxonomySiteMapProviderSettings");
            if (taxonomyNodes == null)
            {
                return;//return if there are no mms relative
            }
            XmlElement xmlEle = null;
            List<Guid> cacheTermSetId = new List<Guid>();
            foreach (XmlNode taxonomyNode in taxonomyNodes)
            {

                xmlEle = taxonomyNode as XmlElement;
                Guid termStoreId = Guid.Empty;
                Guid termSetId = Guid.Empty;
                if (xmlEle.HasAttribute("TermStoreId"))
                {
                    termStoreId = new Guid(xmlEle.Attributes["TermStoreId"].Value);
                }
                if (xmlEle.HasAttribute("TermSetId"))
                {
                    termSetId = new Guid(xmlEle.Attributes["TermSetId"].Value);
                }
                if (termStoreId != Guid.Empty && !cacheTermSetId.Contains(termSetId))
                {
                    AveTaxFieldInfo taxField = new AveTaxFieldInfo();
                    taxField.SspId = termStoreId;
                    taxField.TermSetId = termSetId;
                    taxFieldInfo.Add(taxField);
                    cacheTermSetId.Add(termSetId);
                }
            }
        }

        private IAveList GetFirstUniqueThemeWeb(IAveWeb web)
        {
            if (web.IsRootWeb || !(web.AllProperties.ContainsKey("__InheritsThemedCssFolderUrl") && Convert.ToBoolean(web.AllProperties["__InheritsThemedCssFolderUrl"])))
            {
                return web.GetCatalog(AveListTemplateType.DesignCatalog);
            }
            else
            {
                return GetFirstUniqueThemeWeb(web.ParentWeb);
            }
        }

        private string GetThemeUrl(string combinedUrl)
        {
            return AveUrlUtility.GetServerRelativeUrl(new AveFieldUrlValue(combinedUrl).Url);
        }

        private AveRestorableProperty<AveWebThemeInfo> GetThmxThemeProperties(AveThmxTheme theme)
        {
            AveWebThemeInfo webTheme = new AveWebThemeInfo();
            webTheme.ThemeName = theme.Name;
            webTheme.DarkColor1 = theme.DarkColor1;
            webTheme.DarkColor2 = theme.DarkColor2;
            webTheme.LightColor1 = theme.LightColor1;
            webTheme.LightColor2 = theme.LightColor2;
            webTheme.AccentColor1 = theme.AccentColor1;
            webTheme.AccentColor2 = theme.AccentColor2;
            webTheme.AccentColor3 = theme.AccentColor3;
            webTheme.AccentColor4 = theme.AccentColor4;
            webTheme.AccentColor5 = theme.AccentColor5;
            webTheme.AccentColor6 = theme.AccentColor6;
            webTheme.HyperlinkColor = theme.HyperlinkColor;
            webTheme.FollowedHyperlinkColor = theme.FollowedHyperlinkColor;
            webTheme.MajorFont = theme.MajorFont;
            webTheme.MinorFont = theme.MinorFont;
            return webTheme;
        }

        public object SetObjectData(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
