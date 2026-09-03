using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.StorageOptimization.Schedule.Archiver;
using AvePoint.Wrapper.Common;
using RAArchiverCommon;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using PolicyLevel = AvePoint.GCommon.Contract.CommonFilter.PolicyLevel;

namespace RADiscoveryUnitTest.SharePointScannerTests
{
    /// <summary>
    /// Test implementation of ArchiverSharePointScanner that overrides ALL virtual methods
    /// and uses JSON-based fake data sources instead of real SharePoint objects.
    /// This implements fake discover business logic by controlling data sources.
    /// </summary>
    public class TestArchiverSharePointScanner : ArchiverSharePointScanner
    {
        private readonly ScannerTestData _testData;
        private readonly FakeDiscoverFolder _fakeRootFolder;
        private readonly IBackwardDependencyNodeCache<object> _dependencyObjs;
        public FakeReportManager FakeReportManager { get; }
        public FakeDiscoverNodeWorker FakeDiscoverWorker { get; }
        
        public int ProcessedItemsCount { get; private set; }
        public int ProcessedFoldersCount { get; private set; }
        public int ProcessedVersionsCount { get; private set; }
        public int ProcessedAttachmentsCount { get; private set; }
        public List<Exception> ExceptionsCaught { get; } = new();

        /// <summary>
        /// Context stack for nested folder data. When processing sub-folders recursively,
        /// the current folder's SubItems/SubFolders are pushed onto this stack so that
        /// ProcessItemsAndSubfoldersAsync knows which data to read.
        /// </summary>
        private readonly Stack<DiscoverFolderData> _folderContextStack = new();

        public TestArchiverSharePointScanner(ScannerTestData testData) 
            : base(CreateScanJobSettings(testData))
        {
            _testData = testData;
            _fakeRootFolder = new FakeDiscoverFolder(testData);
            FakeReportManager = new FakeReportManager();
            
            // Create real dependency cache for discover worker
            _dependencyObjs = new BackwardDependenceNodeCache<object>();
            
            // Create discover worker with real dependencies - this will test real business logic
            var jobSettings = CreateScanJobSettings(testData);
            FakeDiscoverWorker = new FakeDiscoverNodeWorker(
                jobSettings, 
                jobSettings.Configuration, 
                _dependencyObjs
            );
            
            // Initialize discover worker with real rule engine from DiscoverNodeWorkerBase
            // This calls the real Init method which sets up RuleManagement and break inherit nodes
            var ruleNode = CreateFakeRuleNodeContract();
            FakeDiscoverWorker.Init(ruleNode);
        }

        private static ScanJobSettings CreateScanJobSettings(ScannerTestData testData)
        {
            var jobId = "SO20260515170609101223"; //It can change to your real job id.
            return new ScanJobSettings
            {
                TreeNode = new AvePoint.RA.Contract.Object.RMSPTreeNode
                {
                    SPObjectId = testData.ListNode?.ID ?? Guid.NewGuid().ToString(),
                    Level = (int)ParseNodeLevel(testData.ListNode?.SPNodeLevel ?? "List"),
                    Name = testData.ListNode?.Name ?? "TestNode",
                    FullPath = testData.ListNode?.FullPath ?? "https://test.sharepoint.com/test"
                },
                Configuration = CreateFakeScheduleConfiguration(jobId, testData.Rules)
            };
        }

        /// <summary>
        /// Build RuleCollection from JSON-defined rules.
        /// When rules are null/empty, uses default wildcard rules that match all documents/folders.
        /// </summary>
        private static Dictionary<int, Rule> BuildRuleCollectionFromData(List<RuleData>? rulesData)
        {
            if (rulesData == null || rulesData.Count == 0)
            {
                return BuildDefaultWildcardRules();
            }

            var ruleCollection = new Dictionary<int, Rule>();
            for (int i = 0; i < rulesData.Count; i++)
            {
                var ruleData = rulesData[i];
                var rule = new Rule
                {
                    Id = ruleData.Id ?? (i + 1).ToString(),
                    Name = ruleData.Name ?? $"Test Rule {i + 1}",
                    PolicyLevel = (PolicyLevel)ruleData.PolicyLevel,
                    Type = Enum.TryParse<RuleType>(ruleData.Type, out var ruleType) ? ruleType : RuleType.ADMIN,
                    Filters = BuildFilterPolicies(ruleData.Filters),
                    AndOrExpression = BuildAndOrExpression(ruleData)
                };
                ruleCollection[i + 1] = rule;
            }
            return ruleCollection;
        }

