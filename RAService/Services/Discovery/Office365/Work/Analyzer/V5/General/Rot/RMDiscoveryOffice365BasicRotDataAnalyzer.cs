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

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.General.Rot
{
    public class RMDiscoveryOffice365BasicRotDataAnalyzer
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365BasicRotDataAnalyzer));

        private readonly IRMDiscoveryOffice365DataV3Dao _dataDao;

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly Guid _o365TenantId;

        private readonly SourceFlag _contentSource;

        private readonly List<RMDiscoveryOffice365BasicRuleLevelRotData> _ruleLevelDataList;

        private readonly List<RMDiscoveryOffice365BasicCategoryLevelRotData> _categoryLevelDataList;

        private readonly List<RMDiscoveryOffice365BasicRootLevelRotData> _rootLevelDataList;

        public RMDiscoveryOffice365BasicRotDataAnalyzer(
            RMDiscoveryJobType jobType,
            Guid o365TenantId,
            SourceFlag contentSource
        )
        {
            _dataDao = new RMDiscoveryOffice365DataV3Dao();
            _nodeDao = new RMDiscoveryOffice365NodeDao();
            _jobType = jobType;
            _o365TenantId = o365TenantId;
            _contentSource = contentSource;
            _ruleLevelDataList = [];
            _categoryLevelDataList = [];
            _rootLevelDataList = [];
        }

        public void Increse(List<RMDiscoveryOffice365SiteRuleLevelRotData> siteDataList)
        {
            if (_jobType == RMDiscoveryJobType.Retry)
            {
                return;
            }

            foreach (var siteData in siteDataList)
            {
                var data = _ruleLevelDataList.FirstOrDefault(item =>
                        item.FileExtension == siteData.FileExtension &&
                        item.SizeRange == siteData.SizeRange &&
                        item.WithoutInDate == siteData.WithoutInDate &&
                        item.Rule == siteData.Rule
                    );
                if (data == null)
                {
                    data = new RMDiscoveryOffice365BasicRuleLevelRotData
                    {
                        WithoutInDate = siteData.WithoutInDate,
                        FileExtension = siteData.FileExtension,
                        SizeRange = siteData.SizeRange,
                        Rule = siteData.Rule,
                        ContentSource = _contentSource
                    };
                    _ruleLevelDataList.Add(data);
                }
                data.FileTotalSize += siteData.FileTotalSize;
                data.FileSumCount += siteData.FileSumCount;
            }
        }

        public void Increse(List<RMDiscoveryOffice365SiteCategoryLevelRotData> siteDataList)
        {
            if (_jobType == RMDiscoveryJobType.Retry)
            {
                return;
            }

            foreach (var siteData in siteDataList)
            {
                var data = _categoryLevelDataList.FirstOrDefault(item =>
                        item.FileExtension == siteData.FileExtension &&
                        item.SizeRange == siteData.SizeRange &&
                        item.WithoutInDate == siteData.WithoutInDate &&
                        item.Category == siteData.Category
                    );
                if (data == null)
                {
                    data = new RMDiscoveryOffice365BasicCategoryLevelRotData
                    {
                        WithoutInDate = siteData.WithoutInDate,
                        FileExtension = siteData.FileExtension,
                        SizeRange = siteData.SizeRange,
                        Category = siteData.Category,
                        ContentSource = _contentSource
                    };
                    _categoryLevelDataList.Add(data);
                }
                data.FileTotalSize += siteData.FileTotalSize;
                data.FileSumCount += siteData.FileSumCount;
            }
        }

        public void Increse(List<RMDiscoveryOffice365SiteRootLevelRotData> siteDataList)
        {
            if (_jobType == RMDiscoveryJobType.Retry)
            {
                return;
            }

            foreach (var siteData in siteDataList)
            {
                var data = _rootLevelDataList.FirstOrDefault(item =>
                        item.FileExtension == siteData.FileExtension &&
                        item.SizeRange == siteData.SizeRange &&
                        item.WithoutInDate == siteData.WithoutInDate
                    );
                if (data == null)
                {
                    data = new RMDiscoveryOffice365BasicRootLevelRotData
                    {
                        WithoutInDate = siteData.WithoutInDate,
                        FileExtension = siteData.FileExtension,
                        SizeRange = siteData.SizeRange,
                        ContentSource = _contentSource
                    };
                    _rootLevelDataList.Add(data);
                }
                data.FileTotalSize += siteData.FileTotalSize;
                data.FileSumCount += siteData.FileSumCount;
            }
        }

        public async Task<bool> SaveAsync()
        {
            var res = true;
            res &= await RuleLevelSaveAsync();
            res &= await CategoryLevelSaveAsync();
            res &= await RootLevelSaveAsync();

            return res;
        }

        public async Task<bool> RefreshAndSaveAsync()
        {
            var res = true;
            res &= await RuleLevelRecalculateAndSaveAsync();
            res &= await CategoryLevelRecalculateAndSaveAsync();
            res &= await RootLevelRecalculateAndSaveAsync();

            return res;
        }

        public async Task<bool> RuleLevelSaveAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Append)
                {
                    return await RuleLevelAppendAndSaveAsync();
                }
                else if (_jobType == RMDiscoveryJobType.Retry)
                {
                    return await RuleLevelRecalculateAndSaveAsync();
                }

                await _dataDao.AddOrUpdateBasicRuleLevelRotDataUnderSameContentSourceAsync(_o365TenantId, _ruleLevelDataList.ToArray());

                _logger.Info($"Succeed save tenant [{_o365TenantId}] [{_contentSource}] [{_ruleLevelDataList.Count}] basic rule level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while save tenant [{_o365TenantId}] [{_contentSource}] [{_ruleLevelDataList.Count}] basic rule level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RuleLevelAppendAndSaveAsync()
        {
            try
            {
                var existsDataList = await _dataDao.GetBasicRuleLevelRotDataListAsync(_o365TenantId, _contentSource);
                foreach (var data in _ruleLevelDataList)
                {
                    var existsBaicData = existsDataList.FirstOrDefault(item =>
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange &&
                        item.WithoutInDate == data.WithoutInDate &&
                        item.Rule == data.Rule
                    );
                    if (existsBaicData == null)
                    {
                        existsBaicData = new RMDiscoveryOffice365BasicRuleLevelRotData
                        {
                            WithoutInDate = data.WithoutInDate,
                            FileExtension = data.FileExtension,
                            SizeRange = data.SizeRange,
                            Rule = data.Rule,
                            ContentSource = _contentSource
                        };
                        existsDataList.Add(existsBaicData);
                    }
                    existsBaicData.FileTotalSize += data.FileTotalSize;
                    existsBaicData.FileSumCount += data.FileSumCount;
                }

                await _dataDao.AddOrUpdateBasicRuleLevelRotDataUnderSameContentSourceAsync(_o365TenantId, existsDataList.ToArray());

                _logger.Info($"Succeed append and save tenant [{_o365TenantId}] [{_contentSource}] [{existsDataList.Count}] basic rule level rot data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while append and save tenant [{_o365TenantId}] [{_contentSource}] [{_ruleLevelDataList.Count}] basic rule level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RuleLevelRecalculateAndSaveAsync()
        {
            try
            {
                _ruleLevelDataList.Clear();
                var containerIds = (await _nodeDao.GetAllDiscoveryContainersAsync(_o365TenantId, _contentSource)).Select(item => item.Id).ToHashSet();
                foreach (var containerId in containerIds)
                {
                    var containerDataList = _dataDao.GetContainerRuleLevelRotDataListAsync(_o365TenantId, containerId);
                    await foreach (var containerData in containerDataList)
                    {
                        var data = _ruleLevelDataList.FirstOrDefault(item =>
                                item.WithoutInDate == containerData.WithoutInDate &&
                                item.FileExtension == containerData.FileExtension &&
                                item.SizeRange == containerData.SizeRange &&
                                item.Rule == containerData.Rule
                            );
                        if (data == null)
                        {
                            data = new RMDiscoveryOffice365BasicRuleLevelRotData
                            {
                                WithoutInDate = containerData.WithoutInDate,
                                FileExtension = containerData.FileExtension,
                                SizeRange = containerData.SizeRange,
                                Rule = containerData.Rule,
                                FileTotalSize = 0,
                                FileSumCount = 0,
                                ContentSource = _contentSource
                            };
                            _ruleLevelDataList.Add(data);
                        }

                        data.FileTotalSize += containerData.FileTotalSize;
                        data.FileSumCount += containerData.FileSumCount;
                    }
                }

                await _dataDao.AddOrUpdateBasicRuleLevelRotDataUnderSameContentSourceAsync(_o365TenantId, _ruleLevelDataList.ToArray());

                _logger.Info($"Succeed recalculate and save tenant [{_o365TenantId}] [{_contentSource}] [{_ruleLevelDataList.Count}] basic rule level rot data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_o365TenantId}] [{_contentSource}] [{_ruleLevelDataList.Count}] basic rule level rot data. Error: {e}");
                return false;
            }
        }

        public async Task<bool> CategoryLevelSaveAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Append)
                {
                    return await CategoryLevelAppendAndSaveAsync();
                }
                else if (_jobType == RMDiscoveryJobType.Retry)
                {
                    return await CategoryLevelRecalculateAndSaveAsync();
                }

                await _dataDao.AddOrUpdateBasicCategoryLevelRotDataUnderSameContentSourceAsync(_o365TenantId, _categoryLevelDataList.ToArray());

                _logger.Info($"Succeed save tenant [{_o365TenantId}] [{_contentSource}] [{_categoryLevelDataList.Count}] basic category level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while save tenant [{_o365TenantId}] [{_contentSource}] [{_categoryLevelDataList.Count}] basic category level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CategoryLevelAppendAndSaveAsync()
        {
            try
            {
                var existsDataList = await _dataDao.GetBasicCategoryLevelRotDataListAsync(_o365TenantId, _contentSource);
                foreach (var data in _categoryLevelDataList)
                {
                    var existsBaicData = existsDataList.FirstOrDefault(item =>
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange &&
                        item.WithoutInDate == data.WithoutInDate &&
                        item.Category == data.Category
                    );
                    if (existsBaicData == null)
                    {
                        existsBaicData = new RMDiscoveryOffice365BasicCategoryLevelRotData
                        {
                            WithoutInDate = data.WithoutInDate,
                            FileExtension = data.FileExtension,
                            SizeRange = data.SizeRange,
                            Category = data.Category,
                            ContentSource = _contentSource
                        };
                        existsDataList.Add(existsBaicData);
                    }
                    existsBaicData.FileTotalSize += data.FileTotalSize;
                    existsBaicData.FileSumCount += data.FileSumCount;
                }

                await _dataDao.AddOrUpdateBasicCategoryLevelRotDataUnderSameContentSourceAsync(_o365TenantId, existsDataList.ToArray());

                _logger.Info($"Succeed append and save tenant [{_o365TenantId}] [{_contentSource}] [{existsDataList.Count}] basic category level rot data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while append and save tenant [{_o365TenantId}] [{_contentSource}] [{_categoryLevelDataList.Count}] basic category level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CategoryLevelRecalculateAndSaveAsync()
        {
            try
            {
                _categoryLevelDataList.Clear();
                var containerIds = (await _nodeDao.GetAllDiscoveryContainersAsync(_o365TenantId, _contentSource)).Select(item => item.Id).ToHashSet();
                foreach (var containerId in containerIds)
                {
                    var containerDataList = _dataDao.GetContainerCategoryLevelRotDataListAsync(_o365TenantId, containerId);
                    await foreach (var containerData in containerDataList)
                    {
                        var data = _categoryLevelDataList.FirstOrDefault(item =>
                                item.WithoutInDate == containerData.WithoutInDate &&
                                item.FileExtension == containerData.FileExtension &&
                                item.SizeRange == containerData.SizeRange &&
                                item.Category == containerData.Category
                            );
                        if (data == null)
                        {
                            data = new RMDiscoveryOffice365BasicCategoryLevelRotData
                            {
                                WithoutInDate = containerData.WithoutInDate,
                                FileExtension = containerData.FileExtension,
                                SizeRange = containerData.SizeRange,
                                Category = containerData.Category,
                                FileTotalSize = 0,
                                FileSumCount = 0,
                                ContentSource = _contentSource
                            };
                            _categoryLevelDataList.Add(data);
                        }

                        data.FileTotalSize += containerData.FileTotalSize;
                        data.FileSumCount += containerData.FileSumCount;
                    }
                }

                await _dataDao.AddOrUpdateBasicCategoryLevelRotDataUnderSameContentSourceAsync(_o365TenantId, _categoryLevelDataList.ToArray());

                _logger.Info($"Succeed recalculate and save tenant [{_o365TenantId}] [{_contentSource}] [{_categoryLevelDataList.Count}] basic category level rot data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_o365TenantId}] [{_contentSource}] [{_categoryLevelDataList.Count}] basic category level rot data. Error: {e}");
                return false;
            }
        }

        public async Task<bool> RootLevelSaveAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Append)
                {
                    return await RootLevelAppendAndSaveAsync();
                }
                else if (_jobType == RMDiscoveryJobType.Retry)
                {
                    return await RootLevelRecalculateAndSaveAsync();
                }

                await _dataDao.AddOrUpdateBasicRootLevelRotDataUnderSameContentSourceAsync(_o365TenantId, _rootLevelDataList.ToArray());

                _logger.Info($"Succeed save tenant [{_o365TenantId}] [{_contentSource}] [{_rootLevelDataList.Count}] basic root level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while save tenant [{_o365TenantId}] [{_contentSource}] [{_rootLevelDataList.Count}] basic root level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RootLevelAppendAndSaveAsync()
        {
            try
            {
                var existsDataList = await _dataDao.GetBasicRootLevelRotDataListAsync(_o365TenantId, _contentSource);
                foreach (var data in _rootLevelDataList)
                {
                    var existsBaicData = existsDataList.FirstOrDefault(item =>
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange &&
                        item.WithoutInDate == data.WithoutInDate
                    );
                    if (existsBaicData == null)
                    {
                        existsBaicData = new RMDiscoveryOffice365BasicRootLevelRotData
                        {
                            WithoutInDate = data.WithoutInDate,
                            FileExtension = data.FileExtension,
                            SizeRange = data.SizeRange,
                            ContentSource = _contentSource
                        };
                        existsDataList.Add(existsBaicData);
                    }
                    existsBaicData.FileTotalSize += data.FileTotalSize;
                    existsBaicData.FileSumCount += data.FileSumCount;
                }

                await _dataDao.AddOrUpdateBasicRootLevelRotDataUnderSameContentSourceAsync(_o365TenantId, existsDataList.ToArray());

                _logger.Info($"Succeed append and save tenant [{_o365TenantId}] [{_contentSource}] [{existsDataList.Count}] basic root level rot data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while append and save tenant [{_o365TenantId}] [{_contentSource}] [{_rootLevelDataList.Count}] basic root level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RootLevelRecalculateAndSaveAsync()
        {
            try
            {
                _rootLevelDataList.Clear();
                var containerIds = (await _nodeDao.GetAllDiscoveryContainersAsync(_o365TenantId, _contentSource)).Select(item => item.Id).ToHashSet();
                foreach (var containerId in containerIds)
                {
                    var containerDataList = _dataDao.GetContainerRootLevelRotDataListAsync(_o365TenantId, containerId);
                    await foreach (var containerData in containerDataList)
                    {
                        var data = _rootLevelDataList.FirstOrDefault(item =>
                                item.WithoutInDate == containerData.WithoutInDate &&
                                item.FileExtension == containerData.FileExtension &&
                                item.SizeRange == containerData.SizeRange
                            );
                        if (data == null)
                        {
                            data = new RMDiscoveryOffice365BasicRootLevelRotData
                            {
                                WithoutInDate = containerData.WithoutInDate,
                                FileExtension = containerData.FileExtension,
                                SizeRange = containerData.SizeRange,
                                FileTotalSize = 0,
                                FileSumCount = 0,
                                ContentSource = _contentSource
                            };
                            _rootLevelDataList.Add(data);
                        }

                        data.FileTotalSize += containerData.FileTotalSize;
                        data.FileSumCount += containerData.FileSumCount;
                    }
                }

                await _dataDao.AddOrUpdateBasicRootLevelRotDataUnderSameContentSourceAsync(_o365TenantId, _rootLevelDataList.ToArray());

                _logger.Info($"Succeed recalculate and save tenant [{_o365TenantId}] [{_contentSource}] [{_rootLevelDataList.Count}] basic root level rot data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_o365TenantId}] [{_contentSource}] [{_rootLevelDataList.Count}] basic root level rot data. Error: {e}");
                return false;
            }
        }
    }
}
