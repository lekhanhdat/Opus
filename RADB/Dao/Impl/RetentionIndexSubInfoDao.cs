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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RetentionIndexSubInfoDao : BaseDao<RetentionIndexSubInfo>,IRetentionIndexSubInfoDao
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(RetentionIndexSubInfoDao));
        public void InsertIntoRetentionIndexSubInfo(RetentionIndexSubInfo subInfo)
        {
            try
            {
                logger.Info("Insert into retention Index Sub Info site collection: {0}, job Id: {1}.", subInfo.SiteURL, subInfo.JobId);
                using (var context = GetNewContext())
                {
                    var index = context.RetentionIndexSubInfos.Add(subInfo);
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                logger.Error($"something went wrong when Insert into retention sub Info,error {e}");
                throw;
            }
        }
        public void InsertIntoRetentionIndexSubInfo(List<RetentionIndexSubInfo> subInfos)
        {
            try
            {   
                using (var context = GetNewContext())
                {
                    foreach (var info in subInfos)
                    {
                        logger.Info("Insert into retention Index Sub Info site collection: {0}, job Id: {1}.", info.SiteURL, info.JobId);
                        context.RetentionIndexSubInfos.Add(info);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                logger.Error($"something went wrong when Insert into retention sub Info,error {e}");
                throw;
            }
        }
        public List<RetentionIndexSubInfo> GetRetentionSubInfoByTime(long startTime, long endTime)
        {
            try
            {
                List<RetentionIndexSubInfo> index = new List<RetentionIndexSubInfo>();
                logger.Info($"get retention Index Sub Info by time,start time:{startTime},end time:{endTime}.");
                using (var context = GetNewContext())
                {
                    index = context.RetentionIndexSubInfos.Where(r=>r.RetentionTime> startTime && r.RetentionTime< endTime).ToList();
                }
                return index;
            }
            catch (Exception e)
            {
                logger.Error($"something went wrong when get retention sub Info,error {e}");
                throw;
            }
        }

        public async Task<List<RetentionIndexSubInfo>> GetRetentionInfoesAsync(string siteUrl)
        {
            using var context = GetNewContext();
            return await context.RetentionIndexSubInfos.AsNoTracking().Where(item => item.SiteURL == siteUrl).ToListAsync();
        }

        public async Task<List<RetentionIndexSubInfo>> GetRetentionInfoesAsync()
        {
            using var context = GetNewContext();
            return await context.RetentionIndexSubInfos.AsNoTracking().ToListAsync();
        }

        public async Task<List<RetentionIndexSubInfo>> GetRetentionInfoesBySiteUrlsAsync(IEnumerable<string> siteUrls)
        {
            var result = new List<RetentionIndexSubInfo>();
            var urlList = siteUrls?.Where(url => !string.IsNullOrWhiteSpace(url)).Distinct().ToList() ?? new List<string>();
            if (urlList.Count == 0)
            {
                return result;
            }

            foreach (var batch in BatchStringList(urlList, 1000))
            {
                using var context = GetNewContext();
                var batchResult = await context.RetentionIndexSubInfos
                    .Where(item => batch.Contains(item.SiteURL))
                    .ToListAsync();
                result.AddRange(batchResult);
            }

            return result;
        }

        private static IEnumerable<List<string>> BatchStringList(List<string> items, int batchSize)
        {
            for (var i = 0; i < items.Count; i += batchSize)
            {
                yield return items.Skip(i).Take(batchSize).ToList();
            }
        }

        public async Task<int> CountRetentionInfoesAsync()
        {
            using var context = GetNewContext();
            return await context.RetentionIndexSubInfos.CountAsync();
        }

        public async Task DeleteAsync(string siteUniqueId, string siteUrl)
        {
            using var context = GetNewContext();
            var jobs = await context.RetentionIndexSubInfos.Where(item => item.SiteId == siteUniqueId && item.SiteURL == siteUrl).ToListAsync();
            context.RetentionIndexSubInfos.RemoveRange(jobs);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(RetentionIndexSubInfo info)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            context.Entry(info).State = EntityState.Deleted;
            //context.RetentionIndexSubInfos.Remove(info);
            await context.SaveChangesAsync();
        }
    }
}
