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
using AvePoint.RA.Contract.Label;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Cloud.Sdk.Data.Amls.Ics.Contracts;
using RADataSynchronize.TermCheck;
using RAGoogle.Helpers;
using RAGoogle.Models;

namespace RAGoogle.Util;

public class LabelManager
{
    private static readonly IRALogger logger = RALogger.GetInstance(typeof(LabelManager));
    public Dictionary<Guid, RMTerm> _termsCache = new Dictionary<Guid, RMTerm>();

    private readonly IRMChangeClassificationDao ChangeClassificationDao = PlatformWindsorManager.GetService<IRMChangeClassificationDao>();
    private IRMMLTermDao trainingTermDao => PlatformWindsorManager.GetService<IRMMLTermDao>();
    private IRMKeyValueDao keyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
    private IRMMLTrainingModelDao trainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();
    private ITermGroupDao TermGroupDao => (ITermGroupDao)PlatformWindsorManager.GetService(typeof(ITermGroupDao));
    private ITermGroupMembershipDao TermGroupMembershipDao => PlatformWindsorManager.GetService<ITermGroupMembershipDao>();

    private string TenantId { get; set; }

    public Dictionary<Guid, long> GetHasChangedLabelIds(long ticks)
    {
        var res = new Dictionary<Guid, long>();

        List<RMChangeClassification> changedLabels = ChangeClassificationDao.GetAllChangedInfo(ticks, (int)TermChangeType.TermRule);

        foreach (var changedLabel in changedLabels)
        {
            res[changedLabel.TermId] = changedLabel.ChangeTime;
        }

        return res;
    }

    public void SetTenantId(string tenantId)
    {
        TenantId = tenantId;
    }

    public async Task<LabelInfo?> GetMatchedLabelInfo(GoogleItemData item, GoogleSettingDto setting, ScoreResponse? scoreResponse = null)
    {
        if (setting.DeployLabelMethod == DeployLabelMethod.UseManualClassification)
        {
            return new LabelInfo
            {
                IsManually = true
            };
        }
        LabelInfo? labelInfo = null;
        if (setting.DeployLabelMethod == DeployLabelMethod.UseAutoClassification)
        {
            labelInfo = GetAutoMatchedRuleLabelInfo(item, setting);
        }
        
        // Try AI classification if no label found yet and AI is enabled
        if (ShouldUseAIClassification(labelInfo, setting))
        {
            labelInfo = await GetPredictResult(item, setting, scoreResponse);
        }
        return labelInfo;
    }

    private bool ShouldUseAIClassification(LabelInfo? labelInfo, GoogleSettingDto setting)
    {
        // Skip AI if we already have a valid label
        if (labelInfo != null && !string.IsNullOrEmpty(labelInfo.UniqueLabelId))
        {
            return false;
        }

        // Use AI if explicitly configured for intelligence classification
        if (setting.DeployLabelMethod == DeployLabelMethod.UseIntelligenceClassification)
        {
            return true;
        }

        // Use AI as fallback if AutoDefault is enabled
        if (setting.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault)
        {
            return true;
        }

        return false;
    }

