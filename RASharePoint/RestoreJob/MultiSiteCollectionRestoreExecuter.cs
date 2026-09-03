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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.SharePoint.ConvertStub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RestoreJob
{
    public class MultiSiteCollectionRestoreExecuter
    {
        private static readonly AveLogger s_logger = AveLogger.GetInstance(typeof(MultiSiteCollectionRestoreExecuter));
        private static readonly TimeSpan s_lockHeartbeatInterval = TimeSpan.FromHours(1);

        private IRestoreSearchService _restoreSearchService;
        private IRestoreSearchService RestoreSearchService => PlatformWindsorManager.GetService(ref _restoreSearchService);

        private IRMSubJobDao _rmSubJobDao;

        private IRMSubJobDao RMSubJobDao => PlatformWindsorManager.GetService(ref _rmSubJobDao);


        private SelectMultiScRestoreInfo _restoreInfo;

        public MultiSiteCollectionRestoreExecuter(string jobId, string param)
        {
            s_logger.Info($"Initializing MultiSiteCollectionRestoreExecuter for job {jobId}, param:{param}");
            string paramJson = RMSubJobDao.GetJobContextSettingByJobId(jobId);
            RMSubJobDao.DeleteJobContext(jobId);
            s_logger.Info($"Constructing MultiSiteCollectionRestoreExecuter for job {jobId}, paramJson:{paramJson}");
            _restoreInfo = SerializerHelper.DeserializeByJsonConvert<SelectMultiScRestoreInfo>(paramJson);
            _restoreInfo.RestoreOption.SerchContract = null;
        }

        public void Execute()
        {
            s_logger.Info("start Executing MultiSiteCollectionRestoreExecuter...");
            using CancellationTokenSource heartbeatCancellation = new CancellationTokenSource();
            Thread heartbeatThread = StartMultiSiteCollectionRestoreHeartbeatThread(heartbeatCancellation.Token);

            try
            {
                if (!_restoreInfo.IsSelectAll)
                {
                    s_logger.Info("Executing MultiSiteCollectionRestoreExecuter for selected site collections...");
                    RestoreSearchService.SaveMultiSiteCollectionRestoreSettingAndRunAsync(_restoreInfo.RestoreOption, true).GetAwaiter().GetResult();
                }
                else
                {
                    s_logger.Info("Executing MultiSiteCollectionRestoreExecuter for all site collections...");
                    foreach (var restoreOption in GetAllSiteCollectionRestoreInfo())
                    {
                        RestoreSearchService.SaveMultiSiteCollectionRestoreSettingAndRunAsync(restoreOption, true).GetAwaiter().GetResult();
                    }
                }
            }
            finally
            {
                heartbeatCancellation.Cancel();

                try
                {
                    RestoreSearchService.ReleaseMultiSiteCollectionRestoreRunLock();
                }
                catch (Exception ex)
                {
                    s_logger.Error($"Failed to release MultiSiteCollectionRestore ticket. Error:{ex}");
                }
            }
            s_logger.Info("end Executing MultiSiteCollectionRestoreExecuter...");
        }

        private Thread StartMultiSiteCollectionRestoreHeartbeatThread(CancellationToken cancellationToken)
        {
            Thread heartbeatThread = new Thread(() => RunMultiSiteCollectionRestoreHeartbeat(cancellationToken))
            {
                IsBackground = true,
                Name = nameof(MultiSiteCollectionRestoreExecuter),
            };

            heartbeatThread.Start();
            return heartbeatThread;
        }

        private void RunMultiSiteCollectionRestoreHeartbeat(CancellationToken cancellationToken)
        {
            while (!cancellationToken.WaitHandle.WaitOne(s_lockHeartbeatInterval))
            {
                if (!RestoreSearchService.UpdateMultiSiteCollectionRestoreRunLock())
                {
                    s_logger.Warn("Stopping MultiSiteCollectionRestore ticket heartbeat because update failed.");
                }
            }
        }

        public IEnumerable<RestoreInfo> GetAllSiteCollectionRestoreInfo()
        {
            ArchiverRestoreResult searchContract = new() { SerchContract = _restoreInfo.SearchContract };
            searchContract.PageIndex = 1;
            searchContract.PageSize = 100;
            ArchiverRestoreResult searchResult = null;
            do
            {
                s_logger.Info($"Getting all site collection restore info, PageIndex:{searchContract.PageIndex}, PageSize:{searchContract.PageSize}");
                searchResult = RestoreSearchService.GetAllSiteCollectionSerchResultAsync(searchContract).GetAwaiter().GetResult();
                if(searchResult?.RestoreSerchNodes?.Any() == true)
                {
                    _restoreInfo.RestoreOption.NodeObjects = searchResult.RestoreSerchNodes;
                    s_logger.Info($"Found {searchResult.RestoreSerchNodes.Count} site collections in this page. url:{string.Join(',', searchResult.RestoreSerchNodes.Select(node => node.SiteUrl))}");
                    yield return _restoreInfo.RestoreOption;
                }
                else
                {
                    s_logger.Info("No site collection found in this page.");
                }
                searchContract.PageIndex++;
            } while (searchResult?.HasNext == true);
            
        }
    }
}
