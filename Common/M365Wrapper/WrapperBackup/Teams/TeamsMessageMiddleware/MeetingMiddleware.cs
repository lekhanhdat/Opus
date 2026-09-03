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
    using ExchangeCommonWrapper;
    using Newtonsoft.Json;
    using AvePoint.RA.CommonUtil;

    public class MeetingMiddleware : TeamsMessageMiddleware
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(MeetingMiddleware));
        private static readonly Dictionary<string, string> users = new Dictionary<string, string>();

        public override void Invoke(TeamsMessageContext context)
        {
            _ = context.TeamService ?? throw new ArgumentNullException(nameof(context.TeamService));
            _ = context.ChannelContext ?? throw new ArgumentNullException(nameof(context.ChannelContext));
            if (string.IsNullOrEmpty(context.ChannelContext.GroupId)) throw new ArgumentNullException(nameof(context.ChannelContext.GroupId));

            context.Message.MessageContent.Meetings = new Dictionary<string, EventEntity>();

            context.Message.Attachments?.Where(attachment => attachment.ContentType == TeamUtil.AttachmentMeetingType && !string.IsNullOrEmpty(attachment.Content)).ForEach(attachment =>
            {
                if (context.TeamService4ServiceAccount != null)
                {
                    try
                    {
                        var eventInfo = JsonConvert.DeserializeObject<MeetingContent>(attachment.Content);
                        var meeting = context.TeamService4ServiceAccount.GetEvent(context.ChannelContext.GroupId, eventInfo.ExchangeId);
                        context.Message.MessageContent.Meetings.Add(attachment.Id, meeting);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Get event error: {0}.", ex);
                    }
                }

                var sender = context.Message.From.User?.DisplayName;
                if (sender?.Contains(context.Message.From.User.Id) ?? false)
                {
                    if (users.TryGetValue(context.Message.From.User.Id, out sender))
                    {
                        context.Message.From.User.DisplayName = sender;
                        return;
                    }
                    sender = null;
                    try
                    {
                        sender = context.TeamService.GetUser(context.Message.From.User.Id)?.DisplayName;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Get user error: {0}", ex);
                    }
                    if (sender != null)
                    {
                        context.Message.From.User.DisplayName = sender;
                        users.Add(context.Message.From.User.Id, sender);
                    }
                }
            });

            Next?.Invoke(context);
        }
    }
}