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

using AvePoint.GCommon;
using AvePoint.ObjectModel.WebServiceCore;
using AvePoint.Wrapper.Common;
using Microsoft365.Authentication;
using Microsoft365.SharePoint.Extension;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace AvePoint.ObjectModel.WebService
{
    public class AveWebServiceRequest : IDisposable,IAveWebServiceRequestOnline
    {
        private static IAveLogger logger = AveLogger.GetInstance(typeof(AveWebServiceRequest));




        private string mWebUrl;
        private string mWebAppName;
        private ITokenProvider tokenProvider;
        private AveBPOSAccountInfo mAccountInfo;

        public AveWebServiceRequest(string siteUrl, AveBPOSAccountInfo accountInfo, ITokenProvider tokenProvider)
        {
            this.tokenProvider = tokenProvider;
            mWebUrl = siteUrl;
           // mServerVersion = serverVersion;
            mAccountInfo = accountInfo;
           // mRequestCommon = new AveHttpWebRequestCommon(mWebUrl, tokenProvider, serverVersion);
        }

        public bool IsAvaliable { get { return true; } }

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

        #region

        public Dictionary<string, object> GetLimitedWebPartManager(string webServerRelativeUrl, string fileServerRelativeUrl, int personalizationScope)
        {
            Dictionary<string, object> webPartManagerProperties = new Dictionary<string, object>();
            Dictionary<string, object> webPartColProperties = new Dictionary<string, object>();
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, tokenProvider))
            {
                string currentWebUrl = AveUrlUtility.CombineUrl(this.WebAppName, webServerRelativeUrl);
                string currentFileUrl= AveUrlUtility.CombineUrl(this.WebAppName, fileServerRelativeUrl);
                mNetWork.InitialNetWorker(AveWebServiceType.WebPartPages, currentWebUrl);
                string webpartPageContent = mNetWork.WebPagePagesGetWebPartOnPage(currentFileUrl);
                if (string.IsNullOrEmpty(webpartPageContent))
                {
                    return webPartManagerProperties;
                }
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(webpartPageContent);
                List<Dictionary<string, object>> webpartPropertiesList = new List<Dictionary<string, object>>();
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Comment)
                    {
                        continue;
                    }
                    if (node.OuterXml.IndexOf("http://schemas.microsoft.com/WebPart/v3") != -1)
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
                if (!string.IsNullOrEmpty(webPartType))
                {
                    webPartProperties["RealWebPartType"] = webPartType.Split(new char[] { ',' })[0];
                }
            }
            webPartProperties.Add("DefinitionXml", definitionXml);
            webPartProperties.Add("ID", element.GetAttribute("ID"));
            XmlElement childNode = null;
            //利用xpaht取得listid，为还原时的postAction准备。
            try
            {
                childNode = element.SelectSingleNode(".//" + versionNameSpace + ":property[@name='TitleUrl']", nsmgr) as XmlElement;
                if (childNode != null)
                {
                    webPartProperties["TitleUrl"] = childNode.InnerText;
                }
                childNode = element.SelectSingleNode(".//" + versionNameSpace + ":property[@name='Title']", nsmgr) as XmlElement;
                if (childNode != null)
                {
                    webPartProperties["Title"] = childNode.InnerText;
                }
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
                logger.Warn("Can not get listid in GetLimitedWebPartManager,fileServerRelativeUrl:{0},error:{1}.", fileServerRelativeUrl, e.ToString());
            }
            //childNode = element.SelectSingleNode(".//*[name() = 'ZoneID']") as XmlElement;
            //if (childNode != null)
            //{
            //    webPartProperties.Add("ZoneID", childNode.InnerText);
            //}
            //childNode = element.SelectSingleNode(".//*[name() = 'PartOrder']") as XmlElement;
            //if (childNode != null && !string.IsNullOrEmpty(childNode.InnerText))
            //{
            //    webPartProperties.Add("PartOrder", Convert.ToInt32(childNode.InnerText));
            //    webPartProperties.Add("ZoneIndex", Convert.ToInt32(childNode.InnerText));
            //}
            //else
            //{
            //    webPartProperties.Add("PartOrder", 0);
            //}

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
            if (node.OuterXml.IndexOf("http://schemas.microsoft.com/WebPart/v2/ListView") != -1)
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
            //childNode = xDocV2.DocumentElement.SelectSingleNode(string.Format("//vList:{0}", "ZoneID"), nsmgr) as XmlElement;
            //if (childNode != null)
            //{
            //    webPartProperties.Add("ZoneID", childNode.InnerText);
            //}
            //childNode = xDocV2.DocumentElement.SelectSingleNode(string.Format("//vList:{0}", "PartOrder"), nsmgr) as XmlElement;
            //if (childNode != null)
            //{
            //    webPartProperties.Add("PartOrder", Convert.ToInt32(childNode.InnerText));
            //    webPartProperties.Add("ZoneIndex", Convert.ToInt32(childNode.InnerText));
            //}
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
            childNode = xDocV2.DocumentElement.SelectSingleNode(string.Format("//vList:{0}", "Title"), nsmgr) as XmlElement;
            if (childNode != null)
            {
                webPartProperties.Add("Title", childNode.InnerText);
            }
            childNode = xDocV2.DocumentElement.SelectSingleNode(string.Format("//vList:{0}", "TypeName"), nsmgr) as XmlElement;
            if (childNode != null)
            {
                webPartProperties.Add("RealWebPartType", childNode.InnerText);
            }
            webpartPropertiesList.Add(webPartProperties);
        }

        public Stream GetFileStream(string webServerRelativeUrl, string fileServerRelativeUrl, string source, bool isSpecialList = false)
        {
            //use UrlEncoder to encode file name which is not correctly encoded
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
            Stream netStream = null;
            AveCoordinatedStream memoryStream = new AveCoordinatedStream("WSRFS");
            try
            {
                var request = ReliableHttpWebRequest.CreateRequest(new Uri(fileFullUrl));
                request.SetTokenProvider(mWebUrl,tokenProvider,false);
                request.Timeout = SharePointFileReadWriteOptions.RequestTimeout; 
                request.ReadWriteTimeout = SharePointFileReadWriteOptions.ReadWriteTimeout;
                request.Accept = "*/*";
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; .NET4.0C; .NET4.0E)";
                request.Headers["Accept-Language"] = "en-US,ja-JP;q=0.5";
                netStream = request.GetResponse().GetResponseStream();
            }
            catch (Exception e)
            {
                logger.Error($"GetFileStream failed for {fileFullUrl}.Error:{e}");
            }
            try
            {
                this.CopyStream(netStream, memoryStream, 64 * 1024, true);
            }
            catch (Exception)
            {
                memoryStream.Dispose();
                throw;
            }
            finally
            {
                netStream?.Dispose();
            }
            return memoryStream;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "_aut:A lesson of of sharepoint local path.")]
        private Stream GetFileStream(string webServerRelativeUrl, string fileServerRelativeUrl, int versionId)
        {
            string url = this.WebAppName.TrimEnd('/') + webServerRelativeUrl.TrimEnd('/') + "/_vti_bin/_vti_aut/author.dll";
            if (fileServerRelativeUrl.StartsWith(webServerRelativeUrl))
            {
                fileServerRelativeUrl = fileServerRelativeUrl.Substring(webServerRelativeUrl.TrimEnd('/').Length).TrimStart('/');
            }
            //如果文件名字里出现等号，并且需要用rpc协议去下载文件，需要对等号进行转移
            if (fileServerRelativeUrl.Contains('='))
            {
                fileServerRelativeUrl = fileServerRelativeUrl.Replace("=", "\\=");
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
                + "&service_name=/" + "&dir_name=" + HttpUtility.UrlEncode(dirName)
                + "&document_name=" + HttpUtility.UrlEncode(fileServerRelativeUrl)
                + "&force=true&get_option=none"
                + "&doc_version=V" + id + "&timeout=0";
            byte[] body = UTF8Encoding.UTF8.GetBytes(postContent);
            Dictionary<string, object> headerInformation = new Dictionary<string, object>();
            headerInformation.Add("X-Vermeer-Content-Type", "application/x-www-form-urlencoded");
            string result = AveHttpWebRequestUtility.HttpReturn(url, tokenProvider, contentType, body, headerInformation);

            if (AveHttpWebRequestUtility.LastException != null)
            {
                throw AveHttpWebRequestUtility.LastException;
            }

            int index = result.IndexOf("<html>");
            int endIndex = result.IndexOf("</html>") + 7;
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

        //public Dictionary<string, object> GetUserProfileByName(string accountName)
        //{
        //    Dictionary<string, object> returnInfo = new Dictionary<string, object>();
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, tokenProvider))
        //    {
        //        mNetWork.InitialNetWorker(AveWebServiceType.UserProfile, mWebUrl);
        //        UserProfileService.PropertyData[] datas = mNetWork.UserProfileGetUserProfile(accountName);
        //        returnInfo["ProfileValues"] = GetUserProfilePropertyValues(datas);//"DefaultProfileSubtypeProperties"+AveObjectModelConstant.ObjectPropertySuffix
        //    }
        //    return returnInfo;
        //}

        //public List<AvePropertyInfo> GetUserProfileSchema()
        //{
        //    using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, tokenProvider))
        //    {
        //        mNetWork.InitialNetWorker(AveWebServiceType.UserProfile, mWebUrl);
        //        UserProfileService.PropertyInfo[] infos = mNetWork.UserProfileGetUserProfileSchema();
        //        return GetUserProfileSchemaValues(infos);
        //    }
        //}

        //private List<AvePropertyInfo> GetUserProfileSchemaValues(UserProfileService.PropertyInfo[] propertyInfos)
        //{
        //    List<AvePropertyInfo> values = new List<AvePropertyInfo>();
        //    foreach (UserProfileService.PropertyInfo prop in propertyInfos)
        //    {
        //        AvePropertyInfo aveProp = new AvePropertyInfo();
        //        aveProp.AllowPolicyOverride = prop.AllowPolicyOverride;
        //        aveProp.ChoiceType = (ChoiceTypes)Enum.Parse(typeof(ChoiceTypes), prop.ChoiceType.ToString());
        //        aveProp.DefaultPrivacy = (AvePrivacy)Enum.Parse(typeof(AvePrivacy), prop.DefaultPrivacy.ToString());
        //        aveProp.Description = prop.Description;
        //        aveProp.DisplayName = prop.DisplayName;
        //        aveProp.DisplayOrder = prop.DisplayOrder;
        //        aveProp.IsAdminEditable = prop.IsAdminEditable;
        //        aveProp.IsAlias = prop.IsAlias;
        //        aveProp.IsColleagueEventLog = prop.IsColleagueEventLog;
        //        aveProp.IsImported = prop.IsImported;
        //        aveProp.IsMultivalued = prop.IsMultiValue;
        //        aveProp.IsReplicable = prop.IsReplicable;
        //        aveProp.IsRequired = prop.IsRequired;
        //        aveProp.IsSearchable = prop.IsSearchable;
        //        aveProp.IsSystem = prop.IsSystem;
        //        aveProp.IsUserEditable = prop.IsUserEditable;
        //        aveProp.IsVisibleOnEditor = prop.IsVisibleOnEditor;
        //        aveProp.IsVisibleOnViewer = prop.IsVisibleOnViewer;
        //        aveProp.Length = prop.Length;
        //        aveProp.ManagedPropertyName = prop.ManagedPropertyName;
        //        aveProp.MaximumShown = prop.MaximumShown;
        //        aveProp.Name = prop.Name;
        //        aveProp.Type = prop.Type;
        //        aveProp.UserOverridePrivacy = prop.UserOverridePrivacy;
        //        values.Add(aveProp);
        //    }
        //    return values;
        //}

        //private List<Dictionary<string, object>> GetUserProfilePropertyValues(UserProfileService.PropertyData[] datas)
        //{
        //    List<Dictionary<string, object>> valueList = new List<Dictionary<string, object>>();
        //    foreach (UserProfileService.PropertyData data in datas)
        //    {
        //        Dictionary<string, object> valueInfo = new Dictionary<string, object>();
        //        valueInfo["NameValue"] = data.Name;
        //        string privacy = data.Privacy.ToString();
        //        valueInfo["Privacy"] = Enum.Parse(typeof(AvePrivacy), privacy);//1.2.4.8.16.1073741824

        //        List<object> values = new List<object>();
        //        foreach (UserProfileService.ValueData value in data.Values)
        //        {
        //            if (!(value.Value is DateTime
        //                && (DateTime)value.Value == DateTime.MinValue))
        //            {
        //                if (data.Name.Equals("SPS-TimeZone", StringComparison.OrdinalIgnoreCase))
        //                {
        //                    values.Add((value.Value as UserProfileService.SPTimeZone).ID);
        //                    continue;
        //                }
        //                values.Add(value.Value);
        //            }
        //        }
        //        valueInfo["Value"] = values;
        //        valueList.Add(valueInfo);
        //    }
        //    return valueList;
        //}

        public Stream GetFileVersionStream(string webServerRelativeUrl, string fileServerRelativeUrl, string fileVerionServerRelativeUrl, int versionId)
        {
            AveTaskRetryHelper retryHelper = new AveTaskRetryHelper(6, new KeyValuePair<string, string>("WebException", "Unable to connect to the remote server"),
                                                                       new KeyValuePair<string, string>("WebException", "The remote server returned an error: (500) Internal Server Error"));
            Stream stream = null;
            //try
            {
                List<string> fileExtensions = new List<string>() { ".aspx", ".master", ".xoml", ".rules" };
                string currentFileExtension = Path.GetExtension(fileServerRelativeUrl);
                retryHelper.ExecuteWithRetryMechanism(() =>
                {
                    if (fileExtensions.Contains(currentFileExtension.ToLowerInvariant()))
                    {
                        stream = GetFileStream(webServerRelativeUrl, fileServerRelativeUrl, versionId);
                    }
                    else
                    {
                        stream = GetFileStream(webServerRelativeUrl, fileVerionServerRelativeUrl, null);
                    }
                });
            }
            //catch (Exception ex)
            //{
            //    logger.Warn("Get file version:{0} stream failed.Error Message:{1}", fileServerRelativeUrl + ":" + versionId.ToString(), ex.ToString());
            //    retryHelper.ExecuteWithRetryMechanism(() =>
            //    {
            //        stream = GetFileStream(webServerRelativeUrl, fileServerRelativeUrl, versionId);
            //    });
            //}
            return stream;
        }

        #endregion

        ///Not supported in App-Only Token
        public string AssociateWorkflowMarkup(string webServerRelativeUrl, string configUrl, string configVersion)
        {
            Uri siteUri = new Uri(mWebUrl);
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, tokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.WebPartPages, webFullUrl);
                return mNetWork.AssociateWorkflowMarkup(configUrl, configVersion);
            }
        }

        /// Not Supported
        public void BrowserEnableUserFormTemplate(string formTemplateUrl)
        {
            Uri siteUri = new Uri(mWebUrl);
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, tokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.FormsServices, siteUri.AbsoluteUri.TrimEnd('/'));
                mNetWork.BrowserEnableUserFormTemplate(formTemplateUrl);
            }
        }

        #region Restore

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used in web request")]
        public void RestoreTheme(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebSettingInfo webSettingInfo, string themedCssFolderUrl)
        {
            string postUrl = this.WebAppName + webServerRelativeUrl.TrimEnd('/') + "/_layouts/themeweb.aspx";
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

            AveWebThemeInfo theme = new AveWebThemeInfo();
            if (webSettingInfo.WebTheme == null)    //when source site theme is set to default, WebTheme will be null
            {
                theme.ThemeName = string.Empty;
                theme.InheritsThemedCssFolderUrl = false;
            }
            else
            {
                theme = webSettingInfo.WebTheme.Value;
            }

            bodyDic["__EVENTTARGET"] = "ctl00%24PlaceHolderMain%24ctl02%24RptControls%24Submit1";
            if (theme.InheritsThemedCssFolderUrl)
            {
                bodyDic["ctl00$PlaceHolderMain$ctl06$inheritThemeSection$inheritThemeGroup"] = "inheritTheme";
                //InputColor(bodyDic, theme);
            }
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
            byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
            AveHttpWebRequestUtility.HttpPost(postUrl, tokenProvider, "application/x-www-form-urlencoded", body, null);
        }

        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="operation">"ACT" is active;"DEA" is deactive.</param>
        ///// <param name="siteUrl"></param>
        ///// <param name="webServerRelativeUrl"></param>
        ///// <param name="id"></param>
        ///// <param name="obj"></param>
        //[SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "diid,Tbl:Special characters of solution's field xml.")]
        //public void OperateOnSolution(string operation, string siteUrl, string webServerRelativeUrl, int id)
        //{
        //    string url = siteUrl.TrimEnd('/') + "/_catalogs/solutions/Forms/Activate.aspx?" + "Op=" + operation + "&ID=" + id.ToString()
        //        + "&Source=" + siteUrl.TrimEnd('/') + "/_catalogs/solutions/Forms/AllItems.aspx"
        //        + "&RootFolder=" + webServerRelativeUrl.TrimEnd('/') + "/_catalogs/solutions" + "&IsDlg=1";
        //    string html = AveHttpWebRequestUtility.HttpGet(url, tokenProvider);
        //    Dictionary<string, object> bodyDic = new Dictionary<string, object>();
        //    string searchContent = "<input type=\"hidden\"";
        //    AveHttpWebRequestUtility.GetInput(html, searchContent, bodyDic);
        //    Dictionary<string, object> buttonDic = new Dictionary<string, object>();
        //    AveHttpWebRequestUtility.GetInput(html, "<input type=\"button\"", buttonDic);
        //    foreach (string key in buttonDic.Keys)
        //    {
        //        if (key.EndsWith("diidIOGoBack", StringComparison.OrdinalIgnoreCase))
        //        {
        //            int index = key.IndexOf("ctl00", 2, StringComparison.OrdinalIgnoreCase);
        //            string target = string.Empty;
        //            if (operation.Equals("ACT"))
        //            {
        //                target = key.Substring(0, index) + "ctl00$ctl00$ctl00$toolBarTbl$RptControls$diidIOActivateSolutionItem";
        //            }
        //            else if (operation.Equals("DEA"))
        //            {
        //                target = key.Substring(0, index) + "ctl00$ctl00$ctl00$toolBarTbl$RptControls$diidIODeactivateSolutionItem";
        //            }
        //            bodyDic["&__EVENTTARGET"] = System.Web.HttpUtility.UrlEncode(target);
        //            break;
        //        }
        //    }
        //    if (bodyDic.ContainsKey("__EVENTVALIDATION"))
        //    {
        //        bodyDic["__EVENTVALIDATION"] = HttpUtility.UrlEncode(bodyDic["__EVENTVALIDATION"].ToString());
        //    }
        //    if (bodyDic.ContainsKey("__VIEWSTATE"))
        //    {
        //        bodyDic["__VIEWSTATE"] = HttpUtility.UrlEncode(bodyDic["__VIEWSTATE"].ToString());
        //    }
        //    bodyDic["&ctl00%24PlaceHolderSearchArea%24ctl01%24ctl00"] = siteUrl;
        //    bodyDic["&ctl00%24PlaceHolderSearchArea%24ctl01%24ctl01"] = siteUrl.TrimEnd('/') + "/_catalogs/solutions";
        //    byte[] body = AveHttpWebRequestUtility.GetByte(bodyDic, null);
        //    string contentType = "application/x-www-form-urlencoded";
        //    AveHttpWebRequestUtility.HttpPost(url, tokenProvider, contentType, body, null);
        //}

        #endregion

        #region Update

        public Dictionary<string, object> UpdateList(string webServerRelativeUrl, string listName, Guid listId, Dictionary<string, object> listProperties)
        {
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, mWebUrl, tokenProvider))
            {
                string url = WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimEnd('/');
                mNetWork.InitialNetWorker(AveWebServiceType.Lists, url);

                XmlNode listInfo = mNetWork.ListGetList(listName);
                string listGUID = listInfo.Attributes["ID"].Value;
                string version = listInfo.Attributes["Version"].Value;

                XmlDocument doc = new XmlDocument();
                XmlElement listPropertiesNode = null;
                if (listProperties != null)
                {
                    listPropertiesNode = doc.CreateElement("List");
                    foreach (KeyValuePair<string, object> pair in listProperties)
                    {
                        if (pair.Value != null)
                        {
                            listPropertiesNode.SetAttribute(pair.Key, pair.Value.ToString());
                        }
                    }
                }
                mNetWork.ListUpdateList(listGUID, (XmlNode)listPropertiesNode, null, null, null, version);
                //List修改Title的情况
                if (listPropertiesNode?.Attributes["Title"] != null)
                {
                    listName = listPropertiesNode.Attributes["Title"].Value;
                }
                listInfo = mNetWork.ListGetList(listName);
                Dictionary<string, object> newListProperties = new Dictionary<string, object>();
                XmlNodeToDicValue(newListProperties, listInfo);
                mNetWork.Dispose();
                return newListProperties;
            }
        }

        public void CheckInFile(string webUrl, string pageUrl, string comment, int checkinType)
        {
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(new AveBPOSAccountInfo(), webUrl, tokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.Lists, webUrl);
                mNetWork.CheckInFile(pageUrl, comment, checkinType);
            }
        }

        public Dictionary<string, object> UpdateContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeId, bool updateChildren, string contentTypeSource, Dictionary<string, object> needUpdateContentTypeProperties, List<string> supportedResourceCultureNames)
        {
            //For workflow, update list contentType XmlDocuments...
            if (string.IsNullOrEmpty(listName))
            {
                return null;
            }
            Dictionary<string, string> XmlDocumentData = null;
            if (needUpdateContentTypeProperties.ContainsKey("AddedDocuments"))
            {
                List<XmlNode> listXmlNodes = new List<XmlNode>();
                XmlDocument xmlDoc = new XmlDocument();
                XmlDocumentData = (Dictionary<string, string>)needUpdateContentTypeProperties["AddedDocuments"];
                foreach (string key in XmlDocumentData.Keys)
                {
                    xmlDoc.LoadXml(XmlDocumentData[key]);
                    XmlNode nodeEty = (XmlNode)xmlDoc.DocumentElement;
                    listXmlNodes.Add(nodeEty);
                    xmlDoc.RemoveAll();
                }

                Uri siteUri = new Uri(WebAppName.Trim('/') + "/" + webServerRelativeUrl.Trim('/'));
                using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, siteUri.AbsoluteUri, tokenProvider))
                {
                    mNetWork.InitialNetWorker(AveWebServiceType.Lists, siteUri.AbsoluteUri.TrimEnd('/'));

                    Dictionary<string, object> listProperties = new Dictionary<string, object>();

                    foreach (XmlNode node in listXmlNodes)
                    {
                        mNetWork.UpdateContentTypeXmlDocuments(listId.ToString(), contentTypeId, node);
                    }
                }
            }
            if (needUpdateContentTypeProperties.ContainsKey("NewDocumentControl")
                || needUpdateContentTypeProperties.ContainsKey("RequireClientRenderingOnNew"))
            {
                StringBuilder contentTypeBuilder = new StringBuilder("<ContentType xmlns=\"http://schemas.microsoft.com/sharepoint/soap/\"");
                if (needUpdateContentTypeProperties.ContainsKey("NewDocumentControl"))
                {
                    contentTypeBuilder.AppendFormat(" NewDocumentControl=\"{0}\"", needUpdateContentTypeProperties["NewDocumentControl"]);
                }
                if (needUpdateContentTypeProperties.ContainsKey("RequireClientRenderingOnNew"))
                {
                    contentTypeBuilder.AppendFormat(" RequireClientRenderingOnNew=\"{0}\"", needUpdateContentTypeProperties["RequireClientRenderingOnNew"].ToString().ToUpper());
                }
                contentTypeBuilder.Append("/>");
                Uri siteUri = new Uri(WebAppName.Trim('/') + "/" + webServerRelativeUrl.Trim('/'));
                using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, siteUri.AbsoluteUri, tokenProvider))
                {
                    mNetWork.InitialNetWorker(AveWebServiceType.Lists, siteUri.AbsoluteUri.TrimEnd('/'));
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(contentTypeBuilder.ToString());
                    mNetWork.UpdateContentType(listId.ToString(), contentTypeId, doc.DocumentElement);
                }
            }
            return new Dictionary<string, object>();
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "metadatacolsettings is a part of xml")]
        public void UpdateListItems(string webAppName, string webRelativeUrl, string listName, int itemId, string fileRef, Dictionary<string, object> itemProp)
        {
            string url = webAppName + webRelativeUrl.TrimEnd('/');
            
            //Lists.Lists listService = new Lists.Lists();
            //listService.Url = url + "/_vti_bin/Lists.asmx";
            //listService.TokenProvider = tokenProvider;
            XmlDocument doc = new XmlDocument();
            StringBuilder updateData = new StringBuilder();
            updateData.Append("<Batch OnError='Continue' DateInUtc='True'>");
            updateData.Append("<Method ID='1' Cmd='Moderate'>");
            updateData.Append("<Field Name='ID'>" + itemId.ToString() + "</Field>");
            updateData.Append("<Field Name='FileRef'>" + fileRef + "</Field>");
            updateData.Append("<Field Name='_ModerationStatus'>" + itemProp["ModerationStatus"] + "</Field>");
            if (itemProp.ContainsKey("Modified"))
            {
                DateTime modified = new DateTime(((DateTime)itemProp["Modified"]).Ticks, DateTimeKind.Utc);
                updateData.Append("<Field Name='Modified'>" + modified.ToString("yyyy-MM-ddTHH:mm:ssZ") + "</Field>");
            }
            if (itemProp.ContainsKey("ModerationComments") && !string.IsNullOrEmpty(itemProp["ModerationComments"].ToString()))
            {
                updateData.Append("<Field Name='_ModerationComments'>" + itemProp["ModerationComments"] + "</Field>");
            }
            updateData.Append("</Method>");
            updateData.Append("</Batch>");
            doc.LoadXml(updateData.ToString());
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, url, tokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.Lists, url.TrimEnd('/'));
                mNetWork.UpdateListItems(listName, doc.DocumentElement);

            }
            
        }

        #endregion

        #region Private method

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

        #endregion

        public void Dispose()
        {
            //mNetWork.Dispose();
        }

        public void SetFormForList(string webServerRelativeUrl, int lcid, string base64FormTemplate, string applicationId, string listGuid, string contentTypeId)
        {
            string webFullUrl = this.WebAppName.TrimEnd('/') + "/" + webServerRelativeUrl.TrimStart('/');
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(mAccountInfo, webFullUrl, tokenProvider))
            {
                mNetWork.InitialNetWorker(AveWebServiceType.FormsServices, webFullUrl);
                mNetWork.SetFormsForListItem(lcid, base64FormTemplate, applicationId, listGuid, contentTypeId);
            }
        }


        public void UpdateBroswerFormWebPartProperty(string webUrl, string webServerRelativeUrl, string fileServerRelativeUrl,Guid newId,string definitionXml)
        {
            using (AveWebServiceNetWork mNetWork = new AveWebServiceNetWork(null, webUrl, tokenProvider))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(definitionXml);
                IWebPartPropertyExtractor wpExtractor = WebPartExtractorFactory.Create(definitionXml);
                string ctID = wpExtractor.GetProperty("ContentTypeId");
                string formLocation = wpExtractor.GetProperty("FormLocation");
                if (ctID != null && formLocation != null)
                {
                    string fileUrl = AveUrlUtility.CombineUrl(AveUrlUtility.GetServerUrl(webUrl), fileServerRelativeUrl);
                    string fileRelativeUrl = AveUrlUtility.GetRelativeUrl(webServerRelativeUrl, fileServerRelativeUrl);
                    string url = webUrl.TrimEnd('/') + "/_vti_bin/_vti_aut/author.dll";
                    MetaInfoHandler metaInfoHandler = new MetaInfoHandler();
                    metaInfoHandler.Add(new MetaInfoProperty("vti_modifiedby", "SHAREPOINT\\system"));
                    MetaInfoProperty modifiedTimeProperty = new MetaInfoProperty("vti_timelastmodified", DateTime.UtcNow.ToString("dd MMM yyyy HH:mm:ss") + " -0000");
                    modifiedTimeProperty.Type = MetaInfoValueType.Time;
                    metaInfoHandler.Add(modifiedTimeProperty);
                    string headInfo = "method=put+document&service%5fname=" + Microsoft.SharePoint.Client.Utilities.HttpUtility.UrlKeyValueEncode(webServerRelativeUrl)
                        + "&document=" + Microsoft.SharePoint.Client.Utilities.HttpUtility.UrlKeyValueEncode("[document_name=" + fileRelativeUrl
                        + ";meta_info=[" + metaInfoHandler.ToUpdateString().TrimEnd(';') + "]]")
                        + "&put%5foption=edit%2cdiscardstreamchanges&comment=&keep%5fchecked%5fout=true";
                    //string headInfo = "method=put+document%3a15%2e0%2e0%2e4420&service%5fname=%2fsites%2finfopath1&document=%5bdocument%5fname%3dSitePages%2fHome%2easpx%3bmeta%5finfo%3d%5bvti%5fmodifiedby%3bSW%7cSHAREPOINT%5c%5csystem%3bvti%5ftimelastmodified%3bTW%7c30+Dec+2013+05%3a23%3a01+%2d0000%5d%5d&put%5foption=edit%2cdiscardstreamchanges&comment=&keep%5fchecked%5fout=false";
                    mNetWork.InitialNetWorker(AveWebServiceType.WebPartPages, webUrl);
                    string sWppHtml = mNetWork.GetWebPartPage(fileRelativeUrl);
                    sWppHtml = sWppHtml.Substring(sWppHtml.IndexOf("</html>") + "</html>".Length).TrimStart();
                    sWppHtml = AddMetaProgId(sWppHtml);
                    HtmlDocument htmlDoc = new HtmlDocument();
                    htmlDoc.OptionOutputOriginalCase = true;
                    htmlDoc.LoadHtml(sWppHtml);
                    HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//node()[@runat='server']");
                    foreach (HtmlNode subNode in nodes)
                    {
                        subNode.Attributes.Remove("__Preview");
                        subNode.Attributes.Remove("__designer:Preview");
                        subNode.Attributes.Remove("__designer:Values");
                        subNode.Attributes.Remove("__designer:templates");
                    }
                    //remove <!--[if gte mso 9] node
                    HtmlNodeCollection commentNodes = htmlDoc.DocumentNode.SelectNodes("//comment()");
                    if (commentNodes != null)
                    {
                        foreach (HtmlNode commentNode in commentNodes)
                        {
                            if (!string.IsNullOrEmpty(commentNode.InnerHtml) && commentNode.InnerHtml.StartsWith("<!--[if gte mso 9]"))
                            {
                                commentNode.Remove();
                            }
                        }
                    }
                    HtmlNode node = htmlDoc.DocumentNode.SelectSingleNode("//node()[@__webpartid='{" + newId.ToString().ToUpper() + "}']");
                    node.SetAttributeValue("FormLocation", formLocation);
                    node.SetAttributeValue("ContentTypeId", ctID);
                    byte[] body = UTF8Encoding.UTF8.GetBytes(headInfo + "\n" + htmlDoc.DocumentNode.OuterHtml.TrimEnd() + "\r\n");
                    Dictionary<string, object> headerInformation = new Dictionary<string, object>();
                    headerInformation.Add("X-Vermeer-Content-Type", "application/x-vermeer-urlencoded");
                    string result = AveHttpWebRequestUtility.HttpReturn(url, tokenProvider, "application/x-vermeer-urlencoded", body, headerInformation, "MSFrontPage/15.0");
                }
            }
        }

        private static string AddMetaProgId(string aspxContent)
        {
            if (aspxContent.StartsWith("<HasByteOrderMark/>"))
            {
                aspxContent = aspxContent.Substring("<HasByteOrderMark/>".Length);
            }
            //doesn't need to add meta progid if the page is inherit from TemplateRedirectionPage
            if (aspxContent.IndexOf("Inherits=\"Microsoft.SharePoint.Publishing.TemplateRedirectionPage") >= 0)
            {
                return aspxContent;
            }
            string pageDirective = "<%@ Page";
            int startIndexOfPageDirective = aspxContent.IndexOf(pageDirective);
            int endIndexOfPageDirective = aspxContent.IndexOf("%>", startIndexOfPageDirective);
            string pageDirectiveContent = aspxContent.Substring(startIndexOfPageDirective, endIndexOfPageDirective - startIndexOfPageDirective);
            if (!pageDirectiveContent.Contains("meta:progid=\"SharePoint.WebPartPage.Document\""))
            {
                string pageDirectiveContentWithMetaInfo = pageDirectiveContent + " meta:webpartpageexpansion=\"full\" meta:progid=\"SharePoint.WebPartPage.Document\" ";
                return aspxContent.Replace(pageDirectiveContent, pageDirectiveContentWithMetaInfo);
            }
            else if (!pageDirectiveContent.Contains("meta:webpartpageexpansion=\"full\""))
            {
                string pageDirectiveContentWithMetaInfo = pageDirectiveContent + " meta:webpartpageexpansion=\"full\" ";
                return aspxContent.Replace(pageDirectiveContent, pageDirectiveContentWithMetaInfo);
            }
            return aspxContent;
        }
    }
}