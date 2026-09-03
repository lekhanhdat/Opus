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
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Utilities;
using AvePoint.Wrapper.Common;
using AvePoint.ObjectModel.WebService;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.Client;
using AveClientRequest.Common;
using System.Diagnostics.CodeAnalysis;
using System.Collections;

namespace AvePoint.ObjectModel.ClientOM
{
    public class Ave2013FolderRestore : IDisposable
    {
        protected static AveLogger mLogger = AveLogger.GetInstance(typeof(AveFolderRestore));
        protected Site mSite;
        protected Web mParentWeb;
        protected List mParentList;
        private string mParentFolderUrl;
        private Folder mParentFolder;
        private string mFolderRelativeUrl;
        private string mName;
        protected int mRowId;
        private Guid mGuid;
        private int mOriginalVersion;
        private int mOriginalLevel;
        private int mOldRowId;
        private Guid mOldGuidId;
        private bool mIsNewCreated;
        private string mParentListName;
        private bool mParentListIsSystem;
        private int mVersion;
        private bool mOverWrite;
        protected int mModerationStatus;
        protected object mObj;
        protected AveListItemRestore mItemRestore;
        protected ClientContext mContext;
        protected ListItem mListItem;
        protected AveClientOM2013Request mRequest;
        protected IAveWeb mAveWebCache;

