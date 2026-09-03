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
namespace AvePoint.GCommon.GraphAPI;

using Newtonsoft.Json;
using System.Collections.Generic;

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class GetMessageObj
{
    [JsonProperty("@odata.etag")]
    public string ODataEtag { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("createdDateTime")]
    public string CreatedDateTime { get; set; }
    
    [JsonProperty("lastModifiedDateTime")]
    public string LastModifiedDateTime { get; set; }
    
    [JsonProperty("changeKey")]
    public string ChangeKey { get; set; }
    
    [JsonProperty("categories")]
    public List<string> Categories { get; set; }    
    
    [JsonProperty("receivedDateTime")]
    public string ReceivedDateTime { get; set; }
    
    [JsonProperty("sentDateTime")]
    public string SentDateTime { get; set; }
    
    [JsonProperty("hasAttachments")]
    public bool HasAttachments { get; set; }
    
    [JsonProperty("internetMessageId")]
    public string InternetMessageId { get; set; }
    
    [JsonProperty("subject")]
    public string Subject { get; set; }
    
    [JsonProperty("bodyPreview")]
    public string BodyPreview { get; set; }
    
    [JsonProperty("importance")]
    public string Importance { get; set; }
    
    [JsonProperty("parentFolderId")]
    public string ParentFolderId { get; set; }
    
    [JsonProperty("conversationId")]
    public string ConversationId { get; set; }
    
    [JsonProperty("conversationIndex")]
    public string ConversationIndex { get; set; }
    
    [JsonProperty("isDeliveryReceiptRequested")]
    public bool IsDeliveryReceiptRequested { get; set; }
    
    [JsonProperty("isReadReceiptRequested")]
    public bool IsReadReceiptRequested { get; set; }
    
    [JsonProperty("isRead")]
    public bool IsRead { get; set; }
    
    [JsonProperty("isDraft")]
    public bool IsDraft { get; set; }
    
    [JsonProperty("webLink")]
    public string WebLink { get; set; }
    
    [JsonProperty("inferenceClassification")]
    public string InferenceClassification { get; set; }
    
    [JsonProperty("body")]
    public MailBody Body { get; set; }
    
    [JsonProperty("sender")]
    public MailRecipients Sender { get; set; }
    
    [JsonProperty("from")]
    public MailRecipients From { get; set; }
    
    [JsonProperty("toRecipients")]
    public List<MailRecipients> ToRecipients { get; set; }
    
    [JsonProperty("ccRecipients")]
    public List<MailRecipients> CcRecipients { get; set; }
    
    [JsonProperty("bccRecipients")]
    public List<MailRecipients> BccRecipients { get; set; }
    
    [JsonProperty("replyTo")]
    public List<MailRecipients> ReplyTo { get; set; }
    
    [JsonProperty("flag")]
    public MailFollowFlag Flag { get; set; }
}

public class MailFollowFlag
{
    [JsonProperty("completedDateTime")]
    public DateTimeTimeZone CompletedDateTime { get; set; }
    
    [JsonProperty("dueDateTime")]
    public DateTimeTimeZone DueDateTime { get; set; }
    
    [JsonProperty("flagStatus")]
    public string FlagStatus { get; set; }
    
    [JsonProperty("startDateTime")]
    public DateTimeTimeZone StartDateTime { get; set; }
}