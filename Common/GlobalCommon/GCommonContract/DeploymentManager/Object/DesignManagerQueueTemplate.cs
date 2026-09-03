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
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.DeploymentManager.Object
{

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DesignManagerQueueTemplate : QueueTemplate
    {
        /// <summary>
        /// 存储Setting信息
        /// </summary>
        [DataMember]
        public DMQueueContent DMContent { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DMQueueContent
    {
        [DataMember]
        public string LanguageMappingName { get; set; }
        [DataMember]
        public string UserMappingName { get; set; }
        [DataMember]
        public string DomainMappingName { get; set; }
        [DataMember]
        public string SourceFilterPolicyName { get; set; }
        [DataMember]
        public string DestinationFilterPolicyName { get; set; }
        [DataMember]
        public bool DeployToRelativeLists { get; set; }
        [DataMember]
        public string ExportLocationName { get; set; }
        [DataMember]
        public bool Security { get; set; }
        [DataMember]
        public bool IsUserContent { get; set; }
        [DataMember]
        public bool IsIncludeUserProfiles { get; set; }
        /// <summary>
        /// 存储ContainerConflictResolution Options值
        /// </summary>
        [DataMember]
        public DPMConflictResolution ContainerConflictResolutionOption { get; set; }
        /// <summary>
        /// 存储ConflictResolution Options值
        /// </summary>
        [DataMember]
        public bool Recursion { get; set; }
        /// <summary>
        /// 存储ConflictResolution Options值
        /// </summary>
        [DataMember]
        public DPMConflictResolution ContentConflictResolutionOption { get; set; }
        /// <summary>
        /// 存储MigrateConfiguration Options值
        /// </summary>
        [DataMember]
        public DPMConflictResolution MigrateTheItemConflictResolution { get; set; }
        /// <summary>
        /// Work Flow Definition
        /// </summary>
        [DataMember]
        public bool IncludeWorkflowDefinition { get; set; }
        [DataMember]
        public MigrateTheItem MigrateTheItem { get; set; }
        /// <summary>
        /// 是否keep null值到目的端
        /// </summary>
        [DataMember]
        public Boolean IsPreserveNullColumnValues { get; set; }
        [DataMember]
        public BatchProcessingType BatchProcessingType { get; set; }

        public Boolean IncludeApp { get; set; }
        [DataMember]
        public DPMConflictResolution AppConflictResolutionOption { set; get; }
    }
}
