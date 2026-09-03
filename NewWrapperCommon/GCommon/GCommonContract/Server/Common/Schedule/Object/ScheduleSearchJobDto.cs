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
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class ScheduleSearchJobDto : ScheduledJobParamDto
    //{
    //    /// <summary>
    //    /// 值的常量都写在了constants.cs文件中。
    //    /// 0 All Jobs
    //    /// 1 Last 7 Days
    //    /// 2 Last 14 Days
    //    /// 3 Last 30 Days
    //    /// 4 Customized
    //    /// </summary>
    //    [DataMember]
    //    public int RangeType { get; set; }
    //    /// <summary>
    //    /// 当前视图是显示的是DataGrid还是Calendar
    //    /// </summary>
    //    [DataMember]
    //    public ViewType ViewType { get; set; }
    //    [DataMember]
    //    public long FromTime { get; set; }
    //    [DataMember]
    //    public long ToTime { get; set; }
    //    [DataMember]
    //    public int Start { get; set; }
    //    [DataMember]
    //    public int Length { get; set; }

    //    private int _pageIndex = 0;
    //    public int PageIndex
    //    {
    //        get
    //        {
    //            return this._pageIndex;
    //        }
    //        set
    //        {
    //            this._pageIndex = value;
    //        }
    //    }

    //    [DataMember]
    //    public int Type { get; set; }
    //    [DataMember]
    //    public string FuzzyQueryForPlanName { get; set; }
    //    [DataMember]
    //    public string OrderBy { get; set; }
    //    [DataMember]
    //    public int OrderDirection { get; set; }
    //    [DataMember]
    //    public string ConfigName { get; set; }
    //    [DataMember]
    //    public int DispType { get; set; }
    //    [DataMember]
    //    public Dictionary<string, List<string>> Filter { get; set; }

    //    private List<ScheduleColumnOrder> orderList;

    //    [DataMember]
    //    public string DistinctPropName { get; set; }

    //    [DataMember]
    //    public string CustomSearch { get; set; }

    //    [DataMember]
    //    public List<string> CustomSearchPropNameList { get; set; }

    //    [DataMember]
    //    public string[] PlanGroupIDs { get; set; }

    //    [DataMember]
    //    public List<ScheduleColumnOrder> OrderList
    //    {
    //        get
    //        {
    //            if (orderList == null)
    //            {
    //                orderList = new List<ScheduleColumnOrder>();
    //            }
    //            return orderList;
    //        }
    //        set
    //        {
    //            orderList = value;
    //        }
    //    }
    //}

    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class ScheduleColumnOrder
    //{
    //    [DataMember]
    //    public string PropName { get; set; }
    //    /// <summary>
    //    /// asc 或者 desc
    //    /// </summary>
    //    [DataMember]
    //    public string OrderType { get; set; }
    //}
}
