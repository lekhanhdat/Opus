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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Global.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.EnforceRetention;
using AvePoint.Wrapper.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer.RMReclassifier
{
    public class RMSPReclassifier : RMReclassifierBase
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private SPOLabelUtility labelUtility = null;
        private IAveSite currentAveSite = null;
        private IAveWeb currentAveWeb = null;
        private SourceFlag _flag = SourceFlag.SharePoint;
        protected override SourceFlag Flag => _flag;
        protected Guid RevIMClassificationColumnID
        {
            get
            {
                return new Guid("20f84bba906045b4af568ee102a52dcb");
            }
        }

        public RMSPReclassifier(ChangeTermDto dto) : base(dto)
        {
            labelUtility = new SPOLabelUtility(true);
        }

        public RMSPReclassifier(ChangeTermDto dto, SourceFlag flag) : base(dto)
        {
            _flag = flag;
            DynamicInitLabelUtility(flag);
        }

        private void DynamicInitLabelUtility(SourceFlag flag)
        {
            labelUtility = flag switch
            {
                SourceFlag.Teams => new TeamsLabelUtility(true),
                _ => new SPOLabelUtility(true)
            };
        }

        public override async System.Threading.Tasks.Task ChangeTermsAsync(List<Record> records)
        {
            try
            {
                using (new PerformanceScope("RMExplorerUtility.ChangeTermForSPFolder"))
                {
                    logger.Info("Change term action start {0}");
                    var startTime = DateTime.Now;
                    var recDic = records.GroupBy(r => r.AveSiteId).ToDictionary(z => z.Key, p => p.ToList());
                    var avesiteIds = recDic.Keys.ToList();
                    Dictionary<string, RemoteSiteCollection> siteDic = new Dictionary<string, RemoteSiteCollection>();
                    List<Guid> failedIds = new List<Guid>();
                    List<Guid> successIds = new List<Guid>();
                    List<Record> successRecords = new List<Record>();
                    if (avesiteIds.Count > 0)
                    {
                        string termName = RMSPReclassifierCache.Instance.Term.Name;
                        Guid termId = RMSPReclassifierCache.Instance.Term.UniqueId;
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
                                        successRecords = ChangeRecordTermAction(spSite, columnName, recList, termName, termId, ref failedIds);
                                        successIds = successRecords.Select(a => a.Id).ToList();
                                        logger.Warn($"[Change Term] 4. time elapsed for ChangeRecordTermAction {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                        startTime = DateTime.Now;
                                        if (successIds.Count > 0)
                                        {
                                            var previousTermId = Guid.Empty;
                                            _explorerDao.UpdateAll(r => successIds.Contains(r.Id), rec =>
                                            {
                                                previousTermId = rec.TermId;
                                                rec.TermId = termId;
                                                rec.TermName = termName;
                                                rec.RuleId = Guid.Empty;
                                                rec.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                rec.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                                rec.RecordOwner_Array = rec.RecordOwner.ExplorerSearchSplit();
                                                if(base.isNewLogicAccount && previousTermId != termId) rec.RemoveManualFields();
                                            });
                                        }
                                        logger.Warn($"[Change Term] 5. time elapsed for updating cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                        //Add reclassify history...
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
                                            mSucceedCount += successIds.Count;
                                            RecordsHistoryService.AddRecordsHistory(successIds, "RM_BCM_Audit_Action_ChangeTerm", _jobContextDto.Comment);
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
                                        foreach (var rd in recList)
                                        {
                                            if (rd.SourceFlag == 2)
                                            {
                                                recIds.Add(rd.Id);
                                            }
                                        }
                                        var term = RMSPReclassifierCache.Instance.Term;
                                        if (term != null)
                                        {
                                            _explorerDao.UpdateAll(r => recIds.Contains(r.Id), rec =>
                                            {
                                                rec.TermId = term.UniqueId;
                                                rec.TermName = term.Name;
                                                rec.RuleId = Guid.Empty;
                                                rec.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                            });
                                        }

                                    }
                                }
                                catch (Exception ee)
                                {
                                    failedIds.AddRange(recList.Select(t => t.Id));
                                    logger.Warn("change term action failed {0}", ee.ToString());
                                    if (mNeedSendReport)
                                    {
                                        foreach (var record in recList)
                                        {
                                            AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Failed, GetRealException(ee), record.ExtensionForFile != "RM_RDM_RecordDetails_DataType_SPItem");
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (failedIds.Count > 0)
                    {
                        mFailedCount += failedIds.Count;
                        RecordsHistoryService.AddRecordsHistory(failedIds, "RM_JS_Audit_ChangeTermErrorMessage");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("change term error:{0}", ex.ToString());
                throw ex;
            }
            finally
            {
                if (labelUtility != null && labelUtility.LabelApplied)
                {
                    await labelUtility.AddLabelHistoryAsync();
                }
                logger.Info("Change term action finish");
            }
        }

        public List<Record> ChangeRecordTermAction(IAveSite site, string bcsColumnName, List<Record> records, string termName, Guid termId, ref List<Guid> failedIds)
        {
            List<Record> successRecords = new List<Record>();
            IAveWeb web = null;
            IAveList list = null;
            IAveTaxonomyField field = null;
            try
            {
                foreach (var record in records)
                {
                    logger.Info("change term action {0}:{1}", record.Id, termName);
                    bool isDocument = false;
                    try
                    {
                        if (base.NeedSkip(record))
                        {
                            logger.Info($"skip change term action.");
                            continue;
                        }
                        if (base.IsProcessedFolder(record))
                        {
                            logger.Info($"Folder has already been processed. id:{record.Id}");
                            continue;
                        }
                        if (web == null || web != null && web.ID != record.WebId)
                        {
                            web = site.OpenWeb(record.WebId);
                        }
                        if (list == null || list != null && list.ID != record.ListId)
                        {
                            list = web.GetList(record.ListId);
                            field = GetBCSField(list, bcsColumnName);
                        }
                        if (!InSameTermScope(termId, field))
                        {
                            throw new Exception("RM_FS_FolderReclassify_FileNotInSameTermScope");
                        }
                        isDocument = list.BaseTemplate == AveListTemplateType.DocumentLibrary || list.BaseType == AveBaseType.DocumentLibrary;
                        IAveListItem item = base.GetAveListItem(record, list);
                        //isDocument = IsDocument(item);
                        UpdateTerm(item, field, termName, termId);
                        successRecords.Add(record);
                        base.AddProcessedFolderId(record);
                        bool labelNotExist = labelUtility.UpdateLabel(item, termId, record.Id, record.TermId);
                        if (mNeedSendReport)
                        {
                            AddReclassifyDetailForGlobalSearch(record, labelNotExist ? JobDetailsStatus.Failed : JobDetailsStatus.Successful, labelNotExist ? "RM_SPO_ApplySetting_LabelNotExist" : "", isDocument);
                            if (labelNotExist)
                            {
                                mFailedCount++;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        JobDetailsStatus _status = JobDetailsStatus.Failed;
                        if (isItemNotFoundError(e))
                        {
                            _status = JobDetailsStatus.Skipped;
                            UpdateRemoveItem(record);
                        }
                        else if (isKnownIssue(e))
                        {
                            _status = JobDetailsStatus.Skipped;
                        }
                        else
                        {
                            failedIds.Add(record.Id);
                        }
                        if (mNeedSendReport)
                        {
                            AddReclassifyDetailForGlobalSearch(record, _status, GetRealException(e), isDocument);
                        }
                        logger.Warn("update item term failed {0}:{1} error {2}", record?.Id, record.TermName, e.ToString());
                    }
                }
            }
            finally
            {
                try
                {
                    currentAveSite?.Dispose();
                    currentAveWeb?.Dispose();
                }
                catch (Exception ce)
                {
                    logger.Warn("Disposal current context error {0}", ce.ToString());
                }
            }
            return successRecords;
        }

        protected virtual string GetBCSColumn(RemoteSiteCollection site)
        {
            var webApp = RABrowserClient.GetWebApplicationById(site.parentId);
            var groupLevelSetting = SharePointSettingDao.GetGroupLevelGlobalSetting(webApp.url, new Guid(webApp.id));
            var columnName = groupLevelSetting.IsUsingExistColumnName ? groupLevelSetting.ExistColumnName : groupLevelSetting.ColumnName;
            return columnName;
        }

        private void UpdateRemoveItem(Record removeRecordInDB)
        {
            try
            {
                if (removeRecordInDB != null)
                {
                    logger.Info("Catch item not found error, remove it from explorer.");
                    if (removeRecordInDB.RecordStatus == (int)RMRecordStatus.Active)
                    {
                        _explorerDao.UpdateRecordState(removeRecordInDB, (int)RMRecordStatus.RMDeleted);
                        logger.Info("update record state to 3, siteId: {0}, Unique ID: {1}, itemId: {2}", removeRecordInDB.ScopeId, removeRecordInDB.RecordsId, removeRecordInDB.ItemRowId);
                    }
                    else
                    {
                        logger.Warn("sp object already archived, siteId: {0}, Unique ID: {1}, itemId: {2}", removeRecordInDB.ScopeId, removeRecordInDB.RecordsId, removeRecordInDB.ItemRowId);
                    }
                }
                else
                {
                    logger.Warn("record is null");
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }
        private bool InSameTermScope(Guid termId, IAveTaxonomyField field)
        {
            try
            {
                if (field.AnchorId == Guid.Empty)
                {
                    //term scope is termset
                    var sourceTermSet = currentAveSite.AveSPTaxonomySession.GetTerm(termId).TermSet;
                    return sourceTermSet.ID.Equals(field.TermSetId) ? true : false;
                }
                else
                {
                    //term scope is term
                    var destinationTerm = currentAveSite.AveSPTaxonomySession.GetTerm(field.AnchorId);
                    if (destinationTerm == null)
                    {
                        return false;
                    }
                    //check if in the same termset
                    var sourceTerm = currentAveSite.AveSPTaxonomySession.GetTerm(termId);
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

        public void UpdateTerm(IAveListItem item, IAveTaxonomyField taxField, string termName, Guid termId)
        {
            IAveTaxonomyFieldValue taxValue = taxField.TaxonomyFieldValue;
            taxValue.TermGuid = termId.ToString();
            taxValue.Label = termName;
            item[taxField.ID] = taxValue;
            item[taxField.TextField] = taxValue.ToString();
            item.SystemUpdateForRecords();
        }
        private IAveTaxonomyField GetBCSField(IAveList list, string columnName)
        {
            IAveTaxonomyField taxField = null;
            var tempField = list.Fields.GetRecordTaxonomyField(columnName);
            if (tempField != null)
            {
                taxField = tempField;
            }
            else
            {
                var bcsColumn = list.Fields.GetFieldById(RevIMClassificationColumnID, false);
                if (bcsColumn != null)
                {
                    taxField = bcsColumn as IAveTaxonomyField;
                }
            }
            return taxField;
        }
        private bool isItemNotFoundError(Exception e)
        {
            if (e.Message != null && e.Message.Contains("Item does not exist"))
            {
                return true;
            }
            if (e.InnerException != null)
            {
                return isItemNotFoundError(e.InnerException);
            }
            return false;
        }
        private void AddReclassifyDetailForGlobalSearch(Record record, JobDetailsStatus status, string comment, bool isDocument)
        {
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new Contract.RMWeb.JobMonitor.JMGlobalSearchActionJobDetails()
            {
                ObjectName = record?.LeafName,
                FullPath = record == null ? "" : currentAveSite == null ? record.DirPath : WebUtil.MakeFullUrl(currentAveSite.Url, record.DirPath),
                Action = "RM_JS_BCM_Explorer_ChangeTerm",
                Status = status,
                Comment = comment,
                Type = GetItemTypeI18N(record, isDocument)
            });
        }

        private bool isKnownIssue(Exception e)
        {
            var knownIssues = new List<string>
            {
                "To update this folder, go to the channel in Microsoft Teams"
            };
            if (e.Message != null && knownIssues.Any(i => e.Message.Contains(i)))
            {
                return true;
            }
            if (e.InnerException != null)
            {
                return isKnownIssue(e.InnerException);
            }
            return false;
        }

    }
}
