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

namespace AvePoint.GCommon.Contract.Server.Common.LogCollector
{
    public class LogConstants
    {
        public class PersistentLogParams
        {
            //区分不同类别的Log
            public static readonly string LogType = "LogType";

            /**************************************************************************************
            **例如可以将DBLevelBackup这个类的Name存入Dictionary的Value(object)中                 **   
            **如果这个值为空,默认读取EventDescription,                                           **
            **************************************************************************************/
            public static readonly string EventDescriptionType = "EventDescriptionType";
        }

        public class LogType
        {
            public static readonly string EventLog = "EventLog";
            public static readonly string NetAppASUPLog = "NetAppASUPLog";
        }

        #region Connector
        public class ConnectorSyncJob
        {
            public static readonly string Job = "Job";
            public static readonly string Control_Agent = "Control Agent";
            public static readonly string Farm = "Farm";
            public static readonly string SharePoint_Version = "SharePoint Version";
            public static readonly string Components = "Components";
            public static readonly string StartTime = "Start Time";
            public static readonly string FinishTime = "Finish Time";
            public static readonly string Exceptions = "Exceptions";
            public static readonly string Exception_Details = "Exception Details";
            public static readonly string Message = "Message";
        }
        #endregion

        #region SO Archiver
        public class ArchiverRuleModify
        {
            public static readonly string Object_Level = "Object Level";
            public static readonly string Name = "Name";
            public static readonly string Criteria = "Criteria";
            public static readonly string Storage_Policy = "Storage Policy";
        }

        public class ArchiverBackup
        {
            public static readonly string Job = "Job";
            public static readonly string Scope = "Scope";
            public static readonly string Farm = "Farm";
            public static readonly string Agent = "Agent";
            public static readonly string SharePoint_Version = "SharePoint Version";
            public static readonly string Components = "Components";
            public static readonly string StartTime = "Start Time";
            public static readonly string FinishTime = "Finish Time";
            public static readonly string Exceptions = "Exceptions";
            public static readonly string Exception_Details = "Exception Details";
        }

        public class ArchiverRestore
        {
            public static readonly string Job = "Job";
            public static readonly string RestoreType = "RestoreType";
            public static readonly string Farm = "Farm";
            public static readonly string Agent = "Agent";
            public static readonly string SharePoint_Version = "SharePoint Version";
            public static readonly string Components = "Components";
            public static readonly string Exceptions = "Exceptions";
            public static readonly string Exception_Details = "Exception Details";
        }

        public class ArchiverFullTextIndex
        {
            public static readonly string Job = "Job";
            public static readonly string MediaService = "Media Service";
            public static readonly string Farm = "Farm";
            public static readonly string SharePoint_Version = "SharePoint Version";
            public static readonly string Components = "Components";
            public static readonly string StartTime = "Start Time";
            public static readonly string FinishTime = "Finish Time";
            public static readonly string Exceptions = "Exceptions";
            public static readonly string Exception_Details = "Exception Details";
        }

        #endregion

        #region SO Storage Manager
        public class ScheduledStorageManager
        {
            public static readonly string Job = "Job";
            public static readonly string Scope = "Scope";
            public static readonly string Farm = "Farm";
            public static readonly string Agent = "Agent";
            public static readonly string SharePoint_Version = "SharePoint Version";
            public static readonly string Components = "Components";
            public static readonly string StartTime = "Start Time";
            public static readonly string FinishTime = "Finish Time";
            public static readonly string Exceptions = "Exceptions";
            public static readonly string Exception_Details = "Exception Details";
        }
        public class ConvertStubToContent
        {
            public static readonly string Job = "Job";
            public static readonly string Farm = "Farm";
            public static readonly string Agent = "Agent";
            public static readonly string SharePoint_Version = "SharePoint Version";
            public static readonly string Components = "Components";
            public static readonly string StartTime = "Start Time";
            public static readonly string FinishTime = "Finish Time";
            public static readonly string Exceptions = "Exceptions";
            public static readonly string Exception_Details = "Exception Details";
        }
        public class EBSProviderSetting
        {
            public static readonly string Farm = "Farm";
            public static readonly string SharePoint_Version = "SharePoint Version";
            public static readonly string Status = "Status";
        }

        public class RBSProviderSetting
        {
            public static readonly string Farm = "Farm";
            public static readonly string SharePoint_Version = "SharePoint Version";
            public static readonly string WebApplications = "WebApplications";
        }
        #endregion

        #region PR
        public class PlatformBackupRestore
        {
            public static readonly string Agent_Version = "Agent Version";
            public static readonly string Clone_Type = "Clone Type";
            public static readonly string Components = "Components";
            public static readonly string Control_Agent = "Control Agent";
            public static readonly string Data_Externalized_using_Archiver = "Data Externalized using Archiver";
            public static readonly string Data_Externalized_using_Extender = "Data Externalized using Extender";
            public static readonly string Data_Externalized_using_File_Share_Connector = "Data Externalized using File Share Connector";
            public static readonly string DeferredIndexingResult = "Deferred indexing result";
            public static readonly string Exception_Details = "Exception Details";
            public static readonly string Exceptions = "Exceptions";
            public static readonly string Farm = "Farm";
            public static readonly string FinishTime = "Finish Time";
            public static readonly string Include_Backup_Data = "Include Backup Data";
            public static readonly string Include_BLOB = "Include BLOB";
            public static readonly string Include_Customized_Volume = "Include Customized Volume";
            public static readonly string Include_Stub_DB = "Include Stub DB";
            public static readonly string Job = "Job";
            public static readonly string JobStatus = "Job status";
            public static readonly string Plan = "Plan";
            public static readonly string Provison_Type = "Provison Type";
            public static readonly string Related_Backup_Job_ID = "Related Backup Job ID";
            public static readonly string SharePoint_Version = "SharePoint Version";
            public static readonly string SMSP_DocAve_Version = "SMSP Version";
            public static readonly string StartTime = "Start Time";
            public static readonly string TypeOfRestoreUsed = "Types of restore used";
            public static readonly string UpdateSnapMirror = "SnapMirror Update";
            public static readonly string UpdateSnapVault = "SnapVault Update";
            public static readonly string VerificationResult = "Verification result";
            public static readonly string Whether_Use_WFA_Storage_Provision = "Whether Use WFA Storage Provision";

        }

