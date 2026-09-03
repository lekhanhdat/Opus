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




namespace AvePoint.GCommon.Contract.Migration.Object
{
    #region usings
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MigrationRunSetting
    {
        [DataMember]
        public long LicenseSize { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public RunJobMode RunMode { get; set; }

        [DataMember]
        public MigrationRunLevel RunLevel { get; set; }

        [DataMember]
        public bool IsRecursion { get; set; }

        [DataMember]
        public ConflictSolutionType ContentConflictSolution { get; set; }

        [DataMember]
        public ConflictSolutionType ContainerConflictSolution { get; set; }

        [DataMember]//delete or not
        public string Description { get; set; }

        [DataMember]//delete or not
        public JobReportType ReportType { get; set; }

        /// <summary>
        /// incremental 时间单位
        /// </summary>
        [DataMember]
        public MigrationTimeUnit TimeUnit { get; set; }

        /// <summary>
        /// incremental 时间间隔
        /// </summary>
        [DataMember]
        public Int32 TimeInterval { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public Boolean MigrateChange { get; set; }

        /// <summary>
        /// Job statistic
        /// </summary>
        [DataMember]
        public string PlanGroupId { get; set; }

        /// <summary>
        /// Job statistic
        /// </summary>
        [DataMember]
        public string PlanGroupName { get; set; }

        /// <summary>
        /// Job statistic
        /// </summary>
        [DataMember]
        public string UserName { get; set; }

        /// <summary>
        /// Job statistic
        /// </summary>
        [DataMember]
        public int PlanOrder { get; set; }

        /// <summary>
        /// Language
        /// </summary>
        [DataMember]
        public string Language { get; set; }
    }

    /// <summary>
    /// 该属性在别的类中有引用，不要轻易更改。
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MigrationTimeUnit
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Minute = 1,

        [EnumMember]
        Hour = 2,

        [EnumMember]
        Day = 3,

        [EnumMember]
        Week = 4,

        [EnumMember]
        Month = 5
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MigrationRunLevel : int
    {
        [EnumMember]
        Full = 0,

        [EnumMember]
        Incremental = 1,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ConflictSolutionType : int
    {
        [EnumMember]
        Skip = 0,

        [EnumMember]
        NotOverwrite = 1,

        [EnumMember]
        Append = 2,

        [EnumMember]
        Overwrite = 3,

        [EnumMember]
        Replace = 4,

        [EnumMember]
        Merge = 5,

        [EnumMember]
        OverwriteByLastModifiedTime = 6,

        /// <summary>
        /// Append a document named like "_1"
        /// </summary>
        [EnumMember]
        AppendWithSuffix = 7,

        [EnumMember]
        AppendANewVersion = 8,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum JobReportType : int
    {
        [EnumMember]
        Detailed = 0,

        [EnumMember]
        Simplified = 1
    }
}
