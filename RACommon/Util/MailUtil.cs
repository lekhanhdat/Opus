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

using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.EmailTemplateSettings.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Reporting.UserNotificationSettings.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Notification;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Common.Configurations;
using AvePoint.Wrapper.Common;
using ArgumentCheck = AvePoint.Wrapper.Common.ArgumentCheck;
using AvePoint.RA.Common.Security;
using System.IO;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Common.Extension;
using AvePoint.RA.Common.GraphApi.Mail;
using AvePoint.RA.Common.Aos;
using AvePoint.GCommon.Contract.ContentManager.Object;
using System.Text.RegularExpressions;
using System.Web.Services.Description;

namespace AvePoint.RA.Common.Util
{
    public static class MailUtil
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(MailUtil));
        private readonly static object sendEmailTemplateLocker = new object();

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        public static readonly string patternCurrentDate = "\\$Current\\.Date(\\+[1-9][0-9]*)?\\$";

        public static readonly string patternCurrentDateNumber = "[0-9]+";

        public static string GenerateSettingBase64String()
        {
            NotificationSettingDto dto = InitNotificationSetting();
            var str = SerializerHelper.SerializeToXmlString(dto);
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(buffer);
        }
        public static NotificationSettingDto GetNotificationSetting()
        {
            try
            {
                NotificationSettingDto dto = null;
                if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
                {
                    string setting = RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.NOTIFICATION_SETTING];
                    dto = SerializerHelper.DeserializeFromXmlString<NotificationSettingDto>(setting);
                    return dto;
                }
                else 
                {
                    var settingPassword = RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.SENDGRID_KEY];
                    var settingPort = int.TryParse(RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.SENDGRID_PORT], out int port) ? port : 587;
                    var settingSender = RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.SENDGRID_SENDER_ADDRESS];
                    var settingUserName = RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.SENDGRID_ACCOUNT];
                    var settingOugoingMailServer = RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.SENDGRID_SERVER];
                    var settingSenderDisplayName = RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.SENDGRID_SENDER_DISPLAYNAME];
                    logger.Info($"Current setting sender display name: {settingSenderDisplayName}");
                    if (string.IsNullOrWhiteSpace(settingUserName) || string.IsNullOrWhiteSpace(settingPassword) || string.IsNullOrWhiteSpace(settingSender) || string.IsNullOrWhiteSpace(settingOugoingMailServer))
                    {
                        logger.Info("MailUtil  Read NOTIFICATION_SETTING");
                        string setting = RMGlobalConfiguration.EncryptConfig[Contract.Configurations.RMCommonSettingKey.NOTIFICATION_SETTING];
                        dto = SerializerHelper.DeserializeFromXmlString<NotificationSettingDto>(setting);
                        return dto;
                    }
                    else
                    {
                        logger.Info("MailUtil  Read Others");
                        dto = new NotificationSettingDto
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
                        return dto;
                    }
                }
                
            }
            catch (Exception ex)
            {
                logger.Error("Please confirm the Notification Setting Info in the config file, Error Message:{0}", ex.ToString());
                throw;
            }
        }
       

        public static void SendEmailTemplate(Contract.RMWeb.CP.EmailTemplateDto templateDto, ParameterDto Para, List<ToUserInfo> users, EmailSenderDefinition emailSenderDefinition)
        {
            if (emailSenderDefinition.EmailSenderType == EmailSenderType.Default)
            {
                SendEmailTemplate(templateDto, Para, users);
                return;
            }

            try
            {
                EmailMessageDto emailDto = new EmailMessageDto();
                emailDto.Subject = GetSubjectAndBodyString(templateDto.Subject, Para, templateDto.IsNewTemplate);
                emailDto.Receivers = GetUserNameList(users);
                emailDto.CcReceivers = templateDto.CC;
                emailDto.Body = GetSubjectAndBodyString(templateDto.Body, Para, templateDto.IsNewTemplate, true);
                emailDto.EmailTemplate = EmailTemplate.TemplateEmailNotificationHtml;
                emailDto.Attachment = null;
                emailDto.ContentType = ContentType.Html;
                emailDto.ModuleCategory = ModuleCategory.Classic;

                var body = "";
                if (templateDto.IsNewTemplate)
                {
                    body = NotificationUtil.ParseNewEmailTemplate(emailDto, (int)templateDto.IsUseDefaultFooter, null);
                }
                else
                {
                    body = NotificationUtil.ParseEmailTemplate(emailDto, null);
                }

                var subject = emailDto.Subject;
                var images = GetAllImageFileByO365Send(templateDto.Id, body);
                foreach (var entity in images)
                {
                    var id = entity.Key.Remove(entity.Key.LastIndexOf('.')).Split('_')[0];
                    body = body.Replace($"cid:{id}", $"data:image/jpeg;base64,{entity.Value}");
                }

                var appProfile = RMAosApiClient.GetProfileById(emailSenderDefinition.AppProfileId).GetAwaiter().GetResult();

                var mailDto = new RMGraphMailMessageDefinition(
                    emailSenderDefinition.EmailSender.UserPrincipalName,
                    emailDto.Receivers,
                    templateDto.CC,
                    subject,
                    body);

                var mailManager = new RMGraphMailManager(appProfile);
                mailManager.SendEmail(mailDto).GetAwaiter().GetResult();

                logger.Info("Success Send Email.");
            }
            catch(Exception e)
            {
                logger.Error("Failed to Send Email, Error Message:{0}", e.ToString());
            }
        }

        public static void SendEmailTemplate(Contract.RMWeb.CP.EmailTemplateDto templateDto, ParameterDto Para ,List<ToUserInfo> users )
        {
            try
            {
                lock (sendEmailTemplateLocker)
                {
                    NotificationSettingDto dto = GetNotificationSetting();
                    if (dto != null && templateDto != null && templateDto.Id != 0)
                    {
                        EmailMessageDto emailDto = new EmailMessageDto();
                        emailDto.Subject = GetSubjectAndBodyString(templateDto.Subject, Para,templateDto.IsNewTemplate);
                        emailDto.Receivers = GetUserNameList(users);
                        emailDto.CcReceivers = templateDto.CC;
                        emailDto.Body = GetSubjectAndBodyString(templateDto.Body, Para, templateDto.IsNewTemplate, true);
                        emailDto.EmailTemplate = EmailTemplate.TemplateEmailNotificationHtml;
                        emailDto.Attachment = null;
                        emailDto.ContentType = ContentType.Html;
                        emailDto.ModuleCategory = ModuleCategory.Classic;
                        using (new PerformanceScope("send email."))
                        {
                            var allImageFiles = GetAllImageFile(templateDto.Id, emailDto.Body);
                            NotificationUtil.SendSyncEmail(dto, emailDto, allImageFiles, templateDto.IsNewTemplate, (int)templateDto.IsUseDefaultFooter);
                            //NotificationUtil.LocalTestSendSyncEmail(emailDto, allImageFiles, templateDto.IsNewTemplate, (int)templateDto.IsUseDefaultFooter);
                        }

                        logger.Info("Success Send Email.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to Send Email, Error Message:{0}", ex.ToString());
                //throw;
            }
        }

        public static void SendEmailTemplate(Contract.RMWeb.CP.EmailTemplateDto templateDto, ManualParameterDto para, EmailSenderDefinition emailSenderDefinition)
        {
            try
            {
                if(emailSenderDefinition.EmailSenderType == EmailSenderType.Default)
                {
                    SendEmailTemplate(templateDto, para);
                    return;
                }

                if (templateDto == null || templateDto.Id == 0)
                {
                    return;
                }

                Func<RequestLinkDto> getRequestLinkInfoAction = GetRequestLinkInfoAction(templateDto.Type);

                EmailMessageDto emailDto = new EmailMessageDto
                {
                    Subject = ReplaceManualSubjectOrBodyString(templateDto.Subject, para, getRequestLinkInfoAction, templateDto.IsNewTemplate),
                    Receivers = para.ReviewerEmail,
                    CcReceivers = templateDto.CC,
                    Body = ReplaceManualSubjectOrBodyString(templateDto.Body, para, getRequestLinkInfoAction, templateDto.IsNewTemplate, true),
                    EmailTemplate = EmailTemplate.TemplateEmailNotificationHtml,
                    Attachment = null,
                    ContentType = ContentType.Html,
                    ModuleCategory = ModuleCategory.Classic
           };

                var body = "";
                if (templateDto.IsNewTemplate)
                {
                    body = NotificationUtil.ParseNewEmailTemplate(emailDto, (int)templateDto.IsUseDefaultFooter, null);
                }
                else
                {
                    body = NotificationUtil.ParseEmailTemplate(emailDto, null);
                }

                var subject = emailDto.Subject;
                var images = GetAllImageFileByO365Send(templateDto.Id, body);
                foreach (var entity in images)
                {
                    var id = entity.Key.Remove(entity.Key.LastIndexOf('.')).Split('_')[0];
                    body = body.Replace($"cid:{id}", $"data:image/jpeg;base64,{entity.Value}");
                }

                var appProfile = RMAosApiClient.GetProfileById(emailSenderDefinition.AppProfileId).GetAwaiter().GetResult();

                var mailDto = new RMGraphMailMessageDefinition(
                    emailSenderDefinition.EmailSender.UserPrincipalName,
                    para.ReviewerEmail,
                    templateDto.CC,
                    subject,
                    body);

                var mailManager = new RMGraphMailManager(appProfile);
                mailManager.SendEmail(mailDto).GetAwaiter().GetResult();

                logger.Info("Success Send Email.");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred send manual email. Error: {e}");
            }
        }

        public static void SendEmailTemplate(Contract.RMWeb.CP.EmailTemplateDto templateDto, ManualParameterDto para)
        {
            try
            {
                if (templateDto == null || templateDto.Id == 0)
                {
                    return;
                }
                var setting = GetNotificationSetting();
                if (setting == null)
                {
                    logger.Warn($"Can't get email noticiation settings.");
                    return;
                }
                Func<RequestLinkDto> getRequestLinkInfoAction = GetRequestLinkInfoAction(templateDto.Type);
                EmailMessageDto emailDto = new EmailMessageDto
                {
                    Subject = ReplaceManualSubjectOrBodyString(templateDto.Subject, para, getRequestLinkInfoAction, templateDto.IsNewTemplate),
                    Receivers = para.ReviewerEmail,
                    CcReceivers = templateDto.CC,
                    Body = ReplaceManualSubjectOrBodyString(templateDto.Body, para, getRequestLinkInfoAction, templateDto.IsNewTemplate, true),
                    EmailTemplate = EmailTemplate.TemplateEmailNotificationHtml,
                    Attachment = null,
                    ContentType = ContentType.Html,
                    ModuleCategory = ModuleCategory.Classic
                   };
                var allImageFiles = GetAllImageFile(templateDto.Id, emailDto.Body);
                using (new PerformanceScope("send email."))
                {
                    NotificationUtil.SendSyncEmail(setting, emailDto, allImageFiles, templateDto.IsNewTemplate, (int)templateDto.IsUseDefaultFooter);
                    //NotificationUtil.LocalTestSendSyncEmail(emailDto, allImageFiles, templateDto.IsNewTemplate, templateDto.IsUseDefaultFooter);
                }

                logger.Info("Success Send Email.");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred send manual email. Error: {e}");
            }
        }

        private static Dictionary<string, MemoryStream> GetAllImageFile(int templateId, string body)
        {
            var imageFileNames = RAStorageUtil.AllBlobNames(TenantLocalValue.LogonGroupId + @"/" + templateId.ToString());
            var existFileNames = imageFileNames.ConvertAll(name => name.Replace(TenantLocalValue.LogonGroupId + @"/" + templateId.ToString() + @"/", ""));
            var imageIds = existFileNames.ConvertAll(name => name.Remove(name.LastIndexOf('.')).Split('_')[0]).Where(id => body.Contains(id));
            var realExistFiles = existFileNames.Where(f => imageIds.Any(id => f.Contains(id)));
            var base64Streams = new Dictionary<string, MemoryStream>();
            foreach (var file in realExistFiles)
            {
                var base64String = RAStorageUtil.DownloadImageBlobToText(TenantLocalValue.LogonGroupId + @"/" + templateId.ToString() + @"/" + file);
                var imageBytes = Convert.FromBase64String(base64String);
                var ms1 = new MemoryStream(imageBytes, 0, imageBytes.Length);
                base64Streams[file] = ms1;
            }
            return base64Streams;
        }

        private static Dictionary<string, string> GetAllImageFileByO365Send(int templateId, string body)
        {
            var res = new Dictionary<string, string>();

            var imageFileNames = RAStorageUtil.AllBlobNames(TenantLocalValue.LogonGroupId + @"/" + templateId.ToString());
            var existFileNames = imageFileNames.ConvertAll(name => name.Replace(TenantLocalValue.LogonGroupId + @"/" + templateId.ToString() + @"/", ""));
            var imageIds = existFileNames.ConvertAll(name => name.Remove(name.LastIndexOf('.')).Split('_')[0]).Where(id => body.Contains(id));
            var realExistFiles = existFileNames.Where(f => imageIds.Any(id => f.Contains(id)));

            foreach (var file in realExistFiles)
            {
                var base64String = RAStorageUtil.DownloadImageBlobToText(TenantLocalValue.LogonGroupId + @"/" + templateId.ToString() + @"/" + file);
                res[file] = base64String;
            }

            return res;
        }

        private static string ReplaceManualSubjectOrBodyString(string template, ManualParameterDto para, Func<RequestLinkDto> getRequestLinkAction, bool isNewTemplate, bool isBody = false)
        {
            var requestLinkDto = getRequestLinkAction();
            template = template.Replace("$Request.Comment$", para.Comment);
            template = template.Replace("$Request.Reviewer$", para.Reviewer);
            template = template.Replace("$Current.Date$", para.CurrentDate);

            var matches = Regex.Matches(template, patternCurrentDate);
            if (matches.Count > 0)
            {
                foreach (Match match in matches.Cast<Match>())
                {
                    var stringDateNumber = Regex.Match(match.Value, patternCurrentDateNumber);
                    if (stringDateNumber.Success)
                    {
                        var intDateNumber = int.Parse(stringDateNumber.Value);
                        var addDateValue = GeneralSettingService.ConvertTiksToDateNoTime(GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult(), DateTimeOffset.Now.AddDays(intDateNumber).Ticks);
                        template = template.Replace(match.Value, addDateValue);
                    }
                }
            }
            
            template = template.Replace("$Request.Reviewer.FirstName$",para.RequestReviewerFirstName);
            if (isBody)
            {
                template = isNewTemplate ? NewReplaceWithHtmlEntityCharacter(template) : ReplaceWithHtmlEntityCharacter(template);
                template = template.Replace("$Request.Link$", $"<div class='manualLink'><a href='{requestLinkDto?.Url}' title='{requestLinkDto?.Url}'>" +
                    $"{I18NEntity.GetString("RM_JS_Common_RecourdAutomation")}>{requestLinkDto?.Title}" +
                    $"</a></div>");
                if (isNewTemplate)
                {
                    template = $"<div>{template}</div>";
            }
            }
            if (isBody && (!isNewTemplate || template.Contains('\n')))
            {
                template = template.Replace("\n", "<br/>");
                return $"<div>{template}</div>";  //class='eContent'
            }
            return template;
        }

        private static Func<RequestLinkDto> GetRequestLinkInfoAction(int emailTemplateType)
        {
            Func<RequestLinkDto> getRequestLinkAction = null;
            if (emailTemplateType == (int)EmailTemplateType.RecordsForReview)
            {
                getRequestLinkAction = GetManualRequestLinkInfo;
            }
            if (emailTemplateType == (int)EmailTemplateType.MLRecordsForReview)
            {
                getRequestLinkAction = GetMLManualRequestLinkInfo;
            }
            return getRequestLinkAction;
        }

        private static RequestLinkDto GetManualRequestLinkInfo()
        {
            return new RequestLinkDto
            {
                Url = AveUrlUtility.CombineUrl(RMSSOHelper.RecoSsoLoginUrl, "?redirect=Root/RDM/ManualApprovalReview"),
                Title = I18NEntity.GetString("RM_DAM_ManualApprovalReview")
            };
        }

        private static RequestLinkDto GetMLManualRequestLinkInfo()
        {
            return new RequestLinkDto
            {
                Url = AveUrlUtility.CombineUrl(RMSSOHelper.RecoSsoLoginUrl, "?redirect=Root/MT/MachineLearningReview"),
                Title = I18NEntity.GetString("RM_MT_MachineLearningReview")
            };
        }

        public static string GetUserNameList(List<ToUserInfo> users)
        {
            string receiversName = string.Empty;
            foreach (ToUserInfo user in users)
            {
                receiversName = receiversName + user.UserPrincipalName + ";";
            }
            return receiversName;
        }

        private static readonly Dictionary<string, string> SpecialCharacterMapping = new Dictionary<string, string>
        {
            {"<", "&#60;"},
            {">", "&#62;"},
            {"&lt", "&#60;"},
            {"&gt", "&#62;"}
        };

        private static readonly Dictionary<string, string> NewSpecialCharacterMapping = new Dictionary<string, string>
        {
            {"&lt;", "&#60;"},
            {"&gt;", "&#62;"}
        };

        private static string NewReplaceWithHtmlEntityCharacter(string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                foreach (var item in NewSpecialCharacterMapping)
                {
                    content = content.Replace(item.Key, item.Value);
                }
            }
            return content;
        }


        private static string ReplaceWithHtmlEntityCharacter(string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                foreach (var item in SpecialCharacterMapping)
                {
                    content = content.Replace(item.Key, item.Value);
                }
            }
            return content;
        }

        public static string GetSubjectAndBodyString(string body, ParameterDto Para, bool isNewTemplate,bool isBody = false)
        {
            if (body != string.Empty && body != "")
            {
                if (body.Contains("$Request.ID$"))
                {
                    body = body.Replace("$Request.ID$", Para.RequestID);
                }
                if (body.Contains("$Request.Comment$"))
                {
                    body = body.Replace("$Request.Comment$", Para.RequsetComment);
                }
                if (body.Contains("$Request.Requester$"))
                {
                    body = body.Replace("$Request.Requester$", Para.Requester);
                }
                if (body.Contains("$Request.Assignee$"))
                {
                    body = body.Replace("$Request.Assignee$", Para.Assignee);
                }
                if (body.Contains("$PhysicalRecords.Name$"))
                {
                    body = body.Replace("$PhysicalRecords.Name$", Para.PhscicalRecordName);
                }
                if (body.Contains("$PhysicalRecords.UID$"))
                {
                    body = body.Replace("$PhysicalRecords.UID$", Para.PhscicalRecordUID);
                }
                if (body.Contains("$Request.JobId$"))
                {
                    body = body.Replace("$Request.JobId$", Para.RestoreJobid);
                }
                if (body.Contains("$Request.Location$"))
                {
                    body = body.Replace("$Request.Location$", Para.ExportLocation);
                }
                if (body.Contains("$Request.Password$"))
                {
                    body = body.Replace("$Request.Password$", Para.ZipPassword);
                }
                if (body.Contains("$Request.Reviewer$"))
                {
                    body = body.Replace("$Request.Reviewer$", Para.Reviewer);
                }
                if (body.Contains("$Request.Requester.FirstName$"))
                {
                    body = body.Replace("$Request.Requester.FirstName$",Para.RequestRequesterFirstname);
                }
                if (body.Contains("$Request.Successful.Count$"))
                {
                    body = body.Replace("$Request.Successful.Count$", Para.MoveInfo?.SuccessfullCount.ToString());
                }
                if (body.Contains("$Request.Failed.Count$"))
                {
                    body = body.Replace("$Request.Failed.Count$", Para.MoveInfo?.FailedCount.ToString());
                }
                if (body.Contains("$Request.SourceLocation$"))
                {
                    body = body.Replace("Request.SourceLocation$", Para.MoveInfo?.OriginalLocation);
                }
                if (body.Contains("$Request.Destination$"))
                {
                    body = body.Replace("$Request.Destination$", Para.MoveInfo?.DestinationLocation);
                }
                if (body.Contains("$Destination.RecordsManager$"))
                {
                    body = body.Replace("$Destination.RecordsManager$", Para.MoveInfo?.DestinationRM);
                }
                //body = ReplaceWithHtmlEntityCharacter(body);
            }
            if (isBody)
            {
                if (body.Contains("AvePoint Cloud Records > Request Management") || body.Contains("AvePoint Cloud Records &gt; Request Management"))
                {
                    body = isNewTemplate ? NewReplaceWithHtmlEntityCharacter(body) : ReplaceWithHtmlEntityCharacter(body);
                    var url = AveUrlUtility.CombineUrl(RMSSOHelper.RecoSsoLoginUrl, "?redirect=Root/PRM/MyRequest");
                    string link = string.Format("<div class='requestLink'><a href='{0}'>{1}</a></div>", url, "AvePoint Cloud Records > Request Management");
                    body = body.Replace("AvePoint Cloud Records &#62; Request Management", link);
                }
                else if (body.Contains("AvePoint Cloud Records > My Tasks > Requests for Review") || body.Contains("AvePoint Cloud Records &gt; My Tasks &gt; Requests for Review"))
                {
                    body = isNewTemplate ? NewReplaceWithHtmlEntityCharacter(body) : ReplaceWithHtmlEntityCharacter(body);
                    var url = AveUrlUtility.CombineUrl(RMSSOHelper.RecoSsoLoginUrl, "?redirect=Root/PRM/MyRequest");
                    string link = string.Format("<div class='requestLink'><a href='{0}' class='requestLink'>{1}</a></div>", url, "AvePoint Cloud Records > My Tasks > Requests for Review");
                    body = body.Replace("AvePoint Cloud Records &#62; My Tasks &#62; Requests for Review", link);
                }
                else if (body.Contains("AvePoint Opus > My Tasks > Requests for Review") || body.Contains("AvePoint Opus &gt; My Tasks &gt; Requests for Review"))
                {
                    body = isNewTemplate ? NewReplaceWithHtmlEntityCharacter(body) : ReplaceWithHtmlEntityCharacter(body);
                    var url = AveUrlUtility.CombineUrl(RMSSOHelper.RecoSsoLoginUrl, "?redirect=Root/PRM/MyRequest");
                    string link = string.Format("<div class='requestLink'><a href='{0}' class='requestLink'>{1}</a></div>", url, "AvePoint Opus > My Tasks > Requests for Review");
                    body = body.Replace("AvePoint Opus &#62; My Tasks &#62; Requests for Review", link);
                }
                else
                {
                    body = isNewTemplate ? NewReplaceWithHtmlEntityCharacter(body) : ReplaceWithHtmlEntityCharacter(body);
                }
                if (body.Contains("\n"))
                {
                    body = body.Replace("\n", "<br/>");
                }
                return string.Format("<div>{0}</div>", body);  // class='eContent'
            }
            else
            {
                return body;
            }
        }


        public static object GetDetail(EmailMessageDto emailDto, DetailKey dk)
        {
            object detail = null;
            foreach (KeyValuePair<string, object> kv in emailDto.DetailMap)
            {
                if (kv.Key == dk.ToString())
                {
                    detail = kv.Value;
                }
            }
            return detail;
        }
        public static NotificationSettingDto InitNotificationSetting()
        {
            NotificationSettingDto dto = new NotificationSettingDto();
            //dto.Id = "1";
            //dto.Name = "SendMail";
            //dto.Useable = true;
            dto.OutgoingMailServer = "smtp.163.com";
            dto.Sender = "lnwoo@163.com";
            dto.ExchangeServer = "smtp.163.com";
            dto.UserName = "lnwoo";
            dto.Password = "43566298";
            dto.Port = 465;
            dto.SslAuthentication = true;
            dto.SecurePasswordAuthentication = false;
            return dto;
        }

    }
    public enum DetailKey
    {
        From = 0,
        To = 1,
        Comment = 2
    }
}
