using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.FileSystem.Core;
using Newtonsoft.Json;
using RAFileSystem.FileSystem.DataSync.Utils;
using RAFileSystem.Utils;
using System;
using System.Linq;

namespace RAFileSystem.FileSystem.Jpmc.DataSync
{
    public class RMFileSystemMetadataAssembler
    {
        private readonly RMFileSystemJobExecutionInfo _executionInfo;

        public RMFileSystemMetadataAssembler(RMFileSystemJobExecutionInfo executionInfo)
        {
            _executionInfo = executionInfo;
        }

        public FileSystemRecordDto AssembleDirectoryRecordInfo(RMFileSystemDirectoryMetadata directoryMetadata)
        {
            if (directoryMetadata == null) throw new ArgumentNullException(nameof(directoryMetadata));

            var directory = new XDirectoryInfoEx(directoryMetadata.DirectoryInfo);

            var record = new FileSystemRecordDto
            {
                DirPath = Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(directoryMetadata.FullPath),
                FolderId = directoryMetadata.ParentId,
                FullPath = directoryMetadata.FullPath,
                ItemId = directoryMetadata.Id,
                NodeId = directoryMetadata.Id,
                ItemRowId = -1,
                RecordStatus = 1,
                LeafName = directory.Name,
                ListId = Guid.Empty,
                NodeType = (int)NodeLevel.FSFolder,
                ScopeId = _executionInfo.ConnectionPath.ToLowerInvariant().ToMd5(),
                AveSiteId = _executionInfo.ConnectionId,
                SourceFlag = (int)SourceFlag.FileSystem,
                TimeCreated1 = directory.CreationTimeUtc,
                TimeLastModified = directory.LastWriteTimeUtc.Ticks,
                ParentId = directoryMetadata.ParentId,
                FileSize = directory.Length,
                JPMCFSFileSize = 0,
                JPMCFSFileCount = 0,
                RecordsId = directoryMetadata.AdsId,
                CreatedBy = FormatCreatedBy(directory.Owner),
                SortTicks = Snowflake.Instance().GetTicks(),
                MetaInfo = JsonConvert.SerializeObject(new RecordMetaInfo
                {
                    FileSize = directory.Length,
                    LocalFullPath = directory.LocalFullPath,
                    LastModifiedTime = directory.LastWriteTimeUtc.Ticks,
                    CreatedTime = directory.CreationTimeUtc.Ticks,
                })
            };

            PopulateCommonMetadata(record, directoryMetadata);
            return record;
        }

        public FileSystemRecordDto AssembleFileRecordInfo(RMFileSystemFileMetadata fileMetadata)
        {
            if (fileMetadata == null) throw new ArgumentNullException(nameof(fileMetadata));

            var file = fileMetadata.FileInfo;
            var extension = Alphaleonis.Win32.Filesystem.Path.GetExtension(file.FileFullPath).TrimStart('.');

            var record = new FileSystemRecordDto
            {
                LeafName = file.Name,
                DirPath = Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(fileMetadata.FullPath),
                ExtensionForFile = extension,
                FolderId = fileMetadata.ParentId,
                FullPath = fileMetadata.FullPath,
                ItemId = fileMetadata.Id,
                NodeId = fileMetadata.Id,
                RecordStatus = 1,
                NodeType = (int)NodeLevel.FSFile,
                ScopeId = _executionInfo.ConnectionPath.ToLowerInvariant().ToMd5(),
                AveSiteId = _executionInfo.ConnectionId,
                SourceFlag = (int)SourceFlag.FileSystem,
                TimeCreated1 = file.CreationTimeUtc,
                TimeLastModified = file.LastWriteTimeUtc.Ticks,
                ParentId = fileMetadata.ParentId,
                FileSize = file.FileSize,
                JPMCFSFileSize = file.FileSize,
                JPMCFSFileCount = 0,
                RecordsId = fileMetadata.AdsId,
                CreatedBy = FormatCreatedBy(file.Owner),
                SortTicks = Snowflake.Instance().GetTicks(),
                MetaInfo = JsonConvert.SerializeObject(new RecordMetaInfo
                {
                    FileSize = file.FileSize,
                    LastAccessTime = file.LastAccessTimeUtc.Ticks,
                    Owner = file.Owner,
                    LocalFullPath = file.FileFullPath,
                    CreatedTime = file.CreationTimeUtc.Ticks,
                    LastModifiedTime = file.LastWriteTimeUtc.Ticks,
                    FileTypeName = FileTypeDescriptionResolver.Resolve(extension),
                })
            };

            //if(fileMetadata.HasSynced && fileMetadata.CurrentRecordInfo != null)
            //{
            //    record.CreateDate = fileMetadata.CurrentRecordInfo.CreateDate;
            //    record.hasDuplicated = fileMetadata.CurrentRecordInfo.RecordStatus == (int)RMRecordStatus.ManualPreSync && fileMetadata.CurrentRecordInfo.CreateDate == 0;
            //}

            PopulateCommonMetadata(record, fileMetadata);
            return record;
        }