        public Ave2013FolderRestore(AveClientOM2013Request request, Site site, ClientContext context, object obj)
        {
            mRequest = request;
            mSite = site;
            mContext = context;
            mObj = obj;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Doclib is a part of Keys")]
        protected virtual void PrepareRestoreContext(Dictionary<string, object> data)
        {
            mParentWeb = mContext.Site.OpenWeb(data["WebUrl"] as string);
            mAveWebCache = data.ContainsKey("AveWebObject") ? (IAveWeb)data["AveWebObject"] : null;
            mParentFolderUrl = data.ContainsKey("FolderUrl") ? data["FolderUrl"] as string : string.Empty;
            mParentFolder = GetFolderByAPI(mParentWeb, mParentFolderUrl);
            mRowId = data.ContainsKey("DoclibRowId") ? Convert.ToInt32(data["DoclibRowId"]) : -1;
            mGuid = data.ContainsKey("GUID") ? new Guid(data["GUID"].ToString()) : Guid.Empty;
            mOriginalVersion = Convert.ToInt32(data["UIVersion"]);
            mOriginalLevel = data.ContainsKey("Level") ? Convert.ToByte(data["Level"]) : (byte)0;
            mModerationStatus = data.ContainsKey("_ModerationStatus") ? Convert.ToInt32(data["_ModerationStatus"]) : -1;
            mName = data.ContainsKey("Title") ? data["Title"] as string : string.Empty;
            mFolderRelativeUrl = data.ContainsKey("ServerRelativeUrl") ? data["ServerRelativeUrl"] as string : string.Empty;
            mOverWrite = data.ContainsKey("DeleteItem") ? Convert.ToBoolean(data["DeleteItem"]) : false;
            mIsNewCreated = data.ContainsKey("IsNewCreated") ? Convert.ToBoolean(data["IsNewCreated"]) : false;
            //处理folder的parent list信息时需要考虑web下的folder的docdata
            if (TryGetListId(data) != Guid.Empty)
            {
                mParentList = mParentWeb.Lists.GetById(new Guid(data["ListId"] as string));
            }
            mItemRestore = mParentList != null ? new AveListItemRestore(mRequest, mSite, mParentWeb, mParentList, mRowId, mModerationStatus, mContext, mObj) : null;
            //mContext.Load(mParentWeb);
            if (mParentList != null)
            {
                mContext.Load(mParentList);
                mContext.Load(mParentList, list => list.DefaultViewUrl);
                mContext.Load(mParentList.RootFolder, folder => folder.ServerRelativeUrl);
            }
        }

        /// <summary>
        /// used for PrepareRestoreContext
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private Guid TryGetListId(Dictionary<string, object> data)
        {
            Guid resultId = Guid.Empty;
            object idObj = null;
            if (data != null && data.TryGetValue("ListId", out idObj) && idObj != null)
            {
                try
                {
                    resultId = new Guid(idObj as string);
                }
                catch (Exception e)
                {
                    mLogger.Debug("An error occurred while trying to get list id.Error:{0}", e);
                }
            }
            return resultId;
        }

        public Dictionary<string, object> RestoreFolder(Dictionary<string, object> docData, Dictionary<string, object> userData)
        {
            Dictionary<string, object> folderProperties = new Dictionary<string, object>();
            Dictionary<string, object> itemProperties = new Dictionary<string, object>();
            Folder spFolder = null;

            PrepareRestoreContext(docData);
            spFolder = GetFolder(mParentFolder);

            if (spFolder == null)
            {
                spFolder = AddFolder(folderProperties);
                if (spFolder != null)
                {
                    folderProperties["IsNewCreated"] = true;
                }
            }
            else
            {
                folderProperties["IsNewCreated"] = false;
            }
            bool isSystemFolder = (mParentListName == "{System Folder}" && mParentListIsSystem && spFolder != null);
            if (!isSystemFolder)//web下的system folder不能通过该比较，暂时先注释这两个条件.否则无法取item，会抛出异常。
            {
                if (mOverWrite || mIsNewCreated)
                {
                    using (AveListModerationSettingRecorder recorder = new AveListModerationSettingRecorder(mContext, mParentList))
                    {
                        /*
                         * 由于访问External List里面的root folder下第一个sub folder的Item对象时，会出现Access Denied的错误，但是第二次取没有问题。
                         * 目前没有找到好的方法来避免这个错误，所以先试取下，然后再走以前的还原逻辑。
                         */
                        if (spFolder != null)
                        {
                            //Diable version settings for Documentset
                            bool isDocumentSet = userData.ContainsKey("ContentType") && userData["ContentType"] != null && userData["ContentType"].ToString().StartsWith("0x0120D520", StringComparison.OrdinalIgnoreCase);
                            if (isDocumentSet && mListItem != null)
                            {
                                mVersion = (int)mListItem["_UIVersion"];
                                if (mVersion % 512 != 0 && mModerationStatus == 0 && mParentList != null && mParentList.EnableModeration)//当还原小version并且是approval的document set时，需要关闭moderation.
                                {
                                    mParentList.EnableModeration = false;
                                    mParentList.Update();
                                }
                            }

                            RestoreFolderSpecialProperties(spFolder, docData, isDocumentSet);
                            if (mListItem != null)
                            {
                                Guid uniqueId = (Guid)mListItem["UniqueId"];
                                if (uniqueId != (Guid)mListItem["UniqueId"])
                                {
                                    spFolder = GetFolderByAPI(mParentWeb, spFolder.ServerRelativeUrl);
                                }
                                //mAveItem.InitBySPListItem((spFolder.Item);
                                mVersion = (int)mListItem["_UIVersion"];

                                if (mOriginalVersion == mVersion)
                                {
                                    mItemRestore.UpdateListItemForFolder(ref mListItem, userData, ListItemUpdateMethodKind.SystemUpdate, false);
                                }
                                else if (mOriginalVersion > mVersion)
                                {
                                    IFolderVersionIncreaser increaser;
                                    if (mParentList == null || (int)mParentList.BaseType != 1)
                                    {
                                        increaser = new AveListFolderVersionIncreaser(mContext, mRequest, mObj, mAveWebCache, mParentList);
                                    }
                                    else
                                    {
                                        increaser = new AveDocLibFolderVersionIncreaser(mContext, mRequest, mObj, mAveWebCache, mParentList, mModerationStatus);
                                    }
                                    increaser.UpdateToSpecificVersion(mListItem, mOriginalVersion, mIsNewCreated);
                                    if (mParentList != null && isDocumentSet && (int)mListItem["_UIVersion"] % 512 != 0)
                                    {
                                        /*
                                         * 1.Pending 状态小version Document set，EnableModeration==true时， update moderation会导致version变成大version， status 变成Approve
                                         * 2.Approved 状态 小version并且EnableModeration== true时，update item会导致涨version，状态变为pending
                                         */
                                        if (!mParentList.EnableModeration)
                                        {
                                            mParentList.EnableModeration = true;
                                            mParentList.Update();
                                        }
                                        if (mListItem.FieldValues.ContainsKey("_ModerationStatus") && (int)mListItem["_ModerationStatus"] != 2 && mParentList.EnableModeration)
                                        {
                                            mParentList.EnableModeration = false;
                                            mParentList.Update();
                                        }
                                        //list version setting 没开小version，会还成大version
                                        if (!mParentList.EnableMinorVersions)
                                        {
                                            mParentList.EnableMinorVersions = true;
                                            mParentList.Update();
                                            folderProperties["ListVersionSettingChanged"] = true;
                                        }
                                    }
                                    mItemRestore.UpdateListItemForFolder(ref mListItem, userData, ListItemUpdateMethodKind.SystemUpdate, false);
                                }
                                //Restore the ModerationStatus of Document Library's Folder.
                                UpdateModifiedAndModeration(userData);
                            }
                        }
                    }
                }
            }

            if (spFolder != null)
            {
                mRequest.AssembleFolderProperties(mContext as AveClientContext, mAveWebCache.ServerRelativeUrl, spFolder, spFolder.ServerRelativeUrl, folderProperties);
                folderProperties["Exists"] = true;

                if (mListItem != null)
                {
                    mRequest.GetItemDic(itemProperties, mListItem);
                    if (!isSystemFolder)
                    {
                        Dictionary<string, object> fieldValue = itemProperties["FieldValues"] as Dictionary<string, object>;
                        if (fieldValue != null && fieldValue.ContainsKey("_ModerationStatus") && (int)fieldValue["_ModerationStatus"] != mModerationStatus)
                        {
                            //Update ModerationStatus之后的ListItem ModerationStatus属性并未重新获取，这里手动赋值
                            fieldValue["_ModerationStatus"] = mModerationStatus;
                            itemProperties["_ModerationStatus"] = mModerationStatus;
                        }
                    }
                    folderProperties["Item" + AveObjectModelConstant.ObjectPropertySuffix] = itemProperties;
                    if (itemProperties.ContainsKey("UniqueId"))
                    {
                        folderProperties["UniqueId"] = itemProperties["UniqueId"];
                    }
                }
            }
            return folderProperties;
        }
        protected virtual Folder RestoreDocumentSetSPecialProperties(Folder spFolder, Dictionary<string, object> docData)
        {
            return RestoreFolderSpecialProperties(spFolder, docData);
        }

        protected bool RestoreFolderSprcialProperties(PropertyValues folderProperties, Dictionary<string, object> docData)
        {
            bool modifyDcLastRefresh = false;
            bool changed = false;
            if (docData.ContainsKey("Properties"))
            {
                Hashtable properties = (Hashtable)docData["Properties"];
                //BrowserFormWebPartProperties
                if (properties.ContainsKey("_ipfs_solutionName"))
                {
                    folderProperties["_ipfs_solutionName"] = properties["_ipfs_solutionName"];
                    changed = true;
                }
                if (properties.ContainsKey("_ipfs_infopathenabled"))
                {
                    folderProperties["_ipfs_infopathenabled"] = properties["_ipfs_infopathenabled"];
                    changed = true;
                }
                if (properties.ContainsKey("docset_LastRefresh"))
                {
                    folderProperties["docset_LastRefresh"] = properties["docset_LastRefresh"];
                    modifyDcLastRefresh = true;
                    changed = true;
                }
            }
            ///SAAS-5048
            if (!modifyDcLastRefresh && docData.ContainsKey("docset_LastRefresh"))
            {
                folderProperties["docset_LastRefresh"] = docData["docset_LastRefresh"];
                changed = true;
            }
            if (docData.ContainsKey("vti_contenttypeorder"))
            {
                folderProperties["vti_contenttypeorder"] = docData["vti_contenttypeorder"];
                changed = true;
            }

            // properties from local backup data
            if (docData.ContainsKey("MetaInfo"))
            {
                string metaInfoString = AveCompressedUtility.GetTCompressedString((byte[])docData["MetaInfo"]);
                Dictionary<string, string> MetaInfoDic = AveCompressedUtility.GetMetaInfoDictionary(metaInfoString);
                List<string> needRestoredProperties = new List<string>() { "vti_winfileattribs", "_ipfs_infopathenabled", "_ipfs_solutionName" };
                foreach (string key in needRestoredProperties)
                {
                    if (MetaInfoDic.ContainsKey(key))
                    {
                        folderProperties[key] = MetaInfoDic[key];
                        changed = true;
                    }
                }
            }
            return changed;
        }
        private Folder RestoreFolderSpecialProperties(Folder spFolder, Dictionary<string, object> docData, bool isDocumentSet)
        {
            if (isDocumentSet)
            {
                return RestoreDocumentSetSPecialProperties(spFolder, docData);
            }
            return RestoreFolderSpecialProperties(spFolder, docData);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "property key")]
        private Folder RestoreFolderSpecialProperties(Folder spFolder, Dictionary<string, object> docData)
        {
            if (RestoreFolderSprcialProperties(spFolder.Properties, docData))
            {
                spFolder.Update();
                mContext.Load(spFolder);
                if (mListItem != null)
                {
                    mContext.Load(mListItem);
                }
                mContext.ExecuteQuery();
            }
            return spFolder;
        }

