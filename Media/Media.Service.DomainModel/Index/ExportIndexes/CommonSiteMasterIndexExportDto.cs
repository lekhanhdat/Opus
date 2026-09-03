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
    [Table("CommonSiteMasterIndex")]
    public class CommonSiteMasterIndexExportDto : IIndexable
    {
        [Column("Id")]
        public string Id { get; set; }

        [Column("ArchiverTime")]
        public long ArchiverTime { get; set; }

        [Column("JobId")]
        public string JobId { get; set; }

        [Column("SiteURL")]
        public string SiteURL { get; set; }

        [Column("StorageId")]
        public string StorageId { get; set; }

        [Column("IndexStorageId")]
        public string IndexStorageId { get; set; }

        [Column("SiteGroupId")]
        public string SiteGroupId { get; set; }

        [Column("TeamId")]
        public string TeamId { get; set; }

        [Column("SiteId")]
        public string SiteId { get; set; }

        [Column("SPVersion")]
        public int SPVersion { get; set; }

        [Column("MergeIndexState")]
        public int MergeIndexState { get; set; }

        [Column("JobState")]
        public int JobState { get; set; }

        [Column("StorageInfo")]
        public string StorageInfo { get; set; }

        [Column("Extension")]
        public string Extension { get; set; }
        [Column("Flag")]
        public int Flag { get; set; }

        [Column("DAOMigrated")]
        public bool? DAOMigrated { get; set; }

        [Column("BackupFileType")]
        public int BackupFileType { get; set; }

        [Column("DuplicateStatus")]
        public int DuplicateStatus { get; set; }

        [Column("DataType")]
        public int DataType { get; set; }

        [Column("O365TenantId")]
        public string? O365TenantId { get; set; }

        public Dictionary<string, object> GenerateInsertDatabaseParameters()
        {
            var parameters = new Dictionary<string, object>
            {
                { "Id", Id },
                { "ArchiverTime", ArchiverTime },
                { "JobId", JobId },
                { "SiteURL", SiteURL },
                { "StorageId", StorageId },
                { "IndexStorageId", IndexStorageId },
                { "SiteGroupId", SiteGroupId },
                { "TeamId", TeamId },
                { "SiteId", SiteId },
                { "SPVersion", SPVersion },
                { "MergeIndexState", MergeIndexState },
                { "JobState", JobState },
                { "StorageInfo", StorageInfo },
                { "Extension", Extension },
                { "Flag", Flag },
                { "DAOMigrated", DAOMigrated },
                { "BackupFileType", BackupFileType },
                { "DuplicateStatus", DuplicateStatus },
                { "DataType", DataType },
                { "O365TenantId", O365TenantId }
            };
            return parameters;
        }
    }
}
