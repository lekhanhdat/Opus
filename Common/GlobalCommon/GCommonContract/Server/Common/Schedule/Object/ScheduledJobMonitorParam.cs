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






namespace AvePoint.GCommon.Contract.Server.Common.Schedule.Object
{
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class ScheduledJobMonitorParam
    //{
    //    /// <summary>
    //    /// 当前是Job Monitor还是Schedule Job Monitor
    //    /// </summary>
    //    [DataMember]
    //    public MonitorType MonitorType { get; set; }

    //    /// <summary>
    //    /// 查询模块的列表
    //    /// </summary>
    //    [DataMember]
    //    public List<PlanDto> Categorys { get; set; }

    //    /// <summary>
    //    /// Job Monitor所用到的Job操作列表。
    //    /// </summary>
    //    [DataMember]
    //    public List<ScheduledJobParamDto> Actions { get; set; }

    //    /// <summary>
    //    /// 向后台调用的操作命令信息，多个操作命令可以并存， 例如：当前操作是根据条件查询Job列表
    //    /// </summary>
    //    [DataMember]
    //    public List<ScheduledJobParamDto> Commands { get; set; }

    //    /// <summary>
    //    /// 分页用的开始的记录数据
    //    /// </summary>
    //    [DataMember]
    //    public int Start { get; set; }

    //    /// <summary>
    //    /// 显示的长度
    //    /// </summary>
    //    [DataMember]
    //    public int Length { get; set; }

    //    /// <summary>
    //    /// 查询Job的类型
    //    /// </summary>
    //    [DataMember]
    //    public List<int> TypeList { get; set; }

    //    /// <summary>
    //    /// 按升序查询还是按降序查询
    //    /// </summary>
    //    [DataMember]
    //    public List<OrderColumn> Sorts { get; set; }

    //    /// <summary> 
    //    /// 开始时间 
    //    /// </summary> 
    //    [DataMember]
    //    public DateTime StartDate { get; set; }

    //    /// <summary> 
    //    /// 结束时间
    //    /// </summary> 
    //    [DataMember]
    //    public DateTime EndDate { get; set; }


    //}

    ///// <summary>
    ///// 需要排序属性的定义
    ///// </summary>
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class OrderColumn
    //{
    //    /// <summary>
    //    /// 需要排序属性的属性名字
    //    /// </summary>
    //    [DataMember]
    //    public string PropName { get; set; }

    //    /// <summary>
    //    /// Order < 0 stand for desc
    //    /// Order > 0 stand for asc
    //    /// </summary>
    //    [DataMember]
    //    public int Order { get; set; }

    //    /// <summary>
    //    /// 查询条件用到的job list.
    //    /// </summary>
    //    [DataMember]
    //    public List<JobParam> JobParamList
    //    {
    //        get
    //        {
    //            if (JobParamList == null)
    //            {
    //                JobParamList = new List<JobParam>();
    //            }
    //            return JobParamList;
    //        }
    //        private set
    //        {
    //            JobParamList = value;
    //        }
    //    }
    //}


    ///// <summary>
    ///// 操作数据时所使用的dto ,例如查询，删除job
    ///// </summary>
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class JobParam : ScheduleDto
    //{
    //    [DataMember]
    //    public string ScheduleId { get; set; }

    //    /// <summary>
    //    /// 当前Job 是否被选中
    //    /// </summary>
    //    [DataMember]
    //    public bool IsChecked { get; set; }
    //}

    ///// <summary>
    ///// 当前Ribbon选择的是Job Monitor还是Schedule Job Monitor
    ///// </summary>
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public enum MonitorType
    //{
    //    [EnumMember]
    //    JobMonitor,             // Job Monitor
    //    [EnumMember]
    //    ScheduleJobMonitor      // Schedule Job Monitor

    //}
}
