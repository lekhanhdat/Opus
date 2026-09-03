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
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRRestoreMessage : PRMessage
    {
        /// <summary>
        /// 备份plan对象
        /// </summary>
        [DataMember]
        public PRRestorePlanDto PlanDto { get; set; }

        /// <summary>
        /// 备份job对象
        /// </summary>
        [DataMember]
        public PRRestoreJobDto JobDto { get; set; }

        /// <summary>
        /// 目的端SPTree
        /// </summary>
        [DataMember]
        public SPTreeNodeDto DestTreeNode { get; set; }

        /// <summary>
        /// 目的端WFE PRTree
        /// </summary>
        [DataMember]
        public PRTreeNodeDto DestWFETreeNode { get; set; }

        /// <summary>
        /// 目的端SSASetting PRTree
        /// </summary>
        [DataMember]
        public PRTreeNodeDto DestSSASettingTreeNode { get; set; }

        /// <summary>
        /// media备份时使用
        /// </summary>
        [DataMember]
        public PlatformRestoreRequest ConfigForMedia { get; set; }

        /// <summary>
        /// media信息,agent使用
        /// </summary>
        [DataMember]
        public ServiceDto MediaInfo { get; set; }

        /// <summary>
        /// schedule 对象
        /// </summary>
        [DataMember]
        public ScheduleDto Schedule { get; set; }

        /// <summary>
        /// 全部PR模块agent
        /// </summary>
        [DataMember]
        public IList<ServiceDto> AgentList { get; set; }

        [DataMember]
        public PRItemMessage ItemMessage { get; set; }

        /// <summary>
        /// AlternateLocation 对象
        /// </summary>
        [DataMember]
        public List<PRManuallyResult> ManuallyResultList { get; set; }

        /// <summary>
        /// 获得当前job下cycle的storageInfo信息
        /// </summary>
        [DataMember]
        public Dictionary<string, string> StorageInfoMap { get; set; }
    }


    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class RestoreConfig
    //{
    //    [DataMember]
    //    public bool IncludingRecycleBinData { get; set; }

    //    [DataMember]
    //    public bool IncludeItemsReport { get; set; }

    //    [DataMember]
    //    public bool CreateNewVersionForDuplicateItems { get; set; }

    //    ///[DataMember]
    //    ///public RestoreVersionSetting RestoreVersionSetting { get; set; }

    //    [DataMember]
    //    public RestoreOption RestoreOption { get; set; }

    //    [DataMember]
    //    public int RestoreType { get; set; }

    //    [DataMember]
    //    public DestinationInfo DestinationInfo { set; get; }

    //    [DataMember]
    //    public bool RestoreWorkflowState { get; set; }

    //    [DataMember]
    //    public int JobType { get; set; }
    //}

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DestinationInfo
    {
        [DataMember]
        public uint Language { get; set; }

        [DataMember]
        public char ReplaceType { get; set; }

        [DataMember]
        public string OwerLogin { get; set; }

        [DataMember]
        public Guid ContentDBId { get; set; }
    }
}