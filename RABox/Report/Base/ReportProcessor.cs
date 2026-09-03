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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Box.Model;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using RABox.Converters;
using SerializerHelper = AvePoint.RA.Common.Global.Utils.SerializerHelper;

namespace RABox.Report.Base
{
    public abstract class ReportProcessor
    {
        private static RALogger _logger = RALogger.GetInstance(typeof(ReportProcessor));
        protected readonly IRMBoxConnectionService BoxConnectionService = PlatformWindsorManager.GetService<IRMBoxConnectionService>();
        private IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private IRMReportService ReportService => PlatformWindsorManager.GetService<IRMReportService>();

        protected readonly RuleManager RuleManager;

        protected readonly TermManager TermManager;

        protected readonly ReportCenter ReportCenter;

        protected readonly RecordManager RecordManager;

        protected string JobId = string.Empty;

        protected JobType JobType;

        protected RMProfileDto ProfileDto;

        public ReportProcessor(string profileId)
        {
            ReportCenter = new ReportCenter();
            RecordManager = new RecordManager();
            TermManager = new TermManager();
            RuleManager = new RuleManager();
            ProfileDto = ReportCenter.GetReportProfile(profileId);
        }

        public async Task Process()
        {
            List<BoxTreeNodeDto> boxTreeNodes = InitializeReportProcess();
            try
            {
                Initialize();
                await ProcessInner(boxTreeNodes);
                StartScheduledExport();
            }
            catch (Exception e)
            {
                ReportCenter.SetJobFinish(JobStatus.Failed, e.Message);
                _logger.Error($"Run Report Job error:{e}");
            }
        }

        public List<BoxTreeNodeDto> InitializeReportProcess()
        {
            try
            {
                ReportCenter.Build(JobType, JobId);
                RecordManager.Build(ReportCenter, SourceFlag.Box);
                var boxTreeNode = new BoxTreeNode();

                if (JobType == JobType.BoxBCSTermUsageReport)
                {
                    boxTreeNode = SerializerHelper.DeserializeByDataContractSerializer<BoxTreeNode>(ProfileDto.Extension2);
                }
                else
                {
                    boxTreeNode = SerializerHelper.DeserializeByJsonSerializer<BoxTreeNode>(RuleSPTreeUtil.BuildBoxTreeJsonStr(ProfileDto.Extension2));
                }

                return BoxTreeScopeUtil.AssembleAllTreeNodeForBoxAsync(boxTreeNode).Result;

            }
            catch (Exception e)
            {
                _logger.Error($"Report ctor error: {e}");
                throw;
            }
        }

        protected abstract void Initialize();

        private void StartScheduledExport()
        {
            if (JobType != JobType.BoxItemsFilesDueDisposalReport || ProfileDto?.ScheduleId == null)
            {
                return;
            }

            var jobIdReal = JobId?.Split('_')[0];
            var job = JobMonitorDao.GetJobById(jobIdReal);
            if (job?.Status != (int)JobStatus.Finished && job?.Status != (int)JobStatus.FinishWithException)
            {
                return;
            }

            var exportModel = new ExportReportCommonModel
            {
                ReportJobType = ((int)ProfileDto.Type).ToString(),
                ReportJobId = jobIdReal,
                ProfileName = ProfileDto.ProfileName,
                ProfileId = ProfileDto.Id.ToString(),
            };
            var reportParameters = SerializerHelper.SerializeByJsonConvert(exportModel);
            ReportService.RunExportReportJob(reportParameters);
            _logger.Info("Started scheduled Box due-disposal report export. JobId:{0}, ProfileId:{1}", jobIdReal, ProfileDto.Id);
        }

        protected async Task ProcessInner(List<BoxTreeNodeDto> boxTreeNodes)
        {
            try
            {
                using (new CheckJobStopScope())
                {
                    if (boxTreeNodes.Count == 0)
                    {
                        _logger.Warn("No tree nodes found.");
                        ReportCenter.SetJobFinish(JobStatus.Finished);
                        return;
                    }
                    _logger.Info($"Start processing [{boxTreeNodes.Count}] node(s) to generate report.");
                    await ProcessSelectedNodeAsync(boxTreeNodes);
                }
                ReportCenter.Completed();
            }
            catch (JobStopException)
            {
                _logger.Warn("This Job is stopped.");
                ReportCenter.SetJobFinish(JobStatus.Stopped);
            }
            catch (Exception e)
            {
                _logger.Error("An error occurred while running. ", e.ToString());
                throw;
            }
        }

        protected async Task ProcessSelectedNodeAsync(List<BoxTreeNodeDto> treeNodes)
        {
            foreach (var treeNode in treeNodes)
            {
                _logger.Info($"Start processing node [{treeNode.ID}-{treeNode.Name}].");
                RecordManager.Config(treeNode.OwnerId);
                using CheckJobStopScope subJScope = new CheckJobStopScope();
                var folder = RecordManager.GetBoxRecordById(new Guid(treeNode.ID));
                if (folder != null)
                {
                    _logger.Info($"Process folder node [{treeNode.ID}-{treeNode.Name}].");
                    ProcessFolder(folder);
                }
                //Process root file
                if (treeNode.Level == NodeLevel.BoxUser)
                {
                    BoxConnectionItem connectionInfo = await BoxConnectionService.GetByIdAsync(new Guid(treeNode.ConnectionId));
                    if (connectionInfo == null)
                    {
                        _logger.Error($"Invalid connection {treeNode.ConnectionId}.");
                    }
                    else
                    {
                        try
                        {
                            _logger.Info($"Process files under root folder of user [{treeNode.ID}-{treeNode.Name}].");
                            var boxService = new RMBoxService(connectionInfo);
                            var rootFolder = new BoxFolderProxy(boxService.GetUserContext(treeNode.OwnerId), BoxUtility.BoxRootFolderId);
                            ProcessFiles(rootFolder.UniqueId);
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Generate root file occur error: {e}");
                        }
                    }
                }
                _logger.Info($"Process all sub folders under {treeNode.Level.ToString()} node [{treeNode.ID}-{treeNode.Name}].");
                ProcessSubFolders(treeNode);
            }
        }

        protected void ProcessSubFolders(BoxTreeNodeDto treeNode)
        {
            bool hasNext = true;
            string pageIndex = string.Empty;
            while (hasNext)
            {
                using CheckJobStopScope subJScope = new CheckJobStopScope();
                Tuple<IEnumerable<Record>, string> result = RecordManager.QueryFolderRecordsByAncestor(new Guid(treeNode.ID), pageIndex);
                hasNext = !string.IsNullOrEmpty(result.Item2);
                pageIndex = result.Item2;
                List<Record> datas = result.Item1.ToList();
                foreach (var folder in datas)
                {
                    ProcessFolder(folder);
                }
            }
        }

        protected virtual void ProcessFolder(Record record)
        {
            try
            {
                ReportCenter.RecordSuccessful(record.GenerateReportJobDetail(), record.NodeType);
                ProcessFiles(record.Id);
            }
            catch (Exception ex)
            {
                _logger.Error($"Process folder has error:{ex}");
                ReportCenter.RecordFailed(record.GenerateReportJobDetail(ex.Message), record.NodeType);
            }
        }

        protected abstract void ProcessFiles(Guid folderId);

        protected abstract void ProcessFile(Record record);
    }
}
