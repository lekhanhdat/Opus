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



using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections.Generic;
using AvePoint.GCommon.Contract.CentralAdmin.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]

    [KnownType("GetKnownTypes")]
    public class BaseJobDto : INotifyPropertyChanged, IJob
    {
        public static IEnumerable<Type> GetKnownTypes()
        {
            return AveKnownTypeContext.GetKnonwTypes(MethodBase.GetCurrentMethod().DeclaringType);
        }


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

        private string _CompletePercent;

        private string _CompletePercentStr;

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
                if (value != this._Progress)
                {
                    this._Progress = value;
                    NotifyPropertyChanged("Progress");
                }
            }
        }
        
        [DataMember]
        public string CompletePercent
        {
            get
            {
                return this._CompletePercent;
            }
            set
            {
                if (value != this._CompletePercent)
                {
                    this._CompletePercent = value;
                    NotifyPropertyChanged("CompletePercent");
                }
            }
        }

        [DataMember]
        public string CompletePercentStr
        {
            get
            {
                return this._CompletePercentStr;
            }
            set
            {
                if (value != this._CompletePercentStr)
                {
                    this._CompletePercentStr = value;
                    NotifyPropertyChanged("CompletePercentStr");
                }
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

        /// <summary>
        /// 用于权限判断时的过滤条件
        /// </summary>
        [DataMember]
        public FiltrationXml Filtration { get; set; }

        /// <summary>
        /// 当job放入job池时用于存贮job 执行方式的JobRunSettings xml配置信息
        /// </summary>
        [DataMember]
        public string SendModeSetting { get; set; }
    }

    /// <summary>
    /// 用于序列化job权限过滤条件如Farm ...
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FiltrationXml
    {
        /// <summary>
        /// job相关的Farm
        /// </summary>
        [DataMember]
        public List<NameAndId> Farms { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NameAndId
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public FarmType FarmType { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum RunJobMode
    {
        [EnumMember]
        Regular = 0,

        [EnumMember]
        TestRun = 1,

        [EnumMember]
        RerunWithDebugLogging = 2,

        [EnumMember]
        Retry = 3,

        [EnumMember]
        Rerun = 4,

        [EnumMember]
        Resume = 5,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
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