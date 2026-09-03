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
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Connector.Object.Settings
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MappingSetting
    {
        [DataMember]
        public MappingType MappingType { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Id { get; set; }
       
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MappingType
    {
        [EnumMember]
        CommonSetting = ProfileType.ConnectorCommonSetting,

        [EnumMember]
        PropertiesSetting = ProfileType.ConnectorPropertiesSetting,

        [EnumMember]
        SecuritySetting = ProfileType.ConnectorSecuritySetting,

        [EnumMember]
        SPPermissionSetting = ProfileType.ConnectorSPPermission,

        [EnumMember]
        ProcessingPool,
    }

}
