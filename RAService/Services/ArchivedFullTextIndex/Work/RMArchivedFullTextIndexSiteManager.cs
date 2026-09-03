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
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex.Impl;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex;
using AvePoint.RA.DB.Model.ArchivedFullTextIndex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb.JobMonitor;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work
{
    public class RMArchivedFullTextIndexSiteManager
    {
        private readonly IRMArchivedFullTextIndexDao _archivedFullTextIndexDao = new RMArchivedFullTextIndexDao();

        private readonly HashSet<bool> _siteJobSyncStatus = [];

        private readonly RMArchivedDataFullTextIndexSiteInfoesV1 _siteInfoV1;

        public long Id => _siteInfoV1.Id;

        public string SiteUrl => _siteInfoV1.SiteUrl;

        public JobStatus Status => _siteInfoV1.Status;

        public long LatestSyncTime => _siteInfoV1.LatestSyncTime;

        public RMArchivedFullTextIndexSiteManager(string siteUrl)
        {
            _siteInfoV1 = GetV1Async(siteUrl).GetAwaiter().GetResult();
        }

        public async Task RecordArchiverTimeAsync(long archiverTime)
        {
            if (archiverTime <= 0)
            {
                return;
            }

            if (_siteInfoV1.MinArchiverTime <= 0 || archiverTime < _siteInfoV1.MinArchiverTime)
            {
                _siteInfoV1.MinArchiverTime = archiverTime;
            }

            if (_siteInfoV1.MaxArchiverTime <= 0 || archiverTime > _siteInfoV1.MaxArchiverTime)
            {
                _siteInfoV1.MaxArchiverTime = archiverTime;
            }
        }

        public async Task IncreseLatestSyncTimeAsync(long latestSyncTime)
        {
            if (_siteInfoV1.LatestSyncTime >= latestSyncTime)
            {
                return;
            }

            _siteInfoV1.LatestSyncTime = latestSyncTime;
            await _archivedFullTextIndexDao.AddOrUpdateSiteInfoAsync(_siteInfoV1);
        }

        public void IncreseProgress(bool succeed)
        {
            _siteJobSyncStatus.Add(succeed);
        }

        public async Task FinishAsync()
        {
            _siteInfoV1.Status = GetStatus(_siteJobSyncStatus);
            await _archivedFullTextIndexDao.AddOrUpdateSiteInfoAsync(_siteInfoV1);
        }

        private static JobStatus GetStatus(HashSet<bool> statusSet)
        {
            if (statusSet.Count == 2)
            {
                return JobStatus.FinishWithException;
            }

            if (statusSet.Contains(false))
            {
                return JobStatus.Failed;
            }

            return JobStatus.Finished;
        }

        private async Task<RMArchivedDataFullTextIndexSiteInfoesV1> GetV1Async(string siteUrl)
        {
            var (has, siteInfo) = await _archivedFullTextIndexDao.TryGetSiteInfoV1Async(siteUrl);
            if (!has)
            {
                siteInfo = new RMArchivedDataFullTextIndexSiteInfoesV1
                {
                    SiteUniqueId = Guid.Empty.ToString(),
                    SiteUrl = siteUrl,
                    Status = Contract.RMWeb.JobMonitor.JobStatus.Finished,
                    LatestSyncTime = 0
                };
                await _archivedFullTextIndexDao.AddOrUpdateSiteInfoAsync(siteInfo);
            }

            return siteInfo;
        }
    }
}
