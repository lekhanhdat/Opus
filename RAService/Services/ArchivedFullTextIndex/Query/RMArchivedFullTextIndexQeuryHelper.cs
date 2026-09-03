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
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex;
using AvePoint.RA.DB.Dao.ArchivedFullTextIndex.Impl;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Query
{
    internal static class RMArchivedFullTextIndexQueryHelper
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(RMArchivedFullTextIndexQueryHelper));
        private static readonly IRMArchivedFullTextIndexDao _archivedFullTextIndexDao = new RMArchivedFullTextIndexDao();

        internal static async Task<(long StartTicks, long EndTicks)> GetClampedArchivedTimeRangeAsync(
            IList<string> siteUrls,
            long startTicks,
            long endTicks)
        {
            var safeSiteUrls = siteUrls ?? new List<string>();
            _logger.Info($"Get site info from db, site count: {safeSiteUrls.Count}, site urls: {string.Join(",", safeSiteUrls)}.");

            var (minArchiverTime, maxArchiverTime) = await _archivedFullTextIndexDao.GetMinMaxArchiverTimeBySiteUrlsAsync(safeSiteUrls);
            _logger.Info($"The minArchiverTime is [{minArchiverTime}] and maxArchiverTime is [{maxArchiverTime}].");

            if (startTicks >= maxArchiverTime)
            {
                return (maxArchiverTime, maxArchiverTime);
            }

            if (startTicks < minArchiverTime)
            {
                startTicks = minArchiverTime;
            }

            if (endTicks > maxArchiverTime)
            {
                endTicks = maxArchiverTime;
            }

            if (endTicks < minArchiverTime)
            {
                return (minArchiverTime, minArchiverTime);
            }

            if (endTicks < startTicks)
            {
                endTicks = startTicks;
            }

            return (startTicks, endTicks);
        }

        internal static async Task<(long StartTicks, long EndTicks)> GetClampedArchivedTimeRangeV1Async(
            IList<string> siteUrls,
            long startTicks,
            long endTicks,
            bool isBlacklistMode = false)
        {
            static (long StartTicks, long EndTicks) NormalizeEqualRange(long start, long end)
            {
                if (start == end)
                {
                    if (start > long.MinValue)
                    {
                        start--;
                    }
                    if (end < long.MaxValue)
                    {
                        end++;
                    }
                }

                return (start, end);
            }

            var safeSiteUrls = siteUrls ?? new List<string>();
            _logger.Info($"Get site info from db, site count: {safeSiteUrls.Count}, site urls: {string.Join(",", safeSiteUrls)}.");

            var (minArchiverTime, maxArchiverTime) = await _archivedFullTextIndexDao.GetMinMaxArchiverTimeBySiteUrlsV1Async(safeSiteUrls, isBlacklistMode);
            _logger.Info($"The minArchiverTime is [{minArchiverTime}] and maxArchiverTime is [{maxArchiverTime}].");

            if (startTicks == 0 && endTicks == 0)
            {
                return NormalizeEqualRange(minArchiverTime, maxArchiverTime);
            }
            
            if (minArchiverTime == 0 && maxArchiverTime == 0)
            {
                return NormalizeEqualRange(startTicks, endTicks);
            }

            if ((startTicks < minArchiverTime && endTicks < minArchiverTime)
                || (startTicks > maxArchiverTime && endTicks > maxArchiverTime))
            {
                return (startTicks, endTicks);
            }

            if (startTicks >= maxArchiverTime)
            {
                return NormalizeEqualRange(maxArchiverTime, maxArchiverTime);
            }

            if (startTicks < minArchiverTime)
            {
                startTicks = minArchiverTime;
            }

            if (endTicks > maxArchiverTime)
            {
                endTicks = maxArchiverTime;
            }

            if (endTicks < minArchiverTime)
            {
                return NormalizeEqualRange(minArchiverTime, minArchiverTime);
            }

            if (endTicks < startTicks)
            {
                endTicks = startTicks;
            }

            return NormalizeEqualRange(startTicks, endTicks);
        }
    }
}
