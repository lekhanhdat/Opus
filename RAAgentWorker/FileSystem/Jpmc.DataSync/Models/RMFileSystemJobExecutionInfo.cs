using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Services;
using System;

namespace RAFileSystem.FileSystem.Jpmc.DataSync
{
    public class RMFileSystemJobExecutionInfo
    {
        public Guid DirectoryId { get; set; }

        public string DirectoryFullPath { get; set; }

        public string DirectoryRelativePath { get; set; }

        public Guid DirectoryParentId { get; set; }

        public string DirectoryParentFullPath { get; set; }

        public string RootId { get; set; }

        public string RootPath { get; set; }

        public string ConnectionGroupId { get; set; }

        public string ConnectionGroupPath { get; set; }

        public string ConnectionId { get; set; }

        public string ConnectionPath { get; set; }

        public DateTime LastScanTime { get; set; }

        public FSJobType ExecutionType { get; set; }

        public bool EnabledRecordManagement { get; set; }

        public RMFileSystemUniqueIdSetting UniqueIdSetting { get; set; } = new RMFileSystemUniqueIdSetting();

        public RMFileSystemClassCode ClassCodeInfo { get; set; } = new RMFileSystemClassCode();

        public int MaxConcurrentExecutionCount { get; set; }

        public override string ToString()
        {
            return @$"
DirectoryId: {DirectoryId},
DirectoryFullPath: {DirectoryFullPath?.LogBase64()},
DirectoryRelativePath: {DirectoryRelativePath?.LogBase64()},
DirectoryParentId: {DirectoryParentId},
DirectoryParentFullPath: {DirectoryParentFullPath?.LogBase64()},
RootId: {RootId},
RootPath: {RootPath?.LogBase64()},
ConnectionGroupId: {ConnectionGroupId},
ConnectionGroupPath: {ConnectionGroupPath?.LogBase64()},
ConnectionId: {ConnectionId},
ConnectionPath: {ConnectionPath?.LogBase64()},
LastScanTime: {LastScanTime},
ExecutionType: {ExecutionType},
EnabledRecordManagement: {EnabledRecordManagement},
UniqueId Setting: (Actived: {UniqueIdSetting?.Actived}, Stored: {UniqueIdSetting.Stored}, Prefix: {UniqueIdSetting.Prefix}),
Class Code Setting: (Id: {ClassCodeInfo?.Id}, Name: {ClassCodeInfo?.Name}, CountryCode: {ClassCodeInfo?.CountryCode}, RetentionType: {ClassCodeInfo?.RetentionType}, StartDate: {ClassCodeInfo?.StartDate}, PolicyValueUnit: {ClassCodeInfo?.PolicyValueUnit}, PolicyValueNumber: {ClassCodeInfo?.PolicyValueNumber})";
        }
    }

    public class RMFileSystemUniqueIdSetting
    {
        public bool Actived { get; set; }

        public bool Stored { get; set; }

        public string Prefix { get; set; }
    }
}
