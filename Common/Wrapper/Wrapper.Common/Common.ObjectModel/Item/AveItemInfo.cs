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
        public Guid ParentListId { get; set; }
        public string ParentFolderRelativeUrl { get; set; }
        public bool IsCheckOut { get; set; }
        public bool IsCurrentVersion { get; set; }
        public bool IsNewCreatedDoc { get; set; }

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

        public int RestoreOption { set; get; }

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

        /// <summary>
        /// 注意，该属性只能记录restore时先删除后还原情况下，删除前folder/document/listitem的docid;如果没有删除，那么就赋值为对应item的docid
        /// </summary>
        public Guid OldUniqueId { set; get; }

        private bool isInCommunityDiscussion = false;
        public bool IsInCommunityDiscussion
        {
            get { return isInCommunityDiscussion; }
            set { isInCommunityDiscussion = value; }
        }

        /// <summary>
        /// Only used for cache
        /// </summary>
        public IAveListItem Item { get; set; }
    }
    /// <summary>
    /// Add For update field value
    /// </summary>
    public class AveItemFieldsInfo
    {
        //Add By Matthew
        public Dictionary<string, object> Fields { get; set; }  //现在所有的FieldsValue Info都存在了这个字典里，到后台处理时会根据Version不同进行区别处理。

        public Dictionary<string, object> UniqueValueFields { get; set; }

        public Dictionary<string, object> FieldsInMetaInfo { get; set; }

        public Dictionary<string, object> MultilookupFields { get; set; }

        public Dictionary<Guid, Guid> TermIdMapping { get; set; }

        public Dictionary<string, string> TaxonomyFieldsInMapping { get; set; }
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

        public bool IsNewCreated { get; set; }

        private bool needChangeItemId = true;
        public bool NeedChangeItemId 
        { 
            get { return needChangeItemId; }
            set { needChangeItemId = value; }
        }

        public AveListItemInfoExtension Extension = new AveListItemInfoExtension();

    }


    public class AveSettingInfo
    {
        public bool DELETE_ITEM { get; set; }

        public bool KEEP_ITEM_TPGUID { get; set; }

        public bool MOVE_ITEM_TO_CONFLICT_FOLDER { get; set; }

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
        public bool IsNewCreated { get; set; }
        /// <summary>
        /// Is new created folder when restore this version
        /// </summary>
        public bool IsNewCreatedFolder { get; set; }
        public string ParentListName { get; set; }
        public bool ParentListIsSystem { get; set; }
        public Guid ListRootFolderId { get; set; }
        public bool IsOverWrite { get; set; }
    }

    public class AveDocumentInfo : AveBaseItemInfo
    {

        public string Url { get; set; }

        public string SourceWebUrl { get; set; }

        public bool HasMoveUp { get; set; }

        public string SetupPath { get; set; }

        public bool IsGhostPage { get; set; }

        public Guid SolutionId { get; set; }

        public string CheckinComment { get; set; }

        public Guid OrignialID { get; set; }

        public bool IsView { get; set; }

        public bool IsNewCreatedView { get; set; }

        private bool needChangeItemId = true;
        public bool NeedChangeItemId 
        { 
            get { return needChangeItemId; }
            set { needChangeItemId = value; }        
        }

        public bool Needskip { get; set; }

        public bool IsOrignialCheckOut { get; set; }

        public Dictionary<Guid, Guid> ListViewMapping;

        public bool IsOverWrite { get; set; }

        public AveModerationStatusType ModerationType { get; set; }

        public Guid OldDocId { get; set; }

        public AveViewDocInfo AveView = new AveViewDocInfo();

        public int GhostPageOption { get; set; }

        private int imageWidth;

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")]
        public int ImageWidth
        {
            get
            {
                if (this.MetaInfoDic != null && this.MetaInfoDic.ContainsKey("vti_lastwidth"))
                {
                    imageWidth = Convert.ToInt32(MetaInfoDic["vti_lastwidth"]);
                }
                else
                {
                    imageWidth = 0;
                }
                return imageWidth;
            }
            set
            {
                imageWidth = value;
            }
        }
        private int imageHeight;
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "SharePoint property")]
        public int ImageHeight
        {
            get
            {
                if (this.MetaInfoDic != null && this.MetaInfoDic.ContainsKey("vti_lastheight"))
                {
                    imageHeight = Convert.ToInt32(MetaInfoDic["vti_lastheight"]);
                }
                else
                {
                    imageHeight = 0;
                }
                return imageHeight;
            }
            set
            {
                imageHeight = value;
            }
        }

        //如果文件被设置成master page，不能被删除，但可以被move。还原文件逻辑中队该文件使用move，需要缓存记录move前web的master page设置。
        public List<AveExtendMasterPageInfo> TempMasterSettings;
        public string TempFileUrl;

        public AveWebPartCache WebPartCache;
        public List<AveWebPartBaseInfo> WebParts;
        public bool WebPartRestored = false;
        public bool SourceCommentsDisabled { get; set; }
        public int SourceCommentsDisabledScope { get; set; }
    }


    public class AveHiddenFileInfo
    {

        public string ID { get; set; }

        public string Name { get; set; }

        public byte Level { get; set; }

        public int DocFlags { get; set; }

        public int Version { get; set; }
    }

    public class AveViewDocInfo
    {
        public string ViewUrl { get; set; }
        public List<AveViewInfo> Vinfos = new List<AveViewInfo>();
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

        public Guid Id
        {
            get { return mId; }
            set { mId = value; }
        }

        public int RowOrdinal
        {
            get { return mRowOrdinal; }
            set { mRowOrdinal = value; }
        }


        public AveFieldValueInfo(string colName, object colValue, int rowOrdinal)
        {
            this.ColName = colName;
            this.ColValue = colValue;
            this.RowOrdinal = rowOrdinal;
        }

        public AveFieldValueInfo()
        { }

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
        public readonly int MajorVersion;
        public readonly int MinorVersion;

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

        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                var version = (AveItemVersionNumber)obj;
                return (MajorVersion * 512 + MinorVersion).Equals(version.MajorVersion * 512 + version.MinorVersion);
            }
            else
            {
                return false;
            }           
        }
    }

    public class RestoringDto
    {
        private static bool mRestoreCurrentVersion = false;
        [ThreadStatic]
        private static string mName;
        [ThreadStatic]
        private static string mNameMapping;

        public string NameMapping
        {
            get { return mNameMapping; }
            set { mNameMapping = value; }
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

        private ConfictType mConfictType;
        private RestoreTargetTable mTargetTable;

        private bool overwriteAllVersion;

        public  bool OverwriteAllVersion
        {
            get { return this.overwriteAllVersion; }
            set { this.overwriteAllVersion = value; }
        }

        public Guid ConflictItemParentFolerGuid = Guid.Empty;

        //For Replicator
        public static bool ChangeToServerRelativeUrl = false;

        [ThreadStatic]
        private static bool mIsNewItem;

        public bool IsNewItem
        {
            get { return mIsNewItem || mRestoreCurrentVersion; }
            set { mIsNewItem = value; }
        }

        public bool IsIncludingRecycleBinData { get; set; }


        public bool ConflictWithDocument
        {
            get { return mConfictType == ConfictType.Document || mConfictType == ConfictType.Both; }
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

        public bool NeedSkipped { get; set; }

        public string NeedSkippedReason { get; set; }

        public string NeedSkippedKey { get; set; }

        public bool DeleteFromRecycleBin
        {
            get { return (mConfictType == ConfictType.RecycleBin || mConfictType == ConfictType.Both) && mOverWrite; }
        }

        public bool SkipRecycleBinData
        {
            get { return (mConfictType == ConfictType.RecycleBin || mConfictType == ConfictType.Both) && !mOverWrite && IsIncludingRecycleBinData; }
        }

        public RestoreTargetTable TargetTable
        {
            get { return mTargetTable; }
            set { mTargetTable = value; }
        }

        //public int DraftVersion
        //{
        //    get { return mDraftUIVersion; }
        //}

        public bool Init(string itemName, bool overWrite, bool flagVersion)
        {
            mOverWrite = overWrite;
            this.NeedSkipped = false;
            if (mName == null ||
                string.Compare(mName, itemName, StringComparison.OrdinalIgnoreCase) != 0)
            {
                mConfictType = ConfictType.None;
                mIsNewItem = false;
                OverwriteAllVersion = false;
                mName = itemName;
                mNameMapping = itemName;
                //mConflictDocId = -1;
                //mConflictRecycleId = -1;
                mPublishingUIVersion = -1;
                mDraftUIVersion = -1;
                //mCurrentUIVersion = -1;
                mOverWrite = overWrite;
                ConflictItemParentFolerGuid = Guid.Empty;
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
            if (mConfictType == ConfictType.None || mConfictType == ConfictType.RecycleBin && mOverWrite)
            {
                return RestoreTargetTable.Unknow;   // We can make this clear, but we need not for now
            }
            else if (mConfictType == ConfictType.RecycleBin && !mOverWrite)
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
                else if (mPublishingUIVersion < 0 && originalVersion % 512 == 0 && mDraftUIVersion - originalVersion < 512)
                // No publish version in AllDocs and come to an publishing version in the source
                {
                    return GetTargetTable(isVersion);
                }
                return RestoreTargetTable.AllDocVersions;    // restore to AllDocVersions
            }
        }

        private RestoreTargetTable GetTargetTable(bool isVersion)
        {
            if (mIsNewItem || mRestoreCurrentVersion)
            {
                if (isVersion)
                {
                    return RestoreTargetTable.AllDocVersions;
                }
                else
                {
                    return RestoreTargetTable.AllDocs;
                }
            }
            else
            {
                this.NeedSkipped = true;
                return RestoreTargetTable.None;
            }
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
            mRestoreCurrentVersion = true;
            ChangeToServerRelativeUrl = true;
        }

        public static void ResetReplicator()
        {
            mRestoreCurrentVersion = false;
            //ChangeToServerRelativeUrl = false;
        }

        public static void SetRestoreCurrentVersion()
        {
            mRestoreCurrentVersion = true;
        }

        public bool Skip(int result)
        {
            if (result == Int32.MinValue ||
                (result == 2 && !(mRestoreCurrentVersion && mOverWrite)))
            {
                return true;
            }
            return false;
        }

        public bool IsSameItem(string cName)
        {
            return string.Compare(mName, cName, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public ConfictType ConfictType
        {
            get
            {
                return this.mConfictType;
            }
            set
            {
                this.mConfictType = value;
            }
        }

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
    }

    public enum ConfictType
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
}
