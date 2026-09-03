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
using AvePoint.GCommon.Contract.PlatformRecovery.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    [DataContract]
    public class PlatformBackupTreeMessage : AveTreeMessage
    {
        [DataMember]
        public List<PRTreeNodeDto> NodeList { get; set; }

        [DataMember]
        public PRTreeNodeDto Node { get; set; }

        /// <summary>存放ssa源端tree</summary>
        [DataMember]
        public PRTreeNodeDto SrcTreeNode { get; set; }

        [DataMember]
        public List<SPTreeNodeDto> SPNodeList { get; set; }

        [DataMember]
        public SPTreeNodeDto SPNode { get; set; }

        /// <summary>GUI赋值(ItemTree的Live Model)</summary>
        [DataMember]
        public Guid SessionId { get; set; }

        /// <summary>Config按钮设置的staging,如果没有设置则为null(ItemTree的Live Model)</summary>
        [DataMember]
        public PRStagingPolicyDto StagingPolicy { get; set; }

        /// <summary>true为Live Model</summary>
        [DataMember]
        public bool IsLiveMode { get; set; }

        /// <summary>DB级别tree根节点farmid</summary>
        [DataMember]
        public string FarmID { get; set; }

        [DataMember]
        public ServiceDto DesAgent { get; set; }

        /// <summary>当为非常规设备时，存放该属性</summary>
        [DataMember]
        public string StorageInfoExtension { get; set; }

        /// <summary>true为可以展开granular节点</summary>
        [DataMember]
        public bool IsBrowseGranularNode { get; set; }

        // 存放level属性
        [DataMember]
        public string Level { get; set; }

        // 为smsp平台存放staging信息
        [DataMember]
        public PRStagingPolicyDto SQLInstanceInfo { get; set; }

        // 平台信息
        [DataMember]
        public PRPlatformType PlatformType { get; set; }

        [DataMember]
        public bool IsRefresh { get; set; }
    }
}
