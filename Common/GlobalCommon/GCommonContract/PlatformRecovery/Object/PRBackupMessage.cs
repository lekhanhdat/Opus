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
using System.ComponentModel;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRBackupMessage : PRMessage
    {
        /// <summary>
        /// plan ID
        /// </summary>
        [DataMember]
        public string PlanId { get; set; }

        /// <summary>
        /// SPTree 对象
        /// </summary>
        [DataMember]
        public SPTreeNodeDto TreeNode { get; set; }

        /// <summary>
        ///  PRTree 对象
        /// </summary>
        [DataMember]
        public PRTreeNodeDto PRTreeNode { get; set; }

        /// <summary>
        /// media备份时使用
        /// </summary>
        [DataMember]
        public PlatformBackupRequest ConfigForMedia { get; set; }

        /// <summary>
        /// media信息,agent使用
        /// </summary>
        [DataMember]
        public ServiceDto MediaInfo { get; set; }

        /// <summary>
        /// schedule 对象
        /// </summary>
        [DataMember]
        public ScheduleDto Schedule { get; set; }

        /// <summary>
        /// 全部PR模块agent
        /// </summary>
        [DataMember]
        public IList<ServiceDto> AgentList { get; set; }

        /// <summary>
        /// 备份plan对象
        /// </summary>
        [DataMember]
        public PRBackupPlanDto Plan { get; set; }

        /// <summary>
        /// 备份job对象,其中planLeve存放备份的level
        /// (siteCollection:1002/site:1003/folder:1004/item:1005/itemversion:1006)
        /// </summary>
        [DataMember]
        public PRBackupJobDto Job { get; set; }

        /// <summary>
        /// 获得当前job下cycle的storageInfo信息
        /// </summary>
        [DataMember]
        public Dictionary<string, string> StorageInfoMap { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GBStatics
    {
        [DataMember]
        public int SiteCollectionCount { get; set; }

        [DataMember]
        public int WebCount { get; set; }

        [DataMember]
        public int ListCount { get; set; }

        [DataMember]
        public int ItemCount { get; set; }

        [DataMember]
        public int ItemVersionCount { get; set; }

        [DataMember]
        public long TotalSize { get; set; }//Bytes
    }
    /// <summary> check schedule time for GUI </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum InvalidScheduleTime : byte
    {
        [EnumMember]
        InvalidStartTime = 0,

        [EnumMember]
        StartTimeEarlierThanNow = 1,

        [EnumMember]
        InvalidEndTime = 2,

        [EnumMember]
        StartTimeNotEarlierThanEndTime = 3,
    }


    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class BackupConfig
    //{
    //    [DataMember]
    //    public bool IsTestRun { get; set; }

    //    [DataMember]
    //    public bool IncludeItemsReport { get; set; }

    //    //[DataMember]
    //    //public bool GenerateFullTextIndex { get; set; }

    //    [DataMember]
    //    public PRBackupType BackupType { get; set; }

    //    [DataMember]
    //    public CompressionType CompressionType { get; set; }

    //    [DataMember]
    //    public DataSecurity DataSecurity { get; set; }

    //    //[DataMember]
    //    //public BackupAdvanceOption Option { get; set; }

    //    [DataMember]
    //    public SiteBinConfig SiteBinConfig { get; set; }

    //    [DataMember]
    //    public PRBackupLevel BackupLevel { get; set; }

    //    [DataMember]
    //    public EncryptionMethods EncryptionMethods { get; set; }

    //    [DataMember]
    //    public int JobType { get; set; }

    //    //FilterPolicy
    //}

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PRBackupType
    {
        [EnumMember]
        Full = 0,
        [EnumMember]
        Incremental,
        [EnumMember]
        Differential
    }


    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum PRBackupLevel
    {
        [EnumMember]
        Database = 0,//不使用，由于media有使用，故暂不删除

        [EnumMember]
        [Description("Full Backup")]
        FullBackup = 1,

        [EnumMember]
        [Description("Incremental Backup")]
        IncrementalBackup = 2,

        [EnumMember]
        [Description("Differential Backup")]
        DifferentialBackup = 4,


        [EnumMember]
        [Description("None")]
        None = 8,//None为Database类型，由于0不能做Flag故改为8

        [EnumMember]
        [Description("Site Collection")]
        SiteCollection = 16,

        [EnumMember]
        Site = 32,

        [EnumMember]
        Folder = 64,

        [EnumMember]
        Item = 128,

        [EnumMember]
        [Description("Item Version")]
        ItemVersion = 256
    }

    //[DataContract(Namespace = ContractConstants.Namespace)]
    public enum CompressionType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Fastest = 1,
        [EnumMember]
        Fast = 2,
        [EnumMember]
        Normal = 3,
        [EnumMember]
        Good = 4,
        [EnumMember]
        Best = 5,
        [EnumMember]
        FastestOne = 6,
        [EnumMember]
        FastOne = 7,
        [EnumMember]
        NormalOne = 8,
        [EnumMember]
        GoodOne = 9,
        [EnumMember]
        BestOne = 10,
    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum BackupAdvanceOption
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        IncludeStub = 1,
        [EnumMember]
        IncludeWorkflow = 2,
        [EnumMember]
        IncludeOrphanMySite = 4
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SiteBinConfig
    {
        [DataMember]
        public bool NeedDelete { get; set; }

        [DataMember]
        public bool mDeleteSite { get; set; }
    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum DataSecurity
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        CompressionMedia = 4,
        [EnumMember]
        CompressionAgent = 16,
        [EnumMember]
        EncryptionMedia = 8,
        [EnumMember]
        EncryptionAgent = 32
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EncryptionMethods
    {
        [EnumMember]
        BLOWFISH_ENCRYPTION = 0,
        [EnumMember]
        AES_ENCRYPTION = 1
    }
}

