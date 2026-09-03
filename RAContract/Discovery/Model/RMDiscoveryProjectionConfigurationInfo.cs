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
using AvePoint.RA.Contract.Discovery.Model.Rule.Condition;
using AvePoint.RA.Contract.Explorer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery.Model
{
    public class RMDiscoveryProjectionConfigurationInfo
    {
        [JsonProperty("o365TenantId")]
        public Guid O365TenantId { get; set; }

        [JsonProperty("latestDateTimeTicks")]
        public long LatestDateTimeTicks { get; set; }

        [JsonProperty("latestYear")]
        public int LatestYear { get; set; } = DateTime.UtcNow.Year;

        [JsonProperty("latestMonth")]
        public int LatestMonth { get; set; } = DateTime.UtcNow.Month;

        [JsonProperty("latestStorageSize")]
        public long LatestStorageSize { get; set; }

        [JsonProperty("oldestDateTimeTicks")]
        public long OldestDateTimeTicks { get; set; }

        [JsonProperty("oldestYear")]
        public int OldestYear { get; set; } = DateTime.UtcNow.AddMonths(-6).Year;

        [JsonProperty("oldestMonth")]
        public int OldestMonth { get; set; } = DateTime.UtcNow.AddMonths(-6).Month;

        [JsonProperty("oldestStorageSize")]
        public long OldestStorageSize { get; set; }

        [JsonProperty("realityMonthlyGrowthRate")]
        public long RealityMonthlyGrowthRate { get; set; }

        [JsonProperty("monthlyGrowthRate")]
        public long MonthlyGrowthRate { get; set; }

        [JsonProperty("odLatestDateTimeTicks")]
        public long OdLatestDateTimeTicks { get; set; }

        [JsonProperty("odLatestYear")]
        public int OdLatestYear { get; set; } = DateTime.UtcNow.Year;

        [JsonProperty("odLatestMonth")]
        public int OdLatestMonth { get; set; } = DateTime.UtcNow.Month;

        [JsonProperty("odLatestStorageSize")]
        public long OdLatestStorageSize { get; set; }

        [JsonProperty("odOldestDateTimeTicks")]
        public long OdOldestDateTimeTicks { get; set; }

        [JsonProperty("odOldestYear")]
        public int OdOldestYear { get; set; } = DateTime.UtcNow.AddMonths(-6).Year;

        [JsonProperty("odOldestMonth")]
        public int OdOldestMonth { get; set; } = DateTime.UtcNow.AddMonths(-6).Month;

        [JsonProperty("odOldestStorageSize")]
        public long OdOldestStorageSize { get; set; }

        [JsonProperty("odRealityMonthlyGrowthRate")]
        public long OdRealityMonthlyGrowthRate { get; set; }

        [JsonProperty("odMonthlyGrowthRate")]
        public long OdMonthlyGrowthRate { get; set; }

        [JsonProperty("realityDailyOptimizationSpeed")]
        public long RealityDailyOptimizationSpeed { get; set; }

        [JsonProperty("dailyOptimizationSpeed")]
        public long DailyOptimizationSpeed { get; set; }

        [JsonProperty("dataSizeUnitType")]
        public RMDiscoveryProjectionDataSizeUnitType DataSizeUnitType { get; set; } = RMDiscoveryProjectionDataSizeUnitType.TB;
    }

    public enum RMDiscoveryProjectionDataSizeUnitType
    {
        None = 0,
        B = 1,
        KB = 2,
        MB = 3,
        GB = 4,
        TB = 5,
    }
}
