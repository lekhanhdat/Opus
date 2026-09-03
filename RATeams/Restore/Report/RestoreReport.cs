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

//namespace Office365GroupRestore
//{
//    #region

//    using System;
//    using System.Collections.Generic;
//    using System.Text;

//    using AvePoint.Common;
//    using AvePoint.Core.License;
//    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup;
//    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineBackup.Object;
//    using AvePoint.GCommon.Contract.Extension;
//    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
//    using AvePoint.GCommon.Contract.Server.Job.Object;
//    using AvePoint.GCommon.JobManagement;

//    using ExchangeCommonWrapper;
//    using Job.ModernManagement.Config;
//    using Job.ModernManagement.Report;
//    using Microsoft365Backup.CommonUtil.Misc;
    

//    #endregion

//    public class RestoreReport : JobReport, IRestoreReport, IDisposable
//    {
//        #region Properties
//        private readonly ICloudBackupLogger log = CloudBackupLogManager.Get(typeof(RestoreReport));

//        public IMExchangeOnlineBackupJobManagementService ExchangeBackupService { get; set; }

//        public RestoreConfig Config { get; set; }

//        protected override List<char> HighLevelTypes { get; set; } = [ReportNodeHeader.Group, ReportNodeHeader.Team];

//        protected override List<char> ComputeTypesForProgress => [ReportNodeHeader.Group, ReportNodeHeader.Team, ReportNodeHeader.Channel, ReportNodeHeader.Conversation, ReportNodeHeader.Plan, ReportNodeHeader.Task];

//        public new long TotalCount { get => base.TotalCount; set { base.TotalCount = value; } }

//        public long CurrentCount { get; set; }

//        #endregion

//        private static RestoreReport instance;

//        public static RestoreReport GetInstance()
//        {
//            return instance;
//        }

//        public void Init(RestoreConfig config)
//        {
//            //Config = config;
//            //instance = this;
//            //JobConfig = new JobConfig(config.JobId, config.JobType, config.JobId.Split("_")[0], config.JobDir, config.PlanId, config.IsMicrosoftTeams ? BackupModule.Teams : BackupModule.Office365Group, false, config.ReportOnlyHighLevel, config.SkippedErrorCodeList);
//            //AgentName = AveEnv.AgentName;
//            //JobStatusInfo = new JobStatusInfo { AgentHost = AveEnv.AgentAddress, Id = JobConfig.Id, Type = JobConfig.Type, Progress = 1 };
//            //SubJobInfo = new SubJobDto { Id = JobConfig.Id, ParentId = JobConfig.ParentId, PlanId = config.PlanId };
//            //JobReportServiceFactory = new JobReportServiceFactoryImpl();
//        }

//        public void AddReport(ReportDto reportDto)
//        {
//            CurrentCount++;
//            base.AddReport(reportDto);
//        }

//        public override void Finish(JobState status, string message)
//        {
//            try
//            {
//                log.Info("Finish Status : {0}", status);
//                AddLastReport(message);
//                SendJobDataSizeToRedis();
//                base.Finish(status, message);
//            }
//            catch (Exception ex)
//            {
//                log.Error("Finish with error : {0}", ex.ToString());
//            }
//        }

//        private void AddLastReport(string errorMessage)
//        {
//            if (!string.IsNullOrEmpty(errorMessage) && CurrentCount == 0)
//            {//Send job failed reason to server
//                AddReport(new ReportDto
//                {
//                    Path = string.Empty,
//                    Title = string.Empty,
//                    ErrorMessage = errorMessage,
//                    Type = ReportNodeHeader.Root,
//                    Status = ReportStatus.Failed
//                });
//            }
//        }

//        private Dictionary<string, string> reportTypeMapping;

//        #region Communacation with Control

//        protected override void SendJobStatus(bool isProgress, bool isPaused = false)
//        {
//            try
//            {
//                //var jobStatusService = JobReportServiceFactory.CreateJobStatusUpdater();
//                //if (isProgress)
//                //{
//                //    log.Info("Update job progress : {0}", JobStatusInfo.Progress);
//                //    JobProcessUtility.CheckIfJobCancelled(jobStatusService.UpdateJobProgress(JobStatusInfo));
//                //}
//                //else
//                //{
//                //    log.Info("Update job status to control.");
//                //    jobStatusService.UpdateJobStatus(JobStatusInfo);
//                //}
//            }
//            catch (Exception ex)
//            {
//                log.Error("Send job status with error : {0}", ex.ToString());
//            }
//        }

//        protected override void SendJobSummary()
//        {
//            //try
//            //{
//            //    var successD = new Dictionary<EOBackupLevel, int>()
//            //    {
//            //        {EOBackupLevel.Team, GetKindCount($"{ReportStatus.Success}{ReportNodeHeader.Team}") },
//            //        {EOBackupLevel.Channel, GetKindCount($"{ReportStatus.Success}{ReportNodeHeader.Channel}") },
//            //        {EOBackupLevel.Conversation, GetKindCount($"{ReportStatus.Success}{ReportNodeHeader.Conversation}") },
//            //        {EOBackupLevel.PlannerPlan, GetKindCount($"{ReportStatus.Success}{ReportNodeHeader.Plan}") },
//            //        {EOBackupLevel.PlannerTask, GetKindCount($"{ReportStatus.Success}{ReportNodeHeader.Task}") }
//            //    };

