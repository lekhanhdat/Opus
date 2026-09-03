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
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work.Analyzer.General.ROT
{
    public class RMDiscoveryAOSPContainerRotDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPContainerRotDataAnalyzer));

        private readonly IRMDiscoveryAOSPDataDao _dataDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly Guid _o365TenantId;

        private readonly int _containerId;

        private readonly List<RMDiscoveryAOSPContainerRuleLevelRotData> _ruleLevelDataList;

        private readonly List<RMDiscoveryAOSPContainerCategoryLevelRotData> _categoryLevelDataList;

        private readonly List<RMDiscoveryAOSPContainerRootLevelRotData> _rootLevelDataList;

        public RMDiscoveryAOSPContainerRotDataAnalyzer(
            RMDiscoveryJobType jobType,
            Guid o365TenantId,
        int containerId
            )
        {
            _dataDao = new RMDiscoveryAOSPDataDao();
            _jobType = jobType;
            _o365TenantId = o365TenantId;
            _containerId = containerId;
            _ruleLevelDataList = [];
            _categoryLevelDataList = [];
            _rootLevelDataList = [];
        }


        public void Increse(List<RMDiscoveryAOSPSiteRuleLevelRotData> siteDataList)
        {
            if (_jobType == RMDiscoveryJobType.Rescan)
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
                    data = new RMDiscoveryAOSPContainerRuleLevelRotData
                    {
                        ContainerId = _containerId,
                        WithoutInDate = siteData.WithoutInDate,
                        FileExtension = siteData.FileExtension,
                        SizeRange = siteData.SizeRange,
                        Rule = siteData.Rule
                    };
                    _ruleLevelDataList.Add(data);
                }

                data.FileTotalSize += siteData.FileTotalSize;
                data.FileSumCount += siteData.FileSumCount;
            }
        }

        public void Increse(List<RMDiscoveryAOSPSiteCategoryLevelRotData> siteDataList)
        {
            if (_jobType == RMDiscoveryJobType.Rescan)
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
                    data = new RMDiscoveryAOSPContainerCategoryLevelRotData
                    {
                        ContainerId = _containerId,
                        WithoutInDate = siteData.WithoutInDate,
                        FileExtension = siteData.FileExtension,
                        SizeRange = siteData.SizeRange,
                        Category = siteData.Category
                    };
                    _categoryLevelDataList.Add(data);
                }

                data.FileTotalSize += siteData.FileTotalSize;
                data.FileSumCount += siteData.FileSumCount;
            }
        }

        public void Increse(List<RMDiscoveryAOSPSiteRootLevelRotData> siteDataList)
        {
            if (_jobType == RMDiscoveryJobType.Rescan)
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
                    data = new RMDiscoveryAOSPContainerRootLevelRotData
                    {
                        ContainerId = _containerId,
                        WithoutInDate = siteData.WithoutInDate,
                        FileExtension = siteData.FileExtension,
                        SizeRange = siteData.SizeRange,
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

        private async Task<bool> RuleLevelSaveAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Rescan)
                {
                    return await RuleLevelRecalculateAndSaveAsync();
                }

                if (_ruleLevelDataList.Count > 0)
                {
                    await _dataDao.AddOrUpdateContainerRuleLevelRotDataUnderSameContainerAsync(_o365TenantId, _ruleLevelDataList.ToArray());
                }

                _logger.Info($"Succeed save tenant [{_o365TenantId}] container [{_containerId}] [{_ruleLevelDataList.Count}] rule level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while save tenant [{_o365TenantId}] container [{_containerId}] rule level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RuleLevelRecalculateAndSaveAsync()
        {
            try
            {
                var enumerableDataList = _dataDao.GetSiteRuleLevelRotDataByContainerIdAsync(_o365TenantId, _containerId);
                await foreach (var data in enumerableDataList)
                {
                    var containerRotData = _ruleLevelDataList.FirstOrDefault(item =>
                        item.WithoutInDate == data.WithoutInDate &&
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange &&
                        item.Rule == data.Rule
                    );

                    if (containerRotData == null)
                    {
                        containerRotData = new RMDiscoveryAOSPContainerRuleLevelRotData
                        {
                            ContainerId = _containerId,
                            WithoutInDate = data.WithoutInDate,
                            FileExtension = data.FileExtension,
                            SizeRange = data.SizeRange,
                            Rule = data.Rule,
                            FileTotalSize = 0,
                            FileSumCount = 0,
                        };
                        _ruleLevelDataList.Add(containerRotData);
                    }

                    containerRotData.FileTotalSize += data.FileTotalSize;
                    containerRotData.FileSumCount += data.FileSumCount;
                }

                await _dataDao.AddOrUpdateContainerRuleLevelRotDataUnderSameContainerAsync(_o365TenantId, _ruleLevelDataList.ToArray());

                _logger.Info($"Succeed recalculate and save tenant [{_o365TenantId}] container [{_containerId}] [{_ruleLevelDataList.Count}] rule level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_o365TenantId}] container [{_containerId}] rule level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CategoryLevelSaveAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Rescan)
                {
                    return await CategoryLevelRecalculateAndSaveAsync();
                }

                if (_categoryLevelDataList.Count > 0)
                {
                    await _dataDao.AddOrUpdateContainerCategoryLevelRotDataUnderSameContainerAsync(_o365TenantId, _categoryLevelDataList.ToArray());
                }

                _logger.Info($"Succeed save tenant [{_o365TenantId}] container [{_containerId}] [{_categoryLevelDataList.Count}] category level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while save tenant [{_o365TenantId}] container [{_containerId}] category level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CategoryLevelRecalculateAndSaveAsync()
        {
            try
            {
                var enumerableDataList = _dataDao.GetSiteCategoryLevelRotDataByContainerIdAsync(_o365TenantId, _containerId);
                await foreach (var data in enumerableDataList)
                {
                    var containerRotData = _categoryLevelDataList.FirstOrDefault(item =>
                        item.WithoutInDate == data.WithoutInDate &&
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange &&
                        item.Category == data.Category
                    );

                    if (containerRotData == null)
                    {
                        containerRotData = new RMDiscoveryAOSPContainerCategoryLevelRotData
                        {
                            ContainerId = _containerId,
                            WithoutInDate = data.WithoutInDate,
                            FileExtension = data.FileExtension,
                            SizeRange = data.SizeRange,
                            Category = data.Category,
                            FileTotalSize = 0,
                            FileSumCount = 0,
                        };
                        _categoryLevelDataList.Add(containerRotData);
                    }

                    containerRotData.FileTotalSize += data.FileTotalSize;
                    containerRotData.FileSumCount += data.FileSumCount;
                }

                await _dataDao.AddOrUpdateContainerCategoryLevelRotDataUnderSameContainerAsync(_o365TenantId, _categoryLevelDataList.ToArray());

                _logger.Info($"Succeed recalculate and save tenant [{_o365TenantId}] container [{_containerId}] [{_categoryLevelDataList.Count}] category level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_o365TenantId}] container [{_containerId}] category level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RootLevelSaveAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Rescan)
                {
                    return await RootLevelRecalculateAndSaveAsync();
                }

                if (_rootLevelDataList.Count > 0)
                {
                    await _dataDao.AddOrUpdateContainerRootLevelRotDataUnderSameContainerAsync(_o365TenantId, _rootLevelDataList.ToArray());
                }

                _logger.Info($"Succeed save tenant [{_o365TenantId}] container [{_containerId}] [{_rootLevelDataList.Count}] root level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while save tenant [{_o365TenantId}] container [{_containerId}] root level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RootLevelRecalculateAndSaveAsync()
        {
            try
            {
                var enumerableDataList = _dataDao.GetSiteRootLevelRotDataByContainerIdAsync(_o365TenantId, _containerId);
                await foreach (var data in enumerableDataList)
                {
                    var containerRotData = _rootLevelDataList.FirstOrDefault(item =>
                        item.WithoutInDate == data.WithoutInDate &&
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange
                    );

                    if (containerRotData == null)
                    {
                        containerRotData = new RMDiscoveryAOSPContainerRootLevelRotData
                        {
                            ContainerId = _containerId,
                            WithoutInDate = data.WithoutInDate,
                            FileExtension = data.FileExtension,
                            SizeRange = data.SizeRange,
                            FileTotalSize = 0,
                            FileSumCount = 0,
                        };
                        _rootLevelDataList.Add(containerRotData);
                    }

                    containerRotData.FileTotalSize += data.FileTotalSize;
                    containerRotData.FileSumCount += data.FileSumCount;
                }

                await _dataDao.AddOrUpdateContainerRootLevelRotDataUnderSameContainerAsync(_o365TenantId, _rootLevelDataList.ToArray());

                _logger.Info($"Succeed recalculate and save tenant [{_o365TenantId}] container [{_containerId}] [{_rootLevelDataList.Count}] root level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_o365TenantId}] container [{_containerId}] root level rot data. Error: {e}");
                return false;
            }
        }
    }
}
