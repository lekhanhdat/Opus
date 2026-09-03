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
using AvePoint.RA.Contract.Archiver;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.RMWeb.Dashboard
{
    public enum ValueAndSavingsTimeRange
    {
        All = 0,
        TwelveMonths = 1,
        SixMonths = 2,
        ThreeMonths = 3,
    }

    public enum ValueAndSavingsSourceFilter
    {
        All = 0,
        Spo = 1,
        Od = 2,
    }

    public class ValueAndSavingsRequest
    {
        public ValueAndSavingsTimeRange TimeRange { get; set; }
        public ValueAndSavingsSourceFilter SourceFilter { get; set; }
    }

    public class ArchivedOverviewItem
    {
        public string Period { get; set; }
        public SizeValue ArchivedStorageBalance { get; set; }
        public SizeValue NewlyArchivedData { get; set; }
        public SizeValue DestroyedDataFromArchive { get; set; }
    }

    public class OptimizationOverviewBySourceItem
    {
        public string Period { get; set; }
        public ValueAndSavingsSourceFilter Source { get; set; }
        public SizeValue ArchivedStorageBalance { get; set; }
        public SizeValue DestroyedData { get; set; }
        public double? SavingsFromArchiving { get; set; }
        public double? SavingsFromDestruction { get; set; }
        public SizeValue DestroyedFromArchiveStorage { get; set; }
        public SizeValue DestroyedFromLiveStorage { get; set; }
        public double? SavingsFromArchivedDestruction { get; set; }
        public double? SavingsFromLiveDestruction { get; set; }
    }

    public class OptimizationContributionBySourceItem
    {
        public string Period { get; set; }
        public double SpoContribution { get; set; }
        public double OdContribution { get; set; }
        public double? TotalSavings { get; set; }
        public double? SpoTotalSavings { get; set; }
        public double? OdTotalSavings { get; set; }
    }

    public class ValueAndSavingsResponse
    {
        public bool HasPriceConfig { get; set; }
        public SizeValue TotalDestroyedDataSize { get; set; }
        public double? TotalSavingsFromArchiving { get; set; }
        public double? TotalSavingsFromDestruction { get; set; }
        public double EstimatedCo2eReduction { get; set; }
    }

    public class SizeValue
    {
        public double Value { get; set; }
        public ArchiverDataUnit Unit { get; set; }
    }

    public class OptimizationOverviewBySourceRequest
    {
        public ValueAndSavingsTimeRange TimeRange { get; set; }
        public ValueAndSavingsSourceFilter SourceFilter { get; set; }
    }

    public class ArchivedOverviewRequest
    {
        public ValueAndSavingsTimeRange TimeRange { get; set; }
    }

    public class ArchivedOverviewResponse
    {
        public bool HasPriceConfig { get; set; }
        public List<ArchivedOverviewItem> ArchivedOverview { get; set; } = new List<ArchivedOverviewItem>();
    }

    public class OptimizationOverviewBySourceResponse
    {
        public bool HasPriceConfig { get; set; }
        public List<OptimizationOverviewBySourceItem> OptimizationOverviewBySource { get; set; } = new List<OptimizationOverviewBySourceItem>();
    }

    public class OptimizationContributionBySourceRequest
    {
        public ValueAndSavingsTimeRange TimeRange { get; set; }
        public ValueAndSavingsSourceFilter SourceFilter { get; set; }
    }

    public class OptimizationContributionBySourceResponse
    {
        public bool HasPriceConfig { get; set; }
        public List<OptimizationContributionBySourceItem> OptimizationContributionBySource { get; set; } = new List<OptimizationContributionBySourceItem>();
    }
}
