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
using AvePoint.Hybrid.Contract.Object;
using CommonModel.DataModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.SignalR
{
    public interface ISignalRService
    {
        void SignalRSetup();
        /// <summary>
        /// select agents order by CPU usage, it will return all of the active agents without checking license or source type.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        Task<ICollection<AgentInformation>> GetAgentsAsync(string tenantId);

        Task<ICollection<AgentInformation>> GetAgentsByFarmIdAsync(string tenantId, string farmId);
        /// <summary>
        /// select agents order by CPU usage, it will check the license according to the source type
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<ICollection<AgentInformation>> GetAgentsByTypeAsync(string tenantId, AvePoint.Hybrid.Contract.Object.SourceType type);

        Task<ICollection<AgentInformation>> GetAvailableAgentsAsync(string tenantId);

        Task<ICollection<AgentInformation>> GetAgentsByTypeAndConnectionGroupIdAsync(string tenantId, SourceType sourceType, Guid connectionGroupId);

        Task<ICollection<AgentInformation>> GetAgentsByTypeAndAgentIdsAsync(string tenantId, SourceType sourceType, List<Guid> agentIds);
    }
}
