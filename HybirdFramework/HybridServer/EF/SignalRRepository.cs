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
using HybirdProxy;
using HybridServer.EF.Entity;
using HybridServer.Log;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HybridServer.EF
{

    public class SignalRRepository
    {
        private readonly SignalRRDBContext _context;


        public SignalRRepository(SignalRRDBContext context)
        {
            _context = context;
        }

        public List<Agent> GetAgents(string tenantId)
        {
            return _context.Agents.Where(a => a.TenantId == tenantId).AsNoTracking().ToList();
        }

        public List<Agent> GetAgents()
        {
            return _context.Agents.AsNoTracking().ToList();
        }

        public async Task<bool> AddOrUpdateAgentAsync(Agent agent)
        {
            var curAgent =_context.Agents.Where(a => a.AgentId == agent.AgentId).FirstOrDefault();
            if(curAgent != null)
            {
                var needNotificate = (curAgent.ConnectionId != agent.ConnectionId) || (curAgent.Status != agent.Status);

                curAgent.ConnectionId = agent.ConnectionId;
                curAgent.Status = agent.Status;
                curAgent.TenantId = agent.TenantId;
                curAgent.LastConnected = agent.LastConnected;
                await _context.SaveChangesAsync();
                return needNotificate;
            }
            else
            {
                _context.Agents.Add(agent);
                await _context.SaveChangesAsync();
                return true;
            }
        }
        
        public async Task<int> AgentDisconnect(string agentId)
        {
            var curAgent = _context.Agents.Where(a => a.AgentId == agentId).FirstOrDefault();
            if (curAgent == null)
            {
                throw new UnexpectedException($"agentId not found:{agentId}");
            }

            curAgent.Status = ConnectionStatus.Disconnected;
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> EnsureAgentConnectionStatus()
        {
            var expiredDatetime = DateTime.UtcNow.AddMinutes(-2);
            
            var expiredAgent = _context.Agents.Where( a => a.LastConnected < expiredDatetime && a.Status == ConnectionStatus.Connected);

            if(expiredAgent.Any())
            {
                await expiredAgent.ForEachAsync(a => a.Status = ConnectionStatus.Disconnected);
                await _context.SaveChangesAsync();
            }
            
            return await Task.FromResult(expiredAgent.Any());
        }

        public async Task BulkMergeAsync(List<Agent> agents)
        {
            if (agents == null || !agents.Any()) return;
            try
            {
                var incomingIds = agents.Select(x => x.AgentId).ToList();
                var existingIds = await _context.Agents.Where(x => incomingIds.Contains(x.AgentId)).Select(x => x.AgentId).ToListAsync();
                var existingIdSet = new HashSet<string>(existingIds);
                foreach (var agent in agents)
                {
                    if (existingIdSet.Contains(agent.AgentId))
                    {
                        _context.Agents.Attach(agent);
                        var entry = _context.Entry(agent);
                        entry.State = EntityState.Modified;
                        entry.Property(x => x.RegistrationTime).IsModified = false;
                        entry.Property(x => x.TenantId).IsModified = false;
                    }
                    else
                    {
                        _context.Agents.Add(agent);
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {

            }
            //await _context.Agents.BulkMergeAsync(agents);
        }
    }
}
