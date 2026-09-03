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
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup.Object;
    using AvePoint.GCommon.Contract.SPMigration.Object;
    #endregion ==

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class GranularBackupJobDto : BaseJobDto
    {
        /// <summary> 其它模块(Content Manager or Replicator)调用Item backup功能的Job Id。</summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string RelatedJobId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_1)]
        public int BackupLevel { get; set; }

        /// <summary> 划分子job数据在不同media进程数据备份。 </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_3)]
        public int Order { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_4)]
        public int Weight { get; set; }

        [DataMember]
        public IList<SubJobDto> SubJobs { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_5)]
        public bool IncludeItemsJobReport { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_6)]
        public JobBackupGroundStatus JobBackupGroundStatus { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.CLOB_1)]
        public string RunJobSettings { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_7)]
        public string JobQueueName { get; set; }
    }

    /// <summary> Run job extension setting. </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BackupJobSettings
    {
        [DataMember]
        public SPExportSetting SPExportSetting { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ItemBackupJobParams
    {
        [DataMember]
        public GranularBackupPlanDto PlanInfo { get; set; }

        [DataMember]
        public ScheduleDto Schedule { get; set; }

        [DataMember]
        public long SkipTimeUTC { get; set; }

        [DataMember]
        public PlanCategory Category { get; set; }

        /// <summary> Run plan group mode </summary>
        [DataMember]
        public PlanGroupParaDto PlanGroupParamInfo { get; set; }

        [DataMember]
        public JobBackupGroundStatus JobBackupGroundStatus { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum JobBackupGroundStatus
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        skipping = 1
    }
}
