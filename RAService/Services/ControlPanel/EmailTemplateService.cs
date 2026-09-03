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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Email;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.MultiGeo;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.ControlPanel.AuditHandler;
using Azure.Storage.Blobs;
using DocumentFormat.OpenXml.Office2016.Drawing.Charts;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AvePoint.RA.Service.Services.ControlPanel
{
    [Audit]
    public class EmailTemplateService : RMServiceBase, IEmailTemplateService
    {
        private RALogger logger = RALogger.GetInstance(typeof(EmailTemplateService));

        public IRMEamilTemplateDao EmailTemplateDao => PlatformWindsorManager.GetService<IRMEamilTemplateDao>();

        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();

        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private readonly IMultiGeoSettingService MultiGEOSettingService = PlatformWindsorManager.GetService<IMultiGeoSettingService>();

        private static readonly Dictionary<string, string> SpecialCharacterMapping = new Dictionary<string, string>
        {
            {"<", "&#60;"},
            {">", "&#62;"},
            {"&lt;", "&#60;"},
            {"&gt;", "&#62;"}
        };

        private string ReplaceWithHtmlEntityCharacter(string content)
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

        public void InitDefaultData()
        {
            try
            {
                EmailTemplateDao.InitDefaultData();
            }
            catch (Exception e)
            {
                logger.Error("InitDefaultData error {0}", e.ToString());
            }
        }

        public async Task<EmailTemplatesInfo> GetAllTemplateDatas(GetAllEmailTemplateDto getAllTemplateDto)
        {
            List<EmailTemplateDto> emailTemplateLsit = new List<EmailTemplateDto>();

            List<RMEmailTemplate> emailTemplates = EmailTemplateDao.GetAllEmailTemplate().Where(item => !item.IsRemoved).ToList();
            List<RMEmailTemplate> emailTemplatesResult = new List<RMEmailTemplate>();
            bool hasArchiverLicence = LicenseHelperService.HasOpusSOLicense;
            bool hasRecordsLicence = LicenseHelperService.HasOpusILLicense;
            bool hasGoogleLicense = LicenseHelperService.HasOpusGoogleLicense;

            List<RMEmailTemplate> sortedEmailTemplate = new();
            RMEmailTemplate tempExportZipPasswordTemplate = emailTemplates.FirstOrDefault(t => t.UniqueId == RMEmailTemplateId.EXPORT_ZIP_PASSWORD);
            RMEmailTemplate tempDefaultManualApprovalTemplate = emailTemplates.FirstOrDefault(t => t.UniqueId == RMEmailTemplateId.MANUAL_APPROVAL);
            RMEmailTemplate tempDefaultJobNotificationTemplate = emailTemplates.FirstOrDefault(t => t.UniqueId == RMEmailTemplateId.JOB_NOTIFICATION);
            if (tempExportZipPasswordTemplate != null)
            {
                foreach (var temp in emailTemplates)
                {
                    if (temp.UniqueId != RMEmailTemplateId.EXPORT_ZIP_PASSWORD && temp.UniqueId != RMEmailTemplateId.MANUAL_APPROVAL && temp.UniqueId != RMEmailTemplateId.JOB_NOTIFICATION && temp.UniqueId != RMEmailTemplateId.HOLD_NOTIFICATION)
                    {
                        sortedEmailTemplate.Add(temp);
                    }

                    if (temp.UniqueId == RMEmailTemplateId.ML_MANUAL_APPROVAL)
                    {
                        sortedEmailTemplate.Add(tempExportZipPasswordTemplate);
                        sortedEmailTemplate.Add(tempDefaultManualApprovalTemplate);
                        sortedEmailTemplate.Add(tempDefaultJobNotificationTemplate);
                    }
                }
            }

            foreach (var temp in sortedEmailTemplate)
            {
				if (!await LicenseHelperService.IsEnableMaestroAI() && !KeyValueDao.EnableZeroShotFeature() && temp.Type == EmailTemplateType.MLRecordsForReview)
				{
					continue;
				}
				if (temp.Type == EmailTemplateType.ExportZipPasswordForReview || temp.Type == EmailTemplateType.JobNotification)
                {
                    if (hasArchiverLicence)
                    {
                        emailTemplatesResult.Add(temp);
                    }
                }
                else
                {
                    if (hasRecordsLicence || hasGoogleLicense)
                    {
                        emailTemplatesResult.Add(temp);
                    }
                }
            }
            List<RMEmailTemplate> currentPagerEmailTemplates = emailTemplatesResult.Skip(getAllTemplateDto.PagerIndex * getAllTemplateDto.PagerSize).Take(getAllTemplateDto.PagerSize).ToList();
            foreach (RMEmailTemplate template in currentPagerEmailTemplates)
            {
                if (template.DisplayName == "RM_CP_Email_ManualApproval")
                {
                    template.DisplayName = "RM_CP_Email_ManualApprovalForRecordsReviewer";
                }
                EmailTemplateDto dto = new EmailTemplateDto();
                dto.Id = template.Id;
                dto.Name = I18NEntity.GetString(template.DisplayName);
                dto.Type = (int)template.Type;
                dto.Subject = template.Subject;
                dto.CC = template.CC;
                dto.Body = template.Body;
                dto.IsCustomTemplate = template.IsCustomTemplate;
                dto.UniqueId = template.UniqueId;
                emailTemplateLsit.Add(dto);
            }

            EmailTemplatesInfo results = new EmailTemplatesInfo();
            results.PagerInfo = new EmailTemplatePagerInfo
            {
                TotalCount = emailTemplatesResult.Count(),
                PagerIndex = getAllTemplateDto.PagerIndex,
                PagerSize = getAllTemplateDto.PagerSize
            };
            results.Items = emailTemplateLsit;
            return results;
        }
        
		public List<EmailTemplateDto> GetAllCustomEmailTemplates()
        {
			List<EmailTemplateDto> templateLsit = new();

			List<RMEmailTemplate> customEmailTemplates = EmailTemplateDao.GetAllEmailTemplate().Where(item => !item.IsRemoved && item.IsCustomTemplate).ToList();

			foreach (RMEmailTemplate template in customEmailTemplates)
			{
				EmailTemplateDto dto = new()
				{
					Id = template.Id,
					Name = I18NEntity.GetString(template.DisplayName),
					Type = (int)template.Type,
					Subject = template.Subject,
					CC = template.CC,
					Body = template.Body,
					UniqueId = template.UniqueId,
					IsCustomTemplate = template.IsCustomTemplate
				};
				templateLsit.Add(dto);
			}
            return templateLsit;
		}

		public EmailTemplateDto GetCustomDefaultEmailTemplate(EmailTemplateInternalType type)
        {
			return EmailTemplateDao.GetCustomDefaultEmailTemplate(type);
        }

		public EmailTemplateDto GetEmailTemplateById(int id)
        {
            EmailTemplateDto result = new();
            List<EmailImageDto> ImageList = new();
            RMEmailTemplate template = EmailTemplateDao.GetEmailTemplateById(id);
            string body = template.Body;
            string forGetImageTemplateId = template.IsCustomTemplate ? template.UniqueId.ToString() : id.ToString();
			if (template.IsNewTemplate)
            {
                var allImages = RAStorageUtil.AllBlobNames(TenantLocalValue.LogonGroupId + @"/" + forGetImageTemplateId);
                foreach (var image in allImages)
                {
                    var fileName = image.Replace(TenantLocalValue.LogonGroupId + @"/" + forGetImageTemplateId + @"/", "");
                    var iamgeId = fileName.Remove(fileName.LastIndexOf('.')).Split('_')[0];
                    var fileType = fileName.Remove(fileName.LastIndexOf('.')).Split('_')[1];
                    if (!string.IsNullOrEmpty(body) && body.Contains(iamgeId))
                    {
                        ImageList.Add(new EmailImageDto() { ImageId = iamgeId, Base64 = RAStorageUtil.DownloadImageBlobToText(image), FileType = fileType });
                    }
                }
            }
            else
            {
                body = ReplaceWithHtmlEntityCharacter(body);
            }
            if (template.DisplayName == "RM_CP_Email_ManualApproval")
            {
                template.DisplayName = "RM_CP_Email_ManualApprovalForRecordsReviewer";
            }
            result.Id = template.Id;
            result.Name = I18NEntity.GetString(template.DisplayName);
            result.Type = (int)template.Type;
            result.Subject = I18NEntity.GetString(template.Subject);
            result.CC = template.CC;
            result.Body = body;
            result.IsUseDefaultFooter = (int)template.IsUseDefaultFooter;
            result.IsNewTemplate = template.IsNewTemplate;
            result.ImageList = ImageList;
			result.IsCustomTemplate = template.IsCustomTemplate;
            result.UniqueId = template.UniqueId;
			return result;
        }

        public EmailTemplateDto GetEmailTemplateByInternalType(EmailTemplateInternalType type)
        {
            EmailTemplateDto result = new EmailTemplateDto();
            RMEmailTemplate template = EmailTemplateDao.GetEmailTemplateByInternalType(type);
            if (template != null)
            {
                result.Id = template.Id;
                result.Name = I18NEntity.GetString(template.DisplayName);
                result.Type = (int)template.Type;
                result.Subject = template.Subject;
                result.CC = template.CC;
                result.Body = template.Body;
                result.IsNewTemplate = template.IsNewTemplate;
                result.IsUseDefaultFooter = (int)template.IsUseDefaultFooter;
                result.UniqueId = template.UniqueId;
            }
            return result;
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.EmailTemplateManagement, Action = AuditAction.EditEmailTempalte, BeforeHandler = typeof(EmailTemplateBeforeAuditHandler), AfterHandler = typeof(EmailTemplateAfterAuditHandler))]
        public string UpdateEmailTemplate(EmailTemplateDto email)
        {
			bool isTemplateNameExist = EmailTemplateDao.CheckTemplateNameExist(email);
			bool isTemplateBodyTooLong = email.Body.Length > 50000;
			if (isTemplateBodyTooLong)
			{
				return I18NEntity.GetString("RM_CP_EamilTemplate_LimitSize");
			}
			if (isTemplateNameExist)
			{
				return I18NEntity.GetString("RM_EmailTemplate_VerifySameName");
			}
			if (!EmailTemplateDao.UpdateEmailTemplate(email.Name, email.Id, email.Subject, email.CC, email.Body, email.IsUseDefaultFooter))
            {
				return I18NEntity.GetString("RM_EmailTemplate_CommonActionFailed");
			}
            return "";
        }

		[Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.EmailTemplateManagement, Action = AuditAction.CreateEmailTemplate, BeforeHandler = typeof(EmailTemplateBeforeAuditHandler), AfterHandler = typeof(EmailTemplateAfterAuditHandler))]
		public string CreateEmailTemplate(EmailTemplateDto email)
		{
            bool isTemplateNameExist = EmailTemplateDao.CheckTemplateNameExist(email);
			bool isTemplateBodyTooLong = email.Body.Length > 50000;
            bool isTemplateNameTooLong = email.Name.Length > 255;
            bool isTemplateSubjectTooLong = email.Subject.Length > 255;
            bool isTemplateCcTooLong = email.CC.Length > 255;
            if (isTemplateNameTooLong || isTemplateSubjectTooLong || isTemplateCcTooLong)
            {
                return I18NEntity.GetString("RM_JS_Common_Msg_CannotExceed255");
            }
			if (isTemplateBodyTooLong)
			{
				return I18NEntity.GetString("RM_CP_EamilTemplate_LimitSize");
			}
			if (isTemplateNameExist)
            {
				return I18NEntity.GetString("RM_EmailTemplate_VerifySameName");
			}
            if (!EmailTemplateDao.CreateEmailTemplate(email.UniqueId, email.Name, email.Subject, email.CC, email.Body, email.IsUseDefaultFooter))
            {
				return I18NEntity.GetString("RM_EmailTemplate_CommonActionFailed");
			}
            if (email.CopySourceId != null && email.CopySourceId != 0)
            {
                string forGetImageTemplateId = email.UniqueId.ToString();
                RMEmailTemplate sourceTemplate = EmailTemplateDao.GetEmailTemplateById(email.CopySourceId.Value);
                string body = sourceTemplate.Body;
                string sourceForGetImageTemplateId = sourceTemplate.IsCustomTemplate ? sourceTemplate.UniqueId.ToString() : sourceTemplate.Id.ToString();
                if (sourceTemplate.IsNewTemplate)
                {
                    var allImages = RAStorageUtil.AllBlobNames(TenantLocalValue.LogonGroupId + @"/" + sourceForGetImageTemplateId);
                    foreach (var image in allImages)
                    {
                        RAStorageUtil.UploadImage(image.Replace(TenantLocalValue.LogonGroupId + @"/" + sourceForGetImageTemplateId + @"/", TenantLocalValue.LogonGroupId + @"/" + forGetImageTemplateId + @"/"), RAStorageUtil.DownloadImageBlobToText(image));
                    }
                }
            }
            return "";
		}

		[Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.EmailTemplateManagement, Action = AuditAction.DeleteEmailTemplate, BeforeHandler = typeof(EmailTemplateBeforeAuditHandler), AfterHandler = typeof(EmailTemplateAfterAuditHandler))]
		public string DeleteEmailTemplate(Guid uniqueId)
		{
            if (EmailTemplateDao.CheckTemplateUsed(uniqueId))
            {
				return I18NEntity.GetString("RM_EmailTemplate_VerifyUsed");
			}

            if (!EmailTemplateDao.DeleteEmailTemplate(uniqueId))
            {
				return I18NEntity.GetString("RM_EmailTemplate_CommonActionFailed");
			}
            return "";
		}

		public async Task<EmailImageDto> UploadImage(Stream imageStream,string templateId,string fileType)
        {
            var imageId = Guid.NewGuid().ToString();
            using var ms = new MemoryStream();
            var buffer = new byte[1024];
            int read;
            while ((read = imageStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            var base64 = Convert.ToBase64String(ms.ToArray());
            var customId = TenantLocalValue.LogonGroupId;
            var blobFolder = SecurityUtils.SafeCombinePath(customId, templateId.ToString());
            var blobName = SecurityUtils.SafeCombinePath(blobFolder, imageId + '_' + fileType + ".txt");
            RAStorageUtil.UploadImage(blobName, base64);
            var emailImageDto = new EmailImageDto() { ImageId = imageId, Base64 = base64, FileType = fileType, EmailTemplateId = templateId};
            if (MultiGEOSettingService.IsEnableMultiGeoFeature().GetAwaiter().GetResult())
            {
                await RAMultiGeoClient.ReplicateToOtherDataCentersAsync(
                       emailImageDto,
                       MultiGeoOperationType.UploadImages);
            }
            return emailImageDto;
        }
        public void UploadImageToOtherDC(EmailImageDto imageDto)
        {
            var customId = TenantLocalValue.LogonGroupId;
            var blobFolder = SecurityUtils.SafeCombinePath(customId, imageDto.EmailTemplateId);
            var blobName = SecurityUtils.SafeCombinePath(blobFolder, imageDto.ImageId + '_' + imageDto.FileType + ".txt");
            RAStorageUtil.UploadImage(blobName, imageDto.Base64);
        }

    }
}
