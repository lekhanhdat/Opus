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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.RMMachineLearning;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.RMMachineLearning;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using AvePoint.Wrapper.Common;
using Azure.Storage.Blobs;
using Cloud.Sdk.Core;
using Cloud.Sdk.Data.Amls.Ics.Category;
using Cloud.Sdk.Data.Amls.Ics.Contracts;
using System.Text;
using Task = System.Threading.Tasks.Task;

namespace AvePoint.RA.ArtificialIntelligence.MachineLearningTraining
{
    public class MLAnalyseProcessor
    {
        private readonly RALogger logger = RALogger.GetInstance(typeof(MLAnalyseProcessor));
        #region Interface
        private readonly IJobMonitorDao jmDao;
        private readonly IRMMLTermDao trainingTermDao;
        private readonly IRMMLTrainingModelDao trainingModelDao;
        private readonly ITermDao termDao;
        private readonly IExplorerDao explorerDao;
        private readonly IRMKeyValueDao keyValueDao;

        private readonly IJobInfoUpdater jobInfoUpdater;
        private readonly IRMReportManager reportManager;

        #endregion


        private const string MachineLearningAnalyseFolderName = "MachineLearningAnalyse";
        private readonly string currentJobId;
        private Dictionary<Guid, RMTerm> termCacheDic;

        private bool startSuccess = false;
        private bool isCosmosBulkOperationEnabled = false; //是否开启了批量插入数据到cosmos db
        private int bulkSize = 0;

