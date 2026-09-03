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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Archiver
{
    [Audit]
    public class FSArchiverManagementService : RMServiceBase, IMFSArchiverJobManagementService
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ArchiverRuleService));

        private IFSIndexSubInfoDao FSIndexSubInfoDao => PlatformWindsorManager.GetService<IFSIndexSubInfoDao>();
        private IFSMasterIndexDao FSMasterIndexDao => PlatformWindsorManager.GetService<IFSMasterIndexDao>();

        public bool CheckCurrentJobHasMerged(string jobId, string groupId)
        {
            logger.Info("Check current job has merged: job Id: {0}, group Id: {1}.", jobId, groupId);
            //var index = ArchiverSiteMasterIndexDao.Find(i => i.JobId == jobId);
            var index = FSIndexSubInfoDao.Find(i => i.SubSubJobId == jobId);
            if (index != null)
            {
                return index.MergeIndexState == (int)MergeIndexState.Succeed || index.MergeIndexState == (int)MergeIndexState.DAOMigrated;
            }
            else
            {
                logger.Warn("Cannot find ArchiverSiteMasterIndex by job Id: {0}, group Id: {1}.", jobId, groupId);
                return false;
            }
        }

        public async Task UpdateMergeIndexStateAsync(string jobId, ArchiverSiteInfoDto siteInfo, MergeIndexState mergeIndexState, string groupId)
        {
            try
            {
                logger.Info("Update merge index state for job id: {0}, merge state: {1}, group Id: {2}.", jobId, mergeIndexState.ToString(), groupId);
                string subJobId = jobId.Substring(0, jobId.LastIndexOf("_", StringComparison.CurrentCulture));
                logger.Info("Update site master index state job id: {0}.", subJobId);
                var index = FSMasterIndexDao.Find(i => i.JobId == subJobId);
                await FSIndexSubInfoDao.UpdateArchiverIndexSubInfoMergeIndexStatusAsync(jobId, (int)mergeIndexState);
                if (index != null)
                {
                    index.MergeIndexState = (int)mergeIndexState;
                }
                await FSMasterIndexDao.UpdateAsync(index);
            }
            catch (Exception ex)
            {
                logger.Error($"Update merge index state failed: {ex},sub job id:{jobId}");
                throw;
            }
        }

        public async Task UpdateSiteMasterMediaDataSizeAsync(string subjobId, long mediaDataSize, string groupId)
        {
            logger.Info("Update site master media data size, subjobId: {0}, mediaDataSize: {1}, groupId: {2}.", subjobId, mediaDataSize, groupId);
            int retryCount = 0;
            while (retryCount <= 3)
            {
                try
                {
                    var subIndex = FSIndexSubInfoDao.Find(a => a.SubSubJobId == subjobId);
                    if (subIndex != null)
                    {
                        subIndex.MediaDataSize = mediaDataSize;
                        await FSIndexSubInfoDao.UpdateAsync(subIndex);
                    }
                    else
                    {
                        logger.Info("Cannot find subIndex, JobId {0}.", subjobId);
                    }
                    break;
                }
                catch (Exception ex)
                {
                    logger.Error($"Update site master media data size failed, subjobId: {subjobId}, mediaDataSize: {mediaDataSize}, groupId: {groupId}, error: {ex}.retry count:{retryCount}");
                    Thread.Sleep(5000);
                    if (retryCount >= 3)
                    {
                        throw;
                    }
                    retryCount++;
                }
            }
        }
    }
}
