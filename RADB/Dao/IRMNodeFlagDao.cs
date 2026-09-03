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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMNodeFlagDao
    {
        void AddSiteFlagInfo(RMNodeFlag scope);
        void AddListFlagInfo(RMNodeFlag scope);
        void ClearDataByType(int type);
        long GetCollectionTime(int type, Guid groupId, Guid nodeId);
        /// <summary>
        /// 对于依赖SP Change Log的Incremental Job, Collection startime不能小于60天前,通过daysBefore可以重置此值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="nodeId"></param>
        /// <param name="daysBefore">days</param>
        /// <returns></returns>
        long GetSPValidChangeTime(int type, Guid groupId, Guid nodeId, long daysBefore = 60);
        List<RMNodeFlag> GetExistScopeInfo(NodeFlagType flagType);

        bool IsNodeFlagExist(Guid groupId, Guid Id, int type);
        long GetAutoJobCollectionTime(int type, Guid folderId, Guid listId, Guid nodeId, Guid groupId);
        RMNodeFlag GetNodeFlagInfoById(Guid id, NodeFlagType flagType);
        Task<IEnumerable<RMNodeFlag>> LoadByPager(int pageIndex, int pageSize);
        Task<long> MultiGeoInsertNodeFlagTableAsync(IEnumerable<RMNodeFlag> nodeFlags);
        Task<long> MultiGeoDeleteAllNodeFlagAsync();
    }
}
