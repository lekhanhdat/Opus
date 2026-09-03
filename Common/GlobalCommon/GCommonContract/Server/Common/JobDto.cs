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





using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public long StartTime { get; set; }

        [DataMember]
        public long FinishTime { get; set; }

        [DataMember]
        public int Progress { get; set; }

        [DataMember]
        public int State { get; set; }

        [DataMember]
        public string ParentId { get; set; }

        [DataMember]
        public long UpdateTime { get; set; }

        [DataMember]
        public int ControlState { get; set; }

        [DataMember]
        public int Category { get; set; }

        [DataMember]
        public int PlanType { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string PlanName { get; set; }

        [DataMember]
        public string LogicalDriveName { get; set; }

        [DataMember]
        public string MediaName { get; set; }

        [DataMember]
        public string SrcAgentName { get; set; }

        [DataMember]
        public string DestAgentName { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember]
        public string Detail { get; set; }

        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public int PlanLevel { get; set; }

        [DataMember] 
        public string ModuleName { get; set; }

        [DataMember]
        public int EndType { get; set; }

        [DataMember]
        public int IndexStatus { get; set; }
    }
}
