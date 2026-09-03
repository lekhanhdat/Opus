# Archive Job - Scanner Module Documentation

## 1. Overview

The Archive Job (specifically the **SharePoint Archive Job** with `JobType = RMArchiverBackup`) is responsible for scanning SharePoint data sources, applying rules to identify content that meets archival criteria, and processing that content through archive, delete, and index/report phases.

This document focuses on the **Scanner** phase — the entry point, initialization, discovery, and rule-checking logic.

---

## 2. Architecture & Class Hierarchy

```
DisposalActivityManagementProcessor (Job Entry Point)
    │
    ├── ProcessRecordsDisposalAsync()     ← SP Archive Job entry
    │       ├── Deserializes RMSPTreeNode from JobContext
    │       ├── Initializes ScheduleConfiguration
    │       ├── Builds RuleCollection via GetArhiverRules()
    │       └── Creates ISharePointScanner (ArchiverSharePointScanner)
    │
    └── GetScanner() → returns concrete scanner based on JobType
            │
            ▼
ISharePointScanner (Interface)
    │
    ▼
SharePointScannerBase (Abstract Base)
    │   ├── RunAsync()                    ← Orchestrates entire scan
    │   ├── ProcessSiteCollectionAsync()
    │   ├── ProcessWebAsync()
    │   ├── ProcessListAsync()
    │   ├── ProcessFolderAsync()
    │   └── GetScanDataReader()
    │
    ▼
ArchiverSharePointScanner (Concrete for RMArchiverBackup)
    │   ├── discoverWorker property → ScanDiscovrerNodeWorker
    │   ├── ListSkipCheck()
    │   ├── ProcessListAsync() override
    │   └── SP Query optimization logic
    │
    ▼
IDiscoverNodeWorker (Interface)
    │
    ▼
DiscoverNodeWorkerBase (Base Discovery Logic)
    │   ├── Init()                        ← Initializes RuleManagement
    │   ├── ProcessContainerAsync()       ← Container-level rule checking
    │   ├── ProcessItemAsync()            ← Item-level rule checking
    │   ├── CheckItemRuleAsync()          ← Dispatches to RuleManagement
    │   ├── HasLowLevelRule()             ← Tree pruning logic
    │   └── TransmitToNextLayer()         ← Writes to approval report
    │
    ▼
ScanDiscovrerNodeWorker (Concrete - minimal overrides)
    │   └── Constructor only, delegates all to base
    │
    ▼
RuleManagement (Rule Engine)
        ├── Constructor: analyzes filters to set Has*Condition flags
        ├── CheckItemCriteria()           ← Document/Item rule checking
        ├── CheckItemVersionCriteria()    ← Version rule checking
        ├── HasLowerLevelRule()           ← Determines if deeper scan needed
        └── HaveCurrentLevelRule()        ← Determines current-level match
```

---

## 3. Job Initialization Flow

### 3.1 Entry Point: `DisposalActivityManagementProcessor`

**Namespace:** `AvePoint.RA.SharePoint.Archiver`  
**File:** `RASharePoint/Archiver/DisposalActivityManagementProcessor.cs`

This is the main job processor class. It supports multiple job types:
- `RecordsDisposal` — Records management disposal
- `RMArchiverBackup` — SharePoint archiver (primary focus)
- `OneDriveRecordsDisposal` — OneDrive disposal
- `TeamsArchiverBackup` — Teams archiver
- `RMEndUserArchiverBackup` — End-user triggered archive
- `SpecifySitesArchiverBackup` — Specific sites archive

### 3.2 `ProcessRecordsDisposalAsync()` — SP Archive Job Entry

This method is the entry point for SharePoint archive jobs. Steps:

