# Archive Job - Data Objects Reference

## 1. Overview

This document describes the key data objects (DTOs, configuration classes, and domain entities) used in the SharePoint Archive Job's Scanner module. These objects carry configuration, tree structure, rules, and scheduling information throughout the scan pipeline.

---

## 2. RMSPTreeNode

**Namespace:** `AvePoint.RA.Contract.Global.Object` (RAContract.Global) / `AvePoint.RA.Contract.Object` (RAContract)  
**File:** `RAContract.Global/Object/RMSPTreeNode.cs`

The tree node object representing a SharePoint object in the source tree browser. It is serialized into `JobContext.JobContextSetting` and deserialized at job start.

### Key Properties

| Property | Type | Description |
|---|---|---|
| `Id` | string | Unique node identifier |
| `Level` | int | Node level in tree (maps to `NodeLevel` enum) |
| `Name` | string | Node name |
| `DisplayName` | string | Display name |
| `Title` | string | Node title |
| `FullPath` | string | Full URL path of the node |
| `NodeType` | int | Type of tree node |
| `SPObjectId` | string | SharePoint object GUID |
| `FarmId` | string | Farm identifier |
| `SPType` | int | SharePoint type |
| `SPVersion` | int | SharePoint version |
| `TemplateId` | int | List template ID |
| `Parent` | RMSPTreeNode | Parent node reference |
| `Children` | List\<RMSPTreeNode\> | Child nodes |
| `BposInfo` | BposInfo | BPOS (Office 365) authentication info |
| `IncludeNew` | int | -1=N/A, 0=not include new, 1=include new |
| `CheckNumber` | int | 1=checked, 0=unchecked |
| `SiteGroupId` | Guid | Site group identifier |
| `TeamName` | string | Teams name (if Teams source) |

### Classification & Term Properties

| Property | Type | Description |
|---|---|---|
| `ColumnName` | string | Metadata column name |
| `TermStoreId` | Guid | Term store ID |
| `TermSetId` | Guid | Term set ID |
| `TermId` | Guid | Term ID |
| `TermName` | string | Term name |
| `isEnableClassification` | bool | Classification enabled flag |
| `IsCustomSetting` | bool | Has custom settings |

### Job Configuration Properties

| Property | Type | Description |
|---|---|---|
| `IsProcessApprovalDatasOnly` | bool | Only process approved data |
| `SupportLockedSite` | bool | Support locked site collections |
| `UserArchiverImportFile` | bool | Use archiver import file |
| `ApprovalType` | int | Approval type (AutoApproval, etc.) |
| `SplitScanDBInfo` | SplitScanDBInfo | Split scan database information |
| `EndUserArchiveSiteCollectionConfig` | EndUserArchiveSiteCollectionConfig | End-user archive settings |
| `SkipRemoveContentAndDestroyAction` | bool | Skip destructive actions |

### Helper Methods

- `Clone()` — Shallow copy
- `Dispose()` — Disposes children
- `GetSiteCollectionNode()` — Navigate to site collection level
- `GetGroupNode()` — Navigate to group level
- `GetTeamsNode()` — Get Teams-specific node

---

## 3. SPTreeNodeDto

**Namespace:** `AvePoint.GCommon.Contract.Tree.Object`  
**File:** `Common/GlobalCommon/GCommonContract/Tree/Object/SPTreeNodeDto.cs`

The DTO version of the tree node used in the scanner's internal processing. `RMSPTreeNode` is converted to `SPTreeNodeDto` via `RMDtoConverter.ConvertRMTree2SPTree()`.

### Key Properties

| Property | Type | Description |
|---|---|---|
| `SPObjectId` | string | SharePoint object GUID |
| `ItemRowId` | int | Item row ID |
| `FarmName` | string | Farm name |
| `AgentId` | string | Agent identifier |
| `Template` | int | List template type |
| `HasSubFolder` | bool | Has sub-folders |
| `SPVersion` | int | SharePoint version |
| `SPType` | SPType | SharePoint type enum |
| `InheritingPermissions` | bool | Inherits parent permissions |
| `SiteLockStatus` | int | Site lock state |

