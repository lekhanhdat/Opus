
# Architecture Decision Record

**Number:** 0001
**Title:** Producer-Consumer Bulk Processing Pipeline: Azure Storage + SQL Server (Simulated Queue)
**Status:** Proposed
**Date:** 2025-12-31

## Context

### Problem
Under high job/request concurrency, the Public API role risks performance degradation and becoming unresponsive. The current “request == processing” model couples:
- bulk data transfer,
- CPU/IO-intensive processing logic, and
- long-running job lifecycle management

into the API request path, leading to thread/connection pool exhaustion, request backlog, timeouts, and cascading failures.

### Current State
- The Agent triggers processing through the Public API and depends on a synchronous or semi-synchronous execution path.
- Bulk payloads amplify latency and failure rate during peak API load.
- There is no standardized mechanism for queuing, consuming, progress tracking, and result delivery.

### Goals
1. Primary goal: Build a horizontally scalable producer-consumer bulk processing pipeline that decouples long-running processing from the Public API request path.

### Constraints & Considerations
* Security
  - Upload/download uses SAS URIs (Add/Write/Read) issued by the Public API, with least privilege and short TTL.
  - SAS leakage, replay, and unauthorized access must be prevented; all actions should be auditable.
* Technology
  - All data transmission uses Protobuf (`.bin`).
  - Storage uses Azure Storage (Blob + Table).
  - The queue capability must be cross-cloud: SQL Server is available in both Azure and GCP.
* Operational limitations
  - Backpressure and retry must be controlled under high concurrency to avoid unbounded accumulation and overload.
  - The system must be explicit about “at-least-once” semantics and consumer-side idempotency/deduplication.
  - **We must cap the number of concurrently running jobs per customer** to prevent a single large customer from monopolizing capacity.
    - A queue (Azure Queue or a SQL-simulated queue) does not inherently guarantee per-customer fairness / throttling.
    - The Public API is responsible for “issue SAS + enqueue + write status” and does not enforce rate limiting; per-customer max concurrency should be enforced by the Scheduler (Timer / job-claim logic).
    - Desired behavior: even if a customer has many messages, processing should be queued and executed up to a configured per-customer concurrency limit, not all at once.

### Options Considered
**Option 1: Azure Storage Queue / Azure Service Bus (managed queue)**  
Summary: Use an Azure-native queue; Jobs consume messages and process Blob/Table.  
Pros:  
- Mature queue features with good throughput and scalability
- Managed availability reduces operational burden
Cons:  
- Azure-only service: not reusable in GCP; vendor lock-in risk
- Multi-cloud requires maintaining divergent queue implementations and consistency strategies

**Option 2: SQL Server simulated queue (cross-cloud)**  
Summary: Implement queue semantics in SQL Server tables (enqueue/dequeue, visibility timeout, retry, dead-letter), while keeping Blob/Table in Azure Storage.  
Pros:  
- Deployable in both Azure and GCP, satisfying cross-cloud alignment
- Reuses existing SQL Server operations/monitoring practices
- Close to job logs/state data for easier correlation and troubleshooting
Cons:  
- Queue semantics must be implemented and maintained (leases, retries, dead-letter, cleanup), increasing complexity
- High throughput can create database contention; requires careful batching/indexing/partitioning
- Transaction boundaries must be designed to avoid duplicates or lost updates

**Option 3: Self-managed cross-cloud messaging (Kafka/RabbitMQ/Redis Streams, etc.)**  
Summary: Introduce an independent messaging system as the queue layer; Jobs consume and process Blob/Table.  
Pros:  
- High throughput with mature consumer-group and replay capabilities
- Consistent messaging across clouds
Cons:  
- Significantly higher infrastructure and operational complexity (clusters, upgrades, DR, cost)
- Adds new dependencies beyond the current scope

## Decision

1. Chosen Solution: Option 2 (SQL Server simulated queue)
2. Rationale:
   - **Cross-cloud portability**: SQL Server is available on Azure and GCP, avoiding an Azure-only queue dependency.
   - **Decoupling and scalability**: Removes long-running work from the API request path and scales via consumers/jobs.
   - **Operational governance**: Centralizes queue and execution logs in SQL for monitoring, alerting, and auditing.
3. Rejected Alternatives:
   - Option 1 rejected due to Azure-only lock-in and poor multi-cloud reuse.
   - Option 3 rejected due to operational and cost overhead.
4. Key Assumptions:
   - Delivery semantics are “at-least-once”; consumers implement idempotent processing (same message may be processed more than once but produces consistent results).
   - Input blobs are immutable after upload for a given message.
   - Batch processing is triggered by a configurable threshold (batch threshold) and can be tuned over time.
