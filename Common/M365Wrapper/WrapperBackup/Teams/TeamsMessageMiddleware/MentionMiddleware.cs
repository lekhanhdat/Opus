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
    using System.Linq;
    using AvePoint.Wrapper.Common;

    public class MentionMiddleware : TeamsMessageMiddleware
    {
        public override void Invoke(TeamsMessageContext context)
        {
            _ = context.ChannelContext ?? throw new ArgumentNullException(nameof(context.ChannelContext));
            if (string.IsNullOrEmpty(context.ChannelContext.GroupId)) throw new ArgumentNullException(nameof(context.ChannelContext.GroupId));

            if (context.Message.Mentions?.Count > 0)
            {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(context.Message.Body.Content);

                htmlDocument.DocumentNode.SelectNodes("//at").ForEach(m =>
                {
                    if (m != null)
                    {
                        var mention = context.Message.Mentions.FirstOrDefault(sub => sub.Id == m.GetAttributeValue("id", null));

                        if (mention?.Mentioned?.Conversation?.ConversationIdentityType == "team")
                        {
                            mention.Mentioned.Conversation.Id = context.ChannelContext.GroupId;
                        }
                        m.InnerHtml = string.IsNullOrEmpty(mention?.MentionText) ? m.InnerHtml : mention.MentionText;
                    }
                });

                context.Message.Body.Content = htmlDocument.DocumentNode.OuterHtml;
            }

            Next?.Invoke(context);
        }
    }
}