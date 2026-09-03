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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Replicator.Object.ProfileContents;
using AvePoint.GCommon.Contract.Replicator.Object.Settings;
using AvePoint.GCommon.Contract.Server.Common.ExportLocation.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.Replicator.Object.Message
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Mapping
    {
        [DataMember]
        [XmlAttribute("id")]
        public string Id { get; set; }

        [DataMember]
        [XmlAttribute("enable")]
        public bool Enable { get; set; }

        [DataMember]
        [XmlAttribute("srcFarmId")]
        public Guid SrcFarmId { get; set; }

        [DataMember]
        [XmlAttribute("destFarmId")]
        public Guid DestFarmId { get; set; }

        [DataMember]
        [XmlAttribute("lastModifyTime")]
        public DateTime LastModifyTime { get; set; }

        [DataMember]
        [XmlAttribute("eventHandlerTypes")]
        public ReplicationEvent EventHandlerTypes { get; set; }

        [DataMember]
        [XmlAttribute("isEventHandlerEnable")]
        public bool IsEventHandlerEnable { get; set; }

        [DataMember]
        [XmlAttribute("isAutoIncludeNewObject")]
        public bool IsAutoIncludeNewObject { get; set; }

        [DataMember]
        [XmlAttribute("isIncludeAllSubSite")]
        public bool IsIncludeAllSubSite { get; set; }

        [DataMember]
        [XmlAttribute("type")]
        public int Type { get; set; }

        [DataMember]
        [XmlAttribute("sourcePath")]
        public string SourcePath { get; set; }

        [DataMember]
        [XmlAttribute("destPath")]
        public string DestPath { get; set; }

        [DataMember]
        [XmlAttribute("sourceFarmName")]
        public string SourceFarmName { get; set; }

        [DataMember]
        [XmlAttribute("destFarmName")]
        public string DestFarmName { get; set; }

        [DataMember]
        public SPTreeNodeDto SrcItems { get; set; }

        [DataMember]
        public SPTreeNodeDto DestItems { get; set; }

        [DataMember]
        [XmlElement("Setting")]
        public MappingSetting Setting { get; set; }

        [DataMember]
        public string SrcAgentGroupId { get; set; }

        [DataMember]
        public string DestAgentGroupId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MappingSetting
    {
        [DataMember]
        [XmlAttribute("isTwoWay")]
        public bool IsTwoWay { get; set; }

        [DataMember]
        public ReplicatorDirection Direction { get; set; }

        [DataMember]
        [XmlAttribute("isByteLevelDifferencing")]
        public bool IsByteLevelDifferencing { get; set; }

        [DataMember]
        public ReplicationOption ReplicationOption { get; set; }

        [DataMember]
        public ReplicatorNetworkControlContent NetworkControl { get; set; }

        [DataMember]
        public ReplicatorAdvancedSetting AdvancedSetting { get; set; }

        [DataMember]
        public ExportLocationDto Location { get; set; }

        [DataMember]
        public string LanguageMappingId { get; set; }

        [Obsolete("Use UserMappingId and DomainMappingId instead of this one.")]
        [DataMember]
        public string UserAndDomainMappingId { get; set; }

        [DataMember]
        public string UserMappingId { get; set; }

        [DataMember]
        public string DomainMappingId { get; set; }

        [DataMember]
        public string ColumnMappingId { get; set; }

        [DataMember]
        public string FilterPolicyId { get; set; }

        [DataMember]
        public string EncyptionProfileId { get; set; }   
    }
}
