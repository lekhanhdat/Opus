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
using System.Threading.Tasks;

namespace AvePoint.RA.Common
{
    public class RecordsConstants
    {
        public static readonly string Records_Processor_Name = "RecordsProcessor.exe";
        public static readonly int Reocrds_Processor_Port = 18006;
        
        public const int RecordHold_Default = 0; 
        public const int RecordHold_Electronic = 1; //默认表示SP和EXO的老数据
        public const int RecordHold_PhyProfile = 2;
        public const int RecordHold_Personal = 3;

        public const int Explorer_RealTime_Success = 0;
        public const int Explorer_RealTime_Failed_Partial = 1;
        public const int Explorer_RealTime_Failed_All = 2;
        public const int Explorer_RealTime_Running = 3;
        public const int Explorer_RealTime_Finished = 4;

        public const int SubJob_Runnable_Exclude = -1;
        public const int SubJob_Runnable_Waiting = 0;
        public const int SubJob_Runnable_CanRun = 1;
        public const int SubJob_Runnable_Runing = 2;

        public const int TenantDBSize = 50;
        public const int ExplorerDBSize = 25;
        public const string ExplorerDBDefaultName = "RECO";

        public const string UniqueId_NoNeedRunJob = "3";

        public const string EXOLocationFormat = "{0}{1}_{2}";

        
        public const string RECORDS_APPLICATION_NAME = "AvePointRecords";
        public const string GOOGLE_CONTROL_APPLICATION_NAME = "GoogleControl";
        public const string CloudArchiving = "Office365Archiving";
        public const string OpusSO = "Office365Archiving";
        public const string ReCenter = "ReCenter";        
        public const string RECORDS_HYBRID_NAME = "HybridAgent";
        public const string RequestIdPrefix = "RC-";
        public const string Office365LogonUrl = "";
        public const string GraphResource = "";
        public const string RedirectOffice365LogOnUrl = "";
        public const string OPUS_MODULE_IL_NAME = "AvePointRecords";
        public const string OPUS_MODULE_SO_NAME = "OpusStorageOptimization";
        public const string OPUS_MODULE_DISCOVERY_NAME = "FileDiscoveryAndAnalysis";
        public const string OPUS_MODULE_GOOGLE_NAME = "InformationLifecycleGoogleWorkspace";
        public const string OPUS_MODULE_Salesforce_Discovery = "FileDiscoveryAndAnalysisForSalesforce";
        public const string OPUS_MODULE_Google_WorkSpace_Discovery = "FileDiscoveryAndAnalysisForGoogleWorkspace";
        public const string OPUS_MODULE_FileSystem_Discovery = "FileDiscoveryAndAnalysisForFileSystem";
        public static Guid FS_ROOT_GUID = new Guid("71A6C027-0773-4C6C-B0E5-8FA9F789B668");

        public static Guid BOX_ROOT_GUID = new Guid("F47AC10B-58CC-4372-A567-0E02B2C3D479");

        public static int ExplorerQueryPageSize = 20000;

        public static int ExplorerQueryPageSizeForTraining = 2000;

        public const string TYPE_STRING_ROOT = "Root";
        public const string TYPE_STRING_TERM_GROUP = "TermGroup";
        public const string TYPE_STRING_TERM_SET = "TermSet";
        public const string TYPE_STRING_TERM = "Term";
        public const string TYPE_STRING_SUB_TERM = "SubTerms";
        public const string TYPE_STRING_BOXES = "Boxes";
        public const string TYPE_STRING_FILES = "Files";
        public const string TYPE_STRING_PhyBox = "PhyBox";
        public const string TYPE_STRING_PhyFile = "PhyFile";

        //public const string SecurityTermCacheKeyPrefix = "SecurityTerms_";
        public const string PhysicalSubPermissionCacheKeyPrefix = "PhysicalSubPermission_";
        public const string DataCenterCacheKeyPrefix = "Datacenter_";
        public const string ArchiverDataBaseConfigCacheKeyPrefix = "ArchiverDataBaseConfig4ManagedIdentity_";//Change cache key by cloud archiver change RECO-25816

        public const int PhysicalLoanOrReturnBatchOperationMaxCount = 100;
        public const int PhysicalMoveBatchOperationMaxCount = 100;

        public static readonly Guid RECORD_DEFAULT_CONTAINER_ID = new Guid("C01A98AD-0D33-477B-A846-43AD41DDEE55");

        public static readonly string HOLD_ACTION_CHANGE = "change";
        public static readonly string HOLD_ACTION_APPEND = "append";

        /// <summary>
        /// Minimum number of training term is 5
        /// </summary>
        public const int TrainingTerm_MinimumNumber = 5;

        /// <summary>
        /// Minimum number of files per term is 20
        /// </summary>
        public const int TrainingFile_MinimumNumberPerTerm = 20;

        /// <summary>
        /// Maximum number of files per term is 500
        /// </summary>
        public const int TrainingFile_MaximumNumberPerTerm = 500;

        /// <summary>
        /// Maximum number of files per term is 500
        /// </summary>
        public const int TrainingFile_MaximumNumberPerTerm4Reclassify = 500;

        /// <summary>
        /// Maximum prediction file number per term is 50
        /// </summary>
        public const int TrainingFile_MaximumPredictionNumberPerTerm = 50;

        public const string TrainingData_FileName = "data.tsv";

        public const string RecordsProdName4BlobFolder = "records";

        public const string BuiltIn_ReviewRole_Name = "Review User";
        public const string BuiltIn_HoldRole_Name = "Hold Manager";

        public static readonly TimeSpan REGEX_DEFAULT_MATCH_TIMEOUT = TimeSpan.FromSeconds(5);

        public static readonly string AVEPOINT_DEFAULT_STORAGEID = "6A040C17-AF8A-4F1F-96C1-7CEB2E23B1F3";

        public static readonly string FAKE_SPECIFY_SITES_RULE_ID = "529604dd-9da7-4f60-b55c-f9743eb9775f";

        public static readonly string END_USER_ARCHIVE_RULE_ID = "1d9a3e26-2f08-477d-bc66-2f50c4a9a162";

        public static readonly string END_USER_DELETE_ONLY_RULE_ID = "c90f9c4d-9bb5-4c97-91c7-6d358488ec59";

        public static readonly string FAKE_SPECIFY_TEAMS_RULE_ID = "371d5b4a-a34c-44d1-91c4-daa7d1b603a2";

        public static readonly string CUSTOM_RECLASSIFY_APPID = "c37dbed4-2e50-4c0f-93b4-8c6cf114155a";
    }
}
