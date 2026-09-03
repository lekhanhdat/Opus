using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Storage;
using AvePoint.RA.Common.Tracking.Performance;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Services;
using RAFileSystemCore.ApiClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Jpmc.DataSync
{
    public class RMFileSystemClassCodeProcessor
    {
        private readonly AveLogger _logger = AveLogger.GetInstance(typeof(RMFileSystemClassCodeProcessor));

        private readonly RMFileSystemJobProgressTracker _jobProgressTracker;

        private readonly RMFileSystemRecoveryProcessor _recoveryProcessor;

        private readonly RMFileSystemJobExecutionInfo _executeNodeInfo;

        private readonly Dictionary<Guid, List<Rule>> _termRules;

        private readonly ConcurrentDictionary<Guid, RMFileSystemClassCode> _classCodeCache = new ConcurrentDictionary<Guid, RMFileSystemClassCode>();

        private readonly ConcurrentDictionary<string, RMFileSystemClassCodeUnitInfo> _classCodeUnitInfoCache = new ConcurrentDictionary<string, RMFileSystemClassCodeUnitInfo>();

        public RMFileSystemClassCodeProcessor(
            RMFileSystemJobProgressTracker jobProgressTracker,
            RMFileSystemRecoveryProcessor recoveryProcessor,
            RMFileSystemJobExecutionInfo executeNodeInfo,
            Dictionary<Guid, List<Rule>> termRules
            )
        {
            _jobProgressTracker = jobProgressTracker;
            _recoveryProcessor = recoveryProcessor;
            _executeNodeInfo = executeNodeInfo;
            _termRules = termRules;
        }

        public async Task<bool> ProcessDirectoryAsync(RMFileSystemDirectoryMetadata directory)
        {
            try
            {
                if (!_classCodeCache.TryGetValue(directory.Id, out var cachedClassCode))
                {
                    cachedClassCode = new RMFileSystemClassCode();

                    if (directory.CurrentRecordInfo != null)
                    {
                        cachedClassCode = RecalculateClassCodeInfo(directory.CurrentRecordInfo, new XDirectoryInfoEx(directory.DirectoryInfo).LastWriteTimeUtc.Ticks);
                    }
                    else
                    {
                        var parentId = directory.ParentId;
                        //only execute once while whole job
                        if (!_classCodeCache.TryGetValue(parentId, out var parentClassCodeInfo))
                        {
                            _logger.Info($"[Classification] Querying parent record for directory: {directory.FullPath.LogBase64()}, parentId: {parentId}");
                            var parentRecord = await HyperHybridAPIClient.Instance.QueryFileSystemRecordAsync(_executeNodeInfo.ConnectionId, parentId).ConfigureAwait(false);
                            if (parentRecord != null && directory.FullPath != _executeNodeInfo.ConnectionPath)
                            {
                                parentClassCodeInfo = new RMFileSystemClassCode
                                {
                                    Id = parentRecord.TermId,
                                    Name = parentRecord.ClassCode,
                                    CountryCode = parentRecord.CountryCode,
                                    RetentionType = parentRecord.RetentionType,
                                    StartDate = parentRecord.StartDate,
                                    PolicyValueUnit = parentRecord.PolicyValueUnit,
                                    PolicyValueNumber = parentRecord.PolicyValueNumber,
                                };
                            }
                            else
                            {
                                _logger.Info($"[Classification] No parent record found for directory: {directory.FullPath.LogBase64()}, parentId: {parentId}, using node setting class code info");
                                parentClassCodeInfo = _executeNodeInfo.ClassCodeInfo.Clone();
                            }
                        }
                        cachedClassCode = parentClassCodeInfo.Clone();
                        cachedClassCode.EndTime = CalculateEndTime(cachedClassCode.StartDate, new XDirectoryInfoEx(directory.DirectoryInfo).LastWriteTimeUtc.Ticks, (RMFileSystemClassCodeRetentionType)cachedClassCode.RetentionType, new RMFileSystemClassCodeUnitInfo
                        {
                            Unit = cachedClassCode.PolicyValueNumber,
                            UnitType = (PolicyValueUnit)cachedClassCode.PolicyValueUnit,
                        });
                    }

                    _classCodeCache[directory.Id] = cachedClassCode;
                }

                directory.ClassCodeInfo = cachedClassCode;
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"[Classification] Error processing directory: {directory.FullPath.LogBase64()}. Error: {ex}");
                _recoveryProcessor.AddFailedItem(directory);
                _jobProgressTracker.AddFailedJobDetail(directory);
                return false;
            }
        }

        public List<RMFileSystemFileMetadata> ProcessFiles(List<RMFileSystemFileMetadata> files)
        {
            var processedFiles = new List<RMFileSystemFileMetadata>();
            foreach (var file in files)
            {
                if (ProcessFile(file))
                {
                    processedFiles.Add(file);
                }
            }
            return processedFiles;
        }

        private bool ProcessFile(RMFileSystemFileMetadata file)
        {
            try
            {
                if (file.CurrentRecordInfo != null)
                {
                    file.ClassCodeInfo = RecalculateClassCodeInfo(file.CurrentRecordInfo, file.FileInfo.LastWriteTimeUtc.Ticks);
                    return true;
                }

                var parentClassCodeInfo = _classCodeCache[file.ParentId];
                var classCodeInfo = parentClassCodeInfo.Clone();
                if (classCodeInfo.Exists)
                {
                    classCodeInfo.EndTime = CalculateEndTime(classCodeInfo.StartDate, file.FileInfo.LastWriteTimeUtc.Ticks, (RMFileSystemClassCodeRetentionType)classCodeInfo.RetentionType, new RMFileSystemClassCodeUnitInfo
                    {
                        Unit = classCodeInfo.PolicyValueNumber,
                        UnitType = (PolicyValueUnit)classCodeInfo.PolicyValueUnit,
                    });
                }

                file.ClassCodeInfo = classCodeInfo;
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"[Classification] Error processing file: {file.FullPath.LogBase64()}. Error: {ex}");
                _recoveryProcessor.AddFailedItem(file);
                _jobProgressTracker.AddFailedJobDetail(file);
                return false;
            }
        }

        private RMFileSystemClassCode RecalculateClassCodeInfo(FileSystemRecordDto record, long lastModifiedTime)
        {
            var classCode = new RMFileSystemClassCode
            {
                Id = record.TermId,
                Name = record.ClassCode,
                CountryCode = record.CountryCode,
                RetentionType = record.RetentionType,
                StartDate = record.StartDate,
                EndTime = record.EndTime,
                PolicyValueUnit = record.PolicyValueUnit,
                PolicyValueNumber = record.PolicyValueNumber,
            };

            var ruleChanged = record.TermId != Guid.Empty &&
                        _termRules.TryGetValue(record.TermId, out var rules) &&
                        rules.Any(rule => rule.ModifyTime > record.CollectionTime);
            var recordModified = record.TimeLastModified < lastModifiedTime;
            var needRecalculateEndTime = ruleChanged || recordModified;

            if (needRecalculateEndTime)
            {
                _logger.Info($"[Classification] Need recalculate end time for record: {record.NodeId}, termId: {record.TermId}, retentionType: {record.RetentionType}, countryCode: {record.CountryCode}, rule changed: {ruleChanged}, record modified: {recordModified}");
                var unitInfo = GetRetentionUnitInfo(record.TermId, record.RetentionType, record.CountryCode);
                var endTime = CalculateEndTime(record.StartDate, lastModifiedTime, (RMFileSystemClassCodeRetentionType)record.RetentionType, unitInfo);

                if(unitInfo != null)
                {
                    classCode.PolicyValueUnit = (int)unitInfo.UnitType;
                    classCode.PolicyValueNumber = unitInfo.Unit;
                }

                classCode.EndTime = endTime;
            }

            return classCode;
        }

        private long CalculateEndTime(long startDate, long modifiedTime, RMFileSystemClassCodeRetentionType retentionType, RMFileSystemClassCodeUnitInfo unitInfo)
        {
            if (unitInfo == null || !_executeNodeInfo.EnabledRecordManagement)
            {
                return 0;
            }

            var needCalculateTimeTicks = retentionType == RMFileSystemClassCodeRetentionType.Event ? startDate : modifiedTime;
            if (needCalculateTimeTicks == 0)
            {
                _logger.Warn($"[Classification] Need calculate time ticks is 0 for retentionType: {retentionType}, unitInfo: {unitInfo.Unit} {unitInfo.UnitType}");
                return 0;
            }

            var needCalculateDateTime = new DateTime(needCalculateTimeTicks);

            DateTime? calculatedDateTime = unitInfo.UnitType switch
            {
                PolicyValueUnit.Days => needCalculateDateTime.AddDays(unitInfo.Unit),
                PolicyValueUnit.Weeks => needCalculateDateTime.AddDays(unitInfo.Unit * 7),
                PolicyValueUnit.Months => needCalculateDateTime.AddMonths(unitInfo.Unit),
                PolicyValueUnit.Years => needCalculateDateTime.AddYears(unitInfo.Unit),
                _ => null
            };

            if (!calculatedDateTime.HasValue)
            {
                _logger.Warn($"[Classification] Calculated date time is null for retentionType: {retentionType}, unitInfo: {unitInfo.Unit} {unitInfo.UnitType}");
                return 0;
            }

            return calculatedDateTime.Value.Ticks;
        }

        private RMFileSystemClassCodeUnitInfo GetRetentionUnitInfo(Guid termId, int retentionType, string countryCode)
        {
            var classCodeUnitInfoCacheKey = $"{termId}_{retentionType}_{countryCode}".ToLowerInvariant();

            var unitInfo = _classCodeUnitInfoCache.GetOrAdd(classCodeUnitInfoCacheKey, (key) =>
            {
                if (!_termRules.TryGetValue(termId, out var rules))
                {
                    _logger.Warn($"[Classification] No rules found for termId: {termId}");
                    return null;
                }

                var availableRules = rules.Where((rule) =>
                {
                    var filters = rule?.FSRule?.Filters;
                    if (filters == null) return false;

                    var countryCodeCriteriaValue = filters.FirstOrDefault(f =>
                        (f.Condition == PolicyCondition.ListIn || f.Condition == PolicyCondition.Equals) && f.Rule is ColumnTextRule)
                        ?.Value?.Value1;

                    var retentionTypeCriteriaValue = filters.FirstOrDefault(f =>
                        f.Condition == PolicyCondition.Equals && f.Rule is ColumnTextRule && string.Equals(f.Rule?.Value1, "[RetentionType]", StringComparison.OrdinalIgnoreCase))
                        ?.Value?.Value1;

                    if (!(retentionTypeCriteriaValue.EqualsIgnoreCase("Event") || retentionTypeCriteriaValue.EqualsIgnoreCase("Flat")))
                    {
                        _logger.Warn($"[Classification] RetentionType criteria value is not 'Event' or 'Flat' for termId: {termId}, rule: {rule.Id}, retentionType: {retentionType}, countryCode: {countryCode}");
                        return false;
                    }

                    if(!retentionTypeCriteriaValue.EqualsIgnoreCase(((RMFileSystemClassCodeRetentionType)retentionType).ToString()))
                    {
                        _logger.Warn($"[Classification] RetentionType criteria value '{retentionTypeCriteriaValue}' does not match retentionType '{((RMFileSystemClassCodeRetentionType)retentionType)}' for termId: {termId}, rule: {rule.Id}, retentionType: {retentionType}, countryCode: {countryCode}");
                        return false;
                    }

                    var countryCodes = countryCodeCriteriaValue?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => c.Trim())
                        .ToList();

                    if (countryCodes == null || countryCodes.Count == 0)
                    {
                        _logger.Warn($"[Classification] No country codes found in criteria for termId: {termId}, rule: {rule.Id}, retentionType: {retentionType}, countryCode: {countryCode}");
                        return false;
                    }

                    return countryCodes.Contains(countryCode);
                }).ToList();

                if (availableRules.Count != 1)
                {
                    _logger.Warn($"[Classification] Expected exactly 1 available rule, but found {availableRules.Count} for termId: {termId}, retentionType: {retentionType}, countryCode: {countryCode}");
                    return null;
                }

                var matchedRule = availableRules.First();
                _logger.Info($"[Classification] Matched rule for termId: {termId}, retentionType: {retentionType}, countryCode: {countryCode} is rule: {matchedRule.Id}");

                var datetimeTypeCriterias = matchedRule.FSRule.Filters.Where(f =>
                    f.Condition == PolicyCondition.OlderThan &&
                    (f.Rule is ColumnDateTimeRule || f.Rule is ModifiedRule))
                    ?.ToList();

                if (datetimeTypeCriterias.Count != 1)
                {
                    _logger.Warn($"[Classification] Expected exactly 1 datetime criteria, but found {datetimeTypeCriterias.Count} for rule: {matchedRule.Id}");
                    return null;
                }

                var matchedDatetimeCriteria = datetimeTypeCriterias.First();
                return new RMFileSystemClassCodeUnitInfo
                {
                    Unit = matchedDatetimeCriteria.Value?.Value1?.ToInt32() ?? 0,
                    UnitType = matchedDatetimeCriteria.Value.Value1Unit,
                };
            });

            return unitInfo;
        }
    }
}
