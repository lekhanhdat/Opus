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
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    /// <summary>
    /// 1, when load rules, server side sent this request to get all rules' definition from agent side location
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdministratorProfileLoadOperation : CAOperation
    {
        [DataMember]
        public List<AdminRuleBasicInfo> AdminRuleBasicInfos { get; set; }
    }

    /// <summary>
    ///2, when run the schedule job, server side send the rules in the scope, agent side collecte all event based the scope 
    ///and rule type. then triggered the rule
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdministratorProfileJobOperation : CAOperation
    {
        [DataMember]
        public List<AdministratorProfileInfo> AdminProfileInfos { get; set; }
        [DataMember]
        public LastRunTimesNodeCollection LastRunTimeNodes { get; set; }
        [DataMember]
        public List<string> AccountFilter { get; set; }
        [DataMember]
        public AuditStatus AuditStatus { get; set; }
        //Apply Profile和StopInherit共用一个CAAction Add，用于区分这两个操作
        [DataMember]
        public InheritAction InheritAction { get; set; }
        [DataMember]
        public int JobType { get; set; }
        [DataMember]
        public bool IsApplyAndRunNow { get; set; }
        //存储farm下所有的应用profile的节点的ID
        [DataMember]
        public Dictionary<string, List<string>> AllApplyNodesUrlAndId { get; set; }

        [DataMember]
        public List<CADocAveNodePolicyKey> NotExsitKeys { get; set; }

        [DataMember]
        public string MgtApiConnString { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AuditStatus
    {
        [EnumMember]
        CheckAuditStatus,
        [EnumMember]
        AuditNotEnabled,
        [EnumMember]
        AuditEnabled
    }

    #region  new

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NewAdministratorProfileJobOperation : CAOperation
    {
        [DataMember]
        public List<AdministratorProfileInfo> AdminProfileInfos { get; set; }
        [DataMember]
        public NewLastRunTimesNodeCollection LastRunTimeNodes { get; set; }
        [DataMember]
        public List<string> AccountFilter { get; set; }
        [DataMember]
        public AuditStatus AuditStatus { get; set; }
        //Apply Profile和StopInherit共用一个CAAction Add，用于区分这两个操作
        [DataMember]
        public InheritAction InheritAction { get; set; }
        [DataMember]
        public int JobType { get; set; }
        [DataMember]
        public bool IsApplyAndRunNow { get; set; }
        //存储farm下所有的应用profile的节点的ID
        [DataMember]
        public Dictionary<string, List<string>> AllApplyNodesUrlAndId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NewLastRunTimesNodeCollection
    {
        [DataMember]
        public string FarmId { get; set; }
        /// <summary>
        /// 37之前老数据
        /// </summary>
        [DataMember]
        public Dictionary<string, Dictionary<AdminEventType, DateTime>> LastRunTimes { get; set; }

        /// <summary>
        /// 37之后新数据
        /// </summary>
        [DataMember]
        public List<LastRunTimesParam> NewLastRunTimes { get; set; }
        /// <summary>
        /// ADO-116296
        /// For O365. My Registered Sites中注册的站点所在系统时区和SharePoint站点的时区不同时 取不到Auditor数据
        /// </summary>
        [DataMember]
        public bool IsUseDefaultRegionalSetting { get; set; }
        /// <summary>
        /// ADO-116296
        /// For O365. My Registered Sites中注册的站点所在系统时区和SharePoint站点的时区不同时 取不到Auditor数据
        /// </summary>
        [DataMember]
        public string TimeZoneDescription { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LastRunTimesParam
    {
        [DataMember]
        public NodeCollection Collection { get; set; }

        [DataMember]
        public Dictionary<AdminEventType, DateTime> Value { get; set; }
    }

    #endregion



    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LastRunTimesNodeCollection
    {
        [DataMember]
        public string FarmId { get; set; }
        /// <summary>
        /// 37之前老数据
        /// </summary>
        [DataMember]
        public Dictionary<string, Dictionary<AdminEventType, DateTime>> LastRunTimes { get; set; }

        /// <summary>
        /// 37之后新数据
        /// </summary>
        [DataMember]
        public Dictionary<NodeCollection, Dictionary<AdminEventType, DateTime>> NewLastRunTimes { get; set; }
        /// <summary>
        /// ADO-116296
        /// For O365. My Registered Sites中注册的站点所在系统时区和SharePoint站点的时区不同时 取不到Auditor数据
        /// </summary>
        [DataMember]
        public bool IsUseDefaultRegionalSetting { get; set; }
        /// <summary>
        /// ADO-116296
        /// For O365. My Registered Sites中注册的站点所在系统时区和SharePoint站点的时区不同时 取不到Auditor数据
        /// </summary>
        [DataMember]
        public string TimeZoneDescription { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType(typeof(TreeNodeCollection))]
    public class NodeCollection
    {
        /// <summary>
        /// SelectNoede SPObjectID
        /// </summary>
        [DataMember]
        public string NodeId { get; set; }

        /// <summary>
        /// SelectNode URl
        /// </summary>
        [DataMember]
        public string Scope { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TreeNodeCollection : NodeCollection
    {
        [DataMember]
        public string ParentId { get; set; }
        [DataMember]
        public NodeLevel NodeLevel { get; set; }
        [DataMember]
        public string TenantId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ApplyNodeInfo
    {
        [DataMember]
        public NodeLevel ApplyLevel { get; set; }
        [DataMember]
        public List<TreeNodeCollection> ApplyNodesInfoList { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdministratorProfileInfo
    {
        /// <summary>
        /// 此属性储存应用此Profile的Node
        /// </summary>
        [DataMember]
        public List<SPTreeNodeDto> ApplyNodes { get; set; }
        /// <summary>
        /// 此属性用来辨别该Profile是否为group级别profile
        /// </summary>
        [DataMember]
        public List<string> GroupIds { get; set; }

        /// <summary>
        /// 此属性存储Profile的Update Time
        /// </summary>
        [DataMember]
        public long UpdateTime { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string ScopeId { get; set; }

        [DataMember]
        public string SchduleTemplateId { get; set; }

        [DataMember]
        public int RetentionIntervalCount { get; set; }

        [DataMember]
        public IntervalType RetentionIntervalType { get; set; }

        [DataMember]
        public List<AdminRuleBasicInfo> AdminRuleBasicInfos { get; set; }

        //SAAS-24407
        [DataMember]
        public bool AutoEnableAuditSetting { get; set; }

        [DataMember]
        public bool IsEnableAuditSetting { get; set; }

        [DataMember]
        public bool IsSendSummaryEmail { get; set; }
        [DataMember]
        public bool IsSendDaily { get; set; }
        [DataMember]
        public DateTime SendDailyStartTime { get; set; }
        [DataMember]
        public int SendDayOfWeek { get; set; }
        [DataMember]
        public DateTime SendWeeklyStartTime { get; set; }

        /// <summary>
        /// 用于存储此Profile的CreatorId, ADO-102367
        /// </summary>
        [DataMember]
        public string CreatorId { get; set; }
        [DataMember]
        public ApplyNodeInfo ApplyNodeInfo { get; set; }

        //支持VPAT
        public override string ToString()
        {
            return Name;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminRuleBasicInfo
    {
        [DataMember]
        public List<string> AccessListIds { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string AssemblyName { get; set; }

        [DataMember]
        public string AssemblyLocation { get; set; }

        [DataMember]
        public string ClassFullName { get; set; }

        [DataMember]
        public bool Status { get; set; }

        [DataMember]
        public bool IsUndo { get; set; }

        [DataMember]
        public bool CanUndo { get; set; }

        [DataMember]
        public bool SecondIsUndo { get; set; }

        [DataMember]
        public bool IsReport { get; set; }

        [DataMember]
        public List<UserDetail> ReportTo { get; set; }

        [DataMember]
        public bool IsReportToSiteAdmin { get; set; }

        [DataMember]
        public bool IsReportToViolateUser { get; set; }

        [DataMember]
        public AdminEventType EventType { get; set; }

        [DataMember]
        public List<AdminRuleParameter> Parameters { get; set; }

        /// <summary>
        /// 此属性给GUI使用, 标记此Rule是Configured还是Default
        /// </summary>
        [DataMember]
        public bool? Configured { get; set; }

        [DataMember]
        public List<ProfileReturnMessage> ReportMessage { get; set; }

        /// <summary>
        /// 此属性给Server使用
        /// </summary>
        public string ServerExtension { get; set; }

        /// <summary>
        /// 此属性给Server使用, 存储Policy的Id
        /// </summary>
        [DataMember]
        public string ParentId { get; set; }

        [DataMember]
        public string ParameterEvents { get; set; }

        //filter
        [DataMember]
        public FilterPolicyInfo FilterPolicyInfo { get; set; }

        //63rule此属性为空
        [DataMember]
        public bool? SendEmailImmediately { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [Flags]
    public enum AdminEventType : long
    {
        [EnumMember]
        None = 0L,
        //SharePoint audit event type
        [EnumMember]
        CheckOut = 1L,
        [EnumMember]
        CheckIn = 2L,
        [EnumMember]
        View = 4L,
        [EnumMember]
        Delete = 8L,
        [EnumMember]
        Update = 16L,
        [EnumMember]
        ProfileChange = 32L,
        [EnumMember]
        ChildDelete = 64L,
        [EnumMember]
        SchemaChange = 128L,
        [EnumMember]
        Undelete = 256L,
        [EnumMember]
        Workflow = 512L,
        [EnumMember]
        Copy = 1024L,
        [EnumMember]
        Move = 2048L,
        [EnumMember]
        AuditMaskChange = 4096L,
        [EnumMember]
        Search = 8192L,
        [EnumMember]
        ChildMove = 16384L,
        [EnumMember]
        FileFragmentWrite = 32768L,
        [EnumMember]
        SecGroupCreate = 65536L,
        [EnumMember]
        SecGroupDelete = 131072L,
        [EnumMember]
        SecGroupMemberAdd = 262144L,
        [EnumMember]
        SecGroupMemberDel = 524288L,
        [EnumMember]
        SecRoleDefCreate = 1048576L,
        [EnumMember]
        SecRoleDefDelete = 2097152L,
        [EnumMember]
        SecRoleDefModify = 4194304L,
        [EnumMember]
        SecRoleDefBreakInherit = 8388608L,
        [EnumMember]
        SecRoleBindUpdate = 16777216L,
        [EnumMember]
        SecRoleBindInherit = 33554432L,
        [EnumMember]
        SecRoleBindBreakInherit = 67108864L,
        [EnumMember]
        EventsDeleted = 134217728L,
        [EnumMember]
        Custom = 268435456L,

        //Admin profile event
        [EnumMember]
        SiteCreation = 536870912L,
        [EnumMember]
        ListCreation = 1073741824L,
        [EnumMember]
        ItemCreation = 2147483648L,

        [EnumMember]
        ScanFarmCondition = 4294967296L,
        [EnumMember]
        ScanWebAppCondition = 8589934592L,
        [EnumMember]
        ScanSiteCollectionCondition = 17179869184L,
        [EnumMember]
        ScanSiteCondition = 34359738368L,
        [EnumMember]
        ScanListCondition = 68719476736L,
        [EnumMember]
        ScanItemCondition = 137438953472L,
        [EnumMember]
        ScanPageCondition = 274877906944L
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ProfilePlanType
    {
        [EnumMember]
        AuditorMode = 0,
        [EnumMember]
        ScanMode,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum InheritAction
    {
        [EnumMember]
        None = 0,  //表示ApplyProfile(CAAction为Add)或RemoveProfile(CAAction为Remove)
        [EnumMember]
        Inherit, //点击Ribbon上的Inherit，用的CAAction为Remove，用于区别RemoveProfile
        [EnumMember]
        StopInherit,//点击StopInherit，用的CAAction为Add，用于区分ApplyProfile
    }

    #region [== 用于Server端判断选中节点应用profile和继承情况，进而初始化Create and Apply Profile页面 ==]

    public class ProfileInheritInfo
    {
        public ProfileInheritStatus Status { get; set; }

        public string ProfileId { get; set; }

        /// <summary>
        /// Inherit亮起逻辑：当前节点不是继承的，并且父节点有应用过的就亮起
        /// </summary>
        public string ParentProfileName { get; set; }

        public string Scope { get; set; }
    }

    public enum ProfileInheritStatus
    {
        /// <summary>
        /// 当前节点和所有父节点都未应用过Profile
        /// </summary>
        None,

        /// <summary>
        /// 当前节点应用了Profile
        /// </summary>
        CurrentApplied,

        /// <summary>
        /// 当前节点未应用Profile，某一级别父节点应用了
        /// </summary>
        Inherited,

        /// <summary>
        /// 当前节点打破继承
        /// </summary>
        StopInherited,

        /// <summary>
        /// 当前节点未应用Profile，某一级别父节点打破继承并且未应用Profile
        /// </summary>
        ParentStopInherited,
    }
    #endregion
}
