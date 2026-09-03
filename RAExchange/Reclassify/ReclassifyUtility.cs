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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.RAExchange.Common;
using ExchangeBackupUtility;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.RMWeb.JobMonitor;

using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Contract.Tenant;

namespace AvePoint.RA.RAExchange.Reclassify
{
    public class ReclassifyUtility : IReclassify
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(ReclassifyUtility));  
        private bool mNeedSendReport = false;
        public int FailedCount = 0;
        private Dictionary<Guid, bool> IsEnableClassificationContainerCache = new();

        private const string SUPPORT_GRAPH_API = "EXOJOB_USING_GRAPH_API";

        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }

        private IRMRecordsUpdateTempDao mRMRecordsUpdateTempDao;
        public IRMRecordsUpdateTempDao RMRecordsUpdateTempDao
        {
            get
            {
                if (mRMRecordsUpdateTempDao == null)
                {
                    mRMRecordsUpdateTempDao = (IRMRecordsUpdateTempDao)PlatformWindsorManager.GetService(typeof(IRMRecordsUpdateTempDao)); ;
                }
                return mRMRecordsUpdateTempDao;
            }
        }

        private IRMClassificationHistoryDao mClassificationHistoryDao;
        protected IRMClassificationHistoryDao ClassificationHistoryDao
        {
            get
            {
                if (mClassificationHistoryDao == null)
                {
                    mClassificationHistoryDao = (IRMClassificationHistoryDao)PlatformWindsorManager.GetService(typeof(IRMClassificationHistoryDao));
                }
                return mClassificationHistoryDao;
            }

        }
        private IRecordsHistoryService mRecordsHistoryService = null;
        public IRecordsHistoryService RecordsHistoryService
        {
            get
            {
                if (mRecordsHistoryService == null)
                {
                    mRecordsHistoryService = (IRecordsHistoryService)PlatformWindsorManager.GetService(typeof(IRecordsHistoryService));
                }
                return mRecordsHistoryService;
            }
        }

        private ITenantService mTenantService;
        public ITenantService TenantService
        {
            get
            {
                if (mTenantService == null)
                {
                    mTenantService = (ITenantService)PlatformWindsorManager.GetService(typeof(ITenantService));
                }
                return mTenantService;
        }
        }

        private IEXOSettingDao mEXOSettingDao;
        public IEXOSettingDao EXOSettingDao
        {
            get
            {
                if (mEXOSettingDao == null)
                {
                    mEXOSettingDao = (IEXOSettingDao)PlatformWindsorManager.GetService(typeof(IEXOSettingDao));
                }
                return mEXOSettingDao;
            }
        }

        private readonly IRMKeyValueDao _rMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private readonly bool _isSupportGraphApi;

        private IReclassify _provider;

        public ReclassifyUtility()
        {
        }

        public ReclassifyUtility(bool needSendReport)
        {
            mNeedSendReport = needSendReport;
        }

        public void ChangeAllTerms(ChangeTermOption changeTermInfo, string tempJobId, bool waiting4OtherSource)
        {
            try
            {
                using (new RA.Common.PerformanceScope("RMExplorerUtility.ChangeTermForExo"))
                {
                    bool isNewLogicAccount = TenantService.IsNewOpusTenant();
                    logger.Info("Change term action start {0}", tempJobId);
                    RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running);
                    mRMRecordsUpdateTempDao.UpdateTempWaiting4OtherSource(tempJobId, waiting4OtherSource);
                    List<Record> records = new List<Record>();
                    //RecordHistoryXml xml = new RecordHistoryXml()
                    //{
                    //    HistoryList = new List<RecordHistory>()
                    //};
                    //var userName = WebUtil.LogOnUserName;
                    //xml.HistoryList.Add(new RecordHistory()
                    //{
                    //    Action = "RM_BCM_Audit_Action_ChangeTerm",
                    //    TimeUTC = DateTime.UtcNow.Ticks,
                    //    User = userName
                    //});
                    if (changeTermInfo.SourceEXORecordIds != null && changeTermInfo.SourceEXORecordIds.Count > 0)
                    {
                        using (new RA.Common.PerformanceScope(string.Format("change.Term.GetRecords")))
                        {
                            records = ExplorerDao.QueryAll(r => changeTermInfo.SourceEXORecordIds.Contains(r.Id)).ToList();
                            //records = CollectionDataDao.GetRecordByIds(changeTermInfo.RecordIds);//to do

                            List<Guid> allGuids = new List<Guid>();
                            allGuids.AddRange(changeTermInfo.SourceRecordIds);
                            allGuids.AddRange(changeTermInfo.SourceEXORecordIds);
                            allGuids.AddRange(changeTermInfo.SourceFSRecordIds);
                            var recordsNoti = ExplorerDao.QueryAll(r => allGuids.Contains(r.Id)).ToList();
                            RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running, JsonConvert.SerializeObject(recordsNoti.Select(r => r.LeafName).ToList()));
                        }

                        var recDics = records.Where(a => a.SourceFlag == 3).GroupBy(r => r.AveSiteId).ToDictionary(z => z.Key, p => p.ToList());
                        var aveEXOIds = recDics.Keys.ToList();
                        Dictionary<string, ExchangeOnlineTreeNodeDto> mailBoxDic = new Dictionary<string, ExchangeOnlineTreeNodeDto>();
                        List<Guid> failedIds = new List<Guid>();
                        List<Guid> successIds = new List<Guid>();
                        List<Record> successRecords = new List<Record>();
                        if (aveEXOIds.Count > 0)
                        {
                            string termName = changeTermInfo.TargetTermName;
                            Guid termId = changeTermInfo.TargetTermUniqueId;
                            foreach (var recDic in recDics)
                            {
                                var firstRec = recDic.Value.FirstOrDefault();
                                //ExchangeOnlineTreeNodeDto mailBox = mDocAveClient.GetExchangeNodeByIdAndAddress(id, recDic[id].FirstOrDefault().EmailAddress);
                                ExchangeOnlineTreeNodeDto mailBox = RABrowserClient.GetExchangeNodeByIdAndAddress(firstRec.AveSiteId, firstRec.EmailAddress);                               
                                if (mailBox != null && !mailBoxDic.ContainsKey(mailBox.ID))
                                {
                                    mailBoxDic.Add(mailBox.ID, mailBox);
                                }
                            }
                            foreach (var recList in recDics.Values)
                            {
                                if (recList.Count > 0)
                                {
                                    try
                                    {
                                        //recList[0].AveSiteId存储的可能是AOS MailboxID(老数据)，也可能是AOS MailboxID(新数据)
                                        //mailBoxDic里面存储的MailboxID一定是AOS MailboxID
                                        //针对新数据，并change Email Address的暂不支持，后续版本继续处理
                                        if (mailBoxDic.ContainsKey(recList[0].AveSiteId) || mailBoxDic.Values.Where(x => x.EmailAddress == recList[0].EmailAddress).Count() > 0)
                                        {
                                            ExchangeOnlineTreeNodeDto mailBox = mailBoxDic.ContainsKey(recList[0].AveSiteId) ? mailBoxDic[recList[0].AveSiteId] : mailBoxDic.Values.Where(x => x.EmailAddress == recList[0].EmailAddress).FirstOrDefault();

                                            TreeManagement tm = new TreeManagement();
                                            var mailboxEmail = mailBox.EmailAddress ?? TreeManagement.GetMailboxNode(mailBox)?.Name;
                                            var useGraph = EXOGraphApiResolver.ShouldUseGraph(_rMKeyValueDao, mailboxEmail, tm.GetRealMailboxStringId(mailBox), mailBox);
                                            _provider = useGraph ? new GraphReclassify() : this;
                                            
                                            successRecords = _provider.ChangeRecordTermAction(mailBox, recList, termName, termId, ref failedIds,ref FailedCount);
                                            successIds.AddRange(successRecords.Select(a => a.Id).ToList());
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
                                                if(isNewLogicAccount && perviousTermId != termId && IsContainerEnableClassification(rec.ContainerId)) rec.RemoveManualFields();
                                            });

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
                                            if (mNeedSendReport)
                                            {
                                                foreach (var record in successRecords)
                                                {
                                                    AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Successful, "");
                                                }

                                                var failedRecords = recList.Where(r => !successIds.Contains(r.Id)).ToList();
                                                foreach (var record in failedRecords)
                                                {
                                                    AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Failed, "RM_JS_Audit_ChangeTermErrorMessage");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            List<Guid> recIds = new List<Guid>();
                                            if (recList[0].SourceFlag == 3)
                                            {
                                                throw new Exception("RM_RDM_MailboxNotFound");
                                            }
                                        }
                                    }
                                    catch (Exception ee)
                                    {
                                        failedIds.AddRange(recList.Select(t => t.Id));
                                        logger.Warn("change term action failed {0}", ee.ToString());
                                        if (mNeedSendReport)
                                        {
                                            string message = !string.IsNullOrWhiteSpace(ee.Message) && ee.Message.Equals("RM_RDM_MailboxNotFound") ? "RM_RDM_MailboxNotFound" : "RM_JS_Audit_ChangeTermErrorMessage";
                                            foreach (var record in recList)
                                            {
                                                AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Failed, message);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (successIds.Count > 0)
                        {
                            //xml.HistoryList[0].Action = "RM_BCM_Audit_Action_ChangeTerm";
                            //ExplorerDao.AddReocrdHistory(successIds, xml);
                            RecordsHistoryService.AddRecordsHistory(successIds, "RM_BCM_Audit_Action_ChangeTerm", changeTermInfo.Comment);
                        }
                        if (failedIds.Count > 0)
                        {
                            FailedCount += failedIds.Count;
                            string failedNames = string.Empty;
                            foreach (var fid in failedIds)
                            {
                                failedNames += records.Where(t => t.Id == fid).FirstOrDefault()?.LeafName + ";";
                            }
                            failedNames = failedNames.TrimEnd(';');
                            RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, failedNames, RecordsConstants.Explorer_RealTime_Failed_Partial);
                            mRMRecordsUpdateTempDao.UpdateTempWaiting4OtherSource(tempJobId, waiting4OtherSource);
                            //xml.HistoryList[0].Action = "RM_JS_Audit_ChangeTermErrorMessage";
                            //ExplorerDao.AddReocrdHistory(failedIds, xml);
                            RecordsHistoryService.AddRecordsHistory(failedIds, "RM_JS_Audit_ChangeTermErrorMessage");
                            if (!mNeedSendReport)
                            {
                                throw new Exception(string.Format(I18NEntity.GetString("RM_RDM_Explorer_ChangeTermError"), failedIds));
                            }
                        }
                        else
                        {
                            RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Finished);
                            mRMRecordsUpdateTempDao.UpdateTempWaiting4OtherSource(tempJobId, waiting4OtherSource);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Failed_Partial);
                mRMRecordsUpdateTempDao.UpdateTempWaiting4OtherSource(tempJobId, waiting4OtherSource);
                logger.Error("change term error:{0}", ex.ToString());
                throw ex;
            }
            finally
            {
                logger.Info("Change term action finish {0}", tempJobId);
            }
        }

        private bool IsContainerEnableClassification(string containerId)
        {
            if(Guid.TryParse(containerId, out Guid containerGuid))
            {
                if (IsEnableClassificationContainerCache.ContainsKey(containerGuid))
                {
                    return IsEnableClassificationContainerCache[containerGuid];
                }
                else
                {
                    var exoSetting = EXOSettingDao.GetSettingInfoByScope(containerGuid, Guid.Empty, containerGuid);
                    var isEnable = exoSetting == null || !exoSetting.IsNullClassificationSetting;
                    IsEnableClassificationContainerCache[containerGuid] = isEnable;
                    return isEnable;
                }
            }
            return false;
        }
        public List<Record> ChangeRecordTermAction(ExchangeOnlineTreeNodeDto mailBox, List<Record> records, string termName, Guid termId, ref List<Guid> failedIds, ref int FailedCount)
        {
            List<Record> successRecords = new List<Record>();
            try
            {
                TreeManagement tm = new TreeManagement();
                ExchangeFolder folder = tm.GetExchangeFolderFromTreeNode(mailBox);
                foreach (var record in records)
                {
                    logger.Info("change term action {0}:{1}", record.Id, termName);
                    try
                    {
                        ExchangeItem item = folder.GetItemById(record.ExternalId);
                        if (item != null)
                        {
                            item.UpdateItemIdField(TermColumnInfo.WellKnowTermColumnGuid, TermColumnInfo.WellKnowTermColumnId, termId.ToString());
                            successRecords.Add(record);                           
                        }
                        else
                        {
                            failedIds.Add(record.Id);
                            FailedCount++;                           
                        }
                    }
                    catch (Exception e)
                    {
                        failedIds.Add(record.Id);
                        FailedCount++;
                        logger.Warn("update item term failed {0}:{1} error {2}", record.FullPath, record.TermName, e.ToString());
                    }
                }
            }
            //catch (Exception e)
            //{
            //    logger.Error("update item term failed,mailbox is {0}", mailBox.Name);
            //    logger.Error(e.Message, e);
            //    //logger.Info(GCommon.Utility.Cryptography.CspCommunicationWrapper.CommunicationEncryptionKey.ToString());
            //    //logger.Info(GCommon.Utility.Cryptography.CspCommunicationWrapper.AuthToken);
            //}
            finally
            {
                logger.Info("update item term finish,mailbox is {0}", mailBox.ID);
            }
            return successRecords;
        }

        private void AddReclassifyDetailForGlobalSearch(Record record, JobDetailsStatus status, string comment)
        {
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = record?.LeafName,
                FullPath = record == null ? "" : record.EmailAddress + record.DirPath + "_" + new DateTime(record.TimeCreated).ToString("R"),
                Action = "RM_JS_BCM_Explorer_ChangeTerm",
                Status = status,
                Comment = comment,
                Type = "RM_JS_Rule_ObjectLevel_ExchangeOnlineItem"
            });
        }

        public object GetGroupKey(Record record)
        {
            return record.AveSiteId;
        }
    }
}
