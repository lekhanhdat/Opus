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
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RAPhysical.API;
using AvePoint.RA.RAPhysical.Report.Interface;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.RMWeb.Physical.ColumnValues;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Contract;

namespace AvePoint.RA.RAPhysical.Report
{
    public class PRCreationAndDestroyedFileReportService : IPRCreationAndDestroyedFileReportService
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(PRCreationAndDestroyedFileReportService));
        public IExplorerService ExplorerService { get; set; }
        public IPRReportProcessor PRReportProcessor { get; set; }
        public ILocationManagementService LocationManagementService { get; set; }

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

        public ITermDao TermDao { get; set; }

        private List<RMTerm> Terms { get; set; }
        private bool SelectCreated;
        private bool SelectDestroyed;
        private DateTime startUtcTime;
        private DateTime endUtcTime;
        private Dictionary<Guid, RMRuleInfos> idRuleInfoDic = new Dictionary<Guid, RMRuleInfos>();
        private Dictionary<int, RMAccount> cacheAllUsers;

        private IRuleManagerService mRuleManagerService;
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
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        public async Task RunReportJobAsync(string jobId, string profileId)
        {

        }

        public async Task RunPRCreationAndDestroyedFileReportJobAsync(RMCreationJobMessage msg)
        {
            await InitParametersAsync(msg);
            var option = new ReportOptions()
            {
                JobId = msg.JobID,
                JobType = JobType.PhysicalCreateAndDestroyedFileReport,
                ProfileId = msg.ProfileId,
                IsUseBuiltInBoxAction = false,
                IsUseBuiltInFileAction = false,
                IsUseBuiltInRecordsGroupAction = false,
                BrowseOptions = new BrowseOptions
                {
                    NeedProcessRecord = true,
                    NeedProcessContainer = true
                }
            };
            PRReportProcessor
                .ConfigTreeAction(treeService =>
                {
                    treeService
                    .ConfigContainerAction(ProcessContainerAsync)
                    .ConfigBoxAction(ProcessBoxAsync)
                    .ConfigFileAction(ProcessFileAsync)
                    .ConfigRecordGroupAction(ProcessRecordAsync);
                    return Task.CompletedTask;
                });
            await PRReportProcessor.ProcessAsync(option);
        }

        private async Task InitParametersAsync(RMCreationJobMessage msg)
        {
            var globalTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(msg.GlobalTimeZoneId.Replace("_", " "));
            startUtcTime = TimeZoneInfo.ConvertTimeToUtc(msg.StartTime, globalTimeZone);
            endUtcTime = TimeZoneInfo.ConvertTimeToUtc(msg.EndTime.AddDays(1), globalTimeZone);//包含当天
            SelectCreated = msg.SelectCreated;
            SelectDestroyed = msg.SelectDestroyed;
            await InitRulesInfoAsync();
            LoadTerms();
            cacheAllUsers = AccountDao.FindAll().ToDictionary(k => k.Id, v => v);
        }
        private void LoadTerms()
        {
            mLog.Info("Begin to load terms.");
            Terms = new TermDao().GetAllTermsForce();
            mLog.Info("Loaded {0} terms.", Terms.Count);
        }
        public async Task ProcessContainerAsync(IPhysicalCustom container)
        {
            try
            {
                var createResult = false;
                var destoryResult = false;
                if (SelectCreated)
                {
                    createResult = ExplorerService.IsPhysicaRecordExistForCreateTime(container.Id, startUtcTime, endUtcTime);
                }
                if (SelectDestroyed)
                {
                    destoryResult = ExplorerService.IsPhysicaRecordExistForDestroyedTime(container.Id, startUtcTime, endUtcTime);
                }
                if (createResult)
                {
                    GenerateJobDetailItemForContainer(container, JobDetailsStatus.Successful);
                    GenerateReportItemForContainer(container, true, false);
                }
                if (destoryResult)
                {
                    GenerateJobDetailItemForContainer(container, JobDetailsStatus.Successful);
                    GenerateReportItemForContainer(container, false, true);
                }
            }
            catch (Exception e)
            {
                mLog.Error($"Process container has error:{e.ToString()}");
                GenerateJobDetailItemForContainer(container, JobDetailsStatus.Failed, e.Message);
            }
        }
        public async Task ProcessBoxAsync(IPhysicalBox box)
        {
            try
            {

                var createResult = false;
                var destoryResult = false;
                if (SelectCreated)
                {
                    createResult = ExplorerService.IsPhysicaRecordExistForCreateTime(box.Id, startUtcTime, endUtcTime);
                }
                if (SelectDestroyed)
                {
                    destoryResult = ExplorerService.IsPhysicaRecordExistForDestroyedTime(box.Id, startUtcTime, endUtcTime);
                }
                if (createResult)
                {
                    GenerateJobDetailItemForBox(box, JobDetailsStatus.Successful);
                    GenerateReportItemForBox(box, true, false);
                }
                if (destoryResult)
                {
                    GenerateJobDetailItemForBox(box, JobDetailsStatus.Successful);
                    GenerateReportItemForBox(box, false, true);
                }
            }
            catch (Exception e)
            {
                mLog.Error($"Process box has error:{e.ToString()}");
                GenerateJobDetailItemForBox(box, JobDetailsStatus.Failed, e.Message);
            }
        }
        public async Task ProcessFileAsync(IPhysicalFile file)
        {
            try
            {
                var createResult = false;
                var destoryResult = false;
                if (SelectCreated)
                {
                    createResult = ExplorerService.IsPhysicaRecordExistForCreateTime(file.Id, startUtcTime, endUtcTime);
                }
                if (SelectDestroyed)
                {
                    destoryResult = ExplorerService.IsPhysicaRecordExistForDestroyedTime(file.Id, startUtcTime, endUtcTime);
                }
                if (createResult)
                {
                    GenerateJobDetailItemForFolder(file, JobDetailsStatus.Successful);
                    GenerateReportItemForFolder(file, true, false);
                }
                if (destoryResult)
                {
                    GenerateJobDetailItemForFolder(file, JobDetailsStatus.Successful);
                    GenerateReportItemForFolder(file, false, true);
                }
            }
            catch (Exception e)
            {
                mLog.Error($"Process box has error:{e.ToString()}");
                GenerateJobDetailItemForFolder(file, JobDetailsStatus.Failed, e.Message);
            }
        }
        public async Task ProcessRecordAsync(IEnumerable<IPhysicalRecord> records)
        {
            foreach (IPhysicalRecord record in records)
            {
                try
                {
                    var creAteResult = false;
                    var destoryResult = false;
                    if (SelectCreated)
                    {
                        creAteResult = ExplorerService.IsPhysicaRecordExistForCreateTime(record.Id, startUtcTime, endUtcTime);
                    }
                    if (SelectDestroyed)
                    {
                        destoryResult = ExplorerService.IsPhysicaRecordExistForDestroyedTime(record.Id, startUtcTime, endUtcTime);
                    }
                    if (creAteResult)
                    {
                        GenerateJobDetailItemForRecord(record, JobDetailsStatus.Successful);
                        GenerateReportItemForRecord(record, true, false);
                    }
                    if (destoryResult)
                    {
                        GenerateJobDetailItemForRecord(record, JobDetailsStatus.Successful);
                        GenerateReportItemForRecord(record, false, true);
                    }
                }
                catch (Exception e)
                {
                    mLog.Error($"Process box has error:{e.ToString()}");
                    GenerateJobDetailItemForRecord(record, JobDetailsStatus.Failed, e.Message);
                }
            }
        }

        private void GenerateJobDetailItemForContainer(IPhysicalCustom container, JobDetailsStatus status, string comments = "")
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            detail.ObjectLevel = GetI18NStringForNodeType((int)RMNodeLevel.PhysicalCustom);
            detail.Title = container.Fields[DefaultColumnIDs.NameOrTitle];
            detail.URL = container.DirPath;
            detail.Status = status;
            detail.Comment = comments;
            PRReportProcessor.AddJobDetail(detail);
        }

        private void GenerateJobDetailItemForBox(IPhysicalBox box, JobDetailsStatus status, string comments = "")
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            detail.ObjectLevel = GetI18NStringForNodeType((int)RMNodeLevel.PhysicalBox);
            detail.Title = box.Fields[DefaultColumnIDs.NameOrTitle];
            detail.URL = box.DirPath;
            detail.Status = status;
            detail.Comment = comments;
            PRReportProcessor.AddJobDetail(detail);
        }
        private void GenerateJobDetailItemForFolder(IPhysicalFile file, JobDetailsStatus status, string comments = "")
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            detail.ObjectLevel = GetI18NStringForNodeType((int)RMNodeLevel.PhysicalFile);
            detail.Title = file.Fields[DefaultColumnIDs.NameOrTitle];
            detail.URL = file.DirPath;
            detail.Status = status;
            detail.Comment = comments;
            PRReportProcessor.AddJobDetail(detail);
        }
        private void GenerateJobDetailItemForRecord(IPhysicalRecord record, JobDetailsStatus status, string comments = "")
        {
            JMCreateAndDestroyedFileReportJobDetail detail = new JMCreateAndDestroyedFileReportJobDetail();
            detail.ObjectLevel = GetI18NStringForNodeType((int)RMNodeLevel.PhysicalRecord);
            detail.Title = record.Fields[DefaultColumnIDs.NameOrTitle];
            detail.URL = record.DirPath;
            detail.Status = status;
            detail.Comment = comments;
            PRReportProcessor.AddJobDetail(detail);
        }

        private string GetI18NStringForNodeType(int nodeType)
        {
            string strNodeType = "";
            switch (nodeType)
            {
                case (int)RMNodeType.PhyCustom:
                    strNodeType = "RM_PRM_PRE_TableItemType_Container";
                    break;
                case (int)RMNodeType.PhyBox:
                    strNodeType = "RM_PRM_PRE_TableItemType_Box";
                    break;
                case (int)RMNodeType.PhyFile:
                    strNodeType = "RM_PRM_PRE_TableItemType_File";
                    break;
                case (int)RMNodeType.PhyRecord:
                    strNodeType = "RM_PRM_PRE_TableItemType_Record";
                    break;
                default:
                    break;
            }
            return strNodeType;
        }

       /* private async Task<string> GetLoactionPathAsync(PhysicalBaseObject item)
        {
            var path = new StringBuilder();
            if (item.NodeType == (int)RMNodeLevel.PhysicalBox)
            {
                path.Append(await ExplorerService.GetPhysicalBoxPathByIdAsync(item.Id));
            }
            else if (item.NodeType == (int)RMNodeLevel.PhysicalFile)
            {
                if (item.BoxId == Guid.Empty)
                {
                    path.Append(LocationManagementService.GetLocationPathById(item.LocationId));
                }
                else
                {
                    path.Append(await ExplorerService.GetPhysicalBoxPathByIdAsync(item.BoxId));
                }
                path.Append($"/{item.Name}");
            }
            else if (item.NodeType == (int)RMNodeLevel.PhysicalRecord)
            {
                IPhysicalRecord record = (IPhysicalRecord)item;
                path.Append(record.DirPath);
            }
            return path.ToString();
        }*/

        private void GenerateReportItemForContainer(IPhysicalCustom container, bool Created, bool Destroyed)
        {
            var report = new CreateAndDestroyedFileReport();
            report.TermName = Terms.Where(t => t.UniqueId == container.TermId)?.FirstOrDefault()?.Name;
            report.Title = container.Fields[DefaultColumnIDs.NameOrTitle];
            report.LevelStr = (int)RMNodeLevel.PhysicalCustom;
            report.Url = container.DirPath;
            report.CreatedTime = container.CreateTimeTicks;
            report.LastModifiedTime = container.ModifiedTimeTicks;
            report.FileType = "RM_PRM_PRE_TableItemType_Container";
            if (Created)
            {
                report.OperationTime = container.CreateTimeTicks.Equals(DateTime.MinValue) ? string.Empty : container.CreateTimeTicks.ToString();
                report.OperationBy = container.CreateBy;
                report.Operation = (int)OperationType.Created;
            }
            if (Destroyed)
            {
                var data = ExplorerDao.GetPhysicalRecordById(container.Id);
                if(data?.ManualArchiveStatus == (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd)
                {
                    if (data.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WaitingApprove || data.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress)
                    {
                        report.InternalApprovedStatus = (int)SOApproveDBStatus.Cancelled;
                        report.ApprovalStatus = (int)SOApproveDBStatus.Cancelled;
                    }
                    else
                    {
                        report.ApprovalStatus = data?.ManualApprovedStatus ?? 0;
                        report.InternalApprovedStatus = data?.ManualInternalApprovedStatus ?? 0;
                    }
                }
                report.OperationTime = data.DestroyedTime.Equals(DateTime.MinValue) ? string.Empty : data.DestroyedTime.ToString();
                report.OperationBy = I18NEntity.GetString("RM_RC_TimeFrame_ArchiverByRASystem"); //data.ModifiedBy;
                report.Operation = (int)OperationType.Destroyed;
                //report.DisposalClass = GetRuleInfo(data.RuleId)?.DisposalClass;
                if (cacheAllUsers.TryGetValue(data.ManualApprovedBy, out RMAccount approveUser) && data.ManualApprovedStatus != (int)AvePoint.RA.Contract.SOApproveDBStatus.Rejected)
                {
                    report.ApprovedBy = approveUser.DisplayName;
                    report.ApprovedByUPN = approveUser.UserPrincipalName;
                }
                report.RecordsId = data.RecordsId;
                report.RuleName = GetRuleInfo(data.RuleId)?.RuleName;
            }
            PRReportProcessor.AddJobReport(report);
        }

        private void GenerateReportItemForBox(IPhysicalBox box, bool Created, bool Destroyed)
        {
            var report = new CreateAndDestroyedFileReport();
            report.TermName = Terms.Where(t => t.UniqueId == box.TermId)?.FirstOrDefault()?.Name;
            report.Title = box.Fields[DefaultColumnIDs.NameOrTitle];
            report.LevelStr = (int)RMNodeLevel.PhysicalBox;
            report.Url = box.DirPath;
            report.CreatedTime = box.CreateTimeTicks;
            report.LastModifiedTime = box.ModifiedTimeTicks;
            report.FileType = "RM_PRM_PRE_Filter_PhysicalBox";
            if (Created)
            {
                report.OperationTime = box.CreateTimeTicks.Equals(DateTime.MinValue) ? string.Empty : box.CreateTimeTicks.ToString();
                report.OperationBy = box.CreateBy;
                report.Operation = (int)OperationType.Created;
            }
            if (Destroyed)
            {
                var data = ExplorerDao.GetPhysicalRecordById(box.Id);
                if(data?.ManualArchiveStatus == (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd)
                {
                    if (data.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WaitingApprove || data.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress)
                    {
                        report.InternalApprovedStatus = (int)SOApproveDBStatus.Cancelled;
                        report.ApprovalStatus = (int)SOApproveDBStatus.Cancelled;
                    }
                    else
                    {
                        report.ApprovalStatus = data?.ManualApprovedStatus ?? 0;
                        report.InternalApprovedStatus = data?.ManualInternalApprovedStatus ?? 0;
                    }
                }
                report.OperationTime = data.DestroyedTime.Equals(DateTime.MinValue) ? string.Empty : data.DestroyedTime.ToString();
                report.OperationBy = I18NEntity.GetString("RM_RC_TimeFrame_ArchiverByRASystem"); //data.ModifiedBy;
                report.Operation = (int)OperationType.Destroyed;
                report.DisposalClass = GetRuleInfo(data.RuleId)?.DisposalClass;
                if (cacheAllUsers.TryGetValue(data.ManualApprovedBy, out RMAccount approveUser) && data.ManualApprovedStatus != (int)AvePoint.RA.Contract.SOApproveDBStatus.Rejected)
                {
                    report.ApprovedBy = approveUser.DisplayName;
                    report.ApprovedByUPN = approveUser.UserPrincipalName;
                }
                report.RecordsId = data.RecordsId;
                report.RuleName = GetRuleInfo(data.RuleId)?.RuleName;
            }
            PRReportProcessor.AddJobReport(report);
        }

        private void GenerateReportItemForFolder(IPhysicalFile file, bool Created, bool Destroyed)
        {
            var report = new CreateAndDestroyedFileReport();
            //foreach (RMTerm term in Terms)
            //{
            //    if (term.UniqueId == file.TermId)
            //    {
            //        report.TermName = term.Name;
            //        break;
            //    }
            //}
            report.TermName = Terms.Where(t => t.UniqueId == file.TermId)?.FirstOrDefault()?.Name;
            report.Title = file.Fields[DefaultColumnIDs.NameOrTitle];
            report.LevelStr = (int)RMNodeLevel.PhysicalFile;
            report.Url = file.DirPath;
            report.CreatedTime = file.CreateTimeTicks;
            report.LastModifiedTime = file.ModifiedTimeTicks;
            report.FileType = "RM_PRM_PRE_Filter_PhysicalFile";
            if (Created)
            {
                report.OperationTime = file.CreateTimeTicks.Equals(DateTime.MinValue) ? string.Empty : file.CreateTimeTicks.ToString();
                report.OperationBy = file.CreateBy;
                report.Operation = (int)OperationType.Created;
            }
            if (Destroyed)
            {
                var data = ExplorerDao.GetPhysicalRecordById(file.Id);
                if(data?.ManualArchiveStatus ==  (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd)
                {
                    if (data.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WaitingApprove || data.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress)
                    {
                        report.InternalApprovedStatus = (int)SOApproveDBStatus.Cancelled;
                        report.ApprovalStatus = (int)SOApproveDBStatus.Cancelled;
                    }
                    else
                    {
                        report.ApprovalStatus = data?.ManualApprovedStatus ?? 0;
                        report.InternalApprovedStatus = data?.ManualInternalApprovedStatus ?? 0;
                    }
                }
                report.OperationTime = data.DestroyedTime.Equals(DateTime.MinValue) ? string.Empty : data.DestroyedTime.ToString();
                report.OperationBy = I18NEntity.GetString("RM_RC_TimeFrame_ArchiverByRASystem"); //data.ModifiedBy;
                report.Operation = (int)OperationType.Destroyed;
                report.DisposalClass = GetRuleInfo(data.RuleId)?.DisposalClass;
                if (cacheAllUsers.TryGetValue(data.ManualApprovedBy, out RMAccount approveUser) && data.ManualApprovedStatus != (int)AvePoint.RA.Contract.SOApproveDBStatus.Rejected)
                {
                    report.ApprovedBy = approveUser.DisplayName;
                    report.ApprovedByUPN = approveUser.UserPrincipalName;
                }
                report.RecordsId = data.RecordsId;
                report.RuleName = GetRuleInfo(data.RuleId)?.RuleName;
            }
            PRReportProcessor.AddJobReport(report);
        }

        private void GenerateReportItemForRecord(IPhysicalRecord record, bool Created, bool Destroyed)
        {
            var report = new CreateAndDestroyedFileReport();
            //foreach (RMTerm term in Terms)
            //{
            //    if (term.UniqueId == record.TermId)
            //    {
            //        report.TermName = term.Name;
            //        break;
            //    }
            //}
            report.TermName = Terms.Where(t => t.UniqueId == record.TermId)?.FirstOrDefault()?.Name;
            report.Title = record.Fields[DefaultColumnIDs.NameOrTitle];
            report.LevelStr = (int)RMNodeLevel.PhysicalRecord;
            report.Url = record.DirPath;
            report.CreatedTime = record.CreateTimeTicks;
            report.LastModifiedTime = record.ModifiedTimeTicks;
            report.FileType = "RM_PRM_PRE_Filter_PhysicalRecord";
            if (Created)
            {
                report.OperationTime = record.CreateTimeTicks.Equals(DateTime.MinValue) ? string.Empty : record.CreateTimeTicks.ToString();
                report.OperationBy = record.CreateBy;
                report.Operation = (int)OperationType.Created;
            }
            if (Destroyed)
            {
                var data = ExplorerDao.GetPhysicalRecordById(record.Id);
                if(data?.ManualArchiveStatus ==  (int)AvePoint.RA.Contract.Schedule.ActionStatus.Archiverd)
                {
                    if (data.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WaitingApprove || data.ManualInternalApprovedStatus == (int)SOApproveDBStatus.WorkflowInProgress)
                    {
                        report.InternalApprovedStatus = (int)SOApproveDBStatus.Cancelled;
                        report.ApprovalStatus = (int)SOApproveDBStatus.Cancelled;
                    }
                    else
                    {
                        report.ApprovalStatus = data?.ManualApprovedStatus ?? 0;
                        report.InternalApprovedStatus = data?.ManualInternalApprovedStatus ?? 0;
                    }
                }
                report.OperationTime = data.DestroyedTime.Equals(DateTime.MinValue) ? string.Empty : data.DestroyedTime.ToString();
                report.OperationBy = I18NEntity.GetString("RM_RC_TimeFrame_ArchiverByRASystem"); //data.ModifiedBy;
                report.Operation = (int)OperationType.Destroyed;
                if (cacheAllUsers.TryGetValue(data.ManualApprovedBy, out RMAccount approveUser) && data.ManualApprovedStatus != (int)AvePoint.RA.Contract.SOApproveDBStatus.Rejected)
                {
                    report.ApprovedBy = approveUser.DisplayName;
                    report.ApprovedByUPN = approveUser.UserPrincipalName;
                }
                report.RecordsId = data.RecordsId;
                report.RuleName = GetRuleInfo(data.RuleId)?.RuleName;
            }
            PRReportProcessor.AddJobReport(report);
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