    private async Task<LabelInfo> GetPredictResult(GoogleItemData item, GoogleSettingDto setting, ScoreResponse? scoreResponse = null)
    {
        LabelInfo result = new();
        // Try to get the predicted label info, using the provided scoreResponse if available
        var predictTerm = await GetMatchedSmartLableInfo(item, setting, scoreResponse);

        if (predictTerm != null)
        {
            result.NeedIpdateCosmosDB = true;
            var trainingTerm = trainingTermDao.GetValidTrainingTerm(new Guid(predictTerm.UniqueLabelId));
            if (trainingTerm != null)
            {
                if (setting.AIApprovalType == (int)ApprovalType.None)
                {
                    result.UniqueLabelId = trainingTerm.Id.ToString();
                    result.LabelName = trainingTerm.Name;
                    result.SmartLabelApplyType = SmartLabelApplyType.AutoApply;
                    result.Score = predictTerm.Score;
                    result.ApplyLabelType = ApplyLabelType.ApplyViaSmartTerm;
                    logger.Info($"direct set item value use predictTerm, itemId: [{item.Id}], itemUniqueId: [{item.UniqueId}], predictTermId: [{predictTerm.UniqueLabelId}]");
                }
                else if (setting.AIApprovalType == (int)ApprovalType.RecordOwners)
                {
                    logger.Info($"use ai manual of current item, itemId: [{item.Id}], itemUniqueId:[{item.UniqueId}], predictTermId: [{predictTerm.UniqueLabelId}]");
                    if (trainingTerm.AutoApply)
                    {
                        result.UniqueLabelId = trainingTerm.Id.ToString();
                        result.LabelName = trainingTerm.Name;
                        result.SmartLabelApplyType = SmartLabelApplyType.AutoApply;
                        result.Score = predictTerm.Score;
                        result.ApplyLabelType = ApplyLabelType.ApplyViaSmartTerm;
                        logger.Info($"set item value use predictTerm, because the predictTerm autoApply is [{trainingTerm.AutoApply}], itemId: [{item.UniqueId}], predictTermId: [{trainingTerm.Id.ToString()}]");
                    }
                    else
                    {
                        result.UniqueLabelId = trainingTerm.Id.ToString();
                        result.LabelName = trainingTerm.Name;
                        result.Score = predictTerm.Score;
                        result.SmartLabelApplyType = SmartLabelApplyType.ManualReview;
                        result.ApplyLabelType = ApplyLabelType.SkipApplyViaSmartTermByManual;
                    }
                }
            }
            else
            {
                if (setting.AIThenIsDefaultTermMethod)
                {
                    result.UniqueLabelId = setting.AIThenDefaultTermId;
                    result.LabelName = setting.AIThenDefaultTermName;
                    result.SmartLabelApplyType = SmartLabelApplyType.AutoApply;
                    result.IsManually = false;
                    result.ApplyLabelType = ApplyLabelType.ApplyDefaultLabel;
                    logger.Info($"when there are no prediction results, use then default term, itemUniqueId: [{item.UniqueId}], termId: [{setting.AIThenDefaultTermId}]");
                }
                else
                {
                    logger.Info($"when there are no prediction results, use manual choose term, itemUniqueId: [{item.UniqueId}]");
                }
            }
        }
        else
        {
            if (setting.AIThenIsDefaultTermMethod)
            {
                result.UniqueLabelId = setting.AIThenDefaultTermId;
                result.LabelName = setting.AIThenDefaultTermName;
                result.SmartLabelApplyType = SmartLabelApplyType.AutoApply;
                result.IsManually = false;
                result.ApplyLabelType = ApplyLabelType.ApplyDefaultLabel;
                logger.Info($"when there are no prediction results, use then default term, itemUniqueId: [{item.UniqueId}], termId: [{setting.AIThenDefaultTermId}]");
            }
            else
            {
                logger.Info($"when there are no prediction results, use manual choose term, itemUniqueId: [{item.UniqueId}]");
            }
        }
        return result;
    }

    public LabelInfo GetAutoMatchedRuleLabelInfo(GoogleItemData item, GoogleSettingDto setting)
    {
        if (item != null)
        {
            var values = GetRuleTypeMappingValue(item);
            if (!TermCriteriaChecker.TryGetAccordWithLabelInfo(setting.AutoClassificationRules, values, out var labelInfo))
            {
                throw new Exception($"The item [{item.Id}] find related term has an error.");
            }
            return labelInfo;
        }
        return null;
    }