### Inherited from AveTreeNodeDto\<T\>

| Property | Type | Description |
|---|---|---|
| `ID` | string | Node identifier |
| `Name` | string | Node name |
| `Url` | string | Full URL |
| `Type` | NodeType | Node type (TeamChannel, SkyDriveProSites, etc.) |
| `Children` | List\<SPTreeNodeDto\> | Child nodes |
| `FullPath` | string | Full path |

### NodeType Enum Values (relevant subset)

```
SiteCollection, Site, List, Library, Folder, Item,
TeamChannel, TeamPrivateChannel, TeamSharedChannel,
SkyDriveProGroup, SkyDriveProSites, SkyDriveProSitesGroup
```

---

## 4. ScheduleConfiguration

**Namespace:** `AvePoint.RA.SharePoint.ArchiverCommon`  
**File:** `RAArchiverCommon/ScheduleConfiguration.cs`

The central configuration object carrying all job-level settings. Created during `ProcessRecordsDisposalAsync()` and passed to all scanner components.

### Core Properties

| Property | Type | Description |
|---|---|---|
| `JobId` | string | Current sub-job ID |
| `MainJobId` | string | Main job ID |
| `SiteCollectionUrl` | string | Target site collection URL |
| `SiteCollectionID` | Guid | Site collection GUID |
| `ContainerId` | Guid | Container (group) GUID |
| `RunJobNodeLevel` | int | Level where job was triggered |
| `RuleCollection` | Dictionary\<int, Rule\> | All rules for this job |
| `currentRule` | Rule | Currently processing rule |
| `actionType` | ActionType | Current action type |
| `jobtype` | JobType | Job type enum |

### Source Identification

| Property | Type | Description |
|---|---|---|
| `IsOneDriverSite` | bool | Is OneDrive site |
| `IsTeams` | bool | Is Teams site |
| `TeamsSiteNodeType` | NodeType | Teams channel type |
| `TeamsId` | string | Teams ID |
| `TeamsAddress` | string | Teams address |
| `ScopePath` | string | Scope path for the job |

### Storage & Archiver Settings

| Property | Type | Description |
|---|---|---|
| `ArchiveTemp` | string | Temp folder path for archive |
| `ScanDBName` | string | SQLite scan database file name |
| `ArchiveJobSplitedDBInfo` | ArchiveJobSplitedDBInfo | Split job info |
| `IsILMode` | bool | Information Lifecycle mode |
| `UseArchiverImportFile` | bool | Use import file |
| `SupportLockedSite` | bool | Support locked sites |

### Rule & Discovery Settings

| Property | Type | Description |
|---|---|---|
| `DiscoverWithSPQuery` | bool | Use SP CAML query optimization |
| `DiscoverWithSPQueryForVersion` | bool | SP Query for versions |
| `SkipDiscoverItemForFolderLevelRule` | bool | Skip item discovery for folder rules |
| `OneDriveNullClassification` | bool | OneDrive null classification mode |
| `ForceFitTeamsRuleID` | string | Force-fit rule for Teams |
| `AutoApprovalManualRule` | bool | Auto-approval for manual rules |
| `UseIncrementalDiscover` | bool | Use change API instead of full crawl |

### Static Config

| Property | Type | Description |
|---|---|---|
| `ListTemplate` | List\<int\> | Known list template IDs (100, 101, 103...) |
| `IsDeleteRecord` | bool | Static delete flag |

### Object Model & Services

| Property | Type | Description |
|---|---|---|
| `aveObjectModelFactory` | AveObjectModelFactory | Factory for SP object model access |
| `JobReportDto` | JobReportImps | Job report tracking |
| `ProgressDto` | JobReportImps | Progress tracking |
| `ArchiverExtendSetting` | ArchiverExtendSettingDto | Extended archiver settings |