        public MLAnalyseProcessor(string jobId)
        {
            currentJobId = jobId;
            jmDao = (IJobMonitorDao)PlatformWindsorManager.GetService(typeof(IJobMonitorDao));
            trainingTermDao = (IRMMLTermDao)PlatformWindsorManager.GetService(typeof(IRMMLTermDao));
            trainingModelDao = (IRMMLTrainingModelDao)PlatformWindsorManager.GetService(typeof(IRMMLTrainingModelDao));
            termDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
            keyValueDao = (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));
            jobInfoUpdater = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));
            explorerDao = new ExplorerDao();

            jobInfoUpdater.UpdateJobState(currentJobId, (int)JobStatus.InProgress);
            ReportMangerFactory.Instance.Init(currentJobId, JobType.MachineLearningAnalyse);
            reportManager = ReportMangerFactory.Instance.ReportManager;
            reportManager.Increase(1);
            reportManager.StartUpdateJobProgress(60);
            termCacheDic = new Dictionary<Guid, RMTerm>();
        }

        public void Run()
        {
            try
            {
                InitCosmosBulkOperation();
                ProcessAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                reportManager.SetJobFinished(JobStatus.Failed);
                logger.Error($"Run training files job failed. error: {e}");
            }
        }

        private void InitCosmosBulkOperation()
        {
            var RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
            isCosmosBulkOperationEnabled = keyValueDao.IsCosmosBulkOperationEnabled();
            bulkSize = keyValueDao.GetCosmosBulkInsertOperationBufferSize();
            if (bulkSize == default(int)) bulkSize = CosmosBulkOperator.DefualtBufferSize;
            logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");
            if (isCosmosBulkOperationEnabled)
            {
                CosmosBulkOperator.Instance.Start(bulkSize, UpdateProcessSucceedRecord, UpdateProcessFailedRecord);
            }
        }

        public async Task ProcessAsync()
        {
            var trainingModel = trainingModelDao.GetDefaultModel();
            if (trainingModel == null)
            {
                logger.Error("training model is null, in records db.");
                reportManager.SetJobFinished(JobStatus.Failed);
                return;
            }
            logger.Info($"Train model info: train model job id:{trainingModel.CurrentTrainingJobId}, train status:{((OperationState)trainingModel.TrainStatus)}, publish status:{((OperationState)trainingModel.PublishStatus)}");
            var modelId = trainingModel.Id;
            var trainingJobId = trainingModel.CurrentTrainingJobId;
            /* Fortify Issue Type: Path Manipulation 
             * Sink Details: this
            * Ignore Reason: 从配置文件中读取的路径，不存在用户恶意输入 
            */
            var reportTempFolder = RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.REPORT_TEMP_FOLDER];
            string downloadToFilename = SecurityUtils.SafeCombinePath(
                reportTempFolder, TenantLocalValue.LogonGroupId, MachineLearningAnalyseFolderName, trainingJobId + ".zip");
            var downloadFolder = Path.GetDirectoryName(downloadToFilename) ?? "";
            if (!Directory.Exists(downloadFolder)) { Directory.CreateDirectory(downloadFolder); }
            var blobName = SecurityUtils.SafeCombinePath(TenantLocalValue.LogonGroupId, JobReportUtility.MachineLearningFolder, trainingJobId + ".zip");
            RAStorageUtil.DownloadReportBlobToFile(blobName, downloadToFilename);
            RAStorageUtil.DeleteReportBlob(blobName);

            var unZipFolder = SecurityUtils.SafeCombinePath(downloadFolder, Path.GetFileNameWithoutExtension(downloadToFilename));
            AvePoint.GCommon.ZipUtil.UnZipFile(downloadToFilename, unZipFolder);

            //var trainingTerms = trainingTermDao.FindList(t => t.Status == (int)MLTermStatus.Training).ToDictionary(t => t.Id, t => t);
            //var trainingTerms = trainingTermDao.FindAll().ToDictionary(t => t.Id, t => t);

            var activeStatus = new int[] { (int)MLTermStatus.NotTrain, (int)MLTermStatus.Training, (int)MLTermStatus.Trained };
            var trainingTerms = (await trainingTermDao.FindListAsync(t => Enumerable.Contains(activeStatus, t.Status))).ToDictionary(t => t.Id, t => t);
            if (trainingModel.PublishStatus == (int)MLModelStatus.Succeeded)
            {
                var client = AosApiUtility.GetIcsClient(TenantLocalValue.LogonGroupId);

                //var tempFolderPath = Path.Combine(JobReportUtility.GetMLTempleFolder("Temple"), trainingJobId, PredictionFolderName);
                var tempFolderPath = unZipFolder;

                var termFiles = Directory.GetFiles(tempFolderPath ?? "");
                foreach (var termFile in termFiles)
                {
                    logger.Info($"Process file: {termFile}");
                    var termId = "";
                    var termName = "";
                    List<List<string>> data = new();
                    try
                    {
                        using (var fileStream = File.OpenRead(termFile))
                        {
                            using StreamReader sr = new(fileStream, Encoding.UTF8);
                            while (!sr.EndOfStream)
                            {
                                string? tsvLine = sr.ReadLine();
                                if (!string.IsNullOrEmpty(tsvLine))
                                {
                                    var lineData = tsvLine.Split("\t");
                                    if (lineData?.Length != 3)
                                    {
                                        logger.Warn("parse tsv error");
                                    }
                                    if (lineData != null)
                                    {
                                        termId = lineData[0];
                                        termName = lineData[1];
                                        data.Add(lineData.ToList());
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn($"Parse data {termFile}, error: {e}");
                    }
                    if (data.Count > 0)
                    {
                        var termGuid = new Guid(termId);
                        if (!Guid.TryParse(termId, out termGuid))
                        {
                            logger.Warn($"Parse term id error: term id:[{termId}]");
                            continue;
                        }
                        if (!trainingTerms.Remove(termGuid, out RMMLTerm? term))
                        {
                            logger.Warn($"This term is train term, but now not exist. term id {termId}");
                            continue;
                        }

                        List<ScoreResponse>? response = null;
                        int retry = 0;
                        var retrySleepTimes = new int[] { 10, 30, 60 };
                        do
                        {
                            using (var scope = new PerformanceScope("Batch Predict One Term", addToStatistics: true))
                            {
                                response = await client.PredictionService.PredictAsync(modelId, GetPredictRequest(data));
                                if (data.Count != response.Count)
                                {
                                    var sleepTime = TimeSpan.FromSeconds(retrySleepTimes[retry]);
                                    await Task.Delay(sleepTime);
                                    retry++;
                                    logger.Info($"Predict retry, term:[{termId}], retry count:{retry}, retry sleep:{sleepTime.TotalMilliseconds}ms");
                                }
                                else
                                {
                                    retry = int.MaxValue;
                                }
                            }
                        } while (retry <= 3);
                        var numberOfHits = 0;
                        if (response == null)
                        {
                            logger.Warn("The predict response is null.");
                        }
                        else
                        {
                            logger.Info($"Term [{termId}] predict request / response count is: {data.Count} / {response.Count}.");
                            foreach (var r in response)
                            {
                                var highestScoreResult = r.Results.OrderByDescending(o => o.Score).FirstOrDefault();
                                logger.Info($"Highest score is: {highestScoreResult?.Score}");
                                if (termId.ToLowerInvariant() == highestScoreResult?.Label?.ToLowerInvariant() && highestScoreResult.Score > RMMLPredictHelper.MinTermScore)
                                {
                                    numberOfHits++;
                                }
                            }

                            var total = (double)data.Count;
                            var accuracy = numberOfHits / total;
                            if (accuracy < 1E-06)
                            {
                                accuracy = 0.01;//Prevent N/A display
                            }
                            logger.Info($"Term [{termId}] accuracy is: {accuracy}, hit count is: {numberOfHits}, total count is: {total}");
                            var mlTerm = trainingTermDao.Find(t => t.Id == termGuid);
                            mlTerm.Accuracy = accuracy;
                            mlTerm.Status = (int)MLTermStatus.Trained;
                            await trainingTermDao.UpdateAsync(mlTerm);
                            UpdateDataToTrained(mlTerm.Id);

                            JMTrainingJobDetails detail = new()
                            {
                                TermName = termName,
                                FullPath = termDao.GetRMTermWithPathByTermId(new Guid(termId)).FullPath
                            };
                            if (data.Count == response.Count)
                            {
                                detail.Status = JobDetailsStatus.Successful;
                            }
                            else if (data.Count != response.Count)
                            {
                                detail.Status = JobDetailsStatus.Failed;
                                detail.Comment = "RM_MachineLearning_AnalyseFailedDetails";
                            }
                            ReportMangerFactory.Instance.ReportManager.SendJobDetail(detail);
                        }
                    }
                }
            }

            if (trainingTerms.Any())
            {
                foreach (var notTrainTerm in trainingTerms.Values)
                {
                    var prevTermStatus = notTrainTerm.Status;
                    notTrainTerm.Accuracy = 0;
                    notTrainTerm.Status = (int)MLTermStatus.NotTrain;
                    await trainingTermDao.UpdateAsync(notTrainTerm);
                    logger.Info($"Term [{notTrainTerm.Id}] updated from {prevTermStatus} to NotTrain.");
                    UpdateDataToNotTrain(notTrainTerm.Id);
                }
            }
            if (isCosmosBulkOperationEnabled && startSuccess)
            {
                CosmosBulkOperator.Instance.Complete();
                CosmosBulkOperator.Instance.Reset();
            }
            if (trainingModel.TrainStatus == (int)MLModelStatus.Succeeded && trainingModel.PublishStatus == (int)MLModelStatus.Succeeded)
            {
                reportManager.SetJobFinished(JobStatus.Finished);
                var tenantId = TenantLocalValue.LogonGroupId;
                logger.Warn($"Current tenant: {tenantId} trainning success. Delete sample files after training is complete");
                await DeleteTrainingFileData(tenantId, trainingModel);
            }
            else
            {
                reportManager.SetJobFinished(JobStatus.Failed, "RM_MachineLearning_EndPointPublishFailed");
            }
        }

        private PredictRequest GetPredictRequest(List<List<string>> items)
        {
            var predictRequest = new PredictRequest
            {
                ScoreRequests = new List<ScoreRequest>()
            };
            List<ScoreRequest> requestItems = new();
            var index = 0;
            items.ForEach(o =>
            {
                var requestItem = new ScoreRequest
                {
                    Name = index.ToString(),
                    Content = o[2]
                };
                if (requestItem != null)
                {
                    requestItems.Add(requestItem);
                }
                index++;
            });
            predictRequest.ScoreRequests.AddRange(requestItems);
            return predictRequest;
        }

        private void UpdateDataToTrained(Guid trainTermId)
        {
            logger.Info($"update data status trained.");
            if (isCosmosBulkOperationEnabled)
            {
                var items = explorerDao.QueryAll(r => r.TrainingScope == (int)MLFileStatus.Training && r.TrainingTermId == trainTermId);
                logger.Info($"UpdateDataToTrained file count:{items.Count()}");
                foreach (var item in items)
                {
                    item.TrainingScope = (int)MLFileStatus.Trained;
                    CosmosBulkOperator.Instance.Add(item);
                    startSuccess = true;
                }
            }
            else
            {
                explorerDao.UpdateAll(r => r.TrainingScope == (int)MLFileStatus.Training, r => { r.TrainingScope = (int)MLFileStatus.Trained; });
            }
        }

        private void UpdateDataToNotTrain(Guid trainTermId)
        {
            logger.Info($"update data status not train.");
            if (isCosmosBulkOperationEnabled)
            {
                var items = explorerDao.QueryAll(r => r.TrainingScope == (int)MLFileStatus.Training && r.TrainingTermId == trainTermId);
                logger.Info($"UpdateDataToNotTrain file count:{items.Count()}");
                foreach (var item in items)
                {
                    item.TrainingScope = (int)MLFileStatus.NotTrain;
                    CosmosBulkOperator.Instance.Add(item);
                    startSuccess = true;
                }
            }
            else
            {
                explorerDao.UpdateAll(r => r.TrainingScope == (int)MLFileStatus.Training, r => { r.TrainingScope = (int)MLFileStatus.NotTrain; });
            }
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

        private async Task DeleteTrainingFileData(string tenantId, RMMLTrainingModel trainingModel)
        {
            try
            {
                RMRetryer retryer = RMRetryerBuilder.CreateBuilder().Build();
                var icsClient = AosApiUtility.GetIcsClient(tenantId);
                var modelId = trainingModel.Id;
                if (!RMGlobalConfiguration.EnvSetting.IsGCPEnvironment)
                {
                    // For non-GCP environments, use the old BlobClient deletion method
                    retryer.Retry(() =>
                    {
                        var storageInfo = icsClient.StorageService.GetStorageInfoForDeleteAsync(modelId, new Cloud.Sdk.Data.Amls.Ics.Contracts.StorageRequest() { TsvFileName = RecordsConstants.TrainingData_FileName }).GetAwaiter().GetResult();
                        var blobClient = new BlobClient(new Uri(storageInfo.SasToken));
                        blobClient.DeleteIfExists();
                        logger.Info($"delete data.tsv success using BlobClient.");
                    });
                }
                else
                {
                    retryer.Retry(() =>
                    {
                        var storageInfo = icsClient.StorageService.GetStorageInfoForDeleteAsync(modelId, new Cloud.Sdk.Data.Amls.Ics.Contracts.StorageRequest() { TsvFileName = RecordsConstants.TrainingData_FileName }).GetAwaiter().GetResult();
                        using (var httpClient = new HttpClient())
                        {
                            var response = httpClient.DeleteAsync(storageInfo.SasToken).GetAwaiter().GetResult();
                            response.EnsureSuccessStatusCode();
                        }
                        logger.Info($"delete data.tsv success in GCP environment.");
                    });
                }
            }
            catch (CloudApiException e)
            {
                if (e.ErrorCode == 404)
                {
                    logger.Warn($"Ics service not found, it means do not need delete: {e}");
                }
                else
                {
                    logger.Error($"DeleteAIRelated error: {e}");
                }
            }
            catch (Exception e)
            {
                logger.Error($"DeleteAIRelated error: {e}");
            }
        }

    }
}
