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
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BlobProviderContract : SONodeContract<BlobProviderContract>
    {
        //数据库的InternalID主键
        [DataMember]
        public int ID { get; set; }
        //Provider Binary信息
        [DataMember]
        public List<BlobProviderBinary> BlobProviderBinaries { get; set; }
        //Stub Database设置
        [DataMember]
        public StubDatabaseInfo StubDatabase { get; set; }
        //SharePoint上该节点的EBS设置(Manager向Client请求时，Client通过该属性将状态标明状态)
        [DataMember]
        public ActionStatus SPEBSStatus { get; set; }
        //GUI上选选择的状态(只有Farm节点，这个属性有用，如果页面上选择的Enalbe/Disable依赖这个属性)
        [DataMember]
        public ActionStatus EBSStatus { get; set; }
        //SharePoint上该节点的RBS设置(Manager向Client请求时，Client通过该属性将状态标明状态)
        [DataMember]
        public ActionStatus SPRBSStatus { get; set; }
        //GUI上选择的状态(在RBS情况下并且该节点是ContentDB时有效，即记录当前节点的CheckBox状态，并且在安装RBS时Manager通过这个属性通知Client，当前节点是否安装/卸载RBS)
        [DataMember]
        public ActionStatus RBSStatus { get; set; }
        //RBS情况下，WebApp节点有效，GUI上选择的状态(在安装RBS时Manager通过这个属性通知Client，当前webapp的IncludeNew状态)
        [DataMember]
        public ActionStatus IncludeNewStatus { get; set; }
        [DataMember]
        public ScheduleDto Schedule { get; set; }
        [DataMember]
        public bool RBSRunNow { get; set; }
        [DataMember]
        public BinaryStatus BinaryStatus { get; set; }
        [DataMember]
        public Dictionary<string,long> StubDBHistory { get; set; }
        /// <summary>
        /// 记录的更新时间
        /// </summary>
        [DataMember]
        public long UpdateTime { get; set; }

        /// <summary>
        /// 区分数据来自Wizard还是Schedule
        /// </summary>
        [DataMember]
        public Source Source { get; set; }

        /// <summary>
        /// SharePoint Configration Database Address
        /// </summary>
        [DataMember]
        public string DatabaseMachineAddress { get; set; }

        [DataMember]
        public Extension Extension { get; set; }

        /// <summary>
        /// 数据升级用来页面显示, 真正的值存在Extension中
        /// </summary>
        [DataMember]
        public string LogicalDeviceId { set; get; }
        /// <summary>
        /// 页面显示用; 不存数据库
        /// </summary>
        [DataMember]
        public string DatabaseName { set; get; }

        public BlobProviderContract Clone() 
        {
            BlobProviderContract cloned = new BlobProviderContract();

            cloned.NodeId = this.NodeId;
            cloned.NodeName = this.NodeName;
            cloned.NodeLevel = this.NodeLevel;
            cloned.ParentNode = this.ParentNode;
            cloned.ParentNodeId = this.ParentNodeId;
            if (this.Children != null)
            {
                cloned.Children = new List<BlobProviderContract>(this.Children.Count);
                this.Children.ForEach(i => cloned.Children.Add(i.Clone()));
            }
            cloned.FarmId = this.FarmId;
            cloned.SPVersion = this.SPVersion;

            cloned.ID = this.ID;
            if (this.BlobProviderBinaries != null)
            {
                cloned.BlobProviderBinaries = new List<BlobProviderBinary>();
                this.BlobProviderBinaries.ForEach(i => cloned.BlobProviderBinaries.Add(i.Clone()));
            }
            if (this.StubDatabase != null)
            {
                cloned.StubDatabase = this.StubDatabase.Clone();
            }
            cloned.SPEBSStatus = this.SPEBSStatus;
            cloned.EBSStatus = this.EBSStatus;
            cloned.SPRBSStatus = this.SPRBSStatus;
            cloned.RBSStatus = this.RBSStatus;
            cloned.IncludeNewStatus = this.IncludeNewStatus;
            if (this.Schedule != null)
            {
                cloned.Schedule = new ScheduleDto();
                cloned.Schedule.Id = this.Schedule.Id;
                cloned.Schedule.PlanId = this.Schedule.PlanId;
                cloned.Schedule.Interval = this.Schedule.Interval;
                cloned.Schedule.IntervalType = this.Schedule.IntervalType;
                cloned.Schedule.StartTime = this.Schedule.StartTime;
                cloned.Schedule.TimeZoneId = this.Schedule.TimeZoneId;
            }
            cloned.RBSRunNow = this.RBSRunNow;
            return cloned;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BlobProviderBinary
    {
        //数据库主键
        [XmlIgnoreAttribute]
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string ServerName { get; set; }
        [DataMember]
        public List<string> ServerNames { get; set; }
        [DataMember]
        public List<string> Services { get; set; }
        [XmlIgnoreAttribute]
        [DataMember]
        public ServiceDto AgentInfo { get; set; }
        //EBS与RBS合到一起，有一个没有安装就是disabled，两个全部安装为enable(注意：07只有EBS，则只装了EBS，此属性也为enable)
        [DataMember]
        public ActionStatus BinaryStatus { get; set; }
        [XmlIgnoreAttribute]
        [DataMember]
        public FarmDto FarmDto { get; set; }

        public BlobProviderBinary Clone()
        {
            BlobProviderBinary cloned = new BlobProviderBinary();
            cloned.ID = this.ID;
            cloned.ServerName = this.ServerName;
            cloned.Services = this.Services;
            cloned.AgentInfo = this.AgentInfo;
            cloned.BinaryStatus = this.BinaryStatus;
            cloned.FarmDto = this.FarmDto;
            return cloned;
        }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Extension
    {
        /// <summary>
        /// 从client端传过来的DataBaseServer
        /// </summary>
        [DataMember]
        public string DataBaseMachineAddress { get; set; }

        [DataMember]
        public string UpgradeLogicalId { set; get; }

        [DataMember]
        public string UpgradeLogicalIdNetaApp { set; get; }

        [DataMember]
        public string UpgradeLogicalId3rdParty { set; get; }

        [DataMember]
        public long LastUpgradeTime { set; get; }

        [DataMember]
        public long LastUpgradeTimeForConnector { set; get; }

        [DataMember]
        public long LastUpgradeTimeNetApp { set; get; }

        [DataMember]
        public long LastUpgradeTimeForConnectorNetApp { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class StubDatabaseInfo
    {
        //数据库SOProfile主键
        [DataMember]
        public string ID { get; set; }
        /// <summary>
        /// 数据库服务机器
        /// </summary>
        [DataMember]
        public string DataBaseServer { get; set; }
        /// <summary>
        /// 数据库名称
        /// </summary>
        [DataMember]
        public string DataBaseName { get; set; }
        /// <summary>
        /// 登录身份验证方式
        /// </summary>
        [DataMember]
        public StubDBAuthentication Authentication { get; set; }
        /// <summary>
        /// 域
        /// </summary>
        [DataMember]
        public string Domain { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        [DataMember]
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [DataMember]
        public string Password { get; set; }
        /// <summary>
        /// 根据stubDB的信息拼成的链接字符串
        /// </summary>
        [DataMember]
        public string ConnectString { get; set; }
        /// <summary>
        /// 故障转移数据库
        /// </summary>
        //[DataMember]
        //public StubDatabaseInfo FailoverDatabase { get; set; }

        /// <summary>
        /// Failover Database Server
        /// </summary>
        [DataMember]
        public string FailoverPartner { get; set; }

        public StubDatabaseInfo Clone()
        {
            StubDatabaseInfo cloned = new StubDatabaseInfo();
            cloned.ID = this.ID;
            cloned.DataBaseServer = this.DataBaseServer;
            cloned.DataBaseName = this.DataBaseName;
            cloned.Authentication = this.Authentication;
            cloned.Domain = this.Domain;
            cloned.UserName = this.UserName;
            cloned.Password = this.Password;
            cloned.FailoverPartner = this.FailoverPartner;
            //if (this.FailoverDatabase != null)
            //{ 
            //    cloned.FailoverDatabase = this.FailoverDatabase.Clone();
            //}
            return cloned;
        }
    }

    #region --- 枚举 ---
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BlobProviderType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        EBS = 1,
        [EnumMember]
        RBS = 2,
        [EnumMember]
        ALL = 3
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum Source
    {
        [EnumMember]
        Wizard = 0,
        [EnumMember]
        Schedule = 1
        
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ProviderType
    {
        [EnumMember]
        NONE = 0,
        [EnumMember]
        EBS = 1,
        [EnumMember]
        RBS = 2,
        [EnumMember]
        ALL = 3
    }

    /// <summary>
    /// 1.表示节点状态时，Disable为CheckBox没有选中，包括IncludeNew
    /// 2.表示节点EBS/RBS状态
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ActionStatus
    {
        [EnumMember]
        Disable = 0,
        [EnumMember]
        Enable = 1,
        [EnumMember]
        Not_Collected = -1
    }

    [DataContract]
    public enum StubDBAuthentication
    {
        [EnumMember]
        Windows = 0,
        [EnumMember]
        SQL = 1
    }

    [DataContract]
    public enum BlobProviderColumn
    {
        [EnumMember]
        EBSStatus = 0,
        [EnumMember]
        RBSStatus = 1,
        [EnumMember]
        BlobProviderBinary = 2,
        [EnumMember]
        StubDBId = 3,
        [EnumMember]
        UpdateTime = 4,
        [EnumMember]
        NodeStatus = 5,
        [EnumMember]
        StubDBHistory = 6,
        [EnumMember]
        Extension = 7
    }
    
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EBSAction
    {
        [EnumMember]
        DISABLE = 0,
        [EnumMember]
        ENABLE = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BinaryStatus
    {
        [EnumMember]
        Not_Install = 0,
        [EnumMember]
        All_Install = 1,
        [EnumMember]
        Section_Install = 2
    }
    #endregion
}