---

## 5. ScanJobSettings

**Namespace:** `AvePoint.RA.SharePoint.Archiver`  
**File:** `RASharePoint/Archiver/Scan/ScanJobSettings.cs`

A simple container that bundles all settings needed by a scanner instance.

### Properties

| Property | Type | Description |
|---|---|---|
| `Id` | string | Main job ID |
| `SubJobId` | string | Sub-job ID |
| `TreeNode` | RMSPTreeNode | The tree node being processed |
| `Action` | ArchiverAction | Archiver action type |
| `Configuration` | ScheduleConfiguration | Full job configuration |
| `DiscoverNode` | RMDiscoverOptimizationNode | For discover optimization jobs |
| `AppProfileId` | string | Application profile ID |
| `SiteAdminUrl` | string | Site admin URL |
| `SourceDataStorageId` | string | HSM source storage |
| `DataContentStorageId` | string | HSM content storage |
| `TraceId` | string | HSM trace ID |

### Usage

Created in `ProcessRecordsDisposalAsync()`:
```csharp
ScanJobSettings scanJobSettings = new ScanJobSettings()
{
    SubJobId = JobId,
    Id = jobContext.MainJobId,
    TreeNode = treeNode,
    Configuration = mConfiguration,
};
```

---

## 6. Rule

**Namespace:** `AvePoint.RA.Contract.Global.Object`  
**File:** `RAContract.Global/Object/SORules.cs`

The rule definition object that specifies filtering criteria and archive actions.

### Core Properties

| Property | Type | Description |
|---|---|---|
| `Id` | string | Rule database ID (GUID) |
| `Name` | string | Rule display name |
| `Type` | RuleType | Rule type (NONE, ENTERPRISE, SCHEDULED, etc.) |
| `Order` | int | Execution priority order |
| `KeepDataOption` | int | Bitflags for action (DeleteOnly, KeepLatestVersion, etc.) |
| `DeleteRecords` | bool | Whether to delete source records |
| `IsManualApproval` | bool | Requires manual approval |
| `ReviewType` | ReviewType | Review workflow type |

### Filter Configuration

| Property | Type | Description |
|---|---|---|
| `PolicyLevel` | int | Target level (Document=64, Folder=16, Item=32, List=8) |
| `Filters` | List\<FilterPolicy\> | Filter policies defining match criteria |
| `SOFilters` | List\<SOFilterPolicy\> | UI-facing filter display objects |
| `AndOrExpression` | Dictionary\<int, string\> | AND/OR expression per level, e.g., `{64: "(1)and(2)"}` |

### Action & Storage

| Property | Type | Description |
|---|---|---|
| `StoragePolicyId` | string | Target storage device ID |
| `Compression` | int | Compression type |
| `Encryption` | int | Encryption type |
| `DataSecurity` | int | Combined: high 4 bits = encryption, low 4 bits = compression |
| `ExportType` | ExportTypeValue | Export format |
| `ExportInfo` | SOExportInfo | Export settings |

### Source-Specific Rules

| Property | Type | Description |
|---|---|---|
| `OneDriveRule` | Rule | OneDrive-specific rule variant |
| `TeamsRule` | Rule | Teams-specific rule variant |
| `FSRule` | Rule | File System rule |
| `SPLocalRule` | Rule | SharePoint On-Premises rule |

### Advanced Options

| Property | Type | Description |
|---|---|---|
| `IncludeNew` | string | "1" = include new items |
| `ProfileType` | ProfileType | ArchiverRule, ScheduledRule, etc. |
| `RelatedRecordOption` | RelatedRecordOption | Related records handling |
| `MoveToRecordCenterAndDelareSetting` | object | Record center settings |
| `KeepLatestMajorAndMinorVersion` | int | Versions to keep |
| `ArchiverSetting` | ArchiverSetting | Archiver-specific settings |
| `TagContentInfo` | List\<TagContentInfo\> | Tag content definitions |

