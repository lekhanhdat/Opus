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
using System.IO;
using AvePoint.Wrapper.Common;
using AvePoint.ObjectModel.ClientOM;
using AvePoint.ObjectModel.WebService;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace AvePoint.ObjectModel.CompoundRequest
{
    [AveCodeReview("2012/03/08", "qwhu@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1 }, "ADO-26504", true)]
    public class AveClientCompoundRequest : AveClientOMRequest, IAveRequest
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveClientCompoundRequest));
        private string mSiteUrl;
        private AveBPOSAccountInfo mAccount;
        private AveWebServiceRequest mWebServiceRequest;

        public AveClientCompoundRequest(string url, AveBPOSAccountInfo userAccountInfo, object obj, string serverVersion)
            : base(url, userAccountInfo, obj, serverVersion)
        {
            mSiteUrl = url;
            mAccount = userAccountInfo;
            Type = AveClientRequestType.AveClientCompoundRequest;
            mWebServiceRequest = new AveWebServiceRequest(url, userAccountInfo, obj, serverVersion, mSiteTrimObj);
        }
        new public void Dispose()
        {
            mWebServiceRequest.Dispose();
            base.Dispose();
            //File.AppendAllText(@"C:\Trimmed Objects.txt", mSiteTrimObj.ToString());
        }

        #region Get
        public new Dictionary<string, object> GetNavigation(string webServerRelativeUrl)
        {
            #region zyq add
            string tmpUrl = string.Empty, webAppUrl = string.Empty, absoluteUrl = mSiteUrl;
            int firstIndex;
            if (mSiteUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                tmpUrl = mSiteUrl.Substring(7);
                firstIndex = tmpUrl.IndexOf('/');
                if (firstIndex < 0)
                {
                    if (webServerRelativeUrl.Equals("/"))
                    {
                        absoluteUrl = mSiteUrl;
                    }
                    else
                    {
                        absoluteUrl = mSiteUrl + webServerRelativeUrl;
                    }
                }
                else
                {
                    webAppUrl = tmpUrl.Substring(0, firstIndex);
                    absoluteUrl = "http://" + webAppUrl + webServerRelativeUrl;
                }
            }
            if (mSiteUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                tmpUrl = mSiteUrl.Substring(8);
                firstIndex = tmpUrl.IndexOf('/');
                if (firstIndex < 0)
                {
                    if (webServerRelativeUrl.Equals("/"))
                    {
                        absoluteUrl = mSiteUrl;
                    }
                    else
                    {
                        absoluteUrl = mSiteUrl + webServerRelativeUrl;
                    }
                }
                else
                {
                    webAppUrl = tmpUrl.Substring(0, firstIndex);
                    absoluteUrl = "https://" + webAppUrl + webServerRelativeUrl;
                }
            }
            #endregion

            Dictionary<string, object> nodesProperties = new Dictionary<string, object>();
            try
            {
                nodesProperties = mWebServiceRequest.GetNavigationNodesProperties(absoluteUrl);//mWebServiceRequest.GetNavigationNodesProperties(absoluteUrl);
            }
            catch (Exception ex)
            {
                mLogger.Warn("Get Web:{0} Navigation failed.Error Message:{1}", webServerRelativeUrl, ex.ToString());
            }
            return base.GetNavigation(webServerRelativeUrl, nodesProperties);
        }
        public new Dictionary<string, object> GetAttachments(string webRelativeUrl, string listTitle, int itemId)
        {
            Dictionary<string, object> urlCol = mWebServiceRequest.GetAttachments(webRelativeUrl, listTitle, itemId);
            List<Dictionary<string, object>> urlList = urlCol["UrlCol"] as List<Dictionary<string, object>>;
            Dictionary<string, object> attachProperties = new Dictionary<string, object>();
            List<Dictionary<string, object>> attachmentPropertiesList = new List<Dictionary<string, object>>();
            if (urlList.Count > 0)
            {
                foreach (Dictionary<string, object> fileUrl in urlList)
                {
                    string url = fileUrl["Url"] as string;
                    Dictionary<string, object> attachmentProperties = new Dictionary<string, object>();
                    Dictionary<string, object> fileProperties = base.GetFile(webRelativeUrl, url, listTitle);
                    //attachmentProperties.Add("ROWID", fileProperties["UniqueId"]);
                    attachmentProperties.Add("FileName", fileProperties.ContainsKey("Name") ? fileProperties["Name"] : fileProperties["LeafName"]);
                    attachmentProperties.Add("ServerRelativeUrl", url);
                    attachmentPropertiesList.Add(attachmentProperties);
                }
                attachProperties.Add(AveObjectModelConstant.ChildrenProperties, attachmentPropertiesList);
                string serverRelativeUrl = urlList[0]["Url"] as string;
                string webAppName = urlCol["webAppName"] as string;
                serverRelativeUrl = serverRelativeUrl.Substring(0, serverRelativeUrl.LastIndexOf('/') + 1);
                string urlPrefix = webAppName.TrimEnd('/') + "/" + serverRelativeUrl.TrimStart('/');
                attachProperties.Add("UrlPrefix", urlPrefix);
                return attachProperties;
            }
            else
            {
                attachProperties.Add(AveObjectModelConstant.ChildrenProperties, attachmentPropertiesList);
                return attachProperties;
            }
        }

        public new Dictionary<string, object> SearchPrincipals(string webServerRelativeUrl, string input, int scopes, int sources, int maxCount)
        {
            return mWebServiceRequest.SearchPrincipals(webServerRelativeUrl, input, scopes, sources, maxCount);
        }

        public new Dictionary<string, object> GetUsers(string webRelativeUrl, string groupName, string userColSource)
        {
            return mWebServiceRequest.GetUsers(webRelativeUrl, groupName, userColSource);
        }

        public new Dictionary<string, object> GetItemVersions(string webRelativeUrl, string listRelativeUrl, string listId, int itemId, string itemUrl, CultureInfo cultureInfo, Dictionary<string, string> needLoadFields)
        {
            return mWebServiceRequest.GetItemVersions(webRelativeUrl, listRelativeUrl, listId, itemId, itemUrl, cultureInfo, needLoadFields);
            //return mWebServiceRequest.GetItemVersionsWithMultiRequest(webRelativeUrl, listRelativeUrl, listId, itemId, itemUrl, needLoadFields);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        public Dictionary<string, object> GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid id, string dirName, string leafName, bool isListItem)
        {
            Dictionary<string, object> itemPro = base.GetItemExist(SiteId, webId, listId, id, dirName, leafName, isListItem);
            if (itemPro != null)
            {
                if (itemPro.ContainsKey("Versions"))
                {
                    List<Dictionary<string, object>> versions = (List<Dictionary<string, object>>)itemPro["Versions"];
                    string webRelativeUrl = itemPro["ServerRelativeUrl"].ToString();
                    Dictionary<string, string> needLoadFields = new Dictionary<string, string> { { "_UIVersion", "Integer" }, { "_Level", "Integer" } };
                    Dictionary<string, object> allVersionProperties = GetItemVersions(webRelativeUrl, string.Empty, listId.ToString(), (int)itemPro["ID"], itemPro["FullUrl"].ToString(), null, needLoadFields);
                    List<Dictionary<string, object>> versionProperties = (List<Dictionary<string, object>>)allVersionProperties["ChildrenProperties"];
                    foreach (Dictionary<string, object> version in versionProperties)
                    {
                        version["ID"] = (int)itemPro["ID"];
                        version["GUID"] = new Guid(itemPro["tp_GUID"].ToString());
                        version["Size"] = 0;
                        version["ObjType"] = itemPro["ObjType"];
                        version["TimeLastModified"] = version["Modified"];
                        int versionId = (int)version["VersionId"];
                        version["Level"] = (byte)version["Level"];
                        version["UIVersion"] = version["VersionId"];
                        version["UserDataGuid"] = version["GUID"];
                        version["IsCurrentVersion"] = versionId == (int)itemPro["UIVersion"] ? true : false;
                        versions.Add(version);
                    }
                }
                else
                {
                    List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
                    Dictionary<string, object> version = new Dictionary<string, object>();
                    version["ID"] = (int)itemPro["ID"];
                    version["GUID"] = new Guid(itemPro["tp_GUID"].ToString());
                    version["Size"] = 0;
                    version["ObjType"] = itemPro["ObjType"];
                    version["TimeLastModified"] = itemPro["TimeLastModified"];
                    version["UIVersion"] = itemPro["UIVersion"];
                    version["UserDataGuid"] = itemPro["tp_GUID"];
                    version["IsCurrentVersion"] = itemPro["IsCurrentVersion"];
                    version["Level"] = itemPro["Level"];
                    versions.Add(version);
                    itemPro["Versions"] = versions;
                }
            }
            return itemPro;

        }
        public new Dictionary<string, object> GetGroups(string webRelativeUrl, string groupColSource, string loginName)
        {
            if (groupColSource.Equals("web.siteGroups"))
            {
                return base.GetGroups(webRelativeUrl, groupColSource, loginName);
            }
            else
            {
                return mWebServiceRequest.GetGroups(webRelativeUrl, groupColSource, loginName);
            }
        }

        public new Dictionary<string, object> GetLimitedWebPartManager(string webServerRelativeUrl, string fileServerRelativeUrl, int personalizationScope, string appWebFulUrl = null)
        {
            return mWebServiceRequest.GetLimitedWebPartManager(webServerRelativeUrl, fileServerRelativeUrl, personalizationScope, appWebFulUrl);
        }
        public new Stream GetFileStream(string webServerRelativeUrl, string fileServerRelativeUrl, string source)
        {
            #region old code
            //if (source.Equals("File"))
            //{
            ////try
            ////{
            ////    return base.GetFileStream(webServerRelativeUrl, fileServerRelativeUrl, source);
            ////}
            ////catch
            ////{
            ////    return mWebServiceRequest.GetFileStream(webServerRelativeUrl, fileServerRelativeUrl, source);
            ////}
            //return mWebServiceRequest.GetFileStream(webServerRelativeUrl, fileServerRelativeUrl, source);
            //}
            //else if (source.Equals("Version"))
            //{
            //return mWebServiceRequest.GetFileStream(webServerRelativeUrl, fileServerRelativeUrl, source);
            //}
            //else
            //{
            //return null;
            //}
            #endregion
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }
            if (source.Equals("File", StringComparison.OrdinalIgnoreCase))
            {
                string filePath = fileServerRelativeUrl;
                if (!fileServerRelativeUrl.StartsWith(webServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    filePath = AveUrlUtility.CombineUrl(webServerRelativeUrl, fileServerRelativeUrl);
                }
                webServerRelativeUrl = AveUrlUtility.CombineUrl(webServerRelativeUrl, "_layouts/download.aspx?SourceUrl=");
            }
            return mWebServiceRequest.GetFileStream(webServerRelativeUrl, fileServerRelativeUrl, source);
        }
        public new byte[] GetFileBinary(string webServerRelativeUrl, string fileServerRelativeUrl, int options)
        {
            webServerRelativeUrl = AveUrlUtility.CombineUrl(webServerRelativeUrl, "_layouts/download.aspx?SourceUrl=");
            return mWebServiceRequest.GetFileBinary(webServerRelativeUrl, fileServerRelativeUrl, options);
        }
        public new Dictionary<string, object> GetUserProfileByName(string accountName, bool isOnlineSite)
        {
            return mWebServiceRequest.GetUserProfileByName(accountName, isOnlineSite);
        }
        public new Stream GetFileVersionStream(string webServerRelativeUrl, string fileServerRelativeUrl, string fileVerionServerRelativeUrl, int versionId)
        {
            return mWebServiceRequest.GetFileVersionStream(webServerRelativeUrl, fileServerRelativeUrl, fileVerionServerRelativeUrl, versionId);
        }
        public new Dictionary<string, object> GetWebTemplates(string webServerRelativeUrl, uint lcid, bool doIncludeCrossLanguage, string webtemplateSource)
        {
            return mWebServiceRequest.GetWebTemplates(webServerRelativeUrl, lcid, doIncludeCrossLanguage, webtemplateSource);
        }
        //public new Dictionary<string, object> GetWebTemplateConfiguration(string webRelativeUrl)
        //{
        //    return base.GetWebTemplateConfiguration(webRelativeUrl);
        //}
        //public Dictionary<string, object> GetLists(string webServerRelativeUrl)
        //{
        //    return mWebServiceRequest.GetLists(webServerRelativeUrl);
        //}
        //public new Dictionary<string, object> GetListAssociastedProperty(string webServerRelativeUrl, string listTitle)
        //{
        //    return mWebServiceRequest.GetListAssociastedProperty(webServerRelativeUrl, listTitle);
        //}
        public new Dictionary<string, object> GetSitePortal(string siteUrl)
        {
            return mWebServiceRequest.GetSitePortal(siteUrl);
        }
        public new List<string> GetSiteEnabledHelpCollections()
        {
            return mWebServiceRequest.GetSiteEnabledHelpCollections();
        }
        public new bool GetListRated(string webServerRelativeUrl, Guid listId)
        {
            return mWebServiceRequest.GetListRated(webServerRelativeUrl, listId);
        }
        public new int GetListItemRatings(string listItemUrl)
        {
            return mWebServiceRequest.GetListItemRatings(listItemUrl);
        }
        public new Dictionary<string, object> GetMetadataNavigationSettings(string webServerRelativeUrl, Guid listId, string listTitle)
        {
            return mWebServiceRequest.GetMetadataNavigationSettings(webServerRelativeUrl, listId, listTitle);
        }
        public new List<Dictionary<string, object>> GetListCheckOutFiles(string webServerRelativeUrl, Guid listId)
        {
            return mWebServiceRequest.GetListCheckOutFiles(webServerRelativeUrl, listId);
        }
        public new Dictionary<string, object> GetMetadataListFieldSettings(string webServerRelativeUrl, string listTitle, Guid listId)
        {
            return mWebServiceRequest.GetMetadataListFieldSettings(webServerRelativeUrl, listTitle, listId);
        }
        public new Dictionary<string, object> GetListVersionLimited(string webServerRelativeUrl, Guid listId)
        {
            return mWebServiceRequest.GetListVersionLimited(webServerRelativeUrl, listId);
        }
        public new Dictionary<string, object> GetPerLocationViewSettings(string webServerRelativeUrl, Guid listId)
        {
            return mWebServiceRequest.GetPerLocationViewSettings(webServerRelativeUrl, listId);
        }
        public new Dictionary<string, object> GetListRssProperties(string webServerRelativeUrl, Guid listId)
        {
            return mWebServiceRequest.GetListRssProperties(webServerRelativeUrl, listId);
        }
        public new List<Dictionary<string, object>> GetPublishedContentTypes()
        {
            return mWebServiceRequest.GetPublishedContentTypes();
        }
        public Dictionary<string, object> GetListGeneralProperties(String webServerRelativeUrl, Guid listId)
        {
            return mWebServiceRequest.GetListGeneralProperties(webServerRelativeUrl, listId);
        }
        public new Dictionary<string, object> GetListEditViewSettingProperties(string webServerRelativeUrl, String listTitle, Guid listId, Guid viewId)
        {
            return mWebServiceRequest.GetListEditViewSettingProperties(webServerRelativeUrl, listTitle, listId, viewId);
        }
        public Dictionary<string, object> GetListAccessRequestsSettingProperties(String webServerRelativeUrl, Guid listId)
        {
            return mWebServiceRequest.GetListAccessRequestsSettingProperties(webServerRelativeUrl, listId);
        }
        public new Dictionary<string, object> GetListAdvancedSettingProperties(string webServerRelativeUrl, Guid listId)
        {
            return mWebServiceRequest.GetListAdvancedSettingProperties(webServerRelativeUrl, listId);
        }
        public new List<Dictionary<string, object>> GetDisplayGroupsForSite()
        {
            return mWebServiceRequest.GetDisplayGroupsForSite();
        }
        public new List<Dictionary<string, object>> GetKeyWords()
        {
            return mWebServiceRequest.GetKeyWords();
        }
        public new Dictionary<string, object> GetWebLogoProperties(string webServerRelativeUrl)
        {
            return mWebServiceRequest.GetWebLogoProperties(webServerRelativeUrl);
        }
        public new Dictionary<string, object> GetWorkflowAssociations(string webServerRelativeUrl, string listName, Guid listId, string workflowSource, Dictionary<string, object> contentTypeProp)
        {
            if (workflowSource == "list.workflow" || workflowSource == "contentType.workflow")
            {
                Dictionary<string, object> clientAPIProperties = null;
                if (contentTypeProp != null && contentTypeProp.ContainsKey("ContentTypeSource") && (contentTypeProp["ContentTypeSource"].Equals("web.availableContentTypes") || contentTypeProp["ContentTypeSource"].Equals("web.contentTypes")))
                {
                    clientAPIProperties = base.GetWorkflowAssociations(webServerRelativeUrl, listName, listId, workflowSource, contentTypeProp);
                }
                else
                {
                    clientAPIProperties = base.GetWorkflowAssociations(webServerRelativeUrl, listName, listId, workflowSource, contentTypeProp);
                    Dictionary<string, object> webRequestProperties = mWebServiceRequest.GetWorkflowAssociations(webServerRelativeUrl, listName, listId, workflowSource, contentTypeProp);
                    List<Dictionary<string, object>> workfolws = clientAPIProperties[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>;
                    foreach (Dictionary<string, object> workflowProp in workfolws)
                    {
                        workflowProp["RunningInstances"] = webRequestProperties[workflowProp["Name"].ToString()];
                    }
                }
                return clientAPIProperties;
            }
            else
            {
                return base.GetWorkflowAssociations(webServerRelativeUrl, listName, listId, workflowSource, contentTypeProp);
            }
        }

        public new Dictionary<string, object> GetWebRegionalSetting(string webServerRelativeUrl)
        {
            return mWebServiceRequest.GetWebRegionalSetting(webServerRelativeUrl);
        }
        public new Dictionary<string, object> GetAllFeatureDefinitions(string Url, string featuresSource)
        {
            return mWebServiceRequest.GetAllFeatureDefinitions(Url, featuresSource);
        }

        public new Dictionary<string, object> GetDefaultRegionalSetting(string webServerRelativeUrl, int lcid)
        {
            return mWebServiceRequest.GetDefaultRegionalSetting(webServerRelativeUrl, lcid);
        }
        public new Dictionary<string, object> GetThemeUrlForWeb(string webServerRelativeUrl, int compatibilityLevel)
        {
            return mWebServiceRequest.GetThemeUrlForWeb(webServerRelativeUrl, compatibilityLevel);
        }
        public new Dictionary<string, object> GetThmxThemeInfo(string webServerRelativeUrl)
        {
            return mWebServiceRequest.GetThmxThemeInfo(webServerRelativeUrl);
        }
        public new Dictionary<string, object> GetMasterPageProperties(string webServerRelativeUrl)
        {
            return mWebServiceRequest.GetMasterPageProperties(webServerRelativeUrl);
        }
        public new bool GetSiteRssSetting()
        {
            return mWebServiceRequest.GetSiteRssSetting();
        }

        public AveRequestAudit GetAuditValues()
        {
            return mWebServiceRequest.GetAuditValues();
        }
        #endregion

        #region Add
        public override Dictionary<string, object> AddAttachmentNow(string webRelativeUrl, string listName, Guid listId, int itemId, string leafName, byte[] attachment)
        {
            Dictionary<string, object> item = base.GetItem(webRelativeUrl, listName, listId, itemId, default(Guid));
            Dictionary<string, object> attach = mWebServiceRequest.AddAttachmentNow(webRelativeUrl, listName, listId, itemId, leafName, attachment);
            Dictionary<string, object> keeps = new Dictionary<string, object>();
            Dictionary<string, object> itemPros = new Dictionary<string, object>();
            #region Reset Modified time to keep modified time property
            itemPros.Add("Modified", item["TimeLastModified"]);
            itemPros.Add("_ModerationStatus", item["_ModerationStatus"]);
            #endregion
            keeps[AveObjectModelConstant.UpdateMethodName] = "Update";
            keeps["ChangedFieldValues"] = itemPros;
            base.UpdateItem(webRelativeUrl, listName, listId, itemId, keeps);
            return attach;
        }

        public new Dictionary<string, object> AddGroup(string webRelativeUrl, string ownerName, string ownerType, string defaultUserName, string groupName, string description, string groupSource)
        {
            return base.AddGroup(webRelativeUrl, ownerName, ownerType, defaultUserName, groupName, description, groupSource);
            //return mWebServiceRequest.AddGroup(webRelativeUrl, ownerName, ownerType, defaultUserName, groupName, description, groupSource);
        }

        public new Dictionary<string, object> AddUser(string webServerRelativeUrl, string source, string groupName, Dictionary<string, object> userProp)
        {
            return mWebServiceRequest.AddUser(webServerRelativeUrl, source, groupName, userProp);
        }

        public new Dictionary<string, object> AddFeature(string webServerRelativeUrl, Guid featureId, bool force, int scope, string featuresSource)
        {
            Dictionary<string, object> featureProp = new Dictionary<string, object>();
            featureProp = mWebServiceRequest.AddFeature(webServerRelativeUrl, featureId, force, scope, featuresSource);
            return featureProp;
        }

        public Dictionary<string, object> AddAlert(string webServerRelativeUrl, string listUrl, string listTitle, Guid listId, int itemId, Dictionary<string, object> data)
        {
            return mWebServiceRequest.AddAlert(webServerRelativeUrl, listUrl, listTitle, listId, itemId, data);
        }

        public new void AddViewToAllNodes(string webServerRelativeUrl, Guid listId, Guid viewId)
        {
            mWebServiceRequest.AddViewToAllNodes(webServerRelativeUrl, listId, viewId);
        }

        public new Dictionary<string, object> AddKeyWord(string term, DateTime startDate, int localId, int calendarType)
        {
            return mWebServiceRequest.AddKeyWord(term, startDate, localId, calendarType);
        }

        public new string AddSynonm(string term, string synTerm, string terms)
        {
            return mWebServiceRequest.AddSynonm(term, synTerm, terms);
        }

        public new Dictionary<string, object> AddBestBet(string term, List<string> bestBetUrlList, Dictionary<string, object> bestBetProp, string action)
        {
            return mWebServiceRequest.AddBestBet(term, bestBetUrlList, bestBetProp, action);
        }

        public new void AddTag(string url, Guid termId, string title, bool? isPrivate)
        {
            mWebServiceRequest.AddTag(url, termId, title, isPrivate);
        }
        public new void AddComment(string url, string comment, bool? isHighPriority, string title)
        {
            mWebServiceRequest.AddComment(url, comment, isHighPriority, title);
        }

        public new Dictionary<string, object> AddUserProfile(string accountName)
        {
            return this.mWebServiceRequest.AddUserProfile(accountName);
        }

        #endregion

        #region
        public string AssociateWorkflowMarkup(string webServerRelativeUrl, string configUrl, string configVersion)
        {
            return mWebServiceRequest.AssociateWorkflowMarkup(webServerRelativeUrl, configUrl, configVersion);
        }

        public void BrowserEnableUserFormTemplate(string formTemplateUrl)
        {
            mWebServiceRequest.BrowserEnableUserFormTemplate(formTemplateUrl);
        }
        #endregion

        #region Delete
        public new void DeleteFileVersion(string fileServerRelativeUrl, string webServerRelativeUrl, string versionLabel)
        {
            mWebServiceRequest.DeleteFileVersion(webServerRelativeUrl, fileServerRelativeUrl, versionLabel);
        }

        public new void DeleteUser(string webServerRelativeUrl, string source, string groupName, string loginName)
        {
            mWebServiceRequest.DeleteUser(webServerRelativeUrl, source, groupName, loginName);
        }

        //public override void DeleteAttachmentNow(string webServerRelativeUrl, string listServerRelativeUrl, string listTitle, int itemId, string leafName)
        //{
        //    mWebServiceRequest.DeleteAttachmentNow(webServerRelativeUrl, listServerRelativeUrl, listTitle, itemId, leafName);
        //}

        public new void DeleteTag(string url, Guid termId)
        {
            mWebServiceRequest.DeleteTag(url, termId);
        }
        #endregion

        #region Restore
        //public new void RestoreWebParts(string webServerRelativeUrl, string fileServerRelativeUrl, int scope, List<AveWebPartBaseInfo> webpartBaseInfoList)
        //{
        //    mWebServiceRequest.RestoreWebParts(webServerRelativeUrl, fileServerRelativeUrl, scope, webpartBaseInfoList);
        //}

        public new Dictionary<string, object> RestoreUserProfileInfo(Dictionary<string, object> userProfileInfo, bool isOnlineSite, bool isExistSkip)
        {
            return this.mWebServiceRequest.RestoreUserProfileInfo(userProfileInfo, isOnlineSite, isExistSkip);
        }

        public new List<Dictionary<string, object>> RestoreFeatures(string webServerRelativeUrl, bool force, int scope, string featuresSource, List<Dictionary<string, object>> featureInfoList)
        {
            return mWebServiceRequest.RestoreFeatures(webServerRelativeUrl, force, scope, featuresSource, featureInfoList);
        }

        public new bool RestoreNavigation(string webServerRelativeUrl, string nodes, System.Collections.Hashtable webAllProperties)
        {
            bool compoundSupport = mWebServiceRequest.RestoreNavigation(webServerRelativeUrl, nodes, webAllProperties);
            return compoundSupport;
        }

        public new void RestoreTheme(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebSettingInfo webSettingInfo, string themedCssFolderUrl)
        {
            mWebServiceRequest.RestoreTheme(webServerRelativeUrl, siteServerRelativeUrl, webSettingInfo, themedCssFolderUrl);
        }
        public new void RestoreMasterPage(string webServerRelativeUrl, string siteServerRelativeUrl, AveWebMasterPageInfo pageInfo, string alternateCssUrl)
        {
            mWebServiceRequest.RestoreMasterPage(webServerRelativeUrl, siteServerRelativeUrl, pageInfo, alternateCssUrl);
        }
        #endregion

        #region Update

        public new Dictionary<string, object> UpdateUser(string webServerRelativeUrl, string loginName, string name, string userColSource, Dictionary<string, object> userProp)
        {
            return mWebServiceRequest.UpdateUser(webServerRelativeUrl, loginName, name, userColSource, userProp);
        }
        public new Dictionary<string, object> UpdateList(string webServerRelativeUrl, string listName, Guid listId, Dictionary<string, object> listProperties)
        {
            int type = listProperties.ContainsKey("ListType") ? (int)listProperties["ListType"] : -1;
            listProperties.Remove("ListType");
            if (type == (int)AveListTemplateType.Survey)
            {
                if (listProperties.ContainsKey("NoCrawl"))
                {
                    Dictionary<string, object> dicPro = new Dictionary<string, object>();
                    dicPro["NoCrawl"] = listProperties["NoCrawl"];
                    base.UpdateList(webServerRelativeUrl, listName, listId, dicPro);
                    listProperties.Remove("NoCrawl");
                }
                return mWebServiceRequest.UpdateList(webServerRelativeUrl, listName, listId, listProperties);//更新Nocrawl的时候不作用。ADO-48559
            }
            else
            {
                Dictionary<string, object> versionLimitedProperties = new Dictionary<string, object>();
                SetVersionSetting(versionLimitedProperties, listProperties);
                Dictionary<string, object> advancedSettingProp = new Dictionary<string, object>();
                SetAdvancedSetting(advancedSettingProp, listProperties);
                Dictionary<string, object> generalSettings = new Dictionary<string, object>();
                SetGeneralSetting(generalSettings, listProperties);
                Dictionary<string, object> updatedListProperties = base.UpdateList(webServerRelativeUrl, listName, listId, listProperties);
                if (advancedSettingProp.Count > 0)
                {
                    mWebServiceRequest.UpdateListAdvancedSetting(webServerRelativeUrl, listId, advancedSettingProp);
                }
                if (versionLimitedProperties.Count > 0)
                {
                    mWebServiceRequest.SetListVersionLimited(webServerRelativeUrl, listId, versionLimitedProperties);
                }
                if (generalSettings.Count > 0)
                {
                    mWebServiceRequest.UpdateListGeneralSetting(webServerRelativeUrl, listId, generalSettings);
                }
                return updatedListProperties;
            }
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Rad is a part of keys")]
        private void SetAdvancedSetting(Dictionary<string, object> advancedSettingProp, Dictionary<string, object> listProperties)
        {
            if (listProperties.ContainsKey("ExcludeFromOfflineClient"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AllowSyncSection$ctl01$AllowSync"] = (bool)listProperties["ExcludeFromOfflineClient"] ? "RadAllowSyncNo" : "RadAllowSyncYes";
                listProperties.Remove("ExcludeFromOfflineClient");
            }
            if (listProperties.ContainsKey("DisableGridEditing"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AllowGridEditingSection$ctl01$AllowGrid"] = (bool)listProperties["DisableGridEditing"] ? "RadAllowGridNo" : "RadAllowGridYes";
                listProperties.Remove("DisableGridEditing");
            }
            if (listProperties.ContainsKey("NavigateForFormsPages"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$DialogForFormsPagesSection$ctl02$DialogForFormsPages"] = (bool)listProperties["NavigateForFormsPages"] ? "RadDialogForFormsPagesNo" : "RadDialogForFormsPagesYes";
                listProperties.Remove("NavigateForFormsPages");
            }
            if (listProperties.ContainsKey("IsSiteAssetsLibrary"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AttachmentLibrarySection$ctl01$AttachmentLibrary"] = (bool)listProperties["IsSiteAssetsLibrary"] ? "RadAttachmentLibraryYes" : "RadAttachmentLibraryNo";
                listProperties.Remove("IsSiteAssetsLibrary");
            }
            if (listProperties.ContainsKey("EnableAttachments"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$AttachmentsSection$ctl01$DisableAttachments"] = (bool)listProperties["EnableAttachments"] ? "RadAttachmentsEnabled" : "RadAttachmentsDisabled";
                listProperties.Remove("EnableAttachments");
            }
            if (listProperties.ContainsKey("DefaultItemOpen"))
            {
                if ((int)listProperties["DefaultItemOpen"] == 0)
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$OpenDocumentSection$ctl00$DefaultItemOpen"] = "RadDefaultItemOpenPreferClient";
                }
                else if (listProperties.ContainsKey("DefaultItemOpenUseListSetting") && !(bool)listProperties["DefaultItemOpenUseListSetting"])
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$OpenDocumentSection$ctl00$DefaultItemOpen"] = "RadDefaultItemOpenServerSetting";
                }
                else
                {
                    advancedSettingProp["ctl00$PlaceHolderMain$OpenDocumentSection$ctl00$DefaultItemOpen"] = "RadDefaultItemOpenBrowser";
                }
                listProperties.Remove("DefaultItemOpen");
            }
            if (listProperties.ContainsKey("SendToLocationName"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$SendToSection$ctl00$TxtSendToLocationName"] = listProperties["SendToLocationName"];
                listProperties.Remove("SendToLocationName");
            }
            if (listProperties.ContainsKey("SendToLocationUrl"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$SendToSection$ctl01$TxtSendToLocationUrl"] = listProperties["SendToLocationUrl"];
                listProperties.Remove("SendToLocationUrl");
            }
            if (listProperties.ContainsKey("ReadSecurity"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$ItemLevelSecuritySection$ctl08$ReadSecurity"] = listProperties["ReadSecurity"];
                listProperties.Remove("ReadSecurity");
            }
            if (listProperties.ContainsKey("WriteSecurity"))
            {
                advancedSettingProp["ctl00$PlaceHolderMain$ItemLevelSecuritySection$ctl09$WriteSecurity"] = listProperties["WriteSecurity"];
                listProperties.Remove("WriteSecurity");
            }
        }
        private void SetVersionSetting(Dictionary<string, object> versionLimitedProperties, Dictionary<string, object> listProperties)
        {
            if (listProperties.ContainsKey("MajorVersionLimit"))
            {
                versionLimitedProperties.Add("MajorVersionLimit", listProperties["MajorVersionLimit"]);
                listProperties.Remove("MajorVersionLimit");
            }
            if (listProperties.ContainsKey("MajorWithMinorVersionsLimit"))
            {
                versionLimitedProperties.Add("MajorWithMinorVersionsLimit", listProperties["MajorWithMinorVersionsLimit"]);
                listProperties.Remove("MajorWithMinorVersionsLimit");
            }
        }

        private void SetGeneralSetting(Dictionary<string, object> generalSettings, Dictionary<string, object> listProperties)
        {
            if (listProperties.ContainsKey("EnablePeopleSelector"))//calendar general setting.
            {
                generalSettings["ctl00$PlaceHolderMain$EventSection$ctl00$enablePeopleSelector"] = (bool)listProperties["EnablePeopleSelector"] ? "RadEnablePeopleSelectorYes" : "RadEnablePeopleSelectorNo";
                listProperties.Remove("EnablePeopleSelector");
            }
        }
        public new Dictionary<string, object> UpdateSite(Dictionary<string, object> siteProperties)
        {
            Dictionary<string, object> needAddProperties = new Dictionary<string, object>();//由于下面用webservice还原的properties没有最终加到返回列表中，用这个去返回
            if (siteProperties.ContainsKey("PortalUrl") || siteProperties.ContainsKey("PortalName"))
            {
                Dictionary<string, object> portalProperties = new Dictionary<string, object>();
                if (siteProperties.ContainsKey("PortalName"))
                {
                    portalProperties.Add("PortalName", siteProperties["PortalName"]);
                    needAddProperties.Add("PortalName", siteProperties["PortalName"]);
                    siteProperties.Remove("PortalName");
                }
                if (siteProperties.ContainsKey("PortalUrl"))
                {
                    portalProperties.Add("PortalUrl", siteProperties["PortalUrl"]);
                    needAddProperties.Add("PortalUrl", siteProperties["PortalUrl"]);
                    siteProperties.Remove("PortalUrl");
                }
                mWebServiceRequest.UpdateSitePortal(portalProperties);
            }
            //if (siteProperties.ContainsKey("SyndicationEnabled"))
            //{
            //    mWebServiceRequest.UpdateSiteRssSetting(Convert.ToBoolean(siteProperties["SyndicationEnabled"]));
            //    needAddProperties.Add("SyndicationEnabled", siteProperties["SyndicationEnabled"]);
            //    siteProperties.Remove("SyndicationEnabled");
            //}
            if (siteProperties.Count > 0)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties = base.UpdateSite(siteProperties);
                foreach (string key in needAddProperties.Keys)
                {
                    if (!properties.ContainsKey(key))
                    {
                        properties.Add(key, needAddProperties[key]);
                    }
                }
                return properties;
            }
            return needAddProperties;
        }
        public new void UpdateMetadataListFieldSettings(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProperties)
        {
            mWebServiceRequest.UpdateMetadataListFieldSettings(webServerRelativeUrl, listId, updateProperties);
        }
        public new void UpdateListRssSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> updateProp)
        {
            mWebServiceRequest.UpdateListRssSetting(webServerRelativeUrl, listId, updateProp);
        }
        public new Dictionary<string, object> UpdateSiteAdministrators(string webServerRelativeUrl, string oldSiteAdmins, List<Dictionary<string, object>> administrators)
        {
            return mWebServiceRequest.UpdateSiteAdministrators(webServerRelativeUrl, oldSiteAdmins, administrators);
        }
        public new void UpdateScopeDisplayGroup(int groupId, string groupName, Dictionary<string, object> updateProp)
        {
            mWebServiceRequest.UpdateScopeDisplayGroup(groupId, groupName, updateProp);
        }
        public new Dictionary<string, object> UpdateKeyWord(string term, int localId, int calendarType, Dictionary<string, object> keyWordProp)
        {
            return mWebServiceRequest.UpdateKeyWord(term, localId, calendarType, keyWordProp);
        }
        public override Dictionary<string, object> UpdateContentType(string webServerRelativeUrl, string listName, Guid listId, string contentTypeId, bool updateChildren, string contentTypeSource, Dictionary<string, object> needUpdateContentTypeProperties)
        {
            base.UpdateContentType(webServerRelativeUrl, listName, listId, contentTypeId, updateChildren, contentTypeSource, needUpdateContentTypeProperties);
            return mWebServiceRequest.UpdateContentType(webServerRelativeUrl, listName, listId, contentTypeId, updateChildren, contentTypeSource, needUpdateContentTypeProperties);
        }

        public virtual Dictionary<string, object> UpdateAudit(Dictionary<string, object> needUpdateProperties)
        {
            return mWebServiceRequest.UpdateAudit(needUpdateProperties);
        }
        #endregion

        #region set
        public new void SetSiteEnabledHelpCollections(string[] enabledHelpCollections)
        {
            mWebServiceRequest.SetSiteEnabledHelpCollections(enabledHelpCollections);
        }
        public new bool SetListRating(string webServerRelativeUrl, string listUrl, Guid listId, bool enableRating)
        {
            return mWebServiceRequest.SetListRating(webServerRelativeUrl, listUrl, listId, enableRating);
        }
        public new void SetMetadataNavigationSettings(string webServerRelativeUrl, string listTitle, Guid listId, Dictionary<string, object> updateProperties)
        {
            mWebServiceRequest.SetMetadataNavigationSettings(webServerRelativeUrl, listTitle, listId, updateProperties);
        }
        public new void SetPerLocalViewSetting(string webServerRelativeUrl, Guid listId, Dictionary<string, object> viewSettingProp)
        {
            mWebServiceRequest.SetPerLocalViewSetting(webServerRelativeUrl, listId, viewSettingProp);
        }
        public new Dictionary<string, object> CreateScopeDisPlayGroup(string name, string description, Uri owningSiteUrl, bool displayInAdminUI)
        {
            return mWebServiceRequest.CreateScopeDisPlayGroup(name, description, owningSiteUrl, displayInAdminUI);
        }
        public new Dictionary<string, object> CreateScope(string name, string description, Uri owningSiteUrl, bool displayInAdminUI, string alternateResultsPage, string compilationType, string filter)
        {
            return mWebServiceRequest.CreateScope(name, description, owningSiteUrl, displayInAdminUI, alternateResultsPage, compilationType, filter);
        }
        public new Dictionary<string, object> AddList(string webServerRelativeUrl, string title, string description, IAveListTemplate listTemplate)
        {
            mWebServiceRequest.AddList(webServerRelativeUrl, title, description, listTemplate);
            return base.GetList(webServerRelativeUrl, title);
        }
        public new Dictionary<string, object> UpdateWeb(string webServerRelativeUrl, Dictionary<string, object> webProperties)
        {
            Dictionary<string, object> webProp = new Dictionary<string, object>();
            webProp = base.UpdateWeb(webServerRelativeUrl, webProperties);
            if (webProperties.ContainsKey("SiteLogoUrl") || webProperties.ContainsKey("SiteLogoDescription") || webProperties.ContainsKey("Name"))
            {
                if (webProperties.ContainsKey("SiteLogoUrl"))
                {
                    webProp["SiteLogoUrl"] = webProperties["SiteLogoUrl"];
                }
                if (webProperties.ContainsKey("SiteLogoDescription"))
                {
                    webProp["SiteLogoDescription"] = webProperties["SiteLogoDescription"];
                }
                //if (webProperties.ContainsKey("Name"))
                //{
                //    webProp["Name"] = webProperties["Name"];
                //}
                mWebServiceRequest.UpdateWebLogo(webServerRelativeUrl, webProperties);
            }
            if (webProperties.ContainsKey("NoCrawl") && webProperties.ContainsKey("ASPXPageIndexMode") && webProperties.ContainsKey("ExcludeFromOfflineClient"))
            {
                mWebServiceRequest.UpdateWebSearchAndOfflineAvailability(webServerRelativeUrl, webProperties);
                webProp["NoCrawl"] = webProperties["NoCrawl"];
                //由于NoCrawl这个属性是用Web Service还原的，而又因为在AllProperties里面也有一个这样的字段，并且没有更新到webPro里面，所以加到里面。
                //注：在web.NoCrawl里面这个属性是bool类型，而在AllProperties里面这个字段是String类型。
                Dictionary<string, object> tempProp = new Dictionary<string, object>();
                if (webProp.ContainsKey("AllProperties" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    tempProp = webProp["AllProperties" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                    if (tempProp.ContainsKey("NoCrawl"))
                    {
                        tempProp["NoCrawl"] = webProperties["NoCrawl"].ToString();
                    }
                }
                webProp["ASPXPageIndexMode"] = webProperties["ASPXPageIndexMode"];
                webProp["ExcludeFromOfflineClient"] = webProperties["ExcludeFromOfflineClient"];
            }
            if (webProperties.ContainsKey("RegionalSettingsChangedProperties"))
            {
                Dictionary<string, object> regionalProp = webProperties["RegionalSettingsChangedProperties"] as Dictionary<string, object>;
                Dictionary<string, object> newProp = new Dictionary<string, object>();
                newProp = mWebServiceRequest.UpdateWebRegionalSetting(webServerRelativeUrl, regionalProp);
                webProp["RegionalSettings" + AveObjectModelConstant.ObjectPropertySuffix] = newProp;
            }
            return webProp;
        }
        public new void RevertAllDocumentContentStreams(string webServerRelativeUrl)
        {
            mWebServiceRequest.RevertAllDocumentContentStreams(webServerRelativeUrl);
        }
        public new void RevertContentStream(string webServerRelativeUrl, string fileUrl)
        {
            mWebServiceRequest.RevertContentStream(webServerRelativeUrl, fileUrl);
        }
        public new void MoveNavigationNode(string webServerRelativeUrl, Dictionary<string, object> navigationNodeProperties, Dictionary<string, object> previousNodeProperties, string moveMethodName)
        {
            if (moveMethodName.Equals("MoveToCollection"))
            {
                mWebServiceRequest.MoveNavigationNodeToCollection(webServerRelativeUrl, navigationNodeProperties);
            }
            else
            {
                mWebServiceRequest.MoveNavigationNode(webServerRelativeUrl, navigationNodeProperties, previousNodeProperties, moveMethodName);
            }
        }
        public new void UpdateSiteRssSetting(bool syndicationEnabled)
        {
            mWebServiceRequest.UpdateSiteRssSetting(syndicationEnabled);
        }
        #endregion

        public AveWebBrowserInfo GetBrowserRootWeb(AveBrowserOption option)
        {
            AveWebBrowserInfo webBrowserInfo = base.GetBrowserRootWeb(option);
            try
            {
                webBrowserInfo.TemplateName = GetWebTemplateConfiguration(webBrowserInfo.ServerRelativeUrl);
            }
            catch (Exception e)
            {
                mLogger.Warn("Can not get web template web url: {0}, exception: {1}.", webBrowserInfo.Url, e.ToString());
            }
            return webBrowserInfo;
        }

        public override List<AveItemVersionBrowserInfo> GetBrowserItemVersions(AveBrowserOption option)
        {
            List<AveItemVersionBrowserInfo> ItemVersionsInfoList = new List<AveItemVersionBrowserInfo>();
            Dictionary<string, string> needLoadFields = new Dictionary<string, string>();
            Guid listId = Guid.Empty;
            needLoadFields.Add("_UIVersionString", "Text");
            int itemId = GetItemIdByUniqueId(option.ParentWebServerRelativeUrl, option.ParentItemUniqueId, option.ParentListTitle, ref listId);
            Dictionary<string, object> versionsInfo = mWebServiceRequest.GetItemVersions(option.ParentWebServerRelativeUrl, listId.ToString(), itemId, needLoadFields);
            if ((versionsInfo[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>).Count < 1)
            {
                return null;
            }
            List<Dictionary<string, object>> versionLabels = (versionsInfo[AveObjectModelConstant.ChildrenProperties] as List<Dictionary<string, object>>);
            if (versionLabels.Count > 0)
            {
                int pagingCount = 0;
                option.ChildrenTotalCount = versionLabels.Count;
                if (option.ChildrenTotalCount - option.StartIndex < option.PerPage)
                {
                    pagingCount = option.ChildrenTotalCount - option.StartIndex;
                }
                else
                {
                    pagingCount = (int)option.PerPage;
                }
                try
                {
                    for (int i = 0; i < pagingCount; i++)
                    {
                        AveItemVersionBrowserInfo versionInfo = new AveItemVersionBrowserInfo();
                        versionInfo.VersionLabel = (versionLabels[i + option.StartIndex]["FieldValues"] as Dictionary<string, object>)["_UIVersionString"].ToString();
                        ItemVersionsInfoList.Add(versionInfo);
                    }
                }
                catch (Exception ex)
                {
                    mLogger.Warn("StartIndex Out of Range when getting browserWebs.StartIndex: {0}, ChildrenCount: {1}, ErrorMessage: {2}", option.StartIndex, option.ChildrenTotalCount, ex.ToString());
                }
            }
            return ItemVersionsInfoList;
        }

        public List<AveWebBrowserInfo> GetBrowserWebs(AveBrowserOption option)
        {
            List<AveWebBrowserInfo> webInfoList = base.GetBrowserWebs(option);
            foreach (AveWebBrowserInfo webinfo in webInfoList)
            {
                try
                {
                    webinfo.TemplateName = GetWebTemplateConfiguration(webinfo.ServerRelativeUrl);
                }
                catch (Exception e)
                {
                    mLogger.Warn("Can not get web template web url:{0}, exception:{1}.", webinfo.Url, e.ToString());
                }
            }
            return webInfoList;
        }

        #region Discovery
        public Dictionary<string, object> QueryListItemForFB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, bool isDiscover, bool includeSystemFolder = false)
        {
            Dictionary<string, string> needLoadFields = new Dictionary<string, string>();
            //needLoadFields.Add("ID", "Counter");
            //needLoadFields.Add("GUID", "Guid");
            needLoadFields.Add("_Level", "Integer");
            //needLoadFields.Add("_IsCurrentVersion", "Boolean");
            needLoadFields.Add("_UIVersion", "Integer");
            Dictionary<string, object> folder = base.QueryListItemForFB(siteId, webId, listId, folderId, folderUrl,isDiscover);
            List<Dictionary<string, object>> items = (List<Dictionary<string, object>>)folder["Items"];
            if (!listId.Equals(Guid.Empty))
            {
                string webUrl = folder.ContainsKey("WebServerRelativeUrl") ?
                    folder["WebServerRelativeUrl"].ToString() : base.GetWeb(webId)["ServerRelativeUrl"].ToString();
                List<Task> getItemVersionTasks = new List<Task>();
                items.ForEach((item) =>
                {
                    if (item.ContainsKey("Versions") && WrapperConfiguration.BPOS_S.IncludeVersionForPerformance)
                    {
                        getItemVersionTasks.Add(() => { GetListItemVersion(item, webUrl, listId, needLoadFields); });
                    }
                    else
                    {// list enable version is false, we just add current version here
                        List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
                        AssembleItemVersionProperty(item, versions);
                        item["HasVersion"] = false;
                    }
                });
                if (getItemVersionTasks.Count > 0)
                {
                    using (AveTaskExecutor taskExecutor = new AveTaskExecutor(WrapperConfiguration.BPOS_S.MaximumThreadsGettingVersions))
                    {
                        taskExecutor.Execute(getItemVersionTasks);
                    }
                }
            }
            else
            {
                items.ForEach((item) =>
                {
                    List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
                    AssembleWebItemVersionProperty(item, versions);
                    item["HasVersion"] = false;
                });
            }
            return folder;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        private void GetListItemVersion(Dictionary<string, object> item, string webUrl, Guid listId, Dictionary<string, string> needLoadFields)
        {
            List<Dictionary<string, object>> versions = (List<Dictionary<string, object>>)item["Versions"];
            Dictionary<string, object> allVersionProperties = GetItemVersions(webUrl,string.Empty, listId.ToString(), (int)item["Id"], "", null, needLoadFields);
            if (allVersionProperties.ContainsKey("HasVersion") && !Convert.ToBoolean(allVersionProperties["HasVersion"]))
            {
                AssembleItemVersionProperty(item, versions);
                item["HasVersion"] = false;
            }
            else
            {
                List<Dictionary<string, object>> versionProperties = (List<Dictionary<string, object>>)allVersionProperties["ChildrenProperties"];
                foreach (Dictionary<string, object> version in versionProperties)
                {
                    version["ID"] = (int)item["Id"];
                    version["GUID"] = new Guid(item["GUID"].ToString());
                    version["Size"] = 0;
                    version["ObjType"] = item["ObjType"];
                    version["TimeLastModified"] = version["Modified"];
                    int versionId = (int)version["VersionId"];
                    if (!version.ContainsKey("Level"))
                    {
                        version["Level"] = (byte)1;
                    }
                    version["UIVersion"] = version["VersionId"];
                    version["UserDataGuid"] = version["GUID"];
                    object fieldValues;
                    if (item.TryGetValue("FieldValues", out fieldValues) && fieldValues != null)
                    {
                        version["IsCurrentVersion"] = versionId == (int)((Dictionary<string, object>)fieldValues)["_UIVersion"];
                    }
                    else
                    {
                        version["IsCurrentVersion"] = versionId == (int)item["UIVersion"];
                    }
                    versions.Add(version);
                }
            }
        }

        public override void DeleteSite(string CAUrl, string url)
        {
            mWebServiceRequest.DeleteSite(CAUrl, url);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        private void AssembleItemVersionProperty(Dictionary<string, object> item, List<Dictionary<string, object>> versions)
        {
            Dictionary<string, object> version = new Dictionary<string, object>();
            version["ID"] = (int)item["ID"];
            if (item.ContainsKey("GUID"))  //Survey List item没有GUID
            {
                version["GUID"] = new Guid(item["GUID"].ToString());
                version["UserDataGuid"] = item["GUID"];
            }
            version["Size"] = 0;
            version["ObjType"] = item["ObjType"];
            version["TimeLastModified"] = item["TimeLastModified"];
            version["UIVersion"] = item["UIVersion"];
            version["IsCurrentVersion"] = item["_IsCurrentVersion"];
            version["Level"] = item["Level"];
            versions.Add(version);
            item["Versions"] = versions;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Obj is a part of keys")]
        private void AssembleWebItemVersionProperty(Dictionary<string, object> item, List<Dictionary<string, object>> versions)
        {
            Dictionary<string, object> version = new Dictionary<string, object>();
            version["ID"] = item.ContainsKey("ID") ? (int)item["ID"] : default(int);
            if (item.ContainsKey("GUID"))  //Survey List item没有GUID
            {
                version["GUID"] = new Guid(item["GUID"].ToString());
                version["UserDataGuid"] = item["GUID"];
            }
            else if (item.ContainsKey("UniqueId"))
            {
                version["GUID"] = new Guid(item["UniqueId"].ToString());
                version["UserDataGuid"] = item["UniqueId"];
            }
            version["Size"] = 0;
            version["ObjType"] = 2;
            version["TimeLastModified"] = item["TimeLastModified"];
            version["UIVersion"] = item["UIVersion"];
            version["IsCurrentVersion"] = true;
            version["Level"] = item["Level"];
            versions.Add(version);
            item["Versions"] = versions;
        }

        public Dictionary<string, object> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId)
        {
            return base.GetItemWebParts(siteId, webId, listId, itemDocId);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Used as a key")]
        public Dictionary<string, object> QueryListItemForIB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, Dictionary<string, object> changeItemsCache)
        {
            Dictionary<string, string> needLoadFields = new Dictionary<string, string>();
            //needLoadFields.Add("ID", "Counter");
            //needLoadFields.Add("GUID", "Guid");
            //needLoadFields.Add("_Level", "Integer");
            //needLoadFields.Add("_IsCurrentVersion", "Boolean");
            needLoadFields.Add("_UIVersion", "Integer");
            Dictionary<string, object> folder = base.QueryListItemForIB(siteId, webId, listId, folderId, folderUrl, changeItemsCache);
            List<Dictionary<string, object>> items = (List<Dictionary<string, object>>)folder["Items"];
            string webUrl = folder.ContainsKey("WebServerRelativeUrl") ?
                folder["WebServerRelativeUrl"].ToString() : base.GetWeb(webId)["ServerRelativeUrl"].ToString();
            foreach (Dictionary<string, object> item in items)
            {
                if ((item.ContainsKey("ChangeType") && (ChangeType)item["ChangeType"] == ChangeType.Delete) || !item.ContainsKey("Id"))
                {
                    item["Versions"] = new List<Dictionary<string, object>>();
                    continue;
                }
                if (item.ContainsKey("Versions") && WrapperConfiguration.BPOS_S.IncludeVersionForPerformance)
                {
                    List<Dictionary<string, object>> versions = (List<Dictionary<string, object>>)item["Versions"];
                    Dictionary<string, object> allVersionProperties = mWebServiceRequest.GetItemVersionsWithMultiRequest(webUrl, listId.ToString(), (int)item["Id"], "", needLoadFields);
                    List<Dictionary<string, object>> versionProperties = (List<Dictionary<string, object>>)allVersionProperties["ChildrenProperties"];
                    foreach (Dictionary<string, object> version in versionProperties)
                    {
                        version["ID"] = (int)item["ID"];
                        version["GUID"] = new Guid(item["GUID"].ToString());
                        version["Size"] = 0;
                        version["ObjType"] = item["ObjType"];
                        version["TimeLastModified"] = version["Modified"];
                        int versionId = (int)version["VersionId"];
                        version["Level"] = versionId == (int)item["UIVersion"] ? item["Level"] : (byte)1;
                        version["UIVersion"] = version["VersionId"];
                        version["UserDataGuid"] = version["GUID"];
                        version["IsCurrentVersion"] = versionId == (int)item["UIVersion"] ? true : false;
                        versions.Add(version);
                    }
                }
                else
                {// list enable version is false, we just add current version here
                    List<Dictionary<string, object>> versions = new List<Dictionary<string, object>>();
                    Dictionary<string, object> version = new Dictionary<string, object>();
                    version["ID"] = (int)item["ID"];
                    version["GUID"] = new Guid(item["GUID"].ToString());
                    version["Size"] = 0;
                    version["ObjType"] = item["ObjType"];
                    version["TimeLastModified"] = item["TimeLastModified"];
                    version["UIVersion"] = item["UIVersion"];
                    version["UserDataGuid"] = item["GUID"];
                    version["IsCurrentVersion"] = item["_IsCurrentVersion"];
                    version["Level"] = item["Level"];
                    versions.Add(version);
                    item["Versions"] = versions;
                }
            }
            return folder;
        }
        #endregion

        public AveRequestKind Kind
        {
            get
            {
                return AveRequestKind.ClientObjectModel | AveRequestKind.WebService;
            }
        }
        public void Dispose(bool KeepRequest)
        {
            //FileStream fs = new FileStream(@"C:\SecurityTrim.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //StreamWriter sw = new StreamWriter(fs);
            //sw.Write(mSiteTrimObj.ToString());
            //sw.Flush();
            //sw.Close();
            //fs.Close();
            this.mWebServiceRequest.Dispose(KeepRequest);
            base.Dispose(KeepRequest);
            //File.AppendAllText(@"C:\Trimmed Objects.txt", mSiteTrimObj.ToString());
        }

        public void CustomizeReport(Dictionary<string, object> parameters, Guid reportId)
        {
            mWebServiceRequest.CustomizeReport(parameters, reportId);
        }

        public override Dictionary<string, object> OperateSolution(string operation, string siteUrl, string webServerRelativeUrl, int id)
        {
            mWebServiceRequest.OperateSolution(operation, siteUrl, webServerRelativeUrl, id);
            return base.OperateSolution(operation, siteUrl, webServerRelativeUrl, id);
        }
        public override Dictionary<string, string> GetMetaInfo(string webServerRelativeUrl, string docServerRelativeUrl)
        {
            return mWebServiceRequest.GetMetaInfo(webServerRelativeUrl, docServerRelativeUrl);
        }
        public override string AddSite(string CAUrl, int compatibilityLevel, uint lcid, string owner, long storageQuota, string template, int timeZoneId, string title, string url, double resourceQuota)
        {
            return mWebServiceRequest.AddSite(CAUrl, compatibilityLevel, lcid, owner, storageQuota, template, timeZoneId, title, url, resourceQuota);
        }

        public override void DeclareOrUndeclareItem(int itemId, Guid listId, string webUrl)
        {
            mWebServiceRequest.DeclareOrUndeclareItem(itemId, listId, webUrl);
        }

        public override void UpdateWorkflowAssociationsOnChildren(string webUrl, string contentTypeId)
        {
            mWebServiceRequest.UpdateWorkflowAssociationsOnChildren(webUrl, contentTypeId);
        }

        public override Dictionary<string, object> UpdateFile(string webServerRelativeUrl, string listName, string fileServerRelativeUrl, Dictionary<string, object> prop)
        {
            if (prop.ContainsKey("ChangedMetaInfo"))
            {
                Dictionary<string, object> changedMetaInfo = prop["ChangedMetaInfo"] as Dictionary<string, object>;
                return mWebServiceRequest.UpdateFile(webServerRelativeUrl, listName, fileServerRelativeUrl, changedMetaInfo);
            }
            return null;
        }

        public override void PublishSharepointList(string webServerRelativeUrl, IAveFile templateFile, int lcid, string listId, string contentTypeId)
        {
            mWebServiceRequest.PublishSharepointList(webServerRelativeUrl, templateFile, lcid, listId, contentTypeId);
        }
    }
}