        private static List<FilterPolicy> BuildFilterPolicies(List<FilterPolicyData>? filtersData)
        {
            var filters = new List<FilterPolicy>();
            if (filtersData == null) return filters;

            foreach (var filterData in filtersData)
            {
                filters.Add(new FilterPolicy
                {
                    SequenceNo = filterData.SequenceNo,
                    Level = (PolicyLevel)filterData.Level,
                    RuleType = (PolicyRuleType)filterData.RuleType,
                    Rule = new NameRule { Value1 = filterData.Value1 ?? "*" },
                    Condition = (PolicyCondition)filterData.Condition,
                    Value = new PolicyValue
                    {
                        Value1 = filterData.Value1 ?? "*",
                    }
                });
            }
            return filters;
        }

        private static Dictionary<PolicyLevel, string> BuildAndOrExpression(RuleData ruleData)
        {
            var expression = new Dictionary<PolicyLevel, string>();
            if (ruleData.AndOrExpression != null)
            {
                foreach (var kvp in ruleData.AndOrExpression)
                {
                    expression[(PolicyLevel)kvp.Key] = kvp.Value;
                }
            }
            else if (ruleData.Filters != null && ruleData.Filters.Count > 0)
            {
                // Auto-generate simple AND expression from filters
                var level = (PolicyLevel)ruleData.PolicyLevel;
                var parts = new List<string>();
                foreach (var f in ruleData.Filters)
                {
                    parts.Add($"({f.SequenceNo})");
                }
                expression[level] = string.Join("AND", parts);
            }
            return expression;
        }

        /// <summary>
        /// Default wildcard rules that match all documents and folders.
        /// Used when JSON test data does not specify custom rules.
        /// </summary>
        private static Dictionary<int, Rule> BuildDefaultWildcardRules()
        {
            return new Dictionary<int, Rule>
            {
                {
                    1, new Rule
                    {
                        Id = "1",
                        Name = "Default Document Rule",
                        PolicyLevel = PolicyLevel.Document,
                        Type = RuleType.ADMIN,
                        Filters = new List<FilterPolicy>
                        {
                            new FilterPolicy
                            {
                                SequenceNo = 1,
                                Level = PolicyLevel.Document,
                                RuleType = PolicyRuleType.Name,
                                Rule = new NameRule { Value1 = "*" },
                                Condition = PolicyCondition.Contains,
                                Value = new PolicyValue { Value1 = "*" }
                            }
                        },
                        AndOrExpression = new Dictionary<PolicyLevel, string>
                        {
                            { PolicyLevel.Document, "(1)" }
                        }
                    }
                },
                {
                    2, new Rule
                    {
                        Id = "2",
                        Name = "Default Folder Rule",
                        PolicyLevel = PolicyLevel.Folder,
                        Type = RuleType.ADMIN,
                        Filters = new List<FilterPolicy>
                        {
                            new FilterPolicy
                            {
                                SequenceNo = 1,
                                Level = PolicyLevel.Folder,
                                RuleType = PolicyRuleType.Name,
                                Rule = new NameRule { Value1 = "*" },
                                Condition = PolicyCondition.Contains,
                                Value = new PolicyValue { Value1 = "*" }
                            }
                        },
                        AndOrExpression = new Dictionary<PolicyLevel, string>
                        {
                            { PolicyLevel.Folder, "(1)" }
                        }
                    }
                }
            };
        }

