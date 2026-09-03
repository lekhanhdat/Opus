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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Google;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Discovery.Model.Rule.Condition;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.Google;
using AvePoint.RA.Service.Services.Discovery.Google.License;
using Cloud.Sdk.Data.IE;
using DataOrchestration.Tag.Sdk.Service.CloudRecords.Contract;
using Newtonsoft.Json;

namespace AvePoint.RA.Service.Services.Discovery.Google.Work.Trigger
{
    public class RMDiscoveryGoogleJobTrigger : RMDiscoveryGoogleWorker
    {
        private readonly IJobMonitorService _jobMonitorService;

        private readonly IRMSubJobDao _subJobDao;

        private readonly IRMDiscoveryGoogleSizeRangeDao _sizeRangeDao;

        private readonly IRMDiscoveryGoogleWithoutInDateDao _withoutInDateDao;

        private readonly IGeneralSettingService _generalSettingService;

        public RMDiscoveryGoogleJobTrigger() : base()
        {
            _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();
            _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
            _sizeRangeDao = new RMDiscoveryGoogleSizeRangeDao();
            _withoutInDateDao = new RMDiscoveryGoogleWithoutInDateDao();
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
                await _jobDao.AddOrUpdateMainJobAsync(mainJob);

                _logger.Info($"The [{mainJob.Type}] job [{mainJob.Id}] is set to [{RMDiscoveryJobStatus.Pending}] status.");

                await HandleGoogleJob(mainJob);
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while trigger discovery job. Error: {e}");
            }
        }

