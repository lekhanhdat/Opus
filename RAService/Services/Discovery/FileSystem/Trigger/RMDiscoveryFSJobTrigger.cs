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
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Discovery.Model.Rule.Condition;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.FileSystem;
using AvePoint.RA.DB.Dao.Discovery.Impl.FileSystem;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.FileSystem;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Work;
using AvePoint.RA.Service.Services.Discovery.FileSystem.Work.Trigger;
using DataOrchestration.Tag.Sdk.Service.CloudRecords.Contract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.FileSystem.Trigger
{
    public class RMDiscoveryFSJobTrigger : RMDiscoveryFSWorker
    {
        private readonly IJobMonitorService _jobMonitorService;

        private readonly IRMSubJobDao _subJobDao;

        private readonly IRMDiscoveryFSSizeRangeDao _sizeRangeDao;

        private readonly IRMDiscoveryFSWithoutInDateDao _withoutInDateDao;

        private readonly IGeneralSettingService _generalSettingService;

        private readonly IRMDiscoveryFSConfigurationService _discoveryFSConfiguration;

        private readonly IRMDiscoveryFSTagRuleInfoDao _tagRuleInfoDao;

        private readonly IAgentMgmtService _agentMgmtService;

        public RMDiscoveryFSJobTrigger() : base()
        {
            _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();
            _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
            _sizeRangeDao = new RMDiscoveryFSSizeRangeDao();
            _withoutInDateDao = new RMDiscoveryFSWithoutInDateDao();
            _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
            _discoveryFSConfiguration = new RMDiscoveryFSConfigurationService();
            _tagRuleInfoDao = new RMDiscoveryFSTagRuleInfoDao();
            _agentMgmtService = PlatformWindsorManager.GetService<IAgentMgmtService>();
        }

        public async Task TriggerAsync()
        {
            try
            {
                var (has, mainJob) = await _jobDao.TryGetMainJobAsync(RMDiscoveryJobStatus.Preparing);
                if (!has)
                {
                    return;
                }

                _logger.Info($"Start trigger [{mainJob.Type}] job [{mainJob.Id}].");

                mainJob.Status = RMDiscoveryJobStatus.Pending;
                await _jobDao.AddOrUpdateMainJobAsync(mainJob);

                _logger.Info($"The [{mainJob.Type}] job [{mainJob.Id}] is set to [{RMDiscoveryJobStatus.Pending}] status.");

                await HandleFSJob(mainJob);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while trigger discovery job. Error: {e}");
            }
        }

        private async Task HandleFSJob(RMDiscoveryFSMainJob mainJob)
        {
            if (_agentMgmtService.HasAgentsInUpgradingProcess())
            {
                _logger.Warn("There are agents in upgrading process, postpone triggering discovery job.");
                await SetJobToFailedAsync(mainJob);
                return;
            }

            if (mainJob.NeedToReRegisterTags)
            {
                if (!await RegisterTagsAsync(mainJob))
                {
                    await SetJobToFailedAsync(mainJob);
                    return;
                }
            }

            IRMDiscoveryFSJobTriggerible trigger = mainJob.Type switch
            {
                RMDiscoveryJobType.Newly => new RMDiscoveryFSJobNewlyTrigger(mainJob),
                _ => throw new NotSupportedException(mainJob.Type.ToString()),
            };

            var (succeed, items) = await trigger.GetWillTriggerJobsAsync();
            if (!succeed)
            {
                await SetJobToFailedAsync(mainJob);
                return;
            }

          
            var initTableSucceed = await trigger.InitTablesAsync();
            if (!initTableSucceed)
            {
                await SetJobToFailedAsync(mainJob);
                return;
            }

            var triggerSucceed = await TriggerJobsAsync(items, mainJob);
            if (!triggerSucceed)
            {
                await SetJobToFailedAsync(mainJob);
                return;
            }

            var connectionCount = items.Sum(item => item.connections.Count);
            mainJob.Status = RMDiscoveryJobStatus.Running;
            mainJob.ConnectionCount = connectionCount;
            await _jobDao.AddOrUpdateMainJobAsync(mainJob);
            _logger.Info($"This [{mainJob.Type}] job [{mainJob.Id}] is set to [{RMDiscoveryJobStatus.Running}] status, ConnectionCount:[{mainJob.ConnectionCount}].");
        }

        private async Task<bool> TriggerJobsAsync(List<(FSConnectionGroup group, List<FSConnection> connections)> items, RMDiscoveryFSMainJob mainJob)
        {
            var mainJobId = mainJob.Id;
            try
            {
                foreach (var (container, connections) in items)
                {
                    var triggerTime = DateTime.UtcNow.Ticks;
                    var needTriggerSites = connections.Where(item => item.FailedCause != RMDiscoveryJobFailedCause.AnalysisFailed).ToList();
                    var discoveryJobRealId = string.Empty;
                    if (connections.Count > 0)
                    {
                        var discoveryJobInfoId = await _discoveryFSConfiguration.RealRunScanFSDiscoveryJob(
                            "RM_TS_RunSchedule",
                            mainJobId,
                            needTriggerSites.Select(item => new RMFSDiscoveryJobSettingDto
                            {
                                ConnectionId = item.Id,
                                UNCPath = item.UNCPath,
                                ConnectionGroupId = container.Id,
                            }).ToList());
                        discoveryJobRealId = discoveryJobInfoId;
                        _logger.Info($"Successful trigger job [{discoveryJobInfoId}] in ie.");
                    }

                    var discoveryJobId = Guid.NewGuid();
                    await _jobDao.AddOrUpdateDiscoveryJobAsync(new RMDiscoveryFSDiscoveryJob
                    {
                        Id = discoveryJobId,
                        RealId = discoveryJobRealId,
                        MainJobId = mainJobId,
                        O365TenantId = Guid.Empty,
                        ContainerId = container.Id,
                        ContainerName = container.Name,
                        ConnectionCount = connections.Count,
                        Status = connections.Count > 0 ? RMDiscoveryJobStatus.Pending : RMDiscoveryJobStatus.Completing,
                        StartTime = DateTime.UtcNow.Ticks,
                        LastCheckedTime = triggerTime,
                    });
                    _logger.Info($"Successful add discovery job [{discoveryJobId} - {discoveryJobRealId}] to opus db.");

                    var analysisJobs = connections.ConvertAll(item => new RMDiscoveryFSAnalysisJob
                    {
                        Id = Guid.NewGuid(),
                        ConnectionName = item.Name,
                        MainJobId = mainJobId,
                        DiscoveryJobId = discoveryJobId,
                        ContainerId = container.Id,
                        ConnectionId = item.Id,
                        UNCPath = item.UNCPath,
                        Status = item.FailedCause == RMDiscoveryJobFailedCause.AnalysisFailed ? RMDiscoveryJobStatus.Pending : RMDiscoveryJobStatus.Preparing,
                        StartTime = DateTime.UtcNow.Ticks,
                    });

                    await _jobDao.BatchInsertAnalysisJobAsync(analysisJobs);
                    _logger.Info($"Successful add discovery analysis jobs [{analysisJobs.Count}] to opus db.");
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while trigger jobs. Error: {e}");
                return false;
            }
        }

        private async Task SetJobToFailedAsync(RMDiscoveryFSMainJob mainJob)
        {
            _logger.Info($"Set job [{mainJob.Id}] to failed status due to failed tags registration");
            mainJob.Status = RMDiscoveryJobStatus.Failed;
            mainJob.EndTime = DateTime.UtcNow.Ticks;
            mainJob.ProfileJobInitStatus = RMDiscoveryJobStatus.Finished;
            await _jobDao.AddOrUpdateMainJobAsync(mainJob);
        }

        private async Task<bool> RegisterTagsAsync(RMDiscoveryFSMainJob mainJob)
        {
            try
            {
                var contentSource = SourceFlag.FileSystem;
                var enabledRules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive, RMDiscoveryRuleDefinitionKind.ROT);

                var customerTagRules = await GetCustomerTagRulesAsync(enabledRules);
                var buildInTagRules = await GetBuildInTagRulesAsync();
                var rotTagRules = await GetRotTagRulesAsync(mainJob, enabledRules);

                var localTagRules = customerTagRules.Concat(buildInTagRules).Concat(rotTagRules).ToList();
                var localTagRuleIds = localTagRules.Select(item => item.TagId).ToHashSet();

                var opusTagRules = await _tagRuleInfoDao.GetAllAsync();
                var opusTagRuleIds = opusTagRules.Select(item => item.TagId).ToHashSet();

                var needDeleteTagIds = opusTagRuleIds.Except(localTagRuleIds).ToList();
                if (needDeleteTagIds.Any()) await _tagRuleInfoDao.DeleteAsync(needDeleteTagIds);
                _logger.Info($"Successful delete [{contentSource}] tags: [{string.Join(",", needDeleteTagIds)}] from opus.");

                var needAddTags = localTagRuleIds.Except(opusTagRuleIds).ConvertAll(item => localTagRules.First(i => i.TagId == item)).ToList();
                if (needAddTags.Any()) await _tagRuleInfoDao.AddOrUpdateAsync(needAddTags);
                _logger.Info($"Successful add [{contentSource}] tags: [{string.Join(",", needAddTags.Select(item => item.TagId))}] to opus.");

                var needUpdateTags = localTagRuleIds.Intersect(opusTagRuleIds).ConvertAll(item => localTagRules.First(i => i.TagId == item)).ToList();
                if (needUpdateTags.Any()) await _tagRuleInfoDao.BatchUpdateAsync(needUpdateTags);
                _logger.Info($"Successful update [{contentSource}] tags: [{string.Join(",", needUpdateTags.Select(item => item.TagId))}] to opus.");

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while register tags. Error: {e}");
                return false;
            }
        }

        private async Task<List<RMDiscoveryFSTagRuleInfo>> GetCustomerTagRulesAsync(List<RMDiscoveryFSRuleInfo> ruleInfoes)
        {
            var gls = await _generalSettingService.GetGeneralSettingAsync();

            var tagRuleModels = ruleInfoes.ConvertAll(rule =>
            {
                var tag = new TagInfo
                {
                    IsBuildIn = false,
                };

                var criteriaInfoes = JsonConvert.DeserializeObject<List<RMDiscoveryRuleCriteriaInfo>>(rule.CriteriaInfoesJson);
                var ruleInfo = new RuleInfo
                {
                    Method = (AnalyseMethod)rule.AnalyseMethod,
                    CriteriaInfoes = criteriaInfoes.ConvertAll(criteriaInfo => new CriteriaInfo
                    {
                        CriteriaType = criteriaInfo.CriteriaType,
                        Order = criteriaInfo.Order,
                        LogicType = (CriteriaLogicType)criteriaInfo.LogicType,
                        ConditionInfo = new ConditionInfo
                        {
                            Category = (ConditionCategory)criteriaInfo.ConditionInfo.Category,
                            Logic = criteriaInfo.ConditionInfo.Logic,
                            Value = criteriaInfo.ConditionInfo.Category == RMDiscoveryConditionCategory.DateTime && criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.Before ?
                            _generalSettingService.ConvertToUTCDateTime(criteriaInfo.ConditionInfo.Value, gls, "yyyy/MM/dd HH:mm") : criteriaInfo.ConditionInfo.Value,
                        }
                    })
                };

                tag.TagDefinition = JsonConvert.SerializeObject(ruleInfo);
                return new RMDiscoveryFSTagRuleInfo
                {
                    TagId = rule.UniqueId,
                    Name = rule.Name,
                    Definition = JsonConvert.SerializeObject(tag),
                    NeedCalculation = true
                };
            });
            return tagRuleModels;
        }

        private async Task<List<RMDiscoveryFSTagRuleInfo>> GetBuildInTagRulesAsync()
        {
            var res = new List<RMDiscoveryFSTagRuleInfo>();

            var sizeRanges = await _sizeRangeDao.GetAllAsync();
            var withoutInDateList = await _withoutInDateDao.GetAllAsync();
            res.Add(new RMDiscoveryFSTagRuleInfo
            {
                TagId = RMDiscoveryBuildInRule.SIZE_RANGE_UNIQUE_ID,
                Name = "size_range",
                Definition = JsonConvert.SerializeObject(new TagInfo
                {
                    IsBuildIn = true,
                    TagDefinition = JsonConvert.SerializeObject(new BuildInRuleInfo
                    {
                        RuleType = BuildInRuleType.DocumentSizeRange,
                        AdditionalInformation = JsonConvert.SerializeObject(sizeRanges)
                    })
                }),
                NeedCalculation = true
            });
            res.Add(new RMDiscoveryFSTagRuleInfo
            {
                TagId = RMDiscoveryBuildInRule.WITHOUT_IN_DATE_UNIQUE_ID,
                Name = "with_in_date",
                Definition = JsonConvert.SerializeObject(new TagInfo
                {
                    IsBuildIn = true,
                    TagDefinition = JsonConvert.SerializeObject(new BuildInRuleInfo
                    {
                        RuleType = BuildInRuleType.DocumentWithoutModifiedIn,
                        AdditionalInformation = JsonConvert.SerializeObject(withoutInDateList)
                    })
                }),
                NeedCalculation = true
            });
            res.Add(new RMDiscoveryFSTagRuleInfo
            {
                TagId = RMDiscoveryBuildInRule.ARCHVIED_UNIQUE_ID,
                Name = "is_archived",
                NeedCalculation = false
            });
            res.Add(new RMDiscoveryFSTagRuleInfo
            {
                TagId = RMDiscoveryBuildInRule.DUPLICATE_UNIQUE_ID,
                Name = "is_duplicate",
                NeedCalculation = false
            });
            return res;
        }

        private async Task<List<RMDiscoveryFSTagRuleInfo>> GetRotTagRulesAsync(RMDiscoveryFSMainJob mainJob, List<RMDiscoveryFSRuleInfo> ruleInfoes)
        {
            var gls = await _generalSettingService.GetGeneralSettingAsync();
            var res = new List<RMDiscoveryFSTagRuleInfo>();

            var rCategoryRules = ruleInfoes.Where(item => item.Category == RMDiscoveryRuleCategory.Redundant).ToList();
            if (rCategoryRules.Any())
            {
                var tagInfo = new TagInfo
                {
                    IsBuildIn = true,
                    TagDefinition = JsonConvert.SerializeObject(new BuildInRuleInfo
                    {
                        RuleType = BuildInRuleType.ROTRuleContainer,
                        AdditionalInformation = JsonConvert.SerializeObject(rCategoryRules.ConvertAll(item => ConvertToRuleInfo(item, gls))),
                    })
                };

                res.Add(new RMDiscoveryFSTagRuleInfo
                {
                    TagId = RMDiscoveryBuildInRule.R_CATEGORY_RULE_UNIQUE_ID,
                    Name = "r_category_rule",
                    Definition = JsonConvert.SerializeObject(tagInfo),
                    NeedCalculation = true,
                });
            }

            var oCategoryRules = ruleInfoes.Where(item => item.Category == RMDiscoveryRuleCategory.Obsolete).ToList();
            if (oCategoryRules.Any())
            {
                var tagInfo = new TagInfo
                {
                    IsBuildIn = true,
                    TagDefinition = JsonConvert.SerializeObject(new BuildInRuleInfo
                    {
                        RuleType = BuildInRuleType.ROTRuleContainer,
                        AdditionalInformation = JsonConvert.SerializeObject(oCategoryRules.ConvertAll(item => ConvertToRuleInfo(item, gls))),
                    })
                };

                res.Add(new RMDiscoveryFSTagRuleInfo
                {
                    TagId = RMDiscoveryBuildInRule.O_CATEGORY_RULE_UNIQUE_ID,
                    Name = "o_category_rule",
                    Definition = JsonConvert.SerializeObject(tagInfo),
                    NeedCalculation = true,
                });
            }

            var tCategoryRules = ruleInfoes.Where(item => item.Category == RMDiscoveryRuleCategory.Trivial).ToList();

            if (tCategoryRules.Any())
            {
                var tagInfo = new TagInfo
                {
                    IsBuildIn = true,
                    TagDefinition = JsonConvert.SerializeObject(new BuildInRuleInfo
                    {
                        RuleType = BuildInRuleType.ROTRuleContainer,
                        AdditionalInformation = JsonConvert.SerializeObject(tCategoryRules.ConvertAll(item => ConvertToRuleInfo(item, gls))),
                    })
                };

                res.Add(new RMDiscoveryFSTagRuleInfo
                {
                    TagId = RMDiscoveryBuildInRule.T_CATEGORY_RULE_UNIQUE_ID,
                    Name = "t_category_rule",
                    Definition = JsonConvert.SerializeObject(tagInfo),
                    NeedCalculation = true,
                });
            }

            if (ruleInfoes.Any())
            {
                var tagInfo = new TagInfo
                {
                    IsBuildIn = true,
                    TagDefinition = JsonConvert.SerializeObject(new BuildInRuleInfo
                    {
                        RuleType = BuildInRuleType.ROTRuleContainer,
                        AdditionalInformation = JsonConvert.SerializeObject(ruleInfoes.Where(item => item.DefinitionKind == RMDiscoveryRuleDefinitionKind.ROT && item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ConvertAll(item => ConvertToRuleInfo(item, gls))),
                    })
                };

                res.Add(new RMDiscoveryFSTagRuleInfo
                {
                    TagId = RMDiscoveryBuildInRule.ROT_RULE_UNIQUE_ID,
                    Name = "rot_rule",
                    Definition = JsonConvert.SerializeObject(tagInfo),
                    NeedCalculation = true,
                });
            }

            return res;
        }

        private RuleInfo ConvertToRuleInfo(RMDiscoveryFSRuleInfo discoveryRuleInfo, GeneralSettingModel gls)
        {
            var criteriaInfoes = JsonConvert.DeserializeObject<List<RMDiscoveryRuleCriteriaInfo>>(discoveryRuleInfo.CriteriaInfoesJson);
            return new RuleInfo
            {
                Method = (AnalyseMethod)discoveryRuleInfo.AnalyseMethod,
                CriteriaInfoes = criteriaInfoes.ConvertAll(criteriaInfo => new CriteriaInfo
                {
                    CriteriaType = criteriaInfo.CriteriaType,
                    Order = criteriaInfo.Order,
                    LogicType = (CriteriaLogicType)criteriaInfo.LogicType,
                    ConditionInfo = new ConditionInfo
                    {
                        Category = (ConditionCategory)criteriaInfo.ConditionInfo.Category,
                        Logic = criteriaInfo.ConditionInfo.Logic,
                        Value = criteriaInfo.ConditionInfo.Category == RMDiscoveryConditionCategory.DateTime && criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.Before ?
                        _generalSettingService.ConvertToUTCDateTime(criteriaInfo.ConditionInfo.Value, gls, "yyyy/MM/dd HH:mm") : criteriaInfo.ConditionInfo.Value,
                    }
                })
            };
        }
    }
}
