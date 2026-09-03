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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.VectorDataCenter.Models;
using AvePoint.RA.VectorDataCenter.Services;
using AvePoint.RA.VectorDataCenter.Similarity;
using AvePoint.RA.VectorDataCenter.Storage;
using RAGoogle.Common;
using RAGoogle.Discover.Impl;
using RAGoogle.GoogleObjDiscover.Impl;
using RAGoogle.Helper;
using RAGoogle.Models;
using RAGoogle.Report;
using RAGoogle.Util;
using Util;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.I18N.Core;
using Google;

namespace RAGoogle.JobProcess;

public abstract class BaseProcessor
{
    protected IRALogger logger = RALogger.GetInstance(typeof(BaseProcessor));
    protected ReportCenter ReportCenter;
    public RecordManager RecordManager;
    protected LabelManager LabelManager;
    protected RuleManager RuleManager;
    protected SettingManager SettingManager;
    public StopJobCts Cts;
    protected string scopeId;
    protected string jobId;
    public JobStatus jobFinishStatus;
    private DateTime scanTime = DateTime.UtcNow.AddMinutes(-15);  //-15 Preventing lost activity
    protected virtual bool NeedScanVersion => false;
    protected long scanTimeTicks
    {
        get { return scanTime.Ticks; }
    }
    protected string CustomerId { get; set; }
    protected JobType jobType;
    protected const int MaxDegreeOfParallelism = 10;
    protected bool isNeedAddOrUpdateVector = true;
    protected RMAosGoogleAppProfile appProfile;

    protected IRMGoogleSettingDao GoogleSettingDao => PlatformWindsorManager.GetService<IRMGoogleSettingDao>();

    protected IRMRemoteGoogleNodeService RemoteGoogleNodeService => PlatformWindsorManager.GetService<IRMRemoteGoogleNodeService>();

    protected IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();

    protected IRMGoogleLabelInfoDao GoogleLabelInfoDao => PlatformWindsorManager.GetService<IRMGoogleLabelInfoDao>();

    protected ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();

    protected IRMMLTermDao RMMLTermDao => PlatformWindsorManager.GetService<IRMMLTermDao>();

    protected IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

    protected IRMMLTrainingModelDao RMMLTrainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();

    //protected ConcurrentDictionary<string, string> GoogleLabelIdNameMapping { get; set; }

    public BaseProcessor(string jobId, JobType jobType)
    {
        this.jobId = jobId;
        ReportCenter = new ReportCenter();
        RecordManager = new RecordManager();
        LabelManager = new LabelManager();
        RuleManager = new RuleManager();
        SettingManager = new SettingManager();
        Cts = new StopJobCts();
        this.jobType = jobType;
    }

    public void Build(string customerId, string tenantId)
    {
        appProfile = RMAosApiClient.GetGoogleAppProfile(customerId, tenantId);
        if(appProfile == null)
        {
            logger.Warn($"Google app profile was not found in AOS for customer {customerId}, tenant {tenantId}.");
            throw new GoogleResourceNotFoundException(I18NEntity.GetString("RM_APP_GoogleAppProfileNotAvailable"));
        }
    }