        protected AveListVersionSettings BackupListVersionSettings(List mParentList)
        {
            if (mParentList == null)
            {
                return new AveListVersionSettings();
            }
            AveListVersionSettings listVersionSettings = new AveListVersionSettings();
            listVersionSettings.EnableVersioning = mParentList.EnableVersioning;
            listVersionSettings.MajorVersionLimit = mParentList.MajorVersionLimit;
            listVersionSettings.EnableMinorVersioning = mParentList.EnableMinorVersions;
            listVersionSettings.MinorVersionLimit = mParentList.MajorWithMinorVersionsLimit;
            return listVersionSettings;
        }

        protected void RevertListVersionSettings(List mParentList, AveListVersionSettings listVersionSettings)
        {
            if (mParentList == null)
            {
                return;
            }
            mParentList.EnableVersioning = listVersionSettings.EnableVersioning;
            mParentList.MajorVersionLimit = listVersionSettings.MajorVersionLimit;

            mParentList.EnableMinorVersions = listVersionSettings.EnableMinorVersioning;
            //mParentList.MajorWithMinorVersionsLimit = listVersionSettings.MinorVersionLimit;
        }

        private void UpdateModifiedAndModeration(Dictionary<string, object> userData)
        {
            if (!WrapperConfiguration.BPOS_S.KeepModeration ||
                mParentList == null ||
                mParentList.BaseType != BaseType.DocumentLibrary ||
                mListItem.FieldValues.Count <= 0)
            {
                return;
            }
            DateTime originalModified = DateTime.MinValue;
            string moderationComments = string.Empty;
            //ADO-199673 Folder 的moderation 只有Pending Approve Reject，不需要Reset
            //ResetModerationStatus(mParentList);
            if (NeedWebServiceUpdate(userData, ref originalModified, ref moderationComments))
            {
                if (!mParentList.EnableModeration)
                {
                    mParentList.EnableModeration = true;
                    mParentList.Update();
                    mContext.ExecuteQuery();
                }
                string webAppName = AveUrlUtility.GetServerUrl(mContext.Url);
                Dictionary<string, object> needKeepData = new Dictionary<string, object>();
                needKeepData["ModerationStatus"] = mModerationStatus;
                needKeepData["Modified"] = originalModified;
                needKeepData["ModerationComments"] = moderationComments;
                UpdateListItemsByWebService(webAppName, needKeepData);
            }
        }

