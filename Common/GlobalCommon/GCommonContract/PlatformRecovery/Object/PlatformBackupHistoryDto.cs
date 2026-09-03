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




namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PlatformBackupHistoryDto
    {
        /// <summary>
        /// primary key
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "Id")]
        public long Id { get; set; }
        /// <summary>
        /// the name of data node
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "Name")]
        public string Name { get; set; }
        /// <summary>
        /// Backup module
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "Module")]
        public PlatformBackupHistoryDto.PRModule Module { get; set; }
        /// <summary>
        /// Plan ID of the job
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "PlanId")]
        public string PlanId { get; set; }
        /// <summary>
        /// Plan name of the job
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "PlanName")]
        public string PlanName { get; set; }
        /// <summary>
        /// Job id of the job
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "JobId")]
        public string JobId { get; set; }
        /// <summary>
        /// the backup database name
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "DatabaseName")]
        public string DatabaseName { get; set; }
        /// <summary>
        /// SQL Alias name
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "SqlAlias")]
        public string SqlAlias { get; set; }
        /// <summary>
        /// SQL Instance name
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "Instance")]
        public string Instance { get; set; }
        /// <summary>
        /// Backup SQL agent name
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "Agent")]
        public string Agent { get; set; }
        /// <summary>
        /// Backup SQL cluster name
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "Cluster")]
        public string Cluster { get; set; }
        /// <summary>
        /// All the nodes' name of cluster
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "ClusterNodes")]
        public string ClusterNodes { get; set; }
        /// <summary>
        /// the size of the database
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "DatabaseSize")]
        public long DatabaseSize { get; set; }
        /// <summary>
        /// the size of the backup data
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "BackupSize")]
        public long BackupSize { get; set; }
        /// <summary>
        /// all the files of the database
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "DatabaseFiles")]
        public string DatabaseFiles { get; set; }
        /// <summary>
        /// the backup method(VSS,VDI,NETAPP)
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "BackupMethod")]
        public PlatformBackupHistoryDto.PRBackupMethod BackupMethod { get; set; }
        /// <summary>
        /// the backup type(full, differential, incremental, copy only)
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "BackupType")]
        public PlatformBackupHistoryDto.PRBackupType BackupType { get; set; }
        /// <summary>
        /// whether is a copy full backup
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "Id")]
        public  bool IsCopyOnly { get; set; }
        /// <summary>
        /// backup media device information
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "MediaInfo")]
        public  string MediaInfo { get; set; }
        /// <summary>
        /// agent id in control database
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "AgentId")]
        public string AgentId { get; set; }
        /// <summary>
        /// Full path of the data node
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "FullPath")]
        public string FullPath { get; set; }
        /// <summary>
        /// First LSN of the database when backup
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "FirstLsn")]
        public decimal FirstLsn { get; set; }
        /// <summary>
        /// Last LSN of the database when backup
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "LastLsn")]
        public decimal LastLsn { get; set; }
        /// <summary>
        /// Check point LSN of the database when backup
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "CheckPointLsn")]
        public decimal CheckPointLsn { get; set; }
        /// <summary>
        /// DatabaseBackupLsn LSN of the database when backup
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "DatabaseBackupLsn")]
        public decimal DatabaseBackupLsn { get; set; }
        /// <summary>
        /// the start time of the backup of the database
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "BackupStartTime")]
        public DateTime BackupStartTime { get; set; }
        /// <summary>
        /// the finish time of the backup of the database
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "BackupFinishTime")]
        public DateTime BackupFinishTime { get; set; }
        /// <summary>
        /// real location of the database
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "DataRealLocation")]
        public short DataRealLocation { get; set; }
        /// <summary>
        /// data security of the backup
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "DataSecurity")]
        public short DataSecurity { get; set; }
        /// <summary>
        /// The snapshot set id of the backup
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "SnapShotSetId")]
        public string SnapShotSetId { get; set; }
        /// <summary>
        /// the control agent name of the backup
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "ControlAgent")]
        public virtual string ControlAgent { get; set; }
        /// <summary>
        /// the control agent id of the backup
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "ControlAgentId")]
        public virtual string ControlAgentId { get; set; }
        /// <summary>
        /// for blob backup
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "AODnsName")]
        public virtual string AODnsName { get; set; }
        /// <summary>
        /// for blob backup
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "AOReplicas")]
        public virtual string AOReplicas { get; set; }
        /// <summary>
        /// for blob backup
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "LoginXml")]
        public virtual string LoginXml { get; set; }
        /// <summary>
        /// VSS backup document and metadata document path
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = "DocumentPath")]
        public virtual string DocumentPath { get; set; }

        [Flags, DataContract(Namespace = ContractConstants.Namespace)]
        public enum PRModule
        {
            [EnumMember]
            PlatformBackup = 0x01,
            [EnumMember]
            HighAvailability = 0x02
        }
        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum PRBackupMethod
        {
            [EnumMember]
            VSS = 0,
            [EnumMember]
            VDI = 1,
            [EnumMember]
            NETAPP = 2,
        }
        [DataContract(Namespace = ContractConstants.Namespace)]
        public enum PRBackupType
        {
            [EnumMember]
            Full = 0,
            [EnumMember]
            Differential = 1,
            [EnumMember]
            Incremental = 2,
            [EnumMember]
            Copy = 3
        }
    }
}
