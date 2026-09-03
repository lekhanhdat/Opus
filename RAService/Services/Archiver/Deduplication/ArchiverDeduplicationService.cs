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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.ArchiverDeduplication;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Archiver.Deduplication
{
    public class ArchiverDeduplicationService : IArchiverDeduplicationService
    {
        private static IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private static IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        private static IArchiverDedupInfoDao ArchiverDedupInfoDao => PlatformWindsorManager.GetService<IArchiverDedupInfoDao>();


        public string GetArchiverIndexStorageId(string jobId)
        {
            var subInfo = ArchiverIndexSubInfoDao.Find(i => i.SubSubJobId == jobId);
            return subInfo?.CurrentStorageId;
        }

        public void UpdateArchiverMasterIndexDeduplicatedState(IEnumerable<string> idList)
        {
            ArchiverSiteMasterIndexDao.UpdateArchiverMasterIndexDeduplicatedState(idList);
        }

        public void UpsertArchiverDedupInfo(string siteUrl, long minDedupTime, long maxDedupTime)
        {
            ArchiverDedupInfoDao.UpsertArchiverDedupInfo(siteUrl, minDedupTime, maxDedupTime);
        }

        public List<string> GetDedupSiteCollections(long dedupFrom, long dedupTo)
        {
            return ArchiverDedupInfoDao.GetDedupSiteCollections(dedupFrom, dedupTo);
        }
    }
}
