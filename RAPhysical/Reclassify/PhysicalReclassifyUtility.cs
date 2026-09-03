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
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.RAPhysical.Disposal.PhysicalDisposalActionImps;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Tenant;

namespace AvePoint.RA.RAPhysical.Reclassify
{
    public class PhysicalReclassifyUtility
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(PhysicalReclassifyUtility));
        #region Interface
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

        private IExplorerService mExplorerService;
        public IExplorerService ExplorerService
        {
            get
            {
                if (mExplorerService == null)
                {
                    mExplorerService = (IExplorerService)PlatformWindsorManager.GetService(typeof(IExplorerService)); ;
                }
                return mExplorerService;
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

        private ITermDao mTermDao { get; set; }
        public ITermDao TermDao
        {
            get
            {
                if (mTermDao == null)
                {
                    mTermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
                }
                return mTermDao;
            }
        }

        private ITermSetDao mTermSetDao { get; set; }
        public ITermSetDao TermSetDao
        {
            get
            {
                if (mTermSetDao == null)
                {
                    mTermSetDao = (ITermSetDao)PlatformWindsorManager.GetService(typeof(ITermSetDao));
                }
                return mTermSetDao;
            }
        }

        private IPhysicalRecordSettingDao mPhysicalRecordSettingDao { get; set; }
        public IPhysicalRecordSettingDao PhysicalRecordSettingDao
        {
            get
            {
                if (mPhysicalRecordSettingDao == null)
                {
                    mPhysicalRecordSettingDao = (IPhysicalRecordSettingDao)PlatformWindsorManager.GetService(typeof(IPhysicalRecordSettingDao));
                }
                return mPhysicalRecordSettingDao;
            }
        }
        private IRMPhysicalRecordSettingsService mRMPhysicalRecordSettingsService { get; set; }
        public IRMPhysicalRecordSettingsService RMPhysicalRecordSettingsService
        {
            get
            {
                if (mRMPhysicalRecordSettingsService == null)
                {
                    mRMPhysicalRecordSettingsService = (IRMPhysicalRecordSettingsService)PlatformWindsorManager.GetService(typeof(IRMPhysicalRecordSettingsService));
                }
                return mRMPhysicalRecordSettingsService;
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

        private Dictionary<Guid, string> mTermPaths = new Dictionary<Guid, string>();
        private Dictionary<Guid, Dictionary<Guid, bool>> mTermAllowToParent = new Dictionary<Guid, Dictionary<Guid, bool>>();
        private List<RMPhysicalRecordSetting> mAllsettings = new List<RMPhysicalRecordSetting>();


        public PhysicalReclassifyUtility()
        {
            mAllsettings = PhysicalRecordSettingDao.GetAllPhysicalRecordSettings();
        }

        public PhysicalReclassifyUtility(bool needSendReport)
        {
            mAllsettings = PhysicalRecordSettingDao.GetAllPhysicalRecordSettings();
            mNeedSendReport = needSendReport;
        }

        public void ChangeAllTermsForPhy(ChangeTermOption changeTermInfo, string tempJobId)
        {
            try
            {
                using (new RA.Common.PerformanceScope("RMExplorerUtility.ChangeTermForPhysical"))
                {
                    var isNewLogicAccount = TenantService.IsNewOpusTenant();
                    logger.Info("Is new logic account is {0}", isNewLogicAccount);
                    logger.Info("Change term action start {0}", tempJobId);
                    RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running);
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
                    if (changeTermInfo.SourcePhyRecordIds != null && changeTermInfo.SourcePhyRecordIds.Count > 0)
                    {
                        using (new PerformanceScope(string.Format("change.Term.GetRecords")))
                        {
                            records = ExplorerDao.QueryAll(r => changeTermInfo.SourcePhyRecordIds.Contains(r.Id)).ToList();
                            List<Guid> allGuids = new List<Guid>();
                            //allGuids.AddRange(changeTermInfo.SourceRecordIds);
                            //allGuids.AddRange(changeTermInfo.SourceEXORecordIds);
                            allGuids.AddRange(changeTermInfo.SourcePhyRecordIds);
                            var recordsNoti = ExplorerDao.QueryAll(r => allGuids.Contains(r.Id)).ToList();
                            RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Running, JsonConvert.SerializeObject(recordsNoti.Select(r => r.LeafName).ToList()));
                        }
                        //locationId
                        var recDic = records.Where(a => a.SourceFlag == 4 && (a.NodeType == (int)RMNodeType.PhyBox || a.NodeType == (int)RMNodeType.PhyFile)).GroupBy(r => r.LocationId).ToDictionary(z => z.Key, p => p.ToList());
                        var locationIds = recDic.Keys.ToList();

                        Dictionary<Guid, PhysicalLocation> locationDic = new Dictionary<Guid, PhysicalLocation>();
                        List<Guid> failedIds = new List<Guid>();
                        List<Guid> successIds = new List<Guid>();
                        List<Record> successRecords = new List<Record>();
                        if (locationIds.Count > 0)
                        {
                            string termName = changeTermInfo.TargetTermName;
                            Guid termId = changeTermInfo.TargetTermUniqueId;
                            foreach (var locationId in locationIds)
                            {
                                var currentLocation = new PhysicalLocation(locationId);

                                if (locationDic.ContainsKey(currentLocation.UniqueId) && currentLocation != null)
                                {
                                    locationDic[currentLocation.UniqueId] = currentLocation;
                                }
                                else if (currentLocation != null)
                                {
                                    locationDic.Add(currentLocation.UniqueId, currentLocation);
                                }
                            }
                            foreach (var recList in recDic.Values)
                            {
                                if (recList.Count > 0)
                                {
                                    try
                                    {
                                        if (locationDic.ContainsKey(recList[0].LocationId))
                                        {
                                            PhysicalLocation location = locationDic[recList[0].LocationId];

                                            if (!IsSameTermScope(location, termId))
                                            {
                                                logger.Debug("This location :{0} is not in the same term scope.", location.Name);
                                                throw new Exception("RM_FS_FolderReclassify_FileNotInSameTermScope");
                                            }
                                            successRecords = ChangeRecordTermAction(location, recList, termName, termId, ref failedIds);
                                            if (mNeedSendReport)
                                            {
                                                foreach (var record in successRecords)
                                                {
                                                    AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Successful, "");
                                                }
                                            }
                                            successIds = successRecords.Select(a => a.Id).ToList();
                                            var previousTermId = Guid.Empty;
                                            ExplorerDao.UpdateAll(r => successIds.Contains(r.Id), rec =>
                                            {
                                                previousTermId = rec.TermId;
                                                rec.TermId = termId;
                                                rec.TermName = termName;
                                                rec.RuleId = Guid.Empty;
                                                rec.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                rec.PreviosDisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                                                rec.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                                                if(isNewLogicAccount && previousTermId != termId) rec.RemoveManualFields();
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
                                                var failedRecords = recList.Where(r => !successIds.Contains(r.Id)).ToList();
                                                foreach (var record in failedRecords)
                                                {
                                                    AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Failed, ".Changed term failed");
                                                }
                                            }
                                        }
                                        else
                                        {
                                            List<Guid> recIds = new List<Guid>();
                                            if (recList[0].SourceFlag == (int)Contract.Explorer.SourceFlag.Physical)
                                            {
                                                throw new Exception("can't get location obj");
                                            }
                                        }
                                    }
                                    catch (Exception ee)
                                    {
                                        failedIds.AddRange(recList.Select(t => t.Id));
                                        if (mNeedSendReport)
                                        {
                                            foreach(var record in recList)
                                            {
                                                AddReclassifyDetailForGlobalSearch(record, JobDetailsStatus.Failed, ee.Message);
                                            }
                                        }
                                        logger.Warn("change term action failed {0}", ee.ToString());
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
                            //xml.HistoryList[0].Action = "RM_JS_Audit_ChangeTermErrorMessage";
                            //ExplorerDao.AddReocrdHistory(failedIds, xml);
                            RecordsHistoryService.AddRecordsHistory(failedIds, "RM_JS_Audit_ChangeTermErrorMessage");
                            if (!mNeedSendReport)
                            {
                                throw new Exception(string.Format(I18NEntity.GetString("RM_RDM_Explorer_ChangeTermError"), failedNames));
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
                //may be update duplicate
                RMRecordsUpdateTempDao.InsertUpdateTemp(tempJobId, "", RecordsConstants.Explorer_RealTime_Failed_Partial);
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

        private bool IsSameTermScope(PhysicalLocation location, Guid termId)
        {
            Guid locationId = location.UniqueId;
            RMPhysicalRecordSettingsService.CheckIsTopLevelSetting(location.DirPathIds, out bool isTopLevelLocation, out Guid topLevelLocationUniqueId, out List<string> locationDirPathIds);
            RMPhysicalRecordSetting bindSetting = PhysicalRecordSettingDao.GetPhysicalRecordSetting(locationId);
            if (bindSetting == null)
            {
                if (!isTopLevelLocation && locationDirPathIds != null)
                {
                    bindSetting = PhysicalRecordSettingDao.GetAncestryPhysicalRecordSetting(locationDirPathIds);
                }
            }
            // bindSetting = mAllsettings.FirstOrDefault(s => s.LocationUniqueId == locationId);
            if (bindSetting == null)
            {
                return false;
            }
            if (CheckTermValue(bindSetting, termId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckTermValue(RMPhysicalRecordSetting setting, Guid termId)
        {
            bool bindTermSet = setting.TermId == Guid.Empty;
            var parentId = bindTermSet ? setting.TermSetId : setting.TermId;
            return CheckTermValue(bindTermSet, parentId, termId);
        }


        private bool CheckTermValue(bool bindTermSet, Guid parentId, Guid termId)
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

        private void AddReclassifyDetailForGlobalSearch(Record record, JobDetailsStatus status, string comment)
        {
            ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = record?.LeafName,
                FullPath = record == null ? "" : GetFullPath(record.NodeId) + "/" + record?.LeafName,
                Action = "RM_JS_BCM_Explorer_ChangeTerm",
                Status = status,
                Comment = comment,
                Type = record == null ? "" : ConvertNodeTypeToReportType(record.NodeType)
            });
        }

        private string GetFullPath(Guid nodeId)
        {
           return ExplorerService.GetPhysicalObjectFullPath(nodeId);
        }

        public string ConvertNodeTypeToReportType(int nodeType)
        {
            string reportType = string.Empty;
            switch (nodeType)
            {
                case (int)RMNodeLevel.PhysicalFile:
                    reportType = "RM_PRM_PRE_Filter_PhysicalFile";
                    break;
                case (int)RMNodeLevel.PhysicalBox:
                    reportType = "RM_Common_ObjectLevel_PhysicalBox";
                    break;
                case (int)RMNodeLevel.PhysicalRecord:
                    reportType = "RM_JS_Rule_ObjectLevel_PhysicalFile";
                    break;
                default:
                    break;
            }
            return reportType;
        }

        public List<Record> ChangeRecordTermAction(PhysicalLocation location, List<Record> records, string termName, Guid termId, ref List<Guid> failedIds)
        {
            List<Record> successRecords = new List<Record>();
            Dictionary<Guid ,string> termIdPathMapping = new Dictionary<Guid ,string>();
            var currentTermPath = TermDao.GetTermNamesPathByTermId(termId);
            try
            {
                //TreeManagement tm = new TreeManagement();
                //ExchangeFolder folder = tm.GetExchangeFolderFromTreeNode(location);
                List<Guid> recordInLocationIds = new List<Guid>();
                List<IPhysicalBox> boxes = new List<IPhysicalBox>();
                List<IPhysicalFile> files = new List<IPhysicalFile>();

                foreach (var record in records)
                {
                    if (record.BoxId == Guid.Empty)
                    {
                        recordInLocationIds.Add(record.Id);
                    }
                    else
                    {
                        var box = location.GetBoxes(b => b.Id == record.BoxId).FirstOrDefault();
                        if (box != null)
                        {
                            var file = box.GetFiles(f => f.Id == record.Id).FirstOrDefault();
                            if (file != null)
                            {
                                files.Add(file);
                            }
                        }
                    }
                }

                boxes.AddRange(location.GetBoxes(b => recordInLocationIds.Contains(b.Id)));
                files.AddRange(location.GetFiles(b => recordInLocationIds.Contains(b.Id)));
                var actionAudits = new List<PhysicalRecordActionAudit>();
    
                foreach (var box in boxes)
                {
                    logger.Info("change term action {0}:{1}", box.Id, termName);
                    try
                    {
                        if(!termIdPathMapping.TryGetValue(box.TermId, out var orignalTermPath))
                        {
                            orignalTermPath = TermDao.GetTermNamesPathByTermId(box.TermId);
                            termIdPathMapping[box.TermId] = orignalTermPath;
                        }
                        var actionAudit = RecordsHistoryService.BuildPhysicalReclassifyAudit(box.Id, orignalTermPath, currentTermPath);
                        actionAudits.Add(actionAudit);
                        var classifyField = new TaxonomyColumnValue() { Id = termId.ToString(), Name = termName };
                        box[MetaInfo.Classification] = JsonConvert.SerializeObject(classifyField);
                        box.Update(true);
                        successRecords.Add((box as PhysicalBox).Record);
                    }
                    catch (Exception e)
                    {
                        failedIds.Add(box.Id);
                        logger.Warn("update item term failed {0}:{1} error {2}", box?.Id, termName, e.ToString());
                    }
                }
                foreach (var file in files)
                {
                    logger.Info("change term action {0}:{1}", file.Id, termName);
                    try
                    {
                        if (!termIdPathMapping.TryGetValue(file.TermId, out var orignalTermPath))
                        {
                            orignalTermPath = TermDao.GetTermNamesPathByTermId(file.TermId);
                            termIdPathMapping[file.TermId] = orignalTermPath;
                        }
                        var actionAudit = RecordsHistoryService.BuildPhysicalReclassifyAudit(file.Id, orignalTermPath, currentTermPath);
                        actionAudits.Add(actionAudit);
                        var classifyField = new TaxonomyColumnValue() { Id = termId.ToString(), Name = termName };
                        file[MetaInfo.Classification] = JsonConvert.SerializeObject(classifyField);
                        file.Update(true);
                        successRecords.Add((file as PhysicalFile).Record);
                    }
                    catch (Exception e)
                    {
                        failedIds.Add(file.Id);
                        logger.Warn("update item term failed {0}:{1} error {2}", file?.Id, termName, e.ToString());
                    }
                }

                RecordsHistoryService.AddPhysicalAudit(actionAudits);
            }
            //catch (Exception e)
            //{
            //    logger.Error("update item term failed,mailbox is {0}", mailBox.Name);
            //    logger.Error(e.Message, e);
            //}
            finally
            {
                logger.Info("update item term finish, location is {0}", location.UniqueId);
            }
            return successRecords;
        }
    }
}