        private static ScheduleConfiguration CreateFakeScheduleConfiguration(string jobId, List<RuleData>? rulesData)
        {
            var ruleCollection = BuildRuleCollectionFromData(rulesData);

            try
            {
                // Try to create real ScheduleConfiguration with rules loaded from JSON
                var config = new ScheduleConfiguration(jobId, true);
                config.RuleCollection = ruleCollection;
                return config;
            }
            catch (Exception)
            {
                // If real ScheduleConfiguration fails, create a minimal fake configuration
                // by bypassing the constructor and using reflection to set required fields
                var config = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ScheduleConfiguration)) as ScheduleConfiguration;
                
                // Set minimal required properties using reflection
                var jobIdProperty = typeof(ScheduleConfiguration).GetProperty("JobId");
                if (jobIdProperty != null)
                {
                    jobIdProperty.SetValue(config, jobId);
                }
                
                // Set RuleCollection from JSON-loaded rules
                config!.RuleCollection = ruleCollection;
                
                // Initialize IsRelativeDataJob field
                var isRelativeDataJobField = typeof(ScheduleConfiguration).GetField("IsRelativeDataJob", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (isRelativeDataJobField != null)
                {
                    isRelativeDataJobField.SetValue(config, false);
                }
                
                // Initialize ArchiveJobSplitedDBInfo property
                var archiveJobSplitedDBInfoProperty = typeof(ScheduleConfiguration).GetProperty("ArchiveJobSplitedDBInfo");
                if (archiveJobSplitedDBInfoProperty != null)
                {
                    var splitedDBInfo = new AvePoint.RA.SharePoint.ArchiverCommon.ArchiveJobSplitedDBInfo
                    {
                        IsNeedSplit = false,
                        IsUseSplitedDB = false,
                        IsLatestSplitedDB = false
                    };
                    archiveJobSplitedDBInfoProperty.SetValue(config, splitedDBInfo);
                }
                var backgroundSettings = BackgroundSettings.GetInstance();
                WrapperConfiguration.RecordsOutputStreamLevel = (int)backgroundSettings.RecordsOutputStreamLevel;
                WrapperConfiguration.ArchiverOutputStreamLevel = (int)backgroundSettings.ArchiverOutputStreamLevel;
                config.ArchiveTemp = backgroundSettings.ArchiveTemp;
                if (!System.IO.Directory.Exists(config.ArchiveTemp))
                {
                    Directory.CreateDirectory(config.ArchiveTemp);
                }
                config.ArchiverUNCTime = DateTime.UtcNow;
                config.ScanDBName = string.Format("scan.{0}.db", Guid.NewGuid().ToString());
                #region wrapper config
                WrapperConfiguration.WrapperConfigurationForBPOS.LoadRootFolderUniqueId = true;
                WrapperConfiguration.WrapperConfigurationForBPOS.SetUserAgent(Office365UserAgentGenerator.Create(ModuleUserAgent.Archive, false));
                #endregion
                return config!;
            }
        }

        private static NodeLevel ParseNodeLevel(string level)
        {
            return Enum.TryParse<NodeLevel>(level, out var result) ? result : NodeLevel.List;
        }

        // Override abstract properties
        public override IDiscoverNodeWorker discoverWorker
        {
            get => FakeDiscoverWorker;
            set { }
        }

        public override bool ListSkipCheck(ArchiverNodeItem list)
        {
            return false;
        }

        // Override ALL virtual methods to implement fake discover logic

        public override async Task RunAsync()
        {
            // Simplified version for testing
            await Task.CompletedTask;
        }

        public override List<string> LoadBreakInheritNodeUrls(string scopeUrl, string siteObjectId)
        {
            return new List<string>();
        }

        public override async Task ProcessSiteCollectionAsync(ArchiverNodeItem sitecollection)
        {
            // For testing, we typically start at list level
            await Task.CompletedTask;
        }

        public override async Task ProcessWebAsync(ArchiverNodeItem web, bool needInitInfo = false)
        {
            // For testing, we typically start at list level
            await Task.CompletedTask;
        }