        private async Task<bool> TriggerJobsAsync(List<(string googleOrganizationId, RMRemoteNode container, List<RMRemoteNode> drives)> items, RMDiscoveryGoogleMainJob mainJob)
        {
            var mainJobId = mainJob.Id;
            try
            {
                foreach (var (googleOrganizationId, container, drives) in items)
                {

                    var triggerTime = DateTime.UtcNow.Ticks;
                    var needTriggerDrives = drives.Where(item => item.FailedCause != RMDiscoveryJobFailedCause.AnalysisFailed).ToList();
                    var discoveryJobRealId = Guid.Empty;
                    var googleAppProfileId = RMAosApiClient.GetGoogleAppProfile(TenantLocalValue.LogonGroupId, googleOrganizationId).AOSAppId;
                    if (needTriggerDrives.Count > 0)
                    {
                        var discoveryJobInfo = await _ieApiClient.JobService.TriggerAsync(new GoogleDriveDocumentJobInitionModel
                        {
                            Type = DataType.GoogleDriveDocument,
                            GoogleDriveDocumentItems = needTriggerDrives.ConvertAll(item => new GoogleItemModel() { ObjectId = item.ObjectId, ItemType = GetGoogleItemType(item.NodeLevel), PrincipalName = item.Url }),
                            AzureTenantId = googleOrganizationId,
                            EnforceTagRuleCheck = true,
                            GoogleAppProfileId = googleAppProfileId
                        });
                        discoveryJobRealId = discoveryJobInfo.Id;
                        _logger.Info($"Successful trigger job [{discoveryJobInfo.Id}] in ie.");
                    }

                    var discoveryJobId = Guid.NewGuid();
                    await _jobDao.AddOrUpdateDiscoveryJobAsync(new RMDiscoveryGoogleDiscoveryJob
                    {
                        Id = discoveryJobId,
                        RealId = discoveryJobRealId,
                        MainJobId = mainJobId,
                        OrganizationId = googleOrganizationId.ToString(),
                        ContainerId = new Guid(container.Id),
                        ContainerName = container.Url,
                        DrivesCount = drives.Count,
                        Status = needTriggerDrives.Count > 0 ? RMDiscoveryJobStatus.Pending : RMDiscoveryJobStatus.Completing,
                        StartTime = DateTime.UtcNow.Ticks,
                        LastCheckedTime = triggerTime,
                    });
                    _logger.Info($"Successful add discovery job [{discoveryJobId} - {discoveryJobRealId}] to opus db.");

                    var analysisJobs = drives.ConvertAll(item => new RMDiscoveryGoogleAnalysisJob
                    {
                        Id = Guid.NewGuid(),
                        DriveName = item.Url ?? "",
                        DriveType = GetGoogleDriveType(item.NodeLevel),
                        MainJobId = mainJobId,
                        DiscoveryJobId = discoveryJobId,
                        OrganizationId = googleOrganizationId.ToString(),
                        ContainerId = new Guid(container.Id),
                        DriveId = item.ObjectId,
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

        private async Task HandleGoogleJob(RMDiscoveryGoogleMainJob mainJob)
        {
            if (!await RegisterTagsAsync(mainJob))
            {
                await SetJobToFailedAsync(mainJob);
                return;
            }

            IRMDiscoveryGoogleJobTriggerible trigger = mainJob.Type switch
            {
                RMDiscoveryJobType.Newly => new RMDiscoveryGoogleJobNewlyTrigger(mainJob),
                _ => throw new NotSupportedException(mainJob.Type.ToString()),
            };

            var (succeed, items) = await trigger.GetWillTriggerJobsWrapperAsync();
            if (!succeed)
            {
                await SetJobToFailedAsync(mainJob);
                return;
            }

            var initTableSucceed = await trigger.InitTablesAsync(items.Select(item => item.googleOrganizationId).ToHashSet().ToList());
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

            mainJob.Status = RMDiscoveryJobStatus.Running;
            mainJob.DrivesCount = items.Sum(item => item.drives.Count);
            await _jobDao.AddOrUpdateMainJobAsync(mainJob);
            _logger.Info($"This [{mainJob.Type}] job [{mainJob.Id}] is set to [{RMDiscoveryJobStatus.Running}] status, DriveCount:[{mainJob.DrivesCount}].");
        }

        private async Task<bool> RegisterTagsAsync(RMDiscoveryGoogleMainJob mainJob)
        {
            try
            {
                var enabledRules = await _ruleInfoDao.GetRuleInfoesAsync(true, RMDiscoveryRuleDefinitionKind.ROT);
                foreach (var contentSource in new List<SourceFlag> { SourceFlag.Google })
                {
                    var customerTagRules = await GetCustomerTagRulesAsync(enabledRules);
                    var buildInTagRules = await GetBuildInTagRulesAsync();
                    var rotTagRules = await GetRotTagRulesAsync(mainJob, enabledRules);
                    var localTagRules = customerTagRules.Concat(buildInTagRules).Concat(rotTagRules).ToList();
                    var localTagRuleIds = localTagRules.Select(item => item.Id).ToHashSet();

                    var aosTagRules = await _ieApiClient.TagRuleService.GetAllAsync(DataType.GoogleDriveDocument, Cloud.Sdk.Data.Core.CallerType.CloudRecords);
                    var aosTagRuleIds = aosTagRules.Select(item => item.Id).ToHashSet();

                    var needDeleteTagIds = aosTagRuleIds.Except(localTagRuleIds).ToList();
                    await _ieApiClient.TagRuleService.DeleteBatchAsync(needDeleteTagIds.ConvertAll(item => new TagRuleDeletionModel { Id = item, DataType = DataType.GoogleDriveDocument }));
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

        private async Task<List<TagRuleModel>> GetCustomerTagRulesAsync(List<RMDiscoveryGoogleRuleInfo> ruleInfoes)
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
                    Type = DataType.GoogleDriveDocument,
                    NeedCalculation = true
                };
            });

            return tagRuleModels;
        }

        private async Task<List<TagRuleModel>> GetBuildInTagRulesAsync()
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
                Type = DataType.GoogleDriveDocument,
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
                Type = DataType.GoogleDriveDocument,
                NeedCalculation = true
            });
            res.Add(new TagRuleModel
            {
                Id = RMDiscoveryBuildInRule.ARCHVIED_UNIQUE_ID,
                Name = "is_archived",
                Product = Cloud.Sdk.Data.Core.CallerType.CloudRecords,
                Type = DataType.GoogleDriveDocument,
                NeedCalculation = false
            });
            res.Add(new TagRuleModel
            {
                Id = RMDiscoveryBuildInRule.DUPLICATE_UNIQUE_ID,
                Name = "is_duplicate",
                Product = Cloud.Sdk.Data.Core.CallerType.CloudRecords,
                Type = DataType.GoogleDriveDocument,
                NeedCalculation = false
            });

            return res;
        }

        private async Task<List<TagRuleModel>> GetRotTagRulesAsync(RMDiscoveryGoogleMainJob mainJob, List<RMDiscoveryGoogleRuleInfo> ruleInfoes)
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
                    Type = DataType.GoogleDriveDocument,
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
                    Type = DataType.GoogleDriveDocument,
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
                    Type = DataType.GoogleDriveDocument,
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
                    Type = DataType.GoogleDriveDocument,
                    NeedCalculation = true,
                });
            }

            return res;
        }

        private RuleInfo ConvertToIERuleInfo(RMDiscoveryGoogleRuleInfo discoveryRuleInfo, GeneralSettingModel gls)
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

        private async Task SetJobToFailedAsync(RMDiscoveryGoogleMainJob mainJob)
        {
            _logger.Info($"Set job [{mainJob.Id}] to failed status due to failed tags registration");
            mainJob.Status = RMDiscoveryJobStatus.Failed;
            mainJob.EndTime = DateTime.UtcNow.Ticks;
            mainJob.ProfileJobInitStatus = RMDiscoveryJobStatus.Finished;
            await _jobDao.AddOrUpdateMainJobAsync(mainJob);
            await RMDiscoveryGoogleLicenseHelper.DecreaseConsumedFrequencyPerYearAsync();
        }

        private GoogleItemType GetGoogleItemType(int nodeLevel)
        {
            switch (nodeLevel)
            {
                case (int)NodeLevel.GoogleMyDrive:
                    return GoogleItemType.User;
                case (int)NodeLevel.GoogleSharedDrive:
                    return GoogleItemType.SharedDrive;
                default: return GoogleItemType.None;
            }
        }

        private RMDiscoveryGoogleDriveType GetGoogleDriveType(int nodeLevel)
        {
            switch (nodeLevel)
            {
                case (int)NodeLevel.GoogleMyDrive:
                    return RMDiscoveryGoogleDriveType.MyDrive;
                case (int)NodeLevel.GoogleSharedDrive:
                    return RMDiscoveryGoogleDriveType.SharedDrive;
                default: return RMDiscoveryGoogleDriveType.None;
            }
        }
    }
}
