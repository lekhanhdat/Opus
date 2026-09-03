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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Lcoker;
using System;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.Service.Services.Discovery.AOSP.Configuration.Checker;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Model.Discovery;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.Service.Services.Discovery.AOSP.Work.Preparer;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.RACommonUtility.Converter.Discovery;
using AngleSharp.Common;
using AvePoint.RA.Service.Services.Discovery.AOSP.Configuration;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.GCommon.Utility;

namespace AvePoint.RA.Service.Services.Discovery.AOSP
{
    public class RMDiscoveryAOSPConfigurationService : IRMDiscoveryAOSPConfigurationService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryAOSPConfigurationService));

        private readonly IRMDiscoveryAOSPConfigurationDao _configurationDao = new RMDiscoveryAOSPConfigurationDao();

        private readonly IRMDiscoveryAOSPRuleInfoDao _ruleInfoDao = new RMDiscoveryAOSPRuleInfoDao();

        private readonly IRMDiscoveryAOSPSizeRangeDao _sizeRangeDao = new RMDiscoveryAOSPSizeRangeDao();

        private readonly IRMDiscoveryAOSPWithoutInDateDao _withoutInDateDao = new RMDiscoveryAOSPWithoutInDateDao();

        private readonly IRMDiscoveryAOSPJobDao _discoveryJobDao = new RMDiscoveryAOSPJobDao();

        private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

        private readonly IJobQueueService _jobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();

        private readonly IRMDiscoveryAOSPNodeDao _nodeDao = new RMDiscoveryAOSPNodeDao();

        private readonly IRMDiscoveryAOSPTenantDao _o365TenantDao = new RMDiscoveryAOSPTenantDao();

        private readonly IRMTenantDiscoveryDBInfoDao _tenantInfoDao = new RMTenantDiscoveryDBInfoDao();

        private readonly ILoginService _loginService = PlatformWindsorManager.GetService<ILoginService>();

        private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly ITenantService _tenantService = PlatformWindsorManager.GetService<ITenantService>();

        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public async Task<RMDiscoveryAOSPConfigurationInfo> GetConfigurationInfoAsync(string o365TenantId)
        {
            try
            {
                o365TenantId = "608ad34e-d9de-43cd-a610-49be96c2c4c1";
                _logger.Info($"Returning AOSP configuration defaults. TenantId:[{o365TenantId}], " +
                             $"IsAllowLockedSites:[false].");
                return new RMDiscoveryAOSPConfigurationInfo
                {
                    ScopeInfo = RMDiscoveryAOSPDefaultConfigurationInfo.DEFAULT_SCOPE_INFO,
                    SizeRangeInfoes = RMDiscoveryAOSPDefaultConfigurationInfo.DEFAULT_SIZE_RANGE_INFOES,
                    DateRangeInfoes = RMDiscoveryAOSPDefaultConfigurationInfo.DEFAULT_DATE_RANGE_INFOES,
                    InactiveDefinition = RMDiscoveryAOSPDefaultConfigurationInfo.DEFAULT_INACTIVE_DEFINITION,
                    RotDefinition = RMDiscoveryAOSPDefaultConfigurationInfo.DEFAULT_ROT_DEFINITION,
                };
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get AOSP configuration info. Error: {e}");
                return new();
            }
        }

        public async Task<RMDiscoveryReturnMessage> AddOrUpdateAOSPConfigurationInfoAsync(RMDiscoveryAOSPConfigurationInfo configurationInfo)
        {
            try
            {
                try
                {
                    var configurationResult = SerializerHelper.SerializeByJsonSerializer(configurationInfo);
                    _logger.Info($"AddOrUpdateAOSPConfigurationInfoAsync.RMDiscoveryAOSPConfigurationInfo:{configurationResult}.");
                    _logger.Info($"Received AOSP locked-sites configuration. " +
                                 $"IsAllowLockedSites:[{configurationInfo?.IsAllowLockedSites}].");
                }
                catch (Exception ex)
                {
                    _logger.Warn($"AddOrUpdateAOSPConfigurationInfoAsync RMDiscoveryAOSPConfigurationInfo failed.Message:{ex}.");
                }
                var isNewTenant = await _tenantService.InitAOSPTenantAsync(configurationInfo.LogonUserName);
                if (isNewTenant)
                {
                    _keyValueDao.Save(new RMKeyValue() { Key = "RunDisposalInRecords", Value = "True" });
                    await _loginService.InitSecurityProfileAsync();
                    await _generalSettingService.VerifyAndCreateDefaultSecurityProfileAsync();
                }
                var resultMessage = new RMDiscoveryReturnMessage();
                var hasRunningDSOJob = _jobMonitorService.GetRunningJobsCount(JobType.DiscoveryAOSPOptimization);
                if (hasRunningDSOJob > 0)
                {
                    resultMessage.MessageType = RAMessageType.Failed;
                    resultMessage.ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_HasRunningDSOJob");
                    return resultMessage;
                }

                var hasRunningProfileJob = _jobMonitorService.GetRunningJobsCount(JobType.DiscoveryProfileJob);
                hasRunningProfileJob += _jobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.DiscoveryProfileJob);
                if (hasRunningProfileJob > 0)
                {
                    resultMessage.MessageType = RAMessageType.Failed;
                    resultMessage.ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_HasRunningProfileJob");
                    return resultMessage;
                }

                var checker = new RMDiscoveryAOSPConfigurationChecker(configurationInfo);
                var (isPassed, message) = await checker.CheckAsync();
                if (!isPassed)
                {
                    _logger.Warn($"AOSP newly security check failed.");
                    resultMessage.MessageType = RAMessageType.Failed;
                    resultMessage.ErrorMessage = I18NEntity.GetString(message);
                    return resultMessage;
                }

                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryConfiguration, TimeSpan.FromMinutes(15)))
                {
                    _logger.Info($"Start add or update AOSP configuration info.");

                    foreach (var o365TenantId in configurationInfo.O365TenantIds)
                    {
                        await RMDiscoveryDBManager.InitAOSPDatabaseAsync(o365TenantId);
                        //if (!await RMDiscoveryOffice365LicenseHelper.IsMeetLimitAsync())
                        //{
                        //    resultMessage.MessageType = RAMessageType.Failed;
                        //    resultMessage.ErrorMessage = I18NEntity.GetString("RM_FA_License_JobLimit");
                        //    return resultMessage;
                        //}

                        var (has, jobInfo) = await _discoveryJobDao.TryGetProcessingMainJobAsync(o365TenantId);
                        if (has)
                        {
                            _logger.Warn($"Has processing main job [{jobInfo.Id}], prohibit add or update AOSP configuration info.");
                            resultMessage.MessageType = RAMessageType.Failed;
                            resultMessage.ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed");
                            return resultMessage;
                        }

                        using var efContext = await RMDiscoveryDBManager.GetEFContextAsync();
                        using var transaction = efContext.Database.BeginTransaction();
                        try
                        {
                            await AddOrUpdateNewlyConfigurationsAsync(efContext, o365TenantId, configurationInfo);
                            await AddOrUpdateNewlySizeRangesAsync(efContext, o365TenantId, configurationInfo);
                            await AddOrUpdateNewlyDateRangesAsync(efContext, o365TenantId, configurationInfo);
                            await AddOrUpdateNewlyRulesAsync(efContext, o365TenantId, configurationInfo);
                            transaction.Commit();
                        }

                        catch (Exception e)
                        {
                            _logger.Error($"An error occured while add or update AOSP configuration info to db. Error: {e}");
                            transaction.Rollback();
                            throw;
                        }
                    }
        
                    _logger.Info($"Finished add or update AOSP configuration info.");
                    resultMessage.MessageType = RAMessageType.Successful;
                    return resultMessage;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while add or update AOSP configuration info. Error: {e}");
                return new()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed"),
                };
            }
        }

        public async Task<RMDiscoveryReturnMessage> RunDiscoveryJob(RMDiscoveryAOSPJobParameter jobParamter)
        {
            try
            {
                var preparer = new RMDiscoveryAOSPJobNewlyPreparer(true, jobParamter);
                var (success, errorMessage, jobId) = await preparer.PrepareAsync();
                _logger.Info($"Prepare AOSP discovery job is [{success}].");
                return new()
                {
                    MessageType = success ? RAMessageType.Successful : RAMessageType.Failed,
                    ErrorMessage = errorMessage,
                };
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while run AOSP discovery job. Error: {e}");
                return new()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed"),
                };
            }

        }

        public async Task<RMDiscoveryReturnMessage> RunDiscoveryJob(RMDiscoveryAOSPRescanJobParameter jobParamter)
        {
            try
            {
                var preparer = new RMDiscoveryAOSPJobRescanPreparer(jobParamter);
                var (success, errorMessage, jobId) = await preparer.PrepareAsync();
                _logger.Info($"Prepare AOSP Rescan discovery job is [{success}].");
                return new()
                {
                    MessageType = success ? RAMessageType.Successful : RAMessageType.Failed,
                    ErrorMessage = errorMessage,
                    JobId = jobId.ToString(),
                };
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while run AOSP Rescan discovery job. Error: {e}");
                return new()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed"),
                };
            }

        }

        public void SendDiscoveryAnalysisJob(Guid mainJobId)
        {
            try
            {
                var (has, mainJob) = _discoveryJobDao.TryGetMainJobAsync(mainJobId).GetAwaiter().GetResult();
                var jobType = JobType.DiscoveryAOSPJob;
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
                    Parameters = mainJobId.ToString(),
                    ProductType = ProductType.AOSP,
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
                var jobType = JobType.DiscoveryAOSPJob;
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


        private async Task AddOrUpdateNewlyConfigurationsAsync(RMDiscoveryDBEFContext efContext, string o365TenantId, RMDiscoveryAOSPConfigurationInfo configurationInfo)
        {
            var willAddOrUpdateConfigurations = new Dictionary<RMDiscoveryConfigurationType, RMDiscoveryAOSPConfiguration>
            {
                {
                    RMDiscoveryConfigurationType.AOSPNewlyScope, new(){
                        ConfigurationType = RMDiscoveryConfigurationType.AOSPNewlyScope,
                        ValueJson = JsonConvert.SerializeObject(configurationInfo.ScopeInfo),
                        CreateTime = DateTime.UtcNow.Ticks,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                        O365TenantId = o365TenantId,
                    }
                },
                {
                    RMDiscoveryConfigurationType.AOSPInactiveDefinition, new(){
                        ConfigurationType = RMDiscoveryConfigurationType.AOSPInactiveDefinition,
                        ValueJson = JsonConvert.SerializeObject(new RMDiscoveryAOSPInactiveDefinition
                        {
                            Enable = configurationInfo.InactiveDefinition.Enable,
                        }),
                        CreateTime = DateTime.UtcNow.Ticks,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                        O365TenantId = o365TenantId,
                    }
                },
                {
                    RMDiscoveryConfigurationType.AOSPROTDefinition, new(){
                        ConfigurationType = RMDiscoveryConfigurationType.AOSPROTDefinition,
                        ValueJson = JsonConvert.SerializeObject(new RMDiscoveryAOSPRotDefinition
                        {
                            Enable = configurationInfo.RotDefinition.Enable,
                        }),
                        CreateTime = DateTime.UtcNow.Ticks,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                        O365TenantId = o365TenantId,
                    }
                },
                {
                    RMDiscoveryConfigurationType.AOSPAllowLockedSites, new(){
                        ConfigurationType = RMDiscoveryConfigurationType.AOSPAllowLockedSites,
                        ValueJson = JsonConvert.SerializeObject(configurationInfo.IsAllowLockedSites),
                        CreateTime = DateTime.UtcNow.Ticks,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                        O365TenantId = o365TenantId,
                    }
                },
            };
            _logger.Info($"Preparing AOSP locked-sites configuration for persistence. " +
                         $"TenantId:[{o365TenantId}], ConfigurationType:[{RMDiscoveryConfigurationType.AOSPAllowLockedSites}], " +
                         $"IsAllowLockedSites:[{configurationInfo.IsAllowLockedSites}], " +
                         $"ValueJson:[{JsonConvert.SerializeObject(configurationInfo.IsAllowLockedSites)}].");
            await _configurationDao.DeleteByO365TenantIdAsync(efContext, o365TenantId);
            await _configurationDao.AddOrUpdateAsync(efContext, [.. willAddOrUpdateConfigurations.Values]);
            _logger.Info($"Persisted AOSP locked-sites configuration. TenantId:[{o365TenantId}], " +
                         $"IsAllowLockedSites:[{configurationInfo.IsAllowLockedSites}].");
        }

        private async Task AddOrUpdateNewlySizeRangesAsync(RMDiscoveryDBEFContext efContext, string o365TenantId, RMDiscoveryAOSPConfigurationInfo configurationInfo)
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

            var willAddOrUpdateSizeRangeInfoes = sizeRangeInfoes.ConvertAll(item => new RMDiscoveryAOSPSizeRange
            {
                GenerateEqual = item.GenerateEqual,
                LessThan = item.LessThan,
                Order = item.Order,
                DisplayName = item.Name,
                O365TenantId = o365TenantId
            });
            await _sizeRangeDao.DeleteAllDataByO365TenantIdAsync(efContext, o365TenantId);
            await _sizeRangeDao.AddOrUpdateAsync(efContext, willAddOrUpdateSizeRangeInfoes);
        }

        private async Task AddOrUpdateNewlyDateRangesAsync(RMDiscoveryDBEFContext efContext, string O365TenantId, RMDiscoveryAOSPConfigurationInfo configurationInfo)
        {
            var withoutInDateInfoes = configurationInfo.DateRangeInfoes.OrderBy(item => item.Unit).ToList();
            for (int i = 0; i < withoutInDateInfoes.Count; i++)
            {
                withoutInDateInfoes[i].Order = i;
            }
            var willAddOrUpdateWithoutInDateInfoes = withoutInDateInfoes.ConvertAll(item => new RMDiscoveryAOSPWithoutInDate
            {
                Unit = item.Unit,
                UnitType = item.UnitType,
                Order = item.Order,
                O365TenantId = O365TenantId
            });
            await _withoutInDateDao.DeleteAllInfoByO365TenantIdAsync(efContext, O365TenantId);
            await _withoutInDateDao.AddOrUpdateAsync(efContext, willAddOrUpdateWithoutInDateInfoes);
        }

        private async Task AddOrUpdateNewlyRulesAsync(RMDiscoveryDBEFContext efContext, string o365TenantId, RMDiscoveryAOSPConfigurationInfo configurationInfo)
        {
            var existsRules = await _ruleInfoDao.GetRuleInfoesAsync(o365TenantId, RMDiscoveryRuleDefinitionKind.Inactive, RMDiscoveryRuleDefinitionKind.ROT);
            var rules = configurationInfo.InactiveDefinition.Rules
                .ConvertAll(item => RMDiscoveryRuleConverter.ConvertToAOSPRuleInfo(item, RMDiscoveryRuleDefinitionKind.Inactive, RMDiscoveryRuleCategory.InactiveVersion))
                .Concat(configurationInfo.RotDefinition.RedundantRules.ConvertAll(item => RMDiscoveryRuleConverter.ConvertToAOSPRuleInfo(item, RMDiscoveryRuleDefinitionKind.ROT, RMDiscoveryRuleCategory.Redundant)))
                .Concat(configurationInfo.RotDefinition.ObsoleteRules.ConvertAll(item => RMDiscoveryRuleConverter.ConvertToAOSPRuleInfo(item, RMDiscoveryRuleDefinitionKind.ROT, RMDiscoveryRuleCategory.Obsolete)))
                .Concat(configurationInfo.RotDefinition.TrivialRules.ConvertAll(item => RMDiscoveryRuleConverter.ConvertToAOSPRuleInfo(item, RMDiscoveryRuleDefinitionKind.ROT, RMDiscoveryRuleCategory.Trivial)))
                .ToList();

            var willAddRules = rules.Where(item => item.IsEnable).ConvertAll(item =>
            {
                item.Id = 0;
                item.CreateTime = DateTime.UtcNow.Ticks;
                item.ModifiedTime = DateTime.UtcNow.Ticks;
                item.UniqueId = item.UniqueId == Guid.Empty ? Guid.NewGuid() : item.UniqueId;
                item.O365TenantId = o365TenantId;
                return item;
            }).ToList();
            _logger.Info($"AddOrUpdateAOSPConfigurationInfoAsync.AddOrUpdateNewlyRulesAsync:RulesCount:{rules.Count}.AddRulesCount:{willAddRules.Count}.");
            await _ruleInfoDao.DeleteRuleInfoByO365TenantIdAsync(efContext, o365TenantId);
            await _ruleInfoDao.AddOrUpdateAsync(willAddRules, efContext);
        }

        private async Task<RAReturnMessage> AddOrUpdateCostSavingInfoAsync(string o365TenantId ,RMDiscoveryAOSPCostSavingInfo costInfo)
        {
            try
            {
                if (costInfo == null ||
                    costInfo.SPFreeStorage < 0 ||
                    costInfo.ODFreeStorage < 0 ||
                    costInfo.SPStoragePrice < 0 ||
                    costInfo.ODStoragePrice < 0 ||
                    costInfo.ArchivedDataStoragePrice < 0)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = I18NEntity.GetString("RM_Discovery_Configuration_SaveCostSavingFailed"),
                    };
                }
                var utcNow = DateTime.UtcNow.Ticks;
                var exist = await _configurationDao.ExistByO365TenantIdAsync(RMDiscoveryConfigurationType.AOSPCostSaving, o365TenantId);
                var costConfigInfo = new RMDiscoveryAOSPConfiguration
                {
                    ConfigurationType = RMDiscoveryConfigurationType.AOSPCostSaving,
                    ValueJson = JsonConvert.SerializeObject(costInfo),
                    ModifiedTime = utcNow,
                };

                if (!exist)
                {
                    costConfigInfo.CreateTime = utcNow;
                }
                else
                {
                    var beforeCostInfo = (await _configurationDao.GetByO365TenantIdAsync(o365TenantId, RMDiscoveryConfigurationType.AOSPCostSaving)).First();
                    costConfigInfo.CreateTime = beforeCostInfo.CreateTime;
                }
                costConfigInfo.O365TenantId = o365TenantId;
                var result = await _configurationDao.UpdateDiscoveryConfigurationAsync(costConfigInfo);
                if (result > 0)
                {
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Successful
                    };
                };
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_Discovery_Configuration_SaveCostSavingFailed"),
                };
            }
            catch (Exception e)
            {
                _logger.Error($"Error occured when saving cost info, error : {e}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_Discovery_Configuration_SaveCostSavingFailed"),
                };
            }
        }

        public async Task<RMDiscoveryAOSPCostSavingInfo> GetCostSavingInfoAsync(string o365TenantId)
        {
            var defaultCostInfo = new RMDiscoveryAOSPCostSavingInfo
            {
                SPFreeStorage = 0,
                SPStoragePrice = 0.20,
                ODFreeStorage = 0,
                ODStoragePrice = 0,
                ArchivedDataStoragePrice = 0.00,
            };
            try
            {
                var exist = await _configurationDao.ExistByO365TenantIdAsync(RMDiscoveryConfigurationType.AOSPCostSaving, o365TenantId);
                if (!exist)
                {
                    await AddOrUpdateCostSavingInfoAsync(o365TenantId, defaultCostInfo);
                    return defaultCostInfo;
                }

                var costInfo = await _configurationDao.GetAsync<RMDiscoveryAOSPCostSavingInfo>(RMDiscoveryConfigurationType.AOSPCostSaving);
                return costInfo;
            }
            catch (Exception e)
            {
                _logger.Error($"Error occured when get cost info, error : {e}");
                return defaultCostInfo;
            }
        }

        public async Task<string> DeleteDiscoveryDBAsync()
        {
            try
            {
                return await RMDiscoveryDBManager.DeleteAOSPDatabaseAsync();
            }
            catch(Exception e)
            {
                _logger.Error($"Error occured when DeleteDiscoveryDBAsync, error : {e}");
                return e.Message;
            }
        }
    }
}
