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
using AvePoint.GCommon.GraphAPI;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Threads;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.VectorDataCenter.Embedding;
using AvePoint.RA.VectorDataCenter.Services;
using AvePoint.RA.VectorDataCenter.Similarity;
using AvePoint.RA.VectorDataCenter.Storage;
using AvePoint.Wrapper.Common;
using Cloud.Sdk.Data.Amls.Ics.Contracts;
using Cloud.Sdk.IE.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Online.SharePoint.TenantAdministration;
using RAGoogle.Extension;
using RAGoogle.GoogleObjDiscover;
using RAGoogle.Models;
using RAGoogle.Services;
using RAGoogle.Util;
using RAGoogle.Util.StreamUtil;
using System;
using System.Collections.Concurrent;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Util.AI.Text.Extractor;


namespace RAGoogle.Helpers
{
    public class MLPredictHelper
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static readonly double MinTermScore = 0.6;

        public static readonly double MinTermScore4ZeroShot = VectorStoreFactory.CheckGCPEnv() ? 0.4 : 0.3;

        private static readonly int ExtractFileContentTimeout = 5;
        private static readonly int ReadFileContentThreadCount = 2;
        private static readonly int MaxPredictItemCount = 30;
        private const string GoogleExportCacheFolder = "GoogleExportCache";
        private static readonly ConcurrentDictionary<Guid, string> fileGetPredictRequestFailCache = new ConcurrentDictionary<Guid, string>();


        private static readonly HttpClient httpClient = new HttpClient();

        private static IRMMLTrainingModelDao mlTrainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();

        private static IRMKeyValueDao mlKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static readonly AIDocumentExtractorProcess _aiExtractor = new AIDocumentExtractorProcess();

        private static Guid _defaultTrainingModeId;
        private static bool IsZeroShotFeature = mlKeyValueDao.EnableZeroShotFeature() && mlTrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot;
        private static bool UsingOldLogicParsing = mlKeyValueDao.UseOldLogicParsing();
        private static bool EmbeddingFullDocument = mlKeyValueDao.EmbeddingFullDocument();
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

        public static async Task<PredictRequest> GetPredictRequest(GoogleItemData item)
        {
            var predictRequest = new PredictRequest
            {
                ScoreRequests = new List<ScoreRequest>()
            };
            ConcurrentBag<ScoreRequest> scoreRequestList = new();

            using (var scope = new PerformanceScope("MachineLearning.GetFilesContent", "MachineLearning read files content.", true))
            {
                try
                {
                    var requestItem = await ConvertToScoreRequestItemAsync(item);
                    if (requestItem != null)
                    {
                        scoreRequestList.Add(requestItem);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"An error while add score request item for item ID {item?.Id}, message: {ex}");
                }
                predictRequest.ScoreRequests.AddRange(scoreRequestList);
                return predictRequest;
            }
        }