        /// <summary>
        /// Override ProcessListAsync to use fake data instead of real AveDiscoverList.GetRootFolder()
        /// but keep the real business logic flow: check skip, init info, process container, then process folder
        /// </summary>
        public override async Task ProcessListAsync(ArchiverNodeItem list, bool needInitInfo = false)
        {
            var logger = RALogger.GetInstance(typeof(TestArchiverSharePointScanner));
            logger.Info("Begin process list,title is:{0}.", list.Title);
            
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.ProcessList"))
            {
                try
                {
                    // Real business logic: Check if list should be skipped
                    if (ListSkipCheck(list))
                    {
                        return;
                    }

                    // Real business logic: Initialize SP object info if needed
                    if (needInitInfo)
                    {
                        await InitialSPObjectInfoAsync(FakeDiscoverWorker, list);
                    }

                    // Real business logic: Process container
                    if ((await FakeDiscoverWorker.ProcessContainerAsync(list, ProcessType.NeedProcess)) == ProcessResult.SkipCurrentNode)
                    {
                        return;
                    }

                    // FAKE DATA: Instead of calling (list.DiscoverSPObject as AveDiscoverList).GetRootFolder(true)
                    // which requires real SharePoint objects, we use our fake folder
                    var rootFolder = GetFakeRootFolder();
                    
                    // Real business logic: Generate folder node and process it
                    // Note: GenerateFolderNodeItem requires real objects we don't have, so we create node manually
                    var folderNode = new ArchiverNodeItem
                    {
                        FullPath = list.FullPath + "/RootFolder",
                        Title = "RootFolder",
                        Name = "RootFolder",
                        SPNodeLevel = NodeLevel.RootFolder,
                        Cache_NodeType = (int)CacheNodeType.Folder,
                        DiscoverSPObject = rootFolder
                    };
                    
                    // Real business logic: Call ProcessFolderAsync (which we've also overridden)
                    await ProcessFolderAsync(folderNode);
                }
                catch (Exception e)
                {
                    logger.Error("An unexpected error occurred while processing list node.Path:{0}.Message:{1}.", list.FullPath, e.ToString());
                    // In real code, would update mConfiguration.JobReportDto, but we skip for testing
                    throw;
                }
            }
        }

