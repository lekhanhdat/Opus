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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.ExtendedProperties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AvePoint.RA.Service.Services.RMFileSystemSettings
{
    public abstract class FSBaseFilterBuilder<TEntity, TFilter> where TEntity : BaseModel
    {
        private IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        
        private TimeZoneInfo _timeZone;

        protected readonly List<TFilter> _filters;

        protected readonly ParameterExpression _param;

        protected FSBaseFilterBuilder(List<TFilter> filters)
        {
            _filters = filters;
            _param = Expression.Parameter(typeof(TEntity), "E");
            var setting = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(setting.TimeZoneId);
        }

        public Expression<Func<TEntity, bool>> Build()
        {
            if (_filters == null || !_filters.Any()) return x => true;

            var expressions = _filters.Select(BuildExpression).Where(e => e != null).ToList();

            var body = expressions.Any() ? expressions.Aggregate(Expression.AndAlso) : Expression.Constant(true);

            return Expression.Lambda<Func<TEntity, bool>>(body, _param);
        }

        protected abstract Expression BuildExpression(TFilter filter);

        protected abstract string GetColumnName(TFilter filter);

        #region Operations
        protected Expression Equals<TValue>(string column, TValue value) => Expression4DynamicQuery.GetEqualExpression(typeof(TEntity), _param, column, value);

        protected Expression In<TValue>(string column, IEnumerable<TValue> values) => Expression4DynamicQuery.GetInExpression(typeof(TEntity), _param, column, values.Cast<object>());

        protected Expression Contains<TValue>(string column, TValue value) => Expression4DynamicQuery.GetContainsExpression(typeof(TEntity), _param, column, value);

        protected Expression ContainsIgnoreCase<TValue>(string column, TValue value) => Expression4DynamicQuery.GetContainsExpressionIgnoreCase(typeof(TEntity), _param, column, value);

        protected Expression Between<TValue>(string column, TValue start, TValue end)
        {
            var gte = Expression4DynamicQuery.GetGreaterThanOrEqualExpression(typeof(TEntity), _param, column, start);
            var lte = Expression4DynamicQuery.GetLessThanOrEqualExpression(typeof(TEntity), _param, column, end);
            return Expression.AndAlso(gte, lte);
        }
        #endregion

        #region Helpers
        protected Expression BuildTimeRangeExpression(string column, List<string> values, string timeZoneId = null)
        {
            var timeFrame = JsonConvert.DeserializeObject<ManualApprovalTimeFrame>(values.First());
            var (startUtc, endUtc) = string.IsNullOrEmpty(timeZoneId) ?ConvertToUtcTicks(timeFrame): ConvertToUtcTicks(timeFrame,timeZoneId);
            return Between(column, startUtc, endUtc);
        }
        private (long startUtc, long endUtc) ConvertToUtcTicks(ManualApprovalTimeFrame timeFrame)
        {
            var endOfDay = timeFrame.EndTime.Date.AddDays(1).AddTicks(-1);
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(timeFrame.StartTime, DateTimeKind.Unspecified), _timeZone).Ticks;
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(endOfDay, DateTimeKind.Unspecified), _timeZone).Ticks;
            return (startUtc, endUtc);
        }
        private (long startUtc, long endUtc) ConvertToUtcTicks(ManualApprovalTimeFrame timeFrame, string timeZoneId)
        {
            var startDate = GeneralSettingService.ConvertDateTimeToUtcAsync(DateTime.SpecifyKind(timeFrame.StartTime, DateTimeKind.Unspecified), timeZoneId).GetAwaiter().GetResult();
            var endDate = GeneralSettingService.ConvertDateTimeToUtcAsync(DateTime.SpecifyKind(timeFrame.EndTime.Date.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified), timeZoneId).GetAwaiter().GetResult();
            var startUtc = startDate.Ticks;
            var endUtc = endDate.Ticks;
            return (startUtc, endUtc);
        }
        #endregion
    }
}
