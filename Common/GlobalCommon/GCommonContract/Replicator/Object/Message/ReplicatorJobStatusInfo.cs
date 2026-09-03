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



namespace AvePoint.GCommon.Contract.Replicator.Object.Message
{
    #region using directives
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Job.Object;

    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorJobStatusInfo : JobStatusInfo
    {
        [DataMember]
        public MappingStatusDto MappingStatus { get; set; }

        public override JobStatusInfo Clone()
        {
            ReplicatorJobStatusInfo clone = new ReplicatorJobStatusInfo();

            clone.AgentHost = this.AgentHost;
            clone.Id = this.Id;
            clone.IsSubJob = this.IsSubJob;
            clone.Progress = this.Progress;
            clone.Stamp = this.Stamp;
            clone.State = this.State;
            clone.Type = this.Type;
            clone.Weight = this.Weight;

            if (this.ErrorInfos != null)
            {
                clone.ErrorInfos = new List<ErrorInfo>();

                foreach (ErrorInfo item in this.ErrorInfos)
                {
                    clone.ErrorInfos.Add(item.Clone());
                }
            }

            if (this.MappingStatus != null)
            {
                var mappingStatus = this.MappingStatus;
                clone.MappingStatus = new MappingStatusDto
                {
                    ConflictItemsCount = mappingStatus.ConflictItemsCount,
                    DestinationAgentName = mappingStatus.DestinationAgentName,
                    DestinationReceivedItemsCount = mappingStatus.DestinationReceivedItemsCount,
                    DestinationSentItemsCount = mappingStatus.DestinationSentItemsCount,
                    ErrorItemsCount = mappingStatus.ErrorItemsCount,
                    MappingNextRetryTime = mappingStatus.MappingNextRetryTime,
                    MappingRetriedCount = mappingStatus.MappingRetriedCount,
                    MappingStatus = mappingStatus.MappingStatus,
                    MappingTotalRetryCount = mappingStatus.MappingTotalRetryCount,
                    SourceAgentName = mappingStatus.SourceAgentName,
                    SourceReceivedItemsCount = mappingStatus.SourceReceivedItemsCount,
                    SourceSentItemsCount = mappingStatus.SourceSentItemsCount,
                    TransferredDataSizeInKiloByte = mappingStatus.TransferredDataSizeInKiloByte,
                    TotalSize = mappingStatus.TotalSize,
                };
            }

            return clone;
        }
    }
}
