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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.MyHub.Model.FIlter;
using AvePoint.RA.Contract.MyHub.Model.QueryRequest.Views;
using AvePoint.RA.Contract.MyHub.Model.Sort;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvePoint.RA.Contract.MyHub.Model.FIlter.Types.RMMyhubEndDateFilter;

namespace AvePoint.RA.Service.Services.MyHub.NewMethods
{
    public class RMMyhubFolderAndItemMethod
    {
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        public async Task<(string sql, List<SqlParameter> parameter)> BuildQuery(RMMyhubFolderItemQueryInfo queryInfo)
        {
            var sql = BaseSql();
            var sqlParameters = BaseSqlParameters(queryInfo.PartitionKeyId);

            (sql, sqlParameters) = await BuildFilterConditions(queryInfo, sql, sqlParameters);

            if (!string.IsNullOrWhiteSpace(queryInfo.SortBy))
            {
                var querySort = RMMyhubFolderItemSortInfo.GetFolderItemSortColumn(queryInfo.SortBy);
                sql += " ORDER BY " + querySort + (queryInfo.IsDesc ? " DESC" : " ASC");
            }
            else
            {
                sql += " ORDER BY c.leafName ASC";
            }

            return (sql, sqlParameters);
        }

        public async Task<(string sql, List<SqlParameter> parameter)> BuildCountQuery(RMMyhubFolderItemQueryInfo queryInfo)
        {
            var sql = TotalCountSql();
            var sqlParameters = BaseSqlParameters(queryInfo.PartitionKeyId);

            return await BuildFilterConditions(queryInfo, sql, sqlParameters);

        }
        private async Task<(string sql, List<SqlParameter> parameter)> BuildFilterConditions(
            RMMyhubFolderItemQueryInfo queryInfo,
            string sql,
            List<SqlParameter> sqlParameters)
        {
            if (queryInfo.ParentId != Guid.Empty)
            {
                sql += $" AND c.parentId = @ParentId";
                AddParameter(sqlParameters, "@ParentId", queryInfo.ParentId);
            }

            if (!string.IsNullOrWhiteSpace(queryInfo.SearchValue))
            {
                var searchValue = queryInfo.SearchValue.ToLowerInvariant();
                sql += $" AND CONTAINS(LOWER(c.leafName), @SearchValue) ";
                AddParameter(sqlParameters, "@SearchValue", searchValue);
            }

            if (queryInfo.FilterInfoes != null)
            {
                foreach (var filterInfo in queryInfo.FilterInfoes)
                {
                    if (filterInfo.ColumnKey.Equals("enddate", StringComparison.OrdinalIgnoreCase))
                    {
                        var filterValue = JsonConvert.DeserializeObject<RMMyhubEndDateFilterValue>(filterInfo.ColumnValue);
                        var processedFilterValue = new RMMyhubEndDateFilterValue();
                        if (filterValue.Option == RMMyhubEndDateFilterOption.AnyTime)
                        {
                            continue;
                        }
                        switch (filterValue.Option)
                        {
                            case RMMyhubEndDateFilterOption.AnyTime:
                                break;
                            case RMMyhubEndDateFilterOption.WithIn:
                                var nowUtc = DateTime.UtcNow;
                                if (queryInfo.TimeZoneId != null)
                                {
                                    nowUtc = await GeneralSettingService.ConvertDateTimeToUtcAsync(nowUtc, queryInfo.TimeZoneId);
                                }
                                else
                                {
                                    nowUtc = await GeneralSettingService.ConvertDateTimeToUtcAsync(nowUtc);
                                }
                                processedFilterValue = new RMMyhubEndDateFilterValue
                                {
                                    Option = filterValue.Option,
                                    DateTimeNow = nowUtc,
                                    WithinOption = filterValue.WithinOption,
                                    WithinNumber = filterValue.WithinNumber
                                };
                                break;
                            case RMMyhubEndDateFilterOption.Between:
                                DateTime startTimeUtc;
                                DateTime endTimeUtc;
                                if (queryInfo.TimeZoneId != null)
                                {
                                    startTimeUtc = await GeneralSettingService.ConvertDateTimeToUtcAsync(filterValue.StartTime, queryInfo.TimeZoneId);
                                    endTimeUtc = await GeneralSettingService.ConvertDateTimeToUtcAsync(filterValue.EndTime.AddDays(1), queryInfo.TimeZoneId);
                                }
                                else
                                {
                                    startTimeUtc = await GeneralSettingService.ConvertDateTimeToUtcAsync(filterValue.StartTime);
                                    endTimeUtc = await GeneralSettingService.ConvertDateTimeToUtcAsync(filterValue.EndTime.AddDays(1));
                                }

                                processedFilterValue = new RMMyhubEndDateFilterValue
                                {
                                    Option = filterValue.Option,
                                    StartTime = startTimeUtc,
                                    EndTime = endTimeUtc,
                                };
                                break;
                        }
                        var endDateFilter = RMMyhubFilterHelper.GetFilter(filterInfo.ColumnKey);
                        var endDatesqlInfo = endDateFilter.GetSQL(JsonConvert.SerializeObject(processedFilterValue));
                        sql += $" {endDatesqlInfo.SQL}";
                        sqlParameters.AddRange(endDatesqlInfo.SQLParameters);

                        continue;
                    }

                    var filter = RMMyhubFilterHelper.GetFilter(filterInfo.ColumnKey);
                    var sqlInfo = filter.GetSQL(filterInfo.ColumnValue);
                    sql += $" {sqlInfo.SQL}";
                    sqlParameters.AddRange(sqlInfo.SQLParameters);
                }
            }

            return (sql, sqlParameters);
        }
        private static void AddParameter(List<SqlParameter> currentParameters, string parameterName, object value)
        {
            currentParameters.Add(new SqlParameter(parameterName, value));
        }
        private string BaseSql()
        {
            return @"SELECT VALUE {
    ""Id"": c.nodeId,
    ""NodeId"":c.nodeId,
    ""PartitionKeyId"": c.l2PartitionKey,
    ""Name"": c.leafName,
    ""Path"": c.dirPath,
    ""ClassCode"": c.classCode,
    ""CountryCode"": c.countryCode,
    ""FileVolume"": c.jpmcFileCount,
    ""Size"": c.jpmcFileSize,
    ""PendingDisposal"": c.manual_approvedStatus,
    ""RecordId"": c.recordsId,
    ""EndDate"": c.endTime,
    ""StartDate"": c.startDate,
    ""RetentionType"":c.retentionType,
    ""IsFolder"": c.nodeType = @nodeType,
    ""ExtentionForFile"": c.extensionForFile
} 
FROM c
WHERE c.sourceFlag = @sourceFlag
AND (c.nodeType = @nodeType OR c.nodeType = @itemNodeType)
AND c.recordStatus = @statuses
AND c.l2PartitionKey = @l2PartitionKey
AND IS_DEFINED(c.recordsId)
AND NOT IS_NULL(c.recordsId)";
        }
        private string TotalCountSql()
        {
            return @"SELECT VALUE COUNT(1) FROM c
WHERE c.sourceFlag = @sourceFlag
AND (c.nodeType = @nodeType OR c.nodeType = @itemNodeType)
AND c.recordStatus = @statuses
AND c.l2PartitionKey = @l2PartitionKey
AND IS_DEFINED(c.recordsId)
AND NOT IS_NULL(c.recordsId)";
        }
        private List<SqlParameter> BaseSqlParameters(string PartitionKeyId)
        {
            var sqlParameters = new List<SqlParameter>
            {
                new SqlParameter("@sourceFlag", (int)SourceFlag.FileSystem),
                new SqlParameter("@nodeType", (int)NodeLevel.FSFolder),
                new SqlParameter("@itemNodeType", (int)NodeLevel.FSFile),
                new SqlParameter("@statuses", (int)RMRecordStatus.Active),
                //l2PartitionKey作用于限定查询树范围
                new SqlParameter("@l2PartitionKey",PartitionKeyId.ToLowerInvariant())
            };
            return sqlParameters;
        }

    }
}