5. Implementation Outline:
   - Phase 1 (API + storage contract)
     - Public API provides endpoints to obtain SAS URIs (Add/Write/Read).
     - Define Protobuf `.bin` schemas and a versioning strategy (backward compatible, reserved fields).
   - Phase 2 (enqueue + status)
     - Agent uploads input `.bin` to Azure Blob.
     - When the batch threshold is reached:
       - Public API creates a corresponding Azure Table record (Queued/Processing/Completed/Failed).
       - Public API inserts a message record into the SQL Server queue table.
   - Phase 3 (scheduling + execution)
     - Timer monitors (peek/claim) the SQL queue; when work is available it starts the corresponding job and writes job info to SQL Server.
     - Jobs continuously claim messages from the SQL queue:
       - Download input `.bin` from Blob and process.
       - Write result `.bin` back to Blob.
       - Update Azure Table status/progress/result location.
       - Mark the SQL queue message as Completed; on failure retry or dead-letter.
   - Phase 4 (progress + result retrieval)
     - Agent queries progress for a message via the Public API (aggregated from Azure Table + SQL job logs).
     - Once completed, Agent downloads the result `.bin` and performs analysis.
6. Approval: Damon Li, 2026-1-1

## Consequences

### Positive Impacts
- Public API shifts from synchronous processing to “issue SAS + enqueue + status query”, reducing blocking and timeout risk under high concurrency.
- Jobs can scale horizontally with consumer pressure, improving throughput and stability.
- SQL Server-based queueing can be reused across clouds, reducing divergence.

### Trade-Offs
- Queue semantics must be implemented and maintained in SQL (visibility timeout/leases, retries, dead-letter, cleanup) and optimized for hotspots.
- SQL Server becomes a critical dependency and requires capacity planning, pooling governance, indexing, and partitioning.

### Risks & Mitigations
- Duplicate processing (at-least-once semantics)
  - Mitigation: Use `messageId`/`correlationId` as an idempotency key; write results via “overwrite or versioned write + final pointer update”.
- Too many jobs for a single customer (noisy-neighbor / capacity monopolization)
  - Mitigation:
    - **Scheduler admission control (core)**: Before claiming a message or starting work, Timer/Job checks the customer’s “running jobs” count. If it exceeds the configured max, do not claim/start and keep the message waiting.
    - **SQL concurrency gate (recommended)**: Maintain per-customer concurrency quotas in SQL (e.g., a `CustomerConcurrency` table) with transactional/locking guarantees; release quota on job completion/failure.
    - **Customer-aware claim strategy**: Messages must include `customerId` (or a mappable key) so the Scheduler can select and defer by customer.
- SQL contention and throughput bottlenecks
  - Mitigation: Claim in batches; add appropriate indexes (e.g., `Status, AvailableAt, PartitionKey`); partition/shard tables; tune consumer concurrency and backoff.
- Stuck messages (consumer crash leaves leases held)
  - Mitigation: Implement `LeaseUntil` (visibility timeout) and `DequeueCount`; auto-reclaim after timeout; dead-letter beyond threshold.
- SAS leakage and unauthorized access
  - Mitigation: Least privilege + short TTL; bind IP/protocol where possible; audit and alert on SAS issuance and access.
- State inconsistency across Table/Blob/Queue
  - Mitigation: Accept eventual consistency; treat the SQL queue as the execution driver (source of truth for execution) and run periodic reconciliation against Table and job logs.

### Follow-Up Actions
- Define SQL queue table schema and dequeue semantics (claim/lease/retry/dead-letter/cleanup).
- Define Azure Table state machine and fields (progress, error codes, result blob path, version).
- Add monitoring and alerts (queue depth, oldest message age, failure rate, retry counts, DB CPU/lock waits).
- Run load tests and capacity planning (batch size, consumer concurrency, DB SKU, storage throughput).
- Write operational runbooks (scaling, recovery, dead-letter handling, replay strategy).

## (Optional) Stakeholder Impact
| Stakeholder | Impact | Actions / Responsibilities | Risk if Ignored |
|-------------|--------|----------------------------|-----------------|
| Product Owners | More stable API experience; async completion | Define async semantics and SLA; update product messaging | Users misinterpret “immediate completion” |
| Ops Team | New queue + job monitoring surface | Build alerts/dashboards/runbooks | Backlogs and stuck work go unnoticed |
| QA / Performance | Must validate idempotency and correctness under load | Build retry/duplicate-consumption tests and performance plans | Duplicate processing or data inconsistency |

## References
* design document: <TBD>
* JIRA ticket: <TBD>
* diagrams: <TBD>
