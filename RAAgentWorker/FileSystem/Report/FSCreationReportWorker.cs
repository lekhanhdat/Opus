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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.Media.Storage;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Global.Object;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Report
{
    public class FSCreationReportWorker: IScheduleJobWorker
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IProgressService ProgressService;
        private IReportService<JMJobDetails> JobDetailService;
        private IReportService<BaseReport> ReportManager;

        private DateTime startUtcTime;
        private DateTime endUtcTime;
        private IXSystem _system; 
        internal static FSFolderStub _rootStub;

        List<AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto> connections;
        internal static AvePoint.RA.Contract.FileSystem.FSSettingDto currentSetting;
        private Guid DefaultTermId;
        public void Bind(string msgStr)
        {
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            ReportManager = JobContext.Current.ReportManager.Create();

            FSJobMessage msg = SerializerHelper.DeserializeByDataContractSerializer<FSJobMessage>(msgStr);
            JobContext.Current.JobMessage = msgStr;
            startUtcTime = msg.StartTime;
            endUtcTime = msg.EndTime;

            List<AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto> node = msg.FSTreeNodes.Select(a=>DtoConverter.ConvertGlobalDto2FSTreeNodeDto(a)).ToList();  
            connections = node;

            logger.Debug("Connections count {0}", node.Count);
        }


        public void Run()
        {
            try
            {
                foreach (var node in connections)
                {
                    logger.Debug(node.ToString().LogBase64());
                    System.Tuple<AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto, AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto, AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto> top3Nodes = ExternalUtil.FindTop3LevelNodes(node);
                    string path = top3Nodes.Item3.FullPath;
                    logger.Debug("The root location is {0}", top3Nodes?.Item3?.ID);
                    FSJobCache.Instance.RootPath = path.TrimEnd('\\');
                    FSJobCache.Instance.RunJobScopePath = node.FullPath;
                    _system = ExternalUtil.OpenXSystem(FSJobCache.Instance.RootPath);

                    string highName = node.FullPath.Substring(path.Length).Trim('\\');
                    StorageInfo dirInfo = new StorageInfo() { HighName = highName };

                    if (_system.DirectoryExists(dirInfo))
                    {
                        XDirectoryInfo dir = _system.OpenDirectory(dirInfo, FileMode.Open);
                        Guid parentId = string.IsNullOrEmpty(dirInfo.HighName) ?
                            new Guid(top3Nodes.Item2.ID)  //level3  connection
                            : ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, Path.GetDirectoryName(ExternalUtil.CombinePath(dir.HighName, dir.LowName))).ToLowerInvariant().ToMd5();  //sub folder
                        string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dir.HighName, dir.LowName);
                        FSFolderStub rootStub = new FSFolderStub() { MediaObj = dir, FullPath = fullPath, SelfId = fullPath.ToLowerInvariant().ToMd5(), ParentId = parentId };
                        _rootStub = rootStub;

                        Guid defaultTermId = Guid.Empty;
                        AvePoint.RA.Contract.FileSystem.FSSettingDto settingOnConnection = null;
                        Guid settingScopeId = QueryScopeTermIdSetting(node);
                        FSJobCache.Instance.DispoalSettingScopeId = settingScopeId;
                        var setting = FSJobCache.Instance.ScopeSettingCache[settingScopeId];  //跑Job的节点的Setting
                        currentSetting = setting;
                        DefaultTermId = setting.DefaultTermId;
                        if (settingScopeId != Guid.Empty && FSJobCache.Instance.ScopeSettingCache.ContainsKey(settingScopeId))
                        {
                            settingOnConnection = FSJobCache.Instance.ScopeSettingCache[settingScopeId];
                        }  //跑Job的节点的Setting
                        if (settingOnConnection != null)
                        {
                            defaultTermId = settingOnConnection.DefaultTermId;
                            if (!settingOnConnection.IsActive)
                            {
                                logger.Warn("This select node is deactive,node name is {0}", node.Name.LogBase64());
                                continue;
                            } 
                        }
                        var allExceptFolderCache = this.GetAllFolders();
                        //缓存与default term不一样的数据，  或Hold的数据。
                        FSJobCache.Instance.DisposalDifferentFolderCache.AddRange(allExceptFolderCache.AsEnumerable());
                        bool parentChecked = node.CheckNumber == 1; 
                        ProcessFolder(_rootStub, node, false, defaultTermId); 
                        ProgressService.IncreaseBase(1);
                    }
                    else
                    {
                        JobDetailService.Commit(new JMFSAgentCreateFileReportJobDetail()
                        {
                            Title = node.Title,
                            URL = node.FullPath,
                            Status = JobDetailsStatus.Failed,
                            Comment = "RM_JS_JMD_FS_PathCanNotAccess",
                            ObjectLevel = "RM_JS_Rule_ObjectLevel_FSFile",
                            //AgentName = OSInformation.HostName
                        }); 
                        FSJobCache.Instance.FailedCount++;
                        //throw new FileNotFoundException("We cannot open the Dir" + node.FullPath);
                    } 
                }
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

        private void ProcessFolder(FSFolderStub folder, AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto tree, bool parentChecked, Guid defaultTermId)
        {
            logger.Info("Process folder {0}", folder.FullPath.LogBase64());
            var settingScopeId = folder.FullPath.ToLowerInvariant().ToMd5();
            if (FSJobCache.Instance.ScopeSettingCache.ContainsKey(settingScopeId))
            {
                var settingOnConnection = FSJobCache.Instance.ScopeSettingCache[settingScopeId];
                if (!settingOnConnection.IsActive)
                {
                    logger.Warn("This select node is deactive,node name is {0}", folder.MediaObj.Name.LogBase64());
                    return;
                }
            }
            if (tree == null || tree.Level == NodeLevel.FSFolder)
            {
                //Guid settingScopeId = QueryScopeTermIdSetting(tree);
                if (settingScopeId != Guid.Empty && FSJobCache.Instance.ScopeSettingCache.ContainsKey(settingScopeId))
                {
                    var setting = FSJobCache.Instance.ScopeSettingCache[settingScopeId];
                    defaultTermId = setting.DefaultTermId;
                }
            }
            bool currentTreeNodeCheck = ((tree != null && tree.CheckNumber == 1) || parentChecked);
            if (currentTreeNodeCheck)
            {
                List<FSFileStub> files = QueryFiles(folder);
                defaultTermId = AnalyzeFileFromFolder(files, folder, defaultTermId);
            }
            else
            {
                FileSystemRecordDto existFolder = FSJobCache.Instance.DisposalDifferentFolderCache.FirstOrDefault(a => a.NodeId == folder.SelfId);
                if (existFolder != null)
                {
                    logger.Info($"Custom setting, default term: {existFolder.TermId} on folder {folder.FullPath.LogBase64()},  Parent or default term:{defaultTermId}");
                    defaultTermId = existFolder.TermId;
                }
            }
            List<FSFolderStub> subFolders = QuerySubFolders(folder);
            foreach (FSFolderStub subFolder in subFolders)
            {
                if (tree != null && !(tree.Children==null || tree.Children.Count == 0))
                {
                    AvePoint.GCommon.Contract.Tree.Object.FSTreeNodeDto child = tree.Children.FirstOrDefault(a => a.ID == subFolder.SelfId.ToString());
                    if (child != null)
                    {
                        ProcessFolder(subFolder, child, currentTreeNodeCheck, defaultTermId);
                    }
                }
                else
                {
                    ProcessFolder(subFolder, null, currentTreeNodeCheck, defaultTermId);
                }
            }
        }

        private Guid AnalyzeFileFromFolder(List<FSFileStub> files, FSFolderStub folder, Guid defaultTerm)
        {
            logger.Debug($"Start to analyze folder {folder.FullPath.LogBase64()}, default term: {defaultTerm}");
            XDirectoryInfoEx xObj_folder = new XDirectoryInfoEx(folder.MediaObj);
            bool addFolder = xObj_folder.CreationTimeUtc > startUtcTime && xObj_folder.CreationTimeUtc < endUtcTime;
            Guid termId = defaultTerm;
            FileSystemRecordDto existFolder = FSJobCache.Instance.DisposalDifferentFolderCache.FirstOrDefault(a => a.NodeId == folder.SelfId);
            if (existFolder != null)
            {
                //如果当前Folder的Term与Default Value不同， 使用当前值
                termId = existFolder.TermId;
                logger.Info($"Custom setting, default term: {termId} on folder {folder.FullPath.LogBase64()},  Parent or default term:{defaultTerm}");
            }
            if (termId == null || termId.Equals(Guid.Empty))
            {
                //如果Term Id是空 不处理 也不报错
                logger.Warn("Term is empty on folder {0}", folder.FullPath.LogBase64());
                return termId;
            }
            //获取Term Name用于jobdetail report
            string termName = FSJobCache.Instance.Terms.ContainsKey(termId) ? FSJobCache.Instance.Terms[termId].Name : null;
            logger.Debug($"Start to analyze folder {folder.FullPath.LogBase64()}, term name : {termName.LogBase64()}");
            if (addFolder)
            {
                GenerateJobDetailItem(folder, JobDetailsStatus.Successful);
                GenerateReportItem(folder, termName);
            }
            foreach (var fileStub in files)
            {
                logger.Debug("Start to analyze file from folder, file id:{0}, folder {1}", fileStub?.SelfId, folder.SelfId);
                try
                {

                    XFileInfoEx xObj = new XFileInfoEx(fileStub.MediaObj);
                    var createdTimeUtc = xObj.CreationTimeUtc;
                    if (createdTimeUtc > startUtcTime && createdTimeUtc < endUtcTime)
                    {
                        GenerateJobDetailItem(fileStub, JobDetailsStatus.Successful);
                        GenerateReportItem(fileStub, termName);
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Error occurred while process file:{fileStub.FullPath.LogBase64()} Error:{e.ToString()}");

                    GenerateJobDetailItem(fileStub, JobDetailsStatus.Successful, e.Message + $"[{OSInformation.HostName}]");

                    FSJobCache.Instance.FailedCount++;
                }
            }
            return termId;
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
                            logger.Info("The folder node {0}  has been deactived.", fullPath.LogBase64());
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
                            if (FilterdIn(new XFileInfoEx(t)))
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

        private string GetI18NStringForNodeType(Stub fsNode)
        {
            if(fsNode is FSFileStub)
            {
                return "RM_JS_Rule_ObjectLevel_FSFile";
            }
            else
            {
                return "RM_JS_Rule_ObjectLevel_FSFolder";
            }
        }
        private void GenerateJobDetailItem(Stub fsNode, JobDetailsStatus status, string comments = "")
        {
            JMFSAgentCreateFileReportJobDetail detail = new JMFSAgentCreateFileReportJobDetail
             {
                ObjectLevel = GetI18NStringForNodeType(fsNode),
                Title = fsNode.MediaObj.Name,
                URL = fsNode.FullPath,
                Status = status,
                Comment = comments
            };
            JobDetailService.Commit(detail);
        }

        private void GenerateReportItem(FSFileStub fsNode, string termName)
        {
            var report = new CreateAndDestroyedFileReport(); 
            report.Title = fsNode.MediaObj.Name;
            report.LevelStr = (int)NodeLevel.FSFile;
            report.Url = fsNode.FullPath;
            report.TermName = termName;

            XFileInfoEx xObj = new XFileInfoEx(fsNode.MediaObj);
            report.CreatedTime = xObj.CreationTimeUtc.Ticks;
            report.LastModifiedTime = xObj.LastWriteTimeUtc.Ticks;
            report.FileType = Alphaleonis.Win32.Filesystem.Path.GetExtension(xObj.FileFullPath).TrimStart(new char[] { '.' });
            report.OperationTime = xObj.CreationTimeUtc.Equals(DateTime.MinValue) ? string.Empty : xObj.CreationTimeUtc.Ticks.ToString();
            report.OperationBy = xObj.Owner;
            report.Operation = 0;  // (int)OperationType.Created; 
            ReportManager.Commit(report);
        }
        private void GenerateReportItem(FSFolderStub fsNode, string termName)
        {
            var report = new CreateAndDestroyedFileReport();
            report.Title = fsNode.MediaObj.Name;
            report.LevelStr = (int)NodeLevel.FSFolder;
            report.Url = fsNode.FullPath;
            report.TermName = termName;

            XDirectoryInfoEx xObj = new XDirectoryInfoEx(fsNode.MediaObj);
            report.CreatedTime = xObj.CreationTimeUtc.Ticks;
            report.LastModifiedTime = xObj.LastWriteTimeUtc.Ticks;
            report.FileType = "RM_Common_ObjectLevel_Folder";
            report.OperationTime = xObj.CreationTimeUtc.Equals(DateTime.MinValue) ? string.Empty : xObj.CreationTimeUtc.Ticks.ToString();
            report.OperationBy = xObj.Owner;
            report.Operation = 0;  // (int)OperationType.Created; 
            ReportManager.Commit(report);
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

    }
}
