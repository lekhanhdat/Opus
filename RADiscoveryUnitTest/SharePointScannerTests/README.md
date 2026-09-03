# SharePoint Scanner JSON-Based Testing Framework

## 概述

这个测试框架实现了完全基于 JSON 数据驱动的 SharePoint 扫描器测试，**不修改任何生产代码**，通过重写所有 virtual 方法实现 fake discover 业务逻辑。

---

## 测试规划

### 测试目标

验证 `ArchiverSharePointScanner` 在各种条件下的扫描逻辑正确性，包括：
- 节点发现与遍历
- Rule 规则评估与过滤
- 异常场景处理
- SP Query 优化路径

### 测试点清单

#### 1. Scanner 初始化

| 编号 | 测试点 | 描述 | 优先级 |
|------|--------|------|--------|
| INIT-01 | ScheduleConfiguration 反序列化 | 从 JobContext 正确解析配置 | 高 |
| INIT-02 | Rule 集合初始化 | GetArhiverRules 正确构建规则列表 | 高 |
| INIT-03 | Tree 节点解析 | RMSPTreeNode → SPTreeNodeDto 转换 | 高 |
| INIT-04 | 空配置处理 | 缺失配置字段时的默认行为 | 中 |

#### 2. Rule 规则评估

| 编号 | 测试点 | 描述 | 优先级 |
|------|--------|------|--------|
| RULE-01 | PolicyLevel 匹配 | SiteCollection/Site/List/Folder/Item/Document 各级别 | 高 |
| RULE-02 | Name - Exactly 匹配 | 文件名精确匹配 | 高 |
| RULE-03 | Name - Contains 匹配 | 文件名包含匹配 | 高 |
| RULE-04 | Name - StartWith 匹配 | 文件名前缀匹配 | 中 |
| RULE-05 | Size - GreaterThan | 文件大小大于阈值 | 高 |
| RULE-06 | Size - LessThan | 文件大小小于阈值 | 高 |
| RULE-07 | CreatedTime 匹配 | 创建时间条件过滤 | 中 |
| RULE-08 | 多条件组合 (AND) | 多个规则同时满足 | 高 |
| RULE-09 | 多条件组合 (OR) | 满足任一规则即可 | 中 |
| RULE-10 | 无规则场景 | 无 Rule 时处理所有数据 | 高 |
| RULE-11 | HasLowLevelRule 检测 | 是否触发 SP Query 优化 | 高 |

#### 3. 节点发现与遍历

| 编号 | 测试点 | 描述 | 优先级 |
|------|--------|------|--------|
| DISC-01 | List 级别处理 | ProcessListAsync 正确处理列表 | 高 |
| DISC-02 | Folder 级别处理 | ProcessFolderAsync 正确处理文件夹 | 高 |
| DISC-03 | 嵌套文件夹递归 | 多层文件夹的递归发现 | 高 |
| DISC-04 | 空列表处理 | Items 和 Folders 为空时 | 高 |
| DISC-05 | 大批量数据 | 批处理逻辑 (IEnumerable<List<T>>) | 中 |
| DISC-06 | Item 类型分类 | DOCUMENT/ITEM_TYPE/ATTACHMENT 正确分类 | 高 |

#### 4. Item 处理

| 编号 | 测试点 | 描述 | 优先级 |
|------|--------|------|--------|
| ITEM-01 | 文档项处理 | 带版本和附件的文档 | 高 |
| ITEM-02 | 文件夹项处理 | 文件夹类型的 Item | 中 |
| ITEM-03 | 匹配项过滤 | 只处理符合 Rule 的项 | 高 |
| ITEM-04 | 不匹配项跳过 | 不符合 Rule 的项被跳过 | 高 |
| ITEM-05 | 版本处理 | ProcessVersionsAsync 正确处理 | 中 |
| ITEM-06 | 附件处理 | ProcessAttachmentsAsync 正确处理 | 中 |

#### 5. 异常处理

| 编号 | 测试点 | 描述 | 优先级 |
|------|--------|------|--------|
| EXC-01 | SPObjectLockedException | 对象被锁定时跳过并报告 | 高 |
| EXC-02 | SPObjectNotFoundException | 对象不存在时跳过并报告 | 高 |
| EXC-03 | SPObjectReadOnlyException | 对象只读时跳过并报告 | 中 |
| EXC-04 | 通用异常 | 未知异常的优雅处理 | 高 |
| EXC-05 | 部分失败继续 | 某项失败后继续处理其他项 | 高 |

#### 6. SP Query 优化