//            //    var failedD = new Dictionary<EOBackupLevel, int>()
//            //    {
//            //        {EOBackupLevel.Team, GetKindCount($"{ReportStatus.Failed}{ReportNodeHeader.Team}") },
//            //        {EOBackupLevel.Channel, GetKindCount($"{ReportStatus.Failed}{ReportNodeHeader.Channel}") },
//            //        {EOBackupLevel.Conversation, GetKindCount($"{ReportStatus.Failed}{ReportNodeHeader.Conversation}") },
//            //        {EOBackupLevel.PlannerPlan, GetKindCount($"{ReportStatus.Failed}{ReportNodeHeader.Plan}") },
//            //        {EOBackupLevel.PlannerTask, GetKindCount($"{ReportStatus.Failed}{ReportNodeHeader.Task}") },
//            //    };

//            //    var SkipD = new Dictionary<EOBackupLevel, int>()
//            //    {
//            //        {EOBackupLevel.Team, GetKindCount($"{ReportStatus.Skipped}{ReportNodeHeader.Team}") },
//            //        {EOBackupLevel.Channel, GetKindCount($"{ReportStatus.Skipped}{ReportNodeHeader.Channel}") },
//            //        {EOBackupLevel.Conversation, GetKindCount($"{ReportStatus.Skipped}{ReportNodeHeader.Conversation}") },
//            //        {EOBackupLevel.PlannerPlan, GetKindCount($"{ReportStatus.Skipped}{ReportNodeHeader.Plan}") },
//            //        {EOBackupLevel.PlannerTask, GetKindCount($"{ReportStatus.Skipped}{ReportNodeHeader.Task}") },
//            //    };
//            //    //for office 365 group
//            //    var successCD = new Dictionary<EOBackupLevel, int>()
//            //    {
//            //        {EOBackupLevel.MailBox, GetKindCount($"{ReportStatus.Success}{ReportNodeHeader.Group}") }
//            //    };
//            //    var failedCD = new Dictionary<EOBackupLevel, int>()
//            //    {
//            //        {EOBackupLevel.MailBox, GetKindCount($"{ReportStatus.Failed}{ReportNodeHeader.Group}") }
//            //    };
//            //    var SkipCD = new Dictionary<EOBackupLevel, int>()
//            //    {
//            //        {EOBackupLevel.MailBox, GetKindCount($"{ReportStatus.Skipped}{ReportNodeHeader.Group}") }
//            //    };
//            //    var exchangeSubJobBackupResult = new ExchangeSubJobBackupResult()
//            //    {
//            //        SuccessObjects = successD,
//            //        FailedObjects = failedD,
//            //        SkippedObjects = SkipD,
//            //        //for office 365 group
//            //        SuccessContainer = successCD,
//            //        FailedContainer = failedCD,
//            //        SkipContainer = SkipCD
//            //    };
//            //    JobManagement jobManagement = new JobManagement(IdentityManager.IdentityContent);
//            //    jobManagement.UpdateSubJobBackupResultForGroupOrTeamsMetadata(Config.JobId, exchangeSubJobBackupResult.Convert());
//            //}
//            //catch (Exception ex)
//            //{
//            //    log.Error("An error occurred while sending the job summary to subjob table. Error message : {0}", ex.ToString());
//            //}
//        }

//        private void SendJobDataSizeToRedis()
//        {
//            ExchangeBackupService.UpdateSubjobMediaDataSize(IdentityManager.IdentityContent, Config.JobId, DataSize, DataSize);
//        }

//        public void UpdateRehydrate(string subJobId, JobState jobState)
//        {
//            //JobStatusInfo.State = (int)jobState;
//            //SendJobStatus(false);
//        }

//        #endregion

//        public void Dispose()
//        {
//        }

//        public void StartKeepAliveThread() => StartKeepAlive();

//        protected override string ConverCharToString(char type)
//        {
//            return type switch
//            {
//                ReportNodeHeader.Mailbox => "Mailbox",
//                ReportNodeHeader.Folder => "Folder",
//                ReportNodeHeader.Item => "Item",
//                ReportNodeHeader.Team => "Team",
//                ReportNodeHeader.Channel => "Channel",
//                ReportNodeHeader.Conversation => "Conversation",
//                ReportNodeHeader.Plan => "Plan",
//                ReportNodeHeader.Task => "Task",
//                _ => type.ToString()
//            };
//        }
//    }

//    internal static class ReportNodeHeader
//    {
//        internal const string Success = "T";
//        internal const string Fail = "F";

//        internal const char Group = 'G';
//        internal const char Mailbox = 'M';
//        internal const char Folder = 'F';
//        internal const char Item = 'I';
//        internal const char Team = 'T';
//        internal const char Channel = 'C';
//        internal const char Conversation = 'R';
//        internal const char Root = 'Z';

//        internal const char Plan = 'P';
//        internal const char Task = 'A';//此处按照单词字母顺序，找到第一个没有被占用的字母

//        internal const string Type = "t";
//        internal const string Size = "s";
//        internal const string Skiped = "isSkipped";
//    }
//}