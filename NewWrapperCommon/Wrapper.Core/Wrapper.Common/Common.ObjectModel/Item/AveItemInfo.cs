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
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Restore;


namespace AvePoint.Wrapper.Common
{
    public class AveBaseItemInfo
    {
        public byte[] UnVersionedMetaInfo { get; set; }
        public int UnVersionedMetaInfoVersion { get; set; }
        public string CopySource { get; set; }
        public bool HasCopyDestinations { get; set; }
        public int Version { get; set; }
        public long Length { get; set; }
        public int Level { get; set; }
        public int RowId { get; set; }
        public Guid GUID { get; set; }
        public string Name { get; set; }
        public bool IsVersion { get; set; }
        public Guid ScopeId { get; set; }
        public string ScopeUrl { get; set; }
        public int DocFlag { get; set; }
        public bool HasStream { get; set; }
        public Guid SiteId { get; set; }
        public Guid WebId { get; set; }
        public int? InternalVersion { get; set; }
        public Guid ListId { get; set; }
        public string ServerRelativeUrl { get; set; }
        public Guid ParentId { get; set; }
        public long DocumentSize { get; set; }
        public AveItemType ItemType { get; set; }
        public bool PageVersion { get; set; }
        public int DraftOwnerId { get; set; }
        public IAveItem AveItem { get; set; }
        public int ModerationStatus { get; set; }
        public string ModerationComments { get; set; }
        public string ParentWebRelativeUrl { get; set; }
        public string ParentListTitle { get; set; }
        public string ParentFolderRelativeUrl { get; set; }
        public bool IsCheckOut { get; set; }
        public bool IsCurrentVersion { get; set; }
        //true if the file is new added, not include version.
        public bool IsNewCreated { get; set; }

        public bool IsStubData { get; set; }

        public Guid CheckOutFileUniqueID { get; set; }

        public int CheckoutUserId { get; set; }

        public byte OriginalLevel { get; set; }

        public int OriginalVersion { get; set; }

        public int OriginalRowId { get; set; }

        public DateTime DTimeCreated { get; set; }

        public DateTime DTimeLastModified { get; set; }

        public bool HasPreCurrentVersion { get; set; }

        public RestoringDto RestoringItem { get; set; }

        public AveItemFieldsInfo FieldsInfo = new AveItemFieldsInfo();

        public AveSettingInfo SettingInfo = new AveSettingInfo();

        public int RestoreVersion { get; set; }

        public AveRestoreMode RestoreOption { set; get; }

        public bool NeedUpdateStatusByNative { get; set; }

        public AveMappingManager MappingManager { get; set; }

        public Dictionary<string, string> MetaInfoDic { get; set; }

        public List<string> NeedSetNullFields { get; set; }

        public bool KeepDefaultValue { get; set; }

        public bool VerifyItemMMSColumnValue { get; set; }

        public AveSiteInfo SourceSiteInfo { get; set; }

        public string ParentSiteServerRelativeUrl { get; set; }

        public Dictionary<string, object> DocData { get; set; }

        public Dictionary<string, object> UserData { get; set; }

        public bool HasUniqueRoleAssignments { set; get; }

        private bool isForceAddTerm = false;
        public bool IsForceAddTerm
        {
            get { return isForceAddTerm; }
            set { isForceAddTerm = value; }
        }
        //For replicator real time
        private int mMaxVersionDiff = 0;
        public int MaxVersionDiff
        {
            get { return mMaxVersionDiff; }
            set { mMaxVersionDiff = value; }
        }

        public bool KeepDestItemRowId = false;
        public int DestItemRowId = 0;
        public Guid DestItemUniqueId = Guid.Empty;

        /// <summary>
        /// Wrapper 重构使用，外围不需要赋值
        /// </summary>
        internal bool NewCodeRestore { get; set; }

