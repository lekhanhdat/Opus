/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AngleSharp.Common;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.FileSystem;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Converter.Discovery;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Audit;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Configuration;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Configuration.Checker;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Preparer;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Report;
using AvePoint.RA.Service.Services.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Util.MSAzure;
using SerializerHelper = AvePoint.GCommon.Utility.SerializerHelper;
using AvePoint.RA.Service.Services.Discovery.FileSystem.License;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem
{
    [AsyncAudit]
    public class RMDiscoveryFSConfigurationService : IRMDiscoveryFSConfigurationService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryFSConfigurationService));

        private readonly IRMDiscoveryConfigurationDao _configurationDao = new RMDiscoveryConfigurationDao();

        private readonly IRMDiscoveryFSRuleInfoDao _ruleInfoDao = new RMDiscoveryFSRuleInfoDao();

        private readonly IRMDiscoveryFSSizeRangeDao _sizeRangeDao = new RMDiscoveryFSSizeRangeDao();

        private readonly IRMDiscoveryFSWithoutInDateDao _withoutInDateDao = new RMDiscoveryFSWithoutInDateDao();

        private readonly IRMDiscoveryFSJobDao _discoveryJobDao = new RMDiscoveryFSJobDao();

        private readonly IRMTenantDiscoveryDBInfoDao _tenantDiscoveryDBInfoDao = new RMTenantDiscoveryDBInfoDao();

        private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

        private readonly IJobQueueService _jobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();

        private readonly IRMSubJobDao _subJobDao = new RMSubJobDao();

        private readonly IRMKeyValueDao _keyValueDao = new RMKeyValueDao();

        private readonly IHybridFileSystemWorkerService HybridFileSystemWorkerService = new HybridFileSystemWorkerService();

        private readonly IRMDiscoveryFSTagRuleInfoDao _tagRuleInfoDao = new RMDiscoveryFSTagRuleInfoDao();

        private static readonly string STORAGE_CONNECTION_STRING = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];

        private static readonly string STORAGE_CONTAINER_NAME = "fs-analyzed-file-container";

        private string TENANT_ID => TenantLocalValue.LogonGroupId.ToLower();

        public async Task<RMDiscoveryFSConfigurationInfo> GetConfigurationInfoAsync()
        {
            try
            {
                if (!await _tenantDiscoveryDBInfoDao.IsInitTenantDiscoveryDBInfoAsync() || !await RMDiscoveryDBManager.CheckFileSystemTablesExistsAsync())
                {
                    return new RMDiscoveryFSConfigurationInfo
                    {
                        ScopeInfo = RMDiscoveryFSDefaultConfigurationInfo.DEFAULT_SCOPE_INFO,
                        SizeRangeInfoes = RMDiscoveryFSDefaultConfigurationInfo.DEFAULT_SIZE_RANGE_INFOES,
                        DateRangeInfoes = RMDiscoveryFSDefaultConfigurationInfo.DEFAULT_DATE_RANGE_INFOES,
                        InactiveDefinition = RMDiscoveryFSDefaultConfigurationInfo.DEFAULT_INACTIVE_DEFINITION,
                        RotDefinition = RMDiscoveryFSDefaultConfigurationInfo.DEFAULT_ROT_DEFINITION,
                    };
                }

                var scopeInfo = (await _configurationDao.GetAsync<RMDiscoveryFSScopeInfo>(RMDiscoveryConfigurationType.FileSystemNewlyScope)).CompatibleConvert();
                var inactiveDefinition = await _configurationDao.GetAsync<RMDiscoveryFSInactiveDefinition>(RMDiscoveryConfigurationType.FileSystemInactiveDefinition);
                var rotDefinition = await _configurationDao.GetAsync<RMDiscoveryFSRotDefinition>(RMDiscoveryConfigurationType.FileSystemROTDefinition);
                var rules = await _ruleInfoDao.GetRuleInfoesAsync(RMDiscoveryRuleDefinitionKind.Inactive, RMDiscoveryRuleDefinitionKind.ROT);
                var sizeRanges = (await _sizeRangeDao.GetAllAsync()).ConvertAll(item => new RMDiscoverySizeRangeDataInfo()
                {
                    Id = item.Id,
                    Name = item.DisplayName,
                    GenerateEqual = item.GenerateEqual,
                    LessThan = item.LessThan,
                    Order = item.Order
                });
                sizeRanges.RemoveAt(sizeRanges.Count - 1);
                var dateRanges = (await _withoutInDateDao.GetAllAsync()).ConvertAll(item => new RMDiscoveryWithoutInDateDataInfo()
                {
                    Id = item.Id,
                    Unit = item.UnitType == RMDiscoveryWithoutInUnitType.Month ? item.Unit : item.Unit * 12,
                    UnitType = RMDiscoveryWithoutInUnitType.Month,
                    Order = item.Order
                });

                var result = RMDiscoveryFSConfigurationAssembler.Instance
                    .AddScopeInfo(scopeInfo)
                    .AddSizeRangeInfo(sizeRanges)
                    .AddDateRangeInfo(dateRanges)
                    .AddInactiveDefinition(inactiveDefinition)
                    .AddRotDefinition(rotDefinition)
                    .AddRules(rules)
                    .Assemble();

                return result;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get file system discovery configuration info. Error: {e}");
                return new();
            }
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryConfiguration, Action = AuditAction.SaveDiscoveryConfiguration, IAsyncBeforeHandler = typeof(RMDiscoveryFSConfigurationBeforeAuditHandler))]
        public async Task<RAReturnMessage> AddOrUpdateNewlyConfigurationInfoAsync(RMDiscoveryFSConfigurationInfo configurationInfo)
        {
            try
            {
                var resultMessage = new RAReturnMessage();
                var checker = new RMDiscoveryFSConfigurationNewlyChecker(configurationInfo);
                var (isPassed, message) = await checker.CheckAsync();
                if (!isPassed)
                {
                    _logger.Warn($"Discovery file system newly security check failed.");
                    resultMessage.MessageType = RAMessageType.Failed;
                    resultMessage.ErrorMessage = I18NEntity.GetString(message);
                    return resultMessage;
                }

                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryConfiguration, TimeSpan.FromMinutes(15)))
                {
                    _logger.Info($"Start add or update file system discovery configuration info.");

                    await RMDiscoveryDBManager.InitFileSytemDatabaseAsync();
                    // Reset counters once when COP license is first activated
                    // to ensure preview usage is not carried over.
                    await RMDiscoveryFSLicenseHelper.EnsureCopLicenseCounterResetAsync();

                    // Enforce scan frequency only for COP-licensed customers.
                    // Legacy preview-only customers have no frequency limit.
                    if (!RMDiscoveryFSLicenseHelper.IsLegacyPreviewOnlyCustomer()
                        && !await RMDiscoveryFSLicenseHelper.IsMeetLimitAsync())
                    {
                        resultMessage.MessageType = RAMessageType.Failed;
                        resultMessage.ErrorMessage = I18NEntity.GetString("RM_FA_License_JobLimit");
                        return resultMessage;
                    }

                    var (has, jobInfo) = await _discoveryJobDao.TryGetProcessingMainJobAsync();
                    if (has)
                    {
                        _logger.Warn($"Has processing main job [{jobInfo.Id}], prohibit add or update file system discovery configuration info.");
                        resultMessage.MessageType = RAMessageType.Failed;
                        resultMessage.ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed");
                        return resultMessage;
                    }

                    using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
                    using var transaction = efContext.Database.BeginTransaction();
                    try
                    {
                        await AddOrUpdateNewlyConfigurationsAsync(efContext, configurationInfo);
                        await AddOrUpdateNewlySizeRangesAsync(efContext, configurationInfo);
                        await AddOrUpdateNewlyDateRangesAsync(efContext, configurationInfo);
                        await AddOrUpdateNewlyRulesAsync(efContext, configurationInfo);
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        _logger.Error($"An error occured while add or update File System discovery configuration info to db. Error: {e}");
                        transaction.Rollback();
                        throw;
                    }

                    _logger.Info($"Finished add or update File System discovery configuration info.");

                    var preparer = new RMDiscoveryFSJobNewlyPreparer(true);
                    var (success, errorMessage) = await preparer.PrepareAsync();

                    if(success)
                    {
                        AzureUtil.DeleteBlobs(STORAGE_CONNECTION_STRING, STORAGE_CONTAINER_NAME, TENANT_ID, true);
                    }

                    _logger.Info($"Prepare File System discovery job is [{success}].");

                    resultMessage.MessageType = success ? RAMessageType.Successful : RAMessageType.Failed;
                    resultMessage.ErrorMessage = errorMessage;
                    return resultMessage;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while add or update File System configuration info. Error: {e}");
                return new()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed"),
                };
            }
        }

        private async Task AddOrUpdateNewlyConfigurationsAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryFSConfigurationInfo configurationInfo)
        {
            var willAddOrUpdateConfigurations = new Dictionary<RMDiscoveryConfigurationType, RMDiscoveryConfiguration>
            {
                {
                    RMDiscoveryConfigurationType.FileSystemNewlyScope, new(){
                        ConfigurationType = RMDiscoveryConfigurationType.FileSystemNewlyScope,
                        ValueJson = JsonConvert.SerializeObject(configurationInfo.ScopeInfo),
                        CreateTime = DateTime.UtcNow.Ticks,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                    }
                },
                {
                    RMDiscoveryConfigurationType.FileSystemInactiveDefinition, new(){
                        ConfigurationType = RMDiscoveryConfigurationType.FileSystemInactiveDefinition,
                        ValueJson = JsonConvert.SerializeObject(new RMDiscoveryFSInactiveDefinition
                        {
                            Enable = configurationInfo.InactiveDefinition.Enable,
                        }),
                        CreateTime = DateTime.UtcNow.Ticks,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                    }
                },
                {
                    RMDiscoveryConfigurationType.FileSystemROTDefinition, new(){
                        ConfigurationType = RMDiscoveryConfigurationType.FileSystemROTDefinition,
                        ValueJson = JsonConvert.SerializeObject(new RMDiscoveryFSRotDefinition
                        {
                            Enable = configurationInfo.RotDefinition.Enable,
                        }),
                        CreateTime = DateTime.UtcNow.Ticks,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                    }
                }
            };
            await _configurationDao.AddOrUpdateAsync(efContext, willAddOrUpdateConfigurations.Values.ToArray());
        }

        private async Task AddOrUpdateNewlySizeRangesAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryFSConfigurationInfo configurationInfo)
        {
            var sizeRangeInfoes = configurationInfo.SizeRangeInfoes;
            sizeRangeInfoes.Add(new RMDiscoverySizeRangeDataInfo()
            {
                GenerateEqual = sizeRangeInfoes.Count <= 0 ? 0 : sizeRangeInfoes[sizeRangeInfoes.Count - 1].LessThan,
                LessThan = int.MaxValue,
                Order = sizeRangeInfoes.Count + 1
            });
            sizeRangeInfoes = sizeRangeInfoes.OrderBy(item => item.GenerateEqual).ToList();
            for (int i = 0; i < sizeRangeInfoes.Count; i++)
            {
                var sizeRangeInfo = sizeRangeInfoes[i];
                sizeRangeInfo.Order = i;
                if (sizeRangeInfo.Order == 0)
                {
                    sizeRangeInfo.Name = "<" + sizeRangeInfo.LessThan.ToString() + " MB";
                    continue;
                }
                sizeRangeInfo.Name = ">=" + sizeRangeInfo.GenerateEqual.ToString() + " MB";
            }

            var willAddOrUpdateSizeRangeInfoes = sizeRangeInfoes.ConvertAll(item => new RMDiscoveryFSSizeRange
            {
                GenerateEqual = item.GenerateEqual,
                LessThan = item.LessThan,
                Order = item.Order,
                DisplayName = item.Name
            });
            await _sizeRangeDao.DeleteAllDataAsync(efContext);
            await _sizeRangeDao.AddOrUpdateAsync(efContext, willAddOrUpdateSizeRangeInfoes);
        }

        private async Task AddOrUpdateNewlyDateRangesAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryFSConfigurationInfo configurationInfo)
        {
            var withoutInDateInfoes = configurationInfo.DateRangeInfoes.OrderBy(item => item.Unit).ToList();
            for (int i = 0; i < withoutInDateInfoes.Count; i++)
            {
                withoutInDateInfoes[i].Order = i;
            }
            var willAddOrUpdateWithoutInDateInfoes = withoutInDateInfoes.ConvertAll(item => new RMDiscoveryFSWithoutInDate
            {
                Unit = item.Unit,
                UnitType = item.UnitType,
                Order = item.Order,
            });
            await _withoutInDateDao.DeleteAllInfoAsync(efContext);
            await _withoutInDateDao.AddOrUpdateAsync(efContext, willAddOrUpdateWithoutInDateInfoes);
        }

        private async Task AddOrUpdateNewlyRulesAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryFSConfigurationInfo configurationInfo)
        {
            var existsRules = await _ruleInfoDao.GetRuleInfoesAsync(RMDiscoveryRuleDefinitionKind.Inactive, RMDiscoveryRuleDefinitionKind.ROT);
            var rules = configurationInfo.InactiveDefinition.Rules
                .ConvertAll(item => RMDiscoveryRuleConverter.ConvertToFileSystemRuleInfo(item, RMDiscoveryRuleDefinitionKind.Inactive, RMDiscoveryRuleCategory.InactiveVersion))
                .Concat(configurationInfo.RotDefinition.RedundantRules.ConvertAll(item => RMDiscoveryRuleConverter.ConvertToFileSystemRuleInfo(item, RMDiscoveryRuleDefinitionKind.ROT, RMDiscoveryRuleCategory.Redundant)))
                .Concat(configurationInfo.RotDefinition.ObsoleteRules.ConvertAll(item => RMDiscoveryRuleConverter.ConvertToFileSystemRuleInfo(item, RMDiscoveryRuleDefinitionKind.ROT, RMDiscoveryRuleCategory.Obsolete)))
                .Concat(configurationInfo.RotDefinition.TrivialRules.ConvertAll(item => RMDiscoveryRuleConverter.ConvertToFileSystemRuleInfo(item, RMDiscoveryRuleDefinitionKind.ROT, RMDiscoveryRuleCategory.Trivial)))
                .ToList();

            var willAddRules = rules.Where(item => item.Id == 0).ConvertAll(item =>
            {
                item.CreateTime = DateTime.UtcNow.Ticks;
                item.ModifiedTime = DateTime.UtcNow.Ticks;
                item.UniqueId = Guid.NewGuid();
                return item;
            }).ToList();
            var willUpdateRules = rules.Where(item => item.Id > 0)
                .Select(item => item.Id).Intersect(existsRules.Select(item => item.Id)).ConvertAll(id =>
                {
                    var existsRule = existsRules.First(item => item.Id == id);
                    var rule = rules.First(item => item.Id == id);
                    existsRule.ModifiedTime = DateTime.UtcNow.Ticks;
                    existsRule.Name = rule.Name;
                    existsRule.Description = rule.Description;
                    existsRule.Order = rule.Order;
                    existsRule.IsEnable = rule.IsEnable;
                    existsRule.DefinitionKind = rule.DefinitionKind;
                    existsRule.AnalyseMethod = rule.AnalyseMethod;
                    existsRule.Category = rule.Category;
                    existsRule.CriteriaInfoesJson = rule.CriteriaInfoesJson;
                    return existsRule;
                });
            var willDeleteRules = existsRules.Select(item => item.Id)
                .Except(rules.Where(item => item.Id > 0).Select(item => item.Id))
                .ConvertAll(id =>
                {
                    var rule = existsRules.First(item => item.Id == id);
                    rule.IsRemoved = true;
                    return rule;
                });
            var willOperationRules = willAddRules.Concat(willUpdateRules).Concat(willDeleteRules).ToList();
            await _ruleInfoDao.AddOrUpdateAsync(willOperationRules, efContext);
        }

        public async Task<string> DownloadDiscoveryJobReportAsync()
        {
            var (has, jobInfo) = await _discoveryJobDao.TryGetLatestMainJobAsync();
            var reportManager = new RMDiscoveryFSJobReportManager(jobInfo.Id);
            return await reportManager.GenerateReportAsync();
        }

        public void SendDiscoveryAnalysisJob(Guid mainJobId)
        {
            try
            {
                var (has, mainJob) = _discoveryJobDao.TryGetMainJobAsync(mainJobId).GetAwaiter().GetResult();
                var jobType = JobType.DiscoveryAnalysisFileSystemV1;
                var queueCount = _jobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, jobType);
                var jobCount = _jobMonitorService.GetRunningJobsCount(jobType);
                if (queueCount + jobCount > 0)
                {
                    _logger.Warn("Discovery analysis next version job already exists. Skipped send.");
                    return;
                }
                JobQueueDto jqDto = new()
                {
                    JobType = jobType,
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = "RM_TS_RunSchedule",
                    Parameters = mainJobId.ToString()
                };
                _jobQueueService.AddToDBJobQueue(jqDto);
                _logger.Info($"Succeed send [{mainJobId}] discovery analysis [{jobType}] job.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while send [{mainJobId}] discovery analysis next version job. Error: {e}");
            }
        }

        public string RealRunDiscoveryAnalysisJob(string parameters)
        {
            try
            {
                var (has, mainJob) = _discoveryJobDao.TryGetMainJobAsync(new Guid(parameters)).GetAwaiter().GetResult();
                var jobType = JobType.DiscoveryAnalysisFileSystemV1;
                var jobId = _jobMonitorService.CreateDiscoveryJobNextVersionAsync("RM_TS_RunSchedule", new Guid(parameters), jobType).GetAwaiter().GetResult();
                _jobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = jobType,
                    CommandLine = $"{jobType} {jobId}",
                });
                return jobId;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while real run Discovery analysis next version job. Error: {e}");
                return string.Empty;
            }
        }


        public void UploadAnalyzedFileToStorage(DiscoveryAnalyzedDataInfo dataInfo)
        {
            try
            {
                var fileName = string.Format("{0}_data.txt", dataInfo.ConnectionId);
                _logger.Info($"Begin upload analyzed file to storage. Path [{fileName}]");
                var blobPath = (SecurityUtils.SafeCombinePath(TENANT_ID, fileName));
                AzureUtil.AppendBlob(STORAGE_CONNECTION_STRING, STORAGE_CONTAINER_NAME, blobPath, dataInfo.File, true);
                _logger.Info("Upload analyzed file to storage success.");
            }
            catch (Exception ex)
            {
                _logger.Error($"An error occurred while upload analyzed file to storage.");
                throw;
            }
        }

        public async Task<string> LoadAllTagRuleInfos()
        {
            var tagRuleInfos = await _tagRuleInfoDao.GetAllAsync();
            return JsonConvert.SerializeObject(tagRuleInfos.ConvertAll(t => t.ConvertToDto()));
        }

        public async Task<string> RealRunScanFSDiscoveryJob(string jobRunByUser, Guid mainJobId, List<RMFSDiscoveryJobSettingDto> connections)
        {
            try
            {
                int subJobCountInConfigFile = _keyValueDao.GetSubJobCountFromDB((int)JobType.DiscoveryFileSystemV1);
                string jobId = string.Empty;
                var loginName = TenantLocalValue.LogonUserEmail;
                var jobType = JobType.DiscoveryFileSystemV1;
                jobId = await _jobMonitorService.CreateDiscoveryJobNextVersionAsync(jobRunByUser, mainJobId, jobType);
                var parallelSubJobCount = subJobCountInConfigFile * await HybridFileSystemWorkerService.GetAgentCountByGroupsAsync(new List<Guid> { connections.First().ConnectionGroupId });
                if (parallelSubJobCount == 0)
                {
                    _logger.Error("No available agent server. Set main job failed.");
                    _jobMonitorService.UpdateJobStatus(jobId, Contract.RMWeb.JobMonitor.JobStatus.Failed, string.Empty);
                    return jobId;
                }
                int subJobCount = connections.Count;
                int currentSubjobIndex = 0;
                foreach (var connection in connections)
                {
                    string subJobId = CreateSubJobForScanFSDiscoverJob(jobId, currentSubjobIndex, jobType, subJobCount, connection, currentSubjobIndex < parallelSubJobCount);
                    if (currentSubjobIndex < parallelSubJobCount)
                    {
                        HybridFileSystemWorkerService.StartJobWithConnectionGroupId(new Hybrid.Contract.RecordsJobArgs()
                        {
                            JobId = subJobId,
                            JobType = Hybrid.Contract.JobType.FSDiscovery,
                            TenantId = TenantLocalValue.LogonGroupId
                        }, connection.ConnectionGroupId);
                    }
                    currentSubjobIndex++;
                }
                return jobId;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while real run Discovery analysis next version job. Error: {e}");
                return string.Empty;
            }
        }

        private string CreateSubJobForScanFSDiscoverJob(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, RMFSDiscoveryJobSettingDto jobInfo, bool sendNow)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait , Weight = 100d / subJobCount, String1 = jobInfo.ConnectionId.ToString() };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext()
            {
                JobId = subJobId,
                Settings = SerializerHelper.SerializeByDataContractSerializer(jobInfo)
            };
            _subJobDao.CreateJob(subJob);
            _logger.Info("Create sub job {0} sucessfull, type {1}, weight {2}", subJob.Id, subJob.JobType, subJob.Weight);
            return subJobId;
        }

        public string GetFSDiscoveryJobMessageAsync(string subJobId)
        {
            try
            {
                _logger.Debug("Start to get FS discovery job message. Job Id:" + subJobId);
                var subJob = _subJobDao.GetSubJob(subJobId, true);
                BaseJobDto jobDto = new BaseJobDto()
                {
                    Id = subJob.Id,
                    JobType = subJob.JobType
                };
                RMFSDiscoveryJobSettingDto jobSettingDto = SerializerHelper.DeserializeByDataContractSerializer<RMFSDiscoveryJobSettingDto>(subJob.JobContext.Settings);
                FSJobMessage jobMsg = new FSJobMessage();
                jobMsg.Job = jobDto;
                jobMsg.JobId = subJobId;
                jobMsg.ConnectionCache = new Dictionary<string, Guid>
                {
                    { jobSettingDto.UNCPath, jobSettingDto.ConnectionId }
                };
                return SerializerHelper.SerializeByDataContractSerializer(jobMsg);
            }
            catch (Exception e) 
            {
                _logger.Error("An error occurred while getting job message. JobId:{0} Error:{1}", subJobId, e.ToString());
                return string.Empty;
            }
            finally
            {
                _logger.Debug("Get job message finished. Job Id: " + subJobId);
            }
        }
    }
}
