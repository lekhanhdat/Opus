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
using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.DeploymentManager.Message
{

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WFEDMMessage
    {
        [DataMember]
        public WFEContent WFEContent { get; set; }

        [DataMember]
        public PlanInfo PlanInfo { get; set; }

        [DataMember]
        public DMMessageType MessageType { get; set; }

        [DataMember]
        public ControlJobType ControlJobType { get; set; }

        [DataMember]
        public WFEOperationType Operation { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WFEContent
    {
        [DataMember]
        public ServiceDto AgentDto { get; set; }
        [DataMember]
        public SPTreeNodeDto SrcTree { get; set; }
        [DataMember]
        public List<DMDestInfo> DMDestInfos { get; set; }
        [DataMember]
        public int CopyOnly { get; set; }
        [DataMember]
        public int DataMode { get; set; }
        [DataMember]
        public string DataVersion { get; set; }
        [DataMember]
        public int IndexLevel { get; set; }
        [DataMember]
        public int Invoice { get; set; }
        [DataMember]
        public int Level { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public int ServerSideDataMode { get; set; }
        [DataMember]
        public bool isIISSecurity { set; get; }
        /// <summary>
        ///  stsadm或者power shell的命令行
        /// </summary>
        [DataMember]
        public string CmdLine { get; set; }
        /// <summary>
        /// SrcAgentId属性server使用，agent不用
        /// </summary>
        [DataMember]
        public string SrcAgentId { get; set; }
        /// <summary>
        ///  stop使用
        /// </summary>
        [DataMember]
        public string ResJobId { get; set; }
        /// <summary>
        /// stop使用
        /// </summary>
        [DataMember]
        public string ResPlanId { get; set; }
        [DataMember]
        public bool IsTestRun { get; set; }
        /// <summary>
        /// 存储ConflictResolution Options值
        /// </summary>
        [DataMember]
        public DPMConflictResolution ConflictResolutionOption { get; set; }
        /// <summary>
        /// agent不在使用，server再用
        /// </summary>
        [DataMember]
        public List<IISConfigInfo> IISConfigInfo { set; get; }

        [DataMember]
        public bool IsWFERollBack { get; set; }

        [DataMember]
        public GeneralBackupRequest WFEBackupRequest { get; set; }

        [DataMember]
        public GeneralRestoreRequest WFERestoreRequest { get; set; }

        [DataMember]
        public ServiceDto RollbackMediaService { get; set; }


        [DataMember]
        public DPMJobType WFEJobType { get; set; }

        [DataMember]
        public bool IncludeContent { get; set; }

        [DataMember]
        public bool IncludeWebConfiguration { get; set; }


        [DataMember]
        public bool IncludeParentProperties { get; set; }

        [DataMember]
        public List<string> WebConfigs { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlanInfo
    {
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public string PlanName { get; set; }
        [DataMember]
        public DMPlanType PlanType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum WFEOperationType
    {
        //[EnumMember]
        //Export,
        //[EnumMember]
        //Import,
        [EnumMember]
        WFEOneSideStsadmCmd,
        [EnumMember]
        WFEOneSidePowerShellCmd,
        [EnumMember]
        OnlineWFEJob,
        [EnumMember]
        WFEBackup,
        [EnumMember]
        WFERestore
    }

}
