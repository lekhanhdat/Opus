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




using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;
using AvePoint.GCommon.Contract.DeploymentManager.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
using AvePoint.GCommon.Contract.Migration.Object;
using AvePoint.GCommon.Contract.PlatformRecovery.Object;
using AvePoint.GCommon.Contract.ReportCenter.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.ExportReport.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.DataManager.DataTransfer;
using AvePoint.GCommon.Contract.Server.ControlPanel.DataManager.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Defragmenter.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.LogManager.Object;
using AvePoint.GCommon.Contract.Server.ExportAndImport;
using AvePoint.GCommon.Contract.Server.GranularBackup.Object;
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.GCommon.Contract.Server.Retention;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Vault.Object;
using AvePoint.GCommon.Contract.CloudAppAdmin.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType(typeof(BackupJobDto))]
    [KnownType(typeof(GranularBackupJobDto))]
    [KnownType(typeof(GranularRestoreJobDto))]
    [KnownType(typeof(CAJobDto))]
    [KnownType(typeof(ContentManagerJobDto))]
    [KnownType(typeof(PRBackupJobDto))]
    [KnownType(typeof(PRRestoreJobDto))]
    [KnownType(typeof(PRMaintenanceJobDto))]
    [KnownType(typeof(ReplicatorJobDto))]
    [KnownType(typeof(SOJob))]
    [KnownType(typeof(RCCollectorJobDto))]
    [KnownType(typeof(DeploymentManagerJobDto))]
    [KnownType(typeof(LogManagerJobDto))]
    [KnownType(typeof(JobPruningJobDto))]
    [KnownType(typeof(AdminReportJobDto))]
    [KnownType(typeof(RetentionJobDto))]
    [KnownType(typeof(BackupDataEIJobDto))]
    [KnownType(typeof(EDSyncJobDto))]
    [KnownType(typeof(EDJobDto))]
    [KnownType(typeof(VaultJobDto))]
    [KnownType(typeof(ExchangeOnlineBackupJobDto))]
    [KnownType(typeof(ExchangeOnlineRestoreJobDto))]
    [KnownType(typeof(ExchangeOnlineLocateJobDto))]
    [KnownType(typeof(DataTransferJobDto))]
    [KnownType(typeof(ExportReportJobDto))]
    [KnownType(typeof(DeleteGroupJobDto))]
    [KnownType(typeof(DefragmenterJobDto))]
    [KnownType(typeof(CAAJobDto))]

    public class BaseJobDto : INotifyPropertyChanged, IJob
    {
        [DataMember]
        [XmlIgnore]
        public IJob Parent { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public int Type { get; set; }
        [DataMember]
        public string TimeZoneId { get; set; }
        [DataMember]
        public long StartTimeWithTimeZone { get; set; }
        [DataMember]
        public long FinishTimeWithTimeZone { get; set; }
        [DataMember]
        public long StartTime { get; set; }
        [DataMember]
        public string StartTimeStr { get; set; }
        [DataMember]
        public ObjectInfoDto ObjectInfo { get; set; }

        private long _FinishTime;

        private string _FinishTimeStr;

        private double _Progress;

        private int _State;

        private string _StateStr;

        private string _Detail;

        [DataMember]
        public long FinishTime
        {
            get
            {
                return this._FinishTime;
            }
            set
            {

                if (value != this._FinishTime)
                {

                    this._FinishTime = value;
                    NotifyPropertyChanged("FinishTime");
                }
            }
        }

        [DataMember]
        public string FinishTimeStr
        {
            get
            {
                return this._FinishTimeStr;
            }
            set
            {
                if (value != this._FinishTimeStr)
                {
                    this._FinishTimeStr = value;
                    NotifyPropertyChanged("FinishTimeStr");
                }
            }
        }

        [DataMember]
        public double Progress
        {
            get
            {
                return this._Progress;
            }
            set
            {
                this._Progress = value;
                NotifyPropertyChanged("Progress");
            }
        }

        [DataMember]
        public int State
        {
            get
            {
                return this._State;
            }
            set
            {
                if (value != this._State)
                {
                    this._State = value;
                    NotifyPropertyChanged("State");
                }
            }
        }

        [DataMember]
        public string StateStr
        {
            get
            {
                return this._StateStr;
            }
            set
            {
                if (value != this._StateStr)
                {
                    this._StateStr = value;
                    NotifyPropertyChanged("StateStr");
                }
            }
        }

        [DataMember]
        public string Detail
        {
            get
            {
                return this._Detail;
            }
            set
            {
                if (value != this._Detail)
                {
                    this._Detail = value;
                    NotifyPropertyChanged("Detail");
                }
            }
        }

        /// <summary>
        /// not used in DocAve Manager
        /// </summary>
        [DataMember]
        public int ControlState { get; set; }
        [DataMember]
        public int PlanType { get; set; }
        [DataMember]
        public int Category { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string PlanName { get; set; }
        [DataMember]
        public string SrcAgentName { get; set; }
        [DataMember]
        public string DestAgentName { get; set; }
        [DataMember]
        public string PlanId { get; set; }
        [DataMember]
        public int IndexStatus { get; set; }
        [DataMember]
        public long Stamp { get; set; }
        [DataMember]
        public long SubJobStateStamp { get; set; }
        [DataMember]
        public string Dependency { get; set; }

        [DataMember]
        public string Performance { get; set; }

        [DataMember]
        public RunJobMode IsTestRun { get; set; }

        [DataMember]
        public int CountOfSubJob { get; set; }

        [DataMember]
        public string ReportLocation { get; set; }

        private BackupPoint _BackupPoint = BackupPoint.BackupSourceAndDest;

        /// <summary> 这个属性是为了标识非backup模块备份的是源端还是目的端的数据 </summary>
        [DataMember]
        public BackupPoint BackupPoint
        {
            get
            {
                return this._BackupPoint;
            }
            set
            {
                if (value != this._BackupPoint)
                {
                    this._BackupPoint = value;
                    NotifyPropertyChanged("BackupPoint");
                }
            }
        }

        [DataMember]
        public JobContextDto JobContext { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {

            if (PropertyChanged != null)
            {

                PropertyChanged(this, new PropertyChangedEventArgs(info));

            }

        }
        [DataMember]
        public string PlanGroupId { get; set; }

        [DataMember]
        public string CommonInfo { get; set; }

        [DataMember]
        public string PlanGroupName { get; set; }

        [DataMember]
        public string PlanGroupExecutionId { get; set; }

        [DataMember]
        public int PlanOrder { get; set; }

        /// <summary>
        /// 标记属性，标记Job可能的特殊类型
        /// </summary>
        [DataMember]
        public long Tags { get; set; }

        public JobTags Tag
        {
            get { return (JobTags)Tags; }
        }
    }

    [Flags]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum JobTags : long
    {
        [EnumMember]
        Nil = 0,
        [EnumMember]
        RemoteFarm = 1,
        [EnumMember]
        ServiceBus = 2,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RunJobMode
    {
        [EnumMember]
        Regular = 0,
        [EnumMember]
        TestRun = 1
    }

    public enum BackupPoint
    {
        [EnumMember]
        None,
        [EnumMember]
        BackupSource,
        [EnumMember]
        BackupDest,
        [EnumMember]
        BackupSourceAndDest

    }
}
