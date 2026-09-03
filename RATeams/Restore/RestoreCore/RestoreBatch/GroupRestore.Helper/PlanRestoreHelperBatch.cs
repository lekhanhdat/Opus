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

namespace Office365GroupRestore
{
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
    using AvePoint.GCommon.GraphAPI;
    using AvePoint.Metadata;
    using ExchangeCommonWrapper;
    using ExchangeUtility.Graph;
    using Job.ModernManagement.Report;
    using Office365GroupBackup;
    using RAArchiverCommon;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PlanRestoreHelperBatch : BaseRestoreHelperBatch
    {
        public PlanRestoreHelperBatch(BaseRestoreHelperBatch baseHelper):base(baseHelper)
        {
           
        }


        private Office365PlanEntity planProperties;
        private string newPlanId = string.Empty;
        private Office365PlanBasicProperties tempPlan;
        protected override void InitReport(MetadataEntity baseEntity, String sourceUrlDic)
        {
            base.InitReport(baseEntity, sourceUrlDic);
            ReportDto.Type = ReportNodeHeader.Plan;
        }

        protected override bool NeedRestore() => !string.IsNullOrEmpty(RestoreConfig.CurrentRestoreMailbox);

        protected override void RealRestore(IEnumerable<ExchangeDataBlockForBatch> dataCollection)
        {
            // Make sure that migration teams/channels are complete before restoring the following parts
            TryCompleteMigration();
            EntityIdDic = new Dictionary<string, string>();
            BucketIdDic = new Dictionary<string, string>();
            _NeedUpdatePlanBuckets = new Dictionary<string, Office365PlannerBucketProperties>();
            _AllTasks = new Dictionary<string, string>();
            _UnmatchBuckets = new List<Bucket>();
            var restoreData = dataCollection.First().RestoreData;
            var entity = restoreData.Metadata;
            var sourceUrlPath = restoreData.SourceUrlPath;

            try
            {
                GetPlanProperties(restoreData);

                this.InitReport(entity, sourceUrlPath);

                logger.Info($"Start to restore {ReportDto.Type}, name:{ReportDto.Name}, path: {ReportDto.Path}, id:{entity.Id}");

                if (null == PlannerService) { throw new Exception("Unsupport AuthType"); }
                if (_SiteNotFound) throw new Exception("Agent.Teams.SiteNotFound_152A5656-8624-4179-86C7-8684C2B1B5F0");
                var isNewPlan = RestorePlan(entity);

                GetNeedCreateBuckets(planProperties.BucketProperties, newPlanId);

                GetAllTaskIdAndName(newPlanId);

                AddOptionReport(isNewPlan, entity.Title);

                logger.Info("[PLAN]:Restore plan finished.");

                if (this.Config.IsMicrosoftTeams) UpdatePlannerTabConfig(entity.Title);
            }
            catch (GraphAPIException ex) when (ex.Error.Message.Contains("do not have the required permissions") && PlannerService is ExchangePlannerAppService)
            {
                var plannerService = PlannerService as ExchangePlannerAppService;
                ReportDto.Status = ReportStatus.Failed;
                if (plannerService.IsCustomApp)
                    ReportDto.ErrorMessage = "Agent.Planner.CustomAppInsufficientPermission_89F6D5AB-DCC2-C6C7-C98F-945B1F04E4CF";
                else
                    ReportDto.ErrorMessage = "Agent.Teams.Reauthorize_C0ACEB56-E16D-4D64-A19A-FB14B951EB10";
            }
            catch (GraphAPIException ex)
            {
                logger.Info($"Failed to restore {ReportDto.Type}, name:{ReportDto.Name}, error:{ex}");
                ReportDto.Status = ReportStatus.Failed;
                ReportDto.ErrorMessage = ErrorCodeConverter.GraphAPIErrorCodeConverter(ex, I18NDataCollector);
            }
            catch (Exception ex)
            {
                logger.Info($"Failed to restore {ReportDto.Type}, name:{ReportDto.Name}, error:{ex}");
                if (ex.Message.StartsWith("Unsupport AuthType"))
                {
                    logger.Error("Can not use AppToken to restore Planner data.{0}", ex);
                    ReportDto.Status = ReportStatus.Skipped;
                    ReportDto.ErrorMessage = "Agent.Office365Group.RestorePlannerFailedWithAppToken_009DDBA9-4786-4B36-87DF-C6D52E937E45";
                }
                else
                {
                    ReportDto.ErrorMessage = ex.Message;
                    ReportDto.Status = ReportStatus.Failed;
                    logger.Error("An error occurred while restore planner plan {0}. Message {1}. ", entity.Title, ex.ToString());
                }
            }
            finally
            {
                Report.AddRestoreReport(ReportDto);
                SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(ReportDto.Size, ReportDto.SourcePath);
            }
        }

        private void GetPlanProperties(ExchangeRestoreDataForBatch restoreData)
        {
            planProperties = restoreData.TryGetMetadata<Office365PlanEntity>(AveMetadataType.ExchangePlannerPlan);
            if (planProperties.DetailsProperties.CategoryDescriptionsDictionary == null)
            {
                planProperties.DetailsProperties.CategoryDescriptionsDictionary = new Dictionary<string, string>()
                {
                    { "Category1", planProperties.DetailsProperties.CategoryDescriptions?.Category1 },
                    { "Category2", planProperties.DetailsProperties.CategoryDescriptions?.Category2 },
                    { "Category3", planProperties.DetailsProperties.CategoryDescriptions?.Category3 },
                    { "Category4", planProperties.DetailsProperties.CategoryDescriptions?.Category4 },
                    { "Category5", planProperties.DetailsProperties.CategoryDescriptions?.Category5 },
                    { "Category6", planProperties.DetailsProperties.CategoryDescriptions?.Category6 },
                };
            }
        }

        private void UpdatePlannerTabConfig(string plannerTitle)
        {
            try
            {
                logger.Info("Start to update planner tab. Planner: {0}.", plannerTitle);
                if (!EntityIdDic.First().Key.Equals(EntityIdDic.First().Value, StringComparison.OrdinalIgnoreCase))
                {
                    var needUpDateTabs = _PlannerTabs.FindAll(pT => pT.PlannerId.Contains(EntityIdDic.First().Key, StringComparison.OrdinalIgnoreCase));
                    foreach (var needUpdateTab in needUpDateTabs)
                    {
                        logger.Info("Update tab info: ChannelId: {0}. TabName: {1}. ", needUpdateTab.ChannelId, needUpdateTab.ChannelTab.DisplayName);
                        TeamsService.UpdateChannelTabConfig(_GroupId, needUpdateTab.ChannelId, needUpdateTab.TabId, TabFactory.CreateTabConfig(needUpdateTab.ChannelTab, EntityIdDic, RestoreConfig.TenantIdMap));
                    }
                }
                logger.Info("Success to update planner tab.");
            }
            catch (Exception ex)
            {
                logger.Info("An error occurred to update planner tab. Planner: {0}.Reason: {1}.", plannerTitle, ex.ToString());
            }
        }

        public Boolean RestorePlan(MetadataEntity entity)
        {
            _GroupId = PlannerService.GetGroupIdByAddress(RestoreConfig.CurrentRestoreMailbox);
            logger.Info("[PLAN]:GroupId: {0}", _GroupId);
            var hasCreatedPlan = CreatePlan(out NewIdDto planDto);
            if (NeedUpdatePlan(entity.Title)) PlannerService.UpdatePlannerPlan(planDto, entity.Title);
            RetryLogic(() => { UpdatePlanDetails(hasCreatedPlan); });
            return hasCreatedPlan;
        }
        private void RetryLogic(Action action)
        {
            var retryTimes = 0;
            do
            {
                try
                {
                    action();
                    return;
                }
                catch (GraphAPIException ex)
                {
                    //由于新创建group添加member后非立即可用
                    //if (ex.Message.Contains( "You do not have the required permissions to access this item")) throw;
                    if (ErrorCodeConverter.GetErrorCode(ex.Error.Code) == ServiceError.MaximumPlannerPlans) throw;
                    if (retryTimes++ < 5)
                    {
                        System.Threading.Thread.Sleep(5000);
                        logger.Warn($"Error occurred in RestorePlan,start retry : {retryTimes} times.");
                        continue;
                    }
                    throw;
                }
            }
            while (retryTimes <= 5);
        }
        private Boolean CreatePlan(out NewIdDto planDto)
        {
            bool needCreatePlan;
            var basicProperties = planProperties.BasicProperties;
            try
            {
                needCreatePlan = NeedCreatePlan(basicProperties, out planDto);
            }
            catch
            {
                needCreatePlan = true;
                planDto = new NewIdDto();
            }
            newPlanId = planDto.NewId;
            if (needCreatePlan)
            {
                RetryLogic(() => { newPlanId = PlannerService.CreatePlannerPlan(basicProperties, _GroupId); });
                _NewlyPlannerPlanIds.Add(newPlanId);
            }
            EntityIdDic.Add(basicProperties.Id, newPlanId);
            logger.Info("[PLAN]:NeedCreatePlan: {0}, NewPlanId: {1}", needCreatePlan, newPlanId);
            return needCreatePlan;
        }

        private Boolean UpdatePlanDetails(bool isNewPlan)
        {
            NewIdDto newPlanDetailsDto;
            var needUpdateDetails = NeedUpdatePlanDetails(isNewPlan, out newPlanDetailsDto, newPlanId, planProperties.DetailsProperties.OdataEtag);
            ToUpdatePlanDetails(planProperties.DetailsProperties, newPlanDetailsDto, needUpdateDetails);
            logger.Info("[PLAN]:NeedUpdateDetails: {0}, {1}", needUpdateDetails, newPlanDetailsDto.ToString());
            return needUpdateDetails;
        }

        private void ToUpdatePlanDetails(Office365PlanDetailsProperties planDetailsEntity, NewIdDto newPlanDetailsDto, bool needUpdateDetails)
        {
            if (needUpdateDetails) PlannerService.UpdatePlanDetails(planDetailsEntity, newPlanDetailsDto);
        }

        #region bucket
        /// <summary>
        /// 此方法获取的是通过id查找，实际不存在的bucket，并且将这部分中通过名字查找存在的bucket进行标记
        /// </summary>
        /// <param name="planBuckets"></param>
        /// <param name="newPlanId"></param>
        private void GetNeedCreateBuckets(List<Office365PlannerBucketProperties> planBuckets, string newPlanId)
        {
            if (!planBuckets?.Any() ?? true) return;
            ChangeBucketsOrderHints(planBuckets);

            var existPlanBuckets = PlannerService.ListAllBucketsByPlanID(newPlanId);
            planBuckets.ForEach(bucket =>
            {
                Office365PlannerBucketProperties matchBucket;
                if (!TryGetIdMatchBucket(existPlanBuckets, bucket.Id, out matchBucket))
                {
                    if (!TryGetNameMatchBucket(existPlanBuckets, bucket.Name, out matchBucket))
                    {
                        if (RestoreConfig.EntirePlannerPlan)
                        {
                            CreateBucketInAdvance(bucket);
                        }
                        else
                        {
                            RecordUnmatchBucket(bucket);
                        }
                    }
                    else
                    {
                        RecordSameNameBucket(bucket, matchBucket);
                    }
                }
                else
                {
                    RecordNeedUpdatedBucket(bucket, matchBucket);
                    //如果 id 能匹配上的 bucket 需要 update name，可能会对其他同名 bucket 还原结果造成影响。
                    //如果找到了 id 能匹配上的 bucket，其他同名 bucket 就不应该再与这个 bucket 匹配。
                    existPlanBuckets.Remove(matchBucket);
                }
            });
        }
        private Boolean TryGetIdMatchBucket(List<Office365PlannerBucketProperties> existPlanBuckets, String bucketId, out Office365PlannerBucketProperties matchBucket)
        {
            matchBucket = existPlanBuckets.FindLast(newBucket => newBucket.Id == bucketId);
            return null != matchBucket;
        }
        private Boolean TryGetNameMatchBucket(List<Office365PlannerBucketProperties> existPlanBuckets, String bucketName, out Office365PlannerBucketProperties matchBucket)
        {
            matchBucket = existPlanBuckets.FindLast(newBucket => newBucket.Name == bucketName);
            return null != matchBucket;
        }
        private void RecordUnmatchBucket(Office365PlannerBucketProperties bucket)
        {
            _UnmatchBuckets.Add(new Bucket()
            {
                OId = bucket.Id,
                Name = bucket.Name,
                OrderHint = bucket.OrderHint,
            });
        }
        private void RecordSameNameBucket(Office365PlannerBucketProperties bucket, Office365PlannerBucketProperties matchBucket)
        {
            _UnmatchBuckets.Add(new Bucket()
            {
                OId = bucket.Id,
                Name = bucket.Name,
                OrderHint = bucket.OrderHint,
                NId = matchBucket.Id,
                CanGetByName = true
            });
        }
        private void RecordNeedUpdatedBucket(Office365PlannerBucketProperties bucket, Office365PlannerBucketProperties matchBucket)
        {
            if (bucket.Name != matchBucket.Name)
            {
                matchBucket.Name = bucket.Name;
                _NeedUpdatePlanBuckets.TryAdd(bucket.Id, matchBucket);
            }
        }
        private void CreateBucketInAdvance(Office365PlannerBucketProperties bucket)
        {
            try
            {
                var newBucketId = PlannerService.CreatePlannerBucket(new Office365PlannerTaskBucketProperties { Name = bucket.Name }, newPlanId, bucket.OrderHint);
                BucketIdDic.TryAdd(bucket.Id, newBucketId);
            }
            catch (Exception ex)
            {
                logger.Warn("Create bucket [{0}] in advance failed, Reason :{1}", bucket.Name, ex.ToString());
            }
        }

        /// <summary>
        /// 调整planner bucket排序属性
        /// </summary>
        /// <param name="planBuckets"></param>
        private void ChangeBucketsOrderHints(List<Office365PlannerBucketProperties> planBuckets)
        {
            var orderHintList = planBuckets.Select(bucket => bucket.OrderHint).ToList();
            var tempDic = OrderHintsSort.ToSimpleSortDictionary(orderHintList);
            planBuckets.ForEach(bucket => bucket.OrderHint = tempDic[bucket.OrderHint]);
        }
        #endregion

        private void GetAllTaskIdAndName(string newPlanId)
        {
            _AllTasks = PlannerService.ListAllTaskByPlanID(newPlanId).ToDictionary(task => task.Id, task => task.Title);
        }
        private Boolean NeedCreatePlan(Office365PlanBasicProperties basicProperties, out NewIdDto planDto)
        {
            var allPlans = PlannerService.ListAllPlansByGroupID(_GroupId).Where(plan => !_NewlyPlannerPlanIds.Contains(plan.Id)).ToList();
            tempPlan = allPlans.FirstOrDefault(plan => plan.Id.Equals(basicProperties.Id));
            if (null == tempPlan)
            {
                var plansByTitle = allPlans.FindAll(plan => plan.Title.Equals(basicProperties.Title, StringComparison.OrdinalIgnoreCase));
                if (plansByTitle.Count == 0)
                {
                    logger.Info("Can not find the plan named {0}, A new Plan will be created.", basicProperties.Title);
                    planDto = new NewIdDto();
                    return true;
                }
                if (plansByTitle.Count == 1)
                {
                    logger.Info("We found a plan [{0}] with the same name through Title", basicProperties.Title);
                    planDto = new NewIdDto() { NewId = plansByTitle.First().Id, OdataEtag = plansByTitle.First().OdataEtag };
                    return false;
                }
                if (plansByTitle.Count > 1)
                {
                    //throw new Exception("Agent.Office365Group.MultipleSameNamePlanSkipped_01023F57-FB57-4781-9DCF-1D630BF98221 ");
                    logger.Info("Multiple same name plan have been found, A new Plan will be created.");
                    planDto = new NewIdDto();
                    return true;
                }
            }
            logger.Info("The original plan still exists");
            planDto = new NewIdDto() { NewId = tempPlan!.Id, OdataEtag = tempPlan.OdataEtag };
            return false;
        }
        private Boolean NeedUpdatePlan(String title)
        {
            if (Config.ContentConflictResolution == EOConflictResolutionType.Skip) return false;
            if (null == tempPlan) return false;// 没有 id 匹配的 plan ，不需要更新 title 
            return title != tempPlan.Title;
        }
        private Boolean NeedUpdatePlanDetails(bool isNewPlan, out NewIdDto newPlanDetailsDto, string newPlanId, string palnDetailsEtag)
        {
            newPlanDetailsDto = PlannerService.GetNewPlanDetailsIdByPlanId(newPlanId);
            if (isNewPlan)
            {
                return isNewPlan;
            }
            else
            {
                if (Config.ContentConflictResolution == EOConflictResolutionType.Skip)
                {
                    return isNewPlan;
                }
                else
                {
                    return !palnDetailsEtag.Equals(newPlanDetailsDto.OdataEtag);
                }
            }
        }

        private void AddOptionReport(bool hasCreatedPlan, string title)
        {
            if (!hasCreatedPlan)
            {
                if (Config.ContentConflictResolution == EOConflictResolutionType.Skip)
                {
                    ReportDto.Option = RestoreOption.Skipped.GetEnumDescription();
                    ReportDto.ErrorMessage = "RM_JMD_Teams_RestorePlanSkipped";
                    ReportDto.Status = ReportStatus.Skipped;
                    logger.Info("The Plan {0} is skipped because it already exist in destination.", title);
                }
                else
                {
                    ReportDto.Option = RestoreOption.Overwritten.GetEnumDescription();
                    ReportDto.Status = ReportStatus.Success;
                    logger.Info("The Plan {0} is Overwrited.", title);
                }
            }
        }
    }
}