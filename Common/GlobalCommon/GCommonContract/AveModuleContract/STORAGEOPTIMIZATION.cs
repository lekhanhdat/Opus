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
    /// <summary>
    /// Storage Optimization模块，由梁林负责
    /// </summary>
    /// 
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class StorageOptimization : AveModuleContainer
    {

        private const string MODULE_TYPE_DOCAVE_STORAGEOPTIMIZATION_NAME = "Archiver";
        private readonly Extender extender = new Extender();

        public Extender Extender
        {
            get { return extender; }
        }

        private readonly Archiver archiver = new Archiver();

        public Archiver Archiver
        {
            get { return archiver; }
        }

        private readonly Connector connector = new Connector();

        public Connector Connector
        {
            get { return connector; }
        }

        private readonly ExchangeArchiver exArchiver = new ExchangeArchiver();

        public ExchangeArchiver ExchangeArchiver
        {
            get { return exArchiver; }
        }

        private readonly PhysicalRecords physicalRecords = new PhysicalRecords();

        public PhysicalRecords PhysicalRecords
        {
            get { return physicalRecords; }
        }

        private readonly int so_convert_stub_to_content_job_dto_type = (int)JobTypes.SOConvertStubToContent;

        public int SO_CONVERT_STUB_TO_CONTENT_JOB_DTO_TYPE
        {
            get { return so_convert_stub_to_content_job_dto_type; }
        }


        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_STORAGEOPTIMIZATION_ID; }
        }

        public override string Name
        {
            get { return MODULE_TYPE_DOCAVE_STORAGEOPTIMIZATION_NAME; }
        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            //result.Add(Extender);
            result.Add(Archiver);
            //result.Add(Connector);
            return result;

        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.Add(SO_CONVERT_STUB_TO_CONTENT_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class Extender : AveModule
    {

        private const string name = "Storage Manager";

        #region agentType
        public const string AGENT_TYPE_REAL_TIME_ARCHIVE = AgentTypes.AGENT_TYPE_REAL_TIME_ARCHIVE;          //70368744177664L

        #endregion

        private readonly BlobProvider blobProvider = new BlobProvider();
        public BlobProvider BlobProvider
        {
            get { return blobProvider; }
        }
        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_REAL_TIME_ARCHIVE);
            return agentTypes;
        }

        #region jobType

        private readonly int so_stub_restore_job_dto_type = (int)JobTypes.SOConvertStubToContent;

        private readonly int so_stub_retention_job_dto_type = (int)JobTypes.SOStubRetentionExtender;

        private readonly int so_extender_scheduled_job_dto_type = (int)JobTypes.SOExtenderScheduled;

        private readonly int so_extender_dataupgrade_job_dto_type = (int)JobTypes.ExtenderDataUpgrade;

        private readonly int so_EBS_stub_upgrade_job_dto_type = (int)JobTypes.EBSStubUpgrade;

        private readonly int so_stub_db_config_job_dto_type = (int)JobTypes.SOConfigStubDB;

        public int SO_STUB_DB_CONFIG_JOB_DTO_TYPE
        {
            get { return so_stub_db_config_job_dto_type; }
        }

        public int SO_STUB_RESTORE_JOB_DTO_TYPE
        {
            get { return so_stub_restore_job_dto_type; }
        }

        public int SO_STUB_RETENTION_JOB_DTO_TYPE
        {
            get { return so_stub_retention_job_dto_type; }
        }

        public int SO_EXTENDER_SCHEDULED_JOB_DTO_TYPE
        {
            get { return so_extender_scheduled_job_dto_type; }
        }

        public int SO_EXTENDER_DATAUPGRADE_JOB_DTO_TYPE
        {
            get { return so_extender_dataupgrade_job_dto_type; }
        }

        public int SO_EBS_STUB_UPGRADE_JOB_DTO_TYPE
        {
            get { return so_EBS_stub_upgrade_job_dto_type; }
        }
        #endregion

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_EXTENDER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

        }

        public override List<AveModule> getSubModules()
        {
            List<AveModule> result = new List<AveModule>();
            result.Add(BlobProvider);
            return result;
        }


        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.Add(SO_STUB_RETENTION_JOB_DTO_TYPE);
            jobTypes.Add(SO_EXTENDER_SCHEDULED_JOB_DTO_TYPE);
            jobTypes.Add(SO_EXTENDER_DATAUPGRADE_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.Available)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class Archiver : AveModule
    {

        private const string name = "Archiver";

        #region agentType
        public const string AGENT_TYPE_ARCHIVER = AgentTypes.AGENT_TYPE_ARCHIVER;          //64L


        public const string AGENT_TYPE_SP2007_ARCHIVER = AgentTypes.AGENT_TYPE_SP2007_ARCHIVER;          //4096L

        #endregion


        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_ARCHIVER);
            agentTypes.Add(AGENT_TYPE_SP2007_ARCHIVER);
            return agentTypes;
        }

        #region
        private readonly int so_archiver_scan_job_dto_type = (int)JobTypes.ArchiverScan;

        private readonly int so_archiver_backup_job_dto_type = (int)JobTypes.ArchiverBackup;

        private readonly int so_archiver_merge_index_job_dto_type = (int)JobTypes.ArchiverMergeIndex;

        private readonly int so_archiver_retention_job_dto_type = (int)JobTypes.ArchiverRetention;

        private readonly int so_archiver_restore_job_dto_type = (int)JobTypes.ArchiverRestore;

        private readonly int so_end_user_archiver_backup_job_dto_type = (int)JobTypes.EndUserArchiverBackup;

        private readonly int so_end_user_merge_index_job_dto_type = (int)JobTypes.EndUserMergeIndex;

        private readonly int so_archiver_data_import_job_dto_type = (int)JobTypes.ArchiverUpgradeData;


        private readonly int so_archiver_full_text_index_job_dto_type = (int)JobTypes.ArchiverFullTextIndexJob;

        private readonly int so_end_user_restore_job_dto_type = (int)JobTypes.EndUserRestore;

        private readonly int so_archiver_move_index_job_dto_type = (int)JobTypes.ArchiverMoveIndex;

        private readonly int so_archiver_veo_merge_job_dto_type = (int)JobTypes.ArchiverVEOMergeJob;//SAAS-26830 Archiver支持Merge VEO job
        
        public int SO_ARCHIVER_SCAN_JOB_DTO_TYPE
        {
            get { return so_archiver_scan_job_dto_type; }
        }
        public int SO_ARCHIVER_BACKUP_JOB_DTO_TYPE
        {
            get { return so_archiver_backup_job_dto_type; }
        }
        public int SO_ARCHIVER_MERGEINDEX_JOB_DTO_TYPE
        {
            get { return so_archiver_merge_index_job_dto_type; }
        }
        public int SO_ARCHIVER_RETENSION_JOB_DTO_TYPE
        {
            get { return so_archiver_retention_job_dto_type; }
        }
        public int SO_ARCHIVER_FULL_TEXT_INDEX_JOB_DTO_TYPE
        {
            get { return so_archiver_full_text_index_job_dto_type; }
        }
        
        public int SO_ARCHIVER_RESTORE_JOB_DTO_TYPE
        {
            get { return so_archiver_restore_job_dto_type; }
        }
        public int SO_END_USER_ARCHIVER_BACKUP_JOB_DTO_TYPE
        {
            get { return so_end_user_archiver_backup_job_dto_type; }
        }

        public int SO_END_USER_MERGE_INDEX_JOB_DTO_TYPE
        {
            get { return so_end_user_merge_index_job_dto_type; }
        }

        public int SO_ARCHIVER_DATA_IMPORT_JOB_DTO_TYPE
        {
            get { return so_archiver_data_import_job_dto_type; }
        }

        public int SO_END_USER_RESTORE_JOB_DTO_TYPE
        {
            get { return so_end_user_restore_job_dto_type; }
        }

        public int SO_ARCHIVER_MOVE_INDEX_JOB_DTO_TYPE
        {
            get { return so_archiver_move_index_job_dto_type; }
        }
        //SAAS-26830 支持Archiver Merge VEO job
        public int SO_ARCHIVER_VEO_MERGE_JOB_DTO_TYPE
        {
            get { return so_archiver_veo_merge_job_dto_type; }
        }

        #endregion

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_ARCHIVER_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            List<int> jobTypes = new List<int>();
            jobTypes.Add(SO_ARCHIVER_SCAN_JOB_DTO_TYPE);
            jobTypes.Add(SO_ARCHIVER_BACKUP_JOB_DTO_TYPE);
            jobTypes.Add(SO_ARCHIVER_RESTORE_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return null;
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class Connector : AveModule
    {

        private const string name = "Connector";
        #region agentType
        public const string AGENT_TYPE_CONNECTOR = AgentTypes.AGENT_TYPE_CONNECTOR;          //35184372088832L



        public const string AGENT_TYPE_CONNECTOR_VIDEO = AgentTypes.AGENT_TYPE_CONNECTOR_VIDEO;          //140737488355328L



        #endregion


        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_CONNECTOR);
            agentTypes.Add(AGENT_TYPE_CONNECTOR_VIDEO);
            return agentTypes;
        }

        public int CONNECTOR_SYNC_JOB_DTO_TYPE
        {
            get
            {
                return (int)JobTypes.ConnectorSync;
            }
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_CONNECTOR_ID;
            }

        }

        public override string Name
        {
            get
            {
                return name;
            }

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
            get { return DisplayMode.Available; }
        }
    }

    /// <summary>
    ///Extender 子模块。(因为 Stub DB是由Blob Provider 点击进入的 ,所以只要控制住Blob Provider权限即可)
    /// </summary>
    #region Blob Provider
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BlobProvider  : AveModule
    {
        private const string name = "Blob Provider";

        public override int ID
        {
            get { return AveModuleID.MODULE_TYPE_DOCAVE_BLOBPROVIDER_ID; }
        }

        public override string Name
        {
            get { return name; }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.None; }
        }

        public override List<string> getAllAgentTypes()
        {
            return new List<string>();
        }

        public override List<AveModule> getSubModules()
        {
            return new List<AveModule>();
        }

        public override List<int> getAllPlanTypes()
        {
            return new List<int>();
        }

        public override List<int> getAllJobTypes()
        {
            return new List<int>();
        }

        public override List<int> getCategories()
        {
            return new List<int>();
        }
    }
    #endregion
    #region ExChange Archiver

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class ExchangeArchiver : AveModule
    {
        private const string name = "ExchangeArchiver";

        #region agentType
        public const string AGENT_TYPE_ARCHIVER = AgentTypes.AGENT_TYPE_ARCHIVER;          //64L


        public const string AGENT_TYPE_SP2007_ARCHIVER = AgentTypes.AGENT_TYPE_SP2007_ARCHIVER;          //4096L

        #endregion


        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_ARCHIVER);
            agentTypes.Add(AGENT_TYPE_SP2007_ARCHIVER);
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
            return new List<int>() { so_exchange_archiver_backup_job_dto_type, so_exchange_archiver_scan_job_dto_type };
        }

        public override List<int> getCategories()
        {
            return null;
        }


        private readonly int so_exchange_archiver_scan_job_dto_type = (int)JobTypes.ExchangeArchiverScan;

        private readonly int so_exchange_archiver_backup_job_dto_type = (int)JobTypes.ExchangeArchiverBackup;
        public int SO_EXCHANGE_ARCHIVER_SCAN_JOB_DTO_TYPE
        {
            get { return so_exchange_archiver_scan_job_dto_type; }
        }
        public int SO_EXCHANGE_ARCHIVER_BACKUP_JOB_DTO_TYPE
        {
            get { return so_exchange_archiver_backup_job_dto_type; }
        }
        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_ARCHIVER_ID;
            }
        }

        public override string Name
        {
            get
            {
                return name;
            }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get
            {
                return DisplayMode.None;
            }
        }
    }

    #endregion

    #region Physical Records 

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class PhysicalRecords : AveModule
    {
        private const string name = "PhysicalRecords";

        #region agentType
        public const string AGENT_TYPE_ARCHIVER = AgentTypes.AGENT_TYPE_ARCHIVER;          //64L


        public const string AGENT_TYPE_SP2007_ARCHIVER = AgentTypes.AGENT_TYPE_SP2007_ARCHIVER;          //4096L

        #endregion


        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_ARCHIVER);
            agentTypes.Add(AGENT_TYPE_SP2007_ARCHIVER);
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
            return new List<int>() { so_physical_records_job_dto_type };
        }

        public override List<int> getCategories()
        {
            return null;
        }


        private readonly int so_physical_records_job_dto_type = (int)JobTypes.PhysicalRecords;

        public int SO_PHYSICAL_RECORDS_JOB_DTO_TYPE
        {
            get { return so_physical_records_job_dto_type; }
        }
        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_ARCHIVER_ID;
            }
        }

        public override string Name
        {
            get
            {
                return name;
            }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get
            {
                return DisplayMode.None;
            }
        }
    }

    #endregion
}
