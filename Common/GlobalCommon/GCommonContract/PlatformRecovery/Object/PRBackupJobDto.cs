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




using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRBackupJobDto : BaseJobDto
    {
        [DataMember]
        public string ScheduleId { get; set; }
        [DataMember]
        public string LevelName { get; set; }
        [DataMember]
        public List<string> BakFileList { get; set; }
        [DataMember]
        public string StagingSqlServerId { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string SpAgentId { get; set; }
        [DataMember]
        public string JobReportId { get; set; }
        // restore from SQL setting
        [DataMember]
        public string AgentId { get; set; }
        [DataMember]
        public string ContentTree { get; set; }
        [DataMember]
        public string MachineName { get; set; }
        // 加密字符GUID
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string ProfileGuid { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string AgentVersion { get; set; }
        // RC performance使用该属性
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string MediaName { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string FarmStr { get; set; }
        //  保存一个可用最佳的media service Id
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string MediaId { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string AgentName { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_7)]
        public string BackupOptionStr { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_8)]
        public string BackupMethodStr { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_9)]
        public string IsPersistentStr { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_10)]
        public string IsVDBStr { get; set; }
        // 标示retention是否完成
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_2)]
        public RetentionIndicatortatus IndicatorStatus { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_4)]
        public int PlatformType { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_3)]
        public int SpVersion { get; set; }
        [DataMember]
        public int Weight { get; set; }
        /// <summary>site collection, site, folder, item, itemVersion</summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_5)]
        public PRBackupLevel BackupLevel { get; set; }
        /// <summary>标识Retention中Backup Management Group选项</summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_6)]
        public BackupManagementGroupType BackupGroupType { get; set; }
        /// <summary>标识generating VDBmapping的状态(Maintenance和JobMonitor使用)</summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_7)]
        public JobExtendStatus MappingStatus { get; set; }
        /// <summary>标识数据从snapshot复制到media是否成功的状态(Maintenance和JobMonitor使用)</summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_8)]
        public JobExtendStatus CopyStatus { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_9)]
        public JobExtendStatus IndexStatusStr { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.CLOB_1)]
        public string PRScheduleAdvanceOptionStr { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_10)]
        public int VerifyStatus { get; set; }
        #region 以下两个属性不再使用,属性移到PRBackupJobExtentionDto中的PRSNBackupInfoDto
        // 存放smsp备份信息
        [DataMember]
        //[ColumnMapAttribute(DBColumn = ContractConstants.CLOB_2)]
        public string PRSNBackupInfoDtoStr { get; set; }
        [DataMember]
        public PRSNBackupInfoDto PRSNBackupInfoDto { get; set; }
        #endregion

        [DataMember]
        public PRBackupJobExtentionDto PRBackupJobExtentionDto { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.CLOB_2)]
        public string PRBackupJobExtentionDtoStr { get; set; }

        #region Retention状态
        // Retention后的状态
        [DataMember]
        public JobRetentionStatus JobRetentionState
        {
            get { return jobRetentionState; }
            set { jobRetentionState = value; }
        }
        private JobRetentionStatus jobRetentionState = JobRetentionStatus.Unknown;
        #endregion

        #region defer 设置
        [DataMember]
        public bool IsCopySnapshot { get; set; }
        [DataMember]
        public bool IsDeferSnapshot { get; set; }
        [DataMember]
        public bool IsDeferVDBMapping { get; set; }
        [DataMember]
        public bool IsDeferIndex { get; set; }
        #endregion
        
        #region backup plan属性
        // 不存入job表,随job对象存入prMasterIndex表
        [DataMember]
        public bool CopyOnly { get; set; }
        #endregion

        #region maintenance界面级别属性
        // 不存入job表
        [DataMember]
        public PRBackupLevel MaintenancePRBackupLevel { get; set; }
        #endregion

        [DataMember]
        public string ProtectionKeyGUIDGuid { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RetentionIndicatortatus
    {
        // 未做retention
        [EnumMember]
        [Description("")]
        Validity = 0,

        // 做了retention
        [EnumMember]
        Removed = 1
    }

    /// <summary>
    /// 对应显示在JobMonitor上的Mapping,Index,CopyData状态
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum JobExtendStatus
    {
        /// <summary>
        /// 备份的所有datanode中，有可以生成Mapping或Copy Snapshot的datanode，但由于Defer或BackupJob失败，导致未生成Mapping 或Copy Snapshot(MappingStatus,CopyStatus)
        /// </summary>
        [EnumMember]
        [Description("Not Started")]
        NotStart = 0,

        /// <summary>
        /// 备份的所有datanode中，没有Mapping或Index或CopyData失败的DataNode，且有Mapping或Index或CopyData成功的DataNode(MappingStatus,IndexStatus,CopyStatus)
        /// </summary>
        [EnumMember]
        [Description("Successful")]
        Succeed = 2,

        /// <summary>
        /// 备份的所有datanode中，没有Mapping或Index或CopyData成功的DataNode，且有失败的DataNode(MappingStatus,IndexStatus,CopyStatus)
        /// </summary>>
        [EnumMember]
        [Description("Failed")]
        Failed = 4,

        /// <summary>
        /// 备份的所有datanode中，有正在生成Index的DataNode(IndexStatus)
        /// </summary>
        [EnumMember]
        [Description("Indexing...")]
        Indexing = 3,

        /// <summary>
        /// 备份的所有DataNode中，有由于DeferIndex而只生成SiteCollectionLevel的Index的DataNode(IndexStatus)
        /// </summary>
        [EnumMember]
        [Description("Partial")]
        Partial = 5,

        /// <summary>
        /// 备份的所有DataNode中，有正在生成Mapping的DataNode(MappingStatus)
        /// </summary>
        [EnumMember]
        [Description("Mapping...")]
        Mapping =6,

        /// <summary>
        /// 备份的所有DataNode中，有正在向Media发送数据的DataNode(CopyStatus)
        /// </summary>
        [EnumMember]
        [Description("Copying...")]
        Copying = 7,

        /// <summary>
        /// 备份的所有DataNode中，有成功的DataNode，且有失败的DataNode(MappingStatus,IndexStatus,CopyStatus)
        /// </summary>
        [EnumMember]
        [Description("Finished with exception")]
        CompleteException = 8,

        /// <summary>
        /// 备份的所有DataNode中，没有支持生成Index或Mapping或CopyData的DataNode(MappingStatus,IndexStatus,CopyStatus)
        /// </summary>
        [EnumMember]
        [Description("Unsupported")]
        Nonsupport = 9,

        /// <summary>
        /// VSS备份时，由于IsPersistSnapshot && !IsCopyData而未向Media发送数据的情况(CopyStatus)
        /// </summary>
        [EnumMember]
        [Description("Unselected")]
        NotCopyData = 10,

        /// <summary>
        /// 备份时IndexLevel == None的情况(IndexStatus)
        /// </summary>
        [EnumMember]
        [Description("None Level")]
        NoneLevel = 11,

        [EnumMember]
        [Description("Verifying")]
        Verifying = 12,

        [EnumMember]
        [Description("N/A")]
        NotAvailable  = 20,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ExtendStatusType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        MappingStatus = 1,
        [EnumMember]
        CopyStatus = 2,
        [EnumMember]
        IndexStatus = 3,
        [EnumMember]
        VerifyStatus = 4,
    }
}
