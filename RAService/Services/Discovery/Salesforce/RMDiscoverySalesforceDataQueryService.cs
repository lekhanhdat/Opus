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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Salesforce.Model;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Discovery.Impl.Salesforce;
using AvePoint.RA.DB.Dao.Discovery.Salesforce;
using AvePoint.RA.Service.Services.Discovery.Cache;
using AvePoint.RA.Service.Services.Discovery.Salesforce.Query.General.Inactive;
using Newtonsoft.Json;
using RASalesforce;

namespace AvePoint.RA.Service.Services.Discovery.Salesforce
{
    public class RMDiscoverySalesforceDataQueryService : IRMDiscoverySalesforceDataQueryService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoverySalesforceDataQueryService));
        private readonly IRMDiscoverySalesforceDataQueryDao _dataQueryDao = new RMDiscoverySalesforceDataQueryDao();
        private readonly IRMDiscoverySalesforceWithoutInDateDao _withoutInDateDao = new RMDiscoverySalesforceWithoutInDateDao();

        public async Task<List<RMSFObjectSelected>> GetObjectByName(RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            try
            {
                var cacheManager = new RMDiscoveryCacheManager(salesforceQueryParameter.OrganizationId, RMDiscoveryCacheDataSource.Salesforce);
                return await cacheManager.TryGetAsync("GetSalesforceObjectName", salesforceQueryParameter, () => _dataQueryDao.GetObjectByName(salesforceQueryParameter));
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while query Object info. Error: {ex}");
                return [];
            }
        }

        public async Task<RMDiscoverySalesforceSummaryStatisticalDataInfo> GetSummaryStaticalDataInfoAsync(string organizationId)
        {
            try
            {
                if (string.IsNullOrEmpty(organizationId))
                {
                    organizationId = await _dataQueryDao.GetOrginazationId();
                }
                var cacheManager = new RMDiscoveryCacheManager(organizationId, RMDiscoveryCacheDataSource.Salesforce);
                var res = await cacheManager.TryGetAsync("GetSalesforceGetSummaryStaticalDataInfo", new RMDiscoverySalesforceQueryParameter(), () => _dataQueryDao.GetAggregateTotalDataAsync(organizationId));
                return new RMDiscoverySalesforceSummaryStatisticalDataInfo()
                {
                    ObjectTotalCount = res.ObjectTotalCount,
                    RecordsTotalCount = res.RecordsTotalCount,
                    BiggestObjectByDataSize = res.BiggestObjectByDataSize,
                    OldestRecords = GetMonthDifference(res.OldestRecordsCreatedTime, DateTime.UtcNow),
                    DataTotalSize = res.DataTotalSize,
                    DataStorageUsage = res.DataStorageLimit,
                    BiggestObjectByRecordCount = res.BiggestObjectByRecordCount,
                    FileTotalSize = res.FileTotalSize,
                    FileStorageUsage = res.FileStorageLimit,
                    BiggestObjectByFileSize = res.BiggestObjectByFileSize
                };
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while Summary Statical Data of inactive. Error: {ex}");
                return new();
            }
        }
        private static int GetMonthDifference(DateTime startDate, DateTime endDate)
        {
            int months = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;

            if (endDate.Day < startDate.Day)
            {
                months--;
            }

            return months;
        }

        public async Task<RMDiscoverySalesforceAggregateStatisticDataInfo> QueryInactiveAggregateInfo(RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            try
            {
                var querier = new RMDiscoverySalesforceInactiveAggregateStatisticQuerier(salesforceQueryParameter);
                var cacheManager = new RMDiscoveryCacheManager(salesforceQueryParameter.OrganizationId, RMDiscoveryCacheDataSource.Salesforce);
                return await cacheManager.TryGetAsync("QuerySalesforceInactiveAggregateInfo", salesforceQueryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query info of inactive. Error: {e}");
                return new();
            }
        }

        public async Task<List<RMDiscoveryFileExtensionDataInfo>> QueryInactiveFileExtensionsAsync(RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            try
            {
                var querier = new RMDiscoverySalesforceInactiveFileExtensionQuerier(salesforceQueryParameter);
                var cacheManager = new RMDiscoveryCacheManager(salesforceQueryParameter.OrganizationId, RMDiscoveryCacheDataSource.Salesforce);
                return await cacheManager.TryGetAsync("QuerySalesforceInactiveFileExtension", salesforceQueryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query file extensions of inactive. Error: {e}");
                return new();
            }
        }

        public async Task<List<RMDiscoverySizeRangeDataInfo>> QueryInactiveSizeRangesAsync(RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            try
            {
                var querier = new RMDiscoverySalesforceInactiveSizeRangeQuerier(salesforceQueryParameter);
                var cacheManager = new RMDiscoveryCacheManager(salesforceQueryParameter.OrganizationId, RMDiscoveryCacheDataSource.Salesforce);
                return await cacheManager.TryGetAsync("QuerySalesforceInactiveSizeRanges", salesforceQueryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query size range of inactive. Error: {e}");
                return new();
            }
        }
        public async Task<Dictionary<string, object>> QueryInactiveSummaryObjectTotalInfo(RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            try
            {
                salesforceQueryParameter.NodeQueryParameter.ObjectIds = [];
                var querier = new RMDiscoverySalesforceInactiveNodeTotalAggregateQuerier(salesforceQueryParameter);
                var cacheManager = new RMDiscoveryCacheManager(salesforceQueryParameter.OrganizationId, RMDiscoveryCacheDataSource.Salesforce);
                return await cacheManager.TryGetAsync("QuerySalesforceInactiveSummaryObjectTotalInfo", salesforceQueryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query object total of inactive. Error: {e}");
                return new();
            }
        }

        public async Task<List<RMDiscoverySalesforceYearlyData>> QueryFigureDataInfo(RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            try
            {
                var monthLastest = await _dataQueryDao.GetMonthLastest();
                var querier = new RMDiscoverySalesforceInactiveFigureQuerier(salesforceQueryParameter);
                var startYear = GetYearFromMonthsAgo(monthLastest);
                var cacheManager = new RMDiscoveryCacheManager(salesforceQueryParameter.OrganizationId, RMDiscoveryCacheDataSource.Salesforce);
                var res = await cacheManager.TryGetAsync("QuerySalesforceFigureDataInfo", salesforceQueryParameter, () => querier.QueryAsync(startYear));
                return CalculateFigureData(res, startYear);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query figure data of inactive. Error: {e}");
                return new();
            }
        }

        public async Task<List<RMDiscoverySalesforceOrgnization>> GetAllOrganizations()
        {
            var appProfiles = await RMAosApiClient.GetSalesforceAppProfiles(TenantLocalValue.LogonGroupId);
            var salesforceOrganizationsKey = $"{TenantLocalValue.LogonGroupId}_Salesforce_Discovery";
            var organizationsJson = CacheService.Get(CacheNamespace.SalesforceOrganizations, salesforceOrganizationsKey);

            var organizations = organizationsJson.IsNotNullOrEmpty()
                ? JsonConvert.DeserializeObject<List<RMDiscoverySalesforceOrgnization>>(organizationsJson)
                : [];

            var forceSetNewCache = appProfiles.Any(appProfile =>
                                       !organizations.Select(organization => organization.Email)
                                           .Contains(appProfile.AuthorizationUserName));
            
            if (organizationsJson.IsNullOrEmpty() || forceSetNewCache)
            {
                if (forceSetNewCache)
                {
                    CacheService.Remove(CacheNamespace.SalesforceToken, salesforceOrganizationsKey);
                }
                foreach (var appProfile in appProfiles)
                {
                    try
                    {
                        var sfService =
                            new SalesforceService(TenantLocalValue.LogonGroupId, appProfile.TenantId).Build();
                        var organization = await sfService.GetOrganizationAsync();

                        organizations.Add(new RMDiscoverySalesforceOrgnization(appProfile.TenantId, organization.Name,
                            appProfile.AuthorizationUserName));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"get organization failed{ex}");
                    }
                }
                CacheService.Set(CacheNamespace.SalesforceOrganizations, salesforceOrganizationsKey, organizationsJson = JsonConvert.SerializeObject(organizations), DateTime.UtcNow.AddMinutes(3));
            }
            
            return JsonConvert.DeserializeObject<List<RMDiscoverySalesforceOrgnization>>(organizationsJson);
        }

        public async Task<int> GetSalesforceObjects()
        {
            try
            {
                return await _dataQueryDao.CountAllObjectInforAsync();
            }
            catch (Exception ex)
            {
                _logger.Error($"count salesforce object failed{ex}");
            }

            return 0;
        }

        public async Task<List<RMDiscoveryWithoutInDateDataInfo>> GetWithoutInDateListAsync()
        {
            try
            {
                return (await _withoutInDateDao.GetAllAsync())
                    .OrderBy(item => item.Order)
                    .ConvertAll(item => new RMDiscoveryWithoutInDateDataInfo
                    {
                        Id = item.Id,
                        Unit = item.Unit,
                        UnitType = item.UnitType,
                        Order = item.Order
                    })
                    .ToList();
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get without in date list. Error: {e}");
                return new();
            }
        }

        private List<RMDiscoverySalesforceYearlyData> CalculateFigureData(List<RMDiscoverySalesforceYearlyData> data, int startYear)
        {
            if (data.Count == 0) return [];

            DateTime currentDay = DateTime.UtcNow;
            DateTime firstDayOfStartYear = new DateTime(startYear, 1, 1);
            DateTime lastDayOfYear = new DateTime(currentDay.Year, 12, 31);

            var (forecastTotalCount, forecastStorageUsed) = CalculateForecasts(data, firstDayOfStartYear, currentDay);

            UpdateMissingYearsData(startYear, data);

            UpdateCurrentYearData(currentDay, data);

            AddNextYearForecast(currentDay, forecastTotalCount, forecastStorageUsed, data);

            return data.OrderBy(x => x.Year).ToList();
        }

        private static (float forecastTotalCount, float forecastStorageUsed) CalculateForecasts(List<RMDiscoverySalesforceYearlyData> data, DateTime startDay, DateTime currentDay)
        {
            int totalDays = DayDiff(startDay, currentDay);
            float forecastTotalCount = (float)data.Sum(x => x.DataCreatedCount) / totalDays;
            float forecastStorageUsed = (float)data.Sum(x => x.TotalStorageUsed) / totalDays;
            return (forecastTotalCount, forecastStorageUsed);
        }

        private void UpdateCurrentYearData(DateTime currentDay, List<RMDiscoverySalesforceYearlyData> data)
        {
            var currentYearData = data.FirstOrDefault(d => d.Year == currentDay.Year);
            var previousYearData = GetPreviousYearData(currentDay.Year, data);

            if (currentYearData != null)
            {
                currentYearData.DataCreatedCount += previousYearData.DataCreatedCount;
                currentYearData.TotalStorageUsed += previousYearData.TotalStorageUsed;
                currentYearData.IsDashLine = true;
            }
            else
            {
                data.Add(new RMDiscoverySalesforceYearlyData
                {
                    Year = currentDay.Year,
                    DataCreatedCount = previousYearData.DataCreatedCount,
                    TotalStorageUsed = previousYearData.TotalStorageUsed,
                    IsDashLine = true
                });
            }
        }

        private static void AddNextYearForecast(DateTime currentDay, float forecastTotalCount, float forecastStorageUsed, List<RMDiscoverySalesforceYearlyData> data)
        {
            var existDataCreatedCount = GetPreviousYearData(currentDay.Year + 1, data).DataCreatedCount;
            var existTotalStorageUsed = GetPreviousYearData(currentDay.Year + 1, data).TotalStorageUsed;


            DateTime lastDayOfYear = new DateTime(currentDay.Year + 1, 12, 31);

            var sn = CalculateForecastForRemainingDays(forecastTotalCount, currentDay, lastDayOfYear);
            data.Add(new RMDiscoverySalesforceYearlyData
            {
                Year = currentDay.Year + 1,
                DataCreatedCount = CalculateForecastForRemainingDays(forecastTotalCount, currentDay, lastDayOfYear) + existDataCreatedCount,
                TotalStorageUsed = CalculateForecastForRemainingDays(forecastStorageUsed, currentDay, lastDayOfYear) + existTotalStorageUsed,
                IsDashLine = true
            });
        }

        private static int CalculateForecastForRemainingDays(float forecastValue, DateTime currentDay, DateTime endDay) => (int)Math.Ceiling(forecastValue * DayDiff(currentDay, endDay));

        private static int DayOfYear(int year) => DateTime.IsLeapYear(year) ? 366 : 365;
        private static int DayDiff(DateTime startDay, DateTime endDay) => (endDay - startDay).Days;
        private static RMDiscoverySalesforceYearlyData GetPreviousYearData(int year, List<RMDiscoverySalesforceYearlyData> data) => data.FirstOrDefault(d => d.Year == year - 1) ?? new RMDiscoverySalesforceYearlyData();

        private static int GetYearFromMonthsAgo(int monthsAgo)
        {
            double yearsAgo = Math.Ceiling(monthsAgo / 12.0);

            int currentYear = DateTime.Now.Year;
            int resultYear = currentYear - (int)yearsAgo;

            return resultYear;
        }

        public List<RMDiscoverySalesforceYearlyData> UpdateMissingYearsData(int startYear, List<RMDiscoverySalesforceYearlyData> data)
        {
            for (int year = startYear; year < DateTime.UtcNow.Year; year++)
            {
                var existingData = data.FirstOrDefault(d => d.Year == year);
                var previousYearData = GetPreviousYearData(year, data);

                if (existingData == null)
                {
                    data.Add(new RMDiscoverySalesforceYearlyData
                    {
                        Year = year,
                        DataCreatedCount = previousYearData.DataCreatedCount,
                        TotalStorageUsed = previousYearData.TotalStorageUsed,
                        IsDashLine = false
                    });
                }
                else
                {
                    existingData.DataCreatedCount += previousYearData.DataCreatedCount;
                    existingData.TotalStorageUsed += previousYearData.TotalStorageUsed;
                }
            }

            return data;
        }

        public async Task<RMDiscoveryNodeDataInfo> QueryAnalysis(RMDiscoverySalesforceQueryParameter salesforceQueryParameter)
        {
            try
            {
                var querier = new RMDiscoverySalesforceInactiveNodeQuerier(salesforceQueryParameter);
                var cacheManager = new RMDiscoveryCacheManager(salesforceQueryParameter.OrganizationId, RMDiscoveryCacheDataSource.Salesforce);
                return await cacheManager.TryGetAsync("QuerySalesforceAnalysis", salesforceQueryParameter, querier.QueryAsync);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while query data info of inactive. Error: {e}");
                return new();
            }
        }
    }
}
