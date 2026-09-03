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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.Server.Common.TimeZone;

namespace AvePoint.Adonis.Replicator.Contract.Settings
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorDetailQueryCondition
    {
        //[DataMember]
        //public int Skip { get; set; }

        //[DataMember]
        //public int Take { get; set; }

        [DataMember]
        public TimeRangeDto TimeRange { get; set; }

        [DataMember]
        public string Keyword { get; set; }

        //[DataMember]
        //public List<ReplicationDetailColumnType> FuzzyColumns { get; set; }

        [DataMember]
        public List<FilterColumnCondition> FilterCondiations { get; set; }

        [DataMember]
        public List<string> SelectMappings { get; set; }

        [DataMember]
        public string PlanId { get; set; }

        //[DataMember]
        //public ReplicationDetailColumnType SortColumn { get; set; }

        ///// <summary>
        ///// True : Asc ; False: Desc;
        ///// </summary>
        //[DataMember]
        //public bool IsAscSort { get; set; }


        #region Ignored to be deleted
        //[DataMember]
        //public bool IsUseAdvancedFilter { get; set; }

        //[DataMember]
        //public DateTime StartTime { get; set; }

        //[DataMember]
        //public DateTime EndTime { get; set; }

        //[DataMember]
        //public List<SharePointLevel> SharePointLevel { get; set; }

        //[DataMember]
        //public List<ReplicatorDashboardDetailStatus> Status { get; set; }

        #endregion
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TimeRangeDto
    {
        [DataMember]
        public long StartTime { get; set; }

        [DataMember]
        public long EndTime { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FilterColumnCondition
    {
        [DataMember]
        public ReplicationDetailColumnType Column { get; set; }

        [DataMember]
        public List<string> Values { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SharePointLevel : int
    {
        [EnumMember]
        WebApplication = 0,
        [EnumMember]
        SiteCollection = 1,
        [EnumMember]
        Site = 2,
        [EnumMember]
        DocumentLibrary = 3,
        [EnumMember]
        GenericList = 4,
        [EnumMember]
        Folder = 5,
        [EnumMember]
        Item = 6,
        [EnumMember]
        Document = 7,
        [EnumMember]
        Attachment = 8,
        [EnumMember]
        Unknown = 9,
        [EnumMember]
        User = 10,
        [EnumMember]
        Group = 11,
        [EnumMember]
        Permission = 12,
        [EnumMember]
        Column = 13,
        [EnumMember]
        PermissionLevel = 14,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicateAction : int
    {
        [EnumMember]
        NewCreated = 1,
        [EnumMember]
        Overwrite = 2,
        [EnumMember]
        Delete = 3,
        [EnumMember]
        Merge = 4,
        [EnumMember]
        Move = 5,
        [EnumMember]
        Skip = 6,
        [EnumMember]
        Unknown = 0
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicateJobMode : int
    {
        [EnumMember]
        Full = 0,
        [EnumMember]
        Incremental = 1,
        [EnumMember]
        RealTime = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicationDetailColumnType : int
    {
        [EnumMember]
        RecordName = 0,
        [EnumMember]
        SharepointLevel = 1,
        [EnumMember]
        Source = 2,
        [EnumMember]
        Destination = 3,
        [EnumMember]
        ReplicationTime = 4,
        [EnumMember]
        PlanName = 5,
        [EnumMember]
        ReplicatedVersion = 6,
        [EnumMember]
        Status = 7,
        [EnumMember]
        Comments = 8,
        [EnumMember]
        JobId = 9,
        [EnumMember]
        RealTime = 10,
        [EnumMember]
        JobMode = 11,
        [EnumMember]
        RealTimeEvent = 12,
        [EnumMember]
        Action = 13,
        [EnumMember]
        SrcAgent = 14,
        [EnumMember]
        DestAgent = 15,
        [EnumMember]
        Size = 16,
        [EnumMember]
        SrcDeletionOperator = 17,
        [EnumMember]
        BackupDetails = 18,
        [EnumMember]
        RestoreDetails = 19,
        [EnumMember]
        DeletionDetail = 20,
        [EnumMember]
        ReplicationMode = 21,
    }
}
