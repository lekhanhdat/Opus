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
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvePoint.RA.Contract.MyHub.Model.FIlter.Types.RMMyhubCountryCodeFilter;

namespace AvePoint.RA.Contract.MyHub.Model.FIlter.Types
{
    public class RMMyhubEndDateFilter : IRMMyhubFilter
    {
        public string Name => "EndDate";

        public RMMyhubSQLInfo GetSQL(string valueJson)
        {

            var value = Newtonsoft.Json.JsonConvert.DeserializeObject<RMMyhubEndDateFilterValue>(valueJson);
            var sqlInfo = new RMMyhubSQLInfo();
            switch (value.Option)
            {
                case RMMyhubEndDateFilterOption.AnyTime:
                    return new();
                case RMMyhubEndDateFilterOption.WithIn:
                    var todayLocal = value.DateTimeNow;
                    DateTime adjustedLocalTime;
                    switch (value.WithinOption)
                    {
                        case RMMyhubEndDateWithinFilter.Days:
                            adjustedLocalTime = todayLocal.AddDays(value.WithinNumber);
                            break;
                        case RMMyhubEndDateWithinFilter.Weeks:
                            adjustedLocalTime = todayLocal.AddDays(value.WithinNumber * 7);
                            break;
                        case RMMyhubEndDateWithinFilter.Months:
                            adjustedLocalTime = todayLocal.AddMonths(value.WithinNumber);
                            break;
                        case RMMyhubEndDateWithinFilter.Years:
                            adjustedLocalTime = todayLocal.AddYears(value.WithinNumber);
                            break;
                        default:
                            throw new NotSupportedException($"Unsupported within option: {value.WithinOption}");
                    }
                    var startUtc = todayLocal.Ticks;
                    var endUtc = adjustedLocalTime.Ticks;
                    return new()
                    {
                        SQL = $"AND c.endTime >= @StartDate AND c.endTime < @EndDate",
                        SQLParameters = [
                            new SqlParameter("@StartDate", startUtc),
                            new SqlParameter("@EndDate", endUtc)
                         ]
                    };
                case RMMyhubEndDateFilterOption.Between:

                    var startedUtc = (value.StartTime).Ticks;
                    var endedUtc = (value.EndTime).Ticks;

                    return new RMMyhubSQLInfo
                    {
                        SQL = "AND c.endTime >= @StartTime AND c.endTime < @EndTime",
                        SQLParameters = [
                            new SqlParameter("@StartTime", startedUtc),
                            new SqlParameter("@EndTime", endedUtc)
                        ]
                    };
            }
            ;
            throw new NotSupportedException($"Unsupported filter option: {value}");
        }
        public RMMyhubPageResult LoadAvaliableValues(RMMyhubPageInfo pageInfo)
        {
            throw new NotImplementedException();
        }

        public enum RMMyhubEndDateFilterOption
        {
            AnyTime = 1,
            WithIn = 2,
            Between = 3
        }
        public enum RMMyhubEndDateWithinFilter
        {
            Days = 1,
            Weeks = 2,
            Months = 3,
            Years = 4
        }
        public class RMMyhubEndDateFilterValue
        {
            [JsonProperty("option")]
            public RMMyhubEndDateFilterOption Option { get; set; }

            [JsonProperty("starttime")]
            public DateTime StartTime { get; set; }

            [JsonProperty("endtime")]
            public DateTime EndTime { get; set; }

            [JsonProperty("withinOption")]
            public RMMyhubEndDateWithinFilter WithinOption { get; set; }

            [JsonProperty("withinNumber")]
            public int WithinNumber { get; set; }

            [JsonProperty("dateTimeNow")]  
            public DateTime DateTimeNow { get; set; }
        }
    }
}
