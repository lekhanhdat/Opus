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

    using AvePoint.Wrapper.Common;

    using ExchangeCommonWrapper;

    using AvePoint.RA.CommonUtil;

    public class CopyMessageMiddleware : TeamsMessageMiddleware
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(CopyMessageMiddleware));

        public override void Invoke(TeamsMessageContext context)
        {
            _ = context.TeamService ?? throw new ArgumentNullException(nameof(context.TeamService));

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(context.Message.Body.Content);

            OperateDivNodes(context, htmlDocument);

            OperateANodes(context, htmlDocument);

            context.Message.Body.Content = htmlDocument.DocumentNode.OuterHtml;

            Next?.Invoke(context);
        }

        private void OperateDivNodes(TeamsMessageContext context, HtmlDocument htmlDocument)
        {
            var divNodes = htmlDocument.DocumentNode.SelectNodes("//div[@itemprop='teams-copy-link']");
            if (divNodes == null || divNodes.Count == 0)
            {
                return;
            }
            logger.Info("This is a copy message with the div of [teams-copy-link].");
            var isLast = true;
            for (var index = divNodes.Count - 1; index >= 0; index--)
            {
                var div = divNodes[index];
                if (div.Attributes.Contains("itemprop") && div.Attributes["itemprop"].Value.Equals("teams-copy-link"))
                {
                    var moreHtml = string.Empty;
                    if (div.HasChildNodes)
                    {
                        TeamChatMessage copyMessage = null;
                        var username = string.Empty;
                        foreach (var child in div.ChildNodes)
                        {
                            if (!child.Attributes.Contains("href"))
                            {
                                moreHtml += $"<div>{child.InnerHtml}</div>";
                            }
                        }
                        foreach (var child in div.ChildNodes)
                        {
                            if (child.Attributes.Contains("href") && child.Attributes["href"].Value.Contains("/message/"))
                            {
                                try
                                {
                                    var href = child.Attributes["href"].Value;
                                    var start = href.IndexOf("/message/") + "/message/".Length;
                                    var end = href.IndexOf("?");
                                    var channelId = href.Substring(start, end - start).Split(new char[] { '/' })[0];
                                    var messageId = href.Substring(start, end - start).Split(new char[] { '/' })[1];
                                    start = href.IndexOf("groupId=") + "groupId=".Length;
                                    end = href.IndexOf("&amp;parentMessageId=");
                                    var groupId = href.Substring(start, end - start);
                                    start = href.IndexOf("parentMessageId=") + "parentMessageId=".Length;
                                    end = href.IndexOf("&amp;teamName=");
                                    var parentId = href.Substring(start, end - start);

                                    copyMessage = GenerateCopyMessage(context, htmlDocument, parentId, groupId, channelId, messageId);

                                    username = copyMessage.From?.DisplayName;
                                }
                                catch (Exception ex)
                                {
                                    logger.Error("Get copy message error: {0}.", ex);
                                }
                                break;
                            }
                        }
                        if (copyMessage != null)
                        {
                            if (isLast)
                            {
                                isLast = false;
                                div.Attributes.Add("style", "border-left: .3rem solid #6264a7;padding: 0.3rem 0 0.3rem 0;");
                            }
                            else
                            {
                                div.Attributes.Add("style", "border-left: .3rem solid #6264a7;padding: 0.3rem 0 0.3rem 0;margin-bottom: 10px;");
                            }
                            div.InnerHtml = $"<div style=\"margin-left: 1rem;\">{username}:{(!string.IsNullOrEmpty(copyMessage.Subject) ? $"<div style=\"font-weight:600;font-size:1.8rem;\">{copyMessage.Subject}</div>" : "")}<div>{copyMessage.Body.Content}</div></div>";
                            if (div.ParentNode.ChildNodes.Count > 1)
                            {
                                var newChildren = new List<HtmlNode>();
                                var findCopy = false;
                                foreach (var node in div.ParentNode.ChildNodes)
                                {
                                    if (node.InnerHtml.Equals(div.InnerHtml))
                                    {
                                        findCopy = true;
                                        newChildren.Add(node);
                                        continue;
                                    }
                                    if (!findCopy)
                                    {
                                        newChildren.Add(node);
                                    }
                                    if (findCopy && node.OuterHtml.StartsWith("<div itemprop=\"teams-copy-link\">"))
                                    {
                                        node.Attributes.Add("style", "margin-left: 1rem;font-style: italic;");
                                        if (!string.IsNullOrEmpty(moreHtml))
                                        {
                                            node.InnerHtml += moreHtml;
                                        }
                                        div.AppendChild(node);
                                        findCopy = false;
                                    }
                                }
                                var topNode = div.ParentNode;
                                topNode.ChildNodes.Clear();
                                for (int i = 0; i < newChildren.Count; i++)
                                {
                                    topNode.AppendChild(newChildren[i]);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OperateANodes(TeamsMessageContext context, HtmlDocument htmlDocument)
        {
            var aNodes = htmlDocument.DocumentNode.SelectNodes("//a");
            if (aNodes == null || aNodes.Count == 0)
            {
                return;
            }
            logger.Info("This is a copy message with a label.");
            var messageLink = new Dictionary<string, string>();
            foreach (var a in aNodes)
            {
                TeamChatMessage copyMessage = null;
                var username = string.Empty;
                if (a.Attributes.Contains("href") && a.Attributes["href"].Value.Contains("/l/message/"))
                {
                    try
                    {
                        var href = a.Attributes["href"].Value;
                        var start = href.IndexOf("/message/") + "/message/".Length;
                        var end = href.IndexOf("?");
                        var channelId = href.Substring(start, end - start).Split(new char[] { '/' })[0];
                        var messageId = href.Substring(start, end - start).Split(new char[] { '/' })[1];
                        start = href.IndexOf("groupId=") + "groupId=".Length;
                        end = href.IndexOf("&amp;parentMessageId=");
                        var groupId = end > 0 ? href.Substring(start, end - start) : href.Substring(start);
                        var parentId = string.Empty;
                        if (href.Contains("parentMessageId="))
                        {
                            start = href.IndexOf("parentMessageId=") + "parentMessageId=".Length;
                            end = href.IndexOf("&amp;teamName=");
                            parentId = href.Substring(start, end - start);
                        }

                        copyMessage = GenerateCopyMessage(context, htmlDocument, parentId, groupId, channelId, messageId);

                        username = copyMessage.From?.DisplayName;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Get copy message error: {0}.", ex);
                    }
                }
                if (copyMessage != null)
                {
                    var guid = Guid.NewGuid().ToString();
                    a.Attributes.RemoveAll();
                    a.Attributes.Add("id", guid);
                    a.InnerHtml = "";
                    messageLink.Add($"<a id=\"{guid}\"></a>", $"<div style=\"border-left: .3rem solid #6264a7;padding: 0.3rem 0 0.3rem 0;display:block;\"><div style=\"margin-left: 1rem;\">{username}:{(!string.IsNullOrEmpty(copyMessage.Subject) ? $"<div style=\"font-weight:600;font-size:1.8rem;\">{copyMessage.Subject}</div>" : "")}<div>{copyMessage.Body.Content}</div></div></div>");
                }
            }
            foreach (var kv in messageLink)
            {
                htmlDocument.LoadHtml(htmlDocument.DocumentNode.OuterHtml.Replace(kv.Key, kv.Value));
            }
        }

        private TeamChatMessage GenerateCopyMessage(TeamsMessageContext context, HtmlDocument htmlDocument, string parentId, string groupId, string channelId, string messageId)
        {
            var copyMessage = string.IsNullOrEmpty(parentId) || parentId == messageId
                ? context.TeamService.GetChannelMessage(groupId, channelId, messageId)
                : context.TeamService.GetChannelMessageReply(groupId, channelId, parentId, messageId);

            TeamsMessageUtility.HtmlEncodeBody(copyMessage);

            AddAttachments(context, copyMessage);

            AddMentions(context, htmlDocument, copyMessage);

            return copyMessage;
        }

        private void AddAttachments(TeamsMessageContext context, TeamChatMessage copyMessage)
        {
            if (copyMessage.Attachments == null || copyMessage.Attachments.Count == 0) return;

            if (context.Message.Attachments == null)
            {
                context.Message.Attachments = copyMessage.Attachments;
            }
            else
            {
                context.Message.Attachments.AddRange(copyMessage.Attachments);
            }
        }

        private void AddMentions(TeamsMessageContext context, HtmlDocument htmlDocument, TeamChatMessage copyMessage)
        {
            if (copyMessage.Mentions == null || copyMessage.Mentions.Count == 0) return;

            if (context.Message.Mentions == null)
            {
                context.Message.Mentions = copyMessage.Mentions;
            }
            else
            {
                var atNodes = htmlDocument.DocumentNode.SelectNodes("//at");
                if (atNodes != null)
                {
                    foreach (var at in atNodes)
                    {
                        if (at.Attributes.Contains("id"))
                        {
                            at.Attributes["id"].Value = (Convert.ToInt32(at.Attributes["id"].Value) + copyMessage.Mentions.Count).ToString();
                        }
                    }
                }
                context.Message.Mentions.ForEach(m => m.Id += copyMessage.Mentions.Count);
                context.Message.Mentions.AddRange(copyMessage.Mentions);
            }
        }
    }
}