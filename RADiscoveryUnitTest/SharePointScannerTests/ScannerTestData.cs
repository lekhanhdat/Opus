using System.Collections.Generic;

namespace RADiscoveryUnitTest.SharePointScannerTests
{
    /// <summary>
    /// Test data structure loaded from JSON files to drive scanner tests.
    /// This allows controlling data sources to test various scenarios like incomplete data,
    /// expected exceptions, and job flow validation.
    /// Rules and fake data are paired together so each scenario defines its own filtering logic.
    /// </summary>
    public class ScannerTestData
    {
        public ArchiverNodeItemData? ListNode { get; set; }
        public ArchiverNodeItemData? FolderNode { get; set; }
        public List<DiscoverItemData> Items { get; set; } = new();
        public List<DiscoverFolderData> Folders { get; set; } = new();

        /// <summary>
        /// Rules that define the filter conditions for this test scenario.
        /// When null or empty, default wildcard rules are used (match all items).
        /// Each rule is paired with the test data to verify correct filtering behavior.
        /// </summary>
        public List<RuleData>? Rules { get; set; }

        public ExpectedBehavior? Expected { get; set; }
    }
}
