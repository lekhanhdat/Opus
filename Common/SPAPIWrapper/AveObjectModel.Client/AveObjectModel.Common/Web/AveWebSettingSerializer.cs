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
    using AvePoint.Wrapper.Common;
    using AvePoint.GCommon.Contract.CodeReview;
    using AvePoint.GCommon;
    #endregion

    [AveCodeReview("2012/01/31", "Navy.Li@avepoint.com", "yanjun.wang@AvePoint.com", new string[2] { CodeReviewConstants.CHECK_LIST_ID_CO_1, CodeReviewConstants.CHECK_LIST_ID_FA_10 }, null, true)]
    internal class AveWebSettingSerializer : IAveWebSettingSerializer
    {
        private bool mKeepLookAndFeel;
        private AveWeb m_Web;
        private int mSettingTypes;
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveWebSettingSerializer));
        public AveWebSettingSerializer(AveWeb web)
        {
            m_Web = web;
        }
        private static List<string> mSearchCenter = new List<string>(new string[] { "SRCHCENTERFAST#0", 
                                                                                    "SRCHCEN#0", 
                                                                                    "SRCHCENTERLITE#0", 
                                                                                    "SRCHCENTERLITE#1" });

        public void SetBackupTypes(int settingTypes)
        {
            mSettingTypes = settingTypes;
        }

        public void SetLookAndFeelOption(bool backupLookAndFeelSettings)
        {
            mKeepLookAndFeel = backupLookAndFeelSettings;
        }

        private void OutputAccessRequestSettings(AveWebSettingInfo webSetting)
        {
            if (webSetting == null)
            {
                mLogger.Info("OutputAccessRequestSettings not executed.WebSetting in cache is null.");
                return;
            }
            StringBuilder auditLog = new StringBuilder();
            auditLog.AppendLine("OutputAccessRequestSettings before Export");
            auditLog.AppendLine($"[AllowMembersEditMembership][{webSetting.AllowMembersEditMembership}]");
            auditLog.AppendLine($"[UseAccessRequestDefault][{webSetting.UseAccessRequestDefault}]");
            auditLog.AppendLine($"[RequestAccessEmail][{webSetting.RequestAccessEmail}]");
            auditLog.AppendLine($"[MembersCanShare][{webSetting.MembersCanShare}]");
            auditLog.AppendLine($"[AccessRequestSiteDescription][{webSetting.AccessRequestSiteDescription}]");
            mLogger.Info(auditLog.ToString());
        }

        public AveWebSettingInfo GetObjectData()
        {
            AveWebSettingInfo webSettingInfo = new AveWebSettingInfo();
            webSettingInfo.SettingTypes = mSettingTypes;

            if (m_Web.DataCache.IsPropertyAvailable("HasUniqueRoleAssignments"))
            {
                webSettingInfo.HasUniqueRoleAssignments = m_Web.HasUniqueRoleAssignments;
            }

            #region Audit Settings
            if (((AveWebSettingTypes)this.mSettingTypes & AveWebSettingTypes.SiteAuditSettings) == AveWebSettingTypes.SiteAuditSettings)
            {
                if (m_Web.IsRootWeb)
                {
                    string auditLogReportStorageLocation = string.Empty;
                    try
                    {
                        var auditlogreport = m_Web.AllProperties["_auditlogreportstoragelocation"];
                        if (auditlogreport != null)
                        {
                            auditLogReportStorageLocation = m_Web.AllProperties["_auditlogreportstoragelocation"].ToString().Replace(m_Web.ServerRelativeUrl, "");
                        }
                    }
                    catch
                    {
                        auditLogReportStorageLocation = "";
                    }
                    webSettingInfo.AuditLogReportStorageLocation = auditLogReportStorageLocation;
                }
            }
            #endregion

            #region Regional Settings
            if (((AveWebSettingTypes)this.mSettingTypes & AveWebSettingTypes.SiteRegionalSettings) == AveWebSettingTypes.SiteRegionalSettings)
            {
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
                    if (m_Web.RegionalSettings.LocaleId != null)
                    {
                        webSettingInfo.LocaleId = Convert.ToUInt32(m_Web.RegionalSettings.LocaleId);
                    }
                }
            }
            #endregion

            #region Language Settings
            if (((AveWebSettingTypes)this.mSettingTypes & AveWebSettingTypes.SiteLanguageSettings) == AveWebSettingTypes.SiteLanguageSettings)
            {
                if (m_Web.DataCache.IsPropertyAvailable("SupportedUILanguageIds"))
                {
                    webSettingInfo.SupportedUICultures = m_Web.SupportedUICultures.ToList<int>();
                }
                if (m_Web.DataCache.IsPropertyAvailable("OverwriteTranslationsOnChange"))
                {
                    webSettingInfo.OverwriteTranslationsOnChange = m_Web.OverwriteTranslationsOnChange;
                }
            }
            #endregion

            #region Access Settings
            if (((AveWebSettingTypes)this.mSettingTypes & AveWebSettingTypes.SiteAccessSettings) == AveWebSettingTypes.SiteAccessSettings)
            {
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
            }
            OutputAccessRequestSettings(webSettingInfo);
            #endregion

            #region Title and Description

            if (((AveWebSettingTypes)this.mSettingTypes & AveWebSettingTypes.SiteTitleAndDescription) == AveWebSettingTypes.SiteTitleAndDescription)
            {
                if (m_Web.DataCache.IsPropertyAvailable("Title"))
                {
                    webSettingInfo.Title = m_Web.Title;
                }
                if (m_Web.DataCache.IsPropertyAvailable("Description"))
                {
                    webSettingInfo.Description = m_Web.Description;
                }

                if (m_Web.DataCache.IsPropertyAvailable(AveUserResourceConstants.TITLE_RESOUCE))
                {
                    webSettingInfo.TitleResourceInfo 
                        = m_Web.DataCache.GetProperty<Dictionary<string,string>>(AveUserResourceConstants.TITLE_RESOUCE);
                    mLogger.Info("[AVE]Export language setting web title.{0}",
                        new AveJsonSerializer().SerializeToJson
                        (webSettingInfo.TitleResourceInfo));
                }
                if (m_Web.DataCache.IsPropertyAvailable(AveUserResourceConstants.DESCRIPTION_RESOUCE))
                {
                    webSettingInfo.DescriptionResourceInfo
                        = m_Web.DataCache.GetProperty<Dictionary<string, string>>(AveUserResourceConstants.DESCRIPTION_RESOUCE);
                    mLogger.Info("[AVE]Export language setting web DescriptionResourceInfo.{0}",
                       new AveJsonSerializer().SerializeToJson
                       (webSettingInfo.DescriptionResourceInfo));
                }
            }
            #endregion

            #region Lookandfeel
            if (((AveWebSettingTypes)this.mSettingTypes & AveWebSettingTypes.SiteLookAndFeel) == AveWebSettingTypes.SiteLookAndFeel)
            {
                try
                {
                    //[Obsolete] For 10 Style site
                    //if (m_Web.Site.CompatibilityLevel == 14) 
                    //{
                    //    webSettingInfo.ThemedCssFolderUrl = m_Web.ThemedCssFolderUrl;
                    //    webSettingInfo.ThemedTemplate = m_Web.ThemedTemplate;
                    //    webSettingInfo.InheritsThemedCssFolderUrl = m_Web.InheritsThemedCssFolderUrl;
                    //    if (!string.IsNullOrEmpty(m_Web.ThemedTemplate))
                    //    {
                    //        if (m_Web.ThemedTemplate.Equals("Custom", StringComparison.Ordinal))
                    //        {
                    //            Dictionary<string, object> thmxThemeProperties = m_Web.GetThmxThemeInfo();
                    //            AveThmxTheme theme = new AveThmxTheme(m_Web.Site, thmxThemeProperties);
                    //            theme.Name = webSettingInfo.ThemedTemplate.Value;
                    //            webSettingInfo.WebTheme = GetThmxThemeProperties(theme);
                    //        }
                    //        else
                    //        {
                    //            AveWebThemeInfo webThemeInfo = new AveWebThemeInfo();
                    //            webThemeInfo.ThemeName = m_Web.ThemedTemplate;
                    //            webSettingInfo.WebTheme = webThemeInfo;
                    //        }
                    //    }
                    //}
                    if (m_Web.Site.CompatibilityLevel == 15)
                    {
                        //
                        IAveList list = GetFirstUniqueThemeWeb(m_Web);
                        AveCamlQuery query = new AveCamlQuery();
                        query.ViewXml = "<View>" +
                                            "<Query><Where>" +
                                            "<Eq><FieldRef Name='DisplayOrder'/><Value Type='Number'>0</Value></Eq>" +
                                            "</Where></Query>" +
                                        "</View>";
                        query.DatesInUtc = true;
                        IAveListItemCollection items = list.GetItems(query);
                        if (items.Count == 1)//ADO-51026
                        {
                            IAveListItem item = items[0];
                            webSettingInfo.ThemedTitle = item["Name"] as string;
                            string themeUrl = string.IsNullOrEmpty(item["ThemeUrl"] as string) ? string.Empty : item["ThemeUrl"] as string;
                            string themeFontUrl = string.IsNullOrEmpty(item["FontSchemeUrl"] as string) ? string.Empty : item["FontSchemeUrl"] as string;
                            string themeImageUrl = string.IsNullOrEmpty(item["ImageUrl"] as string) ? string.Empty : item["ImageUrl"] as string;
                            if (string.IsNullOrEmpty(themeUrl))
                            {
                                string defaultUrl = this.m_Web.Site.ServerRelativeUrl.TrimEnd('/') + "/_catalogs/theme/15/palette001.spcolor";
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
                                webSettingInfo.ThemedImageContent = GetThemeRelatedFileContent(webSettingInfo.ThemedImageUrl.Value);
                            }
                        }
                    }
                    // To Do: look and feel need to contain all the options of Change the look: Theme, Header, Navigation and Footer
                    webSettingInfo.HeaderEmphasis = (int)m_Web.HeaderEmphasis;
                    webSettingInfo.HeaderLayout = (int)m_Web.HeaderLayout;
                    webSettingInfo.MegaMenuEnabled = m_Web.MegaMenuEnabled;
                    webSettingInfo.FooterEnabled = m_Web.FooterEnabled;
                    webSettingInfo.ThemedCssFolderUrl = m_Web.ThemedCssFolderUrl;
                    AveModernThemeInfo modernThemeInfo = new AveModernThemeInfo();
                    webSettingInfo.ModernThemeInfo = modernThemeInfo;
                    if (!string.IsNullOrEmpty(m_Web.ThemedCssFolderUrl))
                    {
                        //To Do: get themed info from themedCssFolderUrl: Modern theme including themeUrl(.spcolor); Classical theme including themeUrl(.spcolor),frontSchemeUrl(.spfront) and imageUrl if applied
                        IAveWeb rootWeb = m_Web.IsRootWeb ? m_Web : m_Web.Site.RootWeb; 
                        IAveFolder themedFolder = rootWeb.GetFolder(m_Web.ThemedCssFolderUrl);
                        if (themedFolder.Exists)
                        {
                            modernThemeInfo.ThemedCssFolderUrl = m_Web.ThemedCssFolderUrl;
                            var themedColorFile = themedFolder.Files.FirstOrDefault(f => f.ServerRelativeUrl.EndsWith(".spcolor", StringComparison.OrdinalIgnoreCase));
                            if (themedColorFile != null && themedColorFile.Exists)
                            {
                                modernThemeInfo.ThemedColorUrl = themedColorFile.ServerRelativeUrl;
                                modernThemeInfo.ThemedColorContent = GetThemeRelatedFileContent(themedColorFile.ServerRelativeUrl);
                            }
                            var themedFrontFile = themedFolder.Files.FirstOrDefault(f => f.ServerRelativeUrl.EndsWith(".spfont", StringComparison.OrdinalIgnoreCase));
                            if (themedFrontFile != null && themedFrontFile.Exists)
                            {
                                modernThemeInfo.ThemedFontUrl = themedFrontFile.ServerRelativeUrl;
                                modernThemeInfo.ThemedFontContent = GetThemeRelatedFileContent(themedFrontFile.ServerRelativeUrl);
                            }
                            //themde image name format xxx.thmediamgtype eg: .themedjpg, .themedpng   
                            //Filter this format:.themecss, For example:/sites/engfolg6dnlvmita/_catalogs/theme/Themed/1E6AC29A/sps_themedforegroundimages-2A28CEDE.themedcss
                            var themedImageFile = themedFolder.Files.FirstOrDefault(f => f.ServerRelativeUrl.Substring(f.ServerRelativeUrl.LastIndexOf('.')).StartsWith(".themed", StringComparison.OrdinalIgnoreCase) && !f.ServerRelativeUrl.EndsWith(".themedcss", StringComparison.OrdinalIgnoreCase));
                            if (themedImageFile != null && themedImageFile.Exists)
                            {
                                try
                                {
                                    //SAAS-39358 If the ImageUrl in the webcatalog is empty, but the image file can be obtained by themedfolder, nothing will be done
                                    if (webSettingInfo == null || webSettingInfo.ThemedImageUrl == null || !webSettingInfo.ThemedImageUrl.IsAvailable || string.IsNullOrEmpty(webSettingInfo.ThemedImageUrl.Value))
                                    {
                                        mLogger.Warn("The image file can be obtained through themed folder, but the webcatalog does not have the image url of this image file.");
                                    }
                                    else
                                    {
                                        mLogger.Info($"Will use themed folder's imageUrl:{themedImageFile.ServerRelativeUrl} instead of webcatalog's imageurl:{webSettingInfo.ThemedImageUrl.Value}");
                                        modernThemeInfo.ThemedImageUrl = themedImageFile.ServerRelativeUrl;
                                        modernThemeInfo.ThemedImageContent = GetThemeRelatedFileContent(themedImageFile.ServerRelativeUrl);
                                    }
                                }
                                catch (Exception e)
                                {
                                    mLogger.Error("An error occured when get theme image info, error:{0}", e);
                                }
                            }
                            mLogger.Info($"Back up theme,CssFolderUrl:{modernThemeInfo.ThemedCssFolderUrl},ColorUrl:{modernThemeInfo.ThemedColorUrl},FrontFileUrl:{ modernThemeInfo.ThemedFontUrl},ImageFileUrl:{modernThemeInfo.ThemedImageUrl}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Get web:{0} theme failed.Error Message:{1}", m_Web.ServerRelativeUrl, ex.ToString());
                }
                try
                {
                    if (!(mSearchCenter.Contains(m_Web.WebTemplate) && !m_Web.AllProperties.ContainsKey("AdditionalSupportedMasterPages")))
                    {
                        if (m_Web.AllProperties.Contains("__PublishingFeatureActivated"))
                        {
                            if (m_Web.AllProperties.Contains("__InheritsCustomMasterUrl"))
                            {
                                webSettingInfo.CInheriting = bool.Parse(m_Web.AllProperties["__InheritsCustomMasterUrl"].ToString());
                            }

                            if (m_Web.AllProperties.Contains("__InheritsMasterUrl"))
                            {
                                webSettingInfo.MInheriting = bool.Parse(m_Web.AllProperties["__InheritsMasterUrl"].ToString());
                            }

                            if (m_Web.AllProperties.Contains("__InheritsAlternateCssUrl"))
                            {
                                webSettingInfo.InheritAlertCss = bool.Parse(m_Web.AllProperties["__InheritsAlternateCssUrl"].ToString());
                            }

                            webSettingInfo.InheritAlertCssUrl = m_Web.AlternateCssUrl;
                        }
                        webSettingInfo.AlternateCSSUrl = m_Web.AlternateCssUrl;
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("Get web:{0} settings failed.Error Message:{1}", m_Web.ServerRelativeUrl, ex.ToString());
                }
                if (m_Web.DataCache.IsPropertyAvailable("CustomMasterUrl"))
                {
                    webSettingInfo.CustomMasterUrl = m_Web.CustomMasterUrl;
                }
                if (m_Web.DataCache.IsPropertyAvailable("MasterUrl"))
                {
                    webSettingInfo.MasterUrl = m_Web.MasterUrl;
                }
            }
            #endregion

            #region Other Settings
            if (((AveWebSettingTypes)this.mSettingTypes & AveWebSettingTypes.SiteOtherSettings) == AveWebSettingTypes.SiteOtherSettings)
            {
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

                //if (m_Web.DataCache.IsPropertyAvailable("AlternateCssUrl"))
                //{
                //    webSettingInfo.AlternateCSSUrl = m_Web.AlternateCssUrl;
                //}

                //if (m_Web.DataCache.IsPropertyAvailable("MasterUrl"))
                //{
                //    webSettingInfo.MasterUrl = m_Web.MasterUrl;
                //}
                //if (m_Web.DataCache.IsPropertyAvailable("CustomMasterUrl"))
                //{
                //    webSettingInfo.CustomMasterUrl = m_Web.CustomMasterUrl;
                //}
                if (m_Web.DataCache.IsPropertyAvailable("RequestAccessEmail"))
                {
                    webSettingInfo.RequestAccessEmail = m_Web.RequestAccessEmail;
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
                    mLogger.Warn("Get web:{0} logo failed.Error Message:{1}", m_Web.ServerRelativeUrl, ex.ToString());
                }
                if (m_Web.DataCache.IsPropertyAvailable("IsMultilingual"))
                {
                    webSettingInfo.IsMultilingual = m_Web.IsMultilingual;
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

                #region Web MetaInfo
                try
                {
                    //用数据库取metaInfo时，得到一个byte数组，还原时用Encoding.UTF8.GetString方法获取到一个属性的字符串
                    //源端BPOS的时候，将取到的AllProperties里面的所有属性拼成一个字符串，主要是模拟成server那样的字符串，然后转换成byte数组，发给目的端。
                    Dictionary<string, object> allProperties = m_Web.DataCache.GetProperty<Dictionary<string, object>>("AllPropertiesObject");
                    string propertiesInfo = string.Empty;
                    foreach (KeyValuePair<string, object> pair in allProperties)
                    {
                        string temp = pair.Value.ToString().Replace(@"\", @"\\").Replace("\r\n", "\\r\\n");
                        propertiesInfo += pair.Key + ":" + "SW|" + temp + "\r\n";
                    }
                    byte[] properties = Encoding.UTF8.GetBytes(propertiesInfo);
                    webSettingInfo.MetaInfo = new AveRestorableProperty<byte[]>(properties);
                }
                //Modify for FxCopCustomRules
                catch (Exception ex)
                {
                    mLogger.Warn("get the web MetaInfo failed, error:{0}", ex.ToString());
                }
                #endregion
            }
            #endregion

            return webSettingInfo;
        }

        private byte[] GetThemeRelatedFileContent(string themedFileUrl)
        {
            long lengthLimit = 1 * 1024 * 1024;
            byte[] content = null;
            IAveWeb rootWeb = m_Web.IsRootWeb ? m_Web : m_Web.Site.RootWeb;
            try
            {
                var themedFile = rootWeb.GetFile(themedFileUrl);
                if (themedFile.Length > 0 && themedFile.Length <= lengthLimit)
                {
                    content = themedFile.OpenBinary();
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("get the web:{0} theme related file content:{1} failed, error:{2}", m_Web.Url, themedFileUrl, ex);
            }

            return content;
        }
        private IAveList GetFirstUniqueThemeWeb(IAveWeb web)
        {
            if (!web.IsRootWeb && mKeepLookAndFeel && web.AllProperties.ContainsKey("__InheritsThemedCssFolderUrl") && Convert.ToBoolean(web.AllProperties["__InheritsThemedCssFolderUrl"]))
            {
                return GetFirstUniqueThemeWeb(web.ParentWeb);
            }
            else
            {
                return web.GetCatalog(AveListTemplateType.DesignCatalog);
            }
        }
        private string GetThemeUrl(string combinedUrl)
        {
            return AveUrlUtility.GetServerRelativeUrl(new AveFieldUrlValue(combinedUrl).Url);
        }

        /*private AveRestorableProperty<AveWebThemeInfo> GetThmxThemeProperties(AveThmxTheme theme)
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
        }*/

        public object SetObjectData(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
