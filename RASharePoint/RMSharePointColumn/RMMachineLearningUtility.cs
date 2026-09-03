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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.Wrapper.Common;
using Cloud.Sdk.Data.Amls.Ics.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Util.AI.Text.Extractor;

namespace AvePoint.RA.SharePoint.RMSharePointColumn
{
    public class RMMachineLearningUtility
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        #region interface
        private static readonly IRMMLTermDao mlTermDao;
        private static readonly IRMKeyValueDao keyValueDao;
        private static readonly IRMMLTrainingModelDao trainingModelDao;
        #endregion

        private static readonly ConcurrentDictionary<Guid, PredictScoreResult> filePredictResultCache;
        private static readonly ConcurrentDictionary<Guid, MLTermDto> trainingTermCache;
        private static readonly ConcurrentDictionary<Guid, IAveTerm> aveTerms = new();
        private static Guid CurrentTermStoreId;
        public static bool IsZeroShot { private set; get; }
        private static bool isEnableShowPredictReport = false;
        public static bool IsEnableShowPredictReport => isEnableShowPredictReport;

        private static double minTermScore = RMMLPredictHelper.MinTermScore; 
        
        static RMMachineLearningUtility()
        {
            keyValueDao = (IRMKeyValueDao)PlatformWindsorManager.GetService(typeof(IRMKeyValueDao));
            trainingModelDao = (IRMMLTrainingModelDao)PlatformWindsorManager.GetService(typeof(IRMMLTrainingModelDao));
            mlTermDao = (IRMMLTermDao)PlatformWindsorManager.GetService(typeof(IRMMLTermDao));
            filePredictResultCache = new ConcurrentDictionary<Guid, PredictScoreResult>();
            trainingTermCache = new ConcurrentDictionary<Guid, MLTermDto>();
            IsZeroShot = keyValueDao.EnableZeroShotFeature() && trainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot;
            isEnableShowPredictReport = keyValueDao.EnableShowPredictReport();
        }

        // <summary>
        /// Sets the minimum term score for predictions.
        /// </summary>
        /// <param name="minScore">The minimum score to set.</param>
        public static void SetMinTermScore(double minScore)
        {
            minTermScore = minScore;
        }