| 编号 | 测试点 | 描述 | 优先级 |
|------|--------|------|--------|
| SPQ-01 | SP Query 模式触发 | HasLowLevelRule=true 时使用服务端过滤 | 高 |
| SPQ-02 | 客户端过滤模式 | 无低级规则时获取全部数据后过滤 | 高 |
| SPQ-03 | 混合模式 | 同时有高级和低级规则时的行为 | 中 |

### 开发优先级

1. **P0 (必须)**: INIT-01~03, RULE-01~02, RULE-05~06, RULE-10~11, DISC-01~04, DISC-06, ITEM-01, ITEM-03~04, EXC-01~02, EXC-04~05, SPQ-01~02
2. **P1 (重要)**: RULE-03, RULE-08, ITEM-05~06, EXC-03
3. **P2 (增强)**: INIT-04, RULE-04, RULE-07, RULE-09, DISC-05, ITEM-02, SPQ-03

### 相关文档

- 架构文档：`docs/tdd/archive-job-scanner-overview.md`
- 数据对象文档：`docs/tdd/archive-job-data-objects.md`
- Agent 定义：`.github/agents/scanner-test-developer.agent.md`
- 开发流程 Skill：`.github/skills/scanner-test-development/SKILL.md`
- 文档支持 Skill：`.github/skills/scanner-test-documentation/SKILL.md`

---

## 架构设计

### 核心组件

1. **TestArchiverSharePointScanner**
   - 继承自 `ArchiverSharePointScanner`
   - 重写所有 virtual 方法
   - 使用 JSON 数据替代真实 SharePoint 对象

2. **FakeDiscoverFolder**
   - 继承自 `AveDiscoverFolder`
   - `GetItemsWithStructureForArchiver()` 返回从 JSON 加载的项目
   - `GetFoldersWithStructure()` 返回从 JSON 加载的文件夹

3. **ScannerTestData**
   - 从 JSON 文件加载的测试数据结构
   - 定义节点参数、项目、文件夹和预期行为

## JSON 数据结构

### 完整示例

```json
{
  "ListNode": {
    "FullPath": "https://test.sharepoint.com/sites/test/Lists/TestList",
    "Title": "TestList",
    "Name": "TestList",
    "ID": "12345678-1234-1234-1234-123456789012",
    "SPNodeLevel": "List",
    "CacheNodeType": 1000,
    "SiteUrl": "https://test.sharepoint.com/sites/test",
    "WebId": "22345678-1234-1234-1234-123456789012",
    "ListId": "32345678-1234-1234-1234-123456789012"
  },
  "FolderNode": {
    "FullPath": "https://test.sharepoint.com/sites/test/Lists/TestList/TestFolder",
    "Title": "TestFolder",
    "Name": "TestFolder",
    "ID": "42345678-1234-1234-1234-123456789012",
    "SPNodeLevel": "Folder",
    "CacheNodeType": 1002,
    "SiteUrl": "https://test.sharepoint.com/sites/test",
    "WebId": "22345678-1234-1234-1234-123456789012",
    "ListId": "32345678-1234-1234-1234-123456789012"
  },
  "Rules": [
    {
      "Id": "1",
      "Name": "Filter by Name Contains docx",
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
      "AndOrExpression": {
        "64": "(1)"
      }
    },
    {
      "Id": "2",
      "Name": "Match All Folders",
      "Type": "ADMIN",
      "PolicyLevel": 16,
      "Filters": [
        {
          "SequenceNo": 1,
          "Level": 16,
          "RuleType": 8,
          "Condition": 8,
          "Value1": "*"
        }
      ],
      "AndOrExpression": {
        "16": "(1)"
      }
    }
  ],
  "Items": [
    {
      "DocID": "52345678-1234-1234-1234-123456789012",
      "FullUrl": "https://test.sharepoint.com/sites/test/Lists/TestList/Document.docx",
      "LeafName": "Document.docx",
      "ID": 101,
      "Uiversion": 512,
      "Attachments": [
        {
          "Name": "Attachment.txt",
          "Url": "https://test.sharepoint.com/sites/test/Lists/TestList/Attachments/101/Attachment.txt"
        }
      ],
      "Versions": [
        { "Uiversion": 256 },
        { "Uiversion": 512 }
      ]
    }
  ],
  "Folders": [
    {
      "UniqueId": "62345678-1234-1234-1234-123456789012",
      "FullUrl": "https://test.sharepoint.com/sites/test/Lists/TestList/SubFolder",
      "ItemName": "SubFolder",
      "ID": 201,
      "Uiversion": 0,
      "Attachments": [],
      "Versions": []
    }
  ],
  "Expected": {
    "ShouldThrowException": false,
    "ExceptionType": null,
    "ExceptionMessage": null,
    "ExpectedProcessedItemsCount": 1,
    "ExpectedProcessedFoldersCount": 1,
    "ExpectedReportedPaths": [],
    "ShouldReportFailure": false
  }
}
```

