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
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Text;

    #endregion using directives

    [Serializable]
    [Table(IndexConstants.TableNameGeneralItem)]
    public class GeneralItemIndex
        : IndexBase
        , IIndexable
    {
        [Column("COL_ID")]
        public String Id { get; set; }

        [Column("COL_FLAG")]
        public Int64 Flag { get { return this.CurrentItemDataMode; } set { this.CurrentItemDataMode = value; } }

        [Column("COL_TYPE")]
        public String Type { get; set; }

        [Column("COL_NAME")]
        public String Name { get; set; }

        [Column("COL_PLANID")]
        public String PlanId { get; set; }

        [Column("COL_JOBID")]
        public String JobId { get { return base.BackupJobId; } set { base.BackupJobId = value; } }

        [Column("COL_DATA_FILE_NUMBER")]
        public Int64 DataFileNumber { get { return this.CurrentItemMetaDataStartFileNumber; } set { this.CurrentItemMetaDataStartFileNumber = value; } }

        [Column("COL_DATA_FILE_OFFSET")]
        public Int64 DataFileOffset { get { return this.CurrentItemMetaDataStartOffset; } set { this.CurrentItemMetaDataStartOffset = value; } }

        [Column("COL_DATA_FILE_LENGTH")]
        public Int64 DataFileLength { get { return this.CurrentItemMetaDataAndContentDataTotalLength; } set { this.CurrentItemMetaDataAndContentDataTotalLength = value; } }

        [Column("COL_DATA_FILE_PREFIX_NUMBER")]
        public Int64 DataFilePrefixNumber { get { return this.CurrentItemMetaDataFilePrefixNumber; } set { CurrentItemMetaDataFilePrefixNumber = value; } }

        [Column("COL_BACKUP_TIME")]
        public Int64 BackupTime { get; set; }

        [Column("COL_CONTENT_OFFSET")]
        public Int64 ContentOffset { get { return this.CurrentItemMetaDataInnerOffset; } set { this.CurrentItemMetaDataInnerOffset = value; } }

        [Column("COL_CONTENT_LENGTH")]
        public Int64 ContentLength { get { return this.CurrentItemContentDataTotalLength; } set { this.CurrentItemContentDataTotalLength = value; } }

        [Column("COL_SEQUENCE")]
        public Int64 Sequence { get; set; }

        [Column("COL_IS_FAILED")]
        public String IsFailed { get; set; }

        [Column("COL_STORAGE_CRC32")]
        public Int64 StorageCrc32 { get; set; }

        [Column("COL_CONTENT_DATA_OFFSET")]
        public Int64 ContentDataOffset { get { return this.CurrentItemContentDataStartOffset; } set { this.CurrentItemContentDataStartOffset = value; } }

        [Column("COL_CONTENT_DATA_FILE_NUMBER")]
        public Int64 ContentDataFileNumber { get { return this.CurrentItemContentDataStartFileNumber; } set { this.CurrentItemContentDataStartFileNumber = value; } }

        [Column("COL_CONTENT_DATA_FILE_PREFIX_NUMBER")]
        public Int64 ContentDataFilePrefixNumber { get { return this.CurrentItemContentDataFilePrefixNumber; } set { this.CurrentItemContentDataFilePrefixNumber = value; } }

        [Column("COL_STORAGEINFO")]
        public String StorageInfo { get { return base.StorageInformation; } set { base.StorageInformation = value; } }

        [Column("COL_VERSION")]
        public Int64 Version { get { return this.CurrentItemVersion; } set { this.CurrentItemVersion = value; } }

        [Column("COL_META_DATA_HEADER_OFFSET")]
        public Int64 MetaDataHeaderOffset { get; set; }

        [Column("COL_CONTENT_DATA_HEADER_OFFSET")]
        public Int64 ContentDataHeaderOffset { get { return this.CurrentItemContentDataDataHeaderStartOffset; } set { this.CurrentItemContentDataDataHeaderStartOffset = value; } }

        [Column("COL_CONTENT_PAGE_SIZE")]
        public Int64 ContentPageSize { get { return this.CurrentItemPageSize; } set { this.CurrentItemPageSize = value; } }

        public override Dictionary<String, Object> GenerateInsertDatabaseParameters()
        {
            var result = new Dictionary<String, Object>();
            result.Add("@COL_ID", Id);
            result.Add("@COL_FLAG", Flag);
            result.Add("@COL_TYPE", Type);
            result.Add("@COL_NAME", Name);
            result.Add("@COL_PLANID", PlanId);
            result.Add("@COL_JOBID", JobId);
            result.Add("@COL_DATA_FILE_NUMBER", DataFileNumber);
            result.Add("@COL_DATA_FILE_OFFSET", DataFileOffset);
            result.Add("@COL_DATA_FILE_LENGTH", DataFileLength);
            result.Add("@COL_DATA_FILE_PREFIX_NUMBER", DataFilePrefixNumber);
            result.Add("@COL_BACKUP_TIME", BackupTime);
            result.Add("@COL_SEQUENCE", Sequence);
            result.Add("@COL_CONTENT_OFFSET", ContentOffset);
            result.Add("@COL_CONTENT_LENGTH", ContentLength);
            result.Add("@COL_IS_FAILED", IsFailed);
            result.Add("@COL_STORAGE_CRC32", StorageCrc32);
            result.Add("@COL_CONTENT_DATA_OFFSET", ContentDataOffset);
            result.Add("@COL_CONTENT_DATA_FILE_NUMBER", ContentDataFileNumber);
            result.Add("@COL_CONTENT_DATA_FILE_PREFIX_NUMBER", ContentDataFilePrefixNumber);
            result.Add("@COL_STORAGEINFO", StorageInfo);
            result.Add("@COL_VERSION", Version);
            result.Add("@COL_META_DATA_HEADER_OFFSET", MetaDataHeaderOffset);
            result.Add("@COL_CONTENT_DATA_HEADER_OFFSET", ContentDataHeaderOffset);
            result.Add("@COL_CONTENT_PAGE_SIZE", ContentPageSize);
            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("GeneralItemIndex: JobId:");
            sb.Append(this.JobId);
            sb.Append(" Name: ");
            sb.Append(this.Name);
            sb.Append(" PlanId: ");
            sb.Append(this.PlanId);
            return sb.ToString();
        }
    }
}