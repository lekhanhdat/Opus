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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.JPMC;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Myhub.Items.Actions;
using AvePoint.RA.Contract.Myhub.Model.QueryRequest.Actions;
using AvePoint.RA.Contract.MyHub.Model.FIlter.Types;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Service.Audit.JPMC;
using AvePoint.RA.Service.Services.MyHub.NewMethods;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static AvePoint.RA.Contract.Audit.JPMC.FSAuditQueryParam;

namespace AvePoint.RA.Service.Services.MyHub.Actions
{
    public class RMMyhubAuditTrialMethod
    {
        private static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private RMMyhubQueryRecordsMethod _recordStore;
        private RMMyhubQueryRecordsMethod RecordStore => _recordStore ??= new RMMyhubQueryRecordsMethod();
        private IFSAuditSinkService AuditSinkService => PlatformWindsorManager.GetService<IFSAuditSinkService>();
        private IGeneralSettingService _GeneralSettingService;
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService(ref _GeneralSettingService);
        private async Task<FSAuditQueryResult> QueryAuditTrailJPMCAsync(FSAuditQueryParam fsQueryDto, string timeZoneId, bool isDaylight, string timeFormat)
        {
            if (fsQueryDto.Filters == null)
            {
                fsQueryDto.Filters = new List<FSAuditQueryFilter>();
            }
            foreach(var filter in fsQueryDto.Filters)
            {
                if(filter.ColumnName == "ExecutedTime")
                {
                    filter.MyhubTimeZoneId = timeZoneId;
                }
            }
            if (!string.IsNullOrWhiteSpace(fsQueryDto.SearchKey))
            {
                fsQueryDto.Filters.Add(new FSAuditQueryFilter
                {
                    ColumnName = nameof(RMFSAudit.ObjectName),
                    ColumnValues = new List<string> { fsQueryDto.SearchKey },
                });
            }
            if (fsQueryDto.Order == null || string.IsNullOrWhiteSpace(fsQueryDto.Order.ColumnName))
            {
                fsQueryDto.Order = new FSAuditQueryOrder
                {
                    ColumnName = "ActionTimeUtc",
                    IsDesc = true
                };
            }
            int skip = (fsQueryDto.PageIndex - 1) * fsQueryDto.PageSize;
            int take = fsQueryDto.PageSize;
            var (items, totalCount) = await AuditSinkService.QueryAsync(fsQueryDto.Filters, skip, take, fsQueryDto.Order);

            GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
            foreach (var item in items)
            {
                item.FormattedTime = item.ActionTimeUtc > 0 ? (string.IsNullOrEmpty(timeZoneId)
                        ? GeneralSettingService.ConvertTiksToDateTime(gls, item.ActionTimeUtc, true).SimplifyFormatTime
                        : GeneralSettingService.ConvertTiksToDateTime(gls, item.ActionTimeUtc, true, Convert.ToInt32(timeZoneId), isDaylight, timeFormat).SimplifyFormatTime)
                        : null;
            }
            var currentCount = skip + items.Count;
            return new FSAuditQueryResult
            {
                Items = items,
                TotalCount = totalCount,
                HasMore = totalCount > currentCount
            };
        }
        public async Task<FSAuditQueryResult> QueryAuditTrailAsync(RMMyhubAuditTrialQueryInfo queryInfo, string timeFormat)
        {
            try
            {
                var result = await QueryAuditTrailJPMCAsync(queryInfo.QueryParam, queryInfo.TimeZoneId, queryInfo.IsDaylight, timeFormat);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to query audit trail", ex);
                return new FSAuditQueryResult();
            }

        }
        public RMMyhubAuditTrialFilterItem QueryAuditTrialFilter()
        {
            return new RMMyhubAuditTrialFilterItem
            {
                UserItems = AuditSinkService.FetchAllAuditUsers(),
                ActionItems = AuditSinkService.FetchAllAuditTypes()
            };
        }

        public async Task<Record> QueryForBeforeApplyClassCodeAudit(Guid nodeId)
        {
            var sql = @"SELECT * FROM c
        WHERE c.sourceFlag = @sourceFlag
        AND c.nodeId=@nodeId";
            var sqlParamater = new List<SqlParameter>
            {
                new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
                new SqlParameter("@nodeId",nodeId)
            };
            var result = await RecordStore.QuerySingleAsync<Record>(sql, sqlParamater);
            return result;
        }
    }
}