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
using System.ComponentModel;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.PlatformRecovery.PRMaintenance;
using AvePoint.PlatformRecovery.PRSNMaintenance;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [KnownType(typeof(PRBrowseMessage))]
    [KnownType(typeof(PRBackupMessage))]
    [KnownType(typeof(PRRestoreMessage))]
    [KnownType(typeof(PRAgentMessage))]
    [KnownType(typeof(PRWFEBrowseMessage))]
    [KnownType(typeof(PRLiveModeBrowserMessage))]
    [KnownType(typeof(PRMultipleControlMessage))]
    [KnownType(typeof(PRMaintenanceMessage))]
    [KnownType(typeof(PRJobRetentionMessage))]
    [KnownType(typeof(PRSNMaintenanceMessage))]
    [KnownType(typeof(PRDisasterRecoveryMessage))]
    [KnownType(typeof(PRSNMigrationBrowseMessage))]
    [DataContract]
    public class PRMessage : AveMessage
    {
        [DataMember]
        public string JobId { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
        [DataMember]
        public MultipleAction Action { get; set; }
        [DataMember]
        public PRPlatformType PlatformType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MultipleAction
    {
        [EnumMember]
        Default = 0,
        [EnumMember]
        OffLine = 1,
        [EnumMember]
        ItemRestore = 2,
        [EnumMember]
        ManuallyRestore = 3,
        [EnumMember]
        AutoRetention = 4,
        [EnumMember]
        DbRetention = 5,
        [EnumMember]
        IndexRetention = 6,
        [EnumMember]
        MultipleIndex = 7,
        [EnumMember]
        JobPruning = 8,
        [EnumMember]
        DBJobPruning = 9,
        [EnumMember]
        GetDBPermission = 10,
        [EnumMember]
        VerifyJob = 11,
        [EnumMember]
        VerifyIndex = 12,
        [EnumMember]
        RestoreRawDatabase = 13,
        [EnumMember]
        Maintenance = 14,
		[EnumMember]
		SNMaintenance = 15,
        [EnumMember]
        VerifyFarmEnv = 16,//16-21 for Disaster Recovery
        [EnumMember]
        DisconnectFarm = 17,
        [EnumMember]
        ConfigurationInfo = 18,
        [EnumMember]
        ConfirmPassphrase = 19,
        [EnumMember]
        ConnectFarm = 20,
        [EnumMember]
        ProvisionInstance = 21,
        [EnumMember]
        DetachDB = 22,//22-24, for NetApp RestoreFromAlternateLocation
        [EnumMember]
        CheckOnline = 23,
        [EnumMember]
        ForceRestore = 24,
        [EnumMember] 
        SaveRestoreSetting = 25,
    }

    [DataContract]
    public enum ApplicationPoolUserConfig
    {
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        Predefined = 1,
        [EnumMember]
        Configurable = 2,
        [EnumMember]
        Both = 3,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PRPlatformType
    {
        [EnumMember]
        [Description("DocAve")]
        DocAve = 0,
        [EnumMember]
        [Description("SMSP")]
        NetApp = 1,
        [EnumMember]
        [Description("All")]
        All = 2,
    }

    [DataContract]
    public class PRBrowseMessage : PRMessage
    {
        [DataMember]
        public IList<ServiceDto> AgentList { get; set; }
        [DataMember]
        public PRBrowserContractBase BrowserContract { get; set; }
        /// <summary>
        /// add for extra operation when browse tree.
        /// 0 ----- normal browse
        /// 1 ----- check netapp lun after browse
        /// </summary>
        [DataMember]
        public int ExtraOperation = 0;
    }

    [DataContract]
    public class PRSNMigrationBrowseMessage : PRBrowseMessage
    {
        [DataMember]
        public List<PRSNMigrationInstanceInfo> InstanceInSelectedAgent { get; set; }
        [DataMember]
        public IList<string> LunsInSelectedAgent { get; set; }
        [DataMember]
        public bool IsDatabase { get; set; }
        [DataMember]
        public ServiceDto SelectedAgent { get; set; }
    }
    [DataContract]
    public class PRWFEBrowseMessage : PRBrowseMessage
    {
        [DataMember]
        public bool BrowseFEWListMark { get; set; }
        [DataMember]
        public string RestoreList { get; set; }
        [DataMember]
        public List<PRWFEObjectInfoDto> FEWList { get; set; }
    }

    [DataContract]
    public class PRJobStopMessage : PRMessage
    {
    }

    [DataContract]
    public class PRBackupBrowserContract : PRBrowserContractBase
    {
        [DataMember]
        public PRTreeNodeDto RootNode { get; set; }
    }

    [DataContract]
    public class PRRegisterManagedAccountContract : PRBrowserContractBase
    {
        [DataMember]
        public string AccountName { get; set; }
        [DataMember]
        public string AccountPassword { get; set; }
        [DataMember]
        public bool RegisterSucceeded { get; set; }
    }

    [DataContract]
    public class PROOPRestoreBrowserContract : PRBrowserContractBase
    {
        [DataMember]
        public PRDBOOPInfo DesDBServerInfo { get; set; }
        [DataMember]
        public List<PRWebAppOOPInfo> DesWebAppList { get; set; }
        [DataMember]
        public List<string> DesManagedAccountList { get; set; }
        [DataMember]
        public List<string> DesWebApplicationPoolList { get; set; }
        [DataMember]
        public List<string> DesServiceAppPoolList { get; set; }
        [DataMember]
        public List<PRServiceAppProxyOOPInfo> DesServiceAppProxyList { get; set; }
        [DataMember]
        public List<PRServiceAppOOPInfo> DesServiceAppList { get; set; }
        [DataMember]
        public List<string> DesWFEServerList { get; set; }
        [DataMember]
        public List<PRSearchServerOOPInfo> DesSearchServerList { get; set; }
        [DataMember]
        public string DesDefaultIndexLocation { get; set; }
        /// <summary>
        /// 由manager端设置DBServer,获得DB信息
        /// </summary>
        [DataMember]
        public string DBServerStr { get; set; }
        /// <summary>
        /// 用于Application Pool User配置选项，与Sharepoint保持一致
        /// </summary>
        [DataMember]
        public ApplicationPoolUserConfig AppPoolConfigurable { get; set; }
        [DataMember]
        public List<string> DesProfileSyncInstanceList { get; set; }
    }

    [DataContract]
    public class PRSiteCollectionBrowserContract : PRBrowserContractBase
    {
    }
    [DataContract]
    public class PRFBABrowserContract : PRBrowserContractBase
    {
        [DataMember]
        public PRTreeNodeDto FBANode { get; set; }
        [DataMember]
        public string RootNodeFullpath { get; set; }
    }

    [DataContract]
    public class PRSearchServiceApplicationBrowserContract : PRBrowserContractBase
    {
        [DataMember]
        public List<string> SearchServiceApplicationNamelist { get; set; }
    }

    [DataContract]
    public class PRStagingBrowserContract : PRBrowserContractBase
    {
        [DataMember]
        public PRStagingBrowserType MessageType { get; set; }
        [DataMember]
        public List<string> InstanceList { get; set; }
        [DataMember]
        public PRStagingTestInfo TestInfo { get; set; }
        [DataMember]
        public List<PRStagingInstanceInfo> InstanceInfoList { get; set; }
        [DataMember]
        public PRStagingPolicyDto StagingPolicy { get; set; }
        [DataMember]
        public List<ServiceDto> AgentList { get; set; }
        [DataMember]
        public bool IsCurrentActive { get; set; }
        [DataMember]
        public bool AvailableSpacePassed { get; set; }
        [DataMember]
        public bool DataLocationPassed { get; set; }
        [DataMember]
        public bool LogLocationPassed { get; set; }
    }

    [DataContract]
    public class PRFastSearchBrowserContract : PRBrowserContractBase
    {
        [DataMember]
        public PRTreeNodeDto RootNode { get; set; }
    }

    [DataContract]
    public class PRCustomDatabaseBrowserContract : PRBrowserContractBase
    {
        [DataMember]
        public List<PRCustomDatabaseInstance> InstanceList { get; set; }
        [DataMember]
        public PRTreeNodeDto RootNode { get; set; }
    }
    [DataContract]
    public class PRCustomDatabaseInstance
    {
        [DataMember]
        public string InstanceName { get; set; }
        [DataMember]
        public string DNSName { get; set; }
        [DataMember]
        public List<string> ClusterNodes { get; set; }
        [DataMember]
        public List<PRTreeNodeDto> Databases { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
        /// <summary>
        /// True is load instance databases successfully
        /// </summary>
        [DataMember]
        public bool IsLoadDatabasesSuccess { get; set; }

        [DataMember]
        public bool IsLoadDatabasesFailed { get; set; }
    }
    [DataContract]
    public class PRAgentVolumeInfoContract : PRBrowserContractBase
    {
        [DataMember]
        public string Agent { get; set; }
        [DataMember]
        public string ResultXml { get; set; }
    }
    [DataContract]
    [KnownType(typeof(PRVssSnapShotDto))]
    [KnownType(typeof(PRTreeNodeDto))]
    public class PRAgentSnapshotInfoContract : PRBrowserContractBase
    {
        [DataMember]
        public string Agent { get; set; }
        [DataMember]
        public List<PRTreeNodeDto> TreeNodeDtoList { get; set; }
        [DataMember]
        public Dictionary<string, PRVolumeSnapshotInfo> Result { get; set; }
    }
    [DataContract]
    public class PRVolumeSnapshotInfo
    {
        [DataMember]
        public int MaxSnapshotCount { get; set; }
        [DataMember]
        public List<PRVssSnapShotDto> Snapshots { get; set; }
    }
    [DataContract]
    public struct PRStagingInstanceInfo
    {
        [DataMember]
        public string InstanceName { get; set; }
        [DataMember]
        public bool IsClustered { get; set; }
        [DataMember]
        public List<string> PhysicalNodeStringList { get; set; }
        [DataMember]
        public List<ServiceDto> PhysicalNodeServiceList { get; set; }
        [DataMember]
        public string InstanceDefaultLocation { get; set; }
        [DataMember]
        public string InstanceDefaultLogLocation { get; set; }
    }
    [DataContract]
    public class PRStagingTestInfo
    {
        [DataMember]
        public string InstanceName { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public bool TestResult { get; set; }
        [DataMember]
        public string TestErrorMessage { get; set; }
    }

    [DataContract]
    public enum PRStagingBrowserType
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        GetInstanceList = 0,
        [EnumMember]
        CheckSQLAuthentication = 1,
        [EnumMember]
        GetSQLDefaultFileLocation = 2,
        [EnumMember]
        CheckIsCurrentActive = 3,
        [EnumMember]
        CheckFileLocationAndSpace = 4,
    }

    [KnownType(typeof(PRBackupBrowserContract))]
    [KnownType(typeof(PROOPRestoreBrowserContract))]
    [KnownType(typeof(PRSiteCollectionBrowserContract))]    
    [KnownType(typeof(PRStagingBrowserContract))]
    [KnownType(typeof(PRAgentVolumeInfoContract))]
    [KnownType(typeof(PRAgentSnapshotInfoContract))]
    [KnownType(typeof(PRCustomDatabaseBrowserContract))]
    [KnownType(typeof(PRFastSearchBrowserContract))]
    [KnownType(typeof(PRSearchServiceApplicationBrowserContract))]
    [KnownType(typeof(PRFastSearchBrowserContract))]
    [KnownType(typeof(PRRegisterManagedAccountContract))]
    [KnownType(typeof(PRFBABrowserContract))]
    [DataContract]
    public class PRBrowserContractBase
    {
    }

    /// <summary>
    /// 这个类是agent和agent之间相互传递消息的，主要用于control agent发给member agent起进程用
    /// </summary>
    [DataContract]
    public class PRAgentMessage : PRMessage
    {
        [DataMember]
        public string MemberProcessName { get; set; }
        [DataMember]
        public string MessageObject { get; set; }
        [DataMember]
        public PRItemMessage ItemMessage { get; set; }
    }
    [DataContract]
    public class PRSNMigrationInstanceInfo
    {
        [DataMember]
        public List<PRSMigrationDBAndIndexItemInfo> ObjectList { get; set; }
        [DataMember]
        public string InstanceName { get; set; } 
        [DataMember]
        public string SingleSnapinfoLocation { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public List<string> Luns { get; set; }
        [DataMember]
        public List<string> LunsForSnapInfo { get; set; }
    }
    [DataContract]
    public class PRSMigrationDBAndIndexItemInfo
    {
        [DataMember]
        public PRTreeNodeDto ItemNode { get; set; }
        [DataMember]
        public string LocationTo { get; set; }
        [DataMember]
        public string LocationFrom { get; set; }

    }
}