        public class DBLevelRestore
        {
            public static readonly string TypeOfRestoreUsed = "Types of restore used";
        }

        public class BackupMaintenance
        {
            public static readonly string VerificationResult = "Verification result";
            public static readonly string DeferredIndexingResult = "Deferred indexing result";
            public static readonly string JobStatus = "Job status";
        }
        #endregion
        
        #region HA
        public class HASyncJob
        {
            public static readonly string SMSP_DocAve_Version = "SMSP Version";
            public static readonly string Plan = "Group";
            public static readonly string Job = "Job";
            public static readonly string Control_Agent = "Control Agent";
            public static readonly string ProductionFarm = "Production Farm";
            public static readonly string StandbyFarm = "Standby Farm";
            public static readonly string SharePoint_Version = "SharePoint Version";
            public static readonly string Sync_Method = "Sync Method";
            public static readonly string EanbleReadOnlyView = "Enable Read-Only View";
            public static readonly string IncludeExtenderData = "Include Storage Manager BLOB Data";
            public static readonly string IncludeConnectorData = "Include Connector BLOB Data";
            public static readonly string EnableRBSEBS = "Enable RBS/EBS During Failover";
            public static readonly string Components = "Components";
            public static readonly string StartTime = "Start Time";
            public static readonly string FinishTime = "Finish Time";
            public static readonly string Exceptions = "Exceptions";
            public static readonly string Exception_Details = "Exception Details";
            public static readonly string JobStatus = "Job Status";
        }
        public class HAFailoverFallbackJob
        {
            public static readonly string SMSP_DocAve_Version = "SMSP Version";
            public static readonly string Plan = "Group";
            public static readonly string Job = "Job";
            public static readonly string Control_Agent = "Control Agent";
            public static readonly string ProductionFarm = "Production Farm";
            public static readonly string StandbyFarm = "Standby Farm";
            public static readonly string SharePoint_Version = "SharePoint Version";
            public static readonly string Sync_Method = "Sync Method";
            public static readonly string Maintenance_Mode = "Maintenance mode";
            public static readonly string KeepDbReadOnly = "Keep database in read-only mode";
            public static readonly string EanbleReadOnlyView = "Enable Read-Only View";
            public static readonly string IncludeExtenderData = "Include Storage Manager BLOB Data";
            public static readonly string IncludeConnectorData = "Include Connector BLOB Data";
            public static readonly string EnableRBSEBS = "Enable RBS/EBS During Failover";
            public static readonly string Components = "Components";
            public static readonly string StartTime = "Start Time";
            public static readonly string FinishTime = "Finish Time";
            public static readonly string Exceptions = "Exceptions";
            public static readonly string Exception_Details = "Exception Details";
            public static readonly string JobStatus = "Job Status";
        }
        #endregion
        #region Installation
        public class Installation
        {
            public static readonly string AgentName = "Agent";
            public static readonly string SMSP_Version = "SMSP Version";
            public static readonly string SMSP_Port = "SMSP Port";
            public static readonly string SharePointVersion = "SharePoint Version";
            public static readonly string SMSQL_Version = "SMSQL Version";
            public static readonly string SnapDrive_Version = "SnapDrive Version";
            public static readonly string SQL_Edition = "SQL Edition";
        }
        #endregion

        public class AgentRegister
        {
            public static readonly string AgentName = "Agent Name";
            public static readonly string AgentType = "Agent Type";
            public static readonly string SharePointVersion = "SharePoint Version";
            public static readonly string AgentAddress = "Agent Address";
            public static readonly string RegisterTime = "Register Time";
            public static readonly string StartTime = "Start Time";
            public static readonly string StopTime = "Stop Time";
        }
        public class ControlService
        { 
            public static readonly string Servicename = "Service name";
            public static readonly string Host = "Host";
            public static readonly string Port = "Port";
            public static readonly string Version = "Version";
            public static readonly string Status = "Status";
        }

        public class MediaServiceStatus
        {
            public static readonly string Servicename = "Service name";
            public static readonly string Host = "Host";
            public static readonly string Port = "Port";
            public static readonly string Version = "Version";
            public static readonly string Status = "Status";
        }

        #region License Manager

        public class LicenseManager
        {
            public static readonly string LogKey = "Apply License Details : ";
        }

        public class LicenseModuleDetail
        {
            public static string ModuleName = "Module";
            public static string LicenseType = "License Type";
            public static string NumberofServers = "The number of Servers";
            public static string RegisteredServer = "Registered Servers";
            public static string ExpireTime = "Expiration Time";
            public static string Status = "Status";
        }

        #endregion

        #region SMSP ASUP

        public class AsupLogInfo
        {
            public static readonly string SmspVersion = "8.3";
        }

        #endregion
    }
}
