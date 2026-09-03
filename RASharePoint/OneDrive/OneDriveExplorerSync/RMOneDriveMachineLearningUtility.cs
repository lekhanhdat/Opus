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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.RACommonUtility.Browser;
using System.Collections.Concurrent;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.Wrapper.Common;
using Cloud.Sdk.Data.Amls.Ics.Contracts;
using Util.AI.Text.Extractor;
using System.IO;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.RMSharePointColumn;
using System.Threading;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.Object;

namespace AvePoint.RA.SharePoint.OneDrive.OneDriveExplorerSync
{
    public  class RMOneDriveMachineLearningUtility
    {
        #region interface
        private static ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private static IRMMLTermDao mlTermDao => PlatformWindsorManager.GetService<IRMMLTermDao>();
        private static ITermSetDao TermSetDao => PlatformWindsorManager.GetService<ITermSetDao>();
        private static IOneDriveSettingDao OneDriveSettingDao => PlatformWindsorManager.GetService<IOneDriveSettingDao>();
        #endregion

        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ConcurrentDictionary<Guid, PredictScoreResult> filePredictResultCache;
        private static readonly ConcurrentDictionary<Guid, MLTermDto> trainingTermCache;
        private static readonly ConcurrentDictionary<Guid, string> siteUrlMapping;
        private static readonly List<RMOneDriveSetting> mAllSettings;
        private static Dictionary<Guid, Dictionary<Guid, bool>> mTermAllowToParent = new Dictionary<Guid, Dictionary<Guid, bool>>();
        private static Dictionary<Guid, string> mTermPaths = new Dictionary<Guid, string>();
        private static Dictionary<Guid, RMOneDriveSetting> mSiteOneDriveSettingMapping = new Dictionary<Guid, RMOneDriveSetting>();
        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private static IRMMLTrainingModelDao TrainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();


        static RMOneDriveMachineLearningUtility()
        {
            filePredictResultCache = new ConcurrentDictionary<Guid, PredictScoreResult>();
            trainingTermCache = new ConcurrentDictionary<Guid, MLTermDto>();
            siteUrlMapping = new ConcurrentDictionary<Guid, string>();
            mAllSettings = OneDriveSettingDao.LoadAllSetting();
        }

        public static void StartPredictTerm(List<IAveListItem> items, Guid settingSiteId, Guid groupId)
        {
            var itemsCount = items?.Count;
            using (var performance = new PerformanceScope("RMOneDriveMachineLearningUtility.StartPredictTerm", $"itemsCount:{itemsCount}", addToStatistics: true))
            {
                if (items == null || items.Count == 0)
                {
                    logger.Warn("No files need to predict term.");
                    return;
                }

                const int batchSize = 50;
                for (int i = 0; i < items.Count; i += batchSize)
                {
                    var batch = items.Skip(i).Take(batchSize).ToList();
                    logger.Info($"Processing batch {i / batchSize + 1} with {batch.Count} items.");

                    var predictRequest = RMMLPredictHelper.GetPredictRequest(batch);
                    if (predictRequest.ScoreRequests.Count == 0)
                    {
                        logger.Warn("The predicted file cannot generate ScoreRequests, possibly because the file type does not support prediction.");
                        continue;
                    }
                    try
                    {
                        var response = RMMLPredictHelper.GetPredictResult(predictRequest);
                        if (response == null || response.Count == 0)
                        {
                            logger.Warn("The predict response is null.");
                        }
                        else
                        {
                            using (new PerformanceScope("RMOneDriveMachineLearningUtility.AnalysePredictResult", addToStatistics: true))
                            {
                                response.ForEach(o =>
                                {
                                    var itemId = new Guid(o.Name);
                                    var itemDirPath = batch.FirstOrDefault(item => item.UniqueId == itemId)?.DirPath();
                                    AnalysePredictResult(o, itemDirPath, settingSiteId, groupId);
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error while call start predict term method, message: {ex}");
                    }
                }
            }
        }

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

        private static void AnalysePredictResult(ScoreResponse response, string dirPath, Guid settingSiteId, Guid groupId)
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
                    var isZeroShotFeature = RMKeyValueDao.EnableZeroShotFeature() && TrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot;
                    var minTermScore = isZeroShotFeature ? RMMLPredictHelper.MinTermScore4ZeroShot : RMMLPredictHelper.MinTermScore;
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
                            if (!IsSameTermScope(dirPath, settingSiteId, groupId, termId))
                            {
                                logger.Info($"The current predict term is not in the same term scope, termId:{termId}");
                                continue;
                            }
                            AddfilePredictResultCache(new Guid(fileId), RMMLPredictHelper.GetPredictScoreResult(termId, result.Score));
                            logger.Info($"The file use predict term, termId is : [{termId}], fileId: [{fileId}]");
                            break;
                        }
                    }
                    else 
                    {
                        logger.Info($"All predict term score is lower than the minimum value : {minTermScore}");
                    }
                }
            }
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

