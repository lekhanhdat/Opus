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
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.Records.Core.Utilities.Extensions;
using RACloudFS.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DAContract = AvePoint.GCommon.Contract.StorageOptimization.Object;
using KeepDataStatus = AvePoint.RA.Contract.FileSystem.KeepDataStatus;

namespace AvePoint.RA.RACloudFS.Report
{
    public class FSContentDueReportService
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(FSContentDueReportService));

        #region Interface
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

        private RA.DB.Explorer.Dao.IExplorerDao mExplorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (mExplorerDao == null)
                {
                    mExplorerDao = new ExplorerDao();
                }
                return mExplorerDao;
            }
        }

        private IExplorerService mExplorerService;
        protected IExplorerService ExplorerService
        {
            get
            {
                if (mExplorerService == null)
                {
                    mExplorerService = (IExplorerService)PlatformWindsorManager.GetService(typeof(IExplorerService));
                }
                return mExplorerService;
            }
        }

        private IRecordLoanAllianceDao mRecordLoanAllianceDao;
        public IRecordLoanAllianceDao RecordLoanAllianceDao
        {
            get
            {
                if (mRecordLoanAllianceDao == null)
                {
                    mRecordLoanAllianceDao = (IRecordLoanAllianceDao)PlatformWindsorManager.GetService(typeof(IRecordLoanAllianceDao));
                }
                return mRecordLoanAllianceDao;
            }
        }

        private IRMReportService mReportService;
        public IRMReportService ReportService
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

        private IFSConnectionDao mFSConnDao;
        public IFSConnectionDao FSConnDao
        {
            get
            {
                if (mFSConnDao == null)
                {
                    mFSConnDao = new FSConnectionDao();
                }
                return mFSConnDao;
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
        #endregion


        private DateTime mTimePoint;
        private DateTime mRunJobTime;
        private List<Guid> deactiveFoldId = new List<Guid>();
        private List<RMFileSystemSetting> deactiveSetting = new List<RMFileSystemSetting>();
        private List<FSTreeNodeDto> fsNodeDtoList;

        protected bool _jobHasException = false;
        protected bool _jobHasStopped = false;


        public FSReportManager FSReportManager { get; set; }

        private Dictionary<Guid, RMTerm> Terms { get; set; }
        /// <summary>
        /// key=rule id  value=rule
        /// </summary>
        private Dictionary<Guid, Rule> Rules { get; set; }
        /// <summary>
        /// key=termid  value=binded rules
        /// </summary>
        private Dictionary<Guid, List<Rule>> TermRuleMapping { get; set; }
        private RMProfileDto profile;
        private string jobId;
        public IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();


        public FSContentDueReportService(string jobId, string profileId)
        {
            try
            {
                this.jobId = jobId;
                ReportMangerFactory.Instance.Init(jobId, JobType.FSItemsFilesDueDisposal, true);
                profile = ReportService.GetProfileByIdForReportJob(profileId);
                mTimePoint = GetTimePoint(profile.Extension1);
                FSReportManager = new FSReportManager(profileId, JobType.FSItemsFilesDueDisposal);
                fsNodeDtoList = FSReportManager.AssembleAllTreeNodeForFSAsync().Result;
                GetDeactiveFoldId();
            }
            catch (Exception e)
            {
                mLog.Error($"Report ctor error: {e}");
            }
        }

        public async Task RunReportJobAsync()
        {
            try
            {
                await InitializeAsync();
                Process();
            }
            catch (Exception e)
            {
                ReportManager.SetJobFinished(JobStatus.Failed, e.Message);
                mLog.Error($"Run Report Job error:{e}");
            }
            finally
            {
                if (profile.ScheduleId != null)
                {
                    var jobIdReal = jobId?.Split('_')[0];
                    var job = JobMonitorDao.GetJobById(jobIdReal);
                    if (job.Status == (int)JobStatus.Finished || job.Status == (int)JobStatus.FinishWithException)
                    {
                        var exportModel = new ExportReportCommonModel
                        {
                            ReportJobType = ((int)profile.Type).ToString(),
                            ReportJobId = jobIdReal,
                            ProfileName = profile.ProfileName,
                            ProfileId = profile.Id.ToString(),
                        };
                        var reportParameters = SerializerHelper.SerializeByJsonConvert(exportModel);
                        ReportService.RunExportReportJob(reportParameters);

                    }

                }
            }
        }

        private DateTime GetTimePoint(string ext1)
        {
            var timePoint = ReportService.GetUtcTimePoint(ext1);
            return timePoint;
        }

        private void Process()
        {
            try
            {
                using (new CheckJobStopScope())
                {
                    if (fsNodeDtoList.Count == 0)
                    {
                        mLog.Warn("No tree nodes found.");
                        return;
                    }
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
                Guid id = new Guid(treeNode.ID);
                var folderId = IsFSConnection(id) ? treeNode.FullPath.ToLowerInvariant().ToMd5() : id;
                var folder = ExplorerDao.GetFSRecordById(folderId);
                if (folder != null)
                {
                    ProcessFolder(folder);
                }
                ProcessSubFolders(treeNode);
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
                SendDetail(record, JobDetailsStatus.Successful);
                ProcessFiles(record.Id);
            }
            catch (Exception ex)
            {
                mLog.Error($"Process folder has error:{ex}");
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
                && o.RecordStatus == (int)RMRecordStatus.Active
                && o.ParentId == folderId
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
                        mLog.Warn("The folder is deactive,id is{0}", folder.Id);
                    }
                }
            }
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

        private void SendDetail(Record record, JobDetailsStatus status, string comments = "")
        {
            JMReportJobDetails detail = new JMReportJobDetails();
            detail.Type = GetI18NStringForNodeType(record.NodeType);
            detail.TitleOrName = record.LeafName;
            detail.Url = GetFSNodeFullPath(record);
            detail.Status = status;
            detail.Comment = comments;
            ReportManager.SendJobDetail(detail);
        }

        private void SendReport(Record record, Rule rule, RuleAction disposalAction)
        {
            DueDisposalReport report = new DueDisposalReport();
            report.TitleOrName = record.LeafName;
            report.Url = GetFSNodeFullPath(record);
            report.BCSTermId = record.TermId.ToString();
            report.BCSTermName = Terms[record.TermId].Name;
            report.ObjectLevel = (int)RMReportObjectLevel.FSFile;
            report.CreatedBy = record.CreatedBy;
            report.CreatedTime = record.TimeCreated;
            report.LastModifiedBy = record.ModifiedBy;
            report.LastModifiedTime = record.TimeModified;
            AddReportProperty(report, rule, record.Id);
            ReportManager.SendJobReport(report);
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

        public ObjectInfoBase ConverDBRecord2FilterObj(Record record, string connectionPath = "")
        {
            var metaInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<RecordMetaInfo>(record.MetaInfo);
            //var metaInfo = SerializerHelper.DeserializeByDataContractSerializer<RecordMetaInfo>(record.MetaInfo);
            if (metaInfo.LastAccessTime == 0)
            {
                mLog.Warn($"{record.Id} LastAccessTime is 0, Need Recollect.");
            }
            if (string.IsNullOrEmpty(metaInfo.Owner))
            {
                mLog.Warn($"{record.Id} Owner is Empty, Need Recollect.");
            }
            FSFileInfo objectInfo = new FSFileInfo()
            {
                Name = Path.GetFileName(record.LeafName),
                Size = metaInfo.FileSize,
                Extension = Path.GetExtension(record.LeafName),
                AccessTime = new DateTime(metaInfo.LastAccessTime),
                Created = new DateTime(record.TimeCreated),
                Modified = new DateTime(record.TimeModified),
                Owner = metaInfo.Owner,
                FilePath = GetFSNodeFullPath(record),
            };
            return objectInfo;
        }

        private void ProcessFile(Record file)
        {
            ReportManager.Increase(1);
            var processFilePath = GetFSNodeFullPath(file);
            List<Rule> rules;
            mLog.Info($"Process File {file.Id}");
            RuleAction action = RuleAction.None;
            try
            {
                var fileTermId = file.TermId;
                if (TermRuleMapping.TryGetValue(fileTermId, out rules))
                {
                    DisposalRuleEngine engine = new DisposalRuleEngine(rules);
                    var fileFilterObj = ConverDBRecord2FilterObj(file);
                    Tuple<Rule, TimeSpan> matchedRule = null;
                    try
                    {
                        matchedRule = engine.MatchPotentialRule(fileFilterObj);
                    }
                    catch (Exception e)
                    {
                        mLog.Warn($"CheckCriteria Rule Exception: Save this rule and try again: {(e.Data.Contains("ruleName") ? e.Data["ruleName"] : "")}. Error: {e}");
                    }
                    if (matchedRule != null && matchedRule.Item1 != null)
                    {
                        mLog.Info($"{file.Id} fit rule :{matchedRule.Item1.Name} Action: {action.ToString()}");
                        bool onHold = ExplorerDao.IsRecordsHold(new List<Guid>() { file.Id, file.BoxId }, mTimePoint.Ticks) || RecordLoanAllianceDao.IsRecordsLoan(new List<Guid>() { file.Id, file.BoxId }, mTimePoint.Ticks);
                        if (onHold)
                        {
                            mLog.Info($"Current file is on hold. id:[{file.Id}]");
                            SendDetail(file, JobDetailsStatus.Skipped, "RM_FS_ReportSkip_OnHold");
                        }
                        else
                        {
                            SendDetail(file, JobDetailsStatus.Successful);
                            SendReport(file, matchedRule?.Item1, action);
                        }
                    }
                }
                else
                {
                    mLog.Info($"Process file skip {file.Id}");
                }
            }
            catch (Exception e)
            {
                mLog.Error($"Disposal file failed {processFilePath} : {e}");
                //throw;
            }
            finally
            {
                ReportManager.Increase(1);
            }

        }

     

        private async Task InitializeAsync()
        {
            ReportManager.StartUpdateJobProgress();
            mRunJobTime = DateTime.UtcNow;
            LoadRules();
            foreach (var rule in Rules.Values)
            {
                try
                {
                    ModifyTimeCriteria(rule, mTimePoint);
                }
                catch (Exception e)
                {
                    mLog.Warn($"[{rule.Name}] ModifyTimeCriteria error:{e}");
                }
            }
            LoadTerms();
            await AssembleTermRuleMappingAsync();
        }

        private void LoadTerms()
        {
            mLog.Info("Begin to load terms.");
            Terms = new TermDao().GetAllTermsForce().ToDictionary(t => t.UniqueId);
            mLog.Info("Loaded {0} terms.", Terms.Count);
        }

        private void LoadRules()
        {
            try
            {
                mLog.Info("Begin to Load rules.");
                Rules = RuleManagerService.GetFSRulesFromRecords().Where(r => r.FSRule != null && r.FSRule.SOFilters.Count != 0).ToDictionary(rule => new Guid(rule.Id));
                mLog.Info("End to load Rules");
            }
            catch (Exception e)
            {
                mLog.Error($"LoadRules Error: {e}");
                throw new Exception(I18NEntity.GetString("RM_JS_DocAve_CommunicationError"));
            }
        }

        private void ModifyTimeCriteria(Rule rule, DateTime timePoint)
        {
            var soFilters = rule.FSRule?.Filters;
            if (soFilters != null)
            {
                mLog.Info($"rule name: {rule.Name}");
                foreach (var filter in soFilters)
                {
                    ModifyOlderThanCriteria(filter, timePoint);
                    filter.SequenceNo += 1;
                }
                //add created time criteria
                soFilters.Add(new SOFilterPolicy()
                {
                    Condition = PolicyCondition.Before,
                    Level = rule.FSRule.PolicyLevel,
                    Rule = new CreatedRule() { Value1 = "Created Time" },
                    RuleType = PolicyRuleType.CreatedTime,
                    Value = new PolicyValue(timePoint.ToString(APIDateTimeFormat.DATETYPEForAPI003)),
                    SequenceNo = 1
                });

                mLog.Info($"Before convert and or express:{rule.FSRule.AndOrExpression[PolicyLevel.FileSysFile]}");
                var tempStrs = rule.FSRule.AndOrExpression[PolicyLevel.FileSysFile].Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                //var tempStrs = rule.FSRule.AndOrExpression[rule.FSRule.PolicyLevel].Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                string andOrExpression = "(1 And (";
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
                andOrExpression += "))";
                rule.FSRule.AndOrExpression = new Dictionary<PolicyLevel, string>()
                {
                    { rule.FSRule.PolicyLevel, andOrExpression }
                };
                mLog.Info($"After convert and or express:{rule.FSRule.AndOrExpression[PolicyLevel.FileSysFile]}");
            }
        }

        private async Task AssembleTermRuleMappingAsync()
        {
            mLog.Info("Begin to assemble term rules mappings.");
            TermRuleMapping = new Dictionary<Guid, List<Rule>>();
            Dictionary<int, Guid> termIdUniqueIdMapping = Terms.Values.ToDictionary(r => r.Id, r => r.UniqueId);
            ITermRuleAssociationDao termRuleAssociationDao = new TermRuleAssociationDao();
            Dictionary<int, List<RMTermRuleAssociation>> tempMapping = termRuleAssociationDao.GetTermWithRule()
                .GroupBy(t => t.TermId)
                .ToDictionary(t => t.Key, v => v.OrderBy(r => r.RuleOrder).ToList());
            Dictionary<int, List<Rule>> termRuleMapping = new Dictionary<int, List<Rule>>();

            tempMapping.ForEach(t =>
            {
                List<Rule> rules = new List<Rule>();
                t.Value.ForEach(association =>
                {
                    if (Rules.ContainsKey(association.RuleId))
                    {
                        rules.Add(Rules[association.RuleId]);
                    }
                });
                termRuleMapping[t.Key] = rules;
            });

            ITermSetMembershipDao membershipDao = new TermSetMembershipDao();
            Dictionary<int, List<int>> memberships = (await membershipDao.FindListWithColumnsAsync(c => new { c.TermId, c.ParentTermId }, e => !e.IsRemoved))
                .GroupBy(t => t.ParentTermId, v => v.TermId)
                .ToDictionary(t => t.Key, v => v.ToList());

            memberships.Keys.OrderBy(k => k).ForEach(pId =>
            {
                if (termRuleMapping.ContainsKey(pId))
                {
                    memberships[pId].ForEach(cId =>
                    {
                        if (!termRuleMapping.ContainsKey(cId))
                        {
                            termRuleMapping[cId] = termRuleMapping[pId];
                        }
                    });
                }
            });
            termRuleMapping.Keys.ForEach(termId =>
            {
                if (termIdUniqueIdMapping.ContainsKey(termId))
                {
                    Guid termGuid = termIdUniqueIdMapping[termId];
                    TermRuleMapping[termGuid] = termRuleMapping[termId];
                }
            });
        }

        private void ModifyOlderThanCriteria(FilterPolicy filter, DateTime timePoint)
        {
            if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                    || filter.Rule is ColumnDateTimeRule || filter.Rule is StubLastAccessTimeRule)
            {
                switch (filter.Condition)
                {
                    case PolicyCondition.OlderThan:
                        int num;
                        DateTime tempDt = DateTime.UtcNow;
                        if (int.TryParse(filter.Value.Value1, out num))
                        {
                            if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                            {
                                tempDt = timePoint.AddDays(-num);
                            }
                            else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                            {
                                tempDt = timePoint.AddDays(-num * 7);
                            }
                            else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                            {
                                tempDt = timePoint.AddMonths(-num);
                            }
                            else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                            {
                                tempDt = timePoint.AddYears(-num);
                            }
                            filter.Value.Value1 = tempDt.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                            filter.Condition = PolicyCondition.Before;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private List<ReportRelatedRecords> GetRelatedRecords(Guid id)
        {
            var currRecord = ExplorerDao.QueryAll(r => r.Id == id).FirstOrDefault();
            //if (string.IsNullOrEmpty(currRecord?.RelatedRecords))
            //{
            //    return result;
            //}
            List<ReportRelatedRecords> result = new List<ReportRelatedRecords>();
            if (!string.IsNullOrEmpty(currRecord?.RelatedRecords))
            {
                List<RMRelatedItemInfo> relatedRecordsInDB = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(currRecord.RelatedRecords);

                foreach (var relatedRecord in relatedRecordsInDB)
                {
                    //result.Add(new ReportRelatedRecords() { Name = relatedRecord.name, Url = relatedRecord.url });
                    if (relatedRecord.SourceFlag == (int)SourceFlag.FileSystem)
                    {
                        result.Add(new ReportRelatedRecords() { Name = relatedRecord.recId, Url = "" });
                    }
                    //else
                    //{
                    //    string itemFullUrl = string.Empty;
                    //    if (!relatedRecord.url.StartsWith(relatedRecord.SiteUrl))
                    //    {
                    //        itemFullUrl = AvePoint.RA.Common.Util.WebUtil.MakeFullUrl(relatedRecord.SiteUrl, relatedRecord.url);
                    //    }
                    //    else
                    //    {
                    //        itemFullUrl = relatedRecord.url;
                    //    }
                    //    result.Add(new ReportRelatedRecords() { Name = relatedRecord.name, Url = itemFullUrl });
                    //}
                }
            }
            return result;
        }

        private BaseReport AddReportProperty(DueDisposalReport report, Rule rule, Guid physicalObjectId)
        {
            report.AppliedRuleId = rule.Id;
            report.AppliedRuleName = rule.Name;
            report.ManualApproval = rule.FSRule.IsManualApproval ? RMDisposalManualApproval.Yes : RMDisposalManualApproval.No;
            report.RelatedRecords = SerializerHelper.SerializeToXmlString(GetRelatedRecords(physicalObjectId));
            report.RelatedRecordsAction = (int)rule.FSRule.RelatedRecordOption;
            report.DisposalAction = (int)GetOperationType(rule.FSRule);
            report.ExportType = (RMExportTypeValue)AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None;
            report.DisposalClass = rule.DisposalClass;
            return report;
        }

        private RMContentDisposalAction GetOperationType(Rule rule)
        {
            if (rule == null)
            {
                return RMContentDisposalAction.None;
            }
            int keepDataOption = rule.KeepDataOption;
            if (keepDataOption == (int)KeepDataStatus.Delete && rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
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
                if(keepDataOption == (int)KeepDataOption.Archive)
                {
                    return RMContentDisposalAction.ArchiveToStorage;
                }
                return deleteOption;
            }
        }
    }

    internal static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }
    }
}
