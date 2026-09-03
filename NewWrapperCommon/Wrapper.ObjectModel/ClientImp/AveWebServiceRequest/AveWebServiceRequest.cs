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
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Reflection;
using System.Xml;
using System.Threading;
using System.IO;
using System.Globalization;
using System.Web;
using AvePoint.GCommon;
using AveClientRequest.Common;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.Client;
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon.Utility;
using System.Collections.ObjectModel;
using AvePoint.Office365.Api;

namespace AvePoint.ObjectModel.WebService
{
    [AveCodeReview("2012/11/15", "cbi@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_FA_4 }, "ADO-53377", true)]
    [AveCodeReview("2012/03/09", "Navy.Li@avepoint.com", "Bingkun.Wang@AvePoint.com",
        new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_1, 
                       CodeReviewConstants.CHECK_LIST_ID_CO_6, 
                       CodeReviewConstants.CHECK_LIST_ID_FA_4 }, null, true)]
    [AveCodeReview("2012/04/19", "yuzhi.jiang@avepoint.com", "yanjun.wang@avepoint.com",
        new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_3,
                       CodeReviewConstants.CHECK_LIST_ID_CO_1 }, null, true)]
    public class AveWebServiceRequest : IDisposable
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveWebServiceRequest));

        //private AveWebServiceNetWork mNetWork;
        private static string mHTTPUnauthorizedMessage = "The request failed with HTTP status 401: Unauthorized.";
        private static string mServerUnauthorizedMessage = "The remote server returned an error: (401) Unauthorized.";
        private AveWebServiceNetWork mNetWork;
        private string mWebUrl;
        private string mWebAppName;
        private object mObj;
        private object mCentralAdminObj;
        private string mServerVersion;
        private static SecurityTrimObject mSiteTrimObj;
        private AveBPOSAccountInfo mAccountInfo;
        private AveHttpWebRequestCommon mRequestCommon;
        private ITokenProvider mTokenProvider;

        public AveClientRequestType Type { get; private set; }
        public ITokenProvider TokenProvider
        {
            get { return mTokenProvider; }
            set
            {
                mRequestCommon.TokenProvider = value;
                mTokenProvider = value;
            }
        }

        public AveWebServiceRequest(string siteUrl, AveBPOSAccountInfo accountInfo, object obj, string serverVersion)
        {
            mObj = obj;
            mWebUrl = siteUrl;
            mServerVersion = serverVersion;
            mAccountInfo = accountInfo;
            Type = AveClientRequestType.AveWebServiceRequest;
            mRequestCommon = new AveHttpWebRequestCommon(mWebUrl, mObj, 14);
            //mNetWork = new AveWebServiceNetWork(accountInfo, siteUrl, obj);
        }

        public AveWebServiceRequest(string siteUrl, AveBPOSAccountInfo accountInfo, object obj, string serverVersion, SecurityTrimObject siteTrimObj)
        {
            mObj = obj;
            mWebUrl = siteUrl;
            mServerVersion = serverVersion;
            mAccountInfo = accountInfo;
            //mNetWork = new AveWebServiceNetWork(accountInfo, siteUrl, obj);
            mSiteTrimObj = siteTrimObj;
            Type = AveClientRequestType.AveWebServiceRequest;
            mRequestCommon = new AveHttpWebRequestCommon(mWebUrl, mObj, 14);
        }

        internal string WebAppName
        {
            get
            {
                if (mWebAppName == null)
                {
                    string siteUrl = mWebUrl;
                    int indexOfSlash = siteUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase);
                    mWebAppName = siteUrl;
                    if (indexOfSlash != -1)
                    {
                        mWebAppName = siteUrl.Substring(0, siteUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase));
                    }
                }
                return mWebAppName;
            }
        }

        public object Credentials
        {
            get
            {
                return mObj;
            }
            set
            {
                this.RefreshCredentials(value);
            }
        }
        public string Url
        {
            get
            {
                return this.mWebUrl;
            }
        }
        public AveRequestKind Kind
        {
            get
            {
                return AveRequestKind.WebService;
            }
        }
        //365有重写。
        #region Get
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "auditsettings.aspx is a sharepoint setting page")]
        public AveRequestAudit GetAuditValues()
        {
            AveRequestAudit requestAudit = new AveRequestAudit();
            string postUrl = mWebUrl.TrimEnd('/') + "/_layouts/AuditSettings.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj);

            if (string.IsNullOrEmpty(html))
            {
                return requestAudit;
            }

            Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(html);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input ";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            string view = "ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditView";
            string edit = "ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditEdit";
            string checkInOut = "ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditCheckInOut";
            string moveCopy = "ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditMoveCopy";
            string deleteRestore = "ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditDeleteRestore";
            string columnsContentType = "ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditColumnsContentType";
            string search = "ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditSearch";
            string perms = "ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditPerms";
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
            requestAudit.TrimAuditLog = GetTrimAuditLog(formValues, "ctl00$PlaceHolderMain$ctl00$ctl03$trimAuditLog");
            requestAudit.AuditLogTrimmingRetention = GetAuditLogTrimmingRetention(formValues, "ctl00$PlaceHolderMain$ctl00$ctl04$TxtTrimRetention");

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
        //public Dictionary<string, object> GetSite()
        //{
        //    Dictionary<string, object> siteProperties = new Dictionary<string, object>();
        //    Uri siteUri = new Uri(mWebUrl);
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
        //    {
        //        mNetWork.InitialNetWorker(AveWebServiceType.SiteData, siteUri.AbsoluteUri.TrimEnd('/'));
        //        siteProperties.Add("Url", siteUri.AbsoluteUri);
        //        siteProperties.Add("ServerRelativeUrl", GetServerRelativeUrl(siteUri.AbsoluteUri));
        //        siteProperties.Add("IsMoss", false);
        //        siteProperties.Add("IsPublish", false);
        //    }
        //    return siteProperties;
        //}

        //public Dictionary<string, object> GetAdminCenterSite()
        //{
        //    throw new NotImplementedException();
        //}

        //365有重写。
        public Dictionary<string, object> GetWebTemplates(string webServerRelativeUrl, uint lcid, bool doIncludeCrossLanguage, string webtemplateSource)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            Uri siteUri = new Uri(mWebUrl);
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.Sites, siteUri.AbsoluteUri);
                Sites.Template[] templates = new Sites.Template[] { };
                mNetWork.GetSiteTemplates(lcid, out templates);
                List<Dictionary<string, object>> templateList = new List<Dictionary<string, object>>();
                foreach (Sites.Template temp in templates)
                {
                    Dictionary<string, object> template = new Dictionary<string, object>();
                    AllDataToDictionary(template, new object[] { temp });
                    template["Lcid"] = lcid;
                    templateList.Add(template);
                }
                templates = null;
                returnInfo.Add(AveObjectModelConstant.ChildrenProperties, templateList);
            }
            return returnInfo;
        }
        //public Dictionary<string, object> GetFile(string webServerRelativeUrl, string serverRelativeUrl, string listName)
        //{
        //    string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
        //    Dictionary<string, object> dic = new Dictionary<string, object>();
        //    AveCamlQuery camlQueryNode = AveCamlQuery.CreateAllItemsQuery();
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
        //    {
        //        if (string.IsNullOrEmpty(listName) || camlQueryNode == null)
        //        {
        //            throw new NotImplementedException();
        //        }
        //        else
        //        {
        //            mNetWork.InitialNetWorker(AveWebServiceType.Lists, webFullUrl);
        //            XmlNode node = mNetWork.FileGetFile(serverRelativeUrl, listName, camlQueryNode.ToStringArray());
        //            GetItemAttributeFromXmlNode(node, dic);
        //            return dic;
        //        }
        //    }
        //}
        //public Dictionary<string, object> GetItems(string webServerRelativeUrl, string listName, Guid listId, string[] camlQueryNode)
        //{
        //    string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
        //    Dictionary<string, object> dic = new Dictionary<string, object>();
        //    XmlNode node = null;
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
        //    {
        //        mNetWork.InitialNetWorker(AveWebServiceType.Lists, webFullUrl);
        //        node = mNetWork.ListGetItems(listName, camlQueryNode);
        //    }
        //    GetItemAttributeFromXmlNode(node, dic);
        //    return dic;
        //}

         //365没有实现此方法。
        public int GetListItemRatings(string listItemUrl)
        {
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
            {
                try
                {
                    mNetWork.InitialNetWorker(AveWebServiceType.SocialDataService, mWebUrl);
                    return mNetWork.SocialDataGetRatings(listItemUrl);
                }
                catch (Exception e)
                {
                    mLogger.Error("Try get item Ratings Error : {0}", e.ToString());
                    return -1;
                }
            }
        }
        //public Dictionary<string, object> GetItem(string webServerRelativeUrl, string listName, Guid listId, int itemId, Guid uniqueId)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetContentTypes(string webServerRelativeUrl)
        //{
        //    string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
        //    XmlNode node = null;
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
        //    {
        //        mNetWork.InitialNetWorker(AveWebServiceType.Webs, webFullUrl);
        //        node = mNetWork.WebGetContentTypes();
        //    }
        //    return AddContentFromXmlNode(node, "ChildrenProperties");
        //}
        //public Dictionary<string, object> GetContentType(string webServerRelativeUrl, string contentTypeId)
        //{
        //    string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
        //    XmlNode node = null;
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
        //    {
        //        mNetWork.InitialNetWorker(AveWebServiceType.Webs, webFullUrl);
        //        try
        //        {
        //            node = mNetWork.WebGetContentType(contentTypeId);
        //        }
        //        catch (Exception ex)
        //        {
        //            //required contentType isn't in web contentType collection
        //            mLogger.Warn("Get web:{0} ContentType failed.Error Message:{1}.", webServerRelativeUrl, ex.ToString());
        //            return null;
        //        }
        //    }
        //    return GetContentTypeAttributeFromXmlNode(node);
        //}
        //public Dictionary<string, object> GetWeb(Guid webId)
        //{
        //    throw new NotImplementedException();
        //}

        //已经无用。
        public uint GetWebLanguage()
        {
            Uri siteUri = new Uri(mWebUrl);
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.SiteData, siteUri.AbsoluteUri);
                return mNetWork.GetWebLanguage();
            }
        }
        //已经无用
        public Dictionary<string, object> GetWeb(string webServerRelativeUrl)
        {
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            Dictionary<string, object> webProperties = new Dictionary<string, object>();
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
            {
                bool IsRootWeb = false;
                //
                string webName = webFullUrl.Substring(mWebUrl.Length);
                if (string.IsNullOrEmpty(webName))
                {
                    IsRootWeb = true;
                }
                mNetWork.InitialNetWorker(AveWebServiceType.Webs, webFullUrl);
                XmlNode node = mNetWork.WebGetWebProperties(string.Empty);//webUri.AbsoluteUri.TrimEnd('/'));
                webProperties = GetAttributeFromSingleXmlNode(node);
                webProperties.Add("IsRootWeb", IsRootWeb);
                webProperties.Add("Name", webName.Trim('/'));
                webProperties.Add("ServerRelativeUrl", webServerRelativeUrl);
                webProperties.Add("IsPublish", false);
            }
            return webProperties;
        }
        
        //public Dictionary<string, object> GetLists(string webServerRelativeUrl)
        //{
        //    Dictionary<string, object> listCollectionProperties = new Dictionary<string, object>();
        //    string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
        //    {
        //        mNetWork.InitialNetWorker(AveWebServiceType.Lists, webFullUrl);
        //        XmlNodeToDicValue(listCollectionProperties, mNetWork.WebGetLists());
        //    }
        //    return listCollectionProperties;
        //}
        //public Dictionary<string, object> GetSubWebs(string webServerRelativeUrl)
        //{
        //    string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
        //    XmlNode node = null;
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
        //    {
        //        mNetWork.InitialNetWorker(AveWebServiceType.Webs, webFullUrl);
        //        node = mNetWork.WebGetWebCollection();
        //    }
        //    return AddContentFromXmlNode(node, AveObjectModelConstant.ChildrenProperties);
        //}
        //public Dictionary<string, object> GetList(int id)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetContentTypes(string webServerRelativeUrl, string listName)
        //{
        //    string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
        //    XmlNode node = null;
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
        //    {
        //        mNetWork.InitialNetWorker(AveWebServiceType.Lists, webFullUrl);
        //        node = mNetWork.ListGetListContentTypes(listName);
        //    }
        //    return GetContentTypesAttributeFromXmlNode(node);
        //}
        //public Dictionary<string, object> GetContentType(string webServerRelativeUrl, string listName, string contentTypeId)
        //{
        //    string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
        //    XmlNode node = null;
        //    if (string.IsNullOrEmpty(contentTypeId))
        //    {
        //        throw new Exception("Parameter error.");
        //    }
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
        //    {
        //        mNetWork.InitialNetWorker(AveWebServiceType.Lists, webFullUrl);
        //        try
        //        {
        //            node = mNetWork.ListOrListItemGetListContentType(listName, contentTypeId);
        //        }
        //        catch (Exception ex)
        //        {
        //            //not in list contentType collection
        //            mLogger.Warn("Get List:{0} ContentType failed.Error Message:{1}.", webServerRelativeUrl + "/" + listName, ex.ToString());
        //            return null;
        //        }
        //    }
        //    return GetContentTypeAttributeFromXmlNode(node);
        //}
        //public Dictionary<string, object> GetFields(string webServerRelativeUrl, string listName)
        //{
        //    string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
        //    XmlNode node = null;
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
        //    {
        //        mNetWork.InitialNetWorker(AveWebServiceType.Lists, webFullUrl);
        //        node = mNetWork.ListGetListAttribute(listName);
        //    }
        //    return GetListAttributeFromXmlNode(node);
        //}
        //public Dictionary<string, object> GetForms(string webServerRelativeUrl, string listName, Guid listId)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetViews(string webServerRelativeUrl, string listName, Guid listId)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool HaveAddAndCustomizePagesPermission
        //{
        //    get { throw new NotImplementedException(); }
        //}
        //public Dictionary<string, object> GetAllWebs()
        //{
        //    Uri siteUri = new Uri(mWebUrl);
        //    Dictionary<string, object> rootWebProperties = this.GetWeb(GetServerRelativeUrl(siteUri.AbsoluteUri));
        //    Dictionary<string, object> subWebsProperties = this.GetSubWebs(siteUri.AbsoluteUri);
        //    List<Dictionary<string, object>> webs = new List<Dictionary<string, object>>();
        //    webs.Add(rootWebProperties);
        //    foreach (Dictionary<string, object> dic in subWebsProperties["WebCollection"] as List<Dictionary<string, object>>)
        //    {
        //        string subWebServerRelativeUrl = GetServerRelativeUrl(dic["Url"] as string);
        //        this.GetSubWebs(webs, subWebServerRelativeUrl, siteUri.AbsoluteUri);
        //    }
        //    Dictionary<string, object> allWebs = new Dictionary<string, object>();
        //    allWebs[AveObjectModelConstant.ChildrenProperties] = webs;
        //    return allWebs;
        //}
        //public Dictionary<string, object> GetRecycleBin(string webServerRelativeUrl = null)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetFolder(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl)
        //{
        //    string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
        //    Dictionary<string, object> returnInfo = new Dictionary<string, object>();
        //    AveCamlQuery camlQueryNode = AveCamlQuery.CreateAllItemsQuery();
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
        //    {
        //        if (string.IsNullOrEmpty(listName) || camlQueryNode == null)
        //        {
        //            //from web root folder request
        //            mNetWork.InitialNetWorker(AveWebServiceType.SiteData, webFullUrl);
        //            SiteData._sFPUrl[] urls = new SiteData._sFPUrl[] { };
        //            try
        //            {
        //                mNetWork.FolderEnumItems(folderServerRelativeUrl, out urls);
        //                returnInfo["Exists"] = true;
        //                returnInfo["ServerRelativeUrl"] = folderServerRelativeUrl;
        //            }
        //            catch (Exception ex)
        //            {
        //                mLogger.Warn("Folder:{0} is not exists.Error Message:{1}.", folderServerRelativeUrl, ex.ToString());
        //                returnInfo["Exists"] = false;
        //            }
        //            return returnInfo;
        //        }
        //        else
        //        {
        //            mNetWork.InitialNetWorker(AveWebServiceType.Lists, webFullUrl);
        //            try
        //            {
        //                XmlNode node = mNetWork.FolderGetItems(listName, folderServerRelativeUrl, camlQueryNode.ToStringArray());
        //                //GetItemAttributeFromXmlNode(node, returnInfo);
        //                returnInfo["Exists"] = true;
        //                returnInfo["ServerRelativeUrl"] = folderServerRelativeUrl;
        //            }
        //            catch (Exception ex)
        //            {
        //                mLogger.Warn("Items in folder:{0} is not exists.Error Message:{1}.", folderServerRelativeUrl, ex.ToString());
        //                returnInfo["Exists"] = false;
        //            }
        //            return returnInfo;
        //        }
        //    }
        //}
        //public Dictionary<string, object> GetFolders(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl)
        //{
        //    string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
        //    Dictionary<string, object> returnInfo = new Dictionary<string, object>();
        //    AveCamlQuery camlQueryNode = AveCamlQuery.CreateAllItemsQuery();
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
        //    {
        //        if (string.IsNullOrEmpty(listName) || camlQueryNode == null)
        //        {
        //            List<Dictionary<string, object>> foldersList = new List<Dictionary<string, object>>();
        //            //from web root folder request
        //            mNetWork.InitialNetWorker(AveWebServiceType.SiteData, webFullUrl);
        //            SiteData._sFPUrl[] urls = new SiteData._sFPUrl[] { };
        //            mNetWork.FolderEnumItems(folderServerRelativeUrl, out urls);
        //            if (urls != null && urls.Length != 0)
        //            {
        //                string subItemServerRelativeUrl = string.Empty;
        //                foreach (SiteData._sFPUrl url in urls)
        //                {
        //                    subItemServerRelativeUrl = folderServerRelativeUrl.TrimEnd('/') + "/" + url.Url;
        //                    if (url.IsFolder == true)
        //                    {
        //                        Dictionary<string, object> folderProp = new Dictionary<string, object>();
        //                        folderProp["ServerRelativeUrl"] = subItemServerRelativeUrl;
        //                        folderProp["Exists"] = true;
        //                        foldersList.Add(folderProp);
        //                    }
        //                }
        //            }
        //            returnInfo[AveObjectModelConstant.ChildrenProperties] = foldersList;
        //            return returnInfo;
        //        }
        //        else
        //        {
        //            mNetWork.InitialNetWorker(AveWebServiceType.Lists, webFullUrl);
        //            XmlNode node = mNetWork.FolderListGetSubFoldersOrFiles(listName, folderServerRelativeUrl, AveFileSystemObjectType.Folder, camlQueryNode.ToStringArray());
        //            GetItemAttributeFromXmlNode(node, returnInfo);
        //            return returnInfo;
        //        }
        //    }
        //}
        //public Dictionary<string, object> GetFiles(string webServerRelativeUrl, string listName, string folderServerRelativeUrl)
        //{
        //    string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
        //    Dictionary<string, object> returnInfo = new Dictionary<string, object>();
        //    AveCamlQuery camlQueryNode = AveCamlQuery.CreateAllItemsQuery();
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
        //    {
        //        if (string.IsNullOrEmpty(listName) || camlQueryNode == null)
        //        {
        //            //from web root folder request
        //            mNetWork.InitialNetWorker(AveWebServiceType.SiteData, webFullUrl);
        //            SiteData._sFPUrl[] urls = new SiteData._sFPUrl[] { };
        //            mNetWork.FolderEnumItems(folderServerRelativeUrl, out urls);
        //            if (urls == null || urls.Length == 0)
        //            {
        //                return returnInfo;
        //            }
        //            else
        //            {
        //                string subItemServerRelativeUrl = string.Empty;
        //                foreach (SiteData._sFPUrl url in urls)
        //                {
        //                    subItemServerRelativeUrl = folderServerRelativeUrl.TrimEnd('/') + "/" + url.Url;
        //                    if (url.IsFolder == false)
        //                    {
        //                        returnInfo[subItemServerRelativeUrl] = url.LastModified;
        //                    }
        //                }
        //                return returnInfo;
        //            }
        //        }
        //        else
        //        {
        //            mNetWork.InitialNetWorker(AveWebServiceType.Lists, webFullUrl);
        //            XmlNode node = mNetWork.FolderListGetSubFoldersOrFiles(listName, folderServerRelativeUrl, AveFileSystemObjectType.File, camlQueryNode.ToStringArray());
        //            GetItemAttributeFromXmlNode(node, returnInfo);
        //            return returnInfo;
        //        }
        //    }
        //}
        //public Dictionary<string, object> GetFileVersions(string webServerRelativeUrl, string fileServerRelativeUrl)
        //{
        //    string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
        //    string fileSiteRelativeUrl = string.Empty;
        //    try
        //    {
        //        fileSiteRelativeUrl = fileServerRelativeUrl.Substring(webServerRelativeUrl.Length);
        //    }
        //    catch (Exception ex)
        //    {
        //        mLogger.Error("Get file versions failed, Illegal Url.Url:{0}.Error Message:{1}.", fileSiteRelativeUrl, ex.ToString());
        //        throw new Exception("Illegal url.");
        //    }
        //    XmlNode node = null;
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
        //    {
        //        mNetWork.InitialNetWorker(AveWebServiceType.Versions, webFullUrl);
        //        node = mNetWork.FileGetVersions(fileSiteRelativeUrl.TrimStart('/').TrimEnd('/'));
        //    }
        //    return GetVersionsFromXmlNode(node);
        //}

        //365有重写。
        public Dictionary<string, object> GetItemVersions(string webServerRelativeUrl, string listId, int itemId, Dictionary<string, string> fields)
        {
            Dictionary<string, object> listItemVersionsProperties = new Dictionary<string, object>();
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            List<Dictionary<string, object>> itemVersionPropertiesList = new List<Dictionary<string, object>>();
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.Lists, webFullUrl);
                int i = 0;
                foreach (KeyValuePair<string, string> kv in fields)
                {
                    try
                    {
                        XmlNode node = mNetWork.ListGetVersionCollection(listId, itemId.ToString(), kv.Key);
                        for (int j = 0; j < node.ChildNodes.Count; j++)
                        {
                            XmlElement versionNode = node.ChildNodes[j] as XmlElement;
                            Dictionary<string, object> listItemVersionProperties = null;
                            if (i == 0 && j == 0)
                            {
                                for (int k = 0; k < node.ChildNodes.Count; k++)
                                {
                                    Dictionary<string, object> itemVersionProps = new Dictionary<string, object>();
                                    Dictionary<string, object> itemVersionFieldValues = new Dictionary<string, object>();
                                    itemVersionProps.Add("FieldValues", itemVersionFieldValues);
                                    itemVersionPropertiesList.Add(itemVersionProps);
                                }
                                listItemVersionProperties = itemVersionPropertiesList[j]["FieldValues"] as Dictionary<string, object>;
                            }
                            else
                            {
                                listItemVersionProperties = itemVersionPropertiesList[j]["FieldValues"] as Dictionary<string, object>;
                            }
                            if (i == 0)
                            {
                                listItemVersionProperties.Add("Modified", GetValueFromType("", versionNode.GetAttribute("Modified")));
                                listItemVersionProperties.Add("Editor", GetValueFromType("", versionNode.GetAttribute("Editor")));
                            }
                            listItemVersionProperties.Add(kv.Key, GetValueFromType(kv.Value, versionNode.GetAttribute(kv.Key)));
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Debug(AveWebServiceRequestResource.GetItemVersionsErrorWithIdString, e.ToString());
                    }
                    i++;
                }
            }
            listItemVersionsProperties.Add(AveObjectModelConstant.ChildrenProperties, itemVersionPropertiesList);
            return listItemVersionsProperties;
        }
        //13及以上版本已经使用client API
        public Dictionary<string, object> GetAttachments(string webRelativeUrl, string listTitle, int itemId)
        {
            Dictionary<string, object> attachProperties = new Dictionary<string, object>();
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webRelativeUrl.TrimStart('/');
            XmlNode attachmentNode = null;
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.Lists, webFullUrl);
                attachmentNode = mNetWork.ListGetAttachmentCollection(listTitle, itemId);
                List<Dictionary<string, object>> attachmentPropertiesList = GetAttachmentCollectionInfo(attachmentNode, WebAppName);
                attachProperties.Add("UrlCol", attachmentPropertiesList);
                attachProperties.Add("webAppName", this.WebAppName);
            }
            return attachProperties;
        }
        //public Dictionary<string, object> GetNavigation(string webServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}
        public Dictionary<string, object> GetUsers(string webRelativeUrl, string groupName, string userColSource)
        {
            Dictionary<string, object> siteUserColProperties = new Dictionary<string, object>();
            string webFullUrl = webRelativeUrl;
            if (!webRelativeUrl.StartsWith("Http", StringComparison.OrdinalIgnoreCase))
            {
                webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webRelativeUrl.TrimStart('/');
            }
            List<Dictionary<string, object>> userPropertiesList = new List<Dictionary<string, object>>();
            XmlNode node = null;
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj,mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.UserGroup, webFullUrl);
                switch (userColSource)
                {
                    case "web.users":
                        node = mNetWork.UserGroupGetUserCollectionFromWeb();
                        break;
                    case "web.allUsers":
                        node = mNetWork.UserGroupGetAllUserCollectionFromWeb();
                        break;
                    case "web.siteUsers":
                        node = mNetWork.UserGroupGetUserCollectionFromSite();
                        break;
                    case "group.users":
                        node = mNetWork.UserGroupGetUserCollectionFromGroup(groupName);
                        break;
                }
            }
            if (node != null && node.FirstChild != null && node.FirstChild.ChildNodes != null)
            {
                XmlNode usersNode = node.FirstChild;
                foreach (XmlElement user in usersNode.ChildElements())
                {
                    Dictionary<string, object> userProperties = new Dictionary<string, object>();
                    userProperties.Add("Id", Convert.ToInt32(user.GetAttribute("ID")));
                    userProperties.Add("SID", user.GetAttribute("Sid"));
                    userProperties.Add("IsSiteAdmin", Convert.ToBoolean(user.GetAttribute("IsSiteAdmin")));
                    userProperties.Add("IsDomainGroup", Convert.ToBoolean(user.GetAttribute("IsDomainGroup")));
                    userProperties.Add("LoginName", user.GetAttribute("LoginName"));
                    userProperties.Add("Name", user.GetAttribute("Name"));
                    userProperties.Add("Email", user.GetAttribute("Email"));
                    userProperties.Add("Notes", user.GetAttribute("Notes"));
                    userPropertiesList.Add(userProperties);
                }
            }
            siteUserColProperties.Add(AveObjectModelConstant.ChildrenProperties, userPropertiesList);
            return siteUserColProperties;
        }
        public Dictionary<string, object> GetGroups(string webRelativeUrl, string groupColSource, string loginName)
        {
            Dictionary<string, object> groupColProperties = new Dictionary<string, object>();
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webRelativeUrl.TrimStart('/');
            XmlNode node = null;
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.UserGroup, webFullUrl);
                switch (groupColSource)
                {
                    case "web.groups":
                        node = mNetWork.UserGroupGetGroupCollectionFromWeb();
                        break;
                    case "web.siteGroups":
                        node = mNetWork.UserGroupGetGroupCollectionFromSite();
                        break;
                    case "user.groups":
                        if (!string.IsNullOrEmpty(loginName))
                        {
                            node = mNetWork.UserGroupGetGroupCollectionFromUser(loginName);
                        }
                        break;
                }
            }
            List<Dictionary<string, object>> groupPropertiesList = new List<Dictionary<string, object>>();
            if (node != null && node.HasChildNodes)
            {
                XmlNode groupsNode = node.FirstChild;

                foreach (XmlElement group in groupsNode.ChildElements())
                {
                    Dictionary<string, object> groupProperties = new Dictionary<string, object>();
                    groupProperties.Add("Id", Convert.ToInt32(group.GetAttribute("ID")));
                    groupProperties.Add("Name", group.GetAttribute("Name"));
                    groupProperties.Add("LoginName", group.GetAttribute("Name"));
                    groupProperties.Add("Description", group.GetAttribute("Description"));
                    groupProperties.Add("OwnerId", Convert.ToInt32(group.GetAttribute("OwnerID")));
                    groupProperties.Add("OwnerType", Convert.ToBoolean(group.GetAttribute("OwnerIsUser")) ? "user" : "group");
                    groupPropertiesList.Add(groupProperties);
                }
            }
            groupColProperties.Add(AveObjectModelConstant.ChildrenProperties, groupPropertiesList);
            return groupColProperties;
        }
        //public Dictionary<string, object> GetGroup(string webServerRelativeUrl, string groupName)
        //{
        //    Dictionary<string, object> prop = new Dictionary<string, object>();
        //    string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
        //    XmlNode node = null;
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
        //    {
        //        mNetWork.InitialNetWorker(AveWebServiceType.UserGroup, webFullUrl);
        //        node = mNetWork.UserGroupGetGroupInfo(groupName);
        //        if (node.HasChildNodes)
        //        {
        //            prop.Add("Xml", node.FirstChild.OuterXml);
        //        }
        //    }
        //    return prop;
        //}
        //public Dictionary<string, object> GetFields(string webServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetAvailableFields(string webServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetAvailableContentTypes(string webServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetSiteGroups(string webServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetListTemplates(string webServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetWebApplication()
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetUserSolutions()
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetRoleAssignments(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, int itemId, string roleAssignmentsSource)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetRoleDefinitions(string webServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetEnsureUser(string webServerRelativeUrl, string loginName)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetCatalog(string webServerRelativeUrl, int typeCatalog)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetAvailableWebTemplates(string webServerRelativeUrl, uint lcid, bool doIncludeCrossLanguage)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetAlerts(string webServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}
        
        //365已重写。
        private bool GetFileComments(Dictionary<string, string> comments, string webUrl, string fileUrl)
        {
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                try
                {
                    mNetWork.InitialNetWorker(AveWebServiceType.Versions, webUrl);
                    XmlNode commentsNode = mNetWork.FileGetVersions(fileUrl);
                    XmlNodeList commentlist = commentsNode.SelectNodes(".//*[name()='result']");
                    if (commentlist != null)
                    {
                        foreach (XmlElement comment in commentlist.OfType<XmlElement>())
                        {
                            if (comment.HasAttribute("version") && comment.HasAttribute("comments"))
                            {
                                comments[comment.Attributes["version"].Value.Trim('@')] = comment.Attributes["comments"].Value;
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    mLogger.Debug("Get file: {0} comments failed.Error Message:{1}", fileUrl, ex.ToString());
                    return false;
                }
            }
        }
        //365已重写。
        public Dictionary<string, object> GetItemVersions(string webRelativeUrl, string listRelativeUrl, string listId, int itemId, string itemUrl, CultureInfo cultureInfo, Dictionary<string, string> needLoadFields)
        {
            Dictionary<string, string> KeyMapping = new Dictionary<string, string>();
            KeyMapping["_UIVersion"] = "VersionId";
            KeyMapping["_UIVersionString"] = "VersionLabel";
            //KeyMapping["ID"] = "VersionId";
            KeyMapping["_IsCurrentVersion"] = "IsCurrentVersion";
            KeyMapping["FileRef"] = "Url";
            KeyMapping["File_x0020_Size"] = "Length";
            KeyMapping["_ModerationStatus"] = "ModerationStatus";
            KeyMapping["Created_x0020_By"] = "CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix;
            KeyMapping["_Level"] = "Level";

            Dictionary<string, object> listItemVersionsProperties = new Dictionary<string, object>();
            string url = this.WebAppName + webRelativeUrl.TrimEnd('/');
            List<Dictionary<string, object>> itemVersionPropertiesList = new List<Dictionary<string, object>>();
            Dictionary<string, string> fileVersionComments = new Dictionary<string, string>();
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj,mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.Lists, url);
                int i = 0;
                foreach (KeyValuePair<string, string> kv in needLoadFields)
                {
                    if (!KeyMapping.ContainsKey(kv.Key))
                    {
                        KeyMapping[kv.Key] = kv.Key;
                    }
                    try
                    {
                        if (kv.Key.Equals("_CheckinComment", StringComparison.OrdinalIgnoreCase))
                        {
                            GetFileComments(fileVersionComments, url, itemUrl);
                            continue;
                        }
                        XmlNode node = mNetWork.ListGetVersionCollection(listId, itemId.ToString(), kv.Key);
                        //means only one version, we will get user data from current listitem object
                        if (kv.Key.Equals("Author", StringComparison.OrdinalIgnoreCase) && node.ChildNodes.Count == 1)
                        {
                            listItemVersionsProperties["HasVersion"] = false;
                            break;
                        }
                        for (int j = 0; j < node.ChildNodes.Count; j++)
                        {
                            XmlElement versionNode = node.ChildNodes[j] as XmlElement;
                            Dictionary<string, object> listItemVersionProperties = null;
                            Dictionary<string, object> itemVersionFieldValues = null;
                            if (i == 0)
                            {
                                if (j == 0)
                                {
                                    for (int k = 0; k < node.ChildNodes.Count; k++)
                                    {
                                        listItemVersionProperties = new Dictionary<string, object>();
                                        itemVersionFieldValues = new Dictionary<string, object>();
                                        listItemVersionProperties["FieldValues"] = itemVersionFieldValues;
                                        itemVersionPropertiesList.Add(listItemVersionProperties);
                                    }
                                }
                                itemVersionFieldValues = itemVersionPropertiesList[j]["FieldValues"] as Dictionary<string, object>;
                                listItemVersionProperties = itemVersionPropertiesList[j] as Dictionary<string, object>;
                                itemVersionFieldValues["Modified"] = GetValueFromType("DateTime", versionNode.GetAttribute("Modified"));
                                itemVersionFieldValues["Editor"] = GetValueFromType("User", versionNode.GetAttribute("Editor"));
                                listItemVersionProperties["Modified"] = GetValueFromType("DateTime", versionNode.GetAttribute("Modified"));
                                listItemVersionProperties["Editor"] = GetValueFromType("User", versionNode.GetAttribute("Editor"));
                            }
                            else
                            {
                                object modifiedTime = GetValueFromType("DateTime", versionNode.GetAttribute("Modified"));
                                listItemVersionProperties = itemVersionPropertiesList.FirstOrDefault((tempDic) => (kv.Key == "Modified" || kv.Key == "Editor" || !tempDic.ContainsKey(KeyMapping[kv.Key])) && tempDic["Modified"].Equals(modifiedTime));
                                itemVersionFieldValues = listItemVersionProperties["FieldValues"] as Dictionary<string, object>;
                            }
                            object value = GetValueFromType(kv.Value, versionNode.GetAttribute(kv.Key), cultureInfo);
                            if (kv.Key.Equals("_Level"))
                            {
                                value = Byte.Parse(value.ToString());
                            }
                            itemVersionFieldValues[kv.Key] = value;
                            listItemVersionProperties[KeyMapping[kv.Key]] = value;
                            if (kv.Key.Equals("_UIVersionString") && fileVersionComments.Count > 0)
                            {
                                string comments = fileVersionComments.ContainsKey(value.ToString()) ? fileVersionComments[value.ToString()] : "";
                                listItemVersionProperties["_CheckinComment"] = comments;
                                itemVersionFieldValues["_CheckinComment"] = comments;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        mLogger.Debug(AveWebServiceRequestResource.GetItemVersionsErrorWithIdInt, itemUrl, e.ToString());
                    }
                    i++;
                }
            }
            listItemVersionsProperties.Add("ChildrenProperties", itemVersionPropertiesList);
            return listItemVersionsProperties;
        }
        //365调用的client api
        public Dictionary<string, object> GetItemVersionsWithMultiRequest(string webRelativeUrl, string listId, int itemId, string itemUrl, Dictionary<string, string> needLoadFields, CultureInfo cultureInfo = null)
        {
            Dictionary<string, object> listItemVersionsProperties = new Dictionary<string, object>();
            string webAppName = GetWebAppNameFromSiteUrl(mWebUrl);
            string url = webAppName + webRelativeUrl.TrimEnd('/');
            List<Dictionary<string, object>> itemVersionPropertiesList = new List<Dictionary<string, object>>();
            Dictionary<string, XmlNode> versionFieldXmls = new Dictionary<string, XmlNode>();

            try
            {
                versionFieldXmls = RetryGetItemVersions(url, listId, itemId, itemUrl, needLoadFields, 3);
                int i = 0;
                foreach (KeyValuePair<string, XmlNode> version in versionFieldXmls)
                {
                    XmlNode node = version.Value;
                    for (int j = 0; j < node.ChildNodes.Count; j++)
                    {
                        XmlElement versionNode = node.ChildNodes[j] as XmlElement;
                        Dictionary<string, object> listItemVersionProperties = null;
                        Dictionary<string, object> itemVersionFieldValues = null;
                        while (itemVersionPropertiesList.Count <= j)
                        {
                            listItemVersionProperties = new Dictionary<string, object>();
                            itemVersionFieldValues = new Dictionary<string, object>();
                            listItemVersionProperties["FieldValues"] = itemVersionFieldValues;
                            itemVersionPropertiesList.Add(listItemVersionProperties);
                        }
                        itemVersionFieldValues = itemVersionPropertiesList[j]["FieldValues"] as Dictionary<string, object>;
                        listItemVersionProperties = itemVersionPropertiesList[j] as Dictionary<string, object>;

                        if (i == 0)
                        {
                            itemVersionFieldValues["Modified"] = GetValueFromType("DateTime", versionNode.GetAttribute("Modified"));
                            itemVersionFieldValues["Editor"] = GetValueFromType("User", versionNode.GetAttribute("Editor"));
                            listItemVersionProperties["Modified"] = GetValueFromType("DateTime", versionNode.GetAttribute("Modified"));
                            listItemVersionProperties["Editor"] = GetValueFromType("User", versionNode.GetAttribute("Editor"));
                        }

                        string key = version.Key;
                        if (!itemVersionFieldValues.ContainsKey(key))
                        {
                            string value = versionNode.GetAttribute(key);
                            string vType = needLoadFields[key];
                            object fieldValue = GetValueFromType(vType, value, cultureInfo);
                            itemVersionFieldValues[key] = fieldValue;
                            switch (key)
                            {
                                case "_UIVersion":
                                    listItemVersionProperties["VersionId"] = fieldValue;
                                    break;
                                case "_UIVersionString":
                                    listItemVersionProperties["VersionLabel"] = fieldValue;
                                    break;
                                case "ID":
                                    //listItemVersionProperties.Add("VersionId", GetValueFromType(kv.Value, versionNode.GetAttribute(kv.Key)));
                                    break;
                                case "_Level":
                                    listItemVersionProperties["Level"] = byte.Parse(value);
                                    break;
                                case "_IsCurrentVersion":
                                    listItemVersionProperties["IsCurrentVersion"] = fieldValue;
                                    break;
                                case "FileRef":
                                    listItemVersionProperties["Url"] = fieldValue;
                                    break;
                                case "File_x0020_Size":
                                    listItemVersionProperties["Length"] = fieldValue;
                                    break;
                                case "_ModerationStatus":
                                    listItemVersionProperties["ModerationStatus"] = fieldValue;
                                    break;
                                case "Created_x0020_By":
                                    listItemVersionProperties["CreatedBy" + AveObjectModelConstant.ObjectPropertySuffix] = fieldValue;
                                    break;
                                default:
                                    listItemVersionProperties[key] = fieldValue;
                                    break;
                            }
                        }
                    }
                    i++;
                }
            }
            catch (Exception ex)
            {
                mLogger.Debug("Error occurred while Get Item Versions With Multi Request.ErrorMessage:{0}.", ex.ToString());
                throw;
            }
            finally
            {
                versionFieldXmls.Clear();
            }

            listItemVersionsProperties.Add("ChildrenProperties", itemVersionPropertiesList);
            return listItemVersionsProperties;
        }

        #endregion
        public Dictionary<string, XmlNode> RetryGetItemVersions(string webUrl, string listId, int itemId, string itemUrl, Dictionary<string, string> needLoadFields, int retryCount)
        {
            Dictionary<string, XmlNode> versionFieldXmls = new Dictionary<string, XmlNode>();
            AveCountdownLatch barrieSemaphore = null;
            Exception error = null;
            if (needLoadFields.Count > 0)
            {
                barrieSemaphore = new AveCountdownLatch(needLoadFields.Count);
            }
            using (AveWebServiceNetWork netWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj,mTokenProvider))
            {
                netWork.InitialNetWorker(AveWebServiceType.Lists, webUrl);
                netWork.ListGetVersionCollectionCompletedRegister((object sender, AvePoint.ObjectModel.WebService.Lists.GetVersionCollectionCompletedEventArgs e) =>
                {
                    try
                    {
                        lock (versionFieldXmls)
                        {
                            versionFieldXmls[e.UserState.ToString()] = e.Result.Clone();
                        }
                    }
                    catch (Exception ex)
                    {
                        mLogger.Debug("There is an error when get version. ItemId: {0}, RetryCount: {1}, Error: {2}", itemId, retryCount, ex);
                        error = ex;
                    }
                    finally
                    {
                        barrieSemaphore.Release();
                    }
                });
                if (needLoadFields.Count > 0)
                {
                    foreach (KeyValuePair<string, string> kv in needLoadFields)
                    {
                        netWork.ListGetVersionCollectionAsync(listId, itemId.ToString(), kv.Key, kv.Key);
                    }
                    bool isNotTimeOut = barrieSemaphore.WaitOne(10 * 60 * 1000);
                    barrieSemaphore.Close();
                    if (error != null && ShouldRetry(error))
                    {
                        retryCount--;
                        if (retryCount > 0)
                        {
                            return RetryGetItemVersions(webUrl, listId, itemId, itemUrl, needLoadFields, retryCount);
                        }
                        else
                        {
                            throw error;
                        }
                    }
                    if (!isNotTimeOut)
                    {
                        throw new TimeoutException("Time out when getting version collection.");
                    }
                }
            }
            return versionFieldXmls;
        }
        private bool ShouldRetry(Exception e)
        {
            int retryInterval = 3000;
            if (IsConnectonForciblyClosedExceptioin(e) || IsUnstableNetworkException(e as WebException))
            {
                Thread.Sleep(retryInterval);
                return true;
            }
            return false;
        }
        private bool IsConnectonForciblyClosedExceptioin(Exception te)
        {
            if (te.InnerException is System.Net.Sockets.SocketException || te.InnerException is IOException)
            {
                return true;
            }
            else if (te.InnerException != null)
            {
                return IsConnectonForciblyClosedExceptioin(te.InnerException);
            }
            return false;
        }
        private bool IsUnstableNetworkException(WebException e)
        {
            if (e.Status == System.Net.WebExceptionStatus.NameResolutionFailure
                || e.Status == WebExceptionStatus.SecureChannelFailure
                || e.Status == WebExceptionStatus.ConnectFailure
                || e.Status == WebExceptionStatus.KeepAliveFailure
                || e.Status == WebExceptionStatus.ConnectionClosed
                || e.Status == WebExceptionStatus.PipelineFailure
                || e.Status == WebExceptionStatus.SendFailure
                || e.Status == WebExceptionStatus.UnknownError
                || e.Status == WebExceptionStatus.Pending)
            {
                return true;
            }
            if (e != null && e.Response != null)
            {
                HttpWebResponse webResponse = e.Response as HttpWebResponse;
                if (webResponse != null
                    && (webResponse.StatusCode == HttpStatusCode.ServiceUnavailable
                    || webResponse.StatusCode == HttpStatusCode.Forbidden))
                {
                    return true;
                }
            }
            return false;
        }
        //public Dictionary<string, object> GetContentTypes(string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetFields(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, string fieldSource, Dictionary<string, object> contentTypeProp)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetFields(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, string contentTypeId, string contentTypeSource)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetFeatures(string serverRelativeUrl, string featuresSource)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetEventReceiverDefinitions(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetSiteEventReceiverDefinitions(string siteServerRelativeUrl, string eventReceiverDefSource)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetRelatedFields(string webServerRelativeUrl, string listTitle, Guid listId)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetNavigationNodes(string webServerRelativeUrl, int navigationNodeId, string navigationNodeSource, Dictionary<string, object> navProperties)
        //{
        //    throw new NotImplementedException();
        //}

        public Dictionary<string, object> GetNavigationNodesProperties(string webFullUrl)
        {
            Dictionary<string, object> nodesProperties = new Dictionary<string, object>();
            string getUrl = webFullUrl.Trim('/') + "/_layouts/AreaNavigationSettings.aspx";
            string html = string.Empty;
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, mTokenProvider);
            }
            catch (Exception e)
            {
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webFullUrl, mSiteTrimObj.Name);
                string[] properties = new string[] { "NavigationNodeType" };
                foreach (string property in properties)
                {
                    if (!webTrimObj.TrimmedProperties.ContainsKey(property))
                    {
                        webTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", e.Message, getUrl);
                    }
                }
                return nodesProperties;
            }
            if (!string.IsNullOrEmpty(html))
            {
                string searchContent = "newNode = new NavigationNode(";
                AveHttpWebRequestUtility.GetNodesProperties(html, searchContent, nodesProperties);
            }
            return nodesProperties;
        }

        //public Dictionary<string, object> GetFieldLinks(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, string contentTypeId, string contentTypeSource)
        //{
        //    throw new NotImplementedException();
        //}
        public Dictionary<string, object> GetLimitedWebPartManager(string webServerRelativeUrl, string fileServerRelativeUrl, int personalizationScope, string appWebFulUrl = null)
        {
            Dictionary<string, object> webPartManagerProperties = new Dictionary<string, object>();
            Dictionary<string, object> webPartColProperties = new Dictionary<string, object>();
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj,mTokenProvider))
            {
                string currentWebUrl = string.IsNullOrEmpty(appWebFulUrl) ? AveUrlUtility.CombineUrl(this.WebAppName, webServerRelativeUrl) : appWebFulUrl;
                mNetWork.InitialNetWorker(AveWebServiceType.WebPartPages, currentWebUrl);
                string webpartPageContent = string.Empty;
                try
                {
                    string pageUrl = string.IsNullOrEmpty(appWebFulUrl) ? AveUrlUtility.CombineUrl(WebAppName, fileServerRelativeUrl) :
                                                                          appWebFulUrl.Replace(webServerRelativeUrl, fileServerRelativeUrl);
                    webpartPageContent = mNetWork.WebPagePagesGetWebPartOnPage(pageUrl);
                }
                catch (Exception e)
                {
                    if (e.Message.Equals(mHTTPUnauthorizedMessage, StringComparison.OrdinalIgnoreCase))
                    {
                        SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                        SecurityTrimObject fileTrimObj = webTrimObj.GetFile(fileServerRelativeUrl, "");
                        fileTrimObj.TrimmedProperties["LimitedWebPartManager"] = string.Format("{0} accessing resource: {1}", mHTTPUnauthorizedMessage, new Uri(new Uri(mWebUrl), fileServerRelativeUrl).ToString());
                        webPartManagerProperties.Add("WebParts" + AveObjectModelConstant.ObjectPropertySuffix, webPartColProperties);
                        return webPartManagerProperties;
                    }
                    else
                    {
                        throw;
                    }
                }
                if (string.IsNullOrEmpty(webpartPageContent))
                {
                    return webPartManagerProperties;
                }
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(webpartPageContent);
                List<Dictionary<string, object>> webpartPropertiesList = new List<Dictionary<string, object>>();
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    if (node is XmlComment)
                    {
                        continue;
                    }
                    if (node.OuterXml.IndexOf("http://schemas.microsoft.com/WebPart/v3", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        CreateWebPartPropertyV3(webpartPropertiesList, node, doc, fileServerRelativeUrl);
                    }
                    else
                    {
                        CreateWebPartPropertyV2(webpartPropertiesList, node, doc);
                    }
                }
                webPartColProperties.Add(AveObjectModelConstant.ChildrenProperties, webpartPropertiesList);
                webPartManagerProperties.Add("WebParts" + AveObjectModelConstant.ObjectPropertySuffix, webPartColProperties);
            }
            return webPartManagerProperties;
        }
        private void CreateWebPartPropertyV3(List<Dictionary<string, object>> webpartPropertiesList, XmlNode node, XmlDocument doc, string fileServerRelativeUrl)
        {
            //为了用xpath取listID，此处必须要加上namespace前缀。
            string versionNameSpace = "vList"; //temp with random value
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace(versionNameSpace, "http://schemas.microsoft.com/WebPart/v3");
            XmlElement element = node as XmlElement;
            if (element == null)
            {
                return;
            }
            Dictionary<string, object> webPartProperties = new Dictionary<string, object>();
            string definitionXml = element.OuterXml;
            XmlNode typeNode = element.SelectSingleNode(".//*[name() = 'type']");
            if (typeNode != null)
            {
                string webPartType = typeNode.Attributes["name"].Value;
            }
            webPartProperties.Add("DefinitionXml", definitionXml);
            webPartProperties.Add("ID", element.GetAttribute("ID"));
            XmlElement childNode = null;
            //利用xpaht取得listid，为还原时的postAction准备。
            try
            {
                childNode = element.SelectSingleNode(".//" + versionNameSpace + ":property[@name='ListName']", nsmgr) as XmlElement;
                if (childNode == null)
                {
                    childNode = element.SelectSingleNode(".//" + versionNameSpace + ":ListName", nsmgr) as XmlElement;
                }
                if (childNode != null && AveTypeHelper.IsGuid(childNode.InnerText))
                {
                    webPartProperties.Add("ListId", new Guid(childNode.InnerText));
                }
            }
            catch (Exception e)
            {
                mLogger.Warn("Can not get ListId in GetLimitedWebPartManager,fileServerRelativeUrl:{0},error:{1}.", fileServerRelativeUrl, e.ToString());
            }
            childNode = element.SelectSingleNode(".//*[name() = 'ZoneID']") as XmlElement;
            if (childNode != null)
            {
                webPartProperties.Add("ZoneID", childNode.InnerText);
            }
            childNode = element.SelectSingleNode(".//*[name() = 'PartOrder']") as XmlElement;
            if (childNode != null)
            {
                webPartProperties.Add("PartOrder", Convert.ToInt32(childNode.InnerText));
            }
            childNode = element.SelectSingleNode(".//*[name() = 'IsIncluded']") as XmlElement;
            if (childNode != null)
            {
                webPartProperties.Add("IsIncluded", Convert.ToBoolean(childNode.InnerText));
            }
            childNode = element.SelectSingleNode(".//*[name() = 'WebPartIdProperty']") as XmlElement;
            if (childNode != null)
            {
                webPartProperties.Add("WebPartIdProperty", childNode.InnerText);
            }
            webpartPropertiesList.Add(webPartProperties);
        }
        private void CreateWebPartPropertyV2(List<Dictionary<string, object>> webpartPropertiesList, XmlNode node, XmlDocument doc)
        {
            string versionNameSpace = "vList"; //temp with random value
            string specialNameSpace = "specialNameSpace";
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace(versionNameSpace, "http://schemas.microsoft.com/WebPart/v2");
            if (node.OuterXml.IndexOf("http://schemas.microsoft.com/WebPart/v2/ListView", StringComparison.OrdinalIgnoreCase) != -1)
            {
                nsmgr.AddNamespace(specialNameSpace, "http://schemas.microsoft.com/WebPart/v2/ListView");
            }
            else
            {
                nsmgr.AddNamespace(specialNameSpace, "http://schemas.microsoft.com/WebPart/v2/ListForm");
            }
            XmlDocument xDocV2 = new XmlDocument();
            xDocV2.LoadXml(node.OuterXml);
            Dictionary<string, object> webPartProperties = new Dictionary<string, object>();
            webPartProperties.Add("DefinitionXml", xDocV2.OuterXml);
            webPartProperties.Add("ID", xDocV2.DocumentElement.GetAttribute("ID"));
            XmlElement childNode = null;
            childNode = xDocV2.DocumentElement.SelectSingleNode(string.Format("//specialNameSpace:{0}", "ListId"), nsmgr) as XmlElement;
            if (childNode != null)
            {
                webPartProperties.Add("ListId", new Guid(childNode.InnerText));
            }
            childNode = xDocV2.DocumentElement.SelectSingleNode(string.Format("//vList:{0}", "ZoneID"), nsmgr) as XmlElement;
            if (childNode != null)
            {
                webPartProperties.Add("ZoneID", childNode.InnerText);
            }
            childNode = xDocV2.DocumentElement.SelectSingleNode(string.Format("//vList:{0}", "PartOrder"), nsmgr) as XmlElement;
            if (childNode != null)
            {
                webPartProperties.Add("PartOrder", Convert.ToInt32(childNode.InnerText));
            }
            childNode = xDocV2.DocumentElement.SelectSingleNode(string.Format("//vList:{0}", "IsIncluded"), nsmgr) as XmlElement;
            if (childNode != null)
            {
                webPartProperties.Add("IsIncluded", Convert.ToBoolean(childNode.InnerText));
            }
            //childNode = xDocV2.DocumentElement.SelectSingleNode(string.Format("//vList:{0}", "ID"), nsmgr) as XmlElement;
            childNode = xDocV2.DocumentElement.SelectSingleNode(string.Format("//vList:{0}", "WebPartIdProperty"), nsmgr) as XmlElement;
            if (childNode != null)
            {
                webPartProperties.Add("WebPartIdProperty", childNode.InnerText);
            }
            webpartPropertiesList.Add(webPartProperties);
        }
        public byte[] GetFileBinary(string webServerRelativeUrl, string fileServerRelativeUrl, int options)
        {
            #region
            /*
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            string fileFullUrl = string.Empty;
            if (fileServerRelativeUrl.StartsWith(webServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                fileFullUrl = this.WebAppName.TrimEnd('/') + "/" + fileServerRelativeUrl.Trim('/');
            }
            else
            {
                fileFullUrl = webFullUrl.TrimEnd('/') + "/" + fileServerRelativeUrl.Trim('/');
            }
            byte[] buffer = new byte[] { };
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.Copy, webFullUrl);
                buffer = mNetWork.GetFileAllBytes(fileFullUrl);
            }
            return buffer;
            */
            #endregion

            using (Stream stream = this.GetFileStream(webServerRelativeUrl, fileServerRelativeUrl, string.Empty))
            {
                byte[] buffer = new byte[stream.Length];
                int len = 0;
                int position = 0;
                int count = stream.Length > 32768 ? 32768 : (int)stream.Length;
                while ((len = stream.Read(buffer, position, count)) != 0)
                {
                    position += len;
                    if (stream.Length - position < count)
                    {
                        count = (int)stream.Length - position;
                    }
                }
                return buffer;
            }
        }
        public Stream GetFileStream(string webServerRelativeUrl, string fileServerRelativeUrl, string source)
        {
            #region old code
            //string downloadPageUrl = AveUrlUtility.CombineUrl(this.WebAppName, webServerRelativeUrl);
            //downloadPageUrl = AveUrlUtility.CombineUrl(downloadPageUrl, "_layouts/download.aspx");
            //string targetUrl = downloadPageUrl + "?SourceUrl=/" + fileServerRelativeUrl.TrimStart('/');
            //if (!fileServerRelativeUrl.StartsWith(webServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            //{
            //targetUrl = downloadPageUrl + "?SourceUrl=/" + AveUrlUtility.CombineUrl(webServerRelativeUrl, fileServerRelativeUrl).TrimStart('/');
            //}
            #endregion
            //Handle file stream both current version and minor version.Current version:fileServerRelativeUrl like ""_layouts/download.aspx";Minor Version:fileServerRelativeUrl like "/_vti_history/versionId"
            string fileFullUrl = string.Empty;
            if (fileServerRelativeUrl.StartsWith(webServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                fileFullUrl = this.WebAppName.TrimEnd('/') + "/" + fileServerRelativeUrl.Trim('/');
            }
            else if (fileServerRelativeUrl.StartsWith("_vti_history/", StringComparison.OrdinalIgnoreCase))
            {
                fileFullUrl = this.WebAppName.TrimEnd('/') + webServerRelativeUrl.TrimEnd('/') + "/" + fileServerRelativeUrl.TrimStart('/');
            }
            else
            {
                fileFullUrl = this.WebAppName.TrimEnd('/') + webServerRelativeUrl.TrimEnd('/') + HttpUtility.UrlEncode(fileServerRelativeUrl);
            }

            AveCoordinatedStream memoryStream = new AveCoordinatedStream();
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj,mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.HttpWebRequest, fileFullUrl);
                Stream netStream = null;
                try
                {
                    netStream = mNetWork.GetVersionDataStream();
                }
                catch (Exception e)
                {
                    if (e.Message.Equals(mServerUnauthorizedMessage, StringComparison.OrdinalIgnoreCase))
                    {
                        SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl.Substring(0, webServerRelativeUrl.IndexOf("_layouts/download.aspx?SourceUrl=", StringComparison.OrdinalIgnoreCase) - 1), mSiteTrimObj.Name);
                        SecurityTrimObject fileTrimObj = webTrimObj.GetFile(fileServerRelativeUrl, "");
                        fileTrimObj.TrimmedProperties["FileStream"] = string.Format("{0} accessing resource: {1}", mHTTPUnauthorizedMessage, new Uri(new Uri(mWebUrl), fileServerRelativeUrl).ToString());
                        return memoryStream;
                    }
                    else
                    {
                        throw;
                    }
                }
                this.CopyStream(netStream, memoryStream, 32768, true);
                netStream.Dispose();
            }
            return memoryStream;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "_aut:A lesson of of sharepoint local path.")]
        public Stream GetFileStream(string webServerRelativeUrl, string fileServerRelativeUrl, int versionId)
        {
            string url = this.WebAppName.TrimEnd('/') + webServerRelativeUrl.TrimEnd('/') + "/_vti_bin/_vti_aut/author.dll";
            if (fileServerRelativeUrl.StartsWith(webServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                fileServerRelativeUrl = fileServerRelativeUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length).TrimStart('/');
            }
            string dirName = fileServerRelativeUrl.Substring(0, fileServerRelativeUrl.LastIndexOf('/'));
            string contentType = "application/x-www-form-urlencoded";
            string id = string.Empty;
            if (versionId % 512 == 0)
            {
                id = (versionId / 512).ToString();
            }
            else
            {
                id = Math.Floor((double)versionId / 512).ToString() + "." + (versionId % 512).ToString();
            }
            string postContent = "method=get+document:" //+ mServerVersion
                + "&service_name=/" + "&dir_name=" + dirName
                + "&document_name=" + fileServerRelativeUrl
                + "&force=true&get_option=none"
                + "&doc_version=V" + id + "&timeout=0";
            byte[] body = UTF8Encoding.UTF8.GetBytes(postContent);
            Dictionary<string, object> headerInformation = new Dictionary<string, object>();
            headerInformation.Add("X-Vermeer-Content-Type", "application/x-www-form-urlencoded");
            string result = AveHttpWebRequestUtility.HttpReturn(url, mObj, contentType, body, headerInformation,string.Empty,mTokenProvider);
            int index = result.IndexOf("<html>", StringComparison.OrdinalIgnoreCase);
            int endIndex = result.IndexOf("</html>", StringComparison.OrdinalIgnoreCase) + 7;
            string streamContent = result.Substring(endIndex + 1);
            byte[] array = Encoding.UTF8.GetBytes(streamContent);
            MemoryStream stream = new MemoryStream(array);
            return stream;
        }
        private void CopyStream(Stream src, Stream dest, int size, bool resetPoistion)
        {
            byte[] buffer = new byte[size];
            int len = 0;
            while ((len = src.Read(buffer, 0, size)) != 0)
            {
                dest.Write(buffer, 0, len);
            }
            if (resetPoistion)
            {
                dest.Position = 0;
            }
        }
        public Dictionary<string, object> GetUserProfileByName(string accountName, bool isOnlineSite)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            try
            {
                string realSiteUrl = mWebUrl;
                //TODO:APPTOKEN
                object cookieContainer =  mObj;
                if (cookieContainer == null && mTokenProvider == null)
                {
                    return new Dictionary<string, object>();
                }
                using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, realSiteUrl, cookieContainer,mTokenProvider))
                {
                    mNetWork.InitialNetWorker(AveWebServiceType.UserProfile, realSiteUrl);
                    UserProfileService.PropertyData[] datas = mNetWork.UserProfileGetUserProfile(accountName);
                    returnInfo["ProfileValues"] = GetUserProfilePropertyValues(datas);//"DefaultProfileSubtypeProperties"+AveObjectModelConstant.ObjectPropertySuffix
                    UserProfileService.ContactData[] colleagueDatas = mNetWork.UserProfileGetUserColleagues(accountName);
                    returnInfo["Colleagues" + AveObjectModelConstant.ObjectPropertySuffix] = GetUserProfileColleagues(colleagueDatas);
                    UserProfileService.MembershipData[] membershipsDatas = mNetWork.UserProfileGetUserMemberShips(accountName);
                    returnInfo["Memberships" + AveObjectModelConstant.ObjectPropertySuffix] = GetUserProfileMemberships(membershipsDatas);
                    UserProfileService.QuickLinkData[] linksDatas = mNetWork.UserProfileGetUserLinks(accountName);
                    returnInfo["QuickLinks" + AveObjectModelConstant.ObjectPropertySuffix] = GetUserProfileLinks(linksDatas);
                }
            }
            catch (Exception e)
            {
                mLogger.Debug("Get UserProfile of {0} failed, Error message: {1}", accountName, e.ToString());
            }
            return returnInfo;
        }

        protected List<Dictionary<string, object>> GetUserProfilePropertyValues(UserProfileService.PropertyData[] datas)
        {
            List<Dictionary<string, object>> valueList = new List<Dictionary<string, object>>();
            foreach (UserProfileService.PropertyData data in datas)
            {
                Dictionary<string, object> valueInfo = new Dictionary<string, object>();
                valueInfo["NameValue"] = data.Name;
                string privacy = data.Privacy.ToString();
                valueInfo["Privacy"] = Enum.Parse(typeof(AvePrivacy), privacy);//1.2.4.8.16.1073741824

                List<object> values = new List<object>();
                foreach (UserProfileService.ValueData value in data.Values)
                {
                    if (!(value.Value is DateTime
                        && (DateTime)value.Value == DateTime.MinValue))
                    {
                        if (data.Name.Equals("SPS-TimeZone", StringComparison.OrdinalIgnoreCase))
                        {
                            values.Add((value.Value as UserProfileService.SPTimeZone).ID);
                            continue;
                        }
                        values.Add(value.Value);
                    }
                }
                valueInfo["Value"] = values;
                valueList.Add(valueInfo);
            }
            return valueList;
        }

        protected Dictionary<string, object> GetUserProfileColleagues(UserProfileService.ContactData[] datas)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            List<Dictionary<string, object>> colleaguesList = new List<Dictionary<string, object>>();
            foreach (UserProfileService.ContactData data in datas)
            {
                Dictionary<string, object> colleagueInfo = new Dictionary<string, object>();
                colleagueInfo["Title"] = data.Title == null ? "" : data.Title;
                colleagueInfo["AccountName"] = data.AccountName;
                string privacy = data.Privacy.ToString();
                colleagueInfo["PrivacyLevel"] = Enum.Parse(typeof(AvePrivacy), privacy);//1.2.4.8.16.1073741824
                colleagueInfo["Group"] = data.Group;
                colleagueInfo["IsInWorkGroup"] = data.IsInWorkGroup;
                colleagueInfo["Url"] = data.Url;
                colleaguesList.Add(colleagueInfo);
            }
            returnInfo.Add(AveObjectModelConstant.ChildrenProperties, colleaguesList);
            return returnInfo;
        }

        protected Dictionary<string, object> GetUserProfileMemberships(UserProfileService.MembershipData[] datas)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            List<Dictionary<string, object>> membershipList = new List<Dictionary<string, object>>();
            foreach (UserProfileService.MembershipData data in datas)
            {
                Dictionary<string, object> membershipInfo = new Dictionary<string, object>();
                Dictionary<string, object> memberGroup = new Dictionary<string, object>();
                membershipInfo["Title"] = data.DisplayName;
                membershipInfo["Url"] = data.Url;
                membershipInfo["Group"] = data.Group;
                string privacy = data.Privacy.ToString();
                membershipInfo["PrivacyLevel"] = Enum.Parse(typeof(AvePrivacy), privacy);//1.2.4.8.16.1073741824
                memberGroup["Source"] = data.Source;
                memberGroup["SourceInternal"] = data.MemberGroup.SourceInternal;
                memberGroup["SourceReference"] = data.MemberGroup.SourceReference;
                memberGroup["DisplayName"] = data.DisplayName;
                memberGroup["MailNickName"] = data.MailNickname;
                membershipInfo["MembershipGroup" + AveObjectModelConstant.ObjectPropertySuffix] = memberGroup;
                membershipList.Add(membershipInfo);
            }
            returnInfo.Add(AveObjectModelConstant.ChildrenProperties, membershipList);
            return returnInfo;
        }
        protected Dictionary<string, object> GetUserProfileLinks(UserProfileService.QuickLinkData[] datas)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            List<Dictionary<string, object>> linkList = new List<Dictionary<string, object>>();
            foreach (UserProfileService.QuickLinkData data in datas)
            {
                Dictionary<string, object> linkInfo = new Dictionary<string, object>();
                linkInfo["Title"] = data.Name;
                linkInfo["Url"] = data.Url;
                linkInfo["Group"] = data.Group;
                linkList.Add(linkInfo);
            }
            returnInfo.Add(AveObjectModelConstant.ChildrenProperties, linkList);
            return returnInfo;
        }
        public Stream GetFileVersionStream(string webServerRelativeUrl, string fileServerRelativeUrl, string fileVerionServerRelativeUrl, int versionId)
        {
            List<string> fileExtensions = new List<string>() { ".aspx", ".master", ".xoml", ".rules" };
            string currentFileExtension = Path.GetExtension(fileServerRelativeUrl);
            if (fileExtensions.Contains(currentFileExtension.ToLowerInvariant()))
            {
                fileServerRelativeUrl = AveHttpUtility.UrlPathEncode(fileServerRelativeUrl, false);
                return GetFileStream(webServerRelativeUrl, fileServerRelativeUrl, versionId);
            }
            return GetFileStream(webServerRelativeUrl, fileVerionServerRelativeUrl, null);
        }
        //public Dictionary<string, object> GetUserProfileManager()
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> GetAudienceManager()
        //{
        //    throw new NotImplementedException();
        //}

        //public Guid GetListId(Guid webId, string listTitle)
        //{
        //    return Guid.Empty;
        //}

        //public IList<Dictionary<string, object>> GetManagedThemes()
        //{
        //    return null;
        //}

        //public Dictionary<string, object> GetPublishingWeb(string webServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}

        //public string GetApplicationPath(string serverRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> ResolvePrincipal(string webServerRelativeUrl, string input, int scopes, int sources, bool inputIsEmailOnly, bool ignoreDomainDiff)
        //{
        //    throw new NotImplementedException();
        //}
        
        //13及以上已经使用client api。
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "aclinv is a part of url")]
        public static bool CheckInvalidUser(string webapp, string webServerRelativeUrl, string input, object obj)
        {
            string postUrl = webapp + webServerRelativeUrl.TrimEnd('/') + "/_layouts/aclinv.aspx?IsDlg=1";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, obj);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = System.Web.HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = System.Web.HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["ctl00%24PlaceHolderDialogBodySection%24ctl04%24OriginalEntities"] = "<Entities />";
            bodyDic["__CALLBACKID"] = "ctl00$PlaceHolderMain$ctl00$ctl01$userPicker";
            bodyDic["__CALLBACKPARAM"] = input;
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            string httpReturn = AveHttpWebRequestUtility.HttpReturn(postUrl, obj, "application/x-www-form-urlencoded", body, null);
            string str = httpReturn.Substring(2);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(str);
            if (!string.IsNullOrEmpty(doc.FirstChild.Attributes["Error"].Value))
            {
                return false;
            }
            return true;
        }

        private Dictionary<string, object> GetDtDic(Dictionary<string, object> jsonObj)
        {
            var ch = jsonObj["Ch"] as ArrayList;
            //ADO-161753 10 ADFS user 数据结构与AD user数据结构不同，ADFS user 信息存在第二层
            if (ch != null && ch.Count > 0)
            {
                if (ch[0] is Dictionary<string, object>)
                {
                    return ((Dictionary<string, object>)ch[0])["Dt"] as Dictionary<string, object>;
                }
            }
            return jsonObj["Dt"] as Dictionary<string, object>;

        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Dneutral is a part of xml")]
        public Dictionary<string, object> SearchPrincipals(string webServerRelativeUrl, string input, int scopes, int sources, int maxCount)
        {
            Dictionary<string, object> principalInfos = new Dictionary<string, object>();
            string postUrl = Url + "/_layouts/Picker.aspx?MultiSelect=True&CustomProperty=User%2CSecGroup%2CSPGroup%3B%3B15%3B%3B%3BFalse&DialogTitle=Select%20People%20and%20Groups&DialogImage=%2F%5Flayouts%2Fimages%2Fppeople%2Egif&PickerDialogType=Microsoft%2ESharePoint%2EWebControls%2EPeoplePickerDialog%2C%20Microsoft%2ESharePoint%2C%20Version%3D14%2E0%2E0%2E0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3D71e9bce111e9429c&ForceClaims=False&DisableClaims=False&EnabledClaimProviders=&EntitySeparator=%3B%EF%BC%9B%EF%B9%94%EF%B8%94%E2%8D%AE%E2%81%8F%E1%8D%A4%D8%9B&DefaultSearch=";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = System.Web.HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = System.Web.HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["ctl00%24PlaceHolderDialogBodySection%24ctl04%24OriginalEntities"] = "%3CEntities%20%2F%3E";
            bodyDic["__CALLBACKID"] = "ctl00%24PlaceHolderDialogBodySection%24ctl06";
            bodyDic["__CALLBACKPARAM"] = "%3B%23%3B%23" + System.Web.HttpUtility.UrlEncode(input) + "%3B%23%3B%23%3B%23";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            string httpReturn = AveHttpWebRequestUtility.HttpReturn(postUrl, mObj, "application/x-www-form-urlencoded", body, null, string.Empty, mTokenProvider);
            List<Dictionary<string, object>> infoList = new List<Dictionary<string, object>>();
            if (httpReturn.IndexOf(";#;#[{", StringComparison.OrdinalIgnoreCase) != -1)
            {
                string jsonData = httpReturn.Substring(httpReturn.IndexOf(";#;#[{", StringComparison.OrdinalIgnoreCase) + 4).TrimEnd('#').TrimEnd(';');
                JavaScriptSerializer jsParser = new JavaScriptSerializer();
                List<Dictionary<string, object>> jsonObj = jsParser.Deserialize<List<Dictionary<string, object>>>(jsonData);
                if (jsonObj != null)
                {
                    foreach (Dictionary<string, object> obj in jsonObj)
                    {

                        Dictionary<string, object> dt = GetDtDic(obj);
                        ArrayList table = dt["ResultTable"] as ArrayList;
                        for (int i = 0; i < table.Count; i++)
                        {
                            Dictionary<string, object> infoDic = new Dictionary<string, object>();
                            Dictionary<string, object> kvpairs = new Dictionary<string, object>();
                            ArrayList principalObj = table[i] as ArrayList;
                            if (principalObj.Count == 0)
                            {
                                continue;
                            }
                            for (int j = 0; j < principalObj.Count; j++)
                            {
                                Dictionary<string, object> kvpair = principalObj[j] as Dictionary<string, object>;
                                kvpairs.Add(kvpair["Key"].ToString(), kvpair["Value"]);
                            }
                            if (kvpairs.ContainsKey("Ek"))
                            {
                                infoDic["LoginName"] = ProcessSpecificCharacter(kvpairs["Ek"]);
                            }
                            if (kvpairs.ContainsKey("DspT"))
                            {
                                infoDic["DisplayName"] = ProcessSpecificCharacter(kvpairs["DspT"]);
                            }
                            if (kvpairs.ContainsKey("PrincipalType"))//(kvpairs.ContainsKey("Rt") && kvpairs["Rt"].ToString().Trim().Length > 1)
                            {
                                string type = kvpairs["PrincipalType"].ToString();
                                if (type.Contains("SharePointGroup"))
                                {
                                    infoDic.Add("PrincipalType", AvePrincipalType.SharePointGroup);
                                }
                                else if (type.Contains("User"))
                                {
                                    infoDic.Add("PrincipalType", AvePrincipalType.User);
                                }
                                else
                                {
                                    infoDic.Add("PrincipalType", AvePrincipalType.SecurityGroup);
                                }
                                infoDic["PrincipalId"] = int.MinValue;
                            }
                            else
                            {
                                if (kvpairs.ContainsKey("Rt") && kvpairs["Rt"].ToString().Trim().Length > 1)
                                {
                                    if (kvpairs["Rt"].ToString().Contains("SharePoint Group"))//)|| kvpairs["Rt"].ToString().Contains("SharePoint グループ"))//ADO-61604 日语环境
                                    {
                                        infoDic.Add("PrincipalType", AvePrincipalType.SharePointGroup);
                                    }
                                    else if (kvpairs["Rt"].ToString().Contains("User") || kvpairs["Rt"].ToString().Contains("ユーザー: テナント"))//ADO-61604 日语环境
                                    {
                                        infoDic.Add("PrincipalType", AvePrincipalType.User);
                                    }
                                    else
                                    {
                                        infoDic.Add("PrincipalType", AvePrincipalType.SecurityGroup);
                                    }
                                }
                            }
                            infoDic.Add("Email", "");
                            if (kvpairs.ContainsKey("Email"))
                            {
                                infoDic["Email"] = ProcessSpecificCharacter(kvpairs["Email"]);
                            }
                            if (kvpairs.ContainsKey("SPGroupID") || kvpairs.ContainsKey("SPUserID"))
                            {
                                //infoDic.Add("PrincipalID", kvpairs.ContainsKey("SPGroupID") ? Convert.ToInt32(kvpairs["SPGroupID"]) : Convert.ToInt32(kvpairs["SPUserID"]));
                                infoDic["PrincipalId"] = kvpairs.ContainsKey("SPGroupID") ? Convert.ToInt32(kvpairs["SPGroupID"]) : Convert.ToInt32(kvpairs["SPUserID"]);
                            }
                            if (kvpairs.ContainsKey("MobilePhone"))
                            {
                                infoDic.Add("Mobile", kvpairs["MobilePhone"]);
                            }
                            if (kvpairs.ContainsKey("Department"))
                            {
                                infoDic.Add("Department", kvpairs["Department"]);
                            }
                            if (kvpairs.ContainsKey("Title"))
                            {
                                infoDic.Add("Title", kvpairs["Title"]);
                            }
                            infoList.Add(infoDic);
                        }
                    }
                }
            }
            principalInfos.Add("Principals", infoList);
            return principalInfos;
        }
        private string ProcessSpecificCharacter(object oldStr)
        {
            if (oldStr != null)
            {
                return oldStr.ToString().Replace("&#39;;", "'");
            }
            return null;
        }

        //public Dictionary<string, object> GetTaxonomySession()
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetTermStores()
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetTaxonomyGroups(Guid guid)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetTermSets(Guid termStoreId, Guid groupId)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetTermSetsInTermStores(string termSetName, int LCID)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetTerms(Guid termStoreId, Guid groupId, Guid termSetId, Guid parentTermId)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetLables(Guid termStoreId, Guid termSetId, Guid parentTermId)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetSiteCollectionGroup(Guid termStoreId, string siteUrl, bool createIfMissing)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetTermSet(Guid termStoreId, Guid termSetId)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetTerm(Guid termStoreId, Guid termId)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetTerm(Guid termStoreId, Guid termSetId, Guid termId)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetTerms(Guid termStoreId, Guid termSetId, string termLabel, bool trimUnavailable)
        //{
        //    throw new NotImplementedException();
        //}
        //public string GetDefaultLabel(Guid termStoreId, Guid termId, int defaultID)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> GetListAssociastedProperty(string webServerRelativeUrl, string listTitle)
        //{
        //    Dictionary<string, object> listAssociatedProperties = new Dictionary<string, object>();
        //    using (mNetWork)
        //    {
        //        string mNetUrl = WebAppName + webServerRelativeUrl;
        //        mNetWork.InitialNetWorker(AveWebServiceType.Lists, mNetUrl);
        //        XmlNode listNode = mNetWork.ListGetList(listTitle);
        //        XmlNodeToDicValue(listAssociatedProperties, listNode);
        //    }
        //    return listAssociatedProperties;
        //}
        public Dictionary<string, object> GetSitePortal(string siteUrl)
        {
            Dictionary<string, object> sitePortal = new Dictionary<string, object>();
            string getUrl = siteUrl.TrimEnd('/') + "/_layouts/portal.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, mTokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            sitePortal.Add("PortalUrl", AveHttpWebRequestUtility.GetComponentValue(html, "ctl00$PlaceHolderMain$ctl00$ctl01$TxtPortalURL"));
            sitePortal.Add("PortalName", AveHttpWebRequestUtility.GetComponentValue(html, "ctl00$PlaceHolderMain$ctl00$ctl02$TxtPortalName"));
            return sitePortal;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "cbxl is a part of xml")]
        public List<string> GetSiteEnabledHelpCollections()
        {
            List<string> helpCollection = new List<string>();
            string getUrl = mWebUrl.TrimEnd('/') + "/_layouts/HelpSettings.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, mTokenProvider);
            string searchContent = "<span id=\"ctl00_PlaceHolderMain_ctl00_ctl00_cbxlAvailableHelpCollections\"";
            GetSiteEnabledHelpCollections(html, searchContent, "</span>", helpCollection);
            return helpCollection;
        }
        private void GetSiteEnabledHelpCollections(string html, string searchContent, string endContent, List<string> helpCollection)
        {
            string information = AveHttpWebRequestUtility.GetInput(html, searchContent, endContent);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(information);
            XmlNodeList inPutNodeList = xmlDoc.SelectNodes("/span/input");
            XmlNodeList labelNodeList = xmlDoc.SelectNodes("/span/label");
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Rad is a part of xml")]
        public bool GetListRated(string webServerRelativeUrl, Guid listId)
        {
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/RatingsSettings.aspx?List=" + listId.ToString("B");
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj,mTokenProvider);
            string searchContent = "ctl00_PlaceHolderMain_ctl00_ctl02_RadEnableRatingsNo";
            bool rating = AveHttpWebRequestUtility.GetCheckInput(html, searchContent);
            return !rating;
        }
        public Dictionary<string, object> GetMetadataNavigationSettings(string webServerRelativeUrl, Guid listId, string listTitle)
        {
            Dictionary<string, object> metadataNavigationSettings = new Dictionary<string, object>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/MetaNavSettings.aspx?List=" + listId.ToString("B");
            string html = string.Empty;
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, mTokenProvider);
            }
            catch (Exception e)
            {
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                SecurityTrimObject listTrimObj = webTrimObj.GetList(listId, listTitle);
                string[] properties = new string[] { "MetadataNavigationSettings", };
                foreach (string property in properties)
                {
                    if (!listTrimObj.TrimmedProperties.ContainsKey(property))
                    {
                        listTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", e.Message, getUrl);
                    }
                }
                return metadataNavigationSettings;
            }
            if (string.IsNullOrEmpty(html))
            {
                return metadataNavigationSettings;
            }
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ms-standardheader is a part of xml")]
        public List<Dictionary<string, object>> GetListCheckOutFiles(string webServerRelativeUrl, Guid listId)
        {
            List<Dictionary<string, object>> checkOutFileProperties = new List<Dictionary<string, object>>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/ManageCheckedOutFiles.aspx?List=" + listId + "";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj, mTokenProvider);
            //string searchContent = "class=\"ms-standardheader\"><b>Files checked out to others:</b></h3></td></tr>";
            string searchPattern = "//table[@width='100%'][@cellpadding='0'][@cellspacing='0'][@border='0'][@id='onetidTable']";
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode(searchPattern);
            HtmlNodeCollection nodeCollection = node.SelectNodes("./tr");
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "metadatacolsettings is a part of xml")]
        public Dictionary<string, object> GetMetadataListFieldSettings(string webServerRelativeUrl, string listTitle, Guid listId)
        {
            Dictionary<string, object> metadataListFieldSettingsProp = new Dictionary<string, object>();
            AveHttpValueCollection values = new AveHttpValueCollection();
            values["List"] = listId.ToString("B");
            values["Source"] = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/listedit.aspx?List=" + listId;
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/metadatacolsettings.aspx?" + values.ToString(true);
            //List={" + listId + "}&Source="+ WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/listedit.aspx?List=" + listId;
            string html = string.Empty;
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj,mTokenProvider);
            }
            catch (Exception e)
            {
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                SecurityTrimObject listTrimObj = webTrimObj.GetList(listId, listTitle);
                string[] properties = new string[] { "Enterprise Metadata and Keywords Settings" };
                foreach (string property in properties)
                {
                    if (!listTrimObj.TrimmedProperties.ContainsKey(property))
                    {
                        listTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", e.Message, getUrl);
                    }
                }
                return metadataListFieldSettingsProp;
            }
            if (html.IndexOf("Apply Enterprise Keywords to all content types on this list", StringComparison.OrdinalIgnoreCase) > 0)
            {
                metadataListFieldSettingsProp["ListHasKeywordsField"] = true;
            }
            string searchContent = "<input id=\"ctl00_PlaceHolderMain_KeywordsSection_ctl00_CheckBoxEnterpriseKeywords\"";
            string information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, "/>");
            if (information.Contains("checked=\"checked\""))
            {
                metadataListFieldSettingsProp["EnableKeywordsField"] = true;
                metadataListFieldSettingsProp["KeywordsFieldExistsInContentTypes"] = true;
            }
            searchContent = "<input id=\"ctl00_PlaceHolderMain_MDPushSection_ctl00_CheckBoxPromoteMetadata\"";
            information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, "/>");
            if (information.Contains("checked=\"checked\""))
            {
                metadataListFieldSettingsProp["EnableMetadataPromotion"] = true;
            }
            return metadataListFieldSettingsProp;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "LstSetng is a part of url")]
        public Dictionary<string, object> GetListVersionLimited(string webServerRelativeUrl, Guid listId)
        {
            Dictionary<string, object> versionLimitedProp = new Dictionary<string, object>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/LstSetng.aspx?List=" + listId;
            string html = string.Empty;
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj,mTokenProvider);
            }
            catch (Exception e)
            {
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                SecurityTrimObject listTrimObj = webTrimObj.GetList(listId, string.Empty);
                string[] properties = new string[] { "MajorVersionLimit", "MajorWithMinorVersionsLimit" };
                foreach (string property in properties)
                {
                    if (!listTrimObj.TrimmedProperties.ContainsKey(property))
                    {
                        listTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", e.Message, getUrl);
                    }
                }
                return versionLimitedProp;
            }
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
        public Dictionary<string, object> GetPerLocationViewSettings(string webServerRelativeUrl, Guid listId)
        {
            Dictionary<string, object> perLocationProp = new Dictionary<string, object>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/MetaNavPerNode.aspx?List=" + listId.ToString("B");
            string html = string.Empty;
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj,mTokenProvider);
            }
            catch (Exception e)
            {
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                SecurityTrimObject listTrimObj = webTrimObj.GetList(listId, string.Empty);
                string[] properties = new string[] { "PerLocationViewSettings" };
                foreach (string property in properties)
                {
                    if (!listTrimObj.TrimmedProperties.ContainsKey(property))
                    {
                        listTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", e.Message, getUrl);
                    }
                }
                return perLocationProp;
            }
            Dictionary<string, List<string[]>> FieldsProp = new Dictionary<string, List<string[]>>();
            XmlDocument xmlDoc = new XmlDocument();
            string searchContent = "<input id=\"ctl00_PlaceHolderMain_ctl01_Picker_data\"";
            string information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</input>");
            //当list为Disussion类型的时候，GetPerLocationViewSetting不支持，直接返回即可。
            if (string.IsNullOrEmpty(information))
            {
                return perLocationProp;
            }
            xmlDoc.LoadXml(information);
            SetAvailableFields(FieldsProp, xmlDoc.FirstChild.Attributes["value"].Value, "AvailableHierarchyFields");

            searchContent = "<input id=\"ctl00_PlaceHolderMain_ctl01_Picker_initial\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</input>");
            xmlDoc.LoadXml(information);
            SetSelectedFields(FieldsProp, xmlDoc.FirstChild.Attributes["value"].Value, "SelectedHierarchyFields");
            perLocationProp.Add("PerLocationViewSettings", FieldsProp);

            searchContent = "<input id=\"ctl00_PlaceHolderMain_ctl00_RadioInheritNo\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            if (information.Contains("disabled=\"disabled\""))
            {
                perLocationProp["IsInheritEnable"] = false;
            }
            else
            {
                perLocationProp["IsInheritEnable"] = true;
            }
            if (information.Contains("checked=\"checked\""))
            {
                perLocationProp["IsInherit"] = false;
            }
            else
            {
                perLocationProp["IsInherit"] = true;
            }
            return perLocationProp;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "listsyndication is a part of url")]
        public Dictionary<string, object> GetListRssProperties(string webServerRelativeUrl, Guid listId)
        {
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/listsyndication.aspx?List=" + listId.ToString("B");
            Dictionary<string, object> rssProperties = new Dictionary<string, object>();
            string html = string.Empty;//AveHttpWebRequestUtility.HttpGet(getUrl, mObj, listTrimObj, "ListRssProperties");
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj,mTokenProvider);
            }
            catch (Exception e)
            {
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                SecurityTrimObject listTrimObj = webTrimObj.GetList(listId, string.Empty);
                string[] properties = new string[] { "ListRssSetting" };
                foreach (string property in properties)
                {
                    if (!listTrimObj.TrimmedProperties.ContainsKey(property))
                    {
                        listTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", e.Message, getUrl);
                    }
                }
                return rssProperties;
            }
            if (!string.IsNullOrEmpty(html))
            {
                string searchContent = "<input id=\"ctl00_PlaceHolderMain_EnableRssSection_ctl00_EnabledTrue\"";
                bool allowListRss = AveHttpWebRequestUtility.GetCheckInput(html, searchContent);
                rssProperties["AllowRssFeeds"] = allowListRss;
                rssProperties["EnableSyndication"] = allowListRss;
                Hashtable folderProp = new Hashtable();
                XmlDocument xmlDoc = new XmlDocument();
                searchContent = "<input id=\"ctl00_PlaceHolderMain_Rss20ChannelInformationSection_ctl00_LimDescTrue\"";
                bool limitDescription = AveHttpWebRequestUtility.GetCheckInput(html, searchContent);
                if (limitDescription)
                {
                    folderProp["vti_rss_LimitDescriptionLength"] = 1;
                }
                searchContent = "<input name=\"ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl01$TxtChannelTitle\"";
                string information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
                if (!string.IsNullOrEmpty(information))
                {
                    xmlDoc.LoadXml(information);
                    folderProp["vti_rss_ChannelTitle"] = xmlDoc.FirstChild.Attributes["value"].Value;
                }
                searchContent = "<textarea name=\"ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl02$TxtChannelDescription\"";
                information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</textarea>");
                if (!string.IsNullOrEmpty(information))
                {
                    xmlDoc.LoadXml(information);
                    folderProp["vti_rss_ChannelDescription"] = xmlDoc.FirstChild.InnerText;
                }
                searchContent = "<input name=\"ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl03$TxtChannelImageUrl\"";
                information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
                if (!string.IsNullOrEmpty(information))
                {
                    xmlDoc.LoadXml(information);
                    folderProp["vti_rss_ChannelImageUrl"] = xmlDoc.FirstChild.Attributes["value"] != null ? xmlDoc.FirstChild.Attributes["value"].Value : String.Empty;
                }
                searchContent = "<input name=\"ctl00$PlaceHolderMain$ItemLimitSection$ctl00$TxtItemLimit\"";
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
                searchContent = "<input name=\"ctl00$PlaceHolderMain$ItemLimitSection$ctl01$TxtDayLimit\"";
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
                searchContent = "<input id=\"ctl00_PlaceHolderMain_EnclosuresSection_ctl00_FileEnclosureTrue\"";
                bool fileEnclosure = AveHttpWebRequestUtility.GetCheckInput(html, searchContent);
                if (fileEnclosure)
                {
                    folderProp["vti_rss_DocumentAsEnclosure"] = 1;
                }
                searchContent = "<input id=\"ctl00_PlaceHolderMain_EnclosuresSection_ctl01_FileLinkTrue\"";
                bool fileLink = AveHttpWebRequestUtility.GetCheckInput(html, searchContent);
                if (fileLink)
                {
                    folderProp["vti_rss_DocumentAsLink"] = 1;
                }
                rssProperties["RootFolderRssProperties"] = folderProp;

            }
            return rssProperties;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "contenttypesyndicationhubs:A part of url.")]
        public List<Dictionary<string, object>> GetPublishedContentTypes()
        {
            List<Dictionary<string, object>> metadataSevices = new List<Dictionary<string, object>>();
            string getUrl = mWebUrl.TrimEnd('/') + "/_Layouts/contenttypesyndicationhubs.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj,mTokenProvider);
            string searchContent = "<tr id=\"ctl00_PlaceHolderMain_ctl02_ctl00_tablerow3\">";
            string information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, "<tr id=\"ctl00_PlaceHolderMain_ctl02_ctl00_tablerow5\">");
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
        public Dictionary<string, object> GetListGeneralProperties(string webServerRelativeUrl, Guid listId)
        {
            Dictionary<string, object> generalProperties = new Dictionary<string, object>();
            string url = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/ListGeneralSettings.aspx?List=" + listId.ToString("B");
            string html = string.Empty;
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(url, mObj,mTokenProvider);
            }
            catch (Exception e)
            {
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                SecurityTrimObject listTrimObj = webTrimObj.GetList(listId, string.Empty);
                string[] properties = new string[] { "GeneralListSettings" };
                foreach (string property in properties)
                {
                    if (!listTrimObj.TrimmedProperties.ContainsKey(property))
                    {
                        listTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", e.Message, url);
                    }
                }
                return generalProperties;
            }
            if (string.IsNullOrEmpty(html))
            {
                return generalProperties;
            }
            //Get Survey Options
            string search = "<input id=\"ctl00_PlaceHolderMain_SurveySection_ctl00_RadShowUserYes\"";
            string content = AveHttpWebRequestUtility.GetInput(html, search, "/>");
            if (!string.IsNullOrEmpty(content))
            {
                generalProperties["ShowUser"] = content.Contains("checked=\"checked\"");
            }
            search = "<input id=\"ctl00_PlaceHolderMain_SurveySection_ctl01_RadAllowMultiResponseYes\"";
            content = AveHttpWebRequestUtility.GetInput(html, search, "/>");
            if (!string.IsNullOrEmpty(content))
            {
                generalProperties["AllowMultiResponses"] = content.Contains("checked=\"checked\"");
            }
            //Get Calendar Options
            search = "<input id=\"ctl00_PlaceHolderMain_EventSection_ctl00_RadEnablePeopleSelectorYes\"";
            content = AveHttpWebRequestUtility.GetInput(html, search, "/>");
            if (!string.IsNullOrEmpty(content))
            {
                generalProperties["EnablePeopleSelector"] = content.Contains("checked=\"checked\"");
            }
            return generalProperties;
        }
        public Dictionary<string, object> GetListEditViewSettingProperties(String webServerRelativeUrl, String listTitle, Guid listId, Guid viewId)
        {
            Dictionary<string, object> editViewProperties = new Dictionary<string, object>();
            StringBuilder sb = new StringBuilder();
            sb.Append(WebAppName);
            sb.Append(webServerRelativeUrl.TrimEnd('/'));
            sb.Append("/_layouts/ViewEdit.aspx?List=");
            sb.Append(listId);
            sb.Append("&View={");
            sb.Append(viewId);
            sb.Append("}&Source=");
            sb.Append(WebAppName);
            sb.Append(webServerRelativeUrl.TrimEnd('/'));
            sb.Append("/Lists/");
            sb.Append(listTitle);
            sb.Append("/AllItems.aspx");
            string html = string.Empty;//AveHttpWebRequestUtility.HttpGet(sb.ToString(), mObj, listTrimObj, new string[] { "Ordered" });
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(sb.ToString(), mObj,mTokenProvider);
            }
            catch (Exception e)
            {
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                SecurityTrimObject listTrimObj = webTrimObj.GetList(listId, listTitle);
                string[] properties = new string[] { "EditViewSetting" };
                foreach (string property in properties)
                {
                    if (!listTrimObj.TrimmedProperties.ContainsKey(property))
                    {
                        listTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", e.Message, sb.ToString());
                    }
                }
                return editViewProperties;
            }
            if (string.IsNullOrEmpty(html))
            {
                return editViewProperties;
            }
            string search = "id=\"OrderedView0\"";
            string content = AveHttpWebRequestUtility.GetInput(html, search, "/>");
            if (string.IsNullOrEmpty(content))
            {
                return editViewProperties;
            }
            editViewProperties["Ordered"] = content.Contains("checked=\"checked\"");
            return editViewProperties;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "chk is a part of xml")]
        public Dictionary<string, object> GetListAccessRequestsSettingProperties(String webServerRelativeUrl, Guid listId)
        {
            Dictionary<string, object> accessRequestsProperties = new Dictionary<string, object>();
            StringBuilder sb = new StringBuilder();
            sb.Append(WebAppName);
            sb.Append(webServerRelativeUrl.TrimEnd('/'));
            sb.Append("/_layouts/setrqacc.aspx?type=list&name={");
            sb.Append(listId);
            sb.Append("}&Source=");
            sb.Append(webServerRelativeUrl.TrimEnd('/'));
            sb.Append("/_layouts/user.aspx?obj={");
            sb.Append(listId);
            sb.Append("},DOCLIB&List={");
            sb.Append(listId);
            sb.Append("}&IsDlg=1");
            string html = string.Empty;//AveHttpWebRequestUtility.HttpGet(sb.ToString(), mObj);
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(sb.ToString(), mObj,mTokenProvider);
            }
            catch (Exception e)
            {
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                SecurityTrimObject listTrimObj = webTrimObj.GetList(listId, string.Empty);
                string[] properties = new string[] { "AccessRequestsSetting" };
                foreach (string property in properties)
                {
                    if (!listTrimObj.TrimmedProperties.ContainsKey(property))
                    {
                        listTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", e.Message, sb.ToString());
                    }
                }
                return accessRequestsProperties;
            }
            if (string.IsNullOrEmpty(html))
            {
                return accessRequestsProperties;
            }
            string search = "<input id=\"ctl00_PlaceHolderMain_ctl00_chkRequestAccess\"";
            string content = AveHttpWebRequestUtility.GetInput(html, search, "/>");
            if (string.IsNullOrEmpty(content))
            {
                return accessRequestsProperties;
            }
            accessRequestsProperties["RequestAccessEnabled"] = content.Contains("checked=\"checked\"");
            return accessRequestsProperties;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Rad is a part of xml")]
        public Dictionary<string, object> GetListAdvancedSettingProperties(string webServerRelativeUrl, Guid listId)
        {
            Dictionary<string, object> advancedProp = new Dictionary<string, object>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/advsetng.aspx?List=" + listId.ToString("B");
            string html = string.Empty;//AveHttpWebRequestUtility.HttpGet(getUrl, mObj, listTrimObj, trimedProperties);
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj,mTokenProvider);
            }
            catch (Exception e)
            {
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                SecurityTrimObject listTrimObj = webTrimObj.GetList(listId, string.Empty);
                string[] properties = new string[] { "AdvancedSetting" };
                foreach (string property in properties)
                {
                    if (!listTrimObj.TrimmedProperties.ContainsKey(property))
                    {
                        listTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", e.Message, getUrl);
                    }
                }
                return advancedProp;
            }
            if (string.IsNullOrEmpty(html))
            {
                return advancedProp;
            }
            string searchContent = "<input id=\"ctl00_PlaceHolderMain_OpenDocumentSection_ctl00_RadDefaultItemOpenPreferClient\"";
            string information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            if (!string.IsNullOrEmpty(information))
            {
                if (information.Contains("checked=\"checked\""))
                {
                    advancedProp["DefaultItemOpen"] = 0;
                }
                else
                {
                    advancedProp["DefaultItemOpen"] = 1;
                    searchContent = "<input id=\"ctl00_PlaceHolderMain_OpenDocumentSection_ctl00_RadDefaultItemOpenBrowser\"";
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
            searchContent = "<input name=\"ctl00$PlaceHolderMain$SendToSection$ctl00$TxtSendToLocationName\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            XmlDocument xmlDoc = new XmlDocument();
            if (!string.IsNullOrEmpty(information))
            {
                xmlDoc.LoadXml(information);
                XmlAttribute valueAtt = xmlDoc.FirstChild.Attributes["value"];
                advancedProp["SendToLocationName"] = valueAtt != null ? valueAtt.Value : string.Empty;
                searchContent = "<input name=\"ctl00$PlaceHolderMain$SendToSection$ctl01$TxtSendToLocationUrl\"";
                information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
                xmlDoc.LoadXml(information);
                valueAtt = xmlDoc.FirstChild.Attributes["value"];
                advancedProp["SendToLocationUrl"] = valueAtt != null ? valueAtt.Value : string.Empty;
            }
            searchContent = "<input id=\"ctl00_PlaceHolderMain_AllowSyncSection_ctl01_RadAllowSyncNo\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            advancedProp["ExcludeFromOfflineClient"] = information.Contains("checked=\"checked\"");
            searchContent = "<input id=\"ctl00_PlaceHolderMain_AllowGridEditingSection_ctl01_RadAllowGridNo\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            advancedProp["DisableGridEditing"] = information.Contains("checked=\"checked\"");
            searchContent = "<input id=\"ctl00_PlaceHolderMain_DialogForFormsPagesSection_ctl02_RadDialogForFormsPagesNo\"";
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
            searchContent = "<input id=\"ctl00_PlaceHolderMain_ManagedIndexesSection_ctl02_RadManagedIndexesNo\" ";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            advancedProp["EnableManagedIndexes"] = information.Contains("checked=\"checked\"");
            return advancedProp;
        }
        public List<Dictionary<string, object>> GetDisplayGroupsForSite()
        {
            List<Dictionary<string, object>> displayGroupProp = new List<Dictionary<string, object>>();
            GetScopeDisplayGroups(displayGroupProp);
            if (displayGroupProp.Count > 0)
            {
                Dictionary<string, object> tempGroupProp = new Dictionary<string, object>();
                GetScopeDisplayGroupsID(tempGroupProp);
                MergeGroupProperies(displayGroupProp, tempGroupProp);
            }
            return displayGroupProp;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "viewscopes is a part of url")]
        private void GetScopeDisplayGroups(List<Dictionary<string, object>> displayGroupProp)
        {
            string url = mWebUrl.TrimEnd('/') + "/_layouts/viewscopes.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(url, mObj,mTokenProvider);
            string searchContent = "<div id=\"__gvctl00_PlaceHolderMain_gridViewListScopes__div\">";
            string information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</div>");
            if (string.IsNullOrEmpty(information))
            {
                return;
            }
            GetScopeDisplayGroups(displayGroupProp, information);
            GetNextPageScopeDisplayGroups(displayGroupProp, html, url);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "gvct is a part of xml")]
        private void GetNextPageScopeDisplayGroups(List<Dictionary<string, object>> displayGroupProp, string html, string postUrl)
        {
            string searchContent = "<input type=\"hidden\"";
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$toolBar$RightRptControls$Pager";
            bodyDic["__EVENTARGUMENT"] = "nextpage";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            string postHtml = AveHttpWebRequestUtility.HttpReturn(postUrl, mObj, "application/x-www-form-urlencoded", body, null,string.Empty,mTokenProvider);
            searchContent = "<div id=\"__gvctl00_PlaceHolderMain_gridViewListScopes__div\">";
            string information = AveHttpWebRequestUtility.GetInput(postHtml, searchContent, "</div>");
            List<Dictionary<string, object>> nextPageProp = new List<Dictionary<string, object>>();
            GetScopeDisplayGroups(nextPageProp, information);
            if (nextPageProp.Count > 0)
            {
                if (!MergeGroups(displayGroupProp, nextPageProp))
                {
                    return;
                }
                GetNextPageScopeDisplayGroups(displayGroupProp, postHtml, postUrl);
            }
        }
        private bool MergeGroups(List<Dictionary<string, object>> displayGroupProp, List<Dictionary<string, object>> nextPageProp)
        {
            bool flag;
            foreach (Dictionary<string, object> dGroup in nextPageProp)
            {
                flag = false;
                foreach (Dictionary<string, object> sGroup in displayGroupProp)
                {
                    if (sGroup["Name"].ToString().Equals(dGroup["Name"].ToString()))
                    {
                        flag = true;
                        bool Compared = false; ;
                        foreach (Dictionary<string, object> dScope in dGroup["Scopes"] as List<Dictionary<string, object>>)
                        {
                            if (!Compared && IsScopeExistInGroup(sGroup["Scopes"] as List<Dictionary<string, object>>, dScope))
                            {
                                Compared = true;
                                return false;
                            }
                            (sGroup["Scopes"] as List<Dictionary<string, object>>).Add(dScope);
                        }
                        break;
                    }
                }
                if (!flag)
                {
                    displayGroupProp.Add(dGroup);
                }
            }
            return true;
        }
        private bool IsScopeExistInGroup(List<Dictionary<string, object>> sCopes, Dictionary<string, object> dScope)
        {
            foreach (Dictionary<string, object> scope in sCopes)
            {
                if (scope["Name"].ToString().Equals(dScope["Name"].ToString()))
                {
                    return true;
                }
            }
            return false;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ms-gb is a  key")]
        private void GetScopeDisplayGroups(List<Dictionary<string, object>> displayGroupProp, string information)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(information.Replace("&nbsp", "*"));
            XmlNode rootNode = xmlDoc.FirstChild.FirstChild;
            XmlNodeList groupNodes = rootNode.ChildNodes;
            for (int i = 2; i < groupNodes.Count; )
            {
                int flag = 0;
                Dictionary<string, object> groupProp = new Dictionary<string, object>();
                XmlNode groupNode = groupNodes[i];
                groupProp["Name"] = groupNode.FirstChild.InnerText.Substring(0, groupNode.FirstChild.InnerText.Length - 4);
                XmlNode scopeNode = groupNodes[++i];
                List<Dictionary<string, object>> scopesProp = new List<Dictionary<string, object>>();
                while (!(scopeNode.Attributes != null && scopeNode.Attributes["class"] != null && scopeNode.Attributes["class"].Value.Equals("ms-gb")))
                {
                    Dictionary<string, object> scopeProp = new Dictionary<string, object>();
                    if (flag == 0)
                    {
                        scopeProp["Default"] = true;
                        flag++;
                    }
                    else
                    {
                        scopeProp["Default"] = false;
                    }
                    XmlNodeList scopeInfoNodes = scopeNode.ChildNodes;
                    if (scopeInfoNodes.Count < 5)
                    {
                        scopeNode = groupNodes[++i];
                        if (scopeNode == null)
                        {
                            break;
                        }
                        continue;
                    }
                    scopeProp["Name"] = scopeInfoNodes[1].SelectNodes("descendant::span")[0].InnerText;
                    scopeProp["ID"] = GetScopeId(scopeInfoNodes[1].SelectNodes("descendant::a")[0]); ;
                    scopeProp["CompilationState"] = scopeInfoNodes[2].InnerText;
                    XmlNode shareDNode = scopeInfoNodes[3].SelectSingleNode("img");
                    scopeProp["IsShared"] = shareDNode == null ? false : true;
                    int itemCount = 0;
                    if (int.TryParse(scopeInfoNodes[4].InnerText, out itemCount))
                    {
                        scopeProp["Count"] = itemCount;
                    }
                    else
                    {
                        scopeProp["Count"] = scopeInfoNodes[4].InnerText;
                    }
                    scopesProp.Add(scopeProp);
                    scopeNode = groupNodes[++i];
                    if (scopeNode == null)
                    {
                        break;
                    }
                }
                groupProp.Add("Scopes", scopesProp);
                displayGroupProp.Add(groupProp);
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "gvct is a part of xml")]
        private void GetScopeDisplayGroupsID(Dictionary<string, object> tempGroupProp)
        {
            string getUrl = mWebUrl.TrimEnd('/') + "/_layouts/listdisplaygroups.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj,mTokenProvider);
            string searchContent = "<div id=\"__gvctl00_PlaceHolderMain_gridViewListScopes__div\">";
            string information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</div>");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(information.Replace("&nbsp", "*"));
            XmlNodeList tempGroupNodes = xmlDoc.FirstChild.FirstChild.ChildNodes;
            XmlNode tempGroupNode = null;
            string groupName;
            string Id;
            for (int i = 1; i < tempGroupNodes.Count; i++)
            {
                tempGroupNode = tempGroupNodes[i];
                groupName = tempGroupNode.SelectNodes("descendant::span")[0].InnerText;
                Id = tempGroupNode.SelectNodes("descendant::a")[0].Attributes["href"].Value.Split('=')[1];
                tempGroupProp[groupName] = Id;
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "menutokenvalues is a key")]
        private int GetScopeId(XmlNode iDNode)
        {
            string idText = iDNode.Attributes["menutokenvalues"].Value;
            return Convert.ToInt32(AveHttpWebRequestUtility.GetInnerText(idText, "ID=", ","));
        }
        private void MergeGroupProperies(List<Dictionary<string, object>> displayGroupProp, Dictionary<string, object> tempGroupProp)
        {
            string groupName = string.Empty;
            foreach (Dictionary<string, object> groupProp in displayGroupProp)
            {
                groupName = groupProp["Name"].ToString();
                if (tempGroupProp.Keys.Contains(groupName))
                {
                    groupProp["ID"] = Convert.ToInt32(tempGroupProp[groupName]);
                    tempGroupProp.Remove(groupName);
                }
            }
            foreach (KeyValuePair<string, object> groupInfo in tempGroupProp)
            {
                Dictionary<string, object> groupProperties = new Dictionary<string, object>();
                groupProperties["Name"] = groupInfo.Key;
                groupProperties["ID"] = Convert.ToInt32(groupInfo.Value);
                groupProperties["Scopes"] = new List<Dictionary<string, object>>();
                groupProperties["Default"] = null;
                displayGroupProp.Add(groupProperties);
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "listkeywords is a part of url")]
        public List<Dictionary<string, object>> GetKeyWords()
        {
            List<Dictionary<string, object>> keyWordsProp = new List<Dictionary<string, object>>();
            string getUrl = mWebUrl.TrimEnd('/') + "/_layouts/listkeywords.aspx";
            this.GetKeyWords(getUrl, keyWordsProp);
            return keyWordsProp;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "pnext is a part of url")]
        private void GetKeyWords(string getUrl, List<Dictionary<string, object>> keyWordsProp)
        {
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj);
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
                nextUrl.Append(WebAppName.TrimEnd('/'));
                nextUrl.Append('/');
                nextUrl.Append(AveHttpWebRequestUtility.GetValue(str, "href=\""));
                this.GetKeyWords(nextUrl.ToString(), keyWordsProp);
            }
        }
        private Dictionary<string, object> GetKeyWordProperties(string keyWordName)
        {
            Dictionary<string, object> keyWordProp = new Dictionary<string, object>();
            string getUrl = mWebUrl.TrimEnd('/') + "/_layouts/Keyword.aspx?k=" + keyWordName;
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj,mTokenProvider);
            GetKeyWordProperties(keyWordName, html, keyWordProp);
            return keyWordProp;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "syn is a part of xml")]
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
            searchContent = "<a href=\"#\" onclick='clickDatePicker(\"ctl00_PlaceHolderMain_startDate_startDateDate\"";//"<input name=\"ctl00$PlaceHolderMain$startDate$startDateDate\"";//
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
            GetBestBetsProperties(html, keyWordName, bestBetsProp);
            keyWordProp.Add("BestBets" + AveObjectModelConstant.ObjectPropertySuffix, bestBetsProp);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "textarea is a part of xml")]
        private void GetBestBetsProperties(string html, string keyWordName, List<Dictionary<string, object>> bestBetsProp)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            HtmlNode node = htmlDocument.DocumentNode.ChildNodes["html"];
            HtmlNodeCollection nodes = node.SelectNodes("//tr[@valign='top']");
            if (nodes != null)
            {
                for (int i = 1; i < nodes.Count; i++)
                {
                    Dictionary<string, object> bestBetProp = new Dictionary<string, object>();
                    HtmlNode bestBetNode = nodes[i];
                    HtmlNode titleNode = bestBetNode.SelectSingleNode("./td/table/tr/td/label");
                    bestBetProp["Title"] = titleNode.InnerText;
                    HtmlNode urlNode = bestBetNode.SelectSingleNode("./td/table/input[@type='hidden']");
                    string bestBetUrl = urlNode.Attributes["value"].Value;
                    bestBetProp["Url"] = bestBetUrl;
                    string url = string.Format("{0}/_layouts/BestBet.aspx?u={1}&k={2}&IsDlg=1", mWebUrl.TrimEnd('/'), bestBetUrl, keyWordName);
                    string descriptionHtml = AveHttpWebRequestUtility.HttpGet(url, mObj,mTokenProvider);
                    string xml = AveHttpWebRequestUtility.GetInput(descriptionHtml, "<textarea name=\"descriptionTextBox", "</textarea>");
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    bestBetProp["Description"] = doc.InnerText;
                    bestBetsProp.Add(bestBetProp);
                }
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "prjsetng is a part of url")]
        public Dictionary<string, object> GetWebLogoProperties(string webServerRelativeUrl)
        {
            Dictionary<string, object> webLogoProp = new Dictionary<string, object>();
            string getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/prjsetng.aspx";
            string html = string.Empty;//AveHttpWebRequestUtility.HttpGet(getUrl, mObj, webTrimObj, "SiteLogoUrl", "SiteLogoDescription");
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj,mTokenProvider);
            }
            catch (Exception e)
            {
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                string[] properties = new string[] { "WebLogo" };
                foreach (string property in properties)
                {
                    if (!webTrimObj.TrimmedProperties.ContainsKey(property))
                    {
                        webTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", e.Message, getUrl);
                    }
                }
                return webLogoProp;
            }
            if (!string.IsNullOrEmpty(html))
            {
                string searContent = "<input name=\"ctl00$PlaceHolderMain$ctl01$ctl02$TxtSiteLogoUrl\"";
                string infomation = AveHttpWebRequestUtility.GetInput(html, searContent, "/>");
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(infomation);
                webLogoProp["SiteLogoUrl"] = xmlDoc.FirstChild.Attributes["value"] != null ? xmlDoc.FirstChild.Attributes["value"].Value : default(string);
                searContent = "<textarea name=\"ctl00$PlaceHolderMain$ctl01$ctl03$TxtLogoUrlDescription\"";
                infomation = AveHttpWebRequestUtility.GetInput(html, searContent, "</textarea>");
                xmlDoc.LoadXml(infomation);
                webLogoProp["SiteLogoDescription"] = xmlDoc.FirstChild.InnerText.StartsWith("\r\n", StringComparison.OrdinalIgnoreCase) ? xmlDoc.FirstChild.InnerText.Substring(2) : xmlDoc.FirstChild.InnerText;
            }
            return webLogoProp;
        }

        //public Dictionary<string, object> GetWorkflowTemplates(string webServerRelativeUrl, string webName, Guid webId, string workflowSource, Dictionary<string, object> contentTypeProp) { throw new NotImplementedException(); }
        //public Dictionary<string, object> GetCustomListTemplates(string webServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ms-standardheader is a part of xml")]
        public Dictionary<string, object> GetAllFeatureDefinitions(string Url, string featuresSource)
        {
            Dictionary<string, object> featureProp = new Dictionary<string, object>();
            Dictionary<string, object> featureDefinitions = new Dictionary<string, object>();
            List<Dictionary<string, object>> featureDefinitionList = new List<Dictionary<string, object>>();
            string getUrl = string.Empty;
            switch (featuresSource)
            {
                case "web.features":
                    getUrl = Url.TrimEnd('/') + "/_layouts/ManageFeatures.aspx";
                    break;
                case "site.features":
                    getUrl = Url.TrimEnd('/') + "/_layouts/ManageFeatures.aspx?Scope=Site";
                    break;
            }
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj,mTokenProvider);

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
        //public bool DoesUserHavePermissions(string webServerRelativeUrl, ulong permissionMask)
        //{
        //    throw new NotImplementedException();
        //}

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "rad is a part of url")]
        public static void GetWebSearchAndOfflineAvailability(string webApp, string webServerRelativeUrl, Dictionary<string, object> webProp, object obj)
        {
            GetWebSearchAndOfflineAvailability(webApp, webServerRelativeUrl, webProp, obj, null);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "rad is a part of url")]
        public static void GetWebSearchAndOfflineAvailability(string webApp, string webServerRelativeUrl, Dictionary<string, object> webProp, object obj, ITokenProvider tokenProvider)
        {
            string getUrl = webApp + webServerRelativeUrl.TrimEnd('/') + "/_layouts/srchvis.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, obj, tokenProvider);
            if (string.IsNullOrEmpty(html))
            {
                return;
            }
            string radIndexSiteContent = AveHttpWebRequestUtility.GetInput(html, "ctl00_PlaceHolderMain_IndexSiteContent_ctl00_radIndexSiteContentNo", "/>");
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
            string allowSync = AveHttpWebRequestUtility.GetInput(html, "ctl00_PlaceHolderMain_AllowSyncSection_ctl00_RadAllowSyncYes", "/>");
            if (allowSync.Contains("checked=\"checked\""))
            {
                webProp["ExcludeFromOfflineClient"] = false;
            }
            else
            {
                webProp["ExcludeFromOfflineClient"] = true;
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Chk is a part of xml")]
        public Dictionary<string, object> GetWebRegionalSetting(string webServerRelativeUrl)
        {
            Dictionary<string, object> regionalProp = new Dictionary<string, object>();
            string getUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/regionalsetng.aspx";
            string html = string.Empty;//AveHttpWebRequestUtility.HttpGet(getUrl, mObj, webTrimObj, "RegionalSettings");
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj,mTokenProvider);
            }
            catch (Exception e)
            {
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                string[] properties = new string[] { "RegionalSetting" };
                foreach (string property in properties)
                {
                    if (!webTrimObj.TrimmedProperties.ContainsKey(property))
                    {
                        webTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", e.Message, getUrl);
                    }
                }
                return regionalProp;
            }
            if (!string.IsNullOrEmpty(html))
            {
                string local = AveHttpWebRequestUtility.GetSelectInputValue(html, "<select name=\"ctl00$PlaceHolderMain$ctl00$ctl00$DdlwebLCID\" id=\"ctl00_PlaceHolderMain_ctl00_ctl00_DdlwebLCID\">");
                regionalProp["LocaleId"] = uint.Parse(local);
                string sortOrder = AveHttpWebRequestUtility.GetSelectInputValue(html, "<select name=\"ctl00$PlaceHolderMain$ctl07$ctl00$DdlwebCollation\" id=\"ctl00_PlaceHolderMain_ctl07_ctl00_DdlwebCollation\">");
                regionalProp["Collation"] = short.Parse(sortOrder);
                string timeZone = AveHttpWebRequestUtility.GetSelectInputValue(html, "<select name=\"ctl00$PlaceHolderMain$ctl01$ctl00$DdlwebTimeZone\" id=\"ctl00_PlaceHolderMain_ctl01_ctl00_DdlwebTimeZone\">");
                Dictionary<string, object> timeZoneProp = AveHttpWebRequestUtility.GetSelectInput(html, "<select name=\"ctl00$PlaceHolderMain$ctl01$ctl00$DdlwebTimeZone\" id=\"ctl00_PlaceHolderMain_ctl01_ctl00_DdlwebTimeZone\">");
                timeZoneProp["ID"] = ushort.Parse(timeZoneProp["Value"].ToString());
                timeZoneProp["Description"] = timeZoneProp["Text"];
                regionalProp["TimeZone" + AveObjectModelConstant.ObjectPropertySuffix] = timeZoneProp;

                #region calendar
                string setCalendar = AveHttpWebRequestUtility.GetSelectInputValue(html, "<select name=\"ctl00$PlaceHolderMain$ctl02$ctl00$DdlwebCalType\" id=\"ctl00_PlaceHolderMain_ctl02_ctl00_DdlwebCalType\">");
                regionalProp["CalendarType"] = short.Parse(setCalendar);
                string adjustHijriDays = AveHttpWebRequestUtility.GetSelectInputValue(html, "<select name=\"ctl00$PlaceHolderMain$ctl02$ctl02$DdlwebHijriDays\" id=\"ctl00_PlaceHolderMain_ctl02_ctl02_DdlwebHijriDays\">");
                if (string.IsNullOrEmpty(adjustHijriDays))
                {
                    regionalProp["AdjustHijriDays"] = (short)1;
                }
                else
                {
                    regionalProp["AdjustHijriDays"] = short.Parse(adjustHijriDays);
                }

                bool showWeeks = AveHttpWebRequestUtility.GetCheckInput(html, "ctl00$PlaceHolderMain$ctl02$ctl01$ChkShowWeekNumber");
                regionalProp["ShowWeeks"] = showWeeks;
                string alternateCalendar = AveHttpWebRequestUtility.GetSelectInputValue(html, "<select name=\"ctl00$PlaceHolderMain$ctl03$ctl00$DdlwebAltCalType\" id=\"ctl00_PlaceHolderMain_ctl03_ctl00_DdlwebAltCalType\">");
                regionalProp["AlternateCalendarType"] = short.Parse(alternateCalendar);
                #endregion

                #region time and time format
                string firstDay = AveHttpWebRequestUtility.GetSelectInputValue(html, "<select name=\"ctl00$PlaceHolderMain$ctl04$ctl01$DdlFirstDayOfWeek\" id=\"ctl00_PlaceHolderMain_ctl04_ctl01_DdlFirstDayOfWeek\">");
                regionalProp["FirstDayOfWeek"] = uint.Parse(firstDay);
                string firstWeek = AveHttpWebRequestUtility.GetSelectInputValue(html, "<select name=\"ctl00$PlaceHolderMain$ctl04$ctl01$DdlFirstWeekOfYear\" id=\"ctl00_PlaceHolderMain_ctl04_ctl01_DdlFirstWeekOfYear\">");
                regionalProp["FirstWeekOfYear"] = short.Parse(firstWeek);

                CultureInfo info = new CultureInfo(int.Parse(local), false);
                string am = info.DateTimeFormat.AMDesignator;
                string pm = info.DateTimeFormat.PMDesignator;
                int designerIndex = -1;
                int designerLength = -1;

                string startTime = AveHttpWebRequestUtility.GetSelectInputValue(html, "<select name=\"ctl00$PlaceHolderMain$ctl04$ctl01$DdlStartTime\" id=\"ctl00_PlaceHolderMain_ctl04_ctl01_DdlStartTime\">");
                int sTimeIndex = startTime.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                int startHour = 0;
                try
                {
                    if (!string.IsNullOrEmpty(am) && startTime.Contains(am))
                    {
                        designerIndex = startTime.IndexOf(am, StringComparison.OrdinalIgnoreCase);
                        designerLength = am.Length + 1;
                        if (designerIndex == 0)
                        {
                            startHour = int.Parse(startTime.Substring(designerLength, sTimeIndex - designerLength));
                        }
                        else
                        {
                            startHour = int.Parse(startTime.Substring(0, sTimeIndex));
                        }
                    }
                    else if (!string.IsNullOrEmpty(pm) && startTime.Contains(pm))
                    {
                        designerIndex = startTime.IndexOf(pm, StringComparison.OrdinalIgnoreCase);
                        designerLength = pm.Length + 1;
                        if (designerIndex == 0)
                        {
                            startHour = int.Parse(startTime.Substring(designerLength, sTimeIndex - designerLength));
                        }
                        else
                        {
                            startHour = int.Parse(startTime.Substring(0, sTimeIndex));
                        }
                    }
                    else
                    {
                        startHour = int.Parse(startTime.Substring(0, sTimeIndex));
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                string endTime = AveHttpWebRequestUtility.GetSelectInputValue(html, "<select name=\"ctl00$PlaceHolderMain$ctl04$ctl01$DdlEndTime\" id=\"ctl00_PlaceHolderMain_ctl04_ctl01_DdlEndTime\">");
                int eTimeIndex = endTime.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                int endHour = 0;
                try
                {
                    if (!string.IsNullOrEmpty(am) && endTime.Contains(am))
                    {
                        designerIndex = endTime.IndexOf(am, StringComparison.OrdinalIgnoreCase);
                        designerLength = am.Length + 1;
                        if (designerIndex == 0)
                        {
                            endHour = int.Parse(endTime.Substring(designerLength, eTimeIndex - designerLength));
                        }
                        else
                        {
                            endHour = int.Parse(endTime.Substring(0, eTimeIndex));
                        }
                    }
                    else if (!string.IsNullOrEmpty(pm) && endTime.Contains(pm))
                    {
                        designerIndex = endTime.IndexOf(pm, StringComparison.OrdinalIgnoreCase);
                        designerLength = pm.Length + 1;
                        if (designerIndex == 0)
                        {
                            endHour = int.Parse(endTime.Substring(designerLength, eTimeIndex - designerLength));
                        }
                        else
                        {
                            endHour = int.Parse(endTime.Substring(0, eTimeIndex));
                        }
                    }
                    else
                    {
                        endHour = int.Parse(endTime.Substring(0, eTimeIndex));
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                string timeForamt = AveHttpWebRequestUtility.GetSelectInputValue(html, "<select name=\"ctl00$PlaceHolderMain$ctl08$ctl00$DdlTimeFormat\" id=\"ctl00_PlaceHolderMain_ctl08_ctl00_DdlTimeFormat");
                if (timeForamt.Equals("0"))
                {
                    regionalProp["Time24"] = false;
                    if (startHour == 12)
                    {
                        startHour = 0;
                    }
                    if (endHour == 12)
                    {
                        endHour = 0;
                    }
                    if (!string.IsNullOrEmpty(pm) && startTime.Contains(pm))
                    {
                        regionalProp["WorkDayStartHour"] = (short)((startHour + 12) * 60);
                    }
                    else
                    {
                        regionalProp["WorkDayStartHour"] = (short)(startHour * 60);
                    }
                    if (!string.IsNullOrEmpty(pm) && endTime.Contains(pm))
                    {
                        regionalProp["WorkDayEndHour"] = (short)((endHour + 12) * 60);
                    }
                    else
                    {
                        regionalProp["WorkDayEndHour"] = (short)(endHour * 60);
                    }
                }
                else
                {
                    regionalProp["Time24"] = true;
                    regionalProp["WorkDayStartHour"] = (short)(startHour * 60);
                    regionalProp["WorkDayEndHour"] = (short)(endHour * 60);
                }
                #endregion

                #region work days
                int workDays = 0;
                bool sun = AveHttpWebRequestUtility.GetCheckInput(html, "ctl00$PlaceHolderMain$ctl04$ctl00$ChkListWeeklyMultiDays$0");
                if (sun)
                {
                    workDays += 64;
                }
                bool mon = AveHttpWebRequestUtility.GetCheckInput(html, "ctl00$PlaceHolderMain$ctl04$ctl00$ChkListWeeklyMultiDays$1");
                if (mon)
                {
                    workDays += 32;
                }
                bool tue = AveHttpWebRequestUtility.GetCheckInput(html, "ctl00$PlaceHolderMain$ctl04$ctl00$ChkListWeeklyMultiDays$2");
                if (tue)
                {
                    workDays += 16;
                }
                bool wen = AveHttpWebRequestUtility.GetCheckInput(html, "ctl00$PlaceHolderMain$ctl04$ctl00$ChkListWeeklyMultiDays$3");
                if (wen)
                {
                    workDays += 8;
                }
                bool thu = AveHttpWebRequestUtility.GetCheckInput(html, "ctl00$PlaceHolderMain$ctl04$ctl00$ChkListWeeklyMultiDays$4");
                if (thu)
                {
                    workDays += 4;
                }
                bool fri = AveHttpWebRequestUtility.GetCheckInput(html, "ctl00$PlaceHolderMain$ctl04$ctl00$ChkListWeeklyMultiDays$5");
                if (fri)
                {
                    workDays += 2;
                }
                bool sat = AveHttpWebRequestUtility.GetCheckInput(html, "ctl00$PlaceHolderMain$ctl04$ctl00$ChkListWeeklyMultiDays$6");
                if (sat)
                {
                    workDays += 1;
                }
                regionalProp["WorkDays"] = (short)workDays;
                #endregion

            }
            return regionalProp;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Ddl is a part of xml")]
        public Dictionary<string, object> GetDefaultRegionalSetting(string webServerRelativeUrl, int lcid)
        {
            Dictionary<string, object> defaultRegionalProp = new Dictionary<string, object>();
            defaultRegionalProp["LocaleId"] = lcid;
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/regionalsetng.aspx";
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj,mTokenProvider);
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = System.Web.HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = System.Web.HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["ctl00%24PlaceHolderMain%24ctl00%24ctl00%24DdlwebLCID"] = lcid;
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl00%24ctl00%24DdlwebLCID";
            bodyDic["Cmd"] = "UPDATEPROJECT";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            string defaultHtml = AveHttpWebRequestUtility.HttpReturn(postUrl, mObj, "application/x-www-form-urlencoded", data, null,string.Empty,mTokenProvider);
            if (!string.IsNullOrEmpty(defaultHtml))
            {
                string sortOrder = AveHttpWebRequestUtility.GetSelectInputValue(defaultHtml, "<select name=\"ctl00$PlaceHolderMain$ctl07$ctl00$DdlwebCollation\" id=\"ctl00_PlaceHolderMain_ctl07_ctl00_DdlwebCollation\">");
                defaultRegionalProp["Collation"] = int.Parse(sortOrder);
                string setCalendar = AveHttpWebRequestUtility.GetSelectInputValue(defaultHtml, "<select name=\"ctl00$PlaceHolderMain$ctl02$ctl00$DdlwebCalType\" id=\"ctl00_PlaceHolderMain_ctl02_ctl00_DdlwebCalType\">");
                defaultRegionalProp["CalendarType"] = int.Parse(setCalendar);
                string timeForamt = AveHttpWebRequestUtility.GetSelectInputValue(defaultHtml, "<select name=\"ctl00$PlaceHolderMain$ctl08$ctl00$DdlTimeFormat\" id=\"ctl00_PlaceHolderMain_ctl08_ctl00_DdlTimeFormat\">");
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

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "themeweb.aspx: A part url of theme url.")]
        public Dictionary<string, object> GetThemeUrlForWeb(string webServerRelativeUrl, int compatibilityLevel)
        {
            Dictionary<string, object> ThemeDic = new Dictionary<string, object>();
            string getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/themeweb.aspx";
            string html = string.Empty;//AveHttpWebRequestUtility.HttpGet(getUrl, mObj, webTrimObj, "ThemedCssFolderUrl", "ThemedTemplate");
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj,mTokenProvider);
            }
            catch (Exception e)
            {
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                string[] properties = new string[] { "ThemedCssFolderUrl", "ThemedTemplate" };
                foreach (string property in properties)
                {
                    if (!webTrimObj.TrimmedProperties.ContainsKey(property))
                    {
                        webTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", e.Message, getUrl);
                    }
                }
                return ThemeDic;
            }
            if (!string.IsNullOrEmpty(html))
            {
                string searchContent = "themes['_current'] = {\"ServerRelativeUrl\":\"";
                int startIndex = html.IndexOf(searchContent, StringComparison.OrdinalIgnoreCase);
                if (startIndex > 0)
                {
                    startIndex = startIndex + searchContent.Length;
                    int endIndex = html.IndexOf("\"", startIndex, StringComparison.OrdinalIgnoreCase);
                    string themeUrl = html.Substring(startIndex, endIndex - startIndex);
                    ThemeDic["ThemeUrl"] = themeUrl;
                    themeUrl = themeUrl.Substring(0, themeUrl.LastIndexOf("/", StringComparison.OrdinalIgnoreCase));
                    ThemeDic["ThemedCssFolderUrl"] = themeUrl;

                    string search = "\"Name\":\"";
                    startIndex = html.IndexOf("\"Name\":\"", endIndex, StringComparison.OrdinalIgnoreCase);
                    startIndex = startIndex + search.Length;
                    endIndex = html.IndexOf("\"", startIndex, StringComparison.OrdinalIgnoreCase);
                    string themeTemplate = html.Substring(startIndex, endIndex - startIndex);
                    ThemeDic["ThemedTemplate"] = themeTemplate;
                }
            }
            return ThemeDic;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Special characters of solution's field xml.")]
        public Dictionary<string, object> GetThmxThemeInfo(string webServerRelativeUrl)
        {
            Dictionary<string, string> colorDic = new Dictionary<string, string>();
            Dictionary<string, object> ThemeInfoDic = new Dictionary<string, object>();
            string getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/themeweb.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj,mTokenProvider);
            string endContent = "/>";
            colorDic["DarkColor1"] = "<input name=\"ctl00$PlaceHolderMain$ctl82$customizeThemeSection$dark1\"";
            colorDic["LightColor1"] = "<input name=\"ctl00$PlaceHolderMain$ctl82$customizeThemeSection$light1\"";
            colorDic["DarkColor2"] = "<input name=\"ctl00$PlaceHolderMain$ctl82$customizeThemeSection$dark2\"";
            colorDic["LightColor2"] = "<input name=\"ctl00$PlaceHolderMain$ctl82$customizeThemeSection$light2\"";
            colorDic["AccentColor1"] = "<input name=\"ctl00$PlaceHolderMain$ctl82$customizeThemeSection$accent1\"";
            colorDic["AccentColor2"] = "<input name=\"ctl00$PlaceHolderMain$ctl82$customizeThemeSection$accent2\"";
            colorDic["AccentColor3"] = "<input name=\"ctl00$PlaceHolderMain$ctl82$customizeThemeSection$accent3\"";
            colorDic["AccentColor4"] = "<input name=\"ctl00$PlaceHolderMain$ctl82$customizeThemeSection$accent4\"";
            colorDic["AccentColor5"] = "<input name=\"ctl00$PlaceHolderMain$ctl82$customizeThemeSection$accent5\"";
            colorDic["AccentColor6"] = "<input name=\"ctl00$PlaceHolderMain$ctl82$customizeThemeSection$accent6\"";
            colorDic["HyperlinkColor"] = "<input name=\"ctl00$PlaceHolderMain$ctl82$customizeThemeSection$hlink\"";
            colorDic["FollowedHyperlinkColor"] = "<input name=\"ctl00$PlaceHolderMain$ctl82$customizeThemeSection$folHlink\"";
            foreach (string colProp in colorDic.Keys)
            {
                string searchContent = colorDic[colProp];
                string infomation = AveHttpWebRequestUtility.GetInput(html, searchContent, endContent);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(infomation);
                ThemeInfoDic[colProp] = doc.FirstChild.Attributes["value"] != null ? doc.FirstChild.Attributes["value"].Value : default(string);
            }
            string font1 = "ctl00$PlaceHolderMain$ctl82$customizeThemeSection$font1";
            string searchKey = "selected=\"selected\" value=\"";
            int index1 = html.IndexOf(font1, StringComparison.OrdinalIgnoreCase);
            int startIndex1 = html.IndexOf(searchKey, index1, StringComparison.OrdinalIgnoreCase);
            if (startIndex1 > 0)
            {
                startIndex1 = startIndex1 + searchKey.Length;
                int endIndex = html.IndexOf("\"", startIndex1, StringComparison.OrdinalIgnoreCase);
                string majorFont = html.Substring(startIndex1, endIndex - startIndex1);
                ThemeInfoDic["MajorFont"] = majorFont;
            }
            string font2 = "ctl00$PlaceHolderMain$ctl82$customizeThemeSection$font2";
            int index2 = html.IndexOf(font2, StringComparison.OrdinalIgnoreCase);
            int startIndex2 = html.IndexOf(searchKey, index2, StringComparison.OrdinalIgnoreCase);
            if (startIndex2 > 0)
            {
                startIndex2 = startIndex2 + searchKey.Length;
                int endIndex = html.IndexOf("\"", startIndex2, StringComparison.OrdinalIgnoreCase);
                string minorFont = html.Substring(startIndex2, endIndex - startIndex2);
                ThemeInfoDic["MinorFont"] = minorFont;
            }
            return ThemeInfoDic;

        }
        public Dictionary<string, object> GetMasterPageProperties(string webServerRelativeUrl)
        {
            Dictionary<string, object> masterPropDic = new Dictionary<string, object>();
            string getUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_Layouts/ChangeSiteMasterPage.aspx";
            string html = string.Empty;//AveHttpWebRequestUtility.HttpGet(getUrl, mObj);
            try
            {
                html = AveHttpWebRequestUtility.HttpGet(getUrl, mObj,mTokenProvider);
            }
            catch (Exception e)
            {
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webServerRelativeUrl, mSiteTrimObj.Name);
                string[] properties = new string[] { "CustomMasterUrl", "MasterUrl", "AlternateCssUrl" };
                foreach (string property in properties)
                {
                    if (!webTrimObj.TrimmedProperties.ContainsKey(property))
                    {
                        webTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", e.Message, getUrl);
                    }
                }
                return masterPropDic;
            }
            int startIndex = 0;
            string searchKey = "selected=\"selected\" value=\"";
            int index = html.IndexOf("ctl00$PlaceHolderMain$ctl00$ctl00$masterPageSelectionControl$ctl00$SiteMasterPageDropDownList", StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                startIndex = html.IndexOf(searchKey, index, StringComparison.OrdinalIgnoreCase);
                if (startIndex > 0)
                {
                    startIndex = startIndex + searchKey.Length;
                    int endIndex = html.IndexOf("\"", startIndex, StringComparison.OrdinalIgnoreCase);
                    string siteMasterUrl = html.Substring(startIndex, endIndex - startIndex);
                    masterPropDic["CustomMasterUrl"] = siteMasterUrl;
                }
            }
            index = html.IndexOf("ctl00$PlaceHolderMain$ctl01$ctl00$systemMasterPageSelectionControl$ctl00$SystemMasterPageDropDownList", StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                startIndex = html.IndexOf(searchKey, index, StringComparison.OrdinalIgnoreCase);
                if (startIndex > 0)
                {
                    startIndex = startIndex + searchKey.Length;
                    int endIndex = html.IndexOf("\"", startIndex, StringComparison.OrdinalIgnoreCase);
                    string sysMasterUrl = html.Substring(startIndex, endIndex - startIndex);
                    masterPropDic["MasterUrl"] = sysMasterUrl;
                }
            }
            string searchContent = "<input name=\"ctl00$PlaceHolderMain$ctl02$ctl00$alternateCssSelector$AssetUrlInput\"";
            string infomation = AveHttpWebRequestUtility.GetInput(html, searchContent, "/>");
            if (!string.IsNullOrEmpty(infomation))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(infomation);
                masterPropDic["AlternateCssUrl"] = doc.FirstChild.Attributes["value"] != null ? doc.FirstChild.Attributes["value"].Value : default(string);
            }
            return masterPropDic;
        }
        //public Dictionary<string, object> OpenThmxTheme(string fileServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "siterss is a part of url")]
        public bool GetSiteRssSetting()
        {
            string netWorkUrl = mWebUrl.TrimEnd('/') + "/_layouts/siterss.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(netWorkUrl, mObj,mTokenProvider);
            bool allowSiteRss = AveHttpWebRequestUtility.GetCheckInput(html, "ctl00$PlaceHolderMain$SiteColRssSection$ctl00$CheckSiteColRss");
            return allowSiteRss;
        }
        //public Dictionary<string, object> GetList(string webServerRelativeUrl, Guid listId)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> GetListByTitle(Guid webId, string listTitle)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> GetRelatedFieldProperties(string webServerRelativeUrl, string fieldName, string fieldSource, string listTitle, Guid listId)
        //{
        //    throw new NotImplementedException();
        //}
        //#endregion

        #region Add

        /// <summary>
        /// Need to be optimized
        /// </summary>
        /// <param name="parameters"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "downlevel is a sharepoint setting page attribute")]
        public void CustomizeReport(Dictionary<string, object> parameters, Guid reportId)
        {
            string postUrl = mWebUrl.TrimEnd('/') + "/_layouts/CustomizeReport.aspx?ReportId=" + reportId.ToString() + "&Category=Auditing";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj,mTokenProvider);
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

            bodyDic["ctl00$PlaceHolderMain$ctl00$ctl01$TxtReportStorageLocation"] = (string)parameters["LibraryLocation"];// "/docave library";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsLocation$ctl00$serializedId"] = "";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsDates$ctl00$DTCStartDate$DTCStartDateDate"] = (string)parameters["StartDateDate"];// "4/2/2012";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsDates$ctl00$DTCStartDate$DTCStartDateDateHours"] = (string)parameters["StartDateDateHours"];// "12 AM";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsDates$ctl00$DTCStartDate$DTCStartDateDateMinutes"] = (string)parameters["StartDateDateMinutes"];// "00";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsDates$ctl00$DTCEndDate$DTCEndDateDate"] = (string)parameters["EndDateDate"];// "4/2/2013";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsDates$ctl00$DTCEndDate$DTCEndDateDateHours"] = (string)parameters["EndDateDateHours"];// "12 AM";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsDates$ctl00$DTCEndDate$DTCEndDateDateMinutes"] = (string)parameters["EndDateDateMinutes"];// "00";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsUser$ctl00$userPicker$hiddenSpanData"] = "";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsUser$ctl00$userPicker$OriginalEntities"] = "<Entities />";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsUser$ctl00$userPicker$HiddenEntityKey"] = "";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsUser$ctl00$userPicker$HiddenEntityDisplayText"] = "";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ifsUser$ctl00$userPicker$downlevelTextBox"] = "&#160;";

            if (parameters.ContainsKey("View") && ((string)parameters["View"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditView"] = "on";
            }
            if (parameters.ContainsKey("Update") && ((string)parameters["Update"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditUpdate"] = "on";
            }
            if (parameters.ContainsKey("CheckInOut") && ((string)parameters["CheckInOut"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditCheckInOut"] = "on";
            }
            if (parameters.ContainsKey("MoveCopy") && ((string)parameters["MoveCopy"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditMoveCopy"] = "on";
            }
            if (parameters.ContainsKey("DeleteRestore") && ((string)parameters["DeleteRestore"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditDeleteRestore"] = "on";
            }
            if (parameters.ContainsKey("ColumnContentType") && ((string)parameters["ColumnContentType"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditColumnContentType"] = "on";
            }
            if (parameters.ContainsKey("Search") && ((string)parameters["Search"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditSearch"] = "on";
            }
            if (parameters.ContainsKey("Perms") && ((string)parameters["Perms"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditPerms"] = "on";
            }
            if (parameters.ContainsKey("Change") && ((string)parameters["Change"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditChange"] = "on";
            }
            if (parameters.ContainsKey("Workflow") && ((string)parameters["Workflow"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditWorkflow"] = "on";
            }
            if (parameters.ContainsKey("Custom") && ((string)parameters["Custom"]).Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl03$ifsEvents$ctl00$CheckBoxAuditCustom"] = "on";
            }
            bodyDic["ctl00$PlaceHolderMain$ctl01$RptControls$btnOK"] = "Ok";

            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);

            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null,mTokenProvider);
        }
        //public Dictionary<string, object> AddRoleDefinition(string webServerRelativeUrl, Dictionary<string, object> roleDefinitionProperties)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> AddRoleAssignment(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> roleAssignmentProperties, string roleAssignmentsSource)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, int itemId, int eventType, int frequency, bool isSendEmail)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, int eventType, int frequency, bool isSendEmail)
        //{
        //    throw new NotImplementedException();
        //}
        public Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> data)
        {
            Dictionary<string, object> featureProp = new Dictionary<string, object>();
            AveHttpValueCollection values = new AveHttpValueCollection();
            values["List"] = listId.ToString("B");
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            string netWorkUrl = string.Empty;
            if (itemId == -2)
            {
                values["Source"] = webServerRelativeUrl.TrimEnd('/') + "/_layouts/Mysubs.aspx";
                //netWorkUrl = webFullUrl.TrimEnd('/') + "/_layouts/SubNew.aspx?List={" + listId.ToString() + "}&Source=" + webServerRelativeUrl.TrimEnd('/') + "/_layouts/Mysubs.aspx";
                netWorkUrl = webFullUrl.TrimEnd('/') + "/_layouts/SubNew.aspx?" + values.ToString(true);
            }
            else
            {
                values["ID"] = itemId.ToString();
                values["Source"] = webFullUrl + listUrl;
                //List={" + listId.ToString() + "}&ID=" + itemId.ToString() + "&Source=" + webFullUrl + listUrl;
                netWorkUrl = webFullUrl.TrimEnd('/') + "/_layouts/SubNew.aspx?" + values.ToString(true);
            }
            string html = AveHttpWebRequestUtility.HttpGet(netWorkUrl, this.mObj,mTokenProvider);//mNetWork.HttpGet(netWorkUrl);
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
            bodyDic["ctl00$PlaceHolderMain$ctl02$ctl00$TextTitle"] = data["AlertTitle"].ToString();
            bodyDic["ctl00$PlaceHolderMain$ctl04$ctl01$rdoDC"] = "rdo_EmailDC";
            bodyDic["ctl00$PlaceHolderMain$ctl06$ctl01$RadioBtnAlertFilter"] = AveHttpWebRequestUtility.GetFilterValue(data);
            if (data.ContainsKey("ViewId"))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl06$ctl01$DdlView"] = data["ViewId"].ToString();
            }
            bodyDic["ctl00$PlaceHolderMain$ctl05$ctl00$RadioBtnEventType"] = data["EventType"].ToString();
            bodyDic["ctl00$PlaceHolderMain$hdnAlwaysNotify"] = "False";
            AveHttpWebRequestUtility.UpateAlertTimeProperties(data, html);
            bodyDic["ctl00$PlaceHolderMain$ctl07$ctl01$RadioBtnAlertFreq"] = data["NotifyFreq"].ToString();
            if ((int)data["NotifyFreq"] == 1)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl07$ctl02$DdlHour"] = data["Time"].ToString();
            }
            else if ((int)data["NotifyFreq"] == 2)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl07$ctl02$DdlWeekDay"] = data["Day"].ToString();
                bodyDic["ctl00$PlaceHolderMain$ctl07$ctl02$DdlHour"] = data["Time"].ToString();
            }
            //string strToken_Me = "Type\\s*=\\s*\"(Integer)?\"[^>]*>[^>]*<UserID[^>]*/>";
            bodyDic["ctl00$PlaceHolderMain$ctl03$ctl00$userPicker$hiddenSpanData"] = HttpUtility.UrlEncode(AveHttpWebRequestUtility.GetPeoplePickerValue(data["User"] as Dictionary<string, object>));
            //bodyDic["ctl00$PlaceHolderMain$ctl03$ctl00$userPicker$downlevelTextBox"] = HttpUtility.UrlEncode(AveHttpWebRequestUtility.GetPicker());
            //bodyDic["ctl00$PlaceHolderMain$ctl03$ctl00$userPicker$OriginalEntities"] = HttpUtility.UrlEncode(AveHttpWebRequestUtility.GetOriginalPicker());
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            string contentType = "application/x-www-form-urlencoded";
            AveHttpWebRequestUtility.HttpPost(netWorkUrl, this.mObj, contentType, body, null,mTokenProvider);
            Dictionary<string, object> featureDefinitionProperties = new Dictionary<string, object>();
            featureProp["Definition" + AveObjectModelConstant.ObjectPropertySuffix] = featureDefinitionProperties;
            return featureProp;
        }
        //public Dictionary<string, object> AddGroup(string webRelativeUrl, string ownerName, string ownerType, string defaultUserName, string groupName, string description, string groupSource)
        //{
        //    Dictionary<string, object> groupProperties = new Dictionary<string, object>();
        //    string webAppName = GetWebAppNameFromSiteUrl(mWebUrl);
        //    string url = webAppName + webRelativeUrl.TrimEnd('/');
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj))
        //    {
        //        mNetWork.InitialNetWorker(AveWebServiceType.UserGroup, url);
        //        switch (groupSource)
        //        {
        //            case "web.siteGroups":
        //                if (string.IsNullOrEmpty(defaultUserName))
        //                {
        //                    if (string.Equals(ownerType, "user", StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        defaultUserName = ownerName;
        //                    }
        //                    else
        //                    {
        //                        defaultUserName = mNetWork.User.Domain + "\\" + mNetWork.User.UserName;
        //                    }
        //                }
        //                if (string.IsNullOrEmpty(ownerType))
        //                {
        //                    ownerType = "user";
        //                    ownerName = defaultUserName;
        //                }
        //                mNetWork.UserGroupAddGroup(groupName, ownerName, ownerType, defaultUserName, description);
        //                XmlElement group = mNetWork.UserGroupGetGroupInfo(groupName).FirstChild as XmlElement;
        //                groupProperties.Add("Id", Convert.ToInt32(group.GetAttribute("ID")));
        //                groupProperties.Add("Name", group.GetAttribute("Name"));
        //                groupProperties.Add("Description", group.GetAttribute("Description"));
        //                groupProperties.Add("OwnerId", Convert.ToInt32(group.GetAttribute("OwnerID")));
        //                groupProperties.Add("OwnerIsUser", Convert.ToBoolean(group.GetAttribute("OwnerIsUser")));
        //                break;
        //            case "web.groups":
        //                throw new Exception("You cannot add a group directly to the Groups collection.  You can add a group to the SiteGroups collection.");
        //        }
        //    }
        //    return groupProperties;
        //}

        public Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, Guid featureId, int webTemplateType)
        {
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj,mTokenProvider))
            {
                string webAppName = GetWebAppNameFromSiteUrl(mWebUrl);
                string url = webAppName + webServerRelativeUrl.TrimEnd('/');
                mNetWork.InitialNetWorker(AveWebServiceType.Lists, url);
                this.mNetWork.ListAddList(title, description, webTemplateType);
                XmlNode listInfo = mNetWork.ListGetList(title);
                Dictionary<string, object> newListProperties = new Dictionary<string, object>();
                XmlNodeToDicValue(newListProperties, listInfo);
                return newListProperties;
            }

        }
        public Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, string url, string featureId, int templateType, string docTemplateType, int quickLaunchOptions)
        {
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj,mTokenProvider))
            {
                string webAppName = GetWebAppNameFromSiteUrl(mWebUrl);
                string listsUrl = webAppName + webServerRelativeUrl.TrimEnd('/');
                mNetWork.InitialNetWorker(AveWebServiceType.Lists, listsUrl);
                XmlNode listInfo = mNetWork.ListAddListFromFeature(title, description, new Guid(featureId), templateType);
                //XmlNode listInfo = mNetWork.ListGetList(title);
                Dictionary<string, object> newListProperties = new Dictionary<string, object>();
                XmlNodeToDicValue(newListProperties, listInfo);
                return newListProperties;
            }
        }

        /// <summary>
        /// add list with listTemplate(support sharepoint10,sharepoint13/O365 can use IAveHttpWebRequestCommon.AddList(string webServerRelativeUrl, string title, string description, IAveListTemplate listTemplate))
        /// </summary>
        /// <param name="webServerRelativeUrl"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="listTemplate"></param>
        /// <returns>return null</returns>
        public Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, IAveListTemplate listTemplate)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/AddGallery.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl.ToString(), mObj,mTokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__REQUESTDIGEST"] = HttpUtility.UrlEncode(bodyDic["__REQUESTDIGEST"].ToString());
            }
            bodyDic["Title"] = title;
            bodyDic["Task"] = "CreateList";
            bodyDic["Description"] = description;
            bodyDic["ListTemplateFeatureId"] = listTemplate.FeatureId.ToString("B");
            bodyDic["ListTemplateType"] = listTemplate.Type_Client;
            bodyDic["CustomTemplate"] = listTemplate.InternalName;
            bodyDic["CurrentWeb"] = webServerRelativeUrl.TrimEnd('/');
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl.ToString(), mObj, "application/x-www-form-urlencoded", body, null,mTokenProvider);
            return null;
        }

        //public Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, string url, Dictionary<string, object> dataSource)
        //{
        //    throw new NotImplementedException();
        //}

        public Dictionary<string, object> AddAttachmentNow(string webRelativeUrl, string listName, Guid listId, int itemId, string leafName, byte[] attachment)
        {
            Dictionary<string, object> attachmentProperties = new Dictionary<string, object>();
            string webAppName = GetWebAppNameFromSiteUrl(mWebUrl);
            string url = webAppName + webRelativeUrl.TrimEnd('/');
            string relativeUrl = string.Empty;
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj,mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.Lists, url);
                relativeUrl = mNetWork.ListAddAttachment(listName, itemId.ToString(), leafName, attachment);

                relativeUrl = webRelativeUrl + "/" + relativeUrl;
                attachmentProperties.Add("FileName", leafName);
                attachmentProperties.Add("ServerRelativeUrl", relativeUrl);
                //Dictionary<string, object> fileProperties = GetFile(webRelativeUrl, relativeUrl, listName);
                //attachmentProperties.Add("ROWID", fileProperties["UniqueId"]);
                //attachmentProperties.Add("FileName", fileProperties["Name"]);
                //attachmentProperties.Add("ServerRelativeUrl", relativeUrl);
            }
            return attachmentProperties;
        }

        //public Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, byte[] file, bool overwrite, string checkInComment, bool checkRequiredFields)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> AddFolder(string webServerRelativeUrl, Guid listId, string folderServerRelativeUrl, string strUrl)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> AddView(string webServerRelativeUrl, string listTitle, Guid listId, string strViewName, StringCollection strCollViewFields, string strQuery, uint iRowLimit, bool bPaged, bool bMakeViewDefault, int type, bool bPersonalView)
        //{
        //    throw new NotImplementedException();
        //}
        //public void AddViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string field)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, string listName, Stream file, bool overwrite, string checkInComment, bool checkRequiredFields, bool? listEnableMinorVersion)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> AddFile(string webServerRelativeUrl, string folderServerRelativeUrl, string urlOfFile, int templateFileType)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> AddWeb(string parentWebRelativeUrl, string webUrl, string description, uint language, string title, bool useSamePermissionsAsParentSite, string webTemplate, bool bConvertIfThere)
        //{
        //    throw new NotImplementedException();
        //}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "Req")]
        public Dictionary<string, object> AddFeature(string webServerRelativeUrl, Guid featureId, bool force, int scope, string featuresSource)
        {
            Dictionary<string, object> featureProp = new Dictionary<string, object>();
            Dictionary<string, object> featureDefinitionProperties;
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            string postUrl = string.Empty;
            bool isFindFeatureButton = false;//Find active button from html.
            switch (featuresSource)
            {
                case "web.features":
                    postUrl = webFullUrl.TrimEnd('/') + "/_layouts/ManageFeatures.aspx";
                    break;
                case "site.features":
                    postUrl = webFullUrl.TrimEnd('/') + "/_layouts/ManageFeatures.aspx?Scope=Site";
                    break;
            }
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj,mTokenProvider);
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
                    return AddFeature(webServerRelativeUrl, featureId, force, scope, "site.features");
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
            string result = AveHttpWebRequestUtility.HttpReturn(postUrl, mObj, "application/x-www-form-urlencoded", body, null,string.Empty,mTokenProvider);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(result);
            foreach (XmlNode node in xmlDoc.SelectNodes("/html/body/h2/a"))
            {
                if (node.Attributes["href"].Value != null && node.Attributes["href"].Value.Contains("ReqFeatures.aspx?"))
                {
                    throw new Exception(WrapperClientResource.Wrapper_Client_AddFeatureFailed);
                }
            }
            featureProp["DefinitionId"] = featureId;
            featureDefinitionProperties = new Dictionary<string, object>();
            featureProp["Definition" + AveObjectModelConstant.ObjectPropertySuffix] = featureDefinitionProperties;
            return featureProp;
        }
        //public Dictionary<string, object> AddContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeSource, Dictionary<string, object> newContentTypeProperties)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> AddEventReceiverDefinition(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource, int receiverType, string assembly, string className, string name)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> AddNavigationNode(string webRelativeUrl, Dictionary<string, object> parentNodeProperties, Dictionary<string, object> newNodeProperties, string navigationSource)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> AddFieldAsXml(string webServerRelativeUrl, string listName, Guid listId, String fieldXml, bool addToDefaultView, int op, string fieldSource, Dictionary<string, object> contentTypeProp)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> GetTaxonomyCatchAllField(string webServerRelativeUrl, string listName, Guid listId)
        //{
        //    throw new NotImplementedException();
        //}
        public Dictionary<string, object> AddUser(string webServerRelativeUrl, string source, string groupName, Dictionary<string, object> userProp)
        {
            string userName = userProp["Name"] as string;
            string userLoginName = userProp["LoginName"] as string;
            string userEmail = userProp["Email"] as string;
            string userNotes = userProp["Notes"] as string;
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            XmlNode node = null;
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj,mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.UserGroup, webFullUrl);
                switch (source)
                {
                    case "group.users":
                        mNetWork.UserGroupAddUserToGroup(groupName, userName, userLoginName, userEmail, userNotes);
                        node = mNetWork.UserGroupGetUserInfo(userLoginName);
                        break;
                    case "web.allUsers":
                    case "web.users":
                    case "web.siteAdministrators":
                    case "web.siteUsers":
                        //Add user to an exist group, and remove it, the user already stays in siteusers collection
                        mNetWork.UserGroupAddUserToGroup(groupName, userName, userLoginName, userEmail, userNotes);
                        mNetWork.UserGroupRemoveUserFromGroup(groupName, userLoginName);
                        node = mNetWork.UserGroupGetUserInfo(userLoginName);
                        break;
                    default:
                        break;
                }
            }
            return this.GetUserDic(node);
        }

        public Dictionary<string, object> AddUserProfile(string accountName)
        {
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj,mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.UserProfile, mWebUrl);
                UserProfileService.PropertyData[] userProfileProperties = null;
                try
                {
                    userProfileProperties = mNetWork.UserProfileGetUserProfile(accountName);
                }
                catch (Exception e)//that means up is not exists
                {
                    mLogger.Info("there is no user profile {0}", e.ToString());
                    userProfileProperties = mNetWork.UserProfileCreateUserProfile(accountName);
                }
                return AssemblyUserProfileProperties(userProfileProperties);
            }
        }

        public Dictionary<string, object> AssemblyUserProfileProperties(UserProfileService.PropertyData[] userProfileProperties)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            for (int i = 0; i < userProfileProperties.Length; i++)
            {
                List<object> valueList = new List<object>();
                UserProfileService.PropertyData propertyData = userProfileProperties[i];
                UserProfileService.ValueData[] valueArray = propertyData.Values;
                foreach (UserProfileService.ValueData valueData in valueArray)
                {
                    valueList.Add(valueData.Value.ToString());
                }
                properties[propertyData.Name] = valueList;
            }
            return properties;
        }

        //public void AddPersonalSite(string accountName, int lcid)
        //{
        //    throw new NotImplementedException();
        //}

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "diid,onetid:Special characters of solution's field xml.")]
        public static void AddSlideFolder(string webApp, string webServerRelativeUrl, string listTitle, string parentFolderServerRelativeUrl, string folderName, object obj)
        {
            AddSlideFolder(webApp, webServerRelativeUrl, listTitle, parentFolderServerRelativeUrl, folderName, obj, null);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "diid,onetid:Special characters of solution's field xml.")]
        public static void AddSlideFolder(string webApp, string webServerRelativeUrl, string listTitle, string parentFolderServerRelativeUrl, string folderName, object obj, ITokenProvider tokenProvider)
        {
            string listUrl = webServerRelativeUrl.TrimEnd('/') + "/" + listTitle;
            string url = webApp + listUrl + "/Forms/Upload.aspx?RootFolder=" + HttpUtility.UrlEncode(parentFolderServerRelativeUrl) + "&Type=1&IsDlg=1";
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            string contentType = "multipart/form-data; boundary=" + boundary;
            string html = AveHttpWebRequestUtility.HttpGet(url, obj);
            Dictionary<string, object> inputDic = new Dictionary<string, object>();
            Dictionary<string, object> buttonDic = new Dictionary<string, object>();
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", inputDic);
            AveHttpWebRequestUtility.GetInput(html, "<input name=", inputDic);
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"button\"", buttonDic);
            foreach (string key in buttonDic.Keys)
            {
                if (key.EndsWith("diidIOSaveItem", StringComparison.OrdinalIgnoreCase))
                {
                    inputDic["__EVENTTARGET"] = key;
                    break;
                }
            }
            foreach (string key in inputDic.Keys)
            {
                if (key.Contains("onetidIOFile"))
                {
                    inputDic[key] = folderName;
                    break;
                }
            }
            byte[] body = AveHttpWebRequestUtility.GetMiltiByte(inputDic, boundary);
            AveHttpWebRequestUtility.HttpPost(url, obj, contentType, body, null);
        }

        //365已重写
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "%5b and %3b are Url Codings.")]
        public static void SaveSpecialBinary(string webAppName, string webServerRelativeUrl, object obj, string fileUrl, Stream fileStream, string serverVersion)
        {
            string url = webAppName + webServerRelativeUrl.TrimEnd('/') + "/_vti_bin/_vti_aut/author.dll";
            string text = new StreamReader(fileStream).ReadToEnd();
            if (fileUrl.StartsWith(webServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                fileUrl = fileUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1);
            }
            string order = "method=put+document%3a" //+ serverVersion.Replace(".", "%2e")
                + "&service%5fname=" + System.Web.HttpUtility.UrlEncode(webServerRelativeUrl)
                + "&document=%5bdocument%5fname%3d" + System.Web.HttpUtility.UrlEncode(fileUrl) + "%3bmeta%5finfo%3d%5bvti%5fmodifiedby%3bSW%7cSHAREPOINT%5c%5csystem%3bvti%5fauthor%3bSW%7cSHAREPOINT%5c%5csystem%5d%5d"
                + "&put%5foption=overwrite" + "&comment=" + "&keep%5fchecked%5fout=false";
            order = order + "\n" + text;
            byte[] body = AveHttpWebRequestUtility.GetByte(null, order);
            string contentType = "application/x-vermeer-urlencoded";
            Dictionary<string, object> headerInformation = new Dictionary<string, object>();
            headerInformation["X-Vermeer-Content-Type"] = "application/x-vermeer-urlencoded";
            AveHttpWebRequestUtility.HttpPost(url, obj, contentType, body, headerInformation);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ctl00$PlaceHolderMain$ctl00$RptControls$btnOK is a key")]
        //public static void AddDocumentSet(string folderName, string webUrl, string parentFolderServerRelativeUrl, string listId, string contentTypeId, object obj)
        //{
        //    string newUrl = webUrl.TrimEnd('/') + "/_layouts/NewDocSet.aspx?";
        //    string url = string.Format("{0}List={1}&RootFolder={2}&ContentTypeId={3}&IsDlg=1", newUrl, listId, parentFolderServerRelativeUrl, contentTypeId);
        //    string html = AveHttpWebRequestUtility.HttpGet(url, obj);
        //    Dictionary<string, object> bodyDic = new Dictionary<string, object>();
        //    AveHttpWebRequestUtility.GetInput(html, "<input type=\"hidden\"", bodyDic);
        //    if (bodyDic.ContainsKey("__EVENTVALIDATION"))
        //    {
        //        bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
        //    }
        //    if (bodyDic.ContainsKey("__VIEWSTATE"))
        //    {
        //        bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
        //    }
        //    bodyDic["ctl00$PlaceHolderMain$idDocSetDisplayFormWebPart$ctl00$ctl01$ctl00$ctl00$ctl00$ctl04$ctl00$ctl00$onetidIOFile"] = folderName;
        //    //bodyDic["ctl00$PlaceHolderMain$idDocSetDisplayFormWebPart$ctl00$ctl02$ctl00$ctl00$ctl00$ctl04$ctl00$ctl00$TextField"] = "dfasdxcfz";
        //    bodyDic["ctl00$PlaceHolderMain$ctl00$RptControls$btnOK"] = "OK";
        //    byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
        //    AveHttpWebRequestUtility.HttpPost(url, obj, "application/x-www-form-urlencoded", data, null);
        //}

        public void AddViewToAllNodes(string webServerRelativeUrl, Guid listId, Guid viewId)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/MetaNavPerNode.aspx?List=" + listId.ToString("B");
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj,mTokenProvider);
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
            bodyDic["__EVENTTARGET"] = "AddToAllLocations";
            bodyDic["__EVENTARGUMENT"] = viewId;
            XmlDocument xmlDoc = new XmlDocument();
            searchContent = "<input id=\"ctl00_PlaceHolderMain_ctl01_Picker_data\"";
            string information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</input>");
            xmlDoc.LoadXml(information);
            bodyDic["ctl00$PlaceHolderMain$ctl01$Picker$data"] = xmlDoc.FirstChild.Attributes["value"].Value;
            searchContent = "<input id=\"ctl00_PlaceHolderMain_ctl01_Picker_initial\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</input>");
            xmlDoc.LoadXml(information);
            bodyDic["ctl00$PlaceHolderMain$ctl01$Picker$initial"] = xmlDoc.FirstChild.Attributes["value"].Value;
            bodyDic["ctl00$PlaceHolderMain$ctl01$Picker"] = xmlDoc.FirstChild.Attributes["value"].Value;
            bodyDic["ctl00$PlaceHolderMain$ctl01$SelectResult"] = viewId;
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
        }

        public Dictionary<string, object> AddKeyWord(string term, DateTime startDate, int localId, int calendarType)
        {
            string url = mWebUrl.TrimEnd('/') + "/_layouts/Keyword.aspx";
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, mObj,mTokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(url, mObj, "application/x-www-form-urlencoded", data, null);
            Dictionary<string, object> keyWordProp = new Dictionary<string, object>();
            try
            {
                keyWordProp = this.GetKeyWordProperties(term);
            }
            catch (Exception ex)
            {
                mLogger.Error("Add KeyWord:{0} Failed.Error Message:{1}.", term, ex.ToString());
                throw new Exception("Add keyword failed.");
            }
            return keyWordProp;
        }

        //public void AddSitePolicy(string policySchema, string siteUrl)
        //{
        //    throw new NotImplementedException();
        //}

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ctl00$PlaceHolderMain$synTextBox is a key")]
        public string AddSynonm(string term, string synTerm, string terms)
        {
            string url = mWebUrl.TrimEnd('/') + string.Format("/_layouts/Keyword.aspx?k={0}", term);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, mObj,mTokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(url, mObj, "application/x-www-form-urlencoded", data, null,mTokenProvider);
            return synTerm;
        }

        public Dictionary<string, object> AddBestBet(string term, List<string> bestBetUrlList, Dictionary<string, object> bestBetProp, string action)
        {
            string url = mWebUrl.TrimEnd('/') + string.Format("/_layouts/Keyword.aspx?k={0}", term);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, mObj,mTokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(url, mObj, "application/x-www-form-urlencoded", data, null,mTokenProvider);

            return bestBetProp;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")]
        public string AddBestBet(string term, string bestBetTitle, string bestBetUrl, string bestBetDescription)
        {
            string url = mWebUrl.TrimEnd('/') + string.Format("/_layouts/BestBet.aspx?k={0}&IsDlg=1", term);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, mObj,mTokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(url, mObj, "application/x-www-form-urlencoded", data, null,mTokenProvider);
            string bestBet = string.Format("{0};{1};{2}", bestBetUrl, bestBetTitle, bestBetDescription);
            return bestBet;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "lstBestBets is a key")]
        public string AddExistBestBet(string term, string bestBetUrl)
        {
            string url = mWebUrl.TrimEnd('/') + string.Format("/_layouts/BestBet.aspx?k={0}&IsDlg=1", term);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, mObj, mTokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(url, mObj, "application/x-www-form-urlencoded", data, null, mTokenProvider);
            string bestBet = string.Format("{0};;;", bestBetUrl);
            return bestBet;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")]
        public string EditBestBet(string term, string bestBetTitle, string bestBetUrl, string bestBetDescription)
        {
            string a = string.Format("{0};;;", bestBetUrl);
            string url = mWebUrl.TrimEnd('/') + "/_layouts/BestBet.aspx?";
            string postUrl = string.Format("{0}u={1}&k={2}&a={3}&IsDlg=1", url, bestBetUrl, term, a);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", data, null, mTokenProvider);
            return a;
        }

        public void AddTag(string url, Guid termId, string title, bool? isPrivate)
        {
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.SocialDataService, mWebUrl);
                mNetWork.AddTag(url, termId, title, isPrivate);
            }
        }
        public void AddComment(string url, string comment, bool? isHighPriority, string title)
        {
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.SocialDataService, mWebUrl);
                mNetWork.AddComment(url, comment, isHighPriority, title);
            }
        }

        #endregion

        public string AssociateWorkflowMarkup(string webServerRelativeUrl, string configUrl, string configVersion)
        {
            Uri siteUri = new Uri(mWebUrl);
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.WebPartPages, webFullUrl);
                return mNetWork.AssociateWorkflowMarkup(configUrl, configVersion);
            }
        }

        public void BrowserEnableUserFormTemplate(string formTemplateUrl)
        {
            Uri siteUri = new Uri(mWebUrl);
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.FormsServices, siteUri.AbsoluteUri.TrimEnd('/'));
                mNetWork.BrowserEnableUserFormTemplate(formTemplateUrl);
            }
        }

        //public Dictionary<string, object> CreateListAssociation(string webServerRelativeUrl, Guid hostListId, string workflowTemplateSource, IAveWorkflowAssociation asso)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> CreateWebAssociation(string webServerRelativeUrl, Guid webId, string workflowTemplateSource, IAveWorkflowAssociation asso)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> CreateListContentTypeAssociation(string webServerRelativeUrl, Guid hostListId, IAveContentTypeId ctId, string workflowTemplateSource, IAveWorkflowAssociation asso)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> CreatWebContentTypeAssociation(string webServerRelativeUrl, IAveContentTypeId ctId, string workflowTemplateSource, IAveWorkflowAssociation asso)
        //{
        //    throw new NotImplementedException();
        //}

        //#region Delete
        //public void DeleteList(string webServerRelativeUrl, string listName, Guid listId)
        //{
        //    throw new NotImplementedException();
        //}
        //public void DeleteView(string webServerRelativeUrl, string listName, Guid listId, Guid viewId)
        //{
        //    throw new NotImplementedException();
        //}
        //public void DeleteRecycleItem(Guid id, string webServerRelativeUrl = null)
        //{
        //    throw new NotImplementedException();
        //}
        //public void DeleteFileVersions(string webServerRelativeUrl, string fileServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}
        //public void DeleteFileVersion(string webServerRelativeUrl, string fileServerRelativeUrl, int id)
        //{
        //    throw new NotImplementedException();
        //}
        public void DeleteFileVersion(string fileServerRelativeUrl, string webServerRelativeUrl, string versionLabel)
        {
            string webAppName = GetWebAppNameFromSiteUrl(mWebUrl);
            string url = webAppName + webServerRelativeUrl.TrimEnd('/');
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.Versions, url);
                mNetWork.FileDeleteVersion(fileServerRelativeUrl, versionLabel);
            }
        }
        //public void DeleteFolder(string webServerRelativeUrl, string folderServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}
        //public void DeleteWeb(string webServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}

        //public void DeleteItem(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId)
        //{
        //    throw new NotImplementedException();
        //}
        public static void DeleteItems(string webUrl, string listName, object obj, List<int> ids)
        {
            DeleteItems(webUrl, listName, obj, null, ids);
        }
        public static void DeleteItems(string webUrl, string listName, object obj, ITokenProvider tokenProvider, List<int> ids)
        {
            Lists.Lists listService = new Lists.Lists();
            listService.Url = webUrl + "/_vti_bin/Lists.asmx";
            if (obj != null)
            {
                NetworkCredential credential = obj as NetworkCredential;
                if (credential != null)
                {
                    listService.Credentials = credential;
                }
                else
                {
                    listService.CookieContainer = obj as CookieContainer;
                }
            }
            else if(tokenProvider != null)
            {
                listService.TokenProvider = tokenProvider;
            }
            else
            {
                throw new Exception("No available credentials.");
            }
            listService.Timeout = WrapperConfiguration.UpLoadFileStreamTimeout * 1000;
            XmlDocument doc = new XmlDocument();
            StringBuilder updateData = new StringBuilder();
            updateData.Append("<Batch OnError='Continue'>");
            for (int i = 0; i < ids.Count; i++)
            {
                updateData.Append(string.Format("<Method ID='{0}' Cmd='Delete'><Field Name='ID'>{1}</Field></Method>", i + 1, ids[i]));
            }
            updateData.Append("</Batch>");
            doc.LoadXml(updateData.ToString());
            listService.UpdateListItems(listName, doc.DocumentElement);
        }

        //public void DeleteItemVersion(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, int versionId)
        //{

        //}
        //public void DeleteRoleAssignment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, int principalId, string source)
        //{
        //    throw new NotImplementedException();
        //}
        //public void DeleteRoleDefinition(string webServerRelativeUrl, string roleDefintionName)
        //{
        //    throw new NotImplementedException();
        //}
        public void DeleteAttachmentNow(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, int itemId, string leafName)
        {
            string webAppName = GetWebAppNameFromSiteUrl(mWebUrl);
            string url = webAppName + webServerRelativeUrl.TrimEnd('/');
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj,mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.Lists, url);
                string attachmentUrl = url + "/" + listServerRelativeUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length + 1).TrimEnd('/') + "/Attachments/" + itemId + "/" + leafName;
                mNetWork.ListDeleteAttachment(listTitle, itemId.ToString(), attachmentUrl);
            }
            //throw new NotImplementedException();
        }
        //public void DeleteNavigationNode(string webServerRelativeUrl, Dictionary<string, object> parentNodeProperties, Dictionary<string, object> deleteNodeProperties)
        //{
        //    throw new NotImplementedException();
        //}
        //public void DeleteGroup(string webServerRelativeUrl, int id)
        //{
        //    throw new NotImplementedException();
        //}
        //public void DeleteViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string fieldName)
        //{
        //    throw new NotImplementedException();
        //}
        //public void DeleteAllViewField(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId)
        //{
        //    throw new NotImplementedException();
        //}
        //public void DeleteFile(string webServerRelativeUrl, string fileServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}
        //public void DeleteEventReceiverDefinition(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource, Guid eventReceiverDefId)
        //{
        //    throw new NotImplementedException();
        //}
        //public void DeleteFeature(string webServerRelativeUrl, Guid featureId, bool force, string featureSource)
        //{
        //    throw new NotImplementedException();
        //}
        //public void DeleteField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, Dictionary<string, object> contentTypeProp)
        //{
        //    throw new NotImplementedException();
        //}
        //public void DeleteUserSolution(Guid solutionId)
        //{
        //    throw new NotImplementedException();
        //}
        public void DeleteUser(string webServerRelativeUrl, string source, string groupName, string loginName)
        {
            string webFullUrl = mWebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.UserGroup, webFullUrl);
                switch (source)
                {
                    case "group.users":
                        mNetWork.UserGroupRemoveUserFromGroup(groupName, loginName);
                        break;
                    case "web.allUsers":
                    case "web.users":
                    case "web.siteAdministrators":
                    case "web.siteUsers":
                        mNetWork.UserGroupRemoveUserFromSite(loginName);
                        break;
                    default:
                        break;
                }
            }
        }
        //public void RemoveThemeFromWeb(string webServerRelativeUrl, bool deleteFiles)
        //{
        //}
        public void DeleteTag(string url, Guid termId)
        {
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.SocialDataService, mWebUrl);
                mNetWork.DeleteTag(url, termId);
            }
        }


        #region Restore
        //public void RestoreRecycleItem(Guid id, string webServerRelativeUrl = null)
        //{
        //    throw new NotImplementedException();
        //}
        public void RestoreFileVersion(string versionLabel, string webServerRelativeUrl, string fileServerRelativeUrl)
        {
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.Versions, webFullUrl);
                mNetWork.FileRestoreVersion(fileServerRelativeUrl, versionLabel);
            }
        }
        //public void RestoreWebParts(string webServerRelativeUrl, string listTitle, Guid listId, string fileServerRelativeUrl, int scope, IList webpartBaseInfoList, AveWebPartCache mapping, bool clearAll, IAveWeb web, IReport report)
        //{
        //    string webFullUrl = this.WebAppName.TrimEnd('/') + '/' + webServerRelativeUrl;
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
        //    {
        //        mNetWork.InitialNetWorker(AveWebServiceType.WebPartPages, webFullUrl);
        //        mNetWork.DeleteAllWebPart(fileServerRelativeUrl);
        //        foreach (AveWebPartBaseInfo webpartBaseInfo in webpartBaseInfoList)
        //        {
        //            mNetWork.AddWebPart(fileServerRelativeUrl, webpartBaseInfo.DefinitionXml, webpartBaseInfo.ZoneID, webpartBaseInfo.PartOrder);
        //        }
        //    }
        //}

        //public Dictionary<string, object> RestoreListItem(Dictionary<string, object> data, Dictionary<string, object> userData)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> RestoreFolder(Dictionary<string, object> data, Dictionary<string, object> userData)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> RestoreDocument(AveDocumentInfo info, Stream fileStream, IReport report)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> RestoreAttachment(Dictionary<string, object> data, Dictionary<string, object> userData, Stream fileStream)
        //{
        //    throw new NotImplementedException();
        //}

        public List<Dictionary<string, object>> RestoreFeatures(string webServerRelativeUrl, bool force, int scope, string featuresSource, List<Dictionary<string, object>> featureInfoList)
        {
            List<Dictionary<string, object>> featuresProperties = new List<Dictionary<string, object>>();
            foreach (Dictionary<string, object> featureInfo in featureInfoList)
            {
                try
                {
                    foreach (Guid id in featureInfo["Dependences"] as List<Guid>)
                    {
                        AddFeature(webServerRelativeUrl, id, force, scope, featuresSource);
                    }
                    Dictionary<string, object> featureProp = new Dictionary<string, object>();
                    Guid featureId = new Guid(featureInfo["ID"].ToString());
                    featureProp = AddFeature(webServerRelativeUrl, featureId, force, scope, featuresSource);
                    featuresProperties.Add(featureProp);
                }
                catch (AveSecurityTrimingException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    mLogger.Error("Add feature to {0}:{1} failed.Error Message:{2}.", featuresSource, webServerRelativeUrl, ex.ToString());
                }
            }
            return featuresProperties;
        }

        public bool RestoreNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/AreaNavigationSettings.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            bool hasEffectValue = false;
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            //继承的时候不应该还原，否则会覆盖掉继承的
            bool quickLaunchNaviShare = webAllProperties["__InheritCurrentNavigation"] != null && Convert.ToBoolean(webAllProperties["__InheritCurrentNavigation"]);
            bool topNaviShare = webAllProperties.ContainsKey("UseShared") && Convert.ToBoolean(webAllProperties["UseShared"]);
            if (!(quickLaunchNaviShare && topNaviShare))
            {
                nodes = HttpUtility.UrlEncode(nodes);
                if (!bodyDic.ContainsKey("nodes"))
                {
                    bodyDic["nodes"] = nodes;
                }
                else if (bodyDic.ContainsKey("nodes") && (!bodyDic["nodes"].ToString().Equals(nodes)))
                {
                    bodyDic["nodes"] = nodes;
                    bodyDic["ctl00%24PlaceHolderMain%24ctl06%24RptControls%24bottomOKButton"] = "OK";
                }
            }
            if (webAllProperties.ContainsKey("__GlobalNavigationIncludeTypes"))
            {
                if (webAllProperties["__GlobalNavigationIncludeTypes"].ToString().Equals("1"))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24globalNavSection%24ctl01%24globalIncludeSubSites"] = "on";
                }
                else if (webAllProperties["__GlobalNavigationIncludeTypes"].ToString().Equals("2"))
                {
                    bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl01$globalIncludePages"] = "on";
                }
                else if (webAllProperties["__GlobalNavigationIncludeTypes"].ToString().Equals("3"))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24globalNavSection%24ctl01%24globalIncludeSubSites"] = "on";
                    bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl01$globalIncludePages"] = "on";
                }
                hasEffectValue = true;
            }
            if (webAllProperties.ContainsKey("__CurrentNavigationIncludeTypes"))
            {
                if (webAllProperties["__CurrentNavigationIncludeTypes"].ToString().Equals("1"))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24currentNavSection%24ctl01%24currentIncludeSubSites"] = "on";
                }
                else if (webAllProperties["__CurrentNavigationIncludeTypes"].ToString().Equals("2"))
                {
                    bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl01$currentIncludePages"] = "on";
                }
                else if (webAllProperties["__CurrentNavigationIncludeTypes"].ToString().Equals("3"))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24currentNavSection%24ctl01%24currentIncludeSubSites"] = "on";
                    bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl01$currentIncludePages"] = "on";
                }
                hasEffectValue = true;
            }
            if (webAllProperties.ContainsKey("__GlobalDynamicChildLimit"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24globalNavSection%24ctl01%24globalDynamicChildLimit"] = webAllProperties["__GlobalDynamicChildLimit"];
            }
            if (webAllProperties.ContainsKey("__CurrentDynamicChildLimit"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24currentNavSection%24ctl01%24currentDynamicChildLimit"] = webAllProperties["__CurrentDynamicChildLimit"];
            }
            if (webAllProperties.ContainsKey("__NavigationOrderingMethod"))
            {
                string method = webAllProperties["__NavigationOrderingMethod"].ToString();
                if (method.Equals("0"))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl01%24SortingMethodRadioGroup"] = "automaticSortingRadioButton";
                }
                else if (method.Equals("1"))
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl01$SortingMethodRadioGroup"] = "manualSortingRadioButton";
                    bodyDic["ctl00$PlaceHolderMain$ctl01$automaticPageSortingCheckBox"] = "on";
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
                        method = webAllProperties["__NavigationAutomaticSortingMethod"].ToString();
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
                else if (method.Equals("2"))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl01%24SortingMethodRadioGroup"] = "manualSortingRadioButton";
                }
            }
            if (webAllProperties.ContainsKey("__DisplayShowHideRibbonActionId"))
            {
                bool ribbon = Convert.ToBoolean(webAllProperties["__DisplayShowHideRibbonActionId"]);
                if (ribbon)
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl03$DisplayShowHideRibbonActionMethodRadioGroup"] = "displayShowHideRibbonActionRadioButtonOptionYes";
                }
                else
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl03$DisplayShowHideRibbonActionMethodRadioGroup"] = "displayShowHideRibbonActionRadioButtonOptionNo";
                }
            }
            else if (hasEffectValue)
            {
                //webservice更新navigation setting属性的时候如果该属性没有值，会还原成false，这里需要使用sharepoint默认属性赋值，确保保持一致性
                bodyDic["ctl00$PlaceHolderMain$ctl03$DisplayShowHideRibbonActionMethodRadioGroup"] = "displayShowHideRibbonActionRadioButtonOptionYes";
            }
            if (webAllProperties.ContainsKey("UseShared"))
            {
                bodyDic["ctl00$PlaceHolderMain$globalNavSection$ctl01$TopNavInheritance"] = "inheritTopNavRadioButton";
            }
            if (webAllProperties.ContainsKey("__NavigationShowSiblings"))
            {
                bool showSiblings = Convert.ToBoolean(webAllProperties["__NavigationShowSiblings"]);
                if (showSiblings)
                {
                    bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl01$LeftNavInheritance"] = "showSiblingsLeftNavRadioButton";
                }
            }
            if (webAllProperties.ContainsKey("__InheritCurrentNavigation"))
            {
                bool inheritCurrentNavigation = Convert.ToBoolean(webAllProperties["__InheritCurrentNavigation"]);
                if (inheritCurrentNavigation)
                {
                    bodyDic["ctl00$PlaceHolderMain$currentNavSection$ctl01$LeftNavInheritance"] = "inheritLeftNavRadioButton";
                }
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
            return true;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used in web request")]
        public void RestoreTheme(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebSettingInfo webSettingInfo, string themedCssFolderUrl)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/themeweb.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
            AveWebThemeInfo theme = webSettingInfo.WebTheme.Value;
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl02%24RptControls%24Submit1";
            if (theme.InheritsThemedCssFolderUrl)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl06$inheritThemeSection$inheritThemeGroup"] = "inheritTheme";    //inherit属性改为使用post方式来还
                //InputColor(bodyDic, theme);
            }
            else
            {
                if (string.IsNullOrEmpty(theme.ThemeName))
                {
                    bodyDic["ctl00$PlaceHolderMain$thmxThemes"] = string.Empty;
                }
                else if (theme.ThemeName.Equals("Custom"))
                {
                    bodyDic["ctl00$PlaceHolderMain$thmxThemes"] = webServerRelativeUrl.TrimEnd('/') + "/_themes/Custom.thmx";
                    bodyDic["ctl00$PlaceHolderMain$ctl82$customizeThemeSection$customThemeDirty"] = true;
                    InputColor(bodyDic, theme);
                }
                else if (string.IsNullOrEmpty(siteServerRelativeUrl))
                {
                    bodyDic["ctl00$PlaceHolderMain$thmxThemes"] = webServerRelativeUrl.TrimEnd('/') + "/_catalogs/theme/" + theme.ThemeName + ".thmx";
                }
                else
                {
                    bodyDic["ctl00$PlaceHolderMain$thmxThemes"] = siteServerRelativeUrl.TrimEnd('/') + "/_catalogs/theme/" + theme.ThemeName + ".thmx";
                }
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
        }

        public void RestoreMasterPage(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebMasterPageInfo pageInfo, string alternateCssUrl)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_Layouts/ChangeSiteMasterPage.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl00$InheritSiteMasterRadioGroup"] = "inheritChromeRadioButton";

            }
            else if (!string.IsNullOrEmpty(pageInfo.CPageUrl))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl00$InheritSiteMasterRadioGroup"] = "selectChromeRadioButton";
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl00$masterPageSelectionControl$ctl00$SiteMasterPageDropDownList"] = pageInfo.CPageUrl;
            }
            if (pageInfo.MInheriting)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl01$ctl00$InheritSystemMasterPageGroup"] = "inheritSystemMasterPageRadioButton";
            }
            else if (!string.IsNullOrEmpty(pageInfo.MPageUrl))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl01$ctl00$InheritSystemMasterPageGroup"] = "selectSystemMasterPageRadioButton";
                bodyDic["ctl00$PlaceHolderMain$ctl01$ctl00$systemMasterPageSelectionControl$ctl00$SystemMasterPageDropDownList"] = pageInfo.MPageUrl;
            }
            if (pageInfo.Inheriting)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl02$ctl00$InheritAlternateCssGroup"] = "inheritAlternateCssRadioButton";
            }
            else if (!string.IsNullOrEmpty(alternateCssUrl))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl02$ctl00$InheritAlternateCssGroup"] = "selectAlternateCssRadioButton";
                bodyDic["ctl00$PlaceHolderMain$ctl02$ctl00$alternateCssSelector$AssetUrlInput"] = alternateCssUrl;
            }
            else
            {
                bodyDic["ctl00$PlaceHolderMain$ctl02$ctl00$InheritAlternateCssGroup"] = "useWssCssRadioButton";
            }
            bodyDic["ctl00$PlaceHolderMain$ctl03$RptControls$ButtonSaveSettings"] = "OK";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "diid,Tbl:Special characters of solution's field xml.")]
        public Dictionary<string, object> OperateSolution(string operation, string siteUrl, string webServerRelativeUrl, int id)
        {
            string url = siteUrl.TrimEnd('/') + "/_catalogs/solutions/Forms/Activate.aspx?" + "Op=" + operation + "&ID=" + id.ToString()
                + "&Source=" + siteUrl.TrimEnd('/') + "/_catalogs/solutions/Forms/AllItems.aspx"
                + "&RootFolder=" + webServerRelativeUrl.TrimEnd('/') + "/_catalogs/solutions" + "&IsDlg=1";
            string html = AveHttpWebRequestUtility.HttpGet(url, mObj, mTokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string searchContent = "<input type=\"hidden\"";
            AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
            Dictionary<string, object> buttonDic = new Dictionary<string, object>();
            AveHttpWebRequestUtility.GetInput(html, "<input type=\"button\"", buttonDic);
            foreach (string key in buttonDic.Keys)
            {
                if (key.EndsWith("diidIOGoBack", StringComparison.OrdinalIgnoreCase))
                {
                    int index = key.IndexOf("ctl00", 2, StringComparison.OrdinalIgnoreCase);
                    string target = string.Empty;
                    if (operation.Equals("ACT"))
                    {
                        target = key.Substring(0, index) + "ctl00$ctl00$ctl00$toolBarTbl$RptControls$diidIOActivateSolutionItem";
                    }
                    else if (operation.Equals("DEA"))
                    {
                        target = key.Substring(0, index) + "ctl00$ctl00$ctl00$toolBarTbl$RptControls$diidIODeactivateSolutionItem";
                    }
                    bodyDic["&__EVENTTARGET"] = System.Web.HttpUtility.UrlEncode(target);
                    break;
                }
            }
            if (bodyDic.ContainsKey("__EVENTVALIDATION"))
            {
                bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
            }
            if (bodyDic.ContainsKey("__VIEWSTATE"))
            {
                bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
            }
            bodyDic["&ctl00%24PlaceHolderSearchArea%24ctl01%24ctl00"] = siteUrl;
            bodyDic["&ctl00%24PlaceHolderSearchArea%24ctl01%24ctl01"] = siteUrl.TrimEnd('/') + "/_catalogs/solutions";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            string contentType = "application/x-www-form-urlencoded";
            AveHttpWebRequestUtility.HttpPost(url, mObj, contentType, body, null, mTokenProvider);
            return null;
        }

        //public Dictionary<string, object> RestoreUserProfileProperties(Dictionary<string, object> userProfilePropertiesInfo, bool isOverWrite)
        //{
        //    return null;
        //}

        public Dictionary<string, object> RestoreUserProfileInfo(Dictionary<string, object> userProfileInfo, bool isOnlineSite, bool isExistSkip)
        {

            string realSiteUrl = mWebUrl;
            //TODO:APPTOKEN
            object cookieContainer =  mObj;
            if (cookieContainer == null && mTokenProvider == null)
            {
                return new Dictionary<string, object>();
            }

            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, realSiteUrl, cookieContainer, mTokenProvider))
            {
                Dictionary<string, object> result = new Dictionary<string, object>();
                string loginName = userProfileInfo["LoginName"] as string;
                mNetWork.InitialNetWorker(AveWebServiceType.UserProfile, realSiteUrl);
                try
                {
                    //UserProfileService.PropertyData[] profileData = GetUserProfile(mNetWork, loginName);
                    UserProfileService.PropertyData[] userProfileProperties = FindOrCreateUserProfile(mNetWork, loginName);
                    UpdateUserProfileProperties(mNetWork, loginName, userProfileInfo, userProfileProperties);
                    RestoreUserProfileLinks(mNetWork, userProfileInfo, loginName);
                    RestoreUserProfileColleagues(mNetWork, userProfileInfo, loginName);
                    RestoreUserProfileMemberShips(mNetWork, userProfileInfo, loginName);
                }
                catch (Exception e)
                {
                    mLogger.Error("restore UserProfileInfo error for the reason: {0}", e.ToString());
                    //result["Exception"] = e.ToString();
                }
                return result;
            }
        }

        public UserProfileService.PropertyData[] GetUserProfile(string loginName)
        {
            try
            {
                using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
                {
                    mNetWork.InitialNetWorker(AveWebServiceType.UserProfile, mWebUrl);
                    return mNetWork.UserProfileGetUserProfile(loginName);
                }
            }
            catch (Exception e)
            {
                mLogger.Info(string.Format("Can't find user profile, LoginName:{0}, Message {1}", loginName, e.ToString()));
                return null;
            }
        }


        public CookieContainer ChangeSiteCollectionToken(string userProfileLoginName, ref string centralAdminSiteUrl)
        {
            if (!(userProfileLoginName.ToLowerInvariant()).Contains(mAccountInfo.UserName.ToLowerInvariant()))
            {
                centralAdminSiteUrl = AveUrlUtility.GetTenantAdminSiteUrl(mWebUrl);
                //If user profile is current login user,There's no need to change the token.
                try
                {
                    if (!string.IsNullOrEmpty(centralAdminSiteUrl) && mCentralAdminObj == null)
                    {
                        SPOnlineAuthentication authentication = new SPOnlineAuthentication(centralAdminSiteUrl);
                        mCentralAdminObj = authentication.Login(mAccountInfo.UserName, mAccountInfo.Password);
                    }
                    return mCentralAdminObj as CookieContainer;
                }
                catch (Exception e)
                {
                    mLogger.Error("Get  Admin Site Authentication Cookie Error: {0}", e.ToString());
                    return null;
                }
            }
            return mObj as CookieContainer;
        }

        /// <summary>
        /// Maybe not accurate, need optimized later
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <returns></returns>
        public string GetOnlineAdminSite(string siteUrl)
        {
            if (string.IsNullOrEmpty(siteUrl))
            {
                return null;
            }
            Uri uri = new Uri(siteUrl);
            string hostName = uri.Host;
            hostName = hostName.Insert(hostName.IndexOf('.'), "-admin");
            return string.Format("{0}://{1}", uri.Scheme, hostName);
        }

        public void RestoreUserProfileMemberShips(AveWebServiceNetWork mNetWork, Dictionary<string, object> userProfileInfo, string loginName)
        {
            List<Dictionary<string, object>> membershipsList = userProfileInfo["Memberships"] as List<Dictionary<string, object>>;
            foreach (Dictionary<string, object> membershipData in membershipsList)
            {
                UserProfileService.MembershipData existMemberShip = null;
                string title = membershipData["Title"] as string;
                foreach (UserProfileService.MembershipData mData in mNetWork.UserProfileGetUserMemberShips(loginName))
                {
                    if (mData.DisplayName.Equals(title))
                    {
                        existMemberShip = mData;
                        break;
                    }
                }
                string group = membershipData["Group"] as string;
                int privacyLevel = (int)Enum.Parse(typeof(UserProfileService.Privacy), ((AvePrivacy)membershipData["PrivacyLevel"]).ToString());
                if (existMemberShip == null)//if not find ,that means not exists in destination ,so create it~!
                {
                    UserProfileService.MembershipData memberData = null;
                    try
                    {
                        memberData = GetMemberShipData(membershipData, title, group, privacyLevel);
                        mNetWork.UserProfileAddMemberShip(loginName, memberData, group, privacyLevel);
                    }
                    catch (Exception e)
                    {
                        mLogger.Debug("add membership error: {0}", e.ToString());
                        bool createGroupSucceess = true;
                        try
                        {
                            mNetWork.UserProfileCreateMemberGroup(memberData);
                        }
                        catch (Exception e1)
                        {
                            createGroupSucceess = false;
                            mLogger.Error("Can not add MemberShip for the reason that: {0}", e1.ToString());
                        }
                        if (createGroupSucceess)
                        {
                            mNetWork.UserProfileAddMemberShip(loginName, memberData, group, privacyLevel);
                        }
                    }
                }
                else
                {
                    if ((int)existMemberShip.Privacy != privacyLevel && privacyLevel != 5)//真实365privacy 不设置，无法更新。
                    {
                        Dictionary<string, object> memberShipGroup = membershipData["MembershipGroup"] as Dictionary<string, object>;
                        mNetWork.UserProfileUpdateMembershipPrivacy(loginName, new Guid(memberShipGroup["SourceInternal"].ToString()), memberShipGroup["SourceReference"] as string, privacyLevel);
                    }
                }
            }
        }
        public void RestoreUserProfileLinks(AveWebServiceNetWork mNetWork, Dictionary<string, object> userProfileInfo, string loginName)
        {
            List<Dictionary<string, object>> quickLinksList = userProfileInfo["Links"] as List<Dictionary<string, object>>;

            foreach (Dictionary<string, object> quickLinkData in quickLinksList)
            {
                string profileManagerUrl = quickLinkData["ProfileManagerUrl"] as string;
                string name = quickLinkData["Title"] as string;
                string url = quickLinkData["Url"] as string;
                string group = quickLinkData["Group"] as string;
                int privacyLevel = (int)Enum.Parse(typeof(UserProfileService.Privacy), ((AvePrivacy)quickLinkData["PrivacyLevel"]).ToString());
                bool isLinkExists = false;
                foreach (UserProfileService.QuickLinkData quickLink in mNetWork.UserProfileGetUserLinks(loginName))
                {
                    if (quickLink.Name.Equals(name))
                    {
                        isLinkExists = true;
                        break;
                    }
                }
                if (isLinkExists)
                {
                    continue;
                }
                try
                {
                    UserProfileService.QuickLinkData newAddLink = mNetWork.UserProfileAddLink(loginName, name, url, group, privacyLevel);
                }
                catch (Exception e)
                {
                    mLogger.Error("Add link failed, error message: {0}", e.Message.ToString());
                }
            }
        }
        public void RestoreUserProfileColleagues(AveWebServiceNetWork mNetWork, Dictionary<string, object> userProfileInfo, string loginName)
        {
            List<Dictionary<string, object>> colleaguesList = userProfileInfo["Colleagues"] as List<Dictionary<string, object>>;
            foreach (Dictionary<string, object> colleagueData in colleaguesList)
            {
                string group = colleagueData["Group"] as string;
                string accountName = colleagueData["AccountName"] as string;
                bool isInWorkGroup = Convert.ToBoolean(colleagueData["IsInWorkGroup"]);
                int privacyLevel = (int)Enum.Parse(typeof(UserProfileService.Privacy), ((AvePrivacy)colleagueData["PrivacyLevel"]).ToString());

                if (string.IsNullOrEmpty(accountName) || loginName.EndsWith(accountName, StringComparison.OrdinalIgnoreCase))//不能follow自己
                {
                    continue;
                }
                UserProfileService.ContactData destinationContactData = null;
                foreach (UserProfileService.ContactData conData in mNetWork.UserProfileGetUserColleagues(loginName))
                {
                    if (conData.AccountName.Equals(accountName, StringComparison.OrdinalIgnoreCase))
                    {
                        destinationContactData = conData;
                        break;
                    }
                }
                try
                {
                    if (destinationContactData == null)
                    {
                        FindOrCreateUserProfile(mNetWork, accountName);//若目的端此Colleague没有UserProfile会加不上去。
                        UserProfileService.ContactData conData = mNetWork.UserProfileAddColleague(loginName, accountName, group, privacyLevel, isInWorkGroup);
                        continue;
                    }
                    if (privacyLevel != (int)destinationContactData.Privacy)//colleague暂时只能更新一个Privacy属性
                    {
                        mNetWork.UserProfileUpdateColleaguePrivacy(loginName, accountName, privacyLevel);
                    }
                }
                catch (Exception e)
                {
                    mLogger.Error("Restore the colleague, {0} Error {1}", accountName, e.Message.ToString());
                }
            }
        }
        public void UpdateUserProfileProperties(AveWebServiceNetWork mNetWork, string loginName, Dictionary<string, object> userProfileInfo, UserProfileService.PropertyData[] userProfileProperties)
        {
            Dictionary<string, UserProfileService.PropertyData> destinationPropertiesDic = userProfileProperties.ToDictionary(v => v.Name, v => v);
            List<Dictionary<string, object>> sourcePropertiesList = userProfileInfo["Properties"] as List<Dictionary<string, object>>;
            List<string> IgnoreProperties = GetIgnoreProperties(mNetWork);
            List<UserProfileService.PropertyData> needUpdateProperties = new List<UserProfileService.PropertyData>();
            foreach (Dictionary<string, object> sourceProperty in sourcePropertiesList)
            {
                UserProfileService.PropertyData needUpdateProperty = null;
                string propertyName = sourceProperty["Name"] as string;
                if (IgnoreProperties.Contains(propertyName) || IgnoreProperties.Contains(propertyName.ToLowerInvariant()))
                {
                    continue;
                }
                if (destinationPropertiesDic.Keys.Contains(propertyName))
                {
                    UserProfileService.PropertyData originalProperty = destinationPropertiesDic[propertyName];
                    List<string> valuesList = sourceProperty["Values"] as List<string>;
                    UserProfileService.Privacy uPrivacyLevel = (UserProfileService.Privacy)Enum.Parse(typeof(UserProfileService.Privacy), ((AvePrivacy)sourceProperty["Privacy"]).ToString());
                    needUpdateProperty = GetNeedUpdateProperty(originalProperty, valuesList, propertyName, uPrivacyLevel);
                    if (needUpdateProperty != null)
                    {
                        //Manager属性中User在目的端不存在时，update会抛异常，单独更新不影响其它属性
                        if (needUpdateProperty.Name.Equals("Manager", StringComparison.OrdinalIgnoreCase)) 
                        {
                            RealUpdate(mNetWork, loginName, new UserProfileService.PropertyData[] { needUpdateProperty });
                        }
                        else
                        {
                            needUpdateProperties.Add(needUpdateProperty);
                        }
                    }
                    if (needUpdateProperties.Count == 10)
                    {
                        RealUpdate(mNetWork, loginName, needUpdateProperties.ToArray());
                        needUpdateProperties.Clear();
                    }
                }
            }
            if (needUpdateProperties.Count > 0)//循环完之后，若有余数则在循环内无法更新，放在外边更新。
            {
                RealUpdate(mNetWork, loginName, needUpdateProperties.ToArray());
                needUpdateProperties.Clear();
            }
            #region old code
            //try
            //{            
            //    mNetWork.UserProfileModifyUserPropertyByAccountName(loginName, needUpdateProperties.ToArray());
            //}
            //catch (Exception e)
            //{
            //    mLogger.Error("Update UserProfile Properties Error: {0}", e.Message.ToString());
            //}
            #endregion
        }

        private void RealUpdate(AveWebServiceNetWork mNetWork, string loginName, UserProfileService.PropertyData[] dataArray)
        {
            try
            {
                mNetWork.UserProfileModifyUserPropertyByAccountName(loginName, dataArray);
            }
            catch (Exception e)
            {
                mLogger.Debug("UPSLog Failed to restpre property values, try to restore one by one. Exception: {0}", e.Message);
                foreach (var data in dataArray)
                {
                    try
                    {
                        mNetWork.UserProfileModifyUserPropertyByAccountName(loginName, new UserProfileService.PropertyData[] { data });
                    }
                    catch (Exception ex)
                    {
                        var errorValues = new StringBuilder("{");
                        foreach (var val in data.Values)
                        {
                            errorValues.AppendFormat("[{0}]", val.Value.ToString());
                        }
                        errorValues.Append("}");
                        mLogger.Error("UPSLog Update UserProfile Properties Failed: {0}, values: {1}, exception:{2}", data.Name, errorValues, ex);
                    }
                }
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "adguid quicklinks sps-feedidentifier sps-masteraccountname sps-resourceaccountname sps-peers sps-proxyaddresses sps-resourcesid")]
        private List<string> GetIgnoreProperties(AveWebServiceNetWork mNetWork)
        {
            List<string> IgnoreProperties = new List<string>();
            IgnoreProperties.Add("sps-proxyaddresses");
            IgnoreProperties.Add("sps-masteraccountname");
            IgnoreProperties.Add("adguid");
            IgnoreProperties.Add("quicklinks");
            IgnoreProperties.Add("sps-peers");
            IgnoreProperties.Add("sps-resourceaccountname");
            IgnoreProperties.Add("sps-resourcesid");
            IgnoreProperties.Add("UserProfile_GUID");
            IgnoreProperties.Add("SID");
            IgnoreProperties.Add("AccountName");
            IgnoreProperties.Add("UserName");
            IgnoreProperties.Add("PersonalSpace");
            IgnoreProperties.Add("sps-feedidentifier");
            //If the property is not allow to edit, it will throw exception when modifying the user profile.
            UserProfileService.PropertyInfo[] userProperties = mNetWork.UserProfileGetUserProfileSchema();
            foreach (UserProfileService.PropertyInfo property in userProperties)
            {
                if (!property.IsAdminEditable)
                {
                    IgnoreProperties.Add(property.Name);
                }
            }
            mLogger.Info("Some user profile properties can not be modified. Please set the user profile property's IsUserEditable as true if you want to modify it.");
            return IgnoreProperties;
        }
        private UserProfileService.MembershipData GetMemberShipData(Dictionary<string, object> membershipData, string title, string group, int privacyLevel)
        {
            UserProfileService.MembershipData memberData = new UserProfileService.MembershipData();
            memberData.Url = membershipData["Url"] as string;
            memberData.DisplayName = title;
            memberData.Group = group;
            memberData.Privacy = (UserProfileService.Privacy)privacyLevel;

            memberData.MemberGroup = new UserProfileService.MemberGroupData();
            Dictionary<string, object> memberShipGroup = membershipData["MembershipGroup"] as Dictionary<string, object>;
            if (memberShipGroup["SourceInternal"] != null)
            {
                memberData.MemberGroup.SourceInternal = new Guid(memberShipGroup["SourceInternal"].ToString());
            }
            memberData.MemberGroup.SourceReference = memberShipGroup["SourceReference"] != null ? memberShipGroup["SourceReference"].ToString() : "";
            memberData.Source = (UserProfileService.MembershipSource)memberShipGroup["Source"];
            memberData.MailNickname = memberShipGroup["MailNickName"] != null ? memberShipGroup["MailNickName"].ToString() : "";
            memberData.MemberGroupID = (long)memberShipGroup["Count"];
            return memberData;
        }
        private UserProfileService.PropertyData GetNeedUpdateProperty(UserProfileService.PropertyData originalProperty, List<string> valuesList, string propertyName, UserProfileService.Privacy privacyLevel)
        {
            if (ProfileValueEquals(valuesList, originalProperty.Values))
            {
                return null;
            }
            UserProfileService.PropertyData needUpdateProperty = new UserProfileService.PropertyData();
            needUpdateProperty.Name = propertyName;
            needUpdateProperty.Privacy = privacyLevel;
            needUpdateProperty.IsPrivacyChanged = true;
            needUpdateProperty.Values = new UserProfileService.ValueData[valuesList.Count];
            int index = 0;
            foreach (string value in valuesList)
            {
                UserProfileService.ValueData valueData = new UserProfileService.ValueData();
                valueData.Value = value;
                needUpdateProperty.Values[index++] = valueData;
            }
            needUpdateProperty.IsValueChanged = true;
            return needUpdateProperty;
        }
        public UserProfileService.PropertyData[] FindOrCreateUserProfile(AveWebServiceNetWork mNetWork, string loginName)
        {
            UserProfileService.PropertyData[] userProfileProperties = null;
            try
            {
                userProfileProperties = mNetWork.UserProfileGetUserProfile(loginName);
            }
            catch (Exception e)
            {
                mLogger.Info(string.Format("Can't find user profile, LoginName:{0}, Message {1}", loginName, e.ToString()));
                userProfileProperties = mNetWork.UserProfileCreateUserProfile(loginName);
            }
            return userProfileProperties;
        }

        public bool ProfileValueEquals(List<string> list, UserProfileService.ValueData[] values)
        {
            if (list.Count != values.Length)
            {
                return false;
            }
            for (int i = 0; i < list.Count; ++i)
            {
                if (!string.Equals(list[i], ConvertValueAsString(values[i])))
                {
                    return false;
                }
            }
            return true;
        }
        public string ConvertValueAsString(object obj)
        {
            if (obj == null)
            {
                return null;
            }
            UserProfileService.SPTimeZone timeZone = obj as UserProfileService.SPTimeZone;
            if (timeZone != null)
            {
                return timeZone.ID.ToString();
            }
            return obj.ToString();
        }
        #endregion

        #region Update
        //public Dictionary<string, object> UpdateView(string webServerRelativeUrl, string listName, Guid listId, Guid viewId, Dictionary<string, object> viewProperties)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> UpdateFolder(string webServerRelativeUrl, string listName, Guid listId, string folderServerRelativeUrl, Dictionary<string, object> folderProperties)
        //{
        //    throw new NotImplementedException();
        //}
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Btn is a key")]
        //public Dictionary<string, object> UpdateSite(Dictionary<string, object> siteProperties)
        //{
        //    throw new NotImplementedException();
        //}
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "onetid is a part of value")]
        public Dictionary<string, object> UpdateSitePortal(Dictionary<string, object> siteProperties)
        {
            Dictionary<string, object> sitePortal = new Dictionary<string, object>();
            string postUrl = mWebUrl.TrimEnd('/') + "/_layouts/portal.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl01$TxtPortalURL"] = siteProperties.ContainsKey("PortalUrl") ? HttpUtility.UrlEncode(siteProperties["PortalUrl"].ToString()) : formValues["ctl00$PlaceHolderMain$ctl00$ctl01$TxtPortalURL"];
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl02$TxtPortalName"] = siteProperties.ContainsKey("PortalName") ? HttpUtility.UrlEncode(siteProperties["PortalName"].ToString()) : formValues["ctl00$PlaceHolderMain$ctl00$ctl02$TxtPortalName"];
            }
            else
            {
                bodyDic["ctl00$PlaceHolderMain$ctl00$portalEnabled"] = "onetidPortalNotEnabled";
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
            return sitePortal;
        }

        //public Dictionary<string, object> UpdateWeb(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        //{ throw new NotImplementedException(); }

        /// <summary>        
        /// For restoring list setting AllowMultiResponses restore.
        /// </summary>
        public Dictionary<string, object> UpdateList(string webServerRelativeUrl, string listName, Guid listId, Dictionary<string, object> listProperties)
        {
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                string webAppName = GetWebAppNameFromSiteUrl(mWebUrl);
                string url = webAppName + webServerRelativeUrl.TrimEnd('/');
                mNetWork.InitialNetWorker(AveWebServiceType.Lists, url);

                XmlNode listInfo = mNetWork.ListGetList(listName);
                string listGUID = listInfo.Attributes["ID"].Value;
                string version = listInfo.Attributes["Version"].Value;

                XmlDocument doc = new XmlDocument();
                XmlElement listPropertiesNode = null;
                if (listProperties != null)
                {
                    listPropertiesNode = doc.CreateElement("List");
                    if (listProperties.ContainsKey("ValidationFormula") || listProperties.ContainsKey("ValidationMessage"))
                    {
                        XmlElement validationNode = listPropertiesNode.AppendChild(doc.CreateElement("Validation")) as XmlElement;
                        validationNode.SetAttribute("Message", listProperties.ContainsKey("ValidationMessage") ? listProperties["ValidationMessage"].ToString() : string.Empty);
                        validationNode.InnerText = listProperties.ContainsKey("ValidationFormula") ? listProperties["ValidationFormula"].ToString() : string.Empty;
                        listProperties.Remove("ValidationFormula");
                        listProperties.Remove("ValidationMessage");
                    }
                    foreach (KeyValuePair<string, object> pair in listProperties)
                    {
                        if (pair.Value != null)
                        {
                            listPropertiesNode.SetAttribute(pair.Key, pair.Value.ToString());
                        }
                    }
                }
                mNetWork.ListUpdateList(listGUID, (XmlNode)listPropertiesNode, null, null, null, version);
                listInfo = mNetWork.ListGetList(listName);
                Dictionary<string, object> newListProperties = new Dictionary<string, object>();
                XmlNodeToDicValue(newListProperties, listInfo);
                mNetWork.Dispose();
                return newListProperties;
            }
        }
        //public Dictionary<string, object> UpdateItem(string webServerRelativeUrl, string listName, Guid listId, int itemId, Dictionary<string, object> itemProperties)
        //{
        //    throw new NotImplementedException();
        //}
        public Dictionary<string, object> UpdateAudit(Dictionary<string, object> needUpdateProperties)
        {
            if (needUpdateProperties.ContainsKey("AuditFlags"))
            {
                int auditFlags = (int)needUpdateProperties["AuditFlags"];
                string postUrl = mWebUrl.TrimEnd('/') + "/_layouts/AuditSettings.aspx";
                string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);

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
                    #region convert flags
                    if ((auditFlags & (int)AveAuditMaskType.View) > 0)
                    {
                        bodyDic["ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditView"] = "on";
                    }
                    else
                    {
                        bodyDic.Remove("ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditView");
                    }
                    if ((auditFlags & (int)AveAuditMaskType.Update) > 0)
                    {
                        bodyDic["ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditEdit"] = "on";
                    }
                    else
                    {
                        bodyDic.Remove("ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditEdit");
                    }
                    if ((auditFlags & (int)AveAuditMaskType.CheckIn) > 0)
                    {
                        bodyDic["ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditCheckInOut"] = "on";
                    }
                    else
                    {
                        bodyDic.Remove("ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditCheckInOut");
                    }
                    if ((auditFlags & (int)AveAuditMaskType.Copy) > 0)
                    {
                        bodyDic["ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditMoveCopy"] = "on";
                    }
                    else
                    {
                        bodyDic.Remove("ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditMoveCopy");
                    }
                    if ((auditFlags & (int)AveAuditMaskType.Delete) > 0)
                    {
                        bodyDic["ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditDeleteRestore"] = "on";
                    }
                    else
                    {
                        bodyDic.Remove("ctl00$PlaceHolderMain$ctl01$ctl00$CheckBoxAuditDeleteRestore");
                    }
                    if ((auditFlags & (int)AveAuditMaskType.SchemaChange) > 0)
                    {
                        bodyDic["ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditColumnsContentType"] = "on";
                    }
                    else
                    {
                        bodyDic.Remove("ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditColumnsContentType");
                    }
                    if ((auditFlags & (int)AveAuditMaskType.Search) > 0)
                    {
                        bodyDic["ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditSearch"] = "on";
                    }
                    else
                    {
                        bodyDic.Remove("ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditSearch");
                    }
                    if ((auditFlags & (int)AveAuditMaskType.SecurityChange) > 0)
                    {
                        bodyDic["ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditPerms"] = "on";
                    }
                    else
                    {
                        bodyDic.Remove("ctl00$PlaceHolderMain$ctl02$ctl00$CheckBoxAuditPerms");
                    }
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl03$RptControls$BtnCancelAuditSettings");
                    bodyDic.Remove("");

                    #endregion
                    bodyDic["ctl00$PlaceHolderMain$ctl03$RptControls$BtnUpdateAuditSettings"] = "OK";

                    byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
                    AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
                }
                else
                {
                    needUpdateProperties["UpdateAuditError"] = "An error occurred while updating audit property, please check your SharePoint Version, Foundation SharePoint do not have Audit function.";
                }
            }
            return needUpdateProperties;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "")]
        private bool ResetOnPremiseTrimAuditLog(Dictionary<string, object> needUpdateProperties, Dictionary<string, object> bodyDic, Dictionary<string, object> formValues)
        {
            if (formValues.ContainsKey("ctl00$PlaceHolderMain$ctl00$ctl03$trimAuditLog"))
            {
                string isEnable = (string)formValues["ctl00$PlaceHolderMain$ctl00$ctl03$trimAuditLog"];
                string radTrimAuditLogYes = "RadTrimAuditLogYes";
                string radTrimAuditLogNo = "RadTrimAuditLogNo";
                if (IsRadTrimAuditLogYes(needUpdateProperties, formValues, "ctl00$PlaceHolderMain$ctl00$ctl03$trimAuditLog"))
                {

                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl03$trimAuditLog"] = radTrimAuditLogYes;
                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl04$TxtTrimRetention"] = needUpdateProperties.ContainsKey("AuditLogTrimmingRetention") ? (int)needUpdateProperties["AuditLogTrimmingRetention"] : formValues["ctl00$PlaceHolderMain$ctl00$ctl04$TxtTrimRetention"];
                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl05$TxtReportStorageLocation"] = needUpdateProperties.ContainsKey("_auditlogreportstoragelocation") ? (string)needUpdateProperties["_auditlogreportstoragelocation"] : formValues["ctl00$PlaceHolderMain$ctl00$ctl05$TxtReportStorageLocation"];
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl03$RptControls$BtnCancelAuditSettings");
                }
                else
                {
                    bodyDic["ctl00$PlaceHolderMain$ctl00$ctl03$trimAuditLog"] = radTrimAuditLogNo;
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl00$ctl04$TxtTrimRetention");
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl00$ctl05$TxtReportStorageLocation");
                    bodyDic.Remove("ctl00$PlaceHolderMain$ctl03$RptControls$BtnCancelAuditSettings");
                }
                return true;
            }
            return false;
        }

        private bool IsRadTrimAuditLogYes(Dictionary<string, object> needUpdateProperties, Dictionary<string, object> formValues, string trimAuditLogKey)
        {
            if (needUpdateProperties.ContainsKey("TrimAuditLog"))
            {
                return (bool)needUpdateProperties["TrimAuditLog"];
            }
            return ((string)formValues[trimAuditLogKey]).Equals("RadTrimAuditLogYes", StringComparison.OrdinalIgnoreCase);
        }

        //public Dictionary<string, object> UpdateAlert(string webServerRelativeUrl, Guid alertId, bool sendEmail, Dictionary<string, object> needUpdateAlertProperties)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> UpdateRoleAssignment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, int principalId, Dictionary<string, object> needUpdateRoleAssignmentProperties, string roleAssignmentsSource)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> UpdateRoleDefinition(string webServerRelativeUrl, int id, Dictionary<string, object> needUpdateRoledefinitionProperties)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> UpdateEventReceiver(string webServerRelativeUrl, string listServerRealtiveUrl, string listTitle, Guid listId, string eventReceiverDefSource, Guid eventReceiverDefId, Dictionary<string, object> needUpdateEventReceiverProperties)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> UpdateReadOnlyField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, Dictionary<string, object> contentTypeProp, Dictionary<string, object> fieldProperties)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> UpdateNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> needUpdateProperties)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> UpdatePropertyBag(string webServerRelativeUrl, string propertyBagSource, Guid alertId, Dictionary<string, object> needUpdateProperties)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> BreakRoleInheritance(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, bool copyRoleAssignments, bool clearSubscopes, string roleAssignmentsSource)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> BreakRoleDefinitionInheritance(string webServerRelativeUrl, bool copyRoleDefinitions, bool keepRoleAssignments)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> ResetRoleInheritance(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid listId, int itemId, string roleAssignmentsSource)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> UpdateGroup(string webServerRelativeUrl, int id, Dictionary<string, object> groupProperties)
        //{
        //    throw new NotImplementedException();
        //}
        //public void MoveFieldTo(string webServerRelativeUrl, string listTitle, Guid listId, Guid viewId, string field, int index)
        //{
        //    throw new NotImplementedException();
        //}
        //public void UpdateComment(string webServerRelativeUrl, string serverRelativeUrl, string comment)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> CheckIn(string webServerRelativeUrl, string fileServerRelativeUrl, string comment, int checkinType)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> CheckOut(string webServerRelativeUrl, string fileServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}
        //public void CopyTo(string webServerRelativeUrl, string fileServerRelativeUrl, string strNewUrl, bool bOverWrite)
        //{
        //    throw new NotImplementedException();
        //}
        //public void MoveTo(string webServerRelativeUrl, string fileServerRelativeUrl, string strNewUrl, int flags)
        //{
        //    throw new NotImplementedException();
        //}

        //public void SaveBinary(string webServerRelativeUrl, string fileServerRelativeUrl, Stream file)
        //{
        //    throw new NotImplementedException();
        //}

        public Dictionary<string, object> UpdateContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeId, bool updateChildren, string contentTypeSource, Dictionary<string, object> needUpdateContentTypeProperties)
        {
            //For workflow, update list contentType XmlDocuments...
            //if (string.IsNullOrEmpty(listName)) 
            //{
            //    return null;
            //}
            bool needUpdate = false;
            List<XmlNode> xmlNodes = new List<XmlNode>();
            XmlDocument contentTypePropertyXml = new XmlDocument();
            if (needUpdateContentTypeProperties.ContainsKey("AddedDocuments"))
            {
                XmlDocument xmlDoc = new XmlDocument();
                Dictionary<string, string> XmlDocumentData = (Dictionary<string, string>)needUpdateContentTypeProperties["AddedDocuments"];
                foreach (string key in XmlDocumentData.Keys)
                {
                    xmlDoc.LoadXml(XmlDocumentData[key]);
                    XmlNode nodeEty = (XmlNode)xmlDoc.DocumentElement;
                    xmlNodes.Add(nodeEty);
                    xmlDoc.RemoveAll();
                    needUpdate = true;
                }
            }
            contentTypePropertyXml.LoadXml("<ContentType/>");
            if (needUpdateContentTypeProperties.ContainsKey("NewDocumentControl"))
            {
                XmlAttribute newAttribute = contentTypePropertyXml.CreateAttribute("NewDocumentControl");
                newAttribute.Value = needUpdateContentTypeProperties["NewDocumentControl"] == null ? string.Empty : needUpdateContentTypeProperties["NewDocumentControl"].ToString();
                contentTypePropertyXml.FirstChild.Attributes.Append(newAttribute);
                needUpdate = true;
            }
            if (needUpdateContentTypeProperties.ContainsKey("RequireClientRenderingOnNew"))
            {
                XmlAttribute newAttribute = contentTypePropertyXml.CreateAttribute("RequireClientRenderingOnNew");
                newAttribute.Value = needUpdateContentTypeProperties["RequireClientRenderingOnNew"] == null ? "false" : needUpdateContentTypeProperties["RequireClientRenderingOnNew"].ToString();
                contentTypePropertyXml.FirstChild.Attributes.Append(newAttribute);
                needUpdate = true;
            }
            if (!needUpdate)
            {
                return new Dictionary<string, object>();
            }
            mLogger.Debug("Use web service to update content type: {0}", contentTypeId);
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                if (string.IsNullOrEmpty(listName))
                {
                    mNetWork.InitialNetWorker(AveWebServiceType.Webs, this.WebAppName + webServerRelativeUrl);
                    foreach (XmlNode node in xmlNodes)
                    {
                        mNetWork.UpdateContentTypeXmlDocuments(null, contentTypeId, node);
                    }
                }
                else
                {
                    mNetWork.InitialNetWorker(AveWebServiceType.Lists, this.WebAppName + webServerRelativeUrl);
                    foreach (XmlNode node in xmlNodes)
                    {
                        mNetWork.UpdateContentTypeXmlDocuments(listName, contentTypeId, node);
                    }
                    if (contentTypePropertyXml.DocumentElement.HasAttributes)
                    {
                        mNetWork.UpdateContentType(listName, contentTypeId, contentTypePropertyXml.FirstChild, null, null, null, "false");
                    }
                }
            }
            return new Dictionary<string, object>();
        }

        //public void SaveBinary(string webServerRelativeUrl, string fileServerRelativeUrl, byte[] file)
        //{
        //    throw new NotImplementedException();
        //}
        //public void UndoCheckOut(string webServerRelativeUrl, string fileServerRelativeUrl)
        //{
        //    throw new NotImplementedException();
        //}
        //public void UnPublish(string webServerRelativeUrl, string fileServerRelativeUrl, string comment)
        //{
        //    throw new NotImplementedException();
        //}
        //public void Publish(string webServerRelativeUrl, string fileServerRelativeUrl, string comment)
        //{
        //    throw new NotImplementedException();
        //}
        public Dictionary<string, object> UpdateFile(string webServerRelativeUrl, string listName, string fileServerRelativeUrl, Dictionary<string, object> prop)
        {
            mRequestCommon.UpdateFileProperties(webServerRelativeUrl, fileServerRelativeUrl, prop);
            return null;
        }
        //public void Approve(string webServerRelativeUrl, string fileServerRelativeUrl, string comment)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Deny(string webServerRelativeUrl, string fileServerRelativeUrl, string comment)
        //{
        //    throw new NotImplementedException();
        //}

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "ctl00%24PlaceHolderMain%24ctl00%24RptControls%24BtnOk is a part of xml")]
        public void MoveNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> previousNodeProperties, string moveMethodName)
        {
            mRequestCommon.MoveNavigationNode(webServerRelativeUrl, navigationNodeProperties, previousNodeProperties, moveMethodName);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "editnav is a part of xml")]
        public void MoveNavigationNodeToCollection(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties)
        {
            try
            {
                string postUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.Trim('/');
                int nodeId = (int)navigationNodeProperties["NodeId"];
                postUrl = postUrl + string.Format("/_layouts/editnav.aspx?ID={0}", nodeId);
                int parentId = (int)navigationNodeProperties["NodeParentId"];
                string nodeTitle = navigationNodeProperties["NodeTitle"].ToString();

                string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
                bodyDic["ctl00%24PlaceHolderMain%24ctl00%24ctl01%24txtTitle"] = nodeTitle;
                bodyDic["ctl00%24PlaceHolderMain%24CategorySection%24ctl00%24SelectList1"] = parentId;
                byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
                AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", data, null, mTokenProvider);
            }
            catch (Exception ex)
            {
                mLogger.Error("Move navigationNode to nodeCollection failed.Web:{0}.Error Message:{1}.", webServerRelativeUrl, ex.ToString());
                throw new Exception("Move navigationNode to nodeCollection failed.");
            }
        }

        //public Dictionary<string, object> UpdateField(string webServerRelativeUrl, string listName, Guid listId, string internalName, string fieldSource, Dictionary<string, object> contentTypeProp, Dictionary<string, object> fieldProperties)
        //{
        //    throw new NotImplementedException();
        //}
        public Dictionary<string, object> UpdateUser(string webServerRelativeUrl, string loginName, string name, string userColSource, Dictionary<string, object> userProp)
        {
            bool updateAdminOnly = false;
            if (userProp.ContainsKey("OldAdministrators") && userProp.ContainsKey("NewAdministrators") && userProp.ContainsKey("IsSiteAdmin"))
            {
                List<Dictionary<string, object>> newAdmins = userProp["NewAdministrators"] as List<Dictionary<string, object>>;
                string oldAdmins = userProp["OldAdministrators"].ToString();
                this.UpdateSiteAdministrators(webServerRelativeUrl, oldAdmins, newAdmins);
                if (userProp.Count == 3)
                {
                    updateAdminOnly = true;
                }
            }
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            string userName = name;
            string userEmail = string.Empty;
            string userNotes = string.Empty;
            foreach (KeyValuePair<string, object> pair in userProp)
            {
                switch (pair.Key)
                {
                    case "Email":
                        userEmail = userProp["Email"] as string;
                        break;
                    case "Name":
                        userName = userProp["Name"] as string;
                        break;
                    case "Notes":
                        userNotes = userProp["Notes"] as string;
                        break;
                    default:
                        break;
                }
            }
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.UserGroup, webFullUrl);
                if (!updateAdminOnly)
                {
                    mNetWork.UserGroupUpdateUser(loginName, userName, userEmail, userNotes);
                }
                XmlNode node = mNetWork.UserGroupGetUserInfo(loginName);
                return this.GetUserDic(node);
            }
        }
        public void SetListItemRatings(string listItemUrl, string itemTitle, int ratings, Guid siteId, Guid webId)
        {
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.SocialDataService, mWebUrl);
                try
                {
                    SocialDataService.FeedbackData dataEntry = new SocialDataService.FeedbackData();
                    dataEntry.SiteId = siteId;
                    dataEntry.RatedAssetWebId = webId;
                    dataEntry.RatedAssetId = listItemUrl;
                    if (itemTitle.Contains("."))
                    {
                        itemTitle = itemTitle.Substring(0, itemTitle.IndexOf('.'));
                    }
                    mNetWork.SocialDataSetRatings(listItemUrl, ratings, itemTitle, dataEntry);
                }
                catch (Exception e)
                {
                    mLogger.Error("Set Item Rating Error : {0}", e.ToString());
                }
            }
        }
        //public void UpdateUserProfileDetails(string accountName, string xml)
        //{
        //    throw new NotImplementedException();
        //}
        //public void UpdateUserProfileMemberships(string accountName, string xml)
        //{
        //    throw new NotImplementedException();
        //}
        //public void UpdateUserProfileColleages(string accountName, string xml)
        //{
        //    throw new NotImplementedException();
        //}
        //public void UpdateUserProfileTags(string accountName, string xml)
        //{
        //    throw new NotImplementedException();
        //}

        //public void SetThemeUrlForWeb(string webServerRelativeUrl, string themeUrl)
        //{

        //}

        //public void ApplyTo(string webServerRelativeUrl, bool shareGenerated, string name)
        //{

        //}

        //public Dictionary<string, object> UpdatePublishingWeb(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> UpdateTermStore(Guid guid, Dictionary<string, object> needUpdateProperties)
        //{
        //    throw new NotImplementedException();
        //}

        //public Dictionary<string, object> UpdateUserProfileProperties(string userProfilePropertyName, Dictionary<string, object> dictionary)
        //{
        //    throw new NotImplementedException();
        //}

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "metadatacolsettings is a part of xml")]
        public static void UpdateListItems(string webAppName, string webRelativeUrl, string listName, int itemId, string fileRef, object obj, Dictionary<string, object> itemProp)
        {
            UpdateListItems(webAppName, webRelativeUrl, listName, itemId, fileRef, obj, itemProp, null);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "metadatacolsettings is a part of xml")]
        public static void UpdateListItems(string webAppName, string webRelativeUrl, string listName, int itemId, string fileRef, object obj, Dictionary<string, object> itemProp, ITokenProvider tokenProvider)
        {
            string url = webAppName + webRelativeUrl.TrimEnd('/');
            Lists.Lists listService = new Lists.Lists();
            listService.Url = url + "/_vti_bin/Lists.asmx";
            if (tokenProvider != null)
            {
                listService.TokenProvider = tokenProvider;
            }
            else
            {
                NetworkCredential credential = obj as NetworkCredential;
                if (credential != null)
                {
                    listService.Credentials = credential;
                }
                else
                {
                    listService.CookieContainer = obj as CookieContainer;
                }
            }
            //Build xml 
            //<Batch OnError="Continue" DateInUtc="True">
            //<Method ID="1" Cmd="Moderate">
            //<Field Name="ID">1</Field> 
            //<Field Name="FileRef">fileRef</Field> 
            //<Field Name="_ModerationStatus">0</Field> 
            //<Field Name="Modified">2014-10-14T10:34:04Z</Field> 
            //</Method>
            //</Batch>

            XmlDocument doc = new XmlDocument();
            var documentElement = doc.CreateElement("Batch");
            documentElement.SetAttribute("OnError", "Continue");
            documentElement.SetAttribute("DateInUtc", "True");

            doc.AppendChild(documentElement);

            var methodElement = doc.CreateElement("Method");
            methodElement.SetAttribute("ID", "1");
            methodElement.SetAttribute("Cmd", "Moderate");
            documentElement.AppendChild(methodElement);

            var field1 = doc.CreateElement("Field");
            field1.SetAttribute("Name", "ID");
            field1.InnerText = itemId.ToString();
            methodElement.AppendChild(field1);

            var field2 = doc.CreateElement("Field");
            field2.SetAttribute("Name", "FileRef");
            field2.InnerText = fileRef;
            methodElement.AppendChild(field2);

            var field3 = doc.CreateElement("Field");
            field3.SetAttribute("Name", "_ModerationStatus");
            field3.InnerText = itemProp["ModerationStatus"].ToString();
            methodElement.AppendChild(field3);

            if (itemProp["Modified"] != null)
            {
                DateTime modified = new DateTime(((DateTime)itemProp["Modified"]).Ticks, DateTimeKind.Utc);
                var field4 = doc.CreateElement("Field");
                field4.SetAttribute("Name", "Modified");
                field4.InnerText = modified.ToString("yyyy-MM-ddTHH:mm:ssZ");
                methodElement.AppendChild(field4);
            }

            listService.UpdateListItems(listName, doc.DocumentElement);
        }

        public static Guid AddWebPartWithWebService(string webUrl, string pageUrl, object identity, AveWebPartBaseInfo webpartInfo)
        {
            return AddWebPartWithWebService(webUrl, pageUrl, identity, null, webpartInfo);
        }
        public static Guid AddWebPartWithWebService(string webUrl, string pageUrl, object identity, ITokenProvider tokenProvider, AveWebPartBaseInfo webpartInfo)
        {
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(null, webUrl, identity, tokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.WebPartPages, webUrl);
                return mNetWork.AddWebPartToZone(pageUrl, webpartInfo.DefinitionXml, webpartInfo.ZoneID, webpartInfo.PartOrder);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "scopedisplaygroup is a part of xml")]
        public void UpdateMetadataListFieldSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/metadatacolsettings.aspx?List={" + listId + "}&Source="
                + WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/listedit.aspx?List=" + listId;
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
            if (updateProperties.ContainsKey("EnableKeywordsField") && (bool)updateProperties["EnableKeywordsField"])
            {
                bodyDic["ctl00$PlaceHolderMain$KeywordsSection$ctl00$CheckBoxEnterpriseKeywords"] = "on";
            }
            if (updateProperties.ContainsKey("EnableMetadataPromotion") && (bool)updateProperties["EnableMetadataPromotion"])
            {
                bodyDic["ctl00$PlaceHolderMain$MDPushSection$ctl00$CheckBoxPromoteMetadata"] = "on";
            }
            bodyDic["ctl00$PlaceHolderMain$ctl00$RptControls$okButton"] = "OK";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Btn is a part of xml")]
        public void UpdateListRssSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProp)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/listsyndication.aspx?List=" + listId.ToString("B");
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
            IList<string> formKeys = new List<string>();
            Dictionary<string, object> formValues = AveHttpWebRequestUtility.GetPostFormValues(html);
            string searchContent = "var readSecurity = ";
            string information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, ";");
            if (!string.IsNullOrEmpty(information))
            {
                formValues["ctl00$PlaceHolderMain$ItemLevelSecuritySection$ctl08$ReadSecurity"] = Convert.ToInt32(information);
                searchContent = "var writeSecurity = ";
                information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, ";");
                formValues["ctl00$PlaceHolderMain$ItemLevelSecuritySection$ctl09$WriteSecurity"] = Convert.ToInt32(information);
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
            bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl00$RptControls$BtnApply";
            bodyDic["ctl00$PlaceHolderMain$EnableRssSection$ctl00$Enabled"] = (bool)updateProp["AllowRss"] ? "EnabledTrue" : "EnabledFalse";
            if (updateProp.ContainsKey("LimitDescriptionLength"))
            {
                bodyDic["ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl00$LimDesc"] = (bool)updateProp["LimitDescriptionLength"] ? "LimDescTrue" : "LimDescFalse";
            }
            if (updateProp.ContainsKey("ChannelTitle"))
            {
                bodyDic["ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl01$TxtChannelTitle"] = updateProp["ChannelTitle"].ToString();
            }
            if (updateProp.ContainsKey("ChannelDescription"))
            {
                bodyDic["ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl02$TxtChannelDescription"] = updateProp["ChannelDescription"].ToString();
            }
            if (updateProp.ContainsKey("ChannelImageUrl"))
            {
                bodyDic["ctl00$PlaceHolderMain$Rss20ChannelInformationSection$ctl03$TxtChannelImageUrl"] = updateProp["ChannelImageUrl"].ToString();
            }

            if (updateProp.ContainsKey("DocumentAsEnclosure"))
            {
                bodyDic["ctl00$PlaceHolderMain$EnclosuresSection$ctl00$FileEnclosure"] = (bool)updateProp["DocumentAsEnclosure"] ? "FileEnclosureTrue" : "FileEnclosureFalse";
            }
            if (updateProp.ContainsKey("DocumentAsLink"))
            {
                bodyDic["ctl00$PlaceHolderMain$EnclosuresSection$ctl01$FileLink"] = (bool)updateProp["DocumentAsLink"] ? "FileLinkTrue" : "FileLinkFalse";
            }

            if (updateProp.ContainsKey("ItemLimit"))
            {
                bodyDic["ctl00$PlaceHolderMain$ItemLimitSection$ctl00$TxtItemLimit"] = updateProp["ItemLimit"];
            }
            if (updateProp.ContainsKey("DayLimit"))
            {
                bodyDic["ctl00$PlaceHolderMain$ItemLimitSection$ctl01$TxtDayLimit"] = updateProp["DayLimit"];
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Btn is a part of xml")]
        public void UpdateListAdvancedSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> advancedSettingProp)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/advsetng.aspx?List=" + listId.ToString("B");
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
            IList<string> formKeys = new List<string>();
            Dictionary<string, object> bodyDic = AveHttpWebRequestUtility.GetPostFormValues(html);
            string searchContent = "var readSecurity = ";
            string information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, ";");
            if (!string.IsNullOrEmpty(information))
            {
                bodyDic["ctl00$PlaceHolderMain$ItemLevelSecuritySection$ctl08$ReadSecurity"] = Convert.ToInt32(information);
                searchContent = "var writeSecurity = ";
                information = AveHttpWebRequestUtility.GetInnerText(html, searchContent, ";");
                bodyDic["ctl00$PlaceHolderMain$ItemLevelSecuritySection$ctl09$WriteSecurity"] = Convert.ToInt32(information);
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
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
        }
        public void UpdateListGeneralSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> generalSettingProp)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/ListGeneralSettings.aspx?List=" + listId.ToString("B");
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "scopedisplaygroup is a part of url")]
        public void UpdateScopeDisplayGroup(int groupId, string groupName, Dictionary<string, object> updateProp)
        {
            string postUrl = mWebUrl.TrimEnd('/') + "/_layouts/scopedisplaygroup.aspx?group=" + groupId;
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
            if (updateProp.ContainsKey("UpdateScopes"))
            {
                List<int> scopeIds = updateProp["UpdateScopes"] as List<int>;
                StringBuilder tempIds = new StringBuilder();
                foreach (int id in scopeIds)
                {
                    tempIds.Append(id + ";");
                }
                bodyDic["ctl00$PlaceHolderMain$scopeIdsHiddenTextBox"] = tempIds.ToString();
            }
            bodyDic["ctl00$PlaceHolderMain$titleTextBox"] = groupName;
            if (updateProp.ContainsKey("Default"))
            {
                bodyDic["ctl00$PlaceHolderMain$defaultScopeDropDown"] = updateProp["Default"];
            }
            bodyDic["ctl00$PlaceHolderMain$okButton"] = "OK";

            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "prjsetng is a part of url")]
        public void UpdateWebLogo(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/prjsetng.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
            bodyDic["__EVENTTARGET"] = "ctl00$PlaceHolderMain$ctl02$RptControls$BtnCreate";
            if (webProperties.ContainsKey("SiteLogoUrl"))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl01$ctl02$TxtSiteLogoUrl"] = webProperties["SiteLogoUrl"];
            }
            if (webProperties.ContainsKey("SiteLogoDescription"))
            {
                bodyDic["ctl00$PlaceHolderMain$ctl01$ctl03$TxtLogoUrlDescription"] = webProperties["SiteLogoDescription"] == null ? string.Empty : HttpUtility.UrlEncode(webProperties["SiteLogoDescription"].ToString());
            }
            //if (webProperties.ContainsKey("Name"))
            //{
            //    bodyDic["ctl00$PlaceHolderMain$idUrlSection$ctl02$TxtCreateSubwebName"] = webProperties["Name"];
            //}
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
        }
        //public void UpdateSpecialProperty(Dictionary<string, object> specialProp)
        //{
        //    throw new NotImplementedException();
        //}
        public void RevertAllDocumentContentStreams(string webServerRelativeUrl)
        {
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.Webs, webFullUrl);
                mNetWork.WebRevertAllDocumentContentStreams();
            }
        }
        public void RevertContentStream(string webServerRelativeUrl, string fileUrl)
        {
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.Webs, webFullUrl);
                mNetWork.WebRevertContentStream(fileUrl);
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Admins is a part of xml")]
        public void UpdateWebSearchAndOfflineAvailability(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/srchvis.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
                    bodyDic["ctl00%24PlaceHolderMain%24IndexSiteContent%24ctl00%24RadIndexSiteContent"] = "radIndexSiteContentNo";
                }
                else
                {
                    bodyDic["ctl00%24PlaceHolderMain%24IndexSiteContent%24ctl00%24RadIndexSiteContent"] = "radIndexSiteContentYes";
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
                    bodyDic["ctl00%24PlaceHolderMain%24AllowSyncSection%24ctl00%24AllowSync"] = "RadAllowSyncNo";
                }
                else
                {
                    bodyDic["ctl00%24PlaceHolderMain%24AllowSyncSection%24ctl00%24AllowSync"] = "RadAllowSyncYes";
                }
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Admins is a part of xml")]
        public Dictionary<string, object> UpdateSiteAdministrators(string webServerRelativeUrl, string oldAdministrators, List<Dictionary<string, object>> newAdministrators)
        {
            Dictionary<string, object> siteAdmins = new Dictionary<string, object>();
            StringBuilder spanData = new StringBuilder("&nbsp;");
            foreach (Dictionary<string, object> dic in newAdministrators)
            {
                string login = dic["LoginName"].ToString().Replace("'", "&#39;");
                string name = dic["Name"].ToString().Replace("'", "&#39;");
                int id = Convert.ToInt32(dic["ID"].ToString());
                dic["IsSiteAdmin"] = true;

                string span1 = string.Format("id=span{0} class=ms-entity-resolved onmouseover=this.contentEditable=false; title={1} tabIndex=-1 onmouseout=this.contentEditable=true; contentEditable=true isContentType=\"true\"", login, login);
                string div1 = string.Format("style=\"DISPLAY: none\" id=divEntityData description=\"{0}\" isresolved=\"True\" displaytext=\"{1}\" key=\"{2}\"", login, name, login);
                string userId = string.Format("<Value xsi:type=\"xsd:string\">{0}</Value>", id);
                string accountName = string.Format("<Value xsi:type=\"xsd:string\">{0}</Value>", login);
                string type = string.Format("<Value xsi:type=\"xsd:string\">{0}</Value>", "User");
                string span2 = string.Format("id=content oncontextmenu=onContextMenuSpnRw(event,ctx); tabIndex=-1 contentEditable=true onmousedown=onMouseDownRw(event);>{0}", name);

                string str =
                    "<SPAN " + span1 + ">"
                    + "<DIV " + div1 + ">"
                        + "<DIV data="
                            + "'<ArrayOfDictionaryEntry xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">"
                                + "<DictionaryEntry>"
                                    + "<Key xsi:type=\"xsd:string\">SPUserID</Key>" + userId
                                + "</DictionaryEntry>"
                                + "<DictionaryEntry>"
                                    + "<Key xsi:type=\"xsd:string\">AccountName</Key>" + accountName
                                + "</DictionaryEntry>"
                                + "<DictionaryEntry>"
                                    + "<Key xsi:type=\"xsd:string\">PrincipalType</Key>" + type
                                + "</DictionaryEntry>"
                            + "</ArrayOfDictionaryEntry>'>"
                        + "</DIV>"
                    + "</DIV>"
                    + "<SPAN " + span2 + "</SPAN>"
                    + "</SPAN>;";
                spanData.Append(str);
            }
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/mngsiteadmin.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
            bodyDic["ctl00%24PlaceHolderMain%24ctl00%24PeopleEditorAdmins%24hiddenSpanData"] = System.Web.HttpUtility.UrlEncode(spanData.ToString());
            bodyDic["ctl00%24PlaceHolderMain%24ctl00%24PeopleEditorAdmins%24downlevelTextBox"] = System.Web.HttpUtility.UrlEncode(spanData.ToString());
            bodyDic["ctl00$PlaceHolderMain$HdnOldSiteAdmins"] = oldAdministrators;
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
            siteAdmins.Add(AveObjectModelConstant.ChildrenProperties, newAdministrators);
            return siteAdmins;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Ddlweb is a part of xml")]
        public Dictionary<string, object> UpdateWebRegionalSetting(string webServerRelativeUrl, Dictionary<string, object> regionalProp)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/regionalsetng.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            this.GetPostBody(postUrl, bodyDic);

            #region Local and Calendar
            int localId = 0;
            if (regionalProp.ContainsKey("LocaleId") || regionalProp.ContainsKey("Local"))
            {
                localId = regionalProp.ContainsKey("LocaleId") ? int.Parse(regionalProp["LocaleId"].ToString()) : int.Parse(regionalProp["Local"].ToString());
                bodyDic["ctl00%24PlaceHolderMain%24ctl00%24ctl00%24DdlwebLCID"] = localId;
                this.UpdateLocal(postUrl, html, bodyDic, regionalProp);
            }
            if (regionalProp.ContainsKey("CalendarType"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24ctl02%24ctl00%24DdlwebCalType"] = regionalProp["CalendarType"].ToString();
                this.UpdateCalendar(postUrl, html, bodyDic, regionalProp);
            }
            #endregion

            #region TimeZone and TimeFormat(更新Time需要先更新TimeZone和TimeFormat)
            if (regionalProp.ContainsKey("TimeZoneChangedProperties"))
            {
                Dictionary<string, object> timeZoneDic = regionalProp["TimeZoneChangedProperties"] as Dictionary<string, object>;
                if (timeZoneDic.ContainsKey("ID"))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl01%24ctl00%24DdlwebTimeZone"] = timeZoneDic["ID"].ToString();
                }
                else if (regionalProp.ContainsKey("TimeZoneId"))
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl01%24ctl00%24DdlwebTimeZone"] = regionalProp["TimeZoneId"].ToString();
                }
            }
            bodyDic["ctl00%24PlaceHolderMain%24ctl08%24ctl00%24DdlTimeFormat"] = 1;

            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl05%24RptControls%24BtnUpdateRegionalSettings";
            byte[] localData = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", localData, null);
            GetPostBody(postUrl, bodyDic);
            #endregion

            #region SortOrder and AlternateCalendar
            if (regionalProp.ContainsKey("Collation"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24ctl07%24ctl00%24DdlwebCollation"] = regionalProp["Collation"].ToString();
            }
            if (regionalProp.ContainsKey("ShowWeeks") && Convert.ToBoolean(regionalProp["ShowWeeks"].ToString()))
            {
                bodyDic["ctl00%24PlaceHolderMain%24ctl02%24ctl01%24ChkShowWeekNumber"] = "on";
            }
            if (regionalProp.ContainsKey("AdjustHijriDays"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24ctl02%24ctl02%24DdlwebHijriDays"] = regionalProp["AdjustHijriDays"].ToString();
            }
            if (regionalProp.ContainsKey("AlternateCalendarType"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24ctl03%24ctl00%24DdlwebAltCalType"] = regionalProp["AlternateCalendarType"].ToString();
            }
            #endregion

            #region WorkWeek and TimeFormat
            if (regionalProp.ContainsKey("WorkDays"))
            {
                short workDays = short.Parse(regionalProp["WorkDays"].ToString());
                if ((workDays & 64) > 0)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl04%24ctl00%24ChkListWeeklyMultiDays%240"] = "on";
                }
                if ((workDays & 32) > 0)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl04%24ctl00%24ChkListWeeklyMultiDays%241"] = "on";
                }
                if ((workDays & 16) > 0)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl04%24ctl00%24ChkListWeeklyMultiDays%242"] = "on";
                }
                if ((workDays & 8) > 0)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl04%24ctl00%24ChkListWeeklyMultiDays%243"] = "on";
                }
                if ((workDays & 4) > 0)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl04%24ctl00%24ChkListWeeklyMultiDays%244"] = "on";
                }
                if ((workDays & 2) > 0)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl04%24ctl00%24ChkListWeeklyMultiDays%245"] = "on";
                }
                if ((workDays & 1) > 0)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl04%24ctl00%24ChkListWeeklyMultiDays%246"] = "on";
                }
            }

            if (regionalProp.ContainsKey("FirstDayOfWeek"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24ctl04%24ctl01%24DdlFirstDayOfWeek"] = regionalProp["FirstDayOfWeek"].ToString();
            }
            if (regionalProp.ContainsKey("FirstWeekOfYear"))
            {
                bodyDic["ctl00%24PlaceHolderMain%24ctl04%24ctl01%24DdlFirstWeekOfYear"] = regionalProp["FirstWeekOfYear"].ToString();
            }

            if (regionalProp.ContainsKey("Time24"))
            {
                bool time24 = Convert.ToBoolean(regionalProp["Time24"].ToString());
                if (!time24)
                {
                    bodyDic["ctl00%24PlaceHolderMain%24ctl08%24ctl00%24DdlTimeFormat"] = 0;
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
                bodyDic["ctl00%24PlaceHolderMain%24ctl04%24ctl01%24DdlStartTime"] = startTime.ToString("HH:mm", info);
            }
            if (regionalProp.ContainsKey("WorkDayEndHour"))
            {
                int endHour = int.Parse(regionalProp["WorkDayEndHour"].ToString()) / 60;
                DateTime endTime = new DateTime(1, 1, 1, endHour, 0, 0);
                bodyDic["ctl00%24PlaceHolderMain%24ctl04%24ctl01%24DdlEndTime"] = endTime.ToString("HH:mm", info);
            }
            #endregion

            bodyDic["Cmd"] = "UPDATEPROJECT";
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl05%24RptControls%24BtnUpdateRegionalSettings";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", data, null, mTokenProvider);
            return this.GetWebRegionalSetting(webServerRelativeUrl);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Btn is a part of xml")]
        private Dictionary<string, object> GetPostBody(string postUrl, Dictionary<string, object> bodyDic)
        {
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Btn is a key")]
        private void UpdateLocal(string postUrl, string html, Dictionary<string, object> bodyDic, Dictionary<string, object> regionalProp)
        {
            bodyDic["Cmd"] = "UPDATEPROJECT";
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl00%24ctl00%24DdlwebLCID";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", data, null, mTokenProvider);
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl05%24RptControls%24BtnUpdateRegionalSettings";
            byte[] newData = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", newData, null, mTokenProvider);
            this.GetPostBody(postUrl, bodyDic);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Btn is a part of xml")]
        private void UpdateCalendar(string postUrl, string html, Dictionary<string, object> bodyDic, Dictionary<string, object> regionalProp)
        {
            bodyDic["Cmd"] = "UPDATEPROJECT";
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl02%24ctl00%24DdlwebCalType";
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", data, null, mTokenProvider);
            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl05%24RptControls%24BtnUpdateRegionalSettings";
            byte[] newData = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", newData, null, mTokenProvider);
            this.GetPostBody(postUrl, bodyDic);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Btn is a key")]
        public void UpdateSiteRssSetting(bool syndicationEnabled)
        {
            string netWorkUrl = mWebUrl + "/_layouts/siterss.aspx";
            string contentType = "application/x-www-form-urlencoded";
            string html = AveHttpWebRequestUtility.HttpGet(netWorkUrl, mObj, mTokenProvider);
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
                bodyDic["ctl00%24PlaceHolderMain%24SiteColRssSection%24ctl00%24CheckSiteColRss"] = "on";
            }
            byte[] data = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(netWorkUrl, mObj, contentType, data, null, mTokenProvider);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "In SharePoint property xml")]
        public Dictionary<string, object> UpdateKeyWord(string term, int localId, int calendarType, Dictionary<string, object> keyWordProp)
        {
            string url = mWebUrl.TrimEnd('/') + string.Format("/_layouts/Keyword.aspx?k={0}", term);
            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            string html = AveHttpWebRequestUtility.HttpGet(url, mObj, mTokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(url, mObj, "application/x-www-form-urlencoded", data, null, mTokenProvider);

            Dictionary<string, object> newKeyWordProp = new Dictionary<string, object>();
            newKeyWordProp = this.GetKeyWordProperties(term);
            return newKeyWordProp;
        }
        public void UpdateNavigationUseShared(string webServerRelativeUrl, bool useShared)
        {
            throw new NotImplementedException();
        }
        public void UpdateWorkflowAssociation(string webServerRelativeUrl, string listName, Guid listId, string ctId, Guid workflowAssociationId, string workflowSource, Dictionary<string, object> needUpdateWorkflowProperties)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Recycle
        public Guid RecycleItem(string webRelativeUrl, string listRelativeUrl, string listTitle, Guid listId, int itemId)
        {
            throw new NotImplementedException();
        }

        public Guid RecycleList(string webRelativeUrl, string listTitle, Guid listId)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Set
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Btn is a part of xml")]
        public void SetSiteEnabledHelpCollections(string[] enabledHelpCollections)
        {
            string postUrl = mWebUrl.TrimEnd('/') + "/_layouts/HelpSettings.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Rad is a part of xml")]
        public bool SetListRating(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/RatingsSettings.aspx?List=" + listId.ToString("B");
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
            bodyDic["ctl00$PlaceHolderSearchArea$ctl01$ctl00"] = mWebUrl;
            bodyDic["ctl00$PlaceHolderSearchArea$ctl01$ctl01"] = listUrl;
            bodyDic["ctl00$PlaceHolderSearchArea$ctl01$ctl05"] = 0;
            bodyDic["ctl00$PlaceHolderMain$ctl01$RptControls$BtnSave"] = "OK";
            if (enableRating)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl02$EnableRatings"] = "RadEnableRatingsYes";
            }
            else
            {
                bodyDic["ctl00$PlaceHolderMain$ctl00$ctl02$EnableRatings"] = "RadEnableRatingsNo";
            }
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
            return true;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Lst is a part of xml")]
        public void SetMetadataNavigationSettings(string webServerRelativeUrl, string listTitle, Guid listId, Dictionary<string, object> updateProperties)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/MetaNavSettings.aspx?List=" + listId.ToString("B");
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Btn is a part of xml")]
        public void SetListVersionLimited(string webServerRelativeUrl, Guid listId, Dictionary<string, object> versionLimitedProperties)
        {
            mRequestCommon.SetListVersionLimited(webServerRelativeUrl, listId, versionLimitedProperties);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Btn is a part of xml")]
        public void SetPerLocalViewSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> viewSettingProp)
        {
            string postUrl = WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/MetaNavPerNode.aspx?List=" + listId.ToString("B");
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
            XmlDocument xmlDoc = new XmlDocument();
            searchContent = "<input id=\"ctl00_PlaceHolderMain_ctl01_Picker_data\"";
            string information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</input>");
            xmlDoc.LoadXml(information);
            bodyDic["ctl00$PlaceHolderMain$ctl01$Picker$data"] = xmlDoc.FirstChild.Attributes["value"].Value;
            searchContent = "<input id=\"ctl00_PlaceHolderMain_ctl01_Picker_initial\"";
            information = AveHttpWebRequestUtility.GetInput(html, searchContent, "</input>");
            xmlDoc.LoadXml(information);
            bodyDic["ctl00$PlaceHolderMain$ctl01$Picker$initial"] = xmlDoc.FirstChild.Attributes["value"].Value;
            bodyDic["ctl00$PlaceHolderMain$ctl01$Picker"] = viewSettingProp["picker"];
            bodyDic["ctl00$PlaceHolderMain$ctl02$RptControls$btnOK"] = "OK";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "scopedisplaygroup is a part of url")]
        public Dictionary<string, object> CreateScopeDisPlayGroup(string name, string description, Uri owningSiteUrl, bool displayInAdminUI)
        {
            string postUrl = mWebUrl.TrimEnd('/') + "/_layouts/scopedisplaygroup.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
            bodyDic["ctl00$PlaceHolderMain$titleTextBox"] = name;
            bodyDic["ctl00$PlaceHolderMain$descriptionTextBox"] = description;
            bodyDic["ctl00$PlaceHolderMain$okButton"] = "OK";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
            Dictionary<string, object> groupProp = GetNewGroupProp(name);
            return groupProp;
        }
        private Dictionary<string, object> GetNewGroupProp(string name)
        {
            Dictionary<string, object> groupProp = new Dictionary<string, object>();
            Dictionary<string, object> tempGroupProp = new Dictionary<string, object>();
            GetScopeDisplayGroupsID(tempGroupProp);
            groupProp["Name"] = name;
            groupProp["ID"] = Convert.ToInt32(tempGroupProp[name]);
            groupProp["Scopes"] = new List<Dictionary<string, object>>();
            groupProp["Default"] = null;
            return groupProp;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "aaa is a value")]
        public Dictionary<string, object> CreateScope(string name, string description, Uri owningSiteUrl, bool displayInAdminUI, string alternateResultsPage, string compilationType, string filter)
        {
            string postUrl = mWebUrl.TrimEnd('/') + "/_layouts/scope.aspx";
            string html = AveHttpWebRequestUtility.HttpGet(postUrl, mObj, mTokenProvider);
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
            bodyDic["ctl00$PlaceHolderMain$titleTextBox"] = name;
            bodyDic["ctl00$PlaceHolderMain$descriptionTextBox"] = description;
            if (string.IsNullOrEmpty(alternateResultsPage))
            {
                bodyDic["ctl00$PlaceHolderMain$targetPageButtonGroup"] = "useDefaultRadioButton";
            }
            else
            {
                bodyDic["ctl00$PlaceHolderMain$targetPageButtonGroup"] = "specifyPageRadioButton";
                bodyDic["ctl00$PlaceHolderMain$targetResultsPageTextBox"] = "aaa";
            }
            bodyDic["ctl00$PlaceHolderMain$okButton"] = "OK";
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, mObj, "application/x-www-form-urlencoded", body, null, mTokenProvider);
            Dictionary<string, object> scopeProprties = GetScopeProperties(name);
            return scopeProprties;
        }
        private Dictionary<string, object> GetScopeProperties(string scopeName)
        {
            List<Dictionary<string, object>> allProperties = new List<Dictionary<string, object>>();
            GetScopeDisplayGroups(allProperties);
            foreach (Dictionary<string, object> groupProp in allProperties)
            {
                if (groupProp["Name"].ToString().Equals("Unused Scopes"))
                {
                    List<Dictionary<string, object>> scopes = groupProp["Scopes"] as List<Dictionary<string, object>>;
                    foreach (Dictionary<string, object> scopeProp in scopes)
                    {
                        if (scopeProp["Name"].ToString().Equals(scopeName))
                        {
                            return scopeProp;
                        }
                    }
                }
            }
            return new Dictionary<string, object>();

        }

        //public Dictionary<string, string> SetCustomProperty(Guid termStoreId, Guid termSetId, Guid termId, string name, string value, AveTermSetItemType type)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, string> SetLocalCustomProperty(Guid termStoreId, Guid termSetId, Guid termId, string name, string value)
        //{
        //    throw new NotImplementedException();
        //}
        //public Dictionary<string, object> SetWebNavigationSettings(string webServerRelativeUrl, int globalSource, int currentSource, Dictionary<string, Guid> globalTaxonomy, Dictionary<string, Guid> currentTaxonomy)
        //{
        //    throw new NotImplementedException();
        //}
        #endregion

        #region Private method
        //private void GetSubWebs(List<Dictionary<string, object>> webs, string webServerRelativeUrl, string siteUrl)
        //{
        //    Dictionary<string, object> webProperties = this.GetWeb(webServerRelativeUrl);
        //    webs.Add(webProperties);
        //    Dictionary<string, object> subWebsProperties = this.GetSubWebs(webServerRelativeUrl);
        //    foreach (Dictionary<string, object> dic in subWebsProperties["WebCollection"] as List<Dictionary<string, object>>)
        //    {
        //        this.GetSubWebs(webs, dic["Url"] as string, siteUrl);
        //    }
        //}
        private void AllDataToDictionary(Dictionary<string, object> properties, object[] data)
        {
            foreach (object obj in data)
            {
                Type typ = obj.GetType();
                foreach (PropertyInfo property in typ.GetProperties())
                {
                    try
                    {
                        properties.Add(property.Name.ToString(), property.GetValue(obj, null));
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn("All Data to Dictionary Failed.Error Message{0}.", ex.ToString());
                    }
                }
            }
        }
        private Dictionary<string, object> GetVersionsFromXmlNode(XmlNode node)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
            returnInfo[AveObjectModelConstant.ChildrenProperties] = versions;
            if (node == null)
            {
                return returnInfo;
            }
            else
            {
                foreach (XmlNode n in node.ChildNodes)
                {
                    if (!n.Name.Equals("result"))
                    {
                        continue;
                    }
                    Dictionary<string, object> version = new Dictionary<string, object>();
                    foreach (XmlAttribute attri in n.Attributes)
                    {
                        switch (attri.Name)
                        {
                            case "created":
                                version["Created"] = DateTime.Parse(attri.Value.ToString());
                                break;
                            case "size":
                                version["StreamLength"] = long.Parse(attri.Value.ToString());
                                break;
                            case "version":
                                if (attri.Value.Contains("@"))
                                {
                                    version["IsCurrentVersion"] = true;
                                    version["VersionLabel"] = attri.Value.Substring(1);
                                }
                                else
                                {
                                    version["IsCurrentVersion"] = false;
                                    version["VersionLabel"] = attri.Value.ToString();
                                }
                                break;
                            default:
                                version[attri.Name] = attri.Value.ToString();
                                break;
                        }
                    }
                    versions.Add(version);
                }
                return returnInfo;
            }
        }
        private Dictionary<string, object> GetListAttributeFromXmlNode(XmlNode node)
        {
            List<Dictionary<string, object>> fields = new List<Dictionary<string, object>>();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (!child.Name.Equals("Fields"))
                {
                    continue;
                }
                //list fields
                foreach (XmlNode childNode in child.ChildNodes)
                {
                    Dictionary<string, object> fieldProperties = new Dictionary<string, object>();
                    foreach (XmlAttribute attri in childNode.Attributes)
                    {
                        switch (attri.Name)
                        {
                            case "ID":
                                fieldProperties["ID"] = new Guid(attri.Value.ToString());
                                break;
                            default:
                                fieldProperties[attri.Name] = attri.Value;
                                break;
                        }
                    }
                    fields.Add(fieldProperties);
                }
            }
            Dictionary<string, object> listPropeties = new Dictionary<string, object>();
            listPropeties.Add("ChildrenProperties", fields);
            //list attribute
            foreach (XmlAttribute attri in node.Attributes)
            {
                switch (attri.Name)
                {
                    case "ID":
                    case "FeatureId":
                    case "WebId":
                    case "ScopeId":
                        if (!string.IsNullOrEmpty(attri.Value))
                        {
                            listPropeties[attri.Name] = new Guid(attri.Value);
                        }
                        break;
                    case "RootFolder":
                        listPropeties["ListRootFolderUrl"] = attri.Value.ToString();
                        break;
                    case "ServerTemplate":
                    case "BaseType":
                    case "Version":
                    case "Flags":
                    case "ItemCount":
                    case "AnonymousPermMask":
                    case "ReadSecurity":
                    case "WriteSecurity":
                    case "Author":
                    case "MajorVersionLimit":
                    case "MajorWithMinorVersionsLimit":
                    case "MaxItemsPerThrottledOperation":
                        listPropeties[attri.Name] = attri.Value;
                        break;
                    case "Created":
                    case "Modified":
                    case "LastDeleted":
                        if (!string.IsNullOrEmpty(attri.Value))
                        {
                            listPropeties[attri.Name] = DateTime.Parse(attri.Value.Insert(4, " ").Insert(7, " ").Insert(10, " "));
                        }
                        break;
                    default:
                        if (attri.Value.Equals("False") || attri.Value.Equals("True"))
                        {
                            listPropeties[attri.Name] = bool.Parse(attri.Value);
                        }
                        else
                        {
                            listPropeties[attri.Name] = attri.Value;
                        }
                        break;
                }
            }
            return listPropeties;
        }
        private Dictionary<string, object> GetContentTypesAttributeFromXmlNode(XmlNode node)
        {
            List<Dictionary<string, object>> contentTypes = new List<Dictionary<string, object>>();
            if (node.HasChildNodes)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    Dictionary<string, object> contentTypeProperties = new Dictionary<string, object>();
                    if (child.Name.Equals("ContentType"))
                    {
                        foreach (XmlAttribute attri in child.Attributes)
                        {
                            switch (attri.Name)
                            {
                                case "ID":
                                    contentTypeProperties.Add("Id" + AveObjectModelConstant.ObjectPropertySuffix, attri.Value.ToString());
                                    break;
                                default:
                                    contentTypeProperties.Add(attri.Name.ToString(), attri.Value.ToString());
                                    break;
                            }
                        }
                    }
                    contentTypes.Add(contentTypeProperties);
                }
            }
            Dictionary<string, object> contentType = new Dictionary<string, object>();
            contentType.Add("ChildrenProperties", contentTypes);
            return contentType;

        }
        private Dictionary<string, object> GetContentTypeAttributeFromXmlNode(XmlNode node)
        {
            Dictionary<string, object> contentTypeProperties = new Dictionary<string, object>();
            List<Dictionary<string, object>> fieldProperties = new List<Dictionary<string, object>>();
            if (node != null)
            {
                foreach (XmlAttribute attri in node.Attributes)
                {
                    switch (attri.Name)
                    {
                        case "FeatureId":
                            contentTypeProperties[attri.Name] = new Guid(attri.Value.ToString());
                            break;
                        default:
                            contentTypeProperties[attri.Name] = attri.Value.ToString();
                            break;
                    }
                }
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (!child.Name.Equals("Fields"))
                    {
                        continue;
                    }
                    foreach (XmlNode childNode in child.ChildNodes)
                    {
                        Dictionary<string, object> fieldP = new Dictionary<string, object>();
                        if (!childNode.Name.Equals("Field"))
                        {
                            continue;
                        }
                        foreach (XmlAttribute a in childNode.Attributes)
                        {
                            switch (a.Name)
                            {
                                case "ID":
                                    fieldP.Add("Id", new Guid(a.Value.ToString()));
                                    break;
                                case "ReadOnly":
                                case "Hidden":
                                case "FromBaseType":
                                    fieldP.Add(a.Name, bool.Parse(a.Value.ToString()));
                                    break;
                                case "Type":
                                    fieldP.Add(a.Name, ConvertType(a.Value.ToString()));
                                    break;
                                case "Name":
                                case "DisplayName":
                                case "SourceID":
                                case "StaticName":
                                case "ColName":
                                    fieldP.Add(a.Name, a.Value.ToString());
                                    break;
                                default:
                                    break;
                            }
                        }
                        fieldProperties.Add(fieldP);
                    }
                }
            }
            //return fields dictionary list with "ChildrenProperties" key
            contentTypeProperties.Add(AveObjectModelConstant.ChildrenProperties, fieldProperties);
            return contentTypeProperties;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "rs:Special characters of xml attribute.")]
        private void GetItemAttributeFromXmlNode(XmlNode node, Dictionary<string, object> dic)
        {
            List<Dictionary<string, object>> items = new List<Dictionary<string, object>>();
            bool itemExist = false;
            if (node != null && node.HasChildNodes)
            {
                XmlNode rsData = null;
                foreach (XmlNode n in node.ChildNodes)
                {
                    if (n.Name.Equals("rs:data"))
                    {
                        rsData = n;
                        break;
                    }
                }
                if (rsData == null)
                {
                    throw new Exception("Null information return.");
                }
                if (rsData.ChildNodes.Count == 0)
                {
                    //no items will throw exception before invoke this operation
                    itemExist = false;
                }
                else
                {
                    //child nodes
                    foreach (XmlNode n in rsData.ChildNodes)
                    {
                        Dictionary<string, object> itemProperties = new Dictionary<string, object>();
                        //file item need the "Exists" property
                        if (!n.Name.Equals("z:row"))
                        {
                            continue;
                        }
                        itemExist = true;
                        itemProperties.Add("Exists", itemExist);
                        foreach (XmlAttribute attribute in n.Attributes)
                        {
                            string name = string.Empty;
                            string value = string.Empty;
                            switch (attribute.Name)
                            {
                                case "ows_Author":
                                case "ows_Editor":
                                    name = GetListItemAttributeName(attribute.Name);
                                    string[] separates = attribute.Value.Split(new char[] { ';', '#' });
                                    itemProperties[name + "Id"] = separates[0];
                                    itemProperties[name + "Name"] = separates[2];
                                    break;
                                case "ows_File_x0020_Size":
                                    value = GetListItemAttributeValue(attribute.Value);
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        itemProperties["Length"] = long.Parse(value);
                                    }
                                    else
                                    {
                                        itemProperties["Length"] = 0;
                                    }
                                    break;
                                case "ows_FileLeafRef":
                                    value = GetListItemAttributeValue(attribute.Value);
                                    itemProperties["Name"] = value;
                                    break;
                                case "ows_ServerUrl":
                                    value = GetListItemAttributeValue(attribute.Value);
                                    itemProperties["ServerRelativeUrl"] = value;
                                    break;
                                case "ows_Created":
                                    value = GetListItemAttributeValue(attribute.Value);
                                    itemProperties["TimeCreated"] = DateTime.Parse(value);
                                    break;
                                case "ows_Modified":
                                    value = GetListItemAttributeValue(attribute.Value);
                                    itemProperties["TimeLastModified"] = DateTime.Parse(value);
                                    break;
                                case "ows_LinkTitle":
                                    value = GetListItemAttributeValue(attribute.Value);
                                    itemProperties["DisplayName"] = value;
                                    break;
                                case "ows_Title":
                                    name = GetListItemAttributeName(attribute.Name);
                                    value = GetListItemAttributeValue(attribute.Value);
                                    itemProperties[name] = value;
                                    break;
                                case "ows_FSObjType":
                                    name = GetListItemAttributeName(attribute.Name);
                                    value = GetListItemAttributeValue(attribute.Value);
                                    itemProperties["FileSystemObjectType"] = (AveFileSystemObjectType)int.Parse(value);
                                    break;
                                /////
                                case "ows_Last_x0020_Modified":
                                case "ows_Created_x0020_Date":
                                    name = GetListItemAttributeName(attribute.Name);
                                    value = GetListItemAttributeValue(attribute.Value);
                                    itemProperties[name] = DateTime.Parse(value);
                                    break;
                                case "ows_UniqueId":
                                case "ows_ScopeId":
                                case "ows_GUID":
                                    name = GetListItemAttributeName(attribute.Name);
                                    value = GetListItemAttributeValue(attribute.Value);
                                    itemProperties[name] = new Guid(value);
                                    break;
                                case "ows__Level":
                                    name = GetListItemAttributeName(attribute.Name);
                                    value = GetListItemAttributeValue(attribute.Value);
                                    itemProperties[name] = (AveFileLevel)int.Parse(value);
                                    break;
                                //case "MajorVersion":
                                //case "MinorVersion":
                                case "ows__UIVersion":
                                    name = GetListItemAttributeName(attribute.Name);
                                    value = GetListItemAttributeValue(attribute.Value);
                                    itemProperties[name] = int.Parse(value);
                                    break;
                                case "ows_ID":
                                    value = GetListItemAttributeValue(attribute.Value);
                                    itemProperties["Id"] = int.Parse(value);
                                    break;
                                case "ows_ContentType":
                                    name = GetListItemAttributeName(attribute.Name);
                                    value = GetListItemAttributeValue(attribute.Value);
                                    //add suffix to avoid conflicting with item attribute "ContentType" key
                                    itemProperties[name + AveObjectModelConstant.ObjectPropertySuffix] = value;
                                    break;
                                default:
                                    name = GetListItemAttributeName(attribute.Name);
                                    value = GetListItemAttributeValue(attribute.Value);
                                    itemProperties[name] = value;
                                    break;
                            }
                        }
                        items.Add(itemProperties);
                    }
                }
            }
            dic.Add(AveObjectModelConstant.ChildrenProperties, items);
        }

        public void DeleteSite(string CAUrl, string url)
        {
            using (AveWebServiceNetWork network = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                network.InitialNetWorker(AveWebServiceType.Admin, CAUrl);
                network.DeleteSite(url);
            }
        }

        /// <summary>
        /// handle the attribute of string type
        /// </summary>
        /// <param name="node"></param>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        private Dictionary<string, object> AddContentFromXmlNode(XmlNode node, string keyValue)
        {
            List<Dictionary<string, object>> properties = new List<Dictionary<string, object>>();
            if (node != null && node.HasChildNodes)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    Dictionary<string, object> singleProperties = new Dictionary<string, object>();
                    foreach (XmlAttribute attribute in child.Attributes)
                    {
                        if (attribute.Name.Equals("ID"))
                        {
                            singleProperties.Add("Id" + AveObjectModelConstant.ObjectPropertySuffix, attribute.Value.ToString());
                        }
                        else
                        {
                            singleProperties.Add(attribute.Name, attribute.Value.ToString());
                        }
                    }
                    properties.Add(singleProperties);
                }
            }
            Dictionary<string, object> returnInfor = new Dictionary<string, object>();
            returnInfor.Add(keyValue, properties);
            return returnInfor;
        }
        private Dictionary<string, object> GetAttributeFromSingleXmlNode(XmlNode node)
        {
            Dictionary<string, object> returnInfo = new Dictionary<string, object>();
            if (node == null)
            {
                return returnInfo;
            }
            else
            {
                foreach (XmlAttribute attri in node.Attributes)
                {
                    switch (attri.Name)
                    {
                        case "Language":
                            uint language = 0;
                            if (uint.TryParse(attri.Value, out language))
                            {
                                returnInfo[attri.Name] = language;
                            }
                            break;
                        case "FarmId":
                        case "Id":
                            returnInfo[attri.Name] = new Guid(attri.Value);
                            break;
                        case "ExcludeFromOfflineClient":
                        case "CellStorageWebServiceEnabled":
                            bool convertValue = false;
                            if (bool.TryParse(attri.Value, out convertValue))
                            {
                                returnInfo[attri.Name] = convertValue;
                            }
                            break;
                        default:
                            returnInfo.Add(attri.Name, attri.Value);
                            break;
                    }
                }
                return returnInfo;
            }
        }
        private void ObjectToDicValue(Dictionary<string, object> DicProperties, object Object)
        {

            foreach (PropertyInfo property in Object.GetType().GetProperties())
            {
                DicProperties.Add(property.Name, property.GetGetMethod().Invoke(Object, null));
            }
        }
        private void ObjectToDicValue(Dictionary<string, object> DicProperties, object Object, Type type)
        {
            object srcValue = null;
            Type srcType = Object.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                string propertyName = String.Empty;
                propertyName = property.Name;
                switch (property.Name)
                {
                    case "Id":
                        propertyName = "WebID";
                        break;
                    default:
                        break;
                }
                PropertyInfo srcProperty = srcType.GetProperty(propertyName);
                if (srcProperty != null)
                {
                    srcValue = srcProperty.GetGetMethod().Invoke(Object, null);
                    if (srcProperty.PropertyType != property.PropertyType && srcValue != null)
                    {
                        switch (propertyName)
                        {
                            case "WebID":
                                srcValue = new Guid(srcValue.ToString());
                                break;
                            case "Language":
                                srcValue = (uint)uint.Parse(srcValue.ToString());
                                break;
                            default:
                                break;
                        }
                        DicProperties.Add(property.Name, srcValue);
                    }
                    else
                    {
                        DicProperties.Add(property.Name, srcValue);
                    }
                }

            }
        }
        private void StringToDicValue(Dictionary<string, object> DicProperties, string strInfo, Type type)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(strInfo);
                if (xmlDoc != null)
                {
                    XmlElement xmlEle = xmlDoc.DocumentElement;
                    XmlNodeToDicValue(DicProperties, (XmlNode)xmlEle);
                }
            }
            catch (Exception ex)
            {
                mLogger.Debug(AveWebServiceRequestResource.XmlStringToDictionaryError, strInfo, ex.ToString());
            }
        }
        private void XmlNodeToDicValue(Dictionary<string, object> DicProperties, XmlNode xmlNodeInfo)
        {
            if (xmlNodeInfo.Attributes != null)
            {
                foreach (XmlAttribute xmlAbt in xmlNodeInfo.Attributes)
                {
                    object objValue = null;
                    string propertyName = String.Empty;
                    objValue = xmlAbt.Value;
                    propertyName = xmlAbt.Name;
                    if (propertyName.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    {
                        propertyName = "Id";
                    }
                    switch (propertyName)
                    {
                        case "Id":
                            //case "FeatureId":
                            objValue = new Guid(objValue.ToString());
                            break;
                        case "Created":
                        case "Modified":
                        case "LastDeleted":
                            objValue = DateTime.Parse((objValue.ToString().Insert(4, " ").Insert(7, " ").Insert(10, " ")));
                            break;
                        case "BaseType":
                        case "BaseTemplate":
                        case "FileSystemObjectType":
                        case "ContentType":
                        case "Level":
                        case "ItemCount":
                        case "AnonymousPermMask":
                        case "ReadSecurity":
                        case "WriteSecurity":
                        case "MajorVersionLimit":
                        case "MajorWithMinorVersionsLimit":
                            objValue = int.Parse(objValue.ToString());
                            break;
                        case "AllowDeletion":
                        case "AllowMultiResponses":
                        case "EnableAttachments":
                        case "EnableModeration":
                        case "EnableVersioning":
                        case "HasExternalDataSource":
                        case "Hidden":
                        case "MultipleDataList":
                        case "Ordered":
                        case "ShowUser":
                        case "EnablePeopleSelector":
                        case "RequireCheckout":
                        case "ExcludeFromOfflineClient":
                        case "EnableFolderCreation":
                        case "IrmEnabled":
                        case "IsApplicationList":
                        case "EnforceDataValidation":
                            objValue = bool.Parse(objValue.ToString());
                            break;
                        case "Language":
                            objValue = uint.Parse(objValue.ToString());
                            break;
                    }
                    if (propertyName.Equals("RootFolder"))
                    {
                        continue;
                    }
                    DicProperties.Add(propertyName, objValue);
                }
            }
            if (xmlNodeInfo.HasChildNodes)
            {
                List<Dictionary<string, object>> listsProperites = new List<Dictionary<string, object>>(xmlNodeInfo.ChildNodes.Count);
                foreach (XmlNode xmlSubNode in xmlNodeInfo.ChildNodes)
                {
                    Dictionary<string, object> SubDicProperties = new Dictionary<string, object>();
                    XmlNodeToDicValue(SubDicProperties, xmlSubNode);
                    listsProperites.Add(SubDicProperties);
                }
                DicProperties.Add(AveObjectModelConstant.ChildrenProperties, listsProperites);
            }
        }
        private string GetServerRelativeUrl(string objectUrl)
        {
            int index = objectUrl.IndexOf("://", 0, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                throw new Exception(string.Format("illegal format:{0}", objectUrl));
            }
            index = objectUrl.IndexOf("/", index + 3, StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                return objectUrl.Substring(index);
            }
            else
            {
                return "/";
            }
        }
        private string GetServerUrl(string url)
        {
            int index = url.IndexOf("://", StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                throw new Exception(string.Format("illegal format:{0} ", url));
            }
            index = url.IndexOf('/', index + 3);
            if (index > 0)
            {
                return url.Substring(0, index);
            }
            else
            {
                return url;
            }
        }
        private string GetWebAppNameFromSiteUrl(string siteUrl)
        {
            int indexOfSlash = siteUrl.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase);
            string webAppName = siteUrl;
            if (indexOfSlash != -1)
            {
                webAppName = siteUrl.Substring(0, indexOfSlash);
            }
            return webAppName;
        }
        private List<Dictionary<string, object>> GetAttachmentCollectionInfo(XmlNode node, string webAppName)
        {
            List<Dictionary<string, object>> attachmentPropertiesList = new List<Dictionary<string, object>>();
            if (node != null)
            {
                foreach (XmlNode child in node)
                {
                    Dictionary<string, object> attachmentProperties = new Dictionary<string, object>();
                    string attachUrl = child.InnerText;
                    attachmentProperties.Add("Url", child.InnerText.Substring(webAppName.Length));
                    //attachmentProperties.Add("FileName", attachUrl.Substring(attachUrl.LastIndexOf('/')));
                    attachmentPropertiesList.Add(attachmentProperties);
                }
            }
            return attachmentPropertiesList;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Whereabout:Type")]
        [SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", MessageId = "SendTo", Justification = "SharePoint Property")]
        private static object GetValueFromType(string vtype, string svalue, CultureInfo cultureInfo = null)
        {
            switch (vtype)
            {
                case "BusinessData":
                case "Choice":
                case "ContactInfo":
                case "ContentTypeId":
                case "FreeBusy":
                case "HTML":
                case "Note":
                case "Overbook":
                case "Text":
                case "URL":
                case "Whereabout":
                case "Image":
                case "WorkflowStatus":
                case "OutcomeChoice"://2013 new field, taskoutcome column
                case "GridChoice"://Rating Scale
                case "ThreadIndex":
                case "Link":
                case "Calculated":
                case "CallTo":
                case "SendTo":
                case "Lookup":
                case "LookupMulti":
                case "User":
                case "UserMulti":
                case "SummaryLinks":
                case "TaxonomyFieldType":
                case "TaxonomyFieldTypeMulti":
                case "MediaFieldType":
                case "MultiChoice":
                case "TargetTo":
                    return svalue;
                case "DateTime":
                case "PublishingScheduleStartDateFieldType":
                case "PublishingScheduleEndDateFieldType":
                    return GetDateTimeObject(svalue, cultureInfo);
                case "AverageRating":
                case "Currency":
                case "Number":
                case "RatingCount":
                    return double.Parse(svalue);
                case "Guid":
                    return new Guid(svalue);
                case "AllDayEvent":
                case "Boolean":
                case "Recurrence":
                case "CrossProjectLink"://calendar中event关联workspace 对应的column为该类型
                    return bool.Parse(svalue);
                case "Counter":
                case "Integer":
                case "ModStat":
                    return int.Parse(svalue);
                case "Computed":
                    break;
                default:
                    break;
            }
            return null;
        }

        private static object GetDateTimeObject(string timeLabel, CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty(timeLabel))
            {
                return null;
            }
            if (timeLabel.Contains("T") && timeLabel.Contains("Z"))
            {
                return DateTime.Parse(timeLabel, new CultureInfo("en-US", false)).ToUniversalTime();
            }
            try
            {
                return DateTime.Parse(timeLabel, cultureInfo);
            }
            catch (FormatException ex)
            {
                mLogger.Log(AveLogLevel.DEBUG, string.Format("Failed to analyze DateTime value.Format:[{0}],Message:{1}.", timeLabel, ex.ToString()));
                return null;
            }
        }

        private string GetListItemAttributeName(string attributeName)
        {
            const string prefix = "ows_";
            int index = attributeName.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return attributeName;
            }
            else
            {
                index += prefix.Length;
                //while (attributeName[index] == '_')
                //{
                //    index++;
                //}
                return attributeName.Substring(index);
            }
        }
        private string GetListItemAttributeValue(string attributeValue)
        {
            int index = attributeValue.IndexOf("#", StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return attributeValue;
            }
            else
            {
                return attributeValue.Substring(++index);
            }
        }
        private AveFieldType ConvertType(string type)
        {
            foreach (FieldInfo field in typeof(AveFieldType).GetFields())
            {
                if (field.Name.Equals(type))
                {
                    return (AveFieldType)field.GetValue(AveFieldType.Invalid);
                }
            }
            return AveFieldType.Invalid;
        }

        private Dictionary<string, object> GetUserDic(XmlNode node)
        {
            Dictionary<string, object> userProperties = new Dictionary<string, object>();
            XmlNode user = node.FirstChild;
            foreach (XmlAttribute attri in user.Attributes)
            {
                switch (attri.Name)
                {
                    case "ID":
                        userProperties.Add("Id", Convert.ToInt32(attri.Value));
                        break;
                    case "Sid":
                        userProperties.Add("SID", attri.Value);
                        break;
                    case "IsSiteAdmin":
                        userProperties.Add("IsSiteAdmin", Convert.ToBoolean(attri.Value));
                        break;
                    case "IsDomainGroup":
                        userProperties.Add("IsDomainGroup", Convert.ToBoolean(attri.Value));
                        break;
                    case "LoginName":
                        userProperties.Add("LoginName", attri.Value);
                        break;
                    case "Name":
                        userProperties.Add("Name", attri.Value);
                        break;
                    case "Email":
                        userProperties.Add("Email", attri.Value);
                        break;
                    case "Notes":
                        userProperties.Add("Notes", attri.Value);
                        break;
                    default:
                        break;
                }
            }
            return userProperties;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Special characters of solution's field xml.")]
        private void InputColor(Dictionary<string, object> bodyDic, AveWebThemeInfo themeInfo)
        {
            bodyDic["ctl00$PlaceHolderMain$ctl82$customizeThemeSection$dark1"] = themeInfo.DarkColor1;
            bodyDic["ctl00$PlaceHolderMain$ctl82$customizeThemeSection$light1"] = themeInfo.LightColor1;
            bodyDic["ctl00$PlaceHolderMain$ctl82$customizeThemeSection$dark2"] = themeInfo.DarkColor2;
            bodyDic["ctl00$PlaceHolderMain$ctl82$customizeThemeSection$light2"] = themeInfo.LightColor2;
            bodyDic["ctl00$PlaceHolderMain$ctl82$customizeThemeSection$accent1"] = themeInfo.AccentColor1;
            bodyDic["ctl00$PlaceHolderMain$ctl82$customizeThemeSection$accent2"] = themeInfo.AccentColor2;
            bodyDic["ctl00$PlaceHolderMain$ctl82$customizeThemeSection$accent3"] = themeInfo.AccentColor3;
            bodyDic["ctl00$PlaceHolderMain$ctl82$customizeThemeSection$accent4"] = themeInfo.AccentColor4;
            bodyDic["ctl00$PlaceHolderMain$ctl82$customizeThemeSection$accent5"] = themeInfo.AccentColor5;
            bodyDic["ctl00$PlaceHolderMain$ctl82$customizeThemeSection$accent6"] = themeInfo.AccentColor6;
            bodyDic["ctl00$PlaceHolderMain$ctl82$customizeThemeSection$hlink"] = themeInfo.HyperlinkColor;
            bodyDic["ctl00$PlaceHolderMain$ctl82$customizeThemeSection$folHlink"] = themeInfo.FollowedHyperlinkColor;
            bodyDic["ctl00$PlaceHolderMain$ctl82$customizeThemeSection$font1"] = themeInfo.MajorFont;
            bodyDic["ctl00$PlaceHolderMain$ctl82$customizeThemeSection$font2"] = themeInfo.MinorFont;
        }

        private CultureInfo GetCultureWithCalendar(int localId, int calendarType)
        {
            CultureInfo info = null;
            switch ((AveCalendarType)calendarType)
            {
                case AveCalendarType.Gregorian:
                    info = new CultureInfo(localId);//new CultureInfo(1033);
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

        #endregion

        public void Dispose()
        {
            //mNetWork.Dispose();
        }

        #region Serializer

        public AveSiteInfo GetSiteInfo()
        {
            throw new NotImplementedException();
        }

        public AveSiteSettingInfo GetSiteSettingInfo()
        {
            throw new NotImplementedException();
        }

        public AveUserInfo GetUserInfo(int principalId)
        {
            throw new NotImplementedException();
        }

        public AveGroupInfo GetGroupInfo(int principalId)
        {
            throw new NotImplementedException();
        }

        public List<AveUserInfo> GetSiteUsers(bool allAvailableUser)
        {
            throw new NotImplementedException();
        }

        public List<AveTermStoreInfo> GetMetadataServiceData()
        {
            throw new NotImplementedException();
        }

        public AveWebInfo GetWebInfo(string webServerRelativeUrl)
        {
            throw new NotImplementedException();
        }

        public List<AveGroupInfo> GetGroups(string webServerRelativeUrl, bool allGroups)
        {
            throw new NotImplementedException();
        }

        public List<AveRoleAssignmentInfo> GetRoleAssignments(Guid siteId, Guid scopeId)
        {
            throw new NotImplementedException();
        }

        public List<AveRoleInfo> GetRoles(string webServerRelativeUrl)
        {
            throw new NotImplementedException();
        }

        public List<AveUserInfo> GetWebUsers(string webServerRelativeUrl, bool allAvailableUser)
        {
            throw new NotImplementedException();
        }

        public AveWebSettingInfo GetWebSettingInfo(string webServerRelativeUrl)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Discovery Query

        public bool IsHaveSameName(Guid webId, Guid listId, string dirName, string leafName)
        {
            return false;
        }

        public Dictionary<string, object> QueryListItemForFB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, bool isDiscover, bool includeSystemFolder)
        {
            Dictionary<string, object> AllItems = new Dictionary<string, object>();
            return AllItems;
        }

        public Dictionary<string, object> QueryListItemForIB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, Dictionary<string, object> changeItemsCache)
        {
            throw new NotImplementedException();
        }

        public Guid GetListItemGuid(Guid webId, Guid listId, Guid tp_Guid, int rowId)
        {
            return Guid.Empty;
        }

        public Guid GetDocIdByTp_Guid(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid tp_Guid, int rowId)
        {
            return Guid.Empty;
        }

        public int GetSiteChangedForIB(Guid siteId, DateTime startTime, DateTime endTime, Dictionary<string, object> changeCache)
        {
            return 0;
        }

        public Dictionary<Guid, object> QueryWebForIB(Dictionary<Guid, object> changedWebsInfo)
        {
            return null;
        }

        public Dictionary<int, object> QuerySiteSecurityForIB(Guid siteId, DateTime startTime, DateTime endTime)
        {
            return null;
        }

        public Dictionary<Guid, Dictionary<string, object>> GetSubWebsBasicInfo(string siteUrl, Guid parentWebId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Guid, object> QueryListForIB(Guid webId, Dictionary<Guid, object> changedListCache)
        {
            return null;
        }

        public Dictionary<string, object> QueryListRootFolder(Guid siteId, Guid webId, Guid mListID)
        {
            return null;
        }

        public Dictionary<string, object> GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid id, string dirName, string leafName, bool isListItem)
        {
            return null;
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid id, bool hasDocLibRowId)
        {
            return DateTime.Now;
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId)
        {
            return DateTime.Now;
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid tp_Guid, ref Guid docId)
        {
            return DateTime.Now;
        }

        public Dictionary<Guid, object> QueryListAlertForIB(Guid siteId, Guid webId, Guid mListID)
        {
            return null;
        }

        public Dictionary<Guid, object> QueryListViewForIB(Guid siteId, Guid webId, Guid mListID)
        {
            return null;
        }

        public bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId)
        {
            return false;
        }

        public Dictionary<int, object> GetItemVersions(Guid siteId, Guid webId, Guid listId, int docLibRowId)
        {
            return null;
        }

        public Dictionary<Guid, object> QueryWebListForFB(Guid siteId, Guid webId)
        {
            return null;
        }

        public Dictionary<string, object> QueryRootWeb(Guid siteId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Guid, object> QueryListViewForFB(Guid siteId, Guid webId, Guid listId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> QueryCurrentFolder(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, string listUrl)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> QueryWebRootFolder(Guid webId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<Guid, object> GetSubWebs(Guid siteId, Guid parentWebId)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IAveRequest Members


        public Dictionary<byte[], object> QueryWebContentTypeForFB(Guid siteId, Guid webId)
        {
            throw new NotImplementedException();
        }

        #endregion
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "g_wsaSiteTemplateId is a key")]
        public static string GetWebTemplateConfiguration(string webAppName, string webRelativeUrl, object obj)
        {
            return GetWebTemplateConfiguration(webAppName, webRelativeUrl, obj, null);
        }
        public static string GetWebTemplateConfiguration(string webAppName, string webRelativeUrl, object obj,ITokenProvider tokenProvider)
        {
            try
            {
                return AveWebTemplateHelper.GetWebTemplateConfiguration(webAppName.TrimEnd('/') + "/" + webRelativeUrl.Trim('/'), obj,tokenProvider, string.Empty, 14);
            }
            catch (Exception e)
            {
                SecurityTrimObject webTrimObj = mSiteTrimObj.GetWeb(webRelativeUrl, mSiteTrimObj.Name);
                string[] properties = new string[] { "WebTemplate", "Configuration" };
                string htmlUrl = webAppName.TrimEnd('/') + "/" + webRelativeUrl.Trim('/') + "/_layouts/settings.aspx";
                foreach (string property in properties)
                {
                    if (!webTrimObj.TrimmedProperties.ContainsKey(property))
                    {
                        webTrimObj.TrimmedProperties[property] = string.Format("{0} accessing resource: {1}", e.Message, htmlUrl);
                    }
                }
                return string.Empty;
            }
        }

        public string GetWebTemplateConfiguration(string webRelativeUrl)
        {
            throw new NotImplementedException();
        }

        public List<AveListBrowserInfo> GetBrowserLists(Guid parentWebId)
        {
            throw new NotImplementedException();
        }


        public AveWebBrowserInfo GetBrowserRootWeb()
        {
            throw new NotImplementedException();
        }


        public List<AveWebBrowserInfo> GetBrowserWebs(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount)
        {
            throw new NotImplementedException();
        }


        public AveFolderBrowserInfo GetBrowserRootFolder(Guid parentWebId, Guid parentListId)
        {
            throw new NotImplementedException();
        }


        public List<AveFolderBrowserInfo> GetBrowserSubFolders(Guid parentWebId, Guid parentFolderUniqueId, string parentFolderServerRelativeUrl)
        {
            throw new NotImplementedException();
        }


        public List<AveItemBrowserInfo> GetBrowserItems(Guid webId, Guid parentFolderUniqueId, string parentFolderServerRelativeUrl, ref string pageInfo, uint perPage)
        {
            throw new NotImplementedException();
        }

        private void RefreshCredentials(object newCredentials)
        {
            //AveBPOSAccountInfo bposAccount = mNetWork.User;
            //mNetWork.Dispose();
            //mNetWork = new AveWebServiceNetWork(bposAccount, mWebUrl, newCredentials);
            mObj = newCredentials;
        }

        public void Dispose(bool KeepRequest)
        {
            if (!KeepRequest)
            {
                this.Dispose();
            }
            else
            {
                //to do DisposeCache
            }
        }

        public Dictionary<string, object> GetFirstUniqueNavigationWeb(string webServerRelativeUrl)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetQuickLaunchFromInheritWeb(string webServerRelativeUrl)
        {
            throw new NotImplementedException();
        }

        public string GetListSchemalXml(string ParentWebUrl, Guid Id, string listTitle)
        {
            throw new NotImplementedException();
        }


        #region IAveRequest Members


        public Dictionary<string, object> GetLists(Guid webId)
        {
            throw new NotImplementedException();
        }

        #endregion

        //由原来的updateUser扩展的方法，方便2013的调用。
        public Dictionary<string, object> GetUserProperties(string webFullUrl, string loginName, string name, bool updateAdminOnly, Dictionary<string, object> userProp)
        {
            string userName = name;
            string userEmail = string.Empty;
            string userNotes = string.Empty;
            foreach (KeyValuePair<string, object> pair in userProp)
            {
                switch (pair.Key)
                {
                    case "Email":
                        userEmail = userProp["Email"] as string;
                        break;
                    case "Name":
                        userName = userProp["Name"] as string;
                        break;
                    case "Notes":
                        userNotes = userProp["Notes"] as string;
                        break;
                    default:
                        break;
                }
            }
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.UserGroup, webFullUrl);
                if (!updateAdminOnly)
                {
                    mNetWork.UserGroupUpdateUser(loginName, userName, userEmail, userNotes);
                }
                XmlNode node = mNetWork.UserGroupGetUserInfo(loginName);
                return this.GetUserDic(node);
            }
        }

        public Dictionary<string, object> GetWorkflowAssociations(string webServerRelativeUrl, string listName, Guid listId, string workflowSource, Dictionary<string, object> contentTypeProp)
        {
            return mRequestCommon.GetWorkflowAssociations(webServerRelativeUrl, listName, listId, workflowSource, contentTypeProp);
        }

        public Dictionary<string, object> GetFeedFor(string postId, Dictionary<string, object> options)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> LikePost(string postId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> CreatePost(string targetId, Dictionary<string, object> creationData)
        {
            throw new NotImplementedException();
        }
        public Dictionary<string, object> GetFullThread(string threadId)
        {
            throw new NotImplementedException();
        }


        public List<AveFolderBrowserInfo> GetBrowserSubFolders(Guid parentWebId, Guid parentFolderUniqueId, Guid parentListId, string parentFolderServerRelativeUrl, int startIndex, uint perPage, ref int childrenCount)
        {
            throw new NotImplementedException();
        }

        public List<AveListBrowserInfo> GetBrowserLists(Guid parentWebId, int startIndex, uint perPage, ref int childrenCount)
        {
            throw new NotImplementedException();
        }

        public AveWebBrowserInfo GetBrowserRootWeb(AveBrowserOption option)
        {
            throw new NotImplementedException();
        }

        public List<AveWebBrowserInfo> GetBrowserWebs(AveBrowserOption option)
        {
            throw new NotImplementedException();
        }

        public AveFolderBrowserInfo GetBrowserRootFolder(AveBrowserOption option)
        {
            throw new NotImplementedException();
        }

        public List<AveFolderBrowserInfo> GetBrowserSubFolders(AveBrowserOption option)
        {
            throw new NotImplementedException();
        }

        public List<AveItemBrowserInfo> GetBrowserItems(AveBrowserOption option)
        {
            throw new NotImplementedException();
        }

        public List<AveListBrowserInfo> GetBrowserLists(AveBrowserOption option)
        {
            throw new NotImplementedException();
        }

        public List<AveItemVersionBrowserInfo> GetBrowserItemVersions(AveBrowserOption option)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> AddItem(string webServerRelativeUrl, string listName, Guid listId, string folderUrl, int parentId, int underlyingObjectType, string leafName, Dictionary<string, object> itemProperties, bool isDiscussion)
        {
            throw new NotImplementedException();
        }


        public Dictionary<string, object> GetManagedSitecollectionData()
        {
            throw new NotImplementedException();
        }

        public bool AddSiteAdmin(string username, string siteCollectionUrl, string tenantAdminSiteUrl = "")
        {
            throw new NotImplementedException();
        }

        public string AddSite(string CAUrl, int compatibilityLevel, uint lcid, string owner, long storageQuota, string template, int timeZoneId, string title, string url, double resourceQuota)
        {
            using (AveWebServiceNetWork network = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                network.InitialNetWorker(AveWebServiceType.Admin, CAUrl);
                return network.AddSite(lcid, owner, template, title, url);
            }
        }

        public Dictionary<string, object> GetWebChangesByQuery(string webServerRelativeUrl, Dictionary<string, object> queryProps)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetListChangesByQuery(string webServerRelativeUrl, Guid listId, Dictionary<string, object> queryProps)
        {
            throw new NotImplementedException();
        }
        public virtual Dictionary<string, object> GetSiteChangesByQuery(Dictionary<string, object> queryProps)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetItemByGuid(Guid webId, Guid listId, Guid tp_Guid)
        {
            throw new NotImplementedException();
        }


        public void DeleteWorkflowAssociation(IAveWorkflowAssociation workflow, string source)
        {
            throw new NotImplementedException();
        }


        public Dictionary<string, string> GetMetaInfo(string webServerRelativeUrl, string docServerRelativeUrl)
        {
            return this.mRequestCommon.GetMetaInfo(webServerRelativeUrl, docServerRelativeUrl);
        }

        public void DeclareOrUndeclareItem(int itemId, Guid listId, string webUrl)
        {
            mRequestCommon.DeclareOrUndeclareItem(itemId, listId, webUrl);
        }


        public void UpdateWorkflowAssociationsOnChildren(string webUrl, string contentTypeId)
        {
            mRequestCommon.UpdateWorkflowAssociationsOnChildren(webUrl, contentTypeId);
        }

        #region used for openWeb() method

        public void SetCurrentWebUrl(string currentWebUrl)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> OpenCurrentWeb()
        {
            throw new NotImplementedException();
        }

        #endregion



        public Dictionary<string, object> GetTermGroup(Guid termStoreId, Guid groupId)
        {
            throw new NotImplementedException();
        }


        public void ApplyWebTemplate(string webUrl, string webTemplate)
        {
            throw new NotImplementedException();
        }


        public void DeleteAttachment(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, Guid webId, Guid listId, int rowId, string attachmentName)
        {
            this.DeleteAttachmentNow(webServerRelativeUrl, listServerRelativeUrl, listTitle, rowId, attachmentName);
        }


        public void PublishSharepointList(string webServerRelativeUrl, IAveFile templateFile, int lcid, string listId, string contentTypeId)
        {
            using (Stream stream = new MemoryStream(templateFile.OpenBinary(), false))
            {
                stream.Seek(0L, SeekOrigin.Begin);
                byte[] buffer = new byte[stream.Length];
                new BinaryReader(stream).Read(buffer, 0, Convert.ToInt32(stream.Length));
                stream.Close();
                string value = Convert.ToBase64String(buffer);
                this.SetFormsForListItem(webServerRelativeUrl, lcid, value, "InfoPath 14", listId, contentTypeId);
            }
        }

        private void SetFormsForListItem(string webServerRelativeUrl, int lcid, string base64FormTemplate, string applicationId, string listGuid, string contentTypeId)
        {
            string webFullUrl = mWebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, mObj, mTokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.FormsServices, webFullUrl);
                mNetWork.SetFormsForListItem(lcid, base64FormTemplate, applicationId, listGuid, contentTypeId);
            }
        }


        public bool DeleteMigrationJob(Guid id)
        {
            throw new NotSupportedException();
        }


        public AveMigrationJobState GetMigrationJobStatus(Guid id)
        {
            throw new NotSupportedException();
        }


        public Guid CreateMigrationJob(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri)
        {
            throw new NotSupportedException();
        }

        public Guid CreateMigrationJobEncrypted(Guid gWebId, string azureContainerSourceUri, string azureContainerManifestUri, string azureQueueReportUri, IAveEncryptionOption options)
        {
            throw new NotSupportedException();
        }


        public void MoveTo(string parentWebUrl, string parentWebServerRelativeUrl, string folderServerRelativeUrl, string newUrl)
        {
            throw new NotSupportedException();
        }


        public Dictionary<string, object> GetFileById(string webServerRelativeUrl, Guid fileId)
        {
            throw new NotSupportedException();
        }


        public Dictionary<string, object> GetFolderById(string webServerRelativeUrl, Guid folderId)
        {
            throw new NotSupportedException();
        }

        public void DeleteAllWorkflowAasociations(string webUrl, Guid listId, string contentTypeId, string source)
        {
            throw new NotImplementedException();
        }

        public void ApplyCustomWebTemplateInSolution(string webServerRelativeUrl, string solutionPath, string solutionName, string webTemplateName, uint lcid, List<AveSolutionFeature> packageFeatures, Guid packageSolutionId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetItemByUniqueId(Guid webId, Guid listId, Guid itemId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetChanges(Guid termStoreId, DateTime startTime)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetChanges(Guid termStoreId, TimeSpan sinceTimeAgo)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetChanges(Guid termStoreId, DateTime startTime, AveChangedItemType itemType)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetChanges(Guid termStoreId, DateTime startTime, AveChangedItemType itemType, AveChangedOperationType operationType)
        {
            throw new NotImplementedException();
        }

        public string GetDescription(Guid termStoreId, Guid termSetId, Guid parentTermId, int lcid)
        {
            throw new NotImplementedException();
        }
        public Dictionary<int, string> GetAllDescriptions(Guid termStoreId, Guid termSetId, Guid parentTermId, Collection<int> lcids)
        {
            throw new NotImplementedException();
        }
        public bool GetSiteExists(string url)
        {
            return false;
        }

        public Dictionary<string, object> GetItemByUrl(Guid webId, string itemUrl,out Guid listId)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetUser(int id)
        {
            throw new NotImplementedException();
        }

        public AveWebMasterPageInfo GetRootWebMasterPageInfo()
        {
            throw new NotImplementedException();
        }

        public void SetRootWebAndMySiteWebMasterPageInfo(string mySiteWebServerRelativeUrl, AveWebMasterPageInfo pageInfo)
        {
            throw new NotImplementedException();
        }

        public virtual WorkflowStartOptionCache BackupWorkflowStartOption(string url, Guid webId, Guid listId)
        {
            return null;
        }

        public virtual void RestoreWorkflowStartOption(string url, Guid webId, Guid listId, WorkflowStartOptionCache cache)
        {

        }

        public void DeleteSiteToRecylebin(string CAUrl, string url)
        {
            DeleteSite(CAUrl, url);
        }

        public void ApplySiteDesign(string webUrl, Guid siteDesignId)
        {
            throw new NotImplementedException();
        }

        public void PostRestoreModernWebpart(IAveSite site, AveSiteMappingManager mapping, AveSiteInfo sourceSitInfo)
        {
        }

        public Dictionary<string, object> GetBrowserSiteInfo()
        {
            throw new NotImplementedException();
        }

        public string GetWebTemplateTitle(string siteUrl, uint language, string templateName)
        {
            throw new NotImplementedException();
        }

        public string GetServerVersion()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetUser(string userEmail)
        {
            throw new NotImplementedException();
        }
    }
}
