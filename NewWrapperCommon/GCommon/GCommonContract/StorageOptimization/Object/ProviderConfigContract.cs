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
using System.Text;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    #region --- SO功能节点设置 ---
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ProviderConfigContract : SONodeContract<ProviderConfigContract>
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public ModeStatus EBSStatus { get; set; }
        [DataMember]
        public ModeStatus RBSStatus { get; set; }
        [DataMember]
        public ProviderStatus EBSProviderStatus { get; set; }
        [DataMember]
        public ProviderStatus RBSProviderStatus { get; set; }

        [DataMember]
        public NodeStatus NodeStatus { get; set; }
        [DataMember]
        public int SPVersion { get; set; }
        // Provider属性
        [DataMember]
        public Dictionary<ProviderType, List<ServerInfo>> ProviderInfo { get; set; }
        // StubDB属性
        [DataMember]
        public string StubDBId { get; set; }
        [DataMember]
        public string StubDBHistory { get; set; }
        [DataMember]
        public Dictionary<int, string> StubDBVersion { get; set; }
        // 设置RBS时用，只有Farm级别有该属性
        [DataMember]
        public ScheduleDto Schedule { get; set; }
        [DataMember]
        public bool RunNow { get; set; }

        ///// <summary>
        ///// 更新数据库以前的StubDB的信息
        ///// </summary>
        //public string StubDBHistory { get; set; }
    }
    #endregion

    #region --- Provider ---
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ServerInfo
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string ServerName { get; set; }
        [DataMember]
        public List<string> Services { get; set; }
        [DataMember]
        public bool AgentInstalled { get; set; }
        [DataMember]
        public ProviderType ProviderType { get; set; }
        [DataMember]
        public InstallStatus InstallStatus { set; get; }
        [DataMember]
        public List<string> IPList { set; get; }
        [DataMember]
        public string FarmId { get; set; }
        [DataMember]
        public int SPVersion { get; set; }
    }
    #endregion

    #region --- SO欢迎页面 ---
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SOMainInfo
    {
        [DataMember]
        public string UpdateTimeStr { get; set; }
        [DataMember]
        public long UpdateTime { get; set; }
        [DataMember]
        public string TimeZoneId { get; set; }
        [DataMember]
        public List<ProviderConfigContract> FarmInfos { get; set; }
    }
    #endregion

    #region --- 枚举 ---
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum NodeStatus
    {
        [EnumMember]
        UNCHECK_NOT_INCLUDE_NEW = 0,
        [EnumMember]
        CHECK_NOT_INCLUDE_NEW = 1,
        [EnumMember]
        UNCHECK_INCLUDENEW = 2,
        [EnumMember]
        CHECK_INCLUDENEW = 3
    }

    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public enum ProviderType
    //{
    //    [EnumMember]
    //    NONE = 0,
    //    [EnumMember]
    //    EBS = 1,
    //    [EnumMember]
    //    RBS = 2,
    //    [EnumMember]
    //    ALL = 3
    //}

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum InstallStatus
    {
        [EnumMember]
        NOT_INSTALLED = 0,
        [EnumMember]
        INSTALLED = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ResultStatus
    {
        [EnumMember]
        SUCCESSFUL = 1,
        [EnumMember]
        FAILED = 0
    }

    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public enum EBSAction
    //{
    //    [EnumMember]
    //    DISABLE = 0,
    //    [EnumMember]
    //    ENABLE = 1
    //}

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ModeStatus
    {
        [EnumMember]
        NOT_COLLECTED = 0,
        [EnumMember]
        COLLECTING = 1,
        [EnumMember]
        DISABLED = 2,
        [EnumMember]
        DEPLOYING = 3,
        [EnumMember]
        ENABLE = 4
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ProviderStatus
    {
        [EnumMember]
        NOT_COLLECTED = 0,
        [EnumMember]
        COLLECTING = 1,
        [EnumMember]
        ALL_NOT_INSTALLED = 2,
        [EnumMember]
        ANY_INSTALLED = 3,
        [EnumMember]
        ALL_INSTALLED = 4
    }

    #endregion
}
