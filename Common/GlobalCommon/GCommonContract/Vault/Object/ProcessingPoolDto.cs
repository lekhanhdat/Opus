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

namespace AvePoint.GCommon.Contract.Vault.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ProcessingPoolDto
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string FarmId { get; set; }
        [DataMember]
        public string AgentGroupId { get; set; }
        [DataMember]
        public int Numeber { get; set; }
        [DataMember]
        public string FarmName { get; set; }
        [DataMember]
        public string FarmDisplayName { get; set; }
        [DataMember]
        public ProcessingPoolLevel PoolLevel { get; set; }
        [DataMember]
        public ModelType ModelType { set; get; }
    }

    /// <summary>
    /// Processing Pool级别
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ProcessingPoolLevel
    {
        [EnumMember]
        User = 0,
        [EnumMember]
        Normal = 1,
        [EnumMember]
        High = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ModelType
    {
        [EnumMember]
        Normal,
        [EnumMember]
        Vault
    }
}
