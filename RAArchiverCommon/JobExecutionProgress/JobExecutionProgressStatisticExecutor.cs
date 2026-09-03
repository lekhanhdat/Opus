using AvePoint.GCommon.Contract.Server.Common.Performance;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using System.Collections.Concurrent;

namespace AvePoint.RA.SharePoint.Common.JobExecutionProgress
{
    public class JobExecutionProgressStatisticExecutor : IDisposable
    {
        private static readonly IRALogger _logger = RALogger.GetInstance(typeof(JobExecutionProgressStatisticExecutor));

        private readonly static object _lock = new();
        private readonly ConcurrentDictionary<string, JMArchiverJobProgressDetails> _progressStatisticsMap = new();

        private string? _currentSubJobId;
        private BaseJobDto? _mainJobInfo;
        private DateTime _lastSaveTime = DateTime.MinValue;

        private bool _isInitSaveProgressInterval = false;
        private int _saveProgressIntervalInSeconds = 60;
        private Timer? _saveProgressTimer;

        private string? _originalJobId;
        private string? _teamsOriginalJobId;

        private bool _isCGScanner = false;

        private readonly IJobProgressDao _jobProgressDao = PlatformWindsorManager.GetService<IJobProgressDao>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();

        private static JobExecutionProgressStatisticExecutor? _instance;
        public static JobExecutionProgressStatisticExecutor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new JobExecutionProgressStatisticExecutor();
                    }
                }
                return _instance;
            }
        }

        private JMArchiverJobProgressDetails? _progressStatictics => !string.IsNullOrEmpty(_currentSubJobId) && _progressStatisticsMap.TryGetValue(_currentSubJobId, out var stats) ? stats : null;
        public JMArchiverJobProgressDetails GetCurrentProgressDetails()
        {
            return _progressStatictics;
        }

        public void InitializeJobExecutionProgressStatictics(string scope, string subJobId, string mainJobId, int jobType, bool isInitStartTime = false, bool isTeams = false)
        {
            _logger.Info($"InitializeJobExecutionProgressStatictics with scope: {scope}, subJobId: {subJobId}, mainJobId: {mainJobId}, jobType: {(JobType)jobType}.");

            _currentSubJobId = subJobId;
            var stats = GetOrCreateProgressStats(subJobId);

            stats.SubJobID = subJobId;
            stats.Scope = scope;
            stats.JobType = (JobType)jobType;
            if (isInitStartTime)
            {
                stats.StartTime = DateTime.UtcNow;
                stats.Status = JobStatus.InProgress;
                InitSaveProgressInterval();
            }
            stats.ProgressStatus = ProgressStatus.Pending;
            stats.Comment = string.Empty;

            _mainJobInfo = new BaseJobDto()
            {
                Id = mainJobId,
                JobType = jobType,
                IsMainJob = true,
            };

            if (isTeams && string.IsNullOrEmpty(_teamsOriginalJobId))
            {
                _teamsOriginalJobId = subJobId;
            }
            else if (!isTeams && string.IsNullOrEmpty(_originalJobId))
            {
                _originalJobId = subJobId;
            }

            var isProgressExist = _jobProgressDao.GetJobProgressBySubJobIdAsync(subJobId).ExecuteAsyncTask() is not null;
            if (!isProgressExist)
            {
                _jobProgressDao.AddJobProgressAsync(ConvertUtil.ConvertToJobProgressTableEntity(stats)).ExecuteAsyncTask();
            }
        }

        private void InitSaveProgressInterval()
        {
            if (_isInitSaveProgressInterval)
            {
                return;
            }
            var rmKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
            _saveProgressIntervalInSeconds = rmKeyValueDao.GetSaveProgressIntervalInSeconds();
            _logger.Info($"InitSaveProgressInterval with saveProgressIntervalInSeconds: {_saveProgressIntervalInSeconds}.");
            _isInitSaveProgressInterval = true;

            StartRecalculateTimer();
        }

        private void StartRecalculateTimer()
        {
            _saveProgressTimer?.Dispose();
            _saveProgressTimer = new Timer(
                _ => RecalculateCurrentStageEstimatedFinishedTime(),
                null,
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMinutes(5));
        }

        private void RecalculateCurrentStageEstimatedFinishedTime()
        {
            try
            {
                _logger.Info($"Start recalculating current stage estimated finished time for mainJobId: {_mainJobInfo?.Id}, subJobId: {_currentSubJobId}.");
                _progressStatictics?.RecalculateCurrentStageEstimatedFinishedTime();
                _logger.Info($"Finished recalculating current stage estimated finished time for mainJobId: {_mainJobInfo?.Id}, subJobId: {_currentSubJobId}.");
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to recalculate current stage estimated finished time for mainJobId: {_mainJobInfo?.Id}, subJobId: {_currentSubJobId}.", ex);
            }
        }

        private JMArchiverJobProgressDetails GetOrCreateProgressStats(string subJobId)
        {
            if (!_progressStatisticsMap.TryGetValue(subJobId, out var stats))
            {
                stats = new JMArchiverJobProgressDetails();
                _progressStatisticsMap.TryAdd(subJobId, stats);
            }
            return stats;
        }

        public void ResetJobId(bool isTeams = false)
        {
            if (string.IsNullOrEmpty(_currentSubJobId))
            {
                return;
            }
            _logger.Info($"ResetJobId with isTeams: {isTeams}, currentSubJobId: {_currentSubJobId}, originalJobId: {_originalJobId}, teamsOriginalJobId: {_teamsOriginalJobId}.");
            if (isTeams && !string.IsNullOrEmpty(_teamsOriginalJobId))
            {
                _currentSubJobId = _teamsOriginalJobId;
                _teamsOriginalJobId = string.Empty;
            }
            else if (!isTeams && !string.IsNullOrEmpty(_originalJobId))
            {
                _currentSubJobId = _originalJobId;
                _originalJobId = string.Empty;
            }
        }

        private void SaveProgressIfNeeded()
        {
            if (DateTime.UtcNow - _lastSaveTime < TimeSpan.FromSeconds(_saveProgressIntervalInSeconds))
            {
                return;
            }

            SaveProgress();
            _lastSaveTime = DateTime.UtcNow;
        }

        public void SaveProgress()
        {
            lock (_lock)
            {
                try
                {
                    if (_progressStatictics is not null)
                    {
                        _logger.Info($"SaveProgress with scope: {_progressStatictics.Scope}, subJobId: {_progressStatictics.SubJobID}, mainJobId: {_mainJobInfo?.Id}, jobType: {_mainJobInfo?.JobType}, progressStatus: {_progressStatictics.ProgressStatus}.");
                        _logger.Info(_progressStatictics.ToString());
                        _progressStatictics.LastUpdatedTime = DateTime.UtcNow;
                        //_jobDetailService.SyncJobDetails([_progressStatictics], _mainJobInfo);
                        _jobProgressDao.UpdateJobProgressAsync(ConvertUtil.ConvertToJobProgressTableEntity(_progressStatictics)).ExecuteAsyncTask();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to save progress for mainJobId: {_mainJobInfo?.Id}, subJobId: {_currentSubJobId}.", ex);
                }
            }
        }

        public void SaveOriginalJobProgress()
        {
            _logger.Info($"SaveOrignalJobProgress with mainJobId: {_mainJobInfo?.Id}, originalJobId: {_originalJobId}, teamsOriginalJobId: {_teamsOriginalJobId}.");
            lock (_lock)
            {
                if (!string.IsNullOrEmpty(_originalJobId) && _progressStatisticsMap.TryGetValue(_originalJobId, out var originalStats))
                {
                    try
                    {
                        //_jobDetailService.SyncJobDetails([originalStats], _mainJobInfo);
                        originalStats.LastUpdatedTime = DateTime.UtcNow;
                        _jobProgressDao.UpdateJobProgressAsync(ConvertUtil.ConvertToJobProgressTableEntity(originalStats)).ExecuteAsyncTask();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to save original job progress for mainJobId: {_mainJobInfo?.Id}, originalJobId: {_originalJobId}.", ex);
                    }
                }
                if (!string.IsNullOrEmpty(_teamsOriginalJobId) && _progressStatisticsMap.TryGetValue(_teamsOriginalJobId, out var teamsOriginalStats))
                {
                    try
                    {
                        //_jobDetailService.SyncJobDetails([teamsOriginalStats], _mainJobInfo);
                        teamsOriginalStats.LastUpdatedTime = DateTime.UtcNow;
                        _jobProgressDao.UpdateJobProgressAsync(ConvertUtil.ConvertToJobProgressTableEntity(teamsOriginalStats)).ExecuteAsyncTask();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to save teams original job progress for mainJobId: {_mainJobInfo?.Id}, teamsOriginalJobId: {_teamsOriginalJobId}.", ex);
                    }
                }
            }
        }

        public void UpdateJobStatus(JobStatus status)
        {
            if (_progressStatictics is null) return;
            _progressStatictics.Status = status;
            SaveProgressIfNeeded();
        }

        public void IncreaseTotalFiles(long totalFiles)
        {
            if (_progressStatictics is null) return;
            _progressStatictics.TotalFiles += totalFiles;
            SaveProgressIfNeeded();
        }

        public void ReCalculateRemainingMatchedRuleFiles(long exportFiles, long archiveFiles, long otherActionsFiles)
        {
            if (_progressStatictics is null) return;
            if (!string.IsNullOrEmpty(_originalJobId) && _progressStatisticsMap.TryGetValue(_originalJobId, out var stats))
            {
                _progressStatictics.TotalMatchedRuleFilesForExport += Math.Min(exportFiles, stats.TotalMatchedRuleFilesForExport);
                stats.TotalMatchedRuleFilesForExport -= Math.Min(exportFiles, stats.TotalMatchedRuleFilesForExport);
                _progressStatictics.TotalMatchedRuleFilesForArchive += Math.Min(archiveFiles, stats.TotalMatchedRuleFilesForArchive);
                stats.TotalMatchedRuleFilesForArchive -= Math.Min(archiveFiles, stats.TotalMatchedRuleFilesForArchive);
                _progressStatictics.TotalMatchedRuleFilesForOtherActions += Math.Min(otherActionsFiles, stats.TotalMatchedRuleFilesForOtherActions);
                stats.TotalMatchedRuleFilesForOtherActions -= Math.Min(otherActionsFiles, stats.TotalMatchedRuleFilesForOtherActions);
                SaveProgress();
            }
        }

        public void EnableCGScanner()
        {
            _logger.Info($"EnableCGScanner for mainJobId: {_mainJobInfo?.Id}, subJobId: {_currentSubJobId}.");
            _isCGScanner = true;
        }

        public void StartProgressForScan()
        {
            if (_progressStatictics is null) return;
            if (_progressStatictics.StartScanTime == DateTime.MinValue)
            {
                _progressStatictics.StartScanTime = DateTime.UtcNow;
            }
            _progressStatictics.ProgressStatus = ProgressStatus.Scan;
            UpdateOriginalJobProgressStatus(ProgressStatus.Scan);
            SaveProgress();
        }

        public void StartProgressForExport()
        {
            if (_progressStatictics is null) return;
            if (_progressStatictics.StartExportTime == DateTime.MinValue)
            {
                _progressStatictics.StartExportTime = DateTime.UtcNow;
            }
            _progressStatictics.ProgressStatus = ProgressStatus.Export;
            UpdateOriginalJobProgressStatus(ProgressStatus.Export);
            SaveProgress();
        }

        public void StartProgressForArchived()
        {
            if (_progressStatictics is null) return;
            if (_progressStatictics.StartArchivedTime == DateTime.MinValue)
            {
                _progressStatictics.StartArchivedTime = DateTime.UtcNow;
            }
            _progressStatictics.ProgressStatus = ProgressStatus.Archive;
            UpdateOriginalJobProgressStatus(ProgressStatus.Archive);
            SaveProgress();
        }

        public void StartProgressForOther()
        {
            if (_progressStatictics is null) return;
            if (_progressStatictics.StartOtherTime == DateTime.MinValue)
            {
                _progressStatictics.StartOtherTime = DateTime.UtcNow;
            }
            _progressStatictics.ProgressStatus = ProgressStatus.Others;
            UpdateOriginalJobProgressStatus(ProgressStatus.Others);
            SaveProgress();
        }

        private void UpdateOriginalJobProgressStatus(ProgressStatus progressStatus)
        {
            bool needSave = false;
            if (string.IsNullOrEmpty(_currentSubJobId))
            {
                return;
            }
            if (!string.IsNullOrEmpty(_originalJobId) && !_currentSubJobId.Equals(_originalJobId) && _progressStatisticsMap.TryGetValue(_originalJobId, out var stats))
            {
                needSave = true;
                stats.ProgressStatus = progressStatus;
            }
            if (!string.IsNullOrEmpty(_teamsOriginalJobId) && !_currentSubJobId.Equals(_teamsOriginalJobId) && _progressStatisticsMap.TryGetValue(_teamsOriginalJobId, out var teamsStats))
            {
                needSave = true;
                teamsStats.ProgressStatus = progressStatus;
            }
            if (needSave)
            {
                SaveOriginalJobProgress();
            }
        }

        public void IncreaseScannedFiles(int scannedFiles = 1)
        {
            if (_progressStatictics is null || _isCGScanner) return;
            _progressStatictics.IncreaseScannedFiles(scannedFiles);
            SaveProgressIfNeeded();
        }

        public void IncreaseExportedFiles(long fileSize)
        {
            if (_progressStatictics is null) return;
            _progressStatictics.IncreaseExportedFiles(fileSize);
            SaveProgressIfNeeded();
        }

        public void IncreaseArchivedFiles(long fileSize)
        {
            if (_progressStatictics is null) return;
            _progressStatictics.IncreaseArchivedFiles(fileSize);
            SaveProgressIfNeeded();
        }

        public void IncreaseOtherActions()
        {
            if (_progressStatictics is null) return;
            _progressStatictics.IncreaseOtherActions();
            SaveProgressIfNeeded();
        }

        public void IncreaseOtherItems(ActionTab action, int cacheNodeType, long fileSize)
        {
            if (_progressStatictics is null) return;
            if (action == ActionTab.Scan && _isCGScanner) return;
            _progressStatictics.IncreaseOtherItems(action, cacheNodeType, fileSize);
            SaveProgressIfNeeded();
        }

        public void IncreaseTotalMatchedRuleFiles(bool isCountForExport = false, bool isCountForArchive = false, bool isCountForOtherAction = true)
        {
            if (_progressStatictics is null) return;
            if (isCountForExport)
            {
                _progressStatictics.TotalMatchedRuleFilesForExport++;
            }
            if (isCountForArchive)
            {
                _progressStatictics.TotalMatchedRuleFilesForArchive++;
            }
            if (isCountForOtherAction)
            {
                _progressStatictics.TotalMatchedRuleFilesForOtherActions++;
            }
            SaveProgressIfNeeded();
        }

        public void DecreaseTotalMatchedRuleFiles(bool isCountForExport = false, bool isCountForArchive = false, bool isCountForOtherAction = true)
        {
            if (_progressStatictics is null) return;
            if (isCountForExport && _progressStatictics.TotalMatchedRuleFilesForExport > 0)
            {
                _progressStatictics.TotalMatchedRuleFilesForExport--;
                _progressStatictics.CalculateEstimatedExportFinishedTime();
            }
            if (isCountForArchive && _progressStatictics.TotalMatchedRuleFilesForArchive > 0)
            {
                _progressStatictics.TotalMatchedRuleFilesForArchive--;
                _progressStatictics.CalculateEstimatedArchivedFinishedTime();
            }
            if (isCountForOtherAction && _progressStatictics.TotalMatchedRuleFilesForOtherActions > 0)
            {
                _progressStatictics.TotalMatchedRuleFilesForOtherActions--;
                _progressStatictics.CalculateEstimatedOtherFinishedTime();
            }
            SaveProgressIfNeeded();
        }

        public void FinishProgress(JobStatus jobStatus = JobStatus.Finished)
        {
            try
            {
                if (_progressStatictics is not null)
                {
                    _logger.Info($"FinishProgress with mainJobId: {_mainJobInfo?.Id}, subJobId: {_currentSubJobId}.");
                    _progressStatictics.FinishTime = DateTime.UtcNow;
                    _progressStatictics.ProgressStatus = JobReportUtility.ConvertJobStatusToProgressStatus(jobStatus);
                    SaveProgress();
                    UpdateExtensionForTeamsSite();
                    RemoveProgress();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to finish progress for mainJobId: {_mainJobInfo?.Id}, subJobId: {_currentSubJobId}.", ex);
            }
        }

        public void RemoveProgress()
        {
            lock (_lock)
            {
                if (_progressStatictics is not null && !string.IsNullOrEmpty(_currentSubJobId))
                {
                    _progressStatisticsMap.TryRemove(_currentSubJobId, out _);
                }
            }
        }
        public void UpdateExtensionForTeamsSite()
        {
            string oldJobExtentionJson = null;
            string newJobExtensionJson = null;
            JobExtension newJobExtension = null;
            _logger.Info(@$"Starting Update Extension before remove job, mainJobId: {_mainJobInfo?.Id}, subJobId: {_currentSubJobId}.");
            if (_currentSubJobId != null && _mainJobInfo?.Id != null)
            {
                _logger.Info(@$"--JobExecutionProgressStatisticExecutor --Update Extension, mainJobId: {_mainJobInfo?.Id}, subJobId: {_currentSubJobId}.");
                try
                {
                    oldJobExtentionJson = RMJobService.GetJobExtension(_mainJobInfo?.Id);
                    newJobExtension = SerializerHelper.DeserializeByJsonConvert<JobExtension>(oldJobExtentionJson);
                }
                catch (Exception ex)
                {
                    _logger.Info(@$"--JobExecutionProgressStatisticExecutor --FinishProgress Deserialize job extention fail, oldJobExtention:{oldJobExtentionJson}, mainJobId :{_mainJobInfo?.Id}");
                }
                if(newJobExtension == null || _progressStatictics == null)
                {
                    _logger.Warn(@$"--JobExecutionProgressStatisticExecutor, newJobExtension is null or _progressStatictics is null, main job Id:{_mainJobInfo?.Id}, sub job id:{_currentSubJobId}");
                    return;
                }
                if (_progressStatictics != null)
                {
                    _logger.Info($@"SaveJobExecution...ProcessedArchivedItemsInfo: {_progressStatictics.ProcessedArchivedItemsInfo}");
                    newJobExtension.SOProgressFileAndSCCount.TotalArchivedSize += (_progressStatictics.ProcessedArchivedItemsInfo.TotalSize + _progressStatictics.ProcessedArchivedItemsInfo.ItemSize);
                    newJobExtension.SOProgressFileAndSCCount.ProgressedSCCount += 1;
                }
                try
                {
                    //For SPO Calculate ETA FinishTime
                    var mainJob = RMJobService.GetJobMonitorStatisDto(_mainJobInfo?.Id);
                    var timeStartCal = DateTime.UtcNow;
                    if (mainJob != null && mainJob.StartTime > 0 && newJobExtension.SOProgressFileAndSCCount != null)
                    {
                        int totalSCCount = newJobExtension.SOProgressFileAndSCCount.AllSCCount;
                        int processedSCCount = newJobExtension.SOProgressFileAndSCCount.ProgressedSCCount;
                        _logger.Info(@$"SOProgressScAndFileStatistic -----,totalSCCount:{totalSCCount}, processedSCCount:{processedSCCount}");
                        if (totalSCCount > 0 && processedSCCount > 0)
                        {
                            double mainJobElapsedSeconds = (timeStartCal - new DateTime(mainJob.StartTime, DateTimeKind.Utc)).TotalSeconds;
                            _logger.Info(@$"SOProgressScAndFileStatistic -----,mainJobElapsedSeconds:{mainJobElapsedSeconds}");
                            if (mainJobElapsedSeconds > 0)
                            {
                                int remainingSC = totalSCCount - processedSCCount;
                                _logger.Info(@$"SOProgressScAndFileStatistic -----,remainingSC:{remainingSC}");
                                if (remainingSC >= 0)
                                {
                                    double scPerSecond = processedSCCount / mainJobElapsedSeconds;
                                    double estimatedRemainingSeconds = remainingSC / scPerSecond;
                                    newJobExtension.SOProgressFileAndSCCount.EstimatedFinishTimeTicks = timeStartCal.AddSeconds(estimatedRemainingSeconds).Ticks;
                                    _logger.Info(@$"SOProgressScAndFileStatistic -----,EstimatedFinishTimeTicks:{newJobExtension.SOProgressFileAndSCCount.EstimatedFinishTimeTicks}");
                                }
                            }
                        }
                    }
                }
                catch (Exception etaEx)
                {
                    _logger.Warn($@"Failed to calculate EstimatedFinishTimeTicks, error: {etaEx.Message}");
                }
                newJobExtensionJson = SerializerHelper.SerializeByJsonConvert(newJobExtension);
                
                try
                {
                    RMJobService.AtomicityUpdateJobExtension(_mainJobInfo?.Id, oldJobExtentionJson, newJobExtensionJson);
                }
                catch (Exception ex)
                {
                    _logger.Info(@$"--JobExecutionProgressStatisticExecutor --FinishProgress AtomicityUpdateJobExtension fail, newJobExtensionJson:{newJobExtensionJson}, oldExtension: {oldJobExtentionJson}, mainJobId :{_mainJobInfo?.Id}");
                }

            }
        }
        public void Dispose()
        {
            _logger.Info($"Disposing JobExecutionProgressStatisticExecutor for mainJobId: {_mainJobInfo?.Id}, subJobId: {_currentSubJobId}.");
            lock (_lock)
            {
                try
                {
                    _saveProgressTimer?.Dispose();
                    _saveProgressTimer = null;
                    _progressStatisticsMap.Clear();
                    _instance = null;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to dispose JobExecutionProgressStatisticExecutor for mainJobId: {_mainJobInfo?.Id}, subJobId: {_currentSubJobId}.", ex);
                }
            }
            _logger.Info($"Disposed JobExecutionProgressStatisticExecutor for mainJobId: {_mainJobInfo?.Id}, subJobId: {_currentSubJobId}.");
        }
    }
}
