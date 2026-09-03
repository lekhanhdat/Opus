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
using AvePoint.RA.ArchiverMigration.Dto;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using Cloud.Sdk.Dao.Services;
using Cloud.Sdk.Data.Dao;
using RAArchiverMaintenance.Deduplication;

namespace AvePoint.RA.ArchiverMigration.JobStage
{
    public class MigrateWPPDeDupStage
    {

        private static AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(MigrateWPPDeDupStage));
        private IArchiverDedupInfoDao ArchiverDedupInfoDao => PlatformWindsorManager.GetService<IArchiverDedupInfoDao>();


        public void RebuildDeDupForWPPMigration()
        {
            logger.Info($"Fetch dedup infoes");
            var dedupInfoes = ArchiverDedupInfoDao.GetAllDedupCollections();

            var count = dedupInfoes?.Count ?? 0;
            logger.Info($"Fetched dedup infoes: {count}");

            if (dedupInfoes != null && dedupInfoes.Count > 0)
            {
                var upgrader = new ArchiverDedupIndexDBUpgrader();
                foreach (var dedupInfo in dedupInfoes)
                {
                    if (!upgrader.Upgrade(dedupInfo))
                    {
                        throw new Exception($"Upgrade dedup index failed: {dedupInfo}");
                    }
                }
            }
        }
    }
}
