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




using System.ComponentModel;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRRestoreJobDto : BaseJobDto
    {
        //扩展字段需要加上[ColumnMapAttribute (DBColumn =  )]
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string BackupJobId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string BackupCycleId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string BackupPlanId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string SrcFarm { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string DestFarm { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string RestoreOption { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_7)]
        public string AgentVersion { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_8)]
        public string MediaName { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_9)]
        public string RestoreOptionOverwriteType { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.LONG_1)]
        public long BackupTime { get; set; }

        [DataMember]
        public bool OutOfPlace { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_3)]
        public int PlatformType { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_4)]
        public AlternateCheckedType AlternateType { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.CLOB_1)]
        public string PRRestoreJobExtentionStr { get; set; }

        [DataMember]
        public PRRestoreJobExtentionDto PRRestoreJobExtentionDto { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AlternateCheckedType
    {
        [EnumMember]
        [Description("N/A")]
        None = 0,

        [EnumMember]
        [Description("No")]
        AlternateUnChecked = 1,

        [Description("Yes")]
        [EnumMember]
        AlternateChecked = 2
    }

    /// <summary>存放在job表clob1列</summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRRestoreJobExtentionDto
    {
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string SrcAgentId { get; set; }

        [DataMember]
        public string DesAgentId { get; set; }

        [DataMember]
        public string MediaId { get; set; }
    }
}
