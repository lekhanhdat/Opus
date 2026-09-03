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

namespace AvePoint.GCommon.GraphAPI
{
    using Newtonsoft.Json;

    public class ChannelMessageCollection : PageableCollection<ChannelRootMessage>
    {
    }

    public class ChannelMessageReplyCollection : PageableCollection<ChatMessage>
    {
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ChatMessage : EntityBase
    {
        [JsonProperty("@odata.context", NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)]
        public string OdataContext { get; set; }

        [JsonProperty("replyToId")]
        public string ReplyToId { get; set; }

        [JsonProperty("etag")]
        public string Etag { get; set; }

        [JsonProperty("messageType")]
        public string MessageType { get; set; }

        [JsonProperty("createdDateTime")]
        public string CreatedDateTime { get; set; }

        [JsonProperty("lastModifiedDateTime")]
        public string LastModifiedDateTime { get; set; }

        [JsonProperty("lastEditedDateTime")]
        public string LastEditedDateTime { get; set; }

        [JsonProperty("deletedDateTime")]
        public string DeletedDateTime { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("chatId")]
        public string ChatId { get; set; }

        [JsonProperty("importance")]
        public string Importance { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("webUrl")]
        public string WebUrl { get; set; }

        [JsonProperty("policyViolation")]
        public object PolicyViolation { get; set; }

        [JsonProperty("from")]
        public CMIdentitySet From { get; set; }

        [JsonProperty("body")]
        public CMBody Body { get; set; }

        [JsonProperty("attachments")]
        public CMAttachment[] Attachments { get; set; }

        [JsonProperty("mentions")]
        public CMMention[] Mentions { get; set; }

        [JsonProperty("reactions")]
        public CMReaction[] Reactions { get; set; }

        [JsonProperty("hostedContents", NullValueHandling = NullValueHandling.Ignore, Required = Required.Default)]
        public CMHostedContents[] HostedContents { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ChannelRootMessage : ChatMessage
    {
        [JsonProperty("replies@odata.context")]
        public string RepliesOdataContext { get; set; }

        [JsonProperty("replies")]
        public ChatMessage[] Replies { get; set; }

        [JsonProperty("replies@odata.count")]
        public int? RepliesOdataCount { get; set; }

        [JsonProperty("replies@odata.nextLink")]
        public string RepliesOdataNextLink { get; set; }
    }

    public class CMBody : EntityBase
    {
        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class CMAttachment : EntityBase
    {
        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("contentUrl")]
        public string ContentUrl { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }
    }

    public class CMMention : EntityBase
    {
        [JsonProperty("mentionText")]
        public string MentionText { get; set; }

        [JsonProperty("mentioned")]
        public CMIdentitySet Mentioned { get; set; }
    }

    public class CMIdentitySet : EntityBase
    {
        [JsonProperty("application")]
        public CMApplication Application { get; set; }

        [JsonProperty("device")]
        public object Device { get; set; }

        [JsonProperty("conversation")]
        public CMConversation Conversation { get; set; }

        [JsonProperty("user")]
        public CMIdentitySetUser User { get; set; }
    }

    public class CMIdentitySetUser : EntityBase
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("userIdentityType")]
        public string UserIdentityType { get; set; }
    }

    public class CMReaction : EntityBase
    {
        [JsonProperty("reactionType")]
        public string ReactionType { get; set; }

        [JsonProperty("createdDateTime")]
        public string CreatedDateTime { get; set; }

        [JsonProperty("user")]
        public CMIdentitySet User { get; set; }
        
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }

    public class CMApplication : EntityBase
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("applicationIdentityType")]
        public string ApplicationIdentityType { get; set; }
    }

    public class CMConversation : EntityBase
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("conversationIdentityType")]
        public string ConversationIdentityType { get; set; }
    }

    public class CMHostedContents : EntityBase
    {
        [JsonProperty("@microsoft.graph.temporaryId")]
        public string TemporaryId { get; set; }

        [JsonProperty("contentBytes")]
        public string ContentBytes { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }
    }
}