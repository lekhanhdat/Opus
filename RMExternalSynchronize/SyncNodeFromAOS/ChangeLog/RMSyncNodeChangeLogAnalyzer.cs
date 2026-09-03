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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Services;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMSynchronize.SyncNodeFromAOS.ChangeLog
{
    public class RMSyncNodeChangeLogAnalyzer
    {

        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMSyncNodeChangeLogAnalyzer));

        private static readonly IRMCache s_cache = PlatformWindsorManager.GetService<IRMCache>();

        private readonly SourceFlag _contentSource;

        public List<RMSyncNodeChangeInfo> MovedChangeInfoes { get; private set; } = new();

        public List<RMSyncNodeChangeInfo> AddedChangeInfoes { get; private set; } = new();

        public List<RMSyncNodeChangeInfo> DeletedChangeInfoes { get; private set; } = new();

        public List<RMSyncNodeChangeInfo> RenamedChangeInfoes { get; private set; } = new();

        public RMSyncNodeChangeLogAnalyzer(SourceFlag contentSource)
        {
            _contentSource = contentSource;
        }

        public async Task Analyze()
        {
            try
            {
                var changeInfoes = await s_cache.GetListAsync<RMSyncNodeChangeInfo>(RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_CACHE_KEY + RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_SOURCE_CACHE_KEY[_contentSource]);
                if(changeInfoes == null || !changeInfoes.Any())
                {
                    return;
                }
                var changeTypeMappings = changeInfoes.GroupBy(item => item.ChangeType, item => item)
                    .ToDictionary(item => item.Key, item => item.ToList());

                var movedChangeInfoes = new List<RMSyncNodeChangeInfo>();

                _ = changeTypeMappings.TryGetValue(RMSyncNodeChangeType.ChangeName, out var renamedChangeInfoes);
                _ = changeTypeMappings.TryGetValue(RMSyncNodeChangeType.Add, out var addedChangeInfoes);


                if (changeTypeMappings.TryGetValue(RMSyncNodeChangeType.Delete, out var deletedChangeInfoes)
                    && addedChangeInfoes != null
                    && addedChangeInfoes.Any())
                {
                    var addedChangeInfoMapping = addedChangeInfoes.ToDictionary(item => item.AosId, item => item);
                    var deletedChangeInfoMapping = deletedChangeInfoes.Where(item => !item.IsContainer).ToDictionary(item => item.AosId, item => item);

                    foreach (var deletedChangeInfo in deletedChangeInfoMapping)
                    {
                        if (addedChangeInfoMapping.TryGetValue(deletedChangeInfo.Key, out var addedChangeInfo))
                        {
                            addedChangeInfo.MoveSourceContainerId = deletedChangeInfo.Value.ContainerId;
                            movedChangeInfoes.Add(addedChangeInfo);
                        }
                    }

                    var movedIds = movedChangeInfoes.Select(item => item.AosId).ToHashSet();
                    addedChangeInfoes = addedChangeInfoes.Where(item => !movedIds.Contains(item.AosId)).ToList();
                    deletedChangeInfoes = deletedChangeInfoes.Where(item => !movedIds.Contains(item.AosId)).ToList();
                }

                MovedChangeInfoes = movedChangeInfoes;
                AddedChangeInfoes = addedChangeInfoes ?? new();
                DeletedChangeInfoes = deletedChangeInfoes ?? new();
                RenamedChangeInfoes = renamedChangeInfoes ?? new();

                s_logger.Debug($"Moved change logs count: [{MovedChangeInfoes.Count}].");
                s_logger.Debug($"Added change logs count: [{AddedChangeInfoes.Count}].");
                s_logger.Debug($"Deleted change logs count: [{DeletedChangeInfoes.Count}].");
                s_logger.Debug($"Renamed change logs count: [{RenamedChangeInfoes.Count}].");
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while analyze change logs. Error: {e}");
            }
        }

        public async Task Empty()
        {
            await s_cache.RemoveAsync(RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_CACHE_KEY + RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_SOURCE_CACHE_KEY[_contentSource]);
        }
    }
}
