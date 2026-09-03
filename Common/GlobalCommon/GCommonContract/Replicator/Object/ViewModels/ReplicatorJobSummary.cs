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



namespace AvePoint.GCommon.Contract.Replicator.Object.ViewModels
{
    #region using directives
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorJobSummary
    {
        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public string Dependency { get; set; }

        [DataMember]
        public string SourceURL { get; set; }

        [DataMember]
        public string DestinationURL { get; set; }

        [DataMember]
        public string StartTime { get; set; }

        [DataMember]
        public string FinishTime { get; set; }

        [DataMember]
        public double Progress { get; set; }

        [DataMember]
        public int State { get; set; }

        [DataMember]
        public string PlanName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public ReplicatorRunLevel RunLevel { get; set; }

        [DataMember]
        public ReplicatorRunOption RunOption { get; set; }

        [DataMember]
        public ReplicatorDirection Direction { get; set; }

        [DataMember]
        public ReplicatorJobType JobType { get; set; }

        [DataMember]
        public bool CanRollback { get; set; }

        [DataMember]
        public string PlanModifiedBy { get; set; }

        [DataMember]
        public string JobOperatedBy { get; set; }

        [DataMember]
        public string SourceFarmName { get; set; }

        [DataMember]
        public string DestFarmName { get; set; }

        [DataMember]
        public string SourceAgentName { get; set; }

        [DataMember]
        public string DestAgentName { get; set; }

        [DataMember]
        public string ManagerVersion { get; set; }

        [DataMember]
        public string AgentVersion { get; set; }

        [DataMember]
        public string SubJobComment { get; set; }

        [DataMember]
        public string SucceededObjects { get; set; }

        [DataMember]
        public string FailedObjects { get; set; }

        [DataMember]
        public string SkippedObjects { get; set; }

        [DataMember]
        public string TotalSize { get; set; }

        [DataMember]
        public string TransferredSize { get; set; }
    }
}
