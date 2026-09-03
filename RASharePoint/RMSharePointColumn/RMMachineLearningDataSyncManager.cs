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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMSharePointColumn
{
    public class RMMachineLearningDataSyncManager
    {
        private static readonly IRALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IExplorerDao ExplorerDao;

        private static readonly ICosmosBulkOperator CosmosOperator;

        private static readonly bool isCosmosBulkOperationEnabled = true;

        private static readonly int bulkSize;
        static RMMachineLearningDataSyncManager()
        {
            ExplorerDao = new ExplorerDao(true);
            var RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
            //isCosmosBulkOperationEnabled = RMKeyValueDao.IsCosmosBulkOperationEnabled();
            bulkSize = RMKeyValueDao.GetCosmosBulkInsertOperationBufferSize();
            if (bulkSize == default) bulkSize = CosmosBulkOperator.DefualtBufferSize;
            logger.Info($"Cosmos bulk operation enabled, bulk size: {bulkSize}");

            CosmosOperator = CosmosBulkOperator.Instance;
            CosmosOperator.Start(bulkSize, ProcessSucceedRecord, ProcessFailedRecord);
            logger.Info("success to start bulk operation.");
        }

        public static async System.Threading.Tasks.Task SyncItemToDBAsync(IAveListItem aveItem, Guid remoteSiteId, RMSharePointSetting setting, ApplySettingPredictResult predictResult)
        {
            using var performance = new PerformanceScope("RMMachineLearningSync.SyncItemToDB", addToStatistics: true);
            var newItem = await AssembleRecordAsync(aveItem, remoteSiteId, setting, predictResult);
            if (isCosmosBulkOperationEnabled)
            {
                Add2BulkOperationQueue(newItem);
            }
        }

        private static void Add2BulkOperationQueue(Record newItem)
        {
            Record dbRecord = null;
            using (var performance = new PerformanceScope("RMMachineLearningSync.GetDBRecord", addToStatistics: true))
            {
                dbRecord = ExplorerDao.ReadById(newItem.ScopeId, newItem.Id);
            }
            if (dbRecord == null)
            {
                CosmosOperator.Add(newItem);
                logger.Info($"create a new record for cosmosdb, itemId:{newItem?.Id}");
            }
            else
            {
                logger.Info($"this data already exists in cosmosdb, itemId:{newItem?.Id}");
                if (ExplorerDao.NeedUpdateMLManualRecord(newItem, false, dbRecord))
                {
                    logger.Info($"update predict information for existing data, itemId: {dbRecord.Id}");
                    UpdateAIPredictInfo(dbRecord, newItem);
                    CosmosOperator.Add(dbRecord);
                }
                else
                {
                    logger.Info($"The predict information has not changed and does not need to be updated, itemId: {dbRecord.Id}");
                }
            }
        }

        public static void Commit()
        {
            if (isCosmosBulkOperationEnabled)
            {
                CosmosOperator.Complete();
                CosmosOperator.Reset();
                CosmosOperator.Start(bulkSize, ProcessSucceedRecord, ProcessFailedRecord);
                logger.Info("reset and success to start bulk operation.");
            }
        }

        private static async Task<Record> AssembleRecordAsync(IAveListItem aveItem, Guid remoteSiteId, RMSharePointSetting setting, ApplySettingPredictResult predictResult)
        {
            using (var performance = new PerformanceScope("RMMachineLearningSync.AssembleRecord", addToStatistics: true))
            {
                var siteId = aveItem.ParentList.ParentWeb.Site.ID;
                var recId = IDGenerator.GetRecordId(siteId, aveItem.UniqueId);
                var itemUrl = aveItem.FullPath();
                var itemName = aveItem?.GetObjectName();
                var extension = GetItemExtension(itemName, aveItem);
                var recoEntity = new Record()
                {
                    Id = recId,
                    ScopeId = siteId,
                    NodeId = aveItem.UniqueId,
                    ContainerId = setting.SiteGroupId.ToString(),
                    DirPath = aveItem.DirPath(),
                    FullPath = itemUrl,
                    LeafName = itemName,
                    ExtensionForFile = extension,
                    AveSiteId = remoteSiteId.ToString(),
                    WebId = aveItem.ParentList.ParentWeb.ID,
                    ListId = aveItem.ParentList.ID,
                    //FolderId = Guid.Empty;
                    ItemId = aveItem.UniqueId,
                    CollectTime = DateTime.UtcNow.Ticks,
                    TimeCreated = aveItem.FieldValues.ContainsKey(SPColumnConstants.SP_Created) ? aveItem.GetUTCDateWithTimeZone(SPColumnConstants.SP_Created).Ticks : 0,
                    NodeType = (int)NodeLevel.Item,
                    HoldStatus = false,
                    SourceFlag = (int)predictResult.Source,
                    CreatedBy = aveItem.GetSingleUserFieldValue(SPColumnConstants.Author),
                    ModifiedBy = aveItem.GetSingleUserFieldValue(SPColumnConstants.Editor),
                    TimeModified = aveItem.FieldValues.ContainsKey(SPColumnConstants.Modified) ? aveItem.GetUTCDateWithTimeZone(SPColumnConstants.Modified).Ticks : 0,
                    ItemRowId = aveItem.ID,
                    RecordStatus = (int)RMRecordStatus.TrainingManualSync
                };
                if (predictResult.UnderReviewMethod == RMMLUnderReview.IsManual)
                {
                    var recordOwnerSettingType = predictResult.Source == SourceFlag.Teams ? RecordOwnerSettingType.AITeams: RecordOwnerSettingType.AISharePointOnline;
                    var recordOwners = await RMMachineLearningReviewerUtility.GetRecordOwnersAsync(setting.Id, recordOwnerSettingType);
                    if (setting.AISendEMail)
                    { 
                        RMMLManualApprovalEmailSender.AddNeedSendEmailUserId(recordOwners);
                    }
                    AppendAIManualInfo(recoEntity, predictResult, recordOwners);
                }

                if (predictResult.UnderReviewMethod == RMMLUnderReview.DirectAssign)
                {
                    AppendAIAutoApplyInfo(recoEntity, predictResult);
                }
                return recoEntity;
            }
        }

        private static void UpdateAIPredictInfo(Record dbRec, Record record)
        {
            if (dbRec != null && record != null)
            {
                dbRec.PredictTermId = record.PredictTermId;
                dbRec.PredictTermScore = record.PredictTermScore;
                dbRec.PredictTime = DateTime.UtcNow.Ticks;
                dbRec.MLUnderReview = record.MLUnderReview;
                dbRec.MLClassificationType = record.MLClassificationType;
                dbRec.MLReviewer = record.MLReviewer;
                dbRec.MLApprovalStatus = record.MLApprovalStatus;
                dbRec.MLEscalateFrom = record.MLEscalateFrom;
                dbRec.MLEscalatedComment = record.MLEscalatedComment;
                dbRec.TrainingModelId = record.TrainingModelId;
            }
        }

        private static void AppendAIAutoApplyInfo(Record record, ApplySettingPredictResult predictResult)
        {
            //AI直接打Term的数据信息，或者Setting开启Manual，但是训练Term开启了AutoApply，都视为AutoApply, 不走Manual流程
            if (record != null)
            {
                record.PredictTermId = predictResult.TermId;
                record.PredictTermScore = predictResult.TermScore;
                record.PredictTime = DateTime.UtcNow.Ticks;
                record.MLUnderReview = (int)RMMLUnderReview.DirectAssign;
                record.MLClassificationType = (int)RMMLClassificationType.AutoClassfied;
                record.MLApprovalStatus = (int)RMMLApprovalStatus.None;
                record.TrainingModelId = RMMLPredictHelper.DefaultTrainingModeId;
                record.MLEscalateFrom = 0;
                record.MLEscalatedComment = "";
            }
        }

        private static void AppendAIManualInfo(Record record, ApplySettingPredictResult predictResult, int[] reviewers)
        {
            //Setting开启Manual，保存AI预测信息到DB，走Manual流程  
            if (record != null)
            {
                record.PredictTermId = predictResult.TermId;
                record.PredictTermScore = predictResult.TermScore;
                record.PredictTime = DateTime.UtcNow.Ticks;
                record.MLUnderReview = (int)RMMLUnderReview.IsManual;
                record.MLClassificationType = (int)RMMLClassificationType.None;
                record.MLReviewer = reviewers;
                record.MLApprovalStatus = (int)RMMLApprovalStatus.WaitingApprove;
                record.TrainingModelId = RMMLPredictHelper.DefaultTrainingModeId;
                record.MLEscalateFrom = 0;
                record.MLEscalatedComment = "";
            }
        }
        private static string GetItemExtension(string objectName, IAveListItem aveItem)
        {
            var result = string.Empty;
            if (aveItem.ParentList.BaseType == AveBaseType.DocumentLibrary)
            {
                var ext = Path.GetExtension(objectName);
                result = ext.IndexOf(".") >= 0 ? ext.Substring(1) : "RM_RDM_RecordDetails_DataType_FileNull";
            }
            else
            {
                result = "RM_RDM_RecordDetails_DataType_SPItem";
            }
            return result;
        }

        private static async System.Threading.Tasks.Task ProcessSucceedRecord(Record record)
        {
            logger.Info($"Success to add record to db, the item id:{record?.Id}");
        }

        private static void ProcessFailedRecord(Record record, Exception ex)
        {
            logger.Warn($"Failed to add record to db, the item id:{record?.Id}");
        }

    }
}
