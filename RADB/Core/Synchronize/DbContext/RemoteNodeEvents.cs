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
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.DB.Core.Synchronize.DbContext.Base;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Core.Synchronize.DbContext;

public class RemoteNodeEvents : IRemoteNodeEvent
{
    private readonly List<IRemoteNodeSubscription> _remoteNodeSubscription = [];
    
    private readonly Dictionary<SynchronizeDbType, IRemoteNodeSubscription> _remoteNodeSubscriptionMap = new();

    public RemoteNodeEvents()
    {
        _remoteNodeSubscriptionMap.Add(SynchronizeDbType.Sqlite, new SqliteRemoteNodeSubscription(this));
        _remoteNodeSubscriptionMap.Add(SynchronizeDbType.SqlServer, new SqlServerRemoteNodeSubscription(this));
    }
    
    public void RegisterSubscription(IRemoteNodeSubscription remoteNodeSubscription)
    {
        _remoteNodeSubscription.Add(remoteNodeSubscription);
    }

    public void RemoveSubscription(IRemoteNodeSubscription remoteNodeSubscription)
    {
        _remoteNodeSubscription.Remove(remoteNodeSubscription);
    }
    
    public void RemoveSubscription(SynchronizeDbType synchronizeDbType)
    {
        _remoteNodeSubscription.Remove(_remoteNodeSubscriptionMap[synchronizeDbType]);;
    }

    public Task NotifyUpdateAsync(IEnumerable<RMRemoteNode> remoteNodes)
    {
        List<Task> tasks = _remoteNodeSubscription.Select(subscription => subscription.UpdateAsync(remoteNodes)).ToList();
        return Task.WhenAll(tasks);
    }

    public Task NotifyAddAsync(IEnumerable<RMRemoteNode> remoteNodes)
    {
        List<Task> tasks = _remoteNodeSubscription.Select(subscription => subscription.AddAsync(remoteNodes)).ToList();
        return Task.WhenAll(tasks);
    }

    public Task NotifyDeleteAsync(IEnumerable<string> remoteNodeIds)
    {
        List<Task> tasks = _remoteNodeSubscription.Select(subscription => subscription.DeleteAsync(remoteNodeIds)).ToList();
        return Task.WhenAll(tasks);
    }

    public Task NotifyDeleteContainerAsync(IEnumerable<string> remoteNodeIds)
    {
        List<Task> tasks = _remoteNodeSubscription.Select(subscription => subscription.DeleteContainerAsync(remoteNodeIds)).ToList();
        return Task.WhenAll(tasks);
        
    }
    
    public enum SynchronizeDbType
    {
        SqlServer,
        Sqlite
    }
}