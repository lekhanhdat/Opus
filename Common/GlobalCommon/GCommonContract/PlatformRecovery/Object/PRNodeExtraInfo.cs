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
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract]
    public class PRNodeExtraInfo
    {
        [DataMember]
        public List<PRDBFileInfo> DBFileList { get; set; }
        [DataMember]
        public List<PRIISSettingInfo> IISSettingList { get; set; }
        [DataMember]
        public PRDBBackupRecord DBBackupRecord { get; set; }
        [DataMember]
        public PRStagingPolicyDto StagingPolicy { get; set; }
        [DataMember]
        public string VssComponentname { get; set; }
        [DataMember]
        public string VssLogicalPath { get; set; }
        [DataMember]
        public string WriterId { get; set; }
        [DataMember]
        public int VariantType { get; set; }//only for crawled property
        [DataMember]
        public string DataType { get; set; }//for crawled property and managed property
        [DataMember]
        public string Realname { get; set; }//for ssaname and crawled name
        [DataMember]
        public string Guids { get; set; }//for crawled propset
        [DataMember]
        public string DataXml { get; set; }
        [DataMember]
        public string SSANodeName { get; set; }
        [DataMember]
        public List<PRNodeTypeId> MetaIds { get; set; }
        [DataMember]
        public string CheckPointLsn { get; set; }
        [DataMember]
        public string FirstLsn { get; set; }
        [DataMember]
        public string LastLsn { get; set; }
        [DataMember]
        public string DatabaseBackupLsn { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string BackupStartTime { get; set; }
        [DataMember]
        public string BackupFinishTime { get; set; }
        [DataMember]
        public Guid SnapshotSetId { get; set; }
        [DataMember]
        public bool IsVssForceFull { get; set; }
        [DataMember]
        public bool IsVssCopyOnly { get; set; }
        [DataMember]
        public bool IsVDIForceFull { get; set; }
        [DataMember]
        public PRFBAInfo FBAInfo { get; set; }
        [DataMember]
        public List<PRServiceStatusInfo> ServiceStatusInfo { get; set; }
        [DataMember]
        public PRNodeTypeId ProjectDBTypeId { get; set; }
        [DataMember]
        public bool IsCluster { get; set; }
        [DataMember]
        public bool IsInRestoreListForCluster { get; set; }
        [DataMember]
        public bool IsMostRencentNode { get; set; }
    }

    [DataContract]
    public class PRServiceStatusInfo
    {
        [DataMember]
        public string InstanceName { get; set; }
        [DataMember]
        public string ServiceName { get; set; }
        [DataMember]
        public string ServiceTypeName { get; set; }
        [DataMember]
        public string ServiceStatus { get; set; }
        [DataMember]
        public PRServerInfo RunOnServer { get; set; }
    }

    [DataContract]
    public class PRServerInfo
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Address { get; set; }
    }

    [DataContract]
    public class PRIISSettingInfo
    {
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int Port { get; set; }
        [DataMember]
        public string HostHeader { get; set; }
        [DataMember]
        public string Path { get; set; }
        [DataMember]
        public string AppUrl { get; set; }
        [DataMember]
        public string Zone { get; set; }
        [DataMember]
        public bool UseSSL { get; set; }
    }

    [DataContract]
    public class PRDBFileInfo
    {
        [DataMember]
        public string LogicalName { get; set; }
        /// <summary>
        /// 路径名，不包括文件名
        /// </summary>
        [DataMember]
        public string FilePath { get; set; }
        /// <summary>
        /// 文件名，不包括路径名
        /// </summary>
        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public PRDBFileType FileType { get; set; }
        /// <summary>
        /// Byte
        /// </summary>
        [DataMember]
        public long FileSize { get; set; }
        /// <summary>
        /// \\?\Volume{6f5da45e-5f29-11e1-af81-00155dca0110}\|mount path
        /// </summary>
        [DataMember]
        public string MountPoint { get; set; }
    }

    [DataContract]
    public enum PRDBFileType
    {
        [EnumMember]
        PrimaryDataFile,		// The file is the primary database file[mdf&data only]
        [EnumMember]
        SecondaryDataFile,		// The file is a secondary database file[ndf&data only]
        [EnumMember]
        LogFile,				// The file is a database log file [log only]
        [EnumMember]
        FilestreamFile,         // The file is a filestream file
        [EnumMember]
        FullTextData,			// Fulltext data
        [EnumMember]
        Unkown       
    }

    [DataContract]
    public class PRDBBackupRecord
    {
        [DataMember]
        public int BackupSetId { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public int SoftWareMajorVersion { get; set; }
        [DataMember]
        public int SoftWareMinorVersion { get; set; }
        [DataMember]
        public int SoftwareBuildVersion { get; set; }
        /// <summary>
        /// Log sequence number of the first or oldest log record in the backup set. Can be NULL.
        /// </summary>
        [DataMember]
        public string FirstLsn { get; set; }
        /// <summary>
        /// Log sequence number of the next log record after the backup set. Can be NULL.
        /// </summary>
        [DataMember]
        public string LastLsn { get; set; }
        /// <summary>
        /// Log sequence number of the log record where redo must start. Can be NULL.
        /// </summary>
        [DataMember]
        public string CheckpointLsn { get; set; }
        /// <summary>
        /// Log sequence number of the most recent full database backup. Can be NULL. 
        /// database_backup_lsn is the “begin of checkpoint” that is triggered when the backup starts. 
        /// This LSN will coincide with first_lsn if the backup is taken when the database is idle and no replication is configured. 
        /// </summary>
        [DataMember]
        public string DatabaseBackupLsn { get; set; }
        [DataMember]
        public string BackupStartDate { get; set; }
        [DataMember]
        public string BackupFinishDate { get; set; }
        [DataMember]
        public PRDatabaseBackupType BackupType { get; set; }
        /// <summary>
        /// numeric(20,0) Size of the backup set, in bytes. Can be NULL.
        /// </summary>
        [DataMember]
        public string BackupSize { get; set; }
        [DataMember]
        public string DatabaseName { get; set; }
        [DataMember]
        public string ServerName { get; set; }
        [DataMember]
        public string MachineName { get; set; }        
        [DataMember]
        public PRSQLRecoveryModel RecoveryModel { get; set; }
        [DataMember]
        public bool IsCopyOnly { get; set; }
    }

    [DataContract]
    public enum PRDatabaseBackupType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Full = 'D',
        [EnumMember]
        Differential = 'I',
        [EnumMember]
        Log = 'L',
        [EnumMember]
        FileOrFileGroup = 'F',
        [EnumMember]
        DifferentialFile = 'G',
        [EnumMember]
        Partial = 'P',
        [EnumMember]
        DifferentialPartial = 'Q',
    }

    [DataContract]
    public class PRSQLRecoveryModel
    {
        [DataMember]
        public static string Full = "FULL";
        [DataMember]
        public static string BulkLogged = "BULK-LOGGED";
        [DataMember]
        public static string Simple = "SIMPLE";
    }

    [DataContract]
    public class PRFBAInfo
    {
        [DataMember]
        public string CAWebAppServerComment { get; set; }
        [DataMember]
        public string CADefaultRoleManager { get; set; }
        [DataMember]
        public string CADefaultMembershipProvider { get; set; }
        [DataMember]
        public string WebAppServerComment { get; set; }
        [DataMember]
        public string DefaultRoleManager { get; set; }
        [DataMember]
        public string DefaultMembershipProvider { get; set; }
        [DataMember]
        public string RoleManager { get; set; }
        [DataMember]
        public string MembershipProvider { get; set; }
        [DataMember]
        public int PreferredInstanceId { get; set; }
    }
}
