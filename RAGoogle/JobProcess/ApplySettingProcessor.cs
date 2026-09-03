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
using AvePoint.GCommon.Contract.Tree;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Label;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Cloud.Sdk.Data.Amls.Ics.Contracts;
using Newtonsoft.Json;
using Polly;
using RAGoogle.Extension;
using RAGoogle.Helper;
using RAGoogle.Models;
using RAGoogle.Services;
using RAGoogle.Util;
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Diagnostics;
using Util;
using EnableRecordManagementSetting = AvePoint.RA.Contract.Global.Object.EnableRecordManagementSetting;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.DB.Dao.Extension;
using Aspose.Pdf.Operators;
using Microsoft.Azure.Cosmos;
using AvePoint.GCommon.Utility;
using RAGoogle.Helpers;
using AvePoint.RA.Common.AI;
using Google;

namespace RAGoogle.JobProcess;

public class ApplySettingProcessor : BaseProcessor
{
    private const int BatchSize = 30;
    private readonly HashSet<string> _invalidLabelCache = [];
    private readonly ConcurrentDictionary<string, List<string>> _invalidItemCache = [];
    private readonly Dictionary<Tuple<Guid, string>, RMGoogleLabelInfo> _googleLabelDictionary;

    protected readonly ConcurrentBag<Exception> _exceptions = new();
    private bool ExtractFileFail = false;
    private static IRMMLTrainingModelDao mlTrainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();
    private IRMMLTermDao trainingTermDao => PlatformWindsorManager.GetService<IRMMLTermDao>();

    private Guid _defaultTrainingModeId;

