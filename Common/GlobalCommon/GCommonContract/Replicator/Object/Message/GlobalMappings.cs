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





using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
using AvePoint.GCommon.Contract.Server.ControlPanel.LanguageMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.DomainMapping;
using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.UserMapping;
using AvePoint.GCommon.Contract.Server.ControlPanel.ColumnMapping.Object;

namespace AvePoint.GCommon.Contract.Replicator.Object.Message
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("GlobalMappings")]
    public class GlobalMappings
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [XmlElement("Mapping")]
        public List<Mapping> MappingList { get; set; }

        [DataMember]
        public Dictionary<string, List<Mapping>> ImplicitMappings { get; set; }

        [DataMember]
        public List<ServiceGroupDto> AgentGroupList { get; set; }

        [DataMember]
        [XmlArray("FarmSettingList")]
        [XmlArrayItem("FarmSetting")]
        public List<FarmSetting> FarmSettingList { get; set; }

        [DataMember]
        public List<LanguageMappingDto> LanguageMappingList { get; set; }

        [Obsolete("Use UserMappingList and DomainMappingList instead of this one.")]
        [DataMember]
        public List<UserAndDomainMappingDto> UserAndDomainMappingList { get; set; }
        [DataMember]
        public List<ColumnMappingDataContract> ColumnMappingList { get; set; }

        [DataMember]
        public List<FilterPolicyWrapper> FilterPolicyList { get; set; }

        [DataMember]
        public List<UserMappingDataContract> UserMappingList { get; set; }

        [DataMember]
        public List<DomainMappingDataContract> DomainMappingList { get; set; }

        [DataMember]
        public List<DataEncryptionProfile> EncryptionList { get; set; }
    }
}
