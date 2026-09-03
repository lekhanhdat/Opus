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
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRDisasterRecoveryMessage : PRMessage
    {
        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public string ControlAgentId { get; set; }

        [DataMember]
        public PRRestorePlanDto Plan { get; set; }

        [DataMember]
        public PRRestoreJobDto Job { get; set; }

        [DataMember]
        public string BackupJobId { get; set; }

        [DataMember]
        public ServiceDto MediaInfo { get; set; }

        [DataMember]
        public PlatformRestoreRequest ConfigForMedia { get; set; }

        [DataMember]
        public IList<ServiceDto> AgentList { get; set; }

        [DataMember]
        public List<ServerInfo> ServerList { get; set; }

        [DataMember]
        public ServerInfo ServerItem { get; set; }

        [DataMember]
        public PRRestoreMessage RestoreMessage { get; set; }

        [DataMember]
        public SPConfigurationInfo ConfigurationInfo { get; set; }

        [DataMember]
        public bool RestoreFromAlternateLocation { get; set; }

        [DataMember]
        public List<ComponentInfos> ComponentInfoList { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ServerInfo
    {
        [DataMember]
        public bool IsSelected { get; set; }
        [DataMember]
        public ServerStatus Status { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Address { get; set; }
        [DataMember]
        public string PSUrlPort { get; set; }
        [DataMember]
        public bool IsCentralAdminServer { get; set; }
        [DataMember]
        public bool NeedInstallWebApplicationContent { get; set; }
        [DataMember]
        public bool ClearCache { get; set; }
        [DataMember]
        public List<string> ServiceInstances { get; set; }
        [DataMember]
        public bool OperationSuccess { get; set; }
        [DataMember]
        public FarmRoles ServerRole { get; set; }
        [DataMember]
        public OperationType NextOperation { get; set; }
        [DataMember]
        public List<SyncServiceInstanceInfo> SyncServiceInstances { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ComponentInfos
    {
        [DataMember]
        public bool IsSelected { get; set; }
        [DataMember]
        public string ComponentName { get; set; }
        [DataMember]
        public string ServerName { get; set; }
        [DataMember]
        public PRNodeTypeId TypeId { get; set; }
        [DataMember]
        public string FullPath { get; set; }
        [DataMember]
        public string Comment { get; set; }
        [DataMember]
        public string SnapshotName { get; set; }
        [DataMember]
        public string OriginalLocation { get; set; }
        [DataMember]
        public int RestoreStatus { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SyncServiceInstanceInfo
    {
        [DataMember]
        public string ServerName { get; set; }
        [DataMember]
        public string ApplicationName { get; set; }
        [DataMember]
        public Guid ApplicationId { get; set; }
        [DataMember]
        public Guid SyncServiceInstanceId { get; set; }
        [DataMember]
        public string SyncServiceInstanceServerName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SPConfigurationInfo
    {
        [DataMember]
        public string ServerName { get; set; }
        [DataMember]
        public string ConfigDBName { get; set; }
        [DataMember]
        public string Port { get; set; }
        [DataMember]
        public string Passphrase { get; set; }
        [DataMember]
        public string CentralAdministrationURL { get; set; }
        [DataMember]
        public bool PassphraseKeyIsValid { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum Status: int
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        Success = 0,
        [EnumMember]
        Failed = 1,
        [EnumMember]
        Skipped = 2,
        [EnumMember]
        Filtered = 3,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum OperationType
    {
        [EnumMember]
        Verify = 0,
        [EnumMember]
        Disconnect = 1,
        [EnumMember]
        Connect = 2,
        [EnumMember]
        Success = 3,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ServerStatus
    {
        [EnumMember]
        Unknown = 0,
        [EnumMember]
        Disconnect = 1,
        [EnumMember]
        Connect = 2,
    }

}
