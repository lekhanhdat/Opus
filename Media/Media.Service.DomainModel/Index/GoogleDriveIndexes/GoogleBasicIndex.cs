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
    using AvePoint.GCommon.Contract.Media.Object;
    using DocumentFormat.OpenXml.Wordprocessing;
    using RACommon.SQLiteDatabase;
    #region using directives
    using System;
    using System.Collections.Generic;
    #endregion

    [Serializable]
    public class GoogleBasicIndex
        : IndexBase,
        IInsertable
    {
        [Column("COL_ID")]
        public string Id { get; set; }

        [Column("COL_FLAG")]
        public long Flag
        {
            get { return CurrentItemDataMode; }
            set { CurrentItemDataMode = value; }
        }

        [Column("COL_TYPE")]
        public int Type { get; set; }

        [Column("COL_PATH")]
        public string Path { get; set; }

        [Column("COL_ITEMID")]
        public string ItemId { get; set; }

        [Column("COL_NAME")]
        public string Name { get; set; }

        [Column("COL_PLAN_ID")]
        public string PlanId { get; set; }

        [Column("COL_JOB_ID")]
        public string JobId { get { return base.BackupJobId; } set { base.BackupJobId = value; } }

        [Column("COL_CYCLE_ID")]
        public string CycleId { get; set; }

        [Column("COL_JOB_TYPE")]
        public string JobType { get; set; }

        [Column("COL_DATA_FILE_LENGTH")]
        public long DataFileLength { get { return CurrentItemMetaDataAndContentDataTotalLength; } set { CurrentItemMetaDataAndContentDataTotalLength = value; } }

        [Column("COL_PATH_MD5")]
        public string PathMD5 { get; set; }

        [Column("COL_PARENT_PATH_MD5")]
        public string ParentPathMD5 { get; set; }

        [Column("COL_DATA_FILE_NUMBER")]
        public long DataFileNumber { get { return CurrentItemMetaDataStartFileNumber; } set { CurrentItemMetaDataStartFileNumber = value; } }

        [Column("COL_DATA_FILE_OFFSET")]
        public long DataFileOffset { get { return CurrentItemMetaDataStartOffset; } set { CurrentItemMetaDataStartOffset = value; } }

        [Column("COL_DATA_FILE_PREFIX_NUMBER")]
        public long DataFilePrefixNumber { get { return CurrentItemMetaDataFilePrefixNumber; } set { CurrentItemMetaDataFilePrefixNumber = value; } }

        [Column("COL_CRC")]
        public long Crc { get; set; }

        [Column("COL_BACKUP_TYPE")]
        public int BackupType { get; set; }

        [Column("COL_ARCHIVE_TIME")]
        public long ArchiveTime { get; set; }

        [Column("COL_CREATE_TIME")]
        public long CreateTime { get; set; }
        [Column("COL_MODIFY_TIME")]
        public long ModifyTime { get; set; }
        [Column("COL_CREATED_BY")]
        public String CreatedBy { get; set; }

        [Column("COL_ATTRIBUTES")]
        public string Attributes { get; set; }

        [Column("COL_SEQUENCE")]
        public long Sequence { get; set; }

        [Column("COL_CONTENT_DATA_OFFSET")]
        public long ContentDataOffset { get { return CurrentItemContentDataStartOffset; } set { CurrentItemContentDataStartOffset = value; } }

        [Column("COL_CONTENT_DATA_FILE_NUMBER")]
        public long ContentDataFileNumber { get { return CurrentItemContentDataStartFileNumber; } set { CurrentItemContentDataStartFileNumber = value; } }

        [Column("COL_CONTENT_DATA_FILE_PREFIX_NUMBER")]
        public long ContentDataFilePrefixNumber { get { return CurrentItemContentDataFilePrefixNumber; } set { CurrentItemContentDataFilePrefixNumber = value; } }

        [Column("COL_META_DATA_HEADER_OFFSET")]
        public long MetaDataHeaderOffset { get { return CurrentItemMetaDataDataHeaderStartOffset; } set { CurrentItemMetaDataDataHeaderStartOffset = value; } }

        [Column("COL_CONTENT_DATA_HEADER_OFFSET")]
        public long ContentDataHeaderOffset { get { return CurrentItemContentDataDataHeaderStartOffset; } set { CurrentItemContentDataDataHeaderStartOffset = value; } }

        [Column("COL_CONTENT_PAGE_SIZE")]
        public long ContentPageSize { get { return CurrentItemPageSize; } set { CurrentItemPageSize = value; } }

        [Column("COL_CONTENT_OFFSET")]
        public long ContentOffset { get { return CurrentItemMetaDataInnerOffset; } set { CurrentItemMetaDataInnerOffset = value; } }

        [Column("COL_CONTENT_LENGTH")]
        public long ContentLength { get { return CurrentItemContentDataTotalLength; } set { CurrentItemContentDataTotalLength = value; } }

        [Column("COL_STORAGE_CRC32")]
        public string StorageCrc32 { get { return CurrentItemStorageCrc; } set { CurrentItemStorageCrc = value; } }

        [Column("COL_PLATFORM_TYPE")]
        public int PlatFormType { get; set; }

        [Column("COL_VERSION")]
        public long Version { get { return CurrentItemVersion; } set { CurrentItemVersion = value; } }

        [Column("COL_STORAGEPOLICYID")]
        public String StoragePolicyId { get; set; }

        [Column("COL_STORAG_ACCESSTIERTYPE")]
        public int StorageAccessTierType { get; set; }

        [Column("COL_STORAGEINFO")]
        public string StorageInfo { get { return base.StorageInformation; } set { base.StorageInformation = value; } }

        [Column("COL_NODE_TYPE")]
        public int NodeType { get; set; }

        [Column("COL_RETENTION_STATUS")]
        public int RetentionStatus { get; set; }

        [Column("COL_RETENTION")]
        public string Retention { get; set; }
        
        
        [Column("COL_SOFT_DELETE_TIME")]
        public long SoftDeleteTime { get; set; }

        [Column("COL_VERSION_NUMBER")]
        public string VersionNumber { get; set; }

        [Column("COL_DRIVE_ID")]
        public string DriveId { get; set; }

        [Column("COL_DRIVE_NAME")]
        public string DriveName { get; set; }

        [Column("COL_PARENT_ID")]
        public string ParentId { get; set; }

        //[Column("COL_MEMBER_EMAIL")]
        //public string MemberEmail { get; set; }

        [Column("COL_EXTENSION_1")]
        public int EXTENSION1 { get; set; }

        [Column("COL_EXTENSION_2")]
        public long EXTENSION2 { get; set; }

        [Column("COL_EXTENSION_3")]
        public string EXTENSION3 { get; set; }
        [Column("COL_EXTENSION_4")]
        public string EXTENSION4 { get; set; }

       
        public int ItemMajorVersion
        {
            get
            {
                int majorVersion = int.MaxValue;
                if (this.Type.Equals((int)GDriveDataType.FileVersion))
                {
                    //int flag = this.Name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
                    //if (flag >= 0)
                    //{
                    //    string versionStr = this.Name.Substring(flag + 1);
                    //    String[] version = versionStr.Split('.');
                    //    Int32.TryParse(version[0], out majorVersion);
                    //}
                    return (int)float.Parse(this.VersionNumber);
                }
                return majorVersion;
            }
        }

        public override Dictionary<string, object> GenerateInsertDatabaseParameters()
        {
            var result = new Dictionary<string, object>();
            result.Add("@COL_ID", Id);
            result.Add("@COL_FLAG", Flag);
            result.Add("@COL_TYPE", Type);
            result.Add("@COL_PATH", Path);
            result.Add("@COL_ITEMID", ItemId);
            result.Add("@COL_NAME", Name);
            result.Add("@COL_PLAN_ID", PlanId);
            result.Add("@COL_JOB_ID", JobId);
            result.Add("@COL_CYCLE_ID", CycleId);
            result.Add("@COL_JOB_TYPE", JobType);
            result.Add("@COL_PATH_MD5", PathMD5);
            result.Add("@COL_PARENT_PATH_MD5", ParentPathMD5);
            result.Add("@COL_DATA_FILE_NUMBER", DataFileNumber);
            result.Add("@COL_DATA_FILE_OFFSET", DataFileOffset);
            result.Add("@COL_DATA_FILE_LENGTH", DataFileLength);
            result.Add("@COL_DATA_FILE_PREFIX_NUMBER", DataFilePrefixNumber);
            result.Add("@COL_CRC", Crc);
            result.Add("@COL_BACKUP_TYPE", BackupType);
            result.Add("@COL_ARCHIVE_TIME", ArchiveTime);
            result.Add("@COL_ATTRIBUTES", Attributes);
            result.Add("@COL_SEQUENCE", Sequence);
            result.Add("@COL_CONTENT_DATA_OFFSET", ContentDataOffset);
            result.Add("@COL_CONTENT_DATA_FILE_NUMBER", ContentDataFileNumber);
            result.Add("@COL_CONTENT_DATA_FILE_PREFIX_NUMBER", ContentDataFilePrefixNumber);
            result.Add("@COL_STORAGEINFO", StorageInfo);
            result.Add("@COL_PLATFORM_TYPE", PlatFormType);
            result.Add("@COL_VERSION", Version);
            result.Add("@COL_META_DATA_HEADER_OFFSET", MetaDataHeaderOffset);
            result.Add("@COL_CONTENT_DATA_HEADER_OFFSET", ContentDataHeaderOffset);
            result.Add("@COL_CONTENT_PAGE_SIZE", ContentPageSize);
            result.Add("@COL_STORAGE_CRC32", StorageCrc32);
            result.Add("@COL_CONTENT_OFFSET", ContentOffset);
            result.Add("@COL_CONTENT_LENGTH", ContentLength);
            result.Add("@COL_NODE_TYPE", NodeType);
            result.Add("@COL_VERSION_NUMBER", VersionNumber);
            result.Add("@COL_DRIVE_ID", DriveId);
            result.Add("@COL_DRIVE_NAME", DriveName);
            result.Add("@COL_PARENT_ID", ParentId);
            result.Add("@COL_CREATE_TIME", CreateTime);
            result.Add("@COL_MODIFY_TIME", ModifyTime);
            result.Add("@COL_CREATED_BY", CreatedBy);
            result.Add("@COL_RETENTION_STATUS", RetentionStatus);
            result.Add("@COL_RETENTION", Retention);
            result.Add("@COL_SOFT_DELETE_TIME", SoftDeleteTime);
            result.Add("@COL_EXTENSION_4", EXTENSION4);
            result.Add("@COL_EXTENSION_1", EXTENSION1);
            result.Add("@COL_EXTENSION_2", EXTENSION2);
            result.Add("@COL_EXTENSION_3", EXTENSION3);
            result.Add("@COL_STORAGEPOLICYID", StoragePolicyId);
            result.Add("@COL_STORAG_ACCESSTIERTYPE", StorageAccessTierType);
            return result;
        }

        public override string ToString()
        {
            return string.Format("Id:{0}, flag: {1}, type: {2}, name: {3}, plan id: {4}, job id: {5}, cycle id: {6}, job type: {7}, PlatFormType: {8}, FileVersion: {9}.",
                Id,
                Flag,
                Type,
                Name,
                PlanId,
                JobId,
                CycleId,
                JobType,
                PlatFormType,
                VersionNumber);
        }
    }
}