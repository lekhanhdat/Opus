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
using AngleSharp.Dom;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ControlPlus;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.Service.Services.ManualApproval.Model;
using AvePoint.RA.Service.Services.ManualApproval.Queriers;
using Microsoft.BusinessData.MetadataModel.Collections;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using RACloudFS.Report;
using RAGlobalSearch;
using RAManualApproval;
using RAManualApproval.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAArtificialIntelligence.MachineLearningReview
{
    public class MLReclassifyBulkAction
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(MLReclassifyBulkAction));

        private static MLManualApprovalRecordRepository Repository => new();

        private static readonly Dictionary<ManualApprovalFilterOptions, IFilter> FilterCollection = new();
        private static readonly Dictionary<ManualApprovalOrderOptions, ISorter> SorterCollection = new();
        private static readonly IRMSecurityTrimmingHelper SecurityTrimmingHelper = PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        private static readonly IExplorerService ExplorerService = (IExplorerService)PlatformWindsorManager.GetService(typeof(IExplorerService));
        private static readonly IRMSubJobDao SubJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
        private static readonly IUserService UserService = PlatformWindsorManager.GetService<IUserService>();
        private static readonly IRMKeyValueDao RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private static readonly IRMMLTrainingModelDao TrainingModelDao = PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();
        private readonly int pageSize = 500;
        private string? Continuation { get; set; }
        private RMSubJob RMSubJob { get; set; }
        private string CurrentJobId { get; set; }
        private JobType CurrentJobType { get; set; }

        private ChangeTermDto jobParam = new();
        private int mFailedCount = 0;
        private int mSuccessCount = 0;
        private bool mHasError = false;
        //private readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;
        private readonly IJobInfoUpdater jobInfoUpdater;
        private readonly IRMReportManager reportManager;
        public MLReclassifyBulkAction(string jobId, JobType jobType, string userId)
        {
            TenantLocalValue.LogonUserId = userId;
            CurrentJobType = jobType;
            CurrentJobId = jobId;
            ReportMangerFactory.Instance.Init(CurrentJobId, CurrentJobType);
            reportManager = ReportMangerFactory.Instance.ReportManager;
            reportManager.Increase(1);
            reportManager.StartUpdateJobProgress(60);
            jobInfoUpdater = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));
            jobInfoUpdater.UpdateJobState(CurrentJobId, (int)JobStatus.InProgress);


            RMSubJob = SubJobDao.GetSubJob(jobId, true);

            InitFilterCollection();
            InitSorterCollection();
        }

        public async Task RunAsync()
        {
            try
            {
                logger.Debug($"[{TenantLocalValue.LogonUserId}] run job filter content: [{RMSubJob.JobContext.Content}]");
                jobParam = SerializerHelper.DeserializeByJsonSerializer<ChangeTermDto>(RMSubJob.JobContext.Content);
                ManualApprovalQueryDefinition queryDefinition = new();
                queryDefinition.Filters = jobParam.QueryDefintion;

                TenantLocalValue.RequesterType = jobParam.RequesterType;

                using (new PerformanceScope("MachineLearningReview_SelectAllAction", "GenerateFilter"))
                {
                    queryDefinition.PageSize = pageSize;
                    queryDefinition.NeedCalculationCount = false;
                    queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
                    {
                        FilterOption = ManualApprovalFilterOptions.MLApprovalStatus,
                        Value = JsonConvert.SerializeObject(new List<RMMLApprovalStatus> { RMMLApprovalStatus.WaitingApprove })
                    });
                }

                using (new PerformanceScope($"Excute reclassify action"))
                {
                    do
                    {
                        ManualApprovalResultModel? manualApprovalPaginateResult = null;
                        using (new PerformanceScope("Query 500 data", "Query Datas by query definition", true))
                        {
                            manualApprovalPaginateResult = await QueryDataAsync(queryDefinition);
                        }
                        logger.Debug($"Current batch need process item count: [{manualApprovalPaginateResult.Items.Count}].");
                        Continuation = manualApprovalPaginateResult.Continuation;
                        using (new PerformanceScope("Excute action for query results"))
                        {
                            reportManager.IncreaseBase(manualApprovalPaginateResult.Items.Count);
                            await ExcuteActionAsync(manualApprovalPaginateResult.Items);
                        }
                    }
                    while (!string.IsNullOrEmpty(Continuation));
                }
            }
            catch (Exception e)
            {
                mHasError = true;
                logger.Error($"An error occurred while process job. Error: {e}");
            }
            finally
            {
                UpdateJobState();
            }
        }

        private async Task<ManualApprovalResultModel> QueryDataAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            var repository = Repository;
            await PrePermissionValidateAsync(queryDefinition);
            var filterExpresions = await BuildCosmosDBFilterAsync(queryDefinition);
            var sorterDefinitions = BuildCosmosDBSorter(queryDefinition);
            var explorerQueryDefinition = new ManualApprovalExplorerQueryDefinition
            {
                PageSize = queryDefinition.PageSize,
                Continuation = Continuation,
                Predicates = filterExpresions,
                OrderDefinitions = sorterDefinitions,
            };

            var result = new ManualApprovalResultModel();

            if (queryDefinition.NeedCalculationCount)
            {
                var count = repository.CountAsync(explorerQueryDefinition).GetAwaiter().GetResult();
                result.Count = count;
            }
            logger.Debug($"Query Defintion:{JsonConvert.SerializeObject(queryDefinition)}");
            var explorerQueryResult = repository.QueryItemsWithPaginationAsync(explorerQueryDefinition).GetAwaiter().GetResult();

            result.Continuation = explorerQueryResult.Continuation;
            result.Items = explorerQueryResult.Items;

            return result;
        }

        private async Task ExcuteActionAsync(List<ManualApprovalRecord> items)
        {
            try
            {
                var changeTermInfo = jobParam;
                var changeTermOption = new ChangeTermOption()
                {
                    SourceRecordIds = new(),
                    SourceOneDriveRecordIds = new(),
                    SourceFSRecordIds = new(),
                    SourceEXORecordIds = new(),
                    SourcePhyRecordIds = new(),
                    SourceAzureFileShareRecordIds = new(),
                    SourceCustomizeConnectorRecordIds = new(),
                    Comment = changeTermInfo.Comment,
                };
                if (changeTermInfo.TermInfo != null)
                {
                    changeTermOption.TargetTermId = changeTermInfo.TermInfo.Id;
                    changeTermOption.TargetTermName = changeTermInfo.TermInfo.Name;
                    changeTermOption.TargetTermUniqueId = changeTermInfo.TermInfo.UniqueId;
                }
                ReportMangerFactory.Instance.ReportManager.IncreaseBase(items.Count);
                ChangeTermType changeTermType = CurrentJobType == JobType.MachineLearningReviewReclassify ? ChangeTermType.AIMAChangeTerm : ChangeTermType.AIMADirectlyApprove;
                using (var scope = new PerformanceScope("Process SharePoint Online Items", addToStatistics: true))
                {
                    var spoItems = items.Where(r => r.SourceFlag == (int)SourceFlag.SharePoint).ToList();
                    if (spoItems.Any())
                    {
                        changeTermOption.SourceRecordIds = spoItems.Select(r => r.Id).ToList();
                        var failedCount = await ExplorerService.ChangeTermForAIJobAsync(spoItems.Select(r => r.Id).ToList(), SourceFlag.SharePoint, CurrentJobId, changeTermType, changeTermOption, true);
                        logger.Info("Failed count:" + failedCount);
                        mFailedCount += failedCount;
                        mSuccessCount += (spoItems.Count - failedCount);
                    } 
                }
                using (var scope = new PerformanceScope("Process OneDrive Items", addToStatistics: true))
                {
                    var oneDriveItems = items.Where(r => r.SourceFlag == (int)SourceFlag.OneDrive).ToList();
                    if (oneDriveItems.Any())
                    {
                        changeTermOption.SourceOneDriveRecordIds = oneDriveItems.Select(r => r.Id).ToList();
                        var failedCount = await ExplorerService.ChangeTermForAIJobAsync(oneDriveItems.Select(r => r.Id).ToList(), SourceFlag.OneDrive, CurrentJobId, changeTermType, changeTermOption, true);
                        logger.Info("Failed count:" + failedCount);
                        mFailedCount += failedCount;
                        mSuccessCount += (oneDriveItems.Count - failedCount);
                    } 
                }

                using (var scope = new PerformanceScope("Process Teams Items", addToStatistics: true))
                {
                    var teamsItems = items.Where(r => r.SourceFlag == (int)SourceFlag.Teams).ToList();
                    if (teamsItems.Any())
                    {
                        changeTermOption.SourceTeamsRecordIds = teamsItems.Select(r => r.Id).ToList();
                        var failedCount = await ExplorerService.ChangeTermForAIJobAsync(teamsItems.Select(r => r.Id).ToList(), SourceFlag.Teams, CurrentJobId, changeTermType, changeTermOption, true);
                        logger.Info("Failed count:" + failedCount);
                        mFailedCount += failedCount;
                        mSuccessCount += (teamsItems.Count - failedCount);
                    }
                }

                using (var scope = new PerformanceScope("Process Google Items", addToStatistics: true))
                {
                    var googleItems = items.Where(r => r.SourceFlag == (int)SourceFlag.Google).ToList();
                    if (googleItems.Any())
                    {
                        changeTermOption.GoogleDriveRecordIds = googleItems.Select(r => r.Id).ToList();
                        var failedCount = await ExplorerService.ChangeTermForAIJobAsync(googleItems.Select(r => r.Id).ToList(), SourceFlag.Google, CurrentJobId, changeTermType, changeTermOption, true);
                        logger.Info("Failed count:" + failedCount);
                        mFailedCount += failedCount;
                        mSuccessCount += (googleItems.Count - failedCount);
                    }
                }

                if (RMKeyValueDao.EnableZeroShotFeature() && TrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot && (changeTermType == ChangeTermType.AIMADirectlyApprove || changeTermType == ChangeTermType.AIMAChangeTerm))
                {
                    IExplorerDao explorerDao = new ExplorerDao();
                    var recordIds = items.Select(r => r.Id).ToList();
                    var allRecords = explorerDao.QueryAll(r => recordIds.Contains(r.Id)).ToList();
                    logger.Info($"Get records from db, records: [{string.Join(",", allRecords.Select(r => r.Id))}]");
                    List<Guid> predictTermIds = allRecords.Select(r => r.PredictTermId).Distinct().ToList();
                    ExplorerService.HandleCalculateZeroShotAccuracy(predictTermIds, changeTermType);
                }

                reportManager.Increase();
            }
            catch (Exception e)
            {
                mFailedCount++;
                //ManualApprovalSelectAllJobManager.AddFailedJobDetail(item, ApprovalStatus, reviewerNames, e.Message);
                logger.Error($"An error occurred while process items. Error: {e}");
            }
        }

        private static void InitFilterCollection()
        {
            try
            {
                var filterType = typeof(IFilter);
                var assembly = Assembly.GetAssembly(filterType);

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsInterface) continue;
                    if (type.GetInterfaces().Contains(filterType))
                    {
                        var instance = Activator.CreateInstance(type) as IFilter;
                        FilterCollection.Add(instance.FilterOption, instance);
                    }
                }
                logger.Info($"Succeed init filter collection.");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while init filter collection. Error: {e}");
                throw;
            }
        }

        private static void InitSorterCollection()
        {
            try
            {
                var sorterType = typeof(ISorter);
                var assembly = Assembly.GetAssembly(sorterType);

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsInterface) continue;
                    if (type.GetInterfaces().Contains(sorterType))
                    {
                        var instance = Activator.CreateInstance(type) as ISorter;
                        SorterCollection.Add(instance.OrderOption, instance);
                    }
                }
                logger.Info($"Succeed init sorter collection.");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while init sorter collection. Error: {e}");
                throw;
            }
        }

        private static async Task PrePermissionValidateAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin);
            if (isAdmin)
            {
                logger.Debug("Current user is manual review admin.");
                return;
            }

            if (TenantLocalValue.RequesterType == RequesterTypeEnum.OpusControlPlus)
            {
                logger.Info($"Current user is google manual review admin.");
                return;
            }

            var reviewerFilter = new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.MLReviewer,
                Value = "[]"
            };
            queryDefinition.Filters.Add(reviewerFilter);

            var userHasPermissionIntIds = await UserService.GetUserAndGroupIdsAsync(TenantLocalValue.LogonUserId);
            reviewerFilter.Value = JsonConvert.SerializeObject(userHasPermissionIntIds);
            logger.Debug($"Get review data by: {JsonConvert.SerializeObject(userHasPermissionIntIds)}.");
        }

        private static async Task<List<Expression<Func<ManualApprovalRecord, bool>>>> BuildCosmosDBFilterAsync(ManualApprovalQueryDefinition queryDefinition)
        {
            var result = new List<Expression<Func<ManualApprovalRecord, bool>>>();
            foreach (var filterDefinition in queryDefinition.Filters)
            {
                var filterOption = filterDefinition.FilterOption;
                var filter = FilterCollection[filterOption];
                var expression = await filter.GetCosmosDBFilterExpressionAsync(filterDefinition.Value);
                result.Add(expression);
            }

            return result;
        }

        private static List<ManualApprovalExplorerOrderDefinition> BuildCosmosDBSorter(ManualApprovalQueryDefinition queryDefinition)
        {
            var result = new List<ManualApprovalExplorerOrderDefinition>();
            if (queryDefinition.OrderBy != ManualApprovalOrderOptions.None)
            {
                var sorter = SorterCollection[queryDefinition.OrderBy];
                var expression = sorter.GetCosmosDBOrderExpression();
                result.Add(new ManualApprovalExplorerOrderDefinition
                {
                    OrderKeySelector = expression,
                    IsDesc = queryDefinition.IsDesc
                });
            }

            if (queryDefinition.OrderBy == ManualApprovalOrderOptions.None)
            {
                var collectionTimeSorter = SorterCollection[ManualApprovalOrderOptions.CollectioinTime];
                var collectionTimeExpression = collectionTimeSorter.GetCosmosDBOrderExpression();
                result.Add(new ManualApprovalExplorerOrderDefinition
                {
                    OrderKeySelector = collectionTimeExpression,
                    IsDesc = true
                });
            }

            return result;
        }
        private void UpdateJobState()
        {
            JobStatus status = JobStatus.Failed;
            if (!mHasError)
            {
                int successCount = mSuccessCount;
                int failedCount = mFailedCount;
                if (failedCount > 0 && successCount == 0)
                {
                    status = JobStatus.Failed;
                }
                else if (failedCount > 0 && successCount > 0)
                {
                    status = JobStatus.FinishWithException;
                }
                else if (successCount == 0)
                {
                    status = JobStatus.Skipped;
                }
                else
                {
                    status = JobStatus.Finished;
                }
            }
            else
            {
                status = JobStatus.Failed;
            }
            reportManager.SetJobFinished(status);
        }
    }
}
