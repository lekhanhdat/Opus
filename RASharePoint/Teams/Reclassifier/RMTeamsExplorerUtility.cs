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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.MachineLearning;
using System.Collections.Generic;
using System.Linq;
using System;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.RMExplorer;
using AvePoint.Wrapper.Common;
using Newtonsoft.Json;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Dao;
using System.Threading.Tasks;
using AvePoint.RA.SharePoint.EnforceRetention;

namespace AvePoint.RA.SharePoint.Teams.Reclassifier
{
    public class RMTeamsExplorerUtility : RMExplorerUtility
    {
        private readonly ITeamsSettingDao TeamsSettingDao = PlatformWindsorManager.GetService<ITeamsSettingDao>();
        private readonly IRMRemoteNodeDao RMRemoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        public RMTeamsExplorerUtility(bool initForReclassify = false, ChangeTermType changeTermType = ChangeTermType.SearchChangeTerm) : base(initForReclassify, changeTermType)
        {
            labelUtility = new TeamsLabelUtility();
        }

        public RMTeamsExplorerUtility(bool needSendReport, bool initForReclassify, ChangeTermType changeTermType = ChangeTermType.SearchChangeTerm) : base(needSendReport, initForReclassify, changeTermType)
        {
            labelUtility = new TeamsLabelUtility(needSendReport);
        }

