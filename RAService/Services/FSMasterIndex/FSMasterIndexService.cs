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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.RMWeb.FSMasterIndex;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.FSMasterIndex
{
    [Audit]
    public class FSMasterIndexService : IFSMasterIndexService
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(FSMasterIndexService));
        private IFSMasterIndexDao FSMasterIndexDao => PlatformWindsorManager.GetService<IFSMasterIndexDao>();
        private IFSIndexSubInfoDao FSIndexSubInfoDao => PlatformWindsorManager.GetService<IFSIndexSubInfoDao>();

        public List<FSMasterIndexContract> GetAllConnectionNodsInfo()
        {
            return FSMasterIndexDao.GetAllConnectionsInfo();
        }

        public string InsertIntoFSMasterIndex(FSMasterIndexContract indexDto)
        {
            return FSMasterIndexDao.InsertIntoFSMasterIndex(indexDto);
        }
        public FSMasterIndexContract GetConnectionMasterInfo(FSMasterIndexContract connection)
        {
            FSMasterIndexContract index = FSMasterIndexDao.GetConnectionInfo(connection);
            return index;
        }
        public FSMasterIndexContract GetConnectionMasterInfoByConnectionId(string connectionId)
        {
            List<FSMasterIndexContract> indexes = FSMasterIndexDao.GetConnectionInfos(connectionId);
            return indexes?.FirstOrDefault();
        }
        public List<FSMasterIndexContract> GetConnectionMasterWithSubInfosList(string connectionId)
        {
            List<FSMasterIndexContract> indexes = FSMasterIndexDao.GetConnectionInfos(connectionId);
            if (!indexes.IsNullOrEmpty())
            {
                foreach (var index in indexes)
                {
                    List<ArchiverIndexSubInfoContract> subInfos = FSIndexSubInfoDao.GetSubInfoesBySubJobId(index.JobId);
                    if (!subInfos.IsNullOrEmpty())
                    {
                        index.SubInfo = subInfos;
                    }
                    else
                    {
                        mLogger.Info("Sub info for node {0} is null or empty.", index.ConnectionId);
                    }
                }
            }
            return indexes;
        }

        public FSMasterIndexContract GetMasterIndexBySubjobId(string subJobId)
        {
            return FSMasterIndexDao.GetIndexByJobId(subJobId)?.FirstOrDefault();
        }

        public void DeleteFSMasterIndex(FSMasterIndexContract indexInfo)
        {
            var index = FSMasterIndexDao.Find(s=>s.Id == indexInfo.Id);
            if (index != null)
            {
                FSMasterIndexDao.Delete(index);
            }
            else
            {
                mLogger.Warn("The index info job id {0} is not exist.", indexInfo.JobId);
            }
        }
    }
}
