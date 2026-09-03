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

using System.Collections.Generic;

namespace AvePoint.GCommon.Contract.CoverageReport
{
    public class BackupSummaryDto
    {
        public long RunTimeInSeconds { get; set; }
        public long BackedUpDataSize { get; set; }
        public Dictionary<ObjectCategory, long> ObjectCountDetails { get; set; } = new();
        public long TotalObjectCount { get; set; }
        public long SucceedObjectCount { get; set; }
        public long FailedObjectCount { get; set; }
        public long SkippedObjectCount { get; set; }
        public long WarnedObjectCount { get; set; }

        /// <summary>
        /// For Office365Group, Teams and YammerGroup modules only, to record the runtime of each subjob.
        /// The value is handled internally to convert the Dictionary<string, long> to json, key is subjob id.
        /// </summary>
        public string RunTimeSeparately { get; set; }

        /// <summary>
        /// For Office365Group, Teams and YammerGroup modules only, to record the backed up data size of each subjob.
        /// The value is handled internally to convert the Dictionary<string, long> to json, key is subjob id.
        /// </summary>
        public string BackedUpDataSizeSeparately { get; set; }

        /// <summary>
        /// For Office365Group, Teams and YammerGroup modules only, to record the succeed object count of each subjob.
        /// The value is handled internally to convert the Dictionary<string, long> to json, key is subjob id.
        /// </summary>
        public string SucceedObjectCountSeparately { get; set; }
    }

    public enum ObjectStatus
    {
        Succeed,
        Failed,
        Skipped,
        Warned
    }

    public enum NodeBackupStatus
    {
        All = -1,
        Skipped = 0,
        Successd = 1,
        Failed = 2,
        Exception = 6
    }

