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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.FileSystem
{
    public class FSIndexSubInfoService : IFSIndexSubInfoService
    {
        private IFSIndexSubInfoDao FSIndexSubInfoDao => PlatformWindsorManager.GetService<IFSIndexSubInfoDao>();

        private IRMRetentionSimulateInfosDao RMRetentionSimulateInfosDao => PlatformWindsorManager.GetService<IRMRetentionSimulateInfosDao>();

        public void DeleteFSIndexSubInfo(ArchiverIndexSubInfoContract subInfo)
        {
            FSIndexSubInfoDao.Delete(GetSubInfoInternal(subInfo));
        }

        public bool ExistFSIndexSubInfoBySubJobId(string subJobId)
        {
            return FSIndexSubInfoDao.GetSubInfoesBySubJobId(subJobId).Count > 0;
        }

        public ArchiverIndexSubInfoContract GetFSIndexSubinfoBySubsubJobId(string subsubJobId)
        {
            return FSIndexSubInfoDao.GetIndexBySubSubJobId(subsubJobId);
        }

        public void UpdateArchiverIndexSubInfoMediaSize(string jobId, long size)
        {
            FSIndexSubInfoDao.UpdateArchiverIndexSubInfoMediaSizeAsync(jobId, size).GetAwaiter().GetResult();
        }

        public void UpdateArchiverRetentionSimulateSize(long size, long fileNumber)
        {
            RMRetentionSimulateInfosDao.AccumulateUpdateRetentionInfo((int)SourceFlag.FileSystem, fileNumber, size);
        }

        public void UpdateFSIndexSubInfo(ArchiverIndexSubInfoContract subInfo)
        {
            FSIndexSubInfoDao.UpdateAsync(ConvertToInfo(subInfo)).GetAwaiter().GetResult();
        }
        private FSIndexSubInfo ConvertToInfo(ArchiverIndexSubInfoContract domain)
        {
            if (domain == null)
            {
                return null;
            }
            FSIndexSubInfo info = FSIndexSubInfoDao.Find(s=>s.Id == domain.Id);
            info.RetentionCount = domain.RetentionCount;
            info.RetentionTime = domain.RetentionTime;
            info.CurrentStorageId = domain.CurrentStorageId;
            return info;
        }
        private FSIndexSubInfo GetSubInfoInternal(ArchiverIndexSubInfoContract domain)
        {
            if (domain == null)
            {
                return null;
            }
            FSIndexSubInfo info = FSIndexSubInfoDao.Find(s => s.Id == domain.Id);
            return info;
        }
    }
}
