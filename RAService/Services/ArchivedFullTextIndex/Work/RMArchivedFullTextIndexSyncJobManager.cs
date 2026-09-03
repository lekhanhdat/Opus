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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model.ArchivedFullTextIndex;
using Castle.Components.DictionaryAdapter;
using RAExportCommon;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work
{
    public class RMArchivedFullTextIndexSyncJobManager
    {
        private readonly IRMArchivedFullTextIndexDao _archivedFullTextIndexDao = new RMArchivedFullTextIndexDao();

        private readonly HashSet<bool> _jobSyncStatus = [];

        private readonly RMArchivedFullTextIndexSiteManager _siteManager;

        private readonly RMArchivedFullTextIndexJobManager _jobManager;

        private readonly string _jobId;

        private readonly long _archiverTime;

        private RMArchivedDataFullTextIndexJobInfoesV1 _jobInfoV1;

        private readonly string _siteId;

        public long Id => _jobInfoV1.Id;

        public string ArchiverJobId => _jobInfoV1.ArchiverJobId;

        public Contract.RMWeb.JobMonitor.JobStatus Status => _jobInfoV1.Status;

        public RMArchivedFullTextIndexSyncJobManager(
            RMArchivedFullTextIndexSiteManager siteManager,
            RMArchivedFullTextIndexJobManager jobManager,
            string jobId,
            string siteId,
            long archiverTime)
        {
            _siteManager = siteManager;
            _jobManager = jobManager;
            _jobId = jobId;
            _siteId = siteId;
            _archiverTime = archiverTime;
        }

        public async Task InitAsync(bool isVirtual)
        {
            if(isVirtual)
            {
                _jobInfoV1 = new()
                {
                    SiteId = _siteId,
                    SiteUrl = _siteManager.SiteUrl,
                    ArchiverJobId = _jobId,
                };
                return;
            }

            var (has, jobInfo) = await _archivedFullTextIndexDao.TryGetJobInfoV1Async(_jobId);
            if(!has)
            {
                jobInfo = new RMArchivedDataFullTextIndexJobInfoesV1
                {
                    SiteId = _siteId,
                    SiteUrl = _siteManager.SiteUrl,
                    FullTextIndexSiteId = _siteManager.Id,
                    ArchiverJobId = _jobId,
                    ArchiverTime = _archiverTime,
                    Status = Contract.RMWeb.JobMonitor.JobStatus.None,
                };
            }

            jobInfo.FullTextIndexSyncJobId = _jobManager.JobId;
            jobInfo.StartTime = DateTime.UtcNow.Ticks;
            _jobInfoV1 = jobInfo;

            await _archivedFullTextIndexDao.AddOrUpdateJobInfoAsync(_jobInfoV1);
        }

        public void IncreseProgress(bool succeed)
        {
            _jobSyncStatus.Add(succeed);
        }

        public async Task SetToFinishedAsync()
        {
            _jobInfoV1 = await _archivedFullTextIndexDao.GetJobInfoByIdV1Async(_jobInfoV1.Id);
            _jobInfoV1.EndTime = DateTime.UtcNow.Ticks;
            _jobInfoV1.Status = GetStatus(_jobSyncStatus);
            await _archivedFullTextIndexDao.AddOrUpdateJobInfoAsync(_jobInfoV1);
        }

        private static Contract.RMWeb.JobMonitor.JobStatus GetStatus(HashSet<bool> statusSet)
        {
            if (statusSet.Count == 2)
            {
                return Contract.RMWeb.JobMonitor.JobStatus.FinishWithException;
            }

            if (statusSet.Contains(false))
            {
                return Contract.RMWeb.JobMonitor.JobStatus.Failed;
            }

            return Contract.RMWeb.JobMonitor.JobStatus.Finished;
        }
    }
}
