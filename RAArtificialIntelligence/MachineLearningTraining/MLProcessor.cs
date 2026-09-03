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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Common.CAMLHelper.CAML;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.Wrapper.Common;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Cloud.Sdk.Data.Amls.Ics.Category;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Util.AI.Text.Extractor;
using Path = System.IO.Path;
using Task = System.Threading.Tasks.Task;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Common.Aos;
using RAGoogle.Services;
using RAGoogle.Extension;
using Newtonsoft.Json;
using RAGoogle.Models;
using System.Net;
using RAGoogle.GoogleObjDiscover;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Service.Services.Explorer;

namespace AvePoint.RA.ArtificialIntelligence.MachineLearningTraining
{
    public class MLProcessor
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(MLProcessor));
        private static readonly RMRetryer retryer = RMRetryerBuilder.CreateBuilder().Build();
        #region Interface
        private readonly IJobMonitorDao jmDao;
        private readonly IRMMLTermDao trainingTermDao;
        private readonly IRMMLTrainingModelDao trainingModelDao;
        private readonly ITermDao termDao;
        private readonly IExplorerDao explorerDao;
        private readonly ISharePointSettingDao sharePointSettingDao;
        private readonly IOneDriveSettingDao oneDriveSettingDao;
        private readonly IRMKeyValueDao keyValueDao;
        private readonly IRMRemoteGoogleNodeService remoteGoogleNodeService;
        private readonly IJobInfoUpdater jobInfoUpdater;
        private readonly IRMReportManager reportManager;
        private readonly IRMRemoteNodeDao remoteNodeDao;
        #endregion

        private const string TrainingFolderName = "Training";
        private const string PredictionFolderName = "Prediction";
        private readonly string currentJobId;
        private HashSet<int> checkDuplicateFile = new();
        private Dictionary<Guid, RMTerm> termCacheDic;
        private Dictionary<string, IAveSite?> sitesCache = new();

        private bool isCosmosBulkOperationEnabled = false; //是否开启了批量插入数据到cosmos db
        private int bulkSize = 0;
        private static readonly int itemsPerTask = 5;

        private int filesCount4Term = 0;
        private int filesCount4PredictionFile = 0;

        private readonly object updateFileCountLocker = new object();
        private readonly object duplicateCheckLocker = new object();
        /// <summary>
        /// Will be set value as RecordsConstants.TrainingFile_MaximumNumberPerTerm + Reclassify files count.
        /// </summary> <summary>
        /// 
        /// </summary>
        private int thisTermTrainingFileMaximumNumber = 0;

        /// <summary>
        /// Cache for storing initial records during term processing.
        /// Used to collect minimum required records (20) before we decide if a term has enough records for training.
        /// </summary>
        private BlockingCollection<Tuple<Record, string>> initialRecordsCache = new(20);

        /// <summary>
        /// Flag to track if the BlockingCollection has been disposed to prevent race conditions
        /// </summary>
        private volatile bool isRecordsCacheDisposed = false;

        private ReaderWriterLockSlim trainingLock = new ReaderWriterLockSlim();
        private FileStream? trainingFileStream;
        private StreamWriter? trainingWriter;

        private ReaderWriterLockSlim predictionLock = new ReaderWriterLockSlim();
        private StreamWriter? predictionWriter;
        private FileStream? predictionFileStream;

        private Cloud.Sdk.Amls.Ics.AmlsIcsApiClient icsClient;

        // Used for distributing records between training and prediction files
        private int recordDistributionIndex = 0;

        private TrainingScopeOption currentScopeOption = TrainingScopeOption.Auto500Laster;

        private readonly SourceFlag[] flags = new[] { SourceFlag.SharePoint, SourceFlag.OneDrive, SourceFlag.Teams, SourceFlag.Google };

        private MLTrainingScopeManage? trainingScopeManage = null;
        private Record? locationRecord = null;

        public MLProcessor(string jobId)
        {
            currentJobId = jobId;
            jmDao = (IJobMonitorDao)PlatformWindsorManager.GetService(typeof(IJobMonitorDao));
            trainingTermDao = (IRMMLTermDao)PlatformWindsorManager.GetService(typeof(IRMMLTermDao));
            trainingModelDao = (IRMMLTrainingModelDao)PlatformWindsorManager.GetService(typeof(IRMMLTrainingModelDao));
            termDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
            sharePointSettingDao = (ISharePointSettingDao)PlatformWindsorManager.GetService(typeof(ISharePointSettingDao));
            oneDriveSettingDao = (IOneDriveSettingDao)PlatformWindsorManager.GetService(typeof(IOneDriveSettingDao));
            keyValueDao = (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));
            jobInfoUpdater = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));
            remoteGoogleNodeService = (IRMRemoteGoogleNodeService)PlatformWindsorManager.GetService(typeof(IRMRemoteGoogleNodeService));
            remoteNodeDao = (IRMRemoteNodeDao)PlatformWindsorManager.GetService(typeof(IRMRemoteNodeDao));
            reportManager = ReportMangerFactory.Instance.ReportManager;
            explorerDao = new ExplorerDao();

            jobInfoUpdater.UpdateJobState(currentJobId, (int)JobStatus.InProgress);
            reportManager.Increase(1);
            reportManager.StartUpdateJobProgress(60);
            ReportMangerFactory.Instance.Init(currentJobId, JobType.MachineLearningTraining);
            termCacheDic = new Dictionary<Guid, RMTerm>();
            icsClient = AosApiUtility.GetIcsClient(TenantLocalValue.LogonGroupId);
        }
        public void Run()
        {
            try
            {
                ProcessAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                reportManager.SetJobFinished(JobStatus.Failed);
                logger.Error($"Run training files job failed. error: {e}");
            }
        }

        public async Task ProcessAsync()
        {
            //prepare train files
            var trainingModel = trainingModelDao.GetDefaultModel();
            if (trainingModel == null)
            {
                logger.Error("training model is null, in records db.");
                reportManager.SetJobFinished(JobStatus.Failed);
                return;
            }
            var modelId = trainingModel.Id;
            currentScopeOption = trainingModel.TrainingScopeOption;
            if (!await EnsureClientModelStatus(modelId))
            {
                return;
            }

            InitCosmosBulkOperation();

            await ResetTrainingModelAsync(trainingModel);

            List<RMMLTerm> activeTerms = await UpdateActiveMLTermAsync();


            switch (trainingModel.TrainingScopeOption)
            {
                case TrainingScopeOption.Manual:
                    RemoveFileHaveTermNotIncludeTrainingTerm();
                    break;
                case TrainingScopeOption.FromLocation:
                    {
                        RemoveTrainingFileStatus();
                        try
                        {
                            trainingScopeManage = SerializerHelper.DeserializeByDataContractSerializer<MLTrainingScopeManage>(trainingModel.Extension);
                        }
                        catch (Exception e)
                        {
                            logger.Error($"Can not deserialize training scope manage:{e}");
                            trainingScopeManage = null;
                        }
                    }
                    break;
                case TrainingScopeOption.Auto500Laster:
                default:
                    RemoveTrainingFileStatus();
                    break;
            }

            //Path like C:\RECO_Reports\Job Report\Machine Learning\Temple\MALT20250616113941153402
            var tempFolderPath = Path.Combine(JobReportUtility.GetMLTempleFolder("Temple"), currentJobId);

            try
            {
                var processSuccess = await WriteTrainingFilesAsync(activeTerms, tempFolderPath);
                if (!processSuccess)
                {
                    logger.Error($"Job failed, total available term count < 5");
                    await SetJobFailedAndResetTrainingStatusAsync("RM_MachineLearning_InsufficientFiles");
                    return;
                }
            }
            catch (Exception e)
            {
                logger.Error($"WriteTrainingFiles error: {e}");
                await SetJobFailedAndResetTrainingStatusAsync();
                return;
            }

            // #if DEBUG
            //             logger.Warn("reset default proxy for debug mode.");
            //             HttpClient.DefaultProxy = new WebProxy();
            // #endif
            bool retryFailed = await UploadTrainingData(modelId, tempFolderPath);

            if (retryFailed)
            {
                await SetJobFailedAndResetTrainingStatusAsync();
                return;
            }
            try
            {
                //OperationState operationState = OperationState.Running;
                OperationState operationState = await StartModelTraining(trainingModel);
                if (operationState == OperationState.Running)
                {
                    logger.Info("The train is running.");
                }
                else
                {
                    logger.Error($"Train model status is: {operationState}");
                    await SetJobFailedAndResetTrainingStatusAsync();
                    return;
                }
            }
            catch (Exception e)
            {
                logger.Error($"String train model error: {e}");
                await SetJobFailedAndResetTrainingStatusAsync();
                return;
            }
            reportManager.SetJobFinished(JobStatus.Pending);
        }

        private async Task<bool> EnsureClientModelStatus(Guid modelId)
        {
            bool result = true;
            using (var scope = new PerformanceScope("EnsureClientModelStatus", "Get training and publish status.", true))
            {
                //icsClient = AosApiUtility.GetIcsClient(TenantLocalValue.LogonGroupId);
                var trainResultBeforeRun = await icsClient.TrainingService.GetStateAsync(modelId);
                if (trainResultBeforeRun.State == OperationState.Running)
                {
                    logger.Info($"Model is traning, state: {trainResultBeforeRun.State}, Skip this job, because the last training has not been completed.");
                    reportManager.SetJobFinished(JobStatus.Skipped, "RM_MachineLearning_JobSkipWithModelRunning");
                    result = false;
                }

                var deployState = await icsClient.EndpointService.GetDeployStateAsync(modelId);
                if (deployState == OperationState.Running)
                {
                    logger.Info($"Model is publishing, state: {deployState}, Skip this job, because the last training has not been completed.");
                    reportManager.SetJobFinished(JobStatus.Skipped, "RM_MachineLearning_JobSkipWithModelRunning");
                    result = false;
                }
            }
            return result;
        }

        private async Task<OperationState> StartModelTraining(RMMLTrainingModel trainingModel)
        {
            // #if DEBUG
            // return OperationState.Failed;
            // #endif
            
            var modelId = trainingModel.Id;
            //icsClient = AosApiUtility.GetIcsClient(TenantLocalValue.LogonGroupId);
            Stopwatch sw = new();
            sw.Start();
            OperationState operationState = await icsClient.TrainingService.TrainAsync(modelId);
            sw.Stop();
            logger.Info($"Start traning state: {operationState}, take: {sw.Elapsed.TotalMilliseconds}ms");

            trainingModel.TrainStatus = (int)operationState;
            //move to timer update finish time
            //trainingModel.LastTrainedTime = DateTime.UtcNow.Ticks;
            await trainingModelDao.UpdateAsync(trainingModel);
            return operationState;
        }

        private void RemoveFileHaveTermNotIncludeTrainingTerm()
        {
            using(var scope = new PerformanceScope("RemoveFileHaveTermNotIncludeTrainingTerm", "Remove training files have term, what does not include the training term", true))
            {
                var trainingTermIds = trainingTermDao.GetAllMLTermIds();
                trainingTermIds.Add(Guid.Empty);
                if (isCosmosBulkOperationEnabled)
                {
                    CosmosBulkOperator.Instance.Start(bulkSize, UpdateProcessSucceedRecord, UpdateProcessFailedRecord);
                    Tuple<IEnumerable<Record>, string> result = new(new List<Record>(), string.Empty);
                    var deleteLastTimeTrainingFiles = 0;
                    do
                    {
                        result = explorerDao.QueryByPage(r => !trainingTermIds.Contains(r.TrainingTermId), r => r.CollectTime, true, RecordsConstants.ExplorerQueryPageSize, result.Item2);
                        foreach(var item in result.Item1)
                        {
                            item.TrainingScope = (int)MLFileStatus.None;
                            item.TrainingTermId = Guid.Empty;
                            deleteLastTimeTrainingFiles++;
                            CosmosBulkOperator.Instance.Add(item);
                        }
                        logger.Info($"Manual option: Reset training files status count is: {deleteLastTimeTrainingFiles}");
                    } while (!string.IsNullOrEmpty(result.Item2));
                    if (isCosmosBulkOperationEnabled)
                    {
                        CosmosBulkOperator.Instance.Complete();
                        CosmosBulkOperator.Instance.Reset();
                    }
                }
                else
                {
                    var deleteLastTimeTrainingFiles = explorerDao.UpdateAll(r => !trainingTermIds.Contains(r.TrainingTermId), r => { r.TrainingScope = (int)MLFileStatus.None; r.TrainingTermId = Guid.Empty; });
                    logger.Info($"Manual option: Reset training files status count is: {deleteLastTimeTrainingFiles}");
                }
            }
        }

        private void RemoveTrainingFileStatus()
        {
            //reset all training files
            //TODO Cyrus reclassify term need special flag, we don't reset this falg record

            using (var scope = new PerformanceScope("RemoveTrainingFileStatus", "Remove training files.", true))
            {
                var trainingStatus = new int[] { (int)MLFileStatus.NotTrain, (int)MLFileStatus.Training, (int)MLFileStatus.Trained };
                if (isCosmosBulkOperationEnabled)
                {
                    CosmosBulkOperator.Instance.Start(bulkSize, UpdateProcessSucceedRecord, UpdateProcessFailedRecord);
                    Tuple<IEnumerable<Record>, string> result = new(new List<Record>(), string.Empty);
                    var deleteLastTimeTrainingFiles = 0;
                    do
                    {
                        result = explorerDao.QueryByPage(r => Enumerable.Contains(trainingStatus, r.TrainingScope), r => r.CollectTime, true, RecordsConstants.ExplorerQueryPageSize, result.Item2);
                        foreach (var item in result.Item1)
                        {
                            item.TrainingScope = (int)MLFileStatus.None;
                            item.TrainingTermId = Guid.Empty;
                            deleteLastTimeTrainingFiles++;
                            CosmosBulkOperator.Instance.Add(item);
                        }
                    } while (!string.IsNullOrEmpty(result.Item2));
                    logger.Info($"Reset training files status count is: {deleteLastTimeTrainingFiles}");
                    if (isCosmosBulkOperationEnabled)
                    {
                        CosmosBulkOperator.Instance.Complete();
                        CosmosBulkOperator.Instance.Reset();
                    }
                }
                else
                {
                    var deleteLastTimeTrainingFiles = explorerDao.UpdateAll(r => Enumerable.Contains(trainingStatus, r.TrainingScope), r => { r.TrainingScope = (int)MLFileStatus.None; r.TrainingTermId = Guid.Empty; });
                    logger.Info($"Reset training files status count is: {deleteLastTimeTrainingFiles}");
                } 
            }
        }

        private async Task<List<RMMLTerm>> UpdateActiveMLTermAsync()
        {
            List<RMMLTerm> activeTerms;
            var activeStatus = new int[] { (int)MLTermStatus.NotTrain, (int)MLTermStatus.Training, (int)MLTermStatus.Trained };
            var allAITerms = await trainingTermDao.FindListAsync(t => Enumerable.Contains(activeStatus, t.Status));
            allAITerms.ForEach(t => { t.Status = (int)MLTermStatus.NotTrain; t.TrainingScopeCount = 0; t.Accuracy = 0; });
            trainingTermDao.BatchUpdate(allAITerms);

            // var exceptDefaultTermIds = sharePointSettingDao.FindList(o => !o.IsRemoved && o.EnableRecordManagement == 1 && o.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm).Select(o => o.DefaultTermId).ToList();
            // exceptDefaultTermIds.AddRange(oneDriveSettingDao.FindList(o => !o.IsRemoved && o.EnableRecordManagement == 1 && o.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm).Select(o => o.DefaultTermId).ToList());
            // exceptDefaultTermIds = exceptDefaultTermIds.Distinct().ToList();
            // activeTerms = allAITerms.Where(term => !exceptDefaultTermIds.Contains(term.Id)).ToList();
            activeTerms = allAITerms;
            termCacheDic = termDao.GetRMTermsByTermIds(activeTerms.Select(t => t.Id).ToList()).ToDictionary(t => t.UniqueId);
            return activeTerms;
        }

        private Task<bool> ResetTrainingModelAsync(RMMLTrainingModel trainingModel)
        {
            trainingModel.CurrentTrainingJobId = currentJobId;
            trainingModel.TrainStatus = (int)OperationState.None;
            trainingModel.PublishStatus = (int)OperationState.None;
            trainingModel.Accuracy = 0;
            return trainingModelDao.UpdateAsync(trainingModel);
        }

        private async Task SetJobFailedAndResetTrainingStatusAsync(string comment = "")
        {
            var resetTrainingTermAndFileStatus = async () =>
            {
                //if job failed Training --> NotTrain
                List<RMMLTerm> trainingTerms = await trainingTermDao.FindListAsync(t => t.Status == (int)MLTermStatus.Training);
                trainingTerms.ForEach(t => { t.Status = (int)MLTermStatus.NotTrain; /*t.TrainingScopeCount = 0;*/ });
                trainingTermDao.BatchUpdate(trainingTerms);

                using (var scope = new PerformanceScope("SetJobFailedAndResetTrainingStatus", addToStatistics: true))
                {
                    if (isCosmosBulkOperationEnabled)
                    {
                        Tuple<IEnumerable<Record>, string> result = new(new List<Record>(), string.Empty);
                        CosmosBulkOperator.Instance.Start(bulkSize, UpdateProcessSucceedRecord, UpdateProcessFailedRecord);
                        var resetTrainingFileCount = 0;
                        do
                        {
                            result = explorerDao.QueryByPage(r => r.TrainingScope == (int)MLFileStatus.Training, r => r.CollectTime, true, RecordsConstants.ExplorerQueryPageSize, result.Item2);
                            foreach (var item in result.Item1)
                            {
                                item.TrainingScope = (int)MLFileStatus.NotTrain;
                                resetTrainingFileCount++;
                                CosmosBulkOperator.Instance.Add(item);
                            }
                        } while (!string.IsNullOrEmpty(result.Item2));
                        logger.Info($"Reset training files status count is: {resetTrainingFileCount}");
                        if (isCosmosBulkOperationEnabled)
                        {
                            CosmosBulkOperator.Instance.Complete();
                            CosmosBulkOperator.Instance.Reset();
                        }
                    }
                    else
                    {
                        explorerDao.UpdateAll(r => r.TrainingScope == (int)MLFileStatus.Training,
                        r =>
                        {
                            r.TrainingScope = (int)MLFileStatus.NotTrain;
                        });
                    }
                }
            };

            reportManager.SetJobFinished(JobStatus.Failed, comment);
            await resetTrainingTermAndFileStatus();
        }

        private async Task<bool> WriteTrainingFilesAsync(List<RMMLTerm> activeTerms, string tempFolderPath)
        {
            var processSuccessTermCount = 0;
            try
            {
                Extractor extractor = new();
                var supportTypes = extractor.GetAllSupportTypes();
                

                if (isCosmosBulkOperationEnabled)
                {
                    CosmosBulkOperator.Instance.Start(bulkSize, UpdateProcessSucceedRecordWithDetails, UpdateProcessFailedRecordWithDetails);
                }
                foreach (var term in activeTerms)
                {
                    Tuple<IEnumerable<Record>, string> result = new(new List<Record>(), string.Empty);
                    
                    // Reset distribution index for each term
                    recordDistributionIndex = 0;

                    logger.Info($"process term: {term.Id}, pageIndex:{result.Item2}");
                    try
                    {
                        if (!termCacheDic.TryGetValue(term.Id, out var cacheTerm))
                        {
                            //RECO-16317 skip term management removed term
                            logger.Warn($"skip term management delete term: {term.Id}");
                            continue;
                        }
                        term.Status = (int)MLTermStatus.Training;
                        await trainingTermDao.UpdateAsync(term);
                        filesCount4Term = 0;
                        filesCount4PredictionFile = 0;
                        thisTermTrainingFileMaximumNumber = RecordsConstants.TrainingFile_MaximumNumberPerTerm;
                        InitializeFileStreamsForTerm(tempFolderPath, term);

                        var additionalTrainScopeAdded = false;
                        do
                        {
                            var (additionalTrainScopeAddedNow, reclassifyFiles) = RetrieveReclassifyRecords(supportTypes, term, additionalTrainScopeAdded);
                            additionalTrainScopeAdded = additionalTrainScopeAddedNow;
                            var trainingFiles = new List<Record>(reclassifyFiles);

                            ExplorerQueryV2Dto GetTrainingQuery(string[] supportTypes, Tuple<IEnumerable<Record>, string> result, RMMLTerm term)
                            {
                                switch (currentScopeOption)
                                {
                                    case TrainingScopeOption.Manual:
                                        return BuildManualExplorerQueryDto(supportTypes, result, term);
                                    case TrainingScopeOption.FromLocation:
                                        return BuildLocationExplorerQueryDto(supportTypes, result, term);
                                    case TrainingScopeOption.Auto500Laster:
                                    default:
                                        return BuildLatestExplorerQueryDto(supportTypes, result, term);
                                }
                            }

                            result = explorerDao.SearchRecordsV2(GetTrainingQuery(supportTypes, result, term));

                            trainingFiles.AddRange(result.Item1);
                            logger.Info($"Get term: {term.Id}, files count: {result.Item1.Count()}");

                            if (trainingFiles.Count < RecordsConstants.TrainingFile_MinimumNumberPerTerm)
                            {
                                logger.Warn($"This term[{term.Id}] files count is {trainingFiles.Count()} < 20");

                                //?duplicate update whit [After parse content]
                                term.Status = (int)MLTermStatus.NotTrain;
                                await trainingTermDao.UpdateAsync(term);
                            }
                            // Process M365 Drive files
                            List<Record> m365Files = [.. trainingFiles.Where(r => r.SourceFlag != (int)SourceFlag.Google)];
                            await ProcessM365TrainingFilesForTermAsync(term, m365Files);

                            // Process Google Drive files
                            List<Record> googleDriveFiles = [.. trainingFiles.Where(r => r.SourceFlag == (int)SourceFlag.Google)];
                            await ProcessGoogleTrainingFilesForTermAsync(term, googleDriveFiles);

                        } while (!string.IsNullOrEmpty(result.Item2) && filesCount4Term < thisTermTrainingFileMaximumNumber);
                        if (filesCount4Term >= RecordsConstants.TrainingFile_MinimumNumberPerTerm)
                        {
                            term.Status = (int)MLTermStatus.Training;
                            await trainingTermDao.UpdateAsync(term);
                            processSuccessTermCount++;
                            logger.Info($"This term[{term.Id}] prediction / all files count is: {filesCount4PredictionFile} / {filesCount4Term}");
                        }
                        else
                        {
                            // Not enough files for training, process any cached records
                            await ProcessCachedRecordsAsync(term);

                            logger.Info($"This term[{term.Id}] all files count is: {filesCount4Term}, prediction file is: {filesCount4PredictionFile}[After parse content]");
                            term.TrainingScopeCount = filesCount4Term;
                            term.Status = (int)MLTermStatus.NotTrain;
                            await trainingTermDao.UpdateAsync(term);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"Process term error:{e}");
                    }
                    finally
                    {
                        trainingWriter?.Dispose();
                        trainingFileStream?.Dispose();

                        predictionWriter?.Dispose();
                        predictionFileStream?.Dispose();
                    }
                }
            }
            finally
            {
                isRecordsCacheDisposed = true;
                initialRecordsCache.Dispose();
                if (isCosmosBulkOperationEnabled)
                {
                    CosmosBulkOperator.Instance.Complete();
                    CosmosBulkOperator.Instance.Reset();
                }
            }
            logger.Info($"Process available term count is: {processSuccessTermCount}");
            return processSuccessTermCount >= RecordsConstants.TrainingTerm_MinimumNumber;
        }

        private void InitializeFileStreamsForTerm(string tempFolderPath, RMMLTerm term)
        {
            var fileName4Term = $"{term.Id.ToString().Replace("-", "")}.tsv";
            var tempTrainingFilePath = GCommon.Utility.SecurityUtils.SafeCombinePath(
                tempFolderPath, TrainingFolderName, fileName4Term);
            Directory.CreateDirectory(Path.GetDirectoryName(tempTrainingFilePath) ?? "");
            trainingFileStream = System.IO.File.OpenWrite(tempTrainingFilePath);
            trainingWriter = new StreamWriter(trainingFileStream);

            var tempPredictionFilePath = GCommon.Utility.SecurityUtils.SafeCombinePath(
                tempFolderPath, PredictionFolderName, fileName4Term);
            Directory.CreateDirectory(Path.GetDirectoryName(tempPredictionFilePath) ?? "");
            predictionFileStream = System.IO.File.OpenWrite(tempPredictionFilePath);
            predictionWriter = new StreamWriter(predictionFileStream);
        }

        private (bool, List<Record>) RetrieveReclassifyRecords(string[] supportTypes, RMMLTerm term, bool additionalTrainScopeAdded)
        {
            var reclassifyRecords = new List<Record>();
            if (!additionalTrainScopeAdded)
            {
                var additionalScope = explorerDao.QueryByPage(r => r.TermId == term.Id
                && Enumerable.Contains(flags, (SourceFlag)r.SourceFlag)
                //&& r.RecordStatus == (int)RMRecordStatus.Active
                && r.TrainingAddType == (int)TrainingAddType.Reclassify
                && (r.NodeType == (int)NodeLevel.Item || r.NodeType == (int)RMNodeLevel.GoogleFile) && Enumerable.Contains(supportTypes, r.ExtensionForFile)
                , r => r.PredictTime, true, RecordsConstants.TrainingFile_MaximumNumberPerTerm4Reclassify);
                //Get the latest 500 traing scopes added by reclassify 
                reclassifyRecords.AddRange(additionalScope.Item1);
                additionalTrainScopeAdded = true;
                thisTermTrainingFileMaximumNumber = thisTermTrainingFileMaximumNumber + additionalScope.Item1.Count();
                logger.Info($"Get term: {term.Id}, reclassify count: {additionalScope.Item1.Count()}");
            }
            return (additionalTrainScopeAdded, reclassifyRecords);
        }

        private ExplorerQueryV2Dto BuildLocationExplorerQueryDto(string[] supportTypes, Tuple<IEnumerable<Record>, string> result, RMMLTerm term)
        {
            if(trainingScopeManage == null)
            {
                throw new Exception("Can not get the location to get files to training");
            }
            switch (trainingScopeManage.SourceFlag)
            {
                case MTSSourceFlag.Google:
                    return BuildLocationGoogleQueryDto(supportTypes, result, term);
                case MTSSourceFlag.SPO:
                default:
                    return BuildLocationSharePointQueryDto(supportTypes, result, term);
            }
        }

        private ExplorerQueryV2Dto BuildLocationGoogleQueryDto(string[] supportTypes, Tuple<IEnumerable<Record>, string> result, RMMLTerm term)
        {
            var googleDrive = remoteNodeDao.GetGoogleDriveByName(trainingScopeManage?.Location);
            return new ExplorerQueryV2Dto()
            {
                PagingInfo = new ExplorerPagingInfo()
                {
                    PageIndex = result.Item2,
                    PageSize = RecordsConstants.ExplorerQueryPageSize,
                },
                QueryOption = new ExplorerQueryOptionV2()
                {
                    FilterOption = new ExplorerFilterOptionV2()
                    {
                        TermIds = new() { term.Id },
                        SourceFlags = [.. flags],
                        Status = new() { RMRecordStatus.Active },
                        NodeTypes = new() { RMNodeLevel.Item, RMNodeLevel.GoogleFile },
                        FileExtensions = supportTypes.ToList(),
                        ContainerIds = new List<string> { googleDrive.ParentId},
                        ScopeId = googleDrive.Id,
                    },
                    OrderColumn = new ExplorerQueryOrderColumn
                    {
                        Column = new ExplorerQueryColumn { Name = CosmosConst.C_CollectionDate },
                        OrderAsc = false
                    }
                }
            };
        }


        private ExplorerQueryV2Dto BuildLocationSharePointQueryDto(string[] supportTypes, Tuple<IEnumerable<Record>, string> result, RMMLTerm term)
        {
            var siteCollection = remoteNodeDao.GetRemoteSiteCollectionByListUrl(trainingScopeManage?.Location);
            if(locationRecord == null)
            {
                locationRecord = explorerDao.GetRecordsByContainerAndNodeType(new Guid(siteCollection.ObjectId), siteCollection.parentId, new List<int> { (int)AvePoint.RA.SharePoint.ArchiverCommon.NodeType.Web, (int)AvePoint.RA.SharePoint.ArchiverCommon.NodeType.List }, trainingScopeManage?.Location, string.Empty, 1)?.Item1.FirstOrDefault() ?? null;
                if (locationRecord == null) throw new Exception("Can not get record of location in DB");
            }
            var query = new ExplorerQueryV2Dto()
            {
                PagingInfo = new ExplorerPagingInfo()
                {
                    PageIndex = result.Item2,
                    PageSize = RecordsConstants.ExplorerQueryPageSizeForTraining,
                },
                QueryOption = new ExplorerQueryOptionV2()
                {
                    FilterOption = new ExplorerFilterOptionV2()
                    {
                        TermIds = new() { term.Id },
                        SourceFlags = [.. flags],
                        Status = new() { RMRecordStatus.Active },
                        NodeTypes = new() { RMNodeLevel.Item, RMNodeLevel.GoogleFile },
                        FileExtensions = supportTypes.ToList(),
                    },
                    OrderColumn = new ExplorerQueryOrderColumn
                    {
                        Column = new ExplorerQueryColumn { Name = CosmosConst.C_CollectionDate },
                        OrderAsc = false
                    }
                }
            };
            switch (locationRecord.NodeType) 
            {
                case (int)RMNodeLevel.List:
                    query.QueryOption.FilterOption.ListId = locationRecord.ListId.ToString();
                    break;
                case (int)RMNodeLevel.Site:
                    {
                        var webIds = new List<string> { locationRecord.WebId.ToString() };
                        var subSiteUnderCurrentSite = explorerDao.QueryAll(_ => _.ContainerId == locationRecord.ContainerId && _.ScopeId == locationRecord.ScopeId && _.DirPath.StartsWith(locationRecord.DirPath + "/"));
                        webIds.AddRange(subSiteUnderCurrentSite?.Select(_ => _.WebId.ToString()).ToList() ?? []);
                        query.QueryOption.FilterOption.WebIds = webIds;
                    }
                    break;
            }
            return query;
        }

        private ExplorerQueryV2Dto BuildManualExplorerQueryDto(string[] supportTypes, Tuple<IEnumerable<Record>, string> result, RMMLTerm term)
        {
            var queryDto = new ExplorerQueryV2Dto()
            {
                PagingInfo = new ExplorerPagingInfo()
                {
                    PageSize = RecordsConstants.ExplorerQueryPageSizeForTraining,
                    PageIndex = result.Item2
                },
                QueryOption = new ExplorerQueryOptionV2()
                {
                    FilterOption = new ExplorerFilterOptionV2()
                    {
                        TermIds = new() { term.Id },
                        SourceFlags = [.. flags],
                        Status = new() { RMRecordStatus.Active },
                        NodeTypes = new() { RMNodeLevel.Item, RMNodeLevel.GoogleFile },
                        FileExtensions = supportTypes.ToList(),
                        TrainingAddTypes = new() { TrainingAddType.None, TrainingAddType.AddManually },
                        TrainScopeStatus = new() { MLFileStatus.NotTrain, MLFileStatus.Training, MLFileStatus.Trained }
                    },
                    OrderColumn = new ExplorerQueryOrderColumn
                    {
                        Column = new ExplorerQueryColumn { Name = CosmosConst.C_CollectionDate },
                        OrderAsc = false
                    }
                }
            };
            logger.Info($"Use manual method, dto is:{JsonConvert.SerializeObject(queryDto)}");
            return queryDto;
        }

        private ExplorerQueryV2Dto BuildLatestExplorerQueryDto(string[] supportTypes, Tuple<IEnumerable<Record>, string> result, RMMLTerm term)
        {
            var queryDto =  new ExplorerQueryV2Dto()
            {
                PagingInfo = new ExplorerPagingInfo()
                {
                    PageSize = RecordsConstants.ExplorerQueryPageSizeForTraining,
                    PageIndex = result.Item2
                },
                QueryOption = new ExplorerQueryOptionV2()
                {
                    FilterOption = new ExplorerFilterOptionV2()
                    {
                        TermIds = new() { term.Id },
                        SourceFlags = [.. flags],
                        Status = new() { RMRecordStatus.Active },
                        NodeTypes = new() { RMNodeLevel.Item, RMNodeLevel.GoogleFile },
                        FileExtensions = supportTypes.ToList(),
                        TrainingAddTypes = new() { TrainingAddType.None }
                    },
                    OrderColumn = new ExplorerQueryOrderColumn
                    {
                        Column = new ExplorerQueryColumn { Name = CosmosConst.C_CollectionDate },
                        OrderAsc = false
                    }
                }
            };
            logger.Info($"Use automatic method, dto is:{JsonConvert.SerializeObject(queryDto)}");
            return queryDto;
        }

        private async Task ProcessM365TrainingFilesForTermAsync(RMMLTerm term, List<Record> m365Files)
        {
            Dictionary<string, List<Record>> siteObjs = m365Files.GroupBy(r => r.AveSiteId).ToDictionary(g => g.Key, p => p.ToList());
            foreach (var aveSiteId in siteObjs.Keys)
            {
                IAveSite? aveSite = await GetAveSiteAsync(aveSiteId);
                if (aveSite == null)
                {
                    continue;
                }

                Dictionary<Guid, List<Record>> webObjs = siteObjs[aveSiteId].GroupBy(r => r.WebId).ToDictionary(g => g.Key, p => p.ToList());
                await ProcessWebsForTermAsync(term, aveSite, webObjs);
            }
        }

        private async Task ProcessWebsForTermAsync(RMMLTerm term, IAveSite aveSite, Dictionary<Guid, List<Record>> webObjs)
        {
            IAveWeb? web = null;
            foreach (var webId in webObjs.Keys)
            {
                if (web == null || !web.ID.Equals(webId))
                {
                    web = aveSite.OpenWeb(webId);
                    logger.Info("Process change web {0}", web.Url);
                }
                var listNodes = webObjs[webId].GroupBy(t => t.ListId).ToDictionary(g => g.Key, p => p.ToList());
                await ProcessListsForWebAsync(term, web, listNodes);
            }
        }

        /// <summary>
        /// Process all lists from a web for a specific term
        /// </summary>
        private async Task ProcessListsForWebAsync(RMMLTerm term, IAveWeb web, Dictionary<Guid, List<Record>> listNodes)
        {
            IAveList? list = null;
            foreach (var listId in listNodes.Keys)
            {
                try
                {
                    if (list == null || !list.ID.Equals(listId))
                    {
                        list = web.GetList(listId);
                        logger.Info("Process change list {0}, {1}", list.RootFolder.Url, list.ID);
                    }
                    var records = listNodes[listId];
                    await ProcessListItemsAsync(term, list, records);
                }
                catch (Exception le)
                {
                    logger.Warn("Process list error {0}:{1}", listId, le.ToString());
                }
            }
        }

        /// <summary>
        /// Process items from a list for a specific term
        /// </summary>
        private async Task ProcessListItemsAsync(RMMLTerm term, IAveList list, List<Record> records)
        {
            var itemIntIds = records.Where(o => o.ItemRowId != 0).Select(i => i.ItemRowId).ToList();
            for (int i = 0; i < itemIntIds.Count; i += 2000)
            {
                var rowIds = itemIntIds.Skip(i).Take(2000).ToList();
                IEnumerable<IAveListItem> items = GetItemsByRowIds(list, rowIds);
                logger.Info($"This term[{term.Id}], Process List Item: {list.RootFolder.Url}, row id count: {rowIds.Count}, result items count:{items.Count()}");
                int existingItemsPerTask = items.Count() / 4;
                CancellationTokenSource? cts = null;

                if (items.Count() > itemsPerTask)
                {
                    cts = new CancellationTokenSource();
                    //cts.CancelAfter(TimeSpan.FromHours(1));

                    // Use Task.Run to wrap AveTenantTasks.RunParallel and make it awaitable
                    await Task.Run(() =>
                    {
                        AveTenantTasks.RunParallel(items, existingItemsPerTask, cts, async changedItem =>
                        {
                            await RealProcessTrainingFileAsync(term, changedItem, records, cts);
                        });
                    });
                }
                else
                {
                    foreach (var changedItem in items)
                    {
                        await RealProcessTrainingFileAsync(term, changedItem, records);
                    }
                }
            }
        }


        public async Task ProcessGoogleTrainingFilesForTermAsync(RMMLTerm term, List<Record> trainingFiles)
        {
            var recordsGroupByScope = trainingFiles.GroupBy(r => r.ScopeId);
            logger.Info($"[Change Label] Start to apply label on {trainingFiles.Count} records, group count:{recordsGroupByScope.Count()}.");
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 5
            };

            var recordsInTheSameDrive = new List<Record>();

            foreach (var recordList in recordsGroupByScope)
            {
                try
                {
                    var scopeId = recordList.Key;
                    recordsInTheSameDrive = recordList.ToList();
                    var treeNode = await remoteGoogleNodeService.GetRemoteNodeByDriveIdAsync(scopeId.ToString());
                    var tenantId = treeNode.GoogleTenantId;
                    var appProfile = RMAosApiClient.GetGoogleAppProfile(TenantLocalValue.LogonGroupId, tenantId);
                    var driveId = treeNode.Level == (int)NodeLevel.GoogleSharedDrive ? treeNode.ObjectId : treeNode.DisplayName;
                    RMGoogleDiscoverBase baseService = new(null);
                    baseService.Init(appProfile);
                    var googleDriveService = await baseService.GetDriveService(driveId);
                    await Parallel.ForEachAsync(recordsInTheSameDrive, parallelOptions, async (record, _) =>
                    {
                        await RealProcessGoogleTrainingFileAsync(term, googleDriveService, record);
                    });
                }
                catch (Exception ex)
                {
                    logger.Warn($"[Change Label] Skipped applying label on scope {recordList.Key} for record  because {ex.Message} is null.");
                }
            }
        }
        private async Task RealProcessGoogleTrainingFileAsync(RMMLTerm term, GoogleDriveService googleDriveService, Record record, CancellationTokenSource? cts = null)
        {
            try
            {
                logger.Info($"Process Item:{record.NodeId}, [ThreadId:{Environment.CurrentManagedThreadId}]");

                var dataString = await TryGetGoogleFileContent(term, googleDriveService, record);
                if (string.IsNullOrEmpty(dataString))
                {
                    logger.Warn($"Data string is null for record id: {record.Id}");
                    return;
                }
                if (string.IsNullOrEmpty(dataString))
                {
                    logger.Warn($"Data string is null.");
                    return;
                }

                // Check if we've reached the maximum number of files for this term
                lock (updateFileCountLocker)
                {
                    if (filesCount4Term >= thisTermTrainingFileMaximumNumber)
                    {
                        logger.Info($"This term[{term.Id}], Reached maximum quantity:[{thisTermTrainingFileMaximumNumber}]");
                        return;
                    }
                    filesCount4Term++;
                }

                // If we have fewer than the minimum required files, cache this record for later processing
                if (filesCount4Term < RecordsConstants.TrainingFile_MinimumNumberPerTerm)
                {
                    // Add to initial cache to process later if we reach the minimum threshold
                    logger.Info($"Adding record to initial cache: {record.Id}");
                    if (!isRecordsCacheDisposed)
                    {
                        try
                        {
                            bool added = initialRecordsCache.TryAdd(new Tuple<Record, string>(record, dataString));
                            if (!added)
                            {
                                logger.Warn($"Failed to add to initial cache, record id: {record.Id}");
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            logger.Warn($"BlockingCollection was disposed while trying to add record: {record.Id}");
                            isRecordsCacheDisposed = true;
                        }
                    }
                    else
                    {
                        logger.Warn($"BlockingCollection already disposed, cannot add record: {record.Id}");
                    }
                    return;
                }
                // If we've reached the minimum required files, process all cached records
                if (filesCount4Term == RecordsConstants.TrainingFile_MinimumNumberPerTerm)
                {
                    logger.Info($"Reached minimum threshold of {RecordsConstants.TrainingFile_MinimumNumberPerTerm} files, processing cached records");

                    // Process all records in the cache
                    if (!isRecordsCacheDisposed)
                    {
                        try
                        {
                            while (initialRecordsCache.TryTake(out Tuple<Record, string>? cachedItem))
                            {
                                logger.Info($"Processing cached record {cachedItem.Item1.Id}");
                                await WriteRecordToTrainingOrPredictionFile(term, cachedItem.Item1, cachedItem.Item2);
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            logger.Warn("BlockingCollection was disposed while processing cached records");
                            isRecordsCacheDisposed = true;
                        }
                    }
                }

                // We have enough files, so write this one to the appropriate file based on distribution rules
                await WriteRecordToTrainingOrPredictionFile(term, record, dataString);
            }
            catch (Exception e)
            {
                logger.Error($"RealProcessTrainingFile item:{record.NodeId} error:{e}");
            }
        }
        private async Task<string> TryGetGoogleFileContent(RMMLTerm term, GoogleDriveService googleDriveService, Record record)
        {
            var dataString = string.Empty;
            try
            {
                logger.Info($"Start to download record, id {record.Id}.");
                if (record.NodeType != (int)RMNodeLevel.GoogleFile) return "";
                IExtract extractor = new Extractor();
                string? content = "";
                Stream? stream = new MemoryStream();
                GoogleItemMetaInfo? metaInfo = JsonConvert.DeserializeObject<GoogleItemMetaInfo>(record.MetaInfo);
                if (metaInfo == null || string.IsNullOrEmpty(metaInfo.DocId))
                {
                    logger.Warn($"This term[{term.Id}], Get file meta info error, record id:{record.Id}");
                    return "";
                }
                var extension = record.ExtensionForFile;
                var tempFile = await googleDriveService.GetFileByIdAsync(metaInfo.DocId);
                if (tempFile == null)
                {
                    logger.Warn($"This term[{term.Id}], Get file by id error, record id:{record.Id}");
                    return "";
                }
                if (tempFile.MimeType
                                is "application/vnd.google-apps.document"
                                or "application/vnd.google-apps.spreadsheet"
                                or "application/vnd.google-apps.presentation")
                {
                    extension = await googleDriveService.DownloadGoogleFileToMemoryStreamAsync(metaInfo.DocId, stream, tempFile.MimeType);
                }
                else
                {
                    await googleDriveService.DownloadFileToStreamAsync(metaInfo.DocId, stream);
                }
                logger.Info($"Record {record.Id} downloaded");

                if (stream == null)
                {
                    logger.Warn($"This term[{term.Id}], Get file content error, record id:{record.Id}");
                    return "";
                }
                using (stream)
                {
                    try
                    {
                        using (new PerformanceScope("ProcessTrainingFile.TryGetFileContent", "extract one item content.", true))
                        {
                            content = AveTenantTasks.ExecuteActionHaveTimeOut<string>(() =>
                            {
                                logger.Debug($"Before extract file: {record.Id}, percess memory {ProcessUtil.GetProcessMemoryMB()}MB");
                                //Thread.Sleep(TimeSpan.FromMinutes(2));
                                var tempContent = extractor.ExtractAsync(stream, extension, new ExtractOption() { MaxCharsCountPerFile = 1024 * 3 * 10 }).GetAwaiter().GetResult();
                                logger.Info($"File content length {tempContent.Length}, record id: {record.Id}");
                                logger.Debug($"After extract file: {record.Id}, percess memory {ProcessUtil.GetProcessMemoryMB()}MB");
                                return tempContent;
                            }, 5);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        record.TrainingParseTimeoutFile = true;
                        if (isCosmosBulkOperationEnabled)
                        {
                            CosmosBulkOperator.Instance.Add(record);
                        }
                        else
                        {
                            explorerDao.Upsert(record);
                        }
                        throw;
                    }

                    var contentHashCode = content != null ? content.GetHashCode() : 0;
                    if (checkDuplicateFile.Contains(contentHashCode))
                    {
                        logger.Warn($"This term[{term.Id}], File content duplicate skip this, record id: {record.Id}");
                        return "";
                    }
                    else
                    {
                        lock (duplicateCheckLocker)
                        {
                            if (checkDuplicateFile.Contains(contentHashCode))
                            {
                                logger.Warn($"This term[{term.Id}], File content duplicate skip this, record id: {record.Id}");
                                if (currentScopeOption == TrainingScopeOption.Manual)
                                {
                                    record.TrainingScope = (int)MLFileStatus.None;
                                    record.TrainingTermId = Guid.Empty;

                                    if (isCosmosBulkOperationEnabled)
                                    {
                                        CosmosBulkOperator.Instance.Add(record);
                                    }
                                    else
                                    {
                                        explorerDao.Upsert(record);
                                    }
                                }
                                return "";
                            }
                            else
                            {
                                checkDuplicateFile.Add(contentHashCode);
                            }
                        }
                    }
                    dataString = content?.ToSmapleFileLine(term.Id.ToString(), termCacheDic[term.Id].Name);
                    var dataColumnsCount = dataString?.Split("\t")?.Length;
                    if (dataColumnsCount != 3)
                    {
                        logger.Warn($"This term[{term.Id}], Get file style error, record id:{record.Id}, data columns count is: {dataColumnsCount}");
                        return "";
                    }
                    record.FullPath = record.DirPath;
                }
            }

            catch (Exception e)
            {
                logger.Error($"This term[{term.Id}], Get file content error, record id:{record.Id}, error:{e}");
                if (e is OperationCanceledException)
                {
                    record.TrainingParseTimeoutFile = true;
                    if (isCosmosBulkOperationEnabled)
                    {
                        CosmosBulkOperator.Instance.Add(record);
                    }
                    else
                    {
                        explorerDao.Upsert(record);
                    }
                }
            }

            return dataString;
        }

        /// <summary>
        /// Processes the cached records when a term doesn't have enough files for training
        /// </summary>
        private async Task ProcessCachedRecordsAsync(RMMLTerm term)
        {
            if (!isRecordsCacheDisposed)
            {
                try
                {
                    if (initialRecordsCache.Count != 0)
                    {
                        logger.Info($"Processing {initialRecordsCache.Count} cached records for term: {term.Id}");

                        while (initialRecordsCache.TryTake(out Tuple<Record, string>? recordCache))
                        {
                            logger.Info($"Update item from cache: {recordCache.Item1.Id}");
                            var recordFile = recordCache.Item1;
                            recordFile.TrainingScope = (int)MLFileStatus.NotTrain;
                            recordFile.TrainingTermId = term.Id;

                            if (isCosmosBulkOperationEnabled)
                            {
                                CosmosBulkOperator.Instance.Add(recordFile);
                            }
                            else
                            {
                                // Make this operation actually async
                                await Task.Run(() =>
                                {
                                    explorerDao.Upsert(recordFile);
                                    SendDetails(termCacheDic[term.Id].Name, recordFile);
                                });
                            }
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    logger.Warn($"BlockingCollection was disposed while processing cached records for term: {term.Id}");
                    isRecordsCacheDisposed = true;
                }
            }
            else
            {
                logger.Warn($"BlockingCollection already disposed, cannot process cached records for term: {term.Id}");
            }
        }

        private async Task<IAveSite?> GetAveSiteAsync(string aveSiteId)
        {
            try
            {
                using (var scope = new PerformanceScope("GetAveSite", addToStatistics: true))
                {
                    if (!sitesCache.TryGetValue(aveSiteId, out IAveSite? aveSite))
                    {
                        var remoteNode = RABrowserClient.GetRemoteSiteCollectionById(aveSiteId);
                        if (remoteNode != null)
                        {
                            var siteUrl = remoteNode.url;
                            AveBPOSAccountInfo user = await PoolUserUtil.GetBPOSInfoAsync(remoteNode);
                            var aveObjectModelFactory = MultiAppUtil.CreateAveObjectModelFactory(siteUrl, user, AveContextKind.ClientObjectModel);
                            aveSite = aveObjectModelFactory.CreateSite(siteUrl);
                            if (aveSite != null)
                            {
                                logger.Info($"[SitesCache] Set site:[{aveSiteId}] to cache.");
                                sitesCache.Add(aveSiteId, aveSite);
                            }
                        }
                        else
                        {
                            logger.Info($"[SitesCache] Set site:[{aveSiteId}]=null to cache.");
                            sitesCache.Add(aveSiteId, null);
                        }
                    }
                    else
                    {
                        logger.Info($"[SitesCache] Get site:[{aveSiteId}] from site cache.");
                    }

                    return aveSite;
                }
            }
            catch (Exception e)
            {
                logger.Warn($"Get site error:{e}");
                return null;
            }
        }

        private async Task<bool> UploadTrainingData(Guid modelId, string tempFolderPath)
        {
            var retryFailed = false;
            try
            {
                var tempTrainingFolderPath = Path.Combine(tempFolderPath, TrainingFolderName);
                var trainingFiles = Directory.GetFiles(tempTrainingFolderPath);
                var outputFilePath = Path.Combine(tempTrainingFolderPath, "mergeToFolder", RecordsConstants.TrainingData_FileName);
                Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath) ?? "");
                if (trainingFiles != null)
                {
                    logger.Info($"Training files count:{trainingFiles.Count()}");
                    File.Create(outputFilePath).Close();
                    using (StreamWriter outputWriter = new StreamWriter(outputFilePath, true))
                    {
                        foreach (string filePath in trainingFiles)
                        {
                            var termFileLineCount = 0;
                            using (var inputReader = new StreamReader(filePath))
                            {
                                string? line;
                                while ((line = await inputReader.ReadLineAsync()) != null)
                                {
                                    var dataColumnsCount = line?.Split("\t")?.Length;
                                    if (dataColumnsCount != 3)
                                    {
                                        logger.Warn($"This term[{filePath}], Get file style error, line count:{termFileLineCount}, data columns count is: {dataColumnsCount}");
                                        continue;
                                    }
                                    termFileLineCount++;
                                    await outputWriter.WriteLineAsync(line);
                                }
                            }
                            logger.Info($"file:{filePath}, data line count is: {termFileLineCount}");
                        }
                    }
                    var storageInfo = await icsClient.StorageService.GetStorageInfoForWriteAsync(modelId, new Cloud.Sdk.Data.Amls.Ics.Contracts.StorageRequest() { TsvFileName = RecordsConstants.TrainingData_FileName });
                    var envName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
                    var isGCP = ContractConstants.ENVIRONMENT_NAME_GCP.Contains(envName?.ToLower());
                    if (isGCP)
                    {
                        // Upload the file using HttpClient with simple PUT method (proven to work)
                        using var httpClient = new HttpClient();

                        logger.Info($"Starting GCS upload using simple PUT method for file: {outputFilePath}");
                        logger.Debug($"GCS signed URL: {storageInfo.SasToken}");

                        // Upload with retry logic using simple PUT method
                        await retryer.Retry(async () =>
                        {
                            using var fileStream = new FileStream(outputFilePath, FileMode.Open, FileAccess.Read);
                            using var content = new StreamContent(fileStream);

                            // Set content type
                            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                            logger.Info($"Uploading file size: {fileStream.Length} bytes to GCS");

                            var response = await httpClient.PutAsync(storageInfo.SasToken, content);

                            if (!response.IsSuccessStatusCode)
                            {
                                var errorContent = await response.Content.ReadAsStringAsync();
                                logger.Error($"GCS PUT upload failed: {response.StatusCode} - {response.ReasonPhrase}");
                                logger.Error($"Error response: {errorContent}");
                                throw new HttpRequestException($"GCS upload failed: {response.StatusCode} - {errorContent}");
                            }

                            logger.Info($"GCS upload completed successfully: {response.StatusCode}");
                            logger.Info($"HTTP Upload success for file: {outputFilePath}");
                            return Task.CompletedTask;
                        });
                    }
                    else
                    {
                        retryer.Retry(() =>
                        {
                            var blobClient = new BlobClient(new Uri(storageInfo.SasToken));
                            blobClient.Upload(outputFilePath, true);
                            logger.Info($"Upload success");
                        });
                    }
                }

                var tempPredictionFolderPath = Path.Combine(tempFolderPath, PredictionFolderName);
                var predictionFiles = Directory.GetFiles(tempPredictionFolderPath);
                if (predictionFiles != null)
                {
                    logger.Info($"Training files count:{predictionFiles.Count()}");

                    await retryer.Retry(() =>
                    {
                        var tempPredictionZip = tempPredictionFolderPath + ".zip";
                        AvePoint.GCommon.ZipUtil.ZipFolder(tempPredictionFolderPath, tempPredictionFolderPath + ".zip", Encoding.UTF8);
                        var customId = TenantLocalValue.LogonGroupId;
                        var blobName = Path.Combine(customId, JobReportUtility.MachineLearningFolder, currentJobId + ".zip");

                        //var blobName = Path.Combine(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, modelId.ToString(), Path.GetFileName(tempPredictionZip));
                        RAStorageUtil.UploadReportBlob(blobName, tempPredictionZip);

                        logger.Info($"Upload success");
                        return Task.CompletedTask;
                    });
                }
            }
            catch (AggregateException ex)
            {
                retryFailed = true;
                logger.Error($"Upload failed: {ex}");
            }
            catch (Exception ex)
            {
                retryFailed = true;
                logger.Error($"Upload failed: {ex}");
            }

            return retryFailed;
        }

        private async Task RealProcessTrainingFileAsync(RMMLTerm term, IAveListItem item, List<Record> itemNodes, CancellationTokenSource? cts = null)
        {
            try
            {
                lock (updateFileCountLocker)
                {
                    if (filesCount4Term >= thisTermTrainingFileMaximumNumber)
                    {
                        logger.Info($"Skip this term[{term.Id}], Reached maximum quantity:[{thisTermTrainingFileMaximumNumber}]");
                        return;
                    }
                }

                logger.Info($"Process Item:{item.UniqueId}, [ThreadId:{Thread.CurrentThread.ManagedThreadId}]");
                var recordFile = itemNodes.Where(i => i.NodeId == item.UniqueId).FirstOrDefault();
                if (recordFile == null)
                {
                    return;
                }

                if (!TryGetFileContent(term, item, recordFile, out string? dataString))
                {
                    // Processing failed, content extraction failed
                    if (currentScopeOption == TrainingScopeOption.Manual)
                    {
                        recordFile.TrainingScope = (int)MLFileStatus.None;
                        recordFile.TrainingTermId = Guid.Empty;

                        if (isCosmosBulkOperationEnabled)
                        {
                            CosmosBulkOperator.Instance.Add(recordFile);
                        }
                        else
                        {
                            explorerDao.Upsert(recordFile);
                        }
                    }
                    return;
                }

                if (string.IsNullOrEmpty(dataString))
                {
                    logger.Warn($"Data string is null.");
                    return;
                }

                // Check if we've reached the maximum number of files for this term
                lock (updateFileCountLocker)
                {
                    if (filesCount4Term >= thisTermTrainingFileMaximumNumber)
                    {
                        logger.Info($"This term[{term.Id}], Reached maximum quantity:[{thisTermTrainingFileMaximumNumber}]");
                        return;
                    }
                    filesCount4Term++;
                }

                // If we have fewer than the minimum required files, cache this record for later processing
                if (filesCount4Term < RecordsConstants.TrainingFile_MinimumNumberPerTerm)
                {
                    // Add to initial cache to process later if we reach the minimum threshold
                    logger.Info($"Adding record to initial cache: {recordFile.Id}");
                    if (!isRecordsCacheDisposed)
                    {
                        try
                        {
                            bool added = initialRecordsCache.TryAdd(new Tuple<Record, string>(recordFile, dataString));
                            if (!added)
                            {
                                logger.Warn($"Failed to add to initial cache, record id: {recordFile.Id}");
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            logger.Warn($"BlockingCollection was disposed while trying to add record: {recordFile.Id}");
                            isRecordsCacheDisposed = true;
                        }
                    }
                    else
                    {
                        logger.Warn($"BlockingCollection already disposed, cannot add record: {recordFile.Id}");
                    }
                    return;
                }
                // If we've reached the minimum required files, process all cached records
                if (filesCount4Term == RecordsConstants.TrainingFile_MinimumNumberPerTerm)
                {
                    logger.Info($"Reached minimum threshold of {RecordsConstants.TrainingFile_MinimumNumberPerTerm} files, processing cached records");

                    // Process all records in the cache
                    if (!isRecordsCacheDisposed)
                    {
                        try
                        {
                            while (initialRecordsCache.TryTake(out Tuple<Record, string>? cachedItem))
                            {
                                logger.Info($"Processing cached record {cachedItem.Item1.Id}");
                                await WriteRecordToTrainingOrPredictionFile(term, cachedItem.Item1, cachedItem.Item2);
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            logger.Warn("BlockingCollection was disposed while processing cached records");
                            isRecordsCacheDisposed = true;
                        }
                    }
                }
                // We have enough files, so write this one to the appropriate file based on distribution rules
                await WriteRecordToTrainingOrPredictionFile(term, recordFile, dataString);
            }
            catch (Exception e)
            {
                logger.Error($"RealProcessTrainingFile item:{item.UniqueId} error:{e}");
            }
        }

        /// <summary>
        /// Determines whether the record should go to training or prediction file and writes it
        /// </summary>
        private async Task WriteRecordToTrainingOrPredictionFile(RMMLTerm term, Record recordFile, string dataString)
        {
            // Use a separate counter for distribution
            int fileIndex = Interlocked.Increment(ref recordDistributionIndex) - 1; // zero-based
            var predictionMod = new int[] { 0, 1, 2 };
            var mod = fileIndex % 10;

            if (predictionMod.Contains(mod) && filesCount4PredictionFile < RecordsConstants.TrainingFile_MaximumPredictionNumberPerTerm)
            {
                // Write to prediction file
                Interlocked.Add(ref filesCount4PredictionFile, 1);
                predictionLock.EnterWriteLock();
                try
                {
                    predictionWriter?.WriteLine(dataString);
                    predictionWriter?.Flush();
                }
                finally
                {
                    predictionLock.ExitWriteLock();
                }
            }
            else
            {
                // Write to training file
                trainingLock.EnterWriteLock();
                try
                {
                    trainingWriter?.WriteLine(dataString);
                    trainingWriter?.Flush();
                }
                finally
                {
                    trainingLock.ExitWriteLock();
                }
            }

            // Update record status
            recordFile.TrainingScope = (int)MLFileStatus.Training;
            recordFile.TrainingTermId = term.Id;

            if (isCosmosBulkOperationEnabled)
            {
                CosmosBulkOperator.Instance.Add(recordFile);
            }
            else
            {
                explorerDao.Upsert(recordFile);
                SendDetails(termCacheDic[term.Id].Name, recordFile);
            }

            // Update term statistics
            term.TrainingScopeCount = filesCount4Term;
            await trainingTermDao.UpdateAsync(term);
        }

        private bool TryGetFileContent(RMMLTerm term, IAveListItem item, Record recordFile, out string? dataString)
        {
            using (var scope = new PerformanceScope("ProcessTrainingFile.TryGetFileContent", "Get one item content.", true))
            {
                dataString = string.Empty;
                try
                {
                    if (recordFile.TrainingParseTimeoutFile)
                    {
                        throw new Exception("This file flag parse timeout.");
                    }
                    IExtract extractor = new Extractor();
                    var support = extractor.IsSupportType(recordFile.ExtensionForFile);
                    if (support)
                    {
                        IAveFile aveDoc = item.File;
                        logger.Info($"Check file length, record id: {recordFile.Id}, length: {aveDoc.Length}");

                        string? content = "";
                        Stream? stream = null;

                        using (new PerformanceScope("ProcessTrainingFile.TryGetFileContent.OpenBinaryStream", "open one item stream.", true))
                        {
                            int retryTimes = 0;
                            while (retryTimes < 3)
                            {
                                try
                                {
                                    stream = aveDoc.OpenBinaryStream();
                                    break;
                                }
                                catch (AveWrapperException awe)
                                {
                                    if (awe.ErrorCode == AveWrapperErrorCode.FileContentLengthDismatch)
                                    {
                                        logger.Warn($"An error occurred while open file binary stream, itemid: {item.UniqueId}. Try times: {retryTimes}, Message : {awe}.");
                                        Thread.Sleep(1000);
                                    }
                                    else
                                    {
                                        throw;
                                    }
                                }
                                catch (Exception)
                                {
                                    throw;
                                }
                                retryTimes++;
                            }
                        }
                        if (stream == null)
                        {
                            logger.Warn($"This term[{term.Id}], Get file content error, record id:{recordFile.Id}");
                            return false;
                        }
                        using (stream)
                        {
                            try
                            {
                                using (new PerformanceScope("ProcessTrainingFile.TryGetFileContent.ExtractContent", "extract one item content.", true))
                                {
                                    content = AveTenantTasks.ExecuteActionHaveTimeOut<string>(() =>
                                    {
                                        logger.Debug($"Before extract file: {recordFile.Id}, percess memory {ProcessUtil.GetProcessMemoryMB()}MB");
                                        //Thread.Sleep(TimeSpan.FromMinutes(2));
                                        var tempContent = extractor.ExtractAsync(stream, recordFile.ExtensionForFile, new ExtractOption() { MaxCharsCountPerFile = 1024 * 3 * 10 }).GetAwaiter().GetResult();
                                        logger.Info($"File content length {tempContent.Length}, record id: {recordFile.Id}");
                                        logger.Debug($"After extract file: {recordFile.Id}, percess memory {ProcessUtil.GetProcessMemoryMB()}MB");
                                        return tempContent;
                                    }, 5);
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                recordFile.TrainingParseTimeoutFile = true;
                                if (isCosmosBulkOperationEnabled)
                                {
                                    CosmosBulkOperator.Instance.Add(recordFile);
                                }
                                else
                                {
                                    explorerDao.Upsert(recordFile);
                                }
                                throw;
                            }

                            var contentHashCode = content != null ? content.GetHashCode() : 0;
                            if (checkDuplicateFile.Contains(contentHashCode))
                            {
                                logger.Warn($"This term[{term.Id}], File content duplicate skip this, record id: {recordFile.Id}");
                                return false;
                            }
                            else
                            {
                                lock (duplicateCheckLocker)
                                {
                                    if (checkDuplicateFile.Contains(contentHashCode))
                                    {
                                        logger.Warn($"This term[{term.Id}], File content duplicate skip this, record id: {recordFile.Id}");
                                        return false;
                                    }
                                    else
                                    {
                                        checkDuplicateFile.Add(contentHashCode);
                                    }
                                }
                            }
                            dataString = content?.ToSmapleFileLine(term.Id.ToString(), termCacheDic[term.Id].Name);
                            var dataColumnsCount = dataString?.Split("\t")?.Length;
                            if (dataColumnsCount != 3)
                            {
                                logger.Warn($"This term[{term.Id}], Get file style error, record id:{recordFile.Id}, data columns count is: {dataColumnsCount}");
                                return false;
                            }
                        }
                        recordFile.FullPath = item.FullPath();
                    }
                    return support;
                }
                catch (Exception e)
                {
                    logger.Warn($"This term[{term.Id}], Get file content error, record id:{recordFile.Id}, error message:{e}");
                    return false;
                }
            }
        }
        private void SendDetails(string termName, Record record, JobDetailsStatus status = JobDetailsStatus.Successful, string comment = "")
        {
            JMTrainingJobDetails detail = new JMTrainingJobDetails()
            {
                TermName = termName,
                FileName = record.LeafName,
                FullPath = record.FullPath,
                Status = status,
                Comment = comment
            };

            reportManager.SendJobDetail(detail);
        }

        /// <summary>
        /// 检查setting，如果开启了批量插入数据到cosmos db,那么会做相关的初始化操作
        /// </summary>
        private void InitCosmosBulkOperation()
        {
            var RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
            isCosmosBulkOperationEnabled = keyValueDao.IsCosmosBulkOperationEnabled();
            bulkSize = keyValueDao.GetCosmosBulkInsertOperationBufferSize();
            if (bulkSize == default(int)) bulkSize = CosmosBulkOperator.DefualtBufferSize;
            logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
        }

        private async Task UpdateProcessSucceedRecord(Record record)
        {
            logger.Info($"Update record to db success, the item id:{record?.Id}");
            //only report item

        }

        private void UpdateProcessFailedRecord(Record record, Exception ex)
        {
            logger.Warn($"Update record to db failed, the item id:{record?.Id}, error: {ex}");
        }

        private async Task UpdateProcessSucceedRecordWithDetails(Record record)
        {
            SendDetails(termCacheDic[record.TrainingTermId].Name, record);
            logger.Info($"Update record with details to db success, the item id:{record?.Id}");
            //only report item

        }

        private void UpdateProcessFailedRecordWithDetails(Record record, Exception ex)
        {
            SendDetails(termCacheDic[record.TrainingTermId].Name, record, JobDetailsStatus.Failed);
            logger.Warn($"Update record with details to db failed, the item id:{record?.Id}, error: {ex}");
        }

        #region Copy from RMSPDiscoverBase

        public AveCamlQuery GetRowIdDiscoverQuery(IAveList list, IAveFolder folder, List<int> rowIds)
        {
            AveCamlQuery query = new AveCamlQuery();
            try
            {
                query.LoadAllItems = false;
                query.FolderServerRelativeUrl = folder.ServerRelativeUrl;
                query.ListItemCollectionPosition = new AveItemCollectionPosition();
                string queryStr = string.Empty;

                CAMLManager cm = new CAMLManager(Types.ScopeTypes.RecursiveAll);
                var group = new QueryGroup();

                //group.Conditions.Add(new QueryCondition(
                //Types.JoinTypes.And,
                //Types.FieldRefTypes.Name,
                //SPBuiltInFieldName.ModifiedTime,
                //Types.FieldTypes.DateTime,
                //Types.QueryTypes.FromTo,
                //CreateISO8601DateTimeFromSystemDateTime(startTime),
                //CreateISO8601DateTimeFromSystemDateTime(endTime),
                //            true));
                foreach (var rowId in rowIds)
                {
                    group.Conditions.Add(new QueryCondition(
                             Types.JoinTypes.Or,
                             Types.FieldRefTypes.Name,
                              "ID",
                            Types.FieldTypes.Number,
                            Types.QueryTypes.Eq,
                             rowId.ToString(), false));
                }
                cm.QueryGroup.AddGroup(group);
                //AddRowLimitQueryCondition(cm, group, startIndex, endIndex, rowLimit);
                string queryXml = cm.GetFullCAML(false);
                query.ViewXml = queryXml;
                query.DatesInUtc = true;
                logger.Info($"Process Folder {folder.ServerRelativeUrl}, row id count: {rowIds.Count}");
                //logger.Info("Query XML:{0}", query.ViewXml);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while getting items with caml query, ERROR:{0}", ex.ToString());
            }
            return query;
        }
        public IEnumerable<IAveListItem> GetItemsByRowIds(IAveList list, List<int> rowIds)
        {
            IEnumerable<IAveListItem> items = Enumerable.Empty<IAveListItem>();
            using (var performance00 = new PerformanceScope("RMSPDiscoverBase.GetItemsByRowIdTotal", addToStatistics: true))
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    for (int j = 0; j < rowIds.Count; j += 100)
                    {
                        //经测试，每次查询120个rowid时性能较好
                        var tempRowIds = rowIds.Skip(j).Take(100).ToList();
                        AveCamlQuery query = GetRowIdDiscoverQuery(list, list.RootFolder, tempRowIds);
                        using (var performance = new PerformanceScope("RMSPDiscoverBase.GetItemsByRowId", addToStatistics: true))
                        {
                            var tempItems = list.GetItemsForRecords(query, j == 0);
                            if (tempItems != null && tempItems.Any())
                            {
                                if (!items.Any())
                                {
                                    items = tempItems;
                                }
                                else
                                {
                                    items = items.Concat(tempItems);
                                }
                            }
                        }
                    }
                }
            }
            return items;
        }

        #endregion
    }
}
