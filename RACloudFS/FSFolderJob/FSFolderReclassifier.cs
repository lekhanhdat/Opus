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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.FileSystem;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using RACloudFS.Util;
using Records.FS.Reclassify;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RACloudFS.FSFolderJob
{
    public class FSFolderReclassifier : IDisposable
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(FSFolderReclassifier));

        private string _jobId;
        public int mFailedCount = 0;
        private bool _jobHasStopped = false;
        public int mSucceedCount = 0;
        private ChangeTermDto _jobContextDto;
        private List<Rule> Rules;
        private ExplorerDao _explorerDao = new ExplorerDao();
        private bool mChangeAllFile = false;
        private int ClassificationLevel;
        private Dictionary<Guid, Dictionary<Guid, bool>> mTermAllowToParent = new Dictionary<Guid, Dictionary<Guid, bool>>();
        private List<RMFileSystemSetting> mAllsettings = new List<RMFileSystemSetting>();
        private Dictionary<Guid, FSConnection> mFSConnectionDic = new Dictionary<Guid, FSConnection>();
        private Dictionary<Guid, string> mTermPaths = new Dictionary<Guid, string>();
        private List<Guid> rootFolders = new List<Guid>();
        private bool IsNewLogicAccount = true;


        #region Interface
        private IRMFunctionSettingDao FunctionSettingDao = PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        private IRMSubJobDao mSubJobDao;
        public IRMSubJobDao SubJobDao
        {
            get
            {
                if (mSubJobDao == null)
                {
                    mSubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
                }
                return mSubJobDao;
            }
        }

        private IRuleManagerService mRuleManagerService;
        public IRuleManagerService RuleManagerService
        {
            get
            {
                if (mRuleManagerService == null)
                {
                    mRuleManagerService = (IRuleManagerService)PlatformWindsorManager.GetService(typeof(IRuleManagerService));
                }
                return mRuleManagerService;
            }
        }

        private IJobInfoUpdater _jobInfoUpdater;
        protected IJobInfoUpdater JobInfoUpdater
        {
            get
            {
                if (_jobInfoUpdater == null)
                {
                    _jobInfoUpdater = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));
                }
                return _jobInfoUpdater;
            }
        }

        private IRMReportManager mReportManger;
        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
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
        private IFileSystemSettingDao mFileSystemSettingDao { get; set; }
        public IFileSystemSettingDao FileSystemSettingDao
        {
            get
            {
                if (mFileSystemSettingDao == null)
                {
                    mFileSystemSettingDao = (IFileSystemSettingDao)PlatformWindsorManager.GetService(typeof(IFileSystemSettingDao));
                }
                return mFileSystemSettingDao;
            }
        }
        public IFSConnectionDao mFSConnectionDao { get; set; }
        public IFSConnectionDao FSConnectionDao
        {
            get
            {
                if (mFSConnectionDao == null)
                {
                    mFSConnectionDao = (IFSConnectionDao)PlatformWindsorManager.GetService(typeof(IFSConnectionDao));
                }
                return mFSConnectionDao;
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

        private ITenantService mTenantService = null;
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
        // use for global search action job
        public FSFolderReclassifier(ChangeTermDto dto)
        {
            _jobContextDto = dto;
            ClassificationLevel = this.GetClassificationLevel();
            LoadRules();
            mChangeAllFile = dto.OverWriteSubFiles;
            FSReclassifierCache.Instance.FolderIds = new List<Guid>();
            FSReclassifierCache.Instance.Init(dto, Rules);           
            mAllsettings = FileSystemSettingDao.LoadAllSetting();
            mAllsettings.ForEach(o => { o.FullPath = EncodeUtil.DecryptByCommunicationKey(o.FullPath); });
            IsNewLogicAccount = TenantService.IsNewOpusTenant();
        }
        public FSFolderReclassifier(string jobId)
        {
            _jobId = jobId;
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(jobId, true);
            var jobContext = SerializerHelper.DeserializeByDataContractSerializer<ChangeTermDto>(subJobWithContext.JobContext.Content);
            _jobContextDto = jobContext;
            TenantLocalValue.LogonUserId = _jobContextDto.UserId;
            JobInfoUpdater.UpdateJobState(_jobId, (int)JobStatus.InProgress);
            JobInfoUpdater.UpdateJobProgress(_jobId, 1);
            ReportManager.StartUpdateJobProgress();

            List<Record> records = new List<Record>();
            logger.Info("Object count from the message is :{0}", _jobContextDto.FSRecordIds.Count);
            records = _explorerDao.GetRecordByIds(_jobContextDto.FSRecordIds);
            ReportManager.IncreaseBase(records.Count);


            ClassificationLevel = this.GetClassificationLevel();
            LoadRules();
            ReportManager.Increase();
            mChangeAllFile = _jobContextDto.OverWriteSubFiles;
            FSReclassifierCache.Instance.Init(_jobContextDto, Rules);
            FSReclassifierCache.Instance.RecordsCache.AddBatch(records);
            FSReclassifierCache.Instance.FolderIds = _jobContextDto.FSRecordIds;
            mAllsettings = FileSystemSettingDao.LoadAllSetting();
            mAllsettings.ForEach(o => { o.FullPath = EncodeUtil.DecryptByCommunicationKey(o.FullPath); });
            IsNewLogicAccount = TenantService.IsNewOpusTenant();
        }
        public async Task RunAsync()
        {
            try
            {
                logger.Info("Start to run the reclassification job.");
                var ruleUtil = new FSRuleUtil(FSReclassifierCache.Instance.Rules);
                FSFolderJobDiscover discover = new FSFolderJobDiscover();
                while (FSReclassifierCache.Instance.RecordsCache.Count > 0)
                {
                    var record = FSReclassifierCache.Instance.RecordsCache.Take();
                    var fullPath = Path.Combine(record.DirPath.TrimEnd('\\').TrimEnd('/'), record.LeafName);
                    try
                    {
                        logger.Debug("Begin to reclassify the object:{0}", record.Id);
                        if (record.TermId == FSReclassifierCache.Instance.Term.UniqueId)
                        {
                            logger.Debug("The term for the object {0} is not changed. And it's skipped.", record.Id);
                            continue;
                        }
                        bool changeResult = false;
                        if (record.NodeType == (int)NodeLevel.FSFile)
                        {
                            if (!IsSameTermScope(record))
                            {
                                logger.Debug("This file :{} is not in the same term scope.", record.Id);
                                SendDetail(record, JobDetailsStatus.Failed, "RM_FS_FolderReclassify_FileNotInSameTermScope");
                                mFailedCount++;
                                continue;
                            }
                            logger.Debug("Begin to assign new term to the object.");
                            changeResult = AssignTerm(record);
                            if (changeResult)
                            {
                                logger.Debug("Begin to bind new rule to the object.");
                                ruleUtil.AssembleRule(record);
                                logger.Debug("Start to update the record in Records table.");
                                _explorerDao.AddOrUpdateRecord(record, true);
                                if (!FSReclassifierCache.Instance.FolderIds.Contains(record.Id))
                                {
                                    AddRecordsHistory(record.Id);
                                }
                            }
                        }

                        if (record.NodeType == (int)NodeLevel.FSFolder)
                        {
                            logger.Debug($"Begin to load children from Records table. Folder is [{record.Id}]");
                            FSReclassifierCache.Instance.RecordsCache.AddBatch(await discover.ProcessFilesAsync(record.Id));
                            FSReclassifierCache.Instance.RecordsCache.AddBatch(discover.ProcessSubFolders(fullPath));
                        }
                        else
                        {
                            if (changeResult)
                            {
                                SendDetail(record, JobDetailsStatus.Successful);
                                mSucceedCount++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Failed to reclassify the object:{0}. Exception:{1}", fullPath, ex.ToString());
                        SendDetail(record, JobDetailsStatus.Failed, ex.Message);//TODO xwwang message
                        mFailedCount++;
                    }
                    finally
                    {
                        ReportManager.Increase();
                    }
                }
                logger.Info("There is no more records to process. Start to send the job summary to Manager.");

            }
            catch (JobStopException)
            {
                logger.Warn("This Job is stopped.");
                _jobHasStopped = true;
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while runnning. ", e.ToString());
                mFailedCount++;
            }
            finally
            {
                var finalStatus = JobStatus.None;
                if (_jobHasStopped)
                {
                    finalStatus = JobStatus.Stopped;
                }
                else
                {
                    if (mSucceedCount == 0 && mFailedCount > 0)
                    {
                        finalStatus = JobStatus.Failed;
                    }
                    else if (mSucceedCount > 0 && mFailedCount > 0)
                    {
                        finalStatus = JobStatus.FinishWithException;
                    }
                    else
                    {
                        finalStatus = JobStatus.Finished;
                    }
                }
                ReportManager.SetJobFinished(finalStatus);
                logger.Info($"Job finished.");
            }
        }

        public async Task RunForGlobalSearchActionAsync(List<Record> records, Hashtable processedFolderId)
        {
            logger.Info($"ClassificationLevel: {ClassificationLevel}");
            rootFolders = records.Select(a => a.Id).ToList();
            FSReclassifierCache.Instance.RecordsCache.AddBatch(records);
            var newFolderIds = records.Where(r => !FSReclassifierCache.Instance.FolderIds.Contains(r.NodeId)).Select(r => r.NodeId).ToList();
            if (newFolderIds != null && newFolderIds.Count > 0)
            {
                FSReclassifierCache.Instance.FolderIds.AddRange(newFolderIds);
            }
            var ruleUtil = new FSRuleUtil(FSReclassifierCache.Instance.Rules);
            FSFolderJobDiscover discover = new FSFolderJobDiscover();

            while (FSReclassifierCache.Instance.RecordsCache.Count > 0)
            {
                var record = FSReclassifierCache.Instance.RecordsCache.Take();
                var fullPath = Path.Combine(record.DirPath.TrimEnd('\\').TrimEnd('/'), record.LeafName);
                try
                {
                    logger.Debug("Begin to reclassify the object:{0}", record.Id);
                    if (record.NodeType == (int)NodeLevel.FSConnectionGroups || record.NodeType == (int)NodeLevel.FSConnectionGroup)
                    {
                        logger.Debug("This item is fs connection groups or fs connection group. id:{0}", record.Id);
                        continue;
                    }
                    if (record.TermId == FSReclassifierCache.Instance.Term.UniqueId && record.NodeType == (int)NodeLevel.FSFile)
                    {
                        logger.Debug("The term for the object {0} is not changed. And it's skipped.", record.Id);
                        continue;
                    }
                    bool changeResult = false;
                    if (record.NodeType == (int)NodeLevel.FSFile)
                    {
                        changeResult = innerProcessRecord(record, ruleUtil);
                    }

                    if (record.NodeType == (int)NodeLevel.FSFolder)
                    {
                        if (processedFolderId.ContainsKey(record.NodeId))
                        {
                            logger.Info($"Folder has already been processed. id:{record.Id}");
                            continue;
                        }
                        else
                        {
                            if (ClassificationLevel == (int)NodeLevel.FSFolder)
                            {
                                changeResult = innerProcessRecord(record, ruleUtil); 
                            }
                            processedFolderId.Add(record.NodeId, null);
                        }
                        logger.Debug($"Begin to load children from Records table. Folder is [{record.Id}]");
                        if (ClassificationLevel == (int)NodeLevel.FSFile)
                        {
                            FSReclassifierCache.Instance.RecordsCache.AddBatch(await discover.ProcessFilesAsync(record.Id));
                        }
                        FSReclassifierCache.Instance.RecordsCache.AddBatch(discover.ProcessSubFolders(fullPath));
                    }

                    if (changeResult)
                    {
                        SendDetailForGlobalSearch(record, JobDetailsStatus.Successful);
                        mSucceedCount++;
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("Failed to reclassify the object:{0}. Exception:{1}", fullPath, ex.ToString());
                    SendDetailForGlobalSearch(record, JobDetailsStatus.Failed, ex.Message);//TODO xwwang message
                    mFailedCount++;
                }
                finally
                {
                    ReportManager.Increase();
                }
            }
        }

        private bool innerProcessRecord(Record record, FSRuleUtil ruleUtil)
        {
            if (!IsSameTermScope(record))
            {
                logger.Debug("This file :{} is not in the same term scope.", record.Id);
                SendDetailForGlobalSearch(record, JobDetailsStatus.Failed, "RM_FS_FolderReclassify_FileNotInSameTermScope");
                mFailedCount++;
                return false;
            }
            logger.Debug("Begin to assign new term to the object.");
            bool changeResult = AssignTerm(record);
            if (changeResult)
            {
                if (ClassificationLevel == (int)NodeLevel.FSFile)
                {
                    logger.Debug("Begin to bind new rule to the object.");
                    ruleUtil.AssembleRule(record);
                    logger.Debug("Start to update the record in Records table.");
                    _explorerDao.AddOrUpdateRecordWithKeepManual(record, true, isKeepManualColumn: false);
                    if (!FSReclassifierCache.Instance.FolderIds.Contains(record.Id))
                    {
                        AddRecordsHistory(record.Id);
                    }
                }
                else
                {
                    logger.Debug("Start to update the fsfolder record in Records table.");
                    _explorerDao.AddOrUpdateRecord(record, true);
                    AddRecordsHistory(record.Id);
                }
            }
            return changeResult;
        }

        private void LoadRules()
        {
            try
            {
                logger.Info("Begin to Load rules.");
                Rules = RuleManagerService.GetRulesFromRecords();
                logger.Info("End to load Rules");
            }
            catch (Exception e)
            {
                logger.Error($"LoadRules Error: {e}");
                throw new Exception(I18NEntity.GetString("RM_JS_DocAve_CommunicationError"));
            }
        }

        private bool AssignTerm(Record record)
        {
            if (record.TermId == null || record.TermId == Guid.Empty || mChangeAllFile || (ClassificationLevel == (int)NodeLevel.FSFolder && rootFolders.Contains(record.Id)))
            {
                if (IsNewLogicAccount && record.TermId != FSReclassifierCache.Instance.Term.UniqueId) record.RemoveManualFields();
                record.TermId = FSReclassifierCache.Instance.Term.UniqueId;
                record.TermName = FSReclassifierCache.Instance.Term.Name;
                logger.Info("Assign new term succefully.");
                return true;
            }
            return false;
        }

        private void AddRecordsHistory(Guid recordId)
        {
            try
            {
                //logger.Info("Begin to add history for reclassify action");
                //RecordHistoryXml historyXml = new RecordHistoryXml();
                //RecordHistory history = new RecordHistory
                //{
                //    Action = "RM_BCM_Audit_Action_ChangeTerm",
                //    DisplayTime = DateTime.UtcNow.ToString(),
                //    TimeUTC = DateTime.UtcNow.Ticks,
                //    User = TenantLocalValue.LogonUserEmail
                //};
                //var list = new List<RecordHistory>
                //{
                //    history
                //};
                //historyXml.HistoryList = list;
                //List<Guid> records = new List<Guid>
                //{
                //    recordId
                //};
                //_explorerDao.AddReocrdHistory(records, historyXml);
                RecordsHistoryService.AddRecordsHistory(new List<Guid> { recordId }, "RM_BCM_Audit_Action_ChangeTerm", _jobContextDto.Comment);
            }
            catch (Exception e)
            {
                logger.Warn("Failed to add records histroy for reclassify action {0}", e.ToString());
            }
        }

        private void SendDetail(Record record, JobDetailsStatus status, string comment = "")
        {
            ReportManager.SendJobDetail(new JMFSReclassifierJobDetails()
            {
                FinishTime = DateTime.UtcNow.Ticks,
                ObjectName = record.LeafName,
                FullPath = GetFullPath(record), //Path.Combine(record.DirPath, record.LeafName),
                ItemType = record.NodeType,
                Status = status,
                Comment = comment
            });
        }

        private void SendDetailForGlobalSearch(Record record, JobDetailsStatus status, string comment = "")
        {
            ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
            {
                ObjectName = record.LeafName,
                FullPath = GetFullPath(record),  //Path.Combine(record.DirPath, record.LeafName),
                Type = record.NodeType == (int)NodeLevel.FSFile ? "RM_JM_GlobalSearch_FSFileType" : "RM_JM_GlobalSearch_FSFolderType",
                Action = "RM_JS_BCM_Explorer_ChangeTerm",
                Status = status,
                Comment = comment
            });
        }

        private bool IsSameTermScope(Record record)
        {
            var connection = GetFSConnection(record);
            if (connection == null)
            {
                logger.Debug($"Cannot find connection. Id:{record.AveSiteId}");
                throw new Exception("Connection Not Found.");
            }
            var nodeBinds = mAllsettings.Where(s => s.IdPath.Contains(record.AveSiteId)).ToDictionary(s => s.FullPath);

            RMFileSystemSetting bindSetting;
            string settingPath;
            bool bTemp;

            settingPath = record.NodeType == (int)NodeLevel.FSFile ? record.DirPath : Path.Combine(record.DirPath, record.LeafName);
            var tempPath = settingPath;
            do
            {
                bTemp = nodeBinds.TryGetValue(tempPath, out bindSetting);
                if (bTemp)
                {
                    break;
                }
                tempPath = tempPath.Substring(0, tempPath.LastIndexOf('\\'));
            } while (tempPath.Length >= connection.UNCPath.Length);
            if (!bTemp)
            {
                var groupSetting = mAllsettings.FirstOrDefault(s => s.ScopeId == connection.GroupId);
                if (groupSetting == null)
                {
                    throw new Exception("Group setting not init.");
                }
                bindSetting = groupSetting;
                nodeBinds[settingPath] = bindSetting;
            }
            if (CheckTermValue(bindSetting, FSReclassifierCache.Instance.Term.UniqueId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private FSConnection GetFSConnection(Record record)
        {
            var connectionId = new Guid(record.AveSiteId);
            if (mFSConnectionDic.ContainsKey(connectionId))
            {
                return mFSConnectionDic[connectionId];
            }
            else
            {
                var connection = FSConnectionDao.GetConnectionById(connectionId);
                mFSConnectionDic.Add(connectionId, connection);
                return connection;
            }
        }

        private bool CheckTermValue(RMFileSystemSetting setting, Guid termId)
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

        public int GetClassificationLevel()
        {
            RMFunctionSetting setting;
            FunctionSettingDao.TryGet(AvePoint.RA.Contract.FunctionSetting.FunctionSettingType.ClassificationLevelSetting, out setting);
            NodeLevel result;
            if (setting == null)
            {
                return (int)NodeLevel.FSFile;
            }
            if (Enum.TryParse<NodeLevel>(setting.SettingInfo, out result))
            {
                return (int)result;
            }
            return (int)NodeLevel.FSFolder;
        }

        public string GetFullPath(Record record)
        {
            string FullPath = Path.Combine(record.DirPath, record.LeafName);
            return FullPath.Replace('/', '\\');
        }

        public void Dispose()
        {
            if (_explorerDao != null)
            {
                _explorerDao.Dispose();
            }
        }
    }
}
