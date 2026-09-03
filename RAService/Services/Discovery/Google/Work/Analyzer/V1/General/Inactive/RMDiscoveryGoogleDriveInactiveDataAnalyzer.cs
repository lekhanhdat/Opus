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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.Service.Services.Discovery.Google.Work.Extensions;
using Cloud.Sdk.IE;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.General.Inactive
{
    public class RMDiscoveryGoogleDriveInactiveDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleDriveInactiveDataAnalyzer));

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

        public RMDiscoveryGoogleDriveInactiveDataAnalyzer(
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
            _rules = rules;
            _fileExtensionManager = fileExtensionManager;
        }

        public async Task<(bool analysisSucceed, List<RMDiscoveryGoogleDriveInactiveData> dataList)> AnalysisAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    await _dataDao.DeleteDriveInactiveDataListAsync(_googleOrganizationId, _driveIntId);
                    _logger.Info($"Successful delete tenant [{_googleOrganizationId}] drive [{_driveStringId}] inactive data.");
                }

                var dataList = await AnalysisDataAsync();

                await _dataDao.AddDriveInactiveDataListAsync(_googleOrganizationId, dataList.ToArray());

                _logger.Info($"Successful analysis tenant [{_googleOrganizationId}] drive [{_driveStringId}] inactive data, count [{dataList.Count}].");

                return (true, dataList);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis tenant [{_googleOrganizationId}] drive [{_driveStringId}] inactive data. Error: {e}");
                return (false, []);
            }
        }

        private async Task<List<RMDiscoveryGoogleDriveInactiveData>> AnalysisDataAsync()
        {
            List<(string ColumnName, string SumName)> inactiveVersionRulesColumns = _rules.ConvertAll(item => (
                "tags_" + item.UniqueId.ToString().ToLower().Replace("-", ""),
                item.ToCustomColumn().Name
            ));

            var inactiveVersionRulesSumSql = inactiveVersionRulesColumns.ConvertAll(item => $"{item.ColumnName}/total_size with sum as {item.SumName}");

            var driveDataList = new List<RMDiscoveryGoogleDriveInactiveData>();

            await foreach (var (dataObjList, sizeRangeId, dateRangeId) in QueryDriveDataObjects(inactiveVersionRulesSumSql, inactiveVersionRulesColumns))
            {
                var fileExtensions = dataObjList.ConvertAll(item => (item.TryGet("_id") as ExpandoObject).GetValue("FileExtension")).ToHashSet();
                await _fileExtensionManager.AddOrUpdateAsync(fileExtensions.ToArray());

                foreach (var dataObj in dataObjList)
                {
                    var groupedDataObj = dataObj.TryGet("_id") as ExpandoObject;

                    var withoutInDate = dateRangeId;
                    var sizeRange = sizeRangeId;
                    var fileExtension = _fileExtensionManager.GetId(groupedDataObj.GetValue("FileExtension"));
                    var exstisDataItem = driveDataList.FirstOrDefault(item =>
                        item.WithoutInDate == withoutInDate &&
                        item.SizeRange == sizeRange &&
                        item.FileExtension == fileExtension);
                    if (exstisDataItem == null)
                    {
                        exstisDataItem = new RMDiscoveryGoogleDriveInactiveData
                        {
                            ContainerId = _containerId,
                            DriveId = _driveIntId,
                            WithoutInDate = withoutInDate,
                            SizeRange = sizeRange,
                            FileExtension = fileExtension,
                        };
                        foreach (var inactiveVersionRuleColumn in inactiveVersionRulesColumns)
                        {
                            exstisDataItem.CustomColumns.Add(new RMDiscoveryCustomColumnWithValue(
                                inactiveVersionRuleColumn.SumName,
                                0L,
                                typeof(long)
                                )
                            );
                        }
                        driveDataList.Add(exstisDataItem);
                    }

                    exstisDataItem.FileTotalSize += dataObj.GetValue<long>("file_total_size") + dataObj.GetValue<long>("file_history_version_total_size");
                    exstisDataItem.FileSumCount += dataObj.GetValue<long>("file_sum_count");
                    foreach (var inactiveVersionRuleColumn in inactiveVersionRulesColumns)
                    {
                        var customColumn = exstisDataItem.CustomColumns.First(item => item.Name == inactiveVersionRuleColumn.SumName);
                        customColumn.Value = Convert.ToInt64(customColumn.Value) + dataObj.GetValue<long>(inactiveVersionRuleColumn.SumName);
                    }
                }
            }
            return driveDataList;
        }

        private async IAsyncEnumerable<(List<ExpandoObject> dataObjList, int sizeRangeId, int dateRangeId)> QueryDriveDataObjects(List<string> inactiveVersionRulesSumSql, List<(string ColumnName, string SumName)> inactiveVersionRulesColumns)
        {
            foreach (var sizeRangeId in _sizeRangeIds)
            {
                foreach (var dateRangeId in _dateRangeIds)
                {
                    _logger.Info($"Start query drive [{_driveStringId}], size range [{sizeRangeId}], date range [{dateRangeId}] inactive data.");

                    List<ExpandoObject> dataObjList;
                    try
                    {
                        var sql = $"{RMDiscoveryGoogleAnalysisConfiguration.ODATA_URI}?$apply=filter(DriveId eq '{_driveStringId}' " +
                        $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                        $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId})" +
                        $"/groupby((FileExtension)," +
                        $"aggregate($count as file_sum_count, FileSize with sum as file_total_size, HistoryVersionsSize with sum as file_history_version_total_size {(inactiveVersionRulesSumSql.Any() ? "," + string.Join(", ", inactiveVersionRulesSumSql) : "")}))";
                        var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _googleOrganizationId.ToString());
                        dataObjList = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson);
                    }
                    catch (Exception e)
                    {
                        _logger.Warn($"An error occurred while query drive [{_driveStringId}], size range [{sizeRangeId}], date range [{dateRangeId}] inactive data. Error: {e}");
                        dataObjList = new();
                    }

                    yield return (dataObjList, sizeRangeId, dateRangeId);

                    _logger.Info($"End query drive [{_driveStringId}], size range [{sizeRangeId}], date range [{dateRangeId}] inactive data.");
                }
            }
        }
    }
}
