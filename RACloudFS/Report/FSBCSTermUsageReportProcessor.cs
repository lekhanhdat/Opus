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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.Records.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RACloudFS.Report
{
    public class FSBCSTermUsageReportProcessor
    {
        private static RALogger logger = RALogger.GetInstance(typeof(FSBCSTermUsageReportProcessor));

        protected bool mJobHasException = false;
        protected bool mJobHasStopped = false;
        private int ClassificationLevel;

        private Dictionary<Guid, RMTermIdentity> mUsageTermInfo;
        //private RMFSTreeNode fsTreeNode;
        private List<FSTreeNodeDto> fsNodeDtoList;
        private bool isOrphanedTermReport;
        private bool mIsRetiredTermReport;
        private List<Guid> deactiveFoldId = new List<Guid>();
        private List<RMFileSystemSetting> deactiveSetting = new List<RMFileSystemSetting>();
        private IRMReportService mReportService;
        private IRMFunctionSettingDao FunctionSettingDao = PlatformWindsorManager.GetService<IRMFunctionSettingDao>();

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

        private AvePoint.RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public AvePoint.RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new AvePoint.RA.DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
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

        private ITermDao TermDao;
        public FSReportManager FSReportManager { get; set; }
        public FSBCSTermUsageReportProcessor(string jobId, string profileId, bool IsOrphanedTermReport, bool isRetiredTermReport)
        {
            ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.FSBCSTermUsageReport, true);
            RMProfileDto profile = ReportService.GetProfileByIdForReportJob(profileId);
            FSReportManager = new FSReportManager(profileId, AvePoint.RA.Contract.JobMonitor.JobType.FSBCSTermUsageReport);
            fsNodeDtoList = FSReportManager.AssembleAllTreeNodeForFSAsync().Result;
            isOrphanedTermReport = IsOrphanedTermReport;
            mIsRetiredTermReport = isRetiredTermReport;
            if (IsOrphanedTermReport)
            {
                mUsageTermInfo = ReportService.GetOrphanedTermsOfRMAsync().Result;
            }
            else if (isRetiredTermReport)
            {
                mUsageTermInfo = ReportService.GetRetiredTermsOfRMAsync().Result;
            }
            else
            {
                mUsageTermInfo = ReportService.GetTermIDsFromBCSTermTreeAsync(profile.Extension1).Result;
            }
            TermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
            SendUsageTermDetails();
        }

        private void SendUsageTermDetails()
        {
            List<JMJobDetails> details = new List<JMJobDetails>();
            foreach (var term in mUsageTermInfo.Values)
            {
                details.Add(new JMTermSelection()
                {
                    Term = term.Name,
                    TermFullPath = term.FullPath
                });
            }
            ReportManager.BatchSendJobDetail(details);
        }

        public void RunReportJob()
        {
            ReportManager.StartUpdateJobProgress();
            if (mUsageTermInfo == null || mUsageTermInfo.Count == 0)
            {
                ReportManager.SetJobFinished(JobStatus.Finished, "RM_RC_TUR_NoTermForReport");
                return;
            }
            try
            {
                GetDeactiveFoldId();
                ProcessSelectedNode(fsNodeDtoList);
            }
            catch (JobStopException ex)
            {
                logger.Warn("This Job is stopped.");
                mJobHasStopped = true;
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while runnning. ", e.ToString());
                mJobHasException = true;
                throw;
            }
            finally
            {
                var finalStatus = JobStatus.Finished;
                if (mJobHasException)
                {
                    finalStatus = JobStatus.FinishWithException;
                }
                if (mJobHasStopped)
                {
                    finalStatus = JobStatus.Stopped;
                }
                ReportManager.SetJobFinished(finalStatus);
            }
        }

        public void ProcessSelectedNode(List<FSTreeNodeDto> treeNodes)
        {
            ClassificationLevel = this.GetClassificationLevel();
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
                        ProcessFolder(folder);
                    }
                    ProcessSubFolders(treeNode);
                }
                else
                {
                    logger.Warn("This select node is deactive,node name is {0}", treeNode.Name);
                }
            }
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
            foreach (var groupId in GroupIds)
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
                        ProcessFolder(folder);
                    }
                    else
                    {
                        logger.Warn("The folder is deactive,name is{0}", folder?.Id);
                    }
                }
            }
        }

        private bool IsFSConnection(Guid id)
        {
            return FSConnDao.GetConnectionById(id) != null;
        }
        public void ProcessFolder(Record record)
        {
            try
            {
                if (IsMatchTerm(record.TermId))
                {
                    if (ClassificationLevel == (int)NodeLevel.FSFolder)
                    {
                        SendReport(record); 
                    }
                    SendDetail(record, JobDetailsStatus.Successful);
                }
                if (ClassificationLevel == (int)NodeLevel.FSFile)
                {
                    ProcessFiles(record.Id); 
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Process folder has error:{ex}");
                mJobHasException = true;
                SendDetail(record, JobDetailsStatus.Failed, ex.Message);
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
                && o.ParentId == folderId
                && o.NodeType == (int)RMNodeLevel.FSFile && o.RecordStatus == 1 , pateSize, pageIndex);
                hasNext = !string.IsNullOrEmpty(result.Item2);
                pageIndex = result.Item2;
                datas = result.Item1.ToList();
                foreach (Record file in datas)
                {
                    ProcessFile(file);
                }
            }
        }

        public void ProcessFile(Record record)
        {
            try
            {
                if (IsMatchTerm(record.TermId))
                {
                    SendReport(record);
                    SendDetail(record, JobDetailsStatus.Successful);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Process file has error:{ex}");
                mJobHasException = true;
                SendDetail(record, JobDetailsStatus.Failed, ex.Message);
            }
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

        //private void ProcessAllFiles(List<Record> fileRecords)
        //{
        //    try
        //    {
        //        foreach (Record fileRecord in fileRecords)
        //        {
        //            if (IsMatchTerm(fileRecord.TermId))
        //            {
        //                SendReport(fileRecord);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        private bool IsMatchTerm(Guid termId)
        {
            return mUsageTermInfo.ContainsKey(termId);
        }

        private void SendReport(Record record)
        {
            BCSTermUsageReport report = new BCSTermUsageReport();
            report.TitleOrName = record.LeafName;
            report.Url = record.DirPath?.TrimEnd('\\') + "\\" + record.LeafName;
            if (record.NodeType == (int)NodeLevel.FSFolder)
            {
                report.ObjectLevel = (int)RMReportObjectLevel.FSFolder;
            }
            else
            {
                report.ObjectLevel = (int)RMReportObjectLevel.FSFile;
            }
            var termUniqueId = record.TermId;
            report.BCSTermId = termUniqueId.ToString();
            report.BCSTermName = mUsageTermInfo[termUniqueId].Name;
            report.TermStatus = mUsageTermInfo[termUniqueId].Status;
            report.BCSTermFullPath = mUsageTermInfo[termUniqueId].FullPath;
            report.CreatedBy = record.CreatedBy;
            report.CreatedTime = record.TimeCreated;
            report.LastModifiedBy = record.ModifiedBy;
            report.LastModifiedTime = record.TimeModified;
            ReportManager.SendJobReport(report);
            ReportManager.Increase(1);
        }


        private void SendDetail(Record record, JobDetailsStatus status, string comments = "")
        {
            JMReportJobDetails detail = new JMReportJobDetails();
            detail.Type = GetI18NStringForNodeType(record.NodeType);
            detail.TitleOrName = record.LeafName;
            detail.Url = record.DirPath + "\\" + record.LeafName;
            detail.Status = status;
            detail.Comment = comments;
            ReportManager.SendJobDetail(detail);
            ReportManager.Increase(1);
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

    }
}
