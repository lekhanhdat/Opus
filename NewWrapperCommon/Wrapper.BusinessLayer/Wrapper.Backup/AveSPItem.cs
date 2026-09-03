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
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Core.SPBackup;
using AvePoint.Wrapper.Core.SPBackupDto;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Globalization;
using AvePoint.Wrapper.Resource.Backup;

namespace AvePoint.Wrapper.Backup
{
    [AveCodeReview("2012/03/1", "gqsun@avepoint.com", "", new string[] { CodeReviewConstants.CHECK_LIST_ID_CS_1 }, "ADO-26504", true)]
    public class AveSPItem : AvePoint.Wrapper.Backup.IAveSPItem
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private AveItemType mItemType;
        private IAveBackupStream mSender;
        private IAveBackupRestoreQueryService mQueryService;
        private AveSPListFieldCollection mFields;
        private bool mFirstTime = true;
        private AveSPList mAveSPList;
        private IAveListItem mSPListItem;
        private AveStorageInfo mStorageInfo;
        private AveStorageInfo13 mStorageInfo13;
        private byte[] mRbsId;
        private List<AveRBSStubInfo13> mStubInfo;
        private bool mIsBackupLinkForArchivedData;
        private int mStreamSchema;
        private bool? isLinkFile;

        public AveItemDataCache DataCache = new AveItemDataCache();

        private Dictionary<string, object> mUserDataCache;
        public Dictionary<string, object> UserDataCache
        {
            get
            {
                if (mItemType == AveItemType.ListItem)
                {
                    GetListItemInfo();//为了获取item的IsVersion的值，需要获取DocInfo，如果之前获取过DocInfo，在这里也不会异常。
                }
                else
                {
                    GetDocInfo(); //为了获取item的IsVersion的值，需要获取DocInfo，如果之前获取过DocInfo，在这里也不会异常。
                }
                if (mUserDataCache == null)
                {
                    mUserDataCache = GetUserData();
                }
                return mUserDataCache;
            }
        }

        private Dictionary<string, object> mDocDataCache;
        public Dictionary<string, object> DocDataCache
        {
            get
            {
                if (this.mDocDataCache == null)
                {
                    this.mDocDataCache = Item.GetDocInfo(BaseItemInfo, AveSPItem.RestoringItemCurrentVersionDocData);
                    if (mDocDataCache.ContainsKey("StreamSchema"))
                    {
                        var streamSchema = mDocDataCache["StreamSchema"];
                        try
                        {
                            mStreamSchema = Convert.ToInt32(streamSchema) & 1;
                        }
                        catch (Exception ex)
                        {
                            log.Warn("Convert StreamSchema error. Schema:{0} error:{1}", streamSchema, ex);
                        }
                        mDocDataCache.Remove("StreamSchema");
                    }
                    mDocDataCache["IsLinkFile"] = NeedBackupLinkFileRealContent;
                }
                if (ExtraPropertyInDataCache != null)
                {
                    ExtraPropertyInDataCache(mDocDataCache, AveSPItem.RestoringItemCurrentVersionDocData);
                }
                return this.mDocDataCache;

            }
        }

        private Dictionary<string, object> mListItemDataCache;
        public Dictionary<string, object> ListItemDataCache
        {
            get
            {
                if (this.mListItemDataCache == null)
                {
                    this.mListItemDataCache = Item.GetListItemInfo(BaseItemInfo, AveSPItem.RestoringItemCurrentVersionDocData);
                    if (mListItemDataCache.ContainsKey("StreamSchema"))
                    {
                        try
                        {
                            mStreamSchema = Convert.ToInt32(mListItemDataCache["StreamSchema"]) & 1;
                        }
                        catch (Exception ex)
                        {
                            log.Warn("Convert StreamSchema error. Schema:{0} error:{1}", mListItemDataCache["StreamSchema"].ToString(), ex.ToString());
                        }
                        mListItemDataCache.Remove("StreamSchema");
                    }
                }
                if (ExtraPropertyInDataCache != null)
                {
                    ExtraPropertyInDataCache(mListItemDataCache, AveSPItem.RestoringItemCurrentVersionDocData);
                }
                return this.mListItemDataCache;

            }
        }


        private bool UserDatajunctionCacheInited;
        private List<Dictionary<string, object>> mUserDatajunctionCache;
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
        private bool isRoleAssignmentCached;
        public List<Dictionary<string, object>> ImmedSubscriptionsCache = null;
        public List<Dictionary<string, object>> SchedSubscriptionsCache = null;
        private List<AveWebPartBaseInfo> mWebPartInfos;
        private AveBaseItemInfo mBaseItemInfo;
        private Dictionary<string, object> oldUserData = new Dictionary<string, object>(); // // used to save data in ALLUserData
        private AveSPSite mAveParentSite;
        private bool mBackupPagesFullContent = true;
        public bool IsPRItemBackup { get; set; }

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

        private bool mIsThirdStub;

        public bool IsThirdStub
        {
            get { return mIsThirdStub; }
        }

