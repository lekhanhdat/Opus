# Architecture Decision Record Template

**Number:** 00023
**Title:** Auto upgrade agent
**Status:** Proposed 
**Approval Date:** 2026-04-01  
**Owner:** Lambert Shen  
**Reviewers:** <list of reviewers>  

## Context
### Problem
- This manual process is highly inefficient and unscalable. When users need to manage and update a large fleet of Agents, downloading and installing updates individually becomes a significant operational burden and a major time sink.

### Current State
- Currently, whenever Opus releases a new version of the Agent, users are required to manually download the installation package and execute the installation process on their local machines.

### Goals
1. Primary goal: To resolve this bottleneck, we plan to implement an automated upgrade framework. The primary objective is to streamline the update process: when a new Agent version is available, users should be able to trigger the upgrade seamlessly by simply clicking an "Upgrade" button directly within the Agent Management page.

### Constraints & Considerations
* Security
  - Access Control: Strict RBAC is required to restrict who can trigger upgrades.
* Operational Limitations
  - irewalls: Customers must whitelist required ports.
* Technology Constraints
  - Resources: Requires sufficient local disk and memory to handle the update package.

### Options Considered
**Option 1: Centralized Push Upgrade**
Summary: Users manually trigger agent upgrades via a dashboard button.  
Pros:  
- High Control: Users choose the timing to avoid business disruption.
= Targeted Rollout: Allows for phased updates (e.g., canary testing).
Cons:  
- Manual Effort: Still requires an admin to initiate the process.
- Version Fragmentation: Legacy versions remain if users choose not to upgrade.

**Option 2: Background Auto-Update**  
Summary: Agents silently poll the server and automatically install updates in the background.
Pros:  
- Zero Maintenance: Fully automated with no user intervention required.
- Unified Versions: Ensures all agents run the latest, most secure release.
Cons:  
- Low Control: Unexpected restarts or downloads may disrupt user tasks.
- Complex Rollbacks: Requires a robust auto-rollback mechanism to prevent fleet-wide outages from a bad update.

## Decision

When finalized, summarize the decision in the following table:

| Item                   | Description |
|------------------------|-------------|
| Chosen Solution        | 1 |
| Rationale              | Prioritizes user control to prevent unexpected downtime. Enables phased rollouts. |
| Rejected Alternatives  | Option 2 (Auto-Update) rejected due to disruption risks and complex rollback requirements. |
| Key Assumptions        | Admins will manually trigger updates. Support can manage a mix of legacy versions. |
| Implementation Outline | 1. UI upgrade button & version display. 2. Backend push mechanism. 3. Upgrade status tracking. |
| Approval               | Date + approvers (arch group, product, ops) |

## Consequences

Include subsections:
Positive Impacts:
- User Control: Prevents business disruption by letting users choose the upgrade time.
- Safer Deployments: Allows for targeted, phased rollouts (canary testing).
- Clear Visibility: Dashboard UI clearly shows which agents need updates.

Trade-Offs:
- Manual Effort: Requires an administrator to log in and click to upgrade.
- Version Fragmentation: Support must handle legacy versions if users ignore updates.

Risks & Mitigations:
- Installation Failures: Corrupted downloads or partial installs.
  - Mitigation: Implement pre-installation health checks and automatic local rollbacks.

Follow-Up Actions:
- Telemetry: Track upgrade success/failure rates and global version adoption speed.
- Documentation: Create user guides on how to manage fleet upgrades via the UI.
- Testing: Load test the backend push mechanism with thousands of concurrent agents.

## References
* design document <Link to design document>
* JIRA ticket: https://jira.avepoint.net/browse/RECO-35664
* diagrams<Link to diagrams / prototypes>
* legacy versions<Link to legacy version ADR>

---
Guidance:
- Keep language concise and factual.
- Update Status as the decision progresses.
- If superseded, link forward/backward to related ADRs.
- Prefer bullets over long paragraphs.
