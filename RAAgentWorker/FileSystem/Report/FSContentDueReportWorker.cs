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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.Media.Storage;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using Microsoft.IdentityModel.Tokens;
using RAFileSystem.FileSystem.Common;
using RAFileSystem.Utils;
using RAFileSystemCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Report
{
    public class FSContentDueReportWorker : IScheduleJobWorker
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IProgressService ProgressService;
        private IReportService<JMJobDetails> JobDetailService;
        private IReportService<BaseReport> ReportManager;
        private IXSystem _system;
        internal static AvePoint.RA.Contract.FileSystem.FSSettingDto currentSetting;
        internal static FSFolderStub _rootStub;
        private Guid DefaultTermId;
        AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto connection;
        private DateTime mTimePoint = DateTime.UtcNow;
        private Dictionary<string, AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> rulesWithTimePointDic = new Dictionary<string,AvePoint.GCommon.Contract.StorageOptimization.Object.Rule>();
        public void Bind(string msgStr)
        {
            try
            {
                ProgressService = JobContext.Current.mProgressManager.Create();
                JobDetailService = JobContext.Current.JobDetailManager.Create();
                ReportManager = JobContext.Current.ReportManager.Create();

                FSJobMessage msg = SerializerHelper.DeserializeByDataContractSerializer<FSJobMessage>(msgStr);
                JobContext.Current.JobMessage = msgStr;

                AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto node = DtoConverter.ConvertGlobalDto2FSTreeNodeDto(msg.FSTreeNodes[0]);  //for now, the sub job can only process one connection.
                logger.Debug(node.ToString().LogBase64());
                connection = node;
                FSJobCache.Instance.RunJobScopePath = node.FullPath;
                mTimePoint = new DateTime(msg.FSTreeNodes[0].TimeStamp);

                System.Tuple<AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto, AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto, AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto> top3Nodes = ExternalUtil.FindTop3LevelNodes(node);
                string path = top3Nodes.Item3.FullPath;
                logger.Debug("The root location is {0}", top3Nodes?.Item3?.ID);
                FSJobCache.Instance.RootPath = path.TrimEnd('\\');
                _system = ExternalUtil.OpenXSystem(FSJobCache.Instance.RootPath);

                string highName = node.FullPath.Substring(path.Length).Trim('\\');
                StorageInfo dirInfo = new StorageInfo() { HighName = highName };
                Guid settingScopeId = QueryScopeTermIdSetting(node);
                FSJobCache.Instance.DispoalSettingScopeId = settingScopeId;
                var setting = FSJobCache.Instance.ScopeSettingCache[settingScopeId];  //跑Job的节点的Setting
                currentSetting = setting;
                DefaultTermId = setting.DefaultTermId;
                if (_system.DirectoryExists(dirInfo))
                {
                    XDirectoryInfo dir = _system.OpenDirectory(dirInfo, FileMode.Open);
                    Guid parentId = string.IsNullOrEmpty(dirInfo.HighName) ?
                        new Guid(top3Nodes.Item2.ID)  //level3  connection
                        : ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, Path.GetDirectoryName(ExternalUtil.CombinePath(dir.HighName, dir.LowName))).ToLowerInvariant().ToMd5();  //sub folder
                    string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dir.HighName, dir.LowName);
                    FSFolderStub rootStub = new FSFolderStub() { MediaObj = dir, FullPath = fullPath, SelfId = fullPath.ToLowerInvariant().ToMd5(), ParentId = parentId, ScopeSettingId = settingScopeId };
                    _rootStub = rootStub; 
                    JobContext.Current.mProgressManager.Create().IncreaseBase(1);
                }
                else
                {
                    JobDetailService.Commit(
                         new JMFSAgentDueDisposalReportJobDetails()
                         { 
                             Url = node.FullPath,
                             Status = JobDetailsStatus.Failed,
                             Comment = "RM_JS_JMD_FS_PathCanNotAccess",
                             Type = "RM_JS_Rule_ObjectLevel_FSFile",
                             //AgentName = OSInformation.HostName
                         });
                    throw new FileNotFoundException("We cannot open the Dir" + node.FullPath);
                }  
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw;
            }
        }

        public void Run()
        {
            try
            {
                var allExceptFolderCache = this.GetAllFolders();
                //缓存与default term不一样的数据，  或Hold的数据。
                FSJobCache.Instance.DisposalDifferentFolderCache.AddRange(allExceptFolderCache.AsEnumerable());
                logger.Info("TimePoint(UTC):{0}", mTimePoint);
                bool parentChecked = connection.CheckNumber == 1; 
                ProcessFolder(_rootStub, connection, false, DefaultTermId); 
            }
            catch (Exception e)
            { 
                logger.Error("An error occurred. Error:" + e.ToString());
                FSJobCache.Instance.FailedCount++;
            }
            finally
            {
                try
                {
                    JobContext.Current.Cleanup();
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while cleaning up. Error:" + e.ToString());
                    FSJobCache.Instance.FailedCount++;
                }
                if (FSJobCache.Instance.FailedCount > 0)
                {
                    JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.FinishWithException, JobContext.Current.JobId);
                }
                else
                {
                    JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.Finished, JobContext.Current.JobId);
                }
            }
        }
        private List<Guid> GetAllDefaultTermId()
        {
            List<Guid> result = new List<Guid>() { currentSetting.DefaultTermId };
            var subSettings = FSJobCache.Instance.ScopeSettingCache.Values.Where(a => a.FullPath.StartsWith(_rootStub.FullPath, StringComparison.InvariantCultureIgnoreCase));
            result.AddRange(subSettings.Select(a => a.DefaultTermId));
            var temp = result.Where(a => a != Guid.Empty).Distinct().ToList();
            logger.Info("Break inherit default term count {0}", temp.Count);
            return temp;
        }
        private List<FileSystemRecordDto> GetAllFolders()
        {
            using (new AgentPerformanceScope("FSDisposal.GetAllDifferentTermFolders", addToStatistics: true))
            {

                AvePoint.RA.Contract.Explorer.SearchFilterParam searchFilterParam = new AvePoint.RA.Contract.Explorer.SearchFilterParam()
                {
                    TermId = currentSetting.DefaultTermId,
                    DataSource = (int)AvePoint.RA.Contract.Explorer.SourceFlag.FileSystem,
                    ScopeId = FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5().ToString(),
                    PageInfo = new AvePoint.RA.Contract.Explorer.SearchPageInfo()
                    {
                        PageIndex = "",
                        PageSize = 100
                    }
                };

                searchFilterParam.Filter = new AvePoint.RA.Contract.Explorer.SearchFilterInfo()
                {
                    NodeTypes = new System.Collections.Generic.List<int> { (int)NodeLevel.FSFolder }
                };
                if (!FSJobCache.Instance.RunJobScopePath.Equals(FSJobCache.Instance.RootPath, StringComparison.OrdinalIgnoreCase))
                {
                    searchFilterParam.Filter.SearchScope = FSJobCache.Instance.RunJobScopePath;
                    searchFilterParam.FolderId = _rootStub.SelfId;
                }
                List<FileSystemRecordDto> ret = new List<FileSystemRecordDto>();
                int index = 0;
                int totalCount = 0;
                do
                {
                    using (new AgentPerformanceScope("FSDisposal.QuerybyPage", addToStatistics: true))
                    {
                        var result = JobContext.Current.ApiClient.GetFSDueRecords(searchFilterParam);
                        if (result != null)
                        {
                            searchFilterParam.PageInfo.HasNextPage = !string.IsNullOrEmpty(result?.PageInfo?.PageIndex);
                            searchFilterParam.PageInfo.PageIndex = result?.PageInfo?.PageIndex;
                            int resultCount = result.Records != null ? result.Records.Count : 0;
                            totalCount += resultCount;
                            index++;
                            logger.Info($"query for {index} times, result count:{resultCount}, has next page:{searchFilterParam.PageInfo.HasNextPage}");
                            //SavePagingResult(result.Records);
                            ret.AddRange(result.Records);
                        }
                        else
                        {
                            logger.Warn($"Query result is null");
                            break;
                        }
                    }
                }
                while (searchFilterParam.PageInfo.HasNextPage);
                logger.Info("finish searching, total result count {0}", totalCount);
                return ret;
            }
        }

        private void ProcessFolder(FSFolderStub folder, AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto tree, bool parentChecked, Guid parentOrDefaultTerm)
        {
            logger.Info("Process folder {0}", folder.FullPath.LogBase64());
            bool currentTreeNodeCheck = ((tree != null && tree.CheckNumber == 1) || parentChecked);
            if (currentTreeNodeCheck)
            {
                List<FSFileStub> files = QueryFiles(folder);
                parentOrDefaultTerm = AnalyzeFileFromFolder(files, folder, parentOrDefaultTerm);
            }
            else
            {
                FileSystemRecordDto existFolder = FSJobCache.Instance.DisposalDifferentFolderCache.FirstOrDefault(a => a.NodeId == folder.SelfId);
                if(existFolder != null)
                {
                    logger.Info($"Custom setting, default term: {existFolder.TermId} on folder {folder.FullPath.LogBase64()},  Parent or default term:{parentOrDefaultTerm}");
                    parentOrDefaultTerm = existFolder.TermId;
                }
            }
            List<FSFolderStub> subFolders = QuerySubFolders(folder);
            foreach (FSFolderStub subFolder in subFolders)
            {
                if (tree != null && !(tree.Children == null || tree.Children.Count == 0))
                {
                    AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto child = tree.Children.FirstOrDefault(a => a.ID == subFolder.SelfId.ToString());
                    if(child != null)
                    {
                        ProcessFolder(subFolder, child, currentTreeNodeCheck, parentOrDefaultTerm);
                    }
                }
                else if(currentTreeNodeCheck)
                {
                    ProcessFolder(subFolder, null, currentTreeNodeCheck, parentOrDefaultTerm);
                }
                else
                {
                    logger.Info("Current node not check, and no children in the tree.");
                }
            }
        }

        private Guid AnalyzeFileFromFolder(List<FSFileStub> files,  FSFolderStub folder, Guid defaultTerm)
        {
            logger.Debug($"Start to analyze folder {folder.FullPath.LogBase64()}, default term: {defaultTerm}");
            Guid termId = defaultTerm;
            FileSystemRecordDto existFolder = FSJobCache.Instance.DisposalDifferentFolderCache.FirstOrDefault(a => a.NodeId == folder.SelfId);
            bool isHold = false;
            if (existFolder != null)
            {
                //如果当前Folder的Term与Default Value不同， 使用当前值
                termId = existFolder.TermId;
                logger.Info($"Custom setting, default term: {termId} on folder {folder.FullPath.LogBase64()},  Parent or default term:{defaultTerm}");
                if (existFolder.HoldStatus && existFolder.HoldReleaseTime > mTimePoint.Ticks)
                {
                    logger.Warn("Current folder is hold {0}", folder.FullPath.LogBase64());
                    //在Folder level判断是否HOld
                    isHold = true;
                }
            }
            if (termId == null || termId.Equals(Guid.Empty))
            {
                //如果Term Id是空 不处理 也不报错
                logger.Warn("Term is empty on folder {0}", folder.FullPath.LogBase64());
                return termId;
            }
            //获取Term Name用于jobdetail report
            string termName = FSJobCache.Instance.Terms.ContainsKey(termId) ? FSJobCache.Instance.Terms[termId].Name : null;

            SendDetail(folder, JobDetailsStatus.Successful);

            foreach (var fileStub in files)
            {
                using (var pc1 = new AgentPerformanceScope("FSContentDue.AnalyzeFileFromFolder", addToStatistics: true))
                {
                    logger.Info("Start to analyze file from folder, file id:{0}, folder {1}", fileStub?.SelfId, folder.SelfId);
                    var id = fileStub.SelfId; 
                    try
                    {
                        if (FSJobCache.Instance.TermRuleMapping.ContainsKey(termId))
                        {
                            var rules = FSJobCache.Instance.TermRuleMapping[termId];
                            var filteredRules = RuleUtil.FilterMoveRules(rules, Path.GetDirectoryName(fileStub.FullPath)).Where(x => x.FSRule != null).ToList();
                            if (filteredRules.Count > 0)
                            {
                                //appendmTimePoint
                                var newRules = appendTimePoint(filteredRules);
                                DisposalRuleEngine engine = new DisposalRuleEngine(newRules);
                                var hasOwnerRule = HasOwnerRule(newRules);
                                ObjectInfoBase filterObject = ObjectConverter.ConvertXObject2FilterObject(new XFileInfoEx(fileStub.MediaObj), FSJobCache.Instance.RootPath, hasOwnerRule);
                                var matchedRule = engine.MatchPotentialRule(filterObject);
                                if (matchedRule != null && matchedRule.Item1 != null && !string.IsNullOrWhiteSpace(matchedRule.Item1.Id))
                                {
                                    var rule = matchedRule.Item1;
                                    if (IsRemoveRule(rule))
                                    {
                                        if (isHold)
                                        {
                                            logger.Info("Skip hold record {0}", fileStub.FullPath.LogBase64());
                                            //add skip detail
                                            SendDetail(fileStub, JobDetailsStatus.Skipped, "RM_FS_ReportSkip_OnHold");
                                            continue;
                                        }
                                    }
                                    //add report 
                                    this.SendReport(fileStub, matchedRule.Item1, termId, termName);
                                    //add detail
                                    this.SendDetail(fileStub, JobDetailsStatus.Successful);
                                }
                                else
                                {
                                    logger.Info($"Current file not match rule.FSPath:{fileStub.FullPath.LogBase64()}.");
                                }
                            }
                            else
                            {
                                logger.Info($"Current Term[{termId}] doesn't have FS rule so skip check rule.FSPath:{fileStub.FullPath.LogBase64()}.");
                            }
                        }
                        else
                        {
                            logger.Warn("Cannot find term from term rule mapping cache. term id:{0}", termId);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Error occurred while disposal file:{fileStub.FullPath.LogBase64()} Error:{e.ToString()}");
                        JMFSAgentDueDisposalReportJobDetails detail = new JMFSAgentDueDisposalReportJobDetails()
                        {
                            TitleOrName = fileStub.MediaObj.Name,
                            Url = fileStub.FullPath,  
                            Status = JobDetailsStatus.Failed,
                            Comment = e.Message + $"[{ OSInformation.HostName}]",
                            Type = "RM_JS_Rule_ObjectLevel_FSFile",
                            //AgentName = OSInformation.HostName
                        };
                        JobDetailService.Commit(detail);
                        FSJobCache.Instance.FailedCount++;
                    }

                }
            }
            return termId;
        }
        private bool HasOwnerRule(List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> rules)
        {
            var hasOwnerRule = false;
            var fsRules = rules.Where(r => r.FSRule != null).Select(r => r.FSRule).ToList();
            if (!(fsRules == null || fsRules.Count==0))
            {
                hasOwnerRule = fsRules.Any(r => r.Filters.Any(f => f.Rule is AvePoint.GCommon.Contract.CommonFilter.OwnerRule));
            }
            return hasOwnerRule;
        }
        private List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> appendTimePoint(List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule> rules)
        {
            var result = new List<AvePoint.GCommon.Contract.StorageOptimization.Object.Rule>();
            foreach(var item in rules)
            {
                if (rulesWithTimePointDic.ContainsKey(item.Id)){
                    result.Add(rulesWithTimePointDic[item.Id]);
                }
                else
                {
                    this.ModifyTimeCriteria(item, mTimePoint);
                    rulesWithTimePointDic.Add(item.Id, item);
                    result.Add(item);
                }
            }
            return result;
        }

        private bool IsRemoveRule(AvePoint.GCommon.Contract.StorageOptimization.Object.Rule rule)
        {
            if (rule != null && rule.FSRule != null && (rule.FSRule.spMoveOption != null && rule.FSRule.spMoveOption.MoveSetting != null && rule.FSRule.spMoveOption.MoveDestination != null))
            {
                //move to
                return false;
            }
            return true;
        }

        private List<FSFolderStub> QuerySubFolders(Stub stub)
        {
            using (new AgentPerformanceScope("FSDiscover.QuerySubFolders", addToStatistics: true))
            {
                //List Dirs and add them to cache
                List<XDirectoryInfo> dirs = _system.ListDirectories(stub.MediaObj);
                List<FSFolderStub> dirStubs = new List<FSFolderStub>();
                foreach (XDirectoryInfo dir in dirs)
                {
                    string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dir.HighName, dir.LowName);
                    Guid id = fullPath.ToLowerInvariant().ToMd5();
                    Guid termSettingId = stub.ScopeSettingId; 
                    if (FSJobCache.Instance.ScopeSettingCache.ContainsKey(id))
                    {
                        if (!FSJobCache.Instance.ScopeSettingCache[id].IsActive)
                        {
                            logger.Debug("The folder node {0}  has been deactived.", fullPath.LogBase64());
                            continue;
                        }
                    } 
                    dirStubs.Add(new FSFolderStub
                    {
                        FullPath = fullPath,
                        MediaObj = dir,
                        ScopeSettingId = termSettingId,
                        SelfId = fullPath.ToLowerInvariant().ToMd5(),
                        ParentId = stub.SelfId
                    });
                } 
                logger.Info("Found {0} new folders", dirs.Count);
                ProgressService.IncreaseBase(dirStubs.Count);
                return dirStubs;
            }
        }
        private List<FSFileStub> QueryFiles(FSFolderStub stub)
        {
            using (new AgentPerformanceScope("FSDiscover.QueryFiles", addToStatistics: true))
            {
                //folder no longer exist
                using (var _system = ExternalUtil.OpenXSystem(stub.FullPath))
                {
                    List<XFileInfo> files = _system.ListFiles(new StorageInfo());
                    List<FSFileStub> fileStubs = new List<FSFileStub>();
                    if (files.Count > 0)
                    {
                        bool resetHighName = !FSJobCache.Instance.RootPath.Equals(stub.FullPath, StringComparison.OrdinalIgnoreCase);
                        string tempPath = stub.FullPath.Substring(FSJobCache.Instance.RootPath.Length, stub.FullPath.Length - FSJobCache.Instance.RootPath.Length).Trim('\\');
                        foreach (var t in files)
                        {
                            if (t.CreationTimeUtc < mTimePoint && FilterdIn(new XFileInfoEx(t)))
                            {
                                if (resetHighName)
                                {
                                    t.HighName = tempPath;
                                }
                                string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, t.HighName, t.LowName);
                                logger.Debug("Start to process file.id :{0}", fullPath.ToLowerInvariant().ToMd5());
                                FSFileStub fileStub = new FSFileStub()
                                {
                                    FullPath = fullPath,
                                    MediaObj = t,
                                    SelfId = fullPath.ToLowerInvariant().ToMd5(),
                                    ParentId = stub.SelfId,
                                    ScopeSettingId = FSJobCache.Instance.DispoalSettingScopeId
                                };
                                fileStubs.Add(fileStub);
                            }
                        }
                    }
                    return fileStubs;
                }
            }
        }

        private void SendDetail(FSFolderStub record, JobDetailsStatus status, string comments = "")
        {
            JMFSAgentDueDisposalReportJobDetails detail = new JMFSAgentDueDisposalReportJobDetails();
            detail.Type = "RM_JS_Rule_ObjectLevel_FSFolder";
            detail.TitleOrName = record.MediaObj.Name;
            detail.Url = record.FullPath;
            detail.Status = status;
            detail.Comment = comments;
            JobDetailService.Commit(detail);
        }
        private void SendDetail(FSFileStub record, JobDetailsStatus status, string comments = "")
        {
            JMFSAgentDueDisposalReportJobDetails detail = new JMFSAgentDueDisposalReportJobDetails();
            detail.Type = "RM_JS_Rule_ObjectLevel_FSFile";
            detail.TitleOrName = record.MediaObj.Name;
            detail.Url =record.FullPath;
            detail.Status = status;
            detail.Comment = comments;
            JobDetailService.Commit(detail);
        }

        private void SendReport(FSFileStub record, AvePoint.GCommon.Contract.StorageOptimization.Object.Rule rule, Guid termId, string termName)
        {
            XFileInfoEx xObj = new XFileInfoEx(record.MediaObj);
            DueDisposalReport report = new DueDisposalReport();
            report.TitleOrName = record.MediaObj.Name; ;
            report.Url = record.FullPath;
            report.BCSTermId = termId.ToString();
            report.BCSTermName = termName;
            report.ObjectLevel = (int)RMReportObjectLevel.FSFile;
            report.CreatedBy = xObj.Owner;
            report.CreatedTime = xObj.CreationTimeUtc.Ticks;
            //report.LastModifiedBy = record.ModifiedBy;
            report.LastModifiedTime = xObj.LastWriteTimeUtc.Ticks;  
            AddReportProperty(report, rule);
            ReportManager.Commit(report);
        }
        private BaseReport AddReportProperty(DueDisposalReport report, AvePoint.GCommon.Contract.StorageOptimization.Object.Rule rule)
        {
            report.AppliedRuleId = rule.Id;
            report.AppliedRuleName = rule.Name;
            report.ManualApproval = rule.FSRule.IsManualApproval ? RMDisposalManualApproval.Yes : RMDisposalManualApproval.No;
            //report.RelatedRecords = SerializerHelper.SerializeToXmlString(GetRelatedRecords(physicalObjectId));
            report.RelatedRecordsAction = (int)rule.FSRule.RelatedRecordOption;
            report.DisposalAction = (int)GetOperationType(rule.FSRule);
            report.ExportType = (RMExportTypeValue)AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None;
            report.DisposalClass = rule.DisposalClass;
            return report;
        }
        private string GetI18NStringForNodeType(int nodeType)
        {
            string strNodeType = "";
            switch (nodeType)
            {
                case (int)NodeLevel.FSFolder:
                    strNodeType = "RM_JS_Rule_ObjectLevel_FSFolder";
                    break;
                case (int)NodeLevel.FSFile:
                    strNodeType = "RM_JS_Rule_ObjectLevel_FSFile";
                    break;
                default:
                    break;
            }
            return strNodeType;
        }
        private RMContentDisposalAction GetOperationType(AvePoint.GCommon.Contract.StorageOptimization.Object.Rule rule)
        {
            if (rule == null)
            {
                return RMContentDisposalAction.None;
            }
            int keepDataOption = rule.KeepDataOption;
            if (keepDataOption == (int)AvePoint.RA.Contract.FileSystem.KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
            {
                return RMContentDisposalAction.Move;
            }
            else
            {
                var deleteOption = RMContentDisposalAction.Remove;
                if (rule.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
                {
                    deleteOption |= RMContentDisposalAction.RelatedRecords;
                }
                if (rule.IsDeleteParentBox)
                {
                    deleteOption |= RMContentDisposalAction.DeleteParentBox;
                }
                if ((keepDataOption & (int)KeepDataOption.LinkDocument) == (int)KeepDataOption.LinkDocument)
                {
                    deleteOption |= RMContentDisposalAction.LeaveStub;
                }

                //Just used for this logic.
                if (keepDataOption == (int)KeepDataOption.ArchiveBackupAndRemove)
                {
                    return RMContentDisposalAction.ArchiveToStorage;
                }
                return deleteOption;
            }
        }
        private bool FilterdIn(XFileInfoEx t)
        {
            if (t.Name.IndexOf(".stub.html", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }
            else
            {
                return true;
            } 
        }
        private void ModifyTimeCriteria(AvePoint.GCommon.Contract.StorageOptimization.Object.Rule rule, DateTime timePoint)
        {
            var soFilters = rule.FSRule?.Filters;
            if (soFilters != null)
            {
                logger.Info($"rule name: {rule.Name.LogBase64()}");
                foreach (var filter in soFilters)
                {
                    ModifyOlderThanCriteria(filter, timePoint);
                    filter.SequenceNo += 1;
                }
                //add created time criteria
                soFilters.Add(new AvePoint.GCommon.Contract.StorageOptimization.Object.SOFilterPolicy()
                {
                    Condition = AvePoint.GCommon.Contract.CommonFilter.PolicyCondition.Before,
                    Level = rule.FSRule.PolicyLevel,
                    Rule = new AvePoint.GCommon.Contract.CommonFilter.CreatedRule() { Value1 = "Created Time" },
                    RuleType = AvePoint.GCommon.Contract.CommonFilter.PolicyRuleType.CreatedTime,
                    Value = new AvePoint.GCommon.Contract.CommonFilter.PolicyValue(timePoint.ToString(APIDateTimeFormat.DATETYPEForAPI003)),
                    SequenceNo = 1
                });


                var tempStrs = rule.FSRule.AndOrExpression[AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.FileSysFile].Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                //var tempStrs = rule.FSRule.AndOrExpression[rule.FSRule.PolicyLevel].Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                string andOrExpression = "(1 And";
                foreach (var str in tempStrs)
                {
                    int sequenceNo = 0;
                    if (int.TryParse(str, out sequenceNo))
                    {
                        sequenceNo++;
                        andOrExpression = string.Format("{0} {1}", andOrExpression, sequenceNo.ToString());
                    }
                    else
                    {
                        andOrExpression = string.Format("{0} {1}", andOrExpression, str);
                    }
                }
                andOrExpression += ")";
                rule.FSRule.AndOrExpression = new Dictionary<AvePoint.GCommon.Contract.CommonFilter.PolicyLevel, string>()
                {
                    { rule.FSRule.PolicyLevel, andOrExpression }
                };
            }
        }
        private void ModifyOlderThanCriteria(AvePoint.GCommon.Contract.CommonFilter.FilterPolicy filter, DateTime timePoint)
        {
            if (filter.Rule is AvePoint.GCommon.Contract.CommonFilter.CreatedRule || filter.Rule is AvePoint.GCommon.Contract.CommonFilter.ModifiedRule
                    || filter.Rule is AvePoint.GCommon.Contract.CommonFilter.ColumnDateTimeRule || filter.Rule is AvePoint.GCommon.Contract.CommonFilter.StubLastAccessTimeRule)
            {
                switch (filter.Condition)
                {
                    case AvePoint.GCommon.Contract.CommonFilter.PolicyCondition.OlderThan:
                        int num;
                        DateTime tempDt = DateTime.UtcNow;
                        if (int.TryParse(filter.Value.Value1, out num))
                        {
                            if (filter.Value.Value1Unit == AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit.Days)
                            {
                                tempDt = timePoint.AddDays(-num);
                            }
                            else if (filter.Value.Value1Unit == AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit.Weeks)
                            {
                                tempDt = timePoint.AddDays(-num * 7);
                            }
                            else if (filter.Value.Value1Unit == AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit.Months)
                            {
                                tempDt = timePoint.AddMonths(-num);
                            }
                            else if (filter.Value.Value1Unit == AvePoint.GCommon.Contract.CommonFilter.PolicyValueUnit.Years)
                            {
                                tempDt = timePoint.AddYears(-num);
                            }
                            filter.Value.Value1 = tempDt.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                            filter.Condition = AvePoint.GCommon.Contract.CommonFilter.PolicyCondition.Before;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        private Guid QueryScopeTermIdSetting(AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto node)
        {
            Guid id = node.Level == NodeLevel.FSFolder ? node.FullPath.ToLowerInvariant().ToMd5() : new Guid(node.ID);
            //Guid id = node.FullPath.ToLowerInvariant().ToMd5();
            if (FSJobCache.Instance.ScopeSettingCache.ContainsKey(id))
            {
                return id;
            }
            else if (node.Parent != null)
            {
                return QueryScopeTermIdSetting(node.Parent);
            }
            else
            {
                return Guid.Empty;
            }
        }
    }
}
