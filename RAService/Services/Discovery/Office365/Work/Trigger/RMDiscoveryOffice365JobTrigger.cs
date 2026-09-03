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
using Aspose.Slides.Export.Web;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Discovery.Model.Rule.Condition;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.Data.IE;
using DataOrchestration.Tag.Sdk.Service.CloudRecords.Contract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Work.Trigger
{
    public class RMDiscoveryOffice365JobTrigger : RMDiscoveryOffice365Worker
    {
        private readonly IJobMonitorService _jobMonitorService;

        private readonly IRMSubJobDao _subJobDao;

        private readonly IRMDiscoveryOffice365SizeRangeDao _sizeRangeDao;

        private readonly IRMDiscoveryOffice365WithoutInDateDao _withoutInDateDao;

        private readonly IGeneralSettingService _generalSettingService;
        private IRMRemoteNodeDao RMRemoteNodeDao;
        private IRMDiscoverySpecificSiteService _specificSiteService;


        public RMDiscoveryOffice365JobTrigger() : base()
        {
            _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();
            _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
            _sizeRangeDao = new RMDiscoveryOffice365SizeRangeDao();
            _withoutInDateDao = new RMDiscoveryOffice365WithoutInDateDao();
            _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();
            RMRemoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
            _specificSiteService = PlatformWindsorManager.GetService<IRMDiscoverySpecificSiteService>();
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

                await HandleMicrosoftOffice365Job(mainJob);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while trigger discovery job. Error: {e}");
            }
        }

        private async Task HandleMicrosoftOffice365Job(RMDiscoveryOffice365MainJob mainJob)
        {
            if (mainJob.NeedToReRegisterTags)
            {
                if (!await RegisterTagsAsync(mainJob))
                {
                    await SetJobToFailedAsync(mainJob);
                    return;
                }
            }

            IRMDiscoveryOffice365JobTriggerible trigger = mainJob.Type switch
            {
                RMDiscoveryJobType.Newly => new RMDiscoveryOffice365JobNewlyTrigger(mainJob),
                RMDiscoveryJobType.Append => new RMDiscoveryOffice365JobAppendTrigger(mainJob),
                RMDiscoveryJobType.Retry => new RMDiscoveryOffice365JobRetryTrigger(mainJob),
                _ => throw new NotSupportedException(mainJob.Type.ToString()),
            };

            var (succeed, items) = await trigger.GetWillTriggerJobsAsync();
            if (!succeed)
            {
                await SetJobToFailedAsync(mainJob);
                return;
            }

            var initTableSucceed = await trigger.InitTablesAsync(items.Select(item => item.o365TenantId).ToHashSet().ToList());
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

            var sitesCount = items.Sum(item => item.sites.Count);
            mainJob.Status = RMDiscoveryJobStatus.Running;
            mainJob.SitesCount = sitesCount;
            await _jobDao.AddOrUpdateMainJobAsync(mainJob);
            _logger.Info($"This [{mainJob.Type}] job [{mainJob.Id}] is set to [{RMDiscoveryJobStatus.Running}] status, SiteCollectionCount:[{mainJob.SitesCount}].");
        }

        private async Task<bool> TriggerJobsAsync(List<(Guid o365TenantId, RMRemoteNode container, List<RMRemoteNode> sites)> items, RMDiscoveryOffice365MainJob mainJob)
        {
            var mainJobId = mainJob.Id;
            var isSupportExcludeSiteFeature = mainJob.Version == RMDiscoveryJobVersion.V4 || mainJob.Version == RMDiscoveryJobVersion.V5;
            try
            {
                foreach (var (o365TenantId, container, sites) in items)
                {
                    var contentSource = GetContainerContentSource(container);

                    var triggerTime = DateTime.UtcNow.Ticks;
                    var needTriggerSites = sites.Where(item => item.FailedCause != RMDiscoveryJobFailedCause.AnalysisFailed && item.FailedCause != RMDiscoveryJobFailedCause.SiteNotFound).ToList();
                    IEnumerable<RMRemoteNode> runnerSites = Enumerable.Empty<RMRemoteNode>();
                    IEnumerable<RMRemoteNode> excludeSites = Enumerable.Empty<RMRemoteNode>();
                    if (isSupportExcludeSiteFeature)
                    {
                        _logger.Info($"Start filter runnable and excluded sites.");
                        (needTriggerSites, runnerSites, excludeSites) = FilterRunnableAndExcludedSites(sites, needTriggerSites, runnerSites, excludeSites);
                    }
                    var discoveryJobRealId = Guid.Empty;
                    if (needTriggerSites.Count > 0)
                    {
                        var discoveryJobInfo = await _ieApiClient.JobService.TriggerAsync(new SharePointJobInitionModel
                        {
                            Type = contentSource == SourceFlag.SharePoint ? DataType.SPDocument : DataType.SPOneDriveDocument,
                            SiteInfos = needTriggerSites.ConvertAll(item => new SiteInfoModel(item.ObjectId, item.Url)),
                            AzureTenantId = o365TenantId.ToString(),
                            EnforceTagRuleCheck = true
                        });
                        discoveryJobRealId = discoveryJobInfo.Id;
                        _logger.Info($"Successful trigger job [{discoveryJobInfo.Id}] in ie.");
                    }

                    var discoveryJobId = Guid.NewGuid();
                    await _jobDao.AddOrUpdateDiscoveryJobAsync(new RMDiscoveryOffice365DiscoveryJob
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
                    });
                    _logger.Info($"Successful add discovery job [{discoveryJobId} - {discoveryJobRealId}] to opus db.");
                    List<RMDiscoveryOffice365AnalysisJob> analysisJobs = [];
                    if (isSupportExcludeSiteFeature)
                    {
                        analysisJobs.AddRange(runnerSites.ConvertAll(item => new RMDiscoveryOffice365AnalysisJob
                        {
                            Id = Guid.NewGuid(),
                            MainJobId = mainJobId,
                            DiscoveryJobId = discoveryJobId,
                            O365TenantId = o365TenantId,
                            ContainerId = new Guid(container.Id),
                            SiteId = new Guid(item.ObjectId),
                            Url = item.Url,
                            Status = item.FailedCause == RMDiscoveryJobFailedCause.AnalysisFailed || item.FailedCause == RMDiscoveryJobFailedCause.SiteNotFound ? RMDiscoveryJobStatus.Pending : RMDiscoveryJobStatus.Preparing,
                            StartTime = DateTime.UtcNow.Ticks,
                            FailedCause = item.FailedCause,
                        }));
                        analysisJobs.AddRange(excludeSites.ConvertAll(item => new RMDiscoveryOffice365AnalysisJob
                        {
                            Id = Guid.NewGuid(),
                            MainJobId = mainJobId,
                            DiscoveryJobId = discoveryJobId,
                            O365TenantId = o365TenantId,
                            ContainerId = new Guid(container.Id),
                            SiteId = new Guid(item.ObjectId),
                            Url = item.Url,
                            Status = RMDiscoveryJobStatus.Skipped,
                            StartTime = DateTime.UtcNow.Ticks,
                            FailedCause = RMDiscoveryJobFailedCause.SkippedExcludedSite,
                        }));
                    }
                    else
                    {
                        analysisJobs = sites.ConvertAll(item => new RMDiscoveryOffice365AnalysisJob
                        {
                            Id = Guid.NewGuid(),
                            MainJobId = mainJobId,
                            DiscoveryJobId = discoveryJobId,
                            O365TenantId = o365TenantId,
                            ContainerId = new Guid(container.Id),
                            SiteId = new Guid(item.ObjectId),
                            Url = item.Url,
                            Status = item.FailedCause == RMDiscoveryJobFailedCause.AnalysisFailed || item.FailedCause == RMDiscoveryJobFailedCause.SiteNotFound ? RMDiscoveryJobStatus.Pending : RMDiscoveryJobStatus.Preparing,
                            StartTime = DateTime.UtcNow.Ticks,
                            FailedCause = item.FailedCause,
                        });
                    }

                    await _jobDao.BatchInsertAnalysisJobAsync(analysisJobs);
                    _logger.Info($"Successful add discovery analysis jobs [{analysisJobs.Count}] to opus db.");

                    if (mainJob.Version == RMDiscoveryJobVersion.V1)
                    {
                        await TriggerOpusJobs(mainJobId, discoveryJobId, analysisJobs);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while trigger jobs. Error: {e}");
                return false;
            }
        }

        private (List<RMRemoteNode> needTriggerSites, IEnumerable<RMRemoteNode> runnerSites, IEnumerable<RMRemoteNode> excludeSites) FilterRunnableAndExcludedSites(List<RMRemoteNode> sites, List<RMRemoteNode> needTriggerSites, IEnumerable<RMRemoteNode> runnerSites, IEnumerable<RMRemoteNode> excludeSites)
        {
            try
            {
                var (runnerSiteUrls, excludeSiteUrls) = _specificSiteService.GetRunnableAndExcludedM365Sites(needTriggerSites.Select(item => item.Url));
                var triggerSites = needTriggerSites.Where(item => runnerSiteUrls.Contains(item.Url)).ToList();
                runnerSites = sites.Where(item => runnerSiteUrls.Contains(item.Url));
                excludeSites = sites.Where(item => excludeSiteUrls.Contains(item.Url));
                return (triggerSites, runnerSites, excludeSites);
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while filter runnable and excluded sites. Error: {e}");
                return (needTriggerSites, needTriggerSites, Enumerable.Empty<RMRemoteNode>());
            }
        }

        private async Task TriggerOpusJobs(Guid mainJobId, Guid discoveryJobId, List<RMDiscoveryOffice365AnalysisJob> analysisJobs)
        {
            var opusMainJobId = await _jobMonitorService.CreateDiscoveryJobAsync("RM_TS_RunSchedule", mainJobId, discoveryJobId);
            _logger.Info($"Successful create main job [{opusMainJobId}] in opus.");

            var willCreateSubJobs = new List<RMSubJob>();
            for (var i = 0; i < analysisJobs.Count; i++)
            {
                string subJobId = string.Format(opusMainJobId + "_{0:D3}", i);
                var subJob = new RMSubJob
                {
                    Id = subJobId,
                    ParentId = opusMainJobId,
                    StartTime = DateTime.UtcNow.Ticks,
                    JobType = (int)JobType.DiscoveryJob,
                    Progress = 0,
                    Status = (int)Contract.RMWeb.JobMonitor.JobStatus.Wait,
                    Weight = 100d / analysisJobs.Count,
                    DiscoveryAnalysisJobId = analysisJobs[i].Id,
                    Runable = RecordsConstants.SubJob_Runnable_Waiting
                };
                willCreateSubJobs.Add(subJob);
            }

            var effectCount = _subJobDao.BatchCreate(willCreateSubJobs);
            _logger.Info($"Successful create sub jobs [{effectCount}] in main job [{opusMainJobId}].");
        }

        private async Task<bool> RegisterTagsAsync(RMDiscoveryOffice365MainJob mainJob)
        {
            try
            {
                var enabledRules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.Inactive, RMDiscoveryRuleDefinitionKind.ROT);
                foreach (var contentSource in new List<SourceFlag> { SourceFlag.SharePoint, SourceFlag.OneDrive })
                {
                    var customerTagRules = await GetCustomerTagRulesAsync(contentSource, enabledRules);
                    var buildInTagRules = await GetBuildInTagRulesAsync(contentSource);
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

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while register tags. Error: {e}");
                return false;
            }
        }

        private async Task<List<TagRuleModel>> GetCustomerTagRulesAsync(SourceFlag contentSource, List<RMDiscoveryOffice365RuleInfo> ruleInfoes)
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
                            ExtraValue = criteriaInfo.ConditionInfo.ExtraValue,
                            Category = ConvertCategory(criteriaInfo.ConditionInfo.Category),
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

        private async Task<List<TagRuleModel>> GetBuildInTagRulesAsync(SourceFlag contentSource)
        {
            var res = new List<TagRuleModel>();

            var sizeRanges = await _sizeRangeDao.GetAllAsync();
            var withoutInDateList = await _withoutInDateDao.GetAllAsync();
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

        private async Task<List<TagRuleModel>> GetRotTagRulesAsync(RMDiscoveryOffice365MainJob mainJob, SourceFlag contentSource, List<RMDiscoveryOffice365RuleInfo> ruleInfoes)
        {
            var gls = await _generalSettingService.GetGeneralSettingAsync();
            var res = new List<TagRuleModel>();

            if (!mainJob.Version.IsOffice365NewVersion())
            {
                return res;
            }

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

        private RuleInfo ConvertToIERuleInfo(RMDiscoveryOffice365RuleInfo discoveryRuleInfo, GeneralSettingModel gls)
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
                        ExtraValue = criteriaInfo.ConditionInfo.ExtraValue,
                        Category = ConvertCategory(criteriaInfo.ConditionInfo.Category),
                        Logic = criteriaInfo.ConditionInfo.Logic,
                        Value = (criteriaInfo.ConditionInfo.Category == RMDiscoveryConditionCategory.DateTime || criteriaInfo.ConditionInfo.Category == RMDiscoveryConditionCategory.DateTimeExtraInput) && criteriaInfo.ConditionInfo.Logic == (int)RMDiscoveryDateTimeConditionType.Before ?
                        _generalSettingService.ConvertToUTCDateTime(criteriaInfo.ConditionInfo.Value, gls, "yyyy/MM/dd HH:mm") : criteriaInfo.ConditionInfo.Value,
                    }
                })
            };
        }

        private ConditionCategory ConvertCategory(RMDiscoveryConditionCategory category) => category switch
        {
            RMDiscoveryConditionCategory.Text or RMDiscoveryConditionCategory.TextExtraInput => ConditionCategory.Text,
            RMDiscoveryConditionCategory.Number or RMDiscoveryConditionCategory.NumberExtraInput => ConditionCategory.Number,
            RMDiscoveryConditionCategory.Date => ConditionCategory.Date,
            RMDiscoveryConditionCategory.DateTime or RMDiscoveryConditionCategory.DateTimeExtraInput => ConditionCategory.DateTime,
            RMDiscoveryConditionCategory.Array => ConditionCategory.Array,
            RMDiscoveryConditionCategory.BooleanLogic => ConditionCategory.BooleanLogic,
            RMDiscoveryConditionCategory.FileSize => ConditionCategory.FileSize,
            RMDiscoveryConditionCategory.Duplicate => ConditionCategory.Duplicate,
            RMDiscoveryConditionCategory.Version => ConditionCategory.Version,
            RMDiscoveryConditionCategory.BooleanExtraInput => ConditionCategory.Boolean,
            RMDiscoveryConditionCategory.None or _ => ConditionCategory.None,
        };

        private async Task SetJobToFailedAsync(RMDiscoveryOffice365MainJob mainJob)
        {
            _logger.Info($"Set job [{mainJob.Id}] to failed status due to failed tags registration");
            mainJob.Status = RMDiscoveryJobStatus.Failed;
            mainJob.EndTime = DateTime.UtcNow.Ticks;
            if(mainJob.Version.IsOffice365NewVersion())
            {
                mainJob.ProfileJobInitStatus = RMDiscoveryJobStatus.Finished;
            }

            await _jobDao.AddOrUpdateMainJobAsync(mainJob);
            await RMDiscoveryOffice365LicenseHelper.DecreaseConsumedFrequencyPreMonthAsync();
            await _executionInfoDao.DeleteByMainJobIdAsync(mainJob.Id);
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
