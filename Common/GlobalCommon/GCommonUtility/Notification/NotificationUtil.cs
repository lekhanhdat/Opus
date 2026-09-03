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

namespace AvePoint.GCommon.Utility.Notification
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Mail;
    using System.Net.Mime;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Reporting.UserNotificationSettings.Object;
    using AvePoint.GCommon.Utility.TransientFault;
    using RazorEngine;
    using RazorEngine.Templating;
    using Contract.Server.Common.EmailTemplateSettings.Object;
    using AvePoint.GCommon.Utility.Cloud;
    using System.Security.Cryptography;
    using AvePoint.GCommon.Utility.Storage;

    public static class NotificationUtil
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static readonly string EmailTempatePath = Path.Combine(GetBaseDirectory(), @"Config/emailVelocityTemplate");

        private static readonly System.Text.Encoding DefaultEncoding = System.Text.Encoding.UTF8;

        public static void SendSyncEmail(NotificationSettingDto dto, EmailMessageDto emailDto, Dictionary<string, MemoryStream> iamgeFileNames, bool isNewTemplate, int isNeedCopyright, EmailTemplateDto template = null)
        {
            if (dto == null)
            {
                throw new ArgumentNullException("notification setting");
            }
            if (emailDto == null)
            {
                throw new ArgumentNullException("email");
            }
            SmtpClient client = GetSmtpClient(dto);
            MailMessage message = AssemblyEmailMessage(dto, emailDto, iamgeFileNames, isNewTemplate, isNeedCopyright, template);
            var retryStrategy = new IncrementalRetryStrategy(3, new TimeSpan(0, 0, 1), new TimeSpan(0, 0, 5));
            AveRetryPolicy retryPolicy = new AveRetryPolicy<AveTransientErrorCatchAllStrategy>(retryStrategy);
            retryPolicy.ExecuteAction(() => client.Send(message));
        }

        private static SmtpClient GetSmtpClient(NotificationSettingDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException("notification setting");
            }
            SmtpClient sc = new SmtpClient();
            sc.DeliveryMethod = SmtpDeliveryMethod.Network;
            sc.Host = dto.OutgoingMailServer;
            sc.EnableSsl = true;
            //默认是25
            if (dto.Port > 0 && dto.Port < 65536)
            {
                sc.Port = dto.Port;
            }

            if (string.IsNullOrEmpty(dto.UserName) || string.IsNullOrEmpty(dto.Password))
            {
                sc.UseDefaultCredentials = true;
            }
            else
            {
                System.Net.NetworkCredential credetntial = new System.Net.NetworkCredential(dto.UserName, dto.Password);
                sc.UseDefaultCredentials = false;
                if (dto.SecurePasswordAuthentication)
                {
                    sc.Credentials = credetntial.GetCredential(sc.Host, sc.Port, "NTLM");
                }
                else
                {
                    sc.Credentials = credetntial;
                }
            }
            sc.Timeout = 18000000;
            sc.EnableSsl = dto.SslAuthentication;
            return sc;
        }

        private static MailMessage AssemblyEmailMessage(NotificationSettingDto dto, EmailMessageDto emailDto, Dictionary<string, MemoryStream> iamgeFileNames, bool isNewTemplate, int isNeedCopyright, EmailTemplateDto template = null)
        {
            var msg = new MailMessage
            {
                From = !string.IsNullOrEmpty(dto.SenderDisplayName) ?
                        new MailAddress(dto.Sender, dto.SenderDisplayName)
                        : new MailAddress(dto.Sender),
                Subject = emailDto.Subject,
                BodyEncoding = DefaultEncoding,
                SubjectEncoding = DefaultEncoding
            };
            emailDto.EmailTemplate = emailDto.EmailTemplate ==
                                         EmailTemplate.TemplateEmailNotificationTest ?
                                         EmailTemplate.TemplateEmailNotificationHtml :
                                         emailDto.EmailTemplate;
            foreach (string receiver in GetReceivers(emailDto.Receivers))
            {
                msg.To.Add(receiver);
            }
            foreach (string receiver in GetReceivers(emailDto.CcReceivers))
            {
                msg.CC.Add(receiver);
            }
            foreach (string receiver in GetReceivers(emailDto.BccReceivers))
            {
                msg.Bcc.Add(receiver);
            }
            if (emailDto.Attachment != null && !string.IsNullOrEmpty(emailDto.AttachmentName))
            {
                string appType = string.Empty;
                if (emailDto.AttachmentName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    appType = MediaTypeNames.Application.Pdf;
                }
                else
                {
                    appType = MediaTypeNames.Application.Zip;
                }
                Stream attachment = new MemoryStream(emailDto.Attachment);
                attachment.Position = 0;
                msg.Attachments.Add(new Attachment(attachment, emailDto.AttachmentName, appType));
            }
            var body = string.Empty;
            if (isNewTemplate)
            {
                body = ParseNewEmailTemplate(emailDto, isNeedCopyright, template);
            }
            else
            {
                body = ParseEmailTemplate(emailDto, template);
            }
            if (emailDto.ContentType == AvePoint.GCommon.Contract.Server.Common.ContentType.Html)
            {
                //remove records banner from email content in Sep RECO-10404,  also changed in email template
                //AllocateLogoImage(msg, body, GetBannerPath(emailDto.ModuleCategory), template);
                msg.IsBodyHtml = true;
            }
            else
            {
                msg.IsBodyHtml = false;
            }
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
            foreach (var file in iamgeFileNames)
            {
                var fileName = file.Key;
                var imageId = fileName.Remove(fileName.LastIndexOf('.')).Split('_')[0];
                LinkedResource linkedResource = new(file.Value, "image/gif")
                {
                    ContentId = imageId
                };

                alternateView.LinkedResources.Add(linkedResource);
            }
            msg.AlternateViews.Add(alternateView);
            msg.Body = body;
            logger.Debug("Assemble email message completed. Email is sending from {0} to {1}", msg.From.Address, msg.To.ToString());
            return msg;
        }

        public static string ParseNewEmailTemplate(EmailMessageDto dto, int isNeedCopyright,EmailTemplateDto template = null)
        {
            if (dto.DetailMap == null)
            {
                dto.DetailMap = new Dictionary<string, object>();
            }
            try
            {
                var fileName = string.Format("{0}.cshtml", dto.EmailTemplate.ToString());
                var fullPath = Path.Combine(EmailTempatePath, fileName);
                var body = File.ReadAllText(Path.Combine(EmailTempatePath, fileName));
                if (isNeedCopyright == (int)DefaultFooterStatus.UseDefaultFooter)
                {
                    if (template == null || template.IsNeedCopyRight)
                    {
                        body = body.Replace(CopyrightContainer,
                     string.Format("<table style=\"{0}\"><tr><td style=\"padding:3px;display:inline-block;\">{1}</td>{2}<td style=\"padding:3px;display:inline-block;\">{3}</td></tr></table>",
                     "width:100%;border-top:1px solid #d7d7d7;font-size:13px;font-family:segoe ui;color:#858484",
                     string.Format("Enterprise Software Service"),
                     "<td style=\"width:50px;display:inline-block;\"></td>",
                     string.Format("© {0} AvePoint ® Inc. All Rights Reserved.", GetCurrentEndYear())));
                    }
                }
                var templateKey = GetMd5Hash(body);
                logger.Info("body to md5 : {0}", templateKey);
                var builder = new StringBuilder(Engine.Razor.RunCompile(body, templateKey, typeof(EmailMessageDto), dto));
                return UpdateEscapeCharacterAndFontFamilyDependsOnCulture(builder,true).ToString();
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return string.Empty;
            }
        }


        public static string ParseEmailTemplate(EmailMessageDto dto, EmailTemplateDto template = null)
        {
            if (dto.DetailMap == null)
            {
                dto.DetailMap = new Dictionary<string, object>();
            }
            try
            {
                //lock (logger)
                //{
                var fileName = string.Format("{0}.cshtml", dto.EmailTemplate.ToString());
                var fullPath = Path.Combine(EmailTempatePath, fileName);
                var body = File.ReadAllText(Path.Combine(EmailTempatePath, fileName));
                if (dto.EmailTemplate == EmailTemplate.TemplateEmailNotificationHtml)
                {
                    switch (dto.ModuleCategory)
                    {
                        case ModuleCategory.CloudManagement:
                            logger.Info("Change the backgroup color by category.");
                            body = body.Replace("#3076e5", "#0089C5");
                            fileName = "CloudManagement_" + fileName;
                            break;
                        default:
                            break;
                    }
                }
                if (template == null || template.IsNeedCopyRight)
                {
                    body = body.Replace(CopyrightContainer,
                    string.Format("<table style=\"{0}\"><tr><td style=\"padding:3px;display:inline-block;\">{1}</td>{2}<td style=\"padding:3px;display:inline-block;\">{3}</td></tr></table>",
                    "width:100%;border-top:1px solid #d7d7d7;font-size:13px;font-family:segoe ui;color:#858484",
                    string.Format("Enterprise Software Service"),
                    "<td style=\"width:50px;display:inline-block;\"></td>",
                    string.Format("© {0} AvePoint ® Inc. All Rights Reserved.", GetCurrentEndYear())));
                }
                else
                {
                    body = body.Replace(CopyrightContainer, "");
                    fileName = "NoCopyRight_" + fileName;//避免因为自定义邮件修改copyright时，相同filename的不同模板出现报错
                }

                var templateKey = GetMd5Hash(body);
                logger.Info("body to md5 : {0}", templateKey);
                var builder = new StringBuilder(Engine.Razor.RunCompile(body, templateKey, typeof(EmailMessageDto), dto));
                return UpdateEscapeCharacterAndFontFamilyDependsOnCulture(builder, false).ToString();

                //}
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                return string.Empty;
            }
        }
        private static string GetMd5Hash(string input)
        {
            var sb = new StringBuilder();
            using (var md5Crpto = MD5.Create())
            {
                var hash = md5Crpto.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
                foreach (byte t in hash)
                {
                    sb.Append(t.ToString("X2"));
                }
            }
            return sb.ToString();
        }
        private static readonly string CopyrightContainer = "<div id=\"copyright\"></div>";

        private static int GetCurrentEndYear()
        {
            return DateTime.UtcNow.Year;
        }



        private static HashSet<string> GetReceivers(string receivers)
        {
            if (!string.IsNullOrEmpty(receivers))
            {
                string[] strs = receivers.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                HashSet<string> set = new HashSet<string>();
                foreach (string receiver in strs)
                {
                    if (!string.IsNullOrEmpty(receiver.Trim()))
                    {
                        set.Add(receiver.Trim());
                    }
                }
                return set;
            }
            return new HashSet<string>();
        }

        private static readonly string JaCulture = "ja-JP";
        private static readonly string JaFont = "font-family: Meiryo UI";

        private static readonly Dictionary<string, string> JaFontFamilyReplaceMapping = new Dictionary<string, string>
        {
            {"font-family: segoe ui", JaFont},
            {"font-family:segoe ui", JaFont},
            {"font-family: Segoe UI", JaFont},
            {"font-family:Segoe UI", JaFont},
            {"font-family: arial", JaFont},
            {"font-family:arial", JaFont},
            {"font-family: Arial", JaFont},
            {"font-family:Arial", JaFont},
        };

        private static readonly Dictionary<string, string> EscapeCharacterMapping = new Dictionary<string, string>
        {
            {"&amp;", "&"},
            {"&lt;", "<"},
            {"&gt;", ">"},
            {"&nbsp;", " "},
            {"&#39;", "'"},
        };

        private static readonly Dictionary<string, string> NewEscapeCharacterMapping = new Dictionary<string, string>
        {
            {"&amp;", "&"},
            {"&lt;", "<"},
            {"&gt;", ">"},
            {"&#39;", "'"},
            {"&quot;","\""},
        };

        private static StringBuilder UpdateEscapeCharacterAndFontFamilyDependsOnCulture(StringBuilder builder, bool isNewTempalte)
        {
            // razor engine 升级引起的问题，数据源的<和>会被自动转义，为保证尽量少的改动，生成body后将其替换
            if(isNewTempalte) 
            {
                foreach(var item in NewEscapeCharacterMapping)
                {
                    builder.Replace(item.Key, item.Value);
                }

            }
            else
            {
                foreach (var item in EscapeCharacterMapping)
                {
                    builder.Replace(item.Key, item.Value);
                }
            }
            if (string.Equals(I18N.I18NUtility.curCulture, JaCulture, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var item in JaFontFamilyReplaceMapping)
                {
                    builder.Replace(item.Key, item.Value);
                }
            }
            return builder;
        }

        private static string GetBaseDirectory()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (baseDirectory.EndsWith(@"bin\", StringComparison.OrdinalIgnoreCase))
            {
                //if path contains bin, remove it.
                baseDirectory = baseDirectory.Substring(0, baseDirectory.Length - 4);
            }
            else if (baseDirectory.EndsWith(@"bin\Debug\net472\", StringComparison.OrdinalIgnoreCase))
            {
                //if path contains bin, remove it.
                baseDirectory = baseDirectory.Substring(0, baseDirectory.Length - 17);
            }
            return baseDirectory;
        }
    }

    public enum DefaultFooterStatus
    {
        UseDefaultFooter = 0,
        NoUseDefaultFooter,
    }
}