    public async Task KickOffAsync()
    {
        using (var performance = new PerformanceScope("BaseProcessor.KickOffAsync"))
        {
            bool isSubJobFailed = false;
            try
            {
                using (CheckJobStopScope jScope = new())
                {
                    Cts.Config();
                    List<GoogleDriveTreeNodeDto> needRunNodes = new();
                    Dictionary<string, RMGoogleSetting> settingNodeMapping = [];
                    if (JobServiceUtility.IsSubJob(jobId))
                    {
                        RMSubJob subJobInfo = ReportCenter.GetSubJobInfo(jobId, true);
                        if (!string.IsNullOrEmpty(subJobInfo.JobContext.Settings))
                        {
                            settingNodeMapping =
                                SerializerHelper.DeserializeByDataContractSerializer<Dictionary<string, RMGoogleSetting>>(subJobInfo.JobContext.Settings);
                            SettingManager.LoadGoogleSettings(settingNodeMapping);
                        }
                        if (!string.IsNullOrEmpty(subJobInfo.JobContext.Content))
                        {
                            var selectedNodes =
                                SerializerHelper.DeserializeByDataContractSerializer<List<RMGoogleTreeNode>>(subJobInfo.JobContext.Content);
                            selectedNodes.ForEach(node => needRunNodes.Add(ConvertHelper.ConvertGoogleRM2Dto(node)));
                        }
                    }
                    if (needRunNodes.IsNotNullOrEmpty())
                    {
                        logger.Info("Need processed node count is {0}", needRunNodes.Count);
                        int totalNodeCount = needRunNodes.Count;
                        int currentNodeIndex = 0;
                        foreach (var node in needRunNodes)
                        {
                            bool isCurrentNodeFailed = false;
                            logger.Info($"Current processing node:{node.ID},object id:{node.ObjectId}, level:{node.Level}");
                            try
                            {
                                using (CheckJobStopScope subScope = new())
                                {
                                    scopeId = node.ID;
                                    var driveId = GetDriveNode(node).ID;
                                    var containerId = GetContainerNode(node).ID;

                                    RMGoogleSetting? setting = SettingManager.TryGetGoogleSetting(containerId, scopeId, driveId);
                                    ReportCenter.Init(scopeId, containerId, setting != null && (setting.DeployLabelMethod == (int)DeployLabelMethod.UseAutoClassification || setting.DeployLabelMethod == (int)DeployLabelMethod.UseIntelligenceClassification)); ;
                                    RecordManager.Init(ReportCenter, AvePoint.RA.Contract.Explorer.SourceFlag.Google);

                                    if (setting == null)
                                    {
                                        logger.Error($"Can not get setting of node {node.ID} and parent node {node.ContainerId}.");
                                        ReportCenter.RecordSkipCommon(ReportCenter.GenerateCommonJobDetail(jobType, node, JobDetailsStatus.Skipped, "The setting node is invalid."), (int)node.Level);
                                        continue;
                                    }
 
                                    if (jobType == JobType.GoogleApplySettings)
                                    {
                                        bool isDeletedTerm = ValidateDeletedTermInApplySetting(setting, node);
                                        if (isDeletedTerm)
                                        {
                                            continue;
                                        }
                                        await HandleAddOrUpdateVectorTerm(setting);
                                    }
                                    var treeNode = await RemoteGoogleNodeService.GetRemoteNodeByDriveIdAsync(node.NodeId);
                                    var tenantId = treeNode.GoogleTenantId;
                                    Build(TenantLocalValue.LogonGroupId, tenantId);
                                    node.TenantId = tenantId;
                                    await RunNowAsync(setting, node);
                                }
                            }
                            catch (JobStopException)
                            {
                                logger.Warn("The job has stopped.");
                                ReportCenter.JobHasStopped = true;
                                throw new JobStopException("The job has stopped.");
                            }
                            catch (GoogleResourceNotFoundException ex)
                            {
                                var detail = jobType == JobType.GoogleDataSynchronization ? ReportCenter.GenerateJobDetailForGoogleSyncContent(jobType, node, JobDetailsStatus.Failed, ex.Message) : ReportCenter.GenerateDeletedDriveJobDetail(jobType, node, ex.Message);
                                ReportCenter.RecordFailed(detail, (int)RMNodeLevel.GoogleDrive);
                            }
                            catch (Exception ex)
                            {
                                isCurrentNodeFailed = true;
                                logger.Info($"Failed to processing node:{node.ID},exception:{ex}");
                                ReportCenter.RecordFailedCommon(ReportCenter.GenerateCommonJobDetail(jobType, node, JobDetailsStatus.Failed, ex.Message), (int)node.Level);
                            }
                            finally
                            {
                                if (ReportCenter.GetMainJobState() == JobStatus.Stopping)
                                {
                                    logger.Warn("The main job is stopping, need to stop all sub job.");
                                    throw new JobStopException("The job has stopped.");
                                }

                                if (!ReportCenter.JobHasStopped && !isCurrentNodeFailed)
                                {
                                    int nextProgress = 100 * ++currentNodeIndex / totalNodeCount;
                                    var currentProgress = ReportCenter.GetProgress(jobId);
                                    if (currentProgress < nextProgress)
                                    {
                                        logger.Info($"Update progress from {currentProgress} to {nextProgress} for: [{node.ID}].");
                                        ReportCenter.SetProgress(jobId, nextProgress);
                                    }
                                }
                            }
                        }
                    }
                    else if (jobType == JobType.TermSynchronization || jobType == JobType.ImportGoogleTermStructure)
                    {
                        try
                        {
                            using (CheckJobStopScope subScope = new())
                            {
                                await RunNowAsync(null, null);
                            }
                        }
                        catch (JobStopException)
                        {
                            logger.Warn("The job has stopped.");
                            ReportCenter.JobHasStopped = true;
                            throw new JobStopException("The job has stopped.");
                        }
                        catch (Exception ex)
                        {
                            isSubJobFailed = true;
                            logger.Info($"Failed to processing job: {jobType},exception:{ex}");
                            throw;
                        }
                        finally
                        {
                            if (ReportCenter.GetMainJobState() == JobStatus.Stopping)
                            {
                                logger.Warn("The main job is stopping, need to stop all sub job.");
                                ReportCenter.JobHasStopped = true;
                            }

                            if (ReportCenter.JobHasStopped != true)
                            {
                                ReportCenter.CommitJobDetails(true);
                            }
                        }
                    }
                }
            }
            catch (JobStopException)
            {
                logger.Warn("The job has stopped.");
                ReportCenter.JobHasStopped = true;
                Cts.Cancel();
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while process job. Error: {ex}");
                isSubJobFailed = true;
                if (jobType != JobType.TermSynchronization)
                {
                    ReportCenter.SetJobFinish(JobStatus.Failed, ex.Message);
                }
            }
            finally
            {
                PerformanceMonitor.WritePerformanceResult();
                if (jobType == JobType.GoogleRecordsDisposal)
                {
                    ReportCenter.CommitDisposalAnalysis();
                }
                if (!isSubJobFailed)
                {
                    if (jobType == JobType.TermSynchronization)
                    {
                        jobFinishStatus = ReportCenter.Completed("", false);
                    }
                    else
                    {
                        ReportCenter.Completed();
                    }
                }
                if (jobType == JobType.GoogleApplySettings || jobType == JobType.GoogleDataSynchronization)
                {
                    await SettingManager.ResetSettingInfoAsync(jobId);
                }

                if (jobType == JobType.GoogleRecordsDisposal)
                {
                    GoogleLiteDBWrapper.CreateInstance(GooglePathUtil.GetDisposalRecordDBPath(jobId)).DeleteDBFile();
                }
                Cts.Dispose();
            }
        }
    }
    //internal async Task InitGoogleLabelIdNameMappingAsync(string tenantId)
    //{
    //    try
    //    {
    //        var appInfo = RMAosApiClient.GetGoogleAppProfile(TenantLocalValue.LogonGroupId, tenantId);