1. **Deserialize Tree Node**: Extracts `RMSPTreeNode` from `jobContext.JobContextSetting`
2. **Set Schedule Settings**: Calls `SetScheduleSettings(treeNode)`
3. **Convert Tree**: `RMDtoConverter.ConvertRMTree2SPTree(treeNode)` → `SPTreeNodeDto`
4. **Extract Site Info**: Gets site collection URL, ID, group ID from tree
5. **Initialize Configuration**: Calls `InitSiteInfoScheduleConfigAsync(mConfiguration)`
6. **Initialize ScanDataCache**: `ScanDataCache.Instance.Initialize()`
7. **Create ScanJobSettings**: Packages `JobId`, `MainJobId`, `TreeNode`, `Configuration`
8. **Build Rule Collection**: Calls `GetArhiverRules(treeNode)` → `Dictionary<int, Rule>`
9. **Create Scanner**: `GetScanner(scanJobSettings, treeNode)` → `ArchiverSharePointScanner`
10. **Execute Scan**: `await scanner.RunAsync()`
11. **Post-Scan**: Either splits into sub-jobs or runs `RealRunRecordsDisposalJob()`

### 3.3 `GetArhiverRules(RMSPTreeNode node)`

Retrieves the rules applied to the current tree node:
1. Calls `GetAppliedRuleIds(node)` to find rule GUIDs bound to the tree node
2. Fetches full `Rule` objects from the rule service
3. For OneDrive sites, uses `rule.OneDriveRule`
4. For Teams, uses `rule.TeamsRule`
5. Returns `List<Rule>` which is converted to `Dictionary<int, Rule>`

---

## 4. Scanner Execution Flow

### 4.1 `SharePointScannerBase.RunAsync()`

**Namespace:** `AvePoint.RA.SharePoint.Archiver`  
**File:** `RASharePoint/Archiver/Scan/Base/SharePointScannerBase.cs`

The base scanner orchestrates the scan by:

1. Converting tree node to `RuleNodeContract`
2. Calling `discoverWorker.Init(ruleNode)` — initializes the `RuleManagement` engine
3. Creating `ArchiverNodeItem` from the rule node config
4. Calculating list count for progress reporting
5. Dispatching to the appropriate level handler based on `SPNodeLevel`:
   - `NodeLevel.SiteCollection` → `ProcessSiteCollectionAsync()`
   - `NodeLevel.Site` → `ProcessWebAsync()`
   - `NodeLevel.List` / `NodeLevel.Library` → `ProcessListAsync()`
   - `NodeLevel.Folder` → `ProcessFolderAsync()`
6. Flushing dependency objects and finalizing

### 4.2 `ArchiverSharePointScanner`

**Namespace:** `AvePoint.RA.SharePoint.Archiver`  
**File:** `RASharePoint/Archiver/Scan/ArchiverSharePointScanner.cs`

Key constructor logic:
- `CheckNeedDiscoverBySPQuery()` — Determines if SP Query optimization is applicable
- `CheckNeedDiscoverBySPQueryForVersion()` — SP Query for version rules
- `CheckHasLastAccessTimeRule()` — Detects LastAccessTime rules

Key properties:
- `discoverWorker` → Lazy-creates `ScanDiscovrerNodeWorker`
- `CAMLManager` — For SP Query (CAML) optimization
- `RuleItemCollection` — Cached rule items for SP Query mode

---

## 5. Discovery & Rule Checking

### 5.1 `DiscoverNodeWorkerBase`

**Namespace:** `AvePoint.RA.SharePoint.Archiver.Scan.Implement`  
**File:** `RASharePoint/Archiver/Scan/Implement/DiscoverNodeWorkerBase.cs`

This is the core business logic class for node discovery and rule evaluation.

#### Constructor
```csharp
public DiscoverNodeWorkerBase(ScanJobSettings jobSettings, 
    ScheduleConfiguration paraConfig, 
    IBackwardDependencyNodeCache<object> dependencyObjs)
```
- Stores references to `ScanJobSettings`, `ScheduleConfiguration`, and dependency cache
- Initializes `mApprovalReportProxy` for writing scan results
- Sets `systemListTable` from `ScheduleConfiguration.ListTemplate`

#### `Init(object obj)`
- Receives `RuleNodeContract` with break-inherit node info
- **Creates `RuleManagement`** from `config.RuleCollection`
- Sets `ForceFitTeamsRuleID` on the rule engine

#### `ProcessContainerAsync(ArchiverNodeItem item, ProcessType type)`

Handles container-level nodes (SiteCollection, Web, List, Folder). Decision flow:

| CacheNodeType | Logic |
|---|---|
| **List (1000)** | 1. Skip if system list → 2. Check rule fit → 3. Check `HasLowLevelRule` → 4. Check list type rule |
| **SiteCollection (1)** | 1. Check rule fit → 2. Check `HasLowLevelRule` |
| **Web (3)** | 1. Check rule fit → 2. Check both current + lower level rules → 3. If only current level, skip list nodes |
| **Folder (1002)** | Same as default: check rule fit → check current + lower level → prune if no lower rules |

Returns `ProcessResult`:
- `Default` — Continue processing children
- `FitRule` — Node fits a rule, stop deeper scan
- `SkipCurrentNode` — Skip this node entirely
- `SkipListNode` — Skip list-level processing

#### `ProcessItemAsync(ArchiverNodeItem item, ArchiverNodeItem parent)`

Delegates to `RealProcessItemAsync()`:
1. If parent has a rule with `DoDelete`, inherit rule to item
2. If system item, skip
3. Call `CheckItemRuleAsync(item)` — the core rule evaluation
4. Process check result
5. `TransmitToNextLayer(item)` — write to approval report

#### `CheckItemRuleAsync(ArchiverNodeItem item)`

Routes to appropriate rule checking based on `item.ItemType`:

| ItemType | Condition Flag | Method Called |
|---|---|---|
| `DOCUMENT (1)` | `HasDocumentCondition` | `mRuleEngine.CheckItemCriteria()` |
| `ITEM_TYPE (4)` | `HasItemCondition` | `mRuleEngine.CheckItemCriteria()` |
| `DOCUMENT_VER (2)` | `HasDocVersionCondition` | `mRuleEngine.CheckItemVersionCriteria()` |
| `ITEM_VERSION (5)` | `HasItemVersionCondition` | `mRuleEngine.CheckItemVersionCriteria()` |
| `ATTACHMENT (6)` | `HasAttachmentCondition` | `mRuleEngine.CheckAttachmentCriteria()` |
| default | — | **Throws exception** |

> **CRITICAL:** If `ItemType` is not set (UNKNOW_TYPE = 0), the default case throws:  
> `"StorageOptimization_SOARScanDiscoverNodeWorkerInitItemLevelNodeWithRule"`

#### Tree Pruning Methods

```csharp
internal bool HasLowLevelRule(ArchiverNodeItem item)
{
    return mRuleEngine.HasLowerLevelRule((int)item.Cache_NodeType);
}

internal bool HasCurrentLevelRule(ArchiverNodeItem item)
{
    return mRuleEngine.HaveCurrentLevelRule((int)item.Cache_NodeType);
}
```

These delegate to `RuleManagement` which compares the node's `CacheNodeType` value against `RuleLevelNumber`.

### 5.2 `ScanDiscovrerNodeWorker`

**Namespace:** `AvePoint.RA.SharePoint.Archiver.Scan.Implement`  
**File:** `RASharePoint/Archiver/Scan/Implement/ScanDiscoverNodeWorker.cs`

A minimal concrete class that inherits from `DiscoverNodeWorkerBase`:
```csharp
class ScanDiscovrerNodeWorker : DiscoverNodeWorkerBase
{
    public ScanDiscovrerNodeWorker(ScanJobSettings jobSettings, 
        ScheduleConfiguration paraConfig, 
        IBackwardDependencyNodeCache<object> dependencyObjs, 
        bool justEstimateListCount)
        : base(jobSettings, paraConfig, dependencyObjs)
    { }
}
```

All business logic is inherited from `DiscoverNodeWorkerBase`. No overrides.

---

## 6. Rule Management Engine

### 6.1 `RuleManagement`

**Namespace:** `AvePoint.RA.SharePoint.Discover`  
**File:** `RASharePoint/Discover/RuleManagement.cs`

The rule engine that evaluates whether items/containers match configured rules.

#### Constructor: `RuleManagement(Dictionary<int, Rule> Rules)`

Iterates all rules and their `Filters` to set condition flags:

```csharp
// For each rule's Filters:
HasAttachmentCondition |= (filter.Level == PolicyLevel.Attachment)
HasDocumentCondition   |= (filter.Level == PolicyLevel.Document)
HasFolderCondition     |= (filter.Level == PolicyLevel.Folder)
HasDocVersionCondition |= (filter.Level == PolicyLevel.DocumentVersion)
HasItemVersionCondition|= (filter.Level == PolicyLevel.ItemVersion)
HasItemCondition       |= (filter.Level == PolicyLevel.Item || filter.Level == PolicyLevel.Newsfeed)
HasListCondition       |= (filter.Level == PolicyLevel.List)
HasSiteCondition       |= (filter.Level == PolicyLevel.Site)
HasSiteCollectionCondition |= (filter.Level == PolicyLevel.SiteCollection || filter.Level == PolicyLevel.Teams)
```

Then sets `RuleLevelNumber` (the deepest rule level):
```
Priority (highest first):
  Item/Attachment/DocVersion/ItemVersion/Document → CacheNodeType.Item (10000)
  Folder                                         → CacheNodeType.Folder (1002)
  List                                           → CacheNodeType.List (1000)
  Site                                           → CacheNodeType.Web (3)
  SiteCollection                                 → CacheNodeType.SiteCollection (1)
```

#### `HasLowerLevelRule(int cacheNodeType)`

```csharp
public bool HasLowerLevelRule(int cacheNodeType)
{
    return cacheNodeType < RuleLevelNumber;
}
```

Example: If `RuleLevelNumber = 10000` (Item) and current node is `List (1000)`, returns `true` — deeper scanning is needed.

#### `HaveCurrentLevelRule(int cacheNodeType)`

```csharp
public bool HaveCurrentLevelRule(int cacheNodeType)
{
    if (cacheNodeType > SiteCollection && cacheNodeType < List) → return RuleLevelNumber == Web
    if (cacheNodeType > List && cacheNodeType < Item)           → return RuleLevelNumber == Folder
    return RuleLevelNumber == cacheNodeType;
}
```

#### `CheckItemCriteria(Guid docId, object oItem)`

1. Extracts `IAveListItem` from the discover object
2. Builds `ObjectInfoBase` (DocumentInfo or ItemInfo) from the list item
3. Applies filter policies to extract field values (Name, Size, CreatedTime, etc.)
4. Calls `CheckCriteria(baseInfo)` which evaluates the AND/OR expression

#### Key Properties

| Property | Type | Description |
|---|---|---|
| `HasDocumentCondition` | bool | Any rule has Document-level filter |
| `HasItemCondition` | bool | Any rule has Item-level filter |
| `HasFolderCondition` | bool | Any rule has Folder-level filter |
| `HasListCondition` | bool | Any rule has List-level filter |
| `HasSiteCondition` | bool | Any rule has Site-level filter |
| `HasSiteCollectionCondition` | bool | Any rule has SiteCollection-level filter |
| `RuleLevelNumber` | int | Deepest rule level (CacheNodeType value) |
| `FilterPolicyCollection` | List\<FilterPolicy\> | Merged filters from all rules |

---

## 7. CacheNodeType Hierarchy

```
WebApplication  = 0
SiteCollection  = 1
Web             = 3
List            = 1000
Folder          = 1002
Item            = 10000
```

These values are critical for the tree-pruning logic. The scanner traverses from high-level (SiteCollection) down. At each level, it checks whether rules exist at a lower level to decide if deeper traversal is needed.

---

## 8. ItemType Enumeration

**Namespace:** `AvePoint.RA.SharePoint.ArchiverCommon`

```
UNKNOW_TYPE    = 0  (default — causes exception in CheckItemRuleAsync)
DOCUMENT       = 1  (files in document libraries)
DOCUMENT_VER   = 2  (file versions)
ITEM_TYPE      = 4  (list items in generic lists)
ITEM_VERSION   = 5  (list item versions)
ATTACHMENT     = 6  (item attachments)
```

> **Important:** Every item node MUST have `ItemType` set before entering `CheckItemRuleAsync()`. Unset values trigger the default exception case.

---

## 9. Sequence Diagram: Scan Flow