### KeepDataOption Flags (Bitfield)

```
DeleteOnly                            = 0x01
ArchiverOnly                          = 0x02  
ArchiverAndRemove                     = combined
KeepLatestVersion                     = 0x04
KeepLatestVersionAndArhiveOthers      = 0x08
KeepLatestMajorAndMinorVersion        = ...
KeepLatestMajorAndMinorVersionAndArchiveOthers = ...
```

---

## 7. FilterPolicy

**Namespace:** `AvePoint.GCommon.Contract.CommonFilter`

Individual filter condition within a rule.

### Properties

| Property | Type | Description |
|---|---|---|
| `Level` | PolicyLevel | Target level (Document, Folder, Item, List, Site, SiteCollection) |
| `Condition` | PolicyCondition | Match condition (Contains, Exactly, OlderThan, etc.) |
| `Rule` | object | Rule type object (UrlRule, SizeRule, etc.) |
| `Value` | PolicyValue | Filter value(s) |
| `RuleType` | PolicyRuleType | Rule type enum |
| `SequenceNo` | int | Sequence number in AND/OR expression |

### PolicyLevel Enum

```csharp
SiteCollection = 1   // or Teams
Site           = 2
List           = 8
Folder         = 16
Item           = 32
Document       = 64
DocumentVersion= 128
ItemVersion    = 256
Attachment     = 512
Newsfeed       = 1024
```

### PolicyCondition Enum (subset)

```csharp
Exactly        = 1
Contains       = 8
StartsWith     = 16
EndsWith       = 32
OlderThan      = 64
Match          = 128   // wildcard match
GreaterThan    = 256
LessThan       = 512
Equals         = 1024
```

### PolicyRuleType Enum (subset)

```csharp
None           = 0
Title          = 4
Name           = 8
Size           = 16
CreatedTime    = 64
ModifiedTime   = 128
Url            = 256
```

---

## 8. RMRuleTermInfos

**Namespace:** `AvePoint.RA.Contract.RMRuleManageMent`  
**File:** `RAContract/RMRuleManageMent/RMRuleInfos.cs`

Associates rules with classification terms (taxonomy). Used when rules are bound to specific terms in the term store.

### Properties

| Property | Type | Description |
|---|---|---|
| `RuleName` | string | Rule display name |
| `RuleId` | string | Rule GUID |
| `TermNames` | string | Associated term names |

### Related: `RMRuleTermsDto`

| Property | Type | Description |
|---|---|---|
| `HasTerms` | bool | Whether terms exist |
| `TermsCount` | int | Number of terms |
| `Terms` | List\<RMRuleTermInfos\> | Term-rule associations |

### Context

In the `ProcessRecordsDisposalAsync()` flow for `RecordsDisposal` job type:
```csharp
rules = ScanDataCache.Instance.RulesBindingInTerms.Values.ToDictionary(v => i++);
```

Rules bound to terms are loaded from `ScanDataCache` and used as the `RuleCollection`.

---

## 9. ArchiverNodeItem

**Namespace:** `AvePoint.RA.SharePoint.ArchiverCommon`

The runtime representation of a node being scanned. Created during traversal and passed to `DiscoverNodeWorkerBase` methods.

### Key Properties

| Property | Type | Description |
|---|---|---|
| `ID` | Guid | Node GUID |
| `Name` | string | Node name |
| `Title` | string | Node title |
| `FullPath` | string | Full URL |
| `SPNodeLevel` | NodeLevel | Logical level (SiteCollection, Site, List, Folder, Item) |
| `Cache_NodeType` | int | CacheNodeType value for rule matching |
| `ItemType` | ItemType | Item type for rule dispatch |
| `DiscoverSPObject` | object | Real SP object (IAveListItem, AveDiscoverItem, etc.) |
| `Parent` | ArchiverNodeItem | Parent node |
| `IsSystemObject` | bool | System object flag |
| `RuleId` | string | Matched rule ID |
| `RuleName` | string | Matched rule name |
| `DoDelete` | bool | Should delete flag |
| `ShouldDoArchive` | bool | Should archive flag |
| `ArchiveLevel` | bool | Is at archive level |
| `RulePolicyLevel` | int | Policy level of matched rule |
| `IsInheritContainerTerm` | bool | Inherits container term |

