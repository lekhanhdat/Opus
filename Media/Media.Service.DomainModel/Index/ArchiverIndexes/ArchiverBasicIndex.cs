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




namespace AvePoint.Media.Service.DomainModel
{
    using AvePoint.GCommon.Contract.StorageOptimization.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.RA.CommonUtil;
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using System.Text.Json.Serialization;
    using System.Xml;
    #endregion

    [Serializable]
    public class ArchiverBasicIndex
        : IndexBase
        , IIndexable
    {
        private static RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        [Column("COL_ID")]
        public string Id { get; set; }

        [Column("COL_FLAG")]
        public long Flag { get { return this.CurrentItemDataMode; } set { this.CurrentItemDataMode = value; } }

        [Column("COL_TYPE")]
        public string Type { get; set; }

        [Column("COL_NAME")]
        public string Name { get { return this.CurrentItemName; } set { this.CurrentItemName = value; } }

        [Column("COL_PATH_MD5")]
        public string PathMD5 { get; set; }

        [Column("COL_PARENT_PATH_MD5")]
        public string ParentPathMD5 { get; set; }

        [Column("COL_DATA_FILE_NUMBER")]
        public long DataFileNumber { get { return this.CurrentItemMetaDataStartFileNumber; } set { this.CurrentItemMetaDataStartFileNumber = value; } }

        [Column("COL_DATA_FILE_OFFSET")]
        public long DataFileOffset { get { return this.CurrentItemMetaDataStartOffset; } set { this.CurrentItemMetaDataStartOffset = value; } }

        [Column("COL_DATA_FILE_LENGTH")]
        public long DataFileLength { get { return this.CurrentItemMetaDataAndContentDataTotalLength; } set { this.CurrentItemMetaDataAndContentDataTotalLength = value; } }

        [Column("COL_CRC")]
        public long Crc { get; set; }

        [Column("COL_FILE_HEADER_TYPE")]
        public int BackupFileType { get; set; }

        [Column("COL_ARCHIVE_TIME")]
        public long ArchiveTime { get; set; }
        [Column("COL_CREATE_TIME")]
        public long CreateTime { get; set; }
        [Column("COL_MODIFY_TIME")]
        public long ModifyTime { get; set; }
        [Column("COL_AUTHOR")]
        public String Author { get; set; }

        [Column("COL_ATTRIBUTES")]
        public string Attributes { get; set; }

        [Column("COL_EXTRAINFO")]
        public string ExtraInfo { get; set; }

        [Column("COL_PLANID")]
        public string PlanId { get; set; }


        [Column("COL_CYCLEID")]
        public string CycleId { get; set; } // unused

        /// <summary>
        /// For dedup file, Job Id is this duplicate file's archiver(backup) job id
        /// </summary>
        [Column("COL_JOBID")]
        public string JobId { get { return this._jobId; } set { this._jobId = value; if (string.IsNullOrEmpty(DedupSourceFileJobId)) { base.BackupJobId = value; } } }

        [Column("COL_SEQUENCE")]
        public long Sequence { get; set; }

        [Column("COL_STORAGEPOLICYID")]
        public String StoragePolicyId { get; set; }

        [Column("COL_EXTENSION_1")]
        public int ListType { get; set; }

        [Column("COL_ITEMID")]
        public string NodeGuid { get; set; }
        /// <summary>
        /// AccessTier(2 Bitwise): --set--[(FlagExtend & ~0x3) | ((int)(value))] --get--[(AccessTierType)(FlagExtend & 0x3)]
        /// ...
        /// </summary>
        [Column("COL_EXTENSION_2")]
        public int FlagExtend { get; set; }

        /// <summary>
        /// 用于记录是否为重复数据，枚举是： IndexDeduplicateFileStatus
        /// 相同CRC64的Files，只有其中一个会标记成 2 作为 Source File，其他都是 1 Duplicate File
        /// </summary>
        [Column("COL_EXTENSION_3")]
        public int DuplicateStatus { get; set; }

        [Column("COL_ISSYSTEMFILE")]
        public String IsSystemFile { get; set; }

        [Column("COL_STUBINFO")]
        public string stubInfo { get; set; }

        [Column("COL_EXTENSION_4")]
        public long ContentOffset { get { return this.CurrentItemMetaDataInnerOffset; } set { this.CurrentItemMetaDataInnerOffset = value; } }

        [Column("COL_EXTENSION_5")]
        public long ContentLength { get { return this.CurrentItemContentDataTotalLength; } set { this.CurrentItemContentDataTotalLength = value; } }

        [Column("COL_EXTENSION_6")]
        public string IsFailed { get; set; }

        [Column("COL_EXTENSION_7")]
        public String Url { get; set; }

        [Column("COL_EXTENSION_8")]
        ///public string StorageCrc32 { get { return this.CurrentItemStorageCrc; } set { this.CurrentItemStorageCrc = value; } }
        public string StorageCrc64 { get; set; }
        [Column("COL_EXTENSION_9")]
        public string Editor { get; set; }

        [Column("COL_EXTENSION_10")]
        public string ListBaseType { get; set; }

        [Column("COL_CONTENT_DATA_OFFSET")]
        public long ContentDataOffset { get { return this.CurrentItemContentDataStartOffset; } set { this.CurrentItemContentDataStartOffset = value; } }

        /// <summary>
        /// For dedup file, use to save Source File's content data file number.
        /// </summary>
        [Column("COL_CONTENT_DATA_FILE_NUMBER")]
        public long ContentDataFileNumber { get { return this.CurrentItemContentDataStartFileNumber; } set { this.CurrentItemContentDataStartFileNumber = value; } }

        /// <summary>
        /// For dedup file, use to save Source File's content data storage info.
        /// </summary>
        [Column("COL_STORAGEINFO")]
        public string StorageInfo { get { return base.StorageInformation; } set { base.StorageInformation = value; } }

        [Column("COL_VERSION")]
        public long Version { get { return this.CurrentItemVersion; } set { this.CurrentItemVersion = value; } }

        [Column("COL_META_DATA_HEADER_OFFSET")]
        public long MetaDataHeaderOffset { get { return this.CurrentItemMetaDataDataHeaderStartOffset; } set { this.CurrentItemMetaDataDataHeaderStartOffset = value; } }

        [Column("COL_CONTENT_DATA_HEADER_OFFSET")]
        public long ContentDataHeaderOffset { get { return this.CurrentItemContentDataDataHeaderStartOffset; } set { this.CurrentItemContentDataDataHeaderStartOffset = value; } }

        [Column("COL_CONTENT_PAGE_SIZE")]
        public long ContentPageSize { get { return this.CurrentItemPageSize; } set { this.CurrentItemPageSize = value; } }

        [Column("COL_PLATFORM_TYPE")]
        public int PlatformType { get { return base.PlatformType; } set { base.PlatformType = value; } }

        [Column("COL_SITE_PATH")]
        public string SitePath { get; set; }

        /// <summary>
        /// In dedup index: 0: wait to delete, 1: already deleted, 2: not need delete
        /// </summary>
        [Column("COL_DEL_STATUS")]
        public int DelStatus { get; set; }

        [Column("COL_META_TAIL_LENGTH")]
        public long SoftDeleteTime { get; set; }

        [Column("COL_USESNAPLOCK ")]
        public int UseSnapLock { get; set; }

        [Column("COL_RETENTION_STATUS")]
        public int RetentionStatus { get; set; }

        [Column("COL_RETENTION")]
        public string Retention { get; set; }

        [Column("COL_ARCHIVE_BY")]
        public string ArchiveBy { get; set; }

        [Column("COL_IS_APP_DATA")]
        public String IsAppData { get; set; }

        [Column("COL_APP_DATA_NAME")]
        public String AppDataName { get; set; }

        [Column("COL_RECYCLE_TIME")]
        public long DedupTime { get; set; }

        /// <summary>
        /// Use to save Deuplicate file's source file index id
        /// </summary>
        [Column("COL_BLOB_INFO")]
        public String DedupSourceFileId { get; set; }

        /// <summary>
        /// Dedup Extension Info
        /// </summary>
        [Column("COL_POOL_GUID")]
        public String DedupExtension 
        {
            get
            {
                return _dedupExtension;
            }
            set
            {
                _dedupExtension = value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    try
                    {
                        _dedupExtensionInfo = SerializerHelper.DeserializeByDataContractJsonSerializer<DedupExtensionInfo>(value);
                        base.BackupJobId = _dedupExtensionInfo.DedupSourceFileJobId;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"DeserializeByDataContractJsonSerializer failed.DedupExtension:{value}.Message:{ex}.");
                        _dedupExtensionInfo = null;
                    }
                }
                else
                {
                    _dedupExtensionInfo = null;
                }
            }
        }

        [Column("COL_CONTENT_DATA_FILE_PREFIX_NUMBER")]
        public long ContentDataFilePrefixNumber { get; set; }   // unused

        [Column("COL_SUB_RETENTION")]
        public string SubRetention { get; set; }    // unused

        [Column("COL_PRUNE_TIME")]
        public long PruneTime { get; set; } // unused

        /// <summary>
        /// save the duplicate file data's file number
        /// </summary>
        public long DuplicateFileNumber => _dedupExtensionInfo?.DuplicateFileNumber ?? 0L;

        /// <summary>
        /// save the duplicate file data's storage info
        /// </summary>
        public string DuplicateFileStorageInfo => _dedupExtensionInfo?.DuplicateFileStorageInfo ?? string.Empty;

        /// <summary>
        /// save the duplicate file data's content item data mode
        /// </summary>
        public long DedupSourceFileFlag => _dedupExtensionInfo?.DedupSourceFileFlag ?? 0L;

        public bool IsDeduplicateData => this.DuplicateStatus == (int)IndexDeduplicateFileStatus.DuplicateFile;

        /// <summary>
        /// For dedup file, use to save Source File's archiver(backup) Job Id
        /// </summary>
        public string DedupSourceFileJobId => _dedupExtensionInfo?.DedupSourceFileJobId;

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ArchiverBasicIndex: JobId:");
            sb.Append(this.JobId);
            sb.Append(" Type: ");
            sb.Append(this.Type);
            sb.Append(" Name: ");
            sb.Append(this.Name);
            sb.Append(" Attributes: ");
            sb.Append(this.Attributes);
            sb.Append(" PlanId: ");
            sb.Append(this.PlanId);
            sb.Append(" Version: ");
            sb.Append(this.Version);
            return sb.ToString();
        }

        /// <summary>
        /// 这个字段不是DB中的字段，是为了排序使用
        /// </summary>
        public float ItemMajorVersion
        {
            get
            {
                float majorVersion = float.MaxValue;
                if (this.Type.Equals("D", StringComparison.OrdinalIgnoreCase)
                    || this.Type.Equals("I", StringComparison.OrdinalIgnoreCase)
                    || this.Type.Equals("U", StringComparison.OrdinalIgnoreCase)
                    || this.Type.Equals("V", StringComparison.OrdinalIgnoreCase))
                {
                    int flag = this.Name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
                    if (flag >= 0)
                    {
                        string versionStr = this.Name.Substring(flag + 1);
                        String[] version = versionStr.Split('.');
                        if (!float.TryParse(version[0], out majorVersion))
                        {
                            majorVersion = float.MaxValue;
                        }
                    }
                }
                return majorVersion;
            }
        }

        /// <summary>
        /// 这个字段不是DB中的字段，是为了排序使用
        /// </summary>
        public float ItemMinorVersion
        {
            get
            {
                float minorVersion = float.MaxValue;
                if (this.Type.Equals("D", StringComparison.OrdinalIgnoreCase)
                    || this.Type.Equals("I", StringComparison.OrdinalIgnoreCase)
                    || this.Type.Equals("U", StringComparison.OrdinalIgnoreCase)
                    || this.Type.Equals("V", StringComparison.OrdinalIgnoreCase))
                {
                    int flag = this.Name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
                    if (flag >= 0)
                    {
                        string versionStr = this.Name.Substring(flag + 1);
                        String[] version = versionStr.Split('.');
                        if (!float.TryParse(version[1], out minorVersion))
                        {
                            minorVersion = float.MaxValue;
                        }
                    }
                }
                return minorVersion;
            }
        }

        /// <summary>
        /// 这个字段不是DB中的字段，是为了排序使用
        /// </summary>
        public float ItemVersionNumber
        {
            get
            {
                float version = float.MaxValue;
                if (this.Type.Equals("D", StringComparison.OrdinalIgnoreCase)
                    || this.Type.Equals("I", StringComparison.OrdinalIgnoreCase)
                    || this.Type.Equals("A", StringComparison.OrdinalIgnoreCase)
                    || this.Type.Equals("U", StringComparison.OrdinalIgnoreCase)
                    || this.Type.Equals("V", StringComparison.OrdinalIgnoreCase))
                {
                    int flag = this.Name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
                    if (flag >= 0)
                    {
                        string versionStr = this.Name.Substring(flag + 1);
                        if (!float.TryParse(versionStr, out version))
                        {
                            version = float.MaxValue;
                        }
                    }
                }
                return version;
            }
        }

        /// <summary>
        /// 这个字段不是DB中的字段，是为了排序使用
        /// </summary>
        public string ItemName
        {
            get
            {
                string itemName = this.Name;
                if (this.Type.Equals("A", StringComparison.OrdinalIgnoreCase)
                    || this.Type.Equals("D", StringComparison.OrdinalIgnoreCase)
                    || this.Type.Equals("I", StringComparison.OrdinalIgnoreCase)
                    || this.Type.Equals("U", StringComparison.OrdinalIgnoreCase)
                    || this.Type.Equals("V", StringComparison.OrdinalIgnoreCase))
                {
                    int flag = this.Name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
                    if (flag >= 0)
                    {
                        itemName = this.Name.Substring(0, flag);
                    }
                }
                return itemName;
            }
        }

        /// <summary>
        /// 这个字段不是DB中的字段，该字段记录根据数据的retention时间获得的数据最后被删除的时间
        /// </summary>
        public Int64 FinalDisposition { get; set; }

        /// <summary>
        /// 这个字段不是DB中的字段，该字段记录数据所在的时区
        /// </summary>
        public String TimeZoneId { get; set; }

        /// <summary>
        /// 用于记录archive file的 job 的id
        /// </summary>
        private String _jobId;
        /// <summary>
        /// 用于记录 dedup file的信息
        /// </summary>
        private String _dedupExtension;
        private DedupExtensionInfo? _dedupExtensionInfo;

        #region stub info
        private bool? _hasStub;
        private string? _stubId;
        private LeaveStubType _leaveStubType = LeaveStubType.None;
        public bool HasStub
        {
            get
            {
                if (_hasStub.HasValue) return _hasStub.Value;

                _hasStub = false;
                if (string.IsNullOrWhiteSpace(stubInfo) || stubInfo.Equals("null", StringComparison.OrdinalIgnoreCase)
                    || (bool.TryParse(IsSystemFile, out var isSystemFile) && isSystemFile) // system file doest not have stub
                    )
                {
                    return _hasStub.Value;
                }

                try
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(stubInfo);
                    var element = doc.GetElementsByTagName("StubInfo").Cast<XmlElement>().FirstOrDefault();
                    if (element == null) return _hasStub.Value;
                    var id = element.HasAttribute("StubId") ? element.GetAttribute("StubId") : null;
                    var type = element.HasAttribute("StubType") ? element.GetAttribute("StubType") : null;
                    if (string.IsNullOrWhiteSpace(id) && (string.IsNullOrWhiteSpace(type) || type == "null")) return _hasStub.Value;

                    _hasStub = true;
                    _stubId = id;
                    if (!string.IsNullOrEmpty(type)) Enum.TryParse(type, true, out _leaveStubType);
                    return _hasStub.Value;
                }
                catch (Exception ex)
                {
                    logger.Warn($"Invalid StubInfo: {stubInfo}, ex: {ex}");
                    return _hasStub.Value;
                }
            }
        }

        public string? StubId
        {
            get
            {
                if (!HasStub) return null;
                return _stubId;
            }
        }

        public LeaveStubType LeaveStubType
        {
            get
            {
                if (!HasStub) return LeaveStubType.None;
                return _leaveStubType;
            }
        }
        #endregion

        public override Dictionary<String, Object> GenerateInsertDatabaseParameters()
        {
            Dictionary<String, Object> dic = new Dictionary<String, Object>();
            dic.Add("@COL_ID", this.Id);
            dic.Add("@COL_FLAG", this.Flag);
            dic.Add("@COL_TYPE", this.Type);
            dic.Add("@COL_NAME", this.Name);
            dic.Add("@COL_PATH_MD5", this.PathMD5);
            dic.Add("@COL_PARENT_PATH_MD5", this.ParentPathMD5);
            dic.Add("@COL_DATA_FILE_NUMBER", this.DataFileNumber);
            dic.Add("@COL_DATA_FILE_OFFSET", this.DataFileOffset);
            dic.Add("@COL_DATA_FILE_LENGTH", this.DataFileLength);
            dic.Add("@COL_CRC", this.Crc);
            dic.Add("@COL_FILE_HEADER_TYPE", this.BackupFileType);
            dic.Add("@COL_ARCHIVE_TIME", this.ArchiveTime);
            dic.Add("@COL_ATTRIBUTES", this.Attributes);
            dic.Add("@COL_EXTRAINFO", this.ExtraInfo);
            dic.Add("@COL_ITEMID", this.NodeGuid);
            dic.Add("@COL_PLANID", this.PlanId);
            dic.Add("@COL_CYCLEID", this.CycleId);
            dic.Add("@COL_JOBID", this.JobId);
            dic.Add("@COL_ISSYSTEMFILE", this.IsSystemFile);
            dic.Add("@COL_STUBINFO", this.stubInfo);
            dic.Add("@COL_SEQUENCE", this.Sequence);
            dic.Add("@COL_STORAGEPOLICYID", this.StoragePolicyId);
            dic.Add("@COL_EXTENSION_1", this.ListType);
            dic.Add("@COL_EXTENSION_2", this.FlagExtend);
            dic.Add("@COL_EXTENSION_3", this.DuplicateStatus);
            dic.Add("@COL_EXTENSION_4", this.ContentOffset);
            dic.Add("@COL_EXTENSION_5", this.ContentLength);
            dic.Add("@COL_EXTENSION_6", this.IsFailed);
            dic.Add("@COL_EXTENSION_7", this.Url);
            dic.Add("@COL_EXTENSION_8", this.StorageCrc64);
            dic.Add("@COL_EXTENSION_9", this.Editor);
            dic.Add("@COL_EXTENSION_10", this.ListBaseType);
            dic.Add("@COL_CONTENT_DATA_OFFSET", this.ContentDataOffset);
            dic.Add("@COL_CONTENT_DATA_FILE_NUMBER", this.ContentDataFileNumber);
            dic.Add("@COL_STORAGEINFO", this.StorageInfo);
            dic.Add("@COL_VERSION", this.Version);
            dic.Add("@COL_META_DATA_HEADER_OFFSET", this.MetaDataHeaderOffset);
            dic.Add("@COL_CONTENT_DATA_HEADER_OFFSET", this.ContentDataHeaderOffset);
            dic.Add("@COL_CONTENT_PAGE_SIZE", this.ContentPageSize);
            dic.Add("@COL_PLATFORM_TYPE", this.PlatformType);
            dic.Add("@COL_SITE_PATH", this.SitePath);
            dic.Add("@COL_DEL_STATUS", this.DelStatus);
            dic.Add("@COL_META_TAIL_LENGTH", this.SoftDeleteTime);
            dic.Add("@COL_USESNAPLOCK", this.UseSnapLock);
            dic.Add("@COL_RETENTION_STATUS", this.RetentionStatus);
            dic.Add("@COL_RETENTION", this.Retention);
            dic.Add("@COL_ARCHIVE_BY", this.ArchiveBy);
            dic.Add("@COL_IS_APP_DATA", this.IsAppData);
            dic.Add("@COL_APP_DATA_NAME", this.AppDataName);
            dic.Add("@COL_CREATE_TIME", this.CreateTime);
            dic.Add("@COL_MODIFY_TIME", this.ModifyTime);
            dic.Add("@COL_AUTHOR", this.Author);
            dic.Add("@COL_RECYCLE_TIME", this.DedupTime);
            dic.Add("@COL_BLOB_INFO", this.DedupSourceFileId);
            dic.Add("@COL_POOL_GUID", this.DedupExtension);
            dic.Add("@COL_CONTENT_DATA_FILE_PREFIX_NUMBER", this.ContentDataFilePrefixNumber);
            dic.Add("@COL_SUB_RETENTION", this.SubRetention);
            dic.Add("@COL_PRUNE_TIME", this.PruneTime);
            return dic;
        }
    }
}