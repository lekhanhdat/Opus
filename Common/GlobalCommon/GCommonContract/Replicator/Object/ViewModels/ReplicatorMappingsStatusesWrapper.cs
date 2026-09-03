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



using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Replicator.Object.Message;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Replicator.Object.ViewModels
{
    [DataContract]
    public class ReplicatorMappingsStatusesWrapper
    {
        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public List<ReplicatorMappingStatus> MappingStatuses { get; set; }
    }

    [DataContract]
    public class ReplicatorMappingStatus
    {
        [DataMember]
        public string MappingId { get; set; }

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public string SubJobId { get; set; }

        [DataMember]
        public MappingStatusDto Detail { get; set; }
        [DataMember]
        public ServiceActive? SourceAgentConnectionStatus { get; set; }
        [DataMember]
        public ServiceActive? DestinationAgentConnectionStatus { get; set; }
        [DataMember]
        public ReplicatorDirection MappingDirection { get; set; }
        [DataMember]
        public string SourceUrl { get; set; }
        [DataMember]
        public string DestinationUrl { get; set; }
        [DataMember]
        public double MappingProgress { get; set; }
    }
}
