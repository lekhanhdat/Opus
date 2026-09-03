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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.SyncNode;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Core.Synchronize.DbContext.Base;
using AvePoint.RA.DB.Dao.SynchronizeDao;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMSyncNodeDao
    {
        Task<List<string>> GetTenantIdListFromDB();
        Task<List<RMContainerInfoAdaption>> GetSiteContainersAsync(NodeLevel nodeLevel);

        Task<List<RMContainerInfoAdaption>> GetExchangeContainersAsync();

        IAsyncEnumerable<RMSiteNodeAdaption> GetSiteNodesAsync(string containerId, string tenantId);

        Task<List<RMExchangeNodeAdaption>> GetExchangeNodesAsync(string containerId, string tenantId);

        Task AddExchangeContainersAsync(IEnumerable<RMContainerInfoAdaption> containerInfoes);

        Task DeleteExchangeContainersAsync(IEnumerable<RMContainerInfoAdaption> containerInfoes);

        Task UpdateExchangeContainerAsync(IEnumerable<RMContainerInfoAdaption> containerInfoes);

        Task DeleteSiteContainersAsync(IEnumerable<RMContainerInfoAdaption> containerInfoes);

        Task UpdateSiteContainerAsync(IEnumerable<RMContainerInfoAdaption> containerInfoes);

        Task AddSiteContainersAsync(IEnumerable<RMContainerInfoAdaption> containerInfoes);

        Task UpdateExchangeNodesAsync(IEnumerable<RMExchangeNodeAdaption> nodes);

        Task DeleteExchangeNodesAsync(IEnumerable<RMExchangeNodeAdaption> nodes);

        Task AddExchangeNodesAsync(IEnumerable<RMExchangeNodeAdaption> nodes);

        Task UpdateSiteNodesAsync(IEnumerable<RMSiteNodeAdaption> nodes);

        Task DeleteSiteNodesAsync(IEnumerable<RMSiteNodeAdaption> nodes);

        Task AddSiteNodesAsync(IEnumerable<RMSiteNodeAdaption> nodes);

        Task<List<RMRemoteNode>> GetContainersAsync();

        Task<int> CountSiteAsync(IEnumerable<Guid> containerIds);

        Task<int> CountSiteAsync();

        Task<int> CountContainerAsync();

        Task<RMRemoteNode> GetContainerAsync(Guid containerId);

        IAsyncEnumerable<RMRemoteNode> GetSitesAsync(Guid containerId);

        Task<bool> HasAnySites();

        void InjectRemoteNodeSynchronizeEvent(IRemoteNodeEvent remoteNodeEvent);
    }
}
