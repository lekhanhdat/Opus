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
using AvePoint.Archiver.Media;
using AvePoint.GCommon.Contract.Media.TCPRequest.Restore;
using AvePoint.Media.Service.ArchiverBackup.Restore;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.SharePoint.RestoreJob.Restore.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.Item.Restore
{
    public class AveItemPreviewRestoreMain : AbstractAveItemRestore
    {
        private static readonly TimeSpan PreviewRestoreResultCacheDuration = TimeSpan.FromMinutes(30);
        // Refreshes the cache entry well before it would expire, so a single long-running site collection
        // (one HandleSimulateRequest call can run far longer than the cache duration) doesn't let the entry
        // expire mid-run and look like the job never started.
        private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(30);

        private readonly List<RestoreSettingAndTree> mRestoreTreeAndSettings;
        private readonly string mMessageId;
        private readonly SimulateResotreResult mSimulateResult;
        // Tracked only by UpdateSimulateResult, which runs synchronously on the main restore loop thread, so no
        // locking is needed to keep the periodic cache refresh from racing with mSimulateResult mutations.
        private DateTime mLastCacheWriteTime;

        private IRMCacheManager RMCacheManager => PlatformWindsorManager.GetService<IRMCacheManager>();

        private IRestoreSearchService _restoreSearchService;
        private IRestoreSearchService RestoreSearchService => PlatformWindsorManager.GetService(ref _restoreSearchService);

        public AveItemPreviewRestoreMain(string jobId, JobType jobType, string extension)
        {
            JobId = jobId;
            this.mJobType = jobType;
            KeyValuePair<string, string> jobParameter = SerializerHelper.DeserializeByDataContractSerializer<KeyValuePair<string, string>>(extension);
            this.mMessageId = jobParameter.Key;
            this.mRestoreTreeAndSettings = SerializerHelper.DeserializeByDataContractSerializer<List<RestoreSettingAndTree>>(jobParameter.Value);
            var time = DateTime.UtcNow.ToString("u");
            this.mSimulateResult = new SimulateResotreResult()
            {
                IsCompleted = false,
                JobId = JobId,
                StartTime = time,
                UpdateTime = time,
                LevelCountMap = new Dictionary<int, long>()
                {
                {(int)PreviewRestoreLevel.SiteCollection, 0 },
                {(int)PreviewRestoreLevel.Site, 0 },
                {(int)PreviewRestoreLevel.List, 0 },
                {(int)PreviewRestoreLevel.Folder, 0 },
                {(int)PreviewRestoreLevel.Item, 0 },
                {(int)PreviewRestoreLevel.ItemVersion, 0 },
                {(int)PreviewRestoreLevel.Document, 0 },
                {(int)PreviewRestoreLevel.DocumentVersion, 0 },
                {(int)PreviewRestoreLevel.Attachment, 0 },
                {(int)PreviewRestoreLevel.Unknown, 0 }
                }
            };
        }

        public override async Task RunNowAsync()
        {
            string errorMessage = string.Empty;
            try
            {
                // Let GetPreviewRestoreResult know the job has actually started running, instead of the caller
                // seeing no cached value at all (which is indistinguishable from "job not picked up yet") until
                // the whole job finishes.
                await RMCacheManager.Cache.SetAsync(IRMCache.Keys.PreviewRestoreResult + mMessageId, mSimulateResult, PreviewRestoreResultCacheDuration);
                mLastCacheWriteTime = DateTime.UtcNow;


                if (mRestoreTreeAndSettings == null || mRestoreTreeAndSettings.Count == 0)
                {
                    mLog.Warn($"No restore tree found for preview restore data size job, jobId:{JobId}, messageId:{mMessageId}.");
                    return;
                }

                foreach (RestoreSettingAndTree mRestore in mRestoreTreeAndSettings)
                {
                    if (mRestore?.Setting == null)
                    {
                        mLog.Warn($"Skip empty restore setting for preview restore data size job, jobId:{JobId}, messageId:{mMessageId}.");
                        continue;
                    }

                    RestoreSettingAndTree resolvedRestore = mRestore;
                    if (mRestore.Tree == null || mRestore.Tree.Count == 0)
                    {
                        // The tree wasn't resolved before queuing (multi-site-collection preview defers the
                        // index search/tree-build to this worker instead of running it on the web tier).
                        resolvedRestore = await RestoreSearchService.ResolvePendingPreviewRestoreTreeAsync(mRestore.Setting);
                        if (resolvedRestore?.Tree == null || resolvedRestore.Tree.Count == 0)
                        {
                            mLog.Warn($"No archived items matched the provided criteria while resolving preview restore tree, jobId:{JobId}, messageId:{mMessageId}.");
                            continue;
                        }
                    }

                    string siteUrl = resolvedRestore.Tree.FirstOrDefault()?.SitePath;
                    try
                    {
                        ArchiverRestoreRequest configForMedia = AssembleRestoreMessage(JobId, resolvedRestore.Tree[0], resolvedRestore, true);
                        // Each site collection needs its own restore service instance so that the downloaded site
                        // index is released (disposed) before moving on to the next site collection in this job.
                        long sizeBeforeSite = mSimulateResult.Size;
                        using (IArchiverRestoreService restoreService = MediaServiceFactory.CreateArchiverRestoreService())
                        {
                            restoreService.HandlePreviewRequest(configForMedia, CancellationToken.None, UpdateSimulateResult);
                            mLog.Info($"Finished preview restore data size for site, jobId:{JobId}, messageId:{mMessageId}, siteUrl:{siteUrl}, size:{mSimulateResult.Size - sizeBeforeSite}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error($"Fail to preview restore data size for site, jobId:{JobId}, messageId:{mMessageId}, siteUrl:{siteUrl}, error:{ex}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                mLog.Error($"Preview restore data size failed, error:{errorMessage}");
            }
            finally
            {
                mSimulateResult.ErrorMessage = errorMessage;
                mSimulateResult.IsCompleted = true;
                var time = DateTime.UtcNow.ToString("u");
                mSimulateResult.FinishTime = time;
                mSimulateResult.UpdateTime = time;
                // The preview restore data size job has no RMSubJob database record, so the result is handed to the
                // caller through Redis cache instead of job report/DB, keyed by the originating job queue message id.
                // Written in finally so a partial/aggregated result is still cached even if a site collection fails
                // and the exception propagates, instead of leaving callers polling with no result forever.
                await RMCacheManager.Cache.SetAsync(IRMCache.Keys.PreviewRestoreResult + mMessageId, mSimulateResult, PreviewRestoreResultCacheDuration);
                mLog.Info($"Preview restore data size job completed, jobId:{JobId}, messageId:{mMessageId}, siteCount:{mRestoreTreeAndSettings?.Count ?? 0}, totalSize:{mSimulateResult.Size}, levelCountMap:[{string.Join(", ", mSimulateResult.LevelCountMap.Select(kv => $"{kv.Key}:{kv.Value}"))}].");
            }
        }

        // Passed to IArchiverRestoreService.HandlePreviewRequest as the progress callback. Called synchronously
        // for every processed item (even mid-site-collection), on the same thread as the main restore loop. Every
        // HeartbeatInterval, it also refreshes the Redis cache entry so a single long-running site collection
        // doesn't let the entry expire mid-run, and callers polling mid-run see live progress.
        private void UpdateSimulateResult(int level, long contentLength)
        {
            mSimulateResult.Size += contentLength;
            long nodeCount = mSimulateResult.LevelCountMap.GetValueOrDefault(level, 0);
            mSimulateResult.LevelCountMap[level] = ++nodeCount;

            if (DateTime.UtcNow - mLastCacheWriteTime < HeartbeatInterval)
            {
                return;
            }

            mLastCacheWriteTime = DateTime.UtcNow;
            mSimulateResult.UpdateTime = mLastCacheWriteTime.ToString("u");
            // Blocking wait is acceptable here: this callback is synchronous (Action<int, long>) and only blocks
            // for a periodic cache write, not per processed item.
            RMCacheManager.Cache.SetAsync(IRMCache.Keys.PreviewRestoreResult + mMessageId, mSimulateResult, PreviewRestoreResultCacheDuration).GetAwaiter().GetResult();
            mLog.Info($"Preview restore data size job heartbeat, jobId:{JobId}, messageId:{mMessageId}, totalSize:{mSimulateResult.Size}, levelCountMap:[{string.Join(", ", mSimulateResult.LevelCountMap.Select(kv => $"{kv.Key}:{kv.Value}"))}].");
        }
    }
}
