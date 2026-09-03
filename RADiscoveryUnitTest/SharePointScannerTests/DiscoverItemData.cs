using System;
using System.Collections.Generic;

namespace RADiscoveryUnitTest.SharePointScannerTests
{
    public class DiscoverItemData
    {
        public string? DocID { get; set; }
        public string? FullUrl { get; set; }
        public string? LeafName { get; set; }
        public int ID { get; set; }
        public int Uiversion { get; set; }
        public List<AttachmentData> Attachments { get; set; } = new();
        public List<VersionData> Versions { get; set; } = new();
        public bool ShouldThrowException { get; set; }
        public string? ExceptionMessage { get; set; }

        // Properties for rule evaluation via FakeAveListItem
        public string? Title { get; set; }
        public long FileSize { get; set; }
        public string? ContentTypeName { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public string? ItemType { get; set; } // DOCUMENT, ITEM_TYPE, DOCUMENT_VER, ITEM_VERSION, ATTACHMENT
        public bool IsSystemObject { get; set; } = false;
        public Dictionary<string, object>? FieldValues { get; set; }
    }
}
