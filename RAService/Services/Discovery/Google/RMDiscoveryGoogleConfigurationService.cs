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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Google;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Converter.Discovery;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.Service.Services.Discovery.Google.Audit;
using AvePoint.RA.Service.Services.Discovery.Google.Configuration;
using AvePoint.RA.Service.Services.Discovery.Google.Configuration.Checker;
using AvePoint.RA.Service.Services.Discovery.Google.License;
using AvePoint.RA.Service.Services.Discovery.Google.Work.Preparer;
using AvePoint.RA.Service.Services.Discovery.Google.Work.Report;
using AvePoint.RA.Service.Services.Discovery.Salesforce.License;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Google
{
    [AsyncAudit]
    public class RMDiscoveryGoogleConfigurationService : IRMDiscoveryGoogleConfigurationService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryGoogleConfigurationInfo));

        private readonly IRMDiscoveryConfigurationDao _configurationDao = new RMDiscoveryConfigurationDao();

        private readonly IRMDiscoveryGoogleRuleInfoDao _ruleInfoDao = new RMDiscoveryGoogleRuleInfoDao();

        private readonly IRMDiscoveryGoogleSizeRangeDao _sizeRangeDao = new RMDiscoveryGoogleSizeRangeDao();

        private readonly IRMDiscoveryGoogleWithoutInDateDao _withoutInDateDao = new RMDiscoveryGoogleWithoutInDateDao();

        private readonly IRMDiscoveryGoogleJobDao _discoveryJobDao = new RMDiscoveryGoogleJobDao();

        private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

        private readonly IJobQueueService _jobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();

        private readonly IRMTenantDiscoveryDBInfoDao _tenantInfoDao = new RMTenantDiscoveryDBInfoDao();

        public async Task<RMDiscoveryGoogleConfigurationInfo> GetConfigurationInfoAsync()
        {
            try
            {
                if (!await _tenantInfoDao.IsInitTenantDiscoveryDBInfoAsync() || !await RMDiscoveryDBManager.CheckGoogleTablesExistsAsync())
                {
                    return new RMDiscoveryGoogleConfigurationInfo
                    {
                        ScopeInfo = RMDiscoveryGoogleDefaultConfigurationInfo.DEFAULT_SCOPE_INFO,
                        SizeRangeInfoes = RMDiscoveryGoogleDefaultConfigurationInfo.DEFAULT_SIZE_RANGE_INFOES,
                        DateRangeInfoes = RMDiscoveryGoogleDefaultConfigurationInfo.DEFAULT_DATE_RANGE_INFOES,
                        RotDefinition = RMDiscoveryGoogleDefaultConfigurationInfo.DEFAULT_ROT_DEFINITION,
                    };
                }

                var scopeInfo = (await _configurationDao.GetAsync<RMDiscoveryGoogleScopeInfo>(RMDiscoveryConfigurationType.GoogleNewlyScope));
                var rotDefinition = await _configurationDao.GetAsync<RMDiscoveryGoogleRotDefinition>(RMDiscoveryConfigurationType.GoogleROTDefinition);
                var rules = await _ruleInfoDao.GetRuleInfoesAsync(RMDiscoveryRuleDefinitionKind.ROT);

                rules.ForEach(item =>
                {
                    if (item.AnalyseMethod == RMDiscoveryRuleAnalyseMethod.Document) item.AnalyseMethod = RMDiscoveryRuleAnalyseMethod.GoogleDocument;
                });

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

                var result = RMDiscoveryGoogleConfigurationAssembler.Instance
                    .AddScopeInfo(scopeInfo)
                    .AddSizeRangeInfo(sizeRanges)
                    .AddDateRangeInfo(dateRanges)
                    .AddRotDefinition(rotDefinition)
                    .AddRules(rules)
                    .Assemble();

                return result;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get discovery google configuration info. Error: {e}");
                return new();
            }
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryConfiguration, Action = AuditAction.SaveDiscoveryConfiguration, IAsyncBeforeHandler = typeof(RMDiscoveryGoogleConfigurationBeforeAuditHandler))]
        public async Task<RAReturnMessage> AddOrUpdateNewlyConfigurationInfoAsync(RMDiscoveryGoogleConfigurationInfo configurationInfo)
        {
            try
            {
                var resultMessage = new RAReturnMessage();
                
                var hasRunningProfileJob = _jobMonitorService.GetRunningJobsCount(JobType.DiscoveryGoogleProfileJob);
                hasRunningProfileJob += _jobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.DiscoveryGoogleProfileJob);
                if (hasRunningProfileJob > 0)
                {
                    resultMessage.MessageType = RAMessageType.Failed;
                    resultMessage.ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_HasRunningProfileJob");
                    return resultMessage;
                }
                
                var checker = new RMDiscoveryGoogleConfigurationNewlyChecker(configurationInfo);
                var (isPassed, message) = await checker.CheckAsync();
                if (!isPassed)
                {
                    _logger.Warn($"Google newly security check failed.");
                    resultMessage.MessageType = RAMessageType.Failed;
                    resultMessage.ErrorMessage = I18NEntity.GetString(message);
                    return resultMessage;
                }

                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryGoogleConfiguration, TimeSpan.FromMinutes(15)))
                {
                    _logger.Info($"Start add or update discovery google configuration info.");

                    await RMDiscoveryDBManager.InitGoogleDatabaseAsync();

                    if (!await RMDiscoveryGoogleLicenseHelper.IsFromGControlWithoutDiscoveryLicenseAsync())
                    {
                        if (!await RMDiscoveryGoogleLicenseHelper.IsMeetLimitAsync())
                        {
                            resultMessage.MessageType = RAMessageType.Failed;
                            resultMessage.ErrorMessage = I18NEntity.GetString("RM_FA_License_JobLimit");
                            return resultMessage;
                        }
                    }

                    var (has, jobInfo) = await _discoveryJobDao.TryGetProcessingMainJobAsync();
                    if (has)
                    {
                        _logger.Warn($"Has processing main job [{jobInfo.Id}], prohibit add or update discovery google configuration info.");
                        resultMessage.MessageType = RAMessageType.Failed;
                        resultMessage.ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed");
                        return resultMessage;
                    }

                    using (var efContext = await RMDiscoveryDBManager.GetEFContextAsync())
                    {
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
                            _logger.Error($"An error occured while add or update discovery google configuration info to db. Error: {e}");
                            transaction.Rollback();
                            throw;
                        }
                    }

                    _logger.Info($"Finished add or update discovery google configuration info.");

                    var preparer = new RMDiscoveryGoogleJobNewlyPreparer();
                    var (success, errorMessage) = await preparer.PrepareAsync();

                    _logger.Info($"Prepare discovery google discovery job is [{success}].");

                    resultMessage.MessageType = success ? RAMessageType.Successful : RAMessageType.Failed;
                    resultMessage.ErrorMessage = errorMessage;

                    return resultMessage;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while add or update discovery google configuration info. Error: {e}");
                return new()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed"),
                };
            }
        }

        public async Task<string> DownloadDiscoveryJobReportAsync()
        {
            var (has, jobInfo) = await _discoveryJobDao.TryGetLatestMainJobAsync();
            var reportManager = new RMDiscoveryGoogleJobReportManager(jobInfo.Id);
            return await reportManager.GenerateReportAsync();
        }

        #region Private Methods

        private async Task AddOrUpdateNewlyConfigurationsAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryGoogleConfigurationInfo configurationInfo)
        {
            var willAddOrUpdateConfigurations = new Dictionary<RMDiscoveryConfigurationType, RMDiscoveryConfiguration>
            {
                {
                    RMDiscoveryConfigurationType.GoogleNewlyScope, new(){
                        ConfigurationType = RMDiscoveryConfigurationType.GoogleNewlyScope,
                        ValueJson = JsonConvert.SerializeObject(configurationInfo.ScopeInfo),
                        CreateTime = DateTime.UtcNow.Ticks,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                    }
                },
                {
                    RMDiscoveryConfigurationType.GoogleROTDefinition, new(){
                        ConfigurationType = RMDiscoveryConfigurationType.GoogleROTDefinition,
                        ValueJson = JsonConvert.SerializeObject(new RMDiscoveryOffice365RotDefinition
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

        private async Task AddOrUpdateNewlySizeRangesAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryGoogleConfigurationInfo configurationInfo)
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

            var willAddOrUpdateSizeRangeInfoes = sizeRangeInfoes.ConvertAll(item => new RMDiscoveryGoogleSizeRange
            {
                GenerateEqual = item.GenerateEqual,
                LessThan = item.LessThan,
                Order = item.Order,
                DisplayName = item.Name
            });
            await _sizeRangeDao.DeleteAllDataAsync(efContext);
            await _sizeRangeDao.AddOrUpdateAsync(efContext, willAddOrUpdateSizeRangeInfoes);
        }

        private async Task AddOrUpdateNewlyDateRangesAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryGoogleConfigurationInfo configurationInfo)
        {
            var withoutInDateInfoes = configurationInfo.DateRangeInfoes.OrderBy(item => item.Unit).ToList();
            for (int i = 0; i < withoutInDateInfoes.Count; i++)
            {
                withoutInDateInfoes[i].Order = i;
            }
            var willAddOrUpdateWithoutInDateInfoes = withoutInDateInfoes.ConvertAll(item => new RMDiscoveryGoogleWithoutInDate
            {
                Unit = item.Unit,
                UnitType = item.UnitType,
                Order = item.Order,
            });
            await _withoutInDateDao.DeleteAllInfoAsync(efContext);
            await _withoutInDateDao.AddOrUpdateAsync(efContext, willAddOrUpdateWithoutInDateInfoes);
        }

        private async Task AddOrUpdateNewlyRulesAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryGoogleConfigurationInfo configurationInfo)
        {
            var existsRules = await _ruleInfoDao.GetRuleInfoesAsync(RMDiscoveryRuleDefinitionKind.ROT);
            var rules = new List<RMDiscoveryGoogleRuleInfo>()
                .Concat(configurationInfo.RotDefinition.RedundantRules.ConvertAll(item => RMDiscoveryRuleConverter.ConvertToGoogleRuleInfo(item, RMDiscoveryRuleDefinitionKind.ROT, RMDiscoveryRuleCategory.Redundant)))
                .Concat(configurationInfo.RotDefinition.ObsoleteRules.ConvertAll(item => RMDiscoveryRuleConverter.ConvertToGoogleRuleInfo(item, RMDiscoveryRuleDefinitionKind.ROT, RMDiscoveryRuleCategory.Obsolete)))
                .Concat(configurationInfo.RotDefinition.TrivialRules.ConvertAll(item => RMDiscoveryRuleConverter.ConvertToGoogleRuleInfo(item, RMDiscoveryRuleDefinitionKind.ROT, RMDiscoveryRuleCategory.Trivial)));

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

        #endregion

        #region Job

        public void SendDiscoveryAnalysisJob(Guid mainJobId)
        {
            try
            {
                var (has, mainJob) = _discoveryJobDao.TryGetMainJobAsync(mainJobId).GetAwaiter().GetResult();
                var queueCount = _jobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.DiscoveryGoogleJobV1);
                var jobCount = _jobMonitorService.GetRunningJobsCount(JobType.DiscoveryGoogleJobV1);
                if (queueCount + jobCount > 0)
                {
                    _logger.Warn("Discovery analysis job already exists. Skipped send.");
                    return;
                }
                JobQueueDto jqDto = new()
                {
                    JobType = JobType.DiscoveryGoogleJobV1,
                    JobRunType = JobRunBy.Schedule,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = "RM_TS_RunSchedule",
                    Parameters = mainJobId.ToString()
                };
                _jobQueueService.AddToDBJobQueue(jqDto);
                _logger.Info($"Succeed send [{mainJobId}] discovery analysis [{JobType.DiscoveryGoogleJobV1}] job.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while send [{mainJobId}] discovery analysis job. Error: {e}");
            }
        }

        public string RealRunDiscoveryAnalysisJob(string parameters)
        {
            try
            {
                var (has, mainJob) = _discoveryJobDao.TryGetMainJobAsync(new Guid(parameters)).GetAwaiter().GetResult();
                var jobId = _jobMonitorService.CreateDiscoveryJobNextVersionAsync("RM_TS_RunSchedule", new Guid(parameters), JobType.DiscoveryGoogleJobV1).GetAwaiter().GetResult();
                _jobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = JobType.DiscoveryGoogleJobV1,
                    CommandLine = $"{JobType.DiscoveryGoogleJobV1} {jobId}",
                });
                return jobId;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while real run Discovery analysis job. Error: {e}");
                return string.Empty;
            }
        }

        #endregion
    }
}
