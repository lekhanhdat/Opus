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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.AzureTable.Model
{
    public class RMManualArchiverSharePointOnlineTableEntity : ITableEntity
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ETag ETag { get; set; }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public int ArchiveLevel { set; get; } //int,

        public int CacheNodeType { set; get; } //int,

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

        public int HasRelatedDocument { get; set; }

        public int DeleteRelatedRecords { get; set; }

        public string RelatedRecordInfo { get; set; }

        public Guid SiteGroupId { set; get; }
    }
}
