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
using AngleSharp.Common;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;
using AvePoint.RA.SharePoint.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V1
{
    public class RMDiscoveryOffice365DuplicateDataAnalyzer : RMDiscoveryOffice365DataAnalyzer
    {
        public RMDiscoveryOffice365DuplicateDataAnalyzer(
            SourceFlag contentSource,
            RMDiscoveryOffice365AnalysisJob jobInfo,
            RMDiscoveryOffice365FileExtensionAnalysisManager fileExtensionAnalysisManager,
            RMDiscoveryOffice365ContainerInfo containerInfo,
            RMDiscoveryOffice365SiteInfo siteInfo, List<int> sizeRangeIds, List<int> dateRangeIds, bool enableExpandQueryTest)
            : base(contentSource, jobInfo, fileExtensionAnalysisManager, containerInfo, siteInfo, sizeRangeIds, dateRangeIds, enableExpandQueryTest)
        {
        }

        public override async Task<bool> AnalysisAsync()
        {
            try
            {
                var rotRules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT);
                var duplicateRules = rotRules.Where(item => item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
                if (!duplicateRules.Any())
                {
                    _logger.Info($"Current cutomer no duplicate rules found.");
                    return true;
                }

                var siteDataList = await QuerySiteDataList(duplicateRules);

                await _dataDao.AddSiteRotDataAsync(_jobInfo.O365TenantId, [.. siteDataList]);
                _logger.Info($"Successful add site duplicate rot data to db.");

                await AnalysisContainerData(siteDataList);

                await AnalysisBasicData(siteDataList);

                _logger.Info($"Finished analysis duplicate rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis duplicate rot data. Error: {e}");
                return false;
            }
        }

        private async Task<List<RMDiscoveryOffice365SiteRotData>> QuerySiteDataList(List<RMDiscoveryOffice365RuleInfo> duplicateRules)
        {
            var res = new List<RMDiscoveryOffice365SiteRotData>();

            var duplicateItemsList = await QuerySiteDuplicateDataList(duplicateRules);
            foreach (var duplicateItems in duplicateItemsList)
            {
                var odataUrl = $"{DiscoveryConfiguration.ODATA_URI[_contentSource]}?$apply=filter(SiteId eq '{_jobInfo.SiteId}' " +
                    $"and ItemId in ({string.Join(",", duplicateItems)}))" +
                    $"/groupby((FileExtension, {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME}, {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME})," +
                    $"aggregate($count as file_sum_count, FileSize with sum as file_total_size, HistoryVersionsSize with sum as file_history_version_total_size))";
                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(odataUrl, _jobInfo.O365TenantId.ToString());
                var dataObjList = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson);

                var fileExtensions = dataObjList.ConvertAll(item => (item.TryGet("_id") as ExpandoObject).GetValue("FileExtension")).ToHashSet();
                await _fileExtensionAnalysisManager.AddOrUpdateAsync([.. fileExtensions]);

                foreach (var dataObj in dataObjList)
                {
                    var groupedDataObj = dataObj.TryGet("_id") as ExpandoObject;

                    var sizeRange = groupedDataObj.GetValue<long>(RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME);
                    var withoutInDate = groupedDataObj.GetValue<long>(RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME);
                    var fileExtension = _fileExtensionAnalysisManager.GetId(groupedDataObj.GetValue("FileExtension"));
                    var fileSumCount = dataObj.GetValue<long>("file_sum_count");
                    var fileTotalSize = dataObj.GetValue<long>("file_total_size") + dataObj.GetValue<long>("file_history_version_total_size");

                    foreach (var duplicateRule in duplicateRules)
                    {
                        res.Add(new()
                        {
                            ContainerId = _containerInfo.Id,
                            SiteId = _siteInfo.Id,
                            WithoutInDate = (int)withoutInDate,
                            SizeRange = (int)sizeRange,
                            FileExtension = fileExtension,
                            Rule = duplicateRule.Id,
                            FileSumCount = fileSumCount,
                            FileTotalSize = fileTotalSize
                        });
                    }
                }
            }

            return res;
        }

        private async Task<List<List<long>>> QuerySiteDuplicateDataList(List<RMDiscoveryOffice365RuleInfo> duplicateRules)
        {
            var duplicateRule = duplicateRules.First();
            var duplicateTagColumn = duplicateRule.ToTagColumn();

            var duplicateDic = new Dictionary<int, List<long>>();

            const int pageSize = 1000;

            for (var i = 0; ; i++)
            {
                var odataUrl = $"{DiscoveryConfiguration.ODATA_URI[_contentSource]}?$top={pageSize}&$skip={i * pageSize}" +
                    $"&filter=(SiteId eq '{_jobInfo.SiteId}' and not IsPHL)" +
                    $"&select=ItemId,{duplicateTagColumn}";
                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(odataUrl, _jobInfo.O365TenantId.ToString());
                var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"];
                var dataObjList = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(values));

                dataObjList.ForEach(item =>
                {
                    var itemId = item.GetValue<long>("ItemId");
                    var duplicateValue = item.GetValue(duplicateTagColumn);
                    var duplicateValueHasCode = duplicateValue.GetHashCode();
                    if (!duplicateDic.TryGetValue(duplicateValueHasCode, out var itemIds))
                    {
                        itemIds = [];
                        duplicateDic.Add(duplicateValueHasCode, itemIds);
                    }

                    itemIds.Add(itemId);
                });

                if (dataObjList.Count < 1000)
                {
                    break;
                }
            }

            var res = duplicateDic.Where(item => item.Value.Count > 1).Select(item => item.Value).ToList();

            return res;
        }

        private async Task AnalysisContainerData(List<RMDiscoveryOffice365SiteRotData> siteDataList)
        {
            await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryAnalysisContainerRotData, $"{_jobInfo.O365TenantId}_{_jobInfo.ContainerId}", TimeSpan.FromMinutes(30)))
            {
                _logger.Info("Start analysis duplicate rot container data.");

                var existsContainerDataList = await _dataDao.GetContainerRotDataListAsync(_jobInfo.O365TenantId, _containerInfo.Id);

                foreach (var siteData in siteDataList)
                {
                    var existsContainerData = existsContainerDataList.FirstOrDefault(item =>
                        item.FileExtension == siteData.FileExtension &&
                        item.SizeRange == siteData.SizeRange &&
                        item.WithoutInDate == siteData.WithoutInDate &&
                        item.Rule == siteData.Rule
                    );
                    if (existsContainerData == null)
                    {
                        existsContainerData = new RMDiscoveryOffice365ContainerRotData
                        {
                            ContainerId = _containerInfo.Id,
                            WithoutInDate = siteData.WithoutInDate,
                            FileExtension = siteData.FileExtension,
                            SizeRange = siteData.SizeRange,
                            Rule = siteData.Rule
                        };
                        existsContainerDataList.Add(existsContainerData);
                    }
                    existsContainerData.FileTotalSize += siteData.FileTotalSize;
                    existsContainerData.FileSumCount += siteData.FileSumCount;
                }

                _logger.Info($"The amount of duplicate rot container data that needs to be added to the DB is [{existsContainerDataList.Count}].");
                await _dataDao.AddOrUpdateContainerRotDataAsync(_jobInfo.O365TenantId, [.. existsContainerDataList]);

                _logger.Info($"Successful add container duplicate rot data to db.");
            }
        }

        private async Task AnalysisBasicData(List<RMDiscoveryOffice365SiteRotData> siteDataList)
        {
            await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryAnalysisBasicRotData, _jobInfo.O365TenantId.ToString(), TimeSpan.FromMinutes(30)))
            {
                var existsBasicDataList = await _dataDao.GetBasicRotDataListAsync(_jobInfo.O365TenantId, _contentSource);
                foreach (var siteData in siteDataList)
                {
                    var existsBaicData = existsBasicDataList.FirstOrDefault(item =>
                        item.FileExtension == siteData.FileExtension &&
                        item.SizeRange == siteData.SizeRange &&
                        item.WithoutInDate == siteData.WithoutInDate &&
                        item.Rule == siteData.Rule
                    );
                    if (existsBaicData == null)
                    {
                        existsBaicData = new RMDiscoveryOffice365BasicRotData
                        {
                            WithoutInDate = siteData.WithoutInDate,
                            FileExtension = siteData.FileExtension,
                            SizeRange = siteData.SizeRange,
                            Rule = siteData.Rule,
                            ContentSource = _contentSource
                        };
                        existsBasicDataList.Add(existsBaicData);
                    }
                    existsBaicData.FileTotalSize += siteData.FileTotalSize;
                    existsBaicData.FileSumCount += siteData.FileSumCount;
                }

                _logger.Info($"The amount of duplicate rot basic data that needs to be added to the DB is [{existsBasicDataList.Count}].");
                await _dataDao.AddOrUpdateBasicRotDataAsync(_jobInfo.O365TenantId, _contentSource, [.. existsBasicDataList]);

                _logger.Info($"Successful add duplicate basic rot data to db.");
            }
        }
    }
}
