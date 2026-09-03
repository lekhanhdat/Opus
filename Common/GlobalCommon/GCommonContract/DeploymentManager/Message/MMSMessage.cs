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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.DeploymentManager.Object;
using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.DeploymentManager.Message
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MMSMessage
    {
        [DataMember]
        public MMSContent MMSContent { get; set; }
        [DataMember]
        public DMMessageType MessageType { get; set; }
        [DataMember]
        public MetadataServiceOptions MMSOption { get; set; }
        /// <summary>
        /// 用来区分import or export功能
        /// </summary>
        [DataMember]
        public DMPlanType DMPlanType { get; set; }

        [DataMember]
        public ControlJobType ControlJobType { get; set; }

        [DataMember]
        public DPMJobType MMSJobType { get; set; }
        /// <summary>
        /// for Back up
        /// </summary>
        [DataMember]
        public ServiceDto BackUpAgentDto { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MMSContent
    {
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public MMSMessageType MMSMessageType { get; set; }
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public bool IsTestRun { get; set; }
        [DataMember]
        public ServiceDto SourceAgent { get; set; }
        [DataMember]
        public SPTreeNodeDto SrcTree { get; set; }
        [DataMember]
        public List<DMDestInfo> DMDestInfos { get; set; }
        [DataMember]
        public DateTime LastJobEndTime { get; set; }
        [DataMember]
        public UserAndDomainMapping UserMapping { get; set; }
        [DataMember]
        public ServiceDto MediaService { get; set; }
        [DataMember]
        public GeneralBackupRequest BackupUndoReuqest { get; set; }
        [DataMember]
        public GeneralRestoreRequest RestoreRequest { get; set; }
        /// <summary>
        /// Work Flow Definition
        /// </summary>
        [DataMember]
        public bool IncludeWorkflowDefinition { get; set; }

    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MetadataServiceOptions
    {
        [DataMember]
        public DPMConflictResolution ConflictResolutionOption { set; get; }

        [DataMember]
        public DeploymentOption DeploymentOption { set; get; }

        /// <summary>
        /// 存储CheckBox中Recurision值
        /// </summary>
        [DataMember]
        public bool Recursion { get; set; }

        /// <summary>
        /// 存储Configuration值
        /// </summary>
        [DataMember]
        public bool IsConfiguration { get; set; }
        /// <summary>
        /// 存储Security值
        /// </summary>
        [DataMember]
        public bool IsSecurity { get; set; }
        /// <summary>
        /// 存储RefreshAllPublishedContentTypes值
        /// </summary>
        [DataMember]
        public bool IsRefreshAll { get; set; }

    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MMSMessageType
    {
        [EnumMember]
        TermStore,
        [EnumMember]
        ContentTypeHub,
        [EnumMember]
        None
    }
}