        /// <summary>
        /// ProcessItemsAndSubfoldersAsync - Override ONLY to fake data source (GetItemsWithStructureForArchiver/GetFoldersWithStructure)
        /// but keep the real business logic structure including exception handling and continue-on-error pattern.
        /// Mirrors the real base class flow:
        ///   Items: foreach item → ProcessItemAsync → process attachments → process versions
        ///   Folders: foreach folder → ProcessContainerAsync → folder attachments → folder versions → recursive ProcessItemsAndSubfoldersAsync
        /// </summary>
        public override async Task ProcessItemsAndSubfoldersAsync(ArchiverNodeItem folderNode, int folderLevel, List<int>? itemIDs = null, bool needInitInfo = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverScan.RealProcessItemsAndSubfolders"))
            {
                var logger = RALogger.GetInstance(typeof(TestArchiverSharePointScanner));

                // Determine data source: if we have a folder context on the stack, use it; otherwise use top-level test data
                List<DiscoverItemData> itemDataList;
                List<DiscoverFolderData> folderDataList;
                if (_folderContextStack.Count > 0)
                {
                    var currentContext = _folderContextStack.Peek();
                    itemDataList = currentContext.SubItems ?? new List<DiscoverItemData>();
                    folderDataList = currentContext.SubFolders ?? new List<DiscoverFolderData>();
                }
                else
                {
                    itemDataList = _testData.Items ?? new List<DiscoverItemData>();
                    folderDataList = _testData.Folders ?? new List<DiscoverFolderData>();
                }

                #region process items/documents
                try
                {
                    foreach (var itemData in itemDataList)
                    {
                        try
                        {
                            if (itemData.ShouldThrowException)
                            {
                                var ex = new InvalidOperationException(
                                    itemData.ExceptionMessage ?? $"Simulated exception for item: {itemData.LeafName}");
                                ExceptionsCaught.Add(ex);
                                logger.Error("An error occurred while RealProcessItemsAndSubfolders.Item:{0}.Message:{1}.",
                                    itemData.FullUrl, ex.ToString());
                                continue;
                            }

                            // Create item node for real rule evaluation (mirrors GenerateItemNodeItem)
                            var itemNode = new ArchiverNodeItem
                            {
                                FullPath = itemData.FullUrl,
                                Title = itemData.LeafName,
                                Name = itemData.LeafName,
                                SPNodeLevel = NodeLevel.Item,
                                Cache_NodeType = (int)CacheNodeType.Item,
                                ItemType = ParseItemType(itemData.ItemType),
                                IsSystemObject = itemData.IsSystemObject,
                                DiscoverSPObject = CreateFakeListItemFromData(itemData)
                            };

                            // Real business logic: ProcessItemAsync (current version)
                            ProcessResult result = await FakeDiscoverWorker.ProcessItemAsync(itemNode, folderNode);
                            ProcessedItemsCount++;

                            if (result == ProcessResult.CurrentVersionHasApprove)
                            {
                                continue;
                            }

                            // Process attachments (mirrors real ProcessVersionAndAttachmentsAsync)
                            if (itemData.Attachments != null && itemData.Attachments.Count > 0)
                            {
                                foreach (var attachment in itemData.Attachments)
                                {
                                    await ProcessItemAttachmentAsync(folderNode, itemNode, attachment, itemData);
                                }
                            }

                            // Process versions (mirrors real ProcessVersionAndAttachmentsAsync)
                            if (itemData.Versions != null && itemData.Versions.Count > 1)
                            {
                                foreach (var version in itemData.Versions)
                                {
                                    // Skip current UIVersion and version 0 (same as real code)
                                    if (version.Uiversion == itemData.Uiversion || version.Uiversion == 0)
                                    {
                                        continue;
                                    }
                                    try
                                    {
                                        await ProcessItemVersionAsync(itemNode, version, folderNode);
                                    }
                                    catch (Exception ex) when (ex.GetType().Name != "JobStopException")
                                    {
                                        ExceptionsCaught.Add(ex);
                                        logger.Error("ProcessItemVersionsError:{0}", ex.ToString());
                                    }
                                }
                            }
                        }
                        catch (Exception ex) when (ex.GetType().Name != "JobStopException")
                        {
                            ExceptionsCaught.Add(ex);
                            logger.Error("Error in Backup Single Item :{0}.ItemFullPath:{1}.", ex.ToString(), itemData.FullUrl);
                        }
                    }
                }
                catch (Exception ex) when (ex.GetType().Name != "JobStopException")
                {
                    logger.Error("An error occurred while RealProcessItemsAndSubfolders.Path:{0}.Message:{1}.",
                        folderNode.FullPath, ex.ToString());
                    ExceptionsCaught.Add(ex);
                }
                #endregion

                #region process folders
                try
                {
                    foreach (var folderData in folderDataList)
                    {
                        try
                        {
                            if (folderData.ShouldThrowException)
                            {
                                ThrowSimulatedException(folderData.ExceptionType, folderData.ExceptionMessage, folderData.FullUrl);
                            }

                            // Create folder node (mirrors GenerateFolderNodeItem)
                            var subFolderNode = new ArchiverNodeItem
                            {
                                FullPath = folderData.FullUrl ?? folderNode.FullPath + "/" + folderData.ItemName,
                                Title = folderData.ItemName ?? "Folder",
                                Name = folderData.ItemName ?? "Folder",
                                SPNodeLevel = NodeLevel.Folder,
                                Cache_NodeType = (int)CacheNodeType.Folder,
                                IsSystemObject = folderData.IsSystemObject,
                                Parent = folderNode
                            };

                            // Real business logic: ProcessContainerAsync for folder-level rule evaluation
                            ProcessResult result = await FakeDiscoverWorker.ProcessContainerAsync(subFolderNode, ProcessType.NeedProcess);
                            if (result == ProcessResult.SkipCurrentNode)
                            {
                                continue;
                            }

                            ProcessedFoldersCount++;

                            // Process folder attachments (mirrors real ProcessDataAsync for folders)
                            if (folderData.Attachments != null && folderData.Attachments.Count > 0)
                            {
                                foreach (var attachment in folderData.Attachments)
                                {
                                    await ProcessFolderAttachmentAsync(folderNode, subFolderNode, attachment);
                                }
                            }

                            // Process folder versions (mirrors real ProcessFolderVersionsAsync)
                            if (folderData.Versions != null && folderData.Versions.Count > 1)
                            {
                                foreach (var version in folderData.Versions)
                                {
                                    if (version.Uiversion == folderData.Uiversion || version.Uiversion == 0)
                                    {
                                        continue;
                                    }
                                    await ProcessFolderVersionAsync(version, subFolderNode, folderData);
                                }
                            }

                            // Recursive: push folder context and call ProcessItemsAndSubfoldersAsync again
                            if (folderData.SubItems != null || folderData.SubFolders != null)
                            {
                                _folderContextStack.Push(folderData);
                                try
                                {
                                    await ProcessItemsAndSubfoldersAsync(subFolderNode, subFolderNode.Cache_NodeType, needInitInfo: needInitInfo);
                                }
                                finally
                                {
                                    _folderContextStack.Pop();
                                }
                            }
                        }
                        catch (SPObjectLockedException sle)
                        {
                            ExceptionsCaught.Add(sle);
                            logger.Info("Folder is Locked. Path:{0}. Message:{1}.", folderData.FullUrl, sle.ToString());
                        }
                        catch (SPObjectNotFoundException snfe)
                        {
                            ExceptionsCaught.Add(snfe);
                            logger.Info("Folder Not Found. Path:{0}. Message:{1}.", folderData.FullUrl, snfe.ToString());
                        }
                        catch (SPObjectReadOnlyException sroe)
                        {
                            ExceptionsCaught.Add(sroe);
                            logger.Info("Folder is ReadOnly. Path:{0}. Message:{1}.", folderData.FullUrl, sroe.ToString());
                        }
                        catch (Exception ex) when (ex.GetType().Name != "JobStopException")
                        {
                            ExceptionsCaught.Add(ex);
                            logger.Error("An unexpected error occurred while processing folder node.Path:{0}.Message:{1}.",
                                folderData.FullUrl, ex.ToString());
                        }
                    }
                }
                catch (Exception ex) when (ex.GetType().Name != "JobStopException")
                {
                    logger.Error("An error occurred while RealProcessItemsAndSubfolders.Path:{0}.Message:{1}.",
                        folderNode.FullPath, ex.ToString());
                    ExceptionsCaught.Add(ex);
                }
                #endregion
            }
        }

