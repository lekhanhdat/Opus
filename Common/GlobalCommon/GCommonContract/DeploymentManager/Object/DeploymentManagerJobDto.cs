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

namespace AvePoint.GCommon.Contract.DeploymentManager.Object
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.Common;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup.Object;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DeploymentManagerJobDto : BaseJobDto
    {
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.INT_6)]
        public int MainJobFinalState { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string DPMRunJobId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_7)]
        public string StoragePolicyId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_9)]
        public string BackUpJobId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_10)]
        public string DPMRunPlanId { get; set; }

        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.CLOB_1)]
        public string JobExtention { get; set; }

        [DataMember]
        public JobLicenseStatus LicenseStatus { get; set; }

        //#region for DPM Data Upgrade
        ///// <summary>
        ///// 存放邮件id
        ///// </summary>
        //[DataMember]
        //[ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        //public string NotificationProfileID { get; set; }

        //[DataMember]
        //[ColumnMapAttribute(DBColumn = ContractConstants.INT_4)]
        //public EIDataType DataType { get; set; }

        //[DataMember]
        //[ColumnMapAttribute(DBColumn = ContractConstants.INT_5)]
        //public EIOperateType OperateType { get; set; }

        //[DataMember]
        //[ColumnMapAttribute(DBColumn = ContractConstants.INT_6)]
        //public ImportDataVersion DataVersion { get; set; }
        //#endregion
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DeploymentManagerJobExtention
    {
        [DataMember]
        public DeploymentManagerMainJobSetting JobSetting { get; set; }

        [DataMember]
        public Dictionary<string, JobState> BackupJobState { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DeploymentManagerMainJobSetting
    {
        [DataMember]
        public string PlanName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public bool IsBackup { get; set; }

        [DataMember]
        public string StoragePolicyId { get; set; }

        [DataMember]
        public string EmailId { get; set; }

        [DataMember]
        public string PlanGroups { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DeploymentManagerRollbackResult
    {
        [DataMember]
        public string FailedJobId { get; set; }

        [DataMember]
        public List<string> FailedBackupJobIds { get; set; }
    }
}