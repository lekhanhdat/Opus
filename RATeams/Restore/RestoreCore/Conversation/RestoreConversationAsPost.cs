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

namespace Office365GroupRestore
{
    using AvePoint.Wrapper.Common;
    using DocumentFormat.OpenXml.Spreadsheet;
    using ExchangeCommonWrapper;
    using ExchangeUtility;
    using ExchangeUtility.Graph;
    using Job.ModernManagement.Report;
    using Office365GroupBackup;
    using RAArchiverCommon;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;

    internal abstract class RestoreConversationAsPost : RestoreConversation
    {
        private const int messageLimit = 15000;
        private const int limitHostedContentLength = 4000000; // actually is 4194304, but total request maybe bigger than it

        //public MicrosoftTeamsAPIBase TeamsServiceForDelegate => M365APIService.TeamsServiceForDelegate;

        public MicrosoftTeamsAPIBase TeamsServiceForConversation { get; set; }

        public RestoreConversationAsPost(BaseRestoreHelperBatch baseHelper) : base(baseHelper)
        {
            if (UseMigrationMode)
            {
                TeamsServiceForConversation = TeamsService;
            }
            else
            {
                TeamsServiceForConversation = TeamsServiceForDelegate;
            }

            TeamsMembershipService = TeamsServiceForConversation.AuthObject.AuthType switch
            {
                AuthObjectType.PasswordAccessToken => TeamsServiceForConversation,
                AuthObjectType.AccessToken or _ => TeamsService
            };
        }

