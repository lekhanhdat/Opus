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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Replicator.Object.ViewModels;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Replicator.Object.OperationResults
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MappingOperationResult : ReplicatorOperationResult
    {
        public MappingOperationResult()
            : base(false, null)
        {

        }
        public MappingOperationResult(bool hasError, ReplicatorOperationResultError exception)
            : base(hasError, exception)
        {

        }
        public static MappingOperationResult Empty = new MappingOperationResult();

        [DataMember]
        public ReplicatorMappingBase Mapping { get; set; }
      
        /// <summary>
        /// ReplcatorSummary 存储 srcUrl,DestUrl,mappingType,planName信息
        /// </summary>
        [DataMember]
        public List<ReplicatorMappingSummary> Mappings { get; set; }

        [DataMember]
        public bool IsValid { get; set; }

        [DataMember]
        public Dictionary<ProfileType, List<ProfileDto>> ProfilesByType { get; set; }

        [DataMember]
        public ContentDto Setting { get; set; }

        [DataMember]
        public Dictionary<string, List<ServiceDto>> AgentsByFarmId { get; set; }

        [DataMember]
        public ReplicatorMappingsStatusesWrapper MappingsStatuses { get; set; }

        [DataMember]
        public string PlanName { get; set; }
    }
}
