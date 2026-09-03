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
using System.Linq;
using AvePoint.Core.License;
using AvePoint.GCommon.Contract.CoverageReport;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.I18N.Core;
using Job.ModernManagement.Report;
using Microsoft.Graph.Models;
using Office365GroupBackup;

namespace M365GroupTeam
{
    public class ReportUtil
    {

        public static JMJobDetails CreateJobDto(DiscoverMailboxEntity entity, string? ruleName, ActionTab actionTab, string? action = null, string errorMessage = "")
        {
            JMArchiverActionJobDetails mArchiverActionJobDetails = new JMArchiverActionJobDetails
            {
                SourceLocation = entity.DisplayPath,
                FileSize = entity.Size,
                Size = I18NEntity.GetString("RM_JS_Common_Pending"),
                RuleName = ruleName,
                Status = JobDetailsStatus.Successful,
                FinishTime = DateTime.UtcNow.Ticks,
                Level = ConvertStatisticsLevelToI18n(StatisticsLevel.TeamsGroup),
                ActionTab = (int)actionTab,
                Action = action,
                Comment = errorMessage
            };

            return mArchiverActionJobDetails;
        }

        public static JMJobDetails CreateJobDto(DiscoverPlanEntity entity, string ruleName, ActionTab actionTab, string? action = null, string errorMessage = "")
        {
            JMArchiverActionJobDetails jobDetails = new JMArchiverActionJobDetails()
            {
                SourceLocation = entity.DisplayPath,
                FileSize = entity.Size,
                Size = entity.Size.ToString(),
                RuleName = ruleName,
                Status = JobDetailsStatus.Successful,
                FinishTime = DateTime.UtcNow.Ticks,
                Level = ConvertStatisticsLevelToI18n(StatisticsLevel.Plan),
                ActionTab = (int)actionTab,
                Action = action,
                Comment = errorMessage
            };

            return jobDetails;
        }

        public static JMJobDetails CreateJobDto(DiscoverTaskEntity entity, string ruleName, ActionTab actionTab, string? action = null, string errorMessage = "")
        {
            JMArchiverActionJobDetails jobDetails = new JMArchiverActionJobDetails()
            {
                SourceLocation = entity.DisplayPath,
                FileSize = entity.Size,
                Size = entity.Size.ToString(),
                RuleName = ruleName,
                Status = JobDetailsStatus.Successful,
                FinishTime = DateTime.UtcNow.Ticks,
                Level = ConvertStatisticsLevelToI18n(StatisticsLevel.Task),
                ActionTab = (int)actionTab,
                Action = action,
                Comment = errorMessage
            };

            return jobDetails;
        }

        public static JMJobDetails CreateJobDto(DiscoverFolderEntity entity, string ruleName, ActionTab actionTab, string? action = null, string errorMessage = "")
        {
            JMArchiverActionJobDetails jobDetails = new JMArchiverActionJobDetails()
            {
                SourceLocation = entity.DisplayPath,
                FileSize = entity.Size,
                Size = entity.Size.ToString(),
                RuleName = ruleName,
                Status = JobDetailsStatus.Successful,
                FinishTime = DateTime.UtcNow.Ticks,
                Level = ConvertStatisticsLevelToI18n(StatisticsLevel.Channel),
                ActionTab = (int)actionTab,
                Action = action,
                Comment = errorMessage
            };

            return jobDetails;
        }

        public static JMJobDetails CreateJobDto(DiscoverChannelMessageEntity entity, string ruleName, ActionTab actionTab, string? action = null, string errorMessage = "")
        {
            JMArchiverActionJobDetails jobDetails = new JMArchiverActionJobDetails()
            {
                SourceLocation = entity.DisplayPath,
                FileSize = entity.Size,
                Size = entity.Size.ToString(),
                RuleName = ruleName,
                Status = JobDetailsStatus.Successful,
                FinishTime = DateTime.UtcNow.Ticks,
                Level = ConvertStatisticsLevelToI18n(StatisticsLevel.ChannelConversation),
                ActionTab = (int)actionTab,
                Action = action,
                Comment = errorMessage
            };

            return jobDetails;
        }

        public static JMJobDetails CreateJobDto(ChannelM entity, string ruleName, ActionTab actionTab, string? action = null, string errorMessage = "")
        {
            JMArchiverActionJobDetails jobDetails = new JMArchiverActionJobDetails()
            {
                SourceLocation = entity.ChannelName, // buid path ?
                FileSize = entity.Size,
                Size = entity.Size.ToString(),
                RuleName = ruleName,
                Status = JobDetailsStatus.Successful,
                FinishTime = DateTime.UtcNow.Ticks,
                Level = ConvertStatisticsLevelToI18n(StatisticsLevel.ChannelConversation),
                ActionTab = (int)actionTab,
                Action = action,
                Comment = errorMessage
            };

            return jobDetails;
        }

