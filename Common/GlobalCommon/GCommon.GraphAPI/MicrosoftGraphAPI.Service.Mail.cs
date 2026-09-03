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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class MicrosoftGraphAPIService
{
    private readonly string[] SelectProperties_MailMessage = { "id", "internetMessageId", "subject", "createdDateTime", "isDraft"};

    public void SendMail(string userIdOrUPN, SendMailObj sendMailObj)
    {
        var sendMial = new SendMail(this.resourceUrl, this.refreshAccessToken, userIdOrUPN, sendMailObj, this.RetryController);
        sendMial.GetApiResult();
    }

    public void ReplyMessage(string userId, string messageId, ReplyMessageObj replyMessageObj)
    {
        var sendMial = new ReplyMailMessage(this.resourceUrl, this.refreshAccessToken, userId, messageId, replyMessageObj, this.RetryController);
        sendMial.GetApiResult();
    }

    public IList<GetMessageObj> ListMessageWithInternetMessageId(string userId, string internetMessageId)
    {
        var listMessage = new ListMessages(this.resourceUrl, this.refreshAccessToken, userId, this.RetryController);
        listMessage.QueryParameters.Select(SelectProperties_MailMessage);
        listMessage.QueryParameters.Filter($"internetMessageId eq '{internetMessageId}'");
        return listMessage.GetApiResult();
    }
}