        public FileSystemRecordDto AssembleRootRecordInfo(RMFileSystemJobExecutionInfo executionInfo)
        {
            var selfId = new Guid(executionInfo.RootId);
            var fullPath = executionInfo.RootPath;
            var record = new FileSystemRecordDto()
            {
                AveSiteId = Guid.Empty.ToString(),
                DirPath = fullPath,
                FolderId = selfId,
                FullPath = fullPath,
                ItemId = selfId,
                ItemRowId = -1,
                LeafName = fullPath,
                NodeId = selfId,
                ParentId = Guid.Empty,
                ScopeId = Guid.Empty,
                TermId = Guid.Empty,
                RuleId = Guid.Empty,
                RecordsId = string.Empty,
                NodeType = (int)NodeLevel.FSConnectionGroups,
                SourceFlag = (int)SourceFlag.FileSystem,
                TimeCreated1 = DateTime.UtcNow,
                TimeLastModified = Convert.ToInt64(DateTime.UtcNow.Ticks),
                RecordStatus = 1,
            };
            return record;
        }

        public FileSystemRecordDto AssembleGroupRecordInfo(RMFileSystemJobExecutionInfo executionInfo)
        {
            var selfId = new Guid(executionInfo.ConnectionGroupId);
            var fullPath = executionInfo.ConnectionGroupPath;
            var parentId = new Guid(executionInfo.RootId);
            var record = new FileSystemRecordDto()
            {
                AveSiteId = Guid.Empty.ToString(),
                DirPath = fullPath,
                FolderId = selfId,
                FullPath = fullPath,
                ItemId = selfId,
                ItemRowId = -1,
                LeafName = fullPath,
                NodeId = selfId,
                ParentId = parentId,
                ScopeId = Guid.Empty,
                TermId = Guid.Empty,
                RuleId = Guid.Empty,
                RecordsId = string.Empty,
                NodeType = (int)NodeLevel.FSConnectionGroup,
                SourceFlag = (int)SourceFlag.FileSystem,
                TimeCreated1 = DateTime.UtcNow,
                TimeLastModified = Convert.ToInt64(DateTime.UtcNow.Ticks),
                RecordStatus = 1,
            };
            return record;
        }

        private void PopulateCommonMetadata(FileSystemRecordDto target, RMFileSystemItemMetadata metadata)
        {
            if (metadata.HasSynced && metadata.CurrentRecordInfo != null)
            {
                CopySyncedRecordFields(target, metadata.CurrentRecordInfo);
            }

            if (metadata.ClassCodeInfo != null && metadata.ClassCodeInfo.Exists)
            {
                CopyClassCodeFields(target, metadata.ClassCodeInfo);
            }

            if (metadata.IsMove && metadata.SameAdsIdRecords != null && metadata.SameAdsIdRecords.Count > 0)
            {
                var latestRecord = metadata.SameAdsIdRecords
                    .OrderByDescending(item => item.record?.CollectionTime ?? 0)
                    .FirstOrDefault().record;

                if (latestRecord != null)
                {
                    CopyMoveFields(target, latestRecord);
                }
            }
        }

