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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using System.Xml;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections;
using AvePoint.GCommon;
using System.IO;
using Microsoft365.Authentication;
using System.Web;

namespace AveClientRequest.Common
{
    public class AveHttpWebRequestCommon
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveHttpWebRequestCommon));
        private ITokenProvider tokenProvider;
        private string mSiteUrl;
        private string mWebAppName;
        private string layout;
        private int mCompatibilityLevel;
        private string mInternalServerVersion;

        public AveHttpWebRequestCommon(string mSiteUrl, ITokenProvider tokenProvider, string serverVersion, string internalServerVersion)
        {
            this.mSiteUrl = mSiteUrl;
            this.tokenProvider = tokenProvider;
            if (serverVersion.StartsWith("15."))
            {
                layout = "/_layouts/15";
            }
            else
            {
                layout = "/_layouts";
            }
            mInternalServerVersion = internalServerVersion;
        }
        public int CompatiblityLevel
        {
            set
            {
                mCompatibilityLevel = value;
                if (mCompatibilityLevel == 15)
                {
                    layout = "/_layouts/15";
                }
                else
                {
                    layout = "/_layouts";
                }
            }
            get
            {
                return mCompatibilityLevel;
            }
        }

        internal string WebAppName
        {
            get
            {
                if (mWebAppName == null)
                {
                    int indexOfSlash = mSiteUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase);
                    mWebAppName = mSiteUrl;
                    if (indexOfSlash != -1)
                    {
                        mWebAppName = mSiteUrl.Substring(0, mSiteUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase));
                    }
                }
                return mWebAppName;
            }
        }
        public Dictionary<string, object> AddFeature(string webServerRelativeUrl, Guid featureId, bool force, int scope, string featuresSource)
        {
            Dictionary<string, object> featureProp = new Dictionary<string, object>();
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            string postUrl = string.Empty;
            bool request = false;//是否需要重新发送请求,用来激活Site Feature。
            switch (featuresSource)
            {
                case "web.features":
                    postUrl = webFullUrl.TrimEnd('/') + layout + "/ManageFeatures.aspx";
                    break;
                case "site.features":
                    postUrl = webFullUrl.TrimEnd('/') + layout + "/ManageFeatures.aspx?Scope=Site";
                    break;
            }
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            string featureList = AveHttpWebRequestUtility.GetFeatureTarget(html, featureId.ToString());
            int index = html.IndexOf("<div id='" + featureId.ToString() + "'", StringComparison.OrdinalIgnoreCase);
            request = index != -1 ? false : true;
            bodyDic["__EVENTTARGET"] = featureList;
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);

            if (request && featuresSource.Equals("web.features", StringComparison.OrdinalIgnoreCase))
            {
                return AddFeature(webServerRelativeUrl, featureId, force, scope, "site.features");
            }
            featureProp["DefinitionId"] = featureId;
            Dictionary<string, object> featureDefinitionProperties = new Dictionary<string, object>();
            featureProp["Definition" + AveObjectModelConstant.ObjectPropertySuffix] = featureDefinitionProperties;
            return featureProp;
        }
        public bool RestoreNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/AreaNavigationSettings.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
            bool hasEffectValue = false;
            nodes = HttpUtility.UrlEncode(nodes);
            if (!bodyDic.ContainsKey("nodes"))
            {
                bodyDic["nodes"] = nodes;
            }
            else if (bodyDic.ContainsKey("nodes") && (!bodyDic["nodes"].ToString().Equals(nodes)))
            {
                bodyDic["nodes"] = nodes;
                bodyDic["ctl00%24PlaceHolderMain%24ctl05%24RptControls%24bottomOKButton"] = "OK";
            }
            if (webAllProperties.ContainsKey("__GlobalNavigationIncludeTypes"))
            {
                if (webAllProperties["__GlobalNavigationIncludeTypes"].ToString().Equals("1"))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24globalNavSection%24ctl02%24globalIncludeSubSites"] = "on";
                }
                else if (webAllProperties["__GlobalNavigationIncludeTypes"].ToString().Equals("2"))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24globalNavSection%24ctl02%24globalIncludePages"] = "on";
                }
                else if (webAllProperties["__GlobalNavigationIncludeTypes"].ToString().Equals("3"))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24globalNavSection%24ctl02%24globalIncludeSubSites"] = "on";
                    bodyDic["ctl00%24PlaceHolderMain%24globalNavSection%24ctl02%24globalIncludePages"] = "on";
                }
                hasEffectValue = true;
            }
            if (webAllProperties.ContainsKey("__CurrentNavigationIncludeTypes"))
            {
                if (webAllProperties["__CurrentNavigationIncludeTypes"].ToString().Equals("1"))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24currentNavSection%24ctl02%24currentIncludeSubSites"] = "on";
                }
                else if (webAllProperties["__CurrentNavigationIncludeTypes"].ToString().Equals("2"))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24currentNavSection%24ctl02%24currentIncludePages"] = "on";
                }
                else if (webAllProperties["__CurrentNavigationIncludeTypes"].ToString().Equals("3"))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24currentNavSection%24ctl02%24currentIncludeSubSites"] = "on";
                    bodyDic["ctl00%24PlaceHolderMain%24currentNavSection%24ctl02%24currentIncludePages"] = "on";
                }
                hasEffectValue = true;
            }
            if (webAllProperties.ContainsKey("__GlobalDynamicChildLimit"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24globalNavSection%24ctl02%24globalDynamicChildLimit"] = webAllProperties["__GlobalDynamicChildLimit"];
            }
            if (webAllProperties.ContainsKey("__CurrentDynamicChildLimit"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24globalNavSection%24ctl02%24globalDynamicChildLimit"] = webAllProperties["__CurrentDynamicChildLimit"];
            }
            if (webAllProperties.ContainsKey("__NavigationAutomaticSortingMethod"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24ctl08%24SortingMethodRadioGroup"] = "automaticSortingRadioButton";
                if (webAllProperties.ContainsKey("__NavigationSortAscending"))
                {
                    bool sortAscending = Convert.ToBoolean(webAllProperties["__NavigationSortAscending"]);
                    if (sortAscending)
                    {
                        bodyDic["ctl00%24PlaceHolderMain%24automaticSortingSection%24SortingDirectionRadioGroup"] = "ascendingRadioButton";
                    }
                    else
                    {
                        bodyDic["ctl00%24PlaceHolderMain%24automaticSortingSection%24SortingDirectionRadioGroup"] = "descendingRadioButton";
                    }
                }
                if (webAllProperties.ContainsKey("__NavigationAutomaticSortingMethod"))
                {
                    string method = webAllProperties["__NavigationAutomaticSortingMethod"].ToString();
                    if (method.Equals("0"))
                    {
                        bodyDic["ctl00%24PlaceHolderMain%24automaticSortingSection%24automaticSortingMethodDropDown"] = "Title";
                    }
                    else if (method.Equals("1"))
                    {
                        bodyDic["ctl00%24PlaceHolderMain%24automaticSortingSection%24automaticSortingMethodDropDown"] = "CreatedDate";
                    }
                    else if (method.Equals("2"))
                    {
                        bodyDic["ctl00%24PlaceHolderMain%24automaticSortingSection%24automaticSortingMethodDropDown"] = "LastModifiedDate";
                    }
                }
            }
            else
            {
                bodyDic["ctl00%24PlaceHolderMain%24ctl08%24SortingMethodRadioGroup"] = "manualSortingRadioButton";
            }
            if (webAllProperties.ContainsKey("__DisplayShowHideRibbonActionId"))
            {
                bool ribbon = Convert.ToBoolean(webAllProperties["__DisplayShowHideRibbonActionId"]);
                if (ribbon)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl02%24DisplayShowHideRibbonActionMethodRadioGroup"] = "displayShowHideRibbonActionRadioButtonOptionYes";
                }
                else
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl02%24DisplayShowHideRibbonActionMethodRadioGroup"] = "displayShowHideRibbonActionRadioButtonOptionNo";
                }
            }
            else if (hasEffectValue)
            {
                //webservice更新navigation setting属性的时候如果该属性没有值，会还原成false，这里需要使用sharepoint默认属性赋值，确保保持一致性
                bodyDic["ctl00%24PlaceHolderMain%24ctl03%24DisplayShowHideRibbonActionMethodRadioGroup"] = "displayShowHideRibbonActionRadioButtonOptionYes";
            }
            if (webAllProperties.ContainsKey("UseShared"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24globalNavSection%24ctl02%24TopNavInheritance"] = "inheritTopNavRadioButton";
            }
            if (webAllProperties.ContainsKey("__NavigationShowSiblings"))
            {
                bool showSiblings = Convert.ToBoolean(webAllProperties["__NavigationShowSiblings"]);
                if (showSiblings)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24currentNavSection%24ctl02%24LeftNavInheritance"] = "showSiblingsLeftNavRadioButton";
                }
            }
            if (webAllProperties.ContainsKey("__InheritCurrentNavigation"))
            {
                bool inheritCurrentNavigation = Convert.ToBoolean(webAllProperties["__InheritCurrentNavigation"]);
                if (inheritCurrentNavigation)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24currentNavSection%24ctl02%24LeftNavInheritance"] = "inheritLeftNavRadioButton";
                }
            }
            bodyDic.Remove(string.Empty);
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
            return true;
        }

        public List<Dictionary<string, object>> GetListCheckedOutFiles(string webServerRelativeUrl, Guid listId)
        {
            List<Dictionary<string, object>> checkOutFileProperties = new List<Dictionary<string, object>>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/ManageCheckedOutFiles.aspx?List=" + listId + "";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            string searchContent = "class=\"ms-standardheader\"><b>Files checked out to others:</b></h3></td></tr>";
            string information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, "</table>");
            if (!string.IsNullOrEmpty(information))
            {
                information = information.Replace("< 1 KB", "LT1KB");
                AnalyzeXmltoFileInfo(information, checkOutFileProperties);
            }
            else
            {
                searchContent = "class=\"ms-standardheader\"><b>Files checked out to me:</b></h3></td></tr>";
                information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, "</table>");
                if (!string.IsNullOrEmpty(information))
                {
                    information = information.Replace("< 1 KB", "LT1KB");
                    AnalyzeXmltoFileInfo(information, checkOutFileProperties);
                }
            }
            return checkOutFileProperties;
        }

        private void AnalyzeXmltoFileInfo(string information, List<Dictionary<string, object>> checkOutFileProperties)
        {
            string trPattern = @"<tr[^>]*>[\s\S]*?<\/tr>";
            string tdPattern = @"<td[^>]*>[\s\S]*?<\/td>";
            MatchCollection trCollection = Regex.Matches(information, trPattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            for (int i = 0; i < trCollection.Count; i++)
            {
                MatchCollection tdCollection = Regex.Matches(trCollection[i].Value, tdPattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                if (tdCollection.Count > 6)
                {
                    Dictionary<string, object> fileInfo = new Dictionary<string, object>();
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(tdCollection[2].Value);
                    fileInfo["LeafName"] = doc.FirstChild.InnerText.Replace("\r\n\t\t\t", "").Replace("\r\n\t\t", "");
                    doc.LoadXml(tdCollection[3].Value);
                    fileInfo["DirName"] = doc.FirstChild.InnerText.Replace("\r\n\t\t\t", "").Replace("\r\n\t\t", "");
                    doc.LoadXml(tdCollection[4].Value);
                    fileInfo["CheckedOutByName"] = doc.FirstChild.InnerText.Replace("\r\n\t\t\t", "").Replace("\r\n\t\t", "");
                    doc.LoadXml(tdCollection[5].Value);
                    fileInfo["TimeLastModified"] = Convert.ToDateTime(doc.InnerText.Replace("\r\n\t\t\t", "").Replace("\r\n\t\t", ""));
                    doc.LoadXml(tdCollection[6].Value);
                    fileInfo["FileSize"] = doc.InnerText.Replace("\r\n\t\t\t", "").Replace("\r\n\t\t", "");
                    checkOutFileProperties.Add(fileInfo);
                }
            }
        }

        public Dictionary<string, object> GetMetadataListFieldSettings(string webServerRelativeUrl, Guid listId)
        {
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/metadatacolsettings.aspx?List={" + listId + "}";
            Dictionary<string, object> metadataListFieldSettingsProp = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            HtmlDocument htmlDom = new HtmlDocument();
            htmlDom.LoadHtml(html);
            HtmlNode enterpriseKeywords = htmlDom.DocumentNode.SelectSingleNode("//input[@name='ctl00$PlaceHolderMain$KeywordsSection$ctl01$CheckBoxEnterpriseKeywords'][@checked='checked']");
            if (enterpriseKeywords != null)
            {
                metadataListFieldSettingsProp["EnableKeywordsField"] = true;
                metadataListFieldSettingsProp["KeywordsFieldExistsInContentTypes"] = true;
            }
            enterpriseKeywords = htmlDom.DocumentNode.SelectSingleNode("//input[@name='ctl00$PlaceHolderMain$MDPushSection$ctl01$CheckBoxPromoteMetadata'][@checked='checked']");
            if (enterpriseKeywords != null)
            {
                metadataListFieldSettingsProp["EnableMetadataPromotion"] = true;
            }
            return metadataListFieldSettingsProp;
        }

        public void UpdateMetadataListFieldSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/metadatacolsettings.aspx?List={" + listId + "}";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
            //if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            //{
            //    bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            //}
            //if (bodyDic.ContainsKey("__VIEWSTATE"))
            //{
            //    bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            //}
            if (updateProperties.ContainsKey("EnableKeywordsField") && (bool)updateProperties["EnableKeywordsField"])
            {
                bodyDic["ctl00$PlaceHolderMain$KeywordsSection$ctl01$CheckBoxEnterpriseKeywords"] = "on";
            }
            if (updateProperties.ContainsKey("EnableMetadataPromotion") && (bool)updateProperties["EnableMetadataPromotion"])
            {
                bodyDic["ctl00$PlaceHolderMain$MDPushSection$ctl01$CheckBoxPromoteMetadata"] = "on";
            }
            //else//SAAS-1070
            //{
            //    bodyDic["ctl00$PlaceHolderMain$MDPushSection$ctl01$CheckBoxPromoteMetadata"] = "off";
            //}
            bodyDic["ctl00$PlaceHolderMain$ctl00$RptControls$okButton"] = "OK";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }

        public void UpdateFileProperties(string webServerRelativeUrl, string fileServerRelativeUrl, Dictionary<string, object> properties)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_vti_bin/_vti_aut/author.dll";
            string metaInfoStr = ParseMetaInfo(properties).TrimEnd(';');            
            Dictionary<string, object> body = new Dictionary<string, object>();
            body["method"] = "setDocsMetaInfo";
            body["url_list"] = HttpUtility.UrlEncode("[" + fileServerRelativeUrl.Substring(webServerRelativeUrl.Length + 1) + "]");
            body["metaInfoList"] = HttpUtility.UrlEncode("[[" + metaInfoStr + "]]");
            body["errorFlags"] = "stopOnFirst";
            body["listHiddenDocs"] = "false";
            body["listFiles"] = "false";
            body["listLinkInfo"] = "false";

            Dictionary<string, object> headerInformation = new Dictionary<string,object>();
            headerInformation["MIME-Version"] = "1.0";
            //headerInformation["User-Agent"] = "MSFrontPage/15.0";
            headerInformation["X-Vermeer-Content-Type"] = "application/x-www-form-urlencoded";

            byte[] bodyContent = AveHttpWebRequestUtility.GetByte(body, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", bodyContent, headerInformation);
        }

        private string ParseMetaInfo(Dictionary<string, object> metaInfos)
        {            
            MetaInfoHandler infoHandler = new AvePoint.Wrapper.Common.MetaInfoHandler();            
            foreach (KeyValuePair<string, object> metainfo in metaInfos)
            {
                if (metainfo.Value != null)
                {
                    infoHandler.Add(new AvePoint.Wrapper.Common.MetaInfoProperty(metainfo.Key, metainfo.Value));
                }
            }
            return infoHandler.ToUpdateString();
        }

        public bool GetListRated(string webServerRelativeUrl, Guid listId)
        {
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/RatingsSettings.aspx?List={" + listId + "}";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            string searchContent = "ctl00$PlaceHolderMain$ctl00$ctl03$EnableRatings";
            bool rating = AveHttpWebRequestUtility.GetCheckInput(html, searchContent);
            return rating;//SAAS-1064
        }

        public bool SetListRating(string webServerRelativeUrl, Guid listId, bool enableRating)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/RatingsSettings.aspx?List={" + listId + "}";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
            bodyDic.Remove("");
            if (enableRating)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl03$EnableRatings"] = "RadEnableRatingsYes";
                bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl00$ctl03$RadEnableRatingsYes";
            }
            else
            {
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl03$EnableRatings"] = "RadEnableRatingsNo";
                bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl00$ctl03$RadEnableRatingsNo";
            }
            bodyDic["__EVENTARGUMENT"] = string.Empty;
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            html = AveHttpWebRequestUtility.HttpReturn(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
            bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
            bodyDic.Remove("");
            bodyDic["ctl00$PlaceHolderMain$ctl01$RptControls$BtnSave"] = "OK";
            body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
            return true;
        }

        public Dictionary<string, object> GetListRssProperties(string webServerRelativeUrl, Guid listId)
        {
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/listsyndication.aspx?List={" + listId + "}";
            Dictionary<string, object> rssProperties = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            string searchContent = "<input id=\"ctl00_PlaceHolderMain_EnableRssSection_ctl01_EnabledTrue\"";
            bool allowListRss = AveHttpWebRequestUtility.GetCheckInput(html, searchContent);
            rssProperties["AllowRssFeeds"] = allowListRss;
            rssProperties["EnableSyndication"] = allowListRss;
            Hashtable folderProp = new Hashtable();
            XmlDocument xmlDoc = new XmlDocument();
            searchContent = "<input id=\"ctl00_PlaceHolderMain_Rss20ChannelInformationSection_ctl01_LimDescTrue\"";
            bool limitDescription = AveHttpWebRequestUtility.GetCheckInput(html, searchContent);
            if (limitDescription)
            {
                folderProp["vti_rss_LimitDescriptionLength"] = 1;
            }
            searchContent = "<input name=\"ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl02$TxtChannelTitle\"";
            string information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            if (!string.IsNullOrEmpty(information))
            {
                xmlDoc.LoadXml(information);
                folderProp["vti_rss_ChannelTitle"] = xmlDoc.FirstChild.Attributes["value"].Value;
            }
            searchContent = "<textarea name=\"ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl03$TxtChannelDescription\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</textarea>");
            if (!string.IsNullOrEmpty(information))
            {
                xmlDoc.LoadXml(information);
                folderProp["vti_rss_ChannelDescription"] = xmlDoc.FirstChild.InnerText;
            }
            searchContent = "<input name=\"ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl04$TxtChannelImageUrl\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            if (!string.IsNullOrEmpty(information))
            {
                xmlDoc.LoadXml(information);
                folderProp["vti_rss_ChannelImageUrl"] = xmlDoc.FirstChild.Attributes["value"].Value;
            }
            searchContent = "<input name=\"ctl00$PlaceHolderMain$ItemLimitSection$ctl01$TxtItemLimit\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            if (!string.IsNullOrEmpty(information))
            {
                xmlDoc.LoadXml(information);
                String s = xmlDoc.FirstChild.Attributes["value"].Value;
                if (!String.IsNullOrEmpty(s))
                {
                    folderProp["vti_rss_ItemLimit"] = Convert.ToInt32(s);
                }
            }
            searchContent = "<input name=\"ctl00$PlaceHolderMain$ItemLimitSection$ctl02$TxtDayLimit\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            if (!string.IsNullOrEmpty(information))
            {
                xmlDoc.LoadXml(information);
                String s = xmlDoc.FirstChild.Attributes["value"].Value;
                if (!String.IsNullOrEmpty(s))
                {
                    folderProp["vti_rss_DayLimit"] = Convert.ToInt32(s);
                }
            }
            searchContent = "<input id=\"ctl00_PlaceHolderMain_EnclosuresSection_ctl01_FileEnclosureTrue\"";
            bool fileEnclosure = AveHttpWebRequestUtility.GetCheckInput(html, searchContent);
            if (fileEnclosure)
            {
                folderProp["vti_rss_DocumentAsEnclosure"] = 1;
            }
            searchContent = "<input id=\"ctl00_PlaceHolderMain_EnclosuresSection_ctl02_FileLinkTrue\"";
            bool fileLink = AveHttpWebRequestUtility.GetCheckInput(html, searchContent);
            if (fileLink)
            {
                folderProp["vti_rss_DocumentAsLink"] = 1;
            }
            rssProperties["RootFolderRssProperties"] = folderProp;

            return rssProperties;
        }

        public void UpdateListRssSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProp)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/listsyndication.aspx?List={" + listId + "}";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
            bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl00$RptControls$BtnApply";
            bodyDic["ctl00$PlaceHolderMain$EnableRssSection$ctl01$Enabled"] = (bool)updateProp["AllowRss"] ? "EnabledTrue" : "EnabledFalse";
            bodyDic["ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl01$LimDesc"] = (bool)updateProp["LimitDescriptionLength"] ? "LimDescTrue" : "LimDescFalse";
            bodyDic["ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl02$TxtChannelTitle"] = updateProp["ChannelTitle"].ToString();
            bodyDic["ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl03$TxtChannelDescription"] = updateProp["ChannelDescription"].ToString();
            bodyDic["ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl04$TxtChannelImageUrl"] = updateProp["ChannelImageUrl"].ToString();
            if (updateProp.ContainsKey("DocumentAsEnclosure"))
            {
                bodyDic["ctl00$PlaceHolderMain$EnclosuresSection$ctl01$FileEnclosure"] = (bool)updateProp["DocumentAsEnclosure"] ? "FileEnclosureTrue" : "FileEnclosureFalse";
            }
            if (updateProp.ContainsKey("DocumentAsLink"))
            {
                bodyDic["ctl00$PlaceHolderMain$EnclosuresSection$ctl02$FileLink"] = (bool)updateProp["DocumentAsLink"] ? "FileLinkTrue" : "FileLinkFalse";
            }
            bodyDic["ctl00$PlaceHolderMain$ItemLimitSection$ctl01$TxtItemLimit"] = updateProp["ItemLimit"];
            bodyDic["ctl00$PlaceHolderMain$ItemLimitSection$ctl02$TxtDayLimit"] = updateProp["DayLimit"];
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }

        #region
        #region Get
        public Dictionary<string, object> GetAllFeatureDefinitions(string Url, string featuresSource)
        {
            string requestUrl = string.Empty;
            switch (featuresSource)
            {
                case "web.features":
                    requestUrl = Url.TrimEnd('/') + layout + "/ManageFeatures.aspx";
                    break;
                case "site.features":
                    requestUrl = Url.TrimEnd('/') + layout + "/ManageFeatures.aspx?Scope=Site";
                    break;
            }
            Dictionary<string, object> featureDefinitions = new Dictionary<string, object>();
            var featureDefinitionList = new List<IDictionary<string, object>>();
            string html = AveHttpWebRequestUtility.HttpGet(requestUrl, tokenProvider);

            string titleKey = "<h3 class=\"ms-standardheader\">";
            string descriptionKey = "<td class=\"ms-vb2\">";
            string idKey = "<div id='";
            string statusKey = "value=\"";
            int index = html.IndexOf(titleKey);
            while (index >= 0)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                int titleStartIndex = index + titleKey.Length;
                int titleEndIndex = html.IndexOf("</h3>", titleStartIndex);
                string title = html.Substring(titleStartIndex, titleEndIndex - titleStartIndex);

                int desStartIndex = html.IndexOf(descriptionKey, titleEndIndex) + descriptionKey.Length;
                int desEndIndex = html.IndexOf("</td>", desStartIndex);
                string description = html.Substring(desStartIndex, desEndIndex - desStartIndex);

                int idStartIndex = html.IndexOf(idKey, desEndIndex) + idKey.Length;
                int idEndIndex = html.IndexOf("</div>", idStartIndex);
                string id = html.Substring(idStartIndex, 36);

                int contentStartIndex = html.IndexOf(statusKey, idStartIndex) + statusKey.Length;
                int contentEndIndex = html.IndexOf("\"", contentStartIndex);
                string status = html.Substring(contentStartIndex, contentEndIndex - contentStartIndex);
                dic.Add("Name", title);
                dic.Add("Description", description);
                dic.Add("ID", new Guid(id));
                dic.Add("Status", status);
                if (featuresSource.Equals("site.features"))
                {
                    dic.Add("Scope", "Site");
                }
                else if (featuresSource.Equals("web.features"))
                {
                    dic.Add("Scope", "Web");
                }
                dic.Add("Hidden", false);
                dic.Add("TypeName", "Microsoft.SharePoint.Administration.SPFeatureDefinition");
                featureDefinitionList.Add(dic);
                index = html.IndexOf(titleKey, idEndIndex);
            }
            featureDefinitions.AddChildren(featureDefinitionList);
            return featureDefinitions;
        }
        public void GetWebSearchAndOfflineAvailability(string webServerRelativeUrl, Dictionary<string, object> webProp, ITokenProvider tokenProvider)
        {
            string getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/srchvis.aspx?AjaxDelta=1 ";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            if (!string.IsNullOrEmpty(html))
            {
                string radIndexSiteContent = AveHttpWebRequestUtility.GetInput(html, "ctl00_PlaceHolderMain_IndexSiteContent_ctl01_radIndexSiteContentNo", "/>");
                if (radIndexSiteContent.Contains("checked=\"checked\""))
                {
                    webProp["NoCrawl"] = true;
                }
                else
                {
                    webProp["NoCrawl"] = false;
                }
                string radIndexAspxContent = AveHttpWebRequestUtility.GetInput(html, "ctl00_PlaceHolderMain_IndexAspxContent_IndexAspxContentControl_radIndexAspxContentAuto", "/>");
                if (radIndexAspxContent.Contains("checked=\"checked\""))
                {
                    webProp["ASPXPageIndexMode"] = 0;
                }
                else
                {
                    radIndexAspxContent = AveHttpWebRequestUtility.GetInput(html, "ctl00_PlaceHolderMain_IndexAspxContent_IndexAspxContentControl_radIndexAspxContentForce", "/>");
                    if (radIndexAspxContent.Contains("checked=\"checked\""))
                    {
                        webProp["ASPXPageIndexMode"] = 1;
                    }
                    else
                    {
                        webProp["ASPXPageIndexMode"] = 2;
                    }
                }
                string allowSync = AveHttpWebRequestUtility.GetInput(html, "ctl00_PlaceHolderMain_AllowSyncSection_ctl01_RadAllowSyncYes", "/>");
                if (allowSync.Contains("checked=\"checked\""))
                {
                    webProp["ExcludeFromOfflineClient"] = false;
                }
                else
                {
                    webProp["ExcludeFromOfflineClient"] = true;
                }
            }
        }
        public Dictionary<string, object> GetWebLogoProperties(string webServerRelativeUrl)
        {
            string getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/prjsetng.aspx?AjaxDelta=1 ";
            Dictionary<string, object> webLogoProp = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            string searContent = "<input name=\"ctl00$PlaceHolderMain$logoSection$ctl03$TxtSiteLogoUrl\"";//ctl00$PlaceHolderMain$logoSection$ctl03$TxtSiteLogoUrl
            string infomation = AveHttpWebRequestUtility.GetInput(html, searContent, "/>");
            XmlDocument xmlDoc = new XmlDocument();
            if (!string.IsNullOrEmpty(infomation))
            {
                xmlDoc.LoadXml(infomation);
                webLogoProp["SiteLogoUrl"] = xmlDoc.FirstChild.Attributes["value"] != null ? xmlDoc.FirstChild.Attributes["value"].Value : default(string);
            }
            else
            {
                webLogoProp["SiteLogoUrl"] = default(string);
            }
            searContent = "<textarea name=\"ctl00$PlaceHolderMain$logoSection$ctl04$TxtLogoUrlDescription\"";
            infomation = AveHttpWebRequestUtility.GetInput(html, searContent, "</textarea>");
            if (!string.IsNullOrEmpty(infomation))
            {
                xmlDoc.LoadXml(infomation);
                webLogoProp["SiteLogoDescription"] = xmlDoc.FirstChild.InnerText;
            }
            else
            {
                webLogoProp["SiteLogoDescription"] = default(string);
            }
            return webLogoProp;
        }
        public Dictionary<string, object> GetMetadataNavigationSettings(string webServerRelativeUrl, Guid listId, string listTitle)
        {
            Dictionary<string, object> metadataNavigationSettings = new Dictionary<string, object>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/MetaNavSettings.aspx?List={" + listId + "}";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            string searchContent = "<input id=\"ctl00_PlaceHolderMain_ctl02_autoIndexingYesRadioButton\"";
            string information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            if (information.Contains("checked=\"checked\""))
            {
                metadataNavigationSettings["AutomaticallyManageListIndexing"] = true;
            }
            Dictionary<string, List<string[]>> FieldsProp = new Dictionary<string, List<string[]>>();
            XmlDocument xmlDoc = new XmlDocument();
            searchContent = "<input id=\"ctl00_PlaceHolderMain_ctl00_groupedHierarchyPicker_data\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</input>");
            xmlDoc.LoadXml(information);
            SetAvailableFields(FieldsProp, xmlDoc.FirstChild.Attributes["value"].Value, "AvailableHierarchyFields");

            searchContent = "<input id=\"ctl00_PlaceHolderMain_ctl00_groupedHierarchyPicker_initial\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</input>");
            xmlDoc.LoadXml(information);
            SetSelectedFields(FieldsProp, xmlDoc.FirstChild.Attributes["value"].Value, "SelectedHierarchyFields");

            searchContent = "<input id=\"ctl00_PlaceHolderMain_ctl01_groupedKeyFilterPicker_data\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</input>");
            xmlDoc.LoadXml(information);
            SetAvailableFields(FieldsProp, xmlDoc.FirstChild.Attributes["value"].Value, "AvailableKeyFilterFields");

            searchContent = "<input id=\"ctl00_PlaceHolderMain_ctl01_groupedKeyFilterPicker_initial\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</input>");
            xmlDoc.LoadXml(information);
            SetSelectedFields(FieldsProp, xmlDoc.FirstChild.Attributes["value"].Value, "SelectedKeyFilterFields");

            metadataNavigationSettings.Add("MetadataNavigationSettings", FieldsProp);
            metadataNavigationSettings["BPOSS"] = true;

            return metadataNavigationSettings;
        }
        public Dictionary<string, object> GetDefaultRegionalSetting(string webServerRelativeUrl, int lcid)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/regionalsetng.aspx";
            Dictionary<string, object> defaultRegionalProp = new Dictionary<string, object>();
            defaultRegionalProp["LocaleId"] = lcid;
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = System.Web.HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = System.Web.HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["ctl00%24PlaceHolderMain%24ctl02%24ctl01%24DdlwebLCID"] = lcid;
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl02%24ctl01%24DdlwebLCID";
            bodyDic["Cmd"] = "UPDATEPROJECT";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            string defaultHtml = AveHttpWebRequestUtility.HttpReturn(postUrl, tokenProvider, "application/x-www-form-urlencoded", data, null);
            if (!string.IsNullOrEmpty(defaultHtml))
            {
                string sortOrder = AveHttpWebRequestUtility.GetSelectInputValue(defaultHtml, "<select name=\"ctl00$PlaceHolderMain$ctl09$ctl01$DdlwebCollation\" id=\"ctl00_PlaceHolderMain_ctl09_ctl01_DdlwebCollation\">");
                defaultRegionalProp["Collation"] = int.Parse(sortOrder);
                string setCalendar = AveHttpWebRequestUtility.GetSelectInputValue(defaultHtml, "<select name=\"ctl00$PlaceHolderMain$ctl03$ctl01$DdlwebCalType\" id=\"ctl00_PlaceHolderMain_ctl03_ctl01_DdlwebCalType\">");
                defaultRegionalProp["CalendarType"] = int.Parse(setCalendar);
                string timeForamt = AveHttpWebRequestUtility.GetSelectInputValue(defaultHtml, "<select name=\"ctl00$PlaceHolderMain$ctl10$ctl01$DdlTimeFormat\" id=\"ctl00_PlaceHolderMain_ctl10_ctl01_DdlTimeFormat\">");
                if (!string.IsNullOrEmpty(timeForamt) && timeForamt.Equals("0"))
                {
                    defaultRegionalProp["Time24"] = false;
                }
                else
                {
                    defaultRegionalProp["Time24"] = true;
                }
            }

            return defaultRegionalProp;
        }
        public List<Dictionary<string, object>> GetKeyWords()
        {
            string getUrl = mSiteUrl.TrimEnd('/') + layout + "/listkeywords.aspx";
            List<Dictionary<string, object>> keyWordsProp = new List<Dictionary<string, object>>();
            this.GetKeyWords(getUrl, keyWordsProp);
            return keyWordsProp;
        }
        private void GetKeyWords(string getUrl, List<Dictionary<string, object>> keyWordsProp)
        {
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            List<string> keyWordsNames = new List<string>();
            string searchContent = "Keyword.aspx?k=";
            AveHttpWebRequestUtility.GetInput(html, searchContent, "\">", keyWordsNames);
            foreach (string keyWordName in keyWordsNames)
            {
                Dictionary<string, object> keyWordProp = GetKeyWordProperties(keyWordName);
                keyWordsProp.Add(keyWordProp);
            }
            string str = AveHttpWebRequestUtility.GetInput(html, "<a  id=\"ctl00$PlaceHolderMain$ctl00$SpecialTermList_pnext_i", ">");
            if (!string.IsNullOrEmpty(str))
            {
                StringBuilder nextUrl = new StringBuilder();
                nextUrl.Append(this.WebAppName.TrimEnd('/'));
                nextUrl.Append('/');
                nextUrl.Append(AveHttpWebRequestUtility.GetValue(str, "href=\""));
                this.GetKeyWords(nextUrl.ToString(), keyWordsProp);
            }
        }
        public Dictionary<string, object> GetKeyWordProperties(string keyWordName)//改成公共的是方便AveWebservice的upadteKeyWords调用
        {
            string getUrl = mSiteUrl.TrimEnd('/') + layout + "/Keyword.aspx?k=" + keyWordName;
            Dictionary<string, object> keyWordProp = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            GetKeyWordProperties(keyWordName, html, keyWordProp);
            return keyWordProp;
        }
        private void GetKeyWordProperties(string keyWordName, string html, Dictionary<string, object> keyWordProp)
        {
            string searchContent = "<input name=\"ctl00$PlaceHolderMain$nameTextBox\"";
            string information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(information);
            keyWordProp["Term"] = xmlDoc.FirstChild.Attributes["value"].Value;
            searchContent = "<input name=\"ctl00$PlaceHolderMain$synTextBox\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            xmlDoc.LoadXml(information);
            keyWordProp["Synonyms" + AveObjectModelConstant.ObjectPropertySuffix] = xmlDoc.FirstChild.Attributes["value"] != null ? xmlDoc.FirstChild.Attributes["value"].Value : string.Empty;
            searchContent = "<textarea name=\"ctl00$PlaceHolderMain$definitionTextBox\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</textarea>");
            xmlDoc.LoadXml(information);
            keyWordProp["Definition"] = xmlDoc.FirstChild.InnerText;
            searchContent = "<input name=\"ctl00$PlaceHolderMain$userPicker$HiddenEntityDisplayText\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            xmlDoc.LoadXml(information);
            keyWordProp["Contact"] = xmlDoc.FirstChild.Attributes["value"] != null ? xmlDoc.FirstChild.Attributes["value"].Value : string.Empty;
            searchContent = "<a href=\"#\" onclick='clickDatePicker(\"ctl00_PlaceHolderMain_startDate_startDateDate\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/a>");
            xmlDoc.LoadXml(information);
            string[] text = new string[] { };
            text = xmlDoc.FirstChild.Attributes[1].Value.Split(',');
            string date = text[2].Trim(' ').Trim('"');
            if (!string.IsNullOrEmpty(date))
            {
                keyWordProp["StartDate"] = Convert.ToDateTime(date).ToUniversalTime();
            }
            else
            {
                keyWordProp["StartDate"] = DateTime.MaxValue;
            }
            searchContent = "<a href=\"#\" onclick='clickDatePicker(\"ctl00_PlaceHolderMain_endDate_endDateDate\"";//"<input name=\"ctl00$PlaceHolderMain$endDate$endDateDate\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/a>");
            xmlDoc.LoadXml(information);
            text = xmlDoc.FirstChild.Attributes[1].Value.Split(',');
            date = text[2].Trim(' ').Trim('"');
            if (!string.IsNullOrEmpty(date))
            {
                keyWordProp["EndDate"] = Convert.ToDateTime(date).ToUniversalTime();
            }
            else
            {
                keyWordProp["EndDate"] = DateTime.MaxValue;
            }
            searchContent = "<a href=\"#\" onclick='clickDatePicker(\"ctl00_PlaceHolderMain_reviewDate_reviewDateDate\"";//"<input name=\"ctl00$PlaceHolderMain$reviewDate$reviewDateDate\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/a>");
            xmlDoc.LoadXml(information);
            text = xmlDoc.FirstChild.Attributes[1].Value.Split(',');
            date = text[2].Trim(' ').Trim('"');
            if (!string.IsNullOrEmpty(date))
            {
                keyWordProp["ReviewDate"] = Convert.ToDateTime(date).ToUniversalTime();
            }
            else
            {
                keyWordProp["ReviewDate"] = DateTime.MaxValue;
            }
            List<Dictionary<string, object>> bestBetsProp = new List<Dictionary<string, object>>();
            searchContent = "href=\"#BestBet\">Add Best Bet</a>";
            information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, "Keyword Definition</h3>");
            information = "<bestbet><tr><td><table>" + information + "Keyword Definition</h3></td></tr></bestbet>";
            GetBestBetsProperties(keyWordName, information, bestBetsProp);
            keyWordProp.Add("BestBets" + AveObjectModelConstant.ObjectPropertySuffix, bestBetsProp);
        }
        private void GetBestBetsProperties(string keyWordName, string infomation, List<Dictionary<string, object>> bestBetsProp)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(infomation);
            XmlNodeList bestBetNodes = xmlDoc.SelectSingleNode("/bestbet/tr/td/table/tr/td/table").ChildNodes;
            XmlNode bestBetNode = null;
            for (int i = 1; i < bestBetNodes.Count; i++)
            {
                Dictionary<string, object> bestBetProp = new Dictionary<string, object>();
                bestBetNode = bestBetNodes[i];
                bestBetProp["Title"] = bestBetNode.ChildNodes[0].SelectNodes("./table/tr/td/label")[0].InnerText;
                string bestBetUrl = bestBetNode.ChildNodes[3].SelectNodes("./table/input")[0].Attributes["value"].Value;
                bestBetProp["Url"] = bestBetUrl;
                string url = string.Format("{0}" + layout + "/BestBet.aspx?u={1}&k={2}&IsDlg=1", mSiteUrl.TrimEnd('/'), bestBetUrl, keyWordName);
                string html = AveHttpWebRequestUtility.HttpGet(url, tokenProvider);
                string xml = AveHttpWebRequestUtility.GetInput(html, "<textarea name=\"descriptionTextBox", "</textarea>");
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                bestBetProp["Description"] = doc.InnerText;
                bestBetsProp.Add(bestBetProp);
            }
        }

        public List<string> GetSiteEnabledHelpCollections()
        {
            string getUrl = mSiteUrl.TrimEnd('/') + layout + "/HelpSettings.aspx";
            List<string> helpCollection = new List<string>();
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            string searchContent = null;
            string endContent = null;
            searchContent = "<table id=\"ctl00_PlaceHolderMain_ctl00_ctl01_cbxlAvailableHelpCollections\"";
            endContent = "</table>";
            if (html.IndexOf(searchContent) == -1)
            {
                searchContent = "<span id=\"ctl00_PlaceHolderMain_ctl00_ctl01_cbxlAvailableHelpCollections\"";
                endContent = "</span>";
            }
            GetSiteEnabledHelpCollections(html, searchContent, endContent, helpCollection);
            return helpCollection;
        }
        public List<Dictionary<string, object>> GetPublishedContentTypes()
        {
            string getUrl = mSiteUrl.TrimEnd('/') + layout + "/contenttypesyndicationhubs.aspx";
            List<Dictionary<string, object>> metadataSevices = new List<Dictionary<string, object>>();
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            string searchContent = "<tr id=\"ctl00_PlaceHolderMain_ctl02_ctl01_tablerow3\">";
            string information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, "<tr id=\"ctl00_PlaceHolderMain_ctl02_ctl01_tablerow5\">");
            information = searchContent + information.Replace("&nbsp", "*");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(information);
            XmlNodeList contentTypeNodes = xmlDoc.SelectNodes("/tr/td/TABLE");
            foreach (XmlNode node in contentTypeNodes)
            {
                Dictionary<string, object> service = new Dictionary<string, object>();
                XmlNode nameNode = node.FirstChild.SelectSingleNode("TD/B");
                service["Name"] = nameNode.InnerText;
                XmlNode urlNode = node.ChildNodes[1].SelectSingleNode("td/a");
                if (urlNode != null)
                {
                    service["ContentTypeUrl"] = urlNode.InnerText;
                    XmlNodeList contentsNode = node.ChildNodes[3].SelectSingleNode("td/table").ChildNodes;
                    List<Dictionary<string, object>> contentTypes = new List<Dictionary<string, object>>();
                    for (int i = 1; i < contentsNode.Count; i++)
                    {
                        Dictionary<string, object> contentType = new Dictionary<string, object>();
                        XmlNode contentTypeNode = contentsNode[i];
                        if (contentTypeNode.ChildNodes.Count < 2)
                        {
                            break;
                        }
                        contentType["Name"] = contentTypeNode.SelectSingleNode("td/a").InnerText;
                        contentType["Group"] = contentTypeNode.ChildNodes[2].InnerText;
                        contentTypes.Add(contentType);
                    }
                    service.Add("ContentTypes", contentTypes);
                }
                metadataSevices.Add(service);
            }
            return metadataSevices;
        }
        public Dictionary<string, object> GetSitePortal(string siteUrl)
        {
            string getUrl = siteUrl.TrimEnd('/') + layout + "/portal.aspx?AjaxDelta=1&isStartPlt1=1344503071152";
            Dictionary<string, object> sitePortal = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            sitePortal.Add("PortalUrl", AveHttpWebRequestUtility.GetComponentValue(html, "ctl00$PlaceHolderMain$ctl00$ctl02$TxtPortalURL"));
            sitePortal.Add("PortalName", AveHttpWebRequestUtility.GetComponentValue(html, "ctl00$PlaceHolderMain$ctl00$ctl03$TxtPortalName"));
            return sitePortal;
        }
        public bool GetSiteRssSetting()
        {
            string netWorkUrl = mSiteUrl.TrimEnd('/') + layout + "/siterss.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(netWorkUrl, tokenProvider);
            bool allowSiteRss = AveHttpWebRequestUtility.GetCheckInput(html, "ctl00$PlaceHolderMain$SiteColRssSection$ctl01$CheckSiteColRss");
            return allowSiteRss;
        }

        public Dictionary<string, object> GetListVersionLimited(string webServerRelativeUrl, Guid listId)
        {
            //没有SecurityTriming，2010于2013可共用这部分代码
            Dictionary<string, object> versionLimitedProp = new Dictionary<string, object>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/LstSetng.aspx?List=" + listId;
            string html = string.Empty;
            html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            XmlDocument xmlDoc = new XmlDocument();

            string searchContent = "id=\"onetidMajorVersionLimit\"";
            string information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            information = information.TrimEnd(new char[] { '/', '>' });
            xmlDoc.LoadXml("<version " + information + "></version>");
            if (string.IsNullOrEmpty(xmlDoc.FirstChild.Attributes["value"].Value))
            {
                versionLimitedProp["MajorVersionLimit"] = 0;
            }
            else
            {
                versionLimitedProp["MajorVersionLimit"] = Convert.ToInt32(xmlDoc.FirstChild.Attributes["value"].Value);
            }

            searchContent = "id=\"onetidMajorWithMinorVersionLimit\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            information = information.TrimEnd(new char[] { '/', '>' });
            xmlDoc.LoadXml("<version " + information + "></version>");
            if (string.IsNullOrEmpty(xmlDoc.FirstChild.Attributes["value"].Value))
            {
                versionLimitedProp["MajorWithMinorVersionsLimit"] = 0;
            }
            else
            {
                versionLimitedProp["MajorWithMinorVersionsLimit"] = Convert.ToInt32(xmlDoc.FirstChild.Attributes["value"].Value);
            }

            return versionLimitedProp;
        }
        public Dictionary<string, object> GetListAdvancedSettingProperties(string webServerRelativeUrl, Guid listId, SecurityTrimObject mSiteTrimObj)
        {
            Dictionary<string, object> advancedProp = new Dictionary<string, object>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/advsetng.aspx?List={" + listId + "}";
            string html = string.Empty;//AveHttpWebRequestUtility.HttpGet(getUrl, mObj, listTrimObj, trimedProperties);
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            }
            catch (Exception e)
            {
                //SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                //SecurityTrimObject listTrimObj = webTrimObj.GetList(listId, string.Empty);
                //string[] properties = new string[] { "AdvancedSetting" };
                //foreach (string property in properties)
                //{
                //    if (!listTrimObj.TrimmedProperties.ContainsKey(property))
                //    {
                //        listTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", e.Message, getUrl);
                //    }
                //}
                return advancedProp;
            }
            if (string.IsNullOrEmpty(html))
            {
                return advancedProp;
            }
            string searchContent = "<input id=\"ctl00_PlaceHolderMain_OpenDocumentSection_ctl01_RadDefaultItemOpenPreferClient\"";
            string information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            if (!string.IsNullOrEmpty(information))
            {
                if (information.Contains("checked=\"checked\""))
                {
                    advancedProp["DefaultItemOpen"] = 0;
                    advancedProp["DefaultItemOpenUseListSetting"] = true;
                }
                else
                {
                    advancedProp["DefaultItemOpen"] = 1;
                    searchContent = "<input id=\"ctl00_PlaceHolderMain_OpenDocumentSection_ctl01_RadDefaultItemOpenBrowser\"";
                    information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
                    advancedProp["DefaultItemOpenUseListSetting"] = information.Contains("checked=\"checked\"");
                }
            }
            searchContent = "<input id=\"ctl00_PlaceHolderMain_TasksIssuesEmailSettingsSection_ctl00_RadEnableAssigntoEmailYes\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            //if (!string.IsNullOrEmpty(information))
            //{
            //    advancedProp["EnableAssignToEmail"] = information.Contains("checked=\"checked\"");
            //}
            searchContent = "<input name=\"ctl00$PlaceHolderMain$SendToSection$ctl01$TxtSendToLocationName\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            XmlDocument xmlDoc = new XmlDocument();
            if (!string.IsNullOrEmpty(information))
            {
                xmlDoc.LoadXml(information);
                XmlAttribute valueAtt = xmlDoc.FirstChild.Attributes["value"];
                advancedProp["SendToLocationName"] = valueAtt != null ? valueAtt.Value : string.Empty;
                searchContent = "<input name=\"ctl00$PlaceHolderMain$SendToSection$ctl02$TxtSendToLocationUrl\"";
                information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
                xmlDoc.LoadXml(information);
                valueAtt = xmlDoc.FirstChild.Attributes["value"];
                advancedProp["SendToLocationUrl"] = valueAtt != null ? valueAtt.Value : string.Empty;
            }
            searchContent = "<input id=\"ctl00_PlaceHolderMain_AllowSyncSection_ctl02_RadAllowSyncNo\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            advancedProp["ExcludeFromOfflineClient"] = information.Contains("checked=\"checked\"");
            searchContent = "<input id=\"ctl00_PlaceHolderMain_AllowGridEditingSection_ctl02_RadAllowGridNo\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            advancedProp["DisableGridEditing"] = information.Contains("checked=\"checked\"");
            searchContent = "<input id=\"ctl00_PlaceHolderMain_DialogForFormsPagesSection_ctl03_RadDialogForFormsPagesNo\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            advancedProp["NavigateForFormsPages"] = information.Contains("checked=\"checked\"");
            searchContent = "var readSecurity = ";
            information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, ";");
            if (!string.IsNullOrEmpty(information))
            {
                advancedProp["ReadSecurity"] = Convert.ToInt32(information);
                searchContent = "var writeSecurity = ";
                information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, ";");
                advancedProp["WriteSecurity"] = Convert.ToInt32(information);
            }
            return advancedProp;
        }

        public Dictionary<string, object> GetListGeneralProperties(string webServerRelativeUrl, Guid listId)
        {
            Dictionary<string, object> generalProperties = new Dictionary<string, object>();
            string url = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/ListGeneralSettings.aspx?List={" + listId + "}";
            string html = string.Empty;
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(url, tokenProvider);
            }
            catch (Exception e)
            {
                mLogger.Warn("Get list calendar general setting failed.Message:{0}", e.ToString());
                return generalProperties;
            }
            if (string.IsNullOrEmpty(html))
            {
                return generalProperties;
            }

            //Get Survey Options
            string search = "<input id=\"ctl00_PlaceHolderMain_SurveySection_ctl01_RadShowUserYes\"";
            string content = AveHttpWebRequestUtility.GetInput(html, search, "/>");
            if (!string.IsNullOrEmpty(content))
            {
                generalProperties["ShowUser"] = content.Contains("checked=\"checked\"");
            }
            search = "<input id=\"ctl00_PlaceHolderMain_SurveySection_ctl02_RadAllowMultiResponseYes\"";
            content = AveHttpWebRequestUtility.GetInput(html, search, "/>");
            if (!string.IsNullOrEmpty(content))
            {
                generalProperties["AllowMultiResponses"] = content.Contains("checked=\"checked\"");
            }
            //Get Calendar Options
            search = "<input id=\"ctl00_PlaceHolderMain_EventSection_ctl01_RadEnablePeopleSelectorYes\"";
            content = AveHttpWebRequestUtility.GetInput(html, search, "/>");
            if (!string.IsNullOrEmpty(content))
            {
                generalProperties["EnablePeopleSelector"] = content.Contains("checked=\"checked\"");
            }
            return generalProperties;
        }

        private Dictionary<string, string> GetSitePolicies(string url)
        {
            string html = AveHttpWebRequestUtility.HttpGet(url, tokenProvider);
            Dictionary<string, string> sitePolicies = new Dictionary<string, string>();
            string searchContent = "<a href=\"projectpolicyconfig.aspx?ctype=";
            string idEnd = "\">";
            string nameEnd = "<";
            if (html.Contains(searchContent))
            {
                int startIndex = html.IndexOf(searchContent) + searchContent.Length;
                int endIndex = html.IndexOf(idEnd, startIndex);
                while (true)
                {
                    string policyId = html.Substring(startIndex, endIndex - startIndex);
                    startIndex = endIndex + idEnd.Length;
                    endIndex = html.IndexOf(nameEnd, startIndex);
                    string policyName = html.Substring(startIndex, endIndex - startIndex);
                    html = html.Substring(endIndex);
                    sitePolicies.Add(policyName, policyId);
                    startIndex = html.IndexOf(searchContent);
                    if (startIndex == -1)
                    {
                        break;
                    }
                    startIndex += searchContent.Length;
                    endIndex = html.IndexOf(idEnd, startIndex);
                }
            }
            return sitePolicies;
        }

        public Dictionary<string, object> GetWorkflowAssociations(string webServerRelativeUrl, string listName, Guid listId, string workflowSource, Dictionary<string, object> contentTypeProp)
        {
            Dictionary<string, object> workflowAssociationsProp = new Dictionary<string, object>();
            string getUrl = string.Empty;
            if (workflowSource.Equals("list.workflow"))
            {
                getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/WrkSetng.aspx?List=" + listId;
            }
            else
            {
                AveHttpValueCollection values = new AveHttpValueCollection();
                values["List"] = listId.ToString();
                values["ctype"] = contentTypeProp["ContentTypeId"].ToString();
                getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/WrkSetng.aspx?" + values.ToString(true);
            }
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            string searchContent = "<tr valign=\"top\">";
            int startIndex = html.IndexOf(searchContent);
            while (startIndex >= 0)
            {
                string information = AveHttpWebRequestUtility.GetInput(html, startIndex, searchContent, "</tr>");
                GetWorkflowProperties(information, workflowAssociationsProp);
                startIndex = html.IndexOf(searchContent, startIndex + 1, StringComparison.OrdinalIgnoreCase);
            }
            return workflowAssociationsProp;
        }
        private void GetWorkflowProperties(string information, Dictionary<string, object> workflowAssociationsProp)
        {
            string tempStr = AveHttpWebRequestUtility.GetInput(information, "<a", "</a>");
            string name = AveHttpWebRequestUtility.GetInnerText(tempStr, ">", "<");
            int value = 0;
            if (int.TryParse(AveHttpWebRequestUtility.GetInnerText(information, "<td class=\"ms-vb\" nowrap=\"nowrap\">", "</td>"), out value))
            {
                workflowAssociationsProp.Add(name, value);
            }
        }
        #endregion

        #region Update
        public void UpdateWebLogo(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/prjsetng.aspx?AjaxDelta=1";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl00$RptControls$BtnCreate";
            if (webProperties.ContainsKey("SiteLogoUrl"))
            {
                bodyDic["ctl00$PlaceHolderMain$logoSection$ctl03$TxtSiteLogoUrl"] = webProperties["SiteLogoUrl"];
            }
            if (webProperties.ContainsKey("SiteLogoDescription"))
            {
                bodyDic["ctl00$PlaceHolderMain$logoSection$ctl04$TxtLogoUrlDescription"] = webProperties["SiteLogoDescription"];
            }
            if (webProperties.ContainsKey("Name"))
            {
                bodyDic["ctl00$PlaceHolderMain$idUrlSection$ctl03$TxtCreateSubwebName"] = webProperties["Name"];
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }
        public void UpdateWebSearchAndOfflineAvailability(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/srchvis.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            string searchContent = "<input type=\"hidden\"";
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl00%24RptControls%24btnOk";
            if (webProperties.ContainsKey("NoCrawl"))
            {
                if (Convert.ToBoolean(webProperties["NoCrawl"].ToString()))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24IndexSiteContent%24ctl01%24RadIndexSiteContent"] = "radIndexSiteContentNo";
                }
                else
                {
                    bodyDic["ctl00%24PlaceHolderMain%24IndexSiteContent%24ctl01%24RadIndexSiteContent"] = "radIndexSiteContentYes";
                }
            }
            if (webProperties.ContainsKey("ASPXPageIndexMode"))
            {
                if ((AveWebASPXPageIndexMode)webProperties["ASPXPageIndexMode"] == AveWebASPXPageIndexMode.Automatic)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24IndexAspxContent%24IndexAspxContentControl%24RadIndexAspxContent"] = "radIndexAspxContentAuto";
                }
                else if ((AveWebASPXPageIndexMode)webProperties["ASPXPageIndexMode"] == AveWebASPXPageIndexMode.Always)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24IndexAspxContent%24IndexAspxContentControl%24RadIndexAspxContent"] = "radIndexAspxContentForce";
                }
                else
                {
                    bodyDic["ctl00%24PlaceHolderMain%24IndexAspxContent%24IndexAspxContentControl%24RadIndexAspxContent"] = "radIndexAspxContentDont";
                }
            }
            if (webProperties.ContainsKey("ExcludeFromOfflineClient"))
            {
                if (Convert.ToBoolean(webProperties["ExcludeFromOfflineClient"].ToString()))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24AllowSyncSection%24ctl01%24AllowSync"] = "RadAllowSyncNo";
                }
                else
                {
                    bodyDic["ctl00%24PlaceHolderMain%24AllowSyncSection%24ctl01%24AllowSync"] = "RadAllowSyncYes";
                }
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }
        public void UpdateWebRegionalSetting(string webServerRelativeUrl, Dictionary<string, object> regionalProp)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/regionalsetng.aspx?AjaxDelta=1";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            this.GetPostBody(postUrl, bodyDic);

            #region Local and Calendar
            int localId = 0;
            if (regionalProp.ContainsKey("LocaleId") || regionalProp.ContainsKey("Local"))
            {
                if (regionalProp.ContainsKey("LocaleId"))
                {
                    localId = int.Parse(regionalProp["LocaleId"].ToString());
                }
                else
                {
                    localId = int.Parse(regionalProp["Local"].ToString());
                }
                bodyDic["ctl00%24PlaceHolderMain%24ctl02%24ctl01%24DdlwebLCID"] = localId;
                UpdateLocal(postUrl, html, bodyDic, regionalProp);
            }
            if (regionalProp.ContainsKey("CalendarType"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24ctl03%24ctl01%24DdlwebCalType"] = regionalProp["CalendarType"].ToString();
                UpdateCalendar(postUrl, html, bodyDic, regionalProp);
            }
            #endregion

            #region TimeZone and TimeFormat(更新Time需要先更新TimeZone和TimeFormat)
            if (regionalProp.ContainsKey("TimeZoneChangedProperties"))
            {
                Dictionary<string, object> timeZoneDic = regionalProp["TimeZoneChangedProperties"] as Dictionary<string, object>;
                if (timeZoneDic.ContainsKey("ID"))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl01%24ctl01%24DdlwebTimeZone"] = timeZoneDic["ID"].ToString();
                }
                else if (regionalProp.ContainsKey("TimeZoneId"))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl01%24ctl01%24DdlwebTimeZone"] = regionalProp["TimeZoneId"].ToString();
                }
            }
            bodyDic["ctl00%24PlaceHolderMain%24ctl10%24ctl01%24DdlTimeFormat"] = 1;

            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl06%24RptControls%24BtnUpdateRegionalSettings";
            byte[] localData = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", localData, null);
            GetPostBody(postUrl, bodyDic);
            #endregion

            #region SortOrder and AlternateCalendar
            if (regionalProp.ContainsKey("Collation"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24ctl09%24ctl01%24DdlwebCollation"] = regionalProp["Collation"].ToString();
            }
            if (regionalProp.ContainsKey("ShowWeeks") && Convert.ToBoolean(regionalProp["ShowWeeks"].ToString()))
            {
                bodyDic["ctl00%24PlaceHolderMain%24ctl03%24ctl02%24ChkShowWeekNumber"] = "on";
            }
            if (regionalProp.ContainsKey("AdjustHijriDays"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24ctl03%24ctl03%24DdlwebHijriDays"] = regionalProp["AdjustHijriDays"].ToString();
            }
            if (regionalProp.ContainsKey("AlternateCalendarType"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24ctl04%24ctl01%24DdlwebAltCalType"] = regionalProp["AlternateCalendarType"].ToString();
            }
            #endregion

            #region WorkWeek and TimeFormat
            if (regionalProp.ContainsKey("WorkDays"))
            {
                short workDays = short.Parse(regionalProp["WorkDays"].ToString());
                if ((workDays & 64) > 0)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl05%24ctl01%24ChkListWeeklyMultiDays%240"] = "on";
                }
                if ((workDays & 32) > 0)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl05%24ctl01%24ChkListWeeklyMultiDays%241"] = "on";
                }
                if ((workDays & 16) > 0)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl05%24ctl01%24ChkListWeeklyMultiDays%242"] = "on";
                }
                if ((workDays & 8) > 0)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl05%24ctl01%24ChkListWeeklyMultiDays%243"] = "on";
                }
                if ((workDays & 4) > 0)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl05%24ctl01%24ChkListWeeklyMultiDays%244"] = "on";
                }
                if ((workDays & 2) > 0)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl05%24ctl01%24ChkListWeeklyMultiDays%245"] = "on";
                }
                if ((workDays & 1) > 0)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl05%24ctl01%24ChkListWeeklyMultiDays%246"] = "on";
                }
            }

            if (regionalProp.ContainsKey("FirstDayOfWeek"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24ctl05%24ctl02%24DdlFirstDayOfWeek"] = regionalProp["FirstDayOfWeek"].ToString();
            }
            if (regionalProp.ContainsKey("FirstWeekOfYear"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24ctl05%24ctl02%24DdlFirstWeekOfYear"] = regionalProp["FirstWeekOfYear"].ToString();
            }

            if (regionalProp.ContainsKey("Time24"))
            {
                bool time24 = Convert.ToBoolean(regionalProp["Time24"].ToString());
                if (!time24)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl10%24ctl01%24DdlTimeFormat"] = 0;
                }
            }
            if (localId == 0 && regionalProp.ContainsKey("Local"))
            {
                localId = int.Parse(regionalProp["Local"].ToString());
            }
            CultureInfo info = new CultureInfo(localId, false);
            if (regionalProp.ContainsKey("WorkDayStartHour"))
            {
                int startHour = int.Parse(regionalProp["WorkDayStartHour"].ToString()) / 60;
                DateTime startTime = new DateTime(1, 1, 1, startHour, 0, 0);
                bodyDic["ctl00%24PlaceHolderMain%24ctl05%24ctl02%24DdlStartTime"] = startTime.ToString("HH:mm", info);
            }
            if (regionalProp.ContainsKey("WorkDayEndHour"))
            {
                int endHour = int.Parse(regionalProp["WorkDayEndHour"].ToString()) / 60;
                DateTime endTime = new DateTime(1, 1, 1, endHour, 0, 0);
                bodyDic["ctl00%24PlaceHolderMain%24ctl05%24ctl02%24DdlEndTime"] = endTime.ToString("HH:mm", info);
            }
            #endregion

            bodyDic["Cmd"] = "UPDATEPROJECT";
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl06%24RptControls%24BtnUpdateRegionalSettings";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", data, null);
        }
        public Dictionary<string, object> UpdateKeyWord(string mWebUrl, string term, int localId, int calendarType, Dictionary<string, object> keyWordProp)
        {
            string url = mWebUrl.TrimEnd('/') + string.Format(layout + "/Keyword.aspx?k={0}", term);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, tokenProvider);
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["ctl00$PlaceHolderMain$keyword"] = term;
            if (keyWordProp.ContainsKey("Term"))
            {
                bodyDic["ctl00$PlaceHolderMain$nameTextBox"] = keyWordProp["Term"].ToString();
            }
            if (keyWordProp.ContainsKey("Definition"))
            {
                bodyDic["ctl00$PlaceHolderMain$definitionTextBox"] = "<div></div>";
                bodyDic["ctl00$PlaceHolderMain$definitionTextBox_spSave"] = string.Format("<DIV>{0}</DIV>", keyWordProp["Definition"].ToString());
            }
            if (keyWordProp.ContainsKey("Contact") && !string.IsNullOrEmpty(keyWordProp["Contact"].ToString()))
            {
                bodyDic["ctl00$PlaceHolderMain$userPicker$hiddenSpanData"] = System.Web.HttpUtility.UrlEncode(string.Format("&nbsp;{0}", keyWordProp["Contact"].ToString()));
            }
            CultureInfo info = this.GetCultureWithCalendar(localId, calendarType);
            DateTime time = DateTime.Now;
            if (keyWordProp.ContainsKey("StartDate"))
            {
                time = Convert.ToDateTime(keyWordProp["StartDate"]);
                bodyDic["ctl00$PlaceHolderMain$startDate$startDateDate"] = System.Web.HttpUtility.UrlEncode(time.ToString(info.DateTimeFormat.ShortDatePattern, info));
            }
            if (keyWordProp.ContainsKey("EndDate"))
            {
                time = Convert.ToDateTime(keyWordProp["EndDate"]);
                if (time != DateTime.MaxValue)
                {
                    bodyDic["ctl00$PlaceHolderMain$endDate$endDateDate"] = System.Web.HttpUtility.UrlEncode(time.ToString(info.DateTimeFormat.ShortDatePattern, info));
                }
            }
            if (keyWordProp.ContainsKey("ReviewDate"))
            {
                time = Convert.ToDateTime(keyWordProp["ReviewDate"]);
                if (time != DateTime.MaxValue)
                {
                    bodyDic["ctl00$PlaceHolderMain$reviewDate$reviewDateDate"] = System.Web.HttpUtility.UrlEncode(time.ToString(info.DateTimeFormat.ShortDatePattern, info));
                }
            }
            bodyDic["ctl00$PlaceHolderMain$cmdOK"] = "OK";

            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(url, tokenProvider, "application/x-www-form-urlencoded", data, null);

            Dictionary<string, object> newKeyWordProp = new Dictionary<string, object>();
            newKeyWordProp = this.GetKeyWordProperties(term);
            return newKeyWordProp;
        }
        public Dictionary<string, object> UpdateSiteAdministrators(string webServerRelativeUrl, string oldAdmins, List<IDictionary<string, object>> newAdmins)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/mngsiteadmin.aspx";
            Dictionary<string, object> siteAdmins = new Dictionary<string, object>();
            StringBuilder strBuild = new StringBuilder("[");
            foreach (Dictionary<string, object> dic in newAdmins)
            {
                string content = null;
                string login = dic["LoginName"].ToString();
                content = "{\"Key\":" + "\"" + login + "\"" + "," + "\"IsResolved\":true}" + ",";
                strBuild.Append(content);
            }
            string info = (strBuild.ToString()).TrimEnd(',') + "]"; ;
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = System.Web.HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = System.Web.HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl01%24RptControls%24BtnSubmit";
            bodyDic["ctl00$PlaceHolderMain$ctl00$PeopleEditorAdminsClientPicker"] = info;
            bodyDic["ctl00$PlaceHolderMain$HdnOldSiteAdmins"] = oldAdmins;
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
            siteAdmins.AddChildren(newAdmins);
            return siteAdmins;
        }
        public Dictionary<string, object> UpdateSitePortalProperties(Dictionary<string, object> siteProperties)
        {
            Dictionary<string, object> sitePortal = new Dictionary<string, object>();
            string postUrl = mSiteUrl.TrimEnd('/') + layout + "/portal.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(html);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl01$RptControls$BtnUpdatePortalSettings";
            if (siteProperties.ContainsKey("PortalUrl") && !string.IsNullOrEmpty(siteProperties["PortalUrl"] as string)
                || siteProperties.ContainsKey("PortalName") && !string.IsNullOrEmpty(siteProperties["PortalName"] as string))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl00$portalEnabled"] = "onetidPortalEnabled";
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl02$TxtPortalURL"] = siteProperties.ContainsKey("PortalUrl") ? HttpUtility.UrlEncode(siteProperties["PortalUrl"].ToString()) : formValues["ctl00$PlaceHolderMain$ctl00$ctl02$TxtPortalURL"];
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl03$TxtPortalName"] = siteProperties.ContainsKey("PortalName") ? HttpUtility.UrlEncode(siteProperties["PortalName"].ToString()) : formValues["ctl00$PlaceHolderMain$ctl00$ctl03$TxtPortalName"];
            }
            else
            {
                bodyDic["ctl00$PlaceHolderMain$ctl00$portalEnabled"] = "onetidPortalNotEnabled";
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
            return sitePortal;
        }
        public void UpdateSiteRssSetting(bool syndicationEnabled)
        {
            string netWorkUrl = mSiteUrl + layout + "/siterss.aspx";
            string contentType = "application/x-www-form-urlencoded";
            string html = AveHttpWebRequestUtility.HttpGet(netWorkUrl, tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = System.Web.HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = System.Web.HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl00%24RptControls%24BtnApply";
            if (syndicationEnabled)
            {
                bodyDic["ctl00%24PlaceHolderMain%24SiteColRssSection%24ctl01%24CheckSiteColRss"] = "on";
            }
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(netWorkUrl, tokenProvider, contentType, data, null);
        }
        public void UpdateListAdvancedSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> advancedSettingProp)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/advsetng.aspx?List={" + listId + "}";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            IList<string> formKeys = new List<string>();
            Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(html);
            string searchContent = "var readSecurity = ";
            string information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, ";");
            if (!string.IsNullOrEmpty(information))
            {
                formValues["ctl00$PlaceHolderMain$ItemLevelSecuritySection$ctl09$ReadSecurity"] = Convert.ToInt32(information);
                searchContent = "var writeSecurity = ";
                information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, ";");
                formValues["ctl00$PlaceHolderMain$ItemLevelSecuritySection$ctl10$WriteSecurity"] = Convert.ToInt32(information);
            }
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl00$RptControls$BtnSaveAsTemplate";
            foreach (KeyValuePair<string, object> kvp in formValues)
            {
                bodyDic[kvp.Key] = kvp.Value;
            }
            foreach (KeyValuePair<string, object> value in advancedSettingProp)
            {
                bodyDic[value.Key] = value.Value;
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }
        public void UpdateListGeneralSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> generalSettingProp)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/ListGeneralSettings.aspx?List={" + listId + "}";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            if (string.IsNullOrEmpty(html))
            {
                return;
            }
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
            bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl01$RptControls$BtnSave";
            foreach (KeyValuePair<string, object> pair in generalSettingProp)
            {
                bodyDic[pair.Key] = pair.Value;
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }
        public void SetListVersionLimited(string webServerRelativeUrl, Guid listId, Dictionary<string, object> versionLimitedProperties)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/LstSetng.aspx?List=" + listId.ToString("B");//{" + listId + "}";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
            if (versionLimitedProperties.ContainsKey("MajorVersionLimit"))
            {
                int majorVersionLimited = (int)versionLimitedProperties["MajorVersionLimit"];
                bodyDic["MajorVersionLimit"] = majorVersionLimited;
                if (majorVersionLimited != 0)
                {
                    bodyDic["MajorVersionLimitEnabled"] = true;
                }
            }
            if (versionLimitedProperties.ContainsKey("MajorWithMinorVersionsLimit"))
            {
                int majorWithMinorVersionsLimit = (int)versionLimitedProperties["MajorWithMinorVersionsLimit"];
                bodyDic["MajorWithMinorVersionsLimit"] = majorWithMinorVersionsLimit;
                if (majorWithMinorVersionsLimit != 0)
                {
                    bodyDic["MajorMinorVersionLimitEnabled"] = true;
                }
            }
            if (bodyDic.ContainsKey("EnableMinorVersions"))//SAAS-3761
            {
                bodyDic.Remove("EnableMinorVersions");
            }
            bodyDic["Cmd"] = "MODLISTSETTINGS";
            bodyDic["List"] = listId;
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_vti_bin/owssvr.dll?CS=65001";
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }
        public void ReorderListFields(string webServerRelativeUrl, Guid listId, List<string> mappedSourceFields)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout + "/formEdt.aspx?List=" + listId.ToString("B");//{" + listId + "}";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(html);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();

            StringBuilder fieldsBuilder = new StringBuilder();
            XmlTextWriter xmlWriter = new XmlTextWriter(new StringWriter(fieldsBuilder));
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.WriteStartElement("Fields");
            for (int i = 0; i < mappedSourceFields.Count; i++)
            {
                xmlWriter.WriteStartElement("Field");
                xmlWriter.WriteAttributeString("Name", mappedSourceFields[i]);
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
            xmlWriter.Flush();

            object value;
            if (formValues.TryGetValue("__REQUESTDIGEST", out value))
            {
                bodyDic["__REQUESTDIGEST"] = value;
            }

            bodyDic["Cmd"] = "REORDERFIELDS";
            bodyDic["List"] = listId;
            bodyDic["ReorderedFields"] = fieldsBuilder.ToString();
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_vti_bin/owssvr.dll?CS=65001";
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }
        public void MoveNavigationNodeToCollection(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties)
        {
            try
            {
                string postUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.Trim('/');
                int nodeId = (int)navigationNodeProperties["NodeId"];
                postUrl = postUrl + string.Format(layout + "/editnav.aspx?ID={0}", nodeId);
                int parentId = (int)navigationNodeProperties["NodeParentId"];
                string nodeTitle = navigationNodeProperties["NodeTitle"].ToString();

                string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
                Dictionary<string, object> bodyDic = new Dictionary<string, object>();
                Dictionary<string, object> buttonDic = new Dictionary<string, object>();
                AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", bodyDic);
                AveHttpWebRequestUtility.GetInput(html, "<input type=\"button\"", buttonDic);
                foreach (string key in buttonDic.Keys)
                {
                    if (key.EndsWith("BtnOk", StringComparison.OrdinalIgnoreCase))
                    {
                        bodyDic["__EVENTTARGET"] = System.Web.HttpUtility.UrlEncode(key);
                        break;
                    }
                }
                if (bodyDic.ContainsKey("__EVENTVALIDATION"))
                {
                    bodyDic["__EVENTVALIDATION"] = System.Web.HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
                }
                if (bodyDic.ContainsKey("__VIEWSTATE"))
                {
                    bodyDic["__VIEWSTATE"] = System.Web.HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
                }
                bodyDic["ctl00%24PlaceHolderMain%24ctl00%24ctl02%24txtTitle"] = nodeTitle;
                bodyDic["ctl00%24PlaceHolderMain%24CategorySection%24ctl01%24SelectList1"] = parentId;
                byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
                AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", data, null);
            }
            catch (Exception ex)
            {
                mLogger.Error("Move NavigationNode to nodeCollection failed.Web:{0}.Error Message:{1}", webServerRelativeUrl, ex.ToString());
                throw new Exception("move navigationNode to nodeCollection failed");
            }
        }
        public void MoveNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> previousNodeProperties, string moveMethodName)
        {
            try
            {
                string source = navigationNodeProperties["NodeSource"].ToString();
                string postUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
                if (source.Equals("QuickLaunch", StringComparison.Ordinal))
                {
                    postUrl = postUrl.TrimEnd('/') + layout + "/qlreord.aspx";
                }
                else if (source.Equals("TopNavigationBar", StringComparison.Ordinal))
                {
                    postUrl = postUrl.TrimEnd('/') + layout + "/tnreord.aspx";
                }
                int oldPosition = (int)navigationNodeProperties["NodeOldPosition"];
                int count = (int)navigationNodeProperties["NodeCount"];
                int parentId = (int)navigationNodeProperties["NodeParentId"];
                string moveItem = string.Empty;
                if (moveMethodName.Equals("MoveToLast"))
                {
                    moveItem = parentId.ToString() + "," + oldPosition.ToString() + " ," + (count - 1).ToString() + ";";
                }
                else if (moveMethodName.Equals("MoveToFirst"))
                {
                    moveItem = parentId.ToString() + "," + oldPosition.ToString() + " ," + "0;";
                }
                else if (moveMethodName.Equals("Move", StringComparison.OrdinalIgnoreCase))
                {
                    int newPosition = (int)navigationNodeProperties["NodeNewPosition"];
                    moveItem = parentId.ToString() + "," + oldPosition.ToString() + " ," + newPosition.ToString() + ";";
                }
                string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
                Dictionary<string, object> bodyDic = new Dictionary<string, object>();
                AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", bodyDic);
                if (bodyDic.ContainsKey("__EVENTVALIDATION"))
                {
                    bodyDic["__EVENTVALIDATION"] = System.Web.HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
                }
                if (bodyDic.ContainsKey("__VIEWSTATE"))
                {
                    bodyDic["__VIEWSTATE"] = System.Web.HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
                }
                bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl00%24RptControls%24BtnOk";
                bodyDic["MovedItems"] = System.Web.HttpUtility.UrlEncode(moveItem);
                byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
                AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", data, null);
            }
            catch (Exception ex)
            {
                mLogger.Error("Move navigation failed.Web:{0}.Error Message:{1]", webServerRelativeUrl, ex.ToString());
                throw new Exception("move navigation failed");
            }
        }
        #endregion

        #region  set
        public void SetSiteEnabledHelpCollections(string[] enabledHelpCollections)
        {
            string postUrl = mSiteUrl.TrimEnd('/') + layout + "/HelpSettings.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl01$RptControls$BtnUpdateHelpSettings";
            for (int i = 0; i < enabledHelpCollections.Length; i++)
            {
                bodyDic[enabledHelpCollections[i]] = "on";
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }
        public static void OperateOnVersion(string url, string webAppName, ITokenProvider tokenProvider, string listUrl, int itemId, int versionId, string listId, string fileName, string op)
        {
            string source = webAppName.TrimEnd('/') + "/" + listUrl.Trim('/') + "?" + "InitialTabId=Ribbon%2EListItem" + "&VisibilityContext=WSSTabPersistence";
            string col = "Number";
            string order = "d";
            string isDlg = "1";

            string getUrl = GetUrl(url, fileName, listId, itemId.ToString(), null, null, source, null, null, isDlg, "get");
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            Dictionary<string, object> inputDic = new Dictionary<string, object>();
            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, inputDic);
            if (inputDic.ContainsKey("__VIEWSTATE"))
            {
                inputDic["__VIEWSTATE"] = System.Web.HttpUtility.UrlEncode(inputDic["__VIEWSTATE"].ToString());
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(inputDic, null);

            string postUrl = GetUrl(url, fileName, listId, itemId.ToString(), col, order, source, op, versionId.ToString(), isDlg, "post");
            string contentType = "application/x-www-form-urlencoded";
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, contentType, body, null);
        }

        public static void DeleteListItemVersions(string url, string webAppName, ITokenProvider tokenProvider, string listUrl, int itemId, string listId, string fileName, string op)
        {
            string source = webAppName.TrimEnd('/') + "/" + listUrl.Trim('/') + "?" + "InitialTabId=Ribbon%2EListItem" + "&VisibilityContext=WSSTabPersistence";
            string col = "Number";
            string order = "d";
            string isDlg = "1";

            string getUrl = GetUrl(url, fileName, listId, itemId.ToString(), null, null, source, null, null, isDlg, "get");
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            Dictionary<string, object> inputDic = new Dictionary<string, object>();
            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, inputDic);
            if (inputDic.ContainsKey("__VIEWSTATE"))
            {
                inputDic["__VIEWSTATE"] = System.Web.HttpUtility.UrlEncode(inputDic["__VIEWSTATE"].ToString());
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(inputDic, null);

            string postUrl = GetUrl(url, fileName, listId, itemId.ToString(), col, order, source, op, isDlg, "post"); ;
            string contentType = "application/x-www-form-urlencoded";
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, contentType, body, null);
        }

        private static string GetUrl(string url, string fileName, string listId, string itemId, string col, string order, string source, string op, string isDlg, string type)
        {
            return url + "FileName=" + fileName + "&list=" + listId + "&ID=" + itemId + "&col=" + col + "&order=" + order + "&Source=" + source + "&op=" + op + "&IsDlg=" + isDlg;
        }
        private static string GetUrl(string url, string fileName, string listId, string itemId, string col, string order, string source, string op, string verId, string isDlg, string type)
        {
            string strUrl = string.Empty;

            switch (type)
            {
                case "get":
                    strUrl = url
                + "list=" + listId
                + "&ID=" + itemId
                + "&FileName=" + fileName
                + "&Source=" + source
                + "&IsDlg=" + isDlg;
                    break;
                case "post":
                    strUrl = url
                 + "FileName=" + fileName
                 + "&list=" + listId
                 + "&ID=" + itemId
                 + "&col=" + col
                 + "&order=" + order
                 + "&Source=" + source
                 + "&op=" + op
                 + "&ver=" + verId
                 + "&IsDlg=" + isDlg;
                    break;
                default:
                    break;
            }

            return strUrl;
        }
        public void RestoreMasterPage(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebMasterPageInfo pageInfo, string alternateCssUrl)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + layout+"/ChangeSiteMasterPage.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            //bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl03%24RptControls%24ButtonSaveSettings";
            if (pageInfo.CInheriting)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl01$InheritSiteMasterRadioGroup"] = "inheritChromeRadioButton";

            }
            else if (!string.IsNullOrEmpty(pageInfo.CPageUrl))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl01$InheritSiteMasterRadioGroup"] = "selectChromeRadioButton";
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl01$masterPageSelectionControl$ctl00$SiteMasterPageDropDownList"] = pageInfo.CPageUrl;
            }
            if (pageInfo.MInheriting)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl01$ctl01$InheritSystemMasterPageGroup"] = "inheritSystemMasterPageRadioButton";
            }
            else if (!string.IsNullOrEmpty(pageInfo.MPageUrl))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl01$ctl01$InheritSystemMasterPageGroup"] = "selectSystemMasterPageRadioButton";
                bodyDic["ctl00$PlaceHolderMain$ctl01$ctl01$systemMasterPageSelectionControl$ctl00$SystemMasterPageDropDownList"] = pageInfo.MPageUrl;
            }
            if (pageInfo.Inheriting)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ctl01$InheritAlternateCssGroup"] = "inheritAlternateCssRadioButton";
            }
            else if (!string.IsNullOrEmpty(alternateCssUrl))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ctl01$InheritAlternateCssGroup"] = "selectAlternateCssRadioButton";
                bodyDic["ctl00$PlaceHolderMain$ctl03$ctl01$alternateCssSelector$AssetUrlInput"] = alternateCssUrl;
            }
            else
            {
                bodyDic["tl00$PlaceHolderMain$ctl03$ctl01$InheritAlternateCssGroup"] = "useWssCssRadioButton";
            }
            bodyDic["ctl00$PlaceHolderMain$ctl04$RptControls$ButtonSaveSettings"] = "OK";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }
        public void SetMetadataNavigationSettings(string webServerRelativeUrl, string listTitle, Guid listId, Dictionary<string, object> updateProperties)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + layout+"/MetaNavSettings.aspx?List={" + listId + "}";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            if ((bool)updateProperties["AutomaticallyManageListIndexing"])
            {
                bodyDic["ctl00$PlaceHolderMain$ctl02$IndexAutoManagementRadioGroup"] = "autoIndexingYesRadioButton";
            }
            else
            {
                bodyDic["ctl00$PlaceHolderMain$ctl02$IndexAutoManagementRadioGroup"] = "autoIndexingNoRadioButton";
            }
            bodyDic["ctl00$PlaceHolderMain$ctl00$groupedHierarchyPicker"] = updateProperties["HierarchyPicker"].ToString();
            bodyDic["ctl00$PlaceHolderMain$ctl00$groupedHierarchyPicker$data"] = updateProperties["HierarchyData"].ToString();
            bodyDic["ctl00$PlaceHolderMain$ctl00$groupedHierarchyPicker$initial"] = updateProperties["HierarchyInitial"].ToString();
            bodyDic["ctl00$PlaceHolderMain$ctl01$groupedKeyFilterPicker$data"] = updateProperties["KeyFilterData"].ToString();
            bodyDic["ctl00$PlaceHolderMain$ctl01$groupedKeyFilterPicker"] = updateProperties["KeyFilterPicker"].ToString();
            bodyDic["ctl00$PlaceHolderMain$ctl01$groupedKeyFilterPicker$initial"] = updateProperties["KeyFilterInitial"].ToString();
            bodyDic["ctl00$PlaceHolderMain$btnsApplyCancel$RptControls$BtnSave"] = "OK";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }
        #endregion

        #region Add
        public Dictionary<string, object> AddBestBet(string term, List<string> bestBetUrlList, Dictionary<string, object> bestBetProp, string action)
        {
            string url = mSiteUrl.TrimEnd('/') + string.Format(layout + "/Keyword.aspx?k={0}", term);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, tokenProvider);
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            string bestBetUrl = bestBetProp.ContainsKey("Url") ? bestBetProp["Url"].ToString() : string.Empty;
            string bestBetTitle = bestBetProp.ContainsKey("Title") ? bestBetProp["Title"].ToString() : string.Empty;
            string BestBetDescription = bestBetProp.ContainsKey("Description") ? bestBetProp["Description"].ToString() : string.Empty;
            //bodyDic["BestBetAction"] = "newBestBet";
            //bodyDic["urlTextBox"] = bestBetUrl;
            //bodyDic["titleTextBox"] = bestBetTitle;
            //bodyDic["descriptionTextBox"] = BestBetDescription;
            //string bestBet = string.Format("{0};{1};{2}", bestBetUrl, bestBetTitle, BestBetDescription);
            string bestBet = string.Empty;
            if (action.Equals("Add"))
            {
                bestBet = this.AddBestBet(term, bestBetTitle, bestBetUrl, BestBetDescription);
            }
            else if (action.Equals("Exist"))
            {
                bestBet = this.AddExistBestBet(term, bestBetUrl);
            }
            else if (action.Equals("Edit"))
            {
                this.AddExistBestBet(term, bestBetUrl);
                bestBet = this.EditBestBet(term, bestBetTitle, bestBetUrl, BestBetDescription);
            }

            bodyDic["ctl00$PlaceHolderMain$bestBet"] = bestBet;
            int id = 140;
            int orderId = 1;
            int order = 1;
            foreach (string betUrl in bestBetUrlList)
            {
                bodyDic[string.Format("ctl00$PlaceHolderMain$ct{0}", id)] = betUrl;
                bodyDic[string.Format("ctl00$PlaceHolderMain$OrderDD{0}", orderId)] = order;
                id += 7;
                orderId += 3;
                order++;
            }
            bodyDic["ctl00$PlaceHolderMain$cmdOK"] = "OK";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(url, tokenProvider, "application/x-www-form-urlencoded", data, null);

            return bestBetProp;
        }
        public string AddBestBet(string term, string bestBetTitle, string bestBetUrl, string bestBetDescription)
        {
            string url = mSiteUrl.TrimEnd('/') + string.Format(layout + "/BestBet.aspx?k={0}&IsDlg=1", term);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, tokenProvider);
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["BestBetAction"] = "newBestBet";
            bodyDic["urlTextBox"] = bestBetUrl;
            bodyDic["titleTextBox"] = bestBetTitle;
            bodyDic["descriptionTextBox"] = bestBetDescription;
            bodyDic["cmdOK"] = "OK";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(url, tokenProvider, "application/x-www-form-urlencoded", data, null);
            string bestBet = string.Format("{0};{1};{2}", bestBetUrl, bestBetTitle, bestBetDescription);
            return bestBet;
        }
        public string AddExistBestBet(string term, string bestBetUrl)
        {
            string url = mSiteUrl.TrimEnd('/') + string.Format(layout + "/BestBet.aspx?k={0}&IsDlg=1", term);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, tokenProvider);
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["BestBetAction"] = "existingBestBet";
            bodyDic["urlTextBox"] = "a";
            bodyDic["titleTextBox"] = "b";
            bodyDic["lstBestBets"] = bestBetUrl;
            bodyDic["cmdOK"] = "OK";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(url, tokenProvider, "application/x-www-form-urlencoded", data, null);
            string bestBet = string.Format("{0};;;", bestBetUrl);
            return bestBet;
        }
        public string EditBestBet(string term, string bestBetTitle, string bestBetUrl, string bestBetDescription)
        {
            string a = string.Format("{0};;;", bestBetUrl);
            string url = mSiteUrl.TrimEnd('/') + layout + "/BestBet.aspx?";
            string postUrl = string.Format("{0}u={1}&k={2}&a={3}&IsDlg=1", url, bestBetUrl, term, a);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["bestBets"] = a;
            bodyDic["urlTextBox"] = bestBetUrl;
            bodyDic["titleTextBox"] = bestBetTitle;
            bodyDic["descriptionTextBox"] = bestBetDescription;
            bodyDic["cmdOK"] = "OK";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", data, null);
            return a;
        }
        
        public void AddSitePolicy(string policySchema, string siteUrl)
        {
            AveSitePolicyInfo policyInfo = new AveSitePolicyInfo();
            policyInfo.LoadFromXml(policySchema);
            string contentType = "application/x-www-form-urlencoded";
            string settingUrl = siteUrl + "/_layouts/15/projectpolicyconfig.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(settingUrl, tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = System.Web.HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = System.Web.HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["ctl00$PlaceHolderMain$ctl00$ctl01$textBoxName"] = policyInfo.Name;
            bodyDic["ctl00$PlaceHolderMain$ctl00$ctl02$textBoxDescription"] = System.Web.HttpUtility.UrlEncode(policyInfo.Description);
            bodyDic["ctl00$PlaceHolderMain$ctl01$ClosureAndDeletionOptions"] = "radioButton" + policyInfo.CloseDeleteOption;
            if (policyInfo.CloseDeleteOption != "NoCloseDelete")
            {
                if (policyInfo.CloseDeleteOption == "CloseDelete")
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl01$textBoxTimePeriodOnClose"] = policyInfo.NumberOfTimePeriodOnClose;
                    bodyDic["ctl00$PlaceHolderMain$ctl01$dropDownListTimePeriodOnClose"] = policyInfo.TimePeriodOnClose;
                    bodyDic["ctl00$PlaceHolderMain$ctl01$textBoxTimePeriodOnCloseDelete"] = policyInfo.NumberOfTimePeriodOnDelete;
                    bodyDic["ctl00$PlaceHolderMain$ctl01$dropDownListTimePeriodOnCloseDelete"] = policyInfo.TimePeriodOnDelete;
                }
                else
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl01$dropDownListFieldOnDelete"] = policyInfo.FieldNameOnDelete;
                    bodyDic["ctl00$PlaceHolderMain$ctl01$textBoxTimePeriodOnDelete"] = policyInfo.NumberOfTimePeriodOnDelete;
                    bodyDic["ctl00$PlaceHolderMain$ctl01$dropDownListTimePeriodOnDelete"] = policyInfo.TimePeriodOnDelete;
                }

                bodyDic["ctl00$PlaceHolderMain$ctl01$textBoxTimePeriodOnWorkflow"] = policyInfo.NumberOfTimePeriodOnWorkflow;
                bodyDic["ctl00$PlaceHolderMain$ctl01$dropDownListTimePeriodOnWorkflow"] = policyInfo.TimePeriodOnWorkflow;
                bodyDic["ctl00$PlaceHolderMain$ctl01$textBoxTimePeriodOnWorkflowRecur"] = policyInfo.NumberOfTimePeriodOnWorkflowRecur;
                bodyDic["ctl00$PlaceHolderMain$ctl01$dropDownListTimePeriodOnWorkflowRecur"] = policyInfo.TimePeriodOnWorkflowRecur;

                if (Convert.ToBoolean(policyInfo.AllowEmailNotification))
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl01$checkBoxEmailNotification"] = "on";
                    bodyDic["ctl00$PlaceHolderMain$ctl01$textBoxTimePeriodOnEmailNotification"] = policyInfo.NumberOfTimePeriodOnEmailNotification;
                    bodyDic["ctl00$PlaceHolderMain$ctl01$dropDownListTimePeriodOnEmailNotification"] = policyInfo.TimePeriodOnEmailNotification;
                }
                if (Convert.ToBoolean(policyInfo.AllowEmailFollowUp))
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl01$checkBoxEmailFollowUp"] = "on";
                    bodyDic["ctl00$PlaceHolderMain$ctl01$textBoxTimePeriodOnEmailFollowUp"] = policyInfo.NumberOfTimePeriodOnEmailFollowUp;
                    bodyDic["ctl00$PlaceHolderMain$ctl01$dropDownListTimePeriodOnEmailFollowUp"] = policyInfo.TimePeriodOnEmailFollowUp;
                }
                if (Convert.ToBoolean(policyInfo.AllowPostpone))
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl01$checkBoxPostpone"] = "on";
                    bodyDic["ctl00$PlaceHolderMain$ctl01$textBoxTimePeriodOnPostpone"] = policyInfo.NumberOfTimePeriodOnPostpone;
                    bodyDic["ctl00$PlaceHolderMain$ctl01$dropDownListTimePeriodOnPostpone"] = policyInfo.TimePeriodOnPostpone;
                }
            }
            if (Convert.ToBoolean(policyInfo.CloseToReadOnly))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl02$checkBoxCloseToReadOnly"] = "on";
            }
            bodyDic["ctl00$PlaceHolderMain$ctl03$RptControls$BtnOK"] = "OK";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            Dictionary<string, string> sitePolicies = GetSitePolicies(siteUrl + "/_layouts/15/ProjectPolicies.aspx");
            if (sitePolicies.ContainsKey(policyInfo.Name))
            {
                string policyId = sitePolicies[policyInfo.Name];
                settingUrl = string.Format("{0}?ctype={1}", settingUrl, policyId);
            }
            AveHttpWebRequestUtility.HttpPost(settingUrl, tokenProvider, contentType, data, null);
        }

        public Dictionary<string, object> AddKeyWord(string term, DateTime startDate, int localId, int calendarType)
        {
            string url = mSiteUrl.TrimEnd('/') + layout + "/Keyword.aspx";
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, tokenProvider);
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["ctl00$PlaceHolderMain$nameTextBox"] = term;
            CultureInfo info = this.GetCultureWithCalendar(localId, calendarType);
            bodyDic["ctl00$PlaceHolderMain$startDate$startDateDate"] = System.Web.HttpUtility.UrlEncode(startDate.ToString(info.DateTimeFormat.ShortDatePattern, info));
            bodyDic["ctl00$PlaceHolderMain$cmdOK"] = "OK";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(url, tokenProvider, "application/x-www-form-urlencoded", data, null);
            Dictionary<string, object> keyWordProp = new Dictionary<string, object>();
            try
            {
                keyWordProp = this.GetKeyWordProperties(term);
            }
            catch (Exception ex)
            {
                mLogger.Error("Add KeyWord:{0} Failed.Error Message:{1}", term, ex.ToString());
                throw new Exception("Add KeyWord Failed");
            }
            return keyWordProp;
        }
        public string AddSynonm(string term, string synTerm, string terms)
        {
            string url = mSiteUrl.TrimEnd('/') + string.Format(layout + "/Keyword.aspx?k={0}", term);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, tokenProvider);
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["ctl00$PlaceHolderMain$keyword"] = term;
            bodyDic["ctl00$PlaceHolderMain$synTextBox"] = terms;
            bodyDic["ctl00$PlaceHolderMain$cmdOK"] = "OK";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(url, tokenProvider, "application/x-www-form-urlencoded", data, null);
            return synTerm;
        }
        #endregion

        #region private function
        private Dictionary<string, object> GetPostBody(string postUrl, Dictionary<string, object> bodyDic)
        {
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = System.Web.HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = System.Web.HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            return bodyDic;
        }
        private void UpdateLocal(string postUrl, string html, Dictionary<string, object> bodyDic, Dictionary<string, object> regionalProp)
        {
            bodyDic["Cmd"] = "UPDATEPROJECT";
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl02%24ctl01%24DdlwebLCID";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", data, null);
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl06%24RptControls%24BtnUpdateRegionalSettings";
            byte[] newData = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", newData, null);
            this.GetPostBody(postUrl, bodyDic);
        }
        private void UpdateCalendar(string postUrl, string html, Dictionary<string, object> bodyDic, Dictionary<string, object> regionalProp)
        {
            bodyDic["Cmd"] = "UPDATEPROJECT";
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl03%24ctl01%24DdlwebCalType";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", data, null);
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl06%24RptControls%24BtnUpdateRegionalSettings";
            byte[] newData = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", newData, null);
            this.GetPostBody(postUrl, bodyDic);
        }
        private void SetAvailableFields(Dictionary<string, List<string[]>> FieldsProp, string infoMation, string type)
        {
            List<string[]> fields = new List<string[]>();
            string[] AvailableFields = infoMation.Split(new string[] { "|t|t" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string field in AvailableFields)
            {
                string[] fieldInfo = field.Split(new string[] { "|t" }, StringSplitOptions.RemoveEmptyEntries);
                fields.Add(fieldInfo);
            }
            FieldsProp.Add(type, fields);
        }
        private void SetSelectedFields(Dictionary<string, List<string[]>> FieldsProp, string infoMation, string type)
        {
            string availbleFieldsType = string.Empty;
            List<string[]> relatedAvailableFields = null;
            List<string[]> SelectedFields = new List<string[]>();
            List<string[]> RealAvailableFields = new List<string[]>();
            if (type.Equals("SelectedHierarchyFields"))
            {
                availbleFieldsType = "AvailableHierarchyFields";
                relatedAvailableFields = FieldsProp["AvailableHierarchyFields"];
            }
            else
            {
                availbleFieldsType = "AvailableKeyFilterFields";
                relatedAvailableFields = FieldsProp["AvailableKeyFilterFields"];
            }
            string[] tempFields = infoMation.Split(new string[] { "|t" }, StringSplitOptions.RemoveEmptyEntries);
            List<string> tempFieldsIds = new List<string>();
            for (int i = 0; i < tempFields.Length; i++)
            {
                tempFieldsIds.Add(tempFields[i]);
                i++;
            }
            foreach (string[] field in relatedAvailableFields)
            {
                if (tempFieldsIds.Contains(field[0]))
                {
                    SelectedFields.Add(field);
                }
                else
                {
                    RealAvailableFields.Add(field);
                }
            }
            FieldsProp[type] = SelectedFields;
            FieldsProp[availbleFieldsType] = RealAvailableFields;
        }

        private CultureInfo GetCultureWithCalendar(int localId, int calendarType)
        {
            CultureInfo info = null;
            switch ((AveCalendarType)calendarType)
            {
                case AveCalendarType.Gregorian:
                    info = new CultureInfo(1033);
                    break;
                case AveCalendarType.Japan:
                    info = new CultureInfo(1041);
                    info.DateTimeFormat.Calendar = new JapaneseCalendar();
                    break;
                case AveCalendarType.Korea:
                    info = new CultureInfo(1042);
                    info.DateTimeFormat.Calendar = new KoreanCalendar();
                    break;
                case AveCalendarType.Hijri:
                    info = new CultureInfo(1025);
                    info.DateTimeFormat.Calendar = new HijriCalendar();
                    break;
                case AveCalendarType.Thai:
                    info = new CultureInfo(1054);
                    info.DateTimeFormat.Calendar = new ThaiBuddhistCalendar();
                    break;
                case AveCalendarType.Hebrew:
                    info = new CultureInfo(1037);
                    info.DateTimeFormat.Calendar = new HebrewCalendar();
                    break;
                case AveCalendarType.GregorianArabic:
                case AveCalendarType.GregorianMEFrench:
                case AveCalendarType.GregorianXLITEnglish:
                case AveCalendarType.GregorianXLITFrench:
                    info = new CultureInfo(3073);
                    break;
                default:
                    info = new CultureInfo(localId);
                    break;
            }
            return info;
        }
        private void GetSiteEnabledHelpCollections(string html, string searchContent, string endContent, List<string> helpCollection)
        {
            string information = AveHttpWebRequestUtility.GetInput(html, searchContent, endContent);
            string inputSearechStr = null;
            string labelSearchStr = null;
            if (endContent.Equals("</span>"))
            {
                inputSearechStr = "/span/input";
                labelSearchStr = "/span/label";
            }
            else
            {
                inputSearechStr = "/table/tr/td/input";
                labelSearchStr = "/table/tr/td/label";
            }
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(information);
            XmlNodeList inPutNodeList = xmlDoc.SelectNodes(inputSearechStr);
            XmlNodeList labelNodeList = xmlDoc.SelectNodes(labelSearchStr);
            Dictionary<string, string> tempCatch = new Dictionary<string, string>();
            foreach (XmlNode inputNode in inPutNodeList)
            {
                string value = inputNode.Attributes["name"].Value;
                if (inputNode.Attributes["checked"] != null)
                {
                    value += "#checked";
                }
                tempCatch.Add(inputNode.Attributes["id"].Value, value);
            }
            foreach (XmlNode labelNode in labelNodeList)
            {
                helpCollection.Add(labelNode.InnerText + "#" + tempCatch[labelNode.Attributes["for"].Value]);
            }
        }
        #endregion
        #endregion
    }
}
