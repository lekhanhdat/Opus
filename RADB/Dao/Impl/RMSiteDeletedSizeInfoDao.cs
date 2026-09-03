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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.RMWeb.SiteDeletedSizeInfo;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model;
using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMSiteDeletedSizeInfoDao : BaseDao<RMSiteDeletedSizeInfo>, IRMSiteDeletedSizeInfoDao
    {
        public async Task CreateInfo(RMSiteDeletedSizeInfo info)
        {
            using var context = GetNewContext();
            context.RMSiteDeletedSizeInfo.Add(info);
            await context.SaveChangesAsync();
        }

        public async Task DeleteInfoBySiteUrl(string siteUrl)
        {
            string sql = "delete from {0}.RMSiteDeletedSizeInfoes where SiteUrl = @siteUrl";
            using (RMDbContext context = GetNewContext())
            {
                await context.Database.ExecuteSqlCommandAsync(string.Format(sql, SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)), new SqlParameter("siteUrl", siteUrl));
            }
        }

        public async Task<List<RMSiteDeletedSizeInfo>> GetSiteDeleteSizeInfoBySiteUrlAsync(string siteUrl)
        {
            using var context = GetNewContext();
            return await context.RMSiteDeletedSizeInfo.Where(a=>a.SiteUrl == siteUrl)?.ToListAsync();
        }
        public Dictionary<string, Tuple<string, long>> GetSiteDeleteSizeInfoWithSiteId()
        {
            return GetSiteDeleteSizeInfoWithSiteId(site => true);
        }

        public Dictionary<string, Tuple<string, long>> GetSiteDeleteSizeInfoWithSiteId(long startTime, long endTime)
        {
            return GetSiteDeleteSizeInfoWithSiteId(site => site.CreateTime <= endTime && site.CreateTime >= startTime);
        }

        public RMSiteDeletedSizeInfo GetSiteDeleteSizeInfoBySiteUrlAndJobId(string siteUrl, string jobId)
        {
            using var context = GetNewContext();
            return context.RMSiteDeletedSizeInfo.FirstOrDefault(a => a.SiteUrl == siteUrl && a.JobId == jobId);
        }

        private Dictionary<string, Tuple<string, long>> GetSiteDeleteSizeInfoWithSiteId(Expression<Func<RMSiteDeletedSizeInfo, bool>> wherePredicate)
        {
            using var context = GetNewContext();
            List<SiteDeletedSizeInfo> sizeInfos = new List<SiteDeletedSizeInfo>();
            Dictionary<string, Tuple<string, long>> res = new Dictionary<string, Tuple<string, long>>();
            int page = 0;
            int size = 1000;
            do
            {
                sizeInfos = context.RMSiteDeletedSizeInfo.AsNoTracking()
                    .Where(wherePredicate).OrderBy(info => info.CreateTime)
                    .Skip(page++ * size).Take(size)
                    .Select(info => new SiteDeletedSizeInfo
                    {
                        SiteUrl = info.SiteUrl,
                        SiteId = info.SiteId,
                        DeletedSize = info.DeletedSize
                    }).ToList();
                MergeSiteDeleteSizeInfo(res, sizeInfos);
            } while (sizeInfos.Count >= size);
            return res;
        }

        private void MergeSiteDeleteSizeInfo(Dictionary<string, Tuple<string, long>> statistic, List<SiteDeletedSizeInfo> sizeInfos)
        {
            Dictionary<string, Tuple<string, long>> temp = sizeInfos.GroupBy(a => a.SiteUrl)?.ToDictionary(a => a.Key, a => new Tuple<string, long>(a.FirstOrDefault().SiteId, a.Sum(b => b.DeletedSize)));
            foreach (string key in temp.Keys)
            {
                if (statistic.ContainsKey(key))
                {
                    Tuple<string, long> oldData = statistic[key];
                    Tuple<string, long> newData = temp[key];
                    statistic[key] = new Tuple<string, long>(oldData.Item1, oldData.Item2 + newData.Item2);
                }
                else
                {
                    statistic.Add(key, temp[key]);
                }
            }
        }
    }
}
