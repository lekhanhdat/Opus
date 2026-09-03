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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Google.Work.Extensions;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer;
using Cloud.Sdk.IE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.General
{
    public class RMDiscoveryGoogleAggregateTotalDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleAggregateTotalDataAnalyzer));

        private readonly IRMDiscoveryGoogleDataDao _dataDao;

        private readonly IRMDiscoveryGoogleNodeDao _nodeDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly string _organizationId;

        private readonly IEApiClient _ieApiClient;

        private readonly List<int> _sizeRangeIds;

        private readonly List<int> _dateRangeIds;

        private RMDiscoveryGoogleAggregateTotalData _data;

        private RMDiscoveryGoogleAggregateTotalData _memoryData;

        public RMDiscoveryGoogleAggregateTotalDataAnalyzer(
            string organizationId,
            RMDiscoveryJobType jobType,
            List<int> sizeRangeIds,
            List<int> dateRangeIds
        )
        {
            _dataDao = new RMDiscoveryGoogleDataDao();
            _nodeDao = new RMDiscoveryGoogleNodeDao();
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();

            _organizationId = organizationId;
            _jobType = jobType;
            _sizeRangeIds = sizeRangeIds;
            _dateRangeIds = dateRangeIds;
            _data = new()
            {
            };
            _memoryData = new()
            {
            };
        }

        public async Task<(bool analysisSucceed, RMDiscoveryGoogleAggregateTotalData data)> AnalysisAsync(string driveId)
        {
            try
            {
                var (fileSumCount, fileTotalSize, totalVersionSize) = await AnalysisTotalSizeDataAsync(driveId);
                var maxFileAge = await AnalysisMaxFileAgeAsync(driveId);

                _logger.Info($"The current drive [{driveId}] file sum count [{fileSumCount}], file total size [{fileTotalSize}], total version size [{totalVersionSize}], max file age [{maxFileAge}].");

                return (true, new()
                {
                    FileSumCount = fileSumCount,
                    FileTotalSize = fileTotalSize,
                    TotalVersionSize = totalVersionSize,
                    MaxFileAge = maxFileAge,
                });
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis drive [{driveId}] aggregate total data. Error: {e}");
                return (false, null);
            }
        }

        public void Memeory()
        {
            _memoryData = JsonConvert.DeserializeObject<RMDiscoveryGoogleAggregateTotalData>(JsonConvert.SerializeObject(_data));
        }

        public void Increse(RMDiscoveryGoogleAggregateTotalData data)
        {
            _data.FileSumCount += data.FileSumCount;
            _data.FileTotalSize += data.FileTotalSize;
            _data.TotalVersionSize += data.TotalVersionSize;
            _data.MaxFileAge = Math.Max(data.MaxFileAge, _data.MaxFileAge);
        }

        public void Fallback()
        {
            _data = _memoryData;
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                var data = await _dataDao.GetAggregateTotalDataAsync(_organizationId);

                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    return await RecalculateAndSaveAsync(data);
                }

                data.FileSumCount += _data.FileSumCount;
                data.FileTotalSize += _data.FileTotalSize;
                data.TotalVersionSize += _data.TotalVersionSize;
                data.MaxFileAge = Math.Max(data.MaxFileAge, _data.MaxFileAge);
                data.DuplicateFileTotalSize = -1;

                await _dataDao.AddOrUpdateAggregateTotalDataAsync(_organizationId, data);
                _logger.Info($"Succeed save organization [{_organizationId}] aggregate total data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while save organization [{_organizationId}] aggregate total data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RecalculateAndSaveAsync(RMDiscoveryGoogleAggregateTotalData data)
        {
            try
            {
                data.FileSumCount = 0;
                data.FileTotalSize = 0;
                data.TotalVersionSize = 0;
                data.MaxFileAge = 0;
                data.DuplicateFileTotalSize = -1;
                var containerInfoes = await _nodeDao.GetAllDiscoveryGoogleContainersAsync(_organizationId);
                foreach (var containerInfo in containerInfoes)
                {
                    data.FileSumCount += containerInfo.FileSumCount;
                    data.FileTotalSize += containerInfo.FileTotalSize;
                    data.TotalVersionSize += containerInfo.VersionTotalSize;
                    data.MaxFileAge = Math.Max(data.MaxFileAge, containerInfo.MaxFileAge);
                }

                await _dataDao.AddOrUpdateAggregateTotalDataAsync(_organizationId, data);
                _logger.Info($"Succeed recalculate and save organization [{_organizationId}] aggregate total data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save organization [{_organizationId}] aggregate total data. Error: {e}");
                return false;
            }

        }

        private async Task<(long FileSumCount, long FileTotalSize, long TotalVersionSize)> AnalysisTotalSizeDataAsync(string driveId)
        {
            try
            {
                (long FileSumCount, long FileTotalSize, long TotalVersionSize) res = (0L, 0L, 0L);

                foreach (var sizeRangeId in _sizeRangeIds)
                {
                    foreach (var dateRangeId in _dateRangeIds)
                    {
                        var totalDataObj = new ExpandoObject();

                        try
                        {
                            var sql = $"{RMDiscoveryGoogleAnalysisConfiguration.ODATA_URI}?$apply=filter(DriveId eq '{driveId}' " +
                            $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                            $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId})/" +
                            $"aggregate(" +
                            $"$count as file_sum_count, " +
                            $"FileSize with sum as file_total_size, " +
                            $"HistoryVersionsSize with sum as file_history_version_total_size)";

                            var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _organizationId.ToString(), "AnalysisTotalSizeData");
                            totalDataObj = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson).FirstOrDefault();
                        }
                        catch (Exception e)
                        {
                            _logger.Warn($"The query drive [{driveId}], size range [{sizeRangeId}], date range [{dateRangeId}] total data occur error. Error: {e}");
                        }

                        if (totalDataObj != null)
                        {
                            res.FileSumCount += totalDataObj.GetValue<long>("file_sum_count");
                            res.FileTotalSize += totalDataObj.GetValue<long>("file_total_size") + totalDataObj.GetValue<long>("file_history_version_total_size");
                            res.TotalVersionSize += totalDataObj.GetValue<long>("file_history_version_total_size");
                        }
                    }
                }

                return res;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis drive [{driveId}] total size data. Error: {e}");
                throw;
            }
        }

        private async Task<int> AnalysisMaxFileAgeAsync(string driveId)
        {
            try
            {
                var sql = $"{RMDiscoveryGoogleAnalysisConfiguration.ODATA_URI}?$filter=DriveId eq '{driveId}'&$orderby=CreatedMonth asc&$select=CreatedMonth&$top=1";
                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _organizationId.ToString(), "AnalysisMaxFileAge");
                var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"];
                var dataObj = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(values)).FirstOrDefault();
                if (dataObj != null)
                {
                    var nowYear = long.Parse(DateTime.UtcNow.Year.ToString());
                    var nowMonth = long.Parse(DateTime.UtcNow.Month.ToString());
                    var createTime = dataObj.GetValue<long>("CreatedMonth");
                    var createYear = createTime / 100;
                    var createMonth = createTime % 100;
                    return (int)((nowYear - createYear) * 12 + (nowMonth - createMonth));
                }

                return 0;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis drive [{driveId}] max file age. Error: {e}");
                throw;
            }
        }

        #region Need add item id

        //private async Task<ExpandoObject> ExpandQueryAggregateTotalDataAsync(string driveId, int sizeRangeId, int dateRangeId)
        //{
        //    _logger.Info($"Start expand query aggregate total data.");

        //    IDictionary<string, object> dataObj = new Dictionary<string, object>
        //        {
        //            { "file_sum_count", 0 },
        //            { "file_total_size", 0 },
        //            { "file_history_version_total_size", 0 }
        //        };

        //    const int pageSize = 1000;
        //    var maxItemId = 0L;

        //    for (var i = 0; ; i++)
        //    {
        //        var sql = $"{RMDiscoveryGoogleAnalysisConfiguration.ODATA_URI}?" +
        //            $"$top={pageSize}" +
        //            $"&filter=DriveId eq '{driveId}' " +
        //            $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
        //            $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
        //            $"and ItemId gt {maxItemId} " +
        //            $"&$orderby=ItemId " +
        //            $"&select=ItemId, FileSize, FileExtension, HistoryVersionsSize";
        //        var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _organizationId.ToString());
        //        var items = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"]));
        //        foreach (var item in items)
        //        {
        //            dataObj["file_sum_count"] = Convert.ToInt64(dataObj["file_sum_count"]) + 1;
        //            dataObj["file_total_size"] = Convert.ToInt64(dataObj["file_total_size"]) + item.GetValue<long>("FileSize");
        //            dataObj["file_history_version_total_size"] = Convert.ToInt64(dataObj["file_history_version_total_size"]) + item.GetValue<long>("HistoryVersionsSize");
        //        }

        //        if (items.Count == 0 || items.Count < pageSize)
        //        {
        //            break;
        //        }

        //        maxItemId = items.Last().GetValue<long>("ItemId");
        //    }

        //    _logger.Info($"End expand query aggregate total data.");
        //    return dataObj.ConvertToExpandoObject();
        //}

        #endregion
    }
}
