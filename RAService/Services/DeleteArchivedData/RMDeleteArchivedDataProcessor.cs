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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Telemetry;
using AvePoint.Wrapper.Restore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DeleteArchivedData
{
    public class RMDeleteArchivedDataProcessor
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDeleteArchivedDataProcessor));

        private readonly IRestoredSitesInfoDao _restoredSitesInfoDao = PlatformWindsorManager.GetService<IRestoredSitesInfoDao>();
        public IJobMonitorDao _jmDao => PlatformWindsorManager.GetService<IJobMonitorDao>();

        private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

        private readonly RMDeleteArchivedDataJobManager _jobManager;

        public RMDeleteArchivedDataProcessor(string jobId)
        {
            _jobManager = new RMDeleteArchivedDataJobManager(jobId);
        }

        public async Task RunAsync()
        {
            try
            {
                var restoredSites = GetRestoredSites();

                _logger.Info($"The number of sites that need to be processed is [{restoredSites.Count}].");
                _jobManager.Init(restoredSites.Count);

                await RMDeleteArchivedDataCosmosDBManager.InitAsync();

                var telemetryDataManager = new RMDeleteArchivedDataTelemetryDataManager();

                foreach (var restoredSite in restoredSites)
                {
                    var siteProcessor = new RMDeleteArchivedDataSiteProcessor(restoredSite, _jobManager, telemetryDataManager);
                    var needRemoveRestoredSiteInfo = await siteProcessor.ProcessAsync();

                    if(needRemoveRestoredSiteInfo)
                    {
                        _logger.Info($"The site [{restoredSite.SiteUrl}] can be remove in restored site info table.");
                        _restoredSitesInfoDao.Remove(restoredSite);
                    }

                    _jobManager.IncreaseProgress();
                }

                await telemetryDataManager.SyncAsync();
                _jobManager.Finish();
                await TelemetryContext.FlushAsync();
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while run job. Error: {e}");
                _jobManager.Fail();
            }
        }

        private List<RestoredSitesInfo> GetRestoredSites()
        {
            var restoredSites = _restoredSitesInfoDao.GetAll().GroupBy(item => item.SiteUrl).ToDictionary(item => item.Key, item => item.ToList().First()).Values.ToList();
            var job = _jmDao.GetJobById(_jobManager.JobId);
            var jobExtension = SerializerHelper.DeserializeByDataContractSerializer<ArchiveJobMonitorExtension>(job.JobConflictExtension);
            _logger.Info($"The sites that are executing the job are [{string.Join("; ", jobExtension.SiteUrls)}]");
            restoredSites = restoredSites.Where(restoredSite => jobExtension.SiteUrls.Contains(restoredSite.SiteUrl)).ToList();
            return restoredSites;
        }
    }
}