        protected override void RealRestore(IEnumerable<ExchangeDataBlockForBatch> dataCollection)
        {
            _ = TeamsServiceForConversation ?? throw new ArgumentNullException(nameof(TeamsServiceForConversation));

            if (!UseMigrationMode && !Config.IsSkipRestoreConversation)
            {
                AddMember();
            }

            string messageId = null;
            TeamChatMessage message = null;
            var conversation = new List<TeamChatMessage>();

            foreach (var dataBlock in dataCollection)
            {
                var baseEntity = dataBlock.RestoreData.Metadata;
                InitReport(baseEntity, baseEntity.DisplayPath);
                ReportDto.Title = TeamsConst.ConversationMessageReportTitle;
                ReportDto.Type = ReportNodeHeader.Conversation;

                ExecuteWithExceptionHandle(() =>
                {
                    message = GenerateMessage(dataBlock.RestoreData);

                    if (message.MessageType.IsSystemMessage()) return;

                    if (Config.IsSkipRestoreConversation)
                    {
                        ReportDto.Status = ReportStatus.Skipped;
                        Report.AddRestoreReport(ReportDto);
                        return;
                    }

                    if (Config.NeedMergeConversation)
                    {
                        RestoreMergeConversation(baseEntity.Id, message);
                    }
                    else
                    {
                        RestoreConversation(baseEntity.Id, message);
                    }
                });
            }

            if (Config.NeedMergeConversation) ExecuteWithExceptionHandle(() => InternalRestoreMergeConversation());

            #region Local functions

            void RestoreConversation(string sourceMessageId, TeamChatMessage message)
            {
                var isTopic = IsTopic(sourceMessageId);
                try
                {
                    if (isTopic)
                    {
                        messageId = TeamsServiceForConversation.SendChannelMessage(_GroupId, _CurrentChannel?.Id, message);
                    }
                    else
                    {
                        TeamsServiceForConversation.ReplyChannelMessage(_GroupId, _CurrentChannel?.Id, messageId, message);
                    }
                }
                catch (Exception ex) when (ex.IsHostedContentExceedLimitLength(message.MessageContent.HostedContents, limitHostedContentLength))
                {
                    logger.Error($"Restore message {sourceMessageId} failed due hosted content to long. ex: {ex}");
                    if (message.MessageContent.HostedContents.Count > 1)
                    {
                        logger.Info($"Try split hosted content in message to replies. Hosted content count: {message.MessageContent.HostedContents.Count}");
                        var (mainMessage, replyMessage, isExceedLimit) = SplitHostedContentMessage(message);
                        if (isExceedLimit)
                        {
                            ReportDto.Status = ReportStatus.Warn;
                            ReportDto.ErrorMessage = "RestoreConversationDueSomeHostedContentExceedLimitLength"; // Due to Microsoft API limitations, we cannot fully restore messages with large hosted content as it exceeds the length limit. The recoverable content will be restored as reply messages. You can restore the channel conversations as HTML files to view the entire message content.
                        }
                        try
                        {
                            if (isTopic)
                            {
                                messageId = TeamsServiceForConversation.SendChannelMessage(_GroupId, _CurrentChannel?.Id, mainMessage);
                                foreach (var reply in replyMessage)
                                {
                                    TeamsServiceForConversation.ReplyChannelMessage(_GroupId, _CurrentChannel?.Id, messageId, reply);
                                }
                            }
                            else
                            {
                                replyMessage.Insert(0, mainMessage);
                                foreach (var reply in replyMessage)
                                {
                                    TeamsServiceForConversation.ReplyChannelMessage(_GroupId, _CurrentChannel?.Id, messageId, reply);
                                }
                            }

                        }
                        catch (Exception e) when (e.IsHostedContentExceedLimitLength(message.MessageContent.HostedContents, limitHostedContentLength))
                        {
                            logger.Error($"Restore mainMessage with split hosted content failed. ex: {ex}");
                            throw new Exception("Agent_Teams_RestoreConversationFailedDueExceedLimitContentLength"); // Failed to restore messages with large hosted content due to Microsoft API limitations. You can restore the channel conversations as HTML files.
                        }
                    }
                    else
                    {
                        throw new Exception("Agent_Teams_RestoreConversationFailedDueExceedLimitContentLength"); // Failed to restore messages with large hosted content due to Microsoft API limitations. You can restore the channel conversations as HTML files.
                    }
                }

                Report.AddRestoreReport(ReportDto);
            }

            void RestoreMergeConversation(string sourceMessageId, TeamChatMessage message)
            {
                if (IsTopic(sourceMessageId))
                {
                    if (conversation.Count > 0)
                    {
                        InternalRestoreMergeConversation();
                    }

                    conversation.Add(message);
                }
                else
                {
                    conversation.Add(message);
                }
            }

            void InternalRestoreMergeConversation()
            {
                var mergeMessage = conversation.Aggregate((current, next) =>
                {
                    if (Encoding.UTF8.GetBytes(current.Body.Content).Length + Encoding.UTF8.GetBytes(next.Body.Content).Length > messageLimit)
                    {
                        SendMergeMessage(current);
                        return next;
                    }

                    return current.Merge(next, messageId == null);
                });
                SendMergeMessage(mergeMessage);

                conversation.Clear();
                messageId = null;
            }

            void SendMergeMessage(TeamChatMessage msg)
            {
                if (messageId == null)
                    messageId = TeamsServiceForConversation.SendChannelMessage(_GroupId, _CurrentChannel?.Id, msg);
                else
                    TeamsServiceForConversation.ReplyChannelMessage(_GroupId, _CurrentChannel?.Id, messageId, msg);

                Report.AddRestoreReport(ReportDto);
                SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(ReportDto.Size, ReportDto.SourcePath);
            }

            void ExecuteWithExceptionHandle(Action action)
            {
                try
                {
                    action();
                }
                catch (AggregateException ex)
                {
                    messageId = null;
                    ReportDto.Status = ReportStatus.Failed;
                    ReportDto.ErrorMessage = ex.WrapAggregateErrorMessage(I18NDataCollector.GetData(DynamicDataKey.UserName));
                    Report.AddRestoreReport(ReportDto);
                }
                catch (Exception ex) when (ex.Message.Contains("UnknownError") && UseMigrationMode && message?.MessageContent?.HostedContents?.Count > 0)
                {
                    logger.Error("Restore post contains hosted contents failed due limitation on Microsoft migration API.");
                    ReportDto.Status = ReportStatus.Failed;
                    ReportDto.ErrorMessage = "Agent.Teams.RestorePostWithHostedContentFailed"; // todo: need i18n key for this error ?
                    Report.AddRestoreReport(ReportDto);
                }
                catch (Exception ex) when (ex.Message.Contains("Forbidden, status code: Forbidden, internal error code:Forbidden") || ex.Message.Contains("UnknownError"))
                {
                    messageId = null;
                    ReportDto.Status = ReportStatus.Failed;
                    ReportDto.ErrorMessage = TeamsServiceForConversation.AuthObject.AuthType switch
                    {
                        AuthObjectType.PasswordAccessToken => "Agent.Teams.NotChannelOwner_1D33458A-47B0-4A7F-A596-B5B31A6EF8E6",
                        AuthObjectType.AccessToken => "Agent.Teams.NotChannelOwnerForApp_402E9475-5C7A-2579-03D1-E1E8643476F1",
                        _ => "Agent.Teams.NotChannelOwner_1D33458A-47B0-4A7F-A596-B5B31A6EF8E6"
                    };
                    Report.AddRestoreReport(ReportDto);

                }
                catch (Exception ex) when (ex.Message.Contains("AADSTS500341"))
                {
                    ReportDto.Status = ReportStatus.Failed;
                    ReportDto.ErrorMessage = RestoreConstants.DelegatedUserNotFoundKey;
                    Report.AddRestoreReport(ReportDto);
                }
                catch (Exception ex)
                {
                    messageId = null;
                    ReportDto.Status = ReportStatus.Failed;
                    ReportDto.ErrorMessage = ex.Message;
                    if (ex.Message.Contains("Ensure user has a valid Office365 license assigned to them"))
                        ReportDto.ErrorMessage = ex.Message + string.Format("and The user is: {0}.", TeamsServiceForConversation.AuthObject.UserName);
                    logger.Error("Send message error: {0}.", ex);
                    Report.AddRestoreReport(ReportDto);
                }
            }

            (TeamChatMessage mainMessage, List<TeamChatMessage> replyMessage, bool isExceedLimit) SplitHostedContentMessage(TeamChatMessage mainMessage)
            {
                bool isHostedContentExceedLimit = false;
                var originHostedContent = mainMessage.MessageContent.HostedContents.ToList();
                mainMessage.MessageContent.HostedContents.Clear();
                logger.Info($"total hosted content: {originHostedContent.Count}");

                var doc = new HtmlDocument();
                doc.LoadHtml(mainMessage.Body.Content);
                var hostedContentNode = new List<HtmlNode>();
                var replyMessages = new List<TeamChatMessage>();

                DiscoverHostedContent(doc.DocumentNode, hostedContentNode);

                var hostedContentLengthList = originHostedContent.Select(h => new KeyValuePair<string, int>(h.TemporaryId, Encoding.UTF8.GetByteCount(h.ContentBytes))).ToList();
                var hostedContentNotExceecdList = hostedContentLengthList.Where(x => x.Value <= limitHostedContentLength).ToList();

                if (hostedContentLengthList.Count > hostedContentNotExceecdList.Count)
                {
                    isHostedContentExceedLimit = true;
                    var hostedContentExccedList = hostedContentLengthList.Except(hostedContentNotExceecdList).Select(x => x.Key).ToList();
                    var hostedNodeExccedList = hostedContentNode.Where(node => hostedContentExccedList.Any(id => node.OuterHtml.Contains(id)));
                    foreach (var node in hostedNodeExccedList)
                    {
                        node.Remove();
                    }

                    logger.Info($"{hostedContentLengthList.Count - hostedContentNotExceecdList.Count} hosted content exceed limit.");
                }

                var hostedContentGroup = GroupHostedContent(hostedContentNotExceecdList);
                logger.Info($"{hostedContentNotExceecdList.Count} hosted content => {hostedContentGroup.Count} group.");

                for (int i = 0; i < hostedContentGroup.Count; i++)
                {
                    var hostedGroupKey = hostedContentGroup[i].Select(h => h.Key).ToList();
                    var hostedContentInGroup = originHostedContent.Where(h => hostedGroupKey.Contains(h.TemporaryId)).ToList();
                    logger.Info($"message {i} has {hostedContentGroup[i].Count} hosted content, total length: {hostedContentGroup[i].Sum(x => x.Value)}");

                    var hostedNodeInGroup = hostedContentNode.Where(node => hostedGroupKey.Any(id => node.OuterHtml.Contains(id))).ToList();
                    var tempId = 1;

                    foreach (var hostedContent in hostedContentInGroup)
                    {
                        var hostedNode = hostedContentNode.First(n => n.OuterHtml.Contains(hostedContent.TemporaryId));
                        var hostedNodeSrc = hostedNode.GetAttributeValue("src", string.Empty);
                        hostedNode.SetAttributeValue("src", hostedNodeSrc.Replace(hostedContent.TemporaryId, tempId.ToString()));
                        hostedContent.TemporaryId = tempId.ToString();
                        tempId++;
                    }

                    if (i == 0)
                    {
                        mainMessage.MessageContent.HostedContents.AddRange(hostedContentInGroup);
                    }
                    else
                    {
                        foreach (var hostedNode in hostedNodeInGroup)
                        {
                            hostedNode.Remove();
                        }

                        replyMessages.Add(new TeamChatMessage()
                        {
                            Body = new Body()
                            {
                                Content = string.Join("", hostedNodeInGroup.Select(n => n.OuterHtml)),
                                ContentType = mainMessage.Body.ContentType
                            },
                            MessageContent = new MessageContent
                            {
                                HostedContents = hostedContentInGroup
                            }
                        });
                    }
                }

                mainMessage.Body.Content = doc.DocumentNode.InnerHtml;

                return (mainMessage, replyMessages, isHostedContentExceedLimit);
            }

            List<List<KeyValuePair<string, int>>> GroupHostedContent(List<KeyValuePair<string, int>> hostedContent)
            {
                List<List<KeyValuePair<string, int>>> hostedContentGroup = new(hostedContent.Count);

                foreach (var item in hostedContent)
                {
                    int containerIndex = -1;

                    for (int i = 0; i < hostedContentGroup.Count; i++)
                    {
                        if (hostedContentGroup[i].Sum(h => h.Value) + item.Value <= limitHostedContentLength)
                        {
                            containerIndex = i;
                            break;
                        }
                    }

                    if (containerIndex != -1)
                    {
                        hostedContentGroup[containerIndex].Add(item);
                    }
                    else
                    {
                        hostedContentGroup.Add(new List<KeyValuePair<string, int>> { item });
                    }
                }

                return hostedContentGroup;
            }

            void DiscoverHostedContent(HtmlNode docNode, List<HtmlNode> hostedContentNode)
            {
                foreach (var node in docNode.ChildNodes)
                {
                    if (node.HasChildNodes)
                    {
                        DiscoverHostedContent(node, hostedContentNode);
                    }
                    else
                    {
                        if (node.GetAttributeValue("src", "").Contains("../hostedContents/"))
                        {
                            hostedContentNode.Add(node);
                        }
                    }
                }
            }

            #endregion
        }

