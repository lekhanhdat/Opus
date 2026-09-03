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
using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;
using System;

namespace AvePoint.RA.DB.Model
{
    public class ArchiverTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ETag ETag { get; set; }

        public ArchiverTableEntity()
        {
        }

        public ArchiverTableEntity(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        #region Inherits from Archiver DB
        public int ArchiveLevel { set; get; } //int,
        public int CacheNodeType { set; get; } //int,
        #region old logic no use
        //public DateTime ExpireTime { set; get; } // Datetime,
        //public long LastModifiedTime { set; get; } // bigint,
        //public string LeafName { set; get; } //nvarchar(255),
        //public int Level { set; get; } //tinyint,
        //public int LibRowID { set; get; } //int,
        //public int NodeType { set; get; } //int,
        //public string Path { set; get; } //nvarchar(512),
        //public string Property { set; get; } // nvarchar(max))
        //public long ScanItemID { set; get; } //bigint IDENTITY(1, 1) primary key not null,
        //public DateTime ScanTime { set; get; } //Datetime,
        //public int SPNodeLevel { set; get; } //int,
        //public string SiteUrl { set; get; }
        //public Guid WebId { set; get; }
        //public Guid ListId { set; get; }
        #region for RECO
        //public Guid SiteGroupId { set; get; }

        //public string Metadata { set; get; }

        //public DateTime ArchivedTime { set; get; }

        //public bool ExportToRECO { get; set; }
        #endregion
        #endregion
        public Guid NodeID { set; get; } //uniqueidentifier not null,
        public Guid ParentID { set; get; } //uniqueidentifier not null,
        public Guid RuleID { set; get; } //nvarchar(128),
        public string ScanJobID { set; get; } //nvarchar(128),
        public Guid ScopeID { set; get; } //uniqueidentifier not null,
        public int Status { set; get; } //int,
        public bool ExportToRECO { get; set; }//RevIM Manual job需要此属性作为判断条件
        public int UIVersion { set; get; } //int not null,
        public string JsonMeta { set; get; }//该属性中保存json格式的数据源内容
        public int SourceFlag { set; get; }//int,标识数据源
        //public int RelatedRecordOption { set; get; }//int,Disposal Action 标识
        public int HasRelatedDocument { get; set; }
        public int DeleteRelatedRecords { get; set; }
        public string RelatedRecordInfo { get; set; }
        public Guid SiteGroupId { set; get; }
        #endregion
		public string SortTicks { set; get; }//use for ReadFrom db method
    }
    public class ArchiverSharePointDto : ArchiverTableEntity
    {
        #region use for sharepoint
        public int ArchiveLevel { set; get; } //int,
        public int CacheNodeType { set; get; } //int,
        public long LastModifiedTime { set; get; } // bigint,
        public string LeafName { set; get; } //nvarchar(255),
        public int Level { set; get; } //tinyint,
        public DateTime ExpireTime { set; get; } // Datetime,
        public int LibRowID { set; get; } //int,
        public Guid ListId { set; get; }
        public int NodeType { set; get; } //int,
        public string Path { set; get; } //nvarchar(512),
        public string Property { set; get; } // nvarchar(max))
        public int SPNodeLevel { set; get; } //int,
        public long ScanItemID { set; get; } //bigint IDENTITY(1, 1) primary key not null,
        public DateTime ScanTime { set; get; } //Datetime,
        public string SiteUrl { set; get; }
        public Guid SiteId { set; get; }
        public Guid RegistedSiteId { get; set; }
        public Guid WebId { set; get; }
        public string Metadata { set; get; }
        public DateTime ArchivedTime { set; get; }
        public int KeepDataStatus { set; get; } //int,
        public Guid NodeID { set; get; } //uniqueidentifier not null,
        public Guid ParentID { set; get; } //uniqueidentifier not null,
        public Guid RuleID { set; get; } //nvarchar(128),
        public string ScanJobID { set; get; } //nvarchar(128),
        public Guid ScopeID { set; get; } //uniqueidentifier not null,
        public int UIVersion { set; get; } //int not null,
        public string SiteTitle { get; set; }
        public string OnedriveTermName { set; get; }
        public long CreatedTime { get; set; }

        public string FileType { get; set; }
        public long CDLastModifiedTime { get; set; }
        public string RecordsId { get; set; }
        #endregion
    }
}
