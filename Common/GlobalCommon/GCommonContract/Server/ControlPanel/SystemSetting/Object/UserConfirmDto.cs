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

namespace AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserConfirmDto
    {
        [DataMember]
        public string UserID { get; set; }
        [DataMember]
        [Obsolete("不提倡使用该属性")]
        public string UserName { get; set; }
        [DataMember]
        public ViewType ViewType { get; set; }
        [DataMember]
        public ConfirmType ConfirmType { get; set; }
        [DataMember]
        public ConfirmModule ConfirmModule { get; set; }
        [DataMember]
        public Dictionary<ConfirmType, List<ConfirmModule>> ConfirmData { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ViewType
    {
        [EnumMember]
        StandarView = 0,
        [EnumMember]
        BasicView = 1,

    }
    public enum ConfirmType
    {
        [EnumMember]
        Delete = 0,
        [EnumMember]
        Link = 1,
        [EnumMember]
        RunPlan = 2,
        [EnumMember]
        SavePlan = 3,
        [EnumMember]
        Continue = 4,
        [EnumMember]
        CauseError = 5
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ConfirmModule
    {
        [EnumMember]
        CentralAdmin = 0,
        [EnumMember]
        ContentManager,
        [EnumMember]
        Replicator,
        [EnumMember]
        DeploymentManager,
        [EnumMember]
        Compliance,
        [EnumMember]
        GranularBackup,
        [EnumMember]
        PlatformBackup,
        [EnumMember]
        ReportCenter,
        [EnumMember]
        Extender,
        [EnumMember]
        Archiver,
        [EnumMember]
        Connector,
        [EnumMember]
        AgentMonitor,
        [EnumMember]
        AgentGroups,
        [EnumMember]
        RemoteInstallation,
        [EnumMember]
        SearchDevice,
        [EnumMember]
        ManagerMonitor,
        [EnumMember]
        SystemPerformance,
        [EnumMember]
        LicenseManager,
        [EnumMember]
        PatchManager,
        [EnumMember]
        PatchReport,
        [EnumMember]
        AccountManager,
        [EnumMember]
        LanguageMapping,
        [EnumMember]
        UserMapping,
        [EnumMember]
        DomainMapping,
        [EnumMember]
        SystemRecovery,
        [EnumMember]
        SystemSettings,
        [EnumMember]
        CacheSettings,
        [EnumMember]
        RemoteWebApplications,
        [EnumMember]
        LogManager,
        [EnumMember]
        LogViewer,
        [EnumMember]
        MOMLoggingSettings,
        [EnumMember]
        UserNotificationSettings,
        [EnumMember]
        StorageManager,
    }
}
