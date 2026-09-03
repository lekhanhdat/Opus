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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Attribute;
using AvePoint.GCommon.Contract.Server.Common;

namespace AvePoint.GCommon.Contract.AveModuleContract
{
    /// <summary>
    /// Storage Optimization模块，由梁林负责
    /// </summary>
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class StorageOptimization : AveModuleContainer
    {

        private const string MODULE_TYPE_DOCAVE_STORAGEOPTIMIZATION_NAME = "Storage Optimization";
        private readonly Extender extender = new Extender();

        public Extender Extender
        {
            get { return extender; }
        }

        #region 专门为控制real-time和schedule的tree用而定义，其他地方请不要使用
        private readonly RealTime realtime = new RealTime();

        public RealTime RealTime
        {
            get { return realtime; }
        }

        private readonly ExtenderSchedule extenderSchedule = new ExtenderSchedule();

        public ExtenderSchedule ExtenderSchedule
        {
            get { return extenderSchedule; }
        }
        #endregion


        private readonly FSArchiver fsArchiver = new FSArchiver();
        public FSArchiver FSArchiver
        {
            get { return fsArchiver; }
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

        private readonly BoxConnector boxConnector = new BoxConnector();

        public BoxConnector BoxConnector
        {
            get { return boxConnector; }
        }

        private readonly Records records = new Records();

        public Records Records
        {
            get { return records; }
        }

        private readonly PhysicalArchiver physicalArchiver = new PhysicalArchiver();
        public PhysicalArchiver PhysicalArchiver
        {
            get { return physicalArchiver; }
        }

        private readonly int so_convert_stub_to_content_job_dto_type = (int)JobTypes.SOConvertStubToContent;

        public int SO_CONVERT_STUB_TO_CONTENT_JOB_DTO_TYPE
        {
            get { return so_convert_stub_to_content_job_dto_type; }
        }

        private readonly int so_storage_report_job_dto_type = (int)JobTypes.SOStorageReport;

        public int SO_STORAGE_REPORT_JOB_DTO_TYPE
        {
            get { return so_storage_report_job_dto_type; }
        }

        private readonly int so_export_location_job_dto_type = (int)JobTypes.SOExportLocation;

        public int SO_EXPORT_LOCATION_JOB_DTO_TYPE
        {
            get { return so_export_location_job_dto_type; }
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
            result.Add(Extender);
            result.Add(Archiver);
            result.Add(Connector);
            result.Add(FSArchiver);
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
            jobTypes.Add(SO_STORAGE_REPORT_JOB_DTO_TYPE);
            jobTypes.Add(SO_EXPORT_LOCATION_JOB_DTO_TYPE);
            return jobTypes;
        }

        public override List<int> getCategories()
        {
            return null;
        }
    }

    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
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

        private readonly int so_extender_scheduled_incremental_job_dto_type = (int)JobTypes.SOExtenderScheduledIncremental;

        private readonly int so_extender_dataupgrade_job_dto_type = (int)JobTypes.ExtenderDataUpgrade;

        private readonly int so_EBS_stub_upgrade_job_dto_type = (int)JobTypes.EBSStubUpgrade;

        private readonly int so_stub_db_config_job_dto_type = (int)JobTypes.SOConfigStubDB;

        private readonly int so_move_blob_job_dto_type = (int)JobTypes.SOMoveBlobToolJob;

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

        public int SO_EXTENDER_SCHEDULED_INCREMENTAL_JOB_DTO_TYPE
        {
            get { return so_extender_scheduled_incremental_job_dto_type; }
        }

        public int SO_EXTENDER_DATAUPGRADE_JOB_DTO_TYPE
        {
            get { return so_extender_dataupgrade_job_dto_type; }
        }

        public int SO_EBS_STUB_UPGRADE_JOB_DTO_TYPE
        {
            get { return so_EBS_stub_upgrade_job_dto_type; }
        }

        public int SO_MOVE_BLOB_JOB_DTO_TYPE
        {
            get { return so_move_blob_job_dto_type; }
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

    #region 专门为控制tree的license定义，其他地方还要使用Extender
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RealTime : AveModule
    {

        private const string name = "Real Time";

        private readonly BlobProvider blobProvider = new BlobProvider();
        public BlobProvider BlobProvider
        {
            get { return blobProvider; }
        }

        public override List<string> getAllAgentTypes()
        {
            return null;
        }

        public override int ID
        {
            get
            {
                return 0;
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
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExtenderSchedule : AveModule
    {

        private const string name = "Extender Schedule";

        private readonly BlobProvider blobProvider = new BlobProvider();
        public BlobProvider BlobProvider
        {
            get { return blobProvider; }
        }

        public override List<string> getAllAgentTypes()
        {
            return null;
        }

        public override int ID
        {
            get
            {
                return 0;
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
            get { return DisplayMode.None; }
        }
    }
    #endregion

    public class FSArchiver : AveModule
    {
        public const string AGENT_TYPE_FILE_SYSTEM_ARCHIVER = AgentTypes.AGENT_TYPE_FILE_SYSTEM_ARCHIVER;

        private const string name = "File System Archiver";

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_FILE_SYSTEM_ARCHIVER_ID;
            }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get
            {
                return DisplayMode.Disable;
            }
        }

        public override string Name
        {
            get
            {
                return name;
            }
        }

        public int SO_FSARCHIVER_SCAN_JOB_DTO_TYPE
        {
            get {return (int)JobTypes.FSArchiverScan; }
        }

        public int SO_FSARCHIVER_FULL_BACKUP_JOB_DTO_TYPE
        {
            get { return (int)JobTypes.FSArchiverBackupFull; }
        }

        public int SO_FSARCHIVER_TESTRUN_TYPE
        {
            get { return (int)JobTypes.FSArchiverTestJob; }
        }
        public int SO_FSARCHIVER_INC_BACKUP_JOB_DTO_TYPE
        {
            get { return (int)JobTypes.FSArchiverBackupInc; }
        }

        public int SO_FSARCHIVER_INC_Scan_JOB_DTO_TYPE
        {
            get { return (int)JobTypes.FSArchiverScanInc; }
        }
        public int SO_FSARCHIVER_FULL_TEXT_INDEX_JOB_DTO_TYPE
        {
            get { return (int)JobTypes.FSArchiverFullTextIndex; }
        }
        public int SO_FSARCHIVER_MERGE_INDEX_JOB_DTO_TYPE
        {
            get { return (int)JobTypes.FSArchiverMergeIndex; }
        }
        public int SO_FSARCHIVER_DOWNLOAD_JOB_DTO_TYPE
        {
            get { return (int)JobTypes.FSArchiverDownloadJob; }
        }

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AgentTypes.AGENT_TYPE_FILE_SYSTEM_ARCHIVER);
            return agentTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.Add((int)JobTypes.FSArchiverScan);
            jobTypes.Add((int)JobTypes.FSArchiverBackupFull);
            jobTypes.Add((int)JobTypes.FSArchiverBackupInc);
            jobTypes.Add((int)JobTypes.FSArchiverMergeIndex);
            jobTypes.Add((int)JobTypes.FSArchiverDownloadJob);
            jobTypes.Add((int)JobTypes.FSArchiverFullTextIndex);
            return jobTypes;
        }

        public override List<int> getAllPlanTypes()
        {
            return null;
        }

        public override List<int> getCategories()
        {
            return new List<int>() { (int)PlanCategory.FSArchiver };
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }
    }

    public class PhysicalArchiver : AveModule
    {
        public const string AGENT_TYPE_FILE_PHYSICAL_ARCHIVER = AgentTypes.AGENT_TYPE_ARCHIVER;

        private const string name = "Physical Archiver";

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_ARCHIVER_ID;
            }
        }

        public override DisplayMode ModuleDisplayMode
        {
            get
            {
                return DisplayMode.Disable;
            }
        }

        public override string Name
        {
            get
            {
                return name;
            }
        }

        public int SO_PHYSICAL_ARCHIVER_JOB_DTO_TYPE
        {
            get { return (int)JobTypes.PhysicalArchiver; }
        }
        

        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AgentTypes.AGENT_TYPE_ARCHIVER);
            return agentTypes;
        }

        public override List<int> getAllJobTypes()
        {
            List<int> jobTypes = new List<int>();
            jobTypes.Add((int)JobTypes.PhysicalArchiver);
            return jobTypes;
        }

        public override List<int> getAllPlanTypes()
        {

            return null;
        }

        public override List<int> getCategories()
        {
            return new List<int>() { (int)PlanCategory.PhysicalArchiver };
        }

        public override List<AveModule> getSubModules()
        {
            return null;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
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
        private readonly int so_archiver_testrun_job_dto_type = (int)JobTypes.ArchiverTestJob;

        private readonly int so_archiver_scan_job_dto_type = (int)JobTypes.ArchiverScan;

        private readonly int so_archiver_incremental_scan_job_dto_type = (int)JobTypes.ArchiverIncrementalScan;

        private readonly int so_archiver_backup_job_dto_type = (int)JobTypes.ArchiverBackup;

        private readonly int so_archiver_lifecycle_backup_job_dto_type = (int)JobTypes.ArchiverLifecycleBackup;

        private readonly int so_archiver_merge_index_job_dto_type = (int)JobTypes.ArchiverMergeIndex;

        private readonly int so_archiver_veo_merge_job_dto_type = (int)JobTypes.ArchiverVEOMergeJob;

        private readonly int so_archiver_retention_job_dto_type = (int)JobTypes.ArchiverRetention;

        private readonly int so_archiver_deletedatacollection_job_dto_type = (int)JobTypes.ArchiverDeleteDataCollection;

        private readonly int so_archiver_retention_approval_job_dto_type = (int)JobTypes.ArchiverRetentionApprovalExport;

        private readonly int so_archiver_approvalexport_job_dto_type = (int)JobTypes.ArchiverApprovalExport;

        private readonly int so_archiver_approvealert_job_dto_type = (int)JobTypes.ArchiverApproveAlert;

        private readonly int so_archiver_emailalert_job_dto_type = (int)JobTypes.ArchiverEmailAlert;

        private readonly int so_archiver_restore_job_dto_type = (int)JobTypes.ArchiverRestore;

        private readonly int so_end_user_archiver_backup_job_dto_type = (int)JobTypes.EndUserArchiverBackup;

        private readonly int so_end_user_merge_index_job_dto_type = (int)JobTypes.EndUserMergeIndex;

        private readonly int so_archiver_data_import_job_dto_type = (int)JobTypes.ArchiverUpgradeData;


        private readonly int so_archiver_full_text_index_job_dto_type = (int)JobTypes.ArchiverFullTextIndexJob;

        private readonly int so_end_user_restore_job_dto_type = (int)JobTypes.EndUserRestore;

        private readonly int so_end_user_archiver_sync_job_dto_type = (int)JobTypes.EndUserArchiverSyncJob;

        public int SO_ARCHIVER_SCAN_JOB_DTO_TYPE
        {
            get { return so_archiver_scan_job_dto_type; }
        }

        public int SO_ARCHIVER_INCREMENTAL_SCAN_JOB_DTO_TYPE
        {
            get { return so_archiver_incremental_scan_job_dto_type; }
        }
        public int SO_ARCHIVER_TESTRUN_JOB_DTO_TYPE
        {
            get { return so_archiver_testrun_job_dto_type; }
        }
        public int SO_ARCHIVER_BACKUP_JOB_DTO_TYPE
        {
            get { return so_archiver_backup_job_dto_type; }
        }
        public int SO_ARCHIVER_LIFECYCLE_BACKUP_JOB_DTO_TYPE
        {
            get { return so_archiver_lifecycle_backup_job_dto_type; }
        }
        public int SO_ARCHIVER_MERGEINDEX_JOB_DTO_TYPE
        {
            get { return so_archiver_merge_index_job_dto_type; }
        }
        public int SO_ARCHIVER_VEOMERGE_JOB_DTO_TYPE
        {
            get { return so_archiver_veo_merge_job_dto_type; }
        }

        public int SO_ARCHIVER_RETENSION_JOB_DTO_TYPE
        {
            get { return so_archiver_retention_job_dto_type; }
        }
        public int SO_ARCHIVER_RETENSIONDELETEDATACELLECTION_JOB_TYPE
        {
            get { return so_archiver_deletedatacollection_job_dto_type; }
        }
        public int SO_ARCHIVER_RETENTION_APPROVAL_JOB_DTO_TYPE
        {
            get { return so_archiver_retention_approval_job_dto_type; }
        }
        public int SO_ARCHIVER_APPROVALEXPORT_JOB_DTO_TYPE
        {
            get { return so_archiver_approvalexport_job_dto_type; }
        }
        public int SO_ARCHIVER_APPROVEALERT_JOB_TYPE
        {
            get { return so_archiver_approvealert_job_dto_type; }
        }

        public int SO_ARCHIVER_EMAILALERT_JOB_TYPE
        {
            get { return so_archiver_emailalert_job_dto_type; }
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
        public int SO_END_USER_ARCHIVER_SYNC_JOB_DTO_TYPE
        {
            get { return so_end_user_archiver_sync_job_dto_type; }
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
            jobTypes.Add(SO_ARCHIVER_INCREMENTAL_SCAN_JOB_DTO_TYPE);
            jobTypes.Add(SO_ARCHIVER_TESTRUN_JOB_DTO_TYPE);
            jobTypes.Add(SO_ARCHIVER_BACKUP_JOB_DTO_TYPE);
            jobTypes.Add(SO_ARCHIVER_LIFECYCLE_BACKUP_JOB_DTO_TYPE);
            jobTypes.Add(SO_ARCHIVER_RESTORE_JOB_DTO_TYPE);
            jobTypes.Add(SO_ARCHIVER_APPROVEALERT_JOB_TYPE);
            jobTypes.Add(SO_ARCHIVER_EMAILALERT_JOB_TYPE);
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

    //[AveModuleAttribute("Account Manager", DisplayMode.None)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
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

        public int CONNECTOR_SYNC_NOW_JOB_TYPE
        {
            get
            {
                return (int)JobTypes.ConnectorSyncNow;
            }
        }

        public int CONNECTOR_REPORT_JOB_TYPE
        {
            get
            {
                return (int)JobTypes.ConnectorReportJob;
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

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    public class BoxConnector : AveModule
    {
        private const string name = "Cloud Connect";

        public const string AGENT_TYPE_CONNECTOR = AgentTypes.AGENT_TYPE_CONNECTOR;          //35184372088832L


        public const string AGENT_TYPE_CONNECTOR_VIDEO = AgentTypes.AGENT_TYPE_CONNECTOR_VIDEO;          //140737488355328L


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

        public int CONNECTOR_SYNC_NOW_JOB_TYPE
        {
            get
            {
                return (int)JobTypes.ConnectorSyncNow;
            }
        }

        public int CONNECTOR_REPORT_JOB_TYPE
        {
            get
            {
                return (int)JobTypes.ConnectorReportJob;
            }
        }

        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_CLOUDCONNECT_ID;
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
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.None)]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BlobProvider : AveModule
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

    [DataContract(Namespace = ContractConstants.Namespace)]
    [AveModuleAttribute("Tenant Permission", DisplayMode.None)]
    [AveModuleAttribute("System Permission", DisplayMode.Available)]
    public class Records : AveModule
    {
        private const string name = "Records";

        public const string AGENT_TYPE_RECORDS = AgentTypes.AGENT_TYPE_ARCHIVER;     
         
        public override List<string> getAllAgentTypes()
        {
            List<string> agentTypes = new List<string>();
            agentTypes.Add(AGENT_TYPE_RECORDS); 
            return agentTypes;
        }

        public int RECORDS_DATA_SYNC_JOB_DTO_TYPE
        {
            get
            {
                return (int)JobTypes.RecordsDataSync;
            }
        }
        public int RECORDS_FS_DATA_SYNC_JOB_DTO_TYPE
        {
            get
            {
                return (int)JobTypes.RecordsFSDataSync;
            }
        }
        public int RECORDS_SHAREPOINT_SETTING_JOB_DTO_TYPE
        {
            get
            {
                return (int)JobTypes.RecordsSharepointSetting;
            }
        }
        public int RECORDS_UNIQUE_ID_JOB_DTO_TYPE
        {
            get
            {
                return (int)JobTypes.RecordsUniqueID;
            }
        }
        public int RECORDS_FORCE_RETENTION_JOB_DTO_TYPE
        {
            get
            {
                return (int)JobTypes.RecordsForceRetention;
            }
        }
        public int RECORDS_DISPOSAL_REPORT_JOB_DTO_TYPE
        {
            get
            {
                return (int)JobTypes.RecordsDisposalReport;
            }
        }
        public int RECORDS_DESTRUCTION_REPORT_JOB_DTO_TYPE
        {
            get
            {
                return (int)JobTypes.RecordsDestructionReport;
            }
        }
        public int RECORDS_TERM_USAGE_REPORT_JOB_DTO_TYPE
        {
            get
            {
                return (int)JobTypes.RecordsTermUsageReport;
            }
        }
        public int RECORDS_MOVE_JOB_DTO_TYPE
        {
            get
            {
                return (int)JobTypes.RecordsMove;
            }
        }

        public int RECORDS_FS_RECLASSIFY_JOB_DTO_TYPE
        {
            get
            {
                return (int)JobTypes.RecordsFSReclassify;
            }
        }
        public int RECORDS_FS_FOLDER_HOLD_JOB_DTO_TYPE
        {
            get
            {
                return (int)JobTypes.RecordsFSFolderHold;
            }
        }

        public int RECORDS_Physical_ExplorerTimer_JOB_DTO_TYPE
        {
            get
            {
                return (int)JobTypes.PhysicalExplorerTimer;
            }
        }

        public int RECORDS_PHYSICAL_AVAILABLESPACE_REPORT_JOB_DTO_TYPE
        {
            get
            {
                return (int)JobTypes.RecordsAvailableSpaceReport;
            }
        }
        public override int ID
        {
            get
            {
                return AveModuleID.MODULE_TYPE_DOCAVE_RECORDS_ID;
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
            return new List<int>() { RECORDS_DATA_SYNC_JOB_DTO_TYPE, RECORDS_SHAREPOINT_SETTING_JOB_DTO_TYPE };
        }

        public override List<int> getCategories()
        {
            return new List<int>() { (int)PlanCategory.Records};
        }


        public override DisplayMode ModuleDisplayMode
        {
            get { return DisplayMode.Available; }
        }
    }

}
