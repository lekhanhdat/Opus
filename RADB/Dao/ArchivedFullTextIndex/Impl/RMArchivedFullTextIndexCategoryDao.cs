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


using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Model.ArchivedFullTextIndex;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.ArchivedFullTextIndex.Impl
{
    public class RMArchivedFullTextIndexCategoryDao : IRMArchivedFullTextIndexCategoryDao
    {
        public async Task<(bool has, RMArchivedDataFullTextIndexCategory category)> TryGetLatestAsync()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var res = await context.FullTextIndexCategories.OrderByDescending(item => item.Id).FirstOrDefaultAsync();
            return (res != null, res);
        }

        public async Task<RMArchivedDataFullTextIndexCategory> GetByIdAsync(int id)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var res = await context.FullTextIndexCategories.FirstAsync(item => item.Id == id);
            return res;
        }

        public async Task<(bool has, RMArchivedDataFullTextIndexCategory categoryInfo)> TryGetByIdAsync(int id)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var res = await context.FullTextIndexCategories.FirstOrDefaultAsync(item => item.Id == id);
            return (res != null, res);
        }

        public async Task AddOrUpdateAsync(RMArchivedDataFullTextIndexCategory category)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            context.FullTextIndexCategories.AddOrUpdate(category);
            await context.SaveChangesAsync();
        }

        public async Task<long> CountAsync()
        {
            using var context = RMDBContextManager.GetNewDBContext();
            return await context.FullTextIndexCategories.CountAsync();
        }

        public async Task<(bool has, RMArchivedDataFullTextIndexCategory category)> TryGetNextAvaliableCategoryAsync(int categoryId, int startMonth, int endMonth)
        {
            using var context = RMDBContextManager.GetNewDBContext();
            var res = await context.FullTextIndexCategories.Where(item => item.Id < categoryId && item.DataSize > 0 && ((item.StartMonth <= startMonth && item.EndMonth >= startMonth) || (item.StartMonth >= startMonth && item.StartMonth <= endMonth)))
                .OrderByDescending(item => item.Id).FirstOrDefaultAsync();
            return (res != null, res);
        }

        public async Task<(bool has, RMArchivedDataFullTextIndexCategory category)> TryGetSiteNextAvaliableCategoryAsync(string siteUrl, int categoryId, int startMonth, int endMonth)
        {

            //(c.startTime <= a && c.endTime >= a) || (c.startTime >= a && c.startTime <= b)
            using var context = RMDBContextManager.GetNewDBContext();
            var query = from siteInfo in context.FullTextIndexSiteInfoes
                        join jobInfo in context.FullTextIndexJobInfoes
                        on siteInfo.Id equals jobInfo.FullTextIndexSiteId
                        join category in context.FullTextIndexCategories
                        on jobInfo.FullTextIndexCategoryId equals category.Id
                        where siteInfo.SiteUrl == siteUrl &&
                        ((category.StartMonth <= startMonth && category.EndMonth >= startMonth) || (category.StartMonth >= startMonth && category.StartMonth <= endMonth)) &&
                        category.DataSize > 0 && category.Id < categoryId
                        orderby category.Id descending
                        select category;
            var res = await query.FirstOrDefaultAsync();
            return (res != null, res);
        }
    }
}