        private bool enableEventReceiver = false;
        public bool EnableEventReceiver
        {
            get { return enableEventReceiver; }
            set { enableEventReceiver = value; }
        }

        public bool IsInOneDriveLibrary { set; get; }

        public Func<string, string> GetUserFromMapping
        {
            get; set;
        }

    }
    /// <summary>
    /// Add For update field value
    /// </summary>
    public class AveItemFieldsInfo
    {
        //Add By Matthew
        public Dictionary<string, object> Fields { get; set; }  //现在所有的FieldsValue Info都存在了这个字典里，到后台处理时会根据Version不同进行区别处理。

        public Dictionary<string, object> MultiLookupFields { get; set; } //value值存储在AllUserDataJunctions中的FieldsValue Info存在这个字典中。

        public List<Dictionary<string, object>> NeedPostRestoreMultiLookupFields { get; set; }

        public Dictionary<string, object> FieldsInMetaInfo { get; set; }

        public Dictionary<Guid, Guid> TermIdMapping { get; set; }

        public Dictionary<Guid, List<Guid>> MergedTermIdMapping { get; set; } //ADO-148478：每个term都有MergedTermIds属性，但它是只读的，没法还到目的端。所以在该还原term时在该mapping中做记录，FindTermById时使用。

        public Dictionary<string, string> TaxonomyFieldsInMapping { get; set; }
        
        public string NintexFormDataForPostAction { get; set; }
    }

    /// <summary>
    /// Add For CM, Version: 6.6
    /// </summary>
    public class AveFileLinksInfo
    {
        public List<AveFileLinkInfo> BackwardLinks { set; get; }
        public List<AveFileLinkInfo> ForwardLinks { set; get; }
    }

    /// <summary>
    /// Add For CM, Version: 6.6
    /// </summary>
    public class AveFileLinkInfo
    {
        public string ServerRelativeUrl { set; get; }
        public string Url { set; get; }
        public string UrlParameter { set; get; }
        public bool IsBroken { set; get; }
        public bool IsInternal { set; get; }
        public bool IsToFolder { set; get; }
        public Guid WebId { set; get; }
    }

    //For Some Special List Template
    public class AveListItemInfoExtension
    {
        public int PrincipalId { get; set; } //For MeetingSeries和Events list

        public string FieldUrlValue { get; set; }   //For MeetingSeries和Events list

        public string DestUrl { get; set; }      //For MeetingSeries和Events list
    }

    public class AveListItemInfo : AveBaseItemInfo
    {
        public Guid tp_Guid { get; set; }

        private bool needChangeItemId = true;
        public bool NeedChangeItemId
        {
            get { return needChangeItemId; }
            set { needChangeItemId = value; }
        }

        private bool listContainsTodayFomula = false;
        public bool ListContainsTodayFomula
        {
            get { return listContainsTodayFomula; }
            set { listContainsTodayFomula = value; }
        }

        public AveListItemInfoExtension Extension = new AveListItemInfoExtension();

    }


    public class AveSettingInfo
    {
        public bool DELETE_ITEM { get; set; }

        public bool KEEP_ITEM_TPGUID { get; set; }

        public bool CheckConflictByUniqueId { get; set; }

        public bool CheckItemByFieldValue { get; set; }

        public string MatchItemFieldDisplayName { get; set; }

        public bool MOVE_ITEM_TO_CONFLICT_FOLDER { get; set; }

        public bool MOVE_SOURCE_TO_CONFLICT_FOLDER { get; set; }

        public bool LIST_SETTING_CHANGED { get; set; }

        public bool DESTSTUB_CONTENT { get; set; }

        public bool OverWriteByModifiedTime { get; set; }

        public bool SKIP_IF_SAME_MODIFIEDTIME { get; set; }

        public bool MIG_STUB_PIC_THUMBNAILS { get; set; }

        public bool IsProcessSolutionStatus { get; set; }

