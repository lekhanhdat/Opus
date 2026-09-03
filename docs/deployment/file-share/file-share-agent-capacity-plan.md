# Opus File Share Deployment Recommendation

## Purpose

This document records the recommended Connection Group and Agent VM deployment for the initial File Share synchronization.

## Connection Groups

Create Connection Groups based on actual data volume.

| Connection Group | Use | Agent VM Profile |
|---|---|---|
| High-Concurrency Connection Group | File Share Connections with large data volumes or higher concurrency needs | High-Concurrency Profile |
| Baseline Connection Group | Normal synchronization workloads, phased rollout, and ongoing operations | Baseline Profile |

## Agent VM Profiles

| Profile | Use | vCPU | RAM | Available Disk |
|---|---|---:|---:|---:|
| Baseline | Normal synchronization workloads | 8 | 16 GB | 200 GB |
| High-Concurrency | Higher-concurrency workloads | 16 | 32 GB | 200 GB |

The high-concurrency profile provides more CPU and memory for workloads that need more capacity.

## Two-Month Agent Deployment

The following deployment is recommended for an initial synchronization target of two months. Agent capacity is calculated separately for each DC and must not be shared across DCs.

| DC | Connections | Total Files and Folders | Recommended Agents |
|---|---:|---:|---:|
| APAC | 7,787 | 1,436,518,214 | 25 |
| EMEA | 4,942 | 1,080,020,415 | 19 |
| Latam | 1,376 | 145,226,983 | 3 |
| NAMR | 14,117 | 3,120,406,826 | 53 |
| **Total** | **28,222** | **5,782,172,438** | **100** |

## Notes

- This recommendation is based on current File Share synchronization test results and planning assumptions.
- Actual throughput can vary because of directory structure, permissions, network latency, API throttling, and retries.
- Use a phased rollout and review actual performance before increasing the number of active Agents.