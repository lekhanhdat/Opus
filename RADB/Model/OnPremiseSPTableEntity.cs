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
    public class OnPremiseSPTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Timestamp { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ETag ETag { get; set; }

        public int ArchiveLevel { set; get; } 
        public int CacheNodeType { set; get; } 
        public Guid NodeID { set; get; }
        public Guid ParentID { set; get; } 
        public Guid RuleID { set; get; }
        public int RuleAction { get; set; }
        public string ScanJobID { set; get; } 
        public Guid ScopeID { set; get; }
        public Guid SiteId { get; set; }
        public Guid ListId { set; get; }
        public int Status { set; get; } 
        public bool MovedToApprovalTable { get; set; }
        public int UIVersion { set; get; } 
        public string JsonMeta { set; get; }
        public int SourceFlag { set; get; }
        public long SortTicks { set; get; }
        public int HasRelatedDocument { get; set; }
        public int DeleteRelatedRecords { get; set; }
        public string RelatedRecordInfo { get; set; }
        public DateTime ArchivedTime { set; get; }
    }

    public class OnPremiseArchiverSharePointDto : OnPremiseSPTableEntity
    {
        public int ArchiveLevel { set; get; }
        public int CacheNodeType { set; get; } 
        public long LastModifiedTime { set; get; }
        public string LeafName { set; get; } 
        public int Level { set; get; }
        public DateTime ExpireTime { set; get; } 
        public int LibRowID { set; get; } 
        public int NodeType { set; get; } 
        public string Path { set; get; } 
        public string Property { set; get; }
        public int SPNodeLevel { set; get; }
        public long ScanItemID { set; get; } 
        public DateTime ScanTime { set; get; }
        public string SiteUrl { set; get; }
        public string SiteId { set; get; }
        public string RegistedSiteId { set; get; }
        public Guid WebId { set; get; }
        public Guid ListId { set; get; }
        public string Metadata { set; get; }
        public DateTime ArchivedTime { set; get; }
        public Guid SiteGroupId { set; get; }
        public int KeepDataStatus { set; get; } 
        public Guid NodeID { set; get; } 
        public Guid ParentID { set; get; } 
        public Guid RuleID { set; get; } 
        public string ScanJobID { set; get; } 
        public Guid ScopeID { set; get; } 
        public int UIVersion { set; get; } 
        public string SiteTitle { set; get; }
    }
}
