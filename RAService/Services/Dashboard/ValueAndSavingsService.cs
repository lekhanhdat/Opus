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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.RMWeb.Dashboard;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Encryption;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace AvePoint.RA.Service.Services.Dashboard
{
    public class ValueAndSavingsService : RMServiceBase, IValueAndSavingsService
    {
        private const string MinimumSupportedPeriod = "202610";
        private const string AllHistoricalPeriodsStart = "000000";
        private const decimal BytesPerGb = 1073741824m;
        private const decimal GbPerTb = 1024m;
        private const decimal TbThresholdInGb = 2048m;
        private const decimal MinimumDisplayedNonZeroValue = 0.01m;
        private const decimal Co2eReductionFactor = 0.028m;
        private const string HideCurrentMonthChartsKey = "ValueAndSavingsHideCurrentMonthCharts";
        private static readonly TimeSpan ChartCacheDuration = TimeSpan.FromHours(1);

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ValueAndSavingsService));
        private static IRMSODashboardMonthlySnapshotDao SODashboardMonthlySnapshotDao => PlatformWindsorManager.GetService<IRMSODashboardMonthlySnapshotDao>();
        private static IRMArchiveSiteInfoDao ArchiveSiteInfoDao => PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();
        private static IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        private static IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private RMAesEncryptorWrapper AesEncryptorWrapper => new();

        public async Task<ValueAndSavingsResponse> GetStorageValueSummaryAsync(ValueAndSavingsRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var timeRange = request.TimeRange;
            var hideCurrentMonth = ShouldHideCurrentMonthCharts();
            var queryContext = await BuildQueryContextAsync(
                timeRange,
                !hideCurrentMonth,
                hideCurrentMonth);
            var totalDestroyedDataSize = CalculateTotalDestroyedDataSize(queryContext.EligibleRows);
            var totalDestroyedDataSizeDisplay = ToSizeValue(totalDestroyedDataSize);

            var response = new ValueAndSavingsResponse
            {
                HasPriceConfig = queryContext.PriceConfiguration.HasPriceConfig,
                TotalDestroyedDataSize = totalDestroyedDataSizeDisplay,
                EstimatedCo2eReduction = Round2(
                    Convert.ToDecimal(totalDestroyedDataSizeDisplay.Value) * Convert.ToDecimal(Co2eReductionFactor)),
            };

            if (queryContext.PriceConfiguration.HasPriceConfig)
            {
                var archivedBalanceSeed = await GetArchivedBalanceSeedAsync();
                var optimizationOverview = BuildOptimizationOverviewBySource(
                    queryContext.EligibleRows,
                    ValueAndSavingsSourceFilter.All,
                    queryContext.PriceConfiguration,
                    archivedBalanceSeed);

                response.TotalSavingsFromArchiving = Round2(optimizationOverview.Sum(item => Convert.ToDecimal(item.SavingsFromArchiving)));
                response.TotalSavingsFromDestruction = Round2(CalculateTotalSavingsFromDestruction(queryContext.EligibleRows, queryContext.PriceConfiguration));
            }

            return response;
        }

        public async Task<ArchivedOverviewResponse> GetArchivedOverviewAsync(ArchivedOverviewRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var priceConfiguration = await GetPriceConfigurationStateAsync();
            var hideCurrentMonth = ShouldHideCurrentMonthCharts();
            var cacheKey = BuildChartCacheKey("ArchivedOverview", request.TimeRange, null, priceConfiguration);

            return await GetChartResponseAsync(hideCurrentMonth, cacheKey, async () =>
            {
                var queryContext = await BuildQueryContextAsync(
                    request.TimeRange,
                    !hideCurrentMonth,
                    hideCurrentMonth,
                    priceConfiguration);
                var archivedBalanceSeed = await GetArchivedBalanceSeedAsync();

                return new ArchivedOverviewResponse
                {
                    HasPriceConfig = priceConfiguration.HasPriceConfig,
                    ArchivedOverview = BuildArchivedOverview(queryContext.EligibleRows, archivedBalanceSeed.Total),
                };
            });
        }

        public async Task<bool> SaveValueAndSavingsPriceConfigurationAsync(ValueAndSavingsPriceConfiguration priceConfiguration)
        {
            ArgumentNullException.ThrowIfNull(priceConfiguration);
            ValidatePriceConfiguration(priceConfiguration, nameof(priceConfiguration));

            var securityConfig = AesEncryptorWrapper.Encrypt(SerializerHelper.SerializeByDataContractSerializer(priceConfiguration));
            return await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.ValueAndSavingsPriceSetting, securityConfig);
        }

        public async Task<ValueAndSavingsPriceConfiguration> GetValueAndSavingsPriceConfigurationAsync()
        {
            var defaultConfig = CreateDefaultPriceConfiguration();
            var defaultSecurityConfig = AesEncryptorWrapper.Encrypt(SerializerHelper.SerializeByDataContractSerializer(defaultConfig));

            await FunctionSettingDao.NotExistCreateIt(FunctionSettingType.ValueAndSavingsPriceSetting, defaultSecurityConfig);

            var encryptedConfig = await FunctionSettingDao.GetSettingInfo(FunctionSettingType.ValueAndSavingsPriceSetting);
            return DeserializePriceConfigurationOrThrow(encryptedConfig);
        }

        public async Task<OptimizationOverviewBySourceResponse> GetOptimizationOverviewBySourceAsync(OptimizationOverviewBySourceRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var timeRange = request.TimeRange;
            var sourceFilter = request.SourceFilter;
            var priceConfiguration = await GetPriceConfigurationStateAsync();
            var hideCurrentMonth = ShouldHideCurrentMonthCharts();
            var cacheKey = BuildChartCacheKey("OptimizationOverviewBySource", timeRange, sourceFilter, priceConfiguration);

            return await GetChartResponseAsync(hideCurrentMonth, cacheKey, async () =>
            {
                var queryContext = await BuildQueryContextAsync(
                    timeRange,
                    !hideCurrentMonth,
                    hideCurrentMonth,
                    priceConfiguration);
                var archivedBalanceSeed = await GetArchivedBalanceSeedAsync();

                return new OptimizationOverviewBySourceResponse
                {
                    HasPriceConfig = priceConfiguration.HasPriceConfig,
                    OptimizationOverviewBySource = BuildOptimizationOverviewBySource(
                        queryContext.EligibleRows,
                        sourceFilter,
                        priceConfiguration,
                        archivedBalanceSeed),
                };
            });
        }

        public async Task<OptimizationContributionBySourceResponse> GetOptimizationContributionBySourceAsync(OptimizationContributionBySourceRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var timeRange = request.TimeRange;
            var sourceFilter = request.SourceFilter;
            var priceConfiguration = await GetPriceConfigurationStateAsync();
            var hideCurrentMonth = ShouldHideCurrentMonthCharts();
            var cacheKey = BuildChartCacheKey("OptimizationContributionBySource", timeRange, sourceFilter, priceConfiguration);

            return await GetChartResponseAsync(hideCurrentMonth, cacheKey, async () =>
            {
                var queryContext = await BuildQueryContextAsync(
                    timeRange,
                    !hideCurrentMonth,
                    hideCurrentMonth,
                    priceConfiguration);
                var archivedBalanceSeed = await GetArchivedBalanceSeedAsync();

                return new OptimizationContributionBySourceResponse
                {
                    HasPriceConfig = priceConfiguration.HasPriceConfig,
                    OptimizationContributionBySource = BuildOptimizationContributionBySource(
                        queryContext.EligibleRows,
                        sourceFilter,
                        priceConfiguration,
                        archivedBalanceSeed),
                };
            });
        }

        private async Task<ValueAndSavingsQueryContext> BuildQueryContextAsync(
            ValueAndSavingsTimeRange timeRange,
            bool includeCurrentPeriod,
            bool applyMinimumSupportedPeriod,
            ValueAndSavingsPriceConfigurationState priceConfiguration = null)
        {
            var now = DateTime.Now;
            var startPeriod = GetStartPeriod(timeRange, now, applyMinimumSupportedPeriod);
            var endPeriod = GetEndPeriod(includeCurrentPeriod, now);
            if (string.CompareOrdinal(startPeriod, endPeriod) > 0)
            {
                return new ValueAndSavingsQueryContext(
                    new List<RMSODashboardMonthlySnapshot>(),
                    priceConfiguration ?? await GetPriceConfigurationStateAsync());
            }

            var rows = await SODashboardMonthlySnapshotDao.GetByStartPeriodAsync(startPeriod);

            return new ValueAndSavingsQueryContext(
                FilterEligibleRows(rows, startPeriod, endPeriod),
                priceConfiguration ?? await GetPriceConfigurationStateAsync());
        }

        private static string GetStartPeriod(
            ValueAndSavingsTimeRange timeRange,
            DateTime now,
            bool applyMinimumSupportedPeriod)
        {
            var currentMonth = new DateTime(now.Year, now.Month, 1);
            var startPeriod = timeRange switch
            {
                ValueAndSavingsTimeRange.All => applyMinimumSupportedPeriod
                    ? MinimumSupportedPeriod
                    : AllHistoricalPeriodsStart,
                ValueAndSavingsTimeRange.TwelveMonths => currentMonth.AddMonths(-11).ToString("yyyyMM"),
                ValueAndSavingsTimeRange.SixMonths => currentMonth.AddMonths(-5).ToString("yyyyMM"),
                ValueAndSavingsTimeRange.ThreeMonths => currentMonth.AddMonths(-2).ToString("yyyyMM"),
                _ => throw new ArgumentException($"Unsupported timeRange: {timeRange}.", nameof(timeRange)),
            };

            return applyMinimumSupportedPeriod && string.CompareOrdinal(startPeriod, MinimumSupportedPeriod) < 0
                ? MinimumSupportedPeriod
                : startPeriod;
        }

        private static string GetEndPeriod(bool includeCurrentPeriod, DateTime now)
        {
            var currentMonth = new DateTime(now.Year, now.Month, 1);
            return includeCurrentPeriod
                ? currentMonth.ToString("yyyyMM")
                : currentMonth.AddMonths(-1).ToString("yyyyMM");
        }

        private static List<RMSODashboardMonthlySnapshot> FilterEligibleRows(IEnumerable<RMSODashboardMonthlySnapshot> rows, string startPeriod, string endPeriod)
        {
            if (rows == null)
            {
                return new List<RMSODashboardMonthlySnapshot>();
            }

            return rows
                .Where(row => row != null
                    && !string.IsNullOrWhiteSpace(row.Period)
                    && string.CompareOrdinal(row.Period, startPeriod) >= 0
                    && string.CompareOrdinal(row.Period, endPeriod) <= 0)
                .OrderBy(row => row.Period)
                .ToList();
        }

        private bool ShouldHideCurrentMonthCharts()
        {
            var value = KeyValueDao.GetValueByKey(HideCurrentMonthChartsKey)?.Value;
            return string.IsNullOrWhiteSpace(value)
                || !bool.TryParse(value, out var enabled)
                || enabled;
        }

        private async Task<T> GetChartResponseAsync<T>(bool useCache, string cacheKey, Func<Task<T>> dataProvider)
        {
            return useCache
                ? await RMCacheManager.Cache.TryGetAsync(cacheKey, dataProvider, ChartCacheDuration, false)
                : await dataProvider();
        }

        private static string BuildChartCacheKey(
            string chartName,
            ValueAndSavingsTimeRange timeRange,
            ValueAndSavingsSourceFilter? sourceFilter,
            ValueAndSavingsPriceConfigurationState priceConfiguration)
        {
            var sourceSegment = sourceFilter.HasValue ? sourceFilter.Value.ToString() : "All";
            var priceSegment = BuildPriceConfigurationCacheSegment(priceConfiguration);
            var currentMonthSegment = DateTime.Now.ToString("yyyyMM");
            return $"ValueAndSavings:{chartName}:{currentMonthSegment}:{timeRange}:{sourceSegment}:{priceSegment}";
        }

        private static string BuildPriceConfigurationCacheSegment(ValueAndSavingsPriceConfigurationState priceConfiguration)
        {
            if (priceConfiguration == null || !priceConfiguration.HasPriceConfig)
            {
                return "NoPrice";
            }

            return string.Join(
                "_",
                priceConfiguration.SpoLivePrice.ToString(CultureInfo.InvariantCulture),
                priceConfiguration.OdLivePrice.ToString(CultureInfo.InvariantCulture),
                priceConfiguration.SpoArchivePrice.ToString(CultureInfo.InvariantCulture),
                priceConfiguration.OdArchivePrice.ToString(CultureInfo.InvariantCulture));
        }

        private async Task<ValueAndSavingsPriceConfigurationState> GetPriceConfigurationStateAsync()
        {
            var encryptedConfig = await FunctionSettingDao.GetSettingInfo(FunctionSettingType.ValueAndSavingsPriceSetting);
            if (string.IsNullOrWhiteSpace(encryptedConfig))
            {
                return ValueAndSavingsPriceConfigurationState.Invalid();
            }

            string decryptedConfig;
            try
            {
                decryptedConfig = AesEncryptorWrapper.Decrypt(encryptedConfig);
            }
            catch(Exception ex)
            {
                Logger.Warn($"Failed to load Value & Savings price configuration: ciphertext is invalid. Exception: {ex}");
                return ValueAndSavingsPriceConfigurationState.Invalid();
            }

            if (string.IsNullOrWhiteSpace(decryptedConfig))
            {
                Logger.Warn("Failed to load Value & Savings price configuration: ciphertext is invalid.");
                return ValueAndSavingsPriceConfigurationState.Invalid();
            }

            try
            {
                var config = DeserializeDecryptedPriceConfigurationOrThrow(decryptedConfig);
                return ValueAndSavingsPriceConfigurationState.Valid(
                    config.SpoLivePrice!.Value,
                    config.OdLivePrice!.Value,
                    config.SpoArchivePrice!.Value,
                    config.OdArchivePrice!.Value);
            }
            catch(Exception ex)
            {
                Logger.Warn($"Failed to load Value & Savings price configuration: serialized content is invalid. Exception: {ex}");
                return ValueAndSavingsPriceConfigurationState.Invalid();
            }
        }

        private ValueAndSavingsPriceConfiguration DeserializePriceConfigurationOrThrow(string encryptedConfig)
        {
            if (string.IsNullOrWhiteSpace(encryptedConfig))
            {
                throw new ArgumentException("Value & Savings price configuration is missing.", nameof(encryptedConfig));
            }

            var decryptedConfig = AesEncryptorWrapper.Decrypt(encryptedConfig);
            return DeserializeDecryptedPriceConfigurationOrThrow(decryptedConfig);
        }

        private static ValueAndSavingsPriceConfiguration DeserializeDecryptedPriceConfigurationOrThrow(string decryptedConfig)
        {
            if (string.IsNullOrWhiteSpace(decryptedConfig))
            {
                throw new ArgumentException("Value & Savings price configuration is invalid.", nameof(decryptedConfig));
            }

            var config = SerializerHelper.DeserializeByDataContractSerializer<ValueAndSavingsPriceConfiguration>(decryptedConfig);
            ValidatePriceConfiguration(config, nameof(config));
            return config;
        }

        private static void ValidatePriceConfiguration(ValueAndSavingsPriceConfiguration priceConfiguration, string parameterName)
        {
            if (priceConfiguration?.SpoLivePrice == null
                || priceConfiguration.OdLivePrice == null
                || priceConfiguration.SpoArchivePrice == null
                || priceConfiguration.OdArchivePrice == null
                || priceConfiguration.SpoLivePrice.Value < 0
                || priceConfiguration.OdLivePrice.Value < 0
                || priceConfiguration.SpoArchivePrice.Value < 0
                || priceConfiguration.OdArchivePrice.Value < 0)
            {
                throw new ArgumentException("Value & Savings price configuration is invalid.", parameterName);
            }
        }

        private static ValueAndSavingsPriceConfiguration CreateDefaultPriceConfiguration()
        {
            return new ValueAndSavingsPriceConfiguration
            {
                SpoLivePrice = 0,
                OdLivePrice = 0,
                SpoArchivePrice = 0,
                OdArchivePrice = 0,
            };
        }

        private static decimal CalculateTotalDestroyedDataSize(IEnumerable<RMSODashboardMonthlySnapshot> rows)
        {
            decimal totalBytes = 0;
            foreach (var row in rows)
            {
                totalBytes += (decimal)row.SpoDestroyedFromArchiveSize
                    + row.OdDestroyedFromArchiveSize
                    + row.SpoDestroyedFromLiveSize
                    + row.OdDestroyedFromLiveSize;
            }

            return totalBytes / BytesPerGb;
        }

        private static decimal CalculateTotalSavingsFromDestruction(IEnumerable<RMSODashboardMonthlySnapshot> rows, ValueAndSavingsPriceConfigurationState priceConfiguration)
        {
            decimal totalSavings = 0;
            foreach (var row in rows)
            {
                totalSavings += ((decimal)row.SpoDestroyedFromArchiveSize * priceConfiguration.SpoArchivePrice)
                    + ((decimal)row.OdDestroyedFromArchiveSize * priceConfiguration.OdArchivePrice)
                    + ((decimal)row.SpoDestroyedFromLiveSize * priceConfiguration.SpoLivePrice)
                    + ((decimal)row.OdDestroyedFromLiveSize * priceConfiguration.OdLivePrice);
            }

            return totalSavings / BytesPerGb;
        }

        private async Task<ArchivedBalanceSeed> GetArchivedBalanceSeedAsync()
        {
            try
            {
                var totalTask = ArchiveSiteInfoDao.GetArchiverDataSizeAsync();
                var sharePointTask = ArchiveSiteInfoDao.GetSharePointArchiverDataSizeAsync();
                var oneDriveTask = ArchiveSiteInfoDao.GetOneDriveArchiverDataSizeAsync();

                await Task.WhenAll(totalTask, sharePointTask, oneDriveTask);

                return new ArchivedBalanceSeed(
                    Convert.ToDecimal(totalTask.Result),
                    Convert.ToDecimal(sharePointTask.Result),
                    Convert.ToDecimal(oneDriveTask.Result));
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to load archived balance seed. Defaulting to 0. Exception: {ex}");
                return ArchivedBalanceSeed.Zero();
            }
        }

        private static List<ArchivedOverviewItem> BuildArchivedOverview(IEnumerable<RMSODashboardMonthlySnapshot> rows, decimal initialBalance)
        {
            var archivedOverview = new List<ArchivedOverviewItem>();
            var rollingBalance = initialBalance;

            foreach (var row in rows)
            {
                var newlyArchivedData = ToGb(GetArchivedBytes(row, ValueAndSavingsSourceFilter.All));
                var destroyedDataFromArchive = ToGb(GetDestroyedFromArchiveBytes(row, ValueAndSavingsSourceFilter.All));
                rollingBalance = CalculateRollingBalance(rollingBalance, newlyArchivedData, destroyedDataFromArchive);

                archivedOverview.Add(new ArchivedOverviewItem
                {
                    Period = row.Period,
                    ArchivedStorageBalance = ToSizeValue(rollingBalance),
                    NewlyArchivedData = ToSizeValue(newlyArchivedData),
                    DestroyedDataFromArchive = ToSizeValue(destroyedDataFromArchive),
                });
            }

            return archivedOverview;
        }

        private static List<OptimizationOverviewBySourceItem> BuildOptimizationOverviewBySource(
            IEnumerable<RMSODashboardMonthlySnapshot> rows,
            ValueAndSavingsSourceFilter sourceFilter,
            ValueAndSavingsPriceConfigurationState priceConfiguration,
            ArchivedBalanceSeed archivedBalanceSeed)
        {
            var optimizationOverviewBySource = new List<OptimizationOverviewBySourceItem>();
            var rollingBalance = GetInitialRollingBalance(sourceFilter, archivedBalanceSeed);
            var spoRollingBalance = archivedBalanceSeed.SharePoint;
            var odRollingBalance = archivedBalanceSeed.OneDrive;

            foreach (var row in rows)
            {
                var archivedData = ToGb(GetArchivedBytes(row, sourceFilter));
                var destroyedFromArchiveStorage = ToGb(GetDestroyedFromArchiveBytes(row, sourceFilter));
                var destroyedFromLiveStorage = ToGb(GetDestroyedFromLiveBytes(row, sourceFilter));
                var destroyedData = destroyedFromArchiveStorage + destroyedFromLiveStorage;
                rollingBalance = CalculateRollingBalance(rollingBalance, archivedData, destroyedFromArchiveStorage);
                spoRollingBalance = CalculateRollingBalance(
                    spoRollingBalance,
                    ToGb(GetArchivedBytes(row, ValueAndSavingsSourceFilter.Spo)),
                    ToGb(GetDestroyedFromArchiveBytes(row, ValueAndSavingsSourceFilter.Spo)));
                odRollingBalance = CalculateRollingBalance(
                    odRollingBalance,
                    ToGb(GetArchivedBytes(row, ValueAndSavingsSourceFilter.Od)),
                    ToGb(GetDestroyedFromArchiveBytes(row, ValueAndSavingsSourceFilter.Od)));
                var savingsFromArchivedDestruction = CalculateSavingsFromArchivedDestruction(row, sourceFilter, priceConfiguration);
                var savingsFromLiveDestruction = CalculateSavingsFromLiveDestruction(row, sourceFilter, priceConfiguration);

                optimizationOverviewBySource.Add(new OptimizationOverviewBySourceItem
                {
                    Period = row.Period,
                    Source = sourceFilter,
                    ArchivedStorageBalance = ToSizeValue(rollingBalance),
                    DestroyedData = ToSizeValue(destroyedData),
                    SavingsFromArchiving = RoundNullable2(CalculateBalanceBasedSavingsFromArchiving(
                        sourceFilter,
                        rollingBalance,
                        spoRollingBalance,
                        odRollingBalance,
                        priceConfiguration)),
                    SavingsFromDestruction = RoundNullable2(SumNullable(savingsFromArchivedDestruction, savingsFromLiveDestruction)),
                    DestroyedFromArchiveStorage = ToSizeValue(destroyedFromArchiveStorage),
                    DestroyedFromLiveStorage = ToSizeValue(destroyedFromLiveStorage),
                    SavingsFromArchivedDestruction = RoundNullable2(savingsFromArchivedDestruction),
                    SavingsFromLiveDestruction = RoundNullable2(savingsFromLiveDestruction),
                });
            }

            return optimizationOverviewBySource;
        }

        private static decimal? CalculateBalanceBasedSavingsFromArchiving(
            ValueAndSavingsSourceFilter sourceFilter,
            decimal rollingBalance,
            decimal spoRollingBalance,
            decimal odRollingBalance,
            ValueAndSavingsPriceConfigurationState priceConfiguration)
        {
            if (!priceConfiguration.HasPriceConfig)
            {
                return null;
            }

            return sourceFilter switch
            {
                ValueAndSavingsSourceFilter.All =>
                    (spoRollingBalance * (priceConfiguration.SpoLivePrice - priceConfiguration.SpoArchivePrice))
                    + (odRollingBalance * (priceConfiguration.OdLivePrice - priceConfiguration.OdArchivePrice)),
                ValueAndSavingsSourceFilter.Spo =>
                    rollingBalance * (priceConfiguration.SpoLivePrice - priceConfiguration.SpoArchivePrice),
                ValueAndSavingsSourceFilter.Od =>
                    rollingBalance * (priceConfiguration.OdLivePrice - priceConfiguration.OdArchivePrice),
                _ => throw new ArgumentException($"Unsupported sourceFilter: {sourceFilter}.", nameof(sourceFilter)),
            };
        }

        private static decimal GetInitialRollingBalance(ValueAndSavingsSourceFilter sourceFilter, ArchivedBalanceSeed archivedBalanceSeed)
        {
            return sourceFilter switch
            {
                ValueAndSavingsSourceFilter.All => archivedBalanceSeed.Total,
                ValueAndSavingsSourceFilter.Spo => archivedBalanceSeed.SharePoint,
                ValueAndSavingsSourceFilter.Od => archivedBalanceSeed.OneDrive,
                _ => throw new ArgumentException($"Unsupported sourceFilter: {sourceFilter}.", nameof(sourceFilter)),
            };
        }

        private static List<OptimizationContributionBySourceItem> BuildOptimizationContributionBySource(
            IEnumerable<RMSODashboardMonthlySnapshot> rows,
            ValueAndSavingsSourceFilter sourceFilter,
            ValueAndSavingsPriceConfigurationState priceConfiguration,
            ArchivedBalanceSeed archivedBalanceSeed)
        {
            var optimizationContributionBySource = new List<OptimizationContributionBySourceItem>();
            var spoRollingBalance = archivedBalanceSeed.SharePoint;
            var odRollingBalance = archivedBalanceSeed.OneDrive;

            foreach (var row in rows)
            {
                spoRollingBalance = CalculateRollingBalance(
                    spoRollingBalance,
                    ToGb(GetArchivedBytes(row, ValueAndSavingsSourceFilter.Spo)),
                    ToGb(GetDestroyedFromArchiveBytes(row, ValueAndSavingsSourceFilter.Spo)));
                odRollingBalance = CalculateRollingBalance(
                    odRollingBalance,
                    ToGb(GetArchivedBytes(row, ValueAndSavingsSourceFilter.Od)),
                    ToGb(GetDestroyedFromArchiveBytes(row, ValueAndSavingsSourceFilter.Od)));

                decimal spoContribution;
                decimal odContribution;
                var spoSavingsFromArchiving = CalculateBalanceBasedSavingsFromArchiving(
                    ValueAndSavingsSourceFilter.Spo,
                    spoRollingBalance,
                    spoRollingBalance,
                    odRollingBalance,
                    priceConfiguration);
                var odSavingsFromArchiving = CalculateBalanceBasedSavingsFromArchiving(
                    ValueAndSavingsSourceFilter.Od,
                    odRollingBalance,
                    spoRollingBalance,
                    odRollingBalance,
                    priceConfiguration);
                var spoTotalSavings = SumNullable(
                    spoSavingsFromArchiving,
                    CalculateSavingsFromDestruction(row, ValueAndSavingsSourceFilter.Spo, priceConfiguration));
                var odTotalSavings = SumNullable(
                    odSavingsFromArchiving,
                    CalculateSavingsFromDestruction(row, ValueAndSavingsSourceFilter.Od, priceConfiguration));
                decimal? totalSavings;

                switch (sourceFilter)
                {
                    case ValueAndSavingsSourceFilter.All:
                        totalSavings = SumNullable(spoTotalSavings, odTotalSavings);
                        var allSourcesTotalSavings = totalSavings.GetValueOrDefault();
                        spoContribution = allSourcesTotalSavings == 0
                            ? 0
                            : spoTotalSavings.GetValueOrDefault() / allSourcesTotalSavings * 100;
                        odContribution = allSourcesTotalSavings == 0
                            ? 0
                            : odTotalSavings.GetValueOrDefault() / allSourcesTotalSavings * 100;
                        break;
                    case ValueAndSavingsSourceFilter.Spo:
                        spoContribution = 100;
                        odContribution = 0;
                        odTotalSavings = priceConfiguration.HasPriceConfig ? 0m : (decimal?)null;
                        totalSavings = spoTotalSavings;
                        break;
                    case ValueAndSavingsSourceFilter.Od:
                        spoContribution = 0;
                        odContribution = 100;
                        spoTotalSavings = priceConfiguration.HasPriceConfig ? 0m : (decimal?)null;
                        totalSavings = odTotalSavings;
                        break;
                    default:
                        throw new ArgumentException($"Unsupported sourceFilter: {sourceFilter}.", nameof(sourceFilter));
                }

                optimizationContributionBySource.Add(new OptimizationContributionBySourceItem
                {
                    Period = row.Period,
                    SpoContribution = Round2(spoContribution),
                    OdContribution = Round2(odContribution),
                    SpoTotalSavings = RoundNullable2(spoTotalSavings),
                    OdTotalSavings = RoundNullable2(odTotalSavings),
                    TotalSavings = RoundNullable2(totalSavings),
                });
            }

            return optimizationContributionBySource;
        }

        private static decimal CalculateRollingBalance(decimal currentBalance, decimal archivedData, decimal destroyedFromArchiveData)
        {
            currentBalance += archivedData - destroyedFromArchiveData;
            return currentBalance < 0 ? 0 : currentBalance;
        }

        private static decimal GetArchivedBytes(RMSODashboardMonthlySnapshot row, ValueAndSavingsSourceFilter sourceFilter)
        {
            return sourceFilter switch
            {
                ValueAndSavingsSourceFilter.All => (decimal)row.SpoArchivedSize + row.OdArchivedSize,
                ValueAndSavingsSourceFilter.Spo => row.SpoArchivedSize,
                ValueAndSavingsSourceFilter.Od => row.OdArchivedSize,
                _ => throw new ArgumentException($"Unsupported sourceFilter: {sourceFilter}.", nameof(sourceFilter)),
            };
        }

        private static decimal GetDestroyedFromArchiveBytes(RMSODashboardMonthlySnapshot row, ValueAndSavingsSourceFilter sourceFilter)
        {
            return sourceFilter switch
            {
                ValueAndSavingsSourceFilter.All => (decimal)row.SpoDestroyedFromArchiveSize + row.OdDestroyedFromArchiveSize,
                ValueAndSavingsSourceFilter.Spo => row.SpoDestroyedFromArchiveSize,
                ValueAndSavingsSourceFilter.Od => row.OdDestroyedFromArchiveSize,
                _ => throw new ArgumentException($"Unsupported sourceFilter: {sourceFilter}.", nameof(sourceFilter)),
            };
        }

        private static decimal GetDestroyedFromLiveBytes(RMSODashboardMonthlySnapshot row, ValueAndSavingsSourceFilter sourceFilter)
        {
            return sourceFilter switch
            {
                ValueAndSavingsSourceFilter.All => (decimal)row.SpoDestroyedFromLiveSize + row.OdDestroyedFromLiveSize,
                ValueAndSavingsSourceFilter.Spo => row.SpoDestroyedFromLiveSize,
                ValueAndSavingsSourceFilter.Od => row.OdDestroyedFromLiveSize,
                _ => throw new ArgumentException($"Unsupported sourceFilter: {sourceFilter}.", nameof(sourceFilter)),
            };
        }

        private static decimal? CalculateSavingsFromDestruction(
            RMSODashboardMonthlySnapshot row,
            ValueAndSavingsSourceFilter sourceFilter,
            ValueAndSavingsPriceConfigurationState priceConfiguration)
        {
            if (!priceConfiguration.HasPriceConfig)
            {
                return null;
            }

            return CalculateDestructionSavingsBytes(row, sourceFilter, priceConfiguration) / BytesPerGb;
        }

        private static decimal? CalculateSavingsFromArchivedDestruction(
            RMSODashboardMonthlySnapshot row,
            ValueAndSavingsSourceFilter sourceFilter,
            ValueAndSavingsPriceConfigurationState priceConfiguration)
        {
            if (!priceConfiguration.HasPriceConfig)
            {
                return null;
            }

            var savings = sourceFilter switch
            {
                ValueAndSavingsSourceFilter.All =>
                    ((decimal)row.SpoDestroyedFromArchiveSize * priceConfiguration.SpoArchivePrice)
                    + ((decimal)row.OdDestroyedFromArchiveSize * priceConfiguration.OdArchivePrice),
                ValueAndSavingsSourceFilter.Spo =>
                    (decimal)row.SpoDestroyedFromArchiveSize * priceConfiguration.SpoArchivePrice,
                ValueAndSavingsSourceFilter.Od =>
                    (decimal)row.OdDestroyedFromArchiveSize * priceConfiguration.OdArchivePrice,
                _ => throw new ArgumentException($"Unsupported sourceFilter: {sourceFilter}.", nameof(sourceFilter)),
            };
            return savings / BytesPerGb;
        }

        private static decimal? CalculateSavingsFromLiveDestruction(
            RMSODashboardMonthlySnapshot row,
            ValueAndSavingsSourceFilter sourceFilter,
            ValueAndSavingsPriceConfigurationState priceConfiguration)
        {
            if (!priceConfiguration.HasPriceConfig)
            {
                return null;
            }

            var savings = sourceFilter switch
            {
                ValueAndSavingsSourceFilter.All =>
                    ((decimal)row.SpoDestroyedFromLiveSize * priceConfiguration.SpoLivePrice)
                    + ((decimal)row.OdDestroyedFromLiveSize * priceConfiguration.OdLivePrice),
                ValueAndSavingsSourceFilter.Spo =>
                    (decimal)row.SpoDestroyedFromLiveSize * priceConfiguration.SpoLivePrice,
                ValueAndSavingsSourceFilter.Od =>
                    (decimal)row.OdDestroyedFromLiveSize * priceConfiguration.OdLivePrice,
                _ => throw new ArgumentException($"Unsupported sourceFilter: {sourceFilter}.", nameof(sourceFilter)),
            };

            return savings / BytesPerGb;
        }

        private static decimal? SumNullable(decimal? left, decimal? right)
        {
            return left.HasValue && right.HasValue
                ? left.Value + right.Value
                : null;
        }

        private static decimal CalculateDestructionSavingsBytes(
            RMSODashboardMonthlySnapshot row,
            ValueAndSavingsSourceFilter sourceFilter,
            ValueAndSavingsPriceConfigurationState priceConfiguration)
        {
            return sourceFilter switch
            {
                ValueAndSavingsSourceFilter.All =>
                    ((decimal)row.SpoDestroyedFromArchiveSize * priceConfiguration.SpoArchivePrice)
                    + ((decimal)row.OdDestroyedFromArchiveSize * priceConfiguration.OdArchivePrice)
                    + ((decimal)row.SpoDestroyedFromLiveSize * priceConfiguration.SpoLivePrice)
                    + ((decimal)row.OdDestroyedFromLiveSize * priceConfiguration.OdLivePrice),
                ValueAndSavingsSourceFilter.Spo =>
                    ((decimal)row.SpoDestroyedFromArchiveSize * priceConfiguration.SpoArchivePrice)
                    + ((decimal)row.SpoDestroyedFromLiveSize * priceConfiguration.SpoLivePrice),
                ValueAndSavingsSourceFilter.Od =>
                    ((decimal)row.OdDestroyedFromArchiveSize * priceConfiguration.OdArchivePrice)
                    + ((decimal)row.OdDestroyedFromLiveSize * priceConfiguration.OdLivePrice),
                _ => throw new ArgumentException($"Unsupported sourceFilter: {sourceFilter}.", nameof(sourceFilter)),
            };
        }

        private static decimal ToGb(decimal bytes)
        {
            return bytes / BytesPerGb;
        }

        private static double Round2(decimal value)
        {
            return decimal.ToDouble(Math.Round(value, 2, MidpointRounding.AwayFromZero));
        }

        private static SizeValue ToSizeValue(decimal valueInGb)
        {
            var unit = ArchiverDataUnit.GB;
            var displayValue = valueInGb;

            if (valueInGb > TbThresholdInGb)
            {
                unit = ArchiverDataUnit.TB;
                displayValue = valueInGb / GbPerTb;
            }

            var roundedValue = Math.Round(displayValue, 2, MidpointRounding.AwayFromZero);
            if (roundedValue == 0 && displayValue != 0)
            {
                roundedValue = MinimumDisplayedNonZeroValue;
            }

            return new SizeValue
            {
                Value = decimal.ToDouble(roundedValue),
                Unit = unit,
            };
        }

        private static double? RoundNullable2(decimal? value)
        {
            return value.HasValue ? Round2(value.Value) : null;
        }

        private sealed class ValueAndSavingsPriceConfigurationState
        {
            public bool HasPriceConfig { get; private set; }
            public decimal SpoLivePrice { get; private set; }
            public decimal OdLivePrice { get; private set; }
            public decimal SpoArchivePrice { get; private set; }
            public decimal OdArchivePrice { get; private set; }

            public static ValueAndSavingsPriceConfigurationState Invalid()
            {
                return new ValueAndSavingsPriceConfigurationState();
            }

            public static ValueAndSavingsPriceConfigurationState Valid(decimal spoLivePrice, decimal odLivePrice, decimal spoArchivePrice, decimal odArchivePrice)
            {
                return new ValueAndSavingsPriceConfigurationState
                {
                    HasPriceConfig = true,
                    SpoLivePrice = spoLivePrice,
                    OdLivePrice = odLivePrice,
                    SpoArchivePrice = spoArchivePrice,
                    OdArchivePrice = odArchivePrice,
                };
            }
        }

        private sealed class ArchivedBalanceSeed
        {
            public ArchivedBalanceSeed(decimal total, decimal sharePoint, decimal oneDrive)
            {
                Total = total;
                SharePoint = sharePoint;
                OneDrive = oneDrive;
            }

            public decimal Total { get; }
            public decimal SharePoint { get; }
            public decimal OneDrive { get; }

            public static ArchivedBalanceSeed Zero()
            {
                return new ArchivedBalanceSeed(0, 0, 0);
            }
        }

        private sealed class ValueAndSavingsQueryContext
        {
            public ValueAndSavingsQueryContext(List<RMSODashboardMonthlySnapshot> eligibleRows, ValueAndSavingsPriceConfigurationState priceConfiguration)
            {
                EligibleRows = eligibleRows;
                PriceConfiguration = priceConfiguration;
            }

            public List<RMSODashboardMonthlySnapshot> EligibleRows { get; }
            public ValueAndSavingsPriceConfigurationState PriceConfiguration { get; }
        }
    }
}
