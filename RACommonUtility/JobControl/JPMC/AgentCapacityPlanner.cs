using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Calculates an even distribution of pending sub-jobs across available agents
/// while strictly honoring each agent's DB-defined concurrent-job limit.
/// Contains NO business logic for sub-job execution — scheduling only.
/// </summary>
public sealed class AgentCapacityPlanner
{
    private readonly RALogger _logger = RALogger.GetInstance(typeof(AgentCapacityPlanner));
    private readonly IRMSubJobDao _subJobDao;
    private readonly List<JobType> _agentJobTypes;

    public AgentCapacityPlanner(IRMSubJobDao subJobDao, List<JobType> agentJobTypes)
    {
        _subJobDao = subJobDao;
        _agentJobTypes = agentJobTypes;
    }

    /// <summary>
    /// Builds capacity per agent
    /// (FSHighPerformanceSetting.MaxJobPerAgent)
    /// </summary>
    public IReadOnlyList<AgentCapacity> BuildCapacities(IEnumerable<string> availableAgentIds, int maxJobPerAgent)
    {
        Dictionary<string, int> currentLoad = _subJobDao.GetAgentJobCount(_agentJobTypes);

        var capacities = availableAgentIds
            .Distinct()
            .Select(id => new AgentCapacity
            {
                AgentId = id,
                MaxConcurrent = maxJobPerAgent,
                CurrentLoad = currentLoad.TryGetValue(id, out var c) ? c : 0
            })
            .ToList();

        foreach (var cap in capacities)
        {
            _logger.Info("Agent {0} capacity: max={1} load={2} remaining={3}", cap.AgentId, cap.MaxConcurrent, cap.CurrentLoad, cap.Remaining);
        }
        return capacities;
    }

    public AgentAllocationPlan Distribute(
        IReadOnlyList<string> pendingSubJobIds,
        IReadOnlyList<AgentCapacity> capacities)
    {
        var immediate = new Dictionary<string, string>();
        var deferred = new List<string>();

        var projected = capacities.ToDictionary(
            c => c.AgentId,
            c => new { c.MaxConcurrent, Load = c.CurrentLoad });

        foreach (var subJobId in pendingSubJobIds)
        {
            // Pick the agent with the most remaining headroom (even spread).
            var target = projected
                .Where(p => p.Value.MaxConcurrent - p.Value.Load > 0)
                .OrderByDescending(p => p.Value.MaxConcurrent - p.Value.Load)
                .ThenBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
                .Select(p => (string?)p.Key)
                .FirstOrDefault();

            if (target is null)
            {
                deferred.Add(subJobId); // no capacity anywhere -> Waiting
                continue;
            }

            immediate[subJobId] = target;
            var prev = projected[target];
            projected[target] = new { prev.MaxConcurrent, Load = prev.Load + 1 };
        }

        _logger.Info("Distribution complete. Immediate={0}, Deferred={1}", immediate.Count, deferred.Count);

        return new AgentAllocationPlan
        {
            ImmediateAssignments = immediate,
            DeferredSubJobIds = deferred
        };
    }
}