

你是一个Archive job的专家，你现在要根据下面的信息，生成job的说明文档。

===========================================

## 下面是详细的context，来帮助你了解前因后果：

DisposalActivityManagementProcessor是一个job的具体实现类，将符合条件的数据进行Archive。
下面以sharepoint数据源进行举例：
运行job的操作流程：
有一个Tree browser，左侧是数据源的tree结构，右侧是setting，设置rule等信息
左侧：
|ContianerA
	|-- Site Collection A
	|-- Site Collection B
				|----Root Site
						|--- Lists
								|---Library A
								|---Library B
										|--- Folder A
										|--- Folder B
				|----Sub Sites
						|----Sub SiteA
						|----Sub SiteB
								|--- Lists
										|---Library A
										|---Library B
												|--- Folder A
												|--- Folder B
右侧：
Option
Setting
Schedule
当用户左侧选择Tree中对应的数据源对象，右侧设置setting后，就可以运行Archive job。

Archive job业务流程包含：

1. Scan 判定那些数据需要进行archive处理。这其中还包含discover，
   根据指定的数据源（例如SharePoint，Onedrive， Teams），根据 Tree中选择的数据类型进行扫描，来获取到具体的数据，
   并根据rule的设置， rule支持各种数据源，各种level（例如：site collection， site， List， folder）
   设定不同的条件，例如：
   1.1.根据name 进行匹配，支持各种方式，例如：等于，包含，开始于。
   1.2.根据size，进行匹配，支持大于，小于
   根据rule来判定数据是否需要进行处理。
2. Archive 将数据进行压缩加密等处理，将content存储到指定的Media介质中。
   暂时不提供具体业务描述
3. Delete：删除成功archive的数据。
   暂时不提供具体业务描述
4. Index& report：生成index，和report信息。
   暂时不提供具体业务描述

下面以sharepoint数据源继续举例，介绍代码的主要类和主要对象：
sharepoint archive job的JobType是RMArchiverBackup

需要生成说明文档的功能类：

# DisposalActivityManagementProcessor：

是这一类job 入口， 支持多种job类型（例如RecordsDisposal，RMArchiverBackup， OneDriveRecordsDisposal），

ProcessRecordsDisposalAsync：是Sharepoint archive job 的入口，并初始化configuration，setting，和rule等信息
GetArhiverRules：根据tree对象初始化rule的集合

# ArchiverSharePointScanner  ： Scan 功能的具体实现类，基类是SharePointScannerBase

# ScanDiscovrerNodeWorker ：Discovery 数据的功能类， 基类是DiscoverNodeWorkerBase

# RuleManagement ： 初始化rule的信息，并包含当前 job中rule的功能方法。

需要生成说明文档的对象类：

# RMSPTreeNode ： tree中 sharepoint 对象的基本信息

# SPTreeNodeDto ： tree中 sharepoint 对象的Dto对象

# ScheduleConfiguration ： job 的configuration

# ScanJobSettings ： SP tree对象的setting 中的信息

# RMRuleTermInfos ： rule关联的term 对象

# Rule ： rule对象类

===============================================================

## 任务目标：

1.将上面提到的功能类和对象类，生成对象的说明文档，以便后续Agent在实现Testing业务逻辑时能快速的了解业务逻辑。

2.目前只生成和Scanner相关的业务逻辑，至少包含上面的功能类和对象类的详细解释

## 文档要求：

1.生成的文档位于docs\tdd
2.你可以生成一个文档，但是当一个文档内容过多时，你也可以生成多个，根据功能命名。
3.这个文档中要包含这个job的各种重要信息， 架构，层次，初始化，业务流程等等

4.这个文档要使用英文生成，不能出现其他语言。
