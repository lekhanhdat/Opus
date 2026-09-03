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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Archiver
{
    public class ArchiverManagementService: RMServiceBase, IMArchiverJobManagementService
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ArchiverManagementService));
        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private IArchiverIndexSubInfoDao ArchiverSubIndexDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        private ICommonSiteMasterIndexDao CommonSiteMasterIndexDao = PlatformWindsorManager.GetService<ICommonSiteMasterIndexDao>();

        public void AddSiteCollectionAfterRestore(RemoteSiteCollection siteCollection, string sitesGroupName, string groupId)
        {
            throw new NotImplementedException();
        }

        public bool CheckCurrentJobHasMerged(string jobId, string groupId)
        {
            logger.Info("Check current job has merged: job Id: {0}, group Id: {1}.", jobId, groupId);
            //var index = ArchiverSiteMasterIndexDao.Find(i => i.JobId == jobId);
            var index = ArchiverSubIndexDao.Find(i => i.SubSubJobId == jobId);
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

        public List<string> GetAllArchiverIndexSubInfo(string jobId, string groupId)
        {
            throw new NotImplementedException();
        }

        public byte[] GetEndUserStubLinkMasterKey(string groupId)
        {
            throw new NotImplementedException();
        }

        public string InsertIntoArchiverSiteMasterIndex(ArchiverSiteMasterIndexContract indexDto, string groupId)
        {
            throw new NotImplementedException();
        }

        public void NotificationRetentionState(ArchiverPruningJob retentionInfo, string groupId)
        {
            throw new NotImplementedException();
        }

        public void UpdateEndUserJobStatisticsByJobId(string jobId, string value, string groupId)
        {
            throw new NotImplementedException();
        }

        public void UpdateIndexDevice(string destIndexDeviceId, string groupId)
        {
            throw new NotImplementedException();
        }

        public void UpdateMainIndexStorageInfo(string siteCollectionUrl, string storageInfo, string groupId)
        {
            throw new NotImplementedException();
        }

        public async System.Threading.Tasks.Task UpdateMergeIndexStateAsync(string jobId, ArchiverSiteInfoDto siteInfo, MergeIndexState mergeIndexState, string groupId)
        {
            try
            {
                logger.Info("Update merge index state for job id: {0}, merge state: {1}, group Id: {2}.", jobId, mergeIndexState.ToString(), groupId);
                string subJobId = jobId.Substring(0, jobId.LastIndexOf("_", StringComparison.CurrentCulture));
                logger.Info("Update site master index state job id: {0}.", subJobId);
                await ArchiverSubIndexDao.UpdateArchiverIndexSubInfoMergeIndexStatusAsync(jobId, (int)mergeIndexState);
                var index = ArchiverSiteMasterIndexDao.Find(i => i.JobId == subJobId);
                if (index != null)
                {
                    index.MergeIndexState = (int)mergeIndexState;
                    await ArchiverSiteMasterIndexDao.UpdateAsync(index);
                }
                var commonIndex = CommonSiteMasterIndexDao.Find(i => i.JobId == subJobId);
                if (commonIndex != null)
                {
                    commonIndex.MergeIndexState = (int)mergeIndexState;
                    await CommonSiteMasterIndexDao.UpdateAsync(commonIndex);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Update merge index state failed: {ex},sub job id:{jobId}");
                throw;
            }
        }
        public async Task UpdateGDriveMergeIndexStateAsync(string jobId, ArchiverSiteInfoDto siteInfo, MergeIndexState mergeIndexState)
        {
            try
            {
                logger.Info("Update merge index state for job id: {0}, merge state: {1}, group Id: {2}.", jobId, mergeIndexState.ToString());
                await ArchiverSubIndexDao.UpdateGDriveArchiverIndexSubInfoMergeIndexStatusAsync(jobId, (int)mergeIndexState);
                var index = ArchiverSiteMasterIndexDao.Find(i => i.JobId == jobId);
                if (index != null)
                {
                    index.MergeIndexState = (int)mergeIndexState;
                    await ArchiverSiteMasterIndexDao.UpdateAsync(index);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Update merge index state failed: {ex},sub job id:{jobId}");
                throw;
            }
        }

        public void UpdateSiteCollectionAfterAchivered(string scUrl, bool isArchivered, string groupId)
        {
            throw new NotImplementedException();
        }

        public void UpdateSiteCollectionAfterAchivered(string scUrl, bool isArchivered, string groupId, string jobId)
        {
            throw new NotImplementedException();
        }

        public async System.Threading.Tasks.Task UpdateSiteMasterMediaDataSizeAsync(string subjobId, long mediaDataSize, string groupId)
        {
            logger.Info("Update site master media data size, subjobId: {0}, mediaDataSize: {1}, groupId: {2}.", subjobId, mediaDataSize, groupId);
            int retryCount = 0;
            while (retryCount <= 3)
            {
                try
                {
                    var subIndex = ArchiverSubIndexDao.Find(a => a.SubSubJobId == subjobId);
                    if (subIndex != null)
                    {
                        subIndex.MediaDataSize = mediaDataSize;
                        await ArchiverSubIndexDao.UpdateAsync(subIndex);
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

        public void UpdateSubIndexStorageInfo(string subJobId, string storageInfo, string groupId)
        {
            throw new NotImplementedException();
        }
    }
}
