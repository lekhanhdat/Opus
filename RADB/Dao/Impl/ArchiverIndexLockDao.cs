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
using AvePoint.Common;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class ArchiverIndexLockDao: BaseDao<ArchiverIndexLock>, IArchiverIndexLockDao
    {
        private readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ArchiverIndexLockDao));
        public bool CanBeRunJob(List<ArchiverSiteMasterIndexContract> siteInfos)
        {
            if (siteInfos == null || siteInfos.Count == 0)
            {
                return false;
            }
            bool canRun = true;

            foreach (ArchiverSiteMasterIndexContract site in siteInfos)
            {
                //检查每一个site collection
                if (!CheckSiteCollectionCanBeRunJob(site))
                {
                    logger.Info("Get site collection lock, lockedJobId:{0}, siteCollection Id:{1}, siteCollection Url:{2}.", site.LockedJobId, site.SiteId, site.SiteURL);
                    canRun = false;
                    break;
                }
            }
            if (canRun)
            {
                foreach (ArchiverSiteMasterIndexContract site in siteInfos)
                {
                    //为site collection加锁
                    if (!string.IsNullOrEmpty(site.LockedJobId))
                    {
                        using (var context = GetNewContext())
                        {
                            ArchiverIndexLock indexLock = new ArchiverIndexLock()
                            {
                                Id = Guid.NewGuid().ToString(),
                                SiteId = site.SiteId,
                                SiteUrl = site.SiteURL,
                                JobId = site.LockedJobId,
                                SiteGroupId = site.WebId,
                            };
                            context.ArchiverIndexLocks.Add(indexLock);
                            context.SaveChanges();
                        }
                        logger.Info("Create site collection lock for job id:{0}.", site.LockedJobId);
                    }
                }
            }

            return canRun;
        }

        public void DeleteLockByJobId(string jobId)
        {
            logger.Info("DeleteLockByJobId tenantGroupId:{0}, lockedJobId:{1}.", IdentityManager.IdentityContent, jobId);
            //删除site collection锁
            if (!string.IsNullOrEmpty(jobId))
            {
                using (var context = GetNewContext())
                {
                    var removeEntitys = context.ArchiverIndexLocks.Where(i => i.JobId.Equals(jobId, StringComparison.OrdinalIgnoreCase));
                    context.ArchiverIndexLocks.RemoveRange(removeEntitys);
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        ///检查是否site collection已加锁
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="siteInfo"></param>
        /// <returns></returns>
        private bool CheckSiteCollectionCanBeRunJob(ArchiverSiteMasterIndexContract siteInfo)
        {
            ArchiverIndexLock iLock = null;
            using (var context = GetNewContext())
            {
                iLock = context.ArchiverIndexLocks.Where(i => i.SiteId.Equals(siteInfo.SiteId, StringComparison.OrdinalIgnoreCase) && i.SiteUrl.Equals(siteInfo.SiteURL, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            }

            return iLock == null || string.IsNullOrEmpty(iLock.JobId) || siteInfo.LockedJobId == iLock.JobId;
        }
    }
}