        /// <summary>
        /// Replicator因为在Discover里面能够知道是否冲突，所以不需要检查conflict。
        /// 默认值为false，主要是replicator赋值使用。
        /// </summary>
        public bool NewItemWithOutVerifyConflict { get; set; }
        /// <summary>
        /// Replicator知道目的端的ItemId，直接拿来初始化ItemId即可，不需要重新通过TP_GUID来找一次。
        /// 默认值为false，主要是Replicator赋值使用
        /// </summary>
        public bool IncreaceVerionWithRowId { get; set; }
    }

    public class AveFileInfo : AveBaseItemInfo
    {

    }



    public class AveFolderInfo : AveBaseItemInfo
    {
        /// <summary>
        /// Is new created folder when restore this version
        /// </summary>
        public bool IsNewCreatedFolder { get; set; }
        public string ParentListName { get; set; }
        public bool ParentListIsSystem { get; set; }
        public Guid ListRootFolderId { get; set; }
        public bool IsOverWrite { get; set; }
        public bool IsRestoreConnectorFolderProperties { get; set; }
    }

    public class AveDocumentInfo : AveBaseItemInfo
    {

        public string Url { get; set; }

        public string SourceWebUrl { get; set; }

        public bool HasMoveUp { get; set; }

        public string SetupPath { get; set; }

        public bool IsGhostPage { get; set; }
        public bool IsLinkFile { get; set; }

        public Guid SolutionId { get; set; }

        public string CheckinComment { get; set; }

        public Guid OrignialID { get; set; }

        public bool IsView { get; set; }

        private bool findViewByTitle = true;
        public bool FindViewByTitle
        {
            get { return findViewByTitle; }
            set { findViewByTitle = value; }
        }

        //use IsNewCreated instead.
        //public bool IsNewCreatedView { get; set; }

        private bool needChangeItemId = true;
        public bool NeedChangeItemId
        {
            get { return needChangeItemId; }
            set { needChangeItemId = value; }
        }

        public bool Needskip { get; set; }

        public bool IsOrignialCheckOut { get; set; }

        public bool IsOverWrite { get; set; }

        public AveModerationStatusType ModerationType { get; set; }

        public Guid OldDocId { get; set; }

        public AveViewDocInfo AveView = new AveViewDocInfo();

        public int GhostPageOption { get; set; }

        public bool IsThumbnails { get; set; }

        private bool mVerifyPageLayout = false;
        public bool VerifyPageLayout
        {
            get { return mVerifyPageLayout; }
            set { mVerifyPageLayout = value; }
        }
        
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")] 
        public int ImageWidth
        {
            get
            {
                if (this.MetaInfoDic != null && this.MetaInfoDic.ContainsKey("vti_lastwidth"))
                {
                    return Convert.ToInt32(MetaInfoDic["vti_lastwidth"]);
                }
                return 0;
            }
            set
            {
                ImageWidth = value;
            }
        }
        
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")]
        public int ImageHeight
        {
            get
            {
                if (this.MetaInfoDic != null && this.MetaInfoDic.ContainsKey("vti_lastheight"))
                {
                    return Convert.ToInt32(MetaInfoDic["vti_lastheight"]);
                }
                return 0;
            }
            set
            {
                ImageHeight = value;
            }
        }
        public AveCustomizedPageStatus OriginalPageStatus { get; set; }
        //如果文件被设置成master page，不能被删除，但可以被move。还原文件逻辑中队该文件使用move，需要缓存记录move前web的master page设置。
        public List<AveExtendMasterPageInfo> TempMasterSettings;
        public string TempFileUrl;

        public List<Guid> ActivatedWebFeatureIDs;
        public List<Guid> ActivatedWebSolutionFeatureIDs;

        public AveWebPartCache WebPartCache;
        public List<AveWebPartBaseInfo> WebParts;
        public bool WebPartRestored = false;

