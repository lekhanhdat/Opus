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
    using Microsoft365Backup.DataBuilder.TeamHtml;
    using AvePoint.Metadata;
    
    using ExchangeCommonWrapper;
    using ExchangeUtility.Graph;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    

    internal class RestoreConversationFromGraphAsHtml : RestoreConversationAsHtml
    {
        private const string cardAppUnknown = "UNKNOWN";
        private const string cardAppPlaces = "Places";
        private const string cardAppWeather = "Weather";
        private const string cardAppNews = "News";

        public RestoreConversationFromGraphAsHtml(BaseRestoreHelperBatch baseHelper) : base(baseHelper)
        {
        }

        private static readonly Dictionary<string, string> cardAppIconMapping = new Dictionary<string, string>
        {
            [cardAppUnknown] = string.Empty,
            [cardAppPlaces] = "https://statics.teams.cdn.office.net/evergreen-assets/places/Places_96x96.png?v=0.1",
            [cardAppWeather] = "https://statics.teams.cdn.office.net/evergreen-assets/apps/Weather_largeimage.png?v=0.3",
            [cardAppNews] = "https://statics.teams.cdn.office.net/evergreen-assets/apps/News_largeimage.png?v=0.5",
        };
        private static readonly List<string> imageExtensions = new List<string> { "jpg", "jpeg", "png", };

        protected override string GenerateChannelName(MetadataEntity baseEntity) => _CurrentChannel?.DisplayName ?? string.Empty;

        protected override string GenerateparentFolderName(string channelName) =>
            string.IsNullOrEmpty(_CurrentChannel?.FilesFolderUrl)
            ? channelName
            : _CurrentChannel?.FilesFolderUrl.Split('/').LastOrDefault() ?? channelName;

        protected override string GetEntityTitle(MetadataEntity baseEntity) => baseEntity.Title;

        protected override (ConversationItem Item, Dictionary<string, string> SiteUrlMap) GenerateConversationItem(ExchangeRestoreDataForBatch restoreData, MetadataEntity baseEntity)
        {
            var message = restoreData.TryGetMetadata<TeamChatMessage>(AveMetadataType.ExchangeMicrosoftTeamsConversationItem);

            if (Config.IsSkipRestoreConversation)
            {
                // no need message info, just return the message type to check if system message.
                return (new ConversationTopic()
                {
                    Type = message.MessageType,
                }, []);
            }

            using (var stream = restoreData.RestoreStream.OpenContentStream())
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    var content = reader.ReadToEnd();
                    try
                    {
                        message.MessageContent = JsonConvert.DeserializeObject<MessageContent>(content);
                    }
                    catch (Exception ex)
                    {
                        logger.Info("It is old version content: {0}.", ex);
                        message.Body.Content = content;
                    }
                }
            }

            if (RestoreConfig.TopicItemIds.Contains(baseEntity.Id))
            {
                return (new ConversationTopic()
                {
                    PostedBy = baseEntity.Sender,
                    PostedTime = message.CreatedDateTime.ToPostedTime(),
                    Body = RegenerateBody(message),
                    Subject = message.Subject,
                    Important = message.Importance.Equals("high", StringComparison.OrdinalIgnoreCase),
                    Type = message.MessageType,
                    HostedContents = message.MessageContent?.HostedContents?.ToDictionary(key => key.TemporaryId, value => value.ContentBytes),
                    Announcement = TeamsMessageUtility.GenerateAnnouncementBanner(message),
                    Reaction = TeamsMessageUtility.GenerateConversationReaction(message)
                }, null);
            }

            return (new ConversationReply()
            {
                PostedBy = baseEntity.Sender,
                PostedTime = message.CreatedDateTime.ToPostedTime(),
                Body = RegenerateBody(message),
                Important = message.Importance.Equals("high", StringComparison.OrdinalIgnoreCase),
                Type = message.MessageType,
                HostedContents = message.MessageContent?.HostedContents?.ToDictionary(key => key.TemporaryId, value => value.ContentBytes),
                Reaction = TeamsMessageUtility.GenerateConversationReaction(message)
            }, null);
        }

        private string RegenerateBody(TeamChatMessage message)
        {
            if (message.Attachments == null || message.Attachments.Count == 0) return message.Body.Content;

            var attachments = message.Attachments.ToLookup(a => a.Id).ToDictionary(a => a.Key, a => a.FirstOrDefault());
            var doc = new AvePoint.Wrapper.Common.HtmlDocument();
            try
            {
                doc.LoadHtml(message.Body.Content);
                var root = doc.DocumentNode;
                var attachmentNodes = root.SelectNodes("//attachment");
                foreach (var node in attachmentNodes)
                {
                    if (attachments.TryGetValue(node.Id, out var attachment))
                    {
                        if (attachment.ContentType.Equals(TeamUtil.AttachmentAnnouncementBannerType))
                        {
                            node.Remove();
                            continue;
                        }
                        var content = AppendAttachment(message.MessageContent, attachment);
                        if (!string.IsNullOrEmpty(content))
                        {
                            node.InnerHtml = content;
                        }
                    }
                }
                return root.InnerHtml;
            }
            catch (Exception ex)
            {
                logger.Warn("Regenerate Message[{0}] body failed, so use the original string. Error: {1}.", message.Id, ex);
                return message.Body.Content;
            }
        }

        private string AppendAttachment(MessageContent messageContent, Attachment attachment)
        {
            switch (attachment.ContentType)
            {
                case TeamUtil.AttachmentReferenceType:
                    return GenerateFileLink(attachment);
                case TeamUtil.AttachmentCardHeroType:
                case TeamUtil.AttachmentCardThumbnailTye:
                    return GenerateHeroCard(attachment);
                case TeamUtil.AttachmentMeetingType:
                    if (messageContent == null) return null;
                    var meeting = TeamsMessageUtility.GenerateMeetingMessage(messageContent, attachment);
                    return string.Format(TeamHtmlResources.MeetingAsHtmlTemplate_html, meeting.Title, meeting.BasicInfo);
                case TeamUtil.AttachmentCardCodeSnippetType:
                    if (messageContent == null) return null;
                    return messageContent.CodeSnippets.TryGetValue(attachment.Id, out var codeSnippet) ? TeamsMessageUtility.GenerateCodeSnippetMessage(codeSnippet) : null;
                default:
                    logger.Warn("Unsupported attachment type: [{0}].", attachment.ContentType);
                    return null;
            }
        }

        private string GenerateFileLink(Attachment attachment)
        {
            var extension = attachment.Name.Split('.').Last().ToLower();
            var channelFilesUrl = _CurrentChannel?.CurrenIsPrivateChannelSite ?? false ? _CurrentChannel.FilesFolderUrl : _GroupSiteFilesUrl;
            var isPrivateChannel = _CurrentChannel.IsPrivateChannel();
            return imageExtensions.Contains(extension)
                ? $"<div align=\"center\"><img alt=\"{attachment.Name}\" src=\"{TeamsMessageUtility.ReplaceAttactmentUrl(channelFilesUrl, attachment.ContentUrl, isPrivateChannel)}\"></img></div>"
                : $"<div class=\"file-container\"><a href=\"{TeamsMessageUtility.ReplaceAttactmentUrl(channelFilesUrl, attachment.ContentUrl, isPrivateChannel)}\" target=\"_blank\">{attachment.Name}</a></div>";
        }

        private string GenerateHeroCard(Attachment attachment)
        {
            var template = TeamHtmlResources.PostCard_HeroCardTemplate_html;
            try
            {
                var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(attachment.Content);
                var tap = GetValue<Dictionary<string, object>>(json, "tap");
                var title = GetValue(json, "title");
                var text = GetValue(json, "text");
                var images = GetValue<object[]>(json, "images");
                var subtitle = GetValue(json, "subtitle");
                var buttons = GetValue<object[]>(json, "buttons");

                var cardAppType = GetCardAppType(tap);

                return string.Format(template, title, subtitle, GenrateContentImg(images), text, GenrateActionButton(buttons), cardAppType, cardAppIconMapping[cardAppType]);
            }
            catch (Exception ex)
            {
                logger.Warn("Generate HeroCard failed. Content: {0}. Error: {1}.", attachment.Content, ex);
            }
            return template;
        }

        private string GetCardAppType(Dictionary<string, object> tap)
        {
            if (tap == null) return cardAppPlaces;

            var value = tap["value"].ToString();
            if (value.EndsWith("?ctsrc=TEAMSWEATHER"))
            {
                return cardAppWeather;
            }
            if (value.StartsWith("https://tech.gmw.cn/"))
            {
                return cardAppNews;
            }
            return cardAppUnknown;
        }

        private object GenrateContentImg(object[] images)
        {
            if (images == null || images.Length == 0) return null;

            var imgDic = images.First() as Dictionary<string, object>;
            return $"<img alt=\"{GetValue(imgDic, "alt")}\" src=\"{GetValue(imgDic, "url")}\">";
        }

        private object GenrateActionButton(object[] buttons)
        {
            if (buttons == null || buttons.Length == 0) return null;

            var buttonDic = buttons.First() as Dictionary<string, object>;
            var btnTitle = buttonDic["title"];
            var btnUrl = buttonDic["value"];
            return $"<button onclick=\"window.open('{btnUrl}')\">{btnTitle}</button>";
        }

        public static object GetValue(Dictionary<string, object> json, string key)
        {
            if (json.TryGetValue(key, out object obj))
            {
                return obj;
            }
            return null;
        }

        private T GetValue<T>(Dictionary<string, object> json, string key)
        {
            if (json.TryGetValue(key, out var obj))
            {
                try
                {
                    return (T)obj;
                }
                catch
                {
                    return default(T);
                }
            }
            return default(T);
        }
    }
}