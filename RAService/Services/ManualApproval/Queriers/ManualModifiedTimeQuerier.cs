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
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Explorer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace AvePoint.RA.Service.Services.ManualApproval.Queriers
{
    public class ManualModifiedTimeQuerier : IFilterWithHistory, ISorter
    {
        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        public ManualApprovalOrderOptions OrderOption => ManualApprovalOrderOptions.ManualModifiedTime;

        public ManualApprovalFilterOptions FilterOption => ManualApprovalFilterOptions.ManualModifiedTime;

        public async Task<Expression<Func<ManualApprovalRecord, bool>>> GetCosmosDBFilterExpressionAsync(string value)
        {
            var timeZoneId = (await GeneralSettingService.GetGeneralSettingAsync()).TimeZoneId;
            var timeZone = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId);

            var timeFrame = JsonConvert.DeserializeObject<ManualApprovalTimeFrame>(value);
            var endTime = new DateTime(timeFrame.EndTime.Year, timeFrame.EndTime.Month, timeFrame.EndTime.Day, 23, 59, 59);

            var startTimeTicks = TimeZoneInfo.ConvertTimeToUtc(timeFrame.StartTime, timeZone).Ticks;
            var endTimeTicks = TimeZoneInfo.ConvertTimeToUtc(endTime, timeZone).Ticks;
            return (record) => record.ManualModifiedTime >= startTimeTicks && record.ManualModifiedTime <= endTimeTicks;
        }

        public Expression<Func<ManualApprovalRecord, dynamic>> GetCosmosDBOrderExpression()
        {
            return (record) => record.ManualModifiedTime;
        }

        public async Task<ManualApprovalSqlDefintion> GetHistorySqlDefinitionAsync(string value)
        {
            var timeFrame = JsonConvert.DeserializeObject<ManualApprovalTimeFrame>(value);
            var timeZoneId = (await GeneralSettingService.GetGeneralSettingAsync()).TimeZoneId;
            var timeZone = GeneralSettingConfig.FindSystemTimeZoneById(timeZoneId);
            var startTimeTicks = TimeZoneInfo.ConvertTimeToUtc(timeFrame.StartTime, timeZone).Ticks;
            var endTimeTicks = TimeZoneInfo.ConvertTimeToUtc(timeFrame.EndTime, timeZone).Ticks;
            var sql = "(ModifiedTime >= @ModifiedTimeStart and ModifiedTime <= @ModifiedTimeEnd)";
            var result = new ManualApprovalSqlDefintion
            {
                Sql = sql,
            };
            result.Parameter.Add(new System.Data.SqlClient.SqlParameter("@ModifiedTimeStart", startTimeTicks));
            result.Parameter.Add(new System.Data.SqlClient.SqlParameter("@ModifiedTimeEnd", endTimeTicks));
            return result;
        }
    }
}
