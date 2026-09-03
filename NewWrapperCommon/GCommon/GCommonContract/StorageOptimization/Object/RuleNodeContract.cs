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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RuleNodeContract : SONodeContract<RuleNodeContract>
    {
        /// <summary>
        /// Manager表key,对应rule alliance表nodeId字段.Agent端不使用.
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// 枚举RuleNodeType区分realtime为0和scheduled为1,archiver为2,index device为3
        /// </summary>
        [DataMember]
        public RuleNodeType Type { get; set; }

        [DataMember]
        public BposInfo BposInfo { get; set; }

        #region == For Scheduled And Archiver ==
        [DataMember]
        public string WebAppId { get; set; }

        [DataMember]
        public string WebAppUrl { get; set; }

        [DataMember]
        public string SiteId { get; set; }

        [DataMember]
        public string SiteUrl { get; set; }

        [DataMember]
        public string WebId { get; set; }

        [DataMember]
        public string ListId { get; set; }

        [DataMember]
        public string ListTitle { get; set; }

        /// <summary>
        /// 设置Archiver Index Setting的情况,存放Full Text Index的Profile Id.
        /// 逻辑修改目前存的是plan id
        /// </summary>
        [DataMember]
        public string PlanId { get; set; }

        /// <summary>
        /// 需要打破继承的nodes, MD5加密, 老版本Agent用(6.00, 6.01, GA)
        /// Vault还是使用该属性传SHA1信息(6.0.2+)
        /// </summary>
        [DataMember]
        //[Obsolete("请使用'BreakInheritNodesEncryptBySha1'")]
        public Dictionary<string, RuleNodeContract> BreakInheritNodes { get; set; }

        /// <summary>
        /// 需要打破继承的nodes, 使用SHA1加密, 新版本Agent用(6.10).
        /// </summary>
        [DataMember]
        public Dictionary<string, RuleNodeContract> BreakInheritNodesEncryptBySha1 { get; set; }

        /// <summary>
        /// 1.archiver site collection index device id
        /// 2.end user archiver setting id(setting id = profile id)
        /// </summary>
        [DataMember]
        public string IndexDeviceId { get; set; }

        [DataMember]
        public string LogicalDeviceName { get; set; }
        //for scheduled and archiver end

        /// <summary>
        /// 此node的provider type
        /// </summary>
        [DataMember]
        public BlobProviderType ProviderType { set; get; }

        /// <summary>
        /// Rule Manager Scope base显示
        /// </summary>
        [DataMember]
        public string AlliancedRuleNames { get; set; }

        /// <summary>
        /// Rule Manager Scope base显示
        /// </summary>
        [DataMember]
        public string ProfileName { get; set; }

        [DataMember]
        public List<NodeDetail> NodeDetails { get; set; }

        [DataMember]
        public string ParentInfo { get; set; }

        [DataMember]
        public RuleNodeExtension Extension { get; set; }

        /// <summary>
        /// node对应的RuleCollection，发给agent端做数据
        /// </summary>
        [DataMember]
        public RuleCollection RuleCollection { get; set; }

        /// <summary>
        /// 存储DirName，查打破继承子node使用
        /// </summary>
        [DataMember]
        public string FullPath { get; set; }
        #endregion

        #region == For Realtime ==
        /// <summary>
        /// 用于表示GUI页面上content db是否勾选
        /// </summary>
        [DataMember]
        public ActionStatus CheckStatus { get; set; }

        /// <summary>
        /// only for web application
        /// 枚举DiscoverNewType：0 none, 1 EBS, 2 RBS, 3 EBS and RBS
        /// Archiver GA+ Approval Mode: 0 auto, 1 manual;  占用DB中的字段, Contract中用ApprovalMode
        /// </summary>
        [DataMember]
        public int DiscoverNew { get; set; }

        /// <summary>
        /// Manager端不使用，Agent端生成xml文件时使用
        /// </summary>
        [DataMember]
        public string RealTimeRuleId { get; set; }

        /// <summary>
        /// realtime rule id list，Manager端将所有Enable的rule赋值给此属性
        /// </summary>
        [DataMember]
        public List<string> RealTimeRuleIds { get; set; }

        /// <summary>
        /// update time for farm level，Agent端根据此属性重新加载extender rules xml
        /// </summary>
        [DataMember]
        public long UpdateTime { get; set; }

        /// <summary>
        /// 用于site collection level
        /// </summary>
        [DataMember]
        public string ContentDBName { get; set; }

        /// <summary>
        /// Content db id for discover new. 用于site collection level
        /// </summary>
        [DataMember]
        public string ContentDBId { get; set; }

        /// <summary>
        /// 用于表示node是否有可用的rule，如果所有的rule都disable，则active为0
        /// </summary>
        [DataMember]
        public ActionStatus Active { get; set; }

        /// <summary>
        /// 表示Content database的Stub Database和RBS的配置情况
        /// realtime页面根据config state控制content database是否可以勾选
        /// </summary>
        [DataMember]
        public ConfigState ConfigState { get; set; }

        #endregion

        /// <summary>
        /// 记录通过什么路径apply的
        /// 目前只有GAO
        /// </summary>
        [DataMember]
        public ApplyInfo NodeInfo { get; set; }

        #region For GA+
        /// <summary>
        /// Archiver GA+ Approval Mode: 0 auto, 1 manual;  占用DB中的DiscoveryNew字段, 
        /// </summary>
        [DataMember]
        public int ApprovalMode { set; get; }
        #endregion

        [DataMember]
        public int LocationId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RuleNodeExtension
    {
        [DataMember]
        public string ContentDatabaseName { get; set; }

        [DataMember]
        public bool UsedcrawlProfile { get; set; }

        [DataMember]
        public bool IsUpgradeData { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NodeDetail
    {
        /// <summary>
        /// rule names apply on the node
        /// </summary>
        [DataMember]
        public List<string> RuleNames { get; set; }

        //[DataMember]
        //public string Type { get; set; }
        
        /// <summary>
        /// Rule Manager页面显示用
        /// </summary>
        [DataMember]
        public string ProfileName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ConfigState
    {
        /// <summary>
        /// 都没配置
        /// </summary>
        [EnumMember]
        None = 0,

        /// <summary>
        /// 配置了stub db
        /// </summary>
        [EnumMember]
        StubDatabase = 1,

        /// <summary>
        /// 配置了RBS
        /// </summary>
        [EnumMember]
        RBS = 2,

        /// <summary>
        /// 都配置了
        /// </summary>
        [EnumMember]
        All = 3,
    }

    [DataContract]
    public enum RuleNodeColumn
    {
        [EnumMember]
        ContentDBId = 0,
        [EnumMember]
        ParentNodeId = 1,
        [EnumMember]
        ParentNodeName = 2,
        [EnumMember]
        NodeName = 3,
        [EnumMember]
        DiscoveryNew = 4,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RuleNodeType
    {
        [EnumMember]
        RealTime = 0,
        [EnumMember]
        Scheduled = 1,
        [EnumMember]
        Archiver = 2,
        [EnumMember]
        IndexDevice = 3,
        [EnumMember]
        Connector = 4,
        [EnumMember]
        EndUserArchiverSetting = 5,
        [EnumMember]
        VaultIndexDevice = 6,
        [EnumMember]
        VaultSettingNode = 7,
        [EnumMember]
        V5ArchiveSiteMasterIndex = 8,
        [EnumMember]
        V5ProviderMapping = 9,
        [EnumMember]
        V5ExtenderIndexDevice = 10,
        //[EnumMember]
        //ArchiverDatabase = 11,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DiscoverNewType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        EBS = 1,
        [EnumMember]
        RBS = 2,
        [EnumMember]
        All = 3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ApplyInfo
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        Normal = 0,
        [EnumMember]
        GAO = 1
    }

}
