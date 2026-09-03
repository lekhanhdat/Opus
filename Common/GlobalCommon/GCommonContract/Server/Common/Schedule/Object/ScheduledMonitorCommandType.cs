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





using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common.Schedule.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ScheduledMonitorCommandType
    {
        [EnumMember]
        GetScheduledValue = 0,  // 根据条件获得schedule列表

        [EnumMember]
        GetScheduleStates = 1,

        [EnumMember]
        DeleteScheduledValues = 2,  // 删除schedule的操作状态

        [EnumMember]
        OptionRibbon = 3,  // 操作Ribbon的操作状态

        [EnumMember]
        GetScheduleDetail = 4,  //获取schedule的详细信息

        [EnumMember]
        GetRibbonState = 5, //根据checkbox的选中状态获取ribbon的状态

        [EnumMember]
        GetScheduleView = 6, //获取schedule视图操作

        [EnumMember]
        UpdateScheduleStatus = 7
    }
}
