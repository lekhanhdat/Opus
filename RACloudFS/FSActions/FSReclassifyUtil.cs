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
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.FileSystem;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACloudFS.FSActions
{
    public class FSReclassifyUtil
    {
        
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region InterFace
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
        #endregion

        private bool mNeedSendReport = false;
        public int FailedCount = 0;
        public FSReclassifyUtil()
        {          
        }
        public FSReclassifyUtil(bool needSendReport)
        {
            mNeedSendReport = needSendReport;
        }

        public void ChangeAllTerms(ChangeTermOption changeTermInfo, string tempJobId, bool waiting4OtherSource)
        {
            try
            {
                using (new RA.Common.PerformanceScope("RMExplorerUtility.ChangeTermForFS"))
                {
                    var isNewLogicAccount = TenantService.IsNewOpusTenant();
                    logger.Info("Change term action start {0}", tempJobId);
                    RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running);
                    //RMRecordsUpdateTempDao.UpdateTempWaiting4OtherSource(tempJobId, waiting4OtherSource);
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
                    var sw = Stopwatch.StartNew();
                    if (changeTermInfo.SourceFSRecordIds != null && changeTermInfo.SourceFSRecordIds.Count > 0)
                    {
                        using (new RA.Common.PerformanceScope(string.Format("change.Term.GetRecords")))
                        {
                            records = ExplorerDao.QueryAll(r => changeTermInfo.SourceFSRecordIds.Contains(r.Id)).ToList();
                            logger.Warn($"[Change Term] 1. time elapsed for query {records.Count} records from cosmos: {sw.Elapsed}");

                            List<Guid> allGuids = new List<Guid>();
                            allGuids.AddRange(changeTermInfo.SourceRecordIds);
                            allGuids.AddRange(changeTermInfo.SourceEXORecordIds);
                            allGuids.AddRange(changeTermInfo.SourceFSRecordIds);
                            sw.Restart();
                            var recordsNoti = ExplorerDao.GetFilterList(a => a.LeafName, r => allGuids.Contains(r.Id)).ToList();
                            //var recordsNoti = ExplorerDao.QueryAll(r => allGuids.Contains(r.Id)).ToList();
                            var jsonRecord = JsonConvert.SerializeObject(recordsNoti);
                            logger.Warn($"[Change Term] 2. time elapsed for query all source data name diaplay notification: {sw.Elapsed}");
                            RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running, jsonRecord);
                        }
                        var fsRecords = records.Where(a => a.SourceFlag == (int)SourceFlag.FileSystem).ToList();
                        List<Guid> failedIds = new List<Guid>();
                        List<Guid> successIds = new List<Guid>();
                        List<Record> successRecords = new List<Record>();

                        string termName = changeTermInfo.TargetTermName;
                        Guid termId = changeTermInfo.TargetTermUniqueId;
                        int termIntId = changeTermInfo.TargetTermId;

                        logger.Info($"[Change Term] 3. start fs classify processor");
                        sw.Restart();
                        FSReclassifyProcessor fsReclassify = new FSReclassifyProcessor();
                        successRecords = fsReclassify.ChangeFSRecordTermAction(fsRecords, termIntId, termName, termId, isNewLogicAccount, ref failedIds);
                        logger.Warn($"[Change Term] 4. time elapsed for ChangeFSRecordTermAction {sw.Elapsed}");
                        if (mNeedSendReport)
                        {
                            foreach (var record in successRecords)
                            {
                                AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Successful, "");
                            }
                        }
                        successIds = successRecords.Select(a => a.Id).ToList();

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

                        if (successIds.Count > 0)
                        {
                            //xml.HistoryList[0].Action = "RM_BCM_Audit_Action_ChangeTerm";
                            //ExplorerDao.AddReocrdHistory(successIds, xml);
                            RecordsHistoryService.AddRecordsHistory(successIds, "RM_BCM_Audit_Action_ChangeTerm", changeTermInfo.Comment);
                        }
                        if (failedIds.Count > 0)
                        {
                            FailedCount += failedIds.Count;
                            if (mNeedSendReport)
                            {
                                var failedRecords = fsRecords.Where(r => failedIds.Contains(r.Id)).ToList();
                                foreach (var record in failedRecords)
                                {
                                    AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Failed, "RM_JM_GlobalSearch_ChangeTermFailed");
                                }
                            }
                            string failedNames = string.Empty;
                            foreach (var fid in failedIds)
                            {
                                failedNames += records.Where(t => t.Id == fid).FirstOrDefault()?.LeafName + ";";
                            }
                            failedNames = failedNames.TrimEnd(';');
                            RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, failedNames, RecordsConstants.Explorer_RealTime_Failed_Partial);
                            RMRecordsUpdateTempDao.UpdateTempWaiting4OtherSource(tempJobId, waiting4OtherSource);
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
                            RMRecordsUpdateTempDao.UpdateTempWaiting4OtherSource(tempJobId, waiting4OtherSource);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Failed_Partial);
                RMRecordsUpdateTempDao.UpdateTempWaiting4OtherSource(tempJobId, waiting4OtherSource);
                logger.Error("change term error:{0}", ex.ToString());
                if (mNeedSendReport)
                {
                    AddReclassifyDetailForGlobalSearch(null, JobDetailsStatus.Failed, ex.Message);
                }
                throw ex;
            }
            finally
            {
                logger.Info("Change term action finish {0}", tempJobId);
            }
        }

        private void AddReclassifyDetailForGlobalSearch(Record record, JobDetailsStatus status, string comment)
        {
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = record?.LeafName,
                FullPath = record == null ? "" : record.DirPath + "\\" + record.LeafName,
                Action = "RM_JS_BCM_Explorer_ChangeTerm",
                Status = status,
                Comment = comment,
                Type = record == null ? "" : ConvertNodeTypeToReportType(record.NodeType)
            });
        }

        public string ConvertNodeTypeToReportType(int nodeType)
        {
            string reportType = string.Empty;
            switch (nodeType)
            {
                case (int)GCommon.Contract.Tree.Object.NodeLevel.FSFile:
                    reportType = "RM_JS_Rule_ObjectLevel_FSFile";
                    break;
                case (int)GCommon.Contract.Tree.Object.NodeLevel.FSFolder:
                    reportType = "RM_JS_Rule_ObjectLevel_FSFolder";
                    break;

                default:
                    break;
            }
            return reportType;
        }
    }
}
