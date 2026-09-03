---
name: scanner-test-development
description: Use when developing new test cases for the SharePoint Archive Job Scanner module. Guides the complete workflow from scenario identification through implementation and verification.
---

# Scanner Test Development Workflow

## Overview

This skill guides the step-by-step process of developing test cases for `ArchiverSharePointScanner` and related scanner classes. The testing pattern uses JSON-driven fake data with virtual method overriding — no mocking frameworks, no production code modifications.

## When to Use

- Adding a new test case for scanner functionality
- Implementing test coverage for a new rule type or condition
- Creating regression tests for bug fixes in the scanner module
- Expanding test scenarios for edge cases or exception handling

## Prerequisites

Before starting, ensure you have read:
1. `docs/tdd/archive-job-scanner-overview.md` — Understand the architecture
2. `docs/tdd/archive-job-data-objects.md` — Understand data objects and enums
3. `RADiscoveryUnitTest/SharePointScannerTests/README.md` — Understand the test framework

## Development Workflow

### Phase 1: Scenario Definition

1. **Identify the business requirement** being tested
2. **Define input conditions** — What tree nodes, items, rules are involved?
3. **Define expected outcomes** — What should the scanner produce?
4. **Name the test** using pattern: `MethodName_WhenCondition_ThenExpectedBehavior`

Example:
```
ProcessListAsync_WhenRuleFiltersByNameContains_OnlyMatchingItemsProcessed
```

### Phase 2: JSON Test Data Creation

Create a JSON file in `RADiscoveryUnitTest/TestData/SharePointScanner/`:

```json
{
  "ListNode": {
    "FullPath": "https://test.sharepoint.com/sites/test/Lists/TestList",
    "Title": "TestList",
    "Name": "TestList",
    "ID": "<guid>",
    "SPNodeLevel": "List",
    "CacheNodeType": 1000,
    "SiteUrl": "https://test.sharepoint.com/sites/test",
    "WebId": "<guid>",
    "ListId": "<guid>"
  },
  "Rules": [
    {
      "Id": "1",
      "Name": "Descriptive Rule Name",
      "Type": "ADMIN",
      "PolicyLevel": 64,
      "Filters": [
        {
          "SequenceNo": 1,
          "Level": 64,
          "RuleType": 8,
          "Condition": 8,
          "Value1": ".docx"
        }
      ],
      "AndOrExpression": { "64": "(1)" }
    }
  ],
  "Items": [...],
  "Folders": [...],
  "Expected": {
    "ShouldThrowException": false,
    "ExpectedProcessedItemsCount": <number>,
    "ExpectedProcessedFoldersCount": <number>
  }
}
```

**Rule design principle:** Rules and data MUST be paired in the same JSON file. Each test scenario defines its own rules that determine how the paired Items/Folders should be filtered. Never rely on hardcoded rules in test code.

Key enum values for JSON:
- `CacheNodeType`: WebApplication=0, SiteCollection=1, Web=3, List=1000, Folder=1002, Item=10000
- `ItemType`: DOCUMENT=1, DOCUMENT_VER=2, ITEM_TYPE=4, ITEM_VERSION=5, ATTACHMENT=6
- `PolicyLevel`: SiteCollection=1, Site=2, List=8, Folder=16, Item=32, Document=64
- `PolicyRuleType`: None=0, Title=4, Name=8, Size=16, CreatedTime=64
- `PolicyCondition`: Exactly=1, Contains=8, StartWith=16, Match=128, GreaterThan=256, LessThan=512

When `Rules` is null or empty in JSON, default wildcard rules (match all documents and folders) are used.

### Phase 3: Rule Design

