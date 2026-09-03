using System;
using System.Collections.Generic;

namespace RADiscoveryUnitTest.SharePointScannerTests
{
    public class DiscoverFolderData
    {
        public string? UniqueId { get; set; }
        public string? FullUrl { get; set; }
        public string? ItemName { get; set; }
        public int? ID { get; set; }
        public int Uiversion { get; set; }
        public bool IsSystemObject { get; set; }
        public bool ShouldThrowException { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? ExceptionType { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public List<DiscoverItemData>? SubItems { get; set; }
        public List<DiscoverFolderData>? SubFolders { get; set; }
        public List<AttachmentData> Attachments { get; set; } = new();
        public List<VersionData> Versions { get; set; } = new();
    }
}
