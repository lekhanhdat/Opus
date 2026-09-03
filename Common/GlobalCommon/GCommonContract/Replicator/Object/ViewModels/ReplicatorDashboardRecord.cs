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
using AvePoint.Adonis.Replicator.Contract.Settings;
using AvePoint.GCommon.Contract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.Replicator.Object.ViewModels
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorDashboardRecord
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string RecordName { get; set; }

        [DataMember]
        public string Source { get; set; }

        [DataMember]
        public string Destination { get; set; }

        [DataMember]
        public string RecordTime { get; set; }

        [DataMember]
        public string PlanName { get; set; }
        /// <summary>
        /// 在RealTime Monitor中为EventId
        /// </summary>
        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public ReplicateJobMode JobMode { get; set; }

        [DataMember]
        public string ReplicatedVersion { get; set; }

        [DataMember]
        public ReplicatorDashboardDetailStatus Status { get; set; }

        [DataMember]
        public string Comments { get; set; }

        [DataMember]
        public string TriggeredEvent { get; set; }

        [DataMember]
        public EventReceiverType RealTimeEvent { get; set; }

        [DataMember]
        public string Property { get; set; }

        [DataMember]
        public SharePointLevel SharePointLevel { get; set; }

        /// <summary>
        /// backup 成功的metadata
        /// </summary>
        [DataMember]
        public string BackupProperty { get; set; }
        /// <summary>
        /// restore 成功的metadata
        /// </summary>
        [DataMember]
        public string RestoreProperty { get; set; }

        [DataMember]
        public ReplicateAction Action { get; set; }

        [DataMember]
        public long Size { get; set; }

        [DataMember]
        public string SourceAgent { get; set; }

        [DataMember]
        public string DestAgent { get; set; }

        [DataMember]
        public string DeletionDetail { get; set; }

        [DataMember]
        public string DeleteOperator { get; set; }

        [DataMember]
        public string PublishCondition { get; set; }

        [DataMember]
        public bool IsPublishingMode { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicatorDashboardDetailStatus
    {
        [EnumMember]
        None = -1,

        [EnumMember]
        Finished = 0,

        [EnumMember]
        Failed = 1,

        [EnumMember]
        Skipped = 2,

        //[EnumMember]
        //Exception = 3,

        [EnumMember]
        Restoring = 4,

        [EnumMember]
        Transferring = 5,

        [EnumMember]
        BackingUp = 6,

        [EnumMember]
        Waiting = 7,

    }
}
