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
using AvePoint.GCommon.Contract.GranularRestore.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.SuperUserConfiguration.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    /// <summary>
    /// Archchiver模块与Agent通信使用此contract，修改此contract需要通知Agent端
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverMessage : AveMessage
    {
        [DataMember]
        public ArchiverAction Action { get; set; }

        [DataMember]
        public ArchiverResponse Response { get; set; }

        [DataMember]
        public string ProcessingPoolId { get; set; }

        //Use SOJob instead of this property
        //原来的StopSOJobRequest，用一个JobID可以替换。
        [DataMember]
        public string SoStopJobID { get; set; }

        [DataMember]
        public SOJob Job { get; set; }
        //InitBaseJobDetail需要用到SOPlan。
        [DataMember]
        public SOPlan Plan { get; set; }

        [DataMember]
        public bool EnableSuperUserDecryptsFiles { get; set; }

        /// <summary>
        /// key is sub job id, value is super user configuration
        /// </summary>
        [DataMember]
        public Dictionary<string, SuperUserConfigurationDto> SuperUserSubJobMappings { get; set; }

        [DataMember]
        public string SubJobId { get; set; }

        [DataMember]
        public string SourceFlag { get; set; } //判断数据源

        [DataMember]
        public List<RuleNodeContract> ScheduledConfigs { get; set; }
        /// <summary>
        /// node对应的RuleCollection，发给agent端做数据
        /// </summary>
        [DataMember]
        public RuleCollection RuleCollection { get; set; }

        [DataMember]
        public Dictionary<string, RuleNodeContract> SubJobConfigs { get; set; }

        [DataMember]
        public ArchiverBackupRequest ArchiverBackupRequest { get; set; }

        [DataMember]
        public Dictionary<string, ArchiverBackupRequest> ConfigForMedia { get; set; }

        [DataMember]
        public Dictionary<string, MergeIndexJobInfo> MergeIndexJobInfos { get; set; }

        [DataMember]
        public ArchiverJobRequest ArchiverJobRequest { get; set; }

        /// <summary>
        /// 标记是full还是Incremental
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public ArchiverType ArchiverType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<SiteCollectionReportInfo> ApprovalSiteInfo { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ValidateSharePointUrlRequest ValidateSharePointUrlRequest { get; set; }

        [DataMember]
        public ArchiverRestoreJobRequest ArchiverRestoreJobRequest { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ArchiverVeoMergeRequest ArchiverVEOMergeRequest { get; set; } //add for RevIM export
        //for end user archiver web application end user archiver feature
        //Use ArchiverTreeNode instead ?
        [DataMember]
        public List<SOTreeNode> SPTreeNodes { get; set; }

        [DataMember]
        public Dictionary<string, EndUserFeatureStatus> SiteFeatureStatus { get; set; }

        [DataMember]
        public List<string> SiteIDs { get; set; }

        [DataMember]
        public bool IsSolutionDeployed { get; set; }

        [DataMember]
        public List<TagMaping> ArchiverTagMappings { get; set; }

        [DataMember]
        public AzureTableConnectContract ArchiverDBInfo { get; set; }

        [DataMember]
        public BlobProviderContract BlobProviderContract { get; set; }

        [DataMember]
        public Dictionary<string, CacheSettingDto> MediaCacheInfoDic { get; set; }

        /// <summary>
        /// End user archiver用于给client发送其所需要的信息
        /// </summary>
        [DataMember]
        public string EndUserArchiverMetaData { get; set; }

        [DataMember]
        public SPTreeNodeDto SPTreeNode { get; set; }
        //ask
        [DataMember]
        public long PhysicalDeviceData { get; set; }

        [DataMember]
        public Guid[] physicalDevices { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string RegisterSiteId { set; get; }

        /// <summary>
        /// VEO 导出时特定字段的默认值,  Archiver Backup Job时使用
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public Dictionary<string, string> VEOMetadataMapping { set; get; } //add for RevIM export

        /// <summary>
        /// for report center 
        /// </summary>
        [DataMember]
        public Dictionary<Guid, long> PhysicalDeviceDatas { get; set; }

        #region Move Index Message

        [DataMember]
        public LogicalDeviceDto SrcIndexDevice { set; get; }

        [DataMember]
        public LogicalDeviceDto DestIndexDevice { set; get; }

        [DataMember]
        public string FarmName { get; set; }

        [DataMember]
        public List<string> SiteUrls { get; set; }

        [DataMember]
        public string WebApp { get; set; }

        #endregion

        //add for support Group Mailbox
        [DataMember]
        public ExchangeOnlineMessage ExchangeOnlineMessage { get; set; }

        /// <summary>
        /// Default 0 is started by DAO self.
        /// 1 is started by Records Online.
        /// ummary>
        [DataMember]
        public int RunDAOArchiverJobProduct { get; set; }

        /// <summary>
        /// for Physical Related Phy-SP Action. 
        /// </summary>
        [DataMember]
        public LogicalDeviceDto PhysicalRecordsLogicalDevice { get; set; }

        [DataMember]
        public RecordsGlobalStorageSettingsDto RecordsGlobalStorageSettingsDto { get; set; }
    }

    /// <summary>
    /// now only for Physical Records Job,may be use for other job type.
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RecordsGlobalStorageSettingsDto
    {
        [DataMember]
        public StoragePolicyDto RecordsStoragePolicyDto { get; set; }

        [DataMember]
        public DataEncryptionInfoWrapper DataEncryptionInfoWrapper { get; set; }

        [DataMember]
        public RecordsGlobalStorageSettings RecordsGlobalStorageSettings { get; set; }
    }

    /// <summary>
    /// define archiver scan job request
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverJobRequest
    {
        [DataMember]
        public List<RuleNodeContract> ArchiverConfigs { get; set; }

        [DataMember]
        public StubDatabaseInfo ArchiverDBInfo { set; get; }

        [DataMember]
        public SOJob Job { get; set; }

        [DataMember]
        public SOPlan Plan { get; set; }

        [DataMember]
        public Dictionary<string, CacheSettingDto> MediaCacheInfoDic { get; set; }

        /// <summary>
        /// End user archiver用于给client发送其所需要的信息
        /// </summary>
        [DataMember]
        public string MetaData { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverRestoreJobRequest
    {
        [DataMember]
        public RestoreConfig Config { get; set; }

        [DataMember]
        public ArchiverRestoreRequest ConfigForMedia { get; set; }

        [DataMember]
        public ServiceDto MediaInfo { get; set; }
    }
    //add for RevIM export
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverVeoMergeRequest
    {
        [DataMember(EmitDefaultValue = false)]
        public Dictionary<string, List<PhysicalDeviceDto>> JobIdAndExportLocation { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsDeleteOldFile { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public double FileSize { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int FileNumber { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string FoldName { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public SPType SPType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverResponse
    {
        [DataMember]
        public ArchiverResponseType MessageType { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ArchiverResponseType
    {
        [EnumMember]
        Successful,
        [EnumMember]
        Failed
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ArchiverAction
    {
        [EnumMember]
        NONE = 0,

        //SO action for archiver
        [EnumMember]
        ARCHIVER_SCAN_JOB_REQUEST = 801,

        [EnumMember]
        ARCHIVER_BACKUP_JOB_REQUEST = 802,

        [EnumMember]
        ARCHIVER_RESTORE_JOB_REQUEST = 803,

        [EnumMember]
        END_USER_ARCHIVER_GET_FEATURE = 804,

        [EnumMember]
        END_USER_ARCHIVER_INSTALL_FEATURE = 805,

        [EnumMember]
        RELATIVEDATA_ARCHIVER_BACKUP_JOB_REQUEST = 806,

        [EnumMember]
        ENDUSER_ARCHIVER_RESTORE_JOB_REQUEST = 807,

        [EnumMember]
        VALIDATE_END_USER_ARCHIVER_TAG_MAPPING = 808,

        [EnumMember]
        ARCHIVER_DATABASE_REQUEST = 809,

        [EnumMember]
        ARCHIVER_TEST_RUN_REQUEST = 810,

        [EnumMember]
        ARCHIVER_VEO_MERGE_REQUEST = 1010,//add for RevIM export

        [EnumMember]
        SEND_STOP_SO_JOB_REQUEST = 1101,

        //??
        [EnumMember]
        ARCHIVER_RETENTION_METADATA = 1200,
        //不太清楚PHYCIAL_DEVICE_ARCHIVE_DATA_REQUEST，PHYCIAL_DEVICE_ARCHIVE_DATA_REQUEST_FOR_REPORTCENTER这两个的用途？
        [EnumMember]
        PHYCIAL_DEVICE_ARCHIVE_DATA_REQUEST = 1001,

        [EnumMember]
        PHYCIAL_DEVICE_ARCHIVE_DATA_REQUEST_FOR_REPORTCENTER = 1002,

        [EnumMember]
        MANUAL_APPROVE_REQUEST = 1007,

        [EnumMember]
        MANUAL_APPROVE_OFFICE365_REQUEST = 1008,

        [EnumMember]
        ARCHIVER_MERGEINDEX_REQUEST = 2000,

        [EnumMember]
        ARCHIVER_MOVEINDEX_REQUEST = 2001,
        
        [EnumMember]
        ARCHIVER_EXCHANGE_SCAN_JOB_REQUEST = 2002,

        [EnumMember]
        ARCHIVER_EXCHANGE_BACKUP_JOB_REQUEST = 2003,

        [EnumMember]
        ARCHIVER_EXCHANGE_MERGEINDEX_REQUEST = 2004,

        [EnumMember]
        ARCHIVER_PHYSICAL_RECORDS_REQUEST = 2005,

        [EnumMember]
        ENDUSER_ARCHIVER_PHYSICAL_RECORDS_REQUEST = 2006,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteCollectionReportInfo
    {
        [DataMember(EmitDefaultValue = false)]
        public string farmName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<RuleNodeContract> Rules { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public SPType SPtype { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SiteCollectionUrl { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ValidateSharePointUrlRequest
    {
        [DataMember(EmitDefaultValue = false)]
        public string URL { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string UserName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Password { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverExtendSettingDto
    {
        [DataMember]
        public bool IsMutiApp { get; set; }

        [DataMember]
        public bool UseHighSpeedCreateStub { get; set; }

        [DataMember]
        public bool KeepHighSpeedImportData { get; set; }

        [DataMember]
        public bool IsCGDiscovery { get; set; }

        [DataMember]
        public string CGDatabaseConnection { get; set; }

        [DataMember]
        public string SiteSummaryDBName { get; set; }

        [DataMember]
        public string SiteSummaryTableName { get; set; }

        [DataMember]
        public bool IsDeleteOnly { get; set; }

        [DataMember]
        public bool IsFileLevelBlock { get; set; }

        [DataMember]
        public bool IsArchiverLatestVersion { get; set; }

        public override string ToString()
        {
            return string.Format("Archiver extend setting [IsMutiApp {0}] [UseHighSpeedCreateStub {1}] [KeepHighSpeedImportData {2}] [IsCGDiscovery {3}] [CGDatabaseConnection {4}] [SiteSummaryDBName {5}] [SiteSummaryTableName {6}] [IsDeleteOnly {7}] [IsFileLevelBlock {8}] [IsArchiverLatestVersion {9}]",
                                                           IsMutiApp, UseHighSpeedCreateStub, KeepHighSpeedImportData, IsCGDiscovery, !string.IsNullOrEmpty(CGDatabaseConnection), SiteSummaryDBName, SiteSummaryTableName, IsDeleteOnly, IsFileLevelBlock, IsArchiverLatestVersion);
        }
    }
}