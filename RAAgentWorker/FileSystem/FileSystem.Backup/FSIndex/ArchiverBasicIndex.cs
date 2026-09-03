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
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
    #endregion

    [Serializable]
    public class ArchiverBasicIndex
        : IndexBase
        , IIndexable
    {
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

        [Column("COL_ATTRIBUTES")]
        public string Attributes { get; set; }

        [Column("COL_EXTRAINFO")]
        public string ExtraInfo { get; set; }

        [Column("COL_PLANID")]
        public string PlanId { get; set; }

        /// <summary>
        /// For dedup file, use to save Source File's archiver(backup) Job Id
        /// </summary>

        [Column("COL_CYCLEID")]
        public string DedupSourceFileJobId { get; set; }

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
        public string RelatedPath { get; set; }
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

        [Column("COL_META_DATA_HEADER_OFFSET")]
        public long MetaDataHeaderOffset { get { return this.CurrentItemMetaDataDataHeaderStartOffset; } set { this.CurrentItemMetaDataDataHeaderStartOffset = value; } }

        [Column("COL_CONTENT_DATA_HEADER_OFFSET")]
        public long ContentDataHeaderOffset { get { return this.CurrentItemContentDataDataHeaderStartOffset; } set { this.CurrentItemContentDataDataHeaderStartOffset = value; } }

        [Column("COL_CONTENT_PAGE_SIZE")]
        public long ContentPageSize { get { return this.CurrentItemPageSize; } set { this.CurrentItemPageSize = value; } }


        [Column("COL_SITE_PATH")]
        public string SitePath { get; set; }


        [Column("COL_META_TAIL_LENGTH")]
        public long SoftDeleteTime { get; set; }

        [Column("COL_RETENTION_STATUS")]
        public int RetentionStatus { get; set; }

        [Column("COL_RETENTION")]
        public string Retention { get; set; }


        [Column("COL_RECYCLE_TIME")]
        public long DedupTime { get; set; }

        /// <summary>
        /// Use to save Deuplicate file's source file index id
        /// </summary>
        [Column("COL_BLOB_INFO")]
        public String DedupSourceFileId { get; set; }

        /// <summary>
        /// save the duplicate file data's file number
        /// </summary>
        [Column("COL_CONTENT_DATA_FILE_PREFIX_NUMBER")]
        public long DuplicateFileNumber { get; set; }


        /// <summary>
        /// save the duplicate file data's content item data mode
        /// </summary>
        [Column("COL_PRUNE_TIME")]
        public long DedupSourceFileFlag { get; set; }

        //public bool IsDeduplicateData
        //{
        //    get
        //    {
        //        return this.DuplicateStatus == (int)IndexDeduplicateFileStatus.DuplicateFile;
        //    }
        //}

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
            dic.Add("@COL_PLANID", this.PlanId);
            dic.Add("@COL_CYCLEID", this.DedupSourceFileJobId);
            dic.Add("@COL_JOBID", this.JobId);
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
            dic.Add("@COL_EXTENSION_8", this.RelatedPath);
            dic.Add("@COL_EXTENSION_9", this.Editor);
            dic.Add("@COL_EXTENSION_10", this.ListBaseType);
            dic.Add("@COL_CONTENT_DATA_OFFSET", this.ContentDataOffset);
            dic.Add("@COL_CONTENT_DATA_FILE_NUMBER", this.ContentDataFileNumber);
            dic.Add("@COL_STORAGEINFO", this.StorageInfo);
            dic.Add("@COL_META_DATA_HEADER_OFFSET", this.MetaDataHeaderOffset);
            dic.Add("@COL_CONTENT_DATA_HEADER_OFFSET", this.ContentDataHeaderOffset);
            dic.Add("@COL_CONTENT_PAGE_SIZE", this.ContentPageSize);
            dic.Add("@COL_PLATFORM_TYPE", this.PlatformType);
            dic.Add("@COL_SITE_PATH", this.SitePath);
            dic.Add("@COL_META_TAIL_LENGTH", this.SoftDeleteTime);
            dic.Add("@COL_RETENTION_STATUS", this.RetentionStatus);
            dic.Add("@COL_RETENTION", this.Retention);
            dic.Add("@COL_CREATE_TIME", this.CreateTime);
            dic.Add("@COL_MODIFY_TIME", this.ModifyTime);
            dic.Add("@COL_RECYCLE_TIME", this.DedupTime);
            dic.Add("@COL_BLOB_INFO", this.DedupSourceFileId);
            dic.Add("@COL_CONTENT_DATA_FILE_PREFIX_NUMBER", this.DuplicateFileNumber);
            dic.Add("@COL_PRUNE_TIME", this.DedupSourceFileFlag);
            return dic;
        }
    }
}