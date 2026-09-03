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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Restore;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server13
{
    class AveListItemImport : IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private AveSite mSite;
        private AveWeb mParentWeb;
        private AveList mParentList;
        private Guid mPreviousUID;
        private IReport mReport;
        public IReport Report
        {
            get
            {
                if (mReport == null)
                {
                    mReport = new AveWrapperReport();
                }
                return mReport;
            }
        }

        public void SetReport(IReport report)
        {
            mReport = report;
        }

        public AveListItemImport(AveSite site, AveWeb web, AveList list)
        {
            mSite = site;
            mParentWeb = web;
            mParentList = list;
        }

        public object GetObjectData()
        {
            throw new NotImplementedException();
        }

        public AveRestoreResult Import(AveListItemInfo itemInfo)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.Import"))
            {

                AveItem aveItem = itemInfo.AveItem as AveItem;
                AveRestoreResult result = AveRestoreResult.Normal;

                CheckConflict(itemInfo, aveItem, itemInfo.DocData, itemInfo.UserData);

                HandleConflictWithConflictType(itemInfo, aveItem, itemInfo.DocData, itemInfo.UserData);

                result = AddListItem(itemInfo, aveItem, itemInfo.DocData, itemInfo.UserData);

                result = UpdateListItemVersion(itemInfo, aveItem, itemInfo.DocData, itemInfo.UserData);

                PostUpdateListItem(itemInfo, aveItem);

                return result;

            }

        }

        /// <summary>
        /// 判断是否冲突，以及与什么冲突
        /// None = 0,
        /// RecycleBin = 1,
        /// Document = 2,
        /// Both = 3
        /// </summary>
        /// <param name="info"></param>
        /// <param name="aveItem"></param>
        /// <param name="docData"></param>
        /// <param name="userData"></param>
        internal void CheckConflict(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> docData, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.CheckConflict"))
            {

                if (mParentList.List != null && mParentList.BaseTemplate == AveListTemplateType.DesignCatalog)
                {
                    aveItem.CheckConflictStateForComposedLooksItems(info, info.SiteId);
                }
                else if (mParentList.List != null && (int)mParentList.BaseTemplate == 880 && mParentList.Fields.ContainsField("Member") && userData != null && userData.ContainsKey("Member") && (userData["Member"] is int))
                {
                    //int memberId = (int)userData["Member"];
                    var tempValue = (AveFieldValueInfo)info.FieldsInfo.Fields["Member"];
                    int memberId = (int)tempValue.ColValue;
                    string memberIdFieldColName = AveAssemblyUtility.GetPropertyValue(mParentList.List.Fields.GetField("Member"), "ColName").ToString();
                    aveItem.CheckConflictStateForCommunityMember(info.RestoringItem, info.SiteId, info.ParentId, memberId, memberIdFieldColName);
                }
                else if (mParentList.List != null && (int)mParentList.BaseTemplate == 160)
                {
                    aveItem.CheckConflictStateForAccessRequest(info.RestoringItem, (int)userData["RequestedByUserId"], (string)userData["RequestedObjectUrl"]);
                    info.NeedChangeItemId = false;
                }
                else if (mParentList != null && mParentList.IsVariationLabelsList())
                {
                    var labelName = userData.ContainsKey("Title") ? userData["Title"].ToString() : null;
                    var isSource = userData.ContainsKey("Is_x0020_Source") ? (bool)userData["Is_x0020_Source"] : false;
                    aveItem.CheckConflictStateForVariationLabels(info.RestoringItem, labelName, isSource);
                }
                else if (mParentList != null && mParentList.IsRelationshipsList())
                {
                    //此处使用替换过的column url
                    var objectID = info.FieldsInfo.Fields.ContainsKey("ObjectID") ? ((AveFieldValueInfo)info.FieldsInfo.Fields["ObjectID"]).ColValue.ToString() : null;
                    aveItem.CheckConflictStateForRelationshipsList(info.RestoringItem, objectID);
                }
                else if (docData.ContainsKey("DoclibRowId"))
                {
                    if (info.SettingInfo.CheckItemByFieldValue)
                    {
                        var field = mParentList.List.Fields[info.SettingInfo.MatchItemFieldDisplayName];
                        object fieldValue;
                        if (!info.FieldsInfo.Fields.TryGetValue(field.InternalName, out fieldValue))
                        {
                            if (!info.UserData.TryGetValue(field.InternalName, out fieldValue))
                            {
                                fieldValue = string.Empty;
                            }
                        }
                        aveItem.CheckConflictByFieldValue(info, field.InternalName, field.TypeAsString, fieldValue);
                    }
                    else if (info.SettingInfo.KEEP_ITEM_TPGUID)
                    {
                        //check conflict by tp_guid
                        aveItem.CheckConflictState(info.RestoringItem, info.SiteId, info.ParentId, info.tp_Guid);
                    }
                    else if (mParentList.List != null && mParentList.BaseTemplate == AveListTemplateType.DiscussionBoard && userData.ContainsKey("MessageId"))
                    {
                        //check conflict by messageid
                        //只有是list是DiscussionBoard的时候而且是reply有messageId 才应该用特殊的判断冲突
                        string messageId = userData["MessageId"].ToString();
                        Guid messgageIdFieldId = SPBuiltInFieldId.MessageId;
                        string messgageIdFieldColName = AveAssemblyUtility.GetPropertyValue(mParentList.List.Fields[messgageIdFieldId], "ColName").ToString();
                        aveItem.CheckConflictStateForDiscussionReply(info.RestoringItem, info.SiteId, info.ParentId, messageId, messgageIdFieldColName);
                    }
                    else
                    {
                        //check conflict by rowid
                        //mParentFolder.RestoringItem.CheckConflictState(mSqlConn, mSiteId, mParentId);
                        aveItem.CheckConflictStateForListItem(info.RestoringItem, info.SiteId);
                    }
                }

            }

        }

        internal void HandleConflict(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> docData, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.HandleConflict"))
            {

                if (info.RestoringItem.ConflilctFromRecycleBin)
                {
                    //只有在Check for Conflicts in Destination Recycle Bin选择yes，冲突处理选择skip的条件下不会清空回收站
                    if (!(info.RestoringItem.IsIncludingRecycleBinData && info.RestoreOption == AveRestoreMode.Default))
                    { 
                        HandleConflictWithRecycleBin(info);
                    }
                }
                if (info.RestoringItem.ConflictWithDocument)
                {
                    HandleConflictWithDocument(info, aveItem, docData, userData);
                }

            }

            info.RestoringItem.TargetTable = info.RestoringItem.GetTargetTable(info.OriginalVersion, info.IsVersion);
            if (info.RestoringItem.TargetTable == RestoreTargetTable.None)
            {
                //return AveRestoreResult.Omit;
                if (info.RestoringItem.SkipRecycleBinData)
                {
                    throw new AveRestoreException(AveRestoreResult.SkipRecycleBinData, string.Empty);
                }
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }
        }

        internal void HandleConflictWithConflictType(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> docData, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.HandleConflict"))
            {

                switch (info.RestoringItem.ConflictType)
                {
                    case ConflictType.None:
                        {

                            break;
                        }
                    //不能每个都调用HandleConflictWithDocument，需要把这个函数拆分，具体的情况，调用对应情况的方法
                    case ConflictType.Document:
                        {
                            InitSPListitem(info, aveItem, docData, userData);
                            if (info.RestoreOption == AveRestoreMode.Default && !info.RestoringItem.IsNewItem)
                            {
                                int maxVersion = info.RestoringItem.PublishingUIVersion >= info.RestoringItem.DraftUIVersion ? info.RestoringItem.PublishingUIVersion : info.RestoringItem.DraftUIVersion;
                                if (info.OriginalVersion >= maxVersion)
                                {
                                    //ADO-129306
                                    //InitSPListitem(info, aveItem, docData, userData);
                                    info.RestoringItem.NeedSkipped = true;
                                    throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                                }
                            }
                            if (info.IsVersion)
                            {
                                HandleConflictWithDocument(info, aveItem, docData, userData);
                                break;
                            }
                            else if (info.RestoreOption == AveRestoreMode.OverWrite)
                            {
                                HandleConflictWithDocument(info, aveItem, docData, userData);
                            }
                            else if (info.RestoreOption == AveRestoreMode.OverWriteByModifiedTime)
                            {
                                if (userData.ContainsKey("BiggestVersionModified") && userData.ContainsKey("Level") && !aveItem.OverwriteByModifiedTime(info, userData["BiggestVersionModified"], userData["Level"]))
                                {
                                    //InitSPListitem(info, aveItem, docData, userData);
                                    info.RestoringItem.NeedSkipped = true;
                                    //return AveRestoreResult.Omit;
                                    throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                                }

                                HandleConflictWithDocument(info, aveItem, docData, userData);
                            }
                            else if (info.RestoreOption == AveRestoreMode.AppendANewVersion)
                            {
                                HandleConflictWithDocument(info, aveItem, docData, userData);
                                //to do 现在在外围，需要移进来，要考虑如何把新的Item Name返回给外围 1_.000:1024
                            }
                            else if (info.RestoreOption == AveRestoreMode.Append)
                            {
                                HandleConflictWithDocument(info, aveItem, docData, userData);
                                //info.RestoringItem.NeedSkipped = true;
                                //throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString()); //to do 异常类型
                            }
                            break;
                        }
                    case ConflictType.RecycleBin:
                        {
                            if (info.RestoringItem.IsIncludingRecycleBinData)
                            {
                                if (info.RestoreOption == AveRestoreMode.Default)
                                {
                                    info.RestoringItem.NeedSkipped = true;
                                    throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                                }
                            }

                            HandleConflictWithRecycleBin(info);
                            break;
                        }
                    case ConflictType.Both: //不可能出现
                        {
                            InitSPListitem(info, aveItem, docData, userData);
                            info.RestoringItem.NeedSkipped = true;
                            throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());//to do 异常类型
                        }
                    default:
                        {
                            info.RestoringItem.NeedSkipped = true;
                            throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());//to do 异常类型
                        }
                }
                if (aveItem.mSPListItem != null)
                {
                    ResetRestoreParentId(info, aveItem.mSPListItem);
                }

            }

            info.RestoringItem.TargetTable = info.RestoringItem.GetTargetTable(info.OriginalVersion, info.IsVersion, info.RestoreOption);
            if (info.RestoringItem.TargetTable == RestoreTargetTable.None)
            {
                throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
            }

        }

        /// <summary>
        /// 由于目的端ListItem的ParentFolder的和源端的ListItem的ParentFolder的结构可能不同（parent Folder不是用一个level），所以不能使用Info中的ParentId，需要使用目的端ListItem重新获取的ParentFolder
        /// </summary>
        /// <param name="info"></param>
        /// <param name="spListItem"></param>
        private void ResetRestoreParentId(AveListItemInfo info, SPListItem spListItem)
        {
            if (spListItem != null)
            {
                var actualParentFolderUrl = GetParentFolderUrl(spListItem);
                var currentFolderUrl = info.ParentFolderRelativeUrl.Substring(spListItem.Web.ServerRelativeUrl.Length + 1);// ListItem的Url不带Web的ServerRelativeUrl部分，所以比较之前要吧folder的前面Web的url去掉（包括/）
                if (!currentFolderUrl.Equals(actualParentFolderUrl, StringComparison.OrdinalIgnoreCase))
                {
                    var actualFolder = spListItem.Web.GetFolder(actualParentFolderUrl);
                    if (actualFolder != null && actualFolder.Exists)
                    {
                        info.ParentId = actualFolder.UniqueId;
                    }
                }
            }
        }

        private string GetParentFolderUrl(SPListItem spListItem)
        {
            int num = spListItem.Url.LastIndexOf('/');
            if (-1 == num)
            {
                return string.Empty;
            }
            return spListItem.Url.Substring(0, num);
        }

        internal void HandleConflictWithRecycleBin(AveListItemInfo info)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.HandleConflictWithRecycleBin"))
            {

                if (info.SettingInfo.KEEP_ITEM_TPGUID)
                {
                    mSite.QueryService.RemoveListItemInRecycleBin(this.mParentWeb.Site, info.ParentId, info.tp_Guid);
                }
                else
                {
                    mSite.QueryService.RemoveItemInRecycleBin(this.mParentWeb.Site, info.ParentId, info.Name);
                }

            }

        }

        internal void HandleConflictWithDocument(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> docData, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.HandleConflictWithDocument"))
            {

                CheckModifiedTimeOfConflictItem(info, aveItem, docData, userData);
                //InitSPListitem(info, aveItem, docData, userData);
                if (aveItem.mSPListItem != null)
                {
                    if (mParentList.List != null && mParentList.BaseTemplate == AveListTemplateType.DiscussionBoard && aveItem.mSPListItem.FileSystemObjectType == SPFileSystemObjectType.Folder)
                    {
                        //add for Discussions List folder
                    }
                    else
                    {
                        CheckIfIdIsTokenByFolder(info, aveItem);
                        DeleteConflictItem(info, aveItem, docData, userData);
                    }
                }
                info.RestoringItem.TargetTable = info.RestoringItem.GetTargetTable(info.OriginalVersion, info.IsVersion);
                if (info.RestoringItem.TargetTable == RestoreTargetTable.None)
                {
                    //return AveRestoreResult.Omit;
                    throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                }

            }

        }
        internal void InitSPListitem(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> docData, Dictionary<string, object> userData)
        {
            if (mParentList.List != null && mParentList.BaseTemplate == AveListTemplateType.DesignCatalog)
            {
                GetConflictComposedLooksItem(info, aveItem);
            }
            else if (mParentList.List != null && (int)mParentList.BaseTemplate == 880 && mParentList.Fields.ContainsField("Member") && userData != null && userData.ContainsKey("Member") && (userData["Member"] is int))
            {
                //add for Community Members item
                int destId = info.RestoringItem.ConflictRowId;
                if (destId > 0)
                {
                    aveItem.mSPListItem = mParentList.List.GetItemById(destId);
                    return;
                }
            }
            else if (mParentList.List != null && (int)mParentList.BaseTemplate == 160)
            {
                if (info.RestoringItem.ConflictRowId > 0)
                {
                    aveItem.mSPListItem = mParentList.List.GetItemById(info.RestoringItem.ConflictRowId);
                    return;
                }
            }
            else
            {
                GetConflictItem(info, aveItem);
            }
        }
        internal void CheckModifiedTimeOfConflictItem(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> docData, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.CheckModifiedTimeOfConflictItem"))
            {

                if (userData.ContainsKey("Modified") && aveItem.SkipIfSameModifiedTime(info, userData["Modified"]))
                {
                    var spistitem = mParentList.List.GetItemById(aveItem.info.OriginalRowId);
                    if (spistitem != null)
                    {
                        aveItem.InitBySPListItem(spistitem);
                    }
                    info.RestoringItem.NeedSkipped = true;
                    throw new AveRestoreException(AveRestoreResult.SkipTheSameItem, AveRestoreResult.SkipTheSameItem.ToString());
                }
                if (docData.ContainsKey("BiggestVersionModified") && !aveItem.OverwriteByModifiedTime(info, docData["BiggestVersionModified"], null))
                {
                    info.RestoringItem.NeedSkipped = true;
                    //return AveRestoreResult.Omit;
                    throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                }


            }

        }

        internal void GetConflictItem(AveListItemInfo info, AveItem aveItem)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.GetConflictItem"))
            {

                int tempRowId = info.OriginalRowId;
                try
                {
                    //只给Replicator使用，因为Replicator之前有discover逻辑，不需要在有Verify的逻辑
                    if (info.SettingInfo.NewItemWithOutVerifyConflict)
                    {
                        return;
                    }//只给Replicator使用，因为Replicator知道目的端的RowId
                    else if (info.SettingInfo.IncreaceVerionWithRowId && info.RowId > 0)
                    {
                        tempRowId = info.RowId;
                    }
                    else if (mParentList != null && (mParentList.IsVariationLabelsList() || mParentList.IsRelationshipsList()))
                    {
                        if (info.RestoringItem.ConflictRowId > 0)
                        {
                            tempRowId = info.RestoringItem.ConflictRowId;
                        }
                    }
                    else
                    {
                        if (info.SettingInfo.CheckItemByFieldValue)
                        {
                            tempRowId = info.RowId;
                        }
                        else if (info.SettingInfo.KEEP_ITEM_TPGUID)
                        {
                            tempRowId = mSite.QueryService.GetTpIdByTpGuid(mSite.ID, info.tp_Guid, mParentList.ID);
                        }
                        else
                        {
                            tempRowId = info.MappingManager.SiteMappingManager.GetMappingItemId(info.ListId, tempRowId, tempRowId);
                        }
                    }

                    if (tempRowId > 0)
                    {
                        aveItem.mSPListItem = mParentList.List.GetItemById(tempRowId);
                        //同步重新赋值
                        aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetConflictItemError, e.ToString());
                    try
                    {
                        if (mParentList.BaseTemplate == AveListTemplateType.Survey && tempRowId > 0)
                        {
                            if (!aveItem.HasFullControlPermission)
                            {
                                throw new AveRestoreException(AveRestoreResult.Omit, "Skip to restore non-completed survey item because of permission issue");
                            }
                            aveItem.mSPListItem = aveItem.LoadCheckoutListItem(mParentWeb.Web, mParentList.List, tempRowId);
                            aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetConflictItemError, ex.ToString());
                    }
                }

            }

        }
        internal void GetConflictComposedLooksItem(AveListItemInfo info, AveItem aveItem)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.GetConflictItem"))
            {

                try
                {
                    int tempRowId = info.OriginalRowId;
                    if (info.UserData.ContainsKey("Title"))
                    {
                        string title = info.UserData["Title"].ToString();
                        List<int> tpDOCIDS = mSite.QueryService.GetItemsByColumnValue(mSite.ID, mParentList.ID, "nvarchar1", title);
                        if (tpDOCIDS != null && tpDOCIDS.Count > 0)
                        {
                            if (tpDOCIDS[0] > 0)
                            {
                                aveItem.mSPListItem = mParentList.List.GetItemById(tpDOCIDS[0]);
                                //同步重新赋值
                                aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetConflictItemError, e.ToString());
                }

            }

        }
        internal void CheckIfIdIsTokenByFolder(AveListItemInfo info, AveItem aveItem)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.CheckIfIdIsTokenByFolder"))
            {

                if (aveItem.mSPListItem.FileSystemObjectType == SPFileSystemObjectType.Folder)
                {
                    SPFolder spFolder = null;
                    if (aveItem != null && aveItem.mSPListItem != null)
                    {
                        spFolder = aveItem.mSPListItem.Folder;
                    }
                    info.RestoringItem.NeedSkipped = true;
                    if (!info.RestoringItem.OverWrite)
                    {
                        throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                    }
                    if (spFolder != null)
                    {
                        throw new AveWrapperException(AveInternalResourceKey.Wrapper_Exception_Restore_ItemTypeConflict, info.OriginalRowId, spFolder.ServerRelativeUrl);
                        //throw new ItemTypeConflictException(info.OriginalRowId, spFolder.ServerRelativeUrl);
                    }
                    throw new AveWrapperException(AveInternalResourceKey.Wrapper_Exception_Restore_ItemTypeConflict, info.OriginalRowId, aveItem.mSPListItem.Url);
                    //throw new ItemTypeConflictException(info.OriginalRowId, aveItem.mSPListItem.Url);
                }

            }

        }

        internal void DeleteConflictItem(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> docData, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.DeleteConflictItem"))
            {

                if (info.SettingInfo.DELETE_ITEM)
                {
                    try
                    {
                        if (!IsReportingMetadataList() && mParentList.BaseTemplate != AveListTemplateType.Meetings)
                        {
                            info.RestoringItem.OverwriteAllVersion = true;
                            bool movedSuccess = false;
                            if (info.SettingInfo.MOVE_ITEM_TO_CONFLICT_FOLDER)
                            {
                                movedSuccess = aveItem.MoveToConflictFolder(aveItem.mSPList, aveItem.mParentFolder, aveItem.mSPListItem, true);
                            }
                            if (!movedSuccess && !IsWorkflowTask(aveItem))//如果Workflow Task外围没有进行过滤，还原时不能删除目的端，以免造成破坏
                            {
                                aveItem.UnLockItem(aveItem.mSPListItem);
                                bool needSetAlertsDirty = aveItem.IsItemHasAlerts(aveItem.mSPListItem);
                                if (info.KeepDestItemRowId)
                                {
                                    info.DestItemRowId = aveItem.mSPListItem.ID;
                                    info.DestItemUniqueId = aveItem.mSPListItem.UniqueId;
                                }
                                AveListItem listItem = new AveListItem(mParentList, aveItem.mSPListItem);
                                listItem.RemoveItemWorkflowInstance();
                                aveItem.mSPListItem.Delete();
                                //ADO-42263:删除item之后，如果这个item上有alert，需要调用下面方法更新SPWeb的Alerts对象
                                if (needSetAlertsDirty)
                                {
                                    AveAssemblyUtility.InvokeMethod(mParentWeb.Web.Alerts, "SetAlertsDirty", new Type[] { }, new object[] { });
                                }
                                aveItem.mSPListItem = null;
                            }
                            else
                            {
                                aveItem.mSPListItem = null;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (aveItem.mSPListItem != null)
                        {
                            logger.Log(AveLogLevel.WARN, ServerAPIResource.ItemCannotBeDelete, aveItem.mSPListItem.Title, e);
                            if (aveItem.mSPListItem.ParentList.BaseTemplate == SPListTemplateType.Events && mSite.QueryService.IsItemExist(aveItem.mSPListItem.ParentList.ID, aveItem.mSPListItem.ID, aveItem.mSPListItem.ParentList.ParentWeb.Site.ID)) //删掉了
                            {
                                aveItem.mSPListItem = null;
                            }
                        }
                    }
                }

            }

        }

        internal AveRestoreResult AddListItem(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> data, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.AddListItem"))
            {

                if (aveItem.mSPListItem == null)
                {
                    if (mParentList.BaseTemplate == AveListTemplateType.DiscussionBoard
                        && userData.ContainsKey("#ThreadIndexParentId")
                        && (int)userData["#ThreadIndexParentId"] > 0)
                    {
                        AddDicussionReply(info, aveItem, data, userData);
                    }
                    else if (mParentList.BaseTemplate == AveListTemplateType.MeetingUser)
                    {
                        AddMeetingUser(info, aveItem, data, userData);
                    }
                    else if (mParentList.BaseTemplate == AveListTemplateType.Meetings)
                    {
                        AddMeetingSeriesItem(info, aveItem, data, userData);
                    }
                    else if (mParentList.BaseTemplate == AveListTemplateType.Survey)
                    {
                        AddSurveyResponse(info, aveItem, data, userData);
                    }
                    else if ((int)mParentList.BaseTemplate == 880)
                    {
                        AddCommunityMembers(info, aveItem, data, userData);
                    }
                    else if ((int)mParentList.BaseTemplate == 160)
                    {
                        AddAccessRequests(info, aveItem, data, userData);
                    }
                    else
                    {
                        AddDefaultItem(info, aveItem, data, userData);
                    }

                    info.IsNewCreated = true;
                    aveItem.RefreshCacheName(aveItem.mSPListItem.ID);
                }

                //TODO: check the SPListItem int ID(map)
                aveItem.InitBySPListItem(aveItem.mSPListItem);

                //mSite.QueryService.UpdateAllDocsPropertyByNative(info, info.DTimeCreated, info.DTimeLastModified, info.OriginalVersion);
                aveItem.SetDocData("TimeCreated", info.DTimeCreated);
                aveItem.SetDocData("TimeLastModified", info.DTimeLastModified);

                return AveRestoreResult.Normal;

            }

        }

        internal void AddDicussionReply(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> data, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.AddDiscussionReply"))
            {

                //the default parent item is subject
                SPListItem parentSubject = aveItem.mParentFolder.Item;
                SPListItem parentItem = parentSubject;
                int parentId = (int)userData["#ThreadIndexParentId"];
                try
                {

                    int newId = info.MappingManager.SiteMappingManager.GetMappingItemId(info.ListId, parentId);
                    if (newId != -1)
                    {
                        parentItem = mParentList.List.GetItemById(newId);
                    }
                    else//Incremental时，ItemIdMapping中没有当前item的parentId，故将parent指定为subject.此处重新获取parentItem，保证Item的Attachment能正常还原.
                    {
                        parentItem = mParentList.List.GetItemById(parentSubject.ID);
                    }

                }
                catch (Exception e)
                {
                    logger.Warn("Can't find the parent item for this discussion reply. List title: {0}, source parent id: {1}, exception message: {2}, stack trace: {3}", mParentList.Title, parentId, e.Message, e.StackTrace);
                }
                aveItem.mSPListItem = SPUtility.CreateNewDiscussionReply(parentItem);
                //同步重新赋值
                aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);

                //新添加一个reply会修改Subject的Last Updated，这update之前获得Parent的Last Updated值
                try
                {
                    //Reload topic before update topic's Modified.
                    parentSubject = mParentList.List.GetItemById(parentSubject.ID);
                }
                catch (Exception ex)
                {
                    logger.Debug(string.Format("Can't reload the topic item. Message: {0}", ex.ToString()));
                }

                bool lastUpdateValid = false;
                DateTime discussionLastUpdated = DateTime.MinValue;
                if (parentSubject != null)
                {
                    if (parentSubject.Fields.Contains(SPBuiltInFieldId.DiscussionLastUpdated))
                    {
                        if (parentSubject[SPBuiltInFieldId.DiscussionLastUpdated] != null)
                        {
                            //新添加一个reply会修改Subject的Last Updated，这update之前获得Parent的Last Updated值 
                            discussionLastUpdated = (DateTime)parentSubject[SPBuiltInFieldId.DiscussionLastUpdated];
                            lastUpdateValid = true;
                        }
                        else
                        {
                            logger.Warn("Parent subject's discussionLastUpdated is null");
                        }
                    }
                    else
                    {
                        logger.Warn("Parent subject does not have the discussionLastUpdated column.");
                    }
                }
                //SPUtility.CreateNewDiscussionReply 内部实现调用SPUtility.CreateThreadIndex时传入的参数是parentList.ParentWeb.ServerNow，
                //该参数最小单位是秒，而当还原速度超过每秒1个item时，就是导致threadindex这个值相同，进而产生这个问题，
                //而解决方案传入的time精确到ticks，而且现在没发现这个time使用web的时区还是local时区对环境以及数据有任何影响，因此传入Local Now
                string threadindex = SPUtility.CreateThreadIndex(parentItem["ThreadIndex"].ToString(), DateTime.Now);
                aveItem.mSPListItem["ThreadIndex"] = threadindex;
                UpdateInitialFieldValues(info, aveItem, data, userData);
                try
                {
                    if (parentSubject != null && lastUpdateValid)
                    {
                        //如果是Reply，将Parent Subject的Last Updated值改回去
                        parentSubject[SPBuiltInFieldId.DiscussionLastUpdated] = discussionLastUpdated;
                        parentSubject.SystemUpdate(false);
                    }
                }
                catch (Exception e)
                {
                    logger.Warn("Failed to revert discussion last updated for parent item: {0}, list title: {1}, exception message: {2}, stack trace: {3}", parentItem.ID, mParentList.Title, e.Message, e.StackTrace);
                }

            }

        }

        internal void AddMeetingUser(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> data, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.AddMeetingUser"))
            {

                aveItem.mSPListItem = mParentList.List.AddItem(aveItem.mParentFolder.ServerRelativeUrl, SPFileSystemObjectType.File);
                //同步重新赋值
                aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);
                if (userData.ContainsKey("Status"))
                {
                    aveItem.mSPListItem["Status"] = userData["Status"].ToString();
                }
                if (userData.ContainsKey("Attendance"))
                {
                    aveItem.mSPListItem["Attendance"] = userData["Attendance"].ToString();
                }
                if (userData.ContainsKey("Title"))
                {
                    AveAssemblyUtility.SetFieldValue(aveItem.mSPListItem, "m_strNewBaseName", userData["Title"].ToString());
                }
                UpdateInitialFieldValues(info, aveItem, data, userData);

            }

        }

        internal void AddCommunityMembers(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> data, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.AddCommunityMembers"))
            {

                aveItem.mSPListItem = mParentList.List.AddItem(aveItem.mParentFolder.ServerRelativeUrl, SPFileSystemObjectType.File);
                //同步重新赋值
                aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);
                if (userData.ContainsKey("Member"))
                {
                    //userData is source data ,we need to get The value of the replacement from 'info.FieldsInfo.Fields'
                    var tempValue = (AveFieldValueInfo)info.FieldsInfo.Fields["Member"];
                    userData["Member"] = tempValue.ColValue;
                    //mBaseItemInfo.FieldsInfo.Fields
                    aveItem.mSPListItem["Member"] = userData["Member"];
                }
                UpdateInitialFieldValues(info, aveItem, data, userData);

            }

        }

        internal void AddDefaultItem(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> data, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.AddDefaultItem"))
            {

                //Below we need to set the content type once item created, otherwise Update() will give this item default value to the default content type.(Doc-59767)
                SPContentTypeId itemContentTypeId = SPContentTypeId.Empty;
                try
                {
                    if (info.FieldsInfo.Fields.ContainsKey("ContentType"))
                    {
                        itemContentTypeId = ((info.FieldsInfo.Fields["ContentType"] as AveFieldValueInfo).ColValue as AveContentTypeId).ContentTypeId;
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetItemCTIdError, e.ToString());
                }

                //aveItem.mSPListItem = mList.Items.Add(aveItem.mParentFolder.ServerRelativeUrl, SPFileSystemObjectType.File);
                aveItem.mSPListItem = mParentList.List.AddItem(aveItem.mParentFolder.ServerRelativeUrl, SPFileSystemObjectType.File);
                //同步重新赋值
                aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);
                //Here we need to set the content type once item created, otherwise Update() will give this item default value to the default content type.(Doc-59767)
                SPContentTypeId emptyContentTypeId = SPContentTypeId.Empty;
                if (!SPContentTypeId.Equals(itemContentTypeId, emptyContentTypeId))
                {
                    aveItem.mSPListItem["ContentTypeId"] = itemContentTypeId;
                }
                //end

                UpdateInitialFieldValues(info, aveItem, data, userData);
                //SP03升级上来的数据，List Item的UIVersion可能是1，这种Case我们需要Keep住UIVersion
                //这样的数据，wrapper中暂时不处理，SPM 外围已经过滤
                //if (info.OriginalVersion < 512)
                //{
                //    mSite.QueryService.UpdateUIVersionByNative(info, aveItem.mSPListItem.UniqueId);
                //}

            }

        }

        private void AddAccessRequests(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> data, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.AddAccessRequests"))
            {

                aveItem.mSPListItem = mParentList.List.AddItem();
                //同步重新赋值
                aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);
                //对于Access Request ListItem进行特殊处理。
                //这个函数中所还原的fields在item.update后无法还原的。
                string[] fieldList = new string[] { "Title", "RequestId", "RequestedListItemId", "RequestedListId", "InheritingRequestedWebId", "RequestedWebId", "RequestedObjectUrl", "RequestedObjectTitle", "RequestedBy", "RequestedByDisplayName", "RequestedFor", "RequestedForDisplayName", "RequestedFor", "RequestedForDisplayName", "RequestedFor", "RequestedForDisplayName", "AnonymousLinkType", /*"WelcomeEmailBody", "WelcomeEmailSubject",SharePoint API建出来即为空*/ "SendWelcomeEmail", "Conversation", "IsInvitation", "RequestedByUserId", "RequestedForUserId", "Status", "Expires", "PermissionLevelRequested", "PermissionType" };
                foreach (string field in fieldList)
                {
                    try
                    {
                        if (userData.ContainsKey(field))
                        {
                            aveItem.mSPListItem[field] = userData[field];
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while restoring fields values of an Access Request ListItem. Field name: {0}, error: {1}", field, e);
                    }
                }
                UpdateInitialFieldValues(info, aveItem, data, userData);

            }

        }
        internal void AddSurveyResponse(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> data, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.AddSurveyResponse"))
            {

                if (!aveItem.HasFullControlPermission && info.OriginalLevel == 255 && info.CheckoutUserId > 0)
                {
                    SPUser user = mParentWeb.Web.SiteUsers.GetByID(info.CheckoutUserId);
                    SPList checkoutList = mSite.GetCheckoutWeb(mParentWeb.Web, mParentList.List, ref user, Guid.Empty).Lists[mParentList.Id];
                    aveItem.mSPListItem = checkoutList.AddItem(aveItem.mParentFolder.ServerRelativeUrl, SPFileSystemObjectType.File);
                    aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);

                    bool isSetID = false;
                    if (info.NeedChangeItemId && mSite.QueryService.GetNextAvailableId(mSite.ID, mParentList.ID) != info.OriginalRowId && mSite.QueryService.CheckItemIdAvailable(mSite.ID, mParentList.ID, info.OriginalRowId))
                    {
                        AveAssemblyUtility.InvokeMethod(aveItem.mSPListItem, "SetIDForMigration", new Type[] { typeof(int) }, new object[] { info.OriginalRowId });
                        isSetID = true;
                    }
                    AveAssemblyUtility.InvokeMethod(aveItem.mSPListItem, "Checkout");
                    info.CheckoutUserId = -1;
                    if (isSetID)
                    {
                        mSite.QueryService.ChangeNextItemId(info.OriginalRowId, mSite.ID, mParentList.ID);//SetIDForMigration not auto change nexid.
                    }
                }
                else
                {
                    AddDefaultItem(info, aveItem, data, userData);
                }
                //survey list response的fields没有GUID ，因此做特殊处理    
                //can't get calculatedversion in office365 to local mode
                if (userData.ContainsKey("#tp_CalculatedVersion") && aveItem.HasFullControlPermission)
                {
                    //ADO-42742 response completed状态为no的level是255，使用api添加上去的默认的是1；所以用当前item的level去更新Guid；
                    mSite.QueryService.UpdateItemGuid(info.tp_Guid, aveItem.mSPListItem.UniqueId, aveItem.mParentFolder.UniqueId, mSite.ID, Convert.ToBoolean(userData["#tp_IsCurrentVersion"]), Convert.ToByte(aveItem.mSPListItem.Level), (int)userData["#tp_CalculatedVersion"]);
                }

            }

        }

        /// <summary>
        /// update initial columns like tp_guid after additem called
        /// </summary>
        /// <param name="info"></param>
        /// <param name="aveItem"></param>
        /// <param name="data"></param>
        /// <param name="userData"></param>
        internal void UpdateInitialFieldValues(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> data, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.UpdateInitialFieldValues"))
            {

                // InstanceID 属性需要更新，但在updateField时更新不成功，所以放在这里更新
                if (userData.ContainsKey("#tp_InstanceID"))
                {
                    aveItem.mSPListItem["InstanceID"] = userData["#tp_InstanceID"];
                }
                if (AveWebDatabaseSite.IsWebDatabaseWeb(aveItem.mWeb.Web))
                {
                    AveWebDatabaseSite.AppendRequiredFieldsForNewItem(aveItem.mSPListItem, data, userData);
                }
                //新建一个item时，在update之前set "GUID"，可以将其更新,不是所有的listitem都有这个guid的属性
                if (userData.ContainsKey("#tp_GUID") && aveItem.mSPListItem.Fields.ContainsField("GUID"))
                {
                    aveItem.mSPListItem["GUID"] = info.tp_Guid;//userData["#tp_GUID"];
                }
                //新建一个item时，在update之前用SetIDForMigration设置itemId，可以将itemId更新
                if (info.DestItemUniqueId != Guid.Empty && info.DestItemRowId > 0)
                {
                    MigrateItemId(aveItem, info);
                }
                else if (info.NeedChangeItemId && mSite.QueryService.GetNextAvailableId(mSite.ID, mParentList.ID) != info.OriginalRowId && mSite.QueryService.CheckItemIdAvailable(mSite.ID, mParentList.ID, info.OriginalRowId))
                {
                    MigrateItemId(aveItem, info);
                }
                else
                {
                    aveItem.mSPListItem.Update();
                }

            }

        }

        private AveRestoreResult UpdateListItemVersion(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> data, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.UpdateListItemVersion"))
            {

                AveRestoreResult result = AveRestoreResult.Normal;
                if (info.OriginalVersion < info.Version)
                {
                    //if (info.RestoreOption == AveRestoreMode.Default)
                    //{
                    //    info.RestoringItem.NeedSkipped = true;
                    //    throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                    //}
                    if (aveItem.HasFullControlPermission)
                    {
                        //originalVersion < destVersion
                        //Insert version
                        if (!mSite.QueryService.CreateVersionByNative(info, info.OriginalVersion, info.RestoringItem))
                        {
                            //return AveRestoreResult.Failed;
                            if (!info.RestoringItem.OverWrite)
                            {
                                info.RestoringItem.NeedSkipped = true;
                                return AveRestoreResult.Normal;
                            }
                            throw new AveRestoreException(AveRestoreResult.Failed, AveRestoreResult.Failed.ToString());
                        }
                        info.FieldsInfo.Fields = aveItem.ConvertToFieldWithNativeName(info.FieldsInfo.Fields);
                        //用数据库增加version，有些field需要添加进来
                        if (data.ContainsKey("DraftOwnerId"))
                        {
                            data["DraftOwnerId"] = info.DraftOwnerId;
                            info.FieldsInfo.Fields.Add("tp_DraftOwnerId", info.DraftOwnerId);
                        }
                        if (userData.ContainsKey("#tp_IsCurrentVersion"))
                        {
                            info.FieldsInfo.Fields.Add("tp_IsCurrentVersion", userData["#tp_IsCurrentVersion"]);
                        }
                        info.FieldsInfo.Fields.Add("tp_ModerationStatus", info.ModerationStatus);

                        //插入Version，如果目的端已经存在，这时候不去修改Level的值，否则会导致结构乱套
                        //如果是100表示这个记录是我们自己插入的
                        int originalLevel = info.OriginalLevel;
                        byte level = mSite.QueryService.GetLevel(info, info.OriginalVersion);
                        if (level != 100)
                        {
                            originalLevel = level;
                        }

                        info.FieldsInfo.Fields.Add("tp_Level", originalLevel);
                        mSite.QueryService.UpdateVersionByNative(info, info.RestoringItem, data, info.FieldsInfo.Fields, info.OriginalVersion);
                        info.Level = originalLevel;
                        result = AveRestoreResult.ResoreLessVersion;
                    }
                    else
                    {
                        string msg = string.Format("Skip to restore historical version of list item because of lack of permission. Item Url:{0}", aveItem.ListItem.Url);
                        logger.Log(AveLogLevel.WARN, msg);
                        throw new AveWrapperSkipException(msg);
                    }
                }
                else if (info.OriginalVersion == info.Version)
                {
                    // originalVersion == destVersion
                    aveItem.UpdateListItemModerationStatus(info);
                    aveItem.SetReport(Report);
                    aveItem.UpdateFields(info.FieldsInfo.Fields, info, false, false);
                    
                    if (info.DraftOwnerId > 0 && aveItem.mSPListItem.Level == SPFileLevel.Draft)
                    {
                        aveItem.SetDocData("DraftOwnerId", info.DraftOwnerId);
                        aveItem.SetUserData("tp_DraftOwnerId", info.DraftOwnerId);
                    }
                    result = AveRestoreResult.RestoreEqualVersion;
                }
                else
                {
                    // originalVersion > destVersion
                    if (!info.RestoringItem.IsNewItem)
                    {
                        info.RestoringItem.NeedSkipped = true;
                        throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                    }
                    aveItem.CreateItemVersion(info.OriginalVersion, info.IsNewCreated);
                    aveItem.UpdateListItemModerationStatus(info);
                    aveItem.SetReport(Report);
                    aveItem.UpdateFields(info.FieldsInfo.Fields, info, false, false);
                    
                    info.IsNewCreated = true;

                    //TODO: update system property
                    result = AveRestoreResult.RestoreBiggerVersion;
                }
                return result;

            }

        }

        private void PostUpdateListItem(AveListItemInfo info, AveItem aveItem)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.PostUpdateListItem"))
            {

                // for survey response
                if (mParentList.BaseTemplate == AveListTemplateType.Survey && aveItem.HasFullControlPermission)
                {
                    if (info.OriginalLevel == 255 && info.CheckoutUserId > 0)
                    {
                        mSite.QueryService.ChangeCheckoutUserID(info, aveItem.mSPListItem.UniqueId, info.CheckoutUserId);
                    }
                    if (aveItem.mSPListItem.Level != (SPFileLevel)info.OriginalLevel || (aveItem.mSPListItem.Level == SPFileLevel.Draft))
                    {
                        mSite.QueryService.ChangeLevelByNative(info, aveItem.ListItem, info.OriginalVersion, info.OriginalLevel, info.DraftOwnerId);
                        info.Level = info.OriginalLevel;
                        logger.Log(AveLogLevel.DEBUG, "Change survey item level by native. Original level: {0}, Row Id: {1}, List Title: {2}, Web Name: {3}", info.OriginalLevel, aveItem.mSPListItem.ID, aveItem.mList == null ? string.Empty : aveItem.mList.Title, aveItem.Web.Name);
                    }
                }
                if (info.NeedUpdateStatusByNative)
                {
                    //mSite.QueryService.ChangeModerationStatusByNative(info, aveItem.ListItem.UniqueId, info.ModerationStatus);                
                    aveItem.SetUserData("tp_ModerationStatus", info.ModerationStatus);
                }
                if (aveItem.ListItem.ContentType != null && aveItem.ListItem.ContentType.ID.IsChildOf(new AveContentTypeId(SPBuiltInContentTypeId.Event)))
                {
                    try
                    {
                        if (aveItem.ListItem.Fields.Contains(SPBuiltInFieldId.WorkspaceLink) && aveItem.ListItem[SPBuiltInFieldId.WorkspaceLink] != null && Convert.ToBoolean(aveItem.ListItem[SPBuiltInFieldId.WorkspaceLink]))
                        {
                            var link = aveItem.ListItem[SPBuiltInFieldId.Workspace].ToString();
                            //选择Recurrence时，url和title为“，”分隔，没有“？”和item id
                            var linkUrl = link.Substring(0, link.IndexOf(link.Contains('?') ? "?" : ",", StringComparison.OrdinalIgnoreCase));
                            var array = new Object[] { aveItem.mSPList.ID.ToString(), aveItem.ListItem.ID };
                            if (!aveItem.info.MappingManager.SiteMappingManager.MeetingWorkSpaceMapping.Keys.Contains(linkUrl))
                            {
                                aveItem.info.MappingManager.SiteMappingManager.MeetingWorkSpaceMapping.Add(linkUrl, array);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while adding an event link mapping. Exception: {0}", e);
                    }
                }
                aveItem.UpdateDataByNative(true, true);


            }

        }

        internal bool IsReportingMetadataList()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.IsReportingMetadataList"))
            {

                bool isReportMetadataList = false;
                try
                {
                    SPWeb web = mParentWeb.Web;
                    if (web.Properties.ContainsKey("_reportinggallerymetadataid"))
                    {
                        string Guid = web.Properties["_reportinggallerymetadataid"];
                        if (web.Properties["_reportinggallerymetadataid"].Equals(mParentList.ID.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            isReportMetadataList = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.IsReportingMetadataFailed, e);
                }
                return isReportMetadataList;

            }

        }

        /// <summary>
        /// 在某些情况下更改了Id然后update，API会出错，暂时没有找到具体原因以及好的解决方案，添加try catch
        /// 如果update失败，revert相关逻辑，不进行Id替换
        /// </summary>
        /// <param name="aveItem"></param>
        /// <param name="info"></param>
        private void MigrateItemId(AveItem aveItem, AveListItemInfo info)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.MigrateItemId"))
            {

                bool keepId = info.DestItemRowId > 0 && info.DestItemUniqueId != Guid.Empty;
                try
                {
                    int rowId = keepId ? info.DestItemRowId : info.OriginalRowId;
                    AveAssemblyUtility.InvokeMethod(aveItem.mSPListItem, "SetIDForMigration", new Type[] { typeof(int) }, new object[] { rowId });
                    if (keepId || info.ListContainsTodayFomula)
                    {
                        //相当调用Update
                        AveAssemblyUtility.InvokeMethod(aveItem.mSPListItem, "MigrationAddOrUpdate", new object[] { true, false, info.DestItemUniqueId, false, true });
                    }
                    else
                    {
                        aveItem.mSPListItem.Update();
                    }
                }
                catch (UnauthorizedAccessException)
                {//ListItem创建以后的第一个Update，需要考虑权限不足的情况，
                    //如果权限不足没有必要再进行还原
                    throw;
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateItemError, e);
                    throw;
                }
                if (!keepId)
                {
                    mSite.QueryService.ChangeNextItemId(info.OriginalRowId, mSite.ID, mParentList.ID);//SetIDForMigration not auto change nexid.
                }

            }

        }
        [SuppressMessage("FxCopCustomRules", "C100003:DoNotUseSpecificSPMethod")]
        private void AddMeetingSeriesItem(AveListItemInfo info, AveItem aveItem, Dictionary<string, object> docData, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.AddMeetingSeriesItem"))
            {

                try
                {
                    SPListItem listItem = null;
                    try
                    {
                        listItem = mParentList.List.GetItemById((int)userData["#tp_ID"]);
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetItemFieldError, e);
                    }
                    if (listItem == null)
                    {
                        listItem = mParentList.List.AddItem();
                    }
                    if (userData.ContainsKey("Title"))
                    {
                        listItem["Title"] = userData["Title"];
                    }
                    int eventType = 0;
                    if (userData.ContainsKey("EventType"))
                    {
                        eventType = (int)userData["EventType"];
                        int IsDetached = userData.ContainsKey("IsDetached") ? (int)userData["IsDetached"] : -1;
                        string mEventUID = userData.ContainsKey("EventUID") ? userData["EventUID"].ToString() : null;
                        //update will fail if we don't assign recurrenceID field when eventtype is 0,please see ADO-5026 for more detail
                        if (eventType == 0)
                        {
                            //之前针对ADO-5026的修改没有考虑在Calendar创建的Item不勾选Recurrence的情况，导致不勾选Recurrence的item关联的workspcae下的Item的EventType还原不正确，
                            //根据不勾选Recurrence的Item的特性添加后半部分的判断。
                            if (IsDetached == 0 && mEventUID != null)
                            {
                                listItem["EventType"] = eventType;
                            }
                            else
                            {
                                listItem["EventType"] = 2;
                                listItem["RecurrenceID"] = DateTime.Now;
                            }
                        }
                        else
                        {
                            listItem["EventType"] = eventType;
                        }
                    }
                    int timeZoneId = -1;
                    SPTimeZone timeZone = null;
                    DateTime eventDate = DateTime.MinValue;
                    DateTime endDate = DateTime.MinValue;
                    int duration = -1;
                    if (userData.ContainsKey("TimeZone"))
                    {
                        timeZoneId = (int)userData["TimeZone"];
                        listItem["TimeZone"] = timeZoneId;
                    }
                    else if (userData.ContainsKey("UID") && (eventType == 2 || eventType == 3))
                    {
                        foreach (SPListItem tItem in mParentList.List.Items)
                        {
                            if (tItem["UID"] != null && (Guid)userData["UID"] == new Guid(tItem["UID"].ToString())
                                && (int)tItem["EventType"] == 1
                                && tItem["TimeZone"] != null)
                            {
                                timeZoneId = (int)tItem["TimeZone"];
                                if (tItem["Duration"] != null)
                                {
                                    duration = (int)tItem["Duration"];
                                }
                                break;
                            }
                        }
                    }
                    //if (AveListMappingManager.TimeZoneDic == null)
                    //{
                    //    AveListMappingManager.TimeZoneDic = new Dictionary<int, IAveTimeZone>();
                    //    foreach (SPTimeZone tz in SPRegionalSettings.GlobalTimeZones)
                    //    {
                    //        AveListMappingManager.TimeZoneDic.Add(tz.ID, new AveTimeZone(tz));
                    //    }
                    //}
                    if (timeZoneId == 0)
                    {
                        timeZoneId = 93;
                    }
                    //if (AveListMappingManager.TimeZoneDic.ContainsKey(timeZoneId))
                    //{
                    //    timeZone = (AveListMappingManager.TimeZoneDic[timeZoneId] as AveTimeZone).TimeZone;
                    //}
                    //else
                    //{
                    SPUser agentAccount = listItem.ParentList.ParentWeb.Site.RootWeb.CurrentUser;
                    if (agentAccount != null && agentAccount.RegionalSettings != null)
                    {
                        timeZone = agentAccount.RegionalSettings.TimeZone;
                    }
                    else
                    {
                        timeZone = listItem.ParentList.ParentWeb.RegionalSettings.TimeZone;
                    }
                    //}
                    if (userData.ContainsKey("EventDate"))
                    {
                        eventDate = timeZone.UTCToLocalTime(Convert.ToDateTime(userData["EventDate"], System.Globalization.DateTimeFormatInfo.InvariantInfo));
                        listItem["EventDate"] = eventDate;
                    }
                    if (userData.ContainsKey("Duration"))
                    {
                        duration = (int)userData["Duration"];
                        listItem["Duration"] = duration;
                    }

                    if (userData.ContainsKey("EndDate"))
                    {
                        endDate = Convert.ToDateTime(userData["EndDate"], System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        //MaxDateTime
                        if (endDate.Year != 9999 && eventDate != DateTime.MinValue && duration >= 0)
                        {
                            TimeSpan tsEventDate = eventDate.TimeOfDay;
                            TimeSpan tsDuration = TimeSpan.FromSeconds(duration);
                            endDate = endDate.Date.Add(tsEventDate).Add(tsDuration);
                        }
                        else
                        {
                            endDate = timeZone.UTCToLocalTime(endDate);
                        }
                        listItem["EndDate"] = endDate;
                    }
                    else if (eventType == 3 && eventDate != DateTime.MinValue && duration >= 0)
                    {
                        TimeSpan tsDuration = TimeSpan.FromSeconds(duration);
                        endDate = eventDate.Add(tsDuration);
                        listItem["EndDate"] = endDate;
                    }
                    if (userData.ContainsKey("RecurrenceID"))
                    {
                        DateTime recurrenceID = mParentList.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(Convert.ToDateTime(userData["RecurrenceID"], System.Globalization.DateTimeFormatInfo.InvariantInfo));
                        listItem["RecurrenceID"] = recurrenceID;
                    }

                    if (userData.ContainsKey("UID"))
                    {
                        listItem["UID"] = userData["UID"];
                        mPreviousUID = (Guid)userData["UID"];
                    }
                    else
                    {
                        listItem["UID"] = mPreviousUID;
                    }
                    if (userData.ContainsKey("Location"))
                    {
                        listItem["Location"] = userData["Location"];
                    }
                    if (userData.ContainsKey("RecurrenceData"))
                    {
                        listItem["RecurrenceData"] = userData["RecurrenceData"];
                    }
                    if (userData.ContainsKey("fAllDayEvent"))
                    {
                        listItem["fAllDayEvent"] = userData["fAllDayEvent"];
                    }
                    if (userData.ContainsKey("fRecurrence"))
                    {
                        listItem["fRecurrence"] = userData["fRecurrence"];
                    }
                    if (userData.ContainsKey("RRule"))
                    {
                        listItem["RRule"] = userData["RRule"];
                    }
                    if (userData.ContainsKey("ExRule"))
                    {
                        listItem["ExRule"] = userData["ExRule"];
                    }
                    if (userData.ContainsKey("SuppressUntil"))
                    {
                        listItem["SuppressUntil"] = mParentList.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(Convert.ToDateTime(userData["SuppressUntil"], System.Globalization.DateTimeFormatInfo.InvariantInfo));
                    }
                    if (userData.ContainsKey("IsOrphaned"))
                    {
                        //DOC-67486，在此处设置listItem["IsOrphaned"]=true或者不设置该值，都会导致listItem.Update抛出异常
                        //所以在此处设置listItem["IsOrphaned"] = false，如果是true在之后更新field的时候会更新正确。
                        //listItem["IsOrphaned"] = userData["IsOrphaned"];
                        listItem["IsOrphaned"] = false;
                    }
                    if (userData.ContainsKey("IsException"))
                    {
                        listItem["IsException"] = userData["IsException"];
                    }
                    if (userData.ContainsKey("IsDetached"))
                    {
                        listItem["IsDetached"] = userData["IsDetached"];
                    }
                    if (userData.ContainsKey("Sequence"))
                    {
                        listItem["Sequence"] = userData["Sequence"];
                    }
                    if (userData.ContainsKey("DTStamp"))
                    {
                        listItem["DTStamp"] = mParentList.ParentWeb.RegionalSettings.TimeZone.UTCToLocalTime(Convert.ToDateTime(userData["DTStamp"], System.Globalization.DateTimeFormatInfo.InvariantInfo));
                    }
                    if (userData.ContainsKey("#tp_InstanceID"))
                    {
                        listItem["InstanceID"] = userData["#tp_InstanceID"];
                    }
                    if (userData.ContainsKey("EventUID"))
                    {
                        ProcessEventUidForMeeting(info, userData);
                        listItem["EventUID"] = userData["EventUID"];
                    }
                    else
                    {
                        //listItem["EventUID"] = null;
                    }
                    if (userData.ContainsKey("Organizer"))
                    {
                        listItem["Organizer"] = info.Extension.PrincipalId;
                    }
                    if (userData.ContainsKey("EventUrl") && userData.ContainsKey("EventUrl#2"))
                    {
                        SPFieldUrlValue tValue = new SPFieldUrlValue();
                        tValue.Description = userData["EventUrl#2"].ToString();
                        tValue.Url = info.Extension.FieldUrlValue;
                        listItem["EventUrl"] = tValue;
                    }
                    //新建一个item时，在update之前set "GUID"，可以将其更新,不是所有的listitem都有这个guid的属性
                    if (userData.ContainsKey("#tp_GUID") && listItem.Fields.ContainsField("GUID"))
                    {
                        listItem["GUID"] = info.tp_Guid;//userData["#tp_GUID"];
                    }
                    //新建一个item时，在update之前用SetIDForMigration设置itemId，可以将itemId更新
                    if (mSite.QueryService.CheckItemIdAvailable(mSite.ID, mParentList.ID, info.OriginalRowId) && info.NeedChangeItemId)
                    {
                        AveAssemblyUtility.InvokeMethod(listItem, "SetIDForMigration", new Type[] { typeof(int) }, new object[] { info.OriginalRowId });
                        mSite.QueryService.ChangeNextItemId(info.OriginalRowId, mSite.ID, mParentList.ID);
                    }
                    listItem.ParentList.ParentWeb.Site.WebApplication.FormDigestSettings.Enabled = false;
                    listItem.Update();
                    aveItem.mSPListItem = listItem;
                    //同步重新赋值
                    aveItem.ListItem = new AveListItem(aveItem.mList, aveItem.mSPListItem);
                }
                catch (Exception e)
                {
                    logger.Log(AveLogLevel.WARN, "An error occurred while adding meeting series item. Error: {0}", e);
                    throw;
                }

            }

        }

        /// <summary>
        /// 将meetingserials的listItem的EventUID中的ListID装换为目的端的ListID
        /// </summary>
        /// <param name="data"></param>
        private void ProcessEventUidForMeeting(AveListItemInfo info, Dictionary<string, object> userData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.ProcessEventUidForMeeting"))
            {

                if (userData.ContainsKey("EventType") && (userData["EventType"].ToString().Equals("1") || userData["EventType"].ToString().Equals("0")))
                {
                    try
                    {
                        if (userData.ContainsKey("EventUrl"))
                        {
                            string sourceUrl = userData["EventUrl"].ToString();
                            string webUrl = info.Extension.DestUrl.Substring(0, info.Extension.DestUrl.LastIndexOf('/'));
                            webUrl = webUrl.Substring(0, webUrl.LastIndexOf('/'));
                            using (SPWeb web = mParentList.List.ParentWeb.Site.OpenWeb(webUrl))
                            {
                                SPList list = web.GetList(info.Extension.DestUrl);
                                string ListID = list.ID.ToString();
                                if (userData.ContainsKey("EventUID"))
                                {
                                    string EventUID = userData["EventUID"].ToString();
                                    string sourceUID = EventUID.Substring(EventUID.IndexOf('{') + 1, 36);
                                    userData["EventUID"] = EventUID.Replace(sourceUID, ListID);
                                    if (info.FieldsInfo.Fields.ContainsKey("EventUID"))
                                    {
                                        (info.FieldsInfo.Fields["EventUID"] as AveFieldValueInfo).ColValue = userData["EventUID"];
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.WARN, ServerAPIResource.ReplaceListIdForMeetingSerialsFailed, e);
                        //mLog.Warn("An error occured while replace the dest ListID for MeetingSerials ListItem");
                    }
                }

            }

        }

        private bool IsWorkflowTask(AveItem item)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveListItemImport.IsWorkflowTask"))
            {

                bool isWorkflowInstance = false;
                if (mParentList.BaseTemplate == AveListTemplateType.Tasks && item != null && item.mSPListItem != null && item.mSPListItem.ContentType != null)
                {
                    if (mParentList.BaseTemplate == AveListTemplateType.Tasks && item != null && item.mSPListItem.ContentType != null)
                    {
                        string contentTypeId = item.mSPListItem.ContentTypeId.ToString();
                        if ((!string.IsNullOrEmpty(contentTypeId)) && contentTypeId.StartsWith("0x010801", StringComparison.OrdinalIgnoreCase))
                        {
                            isWorkflowInstance = true;
                        }
                    }
                }
                return isWorkflowInstance;

            }

        }

        public void Dispose()
        {
            if (mReport != null)
                mReport.Dispose();
        }
    }
}
