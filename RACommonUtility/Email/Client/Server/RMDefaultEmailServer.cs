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
using AvePoint.GCommon.Contract.Server.ControlPanel.Reporting.UserNotificationSettings.Object;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.RACommonUtility.Email.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Email.Client.Server
{
    public class RMDefaultEmailServer : IRMEmailServer
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMDefaultEmailServer));

        private static readonly NotificationSettingDto s_smtpConfig;

        private static readonly RMRetryer s_retryer = RMRetryerBuilder.CreateBuilder().Build();

        static RMDefaultEmailServer()
        {
            try
            {
                if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
                {
                    var smtpConfigJson = RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.NOTIFICATION_SETTING];
                    s_smtpConfig = SerializerHelper.DeserializeFromXmlString<NotificationSettingDto>(smtpConfigJson);
                }
                else
                {
                    var settingPassword = RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.SENDGRID_KEY];
                    var settingPort = int.TryParse(RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.SENDGRID_PORT], out int port) ? port : 587;
                    var settingSender = RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.SENDGRID_SENDER_ADDRESS];
                    var settingUserName = RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.SENDGRID_ACCOUNT];
                    var settingOugoingMailServer = RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.SENDGRID_SERVER];
                    var settingSenderDisplayName = RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.SENDGRID_SENDER_DISPLAYNAME];
                    s_logger.Info($"Current setting sender display name: {settingSenderDisplayName}");
                    if (string.IsNullOrWhiteSpace(settingUserName) || string.IsNullOrWhiteSpace(settingPassword) || string.IsNullOrWhiteSpace(settingSender) || string.IsNullOrWhiteSpace(settingOugoingMailServer))
                    {
                        s_logger.Info("RMDefaultEmailServer  Read NOTIFICATION_SETTING");
                        var smtpConfigJson = RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.NOTIFICATION_SETTING];
                        s_smtpConfig = SerializerHelper.DeserializeFromXmlString<NotificationSettingDto>(smtpConfigJson);
                    }
                    else
                    {
                        s_logger.Info("RMDefaultEmailServer  Read Others");
                        s_smtpConfig = new NotificationSettingDto
                        {
                            Password = settingPassword,
                            UserName = settingUserName,
                            Sender = settingSender,
                            SenderDisplayName = settingSenderDisplayName,
                            Port = settingPort,
                            OutgoingMailServer = settingOugoingMailServer,
                            SslAuthentication = true,
                            SecurePasswordAuthentication = false,
                        };
                    }
                }
            }
            catch(Exception e)
            {
                s_logger.Error($"An error occurred while initial default email server. Error: {e}");
            }
        }

        public void AssemblyImages(RMEmailMessage message)
        {
            // Don't require process
        }

        public Task SendAsync(RMEmailMessage message)
        {
            var client = GetClient();
            var mailMessage = GetMailMessage(message);
            return s_retryer.RetryAsync(async () => await client.SendMailAsync(mailMessage));
        }

        private static SmtpClient GetClient()
        {
            var useDefaultCredentials = string.IsNullOrWhiteSpace(s_smtpConfig.UserName) || string.IsNullOrWhiteSpace(s_smtpConfig.Password);

            var smtpClient = new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Host = s_smtpConfig.OutgoingMailServer,
                Port = s_smtpConfig.Port,
                UseDefaultCredentials = useDefaultCredentials,
                Timeout = 18_000_000,
                EnableSsl = s_smtpConfig.SslAuthentication
            };

            if (!useDefaultCredentials)
            {
                var credential = new NetworkCredential(s_smtpConfig.UserName, s_smtpConfig.Password);
                smtpClient.Credentials = s_smtpConfig.SecurePasswordAuthentication ?
                    credential.GetCredential(s_smtpConfig.OutgoingMailServer, s_smtpConfig.Port, "NTLM") :
                    credential;
            }

            return smtpClient;
        }

        private static MailMessage GetMailMessage(RMEmailMessage message)
        {
            var alternateView = AlternateView.CreateAlternateViewFromString(message.Body, null, MediaTypeNames.Text.Html);
            foreach (var image in message.Images)
            {
                var imageBytes = Convert.FromBase64String(image.Content);
                var imageStream = new MemoryStream(imageBytes);
                var linkedResource = new LinkedResource(imageStream, $"image/gif")
                {
                    ContentId = image.Id
                };

                alternateView.LinkedResources.Add(linkedResource);
            }

            var mailMessage = new MailMessage
            {
                From = !string.IsNullOrEmpty(s_smtpConfig.SenderDisplayName) ?
                        new MailAddress(s_smtpConfig.Sender, s_smtpConfig.SenderDisplayName)
                        : new MailAddress(s_smtpConfig.Sender),
                Subject = message.Subject,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
                IsBodyHtml = true,
                Body = message.Body,
            };

            message.ToUsers.ForEach(mailMessage.To.Add);
            message.CcUsers.ForEach(mailMessage.CC.Add);
            mailMessage.AlternateViews.Add(alternateView);

            return mailMessage;
        }
    }
}
