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
    using M365.Wrapper.Backup.Auth.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;

    public class MicrosoftTeamsWithGraph : MicrosoftTeamsAPIBase
    {
        private MicrosoftGraphAPIService graphClient;

        public MicrosoftTeamsWithGraph(IAppTokenAuthObject authObj) : base(authObj)
        {
            graphClient = new MicrosoftGraphAPIService(
                authObj.ResourceUrl,
                authObj.GetAccessToken,
                new GraphLogger(),
                authObj.AuthType != AuthObjectType.AccessToken);
            graphClient.RetryController = new GraphAPIRetry();
        }

        public override GraphUser GetMe()
        {
            try
            {
                return graphClient.Me;
            }
            catch { return null; }
        }

        #region Team

        public override Tuple<Group, TeamObj> CreateTeam(string displayName, string description, string alias, string classification, string accessType, string currentUser, string dataLocation = null)
        {
            if (!string.IsNullOrEmpty(dataLocation))
            {
                logger.Info("The dataLocation and the current user of restoring team is: {0}, {1}.", dataLocation, currentUser);
                return this.graphClient.CreateTeam(new Group()
                {
                    DisplayName = displayName,
                    Description = description,
                    Classification = classification,//todo:qlluo: ps do not add classification //AOSBR-16751 GCCH 支持该属性
                    MailNickname = alias,
                    Visibility = accessType,
                    PreferredDataLocation = dataLocation,
                }, currentUser);
            }
            else
            {
                logger.Info("The current user of restoring team is: {0}.", currentUser);
                var result = this.graphClient.CreateTeam(new Group()
                {
                    DisplayName = displayName,
                    Description = description,
                    Classification = classification,//todo:qlluo: ps do not add classification //AOSBR-16751 GCCH 支持该属性
                    MailNickname = alias,
                    Visibility = accessType,
                }, currentUser);
                return result;
            }
        }

        public override bool IsTeamExist(string groupId)
        {
            try
            {
                return null != this.graphClient.GetTeamPrimaryChannel(groupId);
            }
            catch (GraphAPIException ex)
            {
                if (ex.Error.Message.Contains("User Login. Teams is disabled in user licenses")) throw;
                return false;
            }
        }

        /// <summary> The shouldSetSpoSiteReadOnlyForMembers parameter is not supported in the application context. </summary>
        public override bool ArchiveTeam(string groupId, bool makeSiteReadOnly)
        {
            try
            {
                this.graphClient.ArchiveTeam(groupId, makeSiteReadOnly);
                //Due to Archive team has delay, sleep 30s to wait.
                Thread.Sleep(30 * 1000);
                return true;
            }
            catch (GraphAPIException ex)
            {
                logger.Error("An error occurred while to archive team. GroupId: {0}. Reason: {1}", groupId, ex);
                return false;
            }
        }

        public override bool UnarchiveTeam(string groupId)
        {
            try
            {
                this.graphClient.UnarchiveTeam(groupId);
                //Due to Unarchive team has delay, sleep 30s to wait.
                Thread.Sleep(30 * 1000);
                return true;
            }
            catch (GraphAPIException ex)
            {
                logger.Error("An error occurred while to unarchive team. GroupId: {0}. Reason: {1}", groupId, ex);
                return false;
            }
        }

        public override void CreateTeamFromGroup(string groupId, MicrosoftTeamsEntity msteamEntity)
        {
            this.graphClient.CreateTeam(groupId);
            if (!string.IsNullOrEmpty(groupId))
            {
                UpdateTeamSettingsInternal(msteamEntity, groupId);
            }
        }

        public override TeamInfo CreateTeam(string displayName, string description, string alias, string classification, string accessType, MicrosoftTeamsEntity msteamEntity, string member = null, string dataLocation = null)
        {
            var currentUserId = GetAvailableTeamUserIdOrDefault(msteamEntity, member);
            var result = CreateTeam(displayName, description, alias, classification, accessType, currentUserId, dataLocation);
            string groupAddress = string.Empty;
            var groupId = result.Item1.Id;
            if (!string.IsNullOrEmpty(groupId))
            {
                logger.Info("Create Group Address: {0}.", result.Item1.Mail);
                Thread.Sleep(20000);
                AvePoint.Wrapper.Common.AveTaskRetryHelper helper = new(5, true, 5000);
                helper.ExecuteWithRetryMechanismV3(() =>
                {
                    groupAddress = this.graphClient.GetGroupInfoById(groupId).Mail;
                });
                logger.Info("ReGet Group Address: {0}.", groupAddress);
                UpdateTeamSettingsInternal(msteamEntity, groupId);
            }
            return new TeamInfo() { GroupId = groupId, Mail = groupAddress };
        }

        private string GetAvailableTeamUserIdOrDefault(MicrosoftTeamsEntity msteamEntity, string member)
        {
            string user = null;
            if (msteamEntity.TeamMembers.FirstOrDefault(m => m.RoleType is TeamMemberRoleType.Owner && TryEnsureUser(m.UserId, m.MailboxAddress, ref user)) is not null)
            {
                return user;
            }
            if (msteamEntity.TeamMembers.FirstOrDefault(m => m.RoleType is TeamMemberRoleType.Member && TryEnsureUser(m.UserId, m.MailboxAddress, ref user)) is not null)
            {
                return user;
            }
            return member.IsNotNullOrEmpty() ? graphClient.GetUser(member).Id : throw new Exception("No available user to create team.");

            bool TryEnsureUser(string id, string upn, ref string user)
            {
                try
                {
                    user = EnsureUser(id, upn, false);
                    return true;
                }
                catch (Exception ex)
                {
                    logger.Warn("Failed to get team member: {0}.", ex);
                    return false;
                }
            }
        }

        public override void SetTeam(string groupId, string displayName, string description, string alias, string classification, string accessType, string preferredDataLocation)
        {
            logger.Info("Start updating team-group infomation.");
            //classification is not support to update
            this.graphClient.UpdateGroup(new Group()
            {
                Id = groupId,
                DisplayName = displayName,
                Description = description,
                //todo:qlluo: ps impl do not update nickname and classification
                //MailNickname =alias,
                Classification = classification, //AOSBR-16751 GCCH 支持该属性
                Visibility = accessType,
                PreferredDataLocation = preferredDataLocation
            });
        }

        public override void UpdateTeam(string groupId, string displayName, string description, string alias, string classification, string accessType, string preferredDataLocation, MicrosoftTeamsEntity msteamEntity)
        {
            try
            {
                this.SetTeam(groupId, displayName, description, alias, classification, accessType, preferredDataLocation);
                this.UpdateTeamsSetting(groupId, msteamEntity);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while to update microsoft teams entity. Reason: ", ex.ToString());
            }
        }

        //impl later
        public override bool RemoveTeam(string groupId)
        {
            throw new NotImplementedException();
        }

        public override MicrosoftTeamsEntity GetTeamSettings(string teamGroupId)
        {
            var team = this.graphClient.GetTeam(teamGroupId);
            var teamM = team.ToM();
            return teamM;
        }

        public override string GetTeamIntenalId(string teamGroupId)
        {
            try
            {
                return this.graphClient.GetTeamPrimaryChannel(teamGroupId).Id;
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while to get team intenal id. Reason :{0}", ex.ToString());
                return null;
            }
        }

        public override string GetTeamSiteUrl(string groupId)
        {
            return this.graphClient.GetGroupRootSite(groupId)?.WebUrl;
        }

        public override string GetTeamSiteDocLibUrl(string groupId)
        {
            return this.graphClient.GetGroupDrive(groupId)?.WebUrl;
        }

        //impl later
        public override bool SetTeamPicture(string groupId, string filePath)
        {
            throw new NotImplementedException();
        }

        public override void UpdateTeamsSetting(string groupId, MicrosoftTeamsEntity msteamEntity)
        {
            try
            {
                logger.Info("Start updating team settings.");
                this.graphClient.UpdateTeam(groupId, msteamEntity.ToG());
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while to update microsoft teams setting. Reason: ", ex.ToString());
            }
        }

        public override void AddTeamMembersAndOwners(string groupId, MicrosoftTeamsEntity msteamEntity, bool isMembershipDynamic = false) =>
            msteamEntity.TeamMembers?.ForEach(member =>
            {
                try
                {
                    if (isMembershipDynamic && member.RoleType == TeamMemberRoleType.Member)
                    {
                        logger.Warn("Membership is dynamic type so skip adding member: {0}", member.MailboxAddress);
                        return;
                    }
                    AddTeamMember(groupId, member);
                }
                catch (Exception ex)
                {
                    logger.Warn("Failed to add team member: {0}, error: {1}.", member.MailboxAddress, ex);
                }
            });

        private void UpdateTeamSettingsInternal(MicrosoftTeamsEntity msteamEntity, string groupId)
        {
            UpdateTeamsSetting(groupId, msteamEntity);
            AddTeamMembersAndOwners(groupId, msteamEntity);
        }

        public override void CompleteChannelMigration(string teamsId, string channelId)
        {
            this.graphClient.CompleteChannelMigration(teamsId, channelId);
        }

        public override void CompleteTeamsMigration(string teamsId)
        {
            this.graphClient.CompleteTeamsMigration(teamsId);
        }

        #endregion

        #region TeamMember

        public override void AddTeamMemberByGroup(string groupId, TeamMember teamMember)
        {
            logger.Info($"Add team member, groupId: {groupId}, userName: {teamMember.MailboxAddress}, type: {teamMember.RoleType}");
            var userId = EnsureUser(teamMember.UserId, teamMember.MailboxAddress, teamMember.RoleType == TeamMemberRoleType.Guest);
            AddTeamMemberInternal(groupId, userId, teamMember.RoleType);
        }

        public override TeamMember AddTeamMember(string groupId, TeamMember teamMember, bool useUserId = true)
        {
            logger.Info($"Add team member, groupId: {groupId}, userName: {teamMember.MailboxAddress}, type: {teamMember.RoleType}");
            var id = useUserId ? EnsureUser(teamMember.UserId, teamMember.MailboxAddress, teamMember.RoleType == TeamMemberRoleType.Guest) : teamMember.MailboxAddress;
            var member = new OTJChannelMember(teamMember.RoleType == TeamMemberRoleType.Owner ? new string[] { "owner" } : new string[] { }, graphClient.GenerateOdataBindString(id));
            return graphClient.AddTeamMember(groupId, member).ToTeamMemberM();
        }

        public override void RemoveTeamMember(string groupId, string membershipId)
        {
            graphClient.RemoveTeamMember(groupId, membershipId);
        }

        private void AddTeamMemberInternal(string groupId, string userId, TeamMemberRoleType roleType)
        {
            switch (roleType)
            {
                case TeamMemberRoleType.Owner:
                    graphClient.AddGroupOwner(groupId, userId);
                    break;
                case TeamMemberRoleType.Member:
                case TeamMemberRoleType.Guest:
                    graphClient.AddGroupMember(groupId, userId);
                    break;
            }
        }

        private string GetTeamLicenseUser(string userId, string upn)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    string[] selectProperties = { "id", "displayName", "mail", "userPrincipalName", "userType", "assignedPlans" };
                    var user = this.graphClient.GetUser(userId, selectProperties);
                    var hasTeamlicense = user?.AssignedPlans?.Any(asdPlan => asdPlan.Service.Equals("TeamspaceAPI", StringComparison.OrdinalIgnoreCase)
                    && asdPlan.CapabilityStatus.Equals("Enabled", StringComparison.OrdinalIgnoreCase)) ?? false;
                    if (hasTeamlicense)
                    {
                        var sb = new StringBuilder($"The Team licsence of user [{user.UserPrincipalName}]:\r\n");
                        foreach (var ap in user.AssignedPlans)
                        {
                            if ("TeamspaceAPI".Equals(ap.Service, StringComparison.OrdinalIgnoreCase)) sb.AppendFormat("[{0},{1},{2},{3}]\r\n", ap.ServicePlanId, ap.AssignedDateTime, ap.CapabilityStatus, ap.Service);
                        }
                        logger.Info(sb.ToString());
                        return user.Id;
                    }
                    throw new UserNotFoundException(new string[] { $"{upn}({userId})" });
                }
                catch (Exception ex)
                {
                    logger.Warn($"Failed to get user via id({userId}), try upn({upn}) instead. Error: {ex}");
                }
            }
            throw new UserNotFoundException(new string[] { $"{upn}({userId})" });

        }

        private string EnsureUser(string userId, string upn, bool external)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    return this.graphClient.GetUser(userId).Id;
                }
                catch (Exception ex)
                {
                    logger.Warn($"Failed to get user via id({userId}), try upn({upn}) instead. Error: {ex}");
                }
            }
            if (!string.IsNullOrEmpty(upn))
            {
                if (external)
                {
                    return this.graphClient.FindUser(upn).Id;
                }
                return this.graphClient.GetUser(upn).Id;
            }
            throw new UserNotFoundException(new string[] { $"{upn}({userId})" });
        }

        public override List<TeamMember> GetTeamMembersByGroup(string groupId)
        {
            var members = this.graphClient.ListGroupMembers(groupId);
            var owners = this.graphClient.ListGroupOwners(groupId);
            return owners.Select(o => o.ToTeamMemberM(true)).Concat(
                members.Select(o => o.ToTeamMemberM(false))).
                Distinct().
                ToList();
        }

        public override List<TeamMember> GetTeamMembers(string groupId, string email = null) => graphClient.GetTeamMembers(groupId, email).Select(o => o.ToTeamMemberM()).ToList();

        #endregion

        #region TeamChannel

        public override string CreateTeamChannel(string groupId, string displayName, string description, string createdDateTime = null)
        {
            var channel = this.graphClient.CreateChannel(groupId, new Channel() 
            { 
                DisplayName = displayName, 
                Description = description, 
                CreatedDateTime = createdDateTime, 
                CreationMode = createdDateTime.IsNotNullOrEmpty() ? "migration" : null 
            });
            if (channel == null) return string.Empty;
            else
            {
                if (createdDateTime.IsNotNullOrEmpty())
                {
                    logger.Info("Create channel [{0}] succeeded, newId is {1}, description is {2}, membershipType is {3}, CreatedDateTime is {4}, CreationMode is migration",
                        channel.DisplayName, channel.Id, channel.Description, channel.MembershipType, createdDateTime);
                }
                else
                {
                    logger.Info("Create channel [{0}] succeeded, newId is {1}, description is {2}, membershipType is {3}",
                        channel.DisplayName, channel.Id, channel.Description, channel.MembershipType);
                }
                return channel.Id;
            }
        }

        public override string CreatePrivateChannel(string teamId, string displayName, string description, string channelOwnerId = null)
        {
            var obj = new Channel() { DisplayName = displayName, Description = description, MembershipType = "private" };
            if (!string.IsNullOrEmpty(channelOwnerId)) obj.Members = new OTJChannelMember[] { new OTJChannelMember(new[] { "owner" }, graphClient.GenerateOdataBindString(channelOwnerId)), };
            try
            {
                var channel = this.graphClient.CreatePrivateChannel(teamId, obj);
                if (channel == null) return string.Empty;
                else
                {
                    logger.Info("Create channel [{0}] succeeded, newId is {1}, description is {2}, membershipType is {3}", channel.DisplayName, channel.Id, channel.Description, channel.MembershipType);
                    if (!channel.MembershipType.Equals("private", StringComparison.OrdinalIgnoreCase)) throw new Exception("The channel's MembershipType is not private as expected.");
                    return channel.Id;
                }
            }
            catch (GraphAPIException ex) when (ex.HttpStatusCode == HttpStatusCode.BadRequest)
            {
                var maxRetries = 12;
                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        var privateChannel = this.graphClient.GetPrivateChannel(teamId, displayName);
                        if (privateChannel != null)
                        {
                            logger.Info($"privateChannel:{displayName} created, id:{privateChannel.Id}");
                            if (!privateChannel.Id.IsNullOrEmpty())
                            {
                                return privateChannel.Id;
                            }
                        }
                        logger.Info($"retry get privateChannel:{displayName} retryCount:{attempt}");
                        System.Threading.Thread.Sleep(10 * 1000);
                    }
                    catch (Exception e)
                    {
                        logger.Info($"Failed to get private channel. channelName:{displayName}, ex:{e}");
                    }
                }

                throw;
            }
            
        }

        public override string CreateSharedChannel(string teamId, string displayName, string description, string channelOwnerId = null)
        {
            var obj = new Channel() { DisplayName = displayName, Description = description, MembershipType = "shared" };
            if (!string.IsNullOrEmpty(channelOwnerId)) obj.Members = new OTJChannelMember[] { new OTJChannelMember(new[] { "owner" }, graphClient.GenerateOdataBindString(channelOwnerId)), };
            var channel = this.graphClient.CreatePrivateChannel(teamId, obj);
            if (channel == null)//Creating a shared channel does not return the channel ID
            {

                var maxRetries = 12;
                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        var sharedChannel = this.graphClient.GetSharedChannel(teamId, displayName);
                        if (sharedChannel != null)
                        {
                            logger.Info($"sharedChannel:{displayName} created, id:{sharedChannel.Id}");
                            if (!sharedChannel.Id.IsNullOrEmpty())
                            {
                                return sharedChannel.Id;
                            }
                        }
                        logger.Info($"retry get sharedChannel:{displayName} retryCount:{attempt}");
                        System.Threading.Thread.Sleep(10 * 1000);
                    }
                    catch (Exception e)
                    {
                        logger.Info($"Failed to get shared channel. channelName:{displayName}, ex:{e}");
                    }
                }

                return this.graphClient.GetSharedChannel(teamId, displayName)?.Id ?? string.Empty;
            }
            else
            {
                logger.Info("Create channel [{0}] succeeded, newId is {1}, description is {2}, membershipType is {3}", channel.DisplayName, channel.Id, channel.Description, channel.MembershipType);
                if (!channel.MembershipType.Equals("shared", StringComparison.OrdinalIgnoreCase)) throw new Exception("The channel's MembershipType is not shared as expected.");
                return channel.Id;
            }
        }

        /// <summary>
        ///只包涵channel基础信息
        /// </summary>
        /// <param name="groupId"></param>
        public override List<TeamChannel> ListChannels(string groupId)
        {
            return this.graphClient.ListChannels(groupId).ToM().ToList();
        }

        public override List<TeamChannel> ListIncomingChannels(string groupId)
        {
            return this.graphClient.ListIncomingChannels(groupId).ToM(isIncomingChannels: true).ToList();
        }

        public override List<TeamChannel> ListAllChannels(string groupId)
        {
            return this.graphClient.ListAllChannels(groupId).ToM().ToList();
        }

        /// <summary>
        /// channel , channel tab, channel member
        /// </summary>
        public override List<TeamChannel> ListTeamChanneslWithDetails(string groupId)
        {
            var teamChannels = this.graphClient.ListChannels(groupId).ToM().ToList();
            teamChannels.ForEach(tc => tc.ChannelTabs = GetChannelTabs(groupId, tc.Id));
            teamChannels.Where(c => c.IsPrivateChannel() || c.IsSharedChannel()).ForEach(channel =>
                {
                    logger.Info("Start to get channel [{0}] members.", channel.DisplayName);
                    try
                    {
                        channel.ChannelMembers = ListChannelMembers(groupId, channel.Id);
                        var ownerCount = channel.ChannelMembers.Count(u => u.Roles.Contains("owner"));
                        logger.Info($"The channel users count is: {channel.ChannelMembers.Count} and owner count is: {ownerCount}.");
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Get channel members failed. Reason : {0}", ex.ToString());
                    }
                });
            return teamChannels;
        }

        public override List<TeamChannel> LoadTeamChannels(string groupId)
        {
            var teamChannels = this.graphClient.ListChannels(groupId).ToM().ToList();
            //foreach (var channel in teamChannels)//AOSBR-17995
            //{
            //    logger.Info($"Start to get channel files folder url. ChannelName: {channel.DisplayName}. ChannelId: {channel.Id}. ");
            //    try
            //    {
            //        channel.FilesFolderUrl = GetChannelFilesUrl(groupId, channel.Id);
            //        logger.Info($"Finish to get channel files folder url. Channel: {channel.DisplayName}. Url: { channel.FilesFolderUrl}. ");
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Error($"Failed to get channel files folder url. Channel: {channel.DisplayName}. Reason: {ex.ToString()}. ");
            //    }
            //}
            return teamChannels;
        }

        public override void SetTeamChannel(string groupId, string channelId, string newDisPlayName, string description)
        {
            this.graphClient.UpdateChannel(groupId, new Channel
            {
                Id = channelId,
                DisplayName = newDisPlayName,
                Description = description ?? String.Empty,
            });
        }

        public override string GetChannelFilesUrl(string groupId, string channelId)
        {
            return this.graphClient.GetChannelFilesFolder(groupId, channelId).WebUrl;
        }

        //impl later
        public override void RemoveTeamChannel(string groupId, string channelId)
        {
            this.graphClient.RemoveChannel(groupId, channelId);
        }

        #endregion

        #region TeamApp

        public override List<TeamsApp> GetTeamApps(string groupId)
        {
            try
            {
                return this.graphClient.ListInstalledApps(groupId).ToM().ToList();
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while to get team apps. Reason: {0}", ex);
                return new List<TeamsApp>();
            }
        }

        public override List<CatalogApp> GetCataLogTeamApps()
        {
            try
            {
                return this.graphClient.ListCatalogTeamsApps().ToM().ToList();
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while to get cataLog apps. Reason: {0}", ex);
                return new List<CatalogApp>();
            }
        }

        public override void AddTeamApps(string groupId, List<TeamsApp> teamsApps)
        {
            teamsApps.ForEach(tA => AddTeamsApp(groupId, tA));
        }

        public override void AddTeamsApp(string groupId, TeamsApp teamsApp)
        {
            this.graphClient.AddTeamsApp(groupId, new TeamsAppObj() { TeamsAppOdataBind = this.graphClient.BuildTeamsAppOdataBind(teamsApp.TeamsAppDefinition.TeamsAppId), });
        }

        public override void RemoveTeamsApps(string groupId, List<string> teamsAppIds)
        {
            teamsAppIds.ForEach(tAI => RemoveTeamsApp(groupId, tAI));
        }

        public override void RemoveTeamsApp(string groupId, string teamsAppId)
        {
            this.graphClient.DeleteTeamsApp(groupId, teamsAppId);
        }

        #endregion

        #region TeamTab

        public override List<ChannelTab> GetChannelTabs(string groupId, string channelId)
        {
            try
            {
                return this.graphClient.ListChannelTabs(groupId, channelId).ToM().ToList();
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while to get channel tabs. ChannelId: {0}. Reason: {1}", channelId, ex);
                return new List<ChannelTab>();
            }
        }

        public override List<ChannelTab> GetChatTabs(string chatId)
        {
            return graphClient.GetChatTabs(chatId).ToM().ToList();
        }

        public override ChannelTab GetChannelTab(string groupId, string channelId, string tabId)
        {
            try
            {
                return this.graphClient.GetChannelTab(groupId, channelId, tabId).ToM();
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while to get channel tab. ChannelId: {0}. TabId: {1}. Reason: {2}", channelId, tabId, ex);
                return new ChannelTab();
            }
        }

        public override void AddChannelTabs(string groupId, string channelId, List<RestoreTab> channelTabs)
        {
            channelTabs.ForEach(cT => AddChannelTab(groupId, channelId, cT));
        }

        public override string AddChannelTab(string groupId, string channelId, RestoreTab channelTab)
        {
            var tempTab = new TabAddObj() { DisplayName = channelTab.ChannelTab.DisplayName, TeamsAppOdataBind = graphClient.BuildTeamsAppOdataBind(channelTab.ChannelTab.TeamsAppId), };
            //if (SupportConfigTabs.Contains(channelTab.ChannelTab.TeamsAppId)) 
            tempTab.Configuration = channelTab.Configuration;
            return graphClient.AddChannelTab(groupId, channelId, tempTab).Id;
        }

        public override void UpdateChannelTab(string groupId, string channelId, RestoreTab channelTab)
        {
            var tempTab = new TabUpdateObj() { Id = channelTab.ChannelTab.Id, DisplayName = channelTab.ChannelTab.DisplayName, SortOrderIndex = channelTab.ChannelTab.SortOrderIndex, };
            if (SupportConfigTabs.Tabs.Contains(channelTab.ChannelTab.TeamsAppId)) 
            tempTab.Configuration = channelTab.Configuration;
            graphClient.UpdateChannelTab(groupId, channelId, tempTab);
        }

        public override void UpdateChannelTabConfig(string groupId, string channelId, string tabId, RestoreTab restoreTab)
        {
            var tempTab = new TabUpdateObj() { Id = tabId, Configuration = restoreTab.Configuration };
            this.graphClient.UpdateChannelTab(groupId, channelId, tempTab);
        }

        public override void DeleteChannelTab(string groupId, string channelId, string tabId)
        {
            this.graphClient.DeleteTeamsTab(groupId, channelId, tabId);
        }

        #endregion

        #region Channel Member



        public override List<ChannelMember> ListChannelMembers(string groupId, string channelId)
        {
            return this.graphClient.ListChannelMembers(groupId, channelId).Select(m => m.ToM()).ToList();
        }

        public override ChannelMember AddChannelMember(string groupId, string channelId, ChannelMember member, bool useUserId = true)
        {
            var id = useUserId
                ? string.IsNullOrEmpty(member.UserId) ? graphClient.GetUser(member.Email).Id : member.UserId
                : member.Email;
            var updataObj = new OTJChannelMember(member.Roles, graphClient.GenerateOdataBindString(id));
            return graphClient.AddChannelMember(groupId, channelId, updataObj).ToM();
        }

        public override void RemoveChannelMember(string teamId, string channelId, string membershipId)
        {
            graphClient.RemoveChannelMember(teamId, channelId, membershipId);
        }

        public override void UpdateChannelMemberRoles(string teamId, string channelId, ExchangeCommonWrapper.ChannelMember member)
        {
            if (string.IsNullOrEmpty(member.Id)) member.Id = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{channelId}##{member.UserId}")); ;
            this.graphClient.UpdateChannelMemberRoles(teamId, channelId, member.Id, member.Roles);
        }

        #endregion

        #region Channel Message

        [Obsolete]
        public override List<TeamChatMessage> QueryChannelMessagesDelta(string groupId, string channelId, ref string deltatoken)
        {
            bool moreAvailable;
            var result = new List<TeamChatMessage>();
            List<TeamChatMessage> tempArray;
            do
            {
                moreAvailable = QueryChannelMessagesDelta(groupId, channelId, ref deltatoken, out tempArray);
                result.AddRange(tempArray);
            }
            while (moreAvailable);

            return result;
        }

        public override bool QueryChannelMessagesDelta(string groupId, string channelId, ref string queryToken, out List<TeamChatMessage> chatMassages)
        {
            var delta = this.graphClient.QueryChannelMessagesDelta(groupId, channelId, queryToken);
            chatMassages = delta.Value.ToM().ToList();
            var moreAvailable = !string.IsNullOrEmpty(delta.OdataNextLink);
            queryToken = moreAvailable ? delta.OdataNextLink : delta.OdataDeltaLink;//ref queryToken must be assigned last
            return moreAvailable;
        }
        public override string TrackingDeltaToken(string teamId, string channelId, params string[] queryParameters)
        {
            bool moreAvailable;
            string queryToken = queryParameters.Any()
                ? $"?{string.Join("&", queryParameters)}"
                : null;
            do
            {
                var delta = this.graphClient.QueryChannelMessagesDelta(teamId, channelId, queryToken);
                moreAvailable = !string.IsNullOrEmpty(delta.OdataNextLink);
                queryToken = moreAvailable ? delta.OdataNextLink : delta.OdataDeltaLink;
            }
            while (moreAvailable);
            return queryToken;
        }

        public override List<TeamChatMessage> ListChannelAllMessages(string groupId, string channelId)
        {
            return this.graphClient.ListChannelAllMessages(groupId, channelId).ToM().ToList();
        }

        public bool ListChannelMessages(string teamId, string channelId, ref string skipToken, out List<TeamChatMessage> chatMassages, params string[] queryParams)
        {
            var collection = this.graphClient.ListChannelMessages(teamId, channelId, skipToken, queryParams);
            var moreAvailable = !string.IsNullOrEmpty(collection.OdataNextLink);
            chatMassages = collection?.Value.ToM().ToList();
            skipToken = collection.OdataNextLink;//ref skipToken must be assigned last
            return moreAvailable;
        }

        public override bool ListChannelMessages(string groupId, string channelId, int pageSize, ref string skipToken, out List<TeamChatMessage> chatMassages)
        {
            var collection = this.graphClient.ListChannelMessages(groupId, channelId, pageSize, skipToken);
            var moreAvailable = !string.IsNullOrEmpty(collection.OdataNextLink);
            chatMassages = collection?.Value.ToM().ToList();
            skipToken = collection.OdataNextLink;//ref skipToken must be assigned last
            return moreAvailable;
        }
        public override TeamChatMessage GetChannelMessage(string groupId, string channelId, string messageId)
        {
            return this.graphClient.GetChannelMessage(groupId, channelId, messageId).ToM();
        }

        public override bool ListChannelMessageReplies(string groupId, string channelId, string messageId, int pageSize, ref string skipToken, out List<TeamChatMessage> chatMassages)
        {
            var ChatMessage = this.graphClient.ListChannelMessageReplies(groupId, channelId, messageId, pageSize, skipToken);
            var moreAvailable = !string.IsNullOrEmpty(ChatMessage.OdataNextLink);
            chatMassages = ChatMessage?.Value.ToM().ToList();
            skipToken = ChatMessage.OdataNextLink;//ref skipToken must be assigned last
            return moreAvailable;
        }

        public override List<TeamChatMessage> ListChannelMessageAllReplies(string groupId, string channelId, string messageId)
        {
            return this.graphClient.ListChannelMessageReplies(groupId, channelId, messageId).ToM().ToList();
        }
        public override TeamChatMessage GetChannelMessageReply(string groupId, string channelId, string messageId, string replyId)
        {
            return this.graphClient.GetChannelMessageReply(groupId, channelId, messageId, replyId).ToM();
        }
        public override string SendChannelMessage(string groupId, string channelId, TeamChatMessage message)
        {
            return this.graphClient.SendChannelMessage(groupId, channelId, message.ToG()).Id;
        }
        public override string ReplyChannelMessage(string groupId, string channelId, string messageId, TeamChatMessage message)
        {
            return this.graphClient.ReplyChannelMessage(groupId, channelId, messageId, message.ToG()).Id;
        }

        #endregion

        #region Chat

        public override IEnumerable<TeamChatMessage> GetChatMessages(string userId, string startTime, string endTime, string? model, int? top) => graphClient.GetChatMessages(userId, startTime, endTime, model, top).ToM();

        public override IEnumerable<TeamChatMessage> GetChatMessagesInChat(string userId, string chatId, string startTime, string endTime, int? top, bool isGcc = false) => graphClient.GetChatMessagesInChat(userId, chatId, startTime, endTime, top, isGcc).ToM();

        public override TeamChatMessage GetChatMessage(string chatId, string messageId) => graphClient.GetChatMessage(chatId, messageId).ToM();

        public override IEnumerable<ChatEntity> GetChats(string userId, int? top, bool isNoSort = false) => graphClient.GetChats(userId, top, isNoSort).ToM();

        public override ChatEntity GetChat(string chatId) => graphClient.GetChat(chatId).ToM();

        #endregion

        public override byte[] GetHostedContentAsByte(string url) => graphClient.GetHostedContentAsByte(url);

        public override string GetHostedContentAsString(string url) => graphClient.GetHostedContentAsString(url);

        public override Dictionary<string, string> BatchGetHostedContentsAsString(Dictionary<string, string> requests, bool useBetaApi)
        {
            var batchRequestObj = graphClient.CreateBatchRequestObj(useBetaApi);
            requests.ForEach(request => batchRequestObj.Add(new BatchItem_GetHostedContentsAsString(request.Key, request.Value)));
            var result = BatchRequestWithRetry.Execute(batchRequestObj);
            return result.ToDictionary(k => k.Id, v => v.Body.ToString());
        }

        public override EventEntity GetEvent(string groupId, string eventId) => graphClient.GetEvent(groupId, eventId).ToM();

        public override ExchangeCommonWrapper.User GetUser(string idOrUserPrincipalName) => graphClient.GetUser(idOrUserPrincipalName).ToM();

        public override IEnumerable<ExchangeCommonWrapper.LicenseDetails> GetLicenseDetails(string userId) => graphClient.GetLicenseDetails(userId)?.ToM();

        public override TeamInfo GetTeamsMailById(string groupId)
        {
            return new TeamInfo { GroupId = groupId, Mail = graphClient.GetGroupById(groupId).Mail };
        }
    }
}