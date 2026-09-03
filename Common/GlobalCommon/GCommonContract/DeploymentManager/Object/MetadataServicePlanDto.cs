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
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.DeploymentManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MetadataServicePlanDto : AbstractDMPlanDto
    {
        private MetadataServicePlanDto metadataServicePlanDto;

        public MetadataServicePlanDto(MetadataServicePlanDto metadataServicePlanDto)
        {
            // TODO: Complete member initialization
            this.metadataServicePlanDto = metadataServicePlanDto;
        }

        public MetadataServicePlanDto()
        {
            // TODO: Complete member initialization
        }

        /// <summary>
        /// 存储DM界面的选项
        /// </summary>
        [DataMember]
        public MetadataServiceOptionForGui MMSOption { get; set; }

        /// <summary>
        /// 存储JobId
        /// </summary>
        [DataMember]
        public string JobId { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MetadataServiceOptionForGui : MetadataServiceOption
    {
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MetadataServiceOption
    {
        /// <summary>
        /// 存储选中节点的level值
        /// </summary>
        [DataMember]
        [XmlAttribute("selectedTreeNodeLevel")]
        public NodeLevel SelectedTreeNodeLevel { get; set; }

        /// <summary>
        /// 存储ConflictResolution Options值
        /// </summary>
        [DataMember]
        [XmlAttribute("conflictResolutionOption")]
        public DPMConflictResolution ConflictResolutionOption { get; set; }
        /// <summary>
        /// 存储Deployment option值
        /// </summary>
        [DataMember]
        [XmlAttribute("deploymentOption")]
        public DeploymentOption DeploymentOption { get; set; }

        [DataMember]
        [XmlAttribute("includeWorkflowDefinition")]
        public bool IncludeWorkflowDefinition { get; set; }

        private bool _recursion = true;
        /// <summary>
        /// 存储CheckBox中Recurision值
        /// </summary>
        [DataMember]
        [XmlAttribute("recursion")]
        public bool Recursion
        {
            get
            {
                return this._recursion;
            }
            set
            {
                this._recursion = value;
            }
        }

        /// <summary>
        /// 存储Configuration值
        /// </summary>
        [DataMember]
        [XmlAttribute("isConfiguration")]
        public bool IsConfiguration { get; set; }

        /// <summary>
        /// 存储Security值
        /// </summary>
        [DataMember]
        [XmlAttribute("isSecurity")]
        public bool IsSecurity { get; set; }

        /// <summary>
        /// 存储RefreshAllPublishedContentTypes值
        /// </summary>
        [DataMember]
        [XmlAttribute("isRefreshAll")]
        public bool IsRefreshAll { get; set; }

        /// <summary>
        /// 存储UserMappingId值
        /// </summary>
        [DataMember]
        [XmlAttribute("userMappingId")]
        public string UserMappingId { get; set; }

        /// <summary>
        /// 存储DomainMappingId值
        /// </summary>
        [DataMember]
        [XmlAttribute("domainMappingId")]
        public string DomainMappingId { get; set; }
    }
}