        public static JMJobDetails CreateJobDto(ChannelMessage entity, string ruleName, ActionTab actionTab, string? action = null, string errorMessage = "")
        {
            JMArchiverActionJobDetails jobDetails = new JMArchiverActionJobDetails()
            {
                SourceLocation = entity.DisplayPath,
                FileSize = entity.Size,
                Size = entity.Size.ToString(),
                RuleName = ruleName,
                Status = JobDetailsStatus.Successful,
                FinishTime = DateTime.UtcNow.Ticks,
                Level = ConvertStatisticsLevelToI18n(StatisticsLevel.ChannelConversation),
                ActionTab = (int)actionTab,
                Action = action,
                Comment = errorMessage
            };

            return jobDetails;
        }

        public static JMJobDetails CreateReportDto(string path, long size, string? ruleName, StatisticsLevel sLevel, ActionTab actionTab, string? action = null, string errorMessage = "")
        {
            JMArchiverActionJobDetails jobDetails = new JMArchiverActionJobDetails()
            {
                SourceLocation = path,
                FileSize = size,
                Size = GetSizeStr(size, sLevel),
                RuleName = ruleName,
                Status = JobDetailsStatus.Successful,
                FinishTime = DateTime.UtcNow.Ticks,
                Level = ConvertStatisticsLevelToI18n(sLevel),
                ActionTab = (int)actionTab,
                Action = action,
                Comment = errorMessage
            };

            return jobDetails;
        }

        private static string GetSizeStr(long size, StatisticsLevel level)
        {
            if (level == StatisticsLevel.TeamsGroup)
            {
                return I18NEntity.GetString("RM_JS_Common_Pending");
            }
            return size.ToString();
        }

        public static NodeBackupStatus ConvertToObjectBackupStatus(ReportStatus status)
        {
            return status switch
            {
                ReportStatus.Failed => NodeBackupStatus.Failed,
                ReportStatus.Success => NodeBackupStatus.Successd,
                ReportStatus.Skipped => NodeBackupStatus.Skipped,
                ReportStatus.Filtered => NodeBackupStatus.Skipped,
                ReportStatus.Warn => NodeBackupStatus.Skipped,
                _ => NodeBackupStatus.Exception
            };
        }
        public static ObjectStatus ConvertToObjectStatus(ReportStatus status)
        {
            return status switch
            {
                ReportStatus.Success => ObjectStatus.Succeed,
                ReportStatus.Skipped => ObjectStatus.Skipped,
                ReportStatus.Failed => ObjectStatus.Failed,
                ReportStatus.Warn => ObjectStatus.Warned,
                _ => ObjectStatus.Skipped
                // _=> throw new NotSupportedException($"Unsupported status:{status}")
            };
        }
        public static ObjectLevel ConvertToObjcetLevel(string type)
        {
            return ConvertToObjcetLevel(type?.FirstOrDefault() ?? ' ');
        }

        public static ObjectLevel ConvertToObjcetLevel(Char type)
        {
            return type switch
            {
                ReportNodeHeader.Mailbox => ObjectLevel.Mailbox,
                ReportNodeHeader.Group => ObjectLevel.Group,
                ReportNodeHeader.Folder => ObjectLevel.Folder,
                ReportNodeHeader.Item => ObjectLevel.Item,
                ReportNodeHeader.Team => ObjectLevel.Team,
                ReportNodeHeader.Channel => ObjectLevel.Channel,
                ReportNodeHeader.Conversation => ObjectLevel.Conversation,
                ReportNodeHeader.Plan => ObjectLevel.PlannerPlan,
                ReportNodeHeader.Task => ObjectLevel.PlannerTask,
                ReportNodeHeader.Event => ObjectLevel.Event,
                ReportNodeHeader.Attachment => ObjectLevel.Attachment,
                ReportNodeHeader.Email => ObjectLevel.Email,
                ReportNodeHeader.Document => ObjectLevel.Document,
                ReportNodeHeader.DocumentVersion => ObjectLevel.DocumentVersion,
                ReportNodeHeader.SiteCollection => ObjectLevel.SiteCollection,
                ReportNodeHeader.Web => ObjectLevel.Site,
                ReportNodeHeader.List => ObjectLevel.List,
                ReportNodeHeader.SiteFolder => ObjectLevel.SiteFolder,
                ReportNodeHeader.SiteAttachment => ObjectLevel.SiteAttachment,
                _ => ObjectLevel.None
            };
        }

        public static string ConvertTeamsObjectLevelToI18N(Char type)
        {
            var objectLevel = ConvertToObjcetLevel(type);
            string I18nStr = objectLevel switch
            {
                ObjectLevel.Group or ObjectLevel.Team => "RM_Archiver_JobDetailTeamsGroupLevel",
                ObjectLevel.Folder or ObjectLevel.Channel => "RM_Archiver_JobDetailChannelLevel",
                ObjectLevel.Conversation => "RM_Archiver_JobDetailChannelConversationLevel",
                ObjectLevel.Mailbox => "RM_Archiver_JobDetailGroupMailboxLevel",
                ObjectLevel.Item => "RM_Archiver_JobDetailGroupMailboxItemLevel",
                ObjectLevel.PlannerPlan => "RM_Archiver_JobDetailPlanLevel",
                ObjectLevel.PlannerTask => "RM_Archiver_JobDetailTaskLevel",
                ObjectLevel.Event => "RM_Archiver_JobDetailEventLevel",
                ObjectLevel.Email => "RM_Archiver_JobDetailConversationLevel",
                ObjectLevel.Attachment => "RM_JS_Rule_ObjectLevel_Attachment",
                ObjectLevel.Document => "RM_JS_Rule_ObjectLevel_Document",
                ObjectLevel.DocumentVersion => "RM_JS_Rule_ObjectLevel_DocumentVersion",
                ObjectLevel.SiteCollection => "RM_JS_Rule_ObjectLevel_SiteCollection",
                ObjectLevel.Site => "RM_JS_Rule_ObjectLevel_Site",
                ObjectLevel.List => "RM_JS_Rule_ObjectLevel_List",
                ObjectLevel.SiteFolder => "RM_JS_Rule_ObjectLevel_Folder",
                ObjectLevel.SiteAttachment => "StorageOptimization.Gui_Attachment",
                _ => "RM_Archiver_JobDetailExceptionLevel",
            };
            return I18nStr;
        }

