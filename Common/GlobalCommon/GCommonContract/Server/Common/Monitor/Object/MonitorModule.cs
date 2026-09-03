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
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BaseMonitorModule
    {
        [DataMember]
        public List<ColumnModel> Columns { get; set; }
        [DataMember]
        public Dictionary<PlanCategory, List<RibbonModel>> RibbonMap { get; set; }
        /// <summary>
        /// 本次查询的符合条件的job总数
        /// </summary>
        [DataMember]
        public int TotalLength { get; set; }

        /// <summary>
        /// 执行Action后的FeedBack提示语
        /// </summary>
        [DataMember]
        public string FeedBackMessage { get; set; }
    }

    /// <summary>
    /// 所有后台向前台返回的操作数据
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MonitorModule : BaseMonitorModule
    {
        /// <summary>
        /// 动态的FilterList
        /// </summary>
        [DataMember]
        public Dictionary<string, List<Filter>> DynamicFilters { get; set; }

        [DataMember]
        public List<BaseJobDto> SelectionValues { get; set; }
        /// <summary>
        /// 保存当前查询的Job数据
        /// </summary>
        [DataMember]
        public List<BaseJobDto> Values { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScheduleMonitorModule : BaseMonitorModule
    {
        /// <summary>
        /// 保存当前查询的Schedule数据
        /// </summary>
        [DataMember]
        public List<ScheduleDto> Values { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class WaitingMonitorModule : BaseMonitorModule
    {
        /// <summary>
        /// 保存当前查询的Waiting Job数据
        /// </summary>
        [DataMember]
        public List<ScheduleJobQueueDto> Values { get; set; }

        /// <summary>
        /// 保存当前登陆user的有权限的Plans(只有是Standard user才会有值)
        /// </summary>
        [DataMember]
        public List<string> AuthorizedPlans { get; set; }
    }

    /// <summary>
    /// 显示Job Monitor数据模型
    /// </summary>
    public class DisplayModule
    {
        /// <summary>
        /// 数据模型的名称  eg: All Item          default view
        /// </summary>
        public string Name { get; set; }

        public int Type { get; set; }

        public bool IsUsed { get; set; }

        /// <summary>
        /// 用于分页的总记录数
        /// </summary>
        public int TotalLength { get; set; }
    }

    /*/// <summary>
    /// 用于存储返回各个模块默认情况下的ribbon状态
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ModuleRibbonList
    {
        [DataMember]
        public PlanCategory planCategory { set; get; }

        [DataMember]
        public List<MonitorRibbonItem> ribbonItems { set; get; }
    }*/

}