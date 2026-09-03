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





using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ExportAndImport;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    /// <summary>
    /// SO模块统一使用的job contract
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SOJob : BaseJobDto
    {
        /// <summary>
        /// 主job对应的子job
        /// </summary>
        [DataMember]
        public IList<SubJobDto> SubJobs { get; set; }

        /// <summary>
        /// 保留ScanFile文件是否生成的状态，默认0为生成，1为未生成
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_1)]
        public int ScanFileIsExist { get; set; }

        /// <summary>
        /// 用于标识Job是否要被stop，默认0为正常跑，1为此job要被stop
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_3)]
        public int JobIsStopping { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string ScheduleId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string FarmName { get; set; }

        /// <summary>
        /// Full Text Index Job的时候,该字段存放的是archiver backup的job id
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string BackupJobId { get; set; }

        /// <summary>
        /// 如果发现当前Job中的Site Collection有其它Job正在运行，将这些SIte Collection记录下来，等待重新运行时做处理。
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.CLOB_1)]
        public string SiteCollectionList { get; set; }

        /// <summary>
        /// 存放scheduled 和 archiver run job节点的scope即fullpath，由于fullpath可能比较大，所以映射String_5字段，有255大小
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string Scope { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string ProcessPaused { get; set; }

        /// <summary>
        /// Full Text Index Job用:保存处理该Job的Profile Id.
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_7)]
        public string ProfileId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_8)]
        public string SrcIndexDeviceId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_9)]
        public string DestIndexDeviceId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_8)]
        public RestoreType RestoreType { get; set; }

        #region  For Extender Upgrade
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_4)]
        public EIDataType DataType { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_5)]
        public EIOperateType OperateType { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_6)]
        public ImportDataVersion DataVersion { get; set; }
        #endregion

        /// <summary>
        /// 在进行archiver restore的时候存放RestoreMode,用来区分full text index export和正常的restore.
        /// </summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_7)]
        public RestoreMode RestoreMode { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_9)]
        public RelatedJobRunningState RunningState { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScanFileExist
    {
        [EnumMember]
        Exist = 0,
        [EnumMember]
        Not_Exist = 1
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RelatedJobRunningState
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        CurrentJobState = 1,
        [EnumMember]
        RelatedJobRunning = 2,
        [EnumMember]
        RelatedJobFinished = 3,
    }
}
