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

using AvePoint.I18N;
using System;

namespace AvePoint.GCommon.Utility.I18N
{
    public class ContextValues
    {
        public static string GetContextValue(Enum key)
        {
            string temp = key.GetType().FullName;
            temp = temp.Substring(temp.IndexOf("ContextValues", StringComparison.OrdinalIgnoreCase) + "ContextValues".Length);
            temp = temp.Replace("+", "_");
            temp = "ContextValue" + temp + "_" + key;
            return EventViewerResources.ResourceManager.GetString(temp);
        }

        public class Authentication
        {
            public enum LoginType
            {
                LocalSystem,
                ActiveDirectoryIntegration,
                ADFSIntegration,
                WindowsIntegration
            }
        }

        public class Configuration
        {
            public class Profile
            {
                public enum ProfileType
                {
                    ADM_Replicator_ProfileSettings,

                    ControlPanel_AgentGroups,
                    ControlPanel_UserNotificationSettings_IncomingEmailSettings,
                    ControlPanel_SharePointSites,
                    ControlPanel_SharePointSites_SiteCollection,
                    ControlPanel_SecurityProfile,
                    ControlPanel_PhysicalDevice,
                    ControlPanel_LogicalDevice,
                    ControlPanel_StoragePolicy,
                    ControlPanel_IndexManager,
                    ControlPanel_ExportLocation,
                    ControlPanel_FilterPolicy,
                    ControlPanel_DomainMapping,
                    ControlPanel_UserMapping,
                    ControlPanel_LanguageMapping,
                    ControlPanel_ColumnMapping,
                    ControlPanel_ContentTypeMapping,
                    ControlPanel_ListTitleMapping,
                    ControlPanel_TemplateMapping,
                    ControlPanel_GroupMapping,
                    ControlPanel_CentralDatabase,
                    ControlPanel_AccountManager_Group,
                    ControlPanel_AccountManager_User,
                    ControlPanel_AccountManager_Permission,
                    ControlPanel_AuthenticationManager_ADIntegration,

                    DP_GranularBackup_PredefinedScheme,

                    PlanGroup,

                    SO_RealtimeStorageManager_Rule,
                    SO_ScheduledStorageManager_Rule,
                    SO_Archiver_Rule,

                    SO_ScheduledStorageManager_Profile,
                    SO_Archiver_Profile,

                    RC_ReportCenterProfile
                }

                public enum OperationType
                {
                    Enabled,
                    Disabled,
                    Activate,
                    Deactivate,
                    Import,
                    Export
                }
            }

            public class Setting
            {
                public enum SettingType
                {
                    ControlPanel_AgentMonitor_ConfigureAgentServiceSetting,
                    ControlPanel_ManagerMonitor_ConfigureMediaServiceSetting,
                    ControlPanel_GeneralSettings,
                    ControlPanel_AdvancedSettings,
                    ControlPanel_SecuritySettings,
                    ControlPanel_SecuritySettings_SecurityInformation_ManagePassphrase,
                    ControlPanel_AuthenticationManager,
                    ControlPanel_LicenseManager,
                    ControlPanel_UpdateManager,
                    ControlPanel_UserNotificationSettings_OutgoingEmailSettings,
                    ControlPanel_JobPruning,
                    ControlPanel_LogManager,
                    ControlPanel_AutoSupportSetting,

                    JobMonitor_ReportLocation,

                    SO_Inherit,
                    SO_StopInherit
                }

                public enum OperationType
                {
                    Enabled,
                    Disabled
                }
            }

            public class Plan
            {
                public enum PlanType
                {
                    ADM_Administrator,
                    ADM_ContentManager,
                    ADM_DeploymentManager,
                    ADM_Replicator,

                    CPL_Vault,
                    
                    DP_GranularBackup,
                    DP_PlatformBackup,

                    MIG_SPMigration,
                    MIG_FileSystemMigration,
                    MIG_eRoomMigration,
                    MIG_LotusNotesMigration,
                    MIG_LiveLinkMigration,
                    MIG_ExchangePublicFolderMigration,
                    MIG_QuickPlaceMigration,
                    MIG_DocumentumMigration
                }
            }
        }

        public class Database
        {
            public enum DatabaseType
            {
                ControlDatabase,
                ReportDatabase,
                AuditorDatabase,
                StubDatabase,
                ReplicatorDatabase,
                ComplianceDatabase,
                MigrationDatabase,
                TempDatabase
            }

            public enum OperationType
            {
                Mount,
                Unmount,
                Clone,
                Backup,
                Restore,
                Configure,
                Upgrade,
                Commit,
                Connect,
                Initialize,
                Create
            }
        }

        public class Driver
        {
            public enum OperationType
            {
                Mount,
                Unmount
            }
        }

        public class Job
        {
            public class JobReport
            {
                public enum OperationType
                {
                    Send,
                    Receive,
                    Merge,
                    Download,
                    Initialize
                }
            }
        }

        public class Packaging
        {
            public enum PackageType
            {
                AgentPackage,
                ManagerPackage
            }
        }
        
        public class Service
        {
            public enum ServiceType
            {
                DocAveControlService,
                DocAveMediaService,
                DocAveReportService,
                DocAveAgentService,
                DocAveTimerService,
                GovernanceAutomationTimerService,

                SharePoint2010Administration,
                SharePoint2010UserCodeHost,
                SharePoint2010Tracing,
                SharePointServerSearch14,
                SharePoint2010Timer,
                SharePointFoundationSearchV4,
                WebAnalyticsService,
                ForefrontIdentityManagerService,
                ForefrontIdentityManagerSynchronizationService,

                SharePoint2010VSSWriter,
                SQLServerVSSWriter,
                VolumeShadowCopy,

                FASTSearchService,
                FASTSearchMonitoring,
                FASTSearchBrowserEngine,
                QRProxyService,
                FASTSearchSAMAdmin,
                FASTSearchSAMWorker,

                IISAdminService,
                NetTcpPortSharingService 
            }

            public enum OperationType
            {
                Register,
                Uninstall,
                Active,
                Inactive,
                CheckStatus
            }
        }

        public class SharePoint
        {
            public class Solution
            {
                public enum OperationType
                {
                    Add,
                    Deploy,
                    Upgrade,
                    Retract,
                    Remove
                }
            }

            public class Database
            {
                public enum DatabaseType
                {
                    ConfigDatabase,
                    SearchDatabase,
                    CustomizeDatabase,
                    ServiceDatabase
                }
            }

            public enum ObjectType
            {
                Column,
                ContentType,
                Item,
                List,
                Site,
                SiteCollection
            }
        }

        public class Snapshot
        {
            public enum OperationType
            {
                Update,
                Mask,
                Import,
                Delete
            }
        }

        public class Storage
        {
            public enum StorageType
            {
                FileSystem,
                TSM,
                FTP,
                Cloud,
                Amazon,
                ATT,
                Atmos,
                Rackspace,
                Azure,
                EMCCentera,
                DELLDXStorage,
                NetApp,
                HCP,
                SFTP
            }
        }
    }
}
