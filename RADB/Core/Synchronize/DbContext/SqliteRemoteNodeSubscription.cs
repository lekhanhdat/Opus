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
using System.Collections.Generic;
using System.Threading.Tasks;
using AvePoint.RA.DB.Core.Synchronize.DbContext.Base;
using AvePoint.RA.DB.Dao.SynchronizeDao;
using AvePoint.RA.DB.Dao.SynchronizeDao.Imp;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Core.Synchronize.DbContext;

public class SqliteRemoteNodeSubscription : IRemoteNodeSubscription
{
    private readonly IRemoteNodeEvent _remoteNodeEvent;
    
    public IRemoteNodeSynchronizeDao RemoteNodeSynchronizeDao => new SqliteRemoteNodeSynchronizeDao();

    public SqliteRemoteNodeSubscription(IRemoteNodeEvent remoteNodeEvent)
    {
        _remoteNodeEvent = remoteNodeEvent;
        _remoteNodeEvent.RegisterSubscription(this);
    }
    
    public Task UpdateAsync(IEnumerable<RMRemoteNode> remoteNodes)
    {
        return RemoteNodeSynchronizeDao.UpdateNodesAsync(remoteNodes);
    }

    public Task AddAsync(IEnumerable<RMRemoteNode> remoteNodes)
    {
        return RemoteNodeSynchronizeDao.AddNodesAsync(remoteNodes);
    }

    public Task DeleteAsync(IEnumerable<string> remoteNodeIds)
    {
        return RemoteNodeSynchronizeDao.DeleteNodesAsync(remoteNodeIds);
    }

    public Task DeleteContainerAsync(IEnumerable<string> remoteNodeIds)
    {
        return RemoteNodeSynchronizeDao.DeleteNodesByParentIdsAsync(remoteNodeIds);
    }
}