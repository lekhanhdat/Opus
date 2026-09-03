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
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.FileSystem;
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.FileSystem.License;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Preparer
{
    public class RMDiscoveryFSJobNewlyPreparer(bool needToReregisterTags) : RMDiscoveryFSWorker, IRMDiscoveryFSJobPreparer
    {
        public async Task<(bool success, string errorMessage)> PrepareAsync()
        {
            try
            {
                var (has, mainJob) = await _jobDao.TryGetProcessingMainJobAsync();

                if (has)
                {
                    _logger.Error($"There is already job [{mainJob.Id}] begin executed.");
                    return (false, I18NEntity.GetString("RM_FA_DiscoveryJob_HasRunningJob"));
                }

                var licenseType = await RMDiscoveryFSLicenseHelper.GetLicenseTypeAsync();
                var (containersCount, connectionCount) = await CalculateFSConnectionCountAsync();

                _logger.Info($"The number of containers to be executed for this File system [{RMDiscoveryJobType.Newly}] job is [{containersCount}], and the number of connection is [{connectionCount}].");

                if (containersCount == 0 | connectionCount == 0)
                {
                    return (false, I18NEntity.GetString("RM_JM_Report_Skip_NoAvailableConnections"));
                }

                mainJob = new RMDiscoveryFSMainJob
                {
                    Id = Guid.NewGuid(),
                    StartTime = DateTime.UtcNow.Ticks,
                    ContainersCount = containersCount,
                    ConnectionCount = connectionCount,
                    Status = RMDiscoveryJobStatus.Preparing,
                    ProfileJobInitStatus = RMDiscoveryJobStatus.None,
                    Type = RMDiscoveryJobType.Newly,
                    Version = RMDiscoveryJobVersion.V1,
                    NeedToReRegisterTags = needToReregisterTags
                };

                await _jobDao.AddOrUpdateMainJobAsync(mainJob);
                await RMDiscoveryFSLicenseHelper.IncreaseConsumedFrequencyPerYearAsync();
                await _executionInfoDao.GenerateByMainJobAsync(mainJob.Id, licenseType);

                _logger.Info($"Discovery File System [{RMDiscoveryJobType.Newly}] job [{mainJob.Id}] is prepared.");

                return (true, string.Empty);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while prepare discovery File System [{RMDiscoveryJobType.Newly}] job.Error: {e}");
                return (false, string.Empty);
            }
        }

        #region Private methods

        private async Task<(int containersCount, int sitesCount)> CalculateFSConnectionCountAsync()
        {
            int needProcessConnectionCount = 0;
            var scopeInfo = await _configurationDao.GetAsync<RMDiscoveryFSScopeInfo>(RMDiscoveryConfigurationType.FileSystemNewlyScope);

            _logger.Info($"The scope of this File System [{RMDiscoveryJobType.Newly}] job is [{scopeInfo.ScopeType}].");

            if(scopeInfo.ScopeType == RMDiscoveryFSScopeType.All)
            {
                var groups = _nodeDao.LoadAllGroupsWithoutConnection();
                needProcessConnectionCount = await _nodeDao.CalculateConnectionCount(groups.Select(i => i.Id).ToList());
                return (groups.Count, needProcessConnectionCount);
            }
            
            _logger.Info($"The containers affected by this File System [{RMDiscoveryJobType.Newly}] job are [{string.Join(",", scopeInfo.SpecifyContainerIds)}].");
            
            needProcessConnectionCount = await _nodeDao.CalculateConnectionCount(scopeInfo.SpecifyContainerIds);
            return (scopeInfo.SpecifyContainerIds.Count, needProcessConnectionCount);
        }

        #endregion
    }
}
