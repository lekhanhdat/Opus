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




namespace AvePoint.GCommon.Contract.Server.GranularRestore.Object
{
    #region == using directives ==
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.GranularBackup.Object;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.GranularBackup.Object;
    #endregion ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BackupDataRecordDto
    {
        [DataMember]
        public string PlanId { get; set; }

        [DataMember]
        public string PlanName { get; set; }

        /// <summary>delete plan time. </summary>
        [DataMember]
        public long DeletePlanTime { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public long BackupTime { get; set; }

        [DataMember]
        public GranularBackupPlanType PlanType { get;set; }

        /// <summary> 标识job是FB，IB，DB,用GranularBackup.BACKUP_JOB_DTO_TYPE_FB等匹配。 </summary>
        [DataMember]
        public int JobType { get; set; }

        [DataMember]
        public BackupLevel BackupLevel { get; set; }

        [DataMember]
        public int JobState { get; set; }

        /// <summary>Backup workflow state. </summary>
        [DataMember]
        public BackupRestoreWorkflow WorkflowState { get; set; }

        /// <summary> Backup user profile setting. </summary>
        [DataMember]
        public bool IncludeUserProfile { get; set; }

        [DataMember]
        public bool IncludeListView { get; set; }

        [DataMember]
        public bool IsCustomActed { get; set; }

        [DataMember]
        public bool IsImportedData { get; set; }

        [DataMember]
        public bool IsIncludeVersion { get; set; }

        [DataMember]
        public ProductVersion DataVersion { get; set; }

        [DataMember]
        public int SPVersion { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BackupDataCollectionDto
    {
        [DataMember]
        public List<BackupDataRecordDto> Jobs { get; set; }

        /// <summary>
        /// Key: farmName, Value:plan info和 farmName的关联关系.
        /// </summary>
        [DataMember]
        public Dictionary<string, List<SimpleDataDto>> PlanFilters { get; set; }

        [DataMember]
        public bool IsBakcupDataArchiveTier { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum TimeStampType : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Day = 1,
        [EnumMember]
        Week = 2,
        [EnumMember]
        Month = 3
    } 
}
