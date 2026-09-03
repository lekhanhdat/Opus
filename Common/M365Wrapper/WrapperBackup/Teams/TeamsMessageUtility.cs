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
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using Microsoft365Backup.DataBuilder.TeamHtml;
    using AvePoint.Wrapper.Common;
    using ExchangeCommonWrapper;
    using AvePoint.RA.CommonUtil;
    using ExchangeCommonWrapper.Message;
    using Newtonsoft.Json;

    public static class TeamsMessageUtility
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(TeamsMessageUtility));

        public static void HtmlEncodeBody(TeamChatMessage message)
        {
            if (message.Body.ContentType == "text" && !message.Body.Content.Contains("<attachment id="))
            {
                message.Body.Content = HttpUtility.HtmlEncode(message.Body.Content);
            }
        }

        public static string ReplaceAttactmentUrl(string folderUrl, string fileUrl, bool isPrivate)
        {
            if (string.IsNullOrEmpty(folderUrl)) return fileUrl;

            folderUrl = HttpUtility.UrlDecode(folderUrl);
            fileUrl = HttpUtility.UrlDecode(fileUrl);

            try
            {
                var sdIndex = fileUrl.IndexOf(ExchangeConstants.SharedDocuments);
                if (sdIndex <= 0)
                {
                    return fileUrl;
                }

                var partUrl = fileUrl.Substring(sdIndex + ExchangeConstants.SharedDocuments.Length + 1);
                if (!isPrivate)
                {
                    return folderUrl + '/' + partUrl;
                }

                var index = partUrl.IndexOf('/');
                return index > 0 ? folderUrl + partUrl.Substring(index) : fileUrl;
            }
            catch (Exception ex)
            {
                logger.Warn("Analyzing url failed. Error: {0}.", ex);
                return fileUrl;
            }
        }

        public static (string Title, string BasicInfo) GenerateMeetingMessage(MessageContent messageContent, Attachment attachment) =>
            messageContent.Meetings.TryGetValue(attachment.Id, out var meeting)
                ? meeting == null
                    ? ($"(Cancelled) {attachment.Name}", string.Empty)
                    : (meeting.Subject, DateTime.Parse(meeting.Start.DateTime).ToString("dddd,MMMM dd,yyyy @ HH:mm tt", CultureInfo.GetCultureInfo("en-us").NumberFormat))
                : (attachment.Name, string.Empty);

        public static string GenerateCodeSnippetMessage(CodeSnippetContent codeSnippet)
        {
            var body = new StringBuilder();
            if (!string.IsNullOrEmpty(codeSnippet.Content))
            {
                var lines = codeSnippet.Content.Split('\n').ToList();
                for (int i = 0; i < lines.Count; i++)
                {
                    body.Append(string.Format(TeamHtmlResources.CodeSnippetBodyTemplate_html, (i + 1).ToString(), HttpUtility.HtmlEncode(lines[i])));
                }
            }
            var language = codeSnippet.Language?.Replace("CSharp", "C#").Replace("ASP", "ASP.NET").Replace("CPP", "C++");
            var head = string.Join(' ', codeSnippet.Name, language).Trim(' ');
            return string.Format(TeamHtmlResources.CodeSnippetTemplate_html, head, body.ToString());
        }

        public static string GenerateMessageReference(MessageReference messageReference)
        {
            return string.Format(
                TeamHtmlResources.MessageRefrence_html,
                messageReference.MessageSender.DisplayName,
                messageReference.MessagePreview,
                string.IsNullOrEmpty(messageReference.CreatedDateTime) ? string.Empty : DateTime.Parse(messageReference.CreatedDateTime).ToUniversalTime().ToString("dd/MM/yyyy, hh:mm tt (UTC)"));
        }

        public static string SplitGraphApiUri(string url)
        {
            var uri = new Uri(url);
            var paths = uri.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join("/", paths).Replace(paths[0], string.Empty);
        }

        public static bool HasHostedContentNode(HtmlNode node) =>
            node.Attributes.Contains("src")
            && !string.IsNullOrEmpty(node.Attributes["src"].Value)
            && node.Attributes["src"].Value.Contains("graph.microsoft.")
            && node.Attributes["src"].Value.Contains("hostedContents");

        public static bool HasHostedContentText(string url) =>
            !string.IsNullOrEmpty(url)
            && url.Contains("hostedContents");

        public static string DropHtmlLabel(this string htmlString)
        {
            htmlString = htmlString.Replace("\r\n", "");
            htmlString = Regex.Replace(htmlString, @"<script.*?</script>", "", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"<style.*?</style>", "", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"<.*?>", "", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"-->", "", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"<!--.*", "", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(nbsp|#160);", "", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            htmlString = Regex.Replace(htmlString, @"&#(\d+);", "", RegexOptions.IgnoreCase);
            htmlString = htmlString.Replace("\r\n", "");
            return htmlString;
        }

        public static bool IsSystemMessage(this string type) => type == TeamsConst.SystemEvenMessageType || type == TeamsConst.UnknownFutureValueMessageType;

        public static bool IsDeleted(this TeamChatMessage message) => !string.IsNullOrEmpty(message.DeletedDateTime) || string.IsNullOrEmpty(message.Body?.Content);

        public static TeamChatMessage Merge(this TeamChatMessage source, TeamChatMessage target, bool isTopic)
        {
            if (source == null) return target;
            if (target == null) return source;

            if (target.Attachments?.Count > 0)
            {
                source.Attachments ??= new List<Attachment>();
                source.Attachments.AddRange(target.Attachments);
            }

            if (target.MessageContent != null)
            {
                if (source.MessageContent == null)
                {
                    source.MessageContent = target.MessageContent;
                }
                else
                {
                    if (target.MessageContent.HostedContents?.Count > 0)
                    {
                        source.MessageContent.HostedContents = source.MessageContent.HostedContents ?? new List<HostedContent>();
                        source.MessageContent.HostedContents.AddRange(target.MessageContent.HostedContents);
                    }
                    if (target.MessageContent.Meetings?.Count > 0)
                    {
                        source.MessageContent.Meetings = source.MessageContent.Meetings ?? new Dictionary<string, EventEntity>();
                        source.MessageContent.Meetings.AddRange(target.MessageContent.Meetings, false);
                    }
                    if (target.MessageContent.CodeSnippets?.Count > 0)
                    {
                        source.MessageContent.CodeSnippets = source.MessageContent.CodeSnippets ?? new Dictionary<string, CodeSnippetContent>();
                        source.MessageContent.CodeSnippets.AddRange(target.MessageContent.CodeSnippets, false);
                    }

                    if (target.MessageContent.MessageReferences?.Count > 0)
                    {
                        source.MessageContent.MessageReferences = source.MessageContent.MessageReferences ?? new Dictionary<string, MessageReference>();
                        source.MessageContent.MessageReferences.AddRange(target.MessageContent.MessageReferences, false);
                    }
                }
            }

            if (target.Mentions?.Count > 0)
            {
                if (source.Mentions == null || source.Mentions.Count == 0)
                {
                    source.Mentions = target.Mentions;
                }
                else
                {
                    var index = 0;

                    MergeMentions(source);

                    MergeMentions(target);

                    void MergeMentions(TeamChatMessage message)
                    {
                        var htmlDocument = new HtmlDocument();
                        htmlDocument.LoadHtml(message.Body.Content);
                        htmlDocument.DocumentNode.SelectNodes("//at").ForEach(m =>
                        {
                            var mention = message.Mentions.FirstOrDefault(sub => sub.Id == m.GetAttributeValue("id", null));
                            if (mention != null)
                            {
                                mention.Id = index.ToString();
                                m.SetAttributeValue("id", mention.Id);
                            }

                            index++;
                        });
                        message.Body.Content = htmlDocument.DocumentNode.OuterHtml;
                    }

                    source.Mentions.AddRange(target.Mentions);
                }
            }

            source.Body.Content += string.Format(isTopic ? TeamHtmlResources.MergeTopicTemplate_html : TeamHtmlResources.MergeReplyTemplate_html, target.Body.Content);

            return source;
        }

        public static string ToPostedTime(this string time) => DateTime.Parse(time).ToPostedTime();

        public static string ToPostedTime(this DateTime time) => time.ToUniversalTime().ToString("MM/dd/yyyy hh:mm tt (UTC)");

        public static string GenerateAnnouncementBanner(TeamChatMessage message)
        {
            if (message.Attachments == null || message.Attachments.Count == 0) return null;

            var attachment = message.Attachments
                .Where(a => a.ContentType.Equals(TeamUtil.AttachmentAnnouncementBannerType))
                .FirstOrDefault();

            if (attachment == null) return null;

            return GenerateAnnouncementBanner(message, attachment);
        }

        private static string GenerateAnnouncementBanner(TeamChatMessage message, Attachment attachment)
        {
            var template = TeamHtmlResources.ColorThemeAnnouncementBannerTemplate_html;
            try
            {
                var announcementBannerContent = JsonConvert.DeserializeObject<AnnouncementBannerContent>(attachment.Content);
                switch (announcementBannerContent.CardImageType)
                {
                    case "colorTheme":
                        var colorThemeContent = JsonConvert.DeserializeObject<ColorThemeContent>(attachment.Content);
                        var (background, foreground) = GetThemeColor(colorThemeContent.CardImageDetails?.ColorTheme ?? string.Empty);
                        return string.Format(template, background, foreground, colorThemeContent.Title);
                    case "uploadedImage":
                        template = TeamHtmlResources.UploadedImageAnnouncementBannerTemplate_html;
                        var uploadedImageContent = JsonConvert.DeserializeObject<UploadImageContent>(attachment.Content);
                        var source = uploadedImageContent.CardImageDetails?.UploadedImageDetail?.CroppedImage?.Source;
                        var hostedContentId = TeamUtil.GetHostedContentIdFromRelativeSource(source);

                        if (message.MessageContent.HostedContents != null && message.MessageContent.HostedContents.Count > 0)
                        {
                            var hostedContent = message.MessageContent.HostedContents.Where(h => h.TemporaryId == hostedContentId).FirstOrDefault();
                            if (hostedContent != null)
                            {
                                var contentBase64 = hostedContent.ContentBytes;
                                return string.Format(template, contentBase64);
                            }
                        }
                        return null;
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("Generate GenerateAnnouncementBanner failed. Content: {0}. Error: {1}.", attachment.Content, ex);
            }
            return null;
        }

        private static (string, string) GetThemeColor(string theme)
        {
            if (TeamUtil.ColorThemeAnnouncementBannerColor.TryGetValue(theme, out (string background, string foreground) value))
            {
                return (value.background, value.foreground);
            }
            return TeamUtil.ColorThemeAnnouncementBannerColor.GetValueOrDefault("periwinkleBlue");
        }

        public static string GenerateConversationReaction(TeamChatMessage message)
        {
            var template = TeamHtmlResources.ConversationReactionTemplate_html;
            var conversationReactionHtml = string.Empty;
            try
            {
                if (message.Reactions?.Count > 0)
                {
                    var reactionGroups = message.Reactions.
                        OrderBy(x =>
                        {
                            _ = DateTime.TryParse(x.CreatedDataTime, out DateTime date);
                            return date;
                        }).
                        GroupBy(x => x.ReactionType).
                        OrderByDescending(x => x.Count()).
                        ThenBy(x => x.First().CreatedDataTime);

                    foreach (var group in reactionGroups)
                    {
                        var reaction = group.First();
                        conversationReactionHtml += string.Format(template, TeamHtmlResources.GetEmojiHtml(reaction.ReactionType, reaction.DisplayName), group.Count());
                    }
                }
            } 
            catch (Exception ex)
            {
                logger.Warn("Generate Conversation Reaction failed. Error: {0}.", ex);
            }
            return conversationReactionHtml;
        }
    }
}