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




namespace AvePoint.GCommon.Contract.Server.GranularBackup.Object
{
    #region == using directives ==
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.AccountManager.Object;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.GranularBackup.Object;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;

    #endregion ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GranularBackupPlanDto : PlanDto
    {
        [DataMember]
        public StoragePolicyDto StoragePolicy { get; set; }

        [DataMember]
        public CompressionType CompressionType { get; set; }

        [DataMember]
        public DataSecurity DataSecurity { get; set; }

        [DataMember]
        public bool LockSiteCollection { get; set; }

        [DataMember]
        public bool FullTextIndex { get; set; }

        [DataMember]
        public BackupRestoreWorkflow WorkflowState { get; set; }

        /// <summary>
        /// 添加该属性是为了更方便处理PlanDto中的planType，因为planType属性是int值，不直观，可读性较差，所以新增加该属性
        /// 来替代planType，使用GranularBackupPlanDto时不用关心planType，我们会在Domain，Dto转化时自动将该属性和planType
        /// 进行映射。
        /// </summary>
        [DataMember]
        public GranularBackupPlanType Type { get; set; }

        /// <summary> Represent user choose a filter policy</summary>
        [DataMember]
        public string FilterPolicyId { get; set; }

        [DataMember]
        public bool IncludeUserProfile { get; set; }

        [DataMember]
        public bool IncludeVersions { get; set; }

        [DataMember]
        public int FBVersionCount { get; set; }

        [DataMember]
        public int IBVersionCount { get; set; }

        [DataMember]
        public bool IncludeListView { get; set; }

        [DataMember]
        public bool EnableMultiThreadInVersionLevel { get; set; }

        [DataMember]
        public bool EnableSuperUserDecryptsFiles { get; set; }

        [DataMember]
        public bool IsNotIncludeTermStore { get; set; }

        [DataMember]
        public BackupMMSSetting BackupMMSSetting { get; set; }

        [DataMember]
        public bool UseBackupMMSSettingProperty { get; set; }

        [DataMember]
        public bool SkipSystemUpdate { get; set; }

        [DataMember]
        public bool IsCloseIRMSetting { get; set; }

        [DataMember]
        public string NotificationProfileId { get; set; }

        [DataMember]
        public string SecurityProfileGuid { get; set; }

        [DataMember]
        public List<string> AssocitedPlanGroups { get; set; }

        /// <summary>
        /// reload plan的时候标识plan.SrcAgentGroup下有没有正常的agent，以便在GUI上控制tree的显示。
        /// </summary>
        [DataMember]
        public bool HasAvailableAgent { get; set; }

        [DataMember]
        public BackupSettingsForMisc SettingsForMisc { get; set; }

        /// <summary>
        /// UpdatePlan时，是否需要check planName,sitecollection
        /// </summary>
        [DataMember]
        public bool NeedCheckPlan { get; set; }

        /// <summary>
        /// EditPlan时,传递新添加的sitecollectionids，check是否需要share
        /// </summary>
        [DataMember]
        public List<string> AddedSiteCollectionIds { get; set; }

        #region == plan settings ==
        [DataMember]
        public RunJobMode IsTestRun { get; set; }

        [DataMember]
        public bool IncludeItemsReport { get; set; }

        [DataMember]
        public long JobStartTime { get; set; }

        [DataMember]
        public string CycleId { get; set; }
        
        [DataMember]
        public bool IsAutoStopIBJob { get; set; }

        [DataMember]
        public bool SetFailAsSkip { get; set; }
        #endregion == plan settings ==
    }

    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum GranularBackupPlanType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ItemLevel = 1,
        [EnumMember]
        SiteLevel = 2,
        [EnumMember]
        SiteCollectionLevel = 4,
        [EnumMember]
        QuickBackupPlan = 8,
        [EnumMember]
        BackupPlanBuilder = 16,
        [EnumMember]
        ContentManager = 32,
        [EnumMember]
        Replicator = 64,
        [EnumMember]
        DeploymentMananger = 128,
        [EnumMember]
        MigrationExport = 256,
        [EnumMember]
        DataTransfer = 512,
    }

    /// <summary> 做CEIP使用,不需存储DB里. </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BackupSettingsForMisc
    {
        [DataMember]
        public BackupPlanMode PlanMode { get; set; }

        [DataMember]
        public BackupScheduleMode ScheduleMode { get; set; }
    }

    /// <summary> GranularBackupPlanType的BackupPlanBuilder区分不出是wizard mode or Form Mode,
    /// 加此枚举为方便做CEIP使用. </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BackupPlanMode : int
    {
        [EnumMember]
        None = -1,
        [EnumMember]
        AdHoc = 0,
        [EnumMember]
        Wizard = 1,
        [EnumMember]
        Form = 2,
        [EnumMember]
        ContentManager = 3,
        [EnumMember]
        Replicator = 4,
        [EnumMember]
        DeploymentManager = 5
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BackupScheduleMode : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Predefined = 1,
        [EnumMember]
        Configure = 2
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BackupPlanOperationResult
    {
        /// <summary>
        /// The operation failure 'planId' collection.或者 UpdatePlan成功时，返回PlanId
        /// </summary>
        [DataMember]
        public List<string> PlanIds { get; set; }

        /// <summary>
        /// 收集Audit Object列显示信息.
        /// </summary>
        [DataMember]
        public List<string> AuditObjects { get; set; }

        /// <summary>
        /// failure simple message.
        /// </summary>
        [DataMember]
        public string ErrorsMessage { get; set; }

        [DataMember]
        public PlanUpdateResult PlanUpdateResult { get; set; }

        [DataMember]
        public EditPlanCheckResult CheckSiteCollectionResult { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PlanUpdateResult
    {
        [EnumMember]
        Failed,
        [EnumMember]
        Passed,
        [EnumMember]
        PlanNameExist,
        [EnumMember]
        NeedShareSiteCollection,
        [EnumMember]
        ScheduleStartTimeError,
        [EnumMember]
        ScheduleDaylightSaveTimeError,
        [EnumMember]
        ExistSiteGroupLevelNodeInSharedPlan,
    }

}
