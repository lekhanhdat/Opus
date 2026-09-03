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
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.RADataBroker.Common;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Extensions;
using Cloud.Sdk.IE;
using Microsoft.OData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V2
{
    public class RMDiscoveryOffice365AggregateTotalDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365AggregateTotalDataAnalyzer));

        private readonly IRMDiscoveryOffice365DataDao _dataDao;

        private readonly IEApiClient _ieApiClient;

        private readonly Guid _o365TenantId;

        private readonly SourceFlag _contentSource;

        private readonly List<int> _sizeRangeIds;

        private readonly List<int> _dateRangeIds;

        private RMDiscoveryOffice365AggregateTotalData _data;

        private RMDiscoveryOffice365AggregateTotalData _memoryData;

        private readonly bool _enableExpandQueryTest;

        public RMDiscoveryOffice365AggregateTotalDataAnalyzer(
                Guid o365TenantId,
                SourceFlag contentSource,
                List<int> sizeRangeIds,
                List<int> dateRangeIds,
                bool enableExpandQueryTest
            )
        {
            _dataDao = new RMDiscoveryOffice365DataDao();
            _ieApiClient = AosApiUtility.GetInsightsEngineApiClient();
            
            _o365TenantId = o365TenantId;
            _contentSource = contentSource;
            _sizeRangeIds = sizeRangeIds;
            _dateRangeIds = dateRangeIds;
            _data = new()
            {
                ContentSource = contentSource
            };
            _memoryData = new()
            {
                ContentSource = contentSource
            };
            _enableExpandQueryTest = enableExpandQueryTest;
        }

        public async Task<(bool analysisSucceed, RMDiscoveryOffice365AggregateTotalData data)> AnalysisAsync(Guid siteId, List<Guid> listIds)
        {
            try
            {
                var (fileSumCount, fileTotalSize, totalVersionSize) = await AnalysisTotalSizeDataAsync(siteId, listIds);
                var maxFileAge = await AnalysisMaxFileAgeAsync(siteId);
                var phlVolume = await AnalysisPHLVolumeAsync(siteId);

                _logger.Info($"The current site [{siteId}] file sum count [{fileSumCount}], file total size [{fileTotalSize}], total version size [{totalVersionSize}], max file age [{maxFileAge}], phl volume [{phlVolume}].");

                return (true, new()
                {
                    ContentSource = _contentSource,
                    FileSumCount = fileSumCount,
                    FileTotalSize = fileTotalSize,
                    TotalVersionSize = totalVersionSize,
                    MaxFileAge = maxFileAge,
                    PHLVolume = phlVolume,
                });
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis site [{siteId}] aggregate total data. Error: {e}");
                return (false, null);
            }
        }
        
        public void Memeory()
        {
            _memoryData = JsonConvert.DeserializeObject<RMDiscoveryOffice365AggregateTotalData>(JsonConvert.SerializeObject(_data));
        }

        public void Increse(RMDiscoveryOffice365AggregateTotalData data)
        {
            _data.FileSumCount += data.FileSumCount;
            _data.FileTotalSize += data.FileTotalSize;
            _data.TotalVersionSize += data.TotalVersionSize;
            _data.MaxFileAge = Math.Max(data.MaxFileAge, _data.MaxFileAge);
            _data.PHLVolume += data.PHLVolume;
        }

        public void Fallback()
        {
            _data = _memoryData;
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                var data = await _dataDao.GetAggregateTotalDataAsync(_o365TenantId, _contentSource);

                data.FileSumCount += _data.FileSumCount;
                data.FileTotalSize += _data.FileTotalSize;
                data.TotalVersionSize += _data.TotalVersionSize;
                data.PHLVolume += _data.PHLVolume;
                data.MaxFileAge = Math.Max(data.MaxFileAge, _data.MaxFileAge);

                await _dataDao.AddOrUpdateAggregateTotalDataAsync(_o365TenantId, data);
                _logger.Info($"Succeed save tenant [{_o365TenantId}] [{_contentSource}] aggregate total data.");

                return true;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while save tenant [{_o365TenantId}] [{_contentSource}] aggregate total data. Error: {e}");
                return false;
            }
        }

        private async Task<(long FileSumCount, long FileTotalSize, long TotalVersionSize)> AnalysisTotalSizeDataAsync(Guid siteId, List<Guid> listIds)
        {
            try
            {
                (long FileSumCount, long FileTotalSize, long TotalVersionSize) res = (0L, 0L, 0L);

                foreach (var listId in listIds)
                {
                    foreach (var sizeRangeId in _sizeRangeIds)
                    {
                        foreach (var dateRangeId in _dateRangeIds)
                        {
                            var totalDataObj = new ExpandoObject();

                            try
                            {
                                if (_enableExpandQueryTest)
                                {
                                    throw new Exception("Enable expand query test.");
                                }

                                var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_contentSource]}?$apply=filter(SiteId eq '{siteId}' " +
                                $"and ListId eq '{listId}' " +
                                $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                                $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                                $"and not IsPHL)/" +
                                $"aggregate(" +
                                $"$count as file_sum_count, " +
                                $"FileSize with sum as file_total_size, " +
                                $"HistoryVersionsSize with sum as file_history_version_total_size)";

                                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _o365TenantId.ToString(), "AnalysisTotalSizeData");
                                totalDataObj = JsonConvert.DeserializeObject<List<ExpandoObject>>(dataJson).FirstOrDefault();
                            }
                            catch (Exception e)
                            {
                                _logger.Warn($"The query site [{siteId}] list [{listId}], size range [{sizeRangeId}], date range [{dateRangeId}] total data occur error. Error: {e}");
                                totalDataObj = await ExpandQueryAggregateTotalDataAsync(siteId, listId, sizeRangeId, dateRangeId);
                            }

                            if (totalDataObj != null)
                            {
                                res.FileSumCount += totalDataObj.GetValue<long>("file_sum_count");
                                res.FileTotalSize += totalDataObj.GetValue<long>("file_total_size") + totalDataObj.GetValue<long>("file_history_version_total_size");
                                res.TotalVersionSize += totalDataObj.GetValue<long>("file_history_version_total_size");
                            }
                        }
                    }
                }

                return res;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while analysis site [{siteId}] total size data. Error: {e}");
                throw;
            }
        }

        private async Task<int> AnalysisMaxFileAgeAsync(Guid siteId)
        {
            try
            {
                var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_contentSource]}?$filter=SiteId eq '{siteId}' and not IsPHL&$orderby=CreatedMonth asc&$select=CreatedMonth&$top=1";
                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _o365TenantId.ToString(), "AnalysisMaxFileAge");
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
                _logger.Error($"An error occurred while analysis site [{siteId}] max file age. Error: {e}");
                throw;
            }
        }

        private async Task<long> AnalysisPHLVolumeAsync(Guid siteId)
        {
            try
            {
                var phlSql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_contentSource]}?$filter=SiteId eq '{siteId}' and IsPHL &$apply=groupby((SiteId, IsPHL)," +
                    $"aggregate(" +
                    $"FileSize with sum as file_total_size, " +
                    $"HistoryVersionsSize with sum as file_history_version_total_size))";
                var phlDataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(phlSql, _o365TenantId.ToString());
                var phlDataObj = JsonConvert.DeserializeObject<List<ExpandoObject>>(phlDataJson).FirstOrDefault();
                if (phlDataObj != null)
                {
                    return phlDataObj.GetValue<long>("file_total_size") + phlDataObj.GetValue<long>("file_history_version_total_size");
                }
                return 0L;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while analysis site [{siteId}] PHL volume. Error: {e}");
                throw;
            }
        }

        private async Task<ExpandoObject> ExpandQueryAggregateTotalDataAsync(Guid siteId, Guid listId, int sizeRangeId, int dateRangeId)
        {
            _logger.Info($"Start expand query aggregate total data.");

            IDictionary<string, object> dataObj = new Dictionary<string, object>
                {
                    { "file_sum_count", 0 },
                    { "file_total_size", 0 },
                    { "file_history_version_total_size", 0 }
                };

            const int pageSize = 1000;
            var maxItemId = 0L;

            for (var i = 0; ; i++)
            {
                var sql = $"{RMDiscoveryOffice365AnalysisConfiguration.ODATA_URI[_contentSource]}?" +
                    $"$top={pageSize}" +
                    $"&filter=SiteId eq '{siteId}' " +
                    $"and ListId eq '{listId}' " +
                    $"and {RMDiscoveryBuildInRule.SIZE_RANGE_COLUMN_NAME} eq {sizeRangeId} " +
                    $"and {RMDiscoveryBuildInRule.WITHOUT_IN_DATE_COLUMN_NAME} eq {dateRangeId} " +
                    $"and ItemId gt {maxItemId} " +
                    $"and not IsPHL " +
                    $"&$orderby=ItemId " +
                    $"&select=ItemId, FileSize, FileExtension, HistoryVersionsSize";
                var dataJson = await _ieApiClient.GetByODataUrlWithRetryAsync(sql, _o365TenantId.ToString());
                var items = JsonConvert.DeserializeObject<List<ExpandoObject>>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<Dictionary<string, object>>(dataJson)["value"]));
                foreach (var item in items)
                {
                    dataObj["file_sum_count"] = Convert.ToInt64(dataObj["file_sum_count"]) + 1;
                    dataObj["file_total_size"] = Convert.ToInt64(dataObj["file_total_size"]) + item.GetValue<long>("FileSize");
                    dataObj["file_history_version_total_size"] = Convert.ToInt64(dataObj["file_history_version_total_size"]) + item.GetValue<long>("HistoryVersionsSize");
                }

                if (items.Count == 0 || items.Count < pageSize)
                {
                    break;
                }

                maxItemId = items.Last().GetValue<long>("ItemId");
            }

            _logger.Info($"End expand query aggregate total data.");
            return dataObj.ConvertToExpandoObject();
        }
    }
}
