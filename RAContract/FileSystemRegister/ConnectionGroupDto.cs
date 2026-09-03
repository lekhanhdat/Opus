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
using AvePoint.GCommon.Contract.Common;
using AvePoint.RA.Contract.Multi_Geo.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.FileSystemRegister
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConnectionGroupDto
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string LastModifiedTime { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public AccessConnectionType AccessConnectionType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ConnectionDto> FSConnections { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ConnectionDto> RemoveFSConnections { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<AgentDto> Agents { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DataCenterType DataCenterType { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string DCInternalName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string DCDisplayName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public MultiGeoOperation MultiGeoOperation { get; set; } = MultiGeoOperation.None;
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConnectionGroupPublic
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public AccessConnectionType AccessConnectionType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> ConnectionIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> ConnectionIdsToRemove { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<Guid> AssignedAgentIds { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DataCenterType DataCenterType { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string DCInternalName { get; set; }
    }
}
