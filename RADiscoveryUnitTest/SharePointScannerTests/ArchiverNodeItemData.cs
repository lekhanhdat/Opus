namespace RADiscoveryUnitTest.SharePointScannerTests
{
    public class ArchiverNodeItemData
    {
        public string? FullPath { get; set; }
        public string? Title { get; set; }
        public string? Name { get; set; }
        public string? ID { get; set; } // Stored as string in JSON, parsed to Guid
        public string? SPNodeLevel { get; set; } // List, Folder, etc.
        public int CacheNodeType { get; set; }
        public string? SiteUrl { get; set; }
        public string? WebId { get; set; }
        public string? ListId { get; set; }
        public bool IsSystemObject { get; set; } = false;
    }
}
