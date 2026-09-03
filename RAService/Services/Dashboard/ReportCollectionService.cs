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

using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Dashboard;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common;
using AvePoint.RA.Service.Services;

namespace AvePoint.RA.Service.Dashboard
{
    public class ReportCollectionService : RMServiceBase, IReportCollectionService
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ReportCollectionService));

        public IRMDataOfDayDao RMDataOfDayDao => PlatformWindsorManager.GetService<IRMDataOfDayDao>();
        public IBoardTotalDao BoardTotalDao => PlatformWindsorManager.GetService<IBoardTotalDao>();
        public IRMSiteCollectionSizeDao RMSiteCollectionSizeDao => PlatformWindsorManager.GetService<IRMSiteCollectionSizeDao>();

        public IRMWaitingApprovalAssigneeDao RMWaitingApprovalAssigneeDao => PlatformWindsorManager.GetService<IRMWaitingApprovalAssigneeDao>();

        public IDashboardTermUsageDao DashboardTermUsageDao => PlatformWindsorManager.GetService<IDashboardTermUsageDao>();

        private IGeneralSettingService  GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private List<string> piecColorList = new List<string>() { "#43ccff", "#fec42c", "#5793f3", "#f2ef1c", "#8560a8", "#a0de3f", "#dd4444", "#3cb878", "#fd9335", "#aaaaaa" };

        #region Dashboard Line Chart
        public List<LineChartItem> GetLineChartItems(LineChartRequestParameter parameter)
        {
            if (parameter.DateRange == ChartDateRange.Last10Days)
            {
                return GetLastDayLineChartItems(parameter.Flag);
            }
            else if (parameter.DateRange == ChartDateRange.Last10Weeks)
            {
                return GetLastWeekLineChartItems(parameter.Flag);
            }
            else
            {
                return GetLastMonthLineChartItems(parameter.Flag);
            }
        }

        private List<LineChartItem> ConvertToLineChartItems(DateTime startDateTime, DateTime endDateTime, List<RMDataOfDay> datas, Func<DateTime, DateTime> getRangePoninter, Func<string, string, string> formatDate)
        {
            var res = new List<LineChartItem>();

            var startPointer = startDateTime;
            var rangePointer = getRangePoninter(startPointer);
            while (rangePointer <= endDateTime)
            {
                var date = formatDate(startPointer.ToString("yyyy-MM-dd"), rangePointer.ToString("yyyy-MM-dd"));
                var createdData = new LineChartItem(LineType.Created, date, 0);
                var destoryedData = new LineChartItem(LineType.Destroyed, date, 0);
                var approvalData = new LineChartItem(LineType.Waiting, date, 0);
                var rangeData = datas.Where(item => item.Dater >= startPointer.Ticks && item.Dater <= rangePointer.Ticks);

                foreach (var data in rangeData)
                {
                    createdData.Value += data.Created;
                    destoryedData.Value += data.Destroyed;
                    approvalData.Value += data.WaitingApproval;
                }

                startPointer = rangePointer.AddDays(1);
                rangePointer = getRangePoninter(startPointer);
                res.Add(createdData);
                res.Add(destoryedData);
                res.Add(approvalData);
            }

            return res;
        }

        private List<LineChartItem> GetLastWeekLineChartItems(SourceFlag flag)
        {
            var day = (int)DateTime.UtcNow.DayOfWeek;
            var startDateTime = DateTime.UtcNow.Subtract(TimeSpan.FromDays(day)).Subtract(TimeSpan.FromDays(9 * 7));
            var endDateTime = DateTime.UtcNow;
            Logger.Info($"The last week find line chart info by source: [{flag}], start time: [{startDateTime}], end time: [{endDateTime}]");
            var dataOfDates = RMDataOfDayDao.FindLineChartInfoByTimeRange(startDateTime, endDateTime, flag);
            return ConvertToLineChartItems(startDateTime, endDateTime, dataOfDates, (dataTime) => dataTime.AddDays(6), (startDateStr, endDateStr) => startDateStr + "~" + endDateStr);
        }

        private List<LineChartItem> GetLastMonthLineChartItems(SourceFlag flag)
        {
            var startDateTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-11);
            var endDateTime = DateTime.UtcNow;
            Logger.Info($"The last month find line chart info by source: [{flag}], start time: [{startDateTime}], end time: [{endDateTime}]");
            var dataOfDates = RMDataOfDayDao.FindLineChartInfoByTimeRange(startDateTime, endDateTime, flag);
            return ConvertToLineChartItems(startDateTime, endDateTime, dataOfDates, (dataTime) => dataTime.AddMonths(1).AddDays(-1), (startDateStr, endDateStr) => startDateStr + "~" + endDateStr);
        }

        private List<LineChartItem> GetLastDayLineChartItems(SourceFlag flag)
        {
            var startDateTime = DateTime.UtcNow.Subtract(TimeSpan.FromDays(10));
            var endDateTime = DateTime.UtcNow;
            Logger.Info($"The last day find line chart info by source: [{flag}], start time: [{startDateTime}], end time: [{endDateTime}]");
            var dataOfDates = RMDataOfDayDao.FindLineChartInfoByTimeRange(startDateTime, endDateTime, flag);
            return ConvertToLineChartItems(startDateTime, endDateTime, dataOfDates, (dataTime) => dataTime, (startDateStr, endDateStr) => startDateStr);
        }
        #endregion

        public void AddTimeLineDatas(List<DataOfDayDto> datas)
        {

            RMDataOfDayDao.AddDatas(datas);
        }


        public void AddApprovalAssigneeData(List<PieChartDto> chartDtos)
        {

            RMWaitingApprovalAssigneeDao.AddDatas(chartDtos);

        }

        public void RemoveAllAssignee()
        {
            RMWaitingApprovalAssigneeDao.RemoveAll();
        }

        //public void RemoveAllTimeLineData()
        //{
        //    RMDataOfDayDao.RemoveAll();
        //}



        public List<PieChartDto> GetTop10ApprovalAssigneeData(BoardQueryOption options)
        {
            List<PieChartDto> result = new List<PieChartDto>();
            var dbwa = RMWaitingApprovalAssigneeDao.GetDatas(options);
     
            result = ConvertToChartDto(dbwa);
            return result;
        }

        private List<PieChartDto> ConvertToChartDto(List<RMWaitingApprovalAssignee> assignees)
        {
            if (assignees == null || assignees.Count() == 0) return new List<PieChartDto>();
            int i = 0;
            return assignees.ConvertAll(a => new PieChartDto()
            { Id = a.Id, name = a.Asssignee, data = a.Count, color = piecColorList[i++] });
        }




        public async Task<LineChartInfo> FindLineChartInfoByTimeRangeAsync(DateTime start, DateTime end, Contract.Explorer.SourceFlag sourceFlag)
        {
            LineChartInfo info = new LineChartInfo()
            {
                ChartInfos = new List<LineChart>()
            };
            long maxCount = 0;
            info.ChartInfos.Add(new LineChart() { LineName = "Created", LineType = LineType.Created, Nodes = new List<LineChartNode>() });
            info.ChartInfos.Add(new LineChart() { LineName = "Destroyed", LineType = LineType.Destroyed, Nodes = new List<LineChartNode>() });
            info.ChartInfos.Add(new LineChart() { LineName = "Waiting Approval", LineType = LineType.Waiting, Nodes = new List<LineChartNode>() });
            string DateTimeFormat = await GeneralSettingService.GetDateFormatAsync();
            List<RMDataOfDay> searchData = RMDataOfDayDao.FindLineChartInfoByTimeRange(start, end, sourceFlag);

            if (searchData == null) { return info; }
            foreach (var d in searchData)
            {
                DateTime temple = new DateTime(d.Dater);

                var globalTimeZoneId = (await GeneralSettingService.GetGeneralSettingAsync()).TimeZoneId;
                TimeZoneInfo cstZone = GeneralSettingConfig.FindSystemTimeZoneById(globalTimeZoneId);
                var utcDateTime = DateTime.SpecifyKind(temple, DateTimeKind.Utc);
                temple = temple + cstZone.GetUtcOffset(utcDateTime);
                long tempMax = 0;
                foreach (var cInfo in info.ChartInfos)
                {
                    long valueCount = 0;
                    switch (cInfo.LineType)
                    {
                        case LineType.Created:
                            valueCount = d.Created;
                            break;
                        case LineType.Destroyed:
                            valueCount = d.Destroyed;
                            break;
                        case LineType.Waiting:
                            valueCount = d.WaitingApproval;
                            break;
                        default:
                            break;
                    }
                    tempMax = valueCount > tempMax ? valueCount : tempMax;
                    cInfo.Nodes.Add(new LineChartNode
                    {
                        LabelStr = temple.ToString(DateTimeFormat),
                        LabelVal = temple.ToString(DateTimeFormat),
                        day = temple.Day,
                        month = temple.Month,
                        year = temple.Year,
                        valueCount = valueCount,
                        dateOfWeek = temple.DayOfWeek
                    });
                }
                maxCount = tempMax > maxCount ? tempMax : maxCount;

            }
            info.MaxNum = maxCount;

            return info;
        }

        public List<BoardTotalDto> GetTotalData()
        {
            return BoardTotalDao.GetTotalInfo().ConvertAll(o => ConvertToBoardTotalDto(o));
        }
        public List<BarChartDto> GetTop10SiteCollectionSizeData(int sourceFlag)
        {
            var result = new List<BarChartDto>();
            var data = RMSiteCollectionSizeDao.GetTop10SiteCollectionSizes(sourceFlag);
            foreach (var d in data)
            {
                result.Add(new BarChartDto() { Title = d.Title, Content = d.Size, TooltipValue = d.SiteUrl});
            }

            return result;
        }
        private BoardTotalDto ConvertToBoardTotalDto(BoardTotal total)
        {
            BoardTotalDto dto = new BoardTotalDto();
            if (null == total) { return dto; }
            dto.SourceFlag = total.SourceFlag;
            dto.CreatedTotal = total.CreatedTotal;
            dto.DestroyTotal = total.DestroyedTotal;
            dto.WaitingTotal = total.WaitingTotal;
            dto.CollectionTime = total.CollectionTime;
            return dto;
        }
              
        public List<BarChartDto> GetTop10TermUsageData(int sourceFlag)
        {
            var result = new List<BarChartDto>();
            var datas = DashboardTermUsageDao.GetTermUsagesBySourceFlag(sourceFlag);
            foreach (var d in datas)
            {
                result.Add(new BarChartDto() { Title = d.TermName, Content = d.Active, TooltipValue = d.TermFullPath });
            }
            return result;
        }
    }
}