---

## 10. Data Flow Summary

```
JobContextSetting (serialized XML)
    │
    ▼ Deserialize
RMSPTreeNode
    │
    ├── Used to build ScanJobSettings
    ├── Converted to SPTreeNodeDto (for scanner internal use)
    └── Used to initialize ScheduleConfiguration
            │
            ├── RuleCollection (Dictionary<int, Rule>)
            │       └── Each Rule has Filters (List<FilterPolicy>)
            │
            └── Various job settings (IsTeams, IsOneDrive, etc.)
                    │
                    ▼
            ArchiverSharePointScanner created with ScanJobSettings
                    │
                    ├── Creates ScanDiscovrerNodeWorker
                    │       └── Init() creates RuleManagement from RuleCollection
                    │
                    └── RunAsync() traverses tree:
                            SiteCollection → Web → List → Folder → Item
                            │
                            Each node becomes ArchiverNodeItem
                            │
                            ▼
                    DiscoverNodeWorkerBase evaluates:
                            ProcessContainerAsync() or ProcessItemAsync()
                            │
                            ▼
                    RuleManagement.CheckItemCriteria()
                            │
                            ▼
                    Results written to IScanDataReader (SQLite)
                            │
                            ▼
                    Post-scan: Archive/Delete phases read from IScanDataReader
```

---

## 11. Enum Reference: CacheNodeType

```csharp
public enum CacheNodeType
{
    WebApplication  = 0,
    SiteCollection  = 1,
    Web             = 3,
    List            = 1000,
    Folder          = 1002,
    Item            = 10000
}
```

### Usage in Rule Pruning

The `RuleManagement.RuleLevelNumber` is set to the CacheNodeType value of the deepest rule level. The scanner uses this to prune the tree:

- If current node's CacheNodeType < RuleLevelNumber → `HasLowerLevelRule` = true → continue deeper
- If current node's CacheNodeType >= RuleLevelNumber → no deeper rules exist → can prune

---

## 12. Enum Reference: ItemType

```csharp
namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    public enum ItemType
    {
        UNKNOW_TYPE    = 0,
        DOCUMENT       = 1,
        DOCUMENT_VER   = 2,
        ITEM_TYPE      = 4,
        ITEM_VERSION   = 5,
        ATTACHMENT     = 6
    }
}
```

### Mapping to PolicyLevel

| ItemType | PolicyLevel checked | RuleManagement flag |
|---|---|---|
| DOCUMENT | PolicyLevel.Document | HasDocumentCondition |
| ITEM_TYPE | PolicyLevel.Item | HasItemCondition |
| DOCUMENT_VER | PolicyLevel.DocumentVersion | HasDocVersionCondition |
| ITEM_VERSION | PolicyLevel.ItemVersion | HasItemVersionCondition |
| ATTACHMENT | PolicyLevel.Attachment | HasAttachmentCondition |

---

## 13. Enum Reference: ActionType

```csharp
public enum ActionType
{
    ArchiverAndRemove,
    ArchiverAndKeepData,
    ExportBeforeArchiver,
    BackupOnly,
    ArchchiveToStorage,
    ExportOnly,
    DeleteOnly,
    ExportBeforeDelete,
    DeleteDocumentToRecyleBinOnly,
    KeepDataOnly,
    ExportBeforeKeepDataOnly,
    ArchiveByMicrosoft,
    Move
}
```

Used in `RealRunRecordsDisposalJob()` to determine post-scan action per rule.

