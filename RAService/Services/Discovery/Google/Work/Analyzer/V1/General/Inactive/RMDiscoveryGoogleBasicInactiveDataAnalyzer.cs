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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Google;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.General.Inactive
{
    public class RMDiscoveryGoogleBasicInactiveDataAnalyzer
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleBasicInactiveDataAnalyzer));

        private readonly IRMDiscoveryGoogleDataDao _dataDao;

        private readonly IRMDiscoveryGoogleNodeDao _nodeDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly string _googleOrganizationId;

        private readonly List<RMDiscoveryGoogleRuleInfo> _rules;

        private readonly List<RMDiscoveryGoogleBasicInactiveData> _dataList;

        public RMDiscoveryGoogleBasicInactiveDataAnalyzer(
            RMDiscoveryJobType jobType,
            string googleOrganizationId,
            List<RMDiscoveryGoogleRuleInfo> rules
        )
        {
            _dataDao = new RMDiscoveryGoogleDataDao();
            _nodeDao = new RMDiscoveryGoogleNodeDao();
            _jobType = jobType;
            _googleOrganizationId = googleOrganizationId;
            _rules = rules;
            _dataList = [];
        }

        public void Increse(List<RMDiscoveryGoogleDriveInactiveData> driveDataList)
        {
            if (_jobType == RMDiscoveryJobType.Retry)
            {
                return;
            }

            var inactiveColumns = _rules.ConvertAll(item => item.ToCustomColumn());

            foreach (var driveData in driveDataList)
            {
                var data = _dataList.FirstOrDefault(item =>
                    item.FileExtension == driveData.FileExtension &&
                    item.SizeRange == driveData.SizeRange &&
                    item.WithoutInDate == driveData.WithoutInDate
                );
                if (data == null)
                {
                    data = new RMDiscoveryGoogleBasicInactiveData
                    {
                        WithoutInDate = driveData.WithoutInDate,
                        FileExtension = driveData.FileExtension,
                        SizeRange = driveData.SizeRange,
                    };
                    foreach (var inactiveColumn in inactiveColumns)
                    {
                        data.CustomColumns.Add(new RMDiscoveryCustomColumnWithValue(inactiveColumn.Name, 0, typeof(long)));
                    }

                    _dataList.Add(data);
                }

                foreach (var inactiveColumn in inactiveColumns)
                {
                    var driveColumnValue = driveData.CustomColumns.First(item => item.Name == inactiveColumn.Name);
                    var baicColumnValue = data.CustomColumns.First(item => item.Name == inactiveColumn.Name);
                    baicColumnValue.Value = long.Parse(baicColumnValue.Value.ToString()) + long.Parse(driveColumnValue.Value.ToString());
                }

                data.FileTotalSize += driveData.FileTotalSize;
                data.FileSumCount += driveData.FileSumCount;
            }
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Append)
                {
                    return await AppendAndSaveAsync();
                }
                else if (_jobType == RMDiscoveryJobType.Retry)
                {
                    return await RecalculateAndSaveAsync();
                }

                await _dataDao.AddOrUpdateBasicInactiveDataAsync(_googleOrganizationId, _dataList.ToArray());

                _logger.Info($"Succeed save tenant [{_googleOrganizationId}]  [{_dataList.Count}] basic inactive data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while save tenant [{_googleOrganizationId}] [{_dataList.Count}] basic inactive data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> AppendAndSaveAsync()
        {
            try
            {
                var inactiveColumns = _rules.ConvertAll(item => item.ToCustomColumn());
                var existsDataList = await _dataDao.GetBasicInactiveDataListAsync(_googleOrganizationId, inactiveColumns);
                foreach (var data in _dataList)
                {
                    var existsBaicData = existsDataList.FirstOrDefault(item =>
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange &&
                        item.WithoutInDate == data.WithoutInDate
                    );
                    if (existsBaicData == null)
                    {
                        existsBaicData = new RMDiscoveryGoogleBasicInactiveData
                        {
                            WithoutInDate = data.WithoutInDate,
                            FileExtension = data.FileExtension,
                            SizeRange = data.SizeRange,
                        };
                        foreach (var inactiveColumn in inactiveColumns)
                        {
                            existsBaicData.CustomColumns.Add(new RMDiscoveryCustomColumnWithValue(inactiveColumn.Name, 0, typeof(long)));
                        }

                        existsDataList.Add(existsBaicData);
                    }

                    foreach (var inactiveColumn in inactiveColumns)
                    {
                        var driveColumnValue = data.CustomColumns.First(item => item.Name == inactiveColumn.Name);
                        var baicColumnValue = existsBaicData.CustomColumns.First(item => item.Name == inactiveColumn.Name);
                        baicColumnValue.Value = long.Parse(baicColumnValue.Value.ToString()) + long.Parse(driveColumnValue.Value.ToString());
                    }

                    existsBaicData.FileTotalSize += data.FileTotalSize;
                    existsBaicData.FileSumCount += data.FileSumCount;
                }

                await _dataDao.AddOrUpdateBasicInactiveDataAsync(_googleOrganizationId, existsDataList.ToArray());

                _logger.Info($"Succeed append and save tenant [{_googleOrganizationId}] [{existsDataList.Count}] basic inactive data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while append and save tenant [{_googleOrganizationId}] basic inactive data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RecalculateAndSaveAsync()
        {
            try
            {
                var customColumns = _rules.ConvertAll(item => item.ToCustomColumn());
                var containerIds = (await _nodeDao.GetAllDiscoveryGoogleContainersAsync(_googleOrganizationId)).Select(item => item.Id).ToHashSet();
                foreach (var containerId in containerIds)
                {
                    var containerDataList = _dataDao.GetContainerInactiveDataListAsync(_googleOrganizationId, containerId, customColumns);
                    await foreach (var containerData in containerDataList)
                    {
                        var data = _dataList.FirstOrDefault(item =>
                                item.WithoutInDate == containerData.WithoutInDate &&
                                item.FileExtension == containerData.FileExtension &&
                                item.SizeRange == containerData.SizeRange
                            );
                        if (data == null)
                        {
                            data = new RMDiscoveryGoogleBasicInactiveData
                            {
                                WithoutInDate = containerData.WithoutInDate,
                                FileExtension = containerData.FileExtension,
                                SizeRange = containerData.SizeRange,
                                FileTotalSize = 0,
                                FileSumCount = 0,
                            };
                            foreach (var inactiveVersionRuleColumn in customColumns)
                            {
                                data.CustomColumns.Add(new RMDiscoveryCustomColumnWithValue(
                                    inactiveVersionRuleColumn.Name,
                                    0L,
                                    typeof(long)
                                    )
                                );
                            }
                            _dataList.Add(data);
                        }

                        data.FileTotalSize += containerData.FileTotalSize;
                        data.FileSumCount += containerData.FileSumCount;

                        foreach (var customColumn in customColumns)
                        {
                            var dataMatchedColumn = containerData.CustomColumns.First(item => item.Name == customColumn.Name);
                            var basicMathcedColumn = data.CustomColumns.First(item => item.Name == customColumn.Name);
                            basicMathcedColumn.Value = Convert.ToInt64(basicMathcedColumn.Value) + Convert.ToInt64(dataMatchedColumn.Value);
                        }
                    }
                }

                await _dataDao.AddOrUpdateBasicInactiveDataAsync(_googleOrganizationId, _dataList.ToArray());

                _logger.Info($"Succeed recalculate and save tenant [{_googleOrganizationId}] [{_dataList.Count}] basic inactive data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_googleOrganizationId}] [{_dataList.Count}] basic inactive data. Error: {e}");
                return false;
            }
        }
    }
}