        /// <summary>
        /// Process a single item's version node (mirrors real ProcessVersionsAsync).
        /// Generates version node and calls ProcessItemAsync.
        /// </summary>
        private async Task ProcessItemVersionAsync(ArchiverNodeItem itemNode, VersionData version, ArchiverNodeItem folderNode)
        {
            var versionNode = new ArchiverNodeItem
            {
                FullPath = itemNode.FullPath + $"?version={version.Uiversion}",
                Title = itemNode.Title,
                Name = itemNode.Name,
                SPNodeLevel = NodeLevel.Item,
                Cache_NodeType = (int)CacheNodeType.Item,
                ItemType = AvePoint.RA.SharePoint.ArchiverCommon.ItemType.DOCUMENT_VER,
                Parent = itemNode
            };

            await FakeDiscoverWorker.ProcessItemAsync(versionNode, itemNode);
            ProcessedVersionsCount++;
        }

        /// <summary>
        /// Process a single item's attachment node (mirrors real ProcessAttachmentsAsync).
        /// Generates attachment node and calls ProcessItemAsync.
        /// </summary>
        private async Task ProcessItemAttachmentAsync(ArchiverNodeItem folderNode, ArchiverNodeItem itemNode, AttachmentData attachment, DiscoverItemData itemData)
        {
            var attachmentNode = new ArchiverNodeItem
            {
                FullPath = attachment.Url ?? itemNode.FullPath + "/Attachments/" + attachment.Name,
                Title = attachment.Name ?? "Attachment",
                Name = attachment.Name ?? "Attachment",
                SPNodeLevel = NodeLevel.Item,
                Cache_NodeType = (int)CacheNodeType.Item,
                ItemType = AvePoint.RA.SharePoint.ArchiverCommon.ItemType.ATTACHMENT,
                Parent = itemNode
            };

            await FakeDiscoverWorker.ProcessItemAsync(attachmentNode, itemNode);
            ProcessedAttachmentsCount++;
        }

