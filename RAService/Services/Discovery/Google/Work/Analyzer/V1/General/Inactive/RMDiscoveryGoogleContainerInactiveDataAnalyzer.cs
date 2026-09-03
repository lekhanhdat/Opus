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
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Model.Discovery.Google;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.General.Inactive
{
    public class RMDiscoveryGoogleContainerInactiveDataAnalyzer
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleContainerInactiveDataAnalyzer));

        private readonly IRMDiscoveryGoogleDataDao _dataDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly string _googleOrganizationId;

        private readonly int _containerId;

        private readonly List<RMDiscoveryGoogleRuleInfo> _rules;

        private readonly List<RMDiscoveryGoogleContainerInactiveData> _dataList;

        public RMDiscoveryGoogleContainerInactiveDataAnalyzer(
            RMDiscoveryJobType jobType,
            string googleOrganizationId,
            int containerId,
            List<RMDiscoveryGoogleRuleInfo> rules
        )
        {
            _dataDao = new RMDiscoveryGoogleDataDao();
            _jobType = jobType;
            _googleOrganizationId = googleOrganizationId;
            _containerId = containerId;
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
                    data = new RMDiscoveryGoogleContainerInactiveData
                    {
                        ContainerId = _containerId,
                        WithoutInDate = driveData.WithoutInDate,
                        FileExtension = driveData.FileExtension,
                        SizeRange = driveData.SizeRange,
                        CustomColumns = new()
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
                    var containerColumnValue = data.CustomColumns.First(item => item.Name == inactiveColumn.Name);
                    containerColumnValue.Value = long.Parse(containerColumnValue.Value.ToString()) + long.Parse(driveColumnValue.Value.ToString());
                }

                data.FileTotalSize += driveData.FileTotalSize;
                data.FileSumCount += driveData.FileSumCount;
            }
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    return await RecalculateAndSaveAsync();
                }

                if (_dataList.Count > 0)
                {
                    await _dataDao.AddOrUpdateContainerInactiveDataUnderSameContainerAsync(_googleOrganizationId, _dataList.ToArray());
                }

                _logger.Info($"Succeed save tenant [{_googleOrganizationId}] container [{_containerId}] [{_dataList.Count}] inactive data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while save tenant [{_googleOrganizationId}] container [{_containerId}] inactive data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RecalculateAndSaveAsync()
        {
            try
            {
                var inactiveColumns = _rules.ConvertAll(item => item.ToCustomColumn());

                var enumerableDataList = _dataDao.GetDriveInactiveDataByContainerIdAsync(_googleOrganizationId, _containerId, inactiveColumns);
                await foreach (var data in enumerableDataList)
                {
                    var containerInactiveData = _dataList.FirstOrDefault(item =>
                        item.WithoutInDate == data.WithoutInDate &&
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange
                    );

                    if (containerInactiveData == null)
                    {
                        containerInactiveData = new RMDiscoveryGoogleContainerInactiveData
                        {
                            ContainerId = _containerId,
                            WithoutInDate = data.WithoutInDate,
                            FileExtension = data.FileExtension,
                            SizeRange = data.SizeRange,
                            FileTotalSize = 0,
                            FileSumCount = 0,
                        };
                        _dataList.Add(containerInactiveData);
                        foreach (var inactiveVersionRuleColumn in inactiveColumns)
                        {
                            containerInactiveData.CustomColumns.Add(new RMDiscoveryCustomColumnWithValue(
                                inactiveVersionRuleColumn.Name,
                                0L,
                                typeof(long)
                                )
                            );
                        }
                    }

                    containerInactiveData.FileTotalSize += data.FileTotalSize;
                    containerInactiveData.FileSumCount += data.FileSumCount;

                    foreach (var customColumn in inactiveColumns)
                    {
                        var dataMatchedColumn = data.CustomColumns.First(item => item.Name == customColumn.Name);
                        var containerMathcedColumn = containerInactiveData.CustomColumns.First(item => item.Name == customColumn.Name);
                        containerMathcedColumn.Value = Convert.ToInt64(containerMathcedColumn.Value) + Convert.ToInt64(dataMatchedColumn.Value);
                    }

                }

                await _dataDao.AddOrUpdateContainerInactiveDataUnderSameContainerAsync(_googleOrganizationId, _dataList.ToArray());
                _logger.Info($"Succeed recalculate and save tenant [{_googleOrganizationId}] container [{_containerId}] [{_dataList.Count}] inactive data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_googleOrganizationId}] container [{_containerId}] inactive data. Error: {e}");
                return false;
            }
        }
    }
}
