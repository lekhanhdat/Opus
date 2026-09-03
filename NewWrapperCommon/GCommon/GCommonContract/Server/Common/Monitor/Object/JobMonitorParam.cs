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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object
{

    /// <summary>
    /// Job Monitor调用的参数
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobMonitorParam
    {

        /// <summary>
        /// 当前是Job Monitor还是Schedule Job Monitor
        /// </summary>
        [DataMember]
        public MonitorType MonitorType { get; set; }

        /// <summary>
        /// 查询模块的列表
        /// </summary>
        [DataMember]
        [Obsolete]
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
        /// 根据Permission过滤出来的JobTypes
        /// </summary>
        [DataMember]
        public HashSet<int> JobTypes { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [DataMember]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [DataMember]
        public DateTime EndDate { get; set; }


        /// <summary>
        /// Job Monitor所用到的Job操作列表。
        /// </summary>
        [DataMember]
        public List<BaseJobParamDto> Actions { get; set; }

        /// <summary>
        /// 向后台调用的操作命令信息，多个操作命令可以并存， 例如：当前操作是根据条件查询Job列表
        /// </summary>
        [DataMember]
        public List<BaseJobParamDto> Commands { get; set; }

        /// <summary>
        /// 分页用的开始的记录数据
        /// </summary>
        [DataMember]
        public int Start { get; set; }

        /// <summary>
        /// 显示的长度
        /// </summary>
        [DataMember]
        public int Length { get; set; }

        /// <summary>
        /// 查询Job的类型
        /// </summary>
        [DataMember]
        public List<int> TypeList { get; set; }

        /// <summary>
        /// 按升序查询还是按降序查询
        /// </summary>
        [DataMember]
        public List<OrderColumn> Sorts { get; set; }

        /// <summary>
        /// 全选状态
        /// </summary>
        [DataMember]
        public MonitorSelectionType Selection { get; set; }

    }

    /// <summary>
    /// 需要排序属性的定义
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class OrderColumn
    {
        /// <summary>
        /// 需要排序属性的属性名字
        /// </summary>
        [DataMember]
        public string PropName { get; set; }

        /// <summary>
        /// Order < 0 stand for desc
        /// Order > 0 stand for asc
        /// </summary>
        [DataMember]
        public int Order { get; set; }



        /// <summary>
        /// 查询条件用到的job list.
        /// </summary>
        [DataMember]
        public List<JobParam> JobParamList
        {
            get
            {
                if (JobParamList == null)
                {
                    JobParamList = new List<JobParam>();
                }
                return JobParamList;
            }
            private set
            {
                JobParamList = value;
            }
        }
    }


    /// <summary>
    /// 操作数据时所使用的dto ,例如查询，删除job
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class JobParam : BaseJobDto
    {
        [DataMember]
        public string JobId { get; set; }

        /// <summary>
        /// 0 not remove data
        /// 1 remove data
        /// </summary>
        [DataMember]
        public int RemoveData { get; set; }

        /// <summary>
        /// 当前Job 是否被选中
        /// </summary>
        [DataMember]
        public bool IsChecked { get; set; }
    }

    /// <summary>
    /// 当前Ribbon选择的是Job Monitor还是Schedule Job Monitor
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MonitorType
    {
        [EnumMember]
        JobMonitor,             // Job Monitor
        [EnumMember]
        ScheduleJobMonitor,      // Schedule Job Monitor
        [EnumMember]
        ViewMappings,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ViewType
    {
        [EnumMember]
        Zero = 0,
        [EnumMember]
        TableView = 1,
        [EnumMember]
        CalendarView = 2
    }
}