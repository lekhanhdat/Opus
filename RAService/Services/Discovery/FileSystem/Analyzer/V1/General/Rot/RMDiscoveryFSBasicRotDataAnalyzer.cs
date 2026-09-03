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
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Analyzer.V1.General.Rot
{
    public class RMDiscoveryFSBasicRotDataAnalyzer
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryFSBasicRotDataAnalyzer));

        private readonly IRMDiscoveryFSDataDao _dataDao;

        private readonly IRMDiscoveryFSNodeDao _nodeDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly List<RMDiscoveryFSBasicRuleLevelRotData> _ruleLevelDataList;

        private readonly List<RMDiscoveryFSBasicCategoryLevelRotData> _categoryLevelDataList;

        private readonly List<RMDiscoveryFSBasicRootLevelRotData> _rootLevelDataList;

        public RMDiscoveryFSBasicRotDataAnalyzer(
            RMDiscoveryJobType jobType
        )
        {
            _dataDao = new RMDiscoveryFSDataDao();
            _nodeDao = new RMDiscoveryFSNodeDao();
            _jobType = jobType;
            _ruleLevelDataList = [];
            _categoryLevelDataList = [];
            _rootLevelDataList = [];
        }

        public void Increse(List<RMDiscoveryFSConnectionRuleLevelRotData> connectionDataList)
        {
            if (_jobType == RMDiscoveryJobType.Retry)
            {
                return;
            }

            foreach (var connectionData in connectionDataList)
            {
                var data = _ruleLevelDataList.FirstOrDefault(item =>
                        item.FileExtension == connectionData.FileExtension &&
                        item.SizeRange == connectionData.SizeRange &&
                        item.WithoutInDate == connectionData.WithoutInDate &&
                        item.Rule == connectionData.Rule
                    );
                if (data == null)
                {
                    data = new RMDiscoveryFSBasicRuleLevelRotData
                    {
                        WithoutInDate = connectionData.WithoutInDate,
                        FileExtension = connectionData.FileExtension,
                        SizeRange = connectionData.SizeRange,
                        Rule = connectionData.Rule,
                    };
                    _ruleLevelDataList.Add(data);
                }
                data.FileTotalSize += connectionData.FileTotalSize;
                data.FileSumCount += connectionData.FileSumCount;
            }
        }

        public void Increse(List<RMDiscoveryFSConnectionCategoryLevelRotData> connectionDataList)
        {
            if (_jobType == RMDiscoveryJobType.Retry)
            {
                return;
            }

            foreach (var connectionData in connectionDataList)
            {
                var data = _categoryLevelDataList.FirstOrDefault(item =>
                        item.FileExtension == connectionData.FileExtension &&
                        item.SizeRange == connectionData.SizeRange &&
                        item.WithoutInDate == connectionData.WithoutInDate &&
                        item.Category == connectionData.Category
                    );
                if (data == null)
                {
                    data = new RMDiscoveryFSBasicCategoryLevelRotData
                    {
                        WithoutInDate = connectionData.WithoutInDate,
                        FileExtension = connectionData.FileExtension,
                        SizeRange = connectionData.SizeRange,
                        Category = connectionData.Category,
                    };
                    _categoryLevelDataList.Add(data);
                }
                data.FileTotalSize += connectionData.FileTotalSize;
                data.FileSumCount += connectionData.FileSumCount;
            }
        }

        public void Increse(List<RMDiscoveryFSConnectionRootLevelRotData> connectionDataList)
        {
            if (_jobType == RMDiscoveryJobType.Retry)
            {
                return;
            }

            foreach (var connectionData in connectionDataList)
            {
                var data = _rootLevelDataList.FirstOrDefault(item =>
                        item.FileExtension == connectionData.FileExtension &&
                        item.SizeRange == connectionData.SizeRange &&
                        item.WithoutInDate == connectionData.WithoutInDate
                    );
                if (data == null)
                {
                    data = new RMDiscoveryFSBasicRootLevelRotData
                    {
                        WithoutInDate = connectionData.WithoutInDate,
                        FileExtension = connectionData.FileExtension,
                        SizeRange = connectionData.SizeRange,
                    };
                    _rootLevelDataList.Add(data);
                }
                data.FileTotalSize += connectionData.FileTotalSize;
                data.FileSumCount += connectionData.FileSumCount;
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
                await _dataDao.AddOrUpdateBasicRuleLevelRotDataUnderSameContentSourceAsync(_ruleLevelDataList.ToArray());

                _logger.Info($"Succeed save [{_ruleLevelDataList.Count}] basic rule level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while save [{_ruleLevelDataList.Count}] basic rule level rot data. Error: {e}");
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

                await _dataDao.AddOrUpdateBasicCategoryLevelRotDataUnderSameContentSourceAsync(_categoryLevelDataList.ToArray());

                _logger.Info($"Succeed save [{_categoryLevelDataList.Count}] basic category level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while save [{_categoryLevelDataList.Count}] basic category level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CategoryLevelAppendAndSaveAsync()
        {
            try
            {
                var existsDataList = await _dataDao.GetBasicCategoryLevelRotDataListAsync();
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
                        existsBaicData = new RMDiscoveryFSBasicCategoryLevelRotData
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

                await _dataDao.AddOrUpdateBasicCategoryLevelRotDataUnderSameContentSourceAsync(existsDataList.ToArray());

                _logger.Info($"Succeed append and save [{existsDataList.Count}] basic category level rot data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while append and save [{_categoryLevelDataList.Count}] basic category level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CategoryLevelRecalculateAndSaveAsync()
        {
            try
            {
                var containerIds = (await _nodeDao.GetAllDiscoveryContainersAsync()).Select(item => item.Id).ToHashSet();
                foreach (var containerId in containerIds)
                {
                    var containerDataList = _dataDao.GetContainerCategoryLevelRotDataListAsync(containerId);
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
                            data = new RMDiscoveryFSBasicCategoryLevelRotData
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

                await _dataDao.AddOrUpdateBasicCategoryLevelRotDataUnderSameContentSourceAsync(_categoryLevelDataList.ToArray());

                _logger.Info($"Succeed recalculate and save [{_categoryLevelDataList.Count}] basic category level rot data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save [{_categoryLevelDataList.Count}] basic category level rot data. Error: {e}");
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

                await _dataDao.AddOrUpdateBasicRootLevelRotDataUnderSameContentSourceAsync(_rootLevelDataList.ToArray());

                _logger.Info($"Succeed save [{_rootLevelDataList.Count}] basic root level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while save [{_rootLevelDataList.Count}] basic root level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RootLevelAppendAndSaveAsync()
        {
            try
            {
                var existsDataList = await _dataDao.GetBasicRootLevelRotDataListAsync();
                foreach (var data in _rootLevelDataList)
                {
                    var existsBaicData = existsDataList.FirstOrDefault(item =>
                        item.FileExtension == data.FileExtension &&
                        item.SizeRange == data.SizeRange &&
                        item.WithoutInDate == data.WithoutInDate
                    );
                    if (existsBaicData == null)
                    {
                        existsBaicData = new RMDiscoveryFSBasicRootLevelRotData
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

                await _dataDao.AddOrUpdateBasicRootLevelRotDataUnderSameContentSourceAsync(existsDataList.ToArray());

                _logger.Info($"Succeed append and save [{existsDataList.Count}] basic root level rot data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while append and save [{_rootLevelDataList.Count}] basic root level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RootLevelRecalculateAndSaveAsync()
        {
            try
            {
                var containerIds = (await _nodeDao.GetAllDiscoveryContainersAsync()).Select(item => item.Id).ToHashSet();
                foreach (var containerId in containerIds)
                {
                    var containerDataList = _dataDao.GetContainerRootLevelRotDataListAsync(containerId);
                    await foreach (var containerData in containerDataList)
                    {
                        var data = _rootLevelDataList.FirstOrDefault(item =>
                                item.WithoutInDate == containerData.WithoutInDate &&
                                item.FileExtension == containerData.FileExtension &&
                                item.SizeRange == containerData.SizeRange
                            );
                        if (data == null)
                        {
                            data = new RMDiscoveryFSBasicRootLevelRotData
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

                await _dataDao.AddOrUpdateBasicRootLevelRotDataUnderSameContentSourceAsync(_rootLevelDataList.ToArray());

                _logger.Info($"Succeed recalculate and save [{_rootLevelDataList.Count}] basic root level rot data.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while recalculate and save [{_rootLevelDataList.Count}] basic root level rot data. Error: {e}");
                return false;
            }
        }
    }
}
