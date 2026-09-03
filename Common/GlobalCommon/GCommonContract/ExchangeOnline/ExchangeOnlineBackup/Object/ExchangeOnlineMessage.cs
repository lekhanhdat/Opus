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




namespace AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object
{
    #region == using directives ==
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
    using AvePoint.GCommon.Contract.Tree.Object;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    #endregion

    /// <summary>
    /// GBMessage中只放与业务逻辑无关的Data，业务相关的请放在BackupConfig里面
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExchangeOnlineMessage : AveMessage
    {
        //[DataMember]
        //public string PlanId { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public ExchangeOnlineTreeNodeDto TreeNode { get; set; }

        [DataMember]
        public ExchangeOnlineBackupConfig Config { get; set; }

        [DataMember]
        public ExchangeBackupRequest ConfigForMedia { get; set; }
    }



    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExchangeOnlineBackupConfig
    {
        [DataMember]
        public EOBackupType BackupType { get; set; }

        [DataMember]
        public bool IsTestRun { get; set; }

        [DataMember]
        public bool GenerateFullTextIndex { get; set; }

        [DataMember]
        public EOCompressionType CompressionType { get; set; }

        //[DataMember]
        //public EODataSecurity DataSecurity { get; set; }

        [DataMember]
        public EOBackupLevel BackupLevel { get; set; }

        [DataMember]
        public EOEncryptionMethods EncryptionMethods { get; set; }

        [DataMember]
        public int JobType { get; set; }

        [DataMember]
        public int JobCategory { get; set; }

        //[DataMember]
        //public ExchangeOnlineBackupFilterDto FilterPolicy { get; set; }

        [DataMember]
        public String PreviousFBJobId { get; set; }

        //[DataMember]
        //public bool ReportOnlyHighLevel { get; set; }

        [DataMember]
        public bool BackupPrivateChannel { get; set; }

        [DataMember]
        public bool BackupSharedChannel { get; set; }

        [DataMember]
        public bool BackupPlanner { get; set; }

        [DataMember]
        public Boolean IsO365Group { get; set; }

        [DataMember]
        public bool IsMicrosoftTeam { get; set; }

        [DataMember]
        public List<string> SkippedErrorCodeList { get; set; }

        [DataMember]
        public List<string> MovetoConnectionStrings { get; set; }

        /// <summary>
        /// Skip failed item at the first time
        /// </summary>
        [DataMember]
        public bool SetFailAsSkip { get; set; }

        [DataMember]
        public bool IncludeFolderPermission { get; set; }

        //[DataMember]
        //public bool JobStatusOption { get; set; }

        [DataMember]
        public bool SupportProtectPrimaryandRecoverableMailBox { get; set; }
        [DataMember]
        public bool BackupAsColdTier { get; set; }
  
        [DataMember]
        public bool IsBackupRecoverableItemsVersionsFolder { get; set; }
        [DataMember]
        public bool UserArchiverImportFile { get; set; }
        [DataMember]
        public bool SupportLockedSite { get; set; }
        [DataMember]
        public bool SupportArchivedTeams { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EOBackupType
    {
        [EnumMember]
        Full = 0,
        [EnumMember]
        Incremental,
        [EnumMember]
        Differential,

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EOBackupLevel
    {
        [EnumMember]
        Undefine = -1,

        [EnumMember]
        MailBox = 0,

        [EnumMember]
        Folder = 1,

        [EnumMember]
        Item = 2,

        [EnumMember]
        Team = 3,

        [EnumMember]
        Channel = 4,

        [EnumMember]
        Conversation = 5,

        [EnumMember]
        Meeting = 6,

        [EnumMember]
        GroupConversation = 7,

        [EnumMember]
        PlannerPlan = 8,

        [EnumMember]
        PlannerTask = 9,

        [EnumMember]
        YammerGroup = 10,

        [EnumMember]
        YammerConversation = 11,

        [EnumMember]
        User = 12,
        [EnumMember]
        Chat = 13,
        [EnumMember]
        ChatMessage = 14,

        [EnumMember]
        Workspace = 15,
        [EnumMember]
        Report = 16,

        [EnumMember]
        Flow = 17,

        [EnumMember]
        PowerApps = 18,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EOCompressionType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Fastest = 1,
        [EnumMember]
        Level2 = 2,
        [EnumMember]
        Fast = 3,
        [EnumMember]
        Level4 = 4,
        [EnumMember]
        Normal = 5,
        [EnumMember]
        Level6 = 6,
        [EnumMember]
        Good = 7,
        [EnumMember]
        Level8 = 8,
        [EnumMember]
        Best = 9

    }


    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum EODataSecurity
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
    public enum EOEncryptionMethods
    {
        [EnumMember]
        BLOWFISH_ENCRYPTION = 0,
        [EnumMember]
        AES_ENCRYPTION = 1
    }

}
