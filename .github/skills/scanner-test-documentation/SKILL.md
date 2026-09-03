---
name: scanner-test-documentation
description: Use when looking up test documentation, understanding available reference materials, or checking support content for the SharePoint Scanner test module
---

# Scanner Test Documentation & Support Content

## Overview

This skill provides a guide to all documentation and support content available for developing and maintaining tests for the SharePoint Archive Job Scanner module.

## When to Use

- You need to understand the system architecture before writing tests
- You need to look up enum values, property types, or class relationships
- You need to find existing test examples or patterns
- You are onboarding to the scanner test module for the first time
- You need to verify business rules or expected behaviors

## Documentation Map

### Architecture & Business Logic

| Document | Path | Content |
|----------|------|---------|
| Scanner Overview | `docs/tdd/archive-job-scanner-overview.md` | Class hierarchy, initialization flow, scan sequence diagram, rule management, SP Query optimization, error handling patterns |
| Data Objects Reference | `docs/tdd/archive-job-data-objects.md` | All DTOs (RMSPTreeNode, SPTreeNodeDto, ScheduleConfiguration, ScanJobSettings, Rule, FilterPolicy, RMRuleTermInfos, ArchiverNodeItem), all enums with values |

### Test Framework

| Document | Path | Content |
|----------|------|---------|
| Test Framework README | `RADiscoveryUnitTest/SharePointScannerTests/README.md` | JSON structure schema (including Rules), test scenarios, usage instructions, virtual method list, extension guide |

### Source Code References

| File | Path | Purpose |
|------|------|---------|
| Test Class | `RADiscoveryUnitTest/SharePointScannerTests/ArchiverSharePointScannerTests.cs` | Main test methods, setup, data loading |
| Fake Scanner | `RADiscoveryUnitTest/SharePointScannerTests/TestArchiverSharePointScanner.cs` | Virtual method overrides, fake data injection, rule loading from JSON |
| Rule Data DTO | `RADiscoveryUnitTest/SharePointScannerTests/RuleData.cs` | JSON-serializable rule/filter policy definitions |
| Scanner Test Data | `RADiscoveryUnitTest/SharePointScannerTests/ScannerTestData.cs` | Root test data structure (includes Rules property) |
| Test Data | `RADiscoveryUnitTest/TestData/SharePointScanner/*.json` | JSON-based test scenarios with paired rules and data |

### Production Code (Read-Only Reference)

| Component | Project | Key Files |
|-----------|---------|-----------|
| Job Entry Point | RAScheduleJob | `DisposalActivityManagementProcessor.cs` |
| Scanner Implementation | RADiscovery | `ArchiverSharePointScanner.cs` |
| Scanner Base | RADiscovery | `SharePointScannerBase.cs` |
| Discovery Worker | RADiscovery | `ScanDiscovrerNodeWorker.cs` |
| Rule Management | RAArchiverCommon | `RuleManagement.cs` |
| Tree Node | RAContract.Global | `RMSPTreeNode.cs` |
| Configuration | RAArchiverCommon | `ScheduleConfiguration.cs` |
| Settings | RAArchiverCommon | `ScanJobSettings.cs` |

## Quick Reference: Key Enums

### CacheNodeType
```
WebApplication = 0
SiteCollection = 1
Web = 3
List = 1000
Folder = 1002
Item = 10000
```

### ItemType (AvePoint.RA.SharePoint.ArchiverCommon.ItemType)
```
UNKNOW_TYPE = 0
DOCUMENT = 1
DOCUMENT_VER = 2
ITEM_TYPE = 4
ITEM_VERSION = 5
ATTACHMENT = 6
```

### PolicyLevel (Flags enum)
```
SiteCollection = 1
Site = 2
List = 8
Folder = 16
Item = 32
Document = 64
```

### PolicyCondition
```
Exactly = 1
Contains = 8
StartWith = 16
Match = 128
GreaterThan = 256
LessThan = 512
```

### PolicyRuleType
```
None = 0
Title = 4
Name = 8
Size = 16
CreatedTime = 64
```

## Support Content Checklist

When developing tests, verify you have access to:

- [ ] `docs/tdd/archive-job-scanner-overview.md` exists and is up to date
- [ ] `docs/tdd/archive-job-data-objects.md` exists and is up to date
- [ ] `RADiscoveryUnitTest/SharePointScannerTests/README.md` matches current test framework
- [ ] Test data directory exists: `RADiscoveryUnitTest/TestData/SharePointScanner/`
- [ ] JSON schema in README matches actual test data files
- [ ] All enum values in docs match production code

## Troubleshooting Guide

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| `CS0103: ItemType does not exist` | Missing namespace | Use full namespace: `AvePoint.RA.SharePoint.ArchiverCommon.ItemType.DOCUMENT` |
| `HasLowLevelRule returns false` | FilterPolicy.Level doesn't include Document/Folder | Set Level to `PolicyLevel.Document \| PolicyLevel.Folder` |
| Rule not matching items | Wrong PolicyCondition or PolicyRuleType | Check enum values in data objects doc |
| Scanner skips items | CacheNodeType mismatch in JSON | Verify CacheNodeType values match expected node types |
| Test data not found | Wrong path to JSON file | Ensure file is in `TestData/SharePointScanner/` and marked as Content/CopyAlways |

### How to Diagnose Rule Evaluation

1. Check `FilterPolicy.Level` includes the node type being processed
2. Check `RuleCollection` contains rules matching the item's properties
3. Check `PolicyCondition` operator is appropriate for the value type
4. Check `PolicyRuleType` matches the property being evaluated
5. Enable debug logging in test to trace rule evaluation path

## Updating Documentation

When the scanner module changes:

1. Update `docs/tdd/archive-job-scanner-overview.md` if class hierarchy or flow changes
2. Update `docs/tdd/archive-job-data-objects.md` if DTOs or enums change
3. Update `RADiscoveryUnitTest/SharePointScannerTests/README.md` if test framework changes
4. Add new JSON test data files for new scenarios
5. Update this skill if new documentation is added
