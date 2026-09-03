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
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using RAGoogle.Discover.Impl;
using RAGoogle.Extension;
using RAGoogle.GoogleObjDiscover.Impl;
using RAGoogle.Helper;
using RAGoogle.Models;
using RAGoogle.Util;
using Util;
using SerializerHelper = AvePoint.RA.Common.Global.Utils.SerializerHelper;

namespace RAGoogle.Report
{
    public abstract class BaseReportProcessor
    {
        #region properties

        protected static readonly IRALogger logger = RALogger.GetInstance(typeof(BaseReportProcessor));
        protected IRMRemoteGoogleNodeService RemoteGoogleNodeService => PlatformWindsorManager.GetService<IRMRemoteGoogleNodeService>();
        protected IRMReportService ReportService => PlatformWindsorManager.GetService<IRMReportService>();
        protected IExplorerDao ExplorerDao => PlatformWindsorManager.GetService<IExplorerDao>();
        protected IRMGoogleSettingDao GoogleSettingDao => PlatformWindsorManager.GetService<IRMGoogleSettingDao>();
        protected ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        protected readonly RuleManager RuleManager;
        protected readonly LabelManager LabelManager;
        protected readonly ReportCenter ReportCenter;
        protected readonly RecordManager RecordManager;
        protected StopJobCts Cts;
        protected JobType jobType;
        protected string jobId;
        protected string? tenantId;
        protected RMProfileDto ProfileDto;
        protected RMAosGoogleAppProfile? appProfile;
        const int MaxDegreeOfParallelism = 10;

        #endregion

        public BaseReportProcessor(string jobId, string profileId)
        {
            ReportCenter = new ReportCenter();
            RecordManager = new RecordManager();
            LabelManager = new LabelManager();
            RuleManager = new RuleManager();
            ProfileDto = ReportCenter.GetReportProfile(profileId);
            Cts = new StopJobCts();
            this.jobId = jobId;
        }

        public void Build(string customerId, string tenantId)
        {
            appProfile = RMAosApiClient.GetGoogleAppProfile(customerId, tenantId);
        }

        public async Task KickOffAsync()
        {
            try
            {
                using (new CheckJobStopScope())
                {
                    // get need run report nodes
                    List<GoogleDriveTreeNodeDto> treeNodes = InitializeReportProcess();
                    InitializeReport();
                    logger.Info($"Start processing [{treeNodes.Count}] node(s) to generate report.");
                    foreach (var node in treeNodes)
                    {
                        var treeNode = await RemoteGoogleNodeService.GetRemoteNodeByDriveIdAsync(node.ID);
                        var tenantId = treeNode.GoogleTenantId;
                        this.tenantId = tenantId;
                        this.Build(TenantLocalValue.LogonGroupId, tenantId);
                        RunNowAsync(node);
                    }
                }
                var finalStatus = ReportCenter.Completed();
                StartScheduledExport(finalStatus);
            }
            catch (Exception e)
            {
                ReportCenter.SetJobFinish(JobStatus.Failed, e.Message);
                logger.Error($"Run Report job failed. Error: {e}");
            }
        }

        private void StartScheduledExport(JobStatus finalStatus)
        {
            if (jobType != JobType.GoogleItemsFilesDueDisposalReport || ProfileDto?.ScheduleId == null
                || (finalStatus != JobStatus.Finished && finalStatus != JobStatus.FinishWithException))
            {
                return;
            }

            var jobIdReal = jobId?.Split('_')[0];
            var exportModel = new ExportReportCommonModel
            {
                ReportJobType = ((int)ProfileDto.Type).ToString(),
                ReportJobId = jobIdReal,
                ProfileName = ProfileDto.ProfileName,
                ProfileId = ProfileDto.Id.ToString(),
            };
            var reportParameters = SerializerHelper.SerializeByJsonConvert(exportModel);
            ReportService.RunExportReportJob(reportParameters);
            logger.Info("Started scheduled Google due-disposal report export. JobId:{0}, ProfileId:{1}", jobIdReal, ProfileDto.Id);
        }

