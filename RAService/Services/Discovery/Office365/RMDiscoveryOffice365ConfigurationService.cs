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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Service.Services.Discovery.Office365.Configuration;
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Service.Services.Discovery.Office365.Configuration.Checker;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Preparer;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Model.Discovery;
using Newtonsoft.Json;
using System.Management.Automation;
using AvePoint.RA.RACommonUtility.Converter.Discovery;
using AvePoint.RA.Contract.Discovery;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Report;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Service.Services.Discovery.Office365.Audit;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;

namespace AvePoint.RA.Service.Services.Discovery.Office365
{
    [AsyncAudit]
    public class RMDiscoveryOffice365ConfigurationService : IRMDiscoveryOffice365ConfigurationService
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365ConfigurationService));

        private readonly IRMDiscoveryConfigurationDao _configurationDao = new RMDiscoveryConfigurationDao();

        private readonly IRMDiscoveryOffice365RuleInfoDao _ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();

        private readonly IRMDiscoveryOffice365SizeRangeDao _sizeRangeDao = new RMDiscoveryOffice365SizeRangeDao();

        private readonly IRMDiscoveryOffice365WithoutInDateDao _withoutInDateDao = new RMDiscoveryOffice365WithoutInDateDao();

        private readonly IRMDiscoveryOffice365JobDao _discoveryJobDao = new RMDiscoveryOffice365JobDao();

        private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

        private readonly IJobQueueService _jobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();

        private readonly IRMDiscoveryOffice365NodeDao _nodeDao = new RMDiscoveryOffice365NodeDao();

        private readonly IRMDiscoveryOffice365TenantDao _o365TenantDao = new RMDiscoveryOffice365TenantDao();

        private readonly IRMTenantDiscoveryDBInfoDao _tenantInfoDao = new RMTenantDiscoveryDBInfoDao();

        #region Newly

        public async Task<RMDiscoveryOffice365ConfigurationInfo> GetConfigurationInfoAsync()
        {
            try
            {
                if (!await _tenantInfoDao.IsInitTenantDiscoveryDBInfoAsync() || !await RMDiscoveryDBManager.CheckOffice365TablesExistsAsync())
                {
                    return new RMDiscoveryOffice365ConfigurationInfo
                    {
                        ScopeInfo = RMDiscoveryOffice365DefaultConfigurationInfo.DEFAULT_SCOPE_INFO,
                        SizeRangeInfoes = RMDiscoveryOffice365DefaultConfigurationInfo.DEFAULT_SIZE_RANGE_INFOES,
                        DateRangeInfoes = RMDiscoveryOffice365DefaultConfigurationInfo.DEFAULT_DATE_RANGE_INFOES,
                        InactiveDefinition = RMDiscoveryOffice365DefaultConfigurationInfo.DEFAULT_INACTIVE_DEFINITION,
                        RotDefinition = RMDiscoveryOffice365DefaultConfigurationInfo.DEFAULT_ROT_DEFINITION,
                    };
                }

                var scopeInfo = (await _configurationDao.GetAsync<RMDiscoveryOffice365ScopeInfo>(RMDiscoveryConfigurationType.Office365NewlyScope)).CompatibleConvert();
                var exclusionInfo = await _configurationDao.GetAsync(RMDiscoveryConfigurationType.Office365Exclusion, new RMDiscoveryExclusionInfo());
                var inactiveDefinition = await _configurationDao.GetAsync<RMDiscoveryOffice365InactiveDefinition>(RMDiscoveryConfigurationType.Office365InactiveDefinition);
                var rotDefinition = await _configurationDao.GetAsync<RMDiscoveryOffice365RotDefinition>(RMDiscoveryConfigurationType.Office365ROTDefinition);
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

                var result = RMDiscoveryOffice365ConfigurationAssembler.Instance
                    .AddScopeInfo(scopeInfo)
                    .AddExclusionInfo(exclusionInfo)
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
                _logger.Error($"An error occurred while get office 365 configuration info. Error: {e}");
                return new();
            }
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryConfiguration, Action = AuditAction.SaveDiscoveryConfiguration, IAsyncBeforeHandler = typeof(RMDiscoveryOffice365ConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryOffice365ConfigurationAfterAuditHandler))]
        public async Task<RAReturnMessage> AddOrUpdateNewlyConfigurationInfoAsync(RMDiscoveryOffice365ConfigurationInfo configurationInfo)
        {
            try
            {
                string configurationInfoJson;
                try
                {
                    configurationInfoJson = JsonConvert.SerializeObject(configurationInfo, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    });
                }
                catch (Exception serializeException)
                {
                    configurationInfoJson = $"<serialize failed: {serializeException.Message}>";
                }

                _logger.Info($"Start add or update office 365 configuration info request. Payload: {configurationInfoJson}");

                var resultMessage = new RAReturnMessage();
                var hasRunningDSOJob = _jobMonitorService.GetRunningJobsCount(JobType.DiscoverOptimization);
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

                var checker = new RMDiscoveryOffice365ConfigurationNewlyChecker(configurationInfo);
                var (isPassed, message) = await checker.CheckAsync();
                if(!isPassed)
                {
                    _logger.Warn($"Office 365 newly security check failed.");
                    resultMessage.MessageType = RAMessageType.Failed;
                    resultMessage.ErrorMessage = I18NEntity.GetString(message);
                    return resultMessage;
                }

                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryConfiguration, TimeSpan.FromMinutes(15)))
                {
                    _logger.Info($"Start add or update office 365 configuration info.");

                    await RMDiscoveryDBManager.InitOffice365DatabaseAsync();
                    if (!await RMDiscoveryOffice365LicenseHelper.IsMeetLimitAsync())
                    {
                        resultMessage.MessageType = RAMessageType.Failed;
                        resultMessage.ErrorMessage = I18NEntity.GetString("RM_FA_License_JobLimit");
                        return resultMessage;
                    }

                    var (has, jobInfo) = await _discoveryJobDao.TryGetProcessingMainJobAsync();
                    if (has)
                    {
                        _logger.Warn($"Has processing main job [{jobInfo.Id}], prohibit add or update office 365 configuration info.");
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
                    catch(Exception e)
                    {
                        _logger.Error($"An error occured while add or update office 365 configuration info to db. Error: {e}");
                        transaction.Rollback();
                        throw;
                    }

                    _logger.Info($"Finished add or update office 365 configuration info.");

                    var preparer = new RMDiscoveryOffice365JobNewlyPreparer(true);
                    var (success, errorMessage) = await preparer.PrepareAsync();

                    _logger.Info($"Prepare office 365 discovery job is [{success}].");

                    resultMessage.MessageType = success ? RAMessageType.Successful : RAMessageType.Failed;
                    resultMessage.ErrorMessage = errorMessage;
                    return resultMessage;
                }
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while add or update office 365 configuration info. Error: {e}");
                return new()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed"),
                };
            }
        }

        private async Task AddOrUpdateNewlyConfigurationsAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryOffice365ConfigurationInfo configurationInfo)
        {
            var willAddOrUpdateConfigurations = new Dictionary<RMDiscoveryConfigurationType, RMDiscoveryConfiguration>
            {
                {
                    RMDiscoveryConfigurationType.Office365NewlyScope, new(){
                        ConfigurationType = RMDiscoveryConfigurationType.Office365NewlyScope,
                        ValueJson = JsonConvert.SerializeObject(configurationInfo.ScopeInfo),
                        CreateTime = DateTime.UtcNow.Ticks,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                    }
                },
                {
                    RMDiscoveryConfigurationType.Office365InactiveDefinition, new(){
                        ConfigurationType = RMDiscoveryConfigurationType.Office365InactiveDefinition,
                        ValueJson = JsonConvert.SerializeObject(new RMDiscoveryOffice365InactiveDefinition
                        {
                            Enable = configurationInfo.InactiveDefinition.Enable,
                        }),
                        CreateTime = DateTime.UtcNow.Ticks,
                        ModifiedTime = DateTime.UtcNow.Ticks,
                    }
                },
                {
                    RMDiscoveryConfigurationType.Office365ROTDefinition, new(){
                        ConfigurationType = RMDiscoveryConfigurationType.Office365ROTDefinition,
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

        private async Task AddOrUpdateNewlySizeRangesAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryOffice365ConfigurationInfo configurationInfo)
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

            var willAddOrUpdateSizeRangeInfoes = sizeRangeInfoes.ConvertAll(item => new RMDiscoveryOffice365SizeRange
            {
                GenerateEqual = item.GenerateEqual,
                LessThan = item.LessThan,
                Order = item.Order,
                DisplayName = item.Name
            });
            await _sizeRangeDao.DeleteAllDataAsync(efContext);
            await _sizeRangeDao.AddOrUpdateAsync(efContext, willAddOrUpdateSizeRangeInfoes);
        }

        private async Task AddOrUpdateNewlyDateRangesAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryOffice365ConfigurationInfo configurationInfo)
        {
            var withoutInDateInfoes = configurationInfo.DateRangeInfoes.OrderBy(item => item.Unit).ToList();
            for (int i = 0; i < withoutInDateInfoes.Count; i++)
            {
                withoutInDateInfoes[i].Order = i;
            }
            var willAddOrUpdateWithoutInDateInfoes = withoutInDateInfoes.ConvertAll(item => new RMDiscoveryOffice365WithoutInDate
            {
                Unit = item.Unit,
                UnitType = item.UnitType,
                Order = item.Order,
            });
            await _withoutInDateDao.DeleteAllInfoAsync(efContext);
            await _withoutInDateDao.AddOrUpdateAsync(efContext, willAddOrUpdateWithoutInDateInfoes);
        }

        private async Task AddOrUpdateNewlyRulesAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryOffice365ConfigurationInfo configurationInfo)
        {
            var existsRules = await _ruleInfoDao.GetRuleInfoesAsync(RMDiscoveryRuleDefinitionKind.Inactive, RMDiscoveryRuleDefinitionKind.ROT);
            var rules = configurationInfo.InactiveDefinition.Rules
                .ConvertAll(item => RMDiscoveryRuleConverter.ConvertToOffice365RuleInfo(item, RMDiscoveryRuleDefinitionKind.Inactive, RMDiscoveryRuleCategory.InactiveVersion))
                .Concat(configurationInfo.RotDefinition.RedundantRules.ConvertAll(item => RMDiscoveryRuleConverter.ConvertToOffice365RuleInfo(item, RMDiscoveryRuleDefinitionKind.ROT, RMDiscoveryRuleCategory.Redundant)))
                .Concat(configurationInfo.RotDefinition.ObsoleteRules.ConvertAll(item => RMDiscoveryRuleConverter.ConvertToOffice365RuleInfo(item, RMDiscoveryRuleDefinitionKind.ROT, RMDiscoveryRuleCategory.Obsolete)))
                .Concat(configurationInfo.RotDefinition.TrivialRules.ConvertAll(item => RMDiscoveryRuleConverter.ConvertToOffice365RuleInfo(item, RMDiscoveryRuleDefinitionKind.ROT, RMDiscoveryRuleCategory.Trivial)))
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
                .ConvertAll(id => {
                    var rule = existsRules.First(item => item.Id == id);
                    rule.IsRemoved = true;
                    return rule;
                });
            var willOperationRules = willAddRules.Concat(willUpdateRules).Concat(willDeleteRules).ToList();
            await _ruleInfoDao.AddOrUpdateAsync(willOperationRules, efContext);
        }

        #endregion

        #region Append

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryConfiguration, Action = AuditAction.DiscoveryAppend, IAsyncBeforeHandler = typeof(RMDiscoveryOffice365ConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryOffice365ConfigurationAfterAuditHandler))]
        public async Task<RAReturnMessage> AddOrUpdateAppendConfigurationInfoAsync(List<Guid> specifyContainerIds)
        {
            try
            {
                var hasRunningProfileJob = _jobMonitorService.GetRunningJobsCount(JobType.DiscoveryProfileJob);
                hasRunningProfileJob += _jobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.DiscoveryProfileJob);
                if (hasRunningProfileJob > 0)
                {
                    return new()
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_HasRunningProfileJob"),
                    };
                }

                var licenseType = await RMDiscoveryOffice365LicenseHelper.GetLicenseTypeAsync();
                if (licenseType == Cloud.Sdk.Data.AosModern.LicenseType.Trial)
                {
                    return new()
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = I18NEntity.GetString("Trial license not support process append job."),
                    };
                }

                var checker = new RMDiscoveryOffice365ConfigurationAppendChecker(specifyContainerIds);
                var (isPassed, message) = await checker.CheckAsync();
                if (!isPassed)
                {
                    _logger.Warn($"Append Security check failed.");
                    return new()
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = I18NEntity.GetString(message)
                    };
                }

                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryConfiguration, TimeSpan.FromMinutes(15)))
                {
                    if (!await RMDiscoveryOffice365LicenseHelper.IsMeetLimitAsync())
                    {
                        return new()
                        {
                            MessageType = RAMessageType.Failed,
                            ErrorMessage = I18NEntity.GetString("RM_FA_License_JobLimit"),
                        };
                    }

                    var canRescanContaienrs = await GetAppendAvailableOpusContainerAsync();
                    var canRescanContainerIds = canRescanContaienrs.Select(item => new Guid(item.id));
                    if (canRescanContainerIds.Intersect(specifyContainerIds).Count() != specifyContainerIds.Count)
                    {
                        return new()
                        {
                            MessageType = RAMessageType.Failed,
                            ErrorMessage = I18NEntity.GetString("The specify container ids has not exsit in can rescan containers."),
                        };
                    }

                    await _configurationDao.AddOrUpdateAsync(new RMDiscoveryConfiguration
                    {
                        ConfigurationType = RMDiscoveryConfigurationType.Office365AppendScope,
                        ValueJson = JsonConvert.SerializeObject(new RMDiscoveryOffice365ScopeInfo
                        {
                            ScopeType = RMDiscoveryOffice365ScopeType.Specify,
                            SpecifyContainerIds = specifyContainerIds
                        })
                    });

                    var preparer = new RMDiscoveryOffice365JobAppendPreparer();
                    var (success, errorMessage) = await preparer.PrepareAsync();

                    _logger.Info($"Prepare append discovery job is [{success}].");

                    return new()
                    {
                        MessageType = success ? RAMessageType.Successful : RAMessageType.Failed,
                        ErrorMessage = errorMessage
                    };
                }
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while add or update append configuration info. Error: {e}");
                return new()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed"),
                };
            }
        }

        public async Task<List<RemoteWebApplication>> GetAppendAvailableOpusContainerAsync()
        {
            try
            {
                var opusContainers = await _nodeDao.GetOpusContainersAsync(SourceFlag.SharePoint, SourceFlag.OneDrive);
                var o365TenantInfoes = await _o365TenantDao.GetAllAsync();
                var discoveredContainers = await o365TenantInfoes.ConvertAllAsync(async item => await _nodeDao.GetAllDiscoveryContainersAsync(item.UniqueId));
                var discoveredContainerIds = discoveredContainers.SelectMany(item => item.Select(item => item.OpusId)).ToHashSet();
                return opusContainers.Where(item => !discoveredContainerIds.Contains(new Guid(item.Id)))
                    .ConvertAll(item => new RemoteWebApplication
                    {
                        id = item.Id,
                        url = item.Url,
                        AosId = item.AosId
                    }).
                    ToList();
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while get can append opus container. Error: {e}");
                return [];
            }
        }

        #endregion

        #region Rerun
        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryConfiguration, Action = AuditAction.DiscoveryRescanFailedSite, IAsyncBeforeHandler = typeof(RMDiscoveryOffice365ConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryOffice365ConfigurationAfterAuditHandler))]
        public async Task<RAReturnMessage> AddOrUpdateRerunConfigurationAsync()
        {
            try
            {
                var hasRunningProfileJob = _jobMonitorService.GetRunningJobsCount(JobType.DiscoveryProfileJob);
                hasRunningProfileJob += _jobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.DiscoveryProfileJob);
                if (hasRunningProfileJob > 0)
                {
                    return new()
                    {
                        MessageType = RAMessageType.Failed,
                        ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_HasRunningProfileJob"),
                    };
                }
                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryConfiguration, TimeSpan.FromMinutes(15)))
                {
                    var (has, jobInfo) = await _discoveryJobDao.TryGetLatestMainJobAsync(RMDiscoveryJobType.Newly, RMDiscoveryJobType.Append, RMDiscoveryJobType.Retry);
                    if (!has)
                    {
                        return new()
                        {
                            MessageType = RAMessageType.Failed,
                            ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed"),
                        };
                    }

                    var preparer = new RMDiscoveryOffice365JobRetryPreparer(jobInfo.Id);
                    var (success, errorMessage) = await preparer.PrepareAsync();

                    _logger.Info($"Prepare retry discovery job is [{success}].");
                    return new()
                    {
                        MessageType = success ? RAMessageType.Successful : RAMessageType.Failed,
                        ErrorMessage = errorMessage
                    };
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while retry failed analysis job. Error: {e}");
                return new()
                {
                    MessageType = RAMessageType.Failed,
                    ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed"),
                };
            }
        }

        #endregion

        #region Cost Saving

        public async Task<RMDiscoveryOffice365CostSavingInfo> GetCostSavingInfoAsync()
        {
            var defaultCostInfo = new RMDiscoveryOffice365CostSavingInfo
            {
                SPFreeStorage = 0,
                SPStoragePrice = 0.20,
                ODFreeStorage = 0,
                ODStoragePrice = 0,
                ArchivedDataStoragePrice = 0.00,
            };
            try
            {
                var exist = await _configurationDao.ExistAsync(RMDiscoveryConfigurationType.Office365CostSaving);
                if (!exist)
                {
                    await AddOrUpdateCostSavingInfoAsync(defaultCostInfo);
                    return defaultCostInfo;
                }

                var costInfo = await _configurationDao.GetAsync<RMDiscoveryOffice365CostSavingInfo>(RMDiscoveryConfigurationType.Office365CostSaving);
                return costInfo;
            }
            catch (Exception e)
            {
                _logger.Error($"Error occured when get cost info, error : {e}");
                return defaultCostInfo;
            }
        }

        [AsyncAudit(Module = AuditModule.Discovery, Category = AuditCategory.DiscoveryConfiguration, Action = AuditAction.SaveCostSavingInfo, IAsyncBeforeHandler = typeof(RMDiscoveryOffice365ConfigurationBeforeAuditHandler), IAsyncAfterHandler = typeof(RMDiscoveryOffice365ConfigurationAfterAuditHandler))]
        public async Task<RAReturnMessage> AddOrUpdateCostSavingInfoAsync(RMDiscoveryOffice365CostSavingInfo costInfo)
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
                var exist = await _configurationDao.ExistAsync(RMDiscoveryConfigurationType.Office365CostSaving);
                var costConfigInfo = new RMDiscoveryConfiguration
                {
                    ConfigurationType = RMDiscoveryConfigurationType.Office365CostSaving,
                    ValueJson = JsonConvert.SerializeObject(costInfo),
                    ModifiedTime = utcNow,
                };

                if (!exist)
                {
                    costConfigInfo.CreateTime = utcNow;
                }
                else
                {
                    var beforeCostInfo = (await _configurationDao.GetAsync(RMDiscoveryConfigurationType.Office365CostSaving)).First();
                    costConfigInfo.CreateTime = beforeCostInfo.CreateTime;
                }
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

        #endregion

        #region Report

        public async Task<string> DownloadDiscoveryJobReportAsync()
        {
            try
            {
                var (has, jobInfo) = await _discoveryJobDao.TryGetLatestMainJobAsync();
                var reportManager = new RMDiscoveryOffice365JobReportManager(jobInfo.Id);
                return await reportManager.GenerateReportAsync();
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while download discovery job report. Error: {e}");
                return string.Empty;
            }
        }

        #endregion

        #region Job

        public void SendCalculateJob(Guid mainJobId)
        {
            try
            {
                var queueCount = _jobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.DiscoveryReCalculate);
                var jobCount = _jobMonitorService.GetRunningJobsCount(JobType.DiscoveryReCalculate);
                if (queueCount + jobCount > 0)
                {
                    _logger.Warn("Recalculating job already exists. Skipped send.");
                    return;
                }
                JobQueueDto jqDto = new()
                {
                    JobType = JobType.DiscoveryReCalculate,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = "RM_TS_RunSchedule",
                    Parameters = mainJobId.ToString()
                };
                _jobQueueService.AddToDBJobQueue(jqDto);
                _logger.Info($"The discovery main job [{mainJobId}] doesn't has processing discovery job. Start run calculate job.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while send main job [{mainJobId}] calculate job. Error: {e}");
            }
        }

        public string RealRunCalculateJob(string parameters)
        {
            try
            {
                var jobId = _jobMonitorService.CreateDiscoveryRetryJobAsync("RM_TS_RunSchedule", new Guid(parameters), Guid.Empty).GetAwaiter().GetResult();
                _jobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage()
                {
                    JobId = jobId,
                    JobType = Contract.JobMonitor.JobType.DiscoveryReCalculate,
                    CommandLine = $"{JobType.DiscoveryReCalculate} {jobId}",
                });
                return jobId;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while real run re calculate job. Error: {e}");
                return string.Empty;
            }
        }

        public void SendNextVersionDiscoveryAnalysisJob(Guid mainJobId)
        {
            try
            {
                var (has, mainJob) = _discoveryJobDao.TryGetMainJobAsync(mainJobId).GetAwaiter().GetResult();
                var jobType = mainJob.Version.ToOffice365JobType();
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

        public string RealRunNextVersionDiscoveryAnalysisJob(string parameters)
        {
            try
            {
                var (has, mainJob) = _discoveryJobDao.TryGetMainJobAsync(new Guid(parameters)).GetAwaiter().GetResult();
                var jobType = mainJob.Version.ToOffice365JobType();
                var jobId = _jobMonitorService.CreateDiscoveryJobNextVersionAsync("RM_TS_RunSchedule", new Guid(parameters), jobType).GetAwaiter().GetResult();
                _jobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage()
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

        #endregion
    }
}
