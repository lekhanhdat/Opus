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
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model.Discovery.Google;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.General.Rot
{
    public class RMDiscoveryGoogleContainerRotDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleContainerRotDataAnalyzer));

        private readonly IRMDiscoveryGoogleDataDao _dataDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly string _googleOrganizationId;

        private readonly int _containerId;

        private readonly List<RMDiscoveryGoogleContainerRuleLevelRotData> _ruleLevelDataList;

        private readonly List<RMDiscoveryGoogleContainerCategoryLevelRotData> _categoryLevelDataList;

        private readonly List<RMDiscoveryGoogleContainerRootLevelRotData> _rootLevelDataList;

        public RMDiscoveryGoogleContainerRotDataAnalyzer(
           RMDiscoveryJobType jobType,
           string googleOrganizationId,
           int containerId
           )
        {
            _dataDao = new RMDiscoveryGoogleDataDao();
            _jobType = jobType;
            _googleOrganizationId = googleOrganizationId;
            _containerId = containerId;
            _ruleLevelDataList = [];
            _categoryLevelDataList = [];
            _rootLevelDataList = [];
        }

        public void Increse(List<RMDiscoveryGoogleDriveRuleLevelRotData> driveDataList)
        {
            if (_jobType == RMDiscoveryJobType.Retry)
            {
                return;
            }

            foreach (var driveData in driveDataList)
            {
                var data = _ruleLevelDataList.FirstOrDefault(item =>
                    item.FileExtension == driveData.FileExtension &&
                    item.SizeRange == driveData.SizeRange &&
                    item.WithoutInDate == driveData.WithoutInDate &&
                    item.Rule == driveData.Rule
                );

                if (data == null)
                {
                    data = new RMDiscoveryGoogleContainerRuleLevelRotData
                    {
                        ContainerId = _containerId,
                        WithoutInDate = driveData.WithoutInDate,
                        FileExtension = driveData.FileExtension,
                        SizeRange = driveData.SizeRange,
                        Rule = driveData.Rule
                    };
                    _ruleLevelDataList.Add(data);
                }

                data.FileTotalSize += driveData.FileTotalSize;
                data.FileSumCount += driveData.FileSumCount;
            }
        }

        public void Increse(List<RMDiscoveryGoogleDriveCategoryLevelRotData> driveDataList)
        {
            if (_jobType == RMDiscoveryJobType.Retry)
            {
                return;
            }

            foreach (var driveData in driveDataList)
            {
                var data = _categoryLevelDataList.FirstOrDefault(item =>
                    item.FileExtension == driveData.FileExtension &&
                    item.SizeRange == driveData.SizeRange &&
                    item.WithoutInDate == driveData.WithoutInDate &&
                    item.Category == driveData.Category
                );

                if (data == null)
                {
                    data = new RMDiscoveryGoogleContainerCategoryLevelRotData
                    {
                        ContainerId = _containerId,
                        WithoutInDate = driveData.WithoutInDate,
                        FileExtension = driveData.FileExtension,
                        SizeRange = driveData.SizeRange,
                        Category = driveData.Category
                    };
                    _categoryLevelDataList.Add(data);
                }

                data.FileTotalSize += driveData.FileTotalSize;
                data.FileSumCount += driveData.FileSumCount;
            }
        }

        public void Increse(List<RMDiscoveryGoogleDriveRootLevelRotData> driveDataList)
        {
            if (_jobType == RMDiscoveryJobType.Retry)
            {
                return;
            }

            foreach (var driveData in driveDataList)
            {
                var data = _rootLevelDataList.FirstOrDefault(item =>
                    item.FileExtension == driveData.FileExtension &&
                    item.SizeRange == driveData.SizeRange &&
                    item.WithoutInDate == driveData.WithoutInDate
                );

                if (data == null)
                {
                    data = new RMDiscoveryGoogleContainerRootLevelRotData
                    {
                        ContainerId = _containerId,
                        WithoutInDate = driveData.WithoutInDate,
                        FileExtension = driveData.FileExtension,
                        SizeRange = driveData.SizeRange,
                    };
                    _rootLevelDataList.Add(data);
                }

                data.FileTotalSize += driveData.FileTotalSize;
                data.FileSumCount += driveData.FileSumCount;
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
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    return await RuleLevelRecalculateAndSaveAsync();
                }

                if (_ruleLevelDataList.Count > 0)
                {
                    await _dataDao.AddOrUpdateContainerRuleLevelRotDataUnderSameContainerAsync(_googleOrganizationId, _ruleLevelDataList.ToArray());
                }

                _logger.Info($"Succeed save tenant [{_googleOrganizationId}] container [{_containerId}] [{_ruleLevelDataList.Count}] rule level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while save tenant [{_googleOrganizationId}] container [{_containerId}] rule level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RuleLevelRecalculateAndSaveAsync()
        {
            try
            {
                var enumerableDataList = _dataDao.GetDriveRuleLevelRotDataByContainerIdAsync(_googleOrganizationId, _containerId);
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
                        containerRotData = new RMDiscoveryGoogleContainerRuleLevelRotData
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

                await _dataDao.AddOrUpdateContainerRuleLevelRotDataUnderSameContainerAsync(_googleOrganizationId, _ruleLevelDataList.ToArray());

                _logger.Info($"Succeed recalculate and save tenant [{_googleOrganizationId}] container [{_containerId}] [{_ruleLevelDataList.Count}] rule level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_googleOrganizationId}] container [{_containerId}] rule level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CategoryLevelSaveAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    return await CategoryLevelRecalculateAndSaveAsync();
                }

                if (_categoryLevelDataList.Count > 0)
                {
                    await _dataDao.AddOrUpdateContainerCategoryLevelRotDataUnderSameContainerAsync(_googleOrganizationId, _categoryLevelDataList.ToArray());
                }

                _logger.Info($"Succeed save tenant [{_googleOrganizationId}] container [{_containerId}] [{_categoryLevelDataList.Count}] category level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while save tenant [{_googleOrganizationId}] container [{_containerId}] category level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CategoryLevelRecalculateAndSaveAsync()
        {
            try
            {
                var enumerableDataList = _dataDao.GetDriveCategoryLevelRotDataByContainerIdAsync(_googleOrganizationId, _containerId);
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
                        containerRotData = new RMDiscoveryGoogleContainerCategoryLevelRotData
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

                await _dataDao.AddOrUpdateContainerCategoryLevelRotDataUnderSameContainerAsync(_googleOrganizationId, _categoryLevelDataList.ToArray());

                _logger.Info($"Succeed recalculate and save tenant [{_googleOrganizationId}] container [{_containerId}] [{_categoryLevelDataList.Count}] category level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_googleOrganizationId}] container [{_containerId}] category level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RootLevelSaveAsync()
        {
            try
            {
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    return await RootLevelRecalculateAndSaveAsync();
                }

                if (_rootLevelDataList.Count > 0)
                {
                    await _dataDao.AddOrUpdateContainerRootLevelRotDataUnderSameContainerAsync(_googleOrganizationId, _rootLevelDataList.ToArray());
                }

                _logger.Info($"Succeed save tenant [{_googleOrganizationId}] container [{_containerId}] [{_rootLevelDataList.Count}] root level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while save tenant [{_googleOrganizationId}] container [{_containerId}] root level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RootLevelRecalculateAndSaveAsync()
        {
            try
            {
                var enumerableDataList = _dataDao.GetDriveRootLevelRotDataByContainerIdAsync(_googleOrganizationId, _containerId);
                await foreach (var data in enumerableDataList)
                {
                    var containerRotData = _rootLevelDataList.FirstOrDefault(item =>
                        item.WithoutInDate == data.WithoutInDate &&
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange
                    );

                    if (containerRotData == null)
                    {
                        containerRotData = new RMDiscoveryGoogleContainerRootLevelRotData
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

                await _dataDao.AddOrUpdateContainerRootLevelRotDataUnderSameContainerAsync(_googleOrganizationId, _rootLevelDataList.ToArray());

                _logger.Info($"Succeed recalculate and save tenant [{_googleOrganizationId}] container [{_containerId}] [{_rootLevelDataList.Count}] root level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_googleOrganizationId}] container [{_containerId}] root level rot data. Error: {e}");
                return false;
            }
        }
    }
}
