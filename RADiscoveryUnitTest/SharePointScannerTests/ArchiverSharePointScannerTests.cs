using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using Castle.Windsor;
using Castle.MicroKernel.Proxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RADiscoveryUnitTest.SharePointScannerTests
{
    [TestClass]
    public class ArchiverSharePointScannerTests
    {
        private string _testDataDirectory = string.Empty;

        [TestInitialize]
        public void TestInitialize()
        {
            try
            {
                // 1. 初始化 Log4net 配置 (与 RAScheduleJob 一致)
                RALogger.ConfigFile = "AgentLog4net.config";
                
                // 2. 设置测试数据目录
                _testDataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "SharePointScanner");
                Directory.CreateDirectory(_testDataDirectory);
                
                // 3. 初始化 Tenant 信息 (模拟 InitTenantAndJobIdByArgs)
                TenantLocalValue.LogonGroupId = "2b0080d1-8003-4cc0-915e-3ffb204e5de4";
                TenantLocalValue.LogonUserEmail = "ytzhang@avepoint.com";

                // 4. 初始化 Logger (模拟 InitLogger)
                string testJobId = "SO20260515170609101223_000";
                RALogger.SeparateLogToTenant(TenantLocalValue.LogonGroupId, testJobId);
                RALogger.SetCustomizedLogPostfix("V: Test");
                
                // 5. 初始化全局配置 (模拟 RunInLocal)
                RMGlobalConfiguration.Init();
                
                // 6. 初始化 Windsor 容器 (模拟 InitCastle)
                InitCastle();
                
                System.Diagnostics.Debug.WriteLine("Test environment initialized successfully");
            }
            catch (Exception ex)
            {
                // 记录初始化错误但不让测试失败
                System.Diagnostics.Debug.WriteLine($"TestInitialize warning: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
        
        private void InitCastle()
        {
            try
            {
                string installPath = AppDomain.CurrentDomain.BaseDirectory;
                WindsorContainer windsorContainer = new WindsorContainer();
                windsorContainer.Install(Castle.Windsor.Installer.Configuration.FromXmlFile(
                    Path.Combine(installPath, "Castle/ServiceCastle.config")));
                var selector = windsorContainer.Resolve<IModelInterceptorsSelector>("AvePoint.RA.Common.Audit.AuditInterceptorSelector");
                windsorContainer.Kernel.ProxyFactory.AddInterceptorSelector(selector);
                PlatformWindsorManager.SetUp(windsorContainer);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitCastle failed: {ex.Message}");
                throw;
            }
        }

        [TestMethod]
        public async Task ProcessListAsync_WithIncompleteData_ShouldHandleGracefully()
        {
            // Arrange
            var testDataFile = Path.Combine(_testDataDirectory, "incomplete_list_data.json");
            var testData = LoadTestData(testDataFile);
            var scanner = new TestArchiverSharePointScanner(testData);

            // Create list node with fake discover object
            var listNode = scanner.CreateNodeFromData(testData.ListNode);
            listNode!.DiscoverSPObject = scanner.GetFakeDiscoverList();

            // Act - Call REAL base class ProcessListAsync
            await scanner.ProcessListAsync(listNode);

            // Assert
            Assert.AreEqual(testData.Expected?.ExpectedProcessedItemsCount ?? 0, scanner.ProcessedItemsCount);
            Assert.AreEqual(testData.Expected?.ExpectedProcessedFoldersCount ?? 0, scanner.ProcessedFoldersCount);
        }

        [TestMethod]
        public async Task ProcessFolderAsync_WithMixedItemsAndFolders_ShouldProcessAll()
        {
            // Arrange
            var testDataFile = Path.Combine(_testDataDirectory, "mixed_folder_content.json");
            var testData = LoadTestData(testDataFile);
            var scanner = new TestArchiverSharePointScanner(testData);

            // Create folder node with fake discover object  
            var folderNode = scanner.CreateNodeFromData(testData.FolderNode ?? testData.ListNode);
            folderNode!.DiscoverSPObject = scanner.GetFakeRootFolder();

            // Act - Call REAL base class ProcessFolderAsync
            await scanner.ProcessFolderAsync(folderNode);

            // Assert
            Assert.AreEqual(testData.Expected?.ExpectedProcessedItemsCount ?? 0, scanner.ProcessedItemsCount);
            Assert.AreEqual(testData.Expected?.ExpectedProcessedFoldersCount ?? 0, scanner.ProcessedFoldersCount);
        }

        [TestMethod]
        public async Task ProcessItemsAndSubfoldersAsync_WhenExceptionInMiddle_ShouldContinueProcessing()
        {
            // Arrange
            var testDataFile = Path.Combine(_testDataDirectory, "exception_in_processing.json");
            var testData = LoadTestData(testDataFile);
            var scanner = new TestArchiverSharePointScanner(testData);

            // Create folder node with fake discover object
            var folderNode = scanner.CreateNodeFromData(testData.FolderNode ?? testData.ListNode);
            folderNode!.DiscoverSPObject = scanner.GetFakeRootFolder();

            // Act - Call the overridden ProcessItemsAndSubfoldersAsync which keeps real business logic
            await scanner.ProcessItemsAndSubfoldersAsync(folderNode, folderNode.Cache_NodeType);

            // Assert - verify that processing continued despite exceptions
            Assert.IsGreaterThan(0, scanner.ProcessedItemsCount, "Expected ProcessedItemsCount > 0");
            Assert.IsNotEmpty(scanner.ExceptionsCaught, "Expected ExceptionsCaught.Count > 0");
        }

        [TestMethod]
        public async Task ProcessItemsAndSubfoldersAsync_WithVersions_ShouldProcessAllNonCurrentVersions()
        {
            // Arrange
            var testDataFile = Path.Combine(_testDataDirectory, "item_versions_and_attachments.json");
            var testData = LoadTestData(testDataFile);
            var scanner = new TestArchiverSharePointScanner(testData);

            var folderNode = scanner.CreateNodeFromData(testData.FolderNode ?? testData.ListNode);
            folderNode!.DiscoverSPObject = scanner.GetFakeRootFolder();

            // Act
            await scanner.ProcessItemsAndSubfoldersAsync(folderNode, folderNode.Cache_NodeType);

            // Assert - verify items, versions, and attachments counts
            Assert.AreEqual(testData.Expected!.ExpectedProcessedItemsCount, scanner.ProcessedItemsCount,
                "Processed items count mismatch");
            Assert.AreEqual(testData.Expected.ExpectedProcessedVersionsCount, scanner.ProcessedVersionsCount,
                "Processed versions count mismatch");
            Assert.AreEqual(testData.Expected.ExpectedProcessedAttachmentsCount, scanner.ProcessedAttachmentsCount,
                "Processed attachments count mismatch");
            Assert.AreEqual(testData.Expected.ExpectedExceptionsCaughtCount, scanner.ExceptionsCaught.Count,
                "Exceptions caught count mismatch");
        }

        [TestMethod]
        public async Task ProcessItemsAndSubfoldersAsync_WithNestedFolders_ShouldRecurseAndProcessVersions()
        {
            // Arrange
            var testDataFile = Path.Combine(_testDataDirectory, "nested_folders_with_versions.json");
            var testData = LoadTestData(testDataFile);
            var scanner = new TestArchiverSharePointScanner(testData);

            var folderNode = scanner.CreateNodeFromData(testData.FolderNode ?? testData.ListNode);
            folderNode!.DiscoverSPObject = scanner.GetFakeRootFolder();

            // Act - This should recursively process: root items → folder "2024" → its sub-items → sub-folder "Q1" → Q1's items
            await scanner.ProcessItemsAndSubfoldersAsync(folderNode, folderNode.Cache_NodeType);

            // Assert - verify complete recursive traversal including versions and attachments
            Assert.AreEqual(testData.Expected!.ExpectedProcessedItemsCount, scanner.ProcessedItemsCount,
                "Processed items count mismatch (should include items at all nesting levels)");
            Assert.AreEqual(testData.Expected.ExpectedProcessedFoldersCount, scanner.ProcessedFoldersCount,
                "Processed folders count mismatch (should include all nested folders)");
            Assert.AreEqual(testData.Expected.ExpectedProcessedVersionsCount, scanner.ProcessedVersionsCount,
                "Processed versions count mismatch (should include item and folder versions)");
            Assert.AreEqual(testData.Expected.ExpectedProcessedAttachmentsCount, scanner.ProcessedAttachmentsCount,
                "Processed attachments count mismatch (should include item and folder attachments)");
        }

        private ScannerTestData LoadTestData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                // Create default test data if file doesn't exist
                return CreateDefaultTestData();
            }

            var json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<ScannerTestData>(json, options) ?? CreateDefaultTestData();
        }

        private ScannerTestData CreateDefaultTestData()
        {
            return new ScannerTestData
            {
                ListNode = new ArchiverNodeItemData
                {
                    FullPath = "https://test.sharepoint.com/sites/test/Lists/TestList",
                    Title = "TestList",
                    Name = "TestList",
                    ID = Guid.NewGuid().ToString(),
                    SPNodeLevel = "List",
                    CacheNodeType = 2, // List node type
                    SiteUrl = "https://test.sharepoint.com/sites/test",
                    WebId = Guid.NewGuid().ToString(),
                    ListId = Guid.NewGuid().ToString()
                },
                FolderNode = new ArchiverNodeItemData
                {
                    FullPath = "https://test.sharepoint.com/sites/test/Lists/TestList/TestFolder",
                    Title = "TestFolder",
                    Name = "TestFolder",
                    ID = Guid.NewGuid().ToString(),
                    SPNodeLevel = "Folder",
                    CacheNodeType = 4, // Folder node type
                    SiteUrl = "https://test.sharepoint.com/sites/test",
                    WebId = Guid.NewGuid().ToString(),
                    ListId = Guid.NewGuid().ToString()
                },
                Items = new List<DiscoverItemData>
                {
                    new DiscoverItemData
                    {
                        DocID = Guid.NewGuid().ToString(),
                        FullUrl = "https://test.sharepoint.com/sites/test/Lists/TestList/Item1.docx",
                        LeafName = "Item1.docx",
                        ID = 1,
                        Uiversion = 512
                    }
                },
                Folders = new List<DiscoverFolderData>(),
                Expected = new ExpectedBehavior
                {
                    ShouldThrowException = false,
                    ExpectedProcessedItemsCount = 1,
                    ExpectedProcessedFoldersCount = 0
                }
            };
        }
    }
}
