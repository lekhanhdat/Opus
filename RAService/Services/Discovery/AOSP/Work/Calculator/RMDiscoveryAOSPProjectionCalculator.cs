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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.GraphApi.UsageReport;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.RACommonUtility.JobControl.O365Tenant;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work.Calculator
{
    public class RMDiscoveryAOSPProjectionCalculator
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPProjectionCalculator));

        private readonly IRMDiscoveryAOSPDataDao _dataDao;

        private readonly IRMDiscoveryAOSPTenantConfigurationDao _configurationDao;

        private readonly IRMDiscoveryAOSPTenantDao _tenantDao;

        private readonly RMDiscoveryAOSPMainJob _jobInfo;

        public RMDiscoveryAOSPProjectionCalculator(RMDiscoveryAOSPMainJob jobInfo)
        {
            _dataDao = new RMDiscoveryAOSPDataDao();
            _configurationDao = new RMDiscoveryAOSPTenantConfigurationDao();
            _tenantDao = new RMDiscoveryAOSPTenantDao();
            _jobInfo = jobInfo;
        }

        public async Task<bool> CalculateAsync()
        {
            try
            {
                _logger.Info($"Start calculate projection data.");

                var allO365Tenants = await _tenantDao.GetAllAsync();
                var discoveredO365Tenants = allO365Tenants.Where(tenant => tenant.UniqueId == new Guid(_jobInfo.O365TenantId)).ToList();
                var o365TenantArchiveSpeeds = await CalculateO365TenantArchiveSpeedAsync(discoveredO365Tenants);
                var spO365TenantGrowthValues = await CalculateO365TenantStorageMonthlyGrowthValueAsync(SourceFlag.SharePoint, discoveredO365Tenants);
                var odO365TenantGrowthValues = await CalculateO365TenantStorageMonthlyGrowthValueAsync(SourceFlag.OneDrive, discoveredO365Tenants);
                foreach (var o365Tenant in discoveredO365Tenants)
                {
                    var o365TenantArchiveSpeed = o365TenantArchiveSpeeds.First(item => item.Key == o365Tenant.UniqueId);
                    var spO365TenantGrowthValue = spO365TenantGrowthValues.First(item => item.Key == o365Tenant.UniqueId);
                    var odO365TenantGrowthValue = odO365TenantGrowthValues.First(item => item.Key == o365Tenant.UniqueId);
                    var configurationInfo = new RMDiscoveryProjectionConfigurationInfo
                    {
                        LatestDateTimeTicks = spO365TenantGrowthValue.Value.latestDateTime,
                        LatestStorageSize = spO365TenantGrowthValue.Value.latestStorageSize,
                        OldestDateTimeTicks = spO365TenantGrowthValue.Value.oldestDateTime,
                        OldestStorageSize = spO365TenantGrowthValue.Value.oldestStroageSize,
                        RealityMonthlyGrowthRate = spO365TenantGrowthValue.Value.montylyGrowthStorageSize,
                        MonthlyGrowthRate = spO365TenantGrowthValue.Value.montylyGrowthStorageSize,
                        OdLatestDateTimeTicks = odO365TenantGrowthValue.Value.latestDateTime,
                        OdLatestStorageSize = odO365TenantGrowthValue.Value.latestStorageSize,
                        OdOldestDateTimeTicks = odO365TenantGrowthValue.Value.oldestDateTime,
                        OdOldestStorageSize = odO365TenantGrowthValue.Value.oldestStroageSize,
                        OdRealityMonthlyGrowthRate = odO365TenantGrowthValue.Value.montylyGrowthStorageSize,
                        OdMonthlyGrowthRate = odO365TenantGrowthValue.Value.montylyGrowthStorageSize,
                        RealityDailyOptimizationSpeed = o365TenantArchiveSpeed.Value,
                        DailyOptimizationSpeed = o365TenantArchiveSpeed.Value,
                        DataSizeUnitType = RMDiscoveryProjectionDataSizeUnitType.TB,
                    };
                    await _configurationDao.AddOrUpdateAsync(o365Tenant.UniqueId, RMDiscoveryO365TenantConfigurationType.ProjectionConfiguration, JsonConvert.SerializeObject(configurationInfo));
                    _logger.Info($"The o365 tenant [{o365Tenant.UniqueId}] successful add projection basic info to db.");
                }

                _logger.Info($"End calculate projection data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while calculate projection data. Error: {e}");
                return false;
            }
        }

        private async Task<Dictionary<Guid, (long latestDateTime, long latestStorageSize, long oldestDateTime, long oldestStroageSize, long montylyGrowthStorageSize)>> CalculateO365TenantStorageMonthlyGrowthValueAsync(SourceFlag contentSource, List<RMDiscoveryAOSPTenantInfo> discoveredO365Tenants)
        {
            var res = new Dictionary<Guid, (long latestDateTime, long latestStorageSize, long oldestDateTime, long oldestStroageSize, long montylyGrowthStorageSize)>();
            var appProfile = await RMAosApiClient.GetAOSPAuthProfileByAppId(TenantLocalValue.LogonGroupId, _jobInfo.AppProfileId);

            foreach (var discoveredO365Tenant in discoveredO365Tenants)
            {
                var o365TenantId = discoveredO365Tenant.UniqueId;
                var manager = new RMGraphUsageReportManager(appProfile);
                var reportInfoes = await manager.GetUsageReportsAsync(contentSource, RMGraphUsageReportPeriod.Day180);
                if (!reportInfoes.Any())
                {
                    _logger.Warn($"The o365 tenant [{o365TenantId}] can't find storage usage report.");
                    res.Add(o365TenantId, (DateTime.UtcNow.Ticks, 0L, DateTime.UtcNow.AddMonths(-6).Ticks, 0L, 0L));
                    continue;
                }

                var latestInfo = reportInfoes.First();
                var oldestInfo = reportInfoes.Last();

                var isLessThan6month = (latestInfo.ReportDate.Year - oldestInfo.ReportDate.Year) * 12 + latestInfo.ReportDate.Month - oldestInfo.ReportDate.Month < 6;
                _logger.Info($"The o365 tenant [{o365TenantId}] storage usage report is less than 6 months [{isLessThan6month}].");
                var dailyGrowthStorageSize = (latestInfo.Size - oldestInfo.Size) / (latestInfo.ReportDate - oldestInfo.ReportDate).TotalDays;
                var monthlyGrowthStorageSize = (long)(dailyGrowthStorageSize * 30);
                _logger.Info($"The o365 tenant [{o365TenantId}] latest storage size [{latestInfo.Size}], monthly growth storage size [{monthlyGrowthStorageSize}]");
                res.Add(o365TenantId, (latestInfo.ReportDate.Ticks, latestInfo.Size, isLessThan6month ? latestInfo.ReportDate.AddMonths(-6).Ticks : oldestInfo.ReportDate.Ticks, isLessThan6month ? 0 : oldestInfo.Size, monthlyGrowthStorageSize));
            }

            return res;
        }

        private async Task<Dictionary<Guid, long>> CalculateO365TenantArchiveSpeedAsync(List<RMDiscoveryAOSPTenantInfo> discoveredO365Tenants)
        {
            const int dailyExecutionHours = 20;

            var res = new Dictionary<Guid, long>();

            var controller = new RMO365TenantSubJobController();
            var tenantSubscribedInfoes = await controller.GetAOSPTenantSubscribedInfoToCache(_jobInfo.AppProfileId);
            var tenantSubJobControlDefinitions = await controller.GetTenantSubJobControlDefinitions(tenantSubscribedInfoes);
            foreach (var discoveredO365Tenant in discoveredO365Tenants)
            {
                var tenantSubscribedInfo = tenantSubscribedInfoes.FirstOrDefault(item => new Guid(item.Id) == discoveredO365Tenant.UniqueId);
                if (tenantSubscribedInfo == null)
                {
                    _logger.Error($"The o365 tenant [{discoveredO365Tenant.UniqueId}] not found subscribed info.");
                    res.Add(discoveredO365Tenant.UniqueId, 0L);
                    continue;
                }

                var maxRunSubJobCount = controller.CalculateSubJobCount(tenantSubscribedInfo.UserSeats, tenantSubJobControlDefinitions[tenantSubscribedInfo.Id]);
                var aggregateTotalDataList = await _dataDao.GetAggregateTotalDataListAsync(new Guid(tenantSubscribedInfo.Id));
                var fileTotalSize = aggregateTotalDataList.Sum(item => item.FileTotalSize);
                var fileTotalVersionSize = aggregateTotalDataList.Sum(item => item.TotalVersionSize);
                var fileSumCount = aggregateTotalDataList.Sum(item => item.FileSumCount);
                if (fileTotalSize == 0 || fileTotalVersionSize == 0 || fileSumCount == 0)
                {
                    _logger.Warn($"The o365 tenant [{discoveredO365Tenant.UniqueId}] did not collect any data.");
                    res.Add(discoveredO365Tenant.UniqueId, 0L);
                    continue;
                }

                var avgFileSize = (fileTotalSize - fileTotalVersionSize) / fileSumCount;
                var archiveSpeed = avgFileSize * maxRunSubJobCount * dailyExecutionHours;
                _logger.Info($"The o365 tenant [{tenantSubscribedInfo.Id}] max can run job count [{maxRunSubJobCount}], avg file size [{avgFileSize}], archive speed [{archiveSpeed}].");
                res.Add(new Guid(tenantSubscribedInfo.Id), archiveSpeed);
            }
            return res;
        }

    }
}