        public static List<ScoreResponse> GetPredictResult(PredictRequest predictRequest)
        {
            List<ScoreResponse> scoreResponses = new();
            var scoreRequestList = predictRequest.ScoreRequests;
            var requestCount = scoreRequestList.Count;

            if (IsZeroShotFeature)
            {
                foreach (var item in scoreRequestList)
                {
                    if(string.IsNullOrEmpty(item.Content))
                    {
                        logger.Warn($"Item content is empty, item uniqueId: {item.Name}");
                    }

                    using (var scope = new PerformanceScope("VectorizationService", $"Vectorization service item: [{item.Name}]", true))
                    {
                        IVectorStore vectorStore = VectorStoreFactory.CreateVectorStore();
                        var queryService = QueryService.CreateWithRAIProvider(vectorStore, new CosineSimilarityCalculator()).GetAwaiter().GetResult();
                        List<(string id, float score)>? result = null;
                        try
                        {
                            var queryResults = queryService.QueryAsync(item.Content).GetAwaiter().GetResult();
                            if (queryResults != null)
                            {
                                result = queryResults.Select(x => (x.id, x.score)).ToList();
                            }
                            else
                            {
                                result = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"An error occurred while querying vector service for item {item.Name}: {ex}");
                        }

                        if (result != null)
                        {
                            var scopeResult = new List<ScoreResult>();
                            foreach(var score in result)
                            {
                                scopeResult.Add(new ScoreResult
                                {
                                    Label = score.id,
                                    Score = score.score
                                });
                            }
                            var scoreResponse = new ScoreResponse
                            {
                                Name = item.Name,
                                Results = scopeResult,
                            };
                            scoreResponses.Add(scoreResponse);
                        }
                    }
                }
                return scoreResponses;
            }

            using (var scope = new PerformanceScope("MachineLearning.GetPredictResult", $"call predict api, total items count:{requestCount}", true))
            {
                logger.Info($"Starting batch prediction for {requestCount} items with max batch size {MaxPredictItemCount}.");
                for (int i = 0; i < requestCount; i += MaxPredictItemCount)
                {
                    var requestItems = scoreRequestList.Skip(i).Take(MaxPredictItemCount).ToList();
                    var newPredictRequest = new PredictRequest { ProfileId = predictRequest.ProfileId, ScoreRequests = requestItems };
                    try
                    {
                        using (new PerformanceScope("MachineLearning.CallPredictApi", $"call predict api once, items count: {requestItems.Count}", true))
                        {
                            logger.Info($"Start call predict api for batch starting at index {i}, items count: {requestItems.Count}");
                            var response = RMAosApiClient.GetPredictResult(TenantLocalValue.LogonGroupId, DefaultTrainingModeId, newPredictRequest);
                            logger.Info($"Received {response?.Count ?? 0} prediction results for batch starting at index {i}.");
                            if (response != null)
                            {
                                scoreResponses.AddRange(response);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error while get predict result, message: {ex}");
                        throw new CallPredictServiceException();
                    }
                }
            }
            return scoreResponses;
        }

        private static async Task<ScoreRequest> ConvertToScoreRequestItemAsync(GoogleItemData item)
        {
            string name = item.Id.ToString();
            var (isSuccess, content) = await TryGetFileContentAsync(item);
            if (isSuccess)
            {
                var request = new ScoreRequest
                {
                    Name = name,
                    Content = content
                };
                logger.Info($"Successfully converted item {item.Id} to ScoreRequest.");
                return request;
            }
            logger.Warn($"Failed to get content for item {item.Id}, skipping ScoreRequest creation.");
            return null;
        }

        private static async Task<string> DownloadFileToLocal(GoogleDriveService googleDriveService, GoogleItemData item, string fileFolder)
        {
            string localFilePath = SecurityUtils.SafeCombinePath(fileFolder, item.Name);
            logger.Info($"Downloading file for {item.Name} to {localFilePath}...");

            try
            {
                if (GoogleConstant.GoogleExportMimeType.TryGetValue(item.MimeType, out string? mimeType))
                {
                    if (!string.IsNullOrEmpty(mimeType))
                    {
                        string fileExtension = GoogleFileExtension.GetFileExtentionFromMimeType(item.MimeType);
                        var formattedFileName = item.Name;
                        var formattedFileNameForDownload = item.Name + $"_{item.Id}" + fileExtension;
                        localFilePath = SecurityUtils.SafeCombinePath(fileFolder, formattedFileNameForDownload);

                        logger.Info($"Exported Google file {item.Name} with size {item.Size} to {localFilePath}.");
                        if (GoogleConstant.GoogleVideoMimeType.Contains(item.MimeType))
                        {
                            logger.Info($"Download media name {item.Name} with size {item.Size}. Local temp path: {localFilePath}");

                            await googleDriveService.DownloadMediaAsync(item.Id, mimeType, localFilePath);

                            logger.Info("Download media '{0}' successfully. Local temp path: {1}", item.Name, localFilePath);
                        }
                        else if (item.Size < GoogleConstant.DRIVE_FILE_SIZE_10MB) // file < 10MB
                        {
                            await googleDriveService.ExportFileAsync(item.Id, localFilePath, mimeType);
                        }
                        else
                        {
                            await googleDriveService.ExportBigFileAsync(item.Id, localFilePath, mimeType);
                        }
                    }
                }
                else
                {
                    await googleDriveService.DownloadFileAsync(item.Id, localFilePath);
                    logger.Info("Download file '{0}' successfully. Local temp path: {1}", item.Name, localFilePath);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to download file {item.Name}. Error: {ex.Message}");
            }
            return localFilePath;
        }


        private static async Task<(bool Success, string Content)> TryGetFileContentAsync(GoogleItemData item)
        {
            using var scope = new PerformanceScope("MachineLearning.TryGetFileContent", $"get one file content", true);
            try
            {
                logger.Info($"using the old logic to parsing: {UsingOldLogicParsing}");

                IExtract extractor = new Extractor();
                var extension = Path.GetExtension(item.Name);

                var appProfile = RMAosApiClient.GetGoogleAppProfile(TenantLocalValue.LogonGroupId, item.TenantId);
                RMGoogleDiscoverBase baseService = new(null);
                baseService.Init(appProfile);
                GoogleDriveService googleDriveService = new(appProfile, item.MemberEmail);
                MemoryStream memoryStream = new MemoryStream();
                using (memoryStream)
                {
                    var mimeType = item.MimeType;
                    if (mimeType == "application/vnd.google-apps.document" ||
                        mimeType == "application/vnd.google-apps.spreadsheet" ||
                        mimeType == "application/vnd.google-apps.presentation")
                    {
                        // For Google Docs, Sheets, and Slides, we need to download the file in a different way
                        extension = await googleDriveService.DownloadGoogleFileToMemoryStreamAsync(item.Id, memoryStream, mimeType);
                    }
                    else
                    {
                        // For other file types, use the standard download method
                        await googleDriveService.DownloadFileToStreamAsync(item.Id, memoryStream);
                    }

                    // #if DEBUG
                    //                     memoryStream.Dispose(); // Reset memory stream for debugging
                    //                     memoryStream = new MemoryStream(); // Reset memory stream for debugging
                    // #endif

                    // Check if memory stream is empty
                    if (memoryStream.Length == 0)
                    {
                        logger.Warn($"Memory stream is empty for item {item.Id}, attempting to download file again.");
                        var defaultDownloadPath = SecurityUtils.SafeCombinePath(AppDomain.CurrentDomain.BaseDirectory, GoogleExportCacheFolder);
                        Directory.CreateDirectory(defaultDownloadPath);
                        var tempFilePath = await DownloadFileToLocal(googleDriveService, item, defaultDownloadPath);
                        using (var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            memoryStream.SetLength(0);
                            await fileStream.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;
                        }
                        extension = Path.GetExtension(tempFilePath);
                        extension = extension.TrimStart('.');
                        try
                        {
                            if (File.Exists(tempFilePath))
                            {
                                File.Delete(tempFilePath);
                                logger.Info($"Temporary file deleted: {tempFilePath}");
                            }
                        }
                        catch (Exception cleanupEx)
                        {
                            logger.Warn($"Failed to delete temporary file {tempFilePath}: {cleanupEx.Message}");
                        }
                    }
                    googleDriveService.Dispose();
                    logger.Info($"Download success, itemId: {item.Id}, mime type: {mimeType}, extension: {extension}, memory stream length: {memoryStream.Length} bytes, position: {memoryStream.Position}");

                    var support = extractor.IsSupportType(extension);
                    if (!support)
                    {
                        logger.Warn($"File type {extension} is not supported for extraction, itemId: {item.Id}");
                        return (Success: false, Content: string.Empty);
                    }
                    var itemSize = Math.Round(memoryStream.Length * 1.0 / 1024 / 1024, 4);
                    string extractedContent = string.Empty;
                    using (new PerformanceScope("MachineLearning.TryGetFileContent.ExtractContent", $"extract one item content, itemId:[{item.Id}], item size:[{itemSize} (MB)]", true))
                    {
                        try
                        {
                            extractedContent = AveTenantTasks.ExecuteActionHaveTimeOut(() =>
                            {
                                var tempContent = extractor.ExtractAsync(memoryStream, extension, new ExtractOption() { MaxCharsCountPerFile = IsZeroShotFeature ? 5 * 1024 * 1024 : 1024 * 3 * 10 }).GetAwaiter().GetResult();
                                logger.Info($"extract file length {tempContent.Length}, itemId: {item.Id}");
                                return tempContent;
                            });
                        }
                        catch (Exception ex)
                        {
                            logger.Warn($"An error occurred while extract file content, item id: {item?.UniqueId}, message: {ex}");
                            return (false, string.Empty);
                        }
                        if (IsZeroShotFeature && !EmbeddingFullDocument)
                        {
                            logger.Info($"start to summary");
                            string templateDataString = extractedContent;
                            extractedContent = AveTenantTasks.ExecuteActionHaveTimeOut(() =>
                            {
                                try
                                {
                                    string jsonData = JsonSerializer.Serialize(new SummaryRequest { doc_content = templateDataString });
                                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                                    HttpResponseMessage response = httpClient.PostAsync(GetSummaryUrl(), content).GetAwaiter().GetResult();
                                    response.EnsureSuccessStatusCode();
                                    var responseData = JsonSerializer.Deserialize<SummaryResponse>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                                    var dataString = GetCombindDataString(item.Name, responseData.summary);
                                    return dataString;
                                }
                                catch (Exception e)
                                {
                                    logger.Warn($"An error occurred while get file content, : {item?.UniqueId}, item id: {item?.Id}, message: {e}");
                                    templateDataString = GetCombindDataString(item.Name, templateDataString);
                                    return templateDataString;
                                }
                            });
                            logger.Info($"extract file length before summary {templateDataString.Length}, after summary {extractedContent.Length}, itemId: {item.UniqueId}");
                        }

                    }

                    return (Success: true, Content: extractedContent);
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"An error occurred while get file content, item id: {item?.Id}, message: {ex}");
                return (Success: false, Content: string.Empty);
            }
        }

        private static string GetSummaryUrl()
        {
            return RMGlobalConfiguration.AppConfig[RMAppSettingKey.GENERATE_SUMMARY_CONTENT_URL].TrimEnd('/') + "/generate_summary";
        }

        private static string GetCombindDataString(string name, string dataString)
        {
            return $"[{name}][SEP][{name}][SEP][{dataString}]";
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
