using System.Collections.Generic;

namespace RADiscoveryUnitTest.SharePointScannerTests
{
    public class ExpectedBehavior
    {
        public bool ShouldThrowException { get; set; }
        public string? ExceptionType { get; set; } // SPObjectLockedException, SPObjectNotFoundException, etc.
        public string? ExceptionMessage { get; set; }
        public int ExpectedProcessedItemsCount { get; set; }
        public int ExpectedProcessedFoldersCount { get; set; }
        public int ExpectedProcessedVersionsCount { get; set; }
        public int ExpectedProcessedAttachmentsCount { get; set; }
        public int ExpectedExceptionsCaughtCount { get; set; }
        public List<string> ExpectedReportedPaths { get; set; } = new();
        public bool ShouldReportFailure { get; set; }
    }
}
