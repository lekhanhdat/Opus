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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource;
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon.Utility;
using System.Diagnostics;
using System.IO.Hashing;
using DocumentFormat.OpenXml.Drawing;

namespace AvePoint.Wrapper.Backup
{
    [AveCodeReview("2012/03/1", "gqsun@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1 }, "ADO-26504", true)]
    public class AveSPItem
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private AveItemType mItemType;
        private IAveBackupRestoreQueryService mQueryService;
        private AveSPListFieldCollection mFields;
        private bool mFirstTime = true;
        private AveSPList mAveSPList;

        private AveStorageInfo mStorageInfo = null;
        private byte[] mRbsId = null;
        private bool mIsBackupLinkForArchivedData;
        public AveItemDataCache DataCache = new AveItemDataCache();
        public Dictionary<string, object> UserDataCache = null;
        public Dictionary<string, object> DocDataCache = null;

        public bool UserDatajunctionCacheInited { get; private set; }

        private List<Dictionary<string, object>> mUserDatajunctionCache = null;

        public List<Dictionary<string, object>> UserDatajunctionCache
        {
            get
            {
                if (!UserDatajunctionCacheInited)
                {
                    UserDatajunctionCacheInited = true;
                    mUserDatajunctionCache = GetUserDataJunction();
                }
                return mUserDatajunctionCache;
            }
        }

        public List<AveRoleAssignmentInfo> RoleAssignmentCache = null;
        public List<Dictionary<string, object>> ImmedSubscriptionsCache = null;
        public List<Dictionary<string, object>> SchedSubscriptionsCache = null;
        private List<AveWebPartBaseInfo> mWebPartInfos = null;
        private AveBaseItemInfo mBaseItemInfo = null;
        private Dictionary<string, object> oldUserData = new Dictionary<string, object>(); // // used to save data in ALLUserData
        private AveSPSite mAveParentSite;
        private bool mBackupPagesFullContent = true;
        public bool IsPRItemBackup = false;

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }

        public AveBaseItemInfo BaseItemInfo
        {
            get { return mBaseItemInfo; }
        }

        public IAveItem mItem = null;

        public IAveItem Item
        {
            get { return mItem; }
        }

        private bool mIsThirdStub = false;

        public bool IsThirdStub
        {
            get { return mIsThirdStub; }
        }

        private bool mIsSpecailData = false;

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "File extension")]
        public bool IsSpecialData
        {
            get
            {
                using (AvePerformanceScope s1 = new AvePerformanceScope("Backup.AveSPItem.IsSpecialData"))
                {
                    //if (this.Item != null && this.Item.ListItem != null && !String.IsNullOrEmpty(this.Item.ListItem.Name) && Path.GetExtension(this.Item.ListItem.Name).Equals(".stp", StringComparison.OrdinalIgnoreCase))
                    if (!String.IsNullOrEmpty(this.BaseItemInfo.Name) && System.IO.Path.GetExtension(this.BaseItemInfo.Name).Equals(".stp", StringComparison.OrdinalIgnoreCase))
                    {
                        mIsSpecailData = true;
                    }
                    return mIsSpecailData;
                }
            }
        }

        private Nullable<AveStorageType> mStorageType = null;

        public AveStorageType StorageType
        {
            get
            {
                if (!mStorageType.HasValue)
                {
                    using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.StorageType"))
                    {
                        mStorageType = AveStorageType.None;
                        if (!IsPRItemBackup)
                        {
                            if (AveSPUtility.IsRbsArchivedData(RbsId))
                            {
                                //if (this.AveSPList.ParentWeb.ParentSite.EnableDocAveRBS)
                                if (this.RBSStubInfo != null)
                                {
                                    mStorageType = AveStorageType.RBS;
                                }
                                else
                                {
                                    mIsThirdStub = true;
                                }
                            }
                            else if (AveSPUtility.IsEbsArchivedData(DocFlag))
                            {
                                if (mQueryService.CheckContentIfAveStub(mBaseItemInfo.SiteId, mBaseItemInfo.GUID, InternalVersion))
                                {
                                    mStorageType = AveStorageType.EBS;
                                }
                                else
                                {
                                    mIsThirdStub = true;
                                }
                            }
                        }
                        else
                        {
                            if (AveSPUtility.IsRbsArchivedData(RbsId))
                            {
                                mIsThirdStub = true;
                                mStorageType = AveStorageType.RBS;
                            }
                            else if (AveSPUtility.IsEbsArchivedData(DocFlag))
                            {
                                if (mQueryService.CheckContentIfAveStub(mBaseItemInfo.SiteId, mBaseItemInfo.GUID, InternalVersion))
                                {
                                    mStorageType = AveStorageType.EBS;
                                }
                                else
                                {
                                    mIsThirdStub = true;
                                }
                            }
                        }
                    }
                }
                return mStorageType.Value;
            }
        }

        public AveStorageInfo StorageInfo
        {
            get
            {
                if (mStorageInfo == null)
                {
                    mStorageInfo = GetStorageInfo();
                }
                mStorageInfo.Size = mBaseItemInfo.DocumentSize;
                return mStorageInfo;
            }
        }

        //add for RevIM export
        public AveUserInfo Author
        {
            get
            {
                if (UserDataCache.ContainsKey("Author"))
                {
                    return ParentSite.GetUserInfo((int)UserDataCache["Author"]);
                }
                return null;
            }
        }

        public string Name
        {
            get { return mBaseItemInfo.Name; }
        }

        public string Title
        {
            get
            {
                if (mBaseItemInfo == null)
                {
                    return string.Empty;
                }
                return mBaseItemInfo.Name;
            }
        }

        public string ScopeUrl
        {
            get { return BaseItemInfo.ScopeUrl; }
            set { BaseItemInfo.ScopeUrl = value; }
        }

        public int RowId
        {
            get { return mBaseItemInfo.RowId; }
            set { mBaseItemInfo.RowId = value; }
        }

        public int Version
        {
            get { return mBaseItemInfo.Version; }
            set { mBaseItemInfo.Version = value; }
        }

        public int InternalVersion
        {
            get
            {
                if (!mBaseItemInfo.InternalVersion.HasValue)
                {
                    using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.InternalVersion"))
                    {
                        if (!mAveParentSite.ObjectModelFactory.IsSPInstalled)
                        {
                            mBaseItemInfo.InternalVersion = 0;
                        }
                        else
                        {
                            mBaseItemInfo.InternalVersion = Item.GetInternalVersion(mBaseItemInfo);
                        }
                    }
                }
                return mBaseItemInfo.InternalVersion.Value;
            }
            set { mBaseItemInfo.InternalVersion = value; }
        }

        public int DocFlag
        {
            get
            {
                if (mBaseItemInfo.DocFlag == 0)
                {
                    using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.DocFlag"))
                    {
                        if (!mAveParentSite.ObjectModelFactory.IsSPInstalled)
                        {
                            mBaseItemInfo.DocFlag = 0;
                        }
                        else
                        {
                            mBaseItemInfo.DocFlag = Item.GetDocFlag(BaseItemInfo);
                        }
                    }
                }
                return mBaseItemInfo.DocFlag;
            }
            set { mBaseItemInfo.DocFlag = value; }
        }

        public int Level
        {
            get { return mBaseItemInfo.Level; }
            set { mBaseItemInfo.Level = value; }
        }

        public bool HasStream
        {
            get { return mBaseItemInfo.HasStream; }
            set { mBaseItemInfo.HasStream = value; }
        }

        public bool IsVersion
        {
            get { return BaseItemInfo.IsVersion; }
            set { BaseItemInfo.IsVersion = value; }
        }

        public bool IsSystemFileOrFolder
        {
            get
            {
                return BaseItemInfo.RowId <= 0;
            }
        }

        public bool PageVersion
        {
            get { return mBaseItemInfo.PageVersion; }
            set { mBaseItemInfo.PageVersion = value; }
        }

        private bool mhasUniqueRoleAssignments;

        public bool HasUniqueRoleAssignments
        {
            get
            {
                return mhasUniqueRoleAssignments;
            }
        }

        private void QueryUniqueRoleAssignments()
        {
            if (restoringItemCurrentVersionDocData != null && restoringItemCurrentVersionDocData.ContainsKey("HasUniqueRoleAssignments"))
            {
                mhasUniqueRoleAssignments = Convert.ToBoolean(restoringItemCurrentVersionDocData["HasUniqueRoleAssignments"]);
            }
        }

        private bool mRbsIdInited;

        public byte[] RbsId
        {
            get
            {
                if (!mRbsIdInited)
                {
                    using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.RbsId"))
                    {
                        mRbsIdInited = true;
                        mRbsId = this.Item.GetRbsIdByNative(BaseItemInfo);
                    }
                }
                return mRbsId;
            }
        }

        public AveSPList AveSPList
        {
            get { return mAveSPList; }
        }

        public IAveListItem SPListItem
        {
            get
            {
                return mItem.ListItem;
            }
        }

        public Guid Id
        {
            get { return BaseItemInfo.GUID; }
            set { BaseItemInfo.GUID = value; }
        }

        public Guid SiteId
        {
            get { return BaseItemInfo.SiteId; }
            set { BaseItemInfo.SiteId = value; }
        }

        public Guid ListId
        {
            get { return mBaseItemInfo.ListId; }
            set { mBaseItemInfo.ListId = value; }
        }

        public Guid ScopeId
        {
            get { return mBaseItemInfo.ScopeId; }
            set { mBaseItemInfo.ScopeId = value; }
        }

        public AveSPListFieldCollection Fields
        {
            get { return this.mFields; }
            set { this.mFields = value; }
        }

        public IAveBackupRestoreQueryService QueryService
        {
            get { return this.mQueryService; }
            set { this.mQueryService = value; }
        }

        public long DocumentSize
        {
            get { return mBaseItemInfo.DocumentSize; }
        }

        public bool IsBackupLinkForArchivedData
        {
            get
            {
                if (IsPageFile())
                {
                    return false;
                }
                return mIsBackupLinkForArchivedData;
            }
            set { mIsBackupLinkForArchivedData = value; }
        }

        public Guid ParentId
        {
            set
            {
                mBaseItemInfo.ParentId = value;
            }
        }

        /// <summary>
        /// current version doc data of restoring item
        /// </summary>
        [ThreadStatic]
        internal static Dictionary<string, object> RestoringItemCurrentVersionDocData;

        private Dictionary<string, object> restoringItemCurrentVersionDocData;

        /// <summary>
        /// Item's all version numbers.
        /// </summary>
        [ThreadStatic]
        internal static Dictionary<Guid, object> ItemVersionNumbers;

        public bool BackupPagesFullContent
        {
            set { mBackupPagesFullContent = value; }
        }

        public List<AveWebPartBaseInfo> WebPartInfos
        {
            get { return mWebPartInfos; }
        }

        public AveSPItem()
        {
        }


        public AveSPItem(
            Guid id,
            int rowId,
            int version,
            AveItemType itemType,
            Guid parentId,
            Guid siteId,
            AveSPList aveList,
            IAveBackupStream stream,
            IAveBackupRestoreQueryService queryService,
            AveSPListFieldCollection fields)
            : this(id, rowId, version, null, itemType, parentId, siteId, aveList, stream, queryService, fields, aveList.SolutionStatus, null)
        {
        }

        public AveSPItem(
            Guid id,
            int rowId,
            int version,
            AveItemType itemType,
            Guid parentId,
            Guid siteId,
            AveSPList aveList,
            IAveBackupStream stream,
            IAveBackupRestoreQueryService queryService,
            AveSPListFieldCollection fields, IAveFolder parentFolder)
            : this(id, rowId, version, null, itemType, parentId, siteId, aveList, stream, queryService, fields, aveList.SolutionStatus, parentFolder)
        {
        }

        public AveSPItem(
            Guid id,
            int rowId,
            int version,
            string serverRelativeurl,
            AveItemType itemType,
            Guid parentId,
            Guid siteId,
            AveSPList aveList,
            IAveBackupStream stream,
            IAveBackupRestoreQueryService queryService,
            AveSPListFieldCollection fields,
            Dictionary<Guid, int> solutionStatus, IAveFolder parentFolder)
            : this(id, rowId, version, serverRelativeurl, itemType, parentId, siteId, aveList, stream, queryService, fields, aveList.SolutionStatus, null, parentFolder)
        {

        }

        public AveSPItem(
            Guid id,
            int rowId,
            int version,
            string serverRelativeurl,
            AveItemType itemType,
            Guid parentId,
            Guid siteId,
            AveSPList aveList,
            IAveBackupStream stream,
            IAveBackupRestoreQueryService queryService,
            AveSPListFieldCollection fields,
            Dictionary<Guid, int> solutionStatus,
            IAveListItem item,
            IAveFolder parentFolder)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.Constructor"))
            {
                if (aveList.ParentWeb.mReloadWebAndParentForSPRequestTimeout != null)
                {
                    aveList.ParentWeb.mReloadWebAndParentForSPRequestTimeout(false);
                }
                mBaseItemInfo = new AveBaseItemInfo();
                mItemType = itemType;
                mAveSPList = aveList;
                mQueryService = queryService;
                mFields = fields;

                //初始化base Info
                mBaseItemInfo.GUID = id;
                mBaseItemInfo.RowId = rowId;
                mBaseItemInfo.Version = version;
                mBaseItemInfo.ParentId = parentId;
                mBaseItemInfo.SiteId = siteId;
                mBaseItemInfo.ListId = aveList.Id;
                mBaseItemInfo.ItemType = mItemType;
                mBaseItemInfo.ServerRelativeUrl = serverRelativeurl;
                mAveParentSite = aveList.ParentWeb.ParentSite;
                mBaseItemInfo.MappingManager = mAveParentSite.MappingManager;
                mBaseItemInfo.MappingManager.SiteMappingManager.SolutionStatus = solutionStatus;
                using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPItem.Constructor.InitAveItem"))
                {
                    //初始化AveItem
                    if (parentFolder == null && itemType != AveItemType.Attachement)
                    {
                        parentFolder = aveList.ParentWeb.SPWeb.GetFolder(parentId, rowId, aveList.Title.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase) ? aveList.ParentWeb.SPWeb.ServerRelativeUrl : aveList.ServerRelativeUrl);
                    }
                    if (parentFolder != null && parentFolder.Exists)
                    {
                        mBaseItemInfo.ParentFolderRelativeUrl = parentFolder.ServerRelativeUrl;
                    }
                    else
                    {
                        mBaseItemInfo.ParentFolderRelativeUrl = aveList.Title.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase) ? aveList.ParentWeb.SPWeb.ServerRelativeUrl : aveList.ServerRelativeUrl;
                    }
                    mBaseItemInfo.Item = item;
                    mItem = mAveParentSite.ObjectModelFactory.CreateAveItem(mBaseItemInfo, parentFolder, mAveSPList.ParentWeb.SPWeb, mAveSPList.SPList);
                }
                restoringItemCurrentVersionDocData = mAveSPList.EnsureRestoringItemCurrentVersionDocData(mBaseItemInfo, mItem);
                QueryUniqueRoleAssignments();//初始化结束后判断对象是否有独立权限--for Replicator 365 multi-threads logic
            }
        }

        public Dictionary<string, object> GetAttachmentInfo()
        {
            Dictionary<string, object> dataCache = new Dictionary<string, object>();
            dataCache = mItem.GetAttachmentInfo(BaseItemInfo);
            return dataCache;
        }

        //we must GetDocInfo before we cache the UserData.
        public Dictionary<string, object> GetDocInfo(bool getAddtional_CommentsOnInfo = false)
        {
            if (this.DocDataCache == null)
            {
                this.DocDataCache = Item.GetDocInfo(BaseItemInfo, restoringItemCurrentVersionDocData);
            }
            AddAdditionalDocInfoToCache(getAddtional_CommentsOnInfo);
            return this.DocDataCache;
        }

        /// <summary>
        /// For SAAS-38248 
        /// </summary>
        /// <param name="getAddtional_CommentsOnInfo"></param>
        private void AddAdditionalDocInfoToCache(bool getAddtional_CommentsOnInfo)
        {
            StringBuilder logicFlowString = new StringBuilder();
            try
            {
                if (getAddtional_CommentsOnInfo && Item != null && Item.ListItem != null && mItemType == AveItemType.Document && Item.ListItem.CommentsDisabledScope == AveCommentsDisabledScope.Item)
                {
                    if (!this.DocDataCache.ContainsKey("CommentsDisabled"))
                    {
                        this.DocDataCache.Add("CommentsDisabled", Item.ListItem.CommentsDisabled);
                        logicFlowString.AppendLine($"[SAAS-38248]Set CommentsDisabled:{Item.ListItem.CommentsDisabled}");
                    }

                    if (!this.DocDataCache.ContainsKey("CommentsDisabledScope"))
                    {
                        this.DocDataCache.Add("CommentsDisabledScope", (int)Item.ListItem.CommentsDisabledScope);
                        logicFlowString.AppendLine($"[SAAS-38248]Set CommentsDisabledScope:{(int)Item.ListItem.CommentsDisabledScope}");
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("An error occured when add additional docInfo to cache, error: {0} ", e);
            }
            finally
            {
                if (logicFlowString.Length > 0)
                {
                    log.Info($"[SAAS-38248]Add Additional DocInfo To Cache, output logic flow:{logicFlowString.ToString()}");
                }
            }
        }
        //GetUserData() should be called after GetDocInfo() because the mSender.DataCache.

        public List<int> GetDocVersions()
        {
            if (AveSPItem.ItemVersionNumbers != null && !AveSPItem.ItemVersionNumbers.Keys.Contains(this.Id))
            {
                AveSPItem.ItemVersionNumbers.Clear();
                AveSPItem.ItemVersionNumbers[this.Id] = mItem.GetDocVersions(mBaseItemInfo);
            }
            else if (AveSPItem.ItemVersionNumbers == null)
            {
                AveSPItem.ItemVersionNumbers = new Dictionary<Guid, object>();
                AveSPItem.ItemVersionNumbers[this.Id] = mItem.GetDocVersions(mBaseItemInfo);
            }
            return AveSPItem.ItemVersionNumbers[this.Id] as List<int>;
        }

        public Dictionary<string, object> GetUserData()
        {
            if (mBaseItemInfo.RowId <= 0)
            {
                return null;
            }

            //Dictionary<string, object> dataCache = mSender.DataCache;
            Dictionary<string, object> userData = mItem.GetUserData(BaseItemInfo);
            if (userData != null && userData.Count > 0)
            {
                if (mFirstTime)
                {
                    AddToCache(userData);
                    mFirstTime = false;
                }
                return userData;
            }
            else
            {
                log.Warn("the item backed up failed since can not get the user data info.Id:{0}", mBaseItemInfo.RowId);
                if (mBaseItemInfo.ItemType != AveItemType.Folder)
                {
                    throw new Exception("the item backed up failed since can not get the user data info.");
                }
                return null;
            }
        }

        #region For Archiver
        internal AveTuple<Dictionary<string, object>, List<AveTermStoreInfo>> GetUserDataInfoWithDependence(
            SPItemMetadataBackupOption backupOption)
        {
            AveBackupOption aveBackupOption = null;
            if (backupOption != null)
            {
                aveBackupOption = new AveBackupOption()
                {
                    BackupRelatedTermSets = backupOption.BackupRelatedTermSets,
                    BackupRelatedTermsOnly = backupOption.BackupRelatedTermsOnly,
                    BackupItemTPGUIDofLookupValue = backupOption.BackupItemTPGUIDofLookupValue
                };
            }
            return GetUserDataInfoWithDependence(aveBackupOption);
        }

        /// <summary>
        /// Return user data info
        /// </summary>
        /// <param name="backupColumnOption"></param>
        /// <returns></returns>
        internal AvePoint.GCommon.Utility.AveTuple<Dictionary<string, object>, List<AveTermStoreInfo>> GetUserDataInfoWithDependence(AveBackupOption backupColumnOption)
        {
            if (UserDataCache == null)
            {
                UserDataCache = this.GetUserData();
            }
            List<AveTermStoreInfo> relatedInfos = null;
            if (UserDataCache != null && UserDataCache.Count > 0)
            {
                if (backupColumnOption != null && (backupColumnOption.BackupRelatedTermSets || backupColumnOption.BackupRelatedTermsOnly))
                {
                    Dictionary<string, AveTaxFieldInfo> taxonomyFields = this.AveSPList.TaxonomyFields;
                    if (taxonomyFields != null && taxonomyFields.Count > 0)
                    {
                        relatedInfos = this.GetMetadataInfo(backupColumnOption);
                        //output.WriteMetadata(AveMetadataType.MetadataService, relatedInfos);
                    }
                }
                //if (mAveSPList.BackupLookUpDisplayValue)
                //{
                //    foreach (KeyValuePair<string, AveLookupFieldInfo> kv in mAveSPList.LookupFields)
                //    {
                //        try
                //        {
                //            if (userData.ContainsKey(kv.Key))
                //            {
                //                string itemId = userData[kv.Key].ToString();
                //                string displayValue = mAveSPList.GetLookupDisplayValuebyItemId(itemId, kv.Value);
                //                if (!string.IsNullOrEmpty(displayValue))
                //                {
                //                    userData[kv.Key] = itemId + ";" + displayValue;
                //                }
                //            }
                //        }
                //        catch (Exception e)
                //        {
                //            log.Warn("Get single lookup display value exception.Error:{0}", e.ToString());
                //        }
                //    }
                //}
                //output.WriteMetadata(AveMetadataType.DocData, userData);
            }

            return new AveTuple<Dictionary<string, object>, List<AveTermStoreInfo>>(UserDataCache, relatedInfos);
        }

        internal AveUserList GetUserCache(bool removeAvailableUser)
        {
            this.CacheUserForUserInfomationList();

            if (removeAvailableUser)
            {
                return GetUnavailableUserInCache();
            }
            else
            {
                this.CachePrincipalOfTargetAudience();
            }

            return GetUserCache();
        }

        /// <summary>
        /// Get unavailable user
        /// </summary>
        /// <returns></returns>
        private AveUserList GetUnavailableUserInCache()
        {
            var list = new AveUserList();
            foreach (AveUserInfo userInfo in DataCache.GetUsersForExport().Users)
            {
                if (!mAveParentSite.SPSite.CheckUserIfAvailable(userInfo.ID))
                {
                    list.Users.Add(userInfo);
                }
            }
            return list;
        }

        private AveUserList GetUserCache()
        {
            return DataCache.GetUsersForExport();
        }

        public AveGroupList GetGroupCache()
        {
            var groups = DataCache.GetGroupsForExport();
            return groups;
        }
        #endregion

        public List<Dictionary<string, object>> GetUserDataJunction()
        {
            return mItem.GetUserDataJunction(BaseItemInfo);
        }

        internal List<Dictionary<string, object>> GetUserDataJunctionCache(bool includeUsesAndGroup)
        {
            if (includeUsesAndGroup)
            {
                this.CachePrincipalFromDatajunction();
            }

            List<Dictionary<string, object>> dataCache = this.UserDatajunctionCache;
            if (dataCache != null)
            {
                //if (mAveSPList.BackupLookUpDisplayValue)
                //{
                //    try
                //    {
                //        foreach (Dictionary<string, object> data in dataCache)
                //        {
                //            if (!data.ContainsKey("DisplayValue"))
                //            {
                //                Guid fieldId = new Guid(data["tp_FieldId"].ToString());
                //                string itemId = data["tp_Id"].ToString();
                //                AveLookupFieldInfo fieldInfo = null;
                //                foreach (AveLookupFieldInfo info in mAveSPList.LookupFields.Values)
                //                {
                //                    if (info.Id == fieldId)
                //                    {
                //                        fieldInfo = info;
                //                        break;
                //                    }
                //                }
                //                if (fieldInfo != null)
                //                {
                //                    string displayValue = mAveSPList.GetLookupDisplayValuebyItemId(itemId, fieldInfo);
                //                    if (!string.IsNullOrEmpty(displayValue))
                //                    {
                //                        data["DisplayValue"] = displayValue;
                //                    }
                //                }
                //            }
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        log.Warn("Get multi lookup display value exception.Error:{0}", ex.ToString());
                //    }
                //}
                //output.WriteMetadata(AveMetadataType.DocDataJunction, dataCache);
            }

            return dataCache;
        }

        /// <summary>
        /// 判断这个文件是否是Pages下面的文件
        /// </summary>
        /// <returns></returns>
        public bool IsPageFile()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.IsPageFile"))
            {
                if (mBackupPagesFullContent && mAveSPList != null && mAveSPList.SPList != null
                    && (int)mAveSPList.SPList.BaseTemplate == 850)// && !String.IsNullOrEmpty(mName) && mName.EndsWith("aspx", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return false;
            }
        }

        //modify for RevIM export
        public Stream GetContent()
        {
            return GetContent(Version);
        }

        public void ExportContent(IAveBackupStream output)
        {
            ExportContentByAPI(output, Version);
        }

        //public void ExportContent(IAveBackupStream output, IStreamConvertor streamConvertor)
        //{
        //    ExportContentByAPI(output, Version, streamConvertor);
        //}

        public string ExportContentByAPIAndCalculateCRC(IAveBackupStream output)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPItem.ExportContentByAPIAndCalculateCRC"))
            {
                Stream stream = GetContent(Version);
                if (stream != null)
                {
                    try
                    {
                        //if (streamConvertor != null)
                        //{
                        //    stream = streamConvertor.Process(mAveSPList.SPList, stream, Path.GetFileName(mBaseItemInfo.ServerRelativeUrl));
                        //}
                        log.Info($"Begin WriteContent when ExportContentByAPIAndCalculateCRC.");
                        mBaseItemInfo.DocumentSize = stream.Length;
                        byte[] buffer = output.DataBuffer;
                        int length;
                        output.FlushMetadata(stream.Length);

                        Stopwatch stopwatchV3 = Stopwatch.StartNew();
                        Crc64 hashAlgorithm = new Crc64();

                        long readSize = 0;
                        while (readSize < stream.Length)//(length = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            length = stream.Read(buffer, 0, buffer.Length);
                            if (length == 0)
                            {
                                break;
                            }
                            readSize += length;
                            hashAlgorithm.Append(new ReadOnlySpan<byte>(buffer, 0, length));
                            output.WriteContent(buffer, 0, length);
                        }

                        var resultV3 = Convert.ToBase64String(hashAlgorithm.GetCurrentHash());
                        stopwatchV3.Stop();
                        log.Info($"End Calculate CRC 64.V3Time:{stopwatchV3.Elapsed}.V3Result:{resultV3}.");
                        return resultV3;
                        ////Calculate CRC 64 V1
                        //Stopwatch stopwatchV1 = Stopwatch.StartNew();
                        ////log.Info($"Begin Calculate CRC 64 V1.");
                        //byte[] bufferCrcV1 = output.DataBuffer;
                        //stream.Position = 0;
                        //var hashAlgorithmV1 = new Media.Storage.Security.AveCrc64();
                        //while (true)
                        //{
                        //    int readLen = stream.Read(bufferCrcV1, 0, bufferCrcV1.Length);
                        //    if (readLen <= 0)
                        //        break;
                        //    hashAlgorithmV1.TransformBlock(bufferCrcV1, 0, readLen, null, 0);
                        //}
                        //hashAlgorithmV1.TransformFinalBlock(new byte[0], 0, 0);
                        //stopwatchV1.Stop();
                        //var resultV1 = Convert.ToBase64String(hashAlgorithmV1.Hash);
                        ////log.Info($"End Calculate CRC 64 V1.Time:{stopwatchV1.Elapsed}.result:{resultV1}.");

                        ////Calculate CRC 64 V2
                        //stream.Position = 0;
                        //Stopwatch stopwatchV2 = Stopwatch.StartNew();
                        //string resultV2 = string.Empty;
                        ////log.Info($"Begin Calculate CRC 64 V2.");
                        //using (var hashAlgorithm = new AveCrc64())
                        //{
                        //    var position = stream.Position;
                        //    hashAlgorithm.ComputeHash(stream);
                        //    resultV2 = Convert.ToBase64String(hashAlgorithm.Hash);
                        //    stream.Seek(position, SeekOrigin.Begin);
                        //}
                        //stopwatchV2.Stop();
                    }
                    finally
                    {
                        stream.Dispose();
                    }
                }
                else
                {
                    output.FlushMetadata(0);
                    return string.Empty;
                }
            }
        }

        private void AddPricipleToDataCache(int principleId)
        {
            try
            {
                if (!DataCache.principalIdAlreadyExists(principleId))
                {
                    object obj = mAveSPList.ParentWeb.ParentSite.DataCache.GetPrincipalInfo(principleId);
                    if (obj is AveUserInfo)
                    {
                        DataCache.AddToCache(principleId, (AveUserInfo)obj);
                    }
                    else if (obj is AveGroupInfo)
                    {
                        DataCache.AddToCache(principleId, (AveGroupInfo)obj);
                    }
                    else
                    {
                        DataCache.AddToCache(principleId);
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while add principal to cache. \n error message:{0}", e));
            }
        }

        public void CheckPageView(Dictionary<string, object> dataCache, IList<AveViewInfo> viewList)
        {
            if (viewList != null)
            {
                int i = 0;
                dataCache.Add("IsViewPage", true);
                dataCache["HasStream"] = Convert.ToInt32(dataCache["CustomizedPageStatus"]) == (int)AveCustomizedPageStatus.Uncustomized ? 0 : 1;
                BaseItemInfo.HasStream = Convert.ToInt32(dataCache["HasStream"]) == 1;
                foreach (AveViewInfo aveViewInfo in viewList)
                {
                    dataCache.Add("ViewID" + i, aveViewInfo.Id);
                    dataCache.Add("ViewType" + i, aveViewInfo.ViewType);
                    dataCache.Add("IsPersonal" + i, aveViewInfo.IsPersonal);
                    dataCache.Add("ViewTitle" + i, aveViewInfo.Title);
                    dataCache.Add("IsMobileView" + i, aveViewInfo.IsMobileView);
                    dataCache.Add("IsDefaultMobileView" + i, aveViewInfo.IsDefaultMobileView);
                    dataCache.Add("Hidden" + i, aveViewInfo.Hidden);
                    if (aveViewInfo.IsDefaultView.HasValue)
                    {
                        dataCache.Add("IsDefaultView" + i, aveViewInfo.IsDefaultView);
                    }
                    dataCache.Add("BaseViewId" + i, aveViewInfo.BaseViewId);
                    dataCache.Add("Scope" + i, aveViewInfo.Scope);
                    dataCache.Add("RowLimit" + i, aveViewInfo.RowLimit);
                    dataCache.Add("ViewData" + i, aveViewInfo.ViewData);
                    dataCache.Add("ContentTypeId" + i, aveViewInfo.ContentTypeId);
                    dataCache.Add("ListViewXml" + i, aveViewInfo.ListViewXml);
                    if (aveViewInfo.UserID.HasValue)
                    {
                        dataCache.Add("UserID" + i, aveViewInfo.UserID);
                        AddPricipleToDataCache(aveViewInfo.UserID.Value);
                    }
                    ++i;
                }
                //SAAS-29933 Keep the spotlightinfo items id to url mapping
                if (IsViewIncludingSpotlightInfo(viewList))
                {
                    dataCache.Add("ViewSpotlightInfoMapping", mAveSPList.SPList.GetViewSpotlightItemsMapping());
                }
            }
        }
        private bool IsViewIncludingSpotlightInfo(IList<AveViewInfo> viewList)
        {
            bool includingSpotlightInfo = false;
            foreach (AveViewInfo aveViewInfo in viewList)
            {
                string listViewXml = aveViewInfo.ListViewXml;
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.LoadXml(listViewXml);
                System.Xml.XmlNode spotlightInfoNode = doc.SelectSingleNode("View/SpotlightInfo");
                if (spotlightInfoNode != null)
                {
                    includingSpotlightInfo = true;
                    break;
                }
            }
            return includingSpotlightInfo;
        }

        private static string GetScopeUrl(AveBaseItemInfo info)
        {
            if (info == null)
            {
                return "";
            }
            if (!string.IsNullOrEmpty(info.ScopeUrl))
            {
                return info.ScopeUrl;
            }
            if (!string.IsNullOrEmpty(info.ServerRelativeUrl))
            {
                return info.ServerRelativeUrl;
            }
            return "";
        }

        internal void CheckFormView(Dictionary<string, object> dataCache)
        {
            lock (this.mAveSPList.FormCache)
            {
                var scopeUrl = GetScopeUrl(BaseItemInfo);
                if (!string.IsNullOrEmpty(scopeUrl))
                {
                    string formUrl = '/' + scopeUrl.TrimStart('/');
                    if (this.mAveSPList.FormCache.ContainsKey(formUrl))
                    {
                        dataCache["IsFormPage"] = true;
                        dataCache["HasStream"] = Convert.ToInt32(dataCache["CustomizedPageStatus"]) == (int)AveCustomizedPageStatus.Uncustomized ? 0 : 1;
                        BaseItemInfo.HasStream = Convert.ToInt32(dataCache["HasStream"]) == 1;
                        this.AveSPList.FormCache.Remove(formUrl);
                    }
                }
            }
        }

        public void CheckPageView(Dictionary<string, object> dataCache, Dictionary<string, List<AveViewInfo>> listViewCache)
        {
            if (ScopeUrl != null && listViewCache.ContainsKey(ScopeUrl))
            {
                int i = 0;
                dataCache.Add("IsViewPage", true);
                List<AveViewInfo> viewList = listViewCache[ScopeUrl];
                foreach (AveViewInfo aveViewInfo in viewList)
                {
                    dataCache.Add("ViewID" + i, aveViewInfo.Id);
                    dataCache.Add("ViewType" + i, aveViewInfo.ViewType);
                    dataCache.Add("IsPersonal" + i, aveViewInfo.IsPersonal);
                    dataCache.Add("ViewTitle" + i, aveViewInfo.Title);
                    if (aveViewInfo.IsDefaultView.HasValue)
                    {
                        dataCache.Add("IsDefaultView" + i, aveViewInfo.IsDefaultView);
                    }
                    dataCache.Add("BaseViewId" + i, aveViewInfo.BaseViewId);
                    if (aveViewInfo.UserID != null)
                    {
                        dataCache.Add("UserID" + i, aveViewInfo.UserID);
                    }
                    ++i;
                }
                listViewCache.Remove(ScopeUrl);
            }
        }

        public void CheckPageView(Dictionary<string, object> dataCache, Dictionary<Guid, List<AveViewInfo>> listViewCache)
        {
            if (listViewCache.ContainsKey(this.Id))
            {
                int i = 0;
                dataCache.Add("IsViewPage", true);
                List<AveViewInfo> viewList = listViewCache[this.Id];
                foreach (AveViewInfo aveViewInfo in viewList)
                {
                    dataCache.Add("ViewID" + i, aveViewInfo.Id);
                    dataCache.Add("ViewType" + i, aveViewInfo.ViewType);
                    dataCache.Add("IsPersonal" + i, aveViewInfo.IsPersonal);
                    dataCache.Add("ViewTitle" + i, aveViewInfo.Title);
                    dataCache.Add("IsMobileView" + i, aveViewInfo.IsMobileView);
                    dataCache.Add("IsDefaultMobileView" + i, aveViewInfo.IsDefaultMobileView);
                    dataCache.Add("Hidden" + i, aveViewInfo.Hidden);
                    if (aveViewInfo.IsDefaultView.HasValue)
                    {
                        dataCache.Add("IsDefaultView" + i, aveViewInfo.IsDefaultView);
                    }
                    dataCache.Add("BaseViewId" + i, aveViewInfo.BaseViewId);
                    dataCache.Add("ContentTypeId" + i, aveViewInfo.ContentTypeId);
                    if (aveViewInfo.UserID.HasValue)
                    {
                        dataCache.Add("UserID" + i, aveViewInfo.UserID);
                        AddPricipleToDataCache(aveViewInfo.UserID.Value);
                    }
                    ++i;
                }
                //SAAS-29933 Keep the spotlightinfo items id to url mapping 
                if (IsViewIncludingSpotlightInfo(viewList))
                {
                    dataCache.Add("ViewSpotlightInfoMapping", mAveSPList.SPList.GetViewSpotlightItemsMapping());
                }
                listViewCache.Remove(this.Id);
            }
        }

        public AveStubDataType GetEBSDataType()
        {
            AveStubDataType DataType = AveStubDataType.UnKnown;
            string stubContent = Item.GetStubInfoByNative(BaseItemInfo);
            if (stubContent.StartsWith("AVE", StringComparison.OrdinalIgnoreCase))
            {
                DataType = AveStubDataType.Extender;
            }
            return DataType;
        }

        public AveStubDataType GetRBSDataType()
        {
            AveStubDataType DataType = AveStubDataType.UnKnown;
            if (RbsId != null)
            {
                //    AveRBSStubInfo RBSInfo = mAveParentSite.RBSBackup.BackupRBSStub(RbsId);
                //    byte blobType = RBSInfo.StoreBlobId[3];
                //    if ((blobType & 1) == 1)
                //        DataType = AveStubDataType.Extender;
                //    else if ((blobType & 2) == 2)
                //        DataType = AveStubDataType.Extender;
                //    else if ((blobType & 4) == 4)
                //        DataType = AveStubDataType.Connector;
                //    else
                //        DataType = AveStubDataType.UnKnown;
            }
            return DataType;
        }

        /*private AveStubDataType GetRBSDataType(byte[] RBSBlobId)
        {
            AveStubDataType DataType = AveStubDataType.UnKnown;
            if (RbsId != null)
            {
                byte blobType = RBSBlobId[3];
                if ((blobType & 1) == 1)
                    DataType = AveStubDataType.Extender;
                else if ((blobType & 2) == 2)
                    DataType = AveStubDataType.Extender;
                else if ((blobType & 4) == 4)
                    DataType = AveStubDataType.Connector;
                else
                    DataType = AveStubDataType.UnKnown;
            }
            return DataType;
        }

        public AveStubDataType GetStubDataType()
        {
            //if (IsEbsArchivedData)
            //{
            //    return GetEBSDataType();
            //}
            //else if (IsRbsArchivedData)
            //{
            //    return GetRBSDataType();
            //}
            return AveStubDataType.UnKnown;
        }*/

        //add for RevIM export
        public Dictionary<string, object> GetColumnValues(ColumnsLevel level = ColumnsLevel.AllColumns)
        {
            if (RowId == 0 || mAveSPList == null || mAveSPList.SPList == null)
            {
                return null;
            }
            //return mAveSPList.AveIndexCache.GetColumnValues(this, level, forceGetByAPI);
            return mAveSPList.AveIndexCache.GetAllColumnValues(this, level);
        }

        public FullTextIndex GetFullTextIndex(FullTextIndexLevel level, Dictionary<string, object> customFieldValues = null)
        {
            if (RowId == 0 || mAveSPList == null || mAveSPList.SPList == null)
            {
                return null;
            }
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPItem.ExportFullTextIndex"))
            {
                var index = mAveSPList.AveIndexCache.GetIndex(this, level);
                if (customFieldValues != null)
                {
                    index.SetCustomColumnValues(customFieldValues);
                }
                return index;
            }
        }

        public void CachePrincipalFromMetadata()
        {
            if (UserDataCache == null)
            {
                UserDataCache = GetUserData();
            }
        }

        private AveStorageInfo GetStorageInfo()
        {
            AveStorageInfo info = new AveStorageInfo();
            try
            {
                if (IsEbsArchivedData)
                {
                    info = mAveSPList.SOIntegrationUtil.BackupEBSStorageInfo(mAveParentSite.SPSite, Id, Version, Level, Item, BaseItemInfo);
                }
                else if (IsRbsArchivedData)
                {
                    info = mAveSPList.SOIntegrationUtil.BackupRBSStorageInfo(mAveParentSite.SPSite, Id, Version, Level, mQueryService, this.RBSStubInfo, RbsId);
                }
                info.IsBackupLinkForArchivedData = IsBackupLinkForArchivedData;
                if (IsThirdStub || IsSpecialData)
                {
                    info.IsBackupLinkForArchivedData = false;
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBGetStorageInfoError, Id, ex.ToString());
            }
            return info;
        }

        private AveRBSStubInfo mRBSStubInfo;

        public AveRBSStubInfo RBSStubInfo
        {
            get
            {
                if (mRBSStubInfo == null)
                {
                    mRBSStubInfo = mAveParentSite.RBSBackup.BackupRBSStub(RbsId);
                }
                return mRBSStubInfo;
            }
        }

        public bool IsEbsArchivedData
        {
            get
            {
                return StorageType == AveStorageType.EBS;
            }
        }

        public bool IsRbsArchivedData
        {
            get
            {
                return StorageType == AveStorageType.RBS;
            }
        }

        public void ExportRbsId(IAveBackupStream output)
        {
            if (IsRbsArchivedData)
            {
                output.WriteMetadata(AveMetadataType.DocRbsId, RbsId);
            }
        }

        public void CacheUserForUserInfomationList()
        {
            if (mAveSPList.SPList.BaseTemplate == AveListTemplateType.UserInformation)
            {
                AddPricipleToDataCache(RowId);
            }
        }

        public void CachePrincipalFromDatajunction()
        {
            if (UserDatajunctionCache != null)
            {
                IAveField spField = null;
                foreach (Dictionary<string, object> value in UserDatajunctionCache)
                {
                    Guid fieldId = (Guid)value["tp_FieldId"];
                    if (spField == null || spField.ID != fieldId)
                    {
                        if (mAveSPList.SPList.Fields.Contains(fieldId))
                        {
                            spField = mAveSPList.SPList.Fields[fieldId];
                        }
                        else
                        {
                            log.Warn(string.Format("There is an error when get item's field.\nitem:{0}\nfield:{1}\nlist:{2}.", mBaseItemInfo.Name, fieldId.ToString(), mAveSPList.SPList.Title));
                            continue;
                        }
                    }
                    //if (spField.TypeAsString == AveFieldType.User.ToString())//(spField is IAveFieldUser)
                    if (spField is IAveFieldUser && value.ContainsKey("tp_Id") && value["tp_Id"] != null)
                    {
                        AddPricipleToDataCache((int)value["tp_Id"]);
                    }
                }
            }
        }

        public void CachePrincipalOfTargetAudience()
        {
            if (UserDataCache != null && UserDataCache.ContainsKey("Target_x0020_Audiences") && UserDataCache["Target_x0020_Audiences"] != null)
            {
                //TargetAudience字段里储存的GroupName格式为“;;;;Group1,Group2”
                string[] names = UserDataCache["Target_x0020_Audiences"].ToString().TrimStart(';').Split(',');

                List<AveGroupInfo> list = mAveSPList.ParentSite.AllGroups;
                if (list != null)
                {
                    foreach (AveGroupInfo groupInfo in list)
                    {
                        if (names.Contains(groupInfo.Title, StringComparer.OrdinalIgnoreCase))
                        {
                            DataCache.AddToCache(groupInfo.ID, groupInfo);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add user/group to datacache
        /// </summary>
        /// <param name="value">1:Users, 2:All Groups, 4:Sharing links groups</param>
        public void CachePrincipalFromPermission(int value)
        {
            AveRoleAssignments roleAssignments = AveRoleAssignments.CreateInstance(this);
            RoleAssignmentCache = roleAssignments.GetRoleAssignments();
            if (RoleAssignmentCache == null)
            {
                return;
            }
            for (int i = 0; i < RoleAssignmentCache.Count; ++i)
            {
                try
                {
                    int principalId = RoleAssignmentCache[i].PrincipalId;
                    if (!DataCache.principalIdAlreadyExists(principalId))
                    {
                        object obj = mAveSPList.ParentWeb.ParentSite.DataCache.GetPrincipalInfo(principalId);

                        if (obj is AveUserInfo && (value & 1) != 0)
                        {
                            DataCache.AddToCache(principalId, (AveUserInfo)obj);
                        }
                        else if (obj is AveGroupInfo && (value & 2) != 0)
                        {
                            var group = (AveGroupInfo)obj;
                            if (AveSPUtility.MatchShareLink.IsMatch(group.Title))
                            {
                                AddToDataCacheForSharingLinkGroup(principalId, group);
                            }
                            else
                            {
                                DataCache.AddToCache(principalId, group);
                            }
                        }
                        else if (obj is AveGroupInfo && (value & 4) != 0) // Cache the sharing links groups only
                        {
                            AddToDataCacheForSharingLinkGroup(principalId, (AveGroupInfo)obj);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while add principal to cache. \n error message:{0}", e));
                }
            }
        }

        private void AddToDataCacheForSharingLinkGroup(int principalId, AveGroupInfo group)
        {
            try
            {
                log.Info($"Check if need to add sharing link group to cache：{group?.Title}-{principalId}.");
                ArgumentNullException.ThrowIfNull(group);
                if (AveSPUtility.MatchShareLink.IsMatch(group?.Title))
                {
                    string idstr = group.Title[(group.Title.LastIndexOf('.') + 1)..];
                    group.ShareLinkId = new Guid(idstr);
                    if (mItem.ListItem.SharingLinks.ContainsKey(group.ShareLinkId))
                    {
                        group.IsVerifiedSharelinkGroup = true;
                        group.ShareLink = mItem.ListItem.SharingLinks[group.ShareLinkId];
                        DataCache.AddToCache(principalId, group);
                        log.Info($"Add sharing link group success. Details:{group.Title}-{principalId}, linkKind:{group.ShareLink.LinkKind}");
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occured while getting sharing link.linkTitle:{0}, ex:{1}.", group?.Title, e.ToString());
            }
        }

        public void CacheUserFromAlert(object obj)
        {
            AveSPAlert aveAlert = AveSPAlert.CreateInstance(obj);
            ImmedSubscriptionsCache = aveAlert.GetImmedSubscriptions();
            SchedSubscriptionsCache = aveAlert.GetSchedSubscriptions();

            for (int i = 0; i < ImmedSubscriptionsCache.Count; i++)
            {
                try
                {
                    int userId = int.Parse(ImmedSubscriptionsCache[i]["UserId"].ToString());
                    if (!DataCache.principalIdAlreadyExists(userId))
                    {
                        AveUserInfo userInfo = (AveUserInfo)mAveSPList.ParentWeb.ParentSite.DataCache.GetPrincipalInfo(userId);
                        DataCache.AddToCache(userId, userInfo);
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while cache user from alert. \n error message:{0}", e));
                }
            }
            for (int i = 0; i < SchedSubscriptionsCache.Count; i++)
            {
                try
                {
                    int userId = int.Parse(SchedSubscriptionsCache[i]["UserId"].ToString());
                    if (!DataCache.principalIdAlreadyExists(userId))
                    {
                        AveUserInfo userInfo = (AveUserInfo)mAveSPList.ParentWeb.ParentSite.DataCache.GetPrincipalInfo(userId);
                        DataCache.AddToCache(userId, userInfo);
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while cache user from alert. \n error message:{0}", e));
                }
            }
        }

        public void CacheUserFromWebParts()
        {
            try
            {
                //if (!(obj is AveSPDoc))
                //{
                //    return;
                //}
                IAveLimitedWebPartManager webpartManager = this.ParentSite.ObjectModelFactory.CreateLimitedWebPartManager(this.ParentSite.SPSite, this.AveSPList.ParentWeb.SPWeb, this.BaseItemInfo.ServerRelativeUrl);
                try
                {
                    mWebPartInfos = webpartManager.GetWebParts(this.BaseItemInfo);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An error occurred when backup web parts, Page:{0}, Version:{1}. Reason:{2}.", this.BaseItemInfo.ServerRelativeUrl, this.BaseItemInfo.Version, ex);
                }
                if (mWebPartInfos != null)
                {
                    for (int i = 0; i < mWebPartInfos.Count; i++)
                    {
                        try
                        {
                            int userId = mWebPartInfos[i].UserID;
                            if (userId != 0)
                            {
                                if (!DataCache.principalIdAlreadyExists(userId))
                                {
                                    AveUserInfo userInfo = mAveSPList.ParentWeb.ParentSite.DataCache.GetPrincipalInfo(userId) as AveUserInfo;
                                    if (userInfo != null)
                                    {
                                        DataCache.AddToCache(userId, userInfo);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.WARN, "Can't get User:{0} for webpart :{1},Exception :{2}", mWebPartInfos[i].UserID, mWebPartInfos[i].ID, e.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, ex.ToString());
            }
        }

        //DOC-70322 for replicator,用于replicator的increment job能够正确还原lookup field的value.
        public Dictionary<string, string> GetLookupFieldGuidValue()
        {
            if (mFields == null)
            {
                return null;
            }
            if (UserDataCache == null)
            {
                UserDataCache = GetUserData();
            }
            Dictionary<string, StringBuilder> lookupFieldGuidValue = new Dictionary<string, StringBuilder>();
            string name = string.Empty;
            if (UserDataCache != null)
            {
                foreach (KeyValuePair<string, object> pair in UserDataCache)
                {
                    if (pair.Key.StartsWith("#", StringComparison.OrdinalIgnoreCase) || pair.Key.EndsWith("#2", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    name = pair.Key;
                    var schemaXml = string.Empty;
                    try
                    {
                        IAveField spField = mAveSPList.SPList.Fields.GetFieldByInternalName(name);
                        if (spField.Type == AveFieldType.Lookup)
                        {
                            IAveFieldLookup lookupField = spField as IAveFieldLookup;
                            schemaXml = lookupField.SchemaXml;
                            Guid lookupListId = new Guid(lookupField.LookupList);
                            int rowId = pair.Value.ToString().Contains(";") ? Convert.ToInt32(pair.Value.ToString().Split(';')[0]) : Convert.ToInt32(pair.Value);
                            Guid tp_GUID = GetLookupGUIDById(lookupListId, rowId);
                            if (tp_GUID != Guid.Empty)
                            {
                                StringBuilder builder = new StringBuilder();
                                builder.Append(rowId);
                                builder.Append('#');
                                builder.Append(tp_GUID);
                                lookupFieldGuidValue[name] = builder;
                                //lookupFieldGuidValue[name] = rowId.ToString() + "#" + tp_GUID.ToString();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, "An error occurred while GetLookupFieldGuidValue in UserDataCache.field name:{0}, schema xml:{1}, error:{2}.", name, schemaXml, ex.ToString());
                    }
                }
            }
            if (UserDatajunctionCache != null)
            {
                foreach (Dictionary<string, object> dic in UserDatajunctionCache)
                {
                    try
                    {
                        Guid fieldId = (Guid)dic["tp_FieldId"];
                        int rowId = (int)dic["tp_Id"];
                        IAveField spField = mAveSPList.SPList.Fields[fieldId];
                        name = spField.InternalName;
                        if (spField.Type == AveFieldType.Lookup)
                        {
                            IAveFieldLookup lookupField = spField as IAveFieldLookup;
                            Guid lookupListId = new Guid(lookupField.LookupList);
                            Guid tp_GUID = GetLookupGUIDById(lookupListId, rowId);
                            if (tp_GUID != Guid.Empty)
                            {
                                StringBuilder builder = null;
                                if (!lookupFieldGuidValue.ContainsKey(name))
                                {
                                    builder = new StringBuilder();
                                    lookupFieldGuidValue[name] = builder;
                                }
                                else
                                {
                                    builder = lookupFieldGuidValue[name];
                                }
                                //lookupFieldGuidValue[name] += rowId.ToString() + "#" + tp_GUID.ToString() + ";";
                                builder.Append(rowId);
                                builder.Append('#');
                                builder.Append(tp_GUID);
                                builder.Append(';');
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN, "An error occurred while GetLookupFieldGuidValue in UserDataJunctionCache.field name:{0},error:{1}.", name, ex.ToString());
                    }
                }
            }

            Dictionary<string, string> allValues = new Dictionary<string, string>();
            if (lookupFieldGuidValue.Count > 0)
            {
                foreach (var keyValue in lookupFieldGuidValue)
                {
                    allValues[keyValue.Key] = keyValue.Value.ToString();
                }
            }

            return allValues;
        }

        public Guid GetLookupGUIDById(Guid lookupListId, int rowId)
        {
            IAveList list = this.mAveSPList.ParentWeb.SPWeb.Lists.GetById(lookupListId);
            if (list != null)
            {
                IAveListItem item = list.GetItemById(rowId);
                if (item != null)
                {
                    return (Guid)item["GUID"];
                }
            }
            return Guid.Empty;
        }

        private void AddToCache(Dictionary<string, object> tempData)
        {
            string name;
            foreach (KeyValuePair<string, object> pair in tempData)
            {
                if (!pair.Key.Equals("#tp_CheckoutUserId") && (pair.Key.StartsWith("#tp_", StringComparison.OrdinalIgnoreCase) || pair.Key.StartsWith("#", StringComparison.OrdinalIgnoreCase) || pair.Key.EndsWith("#2", StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                name = pair.Key.Equals("#tp_CheckoutUserId") ? "CheckoutUser" : pair.Key;
                IAveField spField = null;
                try
                {
                    spField = mAveSPList.SPList.Fields.GetField(name);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBCannotFindField, name, ex.ToString());
                }

                if (spField == null)
                {
                    continue;
                }
                if (spField.TypeAsString == AveFieldType.User.ToString() && pair.Value != null)
                {
                    AddPricipleToDataCache((int)pair.Value);
                }
            }
        }

        public void SetAttachmentInfo()
        {
            HasStream = true;
            IsVersion = false;
            Level = 1;
            mBaseItemInfo.DocumentSize = SetAttachmentSize();
        }

        private long SetAttachmentSize()
        {
            return mItem.SetAttachmentSize(mBaseItemInfo);
        }
        public void ExportUserDataInfo(IAveBackupStream output, AveBackupOption backupColumnOption = null, bool includeUserAndGroup = true, bool onlyUnAvaiableUser = false)
        {
            if (backupColumnOption == null)
            {
                backupColumnOption = new AveBackupOption();
            }
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.ExportDocInfo"))
            {
                var userData = GetUserDataInfoWithDependence(backupColumnOption);

                //CI-31912 不备份AllUserData表中没有数据的document以及document version
                if (mItemType == AveItemType.Document && !this.IsSystemFileOrFolder && (userData.ItemA == null || userData.ItemA.Count == 0))
                {
                    log.Info("Skip backing up current document as it's user data is invalid.");
                    //throw new AveWrapperSkipException(AveInternalResourceKey.Wrapper_Exception_Backup_SkipBackupDocumentWithInvalidUserData);
                }
                if (userData.ItemB != null)
                {
                    output.WriteMetadata(AveMetadataType.MetadataService, userData.ItemB);
                }

                if (userData.ItemA != null)
                {
                    output.WriteMetadata(AveMetadataType.DocData, userData.ItemA);
                }
            }
        }

        #region add for RevIM
        private bool isRoleAssignmentCached;
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "DataJunctionInfo is a part of common method name")]
        public void ExportDataJunctionInfo(IAveBackupStream output, bool includeUserAndGroup = true)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.ExportDataJunctionInfo"))
            {
                var dataCache = GetUserDataJunctionCache(includeUserAndGroup);

                if (includeUserAndGroup)
                {
                    this.ExportUserCache(output);
                    this.ExportGroupCache(output);
                }

                if (dataCache != null)
                {
                    output.WriteMetadata(AveMetadataType.DocDataJunction, dataCache);
                }
            }
        }

        internal void ExportUserCache(IAveBackupStream output)
        {
            var users = GetUserCache();
            if (users.Users.Count > 0)
            {
                output.WriteMetadata(AveMetadataType.UserCache, users);
            }
        }

        internal void ExportGroupCache(IAveBackupStream output)
        {
            var groups = GetGroupCache();
            if (groups.Groups.Count > 0)
            {
                output.WriteMetadata(AveMetadataType.GroupCache, groups);
            }
        }

        /// <param name="includeUserAndGroup">是否先备份相关的User和Group，避免还原的时候不存在</param>
        public void ExportRoleAssignments(IAveBackupStream output, bool includeUserAndGroup)
        {
            if (includeUserAndGroup)
            {
                this.CachePrincipalFromPermission();
                this.ExportUserCache(output);
                this.ExportGroupCache(output);
            }
            AveRoleAssignments roleAssignmetns = AveRoleAssignments.CreateInstance(this);
            roleAssignmetns.Export(output);
        }

        public void CachePrincipalFromPermission()
        {
            if (isRoleAssignmentCached)
            {
                return;
            }
            isRoleAssignmentCached = true;
            if (RoleAssignmentCache == null)
            {
                AveRoleAssignments roleAssignments = AveRoleAssignments.CreateInstance(this);
                RoleAssignmentCache = roleAssignments.GetRoleAssignments();
            }
            if (RoleAssignmentCache == null)
            {
                return;
            }
            for (int i = 0; i < RoleAssignmentCache.Count; ++i)
            {
                try
                {
                    int principalId = RoleAssignmentCache[i].PrincipalId;
                    if (!DataCache.principalIdAlreadyExists(principalId))
                    {
                        AveUserInfo user = mAveSPList.ParentWeb.ParentSite.DataCache.GetPrincipalInfo(principalId) as AveUserInfo;
                        if (user != null)
                        {
                            DataCache.AddToCache(principalId, user);
                            continue;
                        }
                        AveGroupInfo group = mAveSPList.ParentWeb.ParentSite.DataCache.GetPrincipalInfo(principalId) as AveGroupInfo;
                        if (group != null)
                        {
                            if (AveSPUtility.MatchShareLink.IsMatch(group.Title))
                            {
                                AddToDataCacheForSharingLinkGroup(principalId, group);
                            }
                            else
                            {
                                DataCache.AddToCache(principalId, group);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while add principal to cache. \n error message:{0}", e));
                }
            }
        }
        #endregion

        private void ExportContentByAPI(IAveBackupStream output, int uiVersion)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPItem.ExportContentByAPI"))
            {
                Stream stream = GetContent(uiVersion);
                if (stream != null)
                {
                    try
                    {
                        //if (streamConvertor != null)
                        //{
                        //    stream = streamConvertor.Process(mAveSPList.SPList, stream, Path.GetFileName(mBaseItemInfo.ServerRelativeUrl));
                        //}

                        mBaseItemInfo.DocumentSize = stream.Length;
                        byte[] buffer = output.DataBuffer;
                        int length;
                        output.FlushMetadata(stream.Length);
                        long readSize = 0;
                        while (readSize < stream.Length)//(length = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            length = stream.Read(buffer, 0, buffer.Length);
                            if (length == 0)
                            {
                                break;
                            }
                            readSize += length;
                            output.WriteContent(buffer, 0, length);
                        }
                        //output.FlushMetadata((int)stream.Length);
                        //long readSize = 0;
                        //while (readSize < stream.Length)//(length = stream.Read(buffer, 0, buffer.Length)) > 0)
                        //{
                        //    length = stream.Read(buffer, 0, buffer.Length);
                        //    if (length == 0)
                        //    {
                        //        break;
                        //    }
                        //    readSize += length;
                        //    output.WriteContent(buffer, 0, length);
                        //}
                    }
                    finally
                    {
                        stream.Dispose();
                    }
                }
                else
                {
                    output.FlushMetadata(0);
                }
            }
        }

        private Stream GetContent(int uiVersion)
        {
            Stream stream = null;
            int userId = -1;
            //bool needDispose = false;
            IAveFile file;
            IAveSite site = AveSPList.ParentSite.SPSite;
            IAveWeb web = AveSPList.ParentWeb.SPWeb;
            Guid fileId = mBaseItemInfo.GUID;
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.ExportContent.GetFile"))
            {
                file = this.mItem.GetVirtualFile();
            }
            try
            {
                if (file.Exists)
                {
                    using (AvePerformanceScope scope = new AvePerformanceScope("Backup.ExportContent.GetStream"))
                    {
                        if (file.UIVersion == uiVersion)
                        {
                            stream = file.OpenBinaryStream(WrapperConfiguration.OpenBinaryOptions);//AveOpenBinaryOptions.SkipVirusScan | AveOpenBinaryOptions.Unprotected);
                        }
                        else
                        {
                            stream = file.OpenVersionBinaryStream(uiVersion);
                        }
                    }
                }
            }
            finally
            {
                //Sonor Report 当前If 永远为False,由于当前类比较偏底层,如果选择释放需要进一步检查影响范围,进行测试
                //if (needDispose)
                //{
                //    if (web != null)
                //    {
                //        web.Dispose();
                //    }
                //    if (site != null)
                //    {
                //        site.Dispose();
                //    }
                //}
            }
            return stream;
        }

        internal void AddListViewFieldsInfoToDictionary(Dictionary<string, string> vFields, Dictionary<string, object> nameToColname, Dictionary<string, object> tempUserData, IAveFieldCollection curSPFields, string iName)
        {
            object obj = null;
            IAveField baseCurField = curSPFields.GetFieldByInternalName(iName);
            string temp = baseCurField.Title;
            if (nameToColname.TryGetValue(temp, out obj))
            {
                string uN = obj.ToString();
                object uV = null;
                if (uN.Equals("tp_UIVersionString", StringComparison.OrdinalIgnoreCase))
                {
                    uN = "tp_UIVersion";
                    if (tempUserData.TryGetValue(uN, out uV))
                    {
                        string ver = "Version";
                        if (uV is int)
                        {
                            int v = (int)uV;
                            if (v % 512 == 0)
                            {
                                v = v / 512;
                                vFields.Add(ver, v.ToString() + ".0");
                            }
                            else
                            {
                                int r = v % 512;
                                v = v / 512;
                                vFields.Add(ver, v.ToString() + "." + r.ToString());
                            }
                        }
                    }
                }
                else if (tempUserData.TryGetValue(uN, out uV))
                {
                    if (uV == null || string.IsNullOrEmpty(uV.ToString()))
                    {
                        return;
                    }
                    if (!vFields.ContainsKey(temp))
                    {
                        string value = string.Empty;
                        if (TryGetValueFromTaxonomyField(baseCurField, curSPFields, nameToColname, tempUserData, uV, out value))
                        {
                            vFields.Add(temp, value);
                        }
                        else if (temp.Equals("Created By") || temp.Equals("Modified By"))
                        {
                            try
                            {
                                vFields.Add(temp, mAveSPList.SPList.ParentWeb.SiteUsers.GetByID((int)uV).LoginName);
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperBackupResource.AWBAddFieldError, e.ToString());
                            }
                        }
                        else
                        {
                            vFields.Add(temp, mAveSPList.SPList.Fields.GetFieldByInternalName(iName).GetFieldValueAsText(uV));
                        }
                    }
                }
            }
        }

        /*private void ExportContentByNative(IAveBackupStream output)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPItem.ExportContentByNative"))
            {
                if (IsRbsArchivedData)//如果是RBS数据并且备份的是link就不必从数据库取内容备份
                {
                    output.FlushMetadata(0);
                    return;
                }
                byte[] buffer = output.DataBuffer;
                mBaseItemInfo.DocumentSize = 0;
                int size = 0;

                IAveQueryDataReader dr = mQueryService.ExportContentByNative(mBaseItemInfo, InternalVersion);
                try
                {
                    if (dr.Read())
                    {
                        if (dr.IsDBNull(0))
                        {
                            output.FlushMetadata(0);
                            return;
                        }
                        size = (int)dr.GetInt64(0);
                        if (size == 0)
                        {
                            output.FlushMetadata(0);
                            return;
                        }
                        output.FlushMetadata(size);
                        while (true)
                        {
                            int length = 0;
                            try
                            {
                                length = (int)dr.GetBytes(1, mBaseItemInfo.DocumentSize, buffer, 0, buffer.Length);
                            }
                            catch (Exception ex)
                            {
                                log.Warn("An error occurred when getting document content. Current size:{0}, Total Size:{1}, Reason:{2}.", mBaseItemInfo.DocumentSize, size, ex);
                                dr.Dispose();
                                dr = mQueryService.ExportContentByNative(mBaseItemInfo, InternalVersion);
                                length = (int)dr.GetBytes(1, mBaseItemInfo.DocumentSize, buffer, 0, buffer.Length);
                            }
                            if (length <= 0)
                            {
                                break;
                            }
                            output.WriteContent(buffer, 0, length);
                            mBaseItemInfo.DocumentSize += length;
                        }
                    }
                    else
                    {
                        output.FlushMetadata(0);
                    }
                }
                finally
                {
                    if (dr != null)
                    {
                        dr.Dispose();
                        dr = null;
                    }
                }
                if (size != mBaseItemInfo.DocumentSize)
                {
                    log.Log(AveLogLevel.WARN, string.Format("The content length:{0} doesn't match the real size:{1}", size, mBaseItemInfo.DocumentSize));
                    //mLog.Warn("The content length '{0}' doesn't match the real size '{1}'", size, position);
                }
            }
        }*/

        /// <summary>
        /// Get item version's field value in default view.
        /// </summary>
        /// <returns></returns>
        /*public Dictionary<string, string> GetItemViewFields()
        {
            Dictionary<string, string> vFields = new Dictionary<string, string>();
            Dictionary<string, object> nameToColname = new Dictionary<string, object>();
            //Dictionary<string, object> nameToColname = mFields.GetNameToColNameMapping();
            //TODO

            Dictionary<string, object> tempUserData = this.GetUnReplaceUserData();
            IAveFieldCollection curSPFields = mAveSPList.SPList.Fields;

            if (mAveSPList.SPList.DefaultView != null && tempUserData != null)
            {
                foreach (string iName in mAveSPList.SPList.DefaultView.ViewFields)
                {
                    try
                    {
                        AddListViewFieldsInfoToDictionary(vFields, nameToColname, tempUserData, curSPFields, iName);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperBackupResource.AWBAddItemViewError, e.ToString());
                        continue;
                    }
                }
            }
            return vFields;
        }*/

        internal bool TryGetValueFromTaxonomyField(IAveField baseCurField, IAveFieldCollection curSPFields, Dictionary<string, object> nameToColname, Dictionary<string, object> tempUserData, object uV, out string value)
        {
            value = string.Empty;
            try
            {
                if (baseCurField is IAveTaxonomyField)
                {
                    IAveTaxonomyField taxField = baseCurField as IAveTaxonomyField;
                    if (!Guid.Empty.Equals(taxField.TextField) && curSPFields.Contains(taxField.TextField))
                    {
                        IAveField textField = curSPFields[taxField.TextField];
                        object textObjCol = null;
                        object taxRealValue = null;
                        if (nameToColname.TryGetValue(textField.Title, out textObjCol) && tempUserData.TryGetValue(textObjCol.ToString(), out taxRealValue))
                        {
                            IAveTaxonomyFieldValue va = mAveParentSite.ObjectModelFactory.CreateTaxonomyFieldValue(uV + ";#" + taxRealValue);
                            value = taxField.GetFieldValueAsText(va);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, WrapperBackupResource.AWBGetValueFromTaxonomyFieldError, baseCurField.ID, ex.ToString());
            }
            return false;
        }

        public Dictionary<string, object> GetUnReplaceUserData()
        {
            return oldUserData.Count > 0 ? oldUserData : null;
        }

        internal void ExportFullTextIndex(IAveBackupStream output, Dictionary<string, object> customFieldValues, FullTextIndexLevel level)
        {
            if (RowId == 0 || mAveSPList == null || mAveSPList.SPList == null)
            {
                return;
            }
#if PerformanceLog
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPItem.ExportFullTextIndex"))
            {
#endif
                var index = mAveSPList.AveIndexCache.GetIndex(this, level);
                if (customFieldValues != null)
                {
                    index.SetCustomColumnValues(customFieldValues);
                }
                output.WriteMetadata(AveMetadataType.FullTextIndex, index);
#if PerformanceLog
            }
#endif
        }

        internal void ExportUnavailableUserInCache(IAveBackupStream output)
        {
#if PerformanceLog
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPItem.ExportUnavailableUserInCache"))
            {
#endif
                AveUserList list = new AveUserList();
                if (DataCache == null)
                {
                    log.Error("DataCache is null");
                    return;
                }
                else if (DataCache.UserList == null)
                {
                    log.Error("DataCache.UserList is null");
                    return;
                }

                foreach (AveUserInfo userInfo in DataCache.UserList.Users)
                {
                    if (userInfo == null)
                    {
                        log.Error("userInfo is null");
                        continue;
                    }
                    if (mAveParentSite == null)
                    {
                        log.Error("mAveParentSite is null");
                        continue;
                    }
                    else if (mAveParentSite.SPSite == null)
                    {
                        log.Error("mAveParentSite.SPSite is null");
                        continue;
                    }
                    if (!mAveParentSite.SPSite.CheckUserIfAvailable(userInfo.ID))
                    {
                        list.Users.Add(userInfo);
                    }
                }

                if (list.Users.Count > 0)
                {
                    output.WriteMetadata(AveMetadataType.UserCache, list);
                }

#if PerformanceLog
            }
#endif
        }

        internal void ExportStorageInfo(IAveBackupStream output)
        {
            var info = StorageInfo;
            output.WriteMetadata(AveMetadataType.DocStorageInfo.ToString(), info);
        }

        public Dictionary<string, object> GetAllColumnValues(ColumnsLevel getLevel)
        {
            if (RowId == 0 || mAveSPList == null || mAveSPList.SPList == null)
            {
                return null;
            }
            return mAveSPList.AveIndexCache.GetAllColumnValues(this, getLevel);
        }

        internal Dictionary<string, string> GetMetaInfo()
        {
            var allDocData = GetDocInfo();
            if (allDocData == null || !allDocData.ContainsKey("MetaInfo"))
            {
                return null;
            }
            byte[] bts = (byte[])allDocData["MetaInfo"];
            string metaInfo = string.Empty;
            if (AveCompressedUtility.IsTCompressedBytes(bts))
            {
                metaInfo = AveCompressedUtility.GetTCompressedString(bts);
            }
            else
            {
                metaInfo = Encoding.UTF8.GetString(bts);
            }
            return AveCompressedUtility.GetMetaInfoDictionary(metaInfo);
        }

        internal void ExportDataToExcel(string path)
        {
            try
            {
                if (!IsVersion)
                {
                    if (UserDataCache == null)
                    {
                        UserDataCache = GetUserData();
                    }
                    if (UserDataCache != null && UserDataCache.ContainsKey("#tp_ID"))
                    {
                        Dictionary<string, object> userData = new Dictionary<string, object>();
                        userData["#tp_ID"] = UserDataCache["#tp_ID"];
                        foreach (string fieldName in mAveSPList.FieldNameTypeDic.Keys)
                        {
                            if (mAveSPList.metadataFields.ContainsValue(fieldName) || !UserDataCache.ContainsKey(fieldName))
                            {
                                try
                                {
                                    string fieldType = (mAveSPList.FieldNameTypeDic[fieldName]).TypeAsString;
                                    if (fieldType.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase))
                                    {
                                        string tempValue = string.Empty;
                                        IAveFieldLookupValueCollection collection = SPListItem[fieldName] as IAveFieldLookupValueCollection;
                                        ArgumentNullException.ThrowIfNull(collection);
                                        foreach (IAveFieldLookupValue value in collection)
                                        {
                                            tempValue += value.LookupValue + ";#";
                                        }
                                        userData[fieldName] = tempValue.Substring(0, tempValue.Length - 2);
                                    }
                                    else if (fieldType.Equals("UserMulti", StringComparison.OrdinalIgnoreCase))
                                    {
                                        string tempValue = string.Empty;
                                        IAveFieldUserValueCollection collection = SPListItem[fieldName] as IAveFieldUserValueCollection;
                                        AvePoint.Wrapper.Common.ArgumentCheck.CheckNotNull(collection);
                                        foreach (IAveFieldUserValue value in collection)
                                        {
                                            //int iId = value.LookupId;
                                            //string user = string.Empty;
                                            //if (this.mAveParentSite.DataCache.UserCache.Contains(iId))
                                            //{
                                            //    user = this.mAveParentSite.DataCache.UserCache.GetUserInfo(iId).Login;
                                            //}
                                            //else
                                            //{
                                            //    user = this.mAveSPList.ParentWeb.SPWeb.Users.GetByID(iId).LoginName;
                                            //}
                                            tempValue += value.LookupValue + ";#";
                                        }
                                        userData[fieldName] = tempValue.Substring(0, tempValue.Length - 2);
                                    }

                                    #region ADO-36920 ​Excel中metadata类型column值和sharepoint显示不一致

                                    else if (fieldType.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)
                                        || fieldType.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                                    {
                                        object tempMetadataValue = SPListItem[fieldName];
                                        if (tempMetadataValue is IAveTaxonomyFieldValue)
                                        {
                                            userData[fieldName] = (tempMetadataValue as IAveTaxonomyFieldValue).Label;
                                        }
                                        else if (tempMetadataValue is IAveTaxonomyFieldValueCollection)
                                        {
                                            StringBuilder builder = new StringBuilder();
                                            bool flag = true;
                                            foreach (IAveTaxonomyFieldValue value in tempMetadataValue as IAveTaxonomyFieldValueCollection)
                                            {
                                                if (value == null)
                                                {
                                                    continue;
                                                }
                                                if (flag)
                                                {
                                                    flag = false;
                                                }
                                                else
                                                {
                                                    builder.Append(';');
                                                }
                                                builder.Append(value.Label);
                                            }
                                            userData[fieldName] = builder.ToString();
                                        }
                                        else if (SPListItem[fieldName] != null)
                                        {
                                            userData[fieldName] = tempMetadataValue.ToString();
                                        }
                                    }

                                    #endregion ADO-36920 ​Excel中metadata类型column值和sharepoint显示不一致

                                    else
                                    {
                                        userData[fieldName] = SPListItem[fieldName].ToString();
                                    }
                                }
                                catch (Exception e)
                                {
                                    log.Warn("ExportDataToExcel Error.Exception:" + e.ToString());
                                }
                            }
                            else
                            {
                                userData[fieldName] = UserDataCache[fieldName];
                            }
                        }
                        mAveSPList.ExportItemDataToExcel(userData, path);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("ExportDataToExcel Error.Exception:" + ex.ToString());
            }
        }

        public static void ClearRestoringItemCurrentVersionDocData()
        {
            if (RestoringItemCurrentVersionDocData != null)
            {
                RestoringItemCurrentVersionDocData.Clear();
                RestoringItemCurrentVersionDocData = null;
            }
        }

        internal List<AveTermStoreInfo> GetMetadataInfo(AveBackupOption backupColumnOption)
        {
            using (AvePerformanceScope ps = new AvePerformanceScope("Backup.AveSPItem.GetMetadataInfo"))
            {
                {
                    var taxonomyFields = this.AveSPList.TaxonomyFields;
                    var userData = this.UserDataCache;
                    List<AveTaxFieldInfo> infos = new List<AveTaxFieldInfo>();
                    foreach (var kv in taxonomyFields)
                    {
                        AveTaxFieldInfo taxInfo = kv.Value.Clone();
                        taxInfo.TermIds = new List<Guid>();
                        string[] termNames = new string[0];
                        if (userData.ContainsKey(taxInfo.TextFieldInternalName) && userData[taxInfo.TextFieldInternalName] != null)
                        {
                            termNames = userData[taxInfo.TextFieldInternalName].ToString().Split(';');
                        }
                        foreach (string termName in termNames)
                        {
                            if (string.IsNullOrEmpty(termName))
                            {
                                continue;
                            }
                            string tName = termName.StartsWith("#", StringComparison.Ordinal) ? termName.Substring(1) : termName;
                            if (tName.Contains("|"))
                            {
                                string[] temp = tName.Split('|');
                                if (temp.Length == 2)
                                {
                                    taxInfo.TermIds.Add(new Guid(temp[1]));
                                }
                                else
                                {
                                    log.Warn("Metadata column value is illegal.Column Value:{0}", tName);
                                }
                            }
                        }
                        if (taxInfo.TermIds.Count > 0)
                        {
                            infos.Add(taxInfo);
                        }
                    }
                    return mItem.GetRelatedMetadataInfo(infos, backupColumnOption);
                }
            }
        }
    }

    public class FullTextIndex
    {
        public string VersionComment { get; set; }

        public string CreatedByDisplayName { get; set; }

        public string CreatedByLoginName { get; set; }

        public string ModifiedByDisplayName { get; set; }

        public string ModifiedByLoginName { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public string TimeZoneInfoID { get; set; }

        #region Only for Archiver

        public string ArchiveBy { get; set; }

        public DateTime ArchiveTime { get; set; }

        #endregion Only for Archiver

        public int Size { get; set; }

        public string ContentTypeName { get; set; }

        public List<string> Attachments { get; set; }

        public Dictionary<string, object> ColumnValues { get; set; }

        public void SetCustomColumnValues(Dictionary<string, object> customColumnValues)
        {
            if (customColumnValues == null)
            {
                throw new ArgumentNullException("customColumnValues");
            }
            if (this.ColumnValues == null)
            {
                this.ColumnValues = new Dictionary<string, object>(customColumnValues.Count);
            }
            customColumnValues.ToList().ForEach(
                            pair =>
                            {
                                switch (pair.Key)
                                {
                                    //ArchiveBy,ArchiveTime特殊处理一下
                                    case AveWrapperConstants.ARCHIVE_BY:
                                        this.ArchiveBy = pair.Value.ToString();
                                        break;
                                    case AveWrapperConstants.ARCHIVE_TIME:
                                        this.ArchiveTime = (DateTime)pair.Value;
                                        break;
                                    default:
                                        this.ColumnValues[pair.Key] = pair.Value;
                                        break;
                                }
                            });//Overwrite field
        }
    }

    public enum FullTextIndexLevel
    {
        BaseInfo = 0,
        IncludeDefaultViewColumns = 1,
        IncludeAllVisiableColumns = 2,
        IncludeAllColumns = 3,
        IncludeAllColumnsAndSystemColumns = 4,
    }

    /// <summary>
    /// AllVisiableColumns 是非隐藏的所有column，获取是column的Displayname;
    /// AllColumns 指所有的column，包括隐藏和系统column，获取的key 是column的InternalName.
    /// </summary>
    public enum ColumnsLevel
    {
        None = 0,
        AllVisiableColumns = 1,
        AllColumns = 2,
        DisplayColumns = 3,
    }
}