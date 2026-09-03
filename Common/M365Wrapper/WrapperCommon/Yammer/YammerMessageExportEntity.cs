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
    #region namespace
    using System.Runtime.Serialization;

    #endregion

    [DataContract]
    public class YammerMessageExportEntity : ExchangeEntityBase
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string RepliedToId { get; set; }
        [DataMember]
        public string ParentId { get; set; }
        [DataMember]
        public string ThreadId { get; set; }
        [DataMember]
        public string ConversationId { get; set; }
        [DataMember]
        public string GroupId { get; set; }
        [DataMember]
        public string GroupName { get; set; }
        [DataMember]
        public string Participants { get; set; }
        [DataMember]
        public bool InPrivateGroup { get; set; }
        [DataMember]
        public bool InPrivateConversation { get; set; }
        [DataMember]
        public string SenderId { get; set; }
        [DataMember]
        public string SenderType { get; set; }
        [DataMember]
        public string SenderName { get; set; }
        [DataMember]
        public string SenderEmail { get; set; }
        [DataMember]
        public string Body { get; set; }
        [DataMember]
        public string DelegateId { get; set; }
        [DataMember]
        public string ApiUrl { get; set; }
        [DataMember]
        public string Attachments { get; set; }
        [DataMember]
        public string DeletedById { get; set; }
        [DataMember]
        public string DeletedByType { get; set; }
        [DataMember]
        public string CreatedAt { get; set; }
        [DataMember]
        public string DeletedAt { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string HtmlBody { get; set; }
        [DataMember]
        public string MessageType { get; set; }
        [DataMember]
        public string GdprDeleteUrl { get; set; }
    }
}