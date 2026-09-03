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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common.Schedule.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ScheduledMonitorModule
    {
        #region Scheduled Ribbon Module
        [DataMember]
        public List<ScheduledRibbonModule> ScheduledRibbonModule { get; set; }

        [DataMember]
        public List<ScheduledMonitorRibbonItem> ScheduledRibbonItems { get; set; }

        [DataMember]
        public Dictionary<PlanCategory, List<ScheduledMonitorRibbonItem>> ScheduledModuleRibbons { get; set; }
        #endregion

        #region Scheduled Job Monitor View Module
        [DataMember]
        public List<ScheduledDisplayModule> ScheduledDisplayModule { get; set; } 
        #endregion

        /// <summary>
        /// 列配置的集合
        /// </summary>
        [DataMember]
        public List<ScheduledColumnModule> ColumnModules { get; set; }

        /// <summary>
        /// 动态的FilterList
        /// </summary>
        [DataMember]
        public List<ScheduledFilter> DynamicFilters { get; set; }

        /// <summary>
        /// 保存当前查询的Job数据
        /// </summary>
        [DataMember]
        public List<ScheduleDto> Values { get; set; }
        /// <summary>
        /// 本次查询的符合条件的job总数
        /// </summary>
        [DataMember]
        public int TotalLength { get; set; }
    }

    /// <summary>
    /// 显示Scheduled Job Monitor数据模型
    /// </summary>
    public class ScheduledDisplayModule
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
}
