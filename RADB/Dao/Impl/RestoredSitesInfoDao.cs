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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RestoredSitesInfoDao : BaseDao<RestoredSitesInfo>, IRestoredSitesInfoDao
    {
        public void AddOrUpdateRestoredSite(RestoredSitesInfo info)
        {
            using var context = GetNewContext();
            context.RestoredSitesInfos.AddOrUpdate(info);
            context.SaveChanges();
        }

        public RestoredSitesInfo GetInfoByUrl(string Url)
        {
            RestoredSitesInfo result;
            using (var context = GetNewContext())
            {
                result = context.RestoredSitesInfos.AsQueryable().Where(a => a.SiteUrl == Url).FirstOrDefault();
            }
            return result;
        }

        public List<RestoredSitesInfo> GetAll()
        {
            using (var context = GetNewContext())
            {
                return context.RestoredSitesInfos.AsNoTracking().ToList();
            }
        }

        public void Remove(RestoredSitesInfo siteInfo)
        {
            using var context = GetNewContext();
            var willDeleteSiteInfoes = context.RestoredSitesInfos.Where(item => item.SiteUrl == siteInfo.SiteUrl).ToList();
            if(willDeleteSiteInfoes != null)
            {
                context.RestoredSitesInfos.RemoveRange(willDeleteSiteInfoes);
            }
            context.SaveChanges();
        }
    }
}
