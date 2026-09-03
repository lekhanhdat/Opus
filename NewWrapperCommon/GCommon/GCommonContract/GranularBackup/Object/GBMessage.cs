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




namespace AvePoint.GCommon.Contract.GranularBackup.Object
{
    #region == using directives ==
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
    using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Tree.Object;
    #endregion

    /// <summary>
    /// GBMessage中只放与业务逻辑无关的Data，业务相关的请放在BackupConfig里面
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GBMessage : AveMessage
    {
        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public SPTreeNodeDto TreeNode { get; set; }

        [DataMember]
        public BackupConfig Config { get; set; }

        [DataMember]
        public GranularBackupRequest ConfigForMedia { get; set; }

        [DataMember]
        public ServiceDto MediaInfo { get; set; }
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
        public int FolderCount { get; set; }

        [DataMember]
        public int ItemCount { get; set; }

        [DataMember]
        public int ItemVersionCount { get; set; }

        [DataMember]
        public int DocumentCount { get; set; }

        [DataMember]
        public int DocumentVersionCount { get; set; }

        [DataMember]
        public int AttachmentCount { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BackupConfig
    {
        [DataMember]
        public List<ServiceDto> AlternativeMedias { get; set; }

        [DataMember]
        public bool IsTestRun { get; set; }

        [DataMember]
        public bool IncludeItemsReport { get; set; }

        [DataMember]
        public bool GenerateFullTextIndex { get; set; }

        [DataMember]
        public BackupType BackupType { get; set; }

        [DataMember]
        public CompressionType CompressionType { get; set; }

        [DataMember]
        public DataSecurity DataSecurity { get; set; }

        [DataMember]
        public bool LockSiteCollection { get; set; }

        [DataMember]
        public SiteBinConfig SiteBinConfig { get; set; }

        [DataMember]
        public BackupLevel BackupLevel { get; set; }

        [DataMember]
        public EncryptionMethods EncryptionMethods { get; set; }

        [DataMember]
        public int JobType { get; set; }

        [DataMember]
        public int JobCategory { get; set; }

        #region == Item level ==
        [DataMember]
        public BackupRestoreWorkflow WorkflowState { get; set; }

        [DataMember]
        public FilterPolicyInfo FilterPolicy { get; set; }

        [DataMember]
        public bool IncludeUserProfile { get; set; }

        [DataMember]
        public bool IncludeVersions { get; set; }
        [DataMember]
        public int FBVersionCount { get; set; }
        [DataMember]
        public int IBVersionCount { get; set; }

        [DataMember]
        public bool EnableMultiThreadInVersionLevel { get; set; }

        [DataMember]
        public bool IsNotIncludeTermStore { get; set; }

        [DataMember]
        public BackupMMSSetting BackupMMSSetting { get; set; }      

        [DataMember]
        public bool UseBackupMMSSettingProperty { get; set; }

        [DataMember]
        public bool SkipSystemUpdate { get; set; }

        [DataMember]
        public bool IncludeProjectsData { get; set; }

        [DataMember]
        public bool IncludeListView { get; set; }

        [DataMember]
        public bool DisableInformationRightsManagement { get; set; }

        [DataMember]
        public bool EnableSuperUserDecryptsFiles { get; set; }
        #endregion ==

        [DataMember]
        public bool SetFailAsSkip { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BackupType
    {
        [EnumMember]
        Full = 0,
        [EnumMember]
        Incremental,
        [EnumMember]
        Differential
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BackupLevel
    {
        [EnumMember]
        Undefine = -1,
        [EnumMember]
        Item = 0,
        [EnumMember]
        Site = 1,
        [EnumMember]
        [Description("Site Collection")]
        SiteCollection = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CompressionType
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

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BackupSODataType
    {
        [EnumMember]
        Skip,
        [EnumMember]
        BackupStubsAndContent,
        [EnumMember]
        OnlyBackupStubs
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BackupMMSSetting
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        TermsOnly = 1,
        [EnumMember]
        TermSets = 2,
        [EnumMember]
        ManagedMetadataService = 3,
    }
}
