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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model.DisposalStub;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.DisposalStubDao.Impl
{
    public class RMStubDisposalSiteInfoDao : BaseDao<RMStubDisposalSiteInfo>, IRMStubDisposalSiteInfoDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMStubDisposalSiteInfoDao));
        private static string TableName = "RMStubDisposalSiteInfoes";

        public async Task AddOrUpdateStubDisposalSiteInfoAsync(RMStubDisposalSiteInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            if (info.Id == Guid.Empty)
            {
                logger.Info($"Create new StubDisposalSiteInfo for siteUrl: {info.SiteCollectionUrl}");
                info.Id = Guid.NewGuid();
            }

            using (var context = GetNewContext())
            {

                context.RMStubDisposalSiteInfoes.AddOrUpdate(info);
                await context.SaveChangesAsync();
            }
        }

        public async Task<RMStubDisposalSiteInfo> GetStubDisposalSiteInfoBySiteUrlAsync(string siteCollectionUrl)
        {
            using (var context = GetNewContext())
            {
                var existResult = await context.RMStubDisposalSiteInfoes.FirstOrDefaultAsync(m => m.SiteCollectionUrl == siteCollectionUrl);

                if (existResult == null)
                {
                    logger.Info($"No existing RMStubDisposalSiteInfo found for siteUrl: {siteCollectionUrl}");
                }

                return existResult;
            }
        }

        public void UpdateRetentionBySiteUrl(string siteUrl, long minRetentionTime)
        {
            using (var context = GetNewContext())
            {
                string sql = $"update {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.{TableName} set {nameof(RMStubDisposalSiteInfo.MinRetentionTime)} = @MinRetentionTime where {nameof(RMStubDisposalSiteInfo.SiteCollectionUrl)} = @SiteCollectionUrl";

                int result = context.Database.ExecuteSqlCommand(string.Format(sql, context.SchemaName),
                    new SqlParameter("MinRetentionTime", minRetentionTime),
                    new SqlParameter("SiteCollectionUrl", siteUrl)
                    );
            }
        }

        public List<RMStubDisposalSiteInfo> GetStubDisposalSiteInfoesByRetentionTime(long retentionTime)
        {
            using (var context = GetNewContext())
            {
                var result = context.RMStubDisposalSiteInfoes.Where(m => m.MinRetentionTime <= retentionTime).ToList();
                return result;
            }
        }

        public async Task UpdateMinRetentionTimeAsync(Guid id, long globalMinNextRunTicks)
        {
            using (var context = GetNewContext())
            {
                string sql = $"update {SecurityUtils.SanitizeSQLSchemaName(context.SchemaName)}.{TableName} set {nameof(RMStubDisposalSiteInfo.MinRetentionTime)} = @MinRetentionTime where {nameof(RMStubDisposalSiteInfo.Id)} = @Id";

                int result = context.Database.ExecuteSqlCommand(string.Format(sql, context.SchemaName),
                    new SqlParameter("MinRetentionTime", globalMinNextRunTicks),
                    new SqlParameter("Id", id)
                    );
            }
        }
    }
}
