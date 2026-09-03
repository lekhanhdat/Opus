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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft365Backup.DataBuilder.TeamHtml;
    using AvePoint.Metadata;
    
    using ExchangeCommonWrapper;
    using ExchangeUtility.Graph;

    internal class RestoreConversationFromEwsAsHtml : RestoreConversationAsHtml
    {
        public RestoreConversationFromEwsAsHtml(BaseRestoreHelperBatch baseHelper) : base(baseHelper)
        {
        }
        /// <summary>
        /// format:
        ///                                             | In "General" channel  | Has subject
        /// TeamName/TopicId                            |       Y               |     N
        /// TeamName/TopicId/Subject                    |       Y               |     Y
        /// TeamName/ChannelName/TopicId                |       N               |     N
        /// TeamName/ChannelName/TopicId/Subject        |       N               |     Y
        /// </summary>
        /// <returns></returns>
        protected override string GenerateChannelName(MetadataEntity baseEntity)
        {
            logger.Info($"Start to get channel name for uploading conversation file, ChannelName is: [{_CurrentChannel?.DisplayName}].");

            string channelName = null;
            var conversationInfo = baseEntity.Title.Split('/');
            //TeamName/TopicId 
            if (conversationInfo.Length == 2)
            {
                channelName = _GeneralCannelName;
            }
            else if (conversationInfo.Length == 3)
            {
                channelName = Regex.IsMatch(conversationInfo[1], @"[\d]{13}") ? _GeneralCannelName : conversationInfo[1];
            }
            //TeamName/ChannelName/TopicId/Subject
            else if (conversationInfo.Length > 3)
            {
                channelName = conversationInfo[1];
            }

            return string.Equals(channelName, _CurrentChannel?.DisplayName, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(_CurrentChannel?.DisplayName)
                ? channelName
                : _CurrentChannel?.DisplayName ?? string.Empty;
        }

        protected override string GenerateparentFolderName(string channelName) => _TeamsChannels.Contains(channelName) ? channelName : $"General/{channelName}";

        protected override string GetEntityTitle(MetadataEntity baseEntity) => $"{baseEntity.Title}{(char)0x12}{baseEntity.ExchangeId}";

        protected override (ConversationItem Item, Dictionary<string, string> SiteUrlMap) GenerateConversationItem(ExchangeRestoreDataForBatch restoreData, MetadataEntity baseEntity)
        {
            var addtionalProperties = restoreData.TryGetMetadata<MSTeamConversationItem>(AveMetadataType.ExchangeMicrosoftTeamsConversationItem);

            if (Config.IsSkipRestoreConversation)
            {
                // no need message info
                return (new ConversationTopic(), []);
            }

            if (RestoreConfig.TopicItemIds.Contains(baseEntity.ExchangeId))
            {
                return (new ConversationTopic()
                {
                    PostedBy = baseEntity.Sender,
                    PostedTime = baseEntity.SendTime,
                    Body = ReadToEnd(restoreData.RestoreStream),
                    Subject = baseEntity.DisplayPath.Split('\\').LastOrDefault(),
                    Important = addtionalProperties?.Importance == ImportanceM.High,
                }, SiteUrlDic);
            }

            return (new ConversationReply()
            {
                PostedBy = baseEntity.Sender,
                PostedTime = baseEntity.SendTime,
                Body = ReadToEnd(restoreData.RestoreStream),
                Important = addtionalProperties?.Importance == ImportanceM.High,
            }, SiteUrlDic);
        }

        private string ReadToEnd(IRestoreStream restoreStream)
        {
            var body = string.Empty;
            using (var stream = restoreStream.OpenContentStream())
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    body = reader.ReadToEnd();
                }
            }
            return body.Contains(ExchangeConstants.ConversationEmptyBody) ? ExchangeConstants.ConversationDeleteBody : body;
        }
    }
}