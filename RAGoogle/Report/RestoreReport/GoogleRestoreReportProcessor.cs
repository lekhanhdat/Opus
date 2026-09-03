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
using AvePoint.RA.Common;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.SharePoint.Object;
using RAGoogle.Models;
using RAGoogle.Restore;
using Util;
using AvePoint.RA.Common.Global.Utils;

namespace RAGoogle.Report.RestoreReport
{
    public class GoogleRestoreReportProcessor : BaseReportProcessor
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(GoogleRestoreReportProcessor));
        private IJobMonitorDao JMDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private IRMReportService _rMReportService => PlatformWindsorManager.GetService<IRMReportService>();


        private GDriveRestoreReportDetailWorker _gDriveDetailWorker = new GDriveRestoreReportDetailWorker();
        private RMCreationJobMessage _msg = null;
        private DateTime startUtcTime;
        private DateTime endUtcTime;
        private JobContext jobContext;
        private RMProfileDto _profile;
        private StatisticGDriveRestoreJobDetailsExecutor statisticRestoreJobDetailsExecutor;

        public GoogleRestoreReportProcessor(RMCreationJobMessage message) : base(message.JobID, message.ProfileId)
        {
            this.jobType = JobType.GoogleRestoreReport;
            this._msg = message;
            this._msg.EndTime = this._msg.EndTime.AddDays(1);
            var globalTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(this._msg.GlobalTimeZoneId.Replace("_", " "));
            startUtcTime = TimeZoneInfo.ConvertTimeToUtc(this._msg.StartTime, globalTimeZone);
            endUtcTime = TimeZoneInfo.ConvertTimeToUtc(this._msg.EndTime, globalTimeZone);
            jobContext = JobContext.GetInstance(message.JobID, message.JobType);
            statisticRestoreJobDetailsExecutor = new StatisticGDriveRestoreJobDetailsExecutor(startUtcTime, endUtcTime, message.ProfileId, jobContext);

        }
        public async Task RunReportAsync()
        {
            try
            {
                statisticRestoreJobDetailsExecutor.StatictisRestoreJobDetails();
                StartScheduledExport();
                await base.KickOffAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("");
            }
        }
        private void StartScheduledExport()
        {
            var profile = _rMReportService.GetProfileByIdAsync(_msg.ProfileId).GetAwaiter().GetResult();
            if (profile?.ScheduleId != null)
            {
                var jobIdReal = _msg.JobID?.Split('_')[0];
                var job = JMDao.GetJobById(jobIdReal);
                if (job.Status == (int)AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Finished || job.Status == (int)AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.FinishWithException)
                {
                    var exportModel = new ExportReportCommonModel
                    {
                        ReportJobType = ((int)profile.Type).ToString(),
                        ReportJobId = jobIdReal,
                        ProfileName = profile.ProfileName,
                        ProfileId = profile.Id.ToString(),
                    };
                    var reportParameters = SerializerHelper.SerializeByJsonConvert(exportModel);
                    _rMReportService.RunExportReportJob(reportParameters);
                }
            }
        }
        protected override void RunNowAsync(GoogleDriveTreeNodeDto treeNode)
        {
            try
            {
                if (treeNode.Level == NodeLevel.GoogleMyDrive || treeNode.Level == NodeLevel.GoogleSharedDrive)
                {
                    int totalCount = 0;
                    string condition = $"StartTime > {_profile.StartTime.Ticks} and FinishTime < {_profile.EndTime.Ticks} and Level in ('RM_JS_Rule_ObjectLevel_GoogleFile','RM_JS_Rule_ObjectLevel_GoogleDriveFileVersion') and Status = '{(int)JobDetailsStatus.Successful}'";
                    int startPage = 1;
                    IEnumerable<JMJobDetails> googleDriveResult;
                    do
                    {
                        googleDriveResult = _gDriveDetailWorker.GetData(1000, startPage, ref totalCount, condition, treeNode.ObjectId);
                        if (googleDriveResult != null && googleDriveResult.Count() != 0)
                        {
                            InsertIntoRestoreReport(googleDriveResult);
                            startPage++;
                        }
                    }
                    while (googleDriveResult != null && googleDriveResult.Count() > 0);
                    ReportCenter.AddGenerateRestoreReport(treeNode.DisplayName, JobDetailsStatus.Successful);
                }
                
            }
            catch (JobStopException)
            {
                throw;
            }
            catch (Exception e)
            {
                ReportCenter.AddGenerateRestoreReport(treeNode.DisplayName, JobDetailsStatus.Failed);
                logger.Error("An error occurred while running google restore report job. ", e.ToString());
                throw;
            }
        }
        private void InsertIntoRestoreReport(IEnumerable<JMJobDetails> googleDriveResults)
        {
            List<BaseReport> resultReport = new List<BaseReport>();
            foreach (var itemDetail in googleDriveResults)
            {
                resultReport.Add(ConvertToRestoreFileReport(itemDetail));
            }
            var jobDto = new BaseJobDto()
            {
                Id = jobContext.MainJobId,
                JobType = (int)JobType.GenerateRestoreReport,
            };
            ReportService.SyncReportJobDatas(resultReport, jobDto);
        }
        private RestoreFileReport ConvertToRestoreFileReport(JMJobDetails jobDetail)
        {
            var tempDetail = jobDetail as JMRestoreGDriveDetails;
            RestoreFileReport re = new RestoreFileReport();
            re.Size = tempDetail.Size;
            re.RestoreBy = tempDetail.RestoreBy;
            re.JobId = tempDetail.JobId;
            re.StartTime = tempDetail.StartTime;
            re.EndTime = tempDetail.FinishTime;
            re.RestoreTo = tempDetail.RestoreTo;
            re.IsDaoMigration = tempDetail.IsDaoMigration;
            re.IsEndUserOpt = tempDetail.IsEndUserOpt;
            re.Status = tempDetail.Status;
            re.Comment = tempDetail.Comment;
            re.TitleOrName = tempDetail.Name;
            re.Url = tempDetail.SourceURL;
            re.ObjectLevel = (int)JobReportUtility.ConvertDaoOrOpusLevelToObjectLevel(tempDetail.Level);
            return re;
        }

        protected override void InitializeReport()
        {
            _profile = ReportService.GetProfileByIdAsync(_msg.ProfileId).GetAwaiter().GetResult();

        }

        protected override Task ProcessDriveAsync(GoogleDriveTreeNodeDto treeNode, DataQueue<GoogleItemData> itemQueue)
        {
            throw new NotImplementedException();
        }

        protected override void ProcessFileReport(GoogleItemData file)
        {
            throw new NotImplementedException();
        }
    }
}