        private static void CopySyncedRecordFields(FileSystemRecordDto target, FileSystemRecordDto source)
        {
            target.DeclareAsRecord = source.DeclareAsRecord;
            target.DeclaredBy = source.DeclaredBy;
            target.HoldStatus = source.HoldStatus;
            target.HoldReleaseTime = source.HoldReleaseTime;
            target.HoldBy = source.HoldBy;
            target.HoldId = source.HoldId;
            target.HoldType = source.HoldType;
            target.RecordsId = source.RecordsId;
            target.RecordHistory = source.RecordHistory;
            target.RelatedRecords = source.RelatedRecords;
            target.RelatedRecordsCount = source.RelatedRecordsCount;
            target.IsManualSynced = source.IsManualSynced;
            target.ManualActionTime = source.ManualActionTime;
            target.ManualApprovedBy = source.ManualApprovedBy;
            target.ManualEscalatedComment = source.ManualEscalatedComment;
            target.ManualApprovedStatus = source.ManualApprovedStatus;
            target.ManualArchiveStatus = source.ManualArchiveStatus;
            target.ManualInternalApprovedStatus = source.ManualInternalApprovedStatus;
            target.ManualFullPath = source.ManualFullPath;
            target.ManualEscalateFrom = source.ManualEscalateFrom;
            target.ManualExtendTime = source.ManualExtendTime;
            target.ManualExtendComment = source.ManualExtendComment;
            target.ManualCollectionTime = source.ManualCollectionTime;
            target.ManualAudits = source.ManualAudits;
            target.ManualArchivedTime = source.ManualArchivedTime;
            target.ManualPartitionKey = source.ManualPartitionKey;
            target.ManualRowKey = source.ManualRowKey;
            target.ManualRuleName = source.ManualRuleName;
            target.ManualRuleCriteria = source.ManualRuleCriteria;
            target.ManualRuleDisposalClass = source.ManualRuleDisposalClass;
            target.ManualVersion = source.ManualVersion;
            target.ManualReviewer = source.ManualReviewer;
            target.ManualRelatedRecordsAction = source.ManualRelatedRecordsAction;
            target.ManualRelatedRecords = source.ManualRelatedRecords;
            target.ManualIsRelatedRecords = source.ManualIsRelatedRecords;
            target.ManualWorkflowInstanceId = source.ManualWorkflowInstanceId;
            target.ManualExtendCount = source.ManualExtendCount;
            target.ManualEmailNotificationCount = source.ManualEmailNotificationCount;
            target.ManualEmailNotificationLastTime = source.ManualEmailNotificationLastTime;
            target.ManualNeedEmailNotification = source.ManualNeedEmailNotification;
            target.ManualIsAutoReassigned = source.ManualIsAutoReassigned;
            target.HoldByUsers = source.HoldByUsers;
            target.HoldUntilTimes = source.HoldUntilTimes;
            target.AppendHolds_Array = source.AppendHolds_Array;
        }

        private static void CopyClassCodeFields(FileSystemRecordDto target, RMFileSystemClassCode classCode)
        {
            target.CountryCode = classCode.CountryCode;
            target.ClassCode = classCode.Name;
            target.RetentionType = classCode.RetentionType;
            target.StartDate = classCode.StartDate;
            target.EndTime = classCode.EndTime;
            target.TermId = classCode.Id;
            target.TermName = classCode.Name;
            target.PolicyValueNumber = classCode.PolicyValueNumber;
            target.PolicyValueUnit = classCode.PolicyValueUnit;
        }

        private static void CopyMoveFields(FileSystemRecordDto target, FileSystemRecordDto source)
        {
            target.HoldStatus = source.HoldStatus;
            target.HoldType = source.HoldType;
            target.HoldReleaseTime = source.HoldReleaseTime;
            target.HoldId = source.HoldId;
            target.HoldBy = source.HoldBy;
            target.HoldByUsers = source.HoldByUsers;
            target.HoldUntilTimes = source.HoldUntilTimes;
            target.AppendHolds_Array = source.AppendHolds_Array;
            target.DisposalDueDate = source.DisposalDueDate;
        }

        private static string FormatCreatedBy(string owner)
        {
            if (string.IsNullOrWhiteSpace(owner)) return owner;

            if (!owner.Contains('\\')) return owner;

            var split = owner.Split('\\');
            if (split.Length < 2) return owner;

            var domain = split[0];
            var username = split[1];

            bool isMixedCase = username.Any(char.IsUpper) && username.Any(char.IsLower);
            return isMixedCase ? owner : $"{domain}\\{username.ToLowerInvariant()}";
        }
    }
}
