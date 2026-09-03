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


using System.Collections;

namespace AvePoint.ObjectModel.Server13
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Utilities;
    using System.Xml;
    using System.Diagnostics.CodeAnalysis;
    using AvePoint.Wrapper.Resource.ServerAPI2010;
    #endregion

    internal class AveWebSettingSerializer : IAveWebSettingSerializer
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveWebSettingSerializer));
        private AveWeb m_Web;
        private IAveBackupRestoreQueryService m_QueryService;
        private bool backupInheritedTheme;
        private bool backupInheritedNavigation;

        public AveWebSettingSerializer(IAveBackupRestoreQueryService queryService, AveWeb web)
        {
            m_QueryService = queryService;
            m_Web = web;
        }
        public AveWebSettingInfo GetObjectData()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveWebSettingSerializer.GetObjectData"))
            {

                AveWebSettingInfo webSettingInfo = m_QueryService.GetWebSettingFromWebs(m_Web);

                //Publishing Process
                try
                {
                    if (AvePoint.Common.AveEnv.IsMoss)
                    {
                        webSettingInfo = AvePublishing.ProcessWebSettingInfo(webSettingInfo, m_Web);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, ServerAPIResource.MasterPageSettingGetFailed, m_Web == null ? string.Empty : m_Web.Url, e);
                }
                webSettingInfo.TreeViewEnabled = AveWebFlags.IsDiplayTreeViewWeb(webSettingInfo.Flags.Value);
                webSettingInfo.SyndicationEnabled = !AveWebFlags.IsDisableViaRssWeb(webSettingInfo.Flags.Value);
                webSettingInfo.ParserEnabled = !AveWebFlags.IsDocumentParseDiableWeb(webSettingInfo.Flags.Value);
                webSettingInfo.PresenceEnabled = AveWebFlags.IsDisplayUserPresenceInfoWeb(webSettingInfo.Flags.Value);
                if (AveWebFlags.IsMustNotIndexAspPageContentWeb(webSettingInfo.Flags.Value))
                {
                    webSettingInfo.ASPXPageIndexMode = 2;
                }
                else if (AveWebFlags.IsAllowAlwaysAspxIndexWeb(webSettingInfo.Flags.Value))
                {
                    webSettingInfo.ASPXPageIndexMode = 1;
                }
                else
                {
                    webSettingInfo.ASPXPageIndexMode = 0;
                }

                if (AveWebFlags.IsAutoAspxIndexModeWeb(webSettingInfo.Flags.Value))
                {
                    webSettingInfo.AllowAutomaticASPXPageIndexing = true;
                }

                webSettingInfo.HasUniqueRoleAssignments = m_Web.HasUniqueRoleAssignments;
                webSettingInfo.QuickLaunchEnabled = m_Web.QuickLaunchEnabled;
                webSettingInfo.UserSharedNav = m_Web.Navigation.UseShared;
                webSettingInfo.Theme = m_Web.Theme;
                webSettingInfo.AllowUnsafeUpdate = m_Web.AllowUnsafeUpdates;
                webSettingInfo.UiversionConfigurationEnable = m_Web.UIVersionConfigurationEnabled;
                webSettingInfo.SiteLogoUrl = m_Web.SiteLogoUrl;
                webSettingInfo.SiteLogoDescription = m_Web.SiteLogoDescription;
                webSettingInfo.Uiversion = m_Web.UIVersion;
                webSettingInfo.IsMultilingual = m_Web.IsMultilingual;
                webSettingInfo.OverwriteTranslationsOnChange = m_Web.OverwriteTranslationsOnChange;
                webSettingInfo.ThemedCssUrl = m_Web.ThemeCssUrl;
                webSettingInfo.ThemedCssFolderUrl = m_Web.ThemedCssFolderUrl;
                List<int> supportedUICultures = new List<int>();
                foreach (CultureInfo culture in m_Web.SupportedUICultures)
                {
                    supportedUICultures.Add(culture.LCID);
                }
                webSettingInfo.SupportedUICultures = supportedUICultures;
                GetThemeSetting(webSettingInfo);
                webSettingInfo.ServerRelativeUrl = m_Web.ServerRelativeUrl;   // back up for themecssurl
                webSettingInfo.WelcomePage = m_Web.RootFolder.WelcomePage;
                webSettingInfo.AnonymousState = (int)m_Web.AnonymousState;
                webSettingInfo.LastItemModifiedDate = m_Web.LastItemModifiedDate;

                ProcessMetaInfoForNavigation(webSettingInfo.MetaInfo.Value, webSettingInfo);
                webSettingInfo.ExcludeFromOfflineClient = m_Web.ExcludeFromOfflineClient;
                webSettingInfo.HideSiteContentsLink = m_Web.HideSiteContentsLink;

                return webSettingInfo;

            }

        }

        public AveWebSettingInfo GetObjectData(AveBackupOption option)
        {
            SetBackupOption(option);
            AveWebSettingInfo webSettinginfo = GetObjectData();
            if (option.BackupRelatedTermSets)
            {
                BackupMetaDataNavigationRelativeTerm(webSettinginfo.MetaInfo.Value, webSettinginfo, option);
            }
            return webSettinginfo;
        }

        private void SetBackupOption(AveBackupOption option)
        {
            backupInheritedTheme = option.BackupInheritedTheme;
            backupInheritedNavigation = option.BackupInheritedNavigation;
        }

        private void GetThemeSetting(AveWebSettingInfo webSettingInfo)
        {
            try
            {
                switch (m_Web.Site.CompatibilityLevel)
                {
                    case 14:
                        Get10ThemeSetting(webSettingInfo);
                        break;
                    case 15:
                        Get13ThemeSetting(webSettingInfo);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.WARN, ServerAPIResource.SiteThemeGetFailed,
                    m_Web == null ? string.Empty : m_Web.ThemedCssFolderUrl, e);
            }
        }

        /// <summary>
        /// SP2013下的10风格的站点按照SP2010方式备份Theme
        /// </summary>
        /// <param name="webSettingInfo"></param>
        private void Get10ThemeSetting(AveWebSettingInfo webSettingInfo)
        {
            if (!string.IsNullOrEmpty(m_Web.ThemedCssFolderUrl))
            {
                ThmxTheme theme = ThmxTheme.Open(m_Web.Web.Site, ThmxTheme.GetThemeUrlForWeb(m_Web.Web));
                webSettingInfo.ThemedTemplate = theme.Name;
                webSettingInfo.WebTheme = this.GetWebThemeProperty(theme);
                if (m_Web.AllProperties.ContainsKey("__InheritsThemedCssFolderUrl"))
                {
                    webSettingInfo.InheritsThemedCssFolderUrl = new AveRestorableProperty<bool>(bool.Parse(m_Web.AllProperties["__InheritsThemedCssFolderUrl"].ToString()));
                }
            }
            else
            {
                //使用的是默认的Theme
                webSettingInfo.WebTheme = new AveWebThemeInfo() { ThemeName = string.Empty };
            }
        }

        /// <summary>
        /// SP2013下的13风格的站点备份Current Theme Item的Metadata。
        /// </summary>
        /// <param name="webSettingInfo"></param>
        private void Get13ThemeSetting(AveWebSettingInfo webSettingInfo)
        {
            SPList catalog = GetDesignCatalogOfFirstUniqueThemeWeb(m_Web.Web);
            if (catalog != null)
            {
                SPQuery query = new SPQuery();
                query.RowLimit = 1;
                query.Query = "<Where><Eq><FieldRef Name='DisplayOrder'/><Value Type='Number'>0</Value></Eq></Where>";
                query.ViewFields = "<FieldRef Name='DisplayOrder'/><FieldRef Name='Name'/><FieldRef Name='ThemeUrl'/><FieldRef Name='FontSchemeUrl'/><FieldRef Name='ImageUrl'/>";
                query.ViewFieldsOnly = true;
                SPListItemCollection items = catalog.GetItems(query);
                if (items.Count == 1)
                {
                    SPListItem currentThemeItem = items[0];
                    webSettingInfo.ThemedTitle = currentThemeItem["Name"] as string;
                    webSettingInfo.ThemedColorUrl = GetThemeUrl(currentThemeItem["ThemeUrl"] as string);
                    webSettingInfo.ThemedFontUrl = GetThemeUrl(currentThemeItem["FontSchemeUrl"] as string);
                    webSettingInfo.ThemedImageUrl = GetThemeUrl(currentThemeItem["ImageUrl"] as string);
                    if (m_Web.AllProperties.ContainsKey("__InheritsThemedCssFolderUrl"))
                    {
                        webSettingInfo.InheritsThemedCssFolderUrl = new AveRestorableProperty<bool>(bool.Parse(m_Web.AllProperties["__InheritsThemedCssFolderUrl"].ToString()));
                    }
                    else 
                    {
                        webSettingInfo.InheritsThemedCssFolderUrl = new AveRestorableProperty<bool>(false);
                    }
                }
            }
        }

        private SPList GetDesignCatalogOfFirstUniqueThemeWeb(SPWeb web)
        {
            //在SubSite升级到SiteCollection时，如果选择选择Keep look and feel并且SubSite继承上层Theme时，获取上层第一个独立Theme的Web的DesignCatalog
            if (backupInheritedTheme && web.AllProperties.ContainsKey("__InheritsThemedCssFolderUrl") && Convert.ToBoolean(web.AllProperties["__InheritsThemedCssFolderUrl"]))
            {
                return GetDesignCatalogOfFirstUniqueThemeWeb(web.ParentWeb);//是否会有内存问题
            }
            else
            {
                return web.GetCatalog(SPListTemplateType.DesignCatalog);
            }
        }

        private string GetThemeUrl(string combinedUrl)
        {
            return AveUrlUtility.GetServerRelativeUrl(new SPFieldUrlValue(combinedUrl).Url);
        }

        private AveWebThemeInfo GetWebThemeProperty(ThmxTheme theme)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveWebSettingSerializer.GetWebThemeProperty"))
            {

                AveWebThemeInfo webTheme = new AveWebThemeInfo();
                webTheme.ThemeName = theme.Name;
                webTheme.DarkColor1 = "#" + theme.DarkColor1.DefaultColor.Name.Substring(2);
                webTheme.DarkColor2 = "#" + theme.DarkColor2.DefaultColor.Name.Substring(2);
                webTheme.LightColor1 = "#" + theme.LightColor1.DefaultColor.Name.Substring(2);
                webTheme.LightColor2 = "#" + theme.LightColor2.DefaultColor.Name.Substring(2);
                webTheme.AccentColor1 = "#" + theme.AccentColor1.DefaultColor.Name.Substring(2);
                webTheme.AccentColor2 = "#" + theme.AccentColor2.DefaultColor.Name.Substring(2);
                webTheme.AccentColor3 = "#" + theme.AccentColor3.DefaultColor.Name.Substring(2);
                webTheme.AccentColor4 = "#" + theme.AccentColor4.DefaultColor.Name.Substring(2);
                webTheme.AccentColor5 = "#" + theme.AccentColor5.DefaultColor.Name.Substring(2);
                webTheme.AccentColor6 = "#" + theme.AccentColor6.DefaultColor.Name.Substring(2);
                webTheme.HyperlinkColor = "#" + theme.HyperlinkColor.DefaultColor.Name.Substring(2);
                webTheme.FollowedHyperlinkColor = "#" + theme.FollowedHyperlinkColor.DefaultColor.Name.Substring(2);
                webTheme.MajorFont = theme.MajorFont.LatinFontFace.ToString();
                webTheme.MinorFont = theme.MinorFont.LatinFontFace.ToString();
                return webTheme;

            }

        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "webnavigationsettings")]
        private void BackupMetaDataNavigationRelativeTerm(byte[] MetaInfo, AveWebSettingInfo webSettingInfo, AveBackupOption option)
        {
            try
            {
                var metaInfoDictionary = AveCompressedUtility.GetMetaInfoDictionary(MetaInfo);
                if (metaInfoDictionary.ContainsKey("_webnavigationsettings"))
                {
                    //
                    string navigationXml = metaInfoDictionary["_webnavigationsettings"].ToString();
                    navigationXml = navigationXml.Replace("\\r\\n", " ");
                    List<AveTaxFieldInfo> taxFieldInfo = new List<AveTaxFieldInfo>();
                    QueryTaxonomyProperty(navigationXml, taxFieldInfo);

                    AveMetaDataServiceSerializer serializer = m_Web.Site.MetaDataServiceSerializer as AveMetaDataServiceSerializer;
                    webSettingInfo.MetaDataNavigationRelativeTerm = serializer.GetRelatedMetadataInfo(m_Web.Site, taxFieldInfo, option);
                }
            }
            catch (Exception ex)
            {
                logger.Log(AveLogLevel.WARN, "Failed to get navigation related MMS. Error: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// 判断源端是否是Managed Metadata Navigation
        /// </summary>
        /// <param name="navigationXml"></param>
        /// <returns></returns>
        private void QueryTaxonomyProperty(string navigationXml, List<AveTaxFieldInfo>taxFieldInfo)
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

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "This include a label of Database")]
        private void ProcessMetaInfoForNavigation(byte[] MetaInfo, AveWebSettingInfo webSettingInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveWebSettingSerializer.ProcessMetaInfoForNavigation"))
            {

                Dictionary<Guid, string> hiddenPages = new Dictionary<Guid, string>();
                try
                {
                    //将metainfo从byte数组转换为字符串。
                    string metaInfoString = GetTCompressedString(MetaInfo);

                    List<string> allExcludes = new List<string>();

                    //将metainfo转换过来的字符串转换为键值对。
                    var metaInfoDictionary = AveCompressedUtility.GetMetaInfoDictionary(metaInfoString);

                    if (this.backupInheritedNavigation &&
                        m_Web.AllProperties.ContainsKey("__InheritCurrentNavigation") && Convert.ToBoolean(m_Web.AllProperties["__InheritCurrentNavigation"]))
                    {
                        // 判断当前Web Navigation是否是继承
                        //如果是继承，找Parent
                        //取出Parent的_webnavigationsettings值
                        //用取到底值替换 metaInfoString 中当前的值
                        //把 metaInfoString转为 Byte[]
                        //webSettingInfo.MetaInfo.Value = null;
                        SPWeb web = m_Web.Web;
                        while (web.AllProperties.ContainsKey("__InheritCurrentNavigation") && Convert.ToBoolean(web.AllProperties["__InheritCurrentNavigation"]))
                        {
                            web = web.ParentWeb;
                        }

                        //进行替换string中的字符串而不是替换键值对的值
                        string beReplace = "_webnavigationsettings:SW|" + metaInfoDictionary["_webnavigationsettings"];

                        //还得对web.Allproperties进行添加\的操作
                        string property = web.AllProperties["_webnavigationsettings"].ToString().Replace("\r\n", "\\r\\n");

                        string toReplace = "_webnavigationsettings:SW|" + property;
                        string metaInfoTempString = metaInfoString.Replace(beReplace, toReplace);

                        //对字符串进行封装，改成byte数组的形式。
                        MetaInfo = Encoding.UTF8.GetBytes(metaInfoTempString);
                        webSettingInfo.MetaInfo = MetaInfo;
                    }

                    if (metaInfoDictionary.ContainsKey("__GlobalNavigationExcludes"))
                    {
                        string globalNavigationExcludes = metaInfoDictionary["__GlobalNavigationExcludes"];
                        string[] excludes = globalNavigationExcludes.Split(';');
                        foreach (string exclude in excludes)
                        {
                            if (exclude.Trim().Length == 36)
                            {
                                allExcludes.Add(exclude);
                            }
                        }
                    }
                    if (metaInfoDictionary.ContainsKey("__CurrentNavigationExcludes"))
                    {
                        string currentNavigationExcludes = metaInfoDictionary["__CurrentNavigationExcludes"];
                        string[] excludes = currentNavigationExcludes.Split(';');
                        foreach (string exclude in excludes)
                        {
                            if (exclude.Trim().Length == 36)
                            {
                                allExcludes.Add(exclude);
                            }
                        }
                    }
                    Dictionary<string, Dictionary<Guid, string>> webAndPage = new Dictionary<string, Dictionary<Guid, string>>();
                    Dictionary<string, Dictionary<string, Dictionary<Guid, string>>> navigationWebAndPage = new Dictionary<string, Dictionary<string, Dictionary<Guid, string>>>();
                    m_QueryService.GetSubWebsUrl(m_Web.Site.ID, m_Web.ID, webAndPage);

                    string pagesListUrl = string.Empty;
                    try
                    {
                        var pageListIdStr = m_Web.AllProperties["__PagesListId"];

                        if (pageListIdStr != null)
                        {
                            Guid listId = new Guid(pageListIdStr.ToString());
                            m_QueryService.GetListPagesUrl(m_Web.Site.ID, listId, webAndPage);
                        }//或者使用Folder是否存在来判断。
                        //pagesListUrl = m_Web.Url + "/Pages";
                        //Guid listId = m_Web.GetList(pagesListUrl).ID;
                        //pagesListUrl = m_Web.Url + "/Pages";
                        //Guid listId = m_Web.GetList(pagesListUrl).ID;
                        //element.Add("@SiteId", m_Web.Site.ID);
                        //element.Add("@ListId", listId);
                        //m_QueryService.GetSubWebsAndPageInfo(element, webAndPage, "page");
                    }
                    catch (Exception ex)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetWebsAndPageInfo, pagesListUrl, ex);
                    }
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
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.MetaInfoForNavigationFailed,
                        webSettingInfo == null ? string.Empty : webSettingInfo.ServerRelativeUrl, e);
                }

            }

        }

        private static string GetTCompressedString(byte[] buffer)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveWebSettingSerializer.GetTCompressedString"))
            {

                string str = string.Empty;
                if (!IsTCompressedBytes(buffer))
                {
                    //当从07升级到10的情况下，可能出现一些compress字段的值，是Unicode编码的。经过分析，如果是Unicode编码的，buffer字节数组的偶数位基本上是0x00的。
                    //在此以前四个字节的偶数位为0x00情况下，判定为Unicode编码。
                    if (buffer != null && buffer.Length > 4 && buffer[1] == 0x00 && buffer[3] == 0x00)
                    {
                        return Encoding.Unicode.GetString(buffer);
                    }
                    return Encoding.UTF8.GetString(buffer);
                }
                int len = 0;
                for (int i = 3; i >= 0; --i)
                {
                    len <<= 8;
                    len += buffer[i + 8];
                }
                byte[] temp = new byte[len];
                using (MemoryStream ms = new MemoryStream(buffer, 12, buffer.Length - 12))
                {
                    using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress))
                    {
                        ms.ReadByte();
                        ms.ReadByte();
                        ds.Read(temp, 0, len);
                        str = Encoding.UTF8.GetString(temp);
                    }
                }
                return str;

            }

        }

        private static bool IsTCompressedBytes(byte[] buffer)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveWebSettingSerializer.IsTCompressedBytes"))
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

        }

        public object SetObjectData(object obj)
        {
            return null;
        }
    }
}