        private static bool IsSameTermScope(string dirPath, Guid settingSiteId, Guid groupId, Guid targetTermId)
        {
            var siteUrl = GetRemoteSiteCollectionUrl(settingSiteId);
            var fullPath = WebUtil.MakeFullUrl(siteUrl, dirPath);
            RMOneDriveSetting bindSetting = mAllSettings.Where(s => s.SiteGroupId == groupId && fullPath.StartsWith(s.FullPath)).OrderBy(s => s.FullPath.Length).FirstOrDefault();
            if (bindSetting == null)
            {
                bindSetting = GetGroupLevelSetting(settingSiteId);
            }

            if (bindSetting == null)
            {
                return false;
            }

            if (CheckTermValue(bindSetting, targetTermId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static string GetRemoteSiteCollectionUrl(Guid settingSiteId)
        {
            if (!siteUrlMapping.TryGetValue(settingSiteId, out var remoteSiteCollectionUrl))
            { 
                var site = RABrowserClient.GetRemoteSiteCollectionById(settingSiteId.ToString());
                remoteSiteCollectionUrl = site?.url;
                if (!siteUrlMapping.TryAdd(settingSiteId, remoteSiteCollectionUrl))
                {
                    logger.Warn($"Failed to get remote site url, siteid: {settingSiteId}");
                }
            }
            return remoteSiteCollectionUrl;
        }

        private static RMOneDriveSetting GetGroupLevelSetting(Guid siteId)
        {
            if (mSiteOneDriveSettingMapping.ContainsKey(siteId))
            {
                return mSiteOneDriveSettingMapping[siteId];
            }
            else
            {
                var site = RABrowserClient.GetRemoteSiteCollectionById(siteId.ToString());
                if (site != null)
                {
                    var groupId = site.parentId;
                    var groupSetting = mAllSettings.Where(s => s.SiteGroupId == new Guid(groupId) && s.SiteId == Guid.Empty).FirstOrDefault();
                    if (groupSetting != null)
                    {
                        mSiteOneDriveSettingMapping.Add(siteId, groupSetting);
                        return groupSetting;
                    }
                    else
                    {
                        logger.Warn("Cannot find group setting for site, siteid:{0}", siteId);
                        return null;
                    }
                }
                else
                {
                    logger.Warn("Cannot find site, siteid:{0}", siteId);
                    return null;
                }
            }
        }

        private static bool CheckTermValue(RMOneDriveSetting setting, Guid termId)
        {
            bool bindTermSet = setting.TermId == Guid.Empty;
            var parentId = bindTermSet ? setting.TermSetId : setting.TermId;
            return CheckTermValue(bindTermSet, parentId, termId);
        }

        private static bool CheckTermValue(bool bindTermSet, Guid parentId, Guid termId)
        {
            string termPath = null;
            if (!mTermPaths.TryGetValue(termId, out termPath))
            {
                termPath = TermDao.GetTermIdPath(termId);
                mTermPaths[termId] = termPath;
            }

            if (string.IsNullOrEmpty(termPath))
            {
                return false;
            }

            Dictionary<Guid, bool> parentNodes = null;
            if (!mTermAllowToParent.TryGetValue(termId, out parentNodes))
            {
                parentNodes = new Dictionary<Guid, bool>();
                mTermAllowToParent[termId] = parentNodes;
            }

            string parentNodePath = null;
            bool isSubTerm = false;
            if (!parentNodes.TryGetValue(parentId, out isSubTerm))
            {
                if (bindTermSet)
                {
                    parentNodePath = (TermSetDao.GetRMTermSetByGuid(parentId)?.Id)?.ToString() + "/";
                }
                else
                {
                    parentNodePath = TermDao.GetTermIdPath(parentId) + "/";
                }
                isSubTerm = termPath.StartsWith(parentNodePath, StringComparison.OrdinalIgnoreCase);
                parentNodes[parentId] = isSubTerm;
            }
            return isSubTerm;
        }
    }
}
