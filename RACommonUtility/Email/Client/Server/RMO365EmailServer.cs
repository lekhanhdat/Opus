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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.GraphApi.Mail;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.RACommonUtility.Email.Model;
using Cloud.Sdk.Data.AosModern;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Email.Client.Server
{
    public class RMO365EmailServer : IRMEmailServer
    {

        private static readonly RMRetryer s_retryer = RMRetryerBuilder.CreateBuilder().Build();

        private readonly EmailSenderDefinition _emailSenderDefinition;

        private readonly AppProfileInfo _profile;

        public RMO365EmailServer(EmailSenderDefinition emailSenderDefinition)
        {
            _emailSenderDefinition = emailSenderDefinition;
            _profile = RMAosApiClient.GetProfileById(emailSenderDefinition.AppProfileId).GetAwaiter().GetResult();
        }

        public void AssemblyImages(RMEmailMessage message)
        {
            message.Images.ForEach(image =>
            {
                message.Body = message.Body.Replace($"cid:{image.Id}", $"data:image/gif;base64,{image.Content}");
            });
        }

        public Task SendAsync(RMEmailMessage message)
        {
            var mailManager = new RMGraphMailManager(_profile);
            return s_retryer.RetryAsync(async () =>
                     await mailManager.SendEmail(new RMGraphMailMessageDefinition(
                        _emailSenderDefinition.EmailSender.UserPrincipalName,
                        string.Join(";", message.ToUsers),
                        string.Join(";", message.CcUsers),
                        message.Subject,
                        message.Body
                    )
                )
            );
        }
    }
}
