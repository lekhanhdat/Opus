using System;
using System.Collections.Generic;
using AvePoint.Wrapper.Discovery;

namespace RADiscoveryUnitTest.SharePointScannerTests
{
    /// <summary>
    /// Fake discover folder that returns data from JSON test data instead of SharePoint.
    /// Wraps data to provide item/folder lists without inheriting from AveDiscoverFolder.
    /// </summary>
    public class FakeDiscoverFolder
    {
        private readonly ScannerTestData _testData;
        public int ItemCount => _testData.Items?.Count ?? 0;
        public int FolderCount => _testData.Folders?.Count ?? 0;

        public FakeDiscoverFolder(ScannerTestData testData)
        {
            _testData = testData ?? throw new ArgumentNullException(nameof(testData));
        }

        /// <summary>
        /// Returns items loaded from JSON instead of querying SharePoint.
        /// Since AveDiscoverItem cannot be easily instantiated, we return empty batches.
        /// The actual item processing count is tracked separately in ProcessItemsAndSubfoldersAsync.
        /// </summary>
        public IEnumerable<List<AveDiscoverItem>> GetItemsWithStructureForArchiver()
        {
            // Return empty list - we'll track item count directly from JSON data
            // without creating real AveDiscoverItem instances which require internal initialization
            yield return new List<AveDiscoverItem>();
        }

        /// <summary>
        /// Returns folders loaded from JSON instead of querying SharePoint
        /// </summary>
        public IEnumerable<List<AveDiscoverFolder>> GetFoldersWithStructure(bool includeSystemFolders)
        {
            // Return empty list - we'll track folder count directly from JSON data
            yield return new List<AveDiscoverFolder>();
        }

        public void ClearSubItemsCache() { }
        public void ClearSubFoldersCache() { }
        public void RemoveFolderCache(List<int> folderIds) { }
        public void Dispose() { }
    }
}
