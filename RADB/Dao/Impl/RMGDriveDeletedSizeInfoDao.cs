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
using AvePoint.RA.Contract.RMWeb.Google.GDriveDeletedSizeInfo;
using AvePoint.RA.Contract.RMWeb.SiteDeletedSizeInfo;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMGDriveDeletedSizeInfoDao : BaseDao<RMGDriveDeletedSizeInfo>, IRMGDriveDeletedSizeInfoDao
    {
        public async Task CreateInfo(RMGDriveDeletedSizeInfo info)
        {
            using var context = GetNewContext();
            context.RMGDriveDeletedSizeInfo.Add(info);
            await context.SaveChangesAsync();
        }

        public Dictionary<string, GDriveDeletedSizeInfo> GetGDriveDeleteSizeInfoWithDriveId()
        {
            return GetGDriveDeleteSizeInfoWithDriveId(drive => true);
        }

        public Dictionary<string, GDriveDeletedSizeInfo> GetGDriveDeleteSizeInfoWithDriveId(long startTime, long endTime)
        {
            return GetGDriveDeleteSizeInfoWithDriveId(site => site.CreateTime <= endTime && site.CreateTime >= startTime);
        }

        private Dictionary<string, GDriveDeletedSizeInfo> GetGDriveDeleteSizeInfoWithDriveId(Expression<Func<RMGDriveDeletedSizeInfo, bool>> wherePredicate)
        {
            using var context = GetNewContext();
            List<GDriveDeletedSizeInfo> sizeInfos = new List<GDriveDeletedSizeInfo>();
            Dictionary<string, GDriveDeletedSizeInfo> res = new Dictionary<string, GDriveDeletedSizeInfo>();
            int page = 0;
            int size = 1000;
            do
            {
                sizeInfos = context.RMGDriveDeletedSizeInfo.AsNoTracking()
                    .Where(wherePredicate).OrderBy(info => info.CreateTime)
                    .Skip(page++ * size).Take(size)
                    .Select(info => new GDriveDeletedSizeInfo
                    {
                        DriveId = info.DriveId,
                        DriveName = info.DriveName,
                        DeletedSize = info.DeletedSize,
                        TenantId = info.TenantId
                    }).ToList();
                MergeGDriveDeleteSizeInfos(res, sizeInfos);
            } while (sizeInfos.Count >= size);
            return res;
        }
        private void MergeGDriveDeleteSizeInfos(Dictionary<string, GDriveDeletedSizeInfo> statistic, List<GDriveDeletedSizeInfo> driveInfos)
        {
            var temp = driveInfos.Where(a => !string.IsNullOrEmpty(a.DriveId)).GroupBy(a => a.DriveId).ToDictionary(
                    g => g.Key,
                    g => new GDriveDeletedSizeInfo
                    {
                        TenantId = g.First().TenantId,
                        DriveId = g.Key,
                        DriveName = g.First().DriveName,
                        DeletedSize = g.Sum(x => x.DeletedSize)
                    });

            foreach (var key in temp.Keys)
            {
                if (statistic.ContainsKey(key))
                {
                    statistic[key].DeletedSize += temp[key].DeletedSize;
                }
                else
                {
                    statistic.Add(key, temp[key]);
                }
            }
        }
    }
}
