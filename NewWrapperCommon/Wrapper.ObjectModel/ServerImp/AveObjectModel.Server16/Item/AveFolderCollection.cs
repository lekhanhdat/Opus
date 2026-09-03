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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.Office.DocumentManagement.DocumentSets;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using AvePoint.Wrapper.Restore;
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.ObjectModel.Server16
{
    class AveFolderCollection : AveAbstractCommonCollection<IAveFolder>, IAveFolderCollection, IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveFolderCollection));
        private IReport mReport;

        private bool needUpdateGUIDByNative = true;

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
        private SPFolderCollection mFolders;
        private AveWeb mWeb;
        private AveSite mSite;

        public AveFolderCollection(AveWeb web, SPFolderCollection folders)
            : base(folders)
        {
            mWeb = web;
            mFolders = folders;
            mSite = web.Site as AveSite;
        }

        #region IAveFolderCollection Members

        public void SetReport(IReport report)
        {
            mReport = report;
        }

        public IAveFolder Add(string strUrl)
        {
            return new AveFolder(mWeb, mFolders.Add(strUrl));
        }

        public IAveFolder GetByName(string folderName)
        {
            return this[folderName];
        }

        public IAveFolder this[string name]
        {
            get
            {
                return new AveFolder(mWeb, mFolders[name]);
            }
        }

        #endregion

        public override IAveFolder this[int index]
        {
            get
            {
                return new AveFolder(mWeb, mFolders[index]);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveFolder(mWeb, t as SPFolder);
        }

        public override int Count
        {
            get { return mFolders.Count; }
        }

        public IAveWeb Web
        {
            get { return mWeb; }
        }

        public IAveFolder AveFolder
        {
            get { return new AveFolder(this.mWeb, mFolders.Folder); }
        }

        public AveRestoreResult RestoreFolder(AveFolderInfo info, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFolderCollection.RestoreFolder"))
            {

                AveItem aveItem = info.AveItem as AveItem;
                AveRestoreResult restoreResult = AveRestoreResult.Normal;
                PreRestoreFolder(info, aveItem, allDocData);
                bool isSystemFolder = !CheckAndCreateFolder(info, aveItem);
                RestoreMetaInfo(info, aveItem, allDocData, isSystemFolder);//RestoreMetaInfo放到updateFields之前，确保Modify By不受影响；
                if (!isSystemFolder)
                {
                    restoreResult = RealRestoreFolder(info, aveItem, allDocData, allUserData);
                    PostRestoreFolder(info, aveItem, allUserData);
                }
                return restoreResult;

            }

        }

        private IAveDocumentSet CreateDocumentSet(AveFolderInfo info, IAveContentTypeId contentTypeId, SPList list)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFolderCollection.CreateDocumentSet"))
            {

                //我们一直操作的都是aveItem.mList对象
                SPFolder folder = list.ParentWeb.GetFolder(mFolders.Folder.Url);
                Hashtable hashtable = new Hashtable();
                if (info.UserData.ContainsKey("#tp_GUID"))
                {
                    hashtable.Add("GUID", info.UserData["#tp_GUID"]);
                }
                DocumentSet documentSet = null;
                //sharepoint的API在这个方法里边会出现一些错误，这个错误是由于eventrecevier引起的，尽管有recevier，还是走update。具体可用reflector看。
                try
                {
                    documentSet = DocumentSet.Create(folder, info.Name, new SPContentTypeId(contentTypeId.ToString()), hashtable);
                }
                catch (Exception e)
                {
                    logger.Debug("An error occurred while restoring document set.Name:{0}. the exception is:{1}.", new object[] { info.Name }, e.ToString());
                    return new AveDocumentSet(new AveFolder(AveFolder.ParentWeb as AveWeb, mFolders[info.Name]));
                }
                return new AveDocumentSet(new AveFolder(AveFolder.ParentWeb as AveWeb, documentSet.Folder));

            }

        }

        public IAveDocumentSet CreateDocumentSet(string name, IAveContentTypeId contentTypeId, Hashtable properties)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFolderCollection.CreateDocumentSet_1"))
            {

                var documentSet = DocumentSet.Create(mFolders.Folder, name, new SPContentTypeId(contentTypeId.ToString()), null);
                return new AveDocumentSet(new AveFolder(AveFolder.ParentWeb as AveWeb, documentSet.Folder));

            }

        }

        public IAveDocumentSet CreateDocumentSet(string name, Hashtable properties)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFolderCollection.CreateDocumentSet"))
            {

                var listContentType = this.AveFolder.ParentList.ContentTypes.First(contentType => contentType.ID.IsChildOf(new AveContentTypeId(SPBuiltInContentTypeId.DocumentSet)));
                if (listContentType != null)
                {
                    var documentSet = DocumentSet.Create(mFolders.Folder, name, new SPContentTypeId(listContentType.ID.ToString()), properties);
                    return new AveDocumentSet(new AveFolder(AveFolder.ParentWeb as AveWeb, documentSet.Folder));
                }
                return null;

            }

        }

        /// <param name="aveItem"></param>
        /// <param name="allDocData"></param>
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "vti_winfileattribs is a key")]
        private void RestoreMetaInfo(AveFolderInfo info, AveItem aveItem, Dictionary<string, object> allDocData, bool isSystemFolder)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFolderCollection.RestoreSystemFolderRelativeProperty"))
            {

                if (allDocData.ContainsKey("MetaInfo"))
                {
                    string metaInfoString = AveCompressedUtility.GetTCompressedString((byte[])allDocData["MetaInfo"]);
                    Dictionary<string, string> MetaInfoDic = AveCompressedUtility.GetMetaInfoDictionary(metaInfoString);
                    try
                    {
                        if (aveItem.Folder == null)
                        {
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.RestoreFolderMetainfoFailed, metaInfoString, "The folder cannot be found.");
                            return;
                        }
                        if (!aveItem.Folder.Exists)
                        {
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.RestoreFolderMetainfoFailed, metaInfoString, "The folder not exist .");
                            return;
                        }
                        if (aveItem.Folder.Properties == null)
                        {
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.RestoreFolderMetainfoFailed, metaInfoString, "The folder property is null.");
                            return;
                        }
                        List<string> needRestoredProperties = new List<string>() { "vti_winfileattribs" };
                        if (isSystemFolder)
                        {
                            needRestoredProperties.Add("_ipfs_infopathenabled");
                            needRestoredProperties.Add("_ipfs_solutionName");
                        }
                        bool hasChanged = false;
                        foreach (string key in needRestoredProperties)
                        {
                            if (MetaInfoDic.ContainsKey(key))
                            {
                                aveItem.Folder.Properties[key] = MetaInfoDic[key];
                                hasChanged = true;
                            }
                        }
                        if (hasChanged && isSystemFolder)
                        {
                            if (aveItem.mSPList.EnableVersioning)
                            {
                                aveItem.mSPList.EnableVersioning = false;
                                aveItem.mSPList.Update();
                                info.SettingInfo.LIST_SETTING_CHANGED = true;
                            }
                            aveItem.Folder.Update();
                            aveItem.Folder.Reload();
                        }

                        //还原list下folder的content type的order[ADO-46735]
                        if (!isSystemFolder && MetaInfoDic.ContainsKey("vti_contenttypeorder") && MetaInfoDic["vti_contenttypeorder"] != null && aveItem.mSPList != null)
                        {
                            try
                            {
                                string[] ctIds = MetaInfoDic["vti_contenttypeorder"].Split(',');
                                int count = ctIds.Length;
                                IAveContentType[] tempContentTypes = new IAveContentType[count];
                                int index = 0;
                                foreach (string id in ctIds)
                                {
                                    tempContentTypes[index] = aveItem.mAveParentFolder.ParentList.ContentTypes[aveItem.info.MappingManager.ListMappingManager.ListLevelCTIdMapping[id]];
                                    index++;
                                }
                                bool flag3 = false;
                                IList<IAveContentType> contentTypeOrder = aveItem.Folder.UniqueContentTypeOrder;
                                if (contentTypeOrder != null && contentTypeOrder.Count == tempContentTypes.Length)
                                {
                                    index = 0;
                                    foreach (IAveContentType type in contentTypeOrder)
                                    {
                                        if (type.ID.CompareTo(tempContentTypes[index].ID) != 0)
                                        {
                                            flag3 = true;
                                            break;
                                        }
                                        index++;
                                    }
                                }
                                else
                                {
                                    flag3 = true;
                                }
                                if (flag3)
                                {
                                    StringBuilder tempString = new StringBuilder();
                                    foreach (var ct in tempContentTypes)
                                    {
                                        tempString.AppendFormat("{0},",ct.ID.ToString());
                                    }
                                    tempString.Length--;
                                    //SP16 与O365Document Set只要有更新就会涨verison，此处使用SystemUpdate 来更新，避免张version
                                    aveItem.Folder.Item.Properties["vti_contenttypeorder"] = tempString.ToString();
                                    aveItem.Folder.Item.SystemUpdate();

                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Warn("Restore folder ContentType order: " + ex.Message);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.RestoreFolderMetainfoFailed, metaInfoString, e);
                    }
                }

            }

        }

        private void PostRestoreFolder(AveFolderInfo info, AveItem aveItem, Dictionary<string, object> allUserData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFolderCollection.PostRestoreFolder"))
            {

                if (info.IsNewCreatedFolder && info.SettingInfo.KEEP_ITEM_TPGUID && needUpdateGUIDByNative) //keep tp_guid
                {
                    if (info.OriginalRowId != 0)//目前replicator外围会将folder的tp_guid keep住以保证incrementaldeletion的正常工作，但是对于系统自带的folder，不会进行修改
                    {
                        Guid tp_Guid = Guid.Empty;
                        if (allUserData.ContainsKey("#tp_GUID"))
                        {
                            tp_Guid = new Guid(allUserData["#tp_GUID"].ToString());
                        }
                        //mSite.QueryService.ChangeItemTPGuidByNative(info, info.SiteId, aveItem.mParentFolder.UniqueId, aveItem.Folder.UniqueId, tp_Guid);
                        aveItem.SetUserData("tp_GUID", tp_Guid);
                    }
                }
                aveItem.UpdateDataByNative(true, true);


            }

        }

        private AveRestoreResult RealRestoreFolder(AveFolderInfo info, AveItem aveItem, Dictionary<string, object> allDocData, Dictionary<string, object> allUserData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFolderCollection.RealRestoreFolder"))
            {
                var isDocumentSet = aveItem.ListItem.ContentType == null ? false : AveSPDocumentSet.IsDocumentSet(aveItem.ListItem.ContentType.ID);
                // TODO: SharePoint bug in Discussion board?
                if (aveItem.Folder.UniqueId != aveItem.Folder.Item.UniqueId)
                {
                    aveItem.Folder = mWeb.GetFolder(aveItem.Folder.UniqueId);
                }
                //ADO-47486 由于DocumentSet创建的过程中会缓存parentList对象，与我们使用的list不是同一个对象，之后对我们cache的list的处理对该documentSet不生效，所以需要重新loadItem对象(folder.item取出来的也不行)；
                if (isDocumentSet)
                {
                    aveItem.mList.Reload();
                    aveItem.mSPListItem = aveItem.mSPList.GetItemById(aveItem.Folder.Item.ID);
                    aveItem.InitBySPListItem(aveItem.mSPListItem);
                    //aveItem.InitBySPListItem(((AveListItem)aveItem.Folder.Item).ListItem);
                }

                Dictionary<string, string> metaInfoDic = null;
                if (allDocData.ContainsKey("MetaInfo"))
                {
                    if (aveItem.ListItem.ContentType != null && isDocumentSet
                        && allDocData.ContainsKey("snapshots") && info.OriginalVersion >= info.Version)
                    {
                        //ADO-173533:16 documentSet capture version,document set会产生多个version,需要把最大version的snapshots信息记录下来
                        aveItem.info.MappingManager.ListMappingManager.DocumentSetGuidMetaInfoMapping[aveItem.mSPListItem.UniqueId] = allDocData["snapshots"].ToString();
                    }
                }

                if (info.OriginalVersion < info.Version)
                {
                    if (aveItem.HasFullControlPermission)
                    {
                        //insert version
                        //AveDBQueryService.CreateVersionByNative
                        if (!mSite.QueryService.CreateVersionByNative(info, info.OriginalVersion, info.RestoringItem))
                        {
                            return AveRestoreResult.Failed;
                        }
                        if (allDocData.ContainsKey("DraftOwnerId"))
                        {
                            allDocData["DraftOwnerId"] = info.DraftOwnerId;
                            info.FieldsInfo.Fields.Add("tp_DraftOwnerId", info.DraftOwnerId);
                        }
                        //用数据库增加version，有些field需要添加进来
                        info.FieldsInfo.Fields = aveItem.ConvertToFieldWithNativeName(info.FieldsInfo.Fields);
                        info.FieldsInfo.Fields.Add("tp_ModerationStatus", info.ModerationStatus);
                        int originalLevel = info.OriginalLevel;
                        byte level = mSite.QueryService.GetLevel(info, info.OriginalVersion);
                        if (level != 100)
                        {
                            originalLevel = level;
                        }
                        info.FieldsInfo.Fields.Add("tp_Level", originalLevel);
                        mSite.QueryService.UpdateVersionByNative(info, info.RestoringItem, allDocData, info.FieldsInfo.Fields, info.OriginalVersion);
                        info.Level = originalLevel;
                    }
                    else
                    {
                        string msg = string.Format("Skip to restore the historical version of the folder because of lack of permission. Folder Url:{0}", aveItem.Folder.ServerRelativeUrl);
                        logger.Log(AveLogLevel.WARN, msg);
                        throw new AveWrapperSkipException(msg);
                    }
                }
                else if (info.OriginalVersion == info.Version)
                {
                    aveItem.UpdateFolderModerationStatus(info);
                    aveItem.SetReport(Report);
                    aveItem.UpdateFields(info.FieldsInfo.Fields, info, false, false);
                }
                else
                {
                    aveItem.CreateItemVersion(info.OriginalVersion, info.IsNewCreatedFolder);
                    aveItem.UpdateFolderModerationStatus(info);
                    aveItem.SetReport(Report);
                    aveItem.UpdateFields(info.FieldsInfo.Fields, info, false, false);
                    aveItem.ReInitBySPListItemLevel(aveItem.mSPListItem);
                }
                //放在最后统一更新
                //mSite.QueryService.UpdateAllDocsPropertyByNative(info, info.DTimeCreated, info.DTimeLastModified, info.OriginalVersion);

                aveItem.SetDocData("TimeCreated", info.DTimeCreated);
                aveItem.SetDocData("TimeLastModified", info.DTimeLastModified);

                //For Connector, Need to be added in the future;
                if (metaInfoDic != null)
                {
                    if (info.IsRestoreConnectorFolderProperties && metaInfoDic.ContainsKey("ConnectorFolderStubID") && info.IsNewCreated)
                    {
                        try
                        {
                            aveItem.mSPListItem.Properties["ConnectorFolderStubID"] = metaInfoDic["ConnectorFolderStubID"];
                            aveItem.mSPListItem.SystemUpdate(false);
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.ListItemUpdateError, e);
                        }
                    }
                    if (AvePoint.Common.AveEnv.IsMoss)
                    {
                        RestoreDocumentSet(aveItem, info, metaInfoDic);
                    }
                }

                info.RowId = aveItem.mSPListItem.ID;
                aveItem.RestoreFolderConnectorInfo();
                return AveRestoreResult.Normal;

            }


        }

        private static void RestoreDocumentSet(AveItem aveItem, AveFolderInfo folderInfo, Dictionary<string, string> metaInfoDic)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFolderCollection.RestoreDocumentSet"))
            {

                if (aveItem.ListItem.ContentType != null && AveSPDocumentSet.IsDocumentSet(aveItem.ListItem.ContentType.ID))
                {
                    try
                    {
                        object editor;
                        if (folderInfo.FieldsInfo.Fields.TryGetValue("tp_Editor", out editor))
                        {
                            if (editor is int)
                            {
                                aveItem.UpdateSpecialPropertyByNative(editor.ToString(), folderInfo.FieldsInfo.Fields["tp_Author"].ToString(), (DateTime)folderInfo.FieldsInfo.Fields["tp_Modified"], (DateTime)folderInfo.FieldsInfo.Fields["tp_Created"], folderInfo);
                            }
                            else
                            {
                                aveItem.UpdateSpecialPropertyByNative(((AveFieldValueInfo)editor).ColValue.ToString(), ((AveFieldValueInfo)folderInfo.FieldsInfo.Fields["tp_Author"]).ColValue.ToString(), (DateTime)((AveFieldValueInfo)folderInfo.FieldsInfo.Fields["tp_Modified"]).ColValue, (DateTime)((AveFieldValueInfo)folderInfo.FieldsInfo.Fields["tp_Created"]).ColValue, folderInfo);
                            }
                        }
                        else
                        {
                            editor = folderInfo.FieldsInfo.Fields["Editor"];
                            if (editor is int)
                            {
                                aveItem.UpdateSpecialPropertyByNative(folderInfo.FieldsInfo.Fields["Editor"].ToString(), folderInfo.FieldsInfo.Fields["Author"].ToString(), (DateTime)folderInfo.FieldsInfo.Fields["Modified"], (DateTime)folderInfo.FieldsInfo.Fields["Created"], folderInfo);
                            }
                            else
                            {
                                aveItem.UpdateSpecialPropertyByNative(((AveFieldValueInfo)editor).ColValue.ToString(), ((AveFieldValueInfo)folderInfo.FieldsInfo.Fields["Author"]).ColValue.ToString(), (DateTime)((AveFieldValueInfo)folderInfo.FieldsInfo.Fields["Modified"]).ColValue, (DateTime)((AveFieldValueInfo)folderInfo.FieldsInfo.Fields["Created"]).ColValue, folderInfo);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.RestoreDocSetError, e.ToString());
                    }
                }

            }

        }

        private bool CheckAndCreateFolder(AveFolderInfo info, AveItem aveItem)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFolderCollection.CheckAndCreateFolder"))
            {

                if (aveItem.Folder == null)
                {
                    SPContentTypeId itemContentTypeId = SPContentTypeId.Empty;
                    try
                    {
                        if (info.FieldsInfo.Fields.ContainsKey("ContentType"))
                        {
                            var ColValue = (info.FieldsInfo.Fields["ContentType"] as AveFieldValueInfo).ColValue;
                            if (ColValue.GetType().Equals(typeof(byte[])))
                            {
                                itemContentTypeId = new AveContentTypeId((byte[])ColValue).ContentTypeId;
                            }
                            else
                            {
                                itemContentTypeId = (ColValue as AveContentTypeId).ContentTypeId;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetContentTypeIdError, e.ToString());
                    }

                    IAveListItem item = null;
                    bool needUpdateList = false;
                    if (info.OriginalRowId > 0 && aveItem.mSPList != null)
                    {
                        bool isDocumentSet = AveSPDocumentSet.IsDocumentSet(new AveContentTypeId(itemContentTypeId));
                        if ((int)aveItem.mSPList.BaseTemplate == 2100)
                        {
                            try
                            {
                                //AveSPEventReceiverConfig.DisableEventReceiver();
                                item = aveItem.mList.AddItem(aveItem.mParentFolder.ServerRelativeUrl, AveFileSystemObjectType.Folder, info.Name);
                            }
                            finally
                            {
                                //AveSPEventReceiverConfig.EnableEventReceiver();
                            }
                        }
                        if (aveItem.mSPList.BaseTemplate == SPListTemplateType.DiscussionBoard)
                        {
                            SPListItem discussion = null;
                            if (string.Equals(aveItem.mAveParentFolder.ServerRelativeUrl, aveItem.mList.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                discussion = SPUtility.CreateNewDiscussion(aveItem.mList.List, info.Name);
                                if (discussion["ThreadIndex"] == null && info.UserData.ContainsKey("#tp_ThreadIndex"))
                                {
                                    discussion["ThreadIndex"] = info.UserData["#tp_ThreadIndex"];
                                    //discussion.SystemUpdate(false);
                                }
                            }
                            else
                            {
                                //SP2013 can create discussion under discussion.
                                discussion = aveItem.mList.List.AddItem(aveItem.mParentFolder.ServerRelativeUrl, SPFileSystemObjectType.Folder);
                                AveAssemblyUtility.InvokeStaticMethod(typeof(SPUtility), "PrepareNewDiscussionItem", new Type[] { typeof(SPListItem), typeof(string) }, new object[] { discussion, info.Name });
                            }
                            item = new AveListItem(aveItem.mList.Items as AveListItemCollection, discussion);
                        }
                        else
                        {
                            if (info.ModerationStatus == (int)SPModerationStatusType.Approved && aveItem.mList.EnableModeration)
                            {
                                aveItem.mList.EnableModeration = false;
                                needUpdateList = true;
                            }
                            else if (info.ModerationStatus != (int)SPModerationStatusType.Approved && !aveItem.mList.EnableModeration)
                            {
                                aveItem.mList.EnableModeration = true;
                                needUpdateList = true;
                            }
                            if (!isDocumentSet)
                            {
                                if (info.OriginalVersion < 512 && !aveItem.mSPList.EnableMinorVersions)
                                {
                                    aveItem.mSPList.EnableMinorVersions = true;
                                    needUpdateList = true;
                                }

                                if (needUpdateList)
                                {
                                    info.SettingInfo.LIST_SETTING_CHANGED = true;
                                    aveItem.mSPList.Update();
                                }
                                item = aveItem.mList.AddItem(aveItem.mParentFolder.ServerRelativeUrl, AveFileSystemObjectType.Folder, info.Name);
                                //item["Title"] = info.Name;
                                SPContentTypeId emptyContentTypeId = SPContentTypeId.Empty;
                                if (!SPContentTypeId.Equals(itemContentTypeId, emptyContentTypeId))
                                {
                                    item["ContentTypeId"] = itemContentTypeId;
                                }
                            }
                            else
                            {
                                //SP 16的DocumentSet比较特殊，
                                //如果开了大Version，那么创建出来的就是2.0,如果开了小Version，那么创建出来就是1.1，因此添加以下特殊处理
                                if (info.OriginalVersion == 512)
                                {
                                    if (aveItem.mSPList.EnableVersioning)
                                    {
                                        aveItem.mSPList.EnableVersioning = false;
                                        needUpdateList = true;
                                    }
                                }
                                else if (info.OriginalVersion < 1024 && !aveItem.mSPList.EnableMinorVersions)
                                {
                                    aveItem.mSPList.EnableMinorVersions = true;
                                    needUpdateList = true;
                                }

                                if (needUpdateList)
                                {
                                    info.SettingInfo.LIST_SETTING_CHANGED = true;
                                    aveItem.mSPList.Update();
                                }
                                //请不要随便添加reload操作
                                var docset = CreateDocumentSet(info, new AveContentTypeId(itemContentTypeId), aveItem.mSPList);
                                item = docset.ListItem;
                            }
                        }
                        if (info.UserData.ContainsKey("#tp_GUID"))
                        {
                            item["GUID"] = info.UserData["#tp_GUID"];
                            needUpdateGUIDByNative = false;
                        }
                        if (!isDocumentSet &&
                            mSite.QueryService.GetNextAvailableId(mSite.ID, item.ParentList.ID) != info.OriginalRowId &&
                            mSite.QueryService.CheckItemIdAvailable(mSite.ID, item.ParentList.ID, info.OriginalRowId))
                        {
                            // DocumentSet的rowId无法用API修改，如果使用MigrateItemId修改，会在SystemUpdate的时候抛异常。
                            MigrateItemId(item as AveListItem, info);
                        }
                        item.SystemUpdate(false);
                        // 创建folder之后调用 item.SystemUpdate(false);会导致list对象不一致
                        item.ParentList.Reload();
                        aveItem.Folder = item.Folder;
                    }
                    else
                    {
                        aveItem.Folder = this.Add(info.Name);
                        //TODO...Finde some way to add storage.
                        //AveStorage storge = AveStorage.GetStorage(mParentFolder);
                        //try
                        //{
                        //    if (mList != null && (int)mList.BaseTemplate == 2100)
                        //    {
                        //        AveSPEventReceiverConfig.DisableEventReceiver();
                        //    }
                        //    mSPFolder = storge.RestoreFolder(mParentFolder, mName);
                        //}
                        //finally
                        //{
                        //    if (mAveSPList != null && mAveSPList.SPList != null && (int)mAveSPList.SPList.BaseTemplate == 2100)
                        //    {
                        //        AveSPEventReceiverConfig.EnableEventReceiver();
                        //    }
                        //}
                        ////web下的system folder不能通过该比较，暂时先注释这两个条件.否则无法取item，会抛出异常。
                        ////!mSPFolder.Name.Equals("_PolicyCatalog", StringComparison.CurrentCultureIgnoreCase) && 
                        ////!mSPFolder.Name.Equals("_PolicyInternalData",StringComparison.CurrentCultureIgnoreCase))
                        if (!info.ParentListName.Equals("{System Folder}") || !info.ParentListIsSystem)
                        {
                            item = aveItem.Folder.Item;
                        }
                    }

                    try
                    {
                        info.GUID = aveItem.Folder.UniqueId;
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetFolderIdError, e.ToString());
                        info.GUID = mSite.QueryService.GetFolderIdByName(info);
                    }
                    info.IsNewCreatedFolder = true;
                    info.IsNewCreated = true;
                }

                //web下的system folder不能通过该比较，暂时先注释这两个条件.否则无法取item，会抛出异常。
                //mSPFolder.Name.Equals("_PolicyCatalog", StringComparison.CurrentCultureIgnoreCase) || mSPFolder.Name.Equals("images",StringComparison.CurrentCultureIgnoreCase)
                //|| mSPFolder.Name.Equals("_PolicyInternalData",StringComparison.CurrentCultureIgnoreCase))

                if (info.ParentListName == "{System Folder}" && info.ParentListIsSystem)
                {
                    throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                }

                /*
                 * 由于访问External List里面的root folder下第一个sub folder的Item对象时，会出现Access Denied的错误，但是第二次取没有问题。
                 * 目前没有找到好的方法来避免这个错误，所以先试取下，然后再走以前的还原逻辑。
                 */
                if (aveItem.Folder != null)
                {
                    try
                    {
                        aveItem.Folder.ParentList = aveItem.mList;
                        IAveListItem listItem = aveItem.Folder.Item;
                    }
                    catch (Exception ex)
                    {
                        logger.Log(AveLogLevel.WARN, ServerAPIResource.ListItemGetFailed, aveItem.Folder.ServerRelativeUrl, ex);
                    }
                }
                if (aveItem.Folder != null && aveItem.Folder.Item != null)
                {
                    return true;
                }
                return false;

            }

        }

        private void MigrateItemId(AveListItem aveItem, AveFolderInfo info)
        {
            try
            {
                AveAssemblyUtility.InvokeMethod(aveItem.ListItem, "SetIDForMigration", new Type[] { typeof(int) }, new object[] { info.OriginalRowId });
            }
            catch (Exception e)
            {
                logger.Log(AveLogLevel.DEBUG, ServerAPIResource.UpdateItemError, e);
                throw;
            }
            mSite.QueryService.ChangeNextItemId(info.OriginalRowId, mSite.ID, aveItem.ParentList.ID);//SetIDForMigration not auto change nexid.
        }

        private void PreRestoreFolder(AveFolderInfo info, AveItem aveItem, Dictionary<string, object> allDocData)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveFolderCollection.PreRestoreFolder"))
            {

                aveItem.CheckConflictState(info.RestoringItem, info.SiteId, info.ParentId);
                if (info.RestoringItem.ConflilctFromRecycleBin)
                {
                    //只有在Check for Conflicts in Destination Recycle Bin选择yes，冲突处理选择skip的条件下不会清空回收站
                    if (!(info.RestoringItem.IsIncludingRecycleBinData && info.RestoreOption == AveRestoreMode.Default))
                    {
                        mSite.QueryService.RemoveItemInRecycleBin(mSite, info.ParentId, info.Name);
                    }
                }
                SPFolder tempFolder = aveItem.GetFolder(aveItem.mParentFolder, info);

                if (tempFolder != null)
                {
                    aveItem.Folder = new AveFolder(mWeb, tempFolder);
                    aveItem.Folder.ParentList = aveItem.mList;
                }

                if (aveItem.mSPList != null && aveItem.mSPList.BaseTemplate == SPListTemplateType.ExternalList)
                {
                    throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                }

                if (aveItem.Folder != null && !info.RestoringItem.IsNewItem && aveItem.mList != null)
                {
                    try
                    {
                        var listitem = aveItem.mList.GetItemByUniqueId(aveItem.info.GUID);
                        if (listitem != null)
                        {
                            aveItem.InitBySPListItem(listitem);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetListIemFaild, e.ToString());
                    }
                    throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                }

                info.RestoringItem.TargetTable = info.RestoringItem.GetTargetTable(info.OriginalVersion, info.IsVersion);
                if (info.RestoringItem.TargetTable == RestoreTargetTable.None)
                {
                    if (info.RestoringItem.SkipRecycleBinData)
                    {
                        throw new AveRestoreException(AveRestoreResult.SkipRecycleBinData, string.Empty);
                    }
                    //only restore security，需要初始化AveItem，否则不走Restore RoleAssignment。
                    //这个和上面的代码有重复的，需要重构，上面的IsNewItem始终是True，永远走不进去。
                    if (aveItem.Folder != null && aveItem.mList != null)
                    {
                        try
                        {
                            var listitem = aveItem.mList.GetItemByUniqueId(aveItem.info.GUID);
                            if (listitem != null)
                            {
                                aveItem.InitBySPListItem(listitem);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Log(AveLogLevel.DEBUG, ServerAPIResource.GetListIemFaild, e.ToString());
                        }
                    }
                    throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                }

            }

        }

        public void Dispose()
        {
            if (mReport != null)
                mReport.Dispose();
        }
    }
}
