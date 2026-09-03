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

namespace ExchangeUtility.Graph.Teams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft365Backup.DataBuilder.TeamHtml;
    using AvePoint.Wrapper.Common;
    using ExchangeCommonWrapper;
    using AvePoint.RA.CommonUtil;
    using System.Text;

    public class AttachmentMiddleware : TeamsMessageMiddleware
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(AttachmentMiddleware));

        public override void Invoke(TeamsMessageContext context)
        {
            _ = context.TeamService ?? throw new ArgumentNullException(nameof(context.TeamService));
            _ = context.ChannelContext ?? throw new ArgumentNullException(nameof(context.ChannelContext));
            if (string.IsNullOrEmpty(context.ChannelContext.GroupId)) throw new ArgumentNullException(nameof(context.ChannelContext.GroupId));
            if (string.IsNullOrEmpty(context.ChannelContext.ChannelId)) throw new ArgumentNullException(nameof(context.ChannelContext.ChannelId));

            if (context.Message.Attachments == null || context.Message.Attachments.Count == 0)
            {
                Next?.Invoke(context);
                return;
            }

            context.Message.Attachments.ForEach(attachment =>
            {
                if (!context.Message.Body.Content.Contains(attachment.Id))
                {
                    return;
                }

                if (attachment.ContentType == TeamUtil.AttachmentReferenceType && attachment.Id.Length != 36)
                {
                    context.Message.Body.Content = context.Message.Body.Content.Replace($"<attachment id=\"{attachment.Id}\"></attachment>", $"<span>{attachment.Name}</span>");
                    return;
                }

                List<ChannelTab> tabs = null;
                try
                {
                    switch (attachment.ContentType)
                    {
                        case TeamUtil.AttachmentReferenceType:
                            // 目的端不存在附件而被还原或者被Overrite后附件ID改变，获取有困难，所以先不处理，直接用Url
                            attachment.ContentUrl = TeamsMessageUtility.ReplaceAttactmentUrl(context.ChannelContext.ChannelFilesUrl, attachment.ContentUrl, context.ChannelContext.IsPrivate);
                            break;
                        case TeamUtil.AttachmentMeetingType:
                            if (context.Message.MessageContent != null && context.Message.MessageContent.Meetings != null)
                            {
                                var (title, basicInfo) = TeamsMessageUtility.GenerateMeetingMessage(context.Message.MessageContent, attachment);
                                context.Message.MessageContent.HostedContents = null;
                                var contents = context.Message.Body.Content.Split("<attachment");
                                var attachmentPart = contents[1].Replace($"id=\"{attachment.Id}\"></attachment>",
                                        string.Format(TeamHtmlResources.MeetingAsPostTemplate_html, title, basicInfo));
                                context.Message.Body.Content = new StringBuilder().Append(attachmentPart).Append(contents[0]).ToString();
                            }
                            break;
                        case TeamUtil.AttachmentCardCodeSnippetType:
                            CodeSnippetContent codeSnippet = null;
                            if (context.Message.MessageContent?.CodeSnippets?.TryGetValue(attachment.Id, out codeSnippet) ?? false)
                            {
                                context.Message.Body.Content = context.Message.Body.Content
                                    .Replace($"<attachment id=\"{attachment.Id}\"></attachment>", TeamsMessageUtility.GenerateCodeSnippetMessage(codeSnippet));
                            }
                            break;
                        case TeamUtil.AttachmentTabType:
                            if (tabs == null)
                            {
                                tabs = context.TeamService.GetChannelTabs(context.ChannelContext.GroupId, context.ChannelContext.ChannelId) ?? new List<ChannelTab>();
                            }
                            var destinationTab = tabs.FirstOrDefault(t => attachment.Id.Contains(t.Id));
                            if (destinationTab != null)
                            {
                                var content = TeamHtmlResources.FileWithoutIconTemplate_html.Replace("fileUrl", destinationTab.WebUrl).Replace("fileName", destinationTab.DisplayName);
                                context.Message.Body.Content = context.Message.Body.Content.Replace($"<attachment id=\"{attachment.Id}\"></attachment>", content);
                            }
                            break;
                        case TeamUtil.AttachmentMessageReference:
                            MessageReference messageReference = null;
                            if (context.Message.MessageContent?.MessageReferences?.TryGetValue(attachment.Id, out messageReference) ?? false)
                            {
                                context.Message.Body.Content = context.Message.Body.Content
                                    .Replace($"<attachment id=\"{attachment.Id}\"></attachment>", TeamsMessageUtility.GenerateMessageReference(messageReference));
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Operate attachment error: {0}.", ex);
                }
            });

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(context.Message.Body.Content);
            var attachmentNodeIds = new List<string>();
            htmlDocument.DocumentNode.SelectNodes("//attachment")?.ForEach(attachmentNode =>
            {
                var id = attachmentNode.GetAttributeValue("id", null);
                if (id != null)
                {
                    attachmentNodeIds.Add(id);
                }
            });
            context.Message.Attachments.RemoveAll(a => !attachmentNodeIds.Contains(a.Id));
            context.Message.Body.Content = htmlDocument.DocumentNode.OuterHtml;

            Next?.Invoke(context);
        }
    }
}