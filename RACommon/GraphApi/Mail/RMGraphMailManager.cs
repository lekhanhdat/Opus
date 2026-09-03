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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using Cloud.Sdk.Data.AosModern;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.GraphApi.Mail
{

    public record RMGraphMailMessageDefinition(
        string From,
        string To,
        string Cc,
        string Subject,
        string Body
    );

    public class RMGraphMailManager : RMGraphApiManager
    {

        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMGraphMailManager));

        public RMGraphMailManager(AppProfileInfo profile) : base(profile)
        {}

        public Task SendEmail(RMGraphMailMessageDefinition message)
        {
            var requestUri = $"{GraphEndPoint}/{ApiVersion}/users/{message.From}/sendMail";
            //var mail = BuildMailDefinition(message);

            s_logger.Warn($"Send email uri: [{requestUri}]");

            //s_logger.Warn($"Send email token: [{AccessToken}]");

            //var mailMessage = JsonConvert.SerializeObject(mail);

            return HttpHelper.PostAsync(requestUri, JsonConvert.SerializeObject(BuildMailDefinition(message)), AccessToken);
        }

        private static RMGraphMailDefinition BuildMailDefinition(RMGraphMailMessageDefinition message)
        {
            var mail = new RMGraphMailDefinition
            {
                Message = new RMGraphMailMessage
                {
                    Subject = message.Subject,
                    Body = new RMGraphMailBody
                    {
                        ContentType = "HTML",
                        Content = message.Body
                    },
                },
                SaveToSentItems = false,
            };

            var toUsers = message.To.Split(";", StringSplitOptions.RemoveEmptyEntries)
                .ConvertAll(item =>
                    new RMGraphMailReciver
                    {
                        MailAddress = new RMGraphMailAddress { Address = item.Trim() }
                    }).ToList();

            var ccUsers = message.Cc.Split(";", StringSplitOptions.RemoveEmptyEntries)
                .ConvertAll(item =>
                    new RMGraphMailReciver
                    {
                        MailAddress = new RMGraphMailAddress { Address = item.Trim() }
                    }).ToList();

            mail.Message.ToRecipients = toUsers;
            mail.Message.CcRecipients = ccUsers;

            return mail;
        }
    }
}
