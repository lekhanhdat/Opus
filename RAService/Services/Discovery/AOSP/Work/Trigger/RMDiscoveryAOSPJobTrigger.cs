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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Discovery.Model.Rule.Condition;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Impl.AOSP;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using AvePoint.RA.Service.Services.Discovery.Office365.Work.Trigger;
using Cloud.Sdk.Data.IE;
using DataOrchestration.Tag.Sdk.Service.CloudRecords.Contract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.AOSP.Work.Trigger
{
    public class RMDiscoveryAOSPJobTrigger : RMDiscoveryAOSPWorker
    {
        private readonly IJobMonitorService _jobMonitorService;

        private readonly IRMSubJobDao _subJobDao;

        private readonly IRMDiscoveryAOSPSizeRangeDao _sizeRangeDao;

        private readonly IRMDiscoveryAOSPWithoutInDateDao _withoutInDateDao;

        private readonly IGeneralSettingService _generalSettingService;

        public RMDiscoveryAOSPJobTrigger() : base()
        {
            _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();
            _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
            _sizeRangeDao = new RMDiscoveryAOSPSizeRangeDao();
            _withoutInDateDao = new RMDiscoveryAOSPWithoutInDateDao();
            _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
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
                mainJob.Comment = string.Empty;
                await _jobDao.AddOrUpdateMainJobAsync(mainJob);

                _logger.Info($"The [{mainJob.Type}] job [{mainJob.Id}] is set to [{RMDiscoveryJobStatus.Pending}] status.");

                await HandleAOSPJob(mainJob);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while trigger discovery job. Error: {e}");
            }
        }

        private async Task HandleAOSPJob(RMDiscoveryAOSPMainJob mainJob)
        {
            if (mainJob.NeedToReRegisterTags)
            {
                var (isRegisterTagsSuccess, registerTagsErrorMessage) = await RegisterTagsAsync(mainJob);
                if (!isRegisterTagsSuccess)
                {
                    await SetJobToFailedAsync(mainJob, registerTagsErrorMessage);
                    return;
                }
            }

            IRMDiscoveryAOSPJobTriggerible trigger = mainJob.Type switch
            {
                RMDiscoveryJobType.Newly => new RMDiscoveryAOSPJobNewlyTrigger(mainJob),
                RMDiscoveryJobType.Rescan => new RMDiscoveryAOSPJobRescanTrigger(mainJob),
                _ => throw new NotSupportedException($"Not support trigger type [{mainJob.Type}]"),
            };

            var (succeed, items, errorMessage) = await trigger.GetWillTriggerJobsAsync();
            if (!succeed)
            {
                await SetJobToFailedAsync(mainJob, errorMessage);
                return;
            }

            var (initTableSucceed, initTableErrorMessage) = await trigger.InitTablesAsync(items.Select(item => item.o365TenantId).ToHashSet().ToList());
            if (!initTableSucceed)
            {
                await SetJobToFailedAsync(mainJob, initTableErrorMessage);
                return;
            }

            var (triggerSucceed, triggerErrorMessage) = await TriggerJobsAsync(items, mainJob);
            if (!triggerSucceed)
            {
                await SetJobToFailedAsync(mainJob, triggerErrorMessage);
                return;
            }

            var sitesCount = items.Sum(item => item.sites.Count);
            mainJob.Status = RMDiscoveryJobStatus.Running;
            mainJob.SitesCount = sitesCount;
            mainJob.Comment = string.Empty;
            await _jobDao.AddOrUpdateMainJobAsync(mainJob);
            _logger.Info($"This [{mainJob.Type}] job [{mainJob.Id}] is set to [{RMDiscoveryJobStatus.Running}] status, SiteCollectionCount:[{mainJob.SitesCount}].");
        }

        private async Task<(bool succeed, string errorMessage)> TriggerJobsAsync(List<(Guid o365TenantId, RMRemoteNode container, List<RMRemoteNode> sites)> items, RMDiscoveryAOSPMainJob mainJob)
        {
            var mainJobId = mainJob.Id;
            try
            {
                foreach (var (o365TenantId, container, sites) in items)
                {
                    var contentSource = GetContainerContentSource(container);
                    var configurationType = RMDiscoveryConfigurationType.AOSPAllowLockedSites;
                    var tenantId = o365TenantId.ToString();
                    var defaultAllowLockedSites = false;
                    _logger.Info($"Reading AOSP locked-sites configuration. TenantId:[{tenantId}], " +
                                 $"ConfigurationType:[{configurationType}], DefaultValue:[{defaultAllowLockedSites}].");
                    var isAllowLockedSites = await _configurationDao.GetByO365TenantIdAsync(
                        configurationType,
                        tenantId,
                        defaultAllowLockedSites);
                    _logger.Info($"Loaded AOSP locked-sites configuration. TenantId:[{o365TenantId}], " +
                                 $"ConfigurationType:[{configurationType}], " +
                                 $"IsAllowLockedSites:[{isAllowLockedSites}].");

                    var triggerTime = DateTime.UtcNow.Ticks;
                    var needTriggerSites = sites.Where(item => item.FailedCause != RMDiscoveryJobFailedCause.AnalysisFailed).ToList();
                    var appProfileId = mainJob.AppProfileId;
                    var discoveryJobRealId = Guid.Empty;
                    if (needTriggerSites.Count > 0)
                    {
                        var ieJobInitModel = new SharePointJobInitionModel
                        {
                            Type = contentSource == SourceFlag.SharePoint ? DataType.SPDocument : DataType.SPOneDriveDocument,
                            SiteInfos = needTriggerSites.ConvertAll(item => new SiteInfoModel(item.ObjectId, item.Url)),
                            AzureTenantId = o365TenantId.ToString(),
                            EnforceTagRuleCheck = true,
                            AppProfileId = mainJob.AppProfileId,
                            IsAllowLockedSites = isAllowLockedSites,
                        };
                        _logger.Info($"Assigned AOSP locked-sites value to SharePoint job initialization. " +
                                     $"TenantId:[{o365TenantId}], ContentSource:[{contentSource}], " +
                                     $"SiteCount:[{ieJobInitModel.SiteInfos.Count}], " +
                                     $"IsAllowLockedSites:[{ieJobInitModel.IsAllowLockedSites}].");
                        //ApplyAllowLockedSites(ieJobInitModel, isAllowLockedSites);
                        var discoveryJobInfo = await _ieApiClient.JobService.TriggerAsync(ieJobInitModel);
                        discoveryJobRealId = discoveryJobInfo.Id;
                        _logger.Info($"Successful trigger job [{discoveryJobInfo.Id}] in ie. " +
                                     $"TenantId:[{tenantId}], IsAllowLockedSites:[{ieJobInitModel.IsAllowLockedSites}].");
                    }
                    var discoveryJobId = Guid.NewGuid();
                    await _jobDao.AddOrUpdateDiscoveryJobAsync(new RMDiscoveryAOSPDiscoveryJob
                    {
                        Id = discoveryJobId,
                        RealId = discoveryJobRealId,
                        MainJobId = mainJobId,
                        O365TenantId = o365TenantId,
                        ContainerId = new Guid(container.Id),
                        ContainerName = container.Url,
                        SiteCount = sites.Count,
                        Status = needTriggerSites.Count > 0 ? RMDiscoveryJobStatus.Pending : RMDiscoveryJobStatus.Completing,
                        StartTime = DateTime.UtcNow.Ticks,
                        LastCheckedTime = triggerTime,
                        ContentSource = contentSource,
                        Comment = string.Empty,
                    });
                    _logger.Info($"Successful add discovery job [{discoveryJobId} - {discoveryJobRealId}] to opus db.");

                    var analysisJobs = sites.ConvertAll(item => new RMDiscoveryAOSPAnalysisJob
                    {
                        Id = Guid.NewGuid(),
                        MainJobId = mainJobId,
                        DiscoveryJobId = discoveryJobId,
                        O365TenantId = o365TenantId,
                        ContainerId = new Guid(container.Id),
                        SiteId = new Guid(item.ObjectId),
                        Url = item.Url,
                        Status = item.FailedCause == RMDiscoveryJobFailedCause.AnalysisFailed ? RMDiscoveryJobStatus.Pending : RMDiscoveryJobStatus.Preparing,
                        StartTime = DateTime.UtcNow.Ticks,
                        Comment = string.Empty,
                    });

                    await _jobDao.BatchInsertAnalysisJobAsync(analysisJobs);
                    _logger.Info($"Successful add discovery analysis jobs [{analysisJobs.Count}] to opus db.");
                }
                return (true, string.Empty);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while trigger jobs. Error: {e}");
                return (false, e.Message);
            }
        }

        //private void ApplyAllowLockedSites(SharePointJobInitionModel jobInitModel, bool isAllowLockedSites)
        //{
        //    PropertyInfo property = typeof(SharePointJobInitionModel).GetProperty("IsAllowLockedSites");

        //    if (property?.CanWrite == true && property.PropertyType == typeof(bool))
        //    {
        //        property.SetValue(jobInitModel, isAllowLockedSites);
        //        return;
        //    }

        //    _logger.Warn($"SharePointJobInitionModel does not expose a writable locked-sites flag. Value:{isAllowLockedSites}");
        //}

        private async Task<(bool,string)> RegisterTagsAsync(RMDiscoveryAOSPMainJob mainJob)
        {
            try
            {
                var enabledRules = await _ruleInfoDao.GetRuleInfoesAsync(true, mainJob.O365TenantId, RMDiscoveryRuleDefinitionKind.Inactive, RMDiscoveryRuleDefinitionKind.ROT);
                foreach (var contentSource in new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.OneDrive })
                {
                    var customerTagRules = await GetCustomerTagRulesAsync(contentSource, enabledRules);
                    var buildInTagRules = await GetBuildInTagRulesAsync(mainJob.O365TenantId, contentSource);
                    var rotTagRules = await GetRotTagRulesAsync(mainJob, contentSource, enabledRules);
                    var localTagRules = customerTagRules.Concat(buildInTagRules).Concat(rotTagRules).ToList();
                    var localTagRuleIds = localTagRules.Select(item => item.Id).ToHashSet();

                    var aosTagRules = await _ieApiClient.TagRuleService.GetAllAsync(contentSource == SourceFlag.SharePoint ? DataType.SPDocument : DataType.SPOneDriveDocument, Cloud.Sdk.Data.Core.CallerType.CloudRecords);
                    var aosTagRuleIds = aosTagRules.Select(item => item.Id).ToHashSet();

                    var needDeleteTagIds = aosTagRuleIds.Except(localTagRuleIds).ToList();
                    await _ieApiClient.TagRuleService.DeleteBatchAsync(needDeleteTagIds.ConvertAll(item => new TagRuleDeletionModel { Id = item, DataType = contentSource == SourceFlag.SharePoint ? DataType.SPDocument : DataType.SPOneDriveDocument }));
                    _logger.Info($"Successful delete [{contentSource}] tags: [{string.Join(",", needDeleteTagIds)}] from aos.");

                    var needAddTags = localTagRuleIds.Except(aosTagRuleIds).ConvertAll(item => localTagRules.First(i => i.Id == item)).ToList();
                    await _ieApiClient.TagRuleService.AddBatchAsync(needAddTags);
                    _logger.Info($"Successful add [{contentSource}] tags: [{string.Join(",", needAddTags.Select(item => item.Id))}] to aos.");

                    var needUpdateTags = localTagRuleIds.Intersect(aosTagRuleIds).ConvertAll(item => localTagRules.First(i => i.Id == item)).ToList();
                    await _ieApiClient.TagRuleService.UpdateBatchAsync(needUpdateTags);
                    _logger.Info($"Successful update [{contentSource}] tags: [{string.Join(",", needUpdateTags.Select(item => item.Id))}] to aos.");

                    _logger.Info($"Successful register [{contentSource}] tags to aos.");
                }

                return (true,string.Empty);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while register tags. Error: {e}");
                return (false,e.Message);
            }
        }

        private async Task<List<TagRuleModel>> GetCustomerTagRulesAsync(SourceFlag contentSource, List<RMDiscoveryAOSPRuleInfo> ruleInfoes)
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
                return new TagRuleModel
                {
                    Id = rule.UniqueId,
                    Name = rule.Name,
                    Definition = JsonConvert.SerializeObject(tag),
                    Product = Cloud.Sdk.Data.Core.CallerType.CloudRecords,
                    Type = contentSource == SourceFlag.SharePoint ? DataType.SPDocument : DataType.SPOneDriveDocument,
                    NeedCalculation = true
                };
            });

            return tagRuleModels;
        }

        private async Task<List<TagRuleModel>> GetBuildInTagRulesAsync(string o365TenantId ,SourceFlag contentSource)
        {
            var res = new List<TagRuleModel>();

            var sizeRanges = await _sizeRangeDao.GetAllAsync(o365TenantId);
            var withoutInDateList = await _withoutInDateDao.GetAllAsync(o365TenantId);
            res.Add(new TagRuleModel
            {
                Id = RMDiscoveryBuildInRule.SIZE_RANGE_UNIQUE_ID,
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
                Product = Cloud.Sdk.Data.Core.CallerType.CloudRecords,
                Type = contentSource == SourceFlag.SharePoint ? DataType.SPDocument : DataType.SPOneDriveDocument,
                NeedCalculation = true
            });
            res.Add(new TagRuleModel
            {
                Id = RMDiscoveryBuildInRule.WITHOUT_IN_DATE_UNIQUE_ID,
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
                Product = Cloud.Sdk.Data.Core.CallerType.CloudRecords,
                Type = contentSource == SourceFlag.SharePoint ? DataType.SPDocument : DataType.SPOneDriveDocument,
                NeedCalculation = true
            });
            res.Add(new TagRuleModel
            {
                Id = RMDiscoveryBuildInRule.ARCHVIED_UNIQUE_ID,
                Name = "is_archived",
                Product = Cloud.Sdk.Data.Core.CallerType.CloudRecords,
                Type = contentSource == SourceFlag.SharePoint ? DataType.SPDocument : DataType.SPOneDriveDocument,
                NeedCalculation = false
            });
            res.Add(new TagRuleModel
            {
                Id = RMDiscoveryBuildInRule.DUPLICATE_UNIQUE_ID,
                Name = "is_duplicate",
                Product = Cloud.Sdk.Data.Core.CallerType.CloudRecords,
                Type = contentSource == SourceFlag.SharePoint ? DataType.SPDocument : DataType.SPOneDriveDocument,
                NeedCalculation = false
            });

            return res;
        }

        private async Task<List<TagRuleModel>> GetRotTagRulesAsync(RMDiscoveryAOSPMainJob mainJob, SourceFlag contentSource, List<RMDiscoveryAOSPRuleInfo> ruleInfoes)
        {
            var gls = await _generalSettingService.GetGeneralSettingAsync();
            var res = new List<TagRuleModel>();

            var rCategoryRules = ruleInfoes.Where(item => item.Category == RMDiscoveryRuleCategory.Redundant).ToList();
            if (rCategoryRules.Any())
            {
                var tagInfo = new TagInfo
                {
                    IsBuildIn = true,
                    TagDefinition = JsonConvert.SerializeObject(new BuildInRuleInfo
                    {
                        RuleType = BuildInRuleType.ROTRuleContainer,
                        AdditionalInformation = JsonConvert.SerializeObject(rCategoryRules.ConvertAll(item => ConvertToIERuleInfo(item, gls))),
                    })
                };

                res.Add(new TagRuleModel
                {
                    Id = RMDiscoveryBuildInRule.R_CATEGORY_RULE_UNIQUE_ID,
                    Name = "r_category_rule",
                    Definition = JsonConvert.SerializeObject(tagInfo),
                    Product = Cloud.Sdk.Data.Core.CallerType.CloudRecords,
                    Type = contentSource == SourceFlag.SharePoint ? DataType.SPDocument : DataType.SPOneDriveDocument,
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
                        AdditionalInformation = JsonConvert.SerializeObject(oCategoryRules.ConvertAll(item => ConvertToIERuleInfo(item, gls))),
                    })
                };

                res.Add(new TagRuleModel
                {
                    Id = RMDiscoveryBuildInRule.O_CATEGORY_RULE_UNIQUE_ID,
                    Name = "o_category_rule",
                    Definition = JsonConvert.SerializeObject(tagInfo),
                    Product = Cloud.Sdk.Data.Core.CallerType.CloudRecords,
                    Type = contentSource == SourceFlag.SharePoint ? DataType.SPDocument : DataType.SPOneDriveDocument,
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
                        AdditionalInformation = JsonConvert.SerializeObject(tCategoryRules.ConvertAll(item => ConvertToIERuleInfo(item, gls))),
                    })
                };

                res.Add(new TagRuleModel
                {
                    Id = RMDiscoveryBuildInRule.T_CATEGORY_RULE_UNIQUE_ID,
                    Name = "t_category_rule",
                    Definition = JsonConvert.SerializeObject(tagInfo),
                    Product = Cloud.Sdk.Data.Core.CallerType.CloudRecords,
                    Type = contentSource == SourceFlag.SharePoint ? DataType.SPDocument : DataType.SPOneDriveDocument,
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
                        AdditionalInformation = JsonConvert.SerializeObject(ruleInfoes.Where(item => item.DefinitionKind == RMDiscoveryRuleDefinitionKind.ROT && item.AnalyseMethod != RMDiscoveryRuleAnalyseMethod.DuplicatedDocument).ConvertAll(item => ConvertToIERuleInfo(item, gls))),
                    })
                };

                res.Add(new TagRuleModel
                {
                    Id = RMDiscoveryBuildInRule.ROT_RULE_UNIQUE_ID,
                    Name = "rot_rule",
                    Definition = JsonConvert.SerializeObject(tagInfo),
                    Product = Cloud.Sdk.Data.Core.CallerType.CloudRecords,
                    Type = contentSource == SourceFlag.SharePoint ? DataType.SPDocument : DataType.SPOneDriveDocument,
                    NeedCalculation = true,
                });
            }

            return res;
        }

        private RuleInfo ConvertToIERuleInfo(RMDiscoveryAOSPRuleInfo discoveryRuleInfo, GeneralSettingModel gls)
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

        private async Task SetJobToFailedAsync(RMDiscoveryAOSPMainJob mainJob, string errorMessage)
        {
            _logger.Info($"Set job [{mainJob.Id}] to failed status due to failed tags registration");
            mainJob.Status = RMDiscoveryJobStatus.Failed;
            mainJob.EndTime = DateTime.UtcNow.Ticks;
            mainJob.ProfileJobInitStatus = RMDiscoveryJobStatus.Finished;
            mainJob.Comment = errorMessage ?? string.Empty;
            await _jobDao.AddOrUpdateMainJobAsync(mainJob);
        }

        protected static SourceFlag GetContainerContentSource(RMRemoteNode container)
        {
            if (container.NodeLevel == (int)NodeLevel.SkyDriveProGroup)
            {
                return SourceFlag.OneDrive;
            }

            return SourceFlag.SharePoint;
        }

    }
}