        /// <summary>
        /// Process a folder's attachment (mirrors real ProcessAttachmentsAsync for folder type).
        /// </summary>
        private async Task ProcessFolderAttachmentAsync(ArchiverNodeItem parentFolderNode, ArchiverNodeItem folderNode, AttachmentData attachment)
        {
            var attachmentNode = new ArchiverNodeItem
            {
                FullPath = attachment.Url ?? folderNode.FullPath + "/Attachments/" + attachment.Name,
                Title = attachment.Name ?? "Attachment",
                Name = attachment.Name ?? "Attachment",
                SPNodeLevel = NodeLevel.Item,
                Cache_NodeType = (int)CacheNodeType.Item,
                ItemType = AvePoint.RA.SharePoint.ArchiverCommon.ItemType.ATTACHMENT,
                Parent = folderNode
            };

            await FakeDiscoverWorker.ProcessItemAsync(attachmentNode, folderNode);
            ProcessedAttachmentsCount++;
        }

        /// <summary>
        /// Process a folder's version node (mirrors real ProcessFolderVersionsAsync).
        /// Generates folder version node and calls ProcessContainerAsync.
        /// </summary>
        private async Task ProcessFolderVersionAsync(VersionData version, ArchiverNodeItem folderNode, DiscoverFolderData folderData)
        {
            var folderVersionNode = new ArchiverNodeItem
            {
                FullPath = folderNode.FullPath + $"?version={version.Uiversion}",
                Title = folderNode.Title,
                Name = folderNode.Name,
                SPNodeLevel = NodeLevel.Folder,
                Cache_NodeType = (int)CacheNodeType.Folder,
                Parent = folderNode
            };

            await FakeDiscoverWorker.ProcessContainerAsync(folderVersionNode, ProcessType.NeedProcess);
            ProcessedVersionsCount++;
        }

        public override async Task InitialSPObjectInfoAsync(IDiscoverNodeWorker discoverWork, ArchiverNodeItem node)
        {
            // No real SP object initialization in tests
            await Task.CompletedTask;
        }

        // Public helper methods to access internal test data
        public ArchiverNodeItem? CreateNodeFromData(ArchiverNodeItemData? data)
        {
            if (data == null) return null;

            var node = new ArchiverNodeItem
            {
                FullPath = data.FullPath,
                Title = data.Title,
                Name = data.Name,
                ID = Guid.TryParse(data.ID, out var id) ? id : Guid.NewGuid(),
                SPNodeLevel = ParseNodeLevel(data.SPNodeLevel ?? "List"),
                Cache_NodeType = data.CacheNodeType,
                SiteUrl = data.SiteUrl,
                WebId = Guid.TryParse(data.WebId, out var webId) ? webId : Guid.NewGuid(),
                ListId = Guid.TryParse(data.ListId, out var listId) ? listId : Guid.NewGuid(),
                IsSystemObject = data.IsSystemObject
            };

            return node;
        }

        public FakeDiscoverFolder GetFakeRootFolder() => _fakeRootFolder;

        public FakeDiscoverFolder GetFakeDiscoverList() => _fakeRootFolder; // For list level, use same fake folder
        
