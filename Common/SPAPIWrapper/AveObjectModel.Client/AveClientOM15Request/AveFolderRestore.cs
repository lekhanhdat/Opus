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
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Utilities;
using AvePoint.Wrapper.Common;
using AvePoint.ObjectModel.WebService;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.Client;
using AveClientRequest.Common;
using System.Collections;
using AvePoint.Wrapper.Common.Common.Utility;

namespace AvePoint.ObjectModel.ClientOM
{
    enum VersionSettingState
    {
        None,
        MajorOnly,
        MajorAndMinor,
        MajorWithApprove,
        MajorAndMinorWithApprove
    }
    class AveParentListSetting
    {
        public bool EnableModeration { get; set; }
        public bool EnableVersioning { get; set; }
        public Guid ListId { get; set; }
        public int ListTemplate { get; set; }
        public  int ListBaseType { get; set; }
        public string ListTitle { get; set; }
        public bool EnableMinorVersions { get; set; }

        public VersionSettingState VersionSettingState
        {
            get
            {
                if (EnableVersioning)
                {
                    if (EnableMinorVersions)
                    {
                        if (EnableModeration)
                        {
                            return VersionSettingState.MajorAndMinorWithApprove;
                        }
                        else
                        {
                            return VersionSettingState.MajorAndMinor;
                        }
                    }
                    else
                    {
                        if (EnableModeration)
                        {
                            return VersionSettingState.MajorWithApprove;
                        }
                        else
                        {
                            return VersionSettingState.MajorOnly;
                        }
                    }
                }
                else
                {
                    return VersionSettingState.None;
                }
            }
        }
    }
    class AveFolderRestore : IDisposable
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveFolderRestore));
        private Site mSite;
        private Web mParentWeb;
        private List mParentList;
        private AveParentListSetting ParentListSetting;
        private string mParentFolderUrl;
        private ResourcePath mParentFolderPath;
        private string mParentWebServerRelativeUrl;
        private Folder mParentFolder;
        private string mFolderRelativeUrl;
        private ResourcePath mFolderRelativePath;
        private string mName;
        private int mRowId;
        private Guid mGuid;
        private int mOriginalVersion;
        private int mOriginalLevel;
        private bool mIsNewCreated;
        private string mParentListName;
        private bool mParentListIsSystem;
        private string mListRootFolderUrl;
        private string mListDefaultViewUrl;
        private int mListTemplate;
        private Guid mListId;
        private int mListBaseType;
        private string mListTitle;
        private int mVersion;
        private int mModerationStatus;
        private object mObj;
        private AveListItemRestore mItemRestore;
        private ClientContext mContext;
        private ListItem mListItem;
        private AveClientOM2013Request mRequest;

        public AveFolderRestore(AveClientOM2013Request request, Site site, ClientContext context, object obj)
        {
            mRequest = request;
            mSite = site;
            mContext = context;
            mObj = obj;
        }

        protected void PrepareRestoreContext(Dictionary<string, object> data)
        {
            mParentWeb = mContext.Site.OpenWeb(data["WebUrl"] as string);
            Dictionary<string, object> parentWebProperties = data["ParentWebProperties"] as Dictionary<string, object>;
            mParentWebServerRelativeUrl = parentWebProperties["ServerRelativeUrl"] as string;            
            mParentFolderUrl = data.ContainsKey("FolderUrl") ? data["FolderUrl"] as string : string.Empty;
            mParentFolderPath = ResourcePath.FromDecodedUrl(mParentFolderUrl);
            //support special character such as "#,%"
            mParentFolder = mParentWeb.GetFolderByServerRelativePath(mParentFolderPath);
            mRowId = data.ContainsKey("DoclibRowId") ? Convert.ToInt32(data["DoclibRowId"]) : -1;
            mGuid = data.ContainsKey("GUID") ? new Guid(data["GUID"].ToString()) : Guid.Empty;
            mOriginalVersion = Convert.ToInt32(data["UIVersion"]);
            mOriginalLevel = data.ContainsKey("Level") ? Convert.ToByte(data["Level"]) : (byte)0;
            mModerationStatus = data.ContainsKey("_ModerationStatus") ? Convert.ToInt32(data["_ModerationStatus"]) : -1;
            mName = data.ContainsKey("Title") ? data["Title"] as string : string.Empty;
            mFolderRelativeUrl = data.ContainsKey("ServerRelativeUrl") ? data["ServerRelativeUrl"] as string : string.Empty;
            mFolderRelativePath = ResourcePath.FromDecodedUrl(mFolderRelativeUrl);
            if (data.ContainsKey("ParentListProperties"))
            {
                Dictionary<string, object> parentListProperties = data["ParentListProperties"] as Dictionary<string, object>;
                mListTemplate = parentListProperties.ContainsKey("ListTemplate") ? (int)parentListProperties["ListTemplate"] : -1;
                mListBaseType = parentListProperties.ContainsKey("BaseType") ? (int)parentListProperties["BaseType"] : -1;
                mListRootFolderUrl = parentListProperties.ContainsKey("ListRootFolderUrl") ? parentListProperties["ListRootFolderUrl"] as string : string.Empty;
                mListDefaultViewUrl = parentListProperties.ContainsKey("ListDefaultViewUrl") ? parentListProperties["ListDefaultViewUrl"] as string : string.Empty;
                mListTitle = parentListProperties.ContainsKey("ListTitle") ? parentListProperties["ListTitle"] as string : string.Empty;
                mListId = parentListProperties.ContainsKey("ListId") ? (Guid)parentListProperties["ListId"] : Guid.Empty;
                mParentList = data.ContainsKey("ListId") ? mParentWeb.Lists.GetById((Guid)data["ListId"]) : null;

                var enableModeration = parentListProperties.ContainsKey("ListEnableModeration") ? Convert.ToBoolean(parentListProperties["ListEnableModeration"]) : false;
                var enableVersioning = parentListProperties.ContainsKey("ListEnableVersioning") ? Convert.ToBoolean(parentListProperties["ListEnableVersioning"]) : false;
                var enableMinorVersions = parentListProperties.ContainsKey("ListEnableMinorVersions") ? Convert.ToBoolean(parentListProperties["ListEnableMinorVersions"]) : false;

                ParentListSetting = new AveParentListSetting
                {
                    ListTitle = mListTitle,
                    ListId = mListId,
                    ListBaseType = mListBaseType,
                    ListTemplate = mListTemplate,
                    EnableVersioning = enableVersioning,
                    EnableMinorVersions = enableMinorVersions,
                    EnableModeration = enableModeration
                };
            }
            mItemRestore = mRowId > 0 ? new AveListItemRestore(mRequest, mSite, mParentWeb, mParentList, mRowId, mModerationStatus, mContext) : null;
            if (mItemRestore != null)
            {
                mItemRestore.PrepareParentProperties(data);
            }
            //mContext.Load(mParentWeb);            
        }

        public Dictionary<string, object> RestoreFolder(Dictionary<string, object> docData, Dictionary<string, object> userData)
        {
            Dictionary<string, object> folderProperties = new Dictionary<string, object>();
            Dictionary<string, object> itemProperties = new Dictionary<string, object>();
            Folder spFolder = null;
            bool exist = false;
            PrepareRestoreContext(docData);
            spFolder = GetFolder(mParentFolder);
            if (spFolder != null && spFolder.ListItemAllFields.IsPropertyAvailable("HasUniqueRoleAssignments"))
            {
                itemProperties["HasUniqueRoleAssignments"] = spFolder.ListItemAllFields.HasUniqueRoleAssignments;
                exist = true;
            }
            if (spFolder == null)
            {
                spFolder = AddFolderWithRetry(userData);
                folderProperties["IsNewCreated"] = mIsNewCreated;
            }
            if (mListItem != null)
            {
                DeleteComplianceTagIfCreateInThisJob(mListItem, mIsNewCreated);
            }
            //web下的system folder不能通过该比较，暂时先注释这两个条件.否则无法取item，会抛出异常。
            //mSPFolder.Name.Equals("_PolicyCatalog", StringComparison.CurrentCultureIgnoreCase) || mSPFolder.Name.Equals("images",StringComparison.CurrentCultureIgnoreCase)
            //|| mSPFolder.Name.Equals("_PolicyInternalData",StringComparison.CurrentCultureIgnoreCase))

            if (mParentListName == "{System Folder}" && mParentListIsSystem && spFolder != null)
            {
                AveClientOM2013Request.AssembleFolderProperties(mParentWebServerRelativeUrl, spFolder, spFolder.ServerRelativeUrl, folderProperties);
                folderProperties["Exists"] = true;

                if (mListItem != null)
                {
                    AveClientOM2013Request.GetItemDic(itemProperties, mListItem);
                    folderProperties["Item" + AveObjectModelConstant.ObjectPropertySuffix] = itemProperties;
                    folderProperties["UniqueId"] = itemProperties["UniqueId"];
                }

                return folderProperties;
            }

            /*
             * 由于访问External List里面的root folder下第一个sub folder的Item对象时，会出现Access Denied的错误，但是第二次取没有问题。
             * 目前没有找到好的方法来避免这个错误，所以先试取下，然后再走以前的还原逻辑。
             */

            if (spFolder != null && mListItem != null)
            {
                Guid uniqueId = (Guid)mListItem["UniqueId"];
                if (uniqueId != (Guid)mListItem["UniqueId"])
                {
                    spFolder = mParentWeb.GetFolderByServerRelativePath(ResourcePath.FromDecodedUrl(spFolder.ServerRelativeUrl));
                }
                //mAveItem.InitBySPListItem((spFolder.Item);
                mVersion = (int)mListItem["_UIVersion"];
                RestoreFolderSpecialProperties(mListItem, docData,userData);
                bool isDocumentSet = userData.ContainsKey("ContentType") && AveSPUtility.IsChildOfContentType(AveSPUtility.ParseContentTypeId(userData["ContentType"] as string), AveBuiltInContentTypeId.DocumentSet);
                //由于还原DocumentSet version 需要替换其下的File的RowId由于此时不知道其下File还原结束后的RowId，所以将DocumentSet的Version的还原移到ListPostAction。
                //if (isDocumentSet && docData.ContainsKey("snapshots"))
                //{
                //    string snapShotsValue = docData["snapshots"].ToString();
                //    HandleDcVersion(mListItem.Id, snapShotsValue);
                //    docData.Remove("snapshots");
                //    mContext.Load(mListItem);
                //    mContext.ExecuteQuery();
                //}
                //还原discussion时，不需要还原DescendantLikesCount field value，value会自己增加
                if (mListTemplate == (int)ListTemplateType.DiscussionBoard && !IsSubDiscussion(mListRootFolderUrl, mFolderRelativeUrl))
                {
                    if (userData.ContainsKey("DescendantLikesCount"))
                    {
                        userData.Remove("DescendantLikesCount");
                    }
                    if (userData.ContainsKey("TopicPageUrl"))
                    {
                        userData.Remove("TopicPageUrl");
                    }
                }

                userData["FileLeafRef"] = this.mName;
                if (userData.ContainsKey("Title") && !mIsNewCreated && mListBaseType != (int)BaseType.DocumentLibrary)
                {
                    //keep title if new created. Otherwise not keep.
                    //BaseType is DocumentLibrary,"Title" should not remove
                    //otherwise the listtype is not DocumentLibrary folder exist in destination should not replace destination folder title.
                    userData.Remove("Title");
                }
                //SAAS-30821 folder rowId为0时，restore folder空引用    
                if (WrapperConfiguration.WrapperConfigurationForBPOS.SkipReplaceFoler && exist)
                {
                    mLogger.Info("folder exist and conflict type is skip or append,skip restore");
                }
                else
                {
                    if (mItemRestore != null)
                    {
                        if (mOriginalVersion == mVersion || (WrapperConfiguration.WrapperConfigurationForBPOS.OverWriteReplaceFoler && exist))
                        {
                            mItemRestore.UpdateListItem(mListItem, userData, ListItemUpdateMethodKind.SystemUpdate, false);
                        }
                        else if (mOriginalVersion > mVersion)
                        {
                            UpdateToSpecificVersion(mListItem, userData, isDocumentSet, mOriginalVersion, mIsNewCreated);
                            mItemRestore.UpdateListItem(mListItem, userData, ListItemUpdateMethodKind.SystemUpdate, false);
                        }
                    }
                }
            }

            if (spFolder != null)
            {
                AveClientOM2013Request.AssembleFolderProperties(mParentWebServerRelativeUrl, spFolder, spFolder.ServerRelativeUrl, folderProperties);
                folderProperties["Exists"] = true;

                if (mListItem != null)
                {
                    AveClientOM2013Request.GetItemDic(itemProperties, mListItem);
                    Dictionary<string, object> fieldValue = itemProperties["FieldValues"] as Dictionary<string, object>;
                    if (fieldValue != null && fieldValue.ContainsKey("_ModerationStatus") && (int)fieldValue["_ModerationStatus"] != mModerationStatus)
                    {   
                        //Update ModerationStatus之后的ListItem ModerationStatus属性并未重新获取，这里手动赋值
                        fieldValue["_ModerationStatus"] = mModerationStatus;
                        itemProperties["_ModerationStatus"] = mModerationStatus;
                    }
                    folderProperties["Item" + AveObjectModelConstant.ObjectPropertySuffix] = itemProperties;
                    folderProperties["UniqueId"] = itemProperties["UniqueId"];
                }
                SetComplianceTagIfCreateInThisJob(mListItem, docData, mIsNewCreated);
                return folderProperties;
            }

            return folderProperties;
        }

        private void DeleteComplianceTagIfCreateInThisJob(ListItem listItem, bool isNewCreated)
        {
            if (isNewCreated)
            {
                mRequest.GetListItemComplianceInfo(mContext, listItem);
                if (!string.IsNullOrWhiteSpace(listItem.ComplianceInfo?.ComplianceTag))
                {
                    try
                    {
                        mRequest.SetComplianceTagOnBulkItems(mContext, mListRootFolderUrl, new List<int> { listItem.Id }, "");
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error($"Fail delete retention label,error message:{ex.Message},web url:{mParentWebServerRelativeUrl},listUrl:{mListRootFolderUrl},rowId:{listItem.Id},error:{ex}");
                    }
                }
            }
        }

        protected bool IsRecordTypeComplianceTag(string complianceTagName)
        {
            try
            {
                var sitePropertyContext = SitePropertyCache.GetInstance();
                if (sitePropertyContext.AvaliableComplianceTags == null)
                {
                    sitePropertyContext.InitAvaliableComplianceTags(mContext.Url, mContext);
                }
                var complianceTag = sitePropertyContext.AvaliableComplianceTags.FirstOrDefault(info => info.TagName == complianceTagName);
                if (complianceTag != null)
                {
                    if (complianceTag.BlockDelete && complianceTag.BlockEdit)
                    {
                        return true;
                    }
                }
                else
                {
                    mLogger.Warn($"Unable get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{mContext.Url}");
                }
                return false;
            }
            catch (Exception ex)
            {
                mLogger.Error($"Fail get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{mContext.Url}, ex:{ex}");
                throw;
            }
        }

        private void SetComplianceTagIfCreateInThisJob(ListItem listItem, Dictionary<string, object> documentInfo, bool isNewCreated)
        {
            if (isNewCreated && documentInfo.ContainsKey("ComplianceTag") && !string.IsNullOrWhiteSpace(documentInfo?["ComplianceTag"]?.ToString()))
            {
                try
                {
                    mRequest.SetComplianceTagOnBulkItems(mContext, mListRootFolderUrl, new List<int> { listItem.Id }, documentInfo["ComplianceTag"].ToString());
                }
                catch (Exception ex)
                {
                    mLogger.Error($"Fail set retention label,label:{documentInfo["ComplianceTag"]},list url:{mListRootFolderUrl}, row id:{listItem.Id},error message:{ex.Message},error:{ex}");
                    throw;
                }
            }
        }

        private Folder RestoreFolderSpecialProperties(ListItem folder, Dictionary<string, object> docData, Dictionary<string, object> userData)
        {
            bool modifyDcLastRefresh = false;
            bool changed = false;
            Folder spFolder = new Folder(mContext, new ObjectPathMethod(mContext, mParentWeb.Path, "GetFolderByServerRelativePath", new object[] { mFolderRelativePath }));
            if (docData.ContainsKey("Properties"))
            {
                Hashtable properties = (Hashtable)docData["Properties"];
                //BrowserFormWebPartProperties
                if (properties.ContainsKey("_ipfs_solutionName"))
                {
                    spFolder.Properties["_ipfs_solutionName"] = properties["_ipfs_solutionName"];
                    changed = true;
                }
                if (properties.ContainsKey("_ipfs_infopathenabled"))
                {
                    spFolder.Properties["_ipfs_infopathenabled"] = properties["_ipfs_infopathenabled"];
                    changed = true;
                }
                if (properties.ContainsKey("docset_LastRefresh"))
                {
                    spFolder.Properties["docset_LastRefresh"] = properties["docset_LastRefresh"];
                    modifyDcLastRefresh = true;
                    changed = true;
                }
            }
            ///SAAS-5048
            if (!modifyDcLastRefresh && docData.ContainsKey("docset_LastRefresh"))
            {
                spFolder.Properties["docset_LastRefresh"] = docData["docset_LastRefresh"];
                changed = true;
            }
            if (docData.ContainsKey("vti_contenttypeorder") && docData["vti_contenttypeorder"] != null)
            {
                spFolder.Properties["vti_contenttypeorder"] = docData["vti_contenttypeorder"];
                changed = true;
            }
            if (changed)
            {
                spFolder.Update();
                mContext.Load(folder);
                mContext.ExecuteQuery();
            }
            return spFolder;
        }

        protected Folder AddFolderWithRetry(Dictionary<string, object> userData)
        {
            //SAAS-9709
            Folder folder = null;
            AveClientTaskRetryHelper retryHelper = new AveClientTaskRetryHelper(3, new KeyValuePair<string, string>("ServerException", "Cannot create folder"), new KeyValuePair<string, string>("ServerException", "Please try again"));
            retryHelper.ExecuteWithRetryMechanism(() => folder = AddFolder(userData));
            return folder;
        }

        protected Folder AddFolder(Dictionary<string, object> userData)
        {
            Folder folder = null;
            ListItemCollection listItems = null;
            Dictionary<string, object> itemProp = new Dictionary<string, object>();
            bool isDocumentSet = userData.ContainsKey("ContentType") && AveSPUtility.IsChildOfContentType(AveSPUtility.ParseContentTypeId(userData["ContentType"] as string), AveBuiltInContentTypeId.DocumentSet);
            if ((mRowId > 0 /*&& mListBaseType != (int)BaseType.DocumentLibrary*/) || isDocumentSet)
            {
                if ((int)mListTemplate == 2100)
                {
                    try
                    {
                        //AveSPEventReceiverConfig.DisableEventReceiver();
                        ListItemCreationInformationUsingPath creationInfoUsingPath = new ListItemCreationInformationUsingPath();
                        creationInfoUsingPath.FolderPath = mParentFolderPath;
                        creationInfoUsingPath.UnderlyingObjectType = FileSystemObjectType.Folder;
                        creationInfoUsingPath.LeafName = ResourcePath.FromDecodedUrl(mName);
                        mListItem = mParentList.AddItemUsingPath(creationInfoUsingPath);
                    }
                    finally
                    {
                        //AveSPEventReceiverConfig.EnableEventReceiver();
                    }
                }
                if (mListTemplate == (int)ListTemplateType.DiscussionBoard && !IsSubDiscussion(mListRootFolderUrl, mFolderRelativeUrl))
                {
                    mListItem = Utility.CreateNewDiscussion(mContext, mParentList, mName);
                }
                else
                {
                    ListItemCreationInformationUsingPath creationInfoUsingPath = new ListItemCreationInformationUsingPath();
                    creationInfoUsingPath.FolderPath = mParentFolderPath;
                    creationInfoUsingPath.UnderlyingObjectType = FileSystemObjectType.Folder;
                    creationInfoUsingPath.LeafName = ResourcePath.FromDecodedUrl(mName);
                    mListItem = mParentList.AddItemUsingPath(creationInfoUsingPath);
                    if (isDocumentSet)
                    {
                        mListItem["ContentTypeId"] = userData["ContentType"];
                    }
                }
                if (userData.ContainsKey("ThreadIndex"))
                {
                    mListItem["ThreadIndex"] = userData["ThreadIndex"];
                }
                mListItem.Update();
                mContext.Load(mListItem);
                folder = mParentWeb.GetFolderByServerRelativePath(mFolderRelativePath);
                mContext.Load(folder);
                mContext.Load(folder, f => f.ParentFolder);
                mContext.ExecuteQuery();
            }
            else
            {
                //if (mListTemplate == 2100)
                //{
                //    string webApp = AveUrlUtility.GetServerUrl(mContext.Url);
                //    string listTitle = this.mListRootFolderUrl.Substring(mParentWebServerRelativeUrl.Length + 1);
                //    AveWebServiceRequest.AddSlideFolder(webApp, mParentWebServerRelativeUrl, listTitle, mParentFolderUrl, mName, mObj);
                //    folder = mParentWeb.GetFolderByServerRelativeUrl(mFolderRelativeUrl);
                //}
                //else
                //{
                //使用addUsingPath方法添加，否则创建出的带“%”的名字会变成“%25”
                FolderCollectionAddParameters foldersAddParam = new FolderCollectionAddParameters();
                foldersAddParam.Overwrite = true;
                ResourcePath folderNamePath = ResourcePath.FromDecodedUrl(mName);
                folder = mParentFolder.Folders.AddUsingPath(folderNamePath, foldersAddParam);
                //}

                if (mParentList != null && mRowId > 0)
                {
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ViewXml = string.Format(
                        "<View Scope=\"DefaultValue\"><Query><Where><Eq><FieldRef Name=\"FileRef\"/><Value Type=\"Lookup\">{0}</Value></Eq></Where></Query><RowLimit>1</RowLimit></View>",
                        mParentFolderUrl.TrimEnd('/') + "/" + mName);
                    camlQuery.FolderServerRelativePath = mParentFolderPath;
                    listItems = mParentList.GetItems(camlQuery);
                    mContext.Load(listItems);
                }
                mContext.Load(mParentWeb);
                ExceptionHandlingScope excepScope = new ExceptionHandlingScope(mContext);
                using (excepScope.StartScope())
                {
                    using (excepScope.StartTry())
                    {
                        mContext.Load(folder);
                        mContext.Load(folder, f => f.ParentFolder);
                    }
                    using (excepScope.StartCatch())
                    {
                        mContext.Load(folder, f => f.Name);
                        mContext.Load(folder, f => f.ParentFolder);
                        mContext.Load(folder, f => f.ServerRelativeUrl);
                        mContext.Load(folder, f => f.ContentTypeOrder);
                        mContext.Load(folder, f => f.Files);
                        mContext.Load(folder, f => f.Folders);
                    }
                }
                mContext.ExecuteQuery();
                if (listItems != null && listItems.Count == 1)
                {
                    mListItem = listItems[0];
                }
            }
            mIsNewCreated = true;

            return folder;
        }

        private bool IsSubDiscussion(string mListRelativeUrl, String mFolderRelativeUrl)
        {
            if (mFolderRelativeUrl.Length != mListRelativeUrl.Length
                && mFolderRelativeUrl.Substring(mListRelativeUrl.Length + 1).IndexOf('/') != -1)
            {
                return true;
            }
            return false;
        }

        protected void UpdateToSpecificVersion(ListItem spListItem, Dictionary<string, object> userData, bool isDocumentSet, int originalVersion, bool deleteBaseVersion)
        {
            try
            {
                if (spListItem == null)
                {
                    return;
                }
                List<int> versionLabels = new List<int>();

                object ct;
                if (!userData.TryGetValue("ContentType", out ct))
                {
                    mLogger.Warn("ContentType not exist in folder user data.");
                }
                string ctId = Convert.ToString(ct);
                string fileLeafRef = mName;
                if (ParentListSetting.VersionSettingState == VersionSettingState.None)
                {
                    mLogger.Warn("Version Setting is not enabled in current list.Will not increase version number.");
                }
                else
                {
                    mLogger.Info("Begin to update folder version.VersionSettingState:{0},SourceVersion:{1},Current:{2}", ParentListSetting.VersionSettingState, originalVersion, mVersion);
                }

                ListItem refreshedListItem = new ListItem(mParentList.Context, new ObjectPathMethod(mParentList.Context, mParentList.Path, "GetItemById", new object[] { spListItem.Id }));
                int currentMajorVersion = mVersion / 512;
                int currentMinorVersion = mVersion % 512;
                int originalMajorVersion = originalVersion / 512;
                int originalMinorVersion = originalVersion % 512;

                int firstMajor = currentMajorVersion;
                int firstMinor = currentMinorVersion;
                if (deleteBaseVersion)
                {
                    versionLabels.Add(mVersion);
                }

                switch (ParentListSetting.VersionSettingState)
                {
                    case VersionSettingState.MajorOnly:
                    case VersionSettingState.MajorWithApprove:
                        while (originalMajorVersion > currentMajorVersion)
                        {
                            ValidateUpdateVersion(refreshedListItem, ctId, fileLeafRef, currentMajorVersion, currentMinorVersion, isDocumentSet);
                            currentMajorVersion++;
                            currentMinorVersion = 0;
                            versionLabels.Add(currentMajorVersion * 512);
                        }
                        if (originalMinorVersion != currentMinorVersion)
                        {
                            mLogger.Warn("Minor version number will not be kept as minor version setting is not enabled at destination.");
                        }
                        break;
                    case VersionSettingState.MajorAndMinor:
                        if (originalMajorVersion > currentMajorVersion)
                        {
                            mParentList.EnableMinorVersions = false;
                            mParentList.Update();
                            mContext.ExecuteQuery();
                            while (originalMajorVersion > currentMajorVersion)
                            {
                                ValidateUpdateVersion(refreshedListItem, ctId, fileLeafRef, currentMajorVersion, currentMinorVersion, isDocumentSet);
                                currentMajorVersion++;
                                currentMinorVersion = 0;
                                versionLabels.Add(currentMajorVersion * 512);
                            }
                            mParentList.EnableMinorVersions = true;
                            mParentList.Update();
                            mContext.ExecuteQuery();
                        }
                        while (originalMinorVersion > currentMinorVersion)
                        {
                            ValidateUpdateVersion(refreshedListItem, ctId, fileLeafRef, currentMajorVersion, currentMinorVersion, isDocumentSet);
                            currentMinorVersion++;
                            versionLabels.Add(currentMajorVersion * 512 + currentMinorVersion);
                        }
                        break;
                    case VersionSettingState.MajorAndMinorWithApprove:
                        while (originalMajorVersion > currentMajorVersion)
                        {
                            refreshedListItem.Update();
                            refreshedListItem["_ModerationStatus"] = 0;
                            refreshedListItem.Update();
                            currentMajorVersion++;
                            currentMinorVersion = 0;
                            versionLabels.Add(currentMajorVersion * 512);
                        }
                        while (originalMinorVersion > currentMinorVersion)
                        {
                            refreshedListItem.Update();

                            currentMinorVersion++;
                            versionLabels.Add(currentMajorVersion * 512 + currentMinorVersion);
                        }
                        break;
                }

                if (versionLabels.Count > 0)
                {
                    int lastMajorIndex = -1;
                    for (int k = versionLabels.Count - 1; k >= 0; k--)
                    {
                        if (versionLabels[k] % 512 == 0)
                        {
                            lastMajorIndex = k;
                        }

                    }
                    if (versionLabels.Count - 1 != lastMajorIndex && lastMajorIndex != -1)
                    {
                        //last major and draft version should be kept
                        versionLabels.RemoveAt(lastMajorIndex);
                        versionLabels.RemoveAt(versionLabels.Count - 1);
                    }
                    else
                    {
                        //last is current version
                        versionLabels.RemoveAt(versionLabels.Count - 1);
                    }
                }
                if (versionLabels.Count > 0)
                {
                    mContext.ExecuteQuery();
                    mRequest.DeleteHistoryVersions(mParentWebServerRelativeUrl, mListId, spListItem.Id, versionLabels);
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Update folder:{0} to specific version failed.Error Message:{1}", mFolderRelativeUrl, ex.ToString());
            }
        }

        private void ValidateUpdateVersion(ListItem item, string ct, string folderName, int majorVersionNumber,int minorVersionNumber,bool isDocumentSet=false)
        {
            //if ((majorVersionNumber == 1&&minorVersionNumber==0)&&(!isDocumentSet))
            //{
            //    //1.0->2.0 need update twice
            //    ValiteUpdateFolderVersion(item, ct, folderName);
            //}
            ValiteUpdateFolderVersion(item, ct, folderName);
        }

        private static void ValiteUpdateFolderVersion(ListItem item, string ct, string folderName)
        {
            item.ValidateUpdateListItem(new List<ListItemFormUpdateValue>
                            {
                                new ListItemFormUpdateValue
                                {
                                    FieldName="ContentTypeId",
                                    FieldValue=ct
                                },
                                new ListItemFormUpdateValue
                                {
                                   FieldName="FileLeafRef",
                                   FieldValue=folderName
                                },}, false, null, true, true, string.Empty);
        }

        internal Folder GetFolder(Folder parentFolder)
        {
            Folder folder = null;
            ListItemCollection listItems = null;
            ExceptionHandlingScope excepScope = null;
            try
            {
                if (mName.Equals("{System Folder}"))
                {
                    folder = parentFolder;
                    listItems = this.GetItem(folder.ServerRelativeUrl);
                }
                else
                {
                    if (mParentList != null)
                    {
                        excepScope = new ExceptionHandlingScope(mContext);
                        using (excepScope.StartScope())
                        {
                            using (excepScope.StartTry())
                            {
                                //support special characters such as "#,%"
                                folder = mParentWeb.GetFolderByServerRelativePath(mFolderRelativePath);
                                mContext.Load(folder);
                                mContext.Load(folder, f => f.ParentFolder, f => f.ListItemAllFields, f => f.ListItemAllFields.HasUniqueRoleAssignments);
                            }
                            using (excepScope.StartCatch())
                            {
                                mContext.Load(mParentWeb);
                                //mContext.Load(mParentList, l => l.RootFolder.ServerRelativeUrl, l => l.DefaultViewUrl, l => l.BaseType, l => l.BaseTemplate);
                            }
                        }
                        mContext.ExecuteQuery();
                        if (excepScope != null && !excepScope.HasException)
                        {
                            if (folder.IsObjectPropertyInstantiated("ListItemAllFields") && folder.ListItemAllFields.IsPropertyAvailable("Id"))
                            {
                                mListItem = mParentList.GetItemById(Convert.ToInt32(folder.ListItemAllFields.Id));
                                mContext.Load(mListItem);
                            }
                            else
                            {
                                listItems = this.GetItem(mFolderRelativeUrl);
                            }
                        }
                    }
                    else
                    {
                        folder = mParentWeb.GetFolderByServerRelativePath(mFolderRelativePath);
                        mContext.Load(folder);
                        mContext.Load(folder, f => f.ParentFolder);
                    }
                }
                if (mContext.HasPendingRequest)
                {
                    mContext.ExecuteQuery();
                }
                if (excepScope != null && excepScope.HasException)
                {
                    folder = null;
                }
                else if (listItems != null && listItems.Count == 1)
                {
                    mListItem = listItems[0];
                }
            }
            //  catch(ArgumentException)
            catch (Exception ex)
            {
                mLogger.Warn("Get folder:{0} failed.Error Message:{1}",mFolderRelativeUrl,ex.ToString());
                folder = null;
                throw;
            }

            return folder;
        }

        private ListItemCollection GetItem(string folderRelativeUrl)
        {
            ListItemCollection listItems = null;
            CamlQuery camlQuery = new CamlQuery();
            camlQuery.ViewXml = string.Format(
                "<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"FileRef\"/><Value Type=\"Lookup\">{0}</Value></Eq></Where></Query><RowLimit>1</RowLimit></View>",
                folderRelativeUrl);
            if (mParentList != null)
            {
                listItems = mParentList.GetItems(camlQuery);
                mContext.Load(listItems);
            }
            return listItems;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (mContext.HasPendingRequest)
            {
                AveAssemblyUtility.SetFieldValue(mContext, typeof(ClientRuntimeContext), "m_request", null);
            }
        }

        #endregion
    }
}
