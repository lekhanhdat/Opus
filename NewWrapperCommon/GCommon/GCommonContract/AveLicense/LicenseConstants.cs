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
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Attribute;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.AveModuleContract;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.AveLicense
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LicenseType
    {
        /// <summary>
        /// 随包附带的默认license，通常可使用30天，包括所有功能，每个功能30个Agent，Migration给1GB流量
        /// </summary>
        [EnumMember]
        [XmlEnum("Demo")]
        Demo = 0,

        /// <summary>
        /// 正式的license，通常无使用时间限制，如果其中某些功能有时间限制，则该license按照Enterprise + Demo看待。
        /// 包含的模块以及支持的Agent数目根据用户购买的情况生成。
        /// </summary>
        [EnumMember]
        [XmlEnum("Enterprise")]
        Enterprise = 1,

        /// <summary>
        /// 混合式license，这种license的Units可能包含Enterprise或Demo其中的一种。
        /// </summary>
        [EnumMember]
        [XmlEnum("EnterpriseAndDemo")]
        EnterpriseAndDemo = 2,

        /// <summary>
        /// 用于开发目的，非出售类型的license。
        /// </summary>
        [EnumMember]
        [XmlEnum("Development_NonProduction")]
        Development_NonProduction = 3,
    }

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

        [EnumMember]
        [XmlEnum("ComplianceGuardian")]
        ComplianceGuardian,
    }

    public class LicenseConstants
    {
        public static readonly Dictionary<string, Dictionary<LicenseModuleType, IList<ModuleName>>> ModuleNameDictionary = new Dictionary<string, Dictionary<LicenseModuleType, IList<ModuleName>>>() 
        { 
            { 
                ModuleContract.DocAvePlatform.Migration.SPMigration.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2007, new List<ModuleName>(){ ModuleName.MG_SharePoint2007to2010, ModuleName.MG_SharePoint2007to2013 } },
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.MG_SharePoint2007to2010, ModuleName.MG_SharePoint2010to2013 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.MG_SharePoint2007to2013, ModuleName.MG_SharePoint2010to2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.MG_SharePoint2007to2016, ModuleName.MG_SharePoint2010to2016,ModuleName.MG_SharePoint2013to2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.MG_SharePoint2007to2019, ModuleName.MG_SharePoint2010to2019, ModuleName.MG_SharePoint2013to2019, ModuleName.MG_SharePoint2016to2019 } },
                    { LicenseModuleType.Office365SharePoint2010, new List<ModuleName>(){ ModuleName.MG_SharePoint2010toRemoteFarm2013 } },
                    { LicenseModuleType.Office365SharePoint2013, new List<ModuleName>(){ ModuleName.MG_SharePoint2007toRemoteFarm2013, ModuleName.MG_SharePoint2010toRemoteFarm2013, ModuleName.MG_SharePoint2013toRemoteFarm2016, ModuleName.MG_SharePoint2016toRemoteFarm2019 } },
                    { LicenseModuleType.Office365SharePoint2016, new List<ModuleName>(){ ModuleName.MG_SharePoint2013toRemoteFarm2016, ModuleName.MG_SharePoint2016toRemoteFarm2019 } },
                    { LicenseModuleType.Office365SharePoint2019, new List<ModuleName>(){ ModuleName.MG_SharePoint2016toRemoteFarm2019 } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.Migration.NotesMigration.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.MG_LotusNotesMigration2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.MG_LotusNotesMigration2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.MG_LotusNotesMigration2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.MG_LotusNotesMigration2019 } },
                    { LicenseModuleType.Office365SharePoint2010, new List<ModuleName>(){ ModuleName.MG_LotusNotesMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2013, new List<ModuleName>(){ ModuleName.MG_LotusNotesMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2016, new List<ModuleName>(){ ModuleName.MG_LotusNotesMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2019, new List<ModuleName>(){ ModuleName.MG_LotusNotesMigrationOnline } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.Migration.FileMigration.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.MG_FileSystemMigration2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.MG_FileSystemMigration2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.MG_FileSystemMigration2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.MG_FileSystemMigration2019 } },
                    { LicenseModuleType.Office365SharePoint2010, new List<ModuleName>(){ ModuleName.MG_FileSystemMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2013, new List<ModuleName>(){ ModuleName.MG_FileSystemMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2016, new List<ModuleName>(){ ModuleName.MG_FileSystemMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2019, new List<ModuleName>(){ ModuleName.MG_FileSystemMigrationOnline } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.Migration.LivelinkMigration.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.MG_LivelinkMigration2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.MG_LivelinkMigration2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.MG_LivelinkMigration2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.MG_LivelinkMigration2019 } },
                    { LicenseModuleType.Office365SharePoint2010, new List<ModuleName>(){ ModuleName.MG_LivelinkMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2013, new List<ModuleName>(){ ModuleName.MG_LivelinkMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2016, new List<ModuleName>(){ ModuleName.MG_LivelinkMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2019, new List<ModuleName>(){ ModuleName.MG_LivelinkMigrationOnline } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.Migration.eRoomMigration.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.MG_eRoomMigration2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.MG_eRoomMigration2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.MG_eRoomMigration2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.MG_eRoomMigration2019 } },
                    { LicenseModuleType.Office365SharePoint2010, new List<ModuleName>(){ ModuleName.MG_eRoomMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2013, new List<ModuleName>(){ ModuleName.MG_eRoomMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2016, new List<ModuleName>(){ ModuleName.MG_eRoomMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2019, new List<ModuleName>(){ ModuleName.MG_eRoomMigrationOnline } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.Migration.PublicFolderMigration.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.MG_PublicFolderMigration2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.MG_PublicFolderMigration2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.MG_PublicFolderMigration2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.MG_PublicFolderMigration2019 } },
                    { LicenseModuleType.Office365SharePoint2010, new List<ModuleName>(){ ModuleName.MG_PublicFolderMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2013, new List<ModuleName>(){ ModuleName.MG_PublicFolderMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2016, new List<ModuleName>(){ ModuleName.MG_PublicFolderMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2019, new List<ModuleName>(){ ModuleName.MG_PublicFolderMigrationOnline } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.Migration.DocumentumMigration.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.MG_DocumentumMigration2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.MG_DocumentumMigration2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.MG_DocumentumMigration2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.MG_DocumentumMigration2019 } },
                    { LicenseModuleType.Office365SharePoint2010, new List<ModuleName>(){ ModuleName.MG_DocumentumMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2013, new List<ModuleName>(){ ModuleName.MG_DocumentumMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2016, new List<ModuleName>(){ ModuleName.MG_DocumentumMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2019, new List<ModuleName>(){ ModuleName.MG_DocumentumMigrationOnline } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.Migration.QuickPlaceMigration.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.MG_QuickPlaceMigration2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.MG_QuickPlaceMigration2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.MG_QuickPlaceMigration2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.MG_QuickPlaceMigration2019 } },
                    { LicenseModuleType.Office365SharePoint2010, new List<ModuleName>(){ ModuleName.MG_QuickPlaceMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2013, new List<ModuleName>(){ ModuleName.MG_QuickPlaceMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2016, new List<ModuleName>(){ ModuleName.MG_QuickPlaceMigrationOnline } },
                    { LicenseModuleType.Office365SharePoint2019, new List<ModuleName>(){ ModuleName.MG_QuickPlaceMigrationOnline } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.DataProtection.GranularBackup.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.DP_GranularBackup2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.DP_GranularBackup2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.DP_GranularBackup2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.DP_GranularBackup2019 } },
                    { LicenseModuleType.Office365SharePoint2010, new List<ModuleName>(){ ModuleName.OF_GranularBackupOnline } },
                    { LicenseModuleType.Office365SharePoint2013, new List<ModuleName>(){ ModuleName.OF_GranularBackupOnline } },
                    { LicenseModuleType.Office365SharePoint2016, new List<ModuleName>(){ ModuleName.OF_GranularBackupOnline } },
                    { LicenseModuleType.Office365SharePoint2019, new List<ModuleName>(){ ModuleName.OF_GranularBackupOnline } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.DataProtection.PlatformBackup.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.DP_PlatformBackup2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.DP_PlatformBackup2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.DP_PlatformBackup2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.DP_PlatformBackup2019 } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.DataProtection.PlatformBackupForSMSP.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.DP_PlatformBackupForSMSP2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.DP_PlatformBackupForSMSP2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.DP_PlatformBackupForSMSP2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.DP_PlatformBackupForSMSP2019 } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.DataProtection.SqlServerManager.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.DP_SQLServerDataManager2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.DP_SQLServerDataManager2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.DP_SQLServerDataManager2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.DP_SQLServerDataManager2019 } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.Administration.CentralAdmin.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.AD_CentralAdmin2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.AD_CentralAdmin2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.AD_CentralAdmin2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.AD_CentralAdmin2019 } },
                    { LicenseModuleType.Office365SharePoint2010, new List<ModuleName>(){ ModuleName.OF_AdministrationOnline } },
                    { LicenseModuleType.Office365SharePoint2013, new List<ModuleName>(){ ModuleName.OF_AdministrationOnline } },
                    { LicenseModuleType.Office365SharePoint2016, new List<ModuleName>(){ ModuleName.OF_AdministrationOnline } },
                    { LicenseModuleType.Office365SharePoint2019, new List<ModuleName>(){ ModuleName.OF_AdministrationOnline } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.Administration.ContentManager.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.AD_ContentManager2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.AD_ContentManager2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.AD_ContentManager2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.AD_ContentManager2019 } },
                    { LicenseModuleType.Office365SharePoint2010, new List<ModuleName>(){ ModuleName.OF_ConenteManagerOnline } },
                    { LicenseModuleType.Office365SharePoint2013, new List<ModuleName>(){ ModuleName.OF_ConenteManagerOnline } },
                    { LicenseModuleType.Office365SharePoint2016, new List<ModuleName>(){ ModuleName.OF_ConenteManagerOnline } },
                    { LicenseModuleType.Office365SharePoint2019, new List<ModuleName>(){ ModuleName.OF_ConenteManagerOnline } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.Administration.DeploymentManager.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.AD_DeploymentManager2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.AD_DeploymentManager2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.AD_DeploymentManager2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.AD_DeploymentManager2019 } },
                    { LicenseModuleType.Office365SharePoint2010, new List<ModuleName>(){ ModuleName.OF_DeploymentManagerOnline } },
                    { LicenseModuleType.Office365SharePoint2013, new List<ModuleName>(){ ModuleName.OF_DeploymentManagerOnline } },
                    { LicenseModuleType.Office365SharePoint2016, new List<ModuleName>(){ ModuleName.OF_DeploymentManagerOnline } },
                    { LicenseModuleType.Office365SharePoint2019, new List<ModuleName>(){ ModuleName.OF_DeploymentManagerOnline } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.Administration.Replicator.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.AD_Replicator2010, ModuleName.AD_ReplicatorSecureGovEdition2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.AD_Replicator2013, ModuleName.AD_ReplicatorSecureGovEdition2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.AD_Replicator2016, ModuleName.AD_ReplicatorSecureGovEdition2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.AD_Replicator2019, ModuleName.AD_ReplicatorSecureGovEdition2019 } },
                    { LicenseModuleType.Office365SharePoint2010, new List<ModuleName>(){ ModuleName.OF_ReplicatorOnline, ModuleName.OF_ReplicatorSecureGovEditionOnline } },
                    { LicenseModuleType.Office365SharePoint2013, new List<ModuleName>(){ ModuleName.OF_ReplicatorOnline, ModuleName.OF_ReplicatorSecureGovEditionOnline } },
                    { LicenseModuleType.Office365SharePoint2016, new List<ModuleName>(){ ModuleName.OF_ReplicatorOnline, ModuleName.OF_ReplicatorSecureGovEditionOnline } },
                    { LicenseModuleType.Office365SharePoint2019, new List<ModuleName>(){ ModuleName.OF_ReplicatorOnline, ModuleName.OF_ReplicatorSecureGovEditionOnline } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.Compliance.EDiscovery.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.CP_eDiscovery2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.CP_eDiscovery2013 } },
                    //{ LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.CP_eDiscovery2016 } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.Compliance.Vault.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.CP_Vault2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.CP_Vault2013 } },
                }
            },
            { 
                ModuleContract.DocAvePlatform.ReportCenter.RCUsage.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.RC_Usage2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.RC_ComplianceReports2013 } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.ReportCenter.RCInfrastructure.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.RC_Infrastructure2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.RC_Infrastructure2013 } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.ReportCenter.RCAdministration.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.RC_Administration2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.RC_Administration2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.RC_Administration2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.RC_Administration2019 } },
                    { LicenseModuleType.Office365SharePoint2010, new List<ModuleName>(){ ModuleName.OF_AdministrationReportsOnline } },
                    { LicenseModuleType.Office365SharePoint2013, new List<ModuleName>(){ ModuleName.OF_AdministrationReportsOnline } },
                    { LicenseModuleType.Office365SharePoint2016, new List<ModuleName>(){ ModuleName.OF_AdministrationReportsOnline } },
                    { LicenseModuleType.Office365SharePoint2019, new List<ModuleName>(){ ModuleName.OF_AdministrationReportsOnline } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.ReportCenter.RCComplianceReports.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.RC_Usage2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.RC_ComplianceReports2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.RC_ComplianceReports2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.RC_ComplianceReports2019 } },
                    { LicenseModuleType.Office365SharePoint2010, new List<ModuleName>(){ ModuleName.OF_ComplianceReportsOnline } },
                    { LicenseModuleType.Office365SharePoint2013, new List<ModuleName>(){ ModuleName.OF_ComplianceReportsOnline } },
                    { LicenseModuleType.Office365SharePoint2016, new List<ModuleName>(){ ModuleName.OF_ComplianceReportsOnline } },
                    { LicenseModuleType.Office365SharePoint2019, new List<ModuleName>(){ ModuleName.OF_ComplianceReportsOnline } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.ReportCenter.RCAuditorReports.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.RC_AuditorReports2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.RC_AuditorReports2013 } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.ReportCenter.RCCustomize.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.RC_Customize2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.RC_Customize2013 } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.StorageOptimization.Extender.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.SO_RealTimeStorageManager2010, ModuleName.SO_ScheduledStorageManager2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.SO_RealTimeStorageManager2013, ModuleName.SO_ScheduledStorageManager2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.SO_RealTimeStorageManager2016, ModuleName.SO_ScheduledStorageManager2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.SO_RealTimeStorageManager2019, ModuleName.SO_ScheduledStorageManager2019 } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.StorageOptimization.RealTime.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.SO_RealTimeStorageManager2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.SO_RealTimeStorageManager2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.SO_RealTimeStorageManager2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.SO_RealTimeStorageManager2019 } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.StorageOptimization.ExtenderSchedule.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.SO_ScheduledStorageManager2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.SO_ScheduledStorageManager2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.SO_ScheduledStorageManager2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.SO_ScheduledStorageManager2019 } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.StorageOptimization.Connector.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.SO_Connector2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.SO_Connector2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.SO_Connector2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.SO_Connector2019 } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.StorageOptimization.BoxConnector.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.SO_CloudConnect2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.SO_CloudConnect2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.SO_CloudConnect2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.SO_CloudConnect2019 } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.StorageOptimization.Archiver.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.SharePoint2010, new List<ModuleName>(){ ModuleName.SO_Archiver2010 } },
                    { LicenseModuleType.SharePoint2013, new List<ModuleName>(){ ModuleName.SO_Archiver2013 } },
                    { LicenseModuleType.SharePoint2016, new List<ModuleName>(){ ModuleName.SO_Archiver2016 } },
                    { LicenseModuleType.SharePoint2019, new List<ModuleName>(){ ModuleName.SO_Archiver2019 } },
                    { LicenseModuleType.Office365SharePoint2010, new List<ModuleName>(){ ModuleName.OF_ArchiverOnline } },
                    { LicenseModuleType.Office365SharePoint2013, new List<ModuleName>(){ ModuleName.OF_ArchiverOnline } },
                    { LicenseModuleType.Office365SharePoint2016, new List<ModuleName>(){ ModuleName.OF_ArchiverOnline } },
                    { LicenseModuleType.Office365SharePoint2019, new List<ModuleName>(){ ModuleName.OF_ArchiverOnline } },
                } 
            },
            { 
                ModuleContract.DocAvePlatform.DataProtection.VMManagement.Name, 
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.VM, new List<ModuleName>(){ ModuleName.DP_VMBackup } },
                } 
            },
             {
                ModuleContract.DocAvePlatform.StorageOptimization.FSArchiver.Name,
                new Dictionary<LicenseModuleType, IList<ModuleName>>()
                {
                    { LicenseModuleType.FSArchive, new List<ModuleName>(){ ModuleName.SO_FSArchiver } },
                }
            },
        };

        #region LicenseService Version Check
        public const string Current_GA_Version = "GA_DocAve_6.8.0";//废弃该方式，用LicenseVersionType枚举判断
        public const string Current_API_Version = "API_DocAve_6.8.0";//当前版本

        public const string GA_DocAve_622 = "GA_1.3.0";
        public const string API_DocAve_622 = "API_6.2.2";
        public const string GA_DocAve_630 = "GA_1.4.0";
        public const string API_DocAve_630 = "API_6.3.0";
        public const string GA_DocAve_640 = "GA_DocAve_6.4.0";
        public const string API_DocAve_640 = "API_DocAve_6.4.0";

        public static readonly Dictionary<string, int> LicenseVersionDictionary = new Dictionary<string, int>()
        {
            { GA_DocAve_622,(int)LicenseVersionType.DocAve622},
            { API_DocAve_622,(int)LicenseVersionType.DocAve622},
            { GA_DocAve_630,(int)LicenseVersionType.DocAve63},
            { API_DocAve_630,(int)LicenseVersionType.DocAve63},
            { GA_DocAve_640,(int)LicenseVersionType.DocAve64},
            { API_DocAve_640,(int)LicenseVersionType.DocAve64},
            { "GA_DocAve_6.5.0",(int)LicenseVersionType.DocAve65},
            { "API_DocAve_6.5.0",(int)LicenseVersionType.DocAve65},
            { "GA_DocAve_6.6.0",(int)LicenseVersionType.DocAve66},
            { "API_DocAve_6.6.0",(int)LicenseVersionType.DocAve66},
            { "GA_DocAve_6.7.0",(int)LicenseVersionType.DocAve67},
            { "API_DocAve_6.7.0",(int)LicenseVersionType.DocAve67},
            { "GA_DocAve_6.8.0",(int)LicenseVersionType.DocAve68},
            { "API_DocAve_6.8.0",(int)LicenseVersionType.DocAve68},
            { "GA_DocAve_6.11.0",(int)LicenseVersionType.DocAve6110},
            { "API_DocAve_6.11.0",(int)LicenseVersionType.DocAve6110},
        };

        #endregion

    }

    public enum LicenseModuleType
    {
        None,
        SharePoint2007,
        SharePoint2010,
        SharePoint2013,
        Office365SharePoint2010,
        Office365SharePoint2013,
        VM,
        SharePoint2016,
        Office365SharePoint2016,
        FSArchive,
        SharePoint2019,
        Office365SharePoint2019,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ModuleName
    {
        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Archiver),
            LicFileConstants = "Archiver 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("SO_Archiver2010")]
        SO_Archiver2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Connector),
            LicFileConstants = "Connector 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("SO_Connector2010")]
        SO_Connector2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Extender),
            LicFileConstants = "Realtime Storage Manager 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("SO_RealTimeStorageManager2010")]
        SO_RealTimeStorageManager2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Extender),
            LicFileConstants = "Scheduled Storage Manager 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("SO_ScheduledStorageManager2010")]
        SO_ScheduledStorageManager2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.CentralAdmin),
            LicFileConstants = "Central Admin 2010", ExtensionFileConstants = "Policy Enforcer 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("AD_CentralAdmin2010")]
        AD_CentralAdmin2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.ContentManager),
            LicFileConstants = "Content Manager 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("AD_ContentManager2010")]
        AD_ContentManager2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.DeploymentManager),
            LicFileConstants = "Deployment Manager 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("AD_DeploymentManager2010")]
        AD_DeploymentManager2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Replicator),
            LicFileConstants = "Replicator 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("AD_Replicator2010")]
        AD_Replicator2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GranularBackup),
            LicFileConstants = "Granular Backup 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("DP_GranularBackup2010")]
        DP_GranularBackup2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.PlatformBackup),
            LicFileConstants = "Platform Backup 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("DP_PlatformBackup2010")]
        DP_PlatformBackup2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCComplianceReports),
            LicFileConstants = "Usage 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("RC_Usage2010")]
        RC_Usage2010,//(Usage,Customize,Auditor)=>(Compliance Report)

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCInfrastructure),
            LicFileConstants = "Infrastructure 2010")]
        [XmlEnum("RC_Infrastructure2010")]
        RC_Infrastructure2010,//Hide in Gui

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCStorageOptimization),
            LicFileConstants = "Storage Optimization 2010")]
        [XmlEnum("RC_StorageOptimization2010")]
        RC_StorageOptimization2010,//Hide in Gui

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCAdministration),
            LicFileConstants = "Administration 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("RC_Administration2010")]
        RC_Administration2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCComplianceReports),
            LicFileConstants = "Customize 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("RC_Customize2010")]
        RC_Customize2010,//(Usage,Customize,Auditor)=>(Compliance Report)

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCComplianceReports),
            LicFileConstants = "Auditor Reports 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("RC_AuditorReports2010")]
        RC_AuditorReports2010,//(Usage,Customize,Auditor)=>(Compliance Report)

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCRealtimeMonidtoring),
            LicFileConstants = "Real-time Monitoring 2010")]
        [XmlEnum("RC_RealtimeMonidtoring2010")]
        RC_RealtimeMonidtoring2010,//Hide in Gui

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve60)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCActivityHistory),
            LicFileConstants = "Activity History 2010")]
        [XmlEnum("RC_ActivityHistory2010")]
        RC_ActivityHistory2010,//Hide in Gui

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve61)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SPMigration),
            LicFileConstants = "SharePoint 2007 to 2010", MGProfileType = ProfileType.SP07To10Quantity)]
        [XmlEnum("MG_SharePoint2007to2010")]
        MG_SharePoint2007to2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve61)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.FileMigration),
            LicFileConstants = "File System Migration 2010", MGProfileType = ProfileType.FileQuantity)]
        [XmlEnum("MG_FileSystemMigration2010")]
        MG_FileSystemMigration2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve61)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.NotesMigration),
            LicFileConstants = "Lotus Notes Migration 2010", MGProfileType = ProfileType.NotesQuantity)]
        [XmlEnum("MG_LotusNotesMigration2010")]
        MG_LotusNotesMigration2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve61)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.eRoomMigration),
            LicFileConstants = "eRoom Migration 2010", MGProfileType = ProfileType.eRoomQuantity)]
        [XmlEnum("MG_eRoomMigration2010")]
        MG_eRoomMigration2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve61)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.LivelinkMigration),
            LicFileConstants = "Livelink Migration 2010", MGProfileType = ProfileType.LivelinkQuantity)]
        [XmlEnum("MG_LivelinkMigration2010")]
        MG_LivelinkMigration2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve61)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.PublicFolderMigration),
            LicFileConstants = "Public Folder Migration 2010", MGProfileType = ProfileType.PublicFolderQuantity)]
        [XmlEnum("MG_PublicFolderMigration2010")]
        MG_PublicFolderMigration2010,

        [EnumMember]
        //TODO: Add GA+ Module
        [AveLicenseVersion(Version = LicenseVersionType.DocAve602)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GovernanceAutomation),
            LicFileConstants = "Governance Automation 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("GA_GovernanceAutomation2010")]
        GA_GovernanceAutomation2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve61)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Vault),
            LicFileConstants = "Vault 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("CP_Vault2010")]
        CP_Vault2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve61)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.EDiscovery),
            LicFileConstants = "eDiscovery 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("CP_eDiscovery2010")]
        CP_eDiscovery2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve61)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.ContentManager), LicFileConstants = "O365 Content Manager 2010")]
        [XmlEnum("OF_ConenteManager2010")]
        OF_ConenteManager2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve61)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.CentralAdmin), LicFileConstants = "O365 Administrator 2010")]
        [XmlEnum("OF_Administration2010")]
        OF_Administration2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve61)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Replicator), LicFileConstants = "O365 Replicator 2010")]
        [XmlEnum("OF_Replicator2010")]
        OF_Replicator2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve61)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GranularBackup), LicFileConstants = "O365 Granular Backup 2010")]
        [XmlEnum("OF_GranularBackup2010")]
        OF_GranularBackup2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Archiver),
            LicFileConstants = "Archiver 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("SO_Archiver2013")]
        SO_Archiver2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Connector),
            LicFileConstants = "Connector 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("SO_Connector2013")]
        SO_Connector2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Extender),
            LicFileConstants = "Realtime Storage Manager 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("SO_RealTimeStorageManager2013")]
        SO_RealTimeStorageManager2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Extender),
            LicFileConstants = "Scheduled Storage Manager 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("SO_ScheduledStorageManager2013")]
        SO_ScheduledStorageManager2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.CentralAdmin),
            LicFileConstants = "Administrator 2013", ExtensionFileConstants = "Policy Enforcer 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("AD_CentralAdmin2013")]
        AD_CentralAdmin2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.ContentManager),
            LicFileConstants = "Content Manager 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("AD_ContentManager2013")]
        AD_ContentManager2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.DeploymentManager),
            LicFileConstants = "Deployment Manager 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("AD_DeploymentManager2013")]
        AD_DeploymentManager2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Replicator),
            LicFileConstants = "Replicator 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("AD_Replicator2013")]
        AD_Replicator2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GranularBackup),
            LicFileConstants = "Granular Backup 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("DP_GranularBackup2013")]
        DP_GranularBackup2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.PlatformBackup),
            LicFileConstants = "Platform Backup 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("DP_PlatformBackup2013")]
        DP_PlatformBackup2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCComplianceReports))]
        [XmlEnum("RC_Usage2013")]
        RC_Usage2013,//(Usage,Customize,Auditor)=>(Compliance Report)

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCInfrastructure))]
        [XmlEnum("RC_Infrastructure2013")]
        RC_Infrastructure2013,//Hide in Gui

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCStorageOptimization))]
        [XmlEnum("RC_StorageOptimization2013")]
        RC_StorageOptimization2013,//Hide in Gui

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCAdministration),
            LicFileConstants = "Administration Reports 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("RC_Administration2013")]
        RC_Administration2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCComplianceReports),
            LicFileConstants = "Compliance Reports 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("RC_ComplianceReports2013")]
        RC_ComplianceReports2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCComplianceReports))]
        [XmlEnum("RC_Customize2013")]
        RC_Customize2013,//(Usage,Customize,Auditor)=>(Compliance Report)

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCComplianceReports))]
        [XmlEnum("RC_AuditorReports2013")]
        RC_AuditorReports2013,//(Usage,Customize,Auditor)=>(Compliance Report)

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCRealtimeMonidtoring))]
        [XmlEnum("RC_RealtimeMonidtoring2013")]
        RC_RealtimeMonidtoring2013,//Hide in Gui

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCActivityHistory))]
        [XmlEnum("RC_ActivityHistory2013")]
        RC_ActivityHistory2013,//Hide in Gui

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SPMigration),
            LicFileConstants = "SharePoint 2007 to 2013", MGProfileType = ProfileType.SP07To13Quantity)]
        [XmlEnum("MG_SharePoint2007to2013")]
        MG_SharePoint2007to2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SPMigration),
            LicFileConstants = "SharePoint 2010 to 2013", MGProfileType = ProfileType.SP10To13Quantity)]
        [XmlEnum("MG_SharePoint2010to2013")]
        MG_SharePoint2010to2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.FileMigration),
            LicFileConstants = "File System Migration 2013", MGProfileType = ProfileType.FileQuantity2013)]
        [XmlEnum("MG_FileSystemMigration2013")]
        MG_FileSystemMigration2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.NotesMigration),
            LicFileConstants = "Lotus Notes Migration 2013", MGProfileType = ProfileType.NotesQuantity2013)]
        [XmlEnum("MG_LotusNotesMigration2013")]
        MG_LotusNotesMigration2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.eRoomMigration),
            LicFileConstants = "eRoom Migration 2013", MGProfileType = ProfileType.eRoomQuantity2013)]
        [XmlEnum("MG_eRoomMigration2013")]
        MG_eRoomMigration2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.LivelinkMigration),
            LicFileConstants = "Livelink Migration 2013", MGProfileType = ProfileType.LivelinkQuantity2013)]
        [XmlEnum("MG_LivelinkMigration2013")]
        MG_LivelinkMigration2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.PublicFolderMigration),
            LicFileConstants = "Public Folder Migration 2013", MGProfileType = ProfileType.PublicFolderQuantity2013)]
        [XmlEnum("MG_PublicFolderMigration2013")]
        MG_PublicFolderMigration2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GovernanceAutomation),
            LicFileConstants = "Governance Automation 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("GA_GovernanceAutomation2013")]
        GA_GovernanceAutomation2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve63)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Vault),
            LicFileConstants = "Vault 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("CP_Vault2013")]
        CP_Vault2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.ContentManager), LicFileConstants = "O365 Content Manager 2013")]
        [XmlEnum("OF_ConenteManager2013")]
        OF_ConenteManager2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.CentralAdmin), LicFileConstants = "O365 Administrator 2013")]
        [XmlEnum("OF_Administration2013")]
        OF_Administration2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Replicator), LicFileConstants = "O365 Replicator 2013")]
        [XmlEnum("OF_Replicator2013")]
        OF_Replicator2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve62)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GranularBackup), LicFileConstants = "O365 Granular Backup 2013")]
        [XmlEnum("OF_GranularBackup2013")]
        OF_GranularBackup2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve63)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.QuickPlaceMigration),
            LicFileConstants = "Quick Place Migration 2010", MGProfileType = ProfileType.QuickPlaceQuantity)]
        [XmlEnum("MG_QuickPlaceMigration2010")]
        MG_QuickPlaceMigration2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve63)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.DocumentumMigration),
            LicFileConstants = "Documentum Migration 2010", MGProfileType = ProfileType.DocumentumQuantity)]
        [XmlEnum("MG_DocumentumMigration2010")]
        MG_DocumentumMigration2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve63)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.FileMigration), LicFileConstants = "O365 File System Migration 2010")]
        [XmlEnum("MG_FileSystemMigrationRemoteFarm2010")]
        MG_FileSystemMigrationRemoteFarm2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve63)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.FileMigration), LicFileConstants = "O365 File System Migration 2013")]
        [XmlEnum("MG_FileSystemMigrationRemoteFarm2013")]
        MG_FileSystemMigrationRemoteFarm2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve63)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SPMigration),
            LicFileConstants = "O365 SharePoint 2007 to 2013", MGProfileType = ProfileType.SP07ToRemote13Quantity)]
        [XmlEnum("MG_SharePoint2007toRemoteFarm2013")]
        MG_SharePoint2007toRemoteFarm2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve63)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SPMigration),
            LicFileConstants = "O365 SharePoint 2010 to 2013", MGProfileType = ProfileType.SP10ToRemote13Quantity)]
        [XmlEnum("MG_SharePoint2010toRemoteFarm2013")]
        MG_SharePoint2010toRemoteFarm2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve63)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SPMigration),
            LicFileConstants = "O365 SharePoint 2013 to 2013", MGProfileType = ProfileType.SP13ToRemote13Quantity)]
        [XmlEnum("MG_SharePoint2013toRemoteFarm2013")]
        MG_SharePoint2013toRemoteFarm2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve63)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.HighAvailability),
            LicFileConstants = "High Availability 2010", Type = LicenseModuleType.SharePoint2010, ExtensionFileConstants = "Snap Mirror 2010")]
        [XmlEnum("DP_HighAvailability")]
        DP_HighAvailability2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve64)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.HighAvailability),
            LicFileConstants = "High Availability 2013", Type = LicenseModuleType.SharePoint2013, ExtensionFileConstants = "Snap Mirror 2013")]
        [XmlEnum("DP_HighAvailability2013")]
        DP_HighAvailability2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve63)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.DocumentumMigration),
            LicFileConstants = "Documentum Migration 2013", MGProfileType = ProfileType.DocumentumQuantity2013)]
        [XmlEnum("MG_DocumentumMigration2013")]
        MG_DocumentumMigration2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve63)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.QuickPlaceMigration),
            LicFileConstants = "Quick Place Migration 2013", MGProfileType = ProfileType.QuickPlaceQuantity2013)]
        [XmlEnum("MG_QuickPlaceMigration2013")]
        MG_QuickPlaceMigration2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve63)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SqlRecoveryManager),
            LicFileConstants = "SQL Server Data Manager 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("DP_SQLServerDataManager2010")]
        DP_SQLServerDataManager2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve63)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SqlRecoveryManager),
            LicFileConstants = "SQL Server Data Manager 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("DP_SQLServerDataManager2013")]
        DP_SQLServerDataManager2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve622)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GovernanceAutomation), LicFileConstants = "O365 Governance Automation")]
        [XmlEnum("OF_GovernanceAutomation")]
        OF_GovernanceAutomation,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve64)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.ContentManager), LicFileConstants = "O365 Content Manager")]
        [XmlEnum("OF_ConenteManagerOnline")]
        OF_ConenteManagerOnline,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve64)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.CentralAdmin), LicFileConstants = "O365 Administrator")]
        [XmlEnum("OF_AdministrationOnline")]
        OF_AdministrationOnline,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve64)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Replicator), LicFileConstants = "O365 Replicator")]
        [XmlEnum("OF_ReplicatorOnline")]
        OF_ReplicatorOnline,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve64)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GranularBackup), LicFileConstants = "O365 Granular Backup", ExtensionFileConstants = "O365 Granular Restore")]
        [XmlEnum("OF_GranularBackupOnline")]
        OF_GranularBackupOnline,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve63172992)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.eRoomMigration),
            LicFileConstants = "O365 eRoom Migration Online", MGProfileType = ProfileType.eRoomOnlineQuantity)]
        [XmlEnum("MG_eRoomMigrationOnline")]
        MG_eRoomMigrationOnline,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve64)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.LivelinkMigration),
            LicFileConstants = "O365 Livelink Migration Online", MGProfileType = ProfileType.LivelinkOnlineQuantity)]
        [XmlEnum("MG_LivelinkMigrationOnline")]
        MG_LivelinkMigrationOnline,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve64)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.NotesMigration),
            LicFileConstants = "O365 Lotus Notes Migration Online", MGProfileType = ProfileType.NotesOnlineQuantity)]
        [XmlEnum("MG_LotusNotesMigrationOnline")]
        MG_LotusNotesMigrationOnline,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve64)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.PublicFolderMigration),
            LicFileConstants = "O365 Public Folder Migration Online", MGProfileType = ProfileType.PublicFolderOnlineQuantity)]
        [XmlEnum("MG_PublicFolderMigrationOnline")]
        MG_PublicFolderMigrationOnline,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve64)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.DocumentumMigration),
            LicFileConstants = "O365 Documentum Migration Online", MGProfileType = ProfileType.DocumentumOnlineQuantity)]
        [XmlEnum("MG_DocumentumMigrationOnline")]
        MG_DocumentumMigrationOnline,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve64)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.QuickPlaceMigration),
            LicFileConstants = "O365 Quick Place Migration Online", MGProfileType = ProfileType.QuickrOnlineQuantity)]
        [XmlEnum("MG_QuickPlaceMigrationOnline")]
        MG_QuickPlaceMigrationOnline,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve64)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.FileMigration),
            LicFileConstants = "O365 File System Migration Online", MGProfileType = ProfileType.FileOnlineQuantity)]
        [XmlEnum("MG_FileSystemMigrationOnline")]
        MG_FileSystemMigrationOnline,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve64)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.EDiscovery),
            LicFileConstants = "eDiscovery 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("CP_eDiscovery2013")]
        CP_eDiscovery2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve633600101)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GovernanceAutomation), LicFileConstants = "End User Migration Service")]
        [XmlEnum("GA_EndUserMigrationService")]
        GA_EndUserMigrationService,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve65)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.DeploymentManager), LicFileConstants = "O365 Deployment Manager")]
        [XmlEnum("OF_DeploymentManagerOnline")]
        OF_DeploymentManagerOnline,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve65)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Archiver), LicFileConstants = "O365 Archiver")]
        [XmlEnum("OF_ArchiverOnline")]
        OF_ArchiverOnline,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve641)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.BoxConnector),
            LicFileConstants = "Cloud Connect 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("SO_CloudConnect2010")]
        SO_CloudConnect2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve641)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.BoxConnector),
            LicFileConstants = "Cloud Connect 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("SO_CloudConnect2013")]
        SO_CloudConnect2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve65)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.VMManagement), LicFileConstants = "VM Backup and Restore")]
        [XmlEnum("DP_VMBackup")]
        DP_VMBackup,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve66)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCAdministration), LicFileConstants = "O365 Administration Reports")]
        [XmlEnum("OF_AdministrationReportsOnline")]
        OF_AdministrationReportsOnline,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve66)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCAdministration), LicFileConstants = "O365 Compliance Reports")]
        [XmlEnum("OF_ComplianceReportsOnline")]
        OF_ComplianceReportsOnline,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve651)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GovernanceAutomation),
            LicFileConstants = "Governance Automation 2007", Type = LicenseModuleType.SharePoint2007)]
        [XmlEnum("GA_GovernanceAutomation2007")]
        GA_GovernanceAutomation2007,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.PlatformBackup),
            LicFileConstants = "Platform Backup 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("DP_PlatformBackup2016")]
        DP_PlatformBackup2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.DeploymentManager),
            LicFileConstants = "Deployment Manager 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("AD_DeploymentManager2016")]
        AD_DeploymentManager2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.ContentManager),
            LicFileConstants = "Content Manager 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("AD_ContentManager2016")]
        AD_ContentManager2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GranularBackup),
            LicFileConstants = "Granular Backup 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("DP_GranularBackup2016")]
        DP_GranularBackup2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SqlRecoveryManager),
            LicFileConstants = "SQL Server Data Manager 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("DP_SQLServerDataManager2016")]
        DP_SQLServerDataManager2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Replicator),
            LicFileConstants = "Replicator 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("AD_Replicator2016")]
        AD_Replicator2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.CentralAdmin),
            LicFileConstants = "Administrator 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("AD_CentralAdmin2016")]
        AD_CentralAdmin2016,

        //[EnumMember]
        //[AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        //[AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.EDiscovery),
        //    LicFileConstants = "eDiscovery 2016", Type = LicenseModuleType.SharePoint2016)]
        //[XmlEnum("CP_eDiscovery2016")]
        //CP_eDiscovery2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.HighAvailability),
            LicFileConstants = "High Availability 2016", Type = LicenseModuleType.SharePoint2016, ExtensionFileConstants = "Snap Mirror 2016")]
        [XmlEnum("DP_HighAvailability2016")]
        DP_HighAvailability2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Archiver),
            LicFileConstants = "Archiver 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("SO_Archiver2016")]
        SO_Archiver2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.BoxConnector),
            LicFileConstants = "Cloud Connect 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("SO_CloudConnect2016")]
        SO_CloudConnect2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Connector),
            LicFileConstants = "Connector 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("SO_Connector2016")]
        SO_Connector2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Extender),
            LicFileConstants = "Realtime Storage Manager 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("SO_RealTimeStorageManager2016")]
        SO_RealTimeStorageManager2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Extender),
            LicFileConstants = "Scheduled Storage Manager 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("SO_ScheduledStorageManager2016")]
        SO_ScheduledStorageManager2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCAdministration),
            LicFileConstants = "Administration Reports 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("RC_Administration2016")]
        RC_Administration2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCComplianceReports),
            LicFileConstants = "Compliance Reports 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("RC_ComplianceReports2016")]
        RC_ComplianceReports2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SPMigration),
            LicFileConstants = "SharePoint 2007 to 2016", MGProfileType = ProfileType.SP07To16Quantity)]
        [XmlEnum("MG_SharePoint2007to2016")]
        MG_SharePoint2007to2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SPMigration),
            LicFileConstants = "SharePoint 2010 to 2016", MGProfileType = ProfileType.SP10To16Quantity)]
        [XmlEnum("MG_SharePoint2010to2016")]
        MG_SharePoint2010to2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SPMigration),
            LicFileConstants = "SharePoint 2013 to 2016", MGProfileType = ProfileType.SP13To16Quantity)]
        [XmlEnum("MG_SharePoint2013to2016")]
        MG_SharePoint2013to2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.NotesMigration),
            LicFileConstants = "Lotus Notes Migration 2016", MGProfileType = ProfileType.NotesQuantity2016)]
        [XmlEnum("MG_LotusNotesMigration2016")]
        MG_LotusNotesMigration2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.FileMigration),
            LicFileConstants = "File System Migration 2016", MGProfileType = ProfileType.FileQuantity2016)]
        [XmlEnum("MG_FileSystemMigration2016")]
        MG_FileSystemMigration2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.LivelinkMigration),
            LicFileConstants = "Livelink Migration 2016", MGProfileType = ProfileType.LivelinkQuantity2016)]
        [XmlEnum("MG_LivelinkMigration2016")]
        MG_LivelinkMigration2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.eRoomMigration),
            LicFileConstants = "eRoom Migration 2016", MGProfileType = ProfileType.eRoomQuantity2016)]
        [XmlEnum("MG_eRoomMigration2016")]
        MG_eRoomMigration2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.PublicFolderMigration),
            LicFileConstants = "Public Folder Migration 2016", MGProfileType = ProfileType.PublicFolderQuantity2016)]
        [XmlEnum("MG_PublicFolderMigration2016")]
        MG_PublicFolderMigration2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.DocumentumMigration),
            LicFileConstants = "Documentum Migration 2016", MGProfileType = ProfileType.DocumentumQuantity2016)]
        [XmlEnum("MG_DocumentumMigration2016")]
        MG_DocumentumMigration2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.QuickPlaceMigration),
            LicFileConstants = "Quick Place Migration 2016", MGProfileType = ProfileType.QuickPlaceQuantity2016)]
        [XmlEnum("MG_QuickPlaceMigration2016")]
        MG_QuickPlaceMigration2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SPMigration),
            LicFileConstants = "O365 SharePoint 2013 to 2016", MGProfileType = ProfileType.SP13ToRemote16Quantity)]
        [XmlEnum("MG_SharePoint2013toRemoteFarm2016")]
        MG_SharePoint2013toRemoteFarm2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve67)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GovernanceAutomation),
            LicFileConstants = "Governance Automation 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("GA_GovernanceAutomation2016")]
        GA_GovernanceAutomation2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve68)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.PlatformBackupForSMSP),
            LicFileConstants = "Platform Backup for SMSP 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("DP_PlatformBackupForSMSP2010")]
        DP_PlatformBackupForSMSP2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve68)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.PlatformBackupForSMSP),
            LicFileConstants = "Platform Backup for SMSP 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("DP_PlatformBackupForSMSP2013")]
        DP_PlatformBackupForSMSP2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve68)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.PlatformBackupForSMSP),
            LicFileConstants = "Platform Backup for SMSP 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("DP_PlatformBackupForSMSP2016")]
        DP_PlatformBackupForSMSP2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve610)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.FSArchiver), 
            LicFileConstants = "File System Archiver", MGProfileType = ProfileType.FSArchiveQuantity)]
        [XmlEnum("SO_FSArchiver")]
        SO_FSArchiver,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.PlatformBackup),
            LicFileConstants = "Platform Backup 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("DP_PlatformBackup2019")]
        DP_PlatformBackup2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.DeploymentManager),
            LicFileConstants = "Deployment Manager 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("AD_DeploymentManager2019")]
        AD_DeploymentManager2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.ContentManager),
            LicFileConstants = "Content Manager 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("AD_ContentManager2019")]
        AD_ContentManager2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GranularBackup),
            LicFileConstants = "Granular Backup 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("DP_GranularBackup2019")]
        DP_GranularBackup2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SqlRecoveryManager),
            LicFileConstants = "SQL Server Data Manager 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("DP_SQLServerDataManager2019")]
        DP_SQLServerDataManager2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Replicator),
            LicFileConstants = "Replicator 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("AD_Replicator2019")]
        AD_Replicator2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.CentralAdmin),
            LicFileConstants = "Administrator 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("AD_CentralAdmin2019")]
        AD_CentralAdmin2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.HighAvailability),
            LicFileConstants = "High Availability 2019", Type = LicenseModuleType.SharePoint2019, ExtensionFileConstants = "Snap Mirror 2019")]
        [XmlEnum("DP_HighAvailability2019")]
        DP_HighAvailability2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Archiver),
            LicFileConstants = "Archiver 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("SO_Archiver2019")]
        SO_Archiver2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.BoxConnector),
            LicFileConstants = "Cloud Connect 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("SO_CloudConnect2019")]
        SO_CloudConnect2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Connector),
            LicFileConstants = "Connector 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("SO_Connector2019")]
        SO_Connector2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Extender),
            LicFileConstants = "Realtime Storage Manager 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("SO_RealTimeStorageManager2019")]
        SO_RealTimeStorageManager2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Extender),
            LicFileConstants = "Scheduled Storage Manager 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("SO_ScheduledStorageManager2019")]
        SO_ScheduledStorageManager2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCAdministration),
            LicFileConstants = "Administration Reports 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("RC_Administration2019")]
        RC_Administration2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.RCComplianceReports),
            LicFileConstants = "Compliance Reports 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("RC_ComplianceReports2019")]
        RC_ComplianceReports2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SPMigration),
            LicFileConstants = "SharePoint 2007 to 2019", MGProfileType = ProfileType.SP07To19Quantity)]
        [XmlEnum("MG_SharePoint2007to2019")]
        MG_SharePoint2007to2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SPMigration),
            LicFileConstants = "SharePoint 2010 to 2019", MGProfileType = ProfileType.SP10To19Quantity)]
        [XmlEnum("MG_SharePoint2010to2019")]
        MG_SharePoint2010to2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SPMigration),
            LicFileConstants = "SharePoint 2013 to 2019", MGProfileType = ProfileType.SP13To19Quantity)]
        [XmlEnum("MG_SharePoint2013to2019")]
        MG_SharePoint2013to2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SPMigration),
           LicFileConstants = "SharePoint 2016 to 2019", MGProfileType = ProfileType.SP16To19Quantity)]
        [XmlEnum("MG_SharePoint2016to2019")]
        MG_SharePoint2016to2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.NotesMigration),
            LicFileConstants = "Lotus Notes Migration 2019", MGProfileType = ProfileType.NotesQuantity2019)]
        [XmlEnum("MG_LotusNotesMigration2019")]
        MG_LotusNotesMigration2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.FileMigration),
            LicFileConstants = "File System Migration 2019", MGProfileType = ProfileType.FileQuantity2019)]
        [XmlEnum("MG_FileSystemMigration2019")]
        MG_FileSystemMigration2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.LivelinkMigration),
            LicFileConstants = "Livelink Migration 2019", MGProfileType = ProfileType.LivelinkQuantity2019)]
        [XmlEnum("MG_LivelinkMigration2019")]
        MG_LivelinkMigration2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.eRoomMigration),
            LicFileConstants = "eRoom Migration 2019", MGProfileType = ProfileType.eRoomQuantity2019)]
        [XmlEnum("MG_eRoomMigration2019")]
        MG_eRoomMigration2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.PublicFolderMigration),
            LicFileConstants = "Public Folder Migration 2019", MGProfileType = ProfileType.PublicFolderQuantity2019)]
        [XmlEnum("MG_PublicFolderMigration2019")]
        MG_PublicFolderMigration2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.DocumentumMigration),
            LicFileConstants = "Documentum Migration 2019", MGProfileType = ProfileType.DocumentumQuantity2019)]
        [XmlEnum("MG_DocumentumMigration2019")]
        MG_DocumentumMigration2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.QuickPlaceMigration),
            LicFileConstants = "Quick Place Migration 2019", MGProfileType = ProfileType.QuickPlaceQuantity2019)]
        [XmlEnum("MG_QuickPlaceMigration2019")]
        MG_QuickPlaceMigration2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.SPMigration),
           LicFileConstants = "O365 SharePoint 2016 to 2019", MGProfileType = ProfileType.SP16ToRemote19Quantity)]
        [XmlEnum("MG_SharePoint2016toRemoteFarm2019")]
        MG_SharePoint2016toRemoteFarm2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.GovernanceAutomation),
            LicFileConstants = "Governance Automation 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("GA_GovernanceAutomation2019")]
        GA_GovernanceAutomation2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.PlatformBackupForSMSP),
           LicFileConstants = "Platform Backup for SMSP 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("DP_PlatformBackupForSMSP2019")]
        DP_PlatformBackupForSMSP2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Replicator),
            LicFileConstants = "Replicator Secure Gov Edition 2010", Type = LicenseModuleType.SharePoint2010)]
        [XmlEnum("AD_ReplicatorSecureGovEdition2010")]
        AD_ReplicatorSecureGovEdition2010,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Replicator),
           LicFileConstants = "Replicator Secure Gov Edition 2013", Type = LicenseModuleType.SharePoint2013)]
        [XmlEnum("AD_ReplicatorSecureGovEdition2013")]
        AD_ReplicatorSecureGovEdition2013,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Replicator),
            LicFileConstants = "Replicator Secure Gov Edition 2016", Type = LicenseModuleType.SharePoint2016)]
        [XmlEnum("AD_ReplicatorSecureGovEdition2016")]
        AD_ReplicatorSecureGovEdition2016,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Replicator),
           LicFileConstants = "Replicator Secure Gov Edition 2019", Type = LicenseModuleType.SharePoint2019)]
        [XmlEnum("AD_ReplicatorSecureGovEdition2019")]
        AD_ReplicatorSecureGovEdition2019,

        [EnumMember]
        [AveLicenseVersion(Version = LicenseVersionType.DocAve6110)]
        [AveLicenseUnitAttribute(Module = typeof(AvePoint.GCommon.Contract.AveModuleContract.Replicator), LicFileConstants = "O365 Replicator Secure Gov Edition")]
        [XmlEnum("OF_ReplicatorSecureGovEditionOnline")]
        OF_ReplicatorSecureGovEditionOnline,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LicenseVersionType
    {
        DocAve60 = 600000000,
        DocAve601 = 601000000,
        DocAve602 = 602000000,
        DocAve61 = 610000000,
        DocAve611 = 611000000,
        DocAve612 = 612000000,
        DocAve62 = 620000000,
        DocAve621 = 621000000,
        DocAve622 = 622000000,
        DocAve63 = 630000000,
        DocAve631 = 631000000,
        DocAve63172992 = 631729920,
        DocAve632 = 632000000,
        DocAve633 = 633000000,
        DocAve633600101 = 633600101,
        DocAve634 = 634000000,
        DocAve64 = 640000000,
        POC640600101 = 640600101,
        DocAve641 = 641000000,
        DocAve642 = 642000000,
        DocAve65 = 650000000,
        DocAve651 = 651000000,
        DocAve66 = 660000000,
        DocAve67 = 670000000,
        DocAve661 = 661000000,
        DocAve662 = 662000000,
        DocAve68 = 680000000,
        DocAve610 = 61000000,
        DocAve6110 = 61100000
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LicenseStatus
    {
        [EnumMember]
        Valid = 0,

        [EnumMember]
        IpMismatch = 1,

        [EnumMember]
        NotCompliant = 2,

        [EnumMember]
        Registered = 3,

        [EnumMember]
        ApplyDocAveSpecificToNetApp,

        [EnumMember]
        ApplyDocAveSpecificToIBM,

        [EnumMember]
        ApplyNetAppToDocAve,

        [EnumMember]
        ApplyNetAppToIBM,

        [EnumMember]
        ApplyIBMToNetApp,

        [EnumMember]
        ApplyIBMToDocAve,

        [EnumMember]
        GALicense,

        [EnumMember]
        InBlackList,

        [EnumMember]
        ApplyDocAveIllegalToNetApp,

        [EnumMember]
        ApplyDocAveIllegalToIBM,

        [EnumMember]
        NoLicense,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LicensePayType
    {
        [EnumMember]
        [XmlEnum("Server")]
        Server = 0,
        [EnumMember]
        [XmlEnum("UserSeat")]
        UserSeat = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ModuleContainer
    {
        [EnumMember]
        DataProtection,
        [EnumMember]
        Administration,
        [EnumMember]
        StorageOptimzation,
        [EnumMember]
        ReportCenter,
        [EnumMember]
        GavornanceAutomation,
        [EnumMember]
        Compliance,
        [EnumMember]
        Migration,
        [EnumMember]
        Office365,
    }

    public enum LicenseRecordType
    {
        LicenseFile = 0,
        PrimaryInfo = 1,
        Maintenance = 2,
        ModuleInfo = 3,
        FarmRegisterInfo = 5,
        UserInfo = 6,
    }
}