        protected virtual void UpdateListItemsByWebService(string webAppName, Dictionary<string, object> needKeepData)
        {
            AveWebServiceRequest.UpdateListItems(webAppName, mAveWebCache.ServerRelativeUrl, mParentList.Title, mListItem.Id, mListItem.FieldValues["FileRef"].ToString(), mObj, needKeepData);
        }

        private void ResetModerationStatus(List parentList)
        {
            if (parentList.EnableModeration && parentList.EnableMinorVersions && mModerationStatus == 2)
            {
                mModerationStatus = 3;
            }
        }

        private bool NeedWebServiceUpdate(Dictionary<string, object> userData, ref DateTime originalModified, ref string moderationComments) //还原Document时，checkout，checkin增加version会造成ModerationStatus，Modified，
        {
            originalModified = userData.ContainsKey("Modified") ? (DateTime)userData["Modified"] : DateTime.Now;
            moderationComments = userData.ContainsKey("_ModerationComments") ? userData["_ModerationComments"].ToString() : string.Empty;

            return (mListItem.FieldValues.ContainsKey("_ModerationStatus") && !mListItem.FieldValues["_ModerationStatus"].Equals(mModerationStatus)) ||//if ModerationStatus equal.
                   (mListItem.FieldValues.ContainsKey("_ModerationComments") && mListItem.FieldValues["_ModerationComments"] != null && !mListItem.FieldValues["_ModerationComments"].Equals(moderationComments));
        }

