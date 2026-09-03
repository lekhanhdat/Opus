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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExchangeBackupUtility;
using AvePoint.RA.Contract.Object;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using Microsoft.Exchange.WebServices.Data;

namespace AvePoint.RA.RAExchange.Discover.DiscoverImpl
{
    public class IncrementalDiscover : IBatchDiscover
    {
        private readonly RMEXODiscoverHelper discoverHelper = null;
        private readonly NodeFlagType nodeFlagType;
        private Guid groupId;

        //目前的DAO 对象放在了当前类中。考虑到所有Inc 都需要用，暂时不进行抽离。后期可设计成传来一个syncdata 即可
        private IEXONodeFlagDao mEXONodeInfoDao;
        protected IEXONodeFlagDao EXONodeInfoDao
        {
            get
            {
                if (mEXONodeInfoDao == null)
                {
                    mEXONodeInfoDao = new EXONodeFlagDao();
                }
                return mEXONodeInfoDao;
            }
        }

        public IncrementalDiscover(RMEXODiscoverHelper helper, NodeFlagType nodeType, Guid groupId)
        {
            this.discoverHelper = helper;
            this.nodeFlagType = nodeType;
            this.groupId = groupId;
        }

        public IEnumerable<ExchangeItemGroup> GetGroupedItems(ExchangeFolder folder, SearchFilter extraFilter = null)
        {
            //此处存储的均是FolderID，因此不需要修改逻辑支持Incremental July 2021.
            var nodeInfo = EXONodeInfoDao.GetEXONodeInfo(folder.FolderId.ToMd5(), groupId, (int)nodeFlagType);
            return discoverHelper.GetGroupedItemsAsync(folder, nodeInfo?.ItemSyncState).GetConsumingEnumerable();
        }
    }
}
