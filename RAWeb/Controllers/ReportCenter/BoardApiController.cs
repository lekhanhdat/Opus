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
using AvePoint.RA.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Dashboard;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.ReportCenter
{
    [RMApiAuthorize(Contract.RoleAssignments.RMPermissionMasks.JobMonitorAdmin, preferred: false)]
    public class BoardApiController : BaseApiController
    {
        private IReportCollectionService _ReportCollectionService;
        private IReportCollectionService ReportCollectionService => PlatformWindsorManager.GetService(ref _ReportCollectionService);
        private IRMCollectionDataService _DataCollectionService;
        private IRMCollectionDataService DataCollectionService => PlatformWindsorManager.GetService(ref _DataCollectionService);
        private IGeneralSettingService _GeneralService;
        private IGeneralSettingService GeneralService => PlatformWindsorManager.GetService(ref _GeneralService);

        public Dictionary<RecordStatus, long> GetRecordsStatusCount()
        {
            var res = new Dictionary<RecordStatus, long>
            {

            };

            return res;
        }

        [HttpPost]
        public List<LineChartItem> GetLineChartItems([FromBody] LineChartRequestParameter parameter)
        {
            return ReportCollectionService.GetLineChartItems(parameter);
        }

        [HttpPost]
        public List<BarChartDto> GetTopUsageSiteCollections([FromBody] SourceFlag flag)
        {
            return ReportCollectionService.GetTop10SiteCollectionSizeData((int)flag);
        }

        [HttpPost]
        public List<BarChartDto> GetTopUsageTerms([FromBody] SourceFlag flag)
        {
            return ReportCollectionService.GetTop10TermUsageData((int)flag);
        }

        [HttpPost]
        public Dictionary<RecordStatus, long> GetManagedRecordsCount()
        {
            var res = new Dictionary<RecordStatus, long>
            {
                {RecordStatus.Created, 0 },
                {RecordStatus.Destoryed, 1 }
            };

            var totalData = ReportCollectionService.GetTotalData();
            totalData.ForEach(item =>
            {
                res[RecordStatus.Created] += item.CreatedTotal;
                res[RecordStatus.Destoryed] += item.DestroyTotal;
            });

            return res;
        }

        [HttpPost]
        public Dictionary<SourceFlag, long> GetAllSourceActiveRecordsCount()
        {
            var res = new Dictionary<SourceFlag, long>
            {
                {SourceFlag.SharePoint, 0 },
                {SourceFlag.Exchange, 0 },
                {SourceFlag.FileSystem, 0 },
                {SourceFlag.Physical, 0 },
                {SourceFlag.SharePointOnPrem, 0 }
            };
            var totalData = ReportCollectionService.GetTotalData();
            totalData.ForEach(item =>
            {
                res[(SourceFlag)item.SourceFlag] += item.CreatedTotal;
            });
            return res;
        }

        [HttpPost]
        public Dictionary<SourceFlag, long> GetAllSourceUniqueSettingCount()
        {
            var res = new Dictionary<SourceFlag, long>
            {
                {SourceFlag.SharePoint, 0 },
                {SourceFlag.Exchange, 0 },
                {SourceFlag.FileSystem, 0 },
                {SourceFlag.Physical, 0 },
                {SourceFlag.SharePointOnPrem, 0 }
            };

            return res;
        }

        [HttpPost]
        public async Task<string> GetAllDataInfo([FromBody]BoardQueryOption options)
        {
            List<PieChartDto> assigneeData = ReportCollectionService.GetTop10ApprovalAssigneeData(options);
            BoardTotalDto total = new BoardTotalDto();
            SourcePieDto SPieDto = new SourcePieDto();
            SPieDto.Sources = new List<PieChartDto>();
                
            var totalDto = ReportCollectionService.GetTotalData();
            LineChartInfo timeLineData = await GetLineChartInfo(options.LineChartPageMode);
            var globalTimeZoneId = (await GeneralService.GetGeneralSettingAsync()).TimeZoneId;
            TimeZoneInfo cstZone = GeneralSettingConfig.FindSystemTimeZoneById(globalTimeZoneId);
            string DateTimeFormat = await GeneralService.GetDateTimeFormatAsync();
            string displayLastTime = string.Empty;
            if (totalDto != null && totalDto.Count > 0)
            {

                var dt = new DateTime(totalDto.Max(t => t.CollectionTime), DateTimeKind.Utc);
                if (dt != DateTime.MinValue)
                {
                    dt = dt + cstZone.GetUtcOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc));
                    displayLastTime = dt.ToString(DateTimeFormat);
                    total.LastJobTime = string.Format(I18NEntity.GetString("RM_DSB_LastJobTime"), displayLastTime);
                }
                SPieDto.Sources = new List<PieChartDto>()
                {
                    new PieChartDto() { Id = 1, name = I18NEntity.GetString("RM_JS_SPS_TabLabel_SP"), data = 0, color = "#70ad47" },
                    new PieChartDto() { Id = 2, name = I18NEntity.GetString("RM_JS_SPS_TabLabel_FS"), data = 0, color = "#f7941d" },
                    new PieChartDto() { Id = 3, name = I18NEntity.GetString("RM_JS_SPS_TabLabel_EXO"), data = 0, color = "#fec42c" },
                    new PieChartDto() { Id = 4, name = I18NEntity.GetString("RM_JS_SPS_TabLabel_Physical"), data = 0, color = "#448ccb" },
                    new PieChartDto() { Id = 5, name = I18NEntity.GetString("RM_JS_SPS_TabLabel_SPLocal"), data = 0, color = "#8560a8" },
                    new PieChartDto() { Id = 6, name = I18NEntity.GetString("RM_JS_SPS_TabLabel_OneDrive"), data = 0, color = "#dd4444" },
                };
                foreach (var item in totalDto)
                {
                    total.CreatedTotal += item.CreatedTotal;
                    total.DestroyTotal += item.DestroyTotal;
                    total.WaitingTotal += item.WaitingTotal;
                    var p = SPieDto.Sources.Where(s => s.Id == item.SourceFlag).FirstOrDefault();
                    if (p != null)
                    {
                        p.data += item.CreatedTotal;
                    }
                }

            }
            var boardDto = new BoardDto()
            {
                TotalInfo = total,
                SourcePie = SPieDto,
                AssigneesDto = new AssigneesDto() { Assignees = assigneeData },
                LineChartInfo = timeLineData
            };
            return JsonConvert.SerializeObject(boardDto);
        }

        [HttpPost]
        public async Task<LineChartInfo> GetLineChartInfo([FromBody]LineChartPageMode mode)
        {
            (var start,var end) = await GetRangeDateAsync(mode.StartTime, mode.EndTime, mode);
            LineChartInfo result = await GetChartInfosAsync((DateTime)start, (DateTime)end, mode.SourceFlag);
            //该处是要处理chart的纵轴，需要把时间转换成Local的进行处理
            var globalTimeZoneId = (await GeneralService.GetGeneralSettingAsync()).TimeZoneId;
            TimeZoneInfo cstZone = GeneralSettingConfig.FindSystemTimeZoneById(globalTimeZoneId);
            start = start.Value + cstZone.GetUtcOffset(DateTime.SpecifyKind(start.Value, DateTimeKind.Utc));
            end = end.Value + cstZone.GetUtcOffset(DateTime.SpecifyKind(end.Value, DateTimeKind.Utc));
            string DateTimeFormat = await GeneralService.GetDateFormatAsync();
            var totalDays = (end.Value - start.Value).Days;
            Dictionary<string, LineChartNode> tempCreatedDic = new Dictionary<string, LineChartNode>(totalDays);
            Dictionary<string, LineChartNode> tempDestroyedDic = new Dictionary<string, LineChartNode>(totalDays);
            Dictionary<string, LineChartNode> tempWaitingApprovalDic = new Dictionary<string, LineChartNode>(totalDays);
            for (DateTime s = start.Value; DateTime.Compare(s, end.Value) <= 0; s = s.AddDays(1))
            {
                var chartNode = new LineChartNode()
                {
                    LabelStr = s.ToString(DateTimeFormat),
                    valueCount = 0,
                    dateOfWeek = s.DayOfWeek,
                    year = s.Year,
                    day = s.Day,
                    month = s.Month
                };
                tempCreatedDic[s.ToString(DateTimeFormat)] = chartNode;
                tempDestroyedDic[s.ToString(DateTimeFormat)] = chartNode;
                tempWaitingApprovalDic[s.ToString(DateTimeFormat)] = chartNode;
            }

            result.ChartInfos.ForEach((x) =>
            {
                x.Nodes.ForEach((n) => 
                {
                    switch (x.LineType)
                    {
                        case LineType.Created:
                            tempCreatedDic[n.LabelStr] = n;
                            break;
                        case LineType.Destroyed:
                            tempDestroyedDic[n.LabelStr] = n;
                            break;
                        case LineType.Waiting:
                            tempWaitingApprovalDic[n.LabelStr] = n;
                            break;
                        default:
                            break;
                    }
                    
                });
            });

            result.ChartInfos[0].Nodes = tempCreatedDic.Values.ToList();
            result.ChartInfos[1].Nodes = tempDestroyedDic.Values.ToList();
            result.ChartInfos[2].Nodes = tempWaitingApprovalDic.Values.ToList();
            result.Start = start.Value.ToString(DateTimeFormat);
            result.End = end.Value.ToString(DateTimeFormat);
            return result;
        }

        private Task<LineChartInfo> GetChartInfosAsync(DateTime start, DateTime end, Contract.Explorer.SourceFlag sourceFlag)
        {

            return ReportCollectionService.FindLineChartInfoByTimeRangeAsync(start, end, sourceFlag);



        }

        //[HttpGet]
        //public string start()
        //{
        //    DataCollectionService.RunScheduleJob(JobRunBy.Schedule, Contract.JobMonitor.JobType.CollectionDataFull);
        //    return true.ToString();
        //}

        [NonAction]
        private async Task<(DateTime? start, DateTime? end)> GetRangeDateAsync(DateTime? start, DateTime? end, LineChartPageMode mode)
        {
            var globalTimeZoneId = (await GeneralService.GetGeneralSettingAsync()).TimeZoneId;
            TimeZoneInfo cstZone = GeneralSettingConfig.FindSystemTimeZoneById(globalTimeZoneId);

            DateTime now = DateTime.UtcNow;
            //获取当前的日期，需要转换成Local的
            now = now + cstZone.GetUtcOffset(now);
            DateTime tmp = new DateTime();
            if (mode.Range != ChartDateRange.Custom)
            {
                end = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
                tmp = new DateTime(end.Value.Year, end.Value.Month, 1, 0, 0, 0);
            }
            switch (mode.Range)
            {
                case ChartDateRange.Last10Weeks:
                    //每一周的第一天为周一
                    int addDaysTemp = ((int)end.Value.DayOfWeek == 0 ? -6 : -(int)end.Value.DayOfWeek + 1) - 7 * 9;
                    start = end.Value.AddDays(addDaysTemp).AddHours(-23).AddMinutes(-59).AddSeconds(-59);
                    break;
                case ChartDateRange.Last10Days:
                    start = end.Value.AddDays(-9);
                    break;
                case ChartDateRange.Last12Month:
                    start = tmp.AddYears(-1).AddMonths(1);
                    break;
                case ChartDateRange.Custom:
                    start = new DateTime(start.Value.Year, start.Value.Month, start.Value.Day, 0, 0, 0);
                    end = new DateTime(end.Value.Year, end.Value.Month, end.Value.Day, 23, 59, 59);
                    break;
                default:
                    start = end.Value.AddDays(-5).AddHours(-23).AddMinutes(-59).AddSeconds(-59);
                    break;
            }
            //}
            //由于下一步需要查询，需要将时间转换会UTC的时间。
            start = start.Value - cstZone.GetUtcOffset(start.Value);
            end = end.Value - cstZone.GetUtcOffset(end.Value);
            return (start, end);
        }

        [HttpPost]
        public string GetSiteCollectionInfo([FromBody] LineChartPageMode mode)
        {
            List<BarChartDto> data = ReportCollectionService.GetTop10SiteCollectionSizeData((int)mode.SourceFlag);
            return JsonConvert.SerializeObject(data);
        }

        [HttpPost]
        public string GetTermUsageInfo([FromBody] LineChartPageMode mode)
        {
            int sourceFlag = (int)mode.SourceFlag;
            List<BarChartDto> data = ReportCollectionService.GetTop10TermUsageData(sourceFlag);
            return JsonConvert.SerializeObject(data);
        }


        [HttpPost]
        public string ReCalculateSites()
        {
            Logger.Info("Reclaculate item count of all sites from api.");
            string jobId = null;
            try
            {
                jobId = DataCollectionService.RunScheduleJob(JobRunBy.Control, Contract.JobMonitor.JobType.CollectionDataFull);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                return "Start job failed:" + e.Message;
            }
            return string.Format("Job started: {0}", jobId);
        }
    }
}
