# Solution Design Document

## [Product Name]

**Author:** [Author Name - PTM or Dev Lead]  
**Date:** [YYYY-MM-DD]

---

## Table of Contents

- [Basic Info](#basic-info)
- [What](#what)
- [How](#how)
- [Solution Diagram](#solution-diagram)
- [Major Points](#major-points)
  - [Security](#security)
  - [Performance](#performance)
  - [Cost](#cost)
- [Notes from Review](#notes-from-review)
- [ADRs](#adrs)

---

## Basic Info

| Item | Description |
|---|---|
| Feature Name | [Short name of the feature] |
| Reviewer | [Primary reviewer name(s)] |
| QA Owner | [QA owner name] |
| Feature Jira | [Jira link] |
| Architect Review Jira | [Jira link] |
| Primary Dev / Author | [Who created the Jira and is the author of this document] |
| Architects / Reviewers | [Reviewer names] |
| Architecture Review Meeting | [Meeting link / date / notes] |

---

## What

[Briefly describe the feature you want to implement.]

Recommended content:

- What problem does this feature solve?
- What is the user/business value?
- What are the expected outcomes?
- What impact the feature has on the changed scope?
---

## How

[Briefly describe how you want to implement the feature, including all key technical points.]

Recommended content:

- Overall implementation approach
- Main components involved
- Data flow / communication flow
- API changes
- Database / storage changes
- Background jobs / async processing
- Configuration changes
- Dependency on other teams or services
- Rollout / migration strategy
- Test Suggestions for QA teams
---

## Solution Diagram

[A simple and understandable diagram is needed.]

The diagram can be one of the following:

- Flow chart
- Communication diagram
- Architecture diagram
- Sequence diagram
- Data flow diagram

```mermaid
flowchart TD
    A[Start] --> B[Component / Service]
    B --> C[Process Logic]
    C --> D[Result / Output]