        protected abstract TeamChatMessage GenerateMessage(ExchangeRestoreDataForBatch restoreData);

        private void AddMember()
        {
            var sender = TeamsServiceForConversation.AuthObject.UserName;
            if (string.IsNullOrEmpty(sender))
            {
                logger.Info("User name is empty for auth object, try to get user name from graph.");
                try
                {
                    var delegateUser = TeamsServiceForConversation.GetMe();
                    if (delegateUser != null)
                    {
                        sender = delegateUser.UserPrincipalName; //.Mail;
                        logger.Info("Get user name from graph successfully, user name: {0}.", sender);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Get user name from graph failed, exception: {0}.", e);
                }
            }

            var needCheckAddedMember = false;
            var teamMemberKey = _GroupId;
            if (!_ConversationMembers.TryGetValue(teamMemberKey, out var teamMembers) || !teamMembers.Any(m => string.Equals(m.Email, sender, StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    _ConversationMembers[teamMemberKey] = TeamsMembershipService.GetTeamMembers(_GroupId, sender).ConvertAll(m => new ConversationMember { Email = m.MailboxAddress, UserId = m.UserId });
                    if (!string.IsNullOrEmpty(sender) && !_ConversationMembers[teamMemberKey].Any(m => string.Equals(m.Email, sender, StringComparison.OrdinalIgnoreCase)))
                    {
                        var teamMember = TeamsMembershipService.AddTeamMember(_GroupId, new TeamMember
                        {
                            MailboxAddress = sender,
                            RoleType = TeamMemberRoleType.Member
                        }, false);
                        logger.Info("Add member: {0} to the team: {1}, id: {2}.", sender, _GroupId, teamMember.Id);
                        _ConversationMembers[teamMemberKey].Add(new ConversationMember
                        {
                            Id = teamMember.Id,
                            UserId = teamMember.UserId,
                            Email = sender,
                            NeedDelete = true
                        });
                        needCheckAddedMember = true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Add team member failed, {0}.", ex);
                }
            }

            if (_CurrentChannel.CurrenIsPrivateChannelSite)
            {
                var channelMemberKey = $"{_GroupId}{ExchangeConstants.PathParser}{_CurrentChannel?.Id}";
                if (!_ConversationMembers.TryGetValue(channelMemberKey, out var channelMembers) || !channelMembers.Any(m => string.Equals(m.Email, sender, StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        _ConversationMembers[channelMemberKey] = TeamsMembershipService.ListChannelMembers(_GroupId, _CurrentChannel?.Id).ConvertAll(m => new ConversationMember { Email = m.Email, UserId = m.UserId });
                        if (!string.IsNullOrEmpty(sender) && !_ConversationMembers[channelMemberKey].Any(m => string.Equals(m.Email, sender, StringComparison.OrdinalIgnoreCase)))
                        {
                            var channelMember = TeamsMembershipService.AddChannelMember(_GroupId, _CurrentChannel?.Id, new ChannelMember 
                            { 
                                Email = sender, 
                                Roles = new string[] { "member" } 
                            }, false);
                            logger.Info("Add member: {0} to the private channel: {1}/{2}, id: {3}.", sender, _GroupId, _CurrentChannel?.Id, channelMember.Id);
                            _ConversationMembers[channelMemberKey].Add(new ConversationMember 
                            { 
                                Id = channelMember.Id, 
                                UserId = channelMember.UserId, 
                                Email = sender,
                                NeedDelete = true
                            });
                            needCheckAddedMember = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Add channel member failed, {0}.", ex);
                    }
                }
            }

            if (needCheckAddedMember)
            {
                logger.Info("Validate added member: {0} for team: {1}, private channel: {2}.", sender, _GroupId, _CurrentChannel?.Id);
                ValidateAddedMember(sender, _CurrentChannel?.CurrenIsPrivateChannelSite ?? false);
            }
        }

        private void ValidateAddedMember(string sender, bool isChannel)
        {
            try
            {
                var helper = new AveTaskRetryHelper(10, true, 5000);
                helper.ExecuteWithRetryMechanismV3(() =>
                {
                    var members = isChannel 
                        ? TeamsMembershipService.ListChannelMembers(_GroupId, _CurrentChannel?.Id)
                            .ConvertAll(m => new ConversationMember { Email = m.Email, UserId = m.UserId })
                        : TeamsMembershipService.GetTeamMembers(_GroupId, sender)
                            .ConvertAll(m => new ConversationMember { Email = m.MailboxAddress, UserId = m.UserId });

                    if (members?.Any(x => x.Email.Equals(sender, StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        logger.Info($"User {sender} is now a member and can send message. isChannel: {isChannel}");
                        return;
                    }
                    logger.Info($"User {sender} not yet a member. isChannel: {isChannel}");
                    throw new Exception($"User {sender} not yet a member");
                }
                );
            }
            catch (Exception e)
            {
                logger.Error($"Validate added member failed, {e}");
                //throw;
            }
        }

        private bool IsTopic(string messageId) => RestoreConfig.TopicItemIds.Contains(messageId);
    }
}