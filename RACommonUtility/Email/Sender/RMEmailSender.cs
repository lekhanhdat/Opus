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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Google.NexusGovernance;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Email.Client;
using AvePoint.RA.RACommonUtility.Email.Model;
using AvePoint.RA.RACommonUtility.Email.Sender.Storage;
using DocumentFormat.OpenXml.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.Google.GControlPlatform;
using GOneGlobal.GlobalDomain;
using EmailTemplateType = Cloud.Sdk.Data.Nexus.Foundation.EmailTemplateType;

namespace AvePoint.RA.RACommonUtility.Email.Sender
{
    public class RMEmailSender
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMEmailSender));

        private readonly IRMEamilTemplateDao _emailTemplateDao = PlatformWindsorManager.GetService<IRMEamilTemplateDao>();
        private readonly IAccountDao _accountDao = PlatformWindsorManager.GetService<IAccountDao>();

        private readonly IRMEmailStorage _storage;

        private readonly RMEmailClient _client = new();
        private readonly IGControlPlatformEmailService _gControlPlatformEmailService = PlatformWindsorManager.GetService<IGControlPlatformEmailService>();
        
        private readonly IGControlUpdateTaskAssignee _gControlUpdateTaskAssignee = PlatformWindsorManager.GetService<IGControlUpdateTaskAssignee>();
        private readonly INexusGovernancePersonalSettingService _nexusGovernancePersonalSettingService = PlatformWindsorManager.GetService<INexusGovernancePersonalSettingService>();

        public RMEmailSender(IRMEmailStorage storage)
        {
            _storage = storage;
        }

        public void Add(Guid templateId, RMEmailTemplateParameters parameters)
        {
            _storage.Add(templateId, parameters);
        }

        public void AddRange(Guid templateId, IEnumerable<RMEmailTemplateParameters> parameters)
        {
            _storage.AddRange(templateId, parameters);
        }
        
        public void AddGControlRange(Guid templateId, IEnumerable<RMEmailTemplateParameters> parameters)
        {
            _storage.AddGControlRange(templateId, parameters);
        }
        
        public void AddGControlTemplate(Guid templateId, RMEmailTemplateParameters parameter)
        {
            _storage.AddGControlTemplate(templateId, parameter);
        }

        public async Task SendAsync()
        {
            try
            {
                s_logger.Debug("Start send email.");

                var templateIds = _storage.GetTemplates();
                s_logger.Info($"Need send email template count: [{templateIds.Count()}]");

                foreach (var templateId in templateIds)
                {
                    await SendAsync(templateId);
                }

                await SendGControlEmailAsync();

                _storage.Empty();

                s_logger.Debug($"Succeed send email.");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while send email. Error: {e}");
            }
        }

        public async Task SendGControlEmailAsync()
        {
            var isSuccessfullyMappingTaskAssignee =
                await _gControlUpdateTaskAssignee.IsSucceedAddedPendingTaskAssignee();

            if (isSuccessfullyMappingTaskAssignee)
            {
                var googleTemplateIds = _storage.GetGControlTemplates();
                
                foreach (var templateId in googleTemplateIds)
                {
                    await GControlEmailSendAsync(templateId);
                }
            }

        }

        private async Task SendAsync(Guid templateId)
        {
            try
            {
                s_logger.Debug($"Start send email by template: [{templateId}].");

                var template = await _emailTemplateDao.GetEmailTemplate(templateId);

                ValidAndUpdateTemplate(template);

                var templateDto = new EmailTemplateDto
                {
                    Id = template.Id,
                    Name = template.DisplayName,
                    Type = (int)template.Type,
                    Subject = template.Subject,
                    CC = template.CC,
                    Body = template.Body,
                    IsNewTemplate = template.IsNewTemplate,
                    IsUseDefaultFooter = (int)template.IsUseDefaultFooter,
                    IsCustomTemplate = template.IsCustomTemplate,
                    UniqueId = template.UniqueId,
                };

                var parametersList = _storage.GetParameters(templateId);
                s_logger.Info($"Template: [{templateId}] need send email count: [{parametersList.Count()}].");

                foreach (var parameters in parametersList)
                {
                   
                    await _client.SendAsync(templateDto, parameters);
                }

                _storage.Remove(templateId);

                s_logger.Debug($"Succeed send email by template: [{templateId}].");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while send email by template: [{templateId}]. Error: {e}");
            }
        }
        
        private async Task GControlEmailSendAsync(Guid templateId)
        {
            try
            {
                s_logger.Debug($"Start google control send email by template: [{templateId}].");
                
                var parameterList = _storage.GetParameters(templateId);
                s_logger.Info($"Template: [{templateId}] need send email count: [{parameterList.Count()}].");

                foreach (var parameter in parameterList)
                {
                    var parameters = parameter as RMManualEmailTemplateParameters;
                    var references = new Dictionary<string, object>()
                    {
                        [nameof(EmailTemplateReferenceType.TaskName)] = "Records review for disposal",
                        [nameof(EmailTemplateReferenceType.TaskCreatedTime)] = $"{DateTime.UtcNow} UTC Time Zone",
                        [nameof(EmailTemplateReferenceType.TaskLink)] = $"<a href=\"{GCommonRoleConfiguration.GCONTROL_MYHUB_TASK_URL ?? ""}\" title=\"Visit MyHub\">Records review for disposal => Review</a>",
                        ["Request"] = new 
                        {
                            Reviewer = parameters.RequestReviewer,
                            Link = GCommonRoleConfiguration.GCONTROL_MYHUB_TASK_URL ?? "",
                            LinkText = "Records review for disposal => Review",
                            Comment = parameters.RequestComment
                        }
                    };
                    s_logger.Info("start get google user id : " + TenantLocalValue.LogonUserId);
                    var user = await _accountDao.GetUserByUserIdAsync(TenantLocalValue.LogonUserId);
                    var locale = await _nexusGovernancePersonalSettingService.GetPersonalSettingLanguage(user.AADId);

                    await _gControlPlatformEmailService.SendEmailAsync(templateId, parameter.ToUser,
                        EmailTemplateType.NewTask, references, locale);
                }

                _storage.Remove(templateId);

                s_logger.Debug($"Succeed send google control email by template: [{templateId}].");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while google control send email by template: [{templateId}]. Error: {e}");
            }
        }

        private void ValidAndUpdateTemplate(RMEmailTemplate template)
        {
            if(template == null)
            {
                return;
            }
            if(template.UniqueId == RMEamilTemplateDao.ExportZipPassword_Template_Id &&
                !RMEamilTemplateDao.ExportZipPasswordEmailBody.Equals(template.Body))
            {
                if(_emailTemplateDao.UpdateEmailTemplate(template.Id, RMEamilTemplateDao.ExportZipPasswordEmailBody))
                {
                    template.Body = RMEamilTemplateDao.ExportZipPasswordEmailBody;
                }
            }
        }
    }
}
