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
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Analyzer.V1.General.Rot
{
    public class RMDiscoveryFSContainerRotDataAnalyzer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryFSContainerRotDataAnalyzer));

        private readonly IRMDiscoveryFSDataDao _dataDao;

        private readonly RMDiscoveryJobType _jobType;

        private readonly string _connectionId;

        private readonly int _containerId;

        private readonly List<RMDiscoveryFSContainerRuleLevelRotData> _ruleLevelDataList;

        private readonly List<RMDiscoveryFSContainerCategoryLevelRotData> _categoryLevelDataList;

        private readonly List<RMDiscoveryFSContainerRootLevelRotData> _rootLevelDataList;

        public RMDiscoveryFSContainerRotDataAnalyzer(
            RMDiscoveryJobType jobType,
            string connectionId,
            int containerId
            )
        {
            _dataDao = new RMDiscoveryFSDataDao();
            _jobType = jobType;
            _connectionId = connectionId;
            _containerId = containerId;
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
                    data = new RMDiscoveryFSContainerRuleLevelRotData
                    {
                        ContainerId = _containerId,
                        WithoutInDate = connectionData.WithoutInDate,
                        FileExtension = connectionData.FileExtension,
                        SizeRange = connectionData.SizeRange,
                        Rule = connectionData.Rule
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
                    data = new RMDiscoveryFSContainerCategoryLevelRotData
                    {
                        ContainerId = _containerId,
                        WithoutInDate = connectionData.WithoutInDate,
                        FileExtension = connectionData.FileExtension,
                        SizeRange = connectionData.SizeRange,
                        Category = connectionData.Category
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
                    data = new RMDiscoveryFSContainerRootLevelRotData
                    {
                        ContainerId = _containerId,
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

        private async Task<bool> RuleLevelSaveAsync()
        {
            try
            {
                if (_ruleLevelDataList.Count > 0)
                {
                    await _dataDao.AddOrUpdateContainerRuleLevelRotDataUnderSameContainerAsync(_ruleLevelDataList.ToArray());
                }

                _logger.Info($"Succeed save connection [{_connectionId}] container [{_containerId}] [{_ruleLevelDataList.Count}] rule level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while save connection [{_connectionId}] container [{_containerId}] rule level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> CategoryLevelSaveAsync()
        {
            try
            {
                if (_categoryLevelDataList.Count > 0)
                {
                    await _dataDao.AddOrUpdateContainerCategoryLevelRotDataUnderSameContainerAsync(_categoryLevelDataList.ToArray());
                }

                _logger.Info($"Succeed save connection [{_connectionId}] container [{_containerId}] [{_categoryLevelDataList.Count}] category level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while save connection [{_connectionId}] container [{_containerId}] category level rot data. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RootLevelSaveAsync()
        {
            try
            {
                if (_rootLevelDataList.Count > 0)
                {
                    await _dataDao.AddOrUpdateContainerRootLevelRotDataUnderSameContainerAsync(_rootLevelDataList.ToArray());
                }
                _logger.Info($"Succeed save connection [{_connectionId}] container [{_containerId}] [{_rootLevelDataList.Count}] root level rot data.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while save connection [{_connectionId}] container [{_containerId}] root level rot data. Error: {e}");
                return false;
            }
        }
    }
}