    private IJobInfoUpdater _jobInfoUpdater;
    protected IJobInfoUpdater JobInfoUpdater
    {
        get
        {
            if (_jobInfoUpdater == null)
            {
                _jobInfoUpdater = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));
            }
            return _jobInfoUpdater;
        }
    }
    public Guid DefaultTrainingModeId
    {
        get
        {
            if (_defaultTrainingModeId.Equals(Guid.Empty))
            {
                var trainingMode = mlTrainingModelDao.GetDefaultModel();
                if (trainingMode != null)
                {
                    _defaultTrainingModeId = trainingMode.Id;
                }
            }
            return _defaultTrainingModeId;
        }
    }

    private ExplorerDao _explorerDao;
    public IExplorerDao ExplorerDao
    {
        get
        {
            if (_explorerDao == null)
            {
                _explorerDao = new ExplorerDao(true);
            }
            return _explorerDao;
        }
    }

    public ApplySettingProcessor(string jobId) : base(jobId, JobType.GoogleApplySettings)
    {
        ReportCenter.InitCurrentJobInfo(jobId, JobType.GoogleApplySettings);
        // Get all label info from DB
        _googleLabelDictionary = TermDao.GetGoogleLabelInfos();
        JobInfoUpdater.UpdateJobState(jobId, (int)JobStatus.InProgress);
        JobInfoUpdater.UpdateJobProgress(jobId, 1);
    }

    public override async Task RunNowAsync(RMGoogleSetting? setting, GoogleDriveTreeNodeDto? node)
    {
        using (var performance = new PerformanceScope("ApplySettingProcessor.RunNowAsync"))
        {
            try
            {
                using (CheckJobStopScope jScope = new())
                {
                    _invalidLabelCache.Clear();
                    _invalidItemCache.Clear();
                    if (setting is null || node is null)
                    {
                        logger.Error("Setting node and Node info are invalid.");
                        throw new ArgumentNullException("Setting node and Node info are invalid.");
                    }
                    List<int> nodeLevels = [(int)RMNodeLevel.GoogleContainer, (int)RMNodeLevel.GoogleDrive];
                    ReportCenter.AssignAccessLevel(nodeLevels);
                    DataQueue<GoogleItemData> itemQueue = new DataQueue<GoogleItemData>();
                    if (setting.EnableRecordManagement != (int)EnableRecordManagementSetting.Enable)
                    {
                        logger.Info("This node is disable records management.");
                        ReportCenter.RecordSkip(ReportCenter.GenerateCommonJobDetail(jobType, node, JobDetailsStatus.Skipped, I18NResource.DisableRecordsManagement), (int)RMNodeLevel.GoogleDrive);
                        return;
                    }
                    if (setting.DeployLabelMethod == (int)DeployLabelMethod.UseManualClassification)
                    {
                        logger.Info("Manual apply label setting, skip all.");
                        ReportCenter.RecordSkip(ReportCenter.GenerateCommonJobDetail(jobType, node, JobDetailsStatus.Skipped, I18NResource.ManualSelectLabelOption), (int)RMNodeLevel.GoogleDrive);
                        return;
                    }
                    
                    var task = Task.Run(() => ProcessItemDataAsync(setting, itemQueue, node));
                    await ProcessApplySettingsAsync(node, setting, itemQueue);
                    if (setting.AISendEMail)
                    {
                        var recordOwnerSettingType = RecordOwnerSettingType.AIGoogleDrive;
                        var recordOwners = await RMMachineLearningReviewerUtility.GetRecordOwnersAsync(setting.Id, recordOwnerSettingType);
                        if (recordOwners != null && recordOwners.Length > 0)
                        {
                            RMMachineLearningReviewerUtility.AddNeedSendEmailUserId(recordOwners);
                        }
                        try
                        {
                            RMMachineLearningReviewerUtility.Commit(jobId);
                        }
                        catch (Exception e)
                        {
                            logger.Warn($"An error while commit manual reviewers, message: {e}");
                        }
                    }
                    ReportCenter.UpsertLastRunTime(scanTimeTicks);
                    itemQueue.Complete();
                    task.Wait();
                    if (_exceptions.Any())
                    {
                        ReportCenter.RecordFailed(ReportCenter.GenerateCommonJobDetail(jobType, node, JobDetailsStatus.Failed, I18NResource.UnexpectedError), (int)RMNodeLevel.GoogleDrive);
                        return;
                    }
                    JobDetailsStatus detailStatus = ReportCenter.CalculateJobDetails();
                    if (detailStatus != JobDetailsStatus.Successful)
                    {
                        string message = I18NResource.UnexpectedException;
                        if (_invalidLabelCache.HasValue())
                        {
                            message = I18NResource.LabelInvalidException;
                        }
                        else if (_invalidItemCache.ContainsKey(I18NResource.LabelNoPermission))
                        {
                            message = I18NResource.LabelNoPermission;
                        }
                        else if (_invalidItemCache.ContainsKey(I18NResource.LabelInvalidOverwritePermissionException))
                        {
                            message = I18NResource.LabelInvalidOverwritePermissionException;
                        }
                        else if (_invalidItemCache.ContainsKey(I18NResource.LabelLimitApplied))
                        {
                            message = I18NResource.LabelLimitApplied;
                        }
                        ReportCenter.RecordCommon(ReportCenter.GenerateCommonJobDetail(jobType, node, detailStatus, message), (int)RMNodeLevel.GoogleDrive);
                        return;
                    }
                    ReportCenter.RecordSuccessful(ReportCenter.GenerateCommonJobDetail(jobType, node, detailStatus, string.Empty), (int)RMNodeLevel.GoogleDrive);
                }
            }
            catch (JobStopException)
            {
                logger.Warn("The job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception ex)
            {
                logger.Error("Failed to kick off apply setting job, Message: {0}", ex);
                if(ex is GoogleApiException gex && (gex.HttpStatusCode == System.Net.HttpStatusCode.NotFound && gex.Message.Contains(node.ObjectId)))
                {
                    throw new NotFoundDriveException(I18NEntity.GetString("RM_JM_JD_NotFound_Drive"));
                }
                ReportCenter.RecordFailed(ReportCenter.GenerateCommonJobDetail(jobType, node, JobDetailsStatus.Failed, ex.Message), (int)RMNodeLevel.GoogleDrive);
                throw;
            }
        }
    }

    private async Task ProcessApplySettingsAsync(GoogleDriveTreeNodeDto node, RMGoogleSetting setting, DataQueue<GoogleItemData> itemQueue)
    {
        using (var performance = new PerformanceScope("ApplySettingProcessor.ProcessApplySettingsAsync"))
        using (CheckJobStopScope jScop = new CheckJobStopScope())
        {
            try
            {
                await ProcessDiscoveryItemsData(node, setting, itemQueue);
            }
            catch (JobStopException)
            {
                logger.Warn("The apply setting job has been stopped.");
                throw new JobStopException("The job has stopped."); ;
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to process apply setting job, Message: {ex}");
                throw;
            }
        }
    }

    private async Task ProcessItemDataAsync(RMGoogleSetting setting, DataQueue<GoogleItemData> itemQueue, GoogleDriveTreeNodeDto selectedNode)
    {
        using (CheckJobStopScope jScope = new())
        {
            using var scopeTokenUsage = TokenUsageCache.Begin();
            logger.Info($"start to count Token usage");
            var needPredictionData = setting.DeployLabelMethod == (int)DeployLabelMethod.UseIntelligenceClassification || setting.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault;

            if (!needPredictionData)
            {
                await itemQueue.ToIEnumerable().ParallelExecute(async item =>
                {
                    try
                    {
                        using (CheckJobStopScope subJScope = new CheckJobStopScope())
                        {
                            using (new PerformanceScope("GoogleApplySetting:ProcessDataItem"))
                            {
                                if (item.Level == RMNodeLevel.GoogleFile)
                                {
                                    await ExecuteApplySettingAsync(item, setting, selectedNode);
                                }
                            }
                        }
                    }
                    catch (JobStopException ex)
                    {
                        logger.Warn("the job has stopped.");
                        _exceptions.Add(new Exception($"Item ID {item.Id}: {ex.Message}", ex));
                        throw new JobStopException("The job has stopped.");
                    }
                    catch (Exception e)
                    {
                        logger.Error($"An error occurred while process item [{item.Id}]. Error: {e}");
                        _exceptions.Add(new Exception($"Item ID {item.Id}: {e.Message}", e));
                        throw;
                    }
                }, MaxDegreeOfParallelism, Cts.Token);
            }
            else
            {
                logger.Info("Processing items with prediction data (AI classification enabled).");
                await HandlePredictionDataAsync(setting, itemQueue, selectedNode);
            }
            var total = scopeTokenUsage.End();
            logger.Info($"Grand total token usage: {total}");
        }
    }

    private async Task HandlePredictionDataAsync(RMGoogleSetting setting, DataQueue<GoogleItemData> itemQueue, GoogleDriveTreeNodeDto selectedNode)
    {
        BlockingCollection<(GoogleItemData, ScoreResponse)> predictionResultsQueue = new(BatchSize);
        int concurrentThreadCount = 0;
        
        // Start the consumer task
        var consumerTask = Task.Run(async () =>
        {
            logger.Info($"Consumer task started on ThreadId: {Thread.CurrentThread.ManagedThreadId}, Active threads: {Process.GetCurrentProcess().Threads.Count}");
            await predictionResultsQueue.GetConsumingEnumerable()
                .ParallelExecute(async (data) =>
            {
                Thread.CurrentThread.Name = $"GoogleApplySettingWorker-{Environment.CurrentManagedThreadId}";
                var currentConcurrentCount = Interlocked.Increment(ref concurrentThreadCount);
                var (item, scoreResponse) = data;
                logger.Info($"Processing item [{item.Id}] on ThreadId: {Environment.CurrentManagedThreadId}, Concurrent threads in GetConsumingEnumerable: {currentConcurrentCount}");
                try
                {
                    using (CheckJobStopScope subJScope = new CheckJobStopScope())
                    {
                        using (new PerformanceScope("GoogleApplySetting:ProcessDataItem"))
                        {
                            if (item.Level == RMNodeLevel.GoogleFile)
                            {
                                var labelInfo = await ExecuteApplySettingAsync(item, setting, selectedNode, scoreResponse);
                                if (labelInfo?.NeedIpdateCosmosDB == true)
                                {
                                    await CreateOrUpdateMLPredictionRecord(labelInfo, item, setting, selectedNode);
                                    if (labelInfo?.ApplyLabelType == ApplyLabelType.SkipApplyViaSmartTermByManual)
                                    {
                                        ReportCenter.RecordSuccessful(item.GenerateApplySettingJobDetail(labelInfo.LabelName ?? "", I18NResource.ApplyViaSmartTerm,
                                            JobDetailsStatus.Successful, I18NResource.ApplySmartTermViaManual), (int)item.Level);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (JobStopException ex)
                {
                    logger.Warn("the job has stopped.");
                    _exceptions.Add(new Exception($"Item ID {item.Id}: {ex.Message}", ex));
                    throw new JobStopException("The job has stopped.");
                }
                catch (Exception e)
                {
                    logger.Error($"An error occurred while process item [{item.Id}]. Error: {e}");
                    _exceptions.Add(new Exception($"Item ID {item.Id}: {e.Message}", e));
                    throw;
                }
                finally
                {
                    Interlocked.Decrement(ref concurrentThreadCount);
                }
            }, MaxDegreeOfParallelism, Cts.Token);
        });

        // Producer loop
        foreach (var items in itemQueue.ToIEnumerable().Batch(BatchSize))
        {
            logger.Info($"Batch prediction for {items.Count()} items started");
            var itemList = items.ToList(); // Materialize once to avoid multiple enumeration
            PredictRequest predictRequest = new()
            {
                ScoreRequests = new List<ScoreRequest>(itemList.Count * 2) // Pre-allocate with estimated capacity
            };
            foreach (var item in itemList)
            {
                var labelInfor = new LabelInfo();
                if (setting.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
                {
                    // Check label match with auto
                    labelInfor = LabelManager.GetAutoMatchedRuleLabelInfo(item, ConvertHelper.ConvertRMSetting2Dto(setting));
                }
                if (labelInfor == null)
                {
                    logger.Info($"Using AI to get smart term for item: {item.Id}");
                    // TODO use mulit-thread to get prediction request
                    var tempTredictRequest = await MLPredictHelper.GetPredictRequest(item);
                    if (tempTredictRequest.ScoreRequests.Count > 0)
                    {
                        predictRequest.ScoreRequests.AddRange(tempTredictRequest.ScoreRequests);
                    }
                    else
                    {
                        try
                        {
                            var predictResultFail = MLPredictHelper.GetPredictRequestFailCache(item.UniqueId);
                            if (predictResultFail != null)
                            {
                                CacheInvalidItems(I18NResource.UnexpectedExtractFileContent, item.Id);
                                ReportCenter.RecordFailed(item.GenerateApplySettingJobDetail(string.Empty, I18NResource.GoogleApplySettings, JobDetailsStatus.Failed, I18NResource.UnexpectedExtractFileContent), (int)item.Level);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"Error while processing failed predict cache for item {item.UniqueId}, error: {ex}");
                        }
                        logger.Warn($"Item {item.Id} could not generate prediction request, skipped.");
                    }
                }
                else
                {
                    predictionResultsQueue.Add((item, null));
                }
            }
            var predictResponseList = MLPredictHelper.GetPredictResult(predictRequest);
            if (predictResponseList?.Count > 0)
            {
                // Pre-build lookup dictionary for better performance
                var itemLookup = itemList.ToDictionary(i => i.Id.ToString(), i => i);
                
                // Add prediction results to the queue for processing
                foreach (var scoreResponse in predictResponseList)
                {
                    if (scoreResponse?.Name != null && itemLookup.TryGetValue(scoreResponse.Name, out var item))
                    {
                        predictionResultsQueue.Add((item, scoreResponse));
                    }
                }
            }
            else
            {
                logger.Warn("No valid prediction response received.");
            }
        }
        
        predictionResultsQueue.CompleteAdding();
        await consumerTask;
    }

    /// <summary>
    /// Creates or updates ML prediction record for items with smart label predictions
    /// </summary>
    /// <param name="labelInfo">The predicted label information</param>
    /// <param name="item">The Google item data</param>
    /// <param name="setting">The Google setting configuration</param>
    /// <param name="selectedNode">The selected Google Drive tree node</param>
    private async Task CreateOrUpdateMLPredictionRecord(LabelInfo? labelInfo, GoogleItemData item, RMGoogleSetting setting, GoogleDriveTreeNodeDto selectedNode)
    {
        if (labelInfo != null && labelInfo.SmartLabelApplyType != SmartLabelApplyType.None)
        {
            var isExist = RecordManager.TryGetRecordValue(item.UniqueId, 0, out var recordInDB);
            var recordOwners = await RMMachineLearningReviewerUtility.GetRecordOwnersAsync(setting.Id, RecordOwnerSettingType.AIGoogleDrive);
            var trainingTerm = trainingTermDao.GetValidTrainingTerm(new Guid(labelInfo.UniqueLabelId));
            int mLClassificationType = GetMLClassificationType(setting.AIApprovalType, trainingTerm);

            var record = isExist
                    ? CloneAndUpdateExistingRecord(recordInDB)
                    : item.ConvertToRecord(selectedNode, recordInDB, AvePoint.RA.Contract.Explorer.RMRecordStatus.TrainingManualSync);

            UpdateMLPredictFields(record, labelInfo, recordOwners, mLClassificationType, labelInfo.SmartLabelApplyType);

            if (isExist)
            {
                ExplorerDao.UpdateAll(
                        r => r.ScopeId == recordInDB.ScopeId && r.NodeId == recordInDB.NodeId,
                        r =>
                        {
                            r.MLUnderReview = record.MLUnderReview;
                            r.MLApprovalStatus = record.MLApprovalStatus;
                            r.PredictTermId = record.PredictTermId;
                            r.PredictTermScore = record.PredictTermScore;
                            r.MLReviewer = record.MLReviewer;
                            r.PredictTime = record.PredictTime;
                            r.TrainingModelId = record.TrainingModelId;
                            r.MLClassificationType = record.MLClassificationType;
                            r.MLEscalateFrom = 0;
                            r.MLEscalatedComment = "";
                            if (string.IsNullOrEmpty(r.MetaInfo) || r.MetaInfo == "null")
                            {
                                logger.Info($"MetaInfo is null or empty, set MetaInfo for record [{record.Id}]");
                            }
                            r.MetaInfo = JsonConvert.SerializeObject(item.MetaInfo);
                        });
            }
            else
            {
                ExplorerDao.Add(record);
            }

            logger.Info($"record [{record.Id}]. record.SmartLabelApplyType: {record.RecordStatus}");
        }
    }

    private async Task<LabelInfo> ExecuteApplySettingAsync(GoogleItemData item, RMGoogleSetting setting, GoogleDriveTreeNodeDto selectedNode, ScoreResponse? scoreResponse = null)
    {
        try
        {
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                LabelManager.SetTenantId(selectedNode.TenantId);
                LabelInfo? matchedLabelInfo = await LabelManager.GetMatchedLabelInfo(item, ConvertHelper.ConvertRMSetting2Dto(setting), scoreResponse);
                if (matchedLabelInfo == null || matchedLabelInfo.UniqueLabelId == null)
                {
                    // if can not get default label from classification rules, get from setting
                    if (setting.NeedCheckDefaultValue)
                    {
                        matchedLabelInfo = new()
                        {
                            UniqueLabelId = setting.DefaultLabelId,
                            LabelName = setting.DefaultLabelName,
                        };
                        return null;
                    }
                    else
                    {
                        logger.Info("No matched label and choose manually setting, skip.");
                        ReportCenter.RecordSkip(item.GenerateApplySettingJobDetail(string.Empty, I18NResource.GoogleApplySettings,
                            JobDetailsStatus.Skipped, I18NResource.NoMatchedLabel), (int)item.Level);
                        return null;
                    }
                }
                if (matchedLabelInfo.IsManually)
                {
                    logger.Info("Item applied label manually, skipped. ItemId:{0}", item.Id);
                    ReportCenter.RecordSkip(item.GenerateApplySettingJobDetail(string.Empty, I18NResource.GoogleApplySettings,
                        JobDetailsStatus.Skipped, I18NResource.ManualChooseLabel), (int)item.Level);
                }
                else
                {
                    using (GoogleDriveService service = new(appProfile, item.MemberEmail))
                    {
                        var existLabels = item.LableIds;
                        if (existLabels != null && existLabels.Count > 0 && setting.AutoJobOption == (int)AutoJobOption.SkipAndKeep)
                        {
                            // have exists label but choose skip setting, skip
                            logger.Info("Skip and Keep existing label on file. ItemId:{0}", item.Id);
                            ReportCenter.RecordSkip(item.GenerateApplySettingJobDetail(string.Empty, I18NResource.GoogleApplySettings,
                                JobDetailsStatus.Skipped, I18NResource.SkipAppliedLabel), (int)item.Level);
                            return null;
                        }

                        if (_invalidLabelCache.Contains(matchedLabelInfo.UniqueLabelId))
                        {
                            logger.Error($"label id: {matchedLabelInfo.UniqueLabelId} is invalid");
                            return null;
                        }

                        if (_googleLabelDictionary.TryGetValue(new Tuple<Guid, string>(new Guid(matchedLabelInfo.UniqueLabelId), selectedNode.TenantId), out var labelInfo))
                        {
                            logger.Info($"matchedLabelInfo.SmartLabelApplyType:{matchedLabelInfo.SmartLabelApplyType}");

                            if (matchedLabelInfo.SmartLabelApplyType != SmartLabelApplyType.ManualReview)
                            {
                                if (CheckSkipLabel(existLabels, labelInfo.LabelId, setting.AutoJobOption))
                                {
                                    logger.Info("Label already applied on file, skipped. ItemId:{0}, LabelId:{1}", item.Id, labelInfo.LabelId);
                                    ReportCenter.RecordSkip(item.GenerateApplySettingJobDetail(labelInfo.LabelName ?? "", I18NResource.GoogleApplySettings
                                        , JobDetailsStatus.Skipped, I18NResource.LabelAlreadyApplied), (int)item.Level);
                                    return null;
                                }

                                if (setting.AutoJobOption == (int)AutoJobOption.Append)
                                {
                                    await RetryExecuteAppendApplyLabelAsync(service, labelInfo, item, matchedLabelInfo.ApplyLabelType, existLabels);
                                }
                                else
                                {
                                    await RetryExecuteApplyLabelAsync(service, labelInfo, item, matchedLabelInfo.ApplyLabelType, existLabels);
                                }
                            }
                        }
                        else
                        {
                            logger.Error($"can not find the valid matched label info, matched label id: {matchedLabelInfo.UniqueLabelId}");
                            _invalidLabelCache.Add(matchedLabelInfo.UniqueLabelId);
                            ReportCenter.RecordFailed(item.GenerateApplySettingJobDetail(matchedLabelInfo.LabelName, GetI18NValueOfApplyLabelType(matchedLabelInfo.ApplyLabelType), JobDetailsStatus.Failed, I18NResource.LabelInvalidException), (int)item.Level);
                            return null;
                        }
                    }
                }
                return matchedLabelInfo;
            }
        }
        catch (JobStopException)
        {
            logger.Warn("the job has stopped.");
            throw new JobStopException("The job has stopped.");
        }
        catch (Exception e)
        {
            if (e.Message.Contains(I18NResource.LabelInvalidOverwritePermissionException))
            {
                CacheInvalidItems(I18NResource.LabelInvalidOverwritePermissionException, item.Id);
            }

            if (e.Message.Contains(I18NResource.LabelNoPermission))
            {
                CacheInvalidItems(I18NResource.LabelNoPermission, item.Id);
            }
            logger.Error($"Execute apply setting {setting.Id} to item {item.Id} failed, Message: {e}");
            ReportCenter.RecordFailed(item.GenerateApplySettingJobDetail(string.Empty, I18NResource.GoogleApplySettings, JobDetailsStatus.Failed, e.Message), (int)item.Level);
            throw;
        }
    }

    private bool CheckSkipLabel(List<string>? existLabels, string labelApplyId, int autoJobOption)
    {
        return (AutoJobOption)autoJobOption switch
        {
            AutoJobOption.SkipAndKeep or AutoJobOption.Append
                => existLabels?.Contains(labelApplyId) == true,
            AutoJobOption.Override
                => existLabels?.Count == 1 && existLabels[0] == labelApplyId,
            _ => false
        };
    }

    private string GetI18NValueOfApplyLabelType(ApplyLabelType applyLabelType) => applyLabelType switch
    {
        ApplyLabelType.ApplyDefaultLabel => I18NResource.ApplyDefault,
        ApplyLabelType.ApplyViaSmartTerm => I18NResource.ApplyViaSmartTerm,
        ApplyLabelType.SkipApplyViaSmartTermByManual => I18NResource.ApplyViaSmartTerm,
        _ => I18NResource.ApplyAutoPopulate
    };

    private void CacheInvalidItems(string key, string value)
    {
        if (_invalidItemCache.ContainsKey(key))
        {
            _invalidItemCache[key].Add(value);
        }
        else
        {
            _invalidItemCache.TryAdd(key, [value]);
        }
    }

    private async Task RetryExecuteApplyLabelAsync(GoogleDriveService service, RMGoogleLabelInfo labelInfo, GoogleItemData item, ApplyLabelType applyLabelType, List<string>? existLabels = null)
    {
        int retryCount = 0;
        do
        {
            retryCount++;
            try
            {
                if (labelInfo == null)
                {
                    logger.Error("Invalid label info");
                    break;
                }
                if (existLabels.IsNotNullOrEmpty())
                {
                    logger.Info("Remove current applied labels on file. ItemId:{0}", item.Id);
                    await service.BatchRemoveLabelsOnFileAsync(existLabels.ToList(), item.Id);
                }
                await service.AppliedLabelOnFileAsync(labelInfo.LabelId, item.Id);
                logger.Info("Apply label {0} on item {1} successfully.", labelInfo.UniqueId, item.Id);
                ReportCenter.RecordSuccessful(item.GenerateApplySettingJobDetail(labelInfo.LabelName ?? "", GetI18NValueOfApplyLabelType(applyLabelType),
                        JobDetailsStatus.Successful, string.Empty), (int)item.Level);
                break;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("AppliedLabelReachoutLimit") && existLabels.IsNotNullOrEmpty())
                {
                    logger.Info($"The Label cannot be applied because the Label limit on File {item.Id} has been reached. Remove all current labels on File.");
                    await service.BatchRemoveLabelsOnFileAsync(existLabels, item.Id);
                    existLabels.Clear();
                }
                else
                {
                    throw;
                }
            }
        }
        while (retryCount <= 3);
    }
    private async Task RetryExecuteAppendApplyLabelAsync(GoogleDriveService service, RMGoogleLabelInfo labelInfo, GoogleItemData item, ApplyLabelType applyLabelType, List<string>? existLabels = null)
    {
        try
        {
            if (labelInfo == null)
            {
                logger.Error("Invalid label info");
                return;
            }
            if (existLabels.IsNotNullOrEmpty() && existLabels.Count == 5)
            {
                logger.Info($"The Label cannot be applied because the Label limit on File {item.Id} has been reached.");
                ReportCenter.RecordFailed(item.GenerateApplySettingJobDetail(labelInfo.LabelName, GetI18NValueOfApplyLabelType(applyLabelType), JobDetailsStatus.Failed, I18NResource.LabelLimitApplied), (int)item.Level);
                CacheInvalidItems(I18NResource.LabelLimitApplied, item.Id);
                return;
            }
            await service.AppliedLabelOnFileAsync(labelInfo.LabelId, item.Id);
            logger.Info("Apply label {0} on item {1} successfully.", labelInfo.UniqueId, item.Id);
            ReportCenter.RecordSuccessful(item.GenerateApplySettingJobDetail(labelInfo.LabelName ?? "", GetI18NValueOfApplyLabelType(applyLabelType),
                    JobDetailsStatus.Successful, string.Empty), (int)item.Level);
        }
        catch (Exception ex)
        {
            logger.Error($"An error occurred while process item [{item.Id}],[{item.Name}] . Error: {ex}");
            throw;
        }
    }

    private int GetMLClassificationType(ApprovalType approvalType, MLTermDto trainingTerm)
    {
        return approvalType switch
        {
            ApprovalType.None => (int)RMMLClassificationType.AutoClassfied,
            ApprovalType.RecordOwners => trainingTerm.AutoApply
                ? (int)RMMLClassificationType.AutoClassfied
                : (int)RMMLClassificationType.None,
            _ => 0
        };
    }

    private void UpdateMLPredictFields(Record record, LabelInfo labelInfo, int[] recordOwners, int classificationType, SmartLabelApplyType smartLabelApplyType)
    {
        record.MLUnderReview = smartLabelApplyType == SmartLabelApplyType.ManualReview ? (int)RMMLUnderReview.IsManual : (int)RMMLUnderReview.DirectAssign;
        record.MLApprovalStatus = smartLabelApplyType == SmartLabelApplyType.ManualReview ? (int)RMMLApprovalStatus.WaitingApprove : (int)RMMLApprovalStatus.None;
        record.PredictTermId = new Guid(labelInfo.UniqueLabelId);
        record.PredictTermScore = labelInfo.Score;
        record.MLReviewer = recordOwners;
        record.PredictTime = DateTime.UtcNow.Ticks;
        record.TrainingModelId = DefaultTrainingModeId;
        record.MLClassificationType = classificationType;
    }

    private Record CloneAndUpdateExistingRecord(Record existingRecord)
    {
        var clone = new Record();
        clone.CopyFrom(existingRecord);
        return clone;
    }
}
