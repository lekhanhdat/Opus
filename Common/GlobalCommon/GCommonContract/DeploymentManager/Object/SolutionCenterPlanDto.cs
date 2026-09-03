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
using AvePoint.GCommon.Contract.DeploymentManager.Message;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.DeploymentManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SolutionCenterPlanDto : AbstractDMPlanDto
    {
        [DataMember]
        public SolutionCenterOptionForGui SolutionCenterOption { get; set; }
        [DataMember]
        public SCMessageType SCOperation { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SolutionCenterOption
    {
        [DataMember]
        public string FilterId { get; set; }

        [DataMember]
        public string DestFilterId { set; get; }

        /// <summary>
        /// 存储Compare时候Solution的Setting页面存储的Node Dest for GUI
        /// </summary>
        [DataMember]
        public List<SPTreeNodeDto> DestCompareSettingNode { get; set; }

        /// <summary>
        /// 存储Compare时候Solution的Setting页面存储的Node Src for GUI
        /// </summary>
        [DataMember]
        public List<SPTreeNodeDto> SrcCompareSettingNode { get; set; }

        /// <summary>
        /// 存储选中节点的level值
        /// </summary>
        [DataMember]
        public NodeLevel SelectedTreeNodeLevel { get; set; }

        /// <summary>
        /// 存储ConflictResolution Options值
        /// </summary>
        [DataMember]
        public DPMConflictResolution ConflictResolutionOption { get; set; }

        /*/// <summary>
        /// 对应setting界面中Overwrite Solutions if they already exist
        /// </summary>
        [DataMember]
        public bool OverwriteSolutions { get; set; }
        /// <summary>
        /// 对应setting界面中Use Upgrade Solution method
        /// </summary>
        [DataMember]
        public bool UseUpgrade { get; set; }
        /// <summary>
        /// 对应setting界面中Use Retract/re-deploy method
        /// </summary>
        [DataMember]
        public bool UseRetract { get; set; }
        **/
        /// <summary>
        /// 对应Reorder The solutions that you want to deploy中的设置
        /// </summary>
        [DataMember]
        public List<SolutionsReorder> Reorder { get; set; }
        /// <summary>
        /// 存储Storage Policy的Id值
        /// </summary>
        [DataMember]
        public string StoragePolicyId { get; set; }

        /// <summary>
        /// 存储Storage Policy的Name值
        /// </summary>
        [DataMember]
        public string StoragePolicyName { get; set; }

        /// <summary>
        /// 存储Export Location的Id值
        /// </summary>
        [DataMember]
        public string ExportLocationId { get; set; }

        /// <summary>
        /// 存储Solution的类型
        /// </summary>
        [DataMember]
        public SolutionStoreType SolutionStoreType { get; set; }

        /// <summary>
        /// 存储ImportType下拉菜单值
        /// </summary>
        [DataMember]
        public ImportType ImportType { get; set; }

        /// <summary>
        /// 为true的时删除SPTreeNode节点,false时删除Version
        /// </summary>
        [DataMember]
        public bool IsRemoveNode { get; set; }

        [DataMember]
        public MappingSource MappingSource { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SolutionCenterOptionForGui : SolutionCenterOption
    {
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SolutionsReorder
    {
        [DataMember]
        public int Order { get; set; }
        [DataMember]
        public string SolutionId { get; set; }
        [DataMember]
        public string SolutionName { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SolutionStoreType : int
    {
        [EnumMember]
        Undefined = -1,
        [EnumMember]
        SolutionStore = 0,
        [EnumMember]
        FileSystem = 1
    }
}