        /// <summary>
        /// Starts the prediction of terms for the given list items.
        /// </summary>
        /// <param name="items">The list of items to predict terms for.</param>
        /// <param name="taxField">The taxonomy field.</param>
        /// <param name="aveSite">The AvePoint site.</param>
        public static void StartPredictTerm(List<IAveListItem> items, IAveTaxonomyField taxField, IAveSite aveSite)
        {
            var itemsCount = items?.Count;
            using (var performance = new PerformanceScope("RMMachineLearningUtility.StartPredictTerm", $"start predict, items Count:{itemsCount}", addToStatistics: true))
            {
                if (items == null || items.Count == 0)
                {
                    logger.Warn("No files need to predict term.");
                    return;
                }
                logger.Info("start to predict term.");

                ResetCache(aveSite);

                const int batchSize = 10;
                var shouldWritePredictReport = IsZeroShot && isEnableShowPredictReport;
                try
                {
                    for (int i = 0; i < items.Count; i += batchSize)
                    {
                        var batch = items.Skip(i).Take(batchSize).ToList();
                        logger.Info($"Processing batch {i / batchSize + 1}, batch size: {batch.Count}");

                        var predictRequest = RMMLPredictHelper.GetPredictRequest(batch);
                        if (predictRequest.ScoreRequests.Count == 0)
                        {
                            logger.Warn("The predicted file cannot generate ScoreRequests for this batch, possibly because the file type does not support prediction.");
                            continue;
                        }

                        var response = RMMLPredictHelper.GetPredictResult(predictRequest);
                        if (response == null || response.Count == 0)
                        {
                            logger.Warn("The predict response is null for this batch.");
                        }
                        else
                        {
                            using (new PerformanceScope("RMMachineLearningUtility.AnalysePredictResult", $"analyse predict result for batch {i / batchSize + 1}", addToStatistics: true))
                            {
                                response.ForEach(o =>
                                {
                                    AnalysePredictResult(o, taxField, aveSite);
                                });
                            }
                        }

                        if (shouldWritePredictReport)
                        {
                            logger.Info($"Before FlushPredictFileBatch.count:{batch.Count}. memory use:{ProcessUtil.GetProcessMemoryMB()} MB");
                            RMMLPredictHelper.FlushPredictFileBatch(predictRequest.ScoreRequests.Select(_ => _.Name).ToList());
                            logger.Info($"After FlushPredictFileBatch.count:{batch.Count}. memory use:{ProcessUtil.GetProcessMemoryMB()} MB");
                        }
                    }
                }
                finally
                {
                    if (shouldWritePredictReport)
                    {
                        logger.Info($"Before ReportPredictFile.memory use:{AvePoint.RA.Common.Util.ProcessUtil.GetProcessMemoryMB()} MB");
                        try
                        {
                            using (new PerformanceScope("RMMachineLearningUtility.ReportPredictFile", "report predict file", addToStatistics: true))
                            {
                                RMMLPredictHelper.ReportPredictFile();
                            }
                        }
                        finally
                        {
                            logger.Info($"After ReportPredictFile.memory use:{AvePoint.RA.Common.Util.ProcessUtil.GetProcessMemoryMB()} MB");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the Term information predicted by AI for the file
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public static MLTermDto GetFilePredictTerm(Guid fileId)
        {
            try
            {
                if (filePredictResultCache.ContainsKey(fileId))
                {
                    var scoreResult = filePredictResultCache[fileId];
                    var termDto = GetTrainingTermCache(scoreResult.TermId);
                    if (termDto != null)
                    {
                        termDto.PredictTermScore = scoreResult.TermScore;
                    }
                    if (!filePredictResultCache.TryRemove(fileId, out PredictScoreResult result))
                    {
                        logger.Warn($"Failed to remove fileid cache, fileId: {fileId}");
                    }
                    return termDto;
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"An error while get file predict term, fileId: {fileId}, message:{ex}");
            }
            return null;
        }

        /// Check the Scope of the predicted Term
        /// </summary>
        /// <param name="termId"></param>
        /// <param name="field"></param>
        /// <param name="currentAveSite"></param>
        /// <returns></returns>
        private static bool InSameTermScope(Guid termId, IAveTaxonomyField taxField, IAveSite aveSite)
        {
            try
            {
                if (taxField.AnchorId == Guid.Empty)
                {
                    //term scope is termset
                    var sourceTermSet = GetOrAddAveTerm(termId, aveSite).TermSet;
                    return sourceTermSet.ID.Equals(taxField.TermSetId) ? true : false;
                }
                else
                {
                    //term scope is term
                    var destinationTerm = GetOrAddAveTerm(taxField.AnchorId, aveSite);
                    if (destinationTerm == null)
                    {
                        return false;
                    }
                    //check if in the same termset
                    var sourceTerm = GetOrAddAveTerm(termId, aveSite);
                    if (!destinationTerm.TermSet.ID.Equals(sourceTerm.TermSet.ID))
                    {
                        return false;
                    }

                    //check path of term
                    return sourceTerm.PathOfTerm.StartsWith(destinationTerm.PathOfTerm + ";") ? true : false;
                }
            }
            catch (Exception e)
            {
                logger.Warn($"An error occurred while checking same term group. Error{e.ToString()}");
            }
            return false;
        }
      
        private static void AnalysePredictResult(ScoreResponse response, IAveTaxonomyField taxField, IAveSite aveSite)
        {
            if (response != null)
            {
                var fileId = response.Name;
                var results = response.Results;
                logger.Info($"fileId: [{fileId}], resultCount: [{results?.Count}]");
                if (results != null && results.Count > 0)
                {
                    //按评分高低排序
                    results = results.OrderByDescending(o => o.Score).ToList();
                    logger.Debug($"Predict results for fileId: {fileId}: " +
                        string.Join(", ", results.Select(p => $"LabelId: {p.Label}, Score: {p.Score}")));
                    logger.Info($"The min term score for predict jog: {minTermScore}");
                    var availableResult = results.Where(o => o.Score > minTermScore).ToList();
                    if (availableResult.Count > 0)
                    {
                        logger.Info($"There is result above the minimum score [{minTermScore}], count: {availableResult.Count}");
                        foreach (var result in availableResult)
                        {
                            var termId = new Guid(result.Label);
                            logger.Info($"The predict term result, termId: {termId}, term score: {result.Score:0.000000}");
                            if (RMMLPredictHelper.InvalidTerm(termId))
                            {
                                logger.Warn($"The current predict term is invalid, termId:{termId}");
                                continue;
                            }
                            if (!InSameTermScope(termId, taxField, aveSite))
                            {
                                logger.Warn($"The current predict term is not in the same term scope, termId:{termId}");
                                continue;
                            }
                            AddfilePredictResultCache(new Guid(fileId), RMMLPredictHelper.GetPredictScoreResult(termId, result.Score));
                            if(IsZeroShot && isEnableShowPredictReport)
                            {
                                UpdateTermPredictOfPredictFile(fileId, termId);
                            }
                            logger.Info($"The file use predict term, termId is : [{termId}], fileId: [{fileId}]");
                            break;
                        }
                    }
                    else
                    {
                        logger.Info($"The file predicts that the returned term scores are all below {minTermScore}, fileId:[{fileId}]");
                    }
                }
            }
        }

        private static void UpdateTermPredictOfPredictFile(string fileId, Guid termId)
        {
            RMMLPredictHelper.UpdateTermPredictForPredictFile(fileId, termId);
        }

        private static MLTermDto GetTrainingTermCache(Guid termId)
        {
            if (!trainingTermCache.TryGetValue(termId, out var term))
            {
                term = mlTermDao.GetValidTrainingTerm(termId);
                if (!trainingTermCache.TryAdd(termId, term))
                {
                    logger.Warn($"Failed to add training term, id:{termId}");
                }
            }
            return term;
        }

        private static void AddfilePredictResultCache(Guid fileId, PredictScoreResult scoreResult)
        {
            if (!filePredictResultCache.TryAdd(fileId, scoreResult))
            {
                logger.Warn($"Failed to add file predict result mapping, fileId: {fileId}, termId:{scoreResult.TermId}");
            }
        }

        private static IAveTerm GetOrAddAveTerm(Guid termId, IAveSite aveSite)
        {
            if (!aveTerms.TryGetValue(termId, out IAveTerm aveTerm))
            {
                aveTerm = aveSite.AveSPTaxonomySession.GetTerm(termId);
                if (aveTerm == null)
                {
                    throw new Exception("failed to get aveterm from site taxonomy session.");
                }
                if (!aveTerms.TryAdd(termId, aveTerm))
                {
                    logger.Warn($"failed to add aveterm cache, termId: {termId}");
                }
            }
            return aveTerm;
        }

        private static void ResetCache(IAveSite aveSite)
        {
            try
            {
                var termStoreId = GetTermSotreId(aveSite);
                if (termStoreId != CurrentTermStoreId && CurrentTermStoreId != Guid.Empty)
                {
                    RemoveCache();
                }
                CurrentTermStoreId = termStoreId;
                logger.Info($"current term store id: {CurrentTermStoreId}");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to rest cache, message: {ex}");
            }
        }

        private static Guid GetTermSotreId(IAveSite aveSite)
        {
            try
            {
                logger.Info($"start to get site term store, siteId:{aveSite?.ID}");
                if(aveSite == null )
                {
                    logger.Error("site is null");
                    return Guid.Empty;
                }
                return aveSite.AveSPTaxonomySession.TermStores[0].ID;
            }
            catch (Exception ex)
            {
                logger.Error($"failed to get site term store, message:{ex}");
            }
            return Guid.Empty;
        }

        private static void RemoveCache()
        {
            RemoveTrainingTermCache();
            RemoveAveTermCache();
        }

        private static void RemoveTrainingTermCache()
        {
            var termIds = trainingTermCache.Keys;
            termIds?.ForEach(termId =>
            {
                if (!trainingTermCache.TryRemove(termId, out MLTermDto term))
                {
                    logger.Warn($"Faile to remove termid cache, termId: {termId}");
                }
            });
        }

        private static void RemoveAveTermCache()
        {
            var termIds = aveTerms.Keys;
            termIds?.ForEach(termId =>
            {
                if (!aveTerms.TryRemove(termId, out IAveTerm term))
                {
                    logger.Warn($"Faile to remove aveterm cache, termId: {termId}");
                }
            });
        }
    }
}
