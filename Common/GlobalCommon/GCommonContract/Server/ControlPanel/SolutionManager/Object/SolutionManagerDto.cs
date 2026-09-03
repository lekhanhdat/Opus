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

namespace AvePoint.GCommon.Contract.Server.ControlPanel.SolutionManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SolutionManagerDto
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string FarmId { get; set; }
        [DataMember]
        public string ModuleName { get; set; }
        [DataMember]
        public string SolutionName { get; set; }
        [DataMember]
        public long LastRefreshedTime { get; set; }
        [DataMember]
        public string Version { get; set; }
        [DataMember]
        public string FileVersion { get; set; }
        [DataMember]
        public SolutionAction Action { get; set; }
        [DataMember]
        public SolutionStatus Status { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public SolutionInfo SolutionInfo { get; set; }
        [DataMember]
        public bool NeedResetIIS { get; set; }
        [DataMember]
        public bool NeedLinkMessage { get; set; }
        [DataMember]
        public List<WebAppInfoDto> WebAppInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SolutionInfo
    {
        [DataMember]
        public string SolutionName { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public bool WebApplicationResource { get; set; }
        [DataMember]
        public bool GlobalAssembly { get; set; }
        [DataMember]
        public bool CodeAccessSecurityPolicy { get; set; }
        [DataMember]
        public string DeploymentServerType { get; set; }
        [DataMember]
        public string DeploymentStatus { get; set; }
        [DataMember]
        public string LastOperationResult { get; set; }
        [DataMember]
        public string LastOperationTime { get; set; }
        [DataMember]
        public string LastOperationDetails { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SolutionStatus : int
    {
        [EnumMember]
        NA = 0,
        [EnumMember]
        NotDeployed = 1,
        [EnumMember]
        Deployed = 2,
        [EnumMember]
        PartiallyDeployed = 3,
        [EnumMember]
        Deploying = 11,
        [EnumMember]
        Upgrading = 12,
        [EnumMember]
        Removing = 13,
        [EnumMember]
        Retracting = 14,
        [EnumMember]
        Installing = 15,
        [EnumMember]
        Repairing = 16,
        [EnumMember]
        Refreshing = 17,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SolutionAction
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Deploy = 2,
        [EnumMember]
        Retract = 4,
        [EnumMember]
        Remove = 6,
        [EnumMember]
        Repair = 8,
        [EnumMember]
        Upgrade = 10,
        [EnumMember]
        Retrieve = 12,
        [EnumMember]
        Install = 14,
        [EnumMember]
        ResetIIS = 16,
    }


}
