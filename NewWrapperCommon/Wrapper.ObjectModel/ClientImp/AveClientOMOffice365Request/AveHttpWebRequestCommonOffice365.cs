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

using AvePoint.Office365.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AveClientRequest.Common;
using System.Web;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections;
using System.Globalization;
using AvePoint.GCommon.Contract.CodeReview;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SharePoint.Client;
using System.Net;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveHttpWebRequestCommonOffice365 : IAveHttpWebRequestCommon
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveHttpWebRequestCommonOffice365));
        protected string mLayout = "/_layouts/15";
        protected object mObj;
        protected string mSiteUrl;
        private string mWebAppName;
        protected AveHttpWebRequestCommon mRequestCommon;
        protected string sharepointVersion;
        private ITokenProvider tokenProvider;
        public AveHttpWebRequestCommonOffice365(string siteUrl, object obj, ITokenProvider tokenProvider)
        {
            this.tokenProvider = tokenProvider;
            mSiteUrl = siteUrl;
            mObj = obj;
            mRequestCommon = new AveHttpWebRequestCommon(mSiteUrl, mObj, 15);
            mRequestCommon.TokenProvider = tokenProvider;
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
        public void OperateOnVersion(string webServerRelativeUrl, string webAppName, object obj, string listUrl, int itemId, int versionId, string listId, string fileName, string op)
        {
            AveHttpWebRequestCommon.OperateOnVersion(webServerRelativeUrl, webAppName, obj, listUrl, itemId, versionId, listId, fileName, op, mLayout, this.tokenProvider);
        }
        #region Get
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ms-vb and ms-standardheader are a part of values")]
        public Dictionary<string, object> GetAllFeatureDefinitions(string Url, string featuresSource)
        {
            string requestUrl = string.Empty;
            switch (featuresSource)
            {
                case "web.features":
                    requestUrl = Url.TrimEnd('/') + mLayout + "/ManageFeatures.aspx";
                    break;
                case "site.features":
                    requestUrl = Url.TrimEnd('/') + mLayout + "/ManageFeatures.aspx?Scope=Site";
                    break;
            }
            Dictionary<string, object> featureDefinitions = new Dictionary<string, object>();
            List<Dictionary<string, object>> featureDefinitionList = new List<Dictionary<string, object>>();
            string html = AveHttpWebRequestUtility.HttpGet(requestUrl, mObj, this.tokenProvider);

            string titleKey = "<h3 class=\"ms-standardheader\">";
            string descriptionKey = "<td class=\"ms-vb2\">";
            string idKey = "<div id='";
            string statusKey = "value=\"";
            int index = html.IndexOf(titleKey, StringComparison.OrdinalIgnoreCase);
            while (index >= 0)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                int titleStartIndex = index + titleKey.Length;
                int titleEndIndex = html.IndexOf("</h3>", titleStartIndex, StringComparison.OrdinalIgnoreCase);
                string title = html.Substring(titleStartIndex, titleEndIndex - titleStartIndex);

                int desStartIndex = html.IndexOf(descriptionKey, titleEndIndex, StringComparison.OrdinalIgnoreCase) + descriptionKey.Length;
                int desEndIndex = html.IndexOf("</td>", desStartIndex, StringComparison.OrdinalIgnoreCase);
                string description = html.Substring(desStartIndex, desEndIndex - desStartIndex);

                int idStartIndex = html.IndexOf(idKey, desEndIndex, StringComparison.OrdinalIgnoreCase) + idKey.Length;
                int idEndIndex = html.IndexOf("</div>", idStartIndex, StringComparison.OrdinalIgnoreCase);
                string id = html.Substring(idStartIndex, 36);

                int contentStartIndex = html.IndexOf(statusKey, idStartIndex, StringComparison.OrdinalIgnoreCase) + statusKey.Length;
                int contentEndIndex = html.IndexOf("\"", contentStartIndex, StringComparison.OrdinalIgnoreCase);
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
                index = html.IndexOf(titleKey, idEndIndex, StringComparison.OrdinalIgnoreCase);
            }
            featureDefinitions.Add(AveObjectModelConstant.ChildrenProperties, featureDefinitionList);
            return featureDefinitions;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "rad is a part of variable and Rad is a aprt of value,srchvis is a part of url")]
        public void GetWebSearchAndOfflineAvailability(string webServerRelativeUrl, Dictionary<string, object> webProp, object obj)
        {
            string getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/srchvis.aspx?AjaxDelta=1 ";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, obj, this.tokenProvider);
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "textarea is a part of value,prjsetng is a part of url")]
        public Dictionary<string, object> GetWebLogoProperties(string webServerRelativeUrl)
        {
            string getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/prjsetng.aspx?AjaxDelta=1 ";
            Dictionary<string, object> webLogoProp = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
            if (string.IsNullOrEmpty(html))
            {
                mLogger.Warn("Get Web Logo Properties failed. Page Url:{0}, Web Url:{1}", getUrl, webServerRelativeUrl);
                return webLogoProp;
            }
            string searContent = "<input name=\"ctl00$PlaceHolderMain$logoSection$ctl03$TxtSiteLogoUrl\"";//ctl00$PlaceHolderMain$logoSection$ctl03$TxtSiteLogoUrl
            string infomation = AveHttpWebRequestUtility.GetInput(html, searContent, "/>");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(infomation);
            webLogoProp["SiteLogoUrl"] = xmlDoc.FirstChild.Attributes["value"] != null ? xmlDoc.FirstChild.Attributes["value"].Value : default(string);
            searContent = "<textarea name=\"ctl00$PlaceHolderMain$logoSection$ctl04$TxtLogoUrlDescription\"";
            infomation = AveHttpWebRequestUtility.GetInput(html, searContent, "</textarea>");
            xmlDoc.LoadXml(infomation);
            webLogoProp["SiteLogoDescription"] = xmlDoc.FirstChild.InnerText.StartsWith("\r\n", StringComparison.OrdinalIgnoreCase) ? xmlDoc.FirstChild.InnerText.Substring(2) : xmlDoc.FirstChild.InnerText;
            return webLogoProp;
        }
        public Dictionary<string, object> GetMetadataNavigationSettings(string webServerRelativeUrl, Guid listId, string listTitle)
        {
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/MetaNavSettings.aspx?List=" + listId.ToString("B");
            Dictionary<string, object> metadataNavigationSettings = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
            string searchContent = "<input id=\"ctl00_PlaceHolderMain_ctl02_autoIndexingYesRadioButton\"";
            string information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            if (string.IsNullOrEmpty(information))
            {
                return metadataNavigationSettings;
            }
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Ddlweb is a part of Keys")]
        public Dictionary<string, object> GetDefaultRegionalSetting(string webServerRelativeUrl, int lcid)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/regionalsetng.aspx";
            Dictionary<string, object> defaultRegionalProp = new Dictionary<string, object>();
            defaultRegionalProp["LocaleId"] = lcid;
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
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
            string defaultHtml = AveHttpWebRequestUtility.HttpReturn(postUrl, mObj, "application/x-www-form-urlencoded", data, null, string.Empty, this.tokenProvider);
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "listkeywords is a part of url")]
        public List<Dictionary<string, object>> GetKeyWords()
        {
            string getUrl = mSiteUrl.TrimEnd('/') + mLayout + "/listkeywords.aspx";
            List<Dictionary<string, object>> keyWordsProp = new List<Dictionary<string, object>>();
            this.GetKeyWords(getUrl, keyWordsProp);
            return keyWordsProp;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "pnext is a part of Key")]
        private void GetKeyWords(string getUrl, List<Dictionary<string, object>> keyWordsProp)
        {
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
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
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
            GetKeyWordProperties(keyWordName, html, keyWordProp);
            return keyWordProp;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "textarea is a part of makeup")]
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Dlg is a part of IsDlg,bestbet is part of xpath")]
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
                string html = AveHttpWebRequestUtility.HttpGet(url, mObj, this.tokenProvider);
                string xml = AveHttpWebRequestUtility.GetInput(html, "<textarea name=\"descriptionTextBox", "</textarea>");
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                bestBetProp["Description"] = doc.InnerText;
                bestBetsProp.Add(bestBetProp);
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "cbxl is a part of Keys")]
        public List<string> GetSiteEnabledHelpCollections()
        {
            string getUrl = mSiteUrl.TrimEnd('/') + mLayout + "/HelpSettings.aspx";
            List<string> helpCollection = new List<string>();
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
            string searchContent = null;
            string endContent = null;
            searchContent = "<table id=\"ctl00_PlaceHolderMain_ctl00_ctl01_cbxlAvailableHelpCollections\"";
            endContent = "</table>";
            if (html.IndexOf(searchContent, StringComparison.OrdinalIgnoreCase) == -1)
            {
                searchContent = "<span id=\"ctl00_PlaceHolderMain_ctl00_ctl01_cbxlAvailableHelpCollections\"";
                endContent = "</span>";
            }
            GetSiteEnabledHelpCollections(html, searchContent, endContent, helpCollection);
            return helpCollection;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "nbsp is a part of value,contenttypesyndicationhubs is part of of url")]
        public List<Dictionary<string, object>> GetPublishedContentTypes()
        {
            string getUrl = mSiteUrl.TrimEnd('/') + mLayout + "/contenttypesyndicationhubs.aspx";
            List<Dictionary<string, object>> metadataSevices = new List<Dictionary<string, object>>();
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Plt is a part of Url")]
        public Dictionary<string, object> GetSitePortal(string siteUrl)
        {
            string getUrl = siteUrl.TrimEnd('/') + mLayout + "/portal.aspx?AjaxDelta=1&isStartPlt1=1344503071152";
            Dictionary<string, object> sitePortal = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            sitePortal.Add("PortalUrl", AveHttpWebRequestUtility.GetComponentValue(html, "ctl00$PlaceHolderMain$ctl00$ctl02$TxtPortalURL"));
            sitePortal.Add("PortalName", AveHttpWebRequestUtility.GetComponentValue(html, "ctl00$PlaceHolderMain$ctl00$ctl03$TxtPortalName"));
            return sitePortal;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "siterss is a part of url")]
        public bool GetSiteRssSetting()
        {
            string netWorkUrl = mSiteUrl.TrimEnd('/') + mLayout + "/siterss.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(netWorkUrl, mObj, this.tokenProvider);
            bool allowSiteRss = AveHttpWebRequestUtility.GetCheckInput(html, "ctl00$PlaceHolderMain$SiteColRssSection$ctl01$CheckSiteColRss");
            return allowSiteRss;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Assignto and Rad is a part of Keys,advsetng is part of url")]
        public Dictionary<string, object> GetListAdvancedSettingProperties(string webServerRelativeUrl, Guid listId, SecurityTrimObject mSiteTrimObj)
        {
            Dictionary<string, object> advancedProp = new Dictionary<string, object>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/advsetng.aspx?List=" + listId.ToString("B");
            string html = string.Empty;
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
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
                mLogger.Debug("Failed to get list advanced setting properties, error message : {0}", e.ToString());
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
            if (!string.IsNullOrEmpty(information))
            {
                advancedProp["EnableAssignToEmail"] = information.Contains("checked=\"checked\"");
            }
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
            searchContent = "<input id=\"ctl00_PlaceHolderMain_ManagedIndexesSection_ctl02_RadManagedIndexesYes\" ";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            advancedProp["EnableManagedIndexes"] = information.Contains("checked=\"checked\"");

            return advancedProp;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Setng"), System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "valign"), System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Wrk")]
        public Dictionary<string, object> GetWorkflowAssociations(string webServerRelativeUrl, string listName, Guid listId, string workflowSource, Dictionary<string, object> contentTypeProp)
        {
            Dictionary<string, object> workflowAssociationsProp = new Dictionary<string, object>();
            string getUrl = string.Empty;
            if (workflowSource.Equals("list.workflow"))
            {
                getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/WrkSetng.aspx?List=" + listId;
            }
            else if (string.Equals("web.workflow", workflowSource, StringComparison.OrdinalIgnoreCase))
            {
                getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/WrkSetng.aspx";
            }
            else
            {
                AveHttpValueCollection values = new AveHttpValueCollection();
                values["List"] = listId.ToString();
                values["ctype"] = contentTypeProp["ContentTypeId"].ToString();
                getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/WrkSetng.aspx?" + values.ToString(true);//List=" + listId + "&ctype=" + contentTypeProp["ContentTypeId"];
            }
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "ms-vb")]
        private void GetWorkflowProperties(string information, Dictionary<string, object> workflowAssociationsProp)
        {
            string tempStr = AveHttpWebRequestUtility.GetInput(information, "<a", "</a>");
            string name = AveHttpWebRequestUtility.GetInnerText(tempStr, ">", "<");
            int value = Convert.ToInt32(AveHttpWebRequestUtility.GetInnerText(information, "<td class=\"ms-vb\" nowrap=\"nowrap\">", "</td>"));
            workflowAssociationsProp.Add(name, value);
        }
        public Dictionary<string, object> GetListGeneralProperties(string webServerRelativeUrl, Guid listId)
        {
            Dictionary<string, object> generalProperties = new Dictionary<string, object>();
            string url = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/ListGeneralSettings.aspx?List=" + listId.ToString("B");
            string html = string.Empty;
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(url, mObj, this.tokenProvider);
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

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Setng is a part of url,onetid is aprt of value")]
        public Dictionary<string, object> GetListVersionLimited(string webServerRelativeUrl, Guid listId)
        {
            //没有SecurityTriming，2010于2013可共用这部分代码
            Dictionary<string, object> versionLimitedProp = new Dictionary<string, object>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/LstSetng.aspx?List=" + listId;
            string html = string.Empty;
            html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
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

            searchContent = "id=\"MinorVisibilityReader\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            if (information.IndexOf("checked", StringComparison.OrdinalIgnoreCase) > 0)
            {
                versionLimitedProp["DraftVersionVisibility"] = 0;
            }
            searchContent = "id=\"onetidMinorAuthor\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            if (information.IndexOf("checked", StringComparison.OrdinalIgnoreCase) > 0)
            {
                versionLimitedProp["DraftVersionVisibility"] = 1;
            }
            searchContent = "id=\"onetidMinorApprover\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            if (information.IndexOf("checked", StringComparison.OrdinalIgnoreCase) > 0)
            {
                versionLimitedProp["DraftVersionVisibility"] = 2;
            }

            return versionLimitedProp;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ms-standardheader is a part of values")]
        public List<Dictionary<string, object>> GetListCheckedOutFiles(string webServerRelativeUrl, Guid listId)
        {
            List<Dictionary<string, object>> checkOutFileProperties = new List<Dictionary<string, object>>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/ManageCheckedOutFiles.aspx?List=" + listId + "";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
            string searchPattern = "//table[@width='100%'][@cellpadding='0'][@cellspacing='0'][@border='0'][@id='onetidTable']";
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode(searchPattern);
            HtmlNodeCollection nodeCollection = node.SelectNodes("./tr");
            string searchResult = string.Empty;
            if (nodeCollection.Count > 0)
            {
                for (int i = 2; i < nodeCollection.Count; i++)
                {
                    if (nodeCollection[i].SelectSingleNode(".//h3") != null)
                    {
                        continue;
                    }
                    checkOutFileProperties.Add(GetHtmlNodeInformation(nodeCollection[i]));
                }
            }
            #region
            //if (!string.IsNullOrEmpty(searchResult))
            //{
            //    AnalyzeXmltoFileInfo(searchResult, checkOutFileProperties);
            //}
            //string searchContent = "class=\"ms-standardheader\"><b>Files checked out to others:</b></h3></td></tr>";
            //string information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, "</table>");
            //if (!string.IsNullOrEmpty(information))
            //{
            //    information = information.Replace("< 1 KB", "LT1KB");
            //    AnalyzeXmltoFileInfo(information, checkOutFileProperties);
            //}
            //else
            //{
            //    searchContent = "class=\"ms-standardheader\"><b>Files checked out to me:</b></h3></td></tr>";
            //    information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, "</table>");
            //    if (!string.IsNullOrEmpty(information))
            //    {
            //        information = information.Replace("< 1 KB", "LT1KB");
            //        AnalyzeXmltoFileInfo(information, checkOutFileProperties);
            //    }
            //}
            #endregion
            return checkOutFileProperties;
        }
        private Dictionary<string, object> GetHtmlNodeInformation(HtmlNode node)
        {
            Dictionary<string, object> fileInfo = new Dictionary<string, object>();
            HtmlNodeCollection tdCollection = node.SelectNodes("./td");
            HtmlNode secondTd = tdCollection[2].SelectSingleNode("a");//当Others时，这个Td底下没有<a>
            fileInfo["LeafName"] = (secondTd == null ? tdCollection[2] : secondTd).InnerText.Replace("\r\n\t\t\t", "").Replace("\r\n\t\t", "");
            fileInfo["DirName"] = tdCollection[3].SelectSingleNode("a").InnerText.Replace("\r\n\t\t\t", "").Replace("\r\n\t\t", "");
            fileInfo["CheckedOutByName"] = tdCollection[4].SelectSingleNode("span").InnerText.Replace("\r\n\t\t\t", "").Replace("\r\n\t\t", "");
            fileInfo["TimeLastModified"] = Convert.ToDateTime(tdCollection[5].InnerText.Replace("\r\n\t\t\t", "").Replace("\r\n\t\t", ""));
            fileInfo["FileSize"] = tdCollection[6].InnerText.Replace("\r\n\t\t\t", "").Replace("\r\n\t\t", "");
            return fileInfo;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "metadatacolsettings is a part of url")]
        public Dictionary<string, object> GetMetadataListFieldSettings(string webServerRelativeUrl, string listTitle, Guid listId)
        {
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/metadatacolsettings.aspx?List=" + listId.ToString("B");
            Dictionary<string, object> metadataListFieldSettingsProp = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Rad is a part of Keys")]
        public bool GetListRated(string webServerRelativeUrl, Guid listId)
        {
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/RatingsSettings.aspx?List=" + listId.ToString("B");
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
            string value = AveHttpWebRequestUtility.GetInputControlValue(html, "//input[@type='radio'][@checked='checked'][@name='ctl00$PlaceHolderMain$ctl00$ctl03$EnableRatings']");
            return "RadEnableRatingsYes".Equals(value, StringComparison.OrdinalIgnoreCase);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Rad is a part of value")]
        public string GetListExperience(string webServerRelativeUrl, Guid listId)
        {
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/RatingsSettings.aspx?List=" + listId.ToString("B");
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "listsyndication is a part of url,textarea is part of makeup")]
        public Dictionary<string, object> GetListRssProperties(string webServerRelativeUrl, Guid listId)
        {
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/listsyndication.aspx?List=" + listId.ToString("B");
            Dictionary<string, object> rssProperties = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
            if (string.IsNullOrEmpty(html))
            {
                mLogger.Warn("Get List RSS Properties failed.Page Url:{0}, Web Url:{1}", getUrl, webServerRelativeUrl);
                return rssProperties;
            }
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
                folderProp["vti_rss_ChannelDescription"] = xmlDoc.FirstChild.InnerText.Substring(2);
            }
            searchContent = "<input name=\"ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl04$TxtChannelImageUrl\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            if (!string.IsNullOrEmpty(information))
            {
                xmlDoc.LoadXml(information);
                folderProp["vti_rss_ChannelImageUrl"] = xmlDoc.FirstChild.Attributes["value"] != null ? xmlDoc.FirstChild.Attributes["value"].Value : String.Empty;
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

        private Dictionary<string, string> GetSitePolicies(string url)
        {
            string html = AveHttpWebRequestUtility.HttpGet(url, mObj, this.tokenProvider);
            Dictionary<string, string> sitePolicies = new Dictionary<string, string>();
            string searchContent = "<a href=\"projectpolicyconfig.aspx?ctype=";
            string idEnd = "\">";
            string nameEnd = "<";
            if (html.Contains(searchContent))
            {
                int startIndex = html.IndexOf(searchContent, StringComparison.OrdinalIgnoreCase) + searchContent.Length;
                int endIndex = html.IndexOf(idEnd, startIndex, StringComparison.OrdinalIgnoreCase);
                while (true)
                {
                    string policyId = html.Substring(startIndex, endIndex - startIndex);
                    startIndex = endIndex + idEnd.Length;
                    endIndex = html.IndexOf(nameEnd, startIndex, StringComparison.OrdinalIgnoreCase);
                    string policyName = html.Substring(startIndex, endIndex - startIndex);
                    html = html.Substring(endIndex);
                    sitePolicies.Add(policyName, policyId);
                    startIndex = html.IndexOf(searchContent, StringComparison.OrdinalIgnoreCase);
                    if (startIndex == -1)
                    {
                        break;
                    }
                    startIndex += searchContent.Length;
                    endIndex = html.IndexOf(idEnd, startIndex, StringComparison.OrdinalIgnoreCase);
                }
            }
            return sitePolicies;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "IsDlg:A lesson of SharePoint local path.tpc,Dd,ifc are part of database column")]
        public void GetManagedSiteCollectionData(Dictionary<string, object> managedData, string adminUrl, long availableStorageQuota, double availableResourceQuota)
        {
            List<Dictionary<string, object>> languageList = new List<Dictionary<string, object>>();
            Dictionary<string, object> languages = new Dictionary<string, object>();

            List<Dictionary<string, object>> prefixList = new List<Dictionary<string, object>>();
            Dictionary<string, object> prefixs = new Dictionary<string, object>();

            string manageUrl = string.Format("{0}/_layouts/15/online/SiteCollections.aspx", adminUrl.TrimEnd('/'));
            string createUrl = string.Format("{0}/_layouts/15/online/CreateSite.aspx?IsDlg=1", adminUrl.TrimEnd('/'));
            string searchContent = "<input type=\"hidden\"";
            Dictionary<string, object> managedBodyDic = new Dictionary<string, object>();
            string html1 = AveHttpWebRequestUtility.HttpGet(manageUrl, mObj, this.tokenProvider);
            AveHttpWebRequestUtility.GetInput(html1, searchContent, managedBodyDic);
            Dictionary<string, object> createBodyDic = new Dictionary<string, object>();
            createBodyDic["hidParam"] = "undefined";
            createBodyDic["hidParam2"] = "undefined";
            createBodyDic["hidParam3"] = availableStorageQuota;
            createBodyDic["hidParam4"] = availableResourceQuota;
            createBodyDic["__REQUESTDIGEST"] = HttpUtility.UrlEncode(managedBodyDic["__REQUESTDIGEST"].ToString());
            createBodyDic["submit"] = "Submit Query";
            byte[] body2 = AveHttpWebRequestUtility.GetByte(createBodyDic, null);
            string html2 = AveHttpWebRequestUtility.HttpReturn(createUrl, mObj, "application/x-www-form-urlencoded", body2, null, string.Empty, this.tokenProvider);
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
            languages.Add(AveObjectModelConstant.ChildrenProperties, languageList);
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
            prefixs.Add(AveObjectModelConstant.ChildrenProperties, prefixList);
            managedData["Prefixes" + AveObjectModelConstant.ObjectPropertySuffix] = prefixs;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "auditsettings.aspx is a sharepoint setting page")]
        public AveRequestAudit GetRequestAudit()
        {
            AveRequestAudit requestAudit = new AveRequestAudit();
            string postUrl = mSiteUrl.TrimEnd('/') + "/_layouts/15/auditsettings.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
            Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(html);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input ";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            string view = "ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditView";
            string edit = "ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditEdit";
            string checkInOut = "ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditCheckInOut";
            string moveCopy = "ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditMoveCopy";
            string deleteRestore = "ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditDeleteRestore";
            string columnsContentType = "ctl00$PlaceHolderMain$ctl02$ctl01$CheckBoxAuditColumnsContentType";
            string search = "ctl00$PlaceHolderMain$ctl02$ctl01$CheckBoxAuditSearch";
            string perms = "ctl00$PlaceHolderMain$ctl02$ctl01$CheckBoxAuditPerms";
            int flag = 0;
            string tmp = string.Empty;
            int index = html.IndexOf(view, StringComparison.Ordinal) + view.Length + 1;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 4;
            }
            index = html.IndexOf(edit, StringComparison.Ordinal) + edit.Length + 1;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 16;
            }
            index = html.IndexOf(checkInOut, StringComparison.Ordinal) + checkInOut.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 3;
            }
            index = html.IndexOf(moveCopy, StringComparison.Ordinal) + moveCopy.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 6144;
            }
            index = html.IndexOf(deleteRestore, StringComparison.Ordinal) + deleteRestore.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 520;
            }
            index = html.IndexOf(columnsContentType, StringComparison.Ordinal) + columnsContentType.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 160;
            }
            index = html.IndexOf(search, StringComparison.Ordinal) + search.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 8192;
            }
            index = html.IndexOf(perms, StringComparison.Ordinal) + perms.Length + 1;
            tmp = string.Empty;
            tmp = html.Substring(index, 18);
            if (tmp.Equals(" checked=\"checked\"", StringComparison.OrdinalIgnoreCase))
            {
                flag |= 256;
            }
            requestAudit.AuditFlags = (AveAuditMaskType)flag;
            requestAudit.TrimAuditLog = GetTrimAuditLog(formValues, "ctl00$PlaceHolderMain$ctl00$ctl04$trimAuditLog");
            requestAudit.AuditLogTrimmingRetention = GetAuditLogTrimmingRetention(formValues, "ctl00$PlaceHolderMain$ctl00$ctl05$TxtTrimRetention");
            return requestAudit;
        }

        private int GetAuditLogTrimmingRetention(Dictionary<string, object> formValues, string trimRetentionKey)
        {
            int auditLogTrimmingRetention = 0;
            if (formValues.ContainsKey(trimRetentionKey))
            {
                int.TryParse((string)formValues[trimRetentionKey], out auditLogTrimmingRetention);
            }
            return auditLogTrimmingRetention;
        }

        private bool GetTrimAuditLog(Dictionary<string, object> formValues, string trimAuditLogKey)
        {
            if (formValues.ContainsKey(trimAuditLogKey))
            {
                return ((string)formValues[trimAuditLogKey]).Equals("RadTrimAuditLogYes", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        #endregion

        #region Update
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Subweb is a part of Keys,prejsetng is part of url")]
        public void UpdateWebLogo(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/prjsetng.aspx?AjaxDelta=1";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
            if (string.IsNullOrEmpty(html))
            {
                mLogger.Warn("Update Web Logo Failed. Page Url{0}, Web Url{1}.", postUrl, webServerRelativeUrl);
                return;
            }
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
                bodyDic["ctl00$PlaceHolderMain$logoSection$ctl03$TxtSiteLogoUrl"] = webProperties["SiteLogoUrl"] == null ? string.Empty : HttpUtility.UrlEncode(webProperties["SiteLogoUrl"].ToString());
            }
            if (webProperties.ContainsKey("SiteLogoDescription"))
            {
                bodyDic["ctl00$PlaceHolderMain$logoSection$ctl04$TxtLogoUrlDescription"] = webProperties["SiteLogoDescription"] == null ? string.Empty : HttpUtility.UrlEncode(webProperties["SiteLogoDescription"].ToString());
            }
            //与SharePoint界面保持一致，Name属性不放在logo中更新。
            //由于Name更新后会更改ServerRelativeUrl，在此处HttpPost更新，没有reload web对象，调用处可能出现对象不一致
            //if (webProperties.ContainsKey("Name"))
            //{
            //    bodyDic["ctl00$PlaceHolderMain$idUrlSection$ctl03$TxtCreateSubwebName"] = webProperties["Name"];
            //}
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "rad and Dont is a part of values,Rad is part of keys")]
        public void UpdateWebSearchAndOfflineAvailability(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/srchvis.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
            if (string.IsNullOrEmpty(html))
            {
                mLogger.Warn("Update web search and offline availability failed. Page Url{0}. Web URL:{1}", postUrl, webServerRelativeUrl);
                return;
            }
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
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Ddl and Ddlweb are both a part of Keys,reginalsetng is a part of url")]
        public void UpdateWebRegionalSetting(string webServerRelativeUrl, Dictionary<string, object> regionalProp)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/regionalsetng.aspx?AjaxDelta=1";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
            if (string.IsNullOrEmpty(html))
            {
                mLogger.Warn("Update Web Regional Setting failed. Page Url:{0}, Web Url:{1}", postUrl, webServerRelativeUrl);
                return;
            }
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
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", localData, null, this.tokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", data, null, this.tokenProvider);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "nbsp is a part of value")]
        public Dictionary<string, object> UpdateKeyWord(string term, int localId, int calendarType, Dictionary<string, object> keyWordProp)
        {
            string url = mSiteUrl.TrimEnd('/') + string.Format(mLayout + "/Keyword.aspx?k={0}", term);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, mObj, this.tokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(url, mObj, "application/x-www-form-urlencoded", data, null, this.tokenProvider);

            Dictionary<string, object> newKeyWordProp = new Dictionary<string, object>();
            newKeyWordProp = this.GetKeyWordProperties(term);
            return newKeyWordProp;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "mngsiteadmin is a part of url,Hdn is part of keys")]
        public Dictionary<string, object> UpdateSiteAdministrators(string webServerRelativeUrl, string oldAdmins, List<Dictionary<string, object>> newAdmins)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/mngsiteadmin.aspx";
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
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);
            siteAdmins.Add(AveObjectModelConstant.ChildrenProperties, newAdmins);
            return siteAdmins;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "onetid is a part of value")]
        public Dictionary<string, object> UpdateSitePortal(Dictionary<string, object> sitePortalProperties)
        {
            Dictionary<string, object> sitePortal = new Dictionary<string, object>();
            string postUrl = mSiteUrl.TrimEnd('/') + mLayout + "/portal.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
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
            if (sitePortalProperties.ContainsKey("PortalUrl") && !string.IsNullOrEmpty(sitePortalProperties["PortalUrl"] as string)
                || sitePortalProperties.ContainsKey("PortalName") && !string.IsNullOrEmpty(sitePortalProperties["PortalName"] as string))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl00$portalEnabled"] = "onetidPortalEnabled";
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl02$TxtPortalURL"] = sitePortalProperties.ContainsKey("PortalUrl") ? HttpUtility.UrlEncode(sitePortalProperties["PortalUrl"].ToString()) : formValues["ctl00$PlaceHolderMain$ctl00$ctl02$TxtPortalURL"];
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl03$TxtPortalName"] = sitePortalProperties.ContainsKey("PortalName") ? HttpUtility.UrlEncode(sitePortalProperties["PortalName"].ToString()) : formValues["ctl00$PlaceHolderMain$ctl00$ctl03$TxtPortalName"];
            }
            else
            {
                bodyDic["ctl00$PlaceHolderMain$ctl00$portalEnabled"] = "onetidPortalNotEnabled";
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);
            return sitePortal;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "siterss is a part of url")]
        public void UpdateSiteRssSetting(bool syndicationEnabled)
        {
            string netWorkUrl = mSiteUrl + mLayout + "/siterss.aspx";
            string contentType = "application/x-www-form-urlencoded";
            string html = AveHttpWebRequestUtility.HttpGet(netWorkUrl, mObj, this.tokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(netWorkUrl, mObj, contentType, data, null, this.tokenProvider);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "advsetng is a part of url")]
        public void UpdateListAdvancedSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> advancedSettingProp)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/advsetng.aspx?List=" + listId.ToString("B");
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
            if (string.IsNullOrEmpty(html))
            {
                mLogger.Warn("Update List Advanced Setting Failed. Page Url:{0}, Web Url:{1}.", postUrl, webServerRelativeUrl);
                return;
            }
            IList<string> formKeys = new List<string>();
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
            string searchContent = "var readSecurity = ";
            string information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, ";");
            if (!string.IsNullOrEmpty(information))
            {
                bodyDic["ctl00$PlaceHolderMain$ItemLevelSecuritySection$ctl09$ReadSecurity"] = Convert.ToInt32(information);
                searchContent = "var writeSecurity = ";
                information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, ";");
                bodyDic["ctl00$PlaceHolderMain$ItemLevelSecuritySection$ctl10$WriteSecurity"] = Convert.ToInt32(information);
            }
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl00$RptControls$BtnSaveAsTemplate";
            foreach (KeyValuePair<string, object> value in advancedSettingProp)
            {
                bodyDic[value.Key] = value.Value;
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);
        }
        public void UpdateListGeneralSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> generalSettingProp)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/ListGeneralSettings.aspx?List=" + listId.ToString("B");
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
            if (string.IsNullOrEmpty(html))
            {
                return;
            }
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl01$RptControls$BtnSave";
            foreach (KeyValuePair<string, object> pair in generalSettingProp)
            {
                bodyDic[pair.Key] = pair.Value;
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);
        }
        public void SetListVersionLimited(string webServerRelativeUrl, Guid listId, Dictionary<string, object> versionLimitedProperties)
        {
            mRequestCommon.SetListVersionLimited(webServerRelativeUrl, listId, versionLimitedProperties);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "editnav is a part of url")]
        public void MoveNavigationNodeToCollection(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties)
        {
            try
            {
                string postUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.Trim('/');
                int nodeId = (int)navigationNodeProperties["NodeId"];
                postUrl = postUrl + string.Format(mLayout + "/editnav.aspx?ID={0}", nodeId);
                int parentId = (int)navigationNodeProperties["NodeParentId"];
                string nodeTitle = navigationNodeProperties["NodeTitle"].ToString();

                string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
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
                AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", data, null, this.tokenProvider);
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

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Urls is a part of xml element attribute name")]
        private void GetNavigationSettingFromXml(string xml, out string addNewPagesToNavigation, out string createFriendlyUrlsForNewPages)
        {
            try
            {
                int startindex = xml.IndexOf("<NewPageSettings", StringComparison.OrdinalIgnoreCase);
                int endindex = xml.IndexOf(">", startindex, StringComparison.OrdinalIgnoreCase);
                string value = xml.Substring(startindex, endindex - startindex + 1);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(value);
                addNewPagesToNavigation = doc.DocumentElement.GetAttribute("AddNewPagesToNavigation");
                createFriendlyUrlsForNewPages = doc.DocumentElement.GetAttribute("CreateFriendlyUrlsForNewPages");
            }
            catch (Exception ex)
            {
                mLogger.Warn("Cannot get navigation settings from xml as it is invalid./r/n XML: {0},/r/n Error:{1}", xml, ex.ToString());
                addNewPagesToNavigation = string.Empty;
                createFriendlyUrlsForNewPages = string.Empty;
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "neglect the wrong spelling of the aspnet")]
        public bool RestoreNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/AreaNavigationSettings.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
            if (string.IsNullOrEmpty(html))
            {
                mLogger.Warn("Restore navigation failed.Page Url:{0}. Web Url:{1}", postUrl, webServerRelativeUrl);
                return false;
            }
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bool quickLaunchNaviShare = webAllProperties["__InheritCurrentNavigation"] != null && Convert.ToBoolean(webAllProperties["__InheritCurrentNavigation"]);
            bool topNaviShare = webAllProperties.ContainsKey("UseShared") && Convert.ToBoolean(webAllProperties["UseShared"]);
            bool backupFromInheritedWeb = webAllProperties.ContainsKey("BackupFromInheritedWeb") && Convert.ToBoolean(webAllProperties["BackupFromInheritedWeb"]);
            bool isOnlineSite = webAllProperties.ContainsKey("IsOnlineSite") && Convert.ToBoolean(webAllProperties["IsOnlineSite"]);
            if (!(quickLaunchNaviShare && topNaviShare && backupFromInheritedWeb))
            {
                nodes = HttpUtility.UrlEncode(nodes);
                if (!bodyDic.ContainsKey("nodes"))
                {
                    bodyDic["nodes"] = nodes;
                }
                else if (bodyDic.ContainsKey("nodes") && (!bodyDic["nodes"].ToString().Equals(nodes)))
                {
                    bodyDic["nodes"] = nodes;
                    bodyDic["ctl00$PlaceHolderMain$ctl05$RptControls$bottomOKButton"] = "OK";
                }
            }
            //Online Site is different from local site on this page.
            #region local site
            if (!isOnlineSite)
            {
                if (webAllProperties.ContainsKey("__GlobalNavigationIncludeTypes"))
                {
                    if (webAllProperties["__GlobalNavigationIncludeTypes"].ToString().Equals("1"))
                    {
                        bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl02$globalIncludeSubSites"] = "on";
                    }
                    else if (webAllProperties["__GlobalNavigationIncludeTypes"].ToString().Equals("2"))
                    {
                        bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl02$globalIncludePages"] = "on";
                    }
                    else if (webAllProperties["__GlobalNavigationIncludeTypes"].ToString().Equals("3"))
                    {
                        bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl02$globalIncludeSubSites"] = "on";
                        bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl02$globalIncludePages"] = "on";
                    }
                }
                if (webAllProperties.ContainsKey("__CurrentNavigationIncludeTypes"))
                {
                    if (webAllProperties["__CurrentNavigationIncludeTypes"].ToString().Equals("1"))
                    {
                        bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl02$currentIncludeSubSites"] = "on";
                    }
                    else if (webAllProperties["__CurrentNavigationIncludeTypes"].ToString().Equals("2"))
                    {
                        bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl02$currentIncludePages"] = "on";
                    }
                    else if (webAllProperties["__CurrentNavigationIncludeTypes"].ToString().Equals("3"))
                    {
                        bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl02$currentIncludeSubSites"] = "on";
                        bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl02$currentIncludePages"] = "on";
                    }
                }
                if (webAllProperties.ContainsKey("__GlobalDynamicChildLimit"))
                {
                    bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl02$globalDynamicChildLimit"] = webAllProperties["__GlobalDynamicChildLimit"];
                }
                if (webAllProperties.ContainsKey("__CurrentDynamicChildLimit"))
                {
                    bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl02$currentDynamicChildLimit"] = webAllProperties["__CurrentDynamicChildLimit"];
                }
            }
            #endregion
            #region Online Site
            else
            {
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
                    bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl03$currentDynamicChildLimit"] = webAllProperties["__CurrentDynamicChildLimit"];
                }
            }
            #endregion

            #region "_webnavigationsettings"  newPageNavItemCheckBox,newPageFriendlyUrlCheckBox value cannot be get from the default value in html,so we need to set it by navigation setting again

            if (webAllProperties.ContainsKey("_webnavigationsettings"))
            {
                string addNewPagesToNavigation = string.Empty;
                string createFriendlyUrlsForNewPages = string.Empty;
                GetNavigationSettingFromXml(webAllProperties["_webnavigationsettings"].ToString(), out addNewPagesToNavigation, out createFriendlyUrlsForNewPages);

                if (!string.IsNullOrEmpty(addNewPagesToNavigation)
                    && Convert.ToBoolean(addNewPagesToNavigation)
                    && bodyDic.ContainsKey("ctl00$PlaceHolderMain$newPageOptionsSection$ctl01$newPageNavItemCheckBox"))
                {
                    bodyDic["ctl00$PlaceHolderMain$newPageOptionsSection$ctl01$newPageNavItemCheckBox"] = "on";
                }
                if (!string.IsNullOrEmpty(createFriendlyUrlsForNewPages)
                    && Convert.ToBoolean(createFriendlyUrlsForNewPages)
                    && bodyDic.ContainsKey("ctl00$PlaceHolderMain$newPageOptionsSection$ctl01$newPageFriendlyUrlCheckBox"))
                {
                    bodyDic["ctl00$PlaceHolderMain$newPageOptionsSection$ctl01$newPageFriendlyUrlCheckBox"] = "on";
                }
            }

            #endregion

            if (webAllProperties.ContainsKey("__NavigationOrderingMethod"))
            {
                string OrderMethod = webAllProperties["__NavigationOrderingMethod"].ToString();
                switch (OrderMethod)
                {
                    case "0":
                        bodyDic["ctl00$PlaceHolderMain$ctl08$SortingMethodRadioGroup"] = "automaticSortingRadioButton";
                        break;
                    case "1":
                        bodyDic["ctl00$PlaceHolderMain$ctl08$SortingMethodRadioGroup"] = "manualSortingRadioButton";
                        bodyDic["ctl00$PlaceHolderMain$ctl08$automaticPageSortingCheckBox"] = "on";
                        break;
                    case "2":
                        bodyDic["ctl00$PlaceHolderMain$ctl08$SortingMethodRadioGroup"] = "manualSortingRadioButton";
                        break;
                    default: break;
                }
            }
            if (webAllProperties.ContainsKey("__NavigationAutomaticSortingMethod"))
            {
                string AutomaticMethod = webAllProperties["__NavigationAutomaticSortingMethod"].ToString();
                switch (AutomaticMethod)
                {
                    case "0":
                        bodyDic["ctl00$PlaceHolderMain$automaticSortingSection$automaticSortingMethodDropDown"] = "Title";
                        break;
                    case "1":
                        bodyDic["ctl00$PlaceHolderMain$automaticSortingSection$automaticSortingMethodDropDown"] = "CreatedDate";
                        break;
                    case "2":
                        bodyDic["ctl00$PlaceHolderMain$automaticSortingSection$automaticSortingMethodDropDown"] = "LastModifiedDate";
                        break;
                    default: break;
                }
            }
            if (webAllProperties.ContainsKey("__NavigationSortAscending"))
            {
                bodyDic["ctl00$PlaceHolderMain$automaticSortingSection$SortingDirectionRadioGroup"] = Convert.ToBoolean(webAllProperties["__NavigationSortAscending"]) ?
                                                                                                      "ascendingRadioButton" :
                                                                                                      "descendingRadioButton";

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
            if (webAllProperties.ContainsKey("UseShared"))
            {
                bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl02$TopNavInheritance"] = "inheritTopNavRadioButton";
            }
            //if (webAllProperties.ContainsKey("__NavigationShowSiblings"))
            //{
            //    bool showSiblings = Convert.ToBoolean(webAllProperties["__NavigationShowSiblings"]);
            //    if (showSiblings)
            //    {
            //        bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl02$LeftNavInheritance"] = "showSiblingsLeftNavRadioButton";
            //    }               
            //    if (webAllProperties.ContainsKey("__InheritCurrentNavigation"))
            //    {
            //        bool inheritCurrentNavigation = Convert.ToBoolean(webAllProperties["__InheritCurrentNavigation"]);
            //        if (inheritCurrentNavigation)
            //        {
            //            bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl02$LeftNavInheritance"] = "inheritLeftNavRadioButton";
            //        }
            //        else if (!showSiblings)
            //        {
            //            bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl02$LeftNavInheritance"] = "uniqueLeftNavRadioButton";
            //        }
            //    }
            //}            
            bodyDic.Remove(string.Empty);
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);
            return true;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "metadatacolsettings is a part of url")]
        public void UpdateMetadataListFieldSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/metadatacolsettings.aspx?List=" + listId.ToString("B");
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
            if (string.IsNullOrEmpty(html))
            {
                mLogger.Warn("Update Metadata List Field Settings failed. Page Url:{0}. Web Url:{1}.", postUrl, webServerRelativeUrl);
                return;
            }
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
            //ADO-55449 ,在更新时需要使用这两个属性。
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            if (updateProperties.ContainsKey("EnableKeywordsField") && (bool)updateProperties["EnableKeywordsField"])
            {
                bodyDic["ctl00$PlaceHolderMain$KeywordsSection$ctl01$CheckBoxEnterpriseKeywords"] = "on";
            }
            if (updateProperties.ContainsKey("EnableMetadataPromotion") && (bool)updateProperties["EnableMetadataPromotion"])
            {
                bodyDic["ctl00$PlaceHolderMain$MDPushSection$ctl01$CheckBoxPromoteMetadata"] = "on";
            }
            //CheckBoxPromoteMetadata false时不应设置此属性
            //else
            //{
            //    bodyDic["ctl00$PlaceHolderMain$MDPushSection$ctl01$CheckBoxPromoteMetadata"] = "off";
            //}
            bodyDic["ctl00$PlaceHolderMain$ctl00$RptControls$okButton"] = "OK";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);
        }
        public bool SetListRateSetting(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating, bool isLikesExp)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/RatingsSettings.aspx?List=" + listId.ToString("B");
            string html = SetRatedSetting(postUrl, enableRating);
            if (enableRating)
            {
                SetVotingExpSetting(postUrl, html, isLikesExp);
            }
            return true;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Rad is a part of Value")]
        private string SetRatedSetting(string postUrl, bool enableRating)
        {
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
            if (string.IsNullOrEmpty(html))
            {
                mLogger.Warn("Set Rated Setting Failed. Page Url:{0}.", postUrl);
                return null;
            }
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
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
                html = AveHttpWebRequestUtility.HttpReturn(postUrl, mObj, "application/x-www-form-urlencoded", body, null, string.Empty, this.tokenProvider);
            }
            else
            {
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl03$EnableRatings"] = "RadEnableRatingsNo";
                bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl00$ctl03$RadEnableRatingsNo";
                bodyDic["ctl00$PlaceHolderMain$ctl01$RptControls$BtnSave"] = "OK";
                body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
                AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);
            }
            return html;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Rad is a part of Keys")]
        private void SetVotingExpSetting(string postUrl, string html, bool isLikesExp)
        {
            if (string.IsNullOrEmpty(html))
            {
                mLogger.Warn("Set Voting Exp Setting failed. Page Url:{0}.", postUrl);
                return;
            }
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
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
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "listsyndication is a part of urls")]
        public void UpdateListRssSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProp)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/listsyndication.aspx?List=" + listId.ToString("B");
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
            //ADP-55450 这两个属性在post请求时必不可少
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }

            bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl00$RptControls$BtnApply";
            if (updateProp.ContainsKey("AllowRss")) bodyDic["ctl00$PlaceHolderMain$EnableRssSection$ctl01$Enabled"] = (bool)updateProp["AllowRss"] ? "EnabledTrue" : "EnabledFalse";
            if (updateProp.ContainsKey("LimitDescriptionLength")) bodyDic["ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl01$LimDesc"] = (bool)updateProp["LimitDescriptionLength"] ? "LimDescTrue" : "LimDescFalse";
            if (updateProp.ContainsKey("ChannelTitle")) bodyDic["ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl02$TxtChannelTitle"] = updateProp["ChannelTitle"].ToString();
            if (updateProp.ContainsKey("ChannelDescription")) bodyDic["ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl03$TxtChannelDescription"] = updateProp["ChannelDescription"].ToString();
            if (updateProp.ContainsKey("ChannelImageUrl")) bodyDic["ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl04$TxtChannelImageUrl"] = updateProp["ChannelImageUrl"].ToString();
            if (updateProp.ContainsKey("DocumentAsEnclosure"))
            {
                bodyDic["ctl00$PlaceHolderMain$EnclosuresSection$ctl01$FileEnclosure"] = (bool)updateProp["DocumentAsEnclosure"] ? "FileEnclosureTrue" : "FileEnclosureFalse";
            }
            if (updateProp.ContainsKey("DocumentAsLink"))
            {
                bodyDic["ctl00$PlaceHolderMain$EnclosuresSection$ctl02$FileLink"] = (bool)updateProp["DocumentAsLink"] ? "FileLinkTrue" : "FileLinkFalse";
            }
            if (updateProp.ContainsKey("ItemLimit")) bodyDic["ctl00$PlaceHolderMain$ItemLimitSection$ctl01$TxtItemLimit"] = updateProp["ItemLimit"];
            if (updateProp.ContainsKey("DayLimit")) bodyDic["ctl00$PlaceHolderMain$ItemLimitSection$ctl02$TxtDayLimit"] = updateProp["DayLimit"];
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);
        }
        public void SetSiteEnabledHelpCollections(string[] enabledHelpCollections)
        {
            string postUrl = mSiteUrl.TrimEnd('/') + mLayout + "/HelpSettings.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);
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
            for (int i = 0; i < 3; i++)
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
                            Feature feature = web.Features.Add(featureId, true, FeatureDefinitionScope.Site);
                            context.Load(feature);
                            context.ExecuteQuery();
                            featureProp["DefinitionId"] = featureId;
                            Dictionary<string, object> featureDefinitionProperties = new Dictionary<string, object>();
                            featureProp["Definition" + AveObjectModelConstant.ObjectPropertySuffix] = featureDefinitionProperties;
                        }
                        catch (Exception e)
                        {
                            mLogger.Error("failed to activate feature: {0} due to: {1}", featureId, e.ToString());
                        }
                        return featureProp;
                    }
                    featureProp = RestoreFeature(webServerRelativeUrl, featureId, force, scope, featuresSource);
                    if (IsFeatureActivated(webServerRelativeUrl, featureId, featuresSource) == FeatureStatus.Active)
                    {
                        return featureProp;
                    }
                    mLogger.Debug("The feature is not active,it will retry activating feature: {0} , times:{1}", featureId, i);
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

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "featurestatus is a part of Keys")]
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
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
            if (!string.IsNullOrEmpty(html))
            {
                HtmlDocument featurePage = new HtmlDocument();
                featurePage.LoadHtml(html);
                HtmlNode node = featurePage.DocumentNode.SelectSingleNode(string.Format("//div[@id='{0}']", featureId.ToString("D").ToLower(CultureInfo.InvariantCulture)));
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
                    if (featuresSource.Equals("web.features", StringComparison.OrdinalIgnoreCase))
                    {
                        return IsFeatureActivated(webServerRelativeUrl, featureId, "site.features");
                    }
                    else
                    {
                        return FeatureStatus.Hidden;
                    }
                }
            }
            return FeatureStatus.Deactive;
        }

        public void RestoreMasterPage(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebMasterPageInfo pageInfo, string alternateCssUrl)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + this.mLayout + "/ChangeSiteMasterPage.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
            if (string.IsNullOrEmpty(html))
            {
                mLogger.Warn("Restore Master Page Failed.Page Url:{0}. Web Url:{1}", postUrl, webServerRelativeUrl);
                return;
            }
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
            if (pageInfo.InheritingTheme)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl02$ctl01$inheritThemeCheckbox"] = "on";
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
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);
        }

        public Dictionary<string, object> UpdateAudit(Dictionary<string, object> needUpdateProperties)
        {
            return UpdateOnPremiseAudit(needUpdateProperties);
        }

        /// <summary>
        /// Update on-premise site collection audit settings.
        /// </summary>
        /// <param name="needUpdateProperties"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "auditsettings.aspx is a sharepoint setting page")]
        public Dictionary<string, object> UpdateOnPremiseAudit(Dictionary<string, object> needUpdateProperties)
        {
            //13 model
            if (needUpdateProperties.ContainsKey("AuditFlags"))
            {
                string postUrl = mSiteUrl.TrimEnd('/') + "/_layouts/15/auditsettings.aspx";
                string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);

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
                if (ResetOnPremiseTrimAuditLog(needUpdateProperties, bodyDic, formValues))
                {
                    ResetAuditFlags((int)needUpdateProperties["AuditFlags"], bodyDic);
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl03$RptControls$BtnCancelAuditSettings");
                    bodyDic.Remove("");
                    bodyDic["ctl00$PlaceHolderMain$ctl03$RptControls$BtnUpdateAuditSettings"] = "OK";

                    byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
                    AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);
                }
                else
                {
                    needUpdateProperties["UpdateAuditError"] = "An error occurred while updating audit property, please check your SharePoint Version, Foundation SharePoint do not have Audit function.";
                }
            }
            return needUpdateProperties;
        }

        private bool IsRadTrimAuditLogYes(Dictionary<string, object> needUpdateProperties, Dictionary<string, object> formValues, string trimAuditLogKey)
        {
            if (needUpdateProperties.ContainsKey("TrimAuditLog"))
            {
                return (bool)needUpdateProperties["TrimAuditLog"];
            }
            return ((string)formValues[trimAuditLogKey]).Equals("RadTrimAuditLogYes", StringComparison.OrdinalIgnoreCase);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "")]
        private bool ResetOnPremiseTrimAuditLog(Dictionary<string, object> needUpdateProperties, Dictionary<string, object> bodyDic, Dictionary<string, object> formValues)
        {
            if (formValues.ContainsKey("ctl00$PlaceHolderMain$ctl00$ctl04$trimAuditLog"))
            {
                string isEnable = (string)formValues["ctl00$PlaceHolderMain$ctl00$ctl04$trimAuditLog"];
                string radTrimAuditLogYes = "RadTrimAuditLogYes";
                string radTrimAuditLogNo = "RadTrimAuditLogNo";
                if (IsRadTrimAuditLogYes(needUpdateProperties, formValues, "ctl00$PlaceHolderMain$ctl00$ctl04$trimAuditLog"))
                {

                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl04$trimAuditLog"] = radTrimAuditLogYes;
                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl05$TxtTrimRetention"] = needUpdateProperties.ContainsKey("AuditLogTrimmingRetention") ? (int)needUpdateProperties["AuditLogTrimmingRetention"] : formValues["ctl00$PlaceHolderMain$ctl00$ctl05$TxtTrimRetention"];
                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl06$TxtReportStorageLocation"] = needUpdateProperties.ContainsKey("_auditlogreportstoragelocation") ? (string)needUpdateProperties["_auditlogreportstoragelocation"] : formValues["ctl00$PlaceHolderMain$ctl00$ctl06$TxtReportStorageLocation"];
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl03$RptControls$BtnCancelAuditSettings");
                }
                else
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl04$trimAuditLog"] = radTrimAuditLogNo;
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl00$ctl05$TxtTrimRetention");
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl00$ctl06$TxtReportStorageLocation");
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl03$RptControls$BtnCancelAuditSettings");
                }
                return true;
            }
            return false;
        }

        private void ResetAuditFlags(int auditFlags, Dictionary<string, object> bodyDic)
        {
            #region convert flags
            if ((auditFlags & (int)AveAuditMaskType.View) > 0)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditView"] = "on";
            }
            else
            {
                bodyDic.Remove("ctl00$PlaceHolderMain$ctl01$ctl01$CheckBoxAuditView");
            }
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
            #endregion
        }

        #endregion

        #region Add

        public Dictionary<string, object> AddBestBet(string term, List<string> bestBetUrlList, Dictionary<string, object> bestBetProp, string action)
        {
            string url = mSiteUrl.TrimEnd('/') + string.Format(mLayout + "/Keyword.aspx?k={0}", term);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, mObj, this.tokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(url, mObj, "application/x-www-form-urlencoded", data, null, this.tokenProvider);
            return bestBetProp;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Dlg is a part of IsDlg")]
        private string AddBestBet(string term, string bestBetTitle, string bestBetUrl, string bestBetDescription)
        {
            string url = mSiteUrl.TrimEnd('/') + string.Format(mLayout + "/BestBet.aspx?k={0}&IsDlg=1", term);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, mObj, this.tokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(url, mObj, "application/x-www-form-urlencoded", data, null, this.tokenProvider);
            string bestBet = string.Format("{0};{1};{2}", bestBetUrl, bestBetTitle, bestBetDescription);
            return bestBet;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Dlg is a part of IsDlg")]
        private string AddExistBestBet(string term, string bestBetUrl)
        {
            string url = mSiteUrl.TrimEnd('/') + string.Format(mLayout + "/BestBet.aspx?k={0}&IsDlg=1", term);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, mObj, this.tokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(url, mObj, "application/x-www-form-urlencoded", data, null, this.tokenProvider);
            string bestBet = string.Format("{0};;;", bestBetUrl);
            return bestBet;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Dlg is a part of IsDlg=1")]
        private string EditBestBet(string term, string bestBetTitle, string bestBetUrl, string bestBetDescription)
        {
            string a = string.Format("{0};;;", bestBetUrl);
            string url = mSiteUrl.TrimEnd('/') + mLayout + "/BestBet.aspx?";
            string postUrl = string.Format("{0}u={1}&k={2}&a={3}&IsDlg=1", url, bestBetUrl, term, a);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", data, null, this.tokenProvider);
            return a;
        }

        public Dictionary<string, object> AddKeyWord(string term, DateTime startDate, int localId, int calendarType)
        {
            string url = mSiteUrl.TrimEnd('/') + mLayout + "/Keyword.aspx";
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, mObj, this.tokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(url, mObj, "application/x-www-form-urlencoded", data, null, this.tokenProvider);
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "syn is a part of Keys")]
        public string AddSynonm(string term, string synTerm, string terms)
        {
            string url = mSiteUrl.TrimEnd('/') + string.Format(mLayout + "/Keyword.aspx?k={0}", term);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, mObj, this.tokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(url, mObj, "application/x-www-form-urlencoded", data, null, this.tokenProvider);
            return synTerm;
        }

        public void AddSitePolicy(string policySchema, string siteUrl)
        {
            AveSitePolicyInfo policyInfo = new AveSitePolicyInfo();
            policyInfo.LoadFromXml(policySchema);
            string contentType = "application/x-www-form-urlencoded";
            string settingUrl = siteUrl + "/_layouts/15/projectpolicyconfig.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(settingUrl, mObj, this.tokenProvider);
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
            bodyDic["ctl00$PlaceHolderMain$ctl00$ctl02$textBoxDescription"] = policyInfo.Description;
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
            AveHttpWebRequestUtility.HttpPost(settingUrl, mObj, contentType, data, null, this.tokenProvider);
        }

        public Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> data)
        {
            Dictionary<string, object> featureProp = new Dictionary<string, object>();
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            string realListUrl = this.WebAppName.TrimEnd('/') + listUrl;
            string netWorkUrl = string.Empty;
            if (itemId == -2)
            {
                netWorkUrl = webFullUrl.TrimEnd('/') + mLayout + "/SubNew.aspx?List=" + listId.ToString("B");//&Source=" +realListUrl + "?AjaxDelta=1&IsDlg=1";
            }
            else
            {
                AveHttpValueCollection values = new AveHttpValueCollection();
                values["List"] = listId.ToString("B");
                values["ID"] = itemId.ToString();
                netWorkUrl = webFullUrl.TrimEnd('/') + mLayout + "/SubNew.aspx?" + values.ToString(true);//List={" + listId.ToString() + "}&ID=" + itemId.ToString();// +"&Source=" + realListUrl + "?IsDlg=1";
            }
            string html = AveHttpWebRequestUtility.HttpGet(netWorkUrl, this.mObj, this.tokenProvider);
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
            bodyDic["ctl00$PlaceHolderMain$ctl03$ctl01$TextTitle"] = HttpUtility.UrlEncode(data["AlertTitle"].ToString());
            bodyDic["ctl00$PlaceHolderMain$ctl05$ctl02$rdoDC"] = "rdo_EmailDC";
            if (data.ContainsKey("AlertTemplateName") && data.ContainsKey("Filter"))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl07$ctl02$RadioBtnAlertFilter"] = AveHttpWebRequestUtility.GetFilterValue(data);
            }
            if (data.ContainsKey("ViewId"))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl07$ctl02$DdlView"] = data["ViewId"].ToString();
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
            AveHttpWebRequestUtility.HttpPost(netWorkUrl, this.mObj, contentType, body, null, this.tokenProvider);
            Dictionary<string, object> featureDefinitionProperties = new Dictionary<string, object>();
            featureProp["Definition" + AveObjectModelConstant.ObjectPropertySuffix] = featureDefinitionProperties;
            return featureProp;
        }
        #endregion

        #region private function
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Ddlweb is a part of Keys")]
        private Dictionary<string, object> GetPostBody(string postUrl, Dictionary<string, object> bodyDic)
        {
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Ddlweb is a part of Keys")]
        private void UpdateLocal(string postUrl, string html, Dictionary<string, object> bodyDic, Dictionary<string, object> regionalProp)
        {
            bodyDic["Cmd"] = "UPDATEPROJECT";
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl02%24ctl01%24DdlwebLCID";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", data, null, this.tokenProvider);
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl06%24RptControls%24BtnUpdateRegionalSettings";
            byte[] newData = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", newData, null, this.tokenProvider);
            this.GetPostBody(postUrl, bodyDic);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Ddlweb is a part of value")]
        private void UpdateCalendar(string postUrl, string html, Dictionary<string, object> bodyDic, Dictionary<string, object> regionalProp)
        {
            bodyDic["Cmd"] = "UPDATEPROJECT";
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl03%24ctl01%24DdlwebCalType";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", data, null,this.tokenProvider);
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl06%24RptControls%24BtnUpdateRegionalSettings";
            byte[] newData = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", newData, null, this.tokenProvider);
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

        private Dictionary<string, object> RestoreFeature(string webServerRelativeUrl, Guid featureId, bool force, int scope, string featuresSource)
        {
            Dictionary<string, object> featureProp = new Dictionary<string, object>();
            Dictionary<string, object> featureDefinitionProperties;
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            string postUrl = string.Empty;
            bool isFindFeatureButton = false;//Find active button from html.
            switch (featuresSource)
            {
                case "web.features":
                    postUrl = webFullUrl.TrimEnd('/') + mLayout + "/ManageFeatures.aspx";
                    break;
                case "site.features":
                    postUrl = webFullUrl.TrimEnd('/') + mLayout + "/ManageFeatures.aspx?Scope=Site";
                    break;
            }
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
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
            int index = html.IndexOf("<div id='" + featureId.ToString() + "'", StringComparison.OrdinalIgnoreCase);
            isFindFeatureButton = index != -1;
            if (!isFindFeatureButton)
            {
                if (featuresSource.Equals("web.features", StringComparison.OrdinalIgnoreCase))
                {
                    return RestoreFeature(webServerRelativeUrl, featureId, force, scope, "site.features");
                }
                else
                {
                    featureProp["DefinitionId"] = featureId;
                    featureDefinitionProperties = new Dictionary<string, object>();
                    featureProp["Definition" + AveObjectModelConstant.ObjectPropertySuffix] = featureDefinitionProperties;
                    return featureProp;
                }
            }
            string featureList = AveHttpWebRequestUtility.GetFeatureTarget(html, featureId.ToString());
            bodyDic["__EVENTTARGET"] = featureList;
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);

            featureProp["DefinitionId"] = featureId;
            featureDefinitionProperties = new Dictionary<string, object>();
            featureProp["Definition" + AveObjectModelConstant.ObjectPropertySuffix] = featureDefinitionProperties;
            return featureProp;
        }
        #endregion

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "designgallery is a part of Keys")]
        public Dictionary<string, object> GetThemeUrlForWeb(string webServerRelativeUrl)
        {
            Dictionary<string, object> ThemeDic = new Dictionary<string, object>();
            string getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/15/designgallery.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
            if (string.IsNullOrEmpty(html))
            {
                return ThemeDic;
            }
            string searchContent = "\"themedCssFolderUrl\"";
            int startIndex = html.IndexOf(searchContent, StringComparison.OrdinalIgnoreCase);
            if (startIndex > 0)
            {
                startIndex = html.IndexOf("\"", startIndex + searchContent.Length, StringComparison.OrdinalIgnoreCase);
                int endIndex = html.IndexOf("\"", ++startIndex, StringComparison.OrdinalIgnoreCase);
                string themeUrl = html.Substring(startIndex, endIndex - startIndex);
                ThemeDic["ThemedCssFolderUrl"] = themeUrl.TrimEnd('/');
            }
            return ThemeDic;
        }

        /// <summary>
        /// add list with listTemplate(support sharepoint13/O365,sharepoint10 can use AveWebServiceRequest.AddList(string webServerRelativeUrl, string title, string description, IAveListTemplate listTemplate))
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="listTemplate"></param>
        /// <returns>return null</returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "onetid is html element")]
        public Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, IAveListTemplate listTemplate)
        {
            StringBuilder postUrl = new StringBuilder();
            postUrl.Append(WebAppName + webServerRelativeUrl.TrimEnd('/'));
            postUrl.Append(String.Format("/_layouts/15/new.aspx?CustomTemplate={0}", listTemplate.InternalName));
            postUrl.Append(String.Format("&FeatureId={0}", listTemplate.FeatureId.ToString("B")));
            postUrl.Append(String.Format("&ListTemplate={0}&", listTemplate.Type_Client));
            string html = AveHttpWebRequestUtility.HttpGet(postUrl.ToString(), mObj, this.tokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(postUrl.ToString(), mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);
            return null;
        }

        /// <summary>
        /// Need to be optimized
        /// </summary>
        /// <param name="parameters"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "downlevel is a sharepoint setting page attribute")]
        public void CustomizeReport(Dictionary<string, object> parameters)
        {
            //URl likes https://offo.sharepoint.com/_layouts/15/CustomizeReport.aspx?ReportId=f43c916f-4450-4737-b889-8078c9826841&Category=Auditing
            string postUrl = mSiteUrl.TrimEnd('/') + "/_layouts/15/CustomizeReport.aspx?ReportId=f43c916f-4450-4737-b889-8078c9826841&Category=Auditing";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, this.tokenProvider);
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
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl01$CheckBoxAuditView"] = "on";
            }
            if (parameters.ContainsKey("Update") && ((string)parameters["Update"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl01$CheckBoxAuditUpdate"] = "on";
            }
            if (parameters.ContainsKey("CheckInOut") && ((string)parameters["CheckInOut"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl01$CheckBoxAuditCheckInOut"] = "on";
            }
            if (parameters.ContainsKey("MoveCopy") && ((string)parameters["MoveCopy"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl01$CheckBoxAuditMoveCopy"] = "on";
            }
            if (parameters.ContainsKey("DeleteRestore") && ((string)parameters["DeleteRestore"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl01$CheckBoxAuditDeleteRestore"] = "on";
            }
            if (parameters.ContainsKey("ColumnContentType") && ((string)parameters["ColumnContentType"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl01$CheckBoxAuditColumnContentType"] = "on";
            }
            if (parameters.ContainsKey("Search") && ((string)parameters["Search"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl01$CheckBoxAuditSearch"] = "on";
            }
            if (parameters.ContainsKey("Perms") && ((string)parameters["Perms"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl01$CheckBoxAuditPerms"] = "on";
            }
            if (parameters.ContainsKey("Change") && ((string)parameters["Change"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl01$CheckBoxAuditChange"] = "on";
            }
            if (parameters.ContainsKey("Workflow") && ((string)parameters["Workflow"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl01$CheckBoxAuditWorkflow"] = "on";
            }
            if (parameters.ContainsKey("Custom") && ((string)parameters["Custom"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl04$ifsEvents$ctl01$CheckBoxAuditCustom"] = "on";
            }

            bodyDic["ctl00$PlaceHolderMain$ctl01$RptControls$btnOK"] = "Ok";

            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);

            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, this.tokenProvider);
        }

        public void UpdateFileProperties(string webServerRelativeUrl, string fileServerRelativeUrl, Dictionary<string, object> properties)
        {
            mRequestCommon.UpdateFileProperties(webServerRelativeUrl, fileServerRelativeUrl, properties);
        }
        /// <summary>
        /// return installedlanguagesLCID
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <returns></returns>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Dd is html element")]
        public List<Dictionary<string, object>> GetInstalledLanguages(string webServerRelativeUrl)
        {
            StringBuilder postUrl = new StringBuilder();
            List<Dictionary<string, object>> installedLanguages = new List<Dictionary<string, object>>();
            postUrl.Append(WebAppName + webServerRelativeUrl.TrimEnd('/'));
            postUrl.Append("/_layouts/15/newsbweb.aspx");
            string html = AveHttpWebRequestUtility.HttpGet(postUrl.ToString(), mObj, this.tokenProvider);
            string searchContent = "<select name=\"ctl00$PlaceHolderMain$InputFormTemplatePickerControl$ctl00$DDLanguageFormControl$DdLanguageWebTemplate\"";
            string endContent = "</select>";

            string content = AveHttpWebRequestUtility.GetInput(html, searchContent, endContent);
            if (String.IsNullOrEmpty(content))
            {
                return installedLanguages;
            }
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);
            foreach (XmlNode node in doc.FirstChild.ChildNodes)
            {
                if (node.Attributes.Count > 0)
                {
                    Dictionary<string, object> languageProperties = new Dictionary<string, object>();
                    languageProperties["LCID"] = Convert.ToInt32(node.Attributes["value"].Value);
                    //languageProperties["DisplayName"] = node.InnerText;
                    installedLanguages.Add(languageProperties);
                }
            }
            return installedLanguages;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "HTML Element Name")]
        public Dictionary<string, object> GetMasterPageProperties(string webServerRelativeUrl)
        {
            Dictionary<string, object> masterPropDic = new Dictionary<string, object>();
            string getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + this.mLayout + "/ChangeSiteMasterPage.aspx";
            string html = string.Empty;
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, this.tokenProvider);
            }
            catch (Exception e)
            {
                mLogger.Warn("Get master page properties failed. Message:{0}", e.ToString());
                return masterPropDic;
            }
            //2013 Client API contains MasterUrl and CustomMasterUrl property.
            string searchContent = "<input name=\"ctl00$PlaceHolderMain$ctl03$ctl01$alternateCssSelector$AssetUrlInput\"";
            string infomation = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(infomation);
            masterPropDic["AlternateCssUrl"] = doc.FirstChild.Attributes["value"] != null ? doc.FirstChild.Attributes["value"].Value : default(string);
            return masterPropDic;
        }

        public void DeclareOrUndeclareItem(int itemId, Guid listId, string webUrl)
        {
            mRequestCommon.DeclareOrUndeclareItem(itemId, listId, webUrl);
        }

        public void UpdateWorkflowAssociationsOnChildren(string webUrl, string contentTypeId)
        {
            mRequestCommon.UpdateWorkflowAssociationsOnChildren(webUrl, contentTypeId);
        }

        private string GetCookie(CookieContainer cookies)
        {
            if (cookies.GetCookies(new Uri(this.mSiteUrl))["SPOIDCRL"] != null)
            {
                return cookies.GetCookies(new Uri(this.mSiteUrl))["SPOIDCRL"].ToString();
            }
            if (cookies.GetCookies(new Uri(this.mSiteUrl))["FedAuth"] != null)
            {
                return cookies.GetCookies(new Uri(this.mSiteUrl))["FedAuth"].ToString();
            }
            throw new Exception(string.Format("Can not find cookie by site url: {0}.", this.mSiteUrl));
        }

        public Guid PublishNintexWorkflow(System.IO.Stream stream, string publishName, string tenant, string siteServerRelativeUrl, string listName, bool overWrite)
        {
            Uri hosUrl = GenerateHostUrl(publishName, tenant, siteServerRelativeUrl, listName, overWrite);

            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(hosUrl);
            webrequest.Method = "POST";

            string cookie = GetCookie(this.mObj as CookieContainer);

            webrequest.Headers.Add("Set-Cookie", cookie);
            webrequest.ContentType = "application/octet-stream";

            webrequest.ContentLength = stream.Length;
            using (System.IO.Stream requestStream = webrequest.GetRequestStream())
            {
                stream.Position = 0;
                byte[] buffer = new Byte[checked((uint)Math.Min(4096,
                             (int)stream.Length))];
                int bytesRead = 0;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                    requestStream.Write(buffer, 0, bytesRead);
                using (WebResponse responce = webrequest.GetResponse())
                {
                    System.IO.Stream s = responce.GetResponseStream();
                    System.IO.StreamReader sr = new System.IO.StreamReader(s);
                    return GetWorkflowIdFromResponse(sr.ReadToEnd());
                }
            }
        }

        private Guid GetWorkflowIdFromResponse(string response)
        {
            Guid workflowId = Guid.Empty;
            string prefix = "\"workflowId\":\"";
            var index = response.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                index += prefix.Length;
                Guid.TryParse(response.Substring(index, response.LastIndexOf("\"}", StringComparison.OrdinalIgnoreCase) - index), out workflowId);
            }
            return workflowId;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "clientarea and workflowo365 are a part of values")]
        private Uri GenerateHostUrl(string publishName, string tenant, string siteServerRelativeUrl, string listName, bool overWrite)
        {
            const string APIURL = "https://workflowo365.nintex.com//api/clientarea/version1/PublishAPI";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"{0}?workflowName={1}&overwrite={2}&tenant={3}&sitePath={4}"
                , APIURL, publishName, overWrite, tenant, siteServerRelativeUrl);
            if (!string.IsNullOrEmpty(listName))
            {
                sb.AppendFormat("&listName={0}", listName);
            }
            return new Uri(sb.ToString());
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "RPC Protocol")]
        public void MoveTo(string webServerRelativeUrl, string oldUrl, string newUrl)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_vti_bin/_vti_aut/author.dll";
            Dictionary<string, object> body = new Dictionary<string, object>();
            body["method"] = string.IsNullOrEmpty(sharepointVersion) ? "move document"
                : string.Format("move document:{0}", sharepointVersion);
            body["service_name"] = webServerRelativeUrl.TrimEnd('/');
            body["oldUrl"] = GetPostUrl((oldUrl.StartsWith(webServerRelativeUrl, StringComparison.OrdinalIgnoreCase) ? oldUrl.Substring(webServerRelativeUrl.Length) : oldUrl).TrimStart('/'));
            body["newUrl"] = GetPostUrl((newUrl.StartsWith(webServerRelativeUrl, StringComparison.OrdinalIgnoreCase) ? newUrl.Substring(webServerRelativeUrl.Length) : newUrl).TrimStart('/'));
            body["url_list"] = "[]";
            body["rename_option"] = "findbacklinks";
            body["put_option"] = "edit";

            Dictionary<string, object> headerInformation = new Dictionary<string, object>();
            headerInformation["MIME-Version"] = "1.0";
            headerInformation["X-Vermeer-Content-Type"] = "application/x-www-form-urlencoded";
            byte[] bodyContent = AveHttpWebRequestUtility.GetByte(body, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", bodyContent, headerInformation, this.tokenProvider);
        }
        string GetPostUrl(string url)
        {
            List<char> specialChars = new List<char> { '=', '[', ']' };
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }
            StringBuilder sBuilder = new StringBuilder();
            foreach (var ch in url)
            {
                if (specialChars.Contains(ch))
                {
                    sBuilder.Append('\\');
                }
                sBuilder.Append(ch);
            }
            return HttpUtility.UrlEncode(sBuilder.ToString());
        }
    }
}