    //        using (GoogleLabelService service = new(appInfo))
    //        {
    //            var dics = await service.ListLabelsBasicAsync();
    //            GoogleLabelIdNameMapping = new ConcurrentDictionary<string, string>(dics, StringComparer.OrdinalIgnoreCase);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.Error($"An error occurred while init google label mapping. Error: {ex}");
    //        throw;
    //    }
    //}
    public abstract Task RunNowAsync(RMGoogleSetting? setting, GoogleDriveTreeNodeDto? node);

    private async Task HandleAddOrUpdateVectorTerm(RMGoogleSetting setting)
    {
        try
        {
            if (isNeedAddOrUpdateVector && setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable)
            {
                if ((DeployLabelMethod)setting.DeployLabelMethod == DeployLabelMethod.UseIntelligenceClassification && setting.AITermUseType == ArtificialIntelligenceTermUseType.ApplyTerm)
                {
                    if (KeyValueDao.EnableZeroShotFeature() && RMMLTrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot)
                    {
                        logger.Info("Handle Add or Update vector for the term");
                        var mlTerms = RMMLTermDao.GetAllMLTerm();
                        foreach (var term in mlTerms)
                        {
                            try
                            {
                                IVectorStore vectorStore = VectorStoreFactory.CreateVectorStore();
                                var queryService = await QueryService.CreateWithRAIProvider(vectorStore, new CosineSimilarityCalculator());
                                var metaData = await queryService.QueryMetaDataByTermId(term.Id);
                                if (!string.IsNullOrEmpty(term.Description) && !metaData.EqualsIgnoreCase(term.Description))
                                {
                                    logger.Info($"The term {term.Id} do not have vector or do have description change");
                                    var vectorizationService = await VectorizationService.CreateWithRAIProvider(vectorStore);
                                    await vectorizationService.StoreTermAsync(new TermDescription
                                    {
                                        Id = term.Id,
                                        Name = term.Name,
                                        Description = term.Description
                                    });
                                }
                                else
                                {
                                    logger.Info($"Skip update or create vector for term {term?.Id}");
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error($"Add or update the vector for term {term?.Id} has errors: {ex}");
                            }
                        }
                        isNeedAddOrUpdateVector = false;
                    }
                }
            }
        }
        catch(Exception e)
        {
            logger.Error($"Handle add or update vector term has error {e}");
        }
    }
    #region Discover
    public async Task ProcessDiscoveryItemsData(GoogleDriveTreeNodeDto node, RMGoogleSetting setting, DataQueue<GoogleItemData> itemQueue, bool isSyncData = false)
    {
        if (IsGoogleContainer(node.Level))
        {
            List<GoogleDriveTreeNodeDto> treeNodes = RemoteGoogleNodeService.BrowserTreeAsync(node);
            if (CheckIsFullDiscovery(setting, isSyncData))
            {
                foreach (GoogleDriveTreeNodeDto driveNode in treeNodes)
                {
                    await ProcessFullScanDriveAsync(driveNode, itemQueue, isSyncData);
                }
            }
            else
            {
                foreach (GoogleDriveTreeNodeDto driveNode in treeNodes)
                {
                    await ProcessIncrScanDriveAsync(driveNode, itemQueue, isSyncData);
                }
            }
        }
        else
        {
            if (CheckIsFullDiscovery(setting, isSyncData))
            {
                await ProcessFullScanDriveAsync(node, itemQueue, isSyncData);
            }
            else
            {
                await ProcessIncrScanDriveAsync(node, itemQueue, isSyncData);
            }
        }
    }

    private bool CheckIsFullDiscovery(RMGoogleSetting setting, bool isSyncData)
    {
        long lastDiscoverTime = ReportCenter.GetLastRunTime();
        bool isExceedTime = DateTime.UtcNow.AddDays(-59).Ticks >= lastDiscoverTime;
        if ((setting.RunAutoFullJob == true && !isSyncData) || lastDiscoverTime == 0 || isExceedTime)
        {
            return true;
        }

        return false;
    }

    private async Task ProcessFullScanDriveAsync(GoogleDriveTreeNodeDto node, DataQueue<GoogleItemData> itemQueue, bool isSync)
    {
        RMGoogleFullDiscover fullDiscover = new(itemQueue);
        fullDiscover.Init(ReportCenter, appProfile, true, NeedScanVersion);
        GoogleDriveData driveData = ConvertHelper.ConvertDtoNodeTreeToData(node, appProfile.TenantId);
        await fullDiscover.DiscoverAsync(driveData, Cts.Token);
        if (ReportCenter.GetLastRunTime() > 0 && isSync)
        {
            RMGoogleIncrDiscover incrDiscover = new(itemQueue);
            incrDiscover.Init(ReportCenter, appProfile, true);
            incrDiscover.SetScanTime(new DateTime(ReportCenter.GetLastRunTime()), scanTime);
            await incrDiscover.IncrementalDiscoveryDeletedItemsAsync(driveData, Cts.Token);
        }
    }

    private async Task ProcessIncrScanDriveAsync(GoogleDriveTreeNodeDto node, DataQueue<GoogleItemData> itemQueue, bool isSync)
    {
        RMGoogleIncrDiscover incrDiscover = new(itemQueue);
        incrDiscover.Init(ReportCenter, appProfile, true);
        incrDiscover.SetScanTime(new DateTime(ReportCenter.GetLastRunTime()), scanTime);
        GoogleDriveData driveData = ConvertHelper.ConvertDtoNodeTreeToData(node, appProfile.TenantId);
        if (isSync)
        {
            incrDiscover.SetFailedItemIds(ReportCenter.GetFailedItems(node.ContainerId, node.ID.ToString()));
        }
        await incrDiscover.DiscoverAsync(driveData, isSync, Cts.Token);
    }
    #endregion

    #region private methods
    private GoogleDriveTreeNodeDto GetContainerNode(GoogleDriveTreeNodeDto node)
    {
        var currentNode = node;
        while (!IsGoogleContainer(currentNode.Level) && currentNode.Parent != null)
        {
            currentNode = currentNode.Parent;
        }
        return currentNode;
    }

    internal GoogleDriveTreeNodeDto GetDriveNode(GoogleDriveTreeNodeDto node)
    {
        var currentNode = node;
        while (!IsGoogleDrive(currentNode.Level) && currentNode.Parent != null)
        {
            currentNode = currentNode.Parent;
        }
        return currentNode;
    }

    #endregion

    internal bool IsGoogleContainer(NodeLevel level)
    {
        return level == NodeLevel.GoogleMyDriveContainer || level == NodeLevel.GoogleSharedDriveContainer;
    }

    internal bool IsGoogleDrive(NodeLevel level)
    {
        return level == NodeLevel.GoogleMyDrive || level == NodeLevel.GoogleSharedDrive;
    }

    private bool ValidateDeletedTermInApplySetting(RMGoogleSetting setting, GoogleDriveTreeNodeDto node)
    {
        var settingClassificationRule = setting.AutoClassificationRules != null ? SerializerHelper.DeserializeByDataContractSerializer<List<ClassificationRule>>(setting.AutoClassificationRules) : null;
        var termIds = settingClassificationRule?.Where(t => t.TermId != null).Select(t => t.TermId).ToList() ?? new List<string>();
        if (termIds.Count > 0)
        {
            var guidTermIds = new List<Guid>();

            foreach (var id in termIds)
            {
                if (Guid.TryParse(id, out var guid))
                {
                    guidTermIds.Add(guid);
                }
                else
                {
                    logger.Warn($"Invalid GUID. Cannot parse termId: {id}");
                }
            }

            var hasTermDeleted = TermDao.CheckTermDeletedByIds(guidTermIds);

            if (hasTermDeleted)
            {
                ReportCenter.RecordFailedCommon(ReportCenter.GenerateCommonJobDetail(jobType, node, JobDetailsStatus.Failed, I18NResource.LabelInvalidException), (int)RMNodeLevel.GoogleDrive);
                return true;
            }
        }
        return false;
    }
}
