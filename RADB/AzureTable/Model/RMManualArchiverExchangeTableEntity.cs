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
    public class RMManualArchiverExchangeTableEntity : ITableEntity
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ETag ETag { get; set; }

        public string Created { get; set; }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public Guid TermValue { get; set; }

        public string Title { get; set; }

        public long ArchivedTime { set; get; }

        public string FullPath { get; set; }

        public string MailBoxGroupID { get; set; }

        public string ModifiedBy { get; set; }

        public string Operator { get; set; }

        public string SendFrom { get; set; }

        public bool ExportToRECO { get; set; }

        public long LastModifiedTime { get; set; }

        public Guid RuleID { set; get; }

        public int ArchiveLevel { set; get; }

        public int CacheNodeType { set; get; }

        public int DeleteRelatedRecords { get; set; }

        public int HasRelatedDocument { get; set; }

        public int SourceFlag { set; get; }

        public int Status { set; get; }

        public string JsonMeta { set; get; }

        public string MailBoxID { get; set; }

        public string NodeID { set; get; }

        public string ParentID { set; get; }

        public string RelatedRecordInfo { get; set; }

        public string ScanJobID { set; get; }
    }
}
