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
using System.Web;
using System.Xml;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Office365.Api;

namespace AveClientRequest.Common
{
    [AveCodeReview("2012/11/15", "cbi@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_FA_4 }, "ADO-53377", true)]
    public class AveHttpWebRequestCommon
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveHttpWebRequestCommon));
        private object mObj;
        private string mSiteUrl;
        private string mWebAppName;
        private string mLayout;

        private ITokenProvider tokenProvider;
        public ITokenProvider TokenProvider
        {
            set
            {
                tokenProvider = value;
            }
        }

        public AveHttpWebRequestCommon(string siteUrl, object obj, int sharepointVersion)
        {
            this.mSiteUrl = siteUrl;
            mWebAppName = GetWebAppName(siteUrl);
            mObj = obj;
            if (sharepointVersion == 15)
            {
                mLayout = "/_layouts/15";
            }
            else
            {
                mLayout = "/_layouts";
            }
        }


        internal string WebAppName
        {
            get
            {
                return mWebAppName;
            }
        }
        private string GetWebAppName(string siteUrl)
        {
            int indexOfSlash = siteUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase);
            string webAppName = siteUrl;
            if (indexOfSlash != -1)
            {
                webAppName = siteUrl.Substring(0, siteUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase));
            }
            return webAppName;
        }

        #region  common
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "WrkSetng is a part of url")]
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
                values["ctype"] = contentTypeProp["ContentTypeId"].ToString();
                getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/WrkSetng.aspx?" + values.ToString(true);//List=" + listId + "&ctype=" + contentTypeProp["ContentTypeId"];
            }
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, tokenProvider);
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ms-vb:A part of WorkflowProperty value.")]
        private void GetWorkflowProperties(string information, Dictionary<string, object> workflowAssociationsProp)
        {
            string tempStr = AveHttpWebRequestUtility.GetInput(information, "<a", "</a>");
            string name = AveHttpWebRequestUtility.GetInnerText(tempStr, ">", "<");
            int value = Convert.ToInt32(AveHttpWebRequestUtility.GetInnerText(information, "<td class=\"ms-vb\" nowrap=\"nowrap\">", "</td>"));
            workflowAssociationsProp.Add(name, value);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "qlreord and tnreord are a part of url")]
        public void MoveNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> previousNodeProperties, string moveMethodName)
        {
            try
            {
                string source = navigationNodeProperties["NodeSource"].ToString();
                string postUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
                if (source.Equals("QuickLaunch", StringComparison.Ordinal))
                {
                    postUrl = postUrl.TrimEnd('/') + mLayout + "/qlreord.aspx";
                }
                else if (source.Equals("TopNavigationBar", StringComparison.Ordinal))
                {
                    postUrl = postUrl.TrimEnd('/') + mLayout + "/tnreord.aspx";
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
                string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, tokenProvider);
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
                AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", data, null, tokenProvider);
            }
            catch (Exception ex)
            {
                mLogger.Error("Move navigation failed.Web:{0}.Error Message:{1}.", webServerRelativeUrl, ex.ToString());
                throw new AveWrapperBaseException(AveInternalResourceKey.Wrapper_Exception_Office365_RequestCommon_MoveNavigationFailed);
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Setng is a part of Url and owssvr is a part of dll")]
        public void SetListVersionLimited(string webServerRelativeUrl, Guid listId, Dictionary<string, object> versionLimitedProperties)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + mLayout + "/LstSetng.aspx?List=" + listId.ToString("B");//{" + listId + "}";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, tokenProvider);
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
            if (bodyDic.ContainsKey("VersioningEnabled") && bodyDic.ContainsKey("EnableMinorVersions"))
            {
                if (bodyDic["VersioningEnabled"].Equals("2"))
                {//VersioningEnabled为2表示开小version，true表示开大version，false表示不开。此处会获取EnableMinorVersions不对，需把它reset回去。
                    bodyDic["EnableMinorVersions"] = true;
                }
            }
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
                if (bodyDic.ContainsKey("EnableMinorVersions"))
                {
                    bodyDic.Remove("EnableMinorVersions");
                }
                int majorWithMinorVersionsLimit = (int)versionLimitedProperties["MajorWithMinorVersionsLimit"];
                bodyDic["MajorWithMinorVersionsLimit"] = majorWithMinorVersionsLimit;
                if (majorWithMinorVersionsLimit != 0)
                {
                    bodyDic["MajorMinorVersionLimitEnabled"] = true;
                }
            }
            bodyDic["Cmd"] = "MODLISTSETTINGS";
            bodyDic["List"] = listId;
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_vti_bin/owssvr.dll?CS=65001";
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, tokenProvider);
        }

        public static void OperateOnVersion(string webServerRelativeUrl, string webAppName, object obj, string listUrl, int itemId, int versionId, string listId, string fileName, string op, string layout)
        {
            OperateOnVersion(webServerRelativeUrl, webAppName, obj, listUrl, itemId, versionId, listId, fileName, op, layout, null);
        }

        public static void OperateOnVersion(string webServerRelativeUrl, string webAppName, object obj, string listUrl, int itemId, int versionId, string listId, string fileName, string op, string layout, ITokenProvider tokenProvider)
        {
            string url = webAppName.TrimEnd('/') + "/" + webServerRelativeUrl.Trim('/') + layout + "/Versions.aspx?";
            string source = webAppName.TrimEnd('/') + "/" + listUrl.Trim('/') + "?" + "InitialTabId=Ribbon%2EListItem" + "&VisibilityContext=WSSTabPersistence";
            string col = "Number";
            string order = "d";
            string isDlg = "1";

            string getUrl = GetUrl(url, fileName, listId, itemId.ToString(), null, null, source, null, null, isDlg, "get");
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, obj, tokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(postUrl, obj, contentType, body, null, tokenProvider);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "RPC Protocol")]
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
            Dictionary<string, object> headerInformation = new Dictionary<string, object>();
            headerInformation["MIME-Version"] = "1.0";
            //headerInformation["User-Agent"] = "MSFrontPage/15.0";
            headerInformation["X-Vermeer-Content-Type"] = "application/x-www-form-urlencoded";
            byte[] bodyContent = AveHttpWebRequestUtility.GetByte(body, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", bodyContent, headerInformation, tokenProvider);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "RPC Protocol")]
        public Dictionary<string, string> GetMetaInfo(string webServerRelativeUrl, string docServerRelativeUrl)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_vti_bin/_vti_aut/author.dll";
            Dictionary<string, object> body = new Dictionary<string, object>();
            body["method"] = "getDocsMetaInfo";
            body["url_list"] = HttpUtility.UrlEncode("[" + docServerRelativeUrl.Substring(webServerRelativeUrl.Length + 1) + "]");
            body["listHiddenDocs"] = "false";
            body["listLinkInfo"] = "false";
            Dictionary<string, object> headerInformation = new Dictionary<string, object>();
            headerInformation["MIME-Version"] = "1.0";
            headerInformation["X-Vermeer-Content-Type"] = "application/x-www-form-urlencoded";
            string metaInfoHtml = AveHttpWebRequestUtility.HttpReturn(postUrl, mObj, "application/x-www-form-urlencoded", AveHttpWebRequestUtility.GetByte(body, null), headerInformation,null, tokenProvider);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(metaInfoHtml);
            var nodes = doc.DocumentNode.SelectNodes("*//ul");
            string metaInfoString = nodes[nodes.Count - 1].InnerText.Trim(new char[] { '\r', '\n' });
            MetaInfoHandler handler = new MetaInfoHandler(metaInfoString, true);
            return handler.ToStringDictionary();
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

        #endregion

        #region private
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "col and Dlg are a part of url")]
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

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Post Update sharepoint.")]
        public void DeclareOrUndeclareItem(int itemId, Guid listId, string webUrl)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string postUrl = webUrl + mLayout + "/itemexpiration.aspx?" + string.Format("ID={0}&List={1}", itemId, "{" + listId.ToString() + "}");
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, tokenProvider);
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
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
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, tokenProvider);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "WrkSetng is Url.")]
        public void UpdateWorkflowAssociationsOnChildren(string webUrl, string contentTypeId)
        {
            string postUrl = webUrl + mLayout + "/WrkSetng.aspx?" + "ctype=" + contentTypeId;
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, tokenProvider);
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$lbUpdate";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, tokenProvider);
        }
        #endregion
    }
}
