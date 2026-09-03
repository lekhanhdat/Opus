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
    class AveFolderRestore : IDisposable
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveFolderRestore));
        private Site mSite;
        private Web mParentWeb;
        private List mParentList;
        private string mParentFolderUrl;
        private Folder mParentFolder;
        private string mFolderRelativeUrl;
        private string mName;
        private int mRowId;
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
        private int mModerationStatus;        
        private object mObj;
        private AveListItemRestore mItemRestore;
        private AveClientContext mContext;
        private ListItem mListItem;
        private AveClientOMRequest mRequest;

        public AveFolderRestore(AveClientOMRequest request, Site site, AveClientContext context, object obj)
        {
            mSite = site;
            mContext = context;
            mObj = obj;
            mRequest = request;
        }
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Doclib is a part of Keys")]
        protected void PrepareRestoreContext(Dictionary<string, object> data)
        {
            mParentWeb = mContext.Site.OpenWeb(data["WebUrl"] as string);
            mParentFolderUrl = data.ContainsKey("FolderUrl") ? data["FolderUrl"] as string : string.Empty;
            mParentFolder = mParentWeb.GetFolderByServerRelativeUrl(mParentFolderUrl);
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
            if (TryGetListId(data)!=Guid.Empty)
            {
                mParentList = mParentWeb.Lists.GetById(new Guid(data["ListId"] as string));
            }
            mItemRestore = mParentList != null ? new AveListItemRestore(mRequest, mSite, mParentWeb, mParentList, mRowId, mModerationStatus, mContext, mObj) : null;            
            mContext.Load(mParentWeb);
            mContext.Load(mParentFolder);
            if (mParentList != null)
            {
                mContext.Load(mParentList);
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
                    /*
                     * 由于访问External List里面的root folder下第一个sub folder的Item对象时，会出现Access Denied的错误，但是第二次取没有问题。
                     * 目前没有找到好的方法来避免这个错误，所以先试取下，然后再走以前的还原逻辑。
                     */
                    if (spFolder != null )
                    {
                        RestoreFolderSpecialProperties(spFolder, docData);
                        if (mListItem != null)
                        {
                            Guid uniqueId = (Guid)mListItem["UniqueId"];
                            if (uniqueId != (Guid)mListItem["UniqueId"])
                            {
                                spFolder = mParentWeb.GetFolderByServerRelativeUrl(spFolder.ServerRelativeUrl);
                            }

                            //mAveItem.InitBySPListItem((spFolder.Item);

                            mVersion = (int)mListItem["_UIVersion"];

                            if (mOriginalVersion == mVersion)
                            {
                                mItemRestore.UpdateListItem(ref mListItem, userData, ListItemUpdateMethodKind.SystemUpdate, false);
                            }
                            else if (mOriginalVersion > mVersion)
                            {
                                UpdateToSpecificVersion(mListItem, mOriginalVersion, mIsNewCreated);
                                mItemRestore.UpdateListItem(ref mListItem, userData, ListItemUpdateMethodKind.SystemUpdate, false);
                            }
                            //暂时注释掉该方法，由于UpdateModifiedAndModeration存在问题，无法KeepModified，所以注释掉该方法以KeepModified
                            //Restore the ModerationStatus of Document Library's Folder.
                            //if (mParentList != null && mParentList.BaseType == BaseType.DocumentLibrary && mListItem.FieldValues.Count > 0)
                            //{
                            //    UpdateModifiedAndModeration(userData);
                            //}
                        }
                    }
                }
            }

            if (spFolder != null)
            {
                mRequest.AssembleFolderProperties(mContext, mParentWeb.ServerRelativeUrl, spFolder, spFolder.ServerRelativeUrl, folderProperties);
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
                    folderProperties["UniqueId"] = itemProperties["UniqueId"];
                }
            }
            return folderProperties;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "property key")]
        private Folder RestoreFolderSpecialProperties(Folder spFolder, Dictionary<string, object> docData)
        {
            var changeMetaInfo =  new Dictionary<string, object>();
            var changeProperties = new Dictionary<string, object> { { "ChangedMetaInfo", changeMetaInfo } };
            bool modifyDcLastRefresh = false;
            bool changed = false;
            if (docData.ContainsKey("Properties"))
            {
                Hashtable properties = (Hashtable)docData["Properties"];
                //BrowserFormWebPartProperties
                if (properties.ContainsKey("_ipfs_solutionName"))
                {
                    changeMetaInfo["_ipfs_solutionName"] = properties["_ipfs_solutionName"];
                    changed = true;
                }
                if (properties.ContainsKey("_ipfs_infopathenabled"))
                {
                    changeMetaInfo["_ipfs_infopathenabled"] = properties["_ipfs_infopathenabled"];
                    changed = true;
                }
                if (properties.ContainsKey("docset_LastRefresh"))
                {
                    changeMetaInfo["docset_LastRefresh"] = properties["docset_LastRefresh"];
                    modifyDcLastRefresh = true;
                    changed = true;
                }
            }
            ///SAAS-5048
            if (!modifyDcLastRefresh && docData.ContainsKey("docset_LastRefresh"))
            {
                changeMetaInfo["docset_LastRefresh"] = docData["docset_LastRefresh"];
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
                        changeMetaInfo[key] = MetaInfoDic[key];
                        changed = true;
                    }
                }
            }
            if (changed)
            {
                //由于API limitation，10模拟不能直接更新Properties，使用Post方式更新。
                this.mRequest.UpdateFile(this.mParentWeb.ServerRelativeUrl, this.mParentListName, spFolder.ServerRelativeUrl, changeProperties);
            }
            return spFolder;
        }

        private void UpdateModifiedAndModeration(Dictionary<string, object> userData)
        {
            DateTime originalModified = DateTime.MinValue;
            string moderationComments = string.Empty;
            if (NeedWebServiceUpdate(userData, ref originalModified, ref moderationComments))
            {
                ResetModerationStatus(mParentList);
                string webAppName = AveUrlUtility.GetServerUrl(mContext.Url);
                Dictionary<string, object> needKeepData = new Dictionary<string, object>();
                needKeepData["ModerationStatus"] = mModerationStatus;
                needKeepData["Modified"] = originalModified;
                needKeepData["ModerationComments"] = moderationComments;
                AveWebServiceRequest.UpdateListItems(webAppName, mParentWeb.ServerRelativeUrl, mParentList.Title, mListItem.Id, mListItem.FieldValues["FileRef"].ToString(), mObj, needKeepData);
            }
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
                //(mListItem.FieldValues.ContainsKey("Modified") && !mListItem.FieldValues["Modified"].Equals(originalModified)) ||//if modified equal.
                   (mListItem.FieldValues.ContainsKey("_ModerationComments") && mListItem.FieldValues["_ModerationComments"] != null && !mListItem.FieldValues["_ModerationComments"].Equals(moderationComments));
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
                    if (mOriginalVersion > 0 && mOriginalVersion < 512 && !mParentList.EnableMinorVersions)
                    {
                        mParentList.EnableMinorVersions = true;
                        mParentList.Update();
                    }
                }
                if ((mRowId > 0 && mParentList != null && mParentList.BaseType != BaseType.DocumentLibrary))
                {
                    if (mParentList.BaseTemplate == (int)ListTemplateType.DiscussionBoard)
                    {
                        mListItem = Utility.CreateNewDiscussion(mContext, mParentList, mName);
                    }
                    else
                    {
                        ListItemCreationInformation creationInformation = new ListItemCreationInformation();
                        creationInformation.FolderUrl = mParentFolderUrl;
                        creationInformation.UnderlyingObjectType = FileSystemObjectType.Folder;
                        creationInformation.LeafName = mName;
                        mListItem = mParentList.AddItem(creationInformation);
                    }
                    mListItem.Update();
                    mContext.Load(mListItem);
                    mContext.Load(mListItem, it => it.HasUniqueRoleAssignments);
                    folder = mParentWeb.GetFolderByServerRelativeUrl(mFolderRelativeUrl);
                    mContext.Load(folder);
                    mContext.Load(folder, f => f.ParentFolder);
                    mContext.ExecuteQuery();
                }
                else
                {
                    if (mParentList != null && mParentList.BaseTemplate == 2100)
                    {
                        string webApp = AveUrlUtility.GetServerUrl(mContext.Url);
                        AveWebServiceRequest.AddSlideFolder(webApp, mParentWeb.ServerRelativeUrl, mParentList.Title, mParentFolderUrl, mName, mObj);
                        folder = mParentWeb.GetFolderByServerRelativeUrl(mFolderRelativeUrl);
                    }
                    else
                    {
                        folder = mParentFolder.Folders.Add(mName);
                    }

                    if (mParentList != null && mRowId > 0)
                    {
                        CamlQuery camlQuery = new CamlQuery();
                        camlQuery.ViewXml = string.Format(
                            "<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"FileRef\"/><Value Type=\"Lookup\">{0}</Value></Eq></Where></Query><RowLimit>1</RowLimit></View>",
                            mParentFolderUrl.TrimEnd('/') + "/" + mName);
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
            catch (ServerUnauthorizedAccessException ex)
            {
                mLogger.Debug(AveClientOMRequestResource.AddFolderError, mName, mParentFolder.Name, ex.ToString());
                throw;
            }
            catch (Exception e)
            {
                mLogger.Debug(AveClientOMRequestResource.AddFolderError, mName, mParentFolder.Name, e.ToString());
                throw ;
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
                        mRequest.OperateOnVersion(mParentWeb.ServerRelativeUrl, webAppName, mObj, mParentList.DefaultViewUrl, spListItem.Id, versionLabels[i], listId, fileName, op);
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
                                folder = mParentWeb.GetFolderByServerRelativeUrl(mFolderRelativeUrl);
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
                        folder = mParentWeb.GetFolderByServerRelativeUrl(mFolderRelativeUrl);
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
            camlQuery.ViewXml = string.Format(
                "<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"FileRef\"/><Value Type=\"Lookup\">{0}</Value></Eq></Where></Query></View>",
                folderRelativeUrl);
            if (mParentList != null)
            {
                listItems = mParentList.GetItems(camlQuery);
                mContext.Load(listItems,items=>items.IncludeWithDefaultProperties(it=>it.HasUniqueRoleAssignments));
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
