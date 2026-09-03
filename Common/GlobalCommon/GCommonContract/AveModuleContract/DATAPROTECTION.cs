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

        private readonly ExchangeOnlineBackup exchangeOnlineBackup = new ExchangeOnlineBackup();

        public ExchangeOnlineBackup ExchangeOnlineBackup
        {
            get { return exchangeOnlineBackup; }
        }

        private readonly PlatformBackup platformbackup = new PlatformBackup();

        public PlatformBackup PlatformBackup
        {
            get { return platformbackup; }
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
            result.Add(ExchangeOnlineBackup);
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

        public const int BACKUP_JOB_DTO_TYPE_IB = (int)JobTypes.BackupJobIB;

        public const int BACKUP_JOB_DTO_TYPE_DB = (int)JobTypes.BackupJobDB;

        public const int RESTORE_JOB_DTO_TYPE = (int)JobTypes.RestoreJob;

        public const int BACKUP_JOB_DTO_TYPE_RETENTION = (int)JobTypes.GranularRetention;

        public const int BACKUP_IMPORT_JOB_DTO_TYPE = (int)JobTypes.UpgradeImportData;

        public const int BACKUP_JOB_DTO_TYPE_07 = (int)JobTypes.SPMigration07_10_Export;

        public const int BACKUP_JOB_DTO_TYPE_CM = (int)JobTypes.CMBackupJob;

        public const int BACKUP_JOB_DTO_TYPE_Replicator = (int)JobTypes.ReplicatorBackupJob;

        public const int BACKUP_JOB_DTO_TYPE_DPM = (int)JobTypes.DPMBackupJob;

        public const int RESTORE_JOB_DTO_TYPE_CM = (int)JobTypes.CMRestoreJob;

        public const int RESTORE_JOB_DTO_TYPE_Replicator = (int)JobTypes.ReplicatorRestoreJob;

        public const int RESTORE_JOB_DTO_TYPE_DPM = (int)JobTypes.DPMRestoreJob;

        public const int RESTORE_JOB_DTO_TYPE_Advanced_Search = (int)JobTypes.MediaGranularAdvancedSearch;
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
    /// ExchangeOnline模块，由郭明军负责
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class ExchangeOnlineBackup : AveModule
    {
        private readonly ExchangeOnlineBackupSub exchangeOnlineBackupSub = new ExchangeOnlineBackupSub();

        public ExchangeOnlineBackupSub ExchangeOnlineBackupSub
        {
            get { return exchangeOnlineBackupSub; }
        }

        private readonly ExchangeOnlineRestoreSub exchangeOnlineRestoreSub = new ExchangeOnlineRestoreSub();

        public ExchangeOnlineRestoreSub ExchangeOnlineRestoreSub
        {
            get { return exchangeOnlineRestoreSub; }
        }


        public const string module_name = "Exchange Online Backup";
        #region agentType
        public const string AGENT_TYPE_SP2007_ITEM_LEVEL = AgentTypes.AGENT_TYPE_SP2007_ITEM_LEVEL;          //512L


        public const string AGENT_TYPE_SP2007_SITE_LEVEL = AgentTypes.AGENT_TYPE_SP2007_SITE_LEVEL;          //1024L


        public const string AGENT_TYPE_SP2007_SUBSITE_LEVEL = AgentTypes.AGENT_TYPE_SP2007_SUBSITE_LEVEL;          //2048L


        public const string AGENT_TYPE_ITEM_LEVEL = AgentTypes.AGENT_TYPE_ITEM_LEVEL; //8L


        public const string AGENT_TYPE_SITE_LEVEL = AgentTypes.AGENT_TYPE_SITE_LEVEL;//16L


        public const string AGENT_TYPE_SUBSITE_LEVEL = AgentTypes.AGENT_TYPE_SUBSITE_LEVEL;//32L


        #endregion
        #region categroy
        public const int exchangeOnlineBackup = 100;


        #endregion
        #region jobType
        public const int BACKUP_JOB_DTO_TYPE_Adhoc = (int)JobTypes.ExchangeOnlineBackupJobAdhoc;
        public const int BACKUP_JOB_DTO_TYPE_FB = (int)JobTypes.ExchangeOnlineBackupJobFB;
        public const int BACKUP_JOB_DTO_TYPE_IB = (int)JobTypes.ExchangeOnlineBackupJobIB;
        public const int BACKUP_JOB_DTO_TYPE_DB = (int)JobTypes.ExchangeOnlineBackupJobDB;
        public const int BACKUP_JOB_DTO_TYPE_RETENTION = (int)JobTypes.ExchangeOnlineRetention;
        public const int RESTORE_JOB_DTO_TYPE = (int)JobTypes.ExchangeOnlienRestoreJob;
        public const int RESTORE_JOB_DTO_TYPE_LOCATE = (int)JobTypes.MediaExchangeAdvancedSearch;
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
                return AveModuleID.MODULE_TYPE_DOCAVE_ExchangeOnlineBACKUP_ID;
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
            result.Add(ExchangeOnlineBackupSub);
            result.Add(ExchangeOnlineRestoreSub);
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
            jobTypes.Add(BACKUP_JOB_DTO_TYPE_Adhoc);
            jobTypes.Add(BACKUP_JOB_DTO_TYPE_FB);
            jobTypes.Add(BACKUP_JOB_DTO_TYPE_IB);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            List<int> categories = new List<int>();
            categories.Add(exchangeOnlineBackup);
            return categories;
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
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class PlatformBackup : AveModule
    {

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

        public const int PR_DATA_MANAGER_JOB_DTO_FOR_NETAPP_TYPE = (int)JobTypes.PRDataManagerIndex;
        
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
            return null;
        }


        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }

    /// <summary>
    /// Sitebin 模块
    /// </summary>
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

    public class HighAvailability : AveModule
    {
        public const string module_name = "High Availability";
        #region agentType
        public const string AGENT_TYPE_HIGH_AVAILABILITY_SYNC2007 = AgentTypes.AGENT_TYPE_HIGH_AVAILABILITY_SYNC2007;         //16777216L  


        public const string AGENT_TYPE_HIGH_AVAILABILITY_SYNC2010 = AgentTypes.AGENT_TYPE_HIGH_AVAILABILITY_SYNC2010;          //16777216L  


        public const string AGENT_TYPE_HIGH_AVAILABILITY_SQL2007 = AgentTypes.AGENT_TYPE_HIGH_AVAILABILITY_SQL2007;         //33554432L    



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
            agentTypes.Add(AGENT_TYPE_HIGH_AVAILABILITY_SYNC2007);
            agentTypes.Add(AGENT_TYPE_HIGH_AVAILABILITY_SYNC2010);
            agentTypes.Add(AGENT_TYPE_HIGH_AVAILABILITY_SQL2007);
            return agentTypes;
        }

        public override List<AveModule> getSubModules()
        {
            throw new NotImplementedException();
        }

        public override List<int> getAllPlanTypes()
        {
            throw new NotImplementedException();
        }

        public override List<int> getAllJobTypes()
        {
            throw new NotImplementedException();
        }

        public override List<int> getCategories()
        {
            throw new NotImplementedException();
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class GranularBackupSub : AveModule
    {
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
    public class GranularRestoreSub : AveModule
    {

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
    public class ExchangeOnlineBackupSub : AveModule
    {
        public const string module_name = "ExchangeOnline_Backup_Sub";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_ExchangeOnlineBACKUP_ID;
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
    public class ExchangeOnlineRestoreSub : AveModule
    {

        public const string module_name = "ExchangeOnline_Restore_Sub";

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            return agentTypes;
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_ExchangeOnlineBACKUP_ID;
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
}
