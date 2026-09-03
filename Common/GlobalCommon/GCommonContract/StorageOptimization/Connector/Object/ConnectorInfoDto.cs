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
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.StorageOptimization.Connector.Object.Settings;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Connector.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConnectorInfoDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string RuleId { get; set; }

        [DataMember]
        public string NodeId { get; set; }

        [DataMember]
        public string InheritedNodeId { get; set; }

        [DataMember]
        public NodeLevel NodeLevel { get; set; }

        [DataMember]
        public SPTreeNodeDto NodeInfo { get; set; }

        [DataMember]
        public List<MapStoragePathDto> MapStoragePathDtos { get; set; }

        [DataMember]
        public LibrarySetting LibrarySetting { get; set; }

        /*---*/
        [DataMember]
        public List<MappingSetting> MappingSettings { get; set; }

        [DataMember]
        public List<ScheduleDto> Schedules { get; set; }

        [DataMember]
        public string ProcessingPoolId { get; set; }

        [DataMember]
        public string FarmId { get; set; }

        [DataMember]
        public bool IsCurrentNodeHasSyncSettings { get; set; }
    }
}
