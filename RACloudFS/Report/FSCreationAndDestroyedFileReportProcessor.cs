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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.Records.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using System.IO;
using AvePoint.RA.Contract;

namespace RACloudFS.Report
{
    public class FSCreationAndDestroyedFileReportProcessor
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(FSCreationAndDestroyedFileReportProcessor));
        private AvePoint.RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public AvePoint.RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new AvePoint.RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }

        private AvePoint.RA.DB.Dao.IFSConnectionDao _fsConnDao;
        public AvePoint.RA.DB.Dao.IFSConnectionDao FSConnDao
        {
            get
            {
                if (_fsConnDao == null)
                {
                    _fsConnDao = new AvePoint.RA.DB.Dao.Impl.FSConnectionDao();
                }
                return _fsConnDao;
            }
        }

        private NodeLevel ClassificationLevel = NodeLevel.FSFile;
        private List<RMTerm> Terms { get; set; }
        private bool SelectCreated;
        private bool SelectDestroyed;
        private DateTime startUtcTime;
        private DateTime endUtcTime;
        private string profileId;
        private List<Guid> deactiveFoldId = new List<Guid>();
        private List<RMFileSystemSetting> deactiveSetting = new List<RMFileSystemSetting>();
        private List<FSTreeNodeDto> fsNodeDtoList;
        private bool _jobHasException = false;
        private bool _jobHasStopped = false;
        private Dictionary<Guid, RMRuleInfos> idRuleInfoDic = new Dictionary<Guid, RMRuleInfos>();
        private IRuleManagerService mRuleManagerService;
        private string fsAzureTableConnectStr = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING];
        private Dictionary<int, RMAccount> cacheAllUsers;
        protected IRuleManagerService RuleManagerService
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
        public IRMReportService mReportService { get; set; }
        protected IRMReportService ReportService
        {
            get
            {
                if (mReportService == null)
                {
                    mReportService = (IRMReportService)PlatformWindsorManager.GetService(typeof(IRMReportService));
                }
                return mReportService;
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

        private IFileSystemSettingDao _FileSystemSettingDao = null;
        public IFileSystemSettingDao FileSystemSettingDao
        {
            get
            {
                if (_FileSystemSettingDao == null)
                {
                    _FileSystemSettingDao = (IFileSystemSettingDao)PlatformWindsorManager.GetService(typeof(IFileSystemSettingDao));
                }
                return _FileSystemSettingDao;
            }
        }
        private IArchiverTableDao mArchiverTableDao;
        public IArchiverTableDao ArchiverTableDao
        {
            get
            {
                if (mArchiverTableDao == null)
                {
                    mArchiverTableDao = (IArchiverTableDao)PlatformWindsorManager.GetService(typeof(IArchiverTableDao));
                }
                return mArchiverTableDao;
            }
        }

        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        public FSReportManager FSReportManager { get; set; }
        public async Task RunJobAsync(RMCreationJobMessage msg)
        {
            await InitParameters(msg);
            await ProcessAsync();
        }


        private async Task InitParameters(RMCreationJobMessage msg)
        {
            ReportManager.StartUpdateJobProgress();
            ClassificationLevel = msg.JobID.IndexOf("_") != -1 ? NodeLevel.FSFolder : NodeLevel.FSFile;
            var globalTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(msg.GlobalTimeZoneId.Replace("_", " "));
            startUtcTime = TimeZoneInfo.ConvertTimeToUtc(msg.StartTime, globalTimeZone);
            endUtcTime = TimeZoneInfo.ConvertTimeToUtc(msg.EndTime.AddDays(1), globalTimeZone);//包含当天
            SelectCreated = msg.SelectCreated;
            SelectDestroyed = msg.SelectDestroyed;
            profileId = msg.ProfileId;
            FSReportManager = new FSReportManager(profileId, AvePoint.RA.Contract.JobMonitor.JobType.FSCreateAndDestroyedFileReport);
            fsNodeDtoList = await FSReportManager.AssembleAllTreeNodeForFSAsync();
            cacheAllUsers = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
            LoadTerms();
        }

        private async Task ProcessAsync()
        {
            try
            {
                mLog.Info($"Classification Level:{ClassificationLevel}, start {startUtcTime}, end {endUtcTime}");
                using (new CheckJobStopScope())
                {
                    GetDeactiveFoldId();
                    await InitRulesInfoAsync();
                    ProcessSelectedNode(fsNodeDtoList);                     
                }
            }
            catch (JobStopException)
            {
                mLog.Warn("This Job is stopped.");
                _jobHasStopped = true;
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred while runnning. ", e.ToString());
                _jobHasException = true;
                throw;
            }
            finally
            {
                var finalStatus = _jobHasStopped ? JobStatus.Stopped : _jobHasException ? JobStatus.FinishWithException : JobStatus.Finished;
                ReportManager.SetJobFinished(finalStatus);
            }
        }

        public void ProcessSelectedNode(List<FSTreeNodeDto> treeNodes)
        {
            foreach (var treeNode in treeNodes)
            {
                bool isDeactived = FileSystemSettingDao.IsDeactivedNode(GetSelectNodeIdPath(treeNode));
                if (!isDeactived)
                {
                    Guid id = new Guid(treeNode.ID);
                    var folderId = IsFSConnection(id) ? treeNode.FullPath.ToLowerInvariant().ToMd5() : id;
                    var folder = ExplorerDao.GetFSRecordById(folderId);
                    if (folder != null)
                    {
                        ProcessFolder(folder, treeNode.FullPath);
                    }
                    if(ClassificationLevel == NodeLevel.FSFile)
                    {
                        ProcessSubFolders(treeNode);   
                    }
                }
                else
                {
                    mLog.Warn("This select node is deactive,node name is {0}",treeNode.Name);
                }               
            }
        }
        public void ProcessSubFolders(FSTreeNodeDto treeNode)
        {
            bool hasNext = true;
            string pageIndex = string.Empty;
            var pateSize = 5000;
            List<Record> datas = new List<Record>();
            var parentFullPath = treeNode.FullPath;
            if (!parentFullPath.EndsWith("\\"))
            {
                parentFullPath += "\\";
            }
            while (hasNext)
            {
                Tuple<IEnumerable<Record>, string> result = ExplorerDao.QueryByPage(o =>
                o.SourceFlag == (int)SourceFlag.FileSystem
                && (o.DirPath.Contains(parentFullPath) || o.DirPath == treeNode.FullPath)
                && o.NodeType == (int)RMNodeLevel.FSFolder, pateSize, pageIndex);
                hasNext = !string.IsNullOrEmpty(result.Item2);
                pageIndex = result.Item2;
                datas = result.Item1.ToList();
                foreach (var folder in datas)
                {
                    if (!deactiveFoldId.Contains(folder.Id))
                    {
                        ProcessFolder(folder, treeNode.FullPath);
                    }
                    else
                    {
                        mLog.Warn("The folder is deactive,name is{0}", folder?.Id);
                    }
                }
            }
        }

        public void ProcessFolder(Record record, string fullPath)
        {
            try
            {
                var createResult = false;
                if (SelectCreated && ClassificationLevel == NodeLevel.FSFile)
                {
                    createResult = IsMatchOnCreateTime(record);
                }
                if (createResult)
                {
                    GenerateJobDetailItem(record, JobDetailsStatus.Successful);
                    GenerateReportItem(record, true, false);
                }
                if (ClassificationLevel == NodeLevel.FSFile)
                {
                    ProcessFiles(record.Id);
                }
                else
                {
                    //如果是fsfolder level， 只能是Destroyed folder 
                    ProcessFilesFromAzureTable(record, fullPath);
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"Process folder has error:{ex}");
                GenerateJobDetailItem(record, JobDetailsStatus.Failed, ex.Message);
            }
        }

        public void ProcessFiles(Guid folderId)
        {
            bool hasNext = true;
            string pageIndex = string.Empty;
            var pateSize = 5000;
            List<Record> datas = new List<Record>();
            while (hasNext)
            {
                Tuple<IEnumerable<Record>, string> result = ExplorerDao.QueryByPage(o =>
                o.SourceFlag == (int)SourceFlag.FileSystem
                && o.ParentId == folderId && o.RecordStatus !=4
                && o.NodeType == (int)RMNodeLevel.FSFile, pateSize, pageIndex);
                hasNext = !string.IsNullOrEmpty(result.Item2);
                pageIndex = result.Item2;
                datas = result.Item1.ToList();
                foreach (var file in datas)
                {
                    ProcessFile(file);
                }
            }
        }

        public void ProcessFile(Record record)
        {
            try
            {
                var createResult = false;
                var destoryResult = false;

                if (SelectDestroyed)
                {
                    destoryResult = IsMatchOnDestroyedTime(record);
                }
                if (SelectCreated && !destoryResult)
                {
                    createResult = IsMatchOnCreateTime(record);
                }
                if (createResult)
                {
                    GenerateJobDetailItem(record, JobDetailsStatus.Successful);
                    GenerateReportItem(record, true, false);
                }
                if (destoryResult)
                {
                    GenerateJobDetailItem(record, JobDetailsStatus.Successful);
                    GenerateReportItem(record, false, true);
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"Process box has error:{ex}");
                GenerateJobDetailItem(record, JobDetailsStatus.Failed, ex.Message);
            }
        }

        public void ProcessFilesFromAzureTable(Record folder, string fullPath)
        {
            mLog.Info("Process data from fs archiver db");
            Guid folderId = folder.Id;
            Guid scopeId = folder.ScopeId;
            if (!fullPath.EndsWith("\\"))
            {
                fullPath += "\\";
            }
            var pageSize = 1000;
            var continuationToken = string.Empty;
            var pageIndex = 1;
            var totalMatched = 0;

            mLog.Info($"Start query fs destroy table. ScopeId: {scopeId}, FullPath: {fullPath}, StartUtc: {startUtcTime:o}, EndUtc: {endUtcTime:o}, PageSize: {pageSize}");

            do
            {
                var pageResult = ArchiverTableDao.GetDestroyItemForFSDesctruntionByConnectionIdByPage(fsAzureTableConnectStr, TenantLocalValue.LogonGroupId, scopeId.ToString(), pageSize, continuationToken);
                var pageEntities = pageResult.Item1?.ToList() ?? new List<FileSystemTableEntity>();
                continuationToken = pageResult.Item2;

                var needProcessEntities = pageEntities.Where(e =>
                                                            e.FullPath.StartsWith(fullPath)
                                                            && e.AchiveTime > startUtcTime
                                                            && e.AchiveTime < endUtcTime
                                                            && e.Status == (int)SOApproveDBStatus.Archived
                                                            && e.RuleAction == 1
                                                            && !deactiveFoldId.Contains(e.ParentID))
                                                            .ToList();

                totalMatched += needProcessEntities.Count;
                mLog.Info($"FS destroy table page {pageIndex}, fetched {pageEntities.Count}, matched {needProcessEntities.Count}, totalMatched {totalMatched}, hasNext {!string.IsNullOrEmpty(continuationToken)}");

                if (!needProcessEntities.IsNullOrEmpty())
                {
                    foreach (var entity in needProcessEntities)
                    {
                        GenerateJobDetailItem(entity, JobDetailsStatus.Successful);
                        GenerateReportItem(entity, folder);
                    }
                }

                pageIndex++;
            }
            while (!string.IsNullOrEmpty(continuationToken));

            mLog.Info($"Destroyed entity total matched {totalMatched}");
        }

        private void LoadTerms()
        {
            mLog.Info("Begin to load terms.");
            Terms = new TermDao().GetAllTermsForce();
            mLog.Info("Loaded {0} terms.", Terms.Count);
        }

        public string GetSelectNodeIdPath(FSTreeNodeDto node)
        {
            string result = node.ID;
            while (node != null && node.Level != NodeLevel.WebApplication)
            {
                node = node.Parent;
                if (node != null)
                {
                    result = result == "" ? node.ID : node.ID + "|" + result;
                }
            }
            return result;
        }



        private void GetDeactiveFoldId()
        {
            List<Guid> GroupIds = FSReportManager.GetAllGroupIds();
            foreach(var groupId in GroupIds)
            {
                deactiveSetting.AddRange(FileSystemSettingDao.GetAllDeactiveUnderGroup(groupId));
            }
            //获取每个setting下所有存在于custom db里的folder的Id
            foreach (RMFileSystemSetting setting in deactiveSetting)
            {
                if (!deactiveFoldId.Contains(setting.ScopeId))
                {
                    deactiveFoldId.Add(setting.ScopeId);
                }
                string settingPath = setting.FullPath;
                List<Record> deactiveSubFold = ExplorerDao.QueryAll(f => f.SourceFlag == (int)SourceFlag.FileSystem
                                                                    && f.DirPath.Contains(settingPath) && f.NodeType == (int)RMNodeLevel.FSFolder).ToList();
                if (!deactiveSubFold.IsNullOrEmpty())
                {
                    foreach (Record foldRecord in deactiveSubFold)
                    {
                        if (!deactiveFoldId.Contains(foldRecord.Id))
                        {
                            deactiveFoldId.Add(foldRecord.Id);
                        }
                    }
                }
            }
        }

        public void AddJobDetail(JMJobDetails detail)
        {
            ReportManager.SendJobDetail(detail);
            ReportManager.Increase(1);
        }

        public void AddJobReport(BaseReport report)
        {
            ReportManager.SendJobReport(report);
            ReportManager.Increase(1);
        }

        private void GenerateJobDetailItem(Record fsNode, JobDetailsStatus status, string comments = "")
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail
            {
                ObjectLevel = GetI18NStringForNodeType(fsNode.NodeType),
                Title = fsNode.LeafName,
                URL = GetFSNodeFullPath(fsNode),
                Status = status,
                Comment = comments
            };
            AddJobDetail(detail);
        }

        private void GenerateReportItem(Record fsNode, bool Created, bool Destroyed)
        {
            var report = new CreateAndDestroyedFileReport();
            if (Terms.Any(o => o.UniqueId == fsNode.TermId))
            {
                report.TermName = Terms.Where(o => o.UniqueId == fsNode.TermId).First().Name;
            }
            report.Title = fsNode.LeafName;
            report.LevelStr = fsNode.NodeType;
            report.Url = GetFSNodeFullPath(fsNode);
            report.CreatedTime = fsNode.TimeCreated;
            report.LastModifiedTime = fsNode.TimeModified;
            report.FileType = fsNode.NodeType == (int)NodeLevel.FSFolder ? "RM_Common_ObjectLevel_Folder" :  fsNode.ExtensionForFile;
            if (Created)
            {
                report.OperationTime = fsNode.TimeCreated.Equals(DateTime.MinValue) ? string.Empty : fsNode.TimeCreated.ToString();
                report.OperationBy = fsNode.CreatedBy;
                report.Operation = (int)OperationType.Created;
            }
            if (Destroyed)
            {
                if (fsNode?.ManualArchiveStatus == (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd || fsNode?.ManualApprovedStatus == (int)SOApproveDBStatus.Archived)
                {
                    report.ApprovalStatus = fsNode.ManualApprovedStatus == (int)SOApproveDBStatus.Archived ? (int)SOApproveDBStatus.Approved : fsNode.ManualApprovedStatus;
                    if (fsNode.ManualApprovedStatus != (int)SOApproveDBStatus.Archived)
                    {
                        if (fsNode.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WaitingApprove || fsNode.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress)
                        {
                            report.InternalApprovedStatus = (int)SOApproveDBStatus.Cancelled;
                            report.ApprovalStatus = (int)SOApproveDBStatus.Cancelled;
                        }
                        else
                        {
                            report.InternalApprovedStatus = fsNode.ManualInternalApprovedStatus;
                        }
                    }
                    else
                    {
                        if (fsNode.ManualWorkflowDefinitionId.Equals(Guid.Empty))
                        {
                            report.InternalApprovedStatus = (int)SOApproveDBStatus.Approved;
                        }
                        else
                        {
                            report.InternalApprovedStatus = (int)SOApproveDBStatus.WorkflowComplete;
                        }
                    }
                }
                report.DisposalClass = GetRuleInfo(fsNode.RuleId)?.DisposalClass;
                report.OperationTime = fsNode.DestroyedTime.Equals(DateTime.MinValue) ? string.Empty : fsNode.DestroyedTime.ToString();
                report.OperationBy = I18NEntity.GetString("RM_RC_TimeFrame_ArchiverByRASystem");
                report.Operation = (int)OperationType.Destroyed;
                report.RecordsId = fsNode.RecordsId;
                report.RuleName = GetRuleInfo(fsNode.RuleId)?.RuleName;
                if (cacheAllUsers.TryGetValue(fsNode.ManualApprovedBy, out RMAccount approveUser) && fsNode.ManualApprovedStatus != (int)AvePoint.RA.Contract.SOApproveDBStatus.Rejected)
                {
                    report.ApprovedBy = approveUser.DisplayName;
                    report.ApprovedByUPN = approveUser.UserPrincipalName;
                }
            }
            AddJobReport(report);
        }
        private void GenerateJobDetailItem(FileSystemTableEntity entity, JobDetailsStatus status, string comments = "")
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail
            {
                ObjectLevel = "RM_JS_Rule_ObjectLevel_FSFile",
                Title = entity.LowName,
                URL = entity.FullPath,
                Status = status,
                Comment = comments
            };
            AddJobDetail(detail);
        }

        private void GenerateReportItem(FileSystemTableEntity entity, Record parent)
        {
            var report = new CreateAndDestroyedFileReport();
            if (Terms.Any(o => o.UniqueId == parent.TermId))
            {
                report.TermName = Terms.Where(o => o.UniqueId == parent.TermId).First().Name;
            }
            report.Title = entity.LowName;
            report.LevelStr = (int)NodeLevel.FSFile;
            report.Url = entity.FullPath; 
            report.DisposalClass = GetRuleInfo(new Guid(entity.RuleId))?.DisposalClass;
            report.OperationTime = entity.AchiveTime.Equals(DateTime.MinValue) ? string.Empty : entity.AchiveTime.Ticks.ToString();
            report.OperationBy = I18NEntity.GetString("RM_RC_TimeFrame_ArchiverByRASystem");
            report.Operation = (int)OperationType.Destroyed;
            report.CreatedTime = entity.CreateTime.Ticks;
            report.LastModifiedTime = entity.LastModifiedTme.Ticks;
            report.FileType = Path.GetExtension(entity.FullPath).TrimStart(['.']);
            report.RuleName = GetRuleInfo(new Guid(entity.RuleId))?.RuleName;
            try
            {
                if (entity.MovedToApprovalTable)
                {
                    var fsNode = ExplorerDao.GetFirstOrDefault(r => r.Id == new Guid(entity.RowKey));
                    if(fsNode?.ManualArchiveStatus == (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd || fsNode?.ManualApprovedStatus == (int)SOApproveDBStatus.Archived)
                    {
                        report.ApprovalStatus = fsNode.ManualApprovedStatus == (int)SOApproveDBStatus.Archived ? (int)SOApproveDBStatus.Approved : fsNode.ManualApprovedStatus;
                        if (fsNode.ManualApprovedStatus != (int)SOApproveDBStatus.Archived)
                        {
                            if (fsNode.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WaitingApprove || fsNode.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress)
                            {
                                report.InternalApprovedStatus = (int)SOApproveDBStatus.Cancelled;
                                report.ApprovalStatus = (int)SOApproveDBStatus.Cancelled;
                            }
                            else
                            {
                                report.InternalApprovedStatus = fsNode.ManualInternalApprovedStatus;
                            }
                        }
                        else
                        {
                            if (fsNode.ManualWorkflowDefinitionId.Equals(Guid.Empty))
                            {
                                report.InternalApprovedStatus = (int)SOApproveDBStatus.Approved;
                            }
                            else
                            {
                                report.InternalApprovedStatus = (int)SOApproveDBStatus.WorkflowComplete;
                            }
                        }
                    }
                    if (fsNode != null)
                    {
                        if (cacheAllUsers.TryGetValue(fsNode.ManualApprovedBy, out RMAccount approveUser) && fsNode.ManualApprovedStatus != (int)AvePoint.RA.Contract.SOApproveDBStatus.Rejected)
                        {
                            report.ApprovedBy = approveUser.DisplayName;
                            report.ApprovedByUPN = approveUser.UserPrincipalName;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"Get file destroy error {e}");
            }

            AddJobReport(report);
        }

        private bool IsMatchOnCreateTime(Record record)
        {
            bool result = false;
            if (record != null && record.TimeCreated > startUtcTime.Ticks && record.TimeCreated < endUtcTime.Ticks)
            {
                result = true;
            }
            return result;
        }

        private bool IsMatchOnDestroyedTime(Record record)
        {
            bool result = false;
            if (record != null && record.DestroyedTime > startUtcTime.Ticks && record.DestroyedTime < endUtcTime.Ticks)
            {
                result = true;
            }
            return result;
        }

        private bool IsFSConnection(Guid id)
        {
            return FSConnDao.GetConnectionById(id) != null;
        }

        private string GetFSNodeFullPath(Record fsNode)
        {
            var dirPath = fsNode.DirPath.TrimEnd(new char[] { '\\' });
            return $@"{dirPath}\{fsNode.LeafName}";
        }

        private string GetI18NStringForNodeType(int nodeType)
        {
            string strNodeType = "";
            switch (nodeType)
            {
                case (int)RMNodeLevel.FSFolder:
                    strNodeType = "RM_JS_Rule_ObjectLevel_FSFolder";
                    break;
                case (int)RMNodeLevel.FSFile:
                    strNodeType = "RM_JS_Rule_ObjectLevel_FSFile";
                    break;
                default:
                    break;
            }
            return strNodeType;
        }
        private async Task InitRulesInfoAsync()
        {
            using (var performance = new PerformanceScope($"Report.GetRules"))
            {
                var dbRules = await RuleManagerService.GetSimpleRecordsRulesFromDBAsync();
                if (dbRules.Count > 0)
                {
                    idRuleInfoDic = dbRules.ToDictionary(key => new Guid(key.RuleId), value => value);
                }
            }
        }

        private RMRuleInfos GetRuleInfo(Guid id)
        {
            return idRuleInfoDic.ContainsKey(id) ? idRuleInfoDic[id] : null;
        }
    }
    internal enum OperationType
    {
        Created = 0,
        Destroyed = 1
    }
}


