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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Box.Model;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.Wrapper.Common;
using Newtonsoft.Json;
using RABox.Util;

namespace RABox.DataWorker
{
    public abstract class DataProcessor
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(DataProcessor));

        protected const int MaxDegreeOfParallelism = 10;

        protected string JobId = string.Empty;

        protected JobType JobType;

        protected NodeFlagType FlagType;

        protected readonly IRMBoxConnectionService BoxConnectionService = PlatformWindsorManager.GetService<IRMBoxConnectionService>();

        protected readonly RecordManager RecordManager;

        protected readonly ReportCenter ReportCenter;

        protected readonly SettingManager SettingManager;

        protected readonly TermManager TermManager;

        protected readonly RuleManager RuleManager;

        protected StopJobCancellationTokenSource Cts;

        protected static readonly string TenantId = TenantLocalValue.LogonGroupId;

        protected static readonly long JobStartTime = DateTime.UtcNow.Ticks;
        private static IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();
        public DataProcessor()
        {
            ReportCenter = new ReportCenter();
            RecordManager = new RecordManager();
            SettingManager = new SettingManager();
            TermManager = new TermManager();
            RuleManager = new RuleManager();
            Cts = new StopJobCancellationTokenSource();
        }

        public async Task ProcessAsync(string jobId)
        {
            BoxTreeNode? resetSettingNode = null;
            bool isSubjobFailed = false;
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    Cts.Config();
                    ReportCenter.Build(JobType, jobId, FlagType);
                    RecordManager.Build(ReportCenter, SourceFlag.Box);

                    JobId = jobId;
                    var jobContent = ReportCenter.GetJobContent(jobId);
                    var selectedNodes = JsonConvert.DeserializeObject<List<BoxTreeNode>>(jobContent);

                    //ValidateTreeNode
                    if (selectedNodes == null || !selectedNodes.Any())
                    {
                        _logger.Error($"Can not get the selected nodes list information.");
                        throw new ArgumentNullException("The selected nodes list's information is invalid.");
                    }
                    if (selectedNodes.Any(node => node.OwnerId.IsNullOrEmpty()))
                    {
                        _logger.Error($"The Owner Id of Node mustn't be empty.");
                        throw new ArgumentNullException("The Owner Id of Node mustn't be empty."); ;
                    }

                    //ValidateConnection
                    var defaultNode = selectedNodes.First();
                    if (defaultNode != null)
                    {
                        WrapperConfiguration.IsProcessApprovalDatasOnly = defaultNode.IsProcessApprovalDatasOnly;
                        WrapperConfiguration.IsRecheckRule = true;
                        if (WrapperConfiguration.IsProcessApprovalDatasOnly)
                        {
                            var isRecheckRule = FunctionSettingDao.GetSettingInfo(FunctionSettingType.IsRecheckRule).GetAwaiter().GetResult();
                            if (!string.IsNullOrEmpty(isRecheckRule))
                            {
                                bool result = Convert.ToBoolean(isRecheckRule);
                                WrapperConfiguration.IsRecheckRule = result;
                            }
                            else
                            {
                                WrapperConfiguration.IsRecheckRule = true;//the old setting need to check rule
                            }
                            _logger.Info($"current is recheck rule status is :{WrapperConfiguration.IsRecheckRule}");
                        }
                    }
                    BoxConnectionItem connectionInfo = await BoxConnectionService.GetByIdAsync(new Guid(defaultNode.ConnectionId));
                    if (connectionInfo == null)
                    {
                        _logger.Error($"Invaid connection {defaultNode.ConnectionId}.");
                        return;
                    }

                    var boxService = new RMBoxService(connectionInfo);

                    var nodeCount = selectedNodes.Count;
                    var currentNodeIndex = 0;

                    foreach (var topNode in selectedNodes)
                    {
                        var istopNodeFailed = false;
                        try
                        {
                            using (CheckJobStopScope stScope = new CheckJobStopScope())
                            {
                                _logger.Info($"Start processing the node: [{topNode.Id}], Level: [{topNode.Level.ToString()}].");
                                var scopeId = topNode.Id;
                                var folderId = BoxUtility.BoxRootFolderId;

                                if (topNode.Level == RMNodeLevel.BoxFolder)
                                {
                                    folderId = topNode.RealId;
                                }

                                await SettingManager.InitSettingAsync(topNode);

                                var topFolder = new BoxFolderProxy(boxService.GetUserContext(topNode.OwnerId), folderId);

                                var isLastNode = topNode == selectedNodes.Last();

                                await ProcessInnerAsync(topFolder, boxService, topNode, scopeId, isLastNode);

                                await SettingManager.ClearUserAndFolderSettings(topNode);
                                RecordManager.ClearCache();
                            }
                        }
                        catch (JobStopException)
                        {
                            _logger.Warn("the job has stopped.");
                            ReportCenter.JobHasStopped = true;
                            throw new JobStopException("the job has stopped.");
                        }
                        catch (Exception e)
                        {
                            istopNodeFailed = true;
                            if (e.Message.Equals("UserNeedToResetPassword"))
                            {
                                ReportCenter.RecordFailedCommon(boxService.GenJobDetail(JobType, topNode, topNode.Level, I18NResource.NeedResetPassword), (int)topNode.Level);
                            }
                            else if (e.Message.Equals("UserNeedToCompleteEmailConfirmation"))
                            {
                                ReportCenter.RecordFailedCommon(boxService.GenJobDetail(JobType, topNode, topNode.Level, I18NResource.NeedCompleteEmailConfirmation), (int)topNode.Level);
                            }
                            else
                            {
                                ReportCenter.RecordFailedCommon(boxService.GenJobDetail(JobType, topNode, topNode.Level, I18NResource.UnexpectedException), (int)topNode.Level);
                            }
                        }
                        finally
                        {
                            if (ReportCenter.GetMainJobState() == JobStatus.Stopping)
                            {
                                _logger.Warn("the main job is stopping, need to stop the job.");
                                throw new JobStopException("the job has stopped.");
                            }

                            if (!ReportCenter.JobHasStopped && !istopNodeFailed)
                            {
                                int nextProgress = (100 * ++currentNodeIndex) / nodeCount;
                                var currentProgress = ReportCenter.GetProgress(jobId);
                                if (currentProgress < nextProgress)
                                {
                                    _logger.Info($"Update progress from {currentProgress} to {nextProgress} for: [{topNode.Id}].");
                                    ReportCenter.SetProgress(jobId, nextProgress);
                                }
                            }
                        }
                    }

                    if (JobType == JobType.BoxDataSynchronisation || JobType == JobType.BoxDataSynchronisationSchedule)
                    {
                        resetSettingNode = defaultNode;
                    }
                }
            }
            catch (JobStopException)
            {
                _logger.Warn("the job has stopped.");
                ReportCenter.JobHasStopped = true;
                Cts.Cancel();
                throw new JobStopException("the job has stopped.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while process job. Error: {e}");
                isSubjobFailed = true;
                ReportCenter.SetJobFinish(JobStatus.Failed, e.Message);
            }
            finally
            {
                PerformanceMonitor.WritePerformanceResult();

                if (JobType == JobType.BoxRecordsDisposal)
                {
                    ReportCenter.CommitDisposalAnalysis();
                }

                if (!isSubjobFailed)
                {
                    ReportCenter.Completed();
                }

                if (JobType != JobType.BoxRecordsDisposal)
                {
                    RecordManager.WaitComplete();
                }

                if (resetSettingNode != null)
                {
                    _logger.Info($"Job finish. Start processing reset setting for [{resetSettingNode.ContainerId}-{resetSettingNode.ConnectionId}].");
                    await SettingManager.ResetSettingInfoAsync(resetSettingNode, jobId);
                }

                Cts.Dispose();
            }
        }

        public abstract Task ProcessInnerAsync(BoxFolderProxy topFolder, RMBoxService boxService, BoxTreeNode topNode, string scopeId, bool isLastNode = false);

    }
}