        private ListItem CreateDiscussionBoardTopic()
        {
            if (mParentFolderUrl.Equals(mParentList.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                return Utility.CreateNewDiscussion(mContext, mParentList, mName);
            }
            //ADO-59414
            else
            {
                return CreateFolderByAPI();
            }
        }

        private ListItem CreateFolderByAPI()
        {
            ListItemCreationInformation creationInformation = new ListItemCreationInformation();
            creationInformation.FolderUrl = mParentFolderUrl;
            creationInformation.UnderlyingObjectType = FileSystemObjectType.Folder;
            creationInformation.LeafName = mName;
            return AddItemByAPI(mParentList, creationInformation);
        }

        protected virtual ListItem AddItemByAPI(List list, ListItemCreationInformation creationInformation)
        {
            return mParentList.AddItem(creationInformation);
        }

        protected Folder AddFolder(Dictionary<string, object> folderProperties)
        {
            Folder folder = null;
            ListItemCollection listItems = null;
            Dictionary<string, object> itemProp = new Dictionary<string, object>();
            try
            {
                if (mParentList != null)
                {
                    //如果folder version 小于512 name一定是开启了moderation 创建出来的小version
                    if (mOriginalVersion > 0 && mOriginalVersion < 512 && (!mParentList.EnableMinorVersions || !mParentList.EnableModeration))
                    {
                        mParentList.EnableMinorVersions = true;
                        mParentList.EnableModeration = true;
                        mParentList.Update();
                    }
                }
                if (mRowId > 0 && mParentList != null && (mParentList.BaseType != BaseType.DocumentLibrary || mParentList.BaseTemplate == 2100))
                {
                    if (mParentList.BaseTemplate == (int)ListTemplateType.DiscussionBoard)
                    {
                        mListItem = CreateDiscussionBoardTopic();
                    }
                    else
                    {
                        mListItem = CreateFolderByAPI();
                    }
                    mListItem.Update();
                    mContext.Load(mListItem);
                    folder = GetFolderByAPI(mParentWeb, mFolderRelativeUrl);
                    mContext.Load(folder);
                    mContext.Load(folder, f => f.ParentFolder);
                    mContext.ExecuteQuery();
                }
                else
                {
                    folder = AddFolderByAPI(mParentFolder.Folders, mName);
                    if (mParentList != null && mRowId > 0)
                    {
                        var fileUrl = mParentFolderUrl.TrimEnd('/') + "/" + mName;
                        fileUrl = HttpUtility.HtmlEncode(fileUrl);
                        CamlQuery camlQuery = new CamlQuery();
                        camlQuery.ViewXml = string.Format(
                            "<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"FileRef\"/><Value Type=\"Lookup\">{0}</Value></Eq></Where></Query><RowLimit>1</RowLimit></View>",
                            fileUrl);
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
                folderProperties["IsNewCreated"] = mIsNewCreated;
            }
            catch (Exception e)
            {
                string parentFolderName = "Parent folder Name has not been initialized";
                try
                {
                    if (mParentFolder.IsPropertyAvailable("Name")) parentFolderName = mParentFolder.Name;
                }
                catch (Exception ex)
                {
                    mLogger.Debug("An error occurred while trying to get parent folder name.Error:{0}", ex);
                }
                mLogger.Debug(AveClientOMRequestResource.AddFolderError, mName, parentFolderName, e);
                throw;
            }

            return folder;
        }

        protected void UpdateToSpecificVersion(ListItem spListItem, int originalVersion, bool deleteBaseVersion)
        {
            if (spListItem == null)
            {
                return;
            }
            //UnLockItem(spListItem);            
            List<int> versionLabels = new List<int>();

            if (deleteBaseVersion)
            {
                versionLabels.Add(mVersion);
            }
            int preVersion = -1;
            while (originalVersion > mVersion)
            {
                if (originalVersion % 512 == 0)
                {
                    if (mParentList.EnableMinorVersions)
                    {
                        mParentList.EnableMinorVersions = false;
                        mParentList.Update();
                    }

                }
                spListItem.Update();
                if (mVersion < 512)
                {
                    mVersion = 512;
                }
                else
                {
                    mVersion += 512;
                }
                if (preVersion == mVersion)
                {
                    return;
                }
                preVersion = mVersion;
                versionLabels.Add(mVersion);
            }

            if (versionLabels.Count > 0)
            {
                mContext.ExecuteQuery();
                //delete middle versions
                string webAppName = AveUrlUtility.GetServerUrl(mContext.Url);
                string listId = mParentList.Id.ToString();
                string fileName = spListItem["FileRef"].ToString();
                string op = "Delete";

                for (int i = 0; i < versionLabels.Count; i++)
                {
                    try
                    {
                        mRequest.OperateOnVersion(mAveWebCache.ServerRelativeUrl, webAppName, mObj, mParentList.DefaultViewUrl, spListItem.Id, versionLabels[i], listId, fileName, op);
                    }
                    catch (Exception e)
                    {
                        mLogger.Debug(AveClientOMRequestResource.UpdateToSpecificVersionError, versionLabels[i], this.mName, e.ToString());
                    }
                }
            }
        }

        internal Folder GetFolder(Folder parentFolder)
        {
            Folder folder = null;
            ListItemCollection listItems = null;
            ExceptionHandlingScope excepScope01 = null;
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
                        excepScope01 = new ExceptionHandlingScope(mContext);
                        using (excepScope01.StartScope())
                        {
                            using (excepScope01.StartTry())
                            {
                                folder = GetFolderByAPI(mParentWeb, mFolderRelativeUrl);
                                mContext.Load(folder);
                                mContext.Load(folder, f => f.ParentFolder);
                                listItems = this.GetItem(mFolderRelativeUrl);
                            }
                            using (excepScope01.StartCatch())
                            {
                                mContext.Load(mParentList);
                            }
                        }
                    }
                    else
                    {
                        folder = GetFolderByAPI(mParentWeb, mFolderRelativeUrl);
                        mContext.Load(folder);
                        mContext.Load(folder, f => f.ParentFolder);
                    }
                }
                mContext.ExecuteQuery();
                if (excepScope01 != null && excepScope01.HasException)
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
                mLogger.Warn("Get folder:{0} failed.Error Message:{1}", mFolderRelativeUrl, ex.ToString());
                folder = null;
            }

            return folder;
        }

