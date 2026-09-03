# Architecture Decision Record Template

**Number:** 0002 
**Title:** Log Collector: Auto collect agent logs
**Status:** Proposed 
**Approval Date:** 2026-04-01  
**Owner:** Lambert Shen  
**Reviewers:** <list of reviewers>  

## Context
### Problem
- Currently, Agent logs are stored locally on the user's device, which hinders our ability to effectively analyze and troubleshoot customer issues. Therefore, we plan to implement a log collection and upload logic.

### Current State
- Log retrieval is a manual process that involves contacting Support and requesting the customer to supply the log files.

### Goals
1. Primary goal: Implement a log collector to collect and upload logs, and provide users with an option to control log collection permissions.

### Constraints & Considerations
* Security
  - Strictly restrict SAS permissions, allowing only upload operations, limit writable file paths or file names, and control the validity period.
  - Perform virus scanning before download to prevent malicious files from entering storage.
  - Consider implementing an active collection mechanism to avoid excessive irrelevant logs caused by passive collection, improving both efficiency and security.

### Options Considered
**Option 1: Real-Time Structured Logging (e.g., Azure Application Insights)**
Summary: Streams structured logs directly to an analytics platform (e.g., Application Insights) for proactive monitoring and powerful querying.  
Pros:  
- High real-time visibility; excellent built-in querying and analytics capabilities (e.g., using KQL); seamless offline buffering.
Cons:  
- Higher data ingestion and retention costs compared to basic Blob Storage.

**Option 2: Periodic Batch Upload to Azure Blob Storage**  
Summary: Periodically batches and uploads local log files to Azure Blob Storage.
Pros:  
- Extremely cost-effective and simple to implement.
Cons:  
- Lacks real-time visibility and makes log querying highly inefficient.

## Decision

When finalized, summarize the decision in the following table:

| Item                   | Description |
|------------------------|-------------|
| Chosen Solution        | 2 |
| Rationale              | Prioritizes cost and simplicity. Solves manual log retrieval without the overhead of real-time streaming. |
| Rejected Alternatives  | Option 1 (Real-Time Logging) rejected; high ingestion costs outweigh our need for real-time data. |
| Key Assumptions        | Delayed logs are acceptable for Support. Users must have a UI toggle to opt in/out for privacy. |
| Implementation Outline | 1. UI opt-in toggle & local rotation. 2. Batch logic & Azure Blob integration. |
| Approval               | Date + approvers (arch group, product, ops) |

## Consequences

Include subsections:
Positive Impacts:
- UX improvement: Completely eliminates the manual, back-and-forth communication required to retrieve logs from customers.
- Performance gains: Minimal impact on the client application, as periodic batching, zipping, and uploading run as low-priority background tasks.
- Simplified filtering/search: Centralizes all client logs in Azure, providing Support with immediate, self-serve access to diagnostic files.

Trade-Offs:
- Query Complexity: Searching through raw, zipped text files in Blob Storage is highly inefficient compared to querying a structured analytics platform.
- Delayed Visibility (Latency): Logs are not available in real-time; Support must wait for the next periodic batch cycle or a manual trigger to view recent errors.

Risks & Mitigations:
- Client Resource Spikes: Zipping massive log files might temporarily spike user CPU/Disk I/O.
  - Mitigation: Enforce strict local log rotation (e.g., max 50MB), limit zip sizes, and run tasks on background threads.

- Upload Failures: Network interruptions could cause incomplete uploads.
  - Mitigation: Implement robust retry logic and resume capabilities using Azure Blob SAS tokens.

- Privacy/Compliance: Accidentally uploading logs for users who declined tracking.
  - Mitigation: Hardcode the opt-in/opt-out permission check as a strict gatekeeper immediately before the upload function executes.

Follow-Up Actions:
- Instrumentation & Monitoring: Add basic telemetry to track upload success/failure rates and average payload sizes.
- Documentation Updates: Create a runbook for the Support team detailing how to securely access and download logs from Azure Blob Storage.
- Load Tests & QA: Test the upload pipeline under poor network conditions and with maximum-allowed log file sizes.
- Schedule Next ADR Review: Review in 6 months to assess if manually parsing Blob files is too inefficient for Support (evaluating a potential future upgrade to structured logging).

## References
* design document <Link to design document>
* JIRA ticket: https://jira.avepoint.net/browse/RECO-35667
* diagrams<Link to diagrams / prototypes>
* legacy versions<Link to legacy version ADR>

---
Guidance:
- Keep language concise and factual.
- Update Status as the decision progresses.
- If superseded, link forward/backward to related ADRs.
- Prefer bullets over long paragraphs.