### Rules 字段说明

`Rules` 数组中的每个元素定义一条 Rule，与 fake 数据一一对应，用于控制 scanner 的过滤逻辑。

| 字段 | 类型 | 描述 |
|------|------|------|
| `Id` | string | Rule 唯一标识 |
| `Name` | string | Rule 名称 |
| `Type` | string | Rule 类型：`ADMIN`, `MANUAL` |
| `PolicyLevel` | int | Rule 适用的级别（Document=64, Folder=16, Item=32, List=8, Site=2, SiteCollection=1） |
| `Filters` | array | FilterPolicy 列表，定义具体过滤条件 |
| `AndOrExpression` | object | 可选，Key 为 PolicyLevel int 值，Value 为表达式如 `"(1)"` 或 `"(1)AND(2)"` |

#### FilterPolicy 字段

| 字段 | 类型 | 描述 |
|------|------|------|
| `SequenceNo` | int | 序号，用于 AndOrExpression 引用 |
| `Level` | int | PolicyLevel 枚举值 |
| `RuleType` | int | PolicyRuleType：None=0, Title=4, Name=8, Size=16, CreatedTime=64 |
| `Condition` | int | PolicyCondition：Exactly=1, Contains=8, StartWith=16, Match=128, GreaterThan=256, LessThan=512 |
| `Value1` | string | 匹配值（如文件名模式、大小阈值），`"*"` 表示匹配所有 |
| `Value2` | string | 可选，第二个匹配值（如日期范围） |

#### 规则加载机制

- 当 JSON 中 `Rules` 为 `null` 或空数组时，使用默认通配规则（匹配所有文档和文件夹）
- 当 JSON 中 `Rules` 有值时，从 JSON 动态构建 `RuleCollection`
- 每个测试场景的 Rules 与 Items/Folders 数据配对，确保测试逻辑的一致性

#### 常用 Rule 模板

**匹配所有文档（通配）：**
```json
{
  "Id": "1",
  "Name": "Match All Documents",
  "Type": "ADMIN",
  "PolicyLevel": 64,
  "Filters": [{ "SequenceNo": 1, "Level": 64, "RuleType": 8, "Condition": 8, "Value1": "*" }]
}
```

**按文件名包含过滤：**
```json
{
  "Id": "1",
  "Name": "Filter by Name Contains",
  "Type": "ADMIN",
  "PolicyLevel": 64,
  "Filters": [{ "SequenceNo": 1, "Level": 64, "RuleType": 8, "Condition": 8, "Value1": ".docx" }],
  "AndOrExpression": { "64": "(1)" }
}
```

**按文件大小过滤（大于 1MB）：**
```json
{
  "Id": "1",
  "Name": "Filter by Size Greater Than 1MB",
  "Type": "ADMIN",
  "PolicyLevel": 64,
  "Filters": [{ "SequenceNo": 1, "Level": 64, "RuleType": 16, "Condition": 256, "Value1": "1048576" }],
  "AndOrExpression": { "64": "(1)" }
}
```

**多条件组合（AND）：**
```json
{
  "Id": "1",
  "Name": "Name Contains docx AND Size Greater Than 100KB",
  "Type": "ADMIN",
  "PolicyLevel": 64,
  "Filters": [
    { "SequenceNo": 1, "Level": 64, "RuleType": 8, "Condition": 8, "Value1": ".docx" },
    { "SequenceNo": 2, "Level": 64, "RuleType": 16, "Condition": 256, "Value1": "102400" }
  ],
  "AndOrExpression": { "64": "(1)AND(2)" }
}
```

## 测试场景示例

### 1. 测试不完整数据处理

**文件**: `incomplete_list_data.json`
- 空的 Items 和 Folders 列表
- 验证扫描器能正确处理空数据源

### 2. 测试混合内容处理

**文件**: `mixed_folder_content.json`
- 包含 3 个项目（文档）
- 包含 2 个子文件夹
- 验证扫描器能正确处理和计数

### 3. 测试异常处理

**文件**: `exception_in_processing.json`
- 模拟处理过程中的异常
- 验证扫描器继续处理其他项目

### 4. 测试特定异常类型

```json
{
  "Expected": {
    "ShouldThrowException": true,
    "ExceptionType": "SPObjectLockedException",
    "ExceptionMessage": "List is locked",
    ...
  }
}
```