```
DisposalActivityManagementProcessor
    │
    │ ProcessRecordsDisposalAsync()
    │   ├── Deserialize RMSPTreeNode
    │   ├── Build RuleCollection
    │   └── Create ArchiverSharePointScanner
    │
    ▼
ArchiverSharePointScanner.RunAsync()
    │
    │ (via SharePointScannerBase)
    │   ├── Convert tree to RuleNodeContract
    │   ├── discoverWorker.Init(ruleNode)  ← Creates RuleManagement
    │   └── Switch on SPNodeLevel
    │
    ▼
ProcessSiteCollectionAsync()
    │   └── discoverWorker.ProcessContainerAsync(siteItem)
    │           ├── Check rule fit at SC level
    │           ├── HasLowLevelRule? → continue to webs
    │           └── ProcessWebAsync() for each sub-web
    │
    ▼
ProcessWebAsync()
    │   └── discoverWorker.ProcessContainerAsync(webItem)
    │           ├── Check rule fit at Web level
    │           ├── HasLowLevelRule? → continue to lists
    │           └── ProcessListAsync() for each list
    │
    ▼
ProcessListAsync()
    │   ├── ListSkipCheck() — system/design list filtering
    │   └── discoverWorker.ProcessContainerAsync(listItem)
    │           ├── Check rule fit at List level
    │           ├── HasLowLevelRule? → continue to folders/items
    │           └── ProcessFolderAsync() / process items
    │
    ▼
ProcessItemsAndSubfoldersAsync()
    │   └── For each item:
    │       └── discoverWorker.ProcessItemAsync(item, parent)
    │               ├── CheckItemRuleAsync(item)
    │               │       └── Switch on item.ItemType
    │               │           ├── DOCUMENT → CheckItemCriteria()
    │               │           ├── ITEM_TYPE → CheckItemCriteria()
    │               │           └── ...
    │               └── TransmitToNextLayer(item)
    │                       └── Write to ScanDataReader (SQLite DB)
    ▼
scanner.GetScanDataReader() → IScanDataReader
    └── Used by RealRunRecordsDisposalJob for archive/delete
```

---

## 10. SP Query Optimization

When rules are simple enough (single document-level name/size filter), the scanner can use SharePoint CAML queries instead of downloading all items:

- `CheckNeedDiscoverBySPQuery()` — Evaluates if optimization is possible
- `DiscoverWithSPQuery` flag on `ScheduleConfiguration`
- Uses `CAMLManager` to build and execute CAML queries
- Falls back to full discovery if optimization conditions are not met

Conditions that **prevent** SP Query optimization:
- Multiple rule levels (e.g., both Folder and Document rules)
- Complex AND/OR expressions
- Rules using metadata not queryable via CAML

---

## 11. Key Configuration Objects

See [archive-job-data-objects.md](./archive-job-data-objects.md) for detailed object documentation.

---

## 12. Error Handling Patterns

| Error | Cause | Handling |
|---|---|---|
| `StorageOptimization_SOARScanDiscoverNodeWorkerInitItemLevelNodeWithRule` | `ItemType` not set on node | Exception thrown in `CheckItemRuleAsync` default case |
| `JobStopException` | User stopped the job | Re-thrown, not caught |
| `AveSkipLockSiteException` | Site is locked | Job skips the site |
| `SPObjectNotFoundException` | Site/Web/List not found | Reported and skipped |
| `AveExceedStorageLimitException` | Site exceeds quota | Job stops for this site |

---

## 13. Testing Considerations

When writing unit tests for the scanner logic:

1. **RuleManagement initialization**: Must provide `Rule` objects with proper `Filters` containing `FilterPolicy` entries with correct `Level` values
2. **ItemType**: Must be set on every item node (use `ItemType.DOCUMENT` for document library items)
3. **CacheNodeType**: Must match the hierarchy values (List=1000, Folder=1002, Item=10000)
4. **DiscoverSPObject**: Required on items for `CheckItemCriteria()` to work (needs `IAveListItem`)
5. **HasLowerLevelRule**: Depends on `RuleLevelNumber` set during `RuleManagement` construction — ensure Document-level filters exist if testing item scanning
6. **SP Query mode**: Adding Folder-level rules prevents SP Query optimization, ensuring full discovery path is tested