    private async Task<LabelInfo?> GetMatchedSmartLableInfo(GoogleItemData item, GoogleSettingDto setting, ScoreResponse? scoreResponse)
    {
        if (item != null)
        {
            ScoreResponse? resultForOurItem = null;

            if (scoreResponse != null)
            {
                logger.Info($"Using cached scoreResponse for item {item.Id}.");
                // Use the provided scoreResponse
                resultForOurItem = scoreResponse;
            }
            else
            {
                logger.Info($"No cached scoreResponse provided for item {item.Id}, calling ML prediction service.");
                // Get prediction from ML service
                var predictRequest = await MLPredictHelper.GetPredictRequest(item);
                if (predictRequest.ScoreRequests.Count == 0)
                {
                    logger.Warn($"Item {item.Id} is not a supported file type for prediction, skipped.");
                    return null;
                }
                var predictResponse = MLPredictHelper.GetPredictResult(predictRequest);

                if (predictResponse == null || predictResponse.Count == 0)
                {
                    logger.Warn($"Prediction service returned no result for item {item.Id}.");
                    return null;
                }
                resultForOurItem = predictResponse.FirstOrDefault();
            }

            if (resultForOurItem?.Results == null || !resultForOurItem.Results.Any())
            {
                logger.Warn($"No predictions returned in the result for item {item.Id}.");
                return null;
            }
            var isZeroShotFeature = keyValueDao.EnableZeroShotFeature() && trainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot;
            var minTermScore = isZeroShotFeature ? MLPredictHelper.MinTermScore4ZeroShot : MLPredictHelper.MinTermScore;
            logger.Info($"The min term score for predict jog: {minTermScore}");
            logger.Debug($"Predict results for fileId: {item.Id}: " +
                    string.Join(", ", resultForOurItem.Results.OrderByDescending(o => o.Score).Select(p => $"LabelId: {p.Label}, Score: {p.Score}")));
            var highConfidencePredictions = resultForOurItem.Results
                        .Where(r => r.Score > minTermScore)
                        .OrderByDescending(r => r.Score)
                        .ToList();
            if (!highConfidencePredictions.Any())
            {
                logger.Info($"All predict term score for fileId: {item.Id} is lower than the minimum value : {minTermScore}");
                return null;
            }
            var validPredictions = new List<ScoreResult>();
            foreach (var prediction in highConfidencePredictions)
            {
                var labelId = new Guid(prediction.Label);
                var termGroupId = await TermGroupDao.GetTermGroupIdByTermUniqueId(labelId);
                var isInSameScope = TermGroupMembershipDao.ExistTermGroupInfo(new Guid(termGroupId), TenantId);

                if (isInSameScope)
                {
                    validPredictions.Add(prediction);
                }
            }

            if (validPredictions.Any())
            {
                var topPrediction = validPredictions[0];
                logger.Info($"[Prediction Success] fileId: {item.Id}, labelId: {topPrediction.Label}, score: {topPrediction.Score}");
                return new LabelInfo
                {
                    UniqueLabelId = topPrediction.Label,
                    IsManually = false,
                    SmartLabelApplyType = SmartLabelApplyType.ManualReview,
                    Score = topPrediction.Score
                };
            }
            else
            {
                logger.Info($"All predict term score for fileId: {item.Id} is not in the same term scope");
                return null;
            }
        }
        return null;
    }

    private Dictionary<ArchiverFilterRuleType, object> GetRuleTypeMappingValue(GoogleItemData item)
    {
        var nameArr = item.Name.Split('.');
        var extension = nameArr.Length > 1 ? nameArr.Last() : "";
        return new Dictionary<ArchiverFilterRuleType, object>
            {
                { ArchiverFilterRuleType.Name, item.Name },
                { ArchiverFilterRuleType.DocumentSize, item.Size },
                { ArchiverFilterRuleType.ModifiedTime, item.ModifiedTime.Ticks },
                { ArchiverFilterRuleType.CreatedTime, item.CreatedTime.Ticks },
                { ArchiverFilterRuleType.CreatedBy, item.CreatedBy },
                { ArchiverFilterRuleType.ModifiedBy, new List<string> {item.ModifiedBy, item.ModifiedByEmail} },
            };
    }

    public Dictionary<Guid, RMTerm> LoadTerms()
    {
        try
        {
            logger.Info("Begin to load terms.");
            ITermDao termDao = new TermDao();
            _termsCache = termDao.GetAllTermsForce().ToDictionary(t => t.UniqueId);
            logger.Info("Loaded {0} term to cache.", _termsCache.Count);

            return _termsCache;
        }
        catch (Exception e)
        {
            logger.Error($"Failed to load all terms to cache. Error: {e}");
            throw new Exception(I18NEntity.GetString("RM_JS_DocAve_CommunicationError"));
        }
    }

    public bool TryGetLabel(Guid termUniqueId, out RMTerm label)
    {
        return _termsCache.TryGetValue(termUniqueId, out label);
    }

}