        private bool mParentLibraryIsMasterPageGallery = false;
        public bool ParentLibraryIsMasterPageGallery
        {
            get
            {
                return mParentLibraryIsMasterPageGallery;
            }
            set
            {
                mParentLibraryIsMasterPageGallery = value;
            }
        }

        public string XSNStreamHashValue { get; set; } // key:ipfs_streamhash
    }


    public class AveHiddenFileInfo
    {

        public string ID { get; set; }

        public string Name { get; set; }

        public byte Level { get; set; }

        public int DocFlags { get; set; }

        public int Version { get; set; }

        public DateTime TimeLastModified { get; set; }

        public bool HasStream { get; set; }

        public long Size { get; set; }
    }

    public class AveViewDocInfo
    {
        public string ViewUrl { get; set; }
        public List<AveViewInfo> Vinfos = new List<AveViewInfo>();
        //Key:Source Id, Value: Destination Id
        public Dictionary<Guid, Guid> Views = new Dictionary<Guid, Guid>();

        public bool RestoreRssView { get; set; }
    }

    public class AveFieldValueInfo
    {
        private string mColName;
        private object mColValue;
        private int mRowOrdinal;
        private AveFieldType mFieldType;
        private Guid mId;

        public AveFieldType FieldType
        {
            get { return mFieldType; }
            set { mFieldType = value; }
        }

        public string ColName
        {
            get { return mColName; }
            set { mColName = value; }
        }

        public object ColValue
        {
            get { return mColValue; }
            set { mColValue = value; }
        }

        public int RowOrdinal
        {
            get { return mRowOrdinal; }
            set { mRowOrdinal = value; }
        }

        public Guid Id
        {
            get { return mId; }
            set { mId = value; }
        }


        public AveFieldValueInfo(string colName, object colValue, int rowOrdinal)
        {
            this.ColName = colName;
            this.ColValue = colValue;
            this.RowOrdinal = rowOrdinal;
        }

        public AveFieldValueInfo()
        { }

        public override string ToString()
        {
            return mColValue.ToString();
        }

    }

    public class AveAttachmentInfo : AveBaseItemInfo
    {
        public IAveAttachment Attachment { get; set; }
        public string LeafName { get; set; }
        public string SrcUrl { get; set; }
        public string FullName { get; set; }
        public string RealName { get; set; }
        public string Url { get; set; }

        public long Size { get; set; }

        public Collection<string> WebappBlockTypes { get; set; }
    }

    /// <summary>
    /// only item restore use this class, so put it here.
    /// </summary>
    [Serializable]
    public class AveItemVersionNumber
    {
        public int MajorVersion;
        public int MinorVersion;

        public int UIVersion
        {
            get
            {
                return MajorVersion * 512 + MinorVersion;
            }
        }

        public string VersionLabel
        {
            get
            {
                return MajorVersion + "." + MinorVersion;
            }
        }

        public AveItemVersionNumber(int uiversion)
        {
            MajorVersion = uiversion / 512;
            MinorVersion = uiversion % 512;
        }

        public AveItemVersionNumber(string versionLabel)
        {
            MajorVersion = int.Parse(versionLabel.Split('.')[0]);
            MinorVersion = int.Parse(versionLabel.Split('.')[1]);
        }

        public override int GetHashCode()
        {
            return MajorVersion * 512 + MinorVersion;
        }
    }

    public class RestoringDto
    {
        private static bool mRestoreCurrentVersion = true;
        //replicator需要只还某个version，这样不能依赖init里的方法来控制IsNewItem属性 
        private static bool mReplicatorVersionLevelProperty = false;
        [ThreadStatic]
        private static string mName = null;
        [ThreadStatic]
        private static string mNameMapping = null;

        public string NameMapping
        {
            get { return mNameMapping; }
            set { mNameMapping = value; }
        }

