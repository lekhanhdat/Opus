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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.Client;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Wrapper.Common;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AvePoint.GCommon.Utility;
using AvePoint.ObjectModel.Common.Cache;
using AvePoint.RA.Contract.Tenant;

namespace AvePoint.ObjectModel.Common
{
    [AveCodeReviewAttribute("2012/03/09", "Navy.Li@avepoint.com", "Bingkun.Wang@AvePoint.com",
        new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_1, 
                       CodeReviewConstants.CHECK_LIST_ID_CO_6, 
                       CodeReviewConstants.CHECK_LIST_ID_FA_4 }, null, true)]
    class AveList : AveSecurableObject, IAveList
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveList));

        private AveWeb mParentWeb;
        private IAveRequest mRequest;
        private Dictionary<string, string> mNeedLoadFields;
        private readonly object mLoadFieldLock = new object();
        private bool? mIsExceedListViewLookupThreshold;
        private readonly object mIsExceedListViewLookupThresholdLock = new object();
        private static HashSet<Guid> BuiltInLookupColumn = new HashSet<Guid>();
        public static List<string> IgnoreFields = new List<string>();
        public static List<string> ItemBuildInField = new List<string>();
        public static int DefaultLCID = 1033;
        private List<string> m_NeedSetNullFields;
        private ItemIdMapping mItemIdMapping = null;
        private readonly object loadListItemGuidAndRowIdMappingLock = new object();
        public object mItemRestoreLock = new object();
        private readonly object mUpdateTaxonomyFieldLock = new object();
        private IAveEventReceiverDefinitionCollection mEventReceiverDefinitionCol = null;
        private readonly object eventReceiverLock = new object();
        public bool? mOverWrite = null;
        private static List<string> list;
        private IAveSecurableObjectImpl mSecurableObjectImpl;
        #region add to keep item's LastModifiedTime property


        private readonly object privateLock = new object();
        private AveUserResource titleResource;
        private AveUserResource descriptionResource;
        #endregion
        private readonly object mSpotlightInfoLock = new object();
        private string mSpotlightInfoMappingStr;
        private readonly object mGetRootFolderLock = new object();

        internal IAveRequest Request
        {
            get
            {
                return mRequest;
            }
        }

        private Dictionary<string, AveListItemConflictBaseInfo> _fileCollection = new Dictionary<string, AveListItemConflictBaseInfo>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, AveListItemConflictBaseInfo> _foldersCollection = new Dictionary<string, AveListItemConflictBaseInfo>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<Guid, AveListItemConflictBaseInfo> _itemUniqueIDMapping = new Dictionary<Guid, AveListItemConflictBaseInfo>();
        

        private bool _itemMappingInitialized = false;

        #region sqlite cache for conflict check
        private bool NeedSqliteDB4Cache = false;
        private string JobIdForSqliteCache = null;
        private int AveListSqliteCacheTypes = 0;
        private AveListSqliteCache _sqliteCache;
        private string _sqliteCacheErrorMessage;
        #endregion

        private void ClearContentCollection()
        {
            this._fileCollection.Clear();
            this._foldersCollection.Clear();
            this._itemUniqueIDMapping.Clear();
        }

        public Dictionary<string, AveListItemConflictBaseInfo> GetItemsForConflict(AveCamlQuery camlQuery)
        {
            Dictionary<string, AveListItemConflictBaseInfo> listItemsCollection = mRequest.GetItemsForConflict(mParentWeb.ServerRelativeUrl, mParentWeb.Site.ID, mParentWeb.ID, Title, ID, camlQuery.ToStringArray());
            return listItemsCollection;
        }

        public List<AveListItemConflictBaseInfo> GetItems()
        {
            //List<SPOItemInfo> items = mRequest.GetItems(webFullUrl, list, camlQuery);
            AveCamlQuery query = AveCamlQuery.CreateAllItemsQuery(2000, new string[10] { "ID", "Created", "Modified", "FileRef", "_Level", "GUID", "FSObjType", "ParentUniqueId", "Author", "Editor" });
            var aveListItemCollection = this.GetItemsForConflict(query);

            if (aveListItemCollection == null)
            {
                mLogger.Warn($"Failed to load items for confliction check. listTitle:{Title}");
                return new List<AveListItemConflictBaseInfo>();
            }

            mLogger.Info($"Finish to load items for confliction check. listTitle:{Title}, itemCount:{aveListItemCollection.Count}");

            if (aveListItemCollection.Count > 0 && aveListItemCollection.Values.First().Modified.Kind != DateTimeKind.Utc)
            {
                IAveTimeZone webTimeZone = ParentWeb.RegionalSettings.TimeZone;
                var timeZone = AveTimeZoneUtility.ToTimeZoneInfo(webTimeZone);
                mLogger.Info("Time zone {0}-{1}.", timeZone.Id, timeZone.DisplayName);
                var aveWebTimeZoneDescription = webTimeZone.Description;
                mLogger.Info("Load web time zone successfully,time zone:{0}", aveWebTimeZoneDescription);
                if (!string.IsNullOrEmpty(aveWebTimeZoneDescription))
                {
                    foreach (var listItem in aveListItemCollection)
                    {
                        listItem.Value.Modified = DateTime.SpecifyKind(listItem.Value.Modified, DateTimeKind.Unspecified);
                        listItem.Value.Modified = TimeZoneInfo.ConvertTimeToUtc(listItem.Value.Modified, timeZone);
                        listItem.Value.TimeCreated = DateTime.SpecifyKind(listItem.Value.TimeCreated, DateTimeKind.Unspecified);
                        listItem.Value.TimeCreated = TimeZoneInfo.ConvertTimeToUtc(listItem.Value.TimeCreated, timeZone);
                    }
                }
            }
            return aveListItemCollection.Values.ToList();
        }

        private void InitializeItemMappingList()
        {
            ClearContentCollection();

            try
            {
                var items = GetItems();

                foreach (AveListItemConflictBaseInfo item in items)
                {
                    string key = item.ServerRelativeUrl.Replace(this.ParentWeb.ServerRelativeUrl, "").TrimStart('/');
                    if (item.ObjectType == AveFileSystemObjectType.File)
                    {
                        if (!this._fileCollection.ContainsKey(key))
                        {
                            this._fileCollection.Add(key, item);
                        }
                    }
                    else
                    {
                        if (!this._foldersCollection.ContainsKey(key))
                        {
                            this._foldersCollection.Add(key, item);
                        }
                    }

                    this._itemUniqueIDMapping[item.UniqueId] = item;
                }
            }
            catch (Exception e)
            {
                mLogger.Error($"An error occurred while initializing item ID mapping list. ex:{e}");
            }
            _itemMappingInitialized = true;
        }

        public Dictionary<string, AveListItemConflictBaseInfo> FileCollection
        {
            get
            {
                if (!_itemMappingInitialized)
                {
                    // OOM issue
                    if (NeedSqliteDB4Cache && AveListSqliteCacheTypes > 0)
                    {
                        mLogger.Warn($"FileCollection is not initialized yet, but NeedSqliteDB4Cache is true and AveListSqliteCacheTypes is {AveListSqliteCacheTypes}. Need using the _sqliteCache instead.listTitle:{Title}");
                    }
                    else
                    {
                        InitializeItemMappingList();
                    }
                }
                return this._fileCollection;
            }
        }

        public void InitializeSqliteCache()
        {
            try
            {
                string tenantGroupId = TenantLocalValue.LogonGroupId;
                string jobId = JobIdForSqliteCache;
                string listId = this.ID.ToString();

                _sqliteCache = new AveListSqliteCache(tenantGroupId, jobId, listId, AveListSqliteCacheTypes);
                AveCamlQuery query = AveCamlQuery.CreateAllItemsQuery(2000,
                [
                    "ID", "Created", "Modified", "FileRef", "_Level",
                    "GUID", "FSObjType", "ParentUniqueId", "Author", "Editor"
                ]);

                string[] camlQueryNode = query.ToStringArray();
                int batchCount = 0;

                foreach (var batch in mRequest.GetItemsForConflictByBatch(
                    mParentWeb.ServerRelativeUrl,
                    mParentWeb.Site.ID,
                    mParentWeb.ID,
                    Title,
                    ID,
                    camlQueryNode,
                    2000))
                {
                    if (batch == null || batch.Count == 0) continue;

                    //ConvertBatchTimeZone(batch); // same with GetItems but currently no need
                    _sqliteCache.InsertValueToDB(batch, AveListCacheType.FileCollection);
                    batchCount += batch.Count;
                    mLogger.Info($"InitializeSqliteCache inserted batch to sqlite. listTitle:{Title}, batchItemCount:{batch.Count}, totalInserted:{batchCount}");
                }

                mLogger.Info($"InitializeSqliteCache completed. listTitle:{Title}, totalItemCount:{batchCount}");
            }
            catch (Exception e)
            {
                mLogger.Error($"An error occurred while initializing sqlite cache. listTitle:{Title}, ex:{e}");
                _sqliteCacheErrorMessage = e.Message;
                throw;
            }
            finally
            {
                _itemMappingInitialized = true;
            }
        }

        public bool TryGetCachedListItem(string fileRelativeUrl, out AveListItemConflictBaseInfo fileInfo)
        {
            if (!_itemMappingInitialized)
            {
                if (NeedSqliteDB4Cache && AveListSqliteCacheTypes > 0)
                    InitializeSqliteCache();
                else
                    InitializeItemMappingList(); // use in-memory collection as a fallback

                mLogger.Info($"List item sqlite cache is initialized. listTitle:{Title}, NeedSqliteDB4Cache:{NeedSqliteDB4Cache}, AveListSqliteCacheTypes:{AveListSqliteCacheTypes}");
            }

            if (!string.IsNullOrEmpty(_sqliteCacheErrorMessage))
            {
                throw new Exception(_sqliteCacheErrorMessage);
            }

            if (NeedSqliteDB4Cache && _sqliteCache != null)
                return _sqliteCache.TryGetCachedFile(fileRelativeUrl, out fileInfo);

            if (_fileCollection.TryGetValue(fileRelativeUrl, out fileInfo))
            {
                return true;
            }

            return false;
        }

        public Dictionary<string, AveListItemConflictBaseInfo> FoldersCollection
        {
            get
            {
                if (!_itemMappingInitialized)
                {
                    InitializeItemMappingList();
                }
                return this._foldersCollection;
            }
        }

        public Dictionary<Guid, AveListItemConflictBaseInfo> UniqueIDMapping
        {
            get
            {
                if (!_itemMappingInitialized)
                {
                    InitializeItemMappingList();
                }
                return this._itemUniqueIDMapping;
            }
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint Property")]
        static AveList()
        {
            //IgnoreFields.Add("Body");
            //IgnoreFields.Add("PostCategory");
            //IgnoreFields.Add("PublishedDate");
            //IgnoreFields.Add("Keywords");
            IgnoreFields.Add("ImageWidth");
            //IgnoreFields.Add("_Comments");
            //IgnoreFields.Add("AlternateThumbnailUrl");
            //IgnoreFields.Add("wic_System_Copyright");
            IgnoreFields.Add("MediaLengthInSeconds");
            IgnoreFields.Add("Modified");
            IgnoreFields.Add("_CopySource");
            IgnoreFields.Add("CheckoutUser");
            //IgnoreFields.Add("HTML_x0020_File_x0020_Type");
            IgnoreFields.Add("_SourceUrl");
            IgnoreFields.Add("_SharedFileIndex");
            IgnoreFields.Add("TemplateUrl");
            IgnoreFields.Add("xd_ProgID");
            IgnoreFields.Add("xd_Signature");
            IgnoreFields.Add("_HasCopyDestinations");
            IgnoreFields.Add("owshiddenversion");
            IgnoreFields.Add("InstanceID");
            //IgnoreFields.Add("Order");
            IgnoreFields.Add("WorkflowVersion");
            IgnoreFields.Add("WorkflowInstanceID");
            //keep document id, initialized by target SharePoint Service. 
            //IgnoreFields.Add("_dlc_DocIdUrl");
            //IgnoreFields.Add("_dlc_DocId");
            IgnoreFields.Add("dlc_DocIdPersistId");
            IgnoreFields.Add("NumComments");

            IgnoreFields.Add("ID");  // 这些field是不区分version的
            IgnoreFields.Add("GUID");
            IgnoreFields.Add("File_x0020_Type");
            IgnoreFields.Add("Editor"); //get versions方法得到的attribute里面默认就有
            IgnoreFields.Add("Modified_x0020_By");  //还原的时候不用，Editor优先
            //IgnoreFields.Add("_ModerationStatus"); //目前DAO 不支持还原moderation status
            //IgnoreFields.Add("_ModerationComments");

            ItemBuildInField.Add("ID");   //get user data的时候可以直接通过item的属性获取而不需要从version获取
            ItemBuildInField.Add("GUID");
            ItemBuildInField.Add("File_x0020_Type");

            BuiltInLookupColumn.Add(new Guid("1982e408-0f94-4149-8349-16f301d89134"));  // InternalName:PreviouslyAssignedTo
            BuiltInLookupColumn.Add(new Guid("3881510a-4e4a-4ee8-b102-8ee8e2d0dd4b"));  // InternalName:CheckoutUser
            BuiltInLookupColumn.Add(new Guid("94f89715-e097-4e8b-ba79-ea02aa8b7adb"));  // InternalName:FileRef
            BuiltInLookupColumn.Add(new Guid("7111aa1b-e7ae-4b69-acaf-db669b76e03a"));  // InternalName:V4CallTo
            BuiltInLookupColumn.Add(new Guid("c5c4b81c-f1d9-4b43-a6a2-090df32ebb68"));  // InternalName:ProgId
            BuiltInLookupColumn.Add(new Guid("960ff01f-2b6d-4f1b-9c3f-e19ad8927341"));  // InternalName:FolderChildCount
            BuiltInLookupColumn.Add(new Guid("dddd2420-b270-4735-93b5-92b713d0944d"));  // InternalName:ScopeId
            BuiltInLookupColumn.Add(new Guid("6bfaba20-36bf-44b5-a1b2-eb6346d49716"));  // InternalName:AppAuthor
            BuiltInLookupColumn.Add(new Guid("875fab27-6e95-463b-a4a6-82544f1027fb"));  // InternalName:RelatedIssues
            BuiltInLookupColumn.Add(new Guid("53101f38-dd2e-458c-b245-0c236cc13d1a"));  // InternalName:AssignedTo
            BuiltInLookupColumn.Add(new Guid("774eab3a-855f-4a34-99da-69dc21043bec"));  // InternalName:ParentLeafName
            BuiltInLookupColumn.Add(new Guid("38bea83b-350a-1a6e-f34a-93a6af31338b"));  // InternalName:PostCategory
            BuiltInLookupColumn.Add(new Guid("30bb605f-5bae-48fe-b4e3-1f81d9772af9"));  // InternalName:FSObjType
            BuiltInLookupColumn.Add(new Guid("58014f77-5463-437b-ab67-eec79532da67"));  // InternalName:_CheckinComment
            BuiltInLookupColumn.Add(new Guid("b4fa187b-eb65-478e-8bc6-93b0da320f03"));  // InternalName:ResolvedBy
            BuiltInLookupColumn.Add(new Guid("b824e17e-a1b3-426e-aecf-f0184d900485"));  // InternalName:ItemChildCount
            BuiltInLookupColumn.Add(new Guid("7f15088c-1448-41c7-a125-18a3a90ce543"));  // InternalName:LastReplyBy
            BuiltInLookupColumn.Add(new Guid("50d8f08c-8e99-4948-97bf-2be41fa34a0d"));  // InternalName:TaskGroup
            BuiltInLookupColumn.Add(new Guid("687c7f94-686a-42d3-9b67-2782eac4b4f8"));  // InternalName:MetaInfo
            BuiltInLookupColumn.Add(new Guid("c3a92d97-2b77-4a25-9698-3ab54874bc6f"));  // InternalName:Predecessors
            BuiltInLookupColumn.Add(new Guid("f0218b98-d0d6-4fc1-b15b-aabeb89f32a9"));  // InternalName:DiscussionTitleLookup
            BuiltInLookupColumn.Add(new Guid("e0f298a5-7e3e-4895-9ff8-90d88ec4526d"));  // InternalName:V4SendTo
            BuiltInLookupColumn.Add(new Guid("8137f7ad-9170-4c1d-a17b-4ca7f557bc88"));  // InternalName:ParticipantsPicker
            BuiltInLookupColumn.Add(new Guid("fd447db5-3908-4b47-8f8c-a5895ed0aa6a"));  // InternalName:ParentID
            BuiltInLookupColumn.Add(new Guid("4a389cb9-54dd-4287-a71a-90ff362028bc"));  // InternalName:VirusStatus
            BuiltInLookupColumn.Add(new Guid("078b9dba-eb8c-4ec5-bfdd-8d220a3fcc5d"));  // InternalName:MyEditor
            BuiltInLookupColumn.Add(new Guid("8fca95c0-9b7d-456f-8dae-b41ee2728b85"));  // InternalName:File_x0020_Size
            BuiltInLookupColumn.Add(new Guid("173f76c8-aebd-446a-9bc9-769a2bd2c18f"));  // InternalName:Last_x0020_Modified
            BuiltInLookupColumn.Add(new Guid("ff90fecb-7f46-44f5-9698-db44a81b2a8b"));  // InternalName:ParentItemEditor
            BuiltInLookupColumn.Add(new Guid("4b7403de-8d94-43e8-9f0f-137a3e298126"));  // InternalName:UniqueId
            BuiltInLookupColumn.Add(new Guid("a4e7b3e1-1b0a-4ffa-8426-c94d4cb8cc57"));  // InternalName:Facilities
            BuiltInLookupColumn.Add(new Guid("e08400f3-c779-4ed2-a18c-ab7f34caa318"));  // InternalName:AppEditor
            BuiltInLookupColumn.Add(new Guid("56605df6-8fa1-47e4-a04c-5b384d59609f"));  // InternalName:FileDirRef
            BuiltInLookupColumn.Add(new Guid("423874f8-c300-4bfb-b7a1-42e2159e3b19"));  // InternalName:SortBehavior
            BuiltInLookupColumn.Add(new Guid("bc1a8efb-0f4c-49f8-a38f-7fe22af3d3e0"));  // InternalName:ParentVersionString
            BuiltInLookupColumn.Add(new Guid("211a8cfc-93b7-4173-9254-0bfe2d1643da"));  // InternalName:UserName
            BuiltInLookupColumn.Add(new Guid("8ffccefe-998b-4896-a6df-32d566f69141"));  // InternalName:ShortestThreadIndexIdLookup
            BuiltInLookupColumn.Add(new Guid("998b5cff-4a35-47a7-92f3-3914aa6aa4a2"));  // InternalName:Created_x0020_Date
            BuiltInLookupColumn.Add(new Guid("4d64b067-08c3-43dc-a87b-8b8e01673313"));  // InternalName:RatedBy

            BuiltInLookupColumn.Add(new Guid("94f89715-e097-4e8b-ba79-ea02aa8b7adb")); //URL Path
            BuiltInLookupColumn.Add(new Guid("56605df6-8fa1-47e4-a04c-5b384d59609f")); // Path
            BuiltInLookupColumn.Add(new Guid("173f76c8-aebd-446a-9bc9-769a2bd2c18f")); //modified
            BuiltInLookupColumn.Add(new Guid("998b5cff-4a35-47a7-92f3-3914aa6aa4a2")); //created
            BuiltInLookupColumn.Add(new Guid("8fca95c0-9b7d-456f-8dae-b41ee2728b85")); //file Size
            BuiltInLookupColumn.Add(new Guid("30bb605f-5bae-48fe-b4e3-1f81d9772af9")); //item Type
            BuiltInLookupColumn.Add(new Guid("423874f8-c300-4bfb-b7a1-42e2159e3b19")); //sort type
            BuiltInLookupColumn.Add(new Guid("4b7403de-8d94-43e8-9f0f-137a3e298126")); // unique id
            BuiltInLookupColumn.Add(new Guid("c5c4b81c-f1d9-4b43-a6a2-090df32ebb68")); // progid
            BuiltInLookupColumn.Add(new Guid("dddd2420-b270-4735-93b5-92b713d0944d")); // scope id
            BuiltInLookupColumn.Add(new Guid("4a389cb9-54dd-4287-a71a-90ff362028bc")); // virus status
            BuiltInLookupColumn.Add(new Guid("687c7f94-686a-42d3-9b67-2782eac4b4f8")); // property bag
            BuiltInLookupColumn.Add(new Guid("94f89715-e097-4e8b-ba79-ea02aa8b7adb")); //URL Path
            BuiltInLookupColumn.Add(new Guid("56605df6-8fa1-47e4-a04c-5b384d59609f")); // Path
            BuiltInLookupColumn.Add(new Guid("173f76c8-aebd-446a-9bc9-769a2bd2c18f")); //modified
            BuiltInLookupColumn.Add(new Guid("998b5cff-4a35-47a7-92f3-3914aa6aa4a2")); //created
            BuiltInLookupColumn.Add(new Guid("8fca95c0-9b7d-456f-8dae-b41ee2728b85")); //file Size
            BuiltInLookupColumn.Add(new Guid("30bb605f-5bae-48fe-b4e3-1f81d9772af9")); //item Type
            BuiltInLookupColumn.Add(new Guid("423874f8-c300-4bfb-b7a1-42e2159e3b19")); //sort type
            BuiltInLookupColumn.Add(new Guid("4b7403de-8d94-43e8-9f0f-137a3e298126")); // unique id
            BuiltInLookupColumn.Add(new Guid("c5c4b81c-f1d9-4b43-a6a2-090df32ebb68")); // progid
            BuiltInLookupColumn.Add(new Guid("dddd2420-b270-4735-93b5-92b713d0944d")); // scope id
            BuiltInLookupColumn.Add(new Guid("4a389cb9-54dd-4287-a71a-90ff362028bc")); // virus status
            BuiltInLookupColumn.Add(new Guid("687c7f94-686a-42d3-9b67-2782eac4b4f8")); // property bag
            BuiltInLookupColumn.Add(new Guid("94f89715-e097-4e8b-ba79-ea02aa8b7adb")); //URL Path
            BuiltInLookupColumn.Add(new Guid("56605df6-8fa1-47e4-a04c-5b384d59609f")); // Path
            BuiltInLookupColumn.Add(new Guid("173f76c8-aebd-446a-9bc9-769a2bd2c18f")); //modified
            BuiltInLookupColumn.Add(new Guid("998b5cff-4a35-47a7-92f3-3914aa6aa4a2")); //created
            BuiltInLookupColumn.Add(new Guid("8fca95c0-9b7d-456f-8dae-b41ee2728b85")); //file Size
            BuiltInLookupColumn.Add(new Guid("30bb605f-5bae-48fe-b4e3-1f81d9772af9")); //item Type
            BuiltInLookupColumn.Add(new Guid("423874f8-c300-4bfb-b7a1-42e2159e3b19")); //sort type
            BuiltInLookupColumn.Add(new Guid("4b7403de-8d94-43e8-9f0f-137a3e298126")); // unique id
            BuiltInLookupColumn.Add(new Guid("c5c4b81c-f1d9-4b43-a6a2-090df32ebb68")); // progid
            BuiltInLookupColumn.Add(new Guid("dddd2420-b270-4735-93b5-92b713d0944d")); // scope id
            BuiltInLookupColumn.Add(new Guid("4a389cb9-54dd-4287-a71a-90ff362028bc")); // virus status
            BuiltInLookupColumn.Add(new Guid("687c7f94-686a-42d3-9b67-2782eac4b4f8")); // property bag

            BuiltInLookupColumn.Add(new Guid("a7b731a3-1df1-4d74-a5c6-e2efba617ae2")); // CheckedOutUserId
            BuiltInLookupColumn.Add(new Guid("cfaabd0f-bdbd-4bc2-b375-1e779e2cad08")); // IsCheckedoutToLocal
            BuiltInLookupColumn.Add(new Guid("6d2c4fde-3605-428e-a236-ce5f3dc2b4d4")); // SyncClientId
            BuiltInLookupColumn.Add(new Guid("9d4adc35-7cc8-498c-8424-ee5fd541e43a")); // CheckedOutTitle
            BuiltInLookupColumn.Add(new Guid("8e69e8e8-df8a-45dc-858a-1b806dde24c0")); // DocConcurrencyNumber
            BuiltInLookupColumn.Add(new Guid("3b653cee-df6b-4cd4-b66d-ad5ce875b25e")); // ParentUniqueId
            BuiltInLookupColumn.Add(new Guid("692b7133-d1d1-4a01-b604-2987724f86c3")); // StreamHash
        }

        private static List<string> StyleLibrary
        {
            get
            {
                if (list == null)
                {
                    #region initialize list
                    list = new List<string>();
                    list.Add("Style Library,101".ToUpper());
                    //German
                    list.Add("Formatbibliothek,101".ToUpper()); //Style Library
                    //French
                    list.Add("Bibliothèque de styles,101".ToUpper()); //Style Library
                    //Japanese
                    list.Add("スタイル ライブラリ,101".ToUpper());  //Style Library
                    #endregion
                }
                return list;
            }
        }
        public AveList(IAveRequest request, AveWeb web, IDictionary<string, object> listProp)
            : base(request)
        {
            base.DataCache = new AveClientConcurrentObjectData();
            mRequest = request;
            mParentWeb = web;
            listProp["ParentWeb"] = web;
            base.DataCache.AddPropertyies(listProp);
            InitializeIgnoreFieldsByListTemplate();
        }

        private void InitializeIgnoreFieldsByListTemplate()
        {
            switch ((int)this.BaseTemplate)
            {
                case 500:
                case (int)AveListTemplateType.Categories:
                    IgnoreFields.Add("TopicCount");
                    IgnoreFields.Add("ReplyCount");
                    break;
                case (int)AveListTemplateType.CommunityMember:
                    IgnoreFields.Add("NumberOfDiscussions");
                    IgnoreFields.Add("NumberOfReplies");
                    IgnoreFields.Add("ReputationScore");
                    break;
                default:
                    break;
            }
        }

        internal override void InitRoleAssignmentProperties(Dictionary<string, object> roleAssignmentProperties)
        {
            roleAssignmentProperties[AveObjectModelConstant.WebServerRelativeUrl] = mParentWeb.ServerRelativeUrl;
            roleAssignmentProperties[AveObjectModelConstant.ListTitle] = this.Title;
            roleAssignmentProperties[AveObjectModelConstant.ListId] = this.ID;
        }

        internal override Dictionary<string, object> AddRoleAssignment(Dictionary<string, object> roleAssignmentProperties)
        {
            return mRequest.AddRoleAssignment(mParentWeb.ServerRelativeUrl, this.DefaultViewUrl, this.Title, this.ID, -1, roleAssignmentProperties, "list.roleAssignments");
        }

        internal override Dictionary<string, object> UpdateRoleAssignment(int principalId, Dictionary<string, object> roleAssignmentProperties)
        {
            return mRequest.UpdateRoleAssignment(mParentWeb.ServerRelativeUrl, this.DefaultViewUrl, this.Title, this.ID, -1, principalId, roleAssignmentProperties, "list.roleAssignments");
        }

        #region Members

        internal Dictionary<string, string> NeedLoadFields
        {
            get
            {
                if (mNeedLoadFields == null)
                {
                    lock (mLoadFieldLock)
                    {
                        if (mNeedLoadFields == null)
                        {
                            Dictionary<string, string> needLoadFields = new Dictionary<string, string>();
                            IAveField tempField;
                            if ((tempField = this.Fields.GetFieldById(AveBuiltInFieldId.Author, false)) != null)
                            {
                                needLoadFields.Add(tempField.InternalName, tempField.TypeAsString);
                            }
                            foreach (AveField aveField in this.Fields)
                            {
                                if (!(string.IsNullOrEmpty(aveField.ColName) || AveList.IgnoreFields.Contains(aveField.InternalName)) && !aveField.InternalName.Equals("Created_x0020_By"))
                                {
                                    needLoadFields[aveField.InternalName] = aveField.TypeAsString;
                                }
                                else if (aveField.InternalName.Equals("_CheckinComment"))
                                {
                                    needLoadFields[aveField.InternalName] = aveField.TypeAsString;
                                }
                            }
                            mNeedLoadFields = needLoadFields;
                        }
                    }
                }
                return mNeedLoadFields;
            }
            set
            {
                this.mNeedLoadFields = value;
            }
        }

        public bool IsExceedListViewLookupThreshold
        {
            get
            {
                if (mIsExceedListViewLookupThreshold == null)
                {
                    lock (mIsExceedListViewLookupThresholdLock)
                    {
                        if (mIsExceedListViewLookupThreshold == null)
                        {
                            int lookupFieldCount = 0;
                            foreach (IAveField aveField in this.Fields)
                            {
                                if (BuiltInLookupColumn.Contains(aveField.ID))
                                {
                                    continue;
                                }
                                IAveFieldLookup lookupField = aveField as IAveFieldLookup;
                                if ((lookupField != null && !lookupField.IsDependentLookup)
                                    || aveField.Type == AveFieldType.WorkflowStatus)
                                {
                                    lookupFieldCount++;
                                }
                            }
                            mIsExceedListViewLookupThreshold = lookupFieldCount >= 8;
                        }
                    }
                }
                return mIsExceedListViewLookupThreshold.Value;
            }
        }

        public Dictionary<string, int> ListItemGuidAndRowIdMappings
        {
            get
            {
                lock (loadListItemGuidAndRowIdMappingLock)
                {
                    if (mItemIdMapping == null)
                    {
                        mItemIdMapping = mRequest.GetListItemGuidAndRowIdMappingsInLargeList(this.ParentWebUrl, this.RootFolder.ServerRelativeUrl, this.Title, this.ID);
                    }
                    return mItemIdMapping.IdMapping;
                }
            }
        }

        public Dictionary<string, int> ListAppendItemMappings
        {
            get
            {
                lock (loadListItemGuidAndRowIdMappingLock)
                {
                    if (mItemIdMapping == null)
                    {
                        mItemIdMapping = mRequest.GetListItemGuidAndRowIdMappingsInLargeList(this.ParentWebUrl, this.RootFolder.ServerRelativeUrl, this.Title, this.ID);
                    }
                    return mItemIdMapping.AppendItemMapping;
                }
            }
        }

        public bool HasAttachment
        {
            get
            {
                lock (loadListItemGuidAndRowIdMappingLock)
                {
                    if (mItemIdMapping == null)
                    {
                        mItemIdMapping = mRequest.GetListItemGuidAndRowIdMappingsInLargeList(this.ParentWebUrl, this.RootFolder.ServerRelativeUrl, this.Title, this.ID);
                    }
                    return mItemIdMapping.HasAttachment;
                }
            }
        }

        //用作判断special(wp，masterpage，Style Library)list中file是否需要使用API来备份。
        public bool IsSpecialList
        {
            get
            {
                List<AveListTemplateType> catelogs = new List<AveListTemplateType>() { AveListTemplateType.NoCodePublic, AveListTemplateType.WebPartCatalog, AveListTemplateType.MasterPageCatalog };
                if (catelogs.Contains(this.BaseTemplate))
                {
                    return true;
                }
                return StyleLibrary.Contains(this.RootFolder.Name.ToUpper() + "," + ((int)this.BaseTemplate).ToString());
            }
        }
        #endregion

        #region IAveList Members

        public bool AllowDeletion
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AllowDeletion"))
                {
                    try
                    {
                        string schema = this.SchemaXml;
                        if (!string.IsNullOrEmpty(schema))
                        {
                            XmlDocument xDoc = new XmlDocument();
                            xDoc.LoadXml(schema);
                            if (xDoc.DocumentElement.HasAttribute("AllowDeletion"))
                            {
                                base.DataCache.AddProperty("AllowDeletion", Convert.ToBoolean(xDoc.DocumentElement.GetAttribute("AllowDeletion")));
                                return base.DataCache.GetProperty<bool>("AllowDeletion");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error($"init AllowDeletion failed for {this.Title}, {ex}");
                    }
                    
                    base.DataCache.AddProperty("AllowDeletion",default(bool));
                }
                return base.DataCache.GetProperty<bool>("AllowDeletion");
            }
            set
            {
                if (!AllowDeletion.Equals(value))
                {
                    base.DataCache.AddChangedProperty("AllowDeletion", value);
                }
            }
        }

        public bool AllowRssFeeds
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("AllowRssFeeds"))
                {
                    Dictionary<string, object> rssProperties = mRequest.GetListRssProperties(this.ParentWebUrl, this.ID);
                    base.DataCache.AddPropertyies(rssProperties);
                    if (base.DataCache.IsPropertyAvailable("RootFolderRssProperties"))//SAAS-415,不然的话取不到
                    {
                        Hashtable properties = base.DataCache.GetProperty<Hashtable>("RootFolderRssProperties");
                        (this.RootFolder as AveFolder).DataCache.AddProperty("Properties",new AveCustomHashtable(properties, (this.RootFolder as AveFolder).SetChangeProperty));
                    }
                }
                return base.DataCache.GetProperty<bool>("AllowRssFeeds");
            }
        }

        public bool AllowMultiResponses
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowMultiResponses");
            }
            set
            {   //SAAS-8837 还原获取list的时候没有获取AllowMultiResponses属性，源端是false的情况下有影响
                //if (!AllowMultiResponses.Equals(value))
                //{
                base.DataCache.AddChangedProperty("AllowMultiResponses", value);
                //}
            }
        }

        public bool AllowContentTypes
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowContentTypes");
            }
        }

        public IAveUser Author
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Author"))
                {
                    string loginName = base.DataCache.GetProperty<string>("Author" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveUser author = this.ParentWeb.SiteUsers.GetByLoginName(loginName) as AveUser;
                    base.DataCache.AddProperty("Author",author);
                }
                return base.DataCache.GetProperty<IAveUser>("Author");
            }
        }

        public AveBasePermissions AnonymousPermMask64
        {
            get
            {
                return base.DataCache.GetProperty<AveBasePermissions>("AnonymousPermMask64");
            }
            set
            {
                base.DataCache.AddChangedProperty("AnonymousPermMask64", value);
            }
        }

        public AveListTemplateType BaseTemplate
        {
            get
            {
                return base.DataCache.GetProperty<AveListTemplateType>("BaseTemplate");
            }
        }

        public AveBaseType BaseType
        {
            get
            {
                return base.DataCache.GetProperty<AveBaseType>("BaseType");
            }
        }

        public DateTime Created
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("Created");
            }
        }

        private readonly object mLoadCTLock = new object();
        public IAveContentTypeCollection ContentTypes
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ContentTypes"))
                {
                    lock (mLoadCTLock)
                    {
                        if (base.DataCache.IsPropertyNotLoaded("ContentTypes"))
                        {
                            Dictionary<string, object> contentTypesProperties = mRequest.GetContentTypes(this.ParentWeb.ServerRelativeUrl, this.Title, this.ID, "list.contentTypes", AveUserResourceExtension.SupportedResourceCultureNames);
                            AveContentTypeCollection contentTypes = new AveContentTypeCollection(mRequest, this.ParentWeb, this, "list.contentTypes", contentTypesProperties);
                            base.DataCache.AddProperty("ContentTypes",contentTypes);
                        }
                    }
                }
                return base.DataCache.GetProperty<IAveContentTypeCollection>("ContentTypes");
            }
        }

        public bool ContentTypesEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ContentTypesEnabled");
            }
            set
            {
                if (!ContentTypesEnabled.Equals(value))
                {
                    base.DataCache.AddChangedProperty("ContentTypesEnabled", value);
                }
            }
        }

        public bool CrawlNonDefaultViews
        {
            get
            {
                return base.DataCache.GetProperty<bool>("CrawlNonDefaultViews");
            }
            set
            {
                if (!CrawlNonDefaultViews.Equals(value))
                {
                    base.DataCache.AddChangedProperty("CrawlNonDefaultViews", value);
                }
            }
        }

        public Guid DefaultContentApprovalWorkflowId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("DefaultContentApprovalWorkflowId");
            }
            set
            {
                if (!DefaultContentApprovalWorkflowId.Equals(value))
                {
                    base.DataCache.AddChangedProperty("DefaultContentApprovalWorkflowId", value);
                }
            }
        }

        public string DefaultDisplayFormUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("DefaultDisplayFormUrl");
            }
            set
            {
                if (!string.Equals(DefaultDisplayFormUrl, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("DefaultDisplayFormUrl", value);
                }
            }
        }

        public string DefaultEditFormUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("DefaultEditFormUrl");
            }
            set
            {
                if (!string.Equals(DefaultEditFormUrl, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("DefaultEditFormUrl", value);
                }
            }
        }

        public AveDefaultItemOpen DefaultItemOpen
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("DefaultItemOpen"))
                {
                    GetThisPropertie();
                }
                return base.DataCache.GetProperty<AveDefaultItemOpen>("DefaultItemOpen");
            }
            set
            {
                if (DefaultItemOpen != value)
                {
                    base.DataCache.AddChangedProperty("DefaultItemOpen", (int)value);
                }
            }
        }

        public AveListExperience ListExperience
        {
            get
            {
                return base.DataCache.GetProperty<AveListExperience>("ListExperienceOptions");
            }
            set
            {
                if (ListExperience != value)
                {
                    base.DataCache.AddChangedProperty("ListExperienceOptions", (int)value);
                }
            }
        }

        public bool DefaultItemOpenUseListSetting
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("DefaultItemOpenUseListSetting"))
                {
                    GetThisPropertie();
                }
                return base.DataCache.GetProperty<bool>("DefaultItemOpenUseListSetting");
            }
            set
            {
                if (!DefaultItemOpenUseListSetting.Equals(value))
                {
                    base.DataCache.AddChangedProperty("DefaultItemOpenUseListSetting", value);
                }
            }
        }

        public string DefaultNewFormUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("DefaultNewFormUrl");
            }
            set
            {
                if (!string.Equals(DefaultNewFormUrl, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("DefaultNewFormUrl", value);
                }
            }
        }

        public string DefaultViewUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("DefaultViewUrl");
            }
        }

        public string Description
        {
            get
            {
                return base.DataCache.GetProperty<string>("Description");
            }
            set
            {
                if (!string.Equals(Description, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("Description", value);
                }
            }
        }

        public IAveUserResource DescriptionResource
        {
            get
            {
                if (descriptionResource == null)
                {
                    descriptionResource = new AveListUserResource(this, AveUserResourceConstants.DESCRIPTION_RESOUCE, this.DataCache);
                }
                return descriptionResource;
            }
        }

        public AveDraftVisibilityType DraftVersionVisibility
        {
            get
            {
                return (AveDraftVisibilityType)base.DataCache.GetProperty<int>("DraftVersionVisibility");
            }
            set
            {
                if ((int)DraftVersionVisibility != (int)value
                    || base.DataCache.ChangedProperties.ContainsKey("EnableModeration")) //DraftVersionVisibility受EnableModeration控制
                {
                    base.DataCache.AddChangedProperty("DraftVersionVisibility", (int)value);
                }
            }
        }

        public string Direction
        {
            get
            {
                return base.DataCache.GetProperty<string>("Direction");
            }
            set
            {
                if (!string.Equals(Direction, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("Direction", value);
                }
            }
        }

        public bool DisableGridEditing
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("DisableGridEditing"))
                {
                    GetThisPropertie();
                }
                return base.DataCache.GetProperty<bool>("DisableGridEditing");
            }
            set
            {
                if (!DisableGridEditing.Equals(value))
                {
                    base.DataCache.AddChangedProperty("DisableGridEditing", value);
                }
            }
        }

        public string EmailAlias
        {
            get
            {
                return base.DataCache.GetProperty<string>("EmailAlias");
            }
            set
            {
                if (!string.Equals(EmailAlias, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("EmailAlias", value);
                }
            }
        }

        public bool EnableAssignToEmail
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableAssignToEmail");
            }
            set
            {
                if (!EnableAssignToEmail.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableAssignToEmail", value);
                }
            }
        }

        public bool EnableAttachments
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableAttachments");
            }
            set
            {
                if (!EnableAttachments.Equals(value) || EnableAttachments)
                {
                    base.DataCache.AddChangedProperty("EnableAttachments", value);
                }
            }
        }

        public bool EnforceDataValidation
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnforceDataValidation");
            }
            set
            {
                if (!EnforceDataValidation.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnforceDataValidation", value);
                }
            }
        }

        public bool EnableDeployingList
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableDeployingList");
            }
            set
            {
                if (!EnableDeployingList.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableDeployingList", value);
                }
            }
        }

        public bool EnableDeployWithDependentList
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableDeployWithDependentList");
            }
            set
            {
                if (!EnableDeployWithDependentList.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableDeployWithDependentList", value);
                }
            }
        }

        public bool EnableFolderCreation
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableFolderCreation");
            }
            set
            {
                if (!EnableFolderCreation.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableFolderCreation", value);
                }
            }
        }

        public bool EnableManagedIndexes
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("EnableManagedIndexes"))
                {
                    GetThisPropertie();
                }
                return base.DataCache.GetProperty<bool>("EnableManagedIndexes");
            }
            set
            {
                if (!EnableManagedIndexes.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableManagedIndexes", value);
                }
            }
        }

        public bool EnableMinorVersions
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableMinorVersions");
            }
            set
            {
                if (!EnableMinorVersions.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableMinorVersions", value);
                }
            }
        }

        public bool EnableModeration
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableModeration");
            }
            set
            {
                if (!EnableModeration.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableModeration", value);
                }
            }
        }

        public bool EnablePeopleSelector
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnablePeopleSelector");
            }
            set
            {
                base.DataCache.AddChangedProperty("EnablePeopleSelector", value);
            }
        }

        public bool EnableResourceSelector
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableResourceSelector");
            }
            set
            {
                if (!EnableResourceSelector.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableResourceSelector", value);
                }
            }
        }

        public bool EnableSchemaCaching
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableSchemaCaching");
            }
            set
            {
                if (!EnableSchemaCaching.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableSchemaCaching", value);
                }
            }
        }

        public bool EnableSyndication
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableSyndication");
            }
            set
            {
                if (!EnableSyndication.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableSyndication", value);
                }
            }
        }

        public bool EnableThrottling
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableThrottling");
            }
            set
            {
                if (!EnableThrottling.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableThrottling", value);
                }
            }
        }

        public bool EnableVersioning
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableVersioning");
            }
            set
            {
                if (!EnableVersioning.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableVersioning", value);
                }
            }
        }

        public bool ExcludeFromOfflineClient
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ExcludeFromOfflineClient"))
                {
                    GetThisPropertie();
                }
                return base.DataCache.GetProperty<bool>("ExcludeFromOfflineClient");
            }
            set
            {
                if (!ExcludeFromOfflineClient.Equals(value))
                {
                    base.DataCache.AddChangedProperty("ExcludeFromOfflineClient", value);
                }
            }
        }

        public IAveEventReceiverDefinitionCollection EventReceivers
        {
            get
            {
                if (mEventReceiverDefinitionCol == null)
                {
                    lock (eventReceiverLock)
                    {
                        if (mEventReceiverDefinitionCol == null)
                        {
                            Dictionary<string, object> eventReceiversProperties = mRequest.GetEventReceiverDefinitions(this.ParentWeb.ServerRelativeUrl, this.DefaultViewUrl, this.ID, this.Title, "list.eventReceivers");
                            if (eventReceiversProperties != null)
                            {
                                mEventReceiverDefinitionCol = new AveEventReceiverDefinitionCollection(mParentWeb, this, mRequest, "list.eventReceivers", eventReceiversProperties);
                            }
                        }
                    }
                }
                return mEventReceiverDefinitionCol;
            }
        }

        public string EventSinkAssembly
        {
            get
            {
                return base.DataCache.GetProperty<string>("EventSinkAssembly");
            }
            set
            {
                if (!string.Equals(EventSinkAssembly, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("EventSinkAssembly", value);
                }
            }
        }

        public string EntityTypeName
        {
            get
            {
                return base.DataCache.GetProperty<string>("EntityTypeName");
            }
            set
            {
                if (!string.Equals(EntityTypeName, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("EntityTypeName", value);
                }
            }
        }

        public Exception Exception
        {
            get
            {
                return base.DataCache.GetProperty<Exception>("Exception");
            }
        }

        // will clear previous list fields
        public IAveFieldCollection Fields
        {
            get
            {
                if (!this.DataCache.TryGetProperty("Fields", out AveFieldCollection fields))
                {
                    Dictionary<string, object> listFields = mRequest.GetFields(this.ParentWeb.ServerRelativeUrl, this.DefaultViewUrl, this.Title, this.ID, "list.fields", null, AveUserResourceExtension.SupportedResourceCultureNames);
                    fields = new AveFieldCollection(mParentWeb, this, this.mRequest, "list.fields", null, listFields);
                    base.DataCache.AddProperty("Fields", fields);
                    lock (this.ParentWeb.CachingFieldsList)
                    {
                        ParentWeb.CachingFieldsList.Enqueue(this);
                        while (ParentWeb.CachingFieldsList.Count > 10 && ParentWeb.CachingFieldsList.First() != this)
                        {
                            ParentWeb.CachingFieldsList.Dequeue()?.ClearFieldsCache();
                        }
                    }
                }
                return fields;
            }
        }

        public void ClearFieldsCache()
        {
            if (base.DataCache.IsPropertyNotLoaded("Fields"))
            {
                return;
            }
            base.DataCache.RemoveProperty("Fields");
        }

        public bool ForceCheckout
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ForceCheckout");
            }
            set
            {
                if (!ForceCheckout.Equals(value))
                {
                    base.DataCache.AddChangedProperty("ForceCheckout", value);
                }
            }
        }

        public string GetPropertiesXmlForUncustomizedViews()
        {
            return base.DataCache.GetProperty<string>("GetPropertiesXmlForUnCustomizedViews");
        }

        public bool HasExternalDataSource
        {
            get
            {
                return base.DataCache.GetProperty<bool>("HasExternalDataSource");
            }
        }

        public bool Hidden
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Hidden");
            }
            set
            {
                if (!Hidden.Equals(value))
                {
                    base.DataCache.AddChangedProperty("Hidden", value);
                }
            }
        }

        public Guid ID
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public string ImageUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("ImageUrl");
            }
        }

        public bool IsApplicationList
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsApplicationList");
            }
            set
            {
                if (!IsApplicationList.Equals(value))
                {
                    base.DataCache.AddChangedProperty("IsApplicationList", value);
                }
            }
        }

        public bool IsCatalog
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsCatalog");
            }
        }

        public bool IsSiteAssetsLibrary
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsSiteAssetsLibrary");
            }
            set
            {
                if (!IsSiteAssetsLibrary.Equals(value))
                {
                    base.DataCache.AddChangedProperty("IsSiteAssetsLibrary", value);
                }
            }
        }

        public bool IrmEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IrmEnabled");
            }
            set
            {
                if (!IrmEnabled.Equals(value))
                {
                    base.DataCache.AddChangedProperty("IrmEnabled", value);
                }
            }
        }

        public bool IrmExpire
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IrmExpire");
            }
            set
            {
                if (!IrmExpire.Equals(value))
                {
                    base.DataCache.AddChangedProperty("IrmExpire", value);
                }
            }
        }

        public bool IrmReject
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IrmReject");
            }
            set
            {
                if (!IrmReject.Equals(value))
                {
                    base.DataCache.AddChangedProperty("IrmReject", value);
                }
            }
        }

        public IAveListItemCollection Items
        {
            get
            {
                AveCamlQuery query = AveCamlQuery.CreateAllItemsQuery(5000, new string[0]);
                query.DatesInUtc = true;
                IAveListItemCollection listItemsCollection = null;
                if (this.BaseTemplate == AveListTemplateType.ExternalList)
                {
                    //external list doesn't have complete fields of sp list,
                    //can not use any normal query option
                    query.ViewXml = null;
                    query.QueryXml = null;
                    query.QueryOptionXml = null;
                    query.ViewFieldsXml = null;
                    query.FolderServerRelativeUrl = null;
                    listItemsCollection = this.GetItems(query);
                }
                else
                {
                    listItemsCollection = this.GetItems(query);
                }
                base.DataCache.AddProperty("Items",listItemsCollection);
                return listItemsCollection;
            }
        }
        public IAveListItemCollection GetItemsLightly(params string[] loadFieldInternalNames)
        {
            try
            {
                mLogger.Info("Start to get items lightly...");
                ICollection<string> list = ConvertToRealFieldInternalNames(loadFieldInternalNames);
                IAveListItemCollection listItemsCollection = GetItemsLightlyInternal(loadFieldInternalNames);
                return listItemsCollection;
            }
            catch (Exception e)
            {
                mLogger.Error("An error occured when GetItemsLightly, error:{0}", e);
                return Items;
            }
        }

        private ICollection<string> ConvertToRealFieldInternalNames(string[] loadFieldInternalNames)
        {
            ICollection<string> list = new List<string>();
            foreach (var fieldInternalName in loadFieldInternalNames)
            {
                string realFieldInternalName = this.Fields.GetField(fieldInternalName).InternalName;
                list.Add(realFieldInternalName);
            }
            return list;
        }

        public int ItemCount
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ItemCount"))
                {
                    base.DataCache.AddProperty("ItemCount", (this.Items == null) ? 0 : this.Items.Count);
                }
                return base.DataCache.GetProperty<int>("ItemCount");
            }
        }

        public DateTime LastItemDeletedDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("LastItemDeletedDate");
            }
        }

        public DateTime LastItemModifiedDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("LastItemModifiedDate");
            }
        }

        public DateTime LastItemUserModifiedDate
        {
            get
            {
                return base.DataCache.GetProperty<DateTime>("LastItemUserModifiedDate");
            }
        }

        public int MajorWithMinorVersionsLimit
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("MajorWithMinorVersionsLimit"))
                {
                    Dictionary<string, object> versionLimitedProp = mRequest.GetListVersionLimited(this.ParentWebUrl, this.ID);
                    base.DataCache.AddPropertyies(versionLimitedProp);
                }
                return base.DataCache.GetProperty<int>("MajorWithMinorVersionsLimit");
            }
            set
            {
                if (!MajorWithMinorVersionsLimit.Equals(value))
                {
                    base.DataCache.AddChangedProperty("MajorWithMinorVersionsLimit", value);
                }
            }
        }

        public int MajorVersionLimit
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("MajorVersionLimit"))
                {
                    Dictionary<string, object> versionLimitedProp = mRequest.GetListVersionLimited(this.ParentWebUrl, this.ID);
                    base.DataCache.AddPropertyies(versionLimitedProp);
                }
                return base.DataCache.GetProperty<int>("MajorVersionLimit");
            }
            set
            {
                if (!MajorVersionLimit.Equals(value))
                {
                    base.DataCache.AddChangedProperty("MajorVersionLimit", value);
                }
            }
        }

        public bool MultipleDataList
        {
            get
            {
                return base.DataCache.GetProperty<bool>("MultipleDataList");
            }
            set
            {
                if (!MultipleDataList.Equals(value))
                {
                    base.DataCache.AddChangedProperty("MultipleDataList", value);
                }
            }
        }

        public bool NavigateForFormsPages
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("NavigateForFormsPages"))
                {
                    GetThisPropertie();
                }
                return base.DataCache.GetProperty<bool>("NavigateForFormsPages");
            }
            set
            {
                if (!NavigateForFormsPages.Equals(value))
                {
                    base.DataCache.AddChangedProperty("NavigateForFormsPages", value);
                }
            }
        }

        public bool NoCrawl
        {
            get
            {
                return base.DataCache.GetProperty<bool>("NoCrawl");
            }
            set
            {
                if (!NoCrawl.Equals(value))
                {
                    base.DataCache.AddChangedProperty("NoCrawl", value);
                }
            }
        }

        public bool OnQuickLaunch
        {
            get
            {
                return base.DataCache.GetProperty<bool>("OnQuickLaunch");
            }
            set
            {
                if (!OnQuickLaunch.Equals(value))
                {
                    base.DataCache.AddChangedProperty("OnQuickLaunch", value);
                }
            }
        }

        public IAveWeb ParentWeb
        {
            get
            {
                return base.DataCache.GetProperty<IAveWeb>("ParentWeb");
            }
        }

        public string ParentWebUrl
        {
            get
            {
                return this.ParentWeb.ServerRelativeUrl;
            }
        }

        public int ReadSecurity
        {
            get
            {
                return base.DataCache.GetProperty<int>("ReadSecurity");
            }
            set
            {
                if (!ReadSecurity.Equals(value))
                {
                    base.DataCache.AddChangedProperty("ReadSecurity", value);
                }
            }
        }

        public IAveFolder RootFolder
        {
            get
            {
                //if (base.DataCache.IsPropertyNotLoaded("RootFolder"))
                //{
                //    Dictionary<string, object> folderProperties = base.DataCache.GetProperty<Dictionary<string, object>>("RootFolder" + AveObjectModelConstant.ObjectPropertySuffix);
                //    #region 在此处加载Rss Properties会造成效率问题，且Rss Properties是通过模拟Http request支持的，对于这类属性可以不支持，所以这个属性暂时去掉不支持。
                //    //try
                //    //{
                //    //    base.DataCache.AddPropertyies(mRequest.GetListRssProperties(this.ParentWebUrl, this.ID));
                //    //}
                //    //catch (AveSecurityTrimingException ex)
                //    //{
                //    //    mLogger.Warn("An error occurred while get list rssproperties.listid: {0}", this.ID, ex);
                //    //    //throw ex;
                //    //    //contribute level没有权限取得ListRssProperty
                //    //}
                //    #endregion
                //    AveFolder rootFolder = new AveFolder(mRequest, mParentWeb, this, null, folderProperties);
                //    base.DataCache.PropertiesCache["RootFolder"] = rootFolder;
                //    if (base.DataCache.IsPropertyAvailable("RootFolderRssProperties"))
                //    {
                //        Hashtable properties = base.DataCache.GetProperty<Hashtable>("RootFolderRssProperties");
                //        rootFolder.DataCache.PropertiesCache["Properties"] = new AveCustomHashtable(properties, rootFolder.SetChangeProperty);
                //    }
                //}
                lock(mGetRootFolderLock)
                {
                    if (base.DataCache.IsPropertyNotLoaded("RootFolder"))
                    {
                        base.DataCache.AddProperty("RootFolder", GetRootFolder());
                    }
                }
                
                return base.DataCache.GetProperty<IAveFolder>("RootFolder");
            }
        }

        public bool RootWebOnly
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RootWebOnly"))
                {
                    base.DataCache.AddProperty("RootWebOnly",(this.Flags & 0x4000L) != 0);
                }
                return base.DataCache.GetProperty<bool>("RootWebOnly");
            }
            set
            {
                if (!RootWebOnly.Equals(value))
                {
                    base.DataCache.AddChangedProperty("RootWebOnly", value);
                }
            }
        }

        public string SchemaXml
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("SchemaXml"))
                {
                    string schemal = this.mRequest.GetListSchemalXml(this.ParentWebUrl, this.ID, this.Title);
                    AveClientCacheHandler.WriteSchemaXml(schemal, mParentWeb.CacheHandlerId, mParentWeb.ID.ToString(), this.ID.ToString(), this.ID.ToString(), SchemaType.List);
                }
                return AveClientCacheHandler.GetSchemaXml(mParentWeb.CacheHandlerId, mParentWeb.ID.ToString(), this.ID.ToString(), this.ID.ToString(), SchemaType.List);
            }
        }

        public string SendToLocationName
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("SendToLocationName"))
                {
                    GetThisPropertie();
                }
                return base.DataCache.GetProperty<string>("SendToLocationName");
            }
            set
            {
                if (!string.Equals(SendToLocationName, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("SendToLocationName", value);
                }
            }
        }

        public string SendToLocationUrl
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("SendToLocationUrl"))
                {
                    GetThisPropertie();
                }
                return base.DataCache.GetProperty<string>("SendToLocationUrl");
            }
            set
            {
                if (!string.Equals(SendToLocationUrl, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("SendToLocationUrl", value);
                }
            }
        }

        public string DocumentTemplateUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("DocumentTemplateUrl");
            }
            set
            {
                if (!string.Equals(DocumentTemplateUrl, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("DocumentTemplateUrl", value);
                }
            }
        }

        public bool ServerTemplateCanCreateFolders
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ServerTemplateCanCreateFolders");
            }
        }

        public Guid TemplateFeatureId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("TemplateFeatureId");
            }
        }

        public string Title
        {
            get
            {
                return base.DataCache.GetProperty<string>("Title");
            }
            set
            {
                if (!string.Equals(Title, value))
                {
                    base.DataCache.AddChangedProperty("Title", value);
                }
            }
        }

        public IAveUserResource TitleResource
        {
            get
            {
                if (titleResource == null)
                {
                    titleResource = new AveListUserResource(this, AveUserResourceConstants.TITLE_RESOUCE, this.DataCache);
                }
                return titleResource;
            }
        }

        public string ValidationFormula
        {
            get
            {
                return base.DataCache.GetProperty<string>("ValidationFormula");
            }
            set
            {
                if (!string.Equals(ValidationFormula, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("ValidationFormula", value);
                }
            }
        }

        public string ValidationMessage
        {
            get
            {
                return base.DataCache.GetProperty<string>("ValidationMessage");
            }
            set
            {
                if (!string.Equals(ValidationMessage, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("ValidationMessage", value);
                }
            }
        }

        public IAveViewCollection Views
        {
            get
            {
                lock (privateLock)
                {
                    if (base.DataCache.IsPropertyNotLoaded("Views"))
                    {
                        Dictionary<string, object> viewsDic = mRequest.GetViews(mParentWeb.ServerRelativeUrl, this.Title, this.ID);
                        AveViewCollection views = new AveViewCollection(this, mRequest, viewsDic);
                        base.DataCache.AddProperty("Views",views);
                    }
                    return base.DataCache.GetProperty<IAveViewCollection>("Views");
                }
            }
        }

        public int WriteSecurity
        {
            get
            {
                return base.DataCache.GetProperty<int>("WriteSecurity");
            }
            set
            {
                if (!WriteSecurity.Equals(value))
                {
                    base.DataCache.AddChangedProperty("WriteSecurity", value);
                }
            }
        }

        public IAveAlertTemplate AlertTemplate
        {
            get
            {
                return base.DataCache.GetProperty<IAveAlertTemplate>("AlertTemplate");
            }
            set
            {
                base.DataCache.AddChangedProperty("AlertTemplate", value);
            }
        }

        public IAveView DefaultView
        {
            get
            {
                IAveViewCollection vs = this.Views;
                if (vs.Count > 0)
                {
                    foreach (IAveView view in vs)
                    {
                        if (view.DefaultView)
                        {
                            return view;
                        }
                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }
        }

        public IAveUserCustomActionCollection UserCustomActions
        {
            get
            {
                return DataCache.EnsureLoadProperty<IAveUserCustomActionCollection>("UserCustomActionCollection",
                    () =>
                    {
                        Dictionary<string, object> userCustomActions = mRequest.UserCustomActionCollection_Load(AveUserCustomActionScope.List, ParentWeb.ServerRelativeUrl, ID);
                        AveUserCustomActionCollection aveUserCustomActions = new AveUserCustomActionCollection(ParentWeb.Site as AveSite, ParentWeb as AveWeb, this, mRequest, userCustomActions);
                        return aveUserCustomActions;
                    });
            }
        }


        public void EnsureRssSettings()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 设置List Audience Targetting,由于该setting关闭后可能会影响list下数据，关闭需慎重
        /// </summary>
        /// <param name="enableSettings">true:开启 false：关闭</param>
        public void SetAudienceTargetting(bool enableSettings)
        {
            //Target Audiences 365的AudienceTargetting是通过该list中是否存在该field来判断的
            IAveField field = null;
            try
            {
                field = this.Fields.GetById(AveFieldId.AudienceTargeting);
            }
            catch (ArgumentException)
            {
                mLogger.Info("Enable Audience Targetting setting.list:{0}", this.Title);
            }
            if (field == null && enableSettings)
            {
                string createFieldXml = null;
                XmlElement elment = new XmlDataDocument().CreateElement("Field");
                elment.SetAttribute("ID", AveFieldId.AudienceTargeting.ToString());
                elment.SetAttribute("Type", "TargetTo");
                elment.SetAttribute("Name", "TargetTo");
                elment.SetAttribute("DisplayName", "Target Audiences");
                elment.SetAttribute("Required", "FALSE");
                createFieldXml = elment.OuterXml;
                this.Fields.AddFieldAsXml(createFieldXml);
            }
            else if (!enableSettings)
            {
                throw new NotSupportedException("It's not supported to disable Audience Targetting settings.");
                //this.Fields.Delete(field.InternalName);
            }
        }

        /// <summary>
        ///  设置List rattign setting,由于该setting关闭后可能会影响list下数据，关闭需慎重
        /// </summary>
        /// <param name="enableSettings">true:开启  false:关闭</param>
        /// <param name="ratingExperience">"Likes" ro "Ratings"
        public void SetRatingSettings(bool enableSettings, AveRatingSettingType ratingExperience)
        {
            bool isLikesExp = ratingExperience == AveRatingSettingType.Likes;
            if (mRequest == null)
            {
                return;
            }
            if (enableSettings)
            {
                mRequest.SetListRating(this.ParentWebUrl, this.RootFolder.Url, this.ID, true, isLikesExp);
            }
            else
            {
                throw new NotSupportedException("It's not supported to disable list Rating settings.");
                //mRequest.SetListRating(this.ParentWebUrl, this.RootFolder.Url, this.ID, false, true);
            }
        }

        public string EventSinkClass
        {
            get
            {
                return base.DataCache.GetProperty<string>("EventSinkClass");
            }
            set
            {
                if (!string.Equals(EventSinkClass, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("EventSinkClass", value);
                }
            }
        }

        public string EventSinkData
        {
            get
            {
                return base.DataCache.GetProperty<string>("EventSinkData");
            }
            set
            {
                if (!string.Equals(EventSinkData, value, StringComparison.OrdinalIgnoreCase))
                {
                    base.DataCache.AddChangedProperty("EventSinkData", value);
                }
            }
        }

        public IAveFieldIndexCollection FieldIndexes
        {
            get
            {
                return base.DataCache.GetProperty<IAveFieldIndexCollection>("FieldIndexes");
            }
        }

        public IAveFormCollection Forms
        {
            get
            {
                return DataCache.EnsureLoadProperty<IAveFormCollection>("Forms",
                    () => 
                    {
                        Dictionary<string, object> formsPro = mRequest.GetForms(this.ParentWeb.ServerRelativeUrl, this.Title, this.ID);
                        AveFormCollection forms = new AveFormCollection(formsPro);
                        return forms;
                    });
            }
        }

        public IAveAlertTemplate SmsAlertTemplate
        {
            get
            {
                return base.DataCache.GetProperty<IAveAlertTemplate>("SmsAlertTemplate");
            }
            set
            {
                base.DataCache.AddChangedProperty("SmsAlertTemplate", value);
            }
        }

        public IAveWorkflowAssociationCollection WorkflowAssociations
        {
            get
            {
                return DataCache.EnsureLoadProperty<IAveWorkflowAssociationCollection>("WorkflowAssociations",
                    () =>
                    {
                        Dictionary<string, object> workflowsPro = mRequest.GetWorkflowAssociations(this.ParentWeb.ServerRelativeUrl, this.Title, this.ID, "list.workflow", null);
                        AveWorkflowAssociationCollection workflows = new AveWorkflowAssociationCollection(this.ParentWeb, this, "list.workflow", workflowsPro);
                        return workflows;
                    });
            }
        }

        public IAveListItem AddItem(string folderUrl, AveFileSystemObjectType underlyingObjectType)
        {
            return this.AddItem(folderUrl, underlyingObjectType, default(string));
        }

        public IAveListItem AddItem(string fileServerRelativeUrl, Stream body, bool isOverwrite)
        {
            mRequest.AddFileByRestApiWithContext(this.ParentWeb.ServerRelativeUrl, fileServerRelativeUrl, body, isOverwrite);
            return this.GetFileByPath(fileServerRelativeUrl);
        }

        public IAveListItem AddItem(string folderUrl, AveFileSystemObjectType underlyingObjectType, string leafName)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic["folderUrl"] = folderUrl;
            dic["FileSystemObjectType"] = underlyingObjectType;
            dic["leafName"] = leafName;
            return new AveListItem(this.mRequest, this.ParentWeb, this, dic, true);
        }

        public IAveListItem AddItemUsingPath(string folderUrl, AveFileSystemObjectType underlyingObjectType, string leafName)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic["folderUrl"] = folderUrl;
            dic["FileSystemObjectType"] = underlyingObjectType;
            dic["leafName"] = leafName;
            return new AveListItem(this.mRequest, this.ParentWeb, this, dic, true, true);
        }

        public IAveListItem AddItem(AveItemCreationInformation itemCreationInfo)
        {
            return this.AddItem(itemCreationInfo.FolderUrl, itemCreationInfo.UnderlyingObjectType, itemCreationInfo.LeafName);
        }

        public void Delete()
        {
            if (mRequest.DeleteList(this.ParentWeb.ServerRelativeUrl, this.Title, this.ID, (int)this.BaseTemplate, this.EntityTypeName, this.TemplateFeatureId.ToString()))
            {
                (this.ParentWeb.Lists as AveListCollection).ListData.Remove(this);
            }
        }

        internal IAveListItem CreateListItemInstance(Dictionary<string, object> itemProperties)
        {
            if (itemProperties == null)
            {
                throw new ArgumentNullException("item properties");
            }

            return new AveListItem(mRequest, ParentWeb, this, itemProperties, false);
        }

        public IAveListItem GetItemById(int id)
        {
            Dictionary<string, object> itemPro = mRequest.GetItem(this.ParentWeb.ServerRelativeUrl, this.Title, this.ID, id, default(Guid));
            return new AveListItem(mRequest, this.ParentWeb, this, itemPro, false);
        }

        public IAveListItem GetItemById(string id)
        {
            return this.GetItemById(int.Parse(id));
        }
        /// <summary>
        /// Get sub/root folder in current list
        /// </summary>
        /// <param name="serverRelativeUrl">folder ServeRelativUrl</param>
        /// <returns></returns>
        public IAveFolder GetFolder(string serverRelativeUrl)
        {
            Dictionary<string, object> folderProperties = null;
            folderProperties = mRequest.GetFolder(this.ParentWeb.ServerRelativeUrl, this.Title, serverRelativeUrl);
            return new AveFolder(mRequest, this.ParentWeb, this, null, folderProperties);
        }

        public IAveListItemCollection GetItems(AveCamlQuery camlQuery)
        {
            Dictionary<string, object> items = mRequest.GetItems(this.mParentWeb.ServerRelativeUrl, this.Title, this.ID, camlQuery.ToStringArray());
            AveListItemCollection listItemsCollection = new AveListItemCollection(mRequest, this.ParentWeb, this, false, items);
            return listItemsCollection;
        }
        public IAveListItemCollection GetItemsForRecords(AveCamlQuery camlQuery, bool resetItemIdCache = true)
        {
            Dictionary<string, object> items = mRequest.GetItemsForRecords(this.mParentWeb.ServerRelativeUrl, this.Title, this.ID, camlQuery.ToStringArray(), resetItemIdCache);
            AveListItemCollection listItemsCollection = new AveListItemCollection(mRequest, this.ParentWeb, this, false, items);
            return listItemsCollection;
        }

        public IAveListItemCollection GetItemsLightlyInternal(string[] loadFieldInternalNames)
        {
            Dictionary<string, object> items = mRequest.GetItemsLightly(this.mParentWeb.ServerRelativeUrl, this.Title, this.ID, loadFieldInternalNames);
            AveListItemCollection listItemsCollection = new AveListItemCollection(mRequest, this.ParentWeb, this, false, items);
            return listItemsCollection;
        }
        public IAveListItemCollection GetItems(IAveQuery query)
        {
            throw new NotImplementedException();
        }

        public IAveListItemCollection GetPages()
        {
            Dictionary<string, object> items = mRequest.GetPages(this.mParentWeb.ServerRelativeUrl, this.Title, this.ID);
            AveListItemCollection listItemsCollection = new AveListItemCollection(mRequest, this.ParentWeb, this, false, items);
            return listItemsCollection;
        }

        public void Update()
        {
            base.DataCache.AddChangedProperty("ListType", (int)this.BaseTemplate);
            string title = base.DataCache.GetPropertyWithoutChange<string>("Title");
            Dictionary<string, object> updateProperties = new Dictionary<string, object>();
            try
            {
                updateProperties = this.mRequest.UpdateList(this.ParentWeb.ServerRelativeUrl, title, this.ID, base.DataCache.ChangedProperties);
            }
            finally
            {
                if (updateProperties.ContainsKey("SchemaXml"))
                {
                    AveClientCacheHandler.WriteSchemaXml(updateProperties["SchemaXml"].ToString(), mParentWeb.CacheHandlerId, mParentWeb.ID.ToString(), this.ID.ToString(), this.ID.ToString(), SchemaType.List);
                    updateProperties.Remove("SchemaXml");
                }
                base.DataCache.UpdateProperties(updateProperties);
            }
        }

        public IAveListItem GetItemByUniqueId(Guid uniqueId)
        {
            Dictionary<string, object> itemPro = mRequest.GetItem(this.ParentWeb.ServerRelativeUrl, this.Title, this.ID, default(int), uniqueId);
            return new AveListItem(mRequest, this.ParentWeb, this, itemPro, false);
            //IAveListItemCollection items = this.Items;
            //return items[uniqueId];
        }

        public IAveListItemCollection GetItemsByUniqueIds(Guid[] uniqueIds)
        {
            Dictionary<string, object> items = mRequest.GetItemsByUniqueIds(this.ParentWeb.ServerRelativeUrl, this.Title, this.ID, uniqueIds);
            AveListItemCollection listItemsCollection = new AveListItemCollection(mRequest, this.ParentWeb, this, false, items);
            return listItemsCollection;
        }

        public IAveListItem GetFileByPath(string filePath)
        {
            Dictionary<string, object> itemPro = mRequest.GetFileByPath(this.ParentWeb.ServerRelativeUrl, filePath);
            return new AveListItem(mRequest, this.ParentWeb, this, itemPro, false);
            //IAveListItemCollection items = this.Items;
            //return items[uniqueId];
        }


        public void UpdateListSetting(AveListSettingInfo listSettingInfo)
        {
            if (listSettingInfo.Description.IsAvailable)
            {
                this.Description = listSettingInfo.Description.Value != null ? listSettingInfo.Description.Value : "";
            }
            if (listSettingInfo.DefaultItemOpen.IsAvailable)
            {
                if (listSettingInfo.DefaultItemOpen.Value == 0)
                {
                    this.DefaultItemOpenUseListSetting = false;
                }
                else if (listSettingInfo.DefaultItemOpen.Value == 1)
                {
                    this.DefaultItemOpen = AveDefaultItemOpen.Browser;
                }
                else
                {
                    this.DefaultItemOpen = AveDefaultItemOpen.PreferClient;
                }
            }
            if ((this.BaseType != AveBaseType.DocumentLibrary) && (this.BaseType != AveBaseType.Survey)
                && listSettingInfo.EnableAttachments.IsAvailable && (listSettingInfo.EnableAttachments != null))
            {
                this.EnableAttachments = listSettingInfo.EnableAttachments.Value;
            }
            if (this.ServerTemplateCanCreateFolders && listSettingInfo.EnableFolderCreation.IsAvailable && listSettingInfo.EnableFolderCreation != null)
            {
                this.EnableFolderCreation = listSettingInfo.EnableFolderCreation.Value;
            }
            if (this.BaseType == AveBaseType.DocumentLibrary && listSettingInfo.EnableMinorVersions != null)
            {
                if (listSettingInfo.EnableMinorVersions.IsAvailable)
                {
                    this.EnableMinorVersions = listSettingInfo.EnableMinorVersions.Value;
                }
                if (listSettingInfo.EventSinkAssembly.IsAvailable)
                {
                    this.EventSinkAssembly = listSettingInfo.EventSinkAssembly.Value;
                }
            }
            if (this.BaseType != AveBaseType.Survey && listSettingInfo.EnableVersioning.IsAvailable && listSettingInfo.EnableVersioning != null)
            {
                this.EnableVersioning = listSettingInfo.EnableVersioning.Value;
            }
            if (this.BaseType == AveBaseType.Survey && listSettingInfo.AllowMultiResponses.IsAvailable && listSettingInfo.AllowMultiResponses != null)
            {
                this.AllowMultiResponses = listSettingInfo.AllowMultiResponses.Value;
            }

            if (listSettingInfo.ForceCheckout.IsAvailable)
            {
                if (listSettingInfo.ForceCheckout != null)
                {
                    if (!this.HasExternalDataSource && this.BaseTemplate == AveListTemplateType.DocumentLibrary)
                    {
                        this.ForceCheckout = listSettingInfo.ForceCheckout.Value;
                    }
                }
                else
                {
                    this.ForceCheckout = listSettingInfo.ForceCheckout.Value;
                }
            }

            if (listSettingInfo.ValidationMessage.IsAvailable && listSettingInfo.ValidationMessage.Value != null && listSettingInfo.ValidationMessage.Value.Length <= 0x400L)
            {
                this.ValidationMessage = listSettingInfo.ValidationMessage.Value;
            }
            else if (!this.HasExternalDataSource)
            {
                this.NoCrawl = false;
            }

            if (listSettingInfo.ReadSecurity.IsAvailable && listSettingInfo.ReadSecurity != null)
            {
                if (listSettingInfo.ReadSecurity.Value == 1 || listSettingInfo.ReadSecurity.Value == 2)
                {
                    this.ReadSecurity = listSettingInfo.ReadSecurity.Value;
                }
            }
            if (listSettingInfo.WriteSecurity.IsAvailable && listSettingInfo.WriteSecurity != null)
            {
                if (listSettingInfo.WriteSecurity.Value == 1 || listSettingInfo.WriteSecurity.Value == 2 || listSettingInfo.WriteSecurity.Value == 4)
                {
                    this.WriteSecurity = listSettingInfo.WriteSecurity.Value;
                }
            }

            if (listSettingInfo.DraftVersionVisibility.IsAvailable)
            {
                AveDraftVisibilityType temType = (AveDraftVisibilityType)listSettingInfo.DraftVersionVisibility.Value;
                if (temType == AveDraftVisibilityType.Approver || temType == AveDraftVisibilityType.Author || temType == AveDraftVisibilityType.Reader)
                {
                    this.DraftVersionVisibility = (AveDraftVisibilityType)listSettingInfo.DraftVersionVisibility.Value;
                }
            }

            if (listSettingInfo.ThumbnailSize.IsAvailable && listSettingInfo.ThumbnailSize.Value > 0 && this is IAveDocumentLibrary)
            {
                IAveDocumentLibrary spDocLibrary = (IAveDocumentLibrary)this;
                spDocLibrary.ThumbnailsEnabled = true;
                spDocLibrary.ThumbnailSize = listSettingInfo.ThumbnailSize.Value.Value;
            }
            if (listSettingInfo.SendToLocation.IsAvailable && !string.IsNullOrEmpty(listSettingInfo.SendToLocation.Value))
            {
                int temIndex = listSettingInfo.SendToLocation.Value.IndexOf('|');
                this.SendToLocationName = temIndex > 0 ? listSettingInfo.SendToLocation.Value.Substring(0, temIndex) : listSettingInfo.SendToLocation.Value;
                this.SendToLocationUrl = temIndex > 0 ? listSettingInfo.SendToLocation.Value.Substring(temIndex + 1) : string.Empty;
            }
            if ((this.EnableMinorVersions || this.EnableModeration) && listSettingInfo.MaxMajorwithMinorVersionCount.IsAvailable &&
                listSettingInfo.MaxMajorwithMinorVersionCount.Value > 0 && listSettingInfo.MaxMajorwithMinorVersionCount.Value < 0xc350)
            {
                this.MajorWithMinorVersionsLimit = listSettingInfo.MaxMajorwithMinorVersionCount.Value.HasValue ? listSettingInfo.MaxMajorwithMinorVersionCount.Value.Value : AveAllListsTableColumnValue.MaxMajorwithMinorVersionCount;
            }
            if (this.EnableVersioning && listSettingInfo.MaxMajorVersionCount.IsAvailable
                && listSettingInfo.MaxMajorVersionCount.Value > 0 && listSettingInfo.MaxMajorVersionCount.Value < 0xc350)
            {
                this.MajorVersionLimit = listSettingInfo.MaxMajorVersionCount.Value.HasValue ? listSettingInfo.MaxMajorVersionCount.Value.Value : AveAllListsTableColumnValue.MaxMajorVersionCount;
            }
            if (this.HasUniqueRoleAssignments && listSettingInfo.AnonymousPermMask64.IsAvailable)
            {
                if (this.AnonymousPermMask64 != (AveBasePermissions)listSettingInfo.AnonymousPermMask64.Value)
                {
                    this.AnonymousPermMask64 = (AveBasePermissions)listSettingInfo.AnonymousPermMask64.Value;
                }
            }
            string[] dProp = {"AllowDeletion","EnableAssignToEmail", "EnableDeployingList","EnableDeployWithDependentList","EnforceDataValidation",
                  "ExcludeFromOfflineClient","IrmEnabled","IrmExpire","IrmReject","EnablePeopleSelector", "EnableResourceSelector","EnableSchemaCaching","EnableSyndication",
                  "EnableThrottling","DisableGridEditing","NavigateForFormsPages","EmailAlias","SendToLocationName","SendToLocationUrl"};
            string[] sProp = { "Hidden", "OnQuickLaunch", "MultipleDataList", "EnableModeration", "ContentTypesEnabled", "NoCrawl", "ValidationFormula" };
            CopyObjectAve(this, listSettingInfo, sProp, dProp);
            Update();
        }

        public AveListInfo GetListInfo()
        {
            AveListInfo listInfo = new AveListInfo();
            listInfo.Id = this.ID;
            listInfo.Title = this.Title;
            listInfo.BaseTemplate = (int)this.BaseTemplate;
            listInfo.TemplateFeatureId = this.TemplateFeatureId;
            listInfo.BaseType = (int)this.BaseType;
            listInfo.Description = this.Description;
            string url = this.RootFolder.ServerRelativeUrl.Substring(ParentWeb.RootFolder.ServerRelativeUrl.Length).Trim('/');
            listInfo.Url = ParentWeb.Url.TrimEnd('/') + "/" + url;
            listInfo.ServerRelativeUrl = this.RootFolder.ServerRelativeUrl;
            if (this.BaseTemplate == AveListTemplateType.ExternalList)
            {

                listInfo.DataSourceXml = (string)AveAssemblyUtility.InvokeMethod(this.DataSource, this.DataSource.GetType(), "ToXml", null);
            }
            listInfo.RootWebOnly = this.RootWebOnly;
            if (this.ParentWeb.Features[new Guid("961D6A9C-4388-4CF2-9733-38EE8C89AFD4")] != null)
            {
                if (this.BaseTemplate == AveListTemplateType.DiscussionBoard)
                {
                    if (this.EventReceivers != null)
                    {
                        foreach (IAveEventReceiverDefinition def in this.EventReceivers)
                        {
                            if (string.Equals(def.Class, "Microsoft.SharePoint.Portal.CommunityEventReceiver", StringComparison.OrdinalIgnoreCase))
                            {
                                listInfo.IsCommunitySiteDiscussionList = true;
                                break;
                            }
                        }
                    }
                }
            }
            return listInfo;
        }

        public string GetListViewSchema(Guid siteId, Guid listId)
        {
            IAveView defaultView = this.DefaultView;
            if (defaultView != null)
            {
                return defaultView.ViewFields.SchemaXml;
            }
            return string.Empty;
        }

        #endregion

        #region IAveSecurableObject Members

        protected override IAveRoleAssignmentCollection InternalBreakRoleInheritance(bool copyRoleAssignments, bool clearSubscopes)
        {
            Dictionary<string, object> roleAssignmentsProperties = mRequest.BreakRoleInheritance(mParentWeb.ServerRelativeUrl, this.DefaultViewUrl, this.Title, this.ID, -1, copyRoleAssignments, clearSubscopes, "list.roleAssignments");
            AveRoleAssignmentCollection roleAssignmentCol = new AveRoleAssignmentCollection(this, mRequest, ParentWeb.Site as AveSite, ParentWeb as AveWeb, this, -1, "list.roleAssignments", roleAssignmentsProperties);
            return roleAssignmentCol;
        }

        protected override IAveRoleAssignmentCollection InternalResetRoleInheritance()
        {
            Dictionary<string, object> roleAssignmentsProperties = mRequest.ResetRoleInheritance(mParentWeb.ServerRelativeUrl, this.DefaultViewUrl, this.Title, this.ID, -1, "list.roleAssignments");
            AveRoleAssignmentCollection roleAssignmentCol = new AveRoleAssignmentCollection(this, mRequest, ParentWeb.Site as AveSite, ParentWeb as AveWeb, this, -1, "list.roleAssignments", roleAssignmentsProperties);
            return roleAssignmentCol;
        }

        public override void RemoveRoleAssignment(int principalId)
        {
            if (this.RoleAssignments.GetByPrincipalId(principalId) != null)
            {
                mRequest.DeleteRoleAssignment(mParentWeb.ServerRelativeUrl, this.DefaultViewUrl, this.Title, this.ID, -1, principalId, "list.roleAssignments");
            }
        }

        public override IAveRoleAssignmentCollection RoleAssignments
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("RoleAssignments"))
                {
                    if (!this.HasUniqueRoleAssignments)
                    {
                        return this.ParentWeb.RoleAssignments;
                    }
                    Dictionary<string, object> roleAssignmentsProperties = mRequest.GetRoleAssignments(mParentWeb.ServerRelativeUrl, this.DefaultViewUrl, this.Title, this.ID, -1, "list.roleAssignments");
                    AveRoleAssignmentCollection roleAssignments = new AveRoleAssignmentCollection(this, mRequest, ParentWeb.Site as AveSite, ParentWeb as AveWeb, this, -1, "list.roleAssignments", roleAssignmentsProperties);
                    base.DataCache.AddProperty("RoleAssignments",roleAssignments);
                    return roleAssignments;
                }
                return base.DataCache.GetProperty<IAveRoleAssignmentCollection>("RoleAssignments");
            }
        }

        #endregion


        public IAveListDataSource DataSource
        {
            get
            {
                return DataCache.EnsureLoadProperty("DataSource",
                    () =>
                    {
                        var ds = base.DataCache.GetProperty<Dictionary<string, object>>("DataSource" + AveObjectModelConstant.ObjectPropertySuffix);
                        var dataSource = new AveListDataSource(ds);
                        return DataSource;
                    });
            }
        }

        public IAveListItemCollection Folders
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Folders"))
                {
                    AveCamlQuery query = AveCamlQuery.CreateAllFoldersQuery();
                    IAveListItemCollection listItemsCollection = null;
                    if (this.BaseTemplate != AveListTemplateType.ExternalList)
                    {
                        listItemsCollection = this.GetItems(query);
                    }
                    base.DataCache.AddProperty("Folders",listItemsCollection);
                    return listItemsCollection;
                }
                return base.DataCache.GetProperty<IAveListItemCollection>("Folders");
            }
        }

        public IAveListCollection Lists
        {
            get
            {
                return this.ParentWeb.Lists;
            }
        }

        public bool ExcludeFromTemplate
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ExcludeFromTemplate");
            }
        }

        public bool IsThrottled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsThrottled");
            }
        }

        public bool Ordered
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Ordered");
            }
            set
            {
                if (!Ordered.Equals(value))
                {
                    base.DataCache.AddChangedProperty("Ordered", value);
                }
            }
        }

        public bool ShowUser
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ShowUser");
            }
            set
            {
                base.DataCache.AddChangedProperty("ShowUser", value);
            }
        }

        public List<string> NeedSetNullFields
        {
            get { return m_NeedSetNullFields; }
            set { m_NeedSetNullFields = value; }
        }

        public bool IsSchedulingEventOnList()
        {
            return false;
        }

        public IAveListItem AddItem()
        {
            throw new NotImplementedException();
        }

        public void GetViews(ref Dictionary<string, List<AveViewInfo>> viewCache)
        {
            viewCache.Clear();
            foreach (IAveView view in this.Views)
            {
                string url = view.ServerRelativeUrl.Trim('/');
                if (!viewCache.ContainsKey(url))
                {
                    viewCache.Add(url, new List<AveViewInfo>());
                }
                List<AveViewInfo> views = viewCache[url];
                AveViewInfo viewInfo = new AveViewInfo();
                viewInfo.Id = view.ID;
                viewInfo.Title = view.Title;
                viewInfo.IsDefaultView = view.DefaultView;
                viewInfo.IsPersonal = view.PersonalView;
                viewInfo.ViewType = AveViewInfo.GetViewType(view.Type);
                views.Add(viewInfo);
            }
        }

        public IAveView GetView(Guid viewGuid)
        {
            throw new NotImplementedException();
        }

        public AveListSettingInfo GetListSettings()
        {
            AveListSettingInfo listSettingInfo = new AveListSettingInfo();

            GetThisPropertie();
            GetListProperties();
            if (base.DataCache.IsPropertyAvailable("Title"))
            {
                listSettingInfo.Title = this.Title;
            }
            if (base.DataCache.IsPropertyAvailable(AveUserResourceConstants.TITLE_RESOUCE))
            {
                listSettingInfo.TitleResource = base.DataCache.GetProperty<Dictionary<string, string>>(AveUserResourceConstants.TITLE_RESOUCE);
            }
            if (base.DataCache.IsPropertyAvailable("Created"))
            {
                listSettingInfo.Created = this.Created;
            }
            if (base.DataCache.IsPropertyAvailable("Description"))
            {
                listSettingInfo.Description = this.Description;
            }
            if (base.DataCache.IsPropertyAvailable(AveUserResourceConstants.DESCRIPTION_RESOUCE))
            {
                listSettingInfo.DescriptionResource = base.DataCache.GetProperty<Dictionary<string, string>>(AveUserResourceConstants.DESCRIPTION_RESOUCE);
            }
            listSettingInfo.RootFolderInfo = new AveListRootFolderInfo();
            listSettingInfo.RootFolderInfo.Value.MetaInfoDic = this.RootFolder.Properties;
            listSettingInfo.RootFolderInfo.Value.WelcomePageUrl = this.RootFolder.WelcomePage;
            if (AveSPListUtility.IsViewExist(this, "RssView"))
            {
                IAveView rssView = this.Views["RssView"];
                listSettingInfo.RssViewField = rssView.ViewFields.SchemaXml;
            }
            else
            {
                listSettingInfo.RssViewField = "";
            }
            if (base.DataCache.IsPropertyAvailable("MajorWithMinorVersionsLimit"))
            {
                listSettingInfo.MaxMajorwithMinorVersionCount = this.MajorWithMinorVersionsLimit;
            }
            if (base.DataCache.IsPropertyAvailable("MajorVersionLimit"))
            {
                listSettingInfo.MaxMajorVersionCount = this.MajorVersionLimit;
            }
            if (base.DataCache.IsPropertyAvailable("DefaultViewUrl"))
            {
                try
                {
                    listSettingInfo.DefaultView = this.ParentWeb.Url.Substring(0, this.ParentWeb.Url.Length - (this.ParentWebUrl.Length > 1 ? this.ParentWebUrl.Length : 0)) + this.DefaultViewUrl;
                }
                catch (Exception e)
                {
                    mLogger.Warn("An error occurred when getting list default view:{0}. ID:{1}. Reason:{2}.", this.Title, this.ID, e);
                }
            }
            if (AveSPEnv.IsMoss)
            {
                bool allowRating = this.Fields.Contains(AveFieldId.AverageRatings) && this.Fields.Contains(AveFieldId.RatingsCount);
                listSettingInfo.AllowRatingSetting = allowRating;
                if (allowRating && this.RootFolder.Properties.ContainsKey("Ratings_VotingExperience"))
                {
                    listSettingInfo.RatingExperience = this.RootFolder.Properties["Ratings_VotingExperience"].ToString();
                }
            }
            listSettingInfo.EnableAudienceSetting = this.Fields.Contains(AveFieldId.AudienceTargeting);
            if (base.DataCache.IsPropertyAvailable("EventSinkAssembly"))
            {
                listSettingInfo.EventSinkAssembly = this.EventSinkAssembly;
            }
            if (this.ID.Equals(mParentWeb.TaxonomyListId))
            {
                listSettingInfo.IsTaxonomyHiddenList = true;
            }
            if (base.DataCache.IsPropertyAvailable("AllowContentTypes"))
            {
                listSettingInfo.AllowContentTypes = this.AllowContentTypes;
            }
            if (base.DataCache.IsPropertyAvailable("AllowDeletion"))
            {
                listSettingInfo.AllowDeletion = this.AllowDeletion;
            }
            if (base.DataCache.IsPropertyAvailable("ShowUser"))
            {
                listSettingInfo.ShowUser = this.ShowUser;
            }
            if (base.DataCache.IsPropertyAvailable("AllowMultiResponses"))
            {
                listSettingInfo.AllowMultiResponses = this.AllowMultiResponses;
            }
            if (base.DataCache.IsPropertyAvailable("EnableFolderCreation"))
            {
                listSettingInfo.EnableFolderCreation = this.EnableFolderCreation;
            }
            if (base.DataCache.IsPropertyAvailable("EnableModeration"))
            {
                listSettingInfo.EnableModeration = this.EnableModeration;
            }
            if (base.DataCache.IsPropertyAvailable("IrmEnabled"))
            {
                listSettingInfo.IrmEnabled = this.IrmEnabled;
            }
            if (base.DataCache.IsPropertyAvailable("IrmExpire"))
            {
                listSettingInfo.IrmExpire = this.IrmExpire;
            }
            if (base.DataCache.IsPropertyAvailable("IrmReject"))
            {
                listSettingInfo.IrmReject = this.IrmReject;
            }
            if (base.DataCache.IsPropertyAvailable("EnableVersioning"))
            {
                listSettingInfo.EnableVersioning = this.EnableVersioning;
            }
            if (base.DataCache.IsPropertyAvailable("Ordered"))
            {
                listSettingInfo.Ordered = this.Ordered;
            }
            if (base.DataCache.IsPropertyAvailable("ContentTypesEnabled"))
            {
                listSettingInfo.ContentTypesEnabled = this.ContentTypesEnabled;
            }
            if (base.DataCache.IsPropertyAvailable("CrawlNonDefaultViews"))
            {
                listSettingInfo.CrawlNonDefaultViews = this.CrawlNonDefaultViews;
            }
            if (base.DataCache.IsPropertyAvailable("EnableAssignToEmail"))
            {
                listSettingInfo.EnableAssignToEmail = this.EnableAssignToEmail;
            }
            if (base.DataCache.IsPropertyAvailable("LastItemModifiedDate"))
            {
                listSettingInfo.LastModifiedTime = this.LastItemModifiedDate;
            }
            if (base.DataCache.IsPropertyAvailable("EnableDeployWithDependentList"))
            {
                listSettingInfo.EnableDeployWithDependentList = this.EnableDeployWithDependentList;
            }
            if (base.DataCache.IsPropertyAvailable("EnableDeployingList"))
            {
                listSettingInfo.EnableDeployingList = this.EnableDeployingList;
            }
            if (base.DataCache.IsPropertyAvailable("EnablePeopleSelector"))
            {
                listSettingInfo.EnablePeopleSelector = this.EnablePeopleSelector;
            }
            if (base.DataCache.IsPropertyAvailable("EnableResourceSelector"))
            {
                listSettingInfo.EnableResourceSelector = this.EnableResourceSelector;
            }
            if (base.DataCache.IsPropertyAvailable("EnableSchemaCaching"))
            {
                listSettingInfo.EnableSchemaCaching = this.EnableSchemaCaching;
            }
            if (base.DataCache.IsPropertyAvailable("EnforceDataValidation"))
            {
                listSettingInfo.EnforceDataValidation = this.EnforceDataValidation;
            }
            if (base.DataCache.IsPropertyAvailable("EnableSyndication"))
            {
                listSettingInfo.EnableSyndication = this.EnableSyndication;
            }
            if (base.DataCache.IsPropertyAvailable("ExcludeFromTemplate"))
            {
                listSettingInfo.ExcludeFromTemplate = this.ExcludeFromTemplate;
            }
            if (base.DataCache.IsPropertyAvailable("Hidden"))
            {
                listSettingInfo.Hidden = this.Hidden;
            }
            if (base.DataCache.IsPropertyAvailable("MultipleDataList"))
            {
                listSettingInfo.MultipleDataList = this.MultipleDataList;
            }
            if (base.DataCache.IsPropertyAvailable("NoCrawl"))
            {
                listSettingInfo.NoCrawl = this.NoCrawl;
            }
            if (base.DataCache.IsPropertyAvailable("EnableAttachments"))
            {
                listSettingInfo.EnableAttachments = this.EnableAttachments;
            }
            if (base.DataCache.IsPropertyAvailable("EnableMinorVersions"))
            {
                listSettingInfo.EnableMinorVersions = this.EnableMinorVersions;
            }
            if (base.DataCache.IsPropertyAvailable("ForceCheckout"))
            {
                listSettingInfo.ForceCheckout = this.ForceCheckout;
            }

            if (base.DataCache.IsPropertyAvailable("DraftVersionVisibility"))
            {
                listSettingInfo.DraftVersionVisibility = (int)this.DraftVersionVisibility;
            }
            if (base.DataCache.IsPropertyAvailable("AllowRssFeeds"))
            {
                listSettingInfo.AllowRssFeads = this.AllowRssFeeds;
            }
            if (base.DataCache.IsPropertyAvailable("EnableThrottling"))
            {
                listSettingInfo.EnableThrottling = this.EnableThrottling;
            }
            if (base.DataCache.IsPropertyAvailable("IsThrottled"))
            {
                listSettingInfo.IsThrottled = this.IsThrottled;
            }

            if (base.DataCache.IsPropertyAvailable("HasUniqueRoleAssignments"))
            {
                listSettingInfo.HasUniqueRoleAssigntments = this.HasUniqueRoleAssignments;
            }
            if (base.DataCache.IsPropertyAvailable("OnQuickLaunch"))
            {
                listSettingInfo.OnQuickLaunch = this.OnQuickLaunch;
            }

            if (base.DataCache.IsPropertyAvailable("ValidationFormula"))
            {
                listSettingInfo.ValidationFormula = this.ValidationFormula;
            }
            if (base.DataCache.IsPropertyAvailable("ValidationMessage"))
            {
                listSettingInfo.ValidationMessage = this.ValidationMessage;
            }

            if (base.DataCache.IsPropertyAvailable("IsSiteAssetsLibrary"))
            {
                listSettingInfo.IsSiteAssetsLibrary = this.IsSiteAssetsLibrary;
            }

            if (base.DataCache.IsPropertyAvailable("RequestAccessEnabled"))
            {
                listSettingInfo.RequestAccessEnabled = this.RequestAccessEnabled;
            }

            if (base.DataCache.IsPropertyAvailable("EnableKeywordsField"))
            {
                listSettingInfo.EnableKeywordsField = this.EnableKeywordsField;
            }

            if (base.DataCache.IsPropertyAvailable("KeywordsFieldExistsInContentTypes"))
            {
                listSettingInfo.KeywordsFieldExistsInContentTypes = this.KeywordsFieldExistsInContentTypes;
            }

            if (base.DataCache.IsPropertyAvailable("EnableMetadataPromotion"))
            {
                listSettingInfo.EnableMetadataPromotion = this.EnableMetadataPromotion;
            }

            #region advanced Settings

            // 0 , PreferClient
            // 1 , Browser
            if (base.DataCache.IsPropertyAvailable("DefaultItemOpen"))
            {
                listSettingInfo.DefaultItemOpen = (int)this.DefaultItemOpen;
                if (base.DataCache.IsPropertyAvailable("DefaultItemOpenUseListSetting"))
                {
                    listSettingInfo.DefaultItemOpenUseListSetting = this.DefaultItemOpenUseListSetting;
                }
                else
                {
                    listSettingInfo.DefaultItemOpenUseListSetting = false ;
                }
            }

            if (base.DataCache.IsPropertyAvailable("ListExperienceOptions"))
            {
                listSettingInfo.ListExperience = (int)this.ListExperience;
            }
            if (base.DataCache.IsPropertyAvailable("EnableManagedIndexes"))
            {
                listSettingInfo.EnableManagedIndexes = this.EnableManagedIndexes;
            }

            if (base.DataCache.IsPropertyAvailable("ExcludeFromOfflineClient"))
            {
                listSettingInfo.ExcludeFromOfflineClient = this.ExcludeFromOfflineClient;
            }
            if (base.DataCache.IsPropertyAvailable("SendToLocationName") && base.DataCache.IsPropertyAvailable("SendToLocationUrl"))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(this.SendToLocationName);
                sb.Append("|");
                sb.Append(this.SendToLocationUrl);
                listSettingInfo.SendToLocation = sb.ToString();
                listSettingInfo.SendToLocationName = this.SendToLocationName;
                listSettingInfo.SendToLocationUrl = this.SendToLocationUrl;
            }
            if (this is IAveDocumentLibrary)
            {
                listSettingInfo.DocumentTemplateUrl = this.DocumentTemplateUrl;
            }
            if (base.DataCache.IsPropertyAvailable("DisableGridEditing"))
            {
                listSettingInfo.DisableGridEditing = this.DisableGridEditing;
            }
            if (base.DataCache.IsPropertyAvailable("NavigateForFormsPages"))
            {
                listSettingInfo.NavigateForFormsPages = this.NavigateForFormsPages;
            }
            if (base.DataCache.IsPropertyAvailable("ReadSecurity"))
            {
                listSettingInfo.ReadSecurity = this.ReadSecurity;
            }
            if (base.DataCache.IsPropertyAvailable("WriteSecurity"))
            {
                listSettingInfo.WriteSecurity = this.WriteSecurity;
            }
            #endregion

            #region compliance setting (Apply label to items in this list or library)

            var complianceInfo = this.GetListComplianceTag();
            if (complianceInfo != null)
            {
                listSettingInfo.ComplianceTagInfo = complianceInfo;
            }

            #endregion

            return listSettingInfo;
        }

        public AveRestoreResult RestoreListItem(AveListItemInfo info, Dictionary<string, object> data, Dictionary<string, object> userData)
        {
            this.SetTaxonomyField(info, -1, true, info.FieldsInfo.TermIdMapping);
            Dictionary<string, object> docData = AssembleBaseItemInfo(info, this);
            docData["ListTemplate"] = (int)this.BaseTemplate;
            docData["ListEnableModeration"] = this.EnableModeration;
            docData["ListEnableVersioning"] = this.EnableVersioning;
            Dictionary<string, object> fields = AveList.ConvertFieldValuesToString(info.FieldsInfo.Fields, info.FieldsInfo.MultilookupFields, (int)this.BaseTemplate);
            if (!fields.ContainsKey("Modified"))
            {
                fields.Add("Modified", info.DTimeLastModified);
            }

            if (this.BaseTemplate == AveListTemplateType.DiscussionBoard)
            {
                if (data.ContainsKey("DiscussionTopic"))
                {
                    docData["DiscussionTopic"] = data["DiscussionTopic"];
                }
                if (data.ContainsKey("ParentThreadId"))
                {
                    docData["ParentThreadId"] = data["ParentThreadId"];
                }
            }

            if (this.BaseTemplate == AveListTemplateType.Meetings)
            {
                this.AssemblyMeetingItemInfo(info, userData, docData);
            }
            if (this.NeedSetNullFields == null)
            {
                this.NeedSetNullFields = SetNeedSetNullFields(info.KeepDefaultValue, fields);
            }
            fields.Add("NeedSetNullFields", this.NeedSetNullFields);
            Dictionary<string, object> restoreResult = mRequest.RestoreListItem(docData, fields);
            info.IsNewCreated = restoreResult.ContainsKey("IsNewCreated") ? (bool)restoreResult["IsNewCreated"] : false;

            if (!(Boolean)restoreResult["RestoreStatus"])
            {
                throw new AveRestoreException(AveRestoreResult.Failed, restoreResult["Exception"] as string);
            }
            AveListItem item = new AveListItem(mRequest, mParentWeb, this, restoreResult["Item"] as Dictionary<string, object>, false);
            info.AveItem.ListItem = item;
            info.RowId = item.ID;
            return AveRestoreResult.Normal;
        }

        public List<string> SetNeedSetNullFields(bool keepDefaultValue, Dictionary<string, object> fields)
        {
            List<string> needSetNullFields = new List<string>();
            string[] AllCols = new string[]	{"nvarchar1" ,"nvarchar2" ,"nvarchar3" ,"nvarchar4" ,"nvarchar5" ,"nvarchar6" ,"nvarchar7" ,"nvarchar8" ,
                "ntext1" ,"ntext2" ,"ntext3" ,"ntext4" ,"sql_variant1","nvarchar9" ,"nvarchar10" ,"nvarchar11" ,"nvarchar12" ,"nvarchar13" ,
	            "nvarchar14" ,"nvarchar15" ,"nvarchar16" ,"ntext5" ,"ntext6" ,"ntext7" ,"ntext8" ,"sql_variant2","nvarchar17" ,"nvarchar18" ,
                "nvarchar19" ,"nvarchar20" ,"nvarchar21" ,"nvarchar22" ,"nvarchar23" ,"nvarchar24" ,"ntext9" ,"ntext10" ,"ntext11" ,"ntext12" ,
                "sql_variant3","nvarchar25" ,"nvarchar26" ,"nvarchar27" ,"nvarchar28" ,"nvarchar29" ,"nvarchar30" ,"nvarchar31" ,"nvarchar32" ,
                "ntext13" ,"ntext14" ,"ntext15" ,"ntext16" ,"sql_variant4","nvarchar33" ,"nvarchar34" ,"nvarchar35" ,"nvarchar36" ,"nvarchar37" ,
                "nvarchar38" ,"nvarchar39" ,"nvarchar40" ,"ntext17" ,"ntext18" ,"ntext19" ,"ntext20" ,"sql_variant5","nvarchar41" ,"nvarchar42" ,
                "nvarchar43" ,"nvarchar44" ,"nvarchar45" ,"nvarchar46" ,"nvarchar47" ,"nvarchar48" ,"ntext21" ,"ntext22" ,"ntext23" ,"ntext24" ,
                "sql_variant6","nvarchar49" ,"nvarchar50" ,"nvarchar51" , "nvarchar52" ,"nvarchar53" ,"nvarchar54" ,"nvarchar55" ,"nvarchar56" ,
                "ntext25" ,"ntext26" ,"ntext27" ,"ntext28" ,"sql_variant7","nvarchar57" ,"nvarchar58" ,"nvarchar59" ,"nvarchar60" ,"nvarchar61" ,
                "nvarchar62" ,"nvarchar63" ,"nvarchar64" ,"ntext29" ,"ntext30" ,"ntext31" ,"ntext32" ,"sql_variant8","int1","int2","int3","int4",
                "int5","int6","int7","int8","int9","int10","int11","int12","int13","int14","int15","int16","float1","float2","float3","float4",
                "float5","float6","float7","float8","float9","float10","float11","float12", "datetime1","datetime2","datetime3","datetime4",
                "datetime5","datetime6","datetime7","datetime8","bit1","bit2","bit3","bit4","bit5","bit6","bit7","bit8","bit9","bit10","bit11",
                "bit12","bit13","bit14","bit15","bit16","uniqueidentifier1"};

            IAveFieldCollection fieldCollection = null;
            if (fields.ContainsKey("ContentType"))
            {
                string contentTypeId = fields["ContentType"].ToString();
                IAveContentType contentType = this.ContentTypes.GetById(contentTypeId);
                if (contentType != null)
                {
                    fieldCollection = contentType.Fields;
                }
                else
                {
                    fieldCollection = this.Fields;
                }
            }
            else
            {
                fieldCollection = this.Fields;
            }

            foreach (IAveField field in fieldCollection)
            {
                try
                {
                    if (field.Type == AveFieldType.WorkflowStatus)// || field.Type == AveFieldType.Lookup)
                    {
                        continue;
                    }
                    object obj = field.ColName;
                    if (obj != null)
                    {
                        string colName = obj.ToString();
                        if (AllCols.Contains(colName) && !field.Required)
                        {
                            if ((!String.IsNullOrEmpty(field.DefaultValue) || !String.IsNullOrEmpty(field.DefaultFormula)) && keepDefaultValue)
                            {
                                continue;
                            }
                            if (IsUnCompletedLookupField(field as IAveFieldLookup))
                            {
                                continue;
                            }
                            if (NoNeedSetNull(field))
                            {
                                continue;
                            }
                            if (IsCommunitySiteSpecialFields(field.InternalName))
                            {
                                continue;
                            }
                            needSetNullFields.Add(field.InternalName);
                        }
                    }
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveObjectModel_CommonResource.SetNeedSetNullFieldsError, this.Title, this.mParentWeb.Url, e.ToString());
                    //mLog.Log(AveLogLevel.WARN,"An error occurred while SetNeedSetNullFields. error:{0}", e.ToString());
                }
            }
            return needSetNullFields;
        }

        public List<string> SetNeedSetNullFields(bool keepDefaultValue, Dictionary<string, object> fields, Dictionary<string, object> allUserData)
        {
            List<string> needSetNullFields = new List<string>();
            string[] AllCols = new string[] {"nvarchar1" ,"nvarchar2" ,"nvarchar3" ,"nvarchar4" ,"nvarchar5" ,"nvarchar6" ,"nvarchar7" ,"nvarchar8" ,
                "ntext1" ,"ntext2" ,"ntext3" ,"ntext4" ,"sql_variant1","nvarchar9" ,"nvarchar10" ,"nvarchar11" ,"nvarchar12" ,"nvarchar13" ,
                "nvarchar14" ,"nvarchar15" ,"nvarchar16" ,"ntext5" ,"ntext6" ,"ntext7" ,"ntext8" ,"sql_variant2","nvarchar17" ,"nvarchar18" ,
                "nvarchar19" ,"nvarchar20" ,"nvarchar21" ,"nvarchar22" ,"nvarchar23" ,"nvarchar24" ,"ntext9" ,"ntext10" ,"ntext11" ,"ntext12" ,
                "sql_variant3","nvarchar25" ,"nvarchar26" ,"nvarchar27" ,"nvarchar28" ,"nvarchar29" ,"nvarchar30" ,"nvarchar31" ,"nvarchar32" ,
                "ntext13" ,"ntext14" ,"ntext15" ,"ntext16" ,"sql_variant4","nvarchar33" ,"nvarchar34" ,"nvarchar35" ,"nvarchar36" ,"nvarchar37" ,
                "nvarchar38" ,"nvarchar39" ,"nvarchar40" ,"ntext17" ,"ntext18" ,"ntext19" ,"ntext20" ,"sql_variant5","nvarchar41" ,"nvarchar42" ,
                "nvarchar43" ,"nvarchar44" ,"nvarchar45" ,"nvarchar46" ,"nvarchar47" ,"nvarchar48" ,"ntext21" ,"ntext22" ,"ntext23" ,"ntext24" ,
                "sql_variant6","nvarchar49" ,"nvarchar50" ,"nvarchar51" , "nvarchar52" ,"nvarchar53" ,"nvarchar54" ,"nvarchar55" ,"nvarchar56" ,
                "ntext25" ,"ntext26" ,"ntext27" ,"ntext28" ,"sql_variant7","nvarchar57" ,"nvarchar58" ,"nvarchar59" ,"nvarchar60" ,"nvarchar61" ,
                "nvarchar62" ,"nvarchar63" ,"nvarchar64" ,"ntext29" ,"ntext30" ,"ntext31" ,"ntext32" ,"sql_variant8","int1","int2","int3","int4",
                "int5","int6","int7","int8","int9","int10","int11","int12","int13","int14","int15","int16","float1","float2","float3","float4",
                "float5","float6","float7","float8","float9","float10","float11","float12", "datetime1","datetime2","datetime3","datetime4",
                "datetime5","datetime6","datetime7","datetime8","bit1","bit2","bit3","bit4","bit5","bit6","bit7","bit8","bit9","bit10","bit11",
                "bit12","bit13","bit14","bit15","bit16","uniqueidentifier1"};

            IAveFieldCollection fieldCollection = this.Fields;//SAAS-25683  由于新建 document使用的是默认的content type还原指定的content type时，上一个content type 的value还继续显示在页面， by zma
            //if (fields.ContainsKey("ContentType"))
            //{
            //    string contentTypeId = fields["ContentType"].ToString();
            //    IAveContentType contentType = this.ContentTypes.GetById(contentTypeId);
            //    if (contentType != null)
            //    {
            //        fieldCollection = contentType.Fields;
            //    }
            //    else
            //    {
            //        fieldCollection = this.Fields;
            //    }
            //}
            //else
            //{
            //    fieldCollection = this.Fields;
            //}
            Dictionary<string, object> defaultValues = allUserData["#DefaultValues"] as Dictionary<string, object>;
            foreach (IAveField field in fieldCollection)
            {
                try
                {
                    if (field.Type == AveFieldType.WorkflowStatus)// || field.Type == AveFieldType.Lookup)
                    {
                        continue;
                    }
                    object obj = field.ColName;
                    if (obj != null)
                    {
                        string colName = obj.ToString();
                        if (AllCols.Contains(colName) && !field.Required)
                        {
                            if (keepDefaultValue && (defaultValues.ContainsKey(field.InternalName) || !String.IsNullOrEmpty(field.DefaultValue) || !String.IsNullOrEmpty(field.DefaultFormula)))
                            {
                                continue;
                            }
                            if (IsUnCompletedLookupField(field as IAveFieldLookup))
                            {
                                continue;
                            }
                            if (NoNeedSetNull(field))
                            {
                                continue;
                            }
                            if (IsCommunitySiteSpecialFields(field.InternalName))
                            {
                                continue;
                            }
                            needSetNullFields.Add(field.InternalName);
                        }
                    }
                }
                catch (Exception e)
                {
                    mLogger.Error(AveObjectModel_CommonResource.SetNeedSetNullFieldsError, this.Title, this.mParentWeb.Url, e.ToString());
                    //mLog.Log(AveLogLevel.WARN,"An error occurred while SetNeedSetNullFields. error:{0}", e.ToString());
                }
            }
            return needSetNullFields;
        }


        private bool NoNeedSetNull(IAveField field)
        {
            return field.Hidden || field.TypeAsString.Equals("Facilities", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsCommunitySiteSpecialFields(string internalName)
        {
            List<string> internalNames = new List<string>();
            if ((int)this.BaseTemplate == 880)
            {
                internalNames.Add("NumberOfBestResponses");
                internalNames.Add("NumberOfDiscussions");
                internalNames.Add("NumberOfReplies");
                internalNames.Add("ReputationScore");
            }
            else if ((int)this.BaseTemplate == 500)
            {
                internalNames.Add("TopicCount");
                internalNames.Add("ReplyCount");
            }
            if (internalNames.Contains(internalName))
            {
                return true;
            }
            return false;
        }
        //sometimes lookupfield is restored in postaction, should set null for this field
        internal bool IsUnCompletedLookupField(IAveFieldLookup field)
        {
            return field != null && string.IsNullOrEmpty(field.LookupList);
        }

        private IAveTermStore GetTermStore(IAveTaxonomySession session, IAveField field, ref int LCID)
        {
            IAveTermStore termStore = null;
            Guid sspId = Guid.Empty;
            IAveTaxonomyField tField = field as IAveTaxonomyField;
            if (tField.SspId == Guid.Empty && !tField.ID.Equals(new Guid("23F27201-BEE3-471e-B2E7-B64FD8B7CA38")))
            {
                object customProperty = field.GetCustomProperty("SspId");
                if (customProperty != null)
                {
                    sspId = new Guid(customProperty.ToString());
                }
            }
            else
            {
                sspId = tField.SspId;
            }
            if (sspId != Guid.Empty)
            {
                termStore = session.TermStores[sspId];
            }
            else
            {
                termStore = session.DefaultKeywordsTermStore;
                if (termStore == null)
                {
                    termStore = session.DefaultSiteCollectionTermStore;
                }
                if (termStore == null)
                {
                    termStore = session.TermStores[0];
                }
            }
            if (termStore != null && LCID < 0)
            {
                LCID = termStore.DefaultLanguage;
            }
            if (termStore != null && termStore.Languages.Contains(DefaultLCID))
            {
                DefaultLCID = termStore.DefaultLanguage;
                LCID = DefaultLCID;
            }

            return termStore;
        }

        private Dictionary<string, object> AssembleTaxonomyField(IAveTaxonomyField tField, string fieldName, int LCID, List<IAveTerm> terms)
        {
            Dictionary<string, object> taxonomyfield = new Dictionary<string, object>();
            if (tField.AllowMultipleValues)
            {
                //IAveTaxonomyFieldValueCollection taxValueCollection = tField.TaxonomyFieldValueCollection;
                List<string> mutipleText = new List<string>();
                foreach (IAveTerm tTerm in terms)
                {
                    if (tTerm != null)
                    {
                        int effectiveLcid = LCID;
                        string text = tTerm.GetDefaultLabel(effectiveLcid) + "|" + tTerm.ID;
                        mutipleText.Add(text);
                    }
                }
                taxonomyfield.Add("Text", mutipleText);
                taxonomyfield.Add("AllowMultipleValues", true);
            }
            else
            {
                string text = string.Empty;
                if (terms.Count > 0)
                {
                    int effectiveLcid = LCID;
                    text = terms[0].GetDefaultLabel(effectiveLcid) + "|" + terms[0].ID;
                }
                taxonomyfield.Add("Text", text);
                taxonomyfield.Add("AllowMultipleValues", false);
            }
            taxonomyfield.Add("FieldName", fieldName);
            taxonomyfield.Add("Id", tField.ID);
            taxonomyfield.Add("IsKeyWord", tField.IsKeyword);
            return taxonomyfield;
        }

        private IAveTerm GetTermById(Guid termId, Dictionary<Guid, Guid> termIdMapping, IAveTermSet termSet, IAveTaxonomyField tField, IAveTaxonomySession session, IAveTermStore termStore)
        {
            IAveTerm term = null;
            try
            {
                if (termIdMapping != null && termIdMapping.ContainsKey(termId))
                {
                    termId = termIdMapping[termId];
                }
                if (termSet != null)
                {
                    //term = termSet.GetTerm(termId);
                    term = termSet.GetTermById(termId);
                    //if (term == null)
                    //{
                    //    term = GetTermFromTermCollection(termId, termSet.Terms);
                    //}
                    //添加判断，如果是EnterpriseKeyWord类型的Field，当KeyWord值引用TermStore上Term时，关联到正确的TermStore上。
                    if (term == null && tField.ID.Equals(new Guid("23F27201-BEE3-471e-B2E7-B64FD8B7CA38")))
                    {
                        foreach (IAveTermStore tStore in session.TermStores)
                        {
                            if (term == null)
                            {
                                term = tStore.GetTerm(termId);
                            }
                        }
                    }
                }
                else
                {
                    term = termStore.GetTerm(termId);
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Error occurred while get term by id.ErrorMessage:{0}.", ex.ToString());
            }

            return term;
        }

        /// <summary>
        /// This is a recursive method, find term by name, include all subTerms in the collection
        /// </summary>
        /// <param name="termName">term name</param>
        /// <param name="terms">term collection</param>
        /// <returns>if no term if found, return null</returns>
        private IAveTerm GetTermFromTermCollection(string termName, IAveTermCollection terms)
        {
            IAveTerm term = terms[termName];
            if (term == null)
            {
                foreach (IAveTerm term1 in terms)
                {
                    if (term1.Terms.Count > 0)
                    {
                        term = GetTermFromTermCollection(termName, term1.Terms);
                        if (term != null)
                        {
                            return term;
                        }
                    }
                }
            }
            return term;
        }


        private IAveTerm GetTermByName(string termName,
                                       ref string realTermName,
                                       Dictionary<Guid, Guid> termIdMapping,
                                       IAveTermSet termSet,
                                       IAveTaxonomyField tField,
                                       IAveTaxonomySession session,
                                       IAveTermStore termStore,
                                       ref string[] termHiberarchy)
        {
            IAveTerm term = null;
            if (termName.Contains("|"))
            {
                string[] temp = termName.Split('|');
                if (temp.Length == 2)
                {
                    realTermName = temp[0];
                    term = GetTermById(new Guid(temp[1]), termIdMapping, termSet, tField, session, termStore);
                }
            }
            //'<'表示term的层次关系。
            else if (termName.Contains("<"))
            {
                termHiberarchy = termName.Split('<');
                term = termSet.Terms[termHiberarchy[0]];
                for (int i = 1; i < termHiberarchy.Length; i++)
                {
                    if (string.IsNullOrEmpty(termHiberarchy[i]))
                    {
                        continue;
                    }
                    term = term.Terms[NormalizeName(termHiberarchy[i])];
                }
            }
            if (term == null && termSet != null)
            {
                try
                {
                    term = GetTermFromTermCollection(NormalizeName(realTermName), termSet.Terms);
                }
                catch (Exception ex)
                {
                    mLogger.Debug("Error occurred while get term from term set.ErrorMessage:{0}.", ex.ToString());
                    //DOC-78396 使用此方法刷新对象
                    IAveTermCollection ts = termSet.GetTerms(NormalizeName(realTermName).Trim(), true);
                    term = GetTermFromTermCollection(NormalizeName(realTermName).Trim(), ts);
                }
            }
            return term;
        }

        private IAveTerm TryCreateTerm(IAveTermSet termSet, string[] termHiberarchy, ref bool needSubmit, int LCID, string termName)
        {
            IAveTerm term = null;
            if (termHiberarchy != null && termHiberarchy.Length > 0)
            {
                try
                {
                    term = termSet.Terms[NormalizeName(termHiberarchy[0])];
                }
                catch (Exception ex1)
                {
                    mLogger.Debug("An error occurred while get term.Message:{0}.", ex1.ToString());
                    //DOC-78396
                    try
                    {
                        term = termSet.CreateTerm(NormalizeName(termHiberarchy[0]).Trim(), LCID, Guid.NewGuid());
                        termSet.TermStore.CommitAll();
                    }
                    catch (Exception ex2)
                    {
                        mLogger.Debug("An error occurred while creating term.ErrorMessage:{0}.", ex2.ToString());
                        //DOC-78396 使用此方法刷新对象
                        IAveTermCollection ts = termSet.GetTerms(NormalizeName(termHiberarchy[0]).Trim(), true);
                        term = termSet.Terms[NormalizeName(termHiberarchy[0]).Trim()];
                    }
                }
                for (int i = 1; i < termHiberarchy.Length; i++)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(termHiberarchy[i]))
                        {
                            continue;
                        }
                        term = term.Terms[NormalizeName(termHiberarchy[i])];
                    }
                    catch (Exception ex3)
                    {
                        mLogger.Debug("An error occurred.Error Message:{0}.", ex3.ToString());
                        term = term.CreateTerm(NormalizeName(termHiberarchy[i]).Trim(), LCID, Guid.NewGuid());
                        term.TermStore.CommitAll();
                    }
                }
            }
            else
            {
                term = termSet.CreateTerm(termName, LCID, Guid.NewGuid());
                needSubmit = true;
            }

            return term;
        }

        public void SetTaxonomyField(AveBaseItemInfo info, int LCID, bool ForceAddTerm, Dictionary<Guid, Guid> termIdMapping)
        {
            lock (mUpdateTaxonomyFieldLock)
            {
                List<Dictionary<string, object>> needUpdateTaonoxyFields = new List<Dictionary<string, object>>();
                Dictionary<string, string> taxonomyField = info.FieldsInfo.TaxonomyFieldsInMapping;
                if (taxonomyField != null && taxonomyField.Count > 0)
                {
                    try
                    {
                        foreach (string fieldName in taxonomyField.Keys)
                        {
                            IAveField field = this.Fields.GetField(fieldName);
                            IAveTaxonomyField tField = field as IAveTaxonomyField;
                            IAveTaxonomySession session = (mParentWeb.Site).AveSPTaxonomySession;
                            IAveTermStore termStore = GetTermStore(session, field, ref LCID);
                            IAveTermSet termSet = null;
                            if (tField.TermSetId != Guid.Empty && termStore != null)
                            {
                                termSet = termStore.GetTermSet(tField.TermSetId);
                            }

                            bool submit = false;
                            string[] termNames = taxonomyField[fieldName].Split(';');
                            string[] termHiberarchy = null;
                            //TaxonomyFieldValueCollection values = item[fieldName] as TaxonomyFieldValueCollection;
                            List<IAveTerm> terms = new List<IAveTerm>();
                            foreach (string termName in termNames)
                            {
                                if (string.IsNullOrEmpty(termName))
                                {
                                    continue;
                                }
                                IAveTerm term = null;
                                termHiberarchy = null;
                                string realTermName = termName;
                                try
                                {
                                    term = GetTermByName(termName, ref realTermName, termIdMapping, termSet, tField, session, termStore, ref termHiberarchy);
                                }
                                catch (ArgumentOutOfRangeException ex)
                                {
                                    mLogger.Error("An error occurred when get term.ErrorMessage:{0}", ex.ToString());
                                    if (ForceAddTerm && termSet != null)
                                    {
                                        if (termHiberarchy != null && termHiberarchy.Length > 0 && string.IsNullOrEmpty(termHiberarchy[0]))
                                        {
                                            continue;
                                        }
                                        term = TryCreateTerm(termSet, termHiberarchy, ref submit, LCID, realTermName);
                                    }
                                }
                                if (term != null)
                                {
                                    terms.Add(term);
                                    //如果field不允许多值，没有必要找多个term了。
                                    if (!tField.AllowMultipleValues)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (submit)
                            {
                                try
                                {
                                    termStore.CommitAll();
                                    submit = false;
                                }
                                catch (Exception ex4)
                                {
                                    mLogger.Error("Error occurred while Commit All.Message:{0}.", ex4.ToString());
                                    terms.Clear();
                                    foreach (string termName in termNames)
                                    {
                                        if (string.IsNullOrEmpty(termName))
                                        {
                                            continue;
                                        }
                                        try
                                        {
                                            //DOC-78396 使用此方法刷新对象
                                            IAveTermCollection ts = termSet.GetTerms(NormalizeName(termName).Trim(), true);
                                            terms.Add(termSet.Terms[NormalizeName(termName).Trim()]);
                                        }
                                        catch (Exception ex5)
                                        {
                                            mLogger.Error("Error occurred while adding term.Message:{0}.", ex5.ToString());
                                        }
                                    }
                                }
                            }
                            //if the termsetid and anchorid both are empty, means the column is invalid. "value not fall in range" exception will be throwed when call item.update()
                            if (tField.TermSetId != Guid.Empty || tField.AnchorId != Guid.Empty)
                            {
                                needUpdateTaonoxyFields.Add(AssembleTaxonomyField(tField, fieldName, LCID, terms));
                            }
                        }
                        if (!info.FieldsInfo.Fields.ContainsKey("TaxonomyFields"))
                        {
                            info.FieldsInfo.Fields.Add("TaxonomyFields", needUpdateTaonoxyFields);
                        }
                        else
                        {
                            info.FieldsInfo.Fields["TaxonomyFields"] = needUpdateTaonoxyFields;
                        }
                    }
                    catch (NotImplementedException ex)
                    {
                        mLogger.Warn("Taxonomy Field is not support.Error Message:{0}", ex.ToString());
                    }
                    catch (Exception e)
                    {
                        if (e.Message.ToLowerInvariant().Contains("a term with the same default label and parent term"))
                        {
                            mParentWeb.Site.ReloadTaxonomySession();
                        }
                        mLogger.Warn("Restore Taxonomy field, Error Message:{0}", e.ToString());
                    }
                }
            }
        }

        private string NormalizeName(string termName)
        {
            if (termName == null)
            {
                return null;
            }
            Regex trimSpacesRegex = new Regex(@"\s+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            char tempChar = (char)0xff06;
            return trimSpacesRegex.Replace(termName, " ").Replace('&', tempChar);
        }

        public static Dictionary<string, object> AssembleBaseItemInfo(AveBaseItemInfo info, IAveList aveList)
        {
            Dictionary<string, object> docData = new Dictionary<string, object>();
            docData["FolderUrl"] = info.ParentFolderRelativeUrl;
            docData["WebUrl"] = info.ParentWebRelativeUrl;
            docData["ListTitle"] = info.ParentListTitle;
            //docData["ListId"] = info.ParentListId;
            docData["RestoreOption"] = info.RestoreOption;
            docData["DoclibRowId"] = info.OriginalRowId;
            docData["UIVersion"] = info.OriginalVersion;
            docData["Level"] = info.OriginalLevel;
            docData["DraftOwnerId"] = info.DraftOwnerId;
            docData["_ModerationStatus"] = info.ModerationStatus;
            docData["ModerationComments"] = info.ModerationComments;
            docData["CheckOutUserId"] = info.CheckoutUserId;
            docData["DeleteItem"] = info.SettingInfo.DELETE_ITEM;
            //SAAS-40066 处理文件或者文件夹前缀是空格的case. 目前folder, document都会用 docData["Title"]作为名字还原
            docData["Title"] = info.Name?.RemovePrefixSpace();
            docData["Size"] = info.DocumentSize;
            docData["HasStream"] = info.HasStream;
            docData["ServerRelativeUrl"] = info.ServerRelativeUrl;
            docData["HasPreCurrentVersion"] = info.HasPreCurrentVersion;
            docData["Id"] = info.GUID;
            docData["SKIP_IF_SAME_MODIFIEDTIME"] = info.SettingInfo.SKIP_IF_SAME_MODIFIEDTIME;
            docData["MOVE_ITEM_TO_CONFLICT_FOLDER"] = info.SettingInfo.MOVE_ITEM_TO_CONFLICT_FOLDER && info.SettingInfo.DELETE_ITEM;//将MOVE_ITEM_TO_CONFLICT_FOLDER属性封装一下，restore时需要；
            docData["OverwriteByLastModifiedTime"] = info.SettingInfo.OverWriteByModifiedTime;

            if (info.DocData != null && info.DocData.ContainsKey("RestoreSecurityOnly"))
            {
                docData["RestoreSecurityOnly"] = info.DocData["RestoreSecurityOnly"];
            }
            if (info.DocData != null && info.DocData.ContainsKey("BiggestVersionModified"))
            {
                docData["BiggestVersionModified"] = info.DocData["BiggestVersionModified"];
            }
            int desRowId = -1;
            if (info.MappingManager != null)
            {
                desRowId = info.MappingManager.SiteMappingManager.GetMappingItemId(info.ListId, info.OriginalRowId);
            }
            if (info.ParentId != Guid.Empty && (desRowId == -1 || desRowId == 0) && info is AveAttachmentInfo)
            {
                desRowId = aveList.ListItemGuidAndRowIdMappings.ContainsKey(info.ParentId.ToString()) ? aveList.ListItemGuidAndRowIdMappings[info.ParentId.ToString()] : -1;
            }
            docData["DestRowId"] = desRowId;
            AveListItemInfo itemInfo = info as AveListItemInfo;
            if (itemInfo != null)
            {
                docData["GUID"] = itemInfo.tp_Guid;
            }
            if (info.RestoringItem != null)
            {
                docData["IsNewCreated"] = info.RestoringItem.IsNewItem;
            }
            if (aveList != null)
            {
                docData["ListId"] = aveList.ID;
                docData["ListRootFolderServerRelativeUrl"] = aveList.RootFolder.ServerRelativeUrl;
            }
            return docData;
        }

        public Dictionary<string, object> ConvertFieldValuesToStringForHS(Dictionary<string, object> fieldValues, Dictionary<string, object> multipleLookupFieldValues)
        {
            var template = (int)BaseTemplate;
            return ConvertFieldValuesToString(fieldValues, multipleLookupFieldValues, template);
        }

        public static Dictionary<string, object> ConvertFieldValuesToString(Dictionary<string, object> fieldValues, Dictionary<string, object> multipleLookupFieldValues, int ListTemplate)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            //不是ContentTypeId类型的ContentType，Item Update的时候会抛异常，在此删除这种ContentType避免后面的异常
            if (fieldValues.ContainsKey("ContentType"))
            {
                AveFieldValueInfo fieldInfo = fieldValues["ContentType"] as AveFieldValueInfo;
                AveContentTypeId itemContentTypeId = fieldInfo.ColValue as AveContentTypeId;
                if (itemContentTypeId == null)
                {
                    fieldValues.Remove("ContentType");
                }
            }
            TrimFieldsByListTemplate(fieldValues, ListTemplate);
            foreach (KeyValuePair<string, object> kv in fieldValues)
            {
                AveFieldValueInfo fieldInfo = kv.Value as AveFieldValueInfo;
                if (fieldInfo != null && fieldInfo.ColValue != null && !fieldInfo.ColValue.GetType().IsAssignableFrom(typeof(IAveTaxonomyFieldValue)) && !fieldInfo.ColValue.GetType().IsAssignableFrom(typeof(IAveTaxonomyFieldValueCollection)))
                {
                    if (fieldInfo.FieldType == AveFieldType.URL)
                    {
                        AveFieldUrlValue tempUrlValue = null;
                        string currentKey = kv.Key;
                        if (kv.Key.EndsWith("#2", StringComparison.OrdinalIgnoreCase))
                        {
                            currentKey = kv.Key.Remove(kv.Key.IndexOf("#2", StringComparison.OrdinalIgnoreCase));
                            if (fieldValues.ContainsKey(currentKey))
                            {
                                tempUrlValue = ((AveFieldValueInfo)fieldValues[currentKey]).ColValue as AveFieldUrlValue;
                                if (tempUrlValue != null)
                                {
                                    tempUrlValue.Description = fieldInfo.ColValue.ToString();
                                }
                                else
                                {
                                    tempUrlValue = new AveFieldUrlValue();
                                    tempUrlValue.Description = fieldInfo.ColValue.ToString();
                                    fieldInfo.ColValue = tempUrlValue;
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (fieldValues.ContainsKey(currentKey + "#2"))
                            {
                                tempUrlValue = ((AveFieldValueInfo)fieldValues[currentKey + "#2"]).ColValue as AveFieldUrlValue;
                                if (tempUrlValue != null)
                                {
                                    tempUrlValue.Url = fieldInfo.ColValue.ToString();
                                }
                                else
                                {
                                    tempUrlValue = new AveFieldUrlValue();
                                    tempUrlValue.Url = fieldInfo.ColValue.ToString();
                                    fieldInfo.ColValue = tempUrlValue;
                                    continue;
                                }
                            }
                            else
                            {
                                tempUrlValue = new AveFieldUrlValue();
                                tempUrlValue.Url = fieldInfo.ColValue.ToString();
                                tempUrlValue.Description = fieldInfo.ColValue.ToString();
                            }
                        }
                        dic[currentKey] = tempUrlValue.ToString();
                    }
                    else if ((fieldInfo.ColValue.GetType() == typeof(DateTime)))
                    {
                        dic[kv.Key] = fieldInfo.ColValue;
                    }
                    else if ((fieldInfo.ColValue.GetType() == typeof(string[])))
                    {
                        dic[kv.Key] = fieldInfo.ColValue;
                    }
                    else
                    {
                        dic[kv.Key] = fieldInfo.ColValue.ToString();
                    }
                }
            }
            if (multipleLookupFieldValues != null)
            {
                foreach (KeyValuePair<string, object> kv in multipleLookupFieldValues)
                {
                    dic[kv.Key] = kv.Value;
                }
            }
            return dic;
        }

        private static void TrimFieldsByListTemplate(Dictionary<string, object> fieldValues, int ListTemplate)
        {
            string[] needRemoveFields;
            switch (ListTemplate)
            {
                case (int)AveListTemplateType.SolutionCatalog:
                    needRemoveFields = new string[] { "SolutionGalleryItemId", "ResourceQuota" };
                    for (int i = 0; i < needRemoveFields.Length; i++)
                    {
                        fieldValues.Remove(needRemoveFields[i]);
                    }
                    break;
                case (int)AveListTemplateType.WebPageLibrary:
                    if (fieldValues.ContainsKey("WikiField"))
                    {
                        EsacpeWikipageLinkCharacter(fieldValues, ListTemplate);
                    }
                    break;
                case 500://CategoriesList in Community Site
                    if (WrapperConfiguration.UpdateColumnWithEventReciever)
                    {
                        needRemoveFields = new string[] { "TopicCount", "ReplyCount" };
                        for (int i = 0; i < needRemoveFields.Length; i++)
                        {
                            string fieldName = needRemoveFields[i];
                            if (fieldValues.ContainsKey(fieldName))
                            {
                                (fieldValues[fieldName] as AveFieldValueInfo).ColValue = 0;
                            }
                            //fieldValues.Remove(needRemoveFields[i]);
                        }
                    }
                    break;
                case (int)AveListTemplateType.CommunityMember:
                    if (WrapperConfiguration.UpdateColumnWithEventReciever)
                    {
                        needRemoveFields = new string[] { "NumberOfDiscussions", "NumberOfReplies" };
                        for (int i = 0; i < needRemoveFields.Length; i++)
                        {
                            string fieldName = needRemoveFields[i];
                            if (fieldValues.ContainsKey(fieldName))
                            {
                                (fieldValues[fieldName] as AveFieldValueInfo).ColValue = 0;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private static void EsacpeWikipageLinkCharacter(Dictionary<string, object> fieldValues, int listTemplate)
        {
            AveFieldValueInfo wikiFieldValue = fieldValues["WikiField"] as AveFieldValueInfo;
            if (wikiFieldValue == null)
            {
                return;
            }

            string wikiPageContent = wikiFieldValue.ColValue as string;
            if (string.IsNullOrEmpty(wikiPageContent))
            {
                return;
            }

            wikiFieldValue.ColValue = new Regex(@"\[\[[\w\W]*?\]\]").Replace(wikiPageContent, new MatchEvaluator((match) =>
            {
                if (!string.IsNullOrEmpty(match.Value))
                {
                    return @"\" + match.Value.TrimEnd(']') + @"\]]";
                }
                return match.Value;
            }));
        }

        internal void AssemblyMeetingItemInfo(AveListItemInfo itemInfo, Dictionary<string, object> userData, Dictionary<string, object> docData)
        {
            if (userData.ContainsKey("Title"))
            {
                docData["Title"] = userData["Title"];
            }
            int eventType = 0;
            if (userData.ContainsKey("EventType"))
            {
                eventType = (int)userData["EventType"];
                docData["EventType"] = eventType;
            }
            if (userData.ContainsKey("TimeZone"))
            {
                docData["TimeZone"] = (int)userData["TimeZone"];
            }
            else if (userData.ContainsKey("UID") && (eventType == 2 || eventType == 3))
            {
                docData["UID"] = userData["UID"];
            }
            if (userData.ContainsKey("EventDate"))
            {
                docData["EventDate"] = userData["EventDate"];
            }
            if (userData.ContainsKey("Duration"))
            {
                docData["Duration"] = (int)userData["Duration"];
            }
            if (userData.ContainsKey("EndDate"))
            {
                docData["EndDate"] = userData["EndDate"];
            }
            if (userData.ContainsKey("RecurrenceID"))
            {
                docData["RecurrenceID"] = userData["RecurrenceID"];
            }
            if (userData.ContainsKey("UID"))
            {
                docData["UID"] = userData["UID"];
            }
            if (userData.ContainsKey("Location"))
            {
                docData["Location"] = userData["Location"];
            }
            if (userData.ContainsKey("RecurrenceData"))
            {
                docData["RecurrenceData"] = userData["RecurrenceData"];
            }
            if (userData.ContainsKey("fAllDayEvent"))
            {
                docData["fAllDayEvent"] = userData["fAllDayEvent"];
            }
            if (userData.ContainsKey("fRecurrence"))
            {
                docData["fRecurrence"] = userData["fRecurrence"];
            }
            if (userData.ContainsKey("RRule"))
            {
                docData["RRule"] = userData["RRule"];
            }
            if (userData.ContainsKey("ExRule"))
            {
                docData["ExRule"] = userData["ExRule"];
            }
            if (userData.ContainsKey("SuppressUntil"))
            {
                docData["SuppressUntil"] = userData["SuppressUntil"];
            }
            if (userData.ContainsKey("IsOrphaned"))
            {
                //DOC-67486，在此处设置listItem["IsOrphaned"]=true或者不设置该值，都会导致listItem.Update抛出异常
                //所以在此处设置listItem["IsOrphaned"] = false，如果是true在之后更新field的时候会更新正确。
                //listItem["IsOrphaned"] = userData["IsOrphaned"];
                docData["IsOrphaned"] = false;
            }
            if (userData.ContainsKey("IsException"))
            {
                docData["IsException"] = userData["IsException"];
            }
            if (userData.ContainsKey("IsDetached"))
            {
                docData["IsDetached"] = userData["IsDetached"];
            }
            if (userData.ContainsKey("Sequence"))
            {
                docData["Sequence"] = userData["Sequence"];
            }
            if (userData.ContainsKey("DTStamp"))
            {
                docData["DTStamp"] = userData["DTStamp"];
            }
            if (userData.ContainsKey("#tp_InstanceID"))
            {
                docData["InstanceID"] = userData["#tp_InstanceID"];
            }
            if (itemInfo != null)
            {
                if (userData.ContainsKey("EventUID"))
                {
                    docData["EventUID"] = userData["EventUID"];
                    string[] idparts = userData["EventUID"].ToString().Split(':');
                    if (idparts.Length == 5 && itemInfo.MappingManager.SiteMappingManager.ListIdMapping.ContainsKey(new Guid(idparts[2])))
                    {
                        docData["EventUID"] = userData["EventUID"].ToString().Replace(idparts[2], itemInfo.MappingManager.SiteMappingManager.ListIdMapping[new Guid(idparts[2])].ToString("B"));
                    }
                }
                if (userData.ContainsKey("Organizer"))
                {
                    if (itemInfo != null)
                    {
                        docData["Organizer"] = itemInfo.Extension.PrincipalId;
                    }
                }
                if (userData.ContainsKey("EventUrl") && userData.ContainsKey("EventUrl#2"))
                {
                    docData["EventUrl"] = userData["EventUrl"];
                    docData["EventUrl#2"] = userData["EventUrl#2"];
                    docData["FieldUrlValue"] = itemInfo.Extension.FieldUrlValue;
                }
            }
        }

        public IAveRelatedFieldCollection GetRelatedFields()
        {
            Dictionary<string, object> relatedFieldProperties = mRequest.GetRelatedFields(mParentWeb.ServerRelativeUrl, this.Title, this.ID);
            AveRelatedFieldCollection relatedFieldCollection = new AveRelatedFieldCollection(mRequest, this, relatedFieldProperties);
            return relatedFieldCollection;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")]
        public Dictionary<Guid, Guid> GetAlerts(string url, int itemId, AveSPAlertHostType hostType)
        {
            Dictionary<Guid, Guid> listAlerts = new Dictionary<Guid, Guid>();
            Dictionary<string, object> webAlerts = mRequest.GetAlerts(mParentWeb.ServerRelativeUrl);
            if (webAlerts.Count > 0)
            {
                var alerts = webAlerts.GetChildren();
                foreach (var alert in alerts)
                {
                    switch (hostType)
                    {
                        case AveSPAlertHostType.List:
                        case AveSPAlertHostType.Folder:
                            if (alert["ListID"].Equals(this.ID) && !alert.ContainsKey("ItemID"))
                            {
                                Dictionary<string, object> alertProperties = alert["Properties" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                                if (alertProperties != null && alertProperties.ContainsKey("alertoldid"))
                                {
                                    listAlerts.Add(new Guid(alertProperties["alertoldid"].ToString()), new Guid(alert["ID"].ToString()));
                                }
                            }
                            break;
                        case AveSPAlertHostType.Doc:
                        case AveSPAlertHostType.Item:
                            if (alert["ListID"].Equals(this.ID) && alert["ItemID"].Equals(itemId))
                            {
                                Dictionary<string, object> alertProperties = alert["Properties" + AveObjectModelConstant.ObjectPropertySuffix] as Dictionary<string, object>;
                                if (alertProperties != null && alertProperties.ContainsKey("alertoldid"))
                                {
                                    listAlerts.Add(new Guid(alertProperties["alertoldid"].ToString()), new Guid(alert["ID"].ToString()));
                                }
                            }
                            break;
                        default:
                            break;
                    }

                }
            }

            return listAlerts;
        }

        public Guid Recycle()
        {
            return mRequest.RecycleList(this.ParentWeb.ServerRelativeUrl, this.Title, this.ID, (int)this.BaseTemplate, this.EntityTypeName, this.TemplateFeatureId.ToString());
        }

        public bool IsACCSRVSystemList()
        {
            bool isMSysASOSystemList = false;
            bool isMacroSystemList = false;
            IAveWeb web = this.ParentWeb;
            if (web != null && web.WebTemplate != null && web.WebTemplate.Equals("ACCSRV", StringComparison.OrdinalIgnoreCase))
            {
                #region IsMSysASOSystemList
                if (web.AllProperties.ContainsKey("___MSysASOId"))
                {
                    isMSysASOSystemList = this.ID.Equals(new Guid((string)web.AllProperties["___MSysASOId"]));
                }
                else
                {
                    isMSysASOSystemList = this.Title.Equals("MSysASO", StringComparison.OrdinalIgnoreCase);
                }
                #endregion
                #region IsMacroSystemList
                if (!isMSysASOSystemList)
                    isMacroSystemList = this.Title.Equals("Macro", StringComparison.OrdinalIgnoreCase);
                #endregion
            }
            return isMSysASOSystemList || isMacroSystemList;
        }

        public void UpdateWorkflowAssociation(IAveWorkflowAssociation association)
        {
            ((AveWorkflowAssociation)association).Update();
        }

        public IAveWorkflowAssociation AddWorkflowAssociation(IAveWorkflowAssociation association)
        {
            Dictionary<string, object> props = mRequest.CreateListAssociation(ParentWeb.ServerRelativeUrl, ID, "web.workflowTemplates", association);
            return new AveWorkflowAssociation(ParentWeb, this, string.Empty, props);
        }

        public ulong Flags
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Flags"))
                {
                    GetPropertiesFromSchemaXml();
                }
                return base.DataCache.GetProperty<ulong>("Flags");
            }
        }

        public IAveAudit Audit
        {
            get { throw new NotImplementedException(); }
        }


        public void Reload()
        {
            this.DataCache.RemoveProperty("Fields");
            this.DataCache.RemoveProperty("ContentTypes");
        }

        public void ReloadListWorkflowAssociations()
        {
            this.DataCache.RemoveProperty("WorkflowAssociations"); 
        }
        public void SetWorkflowsAssociated(bool bWorkflowsAssociated)
        {
            throw new NotImplementedException();
        }

        public IAveListItem GetItemByIdSelectedFields(int id, params string[] fields)
        {
            AveCamlQuery query = new AveCamlQuery();
            StringBuilder builder = new StringBuilder("");
            if (fields.Length > 0)
            {
                builder.Append("<ViewFields>");
                foreach (string str in fields)
                {
                    if (str != null)
                    {
                        builder.Append("<FieldRef Name=\"" + str + "\"/>");
                    }
                }
                builder.Append("</ViewFields>");
            }
            if (id < 0)
            {
                query.ViewXml = "<View><Query><Where><Eq><FieldRef Name=\"BdcIdentity\"></FieldRef><Value>" + id + "</Value></Eq>" + builder.ToString() + "</Where></Query></View>";
            }
            else
            {
                query.ViewXml = "<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"ID\"></FieldRef><Value Type=\"Integer\">" + id + "</Value></Eq></Where></Query>" + builder.ToString() + "</View>";
            }
            Dictionary<string, object> items = mRequest.GetItemsByIdSelectedFields(this.mParentWeb.ServerRelativeUrl, this.Title, this.ID, query.ToStringArray());

            AveListItemCollection listItemsCollection = new AveListItemCollection(mRequest, this.ParentWeb, this, false, items);

            if (listItemsCollection.Count > 0)
            {
                return listItemsCollection[0];
            }
            return null;
        }

        public void UpdateListRssSetting(Dictionary<string, object> updateProp)
        {
            mRequest.UpdateListRssSetting(this.ParentWebUrl, this.ID, updateProp);
        }



        private void GetThisPropertie()
        {
            if (this.ParentWeb.AppInstanceId == Guid.Empty)
            {
                Dictionary<string, object> advancedProperties = mRequest.GetListAdvancedSettingProperties(this.ParentWebUrl, this.ID);
                base.DataCache.AddPropertyies(advancedProperties);
            }
        }
        /// <summary>
        /// 通过client API无法获取到,但是在list SchemaXml中有的属性,可以通过此方法获得
        /// </summary>
        private void GetPropertiesFromSchemaXml()
        {
            XmlDocument tempDocument = null;
            Dictionary<string, object> properties = new Dictionary<string, object>();
            Dictionary<string, string> propertiesPair = new Dictionary<string, string>();
            propertiesPair["Flags"] = "ulong";
            foreach (KeyValuePair<string, string> tempProperty in propertiesPair)
            {
                if (tempDocument == null)
                {
                    tempDocument = new XmlDocument();
                    tempDocument.LoadXml(this.SchemaXml);
                }
                XmlElement rootNode = tempDocument.DocumentElement;
                if (rootNode.HasAttribute("Flags") && !string.IsNullOrEmpty(rootNode.GetAttribute("Flags")))
                {
                    object value = GetValueFromType(tempProperty.Value, rootNode.GetAttribute("Flags"));
                    if (value != null)
                    {
                        properties[tempProperty.Key] = value;
                    }
                }
            }
            if (properties.Count > 0)
            {
                base.DataCache.AddPropertyies(properties);
            }
        }

        private object GetValueFromType(string type, string strValue)
        {
            object value = null;
            try
            {
                switch (type)
                {
                    case "ulong":
                        value = Convert.ToUInt64(strValue);
                        break;
                    case "boolean":
                        value = Convert.ToBoolean(strValue);
                        break;
                    case "string":
                    default:
                        value = strValue;
                        break;
                }
            }
            catch (Exception ex)
            {
                mLogger.Info(string.Format("Can not convert to certain type.Type:{0},value:{1},Messages:{2}", type, strValue, ex.ToString()));
            }
            return value;
        }
        private void GetListGeneralSettings()
        {
            if (this.BaseType.Equals(AveBaseType.Survey) || this.BaseTemplate.Equals(AveListTemplateType.Events)) //Get Survey and Calendar list general setting.
            {
                Dictionary<string, object> generalProperties = mRequest.GetListGeneralProperties(this.ParentWebUrl, this.ID);
                base.DataCache.AddPropertyies(generalProperties);
            }
        }

        private void GetListProperties()
        {
            if (this.ParentWeb.AppInstanceId == Guid.Empty)
            {
                Dictionary<string, object> limitedProperties = mRequest.GetListVersionLimited(this.ParentWebUrl, this.ID);
                base.DataCache.AddPropertyies(limitedProperties);
                GetListGeneralSettings();
                if (this.DefaultView != null)
                {
                    Dictionary<string, object> editViewProperties = mRequest.GetListEditViewSettingProperties(this.ParentWebUrl, this.Title, this.ID, this.DefaultView.ID);
                    base.DataCache.AddPropertyies(editViewProperties);
                }
                if (this.ParentWeb != null && this.ParentWeb.Features[new Guid("7201d6a4-a5d3-49a1-8c19-19c4bac6e668")] != null)
                {
                    Dictionary<string, object> navigationSettings = mRequest.GetMetadataNavigationSettings(this.ParentWebUrl, this.ID, this.Title);
                    base.DataCache.AddPropertyies(navigationSettings);
                }
                Dictionary<string, object> metadataSettings = mRequest.GetMetadataListFieldSettings(this.ParentWebUrl, this.Title, this.ID);
                base.DataCache.AddPropertyies(metadataSettings);
                Dictionary<string, object> perLocationSettings = mRequest.GetPerLocationViewSettings(this.ParentWebUrl, this.ID);
                base.DataCache.AddPropertyies(perLocationSettings);
                Dictionary<string, object> accessRequestsSetting = mRequest.GetListAccessRequestsSettingProperties(this.ParentWebUrl, this.ID);
                base.DataCache.AddPropertyies(accessRequestsSetting);
            }
        }
        public void GetViews(Dictionary<Guid, List<AveViewInfo>> viewCache)
        {
            viewCache.Clear();
            foreach (IAveView iView in this.Views)
            {
                AveView view = iView as AveView;
                if (iView != null)
                {
                    //Filter out form page in views.  IAveForm.Url is ServerRelativeUrl actually.
                    if (this.Forms.Any(form => form.Url.Equals(iView.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase)))
                    {
                        mLogger.Info("Filter out this form in views. Url: {0}", view.Url);
                        continue;
                    }
                    Guid PageUrlID = view.PageUrlID;
                    if (!viewCache.ContainsKey(PageUrlID))
                    {
                        viewCache.Add(PageUrlID, new List<AveViewInfo>());
                    }
                    List<AveViewInfo> views = viewCache[PageUrlID];
                    AveViewInfo viewInfo = new AveViewInfo();
                    viewInfo.BaseViewId = Convert.ToByte(view.BaseViewId);
                    viewInfo.Id = view.ID;
                    viewInfo.Title = view.Title;
                    viewInfo.IsDefaultView = view.DefaultView;
                    viewInfo.IsPersonal = view.PersonalView;
                    viewInfo.ViewType = AveViewInfo.GetViewType(view.Type);
                    viewInfo.Hidden = view.Hidden;
                    viewInfo.Scope = view.Scope.ToString();
                    viewInfo.RowLimit = view.RowLimit;
                    viewInfo.IsMobileView = view.MobileView;
                    viewInfo.IsDefaultMobileView = view.MobileDefaultView;
                    viewInfo.ViewData = view.ViewData;
                    viewInfo.ContentTypeId = view.ContentTypeId.ToString();
                    viewInfo.ListViewXml = view.ListViewXml;
                    views.Add(viewInfo);
                }
            }
        }

        public bool EnableKeywordsField
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableKeywordsField");
            }
            set
            {
                if (!EnableKeywordsField.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableKeywordsField", value);
                }
            }
        }

        public bool KeywordsFieldExistsInContentTypes
        {
            get
            {
                return base.DataCache.GetProperty<bool>("KeywordsFieldExistsInContentTypes");
            }
            set
            {
                if (!KeywordsFieldExistsInContentTypes.Equals(value))
                {
                    base.DataCache.AddChangedProperty("KeywordsFieldExistsInContentTypes", value);
                }
            }
        }

        public bool EnableMetadataPromotion
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EnableMetadataPromotion");
            }
            set
            {
                if (!EnableMetadataPromotion.Equals(value))
                {
                    base.DataCache.AddChangedProperty("EnableMetadataPromotion", value);
                }
            }
        }

        public bool RequestAccessEnabled
        {
            get
            {
                return base.DataCache.GetProperty<bool>("RequestAccessEnabled");
            }
            set
            {
                if (!RequestAccessEnabled.Equals(value))
                {
                    base.DataCache.AddChangedProperty("RequestAccessEnabled", value);
                }
            }
        }

        public override IAveSecurableObjectImpl SecurableObjectImpl
        {
            get
            {
                if (this.HasUniqueRoleAssignments)
                {
                    if (mSecurableObjectImpl == null)
                    {
                        mSecurableObjectImpl = new AveSecurableObjectImpl(Guid.NewGuid(), this.RoleAssignments);
                    }
                    return mSecurableObjectImpl;
                }
                else
                {
                    return this.ParentWeb.SecurableObjectImpl;
                }
            }
        }

        public Collection<IAveSPListItemInfo> GetItemsWithUniquePermissions()
        {
            throw new NotImplementedException();
        }
        public List<int> GetItemsByColumnValue(string columnDisplayName, string value)
        {
            return null;
        }

        public int Version
        {
            get { throw new NotImplementedException(); }
        }

        public void CleanListData()
        {
            AveClientCacheHandler.CleanSchemaXml(mParentWeb.CacheHandlerId, this.ID.ToString());
            CleanCollectionData();
            mItemIdMapping = null;
            if (_sqliteCache != null)
            {
                _sqliteCache.Dispose();
            }
            _sqliteCacheErrorMessage = null;
        }

        private void CleanCollectionData()
        {
            this.DataCache.RemoveProperty("Fields");
            this.DataCache.RemoveProperty("ContentTypes");
            this.DataCache.RemoveProperty("Views");
            this.DataCache.RemoveProperty("WorkflowAssociations");  //SAAS-21766 释放list的workflow associations和role assignments
            this.DataCache.RemoveProperty("RoleAssignments");
        }

        public bool CheckItemIsExist(int rowId)
        {
            return false;
        }

        public bool CheckItemIsExist(string rowId, Guid itemId)
        {
            IAveListItem item = (Items as AveListItemCollection).GetItemByGuid(itemId);
            if (item == null)
            {
                throw new Exception("Item not find");
            }
            return true;
        }

        public void UpdateListCreated(DateTime created)
        {//Client 不需要实现
        }

        public bool CheckIfHasAlertsOfSpecificConditions(int? itemId, AveEventType eventType, int userId, AveAlertFrequency frequency)
        {
            return false;
        }

        public void RestoreSolutionStatus(IList<AveSolutionInfo> sandboxSolutions)
        {
            mRequest.RestoreSolutionStatus(mParentWeb.Site.ServerRelativeUrl, sandboxSolutions);
        }

        public bool? IsConnectorList { get; set; }

        public IAveInformationRightsManagementSettings InformationRightsManagementSettings
        {
            get
            {
                return DataCache.EnsureLoadProperty("InformationRightsManagementSettings",
                    () =>
                    {
                        Dictionary<string, object> settings = mRequest.GetListInformationRightsManagementSettings(this.ParentWebUrl, this.ID);
                        var irmSetting = new AveInformationRightsManagementSettings(mRequest, this, settings);
                        return irmSetting;
                    });
            }
        }

        #region Add to operate Change Log ** We will implement this in SP2013 first **
        public IAveChangeCollection GetChanges()
        {
            throw new NotImplementedException();
        }

        public IAveChangeCollection GetChanges(IAveChangeQuery query)
        {
            var changeCollectionDic = mRequest.GetListChangesByQuery(mParentWeb.ServerRelativeUrl, this.ID, this.Title,
                (query as AveChangeQuery).DataCache.GetPropertyCache());
            return new AveChangeCollection(changeCollectionDic);
        }

        public IAveChangeCollection GetChanges(IAveChangeToken changeToken)
        {
            throw new NotImplementedException();
        }

        public IAveChangeCollection GetChanges(IAveChangeToken changeToken, IAveChangeToken changeTokenEnd)
        {
            throw new NotImplementedException();
        }

        public IAveFolder GetRootFolder()
        {
            Dictionary<string, object> folderProperties = base.DataCache.GetProperty<Dictionary<string, object>>("RootFolder" + AveObjectModelConstant.ObjectPropertySuffix);
            #region 在此处加载Rss Properties会造成效率问题，且Rss Properties是通过模拟Http request支持的，对于这类属性可以不支持，所以这个属性暂时去掉不支持。
            //try
            //{
            //    base.DataCache.AddPropertyies(mRequest.GetListRssProperties(this.ParentWebUrl, this.ID));
            //}
            //catch (AveSecurityTrimingException ex)
            //{
            //    mLogger.Warn("An error occurred while get list rssproperties.listid: {0}", this.ID, ex);
            //    //throw ex;
            //    //contribute level没有权限取得ListRssProperty
            //}
            #endregion
            AveFolder rootFolder = new AveFolder(mRequest, mParentWeb, this, null, folderProperties);
            //base.DataCache.PropertiesCache["RootFolder"] = rootFolder;
            if (base.DataCache.IsPropertyAvailable("RootFolderRssProperties"))
            {
                Hashtable properties = base.DataCache.GetProperty<Hashtable>("RootFolderRssProperties");
                rootFolder.DataCache.AddProperty("Properties",new AveCustomHashtable(properties, rootFolder.SetChangeProperty));
            }

            return rootFolder;
        }
        #endregion


        public void ReorderListFields(List<string> mappedSourceFields)
        {
            mRequest.ReorderListFields(this.ParentWebUrl, this.ID, mappedSourceFields);
        }

        public void SaveNintexForm(string formXml, string contentTypeId)
        {
                mRequest.SaveNintexForm(formXml, this.mParentWeb.Url, ID, contentTypeId);
        }
        public void PublishNintexForm(string contentTypeId)
        {
            mRequest.PublishNintexForm(this.mParentWeb.Url, ID, contentTypeId);
            
        }
        public Stream ExportNintexForm(string contentTypeId)
        {
                return mRequest.ExportNintexForm(this.mParentWeb.Url, ID, contentTypeId);
        }

        public List<int> GetItemsIdWithUniquePermissions()
        {
            bool isDocLib = !(this.BaseTemplate == AveListTemplateType.GenericList);
            return mRequest.GetItemsIdWithUniquePermissions(ParentWebUrl, this.mParentWeb.Url, this.ID, isDocLib);
        }

        public Tuple<Dictionary<string, int>, Dictionary<int, Guid>> LoadExistingItemIdUrlMapping()
        {
            return mRequest.LoadListItemIDUrlCache(ParentWebUrl,ID);
        }

        /*public Dictionary<int, List<int>> GetUniquePermissionItemsIDInEachFolder()
        {
            return mRequest.GetUniquePermissionItemsIDInEachFolder(this.ParentWeb.ServerRelativeUrl, this.ID);
        }*/

        public string GetViewSpotlightItemsMapping()
        {
            if (string.IsNullOrEmpty(mSpotlightInfoMappingStr))
            {
                lock (mSpotlightInfoLock)
                {
                    if (string.IsNullOrEmpty(mSpotlightInfoMappingStr))
                    {
                        try
                        {
                            // get all item rowids, then query all the items with one request
                            List<int> itemRowIdList = new List<int>();
                            Dictionary<int, string> tempSpotlightInfoMapping = new Dictionary<int, string>();
                            foreach (IAveView view in this.Views)
                            {
                                try
                                {
                                    string listViewXml = view.ListViewXml;
                                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                                    doc.LoadXml(listViewXml);
                                    System.Xml.XmlNode spotlightInfoNode = doc.SelectSingleNode("View/SpotlightInfo");
                                    if (spotlightInfoNode == null)
                                    {
                                        continue;
                                    }
                                    // spot light format: 
                                    // |folderId=itemId;itemId;itemId|folderId=itemId;|
                                    string spotlightInfoStr = "|";
                                    int itemId;
                                    foreach (string spotlight in spotlightInfoNode.InnerText.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                                    {
                                        if (spotlight.Contains('='))
                                        {
                                            string folderRowId = spotlight.Substring(0, spotlight.IndexOf("="));
                                            string itemRowIds = spotlight.Substring(spotlight.IndexOf("=") + 1);
                                            if (folderRowId == "0")
                                            {
                                                if (!tempSpotlightInfoMapping.ContainsKey(0))
                                                    tempSpotlightInfoMapping.Add(0, this.RootFolder.ServerRelativeUrl);
                                            }
                                            else
                                            {
                                                itemId = int.Parse(folderRowId);
                                                if (!itemRowIdList.Contains(itemId))
                                                {
                                                    itemRowIdList.Add(itemId);
                                                }
                                            }
                                            int sourceItemId = 0;
                                            foreach (var itemRowId in itemRowIds.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                                            {
                                                sourceItemId = int.Parse(itemRowId);
                                                if (!itemRowIdList.Contains(sourceItemId))
                                                {
                                                    itemRowIdList.Add(sourceItemId);
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    mLogger.Warn("Error occurred while retrieving spotlightinfo from list viewxml. Ex:{0}", ex.ToString());
                                }
                            }
                            if (itemRowIdList.Count <= 0)
                            {
                                return null;
                            }
                            int totalCount = itemRowIdList.Count;
                            int batchSize = 50;
                            int i = 0;
                            while (totalCount > i * batchSize)
                            {
                                IEnumerable<int> rowIds = itemRowIdList.Skip(i * batchSize).Take(batchSize);
                                GetItemsMappingByCAMLQuery(rowIds.ToList(), tempSpotlightInfoMapping);
                                i++;
                            }
                            mSpotlightInfoMappingStr = SerializerHelper.SerializeToBase64String(tempSpotlightInfoMapping);
                        }
                        catch (Exception ex)
                        {
                            mLogger.Warn("Error occurred while retrieving spotlightinfo from list viewxml. Ex:{0}", ex.ToString());
                        }
                    }
                }
            }
            return mSpotlightInfoMappingStr;
        }

        private void GetItemsMappingByCAMLQuery(List<int> itemIds, Dictionary<int, string> spotlightInfoMapping)
        {
            try
            {
                AveCamlQuery spotlightInfoItemsQuery = new AveCamlQuery();
                //build query based on the row ids 
                string queryValues = string.Empty;
                itemIds.ForEach(id => queryValues += "<Value Type='Counter'>" + id + "</Value>");
                string queryXml = "<Query><Where><In><FieldRef Name='ID'/><Values>" + queryValues + "</Values></In></Where></Query>";
                string viewFields = "<ViewFields><FieldRef Mame='ID'/></ViewFields>";
                string viewXMl = "<View Scope='RecursiveAll'>" + queryXml + viewFields + "</View>";
                spotlightInfoItemsQuery.ViewXml = viewXMl;
                mLogger.Log(AveLogLevel.INFO, "Retireve spotlightinfo items by query, querystring:" + viewXMl);
                IAveListItemCollection items = this.GetItems(spotlightInfoItemsQuery);
                mLogger.Log(AveLogLevel.INFO, "{0} items retrieved", items.Count);
                foreach (var item in items)
                {
                    if (item["FSObjType"].ToString().Equals("1"))
                    {
                        spotlightInfoMapping.Add((int)item["ID"], "F" + this.ParentWeb.ServerRelativeUrl.TrimEnd('/') + "/" + item.Url);
                    }
                    else
                    {
                        spotlightInfoMapping.Add((int)item["ID"], "D" + this.ParentWeb.ServerRelativeUrl.TrimEnd('/') + "/" + item.Url);
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Log(AveLogLevel.WARN, "Error occurred while get items by CAMLQuery, ex:{0}", ex.ToString());
            }
        }

        public AveComplianceTagInfo GetListComplianceTag()
        {
            return mRequest.GetListComplianceTag(this.ParentWeb.Url, this.RootFolder.ServerRelativeUrl);
        }
        public void SetListComplianceTag(AveComplianceTagInfo info)
        {
            mRequest.SetListComplianceTag(this.ParentWeb.Url, this.RootFolder.ServerRelativeUrl, info);
        }

        /*public Dictionary<int, KeyValuePair<int, List<int>>> GetFoldersIncludeUniquePermissionSubItemsOrFolders()
        {
            return mRequest.GetFoldersIncludeUniquePermissionSubItemsOrFolders(this.ParentWeb.ServerRelativeUrl, this.ID);
        }*/

        public void DeclareItemsByRowIds(List<int> rowIds)
        {
            if (rowIds.IsNullOrEmpty()) return;
            mRequest.DeclareItemsByRowIds(this.ParentWeb.ServerRelativeUrl, this.ID, rowIds);
        }
        public void DeleteItemsByRowIds(Dictionary<int,long> rowIdsWithModifiedTime, Dictionary<int, long> rowIdsWithTimeLastModified)
        {
            mRequest.DeleteItemsByRowIds(this.ParentWeb.ServerRelativeUrl, this.ID, rowIdsWithModifiedTime, rowIdsWithTimeLastModified);
        }
        public void DeleteItemsByRowIds(List<int> rowIds)
        {
            if (rowIds.IsNullOrEmpty()) return;
            mRequest.DeleteItemsByRowIds(this.ParentWeb.ServerRelativeUrl, this.ID, rowIds);
        }

        public void SetComplianceTagOnBulkItems(List<int> itemIds, string complianceTagValue)
        {
            mRequest.SetComplianceTagOnBulkItems(this.ParentWeb.Url, this.ParentWeb.ID, this.ID, itemIds, complianceTagValue);
        }

        public void InitSqliteCacheInfo(string jobId, int aveListSqliteCacheTypes)
        {
            JobIdForSqliteCache = jobId;
            AveListSqliteCacheTypes = aveListSqliteCacheTypes;
            NeedSqliteDB4Cache = true;
        }
    }
}
