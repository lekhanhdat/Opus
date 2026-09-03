using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using System;
using System.Collections.Generic;


namespace RAFileSystem.FileSystem.Jpmc.DataSync
{
    public class RMFileSystemItemMetadata
    {
        public Guid Id { get; set; }

        public Guid ParentId { get; set; }

        public Guid FailedRerunId { get; set; }

        public string FullPath { get; set; }

        public bool HasAds { get; set; }

        public string AdsId { get; set; }

        public bool HasSynced { get; set; }

        public bool IsCopy { get; set; }

        public bool IsMove { get; set; }

        public FileSystemRecordDto CurrentRecordInfo { get; set; }

        public List<(bool existInLocal, FileSystemRecordDto record)> SameAdsIdRecords { get; set; }

        public RMFileSystemClassCode ClassCodeInfo { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is RMFileSystemItemMetadata item)) return false;
            return this.Id == item.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class RMFileSystemDirectoryMetadata : RMFileSystemItemMetadata
    {
        public bool HasChanged { get; set; }

        public bool IsRoot { get; set; }

        public bool IsPriorFailure { get; set; }

        public StorageInfo DirectoryInfo { get; set; }

        public bool IsHidden { get; set; }
    }

    public class RMFileSystemFileMetadata : RMFileSystemItemMetadata
    {
        public XFileInfoEx FileInfo { get; set; }
    }
}
