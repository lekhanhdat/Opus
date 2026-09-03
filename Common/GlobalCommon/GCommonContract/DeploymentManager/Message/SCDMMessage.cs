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
using AvePoint.GCommon.Contract.DeploymentManager.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Contract.Server.Common.ExportLocation.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.DeploymentManager.Message
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SCDMMessage : AveMessage
    {
        [DataMember]
        public SCContent SCContent { get; set; }
        [DataMember]
        public DMMessageType MessageType { get; set; }
        [DataMember]
        public SCMessageType SCOperation { get; set; }
        [DataMember]
        public ControlJobType ControlJobType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DMMessageType
    {
        [EnumMember]
        ServerRequest = 0,
        [EnumMember]
        PrimaryRequest = 1
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SCContent
    {
        [DataMember]
        public ServiceDto SrcAgent { get; set; }
        [DataMember]
        public SPTreeNodeDto SrcTree { get; set; }
        [DataMember]
        public List<DMDestInfo> DMDestInfos { get; set; }
        [DataMember]
        public SolutionJobInfo SolutionJobInfo { get; set; }
        [DataMember]
        public ServiceDto MediaServiceDto { get; set; }
        [DataMember]
        public ServiceDto UndoMediaService { get; set; }
        [DataMember]
        public List<FSSolutionNodeDto> FSSolutionNodeDtoList { get; set; }
        [DataMember]
        public SolutionBackupRequest SolutionBackupRequest { get; set; }
        [DataMember]
        public SolutionRestoreRequest SolutionRestoreRequest { get; set; }
        [DataMember]
        public GeneralBackupRequest BackupUndoReuqest { get; set; }
        [DataMember]
        public GeneralRestoreRequest SolutionRollbackReuqest { get; set; }
        [DataMember]
        public string StoragePolicyName { get; set; }
        //ConnectionXRI目前没有用到
        //[DataMember]
        //public string ConnectionXRI { get; set; }
        /// <summary>
        /// 为Media组装消息使用
        /// </summary>
        [DataMember]
        public SolutionCenterRemoveDataParamDto SolutionCenterRemoveDataParamDto { get; set; }

        [DataMember]
        public DPMJobType SCJobType { get; set; }

        /// <summary>
        /// true为目的端选择SiteCollection的Job，false为走正常DM逻辑的Job
        /// </summary>
        [DataMember]
        public bool IsGranularJob { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SolutionJobInfo
    {
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public BrowseLevel BrowseLevel { get; set; }
        /// <summary>
        /// for media
        /// </summary>
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool IsTestRun { get; set; }
        [DataMember]
        public DPMConflictResolution ConflictResolutionOption { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FSSolutionNodeDto
    {
        [DataMember]
        public ExportLocationDto ExportLocationDto { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string FullPath { get; set; }

    }
}