    public enum ObjectCategory
    {
        #region SiteCollection
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.SiteCollection)]
        SucceedSiteCollection = 1,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.SiteCollection)]
        FailedSiteCollection = 2,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.SiteCollection)]
        SkippedSiteCollection = 3,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.SiteCollection)]
        WarnedSiteCollection = 4,
        #endregion

        #region Site
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.Site)]
        SucceedSite = 11,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.Site)]
        FailedSite = 12,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.Site)]
        SkippedSite = 13,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.Site)]
        WarnedSite = 14,
        #endregion

        #region App
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.App)]
        SucceedApp = 21,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.App)]
        FailedApp = 22,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.App)]
        SkippedApp = 23,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.App)]
        WarnedApp = 24,
        #endregion

        #region List
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.List)]
        SucceedList = 31,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.List)]
        FailedList = 32,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.List)]
        SkippedList = 33,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.List)]
        WarnedList = 34,
        #endregion

        #region ProjectOnline
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.ProjectOnline)]
        SucceedProjectOnline = 41,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.ProjectOnline)]
        FailedProjectOnline = 42,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.ProjectOnline)]
        SkippedProjectOnline = 43,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.ProjectOnline)]
        WarnedProjectOnline = 44,
        #endregion

        #region Folder
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.Folder)]
        SucceedFolder = 51,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.Folder)]
        FailedFolder = 52,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.Folder)]
        SkippedFolder = 53,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.Folder)]
        WarnedFolder = 54,
        #endregion

        #region Item
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.Item)]
        SucceedItem = 61,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.Item)]
        FailedItem = 62,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.Item)]
        SkippedItem = 63,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.Item)]
        WarnedItem = 64,
        #endregion

        #region Attachment
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.Attachment)]
        SucceedAttachment = 71,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.Attachment)]
        FailedAttachment = 72,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.Attachment)]
        SkippedAttachment = 73,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.Attachment)]
        WarnedAttachment = 74,
        #endregion

        #region Mailbox
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.Mailbox)]
        SucceedMailbox = 81,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.Mailbox)]
        FailedMailbox = 82,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.Mailbox)]
        SkippedMailbox = 83,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.Mailbox)]
        WarnedMailbox = 84,
        #endregion

        #region Group
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.Group)]
        SucceedGroup = 91,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.Group)]
        FailedGroup = 92,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.Group)]
        SkippedGroup = 93,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.Group)]
        WarnedGroup = 94,

        #endregion

        #region GroupConversation
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.GroupConversation)]
        SucceedGroupConversation = 101,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.GroupConversation)]
        FailedGroupConversation = 102,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.GroupConversation)]
        SkippedGroupConversation = 103,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.GroupConversation)]
        WarnedGroupConversation = 104,
        #endregion

        #region Meeting
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.Meeting)]
        SucceedMeeting = 111,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.Meeting)]
        FailedMeeting = 112,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.Meeting)]
        SkippedMeeting = 113,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.Meeting)]
        WarnedMeeting = 114,
        #endregion

        #region Team
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.Team)]
        SucceedTeam = 121,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.Team)]
        FailedTeam = 122,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.Team)]
        SkippedTeam = 123,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.Team)]
        WarnedTeam = 124,
        #endregion

        #region Channel
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.Channel)]
        SucceedChannel = 131,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.Channel)]
        FailedChannel = 132,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.Channel)]
        SkippedChannel = 133,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.Channel)]
        WarnedChannel = 134,
        #endregion

        #region Conversation
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.Conversation)]
        SucceedConversation = 141,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.Conversation)]
        FailedConversation = 142,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.Conversation)]
        SkippedConversation = 143,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.Conversation)]
        WarnedConversation = 144,
        #endregion

        #region PlannerPlan
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.PlannerPlan)]
        SucceedPlannerPlan = 151,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.PlannerPlan)]
        FailedPlannerPlan = 152,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.PlannerPlan)]
        SkippedPlannerPlan = 153,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.PlannerPlan)]
        WarnedPlannerPlan = 154,
        #endregion

        #region PlannerTask
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.PlannerTask)]
        SucceedPlannerTask = 161,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.PlannerTask)]
        FailedPlannerTask = 162,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.PlannerTask)]
        SkippedPlannerTask = 163,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.PlannerTask)]
        WarnedPlannerTask = 164,
        #endregion

        #region Workspace
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.Workspace)]
        SucceedWorkspace = 171,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.Workspace)]
        FailedWorkspace = 172,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.Workspace)]
        SkippedWorkspace = 173,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.Workspace)]
        WarnedWorkspace = 174,
        #endregion

        #region Report
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.Report)]
        SucceedReport = 181,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.Report)]
        FailedReport = 182,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.Report)]
        SkippedReport = 183,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.Report)]
        WarnedReport = 184,
        #endregion

        #region PowerApps
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.PowerApps)]
        SucceedPowerApps = 191,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.PowerApps)]
        FailedPowerApps = 192,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.PowerApps)]
        SkippedPowerApps = 193,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.PowerApps)]
        WarnedPowerApps = 194,
        #endregion

        #region Flow
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.Flow)]
        SucceedFlow = 201,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.Flow)]
        FailedFlow = 202,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.Flow)]
        SkippedFlow = 203,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.Flow)]
        WarnedFlow = 204,
        #endregion

        #region User
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.User)]
        SucceedUser = 211,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.User)]
        FailedUser = 212,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.User)]
        SkippedUser = 213,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.User)]
        WarnedUser = 214,
        #endregion

        #region Chat
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.Chat)]
        SucceedChat = 221,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.Chat)]
        FailedChat = 222,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.Chat)]
        SkippedChat = 223,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.Chat)]
        WarnedChat = 224,
        #endregion

        #region Message
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.Message)]
        SucceedMessage = 231,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.Message)]
        FailedMessage = 232,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.Message)]
        SkippedMessage = 233,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.Message)]
        WarnedMessage = 234,
        #endregion

        #region Conversations
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.Conversations)]
        SucceedConversations = 241,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.Conversations)]
        FailedConversations = 242,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.Conversations)]
        SkippedConversations = 243,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.Conversations)]
        WarnedConversations = 244,
        #endregion

        #region YammerGroup
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.YammerGroup)]
        SucceedYammerGroup = 251,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.YammerGroup)]
        FailedYammerGroup = 252,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.YammerGroup)]
        SkippedYammerGroup = 253,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.YammerGroup)]
        WarnedYammerGroup = 254,
        #endregion

        #region FolderInExchange
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.FolderInExchange)]
        SucceedFolderInExchange = 261,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.FolderInExchange)]
        FailedFolderInExchange = 262,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.FolderInExchange)]
        SkippedFolderInExchange = 263,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.FolderInExchange)]
        WarnedFolderInExchange = 264,
        #endregion

        #region ItemInExchange
        [Category(Status = ObjectStatus.Succeed, Level = ObjectLevel.ItemInExchange)]
        SucceedItemInExchange = 271,

        [Category(Status = ObjectStatus.Failed, Level = ObjectLevel.ItemInExchange)]
        FailedItemInExchange = 272,

        [Category(Status = ObjectStatus.Skipped, Level = ObjectLevel.ItemInExchange)]
        SkippedItemInExchange = 273,

        [Category(Status = ObjectStatus.Warned, Level = ObjectLevel.ItemInExchange)]
        WarnedItemInExchange = 274,
        #endregion
    }

    public enum ObjectLevel
    {
        None = 0,
        SiteCollection,
        Site,
        App,
        List,
        ProjectOnline,
        Folder,
        Item,
        Attachment,
        Mailbox,
        Group,
        GroupConversation,
        Meeting,
        Team,
        Channel,
        Conversation,
        PlannerPlan,
        PlannerTask,
        Workspace,
        Report,
        PowerApps,
        Flow,
        User,
        Chat,
        Message,
        Conversations,
        YammerGroup,
        FolderInExchange,
        ItemInExchange,
        Email,
        Event,
        Document,
        DocumentVersion,
        SiteFolder,
        SiteAttachment,
    }

}