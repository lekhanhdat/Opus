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
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Utility.StorageTable.Entry
{
    public class ReplicatorReportTableEntity : TableEntity
    {
        public long PId { get; set; }
        public string ID { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string SourceAgent { get; set; }
        public string DestAgent { get; set; }
        public long RecordTime { get; set; }
        public string RecordName { get; set; }
        public string PlanId { get; set; }
        public string PlanName { get; set; }
        public string JobId { get; set; }
        public string MappingId { get; set; }
        public int JobMode { get; set; }
        public string ReplicatedVersion { get; set; }
        public int Status { get; set; }
        public string Comments { get; set; }
        public int TriggerEvent { get; set; }
        public string Property { get; set; }
        public int SharePointLevel { get; set; }
        public string DeletionDetail { get; set; }
        public string SrcDeletionOperator { get; set; }
        public long Size { get; set; }
        public int Action { get; set; }
        public int ReplicationMode { get; set; }
        public string PublishingCondition { get; set; }
    }

    public abstract class RPConstants
    {
        public const string ROOT_NAME = "ReplicationDetailsReport";
        public const string JOB_TABLE_NAME = "ReplicatorJobReportTable";
        public const string AUTO_TRIGGER_TABLE_NAME = "ReplicatorAutoTriggerTable";
        public const string COLUMN_PID = "PId";
        public const string COLUMN_MAPPINGID = "MappingId";
        public const string COLUMN_RECORDNAME = "RecordName";
        public const string COLUMN_RECORDTIME = "RecordTime";
        public const string COLUMN_SOURCEPATH = "Source";
        public const string COLUMN_DESTPATH = "Destination";
        public const string COLUMN_SHAREPOINTLEVEL = "SharePointLevel";
        public const string COLUMN_PLANID = "PlanId";
        public const string COLUMN_PLANNAME = "PlanName";
        public const string COLUMN_JOBMODE = "JobMode";
        public const string COLUMN_STATUS = "Status";
        public const string COLUMN_ID = "ID";
        public const string COLUMN_JOBID = "JobId";
        public const string COLUMN_ISREALTIME = "IsRealTime";
        public const string COLUMN_RPVERSION = "ReplicatedVersion";
        public const string COLUMN_COMMENTS = "Comments";
        public const string COLUMN_TRIGGEREDEVENT = "TriggerEvent";
        public const string COLUMN_PROPERTY = "Property";
        public const string COLUMN_DELETEDETAIL = "DeletionDetail";
        public const string COLUMN_SIZE = "Size";
        public const string COLUMN_SOURCEAGENT = "SourceAgent";
        public const string COLUMN_DESTAGENT = "DestAgent";
        public const string COLUMN_ACTION = "Action";
        public const string COLUMN_DELETEOPERATOR = "SrcDeletionOperator";
        public const string COLUMN_REPLICATIONMODE = "ReplicationMode";
        public const string COLUMN_PUBLISHRULECONDITION = "PublishingCondition";
    }
}
