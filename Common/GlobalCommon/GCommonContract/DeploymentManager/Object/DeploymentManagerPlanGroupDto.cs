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
using AvePoint.Common;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.DeploymentManager.Object
{
    [DataContract]
    public class DeploymentManagerPlanGroupDto : AbstractGroup, IComparable<DeploymentManagerPlanGroupDto>
    {
        [DataMember]
        public List<PlanOrderInfo> PlanOrderInfos { get; set; }

        [DataMember]
        /// <summary>
        /// 为了调整DPM 存plan的逻辑，在html 新版中，Add2Queue时不存plan，而是在save plan时统一存
        /// 所以这里存着Tree的信息，数量与PlanOrderInfos一一对应
        /// </summary>
        public List<AbstractDMPlanDto> PlanOrderInfoTrees { get; set; }

        /// <summary>
        /// 更新Plan时，校正mapping的order
        /// </summary>
        [DataMember]
        public List<string> NewPlanIdOrders { get; set; }

        [DataMember]
        public DMGroupSetting GroupSetting { get; set; }

        [DataMember]
        public PlanGroupType GroupType { get; set; }

        /// <summary>
        /// 存储创建plan,plangroup的时候产生的错误类型
        /// 1.为The name of the plan group (" + this.Content.Name + ") already exists.
        /// 2.为没有选择Backup中StoragePolicy.
        /// </summary>
        [DataMember]
        public int ErrorMessageType { get; set; }

        /// <summary>
        /// GUI Check Box Status, Don't Remove
        /// </summary>
        public bool IsChecked { get; set; }

        public int CompareTo(DeploymentManagerPlanGroupDto other)
        {
            return this.ObjectInfo.CreateTime.CompareTo(other.ObjectInfo.CreateTime);
        }

        [DataMember]
        public ObjectInfoDto ObjectInfo { get; set; }

        /// <summary>
        /// 记录上传的总数量
        /// </summary>
        [DataMember]
        public int UploadTotalCount { get; set; }

        [DataMember]
        public List<string> PlanGroupIds { get; set; }

        [DataMember]
        public List<string> PlanGroupNames { get; set; }

        [DataMember]
        public List<NameAndIdDto> PlanGroups { get; set; }

        public bool IsShared()
        {

            if (ObjectInfo == null || ObjectInfo.ObjectPermissions == null)
            {
                throw new Exception("Object permission info is null");
            }
            var planPermissions = ObjectInfo.ObjectPermissions;
            var shareCount = 0;
            foreach (var permission in planPermissions)
            {
                if (permission.PermissionScope == ObjectPermissionScopeType.User && permission.Permission > 0)
                {
                    shareCount++;
                }
            }
            var isPlanShared = shareCount > 1;
            return isPlanShared;
        }
    }

    [DataContract]
    public class DMGroupSetting
    {
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// For Back up
        /// </summary>
        [DataMember]
        public bool IsBackUp { get; set; }

        [DataMember]
        public string StoragePolicyID { get; set; }

        [DataMember]
        public NotificationDto Notification { get; set; }

        [DataMember]
        public string ProfileID { get; set; }

        /// <summary>
        /// Rest API中支持创建临时的DPM plan，此plan应该被过滤，不需要显示
        /// </summary>
        [DataMember]
        public bool IsHiddenPlan { get; set; }

        [DataMember]
        public bool OverWriteRegionalSetting { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlanOrderInfo : IComparable<PlanOrderInfo>
    {
        [DataMember]
        public string PlanId { get; set; }

        /// <summary>
        /// plan在plan group的planorderinfo中的位置
        /// </summary>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// 表示是什么类型的plan: DM, FE, SC
        /// </summary>
        [DataMember]
        public DMPlanCategory DMCategory { get; set; }

        /// <summary>
        /// enable或disabled两种状态
        /// </summary>
        [DataMember]
        public PlanState PlanState { get; set; }

        /// <summary>
        /// plan group中的check box是否被选中
        /// </summary>
        [DataMember]
        public bool IsCheck { get; set; }

        /// <summary>
        /// plan对应的最后一个Job信息
        /// </summary>
        [DataMember]
        public SubJobDto JobDto { get; set; }

        public int CompareTo(PlanOrderInfo other)
        {
            return this.Order.CompareTo(other.Order);
        }

        [DataMember]
        public string SrcPath { get; set; }

        [DataMember]
        public string DestPath { get; set; }

        /// <summary>
        /// 判断上传的Excel的Tree选择逻辑的正确性
        /// </summary>
        [DataMember]
        public ValidateTreeOperatorResult ValidateResult { get; set; }

        [DataMember]
        [Obsolete]
        public int Progress { get; set; }

        [DataMember]
        [Obsolete]
        public int JobState { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot(ElementName = "PlanOrderSetting")]
    public class PlanOrderSetting
    {
        [DataMember]
        [XmlArray("PlanOrderInfos")]
        public List<PlanOrderInfo> PlanOrderInfos { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType(typeof(DesignManagerPlanDto))]
    [KnownType(typeof(SolutionCenterPlanDto))]
    public class AbstractDMPlanDto : PlanDto
    {
        [DataMember]
        public string GroupId { get; set; }

        /// <summary>
        /// 用于存储当前的Queue所在的Plan的名称
        /// </summary>
        [DataMember]
        public string DPMPlanGroupName { get; set; }

        /// <summary>
        /// 用于判断当前的Plan属于那个功能的，SC,DM,FE
        /// </summary>
        [DataMember]
        public DMPlanCategory DMCategory { get; set; }

        /// <summary>
        /// 用于区分当前plan属于Export,import还是Deploy
        /// </summary>
        [DataMember]
        public DMPlanType DMPlanType { get; set; }

        [DataMember]
        public FarmDto SrcFarm { set; get; }

        /// <summary>
        /// 源端tree节点
        /// </summary>
        [DataMember]
        public SPTreeNodeDto SrcTree { set; get; }

        /// <summary>
        /// 存储目的端tree的集合，其中包括Farm信息
        /// </summary>
        [DataMember]
        public List<DeploymentManagerDestInfo> DMDestInfo { set; get; }

        /// <summary>
        /// 存储原端tree的Path
        /// </summary>
        [DataMember]
        public string SrcPath { set; get; }

        /// <summary>
        /// 存储原端tree的Path
        /// </summary>
        [DataMember]
        public string DestPath { set; get; }

        /// <summary>
        /// 用于存储Import过程中的FileSystem Tree
        /// </summary>
        [DataMember]
        public List<FSTreeNodeDto> FSTreeList { get; set; }

        /// <summary>
        ///  用于后台DTO序列化存储转换
        /// </summary>
        [DataMember]
        public FSTreeNodeList FSTreeNodes { get; set; }

        /// <summary>
        /// 用于判断当前的Plan类型是否是Quick的类型
        /// </summary>
        [DataMember]
        public PlanGroupType DefaultPlanType { get; set; }

        /// <summary>
        /// 判断上传的Excel的Tree选择逻辑的正确性
        /// </summary>
        [DataMember]
        public ValidateTreeOperatorResult ValidateResult { get; set; }

        /// <summary>
        /// 浅复制
        /// </summary>
        /// <returns></returns>
        public AbstractDMPlanDto Clone()
        {
            //AbstractDMPlanDto clone = new AbstractDMPlanDto();
            //clone.GroupId = this.GroupId;
            //clone.DMCategory = this.DMCategory;
            //this.SrcFarm.DeepCopyProperties(clone.SrcFarm);
            //this.SrcTree.CanChildrenBeLoaded
            return this.MemberwiseClone() as AbstractDMPlanDto;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LoadDeploymentManagerPlanResult
    {
        [DataMember]
        public DeploymentManagerPlanGroupDto Plan { get; set; }

        [DataMember]
        public bool IsRunning { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LoadDeploymentManagerMappingResult
    {
        [DataMember]
        public DesignManagerPlanDto DesignPlan { get; set; }

        [DataMember]
        public SolutionCenterPlanDto SolutionPlan { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot(ElementName = "ImportTreeNodeList")]
    public class FSTreeNodeList
    {
        /// <summary>
        /// 用于存储Import过程中的FileSystem Tree
        /// </summary>
        [DataMember]
        [XmlArray("FSTrees")]
        public List<FSTreeNodeDto> FSTrees { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DeploymentManagerDestInfo
    {
        /// <summary>
        /// 用于存储DContent的Id值
        /// </summary>
        [DataMember]
        public string DContentId { get; set; }

        /// <summary>
        /// 存储目的端farm信息
        /// </summary>
        [DataMember]
        public FarmDto DestFarm { set; get; }

        /// <summary>
        /// 存储目的端tree信息
        /// </summary>
        [DataMember]
        public SPTreeNodeDto DestTree { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RunNowParam
    {
        [DataMember]
        public string PlanId { set; get; }

        [DataMember]
        public string Username { set; get; }

        [DataMember]
        public int DeploymentType { set; get; }

        [DataMember]
        public string Desc { set; get; }

        [DataMember]
        public double Version { set; get; }

        [DataMember]
        public string VersionDesc { set; get; }

        [DataMember]
        public bool IsTestRun { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember]
        public string PlanName { get; set; }

        [DataMember]
        public string JobQueueId { get; set; }

        #region == 这两个属性是上传Excel功能所需要使用的属性 ==

        [DataMember]
        public string ForderName { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public JobState State { get; set; }

        [DataMember]
        public ObjectInfoDto ObjectInfo { get; set; }

        #endregion

        #region == 这两个属性是Control Panel Plan Group功能所需要使用的属性 ==

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public string PlanGroupId { get; set; }

        [DataMember]
        public string PlanGroupName { get; set; }

        [DataMember]
        public string PlanGroupExecutionId { get; set; }

        [DataMember]
        public string UserId { get; set; }

        #endregion
    }

    [DataContract]
    public enum DMPlanCategory : int
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        DesignManager = 1,

        [EnumMember]
        FrontEndDeployment = 2,

        [EnumMember]
        SolutionCente = 3,

        [EnumMember]
        MetadataService = 4,

        [EnumMember]
        DMCompareReport = 5,

        [EnumMember]
        SPAppUpdate = 6,

        [EnumMember]
        SPPushAppUpdate = 7
    }

    [DataContract]
    public enum PlanState : int
    {
        [EnumMember]
        Enable = 0,

        [EnumMember]
        Disabled = 1
    }

    [DataContract]
    public enum PlanGroupType : int
    {
        [EnumMember]
        DefaultGroup = 0,

        [EnumMember]
        UserGroup = 1
    }

    [DataContract]
    public enum ValidateTreeOperatorResult : int
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Success = 1,

        [EnumMember]
        Failed = 2,
    }

    [DataContract]
    public enum DMScheduleValidationResultType : int
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        StartTimeNotValid = 1,

        [EnumMember]
        EndTimeNotValid = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DMScheduleValidationResult
    {
        [DataMember]
        public DMScheduleValidationResultType Type { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UploadJobParam
    {
        [DataMember]
        public int CurrQueueIndex { get; set; }

        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public int AllQueueCount { get; set; }

        [DataMember]
        public DMPlanCategory DMPlanType { get; set; }

        [DataMember]
        public int JobType { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public string SubJobId { get; set; }

        [DataMember]
        public bool IsHasError { get; set; }

        [DataMember]
        public bool IsReadExcelError { get; set; }

        [DataMember]
        public string FailedMessage { get; set; }

        [DataMember]
        public ObjectInfoDto ObjectInfo { get; set; }

        [DataMember]
        public Dictionary<string, JobDetail> QueueDetails { get; set; }

        [DataMember]
        public string PlanGroupName { get; set; }

        [DataMember]
        public string PlanGroupDescription { get; set; }
    }

    public struct DeploymentManagerPlanExecuteParameter
    {
        public DeploymentManagerPlanGroupDto Group { get; set; }

        public DeploymentManagerPlanExecuteOption ExecuteOption { get; set; }
    }

    public struct DeploymentManagerPlanUpdatingParameter
    {
        public string PlanId { get; set; }

        public Dictionary<DeploymentManagerMappingUpdateOption, List<string>> MappingOptions { get; set; }

    }

    public enum DeploymentManagerMappingUpdateOption
    {
        Delete,
        Enable,
        Disable
    }

    public enum DeploymentManagerPlanExecuteOption
    {
        Execute,
        ExecuteThenRun,
    }
}