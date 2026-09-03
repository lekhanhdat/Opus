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




namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    #region using directives
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ChartType
    {
        [EnumMember]
        Pie,
        [EnumMember]
        Column,
        [EnumMember]
        Table,
        [EnumMember]
        Line,
        [EnumMember]
        StackedColumn,
        [EnumMember]
        Bar,
        [EnumMember]
        Area,
        //上边是基本类型，下边是为对应模块定制类型
        [EnumMember]
        NetWorking,
        [EnumMember]
        CpuMemoryUsage,
        [EnumMember]
        Topology,
        [EnumMember]
        Prediction,
        [EnumMember]
        CheckOutDocuments,
        [EnumMember]
        StorageTrends,
        [EnumMember]
        SharePointAlerts,
        [EnumMember]
        MostActiveUsers,
        [EnumMember]
        PageTraffic,
        [EnumMember]
        UserStorageSize,
        [EnumMember]
        SiteUsage,
        [EnumMember]
        SiteReferers,
        [EnumMember]
        SiteActivityAndUsage,
        [EnumMember]
        SearchUsage
    }
}