        public List<GoogleDriveTreeNodeDto> InitializeReportProcess()
        {
            try
            {
                ReportCenter.InitCurrentJobInfo(jobId, jobType);
                RecordManager.Init(ReportCenter, SourceFlag.Google);
                RMSubJob subJobInfo = ReportCenter.GetSubJobInfo(jobId, true);
                List<GoogleDriveTreeNodeDto> nodes = new();
                if (!string.IsNullOrEmpty(subJobInfo.JobContext.Settings))
                {
                    var result =  SerializerHelper.DeserializeByDataContractSerializer<List<GoogleDriveTreeNodeDto>>(subJobInfo.JobContext.Settings);
                    foreach (var tempNode in result)
                    {
                        if (tempNode.Level is NodeLevel.GoogleSharedDriveContainer or NodeLevel.GoogleMyDriveContainer)
                        {
                            logger.Info("skip google drive node {0}", tempNode.FullPath);
                        }
                        var setting = GoogleSettingDao.GetSettingInfoByAgentId(tempNode.ID);
                        if (setting == null)
                        {
                            setting = GoogleSettingDao.GetSettingInfoByAgentId(tempNode.ParentId);
                        }
                        if (setting != null && (setting.EnableRecordManagement == (int)EnableRecordManagementSetting.Enable || jobType == JobType.GoogleRestoreReport))
                        {
                            nodes.Add(tempNode);
                        }
                        else
                        {
                            if (this is not GoogleCreationAndDestructionProcessor)
                            {
                                JMReportJobDetails detail = new JMReportJobDetails();
                                detail.Type = I18NResource.ObjectLevelDrive;
                                detail.TitleOrName = tempNode.Name;
                                detail.Url = string.Empty;
                                detail.Status = JobDetailsStatus.Skipped;
                                detail.Comment = "RM_JS_JMD_DisableRecordManagement";
                                ReportCenter.SendJobReport(detail);
                                logger.Info("node is disable {0}", tempNode.FullPath);
                            }
                        }
                    }
                }

                return nodes;
            }
            catch (Exception e)
            {
                logger.Error($"Report ctor error: {e}");
                throw;
            }
        }

        protected abstract void InitializeReport();

        protected virtual void RunNowAsync(GoogleDriveTreeNodeDto treeNode)
        {
            try
            {
                using (new CheckJobStopScope())
                {
                    var itemQueue = new DataQueue<GoogleItemData>();
                    var task = Task.Run(() => ProcessItemDataAsync(itemQueue));
                    ProcessDriveAsync(treeNode, itemQueue).Wait();
                    itemQueue.Complete();
                    task.Wait();
                }
            }
            catch (JobStopException)
            {
                logger.Warn("This Job is stopped.");
                ReportCenter.SetJobFinish(JobStatus.Stopped);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("drive not found", StringComparison.OrdinalIgnoreCase))
                {
                    logger.Error($"Drive is deleted from Google. DriveId: {treeNode.ObjectId}");
                    ReportCenter.RecordFailed(ReportCenter.GenerateCommonReportJobDetail(treeNode, I18NEntity.GetString(I18NResource.NotFoundDrive)), (int)RMNodeLevel.GoogleDrive);
                    return;
                }
                logger.Error("An error occurred while running job. ", e.ToString());
                ReportCenter.RecordFailed(ReportCenter.GenerateCommonReportJobDetail(treeNode, e.Message), (int)RMNodeLevel.GoogleDrive);
                throw;
            }
        }

        protected async Task ProcessItemDataAsync(DataQueue<GoogleItemData> itemQueue)
        {
            using (CheckJobStopScope jScope = new())
            {
                await itemQueue.ToIEnumerable().ParallelExecute(async item =>
                {
                    try
                    {
                        using (CheckJobStopScope subJScope = new CheckJobStopScope())
                        {
                            using (new PerformanceScope("BaseReportProcessor:ProcessDataItemAsync"))
                            {
                                if (item.Level == RMNodeLevel.GoogleFile)
                                {
                                    ProcessFileReport(item);
                                }
                            }
                        }
                    }
                    catch (JobStopException)
                    {
                        logger.Warn("The records disposal job has been stopped.");
                        throw new JobStopException("The job has stopped.");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error occurred while process item [{item.Id}]. Error: {ex}");
                        ReportCenter.RecordFailedCommon(item.GenerateDisposalActionJobDetail(I18NResource.RemoveAndDestroyAction, string.Empty,
                ex.Message), (int)item.Level);
                    }
                }, MaxDegreeOfParallelism, Cts.Token);
            }
        }

        protected async Task ProcessScanTimeRangeDriveAsync(GoogleDriveTreeNodeDto node, DataQueue<GoogleItemData> itemQueue, DateTime startTime, DateTime endTime)
        {
            GoogleDriveData driveData = ConvertHelper.ConvertDtoNodeTreeToData(node, appProfile.TenantId);
            if (startTime != default && endTime != default)
            {
                RMGoogleIncrDiscover incrDiscover = new(itemQueue);
                incrDiscover.Init(ReportCenter, appProfile, true);
                incrDiscover.SetScanTime(startTime, endTime);
                await incrDiscover.DiscoverAsync(driveData, false, Cts.Token);
            }
            else
            {
                RMGoogleFullDiscover fullDiscover = new(itemQueue);
                fullDiscover.Init(ReportCenter, appProfile, true);
                await fullDiscover.DiscoverAsync(driveData, Cts.Token);
            }
        }

        protected abstract Task ProcessDriveAsync(GoogleDriveTreeNodeDto treeNode, DataQueue<GoogleItemData> itemQueue);

        protected abstract void ProcessFileReport(GoogleItemData file);
    }
}