支持的异常类型：
- `SPObjectLockedException`
- `SPObjectNotFoundException`
- `SPObjectReadOnlyException`
- 通用 `Exception`

### 5. 测试 Rule 过滤逻辑

**文件**: `rule_filter_by_name.json`
- 定义 Rule：Name Contains ".docx"
- Items 包含 .docx 和 .xlsx 文件
- 验证只有匹配 Rule 的项被处理
- Rules 与 Items 数据配对验证过滤正确性

## 使用方法

### 1. 创建测试数据文件

在 `RADiscoveryUnitTest/TestData/SharePointScanner/` 目录下创建 JSON 文件：

```bash
TestData/
└── SharePointScanner/
    ├── incomplete_list_data.json
    ├── mixed_folder_content.json
    ├── exception_in_processing.json
    ├── rule_filter_by_name.json
    └── your_custom_test.json
```

### 2. 编写测试用例

```csharp
[TestMethod]
public async Task YourCustomTest()
{
    // Arrange
    var testDataFile = Path.Combine(_testDataDirectory, "your_custom_test.json");
    var testData = LoadTestData(testDataFile);
    var scanner = new TestArchiverSharePointScanner(testData);

    // Act
    await scanner.TestProcessListAsync();

    // Assert
    Assert.AreEqual(testData.Expected.ExpectedProcessedItemsCount, scanner.ProcessedItemsCount);
    Assert.AreEqual(testData.Expected.ExpectedProcessedFoldersCount, scanner.ProcessedFoldersCount);
}
```

### 3. 运行测试

```powershell
dotnet test --filter "FullyQualifiedName~ArchiverSharePointScannerTests"
```

## 重写的 Virtual 方法

TestArchiverSharePointScanner 重写了以下所有 virtual 方法：

- ✅ `RunAsync()`
- ✅ `LoadBreakInheritNodeUrls()`
- ✅ `ProcessSiteCollectionAsync()`
- ✅ `ProcessWebAsync()`
- ✅ `ProcessListAsync()` - 从 JSON 加载参数
- ✅ `ProcessFolderAsync()` - 从 JSON 加载参数
- ✅ `ProcessItemAsync()`
- ✅ `ProcessItemsAndSubfoldersAsync()` - fake `GetItemsWithStructureForArchiver()` 和 `GetFoldersWithStructure()`
- ✅ `InitialSPObjectInfoAsync()`
- ✅ `ProcessVersionAndAttachmentsAsync()`
- ✅ `ProcessVersionsAsync()`
- ✅ `ProcessAttachmentsAsync()`

## 优势

1. **无需修改生产代码** - 所有 fake 逻辑都在测试项目中
2. **数据驱动** - 通过 JSON 控制测试场景
3. **易于扩展** - 添加新测试只需创建新的 JSON 文件
4. **隔离性好** - 测试完全独立于 SharePoint 环境
5. **可复现** - 相同的 JSON 产生相同的测试结果

## 测试覆盖的场景

- ✅ 数据不完整
- ✅ 混合内容（项目 + 文件夹）
- ✅ 处理过程中的异常
- ✅ 特定异常类型（锁定、未找到、只读）
- ✅ Job 业务流程验证
- ✅ 数据源控制测试

## 扩展指南

### 添加新的测试场景

1. 创建新的 JSON 文件
2. 定义节点参数、项目、文件夹
3. 设置预期行为
4. 编写测试方法加载该 JSON 文件

### 添加新的异常类型

在 `CreateExceptionFromExpectedBehavior` 方法中添加新的 case：

```csharp
private Exception CreateExceptionFromExpectedBehavior(ExpectedBehavior expected)
{
    return expected.ExceptionType switch
    {
        "YourNewException" => new YourNewException(expected.ExceptionMessage),
        // ... existing cases
    };
}
```

### 自定义 Fake 对象

在 `FakeDiscoverFolder` 中可以添加更复杂的逻辑来模拟不同的数据源行为。

## 注意事项

1. **Guid 解析** - JSON 中的 ID 字段使用字符串格式，会被解析为 Guid
2. **枚举解析** - `SPNodeLevel` 和 `CacheNodeType` 使用字符串或整数
3. **空数据** - 空列表和 null 都会被正确处理
4. **批处理** - Items 和 Folders 使用 `IEnumerable<List<T>>` 模拟批处理

## 相关文件

- `ArchiverSharePointScannerTests.cs` - 主测试文件
- `TestData/SharePointScanner/*.json` - 测试数据文件
- `README.md` - 本文档
