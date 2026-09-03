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
using System.Linq;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType(typeof(PauseJobDto))]
    [KnownType(typeof(DeleteJobDto))]
    [KnownType(typeof(JobViewDto))]
    [KnownType(typeof(ResumeJobDto))]
    [KnownType(typeof(RibbonStateDto))]
    [KnownType(typeof(SearchJobDto))]
    [KnownType(typeof(StartJobDto))]
    [KnownType(typeof(StopJobDto))]
    [KnownType(typeof(DeleteJobContentDto))]
    [KnownType(typeof(SearchResultDto))]
    [KnownType(typeof(RollbackDto))]
    [KnownType(typeof(RollbackChangesDto))]
    [KnownType(typeof(DeadAccountDeletionDto))]
    [KnownType(typeof(ScheduleJobStatusDto))]
    [KnownType(typeof(WaitingJobDto))]
    public class BaseJobParamDto
    {
        /// <summary>
        /// 登录的用户名
        /// </summary>
        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public JobMonitorCommandType JobMonitorCommandType { get; set; }

        /// <summary>
        /// 查询模块的列表
        /// </summary>
        [DataMember]
        public List<PlanDto> Categorys
        {
            get
            {
                if (Categories == null) return null;
                else return Categories.Select(c => new PlanDto() { Category = c }).ToList();
            }
            set
            {
                if (value != null)
                {
                    Categories = value.Select(p => p.Category).ToList();
                }
            }
        }
        [DataMember]
        public List<PlanCategory> Categories { get; set; }

        /// <summary>
        /// 全选状态
        /// </summary>
        [DataMember]
        public MonitorSelectionType Selection { get; set; }

        /// <summary>
        /// 存储Job Monitor中的一些特殊处理的参数。
        /// </summary>
        [DataMember]
        public JobControlParamDto JobControl { get; set; }

        /// <summary>
        /// 是否为自动刷新job monitor
        /// </summary>
        [DataMember]
        public bool IsBackground { get; set; }

        /// <summary>
        /// 是否选了所有模块
        /// </summary>
        [DataMember]
        public bool IsSelectAllProducts { get; set; }

    }

    /// <summary>
    /// 监控器的选择状态
    /// </summary>
    public enum MonitorSelectionType
    {
        Normal = 0,         // 没有全选，正常选择
        Inverse = 1,        // 反选
        None = 2,           // 一个也没有被选中
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobControlParamDto
    {
        [DataMember]
        public bool IsDeleteBackupData { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobControlResultDto
    {
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public IList<string> FailedJobKeys { get; set; }
        [DataMember]
        public IList<string> SuccessfullJobIds { get; set; }
        [DataMember]
        public List<string> NotDeletedJobIds { get; set; }
    }

    public class JobController
    {
        /// <summary>
        /// 同一模块的Job集合
        /// </summary>
        public List<BaseJobDto> Jobs { get; set; }
        public Dictionary<string, List<SubJobDto>> SubJobMap { get; set; }
    }

    public class JobCleaner : JobController
    {
        public JobClearOption Options { get; set; }
    }

    [Flags]
    public enum JobClearOption
    {
        X = 0,
        RPT = 1,
        SCN = 1 << 1,
        Folder = 1 << 2,
        Log = 1 << 3,
        RCJobData = 1 << 4,
        All = RPT | SCN | Folder | Log | RCJobData,
    }
}
