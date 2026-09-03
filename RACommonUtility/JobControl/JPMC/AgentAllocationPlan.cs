// Immutable result of a distribution calculation.
using System;
using System.Collections.Generic;

public sealed class AgentAllocationPlan
{
    // subJobId -> agentId that must execute it immediately.
    public IReadOnlyDictionary<string, string> ImmediateAssignments { get; init; }
        = new Dictionary<string, string>();

    // Sub-jobs with no free slot; must be persisted as Waiting.
    public IReadOnlyList<string> DeferredSubJobIds { get; init; }
        = new List<string>();
}

public sealed class AgentCapacity
{
    public string AgentId { get; init; }
    public int MaxConcurrent { get; init; }   
    public int CurrentLoad { get; init; }      
    public int Remaining => Math.Max(MaxConcurrent - CurrentLoad, 0);
}