        public new async Task ChangeAllTermsAsync(ChangeTermOption changeTermInfo, string tempJobId, bool waiting4OtherSource)
        {
            try
            {
                using (new PerformanceScope("RMExplorerUtility.ChangeTermForTeams"))
                {
                    var isNewLogicAccount = TenantService.IsNewOpusTenant();
                    logger.Info("Is new logic account is {0}", isNewLogicAccount);
                    logger.Info("Change term action start {0}", tempJobId);
                    RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running, "");
                    RMRecordsUpdateTempDao.UpdateTempWaiting4OtherSource(tempJobId, waiting4OtherSource);
                    List<Record> records = new List<Record>();
                    if (changeTermInfo.SourceTeamsRecordIds != null && changeTermInfo.SourceTeamsRecordIds.Count > 0)
                    {
                        var startTime = DateTime.Now;
                        using (new PerformanceScope(string.Format("change.Term.GetRecords")))
                        {
                            records = ExplorerDao.QueryAll(r => changeTermInfo.SourceTeamsRecordIds.Contains(r.Id)).ToList();
                            logger.Warn($"[Change Term] 1. time elapsed for query {records.Count} records from cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");

                            List<Guid> allGuids = new List<Guid>();
                            allGuids.AddRange(changeTermInfo.SourceTeamsRecordIds);
                            var recordsNoti = ExplorerDao.QueryAll(r => allGuids.Contains(r.Id)).ToList();
                            RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running, JsonConvert.SerializeObject(recordsNoti.Select(r => r.LeafName).ToList()));
                        }
                        var trainingTerm = TrainingTermDao.GetTrainingTerm(changeTermInfo.TargetTermUniqueId);
                        if (mChangeTermType == ChangeTermType.AIMADirectlyApprove)
                        {
                            var termsIds = records.Select(t => t.PredictTermId).ToList();
                            cacheAllTermsDic = (await TermDao.FindListAsync(tm => termsIds.Contains(tm.UniqueId))).ToDictionary(t => t.UniqueId, t => t.Name);
                        }
                        var recDic = records.GroupBy(r => r.AveSiteId).ToDictionary(z => z.Key, p => p.ToList());
                        var avesiteIds = recDic.Keys.ToList();
                        Dictionary<string, RemoteSiteCollection> siteDic = new Dictionary<string, RemoteSiteCollection>();
                        List<Guid> failedIds = new List<Guid>();
                        List<Guid> successIds = new List<Guid>();
                        List<Record> successRecords = new List<Record>();

                        if (mChangeTermType == ChangeTermType.AIMAChangeTerm && changeTermInfo.TargetTermId == -1) //No Term
                        {
                            foreach (var rec in records)
                            {
                                rec.MLApprovalStatus = GetMLApprovalStatus();
                                rec.MLClassificationType = (int)RMMLClassificationType.Rejected;
                            }
                            var faileds = ExplorerDao.BatchUpdate(records, 5);
                            if (mNeedSendReport)
                            {
                                foreach (var rec in records)
                                {
                                    AddReclassifyDetailForGlobalSearch(rec, faileds.Contains(rec.Id) ? JobDetailsStatus.Failed : JobDetailsStatus.Successful, "", rec.ExtensionForFile != "RM_RDM_RecordDetails_DataType_SPItem");
                                }
                            }
                        }
                        else
                        {
                            if (avesiteIds.Count > 0)
                            {
                                string termName = changeTermInfo.TargetTermName;
                                Guid termId = changeTermInfo.TargetTermUniqueId;
                                using (new PerformanceScope(string.Format("change.Term.GetSites")))
                                {
                                    startTime = DateTime.Now;
                                    siteDic = RABrowserClient.GetRemoteSiteCollectionsByIdList(avesiteIds).ToDictionary(r => r.id);
                                    logger.Warn($"[Change Term] 2. time elapsed for query from DAO {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                }
                                foreach (var recList in recDic.Values)
                                {
                                    if (recList.Count > 0)
                                    {
                                        try
                                        {
                                            if (siteDic.ContainsKey(recList[0].AveSiteId))
                                            {
                                                var site = siteDic[recList[0].AveSiteId];
                                                startTime = DateTime.Now;
                                                var bposInfo = await PoolUserUtil.GetBPOSInfoAsync(site);
                                                logger.Warn($"[Declare] 3.time elapsed for GetBPOSInfo {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                startTime = DateTime.Now;
                                                var factory = MultiAppUtil.CreateAveObjectModelFactory(site.url, bposInfo, AveContextKind.ClientObjectModel);
                                                var spSite = factory.CreateSite();
                                                labelUtility.CacheSPLabel(spSite);
                                                currentAveSite = spSite;
                                                logger.Warn($"[Declare] 4.1.time elapsed for CreateSite {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                startTime = DateTime.Now;
                                                var columnName = GetBCSColumn(site);

                                                successRecords = ChangeRecordTermAction(spSite, columnName, recList, termName, termId, factory, bposInfo, ref failedIds);
                                                successIds = successRecords.Select(a => a.Id).ToList();
                                                logger.Warn($"[Change Term] 4. time elapsed for ChangeRecordTermAction {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                startTime = DateTime.Now;
                                                if (successIds.Count > 0)
                                                {
                                                    if (mChangeTermType == ChangeTermType.AIMAChangeTerm)
                                                    {
                                                        if (trainingTerm != null && MLTermStatusHelper.ActiveTermStatus.Contains(trainingTerm.Status))
                                                        {
                                                            var perviousTermId = Guid.Empty;
                                                            ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec =>
                                                            {
                                                                perviousTermId = rec.TermId;
                                                                rec.TermId = termId;
                                                                rec.TermName = termName;
                                                                rec.RuleId = Guid.Empty;
                                                                rec.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                                rec.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                                rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                                                rec.RecordOwner_Array = rec.RecordOwner.ExplorerSearchSplit();
                                                                rec.MLApprovalStatus = GetMLApprovalStatus();
                                                                rec.MLClassificationType = GetMLClassificationType();
                                                                rec.TrainingAddType = GetTrainingAddType();
                                                                rec.TrainingScope = (int)MLFileStatus.NotTrain;
                                                                rec.TrainingTermId = termId;
                                                                if(isNewLogicAccount && perviousTermId != termId) rec.RemoveManualFields();
                                                            });
                                                        }
                                                        else
                                                        {
                                                            var perviousTermId = Guid.Empty;
                                                            ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec =>
                                                            {
                                                                perviousTermId = rec.TermId;
                                                                rec.TermId = termId;
                                                                rec.TermName = termName;
                                                                rec.RuleId = Guid.Empty;
                                                                rec.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                                rec.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                                rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                                                rec.RecordOwner_Array = rec.RecordOwner.ExplorerSearchSplit();

                                                                rec.MLApprovalStatus = GetMLApprovalStatus();
                                                                rec.MLClassificationType = GetMLClassificationType();
                                                                if(isNewLogicAccount && perviousTermId != termId) rec.RemoveManualFields();
                                                            });
                                                        }
                                                    }
                                                    else if (mChangeTermType == ChangeTermType.AIMADirectlyApprove)
                                                    {
                                                        var perviousTermId = Guid.Empty;
                                                        foreach (var tempSuccess in successRecords)
                                                        {
                                                            perviousTermId = tempSuccess.TermId;
                                                            var tempTermName = "";
                                                            if (cacheAllTermsDic.ContainsKey(tempSuccess.PredictTermId))
                                                            {
                                                                termName = cacheAllTermsDic[tempSuccess.PredictTermId];
                                                            }
                                                            else
                                                            {
                                                                logger.Warn($"Can not found this term:{tempSuccess.PredictTermId}");
                                                            }
                                                            tempSuccess.TermId = tempSuccess.PredictTermId;
                                                            tempSuccess.TermName = tempTermName;
                                                            tempSuccess.RuleId = Guid.Empty;
                                                            tempSuccess.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                            tempSuccess.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                            tempSuccess.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                                            tempSuccess.RecordOwner_Array = tempSuccess.RecordOwner.ExplorerSearchSplit();
                                                            tempSuccess.MLApprovalStatus = GetMLApprovalStatus();
                                                            tempSuccess.MLClassificationType = GetMLClassificationType();
                                                            if (isNewLogicAccount && perviousTermId != termId) tempSuccess.RemoveManualFields();
                                                        }
                                                        ExplorerDao.BatchUpdate(successRecords, 5);
                                                    }
                                                    else if (mChangeTermType == ChangeTermType.SearchChangeTerm)
                                                    {
                                                        var perviousTermId = Guid.Empty;
                                                        ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec =>
                                                        {
                                                            perviousTermId = rec.TermId;
                                                            rec.TermId = termId;
                                                            rec.TermName = termName;
                                                            rec.RuleId = Guid.Empty;
                                                            rec.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                            rec.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                            rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                                            rec.RecordOwner_Array = rec.RecordOwner.ExplorerSearchSplit();
                                                            if(isNewLogicAccount && perviousTermId != termId) rec.RemoveManualFields();
                                                        });
                                                    }

                                                }
                                                logger.Warn($"[Change Term] 5. time elapsed for updating cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                foreach (var tempRecord in successRecords)
                                                {
                                                    ClassificationHistoryDao.Create(new RMClassificationHistory()
                                                    {
                                                        RecordId = tempRecord.Id,
                                                        PreviousTermId = tempRecord.TermId,
                                                        NewTermId = termId,
                                                        OperationTime = DateTime.UtcNow.Ticks
                                                    }
                                                    );
                                                }
                                                logger.Warn($"[Change Term] 6. time elapsed for updating cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                if (successIds.Count > 0)
                                                {
                                                    string actionString = GetActionString();
                                                    RecordsHistoryService.AddRecordsHistory(successIds, actionString, changeTermInfo.Comment);
                                                    startTime = DateTime.Now;
                                                    logger.Warn($"[Change Term] 6. time elapsed for AddReocrdHistory(succeed) to cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                                }
                                            }
                                            else
                                            {
                                                List<Guid> recIds = new List<Guid>();
                                                if (recList[0].SourceFlag == 1 || recList[0].SourceFlag == 11)
                                                {
                                                    throw new Exception("RM_RDM_SCNotFound");
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            failedIds.AddRange(recList.Select(t => t.Id));
                                            logger.Warn("change term action failed {0}", ex.ToString());
                                            if (mNeedSendReport)
                                            {
                                                foreach (var record in recList)
                                                {
                                                    AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Failed, getRealException(ex), record.ExtensionForFile != "RM_RDM_RecordDetails_DataType_SPItem");
                                                }
                                            }
                                        }
                                    }
                                }
                                if (mChangeTermType == ChangeTermType.AIMAChangeTerm)
                                {
                                    var trainingScopeCount = ExplorerDao.QueryCount(r => r.TrainingTermId == termId);
                                    var updateTrainingTerm = TrainingTermDao.Find(t => t.Id == termId);

                                    if (updateTrainingTerm != null && MLTermStatusHelper.ActiveTermIntStatus.Contains(updateTrainingTerm.Status))
                                    {
                                        updateTrainingTerm.TrainingScopeCount = trainingScopeCount;
                                        await TrainingTermDao.UpdateAsync(updateTrainingTerm);
                                    }
                                }
                            }
                        }

                        if (failedIds.Count > 0)
                        {
                            if (successIds.Any())
                            {
                                string actionString = GetActionString();
                                RecordsHistoryService.AddRecordsHistory(successIds, actionString, changeTermInfo.Comment);
                                startTime = DateTime.Now;
                                RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Failed_Partial);
                                logger.Warn($"[Change Term] 7. time elapsed for AddReocrdHistory(succeed) to cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                return;
                            }
                            FailedCount += failedIds.Count;
                            string failedNames = string.Empty;
                            foreach (var fid in failedIds)
                            {
                                failedNames += records.Where(t => t.Id == fid).FirstOrDefault().LeafName + ";";
                            }
                            failedNames = failedNames.TrimEnd(';');
                            RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, failedNames, RecordsConstants.Explorer_RealTime_Failed_All);
                            RecordsHistoryService.AddRecordsHistory(failedIds, "RM_JS_Audit_ChangeTermErrorMessage");
                            if (!mNeedSendReport)
                            {
                                throw new Exception(string.Format(I18NEntity.GetString("RM_RDM_Explorer_ChangeTermError"), failedIds));
                            }
                        }
                        else
                        {
                            RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Finished);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Failed_All);
                logger.Error("An error occured while changing term for Teams. Ex: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (labelUtility != null && labelUtility.LabelApplied)
                {
                    await labelUtility.AddLabelHistoryAsync();
                }
                logger.Info("Change term action finish {0}", tempJobId);
            }
        }

        protected override string GetBCSColumn(RemoteSiteCollection site)
        {
            var teamsGroup = RMRemoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(site.TeamId).Item1;
            var groupLevelSetting = TeamsSettingDao.LoadTeamsSetting(new Guid(teamsGroup.parentId), Guid.Empty, Guid.Empty);
            var columnName = groupLevelSetting.IsUsingExistColumnName ? groupLevelSetting.ExistColumnName : groupLevelSetting.ColumnName;
            return columnName;
        }
    }
}
