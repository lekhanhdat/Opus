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
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Common;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.Service.Services.Discovery.Google.Work.Extensions;
using Cloud.Sdk.IE;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.General.Rot
{
    public class RMDiscoveryGoogleDriveRotDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleDriveRotDataAnalyzer));

        private readonly IEApiClient _ieApiClient;

        private readonly IRMDiscoveryGoogleDataDao _dataDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly string _googleOrganizationId;

        private readonly int _containerId;

        private readonly int _driveIntId;

        private readonly string _driveStringId;

        private readonly List<int> _sizeRangeIds;

        private readonly List<int> _dateRangeIds;

        private readonly List<RMDiscoveryGoogleRuleInfo> _rules;

        private readonly RMDiscoveryGoogleFileExtensionAnalysisManager _fileExtensionManager;

        public RMDiscoveryGoogleDriveRotDataAnalyzer(
          RMDiscoveryJobType jobType,
          string googleOrganizationId,
          int containerId,
          int driveIntId,
          string driveStringId,
          List<int> sizeRangeIds,
          List<int> dateRangeIds,
          List<RMDiscoveryGoogleRuleInfo> rules,
          RMDiscoveryGoogleFileExtensionAnalysisManager fileExtensionManager
        )
        {
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
            _dataDao = new RMDiscoveryGoogleDataDao();
            _jobType = jobType;
            _googleOrganizationId = googleOrganizationId;
            _containerId = containerId;
            _driveIntId = driveIntId;
            _driveStringId = driveStringId;
            _sizeRangeIds = sizeRangeIds;
            _dateRangeIds = dateRangeIds;
            _rules = rules.Where(item => item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ToList();
            _fileExtensionManager = fileExtensionManager;
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryGoogleDriveRuleLevelRotData> dataList)> AnalysisRuleLevelAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    await _dataDao.DeleteDriveRuleLevelRotDataListAsync(_googleOrganizationId, _driveIntId);
                    _logger.Info($"Successful delete tenant [{_googleOrganizationId}] drive [{_driveIntId}] rule Level rot data.");
                }

                var dataList = await AnalysisRuleLevelDataAsync();

                await _dataDao.AddDriveRuleLevelRotDataListAsync(_googleOrganizationId, dataList);

                _logger.Info($"Successful analysis tenant [{_googleOrganizationId}] drive [{_driveIntId}] rule level rot data, count [{dataList.Count}].");

                return (true, dataList);
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while analysis tenant [{_googleOrganizationId}] drive [{_driveIntId}] rule level rot data. Error: {e}");
                return (false, []);
            }
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryGoogleDriveCategoryLevelRotData> dataList)> AnalysisCategoryLevelAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    await _dataDao.DeleteDriveCategoryLevelRotDataListAsync(_googleOrganizationId, _driveIntId);
                    _logger.Info($"Successful delete tenant [{_googleOrganizationId}] drive [{_driveIntId}] category Level rot data.");
                }

                var dataList = await AnalysisCategoryLevelDataAsync();

                await _dataDao.AddDriveCategoryLevelRotDataListAsync(_googleOrganizationId, dataList);

                _logger.Info($"Successful analysis tenant [{_googleOrganizationId}] drive [{_driveIntId}] category level rot data, count [{dataList.Count}].");

                return (true, dataList);
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while analysis tenant [{_googleOrganizationId}] drive [{_driveIntId}] category level rot data. Error: {e}");
                return (false, []);
            }
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryGoogleDriveRootLevelRotData> dataList)> AnalysisRootLevelAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    await _dataDao.DeleteDriveRootLevelRotDataListAsync(_googleOrganizationId, _driveIntId);
                    _logger.Info($"Successful delete tenant [{_googleOrganizationId}] drive [{_driveIntId}] root Level rot data.");
                }

                var dataList = await AnalysisRootLevelDataAsync();

                await _dataDao.AddDriveRootLevelRotDataListAsync(_googleOrganizationId, dataList);

                _logger.Info($"Successful analysis tenant [{_googleOrganizationId}] drive [{_driveIntId}] root level rot data, count [{dataList.Count}].");

                return (true, dataList);
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while analysis tenant [{_googleOrganizationId}] drive [{_driveIntId}] root level rot data. Error: {e}");
                return (false, []);
            }
        }


        #region Rule Level
        private async Task<List<RMDiscoveryGoogleDriveRuleLevelRotData>> AnalysisRuleLevelDataAsync()
        {
            var rotRulesWithoutDuplicateColumns = _rules.ConvertAll(item => (
                item.Id,
                IsVersionRule: item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version,
                TagNames: "tags_" + item.UniqueId.ToString().ToLower().Replace("-", ""),
                ColumnName: "tags_" + item.UniqueId.ToString().ToLower().Replace("-", "") + (item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Version ? "/total_size" : ""),
                SumName: item.ToCustomColumn().Name
            )).ToList();

            var driveDataList = new List<RMDiscoveryGoogleDriveRuleLevelRotData>();

            await foreach (var (dataObjList, sizeRangeId, dateRangeId, ruleId) in QueryDriveRuleLevelDataObjects(rotRulesWithoutDuplicateColumns))
            {
                var fileExtensions = dataObjList.ConvertAll(item => (item.TryGet("_id") as ExpandoObject).GetValue("FileExtension")).ToHashSet();
                await _fileExtensionManager.AddOrUpdateAsync(fileExtensions.ToArray());

                foreach (var dataObj in dataObjList)
                {
                    var groupedDataObj = dataObj.TryGet("_id") as ExpandoObject;

                    var withoutInDate = dateRangeId;
                    var sizeRange = sizeRangeId;
                    var fileExtension = _fileExtensionManager.GetId(groupedDataObj.GetValue("FileExtension"));

                    var ruleFileTotalSize = dataObj.GetValue<long>("file_total_size");
                    if (ruleFileTotalSize == 0)
                    {
                        continue;
                    }

                    var exstisDataItem = driveDataList.FirstOrDefault(item =>
                        item.WithoutInDate == withoutInDate &&
                        item.SizeRange == sizeRange &&
                        item.FileExtension == fileExtension &&
                        item.Rule == ruleId);
                    if (exstisDataItem == null)
                    {
                        exstisDataItem = new RMDiscoveryGoogleDriveRuleLevelRotData
                        {
                            ContainerId = _containerId,
                            DriveId = _driveIntId,
                            WithoutInDate = withoutInDate,
                            SizeRange = sizeRange,
                            FileExtension = fileExtension,
                            Rule = ruleId
                        };
                        driveDataList.Add(exstisDataItem);
                    }

                    exstisDataItem.FileTotalSize += ruleFileTotalSize;
                    exstisDataItem.FileSumCount += dataObj.GetValue<long>("file_sum_count");
                }
            }

            return driveDataList;
        }

        private async IAsyncEnumerable<(List<ExpandoObject> dataObjList, int sizeRangeId, int dateRangeId, int ruleId)> QueryDriveRuleLevelDataObjects(List<(int Id, bool IsVersionRule, string TagName, string ColumnName, string SumName)> rotRulesWithoutDuplicateColumns)
        {
            foreach (var sizeRangeId in _sizeRangeIds)
            {
                foreach (var dateRangeId in _dateRangeIds)
                {
                    foreach (var (Id, IsVersionRule, TagName, ColumnName, SumName) in rotRulesWithoutDuplicateColumns)
                    {
                        List<ExpandoObject> dataObjList;
                        try
                        {
                            var sql = $"{RMDiscoveryGoogleAnalysisConfiguration.ODATA_URI}?$apply=filter(DriveId eq '{_driveStringId}' " +
                                $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                                $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                                $"and {ColumnName} gt 0)" +
                                $"/groupby((FileExtension)," +
                                $"aggregate($count as file_sum_count, {ColumnName} with sum as file_total_size))";
                            var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _googleOrganizationId);
                            dataObjList = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson);

                            _logger.Info($"End query drive [{_driveStringId}], size range [{sizeRangeId}], date range [{dateRangeId}], rule [{Id}] rule level rot data.");
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"An error occurred while query drive [{_driveStringId}], size range [{sizeRangeId}], date range [{dateRangeId}], rule [{Id}] rule level rot data. Error: {e}");
                            dataObjList = new();
                        }
                        yield return (dataObjList, sizeRangeId, dateRangeId, Id);
                    }
                }
            }
        }

        #endregion

        #region Category Level

        private async Task<List<RMDiscoveryGoogleDriveCategoryLevelRotData>> AnalysisCategoryLevelDataAsync()
        {

            var driveDataList = new List<RMDiscoveryGoogleDriveCategoryLevelRotData>();

            await foreach (var (dataObjList, sizeRangeId, dateRangeId, category) in QueryDriveCategoryLevelDataObjects())
            {
                var fileExtensions = dataObjList.ConvertAll(item => (item.TryGet("_id") as ExpandoObject).GetValue("FileExtension")).ToHashSet();
                await _fileExtensionManager.AddOrUpdateAsync(fileExtensions.ToArray());

                foreach (var dataObj in dataObjList)
                {
                    var groupedDataObj = dataObj.TryGet("_id") as ExpandoObject;

                    var withoutInDate = dateRangeId;
                    var sizeRange = sizeRangeId;
                    var fileExtension = _fileExtensionManager.GetId(groupedDataObj.GetValue("FileExtension"));

                    var ruleFileTotalSize = dataObj.GetValue<long>("file_total_size");
                    if (ruleFileTotalSize == 0)
                    {
                        continue;
                    }

                    var exstisDataItem = driveDataList.FirstOrDefault(item =>
                        item.WithoutInDate == withoutInDate &&
                        item.SizeRange == sizeRange &&
                        item.FileExtension == fileExtension &&
                        item.Category == category);
                    if (exstisDataItem == null)
                    {
                        exstisDataItem = new RMDiscoveryGoogleDriveCategoryLevelRotData
                        {
                            ContainerId = _containerId,
                            DriveId = _driveIntId,
                            WithoutInDate = withoutInDate,
                            SizeRange = sizeRange,
                            FileExtension = fileExtension,
                            Category = category
                        };
                        driveDataList.Add(exstisDataItem);
                    }

                    exstisDataItem.FileTotalSize += ruleFileTotalSize;
                    exstisDataItem.FileSumCount += dataObj.GetValue<long>("file_sum_count");
                }
            }

            return driveDataList;
        }

        private async IAsyncEnumerable<(List<ExpandoObject> dataObjList, int sizeRangeId, int dateRangeId, RMDiscoveryRuleCategory category)> QueryDriveCategoryLevelDataObjects()
        {
            foreach (var sizeRangeId in _sizeRangeIds)
            {
                foreach (var dateRangeId in _dateRangeIds)
                {
                    foreach (var category in RMDiscoveryGoogleAnalysisConfiguration.ROT_CATEGORY_COLUMN_MAPPING.Keys.ToList())
                    {
                        List<ExpandoObject> dataObjList;
                        var tagName = RMDiscoveryGoogleAnalysisConfiguration.ROT_CATEGORY_COLUMN_MAPPING[category];
                        try
                        {
                            var sql = $"{RMDiscoveryGoogleAnalysisConfiguration.ODATA_URI}?$apply=filter(DriveId eq '{_driveStringId}' " +
                                $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                                $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                                $"and {tagName} gt 0 )" +
                                $"/groupby((FileExtension)," +
                                $"aggregate($count as file_sum_count, {tagName} with sum as file_total_size))";
                            var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _googleOrganizationId);
                            dataObjList = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson);

                            _logger.Info($"End query drive [{_driveStringId}], size range [{sizeRangeId}], date range [{dateRangeId}], category [{category}] category level rot data.");
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"An error occurred while query drive [{_driveStringId}], size range [{sizeRangeId}], date range [{dateRangeId}], category [{category}] category level rot data. Error: {e}");
                            dataObjList = new();
                        }
                        yield return (dataObjList, sizeRangeId, dateRangeId, category);
                    }
                }
            }
        }

        #endregion

        #region Root Level

        private async Task<List<RMDiscoveryGoogleDriveRootLevelRotData>> AnalysisRootLevelDataAsync()
        {

            var driveDataList = new List<RMDiscoveryGoogleDriveRootLevelRotData>();

            await foreach (var (dataObjList, sizeRangeId, dateRangeId) in QueryDriveRootLevelDataObjects())
            {
                var fileExtensions = dataObjList.ConvertAll(item => (item.TryGet("_id") as ExpandoObject).GetValue("FileExtension")).ToHashSet();
                await _fileExtensionManager.AddOrUpdateAsync(fileExtensions.ToArray());

                foreach (var dataObj in dataObjList)
                {
                    var groupedDataObj = dataObj.TryGet("_id") as ExpandoObject;

                    var withoutInDate = dateRangeId;
                    var sizeRange = sizeRangeId;
                    var fileExtension = _fileExtensionManager.GetId(groupedDataObj.GetValue("FileExtension"));

                    var ruleFileTotalSize = dataObj.GetValue<long>("file_total_size");
                    if (ruleFileTotalSize == 0)
                    {
                        continue;
                    }

                    var exstisDataItem = driveDataList.FirstOrDefault(item =>
                        item.WithoutInDate == withoutInDate &&
                        item.SizeRange == sizeRange &&
                        item.FileExtension == fileExtension);
                    if (exstisDataItem == null)
                    {
                        exstisDataItem = new RMDiscoveryGoogleDriveRootLevelRotData
                        {
                            ContainerId = _containerId,
                            DriveId = _driveIntId,
                            WithoutInDate = withoutInDate,
                            SizeRange = sizeRange,
                            FileExtension = fileExtension,
                        };
                        driveDataList.Add(exstisDataItem);
                    }

                    exstisDataItem.FileTotalSize += ruleFileTotalSize;
                    exstisDataItem.FileSumCount += dataObj.GetValue<long>("file_sum_count");
                }
            }

            return driveDataList;
        }

        private async IAsyncEnumerable<(List<ExpandoObject> dataObjList, int sizeRangeId, int dateRangeId)> QueryDriveRootLevelDataObjects()
        {
            foreach (var sizeRangeId in _sizeRangeIds)
            {
                foreach (var dateRangeId in _dateRangeIds)
                {
                    List<ExpandoObject> dataObjList;
                    try
                    {
                        var sql = $"{RMDiscoveryGoogleAnalysisConfiguration.ODATA_URI}?$apply=filter(DriveId eq '{_driveStringId}' " +
                            $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                            $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                            $"and {RMDiscoveryBuildInRule.ROT_RULE_NAME} gt 0 )" +
                            $"/groupby((FileExtension)," +
                            $"aggregate($count as file_sum_count, {RMDiscoveryBuildInRule.ROT_RULE_NAME} with sum as file_total_size))";
                        var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _googleOrganizationId);
                        dataObjList = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson);

                        _logger.Info($"End query drive [{_driveStringId}], size range [{sizeRangeId}], date range [{dateRangeId}], root level rot data.");
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"An error occurred while query drive [{_driveStringId}], size range [{sizeRangeId}], date range [{dateRangeId}], root level rot data. Error: {e}");
                        dataObjList = new();
                    }
                    yield return (dataObjList, sizeRangeId, dateRangeId);
                }
            }
        }

        #endregion
    }
}
