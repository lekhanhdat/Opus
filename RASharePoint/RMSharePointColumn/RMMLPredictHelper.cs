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
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Contract.ContentManager.Object;
using AvePoint.GCommon.Contract.Replicator.Object;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.VectorDataCenter.Embedding;
using AvePoint.RA.VectorDataCenter.Services;
using AvePoint.RA.VectorDataCenter.Similarity;
using AvePoint.RA.VectorDataCenter.Storage;
using AvePoint.Wrapper.Common;
using Cloud.Sdk.Data.Amls.Ics.Contracts;
using Microsoft.SharePoint.Client.RecordsRepository;
using RAGoogle.Helper;
using RAGoogle.Util.StreamUtil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Util.AI.Text.Extractor;

namespace AvePoint.RA.SharePoint.RMSharePointColumn
{
    public class RMMLPredictHelper
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        //预测返回的Term评分如果都小于MinTermScore,则不用AI结果，走If Not Match
        public static readonly double MinTermScore = 0.6;
        public static readonly double MinTermScore4ZeroShot = VectorStoreFactory.CheckGCPEnv() ? 0.4 : 0.3;

        private static readonly int ExtractFileContentTimeout = 5; //单位分钟
        private static readonly int ReadFileContentThreadCount = 2;
        private static readonly int MaxPredictItemCount = 30;
        private static readonly ITermDao rmTermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));

        private static readonly ConcurrentDictionary<Guid, RMTerm> rmTermCache = new ConcurrentDictionary<Guid, RMTerm>();

        private static readonly ConcurrentDictionary<string, PredictFileInfo> rmPredictFileInfoCache = new ConcurrentDictionary<string, PredictFileInfo>();
        private static readonly ConcurrentDictionary<Guid, string> fileGetPredictRequestFailCache = new ConcurrentDictionary<Guid, string>();

        private static readonly HttpClient httpClient = new HttpClient();

        private static IRMMLTrainingModelDao mlTrainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();

        private static IRMKeyValueDao mlKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static Guid _defaultTrainingModeId;
        private static readonly AIDocumentExtractorProcess _aiExtractor = new AIDocumentExtractorProcess();
        private static bool IsZeroShotFeature = RMMachineLearningUtility.IsZeroShot;
        private static bool EmbeddingFullDocument = mlKeyValueDao.EmbeddingFullDocument();
        private static bool IsEnableShowPredictReport = RMMachineLearningUtility.IsEnableShowPredictReport;
        private static bool EnableExportExcelPreviewCsv = mlKeyValueDao.EnableExportExcelPreviewCsv();

        public static Guid DefaultTrainingModeId
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

        public static string jobId;

        private static RMMLPredictLogReport _rMMLPredictLogReport;
        public static RMMLPredictLogReport RMMLPredictLogReport
        {
            get
            {
                if( _rMMLPredictLogReport == null)
                {
                    _rMMLPredictLogReport = new RMMLPredictLogReport();
                    _rMMLPredictLogReport.Init(TenantLocalValue.LogonGroupId, jobId);
                }
                return _rMMLPredictLogReport;
            }
        }

        public static PredictRequest GetPredictRequest(List<IAveListItem> items)
        {
            var predictRequest = new PredictRequest
            {
                ScoreRequests = new List<ScoreRequest>()
            };
            ConcurrentBag<ScoreRequest> scoreRequestList = new();
            using (var scope = new PerformanceScope("MachineLearning.GetFilesContent", "MachineLearning read files content.", true))
            {
                using (AveAppendableTaskExecutor taskExecutor = new AveAppendableTaskExecutor(ReadFileContentThreadCount))
                {
                    taskExecutor.StartExecute();
                    foreach (var item in items)
                    {
                        taskExecutor.AddTask(() =>
                        {
                            try
                            {
                                var requestItem = ConvertToScoreRequestItem(item);
                                if (requestItem != null)
                                {
                                    scoreRequestList.Add(requestItem);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error($"An error while add score request item, message: {ex}");
                            }
                        });
                    }

                    logger.Info($"Add items to task executor finished.");
                    if (!taskExecutor.WaitForAllTasks(Timeout.Infinite))
                    {
                        logger.Error($"Time out exception.");
                    }

                    predictRequest.ScoreRequests.AddRange(scoreRequestList);
                    return predictRequest;
                }
            }
        }

        public static List<ScoreResponse> GetPredictResult(PredictRequest predictRequest)
        {
            List<ScoreResponse> scoreResponses = new();
            var scoreRequestList = predictRequest.ScoreRequests;
            var requestCount = scoreRequestList.Count;
            IVectorStore vectorStore = VectorStoreFactory.CreateVectorStore();
            var queryService = QueryService.CreateWithRAIProvider(vectorStore, new CosineSimilarityCalculator()).GetAwaiter().GetResult();
            if (IsZeroShotFeature)
            {
                foreach (var item in scoreRequestList)
                {
                    if (string.IsNullOrEmpty(item.Content))
                    {
                        logger.Warn($"Item content is empty, item uniqueId: {item.Name}");
                        continue;
                    }

                    using (var scope = new PerformanceScope("VectorizationService", $"Vectorization service item: [{item.Name}].", true))
                    {

                        var result = queryService.QueryAsync(item.Content).GetAwaiter().GetResult();
                        // Assuming 'result' is the output of QueryAsync and needs to be converted to List<ScoreResponse>
                        if(result == null)
                        {
                            logger.Error($"exception during similarity query: {item.Name}");
                            continue;
                        }
                        if (result != null)
                        {
                            var scoreResult = new List<ScoreResult>();
                            foreach (var score in result)
                            {
                                scoreResult.Add(new ScoreResult
                                {
                                    Label = score.id,
                                    Score = score.score,
                                });
                            }
                            // Assuming 'resultItem' can be mapped to ScoreResponse
                            var scoreResponse = new ScoreResponse
                            {
                                Name = item.Name,
                                Results = scoreResult,
                            };
                            scoreResponses.Add(scoreResponse);
                            if (IsEnableShowPredictReport)
                            {
                                rmPredictFileInfoCache.AddOrReplace(item.Name, new PredictFileInfo
                                {
                                    FileID = item.Name,
                                    FileSummary = item.Content,
                                    FilePath = item.Name,
                                    PredictionScores = BuildPredictionScore(scoreResult),
                                });
                            }
                        }
                        string BuildPredictionScore(List<ScoreResult> scoreResult)
                        {
                            try
                            {
                                StringBuilder builder = new StringBuilder();
                                scoreResult.ForEach(scope =>
                                {
                                    if(Guid.TryParse(scope.Label, out Guid labelId))
                                    {
                                        var term = GetRMTerm(labelId);
                                        builder.AppendLine($"{term?.Name ?? string.Empty} - {scope.Label}: {(scope.Score)}");
                                    }
                                    else
                                    {
                                        builder.AppendLine($" - {scope.Label}: {(scope.Score)}");
                                    }
                                });
                                return builder.ToString();
                            }
                            catch(Exception e)
                            {
                                logger.Error($"Build Prediction Score has errors: {e}");
                                return string.Empty;
                            }
                        }
                    }
                }
                return scoreResponses;
            }

            using (var scope = new PerformanceScope("MachineLearning.GetPredictResult", $"call predict api, total items count:{requestCount}", true))
            {
                for (int i = 0; i < requestCount; i += MaxPredictItemCount)
                {
                    var requestItems = scoreRequestList.Skip(i).Take(MaxPredictItemCount).ToList();
                    var newPredictRequest = new PredictRequest { ProfileId = predictRequest.ProfileId, ScoreRequests = requestItems };
                    try
                    {
                        using (new PerformanceScope("MachineLearning.CallPredictApi", $"call predict api once, items count: {requestItems.Count}", true))
                        {
                            logger.Info($"start call predict api once, items count: {requestItems.Count}");
                            var response = RMAosApiClient.GetPredictResult(TenantLocalValue.LogonGroupId, DefaultTrainingModeId, newPredictRequest);
                            scoreResponses.AddRange(response);
                        }
                    }
                    catch (Exception)
                    {
                        logger.Error("An error while get predict result");
                        throw new CallPredictServiceException();
                    }

                }
            }
            return scoreResponses;
        }

        public static void ReportPredictFile()
        {
            try
            {
                FlushPredictFileBatch(rmPredictFileInfoCache.Keys.ToList());
            }
            catch (Exception e)
            {
                logger.Error($"Report predict file info has errors: {e}. memory use:{AvePoint.RA.Common.Util.ProcessUtil.GetProcessMemoryMB()} MB");
            }
            finally
            {
                RMMLPredictLogReport.CompletePredictFileStreaming();
            }
        }

        public static void FlushPredictFileBatch(List<string> fileIds)
        {
            if (fileIds == null || fileIds.Count == 0)
            {
                return;
            }

            List<PredictFileInfo> batch = new List<PredictFileInfo>(fileIds.Count);
            foreach (var fileId in fileIds)
            {
                if (rmPredictFileInfoCache.TryGetValue(fileId, out PredictFileInfo predictFileInfo))
                {
                    batch.Add(predictFileInfo);
                }
            }

            if (batch.Count == 0)
            {
                return;
            }

            RMMLPredictLogReport.AppendPredictFileBatch(batch);
            batch.ForEach(item =>
            {
                if (!rmPredictFileInfoCache.TryRemove(item.FileID, out _))
                {
                    logger.Warn($"Failed to remove predict file info from cache, fileId: {item.FileID}");
                }
            });
        }


        public static void UpdateTermPredictForPredictFile(string fileId, Guid termId)
        {
            try
            {
                if(rmPredictFileInfoCache.TryGetValue(fileId, out PredictFileInfo predictFileInfo))
                {
                    var term = GetRMTerm(termId);
                    predictFileInfo.PredictTerm = $"{term?.Name ?? string.Empty} - {termId}";
                    rmPredictFileInfoCache.AddOrReplace(fileId, predictFileInfo);
                }
                else
                {
                    logger.Warn($"Does not exist file {fileId} in cache");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Update Term Predict For Predict File has errors: {e}");
            }
        }

        private static ScoreRequest ConvertToScoreRequestItem(IAveListItem item)
        {
            string name = item.UniqueId.ToString();
            ScoreRequest request = null;
            
            if (TryGetFileContent(item, out string dataString))
            {
                request = new ScoreRequest
                {
                    Name = name,
                    Content = dataString
                };
            }
            return request;
        }

        public static (Guid Id, string Name)? GetPredictRequestFailCache(Guid fileId)
        {
            try
            {
                if (fileGetPredictRequestFailCache.TryRemove(fileId, out var name))
                {
                    return (fileId, name);
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"An error occurred while getting file predict request. fileId: {fileId}, message: {ex}");
            }

            return null;
        }


        private static bool TryGetFileContent(IAveListItem item, out string dataString)
        {
            using var scope = new PerformanceScope("MachineLearning.TryGetFileContent", $"get one file content", true);
            dataString = string.Empty;
            try
            {

                IExtract extractor = new Extractor();
                var extension = Path.GetExtension(item.Url);
                var support = extractor.IsSupportType(extension);
                if (support)
                {
                    var itemName = item.Name;
                    var itemFileLength = item.File.Length;
                    var itemSize = Math.Round(((itemFileLength * 1.0) / 1024 / 1024), 4);
                    logger.Info($"file length: {itemFileLength}, itemId: {item.UniqueId}");
                    if(itemFileLength == 0)
                    {
                        logger.Info($"file length: {itemFileLength}byte = 0KB, itemId: {item.UniqueId}, do not predict {item.Name}");
                        return false;
                    }
                    using var fileStream = item.File.OpenBinaryStream();
                    using (new PerformanceScope("MachineLearning.TryGetFileContent.ExtractContent", $"extract one item content, itemId:[{item.UniqueId}], item size:[{itemSize} (MB)]", true))
                    {
                        try
                        {
                            dataString = AveTenantTasks.ExecuteActionHaveTimeOut(() =>
                            {
                                if (IsZeroShotFeature && ExcelUtil.CanReadExcelPreviewAsCsv(item.Url))
                                {
                                    var tempContentCsv = ExcelUtil.ReadExcelPreviewAsCsv(fileStream, item.Url);
                                    TryExportExcelPreviewCsv(item.UniqueId, tempContentCsv);
                                    logger.Info($"preview as csv file length {tempContentCsv.Length}, itemId: {item.UniqueId}");
                                    return tempContentCsv;
                                }

                                var tempContent = extractor.ExtractAsync(fileStream, extension, new ExtractOption() 
                                { 
                                    MaxCharsCountPerFile = IsZeroShotFeature ? 5 * 1024 * 1024 : 1024 * 3 * 10,
                                    FastMode = true
                                }).GetAwaiter().GetResult();
                                logger.Info($"extract file length {tempContent.Length}, itemId: {item.UniqueId}");
                                return tempContent;
                            });
                        }
                        catch (Exception ex)
                        {
                            logger.Warn($"An error occurred while extract file content, item id: {item?.UniqueId}, message: {ex}");
                            return false;
                        }
                        if (IsZeroShotFeature && !EmbeddingFullDocument)
                        {
                            logger.Info($"start to summary");
                            string templateDataString = dataString;
                            dataString = AveTenantTasks.ExecuteActionHaveTimeOut(() =>
                            {
                                try
                                {
                                    string jsonData = JsonSerializer.Serialize(new SummaryRequest { doc_content = templateDataString });
                                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                                    HttpResponseMessage response = httpClient.PostAsync(GetSummaryUrl(), content).GetAwaiter().GetResult();
                                    response.EnsureSuccessStatusCode();
                                    var responseData = JsonSerializer.Deserialize<SummaryResponse>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                                    var resultSummary = GetCombindDataString(itemName, responseData.summary);
                                    return resultSummary;
                                }
                                catch (Exception e)
                                {
                                    logger.Warn($"An error occurred while summary file content, : {item?.UniqueId}, item id: {item?.ID}, message: {e}");
                                    templateDataString = GetCombindDataString(itemName, templateDataString);
                                    return templateDataString;
                                }
                            }, ExtractFileContentTimeout);
                            logger.Info($"extract file length before summary {templateDataString.Length}, after summary {dataString.Length}, itemId: {item.UniqueId}");
                        }
                    }
                }
                return support;
            }
            catch (Exception ex)
            {
                logger.Warn($"An error occurred while get file content, item uniqueId: {item?.UniqueId}, item id: {item?.ID}, message: {ex}");
            }
            return false;
        }

        private static void TryExportExcelPreviewCsv(Guid itemUniqueId, string previewCsv)
        {
            if (!EnableExportExcelPreviewCsv || string.IsNullOrEmpty(previewCsv))
            {
                return;
            }

            try
            {
                var logFolderPath = RMMLPredictLogReport.GetJobLogFolderPath(TenantLocalValue.LogonGroupId, jobId);
                Directory.CreateDirectory(logFolderPath);

                var filePath = Path.Combine(logFolderPath, $"ExcelPreviewCsv_{itemUniqueId}.csv");
                File.WriteAllText(filePath, previewCsv, Encoding.UTF8);
                logger.Info($"Exported excel preview csv to {filePath}, itemId: {itemUniqueId}, length: {previewCsv.Length}");
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed to export excel preview csv, itemId: {itemUniqueId}, message: {ex}");
            }
        }

        private static string GetSummaryUrl()
        {
            return  RMGlobalConfiguration.AppConfig[RMAppSettingKey.GENERATE_SUMMARY_CONTENT_URL].TrimEnd('/') + "/generate_summary";
        }
        
        private static string GetCombindDataString(string name, string dataString)
        {
            return $"[{name}][SEP][{name}][SEP][{dataString}]";
        }

        public static PredictScoreResult GetPredictScoreResult(Guid termId, double termScore)
        {
            return new PredictScoreResult
            {
                TermId = termId,
                TermScore = termScore,
            };
        }

        #region 验证term是否在term management是可用状态
        public static bool InvalidTerm(Guid termId)
        {
            var termInvalid = false;
            var rmTerm = GetRMTerm(termId);
            if (rmTerm == null || rmTerm.IsDeprecated || rmTerm.IsRemoved)
            {
                termInvalid = true;
            }
            else
            {
                if (rmTerm.TermExpirationFrom != 0 || rmTerm.TermExpirationTo != 0)
                {
                    if (DateTime.UtcNow.Ticks < rmTerm.TermExpirationFrom || (rmTerm.TermExpirationTo != 0 && DateTime.UtcNow.Ticks > rmTerm.TermExpirationTo))
                    {
                        termInvalid = true;
                    }
                }
            }
            if (termInvalid)
            {
                logger.Warn($"Term is invalid [{termId}].");
            }
            return termInvalid;
        }

        private static RMTerm GetRMTerm(Guid termId)
        {
            if (!rmTermCache.TryGetValue(termId, out RMTerm rmTerm))
            {
                rmTerm = rmTermDao.GetRMTermByGuId(termId);
                if (!rmTermCache.TryAdd(termId, rmTerm))
                {
                    logger.Warn($"failed to add rm term cache, termId: {termId}");
                }
            }
            return rmTerm;
        }
        #endregion

    }

    public class SummaryResponse
    {
        public string summary { get; set; }
    }

    public class SummaryRequest
    {
        public string doc_content { get; set; }
    }
}
