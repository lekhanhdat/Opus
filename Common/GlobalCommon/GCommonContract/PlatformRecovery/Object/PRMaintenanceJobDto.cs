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




using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRMaintenanceJobDto : BaseJobDto
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_1)]
        public int CopyDataJobCount { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_3)]
        public int IndexJobCount { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_10)]
        public int VdbMappingJobCount { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string ManagerVersion { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_4)]
        public int VerifyJobCount { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_5)]
        public int PlatformTypeForMaintenance { get; set; }

        //[DataMember]
        //[ColumnMapAttribute(DBColumn = ContractConstants.CLOB_2)]
        //public string NotificationXml { get; set; }

        //[DataMember]
        //[ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        //public string MediaId { get; set; }
        //[DataMember]
        //public PRBackupJobExtentionDto PRBackupJobExtentionDto { get; set; }
        //[DataMember]
        //[ColumnMapAttribute(DBColumn = ContractConstants.CLOB_2)]
        //public string PRBackupJobExtentionDtoStr { get; set; }
        //[DataMember]
        //public string AgentName { get; set; }
        //[DataMember]
        //[ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        //public string AgentVersion { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string AgentName { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string AgentVersion { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string FarmStr { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string MediaId { get; set; }
        /// <summary>标识generating VDBmapping的状态(Maintenance和JobMonitor使用)</summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_7)]
        public JobExtendStatus MappingStatus { get; set; }
        /// <summary>标识数据从snapshot复制到media是否成功的状态(Maintenance和JobMonitor使用)</summary>
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_8)]
        public JobExtendStatus CopyStatus { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_9)]
        public JobExtendStatus IndexStatusStr { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_6)]
        public int VerifyStatus { get; set; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.CLOB_2)]
        public string PRBackupJobExtentionDtoStr { get; set; }
        [DataMember]
        public PRBackupJobExtentionDto PRBackupJobExtentionDto { get; set; }
        [DataMember]
        public string AgentId { get; set; }

        public PRMaintenanceJobDto()
        {

        }
    }

}
