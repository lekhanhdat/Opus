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
using AvePoint.RA.CommonUtil;
using AvePoint.Hybrid.Utility.Configuration;
using AvePoint.RA.Common.Global.Util;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.RMExplorer.Extension;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using AvePoint.GCommon;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class RMExplorerUtility
    {
        protected AveLogger logger = AveLogger.GetInstance(typeof(RMExplorerUtility));
        static string[] stopWords = new string[] { "a", "an", "are", "as", "at", "be", "by", "for", "in", "is", "it", "of", "on", "or", "the", "to", "was", "will", "with", "the" };
        static char[] seperator = new char[] { ' ', '<', '{', '>', ' ', ',', '_', '|', '"', '\'', '/', '\\', ':', ';', '(', ')', '-', '\n', '\t', '}', '[', ']', '=', '+', '~', '&', '@' };
        private const string BLOCK_DELETE_AND_EDIT = "BlockDelete, BlockEdit";
        private const string ROOTWEB_DECLARE_SETTING_PROPERTY = "ecm_siterecordrestrictions";
        public IProgressService ProgressService { get; set; }
        public IReportService<JMJobDetails> JobDetailService { get; set; }
        protected Guid RevIMClassificationColumnID
        {
            get
            {
                return new Guid("20f84bba906045b4af568ee102a52dcb");
            }
        }
        #region use wrapper method to declared records.
        private AveObjectModelFactory aveObjectModelFactory = null;
        private IAveORecords mRecord = null;
        private IAveSite currentAveSite = null;
        private IAveWeb currentAveWeb = null;
        private IAveList currentAveList = null;
        #endregion
        #region use client api to update term value ,because wrapper update item method doesn't change modify time.
        //private ClientContext currentContext = null;
        //private TaxonomySession taxonomySession = null;
        private AvePoint.RA.Contract.Global.JobMessage.SiteInfo siteCollection = null;
        private string columnName = string.Empty;
        //private ClientContext currentContext = null;
        //private Site currentSite = null;
        //private Web currentWeb = null;
        //private List currentList = null;
        private Guid mRecordFeatureId = new Guid("da2e115b-07e4-49d9-bb2c-35e93bb9fca9");
        private bool mNeedSendReport = false;
        public int FailedCount = 0;
        private readonly Guid BCSColumnID = new Guid("20f84bba906045b4af568ee102a52dcb");
        private MemoryListCacheService<AvePoint.RA.Contract.Global.Object.RMClassificationHistory> mHistoryCache;
        #endregion

        public RMExplorerUtility()
        {
            mHistoryCache = new MemoryListCacheService<Contract.Global.Object.RMClassificationHistory>();
        }
        public RMExplorerUtility(bool needSendReport)
        {
            mNeedSendReport = needSendReport;
            mHistoryCache = new MemoryListCacheService<Contract.Global.Object.RMClassificationHistory>();
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
        }
        public RMExplorerUtility(RemoteSiteCollection site)
        {
            //currentContext = InitContext(site);
        }

        public RMExplorerUtility(string fullPath)
        {
            //currentContext = InitContext(fullPath);
        }

        public string GetBcsColumnName(RemoteSiteCollection site)
        {
            //return SharePointSettingDao.GetBcsColumnName(site);
            return string.Empty;
        }


        public void ChangeAllTerms(AvePoint.RA.Contract.Global.JobMessage.ChangeTermOption changeTermInfo, string tempJobId, bool waiting4OtherSource)
        {
            try
            {
                logger.Info("Change term action start {0}", tempJobId);
                List<AvePoint.RA.Contract.Explorer.RecordDto> records = new List<AvePoint.RA.Contract.Explorer.RecordDto>();

                HybridApiClient.Instance.UpdateRealtimeJobState(new Contract.Global.Object.RealtimeJobState()
                {
                    Jobid = tempJobId,
                    Status = 3
                });

                if (changeTermInfo.SourceSPOnPremRecordIds != null && changeTermInfo.SourceSPOnPremRecordIds.Count > 0)
                {
                    var startTime = DateTime.Now;
                    //using (new RA.Common.PerformanceScope(string.Format("change.Term.GetRecords")))
                    {
                        //var simpleRecords = ExplorerDao.QueryAllSimple(r => changeTermInfo.SourceRecordIds.Contains(r.Id)).ToList();
                        //logger.Warn($"0. time elapsed for query {simpleRecords.Count} simple records from cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                        //startTime = DateTime.Now;
                        records = HybridApiClient.Instance.GetRecordsByIds(changeTermInfo.SourceSPOnPremRecordIds);
                        records = records.Where(r => r.SourceFlag == (int)SourceFlag.SharePointOnPrem).ToList();
                        // ExplorerDao.QueryAll(r => changeTermInfo.SourceRecordIds.Contains(r.Id)).ToList();
                        logger.Warn($"[Change Term] 1. time elapsed for query {records.Count} records from cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                        HybridApiClient.Instance.UpdateRealtimeJobState(new Contract.Global.Object.RealtimeJobState()
                        {
                            Jobid = tempJobId,
                            Status = 3,
                            StartItems = JsonConvert.SerializeObject(records.Select(r => r.LeafName).ToList())
                        });
                    }

                    var recDic = records.GroupBy(r => r.AveSiteId).ToDictionary(z => z.Key, p => p.ToList());
                    var avesiteIds = recDic.Keys.ToList();
                    Dictionary<string, AvePoint.RA.Contract.Global.JobMessage.SiteInfo> siteDic = new Dictionary<string, AvePoint.RA.Contract.Global.JobMessage.SiteInfo>();
                    List<Guid> failedIds = new List<Guid>();
                    List<Guid> successIds = new List<Guid>();
                    List<Contract.Explorer.RecordDto> successRecords = new List<Contract.Explorer.RecordDto>();
                    if (avesiteIds.Count > 0)
                    {
                        string termName = changeTermInfo.TargetTermName;
                        Guid termId = changeTermInfo.TargetTermUniqueId;
                        siteDic = HybridApiClient.Instance.GetOnPremiseSiteInfos(avesiteIds);
                        foreach (var recList in recDic.Values)
                        {
                            if (recList.Count > 0)
                            {
                                string siteUrl = string.Empty;
                                try
                                {
                                    if (mNeedSendReport)
                                    {
                                        ProgressService.IncreaseBase(recList.Count);
                                    }
                                    if (siteDic.ContainsKey(recList[0].AveSiteId))
                                    {
                                        var site = siteDic[recList[0].AveSiteId];
                                        siteUrl = site.SiteUrl;
                                        startTime = DateTime.Now;
                                        //InitContext(site);
                                        //logger.Warn($"[Change Term] 3. time elapsed for initContext  {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                        //startTime = DateTime.Now;

                                        var bposInfo = GetBPOSInfo();
                                        logger.Warn($"[Declare] 3.time elapsed for GetBPOSInfo {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                        startTime = DateTime.Now;
                                        var factory = AveObjectModelFactory.CreateObjectModelFactory(site.SiteUrl, bposInfo, AveContextKind.ClientObjectModel);
                                        var spSite = factory.CreateSite();
                                        currentAveSite = spSite;
                                        successRecords = ChangeRecordTermAction(spSite, site.BCSColumnName, recList, termName, termId, ref failedIds);
                                        successIds = successRecords.Select(a => a.Id).ToList();
                                        logger.Warn($"[Change Term] 4. time elapsed for ChangeRecordTermAction {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                        //ExplorerDao.ChangeTerm(successIds, termInfo.UniqueId);
                                        startTime = DateTime.Now;

                                        if (successIds != null && successIds.Count > 0)
                                        {
                                            try
                                            {
                                                AvePoint.RA.Contract.Global.Explorer.TermChangeItemDto termChangeItemDto = new Contract.Global.Explorer.TermChangeItemDto()
                                                {
                                                    Ids = successIds,
                                                    TermId = termId,
                                                    TermName = termName
                                                };
                                                var result = HybridApiClient.Instance.UpdateTermChangeItems(termChangeItemDto);
                                            }
                                            catch (Exception e)
                                            {
                                                logger.Warn("An error occurred while updating TermChangeItemDto. Error:{0}", e.ToString());
                                            }
                                        }

                                        logger.Warn($"[Change Term] 5. time elapsed for updating cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                        //Add reclassify history...
                                        foreach (var tempRecord in successRecords)
                                        {
                                            var history = new AvePoint.RA.Contract.Global.Object.RMClassificationHistory()
                                            {
                                                RecordId = tempRecord.Id,
                                                PreviousTermId = tempRecord.TermId,
                                                NewTermId = termId,
                                                OperationTime = DateTime.UtcNow.Ticks
                                            };
                                            AddClassificationHistoryToCache(history);
                                        }
                                        logger.Warn($"[Change Term] 6. time elapsed for updating cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                    }
                                    else
                                    {
                                        List<Guid> recIds = new List<Guid>();
                                        if (recList[0].SourceFlag == 1)
                                        {
                                            throw new Exception("can't get site obj");
                                        }
                                    }
                                }
                                catch (Exception ee)
                                {
                                    failedIds.AddRange(recList.Select(t => t.Id));
                                    logger.Warn("change term action failed {0}", ee.ToString());
                                    if (mNeedSendReport)
                                    {
                                        JobDetailService.Commit(new AvePoint.RA.Contract.Global.RMWeb.JobMonitor.JMGlobalSearchActionJobDetails()
                                        {
                                            ObjectName = GetSiteTitle(siteUrl),
                                            FullPath = siteUrl,
                                            Action = "RM_JS_BCM_Explorer_ChangeTerm",
                                            Status = JobDetailsStatus.Failed,
                                            Comment = ee.Message,
                                            Type = "RM_JS_Rule_ObjectLevel_SiteCollection"
                                        });
                                    }
                                }
                            }
                        }
                    }
                    if (successIds.Count > 0)
                    {
                        //xml.HistoryList[0].Action = "RM_BCM_Audit_Action_ChangeTerm";
                        //ExplorerDao.AddReocrdHistory(successIds, xml);
                        try
                        {
                            HybridApiClient.Instance.AddRecordHistory(new Contract.Global.Explorer.RecordHistoryDto()
                            {
                                CurrentIds = successIds,
                                historyAction = "RM_BCM_Audit_Action_ChangeTerm",
                                LogonUser = changeTermInfo.LogonUser,
                                Comment = changeTermInfo.Comment
                            });
                        }
                        catch (Exception e)
                        {
                            logger.Warn("An error occurred while updating RecordHistoryDto. Error:{0}", e.ToString());
                        }
                        startTime = DateTime.Now;
                        logger.Warn($"[Change Term] 6. time elapsed for AddReocrdHistory(succeed) to cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                    }
                    if (failedIds.Count > 0)
                    {
                        FailedCount += failedIds.Count;
                        string failedNames = string.Empty;
                        foreach (var fid in failedIds)
                        {
                            failedNames += records.Where(t => t.Id == fid).FirstOrDefault().LeafName + ";";
                        }
                        failedNames = failedNames.TrimEnd(';');
                        try
                        {
                            HybridApiClient.Instance.AddRecordHistory(new Contract.Global.Explorer.RecordHistoryDto()
                            {
                                CurrentIds = failedIds,
                                historyAction = "RM_JS_Audit_ChangeTermErrorMessage",
                                LogonUser = changeTermInfo.LogonUser
                            });
                        }
                        catch (Exception e)
                        {
                            logger.Warn("An error occurred while updating RecordHistoryDto. Error:{0}", e.ToString());
                        }
                        //RecordsHistoryService.AddRecordsHistory(failedIds, "RM_JS_Audit_ChangeTermErrorMessage");
                        //throw new Exception("have failed record in change term action");
                        if (!mNeedSendReport)
                        {
                            throw new Exception(string.Format(I18NEntity.GetString("RM_RDM_Explorer_ChangeTermError"), failedIds));
                        }
                    }
                    else
                    {
                        HybridApiClient.Instance.UpdateRealtimeJobState(new Contract.Global.Object.RealtimeJobState()
                        {
                            Jobid = tempJobId,
                            Status = 4
                        });
                        // RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Finished);
                    }
                }
            }
            catch (Exception ex)
            {
                HybridApiClient.Instance.UpdateRealtimeJobState(new Contract.Global.Object.RealtimeJobState()
                {
                    Jobid = tempJobId,
                    Status = 1
                });
                // RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Failed_Partial);
                logger.Error("change term error:{0}", ex.ToString());
            }
            finally
            {
                FinalAddClassificationHistoryToCache();
            }
        }

        private string[] ExplorerSearchSplit(string name)
        {
            if (name == null || name == string.Empty)
            {
                return null;
            }
            string[] terms = name.Split(seperator).ToArray();
            List<string> temp = new List<string>();
            foreach (string t in terms)
            {
                if (string.IsNullOrEmpty(t))
                {
                    continue;
                }
                if (t.Contains('.'))
                {
                    double output = 0.0;
                    if (double.TryParse(t, out output))
                    {
                        temp.Add(t);
                    }
                    string[] subterms = t.Split('.');
                    foreach (string sub in subterms)
                    {
                        string lowerSub = sub.ToLower();
                        if (!stopWords.Contains(lowerSub))
                        {
                            temp.Add(lowerSub);
                        }
                    }
                }
                else
                {
                    string lowerT = t.ToLower();
                    if (!stopWords.Contains(lowerT))
                    {
                        temp.Add(lowerT);
                    }
                }
            }
            string[] result = temp.Distinct().ToArray();
            if (result.Length > 0)
            {
                return result;
            }
            return new string[] { name.ToLower() };
        }

        private void AddClassificationHistoryToCache(AvePoint.RA.Contract.Global.Object.RMClassificationHistory history)
        {
            mHistoryCache.Add(history);
            if (mHistoryCache.Count > 100)
            {
                try
                {
                    var histories = mHistoryCache.Take(100).ToList();
                    HybridApiClient.Instance.AddClassificationHistory(histories);
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while AddClassificationHistory. Error:{0}", e.ToString());
                }
            }
        }

        private void FinalAddClassificationHistoryToCache()
        {
            if (mHistoryCache.Count > 0)
            {
                try
                {
                    var histories = mHistoryCache.TakeAll().ToList();
                    HybridApiClient.Instance.AddClassificationHistory(histories);
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while AddClassificationHistory. Error:{0}", e.ToString());
                }
            }
        }
        public void DeclaredRecords(List<Guid> recordIds, string tempJobId, bool isDeclare, string declaredBy)
        {
            try
            {
                logger.Info("Declared Records action start {0}", tempJobId);
                HybridApiClient.Instance.UpdateRealtimeJobState(new Contract.Global.Object.RealtimeJobState()
                {
                    Jobid = tempJobId,
                    Status = 3
                });
                var startTime = DateTime.Now;
                List<Contract.Explorer.RecordDto> records = new List<Contract.Explorer.RecordDto>();
                if (recordIds != null && recordIds.Count > 0)
                {
                    records = HybridApiClient.Instance.GetRecordsByIds(recordIds);
                    records = records.Where(r => r.SourceFlag == (int)SourceFlag.SharePointOnPrem).ToList();
                    //ExplorerDao.QueryAll(r => recordIds.Contains(r.Id)).ToList();
                    logger.Warn($" [Declare] 1.time elapsed for query {records.Count} records from cosmos {(DateTime.Now - startTime).TotalMilliseconds} ms");
                    //records = CollectionDataDao.GetRecordByIds(RecordIds);//to do
                    // RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running, JsonConvert.SerializeObject(records.Select(r => r.LeafName).ToList()));

                    HybridApiClient.Instance.UpdateRealtimeJobState(new Contract.Global.Object.RealtimeJobState()
                    {
                        Jobid = tempJobId,
                        Status = 3,
                        StartItems = JsonConvert.SerializeObject(records.Select(r => r.LeafName).ToList())
                    });
                    var recDic = records.GroupBy(r => r.AveSiteId).ToDictionary(z => z.Key, p => p.ToList());
                    var avesiteIds = recDic.Keys.ToList();
                    Dictionary<string, AvePoint.RA.Contract.Global.JobMessage.SiteInfo> siteDic = new Dictionary<string, AvePoint.RA.Contract.Global.JobMessage.SiteInfo>();
                    List<Guid> failedIds = new List<Guid>();
                    if (avesiteIds.Count > 0)
                    {
                        startTime = DateTime.Now;
                        siteDic = HybridApiClient.Instance.GetOnPremiseSiteInfos(avesiteIds);
                        logger.Warn($"[Declare] 2.time elapsed for query from DAO {(DateTime.Now - startTime).TotalMilliseconds} ms");
                        foreach (var recList in recDic.Values)
                        {
                            if (recList.Count > 0)
                            {
                                string siteUrl = string.Empty;
                                try
                                {
                                    if (mNeedSendReport)
                                    {
                                        ProgressService.IncreaseBase(recList.Count);
                                    }
                                    var site = siteDic[recList[0].AveSiteId];
                                    siteUrl = site.SiteUrl;
                                    startTime = DateTime.Now;
                                    var bposInfo = GetBPOSInfo();
                                    logger.Warn($"[Declare] 3.time elapsed for GetBPOSInfo {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                    startTime = DateTime.Now;
                                    var factory = AveObjectModelFactory.CreateObjectModelFactory(site.SiteUrl, bposInfo, AveContextKind.ClientObjectModel);
                                    var spSite = factory.CreateSite();
                                    logger.Warn($"[Declare] 4.1.time elapsed for CreateSite {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                    startTime = DateTime.Now;
                                    var IRecords = factory.CreateRecords();
                                    logger.Warn($"[Declare] 4.2.time elapsed for CreateRecords {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                    startTime = DateTime.Now;
                                    spSite.EnsureRecordFeatureEnabled(mRecordFeatureId);
                                    logger.Warn($"[Declare] 4.3.time elapsed for EnsureRecordFeatureEnabled {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                    if (isDeclare)
                                    {
                                        startTime = DateTime.Now;
                                        List<Guid> successIds = DeclaredRecord(IRecords, spSite, recList, ref failedIds);
                                        logger.Warn($"[Declare] 5.time elapsed for declare record {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                        startTime = DateTime.Now;

                                        if (successIds != null && successIds.Count > 0)
                                        {
                                            try
                                            {
                                                AvePoint.RA.Contract.Global.Explorer.DeclareItemDto declareItemDto = new Contract.Global.Explorer.DeclareItemDto()
                                                {
                                                    Ids = successIds,
                                                    DeclaredBy = declaredBy,
                                                    IsDeclare = true
                                                };
                                                HybridApiClient.Instance.UpdateDeclaredItems(declareItemDto);
                                            }
                                            catch (Exception e)
                                            {
                                                logger.Warn("An error occurred while updating DeclareItemDto. Error:{0}", e.ToString());
                                            }

                                            try
                                            {
                                                if (mNeedSendReport)
                                                {
                                                    HybridApiClient.Instance.AddRecordHistory(new Contract.Global.Explorer.RecordHistoryDto()
                                                    {
                                                        CurrentIds = successIds,
                                                        historyAction = "RM_BCM_History_DeclareAsRecord",
                                                        LogonUser = declaredBy
                                                    });
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                logger.Warn("An error occurred while updating RecordHistoryDto. Error:{0}", e.ToString());
                                            }
                                        }
                                        // ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec => { rec.DeclareAsRecord = true; rec.DeclaredBy = declaredBy; });
                                        logger.Warn($"[Declare] 6.time elapsed for update cosmos record {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                    }
                                    else
                                    {
                                        startTime = DateTime.Now;
                                        List<Guid> successIds = UnDeclaredRecord(IRecords, spSite, recList, ref failedIds);
                                        logger.Warn($"[UnDeclare] 5.time elapsed for undeclare records {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                        startTime = DateTime.Now;
                                        if (successIds != null && successIds.Count > 0)
                                        {
                                            try
                                            {
                                                AvePoint.RA.Contract.Global.Explorer.DeclareItemDto declareItemDto = new Contract.Global.Explorer.DeclareItemDto()
                                                {
                                                    Ids = successIds,
                                                    DeclaredBy = declaredBy,
                                                    IsDeclare = false
                                                };
                                                HybridApiClient.Instance.UpdateDeclaredItems(declareItemDto);
                                            }
                                            catch (Exception e)
                                            {
                                                logger.Warn("An error occurred while updating DeclareItemDto. Error:{0}", e.ToString());
                                            }

                                            try
                                            {
                                                if (mNeedSendReport)
                                                {
                                                    HybridApiClient.Instance.AddRecordHistory(new Contract.Global.Explorer.RecordHistoryDto()
                                                    {
                                                        CurrentIds = successIds,
                                                        historyAction = "RM_BCM_History_UndeclareAsRecord",
                                                        LogonUser = declaredBy
                                                    });
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                logger.Warn("An error occurred while updating RecordHistoryDto. Error:{0}", e.ToString());
                                            }
                                        }
                                        //ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec => { rec.DeclareAsRecord = false; rec.DeclaredBy = declaredBy; });
                                        logger.Warn($"[UnDeclare] 6.time elapsed for update cosmos record {(DateTime.Now - startTime).TotalMilliseconds} ms");
                                    }
                                }
                                catch (Exception ee)
                                {
                                    failedIds.AddRange(recList.Select(t => t.Id));
                                    logger.Warn("Declared Records action failed {0}", ee.ToString());
                                    if (mNeedSendReport)
                                    {
                                        JobDetailService.Commit(new AvePoint.RA.Contract.Global.RMWeb.JobMonitor.JMGlobalSearchActionJobDetails()
                                        {
                                            ObjectName = GetSiteTitle(siteUrl),
                                            FullPath = siteUrl,
                                            Action = isDeclare ? "RM_BCM_History_DeclareAsRecord" : "RM_RDM_CreateRule_Options_UndeclareDocumnet",
                                            Status = JobDetailsStatus.Failed,
                                            Comment = ee.Message,
                                            Type = "RM_JS_Rule_ObjectLevel_SiteCollection"
                                        });
                                    }
                                }
                            }
                        }
                    }
                    if (failedIds.Count > 0)
                    {
                        FailedCount += failedIds.Count;
                        string failedNames = string.Empty;
                        foreach (var fid in failedIds)
                        {
                            failedNames += records.Where(t => t.Id == fid).FirstOrDefault().LeafName + "; ";
                        }
                        if (!string.IsNullOrEmpty(failedNames))
                        {
                            failedNames = failedNames.Trim().TrimEnd(';');
                        }
                        //RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, failedNames);
                        if (!mNeedSendReport)
                        {
                            throw new Exception(failedNames);
                        }
                    }
                    else
                    {
                        HybridApiClient.Instance.UpdateRealtimeJobState(new Contract.Global.Object.RealtimeJobState()
                        {
                            Jobid = tempJobId,
                            Status = 4
                        });
                        // RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Finished);
                    }
                }
            }
            catch (Exception ex)
            {
                HybridApiClient.Instance.UpdateRealtimeJobState(new Contract.Global.Object.RealtimeJobState()
                {
                    Jobid = tempJobId,
                    Status = 1
                });
                //RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Failed_Partial);
                logger.Error("Declare record error:{0}", ex.ToString());
            }
        }

        private string GetSiteTitle(string siteUrl)
        {
            if (!string.IsNullOrWhiteSpace(siteUrl))
            {
                int index = siteUrl.LastIndexOf('/');
                if (index > 0)
                {
                    return siteUrl.Substring(index + 1, siteUrl.Length - index - 1);
                }
                else
                {
                    return siteUrl;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        private AveBPOSAccountInfo GetBPOSInfo()
        {
            var account = AgentAccountUtil.Get();
            AveBPOSAccountInfo aveBPOSAccountInfo = new AveBPOSAccountInfo()
            {
                Domain = account.Domain,
                UserName = account.UserName,
                Password = account.Password
            };

            return aveBPOSAccountInfo;

        }

        private void AddDeclareDetailForGlobalSearchJob(IAveListItem item, JobDetailsStatus status, string comment, bool isDeclare)
        {
            JobDetailService.Commit(new AvePoint.RA.Contract.Global.RMWeb.JobMonitor.JMGlobalSearchActionJobDetails()
            {
                ObjectName = item?.Name,
                FullPath = item?.FullPath(),
                Action = isDeclare ? "RM_BCM_History_DeclareAsRecord" : "RM_RDM_CreateRule_Options_UndeclareDocumnet",
                Status = status,
                Comment = comment,
                Type = item == null ? "" : item.File != null ? "RM_JS_Rule_CreateRule_FilterLevel_Document" : "RM_JS_Rule_ObjectLevel_Item"
            });
        }

        private void AddReclassifyDetailForGlobalSearch(Contract.Explorer.RecordDto record, JobDetailsStatus status, string comment, bool isDocument)
        {
            JobDetailService.Commit(new AvePoint.RA.Contract.Global.RMWeb.JobMonitor.JMGlobalSearchActionJobDetails()
            {
                ObjectName = record?.LeafName,
                FullPath = record == null ? "" : WebUtil.MakeFullUrl(currentAveSite.Url, record.DirPath),
                Action = "RM_JS_BCM_Explorer_ChangeTerm",
                Status = status,
                Comment = comment,
                Type = isDocument ? "RM_JS_Rule_CreateRule_FilterLevel_Document" : "RM_RDM_RecordDetails_DataType_SPItem"
            });
        }
        // this method all records from same site

        public List<Contract.Explorer.RecordDto> ChangeRecordTermAction(IAveSite site, string bcsColumnName, List<Contract.Explorer.RecordDto> records, string termName, Guid termId, ref List<Guid> failedIds)
        {
            IAveWeb web = null;
            IAveList list = null;
            IAveTaxonomyField field = null;
            List<Contract.Explorer.RecordDto> successRecords = new List<Contract.Explorer.RecordDto>();
            try
            {
                foreach (var record in records)
                {

                    logger.Info("change term action {0}:{1}", (record.DirPath + "/" + record.LeafName).LogBase64(), termName);
                    bool isDocument = false;
                    try
                    {
                        if (web == null || (web != null && web.ID != record.WebId))
                        {
                            web = site.OpenWeb(record.WebId);
                        }
                        if (list == null || (list != null && list.ID != record.ListId))
                        {
                            list = web.GetList(record.ListId);
                            field = GetBCSField(list, bcsColumnName);
                        }
                        IAveListItem item = list.GetItemByUniqueId(record.ItemId);
                        isDocument = IsDocument(item);
                        UpdateTerm(item, field, termName, termId);
                        successRecords.Add(record);
                        if (mNeedSendReport)
                        {
                            AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Successful, "", isDocument);
                        }
                    }
                    catch (Exception e)
                    {
                        failedIds.Add(record.Id);
                        if (mNeedSendReport)
                        {
                            AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Failed, e.Message, isDocument);
                        }
                        logger.Warn("update item term failed {0}:{1} error {2}", (record.DirPath + "/" + record.LeafName).LogBase64(), record.TermName, e.ToString());
                    }
                    finally
                    {
                        if (mNeedSendReport)
                        {
                            ProgressService.Increase();
                        }
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

        private IAveTaxonomyField GetBCSField(IAveList list, string columnName)
        {
            IAveTaxonomyField taxField = null;

            var tempField = list.Fields.Where(f => f.Title == columnName).FirstOrDefault();
            if (tempField != null)
            {
                taxField = tempField as IAveTaxonomyField;
            }
            else
            {
                var bcsColumn = list.Fields.GetFieldById(BCSColumnID, false);
                if (bcsColumn != null)
                {
                    taxField = bcsColumn as IAveTaxonomyField;
                }
            }
            return taxField;
        }

        private bool IsDocument(IAveListItem item)
        {
            bool isDocument = false;
            try
            {
                if (item.File != null && item.File.Exists)
                {
                    isDocument = true;
                }
            }
            catch (Exception)
            { }
            return isDocument;
        }


        #region Wrapper update terms
        //[Obsolete]
        //public List<int> ChangeTermAction(List<RMBaseRecord> records, string termName, Guid termId, ref List<int> failedIds)
        //{
        //    List<int> successIds = new List<int>();
        //    try
        //    {
        //        foreach (var record in records)
        //        {
        //            try
        //            {
        //                IAveListItem item = GetItem(record);
        //                IAveTaxonomyField field = null;
        //                if (InitFiled(currentAveList, ref field))
        //                {
        //                    UpdateTerm(item, field, termName, termId);
        //                    successIds.Add(record.Id);
        //                }
        //                else
        //                {
        //                    throw new Exception("init bcs column failed");
        //                }

        //            }
        //            catch (Exception e)
        //            {
        //                failedIds.Add(record.Id);
        //                logger.Warn("update item term failed {0}:{1} error {2}", record.FullPath, record.TermName, e.ToString());
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        try
        //        {
        //            currentAveSite.Dispose();
        //            currentAveWeb.Dispose();
        //        }
        //        catch (Exception ce)
        //        {
        //            logger.Warn("Disposal current context error {0}", ce.ToString());
        //        }
        //    }
        //    return successIds;
        //}

        [Obsolete]
        private bool InitFiled(IAveList list, ref IAveTaxonomyField aveTaxField)
        {
            bool success = false;
            try
            {
                if (list.Fields.Contains(RevIMClassificationColumnID))
                {
                    aveTaxField = list.Fields[RevIMClassificationColumnID] as IAveTaxonomyField;
                    success = true;
                    logger.Info("begin to init field, field internalName:{0}, listTitle:{1}", RevIMClassificationColumnID, list.Title);
                }
                else
                {
                    logger.Warn("init field, field not exist in the list ,listTitle:{1}, fieldId:{2}", list.Title, RevIMClassificationColumnID);
                }

            }
            catch (Exception ex)
            {
                logger.Warn("error occurred while init field,ERROR:{0}", ex.ToString());
            }
            return success;
        }
        //[Obsolete]
        public void UpdateTerm(IAveListItem item, IAveTaxonomyField taxField, string termName, Guid termId)
        {

            IAveTaxonomyFieldValue taxValue = taxField.TaxonomyFieldValue;
            taxValue.TermGuid = termId.ToString();
            taxValue.Label = termName;
            item[taxField.ID] = taxValue;
            item[taxField.TextField] = taxValue.ToString();
            item.Update();
        }
        #endregion
        /// <summary>
        /// Declared SharePoint Records.
        /// </summary>
        /// <param name="records"></param>
        public List<Guid> DeclaredRecord(IAveORecords IRecords, IAveSite site, List<Contract.Explorer.RecordDto> records, ref List<Guid> failedIds)
        {
            List<Guid> successIds = new List<Guid>();
            IAveWeb web = null;
            IAveList list = null;
            try
            {
                try
                {
                    if (!site.CheckDeclarationSettingIsBlockEditAndDelete())
                    {
                        //all records in one site
                        //for performance, check site once.
                        //var testRecord = records.First();
                        ////var remoteSite = mDocAveClient.GetRemoteSiteCollectionsByIdList(new List<string> { testRecord.AveSiteId }).FirstOrDefault();
                        ////var remoteSite = RABrowserClient.GetRemoteSiteCollectionsByIdList(new List<string> { testRecord.AveSiteId }).FirstOrDefault();
                        //var bposInfo = GetBPOSInfo();
                        //var factory = AveObjectModelFactory.CreateObjectModelFactory(site.Url, bposInfo, AveContextKind.ClientObjectModel);
                        //IAveTenant tenant = factory.CreateTenant(AveUrlUtility.GetSPOAdminUrlBySiteUrl(bposInfo, site.Url));
                        //var siteProperties = tenant.GetSitePropertiesByUrl(site.Url);
                        //SPCommonUtility.DisableDenyAddAndCustomizePages(siteProperties, site.Url);
                    }
                }
                catch (Exception)
                {
                    failedIds = records != null ? records.Select(r => r.Id).ToList() : failedIds;
                    throw;
                }

                //TODO  --ywhe order by path could help?
                foreach (var record in records)
                {
                    logger.Info("Declared Records {0}", WebUtil.MakeFullUrl(site.Url, record.DirPath).LogBase64());
                    try
                    {
                        if (web == null || (web != null && web.ID != record.WebId))
                        {
                            web = site.OpenWeb(record.WebId);
                        }
                        if (list == null || (list != null && list.ID != record.ListId))
                        {
                            list = web.GetList(record.ListId);
                        }
                        IAveListItem item = list.GetItemByUniqueId(record.ItemId);
                        if (!item.IsBlockEditAndDeleteRecord())
                        {
                            if (item.FieldValues.ContainsKey("CheckoutUser") &&
                                item.FieldValues["CheckoutUser"] != null && !string.IsNullOrEmpty(item.FieldValues["CheckoutUser"].ToString()))
                            {
                                logger.Warn("The file is in Checked out status, cannot be declared now. File id: {0}", item?.ID);
                                failedIds.Add(record.Id);
                                if (mNeedSendReport)
                                {
                                    AddDeclareDetailForGlobalSearchJob(item, JobDetailsStatus.Failed, "RM_JM_GlobalSearch_DeclareCheckout", true);
                                }
                            }
                            else
                            {
                                if (CheckisRecord(item))
                                {
                                    IRecords.UndeclareItemAsRecord(item);
                                }
                                var lockerKey = web.Site.ID.ToString();
                                bool lockStatus = false;
                                try
                                {
                                    // lockStatus = RMGlobalLocker.GetRecordsLocker(lockerKey);
                                    site.EnsureWebDeclarationSetting();
                                    IRecords.DeclareItemAsRecord(item);
                                    successIds.Add(record.Id);
                                    if (mNeedSendReport)
                                    {
                                        AddDeclareDetailForGlobalSearchJob(item, JobDetailsStatus.Successful, string.Empty, true);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    failedIds.Add(record.Id);
                                    if (mNeedSendReport)
                                    {
                                        AddDeclareDetailForGlobalSearchJob(item, JobDetailsStatus.Failed, ex.Message, true);
                                    }
                                    logger.Error("error occurred while process items,ERROR:{0}", ex.ToString());
                                }
                                finally
                                {
                                    if (lockStatus && !lockerKey.IsNullOrEmpty())
                                    {
                                        // RMGlobalLocker.ReleaseRecordsLocker(lockerKey);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Declared Records failed {0},{1}", WebUtil.MakeFullUrl(site.Url, record.DirPath).LogBase64(), e.ToString());
                        failedIds.Add(record.Id);
                        if (mNeedSendReport)
                        {
                            AddDeclareDetailForGlobalSearchJob(null, JobDetailsStatus.Failed, e.Message, true);
                        }
                    }
                    finally
                    {
                        if (mNeedSendReport)
                        {
                            ProgressService.Increase();
                        }
                    }
                }//for each end
            }
            catch (Exception ex)
            {
                logger.Warn("Declared Records failed {0}", ex.ToString());
                if (mNeedSendReport)
                {
                    AddDeclareDetailForGlobalSearchJob(null, JobDetailsStatus.Failed, ex.Message, true);
                }
            }
            finally
            {
                try
                {
                    site.Dispose();
                    web.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn("Dispose sp object failed {0}", e.ToString());
                }
            }
            return successIds;
        }

        /// <summary>
        /// UnDeclared SharePoint Records
        /// </summary>
        /// <param name="records"></param>
        /// <param name="failedIds"></param>
        /// <returns></returns>
        public List<Guid> UnDeclaredRecord(IAveORecords IRecords, IAveSite site, List<Contract.Explorer.RecordDto> records, ref List<Guid> failedIds)
        {
            List<Guid> successIds = new List<Guid>();
            IAveWeb web = null;
            IAveList list = null;
            try
            {
                //TODO  --ywhe order by path could help?
                foreach (var record in records)
                {
                    logger.Info("UnDeclared Records {0}", record.FullPath.LogBase64());
                    IAveListItem item = null;
                    try
                    {
                        if (web == null || (web != null && web.ID != record.WebId))
                        {  //memory leak?
                            web = site.OpenWeb(record.WebId);
                        }
                        if (list == null || (list != null && list.ID != record.ListId))
                        {
                            list = web.GetList(record.ListId);
                        }
                        item = list.GetItemByUniqueId(record.ItemId);
                        if (CheckisRecord(item))
                        {
                            IRecords.UndeclareItemAsRecord(item);
                            successIds.Add(record.Id);
                            if (mNeedSendReport)
                            {
                                AddDeclareDetailForGlobalSearchJob(item, JobDetailsStatus.Successful, string.Empty, false);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn("Declared Records failed {0},{1}", record.FullPath.LogBase64(), e.ToString());
                        failedIds.Add(record.Id);
                        if (mNeedSendReport)
                        {
                            AddDeclareDetailForGlobalSearchJob(item, JobDetailsStatus.Failed, e.Message, false);
                        }
                    }
                }
                //CollectionDataDao.UpdateUnDeclaredRecords(successIds);
            }
            catch (Exception ex)
            {
                logger.Warn("Declared Records failed {0}", ex.ToString());
                if (mNeedSendReport)
                {
                    AddDeclareDetailForGlobalSearchJob(null, JobDetailsStatus.Failed, ex.Message, false);
                }
            }
            finally
            {
                try
                {
                    site.Dispose();
                    web.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn("Dispose sp object failed {0}", e.ToString());
                }
            }
            return successIds;
        }
        public bool CheckisRecord(IAveListItem item)
        {
            bool isRecord = false;
            int result = 0;
            try
            {
                object obj = item[new Guid("3AFCC5C7-C6EF-44f8-9479-3561D72F9E8E")];
                if (obj != null && !int.TryParse(obj.ToString(), out result)) result = 0;
            }
            catch (ArgumentException ex)
            {
                result = 0;
            }
            if ((result & 0x1000) != 0 || (result & 0x10) != 0 || (result & 1) != 0 || (result & 0x100) != 0)
            {
                isRecord = true;
            }
            return isRecord;
        }
        //public IAveListItem GetItem(RMBaseRecord record)//replace to client api
        //{
        //    if (currentAveWeb == null || (currentAveWeb != null && currentAveWeb.ID != record.WebId))
        //    {
        //        currentAveWeb = currentAveSite.OpenWeb(record.WebId);
        //    }
        //    if (currentAveList == null || (currentAveList != null && currentAveList.ID != record.ListId))
        //    {
        //        currentAveList = currentAveWeb.GetList(record.ListId);
        //    }
        //    IAveListItem item = currentAveList.GetItemByUniqueId(record.ItemId);
        //    return item;
        //}

        //public RemoteSiteCollection GetSiteNode(string fullPath)
        //{
        //    //return mDocAveClient.GetSiteNode(fullPath);
        //    return RABrowserClient.GetSiteNode(fullPath);
        //}
        //public RemoteSiteCollection GetSiteNode(Guid aveId)
        //{
        //    //return mDocAveClient.GetSiteNode(aveId);
        //    return RABrowserClient.GetSiteNode(aveId);
        //}
        //private IAveSite InitCurrentSite(RemoteSiteCollection site)
        //{
        //    SharePointSettingUtility SPUtility = new SharePointSettingUtility();
        //    var bposInfo = PoolUserUtil.GetBPOSInfo(site);

        //    aveObjectModelFactory = AveObjectModelFactory.CreateObjectModelFactory(site.url, bposInfo, AveContextKind.ClientObjectModel);
        //    currentAveSite = aveObjectModelFactory.CreateSite();
        //    return currentAveSite;
        //}
        //private IAveORecords Record
        //{
        //    get
        //    {
        //        if (mRecord == null)
        //        {
        //            mRecord = aveObjectModelFactory.CreateRecords();
        //        }
        //        return mRecord;
        //    }
        //}
        //public ClientContext InitContext(string siteUrl)
        //{
        //    siteCollection = GetSiteNode(siteUrl);//from cache
        //    columnName = GetBcsColumnName(siteCollection);
        //    CommonClientContext clientContext = new CommonClientContext();
        //    ClientContext context = clientContext.InitClientContext(siteCollection);
        //    currentSite = context.Site;
        //    return context;
        //}
        //public ClientContext InitContext(AvePoint.RA.Contract.Global.JobMessage.SiteInfo info)
        //{
        //    siteCollection = info;//from cache
        //    columnName = info.BCSColumnName;
        //    CommonClientContext clientContext = new CommonClientContext();
        //    ClientContext context = clientContext.InitClientContext(siteCollection);
        //    currentSite = context.Site;
        //    return context;
        //}
        //private ClientContext InitContext(RemoteSiteCollection site)
        //{
        //    var startTime = DateTime.Now;
        //    siteCollection = site;
        //    columnName = GetBcsColumnName(siteCollection);
        //    logger.Info("column name:{0}, parentId:{1}", columnName, site.parentId);
        //    logger.Warn($"[Change Term] 3.1. time elapsed for initContext(GetBcsColumnName)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
        //    startTime = DateTime.Now;
        //    CommonClientContext clientContext = new CommonClientContext();
        //    currentContext = clientContext.InitClientContext(siteCollection);
        //    logger.Warn($"[Change Term] 3.2. time elapsed for initContext(InitClientContext)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
        //    startTime = DateTime.Now;
        //    currentSite = currentContext.Site;
        //    taxonomySession = TaxonomySession.GetTaxonomySession(currentContext);
        //    logger.Warn($"[Change Term] 3.3. time elapsed for initContext(GetTaxonomySession)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
        //    startTime = DateTime.Now;
        //    currentContext.Load(currentSite);
        //    logger.Warn($"[Change Term] 3.4. time elapsed for initContext(load)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
        //    startTime = DateTime.Now;
        //    currentContext.ExecuteQuery();
        //    logger.Warn($"[Change Term] 3.5. time elapsed for initContext(ExecuteQuery)  {(DateTime.Now - startTime).TotalMilliseconds} ms");
        //    currentWeb = null;//reset current web
        //    return currentContext;
        //}
        #region client context update method
        //public void UpdateTerm(ListItem item, string ternName, Guid termId)
        //{
        //    Field textField = null;
        //    TaxonomyField taxField = GetTaxonomyField(item.ParentList, ref textField);
        //    Guid termScopeId = taxField.AnchorId;
        //    var valueTerm = taxonomySession.GetTerm(termId);
        //    currentContext.Load(valueTerm, t => t.PathOfTerm);
        //    if (termScopeId != Guid.Empty)
        //    {
        //        var scopeTerm = taxonomySession.GetTerm(termScopeId);
        //        currentContext.Load(scopeTerm, t => t.PathOfTerm);
        //        currentContext.ExecuteQuery();
        //        if (!valueTerm.PathOfTerm.StartsWith(scopeTerm.PathOfTerm + ";"))
        //        {
        //            logger.Warn("Scope{0} : TermValue{1}", scopeTerm.PathOfTerm, valueTerm.PathOfTerm);
        //            throw new Exception("Term in invalidate scope");
        //        }
        //    }
        //    else
        //    {
        //        var termSetId = taxField.TermSetId;
        //        currentContext.Load(valueTerm, t => t.TermSet);
        //        currentContext.ExecuteQuery();
        //        if (valueTerm.TermSet.Id != termSetId)
        //        {
        //            logger.Warn("Scope termSet {0} : Term Set Value{1}", termSetId, valueTerm.TermSet.Id);
        //            throw new Exception("Term in invalidate scope");
        //        }
        //    }
        //    taxField.ValidateSetValue(item, ternName + "|" + termId);
        //    //var textFieldName = textField.InternalName;
        //    //item[taxField.InternalName] = ternName + "|" + termId.ToString();
        //    //item[textFieldName] = ternName + "|" + termId.ToString();
        //    ////这个方式是最新版的client dll才包含的，注意测试local站点是否好使
        //    ////item.SystemUpdate();
        //    //item.Update();
        //    item.SystemUpdate();
        //    currentContext.ExecuteQuery();
        //}
        //public TaxonomyField GetTaxonomyField(List list, ref Field textField)
        //{
        //    currentContext.Load(list, l => l.Fields);
        //    currentContext.ExecuteQuery();
        //    var field = list.Fields.GetByTitle(columnName);
        //    currentContext.Load(field);
        //    currentContext.ExecuteQuery();
        //    TaxonomyField taxField = currentContext.CastTo<TaxonomyField>(field);
        //    //TaxonomyField taxField = field as TaxonomyField;
        //    currentContext.Load(taxField);
        //    currentContext.ExecuteQuery();

        //    textField = list.Fields.GetById(taxField.TextField);
        //    currentContext.Load(textField);
        //    currentContext.ExecuteQuery();
        //    return taxField;
        //}

        //public ListItem GetListItem(Contract.Explorer.RecordDto record)//replace to client api
        //{
        //    if (currentWeb == null || currentWeb.Id != record.WebId)
        //    {
        //        currentWeb = currentContext.Site.OpenWebById(record.WebId);
        //        currentContext.Load(currentWeb, w => w.Lists, w => w.Id);
        //        currentContext.ExecuteQuery();
        //    }
        //    if (currentList == null || currentList.Id != record.ListId)
        //    {
        //        currentList = currentWeb.Lists.Where(l => l.Id == record.ListId).FirstOrDefault();
        //    }
        //    currentContext.Load(currentList);
        //    currentContext.ExecuteQuery();
        //    ListItem item = currentList.GetItemById(record.ItemRowId);
        //    currentContext.Load(item);
        //    currentContext.Load(item.File);
        //    currentContext.ExecuteQuery();
        //    return item;
        //}



        #endregion

        #region Check Move And Move Rule Location Path
        //public CheckLocationObject ValidationDestUrlForRA(string url)
        //{
        //    CheckLocationObject checkObject = new CheckLocationObject();
        //    bool isLibraryInRA = true;
        //    url = HttpUtility.UrlDecode(url);
        //    try
        //    {
        //        logger.Info("Start check location url for ra.");
        //        Stopwatch watch = new Stopwatch();
        //        watch.Start();
        //        int listTemplate = 0;
        //        RemoteSiteCollection site = GetRemoteSiteCollectionByListUrl(url);
        //        checkObject.ContainerId = site.parentId;
        //        var account = AccountDao.GetActiveUserByName(TenantLocalValue.LogonUserEmail);
        //        if (!IsSPAdmin(account.UserId))
        //        {
        //            List<string> userAndGroupUserIds = UserService.GetUserAndGroupUserIds(account.UserId);
        //            if (!RMScopeRoleAssignmentDao.HavePermissionOnContainerId(new Guid(site.parentId), userAndGroupUserIds))
        //            {
        //                logger.Info($"Current user doesn't have permission on container. Container Id:{site.parentId}.DesUrl:{url}.");
        //                return null;
        //            }
        //        }
        //        var bposInfo = PoolUserUtil.GetBPOSInfo(site);
        //        var mFactory = AveObjectModelFactory.CreateObjectModelFactory(site.url, bposInfo, AveContextKind.ClientObjectModel);
        //        using (IAveSite mSite = mFactory.CreateSite(site.url))
        //        {
        //            var webUrl = GetWebServerRelativeUrl(mSite, url);
        //            using (IAveWeb web = mSite.OpenWeb(webUrl))
        //            {
        //                IAveList list = null;
        //                if (url.Contains("#/"))
        //                {
        //                    list = web.GetListFromUrl(url.Substring(url.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2));
        //                }
        //                else
        //                {
        //                    list = web.GetList(url);
        //                }
        //                listTemplate = Convert.ToInt32(list.BaseTemplate);
        //                //判断是不是library
        //                if (listTemplate == 101 || listTemplate == 1302)
        //                {
        //                    logger.Info("This is a library, List Template is [{0}], List path is {1}", listTemplate, url);
        //                    checkObject.DestRootPath = url;
        //                    checkObject.AveSiteId = new Guid(site.id);
        //                }
        //                else
        //                {
        //                    logger.Info("This is not a library, List Template is [{0}], List path is {1}", listTemplate, url);
        //                    isLibraryInRA = false;
        //                }
        //                checkObject.UserInfoName = bposInfo.UserName;
        //                if (bposInfo.Password != null && bposInfo.Password != string.Empty)
        //                {
        //                    checkObject.UserInfoKey = Convert.ToBase64String(CspCommunicationWrapper.WrapKey(Encoding.UTF8.GetBytes(bposInfo.Password)));
        //                }
        //            }
        //        }
        //        watch.Stop();
        //        logger.Info("End check location url for ra,Take Milliseconds:{0} ms.", watch.ElapsedMilliseconds);
        //    }
        //    catch (Exception ex)
        //    {
        //        isLibraryInRA = false;
        //        logger.Info("Failed check location url for ra, [{0}],error message:{1}", url, ex.Message);
        //    }
        //    if (!isLibraryInRA)
        //    {
        //        checkObject = null;
        //    }
        //    return checkObject;
        //}
        //private bool IsSPAdmin(string userId)
        //{
        //    return UserService.DoesUserHasThisPermission(TenantLocalValue.LogonGroupId, userId, RMPermissionMasks.SPOAdmin);
        //}
        //public RemoteSiteCollection GetRemoteSiteCollectionByListUrl(string listUrl)
        //{
        //    //var client = new DAOAPIClient();
        //    //return mDocAveClient.GetRemoteSiteCollectionByListUrl(listUrl);
        //    return RABrowserClient.GetRemoteSiteCollectionByListUrl(listUrl);
        //}
        //public string GetWebServerRelativeUrl(IAveSite site, string listUrl)
        //{
        //    return site.GetWebServerRelativeUrl(listUrl);
        //}
        #endregion
    }
}
