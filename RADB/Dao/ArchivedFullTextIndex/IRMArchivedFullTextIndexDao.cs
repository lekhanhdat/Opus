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
using AvePoint.RA.Contract.ArchivedFullTextIndex;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Model.ArchivedFullTextIndex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.ArchivedFullTextIndex
{
    public interface IRMArchivedFullTextIndexDao
    {
        Task AddOrUpdateLatestSyncTimeAsync(long latestSyncTime);

        Task<(bool Has, RMArchivedDataFullTextIndexSiteInfoesV1 SiteInfo)> TryGetSiteInfoV1Async(string siteUrl);

        Task<List<RMArchivedDataFullTextIndexSiteInfoesV1>> GetSiteInfoesBySiteUrlsV1Async(IEnumerable<string> siteUrls);

        Task<(long MinArchiverTime, long MaxArchiverTime)> GetMinMaxArchiverTimeBySiteUrlsAsync(IEnumerable<string> siteUrls);

        Task<(long MinArchiverTime, long MaxArchiverTime)> GetMinMaxArchiverTimeBySiteUrlsV1Async(IEnumerable<string> siteUrls, bool isBlacklistMode = false);

        Task<List<RMArchivedDataFullTextIndexJobInfoesV1>> GetSiteJobInfoesV1(long siteId, params JobStatus[] status);

        Task<List<RMArchivedDataFullTextIndexJobInfoesV1>> GetJobInfoesBySiteUrlsV1Async(IEnumerable<string> siteUrls, params JobStatus[] status);

        Task<List<RMArchivedDataFullTextIndexJobInfoesV1>> GetJobInfoesBySiteIdsV1Async(IEnumerable<long> siteIds, params JobStatus[] status);

        Task AddOrUpdateSiteInfoAsync(RMArchivedDataFullTextIndexSiteInfoesV1 siteInfo);

        Task AddOrUpdateJobInfoAsync(params RMArchivedDataFullTextIndexJobInfoesV1[] jobInfoes);

        Task<(bool Has, RMArchivedDataFullTextIndexJobInfoesV1 JobInfo)> TryGetJobInfoV1Async(string archiverSubJobId);

        Task<RMArchivedDataFullTextIndexJobInfoesV1> GetJobInfoByIdV1Async(long id);

        Task AddOrUpdateEDiscoveryJobInfoAsync(params RMArchivedDataFullTextIndexEDiscoveryJobInfoesV1[] jobInfoes);

        Task<long> GetSiteLatestArchivedTimeAsync(string siteUrl);

        Task<long> GetSiteLatestArchivedTimeV1Async(string siteUrl);

        Task<long> GetLatestArchivedTimeAsync();

        Task<long> GetLatestArchivedTimeV1Async();
    }
}
