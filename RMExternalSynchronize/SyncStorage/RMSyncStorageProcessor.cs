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
using AvePoint.GCommon.Utility.Storage;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Records.Core.Utilities.Extensions;
using Azure.Storage.Blobs.Models;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using RAExportCommon;
using RATeams;
using RMSynchronize.SyncNodeFromAOS.ChangeLog;
using RMSynchronize.SyncNodeFromAOS.CheckLicense.ContentSourceInterface;
using RMSynchronize.SyncStorage.CosmosDB;
using RMSynchronize.SyncStorage.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace RMSynchronize.SyncStorage
{
    public class RMSyncStorageProcessor
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMSyncStorageProcessor));

        private static readonly IRMKeyValueDao s_keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static readonly IRMRemoteNodeDao s_remoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        private const string S_CUSTOM_SYNC_NODE_CHANGE_INFOES = "CUSTOM_SYNC_NODE_CHANGE_INFOES";

        private static readonly List<SourceFlag> s_needProcessedContentSources = new()
        {
            SourceFlag.SharePoint,
            SourceFlag.OneDrive,
            SourceFlag.Exchange,
            SourceFlag.Google,
            SourceFlag.Teams
        };

        private readonly RMSyncStorageJobManager _jobManager;

        private readonly string _jobId;

        private Dictionary<Guid, string> containerNameCache = new Dictionary<Guid, string>();

        private HashSet<BlobItem> _unProcessedBlobs = new HashSet<BlobItem>();

        private HashSet<string> _processedJobIds = new HashSet<string>();

        public RMSyncStorageProcessor(string jobId, string syncNodeJobId)
        {
            s_logger.Info($"Current sync permission job trigger by syncNodeJob:{syncNodeJobId}");
            _jobId = jobId;
            _jobManager = new(jobId);
        }


        public async Task RunAsync()
        {
            try
            {
                while (TryGetUnProcessedSyncNodeId(out var syncNodeJobId))
                {
                    _processedJobIds.Add(syncNodeJobId);
                    await RealSyncNodePermission(syncNodeJobId);
                }

                _jobManager.SetJobFinished();
                if (_jobManager.HasFailed)
                {
                    TelemetryContext.SendToQueue(TelemetryModule.PermissionSync, TelemetryEventType.PermissionSyncFailedInfo, [_jobId]);
                }
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while process. Error: {e}");
                _jobManager.SetJobFailed(e.Message);
            }
        }

        private bool TryGetUnProcessedSyncNodeId(out string syncNodeJobId)
        {
            if (!_unProcessedBlobs.Any())
            {
                var allBlobs = RAStorageUtil.GetAllChangeLogReportBlobs(RMSyncNodeAzureChangeLogWorker.GetBlobFolderName());
                s_logger.Info($"Total sync node job blobs count:{allBlobs.Count()}");
                _unProcessedBlobs = allBlobs.Where(b => !_processedJobIds.Contains(Path.GetFileNameWithoutExtension(b.Name))).ToHashSet();
                s_logger.Info($"UnProcessed sync node job blobs count:{_unProcessedBlobs.Count()}");
                if (!_unProcessedBlobs.Any())
                {
                    syncNodeJobId = string.Empty;
                    return false;
                }
            }
            BlobItem item = _unProcessedBlobs.OrderBy(b => b.Properties.CreatedOn).First();
            syncNodeJobId = Path.GetFileNameWithoutExtension(item.Name);
            _unProcessedBlobs.Remove(item);
            return true;
        }

        private async Task RealSyncNodePermission(string syncNodeJobId)
        {
            s_logger.Info($"Current sync storage job trigger by syncNodeJob:{syncNodeJobId}");
            RMSyncNodeAzureChangeLogWorker azureBlobWorker = null;
            try
            {
                azureBlobWorker = new(syncNodeJobId, true);
                azureBlobWorker.DownloadReports();
                await RunFromConfigAsync();
                await RunFromRedisAsync();
                await RunFromAzureAsync(azureBlobWorker);
                s_logger.Info($"Finish sync storage job trigger by syncNodeJob:{syncNodeJobId}");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while process sync node job:{syncNodeJobId}. Error: {e}");
            }
            finally
            {
                azureBlobWorker?.DeleteStorageFile();
            }
        }

        private async Task Process(SourceFlag contentSource, List<RMSyncNodeChangeInfo> changeInfoes, params Func<RMSyncNodeChangeInfo, Task<bool>>[] actions)
        {
            s_logger.Info($"Start to process content source:{contentSource}, change info count:{changeInfoes.Count}.");
            foreach (var changeInfo in changeInfoes)
            {
                try
                {
                    changeInfo.ContentSource = contentSource;
                    if(!changeInfo.IsContainer && contentSource != SourceFlag.Teams)
                    {
                        if (contentSource != SourceFlag.Exchange && contentSource != SourceFlag.Google)
                        {
                            changeInfo.RealId = new Guid(changeInfo.AosId);
                        }

                        if (changeInfo.NodeLevel == AvePoint.GCommon.Contract.Tree.Object.NodeLevel.O365GroupSites)
                        {
                            var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(new AvePoint.GCommon.Contract.Server.ControlPanel.Office365.RemoteSiteCollection
                            {
                                url = changeInfo.Url,
                                TenantId = changeInfo.O365TenantId
                            });

                            var factory = MultiAppUtil.CreateAveObjectModelFactory(changeInfo.Url, bposInfo, AvePoint.Wrapper.Common.AveContextKind.ClientObjectModel);

                            var site = factory.CreateSite(changeInfo.Url);

                            changeInfo.RealId = site.ID;
                        }

                        if (contentSource == SourceFlag.Google)
                        {
                            changeInfo.RealId = new Guid(changeInfo.Id);
                        }
                    }

                    var res = true;
                    foreach (var action in actions)
                    {
                        res = res && await action(changeInfo);
                    }

                    _jobManager.AddDetail(changeInfo, res);
                }
                catch(Exception e)
                {
                    _jobManager.HasFailed = true;
                    _jobManager.AddFailedDetail(changeInfo, e.Message);
                    s_logger.Error($"An error occurred while process [{contentSource}] [{SerializerHelper.SerializeByDataContractSerializer(changeInfo)}]. Error: {e}");
                }
            }
            s_logger.Info($"Finish to process content source:{contentSource}.");
        }

        private async Task RunFromRedisAsync()
        {
            try
            {
                s_logger.Info($"Start to run from redis, count:{s_needProcessedContentSources.Count}.");
                foreach (var contentSource in s_needProcessedContentSources)
                {
                    var analyzer = new RMSyncNodeChangeLogAnalyzer(contentSource);
                    await analyzer.Analyze();

                    var cosmosDBProcessor = new RMSyncCosmosDBProcessor();
                    if(await cosmosDBProcessor.PrepareAsync())
                    {
                        await Process(contentSource, analyzer.AddedChangeInfoes, cosmosDBProcessor.AddAsync);
                        await Process(contentSource, analyzer.DeletedChangeInfoes, cosmosDBProcessor.DeleteAsync);
                        await Process(contentSource, analyzer.MovedChangeInfoes, cosmosDBProcessor.MoveContainerAsync);
                        await cosmosDBProcessor.WaitFinishAsync();
                    }

                    await analyzer.Empty();
                }
                s_logger.Info("Finish run from redis.");
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while run from redis. Error: {e}");
            }
        }

        private async Task RunFromAzureAsync(RMSyncNodeAzureChangeLogWorker azureBlobWorker)
        {
            try
            {
                s_logger.Info($"Start to run from azure, count:{s_needProcessedContentSources.Count}.");
                foreach (var contentSource in s_needProcessedContentSources)
                {
                    var analyzer = new RMSyncNodeAzureChangeLogAnalyzer(contentSource, azureBlobWorker);
                    await analyzer.Analyze();
                    s_logger.Info($"Content source:{contentSource}, added count:{analyzer.AddedChangeInfoes.Count}, deleted count:{analyzer.DeletedChangeInfoes.Count}, moved count:{analyzer.MovedChangeInfoes.Count}.");
                    var cosmosDBProcessor = new RMSyncCosmosDBProcessor();
                    if (await cosmosDBProcessor.PrepareAsync())
                    {
                        await Process(contentSource, analyzer.AddedChangeInfoes, cosmosDBProcessor.AddAsync);
                        await Process(contentSource, analyzer.DeletedChangeInfoes, cosmosDBProcessor.DeleteAsync);
                        await Process(contentSource, analyzer.MovedChangeInfoes, cosmosDBProcessor.MoveContainerAsync);
                        await cosmosDBProcessor.WaitFinishAsync();
                    }
                }
                s_logger.Info("Finish run from azure.");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while run from redis. Error: {e}");
            }
        }

        private async Task RunFromConfigAsync()
        {
            s_logger.Info("Start to run from config.");
            try
            {
                var setting = s_keyValueDao.GetValueByKey(S_CUSTOM_SYNC_NODE_CHANGE_INFOES);
                if (setting == null || string.IsNullOrWhiteSpace(setting.Value))
                {
                    return;
                }

                var cosmosDBProcessor = new RMSyncCosmosDBProcessor();

                if(await cosmosDBProcessor.PrepareAsync())
                {
                    var changeInfoes = JsonConvert.DeserializeObject<List<RMSyncNodeChangeInfo>>(setting.Value);
                    s_logger.Info($"Total change info count from config:{changeInfoes.Count}.");
                    foreach (var changeInfo in changeInfoes)
                    {
                        switch (changeInfo.ChangeType)
                        {
                            case RMSyncNodeChangeType.Add:
                                await Process(changeInfo.ContentSource, new List<RMSyncNodeChangeInfo> { changeInfo }, cosmosDBProcessor.AddAsync);
                                break;
                            case RMSyncNodeChangeType.Delete:
                                await Process(changeInfo.ContentSource, new List<RMSyncNodeChangeInfo> { changeInfo }, cosmosDBProcessor.DeleteAsync);
                                break;
                            case RMSyncNodeChangeType.MoveContainer:
                                await Process(changeInfo.ContentSource, new List<RMSyncNodeChangeInfo> { changeInfo }, cosmosDBProcessor.MoveContainerAsync);
                                break;
                        }
                    }

                    await cosmosDBProcessor.WaitFinishAsync();
                }

                s_keyValueDao.DeleteByKey(S_CUSTOM_SYNC_NODE_CHANGE_INFOES);
                s_logger.Info("Finish run from config.");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while run from config. Error: {e}");
            }
        }
    }
}
