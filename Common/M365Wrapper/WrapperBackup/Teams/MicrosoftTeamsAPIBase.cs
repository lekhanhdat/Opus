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

namespace ExchangeUtility.Graph
{
    using AvePoint.GCommon.GraphAPI;

    using ExchangeCommonWrapper;

    using AvePoint.RA.CommonUtil;

    using System;
    using System.Collections.Generic;

    public abstract class MicrosoftTeamsAPIBase : IDisposable
    {
        protected static RALogger logger = RALogger.GetInstance(typeof(MicrosoftTeamsAPIBase));

        public IAuthObject AuthObject { get; private set; }

        public MicrosoftTeamsAPIBase(IAuthObject authObj)
        {
            this.AuthObject = authObj;
        }

        [AccessTokenPermission(AccessTokenPermissionType.Delegate)]
        public abstract GraphUser GetMe();

        #region Team
        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract TeamInfo GetTeamsMailById(string groupId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void UpdateTeam(string groupId, string displayName, string description, string alias, string classification, string accessType, string preferredDataLocation, MicrosoftTeamsEntity msteamEntity);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract TeamInfo CreateTeam(string displayName, string description, string alias, string classification, string accessType, MicrosoftTeamsEntity msteamEntity, string member = null, string dataLocation = null);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract Tuple<Group, TeamObj> CreateTeam(string displayName, string description, string alias, string classification, string accessType, string currentUser, string dataLocation = null);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void CreateTeamFromGroup(string groupId, MicrosoftTeamsEntity msteamEntity);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract bool IsTeamExist(string groupId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract bool UnarchiveTeam(string groupId);

        /// <summary> The shouldSetSpoSiteReadOnlyForMembers parameter is not supported in the application context. </summary>
        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract bool ArchiveTeam(string groupId, bool makeSiteReadOnly);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void SetTeam(string groupId, string displayName, string description, string alias, string classification, string accessType, string preferredDataLocation);

        [AccessTokenPermission(AccessTokenPermissionType.None)]
        public abstract bool RemoveTeam(string groupId);
        [AccessTokenPermission(AccessTokenPermissionType.Application)]
        public abstract void CompleteChannelMigration(string teamsId, string channelId);
        [AccessTokenPermission(AccessTokenPermissionType.Application)]
        public abstract void CompleteTeamsMigration(string teamsId);
        #endregion

        #region Team Settings&Channel

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract MicrosoftTeamsEntity GetTeamSettings(string teamGroupId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract string GetTeamIntenalId(string teamGroupId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void UpdateTeamsSetting(string groupId, MicrosoftTeamsEntity msteamEntity);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract List<TeamChannel> ListChannels(string groupId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract List<TeamChannel> ListIncomingChannels(string groupId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract List<TeamChannel> ListAllChannels(string groupId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract List<TeamChannel> ListTeamChanneslWithDetails(string groupId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract List<TeamChannel> LoadTeamChannels(string groupId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract string CreateTeamChannel(string groupId, string displayName, string description, string createdDateTime = null);
        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract string CreatePrivateChannel(string teamId, string displayName, string description, string channelOwnerId = null);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract string CreateSharedChannel(string teamId, string displayName, string description, string channelOwnerId = null);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void SetTeamChannel(string groupId, string channelId, string newDisPlayName, string description);

        [AccessTokenPermission(AccessTokenPermissionType.None)]
        public abstract void RemoveTeamChannel(string groupId, string channelId);

        [AccessTokenPermission(AccessTokenPermissionType.None)]
        public abstract bool SetTeamPicture(string groupId, string filePath);

        #endregion

        #region Team membership

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract List<TeamMember> GetTeamMembersByGroup(string groupId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract List<TeamMember> GetTeamMembers(string groupId, string email = null);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void AddTeamMemberByGroup(string groupId, TeamMember teamMember);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract TeamMember AddTeamMember(string groupId, TeamMember teamMember, bool useUserId = true);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void AddTeamMembersAndOwners(string groupId, MicrosoftTeamsEntity msteamEntity, bool isMembershipDynamic = false);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void RemoveTeamMember(string teamId, string membershipId);

        public abstract string GetTeamSiteUrl(string groupId);

        public abstract string GetTeamSiteDocLibUrl(string groupId);

        #endregion

        #region Team Apps & Tabs

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract List<TeamsApp> GetTeamApps(string groupId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void AddTeamApps(string groupId, List<TeamsApp> teamsApps);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void AddTeamsApp(string groupId, TeamsApp teamsApp);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void RemoveTeamsApps(string groupId, List<string> teamsAppIds);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void RemoveTeamsApp(string groupId, string teamsAppId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract List<CatalogApp> GetCataLogTeamApps();

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract List<ChannelTab> GetChannelTabs(string groupId, string channelId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract List<ChannelTab> GetChatTabs(string chatId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract ChannelTab GetChannelTab(string groupId, string channelId, string tabId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void AddChannelTabs(string groupId, string channelId, List<RestoreTab> channelTabs);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract string AddChannelTab(string groupId, string channelId, RestoreTab channelTab);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void UpdateChannelTab(string groupId, string channelId, RestoreTab channelTab);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void UpdateChannelTabConfig(string groupId, string channelId, string tabId, RestoreTab restoreTab);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void DeleteChannelTab(string groupId, string channelId, string tabId);

        #endregion

        #region Channel Member
        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract List<ExchangeCommonWrapper.ChannelMember> ListChannelMembers(string groupId, string channelId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract ChannelMember AddChannelMember(string groupId, string channelId, ChannelMember member, bool useUserId = true);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void RemoveChannelMember(string teamId, string channelId, string membershipId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract void UpdateChannelMemberRoles(string teamId, string channelId, ExchangeCommonWrapper.ChannelMember member);
        #endregion

        #region Channel Message
        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract List<TeamChatMessage> QueryChannelMessagesDelta(string groupId, string channelId, ref string deltatoken);
        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract bool QueryChannelMessagesDelta(string groupId, string channelId, ref string queryToken, out List<TeamChatMessage> chatMassages);
        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract string TrackingDeltaToken(string teamId, string channelId, params string[] queryParameters);
        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract bool ListChannelMessages(string groupId, string channelId, int pageSize, ref string skipToken, out List<TeamChatMessage> chatMassages);
        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract List<TeamChatMessage> ListChannelAllMessages(string groupId, string channelId);
        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract TeamChatMessage GetChannelMessage(string groupId, string channelId, string messageId);
        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract List<TeamChatMessage> ListChannelMessageAllReplies(string groupId, string channelId, string messageId);
        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract bool ListChannelMessageReplies(string groupId, string channelId, string messageId, int pageSize, ref string skipToken, out List<TeamChatMessage> chatMassages);
        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract TeamChatMessage GetChannelMessageReply(string groupId, string channelId, string messagesId, string replyId);
        [AccessTokenPermission(AccessTokenPermissionType.Delegate)]
        public abstract string SendChannelMessage(string groupId, string channelId, TeamChatMessage message);
        [AccessTokenPermission(AccessTokenPermissionType.Delegate)]
        public abstract string ReplyChannelMessage(string groupId, string channelId, string messageId, TeamChatMessage message);
        #endregion
        #region Channel FIles
        public abstract string GetChannelFilesUrl(string groupId, string channelId);
        #endregion

        #region Chat

        [AccessTokenPermission(AccessTokenPermissionType.Application)]
        public abstract IEnumerable<TeamChatMessage> GetChatMessages(string userId, string startTime, string endTime, string? model, int? top);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract IEnumerable<TeamChatMessage> GetChatMessagesInChat(string userId, string chatId, string startTime, string endTime, int? top, bool isGcc = false);


        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract TeamChatMessage GetChatMessage(string chatId, string messageId);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract IEnumerable<ChatEntity> GetChats(string userId, int? top, bool isNoSort = false);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract ChatEntity GetChat(string chatId);

        #endregion

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract byte[] GetHostedContentAsByte(string url);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract string GetHostedContentAsString(string url);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract Dictionary<string, string> BatchGetHostedContentsAsString(Dictionary<string, string> requests, bool useBetaApi);

        [AccessTokenPermission(AccessTokenPermissionType.Delegate)]
        public abstract EventEntity GetEvent(string groupId, string eventId);

        [AccessTokenPermission(AccessTokenPermissionType.Delegate)]
        public abstract ExchangeCommonWrapper.User GetUser(string idOrUserPrincipalName);

        [AccessTokenPermission(AccessTokenPermissionType.All)]
        public abstract IEnumerable<ExchangeCommonWrapper.LicenseDetails> GetLicenseDetails(string userId);

        public virtual void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        { }
    }

    internal class AccessTokenPermission : Attribute
    {
        public AccessTokenPermissionType Type { get; set; }

        public AccessTokenPermission(AccessTokenPermissionType type)
        {
            this.Type = type;
        }
    }

    internal enum AccessTokenPermissionType
    {
        None = 0,
        Delegate = 1,
        Application = 2,
        All = 3,
    }

    public class TeamInfo
    {
        public string GroupId { get; set; }

        public string Mail { get; set; }
    }

    public class PlannerTabUpdateObj
    {
        public string ChannelId { get; set; }

        public string TabId { get; set; }

        public string PlannerId { get; set; }

        public ChannelTab ChannelTab { get; set; }
    }

    public class FileTabUpdateObj
    {
        public string ChannelId { get; set; }

        public string TabId { get; set; }

        public string EntityId { get; set; }

        //public ChannelTab ChannelTab { get; set; }

        public RestoreTab RestoreTab { get; set; }
    }
}