using AvePoint.GCommon.GraphAPI;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Myhub.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RAPhysical.Disposal.PhysicalDisposalActionImps;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RACloudFS.Util
{
    public class FSDashboardAggregator
    {
        private const int MaxTopEntries = 10;
        private readonly ConcurrentDictionary<string, NodeAggregation> _nodes = new();
        private string _lastDirPath;
        private string _rootPath;

        public void SetRootPath(string rootPath)
        {
            _rootPath = rootPath?.TrimEnd('\\', '/');
        }

        public void Accumulate(Record record)
        {
            var dirPath = record.DirPath;
            if (string.IsNullOrEmpty(dirPath))
            {
                return;
            }

            var node = GetOrCreateNode(dirPath);
            node.Accumulate(record);
            _lastDirPath = dirPath;
        }

        public Dictionary<string, FSDashboard> BuildResults()
        {
            PerformFinalRollup();
            var results = new Dictionary<string, FSDashboard>(_nodes.Count);

            foreach (var kvp in _nodes)
            {
                results[kvp.Key] = kvp.Value.ToResult();
            }

            return results;
        }

        private void PerformFinalRollup()
        {
            var sortedPaths = _nodes.Keys
                .OrderByDescending(p => p.Count(c => c == '\\' || c == '/'))
                .ThenByDescending(p => p.Length)
                .ToList();

            foreach (var path in sortedPaths)
            {
                MergeIntoParent(path);
            }
        }

        public void Reset()
        {
            _nodes.Clear();
            _lastDirPath = null;
        }

        private bool IsAtOrAboveRootPath(string path)
        {
            if (string.IsNullOrEmpty(_rootPath))
            {
                return false;
            }

            var trimmedPath = path.TrimEnd('\\', '/');
            return _rootPath.StartsWith(trimmedPath, StringComparison.OrdinalIgnoreCase)
                && trimmedPath.Length < _rootPath.Length;
        }

        private void MergeIntoParent(string childPath)
        {
            var parentPath = GetParentPath(childPath);
            if (string.IsNullOrEmpty(parentPath))
            {
                return;
            }

            if (IsAtOrAboveRootPath(parentPath))
            {
                return;
            }

            if (!_nodes.TryGetValue(childPath, out var childNode))
            {
                return;
            }

            var parentNode = GetOrCreateNode(parentPath);
            parentNode.MergeFrom(childNode);
        }

        private NodeAggregation GetOrCreateNode(string dirPath)
        {
            if (!_nodes.TryGetValue(dirPath, out var node))
            {
                node = new NodeAggregation();
                _nodes[dirPath] = node;
            }
            return node;
        }

        internal static string GetParentPath(string dirPath)
        {
            if (string.IsNullOrEmpty(dirPath))
            {
                return null;
            }

            var trimmed = dirPath.TrimEnd('\\', '/');
            var lastSep = trimmed.LastIndexOfAny(['\\', '/']);
            if (lastSep <= 0)
            {
                return null;
            }

            var parent = trimmed[..lastSep];
            return parent.TrimEnd('\\', '/').Length == 0 ? null : parent;
        }


        private class NodeAggregation
        {
            private const int FolderNodeType = (int)RMNodeLevel.FSFolder; // 2100
            private const int FileNodeType = (int)RMNodeLevel.FSFile;     // 2200

            private const int ActiveRecordStatus = (int)RMRecordStatus.Active; 

            private const int DestroyedRecordStatus = (int)RMRecordStatus.Destroyed;

            private long _totalSize;
            private int _totalCount;

            private long _folderActive;
            private long _folderDestroyed;
            private long _folderTotal;
            private long _fileActive;
            private long _fileDestroyed;
            private long _fileTotal;

            public record struct TrendEntry(int c, int m, int a);
            public readonly record struct EntryStats(long Size, int Count);
            private readonly Dictionary<string, (long size, int count)> _fileTypes = new(MaxTopEntries + 5, StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, (long size, int count)> _creators = new(MaxTopEntries + 5, StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<long, TrendEntry> _lineChars = new();
            private readonly Dictionary<(string termId, string classCode), (long size, int count)> _classCodes = new();
            private readonly Dictionary<long, long> _destroyedStats = new();

            public void Accumulate(Record record)
            {
                var nodeType = record.NodeType;
                var recordStatus = record.RecordStatus;
                var destroyedTime = record.DestroyedTime;

                UpdateStatusSummaries(nodeType, recordStatus);
                if (recordStatus == DestroyedRecordStatus)
                {
                    AccumulateDestroyed(record);
                }
                if (nodeType != FileNodeType || recordStatus != ActiveRecordStatus)
                {
                    return;
                }

                AccumulateActiveFile(record);
            }

            private void UpdateStatusSummaries(int nodeType, int recordStatus)
            {
                if (nodeType == FolderNodeType)
                {
                    _folderTotal++;
                    if (recordStatus == ActiveRecordStatus)
                    {
                        _folderActive++;
                    }
                    else if (recordStatus == DestroyedRecordStatus)
                    {
                        _folderDestroyed++;
                    }
                }
                else if (nodeType == FileNodeType)
                {
                    _fileTotal++;
                    if (recordStatus == ActiveRecordStatus)
                    {
                        _fileActive++;
                    }
                    else if (recordStatus == DestroyedRecordStatus)
                    {
                        _fileDestroyed++;
                    }
                }
            }
            private void AccumulateDestroyed(Record record)
            {
                if (record.DestroyedTime <= 0)
                {
                    return;
                }

                var date = long.Parse(
                    new DateTime(record.DestroyedTime)
                        .ToString("yyyyMMdd"));

                if (_destroyedStats.TryGetValue(date, out var count))
                {
                    _destroyedStats[date] = count + 1;
                }
                else
                {
                    _destroyedStats[date] = 1;
                }
            }
            private void AccumulateActiveFile(Record record)
            {
                var fileSize = record.JPMCFSFileSize;
                _totalSize += fileSize;
                _totalCount++;

                AccumulateFileType(record.ExtensionForFile, fileSize);
                AccumulateCreator(record.CreatedBy, fileSize);
                AccumulateClassCode(record.TermId.ToString(), record.ClassCode, fileSize);

                if (string.IsNullOrEmpty(record.MetaInfo))
                {
                    return;
                }

                var meta = JsonConvert.DeserializeObject<RecordMetaInfo>(record.MetaInfo);
                if (meta != null)
                {
                    AccumulateLineChars(meta);
                }
            }

            public void MergeFrom(NodeAggregation child)
            {
                _totalSize += child._totalSize;
                _totalCount += child._totalCount;

                // Roll up the new status counters.
                _folderActive += child._folderActive;
                _folderDestroyed += child._folderDestroyed;
                _folderTotal += child._folderTotal;
                _fileActive += child._fileActive;
                _fileDestroyed += child._fileDestroyed;
                _fileTotal += child._fileTotal;

                foreach (var kvp in child._fileTypes)
                {
                    if (_fileTypes.TryGetValue(kvp.Key, out var existing))
                    {
                        _fileTypes[kvp.Key] = (existing.size + kvp.Value.size, existing.count + kvp.Value.count);
                    }
                    else
                    {
                        _fileTypes[kvp.Key] = kvp.Value;
                    }
                }

                foreach (var kvp in child._creators)
                {
                    if (_creators.TryGetValue(kvp.Key, out var existing))
                    {
                        _creators[kvp.Key] = (existing.size + kvp.Value.size, existing.count + kvp.Value.count);
                    }
                    else
                    {
                        _creators[kvp.Key] = kvp.Value;
                    }
                }

                foreach (var kvp in child._classCodes)
                {
                    if (_classCodes.TryGetValue(kvp.Key, out var existing))
                    {
                        _classCodes[kvp.Key] = (existing.size + kvp.Value.size, existing.count + kvp.Value.count);
                    }
                    else
                    {
                        _classCodes[kvp.Key] = kvp.Value;
                    }
                }

                foreach (var kvp in child._lineChars)
                {
                    if (_lineChars.TryGetValue(kvp.Key, out var existing))
                    {
                        _lineChars[kvp.Key] = new TrendEntry(existing.c + kvp.Value.c, existing.m + kvp.Value.m, existing.a + kvp.Value.a);
                    }
                    else
                    {
                        _lineChars[kvp.Key] = kvp.Value;
                    }
                }
                foreach (var kvp in child._destroyedStats)
                {
                    if (_destroyedStats.TryGetValue(kvp.Key, out var existing))
                    {
                        _destroyedStats[kvp.Key] = existing + kvp.Value;
                    }
                    else
                    {
                        _destroyedStats[kvp.Key] = kvp.Value;
                    }
                }
            }

            public FSDashboard ToResult()
            {
                return new FSDashboard
                {
                    Storage = new StorageStats
                    {
                        Size = _totalSize,
                        FileCount = _totalCount,
                        TotalSize = _totalSize
                    },
                    FileTypes = _fileTypes
                        .OrderByDescending(x => x.Value.size)
                        .Take(MaxTopEntries)
                        .Select(x => new FileTypeStats { Ext = x.Key, FileCount = x.Value.count, FileSize = x.Value.size })
                        .ToList(),
                    Creators = _creators
                        .OrderByDescending(x => x.Value.count)
                        .Take(MaxTopEntries)
                        .Select(x => new CreatorStats { Creator = x.Key, FileCount = x.Value.count })
                        .ToList(),
                    ClassCodes = _classCodes
                        .OrderByDescending(x => x.Value.count)
                        .Take(MaxTopEntries)
                        .Select(x => new ClassCodeStats
                        {
                            ClassCodeId = x.Key.termId,
                            ClassCodeName = x.Key.classCode,
                            Usage = x.Value.count
                        })
                        .ToList(),
                    ClassCodesTotal = _classCodes.Values.Sum(x => x.count),
                    LineChartData = _lineChars
                        .Select(x => new RecordStats
                        {
                            Date = x.Key,
                            Created = x.Value.c,
                            Modified = x.Value.m,
                            Accessed = x.Value.a
                        })
                        .ToList(),
                    FolderStatusSummary = new StatusSummary
                    {
                        Active = _folderActive,
                        Destroyed = _folderDestroyed,
                        Total = _folderTotal
                    },
                    FileStatusSummary = new StatusSummary
                    {
                        Active = _fileActive,
                        Destroyed = _fileDestroyed,
                        Total = _fileTotal
                    },
                    DestroyedStats = _destroyedStats
                        .OrderBy(x => x.Key)
                        .Select(x => new DestroyedStats
                        {
                            Date = x.Key,
                            Destroyed = x.Value
                        })
                        .ToList(),
                };
            }

            private void AccumulateFileType(string fileType, long size)
            {
                fileType = string.IsNullOrWhiteSpace(fileType) ? "empty" : fileType;
                if (_fileTypes.TryGetValue(fileType, out var entry))
                {
                    _fileTypes[fileType] = (entry.size + size, entry.count + 1);
                }
                else
                {
                    _fileTypes[fileType] = (size, 1);
                }
            }

            private void AccumulateCreator(string creator, long size)
            {
                if (string.IsNullOrEmpty(creator))
                {
                    return;
                }
                if (_creators.TryGetValue(creator, out var entry))
                {
                    _creators[creator] = (entry.size + size, entry.count + 1);
                }
                else
                {
                    _creators[creator] = (size, 1);
                }
            }

            private void AccumulateClassCode(string termId, string classCode, long size)
            {
                var key = (termId, classCode);
                if (_classCodes.TryGetValue(key, out var entry))
                {
                    _classCodes[key] = (entry.size + size, entry.count + 1);
                }
                else
                {
                    _classCodes[key] = (size, 1);
                }
            }

            private void AccumulateLineChars(RecordMetaInfo meta)
            {
                var cutoff = DateTime.UtcNow.AddDays(-180);

                AddTrend(meta.CreatedTime, cutoff, 'c');
                AddTrend(meta.LastModifiedTime, cutoff, 'm');
                AddTrend(meta.LastAccessTime, cutoff, 'a');
            }

            private void AddTrend(long ticks, DateTime cutoffUtc, char type)
            {
                var utc = new DateTime(ticks, DateTimeKind.Utc);

                if (utc < cutoffUtc)
                {
                    return;
                }

                long hourKey = long.Parse(utc.ToString("yyyyMMddHH"));

                UpdateTrend(hourKey, type);
            }
            private void UpdateTrend(long dateKey, char type)
            {
                if (!_lineChars.TryGetValue(dateKey, out var entry))
                {
                    entry = new TrendEntry(0, 0, 0);
                }

                _lineChars[dateKey] = type switch
                {
                    'c' => entry with { c = entry.c + 1 },
                    'm' => entry with { m = entry.m + 1 },
                    'a' => entry with { a = entry.a + 1 },
                    _ => entry
                };
            }
        }
    }
}
