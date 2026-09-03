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
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model.Discovery.Google;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Analyzer.V1.General.Rot
{
    public class RMDiscoveryGoogleBasicRotDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleBasicRotDataAnalyzer));

        private readonly IRMDiscoveryGoogleDataDao _dataDao;

        private readonly IRMDiscoveryGoogleNodeDao _nodeDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly string _googleOrganizationId;

        private readonly SourceFlag _contentSource = SourceFlag.Google;

        private readonly List<RMDiscoveryGoogleBasicRuleLevelRotData> _ruleLevelDataList;

        private readonly List<RMDiscoveryGoogleBasicCategoryLevelRotData> _categoryLevelDataList;

        private readonly List<RMDiscoveryGoogleBasicRootLevelRotData> _rootLevelDataList;

        public RMDiscoveryGoogleBasicRotDataAnalyzer(RMDiscoveryJobType jobType, string googleOrganizationId)
        {
            _dataDao = new RMDiscoveryGoogleDataDao();
            _nodeDao = new RMDiscoveryGoogleNodeDao();
            _jobType = jobType;
            _googleOrganizationId = googleOrganizationId;
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
                    data = new RMDiscoveryGoogleBasicRuleLevelRotData
                    {
                        WithoutInDate = driveData.WithoutInDate,
                        FileExtension = driveData.FileExtension,
                        SizeRange = driveData.SizeRange,
                        Rule = driveData.Rule,
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
                    data = new RMDiscoveryGoogleBasicCategoryLevelRotData
                    {
                        WithoutInDate = driveData.WithoutInDate,
                        FileExtension = driveData.FileExtension,
                        SizeRange = driveData.SizeRange,
                        Category = driveData.Category,
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
                    data = new RMDiscoveryGoogleBasicRootLevelRotData
                    {
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

                await _dataDao.AddOrUpdateBasicRuleLevelRotDataAsync(_googleOrganizationId, _ruleLevelDataList.ToArray());

                _logger.Info($"Succeed save tenant [{_googleOrganizationId}] [{_contentSource}] [{_ruleLevelDataList.Count}] basic rule level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while save tenant [{_googleOrganizationId}] [{_contentSource}] [{_ruleLevelDataList.Count}] basic rule level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RuleLevelAppendAndSaveAsync()
        {
            try
            {
                var existsDataList = await _dataDao.GetBasicRuleLevelRotDataListAsync(_googleOrganizationId);
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
                        existsBaicData = new RMDiscoveryGoogleBasicRuleLevelRotData
                        {
                            WithoutInDate = data.WithoutInDate,
                            FileExtension = data.FileExtension,
                            SizeRange = data.SizeRange,
                            Rule = data.Rule,
                        };
                        existsDataList.Add(existsBaicData);
                    }
                    existsBaicData.FileTotalSize += data.FileTotalSize;
                    existsBaicData.FileSumCount += data.FileSumCount;
                }

                await _dataDao.AddOrUpdateBasicRuleLevelRotDataAsync(_googleOrganizationId, existsDataList.ToArray());

                _logger.Info($"Succeed append and save tenant [{_googleOrganizationId}] [{_contentSource}] [{existsDataList.Count}] basic rule level rot data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while append and save tenant [{_googleOrganizationId}] [{_contentSource}] [{_ruleLevelDataList.Count}] basic rule level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RuleLevelRecalculateAndSaveAsync()
        {
            try
            {
                var containerIds = (await _nodeDao.GetAllDiscoveryGoogleContainersAsync(_googleOrganizationId)).Select(item => item.Id).ToHashSet();
                foreach (var containerId in containerIds)
                {
                    var containerDataList = _dataDao.GetContainerRuleLevelRotDataListAsync(_googleOrganizationId, containerId);
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
                            data = new RMDiscoveryGoogleBasicRuleLevelRotData
                            {
                                WithoutInDate = containerData.WithoutInDate,
                                FileExtension = containerData.FileExtension,
                                SizeRange = containerData.SizeRange,
                                Rule = containerData.Rule,
                                FileTotalSize = 0,
                                FileSumCount = 0,
                            };
                            _ruleLevelDataList.Add(data);
                        }

                        data.FileTotalSize += containerData.FileTotalSize;
                        data.FileSumCount += containerData.FileSumCount;
                    }
                }

                await _dataDao.AddOrUpdateBasicRuleLevelRotDataAsync(_googleOrganizationId, _ruleLevelDataList.ToArray());

                _logger.Info($"Succeed recalculate and save tenant [{_googleOrganizationId}] [{_contentSource}] [{_ruleLevelDataList.Count}] basic rule level rot data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_googleOrganizationId}] [{_contentSource}] [{_ruleLevelDataList.Count}] basic rule level rot data. Error: {e}");
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

                await _dataDao.AddOrUpdateBasicCategoryLevelRotDataAsync(_googleOrganizationId, _categoryLevelDataList.ToArray());

                _logger.Info($"Succeed save tenant [{_googleOrganizationId}] [{_contentSource}] [{_categoryLevelDataList.Count}] basic category level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while save tenant [{_googleOrganizationId}] [{_contentSource}] [{_categoryLevelDataList.Count}] basic category level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CategoryLevelAppendAndSaveAsync()
        {
            try
            {
                var existsDataList = await _dataDao.GetBasicCategoryLevelRotDataListAsync(_googleOrganizationId);
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
                        existsBaicData = new RMDiscoveryGoogleBasicCategoryLevelRotData
                        {
                            WithoutInDate = data.WithoutInDate,
                            FileExtension = data.FileExtension,
                            SizeRange = data.SizeRange,
                            Category = data.Category,
                        };
                        existsDataList.Add(existsBaicData);
                    }
                    existsBaicData.FileTotalSize += data.FileTotalSize;
                    existsBaicData.FileSumCount += data.FileSumCount;
                }

                await _dataDao.AddOrUpdateBasicCategoryLevelRotDataAsync(_googleOrganizationId, existsDataList.ToArray());

                _logger.Info($"Succeed append and save tenant [{_googleOrganizationId}] [{_contentSource}] [{existsDataList.Count}] basic category level rot data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while append and save tenant [{_googleOrganizationId}] [{_contentSource}] [{_categoryLevelDataList.Count}] basic category level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CategoryLevelRecalculateAndSaveAsync()
        {
            try
            {
                var containerIds = (await _nodeDao.GetAllDiscoveryGoogleContainersAsync(_googleOrganizationId)).Select(item => item.Id).ToHashSet();
                foreach (var containerId in containerIds)
                {
                    var containerDataList = _dataDao.GetContainerCategoryLevelRotDataListAsync(_googleOrganizationId, containerId);
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
                            data = new RMDiscoveryGoogleBasicCategoryLevelRotData
                            {
                                WithoutInDate = containerData.WithoutInDate,
                                FileExtension = containerData.FileExtension,
                                SizeRange = containerData.SizeRange,
                                Category = containerData.Category,
                                FileTotalSize = 0,
                                FileSumCount = 0,
                            };
                            _categoryLevelDataList.Add(data);
                        }

                        data.FileTotalSize += containerData.FileTotalSize;
                        data.FileSumCount += containerData.FileSumCount;
                    }
                }

                await _dataDao.AddOrUpdateBasicCategoryLevelRotDataAsync(_googleOrganizationId, _categoryLevelDataList.ToArray());

                _logger.Info($"Succeed recalculate and save tenant [{_googleOrganizationId}] [{_contentSource}] [{_categoryLevelDataList.Count}] basic category level rot data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_googleOrganizationId}] [{_contentSource}] [{_categoryLevelDataList.Count}] basic category level rot data. Error: {e}");
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

                await _dataDao.AddOrUpdateBasicRootLevelRotDataAsync(_googleOrganizationId, _rootLevelDataList.ToArray());

                _logger.Info($"Succeed save tenant [{_googleOrganizationId}] [{_contentSource}] [{_rootLevelDataList.Count}] basic root level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while save tenant [{_googleOrganizationId}] [{_contentSource}] [{_rootLevelDataList.Count}] basic root level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RootLevelAppendAndSaveAsync()
        {
            try
            {
                var existsDataList = await _dataDao.GetBasicRootLevelRotDataListAsync(_googleOrganizationId);
                foreach (var data in _rootLevelDataList)
                {
                    var existsBaicData = existsDataList.FirstOrDefault(item =>
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange &&
                        item.WithoutInDate == data.WithoutInDate
                    );
                    if (existsBaicData == null)
                    {
                        existsBaicData = new RMDiscoveryGoogleBasicRootLevelRotData
                        {
                            WithoutInDate = data.WithoutInDate,
                            FileExtension = data.FileExtension,
                            SizeRange = data.SizeRange,
                        };
                        existsDataList.Add(existsBaicData);
                    }
                    existsBaicData.FileTotalSize += data.FileTotalSize;
                    existsBaicData.FileSumCount += data.FileSumCount;
                }

                await _dataDao.AddOrUpdateBasicRootLevelRotDataAsync(_googleOrganizationId, existsDataList.ToArray());

                _logger.Info($"Succeed append and save tenant [{_googleOrganizationId}] [{_contentSource}] [{existsDataList.Count}] basic root level rot data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while append and save tenant [{_googleOrganizationId}] [{_contentSource}] [{_rootLevelDataList.Count}] basic root level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RootLevelRecalculateAndSaveAsync()
        {
            try
            {
                var containerIds = (await _nodeDao.GetAllDiscoveryGoogleContainersAsync(_googleOrganizationId)).Select(item => item.Id).ToHashSet();
                foreach (var containerId in containerIds)
                {
                    var containerDataList = _dataDao.GetContainerRootLevelRotDataListAsync(_googleOrganizationId, containerId);
                    await foreach (var containerData in containerDataList)
                    {
                        var data = _rootLevelDataList.FirstOrDefault(item =>
                                item.WithoutInDate == containerData.WithoutInDate &&
                                item.FileExtension == containerData.FileExtension &&
                                item.SizeRange == containerData.SizeRange
                            );
                        if (data == null)
                        {
                            data = new RMDiscoveryGoogleBasicRootLevelRotData
                            {
                                WithoutInDate = containerData.WithoutInDate,
                                FileExtension = containerData.FileExtension,
                                SizeRange = containerData.SizeRange,
                                FileTotalSize = 0,
                                FileSumCount = 0,
                            };
                            _rootLevelDataList.Add(data);
                        }

                        data.FileTotalSize += containerData.FileTotalSize;
                        data.FileSumCount += containerData.FileSumCount;
                    }
                }

                await _dataDao.AddOrUpdateBasicRootLevelRotDataAsync(_googleOrganizationId, _rootLevelDataList.ToArray());

                _logger.Info($"Succeed recalculate and save tenant [{_googleOrganizationId}] [{_contentSource}] [{_rootLevelDataList.Count}] basic root level rot data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save tenant [{_googleOrganizationId}] [{_contentSource}] [{_rootLevelDataList.Count}] basic root level rot data. Error: {e}");
                return false;
            }
        }

    }
}
