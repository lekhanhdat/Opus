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
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb.Dashboard;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IReportCollectionService
    {
        void AddTimeLineDatas(List<DataOfDayDto> datas);
        //TimeLineDto GetTimeLineDatas(BoardQueryOption options);

        /// <summary>
        /// remove old data, add new data
        /// </summary>
        /// <param name="chartDtos"></param>
        void AddApprovalAssigneeData(List<PieChartDto> chartDtos);

        void RemoveAllAssignee();
        //void RemoveAllTimeLineData();
        List<PieChartDto> GetTop10ApprovalAssigneeData(BoardQueryOption options);
        Task<LineChartInfo> FindLineChartInfoByTimeRangeAsync(DateTime start, DateTime end, Contract.Explorer.SourceFlag sourceFlag);
        List<BoardTotalDto> GetTotalData();

        List<LineChartItem> GetLineChartItems(LineChartRequestParameter parameter);
        List<BarChartDto> GetTop10SiteCollectionSizeData(int sourceFlag);
        List<BarChartDto> GetTop10TermUsageData(int sourceFlag);
    }
}
