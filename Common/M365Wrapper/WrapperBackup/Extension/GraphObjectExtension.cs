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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using AvePoint.GCommon.GraphAPI;
    using ExchangeCommonWrapper;
    using Newtonsoft.Json;
    using GraphSdk = AvePoint.GCommon.GraphAPI;

    static class TeamExtension
    {
        public static TeamChannel ToM(this Channel channel)
        {
            return new TeamChannel()
            {
                Id = channel.Id,
                Description = channel.Description,
                DisplayName = channel.DisplayName,
                MembershipType = channel.MembershipType,
                FilesFolderUrl = channel.FilesFolderWebUrl,
                AdditionalData = GenerateAdditionalData(channel.AdditionalData),
            };
        }
        public static TeamChannel ToMV2(this Channel channel)
        {
            var temp = channel.ToM();
            if (temp.IsSharedChannel())
            {
                var webUrlInfo = HttpUtility.ParseQueryString(new Uri(channel.WebUrl).Query);
                temp.IsExternal = !channel.TenantId.EqualsIgnoreCase(webUrlInfo.Get("tenantId"));
                if (channel.OdataId.IsNotNullValueOrEmpty() && !temp.IsExternal)
                {
                    var match = Regex.Match(channel.OdataId, "(?<=teams/)[A-F0-9]{8}(-[A-F0-9]{4}){3}-[A-F0-9]{12}", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        temp.IsInComing = !webUrlInfo.Get("groupId").EqualsIgnoreCase(match.Value);
                        if (temp.IsInComing)
                            temp.AdditionalData["HostTeamId"] = match.Value;
                    }
                }
                else
                    temp.IsInComing = temp.IsExternal;
            }
            return temp;
        }
        public static IEnumerable<TeamChannel> ToM(this IEnumerable<Channel> channels, bool isIncomingChannels = false)
        {
            if (isIncomingChannels)
                return channels.Select(c => c.ToMV2());
            else
                return channels.Select(c => c.ToM());
        }

        public static MicrosoftTeamsEntity ToM(this TeamObj team)
        {
            return new MicrosoftTeamsEntity()
            {
                TeamFunSettings = team.FunSettings?.ToM(),
                TeamGuestSettings = team.GuestSettings?.ToM(),
                TeamMemberSettings = team.MemberSettings?.ToM(),
                TeamMessagingSettings = team.MessagingSettings?.ToM(),
                AdditionalData = GenerateAdditionalData(team.AdditionalData),
                IsArchived = team.IsArchived,
            };
        }

        public static TeamFunSetting ToM(this TeamFunSettings funSettings)
        {
            return new TeamFunSetting
            {
                AllowCustomMemes = funSettings.AllowCustomMemes ?? false,
                AllowGiphy = funSettings.AllowGiphy ?? false,
                AllowStickersAndMemes = funSettings.AllowStickersAndMemes ?? false,
                GiphyContentRating = (GiphyContentRating)(funSettings.GiphyContentRating ?? default(GiphyRatingType)),
                AdditionalData = GenerateAdditionalData(funSettings.AdditionalData),
            };
        }
        public static TeamGuestSetting ToM(this TeamGuestSettings guestSettings)
        {
            return new TeamGuestSetting
            {
                AllowCreateUpdateChannels = guestSettings.AllowCreateUpdateChannels ?? false,
                AllowDeleteChannels = guestSettings.AllowDeleteChannels ?? false,
                AdditionalData = GenerateAdditionalData(guestSettings.AdditionalData),
            };
        }
        public static TeamMemberSetting ToM(this TeamMemberSettings memberSettings)
        {
            return new TeamMemberSetting
            {
                AllowAddRemoveApps = memberSettings.AllowAddRemoveApps ?? false,
                AllowCreateUpdateChannels = memberSettings.AllowCreateUpdateChannels ?? false,
                AllowCreateUpdateRemoveConnectors = memberSettings.AllowCreateUpdateRemoveConnectors ?? false,
                AllowCreateUpdateRemoveTabs = memberSettings.AllowCreateUpdateRemoveTabs ?? false,
                AllowDeleteChannels = memberSettings.AllowDeleteChannels ?? false,
                AdditionalData = GenerateAdditionalData(memberSettings.AdditionalData),
            };
        }
        public static TeamMessagingSetting ToM(this TeamMessagingSettings messagingSettings)
        {
            return new TeamMessagingSetting
            {
                AllowChannelMentions = messagingSettings.AllowChannelMentions ?? false,
                AllowOwnerDeleteMessages = messagingSettings.AllowOwnerDeleteMessages ?? false,
                AllowTeamMentions = messagingSettings.AllowTeamMentions ?? false,
                AllowUserDeleteMessages = messagingSettings.AllowUserDeleteMessages ?? false,
                AllowUserEditMessages = messagingSettings.AllowUserEditMessages ?? false,
                AdditionalData = GenerateAdditionalData(messagingSettings.AdditionalData),
            };
        }
        public static TeamObj ToG(this MicrosoftTeamsEntity team)
        {
            return new TeamObj()
            {
                FunSettings = team.TeamFunSettings?.ToG() ?? new TeamFunSettings(),
                GuestSettings = team.TeamGuestSettings?.ToG() ?? new TeamGuestSettings(),
                MemberSettings = team.TeamMemberSettings?.ToG() ?? new TeamMemberSettings(),
                MessagingSettings = team.TeamMessagingSettings?.ToG() ?? new TeamMessagingSettings(),
                //AdditionalData = GenerateJsonAdditionalData(team.AdditionalData),
            };
        }
        public static TeamFunSettings ToG(this TeamFunSetting funSettings)
        {
            return new TeamFunSettings
            {
                AllowCustomMemes = funSettings.AllowCustomMemes,
                AllowGiphy = funSettings.AllowGiphy,
                AllowStickersAndMemes = funSettings.AllowStickersAndMemes,
                GiphyContentRating = (GiphyRatingType)funSettings.GiphyContentRating,
                //AdditionalData = GenerateJsonAdditionalData(funSettings.AdditionalData),
            };
        }
        public static TeamGuestSettings ToG(this TeamGuestSetting guestSettings)
        {
            return new TeamGuestSettings
            {
                AllowCreateUpdateChannels = guestSettings.AllowCreateUpdateChannels,
                AllowDeleteChannels = guestSettings.AllowDeleteChannels,
                //AdditionalData = GenerateJsonAdditionalData(guestSettings.AdditionalData),
            };
        }
        public static TeamMemberSettings ToG(this TeamMemberSetting memberSettings)
        {
            return new TeamMemberSettings
            {
                AllowAddRemoveApps = memberSettings.AllowAddRemoveApps,
                AllowCreateUpdateChannels = memberSettings.AllowCreateUpdateChannels,
                AllowCreateUpdateRemoveConnectors = memberSettings.AllowCreateUpdateRemoveConnectors,
                AllowCreateUpdateRemoveTabs = memberSettings.AllowCreateUpdateRemoveTabs,
                AllowDeleteChannels = memberSettings.AllowDeleteChannels,
                //AdditionalData = GenerateJsonAdditionalData(memberSettings.AdditionalData),
            };
        }
        public static TeamMessagingSettings ToG(this TeamMessagingSetting messagingSettings)
        {
            return new TeamMessagingSettings
            {
                AllowChannelMentions = messagingSettings.AllowChannelMentions,
                AllowOwnerDeleteMessages = messagingSettings.AllowOwnerDeleteMessages,
                AllowTeamMentions = messagingSettings.AllowTeamMentions,
                AllowUserDeleteMessages = messagingSettings.AllowUserDeleteMessages,
                AllowUserEditMessages = messagingSettings.AllowUserEditMessages,
                //AdditionalData = GenerateJsonAdditionalData(messagingSettings.AdditionalData),
            };
        }

        public static TeamsApp ToM(this InstalledApp installedApp)
        {
            return new TeamsApp()
            {
                Id = installedApp.Id,
                TeamsAppDefinition = new TeamsAppDefinition()
                {
                    Id = installedApp.AppDefinition.Id,
                    TeamsAppId = installedApp.AppDefinition.TeamsAppId,
                    DisplayName = installedApp.AppDefinition.DisplayName,
                    Version = installedApp.AppDefinition.Version,
                    AdditionalData = GenerateAdditionalData(installedApp.AppDefinition.AdditionalData),
                },
                AdditionalData = GenerateAdditionalData(installedApp.AdditionalData),
            };
        }

        public static IEnumerable<TeamsApp> ToM(this IEnumerable<InstalledApp> installedApps)
        {
            return installedApps.Select(iA => iA.ToM());
        }

        public static CatalogApp ToM(this CatalogTeamsApp catalogTeamsApp)
        {
            return new CatalogApp()
            {
                Id = catalogTeamsApp.Id,
                ExternalId = catalogTeamsApp.ExternalId,
                Name = catalogTeamsApp.Name,
                DisplayName = catalogTeamsApp.DisplayName,
                DistributionMethod = catalogTeamsApp.DistributionMethod,
                AdditionalData = GenerateAdditionalData(catalogTeamsApp.AdditionalData),
            };
        }

        public static IEnumerable<CatalogApp> ToM(this IEnumerable<CatalogTeamsApp> catalogTeamsApps)
        {
            return catalogTeamsApps.Select(cTA => cTA.ToM());
        }

        public static ChannelTab ToM(this Tab tab)
        {
            return new ChannelTab()
            {
                Id = tab.Id,
                Name = tab.Name,
                DisplayName = tab.DisplayName,
                TeamsAppId = tab.TeamsApp?.Id,
                TeamsApp = tab.TeamsApp?.ToM(),
                SortOrderIndex = tab.SortOrderIndex,
                MessageId = tab.MessageId,
                WebUrl = tab.WebUrl,
                Configuration = tab.Configuration.ToString(),
                AdditionalData = GenerateAdditionalData(tab.AdditionalData),
            };
        }
        public static ExchangeCommonWrapper.TeamsAppInfo ToM(this AvePoint.GCommon.GraphAPI.TeamsAppInfo appinfo)
        {
            return new ExchangeCommonWrapper.TeamsAppInfo()
            {
                Id = appinfo.Id,
                ExternalId = appinfo.ExternalId,
                DisplayName = appinfo.DisplayName,
                DistributionMethod = appinfo.DistributionMethod,
            };
        }
        public static IEnumerable<ChannelTab> ToM(this IEnumerable<Tab> tab)
        {
            return tab.Select(t => t.ToM());
        }

        #region Channel Member
        public static ChannelMember ToM(this Member channelMembser)
        {
            return new ChannelMember()
            {
                Id = channelMembser.Id,
                ODataType = channelMembser.ODataType,
                UserId = channelMembser.UserId,
                DisplayName = channelMembser.DisplayName ?? ExchangeConstants.UnkonwUserDisplayName,
                Roles = channelMembser.Roles,
                Email = channelMembser.Email ?? ExchangeConstants.UnkonwUserEmail,
                VisibleHistoryStartDateTime = channelMembser.VisibleHistoryStartDateTime
            };
        }
        public static OTJChannelMember ToUpdateRolesObj(this ChannelMember member)
        {
            return new OTJChannelMember() { Roles = member.Roles, };
        }
        #endregion

        #region Channel Message

        public static IEnumerable<TeamChatMessage> ToM(this IEnumerable<ChatMessage> chatMessages)
        {
            return chatMessages.Select(msg => msg.ToM());
        }

        public static TeamChatMessage ToM(this ChatMessage chatMessage) =>
            new TeamChatMessage
            {
                OdataContext = chatMessage.OdataContext,
                Id = chatMessage.Id,
                ReplyToId = chatMessage.ReplyToId,
                Etag = chatMessage.Etag,
                MessageType = chatMessage.MessageType,
                CreatedDateTime = chatMessage.CreatedDateTime,
                LastModifiedDateTime = chatMessage.LastModifiedDateTime,
                DeletedDateTime = chatMessage.DeletedDateTime,
                Subject = chatMessage.Subject,
                Summary = chatMessage.Summary,
                ChatId = chatMessage.ChatId,
                Importance = chatMessage.Importance,
                Locale = chatMessage.Locale,
                WebUrl = chatMessage.WebUrl,
                PolicyViolation = chatMessage.PolicyViolation?.ToString(),
                Body = chatMessage.Body == null ? null : new Body
                {
                    Content = chatMessage.Body.Content,
                    ContentType = chatMessage.Body.ContentType,
                },
                From = chatMessage.From == null ? null : new From
                {
                    Application = chatMessage.From.Application == null ? null : new Application
                    {
                        Id = chatMessage.From.Application.Id,
                        DisplayName = chatMessage.From.Application.DisplayName,
                        ApplicationIdentityType = chatMessage.From.Application.ApplicationIdentityType
                    },
                    Device = chatMessage.From.Device?.ToString(),
                    Conversation = chatMessage.From.Conversation == null ? null : new Conversation
                    {
                        Id = chatMessage.From.Conversation.Id,
                        DisplayName = chatMessage.From.Conversation.DisplayName,
                        ConversationIdentityType = chatMessage.From.Conversation.ConversationIdentityType
                    },
                    User = chatMessage.From.User == null ? null : new ExchangeCommonWrapper.User
                    {
                        Id = chatMessage.From.User.Id,
                        DisplayName = chatMessage.From.User.DisplayName,
                        UserIdentityType = chatMessage.From.User.UserIdentityType
                    },
                },
                Attachments = chatMessage.Attachments?.Select(a => new Attachment
                {
                    Id = a.Id,
                    ContentType = a.ContentType,
                    ContentUrl = a.ContentUrl,
                    Content = a.Content,
                    Name = a.Name,
                    ThumbnailUrl = a.ThumbnailUrl,
                }).ToList(),
                Mentions = chatMessage.Mentions?.Select(m => new Mantion()
                {
                    Id = m.Id,
                    MentionText = m.MentionText,
                    Mentioned = m.Mentioned == null ? null : new Mentioned
                    {
                        Application = m.Mentioned.Application == null ? null : new Application
                        {
                            Id = m.Mentioned.Application.Id,
                            DisplayName = m.Mentioned.Application.DisplayName,
                            ApplicationIdentityType = m.Mentioned.Application.ApplicationIdentityType
                        },
                        Device = m.Mentioned.Device?.ToString(),
                        Conversation = m.Mentioned.Conversation == null ? null : new Conversation
                        {
                            Id = m.Mentioned.Conversation.Id,
                            DisplayName = m.Mentioned.Conversation.DisplayName,
                            ConversationIdentityType = m.Mentioned.Conversation.ConversationIdentityType
                        },
                        MUser = m.Mentioned.User == null ? null : new ExchangeCommonWrapper.User
                        {
                            Id = m.Mentioned.User.Id,
                            DisplayName = m.Mentioned.User.DisplayName,
                            UserIdentityType = m.Mentioned.User.UserIdentityType,
                        },
                    },
                }).ToList(),
                Reactions = chatMessage.Reactions?.Select(r => new Reaction
                {
                    CreatedDataTime = r.CreatedDateTime,
                    ReactionType = r.ReactionType,
                    ReUser = r.User?.User == null ? null : new ExchangeCommonWrapper.User
                    {
                        Id = r.User.User.Id,
                        DisplayName = r.User.User.DisplayName,
                        UserIdentityType = r.User.User.UserIdentityType,
                    },
                    DisplayName = r.DisplayName
                }).ToList()
            };

        public static IEnumerable<TeamChatMessage> ToM(this IEnumerable<ChannelRootMessage> chatMessages)
        {
            return chatMessages.Select(msg => msg.ToM());
        }
        public static TeamChatMessage ToM(this ChannelRootMessage chatMessage)
        {
            var message = (chatMessage as ChatMessage).ToM();
            message.RepliesContext = chatMessage.RepliesOdataContext;
            message.RepliesCount = chatMessage.RepliesOdataCount;
            message.RepliesNextLink = chatMessage.RepliesOdataNextLink;
            message.Replies = chatMessage.Replies?.ToM();
            //There is a probability that the API will not return a reply count
            if ((chatMessage?.Replies?.Any() ?? false) && !chatMessage.RepliesOdataCount.HasValue)
            {
                message.RepliesCount = chatMessage.Replies.Length;
            }
            return message;
        }

        public static ChatMessage ToG(this TeamChatMessage message) =>
       new ChatMessage
       {
           Body = new CMBody
           {
               Content = message.Body.Content,
               ContentType = message.Body.ContentType,
           },
           Subject = message.Subject,
           Importance = message.Importance,
           Mentions = message.Mentions?.Select(m => new CMMention
           {
               Id = m.Id,
               MentionText = m.MentionText,
               Mentioned = GenerateMentioned(m)
           }).ToArray(),
           Attachments = message.Attachments?.Select(a => new CMAttachment
           {
               Id = a.Id,
               ContentType = a.ContentType,
               ContentUrl = a.ContentUrl,
               Content = a.Content,
               Name = a.Name,
               ThumbnailUrl = a.ThumbnailUrl,
           }).ToArray(),
           HostedContents = message.MessageContent?.HostedContents?.Select(h => new CMHostedContents
           {
               TemporaryId = h.TemporaryId,
               ContentBytes = h.ContentBytes,
               ContentType = h.ContentType
           }).ToArray()
       };
        private static CMIdentitySet GenerateMentioned(Mantion m)
        {
            var cmIdentitySet = new CMIdentitySet();
            if (m.Mentioned == null) return cmIdentitySet;
            if (m.Mentioned.Application != null)
            {
                cmIdentitySet.Application = new CMApplication
                {
                    Id = m.Mentioned.Application.Id,
                    DisplayName = m.Mentioned.Application.DisplayName,
                    ApplicationIdentityType = m.Mentioned.Application.ApplicationIdentityType
                };
                return cmIdentitySet;
            }
            if (!string.IsNullOrEmpty(m.Mentioned.Device))
            {
                cmIdentitySet.Device = m.Mentioned.Device;
                return cmIdentitySet;
            }
            if (m.Mentioned.Conversation != null)
            {
                cmIdentitySet.Conversation = new CMConversation
                {
                    Id = m.Mentioned.Conversation.Id,
                    DisplayName = m.Mentioned.Conversation.DisplayName,
                    ConversationIdentityType = m.Mentioned.Conversation.ConversationIdentityType
                };
                return cmIdentitySet;
            }
            if (m.Mentioned.MUser != null)
            {
                cmIdentitySet.User = new CMIdentitySetUser
                {
                    Id = m.Mentioned.MUser.Id,
                    DisplayName = m.Mentioned.MUser.DisplayName,
                    UserIdentityType = m.Mentioned.MUser.UserIdentityType,
                };
                return cmIdentitySet;
            }
            cmIdentitySet.User = new CMIdentitySetUser
            {
                Id = Guid.NewGuid().ToString(),
                DisplayName = m.MentionText,
                UserIdentityType = "aadUser",
            };
            return cmIdentitySet;
        }
        #endregion


        #region Chat

        public static IEnumerable<ChatEntity> ToM(this IEnumerable<Chat> chats) => chats.Select(c => c.ToM());

        public static ChatEntity ToM(this Chat chat)
        {
            return new()
            {
                Id = chat.Id,
                Topic = chat.Topic,
                CreatedDateTime = chat.CreatedDateTime,
                LastUpdatedDateTime = chat.LastUpdatedDateTime,
                ChatType = chat.ChatType,
                Members = chat.Members?.Select(m => m.ToM()).ToList(),
                LastMessagePreview = chat.LastMessagePreview?.ToM()
            };
        }

        #endregion

        public static IDictionary<string, object> GenerateAdditionalData(IDictionary<string, object> jsonAdditionalData)
        {
            if (jsonAdditionalData == null) return new Dictionary<string, object>();
            else return jsonAdditionalData.ToDictionary(ad => ad.Key, ad => ad.Value != null ? ad.Value.ToString() as object : null);
        }

        public static IDictionary<string, object> GenerateJsonAdditionalData(IDictionary<string, object> additionalData)
        {
            var jsonAdditionalData = new Dictionary<string, object>();
            if (additionalData != null)
            {
                foreach (var teamAddData in additionalData)
                {
                    if (teamAddData.Value == null) jsonAdditionalData.Add(teamAddData.Key, teamAddData.Value);
                    else jsonAdditionalData.Add(teamAddData.Key, JsonConvert.DeserializeObject(teamAddData.Value.ToString()));
                }
            }
            return jsonAdditionalData;
        }

        public static EventEntity ToM(this Event @event) =>
            new EventEntity
            {
                Id = @event.Id,
                ChangeKey = @event.ChangeKey,
                CreatedDateTime = @event.CreatedDateTime,
                LastModifiedDateTime = @event.LastModifiedDateTime,
                TransactionId = @event.TransactionId,
                OriginalStartTimeZone = @event.OriginalStartTimeZone,
                OriginalEndTimeZone = @event.OriginalEndTimeZone,
                ICalUId = @event.ICalUId,
                ReminderMinutesBeforeStart = @event.ReminderMinutesBeforeStart,
                IsReminderOn = @event.IsReminderOn,
                HasAttachments = @event.HasAttachments,
                Subject = @event.Subject,
                BodyPreview = @event.BodyPreview,
                Importance = @event.Importance,
                Sensitivity = @event.Sensitivity,
                IsAllDay = @event.IsAllDay,
                IsCancelled = @event.IsCancelled,
                IsOrganizer = @event.IsOrganizer,
                ResponseRequested = @event.ResponseRequested,
                SeriesMasterId = @event.SeriesMasterId,
                ShowAs = @event.ShowAs,
                Type = @event.Type,
                WebLink = @event.WebLink,
                OnlineMeetingUrl = @event.OnlineMeetingUrl,
                IsOnlineMeeting = @event.IsOnlineMeeting,
                OnlineMeetingProvider = @event.OnlineMeetingProvider,
                AllowNewTimeProposals = @event.AllowNewTimeProposals,
                IsDraft = @event.IsDraft,
                HideAttendees = @event.HideAttendees,
                Recurrence = @event.Recurrence,
                ResponseStatus = @event.ResponseStatus,
                Body = @event.Body == null ? null : new Body
                {
                    Content = @event.Body.Content,
                    ContentType = @event.Body.ContentType
                },
                Start = @event.Start == null ? null : new DateTimeTimeZoneEntity
                {
                    DateTime = @event.Start.DateTime,
                    TimeZone = @event.Start.TimeZone
                },
                End = @event.End == null ? null : new DateTimeTimeZoneEntity
                {
                    DateTime = @event.End.DateTime,
                    TimeZone = @event.End.TimeZone
                },
                Location = @event.Location,
                Locations = @event.Locations,
                Attendees = @event.Attendees,
                Organizer = @event.Organizer,
                OnlineMeeting = @event.OnlineMeeting,
                OriginalStart = @event.OriginalStart,
                Calendar = @event.Calendar
            };

        public static ExchangeCommonWrapper.User ToM(this GraphUser user) =>
            new ExchangeCommonWrapper.User
            {
                Id = user.Id,
                DisplayName = user.DisplayName
            };

        public static ExchangeCommonWrapper.LicenseDetails ToM(this GraphSdk.LicenseDetails info)
        {
            return new ExchangeCommonWrapper.LicenseDetails()
            {
                SkuId = info.SkuId,
                SkuPartNumber = info.SkuPartNumber,
                ServicePlans = info.ServicePlans?.Select(s => new ExchangeCommonWrapper.ServicePlanInfo
                {
                    AppliesTo = s.AppliesTo,
                    ProvisioningStatus = s.ProvisioningStatus,
                    ServicePlanId = s.ServicePlanId,
                    ServicePlanName = s.ServicePlanName
                }).ToArray()
            };
        }

        public static IEnumerable<ExchangeCommonWrapper.LicenseDetails> ToM(this IEnumerable<GraphSdk.LicenseDetails> infos)
        {
            return infos.Select(i => i.ToM());
        }
    }

    static class UserExtension
    {
        public static TeamMember ToTeamMemberM(this GraphUser user, bool owner)
        {
            return new TeamMember()
            {
                UserId = user.Id,
                DisplayName = user.DisplayName,
                MailboxAddress = user.UserPrincipalName,
                RoleType = ConverToRoleType(user.UserType, owner)
            };
        }

        public static TeamMember ToTeamMemberM(this Member user)
        {
            return new TeamMember()
            {
                Id = user.Id,
                UserId = user.UserId,
                DisplayName = user.DisplayName,
                MailboxAddress = user.Email,
                RoleType = ConverToRoleType(user.Roles)
            };
        }

        private static TeamMemberRoleType ConverToRoleType(string userType, bool owner)
        {
            if (owner) return TeamMemberRoleType.Owner;
            if (string.Equals(userType, "guest", StringComparison.OrdinalIgnoreCase)) return TeamMemberRoleType.Guest;
            return TeamMemberRoleType.Member;
        }

        private static TeamMemberRoleType ConverToRoleType(string[] userType)
        {
            if (userType.Contains("owner")) return TeamMemberRoleType.Owner;
            if (userType.Contains("guest")) return TeamMemberRoleType.Guest;
            return TeamMemberRoleType.Member;
        }
    }

}