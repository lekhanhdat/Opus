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
using AvePoint.Hybrid.Contract.Object;
using AvePoint.RA.Contract.FileSystemRegister.JPMC;
using AvePoint.RA.Contract.Multi_Geo.Enum;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.FileSystemRegister
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConnectionDto
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid GroupId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string LastModifiedTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string UNCPath { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string AgentId { get; set; }//TODO

        [DataMember(EmitDefaultValue = false)]
        public string GroupName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [StringLength(256, ErrorMessage = "InternalId must be 256 characters or fewer.")]
        //[RegularExpression("^[A-Za-z0-9_. -]+$", ErrorMessage = "InternalId can contain only letters, numbers, spaces, periods, hyphens, and underscores.")]
        public string JPMCConnectionId { get; set; }


        [DataMember(EmitDefaultValue = false)]
        public List<ToUserInfo> RecordOwners { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ToUserInfo> InformationOwners { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string LastSyncTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? Monitor { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsEditConnectionPage { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public MultiGeoOperation MultiGeoOperation { get; set; } = MultiGeoOperation.None;

    }
    public class AgentDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public SourceType SourceType { get; set; }

        public ServiceStatus Status { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ConnectionResultData
    {
        [DataMember(EmitDefaultValue = false)]
        public List<ConnectionDto> ConnectionList { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int TotalCount { get; set; }
    }
    [DataContract]
    public class GetConnectionListParam
    {
        [DataMember]
        public int PageIndex { set; get; }
        [DataMember]
        public int PageSize { set; get; }
        [DataMember]
        public string SearchKey { set; get; }
        [DataMember]
        public List<FSConnectionFilter> Filters { get; set; }

        [DataMember]
        public FSConnectionOrder Order { get; set; }

        [DataMember]
        public List<Guid> ConnectionIds { get; set; }
    }
    [DataContract]
    public class ValidateConnectionParam
    {
        [DataMember]
        public List<Guid> ConnectionIds { get; set; }
        [DataMember]
        public List<Guid> AgentIds { get; set; }
        [DataMember]
        public AccessConnectionType AccessConnectionType { get; set; }
        [DataMember]
        public bool IsPublicApiRole { get; set; }
        [DataMember]
        public List<string> TargetDCs { get; set; }
    }

    [DataContract]
    public class FSConnectionFilter
    {
        [DataMember]
        public string ColumnName { get; set; }
        [DataMember]
        public List<string> ColumnValues { get; set; }
    }

    [DataContract]
    public class FSConnectionOrder
    {
        [DataMember]
        public string ColumnName { get; set; }

        [DataMember]
        public bool IsDesc { get; set; }
    }
}
