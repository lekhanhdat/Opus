using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Core.Discovery.DBManager.SQLite;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Cache;
using AvePoint.RA.Service.Services.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Calculator;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Calculator.Duplicate;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Telemetry;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.General.Inactive;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.General.Rot;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.Model;
using Cloud.Sdk.Data.IE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Analyzer.V5.General
{
    public class RMDiscoveryOffice365Analyzer : RMDiscoveryOffice365Worker
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365Analyzer));

        private readonly IRMDiscoveryOffice365JobDao _jobDao = new RMDiscoveryOffice365JobDao();
        private readonly IRMDiscoveryOffice365NodeDao _nodeDao = new RMDiscoveryOffice365NodeDao();
        private readonly IRMDiscoveryOffice365ProfileService _profileService = PlatformWindsorManager.GetService<IRMDiscoveryOffice365ProfileService>();
        private readonly IRMReportManager _reportManager;
        private readonly RMDiscoveryOffice365Temeleter _temeleter;
        private readonly List<RMDiscoveryOffice365RuleInfo> _rotRules;
        private readonly List<RMDiscoveryOffice365RuleInfo> _inactiveRules;

        private readonly Dictionary<Guid, TenantRuntimeContext> _tenantContexts = new();
        private bool _initialized;

        public RMDiscoveryOffice365Analyzer(string jobId, IRMReportManager reportManager, RMDiscoveryOffice365Temeleter temeleter, List<RMDiscoveryOffice365RuleInfo> rules) : base()
        {
            _reportManager = reportManager;
            _temeleter = temeleter;
            _rotRules = rules.Where(item => item.DefinitionKind == RMDiscoveryRuleDefinitionKind.ROT).ToList();
            _inactiveRules = rules.Where(item => item.DefinitionKind == RMDiscoveryRuleDefinitionKind.Inactive).ToList();
            ReportMangerFactory.Instance.Init(jobId, JobType.DiscoveryJobV5);
        }

        public async Task InitializeAsync(RMDiscoveryOffice365MainJob mainJob)
        {
            if (_initialized)
            {
                return;
            }

            _logger.Info($"Start analysis [{mainJob.Id}] [{mainJob.Type}] main job.");

            if (mainJob.Type == RMDiscoveryJobType.Newly)
            {
                RMDiscoveryOffice365SQLiteDBManager.CreateDatabase();
                _logger.Info("Successful create sqlite database.");
            }
            else
            {
                await RMDiscoveryOffice365SQLiteDBManager.DownloadDatabaseAsync();
                _logger.Info("Successful download sqlite database from storage.");
            }

            _initialized = true;
        }

        public async Task<bool> TryProcessNextPendingSiteAsync(RMDiscoveryOffice365MainJob mainJob)
        {
            var pendingAnalysisJobs = await _jobDao.GetAnalysisJobsAsync(mainJob.Id, 1, RMDiscoveryJobStatus.Pending);
            var analysisJob = pendingAnalysisJobs.FirstOrDefault();
            if (analysisJob == null)
            {
                return false;
            }

            var discoveryJob = await _jobDao.GetDiscoveryJobAsync(analysisJob.DiscoveryJobId);
            if (discoveryJob == null)
            {
                await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed);
                _reportManager.Increase();
                return true;
            }

            if (analysisJob.FailedCause == RMDiscoveryJobFailedCause.SiteNotFound)
            {
                _logger.Warn($"The site [{analysisJob.SiteId}] discovery job failed cause deleted, skip analysis.");
                await CleanupDeletedSiteDataAsync(discoveryJob.O365TenantId, analysisJob.SiteId);
                await ChangeDiscoveryFailedAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Skipped);
                _reportManager.Increase();
                return true;
            }

            if (analysisJob.FailedCause == RMDiscoveryJobFailedCause.DiscoveryFailed)
            {
                _logger.Warn($"The site [{analysisJob.SiteId}] discovery job failed, skip analysis.");
                await ChangeDiscoveryFailedAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed);
                _reportManager.Increase();
                return true;
            }

            if (mainJob.Type == RMDiscoveryJobType.Append)
            {
                var existsSiteInfo = await _nodeDao.GetDiscoverySiteInfoAsync(discoveryJob.O365TenantId, analysisJob.SiteId);
                if (existsSiteInfo != null)
                {
                    _logger.Warn($"The site [{analysisJob.SiteId}] has been analyzed before.");
                    await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Finished);
                    _reportManager.Increase();
                    return true;
                }
            }

            var tenantContext = await GetTenantContextAsync(mainJob, discoveryJob.O365TenantId);
            var sourceContext = await tenantContext.GetOrCreateSourceContextAsync(
                discoveryJob.ContentSource,
                mainJob.Type,
                _inactiveRules,
                _rotRules,
                RegisterIndexAsync);

            var discoveryContext = await sourceContext.GetOrCreateDiscoveryContextAsync(
                discoveryJob,
                mainJob.Type,
                _inactiveRules,
                _rotRules);

            if (discoveryContext == null)
            {
                await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed);
                _reportManager.Increase();
                return true;
            }

            return await AnalyzeSiteAsync(mainJob, discoveryJob, analysisJob, tenantContext.FileExtensionAnalyzer, sourceContext, discoveryContext);
        }

        public async Task<bool> FinalizeCompletedDiscoveryJobsAsync(RMDiscoveryOffice365MainJob mainJob)
        {
            var completingDiscoveryJobs = await _jobDao.GetDiscoveryJobsAsync(mainJob.Id, RMDiscoveryJobStatus.Completing);
            var res = true;
            _logger.Info($"Start finalizing completing discovery jobs in main job [{mainJob.Id}], count [{completingDiscoveryJobs.Count}].");

            foreach (var completingDiscoveryJob in completingDiscoveryJobs)
            {
                if (await _jobDao.HasProcessingAnalysisJobAsync(completingDiscoveryJob.Id))
                {
                    _logger.Info($"Skip finalizing discovery job [{completingDiscoveryJob.Id}] in main job [{mainJob.Id}] because processing analysis jobs still exist.");
                    continue;
                }

                if (!_tenantContexts.TryGetValue(completingDiscoveryJob.O365TenantId, out var tenantContext) ||
                    !tenantContext.HasDiscoveryContext(completingDiscoveryJob.ContentSource, completingDiscoveryJob.Id))
                {
                    _logger.Warn($"Discovery job [{completingDiscoveryJob.Id}] in main job [{mainJob.Id}] has no runtime context while status is Completing. Falling back to persisted status finalization.");
                    res &= await FinalizeDiscoveryJobWithoutRuntimeContextAsync(mainJob, completingDiscoveryJob);
                    continue;
                }

                var sourceContext = await tenantContext.GetOrCreateSourceContextAsync(
                    completingDiscoveryJob.ContentSource,
                    mainJob.Type,
                    _inactiveRules,
                    _rotRules,
                    RegisterIndexAsync);

                _logger.Info($"Finalizing discovery job [{completingDiscoveryJob.Id}] in main job [{mainJob.Id}] with runtime context.");
                res &= await sourceContext.FinalizeDiscoveryJobAsync(completingDiscoveryJob, _jobDao);
                _logger.Info($"Finished finalizing discovery job [{completingDiscoveryJob.Id}] in main job [{mainJob.Id}].");
            }

            _logger.Info($"Finish finalizing completing discovery jobs in main job [{mainJob.Id}]. Result [{res}].");
            return res;
        }

        public async Task<bool> FinalizeMainAsync(RMDiscoveryOffice365MainJob mainJob)
        {
            var res = true;
            foreach (var tenantContext in _tenantContexts.Values)
            {
                res &= await tenantContext.FinalizeAsync(mainJob, _jobDao);
            }

            return res;
        }

        private async Task<bool> FinalizeDiscoveryJobWithoutRuntimeContextAsync(RMDiscoveryOffice365MainJob mainJob, RMDiscoveryOffice365DiscoveryJob discoveryJob)
        {
            if (mainJob.Type == RMDiscoveryJobType.Retry)
            {
                _logger.Warn($"Discovery job [{discoveryJob.Id}] in main job [{mainJob.Id}] has no runtime context during Retry finalization. Rebuilding runtime context from persisted data.");

                var tenantContext = await GetTenantContextAsync(mainJob, discoveryJob.O365TenantId);
                var sourceContext = await tenantContext.GetOrCreateSourceContextAsync(
                    discoveryJob.ContentSource,
                    mainJob.Type,
                    _inactiveRules,
                    _rotRules,
                    RegisterIndexAsync);
                if (sourceContext == null)
                {
                    _logger.Error($"Failed to rebuild source runtime context for discovery job [{discoveryJob.Id}] in retry main job [{mainJob.Id}].");
                    return false;
                }

                var discoveryContext = await sourceContext.GetOrCreateDiscoveryContextAsync(
                    discoveryJob,
                    mainJob.Type,
                    _inactiveRules,
                    _rotRules);
                if (discoveryContext == null)
                {
                    _logger.Error($"Failed to rebuild discovery runtime context for discovery job [{discoveryJob.Id}] in retry main job [{mainJob.Id}].");
                    return false;
                }

                return await sourceContext.FinalizeDiscoveryJobAsync(discoveryJob, _jobDao);
            }

            var analysisJobStatusDic = await _jobDao.GetAnalysisCompletedStatusAsync(discoveryJob.Id);
            _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Finished, out var finishedCount);
            _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Failed, out var failedCount);
            _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Timeout, out var timeoutCount);
            _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Skipped, out var skippedCount);

            discoveryJob.EndTime = DateTime.UtcNow.Ticks;
            discoveryJob.Status = RMDiscoveryJobStatus.Finished;
            if (finishedCount > 0 && discoveryJob.SiteCount - finishedCount - skippedCount > 0)
            {
                discoveryJob.Status = RMDiscoveryJobStatus.Exception;
            }
            else if (failedCount + timeoutCount > 0)
            {
                discoveryJob.Status = RMDiscoveryJobStatus.Failed;
            }

            await _jobDao.AddOrUpdateDiscoveryJobAsync(discoveryJob);
            _logger.Info($"Finalized discovery job [{discoveryJob.Id}] in main job [{mainJob.Id}] without runtime context. Finished:[{finishedCount}], Failed:[{failedCount}], Timeout:[{timeoutCount}], Skipped:[{skippedCount}], FinalStatus:[{discoveryJob.Status}].");
            return true;
        }

        public async Task SyncDatabaseAsync()
        {
            await RMDiscoveryOffice365SQLiteDBManager.SyncDatabaseToStorageAsync();
        }

        private async Task<TenantRuntimeContext> GetTenantContextAsync(RMDiscoveryOffice365MainJob mainJob, Guid tenantId)
        {
            if (_tenantContexts.TryGetValue(tenantId, out var tenantContext))
            {
                return tenantContext;
            }

            var inactiveColumns = _inactiveRules.ConvertAll(item => item.ToCustomColumn());
            await RMDiscoveryOffice365SQLiteDBManager.InitInactiveTablesAsync(tenantId, inactiveColumns);
            await RMDiscoveryOffice365SQLiteDBManager.InitRotTablesAsync(tenantId);
            _logger.Info($"Successful create o365 tenant [{tenantId}] inactive & rot table.");

            var fileExtensionAnalyzer = new RMDiscoveryOffice365FileExtensionAnalysisManager(tenantId);
            await fileExtensionAnalyzer.InitAsync();

            tenantContext = new TenantRuntimeContext(tenantId, mainJob.Type, fileExtensionAnalyzer);
            _tenantContexts[tenantId] = tenantContext;
            return tenantContext;
        }

        private async Task<bool> AnalyzeSiteAsync(
            RMDiscoveryOffice365MainJob mainJob,
            RMDiscoveryOffice365DiscoveryJob discoveryJob,
            RMDiscoveryOffice365AnalysisJob analysisJob,
            RMDiscoveryOffice365FileExtensionAnalysisManager fileExtensionAnalyzer,
            SourceRuntimeContext sourceContext,
            DiscoveryRuntimeContext discoveryContext)
        {
            try
            {
                var res = true;

                await ChangeAnalysisJobToRunningAsync(analysisJob);
                _logger.Info($"Start analysis [{analysisJob.Id}] [{analysisJob.Url}] job");

                var jobType = mainJob.Type;
                var contentSource = discoveryJob.ContentSource;
                var o365TenantId = discoveryJob.O365TenantId;
                var containerId = discoveryContext.ContainerId;
                var siteId = analysisJob.SiteId;

                var analyzedDataManager = new RMDiscoveryOffice365AnalyzedDataManager(o365TenantId, siteId);
                var (succeed, aggregateInfo) = sourceContext.AggregateTotalDataAnalyzer.Analysis(analyzedDataManager);
                if (!succeed)
                {
                    await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed);
                    _reportManager.Increase();
                    return false;
                }

                _temeleter.Increse(aggregateInfo.FileSumCount, aggregateInfo.FileTotalSize);

                var siteDataAnalyzer = new RMDiscoveryOffice365SiteDataAnalyzer(jobType, contentSource, containerId, analysisJob);
                var (initSucceed, siteInfo) = await siteDataAnalyzer.InitAsync(aggregateInfo);
                if (!initSucceed)
                {
                    await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed);
                    _reportManager.Increase();
                    return false;
                }

                var siteInactiveDataAnalyzer = new RMDiscoveryOffice365SiteInactiveDataAnalyzer(
                    jobType,
                    o365TenantId,
                    contentSource,
                    containerId,
                    siteInfo.Id,
                    siteInfo.SiteId,
                    _inactiveRules,
                    fileExtensionAnalyzer);

                var siteRotDataAnalyzer = new RMDiscoveryOffice365SiteRotDataAnalyzer(
                    jobType,
                    o365TenantId,
                    contentSource,
                    containerId,
                    siteInfo.Id,
                    siteInfo.SiteId,
                    _rotRules,
                    fileExtensionAnalyzer);

                foreach (var analyzedDataInfo in analyzedDataManager.GetAnalyzedDataInfoes())
                {
                    siteInactiveDataAnalyzer.Increse(analyzedDataInfo);
                    siteRotDataAnalyzer.Increse(analyzedDataInfo);
                }

                var (inactiveSucceed, _) = await siteInactiveDataAnalyzer.AnalysisAsync();
                res &= inactiveSucceed;

                var (rotRuleLevelSucceed, _) = await siteRotDataAnalyzer.AnalysisRuleLevelAsync();
                res &= rotRuleLevelSucceed;

                var (rotCategoryLevelSucceed, _) = await siteRotDataAnalyzer.AnalysisCategoryLevelAsync();
                res &= rotCategoryLevelSucceed;

                var (rotRootLevelSucceed, _) = await siteRotDataAnalyzer.AnalysisRootLevelAsync();
                res &= rotRootLevelSucceed;

                if (res)
                {
                    var siteAnalysisResult = new RMDiscoveryOffice365SiteAnalysisResult
                    {
                        SiteInfo = siteInfo,
                        AggregateInfo = aggregateInfo,
                        InactiveDataList = siteInactiveDataAnalyzer.GetDataList(),
                        RuleLevelRotDataList = siteRotDataAnalyzer.GetRuleLevelDataList(),
                        CategoryLevelRotDataList = siteRotDataAnalyzer.GetCategoryLevelDataList(),
                        RootLevelRotDataList = siteRotDataAnalyzer.GetRootLevelDataList(),
                    };
                    sourceContext.AggregateTotalDataAnalyzer.Increse(aggregateInfo);
                    res &= await sourceContext.PersistSiteResultAsync(discoveryContext, siteAnalysisResult);
                }

                await ChangeAnalysisJobToEndAsync(analysisJob, res ? RMDiscoveryJobStatus.Finished : RMDiscoveryJobStatus.Failed);
                _reportManager.Increase();
                _logger.Info($"End analysis [{analysisJob.Id}] [{analysisJob.Url}] job. Status: [{res}]");
                return res;
            }
            catch (Exception e)
            {
                await ChangeAnalysisJobToEndAsync(analysisJob, RMDiscoveryJobStatus.Failed);
                _reportManager.Increase();
                _logger.Error($"An error occurred while analysis [{analysisJob.Id}] [{analysisJob.Url}] job. Error: {e}");
                return false;
            }
        }

        private async Task<bool> RegisterIndexAsync(Guid o365TenantId, SourceFlag contentSource)
        {
            try
            {
                var rules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT);
                var indexModels = rules.Select(item => new IndexModel
                {
                    Name = item.ToTagColumn(),
                    Definition = JsonConvert.SerializeObject(new Dictionary<string, int>
                    {
                        { item.ToTagColumn(), 1 }
                    }),
                }).ToList();

                indexModels.Add(new IndexModel
                {
                    Name = "Compound_FileSize",
                    Definition = JsonConvert.SerializeObject(new Dictionary<string, int>
                    {
                        { "FileSize", 1 },
                        { "_id", 1 }
                    })
                });

                if (indexModels.Count > 0)
                {
                    await _ieApiClient.DatabaseManagementService.CreateIndexAsync(new IndexCreationModel
                    {
                        DataType = contentSource == SourceFlag.SharePoint ? DataType.SPDocument : DataType.SPOneDriveDocument,
                        Office365TenantId = o365TenantId.ToString(),
                        Indexes = indexModels
                    });
                }

                _logger.Info($"Successful register index for o365 tenant [{o365TenantId}] content source [{contentSource}].");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while register index for o365 tenant [{o365TenantId}] content source [{contentSource}]. Error: {e}");
                return false;
            }
        }

        private async Task ChangeAnalysisJobToEndAsync(RMDiscoveryOffice365AnalysisJob analysisJob, RMDiscoveryJobStatus jobStatus)
        {
            analysisJob.Status = jobStatus;
            analysisJob.FailedCause = jobStatus == RMDiscoveryJobStatus.Finished ? RMDiscoveryJobFailedCause.None : RMDiscoveryJobFailedCause.AnalysisFailed;
            analysisJob.EndTime = DateTime.UtcNow.Ticks;
            await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJob);
        }

        private async Task ChangeAnalysisJobToRunningAsync(RMDiscoveryOffice365AnalysisJob analysisJob)
        {
            analysisJob.Status = RMDiscoveryJobStatus.Running;
            analysisJob.StartTime = DateTime.UtcNow.Ticks;
            await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJob);
        }

        private async Task ChangeDiscoveryFailedAnalysisJobToEndAsync(RMDiscoveryOffice365AnalysisJob analysisJob, RMDiscoveryJobStatus jobStatus)
        {
            analysisJob.Status = jobStatus;
            analysisJob.EndTime = DateTime.UtcNow.Ticks;
            await _jobDao.AddOrUpdateAnalysisJobAsync(analysisJob);
        }

        /// <summary>
        /// Cleans up all data related to a deleted site from both SQL Server and SQLite databases.
        /// </summary>
        /// <param name="o365TenantId">The Office 365 tenant identifier.</param>
        /// <param name="siteId">The site identifier to clean up.</param>
        private async Task CleanupDeletedSiteDataAsync(Guid o365TenantId, Guid siteId)
        {
            try
            {
                _logger.Info($"Starting cleanup for deleted site [{siteId}] in tenant [{o365TenantId}].");

                var siteIdInt = await _nodeDao.DeleteSiteDataBySiteIdAsync(o365TenantId, siteId);
                _logger.Info($"Successfully cleaned up SQL Server data for site [{siteId}].");

                if (siteIdInt < 0)
                {
                    _logger.Warn($"Site ID [{siteId}] is invalid for SQLite cleanup. Skipping SQLite cleanup.");
                    return;
                }

                await RMDiscoveryOffice365SQLiteDBManager.DeleteSiteDataBySiteIdAsync(o365TenantId, siteIdInt);
                _logger.Info($"Successfully cleaned up SQLite data for site [{siteId}].");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while cleaning up data for deleted site [{siteId}]. Error: {e}");
            }
        }

        private sealed class TenantRuntimeContext
        {
            private readonly Guid _tenantId;
            private readonly RMDiscoveryJobType _jobType;
            private readonly RMDiscoveryOffice365FileExtensionAnalysisManager _fileExtensionAnalyzer;
            private readonly Dictionary<SourceFlag, SourceRuntimeContext> _sourceContexts = new();

            public TenantRuntimeContext(Guid tenantId, RMDiscoveryJobType jobType, RMDiscoveryOffice365FileExtensionAnalysisManager fileExtensionAnalyzer)
            {
                _tenantId = tenantId;
                _jobType = jobType;
                _fileExtensionAnalyzer = fileExtensionAnalyzer;
            }

            public RMDiscoveryOffice365FileExtensionAnalysisManager FileExtensionAnalyzer => _fileExtensionAnalyzer;

            public async Task<SourceRuntimeContext> GetOrCreateSourceContextAsync(
                SourceFlag contentSource,
                RMDiscoveryJobType mainJobType,
                List<RMDiscoveryOffice365RuleInfo> inactiveRules,
                List<RMDiscoveryOffice365RuleInfo> rotRules,
                Func<Guid, SourceFlag, Task<bool>> registerIndexAsync)
            {
                if (_sourceContexts.TryGetValue(contentSource, out var context))
                {
                    return context;
                }

                var isRegistered = await registerIndexAsync(_tenantId, contentSource);
                if (!isRegistered)
                {
                    throw new InvalidOperationException($"Failed to register index for tenant [{_tenantId}] content source [{contentSource}].");
                }

                context = new SourceRuntimeContext(_tenantId, contentSource, mainJobType, inactiveRules, rotRules);
                _sourceContexts[contentSource] = context;
                return context;
            }

            public bool HasDiscoveryContext(SourceFlag contentSource, Guid discoveryJobId)
            {
                return _sourceContexts.TryGetValue(contentSource, out var context) && context.HasDiscoveryContext(discoveryJobId);
            }

            public async Task<bool> FinalizeAsync(RMDiscoveryOffice365MainJob mainJob, IRMDiscoveryOffice365JobDao jobDao)
            {
                var res = true;
                foreach (var context in _sourceContexts.Values)
                {
                    res &= await context.FinalizeAsync(mainJob, jobDao);
                }

                return res;
            }
        }

        private sealed class SourceRuntimeContext
        {
            private readonly RALogger _logger = RALogger.GetInstance(typeof(SourceRuntimeContext));

            private readonly Guid _tenantId;
            private readonly SourceFlag _contentSource;
            private readonly RMDiscoveryJobType _jobType;
            private readonly List<RMDiscoveryOffice365RuleInfo> _inactiveRules;
            private readonly List<RMDiscoveryOffice365RuleInfo> _rotRules;
            private readonly Dictionary<Guid, DiscoveryRuntimeContext> _discoveryContexts = new();
            private readonly HashSet<Guid> _discoveryJobIds = new();

            public SourceRuntimeContext(Guid tenantId, SourceFlag contentSource, RMDiscoveryJobType jobType, List<RMDiscoveryOffice365RuleInfo> inactiveRules, List<RMDiscoveryOffice365RuleInfo> rotRules)
            {
                _tenantId = tenantId;
                _contentSource = contentSource;
                _jobType = jobType;
                _inactiveRules = inactiveRules;
                _rotRules = rotRules;
                AggregateTotalDataAnalyzer = new RMDiscoveryOffice365AggregateTotalDataAnalyzer(_tenantId, _jobType, _contentSource);
                BasicInactiveDataAnalyzer = new RMDiscoveryOffice365BasicInactiveDataAnalyzer(_jobType, _tenantId, _contentSource, _inactiveRules);
                BasicRotDataAnalyzer = new RMDiscoveryOffice365BasicRotDataAnalyzer(_jobType, _tenantId, _contentSource);
            }

            public RMDiscoveryOffice365AggregateTotalDataAnalyzer AggregateTotalDataAnalyzer { get; }

            public RMDiscoveryOffice365BasicInactiveDataAnalyzer BasicInactiveDataAnalyzer { get; }

            public RMDiscoveryOffice365BasicRotDataAnalyzer BasicRotDataAnalyzer { get; }

            public async Task<DiscoveryRuntimeContext> GetOrCreateDiscoveryContextAsync(
                RMDiscoveryOffice365DiscoveryJob discoveryJob,
                RMDiscoveryJobType mainJobType,
                List<RMDiscoveryOffice365RuleInfo> inactiveRules,
                List<RMDiscoveryOffice365RuleInfo> rotRules)
            {
                if (_discoveryContexts.TryGetValue(discoveryJob.Id, out var context))
                {
                    return context;
                }

                context = await DiscoveryRuntimeContext.CreateAsync(discoveryJob, mainJobType, inactiveRules, rotRules);
                if (context == null)
                {
                    return null;
                }

                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    AggregateTotalDataAnalyzer.Memeory();
                }

                _discoveryContexts[discoveryJob.Id] = context;
                _discoveryJobIds.Add(discoveryJob.Id);
                return context;
            }

            public bool HasDiscoveryContext(Guid discoveryJobId)
            {
                return _discoveryContexts.ContainsKey(discoveryJobId);
            }

            public async Task<bool> FinalizeDiscoveryJobAsync(RMDiscoveryOffice365DiscoveryJob discoveryJob, IRMDiscoveryOffice365JobDao jobDao)
            {
                if (!_discoveryContexts.TryGetValue(discoveryJob.Id, out var context))
                {
                    return false;
                }

                var res = true;
                if (_jobType == RMDiscoveryJobType.Retry)
                {
                    res = await context.RefreshAndSaveAsync();
                }

                if (!res)
                {
                    if (_jobType == RMDiscoveryJobType.Retry)
                    {
                        AggregateTotalDataAnalyzer.Fallback();
                    }

                    await jobDao.ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus.Failed, RMDiscoveryJobFailedCause.AnalysisFailed, discoveryJob.Id);
                }

                var analysisJobStatusDic = await jobDao.GetAnalysisCompletedStatusAsync(discoveryJob.Id);
                _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Finished, out var finishedCount);
                _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Failed, out var failedCount);
                _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Timeout, out var timeoutCount);
                _ = analysisJobStatusDic.TryGetValue(RMDiscoveryJobStatus.Skipped, out var skippedCount);
                discoveryJob.EndTime = DateTime.UtcNow.Ticks;
                discoveryJob.Status = RMDiscoveryJobStatus.Finished;
                if (finishedCount > 0 && discoveryJob.SiteCount - finishedCount - skippedCount > 0)
                {
                    discoveryJob.Status = RMDiscoveryJobStatus.Exception;
                }
                else if (failedCount + timeoutCount > 0)
                {
                    discoveryJob.Status = RMDiscoveryJobStatus.Failed;
                }

                await jobDao.AddOrUpdateDiscoveryJobAsync(discoveryJob);
                _logger.Info($"Finalized discovery job [{discoveryJob.Id}] with runtime context. Finished:[{finishedCount}], Failed:[{failedCount}], Timeout:[{timeoutCount}], Skipped:[{skippedCount}], FinalStatus:[{discoveryJob.Status}].");
                _discoveryContexts.Remove(discoveryJob.Id);
                return res;
            }

            public async Task<bool> FinalizeAsync(RMDiscoveryOffice365MainJob mainJob, IRMDiscoveryOffice365JobDao jobDao)
            {
                if (_jobType != RMDiscoveryJobType.Retry)
                {
                    return true;
                }

                var basicInactiveSaveSucceed = await BasicInactiveDataAnalyzer.RefreshAndSaveAsync();
                var basicRotSaveSucceed = await BasicRotDataAnalyzer.RefreshAndSaveAsync();
                var basicLevelStatus = basicInactiveSaveSucceed && basicRotSaveSucceed;
                if (basicLevelStatus)
                {
                    basicLevelStatus &= await AggregateTotalDataAnalyzer.RefreshAndSaveAsync();
                }

                if (!basicLevelStatus)
                {
                    var failedDiscoveryJobIds = _discoveryJobIds.ToArray();
                    if (failedDiscoveryJobIds.Any())
                    {
                        _logger.Warn($"Basic level save failed in main job [{mainJob.Id}], marking analysis jobs failed for discovery jobs [{string.Join(", ", failedDiscoveryJobIds)}].");
                        await jobDao.ChangeAnalysisJobsStatusAsync(RMDiscoveryJobStatus.Failed, RMDiscoveryJobFailedCause.AnalysisFailed, failedDiscoveryJobIds);
                    }
                }

                return basicLevelStatus;
            }

            public async Task<bool> PersistSiteResultAsync(DiscoveryRuntimeContext discoveryContext, RMDiscoveryOffice365SiteAnalysisResult siteAnalysisResult)
            {
                var siteResultPersistor = new RMDiscoveryOffice365SiteResultPersistor(
                    _tenantId,
                    _contentSource,
                    _jobType,
                    discoveryContext.ContainerId,
                    _inactiveRules,
                    AggregateTotalDataAnalyzer);
                var persisted = await siteResultPersistor.PersistAsync(siteAnalysisResult);
                if (!persisted)
                {
                    return false;
                }

                await ClearQueryCacheAsync();
                return true;
            }

            private async Task ClearQueryCacheAsync()
            {
                var cacheManager = new RMDiscoveryCacheManager(_tenantId, RMDiscoveryCacheDataSource.Office365);
                await cacheManager.ClearAsync();
            }
        }

        private sealed class DiscoveryRuntimeContext
        {
            private readonly RMDiscoveryJobType _jobType;

            private DiscoveryRuntimeContext(
                RMDiscoveryOffice365DiscoveryJob discoveryJob,
                RMDiscoveryJobType jobType,
                int containerId,
                RMDiscoveryOffice365ContainerDataAnalyzer containerDataAnalyzer,
                RMDiscoveryOffice365ContainerInactiveDataAnalyzer containerInactiveDataAnalyzer,
                RMDiscoveryOffice365ContainerRotDataAnalyzer containerRotDataAnalyzer)
            {
                DiscoveryJob = discoveryJob;
                _jobType = jobType;
                ContainerId = containerId;
                ContainerDataAnalyzer = containerDataAnalyzer;
                ContainerInactiveDataAnalyzer = containerInactiveDataAnalyzer;
                ContainerRotDataAnalyzer = containerRotDataAnalyzer;
            }

            public RMDiscoveryOffice365DiscoveryJob DiscoveryJob { get; }

            public int ContainerId { get; }

            public RMDiscoveryOffice365ContainerDataAnalyzer ContainerDataAnalyzer { get; }

            public RMDiscoveryOffice365ContainerInactiveDataAnalyzer ContainerInactiveDataAnalyzer { get; }

            public RMDiscoveryOffice365ContainerRotDataAnalyzer ContainerRotDataAnalyzer { get; }

            public static async Task<DiscoveryRuntimeContext> CreateAsync(
                RMDiscoveryOffice365DiscoveryJob discoveryJob,
                RMDiscoveryJobType mainJobType,
                List<RMDiscoveryOffice365RuleInfo> inactiveRules,
                List<RMDiscoveryOffice365RuleInfo> rotRules)
            {
                var containerDataAnalyzer = new RMDiscoveryOffice365ContainerDataAnalyzer(mainJobType, discoveryJob);
                var (initSucceed, containerInfo) = await containerDataAnalyzer.InitAsync();
                if (!initSucceed)
                {
                    return null;
                }

                if (mainJobType != RMDiscoveryJobType.Retry)
                {
                    return new DiscoveryRuntimeContext(discoveryJob, mainJobType, containerInfo.Id, null, null, null);
                }

                var containerInactiveDataAnalyzer = new RMDiscoveryOffice365ContainerInactiveDataAnalyzer(mainJobType, discoveryJob.O365TenantId, containerInfo.Id, inactiveRules);
                var containerRotDataAnalyzer = new RMDiscoveryOffice365ContainerRotDataAnalyzer(mainJobType, discoveryJob.O365TenantId, containerInfo.Id);
                return new DiscoveryRuntimeContext(discoveryJob, mainJobType, containerInfo.Id, containerDataAnalyzer, containerInactiveDataAnalyzer, containerRotDataAnalyzer);
            }

            public async Task<bool> RefreshAndSaveAsync()
            {
                if (_jobType != RMDiscoveryJobType.Retry)
                {
                    return true;
                }

                var containerInactiveSaveSucceed = await ContainerInactiveDataAnalyzer.RefreshAndSaveAsync();
                var containerRotSaveSucceed = await ContainerRotDataAnalyzer.RefreshAndSaveAsync();
                var saveSucceed = await ContainerDataAnalyzer.RefreshAndSaveAsync();
                return containerInactiveSaveSucceed && containerRotSaveSucceed && saveSucceed;
            }
        }
    }
}