        public string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        [ThreadStatic]
        private static int mPublishingUIVersion;
        [ThreadStatic]
        private static int mDraftUIVersion;
        //private int mCurrentUIVersion;
        //private int mConflictDocId;
        //private int mConflictRecycleId;
        [ThreadStatic]
        private static bool mOverWrite;
        private int mConflictRowId;

        private ConflictType mConflictType;
        private RestoreTargetTable mTargetTable;

        private bool overwriteAllVersion;

        public  bool OverwriteAllVersion
        {
            get { return this.overwriteAllVersion; }
            set { this.overwriteAllVersion = value; }
        }

        public Guid ConflictItemParentFolerGuid = Guid.Empty;

        [ThreadStatic]
        private static bool mIsNewItem;

        public bool IsNewItem
        {
            get { return mIsNewItem || mReplicatorVersionLevelProperty; }
            set { mIsNewItem = value; }
        }

        public bool IsIncludingRecycleBinData { get; set; }


        public bool ConflictWithDocument
        {
            get { return mConflictType == ConflictType.Document || mConflictType == ConflictType.Both; }
        }

        public bool OverWrite
        {
            get { return mOverWrite; }
        }

        //public int ConflictRowId
        //{
        //    get
        //    {
        //        if (ConflictWithDocument)
        //        {
        //            return mConflictDocId;
        //        }
        //        if (ConflictOnlyWithRecycleBin)
        //        {
        //            return mConflictRecycleId;
        //        }
        //        throw new Exception("Program error : Invlid conflictRowId");
        //    }
        //}

        /// <summary>
        /// 应该去掉，Skip 应该靠抛出Skip异常来控制
        /// </summary>
        public bool NeedSkipped { get; set; }

        /// <summary>
        /// 需要修改，只要不是Skip，其他情况(overwrite,append version\file)都应该清空回收站
        /// </summary>
        public bool ConflilctFromRecycleBin
        {
            get { return mConflictType == ConflictType.RecycleBin || mConflictType == ConflictType.Both; }
        }

        /// <summary>
        /// 需要修改，只有选择 skip + IsIncludingRecycleBinData 时，才应该Skip
        /// </summary>
        public bool SkipRecycleBinData
        {
            get { return (mConflictType == ConflictType.RecycleBin || mConflictType == ConflictType.Both) && !mOverWrite && IsIncludingRecycleBinData; }
        }

        public RestoreTargetTable TargetTable
        {
            get { return mTargetTable; }
            set { mTargetTable = value; }
        }

        //Connector inplace restore时, 如果List被整体删除，Blob数据是永久保存的，还原时为了避免数据多份，需要Overwrite掉
        public bool OverWriteBlob { get; set; }

