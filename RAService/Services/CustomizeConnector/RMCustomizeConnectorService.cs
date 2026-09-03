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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.CustomizeConnector;
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using AvePoint.RA.Contract.CustomizeConnector.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.Service.Services.CustomizeConnector.Audit;
using AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager;
using AvePoint.RA.Service.Services.CustomizeConnector.ColumnManager.BuildIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CustomizeConnector
{
    [AsyncAudit]
    public class RMCustomizeConnectorService : RMServiceBase, IRMCustomizeConnectorService
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMCustomizeConnectorService));

        private static IRMCustomizeConnectorContentSourceDao CustomizeConnectorContentSourceDao => PlatformWindsorManager.GetService<IRMCustomizeConnectorContentSourceDao>();
        private static IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        [AsyncAudit(Module = AuditModule.CustomizeConnector, Category = AuditCategory.CustomizeConnector, Action = AuditAction.CustomizeConnectorCreate, IAsyncBeforeHandler = typeof(CustomizeConnectorAuditBeforeHandler))]
        public async System.Threading.Tasks.Task<CustomizeConnectorActionResult> AddAsync(CustomizeConnectorInfo info)
        {
            try
            {
                var internalNameGenerator = new ColumnInternalNameGenerator();
                internalNameGenerator.Generate(info.ColumnInfoes);

                var contentSourceInfo = CustomizeConnectorConvertor.Convert(info);
                await CustomizeConnectorContentSourceDao.Add(contentSourceInfo);
                await CreateConnectorTimerScheduleAsync();
                return CustomizeConnectorActionResult.Succeed();
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while Add connector info. Error: {e}");
                return CustomizeConnectorActionResult.Failed();
            }
        }

        private async Task<string> CreateConnectorTimerScheduleAsync()
        {
            List<ScheduleInfo> infos = await ScheduleService.GetScheduleByTypeServiceAsync(ScheduleType.ConnectorExplorerTimer);
            ScheduleInfo oldSchedule = null;
            if (infos != null && infos.Count > 0)
            {
                oldSchedule = infos[0];
            }
            var generalSetting = GeneralSettingService.GetGeneralSettingAsync();
            if (oldSchedule == null || oldSchedule.TimeZoneId != (await generalSetting).TimeZoneId)
            {
                if (oldSchedule != null)
                {
                    ScheduleService.DeleteScheduleByType(ScheduleType.ConnectorExplorerTimer);
                }
                var info = new ScheduleInfo
                {
                    Id = Guid.NewGuid().ToString()
                };

                var utcNow = DateTime.UtcNow;
                var globalTimeZoneId = (await generalSetting).TimeZoneId;
                TimeZoneInfo localZone = GeneralSettingConfig.FindSystemTimeZoneById(globalTimeZoneId);
                var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, localZone);
                localNow = localNow.AddDays(1);

                var startTime = new DateTime(localNow.Year, localNow.Month, localNow.Day, 0, 0, 0);
                info.StartTime = startTime.ToString();
                info.EndTime = startTime.ToString();
                info.EndType = 0;
                info.Interval = 1;
                info.IntervalType = IntervalType.Daily;
                info.JobCategory = ScheduleType.ConnectorExplorerTimer;
                info.OccurrencesTotal = 1;
                info.TimeZoneId = (await generalSetting).TimeZoneId;
                await ScheduleService.CreateScheduleServiceAsync(info);
            }
            return string.Empty;
        }

        [AsyncAudit(Module = AuditModule.CustomizeConnector, Category = AuditCategory.CustomizeConnector, Action = AuditAction.CustomizeConnectorDelete, IAsyncBeforeHandler = typeof(CustomizeConnectorAuditBeforeHandler))]
        public async System.Threading.Tasks.Task DeleteAsync(List<Guid> ids)
        {
            try
            {
                await CustomizeConnectorContentSourceDao.Delete(ids);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while delete connectors. Error: {e}");
            }
        }

        public async System.Threading.Tasks.Task<CustomizeConnectorInfo> GetAsync(Guid id)
        {
            try
            {
                var contentSourceInfo = await CustomizeConnectorContentSourceDao.Get(id);
                return CustomizeConnectorConvertor.Convert(contentSourceInfo);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get connector info. Error: {e}");
                return default;
            }
        }

        public async System.Threading.Tasks.Task<IEnumerable<CustomizeConnectorInfo>> GetAllAsync()
        {
            try
            {
                var contentSourceInfoes = await CustomizeConnectorContentSourceDao.GetAllSimpleInfoes(CustomizeConnectorOrigin.ExternalCustomize);
                return contentSourceInfoes.ToList().ConvertAll(CustomizeConnectorConvertor.Convert);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get connector infoes. Error: {e}");
                return default;
            }
        }

        [AsyncAudit(Module = AuditModule.CustomizeConnector, Category = AuditCategory.CustomizeConnector, Action = AuditAction.CustomizeConnectorEdit, IAsyncBeforeHandler = typeof(CustomizeConnectorAuditBeforeHandler))]
        public async System.Threading.Tasks.Task<CustomizeConnectorActionResult> UpdateAsync(CustomizeConnectorInfo info)
        {
            try
            {
                var internalNameGenerator = new ColumnInternalNameGenerator();
                internalNameGenerator.Generate(info.ColumnInfoes);

                var contentSourceInfo = CustomizeConnectorConvertor.Convert(info);
                await CustomizeConnectorContentSourceDao.Update(contentSourceInfo);

                return CustomizeConnectorActionResult.Succeed();
            }   
            catch(Exception e)
            {
                Logger.Error($"An error occurred while update connector: [{info.Id}]. Error: {e}");
                return CustomizeConnectorActionResult.Failed();
            }
        }

        public async Task<List<CustomizeConnectorNameValue<string>>> ViewItemDetailForExplorerSearchAsync(Guid id)
        {
            var res = new List<CustomizeConnectorNameValue<string>>();
            try
            {
                var explorerDao = new ExplorerDao();
                var record = explorerDao.GetFirstOrDefault(item => item.Id == id);
                if(record == null)
                {
                    Logger.Warn($"Can't find customize connector record by id [{id}].");
                    return res;
                }

                var customizeColumnValue = record.CustomColumnDic;
                var connectorInfo = await GetAsync(new Guid(record.ContainerId));
                var columnManager = new ConnectorColumnManager(connectorInfo.ColumnInfoes);
                foreach(var columnInfo in connectorInfo.ColumnInfoes.OrderBy(item => item.Order))
                {
                    if(columnInfo.Id == CustomizeConnectorBuildColumnIds.RowKey)
                    {
                        continue;
                    }
                    if(columnInfo.Origin == CustomizeConnectorOrigin.BuildIn)
                    {
                        var nameValue = await ConnectorBuildInColumnManager.ConvertToNameValueAsync(columnInfo, record);
                        res.Add(nameValue);
                    }
                    else
                    {
                        var nameValue = await columnManager.ConvertToNameValueAsync(columnInfo, customizeColumnValue);
                        res.Add(nameValue);
                    }
                }

                return res;
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while view item [{id}] detail for explorer search. Error: {e}");
                return new List<CustomizeConnectorNameValue<string>>();
            }
        }

        public async Task<(string, string)> GenerateJsonSchemeAsync(Guid id)
        {
            try
            {
                var contentSourceInfo = await CustomizeConnectorContentSourceDao.Get(id);
                return (contentSourceInfo.Name, TemplateSchemeGenerator.GenerateJson(contentSourceInfo));
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while generate json scheme by connector: [{id}]. Error: {e}");
                return (null, null);
            }
        }

        public async Task<CustomizeConnectorInfo> GetSimpleInfoByNameAsync(string name)
        {
            try
            {
                var contentSourceInfo = await CustomizeConnectorContentSourceDao.GetSimpleInfoByName(name);
                return CustomizeConnectorConvertor.Convert(contentSourceInfo);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get connector info by name [{name}]. Error: {e}");
                return new CustomizeConnectorInfo
                {
                    Name = name
                };
            }
        }
    }
}
