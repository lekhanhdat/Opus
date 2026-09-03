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



using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Attribute;

namespace AvePoint.GCommon.Contract.AveLicense
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ProductType
    {
        [EnumMember]
        [XmlEnum("DocAve")]
        DocAve,

        [EnumMember]
        [XmlEnum("NetApp")]
        NetApp,

        [EnumMember]
        [XmlEnum("NetApp_IBM")]
        NetApp_IBM,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ModuleName
    {
        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Archiver))]
        [XmlEnum("SO_Archiver2010")]
        SO_Archiver2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Connector))]
        [XmlEnum("SO_Connector2010")]
        SO_Connector2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Extender))]
        [XmlEnum("SO_RealTimeStorageManager2010")]
        SO_RealTimeStorageManager2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Extender))]
        [XmlEnum("SO_ScheduledStorageManager2010")]
        SO_ScheduledStorageManager2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.CentralAdmin))]
        [XmlEnum("AD_CentralAdmin2010")]
        AD_CentralAdmin2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.ContentManager))]
        [XmlEnum("AD_ContentManager2010")]
        AD_ContentManager2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.DeploymentManager))]
        [XmlEnum("AD_DeploymentManager2010")]
        AD_DeploymentManager2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Replicator))]
        [XmlEnum("AD_Replicator2010")]
        AD_Replicator2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GranularBackup))]
        [XmlEnum("DP_GranularBackup2010")]
        DP_GranularBackup2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.PlatformBackup))]
        [XmlEnum("DP_PlatformBackup2010")]
        DP_PlatformBackup2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCComplianceReports))]
        [XmlEnum("RC_Usage2010")]
        RC_Usage2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCInfrastructure))]
        [XmlEnum("RC_Infrastructure2010")]
        RC_Infrastructure2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCStorageOptimization))]
        [XmlEnum("RC_StorageOptimization2010")]
        RC_StorageOptimization2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCAdministration))]
        [XmlEnum("RC_Administration2010")]
        RC_Administration2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCComplianceReports))]
        [XmlEnum("RC_Customize2010")]
        RC_Customize2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCComplianceReports))]
        [XmlEnum("RC_AuditorReports2010")]
        RC_AuditorReports2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCRealtimeMonidtoring))]
        [XmlEnum("RC_RealtimeMonidtoring2010")]
        RC_RealtimeMonidtoring2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCActivityHistory))]
        [XmlEnum("RC_ActivityHistory2010")]
        RC_ActivityHistory2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SPMigration))]
        [XmlEnum("MG_SharePoint2007to2010")]
        MG_SharePoint2007to2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.FileMigration))]
        [XmlEnum("MG_FileSystemMigration2010")]
        MG_FileSystemMigration2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.NotesMigration))]
        [XmlEnum("MG_LotusNotesMigration2010")]
        MG_LotusNotesMigration2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.eRoomMigration))]
        [XmlEnum("MG_eRoomMigration2010")]
        MG_eRoomMigration2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.LivelinkMigration))]
        [XmlEnum("MG_LivelinkMigration2010")]
        MG_LivelinkMigration2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.PublicFolderMigration))]
        [XmlEnum("MG_PublicFolderMigration2010")]
        MG_PublicFolderMigration2010,

        [EnumMember]
        //TODO: Add GA+ Module
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GovernanceAutomation))]
        [XmlEnum("GA_GovernanceAutomation2010")]
        GA_GovernanceAutomation2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Vault))]
        [XmlEnum("CP_Vault2010")]
        CP_Vault2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.EDiscovery))]
        [XmlEnum("CP_eDiscovery2010")]
        CP_eDiscovery2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.ContentManager))]
        [XmlEnum("OF_ConenteManager2010")]
        OF_ConenteManager2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.CentralAdmin))]
        [XmlEnum("OF_Administration2010")]
        OF_Administration2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Replicator))]
        [XmlEnum("OF_Replicator2010")]
        OF_Replicator2010,

        [EnumMember]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GranularBackup))]
        [XmlEnum("OF_GranularBackup2010")]
        OF_GranularBackup2010,

    }

}