        //public int DraftVersion
        //{
        //    get { return mDraftUIVersion; }
        //}
        //itemId is for Archive job which has different files with the same name.
        public bool Init(string itemName, bool overWrite, bool flagVersion)
        {
            mOverWrite = overWrite;
            mPublishingUIVersion = -1;
            mDraftUIVersion = -1;
            if (mName == null ||
                string.Compare(mName, itemName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                mIsNewItem = false;
                OverwriteAllVersion = false;
                mName = itemName;
                mNameMapping = itemName;
                //mConflictDocId = -1;
                //mConflictRecycleId = -1;
                //mCurrentUIVersion = -1;
                mOverWrite = overWrite;
                ConflictItemParentFolerGuid = Guid.Empty;
                this.NeedSkipped = false;
                if (flagVersion && overWrite)
                {
                    mIsNewItem = true;
                    return true;
                }
                return false;
            }
            return false;
        }

        public RestoreTargetTable GetTargetTable(int originalVersion, bool isVersion)
        {
            if (mConflictType == ConflictType.None || mConflictType == ConflictType.RecycleBin && mOverWrite)
            {
                return RestoreTargetTable.Unknow;   // We can make this clear, but we need not for now
            }
            else if (mConflictType == ConflictType.RecycleBin && !mOverWrite)
            {
                if (IsIncludingRecycleBinData)
                {
                    this.NeedSkipped = true; //include data in recyclebin under not overwrite
                    return RestoreTargetTable.None;
                }
                else
                {
                    mIsNewItem = true; //not include data in recyclebin under not overwrite
                    return RestoreTargetTable.Unknow;
                }
            }

            if (mPublishingUIVersion == originalVersion)  //  originalVersion eques originalVersion but not overwrite
            {
                if (mOverWrite)
                {
                    return RestoreTargetTable.AllDocs;
                }
                else
                {
                    this.NeedSkipped = true;
                    return RestoreTargetTable.None;
                }
            }
            if (mDraftUIVersion < 0)  // One published version in AllDocs
            {
                if (mPublishingUIVersion < originalVersion)    // originalVersion is bigger
                {
                    return GetTargetTable(isVersion);
                }
                return RestoreTargetTable.AllDocVersions;
            }
            else //if (mPublishingUIVersion < 0)   // No publish version in AllDocs
            {
                if ((originalVersion >= mDraftUIVersion && mDraftUIVersion >= mPublishingUIVersion)
                    || (originalVersion >= mPublishingUIVersion && mPublishingUIVersion >= mDraftUIVersion))
                // originalVersion is bigger
                {
                    return GetTargetTable(isVersion);
                }
                else if (mPublishingUIVersion < 0 && originalVersion % 512 == 0 && mDraftUIVersion - originalVersion <= 512)
                // No publish version in AllDocs and come to an publishing version in the source
                {
                    return GetTargetTable(isVersion);
                }
                return RestoreTargetTable.AllDocVersions;    // restore to AllDocVersions
            }
        }

        // 测试用的，最后需要删掉
        public RestoreTargetTable GetTargetTable(int originalVersion, bool isVersion, AveRestoreMode option)
        {
            if (mConflictType == ConflictType.None)
            {
                return RestoreTargetTable.Unknow;   // We can make this clear, but we need not for now
            }
            else if (mConflictType == ConflictType.RecycleBin)
            {
                if (IsIncludingRecycleBinData && option == AveRestoreMode.Default)
                {
                    this.NeedSkipped = true; //include data in recyclebin under not overwrite
                    return RestoreTargetTable.None;
                }
                else
                {
                    mIsNewItem = true; //not include data in recyclebin under not overwrite
                    return RestoreTargetTable.Unknow;
                }
            }
            if (mPublishingUIVersion == originalVersion)  //  originalVersion eques originalVersion but not overwrite
            {
                if (mOverWrite)
                {
                    return RestoreTargetTable.AllDocs;
                }
                else
                {
                    this.NeedSkipped = true;
                    return RestoreTargetTable.None;
                }
            }
            if (mDraftUIVersion < 0)  // One published version in AllDocs
            {
                if (mPublishingUIVersion < originalVersion)    // originalVersion is bigger
                {
                    return GetTargetTable(isVersion);
                }
                return RestoreTargetTable.AllDocVersions;
            }
            else //if (mPublishingUIVersion < 0)   // No publish version in AllDocs
            {
                if ((originalVersion >= mDraftUIVersion && mDraftUIVersion >= mPublishingUIVersion)
                    || (originalVersion >= mPublishingUIVersion && mPublishingUIVersion >= mDraftUIVersion))
                // originalVersion is bigger
                {
                    return GetTargetTable(isVersion);
                }
                else if (mPublishingUIVersion < 0 && originalVersion % 512 == 0 && mDraftUIVersion - originalVersion <= 512)
                // No publish version in AllDocs and come to an publishing version in the source
                {
                    return GetTargetTable(isVersion);
                }
                return RestoreTargetTable.AllDocVersions;    // restore to AllDocVersions
            }
        }

        private RestoreTargetTable GetTargetTable(bool isVersion)
        {
            //if (mIsNewItem||true)
            //{
            if (isVersion)
            {
                return RestoreTargetTable.AllDocVersions;
            }
            else
            {
                return RestoreTargetTable.AllDocs;
            }
            //}
            //else
            //{
            //    this.NeedSkipped = true;
            //    return RestoreTargetTable.None;
            //}
        }

        public void ReSetItemName(string name)
        {
            mNameMapping = name;
        }
        /// <summary>
        /// This function is to reset name & newItem for manually setting some item as new item or not.
        /// </summary>
        /// <param name="isNewItem"></param>
        /// <param name="name"></param>
        /// <param name="itemName"></param>
        public void ResetNewItemValues(bool isNewItem, string name, string nameMapping)
        {
            mIsNewItem = isNewItem;
            mName = name;
            mNameMapping = nameMapping;
        }
        /// <summary>
        /// For replicator
        /// </summary>
        public static void SetReplicator()
        {
            mReplicatorVersionLevelProperty = true;
        }
        public static bool GetIsReplicator()
        {
            return mReplicatorVersionLevelProperty;
        }

        [Obsolete]
        public static void SetRestoreCurrentVersion()
        {
            mRestoreCurrentVersion = true;
        }

        /// <summary>
        /// if restoreCurrentVersion is false, we will skip this version when current version equals restoring version
        /// </summary>
        /// <param name="restoreCurrentVersion"></param>
        [Obsolete]
        public static void SetRestoreCurrentVersion(bool restoreCurrentVersion)
        {
            mRestoreCurrentVersion = restoreCurrentVersion;
        }

        public bool Skip(int result)
        {
            if (result == Int32.MinValue ||
                (result == 2 && !mOverWrite))
            {
                return true;
            }
            return false;
        }

        public bool IsSameItem(string cName)
        {
            return string.Compare(mName, cName, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public ConflictType ConflictType
        {
            get
            {
                return this.mConflictType;
            }
            set
            {
                this.mConflictType = value;
            }
        }

        //public ConflictSolution ConflictSolution
        //{
        //    get;
        //    set;
        //}

        public int PublishingUIVersion
        {
            get
            {
                return mPublishingUIVersion;
            }
            set
            {
                mPublishingUIVersion = value;
            }
        }

        public int DraftUIVersion
        {
            get
            {
                return mDraftUIVersion;
            }
            set
            {
                mDraftUIVersion = value;
            }
        }

        public int ConflictRowId
        {
            get { return mConflictRowId; }
            set { mConflictRowId = value; }
        }

        /// <summary>
        /// reset thread static properties to default value;
        /// </summary>
        public void ResetThreadStaticProperties()
        {
            mName = null;
            mNameMapping = null;
            mIsNewItem = false;
            mOverWrite = false;
            mPublishingUIVersion = 0;
            mDraftUIVersion = 0;
        }
    }

    [Flags]
    public enum ConflictType
    {
        None = 0,
        RecycleBin = 1,
        Document = 2,
        Both = 3
    }

    public enum RestoreTargetTable
    {
        None = 0,
        AllDocs = 1,
        AllDocVersions = 2,
        Unknow = 3
    }

    //public enum ConflictSolution
    //{
    //    Skip = 0,
    //    OverWrite = 1,
    //    AppendVersion = 2,
    //    AppendFile = 3
    //}

    public class AveFolderBrowserInfo
    {
        public string ServerRelativeUrl;
        public string Name;
        public string Url;
        public Guid ParentListId;
        public Guid RootFolderListId;
        public Guid ParentId;
        public Guid UniqueId;
        //public bool ListHasUniqueRoleAssignments;
        public bool HasUniqueRoleAssignments;
        public bool Hidden;
    }

    public class AveItemBrowserInfo
    {
        public string Url;
        public string Name;
        public string DisplayName;
        public Guid UniqueId;
        public int ID;
        public Guid ParentFolderUniqueID;
        public Guid ParentListID;
        public int ListBaseType;
        public bool HasUniqueRoleAssignments;
        public Dictionary<string, byte> Versions = new Dictionary<string, byte>();
        public string CurrentUIVersionString;
        public int LastModifier;
        public string LastModifierName;
        public DateTime LastModifyTime;// utc time
        public byte Level;
        public Guid TpGuid;
    }

    public class AveItemVersionBrowserInfo
    {
        public string Url;
        public string VersionLabel;
        public string ItemName;
        public string ItemDisplayName;
        public Guid ItemID;
        public Guid ItemUniqueID;

    }

    /// <summary>
    /// for 13, every file consists of serveral shreds
    /// </summary>
    public class AveShredInfo
    {
        public byte Partition { get; set; }
        public long BSN { get; set; }

        public int Size { get; set; }
        public byte[] RBSId { get; set; }

    }

    public class AveDocInfo
    {
        public Guid Id { get; set; }
        public string DirName { get; set; }
        public string LeafName { get; set; }
        public int DoclibRowId { get; set; }
        public byte Type { get; set; }
        public byte SortBehavior { get; set; }
        public int Size { get; set; }
        public int UIVersion { get; set; }
        public bool Dirty { get; set; }
        public bool ListDataDirty { get; set; }
        //only in SP 10;
        public Guid CacheParseId { get; set; }
        public int DocFlags { get; set; }
        public bool ThicketFlag { get; set; }
        public int CharSet { get; set; }
        public string ProgId { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeLastModified { get; set; }
        public DateTime NextToLastTimeModified { get; set; }
        public DateTime MetaInfoTimeLastModified { get; set; }
        public DateTime TimeLastWritten { get; set; }
        public byte SetupPathVersion { get; set; }
        public string SetupPath { get; set; }
        public string SetupPathUser { get; set; }
        public int CheckoutUserId { get; set; }
        public DateTime CheckoutDate { get; set; }
        public DateTime CheckoutExpires { get; set; }
        public bool VersionCreatedSinceSTCheckout { get; set; }
        public int LTCheckoutUserId { get; set; }
        public int VirusVendorID { get; set; }
        public byte VirusStatus { get; set; }
        public string VirusInfo { get; set; }
        public byte[] MetaInfo { get; set; }
        public int MetaInfoSize { get; set; }
        public int MetaInfoVersion { get; set; }
        public byte[] UnVersionedMetaInfo { get; set; }
        public int UnVersionedMetaInfoSize { get; set; }
        public int UnVersionedMetaInfoVersion { get; set; }
        public string WelcomePageUrl { get; set; }
        public string WelcomePageParameters { get; set; }
        public bool IsCurrentVersion { get; set; }
        //it should be byte type
        public int Level { get; set; }
        public string CheckinComment { get; set; }
        public int AuditFlags { get; set; }
        public int InheritAuditFlags { get; set; }
        public int DraftOwnerId { get; set; }
        public string UIVersionString { get; set; }
        public Guid ParentId { get; set; }
        public bool HasStream { get; set; }
        public Guid ScopeId { get; set; }
        public byte[] BuildDependencySet { get; set; }
        public int ParentVersion { get; set; }
        public string ParentVersionString { get; set; }
        public Guid TransformerId { get; set; }
        public string ParentLeafName { get; set; }
        public int IsCheckoutToLocal { get; set; }
        public short CtoOffset { get; set; }
        public string Extension { get; set; }
        public string ExtensionForFile { get; set; }
        public int ItemChildCount { get; set; }
        public int FolderChildCount { get; set; }
        public byte[] FileFormatMetaInfo { get; set; }
        public int FileFormatMetaInfoSize { get; set; }
        public int ListSchemaVersion { get; set; }
        public string ClientId { get; set; }
        public int InternalVersion { get; set; }
        public byte BumpVersion { get; set; }
        //only in SP13
        public byte StreamSchema { get; set; }
    }
}
