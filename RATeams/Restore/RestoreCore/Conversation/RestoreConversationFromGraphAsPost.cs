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
    using AvePoint.Metadata;
    
    using ExchangeCommonWrapper;
    using ExchangeUtility.Graph;
    using ExchangeUtility.Graph.Teams;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    internal class RestoreConversationFromGraphAsPost : RestoreConversationAsPost
    {
        public RestoreConversationFromGraphAsPost(BaseRestoreHelperBatch baseHelper) : base(baseHelper)
        {
        }
        protected override TeamChatMessage GenerateMessage(ExchangeRestoreDataForBatch restoreData)
        {
            var message = restoreData.TryGetMetadata<TeamChatMessage>(AveMetadataType.ExchangeMicrosoftTeamsConversationItem);

            if (Config.IsSkipRestoreConversation)
            {
                // no need message info, just return the message type to check if system message.
                return message;
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

            TeamsMessageUtility.HtmlEncodeBody(message);

            var context = new TeamsMessageContext(message)
            {
                TeamService = TeamsService,
                ChannelContext = new ChannelContext
                {
                    GroupId = _GroupId,
                    ChannelId = _CurrentChannel?.Id,
                    ChannelFilesUrl = _CurrentChannel?.CurrenIsPrivateChannelSite ?? false ? _CurrentChannel.FilesFolderUrl : _GroupSiteFilesUrl,
                    IsPrivate = _CurrentChannel?.IsPrivateChannel() ?? false
                }
            };

            var teamsMessageMiddlewares = new List<TeamsMessageMiddleware>
            {
                new ReplaceStickerImageMiddleware(),
                new AttachmentMiddleware(),
                new AttachmentUrlMiddleware(),
                new MentionMiddleware()
            };

            if (!UseMigrationMode)
            {
                teamsMessageMiddlewares.Add(new HeadingMiddleware());
            }

            TeamsMessageMiddlewareBuilder
                .Register(teamsMessageMiddlewares)
                .Invoke(context);

            if (!UseMigrationMode)
            {
                context.Message.From = null;
                context.Message.CreatedDateTime = null;
            }

            return context.Message;
        }
    }
}