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
using AvePoint.GCommon.Contract.Server.Common.Attribute;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.AveModuleContract
{
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class DataProtection : AveModuleContainer
    {

        private const string MODULE_TYPE_DOCAVE_DATAPROTECTION_NAME = "Data Protection";

        private readonly GranularBackup granularbackup = new GranularBackup();

        public GranularBackup GranularBackup
        {
            get { return granularbackup; }
        }

        private readonly PlatformBackup platformbackup = new PlatformBackup();

        public PlatformBackup PlatformBackup
        {
            get { return platformbackup; }
        }

        private readonly SqlRecoveryManager sqlServerManager = new SqlRecoveryManager();

        public SqlRecoveryManager SqlServerManager
        {
            get { return sqlServerManager; }
        }

        private readonly VMManagement vmManagement = new VMManagement();

        public VMManagement VMManagement
        {
            get { return vmManagement; }
        }

        private readonly PlatformBackupForSMSP platformBackupForSMSP = new PlatformBackupForSMSP();

        public PlatformBackupForSMSP PlatformBackupForSMSP
        {
            get { return platformBackupForSMSP; } 
        }

        private readonly SiteBin sitebin = new SiteBin();

        public SiteBin SiteBin
        {
            get { return sitebin; }
        }

        private readonly HighAvailability high_availability = new HighAvailability();

        public HighAvailability HighAvailability
        {
            get { return high_availability; }
        }

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_DATAPROTECTION_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_DATAPROTECTION_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(GranularBackup);
            result.Add(PlatformBackup);
            result.Add(PlatformBackupForSMSP);
            result.Add(SqlServerManager);
            result.Add(VMManagement);
            result.Add(HighAvailability);
            return result;

        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }




    /// <summary>
    /// Granular Backup模块，由易飞鸿负责
    /// </summary>
    [DataContract(Namespace= ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class GranularBackup : AveModule
    {
        private readonly GranularBackupSub granularbackupsub = new GranularBackupSub();

        public GranularBackupSub GranularBackupSub
        {
            get { return granularbackupsub; }
        }

        private readonly GranularRestoreSub granularrestoresub = new GranularRestoreSub();

        public GranularRestoreSub GranularRestoreSub
        {
            get { return granularrestoresub; }
        }

        public const string module_name = "Granular Backup";
        #region agentType
        public const string AGENT_TYPE_SP2007_ITEM_LEVEL = AgentTypes.AGENT_TYPE_SP2007_ITEM_LEVEL;          //512L


        public const string AGENT_TYPE_SP2007_SITE_LEVEL = AgentTypes.AGENT_TYPE_SP2007_SITE_LEVEL;          //1024L


        public const string AGENT_TYPE_SP2007_SUBSITE_LEVEL = AgentTypes.AGENT_TYPE_SP2007_SUBSITE_LEVEL;          //2048L


        public const string AGENT_TYPE_ITEM_LEVEL = AgentTypes.AGENT_TYPE_ITEM_LEVEL; //8L


        public const string AGENT_TYPE_SITE_LEVEL = AgentTypes.AGENT_TYPE_SITE_LEVEL;//16L


        public const string AGENT_TYPE_SUBSITE_LEVEL = AgentTypes.AGENT_TYPE_SUBSITE_LEVEL;//32L


        #endregion
        #region categroy
        public const int granularBackup = 18;


        #endregion
        #region jobType
        public const int BACKUP_JOB_DTO_TYPE = (int)JobTypes.BackupJob;

        public const int BACKUP_JOB_DTO_TYPE_FB = (int)JobTypes.BackupJobFB;

        public const int BACKUP_JOB_DTO_TYPE_AdHoc = (int)JobTypes.GranularAdHocBackupJob;

        public const int BACKUP_JOB_DTO_TYPE_IB = (int)JobTypes.BackupJobIB;

        public const int BACKUP_JOB_DTO_TYPE_DB = (int)JobTypes.BackupJobDB;

        public const int RESTORE_JOB_DTO_TYPE = (int)JobTypes.RestoreJob;

        public const int RESTORE_JOB_DTO_TYPE_OOP = (int)JobTypes.GranularOOPRestoreJob;

        public const int RESTORE_JOB_DTO_TYPE_ENDUSER = (int)JobTypes.GranularEndUserRestore;

        public const int BACKUP_JOB_DTO_TYPE_RETENTION = (int)JobTypes.GranularRetention;

        public const int BACKUP_IMPORT_JOB_DTO_TYPE = (int)JobTypes.UpgradeImportData;

        public const int BACKUP_JOB_DTO_TYPE_07 = (int)JobTypes.SPMigration07Export;

        public const int BACKUP_JOB_DTO_TYPE_10 = (int)JobTypes.SPMigration10Export;

        public const int BACKUP_JOB_DTO_TYPE_13 = (int)JobTypes.SPMigration13Export;

        public const int BACKUP_JOB_DTO_TYPE_16 = (int)JobTypes.SPMigration16Export;

        public const int BACKUP_JOB_DTO_TYPE_CM = (int)JobTypes.CMBackupJob;

        public const int BACKUP_JOB_DTO_TYPE_Replicator = (int)JobTypes.ReplicatorBackupJob;

        public const int BACKUP_JOB_DTO_TYPE_DPM = (int)JobTypes.DPMBackupJob;

        public const int RESTORE_JOB_DTO_TYPE_CM = (int)JobTypes.CMRestoreJob;

        public const int RESTORE_JOB_DTO_TYPE_Replicator = (int)JobTypes.ReplicatorRestoreJob;

        public const int RESTORE_JOB_DTO_TYPE_DPM = (int)JobTypes.DPMRestoreJob;

        public const int BACKUP_JOB_DTO_TYPE_COPY_DATA = (int)JobTypes.GranularSyncDataJob;
        #endregion

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_SP2007_ITEM_LEVEL);
            agentTypes.Add(AGENT_TYPE_SP2007_SITE_LEVEL);
            agentTypes.Add(AGENT_TYPE_SP2007_SUBSITE_LEVEL);
            agentTypes.Add(AGENT_TYPE_ITEM_LEVEL);
            agentTypes.Add(AGENT_TYPE_SITE_LEVEL);
            agentTypes.Add(AGENT_TYPE_SUBSITE_LEVEL);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_GRANULARBACKUP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(GranularBackupSub);
            result.Add(GranularRestoreSub);
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.Add(BACKUP_JOB_DTO_TYPE);
            jobTypes.Add(BACKUP_JOB_DTO_TYPE_FB);
            jobTypes.Add(BACKUP_JOB_DTO_TYPE_IB);
            jobTypes.Add(BACKUP_JOB_DTO_TYPE_DB);
            jobTypes.Add(RESTORE_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            categories.Add(granularBackup);
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }

    /// <summary>
    /// Granular Backup模块，由易飞鸿负责
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class GranularBackupSub : AveModule
    {

        private readonly AdHocBackup adhocbackup = new AdHocBackup();

        public AdHocBackup AdHocBackup
        {
            get { return adhocbackup; }
        }

        private readonly ScheduledPlans scheduledplans = new ScheduledPlans();

        public ScheduledPlans ScheduledPlans
        {
            get { return scheduledplans; }
        }

        public const string module_name = "Granular_Backup_Sub";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_GRANULARBACKUP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(AdHocBackup);
            result.Add(ScheduledPlans);
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    /// <summary>
    /// Granular Backup模块，由易飞鸿负责
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class AdHocBackup : AveModule
    {

        public const string module_name = "Ad Hoc Backup";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_GRANULARBACKUP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    /// <summary>
    /// Granular Backup模块，由易飞鸿负责
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ScheduledPlans : AveModule
    {

        public const string module_name = "Scheduled Plans";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_GRANULARBACKUP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    /// <summary>
    /// Granular Backup模块，由易飞鸿负责
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class GranularRestoreSub : AveModule
    {

        private readonly InPlaceRestore inplacerestore = new InPlaceRestore();

        public InPlaceRestore InPlaceRestore
        {
            get { return inplacerestore; }
        }
        private readonly OutofPlaceRestore outofplacerestore = new OutofPlaceRestore();

        public OutofPlaceRestore OutofPlaceRestore
        {
            get { return outofplacerestore; }
        }

        private readonly EndUserRestoreManagement enduserrestoremanagement = new EndUserRestoreManagement();
        public EndUserRestoreManagement EndUserRestoreManagement
        {
            get { return enduserrestoremanagement; }
        }

        private readonly PreviewDocument previewDocument = new PreviewDocument();
        public PreviewDocument PreviewDocument
        {
            get { return previewDocument; }
        }
       
        public const string module_name = "Granular_Restore_Sub";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_GRANULARBACKUP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(InPlaceRestore);
            result.Add(OutofPlaceRestore);
            result.Add(EndUserRestoreManagement);
            result.Add(PreviewDocument);
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class InPlaceRestore : AveModule
    {

        public const string module_name = "In Place Restore";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_GRANULARBACKUP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class OutofPlaceRestore : AveModule
    {

        public const string module_name = "Out of Place Restore";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_GRANULARBACKUP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class EndUserRestoreManagement : AveModule
    {

        public const string module_name = "End User Restore Management";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_GRANULARBACKUP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class PreviewDocument : AveModule
    {

        public const string module_name = "Preview Document";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_GRANULARBACKUP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    /// <summary>
    /// Platform Backup模块，由魏力航负责
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class PlatformBackupForSMSP : AveModule
    {

        private readonly PlatformBackupSubForSMSP platformbackupsub = new PlatformBackupSubForSMSP();

        public PlatformBackupSubForSMSP PlatformBackupSub
        {
            get { return platformbackupsub; }
        }

        private readonly PlatformRestoreSubForSMSP platformrestoresub = new PlatformRestoreSubForSMSP();

        public PlatformRestoreSubForSMSP PlatformRestoreSub
        {
            get { return platformrestoresub; }
        }

        public const string module_name = "Platform Backup for SMSP";
        #region agentType
        public const string AGENT_TYPE_PR_FOR_SMSP = AgentTypes.AGENT_TYPE_PLATFORM_BACKUP_FOR_SMSP;
        #endregion
        #region categroy
        public const int PlatformRecoveryBackup = 116;
        public const int PlatformRecoveryRestore = 117;
        #endregion
        #region planType
        public const int PR_INPLACE_LEVEL_RESTORE = 0;

        public const int PR_OUTOFPLACE_LEVEL_RESTORE = 3;

        public const int PR_ITEM_INPLACE_LEVEL_RESTORE = 1;

        public const int PR_ITEM_OUTOFPLACE_LEVEL_RESTORE = 2;

        public const int PR_SSP_INPLACE_LEVEL_RESTORE = 4;

        public const int PR_SSP_OUTOFPLACE_LEVEL_RESTORE = 5;

        public const int PR_WFE_INPLACE_LEVEL_RESTORE = 6;

        public const int PR_WFE_OUTOFPLACE_LEVEL_RESTORE = 7;

        public const int RESTORE_RAW_DATABASE = 8;

        public const int PR_SSA_SETTING_INPLACE_RESTORE = 9;

        public const int PR_SSA_SETTING_OUTOFPLACE_RESTORE = 10;

        public const int PR_FARM_REBUILD_NORMAL_RESTORE = 11;

        public const int PR_FARM_REBUILD_FROM_ALTERNATE_LOCATION = 12;

        public const int PR_BLOB_RESTORE = 14;

        public const int PR_FARM_REPAIR_RESTORE = 15;

        public const int PR_VM_INPLACE_RESTORE = 16;

        public const int PR_FARM_CLOINE_RESTORE = 17;

        public const int PR_FARM_REBUILD_VM_RESTORE = 18;

        public const int PR_STORAGE_PROVISION = 19;

        public const int PR_SNAPMIRROR_PROVISION = 20;

        public const int PR_SNAPMIRROR_DISCOVER = 21;

        public const int PR_FARM_CLONE_WITH_VM_RESTORE = 22;
        #endregion
        #region jobType
        public const int PR_DATA_MANAGER_JOB_DTO_FOR_NETAPP_TYPE = (int)JobTypes.PRDataManagerIndex;
        
        public const int PR_BACKUP_FOR_SMSP_JOB_DTO_TYPE_FB = (int)JobTypes.PRBackupJobFBforSMSP;

        public const int PR_RESTORE_FOR_SMSP_JOB_DTO_TYPE = (int)JobTypes.PRRestoreJobforSMSP;

        public const int PR_MAINTENANCE_FOR_SMSP_JOB_DTO_TYPE = (int)JobTypes.PRMaintenanceJobforSMSP;

        public const int PR_MIGRATION_DATABASE_FOR_SMSP_JOB_DTO_TYPE = (int)JobTypes.PRNAMigrationDbforSMSP;

        public const int PR_MIGRATION_INDEX_FOR_SMSP_JOB_DTO_TYPE = (int)JobTypes.PRNAMigrationIndexforSMSP;

        public const int FARM_REBUILD_FOR_SMSP_JOB_DTO_TYPE = (int)JobTypes.FarmRebuildJobforSMSP;

        public const int FARM_CLONE_FOR_SMSP_JOB_DTO_TYPE = (int)JobTypes.PRFarmCloneJobforSMSP;

        public const int FARM_REPAIR_FOR_SMSP_JOB_DTO_TYPE = (int)JobTypes.PRFarmRepairJobforSMSP;

        public const int FARM_REBUILD_WITH_VM_FOR_SMSP_JOB_DTO_TYPE = (int)JobTypes.PRFarmRebuildWithVMJobforSMSP;

        public int PR_DATA_MANAGER_JOB_DTO_FOR_NETAPP_TYPE_VALUE
        {
            get { return PR_DATA_MANAGER_JOB_DTO_FOR_NETAPP_TYPE; }
        }
        #endregion


        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_PR_FOR_SMSP);
            return agentTypes;
        }



        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_PLATFORMBACKUP_FOR_SMSP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            planTypes.Add(PR_INPLACE_LEVEL_RESTORE);
            planTypes.Add(PR_OUTOFPLACE_LEVEL_RESTORE);
            planTypes.Add(PR_ITEM_INPLACE_LEVEL_RESTORE);
            planTypes.Add(PR_ITEM_OUTOFPLACE_LEVEL_RESTORE);
            planTypes.Add(PR_SSP_INPLACE_LEVEL_RESTORE);
            planTypes.Add(PR_SSP_OUTOFPLACE_LEVEL_RESTORE);
            planTypes.Add(PR_WFE_INPLACE_LEVEL_RESTORE);
            planTypes.Add(PR_WFE_OUTOFPLACE_LEVEL_RESTORE);
            planTypes.Add(RESTORE_RAW_DATABASE);
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.Add(PR_BACKUP_FOR_SMSP_JOB_DTO_TYPE_FB);
            jobTypes.Add(PR_RESTORE_FOR_SMSP_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            categories.Add(PlatformRecoveryBackup);
            categories.Add(PlatformRecoveryRestore);
            return categories;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(PlatformBackupSub);
            result.Add(PlatformRestoreSub);
            return result;
        }


        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }


    /// <summary>
    /// Platform Backup模块，由魏力航负责
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class PlatformBackup : AveModule
    {

        private readonly PlatformBackupSub platformbackupsub = new PlatformBackupSub();

        public PlatformBackupSub PlatformBackupSub
        {
            get { return platformbackupsub; }
        }

        private readonly PlatformRestoreSub platformrestoresub = new PlatformRestoreSub();

        public PlatformRestoreSub PlatformRestoreSub
        {
            get { return platformrestoresub; }
        }

        public const string module_name = "Platform Backup";
        #region agentType
        public const string AGENT_TYPE_PR_CONTROL = AgentTypes.AGENT_TYPE_PR_CONTROL;          //131072L
        public const string AGENT_TYPE_PR_MEMBER = AgentTypes.AGENT_TYPE_PR_MEMBER;            //262144L





        #endregion
        #region categroy
        public const int PlatformRecoveryBackup = 6;


        public const int PlatformRecoveryRestore = 7;



        #endregion
        #region planType
        public const int PR_INPLACE_LEVEL_RESTORE = 0;


        public const int PR_OUTOFPLACE_LEVEL_RESTORE = 3;



        public const int PR_ITEM_INPLACE_LEVEL_RESTORE = 1;


        public const int PR_ITEM_OUTOFPLACE_LEVEL_RESTORE = 2;



        public const int PR_SSP_INPLACE_LEVEL_RESTORE = 4;


        public const int PR_SSP_OUTOFPLACE_LEVEL_RESTORE = 5;



        public const int PR_WFE_INPLACE_LEVEL_RESTORE = 6;



        public const int PR_WFE_OUTOFPLACE_LEVEL_RESTORE = 7;


        public const int RESTORE_RAW_DATABASE = 8;


        public const int PR_SSA_SETTING_INPLACE_RESTORE = 9;


        public const int PR_SSA_SETTING_OUTOFPLACE_RESTORE = 10;

        public const int PR_FARM_REBUILD_NORMAL_RESTORE = 11;

        public const int PR_FARM_REBUILD_FROM_ALTERNATE_LOCATION = 12;

        public const int PR_BLOB_RESTORE = 14;

        public const int PR_FARM_REPAIR_RESTORE = 15;

        public const int PR_VM_INPLACE_RESTORE = 16;

        public const int PR_FARM_CLOINE_RESTORE = 17;

        public const int PR_FARM_REBUILD_VM_RESTORE = 18;

        public const int PR_STORAGE_PROVISION = 19;

        public const int PR_SNAPMIRROR_PROVISION = 20;

        public const int PR_SNAPMIRROR_DISCOVER = 21;

        public const int PR_FARM_CLONE_WITH_VM_RESTORE = 22;
        #endregion
        #region jobType
        public const int PR_BACKUP_JOB_DTO_TYPE_FB = (int)JobTypes.PRBackupJobFB;


        public const int PR_BACKUP_JOB_DTO_TYPE_IB = (int)JobTypes.PRBackupJobIB;


        public const int PR_BACKUP_JOB_DTO_TYPE_DB = (int)JobTypes.PRBackupJobDB;


        public const int PR_RESTORE_JOB_DTO_TYPE = (int)JobTypes.PRRestoreJob;

        public const int PR_MAINTENANCE_JOB_DTO_TYPE = (int)JobTypes.PRMaintenanceJob;

        public const int PR_RETENTION_JOB_DTO_TYPE = (int)JobTypes.PRJobRentention;

        public const int PR_RETENTION_JOB_DTO_FOR_NETAPP_TYPE = (int)JobTypes.PRJobRetentionForSN;

        public const int BACKUP_IMPORT_JOB_DTO_TYPE = (int)JobTypes.UpgradeImportData;

        public const int PR_MIGRATION_JOB_DTO_TYPE = (int)JobTypes.PRNAMigrationDbAndIndex;

        public const int PR_MIGRATION_DATABASE_JOB_DTO_TYPE = (int)JobTypes.PRNAMigrationDb;

        public const int PR_MIGRATION_INDEX_JOB_DTO_TYPE = (int)JobTypes.PRNAMigrationIndex;

        public const int FARM_REBUILD_JOB_DTO_TYPE = (int)JobTypes.FarmRebuildJob;

        public const int FARM_CLONE_JOB_DTO_TYPE = (int)JobTypes.PRFarmCloneJob;

        public const int FARM_REPAIR_JOB_DTO_TYPE = (int)JobTypes.PRFarmRepairJob;

        public const int FARM_REBUILD_WITH_VM_JOB_DTO_TYPE = (int)JobTypes.PRFarmRebuildWithVMJob;

        public const int PR_DATA_MANAGER_JOB_DTO_FOR_NETAPP_TYPE = (int)JobTypes.PRDataManagerIndex;

        public const int PR_COPY_DATA_JOB_DTO_TYPE = (int)JobTypes.PlatformSyncDataJob;

        public const int PLATFORM_STORAGE_PROVISION_JOB_DTO_TYPE = (int)JobTypes.PRStorageProvisionJob;

        public const int PLATFORM_SNAPMIRROR_PROVISION_JOB_DTO_TYPE = (int)JobTypes.PRSnapMirrorProvisionJob;

        public const int PLATFORM_SNAPMIRROR_DISCOVER_JOB_DTO_TYPE = (int)JobTypes.PRSnapMirrorDiscoverJob;

        public int PR_DATA_MANAGER_JOB_DTO_FOR_NETAPP_TYPE_VALUE
        {
            get { return PR_DATA_MANAGER_JOB_DTO_FOR_NETAPP_TYPE; }
        }
        #endregion


        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_PR_CONTROL);
            agentTypes.Add(AGENT_TYPE_PR_MEMBER);
            return agentTypes;
        }



        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_PLATFORMBACKUP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            planTypes.Add(PR_INPLACE_LEVEL_RESTORE);
            planTypes.Add(PR_OUTOFPLACE_LEVEL_RESTORE);
            planTypes.Add(PR_ITEM_INPLACE_LEVEL_RESTORE);
            planTypes.Add(PR_ITEM_OUTOFPLACE_LEVEL_RESTORE);
            planTypes.Add(PR_SSP_INPLACE_LEVEL_RESTORE);
            planTypes.Add(PR_SSP_OUTOFPLACE_LEVEL_RESTORE);
            planTypes.Add(PR_WFE_INPLACE_LEVEL_RESTORE);
            planTypes.Add(PR_WFE_OUTOFPLACE_LEVEL_RESTORE);
            planTypes.Add(RESTORE_RAW_DATABASE);
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.Add(PR_BACKUP_JOB_DTO_TYPE_FB);
            jobTypes.Add(PR_BACKUP_JOB_DTO_TYPE_IB);
            jobTypes.Add(PR_BACKUP_JOB_DTO_TYPE_DB);
            jobTypes.Add(PR_RESTORE_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            categories.Add(PlatformRecoveryBackup);
            categories.Add(PlatformRecoveryRestore);
            return categories;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(PlatformBackupSub);
            result.Add(PlatformRestoreSub);
            return result;
        }


        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }
    /// <summary>
    /// Sql Recovery Manager模块，由魏力航负责
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class SqlRecoveryManager : AveModule
    {
        private readonly AnalysisSqlBackup analysissqlbackup = new AnalysisSqlBackup();

        public AnalysisSqlBackup AnalysisSqlBackup
        {
            get { return analysissqlbackup; }
        }

        private readonly RestoreFromSqlBackup restorefromsqlbackup = new RestoreFromSqlBackup();

        public RestoreFromSqlBackup RestoreFromSqlBackup
        {
            get { return restorefromsqlbackup; }
        }
        public const string module_name = "SQL Recovery Manager";
        #region agentType
        public const string AGENT_TYPE_SRM_CONTROL = AgentTypes.AGENT_TYPE_SQL_RECOVERY_MANAGER; 
        #endregion
        #region categroy
        public const int SRMAnalyzeSqlBackup = 1;


        public const int SRMRestoreFromSQLBackup = 2;



        #endregion
        #region planType
        public const int SRM_ANALYZE_SQL_BACKUP = 0;


        public const int SRM_RESTORE_FROM_SQL_BACKUP = 1;



        #endregion
        #region jobType
        public const int SRM_ANALYZE_SQL_BACKUP_JOB_DTO_TYPE = (int)JobTypes.SRMAnalyzeSqlBackup;


        public const int SRM_RESTORE_FROM_SQL_BACKUP_JOB_DTO_TYPE = (int)JobTypes.SRMRestoreFromSQLBackup;

        public const int SSDM_RESTORE_FROM_LIVE_DB_JOB_DTO_TYPE = (int)JobTypes.SSDMRestoreFromLiveDBJob;

        public const int SSDM_ANALYZE_VHD_BACKUP_JOB_DTO_TYPE = (int)JobTypes.SSDMAnalyzeVHDBackup;

        public const int SSDM_RETENTION_JOB_DTO_TYPE = (int)JobTypes.SSDMJobRetention;

        #endregion


        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_SRM_CONTROL);
            return agentTypes;
        }



        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_SQLRECOVERYMANAGER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            planTypes.Add(SRM_ANALYZE_SQL_BACKUP);
            planTypes.Add(SRM_RESTORE_FROM_SQL_BACKUP);
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.Add(SRM_ANALYZE_SQL_BACKUP_JOB_DTO_TYPE);
            jobTypes.Add(SRM_RESTORE_FROM_SQL_BACKUP_JOB_DTO_TYPE);
            jobTypes.Add(SSDM_RESTORE_FROM_LIVE_DB_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            categories.Add(SRMAnalyzeSqlBackup);
            categories.Add(SRMRestoreFromSQLBackup);
            return categories;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(AnalysisSqlBackup);
            result.Add(RestoreFromSqlBackup);
            return result;
        }


        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }

    /// <summary>
    /// VMManagement模块，由 ycao 负责
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class VMManagement : AveModule
    {
        private readonly VMBackupSub vmBackupSub = new VMBackupSub();

        public VMBackupSub VMBackupSub
        {
            get { return vmBackupSub; }
        }

        private readonly VMRestoreSub vmRestoreSub = new VMRestoreSub();

        public VMRestoreSub VMRestoreSub
        {
            get { return vmRestoreSub; }
        }
        public const string module_name = "VM Management";

        #region agentType
        public const string AGENT_TYPE_VM = AgentTypes.AGENT_TYPE_VM;
        #endregion

        #region categroy
        public const int VMBackup = (int)PlanCategory.VMBackup;
        public const int VMRestore = (int)PlanCategory.VMRestore;
        #endregion

        #region planType
        public const int VM_BACKUP = 0;
        public const int VM_RESTORE = 1;
        public const int VM_FILE_LEVEL_RESTORE = 2;
        public const int VM_RETENTION = 3;
        public const int VM_OOP_RESTORE = 4;
        public const int VM_CLONE_VM = 5;
        #endregion

        #region jobType
        public const int VM_BACKUP_JOB_DTO_TYPE_FB = (int)JobTypes.VMBackupJobFB;
        public const int VM_BACKUP_JOB_DTO_TYPE_IB = (int)JobTypes.VMBackupJobIB;
        public const int VM_BACKUP_JOB_DTO_TYPE_DB = (int)JobTypes.VMBackupJobDB;
        public const int VM_RESTORE_JOB_DTO_TYPE = (int)JobTypes.VMRestore;
        public const int VM_DATA_MANAGER_JOB_DTO_TYPE = (int)JobTypes.VMDataManager;
        public const int VM_RETENTION_JOB_DTO_TYPE = (int)JobTypes.VMJobRetention;
        #endregion

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_VM);
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_VM_ID;
            }
        }

        public override string Name
        {
            get
            {
                return module_name;
            }
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            planTypes.Add(VM_BACKUP);
            planTypes.Add(VM_RESTORE);
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.Add(VM_BACKUP_JOB_DTO_TYPE_FB);
            jobTypes.Add(VM_BACKUP_JOB_DTO_TYPE_IB);
            jobTypes.Add(VM_BACKUP_JOB_DTO_TYPE_DB);
            jobTypes.Add(VM_RESTORE_JOB_DTO_TYPE);
            jobTypes.Add(VM_DATA_MANAGER_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            categories.Add(VMBackup);
            categories.Add(VMRestore);
            return categories;
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(VMBackupSub);
            result.Add(VMRestoreSub);
            return result;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }

    /// <summary>
    /// VM BackupSub模块，由 ycao 负责
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class VMBackupSub : AveModule
    {

        public const string module_name = "Platform_Backup_Sub";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_VM_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    /// <summary>
    /// VM RestoreSub模块，由 ycao 负责
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class VMRestoreSub : AveModule
    {

        public const string module_name = "Platform_Backup_Sub";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_VM_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }


    /// <summary>
    /// Sql Recovery Manager模块，由易飞鸿负责
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class AnalysisSqlBackup : AveModule
    {

        public const string module_name = "Analysis SQL Backup";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_PLATFORMBACKUP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    /// <summary>
    /// Sql Recovery Manager模块，由易飞鸿负责
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class RestoreFromSqlBackup : AveModule
    {

        public const string module_name = "Restore From SQL Backup";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_PLATFORMBACKUP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    /// <summary>
    /// Granular Backup模块，由易飞鸿负责
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class PlatformBackupSub : AveModule
    {

        public const string module_name = "Platform_Backup_Sub";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_PLATFORMBACKUP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    /// <summary>
    /// Granular Backup模块，由易飞鸿负责
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class PlatformBackupSubForSMSP : AveModule
    {

        public const string module_name = "Platform_Backup_Sub_For_SMSP";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_PLATFORMBACKUP_FOR_SMSP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    /// <summary>
    /// Granular Backup模块，由易飞鸿负责
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class PlatformRestoreSub : AveModule
    {

        public const string module_name = "Platform_Restore_Sub";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_PLATFORMBACKUP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    /// <summary>
    /// Granular Backup模块，由易飞鸿负责
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class PlatformRestoreSubForSMSP : AveModule
    {

        public const string module_name = "Platform_Restore_Sub_For_SMSP";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_PLATFORMBACKUP_FOR_SMSP_ID;
            }

        }

        public override string Name
        {
            get
            {
                return module_name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    /// <summary>
    /// Sitebin 模块
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class SiteBin : AveModule
    {
        public const string module_name = "SiteBin";
        #region agentType
        public const string AGENT_TYPE_SITE_BIN = AgentTypes.AGENT_TYPE_SITE_BIN;          //17592186044416L   

        #endregion

        /// <summary>
        /// 获取模块的Id值
        /// </summary>
        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_SITEBIN_ID;
            }
        }

        public override string Name
        {
            get
            {
                return module_name;
            }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_SITE_BIN);
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class HighAvailability : AveModule
    {
        public const string module_name = "High Availability";

        #region agent type
        public const string AGENT_TYPE_HIGH_AVAILABILITY_CONTROL = AgentTypes.AGENT_TYPE_HIGH_AVAILABILITY_CONTROL;          //16777216L  
        public const string AGENT_TYPE_HIGH_AVAILABILITY_MEMBER = AgentTypes.AGENT_TYPE_HIGH_AVAILABILITY_MEMBER;         //33554432L    
        #endregion
    
        #region categroy
        public const int HighAvailabilitySync = 1;
        public const int HighAvailabilityFailover = 2;
        public const int HighAvilabilityFallback = 3;
        public const int HighAvailabilityPreScan = 4;
        #endregion

        #region group type
        public const int HA_SYNC_GROUP = 0;
        #endregion

        #region job type
        public const int HA_SYNC_JOB_DTO_TYPE_FB = (int)JobTypes.HASyncJobFB;
        public const int HA_SYNC_JOB_DTO_TYPE_IB = (int)JobTypes.HASyncJobIB;
        public const int HA_FAILOVER_JOB_DTO_TYPE = (int)JobTypes.HAFailoverJob;
        public const int HA_FALLBACK_JOB_DTO_TYPE = (int)JobTypes.HAFallbackJob;
        public const int HA_PRESCAN_JOB_DTO_TYPE = (int)JobTypes.HAPreScan;
        #endregion
        /// <summary>
        /// 获取模块ID值
        /// </summary>
        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_HIGHAVAILABILITY_ID;
            }
        }

        public override string Name
        {
            get
            {
                return module_name;
            }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();   
            agentTypes.Add(AGENT_TYPE_HIGH_AVAILABILITY_CONTROL);
            agentTypes.Add(AGENT_TYPE_HIGH_AVAILABILITY_MEMBER);
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            //throw new NotImplementedException();
            List<AveModule> result = new List<AveModule>();
            return result;
        }

        public override List<int> getAllPlanTypes()
        {
            List<int> planTypes = new List<int>();
            planTypes.Add(HA_SYNC_GROUP);
            return planTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.Add(HA_FAILOVER_JOB_DTO_TYPE);
            jobTypes.Add(HA_FALLBACK_JOB_DTO_TYPE);
            jobTypes.Add(HA_SYNC_JOB_DTO_TYPE_FB);
            jobTypes.Add(HA_SYNC_JOB_DTO_TYPE_IB);
            jobTypes.Add(HA_PRESCAN_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            categories.Add(HighAvailabilitySync);
            categories.Add(HighAvailabilityFailover);
            categories.Add(HighAvilabilityFallback);
            categories.Add(HighAvailabilityPreScan);
            return categories;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }
}
