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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using AvePoint.Common;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object
{
    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum RibbonOption
    {
        [EnumMember]
        Nil = 0,

        /// <summary>
        /// 可见
        /// </summary>
        [EnumMember]
        Visible = 1,

        /// <summary>
        /// 禁用
        /// </summary>
        [EnumMember]
        Disable = 1 << 1,

        /// <summary>
        /// 单选
        /// </summary>
        [EnumMember]
        Single = 1 << 2,

        /// <summary>
        /// 正选
        /// </summary>
        [EnumMember]
        Basic = 1 << 3,

        /// <summary>
        /// 乐观
        /// </summary>
        [EnumMember]
        Optimistic = 1 << 4,

        /// <summary>
        /// 禁用时隐藏
        /// </summary>
        [EnumMember]
        HideIfDisable = 1 << 5,

        /// <summary>
        /// 单个模块内多选
        /// </summary>
        [EnumMember]
        SingleCategory = 1 << 6,

        /// <summary>
        /// 所有模块
        /// </summary>
        [EnumMember]
        AllWilling = 1 << 7,
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class RibbonModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 类型
        /// </summary>
        [DataMember]
        public int Type { set; get; }

        /// <summary>
        /// 项集
        /// </summary>
        [DataMember]
        public RibbonOption Options { get; set; }

        /// <summary>
        /// 可用状态集
        /// </summary>
        [DataMember]
        public List<int> States { get; set; }

        /// <summary>
        /// 禁用 : 可用
        /// </summary>
        public bool Disable
        {
            get { return (Options & RibbonOption.Disable) == RibbonOption.Disable; }
            set
            {
                if (value)
                {
                    Options |= RibbonOption.Disable;
                }
                else
                {
                    Options = ~RibbonOption.Disable & Options;
                }
                OnPropertyChanged("Disable");
            }
        }

        /// <summary>
        /// 可见 : 不可见
        /// </summary>
        public bool Visible
        {
            get
            {
                return (Options & RibbonOption.Visible) == RibbonOption.Visible;
            }
            set
            {
                if (value)
                {
                    Options |= RibbonOption.Visible;
                }
                else
                {
                    Options = ~RibbonOption.Visible & Options;
                }
                OnPropertyChanged("Visible");
            }
        }

        /// <summary>
        /// 单选可用 : 多选可用
        /// </summary>
        public bool OneWilling
        {
            get
            {
                return (Options & RibbonOption.Single) == RibbonOption.Single;
            }
        }

        /// <summary>
        /// 单模块多选 : 多选
        /// </summary>
        public bool OneCategoryWilling
        {
            get
            {
                return ((Options & RibbonOption.SingleCategory) == RibbonOption.SingleCategory) && ((Options & RibbonOption.Single) != RibbonOption.Single);
            }
        }

        /// <summary>
        /// 正选可用 : 正反选可用
        /// </summary>
        public bool Basic
        {
            get
            {
                return (Options & RibbonOption.Basic) == RibbonOption.Basic;
            }
        }

        /// <summary>
        /// 不可用时隐藏 ? default
        /// </summary>
        public bool HideIfDisable
        {
            get
            {
                return (Options & RibbonOption.HideIfDisable) == RibbonOption.HideIfDisable;
            }
        }

        /// <summary>
        /// 乐观？悲观
        /// 多个Ribbon合并时，只要有一个可用,则忽略其他。
        /// 即便乐观成立，也要参与比较的所有Ribbon可见
        /// </summary>
        public bool Optimistic
        {
            get
            {
                return (Options & RibbonOption.Optimistic) == RibbonOption.Optimistic;
            }
            set
            {
                if (value)
                {
                    Options |= RibbonOption.Optimistic;
                }
                else
                {
                    Options = ~RibbonOption.Optimistic & Options;
                }
            }

        }

        public bool AllWilling
        {
            get
            {
                return (Options & RibbonOption.AllWilling) == RibbonOption.AllWilling;
            }
        }

        #region Constructors
        public RibbonModel() { }
        public RibbonModel(JobMonitorRibbonType type)
        {
            this.Type = (int)type;
        }
        public RibbonModel(JobMonitorRibbonType type, RibbonOption option)
            : this(type)
        {
            this.Options = option;
        }

        public RibbonModel(JobMonitorRibbonType type, RibbonOption option, JobState[] states)
            : this(type, option)
        {
            this.States = states.Select(s => (int)s).ToList();
        }

        public RibbonModel(JobMonitorRibbonType type, RibbonOption option, List<int> states)
            : this(type, option)
        {
            this.States = states;
        }

        #endregion Constructors

        /// <summary>
        /// shadow Clone
        /// </summary>
        public RibbonModel Clone()
        {
            return this.MemberwiseClone() as RibbonModel;
        }

        /// <summary>
        /// 合并Ribbon状态时使用，主要针对 Disable 及 Visible
        /// 会覆盖Type
        /// 会合并参数状态到本身
        /// 本身乐观将会影响合并结果
        /// </summary>
        /// <param name="model">other model</param>
        /// <returns>self</returns>
        public RibbonModel Fix(RibbonModel model)
        {
            this.Type = model.Type;

            if (this.Optimistic)
            {
                this.Disable &= model.Disable;
                this.Visible |= model.Visible;
            }
            else
            {
                this.Disable |= model.Disable;
                this.Visible &= model.Visible;
            }
            return this;
        }

        #region INotifyPropertyChanged members.
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string property)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }
        #endregion INotifyPropertyChanged members.


    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum JobMonitorRibbonType
    {
        [EnumMember]
        Zero = 0,

        [EnumMember]
        Close = 2,
        [EnumMember]
        ListView = 99,
        [EnumMember]
        CalendarView = 98,
        [EnumMember]
        TimeZone = 97,
        [EnumMember]
        DateRange = 10,
        [EnumMember]
        Module = 4,
        [EnumMember]
        Download = 16,

        #region Job
        [EnumMember]
        DeleteWithoutNotification = 1,
        [EnumMember]
        ViewDetail = 3,
        [EnumMember]
        Stop = 5,
        [EnumMember]
        Pause = 6,
        [EnumMember]
        Resume = 7,
        [EnumMember]
        Index = 8,
        [EnumMember]
        Rollback = 9,
        [EnumMember]
        DeleteContent = 11,
        [EnumMember]
        Start = 12,
        [EnumMember]
        Restart = 13,
        [EnumMember]
        Mapping = 14,
        [EnumMember]
        CopySnapshot = 15,
        [EnumMember]
        Delete = 17,
        [EnumMember]
        ReportLocation = 18,
        [EnumMember]
        DeadAccountDeletion = 21,
        [EnumMember]
        SearchResult = 22,
        [EnumMember]
        RollbackChanges = 23,
        [EnumMember]
        Maintenance = 24,
        [EnumMember]
        ViewMappings = 25,
        [EnumMember]
        ViewBlob = 26,
        [EnumMember]
        OrphanSitesDeletion = 27,
        [EnumMember]
        ViewItemLife = 28,
        [EnumMember]
        ViewListAccess = 29,
        [EnumMember]
        ViewListDeletion = 30,
        [EnumMember]
        ViewSiteAccess = 31,
        [EnumMember]
        ViewUserLife = 32,
        [EnumMember]
        WebPartManagement = 33,
        [EnumMember]
        ViewWorkFlowStatusReport = 34,
        [EnumMember]
        ViewCustomizedReport = 35,
        [EnumMember]
        ViewUserPermission = 36,
        [EnumMember]
        DuplicateFileTool = 37,
        [EnumMember]
        ViewBestPractice = 38,
        [EnumMember]
        ViewAuditor = 41,
        [EnumMember]
        ViewMetadataChanges = 42,
        [EnumMember]
        ViewContentTypeUsage = 43,
        [EnumMember]
        ViewContentTypeChanges = 44,
        [EnumMember]
        DeleteSourceContents = 45,
        [EnumMember]
        BreakpointResume = 46,
        #endregion

        #region Schedule

        [EnumMember]
        Enable = 39,
        [EnumMember]
        Disable = 40,

        #endregion

        #region Waiting Job
        [EnumMember]
        Remove = 47,
        [EnumMember]
        Promote = 48,
        [EnumMember]
        Refresh = 49,
        #endregion

        [EnumMember]
        DeleteJobAndData = 111,
        [EnumMember]
        CopyJobId = 112,
    }
}