        private bool mIsSpecailData;

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "File extension")]
        public bool IsSpecialData
        {
            get
            {
                using (AvePerformanceScope s1 = new AvePerformanceScope("Backup.AveSPItem.IsSpecialData"))
                {
                    //if (this.Item != null && this.Item.ListItem != null && !String.IsNullOrEmpty(this.Item.ListItem.Name) && Path.GetExtension(this.Item.ListItem.Name).Equals(".stp", StringComparison.OrdinalIgnoreCase))
                    if (!String.IsNullOrEmpty(this.BaseItemInfo.Name) && Path.GetExtension(this.BaseItemInfo.Name).Equals(".stp", StringComparison.OrdinalIgnoreCase))
                    {
                        mIsSpecailData = true;
                    }
                    return mIsSpecailData;
                }
            }
        }

        private Nullable<AveStorageType> mStorageType;

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

        public AveStorageInfo13 StorageInfo13
        {
            get
            {
                if (mStorageInfo13 == null)
                {
                    mStorageInfo13 = GetStorageInfo13();
                }
                mStorageInfo13.Size = mBaseItemInfo.DocumentSize;
                return mStorageInfo13;
            }
        }

        public string ScopeUrl
        {
            get { return BaseItemInfo.ScopeUrl; }
            set { BaseItemInfo.ScopeUrl = value; }
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

        public bool PageVersion
        {
            get { return mBaseItemInfo.PageVersion; }
            set { mBaseItemInfo.PageVersion = value; }
        }

        private bool mRbsIdInited;
        private bool mRbsIdListInited;

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

        public List<AveRBSStubInfo13> StubInfo
        {
            get
            {
                if (!mRbsIdListInited)
                {
                    using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.RbsIdList"))
                    {
                        mRbsIdListInited = true;
                        mStubInfo = this.Item.GetRbsIdListByNative(BaseItemInfo);
                    }
                }
                return mStubInfo;
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

        public IAveBackupStream Sender
        {
            get { return this.mSender; }
            set { this.mSender = value; }
        }

        #region ThreadStatic Dictionary 缓存类型, 并且有当前状态信息, 类似SPContext.Current, 需要保证每个线程一份
        /// <summary>
        /// current version doc data of restoring item
        /// </summary>
        [ThreadStatic]
        internal static Dictionary<string, object> RestoringItemCurrentVersionDocData;

        /// <summary>
        /// current version doc data of parent of restoring item
        /// </summary>
        [ThreadStatic]
        internal static Dictionary<string, object> RestoringItemParentCurrentVersionDocData;

        /// <summary>
        /// Item's all version numbers.
        /// </summary>
        [ThreadStatic]
        internal static Dictionary<Guid, object> ItemVersionNumbers;
        #endregion

        public bool BackupPagesFullContent
        {
            set { mBackupPagesFullContent = value; }
        }

        public List<AveWebPartBaseInfo> WebPartInfos
        {
            get { return mWebPartInfos; }
        }

        public AddExtraPropertyInDataCache ExtraPropertyInDataCache { get; set; }

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
            : this(id, rowId, version, null, itemType, parentId, siteId, aveList, stream, queryService, fields, aveList.SolutionStatus)
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
            Dictionary<Guid, int> solutionStatus, IAveFolder parentFolder = null)
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
                mSender = stream;
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
                    //当前Item的ParentFolder的ParentFolder的UniqueId，为了查询ParentFolder记录使用索引。
                    //todo:qlluo:测试效率。
                    var grandfatherId = Guid.Empty;
                    if (parentFolder == null)
                    {
                        parentFolder = aveList.ParentWeb.SPWeb.GetFolder(parentId, itemType == AveItemType.Attachement ? -1 : rowId, aveList.Title.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase) ? aveList.ParentWeb.SPWeb.ServerRelativeUrl : aveList.ServerRelativeUrl);
                    }
                    if (parentFolder != null)
                    {
                        try
                        {
                            mBaseItemInfo.ParentFolderRelativeUrl = parentFolder.ServerRelativeUrl;
                            grandfatherId = parentFolder.ParentFolder != null ? parentFolder.ParentFolder.UniqueId : Guid.Empty;
                        }
                        catch (Exception ex)
                        {
                            AveSPUtility.ExceptionNeedNotLog(ex);
                            mBaseItemInfo.ParentFolderRelativeUrl = aveList.Title.Equals("{System Folder}", StringComparison.OrdinalIgnoreCase) ? aveList.ParentWeb.SPWeb.ServerRelativeUrl : aveList.ServerRelativeUrl;
                        }
                    }
                    mItem = mAveParentSite.ObjectModelFactory.CreateAveItem(mBaseItemInfo, parentFolder, mAveSPList.ParentWeb.SPWeb, mAveSPList.SPList);
                    mAveSPList.EnsureRestoringItemCurrentVersionDocData(mBaseItemInfo, mItem, grandfatherId);
                }
            }
        }

        public AveSPItem(
            Guid id,
            int rowId,
            int version,
            int level,
            string serverRelativeurl,
            AveItemType itemType,
            Guid parentId,
            Guid siteId,
            AveSPList aveList,
            IAveBackupStream stream,
            IAveBackupRestoreQueryService queryService,
            AveSPListFieldCollection fields,
            Dictionary<Guid, int> solutionStatus,
            IAveFolder parentFolder = null)
            : this(id, rowId, version, serverRelativeurl, itemType, parentId, siteId, aveList, stream, queryService, fields, solutionStatus, parentFolder)
        {
            mBaseItemInfo.Level = level;
        }

        public Dictionary<string, object> GetAttachmentInfo()
        {
            //do not use sender.cache  since it is not thread safe
            Dictionary<string, object> dataCache = new Dictionary<string, object>();
            dataCache = mItem.GetAttachmentInfo(BaseItemInfo);
            if (ExtraPropertyInDataCache != null)
            {
                Dictionary<string, object> tmpDic = new Dictionary<string, object>();
                tmpDic.Add("Id", this.BaseItemInfo.GUID);
                ExtraPropertyInDataCache(dataCache, tmpDic);
            }
            return dataCache;
        }

        //we must GetDocInfo before we cache the UserData.
        public Dictionary<string, object> GetDocInfo()
        {
            return this.DocDataCache;
        }

        public Dictionary<string, object> GetListItemInfo()
        {
            return this.ListItemDataCache;
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
                return new Dictionary<string, object>();
            }

            //Dictionary<string, object> dataCache = mSender.DataCache;
            Dictionary<string, object> userData = mItem.GetUserData(BaseItemInfo);
            if (userData != null && userData.Count > 0)
            {
                if (mFirstTime)
                {
                    AddAccessRequestPrincipalToCache(userData);
                    AddToCache(userData);
                    mFirstTime = false;
                }
                return userData;
            }
            return new Dictionary<string, object>();
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

        public List<Dictionary<string, object>> GetUserDataJunction()
        {
            return mItem.GetUserDataJunction(BaseItemInfo);
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

        public void ExportContent(IAveBackupStream output)
        {
            ExportContent(output, null);
        }

        public void ExportContent(IAveBackupStream output, IStreamConvertor streamConvertor)
        {
            if (NeedBackupLinkFileRealContent)
            {
                ExportLinkFileContent(output);
            }
            else if (this.mAveParentSite.SPContextKind.IsServerMode13Upper() || this.mAveParentSite.SPContextKind == AveContextKind.ClientObjectModel || !this.HasStream || (!IsBackupLinkForArchivedData && StorageType != AveStorageType.None) || mIsThirdStub)            //Backup SP2013 File Content By API
            {
                if (IsPRItemBackup && mStreamSchema == 1 && (mAveSPList.SPList == null || !Convert.ToBoolean(mAveSPList.SPList.IsConnectorList)))
                {
                    ExportContentByAPI(output, Version, streamConvertor);
                    //ExportContentByNativeFor13PRItem(output);
                }
                else if (this.mAveParentSite.SPContextKind.IsServerMode13Upper() && IsBackupLinkForArchivedData && StorageType != AveStorageType.None)
                {
                    ExportContentByNativeFor13(output);
                }
                else
                {
                    ExportContentByAPI(output, Version, streamConvertor);
                }
            }
            else
            {
                ExportContentByNative(output);
            }
        }

        private void ExportContentByNativeFor13(IAveBackupStream output)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPItem.ExportContentByNative"))
            {
                long totalNativeContentSize = mQueryService.GetNativeContentSize(mBaseItemInfo);

                if (totalNativeContentSize == 0)
                {
                    output.FlushMetadata(0);
                    return;
                }
                else
                {
                    output.FlushMetadata(totalNativeContentSize);
                }

                //query all shreds for one document
                var ShredInfoList = mQueryService.GetShredInfo(mBaseItemInfo);

                foreach (var shredInfo in ShredInfoList)
                {
                    var dataReader = mQueryService.GetRBSIdOrContentOfOneShred(mBaseItemInfo, shredInfo);

                    try
                    {
                        if (dataReader == null)
                        {
                            //should throw exception ?
                            continue;
                        }

                        //rbs shred
                        if (shredInfo.RBSId != null)
                        {
                            continue;
                        }
                        else//read content
                        {
                            byte[] buffer = mSender.DataBuffer;
                            int offset = 0;
                            int size = 0;
                            if (dataReader.IsDBNull(1))
                            {
                                continue;
                            }

                            size = (int)dataReader.GetInt32(1);
                            if (size == 0)
                            {
                                continue;
                            }

                            while (true)
                            {
                                int length = 0;
                                try
                                {
                                    length = (int)dataReader.GetBytes(2, offset, buffer, 0, buffer.Length);
                                }
                                catch (Exception ex)
                                {
                                    log.Warn("An error occurred when getting document content. Current size:{0}, Total Size:{1}, BSN:{3},Reason:{2}.", mBaseItemInfo.DocumentSize, size, ex, shredInfo.BSN);
                                    dataReader.Dispose();
                                    dataReader = mQueryService.GetRBSIdOrContentOfOneShred(mBaseItemInfo, shredInfo);
                                    length = (int)dataReader.GetBytes(2, offset, buffer, 0, buffer.Length);
                                }
                                if (length <= 0)
                                {
                                    break;
                                }
                                output.WriteContent(buffer, 0, length);
                                offset += length;
                            }
                        }
                    }
                    finally
                    {
                        if (dataReader != null)
                        {
                            dataReader.Dispose();
                            dataReader = null;
                        }
                    }

                }

            }
        }
        private void ExportLinkFileContent(IAveBackupStream output)
        {
            Stream stream = null;
            try
            {
                log.Debug("Get link file stream by connector API.");
                stream = this.AveSPList.SOIntegrationUtil.GetSourceFileStream(AveSPSite.SPSite.ID,Version, AveSPList.SPList.ParentWebUrl, AveSPList.Id, Id);
            }
            catch (Exception e)
            {
                log.Error("An error occurred while getting connector file link content.It may be broken. Error: {0}", e);
                throw;
            }
            if (stream != null)
            {
                try
                {
                    long size = stream.Length;
                    output.FlushMetadata(size);
                    try
                    {
                        CopyStream(output, stream);
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null)
                        {
                            log.Warn("An error occurred while export content, size:{0}, read length:{1}. exception:{2}\r\n--> Inner Exception:{3}", size, mBaseItemInfo.DocumentSize, ex.ToString(), ex.InnerException);
                        }
                        else
                        {
                            log.Warn("An error occurred while export content, size:{0}, read length:{1}. exception:{2}", size, mBaseItemInfo.DocumentSize, ex.ToString());
                        }
                        #region 一旦备份Content出错,必须填充Stream,否则Restore失败。
                        byte[] buffer = output.DataBuffer;
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] = 0;
                        }
                        while (size - mBaseItemInfo.DocumentSize > 0)
                        {
                            int length = (size - mBaseItemInfo.DocumentSize > buffer.Length) ? buffer.Length : (int)(size - mBaseItemInfo.DocumentSize);
                            output.WriteContent(buffer, 0, length);
                            mBaseItemInfo.DocumentSize += length;
                        }
                        #endregion
                        throw new AveWrapperException(AveInternalResourceKey.Wrapper_Exception_Backup_GetDocumentContentError);
                    }
                }
                finally
                {
                    stream.Dispose();
                }
            }
            else
            {
                throw new AveFileNotFoundException(AveInternalResourceKey.Wrapper_Exception_Server_FileNotFoundException);
            }
        }
        /// <summary>
        /// Reflect from connector code.
        /// </summary>
        /// <returns></returns>
        public bool IsConnectorLinkFile
        {
            get
            {
                if (!isLinkFile.HasValue)
                {

                    try
                    {
                        bool isConnectorList = this.mAveSPList.SPList != null
                        && this.mAveSPList.SPList.IsConnectorList.HasValue
                        && this.mAveSPList.SPList.IsConnectorList.Value;

                        if (isConnectorList && SPListItem != null && SPListItem.Name.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                        {
                            if (SPListItem.Fields.ContainsField("URL") && SPListItem["URL"] != null)
                            {
                                string downloadUrl = mAveSPList.ParentSite.ObjectModelFactory.CreateFieldUrlValue(SPListItem["URL"].ToString()).Url;
                                if (!string.IsNullOrEmpty(downloadUrl)
                                    && downloadUrl.IndexOf("FSDLDownload.aspx", StringComparison.OrdinalIgnoreCase) != -1)
                                {
                                    isLinkFile = true;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log.Warn("An error occurred while checking file is a link file or not. Error: {0}", e);
                    }
                    if (!isLinkFile.HasValue)
                    {
                        isLinkFile = false;
                    }
                }
                return isLinkFile.Value;
            }
        }
        private bool NeedBackupLinkFileRealContent
        {
            get
            {
                if (!ParentSite.BackupOption.BackupLinkFileRealContent 
                    || this.mAveParentSite.SPContextKind == AveContextKind.ClientObjectModel)
                {
                    return false;
                }
                return IsConnectorLinkFile;
            }
        }

        private void AddPricipleToDataCache(int principalId)
        {
            try
            {
                if (!DataCache.PrincipalIdAlreadyExists(principalId))
                {
                    var user = mAveSPList.ParentWeb.ParentSite.DataCache.GetUserInfo(principalId);
                    if (user != null)
                    {
                        user = AveUserUtility.ConvertDomainGroupSidToAccount(user, this.ParentSite.ObjectModelFactory);
                        DataCache.AddToCache(principalId, user);
                        return;
                    }
                    var group = mAveSPList.ParentWeb.ParentSite.DataCache.GetGroupInfo(principalId);
                    if (group != null)
                    {
                        DataCache.AddToCache(principalId, group);
                        return;
                    }
                    DataCache.AddToCache(principalId);
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while add principal to cache. \n error message:{0}", e));
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
                    var tempTitle = aveViewInfo.Title;
                    if (!string.IsNullOrEmpty(tempTitle) && tempTitle.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                    {
                        tempTitle = WrapperRuntime.CurrentContext.ModelFactory.Utility.GetLocalizedString(tempTitle, "core",
                                                                                                      (uint)
                                                                                                      CultureInfo.
                                                                                                          CurrentUICulture.
                                                                                                          LCID);
                    }
                    dataCache.Add("ViewTitle" + i, tempTitle);
                    dataCache.Add("IsMobileView" + i, aveViewInfo.IsMobileView);
                    dataCache.Add("IsDefaultMobileView" + i, aveViewInfo.IsDefaultMobileView);
                    dataCache.Add("Hidden" + i, aveViewInfo.Hidden);
                    if (aveViewInfo.MappingForSpotlight.Count > 0)
                    {
                        dataCache.Add("ListViewXml" + i, aveViewInfo.ListViewXml);//目前HtmlSchemaXml只给Spotlight使用。如果其他用到，在此处放开。
                        dataCache.Add("MappingForSpotlight" + i, aveViewInfo.MappingForSpotlight);
                    }
                    if (aveViewInfo.IsDefaultView.HasValue)
                    {
                        dataCache.Add("IsDefaultView" + i, aveViewInfo.IsDefaultView);
                    }
                    dataCache.Add("BaseViewId" + i, aveViewInfo.BaseViewId);
                    if (aveViewInfo.UserID.HasValue)
                    {
                        dataCache.Add("UserID" + i, aveViewInfo.UserID);
                        AddPricipleToDataCache(aveViewInfo.UserID.Value);
                    }
                    ++i;
                }
                listViewCache.Remove(this.Id);
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
                AveRBSStubInfo RBSInfo = mAveParentSite.RBSBackup.BackupRBSStub(RbsId);
                byte blobType = RBSInfo.StoreBlobId[3];
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

        private AveStubDataType GetRBSDataType(byte[] RBSBlobId)
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
            if (IsEbsArchivedData)
            {
                return GetEBSDataType();
            }
            else if (IsRbsArchivedData)
            {
                return GetRBSDataType();
            }
            return AveStubDataType.UnKnown;
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

        private AveStorageInfo13 GetStorageInfo13()
        {
            AveStorageInfo13 info = new AveStorageInfo13();
            try
            {
                if (IsRbsArchivedData)
                {
                    info = mAveSPList.SOIntegrationUtil.BackupRBSStorageInfo13(mAveParentSite.SPSite, Id, Version, Level, mQueryService, this.RBSStubInfo13);
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
                    try
                    {
                        mRBSStubInfo = mAveParentSite.RBSBackup.BackupRBSStub(RbsId);
                    }
                    catch (Exception e)
                    {
                        //ADO-92627,如果出错则认为不是DocAve的Stub
                        log.Warn("An error occurred while getting RBS stub information, {0}", e);
                    }
                }
                return mRBSStubInfo;
            }
        }

        private List<AveRBSStubInfo13> mRBSStubInfo13List;

        public List<AveRBSStubInfo13> RBSStubInfo13
        {
            get
            {
                if (mRBSStubInfo13List == null)
                {
                    mRBSStubInfo13List = mAveParentSite.RBSBackup.BackupRBSStub13(StubInfo);
                }
                return mRBSStubInfo13List;
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
            if (mAveSPList.SPList != null && mAveSPList.SPList.BaseTemplate == AveListTemplateType.UserInformation)
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
            if (UserDataCache.ContainsKey("Target_x0020_Audiences") && UserDataCache["Target_x0020_Audiences"] != null)
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
                    if (!DataCache.PrincipalIdAlreadyExists(principalId))
                    {
                        var user = mAveSPList.ParentWeb.ParentSite.DataCache.GetUserInfo(principalId);
                        if (user != null)
                        {
                            DataCache.AddToCache(principalId, user);
                            continue;
                        }
                        var group = mAveSPList.ParentWeb.ParentSite.DataCache.GetGroupInfo(principalId);
                        if (group != null)
                        {
                            DataCache.AddToCache(principalId, group);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while add principal to cache. \n error message:{0}", e));
                }
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
                    if (!DataCache.PrincipalIdAlreadyExists(userId))
                    {
                        AveUserInfo userInfo = mAveSPList.ParentWeb.ParentSite.DataCache.GetUserInfo(userId);
                        if (userInfo == null)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperBackupResource.AlertUserTypeInvalidate, userId);
                            continue;
                        }
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
                    if (!DataCache.PrincipalIdAlreadyExists(userId))
                    {
                        AveUserInfo userInfo = mAveSPList.ParentWeb.ParentSite.DataCache.GetUserInfo(userId);
                        if (userInfo == null)
                        {
                            log.Log(AveLogLevel.DEBUG, WrapperBackupResource.AlertUserTypeInvalidate, userId);
                            continue;
                        }
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
                if (!AveUrlUtility.IsAspx(BaseItemInfo.ServerRelativeUrl, false))
                {
                    return;
                }
                using (
                    IAveLimitedWebPartManager webpartManager =
                        this.ParentSite.ObjectModelFactory.CreateLimitedWebPartManager(this.ParentSite.SPSite,
                                                                                       this.AveSPList.ParentWeb.SPWeb,
                                                                                       this.BaseItemInfo
                                                                                           .ServerRelativeUrl))
                {
                    try
                    {
                        mWebPartInfos = webpartManager.GetWebParts(this.BaseItemInfo);
                    }
                    catch (Exception ex)
                    {
                        log.Log(AveLogLevel.WARN,
                                "An error occurred when backup web parts, Page:{0}, Version:{1}. Reason:{2}.",
                                this.BaseItemInfo.ServerRelativeUrl, this.BaseItemInfo.Version, ex);
                    }
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
                                if (!DataCache.PrincipalIdAlreadyExists(userId))
                                {
                                    AveUserInfo userInfo = mAveSPList.ParentWeb.ParentSite.DataCache.GetUserInfo(userId);
                                    if (userInfo != null)
                                    {
                                        DataCache.AddToCache(userId, userInfo);
                                    }
                                }
                            }
                            if (mWebPartInfos[i].Personalization != null && mWebPartInfos[i].Personalization.Count > 0)
                            {
                                for (int j = 0; j < mWebPartInfos[i].Personalization.Count; j++)
                                {
                                    int pUserId = mWebPartInfos[i].Personalization[j].UserID;
                                    if (!DataCache.PrincipalIdAlreadyExists(pUserId))
                                    {
                                        AveUserInfo pUserInfo = mAveSPList.ParentWeb.ParentSite.DataCache.GetUserInfo(pUserId);
                                        if (pUserInfo != null)
                                        {
                                            DataCache.AddToCache(pUserId, pUserInfo);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Log(AveLogLevel.WARN, "Can't get User:{0} for webpart :{1},Exception :{2}", mWebPartInfos[i].UserID, mWebPartInfos[i].ID, ex.ToString());
                        }
                        //备份WebPart相关User信息，如果是Group，则不会添加
                        if (mWebPartInfos[i].ExtensionProperties != null && mWebPartInfos[i].ExtensionProperties.Count > 0)
                        {
                            try
                            {
                                if (mWebPartInfos[i].ExtensionProperties.Keys.Contains("UserId"))
                                {
                                    int relatedUserId = Convert.ToInt32(mWebPartInfos[i].ExtensionProperties["UserId"]);
                                    if (!DataCache.PrincipalIdAlreadyExists(relatedUserId))
                                    {
                                        AveUserInfo userInfo = mAveSPList.ParentWeb.ParentSite.DataCache.GetUserInfo(relatedUserId);
                                        if (userInfo != null)
                                        {
                                            DataCache.AddToCache(relatedUserId, userInfo);
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.WARN, "Can't get User for webpart :{0}. Exception :{1}", mWebPartInfos[i].ID, e.ToString());
                            }
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
        [Obsolete]//外围调用将AveSPList.BackupItemTPGUIDofLookupValue置成true代替调用此方法
        public Dictionary<string, string> GetLookupFieldGuidValue()
        {
            if (mFields == null)
            {
                return null;
            }
            Dictionary<string, StringBuilder> lookupFieldGuidValue = new Dictionary<string, StringBuilder>();
            string name = string.Empty;
            foreach (KeyValuePair<string, object> pair in UserDataCache)
            {
                if (pair.Key.StartsWith("#", StringComparison.OrdinalIgnoreCase) || pair.Key.EndsWith("#2", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                name = pair.Key;
                try
                {
                    IAveField spField = mAveSPList.SPList.Fields.GetFieldByInternalName(name);
                    if (spField.BaseTypeString.Equals("Lookup", StringComparison.OrdinalIgnoreCase)
                        || spField.BaseTypeString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase))
                    {
                        IAveFieldLookup lookupField = spField as IAveFieldLookup;
                        Guid lookupListId = new Guid(lookupField.LookupList);
                        int rowId = pair.Value.ToString().Contains(";") ? Convert.ToInt32(pair.Value.ToString().Split(';')[0]) : Convert.ToInt32(pair.Value);
                        Guid tp_GUID = GetLookupGUIDById(lookupField.LookupWebId, lookupListId, rowId);
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
                    log.Log(AveLogLevel.WARN, "An error occurred while GetLookupFieldGuidValue in UserDataCache.field name:{0},error:{1}.", name, ex.ToString());
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
                        if (spField.BaseTypeString.Equals("Lookup", StringComparison.OrdinalIgnoreCase)
                            || spField.BaseTypeString.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase))
                        {
                            IAveFieldLookup lookupField = spField as IAveFieldLookup;
                            Guid lookupListId = new Guid(lookupField.LookupList);
                            Guid tp_GUID = GetLookupGUIDById(lookupField.LookupWebId, lookupListId, rowId);
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

        [Obsolete]
        public Guid GetLookupGUIDById(Guid lookupListId, int rowId)
        {
            return GetLookupGUIDById(Guid.Empty, lookupListId, rowId);
        }

        public Guid GetLookupGUIDById(Guid lookupWebId, Guid lookupListId, int rowId)
        {
            if (mQueryService != null)
            {
                return mQueryService.GetLookupGUIDById(this.mAveParentSite.SPSite.ID, lookupListId, rowId);
            }
            else if (lookupListId != Guid.Empty)
            {
                Guid itemTPGuid = mAveParentSite.GetLookupItemIdAndGuid(lookupWebId, lookupListId, rowId);
                if (itemTPGuid == Guid.Empty)
                {
                    log.Debug("Can not find the Lookup Item TPGuid. LookupWebId: {0}, LookupListId: {1}, ItemRowId: {2}", lookupWebId, lookupListId, rowId);
                }
                return itemTPGuid;
            }
            else
            {
                return Guid.Empty;
            }
        }

        private void AddAccessRequestPrincipalToCache(Dictionary<string, object> tempData)
        {
            if (this.mAveParentSite.SPContextKind.IsServerMode13Upper() && mAveSPList.SPList != null && (int)mAveSPList.SPList.BaseTemplate == 160)
            {
                if (tempData.ContainsKey("RequestedByUserId") && tempData["RequestedByUserId"] != null)
                {
                    int reqByUserId = (int)tempData["RequestedByUserId"];
                    AddPricipleToDataCache(reqByUserId);
                }
                if (tempData.ContainsKey("RequestedForUserId") && tempData["RequestedForUserId"] != null)
                {
                    int reqForUserId = (int)tempData["RequestedForUserId"];
                    AddPricipleToDataCache(reqForUserId);
                }
                if (tempData.ContainsKey("PermissionLevelRequested") && tempData["PermissionLevelRequested"] != null && ((string)tempData["PermissionType"]).Equals("SharePoint Group", StringComparison.OrdinalIgnoreCase))
                {
                    int groupId = (int)tempData["PermissionLevelRequested"];
                    AddPricipleToDataCache(groupId);
                }
            }
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
                if (mAveSPList.UserFields.Contains(name) && pair.Value != null)
                {
                    int userIdValue;
                    if (int.TryParse(pair.Value.ToString(), out userIdValue))
                    {
                        AddPricipleToDataCache(userIdValue);
                    }
                    else
                    {
                        log.Log(AveLogLevel.WARN, "{0} is not a valid user id. Value:{1}", name, pair.Value);
                    }
                }
            }
        }

        public void SetAttachmentInfo()
        {
            HasStream = true;
            IsVersion = false;
            Level = 1;
            mBaseItemInfo.DocumentSize = GetAttachmentSize();
        }

        private long GetAttachmentSize()
        {
            return mItem.GetAttachmentSize(mBaseItemInfo);
        }

        private void ExportContentByAPI(IAveBackupStream output, int uiVersion, IStreamConvertor streamConvertor)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPItem.ExportContentByAPI"))
            {
                Stream stream = null;
                try
                {
                    stream = GetContent(uiVersion);
                }
                catch (AveWrapperCheckoutFileException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    log.Error("The file content could not be found by SharePointAPI.It may be deleted or broken.Please check it by PowerShell. Error: {0}", e);
                    throw new AveFileNotFoundException(AveInternalResourceKey.Wrapper_Exception_Server_FileNotFoundException);
                }

                if (stream != null)
                {

                    try
                    {
                        if (streamConvertor != null)
                        {
                            stream = streamConvertor.Process(mAveSPList.SPList, stream, Path.GetFileName(mBaseItemInfo.ServerRelativeUrl));
                        }

                        mBaseItemInfo.DocumentSize = 0;
                        long size = stream.Length;
                        output.FlushMetadata(size);
                        try
                        {
                            CopyStream(output, stream);
                        }
                        catch (Exception ex)
                        {
                            if (ex.InnerException != null)
                            {
                                log.Warn("An error occurred while export content, size:{0}, read length:{1}. exception:{2}\r\n--> Inner Exception:{3}", size, mBaseItemInfo.DocumentSize, ex.ToString(), ex.InnerException);
                            }
                            else
                            {
                                log.Warn("An error occurred while export content, size:{0}, read length:{1}. exception:{2}", size, mBaseItemInfo.DocumentSize, ex.ToString());
                            }
                            //主要原因是13中出现很多次获取Content失败的情况，所以添加retry，然后把inner exception给打印出来。
                            #region retry when the document size is 0 from stream.
                            if (mBaseItemInfo.DocumentSize == 0)
                            {
                                using (AvePerformanceScope scope1 = new AvePerformanceScope("Backup.AveSPItem.RetryCopyStream"))
                                {
                                    try
                                    {
                                        stream.Dispose();
                                        stream = GetContent(uiVersion);
                                        if (stream.Length != size)
                                        {
                                            throw new AveWrapperException(string.Format("The current length is {0} which does not equal the previous {1} ", stream.Length, size));
                                        }
                                        CopyStream(output, stream);
                                    }
                                    catch (Exception secondException)
                                    {
                                        if (secondException.InnerException != null)
                                        {
                                            log.Warn("An error occurred while export content second time, size:{0}, read length:{1}. exception:{2}\r\n--> Inner Exception:{3}", size, mBaseItemInfo.DocumentSize, secondException.ToString(), secondException.InnerException);
                                        }
                                        else
                                        {
                                            log.Warn("An error occurred while export content second time, size:{0}, read length:{1}. exception:{2}", size, mBaseItemInfo.DocumentSize, secondException.ToString());
                                        }
                                    }
                                }

                                #endregion

                                byte[] buffer = output.DataBuffer;
                                for (int i = 0; i < buffer.Length; i++)
                                {
                                    buffer[i] = 0;
                                }
                                while (size - mBaseItemInfo.DocumentSize > 0)
                                {
                                    int length = (size - mBaseItemInfo.DocumentSize > buffer.Length) ? buffer.Length : (int)(size - mBaseItemInfo.DocumentSize);
                                    output.WriteContent(buffer, 0, length);
                                    mBaseItemInfo.DocumentSize += length;
                                }
                                throw new AveWrapperException(AveInternalResourceKey.Wrapper_Exception_Backup_GetDocumentContentError);
                            }
                        }

                    }

                    finally
                    {
                        stream.Dispose();
                    }
                }


                else
                {
                    //output.FlushMetadata(0); 
                    throw new AveFileNotFoundException(AveInternalResourceKey.Wrapper_Exception_Server_FileNotFoundException);
                }
            }
        }

        private void CopyStream(IAveBackupStream to, Stream from)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPItem.CopyStream"))
            {
                var mng = new CopyStreamManager(from, to);
                try
                {
                    mng.Copy();
                }
                finally
                {
                    mBaseItemInfo.DocumentSize = mng.CurrentLength;
                }
            }
        }

        class CopyStreamManager
        {
            private const int Limit = 1024 * 1024 * 1024;
            private Stream from;
            private IAveBackupStream to;
            private long totalLength;
            public long CurrentLength { get; private set; }
            private bool enableDiagnostics;
            private Stopwatch readWatch = new Stopwatch();
            private Stopwatch writeWatch = new Stopwatch();

            public CopyStreamManager(Stream from, IAveBackupStream to)
            {
                this.from = from;
                this.to = to;
                this.CurrentLength = 0L;
                this.totalLength = from.Length;
                this.enableDiagnostics = totalLength >= Limit;
            }

            private int Read(byte[] buffer, int offset, int count)
            {
                try
                {
                    StartMonitor(this.readWatch);
                    return this.from.Read(buffer, offset, count);
                }
                finally
                {
                    StopMonitor(this.readWatch);
                }
            }

            private void Write(byte[] buffer, int offset, int length)
            {
                try
                {
                    StartMonitor(this.writeWatch);
                    this.to.WriteContent(buffer, offset, length);
                }
                finally
                {
                    StopMonitor(this.writeWatch);
                }
            }

            private void StartMonitor(Stopwatch watch)
            {
                if (this.enableDiagnostics) watch.Start();
            }

            private void StopMonitor(Stopwatch watch)
            {
                if (this.enableDiagnostics) watch.Stop();
            }

            private void Log(Action action)
            {
                if (this.enableDiagnostics && action != null)
                {
                    action();
                }
            }

            public void Copy()
            {
                var buffer = to.DataBuffer;
                var progress = new ProgressLogger(this.totalLength);
                int count;
                while ((count = Read(buffer, 0, buffer.Length)) > 0)
                {
                    this.CurrentLength += count;
                    Write(buffer, 0, count);
                    Log(() => progress.LogOne(this.CurrentLength));
                }
                Log(() => log.Debug("Read elapsed: {0}s, write elapsed: {1}s", this.readWatch.ElapsedMilliseconds / 1000, this.writeWatch.ElapsedMilliseconds / 1000));
            }

        }

        public void ChangeCheckoutUserForPRItem(IAveSite site, IAveWeb web, int userId, Guid fileId)
        {
            RestoreCheckOutUser(site);
            site.CheckOutUser = userId;
            site.CheckOutFileId = fileId;
            QueryService.ChangeCheckoutUserID(site.ID, fileId, web.CurrentUser.ID);
        }

        /// <summary>
        /// for PRItem change checkout user back In backup
        /// </summary>
        /// <param name="site">parent site</param>
        private void RestoreCheckOutUser(IAveSite site)
        {
            if (site.CheckOutUser != -1 && site.CheckOutFileId != Guid.Empty)
            {
                int tempId = 0;
                if (QueryService.IsCheckOutFile(site.ID, site.CheckOutFileId, ref tempId))
                {
                    QueryService.ChangeCheckoutUserID(site.ID, site.CheckOutFileId, site.CheckOutUser);
                    log.Debug("Restore check out user for PRItem.FileID:{0},UserId:{1}", site.CheckOutFileId, site.CheckOutUser);
                    site.CheckOutFileId = Guid.Empty;
                    site.CheckOutUser = -1;
                }
            }
        }

        private Stream GetContent(int uiVersion)
        {
            Stream stream = null;
            int userId = -1;
            bool needDispose = false;
            IAveFile file;
            IAveSite site = AveSPList.ParentSite.SPSite;
            IAveWeb web = AveSPList.ParentWeb.SPWeb;
            Guid fileId = mBaseItemInfo.GUID;
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.ExportContent.GetFile"))
            {
                //ADO-16383处理Pages下的用其他用户CheckOut的file
                if (web.CurrentUser == null)
                {
                    log.Debug("Get file by no one");
                    file = web.GetFile(fileId);
                }
                else if (mQueryService != null && mQueryService.IsCheckOutVersion(site.ID, fileId, uiVersion, ref userId) && userId != web.CurrentUser.ID)
                {
                    if (!IsPRItemBackup)
                    {
                        IAveUser checkoutUser;
                        try
                        {
                            checkoutUser = web.SiteUsers.GetByID(userId);
                        }
                        catch (Exception ex)
                        {
                            throw new AveWrapperCheckoutFileException(ex.Message, ex) { ErrorType = CheckoutFileErrorType.CheckoutUserNotExist, SiteId = SiteId, WebId = web.ID, FileId = fileId, CheckoutUserId = userId };
                        }
                        IAveWeb checkoutWeb = site.GetCheckoutWeb(site.ID, web, checkoutUser, fileId, true);
                        if (checkoutWeb != null)
                        {
                            log.Debug("Get file by Checkout user");
                            file = checkoutWeb.GetFile(fileId);
                        }
                        else
                        {
                            log.Warn("can not get check out web for file {0} UIVersion {1}", fileId, uiVersion);
                            return null;
                        }
                    }
                    else
                    {
                        ChangeCheckoutUserForPRItem(site, web, userId, fileId);
                        log.Debug("Get file by agent account");
                        file = web.GetFile(fileId);
                    }
                }
                else
                {
                    log.Debug("Get file normally");
                    file = this.mItem.GetFile();//web.GetFile(fileId);
                }
            }
            try
            {
                if (file.Exists)
                {
                    using (AvePerformanceScope scope = new AvePerformanceScope("Backup.ExportContent.GetStream"))
                    {
                        //the content of version file is the same as the current version when has stream equals 0
                        if (file.UIVersion == uiVersion || !HasStream)
                        {
                            log.Debug("Get file stream normally");
                            stream = file.OpenBinaryStream(WrapperConfiguration.OpenBinaryOptions);//AveOpenBinaryOptions.SkipVirusScan | AveOpenBinaryOptions.Unprotected);
                        }
                        else if (file.CheckOutType != AveCheckOutType.None && file.UIVersion < uiVersion && this.mAveParentSite.SPContextKind != AveContextKind.ClientObjectModel)
                        {
                            //Open file binary by the check out user
                            site = mAveParentSite.ObjectModelFactory.CreateSite(site.Url, mAveParentSite.ObjectModelFactory.CreateUserToken(file.CheckedOutByUser.UserToken.BinaryToken));
                            web = site.OpenWeb(web.ID);
                            needDispose = true;
                            file = web.GetFile(fileId);

                            if (uiVersion == file.UIVersion)
                            {
                                log.Debug("Get file stream by checkout user");
                                stream = file.OpenBinaryStream(WrapperConfiguration.OpenBinaryOptions);//AveOpenBinaryOptions.SkipVirusScan | AveOpenBinaryOptions.Unprotected);
                            }
                            else
                            {
                                log.Debug("Get file version stream by checkout user");
                                stream = file.OpenVersionBinaryStream(uiVersion);
                            }
                        }
                        else
                        {
                            log.Debug("Get file version stream normally");
                            stream = file.OpenVersionBinaryStream(uiVersion);
                        }
                    }
                }
                else
                {
                    log.Debug("The {0} with GUID {1} is not found.", mBaseItemInfo.ItemType, mBaseItemInfo.GUID);
                }
            }
            finally
            {
                if (needDispose)
                {
                    if (web != null)
                    {
                        web.Dispose();
                    }
                    if (site != null)
                    {
                        site.Dispose();
                    }
                }
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
                        if (AvePoint.Common.AveEnv.IsMoss && TryGetValueFromTaxonomyField(baseCurField, curSPFields, nameToColname, tempUserData, uV, out value))
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

        private void ExportContentByNative(IAveBackupStream output)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPItem.ExportContentByNative"))
            {
                if (IsRbsArchivedData)//如果是RBS数据并且备份的是link就不必从数据库取内容备份
                {
                    output.FlushMetadata(0);
                    return;
                }
                byte[] buffer = new byte[65536];//mSender.DataBuffer;
                mBaseItemInfo.DocumentSize = 0;
                long size = 0;

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
                        size = (long)dr.GetInt64(0);
                        if (size == 0)
                        {
                            output.FlushMetadata(0);
                            return;
                        }
                        output.FlushMetadata(size);
                        try
                        {
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
                        catch (Exception ex1)
                        {
                            log.Warn("An error occurred while export content. exception:{0}", ex1.ToString());
                            for (int i = 0; i < buffer.Length; i++)
                            {
                                buffer[i] = 0;
                            }
                            while (size - mBaseItemInfo.DocumentSize > 0)
                            {
                                int length = (size - (int)mBaseItemInfo.DocumentSize > buffer.Length) ? buffer.Length : (int)(size - (int)mBaseItemInfo.DocumentSize);
                                output.WriteContent(buffer, 0, length);
                                mBaseItemInfo.DocumentSize += length;
                            }
                            throw new AveWrapperException(AveInternalResourceKey.Wrapper_Exception_Backup_GetDocumentContentError);
                        }
                    }
                    else
                    {
                        //output.FlushMetadata(0);
                        log.Debug("The {0} with GUID {1} is not found.", mBaseItemInfo.ItemType, mBaseItemInfo.GUID);
                        throw new AveFileNotFoundException(AveInternalResourceKey.Wrapper_Exception_Server_FileNotFoundException);
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
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = 0;
                    }
                    while (size - mBaseItemInfo.DocumentSize > 0)
                    {
                        long length = (size - mBaseItemInfo.DocumentSize > buffer.Length) ? buffer.Length : (size - mBaseItemInfo.DocumentSize);
                        output.WriteContent(buffer, 0, (int)length);
                        mBaseItemInfo.DocumentSize += length;
                    }
                }
            }
        }


        /// <summary>
        /// Get item version's field value in default view.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetItemViewFields()
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
        }

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

        internal void ExportUnavailableUserInCache(IAveBackupStream output)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPItem.ExportUnavailableUserInCache"))
            {
                var list = GetUnavailableUserInCache();
                if (list.Users.Count > 0)
                {
                    output.WriteMetadata(AveMetadataType.UserCache, list);
                }
            }
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

        internal void ExportUserCache(IAveBackupStream output)
        {
            var users = GetUserCache();
            if (users.Users.Count > 0)
            {
                output.WriteMetadata(AveMetadataType.UserCache, users);
            }
        }

        private AveUserList GetUserCache()
        {
            return DataCache.GetUsersForExport();
        }

        internal void ExportGroupCache(IAveBackupStream output)
        {
            var groups = GetGroupCache();
            if (groups.Groups.Count > 0)
            {
                output.WriteMetadata(AveMetadataType.GroupCache, groups);
            }
        }

        public AveGroupList GetGroupCache()
        {
            var groups = DataCache.GetGroupsForExport();
            return groups;
        }

        internal void ExportStorageInfo(IAveBackupStream output)
        {
            var storageInfo = GetAllStorageInfo();

            if (storageInfo.Item1 != null)
            {
                output.WriteMetadata(AveMetadataType.DocStorageInfo.ToString(), storageInfo.Item1);
            }
            else if (storageInfo.Item2 != null)
            {
                output.WriteMetadata(AveMetadataType.DocStorageInfo.ToString(), storageInfo.Item2);
            }
        }

        internal Tuple<AveStorageInfo, AveStorageInfo13> GetAllStorageInfo()
        {
            if (this.mAveParentSite.SPContextKind.IsServerMode13Upper())
            {
                return new Tuple<AveStorageInfo, AveStorageInfo13>(null, StorageInfo13);
            }

            return new Tuple<AveStorageInfo, AveStorageInfo13>(StorageInfo, null);
        }

        internal void ExportDataToExcel(string path)
        {
            try
            {
                //ADO-84971 only export current version data to excel.
                //ADO-127081 使用不同的方式去判断当前version是否是current version
                //local备份时使用mBaseItemInfo.IsVersion判断当前version是否是历史version
                //O365备份时，使用mBaseItemInfo.IsCurrentVersion判断当前version是否是current version
                List<AveTermStoreInfo> termStoreInfos = null;
                if (!IsVersion && mBaseItemInfo.IsCurrentVersion && !IsSystemFileOrFolder)
                {
                    if (UserDataCache.ContainsKey("#tp_ID"))
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
                                    if (fieldType.Equals("LookupMulti", StringComparison.OrdinalIgnoreCase) ||
                                        fieldType.Equals("UserMulti", StringComparison.OrdinalIgnoreCase))
                                    {
                                        userData[fieldName] = SPListItem[fieldName];
                                        continue;
                                    }

                                    #region ADO-36920 ​Excel中metadata类型column值和sharepoint显示不一致

                                    else if (fieldType.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase)
                                        || fieldType.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (termStoreInfos == null)
                                        {
                                            termStoreInfos = GetMetadataInfo(new AveBackupOption() { BackupRelatedTermsOnly = true });
                                        }
                                        var fieldInfo = this.mAveSPList.TaxonomyFields[fieldName];
                                        object tempMetadataValue = SPListItem[fieldName];
                                        if (tempMetadataValue is IAveTaxonomyFieldValue)
                                        {
                                            userData[fieldName] = GetTermRelatedFullPath(termStoreInfos, fieldInfo, (tempMetadataValue as IAveTaxonomyFieldValue));
                                        }
                                        else if (tempMetadataValue is IAveTaxonomyFieldValueCollection)
                                        {
                                            StringBuilder builder = new StringBuilder();
                                            foreach (IAveTaxonomyFieldValue value in tempMetadataValue as IAveTaxonomyFieldValueCollection)
                                            {
                                                if (value == null)
                                                {
                                                    continue;
                                                }
                                                else
                                                {
                                                    builder.Append(';');
                                                }
                                                var ValuePath = GetTermRelatedFullPath(termStoreInfos, fieldInfo, value);
                                                builder.Append(ValuePath);
                                            }
                                            userData[fieldName] = builder.ToString().Trim(';');
                                        }
                                        else if (SPListItem[fieldName] != null)
                                        {
                                            userData[fieldName] = tempMetadataValue.ToString();
                                        }
                                    }

                                    #endregion ADO-36920 ​Excel中metadata类型column值和sharepoint显示不一致

                                    else
                                    {
                                        userData[fieldName] = SPListItem[fieldName] != null ? SPListItem[fieldName].ToString() : "";
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
                        if (mAveSPList.ExportExcelDatas.ContainsKey(mBaseItemInfo.RowId))
                        {
                            ExportExcelData newExportExcelData = GetExportExcelData(path, userData, UserDataCache);
                            ExportExcelData oldExportExcelData = mAveSPList.ExportExcelDatas[mBaseItemInfo.RowId];
                            if (newExportExcelData.Version > oldExportExcelData.Version)
                            {
                                mAveSPList.ExportExcelDatas[mBaseItemInfo.RowId] = newExportExcelData;
                            }
                        }
                        else
                        {
                            ExportExcelData exportExcelData = GetExportExcelData(path, userData, UserDataCache);
                            mAveSPList.ExportExcelDatas.Add(mBaseItemInfo.RowId, exportExcelData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("ExportDataToExcel Error.Exception:" + ex.ToString());
            }
        }

        private ExportExcelData GetExportExcelData(string path, Dictionary<string, object> userData, Dictionary<string, object> userDataCache)
        {
            ExportExcelData exportExcelData = new ExportExcelData()
            {
                Path = path,
                UserData = userData,
                Version = 0
            };
            try
            {
                if (userDataCache != null && userDataCache.ContainsKey("#tp_UIVersion"))
                {
                    int version = (int)userDataCache["#tp_UIVersion"];
                    if (version > 0)
                    {
                        exportExcelData.Version = version;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("Failed to get the version info, exception: {0}", ex);
            }
            return exportExcelData;
        }

        private string GetTermRelatedFullPath(List<AveTermStoreInfo> termStoreInfos, AveTaxFieldInfo fieldInfo, IAveTaxonomyFieldValue aveTaxonomyFieldValue)
        {
            var termInfo = termStoreInfos.Find(s => s.Id == fieldInfo.SspId);
            if (termInfo != null)
            {
                var groupInfo = termInfo.Groups.Find(g => g.Id == fieldInfo.GroupId);
                if (groupInfo != null)
                {
                    var termSetInfo = groupInfo.TermSets.Find(ts => ts.Id == fieldInfo.TermSetId);
                    if (termSetInfo != null)
                    {
                        var builder = new StringBuilder();
                        GetTermPathFromTermSet(builder, termSetInfo.Terms, new Guid(aveTaxonomyFieldValue.TermGuid));
                        var result = string.IsNullOrEmpty(builder.ToString()) ? aveTaxonomyFieldValue.Label : builder.ToString(); // 可能在column 赋值后，将term 删除。
                        return result.Trim('<');
                    }
                }
            }
            return aveTaxonomyFieldValue.Label;
        }

        /// <summary>
        /// 获取从第一层term 开始的path
        /// </summary>
        /// <param name="builder">存储path</param>
        /// <param name="terms">下层terms</param>
        /// <param name="sourceTermId">column 值中的termId</param>
        /// <returns>是否找到当前term</returns>
        private bool GetTermPathFromTermSet(StringBuilder builder, List<AveTermInfo> terms, Guid sourceTermId)
        {
            foreach (var term in terms)
            {
                if (term.Id == sourceTermId)
                {
                    builder.Insert(0, "<" + term.Name);
                    return true;
                }
                else
                {
                    if (GetTermPathFromTermSet(builder, term.Terms, sourceTermId))
                    {
                        builder.Insert(0, "<" + term.Name);
                        return true;
                    }
                }
            }
            return false;
        }

        #region IAveSPItem Members

        IAveSPList IAveSPItem.AveSPList
        {
            get { return mAveSPList; }
        }

        public IAveSPSite AveSPSite
        {
            get { return mAveParentSite; }
        }

        public bool HasUniqueRoleAssignments
        {
            get
            {
                if (AveSPItem.RestoringItemCurrentVersionDocData != null && AveSPItem.RestoringItemCurrentVersionDocData.ContainsKey("HasUniqueRoleAssignments"))
                {
                    return Convert.ToBoolean(AveSPItem.RestoringItemCurrentVersionDocData["HasUniqueRoleAssignments"]);
                }
                return false;
            }
        }

        public bool IsSystemFileOrFolder
        {
            get
            {
                return this.mBaseItemInfo.RowId <= 0;
            }
        }

        public bool IsVersion
        {
            get { return BaseItemInfo.IsVersion; }
            internal set { BaseItemInfo.IsVersion = value; }
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

        public Guid Id
        {
            get { return BaseItemInfo.GUID; }
            private set { BaseItemInfo.GUID = value; }
        }

        public int RowId
        {
            get { return mBaseItemInfo.RowId; }
            private set { mBaseItemInfo.RowId = value; }
        }

        public Guid ParentId
        {
            get
            {
                return mBaseItemInfo.ParentId;
            }
        }

        public long DocumentSize
        {
            get { return mBaseItemInfo.DocumentSize; }
        }

        public int Version
        {
            get { return mBaseItemInfo.Version; }
            private set { mBaseItemInfo.Version = value; }
        }

        public Guid ScopeId
        {
            get { return mBaseItemInfo.ScopeId; }
            internal set { mBaseItemInfo.ScopeId = value; }
        }

        public string Name
        {
            get { return mBaseItemInfo.Name; }
            set { mBaseItemInfo.Name = value; }
        }

        public string Title
        {
            get
            {
                if (UserDataCache.ContainsKey("Title"))
                {
                    return UserDataCache["Title"] as string;
                }
                return Name;
            }
        }

        public string ServerRelativeUrl
        {
            get { return mBaseItemInfo.ServerRelativeUrl; }
            set { mBaseItemInfo.ServerRelativeUrl = value; }
        }

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
                if (mItemType == AveItemType.Document && !this.IsSystemFileOrFolder && (userData.Item1 == null || userData.Item1.Count == 0))
                {
                    log.Info("Skip backing up current document as it's user data is invalid.");
                    throw new AveWrapperSkipException(AveInternalResourceKey.Wrapper_Exception_Backup_SkipBackupDocumentWithInvalidUserData);
                }
                if (includeUserAndGroup)
                {
                    this.CacheUserForUserInfomationList();
                    if (onlyUnAvaiableUser)
                    {
                        this.ExportUnavailableUserInCache(output);
                    }
                    else
                    {
                        this.ExportUserCache(output);
                        this.CachePrincipalOfTargetAudience();
                        this.ExportGroupCache(output);
                    }
                }

                if (userData.Item2 != null)
                {
                    output.WriteMetadata(AveMetadataType.MetadataService, userData.Item2);
                }

                if (userData.Item1 != null)
                {
                    output.WriteMetadata(AveMetadataType.DocData, userData.Item1);
                }
            }
        }

        internal Tuple<Dictionary<string, object>, List<AveTermStoreInfo>> GetUserDataInfoWithDependence(
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
        internal Tuple<Dictionary<string, object>, List<AveTermStoreInfo>> GetUserDataInfoWithDependence(AveBackupOption backupColumnOption)
        {
            Dictionary<string, object> userData = this.UserDataCache;
            List<AveTermStoreInfo> relatedInfos = null;
            if (userData.Count > 0)
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
                if (mAveSPList.BackupLookUpDisplayValue || mAveSPList.BackupItemTPGUIDofLookupValue || mAveSPList.BackupItemLookupDisplayValueForRestore || mAveSPList.BackupItemLeafNameOfLookupValue)
                {
                    foreach (KeyValuePair<string, AveLookupFieldInfo> kv in mAveSPList.LookupFields)
                    {
                        string lookupValue = String.Empty;
                        string itemId = String.Empty;
                        try
                        {

                            if (userData.ContainsKey(kv.Key))
                            {
                                lookupValue = userData[kv.Key].ToString();
                                itemId = lookupValue.Contains(";") ? lookupValue.Substring(0, lookupValue.IndexOf(";", StringComparison.OrdinalIgnoreCase)) : lookupValue;
                                if ((mAveSPList.BackupLookUpDisplayValue || mAveSPList.BackupItemLookupDisplayValueForRestore) && !lookupValue.Contains(";"))
                                {
                                    string displayValue = mAveSPList.GetLookupDisplayValuebyItemId(itemId, kv.Value);
                                    if (!string.IsNullOrEmpty(displayValue))
                                    {
                                        StringBuilder builder = new StringBuilder();
                                        builder.Append(itemId);
                                        builder.Append(";");
                                        builder.Append(displayValue);
                                        userData[kv.Key] = builder.ToString();
                                        //userData[kv.Key] = itemId + ";" + displayValue;
                                    }
                                }
                                if (mAveSPList.BackupItemTPGUIDofLookupValue)
                                {
                                    int rowId = Convert.ToInt32(itemId);
                                    Guid tp_GUID = GetLookupGUIDById(kv.Value.LookupWeb, kv.Value.LookupList, rowId);
                                    if (tp_GUID != Guid.Empty)
                                    {
                                        StringBuilder builder = new StringBuilder();
                                        builder.Append(userData[kv.Key].ToString());
                                        //builder.Append('#');
                                        builder.Append("#GUID#");
                                        builder.Append(tp_GUID);
                                        userData[kv.Key] = builder.ToString();
                                        //lookupFieldGuidValue[name] = rowId.ToString() + "#" + tp_GUID.ToString();
                                    }
                                }
                                if (mAveSPList.BackupItemLeafNameOfLookupValue)
                                {
                                    string leafName = mAveSPList.GetLookupItemLeafNameByItemId(itemId, kv.Value);
                                    if (!String.IsNullOrEmpty(leafName))
                                    {
                                        StringBuilder builder = new StringBuilder();
                                        builder.Append(userData[kv.Key].ToString());
                                        //builder.Append('&');
                                        builder.Append("&leafName&");
                                        builder.Append(leafName);
                                        userData[kv.Key] = builder.ToString();
                                    }
                                }
                                if (mAveSPList.BackupItemLookupDisplayValueForRestore)
                                {

                                    StringBuilder builder = new StringBuilder();
                                    builder.Append(userData[kv.Key].ToString());
                                    builder.Append('*');
                                    userData[kv.Key] = builder.ToString();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.Warn("Get single lookup display value exception.ItemId{0}, LookupValue{1},Error:{2}", itemId, lookupValue, e);
                        }
                    }
                }
                //output.WriteMetadata(AveMetadataType.DocData, userData);
            }

            return new Tuple<Dictionary<string, object>, List<AveTermStoreInfo>>(userData, relatedInfos);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "DataJunctionInfo is a part of common method name")]
        public void ExportDataJunctionInfo(IAveBackupStream output, bool includeUserAndGroup = true, bool onlyUnAvaiableUser = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.ExportDataJunctionInfo"))
            {
                var dataCache = GetUserDataJunctionCache(includeUserAndGroup);

                if (includeUserAndGroup)
                {
                    //this.CachePrincipalFromDatajunction();
                    if (onlyUnAvaiableUser)
                    {
                        this.ExportUnavailableUserInCache(output);
                    }
                    else
                    {
                        this.ExportUserCache(output);
                        this.ExportGroupCache(output);
                    }
                }

                if (dataCache != null)
                {
                    output.WriteMetadata(AveMetadataType.DocDataJunction, dataCache);
                }
            }
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
                if (mAveSPList.BackupLookUpDisplayValue || mAveSPList.BackupItemTPGUIDofLookupValue || mAveSPList.BackupItemLookupDisplayValueForRestore || mAveSPList.BackupItemLeafNameOfLookupValue)
                {
                    try
                    {
                        foreach (Dictionary<string, object> data in dataCache)
                        {
                            Guid fieldId = new Guid(data["tp_FieldId"].ToString());
                            int itemId = Convert.ToInt32(data["tp_Id"]);
                            AveLookupFieldInfo fieldInfo = null;
                            foreach (AveLookupFieldInfo info in mAveSPList.LookupFields.Values)
                            {
                                if (info.Id == fieldId)
                                {
                                    fieldInfo = info;
                                    break;
                                }
                            }
                            if (fieldInfo != null)
                            {
                                if (mAveSPList.BackupLookUpDisplayValue || mAveSPList.BackupItemLookupDisplayValueForRestore)
                                {
                                    if (!data.ContainsKey("DisplayValue"))
                                    {
                                        string displayValue = String.Empty;
                                        if (String.IsNullOrEmpty(fieldInfo.LookupColumnRowNameForQuery) && !String.IsNullOrEmpty(fieldInfo.LookupColumnDisplayName))
                                        {
                                            displayValue = this.AveSPList.SPList.GetItemById(this.RowId).FieldValues[fieldInfo.LookupColumnDisplayName].ToString();
                                            if (!string.IsNullOrEmpty(displayValue))
                                            {
                                                if (displayValue.Contains(";#"))
                                                {
                                                    string[] strValues = displayValue.Split(new string[] { ";#" }, StringSplitOptions.None);
                                                    //此处的数据是多值并用 ‘;#’ 进行分割的，如 ‘1;#s1;#2;#1’，奇数位为item row id，偶数位为值(String类型)。由于偶数位可能出现于row id相同的值。如果直接foreach寻找可能出现位置错乱，故使用for循环隔两位查找。
                                                    //此处处理可以选出item id对应的value。
                                                    for (int i = 0; i < strValues.Length; i = i + 2)
                                                    {
                                                        if (Convert.ToInt32(strValues[i]) == itemId)
                                                        {
                                                            data["DisplayValue"] = strValues[i + 1];
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            displayValue = mAveSPList.GetLookupDisplayValuebyItemId(itemId.ToString(), fieldInfo);
                                            if (!String.IsNullOrEmpty(displayValue))
                                            {
                                                data["DisplayValue"] = displayValue;
                                            }
                                        }
                                    }
                                }
                                if (mAveSPList.BackupItemTPGUIDofLookupValue)
                                {
                                    if (!data.ContainsKey("tp_Guid"))
                                    {
                                        Guid itemGuid = GetLookupGUIDById(fieldInfo.LookupWeb, fieldInfo.LookupList, itemId);
                                        if (itemGuid != Guid.Empty)
                                        {
                                            data["tp_Guid"] = itemGuid;
                                        }
                                    }
                                }
                                if (mAveSPList.BackupItemLeafNameOfLookupValue)
                                {
                                    if (!data.ContainsKey("itemLeafName"))
                                    {
                                        string itemLeafName = mAveSPList.GetLookupItemLeafNameByItemId(itemId.ToString(), fieldInfo);
                                        if (!String.IsNullOrEmpty(itemLeafName))
                                        {
                                            data["itemLeafName"] = itemLeafName;
                                        }
                                    }
                                }
                                if (mAveSPList.BackupItemLookupDisplayValueForRestore)
                                {
                                    if (fieldInfo != null && !String.IsNullOrEmpty(fieldInfo.LookupColumnDisplayName))
                                    {
                                        data["NeedRestoreItemLookupColumnNameAndValue"] = fieldInfo.LookupColumnDisplayName;
                                    }

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Warn("Get multi lookup display value exception.Error:{0}", ex.ToString());
                    }
                }
                //output.WriteMetadata(AveMetadataType.DocDataJunction, dataCache);
            }

            return dataCache;
        }

        public void ExportLookupFieldGuidValue(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.ExportLookupFieldGuidValue"))
            {
                Dictionary<string, string> lookupFieldGuidValue = this.GetLookupFieldGuidValue();
                if (lookupFieldGuidValue != null && lookupFieldGuidValue.Count > 0)
                {
                    output.WriteMetadata(AveMetadataType.LookupFieldGuidValue.ToString(), lookupFieldGuidValue);
                }
            }
        }

        public void ExportVersions(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.ExportVersions"))
            {
                List<int> docVersions = this.GetDocVersions();
                if (docVersions != null && docVersions.Count > 0)
                {
                    output.WriteMetadata(AveMetadataType.DocVersions.ToString(), docVersions);
                }
            }
        }
        public List<int> GetItemVersions()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPItem.GetItemVersions"))
            {
                if (RowId > 0)
                {
                    return GetDocVersions();
                }
                return new List<int>();
            }
        }

        /// <summary>只有Raplicator需要，别的模块User不需要单独控制，会跟Permission走</summary>
        public void ExportUsers(IAveBackupStream output)
        {
            this.CachePrincipalFromPermission();
            this.ExportUserCache(output);
        }

        /// <summary>只有Raplicator需要，别的模块Group不需要单独控制，会跟Permission走</summary>
        public void ExportGroups(IAveBackupStream output)
        {
            this.CachePrincipalFromPermission();
            this.ExportGroupCache(output);
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

        //public void ExportWorkflowInstance(IAveBackupStream output, bool forceBackup = false, string connectionString = null)
        //{
        //    if (this.ParentSite.SPContextKind.IsServerMode())
        //    {
        //        AveWorkflow workflow = new AveWorkflow();
        //        if (!string.IsNullOrEmpty(connectionString))
        //        {
        //            workflow.SetNWDBConnectionString(connectionString);
        //        }
        //        workflow.ForceBackupInstance = forceBackup;
        //        workflow.ExportWorkflowInstance(output, this);
        //        workflow.ExportWorkflowSchedule(output, this);
        //    }
        //}

        public void ExportWorkflowInstance(IAveBackupStream output, bool forceBackup = false, string contentDBconnectionString = null, string configDBconnectionString = null)
        {
            if (this.ParentSite.SPContextKind.IsServerMode())
            {
                var workflow = new AveWorkflow() { ForceBackupAssoiciation = true };
                if (!string.IsNullOrEmpty(contentDBconnectionString))
                {
                    workflow.SetNWDBConnectionString(contentDBconnectionString);
                }
                if (!string.IsNullOrEmpty(configDBconnectionString))
                {
                    workflow.SetNWConfigDBConnectionString(configDBconnectionString);
                }
                workflow.ForceBackupInstance = forceBackup;
                workflow.ExportWorkflowInstance(output, this);
                workflow.ExportWorkflowSchedule(output, this);
            }
        }

        /// <summary>
        /// only for DPM, if other module need it contact me.
        /// </summary>
        /// <param name="output"></param>
        /// <param name="forceBackup"></param>
        /// <param name="contentDBconnectionString"></param>
        /// <param name="configDBconnectionString"></param>
        public void ExportWorkflowSchedule(IAveBackupStream output, bool forceBackup = false, string contentDBconnectionString = null, string configDBconnectionString = null)
        {
            if (this.ParentSite.SPContextKind.IsServerMode())
            {
                var workflow = new AveWorkflow() { ForceBackupAssoiciation = forceBackup };
                if (!string.IsNullOrEmpty(contentDBconnectionString))
                {
                    workflow.SetNWDBConnectionString(contentDBconnectionString);
                }
                if (!string.IsNullOrEmpty(configDBconnectionString))
                {
                    workflow.SetNWConfigDBConnectionString(configDBconnectionString);
                }
                workflow.ExportWorkflowSchedule(output, this);
            }
        }

        public void ExportFullTextIndex(IAveBackupStream output, Dictionary<string, object> customFieldValues, FullTextIndexLevel level)
        {
            if (RowId == 0 || mAveSPList == null || mAveSPList.SPList == null)
            {
                return;
            }
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPItem.ExportFullTextIndex"))
            {
                var index = mAveSPList.AveIndexCache.GetIndex(this, level);
                if (customFieldValues != null)
                {
                    index.SetCustomColumnValues(customFieldValues);
                }
                output.WriteMetadata(AveMetadataType.FullTextIndex, index);
            }
        }

        public Stream GetContent()
        {
            return GetContent(Version);
        }

        public Dictionary<string, string> GetMetaInfo()
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

        public Dictionary<string, object> GetColumnValues(ColumnsLevel level = ColumnsLevel.AllColumns, bool forceGetByAPI = true)
        {
            if (RowId == 0 || mAveSPList == null || mAveSPList.SPList == null)
            {
                return null;
            }
            return mAveSPList.AveIndexCache.GetColumnValues(this, level, forceGetByAPI);
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
        #endregion

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

        internal void ExportRoleAssignments(IAveBackupStream stream, SPRoleAssignmentsBakupOption option)
        {
            if (!IsVersion)
            {
                SPRoleAssignmentsDto roleAssignmentsDto = null;

                if (HasUniqueRoleAssignments || option.IncludeInheritedRoleAssignments)
                {
                    using (var roleAssignments = AveRoleAssignments.CreateInstance(this))
                    {
                        roleAssignmentsDto = roleAssignments.GetRoleAssignmentsDto(option.IncludeUsers, option.IncludeGroups);
                    }
                }
                else
                {
                    roleAssignmentsDto = new SPRoleAssignmentsDto();
                    roleAssignmentsDto.IsInherit = true;
                }

                stream.WriteMetadata(AveMetadataType.RoleAssignmentsDto, roleAssignmentsDto);
            }
        }

        internal void ExportRoleAssignments(IAveBackupStream stream, bool includeUsers, bool includeGroups)
        {
            ExportRoleAssignments(stream, new SPRoleAssignmentsBakupOption()
            {
                IncludeUsers = includeUsers,
                IncludeGroups = includeGroups,
                IncludeInheritedRoleAssignments = false,
            });
        }

        public void ExportSocialInfos(IAveBackupStream stream, string url)
        {
            if (this.ParentSite.ObjectModelFactory.ContextKind.IsServerMode10Upper())
            {
                var socialDto = new SPSocialDto();

                socialDto.Comments = new AveSPSocialComment(url, this.ParentSite).GetSocialComments();
                socialDto.Tags = new AveSPSocialTag(url, this.ParentSite).GetSocialTags();

                if ((socialDto.Comments != null && socialDto.Comments.Count > 0) ||
                    (socialDto.Tags != null && socialDto.Tags.Count > 0))
                {
                    stream.WriteMetadata(AveMetadataType.SocialDto, socialDto);
                }
            }
        }

        public void ExportLinksInfos(IAveBackupStream output)
        {
            var linksInfo = new AveFileLinksInfo();
            if (this.SPListItem != null)
            {
                linksInfo.BackwardLinks = this.SPListItem.BackwardLinks.Select(ConvertLinksToDictionary).ToList();
                linksInfo.ForwardLinks = this.SPListItem.ForwardLinks.Select(ConvertLinksToDictionary).ToList();
            }
            output.WriteMetadata(AveMetadataType.FileLink, linksInfo);
        }

        private static AveFileLinkInfo ConvertLinksToDictionary(IAveLink link)
        {
            return new AveFileLinkInfo { IsBroken = link.IsBroken, IsInternal = link.IsInternal, IsToFolder = link.IsToFolder, Url = link.Url, ServerRelativeUrl = link.ServerRelativeUrl, UrlParameter = link.UrlParameter, WebId = link.WebId };
        }

        public void ExportComplianceTag(IAveBackupStream output)
        {
            if (SPListItem != null)
            {
                if (SPListItem.ParentList.ParentWeb.Site.IsOnlineSite && mBaseItemInfo.IsCurrentVersion)
                {
                    var complianceTagInfo = this.SPListItem.ComplianceTagInfo;
                    if (complianceTagInfo != null)
                    {
                        output.WriteMetadata(AveMetadataType.ComplianceTag, new AveItemComplianceTagInfo()
                        {
                            ComplianceTag = complianceTagInfo.ComplianceTag,
                            TagPolicyHold = complianceTagInfo.TagPolicyHold,
                            TagPolicyRecord = complianceTagInfo.TagPolicyRecord,
                            EventBasedTag = complianceTagInfo.TagPolicyEventBased,
                            ComplianceWrittenDate = complianceTagInfo.ComplianceWrittenDate,
                            ComplianceSettingFlag = complianceTagInfo.ComplianceSettingFlags,
                            ComplianceTagUserId = complianceTagInfo.ComplianceTagUserId,
                        });
                    }
                }
            }
        }
    }

    #region moved to wrapper common
    //public class FullTextIndex
    //{
    //    public string VersionComment { get; set; }

    //    public string CreatedByDisplayName { get; set; }

    //    public string CreatedByLoginName { get; set; }

    //    public string ModifiedByDisplayName { get; set; }

    //    public string ModifiedByLoginName { get; set; }

    //    public DateTime Created { get; set; }

    //    public DateTime Modified { get; set; }

    //    public string TimeZoneInfoID { get; set; }

    //    #region Only for Archiver

    //    public string ArchiveBy { get; set; }

    //    public DateTime ArchiveTime { get; set; }

    //    #endregion Only for Archiver

    //    public int Size { get; set; }

    //    public string ContentTypeName { get; set; }

    //    public List<string> Attachments { get; set; }

    //    public Dictionary<string, object> ColumnValues { get; set; }

    //    public void SetCustomColumnValues(Dictionary<string, object> customColumnValues)
    //    {
    //        if (customColumnValues == null)
    //        {
    //            throw new ArgumentNullException("customColumnValues");
    //        }
    //        if (this.ColumnValues == null)
    //        {
    //            this.ColumnValues = new Dictionary<string, object>(customColumnValues.Count);
    //        }
    //        customColumnValues.ToList().ForEach(
    //                        pair =>
    //                        {
    //                            switch (pair.Key)
    //                            {
    //                                //ArchiveBy,ArchiveTime特殊处理一下
    //                                case AveWrapperConstants.ARCHIVE_BY:
    //                                    this.ArchiveBy = pair.Value.ToString();
    //                                    break;
    //                                case AveWrapperConstants.ARCHIVE_TIME:
    //                                    this.ArchiveTime = (DateTime)pair.Value;
    //                                    break;
    //                                default:
    //                                    this.ColumnValues[pair.Key] = pair.Value;
    //                                    break;
    //                            }
    //                        });//Overwrite field
    //    }
    //}

    //public enum FullTextIndexLevel
    //{
    //    Invalid = -1,
    //    BaseInfo = 0,
    //    IncludeDefaultViewColumns = 1,
    //    IncludeAllVisiableColumns = 2,
    //    IncludeAllColumns = 3,
    //    IncludeAllColumnsAndSystemColumns = 4,
    //}

    ///// <summary>
    ///// AllVisiableColumns 是非隐藏的所有column，获取是column的Displayname;
    ///// AllColumns 指所有的column，包括隐藏和系统column，获取的key 是column的InternalName.
    ///// </summary>
    //public enum ColumnsLevel
    //{
    //    None = 0,
    //    AllVisiableColumns = 1,
    //    AllColumns = 2,
    //}
    #endregion
}