        private static RuleNodeContract CreateFakeRuleNodeContract()
        {
            // Create minimal RuleNodeContract for discover worker initialization
            return new RuleNodeContract
            {
                BreakInheritNodesEncryptBySha1 = new Dictionary<string, RuleNodeContract>()
            };
        }

        private static void ThrowSimulatedException(string? exceptionType, string? message, string? path)
        {
            var msg = message ?? $"Simulated exception for: {path}";
            switch (exceptionType?.ToUpperInvariant())
            {
                case "LOCKED":
                    throw new SPObjectLockedException(msg, "Folder", path ?? "");
                case "NOTFOUND":
                    throw new SPObjectNotFoundException(msg);
                case "READONLY":
                    throw new SPObjectReadOnlyException(msg, "Folder", path ?? "");
                default:
                    throw new InvalidOperationException(msg);
            }
        }

        private static AvePoint.RA.SharePoint.ArchiverCommon.ItemType ParseItemType(string? itemType)
        {
            if (string.IsNullOrEmpty(itemType))
                return AvePoint.RA.SharePoint.ArchiverCommon.ItemType.DOCUMENT;

            return itemType.ToUpperInvariant() switch
            {
                "DOCUMENT" => AvePoint.RA.SharePoint.ArchiverCommon.ItemType.DOCUMENT,
                "ITEM_TYPE" => AvePoint.RA.SharePoint.ArchiverCommon.ItemType.ITEM_TYPE,
                "DOCUMENT_VER" => AvePoint.RA.SharePoint.ArchiverCommon.ItemType.DOCUMENT_VER,
                "ITEM_VERSION" => AvePoint.RA.SharePoint.ArchiverCommon.ItemType.ITEM_VERSION,
                "ATTACHMENT" => AvePoint.RA.SharePoint.ArchiverCommon.ItemType.ATTACHMENT,
                _ => AvePoint.RA.SharePoint.ArchiverCommon.ItemType.DOCUMENT
            };
        }

        private FakeAveListItem CreateFakeListItemFromData(DiscoverItemData itemData)
        {
            var fakeItem = new FakeAveListItem
            {
                Title = itemData.Title ?? itemData.LeafName ?? "Untitled",
                Name = itemData.LeafName ?? "Untitled",
                Url = itemData.FullUrl ?? "",
                ID = itemData.ID,
                UniqueId = Guid.TryParse(itemData.DocID, out var docId) ? docId : Guid.NewGuid()
            };

            // Set field values for rule evaluation
            if (itemData.Modified.HasValue)
                fakeItem["Modified"] = itemData.Modified.Value;
            if (itemData.Created.HasValue)
                fakeItem["Created"] = itemData.Created.Value;
            if (itemData.FileSize > 0)
                fakeItem["File_x0020_Size"] = itemData.FileSize.ToString();
            if (!string.IsNullOrEmpty(itemData.LeafName))
                fakeItem["FileLeafRef"] = itemData.LeafName;

            // Set additional field values from JSON
            if (itemData.FieldValues != null)
            {
                foreach (var kvp in itemData.FieldValues)
                {
                    fakeItem[kvp.Key] = kvp.Value;
                }
            }

            // Set ParentList to a shared FakeAveList for BaseType check in CheckItemCriteria
            fakeItem.ParentList = GetOrCreateFakeAveListForItems();

            return fakeItem;
        }

        private FakeAveList _sharedFakeAveList;
        private FakeAveList GetOrCreateFakeAveListForItems()
        {
            if (_sharedFakeAveList == null)
            {
                _sharedFakeAveList = new FakeAveList
                {
                    Title = _testData.ListNode?.Title ?? "TestList",
                    BaseTemplate = AvePoint.Wrapper.Common.AveListTemplateType.DocumentLibrary,
                    BaseType = AvePoint.Wrapper.Common.AveBaseType.DocumentLibrary
                };
            }
            return _sharedFakeAveList;
        }
    }
}
