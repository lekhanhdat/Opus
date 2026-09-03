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
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class YammerMessageCsv
    {
        public const string Id = "id";
        public const string RepliedtoId = "replied_to_id";
        public const string ParentId = "parent_id";
        public const string ThreadId = "thread_id";
        public const string ConversationId = "conversation_id";
        public const string GroupId = "group_id";
        public const string GroupName = "group_name";
        public const string Participants = "participants";
        public const string InPrivateGroup = "in_private_group";
        public const string InPrivateConversation = "in_private_conversation";
        public const string SenderId = "sender_id";
        public const string SenderType = "sender_type";
        public const string SenderName = "sender_name";
        public const string SenderEmail = "sender_email";
        public const string Body = "body";
        public const string DelegateId = "delegate_id";
        public const string ApiUrl = "api_url";
        public const string Attachments = "attachments";
        public const string DeletedById = "deleted_by_id";
        public const string DeletedByType = "deleted_by_type";
        public const string CreatedAt = "created_at";
        public const string DeletedAt = "deleted_at";
        public const string Title = "title";
        public const string HtmlBody = "html_body";
        public const string MessageType = "message_type";
        public const string GdprDeleteUrl = "gdpr_delete_url";
    }
}