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
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Calculator
{
    public class RMDiscoveryOffice365RescanCalculator
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365RescanCalculator));

        private readonly IRMDiscoveryOffice365DataDao _dataDao;

        private readonly IRMDiscoveryOffice365RuleInfoDao _ruleInfoDao;

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao;

        private readonly IRMDiscoveryOffice365JobDao _jobDao;

        private readonly IRMDiscoveryConfigurationDao _configurationDao;

        private readonly RMDiscoveryOffice365MainJob _retryJobInfo;

        public RMDiscoveryOffice365RescanCalculator(RMDiscoveryOffice365MainJob retryJobInfo)
        {
            _dataDao = new RMDiscoveryOffice365DataDao();
            _ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();
            _nodeDao = new RMDiscoveryOffice365NodeDao();
            _jobDao = new RMDiscoveryOffice365JobDao();
            _configurationDao = new RMDiscoveryConfigurationDao();
            _retryJobInfo = retryJobInfo;
        }

        public async Task<bool> CalculateAsync()
        {
            try
            {
                _logger.Info($"Start calculate rescan data.");

                var res = true;

                var inactiveEnable = (await _configurationDao.GetAsync<RMDiscoveryOffice365InactiveDefinition>(RMDiscoveryConfigurationType.Office365InactiveDefinition)).Enable;
                var inactiveRules = inactiveEnable ? await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive) : [];
                var customColumns = inactiveRules.ConvertAll(item => item.ToCustomColumn());

                _logger.Info($"Iactive rule is enable [{inactiveEnable}]. Available rules [{string.Join(", ", inactiveRules.Select(item => item.Id))}].");

                var discoveryJobs = await _jobDao.GetDiscoveryJobsAsync(_retryJobInfo.Id);

                foreach (var discoveryJob in discoveryJobs)
                {
                    var (has, containerInfo) = await _nodeDao.TryGetDiscoveryContainerByOpusIdAsync(discoveryJob.O365TenantId, discoveryJob.ContainerId);
                    if(!has)
                    {
                        _logger.Info($"The container [{discoveryJob.ContainerId}] no data found. Skipped it.");
                        continue;
                    }

                    _logger.Info($"Start calculate [{discoveryJob.O365TenantId}] container [{containerInfo.Id}] data.");

                    res &= await CalculateContainerInactiveDataAsync(discoveryJob.O365TenantId, containerInfo, customColumns);
                    res &= await CalculateContainerRotDataAsync(discoveryJob.O365TenantId, containerInfo);
                    res &= await CalculateContainerDataAsync(discoveryJob.O365TenantId, containerInfo);
                }

                foreach (var o365TenantId in discoveryJobs.Select(item => item.O365TenantId).ToHashSet())
                {
                    var containerInfoes = await _nodeDao.GetAllDiscoveryContainersAsync(o365TenantId);
                    res &= await CalculateBasicInactiveDataAsync(o365TenantId, containerInfoes, customColumns);
                    res &= await CalculateBasicRotDataAsync(o365TenantId, containerInfoes);
                }

                _logger.Info($"End calculate rescan data.");

                return res;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while calculate rescan data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CalculateContainerInactiveDataAsync(Guid o365TenantId, RMDiscoveryOffice365ContainerInfo containerInfo, List<RMDiscoveryCustomColumn> customColumns)
        {
            try
            {
                var containerInactiveDataList = new List<RMDiscoveryOffice365ContainerInactiveData>();
                var enumerableDataList = _dataDao.GetSiteInactiveDataListByContainerAsync(o365TenantId, containerInfo.Id, customColumns);
                await foreach (var data in enumerableDataList)
                {
                    var containerInactiveData = containerInactiveDataList.FirstOrDefault(item =>
                        item.WithoutInDate == data.WithoutInDate &&
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange
                    );

                    if (containerInactiveData == null)
                    {
                        containerInactiveData = new RMDiscoveryOffice365ContainerInactiveData
                        {
                            ContainerId = containerInfo.Id,
                            WithoutInDate = data.WithoutInDate,
                            FileExtension = data.FileExtension,
                            SizeRange = data.SizeRange,
                            FileTotalSize = 0,
                            FileSumCount = 0,
                        };
                        containerInactiveDataList.Add(containerInactiveData);
                        foreach (var inactiveVersionRuleColumn in customColumns)
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

                    foreach (var customColumn in customColumns)
                    {
                        var dataMatchedColumn = data.CustomColumns.First(item => item.Name == customColumn.Name);
                        var containerMathcedColumn = containerInactiveData.CustomColumns.First(item => item.Name == customColumn.Name);
                        containerMathcedColumn.Value = Convert.ToInt64(containerMathcedColumn.Value) + Convert.ToInt64(dataMatchedColumn.Value);
                    }
                }

                await _dataDao.AddOrUpdateContainerInactiveDataAsync(o365TenantId, containerInfo.Id, containerInactiveDataList.ToArray());

                _logger.Info($"Successful calculate container [{o365TenantId}] [{containerInfo.OpusId}] inactive data. Count: [{containerInactiveDataList.Count}]");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while calculate container [{o365TenantId}] [{containerInfo.OpusId}] inactive data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CalculateBasicInactiveDataAsync(Guid o365TenantId, List<RMDiscoveryOffice365ContainerInfo> containerInfoes, List<RMDiscoveryCustomColumn> customColumns)
        {
            try
            {
                var enumerableDataList = _dataDao.GetContainerInactiveDataListAsync(o365TenantId, customColumns);

                foreach (var contentSource in new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.OneDrive })
                {
                    var basicInactiveDataList = new List<RMDiscoveryOffice365BasicInactiveData>();

                    var contentSourceContainerIds = containerInfoes.Where(item => item.ContentSource == contentSource)
                        .Select(item => item.Id).ToHashSet();

                    var contentSourceEnumerableDataList = enumerableDataList.Where(item => contentSourceContainerIds.Contains(item.ContainerId));

                    await foreach (var data in contentSourceEnumerableDataList)
                    {
                        var basicInactiveData = basicInactiveDataList.FirstOrDefault(item =>
                            item.WithoutInDate == data.WithoutInDate &&
                            item.FileExtension == data.FileExtension &&
                            item.SizeRange == data.SizeRange
                        );

                        if (basicInactiveData == null)
                        {
                            basicInactiveData = new RMDiscoveryOffice365BasicInactiveData
                            {
                                WithoutInDate = data.WithoutInDate,
                                FileExtension = data.FileExtension,
                                SizeRange = data.SizeRange,
                                FileTotalSize = 0,
                                FileSumCount = 0,
                                ContentSource = contentSource,
                            };
                            basicInactiveDataList.Add(basicInactiveData);
                            foreach (var inactiveVersionRuleColumn in customColumns)
                            {
                                basicInactiveData.CustomColumns.Add(new RMDiscoveryCustomColumnWithValue(
                                    inactiveVersionRuleColumn.Name,
                                    0L,
                                    typeof(long)
                                    )
                                );
                            }
                        }
                        
                        basicInactiveData.FileTotalSize += data.FileTotalSize;
                        basicInactiveData.FileSumCount += data.FileSumCount;

                        foreach (var customColumn in customColumns)
                        {
                            var dataMatchedColumn = data.CustomColumns.First(item => item.Name == customColumn.Name);
                            var basicMathcedColumn = basicInactiveData.CustomColumns.First(item => item.Name == customColumn.Name);
                            basicMathcedColumn.Value = Convert.ToInt64(basicMathcedColumn.Value) + Convert.ToInt64(dataMatchedColumn.Value);
                        }
                    }

                    await _dataDao.AddOrUpdateBasicInactiveDataAsync(o365TenantId, contentSource, basicInactiveDataList.ToArray());

                    _logger.Info($"Successful calculate [{o365TenantId}] [{contentSource}] basic inactive data.");
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while calculate basic [{o365TenantId}] basic inactive data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CalculateContainerRotDataAsync(Guid o365TenantId, RMDiscoveryOffice365ContainerInfo containerInfo)
        {
            try
            {
                var containerRotDataList = new List<RMDiscoveryOffice365ContainerRotData>();

                var enumerableDataList = _dataDao.GetSiteRotDataListByContainerAsync(o365TenantId, containerInfo.Id);
                await foreach (var data in enumerableDataList)
                {
                    var containerRotData = containerRotDataList.FirstOrDefault(item =>
                        item.WithoutInDate == data.WithoutInDate &&
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange &&
                        item.Rule == data.Rule
                    );

                    if (containerRotData == null)
                    {
                        containerRotData = new RMDiscoveryOffice365ContainerRotData
                        {
                            ContainerId = containerInfo.Id,
                            WithoutInDate = data.WithoutInDate,
                            FileExtension = data.FileExtension,
                            SizeRange = data.SizeRange,
                            Rule = data.Rule,
                            FileTotalSize = 0,
                            FileSumCount = 0,
                        };
                        containerRotDataList.Add(containerRotData);
                    }

                    containerRotData.FileTotalSize += data.FileTotalSize;
                    containerRotData.FileSumCount += data.FileSumCount;
                }

                await _dataDao.AddOrUpdateContainerRotDataAsync(o365TenantId, containerInfo.Id, containerRotDataList.ToArray());

                _logger.Info($"Successful calculate [{o365TenantId}] [{containerInfo.OpusId}] container rot data. Count: [{containerRotDataList.Count}]");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while calculate [{o365TenantId}] [{containerInfo.OpusId}] container rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CalculateBasicRotDataAsync(Guid o365TenantId, List<RMDiscoveryOffice365ContainerInfo> containerInfoes)
        {
            try
            {
                var enumerableDataList = await _dataDao.GetContainerRotDataListAsync(o365TenantId).ToListAsync();

                foreach (var contentSource in new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.OneDrive })
                {
                    var basicRotDataList = new List<RMDiscoveryOffice365BasicRotData>();

                    var contentSourceContainerIds = containerInfoes.Where(item => item.ContentSource == contentSource)
    .Select(item => item.Id).ToHashSet();

                    var contentSourceEnumerableDataList = enumerableDataList.Where(item => contentSourceContainerIds.Contains(item.ContainerId)).ToList();

                    foreach (var data in contentSourceEnumerableDataList)
                    {

                        var baiscRotData = basicRotDataList.FirstOrDefault(item =>
                            item.WithoutInDate == data.WithoutInDate &&
                            item.FileExtension == data.FileExtension &&
                            item.SizeRange == data.SizeRange &&
                            item.Rule == data.Rule
                        );
                        if (baiscRotData == null)
                        {
                            baiscRotData = new RMDiscoveryOffice365BasicRotData
                            {
                                WithoutInDate = data.WithoutInDate,
                                FileExtension = data.FileExtension,
                                SizeRange = data.SizeRange,
                                Rule = data.Rule,
                                FileTotalSize = 0,
                                FileSumCount = 0,
                                ContentSource = contentSource
                            };
                            basicRotDataList.Add(baiscRotData);
                        }

                        baiscRotData.FileTotalSize += data.FileTotalSize;
                        baiscRotData.FileSumCount += data.FileSumCount;
                    }

                    await _dataDao.AddOrUpdateBasicRotDataAsync(o365TenantId, contentSource, basicRotDataList.ToArray());

                    _logger.Info($"Successful calculate [{o365TenantId}] [{contentSource}] basic rot data.");

                }

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while calculate [{o365TenantId}] basic rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CalculateContainerDataAsync(Guid o365TenantId, RMDiscoveryOffice365ContainerInfo containerInfo)
        {
            try
            {
                containerInfo.FileTotalSize = 0;
                containerInfo.FileSumCount = 0;
                containerInfo.SiteCount = 0;

                var siteInfoes = _nodeDao.GetDiscoverySiteInfoesAsync(o365TenantId, containerInfo.Id);

                await foreach (var siteInfo in siteInfoes)
                {
                    containerInfo.FileTotalSize += siteInfo.FileTotalSize;
                    containerInfo.FileSumCount += siteInfo.FileSumCount;
                    containerInfo.SiteCount++;
                }

                await _nodeDao.AddOrUpdateDiscoveryContainerAsync(o365TenantId, containerInfo);

                _logger.Info($"Successful calculate [{o365TenantId}] [{containerInfo.OpusId}] container data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while calculate container data. Error: {e}");
                return false;
            }
        }
    }
}
