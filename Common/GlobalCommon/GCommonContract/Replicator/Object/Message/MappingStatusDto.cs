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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Replicator.Object.Message
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MappingStatusDto
    {
        #region Data from Agent
        [DataMember]
        public string SourceAgentName { get; set; }

        [DataMember]
        public double TotalSize { get; set; }

        [DataMember]
        public string DestinationAgentName { get; set; }

        [DataMember]
        public long SourceSentItemsCount { get; set; }

        [DataMember]
        public long DestinationReceivedItemsCount { get; set; }

        [DataMember]
        public long SourceReceivedItemsCount { get; set; }

        [DataMember]
        public long DestinationSentItemsCount { get; set; }

        [DataMember]
        public double TransferredDataSizeInKiloByte { get; set; }

        [DataMember]
        public long ConflictItemsCount { get; set; }

        [DataMember]
        public long ErrorItemsCount { get; set; }

        [DataMember]
        public int MappingRetriedCount { get; set; }

        [DataMember]
        public int MappingTotalRetryCount { get; set; }

        [DataMember]
        public DateTime MappingNextRetryTime { get; set; }

        [DataMember]
        public MappingJobStatus MappingStatus { get; set; }
        #endregion
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MappingJobStatus
    {
        [EnumMember]
        Undefined,
        [EnumMember]
        Waiting,
        [EnumMember]
        Running,
        [EnumMember]
        Retrying,
        [EnumMember]
        Retried,
        [EnumMember]
        Pausing,
        [EnumMember]
        Paused,
        [EnumMember]
        Resuming,
        [EnumMember]
        Resumed,
        [EnumMember]
        Completed,
        [EnumMember]
        Finished,
        [EnumMember]
        FinishedWithException,
        [EnumMember]
        Failed,
        [EnumMember]
        Stopped,
        [EnumMember]
        Stopping,
        [EnumMember]
        NotStarted,
    }
}