        public static string ConvertToObjcetType(string type)
        {
            return ConvertToObjcetType(type?.ToCharArray().FirstOrDefault() ?? ' ');
        }
        public static string ConvertToObjcetType(char type)
        {
            return type switch
            {
                ReportNodeHeader.Mailbox => "MailBox",
                ReportNodeHeader.Group => "G",
                ReportNodeHeader.Folder => "Folder",
                ReportNodeHeader.Item => "Item",
                ReportNodeHeader.Team => "Team",
                ReportNodeHeader.Channel => "Channel",
                ReportNodeHeader.Conversation => "Conversation",
                ReportNodeHeader.Plan => "Plan",
                ReportNodeHeader.Task => "Task",
                _ => type.ToString()
            };
        }

        public static ObjectCategory GetObjectCategory(ObjectStatus status, ObjectLevel level)
        {
            return Enum.TryParse<ObjectCategory>($"{status}{level}", true, out var result)
                ? result
                : throw new NotSupportedException($"Unsupported category: {$"{status},{level}"}");
        }

        public static ObjectLevel GetTopLevel(BackupModule module)
        {
            return module switch
            {
                BackupModule.YammerGroup => ObjectLevel.Group,
                BackupModule.Office365Group => ObjectLevel.Group,
                BackupModule.Teams => ObjectLevel.Team,
                _ => ObjectLevel.Group
            };
        }

        public static string ConvertStatisticsLevelToI18n(StatisticsLevel statisticsLevel)
        {
            var I18nStr = string.Empty;
            switch (statisticsLevel)
            {
                case StatisticsLevel.None:
                    break;
                case StatisticsLevel.TeamsGroup:
                    I18nStr = "RM_Archiver_JobDetailTeamsGroupLevel";
                    break;
                case StatisticsLevel.Channel:
                    I18nStr = "RM_Archiver_JobDetailChannelLevel";
                    break;
                case StatisticsLevel.ChannelConversation:
                    I18nStr = "RM_Archiver_JobDetailChannelConversationLevel";
                    break;
                case StatisticsLevel.GroupMailbox:
                    I18nStr = "RM_Archiver_JobDetailGroupMailboxLevel";
                    break;
                case StatisticsLevel.GroupMailboxItem:
                    I18nStr = "RM_Archiver_JobDetailGroupMailboxItemLevel";
                    break;
                case StatisticsLevel.SiteCollection:
                    I18nStr = "RM_JS_Rule_ObjectLevel_SiteCollection";
                    break;
                case StatisticsLevel.Site:
                    I18nStr = "RM_JS_Rule_ObjectLevel_Site";
                    break;
                case StatisticsLevel.List:
                    I18nStr = "RM_JS_Rule_ObjectLevel_List";
                    break;
                case StatisticsLevel.Folder:
                    I18nStr = "RM_JS_Rule_ObjectLevel_Folder";
                    break;
                case StatisticsLevel.Item:
                    I18nStr = "RM_JS_Rule_ObjectLevel_Item";
                    break;
                case StatisticsLevel.Plan:
                    I18nStr = "RM_Archiver_JobDetailPlanLevel";
                    break;
                case StatisticsLevel.Task:
                    I18nStr = "RM_Archiver_JobDetailTaskLevel";
                    break;
                case StatisticsLevel.Attachment:
                    I18nStr = "RM_JS_Rule_ObjectLevel_Attachment";
                    break;
                case StatisticsLevel.Exception:
                    I18nStr = "RM_Archiver_JobDetailExceptionLevel";
                    break;
                default:
                    break;
            }

            return I18nStr;
        }
    }

    public enum StatisticsLevel
    {
        None = 0,

        // Teams-related
        TeamsGroup = 1,
        Channel = 2,
        ChannelConversation = 3,

        // Mail-related
        GroupMailbox = 10,
        GroupMailboxItem = 11,

        // SharePoint-related
        SiteCollection = 20,
        Site = 21,
        List = 22,
        Folder = 23,
        Item = 24,
        Document = 25,
        DocumentVersion = 26,

        // Planner-related
        Plan = 30,
        Task = 31,
        Attachment = 32,

        // others
        Exception = 1000,
    }
}
