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
public class SendMailObj
{
    [JsonProperty("message")]
    public SMMessage Message { get; set; }
    
    [JsonProperty("saveToSentItems")]
    public string SaveToSentItems { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class SMMessage
{
    [JsonProperty("internetMessageId")]
    public string InternetMessageId { get; set; }

    [JsonProperty("subject")]
    public string Subject { get; set; }

    [JsonProperty("body")]
    public MailBody Body { get; set; }

    [JsonProperty("toRecipients")]
    public List<MailRecipients> ToRecipients { get; set; }

    [JsonProperty("ccRecipients")]
    public List<MailRecipients> CcRecipients { get; set; }
}

public class MailBody
{
    [JsonProperty("contentType")]
    public string ContentType { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; }
}

public class MailRecipients
{
    [JsonProperty("emailAddress")]
    public MailEmailAdress EmailAdress { get; set; }
}

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class MailEmailAdress
{
    [JsonProperty("address")]
    public string Address { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; }
}