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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.General.Inactive
{
    public class RMDiscoveryOffice365ContainerInactiveDataAnalyzer
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365ContainerInactiveDataAnalyzer));

        private readonly IRMDiscoveryOffice365DataV3Dao _dataDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly Guid _o365TenantId;

        private readonly int _containerId;

        private readonly List<RMDiscoveryOffice365RuleInfo> _rules;

        private readonly List<RMDiscoveryOffice365ContainerInactiveData> _dataList;

        public RMDiscoveryOffice365ContainerInactiveDataAnalyzer(
            RMDiscoveryJobType jobType,
            Guid o365TenantId,
            int containerId,
            List<RMDiscoveryOffice365RuleInfo> rules
        )
        {
            _dataDao = new RMDiscoveryOffice365DataV3Dao();
            _jobType = jobType;
            _o365TenantId = o365TenantId;
            _containerId = containerId;
            _rules = rules;
            _dataList = [];
        }

        public void Increse(List<RMDiscoveryOffice365SiteInactiveData> siteDataList)
        {
            if (_jobType == RMDiscoveryJobType.Retry)
            {
                return;
            }

            var inactiveColumns = _rules.ConvertAll(item => item.ToCustomColumn());

            foreach (var siteData in siteDataList)
            {
                var data = _dataList.FirstOrDefault(item =>
                    item.FileExtension == siteData.FileExtension &&
                    item.SizeRange == siteData.SizeRange &&
                    item.WithoutInDate == siteData.WithoutInDate
                );
                if (data == null)
                {
                    data = new RMDiscoveryOffice365ContainerInactiveData
                    {
                        ContainerId = _containerId,
                        WithoutInDate = siteData.WithoutInDate,
                        FileExtension = siteData.FileExtension,
                        SizeRange = siteData.SizeRange,
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
                    var siteColumnValue = siteData.CustomColumns.First(item => item.Name == inactiveColumn.Name);
                    var containerColumnValue = data.CustomColumns.First(item => item.Name == inactiveColumn.Name);
                    containerColumnValue.Value = long.Parse(containerColumnValue.Value.ToString()) + long.Parse(siteColumnValue.Value.ToString());
                }

                data.FileTotalSize += siteData.FileTotalSize;
                data.FileSumCount += siteData.FileSumCount;
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
                    await _dataDao.AddOrUpdateContainerInactiveDataUnderSameContainerAsync(_o365TenantId, _dataList.ToArray());
                }

                _logger.Info($"Succeed save tenant [{_o365TenantId}] container [{_containerId}] [{_dataList.Count}] inactive data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while save tenant [{_o365TenantId}] container [{_containerId}] inactive data. Error: {e}");
                return false;
            }
        }

        public Task<bool> RefreshAndSaveAsync()
        {
            return RecalculateAndSaveAsync();
        }

        private async Task<bool> RecalculateAndSaveAsync()
        {
            try
            {
                _dataList.Clear();
                var inactiveColumns = _rules.ConvertAll(item => item.ToCustomColumn());

                var enumerableDataList = _dataDao.GetSiteInactiveDataByContainerIdAsync(_o365TenantId, _containerId, inactiveColumns);
                await foreach (var data in enumerableDataList)
                {
                    var containerInactiveData = _dataList.FirstOrDefault(item =>
                        item.WithoutInDate == data.WithoutInDate &&
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange
                    );

                    if (containerInactiveData == null)
                    {
                        containerInactiveData = new RMDiscoveryOffice365ContainerInactiveData
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

                await _dataDao.AddOrUpdateContainerInactiveDataUnderSameContainerAsync(_o365TenantId, _dataList.ToArray());
                _logger.Info($"Succeed recalculate and save tenant [{_o365TenantId}] container [{_containerId}] [{_dataList.Count}] inactive data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_o365TenantId}] container [{_containerId}] inactive data. Error: {e}");
                return false;
            }
        }
    }
}
