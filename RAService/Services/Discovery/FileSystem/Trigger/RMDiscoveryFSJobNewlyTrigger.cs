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
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.FileSystem;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Work;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Trigger;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Trigger
{
    public class RMDiscoveryFSJobNewlyTrigger : RMDiscoveryFSWorker, IRMDiscoveryFSJobTriggerible
    {
        public RMDiscoveryFSJobNewlyTrigger(RMDiscoveryFSMainJob jobInfo) : base() { }

        public async Task<(bool succeed, List<(FSConnectionGroup group, List<FSConnection> connections)> items)> GetWillTriggerJobsAsync()
        {
            try
            {
                var scopeInfo = await _configurationDao.GetAsync<RMDiscoveryFSScopeInfo>(RMDiscoveryConfigurationType.FileSystemNewlyScope);
                var res = new List<(FSConnectionGroup group, List<FSConnection> connections)>();
                var willTriggerContainers = await GetWillTriggerJobContainers(scopeInfo);
                _logger.Info($"This File System [{RMDiscoveryJobType.Newly}] job is will execute as containers [{string.Join(", \n", willTriggerContainers.Select(item => item.Id))}] of scope [{scopeInfo.ScopeType}].");
                foreach (var willTriggerContainer in willTriggerContainers)
                {
                    if (!willTriggerContainer.FSConnections.Any())
                    {
                        _logger.Info($"Container [{willTriggerContainer.Id}] has no connection that need to trigger [{RMDiscoveryJobType.Newly}] job.");
                        continue;
                    }
                    res.Add((willTriggerContainer, willTriggerContainer.FSConnections));
                }
                _logger.Info($"Successful allocate will trigger jobs: [{res.Count}].");
                return (true, res);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get will trigger jobs. Error: {e}");
                return (false, []);
            }
        }

        public async Task<bool> InitTablesAsync()
        {
            try
            {
                await RMDiscoveryDBManager.DropFileSystemTablesAsync();
                await RMDiscoveryDBManager.InitFileSystemBasicTablesAsync();
                await RMDiscoveryDBManager.InitFileSystemRotTablesAsync();
                await RMDiscoveryDBManager.InitFileSystemInactiveTablesAsync();
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init tables async. Error: {e}");
                return false;
            }
        }


        private async Task<List<FSConnectionGroup>> GetWillTriggerJobContainers(RMDiscoveryFSScopeInfo scopeInfo)
        {
            if (scopeInfo.ScopeType == RMDiscoveryFSScopeType.All)
            {
                return await _nodeDao.LoadAllGroupsWithConnection();
            }
            return await _nodeDao.LoadGroupsWithConnectionByIds(scopeInfo.SpecifyContainerIds);
        }
    }
}
