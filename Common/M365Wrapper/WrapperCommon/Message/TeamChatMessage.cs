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

namespace ExchangeCommonWrapper
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class TeamChatMessage
    {
        [DataMember]
        public string OdataContext { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string ReplyToId { get; set; }

        [DataMember]
        public string Etag { get; set; }

        [DataMember]
        public string MessageType { get; set; }

        [DataMember]
        public string CreatedDateTime { get; set; }

        [DataMember]
        public string LastModifiedDateTime { get; set; }

        [DataMember]
        public string DeletedDateTime { get; set; }

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public string Summary { get; set; }

        [DataMember]
        public string ChatId { get; set; }

        [DataMember]
        public string Importance { get; set; }

        [DataMember]
        public string Locale { get; set; }

        [DataMember]
        public string WebUrl { get; set; }

        [DataMember]
        public string PolicyViolation { get; set; }

        [DataMember]
        public From From { get; set; }

        [DataMember]
        public Body Body { get; set; }

        [DataMember]
        public List<Attachment> Attachments { get; set; }

        [DataMember]
        public List<Mantion> Mentions { get; set; }

        [DataMember]
        public List<Reaction> Reactions { get; set; }

        [DataMember]
        public MessageContent MessageContent { get; set; }

        public ChatEntity Chat { get; set; }

        public string RepliesContext { get; set; }
        public int? RepliesCount { get; set; }
        public string RepliesNextLink { get; set; }
        public IEnumerable<TeamChatMessage> Replies { get; set; }
    }

    [DataContract]
    public class User
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string UserIdentityType { get; set; }
    }

    [DataContract]
    public class From
    {
        [DataMember]
        public Application Application { get; set; }

        [DataMember]
        public string Device { get; set; }

        [DataMember]
        public Conversation Conversation { get; set; }

        [DataMember]
        public User User { get; set; }

        public string DisplayName => User?.DisplayName ?? Application?.DisplayName ?? Conversation?.DisplayName;
    }

    [DataContract]
    public class Body
    {
        [DataMember]
        public string ContentType { get; set; }

        [DataMember]
        public string Content { get; set; }
    }

    [DataContract]
    public class Mantion
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string MentionText { get; set; }
        [DataMember]
        public Mentioned Mentioned { get; set; }
    }

    [DataContract]
    public class Mentioned
    {
        [DataMember]
        public Application Application { get; set; }

        [DataMember]
        public string Device { get; set; }

        [DataMember]
        public Conversation Conversation { get; set; }

        [DataMember]
        public User MUser { get; set; }

        public string DisplayName => MUser?.DisplayName ?? Conversation?.DisplayName ?? Application?.DisplayName ?? string.Empty;
    }

    [DataContract]
    public class Reaction
    {
        [DataMember]
        public string ReactionType { get; set; }
        [DataMember]
        public User ReUser { get; set; }
        [DataMember]
        public string CreatedDataTime { get; set; }
        [DataMember]
        public string DisplayName { get; set; }

    }

    [DataContract]
    public class TCMIdentySet
    {
        [DataMember]
        public string Application { get; set; }
        [DataMember]
        public string Device { get; set; }
        [DataMember]
        public string Conversation { get; set; }
        [DataMember]
        public User User { get; set; }
    }

    [DataContract]
    public class Application
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string ApplicationIdentityType { get; set; }
    }

    [DataContract]
    public class Conversation
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string ConversationIdentityType { get; set; }
    }

    public class EmptyTeamChatMessage : TeamChatMessage
    {

    }
}