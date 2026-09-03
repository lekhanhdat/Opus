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
using AvePoint.RA.Common.Email;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Email.Model;
using AvePoint.RA.RACommonUtility.Email.Sender;
using AvePoint.RA.RACommonUtility.Email.Sender.Middleware;
using AvePoint.RA.RACommonUtility.Email.Sender.Storage;
using RATeams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class ProcessJobEmailNotificationExecutor
    {
        private static RALogger s_logger => RALogger.GetInstance(typeof(ProcessJobEmailNotificationExecutor));

        private static readonly IRMReportService RMReportService = PlatformWindsorManager.GetService<IRMReportService>();

        private static readonly IGeneralSettingService GeneralSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static readonly RMEmailSender s_emailSender = new(new RMEmailMemoryStorage(new RMEmailStorageDefaultMiddleware()));

        private static readonly IJobMonitorDao JobMonitorDao = PlatformWindsorManager.GetService<IJobMonitorDao>();
        protected readonly IJobProgressDao JobProgressDao = PlatformWindsorManager.GetService<IJobProgressDao>();

        private static readonly Dictionary<NotificationJobType, string> JobTypeI18N = new()
        {
            {NotificationJobType.ArchiverRestore,  I18NEntity.GetString("RM_JS_JN_JobType_Restore")},
            {NotificationJobType.SOPreScan,  I18NEntity.GetString("RM_JS_JM_JobType_SOPreScan")},
            {NotificationJobType.RMArchiverBackup, I18NEntity.GetString("RM_JS_JM_JobType_RMArchiverBackup")},
            {NotificationJobType.DataSync, I18NEntity.GetString("RM_JS_JM_JobType_DataSynchronisation")},
            {NotificationJobType.DashboardData,  I18NEntity.GetString("RM_JS_JM_JobType_Dashboard")},
            {NotificationJobType.EnforceRetention,  I18NEntity.GetString("RM_JS_JM_JobType_EnforceRetention")},
            {NotificationJobType.EnforceRuleAction,  I18NEntity.GetString("RM_JS_JM_JobType_DisposalActivityManagement")},
            {NotificationJobType.SyncNode, I18NEntity.GetString("RM_JS_JM_JobType_SyncNodesFromAOS")},
            {NotificationJobType.Discovery, I18NEntity.GetString("RM_JS_JM_JobType_DiscoveryJobV3")},
            {NotificationJobType.TermSync, I18NEntity.GetString("RM_JS_JM_JobType_TermSynchronization")},
        };

        private const string EmailBody = @"<table cellspacing=""0"" style=""border-collapse:collapse; width:756px""><tbody><tr><td class=""oa1"" style=""background-color:#f2f2f2; border-color:white; border-style:solid; border-width:1px 1px 4px; height:44px; padding-left:7px; padding-right:7px; padding-top:1px; vertical-align:middle; width:180pt""><span style=""font-size:18pt""><span style=""font-weight:400""><span style=""font-style:normal""><span style=""text-decoration:none""><span style=""font-family:Arial,Helvetica,sans-serif""><span style=""language:en-US""><span style=""line-height:115%""><span style=""unicode-bidi:embed""><span style=""word-break:normal""><span style=""punctuation-wrap:hanging""><span style=""font-size:10pt""><span style=""font-family:Calibri""><span style=""color:black""><span style=""language:en-US""><strong style=""font-weight:bold"">Job</strong></span></span></span></span></span></span></span></span></span></span></span></span></span></span></td><td class=""oa2"" style=""background-color:#f2f2f2; border-color:white; border-style:solid; border-width:1px 1px 4px; padding-left:7px; padding-right:7px; padding-top:1px; text-align:center; vertical-align:middle; width:85pt""><span style=""font-size:18pt""><span style=""font-weight:400""><span style=""font-style:normal""><span style=""text-decoration:none""><span style=""font-family:Arial,Helvetica,sans-serif""><span style=""language:en-US""><span style=""line-height:115%""><span style=""unicode-bidi:embed""><span style=""word-break:normal""><span style=""punctuation-wrap:hanging""><span style=""font-size:10pt""><span style=""font-family:Calibri""><span style=""color:black""><span style=""language:en-US""><strong style=""font-weight:bold"">Successful</strong></span></span></span></span></span></span></span></span></span></span></span></span></span></span></span></td><td class=""oa2"" style=""background-color:#f2f2f2; border-color:white; border-style:solid; border-width:1px 1px 4px; padding-left:7px; padding-right:7px; padding-top:1px; text-align:center; vertical-align:middle; width:80pt""><span style=""font-size:18pt""><span style=""font-weight:400""><span style=""font-style:normal""><span style=""text-decoration:none""><span style=""font-family:Arial,Helvetica,sans-serif""><span style=""language:en-US""><span style=""line-height:115%""><span style=""unicode-bidi:embed""><span style=""word-break:normal""><span style=""punctuation-wrap:hanging""><span style=""font-size:10pt""><span style=""font-family:Calibri""><span style=""color:black""><span style=""language:en-US""><strong style=""font-weight:bold"">Failed</strong></span></span></span></span></span></span></span></span></span></span></span></span></span></span></span></td><td class=""oa2"" style=""background-color:#f2f2f2; border-color:white; border-style:solid; border-width:1px 1px 4px; padding-left:7px; padding-right:7px; padding-top:1px; text-align:center; vertical-align:middle; width:88pt""><span style=""font-size:18pt""><span style=""font-weight:400""><span style=""font-style:normal""><span style=""text-decoration:none""><span style=""font-family:Arial,Helvetica,sans-serif""><span style=""language:en-US""><span style=""line-height:115%""><span style=""unicode-bidi:embed""><span style=""word-break:normal""><span style=""punctuation-wrap:hanging""><span style=""font-size:10pt""><span style=""font-family:Calibri""><span style=""color:black""><span style=""language:en-US""><strong style=""font-weight:bold"">Exception</strong></span></span></span></span></span></span></span></span></span></span></span></span></span></span></span></td><td class=""oa2"" style=""background-color:#f2f2f2; border-color:white; border-style:solid; border-width:1px 1px 4px; padding-left:7px; padding-right:7px; padding-top:1px; text-align:center; vertical-align:middle; width:73pt""><span style=""font-size:18pt""><span style=""font-weight:400""><span style=""font-style:normal""><span style=""text-decoration:none""><span style=""font-family:Arial,Helvetica,sans-serif""><span style=""language:en-US""><span style=""line-height:115%""><span style=""unicode-bidi:embed""><span style=""word-break:normal""><span style=""punctuation-wrap:hanging""><span style=""font-size:10pt""><span style=""font-family:Calibri""><span style=""color:black""><span style=""language:en-US""><strong style=""font-weight:bold"">Skipped</strong></span></span></span></span></span></span></span></span></span></span></span></span></span></span></td><td class=""oa2"" style=""background-color:#f2f2f2; border-color:white; border-style:solid; border-width:1px 1px 4px; padding-left:7px; padding-right:7px; padding-top:1px; text-align:center; vertical-align:middle; width:84pt""><span style=""font-size:18pt""><span style=""font-weight:400""><span style=""font-style:normal""><span style=""text-decoration:none""><span style=""font-family:Arial,Helvetica,sans-serif""><span style=""language:en-US""><span style=""line-height:115%""><span style=""unicode-bidi:embed""><span style=""word-break:normal""><span style=""punctuation-wrap:hanging""><span style=""font-size:10pt""><span style=""font-family:Calibri""><span style=""color:black""><span style=""language:en-US""><strong style=""font-weight:bold"">Stopped</strong></span></span></span></span></span></span></span></span></span></span></span></span></span></span></td></tr>{0}</tbody></table>";

        public async Task ExecutorAsync()
        {
            var profiles = await RMReportService.GetJobNotificationProfiles();
            if (profiles == null || profiles.Count == 0)
            {
                s_logger.Info("No job notification need to processing.");
                return;
            }

            var NotificationProfiles = await Task.WhenAll(profiles.ConvertAll(ConvertToJobNotificationProfile));
            var dailyProfiles = NotificationProfiles.Where(profile => profile.ProfileInterval.IntervalType == NotificationIntervalType.Daily);
            s_logger.Info($"Daily job notification count is {dailyProfiles.Count()}.");

            var weeklyProfiles = NotificationProfiles.Where(profile => profile.ProfileInterval.IntervalType == NotificationIntervalType.Weekly);
            s_logger.Info($"Weekly job notification count is {dailyProfiles.Count()}.");

            foreach (var dailyProfile in dailyProfiles)
            {
                try
                {
                    await PorcessProfile(dailyProfile);
                }
                catch (Exception e)
                {
                    s_logger.Error($"Daily job notification [{dailyProfile.ProfileName}] send email Failed, Error : {e}.");
                }
            }

            foreach (var weeklyProfile in weeklyProfiles)
            {
                try
                {
                    var gls = await GeneralSettingService.GetGeneralSettingAsync();
                    var localTime = GeneralSettingService.ConvertTiksToDateTime(gls, DateTime.UtcNow.Ticks, true);
                    var week = localTime.DataTime.DayOfWeek;
                    var weeklyType = weeklyProfile.ProfileInterval.WeeklyType;
                    if (weeklyType != week)
                    {
                        continue;
                    }
                    await PorcessProfile(weeklyProfile);
                }
                catch (Exception e)
                {
                    s_logger.Error($"Weekly job notification [{weeklyProfile.ProfileName}] send email Failed, Error : {e}.");
                }
            }
        }

        private async Task PorcessProfile(JobNotificationResult profile)
        {
            s_logger.Info($"Begin to process job notification [{profile.ProfileName}].");
            var receivers = profile.ProfileEmailReceivers;
            var jobInfos = profile.ProfileJobInfos;
            var emailBodyList = new List<string>();
            foreach (var jobInfo in jobInfos)
            {
                var needSendEmailJobs = GetNeedSendEmailJobs(profile.ProfileInterval, jobInfo);
                emailBodyList.Add(BuildEmailJobDetails(jobInfo, needSendEmailJobs));
            }
            var jobDetail = string.Format(EmailBody, string.Concat(emailBodyList));
            s_logger.Info($"Job notification [{profile.ProfileName}] needed send email job type {string.Join(';', jobInfos.Select(jobInfo => jobInfo.JobType))}.");
            await SendEmailAsync(receivers, new JobNotificationParameterDto
            {
                JobDetail = jobDetail,
            });
            s_logger.Info($"Job notification [{profile.ProfileName}] send email successful.");
        }

        private static string BuildEmailJobDetails(NotificationJobInfo jobInfo, List<RMJobMonitor> needSendEmailJobs)
        {
            string GetStatusCount(int status) =>
                jobInfo.JobStatuses.Contains((JobStatus)status) ? needSendEmailJobs.Count(job => job.Status == status).ToString() : "-";

            var finished = GetStatusCount((int)JobStatus.Finished);
            var failed = GetStatusCount((int)JobStatus.Failed);
            var finishedWithException = GetStatusCount((int)JobStatus.FinishWithException);
            var stopped = GetStatusCount((int)JobStatus.Stopped);
            var skipped = GetStatusCount((int)JobStatus.Skipped);

            var jobDetailTemplate = @"<tr><td class=""oa3"" style=""background-color:#f2f2f2; border-color:white; border-style:solid; border-width:4px 1px 1px; height:44px; padding-left:7px; padding-right:7px; padding-top:1px; text-align:justify; vertical-align:middle; width:180pt""><span style=""font-size:18pt; font-weight:400; font-style:normal; text-decoration:none; font-family:Arial,Helvetica,sans-serif; language:en-US; line-height:115%; text-justify:inter-ideograph; unicode-bidi:embed; word-break:normal; punctuation-wrap:hanging; font-size:10pt; font-family:Calibri; color:black; language:en-US;""><strong style=""font-weight:bold"">{0}</strong></span></td><td class=""oa4"" style=""border-color:white; border-style:solid; border-width:4px 1px 1px; padding-left:7px; padding-right:7px; padding-top:1px; text-align:center; vertical-align:middle; width:85pt"">{1}</td><td class=""oa4"" style=""border-color:white; border-style:solid; border-width:4px 1px 1px; padding-left:7px; padding-right:7px; padding-top:1px; text-align:center; vertical-align:middle; width:80pt"">{2}</td><td class=""oa4"" style=""border-color:white; border-style:solid; border-width:4px 1px 1px; padding-left:7px; padding-right:7px; padding-top:1px; text-align:center; vertical-align:middle; width:88pt"">{3}</td><td class=""oa4"" style=""border-color:white; border-style:solid; border-width:4px 1px 1px; padding-left:7px; padding-right:7px; padding-top:1px; text-align:center; vertical-align:middle; width:73pt"">{4}</td><td class=""oa4"" style=""border-color:white; border-style:solid; border-width:4px 1px 1px; padding-left:7px; padding-right:7px; padding-top:1px; text-align:center; vertical-align:middle; width:84pt"">{5}</td></tr>";

            return string.Format(jobDetailTemplate,
                JobTypeI18N[jobInfo.JobType],
                finished,
                failed,
                finishedWithException,
                skipped,
                stopped);
        }

        private List<RMJobMonitor> GetNeedSendEmailJobs(NotificationInterval interval, NotificationJobInfo jobInfo)
        {
            var nowDate = DateTime.UtcNow;

            var processJobTypes = GetJobTypesByNotification(jobInfo.JobType);
            var previousTicks = long.MinValue;

            processJobTypes = TeamsPermissionHelper.FilterAllowedTeamsJobTypes(processJobTypes);

            if (processJobTypes.Count == 0)
            {
                return [];
            }
            if (interval.IntervalType == NotificationIntervalType.Daily)
            {
                previousTicks = nowDate.AddDays(-1).Ticks;
            }
            else
            {
                previousTicks = nowDate.AddDays(-7).Ticks;

            }
            return JobMonitorDao.GetJobInfoByTimeRangeAndStatus(previousTicks, nowDate.Ticks, processJobTypes, jobInfo.JobStatuses); ;
        }

        private List<JobType> GetJobTypesByNotification(NotificationJobType notificationJobType)
        {
            switch (notificationJobType)
            {
                case NotificationJobType.ArchiverRestore:
                    return [
                        JobType.ArchiverRestore, 
                        JobType.ArchiverOutPlaceRestore, 
                        JobType.StubOopRestore, 
                        JobType.AOSPRestore, 
                        JobType.TeamsArchiverRestore, 
                        JobType.TeamsOutPlaceRestore,
                        JobType.MailBoxArchiverRestore,
                        JobType.ArchiverToSpoRestore,
                        ];
                case NotificationJobType.SOPreScan:
                    return [
                        JobType.SOPreScan,
                        JobType.TeamsPreScan,
                        ];
                case NotificationJobType.RMArchiverBackup:
                    return [
                        JobType.RMArchiverBackup,
                        JobType.RMEndUserArchiverBackup,
                        JobType.TeamsArchiverBackup,
                        JobType.SpecifyTeamsArchiverBackup,
                        ];//TODO Cyrus SpecifySitesArchiverBackup
                case NotificationJobType.DataSync:
                    return [
                        JobType.DataSynchronisation,
                        JobType.AzureFileShareDataSynchronisation,
                        JobType.AzureFileShareDataSynchronisationSchedule,
                        JobType.EXODataSynchronisation,
                        JobType.EXODataSynchronisationSchedule,
                        JobType.SPDataSynchronisationSchedule,
                        JobType.OneDriveDataSynchronisation,
                        JobType.OneDriveDataSynchronisationSchedule,
                        JobType.FSDataSynchronization,
                        JobType.FSDataSynchronizationSchedule,
                        JobType.SPOnPremDataSync,
                        JobType.SPOnPremDataSyncSchedule,
                        JobType.BoxDataSynchronisation,
                        JobType.BoxDataSynchronisationSchedule, 
                        JobType.TeamsDataSynchronisation,
                        JobType.TeamsDataSynchronisationSchedule,
                        ];
                case NotificationJobType.DashboardData:
                    return [JobType.Dashboard];
                case NotificationJobType.EnforceRetention:
                    return [JobType.EnforceRetention, JobType.EXOEnforceRetention, JobType.OneDriveEnforceRetention, JobType.TeamsEnforceRetention];
                case NotificationJobType.EnforceRuleAction:
                    return [
                        JobType.DisposalActivityManagement,
                        JobType.RecordsDisposal,
                        JobType.EXORecordsDisposal,
                        JobType.FSDisposal,
                        JobType.OneDriveRecordsDisposal,
                        JobType.PhysicalDisposal,
                        JobType.PhysicalRecordsDisposal,
                        JobType.SPOnPremEnforceRuleAction,
                        JobType.SPOnPremEnforceRuleActionSchedule,
                        JobType.BoxRecordsDisposal,
                        JobType.TeamsRecordsDisposal,
                    ];
                case NotificationJobType.SyncNode:
                    return [JobType.SyncNodesFromAOS];
                case NotificationJobType.Discovery:
                    return [JobType.DiscoveryJobV3, JobType.DiscoveryJobV4, JobType.DiscoveryJobV5, JobType.SFDiscoveryJob, JobType.DiscoveryGoogleJobV1];
                case NotificationJobType.TermSync:
                    return [JobType.TermSynchronization, JobType.SPOnPremTermSynchronization, JobType.SPOnPremTermSynchronizationSchedule];
                default:
                    return [];
            }
        }

        private async Task<JobNotificationResult> ConvertToJobNotificationProfile(RMProfileDto profile)
        {
            var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
            var result = SerializerHelper.DeserializeByDataContractSerializer<JobNotificationDto>(profile.Extension1);
            return new()
            {
                ProfileId = profile.Id,
                ProfileName = result.ProfileName,
                ProfileCreatedTime = GeneralSettingService.ConvertTiksToDateTime(generalSetting, long.Parse(result.ProfileCreatedTime), true).SimplifyFormatTime,
                ProfileDes = result.ProfileDes,
                ProfileEmailReceivers = result.ProfileEmailReceivers,
                ProfileInterval = result.ProfileInterval,
                ProfileJobInfos = result.ProfileJobInfos
            };
        }

        private async Task SendEmailAsync(List<ToUserInfo> accounts, JobNotificationParameterDto parameter)
        {
            try
            {
                var parameters = new List<RMJobNotificationEmailTemplateParameters>();
                foreach (var temp in accounts)
                {
                    parameters.Add(new RMJobNotificationEmailTemplateParameters()
                    {
                        RequestReviewer = temp.DisplayName,
                        RequestJobDetail = parameter.JobDetail,
                        ToUser = temp.UserPrincipalName,
                        TemplateType = RMEmailTemplateType.JobNotification,
                    });
                }
                var templateId = RMEmailTemplateId.JOB_NOTIFICATION;
                s_emailSender.AddRange(templateId, parameters);
                await s_emailSender.SendAsync();
                s_logger.Info($"Succeed send job notification email to users.");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while send job notification email to users. Error: {e}");
            }
        }
    }
}
