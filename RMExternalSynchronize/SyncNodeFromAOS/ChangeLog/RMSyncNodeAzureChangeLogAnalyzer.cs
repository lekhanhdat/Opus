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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.AzureTable;
using AvePoint.RA.DB.AzureTable.Model;
using AvePoint.RA.DB.Dao.Impl;
using RAExportCommon;
using System.Text;

namespace RMSynchronize.SyncNodeFromAOS.ChangeLog
{
    public class RMSyncNodeAzureChangeLogAnalyzer
    {

        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMSyncNodeAzureChangeLogAnalyzer));

        private readonly SourceFlag _contentSource;

        private readonly RMSyncNodeAzureChangeLogWorker _syncNodeAzureBlobWorker;

        public List<RMSyncNodeChangeInfo> MovedChangeInfoes { get; private set; } = new();

        public List<RMSyncNodeChangeInfo> AddedChangeInfoes { get; private set; } = new();

        public List<RMSyncNodeChangeInfo> DeletedChangeInfoes { get; private set; } = new();

        public List<RMSyncNodeChangeInfo> RenamedChangeInfoes { get; private set; } = new();

        public RMSyncNodeAzureChangeLogAnalyzer(SourceFlag contentSource, RMSyncNodeAzureChangeLogWorker syncNodeAzureBlobWorker)
        {
            _contentSource = contentSource;
            _syncNodeAzureBlobWorker = syncNodeAzureBlobWorker;
        }

        public Task Analyze()
        {
            try
            {
                var currentMovedChangeInfoes = new List<RMSyncNodeChangeInfo>();
                var currentAddedChangeInfoes = new List<RMSyncNodeChangeInfo>();
                var currentDeletedChangeInfoes = new List<RMSyncNodeChangeInfo>();
                var currentRenamedChangeInfoes = new List<RMSyncNodeChangeInfo>();
                var condition = $"ContentSource == {(int)_contentSource}";
                var startPage = 0;
                var totalCount = _syncNodeAzureBlobWorker.GetCount(condition);
                s_logger.Info($"Start to analyze change logs for content source:[{_contentSource}]. Total change logs count: [{totalCount}].");
                var currentCount = 0;

                var pendingAdds = new Dictionary<Guid, RMSyncNodeChangeInfo>();
                var pendingDeletes = new Dictionary<Guid, RMSyncNodeChangeInfo>();

                while (true)
                {
                    startPage++;
                    var changeInfoes = _syncNodeAzureBlobWorker.GetData(startPage, condition);
                    if (changeInfoes == null || changeInfoes.Count == 0) break;

                    currentCount += changeInfoes.Count;

                    foreach (var ci in changeInfoes)
                    {
                        switch (ci.ChangeType)
                        {
                            case RMSyncNodeChangeType.ChangeName:
                                currentRenamedChangeInfoes.Add(ci);
                                break;

                            case RMSyncNodeChangeType.Add:
                                {
                                    if (pendingDeletes.TryGetValue(new Guid(ci.AosId), out var del))
                                    {
                                        ci.MoveSourceContainerId = del.ContainerId;
                                        currentMovedChangeInfoes.Add(ci);
                                        pendingDeletes.Remove(new Guid(ci.AosId));
                                    }
                                    else
                                    {
                                        pendingAdds[new Guid(ci.AosId)] = ci;
                                    }
                                    break;
                                }

                            case RMSyncNodeChangeType.Delete:
                                {
                                    if (ci.IsContainer) break;

                                    if (pendingAdds.TryGetValue(new Guid(ci.AosId), out var add))
                                    {
                                        add.MoveSourceContainerId = ci.ContainerId;
                                        currentMovedChangeInfoes.Add(add);
                                        pendingAdds.Remove(new Guid(ci.AosId));
                                    }
                                    else
                                    {
                                        pendingDeletes[new Guid(ci.AosId)] = ci;
                                    }
                                    break;
                                }

                            default:
                                break;
                        }
                    }

                    if (currentCount >= totalCount) break;
                }

                currentAddedChangeInfoes.AddRange(pendingAdds.Values);
                currentDeletedChangeInfoes.AddRange(pendingDeletes.Values);

                MovedChangeInfoes = currentMovedChangeInfoes;
                AddedChangeInfoes = currentAddedChangeInfoes;
                DeletedChangeInfoes = currentDeletedChangeInfoes;
                RenamedChangeInfoes = currentRenamedChangeInfoes;

                s_logger.Debug($"Moved change logs count: [{MovedChangeInfoes.Count}].info detail:{SerializerHelper.SerializeByJsonConvert(MovedChangeInfoes)}");
                s_logger.Debug($"Added change logs count: [{AddedChangeInfoes.Count}].info detail:{SerializerHelper.SerializeByJsonConvert(AddedChangeInfoes)}");
                s_logger.Debug($"Deleted change logs count: [{DeletedChangeInfoes.Count}].info detail:{SerializerHelper.SerializeByJsonConvert(DeletedChangeInfoes)}");
                s_logger.Debug($"Renamed change logs count: [{RenamedChangeInfoes.Count}].info detail:{SerializerHelper.SerializeByJsonConvert(RenamedChangeInfoes)}");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while analyze change logs. Error: {e}");
            }

            return Task.CompletedTask;
        }
    }
}
