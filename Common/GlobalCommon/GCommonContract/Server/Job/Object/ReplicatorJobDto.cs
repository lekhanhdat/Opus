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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Replicator.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Server.Job.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorJobDto : BaseJobDto
    {
        [DataMember]
        public ReplicatorRunLevel ReplicatorRunLevel { get; set; }

        [DataMember]
        public ReplicatorRunOption ReplicatorRunOption { get; set; }

        [DataMember]
        public bool IsReplicatorModify { get; set; }

        [DataMember]
        public bool IsReplicatorDeletion { get; set; }

        [DataMember]
        public bool IsUsingSpecialTime { get; set; }

        [DataMember]
        public int SpecialTimeNumber { get; set; }

        [DataMember]
        public TimeUnit SpecialTimeUnit { get; set; }

        [DataMember]
        public ReplicatorJobClob1 BackupBeforeInfo { get; set; }

        [DataMember]
        public string BackupJobId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorSubJobDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string MappingId { get; set; }

        [DataMember]
        public string ParentId { get; set; }

        [DataMember]
        public double Progress { get; set; }

        [DataMember]
        public int State { get; set; }

        [DataMember]
        public double Weight { get; set; }

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public string SourcePath { get; set; }

        [DataMember]
        public string DestPath { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime FinishTime { get; set; }

        [DataMember]
        public string SourceFarmName { get; set; }

        [DataMember]
        public string DestFarmName { get; set; }

        [DataMember]
        public ReplicatorDirection Direction { get; set; }
    }

    [DataContract]
    public sealed class ReplicatorBackupBeforeItem
    {
        [DataMember]
        public string MappingId { get; set; }

        [DataMember]
        public string SrcBackupJobId { get; set; }

        [DataMember]
        public string DestBackupJobId { get; set; }
    }

    [DataContract]
    public sealed class ReplicatorJobClob1
    {
        [DataMember]
        public List<ReplicatorBackupBeforeItem> Items { get; set; }

        [DataMember]
        public List<string> FinishedDependentJobIds { get; set; }
    }
}
