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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AveClientRequest.Common;
using System.Xml;
using System.Web;
using System.Text.RegularExpressions;
using System.Collections;
using System.Globalization;
using Microsoft.SharePoint.Client;
using Microsoft365.Authentication;
using Newtonsoft.Json;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveHttpWebRequestCommon2013 : IAveHttpWebRequestCommon
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveHttpWebRequestCommon2013));
        private string mLayout = "/_layouts/15";
        private ITokenProvider tokenProvider;
        private string mSiteUrl;
        private string mWebAppName;
        private AveHttpWebRequestCommon mRequestCommon;
        private string mInternalServerVersion;

        public AveHttpWebRequestCommon2013(string siteUrl, ITokenProvider tokenProvider, string internalServerVersion)
        {
            mSiteUrl = siteUrl;
            this.tokenProvider = tokenProvider;
            mInternalServerVersion = internalServerVersion;
            mRequestCommon = new AveHttpWebRequestCommon(mSiteUrl, tokenProvider, "15.0.0.0", internalServerVersion);
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

        #region Get
        public Dictionary<string, object> GetAllFeatureDefinitions(string url, int lcid, string featuresSource)
        {
            string requestUrl = string.Empty;
            switch (featuresSource)
            {
                case "web.features":
                    requestUrl = url.TrimEnd('/') + mLayout + "/ManageFeatures.aspx";
                    break;
                case "site.features":
                    requestUrl = url.TrimEnd('/') + mLayout + "/ManageFeatures.aspx?Scope=Site";
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
                title = HttpUtility.HtmlDecode(title);//法语存在转译字符 Listes de collaboration d&#39;équipe 

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

        [Obsolete]
        public void GetWebSearchAndOfflineAvailability(string webServerRelativeUrl, Dictionary<string, object> webProp, ITokenProvider tokenProvider)
        {
            string getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/srchvis.aspx?AjaxDelta=1 ";
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
        public Dictionary<string, object> GetMetadataNavigationSettings(string webServerRelativeUrl, Guid listId, string listTitle)
        {
            Dictionary<string, object> metadataNavigationSettings = new Dictionary<string, object>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/MetaNavSettings.aspx?List={" + listId + "}";
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
            if (!string.IsNullOrEmpty(information))
            {
                xmlDoc.LoadXml(information);
                SetAvailableFields(FieldsProp, xmlDoc.FirstChild.Attributes["value"].Value, "AvailableHierarchyFields");
            }

            searchContent = "<input id=\"ctl00_PlaceHolderMain_ctl00_groupedHierarchyPicker_initial\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</input>");
            if (!string.IsNullOrEmpty(information))
            {
                xmlDoc.LoadXml(information);
                SetSelectedFields(FieldsProp, xmlDoc.FirstChild.Attributes["value"].Value, "SelectedHierarchyFields");
            }

            searchContent = "<input id=\"ctl00_PlaceHolderMain_ctl01_groupedKeyFilterPicker_data\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</input>");
            if (!string.IsNullOrEmpty(information))
            {
                xmlDoc.LoadXml(information);
                SetAvailableFields(FieldsProp, xmlDoc.FirstChild.Attributes["value"].Value, "AvailableKeyFilterFields");
            }

            searchContent = "<input id=\"ctl00_PlaceHolderMain_ctl01_groupedKeyFilterPicker_initial\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</input>");
            if (!string.IsNullOrEmpty(information))
            {
                xmlDoc.LoadXml(information);
                SetSelectedFields(FieldsProp, xmlDoc.FirstChild.Attributes["value"].Value, "SelectedKeyFilterFields");
            }

            metadataNavigationSettings.Add("MetadataNavigationSettings", FieldsProp);
            metadataNavigationSettings["BPOSS"] = true;

            return metadataNavigationSettings;
        }
        public Dictionary<string, object> GetPerLocationViewSettings(string webServerRelativeUrl, Guid listId)
        {
            return new Dictionary<string, object>();
        }
        public Dictionary<string, object> GetDefaultRegionalSetting(string webServerRelativeUrl, int lcid)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/regionalsetng.aspx";
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
            string getUrl = mSiteUrl.TrimEnd('/') + mLayout + "/listkeywords.aspx";
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
            string getUrl = mSiteUrl.TrimEnd('/') + mLayout + "/Keyword.aspx?k=" + keyWordName;
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
                string url = string.Format("{0}" + mLayout + "/BestBet.aspx?u={1}&k={2}&IsDlg=1", mSiteUrl.TrimEnd('/'), bestBetUrl, keyWordName);
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
            string getUrl = mSiteUrl.TrimEnd('/') + mLayout + "/HelpSettings.aspx";
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
            string getUrl = mSiteUrl.TrimEnd('/') + mLayout + "/contenttypesyndicationhubs.aspx";
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
            string getUrl = siteUrl.TrimEnd('/') + mLayout + "/portal.aspx?AjaxDelta=1&isStartPlt1=1344503071152";
            Dictionary<string, object> sitePortal = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string portalUrl = System.Web.HttpUtility.HtmlDecode(AveHttpWebRequestUtility.GetComponentValue(html, "ctl00$PlaceHolderMain$ctl00$ctl02$TxtPortalURL"));
            string portalName = System.Web.HttpUtility.HtmlDecode(AveHttpWebRequestUtility.GetComponentValue(html, "ctl00$PlaceHolderMain$ctl00$ctl03$TxtPortalName"));
            sitePortal.Add("PortalUrl", portalUrl);
            sitePortal.Add("PortalName", portalName);
            return sitePortal;
        }
        public bool GetSiteRssSetting()
        {
            string netWorkUrl = mSiteUrl.TrimEnd('/') + mLayout + "/siterss.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(netWorkUrl, tokenProvider);
            bool allowSiteRss = AveHttpWebRequestUtility.GetCheckInput(html, "ctl00$PlaceHolderMain$SiteColRssSection$ctl01$CheckSiteColRss");
            return allowSiteRss;
        }

        public Dictionary<string, object> GetListAdvancedSettingProperties(string webServerRelativeUrl, Guid listId, SecurityTrimObject mSiteTrimObj)
        {
            Dictionary<string, object> advancedProp = new Dictionary<string, object>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/advsetng.aspx?List={" + listId + "}";
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
            string searchContent = "<input id=\"ctl00_PlaceHolderMain_OpenDocumentSection_ctl01_RadDefaultItemOpenServerSetting\"";
            string information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            if (!string.IsNullOrEmpty(information))
            {
                if (information.Contains("checked=\"checked\""))
                {
                    advancedProp["DefaultItemOpen"] = AveDefaultItemOpen.ServerSetting;
                    advancedProp["DefaultItemOpenUseListSetting"] = false;
                }
                else
                {
                    searchContent = "<input id=\"ctl00_PlaceHolderMain_OpenDocumentSection_ctl01_RadDefaultItemOpenPreferClient\"";
                    information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
                    advancedProp["DefaultItemOpen"] = information.Contains("checked=\"checked\"") ? AveDefaultItemOpen.PreferClient : AveDefaultItemOpen.Browser;
                    advancedProp["DefaultItemOpenUseListSetting"] = true;
                }
            }

            #region Client API Supported
            //searchContent = "<input id=\"ctl00_PlaceHolderMain_ListExperienceSection_ctl02_RadDisplayOnAutoExperience\"";
            //information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            //if (!string.IsNullOrEmpty(information))
            //{
            //    if (information.Contains("checked=\"checked\""))
            //    {
            //        advancedProp["ListExperience"] = AveListExperience.DefaultExperience;
            //    }
            //    else
            //    {
            //        searchContent = "<input id=\"ctl00_PlaceHolderMain_ListExperienceSection_ctl02_RadDisplayOnNewExperience\"";
            //        information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            //        if (!string.IsNullOrEmpty(information))
            //        {
            //            if (information.Contains("checked=\"checked\""))
            //            {
            //                advancedProp["ListExperience"] = AveListExperience.NewExperience;
            //            }
            //            else
            //            {
            //                advancedProp["ListExperience"] = AveListExperience.ClassicExperience;
            //            }
            //        }
            //    }
            //} 
            #endregion

            searchContent = "<input id=\"ctl00_PlaceHolderMain_TasksIssuesEmailSettingsSection_ctl01_RadEnableAssigntoEmailYes\"";
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
            else
            {
                advancedProp["SendToLocationName"] = string.Empty;
                advancedProp["SendToLocationUrl"] = string.Empty;
            }

            searchContent = "<input id=\"ctl00_PlaceHolderMain_ManagedIndexesSection_ctl02_RadManagedIndexesNo\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            advancedProp["EnableManagedIndexes"] = information.Contains("checked=\"checked\"");

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
            string url = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/ListGeneralSettings.aspx?List={" + listId + "}";
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

        public Dictionary<string, object> GetWorkflowAssociations(string webServerRelativeUrl, string listName, Guid listId, string workflowSource, Dictionary<string, object> contentTypeProp)
        {
            Dictionary<string, object> workflowAssociationsProp = new Dictionary<string, object>();
            string getUrl = string.Empty;
            if (workflowSource.Equals("list.workflow"))
            {
                getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/WrkSetng.aspx?List=" + listId;
            }
            else
            {
                AveHttpValueCollection values = new AveHttpValueCollection();
                values["List"] = listId.ToString();
                object contentTypeId;
                if (contentTypeProp.TryGetValue("ContentTypeId", out contentTypeId))
                {
                    values["ctype"] = contentTypeId.ToString();
                }
                getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/WrkSetng.aspx?" + values.ToString(true);
            }
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            string searchContent = "<tr valign=\"top\">";
            int startIndex = html.IndexOf(searchContent, StringComparison.OrdinalIgnoreCase);
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
        public Dictionary<string, object> GetListVersionLimited(string webServerRelativeUrl, Guid listId)
        {
            //没有SecurityTriming，2010于2013可共用这部分代码
            Dictionary<string, object> versionLimitedProp = new Dictionary<string, object>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/LstSetng.aspx?List=" + listId;
            string html = string.Empty;
            html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            XmlDocument xmlDoc = new XmlDocument();

            var hasIssue = false;

            string searchContent = "id=\"onetidMajorVersionLimit\"";
            string information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            information = information.TrimEnd(new char[] { '/', '>' });
            xmlDoc.LoadXml("<version " + information + "></version>");
            var firstChild = xmlDoc.FirstChild as XmlElement;

            if (firstChild == null)
            {
                hasIssue = true;
                versionLimitedProp["MajorVersionLimit"] = 0;
            }
            else
            {
                var intValue = firstChild.GetAttribute("value");
                if (string.IsNullOrEmpty(intValue))
                {
                    versionLimitedProp["MajorVersionLimit"] = 0;
                }
                else
                {
                    versionLimitedProp["MajorVersionLimit"] = Convert.ToInt32(intValue);
                }
            }

            searchContent = "id=\"onetidMajorWithMinorVersionLimit\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            information = information.TrimEnd(new char[] { '/', '>' });
            xmlDoc.LoadXml("<version " + information + "></version>");
            firstChild = xmlDoc.FirstChild as XmlElement;
            if (firstChild == null)
            {
                hasIssue = true;
                versionLimitedProp["MajorWithMinorVersionsLimit"] = 0;
            }
            else
            {
                var intValue = firstChild.GetAttribute("value");
                if (string.IsNullOrEmpty(intValue))
                {
                    versionLimitedProp["MajorWithMinorVersionsLimit"] = 0;
                }
                else
                {
                    versionLimitedProp["MajorWithMinorVersionsLimit"] = Convert.ToInt32(intValue);
                }
            }

            if (hasIssue)
            {
                mLogger.Warn("Cannot get list version limited for list:{0} under web:{1}, result:{2}",
                    listId,
                    webServerRelativeUrl,
                    html);
            }

            return versionLimitedProp;
        }

        public List<Dictionary<string, object>> GetListCheckedOutFiles(string webServerRelativeUrl, Guid listId, int localedId, bool isTime24)
        {
            List<Dictionary<string, object>> checkOutFileProperties = new List<Dictionary<string, object>>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/ManageCheckedOutFiles.aspx?List=" + listId + "";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            string searchContent = "class=\"ms-standardheader\"><b>";
            string information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, "</table>");
            if (!string.IsNullOrEmpty(information))
            {
                information = information.Replace("< 1 KB", "LT1KB");
                AnalyzeXmltoFileInfo(information, checkOutFileProperties, localedId, isTime24);
            }
            return checkOutFileProperties;
        }


        public Dictionary<string, object> GetMetadataListFieldSettings(string webServerRelativeUrl, string listTitle, Guid listId)
        {
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/metadatacolsettings.aspx?List={" + listId + "}";
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
        public bool GetListRated(string webServerRelativeUrl, Guid listId)
        {
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/RatingsSettings.aspx?List={" + listId + "}";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            string searchContent = "ctl00$PlaceHolderMain$ctl00$ctl03$EnableRatings";
            bool rating = AveHttpWebRequestUtility.GetCheckInput(html, searchContent);
            return rating;//SAAS-1064
        }
        public string GetListExperience(string webServerRelativeUrl, Guid listId)
        {
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/RatingsSettings.aspx?List={" + listId + "}";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider);
            string likesOrRatings = AveHttpWebRequestUtility.GetInputControlValue(html, "//input[@type='radio'][@checked='checked'][@name='ctl00$PlaceHolderMain$ctl00$ctl04$VotingExperience']");
            if (string.IsNullOrEmpty(likesOrRatings))
            {
                return null;
            }
            else
            {
                return likesOrRatings.Equals("RadVotingExpLikes", StringComparison.OrdinalIgnoreCase) ? "Likes" : "Ratings";
            }
        }
        public Dictionary<string, object> GetListRssProperties(string webServerRelativeUrl, Guid listId)
        {
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/listsyndication.aspx?List={" + listId + "}";
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
                folderProp["vti_rss_ChannelTitle"] = xmlDoc.FirstChild.Attributes["value"] == null ? string.Empty : xmlDoc.FirstChild.Attributes["value"].Value;
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
                folderProp["vti_rss_ChannelImageUrl"] = xmlDoc.FirstChild.Attributes["value"] == null ? string.Empty : xmlDoc.FirstChild.Attributes["value"].Value;
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
        public void GetManagedSiteCollectionData(Dictionary<string, object> managedData, string adminUrl, long availableStorageQuota, double availableResourceQuota)
        {
            var languageList = new List<IDictionary<string, object>>();
            Dictionary<string, object> languages = new Dictionary<string, object>();

            var prefixList = new List<IDictionary<string, object>>();
            Dictionary<string, object> prefixs = new Dictionary<string, object>();

            string manageUrl = string.Format("{0}/_layouts/15/online/SiteCollections.aspx", adminUrl.TrimEnd('/'));
            string createUrl = string.Format("{0}/_layouts/15/online/CreateSite.aspx?IsDlg=1", adminUrl.TrimEnd('/'));
            string searchContent = "<input type=\"hidden\"";
            Dictionary<string, object> managedBodyDic = new Dictionary<string, object>();
            string html1 = AveHttpWebRequestUtility.HttpGet(manageUrl, tokenProvider);
            AveHttpWebRequestUtility.GetInput(html1, searchContent, managedBodyDic);
            Dictionary<string, object> createBodyDic = new Dictionary<string, object>();
            createBodyDic["hidParam"] = "undefined";
            createBodyDic["hidParam2"] = "undefined";
            createBodyDic["hidParam3"] = availableStorageQuota;
            createBodyDic["hidParam4"] = availableResourceQuota;
            createBodyDic["__REQUESTDIGEST"] = HttpUtility.UrlEncode(managedBodyDic["__REQUESTDIGEST"].ToString());
            createBodyDic["submit"] = "Submit Query";
            byte[] body2 = AveHttpWebRequestUtility.GetByte(createBodyDic, null);
            string html2 = AveHttpWebRequestUtility.HttpReturn(createUrl, tokenProvider, "application/x-www-form-urlencoded", body2, null);
            searchContent = "<select name=\"ctl00$PlaceHolderMain$tpcCreateSite$ctl00$DDLanguageFormControl$DdLanguageWebTemplate\"";
            string languageContent = AveHttpWebRequestUtility.GetInput(html2, searchContent, "</select>");
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(languageContent);
            foreach (XmlNode node in xml.SelectNodes("/select/option"))
            {
                Dictionary<string, object> language = new Dictionary<string, object>();
                language.Add("DisplayName", node.InnerText);
                language.Add("LCID", Convert.ToInt32(node.Attributes["value"].Value));
                languageList.Add(language);
            }
            languages.AddChildren(languageList);
            managedData["InstalledLanguages" + AveObjectModelConstant.ObjectPropertySuffix] = languages;

            searchContent = "<select name=\"ctl00$PlaceHolderMain$ifsAddress$ifcAddress$ddlManagedPathList\"";
            string prefixContent = AveHttpWebRequestUtility.GetInput(html2, searchContent, "</select>");
            xml.LoadXml(prefixContent);
            foreach (XmlNode node in xml.SelectNodes("/select/option"))
            {
                Dictionary<string, object> prefix = new Dictionary<string, object>();
                prefix.Add("Name", node.InnerText);
                prefixList.Add(prefix);
            }
            prefixs.AddChildren(prefixList);
            managedData["Prefixes" + AveObjectModelConstant.ObjectPropertySuffix] = prefixs;
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
        public int GetAuditFlags()
        {
            string postUrl = mSiteUrl.TrimEnd('/') + "/_layouts/15/auditsettings.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);

            Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(html);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input ";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            string edit = "ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditEdit";
            string checkInOut = "ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditCheckInOut";
            string moveCopy = "ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditMoveCopy";
            string deleteRestore = "ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditDeleteRestore";
            string columnsContentType = "ctl00$PlaceHolderMain$ctl02$ctl01$CheckBoxAuditColumnsContentType";
            string search = "ctl00$PlaceHolderMain$ctl02$ctl01$CheckBoxAuditSearch";
            string perms = "ctl00$PlaceHolderMain$ctl02$ctl01$CheckBoxAuditPerms";
            int flag = 0;
            string tmp = string.Empty;
            int index = html.IndexOf(edit) + edit.Length + 1;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 16;
            }
            index = html.IndexOf(checkInOut) + checkInOut.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 3;
            }
            index = html.IndexOf(moveCopy) + moveCopy.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 6144;
            }
            index = html.IndexOf(deleteRestore) + deleteRestore.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 520;
            }
            index = html.IndexOf(columnsContentType) + columnsContentType.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 160;
            }
            index = html.IndexOf(search) + search.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 8192;
            }
            index = html.IndexOf(perms) + perms.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 256;
            }
            return flag;
        }

        public Dictionary<string, object> GetThemeUrlForWeb(string webServerRelativeUrl)
        {
            return new Dictionary<string, object>();
        }

        #endregion

        #region Update
        public void UpdateWebLogo(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/prjsetng.aspx?AjaxDelta=1";
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
                bodyDic["ctl00$PlaceHolderMain$logoSection$ctl03$TxtSiteLogoUrl"] = HttpUtility.UrlEncode(webProperties["SiteLogoUrl"] == null ? string.Empty : webProperties["SiteLogoUrl"].ToString());
            }
            if (webProperties.ContainsKey("SiteLogoDescription"))
            {
                bodyDic["ctl00$PlaceHolderMain$logoSection$ctl04$TxtLogoUrlDescription"] = HttpUtility.UrlEncode(webProperties["SiteLogoDescription"] == null ? string.Empty : webProperties["SiteLogoDescription"].ToString());
            }
            if (webProperties.ContainsKey("Name"))
            {
                bodyDic["ctl00$PlaceHolderMain$idUrlSection$ctl03$TxtCreateSubwebName"] = HttpUtility.UrlEncode(webProperties["Name"] == null ? string.Empty : webProperties["Name"].ToString());
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }

        [Obsolete]
        public void UpdateWebSearchAndOfflineAvailability(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/srchvis.aspx";
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
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/regionalsetng.aspx?AjaxDelta=1";
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
        public Dictionary<string, object> UpdateKeyWord(string term, int localId, int calendarType, Dictionary<string, object> keyWordProp)
        {
            string url = mSiteUrl.TrimEnd('/') + string.Format(mLayout + "/Keyword.aspx?k={0}", term);
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
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/mngsiteadmin.aspx";
            Dictionary<string, object> siteAdmins = new Dictionary<string, object>();
            StringBuilder strBuild = new StringBuilder("[");
            foreach (var dic in newAdmins)
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
        public Dictionary<string, object> UpdateSitePortal(Dictionary<string, object> siteProperties)
        {
            Dictionary<string, object> sitePortal = new Dictionary<string, object>();
            string postUrl = mSiteUrl.TrimEnd('/') + mLayout + "/portal.aspx";
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
            string netWorkUrl = mSiteUrl + mLayout + "/siterss.aspx";
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
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/advsetng.aspx?List={" + listId + "}";
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
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/ListGeneralSettings.aspx?List={" + listId + "}";
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
            mRequestCommon.SetListVersionLimited(webServerRelativeUrl, listId, versionLimitedProperties);
        }
        public void MoveNavigationNodeToCollection(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties)
        {
            try
            {
                string postUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.Trim('/');
                int nodeId = (int)navigationNodeProperties["NodeId"];
                postUrl = postUrl + string.Format(mLayout + "/editnav.aspx?ID={0}", nodeId);
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
            mRequestCommon.MoveNavigationNode(webServerRelativeUrl, navigationNodeProperties, previousNodeProperties, moveMethodName);
        }
        public bool RestoreNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties, AveNavigationInfoList navigationList)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/AreaNavigationSettings.aspx";
            Ave2013NavigationInfo navigationInfo = navigationList as Ave2013NavigationInfo;
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html, false);
            bodyDic.Remove("ctl00$PlaceHolderMain$ctl07$ctl01$managedCreateTermSetButton");
            bodyDic.Remove("ctl00$PlaceHolderMain$ctl05$RptControls$bottomCancelButton");
            bodyDic.Remove("ctl00$PlaceHolderMain$ctl05$RptControls$bottomOKButton");
            bodyDic.Remove("ctl00$PlaceHolderMain$ctl01$RptControls$topCancelButton");
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            nodes = HttpUtility.UrlEncode(nodes);
            mLogger.Info("Post navigation nodes for web {0}.{1}", webServerRelativeUrl,nodes);
            if (!bodyDic.ContainsKey("nodes"))
            {
                bodyDic["nodes"] = nodes;
            }
            else if (bodyDic.ContainsKey("nodes") && (!bodyDic["nodes"].ToString().Equals(nodes)))
            {
                bodyDic["nodes"] = nodes;
                bodyDic["ctl00$PlaceHolderMain$ctl05$RptControls$bottomOKButton"] = "OK";
            }

            if (webAllProperties.ContainsKey("__GlobalNavigationIncludeTypes"))
            {
                if (webAllProperties["__GlobalNavigationIncludeTypes"].ToString().Equals("1"))
                {
                    bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl03$globalIncludeSubSites"] = "on";
                }
                else if (webAllProperties["__GlobalNavigationIncludeTypes"].ToString().Equals("2"))
                {
                    bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl03$globalIncludePages"] = "on";
                }
                else if (webAllProperties["__GlobalNavigationIncludeTypes"].ToString().Equals("3"))
                {
                    bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl03$globalIncludeSubSites"] = "on";
                    bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl03$globalIncludePages"] = "on";
                }
            }
            if (webAllProperties.ContainsKey("__CurrentNavigationIncludeTypes"))
            {
                if (webAllProperties["__CurrentNavigationIncludeTypes"].ToString().Equals("1"))
                {
                    bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl03$currentIncludeSubSites"] = "on";
                }
                else if (webAllProperties["__CurrentNavigationIncludeTypes"].ToString().Equals("2"))
                {
                    bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl03$currentIncludePages"] = "on";
                }
                else if (webAllProperties["__CurrentNavigationIncludeTypes"].ToString().Equals("3"))
                {
                    bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl03$currentIncludeSubSites"] = "on";
                    bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl03$currentIncludePages"] = "on";
                }
            }
            if (webAllProperties.ContainsKey("__GlobalDynamicChildLimit"))
            {
                bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl03$globalDynamicChildLimit"] = webAllProperties["__GlobalDynamicChildLimit"];
            }
            if (webAllProperties.ContainsKey("__CurrentDynamicChildLimit"))
            {
                //bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl03$globalDynamicChildLimit"] = webAllProperties["__CurrentDynamicChildLimit"];
                bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl03$currentDynamicChildLimit"] = webAllProperties["__CurrentDynamicChildLimit"];
            }
            if (webAllProperties.ContainsKey("__NavigationOrderingMethod") && webAllProperties["__NavigationOrderingMethod"].ToString().Equals("0"))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl08$SortingMethodRadioGroup"] = "automaticSortingRadioButton";
                if (webAllProperties.ContainsKey("__NavigationSortAscending"))
                {
                    bool sortAscending = Convert.ToBoolean(webAllProperties["__NavigationSortAscending"]);
                    if (sortAscending)
                    {
                        bodyDic["ctl00$PlaceHolderMain$automaticSortingSection$SortingDirectionRadioGroup"] = "ascendingRadioButton";
                    }
                    else
                    {
                        bodyDic["ctl00$PlaceHolderMain$automaticSortingSection$SortingDirectionRadioGroup"] = "descendingRadioButton";
                    }
                }
                if (webAllProperties.ContainsKey("__NavigationAutomaticSortingMethod"))
                {
                    string method = webAllProperties["__NavigationAutomaticSortingMethod"].ToString();
                    if (method.Equals("0"))
                    {
                        bodyDic["ctl00$PlaceHolderMain$automaticSortingSection$automaticSortingMethodDropDown"] = "Title";
                    }
                    else if (method.Equals("1"))
                    {
                        bodyDic["ctl00$PlaceHolderMain$automaticSortingSection$automaticSortingMethodDropDown"] = "CreatedDate";
                    }
                    else if (method.Equals("2"))
                    {
                        bodyDic["ctl00$PlaceHolderMain$automaticSortingSection$automaticSortingMethodDropDown"] = "LastModifiedDate";
                    }
                }
            }
            else
            {
                bodyDic["ctl00$PlaceHolderMain$ctl08$SortingMethodRadioGroup"] = "manualSortingRadioButton";
            }
            if (webAllProperties.ContainsKey("__DisplayShowHideRibbonActionId"))
            {
                bool ribbon = Convert.ToBoolean(webAllProperties["__DisplayShowHideRibbonActionId"]);
                if (ribbon)
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl02$DisplayShowHideRibbonActionMethodRadioGroup"] = "displayShowHideRibbonActionRadioButtonOptionYes";
                }
                else
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl02$DisplayShowHideRibbonActionMethodRadioGroup"] = "displayShowHideRibbonActionRadioButtonOptionNo";
                }
            }
            if (navigationInfo == null)
            {
                var checkedControls = AveHttpWebRequestUtility.LoadCheckedControls(html,true);
                if (checkedControls.Contains("ctl00$PlaceHolderMain$newPageOptionsSection$ctl01$newPageNavItemCheckBox",StringComparer.OrdinalIgnoreCase))
                {
                    bodyDic["ctl00$PlaceHolderMain$newPageOptionsSection$ctl01$newPageNavItemCheckBox"] = "on";
                }
                if (checkedControls.Contains("ctl00$PlaceHolderMain$newPageOptionsSection$ctl01$newPageFriendlyUrlCheckBox", StringComparer.OrdinalIgnoreCase))
                {
                    bodyDic["ctl00$PlaceHolderMain$newPageOptionsSection$ctl01$newPageFriendlyUrlCheckBox"] = "on";
                }
            }
            if (navigationInfo != null)
            {
                if (navigationInfo.GlobalNavigation.Source == AveStandardNavigationSource.TaxonomyProvider)
                {
                    bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl03$TopNavInheritance"] = "managedTopNavRadioButton";
                    bodyDic["ctl00$PlaceHolderMain$ctl07$ctl01$managedTaxonomyItemPicker$taxonomyItemPickerTermStoreId"] = navigationInfo.GlobalNavigation.TermStoreId.ToString("d");
                    bodyDic["ctl00$PlaceHolderMain$ctl07$ctl01$managedTaxonomyItemPicker$taxonomyItemPickerTermSetId"] = navigationInfo.GlobalNavigation.TermSetId.ToString("d");
                    bodyDic["ctl00$PlaceHolderMain$ctl07$ctl01$managedTaxonomyItemPicker$taxonomyItemPickerGroupId"] = navigationInfo.GlobalNavigation.TermGroupId.ToString("d");
                }
                else if (webAllProperties.ContainsKey("UseShared"))
                {
                    bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl03$TopNavInheritance"] = "inheritTopNavRadioButton";
                }
                if (navigationInfo.CurrentNavigation.Source == AveStandardNavigationSource.TaxonomyProvider)
                {
                    bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl03$LeftNavInheritance"] = "managedLeftNavRadioButton";
                    bodyDic["ctl00$PlaceHolderMain$ctl07$ctl01$managedTaxonomyItemPicker$taxonomyItemPickerTermStoreId"] = navigationInfo.CurrentNavigation.TermStoreId.ToString("d");
                    bodyDic["ctl00$PlaceHolderMain$ctl07$ctl01$managedTaxonomyItemPicker$taxonomyItemPickerTermSetId"] = navigationInfo.CurrentNavigation.TermSetId.ToString("d");
                    bodyDic["ctl00$PlaceHolderMain$ctl07$ctl01$managedTaxonomyItemPicker$taxonomyItemPickerGroupId"] = navigationInfo.CurrentNavigation.TermGroupId.ToString("d");
                }

                if (navigationInfo.CurrentNavigation.Source == AveStandardNavigationSource.TaxonomyProvider 
                    || navigationInfo.GlobalNavigation.Source == AveStandardNavigationSource.TaxonomyProvider
                    || webAllProperties.ContainsKey("UseShared"))
                {
                    if (navigationInfo.AddNewPagesToNavigation)
                    {
                        bodyDic["ctl00$PlaceHolderMain$newPageOptionsSection$ctl01$newPageNavItemCheckBox"] = "on";
                    }
                    if (navigationInfo.CreateFriendlyUrlsForNewPages)
                    {
                        bodyDic["ctl00$PlaceHolderMain$newPageOptionsSection$ctl01$newPageFriendlyUrlCheckBox"] = "on";
                    }

                }
            }
            else if (webAllProperties.ContainsKey("UseShared"))
            {
                bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl03$TopNavInheritance"] = "inheritTopNavRadioButton";
            }
            if (!bodyDic.ContainsKey("ctl00$PlaceHolderMain$currentNavSection$ctl03$LeftNavInheritance"))
            {
                if (webAllProperties.ContainsKey("__NavigationShowSiblings"))
                {
                    bool showSiblings = Convert.ToBoolean(webAllProperties["__NavigationShowSiblings"]);
                    if (showSiblings)
                    {
                        bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl03$LeftNavInheritance"] = "showSiblingsLeftNavRadioButton";
                    }
                }
                if (webAllProperties.ContainsKey("__InheritCurrentNavigation"))
                {
                    bool inheritCurrentNavigation = Convert.ToBoolean(webAllProperties["__InheritCurrentNavigation"]);
                    if (inheritCurrentNavigation)
                    {
                        bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl03$LeftNavInheritance"] = "inheritLeftNavRadioButton";
                    }
                    else if (!bodyDic.ContainsKey("ctl00$PlaceHolderMain$currentNavSection$ctl03$LeftNavInheritance"))//SAAS-3573
                    {
                        bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl03$LeftNavInheritance"] = "uniqueLeftNavRadioButton";
                    }
                }
            }
            bodyDic.Remove(string.Empty);
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
            return true;
        }
        public bool RestoreSearchNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/EnhancedSearch.aspx?level=site";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html, false);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            nodes = HttpUtility.UrlEncode(nodes);
            if (!bodyDic.ContainsKey("nodes"))
            {
                bodyDic["nodes"] = nodes;
            }
            else if (bodyDic.ContainsKey("nodes") && (!bodyDic["nodes"].ToString().Equals(nodes)))
            {
                bodyDic["nodes"] = nodes;
                bodyDic["ctl00$PlaceHolderMain$cmdOK"] = "OK";
            }
            if (webAllProperties.ContainsKey("SRCH_SB_SET_WEB"))
            {
                Dictionary<string, object> propertys = JsonConvert.DeserializeObject<Dictionary<string, object>>(webAllProperties["SRCH_SB_SET_WEB"].ToString());
                if (propertys.ContainsKey("Inherit"))
                {
                    if (propertys["Inherit"].ToString().ToLower().Equals("true"))
                    {
                        bodyDic["ctl00$PlaceHolderMain$inheritSettings"] = "on";
                    }
                }
            }
            if (webAllProperties.ContainsKey("SRCH_VERT_SET_WEB"))
            {
                Dictionary<string, object> propertys = JsonConvert.DeserializeObject<Dictionary<string, object>>(webAllProperties["SRCH_VERT_SET_WEB"].ToString());
                if (propertys.ContainsKey("Inherit"))
                {
                    if (propertys["Inherit"].ToString().ToLower().Equals("true"))
                    {
                        bodyDic["ctl00$PlaceHolderMain$inheritSearchVerticalsSettings"] = "on";
                    }
                }
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
            return true;
        }
        public void UpdateMetadataListFieldSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/metadatacolsettings.aspx?List={" + listId + "}";
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

        public bool SetListRateSetting(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating, bool isLikesExp)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/RatingsSettings.aspx?List={" + listId + "}";
            string html = SetRatedSetting(postUrl, enableRating);
            if (enableRating)
            {
                SetVotingExpSetting(postUrl, html, isLikesExp);
            }
            return true;
        }
        private string SetRatedSetting(string postUrl, bool enableRating)
        {
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html, false);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            byte[] body = null;
            if (enableRating)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl03$EnableRatings"] = "RadEnableRatingsYes";
                bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl00$ctl03$RadEnableRatingsYes";
                body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
                html = AveHttpWebRequestUtility.HttpReturn(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
            }
            else
            {
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl03$EnableRatings"] = "RadEnableRatingsNo";
                bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl00$ctl03$RadEnableRatingsNo";
                bodyDic["ctl00$PlaceHolderMain$ctl01$RptControls$BtnSave"] = "OK";
                body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
                AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
            }
            return html;
        }
        private void SetVotingExpSetting(string postUrl, string html, bool isLikesExp)
        {
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html, false);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["ctl00$PlaceHolderMain$ctl00$ctl03$EnableRatings"] = "RadEnableRatingsYes";
            bodyDic["ctl00$PlaceHolderMain$ctl00$ctl04$VotingExperience"] = isLikesExp ? "RadVotingExpLikes" : "RadVotingExpRatings";
            bodyDic["ctl00$PlaceHolderMain$ctl01$RptControls$BtnSave"] = "OK";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }
        public void UpdateListRssSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProp)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/listsyndication.aspx?List={" + listId + "}";
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
        public void SetSiteEnabledHelpCollections(string[] enabledHelpCollections)
        {
            string postUrl = mSiteUrl.TrimEnd('/') + mLayout + "/HelpSettings.aspx";
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

        public List<Dictionary<string, object>> RestoreFeatures(string webServerRelativeUrl, bool force, int scope, string featuresSource, List<Dictionary<string, object>> featureInfoList, object context, object web)
        {
            List<Dictionary<string, object>> featuresProperties = new List<Dictionary<string, object>>();
            foreach (Dictionary<string, object> featureInfo in featureInfoList)
            {
                try
                {
                    foreach (Guid id in featureInfo["Dependences"] as List<Guid>)
                    {
                        string tempFeatureSource = featuresSource;
                        if (featureInfo.ContainsKey("FeatureSource") && featureInfo["FeatureSource"] != null)
                        {
                            tempFeatureSource = featureInfo["FeatureSource"].ToString();
                        }
                        this.RetryRestoreFeature(context as ClientContext, web as Web, webServerRelativeUrl, id, force, scope, tempFeatureSource);
                    }
                    Dictionary<string, object> featureProp = new Dictionary<string, object>();
                    Guid featureId = new Guid(featureInfo["ID"].ToString());
                    featureProp = this.RetryRestoreFeature(context as ClientContext, web as Web, webServerRelativeUrl, featureId, force, scope, featuresSource);
                    featuresProperties.Add(featureProp);
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    mLogger.Error("Add Feature to {0}:{1} failed.Error Message:{2}", featuresSource, webServerRelativeUrl, ex.ToString());
                }
            }
            return featuresProperties;
        }

        private Dictionary<string, object> RetryRestoreFeature(ClientContext context, Web web, string webServerRelativeUrl, Guid featureId, bool force, int scope, string featuresSource)
        {
            var defaultScope = FeatureDefinitionScope.Farm;
            for(int i = 0; i < 3; i++)
            {
                try
                {
                    Dictionary<string, object> featureProp = null;
                    //有的客户自定义的feature是hidden状态，这种feature不会在页面上显示出来，但可以配置在site的template文件里激活，hidden的feature是没有办法用post方式激活的。
                    if (IsFeatureActivated(webServerRelativeUrl, featureId, featuresSource) == FeatureStatus.Hidden)
                    {
                        try
                        {
                            featureProp = new Dictionary<string, object>();
                            switch (featuresSource)
                            {
                                case "web.features":
                                    if (i >= 1) { defaultScope = FeatureDefinitionScope.Web; }
                                    var webFeature = web.Features.Add(featureId, true, defaultScope);
                                    context.Load(webFeature);
                                    break;
                                case "site.features":
                                    if (i >= 1) { defaultScope = FeatureDefinitionScope.Site; }
                                    var siteFeature = context.Site.Features.Add(featureId, true, defaultScope);
                                    context.Load(siteFeature);
                                    break;
                            }

                            context.ExecuteQuery();
                            featureProp["DefinitionId"] = featureId;
                            Dictionary<string, object> featureDefinitionProperties = new Dictionary<string, object>();
                            featureProp["Definition" + AveObjectModelConstant.ObjectPropertySuffix] = featureDefinitionProperties;
                        }
                        catch (Exception e)
                        {
                            mLogger.Error("failed to activate feature: {0} with scope {1} due to: {2}", featureId, defaultScope, e.ToString());
                        }
                        return featureProp;
                    }
                    featureProp = RestoreFeature(webServerRelativeUrl, featureId, force, scope, featuresSource);
                    if (IsFeatureActivated(webServerRelativeUrl, featureId, featuresSource) == FeatureStatus.Active)
                    {
                        return featureProp;
                    }
                }
                catch (System.Net.WebException we)
                {
                    if (we.Response != null && (we.Response as System.Net.HttpWebResponse).StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                    {
                        if (i < 3)
                        {
                            System.Threading.Thread.Sleep(2000);
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return null;
        }

        public void RestoreMasterPage(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebMasterPageInfo pageInfo, string alternateCssUrl)
        {
            mLogger.Info("Begin to restore web masterpage,weburl:{0}", webServerRelativeUrl);
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/ChangeSiteMasterPage.aspx";
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
                mLogger.Info("Begin update CustomMasterPage Url {0}, in web {1}.", pageInfo.CPageUrl, webServerRelativeUrl);
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl01$InheritSiteMasterRadioGroup"] = "selectChromeRadioButton";
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl01$masterPageSelectionControl$ctl00$SiteMasterPageDropDownList"] = pageInfo.CPageUrl;
            }
            else
            {
                return;     //Source didn't backup masterpage info at all
            }
            if (pageInfo.MInheriting)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl01$ctl01$InheritSystemMasterPageGroup"] = "inheritSystemMasterPageRadioButton";
            }
            else if (!string.IsNullOrEmpty(pageInfo.MPageUrl))
            {
                mLogger.Info("Begin update MasterPage Url {0}, in web {1}.", pageInfo.MPageUrl, webServerRelativeUrl);
                bodyDic["ctl00$PlaceHolderMain$ctl01$ctl01$InheritSystemMasterPageGroup"] = "selectSystemMasterPageRadioButton";
                bodyDic["ctl00$PlaceHolderMain$ctl01$ctl01$systemMasterPageSelectionControl$ctl00$SystemMasterPageDropDownList"] = pageInfo.MPageUrl;
            }
            else
            {
                return;     //Source didn't backup masterpage info at all
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

            var checkedNodeList = AveHttpWebRequestUtility.LoadCheckedControls(html, true);
            if (checkedNodeList != null && checkedNodeList.Contains("ctl00$PlaceHolderMain$ctl02$ctl01$inheritThemeCheckbox", StringComparer.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl02$ctl01$inheritThemeCheckbox"] = "on";
            }

            bodyDic["ctl00$PlaceHolderMain$ctl04$RptControls$ButtonSaveSettings"] = "OK";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }
        public void OperateOnVersion(string webServerRelativeUrl, string webAppName, ITokenProvider tokenProvider, string listUrl, int itemId, int versionId, string listId, string fileName, string op)
        {
            AveHttpWebRequestCommon.OperateOnVersion(webServerRelativeUrl, webAppName, tokenProvider, listUrl, itemId, versionId, listId, fileName, op);
        }

        public void ResetPersonalizationState(string webServerRelativeUrl, string fileServerRelativeUrl, Guid webpartId)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/spcontnt.aspx?pageView=Shared&url=" + HttpUtility.UrlEncode(fileServerRelativeUrl);
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            string webpartIdStr = webpartId.ToString("d").Replace('-', '_');
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html, true);
            bodyDic["hdnToolbarAction"] = 2; //2 means resetdefaults
            bodyDic["ctl00$PlaceHolderMain$g_" + webpartIdStr] = "on";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }

        public Dictionary<string, object> UpdateAudit(Dictionary<string, object> needUpdateProperties)
        {
            //13 model
            if (needUpdateProperties.ContainsKey("AuditFlags"))
            {
                int auditFlags = (int)needUpdateProperties["AuditFlags"];
                string postUrl = mSiteUrl.TrimEnd('/') + "/_layouts/15/auditsettings.aspx";
                string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);

                Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(html);
                Dictionary<string, object> bodyDic = new Dictionary<string, object>();
                string searchContent = "<input ";
                AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);

                if (bodyDic.ContainsKey("__EVENTVALIDATION"))
                {
                    bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
                }
                if (bodyDic.ContainsKey("__VIEWSTATE"))
                {
                    bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
                }
                string isEnable = (string)formValues["ctl00$PlaceHolderMain$ctl00$ctl04$trimAuditLog"];
                if (isEnable.Equals("RadTrimAuditLogYes", StringComparison.OrdinalIgnoreCase))
                {
                    //"RadTrimAuditLogYes"
                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl04$trimAuditLog"] = formValues["ctl00$PlaceHolderMain$ctl00$ctl04$trimAuditLog"];
                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl05$TxtTrimRetention"] = formValues["ctl00$PlaceHolderMain$ctl00$ctl05$TxtTrimRetention"];
                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl06$TxtReportStorageLocation"] = formValues["ctl00$PlaceHolderMain$ctl00$ctl06$TxtReportStorageLocation"];
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl03$RptControls$BtnCancelAuditSettings");
                }
                else
                {
                    //RadTrimAuditLogNo
                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl04$trimAuditLog"] = formValues["ctl00$PlaceHolderMain$ctl00$ctl04$trimAuditLog"];
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl00$ctl05$TxtTrimRetention");
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl00$ctl06$TxtReportStorageLocation");
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl03$RptControls$BtnCancelAuditSettings");
                }
                #region convert flags
                if ((auditFlags & (int)AveAuditMaskType.Update) > 0)
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditEdit"] = "on";
                }
                else
                {
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditEdit");
                }
                if ((auditFlags & (int)AveAuditMaskType.CheckIn) > 0)
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditCheckInOut"] = "on";
                }
                else
                {
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditCheckInOut");
                }
                if ((auditFlags & (int)AveAuditMaskType.Copy) > 0)
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditMoveCopy"] = "on";
                }
                else
                {
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditMoveCopy");
                }
                if ((auditFlags & (int)AveAuditMaskType.Delete) > 0)
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditDeleteRestore"] = "on";
                }
                else
                {
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditDeleteRestore");
                }
                if ((auditFlags & (int)AveAuditMaskType.SchemaChange) > 0)
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl02$ctl01$CheckBoxAuditColumnsContentType"] = "on";
                }
                else
                {
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl02$ctl01$CheckBoxAuditColumnsContentType");
                }
                if ((auditFlags & (int)AveAuditMaskType.Search) > 0)
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl02$ctl01$CheckBoxAuditSearch"] = "on";
                }
                else
                {
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl02$ctl01$CheckBoxAuditSearch");
                }
                if ((auditFlags & (int)AveAuditMaskType.SecurityChange) > 0)
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl02$ctl01$CheckBoxAuditPerms"] = "on";
                }
                else
                {
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl02$ctl01$CheckBoxAuditPerms");
                }
                bodyDic.Remove("ctl00$PlaceHolderMain$ctl03$RptControls$BtnCancelAuditSettings");
                bodyDic.Remove("");

                #endregion
                bodyDic["ctl00$PlaceHolderMain$ctl03$RptControls$BtnUpdateAuditSettings"] = "OK";

                byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
                AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
            }
            return needUpdateProperties;
        }

        public void UpdateWorkflowAssociationsOnChildren(string webUrl, string contentTypeId)
        {
            string postUrl = webUrl + mLayout + "/WrkSetng.aspx?" + "ctype=" + contentTypeId;
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
            //SAAS-11031 当前value已经是转义过的了，‘/’已经转义为‘%2f’，再转义会把‘%’转义成‘%25’
            //if (bodyDic.ContainsKey("__VIEWSTATE"))
            //{
            //    bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            //}
            //if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            //{
            //    bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            //}
            bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$lbUpdate";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }
        #endregion

        #region Add

        public Dictionary<string, object> AddBestBet(string term, List<string> bestBetUrlList, Dictionary<string, object> bestBetProp, string action)
        {
            string url = mSiteUrl.TrimEnd('/') + string.Format(mLayout + "/Keyword.aspx?k={0}", term);
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
            string url = mSiteUrl.TrimEnd('/') + string.Format(mLayout + "/BestBet.aspx?k={0}&IsDlg=1", term);
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
            string url = mSiteUrl.TrimEnd('/') + string.Format(mLayout + "/BestBet.aspx?k={0}&IsDlg=1", term);
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
            string url = mSiteUrl.TrimEnd('/') + mLayout + "/BestBet.aspx?";
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

        public Dictionary<string, object> AddKeyWord(string term, DateTime startDate, int localId, int calendarType)
        {
            string url = mSiteUrl.TrimEnd('/') + mLayout + "/Keyword.aspx";
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
            string url = mSiteUrl.TrimEnd('/') + string.Format(mLayout + "/Keyword.aspx?k={0}", term);
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

        public Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> data)
        {
            Dictionary<string, object> featureProp = new Dictionary<string, object>();
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            string realListUrl = this.WebAppName.TrimEnd('/') + listUrl;
            string netWorkUrl = string.Empty;
            if (itemId == -2)
            {
                netWorkUrl = webFullUrl.TrimEnd('/') + mLayout + "/SubNew.aspx?List={" + listId.ToString() + "}";//&Source=" +realListUrl + "?AjaxDelta=1&IsDlg=1";
            }
            else
            {
                netWorkUrl = webFullUrl.TrimEnd('/') + mLayout + "/SubNew.aspx?List={" + listId.ToString() + "}&ID=" + itemId.ToString();// +"&Source=" + realListUrl + "?IsDlg=1";
            }
            string html = AveHttpWebRequestUtility.HttpGet(netWorkUrl, this.tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            Dictionary<string, object> buttonDic = new Dictionary<string, object>();
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"button\"", buttonDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            foreach (string key in buttonDic.Keys)
            {
                if (key.EndsWith("CreateAlert", StringComparison.OrdinalIgnoreCase))
                {
                    bodyDic["__EVENTTARGET"] = key.ToString();
                    break;
                }
            }
            bodyDic["ctl00$PlaceHolderMain$ctl03$ctl01$TextTitle"] = data["AlertTitle"].ToString();
            bodyDic["ctl00$PlaceHolderMain$ctl05$ctl02$rdoDC"] = "rdo_EmailDC";
            bodyDic["ctl00$PlaceHolderMain$ctl07$ctl02$RadioBtnAlertFilter"] = AveHttpWebRequestUtility.GetFilterValue(data);
            if (data.ContainsKey("ViewId"))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl06$ctl02$DdlView"] = data["ViewId"].ToString();
            }
            bodyDic["ctl00$PlaceHolderMain$ctl06$ctl01$RadioBtnEventType"] = (int)data["EventType"];
            bodyDic["ctl00$PlaceHolderMain$hdnAlwaysNotify"] = "False";
            AveHttpWebRequestUtility.UpateAlertTimeProperties(data, html);
            bodyDic["ctl00$PlaceHolderMain$ctl08$ctl02$RadioBtnAlertFreq"] = (int)data["NotifyFreq"];
            if ((int)data["NotifyFreq"] == 1)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl08$ctl03$DdlHour"] = data["Time"].ToString();
            }
            else if ((int)data["NotifyFreq"] == 2)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl08$ctl03$DdlWeekDay"] = data["Day"].ToString();
                bodyDic["ctl00$PlaceHolderMain$ctl08$ctl03$DdlHour"] = data["Time"].ToString();
            }
            Dictionary<string, object> userInfo = data["User"] as Dictionary<string, object>;
            string userName = userInfo["Name"] as string;
            userName = userName.Replace("\\", "\\\\");  //目的端是模拟Office365时候，须作此操作。
            bodyDic["ctl00$PlaceHolderMain$ctl04$ctl01$clientPeoplePicker"] = "[{'Key':'" + userName + "','IsResolved':true}]";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            string contentType = "application/x-www-form-urlencoded";
            AveHttpWebRequestUtility.HttpPost(netWorkUrl, this.tokenProvider, contentType, body, null);
            Dictionary<string, object> featureDefinitionProperties = new Dictionary<string, object>();
            featureProp["Definition" + AveObjectModelConstant.ObjectPropertySuffix] = featureDefinitionProperties;
            return featureProp;
        }
        public Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, IAveListTemplate listTemplate)
        {
            StringBuilder postUrl = new StringBuilder();
            postUrl.Append(WebAppName + webServerRelativeUrl.TrimEnd('/'));
            postUrl.Append(String.Format("/_layouts/15/new.aspx?CustomTemplate={0}", listTemplate.InternalName));
            postUrl.Append(String.Format("&FeatureId={0}", listTemplate.FeatureId.ToString("B")));
            postUrl.Append(String.Format("&ListTemplate={0}&", listTemplate.Type_Client));
            string html = AveHttpWebRequestUtility.HttpGet(postUrl.ToString(), tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            if (bodyDic.ContainsKey("__REQUESTDIGEST"))
            {
                bodyDic["__REQUESTDIGEST"] = HttpUtility.UrlEncode(bodyDic["__REQUESTDIGEST"].ToString());
            }
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["Title"] = title;
            bodyDic["Cmd"] = "NewList";
            bodyDic["Description"] = description;
            bodyDic["FeatureId"] = listTemplate.FeatureId.ToString("B");
            bodyDic["ctl00$PlaceHolderMain$onetidCreateList"] = "Create";
            bodyDic["ListTemplate"] = listTemplate.Type_Client;
            bodyDic["CustomTemplate"] = listTemplate.InternalName;
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl.ToString(), tokenProvider, "application/x-www-form-urlencoded", body, null);
            return null;
        }
        public void SetMetadataNavigationSettings(string webServerRelativeUrl, string listTitle, Guid listId, Dictionary<string, object> updateProperties)
        {
            mRequestCommon.SetMetadataNavigationSettings(webServerRelativeUrl, listTitle, listId, updateProperties);
        }

        public void UpdateFileProperties(string webServerRelativeUrl, string fileServerRelativeUrl, Dictionary<string, object> properties)
        {
            mRequestCommon.UpdateFileProperties(webServerRelativeUrl, fileServerRelativeUrl, properties);
        }

        public void CustomizeReport(Dictionary<string, object> parameters)
        {
            //URl likes https://offo.sharepoint.com/_layouts/15/CustomizeReport.aspx?ReportId=f43c916f-4450-4737-b889-8078c9826841&Category=Auditing
            string postUrl = mSiteUrl.TrimEnd('/') + "/_layouts/15/CustomizeReport.aspx?ReportId=f43c916f-4450-4737-b889-8078c9826841&Category=Auditing";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();

            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);

            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }

            bodyDic["__SCROLLPOSITIONX"] = "0";
            bodyDic["__SCROLLPOSITIONY"] = "0";

            bodyDic["ctl00$PlaceHolderMain$ctl00$ctl02$TxtReportStorageLocation"] = (string)parameters["LibraryLocation"];// "/docave library";
            bodyDic["ctl00$PlaceHolderMain$ctl04$ifsLocation$ctl02$serializedId"] = "";
            bodyDic["ctl00$PlaceHolderMain$ctl04$ifsDates$ctl01$DTCStartDate$DTCStartDateDate"] = (string)parameters["StartDateDate"];// "4/2/2012";
            bodyDic["ctl00$PlaceHolderMain$ctl04$ifsDates$ctl01$DTCStartDate$DTCStartDateDateHours"] = (string)parameters["StartDateDateHours"];// "12 AM";
            bodyDic["ctl00$PlaceHolderMain$ctl04$ifsDates$ctl01$DTCStartDate$DTCStartDateDateMinutes"] = (string)parameters["StartDateDateMinutes"];// "00";
            bodyDic["ctl00$PlaceHolderMain$ctl04$ifsDates$ctl01$DTCEndDate$DTCEndDateDate"] = (string)parameters["EndDateDate"];// "4/2/2013";
            bodyDic["ctl00$PlaceHolderMain$ctl04$ifsDates$ctl01$DTCEndDate$DTCEndDateDateHours"] = (string)parameters["EndDateDateHours"];// "12 AM";
            bodyDic["ctl00$PlaceHolderMain$ctl04$ifsDates$ctl01$DTCEndDate$DTCEndDateDateMinutes"] = (string)parameters["EndDateDateMinutes"];// "00";
            bodyDic["ctl00$PlaceHolderMain$ctl04$ifsUser$ctl01$userPicker$hiddenSpanData"] = "";
            bodyDic["ctl00$PlaceHolderMain$ctl04$ifsUser$ctl01$userPicker$OriginalEntities"] = "<Entities />";
            bodyDic["ctl00$PlaceHolderMain$ctl04$ifsUser$ctl01$userPicker$HiddenEntityKey"] = "";
            bodyDic["ctl00$PlaceHolderMain$ctl04$ifsUser$ctl01$userPicker$HiddenEntityDisplayText"] = "";
            bodyDic["ctl00$PlaceHolderMain$ctl04$ifsUser$ctl01$userPicker$downlevelTextBox"] = "";

            if (parameters.ContainsKey("View") && ((string)parameters["View"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl00$CheckBoxAuditView"] = "on";
            }
            if (parameters.ContainsKey("Update") && ((string)parameters["Update"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl00$CheckBoxAuditUpdate"] = "on";
            }
            if (parameters.ContainsKey("CheckInOut") && ((string)parameters["CheckInOut"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl00$CheckBoxAuditCheckInOut"] = "on";
            }
            if (parameters.ContainsKey("MoveCopy") && ((string)parameters["MoveCopy"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl00$CheckBoxAuditMoveCopy"] = "on";
            }
            if (parameters.ContainsKey("DeleteRestore") && ((string)parameters["DeleteRestore"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl00$CheckBoxAuditDeleteRestore"] = "on";
            }
            if (parameters.ContainsKey("ColumnContentType") && ((string)parameters["ColumnContentType"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl00$CheckBoxAuditColumnContentType"] = "on";
            }
            if (parameters.ContainsKey("Search") && ((string)parameters["Search"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl00$CheckBoxAuditSearch"] = "on";
            }
            if (parameters.ContainsKey("Perms") && ((string)parameters["Perms"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl00$CheckBoxAuditPerms"] = "on";
            }
            if (parameters.ContainsKey("Change") && ((string)parameters["Change"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl00$CheckBoxAuditChange"] = "on";
            }
            if (parameters.ContainsKey("Workflow") && ((string)parameters["Workflow"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl00$CheckBoxAuditWorkflow"] = "on";
            }
            if (parameters.ContainsKey("Custom") && ((string)parameters["Custom"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl00$CheckBoxAuditCustom"] = "on";
            }

            bodyDic["ctl00$PlaceHolderMain$ctl01$RptControls$btnOK"] = "Ok";

            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);

            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
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


        private void AnalyzeXmltoFileInfo(string information, List<Dictionary<string, object>> checkOutFileProperties, int localedId, bool isTime24)
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
                    //fileInfo["TimeLastModified"] = Convert.ToDateTime(doc.InnerText.Replace("\r\n\t\t\t", "").Replace("\r\n\t\t", ""));
                    fileInfo["TimeLastModified"] = AveTimeZoneUtility.ConvertToDateTime(doc.InnerText,localedId,isTime24);
                    doc.LoadXml(tdCollection[6].Value);
                    fileInfo["FileSize"] = doc.InnerText.Replace("\r\n\t\t\t", "").Replace("\r\n\t\t", "");
                    checkOutFileProperties.Add(fileInfo);
                }
            }
        }

        private Dictionary<string, object> RestoreFeature(string webServerRelativeUrl, Guid featureId, bool force, int scope, string featuresSource)
        {
            Dictionary<string, object> featureProp = new Dictionary<string, object>();
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            string siteFullUrl = this.mSiteUrl;
            string postUrl = string.Empty;
            switch (featuresSource)
            {
                case "web.features":
                    postUrl = webFullUrl.TrimEnd('/') + mLayout + "/ManageFeatures.aspx";
                    break;
                case "site.features":
                    postUrl = siteFullUrl.TrimEnd('/') + mLayout + "/ManageFeatures.aspx?Scope=Site";
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
            bodyDic["__EVENTTARGET"] = featureList;
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
            featureProp["DefinitionId"] = featureId;
            Dictionary<string, object> featureDefinitionProperties = new Dictionary<string, object>();
            featureProp["Definition" + AveObjectModelConstant.ObjectPropertySuffix] = featureDefinitionProperties;
            return featureProp;
        }

        private FeatureStatus IsFeatureActivated(string webServerRelativeUrl, Guid featureId, string featuresSource)
        {
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            string postUrl = string.Empty;

            switch (featuresSource)
            {
                case "web.features":
                    postUrl = webFullUrl.TrimEnd('/') + mLayout + "/ManageFeatures.aspx";
                    break;
                case "site.features":
                    postUrl = webFullUrl.TrimEnd('/') + mLayout + "/ManageFeatures.aspx?Scope=Site";
                    break;
            }
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            if (!string.IsNullOrEmpty(html))
            {
                HtmlDocument featurePage = new HtmlDocument();
                featurePage.LoadHtml(html);
                HtmlNode node = featurePage.DocumentNode.SelectSingleNode(string.Format("//div[@id='{0}']", featureId.ToString("D").ToLower()));
                if (node != null)
                {
                    HtmlNode statusNode = node.ParentNode.NextElementSibling;
                    if (statusNode.FirstElementChild != null)//it maybe the last element
                    {
                        return statusNode.FirstElementChild.GetAttributeValue("featurestatus", "") == "Active" ? FeatureStatus.Active : FeatureStatus.Deactive;
                    }
                    else
                    {
                        return FeatureStatus.Deactive;
                    }
                }
                else
                {
                    //节点在页面上找不到，就认为是隐藏的。
                    return FeatureStatus.Hidden;
                }
            }
            return FeatureStatus.Deactive;
        }

        //public void RefreshCredentials(object credentials)
        //{
        //    mObj = credentials;
        //}
        #endregion

        public Dictionary<string, object> GetMasterPageProperties(string webServerRelativeUrl)
        {
            Dictionary<string, object> masterPropDic = new Dictionary<string, object>();
            string getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_Layouts/15/ChangeSiteMasterPage.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider, true);
           
            int index = html.IndexOf("ctl00$PlaceHolderMain$ctl00$ctl01$masterPageSelectionControl$ctl00$SiteMasterPageDropDownList");
            string searchKey = "selected=\"selected\" value=\"";
            int startIndex = html.IndexOf(searchKey, index);
            if (startIndex > 0)
            {
                startIndex = startIndex + searchKey.Length;
                int endIndex = html.IndexOf("\"", startIndex);
                string siteMasterUrl = html.Substring(startIndex, endIndex - startIndex);
                masterPropDic["CustomMasterUrl"] = siteMasterUrl;
            }
            index = html.IndexOf("ctl00$PlaceHolderMain$ctl01$ctl01$systemMasterPageSelectionControl$ctl00$SystemMasterPageDropDownList");
            startIndex = html.IndexOf(searchKey, index);
            if (startIndex > 0)
            {
                startIndex = startIndex + searchKey.Length;
                int endIndex = html.IndexOf("\"", startIndex);
                string sysMasterUrl = html.Substring(startIndex, endIndex - startIndex);
                masterPropDic["MasterUrl"] = sysMasterUrl;
            }
            string searchContent = "<input name=\"ctl00$PlaceHolderMain$ctl03$ctl01$alternateCssSelector$AssetUrlInput\"";
            string infomation = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(infomation);
            masterPropDic["AlternateCssUrl"] = doc.FirstChild.Attributes["value"] != null ? doc.FirstChild.Attributes["value"].Value : default(string);
            return masterPropDic;
        }

        public void DeclareOrUndeclareItem(int itemId, Guid listId, string webUrl)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string postUrl = webUrl + mLayout + "/itemexpiration.aspx?" + string.Format("ID={0}&List={1}", itemId, "{" + listId.ToString() + "}");
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            bodyDic["__EVENTTARGET"] = string.Empty;
            bodyDic["__EVENTARGUMENT"] = string.Empty;
            Dictionary<string, object> tempDic = AveHttpWebRequestUtility.GetPostFormValues(html, false);
            foreach(string key in tempDic.Keys)
            {
                bodyDic.Add(key, tempDic[key]);
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            bodyDic["__CALLBACKID"] = "__Page";
            bodyDic["__CALLBACKPARAM"] = "ID_Recd";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }


        public void ReorderListFields(string webServerRelativeUrl, Guid listId, List<string> mappedSourceFields)
        {
            mRequestCommon.ReorderListFields(webServerRelativeUrl, listId, mappedSourceFields);
        }

        public List<Guid> GetListsIdContainItemsWithUniquePermissions(string webUrl)
        {
            List<Guid> listsIdContainItemsWithUniquePermissions = new List<Guid>();
            string getUrl = webUrl.TrimEnd('/') + "/_layouts/15/uniqperm.aspx?IsDlg=1";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider, true);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode formNode = doc.GetElementbyId("ctl00_PlaceHolderMain_rptrUniqueItemLists");
            if (formNode != null)
            {
                foreach (HtmlNode trNode in formNode.ChildNodes)
                {
                    if (trNode.Name.Equals("tr", StringComparison.OrdinalIgnoreCase))
                    {
                        HtmlNode tdNode = trNode.ChildNodes[2];
                        foreach (HtmlNode aNode in tdNode.ChildNodes)
                        {
                            if (aNode.Name.Equals("a", StringComparison.OrdinalIgnoreCase))
                            {
                                string listId = HttpUtility.ParseQueryString(aNode.GetAttributeValue("href", ""))["List"];
                                Guid listIdGuid = Guid.Empty;
                                if (!string.IsNullOrEmpty(listId)
                                    && Guid.TryParse(listId, out listIdGuid)
                                    && !listsIdContainItemsWithUniquePermissions.Contains(listIdGuid))
                                {
                                    listsIdContainItemsWithUniquePermissions.Add(listIdGuid);
                                }
                            }
                        }
                    }
                }
            }

            return listsIdContainItemsWithUniquePermissions;
        }

        public List<int> GetItemsIdWithUniquePermissions(string webServerRelativeUrl, string webUrl, Guid listId, bool isDocLib)
        {

            List<int> itemsIdWithUniquePermissions = new List<int>();
            string getUrl = string.Format("{0}/_layouts/15/uniqperm.aspx?obj={1},{2}&list={3}", webUrl.TrimEnd('/'), listId, isDocLib ? "doclib" : "list", listId);
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider, true);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode formNode = doc.GetElementbyId("ctl00_PlaceHolderMain_rptrUniqueLists");
            if (formNode != null)
            {
                foreach (HtmlNode trNode in formNode.ChildNodes)
                {
                    if (trNode.Name.Equals("tr", StringComparison.OrdinalIgnoreCase))
                    {
                        HtmlNodeCollection aNodes = trNode.SelectNodes(".//a/@href");
                        foreach (HtmlNode aNode in aNodes)
                        {
                            //mLogger.Info($"Security Search: href from html is {aNode.GetAttributeValue("href", "")}");
                            //mLogger.Info($"Security Search: webServerRelativeUrl is {webServerRelativeUrl}");
                            if (HttpUtility.UrlDecode(aNode.GetAttributeValue("href", "")).StartsWith(webServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                string itemId = HttpUtility.ParseQueryString(aNode.GetAttributeValue("href", ""))[0];
                                int itemIdInt = 0;
                                if (!string.IsNullOrEmpty(itemId)
                                    && int.TryParse(itemId, out itemIdInt)
                                    && !itemsIdWithUniquePermissions.Contains(itemIdInt))
                                {
                                    itemsIdWithUniquePermissions.Add(itemIdInt);
                                }
                            }
                        }
                    }
                }
            }

            return itemsIdWithUniquePermissions;
        }

        public bool GetRequestAccessEnable(string webUrl)
        {
            List<Guid> listsIdContainItemsWithUniquePermissions = new List<Guid>();
            string getUrl = webUrl.TrimEnd('/') + "/_layouts/15/setrqacc.aspx?type=web";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider, true);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode requestAccessNode = doc.GetElementbyId("ctl00_PlaceHolderMain_ctl00_chkRequestAccess");
            bool haveChecked = !string.IsNullOrEmpty(requestAccessNode.GetAttributeValue("checked", ""));
            return haveChecked;
        }

        public bool SetRequestAccessEnable(string webUrl, bool value)
        {
            List<Guid> listsIdContainItemsWithUniquePermissions = new List<Guid>();
            string getUrl = webUrl.TrimEnd('/') + "/_layouts/15/setrqacc.aspx?type=web";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider, true);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode membersCanShareNode = doc.GetElementbyId("ctl00_PlaceHolderMain_ctl00_chkMembersCanShare");
            bool membersCanShare = !string.IsNullOrEmpty(membersCanShareNode.GetAttributeValue("checked", ""));
            HtmlNode membersCanAddToGroupNode = doc.GetElementbyId("ctl00_PlaceHolderMain_ctl00_chkMembersCanAddToGroup");
            bool membersCanAddToGroup = false;
            if (membersCanAddToGroupNode != null)
            {
                membersCanAddToGroup = !string.IsNullOrEmpty(membersCanAddToGroupNode.GetAttributeValue("checked", ""));
            }
            HtmlNode requestAccessNode = doc.GetElementbyId("ctl00_PlaceHolderMain_ctl00_chkRequestAccess");
            bool requestAccess = !string.IsNullOrEmpty(requestAccessNode.GetAttributeValue("checked", ""));
            HtmlNode test4OwnersNode = doc.GetElementbyId("ctl00_PlaceHolderMain_ctl00_ctl04_labelDefaultSection");
            bool test4Owners = !string.IsNullOrEmpty(requestAccessNode.GetAttributeValue("checked", ""));

            if (requestAccess != value)
            {
                Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(html);
                Dictionary<string, object> bodyDic = new Dictionary<string, object>();
                string searchContent = "<input type=\"hidden\"";
                AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);

                foreach (KeyValuePair<string, object> kvp in formValues)
                {
                    if ((kvp.Key.Equals("ctl00$PlaceHolderMain$ctl00$chkMembersCanShare", StringComparison.OrdinalIgnoreCase) && membersCanShare)
                        || (kvp.Key.Equals("ctl00$PlaceHolderMain$ctl00$chkMembersCanAddToGroup", StringComparison.OrdinalIgnoreCase) && membersCanAddToGroup))
                    {
                        bodyDic[kvp.Key] = "on";
                    }
                    else if (value
                        && kvp.Key.Equals("ctl00$PlaceHolderMain$ctl00$ctl04$AccessRequestApprover", StringComparison.OrdinalIgnoreCase)
                        && test4Owners)
                    {
                        bodyDic[kvp.Key] = "RadOnAccessRequest";
                    }
                    else
                    {
                        bodyDic[kvp.Key] = kvp.Value;
                    }
                }
                bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl01$RptControls$btnSubmit";

                if (!value)
                {
                    if (bodyDic.ContainsKey("ctl00$PlaceHolderMain$ctl00$chkRequestAccess"))
                    {
                        bodyDic.Remove("ctl00$PlaceHolderMain$ctl00$chkRequestAccess");
                    }
                    if (bodyDic.ContainsKey("ctl00$PlaceHolderMain$ctl00$ctl04$txtEmail1"))
                    {
                        bodyDic.Remove("ctl00$PlaceHolderMain$ctl00$ctl04$txtEmail1");
                    }
                    if (bodyDic.ContainsKey("ctl00$PlaceHolderMain$ctl00$ctl04$txtAccreqCustomMsg"))
                    {
                        bodyDic.Remove("ctl00$PlaceHolderMain$ctl00$ctl04$txtAccreqCustomMsg");
                    }
                }
                else
                {
                    if (!bodyDic.ContainsKey("ctl00$PlaceHolderMain$ctl00$chkRequestAccess"))
                    {
                        bodyDic.Add("ctl00$PlaceHolderMain$ctl00$chkRequestAccess", "on");
                    }
                    else
                    {
                        bodyDic["ctl00$PlaceHolderMain$ctl00$chkRequestAccess"] = "on";
                    }
                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl04$AccessRequestApprover"] = "RadOnAccessRequest";
                    if (bodyDic.ContainsKey("ctl00$PlaceHolderMain$ctl00$ctl04$txtEmail1"))
                    {
                        bodyDic.Remove("ctl00$PlaceHolderMain$ctl00$ctl04$txtEmail1");
                    }
                }

                byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
                AveHttpWebRequestUtility.HttpPost(getUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
            }
            return value;
        }

        public bool GetAccessRequestApprover(string webUrl)
        {
            List<Guid> listsIdContainItemsWithUniquePermissions = new List<Guid>();
            string getUrl = webUrl.TrimEnd('/') + "/_layouts/15/setrqacc.aspx?type=web";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider, true);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode accessRequestApproverNode = doc.GetElementbyId("ctl00_PlaceHolderMain_ctl00_ctl04_defaultValue");
            bool haveChecked = !string.IsNullOrEmpty(accessRequestApproverNode.GetAttributeValue("checked", ""));
            return haveChecked;
        }

        public void SetAccessRequestApprover(string webUrl, bool value, string email)
        {
            if (!value && string.IsNullOrEmpty(email))
            {
                value = true;
            }

            List<Guid> listsIdContainItemsWithUniquePermissions = new List<Guid>();
            string getUrl = webUrl.TrimEnd('/') + "/_layouts/15/setrqacc.aspx?type=web";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, tokenProvider, true);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode membersCanShareNode = doc.GetElementbyId("ctl00_PlaceHolderMain_ctl00_chkMembersCanShare");
            bool membersCanShare = !string.IsNullOrEmpty(membersCanShareNode.GetAttributeValue("checked", ""));
            HtmlNode membersCanAddToGroupNode = doc.GetElementbyId("ctl00_PlaceHolderMain_ctl00_chkMembersCanAddToGroup");
            bool membersCanAddToGroup = false;
            if (membersCanAddToGroupNode != null)
            {
                membersCanAddToGroup = !string.IsNullOrEmpty(membersCanAddToGroupNode.GetAttributeValue("checked", ""));
            }
            HtmlNode requestAccessNode = doc.GetElementbyId("ctl00_PlaceHolderMain_ctl00_chkRequestAccess");
            bool requestAccess = !string.IsNullOrEmpty(requestAccessNode.GetAttributeValue("checked", ""));
            HtmlNode test4OwnersNode = doc.GetElementbyId("ctl00_PlaceHolderMain_ctl00_ctl04_labelDefaultSection");
            bool test4Owners = !string.IsNullOrEmpty(requestAccessNode.GetAttributeValue("checked", ""));

            if (requestAccess)
            {
                Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(html);
                Dictionary<string, object> bodyDic = new Dictionary<string, object>();
                string searchContent = "<input type=\"hidden\"";
                AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);

                foreach (KeyValuePair<string, object> kvp in formValues)
                {
                    if ((kvp.Key.Equals("ctl00$PlaceHolderMain$ctl00$chkMembersCanShare", StringComparison.OrdinalIgnoreCase) && membersCanShare)
                        || (kvp.Key.Equals("ctl00$PlaceHolderMain$ctl00$chkMembersCanAddToGroup", StringComparison.OrdinalIgnoreCase) && membersCanAddToGroup)
                        || (kvp.Key.Equals("ctl00$PlaceHolderMain$ctl00$chkRequestAccess", StringComparison.OrdinalIgnoreCase)))
                    {
                        bodyDic[kvp.Key] = "on";
                    }
                    else if (kvp.Key.Equals("ctl00$PlaceHolderMain$ctl00$ctl04$AccessRequestApprover", StringComparison.OrdinalIgnoreCase))
                    {
                        bodyDic[kvp.Key] = value ? "RadOnAccessRequest" : "RadOffAccessRequest";
                    }
                    else
                    {
                        bodyDic[kvp.Key] = kvp.Value;
                    }
                }
                bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl01$RptControls$btnSubmit";

                if (!value)
                {
                    if (bodyDic.ContainsKey("ctl00$PlaceHolderMain$ctl00$ctl04$txtEmail1"))
                    {
                        bodyDic["ctl00$PlaceHolderMain$ctl00$ctl04$txtEmail1"] = HttpUtility.UrlEncode(email);
                    }
                    else
                    {
                        bodyDic.Add("ctl00$PlaceHolderMain$ctl00$ctl04$txtEmail1", HttpUtility.UrlEncode(email));
                    }
                }
                else
                {
                    if (bodyDic.ContainsKey("ctl00$PlaceHolderMain$ctl00$ctl04$txtEmail1"))
                    {
                        bodyDic.Remove("ctl00$PlaceHolderMain$ctl00$ctl04$txtEmail1");
                    }
                }

                byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
                AveHttpWebRequestUtility.HttpPost(getUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
            }
        }
    }

    internal enum FeatureStatus
    {
        Active,
        Deactive,
        Hidden
    }
}
