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




namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    #region using directives
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    #endregion

    /// <summary>
    /// 使用例如STRING_1或者INT_3注解时 注意一定不要和别的重复
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RCCollectorJobDto : BaseJobDto
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string RCCollectorJobType { get; set; }

        /// <summary>
        /// 用来保存job过程中用到的ServiceId，用;分割
        /// 
        /// 例如"06ae7623-886c-409d-8893-f845ac8a442f;5a7d7632-5ec8-4f05-8551-6444509ced7d"
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.CLOB_1)]
        public string JobServiceIds { set; get; }

        /// <summary>
        /// 与枚举ReportType对应
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_2)]
        public int ReportType { get; set; }

        #region for audit pruning
        /// <summary>
        /// 选择MoveData时该值对应数据文件fullpath
        /// </summary>
        //[DataMember]
        //[ColumnMapAttribute(DBColumn = ContractConstants.STRING_7)]
        //public string DataFile { get; set; }
        /// <summary>
        /// 选择move data并且没有执行Restore之前该值为true
        /// 之后Restore之后原来数据就被删除了，该值应设为false
        /// 选择deeleteData时该值应该设为false
        /// 
        /// 1 true
        /// 0 false
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_3)]
        public int CanRestoreData { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_4)]
        public int ProcessedCount { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_5)]
        public AuditPruningJobType AuditPruningJobType { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_6)]
        public BoolEnum IsCompressed { get; set; }

        /// <summary>
        /// restore data时是按时间升序从db file中取数据还原到audit data表，
        /// 每次还原一批则记录LastRestoreDataTime为数据的最新时间以便在job fail时下次重新从LastRestoreDataTime开始取数据
        /// 默认值为0
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.LONG_1)]
        public long LastRestoreDataTime { get; set; }
        #endregion

        #region for audit controller

        /// <summary>
        /// in rc retrieve data     for analysis data
        /// in rc retrieve iislog   for analysis iislog
        /// in audit controller     for matchip
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string AnalysisJobId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_7)]
        public BoolEnum MatchIp { get; set; }

        /// <summary>
        /// rc的auditor该值为true，audit该值为false
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_8)]
        public BoolEnum AnalysisData { get; set; }
        #endregion

        #region for audit report new
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_9)]
        public AuditReportChartType ReportDataType { get; set; }
        /// <summary>
        /// 0 success
        /// 1 too much data
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_10)]
        public int AuditReportResultType { get; set; }
        /// <summary>
        /// ProfileId是创建JobId的profile的副本的id
        /// PlanId是创建JobId的profile的id，在profile被删除或更新是该列被置成null
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_8)]
        public string ProfileId { get; set; }

        /// <summary>
        /// 传输UserId，不保存到数据库
        /// </summary>
        [DataMember]
        public string UserId { set; get; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string ExportLocationName { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_7)]
        public string CustomDatabaseId { get; set; }

        #endregion
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AuditorFunctionType
    {
        [EnumMember]
        Apply = 0,
        [EnumMember]
        Retrieve = 1,
        [EnumMember]
        RunReport = 2,
        [EnumMember]
        ExportReport = 3,
        [EnumMember]
        Pruning = 4,
        [EnumMember]
        Restore = 5,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum BoolEnum
    {
        [EnumMember]
        True = 1,
        [EnumMember]
        False = 0,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdminReportJobDto : BaseJobDto
    {
    }
}