Rules are defined in the JSON test data file (not in C# code). Design your rules to match the test scenario:

**For tests verifying that items MATCH a rule:**
- Define specific rule conditions (e.g., Name Contains ".docx")
- Provide items that satisfy those conditions
- Assert `ExpectedProcessedItemsCount` equals matching items count

**For tests verifying that items are EXCLUDED by a rule:**
- Define specific rule conditions
- Provide items that do NOT satisfy the conditions
- Assert `ExpectedProcessedItemsCount` is 0 or less than total items

**For tests verifying wildcard/no-rule behavior:**
- Omit the `Rules` field or set it to `null`
- Default wildcard rules will match all items
- Assert all items are processed

Example rule for filtering by file size > 1MB:
```json
{
  "Id": "1",
  "Name": "Large Files Only",
  "Type": "ADMIN",
  "PolicyLevel": 64,
  "Filters": [{ "SequenceNo": 1, "Level": 64, "RuleType": 16, "Condition": 256, "Value1": "1048576" }],
  "AndOrExpression": { "64": "(1)" }
}
```

### Phase 4: Test Implementation

```csharp
[TestMethod]
public async Task MethodName_WhenCondition_ThenExpectedBehavior()
{
    // Arrange
    var testDataFile = Path.Combine(_testDataDirectory, "your_scenario.json");
    var testData = LoadTestData(testDataFile);
    
    // Create scanner with fake data
    var scanner = CreateTestScanner(testData);
    
    // Act
    await scanner.TestProcessListAsync(); // or TestProcessFolderAsync, etc.
    
    // Assert
    Assert.AreEqual(
        testData.Expected.ExpectedProcessedItemsCount,
        scanner.ProcessedItemsCount);
}
```

### Phase 5: Verification

1. **Run the test**: `dotnet test --filter "FullyQualifiedName~ArchiverSharePointScannerTests.MethodName"`
2. **Verify it fails first** (Red phase) if writing test before implementation
3. **Check assertions** match expected behavior documented in the JSON
4. **Run all scanner tests** to ensure no regression: `dotnet test --filter "FullyQualifiedName~ArchiverSharePointScannerTests"`

## Common Patterns

### Testing Rule Matching

To test that a rule correctly filters items:
1. Define rules in the JSON `Rules` array with specific conditions (e.g., Name Contains ".docx")
2. Create items in JSON — some matching the rule, some not
3. Assert only matching items are processed
4. The rule and data are self-contained in the same JSON file

### Testing Exception Handling

To test exception scenarios:
1. Set `"ShouldThrowException": true` in JSON Expected section
2. Specify `"ExceptionType"` (e.g., "SPObjectLockedException")
3. The fake scanner will throw at the configured point
4. Assert the scanner handles it gracefully (continues processing other items)

### Testing Nested Folder Discovery

To test folder recursion:
1. Create nested folder structure in JSON
2. Use `ProcessFolderAsync` as the entry point
3. Assert all levels of folders are discovered
4. Verify items at each level are processed

### Testing SP Query Mode vs Client-Side Filtering

- **SP Query mode**: When rules have `HasLowLevelRule = true`, scanner uses server-side filtering
- **Client-side mode**: When no low-level rules, scanner fetches all items and filters locally
- Test both modes by configuring rules at different PolicyLevel values

## Anti-Patterns to Avoid

1. **DO NOT** modify production code to support testing
2. **DO NOT** share JSON test data files between unrelated tests
3. **DO NOT** use hard-coded expected counts without documenting why
4. **DO NOT** skip the Red phase — always see the test fail first
5. **DO NOT** test multiple independent behaviors in one test method
6. **DO NOT** use `Thread.Sleep` or timing-dependent assertions

## File Locations

| File | Purpose |
|------|---------|
| `RADiscoveryUnitTest/SharePointScannerTests/ArchiverSharePointScannerTests.cs` | Main test class |
| `RADiscoveryUnitTest/SharePointScannerTests/TestArchiverSharePointScanner.cs` | Fake scanner implementation |
| `RADiscoveryUnitTest/TestData/SharePointScanner/*.json` | Test data files |
| `RADiscoveryUnitTest/SharePointScannerTests/README.md` | Framework documentation |
| `docs/tdd/archive-job-scanner-overview.md` | System architecture reference |
| `docs/tdd/archive-job-data-objects.md` | Data objects reference |
