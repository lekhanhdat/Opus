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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.HostManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class VMCredentialProfileDto
    {
        /// <summary>
        /// Guid for dto
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// Enum of Host type eg: Hyper-v or VMWare
        /// </summary>
        [DataMember]
        public HostProfileType HostType { get; set; }

        /// <summary>
        /// The selected agent Dto.
        /// do not save to database
        /// </summary>
        [DataMember]
        public ServiceDto Agent { get; set; }

        /// <summary>
        /// The selected agent id
        /// </summary>
        [DataMember]
        public string AgentId { get; set; }

        /// <summary>
        /// Host Profile Name
        /// </summary>
        [DataMember]
        public string ProfileName { get; set; }

        /// <summary>
        /// Host Profile Description
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Host Address /IP
        /// </summary>
        [DataMember]
        public string HostAddress { get; set; }

        /// <summary>
        /// The user to connect host
        /// </summary>
        [DataMember]
        public string HostUserName { get; set; }

        /// <summary>
        /// The password of HostUserName   
        /// </summary>
        [DataMember]
        public string HostPassword { get; set; }

        /// <summary>
        /// HostName，可以是FQDN或者IPAddress.
        /// 可以作为Host唯一标识
        /// </summary>
        [DataMember]
        public string HostName { get; set; }

        /// <summary>
        /// Cluster of VM Host
        /// </summary>
        [DataMember]
        public VMClusterInfo VMCluster { get; set; }

        /// <summary>
        /// 用于区分是正常Create还是DataImport
        /// </summary>
        [DataMember]
        public CreateType CreateType { get; set; }

        /// <summary>
        /// This property is required for VMwareHost and VCenterHost.
        /// Protocol ports for VMware web service SDK.
        /// default value: https:443;http:80
        /// </summary>
        [DataMember]
        public string ProtocolPorts { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CreateType
    {
        [EnumMember]
        Create = 0,
        [EnumMember]
        DataImport = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum HostProfileType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        HyperVHost = 1,
        [EnumMember]
        VMwareHost = 2,
        [EnumMember]
        VCenterHost = 4,
        [EnumMember]
        HyperVCluster = 5,
    }
}
