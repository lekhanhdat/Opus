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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Web.Models.ReportCenter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.ReportCenter
{
    [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, RMDiscoveryPermissionMasks.AccessAll, RMDiscoverySalesforcePermissionMask.AccessAll, RMDiscoveryGoogleROTPermissionMask.AccessAll, RMDiscoveryFileSystemPermissionMask.AccessAll, preferred: false)]
    public class AuditApiController : BaseApiController
    {
        private IAuditService _AuditService;
        private IAuditService AuditService => PlatformWindsorManager.GetService(ref _AuditService);
        private IGeneralSettingService _GeneralService;
        private IGeneralSettingService GeneralService => PlatformWindsorManager.GetService(ref _GeneralService);

        /// <summary>
        /// 获取detail
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        [RACodeReview("Allen Yin")]
        [HttpPost]
        public async Task<AuditTable> GetTableInfo([FromBody]AuditPageMode mode)
        {
            int pageCount = 15;
            DateTime? start = mode.StartTime;
            DateTime? end = mode.EndTime;
            string startDateStr, endDateStr;
            (start, end, startDateStr, endDateStr) = await GetRangeDateAsync(start, end, mode);
            if (string.IsNullOrEmpty(mode.ViewByValue))
            {
                mode.ViewBy = DisplayColumn.Time;
            }
            List<RMAuditInfo> tableInfo = new List<RMAuditInfo>();
            if (mode.FilterInfos != null || mode.IsAscending != null)
            {//get table for sort and filter
                tableInfo = AuditService.FindAuditInfoBySortFilter(mode.PageIndex, mode.PageSize, ref pageCount, (DateTime)start, (DateTime)end, mode.IsAscending, mode.SortBy, mode.FilterInfos, mode.ViewBy, mode.ViewByValue);
            }
            else
            {
                tableInfo = AuditService.FindAuditInfoByTimeInterval(mode.PageIndex, mode.PageSize, ref pageCount, (DateTime)start, (DateTime)end, mode.ViewBy, mode.ViewByValue);
            }
            AuditTable table = new AuditTable();
            if (tableInfo.Count > 0)
            {
                GeneralSettingModel gls = await GeneralService.GetGeneralSettingAsync();
                table.TableInfo = tableInfo.ConvertAll<AuditTableInfo>((m) => {
                    return new AuditTableInfo
                    {
                        Item = m,
                        ActionStr = m.Action.ToDescription(),
                        CategoryStr = m.Category.ToDescription(),
                        ModuleStr = m.Module.ToDescription(),
                        StatusStr = ((AuditStatus)m.Status).ToDescription(),
                        DateStr = GeneralService.ConvertTiksToDateTime(gls, m.ExecuteOn.Ticks, true).SimplifyFormatTime
                    };
                });
                //table.PageCount = pageCount % mode.PageSize > 0 ? (pageCount / mode.PageSize) + 1 : pageCount / mode.PageSize;
                table.PageCount = pageCount;
                table.PageIndex = mode.PageIndex;
            }
            else if (tableInfo.Count == 0)
            {
                //table.PageCount = pageCount % mode.PageSize == 0 ? 1 : (pageCount / mode.PageSize) + 1;
                table.PageCount = pageCount;
                table.PageIndex = mode.PageIndex;
            }
            return table;
        }
        [HttpPost]
        public async Task<AuditTable> GetTableInfoByFilterAndSort([FromBody]AuditPageMode mode)
        {
            int pageCount = 15;
            DateTime? start = mode.StartTime;
            DateTime? end = mode.EndTime;
            string startDateStr, endDateStr;
            (start, end, startDateStr, endDateStr) = await GetRangeDateAsync(start, end, mode);
            if (string.IsNullOrEmpty(mode.ViewByValue))
            {
                mode.ViewBy = DisplayColumn.Time;
            }
            List<RMAuditInfo> tableInfo = AuditService.FindAuditInfoBySortFilter(mode.PageIndex, mode.PageSize, ref pageCount, (DateTime)start, (DateTime)end, mode.IsAscending, mode.SortBy, mode.FilterInfos, mode.ViewBy, mode.ViewByValue);
            AuditTable table = new AuditTable();
            if (tableInfo.Count > 0)
            {
                GeneralSettingModel gls = await GeneralService.GetGeneralSettingAsync();
                table.TableInfo = tableInfo.ConvertAll<AuditTableInfo>((m) =>
                {
                    return new AuditTableInfo
                    {
                        Item = m,
                        ActionStr = m.Action.ToDescription(),
                        CategoryStr = m.Category.ToDescription(),
                        ModuleStr = m.Module.ToDescription(),
                        StatusStr = ((AuditStatus)m.Status).ToDescription(),
                        DateStr = GeneralService.ConvertTiksToDateTime(gls, m.ExecuteOn.Ticks, true).SimplifyFormatTime
                    };
                });
                table.PageCount = pageCount % mode.PageSize > 0 ? (pageCount / mode.PageSize) + 1 : pageCount / mode.PageSize;
                table.PageIndex = mode.PageIndex;
            }
            return table;
        }
        /// <summary>
        /// 获取图表数据
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="ViewBy"></param>
        /// <returns></returns>
        [RACodeReview("Allen Yin", comment: "没有删除临时文件")]
        [HttpPost]
        public async Task<AuditChartInfo> GetChartInfoAsync([FromBody]AuditPageMode mode)
        {
            DateTime? start = mode.StartTime;
            DateTime? end = mode.EndTime;
            string startDateStr, endDateStr;
            (start,end, startDateStr,endDateStr) = await GetRangeDateAsync(start, end, mode);
            List<AuditChart> resultList = await GetChartInfosAsync((DateTime)start, (DateTime)end, mode.ViewBy);
            string DateTimeFormat = await GeneralService.GetDateFormatAsync();
            if (mode.ViewBy != DisplayColumn.Time)
            {
                resultList = resultList.OrderByDescending(x => x.LabelStr).ToList();
            }
            //此处处理的目的：补全list中缺少的日期项。
            if (mode.ViewBy == DisplayColumn.Time)
            {
                //该处是要处理chart的纵轴，需要把时间转换成Local的进行处理
                var globalTimeZoneId = (await GeneralService.GetGeneralSettingAsync()).TimeZoneId;
                TimeZoneInfo cstZone = GeneralSettingConfig.FindSystemTimeZoneById(globalTimeZoneId);
                start = start.Value + cstZone.GetUtcOffset(DateTime.SpecifyKind(start.Value, DateTimeKind.Utc));
                end = end.Value + cstZone.GetUtcOffset(DateTime.SpecifyKind(end.Value, DateTimeKind.Utc));

                var totalDays = (end.Value - start.Value).Days;
                Dictionary<string, AuditChart> tempDic = new Dictionary<string, AuditChart>(totalDays);
                for (DateTime s = start.Value; DateTime.Compare(s, end.Value) <= 0; s = s.AddDays(1))
                {
                    tempDic[s.ToString(DateTimeFormat)] = new AuditChart()
                    {
                        LabelStr = s.ToString(DateTimeFormat),
                        valueCount = 0,
                        dateOfWeek = s.DayOfWeek,
                        year = s.Year,
                        day = s.Day,
                        month = s.Month
                    };
                }

                resultList.ForEach((x) =>
                {
                    tempDic[x.LabelStr] = x;
                });

                resultList = tempDic.Values.ToList();
            }
            AuditChartInfo datas = new AuditChartInfo
            {
                Start = startDateStr,
                End = endDateStr,
                ChartDatas = resultList
            };
            return datas;
        }

        [RACodeReview("Allen Yin")]
        [HttpPost]
        //[Microsoft.AspNetCore.Mvc.TypeFilter(typeof(ValidateAntiForgeryTokenFilterAttribute))]
        //[FileDownloadFilter]
        public async Task<IActionResult> DownLoadReport()
        {
            var datetimeRangeJSON = Request.Form["datetimeRange"].ToString();
            AuditExportMode mode = JsonConvert.DeserializeObject<AuditExportMode>(datetimeRangeJSON);

            DateTime? start = mode.StartTime;
            DateTime? end = mode.EndTime;
            string startDateStr, endDateStr;
            (start, end, startDateStr, endDateStr) = await GetRangeDateAsync(start, end, new AuditPageMode() { Range = mode.Range, StartTime = mode.StartTime, EndTime = mode.EndTime });

            string nowTimeStr = (await GeneralService.ConvertTiksToDateTimeAsync(DateTime.UtcNow.Ticks, false)).DataTime.ToString(AveDateTimeUtility.DATETYPE022);
            HttpResponseMessage response = new HttpResponseMessage();
            //string fileName = string.Format("AvePoint {0} Auditor Report_", WebUtil.ROOTFOLDER) + nowTimeStr;
            string fileName = $"{I18NEntity.GetString("RM_RC_Audit_PageTitle")}_{nowTimeStr}";
            //string fileName = $"{I18NEntity.GetString("RM_RC_Audit_PageTitle")}_{I18NEntity.GetString("RM_Report_SectionTitle_Settings")}_from_{startDateStr}_to_{endDateStr}_{nowTimeStr}";//TODO
            string folderPath = JobReportUtility.GetDownloadRuleUsageReportTempleFolder("Auditor") + Path.DirectorySeparatorChar + fileName + Guid.NewGuid();
            await AuditService.GenerateReportForAuditReportAsync(folderPath, fileName, start.Value, end.Value);
            ZipUtil.ZipFolder(folderPath, folderPath + ".zip", Encoding.UTF8); //Quality Issue
            var memoryStream = new MemoryStream();
            using (var stream = new FileStream(folderPath + ".zip", FileMode.Open, FileAccess.Read))
            {
                stream.CopyTo(memoryStream);
            }
            memoryStream.Position = 0;
            return File(memoryStream, GetContentType(folderPath + ".zip"), fileName + ".zip");
        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        [RACodeReview("Allen Yin")]
        [NonAction]
        private async Task<List<AuditChart>> GetChartInfosAsync(DateTime start, DateTime end, DisplayColumn ViewBy)
        {
            switch (ViewBy)
            {
                case DisplayColumn.Time:
                    string DateTimeFormat = await GeneralService.GetDateFormatAsync();
                    Dictionary<DateTime, int> timeInfo = await AuditService.FindAuditInfoByTimeIntervalAndGroupByTimeAsync(start, end);
                    var infoList = timeInfo.OrderBy(m => m.Key).ToList().ConvertAll<AuditChart>(
                                                       (x) => (new AuditChart
                                                       {
                                                           LabelStr = x.Key.ToString(DateTimeFormat),
                                                           LabelVal = x.Key.ToString(DateTimeFormat),
                                                           day = x.Key.Day,
                                                           month = x.Key.Month,
                                                           year = x.Key.Year,
                                                           valueCount = x.Value,
                                                           dateOfWeek = x.Key.DayOfWeek
                                                       }));
                    return infoList;

                case DisplayColumn.Action:
                    Dictionary<int, int> actionInfo = AuditService.FindAuditInfoByTimeIntervalAndGroupByAction(start, (DateTime)end);
                    return actionInfo.ToList().ConvertAll<AuditChart>(
                                                       (x) => (new AuditChart
                                                       {
                                                           LabelStr = ((AuditAction)x.Key).ToDescription(),
                                                           LabelVal = x.Key,
                                                           valueCount = x.Value,
                                                       }));

                case DisplayColumn.DocAveModule:
                    Dictionary<int, int> moduleInfo = AuditService.FindAuditInfoByTimeIntervalAndGroupByModule(start, end);
                    return moduleInfo.ToList().ConvertAll<AuditChart>(
                                                         (x) => (new AuditChart
                                                         {
                                                             LabelStr = ((AuditCategory)x.Key).ToDescription(),
                                                             LabelVal = x.Key,
                                                             valueCount = x.Value,
                                                         }));
                case DisplayColumn.Object:
                    Dictionary<string, int> objectInfo = AuditService.FindAuditInfoByTimeIntervalAndGroupByObject(start, end);
                    return objectInfo.ToList().ConvertAll<AuditChart>(
                                                        (x) => (new AuditChart
                                                        {
                                                            LabelStr = x.Key.ToString(),
                                                            LabelVal = x.Key,
                                                            valueCount = x.Value,
                                                        }));
                case DisplayColumn.Role:
                    Dictionary<string, int> roleInfo = AuditService.FindAuditInfoByTimeIntervalAndGroupByRole(start, end);
                    return roleInfo.ToList().ConvertAll<AuditChart>(
                                                        (x) => (new AuditChart
                                                        {
                                                            LabelStr = x.Key.ToString(),
                                                            LabelVal = x.Key,
                                                            valueCount = x.Value,
                                                        }));
                case DisplayColumn.Status:
                    Dictionary<int, int> statusInfo = AuditService.FindAuditInfoByTimeIntervalAndGroupByStatus(start, end);
                    return statusInfo.ToList().ConvertAll<AuditChart>(
                                                         (x) => (new AuditChart
                                                         {
                                                             LabelStr = ((AuditStatus)x.Key).ToDescription(),
                                                             LabelVal = x.Key,
                                                             valueCount = x.Value,
                                                         }));
                case DisplayColumn.User:
                    Dictionary<string, int> userInfo = AuditService.FindAuditInfoByTimeIntervalAndGroupByUser(start, end);
                    DeleteRepeatRunScheduleForUserInfo(userInfo);
                    return userInfo.ToList().ConvertAll<AuditChart>(
                                                        (x) => (new AuditChart
                                                        {
                                                            LabelStr = I18NEntity.GetString(x.Key.ToString()),
                                                            LabelVal = I18NEntity.GetString(x.Key),
                                                            valueCount = x.Value,
                                                        }));
            }
            return new List<AuditChart>();
        }

        private void DeleteRepeatRunScheduleForUserInfo(Dictionary<string, int> userInfo)
        {
            if (userInfo.IsNullOrEmpty())
            {
                return;
            }
            if (userInfo.ContainsKey(I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("en-US"))))
            {
                if (!userInfo.ContainsKey("RM_TS_RunSchedule"))
                {
                    userInfo.Add("RM_TS_RunSchedule", userInfo[I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("en-US"))]);
                }
                else
                {
                    userInfo["RM_TS_RunSchedule"] = userInfo["RM_TS_RunSchedule"] + userInfo[I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("en-US"))];
                }
                userInfo.Remove(I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("en-US")));
            }
            if (userInfo.ContainsKey(I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("ja-JP"))))
            {
                if (!userInfo.ContainsKey("RM_TS_RunSchedule"))
                {
                    userInfo.Add("RM_TS_RunSchedule", userInfo[I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("ja-JP"))]);
                }
                else
                {
                    userInfo["RM_TS_RunSchedule"] = userInfo["RM_TS_RunSchedule"] + userInfo[I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("ja-JP"))];
                }
                userInfo.Remove(I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("ja-JP")));
            }
            if (userInfo.ContainsKey(I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("ko-KR"))))
            {
                if (!userInfo.ContainsKey("RM_TS_RunSchedule"))
                {
                    userInfo.Add("RM_TS_RunSchedule", userInfo[I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("ko-KR"))]);
                }
                else
                {
                    userInfo["RM_TS_RunSchedule"] = userInfo["RM_TS_RunSchedule"] + userInfo[I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("ko-KR"))];
                }
                userInfo.Remove(I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("ko-KR")));
            }
            if (userInfo.ContainsKey(I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("fr-FR"))))
            {
                if (!userInfo.ContainsKey("RM_TS_RunSchedule"))
                {
                    userInfo.Add("RM_TS_RunSchedule", userInfo[I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("fr-FR"))]);
                }
                else
                {
                    userInfo["RM_TS_RunSchedule"] = userInfo["RM_TS_RunSchedule"] + userInfo[I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("fr-FR"))];
                }
                userInfo.Remove(I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("fr-FR")));
            }
            if (userInfo.ContainsKey(I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("fr-CA"))))
            {
                if (!userInfo.ContainsKey("RM_TS_RunSchedule"))
                {
                    userInfo.Add("RM_TS_RunSchedule", userInfo[I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("fr-CA"))]);
                }
                else
                {
                    userInfo["RM_TS_RunSchedule"] = userInfo["RM_TS_RunSchedule"] + userInfo[I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("fr-CA"))];
                }
                userInfo.Remove(I18NEntity.GetString("RM_TS_RunSchedule", CultureInfo.CreateSpecificCulture("fr-CA")));
            }
        }
        /// <summary>
        /// 根据Audit页面传递参数，获取时间范围,
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="rangeMode"></param>
        [RACodeReview("Allen Yin")]
        [NonAction]
        private async Task<(DateTime?, DateTime?, string, string)> GetRangeDateAsync(DateTime? start, DateTime? end,AuditPageMode mode)
        {
            string startDateStr;
            string endDateStr;

            var dateFormat = await GeneralService.GetDateFormatAsync();
            var globalTimeZoneId = (await GeneralService.GetGeneralSettingAsync()).TimeZoneId;
            TimeZoneInfo cstZone = GeneralSettingConfig.FindSystemTimeZoneById(globalTimeZoneId);

            //点击chart中的时间纵轴
            if (mode.ViewBy == DisplayColumn.Time && !string.IsNullOrEmpty(mode.ViewByValue))
            {
                //时间范围
                if (mode.ViewByValue.Contains('~'))
                {
                    //start = DateTime.Parse(mode.ViewByValue.Split('~')[0]);
                    start = DateTime.ParseExact(mode.ViewByValue.Split('~')[0].Trim(' '), dateFormat, System.Threading.Thread.CurrentThread.CurrentCulture);
                    start = new DateTime(start.Value.Year, start.Value.Month, start.Value.Day, 0, 0, 0);
                    //end = DateTime.Parse(mode.ViewByValue.Split('~')[1]);
                    end = DateTime.ParseExact(mode.ViewByValue.Split('~')[1].Trim(' '), dateFormat, System.Threading.Thread.CurrentThread.CurrentCulture);
                    end = new DateTime(end.Value.Year, end.Value.Month, end.Value.Day, 23, 59, 59);
                }
                else
                {
                    //取一天
                    start = DateTime.ParseExact(mode.ViewByValue, dateFormat, System.Threading.Thread.CurrentThread.CurrentCulture);
                    //start = DateTime.Parse(mode.ViewByValue);
                    start = new DateTime(start.Value.Year, start.Value.Month, start.Value.Day, 0, 0, 0);
                    end = start.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
            }
            else
            {
                //对于One_Month 这种range,时间范围从月初开始. e.g 当前3月13日，onemonth是3月1日 to now
                DateTime now = DateTime.UtcNow;
                //获取当前的日期，需要转换成Local的
                now = now + cstZone.GetUtcOffset(now);
                DateTime tmp = new DateTime();
                if (mode.Range != DateRange.Custom)
                {
                    end = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
                    tmp = new DateTime(end.Value.Year, end.Value.Month, 1, 0, 0, 0);
                }
                switch (mode.Range)
                {
                    case DateRange.Current_Week:
                        //本周  每一周的第一天为周一
                        int addDaysTemp = (int)end.Value.DayOfWeek == 0 ? -6 : -(int)end.Value.DayOfWeek + 1;
                        start = end.Value.AddDays(addDaysTemp).AddHours(-23).AddMinutes(-59).AddSeconds(-59);
                        break;
                    case DateRange.Current_Month:
                        start = tmp;
                        break;
                    case DateRange.Three_Month:
                        start = tmp.AddMonths(-2);
                        break;
                    case DateRange.Six_Month:
                        start = tmp.AddMonths(-5);
                        break;
                    case DateRange.Custom:
                        start = new DateTime(start.Value.Year, start.Value.Month, start.Value.Day, 0, 0, 0);
                        end = new DateTime(end.Value.Year, end.Value.Month, end.Value.Day, 23, 59, 59);
                        break;
                    default:
                        start = end.Value.AddDays(-5).AddHours(-23).AddMinutes(-59).AddSeconds(-59);
                        break;
                }
            }
            startDateStr = start.Value.ToString(dateFormat);
            endDateStr = end.Value.ToString(dateFormat);
            //由于下一步需要查询，需要将时间转换会UTC的时间。
            start = start.Value - cstZone.GetUtcOffset(start.Value);
            end = end.Value - cstZone.GetUtcOffset(end.Value);

            return (start,end,startDateStr, endDateStr);
        }

        [HttpPost]
        public async Task<FilterSource> GetFiltersSource([FromBody]AuditPageMode mode)
        {
            FilterSource filter = new FilterSource();
            DateTime? start = mode.StartTime;
            DateTime? end = mode.EndTime;
            string startDateStr, endDateStr;
            (start,end,startDateStr,endDateStr) =await GetRangeDateAsync(start, end, mode);
            filter.ActionItems = AuditService.GetActionItemsSource();
            filter.ModuleItems = AuditService.GetModuleItemsSource();
            filter.UserItems = AuditService.GetUserItemsSource();
            filter.StatusItems = AuditService.GetStatusItemsSource();
            return filter;
        }
    }
}