        private ListItemCollection GetItem(string folderRelativeUrl)
        {
            ListItemCollection listItems = null;
            CamlQuery camlQuery = new CamlQuery();
            folderRelativeUrl = HttpUtility.HtmlEncode(folderRelativeUrl);
            camlQuery.ViewXml = string.Format(
                "<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"FileRef\"/><Value Type=\"Lookup\">{0}</Value></Eq></Where></Query></View>",
                folderRelativeUrl);
            if (mParentList != null)
            {
                listItems = mParentList.GetItems(camlQuery);
                mContext.Load(listItems);
            }
            return listItems;
        }

        private void RestoreDocumentSetProperties(Folder folder, Dictionary<string, object> properties)
        {
            try
            {
                foreach (KeyValuePair<string, object> property in properties)
                {
                    folder.Properties[property.Key] = property.Value;
                }
                folder.Update();
                mContext.Load(folder);
                //此处如果不load mListItem,会导致Increase Version throw Version Conflict Exception
                if (mListItem != null)
                {
                    mContext.Load(mListItem);
                }
                mContext.ExecuteQuery();
            }
            catch (Exception e)
            {
                mLogger.Debug("An error has occurred while restoring document set properties, {0}.", e);
            }
        }

        protected virtual Folder GetFolderByAPI(Web web, string url)
        {
            return web.GetFolderByServerRelativeUrl(url);
        }

        protected virtual Folder AddFolderByAPI(FolderCollection folders, string url)
        {
            return folders.Add(url);
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

    public struct AveListVersionSettings
    {
        public bool EnableVersioning;
        //public bool EnableMajorVersioning;
        public bool EnableMinorVersioning;

        public int MajorVersionLimit;
        public int MinorVersionLimit;
    }

    public class AveListModerationSettingRecorder : IDisposable
    {
        private bool enableModeration;
        private List list;
        private ClientContext context;
        public AveListModerationSettingRecorder(ClientContext context,List list)
        {
            if (list != null)
            {
                enableModeration = list.EnableModeration;
                this.list = list;
            }
            this.context = context;
        }

        public void Dispose()
        {
            if (this.list != null && enableModeration == this.list.EnableModeration)
            {
                list.EnableModeration = enableModeration;
                list.Update();
                this.context.ExecuteQuery();
            }
        }
    }
}
