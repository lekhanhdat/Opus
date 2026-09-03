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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using Microsoft.SharePoint.Client.CompliancePolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V4.General.Inactive
{
    public class RMDiscoveryOffice365BasicInactiveDataAnalyzer
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365BasicInactiveDataAnalyzer));

        private readonly IRMDiscoveryOffice365DataV3Dao _dataDao;

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly Guid _o365TenantId;

        private readonly SourceFlag _contentSource;

        private readonly List<RMDiscoveryOffice365RuleInfo> _rules;

        private readonly List<RMDiscoveryOffice365BasicInactiveData> _dataList;

        public RMDiscoveryOffice365BasicInactiveDataAnalyzer(
            RMDiscoveryJobType jobType,
            Guid o365TenantId,
            SourceFlag contentSource,
            List<RMDiscoveryOffice365RuleInfo> rules
        )
        {
            _dataDao = new RMDiscoveryOffice365DataV3Dao();
            _nodeDao = new RMDiscoveryOffice365NodeDao();
            _jobType = jobType;
            _o365TenantId = o365TenantId;
            _contentSource = contentSource;
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
                    data = new RMDiscoveryOffice365BasicInactiveData
                    {
                        WithoutInDate = siteData.WithoutInDate,
                        FileExtension = siteData.FileExtension,
                        SizeRange = siteData.SizeRange,
                        ContentSource = _contentSource,
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
                    var baicColumnValue = data.CustomColumns.First(item => item.Name == inactiveColumn.Name);
                    baicColumnValue.Value = long.Parse(baicColumnValue.Value.ToString()) + long.Parse(siteColumnValue.Value.ToString());
                }

                data.FileTotalSize += siteData.FileTotalSize;
                data.FileSumCount += siteData.FileSumCount;
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

                await _dataDao.AddOrUpdateBasicInactiveDataUnderSameContentSourceAsync(_o365TenantId, _dataList.ToArray());

                _logger.Info($"Succeed save tenant [{_o365TenantId}] [{_contentSource}] [{_dataList.Count}] basic inactive data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while save tenant [{_o365TenantId}] [{_contentSource}] [{_dataList.Count}] basic inactive data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> AppendAndSaveAsync()
        {
            try
            {
                var inactiveColumns = _rules.ConvertAll(item => item.ToCustomColumn());
                var existsDataList = await _dataDao.GetBasicInactiveDataListAsync(_o365TenantId, _contentSource, inactiveColumns);
                foreach (var data in _dataList)
                {
                    var existsBaicData = existsDataList.FirstOrDefault(item =>
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange &&
                        item.WithoutInDate == data.WithoutInDate
                    );
                    if (existsBaicData == null)
                    {
                        existsBaicData = new RMDiscoveryOffice365BasicInactiveData
                        {
                            WithoutInDate = data.WithoutInDate,
                            FileExtension = data.FileExtension,
                            SizeRange = data.SizeRange,
                            ContentSource = _contentSource,
                        };
                        foreach (var inactiveColumn in inactiveColumns)
                        {
                            existsBaicData.CustomColumns.Add(new RMDiscoveryCustomColumnWithValue(inactiveColumn.Name, 0, typeof(long)));
                        }

                        existsDataList.Add(existsBaicData);
                    }

                    foreach (var inactiveColumn in inactiveColumns)
                    {
                        var siteColumnValue = data.CustomColumns.First(item => item.Name == inactiveColumn.Name);
                        var baicColumnValue = existsBaicData.CustomColumns.First(item => item.Name == inactiveColumn.Name);
                        baicColumnValue.Value = long.Parse(baicColumnValue.Value.ToString()) + long.Parse(siteColumnValue.Value.ToString());
                    }

                    existsBaicData.FileTotalSize += data.FileTotalSize;
                    existsBaicData.FileSumCount += data.FileSumCount;
                }

                await _dataDao.AddOrUpdateBasicInactiveDataUnderSameContentSourceAsync(_o365TenantId, existsDataList.ToArray());

                _logger.Info($"Succeed append and save tenant [{_o365TenantId}] [{_contentSource}] [{existsDataList.Count}] basic inactive data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while append and save tenant [{_o365TenantId}] [{_contentSource}] basic inactive data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RecalculateAndSaveAsync()
        {
            try
            {
                var customColumns = _rules.ConvertAll(item => item.ToCustomColumn());
                var containerIds = (await _nodeDao.GetAllDiscoveryContainersAsync(_o365TenantId, _contentSource)).Select(item => item.Id).ToHashSet();
                foreach (var containerId in containerIds)
                {
                    var containerDataList = _dataDao.GetContainerInactiveDataListAsync(_o365TenantId, containerId, customColumns);
                    await foreach (var containerData in containerDataList)
                    {
                        var data = _dataList.FirstOrDefault(item =>
                                item.WithoutInDate == containerData.WithoutInDate &&
                                item.FileExtension == containerData.FileExtension &&
                                item.SizeRange == containerData.SizeRange
                            );
                        if (data == null)
                        {
                            data = new RMDiscoveryOffice365BasicInactiveData
                            {
                                WithoutInDate = containerData.WithoutInDate,
                                FileExtension = containerData.FileExtension,
                                SizeRange = containerData.SizeRange,
                                FileTotalSize = 0,
                                FileSumCount = 0,
                                ContentSource = _contentSource,
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

                await _dataDao.AddOrUpdateBasicInactiveDataUnderSameContentSourceAsync(_o365TenantId, _dataList.ToArray());

                _logger.Info($"Succeed recalculate and save tenant [{_o365TenantId}] [{_contentSource}] [{_dataList.Count}] basic inactive data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_o365TenantId}] [{_contentSource}] [{_dataList.Count}] basic inactive data. Error: {e}");
                return false;
            }
        }
    }
}
